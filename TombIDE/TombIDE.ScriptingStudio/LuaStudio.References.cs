#nullable enable

using DarkUI.Docking;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.ToolStrips;
using TombIDE.ScriptingStudio.ToolWindows;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Presentation;

namespace TombIDE.ScriptingStudio;

public sealed partial class LuaStudio
{
	private TextReferencesResultsToolWindow LuaReferencesResults
		=> GetPaneContent<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults);

	private readonly LuaReferenceSearchService _referenceSearchService;
	private CancellationTokenSource? _referencesCancellationTokenSource;
	private int _referencesRequestToken;

	private TextReferencesResultsToolWindow CreateLuaReferencesResultsToolWindow()
		=> new TextReferencesResultsToolWindow(
			Shared.Strings.Default.LuaReferencesResults,
			nameof(LuaReferencesResults),
			new TextReferencesPresentation(
				Shared.Strings.Default.LuaReferencesNoDocument,
				Shared.Strings.Default.LuaReferencesUnsupported,
				Shared.Strings.Default.LuaReferencesLoading,
				Shared.Strings.Default.NoReferencesFound),
			NavigateToReference);

	private async Task FindReferencesAsync()
	{
		ShowLuaReferencesResults();

		if (CurrentEditor is not LuaEditor editor)
		{
			LuaReferencesResults.ShowNoActiveDocument();
			return;
		}

		if (!_referenceSearchService.SupportsReferences)
		{
			LuaReferencesResults.ShowUnsupported();
			return;
		}

		CancellationToken cancellationToken = ResetReferenceRequestCancellation();
		int requestToken = ++_referencesRequestToken;
		LuaReferencesResults.ShowLoading();

		try
		{
			IReadOnlyList<TextReferenceGroup> referenceGroups = await _referenceSearchService
				.FindReferencesAsync(editor, cancellationToken)
				.ConfigureAwait(true);

			if (cancellationToken.IsCancellationRequested || requestToken != _referencesRequestToken)
				return;

			if (referenceGroups.Count == 0 && !_referenceSearchService.SupportsReferences)
			{
				LuaReferencesResults.ShowUnsupported();
				UpdateDocumentCommandStates();
				return;
			}

			LuaReferencesResults.ShowReferences(referenceGroups);
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

	private void ShowLuaReferencesResults()
		=> ShowPane(UICommand.LuaReferencesResults);

	private void NavigateToReference(TextReferenceListItem reference)
		=> NavigateToLocation(
			reference.FilePath,
			NavigationOrigin.References,
			textEditor => EditorNavigationHelper.CreateRangeLocation(textEditor, reference.FilePath, reference.Range));

	private CancellationToken ResetReferenceRequestCancellation()
	{
		CancelPendingReferenceRequest();
		_referencesCancellationTokenSource = new CancellationTokenSource();
		return _referencesCancellationTokenSource.Token;
	}
}