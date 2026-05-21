using System;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using TombLib.RenderingV2.Rhi;
using VkFormat = Silk.NET.Vulkan.Format;
using RhiFormat = TombLib.RenderingV2.Rhi.Format;
using SwapchainDesc = TombLib.RenderingV2.Rhi.SwapchainDesc;

namespace TombLib.RenderingV2.Backends.Vulkan;

public unsafe sealed partial class VkDevice
{
    public SwapchainHandle CreateSwapchain(in SwapchainDesc desc)
    {
        const int GWLP_HINSTANCE = -6;
        IntPtr hinst = IntPtr.Size == 8
                       ? GetWindowLongPtr(desc.WindowHandle, GWLP_HINSTANCE)
                       : new IntPtr(GetWindowLong(desc.WindowHandle, GWLP_HINSTANCE));
        if (hinst == IntPtr.Zero)
            hinst = GetModuleHandle(null);

        DebugLog($"CreateSwapchain: hwnd=0x{desc.WindowHandle.ToInt64():X}, hinst=0x{hinst.ToInt64():X}, " +
                 $"size={desc.Width}x{desc.Height}, samples={desc.Samples}, vsync={desc.VSync}");

        var surfCi = new Win32SurfaceCreateInfoKHR
        {
            SType     = StructureType.Win32SurfaceCreateInfoKhr,
            Hinstance = hinst,
            Hwnd      = desc.WindowHandle,
        };
        var surfRes = KhrWin32Surface.CreateWin32Surface(Instance, in surfCi, null, out var surface);
        if (surfRes != Result.Success)
            throw new InvalidOperationException($"vkCreateWin32SurfaceKHR failed: {surfRes}");
        DebugLog($"  surface created: 0x{surface.Handle:X}");

        // Confirm the graphics queue can present to this surface.
        Bool32 supported = false;
        KhrSurface.GetPhysicalDeviceSurfaceSupport(PhysicalDevice, GraphicsQueueFamily, surface, out supported);
        if (!supported)
            throw new InvalidOperationException("Selected queue family cannot present to the window's surface");

        // For the first cut we hard-set samples=1 (no MSAA). The viewport
        // looks a touch jaggier than the DX11 path but every Vulkan
        // resolve/MSAA attachment complication goes away.
        var res = new VkSwapchainRes
        {
            Surface     = surface,
            ColorFormat = desc.ColorFormat,
            DepthFormat = desc.DepthFormat,
            Samples     = 1,
            VSync       = desc.VSync,
            Hwnd        = desc.WindowHandle,
            Width       = desc.Width,
            Height      = desc.Height,
        };
        CreateSwapchainResources(res);

        uint id = AllocHandle();
        Swapchains[id] = res;
        return new SwapchainHandle(id);
    }

