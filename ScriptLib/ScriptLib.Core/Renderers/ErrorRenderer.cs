using ICSharpCode.AvalonEdit.Rendering;
using ScriptLib.Core.Structs;
using System.Windows;
using System.Windows.Media;

namespace ScriptLib.Core.Renderers;

public sealed class ErrorRenderer : IBackgroundRenderer
{
	public KnownLayer Layer => KnownLayer.Caret;

	private TextEditorBase _editor;

	public ErrorRenderer(TextEditorBase editor)
		=> _editor = editor;

	public void Draw(TextView textView, DrawingContext drawingContext)
	{
		foreach (DocumentError error in _editor.Errors)
		{
			foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, error.Segment))
			{
				// Create a wavy line at the bottom of the text
				var errorPen = new Pen(Brushes.Red, 1)
				{
					DashStyle = DashStyles.Dash
				};

				// Calculate bottom line Y position and start/end X positions
				double y = rect.Bottom - 1;
				double xStart = rect.Left;
				double xEnd = rect.Right;

				// Create points for wavy line
				const double wavyLineHeight = 2.5;
				const double wavyLineWidth = 4.0;

				var points = new PointCollection();

				for (double x = xStart; x <= xEnd; x += wavyLineWidth / 2)
				{
					// Alternate up and down to create wave effect
					double waveOffset = (x - xStart) / wavyLineWidth % 1.0;
					double waveY = waveOffset < 0.5 ? y : y + wavyLineHeight;
					points.Add(new Point(x, waveY));
				}

				// Draw wavy line
				var geometry = new StreamGeometry();

				using (StreamGeometryContext ctx = geometry.Open())
				{
					ctx.BeginFigure(points[0], false, false);
					ctx.PolyLineTo([.. points.Skip(1)], true, false);
				}

				geometry.Freeze();
				drawingContext.DrawGeometry(null, errorPen, geometry);
			}
		}
	}
}
