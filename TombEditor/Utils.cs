using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
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

        public static void ConvertTextureTo256Width(ref Bitmap bitmap)
        {
            if ((bitmap.Height % 64) != 0)
                throw new ArgumentException("Image height must be of a multiple of 64 pixels.");
            if (bitmap.Width == 256)
                return;
            if (bitmap.Width != 512)
                throw new ArgumentException("Image width must be either 256 or 512 pixels.");

            Bitmap newBitmap = new Bitmap(256, bitmap.Height * 2);
            try
            {
                using (Graphics g = Graphics.FromImage(newBitmap))
                {
                    int numXtiles = 8;
                    int numYtiles = bitmap.Height / 64;

                    for (int yy = 0; yy < numYtiles; yy++)
                        for (int xx = 0; xx < numXtiles; xx++)
                        {
                            if (xx >= 4)
                            {
                                System.Drawing.RectangleF src = new System.Drawing.RectangleF(64 * xx, 64 * yy, 64, 64);
                                System.Drawing.RectangleF dest = new System.Drawing.RectangleF(64 * (xx - 4), 64 * yy * 2 + 64, 64, 64);

                                g.DrawImage(bitmap, dest, src, GraphicsUnit.Pixel);
                            }
                            else
                            {
                                System.Drawing.RectangleF src = new System.Drawing.RectangleF(64 * xx, 64 * yy, 64, 64);
                                System.Drawing.RectangleF dest = new System.Drawing.RectangleF(64 * xx, 64 * yy * 2, 64, 64);

                                g.DrawImage(bitmap, dest, src, GraphicsUnit.Pixel);
                            }
                        }
                }
            }
            catch (Exception)
            {
                newBitmap.Dispose();
                throw;
            }
            bitmap = newBitmap;
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
                    System.Drawing.Color color = Editor.Instance.Level._textureMap.GetPixel(xc + x, page * 256 + yc + y);
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
