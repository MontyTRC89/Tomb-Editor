#nullable enable

using DarkUI.Docking;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Objects;
using TombIDE.ScriptingStudio.ToolStrips;
using TombIDE.ScriptingStudio.ToolWindows;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Enums;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio;

public sealed partial class LuaStudio
{
	public LuaReferencesResults LuaReferencesResults = null!;

	private CancellationTokenSource? _referencesCancellationTokenSource;
	private int _referencesRequestToken;

	private void InitializeLuaReferencesResults()
	{
		LuaReferencesResults = new LuaReferencesResults(NavigateToReference);
	}

	private async Task FindReferencesAsync()
	{
		ShowLuaReferencesResults();

		if (CurrentEditor is not LuaEditor editor)
		{
			LuaReferencesResults.ShowNoActiveDocument();
			return;
		}

		if (!_intellisenseProvider.SupportsReferences)
		{
			LuaReferencesResults.ShowUnsupported();
			return;
		}

		CancellationToken cancellationToken = ResetReferenceRequestCancellation();
		int requestToken = ++_referencesRequestToken;
		LuaReferencesResults.ShowLoading();

		try
		{
			IReadOnlyList<LuaReferenceLocation> references = await _intellisenseProvider
				.GetReferencesAsync(
					editor.FilePath,
					editor.Text,
					Math.Max(0, editor.CurrentRow - 1),
					Math.Max(0, editor.CurrentColumn - 1),
					cancellationToken)
				.ConfigureAwait(true);

			if (cancellationToken.IsCancellationRequested || requestToken != _referencesRequestToken)
				return;

			if (references.Count == 0 && !_intellisenseProvider.SupportsReferences)
			{
				LuaReferencesResults.ShowUnsupported();
				UpdateLuaFeatureCommandAvailability();
				return;
			}

			LuaReferencesResults.ShowReferences(BuildReferenceGroups(references));
		}
		catch (OperationCanceledException)
		{
			// Ignore stale reference requests.
		}
	}

	private void CancelPendingReferenceRequest()
	{
		_referencesCancellationTokenSource?.Cancel();
		_referencesCancellationTokenSource?.Dispose();
		_referencesCancellationTokenSource = null;
	}

	private void UpdateLuaFeatureCommandAvailability()
	{
		bool hasActiveTextEditor = CurrentEditor is TextEditorBase;
		bool hasActiveLuaEditor = CurrentEditor is LuaEditor;

		SetCommandEnabled(UICommand.FindReferences, hasActiveLuaEditor && _intellisenseProvider.SupportsReferences);
		SetCommandEnabled(UICommand.RenameSymbol, hasActiveLuaEditor && _intellisenseProvider.SupportsRename);
		SetCommandEnabled(UICommand.Reindent, hasActiveLuaEditor ? _intellisenseProvider.SupportsFormatting : hasActiveTextEditor);
	}

	private void SetCommandEnabled(UICommand command, bool isEnabled)
	{
		if (MenuStrip.FindItem(command) is ToolStripItem menuItem)
			menuItem.Enabled = isEnabled;

		if (EditorContextMenu.FindItem(command) is ToolStripItem contextMenuItem)
			contextMenuItem.Enabled = isEnabled;
	}

	private void ShowLuaReferencesResults()
	{
		if (!DockPanel.ContainsContent(LuaReferencesResults))
		{
			LuaReferencesResults.DockArea = DarkDockArea.Bottom;
			DockPanel.AddContent(LuaReferencesResults);
		}

		LuaReferencesResults.DockGroup.SetVisibleContent(LuaReferencesResults);
	}

	private void NavigateToReference(LuaReferenceListItem reference)
		=> NavigateToLocation(
			reference.FilePath,
			NavigationOrigin.References,
			textEditor => EditorNavigationHelper.CreateRangeLocation(textEditor, reference.FilePath, reference.Range));

	private IReadOnlyList<LuaReferenceGroup> BuildReferenceGroups(IReadOnlyList<LuaReferenceLocation> references)
	{
		if (references.Count == 0)
			return [];

		var lineCache = new Dictionary<string, string[]?>(StringComparer.OrdinalIgnoreCase);
		var groups = new List<LuaReferenceGroup>();

		foreach (IGrouping<string, LuaReferenceLocation> fileGroup in references
			.Where(reference => !string.IsNullOrWhiteSpace(reference.FilePath))
			.GroupBy(reference => reference.FilePath, StringComparer.OrdinalIgnoreCase)
			.OrderBy(group => GetDisplayPath(group.Key), StringComparer.OrdinalIgnoreCase))
		{
			var items = fileGroup
				.OrderBy(reference => reference.Range.StartLineNumber)
				.ThenBy(reference => reference.Range.StartColumnNumber)
				.Select(reference => new LuaReferenceListItem(
					reference.FilePath,
					reference.Range,
					reference.Range.StartLineNumber,
					reference.Range.StartColumnNumber,
					GetPreviewText(reference.FilePath, reference.Range.StartLineNumber, lineCache)))
				.ToArray();

			groups.Add(new LuaReferenceGroup(fileGroup.Key, GetDisplayPath(fileGroup.Key), items));
		}

		return groups;
	}

	private string GetDisplayPath(string filePath)
	{
		string fullFilePath = Path.GetFullPath(filePath);
		string fullScriptRootPath = Path.GetFullPath(ScriptRootDirectoryPath);

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
		TabPage? tabPage = EditorTabControl.FindTabPage(filePath, EditorType.Text);

		if (tabPage is null || EditorTabControl.GetEditorOfTab(tabPage) is not TextEditorBase textEditor)
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

	private CancellationToken ResetReferenceRequestCancellation()
	{
		CancelPendingReferenceRequest();
		_referencesCancellationTokenSource = new CancellationTokenSource();
		return _referencesCancellationTokenSource.Token;
	}
}