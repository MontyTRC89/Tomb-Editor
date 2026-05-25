using System;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using TombLib.Rendering.Graphics.Rhi;
using RhiFormat = TombLib.Rendering.Graphics.Rhi.Format;
using SwapchainDesc = TombLib.Rendering.Graphics.Rhi.SwapchainDesc;

namespace TombLib.Rendering.Graphics.Backends.Vulkan;

// Swapchain management for the Vulkan backend: the Win32 surface, the
// swapchain images and views, the depth attachment and (for MSAA) the
// off-screen multisampled colour target, plus the framebuffers.
public unsafe sealed partial class VkDevice
{
    public SwapchainHandle CreateSwapchain(in SwapchainDesc desc)
    {
        const int GWLP_HINSTANCE = -6;
        IntPtr hInstance = IntPtr.Size == 8
                           ? GetWindowLongPtr(desc.WindowHandle, GWLP_HINSTANCE)
                           : new IntPtr(GetWindowLong(desc.WindowHandle, GWLP_HINSTANCE));
        if (hInstance == IntPtr.Zero)
            hInstance = GetModuleHandle(null);

        DebugLog($"CreateSwapchain: hwnd=0x{desc.WindowHandle.ToInt64():X}, hinst=0x{hInstance.ToInt64():X}, " +
                 $"size={desc.Width}x{desc.Height}, samples={desc.Samples}, vsync={desc.VSync}");

        var surfaceCreateInfo = new Win32SurfaceCreateInfoKHR
        {
            SType     = StructureType.Win32SurfaceCreateInfoKhr,
            Hinstance = hInstance,
            Hwnd      = desc.WindowHandle,
        };
        var surfaceResult = KhrWin32Surface.CreateWin32Surface(Instance, in surfaceCreateInfo, null, out var surface);
        if (surfaceResult != Result.Success)
            throw new InvalidOperationException($"vkCreateWin32SurfaceKHR failed: {surfaceResult}");
        DebugLog($"  surface created: 0x{surface.Handle:X}");

        // Confirm the graphics queue can present to this surface.
        Bool32 presentSupported = false;
        KhrSurface.GetPhysicalDeviceSurfaceSupport(PhysicalDevice, GraphicsQueueFamily, surface, out presentSupported);
        if (!presentSupported)
            throw new InvalidOperationException("Selected queue family cannot present to the window's surface");

        // 4x MSAA: render into an off-screen multisampled colour + depth
        // image, then let the render pass resolve into the single-sample
        // swapchain image for presentation. Matches the DX11 backend.
        var swapchain = new VkSwapchainRes
        {
            Surface     = surface,
            ColorFormat = desc.ColorFormat,
            DepthFormat = desc.DepthFormat,
            Samples     = MsaaSamples,
            VSync       = desc.VSync,
            Hwnd        = desc.WindowHandle,
            Width       = desc.Width,
            Height      = desc.Height,
        };
        CreateSwapchainResources(swapchain);

        uint id = AllocHandle();
        Swapchains[id] = swapchain;
        return new SwapchainHandle(id);
    }

