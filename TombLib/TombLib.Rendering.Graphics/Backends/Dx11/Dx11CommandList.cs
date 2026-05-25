using System;
using System.Numerics;
using Silk.NET.Core.Native;
using DX = Silk.NET.Direct3D11;
using DXGI = Silk.NET.DXGI;
using TombLib.Rendering.Graphics.Rhi;
// `using static` selectively imports Silk.NET's extension methods on ComPtr<T>
// without pulling in the whole namespace (which would collide with Rhi types
// such as Format / BufferDesc / SamplerDesc).
using static Silk.NET.Direct3D11.D3D11DeviceVtblExtensions;
using static Silk.NET.Direct3D11.D3D11DeviceContextVtblExtensions;
using static Silk.NET.DXGI.DXGIFactoryVtblExtensions;
using static Silk.NET.DXGI.DXGISwapChainVtblExtensions;

namespace TombLib.Rendering.Graphics.Backends.Dx11;

/// <summary>
/// Immediate-mode command list. Each method maps directly to a call on the
/// device's <c>ID3D11DeviceContext</c>. The Begin/Submit pair is preserved
/// for API symmetry with deferred backends (Vulkan, GL).
/// </summary>
public unsafe sealed class Dx11CommandList : ICommandList
{
    private readonly Dx11Device _dev;
    private Dx11Pipeline? _boundPipeline;

    // Currently-bound swapchain target for the open pass, if any. Used by
    // BeginPass(UseSwapchain) to find the RTV/DSV pair.
    private Dx11Swapchain? _passSwapchain;

    internal Dx11CommandList(Dx11Device dev) => _dev = dev;

    public void BeginPass(in PassDesc desc)
    {
        DX.ID3D11RenderTargetView* rtv0 = null;
        DX.ID3D11DepthStencilView* dsv  = null;
        int width = desc.ViewportWidth;
        int height = desc.ViewportHeight;

        if (desc.UseSwapchain)
        {
            var sc = _dev.GetSwapchain(desc.Swapchain);
            _passSwapchain = sc;
            rtv0 = sc.ColorView.Handle;
            dsv  = sc.DepthView.Handle;
            if (width  == 0) width  = sc.Width;
            if (height == 0) height = sc.Height;
        }
        else if (desc.ColorAttachments != null && desc.ColorAttachments.Length > 0)
        {
            var color = _dev.GetTexture(desc.ColorAttachments[0]);
            rtv0 = color.Rtv.Handle;
            if (desc.DepthAttachment.IsValid)
                dsv = _dev.GetTexture(desc.DepthAttachment).Dsv.Handle;
            if (width  == 0) width  = color.Width;
            if (height == 0) height = color.Height;
        }

        // Bind the RT(s) and depth.
        DX.ID3D11RenderTargetView** rtvs = stackalloc DX.ID3D11RenderTargetView*[1];
        rtvs[0] = rtv0;
        _dev.Context.OMSetRenderTargets(rtv0 != null ? 1u : 0u, rtvs, dsv);

        // Clears.
        if (rtv0 != null && desc.ColorLoadOps != null && desc.ColorLoadOps.Length > 0 && desc.ColorLoadOps[0] == LoadOp.Clear)
        {
            Vector4 c = (desc.ClearColors != null && desc.ClearColors.Length > 0) ? desc.ClearColors[0] : default;
            float* col = stackalloc float[4] { c.X, c.Y, c.Z, c.W };
            _dev.Context.ClearRenderTargetView(rtv0, col);
        }
        if (dsv != null)
        {
            uint clearFlags = 0;
            if (desc.DepthLoadOp == LoadOp.Clear) clearFlags |= (uint)DX.ClearFlag.Depth;
            // We don't surface stencil ops separately yet; clear stencil when depth is cleared.
            if (desc.DepthLoadOp == LoadOp.Clear) clearFlags |= (uint)DX.ClearFlag.Stencil;
            if (clearFlags != 0)
                _dev.Context.ClearDepthStencilView(dsv, clearFlags, desc.ClearDepth, desc.ClearStencil);
        }

        // Viewport.
        var vp = new DX.Viewport
        {
            TopLeftX = desc.ViewportX,
            TopLeftY = desc.ViewportY,
            Width    = width,
            Height   = height,
            MinDepth = 0,
            MaxDepth = 1,
        };
        _dev.Context.RSSetViewports(1, in vp);
    }

    public void EndPass()
    {
        _passSwapchain = null;
        _boundPipeline = null;
        // Unbind targets to avoid carryover into the next pass.
        DX.ID3D11RenderTargetView** none = stackalloc DX.ID3D11RenderTargetView*[1] { null };
        _dev.Context.OMSetRenderTargets(0u, none, (DX.ID3D11DepthStencilView*)null);
    }

