using System;
using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace TombLib.Rendering.Vulkan
{
    // Per-frame uniform ring. Drawing classes Allocate(size)+Write(data) → offset,
    // then bind a single shared descriptor set (DescriptorTypeUniformBufferDynamic)
    // with the returned offset. Eliminates per-draw descriptor set creation and
    // per-draw vkCmdUpdateBuffer staging copies — the dominant CPU cost on
    // Vulkan for editor-style scenes with hundreds of small draws per frame.
    //
    // Buffer: HostVisible | HostCoherent (so writes are immediately visible to
    // the GPU without an explicit flush), persistently mapped at creation.
    // Reset() rewinds the offset at frame boundary (VulkanSwapChain.Clear).
    //
    // Sized generously (4 MB default). One Allocate is one Vulkan-aligned slot;
    // alignment is GPU-specific (minUniformBufferOffsetAlignment, typically
    // 64 or 256 bytes).
    public sealed class VulkanFrameUniforms : IDisposable
    {
        public VkBuffer Buffer { get; private set; }
        public DeviceMemory Memory { get; private set; }
        public uint Capacity { get; }
        public uint Alignment { get; }

        private readonly Vk _vk;
        private readonly Device _device;
        private unsafe void* _mapped;
        private uint _offset;

        public unsafe VulkanFrameUniforms(VulkanRenderingDevice device, uint capacity)
        {
            _vk = device.Vk;
            _device = device.Device;
            Capacity = capacity;
            Alignment = (uint)Math.Max(16u, (uint)device.PhysicalDeviceProperties.Limits.MinUniformBufferOffsetAlignment);

            BufferCreateInfo info = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = capacity,
                Usage = BufferUsageFlags.UniformBufferBit,
                SharingMode = SharingMode.Exclusive,
            };
            VkBuffer buf;
            VkCheck.Ok(_vk.CreateBuffer(_device, in info, null, &buf));
            Buffer = buf;

            _vk.GetBufferMemoryRequirements(_device, Buffer, out MemoryRequirements req);
            MemoryAllocateInfo alloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size,
                MemoryTypeIndex = device.FindMemoryType(req.MemoryTypeBits,
                    MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit),
            };
            DeviceMemory mem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in alloc, null, &mem));
            Memory = mem;
            _vk.BindBufferMemory(_device, Buffer, Memory, 0);

            void* mapped;
            VkCheck.Ok(_vk.MapMemory(_device, Memory, 0, capacity, 0, &mapped));
            _mapped = mapped;
        }

        public unsafe void Dispose()
        {
            if (Memory.Handle != 0)
            {
                _vk.UnmapMemory(_device, Memory);
                _vk.FreeMemory(_device, Memory, null);
                Memory = default;
            }
            if (Buffer.Handle != 0) { _vk.DestroyBuffer(_device, Buffer, null); Buffer = default; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Allocate(uint sizeBytes)
        {
            uint aligned = (sizeBytes + Alignment - 1) & ~(Alignment - 1);
            uint offset = _offset;
            uint next = offset + aligned;
            if (next > Capacity)
                throw new InvalidOperationException(
                    $"VulkanFrameUniforms ring overflow at {next}/{Capacity} bytes.");
            _offset = next;
            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe uint Upload<T>(ref T data) where T : unmanaged
        {
            uint size = (uint)sizeof(T);
            uint offset = Allocate(size);
            byte* dst = (byte*)_mapped + offset;
            fixed (T* src = &data)
                System.Buffer.MemoryCopy(src, dst, size, size);
            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe uint Upload(byte[] data, uint length)
        {
            uint offset = Allocate(length);
            fixed (byte* src = data)
                System.Buffer.MemoryCopy(src, (byte*)_mapped + offset, length, length);
            return offset;
        }

        public void Reset() => _offset = 0;
    }
}
