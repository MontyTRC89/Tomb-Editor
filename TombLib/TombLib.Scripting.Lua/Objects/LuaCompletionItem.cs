using System;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class LuaCompletionItem
	{
		public LuaCompletionItem(
			string label,
			string insertText = null,
			string detail = null,
			string description = null,
			string filterText = null,
			double priority = 0.0,
			LuaCompletionItemKind kind = LuaCompletionItemKind.Text,
			LuaCompletionIconKind iconKind = LuaCompletionIconKind.Misc,
			bool isDescriptionMarkdown = false)
		{
			Label = label ?? throw new ArgumentNullException(nameof(label));
			InsertText = string.IsNullOrWhiteSpace(insertText) ? label : insertText;
			Detail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
			Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
			FilterText = string.IsNullOrWhiteSpace(filterText) ? label : filterText;
			Priority = priority;
			Kind = kind;
			IconKind = iconKind;
			IsDescriptionMarkdown = isDescriptionMarkdown;
		}

		public string Label { get; }
		public string InsertText { get; }
		public string Detail { get; }
		public string Description { get; }
		public string FilterText { get; }
		public double Priority { get; }
		public LuaCompletionItemKind Kind { get; }
		public LuaCompletionIconKind IconKind { get; }
		public bool IsDescriptionMarkdown { get; }
	}
}