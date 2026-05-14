using NLog;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TombLib.Utils;
using Format = Silk.NET.DXGI.Format;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using D3D11Usage = Silk.NET.Direct3D11.Usage;

namespace TombLib.Rendering.DirectX11
{
    // Direct3D 11 implementation of RenderingDevice using Silk.NET bindings.
    //
    // Lifecycle: a single instance is created at startup (DeviceManager.DefaultDeviceManager)
    // and shared across every RenderingPanel in the editor. The device is created with
    // SingleThreaded flag, meaning ALL rendering must happen on the UI thread; do not
    // touch Context from a worker.
    //
    // Feature level is locked to 10.0 to support legacy hardware. Anything that would
    // require FL11 (compute shaders, structured buffers, etc.) is not available here.
    public unsafe class Dx11RenderingDevice : RenderingDevice
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // All sector overlay textures (arrows, slope markers, etc.) are forced to this
        // size and packed into a single Texture2DArray. Changing this breaks the embedded
        // PNG resources because they're loaded in by name and validated by dimension.
        public const int SectorTextureSize = 256;
        private static Assembly ThisAssembly = Assembly.GetExecutingAssembly();
        public static ImageC TextureUnavailable = ImageC.FromStream(ThisAssembly.GetManifestResourceStream(nameof(TombLib) + "." + nameof(Rendering) + ".SectorTextures.texture_unavailable.png"));
        public static ImageC TextureCoordOutOfBounds = ImageC.FromStream(ThisAssembly.GetManifestResourceStream(nameof(TombLib) + "." + nameof(Rendering) + ".SectorTextures.texture_coord_out_of_bounds.png"));

        // Silk.NET API entry points for D3D11 and DXGI native function loading.
        public readonly D3D11 D3D11Api;
        public readonly DXGI DXGIApi;

        // Core COM objects. Stored as raw pointers for minimal overhead in the
        // rendering hot path. Released in Dispose().
        public readonly ID3D11Device* Device;
        public readonly IDXGIFactory* Factory;
        public readonly ID3D11DeviceContext* Context;

        public readonly Dx11PipelineState TextShader;
        public readonly Dx11PipelineState SpriteShader;
        public readonly Dx11PipelineState RoomShader;
        public readonly Dx11PipelineState LinesShader;
        public readonly Dx11PipelineState MeshShader;
        public readonly Dx11PipelineState ImportedGeometryShader;
        public readonly ID3D11RasterizerState* RasterizerBackCulling;
        // No back-face culling. Used for lines (where culling is irrelevant) and for
        // double-sided wireframe rendering. Lines are NOT affected by CullMode at all
        // in D3D11, but we still need a no-cull state for triangle wireframe.
        public readonly ID3D11RasterizerState* RasterizerNoCull;
        // Wireframe fill, no cull. Mirrors the legacy _rasterizerWireframe used by
        // Panel3D / WadTool panels for bounding boxes and debug overlays.
        public readonly ID3D11RasterizerState* RasterizerWireframe;
        public readonly ID3D11SamplerState* SamplerDefault;
        public readonly ID3D11SamplerState* SamplerRoundToNearest;
        public readonly ID3D11DepthStencilState* DepthStencilDefault;
        public readonly ID3D11DepthStencilState* DepthStencilNoZBuffer;
        // Depth test enabled, depth write disabled — for translucent passes that should
        // be occluded by opaque geometry but not occlude each other (ghost block bodies,
        // volume fills, etc.).
        public readonly ID3D11DepthStencilState* DepthStencilDepthRead;
        public readonly ID3D11BlendState* BlendingDisabled;
        public readonly ID3D11BlendState* BlendingPremultipliedAlpha;
        // Straight-alpha (non-premultiplied): SrcAlpha / InvSrcAlpha. Caller's vertex
        // colors are interpreted as straight RGBA; the shader does not need to
        // pre-multiply RGB by alpha.
        public readonly ID3D11BlendState* BlendingNonPremultipliedAlpha;
        // Additive glow: One / One. Alpha is ignored on the destination.
        public readonly ID3D11BlendState* BlendingAdditive;
        public readonly ID3D11Texture2D* SectorTextureArray;
        public readonly ID3D11ShaderResourceView* SectorTextureArrayView;
        public Dx11RenderingSwapChain CurrentRenderTarget = null;

