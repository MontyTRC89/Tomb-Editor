using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using Texture2D = SharpDX.Toolkit.Graphics.Texture2D;
using System.IO;

namespace TombLib.Graphics
{
    public static class TextureLoad
    {
        public static Texture2D Load(GraphicsDevice graphicsDevice, Utils.ImageC image)
        {
            Texture2D result = null;
            image.GetIntPtr(data =>
            {
                Texture2DDescription description;
                description.ArraySize = 1;
                description.BindFlags = BindFlags.ShaderResource;
                description.CpuAccessFlags = CpuAccessFlags.None;
                description.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
                description.Height = image.Height;
                description.MipLevels = 1;
                description.OptionFlags = ResourceOptionFlags.None;
                description.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
                description.Usage = ResourceUsage.Immutable;
                description.Width = image.Width;

                //return Texture2D.New(graphicsDevice, description, new DataBox[] { new DataBox(lockData.Scan0, lockData.Stride, 0) }); //Only for the none toolkit version which unfortunately we cannot use currently.
                result = Texture2D.New(graphicsDevice, description.Width, description.Height, description.MipLevels,
                    description.Format,
                    new[] {new DataBox(data, image.Width * Utils.ImageC.PixelSize, 0)}, TextureFlags.ShaderResource, 1,
                    description.Usage);
            });
            return result;
        }

        public static Texture2D Load(GraphicsDevice graphicsDevice, Stream stream)
        {
            return Load(graphicsDevice, Utils.ImageC.FromStream(stream));
        }

        public static Texture2D Load(GraphicsDevice graphicsDevice, string path)
        {
            return Load(graphicsDevice, Utils.ImageC.FromFile(path));
        }
    }
}
