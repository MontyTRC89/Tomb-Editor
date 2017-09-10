using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using System.IO;
using NLog;
using System.Drawing;
using TombEditor.Geometry;

namespace TombEditor
{
    public static class Utils
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        public static string GetDirectoryNameTry(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            try
            {
                return Path.GetDirectoryName(path);
            }
            catch
            {
                return path;
            }
        }

        public static string GetRelativePath(string baseDir, string fileName)
        {
            if (string.IsNullOrEmpty(baseDir))
                return Path.GetFullPath(fileName);

            // https://stackoverflow.com/questions/9042861/how-to-make-an-absolute-path-relative-to-a-particular-folder
            //
            // Roughly based on (slightly improved) 
            //   https://sourceforge.net/p/syncproj/code/HEAD/tree/syncProj.cs#l976
            //   makeRelative
            baseDir = Path.GetFullPath(baseDir);
            fileName = Path.GetFullPath(Path.Combine(baseDir, fileName));

            var dictionarySeperators = new string[] { Path.DirectorySeparatorChar.ToString(), Path.AltDirectorySeparatorChar.ToString() };
            string[] baseDirArr = baseDir.Split(dictionarySeperators, StringSplitOptions.RemoveEmptyEntries);
            string[] fileNameArr = fileName.Split(dictionarySeperators, StringSplitOptions.RemoveEmptyEntries);

            int i = 0;
            for (; i < baseDirArr.Length && i < fileNameArr.Length; i++)
                if (string.Compare(baseDirArr[i], fileNameArr[i], true) != 0) // Case insensitive match
                    break;
            if (i == 0) // Cannot make relative path, for example if resides on different drive
                return null;

            var resultFolders = Enumerable.Repeat("..", Math.Max(0, baseDirArr.Length - i)).Concat(fileNameArr.Skip(i));
            string result = string.Join(Path.DirectorySeparatorChar.ToString(), resultFolders);
            return result;
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

        public static SharpDX.Rectangle Intersect(this SharpDX.Rectangle area, SharpDX.Rectangle other)
        {
            return new SharpDX.Rectangle(
                Math.Max(area.Left, other.Left),
                Math.Max(area.Top, other.Top),
                Math.Min(area.Right, other.Right),
                Math.Min(area.Bottom, other.Bottom));
        }

        public static SharpDX.Rectangle Union(this SharpDX.Rectangle area, SharpDX.Rectangle other)
        {
            return new SharpDX.Rectangle(
                Math.Min(area.Left, other.Left),
                Math.Min(area.Top, other.Top),
                Math.Max(area.Right, other.Right),
                Math.Max(area.Bottom, other.Bottom));
        }

        public static bool Contains(this SharpDX.Rectangle area, SharpDX.Rectangle other)
        {
            return ((area.X <= other.X) && (area.Right >= other.Right)) &&
                ((area.Y <= other.Y) && (area.Bottom >= other.Bottom));
        }

        public static bool Intersects(this SharpDX.Rectangle area, SharpDX.Rectangle other)
        {
            return (area.X <= other.Right) && (area.Right >= other.X) &&
                (area.Y <= other.Bottom) && (area.Bottom >= other.Y);
        }

        public static DrawingPoint Offset(this DrawingPoint basePoint, DrawingPoint point)
        {
            return new DrawingPoint(basePoint.X + point.X, basePoint.Y + point.Y);
        }

        public static SharpDX.Rectangle Offset(this SharpDX.Rectangle area, DrawingPoint point)
        {
            return new SharpDX.Rectangle(area.Left + point.X, area.Top + point.Y, area.Right + point.X, area.Bottom + point.Y);
        }

        public static DrawingPoint OffsetNeg(this DrawingPoint basePoint, DrawingPoint point)
        {
            return new DrawingPoint(basePoint.X - point.X, basePoint.Y - point.Y);
        }

        public static SharpDX.Rectangle OffsetNeg(this SharpDX.Rectangle area, DrawingPoint point)
        {
            return new SharpDX.Rectangle(area.Left - point.X, area.Top - point.Y, area.Right - point.X, area.Bottom - point.Y);
        }

        public static Vector2 ToVec2(this DrawingPoint basePoint)
        {
            return new Vector2(basePoint.X, basePoint.Y);
        }

        public static SharpDX.Rectangle Inflate(this SharpDX.Rectangle area, int width)
        {
            return new SharpDX.Rectangle(area.Left - width, area.Top - width, area.Right + width, area.Bottom + width);
        }

        public static SharpDX.Rectangle Inflate(this SharpDX.Rectangle area, int x, int y)
        {
            return new SharpDX.Rectangle(area.Left - x, area.Top - y, area.Right + x, area.Bottom + y);
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

        public static T FindFirstAfterWithWrapAround<T>(this IEnumerable<T> list, Func<T, bool> IsPrevious, Func<T, bool> Matches) where T : class
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

        public static System.Drawing.Color MixWith(this System.Drawing.Color firstColor, System.Drawing.Color secondColor, double mixFactor)
        {
            if (mixFactor > 1)
                mixFactor = 1;
            if (!(mixFactor >= 0))
                mixFactor = 0;
            return System.Drawing.Color.FromArgb(
                (int)Math.Round(firstColor.A * (1 - mixFactor) + secondColor.A * mixFactor),
                (int)Math.Round(firstColor.R * (1 - mixFactor) + secondColor.R * mixFactor),
                (int)Math.Round(firstColor.G * (1 - mixFactor) + secondColor.G * mixFactor),
                (int)Math.Round(firstColor.B * (1 - mixFactor) + secondColor.B * mixFactor));
        }

        public static void Swap<T>(ref T first, ref T second)
        {
            T temp = first;
            first = second;
            second = temp;
        }

        public static System.Drawing.Color ToWinFormsColor(this Vector4 color)
        {
            return System.Drawing.Color.FromArgb(
                    (int)Math.Max(0, Math.Min(255, Math.Round(color.W * 128.0f))),
                    (int)Math.Max(0, Math.Min(255, Math.Round(color.X * 128.0f))),
                    (int)Math.Max(0, Math.Min(255, Math.Round(color.Y * 128.0f))),
                    (int)Math.Max(0, Math.Min(255, Math.Round(color.Z * 128.0f))));
        }

        public static Vector4 ToFloatColor(this System.Drawing.Color color)
        {
            return new Vector4(color.R, color.G, color.B, color.A) / 128.0f;
        }

        public static Vector3 ToFloatColor3(this System.Drawing.Color color)
        {
            return new Vector3(color.R, color.G, color.B) / 128.0f;
        }

        public static string TryFindAbsolutePath(LevelSettings levelSettings, string filename)
        {
            try
            {
                // Is the file easily found?
                if (File.Exists(filename))
                    return filename;

                string[] filePathComponents = filename.Split(new char[] { '\\', '/' });
                string[] levelPathComponents = levelSettings.GetVariable(VariableType.LevelDirectory).Split(new char[] { '\\', '/' });

                // Try to go up 2 directories to find file (works in original levels)
                // If it turns out that many people have directory structures incompatible to this assumptions
                // we can add more suffisticated options here in the future.
                int filePathCheckDepth = Math.Min(3, filePathComponents.GetLength(0) - 1);
                int levelPathCheckDepth = Math.Min(2, levelPathComponents.GetLength(0) - 1);
                for (int levelPathUntil = 0; levelPathUntil <= levelPathCheckDepth; ++levelPathUntil)
                    for (int filePathAfter = 1; filePathAfter <= filePathCheckDepth; ++filePathAfter)
                    {
                        var basePath = levelPathComponents.Take(levelPathComponents.GetLength(0) - levelPathUntil);
                        var filePath = filePathComponents.Skip(filePathComponents.GetLength(0) - filePathAfter);
                        string filepathSuggestion = string.Join(LevelSettings.Dir.ToString(), basePath.Union(filePath));
                        if (File.Exists(filepathSuggestion))
                            return filepathSuggestion;
                    }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "TryFindAbsolutePath failed");
                // In cas of an error we can just give up to find the absolute path alreasy
                // and prompt the user for the file path.
            }
            return filename;
        }
    }
}
