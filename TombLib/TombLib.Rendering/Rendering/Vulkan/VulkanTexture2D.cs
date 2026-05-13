using System;
using Silk.NET.Vulkan;
using TombLib.Utils;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace TombLib.Rendering.Vulkan
{
    // Standalone VkImage (2D, single layer) for per-submesh imported-geometry
    // textures. NOT the atlas Texture2DArray — that lives on VulkanTextureAllocator.
    // Use this when each draw needs its own dedicated texture (importer-loaded
    // FBX/OBJ materials).
    //
    // Format: B8G8R8A8_UNorm to match ImageC's BGRA8 layout. Upload happens once
    // at construction via a transient command buffer (same pattern as the atlas).
    public sealed class VulkanTexture2D : IDisposable
    {
        public Image Image { get; private set; }
        public DeviceMemory Memory { get; private set; }
        public ImageView View { get; private set; }
        public int Width { get; }
        public int Height { get; }

        private readonly VulkanRenderingDevice _device;
        private readonly Vk _vk;
        private readonly Device _dev;

        public unsafe VulkanTexture2D(VulkanRenderingDevice device, ImageC source)
        {
            _device = device;
            _vk = device.Vk;
            _dev = device.Device;
            Width = source.Width;
            Height = source.Height;

            ImageCreateInfo imgInfo = new ImageCreateInfo
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Format = Format.B8G8R8A8Unorm,
                Extent = new Extent3D((uint)Width, (uint)Height, 1),
                MipLevels = 1,
                ArrayLayers = 1,
                Samples = SampleCountFlags.Count1Bit,
                Tiling = ImageTiling.Optimal,
                Usage = ImageUsageFlags.SampledBit | ImageUsageFlags.TransferDstBit,
                SharingMode = SharingMode.Exclusive,
                InitialLayout = ImageLayout.Undefined,
            };
            Image image;
            VkCheck.Ok(_vk.CreateImage(_dev, in imgInfo, null, &image));
            Image = image;

            _vk.GetImageMemoryRequirements(_dev, Image, out MemoryRequirements req);
            MemoryAllocateInfo alloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size,
                MemoryTypeIndex = device.FindMemoryType(req.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
            };
            DeviceMemory mem;
            VkCheck.Ok(_vk.AllocateMemory(_dev, in alloc, null, &mem));
            Memory = mem;
            _vk.BindImageMemory(_dev, Image, Memory, 0);

            ImageViewCreateInfo viewInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = Image,
                ViewType = ImageViewType.Type2D,
                Format = Format.B8G8R8A8Unorm,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1),
            };
            ImageView view;
            VkCheck.Ok(_vk.CreateImageView(_dev, in viewInfo, null, &view));
            View = view;

            // Stage + upload + transition to ShaderReadOnlyOptimal.
            uint sizeBytes = (uint)(Width * Height * ImageC.PixelSize);
            BufferCreateInfo bufInfo = new BufferCreateInfo
            { SType = StructureType.BufferCreateInfo, Size = sizeBytes,
              Usage = BufferUsageFlags.TransferSrcBit, SharingMode = SharingMode.Exclusive };
            VkBuffer staging;
            VkCheck.Ok(_vk.CreateBuffer(_dev, in bufInfo, null, &staging));
            _vk.GetBufferMemoryRequirements(_dev, staging, out MemoryRequirements bufReq);
            MemoryAllocateInfo bufAlloc = new MemoryAllocateInfo
            { SType = StructureType.MemoryAllocateInfo, AllocationSize = bufReq.Size,
              MemoryTypeIndex = device.FindMemoryType(bufReq.MemoryTypeBits,
                  MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit) };
            DeviceMemory stMem;
            VkCheck.Ok(_vk.AllocateMemory(_dev, in bufAlloc, null, &stMem));
            _vk.BindBufferMemory(_dev, staging, stMem, 0);

            void* mapped;
            _vk.MapMemory(_dev, stMem, 0, sizeBytes, 0, &mapped);
            IntPtr mappedPtr = (IntPtr)mapped;
            source.GetIntPtr(ptr =>
            {
                unsafe { System.Buffer.MemoryCopy((void*)ptr, (void*)mappedPtr, sizeBytes, sizeBytes); }
            });
            _vk.UnmapMemory(_dev, stMem);

            var cb = device.BeginTransient();
            ImageMemoryBarrier toDst = new ImageMemoryBarrier
            { SType = StructureType.ImageMemoryBarrier,
              OldLayout = ImageLayout.Undefined, NewLayout = ImageLayout.TransferDstOptimal,
              SrcQueueFamilyIndex = Vk.QueueFamilyIgnored, DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
              Image = Image, SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1),
              SrcAccessMask = 0, DstAccessMask = AccessFlags.TransferWriteBit };
            _vk.CmdPipelineBarrier(cb, PipelineStageFlags.TopOfPipeBit, PipelineStageFlags.TransferBit,
                0, 0, null, 0, null, 1, in toDst);

            BufferImageCopy copy = new BufferImageCopy
            {
                BufferOffset = 0, BufferRowLength = 0, BufferImageHeight = 0,
                ImageSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1),
                ImageOffset = new Offset3D(0, 0, 0),
                ImageExtent = new Extent3D((uint)Width, (uint)Height, 1),
            };
            _vk.CmdCopyBufferToImage(cb, staging, Image, ImageLayout.TransferDstOptimal, 1, in copy);

            ImageMemoryBarrier toShader = new ImageMemoryBarrier
            { SType = StructureType.ImageMemoryBarrier,
              OldLayout = ImageLayout.TransferDstOptimal, NewLayout = ImageLayout.ShaderReadOnlyOptimal,
              SrcQueueFamilyIndex = Vk.QueueFamilyIgnored, DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
              Image = Image, SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1),
              SrcAccessMask = AccessFlags.TransferWriteBit, DstAccessMask = AccessFlags.ShaderReadBit };
            _vk.CmdPipelineBarrier(cb, PipelineStageFlags.TransferBit, PipelineStageFlags.FragmentShaderBit,
                0, 0, null, 0, null, 1, in toShader);

            device.EndAndSubmitTransient(cb);

            _vk.DestroyBuffer(_dev, staging, null);
            _vk.FreeMemory(_dev, stMem, null);
        }

        public unsafe void Dispose()
        {
            if (View.Handle != 0)  { _vk.DestroyImageView(_dev, View, null);  View = default; }
            if (Image.Handle != 0) { _vk.DestroyImage(_dev, Image, null);     Image = default; }
            if (Memory.Handle != 0){ _vk.FreeMemory(_dev, Memory, null);      Memory = default; }
        }
    }
}
