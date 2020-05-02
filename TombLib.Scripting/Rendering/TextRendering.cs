using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TombLib.Scripting.Rendering
{
	internal class TextRendering
	{
		public static System.Windows.Media.ImageSource CreateZigZagUnderlining(int textWidth, Color color)
		{
			if (textWidth < 3)
				return null;

			List<PointF> zigZagPoints = GenerateZigZagPatternPoints(textWidth);

			Bitmap bitmap = new Bitmap(textWidth, 4);
			DrawPatternLines(ref bitmap, zigZagPoints, color);

			IntPtr handle = bitmap.GetHbitmap();

			try { return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); }
			finally { NativeMethods.DeleteObject(handle); }
		}

		private static List<PointF> GenerateZigZagPatternPoints(int textWidth)
		{
			List<PointF> zigZagPoints = new List<PointF>();

			bool toggle = false;

			for (int i = 0; i <= textWidth; i += 3)
			{
				float x = i;
				float y = toggle ? 0 : 3;

				zigZagPoints.Add(new PointF(x, y));
				toggle = !toggle;
			}

			return zigZagPoints;
		}

		private static void DrawPatternLines(ref Bitmap bitmap, List<PointF> points, Color color)
		{
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.SmoothingMode = SmoothingMode.HighSpeed;
				graphics.DrawLines(new Pen(color, 2), points.ToArray());
			}
		}
	}
}
