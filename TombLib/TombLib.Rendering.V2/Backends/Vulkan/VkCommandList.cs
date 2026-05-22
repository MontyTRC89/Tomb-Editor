using System;
using System.Numerics;
using Silk.NET.Vulkan;
using TombLib.RenderingV2.Rhi;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace TombLib.RenderingV2.Backends.Vulkan;

/// <summary>
/// Records commands into the device's single per-frame command buffer.
/// One-shot — instantiated by <see cref="VkDevice.BeginCommandList"/> and
/// finalised by <see cref="VkDevice.Submit"/>.
/// </summary>
public unsafe sealed class VkCommandList : ICommandList
{
    private readonly VkDevice _dev;
    private readonly CommandBuffer _cmd;

    private VkPipelineRes? _boundPipeline;
    private VkSwapchainRes? _passSwapchain;
    private bool _passActive;

    // Last-set bindings — used to allocate descriptor sets on demand.
    // Stored as plain handles since the spans in `Bindings` are stack-only.
    private readonly BufferHandle[]  _cbufs   = new BufferHandle[RhiLimits.MaxConstantBuffers];
    private int _cbufCount;
    private readonly TextureHandle[] _texs    = new TextureHandle[RhiLimits.MaxTextureBindings];
    private int _texCount;
    private readonly SamplerHandle[] _sampls  = new SamplerHandle[RhiLimits.MaxSamplerBindings];
    private int _samplCount;
    private readonly BufferHandle[]  _ssbos   = new BufferHandle[RhiLimits.MaxStorageBuffers];
    private int _ssboCount;
    private bool _bindingsDirty;

    /// <summary>Swapchain acquired during this frame (for Submit semaphore wait + Present sync).</summary>
    internal VkSwapchainRes? AcquiredSwapchain;

    internal VkCommandList(VkDevice dev, CommandBuffer cmd)
    {
        _dev = dev; _cmd = cmd;
    }

    internal void Finish()
    {
        if (_passActive) EndPass();
    }

    // ============================================================ Pass

    public void BeginPass(in PassDesc desc)
    {
        if (!desc.UseSwapchain)
            throw new NotImplementedException("Vulkan backend: only UseSwapchain BeginPass is wired right now.");

        var sc = _dev.Swapchains[desc.Swapchain.Id];
        _passSwapchain = sc;

        // Acquire the next swapchain image (signals ImageAvailable semaphore).
        if (!sc.ImageAcquired)
        {
            uint idx = 0;
            _dev.KhrSwapchain.AcquireNextImage(_dev.Device, sc.SwapchainHandle, ulong.MaxValue,
                                                _dev.ImageAvailable, default, &idx);
            sc.CurrentImageIndex = idx;
            sc.ImageAcquired = true;
            AcquiredSwapchain = sc;
        }

        int width  = desc.ViewportWidth  > 0 ? desc.ViewportWidth  : sc.Width;
        int height = desc.ViewportHeight > 0 ? desc.ViewportHeight : sc.Height;

        // Clear values are indexed by attachment. Attachment order matches
        // GetOrCreateRenderPass: [0] colour, [1] depth, and — when the
        // swapchain is multisampled — [2] the resolve target (its load op is
        // DontCare so the value is unused, but the array must still cover it).
        int attachCount = sc.Samples > 1 ? 3 : 2;
        var clears = stackalloc ClearValue[3];
        Vector4 c = (desc.ClearColors != null && desc.ClearColors.Length > 0) ? desc.ClearColors[0] : default;
        clears[0] = new ClearValue { Color = new ClearColorValue(c.X, c.Y, c.Z, c.W) };
        clears[1] = new ClearValue { DepthStencil = new ClearDepthStencilValue(desc.ClearDepth, desc.ClearStencil) };
        clears[2] = default;

        var rpbi = new RenderPassBeginInfo
        {
            SType            = StructureType.RenderPassBeginInfo,
            RenderPass       = sc.RenderPass,
            Framebuffer      = sc.Framebuffers[sc.CurrentImageIndex],
            RenderArea       = new Rect2D(new Offset2D(desc.ViewportX, desc.ViewportY), new Extent2D((uint)width, (uint)height)),
            ClearValueCount  = (uint)attachCount,
            PClearValues     = clears,
        };
        _dev.Api.CmdBeginRenderPass(_cmd, in rpbi, SubpassContents.Inline);
        _passActive = true;

        // Default viewport + scissor cover the pass area; pipelines declared
        // viewport / scissor as dynamic state.
        SetViewport(desc.ViewportX, desc.ViewportY, width, height);
        SetScissor(desc.ViewportX, desc.ViewportY, width, height);
    }

    public void EndPass()
    {
        if (!_passActive) return;
        _dev.Api.CmdEndRenderPass(_cmd);
        _passActive = false;
        _boundPipeline = null;
        _passSwapchain = null;
    }

