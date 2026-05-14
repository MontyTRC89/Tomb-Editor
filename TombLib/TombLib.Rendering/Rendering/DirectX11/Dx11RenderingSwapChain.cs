using NLog;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;
using Format = Silk.NET.DXGI.Format;
using D3D11Usage = Silk.NET.Direct3D11.Usage;

namespace TombLib.Rendering.DirectX11
{
    public unsafe class Dx11RenderingSwapChain : RenderingSwapChain
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public readonly Dx11RenderingDevice Device;
        public readonly IDXGISwapChain* SwapChain;
        public ID3D11Texture2D* BackBuffer;
        public ID3D11RenderTargetView* BackBufferView;
        public ID3D11Texture2D* DepthBuffer;
        public ID3D11DepthStencilView* DepthBufferView;

        public static readonly Rational RefreshRate = new Rational(60, 1);
        public const Format ColorFormat = Format.FormatR8G8B8A8Unorm;
        public const Format DepthFormat = Format.FormatD32Float;
        public const int BufferCount = 2;

        public Dx11RenderingSwapChain(Dx11RenderingDevice device, Description description)
        {
            Device = device;
            Size = description.Size;
            RenderException = null;

            var swapChainDesc = new SwapChainDesc
            {
                BufferCount = BufferCount,
                BufferDesc = new ModeDesc
                {
                    Width = (uint)Size.X,
                    Height = (uint)Size.Y,
                    RefreshRate = RefreshRate,
                    Format = ColorFormat,
                },
                Windowed = 1, // true
                OutputWindow = description.WindowHandle,
                SampleDesc = new SampleDesc((uint)GetAntialiasQuality(description.Antialias ? 4 : 1), 0),
                SwapEffect = SwapEffect.Sequential,
                BufferUsage = 0x00000020, // DXGI_USAGE_RENDER_TARGET_OUTPUT
            };

            IDXGISwapChain* swapChain;
            SilkMarshal.ThrowHResult(
                device.Factory->CreateSwapChain(
                    (IUnknown*)device.Device,
                    &swapChainDesc,
                    &swapChain));
            SwapChain = swapChain;

            device.Factory->MakeWindowAssociation(description.WindowHandle, 1 | 2 | 4); // IgnoreAll
            CreateBuffersAndViews();
        }

        private void CreateBuffersAndViews()
        {
            // Get back buffer from swap chain.
            ID3D11Texture2D* backBuffer;
            Guid texGuid = typeof(ID3D11Texture2D).GUID;
            SilkMarshal.ThrowHResult(
                SwapChain->GetBuffer(0, &texGuid, (void**)&backBuffer));
            BackBuffer = backBuffer;

            // Create render target view for the back buffer.
            ID3D11RenderTargetView* rtv;
            SilkMarshal.ThrowHResult(
                Device.Device->CreateRenderTargetView((ID3D11Resource*)BackBuffer, null, &rtv));
            BackBufferView = rtv;

            // Get sample description from the swap chain for depth buffer matching.
            SwapChainDesc scDesc;
            SwapChain->GetDesc(&scDesc);

            // Create depth buffer.
            var depthDesc = new Texture2DDesc
            {
                Format = DepthFormat,
                ArraySize = 1,
                MipLevels = 1,
                Width = (uint)Size.X,
                Height = (uint)Size.Y,
                SampleDesc = new SampleDesc(scDesc.SampleDesc.Count, 0),
                Usage = D3D11Usage.Default,
                BindFlags = (uint)BindFlag.DepthStencil,
                CPUAccessFlags = 0,
                MiscFlags = 0,
            };
            ID3D11Texture2D* depthTex;
            SilkMarshal.ThrowHResult(Device.Device->CreateTexture2D(&depthDesc, null, &depthTex));
            DepthBuffer = depthTex;

            // Create depth stencil view.
            ID3D11DepthStencilView* dsv;
            SilkMarshal.ThrowHResult(
                Device.Device->CreateDepthStencilView((ID3D11Resource*)DepthBuffer, null, &dsv));
            DepthBufferView = dsv;
        }

        private int GetAntialiasQuality(int maxQuality)
        {
            int AntialiasQuality = maxQuality;
            while (AntialiasQuality > 1)
            {
                uint numQualityLevels;
                Device.Device->CheckMultisampleQualityLevels(ColorFormat, (uint)AntialiasQuality, &numQualityLevels);
                if (numQualityLevels != 0)
                    break;
                else
                    AntialiasQuality /= 2;
            }
            return AntialiasQuality;
        }

        public override void Dispose()
        {
            BackBufferView->Release();
            BackBuffer->Release();
            DepthBufferView->Release();
            DepthBuffer->Release();
            SwapChain->Release();
        }

        public void Bind()
        {
            if (Device.CurrentRenderTarget == this)
                return;
            BindForce();
        }

        public override void BindForce()
        {
            var viewport = new Viewport
            {
                TopLeftX = 0,
                TopLeftY = 0,
                Width = Size.X,
                Height = Size.Y,
                MinDepth = 0.0f,
                MaxDepth = 1.0f,
            };
            Device.Context->RSSetViewports(1, &viewport);

            var rtv = BackBufferView;
            Device.Context->OMSetRenderTargets(1, &rtv, DepthBufferView);
            Device.CurrentRenderTarget = this;
        }

