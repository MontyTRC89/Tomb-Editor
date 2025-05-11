using System.Windows;
using System.Windows.Media;

namespace ScriptLib.Core.Renderers;

public static class TextRendering
{
	/// <summary>
	/// Creates a squiggly underline with customizable properties.
	/// </summary>
	/// <param name="textWidth">Width of the text to underline.</param>
	/// <param name="color">Color of the squiggly line.</param>
	/// <param name="amplitude">Height of the squiggly wave (default: 3).</param>
	/// <param name="wavelength">Length of each wave segment (default: 3).</param>
	/// <param name="thickness">Thickness of the line (default: 2).</param>
	/// <returns>An ImageSource representing the squiggly line or null if width is too small.</returns>
	public static ImageSource? CreateSquigglyUnderlining(int textWidth, Color color, int amplitude = 3, int wavelength = 3, double thickness = 2)
	{
		if (textWidth < wavelength)
			return null;

		// Ensure wavelength is at least 2 to avoid zero-division issues
		wavelength = Math.Max(2, wavelength);

		var geometry = new StreamGeometry();

		using (StreamGeometryContext context = geometry.Open())
		{
			context.BeginFigure(new Point(0, amplitude), false, false);

			// Calculate appropriate step size
			int steps = Math.Min(textWidth, 500); // Cap maximum number of steps for very large widths
			double stepSize = (double)textWidth / steps;

			// Pre-calculate full wave cycle length for smoother pattern
			double waveCycle = wavelength * 2;

			for (double i = 0; i <= textWidth; i += stepSize)
			{
				// Create a smoother zigzag pattern based on position in the wave cycle
				double phase = i % waveCycle / waveCycle;
				double y = amplitude * Math.Sin(phase * Math.PI * 2);

				context.LineTo(new Point(i, y + (amplitude / 2)), true, false);
			}
		}

		var drawing = new GeometryDrawing
		{
			Geometry = geometry,
			Pen = new Pen(new SolidColorBrush(color), thickness)
		};

		var drawingImage = new DrawingImage(drawing);
		drawingImage.Freeze(); // Makes it immutable and more efficient

		return drawingImage;
	}
}
