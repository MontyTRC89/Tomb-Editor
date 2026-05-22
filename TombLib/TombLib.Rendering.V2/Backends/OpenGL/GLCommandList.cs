using System;
using System.Numerics;
using Silk.NET.OpenGL;
using TombLib.RenderingV2.Rhi;

namespace TombLib.RenderingV2.Backends.OpenGL;

/// <summary>
/// Immediate-mode command "list" — every method directly issues glXxx calls
/// on the active GL context. Kept for API parity with the deferred backends
/// (DX11 deferred context, Vulkan command buffer).
/// </summary>
public unsafe sealed class GLCommandList : ICommandList
{
    private readonly GLDevice _device;
    private GLPipelineRes?  _boundPipeline;
    private GLSwapchainRes? _passSwapchain;

    // Index-buffer state captured by SetIndexBuffer, consumed by DrawIndexed.
    private IndexFormat _indexFormat;
    private int         _indexOffset;

    internal GLCommandList(GLDevice device) => _device = device;

    public void BeginPass(in PassDesc desc)
    {
        if (!desc.UseSwapchain)
            throw new NotImplementedException("GL backend: only UseSwapchain BeginPass is wired right now.");

        _passSwapchain = _device.Swapchains[desc.Swapchain.Id];

        // Make the GL context current on THIS swapchain's HDC. Without this
        // every glXxx call goes to whatever HDC was last bound — typically the
        // most-recently-created swapchain (often an item-preview panel), not
        // the one the caller actually wants to draw into.
        if (_passSwapchain.Hdc != IntPtr.Zero)
            GLDevice.WglMakeCurrentExt(_passSwapchain.Hdc, _device.Hglrc);

        int width  = desc.ViewportWidth  > 0 ? desc.ViewportWidth  : _passSwapchain.Width;
        int height = desc.ViewportHeight > 0 ? desc.ViewportHeight : _passSwapchain.Height;

        _device.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        // GL's viewport origin is bottom-left; the renderer passes top-left
        // coordinates in PassDesc. The shader Y-flip (done in the SPIRV-Cross
        // GLSL output, `_40.y = -_40.y`) makes the rendered image come out
        // correct on screen without flipping the viewport here.
        _device.Gl.Viewport(desc.ViewportX, desc.ViewportY, (uint)width, (uint)height);
        _device.Gl.Scissor(desc.ViewportX, desc.ViewportY, (uint)width, (uint)height);

        // ---- Clears ----
        bool clearColor = desc.ColorLoadOps is { Length: > 0 } && desc.ColorLoadOps[0] == LoadOp.Clear;
        if (clearColor)
        {
            Vector4 color = desc.ClearColors is { Length: > 0 } ? desc.ClearColors[0] : default;
            _device.Gl.ClearColor(color.X, color.Y, color.Z, color.W);
        }

        ClearBufferMask clearMask = 0;
        if (clearColor)
            clearMask |= ClearBufferMask.ColorBufferBit;
        if (desc.DepthLoadOp == LoadOp.Clear)
        {
            _device.Gl.ClearDepth(desc.ClearDepth);
            _device.Gl.ClearStencil(desc.ClearStencil);
            // GL requires depth writes to be enabled for a Clear to affect the
            // depth buffer. Toggle it on here; the bound pipeline's depth-write
            // mask gets re-applied by SetPipeline.
            _device.Gl.DepthMask(true);
            clearMask |= ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit;
        }
        if (clearMask != 0)
            _device.Gl.Clear(clearMask);
    }

    public void EndPass()
    {
        _passSwapchain = null;
        _boundPipeline = null;
    }

    public void SetPipeline(PipelineHandle pipeline)
    {
        var pipelineRes = _device.Pipelines[pipeline.Id];
        _boundPipeline  = pipelineRes;
        _device.Gl.UseProgram(pipelineRes.Program);
        _device.Gl.BindVertexArray(pipelineRes.Vao);

        // Depth.
        if (pipelineRes.DepthTest) _device.Gl.Enable(EnableCap.DepthTest);
        else                       _device.Gl.Disable(EnableCap.DepthTest);
        _device.Gl.DepthMask(pipelineRes.DepthWrite);
        _device.Gl.DepthFunc(pipelineRes.DepthFunc);

        // Cull.
        if (pipelineRes.CullEnable)
        {
            _device.Gl.Enable(EnableCap.CullFace);
            _device.Gl.CullFace(pipelineRes.CullMode == CullModeMaterial.Back
                                ? TriangleFace.Back : TriangleFace.Front);
        }
        else
        {
            _device.Gl.Disable(EnableCap.CullFace);
        }
        _device.Gl.FrontFace(pipelineRes.FrontFace);
        _device.Gl.PolygonMode(TriangleFace.FrontAndBack, pipelineRes.PolygonMode);

        // Blend.
        if (pipelineRes.BlendEnable)
        {
            _device.Gl.Enable(EnableCap.Blend);
            _device.Gl.BlendFuncSeparate(pipelineRes.BlendSrcRgb, pipelineRes.BlendDstRgb,
                                         pipelineRes.BlendSrcA,   pipelineRes.BlendDstA);
            _device.Gl.BlendEquationSeparate(pipelineRes.BlendOpRgb, pipelineRes.BlendOpA);
        }
        else
        {
            _device.Gl.Disable(EnableCap.Blend);
        }

        // Scissor.
        if (pipelineRes.ScissorEnable) _device.Gl.Enable(EnableCap.ScissorTest);
        else                           _device.Gl.Disable(EnableCap.ScissorTest);
    }

