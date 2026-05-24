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
using TombLib.Scripting.ClassicScript.Compilers;

namespace TombIDE.ScriptingStudio.ClassicScript;

internal sealed record ClassicScriptWorkspaceAutomationCallbacks(
	Action<string> AppendScript,
	Action<string> AddNewLevelNameString,
	Func<string, bool> AddNewPluginEntry,
	Func<string, bool> AddNewNGString,
	Func<string, bool> IsLevelScriptDefined,
	Func<string, bool> IsLevelLanguageStringDefined,
	Action<string, string> RenameRequestedLevelScript,
	Action<string, string> RenameRequestedLanguageString,
	Action ApplyUserSettings,
	Action SaveAll,
	Action ShowCompilerLogsPane,
	Action<string> UpdateCompilerLogs);

internal sealed class ClassicScriptWorkspaceAutomationProvider : IStudioWorkspaceAutomationProvider
{
	private readonly ClassicScriptWorkspaceAutomationCallbacks _callbacks;
	private readonly string _engineDirectoryPath;
	private readonly IWin32Window _promptOwner;
	private readonly string _scriptRootDirectoryPath;
	private readonly StudioSilentActionService _silentActionService;
	private readonly ScriptingWorkspaceProfile _workspaceProfile;

	public ClassicScriptWorkspaceAutomationProvider(
		IWin32Window promptOwner,
		ScriptingWorkspaceProfile workspaceProfile,
		StudioSilentActionService silentActionService,
		string scriptRootDirectoryPath,
		string engineDirectoryPath,
		ClassicScriptWorkspaceAutomationCallbacks callbacks)
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
		if (ideEvent is null)
			return;

		if (IsSilentAction(ideEvent))
		{
			HandleSilentAction(ideEvent);
			return;
		}

