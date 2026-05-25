using System;
using Silk.NET.Core.Native;
using DX = Silk.NET.Direct3D11;
using DXGI = Silk.NET.DXGI;
using TombLib.Rendering.Graphics.Rhi;

namespace TombLib.Rendering.Graphics.Backends.Dx11;

// Internal data containers held by Dx11Device's resource pools. Each owns
// its COM references and releases them on Dispose. None of these are
// exposed across the RHI surface; callers see only opaque handles.

internal sealed class Dx11Buffer : IDisposable
{
    public ComPtr<DX.ID3D11Buffer>               Native;
    public ComPtr<DX.ID3D11ShaderResourceView>   Srv;     // non-null only for StructuredReadOnly
    public int                                   SizeBytes;
    public BufferUsage                           Usage;
    public BufferBindFlags                       BindFlags;
    public int                                   StructureStride;

    public void Dispose()
    {
        Srv.Dispose();
        Native.Dispose();
    }
}

internal sealed class Dx11Texture : IDisposable
{
    public ComPtr<DX.ID3D11Texture2D>            Native;
    public ComPtr<DX.ID3D11ShaderResourceView>   Srv;
    public ComPtr<DX.ID3D11RenderTargetView>     Rtv;
    public ComPtr<DX.ID3D11DepthStencilView>     Dsv;
    public Format                                Format;
    public int                                   Width, Height, ArrayLayers, MipLevels, Samples;
    public TextureBindFlags                      BindFlags;

    public void Dispose()
    {
        Dsv.Dispose();
        Rtv.Dispose();
        Srv.Dispose();
        Native.Dispose();
    }
}

internal sealed class Dx11Sampler : IDisposable
{
    public ComPtr<DX.ID3D11SamplerState> Native;

    public void Dispose() => Native.Dispose();
}

// A pipeline bakes shaders + fixed-function state + input layout together.
// SetPipeline at draw time is a single Apply() of all the cached state.
internal sealed class Dx11Pipeline : IDisposable
{
    public ComPtr<DX.ID3D11VertexShader>      Vs;
    public ComPtr<DX.ID3D11PixelShader>       Ps;
    public ComPtr<DX.ID3D11InputLayout>       InputLayout;
    public ComPtr<DX.ID3D11RasterizerState>   Rasterizer;
    public ComPtr<DX.ID3D11BlendState>        Blend;
    public ComPtr<DX.ID3D11DepthStencilState> Depth;
    public Silk.NET.Core.Native.D3DPrimitiveTopology Topology;
    public int[]                              VertexStrides = Array.Empty<int>();

    public void Dispose()
    {
        Depth.Dispose();
        Blend.Dispose();
        Rasterizer.Dispose();
        InputLayout.Dispose();
        Ps.Dispose();
        Vs.Dispose();
    }
}

internal sealed class Dx11Swapchain : IDisposable
{
    public ComPtr<DXGI.IDXGISwapChain>           Native;
    public ComPtr<DX.ID3D11Texture2D>            ColorBuffer;
    public ComPtr<DX.ID3D11RenderTargetView>     ColorView;
    public ComPtr<DX.ID3D11Texture2D>            DepthBuffer;
    public ComPtr<DX.ID3D11DepthStencilView>     DepthView;
    public int                                   Width, Height, Samples;
    public Format                                ColorFormat, DepthFormat;
    public bool                                  VSync;

    public void ReleaseBackbufferViews()
    {
        DepthView.Dispose();   DepthView   = default;
        DepthBuffer.Dispose(); DepthBuffer = default;
        ColorView.Dispose();   ColorView   = default;
        ColorBuffer.Dispose(); ColorBuffer = default;
    }

    public void Dispose()
    {
        ReleaseBackbufferViews();
        Native.Dispose();
    }
}
