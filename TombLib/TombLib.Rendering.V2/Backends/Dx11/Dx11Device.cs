using System;
using System.Collections.Generic;
using System.Numerics;
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
/// </summary>
public unsafe sealed class Dx11Device : IRhiDevice
{
    private readonly DX.D3D11 _d3d11;
    private readonly DXGI.DXGI _dxgi;

    internal ComPtr<DXGI.IDXGIFactory>       Factory;
    internal ComPtr<DXGI.IDXGIAdapter>       Adapter;
    internal ComPtr<DX.ID3D11Device>         Device;
    internal ComPtr<DX.ID3D11DeviceContext>  Context;

    // Push constants emulation: small dynamic constant buffer bound at b0.
    // PushConstants writes the payload with MAP_WRITE_DISCARD each call.
    internal ComPtr<DX.ID3D11Buffer>         PushCb;

    private uint _nextHandle = 1;
    internal readonly Dictionary<uint, Dx11Buffer>    Buffers    = new();
    internal readonly Dictionary<uint, Dx11Texture>   Textures   = new();
    internal readonly Dictionary<uint, Dx11Sampler>   Samplers   = new();
    internal readonly Dictionary<uint, Dx11Pipeline>  Pipelines  = new();
    internal readonly Dictionary<uint, Dx11Swapchain> Swapchains = new();

    public RhiCapabilities Capabilities { get; }

    public Dx11Device()
    {
        // GetApi(null) loads the system D3D11/DXGI without binding to any
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
        Silk.NET.Core.Native.D3DFeatureLevel[] featureLevels = {
            Silk.NET.Core.Native.D3DFeatureLevel.Level110,
            Silk.NET.Core.Native.D3DFeatureLevel.Level101,
            Silk.NET.Core.Native.D3DFeatureLevel.Level100,
        };
        uint createFlags = (uint)DX.CreateDeviceFlag.BgraSupport;
        // The D3D11 debug layer validates every API call (UpdateSubresource,
        // Draw, every Set*) and adds 1-10 µs per call. When a wad load fires
        // thousands of those in a row the Debug build feels noticeably slower
        // than Release. Opt-in only: set TOMBEDITOR_D3D_DEBUG=1 (or attach a
        // GPU debugger like RenderDoc / PIX which sets it themselves).
        if (Environment.GetEnvironmentVariable("TOMBEDITOR_D3D_DEBUG") == "1")
            createFlags |= (uint)DX.CreateDeviceFlag.Debug;

        ComPtr<DX.ID3D11Device>        dev = default;
        ComPtr<DX.ID3D11DeviceContext> ctx = default;
        Silk.NET.Core.Native.D3DFeatureLevel achievedLevel;
        fixed (Silk.NET.Core.Native.D3DFeatureLevel* pFL = featureLevels)
        {
            SilkMarshal.ThrowHResult(_d3d11.CreateDevice(
                (DXGI.IDXGIAdapter*)Adapter.Handle,
                Silk.NET.Core.Native.D3DDriverType.Unknown,
                0,
                createFlags,
                pFL,
                (uint)featureLevels.Length,
                DX.D3D11.SdkVersion,
                dev.GetAddressOf(),
                &achievedLevel,
                ctx.GetAddressOf()));
        }
        Device  = dev;
        Context = ctx;

        // Allocate the push-constant constant buffer.
        var pcDesc = new DX.BufferDesc
        {
            ByteWidth      = (uint)RhiLimits.PushConstantSize,
            Usage          = DX.Usage.Dynamic,
            BindFlags      = (uint)DX.BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)DX.CpuAccessFlag.Write,
        };
        ComPtr<DX.ID3D11Buffer> pushCb = default;
        SilkMarshal.ThrowHResult(Device.CreateBuffer(in pcDesc, (DX.SubresourceData*)null, pushCb.GetAddressOf()));
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
        foreach (var s in Swapchains.Values) s.Dispose();
        foreach (var p in Pipelines.Values)  p.Dispose();
        foreach (var s in Samplers.Values)   s.Dispose();
        foreach (var t in Textures.Values)   t.Dispose();
        foreach (var b in Buffers.Values)    b.Dispose();
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

    // ---------------------------------------------------------------- Buffers

