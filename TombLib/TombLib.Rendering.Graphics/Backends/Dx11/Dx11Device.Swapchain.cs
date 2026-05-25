using System;
using Silk.NET.Core.Native;
using DX = Silk.NET.Direct3D11;
using DXGI = Silk.NET.DXGI;
using TombLib.Rendering.Graphics.Rhi;
// Selective import of Silk.NET extension methods on ComPtr<T>. The full
// namespace is not imported because it collides with Rhi descriptor types
// (Format, BufferDesc, SamplerDesc).
using static Silk.NET.Direct3D11.D3D11DeviceVtblExtensions;
using static Silk.NET.DXGI.DXGIFactoryVtblExtensions;
using static Silk.NET.DXGI.DXGISwapChainVtblExtensions;

namespace TombLib.Rendering.Graphics.Backends.Dx11;

// Swapchain management for the Direct3D 11 backend: the DXGI swapchain plus
// its backbuffer colour view and a matching depth texture / view.
public unsafe sealed partial class Dx11Device
{
    public SwapchainHandle CreateSwapchain(in SwapchainDesc desc)
    {
        var swapChainDesc = new DXGI.SwapChainDesc
        {
            BufferDesc = new DXGI.ModeDesc
            {
                Width            = (uint)desc.Width,
                Height           = (uint)desc.Height,
                Format           = Dx11Mapping.ToDxgi(desc.ColorFormat),
                RefreshRate      = new DXGI.Rational { Numerator = 0, Denominator = 1 },
                ScanlineOrdering = DXGI.ModeScanlineOrder.Unspecified,
                Scaling          = DXGI.ModeScaling.Unspecified,
            },
            SampleDesc   = new DXGI.SampleDesc { Count = (uint)desc.Samples, Quality = 0 },
            BufferUsage  = DXGI.DXGI.UsageRenderTargetOutput,
            BufferCount  = 2,
            OutputWindow = desc.WindowHandle,
            Windowed     = true,
            SwapEffect   = DXGI.SwapEffect.Discard,
            Flags        = 0,
        };

        ComPtr<DXGI.IDXGISwapChain> native = default;
        SilkMarshal.ThrowHResult(Factory.CreateSwapChain(
            (IUnknown*)Device.Handle, ref swapChainDesc, native.GetAddressOf()));

        var swapchain = new Dx11Swapchain
        {
            Native      = native,
            Width       = desc.Width,
            Height      = desc.Height,
            Samples     = desc.Samples,
            ColorFormat = desc.ColorFormat,
            DepthFormat = desc.DepthFormat,
            VSync       = desc.VSync,
        };
        AcquireSwapchainViews(swapchain);

        uint id = AllocHandle();
        Swapchains[id] = swapchain;
        return new SwapchainHandle(id);
    }

    // Grabs the backbuffer colour view and creates a matching depth texture
    // and view. Called on creation and again after every resize.
    private void AcquireSwapchainViews(Dx11Swapchain swapchain)
    {
        // Colour -- the swapchain's backbuffer texture + render-target view.
        ComPtr<DX.ID3D11Texture2D> backbuffer = default;
        Guid texture2DIid = DX.ID3D11Texture2D.Guid;
        SilkMarshal.ThrowHResult(swapchain.Native.GetBuffer(
            0, ref texture2DIid, (void**)backbuffer.GetAddressOf()));
        swapchain.ColorBuffer = backbuffer;

        ComPtr<DX.ID3D11RenderTargetView> colorView = default;
        SilkMarshal.ThrowHResult(Device.CreateRenderTargetView(
            (DX.ID3D11Resource*)backbuffer.Handle, (DX.RenderTargetViewDesc*)null, colorView.GetAddressOf()));
        swapchain.ColorView = colorView;

        // Depth -- a matching depth texture + depth-stencil view.
        if (swapchain.DepthFormat != Format.Unknown)
        {
            var depthTextureDesc = new DX.Texture2DDesc
            {
                Width      = (uint)swapchain.Width,
                Height     = (uint)swapchain.Height,
                MipLevels  = 1,
                ArraySize  = 1,
                Format     = Dx11Mapping.ToDxgi(swapchain.DepthFormat),
                SampleDesc = new DXGI.SampleDesc { Count = (uint)swapchain.Samples, Quality = 0 },
                Usage      = DX.Usage.Default,
                BindFlags  = (uint)DX.BindFlag.DepthStencil,
            };
            ComPtr<DX.ID3D11Texture2D> depthTexture = default;
            SilkMarshal.ThrowHResult(Device.CreateTexture2D(
                in depthTextureDesc, (DX.SubresourceData*)null, depthTexture.GetAddressOf()));
            swapchain.DepthBuffer = depthTexture;

            ComPtr<DX.ID3D11DepthStencilView> depthView = default;
            SilkMarshal.ThrowHResult(Device.CreateDepthStencilView(
                (DX.ID3D11Resource*)depthTexture.Handle, (DX.DepthStencilViewDesc*)null, depthView.GetAddressOf()));
            swapchain.DepthView = depthView;
        }
    }

    public void ResizeSwapchain(SwapchainHandle handle, int width, int height)
    {
        var swapchain = Swapchains[handle.Id];
        swapchain.ReleaseBackbufferViews();
        SilkMarshal.ThrowHResult(swapchain.Native.ResizeBuffers(
            0,                          // keep the buffer count
            (uint)width, (uint)height,
            DXGI.Format.FormatUnknown,  // keep the format
            0));
        swapchain.Width  = width;
        swapchain.Height = height;
        AcquireSwapchainViews(swapchain);
    }

    public void Destroy(SwapchainHandle handle)
    {
        if (Swapchains.Remove(handle.Id, out var swapchain)) swapchain.Dispose();
    }
}
