using System;
using System.Numerics;
using Silk.NET.OpenGL;
using TombLib.RenderingV2.Rhi;

namespace TombLib.RenderingV2.Backends.OpenGL;

/// <summary>
/// Immediate-mode command "list" — every method directly issues glXxx
/// calls on the active GL context. Kept for API parity with the deferred
/// backends (DX11 deferred context, Vulkan command buffer).
/// </summary>
public unsafe sealed class GLCommandList : ICommandList
{
    private readonly GLDevice _dev;
    private GLPipelineRes? _boundPipeline;
    private GLSwapchainRes? _passSwapchain;

    internal GLCommandList(GLDevice dev) => _dev = dev;

    public void BeginPass(in PassDesc desc)
    {
        if (!desc.UseSwapchain)
            throw new NotImplementedException("GL backend: only UseSwapchain BeginPass is wired right now.");

        _passSwapchain = _dev.Swapchains[desc.Swapchain.Id];

        // Make the GL context current on THIS swapchain's HDC. Without this
        // every glXxx call goes to whatever HDC was last bound — typically
        // the most-recently-created swapchain (often an item-preview panel),
        // not the one the caller actually wants to draw into.
        if (_passSwapchain.Hdc != System.IntPtr.Zero)
            GLDevice.WglMakeCurrentExt(_passSwapchain.Hdc, _dev.Hglrc);

        int w = desc.ViewportWidth  > 0 ? desc.ViewportWidth  : _passSwapchain.Width;
        int h = desc.ViewportHeight > 0 ? desc.ViewportHeight : _passSwapchain.Height;

        _dev.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        // GL viewport origin is bottom-left; the renderer expects top-left
        // coordinates from PassDesc. With our shader Y-flip done via
        // SPIRV-Cross GLSL output (`_40.y = -_40.y`), the rendered image
        // ends up correct on screen without us flipping the viewport here.
        _dev.Gl.Viewport(desc.ViewportX, desc.ViewportY, (uint)w, (uint)h);
        _dev.Gl.Scissor(desc.ViewportX, desc.ViewportY, (uint)w, (uint)h);

        // Clears.
        if (desc.ColorLoadOps != null && desc.ColorLoadOps.Length > 0 && desc.ColorLoadOps[0] == LoadOp.Clear)
        {
            Vector4 c = (desc.ClearColors != null && desc.ClearColors.Length > 0) ? desc.ClearColors[0] : default;
            _dev.Gl.ClearColor(c.X, c.Y, c.Z, c.W);
        }
        ClearBufferMask mask = 0;
        if (desc.ColorLoadOps != null && desc.ColorLoadOps.Length > 0 && desc.ColorLoadOps[0] == LoadOp.Clear)
            mask |= ClearBufferMask.ColorBufferBit;
        if (desc.DepthLoadOp == LoadOp.Clear)
        {
            _dev.Gl.ClearDepth((double)desc.ClearDepth);
            _dev.Gl.ClearStencil(desc.ClearStencil);
            // GL requires depth writes to be enabled for Clear to affect
            // the depth buffer. Toggle on here; the bound pipeline's depth
            // write mask will be re-applied by SetPipeline.
            _dev.Gl.DepthMask(true);
            mask |= ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit;
        }
        if (mask != 0) _dev.Gl.Clear(mask);
    }

    public void EndPass()
    {
        _passSwapchain = null;
        _boundPipeline = null;
    }

    public void SetPipeline(PipelineHandle pipeline)
    {
        var p = _dev.Pipelines[pipeline.Id];
        _boundPipeline = p;
        _dev.Gl.UseProgram(p.Program);
        _dev.Gl.BindVertexArray(p.Vao);

        // Depth.
        if (p.DepthTest) _dev.Gl.Enable(EnableCap.DepthTest);
        else             _dev.Gl.Disable(EnableCap.DepthTest);
        _dev.Gl.DepthMask(p.DepthWrite);
        _dev.Gl.DepthFunc(p.DepthFunc);

        // Cull.
        if (p.CullEnable) _dev.Gl.Enable(EnableCap.CullFace);
        else              _dev.Gl.Disable(EnableCap.CullFace);
        if (p.CullEnable)
            _dev.Gl.CullFace(p.CullMode == CullModeMaterial.Back ? TriangleFace.Back : TriangleFace.Front);
        _dev.Gl.FrontFace(p.FrontFace);
        _dev.Gl.PolygonMode(TriangleFace.FrontAndBack, p.PolygonMode);

        // Blend.
        if (p.BlendEnable)
        {
            _dev.Gl.Enable(EnableCap.Blend);
            _dev.Gl.BlendFuncSeparate(p.BlendSrcRgb, p.BlendDstRgb, p.BlendSrcA, p.BlendDstA);
            _dev.Gl.BlendEquationSeparate(p.BlendOpRgb, p.BlendOpA);
        }
        else
        {
            _dev.Gl.Disable(EnableCap.Blend);
        }

        // Scissor.
        if (p.ScissorEnable) _dev.Gl.Enable(EnableCap.ScissorTest);
        else                 _dev.Gl.Disable(EnableCap.ScissorTest);
    }

