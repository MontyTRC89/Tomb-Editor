using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.Shaderc;
using Silk.NET.Vulkan;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorStructs;
using TombLib.Utils;
using VkBuffer = Silk.NET.Vulkan.Buffer;
using VkPipeline = Silk.NET.Vulkan.Pipeline;

namespace TombLib.Rendering.Vulkan
{
    // Vulkan port of the room drawing batch. Mirrors Dx11RenderingDrawingRoom
    // for the geometry + atlas path:
    //   - 5-stream SoA vertex buffer in a single VkBuffer:
    //       stream 0  POSITION    vec3   12 B  (binding 0)
    //       stream 1  COLOR       uint    4 B  (binding 1, R8G8B8A8_UNorm)
    //       stream 2  OVERLAY     uint    4 B  (binding 2, R8G8B8A8_UNorm)
    //       stream 3  UVW+BLEND   ulong   8 B  (binding 3, R32G32_Uint pair)
    //       stream 4  EDITOR_UV   uint    4 B  (binding 4, R32_Uint)
    //   - Single pipeline (no per-room state cross-product). Front-face winding
    //     handles double-sided triangles via duplicate verts (same as Dx11).
    //
    // What this implementation does NOT yet do (parity gaps relative to Dx11):
    //   - Sector-texture array overlay (arrows etc.). The slot is reserved in the
    //     shader but bound to a dummy texture for now. SectorTextureArray plumbing
    //     is a deferred follow-up.
    //   - Brush overlay (the brush UI cursor). Stubbed out — the fragment shader
    //     reads BrushShape from the FrameData UBO but ignores it.
    //   - Extra blending modes (subtractive, exclude, lighten, screen visuals).
    //     Currently only Opaque / NonPremultipliedAlpha / Additive are wired.
    //   - Texture atlas GC adjust callback. Will be added when WadRenderer is
    //     ported; the simpler path here builds the VB once and never rebuilds.
    public sealed class VulkanDrawingRoom : RenderingDrawingRoom
    {
        private const string VertexShaderGlsl = @"#version 450
layout(set = 0, binding = 0) uniform FrameData {
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
layout(location = 0) out vec4  fsColor;
layout(location = 1) out vec4  fsOverlay;
layout(location = 2) out vec3  fsUvw;
layout(location = 3) out flat int fsBlendMode;
layout(location = 4) out flat int fsEditorSectorTexture;
layout(location = 5) out vec3  fsWorldPos;
layout(location = 6) out vec2  fsEditorUv;
void main() {
    gl_Position = TransformMatrix * vec4(inPosition, 1.0);

    // Vertex color processing — Dx11 multiplies RGB by 2 (HDR-style), keeps A.
    vec4 col = (RoomDisableVertexColors != 0)
        ? vec4(1.0) : vec4(inColor.rgb * 2.0, inColor.a);
    if (LightMode == 1) {
        // 5-bit quantize per channel
        col = vec4(floor(col.rgb * 32.0) / 32.0, col.a);
    } else if (LightMode == 2) {
        float luma = dot(col.rgb, vec3(0.2126, 0.7152, 0.0722));
        col = vec4(vec3(luma), col.a);
    }
    fsColor = col;
    fsOverlay = inOverlay;

    // Decompress UV (fixed point /16777216) and W (raw int).
    float u = float(inUvwBlend.x & 0xffffffu) / 16777216.0;
    float v = float((inUvwBlend.x >> 24) | ((inUvwBlend.y & 0xffffu) << 8)) / 16777216.0;
    float w = float((inUvwBlend.y >> 16) & 0xfffu);
    fsUvw = vec3(u, v, w);
    fsBlendMode = int(inUvwBlend.y >> 28);
    fsEditorSectorTexture = int(inEditorUv);
    fsWorldPos = inPosition;
    // 2-bit signed editor-grid UV (corner index in {-2..1}), one bit per axis.
    // Sign-extend by shifting left to bit 30 then arithmetic right-shift 30.
    fsEditorUv = vec2(
        float((int(inEditorUv) << 30) >> 30),
        float((int(inEditorUv >> 2) << 30) >> 30));
}
";
        private const string FragmentShaderGlsl = @"#version 450
layout(set = 0, binding = 0) uniform FrameData {
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
layout(set = 1, binding = 0) uniform sampler2DArray AtlasSampler;
layout(set = 2, binding = 0) uniform sampler2DArray SectorTextureSampler;
layout(location = 0) in vec4 fsColor;
layout(location = 1) in vec4 fsOverlay;
layout(location = 2) in vec3 fsUvw;
layout(location = 3) in flat int fsBlendMode;
layout(location = 4) in flat int fsEditorSectorTexture;
layout(location = 5) in vec3 fsWorldPos;
layout(location = 6) in vec2 fsEditorUv;
layout(location = 0) out vec4 outColor;
float ddAny(float v) { return length(vec2(dFdx(v), dFdy(v))); }
void main() {
    vec4 result;
    int drawOutline = 0;
    if (fsUvw.x != 0.0 && RoomGridForce == 0) {
        // Textured view — sample atlas, apply vertex tint.
        vec4 texel = texture(AtlasSampler, fsUvw);
        if (ShowLightingWhiteTextureOnly != 0 && RoomDisableVertexColors == 0)
            texel = vec4(1.0);
        vec3 colorAdd = max(fsColor.rgb - 1.0, 0.0) * 0.37;
        vec3 colorMul = min(fsColor.rgb, 1.0);
        texel.rgb = texel.rgb * colorMul + colorAdd;
        texel.a *= fsColor.a;
        // Premultiply alpha (matches Dx11)
        texel.rgb *= fsColor.a;
        texel.rgb *= texel.a;
        if (fsBlendMode >= 2) texel.a = 0.0;   // alpha-blended modes: leave RGB
        result = texel;
        if (texel.a < 0.005 && fsBlendMode < 2) discard;
    } else {
        // Untextured (geometry view). Use sector color encoded in EditorUv bits,
        // or the per-vertex Overlay when an arrow would have been shown (we
        // don't yet sample SectorTexture).
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
                // Sector color + arrow/slide overlay (sampler at layer N-1
                // where N is extracted from EditorSectorTexture bits 8..15).
                result = fsOverlay;
                if ((sectorBits & 0x20u) != 0u) result.rgb *= 0.70;
                float layer = float((sectorBits >> 8) & 0xffu);
                vec4 texColor = texture(SectorTextureSampler, vec3(fsEditorUv, layer));
                // Dx11 RoomShaderPS uses brightness(result) > 0.8 to decide
                // overlay direction. Match it here.
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

    // Sector outline — same math as Dx11 RoomShaderPS: derivative-thickness
    // black border at each sector edge + diagonal. Produces the editor's
    // characteristic 'grid' look in Geometry mode (and for selected-textured
    // faces).
    //
    // The approach: for each of the three candidate edges (horizontal, vertical,
    // diagonal) compute the fragment's distance-to-edge in UV space, then divide
    // by the screen-space derivative of that coordinate to get a pixel count.
    // Subtract a line-width threshold (scaled by 1/clipW for perspective) and
    // clamp to [0,1] — this gives a smooth anti-aliased edge mask.
    if (drawOutline > 0) {
        vec2 absUV = abs(fsEditorUv);
        // HLSL SV_POSITION.w in PS is clip-space W; GLSL gl_FragCoord.w is
        // 1/clipW. So to match the Dx11 formula `* 1024 / position.w` we
        // multiply by gl_FragCoord.w here (= 1/clipW = same numerical value).
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

        public readonly VulkanRenderingDevice DeviceWrapper;
        public readonly RenderingTextureAllocator TextureAllocator;
        private readonly Vk _vk;
        private readonly Device _device;

        private ShaderModule _vs;
        private ShaderModule _fs;
        private DescriptorSetLayout _set0, _set1, _set2;
        private PipelineLayout _pipelineLayout;
        private VkPipeline _pipeline;

        private VkBuffer _vertexBuffer;
        private DeviceMemory _vertexMemory;
        private int _vertexCount;

        // Sub-buffer offsets into the SoA vertex buffer. Each stream occupies a
        // contiguous region: [positions | colors | overlays | uvwBlend | editorUv].
        private uint _offPositions, _offColors, _offOverlays, _offUvwBlend, _offEditorUv;

        private DescriptorSet _frameSet, _atlasSet, _sectorSet;
        private VkBuffer _cachedStateBuffer;
        private ulong _cachedAtlas;
        private bool _cachedBilinear;
        private ulong _cachedSectorView;

        public unsafe VulkanDrawingRoom(VulkanRenderingDevice device, Description description)
        {
            DeviceWrapper = device;
            _vk = device.Vk;
            _device = device.Device;
            TextureAllocator = description.TextureAllocator;

            BuildVertexBuffer(description);
            if (_vertexCount == 0) return;

            byte[] vsSpirv = device.ShaderCompiler.CompileGlslToSpirv(VertexShaderGlsl,   ShaderKind.VertexShader,   "RoomVS");
            byte[] fsSpirv = device.ShaderCompiler.CompileGlslToSpirv(FragmentShaderGlsl, ShaderKind.FragmentShader, "RoomFS");
            _vs = VulkanShaderCompiler.CreateShaderModule(_vk, _device, vsSpirv);
            _fs = VulkanShaderCompiler.CreateShaderModule(_vk, _device, fsSpirv);

            // Descriptor set layouts — note the difference from DrawingMesh:
            //   set 0: FrameData UBO (camera matrix + editor uniforms)
            //   set 1: Atlas combined image sampler (Texture2DArray)
            //   set 2: Sector texture array sampler (editor arrows/overlays)
            // Rooms do NOT use a dynamic UBO for per-draw data because room
            // geometry is drawn in a single vkCmdDraw — all transform state lives
            // in FrameData (the view-projection matrix) and vertex attributes.
            _set0 = CreateSetLayoutSingle(DescriptorType.UniformBuffer,        ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit);
            _set1 = CreateSetLayoutSingle(DescriptorType.CombinedImageSampler, ShaderStageFlags.FragmentBit);
            _set2 = CreateSetLayoutSingle(DescriptorType.CombinedImageSampler, ShaderStageFlags.FragmentBit);

            var layouts = stackalloc DescriptorSetLayout[3] { _set0, _set1, _set2 };
            PipelineLayoutCreateInfo plci = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 3,
                PSetLayouts = layouts,
            };
            PipelineLayout pl;
            VkCheck.Ok(_vk.CreatePipelineLayout(_device, in plci, null, &pl));
            _pipelineLayout = pl;

            _frameSet = AllocateSet(_set0);
            _atlasSet = AllocateSet(_set1);
            _sectorSet = AllocateSet(_set2);
        }

        // Builds the 5-stream SoA (Structure of Arrays) vertex buffer.
        //
        // The SoA layout mirrors the Dx11 path for two reasons:
        //   1. Interop — a future hybrid renderer could share CPU-side vertex
        //      assembly code between the two backends.
        //   2. Cache friendliness — the GPU fetches only the streams it needs per
        //      shader stage (e.g. the fragment shader never reads positions), which
        //      reduces memory bandwidth compared to an interleaved AoS layout.
        //
        // The five streams are packed contiguously in one VkBuffer:
        //   positions[N]  — vec3   (12 B each)
        //   colors[N]     — uint   ( 4 B each, R8G8B8A8_UNorm)
        //   overlays[N]   — uint   ( 4 B each, R8G8B8A8_UNorm)
        //   uvwBlend[N]   — ulong  ( 8 B each, R32G32_Uint)
        //   editorUv[N]   — uint   ( 4 B each, R32_Uint)
        //
        // Double-sided triangles are appended at the tail of the buffer with
        // reversed winding order (v2, v1, v0 instead of v0, v1, v2). This avoids
        // needing CullMode.None which would z-fight the original face.
        private unsafe void BuildVertexBuffer(Description description)
        {
            RoomGeometry roomGeometry = description.Room.RoomGeometry;
            float maxTexCoordSpan = description.Room.Level?.IsTombEngine == true ? 1024.0f : 256.0f;
            Vector3 worldPos = description.Room.WorldPos + description.Offset;
            int singleSided = roomGeometry.VertexPositions.Count;
            int totalVerts = singleSided + roomGeometry.DoubleSidedTriangleCount * 3;
            _vertexCount = totalVerts;
            if (totalVerts == 0) return;

            // SoA byte layout: positions[N] | colors[N] | overlays[N] | uvwBlend[N] | editorUv[N]
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
                Vector3* positions = (Vector3*)(data);
                uint* colors    = (uint*)(data + _offColors);
                uint* overlays  = (uint*)(data + _offOverlays);
                ulong* uvwBlend = (ulong*)(data + _offUvwBlend);
                uint* editorUv  = (uint*)(data + _offEditorUv);

                Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(TextureAllocator.Size.X, TextureAllocator.Size.Y);

                // Pass 1: world-space positions and compressed vertex colors.
                for (int i = 0; i < singleSided; ++i)
                    positions[i] = roomGeometry.VertexPositions[i] + worldPos;
                for (int i = 0; i < singleSided; ++i)
                    colors[i] = CompressColor(roomGeometry.VertexColors[i]);

                // Pass 2: per-triangle sector overlay + per-vertex editor UV.
                // The editorUv packing (see CompressEditorUv / inEditorUv in the VS):
                //   bits [1:0]   — signed 2-bit X corner index (-2..1)
                //   bits [3:2]   — signed 2-bit Y corner index (-2..1)
                //   bit  4       — Highlighted flag
                //   bit  5       — Dimmed flag
                //   bit  6       — HasSectorTexture flag
                //   bit  7       — Selected-textured flag
                //   bits [15:8]  — SectorTexture layer index (or R channel of color)
                //   bits [23:16] — G channel of sector color
                //   bits [31:24] — B channel of sector color
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

                // Pass 3: per-triangle UVW allocation (no retry — atlas GC adjust
                // is wired only when WadRenderer is ported).
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
                                // Placeholder texture path (a real "unavailable"
                                // image is only baked into Dx11RenderingDevice
                                // today; fall back to the first atlas slot which
                                // is harmless visually).
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

                            // Append double-sided triangles with reversed winding
                            // (v2, v1, v0) at the tail of the vertex buffer.
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

            // Stage + upload to a device-local VkBuffer (same staging pattern
            // as VulkanDrawingMesh.CreateImmutableBufferFromArray).
            BufferCreateInfo stagingInfo = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = total,
                Usage = BufferUsageFlags.TransferSrcBit,
                SharingMode = SharingMode.Exclusive,
            };
            VkBuffer stagingBuf;
            VkCheck.Ok(_vk.CreateBuffer(_device, in stagingInfo, null, &stagingBuf));
            _vk.GetBufferMemoryRequirements(_device, stagingBuf, out MemoryRequirements stReq);
            MemoryAllocateInfo stAlloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = stReq.Size,
                MemoryTypeIndex = DeviceWrapper.FindMemoryType(stReq.MemoryTypeBits,
                    MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit),
            };
            DeviceMemory stMem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in stAlloc, null, &stMem));
            _vk.BindBufferMemory(_device, stagingBuf, stMem, 0);
            void* mapped;
            _vk.MapMemory(_device, stMem, 0, total, 0, &mapped);
            fixed (byte* src = staging) System.Buffer.MemoryCopy(src, mapped, total, total);
            _vk.UnmapMemory(_device, stMem);

            BufferCreateInfo bufInfo = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = total,
                Usage = BufferUsageFlags.VertexBufferBit | BufferUsageFlags.TransferDstBit,
                SharingMode = SharingMode.Exclusive,
            };
            VkBuffer buf;
            VkCheck.Ok(_vk.CreateBuffer(_device, in bufInfo, null, &buf));
            _vk.GetBufferMemoryRequirements(_device, buf, out MemoryRequirements req);
            MemoryAllocateInfo alloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size,
                MemoryTypeIndex = DeviceWrapper.FindMemoryType(req.MemoryTypeBits,
                    MemoryPropertyFlags.DeviceLocalBit),
            };
            DeviceMemory mem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in alloc, null, &mem));
            _vk.BindBufferMemory(_device, buf, mem, 0);

            var cb = DeviceWrapper.BeginTransient();
            BufferCopy region = new BufferCopy { Size = total };
            _vk.CmdCopyBuffer(cb, stagingBuf, buf, 1, in region);
            DeviceWrapper.EndAndSubmitTransient(cb);

            _vk.DestroyBuffer(_device, stagingBuf, null);
            _vk.FreeMemory(_device, stMem, null);

            _vertexBuffer = buf;
            _vertexMemory = mem;
        }

        // Compresses a vertex color (range 0..2, HDR-style) into R8G8B8A8_UNorm.
        // The multiplier 127.5 maps the [0..2] HDR range into [0..255]:
        //   color=0.0 → 0,  color=1.0 → 127/128,  color=2.0 → 255.
        // The vertex shader recovers the original range by multiplying * 2.0.
        // Alpha is fixed to 255 (fully opaque) in this overload.
        private static uint CompressColor(Vector3 c)
        {
            uint r = (uint)Math.Clamp((int)(c.X * 127.5f), 0, 255);
            uint g = (uint)Math.Clamp((int)(c.Y * 127.5f), 0, 255);
            uint b = (uint)Math.Clamp((int)(c.Z * 127.5f), 0, 255);
            return r | (g << 8) | (b << 16) | (255u << 24);
        }

        // Overload for overlay colors with explicit alpha. Uses standard 255
        // scaling (not the 127.5 HDR multiplier) because overlay colors are
        // already in [0..1] range.
        private static uint CompressColor(Vector3 c, float alpha)
        {
            uint r = (uint)Math.Clamp((int)(c.X * 255f), 0, 255);
            uint g = (uint)Math.Clamp((int)(c.Y * 255f), 0, 255);
            uint b = (uint)Math.Clamp((int)(c.Z * 255f), 0, 255);
            uint a = (uint)Math.Clamp((int)(alpha * 255f), 0, 255);
            return r | (g << 8) | (b << 16) | (a << 24);
        }

        // Packs atlas UV + layer index + blend mode into a 64-bit value.
        //
        // Bit layout (lo = bits 0..31, hi = bits 32..63):
        //   lo[23: 0]  U  — 24-bit fixed-point (value / 16777216 = normalized U)
        //   lo[31:24]  V low 8 bits
        //   hi[15: 0]  V high 16 bits  (total 24-bit V, same scale as U)
        //   hi[27:16]  W  — 12-bit unsigned atlas layer index
        //   hi[31:28]  Blend mode (4 bits, maps to BlendMode enum)
        //
        // The VS decompresses with bit shifts and divides by 16777216.0 to get
        // normalized [0..1] texture coordinates. The 24-bit precision gives
        // sub-pixel accuracy for atlas sizes up to 16384x16384.
        private static ulong CompressUvw(VectorInt3 pos, Vector2 scale, Vector2 uv, uint blend)
        {
            uint u = (uint)Math.Clamp((int)((pos.X + uv.X) * scale.X), 0, 0xffffff);
            uint v = (uint)Math.Clamp((int)((pos.Y + uv.Y) * scale.Y), 0, 0xffffff);
            uint w = (uint)pos.Z;
            ulong lo = u | ((ulong)(v & 0xff) << 24);
            ulong hi = ((v >> 8) & 0xffff) | ((w & 0xfff) << 16) | ((blend & 0xf) << 28);
            return lo | (hi << 32);
        }

        // Creates a descriptor set layout with a single binding at index 0.
        private unsafe DescriptorSetLayout CreateSetLayoutSingle(DescriptorType type, ShaderStageFlags stages)
        {
            DescriptorSetLayoutBinding b = new DescriptorSetLayoutBinding
            {
                Binding = 0,
                DescriptorCount = 1,
                DescriptorType = type,
                StageFlags = stages,
            };
            DescriptorSetLayoutCreateInfo info = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 1,
                PBindings = &b,
            };
            DescriptorSetLayout l;
            VkCheck.Ok(_vk.CreateDescriptorSetLayout(_device, in info, null, &l));
            return l;
        }

        private unsafe DescriptorSet AllocateSet(DescriptorSetLayout layout)
        {
            DescriptorSetAllocateInfo info = new DescriptorSetAllocateInfo
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = DeviceWrapper.DescriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = &layout,
            };
            DescriptorSet s;
            VkCheck.Ok(_vk.AllocateDescriptorSets(_device, in info, &s));
            return s;
        }

        // Updates a descriptor set to point at a standard (non-dynamic) UBO.
        private unsafe void WriteUbo(DescriptorSet set, VkBuffer buffer, ulong range)
        {
            DescriptorBufferInfo bi = new DescriptorBufferInfo
            {
                Buffer = buffer,
                Offset = 0,
                Range = range,
            };
            WriteDescriptorSet w = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = set,
                DstBinding = 0,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBuffer,
                PBufferInfo = &bi,
            };
            _vk.UpdateDescriptorSets(_device, 1, in w, 0, null);
        }

        // Updates a descriptor set to point at a combined image sampler.
        private unsafe void WriteAtlas(DescriptorSet set, ImageView view, Sampler sampler)
        {
            DescriptorImageInfo ii = new DescriptorImageInfo
            {
                Sampler = sampler,
                ImageView = view,
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
            };
            WriteDescriptorSet w = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = set,
                DstBinding = 0,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.CombinedImageSampler,
                PImageInfo = &ii,
            };
            _vk.UpdateDescriptorSets(_device, 1, in w, 0, null);
        }

        public override unsafe void Dispose()
        {
            _vk.DeviceWaitIdle(_device);
            if (_pipeline.Handle != 0) { _vk.DestroyPipeline(_device, _pipeline, null); _pipeline = default; }
            if (_pipelineLayout.Handle != 0) _vk.DestroyPipelineLayout(_device, _pipelineLayout, null);
            if (_set0.Handle != 0) _vk.DestroyDescriptorSetLayout(_device, _set0, null);
            if (_set1.Handle != 0) _vk.DestroyDescriptorSetLayout(_device, _set1, null);
            if (_set2.Handle != 0) _vk.DestroyDescriptorSetLayout(_device, _set2, null);
            if (_vs.Handle != 0) _vk.DestroyShaderModule(_device, _vs, null);
            if (_fs.Handle != 0) _vk.DestroyShaderModule(_device, _fs, null);
            if (_vertexBuffer.Handle != 0) _vk.DestroyBuffer(_device, _vertexBuffer, null);
            if (_vertexMemory.Handle != 0) _vk.FreeMemory(_device, _vertexMemory, null);
        }

        public override unsafe void Render(RenderArgs arg)
        {
            if (_vertexCount == 0) return;
            if (arg.RenderTarget == null)
                throw new NotSupportedException("VulkanDrawingRoom.Render requires a non-null RenderTarget.");
            var swapChain = (VulkanSwapChain)arg.RenderTarget;
            var stateBuffer = (VulkanStateBuffer)arg.StateBuffer;
            var allocator = TextureAllocator as VulkanTextureAllocator;
            if (allocator == null) return;
            var cb = swapChain.CurrentCommandBuffer;

            if (_pipeline.Handle == 0)
                _pipeline = BuildPipeline(swapChain.RenderPass, swapChain.SampleCount);

            // Re-write descriptors only when the underlying resource changes.
            if (_cachedStateBuffer.Handle != stateBuffer.Buffer.Handle)
            {
                _cachedStateBuffer = stateBuffer.Buffer;
                WriteUbo(_frameSet, stateBuffer.Buffer, VulkanStateBuffer.Size);
            }
            if (_cachedAtlas != allocator.AtlasView.Handle || _cachedBilinear != arg.BilinearFilter)
            {
                _cachedAtlas = allocator.AtlasView.Handle;
                _cachedBilinear = arg.BilinearFilter;
                WriteAtlas(_atlasSet, allocator.AtlasView, arg.BilinearFilter ? DeviceWrapper.SamplerAniso : DeviceWrapper.SamplerPoint);
            }
            if (_cachedSectorView != DeviceWrapper.SectorTextureArrayView.Handle)
            {
                _cachedSectorView = DeviceWrapper.SectorTextureArrayView.Handle;
                WriteAtlas(_sectorSet, DeviceWrapper.SectorTextureArrayView, DeviceWrapper.SamplerAniso);
            }

            _vk.CmdBindPipeline(cb, PipelineBindPoint.Graphics, _pipeline);

            // Bind all 5 SoA streams from the same VkBuffer at different offsets.
            VkBuffer vb = _vertexBuffer;
            var buffers  = stackalloc VkBuffer[5] { vb, vb, vb, vb, vb };
            var offsets = stackalloc ulong[5] { _offPositions, _offColors, _offOverlays, _offUvwBlend, _offEditorUv };
            _vk.CmdBindVertexBuffers(cb, 0, 5, buffers, offsets);

            // Bind descriptor sets: set 0 = FrameData, set 1 = atlas, set 2 = sector textures.
            // No dynamic offsets — rooms have no per-draw UBO.
            var sets = stackalloc DescriptorSet[3] { _frameSet, _atlasSet, _sectorSet };
            _vk.CmdBindDescriptorSets(cb, PipelineBindPoint.Graphics, _pipelineLayout, 0, 3, sets, 0, null);

            _vk.CmdDraw(cb, (uint)_vertexCount, 1, 0, 0);
        }

        private unsafe VkPipeline BuildPipeline(RenderPass rp, SampleCountFlags samples)
        {
            // ---- Shader stages ------------------------------------------------
            var entryName = stackalloc byte[5] { (byte)'m', (byte)'a', (byte)'i', (byte)'n', 0 };
            var stages = stackalloc PipelineShaderStageCreateInfo[2]
            {
                new PipelineShaderStageCreateInfo { SType = StructureType.PipelineShaderStageCreateInfo,
                    Stage = ShaderStageFlags.VertexBit,   Module = _vs, PName = entryName },
                new PipelineShaderStageCreateInfo { SType = StructureType.PipelineShaderStageCreateInfo,
                    Stage = ShaderStageFlags.FragmentBit, Module = _fs, PName = entryName },
            };

            // ---- Vertex input -------------------------------------------------
            // 5 bindings matching the SoA streams (see BuildVertexBuffer).
            var bindings = stackalloc VertexInputBindingDescription[5]
            {
                new VertexInputBindingDescription { Binding = 0, Stride = 12, InputRate = VertexInputRate.Vertex },
                new VertexInputBindingDescription { Binding = 1, Stride = 4,  InputRate = VertexInputRate.Vertex },
                new VertexInputBindingDescription { Binding = 2, Stride = 4,  InputRate = VertexInputRate.Vertex },
                new VertexInputBindingDescription { Binding = 3, Stride = 8,  InputRate = VertexInputRate.Vertex },
                new VertexInputBindingDescription { Binding = 4, Stride = 4,  InputRate = VertexInputRate.Vertex },
            };
            var attrs = stackalloc VertexInputAttributeDescription[5]
            {
                new VertexInputAttributeDescription { Location = 0, Binding = 0, Format = Format.R32G32B32Sfloat, Offset = 0 },
                new VertexInputAttributeDescription { Location = 1, Binding = 1, Format = Format.R8G8B8A8Unorm,  Offset = 0 },
                new VertexInputAttributeDescription { Location = 2, Binding = 2, Format = Format.R8G8B8A8Unorm,  Offset = 0 },
                new VertexInputAttributeDescription { Location = 3, Binding = 3, Format = Format.R32G32Uint,     Offset = 0 },
                new VertexInputAttributeDescription { Location = 4, Binding = 4, Format = Format.R32Uint,        Offset = 0 },
            };
            PipelineVertexInputStateCreateInfo vi = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 5,
                PVertexBindingDescriptions = bindings,
                VertexAttributeDescriptionCount = 5,
                PVertexAttributeDescriptions = attrs,
            };

            // ---- Input assembly -----------------------------------------------
            PipelineInputAssemblyStateCreateInfo ia = new PipelineInputAssemblyStateCreateInfo
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = PrimitiveTopology.TriangleList,
            };

            // ---- Viewport / scissor (dynamic) --------------------------------
            PipelineViewportStateCreateInfo vp = new PipelineViewportStateCreateInfo
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                ScissorCount = 1,
            };

            // ---- Rasterization ------------------------------------------------
            // Match Dx11RenderingDrawingRoom: backface culling on, double-sided
            // triangles are already emitted with a reversed-winding duplicate at
            // the tail of the VB. CullMode.None would draw every triangle from
            // both sides, and the duplicates would z-fight with the originals
            // and win (drawn last) — flipping each wall inside-out.
            PipelineRasterizationStateCreateInfo rs = new PipelineRasterizationStateCreateInfo
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                PolygonMode = PolygonMode.Fill,
                CullMode = CullModeFlags.BackBit,
                // D3D11 default rasterizer treats CW-in-framebuffer as front. In our
                // Vulkan setup the swap-chain uses a Y-flipped viewport — so source
                // CCW-in-NDC vertices project to CW-in-framebuffer (= negative signed
                // area). FrontFace.Clockwise tells the rasterizer "negative area =
                // front", matching D3D11 default rendering 1:1.
                FrontFace = FrontFace.Clockwise,
                LineWidth = 1.0f,
            };

            // ---- Multisampling ------------------------------------------------
            PipelineMultisampleStateCreateInfo ms = new PipelineMultisampleStateCreateInfo
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = samples,
            };

            // ---- Depth / stencil ----------------------------------------------
            PipelineDepthStencilStateCreateInfo ds = new PipelineDepthStencilStateCreateInfo
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = true,
                DepthWriteEnable = true,
                DepthCompareOp = CompareOp.LessOrEqual,
            };

            // ---- Color blending -----------------------------------------------
            // Premultiplied alpha: the fragment shader already multiplies RGB by A,
            // so Src=One, Dst=1-SrcAlpha gives correct compositing.
            ColorComponentFlags mask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit;
            PipelineColorBlendAttachmentState att = new PipelineColorBlendAttachmentState
            {
                BlendEnable = true,
                SrcColorBlendFactor = BlendFactor.One,
                DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
                ColorBlendOp = BlendOp.Add,
                SrcAlphaBlendFactor = BlendFactor.One,
                DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
                AlphaBlendOp = BlendOp.Add,
                ColorWriteMask = mask,
            };
            PipelineColorBlendStateCreateInfo cb = new PipelineColorBlendStateCreateInfo
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                AttachmentCount = 1,
                PAttachments = &att,
            };

            // ---- Dynamic state ------------------------------------------------
            var dynStates = stackalloc DynamicState[2] { DynamicState.Viewport, DynamicState.Scissor };
            PipelineDynamicStateCreateInfo dyn = new PipelineDynamicStateCreateInfo
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 2,
                PDynamicStates = dynStates,
            };

            // ---- Assemble the full pipeline -----------------------------------
            GraphicsPipelineCreateInfo gp = new GraphicsPipelineCreateInfo
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2,
                PStages = stages,
                PVertexInputState = &vi,
                PInputAssemblyState = &ia,
                PViewportState = &vp,
                PRasterizationState = &rs,
                PMultisampleState = &ms,
                PDepthStencilState = &ds,
                PColorBlendState = &cb,
                PDynamicState = &dyn,
                Layout = _pipelineLayout,
                RenderPass = rp,
                Subpass = 0,
            };
            VkPipeline pipeline;
            VkCheck.Ok(_vk.CreateGraphicsPipelines(_device, default, 1, in gp, null, &pipeline));
            return pipeline;
        }
    }
}
