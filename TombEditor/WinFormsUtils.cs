using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vector4 = SharpDX.Vector4;

namespace TombEditor
{
    internal static class WinFormsUtils
    {
        public static bool Contains(this SharpDX.Rectangle This, Point point)
        {
            return This.Contains(point.X, point.Y);
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

        public static Color MixWith(this Color firstColor, Color secondColor, double mixFactor)
        {
            if (mixFactor > 1)
                mixFactor = 1;
            if (!(mixFactor >= 0))
                mixFactor = 0;
            return Color.FromArgb(
                (int)Math.Round(firstColor.A * (1 - mixFactor) + secondColor.A * mixFactor),
                (int)Math.Round(firstColor.R * (1 - mixFactor) + secondColor.R * mixFactor),
                (int)Math.Round(firstColor.G * (1 - mixFactor) + secondColor.G * mixFactor),
                (int)Math.Round(firstColor.B * (1 - mixFactor) + secondColor.B * mixFactor));
        }

        public static void DrawRectangle(this Graphics g, Pen pen, RectangleF rectangle)
        {
            g.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static Color ToWinFormsColor(this Vector4 color)
        {
            return Color.FromArgb(
                    (int)Math.Max(0, Math.Min(255, Math.Round(color.W * 255.0f))),
                    (int)Math.Max(0, Math.Min(255, Math.Round(color.X * 255.0f))),
                    (int)Math.Max(0, Math.Min(255, Math.Round(color.Y * 255.0f))),
                    (int)Math.Max(0, Math.Min(255, Math.Round(color.Z * 255.0f))));
        }

        public static Vector4 ToFloatColor(this Color color)
        {
            return new Vector4(color.R, color.G, color.B, color.A) / 255.0f;
        }

        public static Rectangle ToRectangle(this RectangleF area, float margin, Rectangle maxArea)
        {
            double minX = Math.Floor(Math.Min(area.Left, area.Right) - margin);
            double minY = Math.Floor(Math.Min(area.Top, area.Bottom) - margin);
            double maxX = Math.Ceiling(Math.Max(area.Left, area.Right) + margin);
            double maxY = Math.Ceiling(Math.Max(area.Top, area.Bottom) + margin);
            minX = Math.Max(maxArea.X, Math.Min(maxArea.Right, minX));
            minY = Math.Max(maxArea.Y, Math.Min(maxArea.Bottom, minY));
            maxX = Math.Max(minX, Math.Min(maxArea.Right, maxX));
            maxY = Math.Max(minY, Math.Min(maxArea.Bottom, maxY));
            return Rectangle.FromLTRB((int)minX, (int)minY, (int)maxX, (int)maxY);
        }

        public static Rectangle ToRectangle(this RectangleF area, float margin)
        {
            return area.ToRectangle(margin, Rectangle.FromLTRB(short.MinValue, short.MinValue, short.MaxValue, short.MaxValue));
        }

        public static void InvalidateC(this Control control, RectangleF area, float margin)
        {
            control.Invalidate(area.ToRectangle(margin, control.ClientRectangle));
        }

        public static void InvalidateC(this Control control, RectangleF area, float margin, bool invalidateChildren)
        {
            control.Invalidate(area.ToRectangle(margin, control.ClientRectangle), invalidateChildren);
        }

        public static IEnumerable<T> BoolCombine<T>(IEnumerable<T> oldObjects, IEnumerable<T> newObjects, Keys modifierKeys)
        {
            bool control = modifierKeys.HasFlag(Keys.Control);
            bool shift = modifierKeys.HasFlag(Keys.Shift);

            if (control && shift)
                return oldObjects.Except(newObjects).ToList(); // Difference
            else if (control)
                return oldObjects.Union(newObjects).Except(oldObjects.Intersect(newObjects)).ToList(); // Either-or
            else if (shift)
                return oldObjects.Union(newObjects); // Union
            else
                return newObjects;
        }
    }
}
