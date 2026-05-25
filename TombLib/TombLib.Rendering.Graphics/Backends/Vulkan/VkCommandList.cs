using System;
using System.Numerics;
using Silk.NET.Vulkan;
using TombLib.Rendering.Graphics.Rhi;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace TombLib.Rendering.Graphics.Backends.Vulkan;

/// <summary>
/// Records commands into the device's single per-frame command buffer.
/// One-shot -- instantiated by <see cref="VkDevice.BeginCommandList"/> and
/// finalised by <see cref="VkDevice.Submit"/>.
///
/// <para>The backend records into one shared command buffer, so only one
/// command list can be live at a time. When a new <see cref="VkDevice.BeginCommandList"/>
/// supersedes this one -- typically a re-entrant render (an offscreen preview
/// drawn during the main viewport's frame) -- the device marks the old list
/// <see cref="Disowned"/>. A disowned list's methods all become no-ops: its
/// command buffer has been reset under it, so recording into it (e.g.
/// <c>vkCmdEndRenderPass</c> with no active pass) would crash the driver.</para>
/// </summary>
public unsafe sealed class VkCommandList : ICommandList
{
    private readonly VkDevice      _device;
    private readonly CommandBuffer _commandBuffer;

    private VkPipelineRes?  _boundPipeline;
    private VkSwapchainRes? _passSwapchain;
    private bool            _passActive;

    /// <summary>
    /// Set by the device once a newer command list has taken over the shared
    /// command buffer. Every method below early-outs while this is true.
    /// </summary>
    internal bool Disowned;

    // Last-set bindings -- used to allocate descriptor sets on demand. Stored
    // as plain handle arrays because the spans in `Bindings` are stack-only.
    private readonly BufferHandle[]  _constantBuffers = new BufferHandle[RhiLimits.MaxConstantBuffers];
    private readonly TextureHandle[] _textures        = new TextureHandle[RhiLimits.MaxTextureBindings];
    private readonly SamplerHandle[] _samplers        = new SamplerHandle[RhiLimits.MaxSamplerBindings];
    private readonly BufferHandle[]  _storageBuffers  = new BufferHandle[RhiLimits.MaxStorageBuffers];
    private int  _constantBufferCount;
    private int  _textureCount;
    private int  _samplerCount;
    private int  _storageBufferCount;
    private bool _bindingsDirty;

    /// <summary>Swapchain acquired this frame (for Submit's semaphore wait + Present sync).</summary>
    internal VkSwapchainRes? AcquiredSwapchain;

    internal VkCommandList(VkDevice device, CommandBuffer commandBuffer)
    {
        _device        = device;
        _commandBuffer = commandBuffer;
    }

    internal void Finish()
    {
        if (Disowned)
            return;
        if (_passActive) EndPass();
    }

    // ==================================================================== Pass

