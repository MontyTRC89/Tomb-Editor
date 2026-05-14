using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace TombLib.Rendering.OpenGL
{
    // OpenGL 4.1 port of VulkanDrawingLines. Dynamic SoA vertex buffer,
    // per-batch LineData UBO, topology/blend/depth/wireframe mode selection.
    public sealed class OpenGLRenderingDrawingLines : RenderingDrawingLines
    {
        // ---- GLSL shaders (adapted from Vulkan #450 → #410) ----------------

        private const string VertexShaderGlsl = @"#version 410
layout(std140) uniform FrameData {
    mat4 TransformMatrix;
    float RoomGridLineWidth;
    int   RoomGridForce;
    int   RoomDisableVertexColors;
    int   ShowExtraBlendingModes;
    int   ShowLightingWhiteTextureOnly;
    int   LightMode;
    int   BrushShape;
    float BrushRotation;
    vec4  BrushCenter;
    vec4  BrushColor;
};
layout(std140) uniform LineData {
    mat4 World;
    vec4 Tint;
};
layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec4 inColor;
out vec4 fsColor;
void main() {
    vec4 worldPos = World * vec4(inPosition, 1.0);
    gl_Position = TransformMatrix * worldPos;
    fsColor = inColor * Tint;
}
";
        private const string FragmentShaderGlsl = @"#version 410
in vec4 fsColor;
layout(location = 0) out vec4 outColor;
void main() {
    outColor = fsColor;
}
";

        [StructLayout(LayoutKind.Explicit, Size = 80)]
        private struct LineDataLayout
        {
            [FieldOffset(0)]  public Matrix4x4 World;
            [FieldOffset(64)] public Vector4 Tint;
        }
        private const uint LineDataBindingPoint = 1;

        public readonly OpenGLRenderingDevice DeviceWrapper;
        private readonly GL _gl;

        private uint _program;
        private uint _vao;
        private uint _vbo;
        private uint _vboCapacity;
        private uint _lineDataUbo;

        private SolidLineVertex[] _vertices = Array.Empty<SolidLineVertex>();
        private int _vertexCount;

        public unsafe OpenGLRenderingDrawingLines(OpenGLRenderingDevice device, Description description)
        {
            DeviceWrapper = device;
            _gl = device.Gl;

            _program = device.CreateProgram(VertexShaderGlsl, FragmentShaderGlsl,
                uniformBlocks: new[]
                {
                    ("FrameData", OpenGLRenderingStateBuffer.BindingPoint),
                    ("LineData",  LineDataBindingPoint),
                });

            uint vao = 0; _gl.GenVertexArrays(1, &vao); _vao = vao;
            uint vbo = 0; _gl.GenBuffers(1, &vbo); _vbo = vbo;

            uint ubo = 0; _gl.GenBuffers(1, &ubo); _lineDataUbo = ubo;
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, _lineDataUbo);
            _gl.BufferData(BufferTargetARB.UniformBuffer, (nuint)sizeof(LineDataLayout), null, BufferUsageARB.DynamicDraw);
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
        }

        public override void SetVertices(ReadOnlySpan<SolidLineVertex> vertices)
        {
            if (vertices.Length > _vertices.Length)
                _vertices = new SolidLineVertex[Math.Max(vertices.Length, _vertices.Length * 2)];
            vertices.CopyTo(_vertices);
            _vertexCount = vertices.Length;
        }

        public override unsafe void Render(RenderArgs arg)
        {
            if (_vertexCount == 0) return;

            var stateBuffer = (OpenGLRenderingStateBuffer)arg.StateBuffer;

            // Upload per-batch LineData UBO
            LineDataLayout cbData;
            cbData.World = arg.World;
            cbData.Tint = arg.Tint;
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, _lineDataUbo);
            _gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, (nuint)sizeof(LineDataLayout), &cbData);
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);

            // Upload SoA vertex data (positions then colors)
            uint posBytes = (uint)(_vertexCount * sizeof(Vector3));
            uint colBytes = (uint)(_vertexCount * sizeof(Vector4));
            uint totalBytes = posBytes + colBytes;
            EnsureVbo(totalBytes);

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            byte* dst = (byte*)_gl.MapBufferRange(BufferTargetARB.ArrayBuffer, 0, totalBytes,
                MapBufferAccessMask.MapWriteBit | MapBufferAccessMask.MapInvalidateBufferBit);

            Vector3* posDst = (Vector3*)dst;
            Vector4* colDst = (Vector4*)(dst + posBytes);
            fixed (SolidLineVertex* src = _vertices)
            {
                for (int i = 0; i < _vertexCount; ++i)
                {
                    posDst[i] = src[i].Position;
                    colDst[i] = src[i].Color;
                }
            }
            _gl.UnmapBuffer(BufferTargetARB.ArrayBuffer);

            // Setup VAO
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            // location 0: vec3 position
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vector3), (void*)0);

            // location 1: vec4 color
            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Vector4), (void*)posBytes);

            // Bind UBOs
            stateBuffer.Bind();
            _gl.BindBufferBase(BufferTargetARB.UniformBuffer, LineDataBindingPoint, _lineDataUbo);

            _gl.UseProgram(_program);

            // Set GL state based on render args
            ApplyBlendMode(arg.Blend);
            ApplyDepthMode(arg.Depth);

            if (arg.Wireframe)
                _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

            _gl.Disable(EnableCap.CullFace);

            PrimitiveType prim = arg.Topology == Topology.TriangleList
                ? PrimitiveType.Triangles : PrimitiveType.Lines;
            _gl.DrawArrays(prim, 0, (uint)_vertexCount);

            // Restore defaults
            if (arg.Wireframe)
                _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            _gl.Enable(EnableCap.CullFace);
            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthMask(true);
            _gl.Disable(EnableCap.Blend);
            _gl.BindVertexArray(0);
        }

        private void ApplyBlendMode(BlendMode blend)
        {
            switch (blend)
            {
                case BlendMode.Opaque:
                    _gl.Disable(EnableCap.Blend);
                    break;
                case BlendMode.NonPremultipliedAlpha:
                    _gl.Enable(EnableCap.Blend);
                    _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                    break;
                case BlendMode.Additive:
                    _gl.Enable(EnableCap.Blend);
                    _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
                    break;
                default: // PremultipliedAlpha
                    _gl.Enable(EnableCap.Blend);
                    _gl.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
                    break;
            }
        }

        private void ApplyDepthMode(DepthMode depth)
        {
            switch (depth)
            {
                case DepthMode.Default:
                    _gl.Enable(EnableCap.DepthTest);
                    _gl.DepthMask(true);
                    break;
                case DepthMode.DepthRead:
                    _gl.Enable(EnableCap.DepthTest);
                    _gl.DepthMask(false);
                    break;
                case DepthMode.NoZ:
                    _gl.Disable(EnableCap.DepthTest);
                    _gl.DepthMask(false);
                    break;
            }
        }

        private unsafe void EnsureVbo(uint required)
        {
            if (_vboCapacity >= required) return;
            uint newCap = Math.Max(required, _vboCapacity * 2);
            if (newCap < 4096) newCap = 4096;
            _vboCapacity = newCap;

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, newCap, null, BufferUsageARB.StreamDraw);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }

        public override unsafe void Dispose()
        {
            if (_vbo != 0) { uint b = _vbo; _gl.DeleteBuffers(1, &b); _vbo = 0; }
            if (_lineDataUbo != 0) { uint b = _lineDataUbo; _gl.DeleteBuffers(1, &b); _lineDataUbo = 0; }
            if (_vao != 0) { uint v = _vao; _gl.DeleteVertexArrays(1, &v); _vao = 0; }
            if (_program != 0) { _gl.DeleteProgram(_program); _program = 0; }
        }
    }
}
