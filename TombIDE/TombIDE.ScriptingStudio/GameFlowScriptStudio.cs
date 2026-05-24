using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Bases;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.GameFlowScript;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.Editing;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Compilers;
using TombLib.Scripting.GameFlowScript.Documents;
using TombLib.Scripting.GameFlowScript.Writers;
using TombLib.Scripting.Specifications.GameFlow;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Cleaning;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio
{
	public sealed class GameFlowScriptStudio : TombIDE.ScriptingStudio.Bases.ScriptingStudio
	{
		private readonly GameFlowDocumentLookupService _documentLookupService = new();
		private readonly EditorTabControlTextEditorHost _textEditorHost;
		private readonly ITextFormattingProvider _trimWhitespaceProvider = new TextDocumentFormatterProvider(TrimTrailingWhitespaceFormatter.Instance);
		private readonly TextWorkspaceEditApplier _workspaceEditApplier;
		private readonly TextWorkspaceCommandService _workspaceCommandService;

		#region Construction

		public GameFlowScriptStudio(ScriptingWorkspaceProfile workspaceProfile) : base()
		{
			ArgumentNullException.ThrowIfNull(workspaceProfile);
			_textEditorHost = new EditorTabControlTextEditorHost(EditorTabControl);
			var silentActionService = new StudioSilentActionService(EditorTabControl);
			_workspaceEditApplier = new TextWorkspaceEditApplier(_textEditorHost);
			_workspaceCommandService = new TextWorkspaceCommandService(_workspaceEditApplier);
			var documentCommandHandler = new GameFlowDocumentCommandHandler(
				new GameFlowDocumentCommandCallbacks(
					() => CurrentEditor as GameFlowEditor,
					editor => _workspaceCommandService.FormatDocumentAsync(editor, _trimWhitespaceProvider),
					ShowExtraCommandsDocumentation));
			var workspaceAutomationProvider = new GameFlowWorkspaceAutomationProvider(
				this,
				workspaceProfile,
				silentActionService,
				ScriptRootDirectoryPath,
				EngineDirectoryPath,
				new GameFlowWorkspaceAutomationCallbacks(
					AppendScript,
					IsLevelScriptDefined,
					RenameRequestedLevelScript,
					EditorTabControl.SaveAll,
					() => ShowPane(UICommand.CompilerLogs),
					CompilerLogs.UpdateLogs));
			InitializeHost(
				workspaceProfile,
				(editor, configs) => editor.UpdateSettings(configs.GameFlowScript),
				() => ApplyUserSettingsToOpenEditors(),
				documentCommandHandler: documentCommandHandler,
				workspaceAutomationProvider: workspaceAutomationProvider);
		}

		#endregion Construction

		private void AppendScript(string scriptText)
		{
			TextEditorBase editor = _textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR2));
			editor.AppendText(Environment.NewLine + scriptText + Environment.NewLine);
			editor.ScrollToLine(editor.LineCount);
		}

		private void RenameRequestedLevelScript(string oldName, string newName)
		{
			TextEditorBase editor = _textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR2));
			ScriptReplacer.RenameLevelScript(editor, oldName, newName);
		}

		private bool IsLevelScriptDefined(string levelName)
		{
			TextEditorBase editor = _textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR2));
			return _documentLookupService.IsLevelScriptDefined(editor.Document, levelName);
		}

		#region Other methods

		private static void ShowExtraCommandsDocumentation()
		{
			string pdfPath = GameFlowDocumentationPaths.ExtraCommandsManualPath;

			var process = new ProcessStartInfo
			{
				FileName = pdfPath,
				UseShellExecute = true
			};

			if (File.Exists(pdfPath))
				Process.Start(process);
		}

		#endregion Other methods
	}
}