    public void SetPipeline(PipelineHandle pipeline)
    {
        var p = _dev.GetPipeline(pipeline);
        _boundPipeline = p;

        _dev.Context.IASetInputLayout(p.InputLayout);
        _dev.Context.IASetPrimitiveTopology(p.Topology);
        _dev.Context.VSSetShader(p.Vs, (DX.ID3D11ClassInstance**)null, 0);
        _dev.Context.PSSetShader(p.Ps, (DX.ID3D11ClassInstance**)null, 0);
        _dev.Context.RSSetState(p.Rasterizer);

        float* blendFactor = stackalloc float[4] { 1.0f, 1.0f, 1.0f, 1.0f };
        _dev.Context.OMSetBlendState(p.Blend, blendFactor, 0xFFFFFFFFu);
        _dev.Context.OMSetDepthStencilState(p.Depth, 0);
    }

    public void SetBindings(in Bindings bindings)
    {
        // Constant buffers.
        if (!bindings.ConstantBuffers.IsEmpty)
        {
            int n = Math.Min(bindings.ConstantBuffers.Length, RhiLimits.MaxConstantBuffers);
            DX.ID3D11Buffer** cbs = stackalloc DX.ID3D11Buffer*[RhiLimits.MaxConstantBuffers];
            for (int i = 0; i < n; i++)
                cbs[i] = bindings.ConstantBuffers[i].IsValid
                         ? _dev.GetBuffer(bindings.ConstantBuffers[i]).Native.Handle
                         : null;
            _dev.Context.VSSetConstantBuffers(0u, (uint)n, cbs);
            _dev.Context.PSSetConstantBuffers(0u, (uint)n, cbs);
        }

        // Textures (SRVs).
        if (!bindings.Textures.IsEmpty)
        {
            int n = Math.Min(bindings.Textures.Length, RhiLimits.MaxTextureBindings);
            DX.ID3D11ShaderResourceView** srvs = stackalloc DX.ID3D11ShaderResourceView*[RhiLimits.MaxTextureBindings];
            for (int i = 0; i < n; i++)
                srvs[i] = bindings.Textures[i].IsValid
                          ? _dev.GetTexture(bindings.Textures[i]).Srv.Handle
                          : null;
            _dev.Context.VSSetShaderResources(0u, (uint)n, srvs);
            _dev.Context.PSSetShaderResources(0u, (uint)n, srvs);
        }

        // Samplers.
        if (!bindings.Samplers.IsEmpty)
        {
            int n = Math.Min(bindings.Samplers.Length, RhiLimits.MaxSamplerBindings);
            DX.ID3D11SamplerState** samplers = stackalloc DX.ID3D11SamplerState*[RhiLimits.MaxSamplerBindings];
            for (int i = 0; i < n; i++)
                samplers[i] = bindings.Samplers[i].IsValid
                              ? _dev.GetSampler(bindings.Samplers[i]).Native.Handle
                              : null;
            _dev.Context.VSSetSamplers(0u, (uint)n, samplers);
            _dev.Context.PSSetSamplers(0u, (uint)n, samplers);
        }

        // Storage buffers (structured SRVs) bound as extra SRVs starting after the texture slots.
        if (!bindings.StorageBuffers.IsEmpty)
        {
            int n = Math.Min(bindings.StorageBuffers.Length, RhiLimits.MaxStorageBuffers);
            DX.ID3D11ShaderResourceView** srvs = stackalloc DX.ID3D11ShaderResourceView*[RhiLimits.MaxStorageBuffers];
            for (int i = 0; i < n; i++)
                srvs[i] = bindings.StorageBuffers[i].IsValid
                          ? _dev.GetBuffer(bindings.StorageBuffers[i]).Srv.Handle
                          : null;
            uint startSlot = (uint)RhiLimits.MaxTextureBindings;
            _dev.Context.VSSetShaderResources(startSlot, (uint)n, srvs);
            _dev.Context.PSSetShaderResources(startSlot, (uint)n, srvs);
        }
    }

    public void SetVertexBuffers(ReadOnlySpan<VertexBufferBinding> buffers)
    {
        if (buffers.IsEmpty || _boundPipeline == null)
            return;

        int n = Math.Min(buffers.Length, RhiLimits.MaxVertexBuffers);
        DX.ID3D11Buffer** native = stackalloc DX.ID3D11Buffer*[RhiLimits.MaxVertexBuffers];
        uint* strides            = stackalloc uint[RhiLimits.MaxVertexBuffers];
        uint* offsets            = stackalloc uint[RhiLimits.MaxVertexBuffers];
        for (int i = 0; i < n; i++)
        {
            native[i]  = buffers[i].Buffer.IsValid ? _dev.GetBuffer(buffers[i].Buffer).Native.Handle : null;
            strides[i] = (uint)_boundPipeline.VertexStrides[i];
            offsets[i] = (uint)buffers[i].OffsetBytes;
        }
        _dev.Context.IASetVertexBuffers(0u, (uint)n, native, strides, offsets);
    }

