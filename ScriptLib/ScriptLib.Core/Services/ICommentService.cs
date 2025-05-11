using ICSharpCode.AvalonEdit.Document;

namespace ScriptLib.Core.Services;

/// <summary>
/// Interface for managing comments in text documents.
/// </summary>
public interface ICommentService
{
	/// <summary>
	/// Toggles comments on the specified lines in the document.
	/// </summary>
	/// <param name="document">The document to process.</param>
	/// <param name="lines">The lines to toggle comments on.</param>
	/// <param name="commentPrefix">The prefix used for comments.</param>
	void ToggleComments(TextDocument document, IReadOnlyList<DocumentLine> lines, string commentPrefix);

	/// <summary>
	/// Comments out the specified lines in the document by adding the comment prefix.
	/// </summary>
	/// <param name="document">The document to process.</param>
	/// <param name="lines">The lines to comment out.</param>
	/// <param name="commentPrefix">The prefix to add at the beginning of each line.</param>
	void CommentOutLines(TextDocument document, IReadOnlyList<DocumentLine> lines, string commentPrefix);

	/// <summary>
	/// Removes comment prefixes from the specified lines in the document.
	/// </summary>
	/// <param name="document">The document to process.</param>
	/// <param name="lines">The lines to uncomment.</param>
	/// <param name="commentPrefix">The comment prefix to remove from each line.</param>
	void UncommentLines(TextDocument document, IReadOnlyList<DocumentLine> lines, string commentPrefix);
}
