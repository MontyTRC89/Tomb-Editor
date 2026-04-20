using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using System;
using System.Runtime.InteropServices;
using TombLib.Utils;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11RenderingTextureAllocator : RenderingTextureAllocator
    {
        public readonly ID3D11DeviceContext Context;
        public readonly ID3D11Texture2D Texture;
        public readonly ID3D11ShaderResourceView TextureView;

        public Dx11RenderingTextureAllocator(Dx11RenderingDevice device, Description description)
            : base(device, description)
        {
            Context = device.Context;
            Texture = device.Device.CreateTexture2D(device.CreateTextureDescription(description.Size));
            TextureView = device.Device.CreateShaderResourceView(Texture);
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
                int subresourceIndex = pos.Z + mipLevelToUpload;
                var region = new Box(pos.X, pos.Y, 0, pos.X + originalImage.Width, pos.Y + originalImage.Height, 1);

                // Security clamps
                int left = Math.Max(region.Left, 0);
                int right = Math.Min(region.Right, Size.X);
                int top = Math.Max(region.Top, 0);
                int bottom = Math.Min(region.Bottom, Size.Y);

                if (0 > left || left >= right || right > Size.X ||
                    0 > top || top >= bottom || bottom > Size.Y)
                {
                    throw new ArgumentOutOfRangeException("texture.From.X = " + texture.From.X + ", " +
                                                          "texture.From.Y = " + texture.From.Y + ", " +
                                                          "texture.To.X = " + texture.To.X + ", " +
                                                          "texture.To.Y = " + texture.To.Y + ", " +
                                                          "pos.X = " + pos.X + ", " +
                                                          "pos.Y = " + pos.Y + ", " +
                                                          "region.Left = " + region.Left + ", "+
                                                          "region.Right = " + region.Right + ", " +
                                                          "region.Top = " + region.Top + ", " +
                                                          "region.Bottom = " + region.Bottom );
                }

                region = new Box(left, top, 0, right, bottom, 1);
                int rowPitch = originalImage.Width * ImageC.PixelSize;
                Context.UpdateSubresource(Texture, (uint)subresourceIndex, region, ptr, (uint)rowPitch, 0u);
            });
        }

        public override ImageC RetrieveTestImage()
        {
            const int mipLevelToRetrieve = 0;
            var dx11Description = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.None,
                CPUAccessFlags = CpuAccessFlags.Read,
                Format = Format.B8G8R8A8_UNorm,
                Height = (uint)(Size.X >> mipLevelToRetrieve),
                MipLevels = 1,
                MiscFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Staging,
                Width = (uint)(Size.Y >> mipLevelToRetrieve),
            };
            using (var tempTexture = Context.Device.CreateTexture2D(dx11Description))
            {
                int bytesPerSlice = (Size.X >> mipLevelToRetrieve) * (Size.Y >> mipLevelToRetrieve) * ImageC.PixelSize;
                byte[] result = new byte[bytesPerSlice * Size.Z];
                for (int z = 0; z < Size.Z; ++z)
                {
                    int subresourceIndex = z + mipLevelToRetrieve;
                    Context.CopySubresourceRegion(tempTexture, 0, 0, 0, 0, Texture, (uint)subresourceIndex);
                    var mapped = Context.Map(tempTexture, 0, MapMode.Read);
                    try
                    {
                        Marshal.Copy(mapped.DataPointer, result, bytesPerSlice * z, bytesPerSlice);
                    }
                    finally
                    {
                        Context.Unmap(tempTexture, 0);
                    }
                }
                return ImageC.FromByteArray(result, Size.X >> mipLevelToRetrieve, (Size.Y >> mipLevelToRetrieve) * Size.Z);
            }
        }
    }
}
