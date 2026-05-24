using System;
using System.Windows.Forms;
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

		TabPage cachedTab = _silentActionService.RememberSelectedTab();
		string scriptFilePath = PathHelper.GetScriptFilePath(_scriptRootDirectoryPath, _engine);

		if (ideEvent is IDE.ScriptEditor_ScriptPresenceCheckEvent scriptPresenceEvent)
		{
			SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
			IDE.Instance.ScriptDefined = _callbacks.IsLevelScriptDefined(scriptPresenceEvent.LevelName);
			_silentActionService.Complete(cachedTab, false, _silentActionService.CreateCompletion(scriptFileState, saveAffectedFile: false));
		}
		else if (ideEvent is IDE.ScriptEditor_RenameLevelEvent renameLevelEvent)
		{
			SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
			_callbacks.RenameRequestedLevelScript(renameLevelEvent.OldName, renameLevelEvent.NewName);
			_silentActionService.Complete(cachedTab, true, _silentActionService.CreateCompletion(scriptFileState));
		}
	}

	public void Build()
	{
	}

	public void ShowDocumentation()
	{
	}

	private static bool IsSilentAction(IIDEEvent ideEvent)
		=> ideEvent is IDE.ScriptEditor_ScriptPresenceCheckEvent
		|| ideEvent is IDE.ScriptEditor_RenameLevelEvent;
}