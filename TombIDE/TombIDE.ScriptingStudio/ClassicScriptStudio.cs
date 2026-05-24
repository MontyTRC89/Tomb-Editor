using DarkUI.Docking;
using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Bases;
using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.Editing;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Compilers;
using TombLib.Scripting.ClassicScript.Documents;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Writers;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Cleaning;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Strings;

namespace TombIDE.ScriptingStudio
{
	public sealed class ClassicScriptStudio : TombIDE.ScriptingStudio.Bases.ScriptingStudio
	{
		#region Fields

		private FormReferenceInfo FormReferenceInfo = new FormReferenceInfo();
		private readonly ClassicScriptDocumentLookupService _documentLookupService = new();
		private readonly ClassicScriptReferenceDefinitionService _referenceDefinitionService = new();
		private readonly ClassicScriptReferenceInfoService _referenceInfoService = new();
		private readonly EditorTabControlTextEditorHost _textEditorHost;
		private readonly ITextFormattingProvider _trimWhitespaceProvider = new TextDocumentFormatterProvider(TrimTrailingWhitespaceFormatter.Instance);
		private readonly TextWorkspaceEditApplier _workspaceEditApplier;
		private readonly TextWorkspaceCommandService _workspaceCommandService;

		#endregion Fields

		#region Construction

		public ClassicScriptStudio(ScriptingWorkspaceProfile workspaceProfile) : base()
		{
			ArgumentNullException.ThrowIfNull(workspaceProfile);
			var paneContributionProvider = new StaticStudioPaneContributionProvider(
				new[]
				{
					new StudioPaneContribution(UICommand.ReferenceBrowser, nameof(ReferenceBrowser), () => new ReferenceBrowser())
				});
			_textEditorHost = new EditorTabControlTextEditorHost(EditorTabControl);
			var silentActionService = new StudioSilentActionService(EditorTabControl);
			_workspaceEditApplier = new TextWorkspaceEditApplier(_textEditorHost);
			_workspaceCommandService = new TextWorkspaceCommandService(_workspaceEditApplier);
			var documentCommandHandler = new ClassicScriptDocumentCommandHandler(
				new ClassicScriptDocumentCommandCallbacks(
					() => CurrentEditor as ClassicScriptEditor,
					editor => _workspaceCommandService.FormatDocumentAsync(editor, new TextDocumentFormatterProvider(editor.Formatter)),
					editor => _workspaceCommandService.FormatDocumentAsync(editor, _trimWhitespaceProvider),
					editor => editor.InputFreeIndex(),
					CreateNewFileAtCaretPosition));
			var workspaceAutomationProvider = new ClassicScriptWorkspaceAutomationProvider(
				this,
				workspaceProfile,
				silentActionService,
				ScriptRootDirectoryPath,
				EngineDirectoryPath,
				new ClassicScriptWorkspaceAutomationCallbacks(
					AppendScript,
					AddNewLevelNameString,
					AddNewPluginEntry,
					AddNewNGString,
					IsLevelScriptDefined,
					IsLevelLanguageStringDefined,
					RenameRequestedLevelScript,
					RenameRequestedLanguageString,
					ApplyUserSettings,
					EditorTabControl.SaveAll,
					() => ShowPane(UICommand.CompilerLogs),
					CompilerLogs.UpdateLogs));
			InitializeHost(
				workspaceProfile,
				(editor, configs) => editor.UpdateSettings(configs.ClassicScript),
				() => ApplyUserSettingsToOpenEditors(afterAll: () => StatusStrip.ReloadContributionSettings()),
				documentCommandStatusProvider: new ClassicScriptDocumentCommandStatusProvider(),
				documentCommandHandler: documentCommandHandler,
				documentStatusStripProvider: new ClassicScriptDocumentStatusStripProvider(),
				paneContributionProvider: paneContributionProvider,
				workspaceAutomationProvider: workspaceAutomationProvider);

			EditorTabControl.FileOpened += EditorTabControl_FileOpened;

			ReferenceBrowser.ReferenceDefinitionRequested += ReferenceBrowser_ReferenceDefinitionRequested;
		}

		#endregion Construction

