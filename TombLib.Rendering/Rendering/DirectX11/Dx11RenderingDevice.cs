﻿using NLog;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TombLib.Utils;
using Buffer = SharpDX.Direct3D11.Buffer;
using Factory = SharpDX.DXGI.Factory;
using Format = SharpDX.DXGI.Format;
using SampleDescription = SharpDX.DXGI.SampleDescription;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11RenderingDevice : RenderingDevice
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const int SectorTextureSize = 256;
        private static Assembly ThisAssembly = Assembly.GetExecutingAssembly();
        public static ImageC TextureUnavailable = ImageC.FromStream(ThisAssembly.GetManifestResourceStream(nameof(TombLib) + "." + nameof(Rendering) + ".SectorTextures.texture_unavailable.png"));
        public static ImageC TextureCoordOutOfBounds = ImageC.FromStream(ThisAssembly.GetManifestResourceStream(nameof(TombLib) + "." + nameof(Rendering) + ".SectorTextures.texture_coord_out_of_bounds.png"));
        public readonly Device Device;
        public readonly Factory Factory;
        public readonly DeviceContext Context;
        //public readonly Dx11PipelineState TestShader;
        public readonly Dx11PipelineState TextShader;
        public readonly Dx11PipelineState SpriteShader;
        public readonly Dx11PipelineState RoomShader;
        public readonly RasterizerState RasterizerBackCulling;
        public readonly SamplerState SamplerDefault;
        public readonly SamplerState SamplerRoundToNearest;
        public readonly DepthStencilState DepthStencilDefault;
        public readonly DepthStencilState DepthStencilNoZBuffer;
        public readonly BlendState BlendingDisabled;
        public readonly BlendState BlendingPremultipliedAlpha;
        public readonly Texture2D SectorTextureArray;
        public readonly ShaderResourceView SectorTextureArrayView;
        public Dx11RenderingSwapChain CurrentRenderTarget = null;

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
                Factory = new Factory();
                if (Factory.Adapters.Count() == 0)
                {
                    MessageBox.Show("Your system have no video adapters. Try to install video adapter.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new Exception("There are no valid video adapters in system.");
                }

                var adapter = Factory.GetAdapter(0);
                if (adapter == null)
                {
                    MessageBox.Show("DirectX wasn't able to acquire video adapter. Try to restart your system.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new Exception("DirectX wasn't able to acquire video adapter.");
                }

                if (adapter.Outputs == null || adapter.Outputs.Count() == 0)
                {
                    MessageBox.Show("There are no video displays connected to your system. Try to connect a display.", "DirectX error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new Exception("No connected displays found.");
                }

                Device = new Device(adapter, DebugFlags | DeviceCreationFlags.SingleThreaded, FeatureLevel.Level_10_0);
            }
            catch (Exception exc)
            {
                switch((uint)exc.HResult)
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
            using (InfoQueue DeviceInfoQueue = Device.QueryInterface<InfoQueue>())
            {
                DeviceInfoQueue.SetBreakOnSeverity(MessageSeverity.Warning, true);
            }
#endif

            try
            {
                Context = Device.ImmediateContext;
                /*TestShader = new Dx11PipelineState(this, "TestShader", new InputElement[]
                {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 0, 1, InputClassification.PerVertexData, 0)
                });*/
                TextShader = new Dx11PipelineState(this, "TextShader", new InputElement[]
                {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("UVW", 0, Format.R32G32_UInt, 0, 1, InputClassification.PerVertexData, 0)
                });
                SpriteShader = new Dx11PipelineState(this, "SpriteShader", new InputElement[]
                {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("UVW", 0, Format.R32G32_UInt, 0, 1, InputClassification.PerVertexData, 0)
                });
                RoomShader = new Dx11PipelineState(this, "RoomShader", new InputElement[]
                {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 0, 1, InputClassification.PerVertexData, 0),
                new InputElement("OVERLAY", 0, Format.R8G8B8A8_UNorm, 0, 2, InputClassification.PerVertexData, 0),
                new InputElement("UVWANDBLENDMODE", 0, Format.R32G32_UInt, 0, 3, InputClassification.PerVertexData, 0),
                new InputElement("EDITORUVANDSECTORTEXTURE", 0, Format.R32_UInt, 0, 4, InputClassification.PerVertexData, 0)
                });
                RasterizerBackCulling = new RasterizerState(Device, new RasterizerStateDescription
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
                });
                SamplerDefault = new SamplerState(Device, new SamplerStateDescription
                {
                    AddressU = TextureAddressMode.Mirror,
                    AddressV = TextureAddressMode.Mirror,
                    AddressW = TextureAddressMode.Wrap,
                    Filter = Filter.Anisotropic,
                    MaximumAnisotropy = 4,
                });
                SamplerRoundToNearest = new SamplerState(Device, new SamplerStateDescription
                {
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    Filter = Filter.MinMagMipPoint,
                    MaximumAnisotropy = 4,
                });
                {
                    DepthStencilStateDescription desc = DepthStencilStateDescription.Default();
                    desc.DepthComparison = Comparison.LessEqual;
                    desc.DepthWriteMask = DepthWriteMask.All;
                    desc.IsDepthEnabled = true;
                    desc.IsStencilEnabled = false;
                    DepthStencilDefault = new DepthStencilState(Device, desc);
                }
                {
                    DepthStencilStateDescription desc = DepthStencilStateDescription.Default();
                    desc.DepthComparison = Comparison.Always;
                    desc.DepthWriteMask = DepthWriteMask.Zero;
                    desc.IsDepthEnabled = false;
                    desc.IsStencilEnabled = false;
                    DepthStencilNoZBuffer = new DepthStencilState(Device, desc);
                }
                BlendingDisabled = new BlendState(Device, BlendStateDescription.Default());
                {
                    BlendStateDescription desc = BlendStateDescription.Default();
                    desc.RenderTarget[0].IsBlendEnabled = true;
                    desc.RenderTarget[0].SourceBlend = desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                    desc.RenderTarget[0].DestinationBlend = desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.InverseSourceAlpha;
                    desc.RenderTarget[0].BlendOperation = desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                    desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                    BlendingPremultipliedAlpha = new BlendState(Device, desc);
                }
            }
            catch (Exception exc)
            {
                throw new Exception("Can't assign needed Direct3D parameters! Exception: " + exc);
            }

            // Sector textures
            bool support16BitTexture = Device.CheckFormatSupport(Format.B5G5R5A1_UNorm).HasFlag(FormatSupport.Texture2D); // For some reason not all DirectX devices support 16 bit textures.
            string[] sectorTextureNames = Enum.GetNames(typeof(SectorTexture)).Skip(1).ToArray();
            GCHandle[] handles = new GCHandle[sectorTextureNames.Length];
            try
            {
                DataBox[] dataBoxes = new DataBox[sectorTextureNames.Length];
                for (int i = 0; i < sectorTextureNames.Length; ++i)
                {
                    string name = nameof(TombLib) + "." + nameof(Rendering) + ".SectorTextures." + sectorTextureNames[i] + ".png";
                    using (Stream stream = ThisAssembly.GetManifestResourceStream(name))
                    {
                        ImageC image = ImageC.FromStream(stream);
                        if ((image.Width != SectorTextureSize) || (image.Height != SectorTextureSize))
                            throw new ArgumentOutOfRangeException("The embedded resource '" + name + "' is not of a valid size.");

                        if (support16BitTexture)
                        { // Compress image data into B5G5R5A1 format to save a bit of GPU memory. (3 MB saved with currently 23 images)
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
                            dataBoxes[i] = new DataBox(handles[i].AddrOfPinnedObject(), sizeof(ushort) * SectorTextureSize, 0);
                        }
                        else
                        {
                            handles[i] = GCHandle.Alloc(image.ToByteArray(), GCHandleType.Pinned);
                            dataBoxes[i] = new DataBox(handles[i].AddrOfPinnedObject(), sizeof(uint) * SectorTextureSize, 0);
                        }
                    }
                }

                SectorTextureArray = new Texture2D(Device, new Texture2DDescription
                {
                    Width = SectorTextureSize,
                    Height = SectorTextureSize,
                    MipLevels = 1,
                    ArraySize = sectorTextureNames.Length,
                    Format = support16BitTexture ? Format.B5G5R5A1_UNorm : Format.B8G8R8A8_UNorm,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Immutable,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                }, dataBoxes);
            }
            finally
            {
                foreach (GCHandle handle in handles)
                    handle.Free();
            }
            SectorTextureArrayView = new ShaderResourceView(Device, SectorTextureArray);

            // Set omni present state
            ResetState();

            logger.Info("Dx11 rendering device created.");
        }

        public void ResetState()
        {
            Context.Rasterizer.State = RasterizerBackCulling;
            Context.OutputMerger.SetDepthStencilState(DepthStencilDefault);
            Context.OutputMerger.SetBlendState(BlendingPremultipliedAlpha);
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

        ///<summary>Works even on immutable buffers</summary>
        public byte[] ReadBuffer(Buffer buffer, int size)
        {
            using (Buffer tempBuffer = new Buffer(Device,
                new BufferDescription(size, ResourceUsage.Staging, BindFlags.None,
                CpuAccessFlags.Read, ResourceOptionFlags.None, 0)))
            {
                Context.CopyResource(buffer, tempBuffer);
                DataBox mappedBuffer = Context.MapSubresource(tempBuffer, 0, MapMode.Read, MapFlags.None);
                try
                {
                    byte[] result = new byte[size];
                    Marshal.Copy(mappedBuffer.DataPointer, result, 0, size);
                    return result;
                }
                finally
                {
                    Context.UnmapSubresource(tempBuffer, 0);
                }
            }
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

    public static class Dx11RenderingDeviceDebugging
    {
        public static unsafe void SetDebugName(this DeviceChild child, string debugName)
        {
            byte[] debugNameBytes = Encoding.ASCII.GetBytes(debugName);
            fixed (byte* debugNameBytesPtr = debugNameBytes)
                child.SetPrivateData(CommonGuid.DebugObjectName, debugNameBytes.Length, new IntPtr(debugNameBytesPtr));
        }
    }
}
