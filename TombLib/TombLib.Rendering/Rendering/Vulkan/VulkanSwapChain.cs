using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using TombLib.Utils;

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
    //
    // Sprite/glyph overlays are NOT implemented yet — RenderSprites/RenderGlyphs
    // throw. The session goal is "Clear -> Present cycle works under raw Vulkan."
    public sealed class VulkanSwapChain : RenderingSwapChain
    {
        private const int FramesInFlight = 2;

        public readonly VulkanRenderingDevice DeviceWrapper;
        private readonly Vk _vk;
        private readonly Device _device;

        private SurfaceKHR _surface;
        private SwapchainKHR _swapchain;
        private Format _colorFormat;
        private Format _depthFormat;
        private Extent2D _extent;
        private Image[] _images;
        private ImageView[] _imageViews;

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

        public RenderPass RenderPass => _renderPass;
        public Format ColorFormat => _colorFormat;
        public Format DepthFormat => _depthFormat;
        public Extent2D Extent => _extent;

        // Current frame's command buffer — valid between Clear() and Present()
        // (i.e. inside the render-pass recording window). Drawing* classes call
        // EnsureRecording first, then read this to record their cmd... calls.
        public CommandBuffer CurrentCommandBuffer => _commandBuffers[_slot];

        public unsafe VulkanSwapChain(VulkanRenderingDevice device, Description description)
        {
            DeviceWrapper = device;
            _vk = device.Vk;
            _device = device.Device;
            Size = description.Size;
            RenderException = null;

            CreateSurface(description.WindowHandle);
            CreateSwapchainAndImages();
            CreateDepthBuffer();
            CreateRenderPass();
            CreateFramebuffers();
            CreateCommandObjects();
            CreateSyncObjects();
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
                Samples = SampleCountFlags.Count1Bit,
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
            // Single subpass with two attachments:
            //   0: color (load=clear, store=store, undefined → present)
            //   1: depth (load=clear, store=dontcare, undefined → depth_stencil)
            AttachmentDescription colorAttachment = new AttachmentDescription
            {
                Format = _colorFormat,
                Samples = SampleCountFlags.Count1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr,
            };
            AttachmentDescription depthAttachment = new AttachmentDescription
            {
                Format = _depthFormat,
                Samples = SampleCountFlags.Count1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.DontCare,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
            };
            var attachments = stackalloc AttachmentDescription[2] { colorAttachment, depthAttachment };

            AttachmentReference colorRef = new AttachmentReference(0, ImageLayout.ColorAttachmentOptimal);
            AttachmentReference depthRef = new AttachmentReference(1, ImageLayout.DepthStencilAttachmentOptimal);

            SubpassDescription subpass = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = &colorRef,
                PDepthStencilAttachment = &depthRef,
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
                AttachmentCount = 2,
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
            _framebuffers = new Framebuffer[_imageViews.Length];
            for (int i = 0; i < _imageViews.Length; i++)
            {
                var atts = stackalloc ImageView[2] { _imageViews[i], _depthView };
                FramebufferCreateInfo fbInfo = new FramebufferCreateInfo
                {
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = _renderPass,
                    AttachmentCount = 2,
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

        // ---- Frame cycle ----------------------------------------------------

        public override unsafe void Clear(Vector4 color)
        {
            // Frame boundary — rewind the device-wide UBO ring so every
            // Drawing* call inside this Clear→Present cycle starts allocating
            // from offset 0.
            DeviceWrapper.FrameUniforms.Reset();
            _clearColorValue = new ClearValue { Color = new ClearColorValue(color.X, color.Y, color.Z, color.W) };
            EnsureRecording();
        }

        public override void ClearDepth()
        {
            // The single-subpass render pass already issues a depth clear via
            // LoadOp.Clear at BeginRenderPass time, so per-frame ClearDepth()
            // after Clear() is a no-op for this implementation. Caller-explicit
            // mid-pass depth clears (used in DrawSkybox) need a vkCmdClearAttachments
            // — left as a TODO until skybox is ported.
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
            CreateDepthBuffer();
            CreateFramebuffers();
        }

        // ---- Sprites / glyphs (no-op stubs) ---------------------------------
        // In-viewport sprite icons (entity markers) and 3D text labels haven't
        // been ported to direct Vulkan yet. They are non-critical UI overlays —
        // the 3D scene renders correctly without them. Returning a no-op here
        // instead of throwing keeps the editor functional; callers (Panel3D
        // overlay paths) simply produce no output until a sprite/glyph pipeline
        // is added.
        public override void RenderSprites(RenderingTextureAllocator textureAllocator, bool linearFilter, bool noZ, List<Sprite> sprites) { }
        public override void RenderGlyphs(RenderingTextureAllocator textureAllocator, List<RenderingFont.GlyphRenderInfo> glyphRenderInfos, List<RectangleInt2> overlays) { }

        // ---- Disposal -------------------------------------------------------

        private unsafe void DestroyFramebuffersAndDepth()
        {
            if (_framebuffers != null)
            {
                foreach (var fb in _framebuffers)
                    if (fb.Handle != 0) _vk.DestroyFramebuffer(_device, fb, null);
                _framebuffers = null;
            }
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

        public override unsafe void Dispose()
        {
            _vk.DeviceWaitIdle(_device);

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