    public void SetBindings(in Bindings bindings)
    {
        // Constant buffers → glBindBufferBase(GL_UNIFORM_BUFFER, slot, buffer).
        if (!bindings.ConstantBuffers.IsEmpty)
        {
            for (int i = 0; i < bindings.ConstantBuffers.Length && i < RhiLimits.MaxConstantBuffers; i++)
            {
                if (!bindings.ConstantBuffers[i].IsValid) continue;
                var buffer = _device.Buffers[bindings.ConstantBuffers[i].Id];
                _device.Gl.BindBufferBase(BufferTargetARB.UniformBuffer, (uint)i, buffer.Handle);
            }
        }

        // Textures → glBindTextureUnit(unit, texture).
        if (!bindings.Textures.IsEmpty)
        {
            for (int i = 0; i < bindings.Textures.Length && i < RhiLimits.MaxTextureBindings; i++)
            {
                if (!bindings.Textures[i].IsValid) continue;
                var texture = _device.Textures[bindings.Textures[i].Id];
                _device.Gl.BindTextureUnit((uint)i, texture.Handle);
            }
        }

        // Samplers → glBindSampler(unit, sampler). Sampler[i] binds to texture
        // unit i — matching CreatePipeline, where the first sampler uniform is
        // assigned to unit 0, the second to unit 1, etc. The HLSL→SPIR-V→GLSL
        // pipeline produces one combined sampler per texture, so a 1:1 mapping
        // is correct.
        if (!bindings.Samplers.IsEmpty)
        {
            for (int i = 0; i < bindings.Samplers.Length && i < RhiLimits.MaxSamplerBindings; i++)
            {
                if (!bindings.Samplers[i].IsValid) continue;
                var sampler = _device.Samplers[bindings.Samplers[i].Id];
                _device.Gl.BindSampler((uint)i, sampler.Handle);
            }
        }
    }

    public void SetVertexBuffers(ReadOnlySpan<VertexBufferBinding> buffers)
    {
        if (buffers.IsEmpty || _boundPipeline == null) return;
        for (int i = 0; i < buffers.Length && i < RhiLimits.MaxVertexBuffers; i++)
        {
            var binding = buffers[i];
            uint handle = binding.Buffer.IsValid ? _device.Buffers[binding.Buffer.Id].Handle : 0;
            int  stride = i < _boundPipeline.VertexStrides.Length ? _boundPipeline.VertexStrides[i] : 0;
            _device.Gl.VertexArrayVertexBuffer(_boundPipeline.Vao, (uint)i, handle, binding.OffsetBytes, (uint)stride);
        }
    }

    public void SetIndexBuffer(BufferHandle buffer, IndexFormat format, int offsetBytes = 0)
    {
        if (_boundPipeline == null) return;
        uint handle = buffer.IsValid ? _device.Buffers[buffer.Id].Handle : 0;
        _device.Gl.VertexArrayElementBuffer(_boundPipeline.Vao, handle);
        _indexFormat = format;
        _indexOffset = offsetBytes;
    }

    public void PushConstants(ReadOnlySpan<byte> data)
    {
        // The GL backend doesn't emulate push constants — the V2 renderer
        // never calls PushConstants (it uses uniform buffers instead), so this
        // stays a no-op until it's actually needed.
        _ = data;
    }

    public void SetViewport(int x, int y, int width, int height, float minDepth = 0f, float maxDepth = 1f)
    {
        _device.Gl.Viewport(x, y, (uint)width, (uint)height);
        _device.Gl.DepthRange(minDepth, maxDepth);
    }

    public void SetScissor(int x, int y, int width, int height)
    {
        _device.Gl.Scissor(x, y, (uint)width, (uint)height);
    }

    public void Draw(int vertexCount, int instanceCount = 1, int firstVertex = 0, int firstInstance = 0)
    {
        if (_boundPipeline == null) return;
        if (instanceCount <= 1 && firstInstance == 0)
        {
            _device.Gl.DrawArrays(_boundPipeline.GlTopology, firstVertex, (uint)vertexCount);
        }
        else
        {
            _device.Gl.DrawArraysInstancedBaseInstance(_boundPipeline.GlTopology, firstVertex,
                (uint)vertexCount, (uint)instanceCount, (uint)firstInstance);
        }
    }

    public void DrawIndexed(int indexCount, int instanceCount = 1, int firstIndex = 0,
                            int baseVertex = 0, int firstInstance = 0)
    {
        if (_boundPipeline == null) return;
        var indexType = GLMapping.ToGl(_indexFormat);
        int indexSize = _indexFormat == IndexFormat.U16 ? 2 : 4;
        nint offset   = _indexOffset + firstIndex * indexSize;
        if (instanceCount <= 1 && firstInstance == 0 && baseVertex == 0)
        {
            _device.Gl.DrawElements(_boundPipeline.GlTopology, (uint)indexCount, indexType, (void*)offset);
        }
        else
        {
            _device.Gl.DrawElementsInstancedBaseVertexBaseInstance(_boundPipeline.GlTopology, (uint)indexCount,
                indexType, (void*)offset, (uint)instanceCount, baseVertex, (uint)firstInstance);
        }
    }

    public void UpdateBuffer(BufferHandle buffer, int offsetBytes, ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) return;
        var bufferRes = _device.Buffers[buffer.Id];
        // glNamedBufferSubData is a synchronous, explicit upload — the driver
        // handles GPU synchronisation for us, so it avoids the stale-frame
        // issues that persistent COHERENT mapping caused.
        fixed (byte* src = data)
            _device.Gl.NamedBufferSubData(bufferRes.Handle, offsetBytes, (nuint)data.Length, src);
    }

    public void PushDebugGroup(string name)
        => _device.Gl.PushDebugGroup(DebugSource.DebugSourceApplication, 0, (uint)name.Length, name);

    public void PopDebugGroup()
        => _device.Gl.PopDebugGroup();
}
