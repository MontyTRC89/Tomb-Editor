using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.OpenGL;
using TombLib.RenderingV2.Rhi;

namespace TombLib.RenderingV2.Backends.OpenGL;

public unsafe sealed partial class GLDevice
{
    public PipelineHandle CreatePipeline(PipelineDesc desc)
    {
        // ---- Compile + link the GLSL program ----
        byte[] vsBytes = desc.VertexShader.Glsl430Utf8
            ?? throw new InvalidOperationException("VertexShader.Glsl430Utf8 is null. Did the shader build run spirv-cross?");
        byte[] psBytes = desc.FragmentShader.Glsl430Utf8
            ?? throw new InvalidOperationException("FragmentShader.Glsl430Utf8 is null.");

        uint vs = CompileShader(ShaderType.VertexShader,   Encoding.UTF8.GetString(vsBytes));
        uint ps = CompileShader(ShaderType.FragmentShader, Encoding.UTF8.GetString(psBytes));
        uint prog = Gl.CreateProgram();
        Gl.AttachShader(prog, vs);
        Gl.AttachShader(prog, ps);
        Gl.LinkProgram(prog);
        Gl.GetProgram(prog, ProgramPropertyARB.LinkStatus, out int linked);
        if (linked == 0)
        {
            string log = Gl.GetProgramInfoLog(prog);
            Gl.DeleteShader(vs); Gl.DeleteShader(ps); Gl.DeleteProgram(prog);
            throw new InvalidOperationException("GL program link failed: " + log);
        }
        Gl.DeleteShader(vs);
        Gl.DeleteShader(ps);

        // ---- VAO with the vertex attribute layout ----
        // We use the modern separate-format API (glVertexArrayAttribFormat +
        // glVertexArrayAttribBinding) which lets us swap buffers at draw
        // time without touching the format.
        uint vao;
        Gl.CreateVertexArrays(1, &vao);
        for (int i = 0; i < desc.VertexAttributes.Length; i++)
        {
            var a = desc.VertexAttributes[i];
            var (type, comps, normalized, isInt) = GLMapping.AttribFormat(a.Format);
            Gl.EnableVertexArrayAttrib(vao, (uint)i);
            if (isInt)
                Gl.VertexArrayAttribIFormat(vao, (uint)i, comps, (GLEnum)(uint)type, (uint)a.Offset);
            else
                Gl.VertexArrayAttribFormat(vao, (uint)i, comps, (GLEnum)(uint)type, normalized, (uint)a.Offset);
            Gl.VertexArrayAttribBinding(vao, (uint)i, (uint)a.BufferSlot);
        }
        // Per-instance binding divisors.
        for (int i = 0; i < desc.VertexBufferLayouts.Length; i++)
            if (desc.VertexBufferLayouts[i].PerInstance)
                Gl.VertexArrayBindingDivisor(vao, (uint)i, 1);
        // Per-attribute override too (in case the same slot is used for
        // both per-vertex and per-instance attrs in some future layout).
        for (int i = 0; i < desc.VertexAttributes.Length; i++)
            if (desc.VertexAttributes[i].PerInstance)
                Gl.VertexArrayBindingDivisor(vao, (uint)desc.VertexAttributes[i].BufferSlot, 1);

        // ---- Sampler unit assignments ----
        // The GLSL declares samplers as plain `uniform sampler2D`. We need
        // to bind each to a texture unit at the program level. Texture[0]
        // in the RHI binds to GL_TEXTURE0 by convention.
        var samplerLocs = new System.Collections.Generic.Dictionary<int, int>();
        Gl.UseProgram(prog);
        // The shader's sampler name varies (spirv-cross emits names like
        // SPIRV_Cross_CombinedAtlasAtlasSamp). We look up by querying all
        // active uniforms of sampler type and assigning units in declaration
        // order, mapping the first sampler to texture slot 0.
        Gl.GetProgram(prog, ProgramPropertyARB.ActiveUniforms, out int activeUniforms);
        int nextUnit = 0;
        for (uint u = 0; u < (uint)activeUniforms; u++)
        {
            Gl.GetActiveUniform(prog, u, 256, out _, out _, out UniformType utype, out string name);
            if (utype is UniformType.Sampler2D or UniformType.SamplerCube or UniformType.Sampler2DArray
                       or UniformType.IntSampler2D or UniformType.UnsignedIntSampler2D)
            {
                int loc = Gl.GetUniformLocation(prog, name);
                if (loc < 0) continue;
                Gl.Uniform1(loc, nextUnit);
                samplerLocs[nextUnit] = loc;
                nextUnit++;
            }
        }
        Gl.UseProgram(0);

        // ---- Fixed-function state ----
        var p = new GLPipelineRes
        {
            Program       = prog,
            Vao           = vao,
            Topology      = desc.Topology,
            GlTopology    = GLMapping.ToGl(desc.Topology),
            DepthTest     = desc.DepthStencil.DepthTestEnable,
            DepthWrite    = desc.DepthStencil.DepthWriteEnable,
            DepthFunc     = GLMapping.ToGl(desc.DepthStencil.DepthCompare),
            CullEnable    = desc.Rasterizer.CullMode != Rhi.CullMode.None,
            CullMode      = desc.Rasterizer.CullMode switch
                            {
                                Rhi.CullMode.Back  => CullModeMaterial.Back,
                                Rhi.CullMode.Front => CullModeMaterial.Front,
                                _                  => CullModeMaterial.None,
                            },
            FrontFace     = desc.Rasterizer.FrontCounterClockwise ? FrontFaceDirection.Ccw : FrontFaceDirection.CW,
            PolygonMode   = desc.Rasterizer.FillMode == Rhi.FillMode.Wireframe
                            ? PolygonMode.Line : PolygonMode.Fill,
            ScissorEnable = desc.Rasterizer.ScissorEnable,
            SamplerLocations = samplerLocs,
            VertexStrides = System.Linq.Enumerable.Range(0, desc.VertexBufferLayouts.Length)
                              .Select(i => desc.VertexBufferLayouts[i].StrideBytes).ToArray(),
        };
        var bs = desc.BlendStates.Length > 0 ? desc.BlendStates[0] : Rhi.BlendState.Opaque;
        p.BlendEnable = bs.Enable;
        p.BlendSrcRgb = GLMapping.ToGl(bs.SrcColor);
        p.BlendDstRgb = GLMapping.ToGl(bs.DstColor);
        p.BlendOpRgb  = GLMapping.ToGl(bs.ColorOp);
        p.BlendSrcA   = GLMapping.ToGl(bs.SrcAlpha);
        p.BlendDstA   = GLMapping.ToGl(bs.DstAlpha);
        p.BlendOpA    = GLMapping.ToGl(bs.AlphaOp);

        uint id = AllocHandle();
        Pipelines[id] = p;
        return new PipelineHandle(id);
    }

    public void Destroy(PipelineHandle h)
    {
        if (Pipelines.Remove(h.Id, out var p))
        {
            if (p.Vao != 0)     Gl.DeleteVertexArray(p.Vao);
            if (p.Program != 0) Gl.DeleteProgram(p.Program);
        }
    }

    private uint CompileShader(ShaderType type, string source)
    {
        uint s = Gl.CreateShader(type);
        Gl.ShaderSource(s, source);
        Gl.CompileShader(s);
        Gl.GetShader(s, ShaderParameterName.CompileStatus, out int ok);
        if (ok == 0)
        {
            string log = Gl.GetShaderInfoLog(s);
            Gl.DeleteShader(s);
            throw new InvalidOperationException($"GL {type} compile failed:\n{log}\n--- source ---\n{source}");
        }
        return s;
    }
}
