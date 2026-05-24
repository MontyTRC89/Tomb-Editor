using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Services;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.Scripting.GameFlowScript.Compilers;
using TombLib.Scripting.Specifications.GameFlow;

namespace TombIDE.ScriptingStudio.GameFlowScript;

internal sealed record GameFlowWorkspaceAutomationCallbacks(
	Action<string> AppendScript,
	Func<string, bool> IsLevelScriptDefined,
	Action<string, string> RenameRequestedLevelScript,
	Action SaveAll,
	Action ShowCompilerLogsPane,
	Action<string> UpdateCompilerLogs);

internal sealed class GameFlowWorkspaceAutomationProvider : IStudioWorkspaceAutomationProvider
{
	private readonly GameFlowWorkspaceAutomationCallbacks _callbacks;
	private readonly string _engineDirectoryPath;
	private readonly IWin32Window _promptOwner;
	private readonly string _scriptRootDirectoryPath;
	private readonly StudioSilentActionService _silentActionService;
	private readonly ScriptingWorkspaceProfile _workspaceProfile;

	public GameFlowWorkspaceAutomationProvider(
		IWin32Window promptOwner,
		ScriptingWorkspaceProfile workspaceProfile,
		StudioSilentActionService silentActionService,
		string scriptRootDirectoryPath,
		string engineDirectoryPath,
		GameFlowWorkspaceAutomationCallbacks callbacks)
	{
		_promptOwner = promptOwner ?? throw new ArgumentNullException(nameof(promptOwner));
		_workspaceProfile = workspaceProfile ?? throw new ArgumentNullException(nameof(workspaceProfile));
		_silentActionService = silentActionService ?? throw new ArgumentNullException(nameof(silentActionService));
		_scriptRootDirectoryPath = scriptRootDirectoryPath ?? string.Empty;
		_engineDirectoryPath = engineDirectoryPath ?? string.Empty;
		_callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
	}

	public void HandleIDEEvent(IIDEEvent ideEvent)
	{
		if (ideEvent is null || !IsSilentAction(ideEvent))
			return;

		TabPage cachedTab = _silentActionService.RememberSelectedTab();
		string scriptFilePath = PathHelper.GetScriptFilePath(_scriptRootDirectoryPath, TRVersion.Game.TR2);

		if (ideEvent is IDE.ScriptEditor_AppendScriptEvent appendEvent && appendEvent.Result.HasContent)
		{
			SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
			_callbacks.AppendScript(appendEvent.Result.GameFlowScript);
			_silentActionService.Complete(cachedTab, true, _silentActionService.CreateCompletion(scriptFileState));
		}
		else if (ideEvent is IDE.ScriptEditor_ScriptPresenceCheckEvent scriptPresenceEvent)
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
		_callbacks.SaveAll();

		try
		{
			string engineExecutable = IDE.Instance.Project.GetEngineExecutableFilePath();
			FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(engineExecutable);
			var productVersion = new Version(fileVersionInfo.ProductVersion ?? "0.0");
			bool success;

			if (_workspaceProfile.GameVersion == TRVersion.Game.TR3
				&& productVersion >= new Version(2, 0, 0, 0))
			{
				success = ScriptCompiler.CompileTR3Version2Plus(
					_scriptRootDirectoryPath,
					Path.Combine(_engineDirectoryPath, "data"),
					IDE.Instance.IDEConfiguration.ShowCompilerLogsAfterBuild);
			}
			else
			{
				success = ScriptCompiler.ClassicCompile(
					_scriptRootDirectoryPath,
					Path.Combine(_engineDirectoryPath, "data"),
					_workspaceProfile.GameVersion == TRVersion.Game.TR3,
					IDE.Instance.IDEConfiguration.ShowCompilerLogsAfterBuild);
			}

			_callbacks.UpdateCompilerLogs(success ? "Script compiled successfully!" : "ERROR: Couldn't compile script.");

			if (IDE.Instance.IDEConfiguration.ShowCompilerLogsAfterBuild || !success)
				_callbacks.ShowCompilerLogsPane();
		}
		catch (Exception exception)
		{
			DarkMessageBox.Show(_promptOwner, exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	public void ShowDocumentation()
		=> OpenPathIfExists(GameFlowDocumentationPaths.MainManualPath);

	private static bool IsSilentAction(IIDEEvent ideEvent)
		=> ideEvent is IDE.ScriptEditor_AppendScriptEvent
		|| ideEvent is IDE.ScriptEditor_ScriptPresenceCheckEvent
		|| ideEvent is IDE.ScriptEditor_RenameLevelEvent;

	private static void OpenPathIfExists(string filePath)
	{
		if (!File.Exists(filePath))
			return;

		Process.Start(new ProcessStartInfo
		{
			FileName = filePath,
			UseShellExecute = true
		});
	}
}