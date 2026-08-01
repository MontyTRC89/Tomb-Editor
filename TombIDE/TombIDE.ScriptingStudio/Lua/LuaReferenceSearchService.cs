#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nickelony.LanguageServer.Abstractions.Editing;
using TombLib.Scripting.Lua;
using Nickelony.LanguageServer.Abstractions.Navigation;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Presentation;

namespace TombIDE.ScriptingStudio.Lua;

internal sealed class LuaReferenceSearchService(
	ITextEditorHost textEditorHost,
	ITextReferencesProvider referencesProvider,
	string scriptRootDirectoryPath)
{
	private readonly ITextEditorHost _textEditorHost = textEditorHost ?? throw new ArgumentNullException(nameof(textEditorHost));
	private readonly ITextReferencesProvider _referencesProvider = referencesProvider ?? throw new ArgumentNullException(nameof(referencesProvider));
	private readonly string _scriptRootDirectoryPath = scriptRootDirectoryPath ?? string.Empty;

	public bool SupportsReferences => _referencesProvider.SupportsReferences;

	public async Task<IReadOnlyList<TextReferenceGroup>> FindReferencesAsync(LuaEditor editor, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(editor);

		IReadOnlyList<TextReferenceLocation> references = await _referencesProvider
			.GetReferencesAsync(
				new TextReferenceRequest(
					editor.FilePath,
					editor.Text,
					Math.Max(0, editor.CurrentRow - 1),
					Math.Max(0, editor.CurrentColumn - 1)),
				cancellationToken)
			.ConfigureAwait(true);

		return BuildReferenceGroups(references);
	}

	private IReadOnlyList<TextReferenceGroup> BuildReferenceGroups(IReadOnlyList<TextReferenceLocation> references)
	{
		if (references.Count == 0)
			return [];

		var lineCache = new Dictionary<string, string[]?>(StringComparer.OrdinalIgnoreCase);
		var groups = new List<TextReferenceGroup>();

		foreach (IGrouping<string, TextReferenceLocation> fileGroup in references
			.Where(reference => !string.IsNullOrWhiteSpace(reference.FilePath))
			.GroupBy(reference => reference.FilePath, StringComparer.OrdinalIgnoreCase)
			.OrderBy(group => GetDisplayPath(group.Key), StringComparer.OrdinalIgnoreCase))
		{
			var items = fileGroup
				.OrderBy(reference => reference.StartLineNumber)
				.ThenBy(reference => reference.StartColumnNumber)
				.Select(reference => new TextReferenceListItem(
					reference.FilePath,
					new TextDocumentRange(
						reference.StartLineNumber,
						reference.StartColumnNumber,
						reference.EndLineNumber,
						reference.EndColumnNumber),
					reference.StartLineNumber,
					reference.StartColumnNumber,
					GetPreviewText(reference.FilePath, reference.StartLineNumber, lineCache)))
				.ToArray();

			groups.Add(new TextReferenceGroup(fileGroup.Key, GetDisplayPath(fileGroup.Key), items));
		}

		return groups;
	}

	private string GetDisplayPath(string filePath)
	{
		string fullFilePath = Path.GetFullPath(filePath);
		string fullScriptRootPath = Path.GetFullPath(_scriptRootDirectoryPath);

		if (!fullScriptRootPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
			fullScriptRootPath += Path.DirectorySeparatorChar;

		if (fullFilePath.StartsWith(fullScriptRootPath, StringComparison.OrdinalIgnoreCase))
			return Path.GetRelativePath(fullScriptRootPath, fullFilePath);

		return fullFilePath;
	}

	private string GetPreviewText(string filePath, int lineNumber, Dictionary<string, string[]?> lineCache)
	{
		string? previewText = TryGetOpenEditorLineText(filePath, lineNumber);

		if (previewText is null)
		{
			if (!lineCache.TryGetValue(filePath, out string[]? lines))
			{
				lines = File.Exists(filePath) ? File.ReadAllLines(filePath) : null;
				lineCache[filePath] = lines;
			}

			if (lines is not null && lineNumber >= 1 && lineNumber <= lines.Length)
				previewText = lines[lineNumber - 1];
		}

		return previewText?.Trim() ?? string.Empty;
	}

	private string? TryGetOpenEditorLineText(string filePath, int lineNumber)
	{
		TextEditorBase? textEditor = _textEditorHost.GetOpenEditors(filePath)
			.OfType<TextEditorBase>()
			.FirstOrDefault();

		if (textEditor is null)
			return null;

		return TryGetDocumentLineText(textEditor.Document, lineNumber);
	}

	private static string? TryGetDocumentLineText(TextDocument document, int lineNumber)
	{
		if (lineNumber < 1 || lineNumber > document.LineCount)
			return null;

		DocumentLine line = document.GetLineByNumber(lineNumber);
		return document.GetText(line.Offset, line.Length);
	}
}
