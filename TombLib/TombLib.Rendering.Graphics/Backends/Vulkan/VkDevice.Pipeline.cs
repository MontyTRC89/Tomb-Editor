using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using TombLib.Rendering.Graphics.Rhi;
using RhiFormat = TombLib.Rendering.Graphics.Rhi.Format;
using VkFormat  = Silk.NET.Vulkan.Format;

namespace TombLib.Rendering.Graphics.Backends.Vulkan;

// Graphics-pipeline and render-pass creation for the Vulkan backend.
public unsafe sealed partial class VkDevice
{
    // Cache of render passes keyed by attachment shape. A pipeline needs a
    // "compatible" render pass at creation time and BeginPass needs one too —
    // keying on these values yields one VkRenderPass per shape, reused
    // everywhere.
    internal readonly Dictionary<(VkFormat Color, VkFormat Depth, int Samples, bool Resolve), RenderPass>
        RenderPassCache = new();

    internal RenderPass GetOrCreateRenderPass(VkFormat colorFormat, VkFormat depthFormat,
                                              int samples, bool resolveToSwapchain)
    {
        var key = (colorFormat, depthFormat, samples, resolveToSwapchain);
        if (RenderPassCache.TryGetValue(key, out var cached))
            return cached;

        // Attachment 0 = MSAA / single-sample colour, cleared at start.
        // Attachment 1 = depth, cleared at start.
        // Attachment 2 = swapchain resolve target (MSAA + resolve only).
        bool multisampledResolve = samples > 1 && resolveToSwapchain;
        int  attachmentCount     = multisampledResolve ? 3 : 2;

        var attachments = stackalloc AttachmentDescription[3];
        attachments[0] = new AttachmentDescription
        {
            Format         = colorFormat,
            Samples        = VkMapping.ToSampleCount(samples),
            LoadOp         = AttachmentLoadOp.Clear,
            // No point storing the MSAA target when it gets resolved — the
            // resolve attachment is what gets presented.
            StoreOp        = multisampledResolve ? AttachmentStoreOp.DontCare : AttachmentStoreOp.Store,
            StencilLoadOp  = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout  = ImageLayout.Undefined,
            FinalLayout    = multisampledResolve
                             ? ImageLayout.ColorAttachmentOptimal
                             : resolveToSwapchain ? ImageLayout.PresentSrcKhr
                                                  : ImageLayout.ShaderReadOnlyOptimal,
        };
        attachments[1] = new AttachmentDescription
        {
            Format         = depthFormat,
            Samples        = VkMapping.ToSampleCount(samples),
            LoadOp         = AttachmentLoadOp.Clear,
            StoreOp        = AttachmentStoreOp.DontCare,
            StencilLoadOp  = AttachmentLoadOp.Clear,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout  = ImageLayout.Undefined,
            FinalLayout    = ImageLayout.DepthStencilAttachmentOptimal,
        };
        if (multisampledResolve)
        {
            attachments[2] = new AttachmentDescription
            {
                Format         = colorFormat,
                Samples        = SampleCountFlags.Count1Bit,
                LoadOp         = AttachmentLoadOp.DontCare,
                StoreOp        = AttachmentStoreOp.Store,
                StencilLoadOp  = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout  = ImageLayout.Undefined,
                FinalLayout    = ImageLayout.PresentSrcKhr,
            };
        }

        var colorRef   = new AttachmentReference { Attachment = 0, Layout = ImageLayout.ColorAttachmentOptimal };
        var depthRef   = new AttachmentReference { Attachment = 1, Layout = ImageLayout.DepthStencilAttachmentOptimal };
        var resolveRef = new AttachmentReference { Attachment = 2, Layout = ImageLayout.ColorAttachmentOptimal };

        var subpass = new SubpassDescription
        {
            PipelineBindPoint       = PipelineBindPoint.Graphics,
            ColorAttachmentCount    = 1,
            PColorAttachments       = &colorRef,
            PDepthStencilAttachment = &depthRef,
            PResolveAttachments     = multisampledResolve ? &resolveRef : null,
        };

        // External dependencies so layout transitions happen at the right
        // pipeline stage — one for colour, one for depth.
        var dependencies = stackalloc SubpassDependency[2];
        dependencies[0] = new SubpassDependency
        {
            SrcSubpass    = Vk.SubpassExternal,
            DstSubpass    = 0,
            SrcStageMask  = PipelineStageFlags.ColorAttachmentOutputBit,
            DstStageMask  = PipelineStageFlags.ColorAttachmentOutputBit,
            SrcAccessMask = 0,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit,
        };
        dependencies[1] = new SubpassDependency
        {
            SrcSubpass    = Vk.SubpassExternal,
            DstSubpass    = 0,
            SrcStageMask  = PipelineStageFlags.EarlyFragmentTestsBit | PipelineStageFlags.LateFragmentTestsBit,
            DstStageMask  = PipelineStageFlags.EarlyFragmentTestsBit | PipelineStageFlags.LateFragmentTestsBit,
            SrcAccessMask = 0,
            DstAccessMask = AccessFlags.DepthStencilAttachmentWriteBit,
        };

        var renderPassCreateInfo = new RenderPassCreateInfo
        {
            SType           = StructureType.RenderPassCreateInfo,
            AttachmentCount = (uint)attachmentCount,
            PAttachments    = attachments,
            SubpassCount    = 1,
            PSubpasses      = &subpass,
            DependencyCount = 2,
            PDependencies   = dependencies,
        };
        if (Api.CreateRenderPass(Device, in renderPassCreateInfo, null, out var renderPass) != Result.Success)
            throw new InvalidOperationException("vkCreateRenderPass failed");

        RenderPassCache[key] = renderPass;
        return renderPass;
    }

