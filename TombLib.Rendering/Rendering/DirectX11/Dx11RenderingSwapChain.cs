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
        public readonly Dx11RenderingDevice Device;
        public readonly Factory Factory;
        public readonly SwapChain SwapChain;
        public Texture2D BackBuffer;
        public RenderTargetView BackBufferView;
        public Texture2D DepthBuffer;
        public DepthStencilView DepthBufferView;

        public static readonly SampleDescription SampleDescription = new SampleDescription(1, 0);
        public static readonly Rational RefreshRate = new Rational(60, 1);
        public static readonly Format Format = Format.R8G8B8A8_UNorm;
        public static readonly Format DepthFormat = Format.D32_Float;
        public const int BufferCount = 2;

        public Dx11RenderingSwapChain(Dx11RenderingDevice device, Description description)
        {
            Device = device;
            Size = description.Size;
            Factory = new Factory1();
            SwapChain = new SwapChain(Factory, device.Device,
                new SwapChainDescription
                {
                    BufferCount = BufferCount,
                    ModeDescription = new ModeDescription(Size.X, Size.Y, RefreshRate, Format),
                    IsWindowed = true,
                    OutputHandle = description.WindowHandle,
                    SampleDescription = SampleDescription,
                    SwapEffect = SwapEffect.Sequential,
                    Usage = Usage.RenderTargetOutput
                });
            Factory.MakeWindowAssociation(description.WindowHandle, WindowAssociationFlags.IgnoreAll);
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
                SampleDescription = SampleDescription,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });
            DepthBufferView = new DepthStencilView(Device.Device, DepthBuffer);
        }

        public override void Dispose()
        {
            BackBufferView.Dispose();
            BackBuffer.Dispose();
            DepthBufferView.Dispose();
            DepthBuffer.Dispose();
            SwapChain.Dispose();
            Factory.Dispose();
        }

        public void Bind()
        {
            if (Device.CurrentRenderTarget == this)
                return;
            BindForce();
        }

        public void BindForce()
        {
            Device.Context.Rasterizer.SetViewport(0, 0, Size.X, Size.Y, 0.0f, 1.0f);
            Device.Context.OutputMerger.SetTargets(DepthBufferView, BackBufferView);
            Device.CurrentRenderTarget = this;
        }

        public override void Clear(Vector4 color)
        {
            Device.Context.ClearDepthStencilView(DepthBufferView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            Device.Context.ClearRenderTargetView(BackBufferView, new SharpDX.Color4(color.X, color.Y, color.Z, color.W));
        }

        public override void ClearDepth()
        {
            Device.Context.ClearDepthStencilView(DepthBufferView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
        }

        public override void Present()
        {
            SwapChain.Present(0, PresentFlags.None);
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

        public override unsafe void RenderSprites(RenderingTextureAllocator textureAllocator, bool linearFilter, params Sprite[] sprites)
        {
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            // Build vertex buffer
            int vertexCount = sprites.Length * 6;
            int bufferSize = vertexCount * (sizeof(Vector2) + sizeof(ulong));
            fixed (byte* data = new byte[bufferSize])
            {
                Vector2* positions = (Vector2*)(data);
                ulong* uvws = (ulong*)(data + vertexCount * sizeof(Vector2));

                // Setup vertices
                int count = sprites.Length;
                for (int i = 0; i < count; ++i)
                {
                    Sprite sprite = sprites[i];
                    VectorInt3 texPos = textureAllocator.Get(sprite.Texture);
                    VectorInt2 texSize = sprite.Texture.To - sprite.Texture.From;

                    positions[i * 6 + 0] = sprite.Pos00;
                    positions[i * 6 + 2] = positions[i * 6 + 3] = sprite.Pos10;
                    positions[i * 6 + 1] = positions[i * 6 + 4] = sprite.Pos01;
                    positions[i * 6 + 5] = sprite.Pos11;
                    uvws[i * 6 + 0] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(0.5f, 0.5f));
                    uvws[i * 6 + 2] = uvws[i * 6 + 3] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(texSize.X - 0.5f, 0.5f));
                    uvws[i * 6 + 1] = uvws[i * 6 + 4] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(0.5f, texSize.Y - 0.5f));
                    uvws[i * 6 + 5] = Dx11RenderingDevice.CompressUvw(texPos, textureScaling, new Vector2(texSize.X - 0.5f, texSize.Y - 0.5f));
                }

                // Create GPU resources
                using (var VertexBuffer = new Buffer(Device.Device, new IntPtr(data),
                    new BufferDescription(bufferSize, ResourceUsage.Immutable, BindFlags.VertexBuffer,
                    CpuAccessFlags.None, ResourceOptionFlags.None, 0)))
                {
                    var VertexBufferBindings = new VertexBufferBinding[] {
                        new VertexBufferBinding(VertexBuffer, sizeof(Vector2), (int)((byte*)positions - data)),
                        new VertexBufferBinding(VertexBuffer, sizeof(ulong), (int)((byte*)uvws - data)) };

                    // Render
                    Bind();
                    Device.SpriteShader.Apply(Device.Context);
                    Device.Context.PixelShader.SetSampler(0, linearFilter ? Device.SamplerDefault : Device.SamplerRoundToNearest);
                    Device.Context.PixelShader.SetShaderResources(0, ((Dx11RenderingTextureAllocator)(textureAllocator)).TextureView);
                    Device.Context.InputAssembler.SetVertexBuffers(0, VertexBufferBindings);
                    Device.Context.OutputMerger.SetDepthStencilState(Device.DepthStencilNoZBuffer);

                    // Render
                    Device.Context.Draw(vertexCount, 0);

                    // Reset state
                    Device.Context.OutputMerger.SetDepthStencilState(Device.DepthStencilDefault);
                }
            }
        }

        public override unsafe void RenderGlyphs(RenderingTextureAllocator textureAllocator, List<RenderingFont.GlyphRenderInfo> glyphRenderInfos)
        {
            Vector2 posScaling = new Vector2(1.0f) / (Size / 2); // Divide the integer coordinates to avoid pixel mishmash.
            Vector2 posOffset = VectorInt2.FromRounded(posScaling * 0.5f);
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            // Build vertex buffer
            int vertexCount = glyphRenderInfos.Count * 6;
            int bufferSize = vertexCount * (sizeof(Vector2) + sizeof(ulong));
            fixed (byte* data = new byte[bufferSize])
            {
                Vector2* positions = (Vector2*)(data);
                ulong* uvws = (ulong*)(data + vertexCount * sizeof(Vector2));

                // Setup vertices
                int count = glyphRenderInfos.Count;
                for (int i = 0; i < count; ++i)
                {
                    RenderingFont.GlyphRenderInfo info = glyphRenderInfos[i];
                    Vector2 posStart = info.PosStart * posScaling + posOffset;
                    Vector2 posEnd = (info.PosEnd - new Vector2(1)) * posScaling + posOffset;

                    positions[i * 6 + 0] = new Vector2(posStart.X, posStart.Y);
                    positions[i * 6 + 2] = positions[i * 6 + 3] = new Vector2(posEnd.X, posStart.Y);
                    positions[i * 6 + 1] = positions[i * 6 + 4] = new Vector2(posStart.X, posEnd.Y);
                    positions[i * 6 + 5] = new Vector2(posEnd.X, posEnd.Y);

                    uvws[i * 6 + 0] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, new Vector2(0.5f, 0.5f));
                    uvws[i * 6 + 2] = uvws[i * 6 + 3] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, new Vector2(info.TexSize.X - 0.5f, 0.5f));
                    uvws[i * 6 + 1] = uvws[i * 6 + 4] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, new Vector2(0.5f, info.TexSize.Y - 0.5f));
                    uvws[i * 6 + 5] = Dx11RenderingDevice.CompressUvw(info.TexStart, textureScaling, new Vector2(info.TexSize.X - 0.5f, info.TexSize.Y - 0.5f));
                }

                // Create GPU resources
                using (var VertexBuffer = new Buffer(Device.Device, new IntPtr(data),
                    new BufferDescription(bufferSize, ResourceUsage.Immutable, BindFlags.VertexBuffer,
                    CpuAccessFlags.None, ResourceOptionFlags.None, 0)))
                {
                    var VertexBufferBindings = new VertexBufferBinding[] {
                        new VertexBufferBinding(VertexBuffer, sizeof(Vector2), (int)((byte*)positions - data)),
                        new VertexBufferBinding(VertexBuffer, sizeof(ulong), (int)((byte*)uvws - data)) };

                    // Render
                    Bind();
                    Device.TextShader.Apply(Device.Context);
                    Device.Context.PixelShader.SetSampler(0, Device.SamplerDefault);
                    Device.Context.PixelShader.SetShaderResources(0, ((Dx11RenderingTextureAllocator)(textureAllocator)).TextureView);
                    Device.Context.InputAssembler.SetVertexBuffers(0, VertexBufferBindings);
                    Device.Context.OutputMerger.SetDepthStencilState(Device.DepthStencilNoZBuffer);

                    // Render
                    Device.Context.Draw(vertexCount, 0);

                    // Reset state
                    Device.Context.OutputMerger.SetDepthStencilState(Device.DepthStencilDefault);
                }
            }
        }
    }
}
