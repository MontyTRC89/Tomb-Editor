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
            var width  = texture.To.X - texture.From.X;
            var height = texture.To.Y - texture.From.Y;

            // Copy original region to new image
            var originalImage = ImageC.CreateNew(width+2, height+2);
            originalImage.CopyFrom(1, 1, texture.Image, texture.From.X, texture.From.Y, width, height);

            // Add 1px padding to prevent border bleeding
            originalImage.SetPixel(0, 0, originalImage.GetPixel(1, 1));
            originalImage.SetPixel(width + 1, 0, originalImage.GetPixel(width, 1));
            originalImage.SetPixel(0, height + 1, originalImage.GetPixel(1, height));
            originalImage.SetPixel(width + 1, height + 1, originalImage.GetPixel(width, height));
            originalImage.CopyFrom(0, 1, originalImage, 1, 1, 1, height);
            originalImage.CopyFrom(width + 1, 1, originalImage, width, 1, 1, height);
            originalImage.CopyFrom(1, 0, originalImage, 1, 1, width, 1);
            originalImage.CopyFrom(1, height + 1, originalImage, 1, height, width, 1);

            originalImage.GetIntPtr(ptr =>
            {
                const int mipLevelToUpload = 0;
                int subresourceIndex = MipLevelCount * pos.Z + mipLevelToUpload;
                ResourceRegion region;
                region.Left = pos.X;
                region.Right = pos.X + originalImage.Width;
                region.Top = pos.Y;
                region.Bottom = pos.Y + originalImage.Height;
                region.Front = 0;
                region.Back = 1;

                // Security clamps
                // TODO: it doesn't cover all cases and it hides a potential bug, but I can't still 
                // understand how the renderer is working
                region.Left = Math.Max(region.Left, 0);
                region.Right = Math.Min(region.Right, Size.X);
                region.Top = Math.Max(region.Top, 0);
                region.Bottom = Math.Min(region.Bottom, Size.Y);

                if (0 > region.Left || region.Left >= region.Right || region.Right > Size.X ||
                    0 > region.Top || region.Top >= region.Bottom || region.Bottom > Size.Y)
                {
                    throw new ArgumentOutOfRangeException(); // This check is important, otherwise the graphics driver may crash the entire system as it turned out.
                }

                DataBox box;
                box.DataPointer = ptr;
                box.RowPitch = originalImage.Width * ImageC.PixelSize;
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
