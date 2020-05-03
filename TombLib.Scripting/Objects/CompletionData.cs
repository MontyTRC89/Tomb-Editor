using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Windows.Media;

namespace TombLib.Scripting.Objects
{
	/// <summary>
	/// Implements AvalonEdit ICompletionData interface to provide the entries in the completion drop down.
	/// </summary>
	public sealed class CompletionData : ICompletionData
	{
		public CompletionData(string text)
		{
			Text = text;
		}

		public ImageSource Image { get { return null; } }

		public string Text { get; private set; }
		public object Content { get { return Text; } }
		public object Description { get { return null; } }
		public double Priority { get { return 0; } }

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs) =>
			textArea.Document.Replace(completionSegment, Text);
	}
}
