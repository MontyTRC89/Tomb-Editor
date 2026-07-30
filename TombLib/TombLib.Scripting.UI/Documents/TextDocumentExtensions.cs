#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;

namespace TombLib.Scripting.UI.Documents;

/// <summary>
/// Provides shared helpers for working with AvalonEdit text documents.
/// </summary>
public static class TextDocumentExtensions
{
	/// <summary>
	/// Clamps an offset so it always falls within the current document bounds.
	/// </summary>
	/// <param name="document">The document whose bounds should be used.</param>
	/// <param name="offset">The offset to clamp.</param>
	/// <returns>A valid document offset between 0 and <see cref="TextDocument.TextLength"/>.</returns>
	public static int ClampOffset(this TextDocument document, int offset)
	{
		ArgumentNullException.ThrowIfNull(document);

		return Math.Clamp(offset, 0, document.TextLength);
	}
}
