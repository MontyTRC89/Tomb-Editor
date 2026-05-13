using System;
using Silk.NET.Vulkan;
using TombLib.Utils;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace TombLib.Rendering.Vulkan
{
    // Vulkan port of the texture atlas (Texture2DArray + ImageView). The base
    // class handles rect-packing + GC; this class only owns the GPU image and
    // implements per-region upload via a transient command buffer.
    //
    // Format: B8G8R8A8_UNorm (matches ImageC's BGRA8 in-memory layout). The
    // image is created with TransferDst (for uploads) + Sampled (for shader
    // reads). Upload path: stage to a host-visible buffer, transition image
    // layout to TRANSFER_DST, vkCmdCopyBufferToImage on the affected sub-region,
    // transition layout back to SHADER_READ_ONLY.
    public sealed class VulkanTextureAllocator : RenderingTextureAllocator
    {
        public readonly VulkanRenderingDevice DeviceWrapper;
        public Image AtlasImage { get; private set; }
        public ImageView AtlasView { get; private set; }
        public DeviceMemory AtlasMemory { get; private set; }

        private readonly Vk _vk;
        private readonly Device _device;
        // Per-layer flag: true once we've ever uploaded into that array slice
        // (so its initial layout is ShaderReadOnlyOptimal, not Undefined).
        // Image layout transitions must specify the actual current layout per
        // layer; using a single flag would mis-transition layers that had never
        // been touched.
        private readonly bool[] _layerEverUploaded;

        public unsafe VulkanTextureAllocator(VulkanRenderingDevice device, Description description)
            : base(device, description)
        {
            DeviceWrapper = device;
            _vk = device.Vk;
            _device = device.Device;
            _layerEverUploaded = new bool[description.Size.Z];

            ImageCreateInfo imgInfo = new ImageCreateInfo
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Format = Format.B8G8R8A8Unorm,
                Extent = new Extent3D((uint)description.Size.X, (uint)description.Size.Y, 1),
                MipLevels = 1,
                ArrayLayers = (uint)description.Size.Z,
                Samples = SampleCountFlags.Count1Bit,
                Tiling = ImageTiling.Optimal,
                Usage = ImageUsageFlags.SampledBit | ImageUsageFlags.TransferDstBit,
                SharingMode = SharingMode.Exclusive,
                InitialLayout = ImageLayout.Undefined,
            };
            Image image;
            VkCheck.Ok(_vk.CreateImage(_device, in imgInfo, null, &image));
            AtlasImage = image;

            _vk.GetImageMemoryRequirements(_device, AtlasImage, out MemoryRequirements req);
            MemoryAllocateInfo alloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size,
                MemoryTypeIndex = device.FindMemoryType(req.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
            };
            DeviceMemory mem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in alloc, null, &mem));
            AtlasMemory = mem;
            _vk.BindImageMemory(_device, AtlasImage, AtlasMemory, 0);

            ImageViewCreateInfo viewInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = AtlasImage,
                ViewType = ImageViewType.Type2DArray,
                Format = Format.B8G8R8A8Unorm,
                SubresourceRange = new ImageSubresourceRange(
                    ImageAspectFlags.ColorBit, 0, 1, 0, (uint)description.Size.Z),
            };
            ImageView view;
            VkCheck.Ok(_vk.CreateImageView(_device, in viewInfo, null, &view));
            AtlasView = view;
        }

        public override unsafe void Dispose()
        {
            if (AtlasView.Handle != 0) { _vk.DestroyImageView(_device, AtlasView, null); AtlasView = default; }
            if (AtlasImage.Handle != 0) { _vk.DestroyImage(_device, AtlasImage, null); AtlasImage = default; }
            if (AtlasMemory.Handle != 0) { _vk.FreeMemory(_device, AtlasMemory, null); AtlasMemory = default; }
        }

        protected override unsafe void UploadTexture(RenderingTexture texture, VectorInt3 pos)
        {
            int width = texture.To.X - texture.From.X;
            int height = texture.To.Y - texture.From.Y;

            // 1-pixel mirrored border to prevent atlas bleeding under linear
            // sampling — same trick as the Dx11 path.
            var padded = ImageC.CreateNew(width + 2, height + 2);
            padded.CopyFrom(1, 1, texture.Image, texture.From.X, texture.From.Y, width, height);
            padded.SetPixel(0, 0,         padded.GetPixel(1, 1));
            padded.SetPixel(width + 1, 0, padded.GetPixel(width, 1));
            padded.SetPixel(0, height + 1,         padded.GetPixel(1, height));
            padded.SetPixel(width + 1, height + 1, padded.GetPixel(width, height));
            padded.CopyFrom(0, 1, padded, 1, 1, 1, height);
            padded.CopyFrom(width + 1, 1, padded, width, 1, 1, height);
            padded.CopyFrom(1, 0, padded, 1, 1, width, 1);
            padded.CopyFrom(1, height + 1, padded, 1, height, width, 1);

            uint regionWidth = (uint)padded.Width;
            uint regionHeight = (uint)padded.Height;
            uint sizeBytes = regionWidth * regionHeight * (uint)ImageC.PixelSize;

            // Staging buffer: HOST_VISIBLE | HOST_COHERENT so we can map+memcpy
            // without an explicit flush.
            BufferCreateInfo bufInfo = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = sizeBytes,
                Usage = BufferUsageFlags.TransferSrcBit,
                SharingMode = SharingMode.Exclusive,
            };
            VkBuffer staging;
            VkCheck.Ok(_vk.CreateBuffer(_device, in bufInfo, null, &staging));

            _vk.GetBufferMemoryRequirements(_device, staging, out MemoryRequirements bufReq);
            MemoryAllocateInfo bufAlloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = bufReq.Size,
                MemoryTypeIndex = DeviceWrapper.FindMemoryType(bufReq.MemoryTypeBits,
                    MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit),
            };
            DeviceMemory stagingMem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in bufAlloc, null, &stagingMem));
            _vk.BindBufferMemory(_device, staging, stagingMem, 0);

            void* mapped;
            VkCheck.Ok(_vk.MapMemory(_device, stagingMem, 0, sizeBytes, 0, &mapped));
            // Hoist the pointer out of the closure capture: lambdas can't take
            // the address of locals, so route through an IntPtr local that the
            // closure CAN capture.
            IntPtr mappedPtr = (IntPtr)mapped;
            padded.GetIntPtr(ptr =>
            {
                unsafe
                {
                    System.Buffer.MemoryCopy((void*)ptr, (void*)mappedPtr, sizeBytes, sizeBytes);
                }
            });
            _vk.UnmapMemory(_device, stagingMem);

            // Record + submit the copy + layout transitions.
            var cb = DeviceWrapper.BeginTransient();

            // First upload to a layer transitions Undefined → TransferDstOptimal.
            // Subsequent uploads transition ShaderReadOnlyOptimal → TransferDstOptimal.
            bool firstForLayer = !_layerEverUploaded[pos.Z];
            ImageMemoryBarrier toDst = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = firstForLayer ? ImageLayout.Undefined : ImageLayout.ShaderReadOnlyOptimal,
                NewLayout = ImageLayout.TransferDstOptimal,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = AtlasImage,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit,
                    0, 1, (uint)pos.Z, 1),
                SrcAccessMask = firstForLayer ? 0 : AccessFlags.ShaderReadBit,
                DstAccessMask = AccessFlags.TransferWriteBit,
            };
            _vk.CmdPipelineBarrier(cb,
                firstForLayer ? PipelineStageFlags.TopOfPipeBit : PipelineStageFlags.FragmentShaderBit,
                PipelineStageFlags.TransferBit,
                0, 0, null, 0, null, 1, in toDst);

            BufferImageCopy copy = new BufferImageCopy
            {
                BufferOffset = 0,
                BufferRowLength = 0,
                BufferImageHeight = 0,
                ImageSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, (uint)pos.Z, 1),
                ImageOffset = new Offset3D(pos.X, pos.Y, 0),
                ImageExtent = new Extent3D(regionWidth, regionHeight, 1),
            };
            _vk.CmdCopyBufferToImage(cb, staging, AtlasImage,
                ImageLayout.TransferDstOptimal, 1, in copy);

            ImageMemoryBarrier toShader = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = ImageLayout.TransferDstOptimal,
                NewLayout = ImageLayout.ShaderReadOnlyOptimal,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = AtlasImage,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit,
                    0, 1, (uint)pos.Z, 1),
                SrcAccessMask = AccessFlags.TransferWriteBit,
                DstAccessMask = AccessFlags.ShaderReadBit,
            };
            _vk.CmdPipelineBarrier(cb,
                PipelineStageFlags.TransferBit,
                PipelineStageFlags.FragmentShaderBit,
                0, 0, null, 0, null, 1, in toShader);

            DeviceWrapper.EndAndSubmitTransient(cb);

            _vk.DestroyBuffer(_device, staging, null);
            _vk.FreeMemory(_device, stagingMem, null);

            _layerEverUploaded[pos.Z] = true;
        }

        // Debug-only readback. The legacy diagnostic isn't ported.
        public override ImageC RetrieveTestImage() => ImageC.CreateNew(Size.X, Size.Y);
    }
}
