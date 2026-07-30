using System;
using TombIDE.ScriptingStudio.Services;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;

namespace TombIDE.ScriptingStudio.TRX;

internal sealed record TrxWorkspaceAutomationCallbacks(
	Func<string, bool> IsLevelScriptDefined,
	Action<string, string> RenameRequestedLevelScript);

internal sealed class TrxWorkspaceAutomationProvider : IStudioWorkspaceAutomationProvider
{
	private readonly TrxWorkspaceAutomationCallbacks _callbacks;
	private readonly TRVersion.Game _engine;
	private readonly string _scriptRootDirectoryPath;
	private readonly StudioSilentActionService _silentActionService;

	public TrxWorkspaceAutomationProvider(
		StudioSilentActionService silentActionService,
		string scriptRootDirectoryPath,
		TRVersion.Game engine,
		TrxWorkspaceAutomationCallbacks callbacks)
	{
		_silentActionService = silentActionService ?? throw new ArgumentNullException(nameof(silentActionService));
		_scriptRootDirectoryPath = scriptRootDirectoryPath ?? string.Empty;
		_engine = engine;
		_callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
	}

	public void HandleIDEEvent(IIDEEvent ideEvent)
	{
		if (ideEvent is null || !IsSilentAction(ideEvent))
			return;

		switch (ideEvent)
		{
			case IDE.ScriptEditor_ScriptPresenceCheckEvent scriptPresenceEvent:
				IDE.Instance.ScriptDefined = IsScriptDefined(scriptPresenceEvent.LevelName);
				break;

			case IDE.ScriptEditor_RenameLevelEvent renameLevelEvent:
				RenameLevel(renameLevelEvent.OldName, renameLevelEvent.NewName);
				break;
		}
	}

	public bool IsScriptDefined(string levelName)
	{
		var cachedEditor = _silentActionService.RememberSelectedEditor();
		string scriptFilePath = PathHelper.GetScriptFilePath(_scriptRootDirectoryPath, _engine);
		SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
		bool isDefined = _callbacks.IsLevelScriptDefined(levelName);
		_silentActionService.Complete(cachedEditor, false, _silentActionService.CreateCompletion(scriptFileState, saveAffectedFile: false));
		return isDefined;
	}

	public void RenameLevel(string oldName, string newName)
	{
		var cachedEditor = _silentActionService.RememberSelectedEditor();
		string scriptFilePath = PathHelper.GetScriptFilePath(_scriptRootDirectoryPath, _engine);
		SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
		_callbacks.RenameRequestedLevelScript(oldName, newName);
		_silentActionService.Complete(cachedEditor, true, _silentActionService.CreateCompletion(scriptFileState));
	}

	private static bool IsSilentAction(IIDEEvent ideEvent) => ideEvent
		is IDE.ScriptEditor_ScriptPresenceCheckEvent
		or IDE.ScriptEditor_RenameLevelEvent;
}
