using System.Windows.Media;

namespace TombLib.WPF;

public static class BrushHelpers
{
	public static SolidColorBrush CreateFrozenBrush(Color color)
	{
		var brush = new SolidColorBrush(color);
		brush.Freeze();
		return brush;
	}

	public static SolidColorBrush CreateFrozenBrush(string colorValue)
	{
		var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorValue));
		brush.Freeze();
		return brush;
	}

	public static Pen CreateFrozenPen(Brush brush, double thickness)
	{
		var pen = new Pen(brush, thickness);
		pen.Freeze();
		return pen;
	}

	public static Pen CreateFrozenPen(Color color, double thickness)
	{
		return CreateFrozenPen(CreateFrozenBrush(color), thickness);
	}
}