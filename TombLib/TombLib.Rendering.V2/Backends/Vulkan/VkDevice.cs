using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using TombLib.RenderingV2.Rhi;
using RhiFormat   = TombLib.RenderingV2.Rhi.Format;
using VkFormat    = Silk.NET.Vulkan.Format;
using VkBuffer    = Silk.NET.Vulkan.Buffer;
using BufferDesc  = TombLib.RenderingV2.Rhi.BufferDesc;
using TextureDesc = TombLib.RenderingV2.Rhi.TextureDesc;
using SamplerDesc = TombLib.RenderingV2.Rhi.SamplerDesc;
using SwapchainDesc = TombLib.RenderingV2.Rhi.SwapchainDesc;

namespace TombLib.RenderingV2.Backends.Vulkan;

/// <summary>
/// Native Vulkan backend for <see cref="IRhiDevice"/>. Single-threaded:
/// assumes all calls come from the same rendering thread.
///
/// <para>Design notes (kept deliberately simple — see Dx11Device for the
/// reference implementation):
///   - One frame in flight. CPU waits on the previous frame's fence before
///     recording the next, so we don't need per-frame command pools / descriptor
///     pools / per-resource staging-buffer rings.
///   - Per-resource memory allocations (vkAllocateMemory / vkBindBufferMemory).
///     Not optimal vs a VMA-style sub-allocator, but the editor allocates a
///     few hundred buffers/textures total, well under the typical Vulkan
///     allocation limit (4096).
///   - Device-shared descriptor set layout sized to RhiLimits. Every
///     pipeline shares it; SetBindings allocates a fresh descriptor set from
///     the per-frame transient pool (which gets reset at frame start).
///   - Push constants block: 128 bytes at VS + FS stages.
/// </para>
/// </summary>
public unsafe sealed partial class VkDevice : IRhiDevice
{
    internal readonly Vk Api;
    internal Instance Instance;
    internal PhysicalDevice PhysicalDevice;
    internal Device Device;
    internal uint GraphicsQueueFamily;
    internal Queue GraphicsQueue;

    internal KhrSurface     KhrSurface;
    internal KhrSwapchain   KhrSwapchain;
    internal KhrWin32Surface KhrWin32Surface;
    internal Silk.NET.Vulkan.Extensions.EXT.ExtDebugUtils? DebugUtils;
    internal DebugUtilsMessengerEXT DebugMessenger;
    // Hold the callback delegate so the GC doesn't collect it while Vulkan
    // still holds the function pointer.
    internal DebugUtilsMessengerCallbackFunctionEXT? DebugCallback;

    // Memory properties cached after device creation — used by every
    // FindMemoryType call.
    internal PhysicalDeviceMemoryProperties MemProps;
    internal PhysicalDeviceLimits           Limits;
    internal PhysicalDeviceFeatures         Features;

    // Shared descriptor layout / pipeline layout. All RHI pipelines use this
    // single descriptor model:
    //   set 0:
    //     b0..b3    : MaxConstantBuffers uniform buffers      (VS + FS)
    //     t0..t7    : MaxTextureBindings sampled images       (VS + FS)
    //     s0..s3    : MaxSamplerBindings samplers             (VS + FS)
    //     ssbo0..1  : MaxStorageBuffers storage buffers       (VS + FS)
    //   push constants: 128 bytes at offset 0, stages = VS|FS
    internal DescriptorSetLayout SharedDescLayout;
    internal PipelineLayout      SharedPipelineLayout;

    // Per-frame resources. With one frame in flight there's just one of each.
    internal CommandPool          GraphicsPool;
    internal CommandBuffer        FrameCmd;
    internal Fence                FrameFence;
    internal Semaphore            ImageAvailable;
    internal DescriptorPool       TransientDescPool;
    internal bool                 FrameRecording;     // true between BeginCommandList and Submit

    // Swapchain MSAA sample count. Rendering targets an off-screen 4x colour
    // + depth image which the render pass resolves into the single-sample
    // swapchain image at EndPass. The Vulkan spec guarantees both
    // framebufferColorSampleCounts and framebufferDepthSampleCounts include
    // 1x and 4x, so this needs no capability probe. Matches the DX11 backend.
    internal const int MsaaSamples = 4;

    // Resource pools (handle id → resource).
    private uint _nextHandle = 1;
    internal readonly Dictionary<uint, VkBufferRes>    Buffers    = new();
    internal readonly Dictionary<uint, VkTextureRes>   Textures   = new();
    internal readonly Dictionary<uint, VkSamplerRes>   Samplers   = new();
    internal readonly Dictionary<uint, VkPipelineRes>  Pipelines  = new();
    internal readonly Dictionary<uint, VkSwapchainRes> Swapchains = new();

    public RhiCapabilities Capabilities { get; }

    public VkDevice()
    {
        Api = Vk.GetApi();
        CreateInstance();
        PickPhysicalDevice();
        CreateLogicalDevice();
        CreateSharedDescriptorLayout();
        CreateFrameResources();

        Capabilities = new RhiCapabilities(
            backend:        RhiBackendKind.Vulkan,
            instanced:      true,
            structured:     true,
            nativePush:     true,
            anisotropy:     Features.SamplerAnisotropy,
            debugMarkers:   true,
            maxTexSize:     (int)Limits.MaxImageDimension2D,
            maxArrayLayers: (int)Limits.MaxImageArrayLayers);
    }

    // ============================================================ Instance

