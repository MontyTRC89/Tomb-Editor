using NLog;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TombLib.Rendering.Vulkan
{
    // Direct Vulkan rendering backend. NO middleware layer (Veldrid, etc.) — we
    // use Silk.NET.Vulkan as a thin binding only. Every per-frame hot-path
    // decision is made here so we can apply patterns the editor needs to be
    // fast on Vulkan: persistently-mapped uniform buffers with dynamic offsets,
    // descriptor caching, frame-in-flight, pipeline reuse, minimal vkCmd*
    // calls per draw.
    //
    // This class owns the long-lived globals (VkInstance, VkPhysicalDevice,
    // VkDevice, queues, KHR extensions). Per-window state (surface, swapchain,
    // command buffers, frame sync) lives on VulkanSwapChain.
    //
    // Lifecycle:
    //   1) Validate that the Vulkan loader is present (Silk.NET.Vulkan throws if
    //      not). Create a VkInstance with the surface extensions we need.
    //   2) Pick a physical device that has a graphics queue family AND supports
    //      Win32 surface presentation. Prefer discrete GPUs.
    //   3) Create a logical device + a single graphics+present queue.
    //   4) Resolve the KHR_swapchain / KHR_win32_surface extension function pointers.
    //
    // Factory methods that aren't implemented yet throw NotSupportedException
    // with a clear "not ported" message.
    public sealed class VulkanRenderingDevice : RenderingDevice
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public readonly Vk Vk;
        public Instance Instance { get; private set; }
        public PhysicalDevice PhysicalDevice { get; private set; }
        public PhysicalDeviceProperties PhysicalDeviceProperties { get; private set; }
        public PhysicalDeviceMemoryProperties PhysicalDeviceMemoryProperties { get; private set; }
        public Device Device { get; private set; }
        public uint GraphicsQueueFamily { get; private set; }
        public Queue GraphicsQueue { get; private set; }

        public KhrSurface KhrSurface { get; private set; }
        public KhrSwapchain KhrSwapchain { get; private set; }
        public KhrWin32Surface KhrWin32Surface { get; private set; }

        // Validation layer + debug messenger. Off by default; enable with
        // env var TOMBEDITOR_VK_VALIDATION=1. Heavy CPU cost (~5-10x) so it
        // never ships on by accident.
        private readonly bool _validationEnabled;
        private ExtDebugUtils _debugUtils;
        private DebugUtilsMessengerEXT _debugMessenger;

        public VulkanRenderingDevice()
        {
            Vk = Vk.GetApi();
            _validationEnabled = Environment.GetEnvironmentVariable("TOMBEDITOR_VK_VALIDATION") == "1";

            CreateInstance();
            if (_validationEnabled) SetupDebugMessenger();
            PickPhysicalDevice();
            CreateLogicalDevice();
            ResolveSwapchainExtension();

            logger.Info("VulkanRenderingDevice initialised. GPU=\"{0}\" validation={1}",
                GetDeviceName(), _validationEnabled);
        }

        // PhysicalDeviceProperties.DeviceName is a fixed-size buffer that C# can
        // only address inside a fixed statement. Wrap that here so the caller
        // can pass the result around as a regular string.
        public unsafe string GetDeviceName()
        {
            PhysicalDeviceProperties props = PhysicalDeviceProperties;
            return Marshal.PtrToStringAnsi((IntPtr)props.DeviceName) ?? "<unknown>";
        }

        // ---- Instance --------------------------------------------------------

        private unsafe void CreateInstance()
        {
            ApplicationInfo appInfo = new ApplicationInfo
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)SilkMarshal.StringToPtr("TombEditor"),
                ApplicationVersion = new Version32(1, 0, 0),
                PEngineName = (byte*)SilkMarshal.StringToPtr("TombEditor"),
                EngineVersion = new Version32(1, 0, 0),
                ApiVersion = Vk.Version13,
            };

            // Required instance extensions: surface (cross-platform) + win32 surface
            // (Windows-specific). Add debug utils when validation is requested.
            var extensions = new List<string>
            {
                "VK_KHR_surface",
                "VK_KHR_win32_surface",
            };
            if (_validationEnabled) extensions.Add("VK_EXT_debug_utils");

            var layers = new List<string>();
            if (_validationEnabled) layers.Add("VK_LAYER_KHRONOS_validation");

            byte** ppExtensions = (byte**)SilkMarshal.StringArrayToPtr(extensions.ToArray());
            byte** ppLayers     = (byte**)SilkMarshal.StringArrayToPtr(layers.ToArray());

            InstanceCreateInfo createInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledExtensionCount = (uint)extensions.Count,
                PpEnabledExtensionNames = ppExtensions,
                EnabledLayerCount = (uint)layers.Count,
                PpEnabledLayerNames = ppLayers,
            };

            try
            {
                Instance instance;
                VkCheck.Ok(Vk.CreateInstance(in createInfo, null, &instance));
                Instance = instance;
            }
            finally
            {
                SilkMarshal.Free((nint)appInfo.PApplicationName);
                SilkMarshal.Free((nint)appInfo.PEngineName);
                SilkMarshal.Free((nint)ppExtensions);
                SilkMarshal.Free((nint)ppLayers);
            }

            // Resolve the surface extension function pointers (needed even before
            // any swap chain is created — picking a physical device queries
            // surface presentation support for each queue family).
            if (!Vk.TryGetInstanceExtension(Instance, out KhrSurface surface))
                throw new InvalidOperationException("VK_KHR_surface extension not present.");
            KhrSurface = surface;
            if (!Vk.TryGetInstanceExtension(Instance, out KhrWin32Surface win32))
                throw new InvalidOperationException("VK_KHR_win32_surface extension not present.");
            KhrWin32Surface = win32;
        }

        private unsafe void SetupDebugMessenger()
        {
            if (!Vk.TryGetInstanceExtension(Instance, out _debugUtils))
            {
                logger.Warn("Validation requested but VK_EXT_debug_utils not present. Skipping debug messenger.");
                return;
            }

            DebugUtilsMessengerCreateInfoEXT info = new DebugUtilsMessengerCreateInfoEXT
            {
                SType = StructureType.DebugUtilsMessengerCreateInfoExt,
                MessageSeverity =
                    DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                    DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
                MessageType =
                    DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                    DebugUtilsMessageTypeFlagsEXT.ValidationBitExt |
                    DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt,
                PfnUserCallback = new PfnDebugUtilsMessengerCallbackEXT(DebugCallback),
            };

            DebugUtilsMessengerEXT messenger;
            VkCheck.Ok(_debugUtils.CreateDebugUtilsMessenger(Instance, in info, null, &messenger));
            _debugMessenger = messenger;
        }

        private static unsafe uint DebugCallback(
            DebugUtilsMessageSeverityFlagsEXT severity,
            DebugUtilsMessageTypeFlagsEXT type,
            DebugUtilsMessengerCallbackDataEXT* data,
            void* userData)
        {
            string msg = Marshal.PtrToStringAnsi((IntPtr)data->PMessage) ?? "<no message>";
            if ((severity & DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt) != 0)
                logger.Error("[Vulkan] {0}", msg);
            else
                logger.Warn("[Vulkan] {0}", msg);
            return 0; // VK_FALSE — do not abort the call
        }

        // ---- Physical device pick -------------------------------------------

        private unsafe void PickPhysicalDevice()
        {
            uint count = 0;
            VkCheck.Ok(Vk.EnumeratePhysicalDevices(Instance, &count, null));
            if (count == 0) throw new InvalidOperationException("No Vulkan-capable GPU found.");
            var devices = new PhysicalDevice[count];
            fixed (PhysicalDevice* p = devices)
                VkCheck.Ok(Vk.EnumeratePhysicalDevices(Instance, &count, p));

            // Score each device and pick the best. Discrete + graphics queue =
            // strong preference. Prefer higher Vulkan API version on tiebreak.
            PhysicalDevice best = default;
            int bestScore = int.MinValue;
            foreach (var dev in devices)
            {
                int score = ScoreDevice(dev, out _);
                if (score > bestScore) { bestScore = score; best = dev; }
            }
            if (bestScore < 0)
                throw new InvalidOperationException("No suitable Vulkan GPU (need graphics queue + Win32 presentation).");

            PhysicalDevice = best;
            ScoreDevice(best, out uint graphicsFamily);
            GraphicsQueueFamily = graphicsFamily;

            Vk.GetPhysicalDeviceProperties(PhysicalDevice, out PhysicalDeviceProperties props);
            PhysicalDeviceProperties = props;
            Vk.GetPhysicalDeviceMemoryProperties(PhysicalDevice, out PhysicalDeviceMemoryProperties mem);
            PhysicalDeviceMemoryProperties = mem;
        }

        // Score = discrete bonus + graphics-queue-available + memory size proxy.
        // Returns -1 if the device can't be used (no graphics queue or no Win32
        // presentation support). `graphicsFamily` is set to the index of a
        // queue family that supports BOTH graphics and Win32 presentation.
        private unsafe int ScoreDevice(PhysicalDevice device, out uint graphicsFamily)
        {
            graphicsFamily = uint.MaxValue;

            uint queueCount = 0;
            Vk.GetPhysicalDeviceQueueFamilyProperties(device, &queueCount, null);
            var queueProps = new QueueFamilyProperties[queueCount];
            fixed (QueueFamilyProperties* qp = queueProps)
                Vk.GetPhysicalDeviceQueueFamilyProperties(device, &queueCount, qp);

            for (uint i = 0; i < queueCount; i++)
            {
                if ((queueProps[i].QueueFlags & QueueFlags.GraphicsBit) == 0) continue;
                // Win32 presentation support is queried per queue family.
                if (!KhrWin32Surface.GetPhysicalDeviceWin32PresentationSupport(device, i)) continue;
                graphicsFamily = i;
                break;
            }
            if (graphicsFamily == uint.MaxValue) return -1;

            Vk.GetPhysicalDeviceProperties(device, out PhysicalDeviceProperties p);
            int score = 0;
            if (p.DeviceType == PhysicalDeviceType.DiscreteGpu) score += 1000;
            else if (p.DeviceType == PhysicalDeviceType.IntegratedGpu) score += 100;
            score += (int)(p.Limits.MaxImageDimension2D / 1024);
            return score;
        }

        // ---- Logical device -------------------------------------------------

        private unsafe void CreateLogicalDevice()
        {
            float queuePriority = 1.0f;
            DeviceQueueCreateInfo queueInfo = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = GraphicsQueueFamily,
                QueueCount = 1,
                PQueuePriorities = &queuePriority,
            };

            PhysicalDeviceFeatures features = new PhysicalDeviceFeatures
            {
                SamplerAnisotropy = true,
                FillModeNonSolid = true,    // wireframe rasterizer
                IndependentBlend = true,
            };

            string[] deviceExtensions = { "VK_KHR_swapchain" };
            byte** ppExt = (byte**)SilkMarshal.StringArrayToPtr(deviceExtensions);

            DeviceCreateInfo info = new DeviceCreateInfo
            {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = 1,
                PQueueCreateInfos = &queueInfo,
                PEnabledFeatures = &features,
                EnabledExtensionCount = (uint)deviceExtensions.Length,
                PpEnabledExtensionNames = ppExt,
            };

            try
            {
                Device device;
                VkCheck.Ok(Vk.CreateDevice(PhysicalDevice, in info, null, &device));
                Device = device;
            }
            finally
            {
                SilkMarshal.Free((nint)ppExt);
            }

            Queue queue;
            Vk.GetDeviceQueue(Device, GraphicsQueueFamily, 0, &queue);
            GraphicsQueue = queue;
        }

        private void ResolveSwapchainExtension()
        {
            if (!Vk.TryGetDeviceExtension(Instance, Device, out KhrSwapchain sc))
                throw new InvalidOperationException("VK_KHR_swapchain extension not present on device.");
            KhrSwapchain = sc;
        }

        // ---- Cleanup --------------------------------------------------------

        public override unsafe void Dispose()
        {
            if (Device.Handle != 0)
            {
                Vk.DeviceWaitIdle(Device);
                Vk.DestroyDevice(Device, null);
                Device = default;
            }
            if (_debugMessenger.Handle != 0 && _debugUtils != null)
            {
                _debugUtils.DestroyDebugUtilsMessenger(Instance, _debugMessenger, null);
                _debugMessenger = default;
            }
            if (Instance.Handle != 0)
            {
                Vk.DestroyInstance(Instance, null);
                Instance = default;
            }
            Vk?.Dispose();
        }

        // ---- Factory methods ------------------------------------------------
        // All Drawing* / atlas / font implementations are still pending — this
        // session's deliverable is the foundation + clear/present cycle.

        public override RenderingSwapChain CreateSwapChain(RenderingSwapChain.Description description)
            => new VulkanSwapChain(this, description);

        public override RenderingTextureAllocator CreateTextureAllocator(RenderingTextureAllocator.Description description)
            => throw new NotSupportedException("VulkanRenderingDevice.CreateTextureAllocator not implemented yet.");

        public override RenderingStateBuffer CreateStateBuffer()
            => throw new NotSupportedException("VulkanRenderingDevice.CreateStateBuffer not implemented yet.");

        public override RenderingDrawingTest CreateDrawingTest(RenderingDrawingTest.Description description)
            => throw new NotSupportedException("VulkanRenderingDevice.CreateDrawingTest not implemented yet.");

        public override RenderingDrawingRoom CreateDrawingRoom(RenderingDrawingRoom.Description description)
            => throw new NotSupportedException("VulkanRenderingDevice.CreateDrawingRoom not implemented yet.");

        public override RenderingFont CreateFont(RenderingFont.Description description)
            => throw new NotSupportedException("VulkanRenderingDevice.CreateFont not implemented yet.");

        // ---- Memory helper --------------------------------------------------

        // Walk the physical-device memory types table for the first one that:
        //   1) is allowed by the resource's required type bitmask, AND
        //   2) has every property flag we're asking for.
        // Throws if no match — every Vulkan-capable GPU exposes HOST_VISIBLE +
        // DEVICE_LOCAL memory, so a miss means the caller asked for an
        // incompatible combination.
        public uint FindMemoryType(uint typeFilter, MemoryPropertyFlags required)
        {
            for (uint i = 0; i < PhysicalDeviceMemoryProperties.MemoryTypeCount; i++)
            {
                bool typeAllowed = (typeFilter & (1u << (int)i)) != 0;
                bool hasProps = (PhysicalDeviceMemoryProperties.MemoryTypes[(int)i].PropertyFlags & required) == required;
                if (typeAllowed && hasProps) return i;
            }
            throw new InvalidOperationException($"No memory type matches filter=0x{typeFilter:x} required={required}");
        }
    }
}
