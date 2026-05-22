using System;
using System.Collections.Generic;
using Silk.NET.Core.Native;
using DX = Silk.NET.Direct3D11;
using DXGI = Silk.NET.DXGI;
using TombLib.RenderingV2.Rhi;
// Selective import of Silk.NET extension methods on ComPtr<T>. The full
// namespace is not imported because it collides with Rhi descriptor types
// (Format, BufferDesc, SamplerDesc).
using static Silk.NET.Direct3D11.D3D11DeviceVtblExtensions;
using static Silk.NET.Direct3D11.D3D11DeviceContextVtblExtensions;
using static Silk.NET.DXGI.DXGIFactoryVtblExtensions;
using static Silk.NET.DXGI.DXGISwapChainVtblExtensions;

namespace TombLib.RenderingV2.Backends.Dx11;

/// <summary>
/// Direct3D 11 backend for <see cref="IRhiDevice"/>. Talks to Silk.NET's
/// Direct3D11 / DXGI bindings directly — no SharpDX, no middleware.
/// Single-threaded: assumes all calls come from the same rendering thread.
///
/// <para>Split across partial files by concern:
///   Dx11Device.cs           — device, resources (buffers / textures / samplers).
///   Dx11Device.Pipeline.cs  — pipeline-state objects.
///   Dx11Device.Swapchain.cs — DXGI swapchain.</para>
/// </summary>
public unsafe sealed partial class Dx11Device : IRhiDevice
{
    private readonly DX.D3D11 _d3d11;
    private readonly DXGI.DXGI _dxgi;

    internal ComPtr<DXGI.IDXGIFactory>      Factory;
    internal ComPtr<DXGI.IDXGIAdapter>      Adapter;
    internal ComPtr<DX.ID3D11Device>        Device;
    internal ComPtr<DX.ID3D11DeviceContext> Context;

    // Push-constant emulation: a small dynamic constant buffer bound at b0.
    // PushConstants rewrites the payload with MAP_WRITE_DISCARD on each call.
    internal ComPtr<DX.ID3D11Buffer> PushCb;

    private uint _nextHandle = 1;

    internal readonly Dictionary<uint, Dx11Buffer>    Buffers    = new();
    internal readonly Dictionary<uint, Dx11Texture>   Textures   = new();
    internal readonly Dictionary<uint, Dx11Sampler>   Samplers   = new();
    internal readonly Dictionary<uint, Dx11Pipeline>  Pipelines  = new();
    internal readonly Dictionary<uint, Dx11Swapchain> Swapchains = new();

    public RhiCapabilities Capabilities { get; }

    public Dx11Device()
    {
        // GetApi(null) loads the system D3D11 / DXGI without binding to any
        // specific window — the swapchain explicitly supplies its HWND later.
        _d3d11 = DX.D3D11.GetApi((Silk.NET.Core.Contexts.INativeWindowSource?)null);
        _dxgi  = DXGI.DXGI.GetApi((Silk.NET.Core.Contexts.INativeWindowSource?)null);

        // DXGI factory + first adapter.
        ComPtr<DXGI.IDXGIFactory> factory = default;
        SilkMarshal.ThrowHResult(_dxgi.CreateDXGIFactory(out factory));
        Factory = factory;

        ComPtr<DXGI.IDXGIAdapter> adapter = default;
        SilkMarshal.ThrowHResult(Factory.EnumAdapters(0, ref adapter));
        Adapter = adapter;

        // D3D11 device + immediate context.
        D3DFeatureLevel[] featureLevels =
        {
            D3DFeatureLevel.Level110,
            D3DFeatureLevel.Level101,
            D3DFeatureLevel.Level100,
        };

        uint createFlags = (uint)DX.CreateDeviceFlag.BgraSupport;
        // The D3D11 debug layer validates every API call (UpdateSubresource,
        // Draw, every Set*) and adds 1-10 µs per call. When a wad load fires
        // thousands of those in a row the Debug build feels noticeably slower
        // than Release. Opt-in only: set TOMBEDITOR_D3D_DEBUG=1 (or attach a
        // GPU debugger like RenderDoc / PIX, which set it themselves).
        if (Environment.GetEnvironmentVariable("TOMBEDITOR_D3D_DEBUG") == "1")
            createFlags |= (uint)DX.CreateDeviceFlag.Debug;

        ComPtr<DX.ID3D11Device>        device  = default;
        ComPtr<DX.ID3D11DeviceContext> context = default;
        D3DFeatureLevel achievedLevel;
        fixed (D3DFeatureLevel* pFeatureLevels = featureLevels)
        {
            SilkMarshal.ThrowHResult(_d3d11.CreateDevice(
                (DXGI.IDXGIAdapter*)Adapter.Handle,
                D3DDriverType.Unknown,
                0,
                createFlags,
                pFeatureLevels,
                (uint)featureLevels.Length,
                DX.D3D11.SdkVersion,
                device.GetAddressOf(),
                &achievedLevel,
                context.GetAddressOf()));
        }
        Device  = device;
        Context = context;

        // Allocate the push-constant constant buffer.
        var pushCbDesc = new DX.BufferDesc
        {
            ByteWidth      = (uint)RhiLimits.PushConstantSize,
            Usage          = DX.Usage.Dynamic,
            BindFlags      = (uint)DX.BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)DX.CpuAccessFlag.Write,
        };
        ComPtr<DX.ID3D11Buffer> pushCb = default;
        SilkMarshal.ThrowHResult(Device.CreateBuffer(in pushCbDesc, (DX.SubresourceData*)null, pushCb.GetAddressOf()));
        PushCb = pushCb;

