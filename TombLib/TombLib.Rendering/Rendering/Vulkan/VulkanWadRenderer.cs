using Silk.NET.Vulkan;
using System.Collections.Generic;
using TombLib.Graphics;
using TombLib.Utils;
using VkBuffer = Silk.NET.Vulkan.Buffer;

namespace TombLib.Rendering.Vulkan
{
    // Vulkan implementation of WadRenderer's atlas. The atlas is a VkImage with
    // image type 2D + ArrayLayers == page count, B8G8R8A8_UNorm. Grown by
    // allocating a fresh VkImage with N layers and copying the existing layers
    // across — same strategy as the DX11 implementation (D3D11's
    // CopySubresourceRegion is replaced by vkCmdCopyImage on a per-layer extent).
    //
    // The asset cache (Moveables/Statics dictionaries) and rect-packing logic
    // live in WadRenderer (base class) and are reused unchanged.
    public sealed class VulkanWadRenderer : WadRenderer
    {
        public VulkanRenderingDevice DeviceWrapper { get; }
        public Image AtlasImage { get; private set; }
        public DeviceMemory AtlasMemory { get; private set; }
        public ImageView AtlasView { get; private set; }
        private int _currentArrayLayers;
        private readonly Vk _vk;
        private readonly Device _device;

        // No retained-list here — old atlas resources go through the device's
        // fence-based deletion queue (QueueDestroy), which auto-destroys them
        // as soon as every registered swap chain's in-flight fence has been
        // signaled (i.e. GPU has retired any CB that could reference the old
        // ImageView via a cached descriptor set). Bounded, self-cleaning,
        // no per-session leak.

        // Caller passes this to RenderArgs.Atlas — the Vulkan drawing classes
        // (VulkanDrawingMesh / VulkanDrawingImportedGeometry) recognise both
        // ImageView and VulkanTextureAllocator via their ResolveAtlas helpers.
        public override object Texture => AtlasView;

        public VulkanWadRenderer(VulkanRenderingDevice device, bool compactTexture, bool correctTexture, int atlasSize, int maxAllocationSize, bool loadAnimations)
            : base(compactTexture, correctTexture, atlasSize, maxAllocationSize, loadAnimations)
        {
            DeviceWrapper = device;
            _vk = device.Vk;
            _device = device.Device;
        }

        protected override unsafe void OnInitializeTexture()
        {
            if (AtlasImage.Handle != 0) return;
            CreateAtlas(CurrentPageCount);
        }

