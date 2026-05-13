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
layout(location = 0) in vec4 fsColor;
layout(location = 1) in vec4 fsOverlay;
layout(location = 2) in vec3 fsUvw;
layout(location = 3) in flat int fsBlendMode;
layout(location = 4) in flat int fsEditorSectorTexture;
layout(location = 5) in vec3 fsWorldPos;
layout(location = 0) out vec4 outColor;
void main() {
    vec4 result;
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
        // Untextured (geometry view). Use sector-overlay colour from
        // editor-uv high bits, OR the per-vertex Overlay stream when set.
        uint sectorBits = uint(fsEditorSectorTexture);
        uint r = (sectorBits >> 8)  & 0xffu;
        uint g = (sectorBits >> 16) & 0xffu;
        uint b = (sectorBits >> 24) & 0xffu;
        vec3 sectorColor = vec3(float(r), float(g), float(b)) / 255.0;
        bool hasSectorTex = (sectorBits & 0x40u) != 0u;
        // We don't yet sample SectorTexture in the Vulkan path — fall back to
        // the sector color (or the overlay when an arrow would be shown).
        result = vec4(hasSectorTex ? fsOverlay.rgb : sectorColor, 1.0);
        if (fsUvw.x == 0.0 && fsColor.a < 0.005) discard; // invisible flag
    }
    outColor = result;
}
";

        public readonly VulkanRenderingDevice DeviceWrapper;
        public readonly RenderingTextureAllocator TextureAllocator;
        private readonly Vk _vk;
        private readonly Device _device;

        private ShaderModule _vs;
        private ShaderModule _fs;
        private DescriptorSetLayout _set0, _set1;
        private PipelineLayout _pipelineLayout;
        private VkPipeline _pipeline;

        private VkBuffer _vertexBuffer;
        private DeviceMemory _vertexMemory;
        private int _vertexCount;

        // Sub-buffer offsets matching the Dx11 SoA layout.
        private uint _offPositions, _offColors, _offOverlays, _offUvwBlend, _offEditorUv;

        private DescriptorSet _frameSet, _atlasSet;
        private VkBuffer _cachedStateBuffer;
        private ulong _cachedAtlas;
        private bool _cachedBilinear;

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

            _set0 = CreateSetLayoutSingle(DescriptorType.UniformBuffer,        ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit);
            _set1 = CreateSetLayoutSingle(DescriptorType.CombinedImageSampler, ShaderStageFlags.FragmentBit);

            var layouts = stackalloc DescriptorSetLayout[2] { _set0, _set1 };
            PipelineLayoutCreateInfo plci = new PipelineLayoutCreateInfo
            { SType = StructureType.PipelineLayoutCreateInfo, SetLayoutCount = 2, PSetLayouts = layouts };
            PipelineLayout pl;
            VkCheck.Ok(_vk.CreatePipelineLayout(_device, in plci, null, &pl));
            _pipelineLayout = pl;

            _frameSet = AllocateSet(_set0);
            _atlasSet = AllocateSet(_set1);
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

                for (int i = 0; i < singleSided; ++i)
                    positions[i] = roomGeometry.VertexPositions[i] + worldPos;
                for (int i = 0; i < singleSided; ++i)
                    colors[i] = CompressColor(roomGeometry.VertexColors[i]);

                // Pass 2: per-triangle sector overlay + per-vertex editor uv
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

            // Stage + upload to a device-local VkBuffer.
            BufferCreateInfo stagingInfo = new BufferCreateInfo
            { SType = StructureType.BufferCreateInfo, Size = total, Usage = BufferUsageFlags.TransferSrcBit, SharingMode = SharingMode.Exclusive };
            VkBuffer stagingBuf;
            VkCheck.Ok(_vk.CreateBuffer(_device, in stagingInfo, null, &stagingBuf));
            _vk.GetBufferMemoryRequirements(_device, stagingBuf, out MemoryRequirements stReq);
            MemoryAllocateInfo stAlloc = new MemoryAllocateInfo
            { SType = StructureType.MemoryAllocateInfo, AllocationSize = stReq.Size,
              MemoryTypeIndex = DeviceWrapper.FindMemoryType(stReq.MemoryTypeBits, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit) };
            DeviceMemory stMem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in stAlloc, null, &stMem));
            _vk.BindBufferMemory(_device, stagingBuf, stMem, 0);
            void* mapped;
            _vk.MapMemory(_device, stMem, 0, total, 0, &mapped);
            fixed (byte* src = staging) System.Buffer.MemoryCopy(src, mapped, total, total);
            _vk.UnmapMemory(_device, stMem);

            BufferCreateInfo bufInfo = new BufferCreateInfo
            { SType = StructureType.BufferCreateInfo, Size = total,
              Usage = BufferUsageFlags.VertexBufferBit | BufferUsageFlags.TransferDstBit,
              SharingMode = SharingMode.Exclusive };
            VkBuffer buf;
            VkCheck.Ok(_vk.CreateBuffer(_device, in bufInfo, null, &buf));
            _vk.GetBufferMemoryRequirements(_device, buf, out MemoryRequirements req);
            MemoryAllocateInfo alloc = new MemoryAllocateInfo
            { SType = StructureType.MemoryAllocateInfo, AllocationSize = req.Size,
              MemoryTypeIndex = DeviceWrapper.FindMemoryType(req.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit) };
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

        // Same packed format the Dx11 path uses.
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

        private unsafe DescriptorSetLayout CreateSetLayoutSingle(DescriptorType type, ShaderStageFlags stages)
        {
            DescriptorSetLayoutBinding b = new DescriptorSetLayoutBinding
            { Binding = 0, DescriptorCount = 1, DescriptorType = type, StageFlags = stages };
            DescriptorSetLayoutCreateInfo info = new DescriptorSetLayoutCreateInfo
            { SType = StructureType.DescriptorSetLayoutCreateInfo, BindingCount = 1, PBindings = &b };
            DescriptorSetLayout l;
            VkCheck.Ok(_vk.CreateDescriptorSetLayout(_device, in info, null, &l));
            return l;
        }
        private unsafe DescriptorSet AllocateSet(DescriptorSetLayout layout)
        {
            DescriptorSetAllocateInfo info = new DescriptorSetAllocateInfo
            { SType = StructureType.DescriptorSetAllocateInfo, DescriptorPool = DeviceWrapper.DescriptorPool,
              DescriptorSetCount = 1, PSetLayouts = &layout };
            DescriptorSet s;
            VkCheck.Ok(_vk.AllocateDescriptorSets(_device, in info, &s));
            return s;
        }
        private unsafe void WriteUbo(DescriptorSet set, VkBuffer buffer, ulong range)
        {
            DescriptorBufferInfo bi = new DescriptorBufferInfo { Buffer = buffer, Offset = 0, Range = range };
            WriteDescriptorSet w = new WriteDescriptorSet
            { SType = StructureType.WriteDescriptorSet, DstSet = set, DstBinding = 0,
              DescriptorCount = 1, DescriptorType = DescriptorType.UniformBuffer, PBufferInfo = &bi };
            _vk.UpdateDescriptorSets(_device, 1, in w, 0, null);
        }
        private unsafe void WriteAtlas(DescriptorSet set, ImageView view, Sampler sampler)
        {
            DescriptorImageInfo ii = new DescriptorImageInfo
            { Sampler = sampler, ImageView = view, ImageLayout = ImageLayout.ShaderReadOnlyOptimal };
            WriteDescriptorSet w = new WriteDescriptorSet
            { SType = StructureType.WriteDescriptorSet, DstSet = set, DstBinding = 0,
              DescriptorCount = 1, DescriptorType = DescriptorType.CombinedImageSampler, PImageInfo = &ii };
            _vk.UpdateDescriptorSets(_device, 1, in w, 0, null);
        }

        public override unsafe void Dispose()
        {
            _vk.DeviceWaitIdle(_device);
            if (_pipeline.Handle != 0) { _vk.DestroyPipeline(_device, _pipeline, null); _pipeline = default; }
            if (_pipelineLayout.Handle != 0) _vk.DestroyPipelineLayout(_device, _pipelineLayout, null);
            if (_set0.Handle != 0) _vk.DestroyDescriptorSetLayout(_device, _set0, null);
            if (_set1.Handle != 0) _vk.DestroyDescriptorSetLayout(_device, _set1, null);
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
                _pipeline = BuildPipeline(swapChain.RenderPass);
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

            _vk.CmdBindPipeline(cb, PipelineBindPoint.Graphics, _pipeline);

            VkBuffer vb = _vertexBuffer;
            var buffers  = stackalloc VkBuffer[5] { vb, vb, vb, vb, vb };
            var offsets = stackalloc ulong[5] { _offPositions, _offColors, _offOverlays, _offUvwBlend, _offEditorUv };
            _vk.CmdBindVertexBuffers(cb, 0, 5, buffers, offsets);

            var sets = stackalloc DescriptorSet[2] { _frameSet, _atlasSet };
            _vk.CmdBindDescriptorSets(cb, PipelineBindPoint.Graphics, _pipelineLayout, 0, 2, sets, 0, null);

            _vk.CmdDraw(cb, (uint)_vertexCount, 1, 0, 0);
        }

        private unsafe VkPipeline BuildPipeline(RenderPass rp)
        {
            var entryName = stackalloc byte[5] { (byte)'m', (byte)'a', (byte)'i', (byte)'n', 0 };
            var stages = stackalloc PipelineShaderStageCreateInfo[2]
            {
                new PipelineShaderStageCreateInfo { SType = StructureType.PipelineShaderStageCreateInfo,
                    Stage = ShaderStageFlags.VertexBit,   Module = _vs, PName = entryName },
                new PipelineShaderStageCreateInfo { SType = StructureType.PipelineShaderStageCreateInfo,
                    Stage = ShaderStageFlags.FragmentBit, Module = _fs, PName = entryName },
            };
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
            { SType = StructureType.PipelineVertexInputStateCreateInfo, VertexBindingDescriptionCount = 5,
              PVertexBindingDescriptions = bindings, VertexAttributeDescriptionCount = 5, PVertexAttributeDescriptions = attrs };
            PipelineInputAssemblyStateCreateInfo ia = new PipelineInputAssemblyStateCreateInfo
            { SType = StructureType.PipelineInputAssemblyStateCreateInfo, Topology = PrimitiveTopology.TriangleList };
            PipelineViewportStateCreateInfo vp = new PipelineViewportStateCreateInfo
            { SType = StructureType.PipelineViewportStateCreateInfo, ViewportCount = 1, ScissorCount = 1 };
            PipelineRasterizationStateCreateInfo rs = new PipelineRasterizationStateCreateInfo
            { SType = StructureType.PipelineRasterizationStateCreateInfo, PolygonMode = PolygonMode.Fill,
              CullMode = CullModeFlags.None,
              FrontFace = FrontFace.Clockwise,   // compensates VulkanSwapChain's Y-flipped viewport
              LineWidth = 1.0f };
            PipelineMultisampleStateCreateInfo ms = new PipelineMultisampleStateCreateInfo
            { SType = StructureType.PipelineMultisampleStateCreateInfo, RasterizationSamples = SampleCountFlags.Count1Bit };
            PipelineDepthStencilStateCreateInfo ds = new PipelineDepthStencilStateCreateInfo
            { SType = StructureType.PipelineDepthStencilStateCreateInfo, DepthTestEnable = true, DepthWriteEnable = true, DepthCompareOp = CompareOp.LessOrEqual };
            ColorComponentFlags mask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit;
            PipelineColorBlendAttachmentState att = new PipelineColorBlendAttachmentState
            { BlendEnable = true,
              SrcColorBlendFactor = BlendFactor.One, DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha, ColorBlendOp = BlendOp.Add,
              SrcAlphaBlendFactor = BlendFactor.One, DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha, AlphaBlendOp = BlendOp.Add,
              ColorWriteMask = mask };
            PipelineColorBlendStateCreateInfo cb = new PipelineColorBlendStateCreateInfo
            { SType = StructureType.PipelineColorBlendStateCreateInfo, AttachmentCount = 1, PAttachments = &att };
            var dynStates = stackalloc DynamicState[2] { DynamicState.Viewport, DynamicState.Scissor };
            PipelineDynamicStateCreateInfo dyn = new PipelineDynamicStateCreateInfo
            { SType = StructureType.PipelineDynamicStateCreateInfo, DynamicStateCount = 2, PDynamicStates = dynStates };
            GraphicsPipelineCreateInfo gp = new GraphicsPipelineCreateInfo
            { SType = StructureType.GraphicsPipelineCreateInfo, StageCount = 2, PStages = stages,
              PVertexInputState = &vi, PInputAssemblyState = &ia, PViewportState = &vp,
              PRasterizationState = &rs, PMultisampleState = &ms, PDepthStencilState = &ds,
              PColorBlendState = &cb, PDynamicState = &dyn,
              Layout = _pipelineLayout, RenderPass = rp, Subpass = 0 };
            VkPipeline pipeline;
            VkCheck.Ok(_vk.CreateGraphicsPipelines(_device, default, 1, in gp, null, &pipeline));
            return pipeline;
        }
    }
}
