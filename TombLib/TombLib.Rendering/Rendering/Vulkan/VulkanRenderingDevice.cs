using NLog;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TombLib.Utils;

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

        // Process-wide pool of descriptor sets shared across every subsystem.
        // Sized generously for a typical editor scene: ~thousand UBO-bound
        // sets + ~thousand image+sampler bound sets is enough headroom.
        public DescriptorPool DescriptorPool { get; private set; }
        // Transient command pool for one-shot setup commands (texture upload,
        // image layout transitions, buffer copies). Subsystems request a
        // command buffer via BeginTransient(), record their work, and call
        // EndAndSubmitTransient() which waits for the GPU to finish before
        // freeing the buffer. Not for hot-path use — that's per-frame
        // command buffers on VulkanSwapChain.
        public CommandPool TransientPool { get; private set; }

        // Shared GLSL → SPIR-V compiler. Each Drawing* keeps its shader source
        // as inline GLSL strings (set N / binding M annotations) and asks the
        // compiler to produce the SPIR-V bytecode at init time.
        public VulkanShaderCompiler ShaderCompiler { get; private set; }

        // Device-wide per-frame uniform ring shared across all swap chains.
        // Reset by VulkanSwapChain.Clear at frame start. (Multi-panel ring
        // collision is theoretically possible if Panel B's Clear resets while
        // Panel A's submitted CB still reads from the ring — but in practice
        // the WinForms UI thread serialises Paint events and the GPU usually
        // finishes A's small CB before B's reset; reproducing the collision
        // would require simultaneous Panel A + Panel B with very heavy GPU
        // work on A. A per-swap-chain ring would solve it but causes a worse
        // problem: rewriting _meshSet's descriptor when ping-ponging between
        // swap chains is a spec violation while a CB is pending → flickering.)
        public VulkanFrameUniforms FrameUniforms { get; private set; }

        // Shared samplers used across drawing classes. Bilinear+aniso for
        // textured rendering at quality, point-sampling for thumbnails / no
        // filtering. Created once at device init.
        public Sampler SamplerAniso { get; private set; }
        public Sampler SamplerPoint { get; private set; }

        // Sector-overlay icons (arrows, slides, illegal-slope markers etc.).
        // 256x256 per-layer Texture2DArray loaded once from embedded PNG
        // resources at device init. Used exclusively by VulkanDrawingRoom
        // when EditorSectorTexture bit 0x40 is set.
        public Image SectorTextureArray { get; private set; }
        public DeviceMemory SectorTextureArrayMemory { get; private set; }
        public ImageView SectorTextureArrayView { get; private set; }

        // Validation layer + debug messenger. Off by default; enable with
        // env var TOMBEDITOR_VK_VALIDATION=1. Heavy CPU cost (~5-10x) so it
        // never ships on by accident.
        private bool _validationEnabled;
        private ExtDebugUtils _debugUtils;
        private DebugUtilsMessengerEXT _debugMessenger;

        // ---- Fence-based deletion queue ------------------------------------
        //
        // Drawing*.Dispose() can be called mid-paint (e.g. Panel3D's
        // _renderingCachedRooms.Clear() fires from ConfigurationChangedEvent
        // while a swap chain CB is in recording state). DeviceWaitIdle does
        // NOT cover recording CBs, so immediate destruction invalidates the
        // CB → device-lost at next Present.
        //
        // Solution: callers stash a destroyer lambda via QueueDestroy. At
        // every swap chain Clear(), TryDrainPendingDestroys checks (non-
        // blocking, via vkGetFenceStatus) whether ALL registered swap
        // chains' last-submit fences have signaled. If they have, the GPU
        // has retired every CB that could possibly reference these resources,
        // so it's safe to run the destroyers. If any fence is still pending,
        // the drain skips and tries again next Clear. No DeviceWaitIdle stall
        // — pending destroys flush naturally within 1-2 frames.
        private readonly object _pendingDestroysLock = new object();
        private readonly Queue<Action> _pendingDestroys = new Queue<Action>();
        private readonly List<VulkanSwapChain> _registeredSwapChains = new List<VulkanSwapChain>();

        internal void RegisterSwapChain(VulkanSwapChain sc)
        {
            lock (_pendingDestroysLock) _registeredSwapChains.Add(sc);
        }

        internal void UnregisterSwapChain(VulkanSwapChain sc)
        {
            lock (_pendingDestroysLock) _registeredSwapChains.Remove(sc);
        }

        public void QueueDestroy(Action destroyer)
        {
            if (destroyer == null) return;
            lock (_pendingDestroysLock) _pendingDestroys.Enqueue(destroyer);
        }

        public unsafe void TryDrainPendingDestroys()
        {
            Action[] toRun;
            lock (_pendingDestroysLock)
            {
                if (_pendingDestroys.Count == 0) return;

                // All registered swap chains must have their last-submit fence
                // signaled (i.e. GPU is idle on every panel). vkGetFenceStatus
                // is non-blocking — returns NotReady if work is still pending.
                foreach (var sc in _registeredSwapChains)
                {
                    Fence f = sc.CurrentInFlightFence;
                    if (f.Handle == 0) continue;
                    if (Vk.GetFenceStatus(Device, f) != Result.Success)
                        return; // still pending — try again next Clear
                }

                toRun = _pendingDestroys.ToArray();
                _pendingDestroys.Clear();
            }
            foreach (var a in toRun) a();
        }

        public void DrainAllPendingDestroys()
        {
            // Called at device shutdown after DeviceWaitIdle — everything is
            // guaranteed safe to destroy regardless of fence state.
            Action[] toRun;
            lock (_pendingDestroysLock)
            {
                if (_pendingDestroys.Count == 0) return;
                toRun = _pendingDestroys.ToArray();
                _pendingDestroys.Clear();
            }
            foreach (var a in toRun) a();
        }

        public VulkanRenderingDevice()
        {
            Vk = Vk.GetApi();
            bool envRequested = Environment.GetEnvironmentVariable("TOMBEDITOR_VK_VALIDATION") == "1";
#if DEBUG
            _validationEnabled = Environment.GetEnvironmentVariable("TOMBEDITOR_VK_VALIDATION") != "0";
#else
            _validationEnabled = envRequested;
#endif

            CreateInstance();
            if (_validationEnabled) SetupDebugMessenger();
            PickPhysicalDevice();
            CreateLogicalDevice();
            ResolveSwapchainExtension();
            CreateDescriptorPool();
            CreateTransientPool();
            ShaderCompiler = new VulkanShaderCompiler();
            FrameUniforms = new VulkanFrameUniforms(this, 4 * 1024 * 1024);
            CreateSamplers();
            CreateSectorTextureArray();

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

        private unsafe bool IsLayerAvailable(string layerName)
        {
            uint count = 0;
            Vk.EnumerateInstanceLayerProperties(&count, null);
            if (count == 0)
                return false;
            var props = new LayerProperties[count];
            fixed (LayerProperties* p = props)
                Vk.EnumerateInstanceLayerProperties(&count, p);
            foreach (var lp in props)
            {
                string name = Marshal.PtrToStringAnsi((IntPtr)lp.LayerName);
                if (name == layerName)
                    return true;
            }
            return false;
        }

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

            // Check whether the validation layer is actually installed before
            // requesting it — avoids ErrorLayerNotPresent when the Vulkan SDK
            // is not present on the machine.
            var layers = new List<string>();
            if (_validationEnabled)
            {
                if (IsLayerAvailable("VK_LAYER_KHRONOS_validation"))
                {
                    layers.Add("VK_LAYER_KHRONOS_validation");
                    extensions.Add("VK_EXT_debug_utils");
                    extensions.Add("VK_EXT_validation_features");
                }
                else
                {
                    logger.Warn("Validation requested but VK_LAYER_KHRONOS_validation is not installed (install the Vulkan SDK). Continuing without validation.");
                    _validationEnabled = false;
                }
            }

            byte** ppExtensions = (byte**)SilkMarshal.StringArrayToPtr(extensions.ToArray());
            byte** ppLayers     = (byte**)SilkMarshal.StringArrayToPtr(layers.ToArray());

            // Enable GPU-assisted + sync validation. GAV instruments SPIR-V to
            // bounds-check descriptor reads on the GPU and reports the offending
            // shader/descriptor right before a device-lost. Sync validation
            // catches missing barriers between submits / between CB regions.
            var enabledFeatures = stackalloc ValidationFeatureEnableEXT[3]
            {
                ValidationFeatureEnableEXT.GpuAssistedExt,
                ValidationFeatureEnableEXT.GpuAssistedReserveBindingSlotExt,
                ValidationFeatureEnableEXT.SynchronizationValidationExt,
            };
            ValidationFeaturesEXT validationFeatures = new ValidationFeaturesEXT
            {
                SType = StructureType.ValidationFeaturesExt,
                EnabledValidationFeatureCount = 3,
                PEnabledValidationFeatures = enabledFeatures,
            };

            InstanceCreateInfo createInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledExtensionCount = (uint)extensions.Count,
                PpEnabledExtensionNames = ppExtensions,
                EnabledLayerCount = (uint)layers.Count,
                PpEnabledLayerNames = ppLayers,
                PNext = _validationEnabled ? &validationFeatures : null,
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
                    DebugUtilsMessageSeverityFlagsEXT.InfoBitExt |
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
            else if ((severity & DebugUtilsMessageSeverityFlagsEXT.WarningBitExt) != 0)
                logger.Warn("[Vulkan] {0}", msg);
            else
                logger.Info("[Vulkan] {0}", msg);
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
                // Required for GPU-assisted validation to instrument shader
                // stores. Harmless if GAV is off — GPU drivers expose both
                // on every desktop part.
                FragmentStoresAndAtomics = _validationEnabled,
                VertexPipelineStoresAndAtomics = _validationEnabled,
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

        // ---- Shared pools ---------------------------------------------------

        private unsafe void CreateDescriptorPool()
        {
            var sizes = stackalloc DescriptorPoolSize[3]
            {
                new DescriptorPoolSize(DescriptorType.UniformBuffer, 1024),
                new DescriptorPoolSize(DescriptorType.UniformBufferDynamic, 1024),
                new DescriptorPoolSize(DescriptorType.CombinedImageSampler, 1024),
            };
            DescriptorPoolCreateInfo info = new DescriptorPoolCreateInfo
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                MaxSets = 2048,
                PoolSizeCount = 3,
                PPoolSizes = sizes,
                Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit,
            };
            DescriptorPool pool;
            VkCheck.Ok(Vk.CreateDescriptorPool(Device, in info, null, &pool));
            DescriptorPool = pool;
        }

        private unsafe void CreateTransientPool()
        {
            CommandPoolCreateInfo info = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = GraphicsQueueFamily,
                Flags = CommandPoolCreateFlags.TransientBit | CommandPoolCreateFlags.ResetCommandBufferBit,
            };
            CommandPool pool;
            VkCheck.Ok(Vk.CreateCommandPool(Device, in info, null, &pool));
            TransientPool = pool;
        }

        // Allocate + begin a one-shot command buffer. Caller records work,
        // then calls EndAndSubmitTransient(cb).
        public unsafe CommandBuffer BeginTransient()
        {
            CommandBufferAllocateInfo info = new CommandBufferAllocateInfo
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = TransientPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1,
            };
            CommandBuffer cb;
            VkCheck.Ok(Vk.AllocateCommandBuffers(Device, in info, &cb));
            CommandBufferBeginInfo begin = new CommandBufferBeginInfo
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
            };
            Vk.BeginCommandBuffer(cb, in begin);
            return cb;
        }

        private unsafe void CreateSamplers()
        {
            SamplerCreateInfo aniso = new SamplerCreateInfo
            {
                SType = StructureType.SamplerCreateInfo,
                MagFilter = Filter.Linear,
                MinFilter = Filter.Linear,
                MipmapMode = SamplerMipmapMode.Linear,
                AddressModeU = SamplerAddressMode.Repeat,
                AddressModeV = SamplerAddressMode.Repeat,
                AddressModeW = SamplerAddressMode.Repeat,
                AnisotropyEnable = true,
                MaxAnisotropy = 4f,
                CompareEnable = false,
                MaxLod = Vk.LodClampNone,
            };
            Sampler s1;
            VkCheck.Ok(Vk.CreateSampler(Device, in aniso, null, &s1));
            SamplerAniso = s1;

            SamplerCreateInfo point = new SamplerCreateInfo
            {
                SType = StructureType.SamplerCreateInfo,
                MagFilter = Filter.Nearest,
                MinFilter = Filter.Nearest,
                MipmapMode = SamplerMipmapMode.Nearest,
                AddressModeU = SamplerAddressMode.Repeat,
                AddressModeV = SamplerAddressMode.Repeat,
                AddressModeW = SamplerAddressMode.Repeat,
                MaxLod = Vk.LodClampNone,
            };
            Sampler s2;
            VkCheck.Ok(Vk.CreateSampler(Device, in point, null, &s2));
            SamplerPoint = s2;
        }

        // Decode every SectorTexture-named PNG embedded into TombLib.Rendering.dll
        // and pack them into a single Texture2DArray. The RoomShader samples this
        // by layer index from EditorSectorTexture[15..8] when bit 0x40 is set
        // (i.e. arrow / climbable / illegal-slope overlay on a sector). 256×256
        // per-layer, B8G8R8A8_UNorm.
        private unsafe void CreateSectorTextureArray()
        {
            const int Size = 256;
            // Skip "None" — only the actual texture entries are uploaded.
            string[] names = Enum.GetNames(typeof(SectorTexture)).Skip(1).ToArray();
            int layers = names.Length;
            var assembly = typeof(VulkanRenderingDevice).Assembly;

            ImageCreateInfo imgInfo = new ImageCreateInfo
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Format = Format.B8G8R8A8Unorm,
                Extent = new Extent3D(Size, Size, 1),
                MipLevels = 1,
                ArrayLayers = (uint)layers,
                Samples = SampleCountFlags.Count1Bit,
                Tiling = ImageTiling.Optimal,
                Usage = ImageUsageFlags.SampledBit | ImageUsageFlags.TransferDstBit,
                SharingMode = SharingMode.Exclusive,
                InitialLayout = ImageLayout.Undefined,
            };
            Image img;
            VkCheck.Ok(Vk.CreateImage(Device, in imgInfo, null, &img));
            SectorTextureArray = img;

            Vk.GetImageMemoryRequirements(Device, SectorTextureArray, out MemoryRequirements req);
            MemoryAllocateInfo alloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size,
                MemoryTypeIndex = FindMemoryType(req.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
            };
            DeviceMemory mem;
            VkCheck.Ok(Vk.AllocateMemory(Device, in alloc, null, &mem));
            SectorTextureArrayMemory = mem;
            Vk.BindImageMemory(Device, SectorTextureArray, SectorTextureArrayMemory, 0);

            ImageViewCreateInfo viewInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = SectorTextureArray,
                ViewType = ImageViewType.Type2DArray,
                Format = Format.B8G8R8A8Unorm,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, (uint)layers),
            };
            ImageView view;
            VkCheck.Ok(Vk.CreateImageView(Device, in viewInfo, null, &view));
            SectorTextureArrayView = view;

            // Transition every layer Undefined→TransferDst once, then per-layer upload + final transition to ShaderReadOnly.
            var cb = BeginTransient();
            ImageMemoryBarrier toDst = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = ImageLayout.Undefined, NewLayout = ImageLayout.TransferDstOptimal,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored, DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = SectorTextureArray,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, (uint)layers),
                SrcAccessMask = 0, DstAccessMask = AccessFlags.TransferWriteBit,
            };
            Vk.CmdPipelineBarrier(cb, PipelineStageFlags.TopOfPipeBit, PipelineStageFlags.TransferBit,
                0, 0, null, 0, null, 1, in toDst);

            // Stage every layer into one big buffer, copy region by region.
            uint perLayerBytes = (uint)(Size * Size * 4);
            uint totalBytes = perLayerBytes * (uint)layers;
            BufferCreateInfo bi = new BufferCreateInfo
            { SType = StructureType.BufferCreateInfo, Size = totalBytes,
              Usage = BufferUsageFlags.TransferSrcBit, SharingMode = SharingMode.Exclusive };
            Silk.NET.Vulkan.Buffer staging;
            VkCheck.Ok(Vk.CreateBuffer(Device, in bi, null, &staging));
            Vk.GetBufferMemoryRequirements(Device, staging, out MemoryRequirements bufReq);
            MemoryAllocateInfo bufAlloc = new MemoryAllocateInfo
            { SType = StructureType.MemoryAllocateInfo, AllocationSize = bufReq.Size,
              MemoryTypeIndex = FindMemoryType(bufReq.MemoryTypeBits,
                  MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit) };
            DeviceMemory stagingMem;
            VkCheck.Ok(Vk.AllocateMemory(Device, in bufAlloc, null, &stagingMem));
            Vk.BindBufferMemory(Device, staging, stagingMem, 0);
            void* mapped;
            Vk.MapMemory(Device, stagingMem, 0, totalBytes, 0, &mapped);
            IntPtr mappedPtr = (IntPtr)mapped;

            for (int i = 0; i < layers; i++)
            {
                string resourceName = "TombLib.Rendering.SectorTextures." + names[i] + ".png";
                using (Stream s = assembly.GetManifestResourceStream(resourceName))
                {
                    if (s == null)
                        throw new InvalidOperationException("Missing embedded resource: " + resourceName);
                    ImageC img2 = ImageC.FromStream(s);
                    if (img2.Width != Size || img2.Height != Size)
                        throw new ArgumentOutOfRangeException("SectorTexture wrong size: " + resourceName);
                    int layerOffset = i * (int)perLayerBytes;
                    img2.GetIntPtr(ptr =>
                    {
                        unsafe
                        {
                            System.Buffer.MemoryCopy((void*)ptr, (byte*)mappedPtr + layerOffset, perLayerBytes, perLayerBytes);
                        }
                    });

                    BufferImageCopy copy = new BufferImageCopy
                    {
                        BufferOffset = (ulong)layerOffset,
                        BufferRowLength = 0,
                        BufferImageHeight = 0,
                        ImageSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, (uint)i, 1),
                        ImageOffset = new Offset3D(0, 0, 0),
                        ImageExtent = new Extent3D(Size, Size, 1),
                    };
                    Vk.CmdCopyBufferToImage(cb, staging, SectorTextureArray, ImageLayout.TransferDstOptimal, 1, in copy);
                }
            }
            Vk.UnmapMemory(Device, stagingMem);

            ImageMemoryBarrier toShader = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = ImageLayout.TransferDstOptimal, NewLayout = ImageLayout.ShaderReadOnlyOptimal,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored, DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = SectorTextureArray,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, (uint)layers),
                SrcAccessMask = AccessFlags.TransferWriteBit, DstAccessMask = AccessFlags.ShaderReadBit,
            };
            Vk.CmdPipelineBarrier(cb, PipelineStageFlags.TransferBit, PipelineStageFlags.FragmentShaderBit,
                0, 0, null, 0, null, 1, in toShader);
            EndAndSubmitTransient(cb);

            Vk.DestroyBuffer(Device, staging, null);
            Vk.FreeMemory(Device, stagingMem, null);
        }

        // End + submit + WAIT for completion (synchronous). Used for setup
        // operations only — never on the per-frame hot path.
        // Uses a fence (not QueueWaitIdle) so we only wait for *this* CB,
        // avoiding a deadlock when a QueuePresent is still in-flight and
        // the Windows presentation engine needs the message pump to retire it.
        public unsafe void EndAndSubmitTransient(CommandBuffer cb)
        {
            Vk.EndCommandBuffer(cb);

            FenceCreateInfo fenceInfo = new FenceCreateInfo { SType = StructureType.FenceCreateInfo };
            Fence fence;
            VkCheck.Ok(Vk.CreateFence(Device, in fenceInfo, null, out fence));

            SubmitInfo submit = new SubmitInfo
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = &cb,
            };
            VkCheck.Ok(Vk.QueueSubmit(GraphicsQueue, 1, in submit, fence));
            VkCheck.Ok(Vk.WaitForFences(Device, 1, in fence, true, ulong.MaxValue));

            Vk.DestroyFence(Device, fence, null);
            Vk.FreeCommandBuffers(Device, TransientPool, 1, in cb);
        }

        // ---- Cleanup --------------------------------------------------------

        public override unsafe void Dispose()
        {
            if (Device.Handle != 0)
            {
                Vk.DeviceWaitIdle(Device);
                DrainAllPendingDestroys();
                FrameUniforms?.Dispose(); FrameUniforms = null;
                ShaderCompiler?.Dispose(); ShaderCompiler = null;
                if (SectorTextureArrayView.Handle != 0)   { Vk.DestroyImageView(Device, SectorTextureArrayView, null);   SectorTextureArrayView = default; }
                if (SectorTextureArray.Handle != 0)       { Vk.DestroyImage(Device, SectorTextureArray, null);           SectorTextureArray = default; }
                if (SectorTextureArrayMemory.Handle != 0) { Vk.FreeMemory(Device, SectorTextureArrayMemory, null);       SectorTextureArrayMemory = default; }
                if (SamplerAniso.Handle != 0) { Vk.DestroySampler(Device, SamplerAniso, null); SamplerAniso = default; }
                if (SamplerPoint.Handle != 0) { Vk.DestroySampler(Device, SamplerPoint, null); SamplerPoint = default; }
                if (TransientPool.Handle != 0) { Vk.DestroyCommandPool(Device, TransientPool, null); TransientPool = default; }
                if (DescriptorPool.Handle != 0) { Vk.DestroyDescriptorPool(Device, DescriptorPool, null); DescriptorPool = default; }
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
            => new VulkanTextureAllocator(this, description);

        public override RenderingStateBuffer CreateStateBuffer()
            => new VulkanStateBuffer(this);

        // RenderingFont is backend-agnostic — it uses GDI for glyph rasterisation
        // and uploads via the abstract RenderingTextureAllocator.Get path which
        // VulkanTextureAllocator implements.
        public override RenderingFont CreateFont(RenderingFont.Description description)
            => new RenderingFont(description);

        public override RenderingDrawingLines CreateDrawingLines(RenderingDrawingLines.Description description)
            => new VulkanDrawingLines(this, description);

        public override RenderingDrawingTest CreateDrawingTest(RenderingDrawingTest.Description description)
            => new VulkanDrawingTest(this, description);

        public override RenderingDrawingRoom CreateDrawingRoom(RenderingDrawingRoom.Description description)
            => new VulkanDrawingRoom(this, description);

        public override RenderingDrawingMesh CreateDrawingMesh(RenderingDrawingMesh.Description description)
            => new VulkanDrawingMesh(this, description);

        public override RenderingDrawingImportedGeometry CreateDrawingImportedGeometry(RenderingDrawingImportedGeometry.Description description)
            => new VulkanDrawingImportedGeometry(this, description);

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