    public void BeginPass(in PassDesc desc)
    {
        if (Disowned)
            return;

        if (!desc.UseSwapchain)
            throw new NotImplementedException("Vulkan backend: only UseSwapchain BeginPass is wired right now.");

        var swapchain = _device.Swapchains[desc.Swapchain.Id];
        _passSwapchain = swapchain;

        // This command list renders into `swapchain` whether or not it
        // acquires the image now -- a previous abandoned frame may have left
        // one already acquired, and Submit still needs to present it.
        AcquiredSwapchain = swapchain;
        if (!swapchain.ImageAcquired)
        {
            // Acquire the next swapchain image (signals ImageAvailable).
            uint imageIndex = 0;
            _device.KhrSwapchain.AcquireNextImage(_device.Device, swapchain.SwapchainHandle, ulong.MaxValue,
                                                  _device.ImageAvailable, default, &imageIndex);
            swapchain.CurrentImageIndex = imageIndex;
            swapchain.ImageAcquired     = true;
        }

        int width  = desc.ViewportWidth  > 0 ? desc.ViewportWidth  : swapchain.Width;
        int height = desc.ViewportHeight > 0 ? desc.ViewportHeight : swapchain.Height;

        // Clear values are indexed by attachment. Attachment order matches
        // GetOrCreateRenderPass: [0] colour, [1] depth, and -- when the
        // swapchain is multisampled -- [2] the resolve target (its load op is
        // DontCare so the value is unused, but the array must still cover it).
        int attachmentCount = swapchain.Samples > 1 ? 3 : 2;
        var clearValues = stackalloc ClearValue[3];
        Vector4 clearColor = desc.ClearColors is { Length: > 0 } ? desc.ClearColors[0] : default;
        clearValues[0] = new ClearValue { Color = new ClearColorValue(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W) };
        clearValues[1] = new ClearValue { DepthStencil = new ClearDepthStencilValue(desc.ClearDepth, desc.ClearStencil) };
        clearValues[2] = default;

        var passBeginInfo = new RenderPassBeginInfo
        {
            SType           = StructureType.RenderPassBeginInfo,
            RenderPass      = swapchain.RenderPass,
            Framebuffer     = swapchain.Framebuffers[swapchain.CurrentImageIndex],
            RenderArea      = new Rect2D(new Offset2D(desc.ViewportX, desc.ViewportY),
                                         new Extent2D((uint)width, (uint)height)),
            ClearValueCount = (uint)attachmentCount,
            PClearValues    = clearValues,
        };
        _device.Api.CmdBeginRenderPass(_commandBuffer, in passBeginInfo, SubpassContents.Inline);
        _passActive = true;

        // Default viewport + scissor cover the pass area; pipelines declare
        // viewport / scissor as dynamic state.
        SetViewport(desc.ViewportX, desc.ViewportY, width, height);
        SetScissor(desc.ViewportX, desc.ViewportY, width, height);
    }

    public void EndPass()
    {
        if (Disowned || !_passActive)
            return;
        _device.Api.CmdEndRenderPass(_commandBuffer);
        _passActive    = false;
        _boundPipeline = null;
        _passSwapchain = null;
    }

    // =================================================== Pipeline / bindings

    public void SetPipeline(PipelineHandle pipeline)
    {
        if (Disowned)
            return;
        var pipelineRes = _device.Pipelines[pipeline.Id];
        _boundPipeline  = pipelineRes;
        _device.Api.CmdBindPipeline(_commandBuffer, PipelineBindPoint.Graphics, pipelineRes.Handle);
        _bindingsDirty = true;
    }

    public void SetBindings(in Bindings bindings)
    {
        if (Disowned)
            return;

        // Copy the spans into our private arrays so EnsureDescriptorSet can
        // reuse them later (the input spans are stack-only).
        _constantBufferCount = Math.Min(bindings.ConstantBuffers.Length, RhiLimits.MaxConstantBuffers);
        for (int i = 0; i < _constantBufferCount; i++) _constantBuffers[i] = bindings.ConstantBuffers[i];

        _textureCount = Math.Min(bindings.Textures.Length, RhiLimits.MaxTextureBindings);
        for (int i = 0; i < _textureCount; i++) _textures[i] = bindings.Textures[i];

        _samplerCount = Math.Min(bindings.Samplers.Length, RhiLimits.MaxSamplerBindings);
        for (int i = 0; i < _samplerCount; i++) _samplers[i] = bindings.Samplers[i];

        _storageBufferCount = Math.Min(bindings.StorageBuffers.Length, RhiLimits.MaxStorageBuffers);
        for (int i = 0; i < _storageBufferCount; i++) _storageBuffers[i] = bindings.StorageBuffers[i];

        _bindingsDirty = true;
    }

