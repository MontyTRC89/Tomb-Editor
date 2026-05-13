using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace TombLib.Rendering.Vulkan
{
    // Per-frame uniform block (vk binding for "FrameData"). Layout matches the
    // legacy Dx11 ConstantBufferLayout bit-for-bit so the editor's Set(state)
    // marshalling code is unchanged.
    //
    // Memory: HOST_VISIBLE | HOST_COHERENT and persistently mapped — Set()
    // is a memcpy into the mapped pointer with no syscall. The buffer is
    // small (128 bytes) and updated once per frame, so HOST memory is the
    // right trade-off (no staging copy, no command-list update overhead).
    public sealed class VulkanStateBuffer : RenderingStateBuffer
    {
        [StructLayout(LayoutKind.Explicit, Size = 128)]
        public struct ConstantBufferLayout
        {
            [FieldOffset(0)]   public Matrix4x4 TransformMatrix;
            [FieldOffset(64)]  public float RoomGridLineWidth;
            [FieldOffset(68)]  public int RoomGridForce;
            [FieldOffset(72)]  public int RoomDisableVertexColors;
            [FieldOffset(76)]  public int ShowExtraBlendingModes;
            [FieldOffset(80)]  public int ShowLightingWhiteTextureOnly;
            [FieldOffset(84)]  public int LightMode;
            [FieldOffset(88)]  public int BrushShape;
            [FieldOffset(92)]  public float BrushRotation;
            [FieldOffset(96)]  public Vector4 BrushCenter;
            [FieldOffset(112)] public Vector4 BrushColor;
        }
        public const uint Size = 128;

        public readonly VulkanRenderingDevice DeviceWrapper;
        public VkBuffer Buffer { get; private set; }
        public DeviceMemory Memory { get; private set; }

        private readonly Vk _vk;
        private readonly Device _device;
        private unsafe void* _mapped;

        public unsafe VulkanStateBuffer(VulkanRenderingDevice device)
        {
            DeviceWrapper = device;
            _vk = device.Vk;
            _device = device.Device;

            BufferCreateInfo info = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = Size,
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

            // Map once, never unmap (persistent mapping). Coherent memory means
            // CPU writes are visible to the GPU without explicit flushes.
            void* mapped;
            VkCheck.Ok(_vk.MapMemory(_device, Memory, 0, Size, 0, &mapped));
            _mapped = mapped;
        }

        public override unsafe void Dispose()
        {
            if (Memory.Handle != 0)
            {
                _vk.UnmapMemory(_device, Memory);
                _vk.FreeMemory(_device, Memory, null);
                Memory = default;
            }
            if (Buffer.Handle != 0) { _vk.DestroyBuffer(_device, Buffer, null); Buffer = default; }
        }

        public override unsafe void Set(RenderingState State)
        {
            ConstantBufferLayout cb;
            cb.TransformMatrix = State.TransformMatrix;
            cb.RoomGridLineWidth = State.RoomGridLineWidth;
            cb.RoomGridForce = State.RoomGridForce ? 1 : 0;
            cb.RoomDisableVertexColors = State.RoomDisableVertexColors ? 1 : 0;
            cb.ShowExtraBlendingModes = State.ShowExtraBlendingModes ? 1 : 0;
            cb.ShowLightingWhiteTextureOnly = State.ShowLightingWhiteTextureOnly ? 1 : 0;
            cb.LightMode = State.LightMode;
            cb.BrushShape = State.BrushShape;
            cb.BrushRotation = State.BrushRotation;
            cb.BrushCenter = State.BrushCenter;
            cb.BrushColor = State.BrushColor;
            System.Buffer.MemoryCopy(&cb, _mapped, Size, Size);
        }
    }
}