    public PipelineHandle CreatePipeline(PipelineDesc desc)
    {
        // ---- Shader stages — load SPIR-V into shader modules ----
        byte[] vertexShaderSpirv = desc.VertexShader.SpirvBytes
            ?? throw new InvalidOperationException("VertexShader.SpirvBytes is null. Did dxc produce the .spv?");
        byte[] pixelShaderSpirv = desc.FragmentShader.SpirvBytes
            ?? throw new InvalidOperationException("FragmentShader.SpirvBytes is null. Did dxc produce the .spv?");
        var vertexShaderModule = CreateShaderModule(vertexShaderSpirv);
        var pixelShaderModule  = CreateShaderModule(pixelShaderSpirv);

        // The SPIR-V entry points keep their HLSL names ("vs_main" / "ps_main")
        // because the build pipeline invokes DXC with `-E <name>`; the pipeline
        // must reference each by the exact name baked into its SPIR-V.
        byte* vertexEntryPoint = (byte*)Marshal.StringToHGlobalAnsi(desc.VertexShader.EntryPoint   ?? "vs_main");
        byte* pixelEntryPoint  = (byte*)Marshal.StringToHGlobalAnsi(desc.FragmentShader.EntryPoint ?? "ps_main");
        var shaderStages = stackalloc PipelineShaderStageCreateInfo[2]
        {
            new()
            {
                SType  = StructureType.PipelineShaderStageCreateInfo,
                Stage  = ShaderStageFlags.VertexBit,
                Module = vertexShaderModule,
                PName  = vertexEntryPoint,
            },
            new()
            {
                SType  = StructureType.PipelineShaderStageCreateInfo,
                Stage  = ShaderStageFlags.FragmentBit,
                Module = pixelShaderModule,
                PName  = pixelEntryPoint,
            },
        };

        // ---- Vertex input — bindings + attributes from PipelineDesc ----
        var vertexBindings = new VertexInputBindingDescription[desc.VertexBufferLayouts.Length];
        for (int i = 0; i < desc.VertexBufferLayouts.Length; i++)
        {
            var layout = desc.VertexBufferLayouts[i];
            vertexBindings[i] = new VertexInputBindingDescription
            {
                Binding   = (uint)i,
                Stride    = (uint)layout.StrideBytes,
                InputRate = layout.PerInstance ? VertexInputRate.Instance : VertexInputRate.Vertex,
            };
        }

        // SPIR-V from DXC HLSL puts attribute locations at the explicit
        // [[vk::location(N)]] decoration (emitted in Bindings.hlsli). We
        // assume Location == index-in-VertexAttributes, which the existing
        // shaders are authored to match.
        var vertexAttributes = new VertexInputAttributeDescription[desc.VertexAttributes.Length];
        for (int i = 0; i < desc.VertexAttributes.Length; i++)
        {
            var attribute = desc.VertexAttributes[i];
            vertexAttributes[i] = new VertexInputAttributeDescription
            {
                Location = (uint)i,
                Binding  = (uint)attribute.BufferSlot,
                Format   = VkMapping.ToVk(attribute.Format),
                Offset   = (uint)attribute.Offset,
            };
        }

        // Silk takes the input rate per *binding*, so make sure every binding
        // referenced by a per-instance attribute is marked per-instance.
        for (int i = 0; i < vertexAttributes.Length; i++)
        {
            if (desc.VertexAttributes[i].PerInstance)
            {
                int slot = desc.VertexAttributes[i].BufferSlot;
                if (slot < vertexBindings.Length)
                    vertexBindings[slot].InputRate = VertexInputRate.Instance;
            }
        }

        fixed (VertexInputBindingDescription*   pBindings   = vertexBindings)
        fixed (VertexInputAttributeDescription* pAttributes = vertexAttributes)
        {
            var vertexInputState = new PipelineVertexInputStateCreateInfo
            {
                SType                           = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount   = (uint)vertexBindings.Length,
                PVertexBindingDescriptions      = pBindings,
                VertexAttributeDescriptionCount = (uint)vertexAttributes.Length,
                PVertexAttributeDescriptions    = pAttributes,
            };

            var inputAssemblyState = new PipelineInputAssemblyStateCreateInfo
            {
                SType    = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = VkMapping.ToVk(desc.Topology),
            };

            // Viewport / scissor are dynamic state, so only the counts matter here.
            var viewportState = new PipelineViewportStateCreateInfo
            {
                SType         = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                ScissorCount  = 1,
            };

            var rasterizer = desc.Rasterizer;
            // With DXC -fvk-invert-y the SV_Position.y is negated in clip
            // space, which after the standard Vulkan viewport transform
            // produces the same SCREEN positions as DX11 — and the same
            // winding from the rasterizer's POV. So FrontFace matches the RHI
            // request 1:1, with no inversion.
            var rasterizationState = new PipelineRasterizationStateCreateInfo
            {
                SType                   = StructureType.PipelineRasterizationStateCreateInfo,
                DepthClampEnable        = false,
                RasterizerDiscardEnable = false,
                PolygonMode             = VkMapping.ToVk(rasterizer.FillMode),
                CullMode                = VkMapping.ToVk(rasterizer.CullMode),
                FrontFace               = rasterizer.FrontCounterClockwise
                                          ? FrontFace.CounterClockwise
                                          : FrontFace.Clockwise,
                DepthBiasEnable         = rasterizer.DepthBias != 0,
                DepthBiasConstantFactor = rasterizer.DepthBias,
                DepthBiasSlopeFactor    = rasterizer.SlopeScaledDepthBias,
                DepthBiasClamp          = 0,
                LineWidth               = 1.0f,
            };

            // Every pipeline in this renderer targets the (MSAA) swapchain, so
            // it is compiled at the swapchain's sample count and against the
            // swapchain's render-pass shape.
            var multisampleState = new PipelineMultisampleStateCreateInfo
            {
                SType                = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = VkMapping.ToSampleCount(MsaaSamples),
                SampleShadingEnable  = false,
            };

            var depthStencil = desc.DepthStencil;
            var depthStencilState = new PipelineDepthStencilStateCreateInfo
            {
                SType                 = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable       = depthStencil.DepthTestEnable,
                DepthWriteEnable      = depthStencil.DepthWriteEnable,
                DepthCompareOp        = VkMapping.ToVk(depthStencil.DepthCompare),
                DepthBoundsTestEnable = false,
                StencilTestEnable     = false,
            };

            var blend = desc.BlendStates.Length > 0 ? desc.BlendStates[0] : BlendState.Opaque;
            var blendAttachment = new PipelineColorBlendAttachmentState
            {
                BlendEnable         = blend.Enable,
                SrcColorBlendFactor = VkMapping.ToVk(blend.SrcColor),
                DstColorBlendFactor = VkMapping.ToVk(blend.DstColor),
                ColorBlendOp        = VkMapping.ToVk(blend.ColorOp),
                SrcAlphaBlendFactor = VkMapping.ToVk(blend.SrcAlpha),
                DstAlphaBlendFactor = VkMapping.ToVk(blend.DstAlpha),
                AlphaBlendOp        = VkMapping.ToVk(blend.AlphaOp),
                ColorWriteMask      = ColorComponentFlags.RBit | ColorComponentFlags.GBit |
                                      ColorComponentFlags.BBit | ColorComponentFlags.ABit,
            };
            var colorBlendState = new PipelineColorBlendStateCreateInfo
            {
                SType           = StructureType.PipelineColorBlendStateCreateInfo,
                LogicOpEnable   = false,
                AttachmentCount = 1,
                PAttachments    = &blendAttachment,
            };

            // Dynamic state — viewport + scissor are set per draw.
            var dynamicStates = stackalloc DynamicState[2] { DynamicState.Viewport, DynamicState.Scissor };
            var dynamicState  = new PipelineDynamicStateCreateInfo
            {
                SType             = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 2,
                PDynamicStates    = dynamicStates,
            };

            // Compile against a render pass whose shape matches the swapchain
            // exactly (MsaaSamples + resolveToSwapchain), so the same cache key
            // yields the same VkRenderPass and the pipeline is guaranteed
            // compatible at draw time.
            var colorFormat = desc.ColorAttachmentFormats.Length > 0
                              ? desc.ColorAttachmentFormats[0]
                              : RhiFormat.R8G8B8A8_UNorm;
            var depthFormat = desc.DepthAttachmentFormat;
            var renderPass  = GetOrCreateRenderPass(
                VkMapping.ToVk(colorFormat), VkMapping.ToVk(depthFormat), MsaaSamples, resolveToSwapchain: true);

            var pipelineCreateInfo = new GraphicsPipelineCreateInfo
            {
                SType               = StructureType.GraphicsPipelineCreateInfo,
                StageCount          = 2,
                PStages             = shaderStages,
                PVertexInputState   = &vertexInputState,
                PInputAssemblyState = &inputAssemblyState,
                PViewportState      = &viewportState,
                PRasterizationState = &rasterizationState,
                PMultisampleState   = &multisampleState,
                PDepthStencilState  = &depthStencilState,
                PColorBlendState    = &colorBlendState,
                PDynamicState       = &dynamicState,
                Layout              = SharedPipelineLayout,
                RenderPass          = renderPass,
                Subpass             = 0,
            };

            Pipeline pipeline;
            var result = Api.CreateGraphicsPipelines(Device, default, 1, in pipelineCreateInfo, null, &pipeline);
            if (result != Result.Success)
                throw new InvalidOperationException(
                    $"vkCreateGraphicsPipelines failed: {result} (debug='{desc.DebugName}'). " +
                    "Common causes: SPIR-V entry-point name mismatch, vertex attribute " +
                    "location not declared in the shader, push-constant size > device max, " +
                    "or render-pass attachment count mismatch.");

            Api.DestroyShaderModule(Device, vertexShaderModule, null);
            Api.DestroyShaderModule(Device, pixelShaderModule, null);
            Marshal.FreeHGlobal((IntPtr)vertexEntryPoint);
            Marshal.FreeHGlobal((IntPtr)pixelEntryPoint);

            int[] vertexStrides = new int[desc.VertexBufferLayouts.Length];
            for (int i = 0; i < vertexStrides.Length; i++)
                vertexStrides[i] = desc.VertexBufferLayouts[i].StrideBytes;

            uint id = AllocHandle();
            Pipelines[id] = new VkPipelineRes
            {
                Handle        = pipeline,
                Layout        = SharedPipelineLayout,
                VertexStrides = vertexStrides,
                ColorFormats  = desc.ColorAttachmentFormats,
                DepthFormat   = depthFormat,
                Samples       = MsaaSamples,
                BasePass      = renderPass,
            };
            return new PipelineHandle(id);
        }
    }

    private ShaderModule CreateShaderModule(byte[] spirv)
    {
        fixed (byte* pSpirv = spirv)
        {
            var createInfo = new ShaderModuleCreateInfo
            {
                SType    = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)spirv.Length,
                PCode    = (uint*)pSpirv,
            };
            Api.CreateShaderModule(Device, in createInfo, null, out var module);
            return module;
        }
    }

    public void Destroy(PipelineHandle handle)
    {
        if (Pipelines.Remove(handle.Id, out var pipeline))
            _pendingDeletes.Add(() => DestroyPipelineInternal(pipeline));
    }

    internal void DestroyPipelineInternal(VkPipelineRes pipeline)
    {
        if (pipeline.Handle.Handle != 0)
            Api.DestroyPipeline(Device, pipeline.Handle, null);
        // Layout is shared (SharedPipelineLayout) — not destroyed here.
    }
}