    private void CreateInstance()
    {
        var appInfo = new ApplicationInfo
        {
            SType        = StructureType.ApplicationInfo,
            PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("TombEditor"),
            ApplicationVersion = 1,
            PEngineName  = (byte*)Marshal.StringToHGlobalAnsi("TombEditor"),
            EngineVersion = 1,
            ApiVersion   = Vk.Version11,
        };

        // Validation is opt-out (TOMBEDITOR_VK_VALIDATION=0 disables) when
        // the Khronos validation layer is installed. Captured messages go to
        // %TEMP%\TombEditorVk.log via a debug-utils messenger so we get the
        // real driver / spec violation text when an API call fails.
        bool wantValidation = Environment.GetEnvironmentVariable("TOMBEDITOR_VK_VALIDATION") != "0"
                              && IsLayerAvailable("VK_LAYER_KHRONOS_validation");
        var extensions = new List<string>
        {
            KhrSurface.ExtensionName,
            KhrWin32Surface.ExtensionName,
        };
        if (wantValidation) extensions.Add("VK_EXT_debug_utils");

        var layers = new List<string>();
        if (wantValidation) layers.Add("VK_LAYER_KHRONOS_validation");
        DebugLog($"VkDevice init: validation={(wantValidation ? "ON" : "off")}, layers={layers.Count}, extensions={extensions.Count}");

        var pExtNames = new byte*[extensions.Count];
        var pLayerNames = new byte*[layers.Count];
        try
        {
            for (int i = 0; i < extensions.Count; i++)
                pExtNames[i] = (byte*)Marshal.StringToHGlobalAnsi(extensions[i]);
            for (int i = 0; i < layers.Count; i++)
                pLayerNames[i] = (byte*)Marshal.StringToHGlobalAnsi(layers[i]);

            fixed (byte** ppExt = pExtNames)
            fixed (byte** ppLay = pLayerNames)
            {
                var ci = new InstanceCreateInfo
                {
                    SType                   = StructureType.InstanceCreateInfo,
                    PApplicationInfo        = &appInfo,
                    EnabledExtensionCount   = (uint)extensions.Count,
                    PpEnabledExtensionNames = ppExt,
                    EnabledLayerCount       = (uint)layers.Count,
                    PpEnabledLayerNames     = ppLay,
                };
                if (Api.CreateInstance(in ci, null, out Instance) != Result.Success)
                    throw new InvalidOperationException("vkCreateInstance failed");
            }
        }
        finally
        {
            for (int i = 0; i < pExtNames.Length; i++)
                if (pExtNames[i] != null) Marshal.FreeHGlobal((IntPtr)pExtNames[i]);
            for (int i = 0; i < pLayerNames.Length; i++)
                if (pLayerNames[i] != null) Marshal.FreeHGlobal((IntPtr)pLayerNames[i]);
            Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);
        }

        if (!Api.TryGetInstanceExtension(Instance, out KhrSurface))
            throw new InvalidOperationException("VK_KHR_surface not present on instance");
        if (!Api.TryGetInstanceExtension(Instance, out KhrWin32Surface))
            throw new InvalidOperationException("VK_KHR_win32_surface not present on instance");

