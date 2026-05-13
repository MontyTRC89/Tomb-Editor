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
    // Vulkan port of the lines/triangles dynamic batch. The hot-path per-frame
    // pattern:
    //   1) Caller stashes vertices via SetVertices() (CPU copy).
    //   2) Caller calls Render() — we map+memcpy the vertex bytes into our
    //      persistent vertex buffer, push the per-batch LineData (World+Tint)
    //      into the device-wide FrameUniforms ring (no allocation), pick the
    //      pipeline that matches (Blend, Depth, Topology, Wireframe), bind two
    //      descriptor sets (one for FrameData = StateBuffer, one for LineData
    //      = ring with dynamic offset), bind the vertex buffer, vkCmdDraw.
    public sealed class VulkanDrawingLines : RenderingDrawingLines
    {
        private const string VertexShaderGlsl = @"#version 450
layout(set = 0, binding = 0) uniform FrameData {
    mat4 TransformMatrix;
};
layout(set = 1, binding = 0) uniform LineData {
    mat4 World;
    vec4 Tint;
};
layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec4 inColor;
layout(location = 0) out vec4 fsColor;
void main() {
    vec4 worldPos = World * vec4(inPosition, 1.0);
    gl_Position = TransformMatrix * worldPos;
    fsColor = inColor * Tint;
}
";
        private const string FragmentShaderGlsl = @"#version 450
layout(location = 0) in vec4 fsColor;
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

        public readonly VulkanRenderingDevice DeviceWrapper;
        private readonly Vk _vk;
        private readonly Device _device;

        private ShaderModule _vsModule;
        private ShaderModule _fsModule;
        private DescriptorSetLayout _set0Layout;     // FrameData (StateBuffer)
        private DescriptorSetLayout _set1Layout;     // LineData (dynamic UBO ring)
        private PipelineLayout _pipelineLayout;

        // One DescriptorSet for set 0 bound to the active StateBuffer's buffer
        // (cached by StateBuffer identity), one for set 1 bound to the device
        // FrameUniforms ring (single, reused with dynamic offsets).
        private VkBuffer _cachedStateBuffer;
        private DescriptorSet _frameDataSet;
        private DescriptorSet _lineDataSet;

        // Vertex buffer (host visible coherent, persistently mapped). Grown on
        // demand. Writing is a memcpy into the mapped pointer; no staging.
        private VkBuffer _vertexBuffer;
        private DeviceMemory _vertexMemory;
        private unsafe void* _vertexMapped;
        private uint _vertexBufferCapacity;

        private SolidLineVertex[] _vertices = Array.Empty<SolidLineVertex>();
        private int _vertexCount;
        private byte[] _staging = Array.Empty<byte>();

        private readonly struct PipelineKey : IEquatable<PipelineKey>
        {
            public readonly BlendMode Blend;
            public readonly DepthMode Depth;
            public readonly Topology Topology;
            public readonly bool Wireframe;
            public readonly RenderPass RenderPass;
            public readonly SampleCountFlags Samples;
            public PipelineKey(BlendMode b, DepthMode d, Topology t, bool w, RenderPass rp, SampleCountFlags s)
            { Blend = b; Depth = d; Topology = t; Wireframe = w; RenderPass = rp; Samples = s; }
            public bool Equals(PipelineKey o) => Blend == o.Blend && Depth == o.Depth
                && Topology == o.Topology && Wireframe == o.Wireframe && RenderPass.Handle == o.RenderPass.Handle
                && Samples == o.Samples;
            public override bool Equals(object obj) => obj is PipelineKey k && Equals(k);
            public override int GetHashCode() => ((int)Blend * 73 + (int)Depth) * 71 + (int)Topology
                + (Wireframe ? 1024 : 0) + RenderPass.Handle.GetHashCode() + (int)Samples * 7919;
        }
        private readonly Dictionary<PipelineKey, VkPipeline> _pipelineCache = new Dictionary<PipelineKey, VkPipeline>();

        public unsafe VulkanDrawingLines(VulkanRenderingDevice device, Description description)
        {
            DeviceWrapper = device;
            _vk = device.Vk;
            _device = device.Device;

            byte[] vsSpirv = device.ShaderCompiler.CompileGlslToSpirv(VertexShaderGlsl,   ShaderKind.VertexShader,   "LinesVS");
            byte[] fsSpirv = device.ShaderCompiler.CompileGlslToSpirv(FragmentShaderGlsl, ShaderKind.FragmentShader, "LinesFS");
            _vsModule = VulkanShaderCompiler.CreateShaderModule(_vk, _device, vsSpirv);
            _fsModule = VulkanShaderCompiler.CreateShaderModule(_vk, _device, fsSpirv);

            // set 0: standard UBO (StateBuffer is bound by identity, not by offset).
            // set 1: DYNAMIC UBO (FrameUniforms ring buffer; per-draw offset).
            _set0Layout = CreateSetLayout(DescriptorType.UniformBuffer);
            _set1Layout = CreateSetLayout(DescriptorType.UniformBufferDynamic);

            var layouts = stackalloc DescriptorSetLayout[2] { _set0Layout, _set1Layout };
            PipelineLayoutCreateInfo plInfo = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 2,
                PSetLayouts = layouts,
            };
            PipelineLayout pl;
            VkCheck.Ok(_vk.CreatePipelineLayout(_device, in plInfo, null, &pl));
            _pipelineLayout = pl;

            // Allocate the per-class descriptor sets up front. set 0 isn't bound
            // to anything yet — that happens on first Render when we see the
            // StateBuffer. set 1 is bound to FrameUniforms.Buffer immediately.
            _frameDataSet = AllocateDescriptorSet(_set0Layout);
            _lineDataSet  = AllocateDescriptorSet(_set1Layout);
            WriteDescriptorSetUboDynamic(_lineDataSet, device.FrameUniforms.Buffer, (ulong)sizeof(LineDataLayout));
        }

        private unsafe DescriptorSetLayout CreateSetLayout(DescriptorType type)
        {
            DescriptorSetLayoutBinding b = new DescriptorSetLayoutBinding
            {
                Binding = 0,
                DescriptorCount = 1,
                DescriptorType = type,
                StageFlags = ShaderStageFlags.VertexBit,
            };
            DescriptorSetLayoutCreateInfo info = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 1,
                PBindings = &b,
            };
            DescriptorSetLayout layout;
            VkCheck.Ok(_vk.CreateDescriptorSetLayout(_device, in info, null, &layout));
            return layout;
        }

        private unsafe DescriptorSet AllocateDescriptorSet(DescriptorSetLayout layout)
        {
            DescriptorSetAllocateInfo info = new DescriptorSetAllocateInfo
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = DeviceWrapper.DescriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = &layout,
            };
            DescriptorSet set;
            VkCheck.Ok(_vk.AllocateDescriptorSets(_device, in info, &set));
            return set;
        }

        private unsafe void WriteDescriptorSetUbo(DescriptorSet set, VkBuffer buffer, ulong range)
        {
            DescriptorBufferInfo bi = new DescriptorBufferInfo { Buffer = buffer, Offset = 0, Range = range };
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

        private unsafe void WriteDescriptorSetUboDynamic(DescriptorSet set, VkBuffer buffer, ulong range)
        {
            DescriptorBufferInfo bi = new DescriptorBufferInfo { Buffer = buffer, Offset = 0, Range = range };
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
            var swapChain = (VulkanSwapChain)arg.RenderTarget;
            var stateBuffer = (VulkanStateBuffer)arg.StateBuffer;
            var cb = swapChain.CurrentCommandBuffer;

            // Frame-data descriptor — point set 0 at the StateBuffer's buffer
            // if it has changed since last frame.
            if (_cachedStateBuffer.Handle != stateBuffer.Buffer.Handle)
            {
                _cachedStateBuffer = stateBuffer.Buffer;
                WriteDescriptorSetUbo(_frameDataSet, stateBuffer.Buffer, VulkanStateBuffer.Size);
            }

            // Push the per-batch LineData into the frame UBO ring. The returned
            // offset is the descriptor's dynamic offset.
            LineDataLayout cbData;
            cbData.World = arg.World;
            cbData.Tint = arg.Tint;
            uint lineDataOffset = DeviceWrapper.FrameUniforms.Upload(ref cbData);

            // Upload vertices. SoA in one buffer: positions[] then colors[].
            uint posBytes = (uint)(_vertexCount * sizeof(Vector3));
            uint colBytes = (uint)(_vertexCount * sizeof(Vector4));
            uint totalBytes = posBytes + colBytes;
            EnsureVertexBufferCapacity(totalBytes);

            if (_staging.Length < totalBytes) _staging = new byte[Math.Max(totalBytes, (uint)_staging.Length * 2)];
            fixed (byte* dst = _staging)
            fixed (SolidLineVertex* src = _vertices)
            {
                Vector3* posDst = (Vector3*)dst;
                Vector4* colDst = (Vector4*)(dst + posBytes);
                for (int i = 0; i < _vertexCount; ++i)
                {
                    posDst[i] = src[i].Position;
                    colDst[i] = src[i].Color;
                }
            }
            // memcpy into the persistently-mapped vertex buffer (host coherent).
            fixed (byte* sp = _staging)
                System.Buffer.MemoryCopy(sp, _vertexMapped, totalBytes, totalBytes);

            // Pipeline for (blend, depth, topology, wireframe, render-pass, samples).
            var key = new PipelineKey(arg.Blend, arg.Depth, arg.Topology, arg.Wireframe, swapChain.RenderPass, swapChain.SampleCount);
            if (!_pipelineCache.TryGetValue(key, out VkPipeline pipeline))
            {
                pipeline = BuildPipeline(key);
                _pipelineCache[key] = pipeline;
            }

            _vk.CmdBindPipeline(cb, PipelineBindPoint.Graphics, pipeline);

            // Two SoA vertex buffer bindings: slot 0 = Position (offset 0),
            // slot 1 = Color (offset posBytes), both into the same VkBuffer.
            var bufs = stackalloc VkBuffer[2] { _vertexBuffer, _vertexBuffer };
            var offs = stackalloc ulong[2] { 0UL, posBytes };
            _vk.CmdBindVertexBuffers(cb, 0, 2, bufs, offs);

            var sets = stackalloc DescriptorSet[2] { _frameDataSet, _lineDataSet };
            uint dynOffset = lineDataOffset;
            _vk.CmdBindDescriptorSets(cb, PipelineBindPoint.Graphics, _pipelineLayout,
                0, 2, sets, 1, in dynOffset);

            _vk.CmdDraw(cb, (uint)_vertexCount, 1, 0, 0);
        }

        private unsafe void EnsureVertexBufferCapacity(uint size)
        {
            if (_vertexBuffer.Handle != 0 && _vertexBufferCapacity >= size) return;

            // Free the old buffer if any.
            if (_vertexBuffer.Handle != 0)
            {
                _vk.DeviceWaitIdle(_device); // safe: caller is on UI thread, not in CL recording for this buffer
                _vk.UnmapMemory(_device, _vertexMemory);
                _vk.DestroyBuffer(_device, _vertexBuffer, null);
                _vk.FreeMemory(_device, _vertexMemory, null);
            }

            uint newCap = Math.Max(size, _vertexBufferCapacity * 2);
            if (newCap < 4096) newCap = 4096;
            _vertexBufferCapacity = newCap;

            BufferCreateInfo info = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = newCap,
                Usage = BufferUsageFlags.VertexBufferBit,
                SharingMode = SharingMode.Exclusive,
            };
            VkBuffer buf;
            VkCheck.Ok(_vk.CreateBuffer(_device, in info, null, &buf));
            _vertexBuffer = buf;

            _vk.GetBufferMemoryRequirements(_device, _vertexBuffer, out MemoryRequirements req);
            MemoryAllocateInfo alloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size,
                MemoryTypeIndex = DeviceWrapper.FindMemoryType(req.MemoryTypeBits,
                    MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit),
            };
            DeviceMemory mem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in alloc, null, &mem));
            _vertexMemory = mem;
            _vk.BindBufferMemory(_device, _vertexBuffer, _vertexMemory, 0);

            void* mapped;
            VkCheck.Ok(_vk.MapMemory(_device, _vertexMemory, 0, newCap, 0, &mapped));
            _vertexMapped = mapped;
        }

        // ---- Pipeline construction -----------------------------------------

        private unsafe VkPipeline BuildPipeline(PipelineKey key)
        {
            // Shaders
            var entryName = stackalloc byte[5] { (byte)'m', (byte)'a', (byte)'i', (byte)'n', 0 };
            PipelineShaderStageCreateInfo vs = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.VertexBit,
                Module = _vsModule,
                PName = entryName,
            };
            PipelineShaderStageCreateInfo fs = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.FragmentBit,
                Module = _fsModule,
                PName = entryName,
            };
            var stages = stackalloc PipelineShaderStageCreateInfo[2] { vs, fs };

            // Vertex input: 2 bindings (SoA — Position float3, Color float4)
            var bindings = stackalloc VertexInputBindingDescription[2]
            {
                new VertexInputBindingDescription { Binding = 0, Stride = (uint)sizeof(Vector3), InputRate = VertexInputRate.Vertex },
                new VertexInputBindingDescription { Binding = 1, Stride = (uint)sizeof(Vector4), InputRate = VertexInputRate.Vertex },
            };
            var attributes = stackalloc VertexInputAttributeDescription[2]
            {
                new VertexInputAttributeDescription { Location = 0, Binding = 0, Format = Format.R32G32B32Sfloat,    Offset = 0 },
                new VertexInputAttributeDescription { Location = 1, Binding = 1, Format = Format.R32G32B32A32Sfloat, Offset = 0 },
            };
            PipelineVertexInputStateCreateInfo viState = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 2,
                PVertexBindingDescriptions = bindings,
                VertexAttributeDescriptionCount = 2,
                PVertexAttributeDescriptions = attributes,
            };

            PipelineInputAssemblyStateCreateInfo iaState = new PipelineInputAssemblyStateCreateInfo
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = key.Topology == Topology.TriangleList ? PrimitiveTopology.TriangleList : PrimitiveTopology.LineList,
                PrimitiveRestartEnable = false,
            };

            // Viewport + scissor are dynamic (set on every frame by VulkanSwapChain.EnsureRecording).
            PipelineViewportStateCreateInfo vpState = new PipelineViewportStateCreateInfo
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                ScissorCount = 1,
            };

            PipelineRasterizationStateCreateInfo rs = new PipelineRasterizationStateCreateInfo
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                PolygonMode = key.Wireframe ? PolygonMode.Line : PolygonMode.Fill,
                CullMode = CullModeFlags.None,
                FrontFace = FrontFace.CounterClockwise,
                LineWidth = 1.0f,
                DepthClampEnable = false,
                RasterizerDiscardEnable = false,
                DepthBiasEnable = false,
            };

            PipelineMultisampleStateCreateInfo ms = new PipelineMultisampleStateCreateInfo
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = key.Samples,
            };

            PipelineDepthStencilStateCreateInfo ds = new PipelineDepthStencilStateCreateInfo
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = key.Depth != DepthMode.NoZ,
                DepthWriteEnable = key.Depth == DepthMode.Default,
                DepthCompareOp = CompareOp.LessOrEqual,
                DepthBoundsTestEnable = false,
                StencilTestEnable = false,
            };

            PipelineColorBlendAttachmentState att = BuildBlendAttachment(key.Blend);
            PipelineColorBlendStateCreateInfo cb = new PipelineColorBlendStateCreateInfo
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                LogicOpEnable = false,
                AttachmentCount = 1,
                PAttachments = &att,
            };

            var dynStates = stackalloc DynamicState[2] { DynamicState.Viewport, DynamicState.Scissor };
            PipelineDynamicStateCreateInfo dyn = new PipelineDynamicStateCreateInfo
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 2,
                PDynamicStates = dynStates,
            };

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

        private static PipelineColorBlendAttachmentState BuildBlendAttachment(BlendMode blend)
        {
            ColorComponentFlags mask =
                ColorComponentFlags.RBit | ColorComponentFlags.GBit |
                ColorComponentFlags.BBit | ColorComponentFlags.ABit;
            return blend switch
            {
                BlendMode.Opaque => new PipelineColorBlendAttachmentState
                {
                    BlendEnable = false,
                    ColorWriteMask = mask,
                },
                BlendMode.NonPremultipliedAlpha => new PipelineColorBlendAttachmentState
                {
                    BlendEnable = true,
                    SrcColorBlendFactor = BlendFactor.SrcAlpha,
                    DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
                    ColorBlendOp = BlendOp.Add,
                    SrcAlphaBlendFactor = BlendFactor.One,
                    DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
                    AlphaBlendOp = BlendOp.Add,
                    ColorWriteMask = mask,
                },
                BlendMode.Additive => new PipelineColorBlendAttachmentState
                {
                    BlendEnable = true,
                    SrcColorBlendFactor = BlendFactor.SrcAlpha,
                    DstColorBlendFactor = BlendFactor.One,
                    ColorBlendOp = BlendOp.Add,
                    SrcAlphaBlendFactor = BlendFactor.One,
                    DstAlphaBlendFactor = BlendFactor.One,
                    AlphaBlendOp = BlendOp.Add,
                    ColorWriteMask = mask,
                },
                _ => new PipelineColorBlendAttachmentState
                {
                    BlendEnable = true,
                    SrcColorBlendFactor = BlendFactor.One,
                    DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
                    ColorBlendOp = BlendOp.Add,
                    SrcAlphaBlendFactor = BlendFactor.One,
                    DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
                    AlphaBlendOp = BlendOp.Add,
                    ColorWriteMask = mask,
                },
            };
        }

        public override unsafe void Dispose()
        {
            _vk.DeviceWaitIdle(_device);
            foreach (var p in _pipelineCache.Values)
                if (p.Handle != 0) _vk.DestroyPipeline(_device, p, null);
            _pipelineCache.Clear();

            if (_vertexMapped != null) { _vk.UnmapMemory(_device, _vertexMemory); _vertexMapped = null; }
            if (_vertexBuffer.Handle != 0) { _vk.DestroyBuffer(_device, _vertexBuffer, null); _vertexBuffer = default; }
            if (_vertexMemory.Handle != 0) { _vk.FreeMemory(_device, _vertexMemory, null); _vertexMemory = default; }

            if (_pipelineLayout.Handle != 0) { _vk.DestroyPipelineLayout(_device, _pipelineLayout, null); _pipelineLayout = default; }
            if (_set0Layout.Handle != 0) { _vk.DestroyDescriptorSetLayout(_device, _set0Layout, null); _set0Layout = default; }
            if (_set1Layout.Handle != 0) { _vk.DestroyDescriptorSetLayout(_device, _set1Layout, null); _set1Layout = default; }
            if (_vsModule.Handle != 0) { _vk.DestroyShaderModule(_device, _vsModule, null); _vsModule = default; }
            if (_fsModule.Handle != 0) { _vk.DestroyShaderModule(_device, _fsModule, null); _fsModule = default; }
        }
    }
}
