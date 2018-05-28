using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            Texture2DDescription Dx11Description;
            Dx11Description.ArraySize = description.Size.Z;
            Dx11Description.BindFlags = BindFlags.ShaderResource;
            Dx11Description.CpuAccessFlags = CpuAccessFlags.None;
            Dx11Description.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            Dx11Description.Height = description.Size.X;
            Dx11Description.MipLevels = MipLevelCount;
            Dx11Description.OptionFlags = ResourceOptionFlags.None;
            Dx11Description.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
            Dx11Description.Usage = ResourceUsage.Default; // Perhaps dynamic could be used?
            Dx11Description.Width = description.Size.Y;
            Texture = new Texture2D(device.Device, Dx11Description);
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
                const int MipLevel = 0;
                int subresourceIndex = MipLevelCount * pos.Z + MipLevel;
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
    }
}