    private void CreateSwapchainResources(VkSwapchainRes swapchain)
    {
        // ---- Surface format: prefer the requested ColorFormat, else fall
        // back to the surface's first supported format. ----
        uint formatCount = 0;
        KhrSurface.GetPhysicalDeviceSurfaceFormats(PhysicalDevice, swapchain.Surface, ref formatCount, null);
        var formats = new SurfaceFormatKHR[formatCount];
        fixed (SurfaceFormatKHR* pFormats = formats)
            KhrSurface.GetPhysicalDeviceSurfaceFormats(PhysicalDevice, swapchain.Surface, ref formatCount, pFormats);

        var preferredFormat = VkMapping.ToVk(swapchain.ColorFormat);
        SurfaceFormatKHR chosenFormat = formats[0];
        for (int i = 0; i < formats.Length; i++)
            if (formats[i].Format == preferredFormat) { chosenFormat = formats[i]; break; }
        swapchain.SwapchainFormat = chosenFormat.Format;

        // ---- Surface capabilities: extent, image count, transform. ----
        KhrSurface.GetPhysicalDeviceSurfaceCapabilities(PhysicalDevice, swapchain.Surface, out var surfaceCaps);
        uint desiredImageCount = surfaceCaps.MinImageCount + 1;
        if (surfaceCaps.MaxImageCount > 0 && desiredImageCount > surfaceCaps.MaxImageCount)
            desiredImageCount = surfaceCaps.MaxImageCount;
        var extent = surfaceCaps.CurrentExtent.Width != uint.MaxValue
                     ? surfaceCaps.CurrentExtent
                     : new Extent2D(
                         (uint)Math.Clamp(swapchain.Width,  (int)surfaceCaps.MinImageExtent.Width,  (int)surfaceCaps.MaxImageExtent.Width),
                         (uint)Math.Clamp(swapchain.Height, (int)surfaceCaps.MinImageExtent.Height, (int)surfaceCaps.MaxImageExtent.Height));
        swapchain.Width  = (int)extent.Width;
        swapchain.Height = (int)extent.Height;

        // ---- Present mode: FIFO is always supported; MAILBOX / IMMEDIATE
        // (vsync off) only sometimes -- fall back to FIFO if missing so the
        // editor still runs on a tearing-conservative driver. ----
        var presentMode = PresentModeKHR.FifoKhr;
        if (!swapchain.VSync)
        {
            uint presentModeCount = 0;
            KhrSurface.GetPhysicalDeviceSurfacePresentModes(PhysicalDevice, swapchain.Surface, ref presentModeCount, null);
            var presentModes = new PresentModeKHR[presentModeCount];
            fixed (PresentModeKHR* pPresentModes = presentModes)
                KhrSurface.GetPhysicalDeviceSurfacePresentModes(PhysicalDevice, swapchain.Surface, ref presentModeCount, pPresentModes);

            for (int i = 0; i < presentModes.Length; i++)
                if (presentModes[i] == PresentModeKHR.MailboxKhr) { presentMode = PresentModeKHR.MailboxKhr; break; }
            if (presentMode == PresentModeKHR.FifoKhr)
                for (int i = 0; i < presentModes.Length; i++)
                    if (presentModes[i] == PresentModeKHR.ImmediateKhr) { presentMode = PresentModeKHR.ImmediateKhr; break; }
        }

        // Image usage: ColorAttachmentBit is always allowed; TransferDstBit is
        // only allowed if the surface advertises it (most do, but not all).
        var imageUsage = ImageUsageFlags.ColorAttachmentBit;
        if ((surfaceCaps.SupportedUsageFlags & ImageUsageFlags.TransferDstBit) != 0)
            imageUsage |= ImageUsageFlags.TransferDstBit;

        // Composite alpha: Opaque is the natural choice, but some compositors
        // only offer Inherit / PreMultiplied / PostMultiplied.
        var compositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr;
        if ((surfaceCaps.SupportedCompositeAlpha & CompositeAlphaFlagsKHR.OpaqueBitKhr) == 0)
        {
            if ((surfaceCaps.SupportedCompositeAlpha & CompositeAlphaFlagsKHR.InheritBitKhr) != 0)
                compositeAlpha = CompositeAlphaFlagsKHR.InheritBitKhr;
            else if ((surfaceCaps.SupportedCompositeAlpha & CompositeAlphaFlagsKHR.PreMultipliedBitKhr) != 0)
                compositeAlpha = CompositeAlphaFlagsKHR.PreMultipliedBitKhr;
            else if ((surfaceCaps.SupportedCompositeAlpha & CompositeAlphaFlagsKHR.PostMultipliedBitKhr) != 0)
                compositeAlpha = CompositeAlphaFlagsKHR.PostMultipliedBitKhr;
        }

        // CRITICAL: pass the existing swapchain (if any) as OldSwapchain.
        // Without this, Vulkan keeps the previous swapchain alive -- the surface
        // is "in use" by it, and the second CreateSwapchain on the same surface
        // fails with ErrorNativeWindowInUseKhr. The old one is destroyed only
        // *after* the new one succeeds, so the driver can recycle resources.
        var oldSwapchain = swapchain.SwapchainHandle;

        var swapchainCreateInfo = new SwapchainCreateInfoKHR
        {
            SType            = StructureType.SwapchainCreateInfoKhr,
            Surface          = swapchain.Surface,
            MinImageCount    = desiredImageCount,
            ImageFormat      = chosenFormat.Format,
            ImageColorSpace  = chosenFormat.ColorSpace,
            ImageExtent      = extent,
            ImageArrayLayers = 1,
            ImageUsage       = imageUsage,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform     = (surfaceCaps.SupportedTransforms & SurfaceTransformFlagsKHR.IdentityBitKhr) != 0
                               ? SurfaceTransformFlagsKHR.IdentityBitKhr
                               : surfaceCaps.CurrentTransform,
            CompositeAlpha   = compositeAlpha,
            PresentMode      = presentMode,
            Clipped          = true,
            OldSwapchain     = oldSwapchain,
        };
        DebugLog($"  attempting swapchain: minImg={desiredImageCount}, format={chosenFormat.Format}/{chosenFormat.ColorSpace}, " +
                 $"extent={extent.Width}x{extent.Height}, usage={imageUsage}, composite={compositeAlpha}, present={presentMode}");

        var swapchainResult = KhrSwapchain.CreateSwapchain(Device, in swapchainCreateInfo, null, out swapchain.SwapchainHandle);
        if (swapchainResult != Result.Success)
        {
            DebugLog($"  swapchain failed: {swapchainResult}");
            DebugLog($"  surface caps: Min/Max={surfaceCaps.MinImageCount}/{surfaceCaps.MaxImageCount}, " +
                     $"SupportedUsage={surfaceCaps.SupportedUsageFlags}, SupportedComposite={surfaceCaps.SupportedCompositeAlpha}, " +
                     $"CurrentExtent={surfaceCaps.CurrentExtent.Width}x{surfaceCaps.CurrentExtent.Height}, " +
                     $"SupportedTransforms={surfaceCaps.SupportedTransforms}, CurrentTransform={surfaceCaps.CurrentTransform}");
            throw new InvalidOperationException(
                $"vkCreateSwapchainKHR failed: {swapchainResult}. " +
                $"HWND=0x{swapchain.Hwnd.ToInt64():X}, Format={chosenFormat.Format}, ColorSpace={chosenFormat.ColorSpace}, Extent={extent.Width}x{extent.Height}, " +
                $"MinImageCount={desiredImageCount}, ImageUsage={imageUsage}, CompositeAlpha={compositeAlpha}, PresentMode={presentMode}. " +
                $"Surface caps: Min/Max={surfaceCaps.MinImageCount}/{surfaceCaps.MaxImageCount}, " +
                $"SupportedUsage={surfaceCaps.SupportedUsageFlags}, SupportedComposite={surfaceCaps.SupportedCompositeAlpha}. " +
                $"Full negotiation log at: {System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TombEditorVk.log")}");
        }
        DebugLog($"  swapchain OK: handle=0x{swapchain.SwapchainHandle.Handle:X}");

