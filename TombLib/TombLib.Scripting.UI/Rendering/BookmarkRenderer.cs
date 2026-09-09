using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.UI.Documents;
using static TombLib.WPF.BrushHelpers;

namespace TombLib.Scripting.UI.Rendering;

internal sealed class BookmarkRenderer : IBackgroundRenderer
{
	private static readonly Brush BackgroundBrush = CreateFrozenBrush(Color.FromArgb(40, 128, 128, 255));

	private readonly BookmarkCoordinator _bookmarkCoordinator;

	// Construction

	public BookmarkRenderer(BookmarkCoordinator bookmarkCoordinator) => _bookmarkCoordinator = bookmarkCoordinator;

	public KnownLayer Layer => KnownLayer.Background;

	// Drawing

	public void Draw(TextView textView, DrawingContext drawingContext)
	{
		foreach (DocumentLine line in _bookmarkCoordinator.GetBookmarkedLines())
		{
			var segment = new TextSegment { StartOffset = line.Offset, EndOffset = line.EndOffset };

			foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment, true))
				drawingContext.DrawRectangle(BackgroundBrush, null, new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height)));
		}
	}
}
