using System;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class LuaCompletionItem
	{
		public LuaCompletionItem(string label, string insertText = null, string description = null)
		{
			Label = label ?? throw new ArgumentNullException(nameof(label));
			InsertText = string.IsNullOrWhiteSpace(insertText) ? label : insertText;
			Description = description;
		}

		public string Label { get; }
		public string InsertText { get; }
		public string Description { get; }
	}
}