using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace TombIDE.Shared
{
	public class ImageHandling
	{
		public static Image ResizeImage(Image image, int width, int height)
		{
			Rectangle destRect = new Rectangle(0, 0, width, height);
			Bitmap destImage = new Bitmap(width, height);

			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (Graphics graphics = Graphics.FromImage(destImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using (ImageAttributes attributes = new ImageAttributes())
				{
					attributes.SetWrapMode(WrapMode.TileFlipXY);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
				}
			}

			return destImage;
		}

		public static Image ResizeKeepAspect(Image sourceImage, int maxWidth, int maxHeight, bool enlarge = false)
		{
			Size imgSize = sourceImage.Size;

			maxWidth = enlarge ? maxWidth : Math.Min(maxWidth, imgSize.Width);
			maxHeight = enlarge ? maxHeight : Math.Min(maxHeight, imgSize.Height);

			decimal rnd = Math.Min(maxWidth / (decimal)imgSize.Width, maxHeight / (decimal)imgSize.Height);
			Size scaledSize = new Size((int)Math.Round(imgSize.Width * rnd), (int)Math.Round(imgSize.Height * rnd));

			return ResizeImage(sourceImage, scaledSize.Width, scaledSize.Height);
		}

		public static byte[] GetRawDataFromBitmap(Bitmap bitmap)
		{
			List<byte> bytes = new List<byte>();

			for (int i = 0; i < bitmap.Height; i++)
			{
				for (int j = 0; j < bitmap.Width; j++)
				{
					Color color = bitmap.GetPixel(j, i);

					bytes.Add(color.R);
					bytes.Add(color.G);
					bytes.Add(color.B);
				}
			}

			return bytes.ToArray();
		}

		public static byte[] GetRawDataFromImage(Image image)
		{
			Bitmap bitmap = (Bitmap)image;

			List<byte> bytes = new List<byte>();

			for (int i = 0; i < bitmap.Height; i++)
			{
				for (int j = 0; j < bitmap.Width; j++)
				{
					Color color = bitmap.GetPixel(j, i);

					if (color.A > 0 && color.R == 0 && color.G == 0 && color.B == 0)
						color = Color.FromArgb(1, 1, 1);
					else if (color.A == 0)
						color = Color.FromArgb(0, 0, 0);

					bytes.Add(color.R);
					bytes.Add(color.G);
					bytes.Add(color.B);
				}
			}

			return bytes.ToArray();
		}

		public static Image GetImageFromRawData(byte[] data, int width, int height)
		{
			Bitmap bitmap = new Bitmap(width, height);

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					int red = data[((i * width) + j) * 3];
					int green = data[((i * width) + j) * 3 + 1];
					int blue = data[((i * width) + j) * 3 + 2];

					Color color = new Color();

					if (red == 0 && green == 0 && blue == 0) // Black (#000000) = Transparent
						color = Color.FromArgb(0);
					else
						color = Color.FromArgb(red, green, blue);

					bitmap.SetPixel(j, i, color);
				}
			}

			return bitmap;
		}

		public static byte[] GetBitmapStream(Bitmap bitmap)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				bitmap.Save(stream, ImageFormat.Bmp);
				return stream.ToArray();
			}
		}

		public static Bitmap CropBitmapWhitespace(Bitmap bitmap)
		{
			// Find the min/max non-white/transparent pixels
			Point min = new Point(int.MaxValue, int.MaxValue);
			Point max = new Point(int.MinValue, int.MinValue);

			for (int x = 0; x < bitmap.Width; ++x)
			{
				for (int y = 0; y < bitmap.Height; ++y)
				{
					Color pixelColor = bitmap.GetPixel(x, y);

					if (pixelColor.A > 0)
					{
						if (x < min.X)
							min.X = x;

						if (y < min.Y)
							min.Y = y;

						if (x > max.X)
							max.X = x;

						if (y > max.Y)
							max.Y = y;
					}
				}
			}

			// Create a new bitmap from the crop rectangle
			Rectangle cropRectangle = new Rectangle(min.X, min.Y, max.X - min.X + 1, max.Y - min.Y + 1);
			Bitmap newBitmap = new Bitmap(cropRectangle.Width, cropRectangle.Height);

			using (Graphics graphics = Graphics.FromImage(newBitmap))
				graphics.DrawImage(bitmap, 0, 0, cropRectangle, GraphicsUnit.Pixel);

			return newBitmap;
		}
	}
}
