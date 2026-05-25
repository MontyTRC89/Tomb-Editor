using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using TombLib.Rendering.Graphics.Rhi;
using VkFormat    = Silk.NET.Vulkan.Format;
using VkBuffer    = Silk.NET.Vulkan.Buffer;
using BufferDesc  = TombLib.Rendering.Graphics.Rhi.BufferDesc;
using TextureDesc = TombLib.Rendering.Graphics.Rhi.TextureDesc;
using SamplerDesc = TombLib.Rendering.Graphics.Rhi.SamplerDesc;

namespace TombLib.Rendering.Graphics.Backends.Vulkan;

/// <summary>
/// Native Vulkan backend for <see cref="IRhiDevice"/>. Single-threaded:
/// assumes all calls come from the same rendering thread.
///
/// <para>Design notes (kept deliberately simple — see Dx11Device for the
/// reference implementation):
///   - One frame in flight. The CPU waits on the previous frame's fence
///     before recording the next, so there's no need for per-frame command
///     pools / descriptor pools / per-resource staging-buffer rings.
///   - Per-resource memory allocations (vkAllocateMemory / vkBindBufferMemory).
///     Not optimal vs a VMA-style sub-allocator, but the editor allocates a
///     few hundred buffers / textures total — well under the typical Vulkan
///     allocation limit (4096).
///   - One device-shared descriptor-set layout sized to RhiLimits. Every
///     pipeline shares it; SetBindings allocates a fresh descriptor set from
///     the per-frame transient pool (reset at frame start).
///   - Push-constant block: 128 bytes at the VS + FS stages.</para>
///
/// <para>Split across partial files by concern:
///   VkDevice.cs           — device, resources (buffers / textures / samplers).
///   VkDevice.Pipeline.cs  — pipelines and render passes.
///   VkDevice.Swapchain.cs — surface and swapchain.</para>
/// </summary>
public unsafe sealed partial class VkDevice : IRhiDevice
{
    internal readonly Vk        Api;
    internal Instance           Instance;
    internal PhysicalDevice     PhysicalDevice;
    internal Device             Device;
    internal uint               GraphicsQueueFamily;
    internal Queue              GraphicsQueue;

    internal KhrSurface      KhrSurface;
    internal KhrSwapchain    KhrSwapchain;
    internal KhrWin32Surface KhrWin32Surface;
    internal Silk.NET.Vulkan.Extensions.EXT.ExtDebugUtils? DebugUtils;
    internal DebugUtilsMessengerEXT DebugMessenger;
    // Hold the callback delegate so the GC doesn't collect it while Vulkan
    // still holds the function pointer.
    internal DebugUtilsMessengerCallbackFunctionEXT? DebugCallback;

    // Device capabilities cached after device creation — used by every
    // FindMemoryType call and the Capabilities query.
    internal PhysicalDeviceMemoryProperties MemoryProperties;
    internal PhysicalDeviceLimits           Limits;
    internal PhysicalDeviceFeatures         Features;

    // Shared descriptor / pipeline layout. All RHI pipelines use this single
    // descriptor model:
    //   set 0:
    //     b0..b3   : MaxConstantBuffers uniform buffers  (VS + FS)
    //     t0..t7   : MaxTextureBindings sampled images   (VS + FS)
    //     s0..s3   : MaxSamplerBindings samplers         (VS + FS)
    //     ssbo0..1 : MaxStorageBuffers storage buffers   (VS + FS)
    //   push constants: 128 bytes at offset 0, stages = VS | FS
    internal DescriptorSetLayout SharedDescLayout;
    internal PipelineLayout      SharedPipelineLayout;

    // Per-frame resources. With one frame in flight there's just one of each.
    internal CommandPool    GraphicsPool;
    internal CommandBuffer  FrameCmd;
    internal Fence          FrameFence;
    internal Semaphore      ImageAvailable;
    internal DescriptorPool TransientDescPool;
    internal bool           FrameRecording;   // true between BeginCommandList and Submit

    // Swapchain MSAA sample count. Rendering targets an off-screen 4× colour +
    // depth image which the render pass resolves into the single-sample
    // swapchain image at EndPass. The Vulkan spec guarantees both
    // framebufferColorSampleCounts and framebufferDepthSampleCounts include
    // 1× and 4×, so this needs no capability probe. Matches the DX11 backend.
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

    // =============================================================== Instance