    private void EnsureDescriptorSet()
    {
        if (!_bindingsDirty || _boundPipeline == null)
            return;
        _bindingsDirty = false;

        // Allocate a fresh descriptor set from the per-frame transient pool.
        // The pool was reset at BeginCommandList, so allocations always
        // succeed up to the pool's MaxSets capacity.
        var setLayout = _device.SharedDescLayout;
        var allocInfo = new DescriptorSetAllocateInfo
        {
            SType              = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool     = _device.TransientDescPool,
            DescriptorSetCount = 1,
            PSetLayouts        = &setLayout,
        };
        DescriptorSet set;
        _device.Api.AllocateDescriptorSets(_device.Device, in allocInfo, &set);

        // Layout binding numbers match the HLSL VK_BINDING values:
        //   binding 0 = ConstantBuffers[0]  (ViewParams cbuf)
        //   binding 1 = Textures[0]         (Atlas)
        //   binding 2 = Samplers[0]         (AtlasSamp)
        // Bindings beyond slot 0 of each category are ignored for now -- if a
        // shader needs them, both the VK_BINDING decoration and this mapping
        // must grow together.
        var writes = stackalloc WriteDescriptorSet[3];
        DescriptorBufferInfo bufferInfo  = default;
        DescriptorImageInfo  imageInfo   = default;
        DescriptorImageInfo  samplerInfo = default;
        int writeCount = 0;

        if (_constantBufferCount > 0 && _constantBuffers[0].IsValid)
        {
            var buffer = _device.Buffers[_constantBuffers[0].Id];
            bufferInfo = new DescriptorBufferInfo { Buffer = buffer.Handle, Offset = 0, Range = buffer.Size };
            writes[writeCount++] = new WriteDescriptorSet
            {
                SType           = StructureType.WriteDescriptorSet,
                DstSet          = set,
                DstBinding      = 0,
                DescriptorType  = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                PBufferInfo     = &bufferInfo,
            };
        }
        if (_textureCount > 0 && _textures[0].IsValid)
        {
            var texture = _device.Textures[_textures[0].Id];
            imageInfo = new DescriptorImageInfo
            {
                ImageView   = texture.View,
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
            };
            writes[writeCount++] = new WriteDescriptorSet
            {
                SType           = StructureType.WriteDescriptorSet,
                DstSet          = set,
                DstBinding      = 1,
                DescriptorType  = DescriptorType.SampledImage,
                DescriptorCount = 1,
                PImageInfo      = &imageInfo,
            };
        }
        if (_samplerCount > 0 && _samplers[0].IsValid)
        {
            var sampler = _device.Samplers[_samplers[0].Id];
            samplerInfo = new DescriptorImageInfo { Sampler = sampler.Handle };
            writes[writeCount++] = new WriteDescriptorSet
            {
                SType           = StructureType.WriteDescriptorSet,
                DstSet          = set,
                DstBinding      = 2,
                DescriptorType  = DescriptorType.Sampler,
                DescriptorCount = 1,
                PImageInfo      = &samplerInfo,
            };
        }

        if (writeCount > 0)
            _device.Api.UpdateDescriptorSets(_device.Device, (uint)writeCount, writes, 0, null);
        _device.Api.CmdBindDescriptorSets(_commandBuffer, PipelineBindPoint.Graphics,
                                          _device.SharedPipelineLayout, 0, 1, in set, 0, null);
    }

    public void SetVertexBuffers(ReadOnlySpan<VertexBufferBinding> buffers)
    {
        if (Disowned || buffers.IsEmpty || _boundPipeline == null)
            return;

        int count = Math.Min(buffers.Length, RhiLimits.MaxVertexBuffers);
        var vertexBuffers = stackalloc VkBuffer[RhiLimits.MaxVertexBuffers];
        var offsets       = stackalloc ulong[RhiLimits.MaxVertexBuffers];
        for (int i = 0; i < count; i++)
        {
            vertexBuffers[i] = buffers[i].Buffer.IsValid ? _device.Buffers[buffers[i].Buffer.Id].Handle : default;
            offsets[i]       = (ulong)buffers[i].OffsetBytes;
        }
        _device.Api.CmdBindVertexBuffers(_commandBuffer, 0, (uint)count, vertexBuffers, offsets);
    }

    public void SetIndexBuffer(BufferHandle buffer, IndexFormat format, int offsetBytes = 0)
    {
        if (Disowned || !buffer.IsValid)
            return;
        var bufferRes = _device.Buffers[buffer.Id];
        _device.Api.CmdBindIndexBuffer(_commandBuffer, bufferRes.Handle, (ulong)offsetBytes,
                                       format == IndexFormat.U16 ? IndexType.Uint16 : IndexType.Uint32);
    }

