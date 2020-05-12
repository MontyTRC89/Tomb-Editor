﻿using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.TextEditors.Controls;

namespace TombLib.Scripting.TextEditors.Rendering
{
	public sealed class BookmarkRenderer : IBackgroundRenderer
	{
		private TextEditorBase _editor;

		#region Construction

		public BookmarkRenderer(TextEditorBase e)
		{
			_editor = e;
		}

		public KnownLayer Layer { get { return KnownLayer.Background; } }

		#endregion Construction

		#region Drawing

		public void Draw(TextView textView, DrawingContext drawingContext)
		{
			foreach (DocumentLine line in _editor.Document.Lines)
				if (line.IsBookmarked)
				{
					TextSegment segment = new TextSegment { StartOffset = line.Offset, EndOffset = line.EndOffset };
					SolidColorBrush background = new SolidColorBrush(Color.FromArgb(40, 128, 128, 255)); // Light Blue
					Pen border = new Pen(Brushes.Transparent, 0);

					foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment, true))
						drawingContext.DrawRectangle(background, border, new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height)));
				}
		}

		#endregion Drawing
	}
}