    private void CreateInstance()
    {
        var appInfo = new ApplicationInfo
        {
            SType              = StructureType.ApplicationInfo,
            PApplicationName   = (byte*)Marshal.StringToHGlobalAnsi("TombEditor"),
            ApplicationVersion = 1,
            PEngineName        = (byte*)Marshal.StringToHGlobalAnsi("TombEditor"),
            EngineVersion      = 1,
            ApiVersion         = Vk.Version11,
        };

        // Validation is opt-out (TOMBEDITOR_VK_VALIDATION=0 disables) when the
        // Khronos validation layer is installed. Captured messages go to
        // %TEMP%\TombEditorVk.log via a debug-utils messenger so the real
        // driver / spec-violation text is available when an API call fails.
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
        DebugLog($"VkDevice init: validation={(wantValidation ? "ON" : "off")}, " +
                 $"layers={layers.Count}, extensions={extensions.Count}");

        var extensionPtrs = new byte*[extensions.Count];
        var layerPtrs     = new byte*[layers.Count];
        try
        {
            for (int i = 0; i < extensions.Count; i++)
                extensionPtrs[i] = (byte*)Marshal.StringToHGlobalAnsi(extensions[i]);
            for (int i = 0; i < layers.Count; i++)
                layerPtrs[i] = (byte*)Marshal.StringToHGlobalAnsi(layers[i]);

            fixed (byte** ppExtensions = extensionPtrs)
            fixed (byte** ppLayers     = layerPtrs)
            {
                var instanceInfo = new InstanceCreateInfo
                {
                    SType                   = StructureType.InstanceCreateInfo,
                    PApplicationInfo        = &appInfo,
                    EnabledExtensionCount   = (uint)extensions.Count,
                    PpEnabledExtensionNames = ppExtensions,
                    EnabledLayerCount       = (uint)layers.Count,
                    PpEnabledLayerNames     = ppLayers,
                };
                if (Api.CreateInstance(in instanceInfo, null, out Instance) != Result.Success)
                    throw new InvalidOperationException("vkCreateInstance failed");
            }
        }
        finally
        {
            for (int i = 0; i < extensionPtrs.Length; i++)
                if (extensionPtrs[i] != null) Marshal.FreeHGlobal((IntPtr)extensionPtrs[i]);
            for (int i = 0; i < layerPtrs.Length; i++)
                if (layerPtrs[i] != null) Marshal.FreeHGlobal((IntPtr)layerPtrs[i]);
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
        if (wantValidation &&
            Api.TryGetInstanceExtension(Instance, out Silk.NET.Vulkan.Extensions.EXT.ExtDebugUtils debugUtils))
        {
            DebugUtils    = debugUtils;
            DebugCallback = DebugMessageCallback;
            var messengerInfo = new DebugUtilsMessengerCreateInfoEXT
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
            DebugUtils.CreateDebugUtilsMessenger(Instance, in messengerInfo, null, out DebugMessenger);
            DebugLog("VkDevice debug messenger attached");
        }
    }

    private static uint DebugMessageCallback(
        DebugUtilsMessageSeverityFlagsEXT severity,
        DebugUtilsMessageTypeFlagsEXT type,
        DebugUtilsMessengerCallbackDataEXT* data,
        void* userData)
    {
        string message = data != null && data->PMessage != null
                         ? Marshal.PtrToStringAnsi((IntPtr)data->PMessage) ?? "(no message)"
                         : "(no data)";
        DebugLog($"VK [{severity}] {message}");
        return 0u; // VK_FALSE: don't abort the call
    }

    private bool IsLayerAvailable(string name)
    {
        uint count = 0;
        Api.EnumerateInstanceLayerProperties(ref count, null);
        if (count == 0) return false;

        var layerProperties = new LayerProperties[count];
        fixed (LayerProperties* pLayerProperties = layerProperties)
            Api.EnumerateInstanceLayerProperties(ref count, pLayerProperties);

        for (int i = 0; i < layerProperties.Length; i++)
        {
            fixed (byte* pName = layerProperties[i].LayerName)
            {
                string layerName = Marshal.PtrToStringAnsi((IntPtr)pName) ?? "";
                if (layerName == name) return true;
            }
        }
        return false;
    }

    // ============================================ Physical / logical device

    private void PickPhysicalDevice()
    {
        uint count = 0;
        Api.EnumeratePhysicalDevices(Instance, ref count, null);
        if (count == 0) throw new InvalidOperationException("No Vulkan physical devices");

        var devices = new PhysicalDevice[count];
        fixed (PhysicalDevice* pDevices = devices)
            Api.EnumeratePhysicalDevices(Instance, ref count, pDevices);

        PhysicalDevice best = default;
        int bestScore = -1;
        for (int i = 0; i < devices.Length; i++)
        {
            var candidate = devices[i];
            Api.GetPhysicalDeviceProperties(candidate, out var properties);
            // We always need a graphics queue and KHR_swapchain support;
            // score discrete GPU > integrated > anything else.
            if (!HasGraphicsQueue(candidate)) continue;
            if (!HasExtension(candidate, "VK_KHR_swapchain")) continue;
            int score = properties.DeviceType switch
            {
                PhysicalDeviceType.DiscreteGpu   => 1000,
                PhysicalDeviceType.IntegratedGpu => 500,
                PhysicalDeviceType.VirtualGpu    => 100,
                _                                => 10,
            };
            if (score > bestScore) { best = candidate; bestScore = score; }
        }
        if (bestScore < 0)
            throw new InvalidOperationException("No suitable Vulkan device (need graphics + swapchain)");

        PhysicalDevice = best;
        Api.GetPhysicalDeviceMemoryProperties(PhysicalDevice, out MemoryProperties);
        Api.GetPhysicalDeviceProperties(PhysicalDevice, out var deviceProperties);
        Limits = deviceProperties.Limits;
        Api.GetPhysicalDeviceFeatures(PhysicalDevice, out Features);
    }

    private bool HasGraphicsQueue(PhysicalDevice device)
    {
        uint count = 0;
        Api.GetPhysicalDeviceQueueFamilyProperties(device, ref count, null);
        var queueFamilies = new QueueFamilyProperties[count];
        fixed (QueueFamilyProperties* pQueueFamilies = queueFamilies)
            Api.GetPhysicalDeviceQueueFamilyProperties(device, ref count, pQueueFamilies);

        for (int i = 0; i < queueFamilies.Length; i++)
            if ((queueFamilies[i].QueueFlags & QueueFlags.GraphicsBit) != 0)
                return true;
        return false;
    }

    private bool HasExtension(PhysicalDevice device, string name)
    {
        uint count = 0;
        Api.EnumerateDeviceExtensionProperties(device, (byte*)null, ref count, null);
        var extensionProperties = new ExtensionProperties[count];
        fixed (ExtensionProperties* pExtensionProperties = extensionProperties)
            Api.EnumerateDeviceExtensionProperties(device, (byte*)null, ref count, pExtensionProperties);

        for (int i = 0; i < extensionProperties.Length; i++)
        {
            fixed (byte* pName = extensionProperties[i].ExtensionName)
            {
                string extensionName = Marshal.PtrToStringAnsi((IntPtr)pName) ?? "";
                if (extensionName == name) return true;
            }
        }
        return false;
    }

    private void CreateLogicalDevice()
    {
        // Pick the first graphics + present queue family. On Windows the
        // graphics queue is always presentation-capable on consumer GPUs, so
        // there's no need to query per-surface presentation support before
        // device creation (which would require a dummy surface).
        uint count = 0;
        Api.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, ref count, null);
        var queueFamilies = new QueueFamilyProperties[count];
        fixed (QueueFamilyProperties* pQueueFamilies = queueFamilies)
            Api.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, ref count, pQueueFamilies);

