using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.ClassicScript.Parsers;

namespace TombLib.Scripting.ClassicScript.Objects
{
	public sealed class SectionRenderer : IBackgroundRenderer
	{
		private ClassicScriptEditor _editor;

		#region Construction

		public SectionRenderer(ClassicScriptEditor e)
			=> _editor = e;

		public KnownLayer Layer => KnownLayer.Caret;

		#endregion Construction

		#region Drawing

		public void Draw(TextView textView, DrawingContext drawingContext)
		{
			foreach (DocumentLine line in _editor.Document.Lines)
			{
				string lineText = _editor.Document.GetText(line.Offset, line.Length);

				if (LineParser.IsSectionHeaderLine(lineText))
				{
					var segment = new TextSegment { StartOffset = line.Offset, EndOffset = line.EndOffset };
					var border = new Pen(new SolidColorBrush(Color.FromRgb(192, 192, 192)), 0.5);

					foreach (Rect rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment, true))
						drawingContext.DrawLine(border, new Point(rect.Location.X, rect.Location.Y), new Point(textView.ActualWidth, rect.Location.Y));
				}
			}
		}

		#endregion Drawing
	}
}
