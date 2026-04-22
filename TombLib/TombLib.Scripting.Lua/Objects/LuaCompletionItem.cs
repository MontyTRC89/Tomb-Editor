using System;
using System.Threading;
using System.Threading.Tasks;

namespace TombLib.Scripting.Lua.Objects;

/// <summary>
/// Represents a single completion entry returned by a Lua IntelliSense provider.
/// </summary>
public sealed class LuaCompletionItem
{
	private readonly Func<CancellationToken, Task<LuaCompletionItem>>? _resolveAsync;

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaCompletionItem"/> class.
	/// </summary>
	/// <param name="label">The label shown in the completion list.</param>
	/// <param name="insertText">The text inserted when the item is committed.</param>
	/// <param name="detail">Optional secondary detail shown in the completion list.</param>
	/// <param name="description">Optional description shown in the completion tooltip.</param>
	/// <param name="filterText">Optional filter text used when matching the item.</param>
	/// <param name="priority">The sort priority for the item.</param>
	/// <param name="iconKind">The icon category shown for the item.</param>
	/// <param name="isDescriptionMarkdown">Whether <paramref name="description"/> should be rendered as Markdown.</param>
	/// <param name="resolveAsync">Optional callback that lazily resolves additional item data.</param>
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
		Label = label;
		InsertText = string.IsNullOrWhiteSpace(insertText) ? label : insertText;
		Detail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
		Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
		FilterText = string.IsNullOrWhiteSpace(filterText) ? label : filterText;
		Priority = priority;
		IconKind = iconKind;
		IsDescriptionMarkdown = isDescriptionMarkdown;
		_resolveAsync = resolveAsync;
	}

	/// <summary>
	/// Gets the label shown in the completion list.
	/// </summary>
	public string Label { get; }

	/// <summary>
	/// Gets the text inserted when the item is committed.
	/// </summary>
	public string InsertText { get; }

	/// <summary>
	/// Gets the optional secondary detail shown alongside the label.
	/// </summary>
	public string? Detail { get; }

	/// <summary>
	/// Gets the optional completion description.
	/// </summary>
	public string? Description { get; }

	/// <summary>
	/// Gets the text used to filter or match the item.
	/// </summary>
	public string FilterText { get; }

	/// <summary>
	/// Gets the sort priority for the item.
	/// </summary>
	public double Priority { get; }

	/// <summary>
	/// Gets the icon category shown for the item.
	/// </summary>
	public LuaCompletionIconKind IconKind { get; }

	/// <summary>
	/// Gets a value indicating whether <see cref="Description"/> should be rendered as Markdown.
	/// </summary>
	public bool IsDescriptionMarkdown { get; }

	/// <summary>
	/// Gets a value indicating whether additional item details can be resolved lazily.
	/// </summary>
	public bool CanResolve => _resolveAsync is not null;

	/// <summary>
	/// Resolves the completion item, returning either the current instance or a lazily populated copy.
	/// </summary>
	/// <param name="cancellationToken">A token that can cancel the resolve request.</param>
	/// <returns>The resolved completion item.</returns>
	public Task<LuaCompletionItem> ResolveAsync(CancellationToken cancellationToken = default)
	{
		return _resolveAsync is null
			? Task.FromResult(this)
			: _resolveAsync(cancellationToken);
	}

	/// <summary>
	/// Returns a copy of this item with the supplied resolve callback attached.
	/// </summary>
	/// <param name="resolveAsync">The lazy resolve callback.</param>
	/// <returns>A new completion item with the resolve callback attached.</returns>
	public LuaCompletionItem WithResolveCallback(Func<CancellationToken, Task<LuaCompletionItem>> resolveAsync)
		=> new(Label, InsertText, Detail, Description, FilterText, Priority, IconKind, IsDescriptionMarkdown, resolveAsync);
}