        // Wire the debug-utils messenger so validation messages land in our
        // log file alongside the surface / swapchain trace. Without this,
        // validation output goes to the OutputDebugString channel and never
        // reaches the user.
        if (wantValidation && Api.TryGetInstanceExtension(Instance, out Silk.NET.Vulkan.Extensions.EXT.ExtDebugUtils du))
        {
            DebugUtils = du;
            DebugCallback = DebugMessageCallback;
            var dmci = new DebugUtilsMessengerCreateInfoEXT
            {
                SType           = StructureType.DebugUtilsMessengerCreateInfoExt,
                MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.WarningBitExt
                                | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt
                                | DebugUtilsMessageSeverityFlagsEXT.InfoBitExt,
                MessageType     = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt
                                | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt
                                | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt,
                PfnUserCallback = new PfnDebugUtilsMessengerCallbackEXT(DebugCallback),
            };
            DebugUtils.CreateDebugUtilsMessenger(Instance, in dmci, null, out DebugMessenger);
            DebugLog("VkDevice debug messenger attached");
        }
    }

    private static uint DebugMessageCallback(
        DebugUtilsMessageSeverityFlagsEXT severity,
        DebugUtilsMessageTypeFlagsEXT type,
        DebugUtilsMessengerCallbackDataEXT* data,
        void* userData)
    {
        string msg = data != null && data->PMessage != null
                     ? Marshal.PtrToStringAnsi((IntPtr)data->PMessage) ?? "(no message)"
                     : "(no data)";
        DebugLog($"VK [{severity}] {msg}");
        return 0u; // VK_FALSE: don't abort the call
    }

    private bool IsLayerAvailable(string name)
    {
        uint count = 0;
        Api.EnumerateInstanceLayerProperties(ref count, null);
        if (count == 0) return false;
        var props = new LayerProperties[count];
        fixed (LayerProperties* p = props)
            Api.EnumerateInstanceLayerProperties(ref count, p);
        for (int i = 0; i < props.Length; i++)
        {
            fixed (byte* n = props[i].LayerName)
            {
                string layerName = Marshal.PtrToStringAnsi((IntPtr)n) ?? "";
                if (layerName == name) return true;
            }
        }
        return false;
    }

    // ============================================================ Physical / Logical device

    private void PickPhysicalDevice()
    {
        uint count = 0;
        Api.EnumeratePhysicalDevices(Instance, ref count, null);
        if (count == 0) throw new InvalidOperationException("No Vulkan physical devices");
        var devs = new PhysicalDevice[count];
        fixed (PhysicalDevice* p = devs) Api.EnumeratePhysicalDevices(Instance, ref count, p);

        PhysicalDevice best = default;
        int bestScore = -1;
        for (int i = 0; i < devs.Length; i++)
        {
            var d = devs[i];
            Api.GetPhysicalDeviceProperties(d, out var props);
            // Score: discrete GPU > integrated > anything else; we always
            // need a graphics queue and KHR_swapchain support.
            if (!HasGraphicsQueue(d)) continue;
            if (!HasExtension(d, "VK_KHR_swapchain")) continue;
            int score = props.DeviceType switch
            {
                PhysicalDeviceType.DiscreteGpu   => 1000,
                PhysicalDeviceType.IntegratedGpu => 500,
                PhysicalDeviceType.VirtualGpu    => 100,
                _                                 => 10,
            };
            if (score > bestScore) { best = d; bestScore = score; }
        }
        if (bestScore < 0) throw new InvalidOperationException("No suitable Vulkan device (need graphics + swapchain)");

        PhysicalDevice = best;
        Api.GetPhysicalDeviceMemoryProperties(PhysicalDevice, out MemProps);
        Api.GetPhysicalDeviceProperties(PhysicalDevice, out var p2);
        Limits = p2.Limits;
        Api.GetPhysicalDeviceFeatures(PhysicalDevice, out Features);
    }

    private bool HasGraphicsQueue(PhysicalDevice d)
    {
        uint count = 0;
        Api.GetPhysicalDeviceQueueFamilyProperties(d, ref count, null);
        var props = new QueueFamilyProperties[count];
        fixed (QueueFamilyProperties* p = props) Api.GetPhysicalDeviceQueueFamilyProperties(d, ref count, p);
        for (int i = 0; i < props.Length; i++)
            if ((props[i].QueueFlags & QueueFlags.GraphicsBit) != 0)
                return true;
        return false;
    }

    private bool HasExtension(PhysicalDevice d, string name)
    {
        uint count = 0;
        Api.EnumerateDeviceExtensionProperties(d, (byte*)null, ref count, null);
        var props = new ExtensionProperties[count];
        fixed (ExtensionProperties* p = props) Api.EnumerateDeviceExtensionProperties(d, (byte*)null, ref count, p);
        for (int i = 0; i < props.Length; i++)
        {
            fixed (byte* n = props[i].ExtensionName)
            {
                string ext = Marshal.PtrToStringAnsi((IntPtr)n) ?? "";
                if (ext == name) return true;
            }
        }
        return false;
    }

    private void CreateLogicalDevice()
    {
        // Pick first graphics+present queue family. On Windows the graphics
        // queue is always presentation-capable on consumer GPUs, so we don't
        // need to query per-surface presentation support before device
        // creation (we'd need a dummy surface for that).
        uint count = 0;
        Api.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, ref count, null);
        var qProps = new QueueFamilyProperties[count];
        fixed (QueueFamilyProperties* p = qProps) Api.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, ref count, p);
        GraphicsQueueFamily = uint.MaxValue;
        for (uint i = 0; i < qProps.Length; i++)
            if ((qProps[i].QueueFlags & QueueFlags.GraphicsBit) != 0)
            { GraphicsQueueFamily = i; break; }
        if (GraphicsQueueFamily == uint.MaxValue)
            throw new InvalidOperationException("No graphics queue family");

        float priority = 1.0f;
        var qci = new DeviceQueueCreateInfo
        {
            SType            = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = GraphicsQueueFamily,
            QueueCount       = 1,
            PQueuePriorities = &priority,
        };

        var deviceExts = new[] { "VK_KHR_swapchain" };
        var pDevExts = new byte*[deviceExts.Length];
        for (int i = 0; i < deviceExts.Length; i++)
            pDevExts[i] = (byte*)Marshal.StringToHGlobalAnsi(deviceExts[i]);

        var feats = new PhysicalDeviceFeatures
        {
            SamplerAnisotropy = Features.SamplerAnisotropy,
            FillModeNonSolid  = Features.FillModeNonSolid, // wireframe
        };

        try
        {
            fixed (byte** ppExt = pDevExts)
            {
                var dci = new DeviceCreateInfo
                {
                    SType                   = StructureType.DeviceCreateInfo,
                    QueueCreateInfoCount    = 1,
                    PQueueCreateInfos       = &qci,
                    EnabledExtensionCount   = (uint)deviceExts.Length,
                    PpEnabledExtensionNames = ppExt,
                    PEnabledFeatures        = &feats,
                };
                if (Api.CreateDevice(PhysicalDevice, in dci, null, out Device) != Result.Success)
                    throw new InvalidOperationException("vkCreateDevice failed");
            }
        }
        finally
        {
            for (int i = 0; i < pDevExts.Length; i++) Marshal.FreeHGlobal((IntPtr)pDevExts[i]);
        }

        Api.GetDeviceQueue(Device, GraphicsQueueFamily, 0, out GraphicsQueue);
        if (!Api.TryGetDeviceExtension(Instance, Device, out KhrSwapchain))
            throw new InvalidOperationException("VK_KHR_swapchain not available on device");
    }

    // ============================================================ Memory helper

    internal uint FindMemoryType(uint typeBits, MemoryPropertyFlags required)
    {
        for (uint i = 0; i < MemProps.MemoryTypeCount; i++)
        {
            if ((typeBits & (1u << (int)i)) == 0) continue;
            if ((MemProps.MemoryTypes[(int)i].PropertyFlags & required) == required)
                return i;
        }
        throw new InvalidOperationException($"No suitable memory type for required flags {required}");
    }

    internal DeviceMemory AllocMemory(ulong size, uint typeBits, MemoryPropertyFlags required)
    {
        var ai = new MemoryAllocateInfo
        {
            SType           = StructureType.MemoryAllocateInfo,
            AllocationSize  = size,
            MemoryTypeIndex = FindMemoryType(typeBits, required),
        };
        if (Api.AllocateMemory(Device, in ai, null, out var mem) != Result.Success)
            throw new InvalidOperationException("vkAllocateMemory failed");
        return mem;
    }

    // ============================================================ Shared layouts

    private void CreateSharedDescriptorLayout()
    {
        // Layout matches what the shaders actually declare via VK_BINDING:
        //   binding 0 = ViewParams cbuffer
        //   binding 1 = Atlas texture
        //   binding 2 = Atlas sampler
        // C# code uses ConstantBuffers[0] / Textures[0] / Samplers[0]
        // respectively. If a future shader needs a second cbuf / texture,
        // extend this layout *and* the VK_BINDING numbers in the HLSL.
        var bindings = stackalloc DescriptorSetLayoutBinding[3]
        {
            new()
            {
                Binding         = 0,
                DescriptorType  = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags      = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            },
            new()
            {
                Binding         = 1,
                DescriptorType  = DescriptorType.SampledImage,
                DescriptorCount = 1,
                StageFlags      = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            },
            new()
            {
                Binding         = 2,
                DescriptorType  = DescriptorType.Sampler,
                DescriptorCount = 1,
                StageFlags      = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            },
        };
        var slci = new DescriptorSetLayoutCreateInfo
        {
            SType        = StructureType.DescriptorSetLayoutCreateInfo,
            BindingCount = 3,
            PBindings    = bindings,
        };
        if (Api.CreateDescriptorSetLayout(Device, in slci, null, out SharedDescLayout) != Result.Success)
            throw new InvalidOperationException("vkCreateDescriptorSetLayout failed");

        var pcRange = new PushConstantRange
        {
            StageFlags = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            Offset     = 0,
            Size       = (uint)RhiLimits.PushConstantSize,
        };
        var dsl = SharedDescLayout;
        var plci = new PipelineLayoutCreateInfo
        {
            SType                  = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount         = 1,
            PSetLayouts            = &dsl,
            PushConstantRangeCount = 1,
            PPushConstantRanges    = &pcRange,
        };
        if (Api.CreatePipelineLayout(Device, in plci, null, out SharedPipelineLayout) != Result.Success)
            throw new InvalidOperationException("vkCreatePipelineLayout failed");
    }

    private void CreateFrameResources()
    {
        var cpci = new CommandPoolCreateInfo
        {
            SType            = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = GraphicsQueueFamily,
            Flags            = CommandPoolCreateFlags.ResetCommandBufferBit,
        };
        if (Api.CreateCommandPool(Device, in cpci, null, out GraphicsPool) != Result.Success)
            throw new InvalidOperationException("vkCreateCommandPool failed");

        var cba = new CommandBufferAllocateInfo
        {
            SType              = StructureType.CommandBufferAllocateInfo,
            CommandPool        = GraphicsPool,
            Level              = CommandBufferLevel.Primary,
            CommandBufferCount = 1,
        };
        CommandBuffer cb = default;
        if (Api.AllocateCommandBuffers(Device, in cba, &cb) != Result.Success)
            throw new InvalidOperationException("vkAllocateCommandBuffers failed");
        FrameCmd = cb;

        var fci = new FenceCreateInfo
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit, // start signaled so first WaitForFences passes
        };
        Api.CreateFence(Device, in fci, null, out FrameFence);

        var sci = new SemaphoreCreateInfo { SType = StructureType.SemaphoreCreateInfo };
        // ImageAvailable is one shared acquire semaphore — safe with a single
        // frame in flight (BeginCommandList waits FrameFence before the next
        // acquire). The "render finished" semaphores are per swapchain image
        // and live on VkSwapchainRes; see CreateSwapchainResources.
        Api.CreateSemaphore(Device, in sci, null, out ImageAvailable);

        // Transient descriptor pool — sized to one descriptor of each type
        // per allocation × MaxSets. SetBindings allocates a fresh descriptor
        // set every call within a frame; the pool resets at frame start.
        const int setsPerFrame = 1024;
        var poolSizes = stackalloc DescriptorPoolSize[3]
        {
            new() { Type = DescriptorType.UniformBuffer, DescriptorCount = setsPerFrame },
            new() { Type = DescriptorType.SampledImage,  DescriptorCount = setsPerFrame },
            new() { Type = DescriptorType.Sampler,       DescriptorCount = setsPerFrame },
        };
        var dpci = new DescriptorPoolCreateInfo
        {
            SType         = StructureType.DescriptorPoolCreateInfo,
            MaxSets       = setsPerFrame,
            PoolSizeCount = 3,
            PPoolSizes    = poolSizes,
            Flags         = 0, // no individual free; we reset the whole pool each frame
        };
        if (Api.CreateDescriptorPool(Device, in dpci, null, out TransientDescPool) != Result.Success)
            throw new InvalidOperationException("vkCreateDescriptorPool failed");
    }

    internal uint AllocHandle() => _nextHandle++;

    // Deferred resource destruction. The backend runs a single frame in
    // flight, so a buffer / texture / sampler / pipeline stays GPU-visible
    // until the NEXT BeginCommandList drains the previous frame's fence.
    // Freeing one immediately races that in-flight frame
    // (VUID-vkDestroy*-*-00922 / sampler-01082 / ... → device lost → the next
    // WaitForFences hangs). Destroy() therefore only unregisters the handle
    // and queues the real free here; the queue is flushed at the start of
    // every BeginCommandList, once the previous frame is provably complete.
    private readonly List<Action> _pendingDeletes = new();

    private void FlushPendingDeletes()
    {
        if (_pendingDeletes.Count == 0) return;
        foreach (var del in _pendingDeletes) del();
        _pendingDeletes.Clear();
    }

    public void Dispose()
    {
        // Guard against double-dispose: a second call would run WaitIdle() and
        // Api.* against an already-destroyed device / disposed Api and crash
        // with an access violation.
        if (_disposed) return;
        WaitIdle();
        _disposed = true;
        FlushPendingDeletes();
        foreach (var s in Swapchains.Values) DestroySwapchainInternal(s);
        foreach (var p in Pipelines.Values)  DestroyPipelineInternal(p);
        foreach (var s in Samplers.Values)   Api.DestroySampler(Device, s.Handle, null);
        foreach (var t in Textures.Values)   DestroyTextureInternal(t);
        foreach (var b in Buffers.Values)    DestroyBufferInternal(b);
        Swapchains.Clear();
        Pipelines.Clear();
        Samplers.Clear();
        Textures.Clear();
        Buffers.Clear();

        if (TransientDescPool.Handle    != 0) Api.DestroyDescriptorPool(Device, TransientDescPool, null);
        if (ImageAvailable.Handle       != 0) Api.DestroySemaphore(Device, ImageAvailable, null);
        if (FrameFence.Handle           != 0) Api.DestroyFence(Device, FrameFence, null);
        if (GraphicsPool.Handle         != 0) Api.DestroyCommandPool(Device, GraphicsPool, null);
        if (SharedPipelineLayout.Handle != 0) Api.DestroyPipelineLayout(Device, SharedPipelineLayout, null);
        if (SharedDescLayout.Handle     != 0) Api.DestroyDescriptorSetLayout(Device, SharedDescLayout, null);
        if (Device.Handle               != 0) Api.DestroyDevice(Device, null);
        if (DebugMessenger.Handle != 0 && DebugUtils != null)
            DebugUtils.DestroyDebugUtilsMessenger(Instance, DebugMessenger, null);
        if (Instance.Handle             != 0) Api.DestroyInstance(Instance, null);
        Api.Dispose();
    }

    // Set once Dispose() has destroyed the device — blocks any further
    // WaitIdle() / swapchain teardown from touching freed Vulkan handles.
    private bool _disposed;

    public void WaitIdle()
    {
        if (_disposed || Device.Handle == 0) return;
        Api.DeviceWaitIdle(Device);
    }

    // ============================================================ Buffers

    public BufferHandle CreateBuffer(in BufferDesc desc, ReadOnlySpan<byte> initialData)
    {
        bool isDynamic = desc.Usage == BufferUsage.DynamicUniform || desc.Usage == BufferUsage.DynamicVertex;

        // Usage flags for the GPU-side buffer.
        BufferUsageFlags usageFlags = 0;
        if ((desc.BindFlags & BufferBindFlags.Vertex)     != 0) usageFlags |= BufferUsageFlags.VertexBufferBit;
        if ((desc.BindFlags & BufferBindFlags.Index)      != 0) usageFlags |= BufferUsageFlags.IndexBufferBit;
        if ((desc.BindFlags & BufferBindFlags.Constant)   != 0) usageFlags |= BufferUsageFlags.UniformBufferBit;
        if ((desc.BindFlags & BufferBindFlags.Structured) != 0) usageFlags |= BufferUsageFlags.StorageBufferBit;
        // We also need TransferDst on every buffer that can receive a staging
        // copy (immutable + initialData and dynamic UpdateBuffer fallback).
        usageFlags |= BufferUsageFlags.TransferDstBit;

        var bci = new BufferCreateInfo
        {
            SType       = StructureType.BufferCreateInfo,
            Size        = (ulong)desc.SizeBytes,
            Usage       = usageFlags,
            SharingMode = SharingMode.Exclusive,
        };
        if (Api.CreateBuffer(Device, in bci, null, out var buf) != Result.Success)
            throw new InvalidOperationException("vkCreateBuffer failed");
        Api.GetBufferMemoryRequirements(Device, buf, out var req);

        // Dynamic buffers go in HOST_VISIBLE | HOST_COHERENT memory so the
        // CPU can map them persistently; immutable buffers go in DEVICE_LOCAL.
        var memFlags = isDynamic
                       ? (MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)
                       : MemoryPropertyFlags.DeviceLocalBit;
        var mem = AllocMemory(req.Size, req.MemoryTypeBits, memFlags);
        Api.BindBufferMemory(Device, buf, mem, 0);

        void* mapped = null;
        if (isDynamic)
        {
            void* p;
            Api.MapMemory(Device, mem, 0, req.Size, 0, &p);
            mapped = p;
        }

        var res = new VkBufferRes
        {
            Handle    = buf,
            Memory    = mem,
            Size      = (ulong)desc.SizeBytes,
            Mapped    = mapped,
            Usage     = desc.Usage,
            BindFlags = desc.BindFlags,
        };

        // Initial upload (immutable path: staging buffer + copy).
        if (initialData.Length > 0)
        {
            if (isDynamic)
            {
                fixed (byte* src = initialData)
                    System.Buffer.MemoryCopy(src, mapped, (long)res.Size, initialData.Length);
            }
            else
            {
                UploadImmutable(res, initialData);
            }
        }

        uint id = AllocHandle();
        Buffers[id] = res;
        return new BufferHandle(id);
    }

    private void UploadImmutable(VkBufferRes dst, ReadOnlySpan<byte> data)
    {
        // Staging buffer in host memory + one-shot copy command.
        var sci = new BufferCreateInfo
        {
            SType       = StructureType.BufferCreateInfo,
            Size        = (ulong)data.Length,
            Usage       = BufferUsageFlags.TransferSrcBit,
            SharingMode = SharingMode.Exclusive,
        };
        Api.CreateBuffer(Device, in sci, null, out var staging);
        Api.GetBufferMemoryRequirements(Device, staging, out var sReq);
        var sMem = AllocMemory(sReq.Size, sReq.MemoryTypeBits,
                               MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);
        Api.BindBufferMemory(Device, staging, sMem, 0);
        void* p;
        Api.MapMemory(Device, sMem, 0, sReq.Size, 0, &p);
        fixed (byte* src = data) System.Buffer.MemoryCopy(src, p, (long)sReq.Size, data.Length);
        Api.UnmapMemory(Device, sMem);

        var cmd = OneShotBegin();
        var region = new BufferCopy { Size = (ulong)data.Length };
        Api.CmdCopyBuffer(cmd, staging, dst.Handle, 1, in region);
        OneShotEndSubmitWait(cmd);

        Api.DestroyBuffer(Device, staging, null);
        Api.FreeMemory(Device, sMem, null);
    }

    public void Destroy(BufferHandle h)
    {
        if (Buffers.Remove(h.Id, out var b))
            _pendingDeletes.Add(() => DestroyBufferInternal(b));
    }

    private void DestroyBufferInternal(VkBufferRes b)
    {
        if (b.Mapped != null) Api.UnmapMemory(Device, b.Memory);
        Api.DestroyBuffer(Device, b.Handle, null);
        Api.FreeMemory(Device, b.Memory, null);
    }

    // ============================================================ Textures

    public TextureHandle CreateTexture(in TextureDesc desc, ReadOnlySpan<byte> initialData)
    {
        bool hasColor = (desc.BindFlags & TextureBindFlags.RenderTarget) != 0;
        bool hasDepth = (desc.BindFlags & TextureBindFlags.DepthStencil) != 0;
        bool hasShader = (desc.BindFlags & TextureBindFlags.ShaderResource) != 0;

        var aspect = VkMapping.AspectOf(desc.Format);
        var imgUsage = ImageUsageFlags.None;
        if (hasShader) imgUsage |= ImageUsageFlags.SampledBit;
        if (hasColor)  imgUsage |= ImageUsageFlags.ColorAttachmentBit;
        if (hasDepth)  imgUsage |= ImageUsageFlags.DepthStencilAttachmentBit;
        // Always allow TransferDst. It is needed for create-time uploads
        // (below) AND for every later UpdateTexture call — and UpdateTexture
        // routinely targets a texture created with no initial data (the
        // preview / WAD-thumbnail atlases create empty on purpose, then fill
        // regions incrementally). Gating this on initialData made
        // vkCmdCopyBufferToImage fail validation (VUID-...-dstImage-00177) on
        // those atlases, which on a strict driver loses the device and hangs
        // the next frame's WaitForFences. It is free on a texture that is
        // never copied into — exactly like TransferSrc just below.
        imgUsage |= ImageUsageFlags.TransferDstBit;
        // We always allow TransferSrc so ReadTexture (thumbnail capture)
        // can copy out of any texture; vkCmdCopyImageToBuffer needs it.
        imgUsage |= ImageUsageFlags.TransferSrcBit;

        int mip = Math.Max(1, desc.MipLevels);
        var ici = new ImageCreateInfo
        {
            SType         = StructureType.ImageCreateInfo,
            ImageType     = ImageType.Type2D,
            Format        = VkMapping.ToVk(desc.Format),
            Extent        = new Extent3D((uint)desc.Width, (uint)desc.Height, 1),
            MipLevels     = (uint)mip,
            ArrayLayers   = (uint)desc.ArrayLayers,
            Samples       = VkMapping.ToSampleCount(desc.Samples),
            Tiling        = ImageTiling.Optimal,
            Usage         = imgUsage,
            SharingMode   = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Undefined,
            Flags         = desc.Kind == TextureKind.TextureCube ? ImageCreateFlags.CreateCubeCompatibleBit : 0,
        };
        if (Api.CreateImage(Device, in ici, null, out var img) != Result.Success)
            throw new InvalidOperationException("vkCreateImage failed");
        Api.GetImageMemoryRequirements(Device, img, out var req);
        var mem = AllocMemory(req.Size, req.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit);
        Api.BindImageMemory(Device, img, mem, 0);

        var view = CreateImageView(img, VkMapping.ToVk(desc.Format), aspect, (uint)mip);

        var res = new VkTextureRes
        {
            Image         = img,
            View          = view,
            Memory        = mem,
            Format        = desc.Format,
            Width         = desc.Width,
            Height        = desc.Height,
            MipLevels     = mip,
            ArrayLayers   = desc.ArrayLayers,
            Samples       = desc.Samples,
            BindFlags     = desc.BindFlags,
            Aspect        = aspect,
            CurrentLayout = ImageLayout.Undefined,
        };

        if (initialData.Length > 0)
            UploadTextureMip0(res, initialData);
        else
        {
            // Transition to a stable layout so first use as shader resource
            // doesn't validate as Undefined.
            if (hasShader)
                TransitionImage(res, ImageLayout.ShaderReadOnlyOptimal);
            else if (hasColor)
                TransitionImage(res, ImageLayout.ColorAttachmentOptimal);
            else if (hasDepth)
                TransitionImage(res, ImageLayout.DepthStencilAttachmentOptimal);
        }

        uint id = AllocHandle();
        Textures[id] = res;
        return new TextureHandle(id);
    }

    private ImageView CreateImageView(Image img, VkFormat fmt, ImageAspectFlags aspect, uint mipLevels)
    {
        var ivci = new ImageViewCreateInfo
        {
            SType    = StructureType.ImageViewCreateInfo,
            Image    = img,
            ViewType = ImageViewType.Type2D,
            Format   = fmt,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask     = aspect,
                BaseMipLevel   = 0,
                LevelCount     = mipLevels,
                BaseArrayLayer = 0,
                LayerCount     = 1,
            },
        };
        Api.CreateImageView(Device, in ivci, null, out var view);
        return view;
    }

    private void UploadTextureMip0(VkTextureRes tex, ReadOnlySpan<byte> data)
    {
        int bpp = VkMapping.BytesPerPixel(tex.Format);
        ulong size = (ulong)(tex.Width * tex.Height * bpp);

        var sci = new BufferCreateInfo
        {
            SType       = StructureType.BufferCreateInfo,
            Size        = size,
            Usage       = BufferUsageFlags.TransferSrcBit,
            SharingMode = SharingMode.Exclusive,
        };
        Api.CreateBuffer(Device, in sci, null, out var staging);
        Api.GetBufferMemoryRequirements(Device, staging, out var sReq);
        var sMem = AllocMemory(sReq.Size, sReq.MemoryTypeBits,
                               MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);
        Api.BindBufferMemory(Device, staging, sMem, 0);
        void* p;
        Api.MapMemory(Device, sMem, 0, sReq.Size, 0, &p);
        fixed (byte* src = data) System.Buffer.MemoryCopy(src, p, (long)sReq.Size, Math.Min(data.Length, (int)size));
        Api.UnmapMemory(Device, sMem);

        var cmd = OneShotBegin();
        TransitionImage(cmd, tex, ImageLayout.TransferDstOptimal);
        var region = new BufferImageCopy
        {
            BufferOffset      = 0,
            BufferRowLength   = 0,
            BufferImageHeight = 0,
            ImageSubresource  = new ImageSubresourceLayers { AspectMask = tex.Aspect, MipLevel = 0, BaseArrayLayer = 0, LayerCount = 1 },
            ImageOffset       = new Offset3D(0, 0, 0),
            ImageExtent       = new Extent3D((uint)tex.Width, (uint)tex.Height, 1),
        };
        Api.CmdCopyBufferToImage(cmd, staging, tex.Image, ImageLayout.TransferDstOptimal, 1, in region);
        TransitionImage(cmd, tex, ImageLayout.ShaderReadOnlyOptimal);
        OneShotEndSubmitWait(cmd);

        Api.DestroyBuffer(Device, staging, null);
        Api.FreeMemory(Device, sMem, null);
    }

    public void UpdateTexture(TextureHandle handle, int subresource,
                              int x, int y, int width, int height,
                              int rowPitchBytes, ReadOnlySpan<byte> data)
    {
        var t = Textures[handle.Id];
        // Build a tightly-packed staging copy. The caller's rowPitch may be
        // larger than width*bpp on D3D, but in practice TombEditor never sends
        // padded rows here — keep it simple.
        int bpp = VkMapping.BytesPerPixel(t.Format);
        ulong size = (ulong)(width * height * bpp);

        var sci = new BufferCreateInfo
        {
            SType       = StructureType.BufferCreateInfo,
            Size        = size,
            Usage       = BufferUsageFlags.TransferSrcBit,
            SharingMode = SharingMode.Exclusive,
        };
        Api.CreateBuffer(Device, in sci, null, out var staging);
        Api.GetBufferMemoryRequirements(Device, staging, out var sReq);
        var sMem = AllocMemory(sReq.Size, sReq.MemoryTypeBits,
                               MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);
        Api.BindBufferMemory(Device, staging, sMem, 0);
        void* p;
        Api.MapMemory(Device, sMem, 0, sReq.Size, 0, &p);
        // Copy row by row, dropping any source padding.
        fixed (byte* src = data)
        {
            for (int yy = 0; yy < height; yy++)
                System.Buffer.MemoryCopy(src + yy * rowPitchBytes,
                                         (byte*)p + yy * width * bpp,
                                         width * bpp, width * bpp);
        }
        Api.UnmapMemory(Device, sMem);

        var cmd = OneShotBegin();
        TransitionImage(cmd, t, ImageLayout.TransferDstOptimal);
        var region = new BufferImageCopy
        {
            ImageSubresource = new ImageSubresourceLayers
            {
                AspectMask = t.Aspect, MipLevel = (uint)subresource,
                BaseArrayLayer = 0, LayerCount = 1,
            },
            ImageOffset = new Offset3D(x, y, 0),
            ImageExtent = new Extent3D((uint)width, (uint)height, 1),
        };
        Api.CmdCopyBufferToImage(cmd, staging, t.Image, ImageLayout.TransferDstOptimal, 1, in region);
        TransitionImage(cmd, t, ImageLayout.ShaderReadOnlyOptimal);
        OneShotEndSubmitWait(cmd);

        Api.DestroyBuffer(Device, staging, null);
        Api.FreeMemory(Device, sMem, null);
    }

    public byte[] ReadTexture(TextureHandle handle, int subresource = 0)
    {
        var t = Textures[handle.Id];
        int bpp = VkMapping.BytesPerPixel(t.Format);
        ulong size = (ulong)(t.Width * t.Height * bpp);
        byte[] result = new byte[size];

        var sci = new BufferCreateInfo
        {
            SType       = StructureType.BufferCreateInfo,
            Size        = size,
            Usage       = BufferUsageFlags.TransferDstBit,
            SharingMode = SharingMode.Exclusive,
        };
        Api.CreateBuffer(Device, in sci, null, out var staging);
        Api.GetBufferMemoryRequirements(Device, staging, out var sReq);
        var sMem = AllocMemory(sReq.Size, sReq.MemoryTypeBits,
                               MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);
        Api.BindBufferMemory(Device, staging, sMem, 0);

        var cmd = OneShotBegin();
        var prevLayout = t.CurrentLayout;
        TransitionImage(cmd, t, ImageLayout.TransferSrcOptimal);
        var region = new BufferImageCopy
        {
            ImageSubresource = new ImageSubresourceLayers
            {
                AspectMask = t.Aspect, MipLevel = (uint)subresource,
                BaseArrayLayer = 0, LayerCount = 1,
            },
            ImageOffset = new Offset3D(0, 0, 0),
            ImageExtent = new Extent3D((uint)t.Width, (uint)t.Height, 1),
        };
        Api.CmdCopyImageToBuffer(cmd, t.Image, ImageLayout.TransferSrcOptimal, staging, 1, in region);
        // Restore to a sensible post-read layout: shader read for sampled
        // textures, otherwise the previous layout the caller relied on.
        var restore = (t.BindFlags & TextureBindFlags.ShaderResource) != 0
                      ? ImageLayout.ShaderReadOnlyOptimal
                      : prevLayout != ImageLayout.Undefined ? prevLayout : ImageLayout.General;
        TransitionImage(cmd, t, restore);
        OneShotEndSubmitWait(cmd);

        void* p;
        Api.MapMemory(Device, sMem, 0, size, 0, &p);
        fixed (byte* dst = result)
            System.Buffer.MemoryCopy(p, dst, (long)size, (long)size);
        Api.UnmapMemory(Device, sMem);

        Api.DestroyBuffer(Device, staging, null);
        Api.FreeMemory(Device, sMem, null);
        return result;
    }

    public void Destroy(TextureHandle h)
    {
        if (Textures.Remove(h.Id, out var t))
            _pendingDeletes.Add(() => DestroyTextureInternal(t));
    }

    private void DestroyTextureInternal(VkTextureRes t)
    {
        if (t.View.Handle  != 0) Api.DestroyImageView(Device, t.View, null);
        if (t.Image.Handle != 0) Api.DestroyImage(Device, t.Image, null);
        if (t.Memory.Handle != 0) Api.FreeMemory(Device, t.Memory, null);
    }

    internal void TransitionImage(VkTextureRes tex, ImageLayout newLayout)
    {
        var cmd = OneShotBegin();
        TransitionImage(cmd, tex, newLayout);
        OneShotEndSubmitWait(cmd);
    }

    internal void TransitionImage(CommandBuffer cmd, VkTextureRes tex, ImageLayout newLayout)
    {
        if (tex.CurrentLayout == newLayout) return;
        var barrier = new ImageMemoryBarrier
        {
            SType            = StructureType.ImageMemoryBarrier,
            OldLayout        = tex.CurrentLayout,
            NewLayout        = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image            = tex.Image,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = tex.Aspect, BaseMipLevel = 0, LevelCount = (uint)tex.MipLevels,
                BaseArrayLayer = 0, LayerCount = (uint)tex.ArrayLayers,
            },
        };
        PipelineStageFlags srcStage = PipelineStageFlags.AllCommandsBit;
        PipelineStageFlags dstStage = PipelineStageFlags.AllCommandsBit;
        // Conservative access masks — AllCommands stage covers everything;
        // overkill but simple.
        barrier.SrcAccessMask = AccessFlags.MemoryReadBit | AccessFlags.MemoryWriteBit;
        barrier.DstAccessMask = AccessFlags.MemoryReadBit | AccessFlags.MemoryWriteBit;
        Api.CmdPipelineBarrier(cmd, srcStage, dstStage, 0,
                               0, null, 0, null, 1, in barrier);
        tex.CurrentLayout = newLayout;
    }

    // ============================================================ Samplers

    public SamplerHandle CreateSampler(in SamplerDesc desc)
    {
        bool aniso = desc.MinFilter == FilterMode.Anisotropic && Features.SamplerAnisotropy;
        var sci = new SamplerCreateInfo
        {
            SType            = StructureType.SamplerCreateInfo,
            MagFilter        = desc.MagFilter == FilterMode.Nearest ? Filter.Nearest : Filter.Linear,
            MinFilter        = desc.MinFilter == FilterMode.Nearest ? Filter.Nearest : Filter.Linear,
            MipmapMode       = desc.MipFilter == FilterMode.Nearest ? SamplerMipmapMode.Nearest : SamplerMipmapMode.Linear,
            AddressModeU     = VkMapping.ToVk(desc.AddressU),
            AddressModeV     = VkMapping.ToVk(desc.AddressV),
            AddressModeW     = VkMapping.ToVk(desc.AddressW),
            MipLodBias       = 0,
            AnisotropyEnable = aniso,
            MaxAnisotropy    = aniso ? Math.Min(desc.MaxAnisotropy, (int)Limits.MaxSamplerAnisotropy) : 1,
            CompareEnable    = false,
            CompareOp        = Silk.NET.Vulkan.CompareOp.Never,
            MinLod           = 0,
            MaxLod           = Vk.LodClampNone,
            BorderColor      = BorderColor.FloatTransparentBlack,
            UnnormalizedCoordinates = false,
        };
        Api.CreateSampler(Device, in sci, null, out var s);

        uint id = AllocHandle();
        Samplers[id] = new VkSamplerRes { Handle = s };
        return new SamplerHandle(id);
    }

    public void Destroy(SamplerHandle h)
    {
        if (Samplers.Remove(h.Id, out var s))
            _pendingDeletes.Add(() => Api.DestroySampler(Device, s.Handle, null));
    }

    // ============================================================ One-shot command helper

    internal CommandBuffer OneShotBegin()
    {
        var alloc = new CommandBufferAllocateInfo
        {
            SType              = StructureType.CommandBufferAllocateInfo,
            CommandPool        = GraphicsPool,
            Level              = CommandBufferLevel.Primary,
            CommandBufferCount = 1,
        };
        CommandBuffer cb = default;
        Api.AllocateCommandBuffers(Device, in alloc, &cb);
        var bi = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
        };
        Api.BeginCommandBuffer(cb, in bi);
        return cb;
    }

    internal void OneShotEndSubmitWait(CommandBuffer cb)
    {
        Api.EndCommandBuffer(cb);
        var si = new SubmitInfo
        {
            SType              = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers    = &cb,
        };
        Api.QueueSubmit(GraphicsQueue, 1, in si, default);
        Api.QueueWaitIdle(GraphicsQueue);
        Api.FreeCommandBuffers(Device, GraphicsPool, 1, &cb);
    }

    // ============================================================ Command stream

    private VkCommandList? _activeCmd;

    public ICommandList BeginCommandList()
    {
        var fence = FrameFence;
        if (FrameRecording)
        {
            // The previous frame was abandoned before Submit — an exception
            // fired between BeginCommandList and Submit (e.g. the not-yet-
            // implemented offscreen BeginPass that thumbnail rendering uses).
            // FrameFence was reset but never submitted, so waiting on it would
            // deadlock; and since nothing was submitted the GPU is idle. Skip
            // the wait — ResetCommandPool below recycles the orphaned FrameCmd
            // whatever state it was left in.
            FrameRecording = false;
        }
        else
        {
            // Wait for the previous (submitted) frame to finish using FrameCmd.
            Api.WaitForFences(Device, 1, in fence, true, ulong.MaxValue);
        }
        // The previous frame is now complete — free anything queued for
        // deletion before reusing the command pool / descriptors.
        FlushPendingDeletes();
        Api.ResetFences(Device, 1, in fence);
        Api.ResetCommandPool(Device, GraphicsPool, 0);
        Api.ResetDescriptorPool(Device, TransientDescPool, 0);

        var bi = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
        };
        Api.BeginCommandBuffer(FrameCmd, in bi);
        FrameRecording = true;

        _activeCmd = new VkCommandList(this, FrameCmd);
        return _activeCmd;
    }

    public void Submit(ICommandList commandList)
    {
        if (!FrameRecording) return;
        var cl = (VkCommandList)commandList;
        cl.Finish();
        Api.EndCommandBuffer(FrameCmd);

        // If any swapchain image was acquired during the frame, we need to
        // synchronise present with the render submission.
        var swapchain = cl.AcquiredSwapchain;
        if (swapchain != null)
        {
            var waitSem = ImageAvailable;
            // Signal this image's own "render finished" semaphore. A single
            // shared one is illegal — the previous image's present may still
            // be consuming it (VUID-vkQueueSubmit-pSignalSemaphores-00067).
            var signalSem = swapchain.RenderFinishedSemaphores[swapchain.CurrentImageIndex];
            var waitStage = PipelineStageFlags.ColorAttachmentOutputBit;
            var cb = FrameCmd;
            var si = new SubmitInfo
            {
                SType                = StructureType.SubmitInfo,
                WaitSemaphoreCount   = 1,
                PWaitSemaphores      = &waitSem,
                PWaitDstStageMask    = &waitStage,
                CommandBufferCount   = 1,
                PCommandBuffers      = &cb,
                SignalSemaphoreCount = 1,
                PSignalSemaphores    = &signalSem,
            };
            Api.QueueSubmit(GraphicsQueue, 1, in si, FrameFence);
        }
        else
        {
            var cb = FrameCmd;
            var si = new SubmitInfo
            {
                SType              = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers    = &cb,
            };
            Api.QueueSubmit(GraphicsQueue, 1, in si, FrameFence);
        }
        FrameRecording = false;
        _activeCmd = null;
    }

    public void Present(SwapchainHandle handle)
    {
        var sc = Swapchains[handle.Id];
        if (!sc.ImageAcquired) return;
        // Wait on the same per-image semaphore Submit signalled for this image.
        var renderFinished = sc.RenderFinishedSemaphores[sc.CurrentImageIndex];
        var swap = sc.SwapchainHandle;
        var idx = sc.CurrentImageIndex;
        var pi = new PresentInfoKHR
        {
            SType              = StructureType.PresentInfoKhr,
            WaitSemaphoreCount = 1,
            PWaitSemaphores    = &renderFinished,
            SwapchainCount     = 1,
            PSwapchains        = &swap,
            PImageIndices      = &idx,
        };
        KhrSwapchain.QueuePresent(GraphicsQueue, in pi);
        sc.ImageAcquired = false;
    }
}
