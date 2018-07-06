using NLog;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using System;
using System.IO;
using TombLib.Utils;
using Texture2D = SharpDX.Toolkit.Graphics.Texture2D;

namespace TombLib.Graphics
{
    public static class TextureLoad
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static Texture2D Load(GraphicsDevice graphicsDevice, ImageC image, ResourceUsage usage = ResourceUsage.Immutable)
        {
            Texture2D result = null;
            image.GetIntPtr((IntPtr data) =>
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
                    description.Usage = usage;
                    description.Width = image.Width;

                        //return Texture2D.New(graphicsDevice, description, new DataBox[] { new DataBox(lockData.Scan0, lockData.Stride, 0) }); //Only for the none toolkit version which unfortunately we cannot use currently.
                        result = Texture2D.New(graphicsDevice, description.Width, description.Height, description.MipLevels, description.Format,
                                new[] { new SharpDX.DataBox(data, image.Width * ImageC.PixelSize, 0) }, TextureFlags.ShaderResource, 1, description.Usage);
                });
            return result;
        }

        public static Texture2D Load(GraphicsDevice graphicsDevice, Stream stream)
        {
            return Load(graphicsDevice, ImageC.FromStream(stream));
        }

        public static Texture2D Load(GraphicsDevice graphicsDevice, string path)
        {
            return Load(graphicsDevice, ImageC.FromFile(path));
        }

        public static void Update(GraphicsDevice graphicsDevice, Texture2D texture, ImageC image, VectorInt2 position)
        {
            if (image.Width == 0 || image.Height == 0)
                return;
            var deviceContext = (DeviceContext)graphicsDevice;
            image.GetIntPtr((IntPtr data) =>
            {
                ResourceRegion region;
                region.Left = position.X;
                region.Right = position.X + image.Width;
                region.Top = position.Y;
                region.Bottom = position.Y + image.Height;
                region.Front = 0;
                region.Back = 1;
                texture.SetData(graphicsDevice, new SharpDX.DataPointer(data, image.DataSize), 0, 0, region);
            });
        }
    }
}
