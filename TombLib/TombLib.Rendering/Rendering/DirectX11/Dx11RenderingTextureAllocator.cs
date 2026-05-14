using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Runtime.InteropServices;
using TombLib.Utils;
using Format = Silk.NET.DXGI.Format;
using D3D11Usage = Silk.NET.Direct3D11.Usage;

namespace TombLib.Rendering.DirectX11
{
    public unsafe class Dx11RenderingTextureAllocator : RenderingTextureAllocator
    {
        public readonly ID3D11DeviceContext* Context;
        public readonly ID3D11Texture2D* Texture;
        public readonly ID3D11ShaderResourceView* TextureView;

        // Keep a reference to the device so we can create staging textures for readback.
        private readonly ID3D11Device* _device;

        public Dx11RenderingTextureAllocator(Dx11RenderingDevice device, Description description)
            : base(device, description)
        {
            Context = device.Context;
            _device = device.Device;

            var texDesc = device.CreateTextureDescription(description.Size);
            ID3D11Texture2D* tex;
            SilkMarshal.ThrowHResult(device.Device->CreateTexture2D(&texDesc, null, &tex));
            Texture = tex;

            ID3D11ShaderResourceView* srv;
            SilkMarshal.ThrowHResult(device.Device->CreateShaderResourceView((ID3D11Resource*)Texture, null, &srv));
            TextureView = srv;
        }

        public override void Dispose()
        {
            TextureView->Release();
            Texture->Release();
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

                // Use Box instead of ResourceRegion (Silk.NET equivalent).
                var box = new Box();
                box.Left = (uint)pos.X;
                box.Right = (uint)(pos.X + originalImage.Width);
                box.Top = (uint)pos.Y;
                box.Bottom = (uint)(pos.Y + originalImage.Height);
                box.Front = 0;
                box.Back = 1;

                // Security clamps
                // TODO: it doesn't cover all cases and it hides a potential bug, but I can't still
                // understand how the renderer is working
                box.Left = (uint)Math.Max((int)box.Left, 0);
                box.Right = (uint)Math.Min((int)box.Right, Size.X);
                box.Top = (uint)Math.Max((int)box.Top, 0);
                box.Bottom = (uint)Math.Min((int)box.Bottom, Size.Y);

                if (0 > (int)box.Left || box.Left >= box.Right || box.Right > (uint)Size.X ||
                    0 > (int)box.Top || box.Top >= box.Bottom || box.Bottom > (uint)Size.Y)
                {
                    // This check is important, otherwise the graphics driver may crash the entire system as it turned out.
                    throw new ArgumentOutOfRangeException("texture.From.X = " + texture.From.X + ", " +
                                                          "texture.From.Y = " + texture.From.Y + ", " +
                                                          "texture.To.X = " + texture.To.X + ", " +
                                                          "texture.To.Y = " + texture.To.Y + ", " +
                                                          "pos.X = " + pos.X + ", " +
                                                          "pos.Y = " + pos.Y + ", " +
                                                          "region.Left = " + box.Left + ", "+
                                                          "region.Right = " + box.Right + ", " +
                                                          "region.Top = " + box.Top + ", " +
                                                          "region.Bottom = " + box.Bottom );
                }

                uint rowPitch = (uint)(originalImage.Width * ImageC.PixelSize);
                Context->UpdateSubresource(
                    (ID3D11Resource*)Texture,
                    (uint)subresourceIndex,
                    &box,
                    (void*)ptr,
                    rowPitch,
                    0);
            });
        }

        public override ImageC RetrieveTestImage()
        {
            const int mipLevelToRetrieve = 0;

            var dx11Description = new Texture2DDesc
            {
                ArraySize = 1,
                BindFlags = 0,
                CPUAccessFlags = (uint)CpuAccessFlag.Read,
                Format = Format.FormatB8G8R8A8Unorm,
                Height = (uint)(Size.X >> mipLevelToRetrieve),
                MipLevels = 1,
                MiscFlags = 0,
                SampleDesc = new SampleDesc(1, 0),
                Usage = D3D11Usage.Staging,
                Width = (uint)(Size.Y >> mipLevelToRetrieve),
            };

            ID3D11Texture2D* tempTexture;
            SilkMarshal.ThrowHResult(_device->CreateTexture2D(&dx11Description, null, &tempTexture));
            try
            {
                int bytesPerSlice = (Size.X >> mipLevelToRetrieve) * (Size.Y >> mipLevelToRetrieve) * ImageC.PixelSize;
                byte[] result = new byte[bytesPerSlice * Size.Z];
                for (int z = 0; z < Size.Z; ++z)
                {
                    int subresourceIndex = z + mipLevelToRetrieve;

                    // CopySubresourceRegion: dst first, src second (Silk.NET convention).
                    // Copy from the atlas texture (source) into the staging texture (destination).
                    Context->CopySubresourceRegion(
                        (ID3D11Resource*)tempTexture, // destination
                        0,                            // dst subresource
                        0, 0, 0,                      // dst x, y, z
                        (ID3D11Resource*)Texture,     // source
                        (uint)subresourceIndex,       // src subresource
                        null);                        // src box (null = entire subresource)

                    MappedSubresource mapped;
                    SilkMarshal.ThrowHResult(
                        Context->Map((ID3D11Resource*)tempTexture, 0, Silk.NET.Direct3D11.Map.Read, 0, &mapped));
                    try
                    {
                        Marshal.Copy((IntPtr)mapped.PData, result, bytesPerSlice * z, bytesPerSlice);
                    }
                    finally
                    {
                        Context->Unmap((ID3D11Resource*)tempTexture, 0);
                    }
                }
                return ImageC.FromByteArray(result, Size.X >> mipLevelToRetrieve, (Size.Y >> mipLevelToRetrieve) * Size.Z);
            }
            finally
            {
                tempTexture->Release();
            }
        }
    }
}