		private void AppendScript(string scriptText)
		{
			TextEditorBase editor = _textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR4));
			editor.AppendText(Environment.NewLine + scriptText + Environment.NewLine);
			editor.ScrollToLine(editor.LineCount);
		}

		private void AddNewLevelNameString(string levelName)
		{
			TextEditorBase editor = _textEditorHost.OpenTextEditor(
				PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR4),
				openSourceView: true);
			LanguageStringWriter.WriteNewLevelNameString(editor, levelName);
		}

		private bool AddNewPluginEntry(string pluginString)
		{
			if (_textEditorHost.OpenEditor<ClassicScriptEditor>(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR4)) is ClassicScriptEditor editor)
				return editor.TryAddNewPluginEntry(pluginString);

			return false;
		}

		private bool AddNewNGString(string ngString)
		{
			if (_textEditorHost.OpenTextEditor(
				PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TRNG),
				openSourceView: true) is TextEditorBase editor)
				return LanguageStringWriter.WriteNewNGString(editor, ngString);

			return false;
		}

		private void RenameRequestedLevelScript(string oldName, string newName)
		{
			TextEditorBase editor = _textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR4));
			ScriptReplacer.RenameLevelScript(editor, oldName, newName);
		}

		private void RenameRequestedLanguageString(string oldName, string newName)
		{
			TextEditorBase editor = _textEditorHost.OpenTextEditor(
				PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR4),
				openSourceView: true);
			ScriptReplacer.RenameLanguageString(editor, oldName, newName);
		}

		private bool IsLevelScriptDefined(string levelName)
		{
			TextEditorBase editor = _textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR4));
			return _documentLookupService.IsLevelScriptDefined(editor.Document, levelName);
		}

		private bool IsLevelLanguageStringDefined(string levelName)
		{
			TextEditorBase editor = _textEditorHost.OpenTextEditor(
				PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR4),
				openSourceView: true);
			return _documentLookupService.IsLevelLanguageStringDefined(editor.Document, levelName);
		}

		#region Events

		private void EditorTabControl_FileOpened(object sender, EventArgs e)
		{
			if (sender is ClassicScriptEditor textEditor)
			{
				textEditor.MouseDoubleClick += TextEditor_MouseDoubleClick;
				textEditor.KeyDown += TextEditor_KeyDown;
				textEditor.WordDefinitionRequested += TextEditor_WordDefinitionRequested;
			}
		}

		private void TextEditor_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (CurrentEditor is ClassicScriptEditor editor)
			{
				if (e.ChangedButton == System.Windows.Input.MouseButton.Left && ModifierKeys == Keys.Control)
					OpenIncludeFile(editor);
			}
		}

		private void TextEditor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (CurrentEditor is ClassicScriptEditor editor)
			{
				if (e.Key == System.Windows.Input.Key.F5 && ModifierKeys != Keys.Control)
					OpenIncludeFile(editor);
			}
		}

		private void TextEditor_WordDefinitionRequested(object sender, WordDefinitionEventArgs e)
		{
			if (sender is not ClassicScriptEditor editor)
				return;

			int offset = e.HoveredOffset != -1 ? e.HoveredOffset : editor.CaretOffset;
			ClassicScriptReferenceDefinition definition = _referenceDefinitionService.ResolveReference(editor.Document, e.Word, e.Type, offset);
			ClassicScriptReferenceInfo referenceInfo = _referenceInfoService.GetReferenceInfo(definition.Keyword, definition.Type);

			FormReferenceInfo.Show(referenceInfo);
		}

		private void ReferenceBrowser_ReferenceDefinitionRequested(object sender, ReferenceDefinitionEventArgs e)
			=> FormReferenceInfo.Show(_referenceInfoService.GetReferenceInfo(e.Keyword, e.Type));

		#endregion Events

		#region Other methods

		private void CreateNewFileAtCaretPosition(ClassicScriptEditor editor)
		{
			string filePath = FileExplorer.CreateNewFile();

			if (filePath != null)
			{
				editor.SuppressAutocomplete = true;

				string includeValue = filePath.Replace(Path.GetDirectoryName(editor.FilePath), string.Empty).TrimStart('\\');
				editor.TextArea.PerformTextInput($"{Environment.NewLine}#INCLUDE \"{includeValue}\"");

				editor.SuppressAutocomplete = false;
			}
		}

		private void OpenIncludeFile(ClassicScriptEditor editor)
		{
			string fullFilePath = _documentLookupService.TryGetIncludeFilePath(editor.Document, editor.CaretOffset);

			if (!string.IsNullOrWhiteSpace(fullFilePath) && File.Exists(fullFilePath))
				EditorTabControl.OpenFile(fullFilePath);
			else
				DarkMessageBox.Show(this, "Couldn't find the target file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

			editor.SelectionLength = 0;
		}

		private void CompileTR4Script()
		{
			try
			{
				string logs = TR4Compiler.Compile(ScriptRootDirectoryPath, EngineDirectoryPath);

				if (IDE.Instance.IDEConfiguration.ShowCompilerLogsAfterBuild)
					ShowPane(UICommand.CompilerLogs);

				CompilerLogs.UpdateLogs(logs);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void CompileTRNGScript()
		{
			try
			{
				bool success = NGCompiler.Compile(
					ScriptRootDirectoryPath, EngineDirectoryPath,
					IDE.Instance.IDEConfiguration.UseNewIncludeMethod);

				string logFilePath = Path.Combine(DefaultPaths.VGEDirectory, "LastCompilerLog.txt");
				CompilerLogs.UpdateLogs(File.ReadAllText(logFilePath));

				if (!success)
					DarkMessageBox.Show(this, "Script compilation yielded an error. Please check the logs.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				if (IDE.Instance.IDEConfiguration.ShowCompilerLogsAfterBuild || !success)
					ShowPane(UICommand.CompilerLogs);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private ReferenceBrowser ReferenceBrowser
			=> GetPaneContent<ReferenceBrowser>(UICommand.ReferenceBrowser);

		#endregion Other methods
	}
}
