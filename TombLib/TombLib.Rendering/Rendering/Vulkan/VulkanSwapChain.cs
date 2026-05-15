using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Shaderc;
using Silk.NET.Vulkan;
using TombLib.Utils;
using VkBuffer = Silk.NET.Vulkan.Buffer;
using VkPipeline = Silk.NET.Vulkan.Pipeline;

namespace TombLib.Rendering.Vulkan
{
    // Per-window Vulkan presentation surface + swap chain + render machinery.
    // Owns:
    //   - VkSurfaceKHR (from a Win32 HWND)
    //   - VkSwapchainKHR + N color images / image views
    //   - VkImage + memory for the depth attachment
    //   - VkRenderPass (single subpass: 1 color + 1 depth)
    //   - VkFramebuffer per swap image
    //   - VkCommandPool + VkCommandBuffer per frame-in-flight slot
    //   - VkSemaphore (image-available) + VkSemaphore (render-done) + VkFence per slot
    //
    // Frame model: 2 frames-in-flight (FRAMES_IN_FLIGHT). On Clear we acquire the
    // next swap image, wait on the slot's fence, reset the command buffer, begin
    // recording, and open the render pass. Drawing classes record their commands
    // into the recording command buffer. Present ends the pass, submits, and
    // calls vkQueuePresentKHR.
    public sealed class VulkanSwapChain : RenderingSwapChain
    {
        // Single-frame model — matches DX11's mental model: at every Clear()
        // the previous frame's CB has fully completed on the GPU, so resource
        // Dispose() can simply DeviceWaitIdle and destroy. No deletion queue,
        // no per-slot fence juggling. Trades a tiny amount of GPU pipelining
        // for total elimination of resource-lifetime bug surface. Can be
        // raised back to 2 once everything else is solid.
        private const int FramesInFlight = 1;

        public readonly VulkanRenderingDevice DeviceWrapper;
        private readonly Vk _vk;
        private readonly Device _device;
        private readonly bool _antialiasRequested;

        private SurfaceKHR _surface;
        private SwapchainKHR _swapchain;
        private Format _colorFormat;
        private Format _depthFormat;
        private Extent2D _extent;
        private SampleCountFlags _sampleCount;  // 1 = no MSAA, 4 = MSAA 4x
        private Image[] _images;
        private ImageView[] _imageViews;

        // Multisample color attachment (only created when _sampleCount > 1).
        // Rendering writes here; subpass resolve copies to the matching swap
        // image at end-of-pass.
        private Image _msaaImage;
        private DeviceMemory _msaaMemory;
        private ImageView _msaaView;

        private Image _depthImage;
        private DeviceMemory _depthMemory;
        private ImageView _depthView;

        private RenderPass _renderPass;
        private Framebuffer[] _framebuffers;

        private CommandPool _commandPool;
        private CommandBuffer[] _commandBuffers;
        private Semaphore[] _imageAvailable;
        private Semaphore[] _renderFinished;
        private Fence[] _inFlight;

        private int _slot;                  // current frame-in-flight slot
        private uint _imageIndex;           // current swap image index
        private bool _recording;            // command buffer between Begin/End

        // Cleared each frame at Clear() time. The render-pass clear values come
        // from these — VkCmdBeginRenderPass picks them up.
        private ClearValue _clearColorValue;
        private ClearValue _clearDepthValue = new ClearValue { DepthStencil = new ClearDepthStencilValue(1.0f, 0) };

        // ---- Text overlay pipeline resources --------------------------------
        private ShaderModule _textVs, _textFs;
        private DescriptorSetLayout _textSetLayout;
        private PipelineLayout _textPipelineLayout;
        private VkPipeline _textPipeline;
        private DescriptorSet _textAtlasSet;
        private ulong _textCachedAtlasView;
        private VkBuffer _textVB;
        private DeviceMemory _textVBMem;
        private unsafe void* _textVBMapped;
        private uint _textVBCapacity;

        // ---- Sprite overlay pipeline resources ------------------------------
        private ShaderModule _spriteVs, _spriteFs;
        private DescriptorSetLayout _spriteSetLayout;
        private PipelineLayout _spritePipelineLayout;
        private readonly Dictionary<bool, VkPipeline> _spritePipelines = new Dictionary<bool, VkPipeline>();
        private DescriptorSet _spriteAtlasSet;
        private ulong _spriteCachedAtlasView;
        private bool _spriteCachedLinear;
        private VkBuffer _spriteVB;
        private DeviceMemory _spriteVBMem;
        private unsafe void* _spriteVBMapped;
        private uint _spriteVBCapacity;
        // Per-frame append cursor — reset in Clear(). Each RenderSprites
        // / RenderGlyphs call writes at the cursor and advances it, so
        // multiple calls per frame don't trample each other's data.
        private uint _spriteVBFrameCursor;
        private uint _textVBFrameCursor;

        public RenderPass RenderPass => _renderPass;
        public Format ColorFormat => _colorFormat;
        public Format DepthFormat => _depthFormat;
        public Extent2D Extent => _extent;

        // Sample count of the render pass color/depth attachments. Pipelines
        // must declare this in PipelineMultisampleStateCreateInfo to match the
        // render pass — Vulkan rejects mismatched samples at create time.
        public SampleCountFlags SampleCount => _sampleCount;

        // Current frame's command buffer — valid between Clear() and Present()
        // (i.e. inside the render-pass recording window). Drawing* classes call
        // EnsureRecording first, then read this to record their cmd... calls.
        public CommandBuffer CurrentCommandBuffer => _commandBuffers[_slot];

        // Used by VulkanRenderingDevice.TryDrainPendingDestroys to check whether
        // this swap chain's GPU work has retired (vkGetFenceStatus). Initial
        // fence is created signaled, so a freshly-created swap chain reports
        // "idle" — pending destroys can flush immediately.
        public Fence CurrentInFlightFence => _inFlight != null && _inFlight.Length > 0 ? _inFlight[_slot] : default;

        // Monotonically bumped in Clear(). Drawing classes that maintain a
        // per-frame append cursor inside a persistently-mapped vertex buffer
        // (e.g. VulkanDrawingLines) compare against their last-seen FrameIndex
        // to know when to rewind the cursor, instead of trampling offset 0 on
        // every call.
        public uint FrameIndex { get; private set; }

        // FrameUniforms lives on the device (shared across swap chains).
        // See VulkanRenderingDevice.FrameUniforms for the rationale.

        // ---- GLSL shaders (inline) ------------------------------------------

        private const string TextVertGlsl = @"#version 450
layout(location = 0) in vec2 inPosition;
layout(location = 1) in uvec2 inUvw;

layout(location = 0) out vec3 fsUvw;
layout(location = 1) flat out int fsBlendMode;

void main() {
    gl_Position = vec4(inPosition, 1.0, 1.0);

    uint u = inUvw.x & 0xffffffu;
    uint v = (inUvw.x >> 24) | ((inUvw.y & 0xffffu) << 8);
    uint w = (inUvw.y >> 16) & 0xfffu;
    fsUvw = vec3(float(u) / 16777216.0, float(v) / 16777216.0, float(w));
    fsBlendMode = int(inUvw.y >> 28);
}
";
        private const string TextFragGlsl = @"#version 450
layout(set = 0, binding = 0) uniform sampler2DArray FontTexture;

layout(location = 0) in vec3 fsUvw;
layout(location = 1) flat in int fsBlendMode;

layout(location = 0) out vec4 outColor;

void main() {
    if (fsBlendMode == 0) {
        vec4 s = texture(FontTexture, fsUvw);
        float a = (s.r + s.g + s.b) / 3.0;
        outColor = vec4(s.rgb * a, a);
    } else {
        outColor = vec4(0.0, 0.0, 0.0, 0.6);
    }
}
";