        protected override unsafe void OnEnsureCapacity(int pages)
        {
            if (pages <= _currentArrayLayers) return;

            // Allocate a new larger image; copy existing layers across; swap.
            Image oldImage = AtlasImage;
            DeviceMemory oldMemory = AtlasMemory;
            ImageView oldView = AtlasView;
            int oldLayers = _currentArrayLayers;

            CreateAtlasResources(pages, out Image newImage, out DeviceMemory newMemory, out ImageView newView);

            // Copy oldLayers from oldImage→newImage via a single transient cmd.
            var cb = DeviceWrapper.BeginTransient();

            // Transition old image to TransferSrc, new image to TransferDst.
            var toSrc = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = ImageLayout.ShaderReadOnlyOptimal,
                NewLayout = ImageLayout.TransferSrcOptimal,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored, DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = oldImage,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, (uint)oldLayers),
                SrcAccessMask = AccessFlags.ShaderReadBit, DstAccessMask = AccessFlags.TransferReadBit,
            };
            _vk.CmdPipelineBarrier(cb, PipelineStageFlags.FragmentShaderBit, PipelineStageFlags.TransferBit,
                0, 0, null, 0, null, 1, in toSrc);
            var toDst = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = ImageLayout.Undefined,
                NewLayout = ImageLayout.TransferDstOptimal,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored, DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = newImage,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, (uint)pages),
                SrcAccessMask = 0, DstAccessMask = AccessFlags.TransferWriteBit,
            };
            _vk.CmdPipelineBarrier(cb, PipelineStageFlags.TopOfPipeBit, PipelineStageFlags.TransferBit,
                0, 0, null, 0, null, 1, in toDst);

            for (uint i = 0; i < (uint)oldLayers; i++)
            {
                var copy = new ImageCopy
                {
                    SrcSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, i, 1),
                    DstSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, i, 1),
                    SrcOffset = new Offset3D(0, 0, 0),
                    DstOffset = new Offset3D(0, 0, 0),
                    Extent = new Extent3D((uint)TextureAtlasSize, (uint)TextureAtlasSize, 1),
                };
                _vk.CmdCopyImage(cb, oldImage, ImageLayout.TransferSrcOptimal,
                    newImage, ImageLayout.TransferDstOptimal, 1, in copy);
            }

            // New image → ShaderReadOnly for sampling.
            var toShader = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = ImageLayout.TransferDstOptimal,
                NewLayout = ImageLayout.ShaderReadOnlyOptimal,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored, DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = newImage,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, (uint)pages),
                SrcAccessMask = AccessFlags.TransferWriteBit, DstAccessMask = AccessFlags.ShaderReadBit,
            };
            _vk.CmdPipelineBarrier(cb, PipelineStageFlags.TransferBit, PipelineStageFlags.FragmentShaderBit,
                0, 0, null, 0, null, 1, in toShader);

            DeviceWrapper.EndAndSubmitTransient(cb);

            // Defer destroy via the device-wide fence-based queue. Drained
            // automatically as soon as no swap chain has any GPU work that
            // could still reference these handles (cached per-mesh _atlasSet
            // descriptors that point at oldView).
            if (oldView.Handle != 0 || oldImage.Handle != 0 || oldMemory.Handle != 0)
            {
                var vk = _vk;
                var dev = _device;
                ImageView capView = oldView;
                Image capImage = oldImage;
                DeviceMemory capMemory = oldMemory;
                DeviceWrapper.QueueDestroy(() =>
                {
                    if (capView.Handle != 0)   vk.DestroyImageView(dev, capView, null);
                    if (capImage.Handle != 0)  vk.DestroyImage(dev, capImage, null);
                    if (capMemory.Handle != 0) vk.FreeMemory(dev, capMemory, null);
                });
            }

            AtlasImage = newImage;
            AtlasMemory = newMemory;
            AtlasView = newView;
            _currentArrayLayers = pages;
        }

        private unsafe void CreateAtlas(int pages)
        {
            CreateAtlasResources(pages, out Image image, out DeviceMemory memory, out ImageView view);
            AtlasImage = image; AtlasMemory = memory; AtlasView = view;
            _currentArrayLayers = pages;

            // Transition every layer Undefined→ShaderReadOnly so subsequent
            // OnUploadSubregion barriers can rely on the "subsequent upload"
            // path (ShaderReadOnly→TransferDst→ShaderReadOnly).
            var cb = DeviceWrapper.BeginTransient();
            var toShader = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = ImageLayout.Undefined,
                NewLayout = ImageLayout.ShaderReadOnlyOptimal,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored, DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = AtlasImage,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, (uint)pages),
                SrcAccessMask = 0, DstAccessMask = AccessFlags.ShaderReadBit,
            };
            _vk.CmdPipelineBarrier(cb, PipelineStageFlags.TopOfPipeBit, PipelineStageFlags.FragmentShaderBit,
                0, 0, null, 0, null, 1, in toShader);
            DeviceWrapper.EndAndSubmitTransient(cb);
        }

        private unsafe void CreateAtlasResources(int pages, out Image image, out DeviceMemory memory, out ImageView view)
        {
            ImageCreateInfo imgInfo = new ImageCreateInfo
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Format = Format.B8G8R8A8Unorm,
                Extent = new Extent3D((uint)TextureAtlasSize, (uint)TextureAtlasSize, 1),
                MipLevels = 1,
                ArrayLayers = (uint)pages,
                Samples = SampleCountFlags.Count1Bit,
                Tiling = ImageTiling.Optimal,
                Usage = ImageUsageFlags.SampledBit | ImageUsageFlags.TransferDstBit | ImageUsageFlags.TransferSrcBit,
                SharingMode = SharingMode.Exclusive,
                InitialLayout = ImageLayout.Undefined,
            };
            Image img;
            VkCheck.Ok(_vk.CreateImage(_device, in imgInfo, null, &img));

            _vk.GetImageMemoryRequirements(_device, img, out MemoryRequirements req);
            MemoryAllocateInfo alloc = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size,
                MemoryTypeIndex = DeviceWrapper.FindMemoryType(req.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
            };
            DeviceMemory mem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in alloc, null, &mem));
            _vk.BindImageMemory(_device, img, mem, 0);

            ImageViewCreateInfo viewInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = img,
                ViewType = ImageViewType.Type2DArray,
                Format = Format.B8G8R8A8Unorm,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, (uint)pages),
            };
            ImageView v;
            VkCheck.Ok(_vk.CreateImageView(_device, in viewInfo, null, &v));

            image = img; memory = mem; view = v;
        }

        protected override unsafe void OnUploadSubregion(ImageC image, VectorInt3 position)
        {
            if (image.Width == 0 || image.Height == 0) return;

            uint sizeBytes = (uint)(image.Width * image.Height * ImageC.PixelSize);

            // Stage host → device.
            BufferCreateInfo bi = new BufferCreateInfo
            { SType = StructureType.BufferCreateInfo, Size = sizeBytes,
              Usage = BufferUsageFlags.TransferSrcBit, SharingMode = SharingMode.Exclusive };
            VkBuffer staging;
            VkCheck.Ok(_vk.CreateBuffer(_device, in bi, null, &staging));
            _vk.GetBufferMemoryRequirements(_device, staging, out MemoryRequirements req);
            MemoryAllocateInfo alloc = new MemoryAllocateInfo
            { SType = StructureType.MemoryAllocateInfo, AllocationSize = req.Size,
              MemoryTypeIndex = DeviceWrapper.FindMemoryType(req.MemoryTypeBits,
                  MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit) };
            DeviceMemory stagingMem;
            VkCheck.Ok(_vk.AllocateMemory(_device, in alloc, null, &stagingMem));
            _vk.BindBufferMemory(_device, staging, stagingMem, 0);

            void* mapped;
            _vk.MapMemory(_device, stagingMem, 0, sizeBytes, 0, &mapped);
            System.IntPtr mappedPtr = (System.IntPtr)mapped;
            image.GetIntPtr(ptr =>
            {
                unsafe { System.Buffer.MemoryCopy((void*)ptr, (void*)mappedPtr, sizeBytes, sizeBytes); }
            });
            _vk.UnmapMemory(_device, stagingMem);

            var cb = DeviceWrapper.BeginTransient();

            // Layer is in ShaderReadOnly after CreateAtlas / previous upload —
            // bring it to TransferDst for the copy.
            var toDst = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = ImageLayout.ShaderReadOnlyOptimal,
                NewLayout = ImageLayout.TransferDstOptimal,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored, DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = AtlasImage,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, (uint)position.Z, 1),
                SrcAccessMask = AccessFlags.ShaderReadBit, DstAccessMask = AccessFlags.TransferWriteBit,
            };
            _vk.CmdPipelineBarrier(cb, PipelineStageFlags.FragmentShaderBit, PipelineStageFlags.TransferBit,
                0, 0, null, 0, null, 1, in toDst);

            BufferImageCopy copy = new BufferImageCopy
            {
                BufferOffset = 0, BufferRowLength = 0, BufferImageHeight = 0,
                ImageSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, (uint)position.Z, 1),
                ImageOffset = new Offset3D(position.X, position.Y, 0),
                ImageExtent = new Extent3D((uint)image.Width, (uint)image.Height, 1),
            };
            _vk.CmdCopyBufferToImage(cb, staging, AtlasImage, ImageLayout.TransferDstOptimal, 1, in copy);

            var toShader = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = ImageLayout.TransferDstOptimal,
                NewLayout = ImageLayout.ShaderReadOnlyOptimal,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored, DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = AtlasImage,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, (uint)position.Z, 1),
                SrcAccessMask = AccessFlags.TransferWriteBit, DstAccessMask = AccessFlags.ShaderReadBit,
            };
            _vk.CmdPipelineBarrier(cb, PipelineStageFlags.TransferBit, PipelineStageFlags.FragmentShaderBit,
                0, 0, null, 0, null, 1, in toShader);

            DeviceWrapper.EndAndSubmitTransient(cb);

            _vk.DestroyBuffer(_device, staging, null);
            _vk.FreeMemory(_device, stagingMem, null);
        }

        protected override unsafe void OnDisposeTexture()
        {
            // OnDisposeTexture is called from MULTIPLE paths:
            //   1. PanelItemPreview / Panel3D Dispose (end-of-life, between paints)
            //   2. WadRenderer.GarbageCollect (triggered by LoadedWadsChangedEvent
            //      / LevelChangedEvent — between paints, usually)
            //   3. WadRenderer.GetMoveable atlas-full rebuild path — runs MID-PAINT
            //      from inside a render call, while the swap chain CB has descriptor
            //      sets bound that reference the current atlas view
            //
            // All three cases are handled uniformly via the fence-based deletion
            // queue. The actual destroyer runs only when every registered swap
            // chain's GPU work has retired, so a mid-paint dispose is safe.
            if (AtlasView.Handle != 0 || AtlasImage.Handle != 0 || AtlasMemory.Handle != 0)
            {
                var vk = _vk;
                var dev = _device;
                ImageView capView = AtlasView;
                Image capImage = AtlasImage;
                DeviceMemory capMemory = AtlasMemory;
                DeviceWrapper.QueueDestroy(() =>
                {
                    if (capView.Handle != 0)   vk.DestroyImageView(dev, capView, null);
                    if (capImage.Handle != 0)  vk.DestroyImage(dev, capImage, null);
                    if (capMemory.Handle != 0) vk.FreeMemory(dev, capMemory, null);
                });
            }
            AtlasView = default;
            AtlasImage = default;
            AtlasMemory = default;
            _currentArrayLayers = 0;
        }
    }
}