    public void PushConstants(ReadOnlySpan<byte> data)
    {
        if (Disowned || data.IsEmpty || _boundPipeline == null)
            return;
        fixed (byte* pData = data)
            _device.Api.CmdPushConstants(_commandBuffer, _device.SharedPipelineLayout,
                                         ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
                                         0, (uint)data.Length, pData);
    }

    public void SetViewport(int x, int y, int width, int height, float minDepth = 0.0f, float maxDepth = 1.0f)
    {
        if (Disowned)
            return;

        // Y-axis compensation is done at SPIR-V level via the DXC
        // `-fvk-invert-y` build flag, so the viewport stays in standard
        // Vulkan orientation (Y down, positive height). This keeps the
        // triangle winding the same as DX11, so CullMode.Back +
        // FrontFace.Clockwise work without inversion.
        var viewport = new Viewport
        {
            X        = x,        Y        = y,
            Width    = width,    Height   = height,
            MinDepth = minDepth, MaxDepth = maxDepth,
        };
        _device.Api.CmdSetViewport(_commandBuffer, 0, 1, in viewport);
    }

    public void SetScissor(int x, int y, int width, int height)
    {
        if (Disowned)
            return;
        var scissor = new Rect2D(new Offset2D(x, y), new Extent2D((uint)width, (uint)height));
        _device.Api.CmdSetScissor(_commandBuffer, 0, 1, in scissor);
    }

    public void Draw(int vertexCount, int instanceCount = 1, int firstVertex = 0, int firstInstance = 0)
    {
        if (Disowned)
            return;
        EnsureDescriptorSet();
        _device.Api.CmdDraw(_commandBuffer, (uint)vertexCount, (uint)Math.Max(1, instanceCount),
                            (uint)firstVertex, (uint)firstInstance);
    }

    public void DrawIndexed(int indexCount, int instanceCount = 1, int firstIndex = 0,
                            int baseVertex = 0, int firstInstance = 0)
    {
        if (Disowned)
            return;
        EnsureDescriptorSet();
        _device.Api.CmdDrawIndexed(_commandBuffer, (uint)indexCount, (uint)Math.Max(1, instanceCount),
                                   (uint)firstIndex, baseVertex, (uint)firstInstance);
    }

    public void UpdateBuffer(BufferHandle buffer, int offsetBytes, ReadOnlySpan<byte> data)
    {
        if (Disowned || data.IsEmpty)
            return;
        var bufferRes = _device.Buffers[buffer.Id];

        // Dynamic buffers: write directly into the persistently-mapped region.
        // No barrier needed because HOST_COHERENT memory is visible to the GPU
        // without an explicit flush. Callers are expected to call UpdateBuffer
        // outside an active render pass -- but since dynamic vertex buffers are
        // typically written then used in the same pass, that case is tolerated
        // (mapping is unaffected by the render pass).
        if (bufferRes.Mapped != null)
        {
            fixed (byte* src = data)
                System.Buffer.MemoryCopy(src, (byte*)bufferRes.Mapped + offsetBytes,
                                         (long)bufferRes.Size - offsetBytes, data.Length);
            return;
        }

        // Static (DEVICE_LOCAL) buffer: CmdUpdateBuffer is limited to 65536
        // bytes and is only valid outside a render pass. For larger updates
        // the caller should supply a staging buffer at creation time.
        if (data.Length > 65536)
            throw new InvalidOperationException(
                "UpdateBuffer on a non-dynamic buffer is limited to 65536 bytes (use Immutable + create-time data for larger).");
        if (_passActive)
            throw new InvalidOperationException("UpdateBuffer on a non-dynamic buffer cannot be called inside a render pass.");
        fixed (byte* src = data)
            _device.Api.CmdUpdateBuffer(_commandBuffer, bufferRes.Handle, (ulong)offsetBytes, (ulong)data.Length, src);
    }

    public void PushDebugGroup(string name) { /* no-op (debug-utils integration TBD) */ }
    public void PopDebugGroup()             { /* no-op */ }
}
