using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.Services;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;

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
			HandleProgramClosing();
			return;
		}

		if (!IsSilentAction(ideEvent))
			return;

		switch (ideEvent)
		{
			case IDE.ScriptEditor_AppendScriptEvent appendEvent:
				AppendScript(appendEvent.Result);
				break;

			case IDE.ScriptEditor_ScriptPresenceCheckEvent scriptPresenceEvent:
				IDE.Instance.ScriptDefined = IsScriptDefined(scriptPresenceEvent.LevelName);
				break;

			case IDE.ScriptEditor_StringPresenceCheckEvent stringPresenceEvent:
				IDE.Instance.StringDefined = IsStringDefined(stringPresenceEvent.String);
				break;

			case IDE.ScriptEditor_RenameLevelEvent renameLevelEvent:
				RenameLevel(renameLevelEvent.OldName, renameLevelEvent.NewName);
				break;
		}
	}

	public void AppendScript(ScriptGenerationResult result)
	{
		if (!result.HasOutput)
			return;

		var cachedEditor = _silentActionService.RememberSelectedEditor();
		string scriptFilePath = PathHelper.GetScriptFilePath(_scriptRootDirectoryPath, TRVersion.Game.TombEngine);
		string languageFilePath = PathHelper.GetLanguageFilePath(_scriptRootDirectoryPath, TRVersion.Game.TombEngine);
		SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
		SilentActionFileState languageFileState = _silentActionService.CaptureFileState(languageFilePath);
		(bool scriptUpdated, bool languageUpdated) = _callbacks.AppendScript(result);
		var completions = new List<SilentActionCompletion>();

		if (scriptUpdated)
			completions.Add(_silentActionService.CreateCompletion(scriptFileState));

		if (languageUpdated)
			completions.Add(_silentActionService.CreateCompletion(languageFileState));

		_silentActionService.Complete(cachedEditor, scriptUpdated || languageUpdated, completions.ToArray());
	}

	public void HandleProgramClosing()
		=> _callbacks.DisposeIntellisense();

	public bool IsScriptDefined(string levelName)
		=> _callbacks.IsLevelScriptDefined(levelName);

	public bool IsStringDefined(string value)
	{
		var cachedEditor = _silentActionService.RememberSelectedEditor();
		string languageFilePath = PathHelper.GetLanguageFilePath(_scriptRootDirectoryPath, TRVersion.Game.TombEngine);
		SilentActionFileState languageFileState = _silentActionService.CaptureFileState(languageFilePath);
		bool isDefined = _callbacks.IsLevelLanguageStringDefined(value);
		_silentActionService.Complete(cachedEditor, false, _silentActionService.CreateCompletion(languageFileState, saveAffectedFile: false));
		return isDefined;
	}

	public void RenameLevel(string oldName, string newName)
	{
		var cachedEditor = _silentActionService.RememberSelectedEditor();
		string languageFilePath = PathHelper.GetLanguageFilePath(_scriptRootDirectoryPath, TRVersion.Game.TombEngine);
		SilentActionFileState languageFileState = _silentActionService.CaptureFileState(languageFilePath);
		_callbacks.RenameRequestedLanguageString(oldName, newName);
		_silentActionService.Complete(cachedEditor, true, _silentActionService.CreateCompletion(languageFileState));
	}

	private static bool IsSilentAction(IIDEEvent ideEvent) => ideEvent
		is IDE.ScriptEditor_AppendScriptEvent
		or IDE.ScriptEditor_ScriptPresenceCheckEvent
		or IDE.ScriptEditor_StringPresenceCheckEvent
		or IDE.ScriptEditor_RenameLevelEvent;
}
