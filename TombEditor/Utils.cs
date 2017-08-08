using System;
using System.Collections.Generic;
using System.Text;
using SharpDX;
using System.Drawing;
using System.IO;
using TombEditor.Geometry;
using zlib;
using NLog;
using System.Runtime.InteropServices;

namespace TombEditor
{
    public static class Utils
    {
        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;

        [DllImport("shlwapi.dll", SetLastError = true)]
        private static extern int PathRelativePathTo(StringBuilder pszPath, string pszFrom, int dwAttrFrom,
            string pszTo, int dwAttrTo);

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        static extern bool PathIsRelative([In] string lpszPath);

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
                                System.Drawing.RectangleF dest =
                                    new System.Drawing.RectangleF(64 * (xx - 4), 64 * yy * 2 + 64, 64, 64);

                                g.DrawImage(bitmap, dest, src, GraphicsUnit.Pixel);
                            }
                            else
                            {
                                System.Drawing.RectangleF src = new System.Drawing.RectangleF(64 * xx, 64 * yy, 64, 64);
                                System.Drawing.RectangleF
                                    dest = new System.Drawing.RectangleF(64 * xx, 64 * yy * 2, 64, 64);

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

        public static Bitmap GetTextureTileFromMap(Level level, int xc, int yc, int page)
        {
            Bitmap bmp = new Bitmap(64, 64);

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    System.Drawing.Color color = level.TextureMap.GetPixel(xc + x, page * 256 + yc + y);
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

        public static string GetRelativePath(string prjFileName, string fileName)
        {
            // If fileName is already a relative path return it, PathRelativePathTo() would return error
            if (PathIsRelative(fileName)) return fileName;

            string pathOfPrj = Path.GetDirectoryName(prjFileName) + "\\";

            // Use PathRelativePathTo() for returning the relative path
            StringBuilder sb = new StringBuilder(260);
            int result = PathRelativePathTo(sb, pathOfPrj, FILE_ATTRIBUTE_DIRECTORY, fileName, FILE_ATTRIBUTE_NORMAL);
            if (result == 0) return fileName;

            return sb.ToString();
        }

        public static bool Contains(this SharpDX.Rectangle This, Point point)
        {
            return This.Contains(point.X, point.Y);
        }

        public static void DrawRectangle(this Graphics g, Pen pen, System.Drawing.RectangleF rectangle)
        {
            g.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static Point Max(this Point point0, Point point1)
        {
            return new Point(Math.Max(point0.X, point1.X), Math.Max(point0.Y, point1.Y));
        }

        public static PointF Max(this PointF point0, PointF point1)
        {
            return new PointF(Math.Max(point0.X, point1.X), Math.Max(point0.Y, point1.Y));
        }

        public static Point Min(this Point point0, Point point1)
        {
            return new Point(Math.Min(point0.X, point1.X), Math.Min(point0.Y, point1.Y));
        }

        public static PointF Min(this PointF point0, PointF point1)
        {
            return new PointF(Math.Min(point0.X, point1.X), Math.Min(point0.Y, point1.Y));
        }

        public static bool ContainedBetween(this Point point, Point point0, Point point1)
        {
            return
                (point.X <= Math.Min(point0.X, point1.X)) &&
                (point.Y <= Math.Min(point0.Y, point1.Y)) &&
                (point.X >= Math.Max(point0.X, point1.X)) &&
                (point.X >= Math.Max(point0.X, point1.X));
        }

        public static bool ContainedBetween(this PointF point, PointF point0, PointF point1)
        {
            return
                (point.X <= Math.Min(point0.X, point1.X)) &&
                (point.Y <= Math.Min(point0.Y, point1.Y)) &&
                (point.X >= Math.Max(point0.X, point1.X)) &&
                (point.X >= Math.Max(point0.X, point1.X));
        }

        public static int ReferenceIndexOf<T>(this IList<T> list, T needle)
        {
            // This is not implemented for IEnumerable on purpose to avoid abuse of this method on non ordered containers.
            // (HashSet, Dictionary, ...)

            if (needle == null)
                return -1;

            for (int i = 0; i < list.Count; ++i)
                if (ReferenceEquals(list[i], needle))
                    return i;

            return -1;
        }

        public static IEnumerable<T> Unwrap<T>(this T[,] array)
        {
            for (int x = 0; x < array.GetLength(0); ++x)
                for (int y = 0; y < array.GetLength(1); ++y)
                    yield return array[x, y];
        }

        public static T TryGet<T>(this T[] array, int index0) where T : class
        {
            if ((index0 < 0) || (index0 >= array.GetLength(0)))
                return null;
            return array[index0];
        }

        public static T TryGet<T>(this T[,] array, int index0, int index1) where T : class
        {
            if ((index0 < 0) || (index0 >= array.GetLength(0)))
                return null;
            if ((index1 < 0) || (index1 >= array.GetLength(1)))
                return null;
            return array[index0, index1];
        }

        public static T TryGet<T>(this T[,,] array, int index0, int index1, int index2) where T : class
        {
            if ((index0 < 0) || (index0 >= array.GetLength(0)))
                return null;
            if ((index1 < 0) || (index1 >= array.GetLength(1)))
                return null;
            if ((index2 < 0) || (index2 >= array.GetLength(2)))
                return null;
            return array[index0, index1, index2];
        }

        public static T FindFirstAfterWithWrapAround<T>(this IEnumerable<T> list, Func<T, bool> IsPrevious,
            Func<T, bool> Matches) where T : class
        {
            bool ignoreMatches = true;

            // Search for matching objects after the previous one
            foreach (T obj in list)
            {
                if (ignoreMatches)
                {
                    if (IsPrevious(obj))
                        ignoreMatches = false;
                    continue;
                }

                // Does it match
                if (Matches(obj))
                    return obj;
            }

            // Search for any matching objects
            foreach (T obj in list)
                if (Matches(obj))
                    return obj;

            return null;
        }
    }
}
