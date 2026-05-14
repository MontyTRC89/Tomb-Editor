using System;
using System.Numerics;
using Silk.NET.OpenGL;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorStructs;
using TombLib.Utils;

namespace TombLib.Rendering.OpenGL
{
    // OpenGL 4.1 port of VulkanDrawingRoom. Pre-baked 5-stream SoA vertex
    // buffer, sector overlays, grid outlines. Single draw call per room.
    public sealed class OpenGLRenderingDrawingRoom : RenderingDrawingRoom
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
layout(location = 0) in vec3  inPosition;
layout(location = 1) in vec4  inColor;
layout(location = 2) in vec4  inOverlay;
layout(location = 3) in uvec2 inUvwBlend;
layout(location = 4) in uint  inEditorUv;
out vec4  fsColor;
out vec4  fsOverlay;
out vec3  fsUvw;
flat out int fsBlendMode;
flat out int fsEditorSectorTexture;
out vec3  fsWorldPos;
out vec2  fsEditorUv;
void main() {
    gl_Position = TransformMatrix * vec4(inPosition, 1.0);
    vec4 col = (RoomDisableVertexColors != 0)
        ? vec4(1.0) : vec4(inColor.rgb * 2.0, inColor.a);
    if (LightMode == 1) {
        col = vec4(floor(col.rgb * 32.0) / 32.0, col.a);
    } else if (LightMode == 2) {
        float luma = dot(col.rgb, vec3(0.2126, 0.7152, 0.0722));
        col = vec4(vec3(luma), col.a);
    }
    fsColor = col;
    fsOverlay = inOverlay;
    float u = float(inUvwBlend.x & 0xffffffu) / 16777216.0;
    float v = float((inUvwBlend.x >> 24) | ((inUvwBlend.y & 0xffffu) << 8)) / 16777216.0;
    float w = float((inUvwBlend.y >> 16) & 0xfffu);
    fsUvw = vec3(u, v, w);
    fsBlendMode = int(inUvwBlend.y >> 28);
    fsEditorSectorTexture = int(inEditorUv);
    fsWorldPos = inPosition;
    fsEditorUv = vec2(
        float((int(inEditorUv) << 30) >> 30),
        float((int(inEditorUv >> 2) << 30) >> 30));
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
uniform sampler2DArray AtlasSampler;
uniform sampler2DArray SectorTextureSampler;
in vec4 fsColor;
in vec4 fsOverlay;
in vec3 fsUvw;
flat in int fsBlendMode;
flat in int fsEditorSectorTexture;
in vec3 fsWorldPos;
in vec2 fsEditorUv;
layout(location = 0) out vec4 outColor;
float ddAny(float v) { return length(vec2(dFdx(v), dFdy(v))); }
void main() {
    vec4 result;
    int drawOutline = 0;
    if (fsUvw.x != 0.0 && RoomGridForce == 0) {
        vec4 texel = texture(AtlasSampler, fsUvw);
        if (ShowLightingWhiteTextureOnly != 0 && RoomDisableVertexColors == 0)
            texel = vec4(1.0);
        vec3 colorAdd = max(fsColor.rgb - 1.0, 0.0) * 0.37;
        vec3 colorMul = min(fsColor.rgb, 1.0);
        texel.rgb = texel.rgb * colorMul + colorAdd;
        texel.a *= fsColor.a;
        texel.rgb *= fsColor.a;
        texel.rgb *= texel.a;
        if (fsBlendMode >= 2) texel.a = 0.0;
        result = texel;
        if (texel.a < 0.005 && fsBlendMode < 2) discard;
    } else {
        drawOutline = 1;
        if (fsUvw.y == 0.0 && RoomGridForce == 0) {
            result = vec4(0.0);
        } else {
            uint sectorBits = uint(fsEditorSectorTexture);
            uint r = (sectorBits >> 8)  & 0xffu;
            uint g = (sectorBits >> 16) & 0xffu;
            uint b = (sectorBits >> 24) & 0xffu;
            vec3 sectorColor = vec3(float(r), float(g), float(b)) / 255.0;
            bool hasSectorTex = (sectorBits & 0x40u) != 0u;
            if (hasSectorTex) {
                result = fsOverlay;
                if ((sectorBits & 0x20u) != 0u) result.rgb *= 0.70;
                float layer = float((sectorBits >> 8) & 0xffu);
                vec4 texColor = texture(SectorTextureSampler, vec3(fsEditorUv, layer));
                float bri = sqrt(result.r*result.r*0.299 + result.g*result.g*0.587 + result.b*result.b*0.114);
                if (bri > 0.8) result.rgb = clamp(result.rgb - texColor.rgb, vec3(0.0), vec3(1.0));
                else           result.rgb = clamp(result.rgb + texColor.rgb, vec3(0.0), vec3(1.0));
            } else {
                result = vec4(sectorColor, 1.0);
                if ((sectorBits & 0x20u) != 0u) result.rgb *= 0.70;
            }
        }
    }

    if ((uint(fsEditorSectorTexture) & 0x80u) != 0u && RoomGridForce == 0) {
        result.rgb = clamp(result.rgb + fsOverlay.rgb * 0.1 * result.a, vec3(0.0), vec3(1.0));
        drawOutline = 2;
    }
    if ((uint(fsEditorSectorTexture) & 0x10u) != 0u)
        result.rgb = clamp(result.rgb + 0.2, vec3(0.0), vec3(1.0));

    if (drawOutline > 0) {
        vec2 absUV = abs(fsEditorUv);
        float lineWidth = (RoomGridLineWidth * 1024.0) * gl_FragCoord.w - 0.5;
        float rx = ddAny(fsEditorUv.x);
        float ry = ddAny(fsEditorUv.y);
        float rd = ddAny(fsEditorUv.x + fsEditorUv.y);
        float dx = min(absUV.x, 1.0 - absUV.x);
        float dy = min(absUV.y, 1.0 - absUV.y);
        float dd = min(abs(fsEditorUv.x + fsEditorUv.y + 1.0),
                       abs(fsEditorUv.x + fsEditorUv.y));
        float lx = dx / max(rx, 1e-8) - lineWidth;
        float ly = dy / max(ry, 1e-8) - lineWidth;
        float ld = dd / max(rd, 1e-8) - lineWidth;
        float strength = clamp(min(min(lx, ly), ld), 0.0, 1.0);
        result.rgb *= strength;
        if (drawOutline == 2)
            result.rgb -= (strength - 1.0) * fsOverlay.rgb;
        result.a = 1.0 - (1.0 - result.a) * strength;
    }
    result *= fsOverlay.a;
    if ((result.r + result.g + result.b + result.a) < 0.02) discard;
    outColor = result;
}
";

        public readonly OpenGLRenderingDevice DeviceWrapper;
        public readonly RenderingTextureAllocator TextureAllocator;
        private readonly GL _gl;

        private uint _program;
        private uint _vao;
        private uint _vbo;
        private int _vertexCount;

        // SoA stream offsets within _vbo
        private uint _offPositions, _offColors, _offOverlays, _offUvwBlend, _offEditorUv;

        public unsafe OpenGLRenderingDrawingRoom(OpenGLRenderingDevice device, Description description)
        {
            DeviceWrapper = device;
            _gl = device.Gl;
            TextureAllocator = description.TextureAllocator;

            BuildVertexBuffer(description);
            if (_vertexCount == 0) return;

            _program = device.CreateProgram(VertexShaderGlsl, FragmentShaderGlsl,
                uniformBlocks: new[] { ("FrameData", OpenGLRenderingStateBuffer.BindingPoint) },
                samplers: new[] { ("AtlasSampler", 0), ("SectorTextureSampler", 1) });

            // Setup VAO with 5 SoA vertex attribute bindings
            uint vao = 0; _gl.GenVertexArrays(1, &vao); _vao = vao;
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            // location 0: vec3 position (12 bytes stride)
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 12, (void*)_offPositions);

            // location 1: vec4 color as R8G8B8A8_UNorm (4 bytes stride)
            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, 4, (void*)_offColors);