        private const string SpriteVertGlsl = @"#version 450
layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec4 inColor;
layout(location = 2) in uvec2 inUvw;

layout(location = 0) out vec4 fsColor;
layout(location = 1) out vec3 fsUvw;

void main() {
    gl_Position = vec4(inPosition, 1.0);

    uint u = inUvw.x & 0xffffffu;
    uint v = (inUvw.x >> 24) | ((inUvw.y & 0xffffu) << 8);
    uint w = (inUvw.y >> 16) & 0xfffu;
    fsUvw = vec3(float(u) / 16777216.0, float(v) / 16777216.0, float(w));
    fsColor = inColor;
}
";
        private const string SpriteFragGlsl = @"#version 450
layout(set = 0, binding = 0) uniform sampler2DArray SpriteTexture;

layout(location = 0) in vec4 fsColor;
layout(location = 1) in vec3 fsUvw;

layout(location = 0) out vec4 outColor;

void main() {
    vec4 s = texture(SpriteTexture, fsUvw);
    vec4 result = s * fsColor * s.a;
    result.rgb *= result.a;
    if (result.a <= 0.05)
        discard;
    outColor = result;
}
";

        public unsafe VulkanSwapChain(VulkanRenderingDevice device, Description description)
        {
            DeviceWrapper = device;
            _vk = device.Vk;
            _device = device.Device;
            _antialiasRequested = description.Antialias;
            Size = description.Size;
            RenderException = null;

            CreateSurface(description.WindowHandle);
            CreateSwapchainAndImages();
            _sampleCount = PickSampleCount(_antialiasRequested);
            CreateMsaaImage();
            CreateDepthBuffer();
            CreateRenderPass();
            CreateFramebuffers();
            CreateCommandObjects();
            CreateSyncObjects();
            CreateTextPipeline();
            CreateSpritePipeline();

            // Register so the device-wide fence-based deletion queue can
            // observe this swap chain's in-flight fence.
            DeviceWrapper.RegisterSwapChain(this);
        }

        // Choose the highest sample count up to 4x that BOTH color and depth
        // attachments support, when MSAA is requested. Pinned to 1x otherwise.
        private SampleCountFlags PickSampleCount(bool requested)
        {
            if (!requested) return SampleCountFlags.Count1Bit;
            SampleCountFlags supported = DeviceWrapper.PhysicalDeviceProperties.Limits.FramebufferColorSampleCounts
                                       & DeviceWrapper.PhysicalDeviceProperties.Limits.FramebufferDepthSampleCounts;
            if ((supported & SampleCountFlags.Count4Bit) != 0) return SampleCountFlags.Count4Bit;
            if ((supported & SampleCountFlags.Count2Bit) != 0) return SampleCountFlags.Count2Bit;
            return SampleCountFlags.Count1Bit;
        }

