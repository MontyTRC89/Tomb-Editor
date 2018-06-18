using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using TombLib.Utils;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11RenderingTextureAllocator : RenderingTextureAllocator
    {
        private const int MipLevelCount = 1; // TODO Perhaps we can align the texture and allow mip levels?

        public readonly DeviceContext Context;
        public readonly Texture2D Texture;
        public readonly ShaderResourceView TextureView;

        public Dx11RenderingTextureAllocator(Dx11RenderingDevice device, Description description)
            : base(device, description)
        {
            Context = device.Context;

            Texture2DDescription dx11Description;
            dx11Description.ArraySize = description.Size.Z;
            dx11Description.BindFlags = BindFlags.ShaderResource;
            dx11Description.CpuAccessFlags = CpuAccessFlags.None;
            dx11Description.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            dx11Description.Height = description.Size.X;
            dx11Description.MipLevels = MipLevelCount;
            dx11Description.OptionFlags = ResourceOptionFlags.None;
            dx11Description.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
            dx11Description.Usage = ResourceUsage.Default; // Perhaps dynamic could be used?
            dx11Description.Width = description.Size.Y;
            Texture = new Texture2D(device.Device, dx11Description);
            TextureView = new ShaderResourceView(device.Device, Texture);
        }

        public override void Dispose()
        {
            TextureView.Dispose();
            Texture.Dispose();
        }

        protected override void UploadTexture(RenderingTexture texture, VectorInt3 pos)
        {
            texture.Image.GetIntPtr(ptr =>
            {
                const int mipLevelToUpload = 0;
                int subresourceIndex = MipLevelCount * pos.Z + mipLevelToUpload;
                ResourceRegion region;
                region.Left = pos.X;
                region.Right = pos.X + (texture.To.X - texture.From.X);
                region.Top = pos.Y;
                region.Bottom = pos.Y + (texture.To.Y - texture.From.Y);
                region.Front = 0;
                region.Back = 1;
                DataBox box;
                box.DataPointer = ptr + (texture.From.X + texture.Image.Width * texture.From.Y) * ImageC.PixelSize;
                box.RowPitch = texture.Image.Width * ImageC.PixelSize;
                box.SlicePitch = box.RowPitch * texture.Image.Height;
                Context.UpdateSubresource(box, Texture, subresourceIndex, region);
            });
        }

        public override ImageC RetriveTestImage()
        {
            const int mipLevelToRetrive = 0;
            Texture2DDescription dx11Description;
            dx11Description.ArraySize = 1;
            dx11Description.BindFlags = BindFlags.None;
            dx11Description.CpuAccessFlags = CpuAccessFlags.Read;
            dx11Description.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            dx11Description.Height = Size.X >> mipLevelToRetrive;
            dx11Description.MipLevels = 1;
            dx11Description.OptionFlags = ResourceOptionFlags.None;
            dx11Description.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
            dx11Description.Usage = ResourceUsage.Staging;
            dx11Description.Width = Size.Y >> mipLevelToRetrive;
            using (Texture2D tempTexture = new Texture2D(Context.Device, dx11Description))
            {
                int bytesPerSlice = (Size.X >> mipLevelToRetrive) * (Size.Y >> mipLevelToRetrive) * ImageC.PixelSize;
                byte[] result = new byte[bytesPerSlice * Size.Z];
                for (int z = 0; z < Size.Z; ++z)
                {
                    int subresourceIndex = MipLevelCount * z + mipLevelToRetrive;
                    Context.CopySubresourceRegion(Texture, subresourceIndex, null, tempTexture, 0);
                    DataBox mappedBuffer = Context.MapSubresource(tempTexture, 0, MapMode.Read, MapFlags.None);
                    try
                    {
                        Marshal.Copy(mappedBuffer.DataPointer, result, bytesPerSlice * z, bytesPerSlice);
                    }
                    finally
                    {
                        Context.UnmapSubresource(tempTexture, 0);
                    }
                }
                return ImageC.FromByteArray(result, Size.X >> mipLevelToRetrive, (Size.Y >> mipLevelToRetrive) * Size.Z);
            }
        }
    }
}
