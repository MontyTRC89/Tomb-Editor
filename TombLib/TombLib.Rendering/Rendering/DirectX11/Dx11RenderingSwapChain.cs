using NLog;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11RenderingSwapChain : RenderingSwapChain
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public readonly Dx11RenderingDevice Device;
        public readonly SwapChain SwapChain;
        public Texture2D BackBuffer;
        public RenderTargetView BackBufferView;
        public Texture2D DepthBuffer;
        public DepthStencilView DepthBufferView;

        public static readonly Rational RefreshRate = new Rational(60, 1);
        public static readonly Format Format = Format.R8G8B8A8_UNorm;
        public static readonly Format DepthFormat = Format.D32_Float;
        public const int BufferCount = 2;

        public Dx11RenderingSwapChain(Dx11RenderingDevice device, Description description)
        {
            Device = device;
            Size = description.Size;
            RenderException = null;
            SwapChain = new SwapChain(device.Factory, device.Device,
                new SwapChainDescription
                {
                    BufferCount = BufferCount,
                    ModeDescription = new ModeDescription(Size.X, Size.Y, RefreshRate, Format),
                    IsWindowed = true,
                    OutputHandle = description.WindowHandle,
                    SampleDescription = new SampleDescription(GetAntialiasQuality(description.Antialias ? 4 : 1), 0),
                    SwapEffect = SwapEffect.Sequential,
                    Usage = Usage.RenderTargetOutput
                });
            device.Factory.MakeWindowAssociation(description.WindowHandle, WindowAssociationFlags.IgnoreAll);
            CreateBuffersAndViews();
        }

        private void CreateBuffersAndViews()
        {
            BackBuffer = SharpDX.Direct3D11.Resource.FromSwapChain<Texture2D>(SwapChain, 0);
            BackBufferView = new RenderTargetView(Device.Device, BackBuffer);

            DepthBuffer = new Texture2D(Device.Device, new Texture2DDescription
            {
                Format = DepthFormat,
                ArraySize = 1,
                MipLevels = 1,
                Width = Size.X,
                Height = Size.Y,
                SampleDescription = new SampleDescription(SwapChain.Description.SampleDescription.Count, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });
            DepthBufferView = new DepthStencilView(Device.Device, DepthBuffer);
        }

        private int GetAntialiasQuality(int maxQuality)
        {
            int AntialiasQuality = maxQuality;
            while (AntialiasQuality > 1)
            {
                if (Device.Device.CheckMultisampleQualityLevels(Format.R8G8B8A8_UNorm, AntialiasQuality) != 0)
                    break;
                else
                    AntialiasQuality /= 2;
            }
            return AntialiasQuality;
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

        public override void BindForce()
        {
            Device.Context.Rasterizer.SetViewport(0, 0, Size.X, Size.Y, 0.0f, 1.0f);
            Device.Context.OutputMerger.SetTargets(DepthBufferView, BackBufferView);
            Device.CurrentRenderTarget = this;
        }

        public override void Clear(Vector4 color)
        {
            // DepthFormat = D32_Float has no stencil component — passing the Stencil flag
            // is silently ignored by the runtime but generates D3D11 warnings. Depth-only.
            Device.Context.ClearDepthStencilView(DepthBufferView, DepthStencilClearFlags.Depth, 1.0f, 0);
            Device.Context.ClearRenderTargetView(BackBufferView, new SharpDX.Color4(color.X, color.Y, color.Z, color.W));
        }

        public override void ClearDepth()
        {
            // DepthFormat = D32_Float has no stencil component — passing the Stencil flag
            // is silently ignored by the runtime but generates D3D11 warnings. Depth-only.
            Device.Context.ClearDepthStencilView(DepthBufferView, DepthStencilClearFlags.Depth, 1.0f, 0);
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
                            message = "Renderer unexpectedly stopped due to DXGI_ERROR_DEVICE_REMOVED exception. Error code: " + Device.Device.DeviceRemovedReason.Code;
                            break;

                        case 0x887A0020:
                            message = "Rendering device was lost due to DXGI_ERROR_DRIVER_INTERNAL_ERROR exception. Error code: " + Device.Device.DeviceRemovedReason.Code;
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
                Device.CurrentRenderTarget = null; // To reset the viewport dimensions
                Device.Context.OutputMerger.ResetTargets();
            }

            Size = newSize;
            BackBufferView.Dispose();
            BackBuffer.Dispose();
            DepthBufferView.Dispose();
            DepthBuffer.Dispose();
            SwapChain.ResizeBuffers(BufferCount, newSize.X, newSize.Y, Format, SwapChainFlags.None);
            CreateBuffersAndViews();
            // ResizeTarget is not the correct method!
        }

        // Renders an unindexed quad list (6 vertices per sprite) into the back buffer.
        // Uses the device-wide dynamic VB pool to avoid allocating + disposing a new
        // ID3D11Buffer per call. Three interleaved streams are written into one slice,
        // each addressed via its own VertexBufferBinding offset.
        //
        // Vertex layout per sprite (matches SpriteShader input layout):
        //   POSITION : float3   (xy = screen pos in NDC, z = depth or 1.0f if no Z)
        //   COLOR    : float4   (per-sprite tint, replicated to all 6 verts)
        //   UVW      : uint2    (packed atlas coords; see CompressUvw)
        //
        // Triangle order (i*6 .. i*6+5):  Pos00,Pos01,Pos10  +  Pos10,Pos01,Pos11
        public override unsafe void RenderSprites(RenderingTextureAllocator textureAllocator, bool linearFilter, bool noZ, List<Sprite> sprites)
        {
            if (sprites.Count == 0)
                return;

            // Reciprocal scale baked into the UV packing so the shader can recover
            // sub-pixel atlas coordinates from a uint pair (see CompressUvw doc).
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            int vertexCount = sprites.Count * 6;
            int posBytes = vertexCount * sizeof(Vector3);
            int colBytes = vertexCount * sizeof(Vector4);
            int uvwBytes = vertexCount * sizeof(ulong);
            int bufferSize = posBytes + colBytes + uvwBytes;

            var slice = Device.DynamicVertexBuffers.Allocate(bufferSize);
            byte* data = (byte*)slice.Data;
            Vector3* positions = (Vector3*)(data);
            Vector4* colours   = (Vector4*)(data + posBytes);
            ulong*   uvws      = (ulong*)  (data + posBytes + colBytes);

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
                // 0.5px insets prevent atlas-bleeding from neighbouring entries
                uvws[i * 6 + 1] = uvws[i * 6 + 4] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(0.5f, 0.5f));
                uvws[i * 6 + 5] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(texSize.X - 0.5f, 0.5f));
                uvws[i * 6 + 0] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(0.5f, texSize.Y - 0.5f));
                uvws[i * 6 + 2] = uvws[i * 6 + 3] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(texSize.X - 0.5f, texSize.Y - 0.5f));

                for (int j = 0; j < 6; j++)
                    colours[i * 6 + j] = sprite.Tint;
            }

            // Hand the slice back to the pool (Unmap) and bind it for the IA.
            Buffer vb = slice.Finish();
            int baseOffset = slice.Offset;
            var bindings = new VertexBufferBinding[] {
                new VertexBufferBinding(vb, sizeof(Vector3), baseOffset),
                new VertexBufferBinding(vb, sizeof(Vector4), baseOffset + posBytes),
                new VertexBufferBinding(vb, sizeof(ulong),   baseOffset + posBytes + colBytes) };

            Bind();
            Device.SpriteShader.Apply(Device.Context);
            Device.Context.PixelShader.SetSampler(0, linearFilter ? Device.SamplerDefault : Device.SamplerRoundToNearest);
            Device.Context.PixelShader.SetShaderResources(0, ((Dx11RenderingTextureAllocator)(textureAllocator)).TextureView);
            Device.Context.InputAssembler.SetVertexBuffers(0, bindings);
            Device.Context.OutputMerger.SetDepthStencilState(noZ ? Device.DepthStencilNoZBuffer : Device.DepthStencilDefault);

            Device.Context.Draw(vertexCount, 0);

            // Restore default depth-stencil so the next subsystem (room rendering, etc.)
            // does not inherit our noZ state.
            Device.Context.OutputMerger.SetDepthStencilState(Device.DepthStencilDefault);

            // Oversized fallback owns its buffer; release it. Ring-pool path returns
            // the shared buffer which must NOT be disposed.
            if (slice.Oversized != null) vb.Dispose();
        }

        // Renders text glyphs and (optionally) the rectangular text-background overlays
        // in a single draw. Both lists share the same vertex layout and are concatenated:
        // overlays first (no UV — sentinel UV signals "solid background" to the shader),
        // glyphs after (real atlas UVs).
        //
        // Vertex layout per quad (matches TextShader input layout):
        //   POSITION : float2   (NDC)
        //   UVW      : uint2    (packed atlas coords; UV=0,blendMode=1 means "no texture")
        public override unsafe void RenderGlyphs(RenderingTextureAllocator textureAllocator, List<RenderingFont.GlyphRenderInfo> glyphRenderInfos, List<RectangleInt2> overlays)
        {
            // Snap pixel positions to the actual half-pixel grid of the back buffer to
            // keep glyph edges crisp regardless of size.
            Vector2 posScaling = new Vector2(1.0f) / (Size / 2);
            Vector2 posOffset = VectorInt2.FromRounded(posScaling * 0.5f);
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            int vertexCount = glyphRenderInfos.Count * 6 + overlays.Count * 6;
            if (vertexCount == 0)
                return;

            int posBytes = vertexCount * sizeof(Vector2);
            int uvwBytes = vertexCount * sizeof(ulong);
            int bufferSize = posBytes + uvwBytes;

            var slice = Device.DynamicVertexBuffers.Allocate(bufferSize);
            byte* data = (byte*)slice.Data;
            Vector2* positions = (Vector2*)(data);
            ulong* uvws = (ulong*)(data + posBytes);

            int c = 0;

            // Background overlays: a single packed UVW value with the "solid color"
            // sentinel (highestBits = 1) is written to all 6 verts of the quad.
            ulong overlayUvw = Dx11RenderingDevice.CompressUvw(VectorInt3.Zero, Vector2.Zero, Vector2.Zero, 1);
            for (int i = 0; i < overlays.Count; ++i, ++c)
            {
                var overlay = overlays[i];
                Vector2 posStart = overlay.Start * posScaling + posOffset;
                Vector2 posEnd = (overlay.End + new Vector2(1)) * posScaling + posOffset;

                positions[c * 6 + 0] = new Vector2(posStart.X, posStart.Y);
                positions[c * 6 + 2] = positions[c * 6 + 3] = new Vector2(posEnd.X, posStart.Y);
                positions[c * 6 + 1] = positions[c * 6 + 4] = new Vector2(posStart.X, posEnd.Y);
                positions[c * 6 + 5] = new Vector2(posEnd.X, posEnd.Y);

                uvws[c * 6 + 0] = uvws[c * 6 + 1] = uvws[c * 6 + 2] =
                uvws[c * 6 + 3] = uvws[c * 6 + 4] = uvws[c * 6 + 5] = overlayUvw;
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

            Buffer vb = slice.Finish();
            int baseOffset = slice.Offset;
            var bindings = new VertexBufferBinding[] {
                new VertexBufferBinding(vb, sizeof(Vector2), baseOffset),
                new VertexBufferBinding(vb, sizeof(ulong),   baseOffset + posBytes) };

            Bind();
            Device.TextShader.Apply(Device.Context);
            Device.Context.PixelShader.SetSampler(0, Device.SamplerRoundToNearest);
            Device.Context.PixelShader.SetShaderResources(0, ((Dx11RenderingTextureAllocator)(textureAllocator)).TextureView);
            Device.Context.InputAssembler.SetVertexBuffers(0, bindings);
            Device.Context.OutputMerger.SetDepthStencilState(Device.DepthStencilNoZBuffer);

            Device.Context.Draw(vertexCount, 0);

            Device.Context.OutputMerger.SetDepthStencilState(Device.DepthStencilDefault);
            if (slice.Oversized != null) vb.Dispose();
        }
    }
}
