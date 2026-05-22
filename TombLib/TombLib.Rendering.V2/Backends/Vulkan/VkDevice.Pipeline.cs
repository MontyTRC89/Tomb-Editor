using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using TombLib.RenderingV2.Rhi;
using RhiFormat = TombLib.RenderingV2.Rhi.Format;
using VkFormat  = Silk.NET.Vulkan.Format;

namespace TombLib.RenderingV2.Backends.Vulkan;

public unsafe sealed partial class VkDevice
{
    // Cache of render passes keyed by (color format, depth format, samples).
    // The pipeline needs a "compatible" render pass at creation time, and
    // BeginPass also needs one — by keying on those three values we get one
    // VkRenderPass per attachment shape and reuse it everywhere.
    internal readonly Dictionary<(VkFormat color, VkFormat depth, int samples, bool resolve), RenderPass>
        RenderPassCache = new();

    internal RenderPass GetOrCreateRenderPass(VkFormat colorFmt, VkFormat depthFmt, int samples, bool resolveToSwapchain)
    {
        var key = (colorFmt, depthFmt, samples, resolveToSwapchain);
        if (RenderPassCache.TryGetValue(key, out var existing)) return existing;

        // Attachment 0 = MSAA / single-sample color, cleared at start.
        // Attachment 1 = depth, cleared at start.
        // Attachment 2 (optional, MSAA only) = swapchain resolve target.
        int attachmentCount = samples > 1 && resolveToSwapchain ? 3 : 2;
        var attachments = stackalloc AttachmentDescription[3];
        attachments[0] = new AttachmentDescription
        {
            Format         = colorFmt,
            Samples        = VkMapping.ToSampleCount(samples),
            LoadOp         = AttachmentLoadOp.Clear,
            // Don't bother storing the MSAA target if we're resolving it —
            // the resolve attachment is what we present.
            StoreOp        = samples > 1 && resolveToSwapchain ? AttachmentStoreOp.DontCare : AttachmentStoreOp.Store,
            StencilLoadOp  = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout  = ImageLayout.Undefined,
            FinalLayout    = samples > 1 && resolveToSwapchain
                             ? ImageLayout.ColorAttachmentOptimal
                             : (resolveToSwapchain ? ImageLayout.PresentSrcKhr : ImageLayout.ShaderReadOnlyOptimal),
        };
        attachments[1] = new AttachmentDescription
        {
            Format         = depthFmt,
            Samples        = VkMapping.ToSampleCount(samples),
            LoadOp         = AttachmentLoadOp.Clear,
            StoreOp        = AttachmentStoreOp.DontCare,
            StencilLoadOp  = AttachmentLoadOp.Clear,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout  = ImageLayout.Undefined,
            FinalLayout    = ImageLayout.DepthStencilAttachmentOptimal,
        };
        if (samples > 1 && resolveToSwapchain)
        {
            attachments[2] = new AttachmentDescription
            {
                Format         = colorFmt,
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
            PResolveAttachments     = samples > 1 && resolveToSwapchain ? &resolveRef : null,
        };

        // External dependency to ensure layout transitions happen at the
        // right pipeline stage. Two: one for color, one for depth.
        var deps = stackalloc SubpassDependency[2];
        deps[0] = new SubpassDependency
        {
            SrcSubpass    = Vk.SubpassExternal,
            DstSubpass    = 0,
            SrcStageMask  = PipelineStageFlags.ColorAttachmentOutputBit,
            DstStageMask  = PipelineStageFlags.ColorAttachmentOutputBit,
            SrcAccessMask = 0,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit,
        };
        deps[1] = new SubpassDependency
        {
            SrcSubpass    = Vk.SubpassExternal,
            DstSubpass    = 0,
            SrcStageMask  = PipelineStageFlags.EarlyFragmentTestsBit | PipelineStageFlags.LateFragmentTestsBit,
            DstStageMask  = PipelineStageFlags.EarlyFragmentTestsBit | PipelineStageFlags.LateFragmentTestsBit,
            SrcAccessMask = 0,
            DstAccessMask = AccessFlags.DepthStencilAttachmentWriteBit,
        };

        var rpci = new RenderPassCreateInfo
        {
            SType           = StructureType.RenderPassCreateInfo,
            AttachmentCount = (uint)attachmentCount,
            PAttachments    = attachments,
            SubpassCount    = 1,
            PSubpasses      = &subpass,
            DependencyCount = 2,
            PDependencies   = deps,
        };
        if (Api.CreateRenderPass(Device, in rpci, null, out var rp) != Result.Success)
            throw new InvalidOperationException("vkCreateRenderPass failed");
        RenderPassCache[key] = rp;
        return rp;
    }

    public PipelineHandle CreatePipeline(PipelineDesc desc)
    {
        // ---- Shader stages: load SPIR-V into shader modules.
        byte[] vsBytes = desc.VertexShader.SpirvBytes
            ?? throw new InvalidOperationException("VertexShader.SpirvBytes is null. Did dxc produce the .spv?");
        byte[] psBytes = desc.FragmentShader.SpirvBytes
            ?? throw new InvalidOperationException("FragmentShader.SpirvBytes is null. Did dxc produce the .spv?");
        var vsModule = CreateShaderModule(vsBytes);
        var psModule = CreateShaderModule(psBytes);

        // The SPIR-V entry points keep their HLSL names ("vs_main" / "ps_main")
        // because the build pipeline invokes DXC with `-E <name>`. The
        // pipeline must reference each by the exact name in its SPIR-V.
        byte* vsEntry = (byte*)Marshal.StringToHGlobalAnsi(desc.VertexShader.EntryPoint   ?? "vs_main");
        byte* psEntry = (byte*)Marshal.StringToHGlobalAnsi(desc.FragmentShader.EntryPoint ?? "ps_main");
        var stages = stackalloc PipelineShaderStageCreateInfo[2]
        {
            new()
            {
                SType  = StructureType.PipelineShaderStageCreateInfo,
                Stage  = ShaderStageFlags.VertexBit,
                Module = vsModule,
                PName  = vsEntry,
            },
            new()
            {
                SType  = StructureType.PipelineShaderStageCreateInfo,
                Stage  = ShaderStageFlags.FragmentBit,
                Module = psModule,
                PName  = psEntry,
            },
        };

        // ---- Vertex input: bindings + attributes from PipelineDesc.
        var bindings = new VertexInputBindingDescription[desc.VertexBufferLayouts.Length];
        for (int i = 0; i < desc.VertexBufferLayouts.Length; i++)
        {
            var l = desc.VertexBufferLayouts[i];
            bindings[i] = new VertexInputBindingDescription
            {
                Binding   = (uint)i,
                Stride    = (uint)l.StrideBytes,
                InputRate = l.PerInstance ? VertexInputRate.Instance : VertexInputRate.Vertex,
            };
        }
        // SPIR-V from DXC HLSL puts attribute locations at the explicit
        // [[vk::location(N)]] decoration (we emit those in Bindings.hlsli).
        // We assume Location == index-in-VertexAttributes here, since the
        // existing shaders are authored to match.
        var attrs = new VertexInputAttributeDescription[desc.VertexAttributes.Length];
        for (int i = 0; i < desc.VertexAttributes.Length; i++)
        {
            var a = desc.VertexAttributes[i];
            attrs[i] = new VertexInputAttributeDescription
            {
                Location = (uint)i,
                Binding  = (uint)a.BufferSlot,
                Format   = VkMapping.ToVk(a.Format),
                Offset   = (uint)a.Offset,
            };
        }
        // Also rebinding per-instance flag — Silk takes input rate per
        // *binding*, so if a binding is per-instance the layout already said so.
        // Make sure perInstance attribute's binding has matching input rate.
        for (int i = 0; i < attrs.Length; i++)
        {
            if (desc.VertexAttributes[i].PerInstance)
            {
                int slot = desc.VertexAttributes[i].BufferSlot;
                if (slot < bindings.Length)
                    bindings[slot].InputRate = VertexInputRate.Instance;
            }
        }

        fixed (VertexInputBindingDescription*   pBind = bindings)
        fixed (VertexInputAttributeDescription* pAttr = attrs)
        {
            var vi = new PipelineVertexInputStateCreateInfo
            {
                SType                           = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount   = (uint)bindings.Length,
                PVertexBindingDescriptions      = pBind,
                VertexAttributeDescriptionCount = (uint)attrs.Length,
                PVertexAttributeDescriptions    = pAttr,
            };

            var ia = new PipelineInputAssemblyStateCreateInfo
            {
                SType    = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = VkMapping.ToVk(desc.Topology),
            };

            // Viewport / scissor are dynamic, so just declare counts.
            var vp = new PipelineViewportStateCreateInfo
            {
                SType         = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                ScissorCount  = 1,
            };

            var rs = desc.Rasterizer;
            // With DXC -fvk-invert-y the SV_Position.y is negated in clip
            // space, which after the standard Vulkan viewport transform
            // produces the same SCREEN positions as DX11 — and the same
            // winding from the rasterizer's POV. So the FrontFace
            // declaration matches the RHI request 1:1 (no inversion).
            var rsCi = new PipelineRasterizationStateCreateInfo
            {
                SType                   = StructureType.PipelineRasterizationStateCreateInfo,
                DepthClampEnable        = false,
                RasterizerDiscardEnable = false,
                PolygonMode             = VkMapping.ToVk(rs.FillMode),
                CullMode                = VkMapping.ToVk(rs.CullMode),
                FrontFace               = rs.FrontCounterClockwise ? FrontFace.CounterClockwise : FrontFace.Clockwise,
                DepthBiasEnable         = rs.DepthBias != 0,
                DepthBiasConstantFactor = rs.DepthBias,
                DepthBiasSlopeFactor    = rs.SlopeScaledDepthBias,
                DepthBiasClamp          = 0,
                LineWidth               = 1.0f,
            };

            int samples = desc.ColorAttachmentFormats.Length > 0 ? 1 : 1;
            // We don't know samples from PipelineDesc — assume the worst (any
            // 1× / 4× compatible pass). For pipelines we set samples=1 by
            // default; multi-sample pipelines are recompiled lazily when a
            // pass requires it. To keep things simple here, we always create
            // the pipeline at the swapchain's MSAA sample count when used on
            // the swapchain. Since PipelineDesc doesn't carry samples, we
            // default to 1 and rely on the user creating one pipeline per
            // MSAA-distinct render target. The existing renderer only renders
            // to the swapchain, so we compile for the swapchain samples below.
            // (See AdaptPipelineSamples — built on first BeginPass with a
            //  different sample count.)
            var ms = new PipelineMultisampleStateCreateInfo
            {
                SType                 = StructureType.PipelineMultisampleStateCreateInfo,
                // Must match the swapchain render pass' sample count — every
                // pipeline in this renderer targets the (MSAA) swapchain.
                RasterizationSamples  = VkMapping.ToSampleCount(MsaaSamples),
                SampleShadingEnable   = false,
            };

            var ds = desc.DepthStencil;
            var dsCi = new PipelineDepthStencilStateCreateInfo
            {
                SType                 = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable       = ds.DepthTestEnable,
                DepthWriteEnable      = ds.DepthWriteEnable,
                DepthCompareOp        = VkMapping.ToVk(ds.DepthCompare),
                DepthBoundsTestEnable = false,
                StencilTestEnable     = false,
            };

            var bs = desc.BlendStates.Length > 0 ? desc.BlendStates[0] : BlendState.Opaque;
            var blendAttach = new PipelineColorBlendAttachmentState
            {
                BlendEnable         = bs.Enable,
                SrcColorBlendFactor = VkMapping.ToVk(bs.SrcColor),
                DstColorBlendFactor = VkMapping.ToVk(bs.DstColor),
                ColorBlendOp        = VkMapping.ToVk(bs.ColorOp),
                SrcAlphaBlendFactor = VkMapping.ToVk(bs.SrcAlpha),
                DstAlphaBlendFactor = VkMapping.ToVk(bs.DstAlpha),
                AlphaBlendOp        = VkMapping.ToVk(bs.AlphaOp),
                ColorWriteMask      = ColorComponentFlags.RBit | ColorComponentFlags.GBit |
                                      ColorComponentFlags.BBit | ColorComponentFlags.ABit,
            };
            var cb = new PipelineColorBlendStateCreateInfo
            {
                SType           = StructureType.PipelineColorBlendStateCreateInfo,
                LogicOpEnable   = false,
                AttachmentCount = 1,
                PAttachments    = &blendAttach,
            };

            // Dynamic state: viewport + scissor (set per draw).
            var dynStates = stackalloc DynamicState[2] { DynamicState.Viewport, DynamicState.Scissor };
            var dyn = new PipelineDynamicStateCreateInfo
            {
                SType             = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 2,
                PDynamicStates    = dynStates,
            };

            // Pick a render-pass shape that matches the pipeline's color +
            // depth formats. samples=1 for the "1×" variant; 4× variants will
            // be built lazily by BeginPass on first use.
            var colorFmtRhi = desc.ColorAttachmentFormats.Length > 0 ? desc.ColorAttachmentFormats[0] : RhiFormat.R8G8B8A8_UNorm;
            var depthFmtRhi = desc.DepthAttachmentFormat;
            // MsaaSamples + resolveToSwapchain:true so this matches the
            // swapchain's render pass shape exactly (same cache key → same
            // VkRenderPass → guaranteed compatible at draw time).
            var rp = GetOrCreateRenderPass(VkMapping.ToVk(colorFmtRhi), VkMapping.ToVk(depthFmtRhi), MsaaSamples, resolveToSwapchain: true);

            var gpci = new GraphicsPipelineCreateInfo
            {
                SType               = StructureType.GraphicsPipelineCreateInfo,
                StageCount          = 2,
                PStages             = stages,
                PVertexInputState   = &vi,
                PInputAssemblyState = &ia,
                PViewportState      = &vp,
                PRasterizationState = &rsCi,
                PMultisampleState   = &ms,
                PDepthStencilState  = &dsCi,
                PColorBlendState    = &cb,
                PDynamicState       = &dyn,
                Layout              = SharedPipelineLayout,
                RenderPass          = rp,
                Subpass             = 0,
            };

            Pipeline pipeline;
            var res = Api.CreateGraphicsPipelines(Device, default, 1, in gpci, null, &pipeline);
            if (res != Result.Success)
                throw new InvalidOperationException(
                    $"vkCreateGraphicsPipelines failed: {res} (debug='{desc.DebugName}'). " +
                    "Common causes: SPIR-V entry-point name mismatch, vertex attribute " +
                    "location not declared in the shader, push-constant size > device max, " +
                    "or render-pass attachment count mismatch.");

            Api.DestroyShaderModule(Device, vsModule, null);
            Api.DestroyShaderModule(Device, psModule, null);
            Marshal.FreeHGlobal((IntPtr)vsEntry);
            Marshal.FreeHGlobal((IntPtr)psEntry);

            int[] strides = new int[desc.VertexBufferLayouts.Length];
            for (int i = 0; i < strides.Length; i++) strides[i] = desc.VertexBufferLayouts[i].StrideBytes;

            uint id = AllocHandle();
            Pipelines[id] = new VkPipelineRes
            {
                Handle        = pipeline,
                Layout        = SharedPipelineLayout,
                VertexStrides = strides,
                ColorFormats  = desc.ColorAttachmentFormats,
                DepthFormat   = depthFmtRhi,
                Samples       = MsaaSamples,
                BasePass      = rp,
            };
            return new PipelineHandle(id);
        }
    }

    private ShaderModule CreateShaderModule(byte[] spirv)
    {
        fixed (byte* p = spirv)
        {
            var smci = new ShaderModuleCreateInfo
            {
                SType    = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)spirv.Length,
                PCode    = (uint*)p,
            };
            Api.CreateShaderModule(Device, in smci, null, out var mod);
            return mod;
        }
    }

    public void Destroy(PipelineHandle h)
    {
        if (Pipelines.Remove(h.Id, out var p))
            _pendingDeletes.Add(() => DestroyPipelineInternal(p));
    }

    internal void DestroyPipelineInternal(VkPipelineRes p)
    {
        if (p.Handle.Handle != 0) Api.DestroyPipeline(Device, p.Handle, null);
        // Note: Layout is shared (SharedPipelineLayout), don't destroy here.
    }
}
