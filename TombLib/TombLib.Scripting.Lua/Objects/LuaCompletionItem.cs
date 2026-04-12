using System;
using System.Threading;
using System.Threading.Tasks;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class LuaCompletionItem
	{
		private readonly Func<CancellationToken, Task<LuaCompletionItem>>? _resolveAsync;

		public LuaCompletionItem(
			string label,
			string? insertText = null,
			string? detail = null,
			string? description = null,
			string? filterText = null,
			double priority = 0.0,
			LuaCompletionIconKind iconKind = LuaCompletionIconKind.Misc,
			bool isDescriptionMarkdown = false,
			Func<CancellationToken, Task<LuaCompletionItem>>? resolveAsync = null)
		{
			Label = label ?? throw new ArgumentNullException(nameof(label));
			InsertText = string.IsNullOrWhiteSpace(insertText) ? label : insertText;
			Detail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
			Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
			FilterText = string.IsNullOrWhiteSpace(filterText) ? label : filterText;
			Priority = priority;
			IconKind = iconKind;
			IsDescriptionMarkdown = isDescriptionMarkdown;
			_resolveAsync = resolveAsync;
		}

		public string Label { get; }
		public string InsertText { get; }
		public string? Detail { get; }
		public string? Description { get; }
		public string FilterText { get; }
		public double Priority { get; }
		public LuaCompletionIconKind IconKind { get; }
		public bool IsDescriptionMarkdown { get; }
		public bool CanResolve => _resolveAsync is not null;

		public Task<LuaCompletionItem> ResolveAsync(CancellationToken cancellationToken = default)
			=> _resolveAsync is null
				? Task.FromResult(this)
				: _resolveAsync(cancellationToken);
	}
}