#nullable enable

using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Scripting.Presentation;
using TombLib.Scripting.UI.Bases;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class LuaReferencesPaneProvider : IStudioPaneContributionProvider
{
	private readonly ScriptingWorkspaceProfile _profile;
	private readonly IEditorDocumentController _documentController;

	public LuaReferencesPaneProvider(
		ScriptingWorkspaceProfile profile,
		IEditorDocumentController documentController)
	{
		ArgumentNullException.ThrowIfNull(profile);
		ArgumentNullException.ThrowIfNull(documentController);

		_profile = profile;
		_documentController = documentController;
	}

	public IReadOnlyList<StudioPaneContribution> GetPaneContributions()
	{
		if (_profile.Kind != ScriptingWorkspaceKind.Lua || !_profile.SupportsView(UICommand.LuaReferencesResults))
			return [];

		var pane = new TextReferencesResultsToolWindow(
			Shared.Strings.Default.LuaReferencesResults,
			"LuaReferencesResults",
			new TextReferencesPresentation(
				Shared.Strings.Default.LuaReferencesNoDocument,
				Shared.Strings.Default.LuaReferencesUnsupported,
				Shared.Strings.Default.LuaReferencesLoading,
				Shared.Strings.Default.NoReferencesFound),
			NavigateToReference);

		return [new StudioPaneContribution(UICommand.LuaReferencesResults, pane.SerializationKey, () => pane)];
	}

	private void NavigateToReference(TextReferenceListItem reference)
	{
		NavigateToLocation(
			reference.FilePath,
			textEditor => EditorNavigationHelper.CreateRangeLocation(textEditor, reference.FilePath, reference.Range));
	}

	private void NavigateToLocation(string filePath, Func<TextEditorBase, EditorNavigationLocation?> locationFactory)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			return;

		_documentController.OpenFile(filePath);

		if (_documentController.CurrentEditor is not TextEditorBase textEditor)
			return;

		EditorNavigationLocation? location = locationFactory(textEditor);

		if (location is null)
			return;

		EditorNavigationHelper.ApplyLocation(textEditor, location.Value);
	}
}
