using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using TombIDE.ScriptEditor.Controls;

namespace TombIDE.ScriptEditor.Rendering
{
	public class BookmarkRenderer : IBackgroundRenderer
	{
		private AvalonTextBox _editor;

		public BookmarkRenderer(AvalonTextBox e)
		{
			_editor = e;
		}

		public KnownLayer Layer
		{
			get { return KnownLayer.Caret; }
		}

		public void Draw(TextView textView, DrawingContext drawingContext)
		{
			textView.EnsureVisualLines();

			List<int> lineNumbers = new List<int>(_editor.BookmarkedLines);

			foreach (int lineNumber in lineNumbers)
			{
				if (lineNumber > _editor.LineCount)
				{
					_editor.BookmarkedLines.Remove(lineNumber);
					continue;
				}

				DocumentLine line = _editor.Document.GetLineByNumber(lineNumber);
				TextSegment segment = new TextSegment { StartOffset = line.Offset, EndOffset = line.EndOffset };
				SolidColorBrush background = new SolidColorBrush(Color.FromArgb(40, 128, 128, 255));
				Pen border = new Pen(new SolidColorBrush(Color.FromArgb(40, 128, 128, 255)), 1);

				foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
					drawingContext.DrawRoundedRectangle(background, border, new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height)), 3, 3);
			}
		}
	}
}
