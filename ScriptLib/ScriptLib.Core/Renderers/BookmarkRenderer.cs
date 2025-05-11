using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;

namespace ScriptLib.Core.Renderers;

public sealed class BookmarkRenderer : IBackgroundRenderer
{
	public KnownLayer Layer => KnownLayer.Background;

	private readonly TextEditorBase _editor;

	public BookmarkRenderer(TextEditorBase editor)
		=> _editor = editor;

	public void Draw(TextView textView, DrawingContext drawingContext)
	{
		foreach (int bookmarkIndex in _editor.Bookmarks)
		{
			DocumentLine? line = _editor.TryGetLineByNumber(bookmarkIndex);

			if (line is null)
				continue;

			var segment = new TextSegment { StartOffset = line.Offset, EndOffset = line.EndOffset };
			var background = new SolidColorBrush(Color.FromArgb(40, 128, 128, 255)); // Light blue
			var border = new Pen(Brushes.Transparent, 0);

			foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment, true))
				drawingContext.DrawRectangle(background, border, new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height)));
		}
	}
}
