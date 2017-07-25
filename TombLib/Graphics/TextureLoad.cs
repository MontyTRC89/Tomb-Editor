using System;
using System.Collections.Generic;
using System.IO;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using Texture2D = SharpDX.Toolkit.Graphics.Texture2D;
using Effect = SharpDX.Toolkit.Graphics.Effect;

namespace TombLib.Graphics
{
    public static class TextureLoad
    {
        public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            //Avoid calling this function to avoid the Direct2D1 dependency.
            using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(stream))
            {
                var lockData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
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
        }
    }
} 