    public void SetIndexBuffer(BufferHandle buffer, IndexFormat format, int offsetBytes = 0)
    {
        var b = buffer.IsValid ? _dev.GetBuffer(buffer) : null;
        _dev.Context.IASetIndexBuffer(
            b != null ? b.Native.Handle : (DX.ID3D11Buffer*)null,
            format == IndexFormat.U16 ? DXGI.Format.FormatR16Uint : DXGI.Format.FormatR32Uint,
            (uint)offsetBytes);
    }

    public void PushConstants(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return;
        if (data.Length > RhiLimits.PushConstantSize)
            throw new ArgumentException($"Push constant payload exceeds {RhiLimits.PushConstantSize} bytes.");

        // Map(WriteDiscard) -> copy -> Unmap.
        DX.MappedSubresource mapped;
        SilkMarshal.ThrowHResult(_dev.Context.Map(
            (DX.ID3D11Resource*)_dev.PushCb.Handle, 0u, DX.Map.WriteDiscard, 0u, &mapped));
        fixed (byte* src = data)
            Buffer.MemoryCopy(src, mapped.PData, RhiLimits.PushConstantSize, data.Length);
        _dev.Context.Unmap((DX.ID3D11Resource*)_dev.PushCb.Handle, 0u);

        // Bind at the *last* cbuffer slot so it never collides with user cbufs at b0..b3.
        DX.ID3D11Buffer** cb = stackalloc DX.ID3D11Buffer*[1] { _dev.PushCb.Handle };
        uint pushSlot = (uint)RhiLimits.MaxConstantBuffers;
        _dev.Context.VSSetConstantBuffers(pushSlot, 1u, cb);
        _dev.Context.PSSetConstantBuffers(pushSlot, 1u, cb);
    }

    public void SetViewport(int x, int y, int width, int height, float minDepth = 0.0f, float maxDepth = 1.0f)
    {
        var vp = new DX.Viewport
        {
            TopLeftX = x, TopLeftY = y,
            Width    = width, Height = height,
            MinDepth = minDepth, MaxDepth = maxDepth,
        };
        _dev.Context.RSSetViewports(1, in vp);
    }

    public void SetScissor(int x, int y, int width, int height)
    {
        // D3D11 RECT layout: { LONG left; LONG top; LONG right; LONG bottom; }
        // Silk binds this as Box2D<int> with Min/Max corners.
        var rect = new Silk.NET.Maths.Box2D<int>(x, y, x + width, y + height);
        _dev.Context.RSSetScissorRects(1u, &rect);
    }

    public void Draw(int vertexCount, int instanceCount = 1, int firstVertex = 0, int firstInstance = 0)
    {
        if (instanceCount <= 1 && firstInstance == 0)
            _dev.Context.Draw((uint)vertexCount, (uint)firstVertex);
        else
            _dev.Context.DrawInstanced((uint)vertexCount, (uint)instanceCount, (uint)firstVertex, (uint)firstInstance);
    }

    public void DrawIndexed(int indexCount, int instanceCount = 1, int firstIndex = 0, int baseVertex = 0, int firstInstance = 0)
    {
        if (instanceCount <= 1 && firstInstance == 0)
            _dev.Context.DrawIndexed((uint)indexCount, (uint)firstIndex, baseVertex);
        else
            _dev.Context.DrawIndexedInstanced((uint)indexCount, (uint)instanceCount, (uint)firstIndex, baseVertex, (uint)firstInstance);
    }

    public void UpdateBuffer(BufferHandle buffer, int offsetBytes, ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return;
        var b = _dev.GetBuffer(buffer);

        if (b.Usage == BufferUsage.DynamicUniform || b.Usage == BufferUsage.DynamicVertex)
        {
            // Map / write / Unmap on dynamic buffers.
            DX.MappedSubresource mapped;
            SilkMarshal.ThrowHResult(_dev.Context.Map(
                (DX.ID3D11Resource*)b.Native.Handle, 0u, DX.Map.WriteDiscard, 0u, &mapped));
            fixed (byte* src = data)
                Buffer.MemoryCopy(src, (byte*)mapped.PData + offsetBytes, b.SizeBytes - offsetBytes, data.Length);
            _dev.Context.Unmap((DX.ID3D11Resource*)b.Native.Handle, 0u);
        }
        else
        {
            // UpdateSubresource for Default/Immutable buffers (Immutable will fail at runtime -- by design).
            var box = new DX.Box
            {
                Left   = (uint)offsetBytes,
                Right  = (uint)(offsetBytes + data.Length),
                Top    = 0, Bottom = 1,
                Front  = 0, Back   = 1,
            };
            fixed (byte* src = data)
                _dev.Context.UpdateSubresource(
                    (DX.ID3D11Resource*)b.Native.Handle, 0u, &box, src, 0u, 0u);
        }
    }

    public void PushDebugGroup(string name)
    {
        // ID3DUserDefinedAnnotation hookup is optional; no-op for the first pass.
        _ = name;
    }

    public void PopDebugGroup() { /* no-op */ }
}