        // Now safe to destroy the previous swapchain -- the driver has migrated
        // its surface ownership to the new one.
        if (oldSwapchain.Handle != 0)
        {
            KhrSwapchain.DestroySwapchain(Device, oldSwapchain, null);
            DebugLog($"  retired old swapchain: 0x{oldSwapchain.Handle:X}");
        }

        // ---- Swapchain colour images + views. ----
        uint imageCount = 0;
        KhrSwapchain.GetSwapchainImages(Device, swapchain.SwapchainHandle, ref imageCount, null);
        swapchain.ColorImages  = new Image[imageCount];
        swapchain.ColorViews   = new ImageView[imageCount];
        swapchain.ImageLayouts = new ImageLayout[imageCount];
        fixed (Image* pImages = swapchain.ColorImages)
            KhrSwapchain.GetSwapchainImages(Device, swapchain.SwapchainHandle, ref imageCount, pImages);

        for (int i = 0; i < imageCount; i++)
        {
            var colorViewInfo = new ImageViewCreateInfo
            {
                SType    = StructureType.ImageViewCreateInfo,
                Image    = swapchain.ColorImages[i],
                ViewType = ImageViewType.Type2D,
                Format   = chosenFormat.Format,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask     = ImageAspectFlags.ColorBit,
                    BaseMipLevel   = 0, LevelCount = 1,
                    BaseArrayLayer = 0, LayerCount = 1,
                },
            };
            Api.CreateImageView(Device, in colorViewInfo, null, out swapchain.ColorViews[i]);
            swapchain.ImageLayouts[i] = ImageLayout.Undefined;
        }

        // ---- Depth image + view (multisampled when Samples > 1). ----
        var depthFormatVk = VkMapping.ToVk(swapchain.DepthFormat);
        swapchain.DepthFormatVk = depthFormatVk;
        var depthImageInfo = new ImageCreateInfo
        {
            SType         = StructureType.ImageCreateInfo,
            ImageType     = ImageType.Type2D,
            Format        = depthFormatVk,
            Extent        = new Extent3D(extent.Width, extent.Height, 1),
            MipLevels     = 1,
            ArrayLayers   = 1,
            Samples       = VkMapping.ToSampleCount(swapchain.Samples),
            Tiling        = ImageTiling.Optimal,
            Usage         = ImageUsageFlags.DepthStencilAttachmentBit,
            SharingMode   = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Undefined,
        };
        Api.CreateImage(Device, in depthImageInfo, null, out swapchain.DepthImage);
        Api.GetImageMemoryRequirements(Device, swapchain.DepthImage, out var depthMemReq);
        swapchain.DepthMemory = AllocMemory(depthMemReq.Size, depthMemReq.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit);
        Api.BindImageMemory(Device, swapchain.DepthImage, swapchain.DepthMemory, 0);

        var depthViewInfo = new ImageViewCreateInfo
        {
            SType    = StructureType.ImageViewCreateInfo,
            Image    = swapchain.DepthImage,
            ViewType = ImageViewType.Type2D,
            Format   = depthFormatVk,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.DepthBit |
                             (swapchain.DepthFormat == RhiFormat.D24_UNorm_S8_UInt ? ImageAspectFlags.StencilBit : 0),
                BaseMipLevel   = 0, LevelCount = 1,
                BaseArrayLayer = 0, LayerCount = 1,
            },
        };
        Api.CreateImageView(Device, in depthViewInfo, null, out swapchain.DepthView);

        // ---- Off-screen multisampled colour target. Rendering is
        // multisampled into this image; the render pass resolves it into the
        // single-sample swapchain image at the end of the pass. ----
        if (swapchain.Samples > 1)
        {
            var msaaImageInfo = new ImageCreateInfo
            {
                SType         = StructureType.ImageCreateInfo,
                ImageType     = ImageType.Type2D,
                Format        = chosenFormat.Format,
                Extent        = new Extent3D(extent.Width, extent.Height, 1),
                MipLevels     = 1,
                ArrayLayers   = 1,
                Samples       = VkMapping.ToSampleCount(swapchain.Samples),
                Tiling        = ImageTiling.Optimal,
                Usage         = ImageUsageFlags.ColorAttachmentBit,
                SharingMode   = SharingMode.Exclusive,
                InitialLayout = ImageLayout.Undefined,
            };
            Api.CreateImage(Device, in msaaImageInfo, null, out swapchain.MsaaColorImage);
            Api.GetImageMemoryRequirements(Device, swapchain.MsaaColorImage, out var msaaMemReq);
            swapchain.MsaaColorMemory = AllocMemory(msaaMemReq.Size, msaaMemReq.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit);
            Api.BindImageMemory(Device, swapchain.MsaaColorImage, swapchain.MsaaColorMemory, 0);

            var msaaViewInfo = new ImageViewCreateInfo
            {
                SType    = StructureType.ImageViewCreateInfo,
                Image    = swapchain.MsaaColorImage,
                ViewType = ImageViewType.Type2D,
                Format   = chosenFormat.Format,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask     = ImageAspectFlags.ColorBit,
                    BaseMipLevel   = 0, LevelCount = 1,
                    BaseArrayLayer = 0, LayerCount = 1,
                },
            };
            Api.CreateImageView(Device, in msaaViewInfo, null, out swapchain.MsaaColorView);
        }

        // ---- Render pass + framebuffers. With resolveToSwapchain=true and
        // Samples > 1 the pass gains a resolve attachment (final layout
        // PRESENT_SRC_KHR); at Samples == 1 the colour attachment is the
        // present target itself. ----
        swapchain.RenderPass = GetOrCreateRenderPass(chosenFormat.Format, depthFormatVk, swapchain.Samples, resolveToSwapchain: true);

        swapchain.Framebuffers = new Framebuffer[imageCount];
        for (int i = 0; i < imageCount; i++)
        {
            // Attachment order must match GetOrCreateRenderPass:
            //   MSAA -> [0] multisampled colour, [1] depth, [2] resolve (swapchain image)
            //   1x   -> [0] colour (swapchain image), [1] depth
            var attachments = stackalloc ImageView[3];
            uint attachmentCount;
            if (swapchain.Samples > 1)
            {
                attachments[0]  = swapchain.MsaaColorView;
                attachments[1]  = swapchain.DepthView;
                attachments[2]  = swapchain.ColorViews[i];
                attachmentCount = 3;
            }
            else
            {
                attachments[0]  = swapchain.ColorViews[i];
                attachments[1]  = swapchain.DepthView;
                attachmentCount = 2;
            }
            var framebufferInfo = new FramebufferCreateInfo
            {
                SType           = StructureType.FramebufferCreateInfo,
                RenderPass      = swapchain.RenderPass,
                AttachmentCount = attachmentCount,
                PAttachments    = attachments,
                Width           = extent.Width,
                Height          = extent.Height,
                Layers          = 1,
            };
            Api.CreateFramebuffer(Device, in framebufferInfo, null, out swapchain.Framebuffers[i]);
        }

        // One "render finished" semaphore per swapchain image (see
        // VkSwapchainRes). Recreated alongside the images on every resize.
        swapchain.RenderFinishedSemaphores = new Semaphore[imageCount];
        var semaphoreInfo = new SemaphoreCreateInfo { SType = StructureType.SemaphoreCreateInfo };
        for (int i = 0; i < imageCount; i++)
            Api.CreateSemaphore(Device, in semaphoreInfo, null, out swapchain.RenderFinishedSemaphores[i]);
    }

    public void ResizeSwapchain(SwapchainHandle handle, int width, int height)
    {
        if (_disposed || !Swapchains.TryGetValue(handle.Id, out var swapchain)) return;
        WaitIdle();
        DestroySwapchainViews(swapchain);
        swapchain.Width  = width;
        swapchain.Height = height;
        CreateSwapchainResources(swapchain);
    }

    public void Destroy(SwapchainHandle handle)
    {
        if (Swapchains.Remove(handle.Id, out var swapchain))
            DestroySwapchainInternal(swapchain);
    }

    internal void DestroySwapchainInternal(VkSwapchainRes swapchain)
    {
        WaitIdle();
        DestroySwapchainViews(swapchain);
        if (swapchain.SwapchainHandle.Handle != 0)
            KhrSwapchain.DestroySwapchain(Device, swapchain.SwapchainHandle, null);
        if (swapchain.Surface.Handle != 0)
            KhrSurface.DestroySurface(Instance, swapchain.Surface, null);
    }

    // Destroys every per-image / per-extent object so the swapchain can be
    // recreated at a new size (or fully torn down).
    private void DestroySwapchainViews(VkSwapchainRes swapchain)
    {
        if (swapchain.RenderFinishedSemaphores != null)
            foreach (var semaphore in swapchain.RenderFinishedSemaphores)
                if (semaphore.Handle != 0) Api.DestroySemaphore(Device, semaphore, null);

        if (swapchain.Framebuffers != null)
            foreach (var framebuffer in swapchain.Framebuffers)
                if (framebuffer.Handle != 0) Api.DestroyFramebuffer(Device, framebuffer, null);

        if (swapchain.DepthView.Handle   != 0) Api.DestroyImageView(Device, swapchain.DepthView,   null);
        if (swapchain.DepthImage.Handle  != 0) Api.DestroyImage(Device, swapchain.DepthImage,      null);
        if (swapchain.DepthMemory.Handle != 0) Api.FreeMemory(Device, swapchain.DepthMemory,       null);

        if (swapchain.MsaaColorView.Handle   != 0) Api.DestroyImageView(Device, swapchain.MsaaColorView,  null);
        if (swapchain.MsaaColorImage.Handle  != 0) Api.DestroyImage(Device, swapchain.MsaaColorImage,     null);
        if (swapchain.MsaaColorMemory.Handle != 0) Api.FreeMemory(Device, swapchain.MsaaColorMemory,      null);

        if (swapchain.ColorViews != null)
            foreach (var colorView in swapchain.ColorViews)
                if (colorView.Handle != 0) Api.DestroyImageView(Device, colorView, null);
    }

    // ----------------------------------------------------------- Win32 interop

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hwnd, int nIndex);

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong(IntPtr hwnd, int nIndex);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW",
        CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    // Diagnostic log into %TEMP%\TombEditorVk.log so surface + swapchain
    // negotiation is captured regardless of whether stderr is attached.
    private static readonly object _logLock = new();

    internal static void DebugLog(string message)
    {
        try
        {
            lock (_logLock)
            {
                var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TombEditorVk.log");
                System.IO.File.AppendAllText(path, $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n");
            }
        }
        catch { /* never fail because of logging */ }
    }
}
