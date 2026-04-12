using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.Bases;
using static TombLib.WPF.BrushHelpers;

namespace TombLib.Scripting.Rendering
{
	public sealed class BookmarkRenderer : IBackgroundRenderer
	{
		private static readonly Brush BackgroundBrush = CreateFrozenBrush(Color.FromArgb(40, 128, 128, 255));

		private TextEditorBase _editor;

		#region Construction

		public BookmarkRenderer(TextEditorBase e)
			=> _editor = e;

		public KnownLayer Layer => KnownLayer.Background;

		#endregion Construction

		#region Drawing

		public void Draw(TextView textView, DrawingContext drawingContext)
		{
			foreach (DocumentLine line in _editor.GetBookmarkedLines())
			{
				var segment = new TextSegment { StartOffset = line.Offset, EndOffset = line.EndOffset };

				foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment, true))
					drawingContext.DrawRectangle(BackgroundBrush, null, new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height)));
			}
		}

		#endregion Drawing
	}
}
