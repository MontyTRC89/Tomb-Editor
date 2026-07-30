namespace TombLib.Scripting.Completion;

/// <summary>
/// Represents one shared completion entry that can be rendered by any scripting editor.
/// </summary>
/// <remarks>
/// Optional fields drive optional UI regions. Missing detail, description, resolve callbacks,
/// or protocol text-edit metadata should suppress those behaviors rather than requiring a
/// language-specific completion DTO.
/// </remarks>
public sealed class TextCompletionItem
{
	private readonly Func<CancellationToken, Task<TextCompletionItem>>? _resolveAsync;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextCompletionItem"/> class.
	/// </summary>
	/// <param name="label">The display label shown in the completion list.</param>
	/// <param name="insertText">The text inserted when the item is committed.</param>
	/// <param name="description">Optional descriptive content for tooltip or detail rendering.</param>
	/// <param name="priority">The sort priority used by the completion UI.</param>
	/// <param name="kind">The semantic category used for icons and styling.</param>
	/// <param name="detail">Optional short detail text shown beside the label.</param>
	/// <param name="filterText">The text used to match the item during filtering.</param>
	/// <param name="isDescriptionMarkdown">Whether <paramref name="description"/> should be rendered as Markdown.</param>
	/// <param name="resolveAsync">An optional asynchronous resolver for lazily loading richer content.</param>
	/// <param name="textEdit">Optional protocol-style insert and replace ranges for custom commit behavior.</param>
	/// <param name="requestDocumentVersion">The originating document version for staleness checks.</param>
	/// <param name="requestGeneration">The originating request generation for staleness checks.</param>
	/// <param name="insertCaretOffset">An optional caret position to use after commit.</param>
	public TextCompletionItem(
		string label,
		string? insertText = null,
		string? description = null,
		double priority = 0.0,
		TextCompletionItemKind kind = TextCompletionItemKind.Generic,
		string? detail = null,
		string? filterText = null,
		bool isDescriptionMarkdown = false,
		Func<CancellationToken, Task<TextCompletionItem>>? resolveAsync = null,
		TextCompletionTextEdit? textEdit = null,
		int? requestDocumentVersion = null,
		int? requestGeneration = null,
		int? insertCaretOffset = null)
	{
		Label = label;
		InsertText = string.IsNullOrWhiteSpace(insertText) ? label : insertText;
		Description = string.IsNullOrWhiteSpace(description) ? null : isDescriptionMarkdown ? description : description.Trim();
		Priority = priority;
		Kind = kind;
		Detail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
		FilterText = string.IsNullOrWhiteSpace(filterText) ? label : filterText;
		IsDescriptionMarkdown = isDescriptionMarkdown;
		TextEdit = textEdit;
		RequestDocumentVersion = requestDocumentVersion;
		RequestGeneration = requestGeneration;
		InsertCaretOffset = insertCaretOffset;

		_resolveAsync = resolveAsync;
	}

	/// <summary>
	/// Gets the display label shown in the completion list.
	/// </summary>
	public string Label { get; }

	/// <summary>
	/// Gets the text inserted when the item is committed.
	/// </summary>
	public string InsertText { get; }

	/// <summary>
	/// Gets optional descriptive content for tooltip or detail rendering.
	/// </summary>
	public string? Description { get; }

	/// <summary>
	/// Gets the sort priority used by the completion UI.
	/// </summary>
	public double Priority { get; }

	/// <summary>
	/// Gets the semantic category used for icons and styling.
	/// </summary>
	public TextCompletionItemKind Kind { get; }

	/// <summary>
	/// Gets optional short detail text shown beside or above the label.
	/// </summary>
	public string? Detail { get; }

	/// <summary>
	/// Gets the text used to match the item during filtering.
	/// </summary>
	public string FilterText { get; }

	/// <summary>
	/// Gets a value indicating whether <see cref="Description"/> should be rendered as Markdown.
	/// </summary>
	public bool IsDescriptionMarkdown { get; }

	/// <summary>
	/// Gets the optional protocol-style edit payload used when committing the item.
	/// </summary>
	public TextCompletionTextEdit? TextEdit { get; }

	/// <summary>
	/// Gets the originating document version used to reject stale completions.
	/// </summary>
	public int? RequestDocumentVersion { get; }

	/// <summary>
	/// Gets the originating request generation used to reject stale completions.
	/// </summary>
	public int? RequestGeneration { get; }

	/// <summary>
	/// Gets the optional caret offset to place after commit.
	/// </summary>
	public int? InsertCaretOffset { get; }

	/// <summary>
	/// Gets a value indicating whether this item can be asynchronously resolved.
	/// </summary>
	public bool CanResolve => _resolveAsync is not null;