    // ============================================================ Pipeline / bindings

    public void SetPipeline(PipelineHandle pipeline)
    {
        var p = _dev.Pipelines[pipeline.Id];
        _boundPipeline = p;
        _dev.Api.CmdBindPipeline(_cmd, PipelineBindPoint.Graphics, p.Handle);
        _bindingsDirty = true;
    }

    public void SetBindings(in Bindings bindings)
    {
        // Copy the spans into our private arrays so EnsureDescriptorSet can
        // re-use them (the input spans are stack-only).
        _cbufCount = Math.Min(bindings.ConstantBuffers.Length, RhiLimits.MaxConstantBuffers);
        for (int i = 0; i < _cbufCount; i++) _cbufs[i] = bindings.ConstantBuffers[i];
        _texCount = Math.Min(bindings.Textures.Length, RhiLimits.MaxTextureBindings);
        for (int i = 0; i < _texCount; i++) _texs[i] = bindings.Textures[i];
        _samplCount = Math.Min(bindings.Samplers.Length, RhiLimits.MaxSamplerBindings);
        for (int i = 0; i < _samplCount; i++) _sampls[i] = bindings.Samplers[i];
        _ssboCount = Math.Min(bindings.StorageBuffers.Length, RhiLimits.MaxStorageBuffers);
        for (int i = 0; i < _ssboCount; i++) _ssbos[i] = bindings.StorageBuffers[i];
        _bindingsDirty = true;
    }

    private void EnsureDescriptorSet()
    {
        if (!_bindingsDirty || _boundPipeline == null) return;
        _bindingsDirty = false;

        // Allocate a fresh descriptor set from the per-frame transient pool.
        // The pool was reset at BeginCommandList, so allocations always
        // succeed up to the pool's MaxSets capacity.
        var dsl = _dev.SharedDescLayout;
        var dai = new DescriptorSetAllocateInfo
        {
            SType              = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool     = _dev.TransientDescPool,
            DescriptorSetCount = 1,
            PSetLayouts        = &dsl,
        };
        DescriptorSet set;
        _dev.Api.AllocateDescriptorSets(_dev.Device, in dai, &set);

        // Layout binding numbers match the HLSL VK_BINDING values:
        //   binding 0 = ConstantBuffers[0]   (ViewParams cbuf)
        //   binding 1 = Textures[0]          (Atlas)
        //   binding 2 = Samplers[0]          (AtlasSamp)
        // Bindings beyond slot 0 of each category are ignored for now — if a
        // shader needs them, both the VK_BINDING decoration and this
        // mapping must grow together.
        var writes  = stackalloc WriteDescriptorSet[3];
        DescriptorBufferInfo bInfo = default;
        DescriptorImageInfo  tInfo = default;
        DescriptorImageInfo  sInfo = default;
        int wi = 0;

        if (_cbufCount > 0 && _cbufs[0].IsValid)
        {
            var b = _dev.Buffers[_cbufs[0].Id];
            bInfo = new DescriptorBufferInfo { Buffer = b.Handle, Offset = 0, Range = b.Size };
            writes[wi++] = new WriteDescriptorSet
            {
                SType           = StructureType.WriteDescriptorSet,
                DstSet          = set,
                DstBinding      = 0,
                DescriptorType  = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                PBufferInfo     = &bInfo,
            };
        }
        if (_texCount > 0 && _texs[0].IsValid)
        {
            var t = _dev.Textures[_texs[0].Id];
            tInfo = new DescriptorImageInfo
            {
                ImageView   = t.View,
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
            };
            writes[wi++] = new WriteDescriptorSet
            {
                SType           = StructureType.WriteDescriptorSet,
                DstSet          = set,
                DstBinding      = 1,
                DescriptorType  = DescriptorType.SampledImage,
                DescriptorCount = 1,
                PImageInfo      = &tInfo,
            };
        }
        if (_samplCount > 0 && _sampls[0].IsValid)
        {
            var s = _dev.Samplers[_sampls[0].Id];
            sInfo = new DescriptorImageInfo { Sampler = s.Handle };
            writes[wi++] = new WriteDescriptorSet
            {
                SType           = StructureType.WriteDescriptorSet,
                DstSet          = set,
                DstBinding      = 2,
                DescriptorType  = DescriptorType.Sampler,
                DescriptorCount = 1,
                PImageInfo      = &sInfo,
            };
        }

        if (wi > 0)
            _dev.Api.UpdateDescriptorSets(_dev.Device, (uint)wi, writes, 0, null);
        _dev.Api.CmdBindDescriptorSets(_cmd, PipelineBindPoint.Graphics, _dev.SharedPipelineLayout,
                                        0, 1, in set, 0, null);
    }