            // location 2: vec4 overlay as R8G8B8A8_UNorm (4 bytes stride)
            _gl.EnableVertexAttribArray(2);
            _gl.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, 4, (void*)_offOverlays);

            // location 3: uvec2 uvwBlend (8 bytes stride)
            _gl.EnableVertexAttribArray(3);
            _gl.VertexAttribIPointer(3, 2, VertexAttribIType.UnsignedInt, 8, (void*)_offUvwBlend);

            // location 4: uint editorUv (4 bytes stride)
            _gl.EnableVertexAttribArray(4);
            _gl.VertexAttribIPointer(4, 1, VertexAttribIType.UnsignedInt, 4, (void*)_offEditorUv);

            _gl.BindVertexArray(0);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }

        private unsafe void BuildVertexBuffer(Description description)
        {
            RoomGeometry roomGeometry = description.Room.RoomGeometry;
            float maxTexCoordSpan = description.Room.Level?.IsTombEngine == true ? 1024.0f : 256.0f;
            Vector3 worldPos = description.Room.WorldPos + description.Offset;
            int singleSided = roomGeometry.VertexPositions.Count;
            int totalVerts = singleSided + roomGeometry.DoubleSidedTriangleCount * 3;
            _vertexCount = totalVerts;
            if (totalVerts == 0) return;

            uint bytesPos     = (uint)(totalVerts * 12);
            uint bytesCol     = (uint)(totalVerts * 4);
            uint bytesOverlay = (uint)(totalVerts * 4);
            uint bytesUvw     = (uint)(totalVerts * 8);
            uint bytesEditor  = (uint)(totalVerts * 4);
            uint total = bytesPos + bytesCol + bytesOverlay + bytesUvw + bytesEditor;

            _offPositions = 0;
            _offColors    = bytesPos;
            _offOverlays  = bytesPos + bytesCol;
            _offUvwBlend  = bytesPos + bytesCol + bytesOverlay;
            _offEditorUv  = bytesPos + bytesCol + bytesOverlay + bytesUvw;

            byte[] staging = new byte[total];
            fixed (byte* data = staging)
            {
                Vector3* positions = (Vector3*)data;
                uint* colors    = (uint*)(data + _offColors);
                uint* overlays  = (uint*)(data + _offOverlays);
                ulong* uvwBlend = (ulong*)(data + _offUvwBlend);
                uint* editorUv  = (uint*)(data + _offEditorUv);

                Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(TextureAllocator.Size.X, TextureAllocator.Size.Y);

                for (int i = 0; i < singleSided; ++i)
                    positions[i] = roomGeometry.VertexPositions[i] + worldPos;
                for (int i = 0; i < singleSided; ++i)
                    colors[i] = CompressColor(roomGeometry.VertexColors[i]);

                for (int i = 0; i < singleSided; ++i)
                {
                    Vector2 vertexEditorUv = roomGeometry.VertexEditorUVs[i];
                    uint eu = 0;
                    eu |= (uint)((int)vertexEditorUv.X) & 3;
                    eu |= ((uint)((int)vertexEditorUv.Y) & 3) << 2;
                    editorUv[i] = eu;
                }

                {
                    SectorFaceIdentity lastFace = new SectorFaceIdentity(-1, -1, SectorFace.Floor);
                    uint lastSectorTex = 0;
                    uint overlay = 0;
                    for (int i = 0, tri = singleSided / 3; i < tri; ++i)
                    {
                        SectorFaceIdentity face = roomGeometry.TriangleSectorInfo[i];
                        if (!lastFace.Equals(face))
                        {
                            SectorTextureResult result = description.SectorTextureGet(description.Room, face.Position.X, face.Position.Y, face.Face);
                            lastFace = face;
                            lastSectorTex = 0;
                            if (result.SectorTexture != SectorTexture.None)
                                lastSectorTex = 0x40 | (((uint)result.SectorTexture - 1) << 8);
                            else
                                lastSectorTex =
                                    (((uint)(result.Color.X * 255)) << 8)  |
                                    (((uint)(result.Color.Y * 255)) << 16) |
                                    (((uint)(result.Color.Z * 255)) << 24);
                            if (result.Highlighted) lastSectorTex |= 0x10;
                            if (result.Dimmed)      lastSectorTex |= 0x20;
                            if (result.Selected && roomGeometry.TriangleTextureAreas[i].Texture != null) lastSectorTex |= 0x80;
                            overlay = CompressColor(new Vector3(result.Overlay.X, result.Overlay.Y, result.Overlay.Z), result.Hidden ? 0.4f : 1.0f);
                        }
                        editorUv[i * 3 + 0] |= lastSectorTex;
                        editorUv[i * 3 + 1] |= lastSectorTex;
                        editorUv[i * 3 + 2] |= lastSectorTex;
                        overlays[i * 3 + 0] = overlay;
                        overlays[i * 3 + 1] = overlay;
                        overlays[i * 3 + 2] = overlay;
                    }
                }

                {
                    int dsIdx = singleSided;
                    for (int i = 0, tri = singleSided / 3; i < tri; ++i)
                    {
                        TextureArea texture = roomGeometry.TriangleTextureAreas[i];
                        if (texture.Texture == null)
                        {
                            uvwBlend[i * 3 + 0] = 1ul << 24;
                            uvwBlend[i * 3 + 1] = 1ul << 24;
                            uvwBlend[i * 3 + 2] = 1ul << 24;
                        }
                        else if (texture.Texture is TextureInvisible)
                        {
                            uvwBlend[i * 3 + 0] = 0ul;
                            uvwBlend[i * 3 + 1] = 0ul;
                            uvwBlend[i * 3 + 2] = 0ul;
                        }
                        else
                        {
                            VectorInt3 pos;
                            if (texture.Texture.IsUnavailable || texture.AreTriangleCoordsOutOfBounds(maxTexCoordSpan))
                            {
                                pos = new VectorInt3(0, 0, 0);
                                uvwBlend[i * 3 + 0] = CompressUvw(pos, textureScaling, Vector2.Zero, (uint)texture.BlendMode);
                                uvwBlend[i * 3 + 1] = uvwBlend[i * 3 + 0];
                                uvwBlend[i * 3 + 2] = uvwBlend[i * 3 + 0];
                            }
                            else
                            {
                                pos = TextureAllocator.GetForTriangle(texture);
                                uvwBlend[i * 3 + 0] = CompressUvw(pos, textureScaling, texture.TexCoord0, (uint)texture.BlendMode);
                                uvwBlend[i * 3 + 1] = CompressUvw(pos, textureScaling, texture.TexCoord1, (uint)texture.BlendMode);
                                uvwBlend[i * 3 + 2] = CompressUvw(pos, textureScaling, texture.TexCoord2, (uint)texture.BlendMode);
                            }

                            if (texture.DoubleSided)
                            {
                                positions[dsIdx]   = positions[i * 3 + 2];
                                colors[dsIdx]     = colors[i * 3 + 2];
                                overlays[dsIdx]   = overlays[i * 3 + 2];
                                uvwBlend[dsIdx]   = uvwBlend[i * 3 + 2];
                                editorUv[dsIdx++] = editorUv[i * 3 + 2];

                                positions[dsIdx]   = positions[i * 3 + 1];
                                colors[dsIdx]     = colors[i * 3 + 1];
                                overlays[dsIdx]   = overlays[i * 3 + 1];
                                uvwBlend[dsIdx]   = uvwBlend[i * 3 + 1];
                                editorUv[dsIdx++] = editorUv[i * 3 + 1];

                                positions[dsIdx]   = positions[i * 3 + 0];
                                colors[dsIdx]     = colors[i * 3 + 0];
                                overlays[dsIdx]   = overlays[i * 3 + 0];
                                uvwBlend[dsIdx]   = uvwBlend[i * 3 + 0];
                                editorUv[dsIdx++] = editorUv[i * 3 + 0];
                            }
                        }
                    }
                }
            }

            // Upload to GL buffer
            uint vbo = 0;
            _gl.GenBuffers(1, &vbo);
            _vbo = vbo;
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            fixed (byte* src = staging)
                _gl.BufferData(BufferTargetARB.ArrayBuffer, total, src, BufferUsageARB.StaticDraw);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }

        private static uint CompressColor(Vector3 c)
        {
            uint r = (uint)Math.Clamp((int)(c.X * 127.5f), 0, 255);
            uint g = (uint)Math.Clamp((int)(c.Y * 127.5f), 0, 255);
            uint b = (uint)Math.Clamp((int)(c.Z * 127.5f), 0, 255);
            return r | (g << 8) | (b << 16) | (255u << 24);
        }

        private static uint CompressColor(Vector3 c, float alpha)
        {
            uint r = (uint)Math.Clamp((int)(c.X * 255f), 0, 255);
            uint g = (uint)Math.Clamp((int)(c.Y * 255f), 0, 255);
            uint b = (uint)Math.Clamp((int)(c.Z * 255f), 0, 255);
            uint a = (uint)Math.Clamp((int)(alpha * 255f), 0, 255);
            return r | (g << 8) | (b << 16) | (a << 24);
        }

        private static ulong CompressUvw(VectorInt3 pos, Vector2 scale, Vector2 uv, uint blend)
        {
            uint u = (uint)Math.Clamp((int)((pos.X + uv.X) * scale.X), 0, 0xffffff);
            uint v = (uint)Math.Clamp((int)((pos.Y + uv.Y) * scale.Y), 0, 0xffffff);
            uint w = (uint)pos.Z;
            ulong lo = u | ((ulong)(v & 0xff) << 24);
            ulong hi = ((v >> 8) & 0xffff) | ((w & 0xfff) << 16) | ((blend & 0xf) << 28);
            return lo | (hi << 32);
        }

        public override unsafe void Render(RenderArgs arg)
        {
            if (_vertexCount == 0) return;

            var stateBuffer = (OpenGLRenderingStateBuffer)arg.StateBuffer;
            var allocator = TextureAllocator as OpenGLRenderingTextureAllocator;
            if (allocator == null) return;

            _gl.UseProgram(_program);
            stateBuffer.Bind();

            // Bind atlas texture at unit 0
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2DArray, allocator.AtlasTexture);
            _gl.BindSampler(0, arg.BilinearFilter ? DeviceWrapper.SamplerAniso : DeviceWrapper.SamplerPoint);

            // Bind sector texture array at unit 1
            _gl.ActiveTexture(TextureUnit.Texture1);
            _gl.BindTexture(TextureTarget.Texture2DArray, DeviceWrapper.SectorTextureArray);
            _gl.BindSampler(1, DeviceWrapper.SamplerAniso);

            // Draw state: premultiplied alpha, depth test, backface culling
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthFunc(DepthFunction.Lequal);
            _gl.DepthMask(true);
            _gl.Enable(EnableCap.CullFace);
            _gl.CullFace(TriangleFace.Back);

            _gl.BindVertexArray(_vao);
            _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)_vertexCount);

            // Restore
            _gl.BindVertexArray(0);
            _gl.BindSampler(0, 0);
            _gl.BindSampler(1, 0);
            _gl.ActiveTexture(TextureUnit.Texture0);
        }

        public override unsafe void Dispose()
        {
            if (_vbo != 0) { uint b = _vbo; _gl.DeleteBuffers(1, &b); _vbo = 0; }
            if (_vao != 0) { uint v = _vao; _gl.DeleteVertexArrays(1, &v); _vao = 0; }
            if (_program != 0) { _gl.DeleteProgram(_program); _program = 0; }
        }
    }
}