	/// <summary>
	/// Resolves the item to richer content when a resolve callback is available.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token for the resolve request.</param>
	/// <returns>The resolved completion item.</returns>
	public Task<TextCompletionItem> ResolveAsync(CancellationToken cancellationToken = default)
	{
		return _resolveAsync is null
			? Task.FromResult(this)
			: _resolveAsync(cancellationToken);
	}

	/// <summary>
	/// Creates a copy of the item with a resolve callback attached.
	/// </summary>
	/// <param name="resolveAsync">The resolve callback to attach.</param>
	/// <returns>A completion item with resolve support.</returns>
	public TextCompletionItem WithResolveCallback(Func<CancellationToken, Task<TextCompletionItem>> resolveAsync)
	{
		return new(Label, InsertText, Description, Priority, Kind, Detail, FilterText, IsDescriptionMarkdown,
			resolveAsync, TextEdit, RequestDocumentVersion, RequestGeneration, InsertCaretOffset);
	}

	/// <summary>
	/// Creates a copy stamped with request metadata used to reject stale completions.
	/// </summary>
	/// <param name="requestDocumentVersion">The originating document version.</param>
	/// <param name="requestGeneration">The originating request generation.</param>
	/// <returns>A completion item stamped with the supplied request context.</returns>
	public TextCompletionItem WithRequestContext(int requestDocumentVersion, int requestGeneration)
	{
		if (RequestDocumentVersion == requestDocumentVersion && RequestGeneration == requestGeneration)
			return this;

		Func<CancellationToken, Task<TextCompletionItem>>? resolveAsync = _resolveAsync is null
			? null
			: async cancellationToken =>
				(await _resolveAsync(cancellationToken).ConfigureAwait(false))
					.WithRequestContext(requestDocumentVersion, requestGeneration);

		return new TextCompletionItem(Label, InsertText, Description, Priority, Kind, Detail, FilterText,
			IsDescriptionMarkdown, resolveAsync, TextEdit, requestDocumentVersion, requestGeneration, InsertCaretOffset);
	}

	/// <summary>
	/// Creates a copy narrowed for commit-time use after filtering out stale text-edit metadata.
	/// </summary>
	/// <param name="requestDocumentVersion">The originating document version.</param>
	/// <param name="requestGeneration">The originating request generation.</param>
	/// <returns>A completion item prepared for safe commit against the current document.</returns>
	public TextCompletionItem WithFilteredCommitContext(int requestDocumentVersion, int requestGeneration)
	{
		if (RequestDocumentVersion == requestDocumentVersion
			&& RequestGeneration == requestGeneration
			&& TextEdit is null)
		{
			return this;
		}

		Func<CancellationToken, Task<TextCompletionItem>>? resolveAsync = _resolveAsync is null
			? null
			: async cancellationToken =>
				(await _resolveAsync(cancellationToken).ConfigureAwait(false))
					.WithFilteredCommitContext(requestDocumentVersion, requestGeneration);

		return new TextCompletionItem(
			Label, InsertText, Description, Priority, Kind, Detail, FilterText, IsDescriptionMarkdown, resolveAsync,
			textEdit: null,
			requestDocumentVersion: requestDocumentVersion,
			requestGeneration: requestGeneration,
			insertCaretOffset: InsertCaretOffset);
	}

	/// <summary>
	/// Creates a copy that merges richer resolved content onto the current item.
	/// </summary>
	/// <param name="resolvedItem">The resolved completion item carrying richer detail and description.</param>
	/// <returns>A merged completion item that preserves the original commit metadata.</returns>
	public TextCompletionItem WithResolvedContent(TextCompletionItem resolvedItem)
	{
		string? detail = string.IsNullOrWhiteSpace(resolvedItem.Detail) ? Detail : resolvedItem.Detail;
		string? description = string.IsNullOrWhiteSpace(resolvedItem.Description) ? Description : resolvedItem.Description;

		bool isDescriptionMarkdown = string.IsNullOrWhiteSpace(resolvedItem.Description)
			? IsDescriptionMarkdown
			: resolvedItem.IsDescriptionMarkdown;

		TextCompletionItemKind kind = resolvedItem.Kind == TextCompletionItemKind.Generic ? Kind : resolvedItem.Kind;

		return new TextCompletionItem(
			Label, InsertText, description, Priority, kind, detail, FilterText, isDescriptionMarkdown,
			resolveAsync: null,
			textEdit: TextEdit,
			requestDocumentVersion: RequestDocumentVersion,
			requestGeneration: RequestGeneration,
			insertCaretOffset: InsertCaretOffset);
	}
}
