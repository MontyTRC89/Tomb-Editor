using System;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Bases;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.TRX;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.Scripting.Editing;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Documents;
using TombLib.Scripting.TRX.Writers;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Cleaning;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio
{
	public sealed class TRXStudio : TombIDE.ScriptingStudio.Bases.ScriptingStudio
	{
		private TRVersion.Game _engine;
		private readonly TRXDocumentLookupService _documentLookupService = new TRXDocumentLookupService();
		private readonly EditorTabControlTextEditorHost _textEditorHost;
		private readonly ITextFormattingProvider _trimWhitespaceProvider = new TextDocumentFormatterProvider(TrimTrailingWhitespaceFormatter.Instance);
		private readonly TextWorkspaceEditApplier _workspaceEditApplier;
		private readonly TextWorkspaceCommandService _workspaceCommandService;

		#region Construction

		public TRXStudio(ScriptingWorkspaceProfile workspaceProfile) : base()
		{
			ArgumentNullException.ThrowIfNull(workspaceProfile);
			_engine = workspaceProfile.GameVersion;
			_textEditorHost = new EditorTabControlTextEditorHost(EditorTabControl);
			var silentActionService = new StudioSilentActionService(EditorTabControl);
			_workspaceEditApplier = new TextWorkspaceEditApplier(_textEditorHost);
			_workspaceCommandService = new TextWorkspaceCommandService(_workspaceEditApplier);
			var documentCommandHandler = new TrxDocumentCommandHandler(
				new TrxDocumentCommandCallbacks(
					() => CurrentEditor as TRXEditor,
					editor => _workspaceCommandService.FormatDocumentAsync(editor, _trimWhitespaceProvider)));
			var workspaceAutomationProvider = new TrxWorkspaceAutomationProvider(
				silentActionService,
				ScriptRootDirectoryPath,
				_engine,
				new TrxWorkspaceAutomationCallbacks(
					IsLevelScriptDefined,
					RenameRequestedLevelScript));
			InitializeHost(
				workspaceProfile,
				(editor, configs) => editor.UpdateSettings(configs.TRX),
				() => ApplyUserSettingsToOpenEditors(),
				documentCommandHandler: documentCommandHandler,
				workspaceAutomationProvider: workspaceAutomationProvider);
		}

		#endregion Construction

		private void RenameRequestedLevelScript(string oldName, string newName)
		{
			TextEditorBase editor = _textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, _engine));
			ScriptReplacer.RenameLevelScript(editor, oldName, newName);
		}

		private bool IsLevelScriptDefined(string levelName)
		{
			TextEditorBase editor = _textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, _engine));
			return _documentLookupService.IsLevelScriptDefined(editor.Document, levelName);
		}

		#region Other methods

		#endregion Other methods
	}
}