    private void CreateSwapchainResources(VkSwapchainRes sc)
    {
        // Pick a surface format. Prefer the requested ColorFormat;
        // fall back to the first supported BGRA8.
        uint fmtCount = 0;
        KhrSurface.GetPhysicalDeviceSurfaceFormats(PhysicalDevice, sc.Surface, ref fmtCount, null);
        var formats = new SurfaceFormatKHR[fmtCount];
        fixed (SurfaceFormatKHR* p = formats)
            KhrSurface.GetPhysicalDeviceSurfaceFormats(PhysicalDevice, sc.Surface, ref fmtCount, p);
        var wantVk = VkMapping.ToVk(sc.ColorFormat);
        SurfaceFormatKHR chosen = formats[0];
        for (int i = 0; i < formats.Length; i++)
            if (formats[i].Format == wantVk) { chosen = formats[i]; break; }
        sc.SwapchainFormat = chosen.Format;

        // Capabilities (extent, image count, transform).
        KhrSurface.GetPhysicalDeviceSurfaceCapabilities(PhysicalDevice, sc.Surface, out var caps);
        uint imageCount = caps.MinImageCount + 1;
        if (caps.MaxImageCount > 0 && imageCount > caps.MaxImageCount) imageCount = caps.MaxImageCount;
        var extent = caps.CurrentExtent.Width != uint.MaxValue
                     ? caps.CurrentExtent
                     : new Extent2D((uint)Math.Clamp(sc.Width,  (int)caps.MinImageExtent.Width,  (int)caps.MaxImageExtent.Width),
                                    (uint)Math.Clamp(sc.Height, (int)caps.MinImageExtent.Height, (int)caps.MaxImageExtent.Height));
        sc.Width  = (int)extent.Width;
        sc.Height = (int)extent.Height;

        // Present-mode negotiation. FIFO is always supported; IMMEDIATE
        // (vsync off) only sometimes — fall back to FIFO if missing so the
        // editor still runs on a tearing-conservative driver.
        var present = PresentModeKHR.FifoKhr; // safe default
        if (!sc.VSync)
        {
            uint pmCount = 0;
            KhrSurface.GetPhysicalDeviceSurfacePresentModes(PhysicalDevice, sc.Surface, ref pmCount, null);
            var modes = new PresentModeKHR[pmCount];
            fixed (PresentModeKHR* pp = modes)
                KhrSurface.GetPhysicalDeviceSurfacePresentModes(PhysicalDevice, sc.Surface, ref pmCount, pp);
            for (int i = 0; i < modes.Length; i++)
                if (modes[i] == PresentModeKHR.MailboxKhr) { present = PresentModeKHR.MailboxKhr; break; }
            if (present == PresentModeKHR.FifoKhr)
                for (int i = 0; i < modes.Length; i++)
                    if (modes[i] == PresentModeKHR.ImmediateKhr) { present = PresentModeKHR.ImmediateKhr; break; }
        }

        // Image usage: ColorAttachmentBit is always allowed; TransferDstBit is
        // only allowed if the surface advertises it (most do, but not all).
        var imgUsage = ImageUsageFlags.ColorAttachmentBit;
        if ((caps.SupportedUsageFlags & ImageUsageFlags.TransferDstBit) != 0)
            imgUsage |= ImageUsageFlags.TransferDstBit;

        // CompositeAlpha negotiation: pick the first supported mode. Opaque
        // is the natural choice but some compositors only offer Inherit /
        // PreMultiplied.
        var compositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr;
        if ((caps.SupportedCompositeAlpha & CompositeAlphaFlagsKHR.OpaqueBitKhr) == 0)
        {
            if ((caps.SupportedCompositeAlpha & CompositeAlphaFlagsKHR.InheritBitKhr) != 0)
                compositeAlpha = CompositeAlphaFlagsKHR.InheritBitKhr;
            else if ((caps.SupportedCompositeAlpha & CompositeAlphaFlagsKHR.PreMultipliedBitKhr) != 0)
                compositeAlpha = CompositeAlphaFlagsKHR.PreMultipliedBitKhr;
            else if ((caps.SupportedCompositeAlpha & CompositeAlphaFlagsKHR.PostMultipliedBitKhr) != 0)
                compositeAlpha = CompositeAlphaFlagsKHR.PostMultipliedBitKhr;
        }

        // CRITICAL: pass the existing swapchain (if any) as OldSwapchain.
        // Without this, Vulkan keeps the previous swapchain alive — the
        // surface is "in use" by it, and the second CreateSwapchain on the
        // same surface fails with ErrorNativeWindowInUseKhr. We destroy
        // the old one only *after* the new one has been successfully
        // created (so the driver can recycle resources).
        var oldSwapchain = sc.SwapchainHandle;

        var sci = new SwapchainCreateInfoKHR
        {
            SType            = StructureType.SwapchainCreateInfoKhr,
            Surface          = sc.Surface,
            MinImageCount    = imageCount,
            ImageFormat      = chosen.Format,
            ImageColorSpace  = chosen.ColorSpace,
            ImageExtent      = extent,
            ImageArrayLayers = 1,
            ImageUsage       = imgUsage,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform     = (caps.SupportedTransforms & SurfaceTransformFlagsKHR.IdentityBitKhr) != 0
                               ? SurfaceTransformFlagsKHR.IdentityBitKhr
                               : caps.CurrentTransform,
            CompositeAlpha   = compositeAlpha,
            PresentMode      = present,
            Clipped          = true,
            OldSwapchain     = oldSwapchain,
        };
        DebugLog($"  attempting swapchain: minImg={imageCount}, format={chosen.Format}/{chosen.ColorSpace}, " +
                 $"extent={extent.Width}x{extent.Height}, usage={imgUsage}, composite={compositeAlpha}, present={present}");
        var scRes = KhrSwapchain.CreateSwapchain(Device, in sci, null, out sc.SwapchainHandle);
        if (scRes != Result.Success)
        {
            DebugLog($"  swapchain failed: {scRes}");
            DebugLog($"  surface caps: Min/Max={caps.MinImageCount}/{caps.MaxImageCount}, " +
                     $"SupportedUsage={caps.SupportedUsageFlags}, SupportedComposite={caps.SupportedCompositeAlpha}, " +
                     $"CurrentExtent={caps.CurrentExtent.Width}x{caps.CurrentExtent.Height}, " +
                     $"SupportedTransforms={caps.SupportedTransforms}, CurrentTransform={caps.CurrentTransform}");
            throw new InvalidOperationException(
                $"vkCreateSwapchainKHR failed: {scRes}. " +
                $"HWND=0x{sc.Hwnd.ToInt64():X}, Format={chosen.Format}, ColorSpace={chosen.ColorSpace}, Extent={extent.Width}x{extent.Height}, " +
                $"MinImageCount={imageCount}, ImageUsage={imgUsage}, CompositeAlpha={compositeAlpha}, PresentMode={present}. " +
                $"Surface caps: Min/Max={caps.MinImageCount}/{caps.MaxImageCount}, " +
                $"SupportedUsage={caps.SupportedUsageFlags}, SupportedComposite={caps.SupportedCompositeAlpha}. " +
                $"Full negotiation log at: {System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TombEditorVk.log")}");
        }
        DebugLog($"  swapchain OK: handle=0x{sc.SwapchainHandle.Handle:X}");

