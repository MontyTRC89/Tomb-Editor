using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11RenderingSwapChain : RenderingSwapChain
    {
        public readonly Dx11RenderingDevice Device;
        public VectorInt2 Size;
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
            Device.Context.Rasterizer.SetViewport(0, 0, Size.X, Size.Y, 0.0f, 1.0f);
            Device.Context.OutputMerger.SetTargets(DepthBufferView, BackBufferView);
            Device.CurrentRenderTarget = this;
        }

        public override void Clear(Vector4 Color)
        {
            Device.Context.ClearDepthStencilView(DepthBufferView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            Device.Context.ClearRenderTargetView(BackBufferView, new SharpDX.Color4(Color.X, Color.Y, Color.Z, Color.W));
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
    }
}
