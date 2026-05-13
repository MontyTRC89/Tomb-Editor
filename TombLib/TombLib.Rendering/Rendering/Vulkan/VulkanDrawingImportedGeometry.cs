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
    // Vulkan port of imported-geometry drawing (FBX/OBJ/COLLADA dropped meshes).
    // Differs from VulkanDrawingMesh in two ways: each submesh carries its own
    // texture (no shared atlas) and UVs come in pixel space (divided in the VS
    // by the per-submesh reciprocal texture size).
    public sealed class VulkanDrawingImportedGeometry : RenderingDrawingImportedGeometry
    {
        // 64 (World) + 16 (Tint) + 16 (ReciprocalTextureSize/flags) = 96 bytes
        private const uint MeshDataSize = 96;

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
layout(set = 1, binding = 0) uniform MeshData {
    mat4 World;
    vec4 Tint;
    vec4 RecipTexSize;   // .xy = 1/textureSize, .z = UseVertexColors, .w = AlphaTest
};
layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec3 inColor;
layout(location = 3) in vec3 inNormal;
layout(location = 0) out vec2 fsUV;
layout(location = 1) out vec4 fsColor;
layout(location = 2) out vec3 fsWorldPos;
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
        private const string FragmentShaderGlsl = @"#version 450
layout(set = 1, binding = 0) uniform MeshData {
    mat4 World;
    vec4 Tint;
    vec4 RecipTexSize;
};
layout(set = 2, binding = 0) uniform sampler2D TextureSampler;
layout(location = 0) in vec2 fsUV;
layout(location = 1) in vec4 fsColor;
layout(location = 2) in vec3 fsWorldPos;
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

        public readonly VulkanRenderingDevice DeviceWrapper;
        private readonly Vk _vk;
        private readonly Device _device;

        private ShaderModule _vs;
        private ShaderModule _fs;
        private DescriptorSetLayout _set0, _set1, _set2;
        private PipelineLayout _pipelineLayout;

        private VkBuffer _vertexBuffer, _indexBuffer;
        private DeviceMemory _vertexMemory, _indexMemory;
        private readonly Submesh[] _submeshes;
        private readonly byte[] _cbufferStaging = new byte[MeshDataSize];

        private VkBuffer _cachedStateBuffer;
        private DescriptorSet _frameSet;
        private DescriptorSet _meshSet;
        // One texture descriptor set per submesh, lazily updated when the
        // underlying ImageView changes. Keyed by submesh index because each
        // submesh has its own Texture.
        private readonly DescriptorSet[] _textureSets;
        private readonly ulong[] _textureSetCachedView;

        private readonly struct PipelineKey : IEquatable<PipelineKey>
        {
            public readonly bool DoubleSided;
            public readonly bool Additive;
            public readonly RenderPass RenderPass;
            public PipelineKey(bool d, bool a, RenderPass r) { DoubleSided = d; Additive = a; RenderPass = r; }
            public bool Equals(PipelineKey o) => DoubleSided == o.DoubleSided && Additive == o.Additive && RenderPass.Handle == o.RenderPass.Handle;
            public override bool Equals(object o) => o is PipelineKey k && Equals(k);
            public override int GetHashCode() => (DoubleSided ? 1 : 0) | (Additive ? 2 : 0) | RenderPass.Handle.GetHashCode();
        }
        private readonly Dictionary<PipelineKey, VkPipeline> _pipelineCache = new Dictionary<PipelineKey, VkPipeline>();

        public unsafe VulkanDrawingImportedGeometry(VulkanRenderingDevice device, Description description)
        {
            DeviceWrapper = device;
            _vk = device.Vk;
            _device = device.Device;

            _submeshes = new Submesh[description.Submeshes?.Count ?? 0];
            if (description.Submeshes != null)
                for (int i = 0; i < _submeshes.Length; i++) _submeshes[i] = description.Submeshes[i];

            int vCount = description.Vertices.Count;
            int iCount = description.Indices.Count;
            uint vBytes = (uint)(vCount * sizeof(Vertex));
            uint iBytes = (uint)(iCount * sizeof(int));

            CreateImmutableBufferFromList(description.Vertices, vBytes, BufferUsageFlags.VertexBufferBit, out _vertexBuffer, out _vertexMemory);
            CreateImmutableBufferFromList(description.Indices,  iBytes, BufferUsageFlags.IndexBufferBit,  out _indexBuffer,  out _indexMemory);

            byte[] vsSpirv = device.ShaderCompiler.CompileGlslToSpirv(VertexShaderGlsl,   ShaderKind.VertexShader,   "ImpGeoVS");
            byte[] fsSpirv = device.ShaderCompiler.CompileGlslToSpirv(FragmentShaderGlsl, ShaderKind.FragmentShader, "ImpGeoFS");
            _vs = VulkanShaderCompiler.CreateShaderModule(_vk, _device, vsSpirv);
            _fs = VulkanShaderCompiler.CreateShaderModule(_vk, _device, fsSpirv);

            _set0 = CreateSetLayoutSingle(DescriptorType.UniformBuffer,        ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit);
            _set1 = CreateSetLayoutSingle(DescriptorType.UniformBufferDynamic, ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit);
            _set2 = CreateSetLayoutSingle(DescriptorType.CombinedImageSampler, ShaderStageFlags.FragmentBit);

            var layouts = stackalloc DescriptorSetLayout[3] { _set0, _set1, _set2 };
            PipelineLayoutCreateInfo plci = new PipelineLayoutCreateInfo
            { SType = StructureType.PipelineLayoutCreateInfo, SetLayoutCount = 3, PSetLayouts = layouts };
            PipelineLayout pl;
            VkCheck.Ok(_vk.CreatePipelineLayout(_device, in plci, null, &pl));
            _pipelineLayout = pl;

            _frameSet = AllocateSet(_set0);
            _meshSet = AllocateSet(_set1);
            WriteUboDynamic(_meshSet, device.FrameUniforms.Buffer, MeshDataSize);

            _textureSets = new DescriptorSet[_submeshes.Length];
            _textureSetCachedView = new ulong[_submeshes.Length];
            for (int i = 0; i < _submeshes.Length; i++) _textureSets[i] = AllocateSet(_set2);
        }

        private unsafe void CreateImmutableBufferFromList<T>(IList<T> data, uint sizeBytes, BufferUsageFlags usage, out VkBuffer buffer, out DeviceMemory memory)
        {
            BufferCreateInfo stagingInfo = new BufferCreateInfo
            { SType = StructureType.BufferCreateInfo, Size = sizeBytes, Usage = BufferUsageFlags.TransferSrcBit, SharingMode = SharingMode.Exclusive };
            VkBuffer staging;
            VkCheck.Ok(_vk.CreateBuffer(_device, in stagingInfo, null, &staging));
            _vk.GetBufferMemoryRequirements(_device, staging, out MemoryRequirements stReq);
            MemoryAllocateInfo stAlloc = new MemoryAllocateInfo
            { SType = StructureType.MemoryAllocateInfo, AllocationSize = stReq.Size,
              MemoryTypeIndex = DeviceWrapper.FindMemoryType(stReq.MemoryTypeBits, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit) };
            DeviceMemory stMem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in stAlloc, null, &stMem));
            _vk.BindBufferMemory(_device, staging, stMem, 0);
            void* mapped;
            _vk.MapMemory(_device, stMem, 0, sizeBytes, 0, &mapped);
            GCHandle h = GCHandle.Alloc(data is T[] arr ? arr : ToArray(data), GCHandleType.Pinned);
            try { System.Buffer.MemoryCopy((void*)h.AddrOfPinnedObject(), mapped, sizeBytes, sizeBytes); }
            finally { h.Free(); }
            _vk.UnmapMemory(_device, stMem);

            BufferCreateInfo bufInfo = new BufferCreateInfo
            { SType = StructureType.BufferCreateInfo, Size = sizeBytes,
              Usage = usage | BufferUsageFlags.TransferDstBit, SharingMode = SharingMode.Exclusive };
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
            BufferCopy region = new BufferCopy { Size = sizeBytes };
            _vk.CmdCopyBuffer(cb, staging, buf, 1, in region);
            DeviceWrapper.EndAndSubmitTransient(cb);

            _vk.DestroyBuffer(_device, staging, null);
            _vk.FreeMemory(_device, stMem, null);

            buffer = buf;
            memory = mem;
        }

        private static T[] ToArray<T>(IList<T> list) { var a = new T[list.Count]; for (int i = 0; i < list.Count; i++) a[i] = list[i]; return a; }

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
        private unsafe void WriteUboDynamic(DescriptorSet set, VkBuffer buffer, ulong range)
        {
            DescriptorBufferInfo bi = new DescriptorBufferInfo { Buffer = buffer, Offset = 0, Range = range };
            WriteDescriptorSet w = new WriteDescriptorSet
            { SType = StructureType.WriteDescriptorSet, DstSet = set, DstBinding = 0,
              DescriptorCount = 1, DescriptorType = DescriptorType.UniformBufferDynamic, PBufferInfo = &bi };
            _vk.UpdateDescriptorSets(_device, 1, in w, 0, null);
        }
        private unsafe void WriteTexture(DescriptorSet set, ImageView view, Sampler sampler)
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
            foreach (var p in _pipelineCache.Values) if (p.Handle != 0) _vk.DestroyPipeline(_device, p, null);
            _pipelineCache.Clear();
            if (_pipelineLayout.Handle != 0) { _vk.DestroyPipelineLayout(_device, _pipelineLayout, null); _pipelineLayout = default; }
            if (_set0.Handle != 0) _vk.DestroyDescriptorSetLayout(_device, _set0, null);
            if (_set1.Handle != 0) _vk.DestroyDescriptorSetLayout(_device, _set1, null);
            if (_set2.Handle != 0) _vk.DestroyDescriptorSetLayout(_device, _set2, null);
            if (_vs.Handle != 0) _vk.DestroyShaderModule(_device, _vs, null);
            if (_fs.Handle != 0) _vk.DestroyShaderModule(_device, _fs, null);
            if (_vertexBuffer.Handle != 0) _vk.DestroyBuffer(_device, _vertexBuffer, null);
            if (_indexBuffer.Handle != 0)  _vk.DestroyBuffer(_device, _indexBuffer, null);
            if (_vertexMemory.Handle != 0) _vk.FreeMemory(_device, _vertexMemory, null);
            if (_indexMemory.Handle != 0)  _vk.FreeMemory(_device, _indexMemory, null);
        }

        public override unsafe void Render(RenderArgs arg)
        {
            if (_submeshes.Length == 0) return;
            if (arg.RenderTarget == null)
                throw new NotSupportedException("VulkanDrawingImportedGeometry.Render requires a non-null RenderTarget.");
            var swapChain = (VulkanSwapChain)arg.RenderTarget;
            var stateBuffer = (VulkanStateBuffer)arg.StateBuffer;
            var cb = swapChain.CurrentCommandBuffer;

            if (_cachedStateBuffer.Handle != stateBuffer.Buffer.Handle)
            {
                _cachedStateBuffer = stateBuffer.Buffer;
                WriteUbo(_frameSet, stateBuffer.Buffer, VulkanStateBuffer.Size);
            }

            VkBuffer vb = _vertexBuffer;
            ulong vOff = 0;
            _vk.CmdBindVertexBuffers(cb, 0, 1, &vb, &vOff);
            _vk.CmdBindIndexBuffer(cb, _indexBuffer, 0, IndexType.Uint32);

            Sampler sampler = arg.BilinearFilter ? DeviceWrapper.SamplerAniso : DeviceWrapper.SamplerPoint;

            for (int i = 0; i < _submeshes.Length; i++)
            {
                var sub = _submeshes[i];
                if (sub.IndexCount == 0) continue;

                // Per-submesh MeshData (different RecipTexSize, AlphaTest).
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
                uint meshOff = DeviceWrapper.FrameUniforms.Upload(_cbufferStaging, MeshDataSize);

                ImageView texView = ResolveTexture(sub.Texture);
                if (texView.Handle == 0) continue;
                if (_textureSetCachedView[i] != texView.Handle)
                {
                    _textureSetCachedView[i] = texView.Handle;
                    WriteTexture(_textureSets[i], texView, sampler);
                }

                bool additive = sub.AdditiveBlending || arg.ForceAdditive;
                var key = new PipelineKey(sub.DoubleSided, additive, swapChain.RenderPass);
                if (!_pipelineCache.TryGetValue(key, out VkPipeline pipeline))
                {
                    pipeline = BuildPipeline(key);
                    _pipelineCache[key] = pipeline;
                }
                _vk.CmdBindPipeline(cb, PipelineBindPoint.Graphics, pipeline);

                var sets = stackalloc DescriptorSet[3] { _frameSet, _meshSet, _textureSets[i] };
                uint dynOff = meshOff;
                _vk.CmdBindDescriptorSets(cb, PipelineBindPoint.Graphics, _pipelineLayout, 0, 3, sets, 1, in dynOff);
                _vk.CmdDrawIndexed(cb, (uint)sub.IndexCount, 1, (uint)sub.IndexStart, 0, 0);
            }
        }

        private static ImageView ResolveTexture(object tex)
        {
            if (tex == null) return default;
            if (tex is ImageView iv) return iv;
            if (tex is VulkanTexture2D t) return t.View;
            return default;
        }

        private unsafe VkPipeline BuildPipeline(PipelineKey key)
        {
            var entryName = stackalloc byte[5] { (byte)'m', (byte)'a', (byte)'i', (byte)'n', 0 };
            var stages = stackalloc PipelineShaderStageCreateInfo[2]
            {
                new PipelineShaderStageCreateInfo { SType = StructureType.PipelineShaderStageCreateInfo,
                    Stage = ShaderStageFlags.VertexBit,   Module = _vs, PName = entryName },
                new PipelineShaderStageCreateInfo { SType = StructureType.PipelineShaderStageCreateInfo,
                    Stage = ShaderStageFlags.FragmentBit, Module = _fs, PName = entryName },
            };
            // Vertex: Position@0 (12), UV@12 (8), Color@20 (12), Normal@32 (12). Stride 44.
            var binding = new VertexInputBindingDescription { Binding = 0, Stride = 44, InputRate = VertexInputRate.Vertex };
            var attributes = stackalloc VertexInputAttributeDescription[4]
            {
                new VertexInputAttributeDescription { Location = 0, Binding = 0, Format = Format.R32G32B32Sfloat, Offset = 0 },
                new VertexInputAttributeDescription { Location = 1, Binding = 0, Format = Format.R32G32Sfloat,    Offset = 12 },
                new VertexInputAttributeDescription { Location = 2, Binding = 0, Format = Format.R32G32B32Sfloat, Offset = 20 },
                new VertexInputAttributeDescription { Location = 3, Binding = 0, Format = Format.R32G32B32Sfloat, Offset = 32 },
            };
            PipelineVertexInputStateCreateInfo vi = new PipelineVertexInputStateCreateInfo
            { SType = StructureType.PipelineVertexInputStateCreateInfo, VertexBindingDescriptionCount = 1, PVertexBindingDescriptions = &binding,
              VertexAttributeDescriptionCount = 4, PVertexAttributeDescriptions = attributes };
            PipelineInputAssemblyStateCreateInfo ia = new PipelineInputAssemblyStateCreateInfo
            { SType = StructureType.PipelineInputAssemblyStateCreateInfo, Topology = PrimitiveTopology.TriangleList };
            PipelineViewportStateCreateInfo vp = new PipelineViewportStateCreateInfo
            { SType = StructureType.PipelineViewportStateCreateInfo, ViewportCount = 1, ScissorCount = 1 };
            PipelineRasterizationStateCreateInfo rs = new PipelineRasterizationStateCreateInfo
            { SType = StructureType.PipelineRasterizationStateCreateInfo, PolygonMode = PolygonMode.Fill,
              CullMode = key.DoubleSided ? CullModeFlags.None : CullModeFlags.BackBit,
              FrontFace = FrontFace.Clockwise,   // matches D3D11 default (CW=front in fb space) given the Y-flipped Vulkan viewport
              LineWidth = 1.0f };
            PipelineMultisampleStateCreateInfo ms = new PipelineMultisampleStateCreateInfo
            { SType = StructureType.PipelineMultisampleStateCreateInfo, RasterizationSamples = SampleCountFlags.Count1Bit };
            PipelineDepthStencilStateCreateInfo ds = new PipelineDepthStencilStateCreateInfo
            { SType = StructureType.PipelineDepthStencilStateCreateInfo, DepthTestEnable = true, DepthWriteEnable = true, DepthCompareOp = CompareOp.LessOrEqual };
            ColorComponentFlags mask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit;
            PipelineColorBlendAttachmentState att = key.Additive
                ? new PipelineColorBlendAttachmentState { BlendEnable = true,
                    SrcColorBlendFactor = BlendFactor.One, DstColorBlendFactor = BlendFactor.One, ColorBlendOp = BlendOp.Add,
                    SrcAlphaBlendFactor = BlendFactor.One, DstAlphaBlendFactor = BlendFactor.One, AlphaBlendOp = BlendOp.Add, ColorWriteMask = mask }
                : new PipelineColorBlendAttachmentState { BlendEnable = true,
                    SrcColorBlendFactor = BlendFactor.One, DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha, ColorBlendOp = BlendOp.Add,
                    SrcAlphaBlendFactor = BlendFactor.One, DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha, AlphaBlendOp = BlendOp.Add, ColorWriteMask = mask };
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
              Layout = _pipelineLayout, RenderPass = key.RenderPass, Subpass = 0 };
            VkPipeline pipeline;
            VkCheck.Ok(_vk.CreateGraphicsPipelines(_device, default, 1, in gp, null, &pipeline));
            return pipeline;
        }
    }
}
