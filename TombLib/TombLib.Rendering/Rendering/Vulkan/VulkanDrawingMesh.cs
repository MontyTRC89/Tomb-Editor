using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Shaderc;
using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;
using VkPipeline = Silk.NET.Vulkan.Pipeline;

namespace TombLib.Rendering.Vulkan
{
    // Vulkan port of the moveable/static/imported-geometry mesh drawing.
    // 32-bone GPU skinning, atlas Texture2DArray sampling, brush overlay,
    // per-submesh (DoubleSided / AdditiveBlending) pipeline selection.
    //
    // Per-render hot path:
    //   1) push the per-batch MeshData (World/Tint/Bones/flags) into the
    //      device-wide FrameUniforms ring (returns a dynamic offset)
    //   2) bind FrameData (set 0) + MeshData (set 1, dynamic offset) +
    //      Atlas+Sampler (set 2) descriptor sets
    //   3) for each submesh: pick pipeline (cached) + draw
    public sealed class VulkanDrawingMesh : RenderingDrawingMesh
    {
        private const int MaxBones = 32;
        private const uint MeshDataSize = 64 + 16 + (uint)MaxBones * 64 + 16;

        private const string VertexShaderGlsl = @"#version 450
#define MAX_BONES 32
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
layout(set = 1, binding = 0) uniform MeshData {
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
layout(location = 0) out vec3 fsUVW;
layout(location = 1) out vec4 fsColor;
layout(location = 2) out vec3 fsWorldPosition;
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
layout(set = 1, binding = 0) uniform MeshData {
    mat4 World;
    vec4 Tint;
    mat4 Bones[32];
    int  Skinned;
    int  StaticLighting;
    int  ColoredVertices;
    int  AlphaTest;
};
layout(set = 2, binding = 0) uniform sampler2DArray AtlasSampler;
layout(location = 0) in vec3 fsUVW;
layout(location = 1) in vec4 fsColor;
layout(location = 2) in vec3 fsWorldPosition;
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

        public readonly VulkanRenderingDevice DeviceWrapper;
        private readonly Vk _vk;
        private readonly Device _device;

        private ShaderModule _vs;
        private ShaderModule _fs;
        private DescriptorSetLayout _set0, _set1, _set2;
        private PipelineLayout _pipelineLayout;

        private VkBuffer _vertexBuffer;
        private VkBuffer _indexBuffer;
        private DeviceMemory _vertexMemory, _indexMemory;
        private readonly Submesh[] _submeshes;

        // CPU-side staging area for the per-draw MeshData constant buffer.
        // Layout (byte offsets, matching the GLSL `uniform MeshData` block):
        //   [  0 ..  63]  World          mat4   (64 bytes)
        //   [ 64 ..  79]  Tint           vec4   (16 bytes)
        //   [ 80 .. 2127] Bones[32]      mat4[] (32 * 64 = 2048 bytes)
        //   [2128 .. 2143] Skinned/StaticLighting/ColoredVertices/AlphaTest
        //                                ivec4  (4 * 4 = 16 bytes)
        // Total: 2144 bytes  (= MeshDataSize)
        private readonly byte[] _cbufferStaging = new byte[MeshDataSize];

        private VkBuffer _cachedStateBuffer;
        private DescriptorSet _frameSet;
        private DescriptorSet _meshSet;
        private ImageView _cachedAtlas;
        private bool _cachedBilinear;
        private DescriptorSet _atlasSet;

        // Pipelines are cached by (DoubleSided, Additive, RenderPass, Samples)
        // because those are the only axes that change the Vulkan pipeline state
        // object between submeshes. Creating a VkPipeline is expensive, so we
        // build on first use and reuse thereafter. The RenderPass + SampleCount
        // can change when the swap chain is resized or MSAA settings change.
        private readonly struct PipelineKey : IEquatable<PipelineKey>
        {
            public readonly bool DoubleSided;
            public readonly bool Additive;
            public readonly RenderPass RenderPass;
            public readonly SampleCountFlags Samples;
            public PipelineKey(bool ds, bool ad, RenderPass rp, SampleCountFlags s) { DoubleSided = ds; Additive = ad; RenderPass = rp; Samples = s; }
            public bool Equals(PipelineKey o) => DoubleSided == o.DoubleSided && Additive == o.Additive && RenderPass.Handle == o.RenderPass.Handle && Samples == o.Samples;
            public override bool Equals(object obj) => obj is PipelineKey k && Equals(k);
            public override int GetHashCode() => (DoubleSided ? 1 : 0) | (Additive ? 2 : 0) | RenderPass.Handle.GetHashCode() | ((int)Samples << 16);
        }
        private readonly Dictionary<PipelineKey, VkPipeline> _pipelineCache = new Dictionary<PipelineKey, VkPipeline>();

        public unsafe VulkanDrawingMesh(VulkanRenderingDevice device, Description description)
        {
            DeviceWrapper = device;
            _vk = device.Vk;
            _device = device.Device;

            _submeshes = new Submesh[description.Submeshes?.Count ?? 0];
            if (description.Submeshes != null)
                for (int i = 0; i < _submeshes.Length; i++) _submeshes[i] = description.Submeshes[i];

            int vertexCount = description.Vertices.Count;
            int indexCount = description.Indices.Count;
            uint vBytes = (uint)(vertexCount * sizeof(MeshVertex));
            uint iBytes = (uint)(indexCount * sizeof(int));

            // Upload vertices + indices to device-local memory via staging.
            CreateImmutableBufferFromArray(description.Vertices, vBytes, BufferUsageFlags.VertexBufferBit, out _vertexBuffer, out _vertexMemory);
            CreateImmutableBufferFromArray(description.Indices,  iBytes, BufferUsageFlags.IndexBufferBit,  out _indexBuffer,  out _indexMemory);

            byte[] vsSpirv = device.ShaderCompiler.CompileGlslToSpirv(VertexShaderGlsl,   ShaderKind.VertexShader,   "MeshVS");
            byte[] fsSpirv = device.ShaderCompiler.CompileGlslToSpirv(FragmentShaderGlsl, ShaderKind.FragmentShader, "MeshFS");
            _vs = VulkanShaderCompiler.CreateShaderModule(_vk, _device, vsSpirv);
            _fs = VulkanShaderCompiler.CreateShaderModule(_vk, _device, fsSpirv);

            // Descriptor set layouts — one binding per set:
            //   set 0: FrameData UBO (frame-wide camera/editor state)
            //   set 1: MeshData UBO with dynamic offset (per-draw transform/bones)
            //   set 2: Atlas texture + sampler (shared Texture2DArray)
            _set0 = CreateSetLayoutSingle(DescriptorType.UniformBuffer,        ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit);
            _set1 = CreateSetLayoutSingle(DescriptorType.UniformBufferDynamic, ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit);
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
            _meshSet  = AllocateSet(_set1);
            _atlasSet = AllocateSet(_set2);
            WriteUboDynamic(_meshSet, device.FrameUniforms.Buffer, MeshDataSize);
        }

        // Staging buffer upload pattern:
        //   1. Create a host-visible (CPU-writable) staging buffer
        //   2. Map it, memcpy the managed array in, unmap
        //   3. Create a device-local (GPU-fast) buffer of the same size
        //   4. Record a vkCmdCopyBuffer inside a transient command buffer
        //   5. Submit and wait, then destroy the staging buffer
        // This is the standard Vulkan approach for immutable geometry: device-local
        // memory is fastest for the GPU but not directly writable by the CPU, so
        // we bounce through a temporary host-visible buffer.
        private unsafe void CreateImmutableBufferFromArray<T>(IList<T> data, uint sizeBytes, BufferUsageFlags usage, out VkBuffer buffer, out DeviceMemory memory)
        {
            // Staging buffer (host visible)
            BufferCreateInfo stagingInfo = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = sizeBytes,
                Usage = BufferUsageFlags.TransferSrcBit,
                SharingMode = SharingMode.Exclusive,
            };
            VkBuffer staging;
            VkCheck.Ok(_vk.CreateBuffer(_device, in stagingInfo, null, &staging));
            _vk.GetBufferMemoryRequirements(_device, staging, out MemoryRequirements stReq);
            MemoryAllocateInfo stAlloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = stReq.Size,
                MemoryTypeIndex = DeviceWrapper.FindMemoryType(stReq.MemoryTypeBits,
                    MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit),
            };
            DeviceMemory stMem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in stAlloc, null, &stMem));
            _vk.BindBufferMemory(_device, staging, stMem, 0);

            void* mapped;
            _vk.MapMemory(_device, stMem, 0, sizeBytes, 0, &mapped);
            // GCHandle pin the managed array; memcpy into the mapped device pointer.
            GCHandle handle = GCHandle.Alloc(data is T[] arr ? arr : ToArray(data), GCHandleType.Pinned);
            try
            {
                System.Buffer.MemoryCopy((void*)handle.AddrOfPinnedObject(), mapped, sizeBytes, sizeBytes);
            }
            finally { handle.Free(); }
            _vk.UnmapMemory(_device, stMem);

            // Device-local buffer
            BufferCreateInfo bufInfo = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = sizeBytes,
                Usage = usage | BufferUsageFlags.TransferDstBit,
                SharingMode = SharingMode.Exclusive,
            };
            VkBuffer buf;
            VkCheck.Ok(_vk.CreateBuffer(_device, in bufInfo, null, &buf));
            _vk.GetBufferMemoryRequirements(_device, buf, out MemoryRequirements req);
            MemoryAllocateInfo allocInfo = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size,
                MemoryTypeIndex = DeviceWrapper.FindMemoryType(req.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
            };
            DeviceMemory mem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in allocInfo, null, &mem));
            _vk.BindBufferMemory(_device, buf, mem, 0);

            // Copy staging → device-local via a transient one-shot command buffer.
            var cb = DeviceWrapper.BeginTransient();
            BufferCopy region = new BufferCopy { Size = sizeBytes };
            _vk.CmdCopyBuffer(cb, staging, buf, 1, in region);
            DeviceWrapper.EndAndSubmitTransient(cb);

            _vk.DestroyBuffer(_device, staging, null);
            _vk.FreeMemory(_device, stMem, null);

            buffer = buf;
            memory = mem;
        }

        private static T[] ToArray<T>(IList<T> list) { var a = new T[list.Count]; for (int i = 0; i < list.Count; i++) a[i] = list[i]; return a; }

        // Creates a descriptor set layout with a single binding at index 0.
        // This is the simplest possible layout: one UBO or one sampler per set.
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
            WriteDescriptorSet write = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = set,
                DstBinding = 0,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBuffer,
                PBufferInfo = &bi,
            };
            _vk.UpdateDescriptorSets(_device, 1, in write, 0, null);
        }

        // Updates a descriptor set to point at a dynamic UBO. The actual offset
        // into the buffer is supplied at bind time via vkCmdBindDescriptorSets'
        // pDynamicOffsets parameter — this allows reusing one descriptor set for
        // many per-draw uploads into the FrameUniforms ring buffer.
        private unsafe void WriteUboDynamic(DescriptorSet set, VkBuffer buffer, ulong range)
        {
            DescriptorBufferInfo bi = new DescriptorBufferInfo
            {
                Buffer = buffer,
                Offset = 0,
                Range = range,
            };
            WriteDescriptorSet write = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = set,
                DstBinding = 0,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBufferDynamic,
                PBufferInfo = &bi,
            };
            _vk.UpdateDescriptorSets(_device, 1, in write, 0, null);
        }

        // Updates a descriptor set to point at a combined image sampler (atlas).
        private unsafe void WriteAtlas(DescriptorSet set, ImageView view, Sampler sampler)
        {
            DescriptorImageInfo ii = new DescriptorImageInfo
            {
                Sampler = sampler,
                ImageView = view,
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
            };
            WriteDescriptorSet write = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = set,
                DstBinding = 0,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.CombinedImageSampler,
                PImageInfo = &ii,
            };
            _vk.UpdateDescriptorSets(_device, 1, in write, 0, null);
        }

        public override unsafe void Dispose()
        {
            _vk.DeviceWaitIdle(_device);
            foreach (var p in _pipelineCache.Values) if (p.Handle != 0) _vk.DestroyPipeline(_device, p, null);
            _pipelineCache.Clear();
            if (_pipelineLayout.Handle != 0) { _vk.DestroyPipelineLayout(_device, _pipelineLayout, null); _pipelineLayout = default; }
            if (_set0.Handle != 0) { _vk.DestroyDescriptorSetLayout(_device, _set0, null); _set0 = default; }
            if (_set1.Handle != 0) { _vk.DestroyDescriptorSetLayout(_device, _set1, null); _set1 = default; }
            if (_set2.Handle != 0) { _vk.DestroyDescriptorSetLayout(_device, _set2, null); _set2 = default; }
            if (_vs.Handle != 0) { _vk.DestroyShaderModule(_device, _vs, null); _vs = default; }
            if (_fs.Handle != 0) { _vk.DestroyShaderModule(_device, _fs, null); _fs = default; }
            if (_vertexBuffer.Handle != 0) { _vk.DestroyBuffer(_device, _vertexBuffer, null); _vertexBuffer = default; }
            if (_indexBuffer.Handle != 0) { _vk.DestroyBuffer(_device, _indexBuffer, null); _indexBuffer = default; }
            if (_vertexMemory.Handle != 0) { _vk.FreeMemory(_device, _vertexMemory, null); _vertexMemory = default; }
            if (_indexMemory.Handle != 0) { _vk.FreeMemory(_device, _indexMemory, null); _indexMemory = default; }
        }

        public override unsafe void Render(RenderArgs arg)
        {
            if (_submeshes.Length == 0) return;
            if (arg.RenderTarget == null)
                throw new NotSupportedException("VulkanDrawingMesh.Render requires a non-null RenderTarget.");
            var swapChain = (VulkanSwapChain)arg.RenderTarget;
            var stateBuffer = (VulkanStateBuffer)arg.StateBuffer;
            var atlas = ResolveAtlas(arg.Atlas);
            if (atlas.Handle == 0) return;
            var cb = swapChain.CurrentCommandBuffer;

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
            uint meshDataOffset = DeviceWrapper.FrameUniforms.Upload(_cbufferStaging, MeshDataSize);

            // Re-write descriptors when their underlying resources change.
            if (_cachedStateBuffer.Handle != stateBuffer.Buffer.Handle)
            {
                _cachedStateBuffer = stateBuffer.Buffer;
                WriteUbo(_frameSet, stateBuffer.Buffer, VulkanStateBuffer.Size);
            }
            if (_cachedAtlas.Handle != atlas.Handle || _cachedBilinear != arg.BilinearFilter)
            {
                _cachedAtlas = atlas;
                _cachedBilinear = arg.BilinearFilter;
                WriteAtlas(_atlasSet, atlas, arg.BilinearFilter ? DeviceWrapper.SamplerAniso : DeviceWrapper.SamplerPoint);
            }

            VkBuffer vb = _vertexBuffer;
            ulong vOff = 0;
            _vk.CmdBindVertexBuffers(cb, 0, 1, &vb, &vOff);
            _vk.CmdBindIndexBuffer(cb, _indexBuffer, 0, IndexType.Uint32);

            // Descriptor set binding model:
            //   set 0 = FrameData UBO (camera matrix + editor uniforms, shared across all draws)
            //   set 1 = MeshData dynamic UBO (per-draw World/Tint/Bones via ring offset)
            //   set 2 = Atlas combined image sampler (shared Texture2DArray + sampler)
            // A single dynamic offset (meshDataOffset) is passed so that set 1
            // reads the correct slice of the FrameUniforms ring buffer for this draw.
            var sets = stackalloc DescriptorSet[3] { _frameSet, _meshSet, _atlasSet };
            uint dynOff = meshDataOffset;

            foreach (var sub in _submeshes)
            {
                if (sub.IndexCount == 0) continue;
                var key = new PipelineKey(sub.DoubleSided, sub.AdditiveBlending, swapChain.RenderPass, swapChain.SampleCount);
                if (!_pipelineCache.TryGetValue(key, out VkPipeline pipeline))
                {
                    pipeline = BuildPipeline(key);
                    _pipelineCache[key] = pipeline;
                }
                _vk.CmdBindPipeline(cb, PipelineBindPoint.Graphics, pipeline);
                _vk.CmdBindDescriptorSets(cb, PipelineBindPoint.Graphics, _pipelineLayout, 0, 3, sets, 1, in dynOff);
                _vk.CmdDrawIndexed(cb, (uint)sub.IndexCount, 1, (uint)sub.IndexStart, 0, 0);
            }
        }

        private static ImageView ResolveAtlas(object atlas)
        {
            if (atlas == null) return default;
            if (atlas is ImageView iv) return iv;
            if (atlas is VulkanTextureAllocator a) return a.AtlasView;
            return default;
        }

        private unsafe VkPipeline BuildPipeline(PipelineKey key)
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
            // MeshVertex: Position@0, UVW@12, Normal@24, Color@36, BoneIdx@48, BoneW@64. Stride 80.
            var binding = new VertexInputBindingDescription { Binding = 0, Stride = 80, InputRate = VertexInputRate.Vertex };
            var attributes = stackalloc VertexInputAttributeDescription[6]
            {
                new VertexInputAttributeDescription { Location = 0, Binding = 0, Format = Format.R32G32B32Sfloat,    Offset = 0  },
                new VertexInputAttributeDescription { Location = 1, Binding = 0, Format = Format.R32G32B32Sfloat,    Offset = 12 },
                new VertexInputAttributeDescription { Location = 2, Binding = 0, Format = Format.R32G32B32Sfloat,    Offset = 24 },
                new VertexInputAttributeDescription { Location = 3, Binding = 0, Format = Format.R32G32B32Sfloat,    Offset = 36 },
                new VertexInputAttributeDescription { Location = 4, Binding = 0, Format = Format.R32G32B32A32Sfloat, Offset = 48 },
                new VertexInputAttributeDescription { Location = 5, Binding = 0, Format = Format.R32G32B32A32Sfloat, Offset = 64 },
            };
            PipelineVertexInputStateCreateInfo viState = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 1,
                PVertexBindingDescriptions = &binding,
                VertexAttributeDescriptionCount = 6,
                PVertexAttributeDescriptions = attributes,
            };

            // ---- Input assembly -----------------------------------------------
            // All mesh geometry is submitted as indexed triangle lists.
            PipelineInputAssemblyStateCreateInfo iaState = new PipelineInputAssemblyStateCreateInfo
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = PrimitiveTopology.TriangleList,
            };

            // ---- Viewport / scissor ------------------------------------------
            // Both are dynamic state (set per-frame by VulkanSwapChain), so we
            // just declare count = 1 here without providing actual values.
            PipelineViewportStateCreateInfo vpState = new PipelineViewportStateCreateInfo
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                ScissorCount = 1,
            };

            // ---- Rasterization ------------------------------------------------
            // Fill mode, with back-face culling toggled per submesh (DoubleSided
            // disables culling). FrontFace.Clockwise matches D3D11 default under
            // the Y-flipped Vulkan viewport.
            PipelineRasterizationStateCreateInfo rs = new PipelineRasterizationStateCreateInfo
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                PolygonMode = PolygonMode.Fill,
                CullMode = key.DoubleSided ? CullModeFlags.None : CullModeFlags.BackBit,
                FrontFace = FrontFace.Clockwise,
                LineWidth = 1.0f,
            };

            // ---- Multisampling ------------------------------------------------
            // Sample count is inherited from the swap chain's current MSAA mode.
            PipelineMultisampleStateCreateInfo ms = new PipelineMultisampleStateCreateInfo
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = key.Samples,
            };

            // ---- Depth / stencil ----------------------------------------------
            // Standard opaque depth testing: write + test with LessOrEqual.
            PipelineDepthStencilStateCreateInfo ds = new PipelineDepthStencilStateCreateInfo
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = true,
                DepthWriteEnable = true,
                DepthCompareOp = CompareOp.LessOrEqual,
            };

            // ---- Color blending -----------------------------------------------
            // Two modes: additive (Src=One, Dst=One) for glow/laser effects,
            // or premultiplied alpha (Src=One, Dst=1-SrcAlpha) for normal meshes.
            ColorComponentFlags mask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit;
            PipelineColorBlendAttachmentState att = key.Additive
                ? new PipelineColorBlendAttachmentState
                {
                    BlendEnable = true,
                    SrcColorBlendFactor = BlendFactor.One,
                    DstColorBlendFactor = BlendFactor.One,
                    ColorBlendOp = BlendOp.Add,
                    SrcAlphaBlendFactor = BlendFactor.One,
                    DstAlphaBlendFactor = BlendFactor.One,
                    AlphaBlendOp = BlendOp.Add,
                    ColorWriteMask = mask,
                }
                : new PipelineColorBlendAttachmentState
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
            // Viewport and scissor are set every frame so they don't bake into the
            // pipeline. This avoids recreating pipelines on window resize.
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
                PVertexInputState = &viState,
                PInputAssemblyState = &iaState,
                PViewportState = &vpState,
                PRasterizationState = &rs,
                PMultisampleState = &ms,
                PDepthStencilState = &ds,
                PColorBlendState = &cb,
                PDynamicState = &dyn,
                Layout = _pipelineLayout,
                RenderPass = key.RenderPass,
                Subpass = 0,
            };
            VkPipeline pipeline;
            VkCheck.Ok(_vk.CreateGraphicsPipelines(_device, default, 1, in gp, null, &pipeline));
            return pipeline;
        }
    }
}