    public void SetVertexBuffers(ReadOnlySpan<VertexBufferBinding> buffers)
    {
        if (buffers.IsEmpty || _boundPipeline == null) return;
        int n = Math.Min(buffers.Length, RhiLimits.MaxVertexBuffers);
        var vbs     = stackalloc VkBuffer[RhiLimits.MaxVertexBuffers];
        var offsets = stackalloc ulong[RhiLimits.MaxVertexBuffers];
        for (int i = 0; i < n; i++)
        {
            vbs[i]     = buffers[i].Buffer.IsValid ? _dev.Buffers[buffers[i].Buffer.Id].Handle : default;
            offsets[i] = (ulong)buffers[i].OffsetBytes;
        }
        _dev.Api.CmdBindVertexBuffers(_cmd, 0, (uint)n, vbs, offsets);
    }

    public void SetIndexBuffer(BufferHandle buffer, IndexFormat format, int offsetBytes = 0)
    {
        if (!buffer.IsValid) return;
        var b = _dev.Buffers[buffer.Id];
        _dev.Api.CmdBindIndexBuffer(_cmd, b.Handle, (ulong)offsetBytes,
                                     format == IndexFormat.U16 ? IndexType.Uint16 : IndexType.Uint32);
    }

    public void PushConstants(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty || _boundPipeline == null) return;
        fixed (byte* p = data)
            _dev.Api.CmdPushConstants(_cmd, _dev.SharedPipelineLayout,
                                       ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
                                       0, (uint)data.Length, p);
    }

    public void SetViewport(int x, int y, int width, int height, float minDepth = 0f, float maxDepth = 1f)
    {
        // Y-axis compensation is done at SPIR-V level via the DXC
        // `-fvk-invert-y` build flag, so the viewport stays in standard
        // Vulkan orientation (Y down, positive height). This keeps the
        // triangle winding the same as DX11, so CullMode.Back +
        // FrontFace.Clockwise work without inversion.
        var vp = new Viewport
        {
            X = x, Y = y,
            Width = width, Height = height,
            MinDepth = minDepth, MaxDepth = maxDepth,
        };
        _dev.Api.CmdSetViewport(_cmd, 0, 1, in vp);
    }

    public void SetScissor(int x, int y, int width, int height)
    {
        var r = new Rect2D(new Offset2D(x, y), new Extent2D((uint)width, (uint)height));
        _dev.Api.CmdSetScissor(_cmd, 0, 1, in r);
    }

    public void Draw(int vertexCount, int instanceCount = 1, int firstVertex = 0, int firstInstance = 0)
    {
        EnsureDescriptorSet();
        _dev.Api.CmdDraw(_cmd, (uint)vertexCount, (uint)Math.Max(1, instanceCount), (uint)firstVertex, (uint)firstInstance);
    }

    public void DrawIndexed(int indexCount, int instanceCount = 1, int firstIndex = 0, int baseVertex = 0, int firstInstance = 0)
    {
        EnsureDescriptorSet();
        _dev.Api.CmdDrawIndexed(_cmd, (uint)indexCount, (uint)Math.Max(1, instanceCount),
                                 (uint)firstIndex, baseVertex, (uint)firstInstance);
    }

    public void UpdateBuffer(BufferHandle buffer, int offsetBytes, ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) return;
        var b = _dev.Buffers[buffer.Id];

        // Dynamic buffers: write directly into the persistently-mapped region.
        // No barrier needed because HOST_COHERENT memory is visible to GPU
        // without explicit flush. Caller is expected to call UpdateBuffer
        // outside an active render pass — but since dynamic vertex buffers
        // are typically written then used in the same pass, we tolerate it
        // for those (mapping is unaffected by the render pass).
        if (b.Mapped != null)
        {
            fixed (byte* src = data)
                System.Buffer.MemoryCopy(src, (byte*)b.Mapped + offsetBytes,
                                          (long)b.Size - offsetBytes, data.Length);
            return;
        }

        // Static (DEVICE_LOCAL) buffer: cmdUpdateBuffer is limited to 65536
        // bytes and only valid outside a render pass. For larger updates the
        // caller should use a staging buffer at creation time.
        if (data.Length > 65536)
            throw new InvalidOperationException(
                "UpdateBuffer on a non-dynamic buffer is limited to 65536 bytes (use Immutable + create-time data for larger).");
        if (_passActive)
            throw new InvalidOperationException("UpdateBuffer on a non-dynamic buffer cannot be called inside a render pass.");
        fixed (byte* src = data)
            _dev.Api.CmdUpdateBuffer(_cmd, b.Handle, (ulong)offsetBytes, (ulong)data.Length, src);
    }

    public void PushDebugGroup(string name) { /* no-op (debug-utils integration TBD) */ }
    public void PopDebugGroup() { /* no-op */ }
}