        Capabilities = new RhiCapabilities(
            backend:        RhiBackendKind.DirectX11,
            instanced:      true,
            structured:     true,
            nativePush:     false,
            anisotropy:     true,
            debugMarkers:   true,
            maxTexSize:     16384,
            maxArrayLayers: 2048);
    }

    private uint AllocHandle() => _nextHandle++;

    public void Dispose()
    {
        foreach (var swapchain in Swapchains.Values) swapchain.Dispose();
        foreach (var pipeline  in Pipelines.Values)  pipeline.Dispose();
        foreach (var sampler   in Samplers.Values)   sampler.Dispose();
        foreach (var texture   in Textures.Values)   texture.Dispose();
        foreach (var buffer    in Buffers.Values)    buffer.Dispose();
        Swapchains.Clear();
        Pipelines.Clear();
        Samplers.Clear();
        Textures.Clear();
        Buffers.Clear();

        PushCb.Dispose();
        Context.Dispose();
        Device.Dispose();
        Adapter.Dispose();
        Factory.Dispose();
    }

    // ================================================================ Buffers

    public BufferHandle CreateBuffer(in BufferDesc desc, ReadOnlySpan<byte> initialData)
    {
        var (usage, cpuAccess) = Dx11Mapping.ToD3d(desc.Usage);
        var bufferDesc = new DX.BufferDesc
        {
            ByteWidth           = (uint)desc.SizeBytes,
            Usage               = usage,
            BindFlags           = (uint)Dx11Mapping.ToBindFlags(desc.BindFlags),
            CPUAccessFlags      = (uint)cpuAccess,
            MiscFlags           = (desc.BindFlags & BufferBindFlags.Structured) != 0
                                  ? (uint)DX.ResourceMiscFlag.BufferStructured
                                  : 0u,
            StructureByteStride = (uint)desc.StructureStride,
        };

        ComPtr<DX.ID3D11Buffer> native = default;
        fixed (byte* pInitialData = initialData)
        {
            if (initialData.Length > 0)
            {
                var subresource = new DX.SubresourceData { PSysMem = pInitialData };
                SilkMarshal.ThrowHResult(Device.CreateBuffer(in bufferDesc, &subresource, native.GetAddressOf()));
            }
            else
            {
                SilkMarshal.ThrowHResult(Device.CreateBuffer(in bufferDesc, (DX.SubresourceData*)null, native.GetAddressOf()));
            }
        }

        // Optional shader-resource view for structured buffers.
        ComPtr<DX.ID3D11ShaderResourceView> srv = default;
        if ((desc.BindFlags & BufferBindFlags.Structured) != 0 && desc.StructureStride > 0)
        {
            var srvDesc = new DX.ShaderResourceViewDesc
            {
                Format        = DXGI.Format.FormatUnknown,
                ViewDimension = D3DSrvDimension.D3D11SrvDimensionBufferex,
            };
            srvDesc.Anonymous.BufferEx = new DX.BufferexSrv
            {
                FirstElement = 0,
                NumElements  = (uint)(desc.SizeBytes / desc.StructureStride),
                Flags        = 0,
            };
            SilkMarshal.ThrowHResult(Device.CreateShaderResourceView(
                (DX.ID3D11Resource*)native.Handle, in srvDesc, srv.GetAddressOf()));
        }

        uint id = AllocHandle();
        Buffers[id] = new Dx11Buffer
        {
            Native          = native,
            Srv             = srv,
            SizeBytes       = desc.SizeBytes,
            Usage           = desc.Usage,
            BindFlags       = desc.BindFlags,
            StructureStride = desc.StructureStride,
        };
        return new BufferHandle(id);
    }

    public void Destroy(BufferHandle handle)
    {
        if (Buffers.Remove(handle.Id, out var buffer)) buffer.Dispose();
    }

    // =============================================================== Textures

    public TextureHandle CreateTexture(in TextureDesc desc, ReadOnlySpan<byte> initialData)
    {
        // MipLevels == 0 → auto-generate a full mip chain. Required for
        // anisotropic filtering to look right at distance. Such a texture
        // also has to be a render target so GenerateMips can run on it.
        bool autoMip   = desc.MipLevels == 0;
        bool hasShader = (desc.BindFlags & TextureBindFlags.ShaderResource) != 0;
        uint bindFlags = (uint)Dx11Mapping.ToBindFlags(desc.BindFlags);
        uint miscFlags = desc.Kind == TextureKind.TextureCube
                         ? (uint)DX.ResourceMiscFlag.Texturecube
                         : 0u;
        if (autoMip)
        {
            // GenerateMips requires SR + RT bind and the GenerateMips misc flag.
            bindFlags |= (uint)(DX.BindFlag.ShaderResource | DX.BindFlag.RenderTarget);
            miscFlags |= (uint)DX.ResourceMiscFlag.GenerateMips;
        }

        var textureDesc = new DX.Texture2DDesc
        {
            Width      = (uint)desc.Width,
            Height     = (uint)desc.Height,
            MipLevels  = autoMip ? 0u : (uint)desc.MipLevels,
            ArraySize  = (uint)desc.ArrayLayers,
            Format     = Dx11Mapping.ToDxgi(desc.Format),
            SampleDesc = new DXGI.SampleDesc { Count = (uint)desc.Samples, Quality = 0 },
            // Auto-mip textures must be Usage.Default (Immutable forbids
            // GenerateMips / UpdateSubresource), and the initial-data path
            // can't supply every level, so mip 0 is uploaded separately below.
            Usage      = autoMip
                         ? DX.Usage.Default
                         : initialData.Length > 0 && (desc.BindFlags & (TextureBindFlags.RenderTarget | TextureBindFlags.DepthStencil)) == 0
                           ? DX.Usage.Immutable
                           : DX.Usage.Default,
            BindFlags      = bindFlags,
            CPUAccessFlags = 0,
            MiscFlags      = miscFlags,
        };

        ComPtr<DX.ID3D11Texture2D> native = default;
        fixed (byte* pInitialData = initialData)
        {
            if (initialData.Length > 0 && !autoMip)
            {
                // Single-subresource upload: tight rows, mip 0, layer 0.
                int bytesPerPixel = BytesPerPixelOf(desc.Format);
                var subresource = new DX.SubresourceData
                {
                    PSysMem     = pInitialData,
                    SysMemPitch = (uint)(desc.Width * bytesPerPixel),
                };
                SilkMarshal.ThrowHResult(Device.CreateTexture2D(in textureDesc, &subresource, native.GetAddressOf()));
            }
            else
            {
                SilkMarshal.ThrowHResult(Device.CreateTexture2D(in textureDesc, (DX.SubresourceData*)null, native.GetAddressOf()));
            }
        }

        // For auto-mip textures, upload mip 0 and then generate the chain.
        if (autoMip && initialData.Length > 0)
        {
            int bytesPerPixel = BytesPerPixelOf(desc.Format);
            fixed (byte* pInitialData = initialData)
            {
                Context.UpdateSubresource(
                    (DX.ID3D11Resource*)native.Handle, 0u,
                    (DX.Box*)null, pInitialData, (uint)(desc.Width * bytesPerPixel), 0u);
            }
        }

        ComPtr<DX.ID3D11ShaderResourceView> srv = default;
        ComPtr<DX.ID3D11RenderTargetView>   rtv = default;
        ComPtr<DX.ID3D11DepthStencilView>   dsv = default;

        if (hasShader || autoMip)
        {
            SilkMarshal.ThrowHResult(Device.CreateShaderResourceView(
                (DX.ID3D11Resource*)native.Handle, (DX.ShaderResourceViewDesc*)null, srv.GetAddressOf()));
            if (autoMip)
                Context.GenerateMips(srv);
        }
        if ((desc.BindFlags & TextureBindFlags.RenderTarget) != 0)
        {
            SilkMarshal.ThrowHResult(Device.CreateRenderTargetView(
                (DX.ID3D11Resource*)native.Handle, (DX.RenderTargetViewDesc*)null, rtv.GetAddressOf()));
        }
        if ((desc.BindFlags & TextureBindFlags.DepthStencil) != 0)
        {
            SilkMarshal.ThrowHResult(Device.CreateDepthStencilView(
                (DX.ID3D11Resource*)native.Handle, (DX.DepthStencilViewDesc*)null, dsv.GetAddressOf()));
        }

        uint id = AllocHandle();
        Textures[id] = new Dx11Texture
        {
            Native      = native,
            Srv         = srv,
            Rtv         = rtv,
            Dsv         = dsv,
            Format      = desc.Format,
            Width       = desc.Width,
            Height      = desc.Height,
            ArrayLayers = desc.ArrayLayers,
            MipLevels   = desc.MipLevels,
            Samples     = desc.Samples,
            BindFlags   = desc.BindFlags,
        };
        return new TextureHandle(id);
    }

    public void UpdateTexture(TextureHandle handle, int subresource,
                              int x, int y, int width, int height,
                              int rowPitchBytes, ReadOnlySpan<byte> data)
    {
        var texture = Textures[handle.Id];
        var box = new DX.Box
        {
            Left   = (uint)x,
            Top    = (uint)y,
            Front  = 0,
            Right  = (uint)(x + width),
            Bottom = (uint)(y + height),
            Back   = 1,
        };
        fixed (byte* pData = data)
        {
            Context.UpdateSubresource(
                (DX.ID3D11Resource*)texture.Native.Handle,
                (uint)subresource,
                &box,
                pData,
                (uint)rowPitchBytes,
                0);
        }
    }

    public void Destroy(TextureHandle handle)
    {
        if (Textures.Remove(handle.Id, out var texture)) texture.Dispose();
    }

    public byte[] ReadTexture(TextureHandle handle, int subresource = 0)
    {
        var texture       = Textures[handle.Id];
        int bytesPerPixel = BytesPerPixelOf(texture.Format);
        int rowBytes      = texture.Width * bytesPerPixel;
        byte[] pixels     = new byte[rowBytes * texture.Height];

        // Staging texture — CPU-readable, no bind flags, USAGE_STAGING.
        var stagingDesc = new DX.Texture2DDesc
        {
            Width          = (uint)texture.Width,
            Height         = (uint)texture.Height,
            MipLevels      = 1,
            ArraySize      = 1,
            Format         = Dx11Mapping.ToDxgi(texture.Format),
            SampleDesc     = new DXGI.SampleDesc { Count = 1, Quality = 0 },
            Usage          = DX.Usage.Staging,
            BindFlags      = 0,
            CPUAccessFlags = (uint)DX.CpuAccessFlag.Read,
        };
        ComPtr<DX.ID3D11Texture2D> staging = default;
        SilkMarshal.ThrowHResult(Device.CreateTexture2D(in stagingDesc, (DX.SubresourceData*)null, staging.GetAddressOf()));
        try
        {
            // Copy the chosen subresource of the source into mip 0 of the staging texture.
            Context.CopySubresourceRegion(
                (DX.ID3D11Resource*)staging.Handle, 0u, 0u, 0u, 0u,
                (DX.ID3D11Resource*)texture.Native.Handle, (uint)subresource,
                (DX.Box*)null);

            DX.MappedSubresource mapped;
            SilkMarshal.ThrowHResult(Context.Map(
                (DX.ID3D11Resource*)staging.Handle, 0u, DX.Map.Read, 0u, &mapped));
            try
            {
                fixed (byte* pDst = pixels)
                {
                    for (int y = 0; y < texture.Height; y++)
                        Buffer.MemoryCopy(
                            (byte*)mapped.PData + y * mapped.RowPitch,
                            pDst + y * rowBytes,
                            rowBytes, rowBytes);
                }
            }
            finally
            {
                Context.Unmap((DX.ID3D11Resource*)staging.Handle, 0u);
            }
        }
        finally
        {
            staging.Dispose();
        }
        return pixels;
    }

    // =============================================================== Samplers

    public SamplerHandle CreateSampler(in SamplerDesc desc)
    {
        var (filter, maxAnisotropy) = Dx11Mapping.ToD3d(desc.MinFilter, desc.MipFilter, desc.MaxAnisotropy);
        var samplerDesc = new DX.SamplerDesc
        {
            Filter         = filter,
            AddressU       = Dx11Mapping.ToD3d(desc.AddressU),
            AddressV       = Dx11Mapping.ToD3d(desc.AddressV),
            AddressW       = Dx11Mapping.ToD3d(desc.AddressW),
            MipLODBias     = 0,
            MaxAnisotropy  = maxAnisotropy,
            ComparisonFunc = DX.ComparisonFunc.Never,
            MinLOD         = 0,
            MaxLOD         = float.MaxValue,
        };
        samplerDesc.BorderColor[0] = desc.BorderColor.X;
        samplerDesc.BorderColor[1] = desc.BorderColor.Y;
        samplerDesc.BorderColor[2] = desc.BorderColor.Z;
        samplerDesc.BorderColor[3] = desc.BorderColor.W;

        ComPtr<DX.ID3D11SamplerState> native = default;
        SilkMarshal.ThrowHResult(Device.CreateSamplerState(in samplerDesc, native.GetAddressOf()));

        uint id = AllocHandle();
        Samplers[id] = new Dx11Sampler { Native = native };
        return new SamplerHandle(id);
    }

    public void Destroy(SamplerHandle handle)
    {
        if (Samplers.Remove(handle.Id, out var sampler)) sampler.Dispose();
    }

    // ========================================================= Command stream

    public ICommandList BeginCommandList() => new Dx11CommandList(this);

    public void Submit(ICommandList commandList)
    {
        // Immediate-mode command list: every Set / Draw has already executed
        // against the device context. Submit is just a no-op flush hook.
        _ = (Dx11CommandList)commandList;
    }

    public void Present(SwapchainHandle handle)
    {
        var swapchain = Swapchains[handle.Id];
        SilkMarshal.ThrowHResult(swapchain.Native.Present(swapchain.VSync ? 1u : 0u, 0u));
    }

    public void WaitIdle()
    {
        // ID3D11DeviceContext.Flush() returns once the GPU has accepted the
        // recorded commands; a real fence would be more thorough, but for
        // shutdown / resize this is sufficient on a single-threaded device.
        Context.Flush();
    }

    // ================================================================ Helpers

    internal Dx11Buffer    GetBuffer(BufferHandle handle)       => Buffers[handle.Id];
    internal Dx11Texture   GetTexture(TextureHandle handle)     => Textures[handle.Id];
    internal Dx11Sampler   GetSampler(SamplerHandle handle)     => Samplers[handle.Id];
    internal Dx11Pipeline  GetPipeline(PipelineHandle handle)   => Pipelines[handle.Id];
    internal Dx11Swapchain GetSwapchain(SwapchainHandle handle) => Swapchains[handle.Id];

    private static int BytesPerPixelOf(Format format) => format switch
    {
        Format.R8G8B8A8_UNorm or Format.R8G8B8A8_UNorm_SRgb or Format.B8G8R8A8_UNorm
            or Format.R8G8B8A8_UInt or Format.R32_UInt or Format.R32_Float        => 4,
        Format.R16G16B16A16_Float or Format.R16G16B16A16_UNorm or Format.R32G32_Float => 8,
        Format.R32G32B32_Float                                                       => 12,
        Format.R32G32B32A32_Float                                                    => 16,
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "BytesPerPixel undefined for this format"),
    };
}