        GraphicsQueueFamily = uint.MaxValue;
        for (uint i = 0; i < queueFamilies.Length; i++)
            if ((queueFamilies[i].QueueFlags & QueueFlags.GraphicsBit) != 0)
            {
                GraphicsQueueFamily = i;
                break;
            }
        if (GraphicsQueueFamily == uint.MaxValue)
            throw new InvalidOperationException("No graphics queue family");

        float queuePriority = 1.0f;
        var queueInfo = new DeviceQueueCreateInfo
        {
            SType            = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = GraphicsQueueFamily,
            QueueCount       = 1,
            PQueuePriorities = &queuePriority,
        };

        var deviceExtensions    = new[] { "VK_KHR_swapchain" };
        var deviceExtensionPtrs = new byte*[deviceExtensions.Length];
        for (int i = 0; i < deviceExtensions.Length; i++)
            deviceExtensionPtrs[i] = (byte*)Marshal.StringToHGlobalAnsi(deviceExtensions[i]);

        var enabledFeatures = new PhysicalDeviceFeatures
        {
            SamplerAnisotropy = Features.SamplerAnisotropy,
            FillModeNonSolid  = Features.FillModeNonSolid, // wireframe
        };

        try
        {
            fixed (byte** ppDeviceExtensions = deviceExtensionPtrs)
            {
                var deviceInfo = new DeviceCreateInfo
                {
                    SType                   = StructureType.DeviceCreateInfo,
                    QueueCreateInfoCount    = 1,
                    PQueueCreateInfos       = &queueInfo,
                    EnabledExtensionCount   = (uint)deviceExtensions.Length,
                    PpEnabledExtensionNames = ppDeviceExtensions,
                    PEnabledFeatures        = &enabledFeatures,
                };
                if (Api.CreateDevice(PhysicalDevice, in deviceInfo, null, out Device) != Result.Success)
                    throw new InvalidOperationException("vkCreateDevice failed");
            }
        }
        finally
        {
            for (int i = 0; i < deviceExtensionPtrs.Length; i++)
                Marshal.FreeHGlobal((IntPtr)deviceExtensionPtrs[i]);
        }

