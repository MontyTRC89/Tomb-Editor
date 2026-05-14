using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace TombLib.Rendering.OpenGL
{
    // OpenGL 4.1 port of VulkanDrawingImportedGeometry. Per-submesh texture
    // binding, indexed draws, pixel-space UV normalization.
    public sealed class OpenGLRenderingDrawingImportedGeometry : RenderingDrawingImportedGeometry
    {
        private const uint MeshDataSize = 96;
        private const uint MeshDataBindingPoint = 1;

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
layout(std140) uniform MeshData {
    mat4 World;
    vec4 Tint;
    vec4 RecipTexSize;   // .xy = 1/textureSize, .z = UseVertexColors, .w = AlphaTest
};
layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec3 inColor;
layout(location = 3) in vec3 inNormal;
out vec2 fsUV;
out vec4 fsColor;
out vec3 fsWorldPos;
void main() {
    vec4 worldPos = World * vec4(inPosition, 1.0);
    gl_Position = TransformMatrix * worldPos;
    fsUV = inUV * RecipTexSize.xy;
    fsColor = (RecipTexSize.z > 0.5)
        ? vec4(Tint.rgb * inColor, Tint.a)
        : Tint;
    fsWorldPos = worldPos.xyz;
}
";
        private const string FragmentShaderGlsl = @"#version 410
layout(std140) uniform MeshData {
    mat4 World;
    vec4 Tint;
    vec4 RecipTexSize;
};
uniform sampler2D TextureSampler;
in vec2 fsUV;
in vec4 fsColor;
in vec3 fsWorldPos;
layout(location = 0) out vec4 outColor;
void main() {
    vec4 texel = texture(TextureSampler, fsUV);
    vec3 colorAdd = max(fsColor.rgb - 1.0, 0.0) * 0.37;
    vec3 colorMul = min(fsColor.rgb, 1.0);
    texel.rgb = texel.rgb * colorMul + colorAdd;
    texel.a *= fsColor.a;
    if (RecipTexSize.w > 0.5 && texel.a <= 0.01) discard;
    outColor = texel;
}
";

        public readonly OpenGLRenderingDevice DeviceWrapper;
        private readonly GL _gl;

        private uint _program;
        private uint _vao;
        private uint _vbo, _ibo;
        private uint _meshDataUbo;
        private readonly Submesh[] _submeshes;
        private readonly byte[] _cbufferStaging = new byte[MeshDataSize];

        public unsafe OpenGLRenderingDrawingImportedGeometry(OpenGLRenderingDevice device, Description description)
        {
            DeviceWrapper = device;
            _gl = device.Gl;

            _submeshes = new Submesh[description.Submeshes?.Count ?? 0];
            if (description.Submeshes != null)
                for (int i = 0; i < _submeshes.Length; i++) _submeshes[i] = description.Submeshes[i];

            int vCount = description.Vertices.Count;
            int iCount = description.Indices.Count;

            // Upload vertex buffer (Vertex struct, 44 bytes/vertex)
            uint vbo = 0; _gl.GenBuffers(1, &vbo); _vbo = vbo;
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            Vertex[] vArr = description.Vertices is Vertex[] va ? va : ToArray(description.Vertices);
            fixed (Vertex* vp = vArr)
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vCount * sizeof(Vertex)), vp, BufferUsageARB.StaticDraw);

            // Upload index buffer
            uint ibo = 0; _gl.GenBuffers(1, &ibo); _ibo = ibo;
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ibo);
            int[] iArr = description.Indices is int[] ia ? ia : ToArray(description.Indices);
            fixed (int* ip = iArr)
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(iCount * sizeof(int)), ip, BufferUsageARB.StaticDraw);

            // Create shader program
            _program = device.CreateProgram(VertexShaderGlsl, FragmentShaderGlsl,
                uniformBlocks: new[]
                {
                    ("FrameData", OpenGLRenderingStateBuffer.BindingPoint),
                    ("MeshData",  MeshDataBindingPoint),
                },
                samplers: new[] { ("TextureSampler", 0) });

            // Create MeshData UBO
            uint ubo = 0; _gl.GenBuffers(1, &ubo); _meshDataUbo = ubo;
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, _meshDataUbo);
            _gl.BufferData(BufferTargetARB.UniformBuffer, MeshDataSize, null, BufferUsageARB.DynamicDraw);
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);

            // Setup VAO: Vertex layout is Position@0(12), UV@12(8), Color@20(12), Normal@32(12) = 44
            uint vao = 0; _gl.GenVertexArrays(1, &vao); _vao = vao;
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ibo);

            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 44, (void*)0);   // Position
            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 44, (void*)12);  // UV
            _gl.EnableVertexAttribArray(2);
            _gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 44, (void*)20);  // Color
            _gl.EnableVertexAttribArray(3);
            _gl.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 44, (void*)32);  // Normal

            _gl.BindVertexArray(0);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        }

        private static T[] ToArray<T>(IList<T> list) { var a = new T[list.Count]; for (int i = 0; i < list.Count; i++) a[i] = list[i]; return a; }

        public override unsafe void Render(RenderArgs arg)
        {
            if (_submeshes.Length == 0) return;

            var stateBuffer = (OpenGLRenderingStateBuffer)arg.StateBuffer;
            _gl.UseProgram(_program);
            stateBuffer.Bind();

            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthFunc(DepthFunction.Lequal);
            _gl.DepthMask(true);

            _gl.BindVertexArray(_vao);

            uint sampler = arg.BilinearFilter ? DeviceWrapper.SamplerAniso : DeviceWrapper.SamplerPoint;

            for (int i = 0; i < _submeshes.Length; i++)
            {
                var sub = _submeshes[i];
                if (sub.IndexCount == 0) continue;

                // Per-submesh MeshData
                fixed (byte* dst = _cbufferStaging)
                {
                    ((Matrix4x4*)(dst + 0))[0] = arg.World;
                    ((Vector4*)(dst + 64))[0] = arg.Tint;
                    Vector2 recip = (sub.TextureSize.X > 0 && sub.TextureSize.Y > 0)
                        ? new Vector2(1f / sub.TextureSize.X, 1f / sub.TextureSize.Y)
                        : Vector2.One;
                    ((Vector4*)(dst + 80))[0] = new Vector4(recip.X, recip.Y,
                        arg.UseVertexColors ? 1f : 0f, arg.AlphaTest ? 1f : 0f);
                }

                _gl.BindBuffer(BufferTargetARB.UniformBuffer, _meshDataUbo);
                fixed (byte* src = _cbufferStaging)
                    _gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, MeshDataSize, src);
                _gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
                _gl.BindBufferBase(BufferTargetARB.UniformBuffer, MeshDataBindingPoint, _meshDataUbo);

                // Per-submesh texture
                uint texName = ResolveTexture(sub.Texture);
                if (texName == 0) continue;
                _gl.ActiveTexture(TextureUnit.Texture0);
                _gl.BindTexture(TextureTarget.Texture2D, texName);
                _gl.BindSampler(0, sampler);

                // Per-submesh cull mode
                if (sub.DoubleSided)
                    _gl.Disable(EnableCap.CullFace);
                else
                {
                    _gl.Enable(EnableCap.CullFace);
                    _gl.CullFace(TriangleFace.Back);
                }

                // Per-submesh blend mode
                bool additive = sub.AdditiveBlending || arg.ForceAdditive;
                if (additive)
                {
                    _gl.Enable(EnableCap.Blend);
                    _gl.BlendFunc(BlendingFactor.One, BlendingFactor.One);
                }
                else
                {
                    _gl.Enable(EnableCap.Blend);
                    _gl.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
                }

                _gl.DrawElements(PrimitiveType.Triangles, (uint)sub.IndexCount, DrawElementsType.UnsignedInt, (void*)(sub.IndexStart * sizeof(int)));
            }

            // Restore
            _gl.BindVertexArray(0);
            _gl.BindSampler(0, 0);
            _gl.Enable(EnableCap.CullFace);
        }

        private static uint ResolveTexture(object tex)
        {
            if (tex is OpenGLTexture2D t) return t.Texture;
            if (tex is uint u) return u;
            return 0;
        }

        public override unsafe void Dispose()
        {
            if (_vbo != 0) { uint b = _vbo; _gl.DeleteBuffers(1, &b); _vbo = 0; }
            if (_ibo != 0) { uint b = _ibo; _gl.DeleteBuffers(1, &b); _ibo = 0; }
            if (_meshDataUbo != 0) { uint b = _meshDataUbo; _gl.DeleteBuffers(1, &b); _meshDataUbo = 0; }
            if (_vao != 0) { uint v = _vao; _gl.DeleteVertexArrays(1, &v); _vao = 0; }
            if (_program != 0) { _gl.DeleteProgram(_program); _program = 0; }
        }
    }
}
