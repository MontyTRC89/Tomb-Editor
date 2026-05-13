using SharpDX.Direct3D11;
using TombLib.Utils;

namespace TombLib.Graphics
{
    // SharpDX/D3D11 implementation of WadRenderer's atlas. Holds a
    // Texture2DArray + ShaderResourceView. The legacy WadRenderer body now
    // lives here unchanged in spirit — only the atlas-storage bits.
    public class Dx11WadRenderer : WadRenderer
    {
        public Device Device { get; }
        public Texture2D AtlasTexture { get; private set; }
        public ShaderResourceView TextureView { get; private set; }

        public override object Texture => TextureView;

        public Dx11WadRenderer(Device device, bool compactTexture, bool correctTexture, int atlasSize, int maxAllocationSize, bool loadAnimations)
            : base(compactTexture, correctTexture, atlasSize, maxAllocationSize, loadAnimations)
        {
            Device = device;
        }

        protected override void OnInitializeTexture()
        {
            if (AtlasTexture is not null) return;
            AtlasTexture = new Texture2D(Device, new Texture2DDescription
            {
                Width = TextureAtlasSize,
                Height = TextureAtlasSize,
                MipLevels = 1,
                ArraySize = CurrentPageCount,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            });
            TextureView = new ShaderResourceView(Device, AtlasTexture);
        }

        protected override void OnEnsureCapacity(int pages)
        {
            int arraySize = AtlasTexture.Description.ArraySize;
            if (pages <= arraySize) return;

            var newDesc = AtlasTexture.Description;
            newDesc.ArraySize = pages;
            var newTexture = new Texture2D(Device, newDesc);
            for (int i = 0; i < arraySize; i++)
            {
                int from = Texture2D.CalculateSubResourceIndex(0, i, 1);
                int to = Texture2D.CalculateSubResourceIndex(0, i, 1);
                Device.ImmediateContext.CopySubresourceRegion(AtlasTexture, from, null, newTexture, to);
            }
            TextureView?.Dispose();
            AtlasTexture?.Dispose();
            AtlasTexture = newTexture;
            TextureView = new ShaderResourceView(Device, AtlasTexture);
        }

        protected override void OnUploadSubregion(ImageC image, VectorInt3 position)
        {
            TextureLoad.Update(Device.ImmediateContext, AtlasTexture, image, position);
        }

        protected override void OnDisposeTexture()
        {
            TextureView?.Dispose();
            TextureView = null;
            AtlasTexture?.Dispose();
            AtlasTexture = null;
        }
    }
}
