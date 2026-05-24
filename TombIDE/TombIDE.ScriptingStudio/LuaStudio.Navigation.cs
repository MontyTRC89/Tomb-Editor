#nullable enable

using System;
using System.IO;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Navigation;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Bases;

namespace TombIDE.ScriptingStudio;

public sealed partial class LuaStudio
{
	private readonly EditorNavigationHistoryService _navigationHistory = new();

	private enum NavigationOrigin
	{
		Definition,
		Diagnostics,
		References,
		SearchResults,
		HistoryBack,
		HistoryForward
	}

	private void LuaEditor_StatusChanged(object? sender, EventArgs e)
	{
		if (sender is not LuaEditor editor)
			return;

		_navigationHistory.Observe(EditorNavigationHelper.CreateLocation(editor));
	}

	private void NavigateBack()
	{
		if (TryGetCurrentNavigationLocation() is not EditorNavigationLocation currentLocation)
			return;

		if (!_navigationHistory.TryNavigateBack(currentLocation, out EditorNavigationLocation? targetLocation)
			|| targetLocation is null)
			return;

		NavigateToLocation(
			targetLocation.Value.FilePath,
			NavigationOrigin.HistoryBack,
			_ => targetLocation.Value);
	}

	private void NavigateForward()
	{
		if (TryGetCurrentNavigationLocation() is not EditorNavigationLocation currentLocation)
			return;

		if (!_navigationHistory.TryNavigateForward(currentLocation, out EditorNavigationLocation? targetLocation)
			|| targetLocation is null)
			return;

		NavigateToLocation(
			targetLocation.Value.FilePath,
			NavigationOrigin.HistoryForward,
			_ => targetLocation.Value);
	}

	private EditorNavigationLocation? TryGetCurrentNavigationLocation()
	{
		if (CurrentEditor is not TextEditorBase textEditor || string.IsNullOrWhiteSpace(textEditor.FilePath))
			return null;

		return EditorNavigationHelper.CreateLocation(textEditor);
	}

	private void NavigateToLocation(
		string filePath,
		NavigationOrigin origin,
		Func<TextEditorBase, EditorNavigationLocation?> locationFactory,
		EditorNavigationLocation? sourceLocation = null)
	{
		if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
			return;

		EditorNavigationLocation? currentLocation = sourceLocation ?? TryGetCurrentNavigationLocation();

		using IDisposable suppression = _navigationHistory.SuppressRecording();

		EditorTabControl.OpenFile(filePath);

		if (CurrentEditor is not TextEditorBase textEditor)
			return;

		EditorNavigationLocation? targetLocation = locationFactory(textEditor);
		if (targetLocation is null)
			return;

		if (origin is NavigationOrigin.Definition or NavigationOrigin.Diagnostics or NavigationOrigin.References or NavigationOrigin.SearchResults
			&& currentLocation is EditorNavigationLocation source
			&& !source.IsEquivalentTo(targetLocation.Value))
		{
			_navigationHistory.RecordProgrammaticJump(source, targetLocation.Value);
		}

		EditorNavigationHelper.ApplyLocation(textEditor, targetLocation.Value);
		_navigationHistory.SetCurrentLocation(EditorNavigationHelper.CreateLocation(textEditor));
	}

	protected override void NavigateToSearchResult(string filePath, FindReplaceItem item)
		=> NavigateToLocation(
			filePath,
			NavigationOrigin.SearchResults,
			textEditor => EditorNavigationHelper.TryCreateSearchResultLocation(textEditor, filePath, item, out EditorNavigationLocation? location)
				? location
				: null);
}