        public override void Clear(Vector4 color)
        {
            // DepthFormat = D32_Float has no stencil component — passing the Stencil flag
            // is silently ignored by the runtime but generates D3D11 warnings. Depth-only.
            Device.Context->ClearDepthStencilView(DepthBufferView, (uint)ClearFlag.Depth, 1.0f, 0);

            float* clearColor = stackalloc float[4];
            clearColor[0] = color.X;
            clearColor[1] = color.Y;
            clearColor[2] = color.Z;
            clearColor[3] = color.W;
            Device.Context->ClearRenderTargetView(BackBufferView, clearColor);
        }

        public override void ClearDepth()
        {
            // DepthFormat = D32_Float has no stencil component — passing the Stencil flag
            // is silently ignored by the runtime but generates D3D11 warnings. Depth-only.
            Device.Context->ClearDepthStencilView(DepthBufferView, (uint)ClearFlag.Depth, 1.0f, 0);
        }

        public override void Present()
        {
            try
            {
                int hr = SwapChain->Present(0, 0);
                SilkMarshal.ThrowHResult(hr);
            }
            catch (Exception ex)
            {
                if (RenderException == null)
                {
                    string message = string.Empty;

                    switch (unchecked((uint)ex.HResult))
                    {
                        case 0x887A0005:
                            {
                                int removedReason = Device.Device->GetDeviceRemovedReason();
                                message = "Renderer unexpectedly stopped due to DXGI_ERROR_DEVICE_REMOVED exception. Error code: " + removedReason;
                            }
                            break;

                        case 0x887A0020:
                            {
                                int removedReason = Device.Device->GetDeviceRemovedReason();
                                message = "Rendering device was lost due to DXGI_ERROR_DRIVER_INTERNAL_ERROR exception. Error code: " + removedReason;
                            }
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
                Device.Context->OMSetRenderTargets(0, null, null);
            }

            Size = newSize;
            BackBufferView->Release();
            BackBuffer->Release();
            DepthBufferView->Release();
            DepthBuffer->Release();
            SwapChain->ResizeBuffers(BufferCount, (uint)newSize.X, (uint)newSize.Y, ColorFormat, 0);
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
        public override void RenderSprites(RenderingTextureAllocator textureAllocator, bool linearFilter, bool noZ, List<Sprite> sprites)
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
            ID3D11Buffer* vb = slice.Finish();
            int baseOffset = slice.Offset;
            var bindings = new Dx11VertexBufferBinding[] {
                new Dx11VertexBufferBinding(vb, sizeof(Vector3), baseOffset),
                new Dx11VertexBufferBinding(vb, sizeof(Vector4), baseOffset + posBytes),
                new Dx11VertexBufferBinding(vb, sizeof(ulong),   baseOffset + posBytes + colBytes) };

            Bind();
            Device.SpriteShader.Apply(Device.Context);

            var sampler = linearFilter ? Device.SamplerDefault : Device.SamplerRoundToNearest;
            Device.Context->PSSetSamplers(0, 1, &sampler);

            var texSrv = ((Dx11RenderingTextureAllocator)(textureAllocator)).TextureView;
            Device.Context->PSSetShaderResources(0, 1, &texSrv);

            Dx11RenderingDevice.SetVertexBuffers(Device.Context, 0, bindings);

            Device.Context->OMSetDepthStencilState(noZ ? Device.DepthStencilNoZBuffer : Device.DepthStencilDefault, 0);

            Device.Context->Draw((uint)vertexCount, 0);

            // Restore default depth-stencil so the next subsystem (room rendering, etc.)
            // does not inherit our noZ state.
            Device.Context->OMSetDepthStencilState(Device.DepthStencilDefault, 0);

            // Oversized fallback owns its buffer; release it. Ring-pool path returns
            // the shared buffer which must NOT be released.
            if (slice.Oversized != null) vb->Release();
        }

        // Renders text glyphs and (optionally) the rectangular text-background overlays
        // in a single draw. Both lists share the same vertex layout and are concatenated:
        // overlays first (no UV — sentinel UV signals "solid background" to the shader),
        // glyphs after (real atlas UVs).
        //
        // Vertex layout per quad (matches TextShader input layout):
        //   POSITION : float2   (NDC)
        //   UVW      : uint2    (packed atlas coords; UV=0,blendMode=1 means "no texture")
        public override void RenderGlyphs(RenderingTextureAllocator textureAllocator, List<RenderingFont.GlyphRenderInfo> glyphRenderInfos, List<RectangleInt2> overlays)
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

            ID3D11Buffer* vb = slice.Finish();
            int baseOffset = slice.Offset;
            var bindings = new Dx11VertexBufferBinding[] {
                new Dx11VertexBufferBinding(vb, sizeof(Vector2), baseOffset),
                new Dx11VertexBufferBinding(vb, sizeof(ulong),   baseOffset + posBytes) };

            Bind();
            Device.TextShader.Apply(Device.Context);

            var sampler = Device.SamplerRoundToNearest;
            Device.Context->PSSetSamplers(0, 1, &sampler);

            var texSrv = ((Dx11RenderingTextureAllocator)(textureAllocator)).TextureView;
            Device.Context->PSSetShaderResources(0, 1, &texSrv);

            Dx11RenderingDevice.SetVertexBuffers(Device.Context, 0, bindings);

            Device.Context->OMSetDepthStencilState(Device.DepthStencilNoZBuffer, 0);

            Device.Context->Draw((uint)vertexCount, 0);

            Device.Context->OMSetDepthStencilState(Device.DepthStencilDefault, 0);
            if (slice.Oversized != null) vb->Release();
        }
    }
}