        // Lazy multi-sample color attachment, sized to _extent, _sampleCount
        // samples. Drawing writes here; the subpass resolve to the matching
        // swap-chain image (single-sample) happens automatically at end of
        // subpass.
        private unsafe void CreateMsaaImage()
        {
            if (_sampleCount == SampleCountFlags.Count1Bit) return;
            ImageCreateInfo info = new ImageCreateInfo
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Extent = new Extent3D(_extent.Width, _extent.Height, 1),
                MipLevels = 1, ArrayLayers = 1,
                Format = _colorFormat,
                Tiling = ImageTiling.Optimal,
                InitialLayout = ImageLayout.Undefined,
                // TransientAttachmentBit is a hint that the image's contents need
                // not be preserved beyond a render pass — the driver may keep it
                // in tile memory entirely on mobile GPUs.
                Usage = ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.TransientAttachmentBit,
                Samples = _sampleCount,
                SharingMode = SharingMode.Exclusive,
            };
            Image img;
            VkCheck.Ok(_vk.CreateImage(_device, in info, null, &img));
            _msaaImage = img;
            _vk.GetImageMemoryRequirements(_device, _msaaImage, out MemoryRequirements req);
            MemoryAllocateInfo alloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size,
                MemoryTypeIndex = DeviceWrapper.FindMemoryType(req.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
            };
            DeviceMemory mem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in alloc, null, &mem));
            _msaaMemory = mem;
            _vk.BindImageMemory(_device, _msaaImage, _msaaMemory, 0);
            ImageViewCreateInfo viewInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = _msaaImage,
                ViewType = ImageViewType.Type2D,
                Format = _colorFormat,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1),
            };
            ImageView v;
            VkCheck.Ok(_vk.CreateImageView(_device, in viewInfo, null, &v));
            _msaaView = v;
        }

        // ---- Surface --------------------------------------------------------

        private unsafe void CreateSurface(IntPtr hwnd)
        {
            // VkWin32SurfaceCreateInfoKHR needs the module HINSTANCE that
            // owns the window class. Marshal.GetHINSTANCE returns the HMODULE
            // for the calling assembly which is sufficient for any modern Win32
            // window created from the same process.
            IntPtr hinstance = Marshal.GetHINSTANCE(typeof(VulkanSwapChain).Module);
            Win32SurfaceCreateInfoKHR info = new Win32SurfaceCreateInfoKHR
            {
                SType = StructureType.Win32SurfaceCreateInfoKhr,
                Hwnd = hwnd,
                Hinstance = hinstance,
            };
            SurfaceKHR surface;
            VkCheck.Ok(DeviceWrapper.KhrWin32Surface.CreateWin32Surface(
                DeviceWrapper.Instance, in info, null, &surface));
            _surface = surface;

            // Verify the chosen queue family can actually present to this
            // specific surface (one final per-surface check on top of the
            // generic per-queue Win32 support check done at device pick).
            DeviceWrapper.KhrSurface.GetPhysicalDeviceSurfaceSupport(
                DeviceWrapper.PhysicalDevice, DeviceWrapper.GraphicsQueueFamily, _surface, out Bool32 supported);
            if (!supported)
                throw new InvalidOperationException("Graphics queue family does not support presentation to this surface.");
        }

        // ---- Swap chain + color images --------------------------------------

        private unsafe void CreateSwapchainAndImages()
        {
            // Surface capabilities — defines min/max image count, supported
            // transforms, current/desired extent.
            DeviceWrapper.KhrSurface.GetPhysicalDeviceSurfaceCapabilities(
                DeviceWrapper.PhysicalDevice, _surface, out SurfaceCapabilitiesKHR caps);

            // Pick a format. Prefer B8G8R8A8_SRGB if present (matches D3D11),
            // else the first available.
            uint formatCount = 0;
            DeviceWrapper.KhrSurface.GetPhysicalDeviceSurfaceFormats(
                DeviceWrapper.PhysicalDevice, _surface, &formatCount, null);
            var formats = new SurfaceFormatKHR[formatCount];
            fixed (SurfaceFormatKHR* p = formats)
                DeviceWrapper.KhrSurface.GetPhysicalDeviceSurfaceFormats(
                    DeviceWrapper.PhysicalDevice, _surface, &formatCount, p);
            SurfaceFormatKHR chosen = formats[0];
            foreach (var f in formats)
            {
                if (f.Format == Format.B8G8R8A8Unorm && f.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
                { chosen = f; break; }
            }
            _colorFormat = chosen.Format;

            // Pick present mode — Mailbox is low-latency tear-free; if not
            // available fall back to FIFO (always supported, vsync-on).
            uint pmCount = 0;
            DeviceWrapper.KhrSurface.GetPhysicalDeviceSurfacePresentModes(
                DeviceWrapper.PhysicalDevice, _surface, &pmCount, null);
            var modes = new PresentModeKHR[pmCount];
            fixed (PresentModeKHR* p = modes)
                DeviceWrapper.KhrSurface.GetPhysicalDeviceSurfacePresentModes(
                    DeviceWrapper.PhysicalDevice, _surface, &pmCount, p);
            PresentModeKHR mode = PresentModeKHR.FifoKhr;
            foreach (var m in modes)
                if (m == PresentModeKHR.MailboxKhr) { mode = m; break; }

            // Extent — pin to the requested swap-chain size, clamped by surface limits.
            _extent = new Extent2D(
                Math.Clamp((uint)Size.X, caps.MinImageExtent.Width, caps.MaxImageExtent.Width),
                Math.Clamp((uint)Size.Y, caps.MinImageExtent.Height, caps.MaxImageExtent.Height));

            uint imageCount = caps.MinImageCount + 1;
            if (caps.MaxImageCount > 0 && imageCount > caps.MaxImageCount)
                imageCount = caps.MaxImageCount;

            SwapchainCreateInfoKHR scInfo = new SwapchainCreateInfoKHR
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = _surface,
                MinImageCount = imageCount,
                ImageFormat = chosen.Format,
                ImageColorSpace = chosen.ColorSpace,
                ImageExtent = _extent,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ColorAttachmentBit,
                ImageSharingMode = SharingMode.Exclusive,
                PreTransform = caps.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
                PresentMode = mode,
                Clipped = true,
                OldSwapchain = default,
            };

            SwapchainKHR sc;
            VkCheck.Ok(DeviceWrapper.KhrSwapchain.CreateSwapchain(_device, in scInfo, null, &sc));
            _swapchain = sc;

            // Pull the actual image handles created by the driver.
            uint imgCount = 0;
            DeviceWrapper.KhrSwapchain.GetSwapchainImages(_device, _swapchain, &imgCount, null);
            _images = new Image[imgCount];
            fixed (Image* p = _images)
                DeviceWrapper.KhrSwapchain.GetSwapchainImages(_device, _swapchain, &imgCount, p);

            // One image view per swap image (2D, single mip/layer, color aspect).
            _imageViews = new ImageView[imgCount];
            for (int i = 0; i < imgCount; i++)
            {
                ImageViewCreateInfo ivInfo = new ImageViewCreateInfo
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = _images[i],
                    ViewType = ImageViewType.Type2D,
                    Format = _colorFormat,
                    Components = new ComponentMapping(
                        ComponentSwizzle.Identity, ComponentSwizzle.Identity,
                        ComponentSwizzle.Identity, ComponentSwizzle.Identity),
                    SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1),
                };
                ImageView view;
                VkCheck.Ok(_vk.CreateImageView(_device, in ivInfo, null, &view));
                _imageViews[i] = view;
            }
        }

        // ---- Depth buffer ---------------------------------------------------

        private unsafe void CreateDepthBuffer()
        {
            // Pick a depth format the device supports as a depth-stencil
            // attachment. D32_SFLOAT and D24_UNORM_S8_UINT are both broadly
            // supported; prefer the higher-precision one when available.
            Format[] candidates = { Format.D32SfloatS8Uint, Format.D32Sfloat, Format.D24UnormS8Uint };
            foreach (var f in candidates)
            {
                _vk.GetPhysicalDeviceFormatProperties(DeviceWrapper.PhysicalDevice, f, out FormatProperties props);
                if ((props.OptimalTilingFeatures & FormatFeatureFlags.DepthStencilAttachmentBit) != 0)
                {
                    _depthFormat = f;
                    break;
                }
            }
            if (_depthFormat == Format.Undefined)
                throw new InvalidOperationException("No supported depth format.");

            ImageCreateInfo info = new ImageCreateInfo
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Extent = new Extent3D(_extent.Width, _extent.Height, 1),
                MipLevels = 1,
                ArrayLayers = 1,
                Format = _depthFormat,
                Tiling = ImageTiling.Optimal,
                InitialLayout = ImageLayout.Undefined,
                Usage = ImageUsageFlags.DepthStencilAttachmentBit,
                Samples = _sampleCount,
                SharingMode = SharingMode.Exclusive,
            };
            Image image;
            VkCheck.Ok(_vk.CreateImage(_device, in info, null, &image));
            _depthImage = image;

            _vk.GetImageMemoryRequirements(_device, _depthImage, out MemoryRequirements req);
            MemoryAllocateInfo alloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size,
                MemoryTypeIndex = DeviceWrapper.FindMemoryType(req.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
            };
            DeviceMemory mem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in alloc, null, &mem));
            _depthMemory = mem;
            _vk.BindImageMemory(_device, _depthImage, _depthMemory, 0);

            ImageAspectFlags aspect = ImageAspectFlags.DepthBit;
            if (_depthFormat == Format.D32SfloatS8Uint || _depthFormat == Format.D24UnormS8Uint)
                aspect |= ImageAspectFlags.StencilBit;

            ImageViewCreateInfo viewInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = _depthImage,
                ViewType = ImageViewType.Type2D,
                Format = _depthFormat,
                SubresourceRange = new ImageSubresourceRange(aspect, 0, 1, 0, 1),
            };
            ImageView dv;
            VkCheck.Ok(_vk.CreateImageView(_device, in viewInfo, null, &dv));
            _depthView = dv;
        }

        // ---- Render pass ----------------------------------------------------

        private unsafe void CreateRenderPass()
        {
            bool msaa = _sampleCount != SampleCountFlags.Count1Bit;

            // No-MSAA: attachment 0 is the swap image (clear → present).
            // MSAA: attachment 0 is the MSAA color (clear → resolveSrc),
            //       attachment 2 is the swap image used as resolve target (no
            //       clear → present). The driver writes the resolved 1-sample
            //       result into 2 at end-of-subpass.
            AttachmentDescription colorAttachment = new AttachmentDescription
            {
                Format = _colorFormat,
                Samples = _sampleCount,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = msaa ? AttachmentStoreOp.DontCare : AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = msaa ? ImageLayout.ColorAttachmentOptimal : ImageLayout.PresentSrcKhr,
            };
            AttachmentDescription depthAttachment = new AttachmentDescription
            {
                Format = _depthFormat,
                Samples = _sampleCount,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.DontCare,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
            };
            AttachmentDescription resolveAttachment = new AttachmentDescription
            {
                Format = _colorFormat,
                Samples = SampleCountFlags.Count1Bit,
                LoadOp = AttachmentLoadOp.DontCare,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr,
            };

            uint attachmentCount = msaa ? 3u : 2u;
            var attachments = stackalloc AttachmentDescription[3] { colorAttachment, depthAttachment, resolveAttachment };

            AttachmentReference colorRef = new AttachmentReference(0, ImageLayout.ColorAttachmentOptimal);
            AttachmentReference depthRef = new AttachmentReference(1, ImageLayout.DepthStencilAttachmentOptimal);
            AttachmentReference resolveRef = new AttachmentReference(2, ImageLayout.ColorAttachmentOptimal);

            SubpassDescription subpass = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = &colorRef,
                PDepthStencilAttachment = &depthRef,
                PResolveAttachments = msaa ? &resolveRef : null,
            };

            SubpassDependency dep = new SubpassDependency
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
                SrcAccessMask = 0,
                DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
                DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.DepthStencilAttachmentWriteBit,
            };

            RenderPassCreateInfo rpInfo = new RenderPassCreateInfo
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = attachmentCount,
                PAttachments = attachments,
                SubpassCount = 1,
                PSubpasses = &subpass,
                DependencyCount = 1,
                PDependencies = &dep,
            };
            RenderPass rp;
            VkCheck.Ok(_vk.CreateRenderPass(_device, in rpInfo, null, &rp));
            _renderPass = rp;
        }

        // ---- Framebuffers ---------------------------------------------------

        private unsafe void CreateFramebuffers()
        {
            bool msaa = _sampleCount != SampleCountFlags.Count1Bit;
            _framebuffers = new Framebuffer[_imageViews.Length];
            for (int i = 0; i < _imageViews.Length; i++)
            {
                // No-MSAA: [swapImage, depth]. MSAA: [msaaColor, depth, swapImage].
                var atts = stackalloc ImageView[3] { msaa ? _msaaView : _imageViews[i], _depthView, _imageViews[i] };
                FramebufferCreateInfo fbInfo = new FramebufferCreateInfo
                {
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = _renderPass,
                    AttachmentCount = msaa ? 3u : 2u,
                    PAttachments = atts,
                    Width = _extent.Width,
                    Height = _extent.Height,
                    Layers = 1,
                };
                Framebuffer fb;
                VkCheck.Ok(_vk.CreateFramebuffer(_device, in fbInfo, null, &fb));
                _framebuffers[i] = fb;
            }
        }

        // ---- Command pool / buffers -----------------------------------------

        private unsafe void CreateCommandObjects()
        {
            CommandPoolCreateInfo poolInfo = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = DeviceWrapper.GraphicsQueueFamily,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
            };
            CommandPool pool;
            VkCheck.Ok(_vk.CreateCommandPool(_device, in poolInfo, null, &pool));
            _commandPool = pool;

            _commandBuffers = new CommandBuffer[FramesInFlight];
            CommandBufferAllocateInfo cbInfo = new CommandBufferAllocateInfo
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = _commandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = FramesInFlight,
            };
            fixed (CommandBuffer* p = _commandBuffers)
                VkCheck.Ok(_vk.AllocateCommandBuffers(_device, in cbInfo, p));
        }

        // ---- Per-frame sync -------------------------------------------------

        private unsafe void CreateSyncObjects()
        {
            _imageAvailable = new Semaphore[FramesInFlight];
            _renderFinished = new Semaphore[FramesInFlight];
            _inFlight = new Fence[FramesInFlight];
            SemaphoreCreateInfo semInfo = new SemaphoreCreateInfo { SType = StructureType.SemaphoreCreateInfo };
            FenceCreateInfo fenceInfo = new FenceCreateInfo
            {
                SType = StructureType.FenceCreateInfo,
                // Start signalled so the first frame's WaitForFences returns
                // immediately rather than blocking forever.
                Flags = FenceCreateFlags.SignaledBit,
            };
            for (int i = 0; i < FramesInFlight; i++)
            {
                Semaphore sIa, sRf; Fence f;
                VkCheck.Ok(_vk.CreateSemaphore(_device, in semInfo, null, &sIa));
                VkCheck.Ok(_vk.CreateSemaphore(_device, in semInfo, null, &sRf));
                VkCheck.Ok(_vk.CreateFence(_device, in fenceInfo, null, &f));
                _imageAvailable[i] = sIa;
                _renderFinished[i] = sRf;
                _inFlight[i] = f;
            }
        }

        // ---- Text pipeline creation -----------------------------------------

        private unsafe void CreateTextPipeline()
        {
            byte[] vsSpirv = DeviceWrapper.ShaderCompiler.CompileGlslToSpirv(TextVertGlsl, ShaderKind.VertexShader, "TextVS");
            byte[] fsSpirv = DeviceWrapper.ShaderCompiler.CompileGlslToSpirv(TextFragGlsl, ShaderKind.FragmentShader, "TextFS");
            _textVs = VulkanShaderCompiler.CreateShaderModule(_vk, _device, vsSpirv);
            _textFs = VulkanShaderCompiler.CreateShaderModule(_vk, _device, fsSpirv);

            // Single descriptor set: CombinedImageSampler for the font atlas.
            _textSetLayout = CreateSamplerSetLayout();

            var layouts = stackalloc DescriptorSetLayout[1] { _textSetLayout };
            PipelineLayoutCreateInfo plInfo = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 1,
                PSetLayouts = layouts,
            };
            PipelineLayout pl;
            VkCheck.Ok(_vk.CreatePipelineLayout(_device, in plInfo, null, &pl));
            _textPipelineLayout = pl;

            _textAtlasSet = AllocateDescriptorSet(_textSetLayout);

            _textPipeline = BuildTextPipeline();
        }

        private unsafe VkPipeline BuildTextPipeline()
        {
            var entryName = stackalloc byte[5] { (byte)'m', (byte)'a', (byte)'i', (byte)'n', 0 };
            var stages = stackalloc PipelineShaderStageCreateInfo[2]
            {
                new PipelineShaderStageCreateInfo
                {
                    SType = StructureType.PipelineShaderStageCreateInfo,
                    Stage = ShaderStageFlags.VertexBit,
                    Module = _textVs, PName = entryName,
                },
                new PipelineShaderStageCreateInfo
                {
                    SType = StructureType.PipelineShaderStageCreateInfo,
                    Stage = ShaderStageFlags.FragmentBit,
                    Module = _textFs, PName = entryName,
                },
            };

            // 2 SoA bindings: position float2, uvw uint2
            var bindings = stackalloc VertexInputBindingDescription[2]
            {
                new VertexInputBindingDescription { Binding = 0, Stride = (uint)sizeof(Vector2), InputRate = VertexInputRate.Vertex },
                new VertexInputBindingDescription { Binding = 1, Stride = sizeof(ulong), InputRate = VertexInputRate.Vertex },
            };
            var attributes = stackalloc VertexInputAttributeDescription[2]
            {
                new VertexInputAttributeDescription { Location = 0, Binding = 0, Format = Format.R32G32Sfloat, Offset = 0 },
                new VertexInputAttributeDescription { Location = 1, Binding = 1, Format = Format.R32G32Uint,   Offset = 0 },
            };
            PipelineVertexInputStateCreateInfo viState = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 2, PVertexBindingDescriptions = bindings,
                VertexAttributeDescriptionCount = 2, PVertexAttributeDescriptions = attributes,
            };

            PipelineInputAssemblyStateCreateInfo iaState = new PipelineInputAssemblyStateCreateInfo
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = PrimitiveTopology.TriangleList,
            };

            PipelineViewportStateCreateInfo vpState = new PipelineViewportStateCreateInfo
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1, ScissorCount = 1,
            };

            PipelineRasterizationStateCreateInfo rs = new PipelineRasterizationStateCreateInfo
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                PolygonMode = PolygonMode.Fill,
                CullMode = CullModeFlags.None,
                FrontFace = FrontFace.CounterClockwise,
                LineWidth = 1.0f,
            };

            PipelineMultisampleStateCreateInfo ms = new PipelineMultisampleStateCreateInfo
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = _sampleCount,
            };

            // Depth OFF for text overlays.
            PipelineDepthStencilStateCreateInfo ds = new PipelineDepthStencilStateCreateInfo
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = false,
                DepthWriteEnable = false,
            };

            // Premultiplied alpha: One / OneMinusSrcAlpha.
            ColorComponentFlags mask = ColorComponentFlags.RBit | ColorComponentFlags.GBit |
                                       ColorComponentFlags.BBit | ColorComponentFlags.ABit;
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
            PipelineColorBlendStateCreateInfo cbState = new PipelineColorBlendStateCreateInfo
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                AttachmentCount = 1, PAttachments = &att,
            };

            var dynStates = stackalloc DynamicState[2] { DynamicState.Viewport, DynamicState.Scissor };
            PipelineDynamicStateCreateInfo dyn = new PipelineDynamicStateCreateInfo
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 2, PDynamicStates = dynStates,
            };

            GraphicsPipelineCreateInfo gp = new GraphicsPipelineCreateInfo
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2, PStages = stages,
                PVertexInputState = &viState,
                PInputAssemblyState = &iaState,
                PViewportState = &vpState,
                PRasterizationState = &rs,
                PMultisampleState = &ms,
                PDepthStencilState = &ds,
                PColorBlendState = &cbState,
                PDynamicState = &dyn,
                Layout = _textPipelineLayout,
                RenderPass = _renderPass,
                Subpass = 0,
            };

            VkPipeline pipeline;
            VkCheck.Ok(_vk.CreateGraphicsPipelines(_device, default, 1, in gp, null, &pipeline));
            return pipeline;
        }

        // ---- Sprite pipeline creation ---------------------------------------

        private unsafe void CreateSpritePipeline()
        {
            byte[] vsSpirv = DeviceWrapper.ShaderCompiler.CompileGlslToSpirv(SpriteVertGlsl, ShaderKind.VertexShader, "SpriteVS");
            byte[] fsSpirv = DeviceWrapper.ShaderCompiler.CompileGlslToSpirv(SpriteFragGlsl, ShaderKind.FragmentShader, "SpriteFS");
            _spriteVs = VulkanShaderCompiler.CreateShaderModule(_vk, _device, vsSpirv);
            _spriteFs = VulkanShaderCompiler.CreateShaderModule(_vk, _device, fsSpirv);

            _spriteSetLayout = CreateSamplerSetLayout();

            var layouts = stackalloc DescriptorSetLayout[1] { _spriteSetLayout };
            PipelineLayoutCreateInfo plInfo = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 1,
                PSetLayouts = layouts,
            };
            PipelineLayout pl;
            VkCheck.Ok(_vk.CreatePipelineLayout(_device, in plInfo, null, &pl));
            _spritePipelineLayout = pl;

            _spriteAtlasSet = AllocateDescriptorSet(_spriteSetLayout);
        }

        private unsafe VkPipeline BuildSpritePipeline(bool noZ)
        {
            var entryName = stackalloc byte[5] { (byte)'m', (byte)'a', (byte)'i', (byte)'n', 0 };
            var stages = stackalloc PipelineShaderStageCreateInfo[2]
            {
                new PipelineShaderStageCreateInfo
                {
                    SType = StructureType.PipelineShaderStageCreateInfo,
                    Stage = ShaderStageFlags.VertexBit,
                    Module = _spriteVs, PName = entryName,
                },
                new PipelineShaderStageCreateInfo
                {
                    SType = StructureType.PipelineShaderStageCreateInfo,
                    Stage = ShaderStageFlags.FragmentBit,
                    Module = _spriteFs, PName = entryName,
                },
            };

            // 3 SoA bindings: position float3, color float4, uvw uint2
            var bindings = stackalloc VertexInputBindingDescription[3]
            {
                new VertexInputBindingDescription { Binding = 0, Stride = (uint)sizeof(Vector3), InputRate = VertexInputRate.Vertex },
                new VertexInputBindingDescription { Binding = 1, Stride = (uint)sizeof(Vector4), InputRate = VertexInputRate.Vertex },
                new VertexInputBindingDescription { Binding = 2, Stride = sizeof(ulong), InputRate = VertexInputRate.Vertex },
            };
            var attributes = stackalloc VertexInputAttributeDescription[3]
            {
                new VertexInputAttributeDescription { Location = 0, Binding = 0, Format = Format.R32G32B32Sfloat,    Offset = 0 },
                new VertexInputAttributeDescription { Location = 1, Binding = 1, Format = Format.R32G32B32A32Sfloat, Offset = 0 },
                new VertexInputAttributeDescription { Location = 2, Binding = 2, Format = Format.R32G32Uint,         Offset = 0 },
            };
            PipelineVertexInputStateCreateInfo viState = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 3, PVertexBindingDescriptions = bindings,
                VertexAttributeDescriptionCount = 3, PVertexAttributeDescriptions = attributes,
            };

            PipelineInputAssemblyStateCreateInfo iaState = new PipelineInputAssemblyStateCreateInfo
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = PrimitiveTopology.TriangleList,
            };

            PipelineViewportStateCreateInfo vpState = new PipelineViewportStateCreateInfo
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1, ScissorCount = 1,
            };

            PipelineRasterizationStateCreateInfo rs = new PipelineRasterizationStateCreateInfo
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                PolygonMode = PolygonMode.Fill,
                CullMode = CullModeFlags.None,
                FrontFace = FrontFace.CounterClockwise,
                LineWidth = 1.0f,
            };

            PipelineMultisampleStateCreateInfo ms = new PipelineMultisampleStateCreateInfo
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = _sampleCount,
            };

            PipelineDepthStencilStateCreateInfo ds = new PipelineDepthStencilStateCreateInfo
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = !noZ,
                DepthWriteEnable = !noZ,
                DepthCompareOp = CompareOp.LessOrEqual,
            };

            ColorComponentFlags mask = ColorComponentFlags.RBit | ColorComponentFlags.GBit |
                                       ColorComponentFlags.BBit | ColorComponentFlags.ABit;
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
            PipelineColorBlendStateCreateInfo cbState = new PipelineColorBlendStateCreateInfo
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                AttachmentCount = 1, PAttachments = &att,
            };

            var dynStates = stackalloc DynamicState[2] { DynamicState.Viewport, DynamicState.Scissor };
            PipelineDynamicStateCreateInfo dyn = new PipelineDynamicStateCreateInfo
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 2, PDynamicStates = dynStates,
            };

            GraphicsPipelineCreateInfo gp = new GraphicsPipelineCreateInfo
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2, PStages = stages,
                PVertexInputState = &viState,
                PInputAssemblyState = &iaState,
                PViewportState = &vpState,
                PRasterizationState = &rs,
                PMultisampleState = &ms,
                PDepthStencilState = &ds,
                PColorBlendState = &cbState,
                PDynamicState = &dyn,
                Layout = _spritePipelineLayout,
                RenderPass = _renderPass,
                Subpass = 0,
            };

            VkPipeline pipeline;
            VkCheck.Ok(_vk.CreateGraphicsPipelines(_device, default, 1, in gp, null, &pipeline));
            return pipeline;
        }

        // ---- Shared helpers -------------------------------------------------

        private unsafe DescriptorSetLayout CreateSamplerSetLayout()
        {
            DescriptorSetLayoutBinding b = new DescriptorSetLayoutBinding
            {
                Binding = 0, DescriptorCount = 1,
                DescriptorType = DescriptorType.CombinedImageSampler,
                StageFlags = ShaderStageFlags.FragmentBit,
            };
            DescriptorSetLayoutCreateInfo info = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 1, PBindings = &b,
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

        private unsafe void WriteAtlasDescriptor(DescriptorSet set, ImageView view, Sampler sampler)
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
                DstSet = set, DstBinding = 0,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.CombinedImageSampler,
                PImageInfo = &ii,
            };
            _vk.UpdateDescriptorSets(_device, 1, in w, 0, null);
        }

        private unsafe void EnsureVertexBuffer(ref VkBuffer buffer, ref DeviceMemory memory,
            ref void* mapped, ref uint capacity, uint requiredSize)
        {
            if (buffer.Handle != 0 && capacity >= requiredSize) return;

            if (buffer.Handle != 0)
            {
                // Defer via device-wide deletion queue (fence-based).
                _vk.UnmapMemory(_device, memory);
                VkBuffer capBuf = buffer;
                DeviceMemory capMem = memory;
                var vk = _vk;
                var dev = _device;
                DeviceWrapper.QueueDestroy(() =>
                {
                    if (capBuf.Handle != 0) vk.DestroyBuffer(dev, capBuf, null);
                    if (capMem.Handle != 0) vk.FreeMemory(dev, capMem, null);
                });
            }

            uint newCap = Math.Max(requiredSize, capacity * 2);
            if (newCap < 4096) newCap = 4096;
            capacity = newCap;

            BufferCreateInfo bci = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = newCap,
                Usage = BufferUsageFlags.VertexBufferBit,
                SharingMode = SharingMode.Exclusive,
            };
            VkBuffer buf;
            VkCheck.Ok(_vk.CreateBuffer(_device, in bci, null, &buf));
            buffer = buf;

            _vk.GetBufferMemoryRequirements(_device, buffer, out MemoryRequirements req);
            MemoryAllocateInfo alloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size,
                MemoryTypeIndex = DeviceWrapper.FindMemoryType(req.MemoryTypeBits,
                    MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit),
            };
            DeviceMemory mem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in alloc, null, &mem));
            memory = mem;
            _vk.BindBufferMemory(_device, buffer, memory, 0);

            void* ptr;
            VkCheck.Ok(_vk.MapMemory(_device, memory, 0, newCap, 0, &ptr));
            mapped = ptr;
        }

        private static ulong CompressUvw(VectorInt3 position, Vector2 textureScaling, Vector2 uv, uint highestBits = 0)
        {
            uint blendMode2 = Math.Min(highestBits, 15);
            uint x = (uint)((position.X + uv.X) * textureScaling.X);
            uint y = (uint)((position.Y + uv.Y) * textureScaling.Y);
            return x | ((ulong)y << 24) | ((ulong)position.Z << 48) | ((ulong)blendMode2 << 60);
        }

        // ---- Frame cycle ----------------------------------------------------

        public override unsafe void Clear(Vector4 color)
        {
            // Frame boundary — rewind the device-wide UBO ring so every
            // Drawing* call inside this Clear→Present cycle starts allocating
            // from offset 0.
            DeviceWrapper.FrameUniforms.Reset();
            // Rewind the sprite/text VB append cursors so multiple
            // RenderSprites / RenderGlyphs calls this frame are appended
            // rather than trampling offset 0.
            _spriteVBFrameCursor = 0;
            _textVBFrameCursor = 0;
            // Bump frame index — Drawing classes with their own per-frame
            // append cursor (VulkanDrawingLines) reset on FrameIndex change.
            FrameIndex++;
            // Try to drain any deferred destroyers from prior mid-frame
            // Disposes. Non-blocking — only fires if every swap chain's GPU
            // work has retired.
            DeviceWrapper.TryDrainPendingDestroys();
            _clearColorValue = new ClearValue { Color = new ClearColorValue(color.X, color.Y, color.Z, color.W) };
            EnsureRecording();
        }

        public override unsafe void ClearDepth()
        {
            // Mid-pass depth clear (used before drawing gizmos, light/camera
            // rings, the skybox tail, etc. so they sit on top of world
            // geometry). DX11's ClearDepthStencilView equivalent inside a
            // running render pass is vkCmdClearAttachments — it ignores
            // attachment indices for the depth aspect (the depth attachment
            // is implicit in the active subpass).
            if (!_recording) return;

            ClearAttachment clear = new ClearAttachment
            {
                AspectMask = ImageAspectFlags.DepthBit,
                ColorAttachment = 0, // ignored for depth aspect
                ClearValue = new ClearValue { DepthStencil = new ClearDepthStencilValue(1.0f, 0) }
            };
            ClearRect rect = new ClearRect
            {
                Rect = new Rect2D(new Offset2D(0, 0), _extent),
                BaseArrayLayer = 0,
                LayerCount = 1,
            };
            _vk.CmdClearAttachments(_commandBuffers[_slot], 1, in clear, 1, in rect);
        }

        private unsafe void EnsureRecording()
        {
            if (_recording) return;

            Fence fence = _inFlight[_slot];
            _vk.WaitForFences(_device, 1, in fence, true, ulong.MaxValue);

            // Acquire the next presentable image. If the swap chain is out of
            // date (window resized) we have to rebuild — but for this session
            // we keep it simple and trust the host control's resize handler.
            uint imageIdx;
            Result acq = DeviceWrapper.KhrSwapchain.AcquireNextImage(
                _device, _swapchain, ulong.MaxValue,
                _imageAvailable[_slot], default, &imageIdx);
            if (acq != Result.Success && acq != Result.SuboptimalKhr)
                VkCheck.Ok(acq);
            _imageIndex = imageIdx;

            _vk.ResetFences(_device, 1, in fence);

            CommandBuffer cb = _commandBuffers[_slot];
            _vk.ResetCommandBuffer(cb, 0);

            CommandBufferBeginInfo cbBegin = new CommandBufferBeginInfo
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
            };
            _vk.BeginCommandBuffer(cb, in cbBegin);

            var clears = stackalloc ClearValue[2] { _clearColorValue, _clearDepthValue };
            RenderPassBeginInfo rpBegin = new RenderPassBeginInfo
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = _renderPass,
                Framebuffer = _framebuffers[_imageIndex],
                RenderArea = new Rect2D(new Offset2D(0, 0), _extent),
                ClearValueCount = 2,
                PClearValues = clears,
            };
            _vk.CmdBeginRenderPass(cb, in rpBegin, SubpassContents.Inline);

            // Default viewport + scissor (flipped Y to match D3D11-style NDC).
            Viewport vp = new Viewport
            {
                X = 0,
                Y = _extent.Height,
                Width = _extent.Width,
                Height = -(float)_extent.Height,
                MinDepth = 0,
                MaxDepth = 1,
            };
            _vk.CmdSetViewport(cb, 0, 1, in vp);
            Rect2D scissor = new Rect2D(new Offset2D(0, 0), _extent);
            _vk.CmdSetScissor(cb, 0, 1, in scissor);

            _recording = true;
        }

        public override unsafe void Present()
        {
            if (!_recording)
            {
                // Clear was never called this frame — nothing to do.
                return;
            }

            CommandBuffer cb = _commandBuffers[_slot];
            _vk.CmdEndRenderPass(cb);
            _vk.EndCommandBuffer(cb);

            Semaphore waitSem = _imageAvailable[_slot];
            Semaphore signalSem = _renderFinished[_slot];
            PipelineStageFlags waitStage = PipelineStageFlags.ColorAttachmentOutputBit;

            SubmitInfo submit = new SubmitInfo
            {
                SType = StructureType.SubmitInfo,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = &waitSem,
                PWaitDstStageMask = &waitStage,
                CommandBufferCount = 1,
                PCommandBuffers = &cb,
                SignalSemaphoreCount = 1,
                PSignalSemaphores = &signalSem,
            };
            VkCheck.Ok(_vk.QueueSubmit(DeviceWrapper.GraphicsQueue, 1, in submit, _inFlight[_slot]));

            SwapchainKHR sc = _swapchain;
            uint idx = _imageIndex;
            PresentInfoKHR present = new PresentInfoKHR
            {
                SType = StructureType.PresentInfoKhr,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = &signalSem,
                SwapchainCount = 1,
                PSwapchains = &sc,
                PImageIndices = &idx,
            };
            Result pr = DeviceWrapper.KhrSwapchain.QueuePresent(DeviceWrapper.GraphicsQueue, in present);
            if (pr != Result.Success && pr != Result.SuboptimalKhr && pr != Result.ErrorOutOfDateKhr)
                VkCheck.Ok(pr);

            _slot = (_slot + 1) % FramesInFlight;
            _recording = false;
        }

        // ---- Resize ---------------------------------------------------------

        public override unsafe void Resize(VectorInt2 newSize)
        {
            if (newSize.X <= 0 || newSize.Y <= 0) return;
            if (newSize == Size) return;
            Size = newSize;

            // Wait for the GPU to finish any work that may reference the
            // current swap chain before tearing it down.
            _vk.DeviceWaitIdle(_device);

            DestroyFramebuffersAndDepth();
            DestroyImageViewsAndSwapchain();
            CreateSwapchainAndImages();
            CreateMsaaImage();
            CreateDepthBuffer();
            CreateFramebuffers();
        }

        // ---- Sprites / glyphs -----------------------------------------------

        public override unsafe void RenderSprites(RenderingTextureAllocator textureAllocator, bool linearFilter, bool noZ, List<Sprite> sprites)
        {
            if (sprites.Count == 0 || !_recording) return;

            var vkAllocator = (VulkanTextureAllocator)textureAllocator;
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            int vertexCount = sprites.Count * 6;
            uint posBytes = (uint)(vertexCount * sizeof(Vector3));
            uint colBytes = (uint)(vertexCount * sizeof(Vector4));
            uint uvwBytes = (uint)(vertexCount * sizeof(ulong));
            uint totalBytes = posBytes + colBytes + uvwBytes;

            // Append into the per-frame sprite VB at the current cursor. If
            // multiple RenderSprites calls happen this frame (e.g. depth-sorted
            // pass + flat HUD pass in Panel3DDraw), each gets its own slice
            // instead of trampling the previous one's data — which would draw
            // huge garbage sprites with stale positions.
            uint baseOffset = _spriteVBFrameCursor;
            EnsureVertexBuffer(ref _spriteVB, ref _spriteVBMem, ref _spriteVBMapped, ref _spriteVBCapacity, baseOffset + totalBytes);

            byte* dst = (byte*)_spriteVBMapped + baseOffset;
            Vector3* positions = (Vector3*)dst;
            Vector4* colours   = (Vector4*)(dst + posBytes);
            ulong*   uvws      = (ulong*)(dst + posBytes + colBytes);

            int count = sprites.Count;
            for (int i = 0; i < count; ++i)
            {
                Sprite sprite = sprites[i];
                VectorInt3 texPos = textureAllocator.Get(sprite.Texture);
                VectorInt2 texSize = sprite.Texture.To - sprite.Texture.From;
                float depth = sprite.Depth.HasValue ? sprite.Depth.Value : 1.0f;

                positions[i * 6 + 0] = new Vector3(sprite.Pos00.X, sprite.Pos00.Y, depth);
                positions[i * 6 + 2] = positions[i * 6 + 3] = new Vector3(sprite.Pos10.X, sprite.Pos10.Y, depth);
                positions[i * 6 + 1] = positions[i * 6 + 4] = new Vector3(sprite.Pos01.X, sprite.Pos01.Y, depth);
                positions[i * 6 + 5] = new Vector3(sprite.Pos11.X, sprite.Pos11.Y, depth);
                // 0.5px insets prevent atlas-bleeding from neighbouring entries
                uvws[i * 6 + 1] = uvws[i * 6 + 4] = CompressUvw(texPos, textureScaling, new Vector2(0.5f, 0.5f));
                uvws[i * 6 + 5] = CompressUvw(texPos, textureScaling, new Vector2(texSize.X - 0.5f, 0.5f));
                uvws[i * 6 + 0] = CompressUvw(texPos, textureScaling, new Vector2(0.5f, texSize.Y - 0.5f));
                uvws[i * 6 + 2] = uvws[i * 6 + 3] = CompressUvw(texPos, textureScaling, new Vector2(texSize.X - 0.5f, texSize.Y - 0.5f));

                for (int j = 0; j < 6; j++)
                    colours[i * 6 + j] = sprite.Tint;
            }

            // Update descriptor set if atlas view or sampler changed.
            Sampler sampler = linearFilter ? DeviceWrapper.SamplerAniso : DeviceWrapper.SamplerPoint;
            if (_spriteCachedAtlasView != vkAllocator.AtlasView.Handle || _spriteCachedLinear != linearFilter)
            {
                WriteAtlasDescriptor(_spriteAtlasSet, vkAllocator.AtlasView, sampler);
                _spriteCachedAtlasView = vkAllocator.AtlasView.Handle;
                _spriteCachedLinear = linearFilter;
            }

            // Lazy pipeline creation per noZ variant.
            if (!_spritePipelines.TryGetValue(noZ, out VkPipeline pipeline))
            {
                pipeline = BuildSpritePipeline(noZ);
                _spritePipelines[noZ] = pipeline;
            }

            CommandBuffer cb = _commandBuffers[_slot];
            _vk.CmdBindPipeline(cb, PipelineBindPoint.Graphics, pipeline);

            var bufs = stackalloc VkBuffer[3] { _spriteVB, _spriteVB, _spriteVB };
            var offs = stackalloc ulong[3] { baseOffset, baseOffset + posBytes, baseOffset + posBytes + colBytes };
            _vk.CmdBindVertexBuffers(cb, 0, 3, bufs, offs);

            _vk.CmdBindDescriptorSets(cb, PipelineBindPoint.Graphics, _spritePipelineLayout,
                0, 1, in _spriteAtlasSet, 0, null);

            _vk.CmdDraw(cb, (uint)vertexCount, 1, 0, 0);

            _spriteVBFrameCursor = baseOffset + totalBytes;
        }

        public override unsafe void RenderGlyphs(RenderingTextureAllocator textureAllocator, List<RenderingFont.GlyphRenderInfo> glyphRenderInfos, List<RectangleInt2> overlays)
        {
            int vertexCount = glyphRenderInfos.Count * 6 + overlays.Count * 6;
            if (vertexCount == 0 || !_recording) return;

            var vkAllocator = (VulkanTextureAllocator)textureAllocator;

            // Snap pixel positions to the actual half-pixel grid of the back buffer.
            Vector2 posScaling = new Vector2(1.0f) / (Size / 2);
            Vector2 posOffset = VectorInt2.FromRounded(posScaling * 0.5f);
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(textureAllocator.Size.X, textureAllocator.Size.Y);

            uint posBytes = (uint)(vertexCount * sizeof(Vector2));
            uint uvwBytes = (uint)(vertexCount * sizeof(ulong));
            uint totalBytes = posBytes + uvwBytes;

            // Append at frame cursor — same rationale as RenderSprites.
            uint baseOffset = _textVBFrameCursor;
            EnsureVertexBuffer(ref _textVB, ref _textVBMem, ref _textVBMapped, ref _textVBCapacity, baseOffset + totalBytes);

            byte* dst = (byte*)_textVBMapped + baseOffset;
            Vector2* positions = (Vector2*)dst;
            ulong* uvws = (ulong*)(dst + posBytes);

            int c = 0;

            // Background overlays: a single packed UVW value with the "solid color"
            // sentinel (highestBits = 1) is written to all 6 verts of the quad.
            ulong overlayUvw = CompressUvw(VectorInt3.Zero, Vector2.Zero, Vector2.Zero, 1);
            for (int i = 0; i < overlays.Count; ++i, ++c)
            {
                var overlay = overlays[i];
                Vector2 posStart = overlay.Start * posScaling + posOffset;
                Vector2 posEnd = (overlay.End + new Vector2(1)) * posScaling + posOffset;

                positions[c * 6 + 0] = new Vector2(posStart.X, posStart.Y);
                positions[c * 6 + 2] = positions[c * 6 + 3] = new Vector2(posEnd.X, posStart.Y);
                positions[c * 6 + 1] = positions[c * 6 + 4] = new Vector2(posStart.X, posEnd.Y);
                positions[c * 6 + 5] = new Vector2(posEnd.X, posEnd.Y);

                uvws[c * 6 + 0] = uvws[c * 6 + 1] = uvws[c * 6 + 2] =
                uvws[c * 6 + 3] = uvws[c * 6 + 4] = uvws[c * 6 + 5] = overlayUvw;
            }

            for (int i = 0; i < glyphRenderInfos.Count; ++i, ++c)
            {
                RenderingFont.GlyphRenderInfo info = glyphRenderInfos[i];
                Vector2 posStart = info.PosStart * posScaling + posOffset;
                Vector2 posEnd = (info.PosEnd - new Vector2(1)) * posScaling + posOffset;

                positions[c * 6 + 0] = new Vector2(posStart.X, posStart.Y);
                positions[c * 6 + 2] = positions[c * 6 + 3] = new Vector2(posEnd.X, posStart.Y);
                positions[c * 6 + 1] = positions[c * 6 + 4] = new Vector2(posStart.X, posEnd.Y);
                positions[c * 6 + 5] = new Vector2(posEnd.X, posEnd.Y);

                uvws[c * 6 + 0] = CompressUvw(info.TexStart, textureScaling, Vector2.Zero);
                uvws[c * 6 + 2] = uvws[c * 6 + 3] = CompressUvw(info.TexStart, textureScaling, new Vector2(info.TexSize.X - 1, 0));
                uvws[c * 6 + 1] = uvws[c * 6 + 4] = CompressUvw(info.TexStart, textureScaling, new Vector2(0, info.TexSize.Y - 1));
                uvws[c * 6 + 5] = CompressUvw(info.TexStart, textureScaling, new Vector2(info.TexSize.X - 1, info.TexSize.Y - 1));
            }

            // Update descriptor set if atlas view changed.
            if (_textCachedAtlasView != vkAllocator.AtlasView.Handle)
            {
                WriteAtlasDescriptor(_textAtlasSet, vkAllocator.AtlasView, DeviceWrapper.SamplerPoint);
                _textCachedAtlasView = vkAllocator.AtlasView.Handle;
            }

            CommandBuffer cb = _commandBuffers[_slot];
            _vk.CmdBindPipeline(cb, PipelineBindPoint.Graphics, _textPipeline);

            var bufs = stackalloc VkBuffer[2] { _textVB, _textVB };
            var offs = stackalloc ulong[2] { baseOffset, baseOffset + posBytes };
            _vk.CmdBindVertexBuffers(cb, 0, 2, bufs, offs);

            _vk.CmdBindDescriptorSets(cb, PipelineBindPoint.Graphics, _textPipelineLayout,
                0, 1, in _textAtlasSet, 0, null);

            _vk.CmdDraw(cb, (uint)vertexCount, 1, 0, 0);

            _textVBFrameCursor = baseOffset + totalBytes;
        }

        // ---- Disposal -------------------------------------------------------

        private unsafe void DestroyFramebuffersAndDepth()
        {
            if (_framebuffers != null)
            {
                foreach (var fb in _framebuffers)
                    if (fb.Handle != 0) _vk.DestroyFramebuffer(_device, fb, null);
                _framebuffers = null;
            }
            if (_msaaView.Handle != 0)   { _vk.DestroyImageView(_device, _msaaView, null);  _msaaView = default; }
            if (_msaaImage.Handle != 0)  { _vk.DestroyImage(_device, _msaaImage, null);    _msaaImage = default; }
            if (_msaaMemory.Handle != 0) { _vk.FreeMemory(_device, _msaaMemory, null);     _msaaMemory = default; }
            if (_depthView.Handle != 0) { _vk.DestroyImageView(_device, _depthView, null); _depthView = default; }
            if (_depthImage.Handle != 0) { _vk.DestroyImage(_device, _depthImage, null); _depthImage = default; }
            if (_depthMemory.Handle != 0) { _vk.FreeMemory(_device, _depthMemory, null); _depthMemory = default; }
        }

        private unsafe void DestroyImageViewsAndSwapchain()
        {
            if (_imageViews != null)
            {
                foreach (var v in _imageViews)
                    if (v.Handle != 0) _vk.DestroyImageView(_device, v, null);
                _imageViews = null;
            }
            if (_swapchain.Handle != 0)
            {
                DeviceWrapper.KhrSwapchain.DestroySwapchain(_device, _swapchain, null);
                _swapchain = default;
            }
        }

        private unsafe void DestroyTextResources()
        {
            if (_textVBMapped != null) { _vk.UnmapMemory(_device, _textVBMem); _textVBMapped = null; }
            if (_textVB.Handle != 0) { _vk.DestroyBuffer(_device, _textVB, null); _textVB = default; }
            if (_textVBMem.Handle != 0) { _vk.FreeMemory(_device, _textVBMem, null); _textVBMem = default; }
            if (_textPipeline.Handle != 0) { _vk.DestroyPipeline(_device, _textPipeline, null); _textPipeline = default; }
            if (_textPipelineLayout.Handle != 0) { _vk.DestroyPipelineLayout(_device, _textPipelineLayout, null); _textPipelineLayout = default; }
            if (_textSetLayout.Handle != 0) { _vk.DestroyDescriptorSetLayout(_device, _textSetLayout, null); _textSetLayout = default; }
            if (_textVs.Handle != 0) { _vk.DestroyShaderModule(_device, _textVs, null); _textVs = default; }
            if (_textFs.Handle != 0) { _vk.DestroyShaderModule(_device, _textFs, null); _textFs = default; }
        }

        private unsafe void DestroySpriteResources()
        {
            if (_spriteVBMapped != null) { _vk.UnmapMemory(_device, _spriteVBMem); _spriteVBMapped = null; }
            if (_spriteVB.Handle != 0) { _vk.DestroyBuffer(_device, _spriteVB, null); _spriteVB = default; }
            if (_spriteVBMem.Handle != 0) { _vk.FreeMemory(_device, _spriteVBMem, null); _spriteVBMem = default; }
            foreach (var p in _spritePipelines.Values)
                if (p.Handle != 0) _vk.DestroyPipeline(_device, p, null);
            _spritePipelines.Clear();
            if (_spritePipelineLayout.Handle != 0) { _vk.DestroyPipelineLayout(_device, _spritePipelineLayout, null); _spritePipelineLayout = default; }
            if (_spriteSetLayout.Handle != 0) { _vk.DestroyDescriptorSetLayout(_device, _spriteSetLayout, null); _spriteSetLayout = default; }
            if (_spriteVs.Handle != 0) { _vk.DestroyShaderModule(_device, _spriteVs, null); _spriteVs = default; }
            if (_spriteFs.Handle != 0) { _vk.DestroyShaderModule(_device, _spriteFs, null); _spriteFs = default; }
        }

        public override unsafe void Dispose()
        {
            _vk.DeviceWaitIdle(_device);

            // Unregister BEFORE flushing pending destroys, so TryDrain doesn't
            // observe a swap chain whose fences we're about to destroy.
            DeviceWrapper.UnregisterSwapChain(this);
            // Flush any deferred destroyers now — device is idle, safe to run
            // even if they reference resources from THIS swap chain.
            DeviceWrapper.DrainAllPendingDestroys();

            DestroyTextResources();
            DestroySpriteResources();

            if (_inFlight != null)
                foreach (var f in _inFlight)
                    if (f.Handle != 0) _vk.DestroyFence(_device, f, null);
            if (_imageAvailable != null)
                foreach (var s in _imageAvailable)
                    if (s.Handle != 0) _vk.DestroySemaphore(_device, s, null);
            if (_renderFinished != null)
                foreach (var s in _renderFinished)
                    if (s.Handle != 0) _vk.DestroySemaphore(_device, s, null);

            if (_commandPool.Handle != 0) { _vk.DestroyCommandPool(_device, _commandPool, null); _commandPool = default; }
            DestroyFramebuffersAndDepth();
            if (_renderPass.Handle != 0) { _vk.DestroyRenderPass(_device, _renderPass, null); _renderPass = default; }
            DestroyImageViewsAndSwapchain();

            if (_surface.Handle != 0)
            {
                DeviceWrapper.KhrSurface.DestroySurface(DeviceWrapper.Instance, _surface, null);
                _surface = default;
            }
        }
    }
}
