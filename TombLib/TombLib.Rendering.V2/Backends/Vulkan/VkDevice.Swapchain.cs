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
        // Win32 surface.
        var surfCi = new Win32SurfaceCreateInfoKHR
        {
            SType     = StructureType.Win32SurfaceCreateInfoKhr,
            Hinstance = System.Runtime.InteropServices.Marshal.GetHINSTANCE(typeof(VkDevice).Module),
            Hwnd      = desc.WindowHandle,
        };
        if (KhrWin32Surface.CreateWin32Surface(Instance, in surfCi, null, out var surface) != Result.Success)
            throw new InvalidOperationException("vkCreateWin32SurfaceKHR failed");

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

        var present = sc.VSync ? PresentModeKHR.FifoKhr : PresentModeKHR.ImmediateKhr;
        var sci = new SwapchainCreateInfoKHR
        {
            SType            = StructureType.SwapchainCreateInfoKhr,
            Surface          = sc.Surface,
            MinImageCount    = imageCount,
            ImageFormat      = chosen.Format,
            ImageColorSpace  = chosen.ColorSpace,
            ImageExtent      = extent,
            ImageArrayLayers = 1,
            ImageUsage       = ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.TransferDstBit,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform     = caps.CurrentTransform,
            CompositeAlpha   = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode      = present,
            Clipped          = true,
            OldSwapchain     = default,
        };
        if (KhrSwapchain.CreateSwapchain(Device, in sci, null, out sc.SwapchainHandle) != Result.Success)
            throw new InvalidOperationException("vkCreateSwapchainKHR failed");

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