		if (ideEvent is IDE.ScriptEditor_ReloadSyntaxHighlightingEvent)
			_callbacks.ApplyUserSettings();
	}

	public void Build()
	{
		_callbacks.SaveAll();

		if (_workspaceProfile.GameVersion == TRVersion.Game.TR4)
			CompileTR4Script();
		else if (_workspaceProfile.GameVersion == TRVersion.Game.TRNG)
			CompileTRNGScript();
	}

	public void ShowDocumentation()
	{
		string pdfPath = Path.Combine(DefaultPaths.ResourcesDirectory, "ClassicScript", "TRNG Script Reference Manual.pdf");
		OpenPathIfExists(pdfPath);
	}

	private static bool IsSilentAction(IIDEEvent ideEvent)
		=> ideEvent is IDE.ScriptEditor_AppendScriptEvent
		|| ideEvent is IDE.ScriptEditor_AddNewLevelStringEvent
		|| ideEvent is IDE.ScriptEditor_AddNewPluginEntryEvent
		|| ideEvent is IDE.ScriptEditor_AddNewNGStringEvent
		|| ideEvent is IDE.ScriptEditor_ScriptPresenceCheckEvent
		|| ideEvent is IDE.ScriptEditor_StringPresenceCheckEvent
		|| ideEvent is IDE.ScriptEditor_RenameLevelEvent;

	private void HandleSilentAction(IIDEEvent ideEvent)
	{
		TabPage cachedTab = _silentActionService.RememberSelectedTab();
		string scriptFilePath = PathHelper.GetScriptFilePath(_scriptRootDirectoryPath, TRVersion.Game.TR4);
		string languageFilePath = PathHelper.GetLanguageFilePath(_scriptRootDirectoryPath, TRVersion.Game.TR4);
		string ngLanguageFilePath = PathHelper.GetLanguageFilePath(_scriptRootDirectoryPath, TRVersion.Game.TRNG);

		if (ideEvent is IDE.ScriptEditor_AppendScriptEvent appendEvent && appendEvent.Result.HasContent)
		{
			SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
			_callbacks.AppendScript(appendEvent.Result.GameFlowScript);
			_silentActionService.Complete(cachedTab, true, _silentActionService.CreateCompletion(scriptFileState));
		}
		else if (ideEvent is IDE.ScriptEditor_AddNewLevelStringEvent addLevelStringEvent)
		{
			SilentActionFileState languageFileState = _silentActionService.CaptureSourceFileState(languageFilePath);
			_callbacks.AddNewLevelNameString(addLevelStringEvent.LevelName);
			_silentActionService.Complete(cachedTab, true, _silentActionService.CreateCompletion(languageFileState));
		}
		else if (ideEvent is IDE.ScriptEditor_AddNewPluginEntryEvent addPluginEntryEvent)
		{
			SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
			bool isChanged = _callbacks.AddNewPluginEntry(addPluginEntryEvent.PluginString);
			_silentActionService.Complete(cachedTab, isChanged, _silentActionService.CreateCompletion(scriptFileState));
		}
		else if (ideEvent is IDE.ScriptEditor_AddNewNGStringEvent addNgStringEvent)
		{
			SilentActionFileState ngLanguageFileState = _silentActionService.CaptureSourceFileState(ngLanguageFilePath);
			bool isChanged = _callbacks.AddNewNGString(addNgStringEvent.NGString);
			_silentActionService.Complete(cachedTab, isChanged, _silentActionService.CreateCompletion(ngLanguageFileState));
		}
		else if (ideEvent is IDE.ScriptEditor_ScriptPresenceCheckEvent scriptPresenceEvent)
		{
			SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
			IDE.Instance.ScriptDefined = _callbacks.IsLevelScriptDefined(scriptPresenceEvent.LevelName);
			_silentActionService.Complete(cachedTab, false, _silentActionService.CreateCompletion(scriptFileState, saveAffectedFile: false));
		}
		else if (ideEvent is IDE.ScriptEditor_StringPresenceCheckEvent stringPresenceEvent)
		{
			SilentActionFileState languageFileState = _silentActionService.CaptureSourceFileState(languageFilePath);
			IDE.Instance.StringDefined = _callbacks.IsLevelLanguageStringDefined(stringPresenceEvent.String);
			_silentActionService.Complete(cachedTab, false, _silentActionService.CreateCompletion(languageFileState, saveAffectedFile: false));
		}
		else if (ideEvent is IDE.ScriptEditor_RenameLevelEvent renameLevelEvent)
		{
			SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
			SilentActionFileState languageFileState = _silentActionService.CaptureSourceFileState(languageFilePath);
			_callbacks.RenameRequestedLevelScript(renameLevelEvent.OldName, renameLevelEvent.NewName);
			_callbacks.RenameRequestedLanguageString(renameLevelEvent.OldName, renameLevelEvent.NewName);
			_silentActionService.Complete(
				cachedTab,
				true,
				_silentActionService.CreateCompletion(scriptFileState),
				_silentActionService.CreateCompletion(languageFileState));
		}
	}

	private void CompileTR4Script()
	{
		try
		{
			string logs = TR4Compiler.Compile(_scriptRootDirectoryPath, _engineDirectoryPath);

			if (IDE.Instance.IDEConfiguration.ShowCompilerLogsAfterBuild)
				_callbacks.ShowCompilerLogsPane();

			_callbacks.UpdateCompilerLogs(logs);
		}
		catch (Exception exception)
		{
			ShowError(exception.Message);
		}
	}

	private void CompileTRNGScript()
	{
		try
		{
			bool success = NGCompiler.Compile(
				_scriptRootDirectoryPath,
				_engineDirectoryPath,
				IDE.Instance.IDEConfiguration.UseNewIncludeMethod);

			string logFilePath = Path.Combine(DefaultPaths.VGEDirectory, "LastCompilerLog.txt");
			_callbacks.UpdateCompilerLogs(File.ReadAllText(logFilePath));

			if (!success)
				ShowError("Script compilation yielded an error. Please check the logs.");

			if (IDE.Instance.IDEConfiguration.ShowCompilerLogsAfterBuild || !success)
				_callbacks.ShowCompilerLogsPane();
		}
		catch (Exception exception)
		{
			ShowError(exception.Message);
		}
	}

	private void ShowError(string message)
		=> DarkMessageBox.Show(_promptOwner, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

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