    public void SetBindings(in Bindings bindings)
    {
        // Constant buffers → glBindBufferBase(GL_UNIFORM_BUFFER, slot, buffer)
        if (!bindings.ConstantBuffers.IsEmpty)
        {
            for (int i = 0; i < bindings.ConstantBuffers.Length && i < RhiLimits.MaxConstantBuffers; i++)
            {
                if (!bindings.ConstantBuffers[i].IsValid) continue;
                var b = _dev.Buffers[bindings.ConstantBuffers[i].Id];
                _dev.Gl.BindBufferBase(BufferTargetARB.UniformBuffer, (uint)i, b.Handle);
            }
        }

        // Textures → glBindTextureUnit(unit, texture)
        if (!bindings.Textures.IsEmpty)
        {
            for (int i = 0; i < bindings.Textures.Length && i < RhiLimits.MaxTextureBindings; i++)
            {
                if (!bindings.Textures[i].IsValid) continue;
                var t = _dev.Textures[bindings.Textures[i].Id];
                _dev.Gl.BindTextureUnit((uint)i, t.Handle);
            }
        }

        // Samplers → glBindSampler(unit, sampler).
        // We bind sampler[i] to texture unit i, matching the convention in
        // CreatePipeline where the first sampler uniform was assigned to
        // unit 0, the second to unit 1, etc. The HLSL→SPIR-V→GLSL pipeline
        // produces a single combined sampler per texture in the shader,
        // so a 1:1 mapping is correct.
        if (!bindings.Samplers.IsEmpty)
        {
            for (int i = 0; i < bindings.Samplers.Length && i < RhiLimits.MaxSamplerBindings; i++)
            {
                if (!bindings.Samplers[i].IsValid) continue;
                var s = _dev.Samplers[bindings.Samplers[i].Id];
                _dev.Gl.BindSampler((uint)i, s.Handle);
            }
        }
    }

    public void SetVertexBuffers(ReadOnlySpan<VertexBufferBinding> buffers)
    {
        if (buffers.IsEmpty || _boundPipeline == null) return;
        for (int i = 0; i < buffers.Length && i < RhiLimits.MaxVertexBuffers; i++)
        {
            var binding = buffers[i];
            uint handle = binding.Buffer.IsValid ? _dev.Buffers[binding.Buffer.Id].Handle : 0;
            int stride = i < _boundPipeline.VertexStrides.Length ? _boundPipeline.VertexStrides[i] : 0;
            _dev.Gl.VertexArrayVertexBuffer(_boundPipeline.Vao, (uint)i, handle, binding.OffsetBytes, (uint)stride);
        }
    }

    public void SetIndexBuffer(BufferHandle buffer, IndexFormat format, int offsetBytes = 0)
    {
        if (_boundPipeline == null) return;
        if (buffer.IsValid)
        {
            var b = _dev.Buffers[buffer.Id];
            _dev.Gl.VertexArrayElementBuffer(_boundPipeline.Vao, b.Handle);
        }
        else
        {
            _dev.Gl.VertexArrayElementBuffer(_boundPipeline.Vao, 0);
        }
        _passIndexFormat = format;
        _passIndexOffset = offsetBytes;
    }
    private IndexFormat _passIndexFormat;
    private int _passIndexOffset;

    public void PushConstants(ReadOnlySpan<byte> data)
    {
        // GL backend doesn't currently emulate push constants — the V2
        // renderer doesn't call PushConstants (it uses uniform buffers
        // instead), so this stays a no-op until we need it.
        _ = data;
    }

    public void SetViewport(int x, int y, int width, int height, float minDepth = 0f, float maxDepth = 1f)
    {
        _dev.Gl.Viewport(x, y, (uint)width, (uint)height);
        _dev.Gl.DepthRange(minDepth, maxDepth);
    }

    public void SetScissor(int x, int y, int width, int height)
    {
        _dev.Gl.Scissor(x, y, (uint)width, (uint)height);
    }

    public void Draw(int vertexCount, int instanceCount = 1, int firstVertex = 0, int firstInstance = 0)
    {
        if (_boundPipeline == null) return;
        if (instanceCount <= 1 && firstInstance == 0)
            _dev.Gl.DrawArrays(_boundPipeline.GlTopology, firstVertex, (uint)vertexCount);
        else
            _dev.Gl.DrawArraysInstancedBaseInstance(_boundPipeline.GlTopology, firstVertex,
                (uint)vertexCount, (uint)instanceCount, (uint)firstInstance);
    }

    public void DrawIndexed(int indexCount, int instanceCount = 1, int firstIndex = 0, int baseVertex = 0, int firstInstance = 0)
    {
        if (_boundPipeline == null) return;
        var type = GLMapping.ToGl(_passIndexFormat);
        int sizeOfIndex = _passIndexFormat == IndexFormat.U16 ? 2 : 4;
        nint offset = _passIndexOffset + firstIndex * sizeOfIndex;
        if (instanceCount <= 1 && firstInstance == 0 && baseVertex == 0)
            _dev.Gl.DrawElements(_boundPipeline.GlTopology, (uint)indexCount, type, (void*)offset);
        else
            _dev.Gl.DrawElementsInstancedBaseVertexBaseInstance(_boundPipeline.GlTopology, (uint)indexCount,
                type, (void*)offset, (uint)instanceCount, baseVertex, (uint)firstInstance);
    }

    public void UpdateBuffer(BufferHandle buffer, int offsetBytes, ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) return;
        var b = _dev.Buffers[buffer.Id];
        // glNamedBufferSubData is a synchronous, explicit upload — the
        // driver handles GPU synchronisation for us, so we don't get the
        // stale-frame issues we had with persistent COHERENT mapping.
        fixed (byte* src = data)
            _dev.Gl.NamedBufferSubData(b.Handle, offsetBytes, (nuint)data.Length, src);
    }

    public void PushDebugGroup(string name)
    {
        _dev.Gl.PushDebugGroup(DebugSource.DebugSourceApplication, 0, (uint)name.Length, name);
    }
    public void PopDebugGroup()
    {
        _dev.Gl.PopDebugGroup();
    }
}
