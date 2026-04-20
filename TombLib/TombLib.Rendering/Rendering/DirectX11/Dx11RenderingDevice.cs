using NLog;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TombLib.Utils;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11RenderingDevice : RenderingDevice
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const int SectorTextureSize = 256;
        private static readonly Assembly ThisAssembly = Assembly.GetExecutingAssembly();
        public static ImageC TextureUnavailable = ImageC.FromStream(ThisAssembly.GetManifestResourceStream(nameof(TombLib) + "." + nameof(Rendering) + ".SectorTextures.texture_unavailable.png"));
        public static ImageC TextureCoordOutOfBounds = ImageC.FromStream(ThisAssembly.GetManifestResourceStream(nameof(TombLib) + "." + nameof(Rendering) + ".SectorTextures.texture_coord_out_of_bounds.png"));
        public readonly ID3D11Device Device;
        public readonly IDXGIFactory1 Factory;
        public readonly ID3D11DeviceContext Context;
        public readonly Dx11PipelineState TextShader;
        public readonly Dx11PipelineState SpriteShader;
        public readonly Dx11PipelineState RoomShader;
        public readonly ID3D11RasterizerState RasterizerBackCulling;
        public readonly ID3D11SamplerState SamplerDefault;
        public readonly ID3D11SamplerState SamplerRoundToNearest;
        public readonly ID3D11DepthStencilState DepthStencilDefault;
        public readonly ID3D11DepthStencilState DepthStencilNoZBuffer;
        public readonly ID3D11BlendState BlendingDisabled;
        public readonly ID3D11BlendState BlendingPremultipliedAlpha;
        public readonly ID3D11Texture2D SectorTextureArray;
        public readonly ID3D11ShaderResourceView SectorTextureArrayView;
        public RenderingSwapChain CurrentRenderTarget = null;

        public Dx11RenderingDevice()
        {
            logger.Info("Dx11 rendering device creating.");
#if DEBUG
            const DeviceCreationFlags DebugFlags = DeviceCreationFlags.Debug;
#else
            const DeviceCreationFlags DebugFlags = DeviceCreationFlags.None;
#endif
            try
            {
                Factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();

                Factory.EnumAdapters1(0, out var adapter);
                if (adapter == null)
                {
                    MessageBox.Show("DirectX wasn't able to acquire video adapter. Try to restart your system.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new Exception("DirectX wasn't able to acquire video adapter.");
                }

                var adapterDesc = adapter.Description1;
                logger.Info("Creating D3D device: " + adapterDesc.Description + ", " +
                    (adapterDesc.DedicatedVideoMemory / 1024 / 1024) + " MB GPU RAM.");

                D3D11.D3D11CreateDevice(
                    adapter,
                    DriverType.Unknown,
                    DebugFlags,
                    new[] { FeatureLevel.Level_10_0 },
                    out var device,
                    out _,
                    out var context);

                Device = device!;
                Context = context!;
                adapter.Dispose();
            }
            catch (Exception exc)
            {
                switch (unchecked((uint)exc.HResult))
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

            try
            {
                TextShader = new Dx11PipelineState(this, "TextShader", new InputElementDescription[]
                {
                    new InputElementDescription("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
                    new InputElementDescription("UVW", 0, Format.R32G32_UInt, 0, 1, InputClassification.PerVertexData, 0)
                });
                SpriteShader = new Dx11PipelineState(this, "SpriteShader", new InputElementDescription[]
                {
                    new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                    new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 0, 1, InputClassification.PerVertexData, 0),
                    new InputElementDescription("UVW", 0, Format.R32G32_UInt, 0, 2, InputClassification.PerVertexData, 0)
                });
                RoomShader = new Dx11PipelineState(this, "RoomShader", new InputElementDescription[]
                {
                    new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                    new InputElementDescription("COLOR", 0, Format.R8G8B8A8_UNorm, 0, 1, InputClassification.PerVertexData, 0),
                    new InputElementDescription("OVERLAY", 0, Format.R8G8B8A8_UNorm, 0, 2, InputClassification.PerVertexData, 0),
                    new InputElementDescription("UVWANDBLENDMODE", 0, Format.R32G32_UInt, 0, 3, InputClassification.PerVertexData, 0),
                    new InputElementDescription("EDITORUVANDSECTORTEXTURE", 0, Format.R32_UInt, 0, 4, InputClassification.PerVertexData, 0)
                });
                RasterizerBackCulling = Device.CreateRasterizerState(new RasterizerDescription
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
                });
                SamplerDefault = Device.CreateSamplerState(new SamplerDescription
                {
                    AddressU = TextureAddressMode.Mirror,
                    AddressV = TextureAddressMode.Mirror,
                    AddressW = TextureAddressMode.Wrap,
                    Filter = Filter.Anisotropic,
                    MaxAnisotropy = 4,
                });
                SamplerRoundToNearest = Device.CreateSamplerState(new SamplerDescription
                {
                    AddressU = TextureAddressMode.Mirror,
                    AddressV = TextureAddressMode.Mirror,
                    AddressW = TextureAddressMode.Wrap,
                    Filter = Filter.MinMagMipPoint,
                    MaxAnisotropy = 4,
                });
                {
                    var desc = new DepthStencilDescription
                    {
                        DepthEnable = true,
                        DepthWriteMask = DepthWriteMask.All,
                        DepthFunc = ComparisonFunction.LessEqual,
                        StencilEnable = false,
                    };
                    DepthStencilDefault = Device.CreateDepthStencilState(desc);
                }
                {
                    var desc = new DepthStencilDescription
                    {
                        DepthEnable = false,
                        DepthWriteMask = DepthWriteMask.Zero,
                        DepthFunc = ComparisonFunction.Always,
                        StencilEnable = false,
                    };
                    DepthStencilNoZBuffer = Device.CreateDepthStencilState(desc);
                }
                BlendingDisabled = Device.CreateBlendState(BlendDescription.Opaque);
                {
                    var desc = new BlendDescription();
                    desc.RenderTarget[0].BlendEnable = true;
                    desc.RenderTarget[0].SourceBlend = desc.RenderTarget[0].SourceBlendAlpha = Blend.One;
                    desc.RenderTarget[0].DestinationBlend = desc.RenderTarget[0].DestinationBlendAlpha = Blend.InverseSourceAlpha;
                    desc.RenderTarget[0].BlendOperation = desc.RenderTarget[0].BlendOperationAlpha = BlendOperation.Add;
                    desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteEnable.All;
                    BlendingPremultipliedAlpha = Device.CreateBlendState(desc);
                }
            }
            catch (Exception exc)
            {
                throw new Exception("Can't assign needed Direct3D parameters! Exception: " + exc);
            }

            // Sector textures
            bool support16BitTexture = Device.CheckFormatSupport(Format.B5G5R5A1_UNorm).HasFlag(FormatSupport.Texture2D);
            string[] sectorTextureNames = Enum.GetNames(typeof(SectorTexture)).Skip(1).ToArray();
            GCHandle[] handles = new GCHandle[sectorTextureNames.Length];
            try
            {
                SubresourceData[] subresourceData = new SubresourceData[sectorTextureNames.Length];
                for (int i = 0; i < sectorTextureNames.Length; ++i)
                {
                    string name = nameof(TombLib) + "." + nameof(Rendering) + ".SectorTextures." + sectorTextureNames[i] + ".png";
                    using (Stream stream = ThisAssembly.GetManifestResourceStream(name))
                    {
                        ImageC image = ImageC.FromStream(stream);
                        if ((image.Width != SectorTextureSize) || (image.Height != SectorTextureSize))
                            throw new ArgumentOutOfRangeException("The embedded resource '" + name + "' is not of a valid size.");

                        if (support16BitTexture)
                        {
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
                            subresourceData[i] = new SubresourceData(handles[i].AddrOfPinnedObject(), (uint)(sizeof(ushort) * SectorTextureSize));
                        }
                        else
                        {
                            handles[i] = GCHandle.Alloc(image.ToByteArray(), GCHandleType.Pinned);
                            subresourceData[i] = new SubresourceData(handles[i].AddrOfPinnedObject(), (uint)(sizeof(uint) * SectorTextureSize));
                        }
                    }
                }

                SectorTextureArray = Device.CreateTexture2D(new Texture2DDescription
                {
                    Width = (uint)SectorTextureSize,
                    Height = (uint)SectorTextureSize,
                    MipLevels = 1,
                    ArraySize = (uint)sectorTextureNames.Length,
                    Format = support16BitTexture ? Format.B5G5R5A1_UNorm : Format.B8G8R8A8_UNorm,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Immutable,
                    BindFlags = BindFlags.ShaderResource,
                    CPUAccessFlags = CpuAccessFlags.None,
                    MiscFlags = ResourceOptionFlags.None
                }, subresourceData);
            }
            finally
            {
                foreach (GCHandle handle in handles)
                    handle.Free();
            }
            SectorTextureArrayView = Device.CreateShaderResourceView(SectorTextureArray);

            // Set omni present state
            ResetState();

            logger.Info("Dx11 rendering device created.");
        }

        public void ResetState()
        {
            Context.RSSetState(RasterizerBackCulling);
            Context.OMSetDepthStencilState(DepthStencilDefault);
            Context.OMSetBlendState(BlendingPremultipliedAlpha);
        }

        public override void Dispose()
        {
            try
            {
                Context.ClearState();
                Context.Flush();
            }
            finally
            {
                SectorTextureArrayView.Dispose();
                SectorTextureArray.Dispose();
                DepthStencilDefault.Dispose();
                DepthStencilNoZBuffer.Dispose();
                BlendingDisabled.Dispose();
                BlendingPremultipliedAlpha.Dispose();
                SamplerDefault.Dispose();
                SamplerRoundToNearest.Dispose();
                RasterizerBackCulling.Dispose();
                RoomShader.Dispose();
                Context.Dispose();
                Device.Dispose();
                Factory.Dispose();
            }
        }

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

        ///<summary>Works even on immutable buffers.</summary>
        public byte[] ReadBuffer(ID3D11Buffer buffer, int size)
        {
            using (var tempBuffer = Device.CreateBuffer(new BufferDescription((uint)size, BindFlags.None, ResourceUsage.Staging, CpuAccessFlags.Read)))
            {
                Context.CopyResource(tempBuffer, buffer);
                var mapped = Context.Map(tempBuffer, 0, MapMode.Read);
                try
                {
                    byte[] result = new byte[size];
                    Marshal.Copy(mapped.DataPointer, result, 0, size);
                    return result;
                }
                finally
                {
                    Context.Unmap(tempBuffer, 0);
                }
            }
        }

        public Texture2DDescription CreateTextureDescription(VectorInt3 size)
        {
            return new Texture2DDescription
            {
                Height = (uint)size.X,
                Width = (uint)size.Y,
                ArraySize = (uint)size.Z,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                MipLevels = 1,
                MiscFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
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
                try
                {
                    var test = Device.CreateTexture2D(dx11Description);
                    test.Dispose();

                    logger.Info(dx11Description.ArraySize + " texture pages were successfully reserved.");
                    return new VectorInt3(size.X, size.Y, (int)dx11Description.ArraySize);
                }
                catch
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
    }
}
