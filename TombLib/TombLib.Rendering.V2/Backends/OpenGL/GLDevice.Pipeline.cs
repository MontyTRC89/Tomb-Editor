using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silk.NET.OpenGL;
using TombLib.RenderingV2.Rhi;

namespace TombLib.RenderingV2.Backends.OpenGL;

// Pipeline creation for the OpenGL backend. One RHI pipeline maps to a linked
// GL program plus a VAO that captures the vertex-attribute layout; the
// fixed-function state is stored on GLPipelineRes and applied by SetPipeline.
public unsafe sealed partial class GLDevice
{
    public PipelineHandle CreatePipeline(PipelineDesc desc)
    {
        // ---- Compile + link the GLSL program ----
        byte[] vertexShaderGlsl = desc.VertexShader.Glsl430Utf8
            ?? throw new InvalidOperationException("VertexShader.Glsl430Utf8 is null. Did the shader build run spirv-cross?");
        byte[] pixelShaderGlsl = desc.FragmentShader.Glsl430Utf8
            ?? throw new InvalidOperationException("FragmentShader.Glsl430Utf8 is null.");

        uint vertexShader = CompileShader(ShaderType.VertexShader,   Encoding.UTF8.GetString(vertexShaderGlsl));
        uint pixelShader  = CompileShader(ShaderType.FragmentShader, Encoding.UTF8.GetString(pixelShaderGlsl));

        uint program = Gl.CreateProgram();
        Gl.AttachShader(program, vertexShader);
        Gl.AttachShader(program, pixelShader);
        Gl.LinkProgram(program);
        Gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus == 0)
        {
            string infoLog = Gl.GetProgramInfoLog(program);
            Gl.DeleteShader(vertexShader);
            Gl.DeleteShader(pixelShader);
            Gl.DeleteProgram(program);
            throw new InvalidOperationException("GL program link failed: " + infoLog);
        }
        Gl.DeleteShader(vertexShader);
        Gl.DeleteShader(pixelShader);

        // ---- VAO with the vertex-attribute layout ----
        // The modern separate-format API (glVertexArrayAttribFormat +
        // glVertexArrayAttribBinding) lets buffers be swapped at draw time
        // without re-specifying the format.
        uint vao;
        Gl.CreateVertexArrays(1, &vao);
        for (int i = 0; i < desc.VertexAttributes.Length; i++)
        {
            var attribute = desc.VertexAttributes[i];
            var (glType, componentCount, normalized, isInteger) = GLMapping.AttribFormat(attribute.Format);
            Gl.EnableVertexArrayAttrib(vao, (uint)i);
            if (isInteger)
                Gl.VertexArrayAttribIFormat(vao, (uint)i, componentCount, (GLEnum)(uint)glType, (uint)attribute.Offset);
            else
                Gl.VertexArrayAttribFormat(vao, (uint)i, componentCount, (GLEnum)(uint)glType, normalized, (uint)attribute.Offset);
            Gl.VertexArrayAttribBinding(vao, (uint)i, (uint)attribute.BufferSlot);
        }

        // Per-binding instance divisors.
        for (int i = 0; i < desc.VertexBufferLayouts.Length; i++)
            if (desc.VertexBufferLayouts[i].PerInstance)
                Gl.VertexArrayBindingDivisor(vao, (uint)i, 1);
        // Per-attribute override too, in case a future layout uses the same
        // slot for both per-vertex and per-instance attributes.
        for (int i = 0; i < desc.VertexAttributes.Length; i++)
            if (desc.VertexAttributes[i].PerInstance)
                Gl.VertexArrayBindingDivisor(vao, (uint)desc.VertexAttributes[i].BufferSlot, 1);

        // ---- Sampler unit assignments ----
        // The GLSL declares samplers as plain `uniform sampler2D`; each must be
        // bound to a texture unit at the program level. RHI Texture[0] binds to
        // GL_TEXTURE0 by convention. The shader's sampler name varies
        // (spirv-cross emits names like SPIRV_Cross_CombinedAtlasAtlasSamp), so
        // we query every active sampler-typed uniform and assign units in
        // declaration order.
        var samplerLocations = new Dictionary<int, int>();
        Gl.UseProgram(program);
        Gl.GetProgram(program, ProgramPropertyARB.ActiveUniforms, out int activeUniforms);
        int nextTextureUnit = 0;
        for (uint uniformIndex = 0; uniformIndex < (uint)activeUniforms; uniformIndex++)
        {
            Gl.GetActiveUniform(program, uniformIndex, 256, out _, out _,
                                out UniformType uniformType, out string uniformName);
            if (uniformType is UniformType.Sampler2D or UniformType.SamplerCube or UniformType.Sampler2DArray
                            or UniformType.IntSampler2D or UniformType.UnsignedIntSampler2D)
            {
                int location = Gl.GetUniformLocation(program, uniformName);
                if (location < 0) continue;
                Gl.Uniform1(location, nextTextureUnit);
                samplerLocations[nextTextureUnit] = location;
                nextTextureUnit++;
            }
        }
        Gl.UseProgram(0);

        // ---- Fixed-function state ----
        var pipeline = new GLPipelineRes
        {
            Program       = program,
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
            SamplerLocations = samplerLocations,
            VertexStrides = desc.VertexBufferLayouts.Select(layout => layout.StrideBytes).ToArray(),
        };

        var blend = desc.BlendStates.Length > 0 ? desc.BlendStates[0] : Rhi.BlendState.Opaque;
        pipeline.BlendEnable = blend.Enable;
        pipeline.BlendSrcRgb = GLMapping.ToGl(blend.SrcColor);
        pipeline.BlendDstRgb = GLMapping.ToGl(blend.DstColor);
        pipeline.BlendOpRgb  = GLMapping.ToGl(blend.ColorOp);
        pipeline.BlendSrcA   = GLMapping.ToGl(blend.SrcAlpha);
        pipeline.BlendDstA   = GLMapping.ToGl(blend.DstAlpha);
        pipeline.BlendOpA    = GLMapping.ToGl(blend.AlphaOp);

        uint id = AllocHandle();
        Pipelines[id] = pipeline;
        return new PipelineHandle(id);
    }

    public void Destroy(PipelineHandle handle)
    {
        if (Pipelines.Remove(handle.Id, out var pipeline))
        {
            if (pipeline.Vao     != 0) Gl.DeleteVertexArray(pipeline.Vao);
            if (pipeline.Program != 0) Gl.DeleteProgram(pipeline.Program);
        }
    }

    private uint CompileShader(ShaderType type, string source)
    {
        uint shader = Gl.CreateShader(type);
        Gl.ShaderSource(shader, source);
        Gl.CompileShader(shader);
        Gl.GetShader(shader, ShaderParameterName.CompileStatus, out int compileStatus);
        if (compileStatus == 0)
        {
            string infoLog = Gl.GetShaderInfoLog(shader);
            Gl.DeleteShader(shader);
            throw new InvalidOperationException($"GL {type} compile failed:\n{infoLog}\n--- source ---\n{source}");
        }
        return shader;
    }
}
