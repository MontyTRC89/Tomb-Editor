using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

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
	}
}
