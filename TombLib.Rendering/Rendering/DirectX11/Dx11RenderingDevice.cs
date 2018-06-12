using NLog;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;
using Buffer = SharpDX.Direct3D11.Buffer;
using Format = SharpDX.DXGI.Format;
using SampleDescription = SharpDX.DXGI.SampleDescription;
using Vector2 = System.Numerics.Vector2;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11RenderingDevice : RenderingDevice
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const int SectorTextureSize = 256;
        private static Assembly ThisAssembly = Assembly.GetExecutingAssembly();
        public readonly Device Device;
        public readonly DeviceContext Context;
        public readonly Dx11PipelineState TestShader;
        public readonly Dx11PipelineState TextShader;
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

        public unsafe Dx11RenderingDevice()
        {
            logger.Info("Dx11 rendering device creating.");
#if DEBUG
            const DeviceCreationFlags DebugFlags = DeviceCreationFlags.Debug;
#else
            const DeviceCreationFlags DebugFlags = DeviceCreationFlags.None;
#endif

            Device = new Device(DriverType.Hardware, DebugFlags | DeviceCreationFlags.SingleThreaded, FeatureLevel.Level_10_0);
            Context = Device.ImmediateContext;
            TestShader = new Dx11PipelineState(this, "TestShader", new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 0, 1, InputClassification.PerVertexData, 0)
            });
            TextShader = new Dx11PipelineState(this, "TextShader", new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("UVW", 0, Format.R32G32_UInt, 0, 1, InputClassification.PerVertexData, 0)
            });
            RoomShader = new Dx11PipelineState(this, "RoomShader", new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 0, 1, InputClassification.PerVertexData, 0),
                new InputElement("UVWANDBLENDMODE", 0, Format.R32G32_UInt, 0, 2, InputClassification.PerVertexData, 0),
                new InputElement("EDITORUVANDSECTORTEXTURE", 0, Format.R32_UInt, 0, 3, InputClassification.PerVertexData, 0)
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
                //desc.AlphaToCoverageEnable = true;
                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].SourceBlend = desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationBlend = desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.InverseSourceAlpha;
                desc.RenderTarget[0].BlendOperation = desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                BlendingPremultipliedAlpha = new BlendState(Device, desc);
            }

            // Sector textures
            string[] sectorTextureNames = Enum.GetNames(typeof(SectorTexture)).Skip(1).ToArray();
            ushort[,] sectorTextureData = new ushort[sectorTextureNames.Length, SectorTextureSize * SectorTextureSize];
            for (int i = 0; i < sectorTextureNames.Length; ++i)
            {
                string name = nameof(TombLib) + "." + nameof(Rendering) + ".SectorTextures." + sectorTextureNames[i] + ".png";
                using (Stream stream = ThisAssembly.GetManifestResourceStream(name))
                {
                    ImageC image = ImageC.FromStream(stream);
                    if ((image.Width != SectorTextureSize) || (image.Height != SectorTextureSize))
                        throw new ArgumentOutOfRangeException("The embedded resource '" + name + "' is not of a valid size.");

                    // Compress image data into B5G5R5A1 format to save a bit of GPU memory. (3 MB saved with currently 23 images)
                    for (int j = 0; j < (SectorTextureSize * SectorTextureSize); ++j)
                    {
                        ColorC Color = image.Get(j);
                        sectorTextureData[i, j] = (ushort)(
                            ((Color.B >> 3) << 0) |
                            ((Color.G >> 3) << 5) |
                            ((Color.R >> 3) << 10) |
                            ((Color.A >> 7) << 15));
                    }
                }
            }
            fixed (ushort* sectorTextureDataPtr = sectorTextureData)
            {
                DataBox[] dataBoxes = new DataBox[sectorTextureNames.Length];
                for (int i = 0; i < sectorTextureNames.Length; ++i)
                    dataBoxes[i] = new DataBox(
                        new IntPtr(sectorTextureDataPtr + SectorTextureSize * SectorTextureSize * i),
                        sizeof(ushort) * SectorTextureSize,
                        sizeof(ushort) * SectorTextureSize * SectorTextureSize);
                SectorTextureArray = new Texture2D(Device, new Texture2DDescription
                {
                    Width = SectorTextureSize,
                    Height = SectorTextureSize,
                    MipLevels = 1,
                    ArraySize = sectorTextureNames.Length,
                    Format = Format.B5G5R5A1_UNorm,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Immutable,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                }, dataBoxes);
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
            }
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
}
