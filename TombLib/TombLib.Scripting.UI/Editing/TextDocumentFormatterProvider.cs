#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Nickelony.LanguageServer.Core.Editing;
using TombLib.Scripting.UI.Cleaning;

namespace TombLib.Scripting.UI.Editing;

/// <summary>
/// Adapts a local document formatter to the shared workspace-edit formatting contract.
/// </summary>
public sealed class TextDocumentFormatterProvider : ITextFormattingProvider
{
	private readonly ITextDocumentFormatter _documentFormatter;

	public TextDocumentFormatterProvider(ITextDocumentFormatter documentFormatter)
	{
		_documentFormatter = documentFormatter ?? throw new ArgumentNullException(nameof(documentFormatter));
	}

	public bool SupportsFormatting => true;

	public Task<TextWorkspaceEdit?> FormatDocumentAsync(TextFormatRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		cancellationToken.ThrowIfCancellationRequested();

		string formattedText = _documentFormatter.FormatDocument(request.DocumentText);

		if (string.Equals(formattedText, request.DocumentText, StringComparison.Ordinal))
			return Task.FromResult<TextWorkspaceEdit?>(null);

		TextDocumentRange documentRange = CreateDocumentRange(request.DocumentText);

		return Task.FromResult<TextWorkspaceEdit?>(new TextWorkspaceEdit([
			new TextDocumentEdit(request.FilePath, [
				new TextEdit(documentRange, formattedText)
			])
		]));
	}

	private static TextDocumentRange CreateDocumentRange(string content)
	{
		string[] lines = content.Replace("\r", string.Empty).Split('\n');
		int endLineNumber = lines.Length;
		int endColumnNumber = lines[^1].Length + 1;

		return new TextDocumentRange(1, 1, endLineNumber, endColumnNumber);
	}
}
