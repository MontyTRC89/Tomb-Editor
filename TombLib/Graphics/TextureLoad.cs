using NLog;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Texture2D = SharpDX.Toolkit.Graphics.Texture2D;

namespace TombLib.Graphics
{
    public static class TextureLoad
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static Texture2D Load(GraphicsDevice graphicsDevice, Utils.ImageC image)
        {
            Texture2D result = null;

            try
            {
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
                        description.Usage = ResourceUsage.Immutable;
                        description.Width = image.Width;

                    //return Texture2D.New(graphicsDevice, description, new DataBox[] { new DataBox(lockData.Scan0, lockData.Stride, 0) }); //Only for the none toolkit version which unfortunately we cannot use currently.
                    result = Texture2D.New(graphicsDevice, description.Width, description.Height, description.MipLevels, description.Format,
                            new SharpDX.DataBox[] { new SharpDX.DataBox(data, image.Width * Utils.ImageC.PixelSize, 0) }, TextureFlags.ShaderResource, 1, description.Usage);
                    });
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Unable to create DirectX texture (image size: " + image.Size);
            }
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
