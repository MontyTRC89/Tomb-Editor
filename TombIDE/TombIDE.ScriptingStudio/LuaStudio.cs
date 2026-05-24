using DarkUI.Docking;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Bases;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.ScriptingStudio.ToolWindows;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.Editing;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Lua.Documents;
using TombLib.Scripting.UI.Cleaning;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio
{
	public sealed partial class LuaStudio : TombIDE.ScriptingStudio.Bases.ScriptingStudio
	{
		#region Fields

		private readonly TombEngineLevelScriptService _levelScriptService = new();
		private readonly TombEngineLanguageScriptService _languageScriptService = new();
		private readonly EditorTabControlTextEditorHost _textEditorHost;
		private readonly ITextFormattingProvider _trimWhitespaceProvider = new TextDocumentFormatterProvider(TrimTrailingWhitespaceFormatter.Instance);

		#endregion Fields

		#region Construction

		public LuaStudio(ScriptingWorkspaceProfile workspaceProfile) : base()
		{
			ArgumentNullException.ThrowIfNull(workspaceProfile);
			var paneContributionProvider = new StaticStudioPaneContributionProvider(
				new[]
				{
					new StudioPaneContribution(UICommand.LuaDiagnostics, nameof(LuaDiagnostics), CreateLuaDiagnosticsToolWindow),
					new StudioPaneContribution(UICommand.LuaReferencesResults, nameof(LuaReferencesResults), CreateLuaReferencesResultsToolWindow)
				});
			_textEditorHost = new EditorTabControlTextEditorHost(EditorTabControl);
			var silentActionService = new StudioSilentActionService(EditorTabControl);

			_intellisenseProvider = CreateLuaIntellisenseProvider();
			_intellisenseEventBridge = new LuaIntellisenseEventBridge(
				this,
				_intellisenseProvider,
				IntellisenseProvider_DiagnosticsUpdated,
				IntellisenseProvider_SemanticTokensUpdated,
				IntellisenseProvider_StartupFailed,
				IntellisenseProvider_WorkspaceWatcherFailed);
			_referenceSearchService = new LuaReferenceSearchService(_textEditorHost, _intellisenseProvider, ScriptRootDirectoryPath);
			_workspaceEditApplier = new TextWorkspaceEditApplier(_textEditorHost);
			_trackedDocumentStateService = new LuaTrackedDocumentStateService(_textEditorHost, _intellisenseProvider);
			_documentLifecycleCoordinator = new LuaDocumentLifecycleCoordinator(
				EditorTabControl,
				_intellisenseProvider,
				_trackedDocumentStateService,
				() => EditorTabControl_LuaSelectedIndexChanged(this, EventArgs.Empty),
				LuaEditor_StatusChanged,
				LuaEditor_TextChanged,
				NavigateToDefinition,
				LuaEditorOpened,
				CurrentLuaEditorRenamed);
			_workspaceCommandService = new TextWorkspaceCommandService(_workspaceEditApplier, _intellisenseProvider);
			var documentCommandHandler = new LuaDocumentCommandHandler(
				new LuaDocumentCommandCallbacks(
					() => CurrentEditor as LuaEditor,
					ReformatDocumentAsync,
					TrimWhitespaceAsync,
					NavigateBack,
					NavigateForward,
					editor => _ = editor.NavigateToDefinitionAtCaretAsync(),
					FindReferencesAsync,
					RenameSymbolAsync,
					ShowLuaBasicsDocumentation));
			var documentCommandStatusProvider = new LuaDocumentCommandStatusProvider(_intellisenseProvider, _referenceSearchService, _workspaceCommandService);
			_workspaceEditHistory = new TextWorkspaceEditHistoryService(_workspaceEditApplier);
			var workspaceAutomationProvider = new LuaWorkspaceAutomationProvider(
				silentActionService,
				ScriptRootDirectoryPath,
				new LuaWorkspaceAutomationCallbacks(
					AppendScript,
					IsLevelScriptDefined,
					IsLevelLanguageStringDefined,
					RenameRequestedLanguageString,
					DisposeLuaIntellisense));
			HookLuaIntellisense();
			InitializeHost(
				workspaceProfile,
				(editor, configs) => editor.UpdateSettings(configs.Lua),
				() => ApplyUserSettingsToOpenEditors(afterApply: editor =>
				{
					if (editor is LuaEditor luaEditor)
						_trackedDocumentStateService.ApplyTrackedState(luaEditor);
				}),
				documentCommandStatusProvider: documentCommandStatusProvider,
				documentCommandHandler: documentCommandHandler,
				paneContributionProvider: paneContributionProvider,
				workspaceAutomationProvider: workspaceAutomationProvider,
				dockPanelLayoutRestored: EnsureLuaToolWindowsInDockPanel);
		}

		#endregion Construction

		private (bool ScriptUpdated, bool LanguageUpdated) AppendScript(ScriptGenerationResult result)
		{
			bool scriptUpdated = false;
			bool languageUpdated = false;

			try
			{
				if (result.GameFlowScript.Length > 0)
				{
					AppendGameFlowScript(result.GameFlowScript);
					scriptUpdated = true;
				}

				if (result.LanguageScript.Length > 0)
				{
					AppendLanguageScript(result.LanguageScript);
					languageUpdated = true;
				}

				CreateGeneratedFiles(result.FilesToCreate);
			}
			catch (Exception exception)
			{
				Debug.WriteLine($"[LuaStudio] Failed to append generated Lua script output: {exception}");
			}

			return (scriptUpdated, languageUpdated);
		}

		private void AppendGameFlowScript(string scriptText)
		{
			TextEditorBase editor = _textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine));
			editor.AppendText(Environment.NewLine + scriptText + Environment.NewLine);
			editor.ScrollToLine(editor.LineCount);
		}

		private void AppendLanguageScript(string languageScript)
		{
			if (_textEditorHost.OpenTextEditor(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine)) is TextEditorBase stringsEditor)
			{
				int? insertedLineNumber = _languageScriptService.TryInsertLanguageScript(stringsEditor.Document, languageScript);

				if (insertedLineNumber is not null)
				{
					stringsEditor.ResetSelectionAt(insertedLineNumber.Value);
					stringsEditor.ScrollToLine(insertedLineNumber.Value);
				}
			}
		}

		private void CreateGeneratedFiles(IReadOnlyList<GeneratedScriptFile> files)
		{
			string scriptRootPath = Path.GetFullPath(ScriptRootDirectoryPath);

			if (!scriptRootPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
				scriptRootPath += Path.DirectorySeparatorChar;

			foreach (GeneratedScriptFile file in files)
			{
				string filePath = Path.GetFullPath(Path.Combine(scriptRootPath, file.RelativePath));

				if (!filePath.StartsWith(scriptRootPath, StringComparison.OrdinalIgnoreCase))
					continue;

				string directory = Path.GetDirectoryName(filePath);

				if (directory is not null && !Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				File.WriteAllText(filePath, file.Content);
			}
		}

		private bool IsLevelLanguageStringDefined(string levelName)
		{
			TextEditorBase editor = _textEditorHost.OpenTextEditor(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine));
			var regex = new Regex($"\"{Regex.Escape(levelName)}\"");
			var stringLine = editor.Document.Lines.FirstOrDefault(line => regex.IsMatch(editor.Document.GetText(line)));

			return stringLine is not null;
		}

		private bool IsLevelScriptDefined(string levelName)
		{
			TextDocument scriptDocument = _textEditorHost.TryGetTextDocument(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine));
			TextDocument languageDocument = _textEditorHost.TryGetTextDocument(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine));

			if (scriptDocument is null || languageDocument is null)
				return false;

			return _levelScriptService.IsLevelScriptDefined(scriptDocument, languageDocument, levelName);
		}

		private void RenameRequestedLanguageString(string oldName, string newName)
		{
			if (_textEditorHost.OpenTextEditor(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine)) is TextEditorBase editor)
			{
				var regex = new Regex($"\"{Regex.Escape(oldName)}\"");
				var stringLine = editor.Document.Lines.FirstOrDefault(line => regex.IsMatch(editor.Document.GetText(line)));

				if (stringLine is not null)
				{
					string lineText = editor.Document.GetText(stringLine);
					editor.ReplaceLine(stringLine, regex.Replace(lineText, $"\"{newName}\""));
					editor.ScrollToLine(stringLine.LineNumber);
				}
			}
		}

		#region Other methods

		private void EnsureLuaToolWindowsInDockPanel()
		{
			if (DockPanel is null)
				return;

			DarkDockGroup bottomGroup = SearchResults?.DockGroup ?? CompilerLogs?.DockGroup;

			bottomGroup = EnsureLuaToolWindowInDockPanel(LuaDiagnostics, bottomGroup);
			EnsureLuaToolWindowInDockPanel(LuaReferencesResults, bottomGroup);
		}

		private DarkDockGroup EnsureLuaToolWindowInDockPanel(DarkToolWindow toolWindow, DarkDockGroup bottomGroup)
		{
			if (DockPanel.ContainsContent(toolWindow))
				return bottomGroup ?? toolWindow.DockGroup;

			toolWindow.DockArea = DarkDockArea.Bottom;

			if (bottomGroup is not null)
				DockPanel.AddContent(toolWindow, bottomGroup);
			else
				DockPanel.AddContent(toolWindow);

			return bottomGroup ?? toolWindow.DockGroup;
		}

		private static void ShowLuaBasicsDocumentation()
		{
			const string url = "https://github.com/MontyTRC89/TombEngine/wiki/Basics-of-Lua-Programming";

			var process = new ProcessStartInfo
			{
				FileName = url,
				UseShellExecute = true
			};

			Process.Start(process);
		}

		#endregion Other methods
	}
}