    public BufferHandle CreateBuffer(in BufferDesc desc, ReadOnlySpan<byte> initialData)
    {
        var (usage, cpu) = Dx11Mapping.ToD3d(desc.Usage);
        var bd = new DX.BufferDesc
        {
            ByteWidth           = (uint)desc.SizeBytes,
            Usage               = usage,
            BindFlags           = (uint)Dx11Mapping.ToBindFlags(desc.BindFlags),
            CPUAccessFlags      = (uint)cpu,
            MiscFlags           = (desc.BindFlags & BufferBindFlags.Structured) != 0
                                  ? (uint)DX.ResourceMiscFlag.BufferStructured
                                  : 0u,
            StructureByteStride = (uint)desc.StructureStride,
        };

        ComPtr<DX.ID3D11Buffer> native = default;
        fixed (byte* p = initialData)
        {
            if (initialData.Length > 0)
            {
                var sd = new DX.SubresourceData { PSysMem = p };
                SilkMarshal.ThrowHResult(Device.CreateBuffer(in bd, &sd, native.GetAddressOf()));
            }
            else
            {
                SilkMarshal.ThrowHResult(Device.CreateBuffer(in bd, (DX.SubresourceData*)null, native.GetAddressOf()));
            }
        }

        // Optional SRV for structured buffers (StructuredReadOnly usage).
        ComPtr<DX.ID3D11ShaderResourceView> srv = default;
        if ((desc.BindFlags & BufferBindFlags.Structured) != 0 && desc.StructureStride > 0)
        {
            var srvDesc = new DX.ShaderResourceViewDesc
            {
                Format        = DXGI.Format.FormatUnknown,
                ViewDimension = Silk.NET.Core.Native.D3DSrvDimension.D3D11SrvDimensionBufferex,
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

    public void Destroy(BufferHandle h)
    {
        if (Buffers.Remove(h.Id, out var b)) b.Dispose();
    }

    // ---------------------------------------------------------------- Textures

    public TextureHandle CreateTexture(in TextureDesc desc, ReadOnlySpan<byte> initialData)
    {
        // MipLevels == 0 → auto-generate a full mip chain. Required for
        // anisotropic filtering to look right at distance. The texture has
        // to also be a render target so we can call GenerateMips on it.
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

        var td = new DX.Texture2DDesc
        {
            Width            = (uint)desc.Width,
            Height           = (uint)desc.Height,
            MipLevels        = autoMip ? 0u : (uint)desc.MipLevels,
            ArraySize        = (uint)desc.ArrayLayers,
            Format           = Dx11Mapping.ToDxgi(desc.Format),
            SampleDesc       = new DXGI.SampleDesc { Count = (uint)desc.Samples, Quality = 0 },
            // Auto-mip textures must be Usage.Default (Immutable forbids
            // GenerateMips / UpdateSubresource), and the initial-data path
            // can't supply every level, so upload mip 0 separately below.
            Usage            = autoMip
                               ? DX.Usage.Default
                               : initialData.Length > 0 && (desc.BindFlags & (TextureBindFlags.RenderTarget | TextureBindFlags.DepthStencil)) == 0
                                 ? DX.Usage.Immutable
                                 : DX.Usage.Default,
            BindFlags        = bindFlags,
            CPUAccessFlags   = 0,
            MiscFlags        = miscFlags,
        };

        ComPtr<DX.ID3D11Texture2D> native = default;
        fixed (byte* p = initialData)
        {
            if (initialData.Length > 0 && !autoMip)
            {
                // Single-subresource upload path: tight rows, mip 0, layer 0.
                int bpp = BytesPerPixelOf(desc.Format);
                var sd = new DX.SubresourceData
                {
                    PSysMem     = p,
                    SysMemPitch = (uint)(desc.Width * bpp),
                };
                SilkMarshal.ThrowHResult(Device.CreateTexture2D(in td, &sd, native.GetAddressOf()));
            }
            else
            {
                SilkMarshal.ThrowHResult(Device.CreateTexture2D(in td, (DX.SubresourceData*)null, native.GetAddressOf()));
            }
        }

        // For auto-mip textures, push mip 0 then generate the chain.
        if (autoMip && initialData.Length > 0)
        {
            int bpp = BytesPerPixelOf(desc.Format);
            fixed (byte* p = initialData)
            {
                Context.UpdateSubresource(
                    (DX.ID3D11Resource*)native.Handle, 0u,
                    (DX.Box*)null, p, (uint)(desc.Width * bpp), 0u);
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
        var t = Textures[handle.Id];
        var box = new DX.Box
        {
            Left   = (uint)x,
            Top    = (uint)y,
            Front  = 0,
            Right  = (uint)(x + width),
            Bottom = (uint)(y + height),
            Back   = 1,
        };
        fixed (byte* p = data)
        {
            Context.UpdateSubresource(
                (DX.ID3D11Resource*)t.Native.Handle,
                (uint)subresource,
                &box,
                p,
                (uint)rowPitchBytes,
                0);
        }
    }

    public void Destroy(TextureHandle h)
    {
        if (Textures.Remove(h.Id, out var t)) t.Dispose();
    }

    public byte[] ReadTexture(TextureHandle handle, int subresource = 0)
    {
        var t = Textures[handle.Id];
        int bpp = BytesPerPixelOf(t.Format);
        int rowBytes = t.Width * bpp;
        byte[] pixels = new byte[rowBytes * t.Height];

        // Staging texture (CPU-readable, no bind flags, USAGE_STAGING).
        var sd = new DX.Texture2DDesc
        {
            Width      = (uint)t.Width,
            Height     = (uint)t.Height,
            MipLevels  = 1,
            ArraySize  = 1,
            Format     = Dx11Mapping.ToDxgi(t.Format),
            SampleDesc = new DXGI.SampleDesc { Count = 1, Quality = 0 },
            Usage      = DX.Usage.Staging,
            BindFlags  = 0,
            CPUAccessFlags = (uint)DX.CpuAccessFlag.Read,
        };
        ComPtr<DX.ID3D11Texture2D> staging = default;
        SilkMarshal.ThrowHResult(Device.CreateTexture2D(in sd, (DX.SubresourceData*)null, staging.GetAddressOf()));
        try
        {
            // Copy the chosen subresource of the source into mip-0 of the staging texture.
            Context.CopySubresourceRegion(
                (DX.ID3D11Resource*)staging.Handle, 0u, 0u, 0u, 0u,
                (DX.ID3D11Resource*)t.Native.Handle, (uint)subresource,
                (DX.Box*)null);

            DX.MappedSubresource mapped;
            SilkMarshal.ThrowHResult(Context.Map(
                (DX.ID3D11Resource*)staging.Handle, 0u, DX.Map.Read, 0u, &mapped));
            try
            {
                fixed (byte* dst = pixels)
                {
                    for (int y = 0; y < t.Height; y++)
                        Buffer.MemoryCopy(
                            (byte*)mapped.PData + y * mapped.RowPitch,
                            dst + y * rowBytes,
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

    // ---------------------------------------------------------------- Samplers

    public SamplerHandle CreateSampler(in SamplerDesc desc)
    {
        var (filter, maxAniso) = Dx11Mapping.ToD3d(desc.MinFilter, desc.MipFilter, desc.MaxAnisotropy);
        var sd = new DX.SamplerDesc
        {
            Filter         = filter,
            AddressU       = Dx11Mapping.ToD3d(desc.AddressU),
            AddressV       = Dx11Mapping.ToD3d(desc.AddressV),
            AddressW       = Dx11Mapping.ToD3d(desc.AddressW),
            MipLODBias     = 0,
            MaxAnisotropy  = maxAniso,
            ComparisonFunc = DX.ComparisonFunc.Never,
            MinLOD         = 0,
            MaxLOD         = float.MaxValue,
        };
        sd.BorderColor[0] = desc.BorderColor.X;
        sd.BorderColor[1] = desc.BorderColor.Y;
        sd.BorderColor[2] = desc.BorderColor.Z;
        sd.BorderColor[3] = desc.BorderColor.W;

        ComPtr<DX.ID3D11SamplerState> native = default;
        SilkMarshal.ThrowHResult(Device.CreateSamplerState(in sd, native.GetAddressOf()));

        uint id = AllocHandle();
        Samplers[id] = new Dx11Sampler { Native = native };
        return new SamplerHandle(id);
    }

    public void Destroy(SamplerHandle h)
    {
        if (Samplers.Remove(h.Id, out var s)) s.Dispose();
    }

    // --------------------------------------------------------------- Pipelines

    public PipelineHandle CreatePipeline(PipelineDesc desc)
    {
        // Shaders.
        ComPtr<DX.ID3D11VertexShader> vs = default;
        ComPtr<DX.ID3D11PixelShader>  ps = default;

        byte[] vsBytes = desc.VertexShader.DxbcBytes
            ?? throw new InvalidOperationException("VertexShader.DxbcBytes is null. Did the shader build pipeline run?");
        byte[] psBytes = desc.FragmentShader.DxbcBytes
            ?? throw new InvalidOperationException("FragmentShader.DxbcBytes is null. Did the shader build pipeline run?");

        fixed (byte* pVs = vsBytes)
        fixed (byte* pPs = psBytes)
        {
            SilkMarshal.ThrowHResult(Device.CreateVertexShader(pVs, (nuint)vsBytes.Length, (DX.ID3D11ClassLinkage*)null, vs.GetAddressOf()));
            SilkMarshal.ThrowHResult(Device.CreatePixelShader (pPs, (nuint)psBytes.Length, (DX.ID3D11ClassLinkage*)null, ps.GetAddressOf()));
        }

        // Input layout.
        ComPtr<DX.ID3D11InputLayout> layout = default;
        var elements = new DX.InputElementDesc[desc.VertexAttributes.Length];
        // Pin one byte array per semantic name so its address stays valid through CreateInputLayout.
        byte[][] semanticBlobs = new byte[desc.VertexAttributes.Length][];
        for (int i = 0; i < desc.VertexAttributes.Length; i++)
        {
            var a = desc.VertexAttributes[i];
            semanticBlobs[i] = System.Text.Encoding.ASCII.GetBytes(a.SemanticName + "\0");
        }
        var semanticHandles = new System.Runtime.InteropServices.GCHandle[semanticBlobs.Length];
        try
        {
            for (int i = 0; i < desc.VertexAttributes.Length; i++)
            {
                var a = desc.VertexAttributes[i];
                semanticHandles[i] = System.Runtime.InteropServices.GCHandle.Alloc(
                    semanticBlobs[i], System.Runtime.InteropServices.GCHandleType.Pinned);
                elements[i] = new DX.InputElementDesc
                {
                    SemanticName         = (byte*)semanticHandles[i].AddrOfPinnedObject(),
                    SemanticIndex        = (uint)a.SemanticIndex,
                    Format               = Dx11Mapping.ToDxgi(a.Format),
                    InputSlot            = (uint)a.BufferSlot,
                    AlignedByteOffset    = (uint)a.Offset,
                    InputSlotClass       = a.PerInstance
                                           ? DX.InputClassification.PerInstanceData
                                           : DX.InputClassification.PerVertexData,
                    InstanceDataStepRate = a.PerInstance ? 1u : 0u,
                };
            }
            fixed (DX.InputElementDesc* pEl = elements)
            fixed (byte* pVs = vsBytes)
            {
                SilkMarshal.ThrowHResult(Device.CreateInputLayout(
                    pEl,
                    (uint)elements.Length,
                    pVs,
                    (nuint)vsBytes.Length,
                    layout.GetAddressOf()));
            }
        }
        finally
        {
            for (int i = 0; i < semanticHandles.Length; i++)
                if (semanticHandles[i].IsAllocated) semanticHandles[i].Free();
        }

        // Rasterizer.
        var rs = desc.Rasterizer;
        var rsDesc = new DX.RasterizerDesc
        {
            FillMode              = Dx11Mapping.ToD3d(rs.FillMode),
            CullMode              = Dx11Mapping.ToD3d(rs.CullMode),
            FrontCounterClockwise = rs.FrontCounterClockwise,
            DepthBias             = rs.DepthBias,
            DepthBiasClamp        = 0,
            SlopeScaledDepthBias  = rs.SlopeScaledDepthBias,
            DepthClipEnable       = true,
            ScissorEnable         = rs.ScissorEnable,
            // Use MSAA-aware coverage rules when the bound render target
            // is multisampled — harmless on single-sample targets.
            MultisampleEnable     = true,
            AntialiasedLineEnable = false,
        };
        ComPtr<DX.ID3D11RasterizerState> rsState = default;
        SilkMarshal.ThrowHResult(Device.CreateRasterizerState(in rsDesc, rsState.GetAddressOf()));

        // Depth/stencil.
        var ds = desc.DepthStencil;
        var dsDesc = new DX.DepthStencilDesc
        {
            DepthEnable      = ds.DepthTestEnable,
            DepthWriteMask   = ds.DepthWriteEnable ? DX.DepthWriteMask.All : DX.DepthWriteMask.Zero,
            DepthFunc        = Dx11Mapping.ToD3d(ds.DepthCompare),
            StencilEnable    = false,
            StencilReadMask  = 0xFF,
            StencilWriteMask = 0xFF,
        };
        ComPtr<DX.ID3D11DepthStencilState> dsState = default;
        SilkMarshal.ThrowHResult(Device.CreateDepthStencilState(in dsDesc, dsState.GetAddressOf()));

        // Blend.
        var bs = desc.BlendStates.Length > 0 ? desc.BlendStates[0] : BlendState.Opaque;
        var bDesc = new DX.BlendDesc
        {
            AlphaToCoverageEnable  = false,
            IndependentBlendEnable = false,
        };
        bDesc.RenderTarget[0] = new DX.RenderTargetBlendDesc
        {
            BlendEnable           = bs.Enable,
            SrcBlend              = Dx11Mapping.ToD3d(bs.SrcColor),
            DestBlend             = Dx11Mapping.ToD3d(bs.DstColor),
            BlendOp               = Dx11Mapping.ToD3d(bs.ColorOp),
            SrcBlendAlpha         = Dx11Mapping.ToD3d(bs.SrcAlpha),
            DestBlendAlpha        = Dx11Mapping.ToD3d(bs.DstAlpha),
            BlendOpAlpha          = Dx11Mapping.ToD3d(bs.AlphaOp),
            RenderTargetWriteMask = Dx11Mapping.ToD3dColorWriteMask(bs.WriteMask),
        };
        ComPtr<DX.ID3D11BlendState> blendState = default;
        SilkMarshal.ThrowHResult(Device.CreateBlendState(in bDesc, blendState.GetAddressOf()));

        var strides = new int[RhiLimits.MaxVertexBuffers];
        foreach (var l in desc.VertexBufferLayouts)
            _ = l; // strides come from VertexBufferLayouts; computed on bind by caller.
        for (int i = 0; i < desc.VertexBufferLayouts.Length && i < strides.Length; i++)
            strides[i] = desc.VertexBufferLayouts[i].StrideBytes;

        uint id = AllocHandle();
        Pipelines[id] = new Dx11Pipeline
        {
            Vs            = vs,
            Ps            = ps,
            InputLayout   = layout,
            Rasterizer    = rsState,
            Blend         = blendState,
            Depth         = dsState,
            Topology      = Dx11Mapping.ToD3d(desc.Topology),
            VertexStrides = strides,
        };
        return new PipelineHandle(id);
    }

    public void Destroy(PipelineHandle h)
    {
        if (Pipelines.Remove(h.Id, out var p)) p.Dispose();
    }

    // --------------------------------------------------------------- Swapchain

    public SwapchainHandle CreateSwapchain(in SwapchainDesc desc)
    {
        var scDesc = new DXGI.SwapChainDesc
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
            (IUnknown*)Device.Handle,
            ref scDesc,
            native.GetAddressOf()));

        var sc = new Dx11Swapchain
        {
            Native      = native,
            Width       = desc.Width,
            Height      = desc.Height,
            Samples     = desc.Samples,
            ColorFormat = desc.ColorFormat,
            DepthFormat = desc.DepthFormat,
            VSync       = desc.VSync,
        };
        AcquireSwapchainViews(sc);

        uint id = AllocHandle();
        Swapchains[id] = sc;
        return new SwapchainHandle(id);
    }

    private void AcquireSwapchainViews(Dx11Swapchain sc)
    {
        // Color: grab backbuffer texture + RTV.
        ComPtr<DX.ID3D11Texture2D> backbuffer = default;
        Guid iid = DX.ID3D11Texture2D.Guid;
        SilkMarshal.ThrowHResult(sc.Native.GetBuffer(0, ref iid, (void**)backbuffer.GetAddressOf()));
        sc.ColorBuffer = backbuffer;

        ComPtr<DX.ID3D11RenderTargetView> rtv = default;
        SilkMarshal.ThrowHResult(Device.CreateRenderTargetView(
            (DX.ID3D11Resource*)backbuffer.Handle, (DX.RenderTargetViewDesc*)null, rtv.GetAddressOf()));
        sc.ColorView = rtv;

        // Depth: create matching depth texture + DSV.
        if (sc.DepthFormat != Format.Unknown)
        {
            var dDesc = new DX.Texture2DDesc
            {
                Width      = (uint)sc.Width,
                Height     = (uint)sc.Height,
                MipLevels  = 1,
                ArraySize  = 1,
                Format     = Dx11Mapping.ToDxgi(sc.DepthFormat),
                SampleDesc = new DXGI.SampleDesc { Count = (uint)sc.Samples, Quality = 0 },
                Usage      = DX.Usage.Default,
                BindFlags  = (uint)DX.BindFlag.DepthStencil,
            };
            ComPtr<DX.ID3D11Texture2D> depth = default;
            SilkMarshal.ThrowHResult(Device.CreateTexture2D(in dDesc, (DX.SubresourceData*)null, depth.GetAddressOf()));
            sc.DepthBuffer = depth;

            ComPtr<DX.ID3D11DepthStencilView> dsv = default;
            SilkMarshal.ThrowHResult(Device.CreateDepthStencilView(
                (DX.ID3D11Resource*)depth.Handle, (DX.DepthStencilViewDesc*)null, dsv.GetAddressOf()));
            sc.DepthView = dsv;
        }
    }

    public void ResizeSwapchain(SwapchainHandle handle, int width, int height)
    {
        var sc = Swapchains[handle.Id];
        sc.ReleaseBackbufferViews();
        SilkMarshal.ThrowHResult(sc.Native.ResizeBuffers(
            0,                                  // keep buffer count
            (uint)width, (uint)height,
            DXGI.Format.FormatUnknown,          // keep format
            0));
        sc.Width  = width;
        sc.Height = height;
        AcquireSwapchainViews(sc);
    }

    public void Destroy(SwapchainHandle h)
    {
        if (Swapchains.Remove(h.Id, out var s)) s.Dispose();
    }

    // ----------------------------------------------------------- Command stream

    public ICommandList BeginCommandList() => new Dx11CommandList(this);

    public void Submit(ICommandList commandList)
    {
        // Immediate-mode command list: every Set/Draw already executed
        // against Context. Submit is a no-op flush hook.
        _ = (Dx11CommandList)commandList;
    }

    public void Present(SwapchainHandle handle)
    {
        var sc = Swapchains[handle.Id];
        SilkMarshal.ThrowHResult(sc.Native.Present(sc.VSync ? 1u : 0u, 0u));
    }

    public void WaitIdle()
    {
        // ID3D11DeviceContext.Flush() returns once the GPU has accepted the
        // recorded commands; a real fence would be more thorough, but for
        // shutdown / resize this is sufficient on a single-threaded device.
        Context.Flush();
    }

    // -------------------------------------------------------------- Helpers

    internal Dx11Buffer    GetBuffer(BufferHandle h)    => Buffers[h.Id];
    internal Dx11Texture   GetTexture(TextureHandle h)  => Textures[h.Id];
    internal Dx11Sampler   GetSampler(SamplerHandle h)  => Samplers[h.Id];
    internal Dx11Pipeline  GetPipeline(PipelineHandle h)=> Pipelines[h.Id];
    internal Dx11Swapchain GetSwapchain(SwapchainHandle h) => Swapchains[h.Id];

    private static int BytesPerPixelOf(Format f) => f switch
    {
        Format.R8G8B8A8_UNorm or Format.R8G8B8A8_UNorm_SRgb or Format.B8G8R8A8_UNorm or Format.R8G8B8A8_UInt or Format.R32_UInt or Format.R32_Float => 4,
        Format.R16G16B16A16_Float or Format.R16G16B16A16_UNorm or Format.R32G32_Float                                                              => 8,
        Format.R32G32B32_Float                                                                                                                     => 12,
        Format.R32G32B32A32_Float                                                                                                                  => 16,
        _ => throw new ArgumentOutOfRangeException(nameof(f), f, "BytesPerPixel undefined for this format"),
    };
}
