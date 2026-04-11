using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Windows.Media;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class LuaCompletionData : ICompletionData
	{
		private readonly LuaCompletionItem _item;

		public LuaCompletionData(LuaCompletionItem item)
			=> _item = item ?? throw new ArgumentNullException(nameof(item));

		public ImageSource Image => null;
		public string Text => _item.Label;
		public object Content => _item.Label;
		public object Description => _item.Description;
		public double Priority => 0;

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
			=> textArea.Document.Replace(completionSegment, _item.InsertText);
	}
}