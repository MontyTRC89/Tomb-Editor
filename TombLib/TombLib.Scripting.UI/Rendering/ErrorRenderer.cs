using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.Diagnostics;
using static TombLib.WPF.BrushHelpers;

namespace TombLib.Scripting.UI.Rendering
{
	public sealed class ErrorRenderer : IBackgroundRenderer
	{
		private static readonly Brush ErrorBrush = CreateFrozenBrush(Color.FromArgb(224, 220, 76, 60));
		private static readonly Brush WarningBrush = CreateFrozenBrush(Color.FromArgb(224, 226, 165, 44));
		private static readonly Brush InformationBrush = CreateFrozenBrush(Color.FromArgb(224, 88, 170, 255));
		private static readonly Brush HintBrush = CreateFrozenBrush(Color.FromArgb(192, 166, 166, 166));
		private static readonly Pen ErrorPen = CreateFrozenPen(ErrorBrush, 1.4);
		private static readonly Pen WarningPen = CreatePen(WarningBrush, new double[] { 1.0, 2.0 });
		private static readonly Pen InformationPen = CreatePen(InformationBrush, new double[] { 2.0, 2.0 });
		private static readonly Pen HintPen = CreatePen(HintBrush, new double[] { 1.0, 3.0 });

		private TextEditorBase _editor;

		#region Construction

		public ErrorRenderer(TextEditorBase e)
			=> _editor = e;

		public KnownLayer Layer => KnownLayer.Caret;

		#endregion Construction

		#region Drawing

		public void Draw(TextView textView, DrawingContext drawingContext)
		{
			if (!_editor.LiveErrorUnderlining || _editor.Diagnostics.Count == 0)
				return;

			foreach (TextEditorDiagnostic diagnostic in _editor.Diagnostics)
			{
				if (!TryCreateSegment(textView.Document, diagnostic, out TextSegment segment))
					continue;

				foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment, false))
				{
					if (rect.Width < 2.0)
						continue;

					switch (diagnostic.Severity)
					{
						case TextEditorDiagnosticSeverity.Warning:
							DrawStraightUnderline(drawingContext, rect, WarningPen);
							break;

						case TextEditorDiagnosticSeverity.Information:
							DrawStraightUnderline(drawingContext, rect, InformationPen);
							break;

						case TextEditorDiagnosticSeverity.Hint:
							DrawStraightUnderline(drawingContext, rect, HintPen);
							break;

						default:
							DrawErrorUnderline(drawingContext, rect);
							break;
					}
				}
			}
		}

		private static bool TryCreateSegment(TextDocument document, TextEditorDiagnostic diagnostic, out TextSegment segment)
		{
			segment = null;

			if (document is null || diagnostic is null || document.TextLength == 0)
				return false;

			int startOffset = Math.Max(0, Math.Min(diagnostic.StartOffset, document.TextLength - 1));
			int endOffset = Math.Max(startOffset + 1, Math.Min(diagnostic.EndOffset, document.TextLength));

			if (endOffset <= startOffset)
				return false;

			segment = new TextSegment
			{
				StartOffset = startOffset,
				EndOffset = endOffset
			};

			return true;
		}

		private static void DrawErrorUnderline(DrawingContext drawingContext, Rect rect)
		{
			double baseline = rect.Bottom - 1.0;
			double amplitude = 1.6;
			double step = 4.0;

			var geometry = new StreamGeometry();

			using (StreamGeometryContext context = geometry.Open())
			{
				bool goingUp = true;
				context.BeginFigure(new Point(rect.Left, baseline), false, false);

				for (double x = rect.Left; x < rect.Right; x += step)
				{
					double nextX = Math.Min(x + step / 2.0, rect.Right);
					double y = baseline + (goingUp ? -amplitude : amplitude);
					context.LineTo(new Point(nextX, y), true, false);
					goingUp = !goingUp;

					nextX = Math.Min(x + step, rect.Right);
					context.LineTo(new Point(nextX, baseline), true, false);
				}
			}

			geometry.Freeze();
			drawingContext.DrawGeometry(null, ErrorPen, geometry);
		}

		private static void DrawStraightUnderline(DrawingContext drawingContext, Rect rect, Pen pen)
		{
			double y = rect.Bottom - 1.0;
			drawingContext.DrawLine(pen, new Point(rect.Left, y), new Point(rect.Right, y));
		}

		private static Pen CreatePen(Brush brush, double[] dashPattern)
		{
			var pen = new Pen(brush, 1.5)
			{
				DashStyle = new DashStyle(dashPattern, 0.0),
				StartLineCap = PenLineCap.Round,
				EndLineCap = PenLineCap.Round
			};

			pen.Freeze();
			return pen;
		}

		#endregion Drawing
	}
}
