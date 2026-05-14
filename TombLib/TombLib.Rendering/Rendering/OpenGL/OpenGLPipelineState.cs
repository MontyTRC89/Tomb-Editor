using System;
using Silk.NET.OpenGL;

namespace TombLib.Rendering.OpenGL
{
    // Bundles a GL program (linked VS+FS) with a VAO. The equivalent of
    // Dx11PipelineState (VS+PS+InputLayout) or a VkPipeline.
    //
    // Unlike Vulkan, OpenGL pipeline state (blend, depth, cull) is set globally
    // via glEnable/glDisable before the draw call — it is NOT baked into this
    // object. This class only owns the shader program and VAO.
    public sealed class OpenGLPipelineState : IDisposable
    {
        public uint Program { get; private set; }
        public uint Vao { get; private set; }

        private readonly GL _gl;

        public OpenGLPipelineState(GL gl, uint program, uint vao)
        {
            _gl = gl;
            Program = program;
            Vao = vao;
        }

        public void Bind()
        {
            _gl.UseProgram(Program);
            _gl.BindVertexArray(Vao);
        }

        public unsafe void Dispose()
        {
            if (Vao != 0) { uint v = Vao; _gl.DeleteVertexArrays(1, &v); Vao = 0; }
            if (Program != 0) { _gl.DeleteProgram(Program); Program = 0; }
        }
    }
}
