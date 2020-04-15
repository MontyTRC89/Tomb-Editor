using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using TombIDE.ScriptEditor.Controls;
using TombIDE.ScriptEditor.Objects;

namespace TombIDE.ScriptEditor.Rendering
{
	public class ErrorRenderer : IBackgroundRenderer
	{
		private AvalonTextBox _editor;

		public ErrorRenderer(AvalonTextBox e)
		{
			_editor = e;
		}

		public KnownLayer Layer
		{
			get { return KnownLayer.Caret; }
		}

		public void Draw(TextView textView, DrawingContext drawingContext)
		{
			List<ErrorLine> errorLines = new List<ErrorLine>(_editor.ErrorLines);

			foreach (ErrorLine errorLine in errorLines)
			{
				if (errorLine.LineNumber > _editor.LineCount)
				{
					_editor.ErrorLines.Remove(errorLine);
					continue;
				}

				DocumentLine line = _editor.Document.GetLineByNumber(errorLine.LineNumber);
				TextSegment segment = new TextSegment { StartOffset = line.Offset, EndOffset = line.EndOffset };
				SolidColorBrush background = new SolidColorBrush(Color.FromArgb(40, 255, 128, 128));
				Pen border = new Pen(new SolidColorBrush(Color.FromArgb(40, 255, 128, 128)), 1);

				foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
					drawingContext.DrawRoundedRectangle(background, border, new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height)), 3, 3);
			}
		}
	}
}
