using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;
using RhiFormat = TombLib.RenderingV2.Rhi.Format;
using RhiBufferUsage = TombLib.RenderingV2.Rhi.BufferUsage;
using RhiBufferBindFlags = TombLib.RenderingV2.Rhi.BufferBindFlags;
using RhiTextureBindFlags = TombLib.RenderingV2.Rhi.TextureBindFlags;

namespace TombLib.RenderingV2.Backends.Vulkan;

// Internal data containers held by VkDevice's resource pools. Each owns its
// raw Vulkan handles + the memory allocation that backs it. None of these
// are exposed across the RHI surface; callers see only opaque handles.

internal sealed class VkBufferRes
{
    public VkBuffer       Handle;
    public DeviceMemory   Memory;
    public ulong          Size;
    public unsafe void*   Mapped;        // non-null for host-visible buffers (persistently mapped)
    public RhiBufferUsage Usage;
    public RhiBufferBindFlags BindFlags;
}

internal sealed class VkTextureRes
{
    public Image           Image;
    public ImageView       View;
    public DeviceMemory    Memory;
    public RhiFormat       Format;        // RHI format (not VkFormat)
    public int             Width, Height;
    public int             MipLevels;
    public int             ArrayLayers;
    public int             Samples;
    public RhiTextureBindFlags BindFlags;
    public ImageAspectFlags Aspect;       // COLOR | DEPTH
    public ImageLayout     CurrentLayout; // tracked so we transition correctly when written / read
}

internal sealed class VkSamplerRes
{
    public Sampler Handle;
}

internal sealed class VkPipelineRes
{
    public Pipeline       Handle;
    public PipelineLayout Layout;
    public int[]          VertexStrides = System.Array.Empty<int>();
    public RhiFormat[]    ColorFormats  = System.Array.Empty<RhiFormat>();
    public RhiFormat      DepthFormat;
    public int            Samples;
    // The render pass the pipeline was compiled against. Reused by BeginPass
    // when the bound pipeline matches the swapchain's render pass shape.
    public RenderPass     BasePass;
}

internal sealed class VkSwapchainRes
{
    public SurfaceKHR     Surface;
    public SwapchainKHR   SwapchainHandle;
    public Silk.NET.Vulkan.Format SwapchainFormat; // Vulkan format chosen at create time
    public Image[]        ColorImages = System.Array.Empty<Image>();
    public ImageView[]    ColorViews  = System.Array.Empty<ImageView>();
    public ImageLayout[]  ImageLayouts = System.Array.Empty<ImageLayout>();

    // Depth attachment matching the swapchain dimensions.
    public Image          DepthImage;
    public ImageView      DepthView;
    public DeviceMemory   DepthMemory;
    public RhiFormat      DepthFormat;       // RHI format
    public Silk.NET.Vulkan.Format DepthFormatVk;

    public RenderPass     RenderPass;
    public Framebuffer[]  Framebuffers = System.Array.Empty<Framebuffer>();

    public int            Width, Height, Samples;
    public RhiFormat      ColorFormat;       // RHI format
    public bool           VSync;
    public System.IntPtr  Hwnd;
    public uint           CurrentImageIndex;
    public bool           ImageAcquired;
}
