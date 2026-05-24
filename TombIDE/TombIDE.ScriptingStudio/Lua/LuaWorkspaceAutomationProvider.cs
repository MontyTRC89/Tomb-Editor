using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Services;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.Scripting.Lua.Documents;

namespace TombIDE.ScriptingStudio.Lua;

internal sealed record LuaWorkspaceAutomationCallbacks(
	Func<ScriptGenerationResult, (bool ScriptUpdated, bool LanguageUpdated)> AppendScript,
	Func<string, bool> IsLevelScriptDefined,
	Func<string, bool> IsLevelLanguageStringDefined,
	Action<string, string> RenameRequestedLanguageString,
	Action DisposeIntellisense);

internal sealed class LuaWorkspaceAutomationProvider : IStudioWorkspaceAutomationProvider
{
	private readonly LuaWorkspaceAutomationCallbacks _callbacks;
	private readonly string _scriptRootDirectoryPath;
	private readonly StudioSilentActionService _silentActionService;

	public LuaWorkspaceAutomationProvider(
		StudioSilentActionService silentActionService,
		string scriptRootDirectoryPath,
		LuaWorkspaceAutomationCallbacks callbacks)
	{
		_silentActionService = silentActionService ?? throw new ArgumentNullException(nameof(silentActionService));
		_scriptRootDirectoryPath = scriptRootDirectoryPath ?? string.Empty;
		_callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
	}

	public void HandleIDEEvent(IIDEEvent ideEvent)
	{
		if (ideEvent is null)
			return;

		if (ideEvent is IDE.ProgramClosingEvent)
		{
			_callbacks.DisposeIntellisense();
			return;
		}

		if (!IsSilentAction(ideEvent))
			return;

		TabPage cachedTab = _silentActionService.RememberSelectedTab();
		string scriptFilePath = PathHelper.GetScriptFilePath(_scriptRootDirectoryPath, TRVersion.Game.TombEngine);
		string languageFilePath = PathHelper.GetLanguageFilePath(_scriptRootDirectoryPath, TRVersion.Game.TombEngine);

		if (ideEvent is IDE.ScriptEditor_AppendScriptEvent appendEvent && appendEvent.Result.HasOutput)
		{
			SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
			SilentActionFileState languageFileState = _silentActionService.CaptureFileState(languageFilePath);
			(bool scriptUpdated, bool languageUpdated) = _callbacks.AppendScript(appendEvent.Result);
			var completions = new List<SilentActionCompletion>();

			if (scriptUpdated)
				completions.Add(_silentActionService.CreateCompletion(scriptFileState));

			if (languageUpdated)
				completions.Add(_silentActionService.CreateCompletion(languageFileState));

			_silentActionService.Complete(cachedTab, scriptUpdated || languageUpdated, completions.ToArray());
		}
		else if (ideEvent is IDE.ScriptEditor_ScriptPresenceCheckEvent scriptPresenceEvent)
		{
			IDE.Instance.ScriptDefined = _callbacks.IsLevelScriptDefined(scriptPresenceEvent.LevelName);
		}
		else if (ideEvent is IDE.ScriptEditor_StringPresenceCheckEvent stringPresenceEvent)
		{
			SilentActionFileState languageFileState = _silentActionService.CaptureFileState(languageFilePath);
			IDE.Instance.StringDefined = _callbacks.IsLevelLanguageStringDefined(stringPresenceEvent.String);
			_silentActionService.Complete(cachedTab, false, _silentActionService.CreateCompletion(languageFileState, saveAffectedFile: false));
		}
		else if (ideEvent is IDE.ScriptEditor_RenameLevelEvent renameLevelEvent)
		{
			SilentActionFileState languageFileState = _silentActionService.CaptureFileState(languageFilePath);
			_callbacks.RenameRequestedLanguageString(renameLevelEvent.OldName, renameLevelEvent.NewName);
			_silentActionService.Complete(cachedTab, true, _silentActionService.CreateCompletion(languageFileState));
		}
	}

	public void Build()
	{
	}

	public void ShowDocumentation()
	{
	}

	private static bool IsSilentAction(IIDEEvent ideEvent)
		=> ideEvent is IDE.ScriptEditor_AppendScriptEvent
		|| ideEvent is IDE.ScriptEditor_ScriptPresenceCheckEvent
		|| ideEvent is IDE.ScriptEditor_StringPresenceCheckEvent
		|| ideEvent is IDE.ScriptEditor_RenameLevelEvent;
}