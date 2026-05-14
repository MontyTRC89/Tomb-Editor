using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace TombLib.Rendering.OpenGL
{
    // OpenGL 4.1 port of VulkanDrawingMesh. Immutable VBO/IBO, 32-bone GPU
    // skinning, per-submesh (DoubleSided / Additive) GL state switching.
    public sealed class OpenGLRenderingDrawingMesh : RenderingDrawingMesh
    {
        private const int MaxBones = 32;
        private const uint MeshDataSize = 64 + 16 + (uint)MaxBones * 64 + 16;
        private const uint MeshDataBindingPoint = 1;

        // ---- GLSL shaders (adapted from Vulkan #450 → #410) ----------------

        private const string VertexShaderGlsl = @"#version 410
#define MAX_BONES 32
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
    mat4 Bones[MAX_BONES];
    int  Skinned;
    int  StaticLighting;
    int  ColoredVertices;
    int  AlphaTest;
};
layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec3 inUVW;
layout(location = 2) in vec3 inNormal;
layout(location = 3) in vec3 inColor;
layout(location = 4) in vec4 inBoneIdx;
layout(location = 5) in vec4 inBoneW;
out vec3 fsUVW;
out vec4 fsColor;
out vec3 fsWorldPosition;
void main() {
    vec4 localPos = vec4(inPosition, 1.0);
    if (Skinned != 0) {
        float totalW = dot(inBoneW, vec4(1.0));
        ivec4 bi = ivec4(
            clamp(int(inBoneIdx.x), 0, MAX_BONES - 1),
            clamp(int(inBoneIdx.y), 0, MAX_BONES - 1),
            clamp(int(inBoneIdx.z), 0, MAX_BONES - 1),
            clamp(int(inBoneIdx.w), 0, MAX_BONES - 1));
        mat4 blended;
        if (totalW < 1e-30) blended = Bones[bi.x];
        else {
            blended  = Bones[bi.x] * (inBoneW.x / totalW);
            blended += Bones[bi.y] * (inBoneW.y / totalW);
            blended += Bones[bi.z] * (inBoneW.z / totalW);
            blended += Bones[bi.w] * (inBoneW.w / totalW);
            blended[0].w = 0; blended[1].w = 0; blended[2].w = 0; blended[3].w = 1;
        }
        localPos = blended * localPos;
    }
    vec4 worldPos = World * localPos;
    gl_Position = TransformMatrix * worldPos;
    fsUVW = inUVW;
    fsWorldPosition = worldPos.xyz;
    vec3 vc = inColor;
    if (ColoredVertices == 0) {
        float luma = vc.r * 0.2126 + vc.g * 0.7152 + vc.b * 0.0722;
        vc = vec3(luma);
    }
    fsColor = (StaticLighting != 0) ? vec4(Tint.rgb * vc, Tint.a) : Tint;
}
";
        private const string FragmentShaderGlsl = @"#version 410
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
    mat4 Bones[32];
    int  Skinned;
    int  StaticLighting;
    int  ColoredVertices;
    int  AlphaTest;
};
uniform sampler2DArray AtlasSampler;
in vec3 fsUVW;
in vec4 fsColor;
in vec3 fsWorldPosition;
layout(location = 0) out vec4 outColor;
void ApplyBrushOverlay(inout vec3 rgb, inout float a, bool updateAlpha, vec4 svPos, vec3 worldPos, float lineWidthFactor) {
    if (BrushShape == 0) return;
    vec2 delta = worldPos.xz - BrushCenter.xz;
    float dist = (BrushShape == 1) ? length(delta) : max(abs(delta.x), abs(delta.y));
    float edge = abs(dist - BrushCenter.w);
    float lineWidth = (lineWidthFactor * 2048.0) * svPos.w;
    float fw = max(fwidth(dist), 0.001);
    float fillAlpha = step(dist, BrushCenter.w) * BrushColor.w;
    vec3 excludeColor = rgb + BrushColor.xyz - 2.0 * rgb * BrushColor.xyz;
    vec3 diffColor = abs(rgb - BrushColor.xyz);
    rgb = mix(rgb, clamp(excludeColor + diffColor, 0.0, 1.0), fillAlpha);
    if (updateAlpha) a = max(a, fillAlpha);
    float contourAlpha = clamp(1.0 - (edge / fw) / max(lineWidth, 0.001), 0.0, 1.0);
    rgb = mix(rgb, vec3(1.0), contourAlpha);
    if (updateAlpha) a = max(a, contourAlpha);
}
void main() {
    vec4 texel = texture(AtlasSampler, fsUVW);
    vec3 colorAdd = max(fsColor.rgb - 1.0, 0.0) * 0.37;
    vec3 colorMul = min(fsColor.rgb, 1.0);
    texel.rgb = texel.rgb * colorMul + colorAdd;
    texel.a *= fsColor.a;
    if (AlphaTest != 0 && texel.a <= 0.01) discard;
    ApplyBrushOverlay(texel.rgb, texel.a, false, gl_FragCoord, fsWorldPosition, RoomGridLineWidth);
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

        public unsafe OpenGLRenderingDrawingMesh(OpenGLRenderingDevice device, Description description)
        {
            DeviceWrapper = device;
            _gl = device.Gl;

            _submeshes = new Submesh[description.Submeshes?.Count ?? 0];
            if (description.Submeshes != null)
                for (int i = 0; i < _submeshes.Length; i++) _submeshes[i] = description.Submeshes[i];

            int vCount = description.Vertices.Count;
            int iCount = description.Indices.Count;

            // Upload vertex buffer (interleaved MeshVertex, 80 bytes/vertex)
            uint vbo = 0; _gl.GenBuffers(1, &vbo); _vbo = vbo;
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            MeshVertex[] vArr = description.Vertices is MeshVertex[] va ? va : ToArray(description.Vertices);
            fixed (MeshVertex* vp = vArr)
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vCount * sizeof(MeshVertex)), vp, BufferUsageARB.StaticDraw);

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
                samplers: new[] { ("AtlasSampler", 0) });

            // Create MeshData UBO
            uint ubo = 0; _gl.GenBuffers(1, &ubo); _meshDataUbo = ubo;
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, _meshDataUbo);
            _gl.BufferData(BufferTargetARB.UniformBuffer, MeshDataSize, null, BufferUsageARB.DynamicDraw);
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);

            // Setup VAO with MeshVertex layout (80 bytes stride)
            uint vao = 0; _gl.GenVertexArrays(1, &vao); _vao = vao;
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ibo);

            // MeshVertex: Position@0(12), UVW@12(12), Normal@24(12), Color@36(12), BoneIdx@48(16), BoneW@64(16) = 80
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 80, (void*)0);   // Position
            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 80, (void*)12);  // UVW
            _gl.EnableVertexAttribArray(2);
            _gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 80, (void*)24);  // Normal
            _gl.EnableVertexAttribArray(3);
            _gl.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 80, (void*)36);  // Color
            _gl.EnableVertexAttribArray(4);
            _gl.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, 80, (void*)48);  // BoneIdx
            _gl.EnableVertexAttribArray(5);
            _gl.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, 80, (void*)64);  // BoneW

            _gl.BindVertexArray(0);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        }

        private static T[] ToArray<T>(IList<T> list) { var a = new T[list.Count]; for (int i = 0; i < list.Count; i++) a[i] = list[i]; return a; }

        public override unsafe void Render(RenderArgs arg)
        {
            if (_submeshes.Length == 0) return;

            var stateBuffer = (OpenGLRenderingStateBuffer)arg.StateBuffer;
            uint atlasTexture = ResolveAtlas(arg.Atlas);
            if (atlasTexture == 0) return;

            // Build MeshData payload
            fixed (byte* dst = _cbufferStaging)
            {
                ((Matrix4x4*)(dst + 0))[0] = arg.World;
                ((Vector4*)(dst + 64))[0] = arg.Tint;
                Matrix4x4* bonesPtr = (Matrix4x4*)(dst + 80);
                int boneCount = arg.BoneMatrices?.Length ?? 0;
                int useBones = Math.Min(boneCount, MaxBones);
                for (int i = 0; i < useBones; ++i) bonesPtr[i] = arg.BoneMatrices[i];
                for (int i = useBones; i < MaxBones; ++i) bonesPtr[i] = Matrix4x4.Identity;
                int* flagsPtr = (int*)(dst + 80 + MaxBones * 64);
                flagsPtr[0] = arg.Skinned ? 1 : 0;
                flagsPtr[1] = arg.StaticLighting ? 1 : 0;
                flagsPtr[2] = arg.ColoredVertices ? 1 : 0;
                flagsPtr[3] = arg.AlphaTest ? 1 : 0;
            }

            // Upload MeshData UBO
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, _meshDataUbo);
            fixed (byte* src = _cbufferStaging)
                _gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, MeshDataSize, src);
            _gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);

            // Bind program and UBOs
            _gl.UseProgram(_program);
            stateBuffer.Bind();
            _gl.BindBufferBase(BufferTargetARB.UniformBuffer, MeshDataBindingPoint, _meshDataUbo);

            // Bind atlas texture
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2DArray, atlasTexture);
            _gl.BindSampler(0, arg.BilinearFilter ? DeviceWrapper.SamplerAniso : DeviceWrapper.SamplerPoint);

            // Default draw state
            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthFunc(DepthFunction.Lequal);
            _gl.DepthMask(true);

            _gl.BindVertexArray(_vao);

            foreach (var sub in _submeshes)
            {
                if (sub.IndexCount == 0) continue;

                // Per-submesh cull mode
                if (sub.DoubleSided)
                    _gl.Disable(EnableCap.CullFace);
                else
                {
                    _gl.Enable(EnableCap.CullFace);
                    _gl.CullFace(TriangleFace.Back);
                }

                // Per-submesh blend mode
                if (sub.AdditiveBlending)
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

        private static uint ResolveAtlas(object atlas)
        {
            if (atlas is OpenGLRenderingTextureAllocator a) return a.AtlasTexture;
            if (atlas is uint t) return t;
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
