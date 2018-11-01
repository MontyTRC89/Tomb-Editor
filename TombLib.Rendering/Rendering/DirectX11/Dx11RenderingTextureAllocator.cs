using SharpDX;
using SharpDX.Direct3D11;
using System;
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
                if (0 > region.Left || region.Left >= region.Right || region.Right > Size.X ||
                    0 > region.Top || region.Top >= region.Bottom || region.Bottom > Size.Y)
                    throw new ArgumentOutOfRangeException(); // This check is important, otherwise the graphics driver may crash the entire system as it turned out.

                DataBox box;
                box.DataPointer = ptr + (texture.From.X + texture.Image.Width * texture.From.Y) * ImageC.PixelSize;
                box.RowPitch = texture.Image.Width * ImageC.PixelSize;
                box.SlicePitch = 0;
                Context.UpdateSubresource(box, Texture, subresourceIndex, region);
            });
        }

        public override ImageC RetrieveTestImage()
        {
            const int mipLevelToRetrieve = 0;
            Texture2DDescription dx11Description;
            dx11Description.ArraySize = 1;
            dx11Description.BindFlags = BindFlags.None;
            dx11Description.CpuAccessFlags = CpuAccessFlags.Read;
            dx11Description.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            dx11Description.Height = Size.X >> mipLevelToRetrieve;
            dx11Description.MipLevels = 1;
            dx11Description.OptionFlags = ResourceOptionFlags.None;
            dx11Description.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
            dx11Description.Usage = ResourceUsage.Staging;
            dx11Description.Width = Size.Y >> mipLevelToRetrieve;
            using (Texture2D tempTexture = new Texture2D(Context.Device, dx11Description))
            {
                int bytesPerSlice = (Size.X >> mipLevelToRetrieve) * (Size.Y >> mipLevelToRetrieve) * ImageC.PixelSize;
                byte[] result = new byte[bytesPerSlice * Size.Z];
                for (int z = 0; z < Size.Z; ++z)
                {
                    int subresourceIndex = MipLevelCount * z + mipLevelToRetrieve;
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
                return ImageC.FromByteArray(result, Size.X >> mipLevelToRetrieve, (Size.Y >> mipLevelToRetrieve) * Size.Z);
            }
        }
    }
}
