using NLog;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace TombLib.Rendering.DirectX11
{
    /// <summary>
    /// An offscreen rendering target that renders to a Texture2D instead of a DXGI SwapChain.
    /// Provides pixel readback for use with WPF WriteableBitmap.
    /// </summary>
    public class Dx11OffscreenSwapChain : RenderingSwapChain
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public readonly Dx11RenderingDevice Device;

        private ID3D11Texture2D _backBuffer;
        private ID3D11RenderTargetView _backBufferView;
        private ID3D11Texture2D _depthBuffer;
        private ID3D11DepthStencilView _depthBufferView;
        private ID3D11Texture2D _stagingBuffer;

        public static readonly Format Format = Format.B8G8R8A8_UNorm;
        public static readonly Format DepthFormat = Format.D32_Float;

        public Dx11OffscreenSwapChain(Dx11RenderingDevice device, VectorInt2 size)
        {
            Device = device;
            Size = size;
            RenderException = null;
            CreateBuffersAndViews();
        }

        private void CreateBuffersAndViews()
        {
            _backBuffer = Device.Device.CreateTexture2D(new Texture2DDescription
            {
                Format = Format,
                Width = (uint)Size.X,
                Height = (uint)Size.Y,
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            });
            _backBufferView = Device.Device.CreateRenderTargetView(_backBuffer);

            _depthBuffer = Device.Device.CreateTexture2D(new Texture2DDescription
            {
                Format = DepthFormat,
                Width = (uint)Size.X,
                Height = (uint)Size.Y,
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            });
            _depthBufferView = Device.Device.CreateDepthStencilView(_depthBuffer);

            _stagingBuffer = Device.Device.CreateTexture2D(new Texture2DDescription
            {
                Format = Format,
                Width = (uint)Size.X,
                Height = (uint)Size.Y,
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CPUAccessFlags = CpuAccessFlags.Read,
                MiscFlags = ResourceOptionFlags.None
            });
        }

        private void DisposeBuffersAndViews()
        {
            _backBufferView?.Dispose();
            _backBuffer?.Dispose();
            _depthBufferView?.Dispose();
            _depthBuffer?.Dispose();
            _stagingBuffer?.Dispose();
        }

        public override void Dispose()
        {
            DisposeBuffersAndViews();
        }

        public void Bind()
        {
            if (Device.CurrentRenderTarget == this)
                return;
            BindForce();
        }

        public void BindForce()
        {
            Device.Context.RSSetViewport(0, 0, Size.X, Size.Y, 0.0f, 1.0f);
            Device.Context.OMSetRenderTargets(_backBufferView, _depthBufferView);
            Device.CurrentRenderTarget = this;
        }

        public override void Clear(Vector4 color)
        {
            Device.Context.ClearDepthStencilView(_depthBufferView,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            Device.Context.ClearRenderTargetView(_backBufferView,
                new Color4(color.X, color.Y, color.Z, color.W));
        }

        public override void ClearDepth()
        {
            Device.Context.ClearDepthStencilView(_depthBufferView,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
        }

        public override void Present()
        {
            // No-op for offscreen target. Use ReadPixelsDirect() to get the rendered image.
        }

        /// <summary>
        /// Copies the rendered frame into the provided byte array (BGRA format).
        /// Returns the row pitch of the source data.
        /// </summary>
        public int ReadPixels(byte[] destination)
        {
            Device.Context.CopyResource(_stagingBuffer, _backBuffer);

            var mapped = Device.Context.Map(_stagingBuffer, 0, MapMode.Read);
            try
            {
                int bytesPerPixel = 4;
                int rowPitch = (int)mapped.RowPitch;
                int destRowSize = Size.X * bytesPerPixel;

                for (int y = 0; y < Size.Y; y++)
                    Marshal.Copy(mapped.DataPointer + y * rowPitch, destination, y * destRowSize, destRowSize);

                return rowPitch;
            }
            finally
            {
                Device.Context.Unmap(_stagingBuffer, 0);
            }
        }

        /// <summary>
        /// Copies the rendered frame directly into the provided IntPtr buffer (BGRA format).
        /// Suitable for writing directly into a WriteableBitmap back buffer.
        /// </summary>
        public unsafe void ReadPixelsDirect(IntPtr destination, int destinationStride)
        {
            Device.Context.CopyResource(_stagingBuffer, _backBuffer);

            var mapped = Device.Context.Map(_stagingBuffer, 0, MapMode.Read);
            try
            {
                int bytesPerPixel = 4;
                int copyWidth = Size.X * bytesPerPixel;
                byte* src = (byte*)mapped.DataPointer;
                byte* dst = (byte*)destination;

                for (int y = 0; y < Size.Y; y++)
                {
                    System.Buffer.MemoryCopy(
                        src + y * mapped.RowPitch,
                        dst + y * destinationStride,
                        destinationStride,
                        copyWidth);
                }
            }
            finally
            {
                Device.Context.Unmap(_stagingBuffer, 0);
            }
        }

        public override void Resize(VectorInt2 newSize)
        {
            if (newSize.X <= 0 || newSize.Y <= 0)
                return;

            if (Device.CurrentRenderTarget == this)
            {
                Device.CurrentRenderTarget = null;
                Device.Context.OMSetRenderTargets((ID3D11RenderTargetView)null, (ID3D11DepthStencilView)null);
            }

            Size = newSize;
            DisposeBuffersAndViews();
            CreateBuffersAndViews();
        }

        public override unsafe void RenderSprites(RenderingTextureAllocator textureAllocator,
            bool linearFilter, bool noZ, List<Sprite> sprites)
        {
            if (sprites.Count == 0)
                return;

            var textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            int vertexCount = sprites.Count * 6;
            int bufferSize = vertexCount * (sizeof(Vector3) + sizeof(Vector4) + sizeof(ulong));

            fixed (byte* data = new byte[bufferSize])
            {
                var positions = (Vector3*)(data);
                var colours = (Vector4*)(data + vertexCount * sizeof(Vector3));
                var uvws = (ulong*)(data + vertexCount * (sizeof(Vector3) + sizeof(Vector4)));

                int count = sprites.Count;
                for (int i = 0; i < count; ++i)
                {
                    var sprite = sprites[i];
                    var texPos = textureAllocator.Get(sprite.Texture);
                    float depth = sprite.Depth.HasValue ? sprite.Depth.Value : 1.0f;
                    var texSize = sprite.Texture.To - sprite.Texture.From;

                    positions[i * 6 + 0] = new Vector3(sprite.Pos00.X, sprite.Pos00.Y, depth);
                    positions[i * 6 + 2] = positions[i * 6 + 3] = new Vector3(sprite.Pos10.X, sprite.Pos10.Y, depth);
                    positions[i * 6 + 1] = positions[i * 6 + 4] = new Vector3(sprite.Pos01.X, sprite.Pos01.Y, depth);
                    positions[i * 6 + 5] = new Vector3(sprite.Pos11.X, sprite.Pos11.Y, depth);
                    uvws[i * 6 + 1] = uvws[i * 6 + 4] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(0.5f, 0.5f));
                    uvws[i * 6 + 5] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(texSize.X - 0.5f, 0.5f));
                    uvws[i * 6 + 0] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(0.5f, texSize.Y - 0.5f));
                    uvws[i * 6 + 2] = uvws[i * 6 + 3] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(texSize.X - 0.5f, texSize.Y - 0.5f));

                    for (int j = 0; j < 6; j++)
                        colours[i * 6 + j] = sprite.Tint;
                }

                int posOffset = (int)((byte*)positions - data);
                int colOffset = (int)((byte*)colours - data);
                int uvwOffset = (int)((byte*)uvws - data);

                using (var vertexBuffer = Device.Device.CreateBuffer(
                    new BufferDescription((uint)bufferSize, BindFlags.VertexBuffer, ResourceUsage.Immutable),
                    new SubresourceData(new IntPtr(data))))
                {
                    Bind();
                    Device.SpriteShader.Apply(Device.Context);
                    Device.Context.PSSetSampler(0, linearFilter ? Device.SamplerDefault : Device.SamplerRoundToNearest);
                    Device.Context.PSSetShaderResource(0, ((Dx11RenderingTextureAllocator)(textureAllocator)).TextureView);
                    Device.Context.IASetVertexBuffers(0,
                        new ID3D11Buffer[] { vertexBuffer, vertexBuffer, vertexBuffer },
                        new uint[] { (uint)sizeof(Vector3), (uint)sizeof(Vector4), (uint)sizeof(ulong) },
                        new uint[] { (uint)posOffset, (uint)colOffset, (uint)uvwOffset });

                    if (noZ)
                        Device.Context.OMSetDepthStencilState(Device.DepthStencilNoZBuffer);
                    else
                        Device.Context.OMSetDepthStencilState(Device.DepthStencilDefault);

                    Device.Context.Draw((uint)vertexCount, 0);
                    Device.Context.OMSetDepthStencilState(Device.DepthStencilDefault);
                }
            }
        }

        public override unsafe void RenderGlyphs(RenderingTextureAllocator textureAllocator,
            List<RenderingFont.GlyphRenderInfo> glyphRenderInfos, List<RectangleInt2> overlays)
        {
            var posScaling = new Vector2(1.0f) / (Size / 2);
            var posOffset = VectorInt2.FromRounded(posScaling * 0.5f);
            var textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            int vertexCount = glyphRenderInfos.Count * 6 + overlays.Count * 6;
            int bufferSize = vertexCount * (sizeof(Vector2) + sizeof(ulong));

            fixed (byte* data = new byte[bufferSize])
            {
                var positions = (Vector2*)(data);
                var uvws = (ulong*)(data + vertexCount * sizeof(Vector2));

                int c = 0;
                for (int i = 0; i < overlays.Count; ++i, ++c)
                {
                    var overlay = overlays[i];
                    var posStart = overlay.Start * posScaling + posOffset;
                    var posEnd = (overlay.End + new Vector2(1)) * posScaling + posOffset;

                    positions[c * 6 + 0] = new Vector2(posStart.X, posStart.Y);
                    positions[c * 6 + 2] = positions[c * 6 + 3] = new Vector2(posEnd.X, posStart.Y);
                    positions[c * 6 + 1] = positions[c * 6 + 4] = new Vector2(posStart.X, posEnd.Y);
                    positions[c * 6 + 5] = new Vector2(posEnd.X, posEnd.Y);

                    uvws[c * 6 + 2] = uvws[c * 6 + 3] =
                    uvws[c * 6 + 1] = uvws[c * 6 + 4] =
                    uvws[c * 6 + 5] = uvws[c * 6 + 0] = Dx11RenderingDevice.CompressUvw(VectorInt3.Zero, Vector2.Zero, Vector2.Zero, 1);
                }

                for (int i = 0; i < glyphRenderInfos.Count; ++i, ++c)
                {
                    var info = glyphRenderInfos[i];
                    var posStart = info.PosStart * posScaling + posOffset;
                    var posEnd = (info.PosEnd - new Vector2(1)) * posScaling + posOffset;

                    positions[c * 6 + 0] = new Vector2(posStart.X, posStart.Y);
                    positions[c * 6 + 2] = positions[c * 6 + 3] = new Vector2(posEnd.X, posStart.Y);
                    positions[c * 6 + 1] = positions[c * 6 + 4] = new Vector2(posStart.X, posEnd.Y);
                    positions[c * 6 + 5] = new Vector2(posEnd.X, posEnd.Y);

                    uvws[c * 6 + 0] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, Vector2.Zero);
                    uvws[c * 6 + 2] = uvws[c * 6 + 3] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, new Vector2(info.TexSize.X - 1, 0));
                    uvws[c * 6 + 1] = uvws[c * 6 + 4] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, new Vector2(0, info.TexSize.Y - 1));
                    uvws[c * 6 + 5] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, new Vector2(info.TexSize.X - 1, info.TexSize.Y - 1));
                }

                int positionsOffset = (int)((byte*)positions - data);
                int uvwsOffset = (int)((byte*)uvws - data);

                using (var vertexBuffer = Device.Device.CreateBuffer(
                    new BufferDescription((uint)bufferSize, BindFlags.VertexBuffer, ResourceUsage.Immutable),
                    new SubresourceData(new IntPtr(data))))
                {
                    Bind();
                    Device.TextShader.Apply(Device.Context);
                    Device.Context.PSSetSampler(0, Device.SamplerRoundToNearest);
                    Device.Context.PSSetShaderResource(0, ((Dx11RenderingTextureAllocator)(textureAllocator)).TextureView);
                    Device.Context.IASetVertexBuffers(0,
                        new ID3D11Buffer[] { vertexBuffer, vertexBuffer },
                        new uint[] { (uint)sizeof(Vector2), (uint)sizeof(ulong) },
                        new uint[] { (uint)positionsOffset, (uint)uvwsOffset });
                    Device.Context.OMSetDepthStencilState(Device.DepthStencilNoZBuffer);

                    Device.Context.Draw((uint)vertexCount, 0);
                    Device.Context.OMSetDepthStencilState(Device.DepthStencilDefault);
                }
            }
        }
    }
}
