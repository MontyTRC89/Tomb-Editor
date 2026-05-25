using System;

namespace TombLib.Rendering.Graphics.Rhi;

/// <summary>
/// Identifies the underlying graphics API a backend talks to.
/// </summary>
public enum RhiBackendKind : byte
{
    DirectX11,
    Vulkan,
    OpenGL,
}

/// <summary>
/// Per-backend feature switches. The renderer core inspects these to choose
/// the best path (e.g. instancing always; structured buffers only when
/// supported). All current targets (DX11, Vulkan, OpenGL 4.3) provide the
/// full set, so this struct is mostly future-proofing.
/// </summary>
public readonly struct RhiCapabilities
{
    public readonly RhiBackendKind Backend;
    public readonly bool SupportsInstancedDraw;
    public readonly bool SupportsStructuredBuffers;
    public readonly bool SupportsNativePushConstants;  // true only on Vulkan
    public readonly bool SupportsAnisotropicFiltering;
    public readonly bool SupportsDebugMarkers;
    public readonly int  MaxTextureSize;
    public readonly int  MaxTextureArrayLayers;

    public RhiCapabilities(RhiBackendKind backend, bool instanced, bool structured,
                           bool nativePush, bool anisotropy, bool debugMarkers,
                           int maxTexSize, int maxArrayLayers)
    {
        Backend                     = backend;
        SupportsInstancedDraw       = instanced;
        SupportsStructuredBuffers   = structured;
        SupportsNativePushConstants = nativePush;
        SupportsAnisotropicFiltering= anisotropy;
        SupportsDebugMarkers        = debugMarkers;
        MaxTextureSize              = maxTexSize;
        MaxTextureArrayLayers       = maxArrayLayers;
    }
}

/// <summary>
/// Thin GPU device facade. The renderer core only ever talks to this
/// interface; concrete backends (DX11, Vulkan, OpenGL) implement it.
///
/// <para>Threading: methods are intended to be called from a single
/// rendering thread. Resource creation may be safely called from any thread
/// where the backend supports it (DX11 device is thread-safe for create
/// calls; Vulkan uses external synchronisation).</para>
///
/// <para>Lifetime: every <c>Create*</c> returns a handle that must be released
/// with the corresponding <c>Destroy*</c> when no longer used. The device
/// itself owns all GPU objects and frees anything still alive on
/// <see cref="Dispose"/>.</para>
/// </summary>
public interface IRhiDevice : IDisposable
{
    /// <summary>Backend kind + feature switches.</summary>
    RhiCapabilities Capabilities { get; }

    // -- Resource creation -------------------------------------------------

    BufferHandle    CreateBuffer(in BufferDesc desc, ReadOnlySpan<byte> initialData);
    TextureHandle   CreateTexture(in TextureDesc desc, ReadOnlySpan<byte> initialData);
    SamplerHandle   CreateSampler(in SamplerDesc desc);
    PipelineHandle  CreatePipeline(PipelineDesc desc);
    SwapchainHandle CreateSwapchain(in SwapchainDesc desc);

    // -- Resource updates --------------------------------------------------

    /// <summary>Resize an existing swapchain (window resize event).</summary>
    void ResizeSwapchain(SwapchainHandle swapchain, int width, int height);

    /// <summary>
    /// Upload a region of a texture from CPU memory. Mip / array layer
    /// addressing is collapsed into a single subresource index in
    /// DX11 fashion: <c>subresource = mip + layer * mipLevels</c>.
    /// </summary>
    void UpdateTexture(TextureHandle texture, int subresource,
                       int x, int y, int width, int height,
                       int rowPitchBytes, ReadOnlySpan<byte> data);

    /// <summary>
    /// Read back a texture's pixel data to CPU memory. Allocates and
    /// returns a freshly tightly packed byte buffer (no row padding).
    /// Forces a CPU/GPU sync — use only for thumbnail capture / debug.
    /// </summary>
    byte[] ReadTexture(TextureHandle texture, int subresource = 0);

    // -- Resource destruction ---------------------------------------------

    void Destroy(BufferHandle handle);
    void Destroy(TextureHandle handle);
    void Destroy(SamplerHandle handle);
    void Destroy(PipelineHandle handle);
    void Destroy(SwapchainHandle handle);

    // -- Command submission -----------------------------------------------

    /// <summary>Start recording a one-shot command list.</summary>
    ICommandList BeginCommandList();

    /// <summary>Submit a recorded command list for execution.</summary>
    void Submit(ICommandList commandList);

    /// <summary>Present the swapchain's back buffer.</summary>
    void Present(SwapchainHandle swapchain);

    /// <summary>
    /// Block the CPU until previously submitted work is finished. Used at
    /// shutdown and around resize / device-lost handling. Avoid in the
    /// per-frame hot path.
    /// </summary>
    void WaitIdle();
}
