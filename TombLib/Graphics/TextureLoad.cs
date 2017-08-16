using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using Texture2D = SharpDX.Toolkit.Graphics.Texture2D;
using Rectangle = System.Drawing.Rectangle;

namespace TombLib.Graphics
{
    public static class TextureLoad
    {
        public static Bitmap LoadToBitmap(Stream stream)
        {
            long PreviousPosition = stream.Position;
            // First try to open it with .Net methods
            try
            {
                return (Bitmap)Bitmap.FromStream(stream);
            }
            catch (ArgumentException) //Fires if default .NET methods fail 
            { // Try to open it as tga file
                stream.Position = PreviousPosition;
                return Paloma.TargaImage.LoadTargaImage(stream);
            }
        }
        public static Bitmap LoadToBitmap(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return LoadToBitmap(stream);
        }
        public static Texture2D LoadToTexture(GraphicsDevice graphicsDevice, Bitmap bitmap)
        {
            var lockData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                Texture2DDescription description;
                description.ArraySize = 1;
                description.BindFlags = BindFlags.ShaderResource;
                description.CpuAccessFlags = CpuAccessFlags.None;
                description.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
                description.Height = bitmap.Height;
                description.MipLevels = 1;
                description.OptionFlags = ResourceOptionFlags.None;
                description.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
                description.Usage = ResourceUsage.Immutable;
                description.Width = bitmap.Width;

                //return Texture2D.New(graphicsDevice, description, new DataBox[] { new DataBox(lockData.Scan0, lockData.Stride, 0) }); //Only for the none toolkit version which unfortunately we cannot use currently.
                return Texture2D.New(graphicsDevice, description.Width, description.Height, description.MipLevels, description.Format,
                    new DataBox[] { new DataBox(lockData.Scan0, lockData.Stride, 0) }, TextureFlags.ShaderResource, 1, description.Usage);
            }
            finally
            {
                bitmap.UnlockBits(lockData);
            }
        }
        public static Texture2D LoadToTexture(GraphicsDevice graphicsDevice, Stream stream)
        {
            using (Bitmap bitmap = LoadToBitmap(stream))
                return LoadToTexture(graphicsDevice, bitmap);
        }
        public static Texture2D LoadToTexture(GraphicsDevice graphicsDevice, string path)
        {
            using (Bitmap bitmap = LoadToBitmap(path))
                return LoadToTexture(graphicsDevice, bitmap);
        }
    }
} 
