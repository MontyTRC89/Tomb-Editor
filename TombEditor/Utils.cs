using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Paloma;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using TombEditor.Geometry;
using zlib;
using System.Diagnostics;
using NLog;

namespace TombEditor
{
    class Utils
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static Vector3 PositionInWorldCoordinates(Vector3 pos)
        {
            return new Vector3(pos.X * 1024.0f, pos.Y * 256.0f, pos.Z * 1024.0f);
        }

        public static bool IsIntegerNumber(string value)
        {
            long number;
            return Int64.TryParse(value, out number);
        }

        public static bool ConvertTGAtoPNG(string filename, out string newName)
        {
            logger.Debug("Converting TGA texture map to PNG format");

            newName = "";

            Stopwatch watch = new Stopwatch();
            watch.Start();
                       
            Bitmap tga = Paloma.TargaImage.LoadTargaImage(filename);
            if ((tga.Width != 256 && tga.Width != 512) || (tga.Height % 256 != 0 && tga.Height % 128 != 0 && tga.Height % 64 != 0)) return false;
            
            int height = tga.Height * (tga.Width == 256 ? 1 : 2);
            int numPages = height / 256;
            if (height % 256 != 0) numPages++;

            Bitmap png = new Bitmap(256, numPages*256);

            if (tga.Width == 256)
            {
                Graphics g = Graphics.FromImage(png);
                g.DrawImage(tga, 0, 0);
                /*
                for (int y = 0; y < tga.Height; y++)
                {
                    for (int x = 0; x < tga.Width; x++)
                    {
                        png.SetPixel(x, y, tga.GetPixel(x, y));
                    }
                }*/
            }
            else
            {
                Graphics g = Graphics.FromImage(png);

                int numXtiles = 8;
                int numYtiles = tga.Height / 64;

                for (int yy = 0; yy < numYtiles; yy++)
                {
                    for (int xx = 0; xx < numXtiles; xx++)
                    {
                        if (xx >= 4)
                        {
                            System.Drawing.RectangleF src = new System.Drawing.RectangleF(64 * xx, 64 * yy, 64, 64);
                            System.Drawing.RectangleF dest = new System.Drawing.RectangleF(64 * (xx - 4), 64 * yy * 2 + 64, 64, 64);

                            g.DrawImage(tga, dest, src, GraphicsUnit.Pixel);
                        }
                        else
                        {
                            System.Drawing.RectangleF src = new System.Drawing.RectangleF(64 * xx, 64 * yy, 64, 64);
                            System.Drawing.RectangleF dest = new System.Drawing.RectangleF(64 * xx, 64 * yy * 2, 64, 64);

                            g.DrawImage(tga, dest, src, GraphicsUnit.Pixel);
                        }

                        /*for (int y = 0; y < 64; y++)
                        {
                            for (int x = 0; x < 64; x++)
                            {
                                if (xx >= 4)
                                    png.SetPixel(64 * (xx - 4) + x, 64 * yy * 2 + 64 + y, tga.GetPixel(64 * xx + x, 64 * yy + y));
                                else
                                    png.SetPixel(64 * xx + x, 64 * yy * 2 + y, tga.GetPixel(64 * xx + x, 64 * yy + y));
                            }
                        }*/


                    }
                }
            }

            newName = "Textures\\Imported\\" + Path.GetFileNameWithoutExtension(filename) + ".png";
            png.Save(newName, ImageFormat.Png);
            png.Dispose();
            tga.Dispose();

            watch.Stop();

            logger.Debug("Texture map converted");
            logger.Debug("    Elapsed time: " + watch.ElapsedMilliseconds + " ms");

            png = null;
            tga = null;

            GC.Collect();

            return true;
        }

        public static int GetWorldX(Room room, int x)
        {
            return (int)(x + room.Position.X);
        }

        public static int GetWorldZ(Room room, int z)
        {
            return (int)(z + room.Position.Z);
        }

        public static Bitmap GetTextureTileFromMap(int xc, int yc, int page)
        {
            Bitmap bmp = new Bitmap(64, 64);

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    System.Drawing.Color color = Editor.Instance.Level.TextureMap.GetPixel(xc + x, page * 256 + yc + y);
                    bmp.SetPixel(x, y, color);
                }
            }

            return bmp;
        }

        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        public static byte[] CompressDataZLIB(byte[] data)
        {
            MemoryStream outFileStream = new MemoryStream();
            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outFileStream, zlib.zlibConst.Z_BEST_COMPRESSION);
            MemoryStream inFileStream = new MemoryStream(data);
            try
            {
                CopyStream(inFileStream, outZStream);
            }
            finally
            {
                outZStream.Close();
                outFileStream.Close();
                inFileStream.Close();
            }

            return outFileStream.ToArray();
        }

        public static byte[] DecompressData(byte[] inData)
        {
            byte[] outData;

            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }

            return outData;
        }
    }
}