        // Now safe to destroy the previous swapchain — the driver has
        // migrated its surface ownership to the new one.
        if (oldSwapchain.Handle != 0)
        {
            KhrSwapchain.DestroySwapchain(Device, oldSwapchain, null);
            DebugLog($"  retired old swapchain: 0x{oldSwapchain.Handle:X}");
        }

        uint n = 0;
        KhrSwapchain.GetSwapchainImages(Device, sc.SwapchainHandle, ref n, null);
        sc.ColorImages = new Image[n];
        sc.ColorViews  = new ImageView[n];
        sc.ImageLayouts = new ImageLayout[n];
        fixed (Image* p = sc.ColorImages)
            KhrSwapchain.GetSwapchainImages(Device, sc.SwapchainHandle, ref n, p);
        for (int i = 0; i < n; i++)
        {
            var ivci = new ImageViewCreateInfo
            {
                SType    = StructureType.ImageViewCreateInfo,
                Image    = sc.ColorImages[i],
                ViewType = ImageViewType.Type2D,
                Format   = chosen.Format,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0, LevelCount = 1,
                    BaseArrayLayer = 0, LayerCount = 1,
                },
            };
            Api.CreateImageView(Device, in ivci, null, out sc.ColorViews[i]);
            sc.ImageLayouts[i] = ImageLayout.Undefined;
        }

        // Depth image.
        var depthVk = VkMapping.ToVk(sc.DepthFormat);
        sc.DepthFormatVk = depthVk;
        var dci = new ImageCreateInfo
        {
            SType         = StructureType.ImageCreateInfo,
            ImageType     = ImageType.Type2D,
            Format        = depthVk,
            Extent        = new Extent3D(extent.Width, extent.Height, 1),
            MipLevels     = 1,
            ArrayLayers   = 1,
            Samples       = SampleCountFlags.Count1Bit,
            Tiling        = ImageTiling.Optimal,
            Usage         = ImageUsageFlags.DepthStencilAttachmentBit,
            SharingMode   = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Undefined,
        };
        Api.CreateImage(Device, in dci, null, out sc.DepthImage);
        Api.GetImageMemoryRequirements(Device, sc.DepthImage, out var dReq);
        sc.DepthMemory = AllocMemory(dReq.Size, dReq.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit);
        Api.BindImageMemory(Device, sc.DepthImage, sc.DepthMemory, 0);
        var dvci = new ImageViewCreateInfo
        {
            SType    = StructureType.ImageViewCreateInfo,
            Image    = sc.DepthImage,
            ViewType = ImageViewType.Type2D,
            Format   = depthVk,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.DepthBit |
                             (sc.DepthFormat == RhiFormat.D24_UNorm_S8_UInt ? ImageAspectFlags.StencilBit : 0),
                BaseMipLevel = 0, LevelCount = 1,
                BaseArrayLayer = 0, LayerCount = 1,
            },
        };
        Api.CreateImageView(Device, in dvci, null, out sc.DepthView);

        // Render pass + framebuffers. resolveToSwapchain=true so the final
        // layout of the color attachment is PRESENT_SRC_KHR.
        sc.RenderPass = GetOrCreateRenderPass(chosen.Format, depthVk, 1, resolveToSwapchain: true);

        sc.Framebuffers = new Framebuffer[n];
        for (int i = 0; i < n; i++)
        {
            var attachments = stackalloc ImageView[2] { sc.ColorViews[i], sc.DepthView };
            var fbci = new FramebufferCreateInfo
            {
                SType           = StructureType.FramebufferCreateInfo,
                RenderPass      = sc.RenderPass,
                AttachmentCount = 2,
                PAttachments    = attachments,
                Width           = extent.Width,
                Height          = extent.Height,
                Layers          = 1,
            };
            Api.CreateFramebuffer(Device, in fbci, null, out sc.Framebuffers[i]);
        }
    }

    public void ResizeSwapchain(SwapchainHandle handle, int width, int height)
    {
        var sc = Swapchains[handle.Id];
        WaitIdle();
        DestroySwapchainViews(sc);
        sc.Width = width; sc.Height = height;
        CreateSwapchainResources(sc);
    }

    public void Destroy(SwapchainHandle h)
    {
        if (Swapchains.Remove(h.Id, out var s)) DestroySwapchainInternal(s);
    }

    internal void DestroySwapchainInternal(VkSwapchainRes sc)
    {
        WaitIdle();
        DestroySwapchainViews(sc);
        if (sc.SwapchainHandle.Handle != 0)
            KhrSwapchain.DestroySwapchain(Device, sc.SwapchainHandle, null);
        if (sc.Surface.Handle != 0)
            KhrSurface.DestroySurface(Instance, sc.Surface, null);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hwnd, int nIndex);
    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong(IntPtr hwnd, int nIndex);
    [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    // Diagnostic log into %TEMP%\TombEditorVk.log so we capture surface +
    // swapchain negotiation regardless of whether stderr is attached.
    private static readonly object _logLock = new();
    internal static void DebugLog(string msg)
    {
        try
        {
            lock (_logLock)
            {
                var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TombEditorVk.log");
                System.IO.File.AppendAllText(path,
                    $"[{DateTime.Now:HH:mm:ss.fff}] {msg}\n");
            }
        }
        catch { /* never fail because of logging */ }
    }

    private void DestroySwapchainViews(VkSwapchainRes sc)
    {
        if (sc.Framebuffers != null)
            for (int i = 0; i < sc.Framebuffers.Length; i++)
                if (sc.Framebuffers[i].Handle != 0)
                    Api.DestroyFramebuffer(Device, sc.Framebuffers[i], null);
        if (sc.DepthView.Handle  != 0) Api.DestroyImageView(Device, sc.DepthView,  null);
        if (sc.DepthImage.Handle != 0) Api.DestroyImage(Device, sc.DepthImage,     null);
        if (sc.DepthMemory.Handle!= 0) Api.FreeMemory(Device, sc.DepthMemory,      null);
        if (sc.ColorViews != null)
            for (int i = 0; i < sc.ColorViews.Length; i++)
                if (sc.ColorViews[i].Handle != 0)
                    Api.DestroyImageView(Device, sc.ColorViews[i], null);
    }
}
