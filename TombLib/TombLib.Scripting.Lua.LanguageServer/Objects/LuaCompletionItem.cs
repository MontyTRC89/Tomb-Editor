namespace TombLib.Scripting.Lua.Objects;

/// <summary>
/// Represents a zero-based document position used by LSP completion text edits.
/// </summary>
public readonly record struct LuaCompletionPosition(int Line, int Character);

/// <summary>
/// Represents a zero-based document range used by LSP completion text edits.
/// </summary>
public readonly record struct LuaCompletionRange(LuaCompletionPosition Start, LuaCompletionPosition End);

/// <summary>
/// Describes the insert/replace ranges supplied by an LSP completion item.
/// </summary>
public readonly record struct LuaCompletionTextEdit(LuaCompletionRange InsertRange, LuaCompletionRange? ReplaceRange = null)
{
	/// <summary>
	/// Gets the effective replacement range, falling back to <see cref="InsertRange"/> when no distinct replace range exists.
	/// </summary>
	public LuaCompletionRange ReplacementRange => ReplaceRange ?? InsertRange;
}

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
	/// <param name="textEdit">The optional LSP insert/replace edit metadata.</param>
	/// <param name="requestDocumentVersion">The editor document version associated with the completion request.</param>
	/// <param name="requestGeneration">The editor request generation associated with the completion request.</param>
	/// <param name="insertCaretOffset">The optional caret offset within <paramref name="insertText"/> after plain-text insertion.</param>
	public LuaCompletionItem(
		string label,
		string? insertText = null,
		string? detail = null,
		string? description = null,
		string? filterText = null,
		double priority = 0.0,
		LuaCompletionIconKind iconKind = LuaCompletionIconKind.Misc,
		bool isDescriptionMarkdown = false,
		Func<CancellationToken, Task<LuaCompletionItem>>? resolveAsync = null,
		LuaCompletionTextEdit? textEdit = null,
		int? requestDocumentVersion = null,
		int? requestGeneration = null,
		int? insertCaretOffset = null)
	{
		Label = label;
		InsertText = string.IsNullOrWhiteSpace(insertText) ? label : insertText;
		Detail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
		Description = string.IsNullOrWhiteSpace(description) ? null : isDescriptionMarkdown ? description : description.Trim();
		FilterText = string.IsNullOrWhiteSpace(filterText) ? label : filterText;
		Priority = priority;
		IconKind = iconKind;
		IsDescriptionMarkdown = isDescriptionMarkdown;
		TextEdit = textEdit;
		RequestDocumentVersion = requestDocumentVersion;
		RequestGeneration = requestGeneration;
		InsertCaretOffset = insertCaretOffset;
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
	/// Gets the optional LSP insert/replace edit metadata.
	/// </summary>
	public LuaCompletionTextEdit? TextEdit { get; }

	/// <summary>
	/// Gets the editor document version associated with the originating completion request.
	/// </summary>
	public int? RequestDocumentVersion { get; }

	/// <summary>
	/// Gets the editor request generation associated with the originating completion request.
	/// </summary>
	public int? RequestGeneration { get; }

	/// <summary>
	/// Gets the optional caret offset within <see cref="InsertText"/> after plain-text insertion.
	/// </summary>
	public int? InsertCaretOffset { get; }

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
		=> new(Label, InsertText, Detail, Description, FilterText, Priority, IconKind, IsDescriptionMarkdown,
			resolveAsync, TextEdit, RequestDocumentVersion, RequestGeneration, InsertCaretOffset);

	/// <summary>
	/// Returns a copy of this item with editor request metadata attached.
	/// </summary>
	/// <param name="requestDocumentVersion">The editor document version for the completion request.</param>
	/// <param name="requestGeneration">The editor request generation for the completion request.</param>
	/// <returns>A new completion item with request metadata attached.</returns>
	public LuaCompletionItem WithRequestContext(int requestDocumentVersion, int requestGeneration)
	{
		if (RequestDocumentVersion == requestDocumentVersion && RequestGeneration == requestGeneration)
			return this;

		Func<CancellationToken, Task<LuaCompletionItem>>? resolveAsync = _resolveAsync is null
			? null
			: async cancellationToken =>
				(await _resolveAsync(cancellationToken).ConfigureAwait(false))
					.WithRequestContext(requestDocumentVersion, requestGeneration);

		return new LuaCompletionItem(Label, InsertText, Detail, Description, FilterText, Priority, IconKind,
			IsDescriptionMarkdown, resolveAsync, TextEdit, requestDocumentVersion, requestGeneration, InsertCaretOffset);
	}

	/// <summary>
	/// Returns a copy of this item rebound to the current filtered popup state.
	/// </summary>
	/// <param name="requestDocumentVersion">The current editor document version.</param>
	/// <param name="requestGeneration">The current editor request generation.</param>
	/// <returns>A new completion item that commits against the current completion segment.</returns>
	public LuaCompletionItem WithFilteredCommitContext(int requestDocumentVersion, int requestGeneration)
	{
		if (RequestDocumentVersion == requestDocumentVersion
			&& RequestGeneration == requestGeneration
			&& TextEdit is null)
		{
			return this;
		}

		Func<CancellationToken, Task<LuaCompletionItem>>? resolveAsync = _resolveAsync is null
			? null
			: async cancellationToken =>
				(await _resolveAsync(cancellationToken).ConfigureAwait(false))
					.WithFilteredCommitContext(requestDocumentVersion, requestGeneration);

		return new LuaCompletionItem(Label, InsertText, Detail, Description, FilterText, Priority, IconKind,
			IsDescriptionMarkdown, resolveAsync, textEdit: null, requestDocumentVersion, requestGeneration, InsertCaretOffset);
	}

	/// <summary>
	/// Returns a copy of this item with resolved detail and documentation merged onto the original insertion metadata.
	/// </summary>
	/// <param name="resolvedItem">The lazily resolved item.</param>
	/// <returns>A new completion item with resolved presentation data and preserved insertion metadata.</returns>
	public LuaCompletionItem WithResolvedContent(LuaCompletionItem resolvedItem)
	{
		ArgumentNullException.ThrowIfNull(resolvedItem);

		string? detail = string.IsNullOrWhiteSpace(resolvedItem.Detail) ? Detail : resolvedItem.Detail;
		string? description = string.IsNullOrWhiteSpace(resolvedItem.Description) ? Description : resolvedItem.Description;
		bool isDescriptionMarkdown = string.IsNullOrWhiteSpace(resolvedItem.Description)
			? IsDescriptionMarkdown
			: resolvedItem.IsDescriptionMarkdown;

		return new LuaCompletionItem(Label, InsertText, detail, description, FilterText, Priority, IconKind,
			isDescriptionMarkdown, resolveAsync: null, TextEdit, RequestDocumentVersion, RequestGeneration, InsertCaretOffset);
	}
}