        Api.GetDeviceQueue(Device, GraphicsQueueFamily, 0, out GraphicsQueue);
        if (!Api.TryGetDeviceExtension(Instance, Device, out KhrSwapchain))
            throw new InvalidOperationException("VK_KHR_swapchain not available on device");
    }

    // ========================================================= Memory helper

    internal uint FindMemoryType(uint typeBits, MemoryPropertyFlags required)
    {
        for (uint i = 0; i < MemoryProperties.MemoryTypeCount; i++)
        {
            if ((typeBits & (1u << (int)i)) == 0) continue;
            if ((MemoryProperties.MemoryTypes[(int)i].PropertyFlags & required) == required)
                return i;
        }
        throw new InvalidOperationException($"No suitable memory type for required flags {required}");
    }

    internal DeviceMemory AllocMemory(ulong size, uint typeBits, MemoryPropertyFlags required)
    {
        var allocInfo = new MemoryAllocateInfo
        {
            SType           = StructureType.MemoryAllocateInfo,
            AllocationSize  = size,
            MemoryTypeIndex = FindMemoryType(typeBits, required),
        };
        if (Api.AllocateMemory(Device, in allocInfo, null, out var memory) != Result.Success)
            throw new InvalidOperationException("vkAllocateMemory failed");
        return memory;
    }

    // ========================================================= Shared layouts

    private void CreateSharedDescriptorLayout()
    {
        // Layout matches what the shaders declare via VK_BINDING:
        //   binding 0 = ViewParams cbuffer
        //   binding 1 = Atlas texture
        //   binding 2 = Atlas sampler
        // The C# side uses ConstantBuffers[0] / Textures[0] / Samplers[0]
        // respectively. If a future shader needs a second cbuf / texture,
        // extend this layout *and* the VK_BINDING numbers in the HLSL.
        var layoutBindings = stackalloc DescriptorSetLayoutBinding[3]
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
        var layoutInfo = new DescriptorSetLayoutCreateInfo
        {
            SType        = StructureType.DescriptorSetLayoutCreateInfo,
            BindingCount = 3,
            PBindings    = layoutBindings,
        };
        if (Api.CreateDescriptorSetLayout(Device, in layoutInfo, null, out SharedDescLayout) != Result.Success)
            throw new InvalidOperationException("vkCreateDescriptorSetLayout failed");

        var pushConstantRange = new PushConstantRange
        {
            StageFlags = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            Offset     = 0,
            Size       = (uint)RhiLimits.PushConstantSize,
        };
        var setLayout = SharedDescLayout;
        var pipelineLayoutInfo = new PipelineLayoutCreateInfo
        {
            SType                  = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount         = 1,
            PSetLayouts            = &setLayout,
            PushConstantRangeCount = 1,
            PPushConstantRanges    = &pushConstantRange,
        };
        if (Api.CreatePipelineLayout(Device, in pipelineLayoutInfo, null, out SharedPipelineLayout) != Result.Success)
            throw new InvalidOperationException("vkCreatePipelineLayout failed");
    }

    private void CreateFrameResources()
    {
        var commandPoolInfo = new CommandPoolCreateInfo
        {
            SType            = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = GraphicsQueueFamily,
            Flags            = CommandPoolCreateFlags.ResetCommandBufferBit,
        };
        if (Api.CreateCommandPool(Device, in commandPoolInfo, null, out GraphicsPool) != Result.Success)
            throw new InvalidOperationException("vkCreateCommandPool failed");

        var commandBufferAllocInfo = new CommandBufferAllocateInfo
        {
            SType              = StructureType.CommandBufferAllocateInfo,
            CommandPool        = GraphicsPool,
            Level              = CommandBufferLevel.Primary,
            CommandBufferCount = 1,
        };
        CommandBuffer commandBuffer = default;
        if (Api.AllocateCommandBuffers(Device, in commandBufferAllocInfo, &commandBuffer) != Result.Success)
            throw new InvalidOperationException("vkAllocateCommandBuffers failed");
        FrameCmd = commandBuffer;

        var fenceInfo = new FenceCreateInfo
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit, // start signalled so the first WaitForFences passes
        };
        Api.CreateFence(Device, in fenceInfo, null, out FrameFence);

        // ImageAvailable is one shared acquire semaphore — safe with a single
        // frame in flight (BeginCommandList waits FrameFence before the next
        // acquire). The "render finished" semaphores are per swapchain image
        // and live on VkSwapchainRes; see CreateSwapchainResources.
        var semaphoreInfo = new SemaphoreCreateInfo { SType = StructureType.SemaphoreCreateInfo };
        Api.CreateSemaphore(Device, in semaphoreInfo, null, out ImageAvailable);

        // Transient descriptor pool — sized to one descriptor of each type per
        // allocation × MaxSets. SetBindings allocates a fresh descriptor set
        // every call within a frame; the pool resets at frame start.
        const int setsPerFrame = 1024;
        var poolSizes = stackalloc DescriptorPoolSize[3]
        {
            new() { Type = DescriptorType.UniformBuffer, DescriptorCount = setsPerFrame },
            new() { Type = DescriptorType.SampledImage,  DescriptorCount = setsPerFrame },
            new() { Type = DescriptorType.Sampler,       DescriptorCount = setsPerFrame },
        };
        var descriptorPoolInfo = new DescriptorPoolCreateInfo
        {
            SType         = StructureType.DescriptorPoolCreateInfo,
            MaxSets       = setsPerFrame,
            PoolSizeCount = 3,
            PPoolSizes    = poolSizes,
            Flags         = 0, // no individual free — the whole pool resets each frame
        };
        if (Api.CreateDescriptorPool(Device, in descriptorPoolInfo, null, out TransientDescPool) != Result.Success)
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
        foreach (var deleteAction in _pendingDeletes) deleteAction();
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

        foreach (var swapchain in Swapchains.Values) DestroySwapchainInternal(swapchain);
        foreach (var pipeline  in Pipelines.Values)  DestroyPipelineInternal(pipeline);
        foreach (var sampler   in Samplers.Values)   Api.DestroySampler(Device, sampler.Handle, null);
        foreach (var texture   in Textures.Values)   DestroyTextureInternal(texture);
        foreach (var buffer    in Buffers.Values)    DestroyBufferInternal(buffer);
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

    // ================================================================ Buffers

    public BufferHandle CreateBuffer(in BufferDesc desc, ReadOnlySpan<byte> initialData)
    {
        bool isDynamic = desc.Usage == BufferUsage.DynamicUniform || desc.Usage == BufferUsage.DynamicVertex;

        // Usage flags for the GPU-side buffer.
        BufferUsageFlags usageFlags = 0;
        if ((desc.BindFlags & BufferBindFlags.Vertex)     != 0) usageFlags |= BufferUsageFlags.VertexBufferBit;
        if ((desc.BindFlags & BufferBindFlags.Index)      != 0) usageFlags |= BufferUsageFlags.IndexBufferBit;
        if ((desc.BindFlags & BufferBindFlags.Constant)   != 0) usageFlags |= BufferUsageFlags.UniformBufferBit;
        if ((desc.BindFlags & BufferBindFlags.Structured) != 0) usageFlags |= BufferUsageFlags.StorageBufferBit;
        // Every buffer also needs TransferDst so it can receive a staging copy
        // (immutable + initialData, and the dynamic UpdateBuffer fallback).
        usageFlags |= BufferUsageFlags.TransferDstBit;

        var bufferInfo = new BufferCreateInfo
        {
            SType       = StructureType.BufferCreateInfo,
            Size        = (ulong)desc.SizeBytes,
            Usage       = usageFlags,
            SharingMode = SharingMode.Exclusive,
        };
        if (Api.CreateBuffer(Device, in bufferInfo, null, out var buffer) != Result.Success)
            throw new InvalidOperationException("vkCreateBuffer failed");
        Api.GetBufferMemoryRequirements(Device, buffer, out var memoryReq);

        // Dynamic buffers go in HOST_VISIBLE | HOST_COHERENT memory so the CPU
        // can map them persistently; immutable buffers go in DEVICE_LOCAL.
        var memoryFlags = isDynamic
                          ? MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit
                          : MemoryPropertyFlags.DeviceLocalBit;
        var memory = AllocMemory(memoryReq.Size, memoryReq.MemoryTypeBits, memoryFlags);
        Api.BindBufferMemory(Device, buffer, memory, 0);

        void* mapped = null;
        if (isDynamic)
        {
            void* mappedPtr;
            Api.MapMemory(Device, memory, 0, memoryReq.Size, 0, &mappedPtr);
            mapped = mappedPtr;
        }

        var bufferRes = new VkBufferRes
        {
            Handle    = buffer,
            Memory    = memory,
            Size      = (ulong)desc.SizeBytes,
            Mapped    = mapped,
            Usage     = desc.Usage,
            BindFlags = desc.BindFlags,
        };

        // Initial upload — dynamic: copy into the mapped region; immutable:
        // staging buffer + copy command.
        if (initialData.Length > 0)
        {
            if (isDynamic)
            {
                fixed (byte* src = initialData)
                    System.Buffer.MemoryCopy(src, mapped, (long)bufferRes.Size, initialData.Length);
            }
            else
            {
                UploadImmutable(bufferRes, initialData);
            }
        }

        uint id = AllocHandle();
        Buffers[id] = bufferRes;
        return new BufferHandle(id);
    }

    private void UploadImmutable(VkBufferRes destination, ReadOnlySpan<byte> data)
    {
        // Staging buffer in host memory + one-shot copy command.
        var stagingInfo = new BufferCreateInfo
        {
            SType       = StructureType.BufferCreateInfo,
            Size        = (ulong)data.Length,
            Usage       = BufferUsageFlags.TransferSrcBit,
            SharingMode = SharingMode.Exclusive,
        };
        Api.CreateBuffer(Device, in stagingInfo, null, out var staging);
        Api.GetBufferMemoryRequirements(Device, staging, out var stagingMemoryReq);
        var stagingMemory = AllocMemory(stagingMemoryReq.Size, stagingMemoryReq.MemoryTypeBits,
                                        MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);
        Api.BindBufferMemory(Device, staging, stagingMemory, 0);

        void* mappedPtr;
        Api.MapMemory(Device, stagingMemory, 0, stagingMemoryReq.Size, 0, &mappedPtr);
        fixed (byte* src = data)
            System.Buffer.MemoryCopy(src, mappedPtr, (long)stagingMemoryReq.Size, data.Length);
        Api.UnmapMemory(Device, stagingMemory);

        var commandBuffer = OneShotBegin();
        var region = new BufferCopy { Size = (ulong)data.Length };
        Api.CmdCopyBuffer(commandBuffer, staging, destination.Handle, 1, in region);
        OneShotEndSubmitWait(commandBuffer);

        Api.DestroyBuffer(Device, staging, null);
        Api.FreeMemory(Device, stagingMemory, null);
    }

    public void Destroy(BufferHandle handle)
    {
        if (Buffers.Remove(handle.Id, out var buffer))
            _pendingDeletes.Add(() => DestroyBufferInternal(buffer));
    }

    private void DestroyBufferInternal(VkBufferRes buffer)
    {
        if (buffer.Mapped != null) Api.UnmapMemory(Device, buffer.Memory);
        Api.DestroyBuffer(Device, buffer.Handle, null);
        Api.FreeMemory(Device, buffer.Memory, null);
    }

    // =============================================================== Textures

    public TextureHandle CreateTexture(in TextureDesc desc, ReadOnlySpan<byte> initialData)
    {
        bool hasColor  = (desc.BindFlags & TextureBindFlags.RenderTarget)   != 0;
        bool hasDepth  = (desc.BindFlags & TextureBindFlags.DepthStencil)   != 0;
        bool hasShader = (desc.BindFlags & TextureBindFlags.ShaderResource) != 0;

        var aspect     = VkMapping.AspectOf(desc.Format);
        var imageUsage = ImageUsageFlags.None;
        if (hasShader) imageUsage |= ImageUsageFlags.SampledBit;
        if (hasColor)  imageUsage |= ImageUsageFlags.ColorAttachmentBit;
        if (hasDepth)  imageUsage |= ImageUsageFlags.DepthStencilAttachmentBit;
        // Always allow TransferDst. It is needed for create-time uploads
        // (below) AND for every later UpdateTexture call — and UpdateTexture
        // routinely targets a texture created with no initial data (the
        // preview / WAD-thumbnail atlases create empty on purpose, then fill
        // regions incrementally). Gating this on initialData made
        // vkCmdCopyBufferToImage fail validation (VUID-...-dstImage-00177) on
        // those atlases, which on a strict driver loses the device and hangs
        // the next frame's WaitForFences. It is free on a texture that is
        // never copied into — exactly like TransferSrc just below.
        imageUsage |= ImageUsageFlags.TransferDstBit;
        // Always allow TransferSrc so ReadTexture (thumbnail capture) can copy
        // out of any texture; vkCmdCopyImageToBuffer needs it.
        imageUsage |= ImageUsageFlags.TransferSrcBit;

        int mipLevels = Math.Max(1, desc.MipLevels);
        var imageInfo = new ImageCreateInfo
        {
            SType         = StructureType.ImageCreateInfo,
            ImageType     = ImageType.Type2D,
            Format        = VkMapping.ToVk(desc.Format),
            Extent        = new Extent3D((uint)desc.Width, (uint)desc.Height, 1),
            MipLevels     = (uint)mipLevels,
            ArrayLayers   = (uint)desc.ArrayLayers,
            Samples       = VkMapping.ToSampleCount(desc.Samples),
            Tiling        = ImageTiling.Optimal,
            Usage         = imageUsage,
            SharingMode   = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Undefined,
            Flags         = desc.Kind == TextureKind.TextureCube ? ImageCreateFlags.CreateCubeCompatibleBit : 0,
        };
        if (Api.CreateImage(Device, in imageInfo, null, out var image) != Result.Success)
            throw new InvalidOperationException("vkCreateImage failed");
        Api.GetImageMemoryRequirements(Device, image, out var memoryReq);
        var memory = AllocMemory(memoryReq.Size, memoryReq.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit);
        Api.BindImageMemory(Device, image, memory, 0);

        var view = CreateImageView(image, VkMapping.ToVk(desc.Format), aspect, (uint)mipLevels);

        var textureRes = new VkTextureRes
        {
            Image         = image,
            View          = view,
            Memory        = memory,
            Format        = desc.Format,
            Width         = desc.Width,
            Height        = desc.Height,
            MipLevels     = mipLevels,
            ArrayLayers   = desc.ArrayLayers,
            Samples       = desc.Samples,
            BindFlags     = desc.BindFlags,
            Aspect        = aspect,
            CurrentLayout = ImageLayout.Undefined,
        };

        if (initialData.Length > 0)
        {
            UploadTextureMip0(textureRes, initialData);
        }
        else
        {
            // Transition to a stable layout so the first use as a shader
            // resource doesn't validate as Undefined.
            if (hasShader)
                TransitionImage(textureRes, ImageLayout.ShaderReadOnlyOptimal);
            else if (hasColor)
                TransitionImage(textureRes, ImageLayout.ColorAttachmentOptimal);
            else if (hasDepth)
                TransitionImage(textureRes, ImageLayout.DepthStencilAttachmentOptimal);
        }

        uint id = AllocHandle();
        Textures[id] = textureRes;
        return new TextureHandle(id);
    }

    private ImageView CreateImageView(Image image, VkFormat format, ImageAspectFlags aspect, uint mipLevels)
    {
        var viewInfo = new ImageViewCreateInfo
        {
            SType    = StructureType.ImageViewCreateInfo,
            Image    = image,
            ViewType = ImageViewType.Type2D,
            Format   = format,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask     = aspect,
                BaseMipLevel   = 0,
                LevelCount     = mipLevels,
                BaseArrayLayer = 0,
                LayerCount     = 1,
            },
        };
        Api.CreateImageView(Device, in viewInfo, null, out var view);
        return view;
    }

    private void UploadTextureMip0(VkTextureRes texture, ReadOnlySpan<byte> data)
    {
        int   bytesPerPixel = VkMapping.BytesPerPixel(texture.Format);
        ulong size          = (ulong)(texture.Width * texture.Height * bytesPerPixel);

        var stagingInfo = new BufferCreateInfo
        {
            SType       = StructureType.BufferCreateInfo,
            Size        = size,
            Usage       = BufferUsageFlags.TransferSrcBit,
            SharingMode = SharingMode.Exclusive,
        };
        Api.CreateBuffer(Device, in stagingInfo, null, out var staging);
        Api.GetBufferMemoryRequirements(Device, staging, out var stagingMemoryReq);
        var stagingMemory = AllocMemory(stagingMemoryReq.Size, stagingMemoryReq.MemoryTypeBits,
                                        MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);
        Api.BindBufferMemory(Device, staging, stagingMemory, 0);

        void* mappedPtr;
        Api.MapMemory(Device, stagingMemory, 0, stagingMemoryReq.Size, 0, &mappedPtr);
        fixed (byte* src = data)
            System.Buffer.MemoryCopy(src, mappedPtr, (long)stagingMemoryReq.Size, Math.Min(data.Length, (int)size));
        Api.UnmapMemory(Device, stagingMemory);

        var commandBuffer = OneShotBegin();
        TransitionImage(commandBuffer, texture, ImageLayout.TransferDstOptimal);
        var region = new BufferImageCopy
        {
            BufferOffset      = 0,
            BufferRowLength   = 0,
            BufferImageHeight = 0,
            ImageSubresource  = new ImageSubresourceLayers
            {
                AspectMask = texture.Aspect, MipLevel = 0, BaseArrayLayer = 0, LayerCount = 1,
            },
            ImageOffset = new Offset3D(0, 0, 0),
            ImageExtent = new Extent3D((uint)texture.Width, (uint)texture.Height, 1),
        };
        Api.CmdCopyBufferToImage(commandBuffer, staging, texture.Image, ImageLayout.TransferDstOptimal, 1, in region);
        TransitionImage(commandBuffer, texture, ImageLayout.ShaderReadOnlyOptimal);
        OneShotEndSubmitWait(commandBuffer);

        Api.DestroyBuffer(Device, staging, null);
        Api.FreeMemory(Device, stagingMemory, null);
    }

    public void UpdateTexture(TextureHandle handle, int subresource,
                              int x, int y, int width, int height,
                              int rowPitchBytes, ReadOnlySpan<byte> data)
    {
        var texture = Textures[handle.Id];
        // Build a tightly-packed staging copy. The caller's rowPitch may be
        // larger than width*bpp on D3D, but in practice TombEditor never sends
        // padded rows here — keep it simple.
        int   bytesPerPixel = VkMapping.BytesPerPixel(texture.Format);
        ulong size          = (ulong)(width * height * bytesPerPixel);

        var stagingInfo = new BufferCreateInfo
        {
            SType       = StructureType.BufferCreateInfo,
            Size        = size,
            Usage       = BufferUsageFlags.TransferSrcBit,
            SharingMode = SharingMode.Exclusive,
        };
        Api.CreateBuffer(Device, in stagingInfo, null, out var staging);
        Api.GetBufferMemoryRequirements(Device, staging, out var stagingMemoryReq);
        var stagingMemory = AllocMemory(stagingMemoryReq.Size, stagingMemoryReq.MemoryTypeBits,
                                        MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);
        Api.BindBufferMemory(Device, staging, stagingMemory, 0);

        void* mappedPtr;
        Api.MapMemory(Device, stagingMemory, 0, stagingMemoryReq.Size, 0, &mappedPtr);
        // Copy row by row, dropping any source padding.
        fixed (byte* src = data)
        {
            for (int row = 0; row < height; row++)
                System.Buffer.MemoryCopy(src + row * rowPitchBytes,
                                         (byte*)mappedPtr + row * width * bytesPerPixel,
                                         width * bytesPerPixel, width * bytesPerPixel);
        }
        Api.UnmapMemory(Device, stagingMemory);

        var commandBuffer = OneShotBegin();
        TransitionImage(commandBuffer, texture, ImageLayout.TransferDstOptimal);
        var region = new BufferImageCopy
        {
            ImageSubresource = new ImageSubresourceLayers
            {
                AspectMask = texture.Aspect, MipLevel = (uint)subresource,
                BaseArrayLayer = 0, LayerCount = 1,
            },
            ImageOffset = new Offset3D(x, y, 0),
            ImageExtent = new Extent3D((uint)width, (uint)height, 1),
        };
        Api.CmdCopyBufferToImage(commandBuffer, staging, texture.Image, ImageLayout.TransferDstOptimal, 1, in region);
        TransitionImage(commandBuffer, texture, ImageLayout.ShaderReadOnlyOptimal);
        OneShotEndSubmitWait(commandBuffer);

        Api.DestroyBuffer(Device, staging, null);
        Api.FreeMemory(Device, stagingMemory, null);
    }

    public byte[] ReadTexture(TextureHandle handle, int subresource = 0)
    {
        var texture = Textures[handle.Id];
        int   bytesPerPixel = VkMapping.BytesPerPixel(texture.Format);
        ulong size          = (ulong)(texture.Width * texture.Height * bytesPerPixel);
        byte[] result       = new byte[size];

        var stagingInfo = new BufferCreateInfo
        {
            SType       = StructureType.BufferCreateInfo,
            Size        = size,
            Usage       = BufferUsageFlags.TransferDstBit,
            SharingMode = SharingMode.Exclusive,
        };
        Api.CreateBuffer(Device, in stagingInfo, null, out var staging);
        Api.GetBufferMemoryRequirements(Device, staging, out var stagingMemoryReq);
        var stagingMemory = AllocMemory(stagingMemoryReq.Size, stagingMemoryReq.MemoryTypeBits,
                                        MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);
        Api.BindBufferMemory(Device, staging, stagingMemory, 0);

        var commandBuffer = OneShotBegin();
        var previousLayout = texture.CurrentLayout;
        TransitionImage(commandBuffer, texture, ImageLayout.TransferSrcOptimal);
        var region = new BufferImageCopy
        {
            ImageSubresource = new ImageSubresourceLayers
            {
                AspectMask = texture.Aspect, MipLevel = (uint)subresource,
                BaseArrayLayer = 0, LayerCount = 1,
            },
            ImageOffset = new Offset3D(0, 0, 0),
            ImageExtent = new Extent3D((uint)texture.Width, (uint)texture.Height, 1),
        };
        Api.CmdCopyImageToBuffer(commandBuffer, texture.Image, ImageLayout.TransferSrcOptimal, staging, 1, in region);
        // Restore to a sensible post-read layout: shader-read for sampled
        // textures, otherwise the previous layout the caller relied on.
        var restoreLayout = (texture.BindFlags & TextureBindFlags.ShaderResource) != 0
                            ? ImageLayout.ShaderReadOnlyOptimal
                            : previousLayout != ImageLayout.Undefined ? previousLayout : ImageLayout.General;
        TransitionImage(commandBuffer, texture, restoreLayout);
        OneShotEndSubmitWait(commandBuffer);

        void* mappedPtr;
        Api.MapMemory(Device, stagingMemory, 0, size, 0, &mappedPtr);
        fixed (byte* dst = result)
            System.Buffer.MemoryCopy(mappedPtr, dst, (long)size, (long)size);
        Api.UnmapMemory(Device, stagingMemory);

        Api.DestroyBuffer(Device, staging, null);
        Api.FreeMemory(Device, stagingMemory, null);
        return result;
    }

    public void Destroy(TextureHandle handle)
    {
        if (Textures.Remove(handle.Id, out var texture))
            _pendingDeletes.Add(() => DestroyTextureInternal(texture));
    }

    private void DestroyTextureInternal(VkTextureRes texture)
    {
        if (texture.View.Handle   != 0) Api.DestroyImageView(Device, texture.View, null);
        if (texture.Image.Handle  != 0) Api.DestroyImage(Device, texture.Image, null);
        if (texture.Memory.Handle != 0) Api.FreeMemory(Device, texture.Memory, null);
    }

    // One-shot variant — begins / submits / waits its own command buffer.
    internal void TransitionImage(VkTextureRes texture, ImageLayout newLayout)
    {
        var commandBuffer = OneShotBegin();
        TransitionImage(commandBuffer, texture, newLayout);
        OneShotEndSubmitWait(commandBuffer);
    }

    internal void TransitionImage(CommandBuffer commandBuffer, VkTextureRes texture, ImageLayout newLayout)
    {
        if (texture.CurrentLayout == newLayout) return;
        var barrier = new ImageMemoryBarrier
        {
            SType               = StructureType.ImageMemoryBarrier,
            OldLayout           = texture.CurrentLayout,
            NewLayout           = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image               = texture.Image,
            SubresourceRange    = new ImageSubresourceRange
            {
                AspectMask     = texture.Aspect,
                BaseMipLevel   = 0, LevelCount = (uint)texture.MipLevels,
                BaseArrayLayer = 0, LayerCount = (uint)texture.ArrayLayers,
            },
        };
        // Conservative access masks — the AllCommands stage covers everything;
        // overkill but simple.
        barrier.SrcAccessMask = AccessFlags.MemoryReadBit | AccessFlags.MemoryWriteBit;
        barrier.DstAccessMask = AccessFlags.MemoryReadBit | AccessFlags.MemoryWriteBit;
        Api.CmdPipelineBarrier(commandBuffer,
                               PipelineStageFlags.AllCommandsBit, PipelineStageFlags.AllCommandsBit, 0,
                               0, null, 0, null, 1, in barrier);
        texture.CurrentLayout = newLayout;
    }

    // =============================================================== Samplers

    public SamplerHandle CreateSampler(in SamplerDesc desc)
    {
        bool anisotropyEnabled = desc.MinFilter == FilterMode.Anisotropic && Features.SamplerAnisotropy;
        var samplerInfo = new SamplerCreateInfo
        {
            SType            = StructureType.SamplerCreateInfo,
            MagFilter        = desc.MagFilter == FilterMode.Nearest ? Filter.Nearest : Filter.Linear,
            MinFilter        = desc.MinFilter == FilterMode.Nearest ? Filter.Nearest : Filter.Linear,
            MipmapMode       = desc.MipFilter == FilterMode.Nearest ? SamplerMipmapMode.Nearest : SamplerMipmapMode.Linear,
            AddressModeU     = VkMapping.ToVk(desc.AddressU),
            AddressModeV     = VkMapping.ToVk(desc.AddressV),
            AddressModeW     = VkMapping.ToVk(desc.AddressW),
            MipLodBias       = 0,
            AnisotropyEnable = anisotropyEnabled,
            MaxAnisotropy    = anisotropyEnabled
                               ? Math.Min(desc.MaxAnisotropy, (int)Limits.MaxSamplerAnisotropy)
                               : 1,
            CompareEnable    = false,
            CompareOp        = Silk.NET.Vulkan.CompareOp.Never,
            MinLod           = 0,
            MaxLod           = Vk.LodClampNone,
            BorderColor      = BorderColor.FloatTransparentBlack,
            UnnormalizedCoordinates = false,
        };
        Api.CreateSampler(Device, in samplerInfo, null, out var sampler);

        uint id = AllocHandle();
        Samplers[id] = new VkSamplerRes { Handle = sampler };
        return new SamplerHandle(id);
    }

    public void Destroy(SamplerHandle handle)
    {
        if (Samplers.Remove(handle.Id, out var sampler))
            _pendingDeletes.Add(() => Api.DestroySampler(Device, sampler.Handle, null));
    }

    // ================================================== One-shot command helper

    internal CommandBuffer OneShotBegin()
    {
        var allocInfo = new CommandBufferAllocateInfo
        {
            SType              = StructureType.CommandBufferAllocateInfo,
            CommandPool        = GraphicsPool,
            Level              = CommandBufferLevel.Primary,
            CommandBufferCount = 1,
        };
        CommandBuffer commandBuffer = default;
        Api.AllocateCommandBuffers(Device, in allocInfo, &commandBuffer);

        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
        };
        Api.BeginCommandBuffer(commandBuffer, in beginInfo);
        return commandBuffer;
    }

    internal void OneShotEndSubmitWait(CommandBuffer commandBuffer)
    {
        Api.EndCommandBuffer(commandBuffer);
        var submitInfo = new SubmitInfo
        {
            SType              = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers    = &commandBuffer,
        };
        Api.QueueSubmit(GraphicsQueue, 1, in submitInfo, default);
        Api.QueueWaitIdle(GraphicsQueue);
        Api.FreeCommandBuffers(Device, GraphicsPool, 1, &commandBuffer);
    }

    // ========================================================= Command stream

    private VkCommandList? _activeCmd;

    // Swapchain whose frame was submitted and is now waiting to be presented.
    // Cleared by BeginCommandList; Present only proceeds for a matching entry,
    // so a Present after an abandoned (re-entrant) frame can't hang forever
    // waiting on a render-finished semaphore that was never signalled.
    private VkSwapchainRes? _pendingPresent;

    public ICommandList BeginCommandList()
    {
        // A previous command list — abandoned, or the outer frame of a
        // re-entrant render — must stop touching the shared command buffer
        // once it is reset below.
        if (_activeCmd != null) _activeCmd.Disowned = true;
        _pendingPresent = null;

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

        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
        };
        Api.BeginCommandBuffer(FrameCmd, in beginInfo);
        FrameRecording = true;

        _activeCmd = new VkCommandList(this, FrameCmd);
        return _activeCmd;
    }

    public void Submit(ICommandList commandList)
    {
        if (!FrameRecording) return;
        var vkCommandList = (VkCommandList)commandList;
        // A stale command list — a newer BeginCommandList already took over
        // the shared command buffer — must not finalise this frame.
        if (vkCommandList != _activeCmd) return;
        vkCommandList.Finish();
        Api.EndCommandBuffer(FrameCmd);

        // If a swapchain image was acquired this frame, synchronise present
        // with the render submission.
        var swapchain = vkCommandList.AcquiredSwapchain;
        if (swapchain != null)
        {
            _pendingPresent = swapchain;   // this frame is now eligible for Present
            var waitSemaphore = ImageAvailable;
            // Signal this image's own "render finished" semaphore. A single
            // shared one is illegal — the previous image's present may still
            // be consuming it (VUID-vkQueueSubmit-pSignalSemaphores-00067).
            var signalSemaphore = swapchain.RenderFinishedSemaphores[swapchain.CurrentImageIndex];
            var waitStage       = PipelineStageFlags.ColorAttachmentOutputBit;
            var commandBuffer   = FrameCmd;
            var submitInfo = new SubmitInfo
            {
                SType                = StructureType.SubmitInfo,
                WaitSemaphoreCount   = 1,
                PWaitSemaphores      = &waitSemaphore,
                PWaitDstStageMask    = &waitStage,
                CommandBufferCount   = 1,
                PCommandBuffers      = &commandBuffer,
                SignalSemaphoreCount = 1,
                PSignalSemaphores    = &signalSemaphore,
            };
            Api.QueueSubmit(GraphicsQueue, 1, in submitInfo, FrameFence);
        }
        else
        {
            var commandBuffer = FrameCmd;
            var submitInfo = new SubmitInfo
            {
                SType              = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers    = &commandBuffer,
            };
            Api.QueueSubmit(GraphicsQueue, 1, in submitInfo, FrameFence);
        }
        FrameRecording = false;
        _activeCmd = null;
    }

    public void Present(SwapchainHandle handle)
    {
        var swapchain = Swapchains[handle.Id];
        // Present only the swapchain whose frame was actually submitted this
        // cycle. Presenting an abandoned frame would wait on a render-finished
        // semaphore that was never signalled.
        if (swapchain != _pendingPresent || !swapchain.ImageAcquired) return;
        _pendingPresent = null;

        // Wait on the same per-image semaphore Submit signalled for this image.
        var renderFinished  = swapchain.RenderFinishedSemaphores[swapchain.CurrentImageIndex];
        var swapchainHandle = swapchain.SwapchainHandle;
        var imageIndex      = swapchain.CurrentImageIndex;
        var presentInfo = new PresentInfoKHR
        {
            SType              = StructureType.PresentInfoKhr,
            WaitSemaphoreCount = 1,
            PWaitSemaphores    = &renderFinished,
            SwapchainCount     = 1,
            PSwapchains        = &swapchainHandle,
            PImageIndices      = &imageIndex,
        };
        KhrSwapchain.QueuePresent(GraphicsQueue, in presentInfo);
        swapchain.ImageAcquired = false;
    }
}
