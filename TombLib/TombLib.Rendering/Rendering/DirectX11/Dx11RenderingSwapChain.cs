using NLog;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11RenderingSwapChain : RenderingSwapChain
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public readonly Dx11RenderingDevice Device;
        public readonly IDXGISwapChain SwapChain;
        public ID3D11Texture2D BackBuffer;
        public ID3D11RenderTargetView BackBufferView;
        public ID3D11Texture2D DepthBuffer;
        public ID3D11DepthStencilView DepthBufferView;

        public static readonly Rational RefreshRate = new Rational(60, 1);
        public static readonly Format Format = Format.R8G8B8A8_UNorm;
        public static readonly Format DepthFormat = Format.D32_Float;
        public const int BufferCount = 2;

        public Dx11RenderingSwapChain(Dx11RenderingDevice device, Description description)
        {
            Device = device;
            Size = description.Size;
            RenderException = null;
            SwapChain = device.Factory.CreateSwapChain(device.Device,
                new SwapChainDescription
                {
                    BufferCount = BufferCount,
                    BufferDescription = new ModeDescription((uint)Size.X, (uint)Size.Y, RefreshRate, Format),
                    Windowed = true,
                    OutputWindow = description.WindowHandle,
                    SampleDescription = new SampleDescription((uint)GetAntialiasQuality(description.Antialias ? 4 : 1), 0),
                    SwapEffect = SwapEffect.Sequential,
                    BufferUsage = Usage.RenderTargetOutput
                });
            device.Factory.MakeWindowAssociation(description.WindowHandle, WindowAssociationFlags.IgnoreAll);
            CreateBuffersAndViews();
        }

        private void CreateBuffersAndViews()
        {
            BackBuffer = SwapChain.GetBuffer<ID3D11Texture2D>(0);
            BackBufferView = Device.Device.CreateRenderTargetView(BackBuffer);

            DepthBuffer = Device.Device.CreateTexture2D(new Texture2DDescription
            {
                Format = DepthFormat,
                ArraySize = 1,
                MipLevels = 1,
                Width = (uint)Size.X,
                Height = (uint)Size.Y,
                SampleDescription = new SampleDescription(SwapChain.Description.SampleDescription.Count, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            });
            DepthBufferView = Device.Device.CreateDepthStencilView(DepthBuffer);
        }

        private int GetAntialiasQuality(int maxQuality)
        {
            int antialiasQuality = maxQuality;
            while (antialiasQuality > 1)
            {
                if (Device.Device.CheckMultisampleQualityLevels(Format.R8G8B8A8_UNorm, (uint)antialiasQuality) != 0)
                    break;
                else
                    antialiasQuality /= 2;
            }
            return antialiasQuality;
        }

        public override void Dispose()
        {
            BackBufferView.Dispose();
            BackBuffer.Dispose();
            DepthBufferView.Dispose();
            DepthBuffer.Dispose();
            SwapChain.Dispose();
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
            Device.Context.OMSetRenderTargets(BackBufferView, DepthBufferView);
            Device.CurrentRenderTarget = this;
        }

        public override void Clear(Vector4 color)
        {
            Device.Context.ClearDepthStencilView(DepthBufferView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            Device.Context.ClearRenderTargetView(BackBufferView, new Color4(color.X, color.Y, color.Z, color.W));
        }

        public override void ClearDepth()
        {
            Device.Context.ClearDepthStencilView(DepthBufferView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
        }

        public override void Present()
        {
            try
            {
                SwapChain.Present(0, PresentFlags.None);
            }
            catch (Exception ex)
            {
                if (RenderException == null)
                {
                    string message = string.Empty;

                    switch (unchecked((uint)ex.HResult))
                    {
                        case 0x887A0005:
                            message = "Renderer unexpectedly stopped due to DXGI_ERROR_DEVICE_REMOVED exception. Error code: " + Device.Device.DeviceRemovedReason;
                            break;

                        case 0x887A0020:
                            message = "Rendering device was lost due to DXGI_ERROR_DRIVER_INTERNAL_ERROR exception. Error code: " + Device.Device.DeviceRemovedReason;
                            break;

                        case 0x887A0006:
                            message = "Rendering device stopped responding due to DXGI_ERROR_DEVICE_HUNG exception.";
                            break;

                        case 0x887A0007:
                            message = "Rendering device was lost due to DXGI_ERROR_DEVICE_RESET exception.";
                            break;

                        case 0x887A0001:
                            message = "Rendering device received invalid call. Possibly one of the shaders was badly modified.";
                            break;

                        default:
                            message = "Unknown error was encountered while rendering.";
                            break;
                    }

                    message += "\n" + "Additional message: " + ex.Message;
                    logger.Error(message);
                    RenderException = ex;
                }
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
            BackBufferView.Dispose();
            BackBuffer.Dispose();
            DepthBufferView.Dispose();
            DepthBuffer.Dispose();
            SwapChain.ResizeBuffers(BufferCount, (uint)newSize.X, (uint)newSize.Y, Format, SwapChainFlags.None);
            CreateBuffersAndViews();
        }

        public override unsafe void RenderSprites(RenderingTextureAllocator textureAllocator, bool linearFilter, bool noZ, List<Sprite> sprites)
        {
            if (sprites.Count == 0)
                return;

            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            // Build vertex buffer
            int vertexCount = sprites.Count * 6;
            int bufferSize = vertexCount * (sizeof(Vector3) + sizeof(Vector4) + sizeof(ulong));
            fixed (byte* data = new byte[bufferSize])
            {
                Vector3* positions = (Vector3*)(data);
                Vector4* colours   = (Vector4*)(data + vertexCount * sizeof(Vector3));
                ulong*   uvws      = (ulong*)  (data + vertexCount * (sizeof(Vector3) + sizeof(Vector4)));

                // Setup vertices
                int count = sprites.Count;
                for (int i = 0; i < count; ++i)
                {
                    Sprite sprite = sprites[i];
                    VectorInt3 texPos = textureAllocator.Get(sprite.Texture);
                    VectorInt2 texSize = sprite.Texture.To - sprite.Texture.From;
                    float depth = sprite.Depth.HasValue ? sprite.Depth.Value : 1.0f;

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

                // Create GPU resources
                int posOffset = (int)((byte*)positions - data);
                int colOffset = (int)((byte*)colours - data);
                int uvwOffset = (int)((byte*)uvws - data);

                using (var vertexBuffer = Device.Device.CreateBuffer(
                    new BufferDescription((uint)bufferSize, BindFlags.VertexBuffer, ResourceUsage.Immutable),
                    new SubresourceData(new IntPtr(data))))
                {
                    // Render
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

                    // Render
                    Device.Context.Draw((uint)vertexCount, 0);

                    // Reset state
                    Device.Context.OMSetDepthStencilState(Device.DepthStencilDefault);
                }
            }
        }

        public override unsafe void RenderGlyphs(RenderingTextureAllocator textureAllocator, List<RenderingFont.GlyphRenderInfo> glyphRenderInfos, List<RectangleInt2> overlays)
        {
            Vector2 posScaling = new Vector2(1.0f) / (Size / 2);
            Vector2 posOffset = VectorInt2.FromRounded(posScaling * 0.5f);
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            // Build vertex buffer
            int vertexCount = glyphRenderInfos.Count * 6 + overlays.Count * 6;
            int bufferSize = vertexCount * (sizeof(Vector2) + sizeof(ulong));
            fixed (byte* data = new byte[bufferSize])
            {
                Vector2* positions = (Vector2*)(data);
                ulong* uvws = (ulong*)(data + vertexCount * sizeof(Vector2));

                // Setup vertices
                int c = 0;
                for (int i = 0; i < overlays.Count; ++i, ++c)
                {
                    var overlay = overlays[i];
                    Vector2 posStart = overlay.Start * posScaling + posOffset;
                    Vector2 posEnd = (overlay.End + new Vector2(1)) * posScaling + posOffset;

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
                    RenderingFont.GlyphRenderInfo info = glyphRenderInfos[i];
                    Vector2 posStart = info.PosStart * posScaling + posOffset;
                    Vector2 posEnd = (info.PosEnd - new Vector2(1)) * posScaling + posOffset;

                    positions[c * 6 + 0] = new Vector2(posStart.X, posStart.Y);
                    positions[c * 6 + 2] = positions[c * 6 + 3] = new Vector2(posEnd.X, posStart.Y);
                    positions[c * 6 + 1] = positions[c * 6 + 4] = new Vector2(posStart.X, posEnd.Y);
                    positions[c * 6 + 5] = new Vector2(posEnd.X, posEnd.Y);

                    uvws[c * 6 + 0] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, Vector2.Zero);
                    uvws[c * 6 + 2] = uvws[c * 6 + 3] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, new Vector2(info.TexSize.X - 1, 0));
                    uvws[c * 6 + 1] = uvws[c * 6 + 4] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, new Vector2(0, info.TexSize.Y - 1));
                    uvws[c * 6 + 5] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, new Vector2(info.TexSize.X - 1, info.TexSize.Y - 1));
                }

                // Create GPU resources
                int positionsOffset = (int)((byte*)positions - data);
                int uvwsOffset = (int)((byte*)uvws - data);

                using (var vertexBuffer = Device.Device.CreateBuffer(
                    new BufferDescription((uint)bufferSize, BindFlags.VertexBuffer, ResourceUsage.Immutable),
                    new SubresourceData(new IntPtr(data))))
                {
                    // Render
                    Bind();
                    Device.TextShader.Apply(Device.Context);
                    Device.Context.PSSetSampler(0, Device.SamplerRoundToNearest);
                    Device.Context.PSSetShaderResource(0, ((Dx11RenderingTextureAllocator)(textureAllocator)).TextureView);
                    Device.Context.IASetVertexBuffers(0,
                        new ID3D11Buffer[] { vertexBuffer, vertexBuffer },
                        new uint[] { (uint)sizeof(Vector2), (uint)sizeof(ulong) },
                        new uint[] { (uint)positionsOffset, (uint)uvwsOffset });
                    Device.Context.OMSetDepthStencilState(Device.DepthStencilNoZBuffer);

                    // Render
                    Device.Context.Draw((uint)vertexCount, 0);

                    // Reset state
                    Device.Context.OMSetDepthStencilState(Device.DepthStencilDefault);
                }
            }
        }
    }
}
