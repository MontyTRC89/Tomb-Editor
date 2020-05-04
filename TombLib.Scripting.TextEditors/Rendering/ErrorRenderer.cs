using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.TextEditors.Controls;

namespace TombLib.Scripting.TextEditors.Rendering
{
	public sealed class ErrorRenderer : IBackgroundRenderer
	{
		private TextEditorBase _editor;

		public ErrorRenderer(TextEditorBase e) => _editor = e;
		public KnownLayer Layer { get { return KnownLayer.Caret; } }

		public void Draw(TextView textView, DrawingContext drawingContext)
		{
			foreach (DocumentLine line in _editor.Document.Lines)
			{
				if (string.IsNullOrEmpty(line.ErrorSegmentText))
					continue;

				string lineText = _editor.Document.GetText(line.Offset, line.Length);

				int matchIndex = lineText.IndexOf(line.ErrorSegmentText);

				if (matchIndex == -1)
					continue;

				TextSegment segment = new TextSegment
				{
					StartOffset = line.Offset + matchIndex,
					Length = line.ErrorSegmentText.Length
				};

				foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
				{
					ImageSource underlining = TextRendering.CreateZigZagUnderlining((int)rect.Width, System.Drawing.Color.FromArgb(192, 255, 0, 0));

					if (underlining == null)
						continue;

					drawingContext.DrawImage(underlining,
						new Rect(new Point(rect.Location.X, rect.Location.Y + rect.Height - 2), new Size(rect.Width, 4)));
				}
			}
		}
	}
}
