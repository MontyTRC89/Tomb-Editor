using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TombLib.LevelData;

namespace TombEditor.WPF.ToolWindows;

/// <summary>
/// Interaction logic for Palette.xaml
/// </summary>
public partial class Palette : UserControl
{
	private WriteableBitmap WriteableBitmap { get; set; }

	public Palette()
	{
		InitializeComponent();
		ImageControl.Source = WriteableBitmap;
	}

	private void PaletteGridContainer_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		WriteableBitmap = BitmapFactory.New((int)PaletteGridContainer.ActualWidth, (int)PaletteGridContainer.ActualHeight);
		ImageControl.Source = WriteableBitmap;
		DrawGrid();
	}

	private const int ITEM_WIDTH = 10;
	private const int ITEM_HEIGHT = 10;

	private void DrawGrid()
	{
		int strokeWidth = (int)Scaler.ScaleX;

		int itemWidth = (int)(ITEM_WIDTH * Scaler.ScaleX),
			itemHeight = (int)(ITEM_HEIGHT * Scaler.ScaleY);

		List<TombLib.Utils.ColorC> palette = LevelSettings.LoadPalette();

		using (WriteableBitmap.GetBitmapContext())
		{
			WriteableBitmap.Clear();

			int columnCount = (int)(PaletteGridContainer.ActualWidth / itemWidth);
			int rowCount = (int)(PaletteGridContainer.ActualHeight / itemHeight);

			for (int y = 0; y < rowCount; y++)
			{
				for (int x = 0; x < columnCount; x++)
				{
					int index = (y * columnCount) + x;

					Color fillColor = index < palette.Count
						? Color.FromArgb(palette[index].A, palette[index].R, palette[index].G, palette[index].B)
						: Colors.Transparent;

					WriteableBitmap.FillRectangle(
						x * itemWidth, y * itemHeight,
						(x * itemWidth) + itemWidth, (y * itemHeight) + itemHeight,
						fillColor);

					DrawItemBorder(WriteableBitmap,
						x * itemWidth, y * itemHeight,
						(x * itemWidth) + itemWidth, (y * itemHeight) + itemHeight, strokeWidth,
						Color.FromArgb(192, 0, 0, 0));
				}
			}

			// Draw outside border
			DrawOuterBorder(WriteableBitmap,
				0, 0,
				columnCount * itemWidth + strokeWidth, rowCount * itemHeight + strokeWidth, strokeWidth,
				Colors.Black);

			// Draw selection rectangle
			if (_selectedIndex >= 0 && columnCount > 0)
			{
				int x = _selectedIndex % columnCount,
					y = _selectedIndex / columnCount;

				if (y >= rowCount)
					return;

				x *= itemWidth;
				y *= itemHeight;

				DrawSelectionBorder(WriteableBitmap,
					x, y,
					x + itemWidth, y + itemHeight, strokeWidth,
					Colors.White);
			}
		}
	}

	private int _selectedIndex = -1;

	private void ImageControl_MouseDown(object sender, MouseButtonEventArgs e)
	{
		e.MouseDevice.Capture(ImageControl);

		Point position = e.GetPosition(ImageControl);
		UpdateSelectedIndex(position);
	}

	private void UpdateSelectedIndex(Point position)
	{
		int itemWidth = (int)(ITEM_WIDTH * Scaler.ScaleX),
			itemHeight = (int)(ITEM_HEIGHT * Scaler.ScaleY);

		int x = position.X < 0 ? 0 : (int)(position.X / itemWidth),
			y = position.Y < 0 ? 0 : (int)(position.Y / itemHeight);

		x = Math.Min(x, (int)((ImageControl.ActualWidth - itemWidth) / itemWidth));
		y = Math.Min(y, (int)((ImageControl.ActualHeight - itemHeight) / itemHeight));

		int itemsPerRow = (int)((ImageControl.ActualWidth - 1) / itemWidth);
		_selectedIndex = (y * itemsPerRow) + x;

		DrawGrid();
	}

	private void ImageControl_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			Point position = e.GetPosition(ImageControl);
			UpdateSelectedIndex(position);
		}
	}

	private void ImageControl_MouseUp(object sender, MouseButtonEventArgs e)
		=> e.MouseDevice.Capture(null);

	private static void DrawOuterBorder(WriteableBitmap bitmap, int x1, int y1, int x2, int y2, int strokeWidth, Color color)
	{
		int convertedColor = WriteableBitmapExtensions.ConvertColor(color);

		// Left line
		bitmap.FillRectangle(
			x1, y1 + strokeWidth,
			x1 + strokeWidth, y2 - strokeWidth,
			convertedColor, true);

		// Top line
		bitmap.FillRectangle(
			x1, y1,
			x2, y1 + strokeWidth,
			convertedColor, true);

		// Right line
		bitmap.FillRectangle(
			x2 - strokeWidth, y1 + strokeWidth,
			x2, y2 - strokeWidth,
			convertedColor, true);

		// Bottom line
		bitmap.FillRectangle(
			x1, y2 - strokeWidth,
			x2, y2,
			convertedColor, true);
	}

	private static void DrawItemBorder(WriteableBitmap bitmap, int x1, int y1, int x2, int y2, int strokeWidth, Color color)
	{
		int convertedColor = WriteableBitmapExtensions.ConvertColor(color);

		// Top line
		bitmap.FillRectangle(
			x1, y1,
			x2, y1 + strokeWidth,
			convertedColor, true);

		// Left line
		bitmap.FillRectangle(
			x1, y1 + strokeWidth,
			x1 + strokeWidth, y2,
			convertedColor, true);
	}

	private static void DrawSelectionBorder(WriteableBitmap bitmap, int x1, int y1, int x2, int y2, int strokeWidth, Color color)
	{
		int convertedColor = WriteableBitmapExtensions.ConvertColor(color);

		// Left line
		bitmap.FillRectangle(
			x1, y1 + strokeWidth,
			x1 + strokeWidth, y2,
			convertedColor, true);

		// Top line
		bitmap.FillRectangle(
			x1, y1,
			x2 + strokeWidth, y1 + strokeWidth,
			convertedColor, true);

		// Right line
		bitmap.FillRectangle(
			x2, y1 + strokeWidth,
			x2 + strokeWidth, y2,
			convertedColor, true);

		// Bottom line
		bitmap.FillRectangle(
			x1, y2,
			x2 + strokeWidth, y2 + strokeWidth,
			convertedColor, true);
	}

	private static int AlphaBlendColors(int pixel, int a, int r, int g, int b)
	{
		int pixelA = (pixel >> 24) & 0xFF;
		int pixelR = (pixel >> 16) & 0xFF;
		int pixelG = (pixel >> 8) & 0xFF;
		int pixelB = pixel & 0xFF;

		int blendedA = (a + ((pixelA * (255 - a) * 32897) >> 23)) << 24,
			blendedR = (r + ((pixelR * (255 - a) * 32897) >> 23)) << 16,
			blendedG = (g + ((pixelG * (255 - a) * 32897) >> 23)) << 8,
			blendedB = b + ((pixelB * (255 - a) * 32897) >> 23);

		return blendedA | blendedR | blendedG | blendedB;
	}
}
