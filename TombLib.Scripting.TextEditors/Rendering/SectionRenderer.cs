using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.Helpers;
using TombLib.Scripting.TextEditors.Controls;

namespace TombLib.Scripting.TextEditors.Rendering
{
	public sealed class SectionRenderer : IBackgroundRenderer
	{
		private ScriptTextEditor _editor;

		#region Construction

		public SectionRenderer(ScriptTextEditor e)
		{
			_editor = e;
		}

		public KnownLayer Layer { get { return KnownLayer.Caret; } }

		#endregion Construction

		#region Drawing

		public void Draw(TextView textView, DrawingContext drawingContext)
		{
			foreach (DocumentLine line in _editor.Document.Lines)
			{
				string lineText = _editor.Document.GetText(line.Offset, line.Length);

				if (LineHelper.IsSectionHeaderLine(lineText))
				{
					TextSegment segment = new TextSegment { StartOffset = line.Offset, EndOffset = line.EndOffset };
					Pen border = new Pen(new SolidColorBrush(Color.FromRgb(220, 220, 220)), 0.5);

					foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment, true))
						drawingContext.DrawLine(border, new Point(rect.Location.X, rect.Location.Y), new Point(textView.ActualWidth, rect.Location.Y));
				}
			}
		}

		#endregion Drawing
	}
}