        // Shared ring buffer for transient per-frame VBs (sprites, glyphs, debug lines).
        // See Dx11DynamicVertexBufferPool for the rationale. Created lazily because the
        // ctor body is already long enough; first user constructs it.
        private Dx11DynamicVertexBufferPool _dynamicVertexBuffers;
        public Dx11DynamicVertexBufferPool DynamicVertexBuffers
            => _dynamicVertexBuffers ??= new Dx11DynamicVertexBufferPool(this);

        public Dx11RenderingDevice()
        {
            logger.Info("Dx11 rendering device creating.");
#if DEBUG
            const uint DebugFlags = (uint)CreateDeviceFlag.Debug;
#else
            const uint DebugFlags = 0;
#endif
            try
            {
                D3D11Api = D3D11.GetApi();
                DXGIApi = DXGI.GetApi();

                // Create DXGI factory for adapter enumeration.
                IDXGIFactory* factory;
                SilkMarshal.ThrowHResult(
                    DXGIApi.CreateDXGIFactory(ref SilkMarshal.GuidOf<IDXGIFactory>(), (void**)&factory));
                Factory = factory;

                // Enumerate adapters.
                IDXGIAdapter* adapter;
                int hrAdapter = Factory->EnumAdapters(0, &adapter);
                if (hrAdapter != 0 || adapter == null)
                {
                    MessageBox.Show("Your system have no video adapters. Try to install video adapter.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new Exception("There are no valid video adapters in system.");
                }

                // Check for outputs.
                IDXGIOutput* output;
                int hrOutput = adapter->EnumOutputs(0, &output);
                if (hrOutput != 0)
                {
                    MessageBox.Show("There are no video displays connected to your system. Try to connect a display.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new Exception("No connected displays found.");
                }
                output->Release();

                // Log adapter info.
                AdapterDesc adapterDesc;
                adapter->GetDesc(&adapterDesc);
                string adapterName = new string((char*)adapterDesc.Description);
                long vramMB = (long)adapterDesc.DedicatedVideoMemory / 1024 / 1024;
                logger.Info("Creating D3D device: " + adapterName + ", " + vramMB + " MB GPU RAM.");

                // Create D3D11 device at feature level 10.0.
                D3DFeatureLevel featureLevel = D3DFeatureLevel.Level100;
                ID3D11Device* device;
                ID3D11DeviceContext* context;
                SilkMarshal.ThrowHResult(
                    D3D11Api.CreateDevice(
                        (IDXGIAdapter*)adapter,
                        D3DDriverType.Unknown,
                        0,
                        DebugFlags | (uint)CreateDeviceFlag.Singlethreaded,
                        &featureLevel, 1,
                        D3D11.SdkVersion,
                        &device, null, &context));
                Device = device;
                Context = context;

                adapter->Release();
            }
            catch (Exception exc)
            {
                switch ((uint)exc.HResult)
                {
                    case 0x887A0004:
                        MessageBox.Show("Your DirectX version, videocard or drivers are out of date.\nDirectX 11 installation and videocard with DirectX 10 support is required.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case 0x887A002D:
                        MessageBox.Show("Warning: provided build is a debug build.\nPlease install DirectX SDK or request release build from QA team.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case 0x887A0005:
                    case 0x887A0020:
                        MessageBox.Show("There was a serious video system error while initializing Direct3D device.\nTry to restart your system.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show("Unknown error while creating Direct3D device!\nShutting down now.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }

                throw new Exception("Can't create Direct3D 11 device! Exception: " + exc);
            }

#if DEBUG
            {
                ID3D11InfoQueue* infoQueue;
                Guid iid = typeof(ID3D11InfoQueue).GUID;
                int hr = ((IUnknown*)Device)->QueryInterface(&iid, (void**)&infoQueue);
                if (hr == 0)
                {
                    infoQueue->SetBreakOnSeverity(MessageSeverity.Warning, 1);
                    infoQueue->Release();
                }
            }
#endif

            try
            {
                TextShader = new Dx11PipelineState(this, "TextShader", new Dx11InputElement[]
                {
                new Dx11InputElement("POSITION", 0, Format.FormatR32G32Float, 0, 0, InputClassification.PerVertexData, 0),
                new Dx11InputElement("UVW", 0, Format.FormatR32G32Uint, 0, 1, InputClassification.PerVertexData, 0)
                });
                SpriteShader = new Dx11PipelineState(this, "SpriteShader", new Dx11InputElement[]
                {
                new Dx11InputElement("POSITION", 0, Format.FormatR32G32B32Float, 0, 0, InputClassification.PerVertexData, 0),
                new Dx11InputElement("COLOR", 0, Format.FormatR32G32B32A32Float, 0, 1, InputClassification.PerVertexData, 0),
                new Dx11InputElement("UVW", 0, Format.FormatR32G32Uint, 0, 2, InputClassification.PerVertexData, 0)
                });
                RoomShader = new Dx11PipelineState(this, "RoomShader", new Dx11InputElement[]
                {
                new Dx11InputElement("POSITION", 0, Format.FormatR32G32B32Float, 0, 0, InputClassification.PerVertexData, 0),
                new Dx11InputElement("COLOR", 0, Format.FormatR8G8B8A8Unorm, 0, 1, InputClassification.PerVertexData, 0),
                new Dx11InputElement("OVERLAY", 0, Format.FormatR8G8B8A8Unorm, 0, 2, InputClassification.PerVertexData, 0),
                new Dx11InputElement("UVWANDBLENDMODE", 0, Format.FormatR32G32Uint, 0, 3, InputClassification.PerVertexData, 0),
                new Dx11InputElement("EDITORUVANDSECTORTEXTURE", 0, Format.FormatR32Uint, 0, 4, InputClassification.PerVertexData, 0)
                });
                LinesShader = new Dx11PipelineState(this, "LinesShader", new Dx11InputElement[]
                {
                new Dx11InputElement("POSITION", 0, Format.FormatR32G32B32Float, 0, 0, InputClassification.PerVertexData, 0),
                new Dx11InputElement("COLOR", 0, Format.FormatR32G32B32A32Float, 0, 1, InputClassification.PerVertexData, 0)
                });
                // Single interleaved AOS layout matching MeshVertex (R32G32B32_Float per
                // Vector3 field; R32G32B32A32_Float per Vector4 field). All in slot 0.
                MeshShader = new Dx11PipelineState(this, "MeshShader", new Dx11InputElement[]
                {
                new Dx11InputElement("POSITION",     0, Format.FormatR32G32B32Float,    0, 0, InputClassification.PerVertexData, 0),
                new Dx11InputElement("TEXCOORD",     0, Format.FormatR32G32B32Float,   12, 0, InputClassification.PerVertexData, 0),
                new Dx11InputElement("NORMAL",       0, Format.FormatR32G32B32Float,   24, 0, InputClassification.PerVertexData, 0),
                new Dx11InputElement("COLOR",        0, Format.FormatR32G32B32Float,   36, 0, InputClassification.PerVertexData, 0),
                new Dx11InputElement("BLENDINDICES", 0, Format.FormatR32G32B32A32Float, 48, 0, InputClassification.PerVertexData, 0),
                new Dx11InputElement("BLENDWEIGHTS", 0, Format.FormatR32G32B32A32Float, 64, 0, InputClassification.PerVertexData, 0),
                });
                // Layout matches RenderingDrawingImportedGeometry.Vertex: Pos@0, UV@12,
                // Color@20, Normal@32. Total 44 bytes per vertex.
                ImportedGeometryShader = new Dx11PipelineState(this, "ImportedGeometryShader", new Dx11InputElement[]
                {
                new Dx11InputElement("POSITION", 0, Format.FormatR32G32B32Float,  0, 0, InputClassification.PerVertexData, 0),
                new Dx11InputElement("TEXCOORD", 0, Format.FormatR32G32Float,    12, 0, InputClassification.PerVertexData, 0),
                new Dx11InputElement("COLOR",    0, Format.FormatR32G32B32Float, 20, 0, InputClassification.PerVertexData, 0),
                new Dx11InputElement("NORMAL",   0, Format.FormatR32G32B32Float, 32, 0, InputClassification.PerVertexData, 0),
                });

                // Rasterizer states
                {
                    var desc = new RasterizerDesc
                    {
                        CullMode = CullMode.Back,
                        FillMode = FillMode.Solid,
                    };
                    ID3D11RasterizerState* rs;
                    SilkMarshal.ThrowHResult(Device->CreateRasterizerState(&desc, &rs));
                    RasterizerBackCulling = rs;
                }
                {
                    var desc = new RasterizerDesc
                    {
                        CullMode = CullMode.None,
                        FillMode = FillMode.Solid,
                    };
                    ID3D11RasterizerState* rs;
                    SilkMarshal.ThrowHResult(Device->CreateRasterizerState(&desc, &rs));
                    RasterizerNoCull = rs;
                }
                {
                    var desc = new RasterizerDesc
                    {
                        CullMode = CullMode.None,
                        FillMode = FillMode.Wireframe,
                        AntialiasedLineEnable = 1,
                    };
                    ID3D11RasterizerState* rs;
                    SilkMarshal.ThrowHResult(Device->CreateRasterizerState(&desc, &rs));
                    RasterizerWireframe = rs;
                }

                // Sampler states
                {
                    var desc = new SamplerDesc
                    {
                        AddressU = TextureAddressMode.Mirror,
                        AddressV = TextureAddressMode.Mirror,
                        AddressW = TextureAddressMode.Wrap,
                        Filter = Filter.Anisotropic,
                        MaxAnisotropy = 4,
                        MaxLOD = float.MaxValue,
                    };
                    ID3D11SamplerState* ss;
                    SilkMarshal.ThrowHResult(Device->CreateSamplerState(&desc, &ss));
                    SamplerDefault = ss;
                }
                {
                    var desc = new SamplerDesc
                    {
                        AddressU = TextureAddressMode.Mirror,
                        AddressV = TextureAddressMode.Mirror,
                        AddressW = TextureAddressMode.Wrap,
                        Filter = Filter.MinMagMipPoint,
                        MaxAnisotropy = 4,
                        MaxLOD = float.MaxValue,
                    };
                    ID3D11SamplerState* ss;
                    SilkMarshal.ThrowHResult(Device->CreateSamplerState(&desc, &ss));
                    SamplerRoundToNearest = ss;
                }

                // Depth stencil states
                {
                    var desc = new DepthStencilDesc
                    {
                        DepthEnable = 1,
                        DepthWriteMask = DepthWriteMask.All,
                        DepthFunc = ComparisonFunc.LessEqual,
                        StencilEnable = 0,
                        StencilReadMask = 0xFF,
                        StencilWriteMask = 0xFF,
                        FrontFace = DefaultStencilOp(),
                        BackFace = DefaultStencilOp(),
                    };
                    ID3D11DepthStencilState* dss;
                    SilkMarshal.ThrowHResult(Device->CreateDepthStencilState(&desc, &dss));
                    DepthStencilDefault = dss;
                }
                {
                    var desc = new DepthStencilDesc
                    {
                        DepthEnable = 0,
                        DepthWriteMask = DepthWriteMask.Zero,
                        DepthFunc = ComparisonFunc.Always,
                        StencilEnable = 0,
                        StencilReadMask = 0xFF,
                        StencilWriteMask = 0xFF,
                        FrontFace = DefaultStencilOp(),
                        BackFace = DefaultStencilOp(),
                    };
                    ID3D11DepthStencilState* dss;
                    SilkMarshal.ThrowHResult(Device->CreateDepthStencilState(&desc, &dss));
                    DepthStencilNoZBuffer = dss;
                }
                {
                    var desc = new DepthStencilDesc
                    {
                        DepthEnable = 1,
                        DepthWriteMask = DepthWriteMask.Zero, // read but don't write
                        DepthFunc = ComparisonFunc.LessEqual,
                        StencilEnable = 0,
                        StencilReadMask = 0xFF,
                        StencilWriteMask = 0xFF,
                        FrontFace = DefaultStencilOp(),
                        BackFace = DefaultStencilOp(),
                    };
                    ID3D11DepthStencilState* dss;
                    SilkMarshal.ThrowHResult(Device->CreateDepthStencilState(&desc, &dss));
                    DepthStencilDepthRead = dss;
                }

                // Blend states
                {
                    var desc = DefaultBlendDesc();
                    ID3D11BlendState* bs;
                    SilkMarshal.ThrowHResult(Device->CreateBlendState(&desc, &bs));
                    BlendingDisabled = bs;
                }
                {
                    var desc = DefaultBlendDesc();
                    desc.RenderTarget[0].BlendEnable = 1;
                    desc.RenderTarget[0].SrcBlend = desc.RenderTarget[0].SrcBlendAlpha = Blend.One;
                    desc.RenderTarget[0].DestBlend = desc.RenderTarget[0].DestBlendAlpha = Blend.InvSrcAlpha;
                    desc.RenderTarget[0].BlendOp = desc.RenderTarget[0].BlendOpAlpha = BlendOp.Add;
                    desc.RenderTarget[0].RenderTargetWriteMask = (byte)ColorWriteEnable.All;
                    ID3D11BlendState* bs;
                    SilkMarshal.ThrowHResult(Device->CreateBlendState(&desc, &bs));
                    BlendingPremultipliedAlpha = bs;
                }
                {
                    var desc = DefaultBlendDesc();
                    desc.RenderTarget[0].BlendEnable = 1;
                    desc.RenderTarget[0].SrcBlend = desc.RenderTarget[0].SrcBlendAlpha = Blend.SrcAlpha;
                    desc.RenderTarget[0].DestBlend = desc.RenderTarget[0].DestBlendAlpha = Blend.InvSrcAlpha;
                    desc.RenderTarget[0].BlendOp = desc.RenderTarget[0].BlendOpAlpha = BlendOp.Add;
                    desc.RenderTarget[0].RenderTargetWriteMask = (byte)ColorWriteEnable.All;
                    ID3D11BlendState* bs;
                    SilkMarshal.ThrowHResult(Device->CreateBlendState(&desc, &bs));
                    BlendingNonPremultipliedAlpha = bs;
                }
                {
                    var desc = DefaultBlendDesc();
                    desc.RenderTarget[0].BlendEnable = 1;
                    desc.RenderTarget[0].SrcBlend = desc.RenderTarget[0].SrcBlendAlpha = Blend.One;
                    desc.RenderTarget[0].DestBlend = desc.RenderTarget[0].DestBlendAlpha = Blend.One;
                    desc.RenderTarget[0].BlendOp = desc.RenderTarget[0].BlendOpAlpha = BlendOp.Add;
                    desc.RenderTarget[0].RenderTargetWriteMask = (byte)ColorWriteEnable.All;
                    ID3D11BlendState* bs;
                    SilkMarshal.ThrowHResult(Device->CreateBlendState(&desc, &bs));
                    BlendingAdditive = bs;
                }
            }
            catch (Exception exc)
            {
                throw new Exception("Can't assign needed Direct3D parameters! Exception: " + exc);
            }

            // Sector textures
            uint formatSupport;
            Device->CheckFormatSupport(Format.FormatB5G5R5A1Unorm, &formatSupport);
            bool support16BitTexture = (formatSupport & (uint)FormatSupport.Texture2D) != 0;
            string[] sectorTextureNames = Enum.GetNames(typeof(SectorTexture)).Skip(1).ToArray();
            GCHandle[] handles = new GCHandle[sectorTextureNames.Length];
            try
            {
                SubresourceData[] subresources = new SubresourceData[sectorTextureNames.Length];
                for (int i = 0; i < sectorTextureNames.Length; ++i)
                {
                    string name = nameof(TombLib) + "." + nameof(Rendering) + ".SectorTextures." + sectorTextureNames[i] + ".png";
                    using (Stream stream = ThisAssembly.GetManifestResourceStream(name))
                    {
                        ImageC image = ImageC.FromStream(stream);
                        if ((image.Width != SectorTextureSize) || (image.Height != SectorTextureSize))
                            throw new ArgumentOutOfRangeException("The embedded resource '" + name + "' is not of a valid size.");

                        if (support16BitTexture)
                        { // Compress image data into B5G5R5A1 format to save a bit of GPU memory.
                            ushort[] sectorTextureData = new ushort[SectorTextureSize * SectorTextureSize];
                            for (int j = 0; j < (SectorTextureSize * SectorTextureSize); ++j)
                            {
                                ColorC Color = image.Get(j);
                                sectorTextureData[j] = (ushort)(
                                    ((Color.B >> 3) << 0) |
                                    ((Color.G >> 3) << 5) |
                                    ((Color.R >> 3) << 10) |
                                    ((Color.A >> 7) << 15));
                            }
                            handles[i] = GCHandle.Alloc(sectorTextureData, GCHandleType.Pinned);
                            subresources[i] = new SubresourceData
                            {
                                PSysMem = (void*)handles[i].AddrOfPinnedObject(),
                                SysMemPitch = (uint)(sizeof(ushort) * SectorTextureSize),
                            };
                        }
                        else
                        {
                            handles[i] = GCHandle.Alloc(image.ToByteArray(), GCHandleType.Pinned);
                            subresources[i] = new SubresourceData
                            {
                                PSysMem = (void*)handles[i].AddrOfPinnedObject(),
                                SysMemPitch = (uint)(sizeof(uint) * SectorTextureSize),
                            };
                        }
                    }
                }

                var texDesc = new Texture2DDesc
                {
                    Width = SectorTextureSize,
                    Height = SectorTextureSize,
                    MipLevels = 1,
                    ArraySize = (uint)sectorTextureNames.Length,
                    Format = support16BitTexture ? Format.FormatB5G5R5A1Unorm : Format.FormatB8G8R8A8Unorm,
                    SampleDesc = new SampleDesc(1, 0),
                    Usage = D3D11Usage.Immutable,
                    BindFlags = (uint)BindFlag.ShaderResource,
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                };

                ID3D11Texture2D* tex;
                fixed (SubresourceData* pSubresources = subresources)
                {
                    SilkMarshal.ThrowHResult(Device->CreateTexture2D(&texDesc, pSubresources, &tex));
                }
                SectorTextureArray = tex;
            }
            finally
            {
                foreach (GCHandle handle in handles)
                    handle.Free();
            }
            {
                ID3D11ShaderResourceView* srv;
                SilkMarshal.ThrowHResult(Device->CreateShaderResourceView((ID3D11Resource*)SectorTextureArray, null, &srv));
                SectorTextureArrayView = srv;
            }

            // Set omni present state
            ResetState();

            logger.Info("Dx11 rendering device created.");
        }

        private static DepthStencilopDesc DefaultStencilOp()
        {
            return new DepthStencilopDesc
            {
                StencilFailOp = StencilOp.Keep,
                StencilDepthFailOp = StencilOp.Keep,
                StencilPassOp = StencilOp.Keep,
                StencilFunc = ComparisonFunc.Always,
            };
        }

        private static BlendDesc DefaultBlendDesc()
        {
            var desc = new BlendDesc();
            for (int i = 0; i < 8; i++)
            {
                desc.RenderTarget[i].BlendEnable = 0;
                desc.RenderTarget[i].SrcBlend = Blend.One;
                desc.RenderTarget[i].DestBlend = Blend.Zero;
                desc.RenderTarget[i].BlendOp = BlendOp.Add;
                desc.RenderTarget[i].SrcBlendAlpha = Blend.One;
                desc.RenderTarget[i].DestBlendAlpha = Blend.Zero;
                desc.RenderTarget[i].BlendOpAlpha = BlendOp.Add;
                desc.RenderTarget[i].RenderTargetWriteMask = (byte)ColorWriteEnable.All;
            }
            return desc;
        }

        public override void ResetState()
        {
            Context->RSSetState(RasterizerBackCulling);
            Context->OMSetDepthStencilState(DepthStencilDefault, 0);
            Context->OMSetBlendState(BlendingPremultipliedAlpha, null, 0xFFFFFFFF);
        }

        public override void Dispose()
        {
            try
            {
                Context->ClearState();
                Context->Flush();
            }
            finally
            {
                _dynamicVertexBuffers?.Dispose();
                SectorTextureArrayView->Release();
                SectorTextureArray->Release();
                DepthStencilDefault->Release();
                DepthStencilNoZBuffer->Release();
                DepthStencilDepthRead->Release();
                BlendingDisabled->Release();
                BlendingPremultipliedAlpha->Release();
                BlendingNonPremultipliedAlpha->Release();
                BlendingAdditive->Release();
                SamplerDefault->Release();
                SamplerRoundToNearest->Release();
                RasterizerBackCulling->Release();
                RasterizerNoCull->Release();
                RasterizerWireframe->Release();
                LinesShader.Dispose();
                MeshShader.Dispose();
                ImportedGeometryShader.Dispose();
                RoomShader.Dispose();
                Context->Release();
                Device->Release();
                Factory->Release();
            }
        }

        // Packs an RGBA color into a single uint in the same layout the room shader
        // expects (R8G8B8A8_UNorm).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CompressColor(Vector3 color, float alpha = 1.0f, bool average = true)
        {
            float multiplier = average ? 128.0f : 255.0f;
            color = Vector3.Max(new Vector3(), Vector3.Min(new Vector3(255.0f), color * multiplier + new Vector3(0.5f)));
            return ((uint)color.X) | (((uint)color.Y) << 8) | (((uint)color.Z) << 16) | ((uint)(MathC.Clamp(alpha, 0, 1) * 255.0f) << 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CompressUvw(VectorInt3 position, Vector2 textureScaling, Vector2 uv, uint highestBits = 0)
        {
            uint blendMode2 = Math.Min(highestBits, 15);
            uint x = (uint)((position.X + uv.X) * textureScaling.X);
            uint y = (uint)((position.Y + uv.Y) * textureScaling.Y);
            return x | ((ulong)y << 24) | ((ulong)position.Z << 48) | ((ulong)blendMode2 << 60);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorInt3 UncompressUvw(ulong value, Vector2 textureScaling)
        {
            Vector2 uv = new Vector2(value & 0xFFFFFF, (value >> 24) & 0xFFFFFF) / textureScaling;
            int w = (int)((value >> 48) & 0x3FF);
            return new VectorInt3((int)uv.X, (int)uv.Y, w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UncompressUvw(ulong value, VectorInt3 position, Vector2 textureScaling, out Vector2 uv, out uint highestBits)
        {
            uv = new Vector2(value & 0xFFFFFF, (value >> 24) & 0xFFFFFF) / textureScaling.X - new Vector2(position.X, position.Y);
            highestBits = (uint)(value >> 60);
        }

        ///<summary>Works even on immutable buffers</summary>
        public byte[] ReadBuffer(ID3D11Buffer* buffer, int size)
        {
            var desc = new BufferDesc
            {
                ByteWidth = (uint)size,
                Usage = D3D11Usage.Staging,
                BindFlags = 0,
                CPUAccessFlags = (uint)CpuAccessFlag.Read,
                MiscFlags = 0,
                StructureByteStride = 0,
            };
            ID3D11Buffer* tempBuffer;
            SilkMarshal.ThrowHResult(Device->CreateBuffer(&desc, null, &tempBuffer));
            try
            {
                Context->CopyResource((ID3D11Resource*)tempBuffer, (ID3D11Resource*)buffer);
                MappedSubresource mapped;
                SilkMarshal.ThrowHResult(Context->Map((ID3D11Resource*)tempBuffer, 0, Map.Read, 0, &mapped));
                try
                {
                    byte[] result = new byte[size];
                    Marshal.Copy((IntPtr)mapped.PData, result, 0, size);
                    return result;
                }
                finally
                {
                    Context->Unmap((ID3D11Resource*)tempBuffer, 0);
                }
            }
            finally
            {
                tempBuffer->Release();
            }
        }

        public Texture2DDesc CreateTextureDescription(VectorInt3 size)
        {
            return new Texture2DDesc
            {
                Height = (uint)size.X,
                Width = (uint)size.Y,
                ArraySize = (uint)size.Z,
                BindFlags = (uint)BindFlag.ShaderResource,
                CPUAccessFlags = 0,
                Format = Format.FormatB8G8R8A8Unorm,
                MipLevels = 1,
                MiscFlags = 0,
                SampleDesc = new SampleDesc(1, 0),
                Usage = D3D11Usage.Default,
            };
        }

        public VectorInt3 GetAvailableTextureAllocatorSize(VectorInt3 size)
        {
            if (size.Z < RenderingTextureAllocator.MinimumPageCount)
                return size;

            logger.Info("Trying to reserve " + size.Z + " " + size.X + "x" + size.Y + " pages of texture memory...");

            var dx11Description = CreateTextureDescription(size);

            while (dx11Description.ArraySize >= RenderingTextureAllocator.MinimumPageCount)
            {
                ID3D11Texture2D* test;
                int hr = Device->CreateTexture2D(&dx11Description, null, &test);
                if (hr == 0)
                {
                    test->Release();
                    logger.Info(dx11Description.ArraySize + " texture pages were successfully reserved.");
                    return new VectorInt3(size.X, size.Y, (int)dx11Description.ArraySize);
                }
                else
                {
                    logger.Warn("Not enough memory to allocate " + dx11Description.ArraySize + " texture pages. Trying to reduce page count...");
                    dx11Description.ArraySize -= 2;
                }
            }

            throw new NotSupportedException("Video system does not have enough video memory to create texture array. Please upgrade your graphics adapter.");
        }

        public override RenderingSwapChain CreateSwapChain(RenderingSwapChain.Description description)
        {
            return new Dx11RenderingSwapChain(this, description);
        }

        public override RenderingDrawingTest CreateDrawingTest(RenderingDrawingTest.Description description)
        {
            return new Dx11RenderingDrawingTest(this, description);
        }

        public override RenderingDrawingRoom CreateDrawingRoom(RenderingDrawingRoom.Description description)
        {
            return new Dx11RenderingDrawingRoom(this, description);
        }

        public override RenderingTextureAllocator CreateTextureAllocator(RenderingTextureAllocator.Description description)
        {
            description.Size = GetAvailableTextureAllocatorSize(description.Size);
            return new Dx11RenderingTextureAllocator(this, description);
        }
        public override RenderingFont CreateFont(RenderingFont.Description description)
        {
            return new RenderingFont(description);
        }

        public override RenderingStateBuffer CreateStateBuffer()
        {
            return new Dx11RenderingStateBuffer(this);
        }

        public override RenderingDrawingLines CreateDrawingLines(RenderingDrawingLines.Description description)
        {
            return new Dx11RenderingDrawingLines(this, description);
        }

        public override RenderingDrawingMesh CreateDrawingMesh(RenderingDrawingMesh.Description description)
        {
            return new Dx11RenderingDrawingMesh(this, description);
        }

        public override RenderingDrawingImportedGeometry CreateDrawingImportedGeometry(RenderingDrawingImportedGeometry.Description description)
        {
            return new Dx11RenderingDrawingImportedGeometry(this, description);
        }

        // Helper to set a debug name on a D3D11 device child via SetPrivateData.
        public static void SetDebugName(ID3D11DeviceChild* child, string debugName)
        {
            if (child == null) return;
            byte[] nameBytes = Encoding.ASCII.GetBytes(debugName);
            Guid guid = new Guid("429b8c22-9188-4b0c-8742-acb0bf85c200"); // WKPDID_D3DDebugObjectName
            fixed (byte* ptr = nameBytes)
                child->SetPrivateData(&guid, (uint)nameBytes.Length, ptr);
        }

        // Helper to bind vertex buffers from Dx11VertexBufferBinding arrays.
        public static void SetVertexBuffers(ID3D11DeviceContext* context, uint startSlot, Dx11VertexBufferBinding[] bindings)
        {
            int count = bindings.Length;
            ID3D11Buffer** buffers = stackalloc ID3D11Buffer*[count];
            uint* strides = stackalloc uint[count];
            uint* offsets = stackalloc uint[count];
            for (int i = 0; i < count; i++)
            {
                buffers[i] = bindings[i].Buffer;
                strides[i] = bindings[i].Stride;
                offsets[i] = bindings[i].Offset;
            }
            context->IASetVertexBuffers(startSlot, (uint)count, buffers, strides, offsets);
        }
    }

    // Replacement for SharpDX.Direct3D11.VertexBufferBinding.
    public unsafe struct Dx11VertexBufferBinding
    {
        public ID3D11Buffer* Buffer;
        public uint Stride;
        public uint Offset;

        public Dx11VertexBufferBinding(ID3D11Buffer* buffer, int stride, int offset)
        {
            Buffer = buffer;
            Stride = (uint)stride;
            Offset = (uint)offset;
        }
    }

    // Managed representation of an input element, carrying the semantic name as a
    // string. Dx11PipelineState converts these to InputElementDesc (with byte*
    // SemanticName) during CreateInputLayout.
    public struct Dx11InputElement
    {
        public string SemanticName;
        public uint SemanticIndex;
        public Format Format;
        public uint AlignedByteOffset;
        public uint InputSlot;
        public InputClassification InputSlotClass;
        public uint InstanceDataStepRate;

        public Dx11InputElement(string semanticName, uint semanticIndex, Format format, uint alignedByteOffset, uint inputSlot, InputClassification inputSlotClass, uint instanceDataStepRate)
        {
            SemanticName = semanticName;
            SemanticIndex = semanticIndex;
            Format = format;
            AlignedByteOffset = alignedByteOffset;
            InputSlot = inputSlot;
            InputSlotClass = inputSlotClass;
            InstanceDataStepRate = instanceDataStepRate;
        }
    }
}
