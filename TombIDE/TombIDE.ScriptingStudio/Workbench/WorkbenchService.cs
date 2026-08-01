#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Build;
using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Diagnostics;
using TombIDE.ScriptingStudio.DocumentOutline;
using TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.Properties;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.Shortcuts;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared;
using TombIDE.Shared.Messaging.Scripting;
using TombLib.Forms;
using Nickelony.LanguageServer.Abstractions;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;
using Nickelony.LanguageServer.Abstractions.Navigation;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Text;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class WorkbenchService : IWorkbenchService
{
	private static readonly IStudioDocumentStatusStripProvider ClassicScriptStatusStripProvider = new ClassicScriptDocumentStatusStripProvider();

	private readonly IMenuService _menuService;
	private readonly IToolBarService _toolBarService;
	private readonly IStatusBarService _statusBarService;
	private readonly IPaneHostService _paneHostService;
	private readonly IEditorDocumentController _documentController;
	private readonly IAvalonDockHost _dockHost;
	private readonly PaneCatalog _paneCatalog;
	private readonly IEditorLifecycleService _editorLifecycleService;
	private readonly ILuaEditorLifecycleService _luaEditorLifecycleService;
	private readonly ILuaIntellisenseBridge _luaIntellisenseBridge;
	private readonly LuaTrackedDocumentStateService _luaTrackedDocumentStateService;
	private readonly IMessageService _messageService;
	private readonly ScriptingMessageService _scriptingMessageService;
	private readonly ScriptingWorkspaceProfile _workspaceProfile;
	private readonly ClassicScriptLanguageServices _languageServices;
	private readonly FindAndReplaceViewModel _findAndReplaceViewModel;
	private readonly FindAndReplaceView _findAndReplaceView;
	private readonly IWin32DialogOwnerProvider _dialogOwnerProvider;
	private readonly TRXLanguageServices _trxLanguageServices;
	private Action? _buildAction;
	private Action? _showDocumentationAction;

	public WorkbenchService(
		ScriptingWorkspaceProfile workspaceProfile,
		IScriptingProjectContext projectContext,
		IMessenger messenger,
		IMessageService messageService,
		IShortcutBindingService shortcutBindingService,
		IMenuService menuService,
		IToolBarService toolBarService,
		IStatusBarService statusBarService,
		IPaneHostService paneHostService,
		IWin32DialogOwnerProvider dialogOwnerProvider,
		IEditorDocumentController documentController,
		IAvalonDockHost dockHost,
		PaneCatalog paneCatalog,
		FindAndReplaceViewModel findAndReplaceViewModel,
		ILuaEditorLifecycleService luaEditorLifecycleService,
		ILuaIntellisenseBridge luaIntellisenseBridge,
		LuaTrackedDocumentStateService luaTrackedDocumentStateService,
		Func<bool> showCompilerLogsAfterBuild,
		Func<bool> useNewIncludeMethod,
		ClassicScriptLanguageServices languageServices,
		GameFlowLanguageServices gameFlowLanguageServices,
		TRXLanguageServices trxLanguageServices)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);
		ArgumentNullException.ThrowIfNull(projectContext);
		ArgumentNullException.ThrowIfNull(messenger);
		ArgumentNullException.ThrowIfNull(messageService);
		ArgumentNullException.ThrowIfNull(shortcutBindingService);
		ArgumentNullException.ThrowIfNull(menuService);
		ArgumentNullException.ThrowIfNull(toolBarService);
		ArgumentNullException.ThrowIfNull(statusBarService);
		ArgumentNullException.ThrowIfNull(paneHostService);
		ArgumentNullException.ThrowIfNull(dialogOwnerProvider);
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(dockHost);
		ArgumentNullException.ThrowIfNull(paneCatalog);
		ArgumentNullException.ThrowIfNull(findAndReplaceViewModel);
		ArgumentNullException.ThrowIfNull(luaEditorLifecycleService);
		ArgumentNullException.ThrowIfNull(luaIntellisenseBridge);
		ArgumentNullException.ThrowIfNull(luaTrackedDocumentStateService);
		ArgumentNullException.ThrowIfNull(showCompilerLogsAfterBuild);
		ArgumentNullException.ThrowIfNull(useNewIncludeMethod);
		ArgumentNullException.ThrowIfNull(languageServices);
		ArgumentNullException.ThrowIfNull(gameFlowLanguageServices);
		ArgumentNullException.ThrowIfNull(trxLanguageServices);

		_workspaceProfile = workspaceProfile;
		_menuService = menuService;
		_toolBarService = toolBarService;
		_statusBarService = statusBarService;
		_paneHostService = paneHostService;
		_messageService = messageService;
		_documentController = documentController;
		_dockHost = dockHost;
		_paneCatalog = paneCatalog;
		_dialogOwnerProvider = dialogOwnerProvider;
		_luaEditorLifecycleService = luaEditorLifecycleService;
		_luaIntellisenseBridge = luaIntellisenseBridge;
		_luaTrackedDocumentStateService = luaTrackedDocumentStateService;
		_languageServices = languageServices;
		_trxLanguageServices = trxLanguageServices;

		_findAndReplaceViewModel = findAndReplaceViewModel;
		_findAndReplaceView = new FindAndReplaceView(findAndReplaceViewModel);

		messenger.Register<FindAllPerformedMessage>(this, (_, m) => HandleFindAllPerformed(m.Value));

		_dockHost.RestoreLayout(_workspaceProfile.LoadAvalonDockLayoutXml(), _workspaceProfile.LoadDockPanelState(), UpdateUi);
		_paneCatalog.EnsurePanesRegistered(_dockHost);
		_paneCatalog.UpdatePaneVisibilityChecks(_dockHost, _paneHostService);

		_scriptingMessageService = new ScriptingMessageService(
			messenger,
			_workspaceProfile,
			_documentController,
			projectContext.Project.GetEngineRootDirectoryPath(),
			projectContext.Project.GetEngineExecutableFilePath(),
			new ScriptingMessageServiceOptions
			{
				GetDockLayoutXml = () => _dockHost.SaveLayout(),
				ShowCompilerLogsPane = ShowCompilerLogsPane,
				UpdateCompilerLogs = UpdateCompilerLogs,
				ShowCompilerLogsAfterBuild = showCompilerLogsAfterBuild,
				UseNewIncludeMethod = useNewIncludeMethod
			},
			languageServices,
			gameFlowLanguageServices,
			_trxLanguageServices);

		_buildAction = _scriptingMessageService.Build;
		_showDocumentationAction = _scriptingMessageService.ShowDocumentation;

		_editorLifecycleService = new StudioEditorLifecycleCoordinator(
			_documentController,
			messenger,
			ApplySettingsToEditor,
			ExecuteCommand,
			CanExecuteCommand,
			shortcutBindingService);

		messenger.Register<ShellUiRefreshMessage>(this, (_, _) => UpdateUi());
		messenger.Register<CommandStateRefreshMessage>(this, (_, _) => UpdateCommandStates());
		messenger.Register<LuaDefinitionNavigationMessage>(this, (_, m) => NavigateToLuaDefinition(m.Value));
		messenger.Register<LuaDiagnosticsUpdatedMessage>(this, (_, m) => HandleLuaDiagnosticsUpdated(m.Value));
		messenger.Register<LuaSemanticTokensUpdatedMessage>(this, (_, m) => HandleLuaSemanticTokensUpdated(m.Value));
		messenger.Register<LuaStartupFailedMessage>(this, (_, m) => HandleLuaStartupFailed(m.Value));
		messenger.Register<LuaWorkspaceWatcherFailedMessage>(this, (_, m) => HandleLuaWorkspaceWatcherFailed(m.Value));

		_documentController.CurrentEditorChanged += DocumentController_CurrentEditorChanged;
		_documentController.EditorClosed += DocumentController_EditorClosed;
		_documentController.EditorTitleChanged += DocumentController_EditorTitleChanged;
		_luaIntellisenseBridge.Attach();
		_luaEditorLifecycleService.Attach();
		_editorLifecycleService.Attach();

		_documentController.CheckPreviousSession();

		if (!_documentController.GetOpenEditors().Any() && File.Exists(_workspaceProfile.InitialFilePath))
			_documentController.OpenFile(_workspaceProfile.InitialFilePath);

		ApplyEditorSettings();

		UpdateUi();
	}

	public FrameworkElement WorkbenchView => _dockHost.View;

	public string CaptureLayout() => _dockHost.SaveLayout();

	public void Dispose()
	{
		_workspaceProfile.SaveAvalonDockLayoutXml(_dockHost.SaveLayout());
		_luaEditorLifecycleService.Dispose();
		_luaIntellisenseBridge.Dispose();
		_scriptingMessageService.Dispose();
		_paneCatalog.Dispose();
		_editorLifecycleService.Dispose();
		_findAndReplaceView.ClosePermanently();
		_documentController.CurrentEditorChanged -= DocumentController_CurrentEditorChanged;
		_documentController.EditorClosed -= DocumentController_EditorClosed;
		_documentController.EditorTitleChanged -= DocumentController_EditorTitleChanged;
		_dockHost.DetachDocumentController();
	}

	public void EnsureTabFileSynchronization()
		=> _documentController.EnsureTabFileSynchronization();

	public void ApplyEditorSettings()
	{
		_scriptingMessageService.ApplyEditorSettings();

		foreach (LuaEditor editor in _documentController.GetOpenEditors().OfType<LuaEditor>())
			_luaTrackedDocumentStateService.ApplyTrackedState(editor);

		UpdateUi();
	}

	private void ApplySettingsToEditor(IEditorControl editor)
	{
		var configs = new ConfigurationCollection();

		switch (_workspaceProfile.Kind)
		{
			case ScriptingWorkspaceKind.ClassicScript:
				if (editor is ClassicScriptEditor classicScriptEditor)
					classicScriptEditor.UpdateSettings(configs.ClassicScript);
				else if (editor is IStringSectionNavigator)
					editor.UpdateSettings(configs.ClassicScript);
				break;

			case ScriptingWorkspaceKind.GameFlowScript:
				if (editor is GameFlowEditor gameFlowEditor)
					gameFlowEditor.UpdateSettings(configs.GameFlowScript);
				break;

			case ScriptingWorkspaceKind.TRX:
				if (editor is TRXEditor trxEditor)
					trxEditor.UpdateSettings(configs.TRX);
				break;

			case ScriptingWorkspaceKind.Lua:
				if (editor is LuaEditor luaEditor)
					luaEditor.UpdateSettings(configs.Lua);
				break;
		}
	}

	public void NotifyMainWindowFocusChanged(bool isFocused)
	{
		var fileSyncService = new StudioFileExplorerDocumentSyncService();
		fileSyncService.ApplyWindowFocus(_documentController, isFocused);
	}

	public void RestoreDefaultLayout()
	{
		_dockHost.RestoreDefaultLayout(_workspaceProfile.LoadDockPanelState(), UpdateUi);
		_paneCatalog.EnsurePanesRegistered(_dockHost);
		UpdateUi();
	}

	public bool TryExecuteCommand(UICommand command)
	{
		// Handle pane and service commands (was in WorkbenchPaneService.TryExecuteCommand).
		switch (command)
		{
			case UICommand.NewFile:
				_paneCatalog.GetPane<FileExplorerToolWindow>(UICommand.FileExplorer)?.CreateNewFile();
				UpdateUi();
				return true;

			case UICommand.Find:
				ShowFindReplace();
				UpdateUi();
				return true;

			case UICommand.Build when _workspaceProfile.SupportsBuild && _buildAction is not null:
				_buildAction();
				UpdateUi();
				return true;

			case UICommand.ScriptingDocumentation when _workspaceProfile.SupportsDocumentation && _showDocumentationAction is not null:
				_showDocumentationAction();
				UpdateUi();
				return true;

			case UICommand.About:
				ShowAboutDialog();
				UpdateUi();
				return true;

			case UICommand.GoToDefinition:
				ExecuteGoToDefinition();
				UpdateUi();
				return true;

			case UICommand.FindReferences:
				ExecuteFindReferences();
				UpdateUi();
				return true;

			case UICommand.RenameSymbol:
				ExecuteRenameSymbol();
				UpdateUi();
				return true;

			default:
				if (_paneCatalog.TryTogglePane(command, _dockHost, _paneHostService))
				{
					UpdateUi();
					return true;
				}
				break;
		}

		if (!CanExecuteCommand(command))
			return false;

		switch (command)
		{
			case UICommand.Save:
				_documentController.SaveFile();
				break;

			case UICommand.SaveAs:
				_documentController.SaveFileAs();
				break;

			case UICommand.SaveAll:
				_documentController.SaveAll();
				break;

			case UICommand.Undo:
				_documentController.CurrentEditor?.Undo();
				break;

			case UICommand.Redo:
				_documentController.CurrentEditor?.Redo();
				break;

			case UICommand.Cut:
				_documentController.CurrentEditor?.Cut();
				break;

			case UICommand.Copy:
				_documentController.CurrentEditor?.Copy();
				break;

			case UICommand.Paste:
				_documentController.CurrentEditor?.Paste();
				break;

			case UICommand.SelectAll:
				_documentController.CurrentEditor?.SelectAll();
				break;

			default:
				if (!TryExecuteDocumentCommand(command))
					return false;
				break;
		}

		UpdateUi();
		return true;
	}

	private bool CanExecuteBuiltInDocumentCommand(UICommand command)
	{
		if (_documentController.CurrentEditor is TextEditorBase)
		{
			switch (command)
			{
				case UICommand.TabsToSpaces:
				case UICommand.SpacesToTabs:
				case UICommand.Reindent:
				case UICommand.TrimWhiteSpace:
				case UICommand.ToggleComment:
				case UICommand.CommentOut:
				case UICommand.Uncomment:
				case UICommand.ToggleBookmark:
				case UICommand.PrevBookmark:
				case UICommand.NextBookmark:
				case UICommand.ClearBookmarks:
					return true;
			}
		}

		if (_documentController.CurrentEditor is IStringSectionNavigator)
		{
			switch (command)
			{
				case UICommand.PrevSection:
				case UICommand.NextSection:
				case UICommand.ClearString:
				case UICommand.RemoveLastString:
					return true;
			}
		}

		return false;
	}

	private bool CanExecuteCommand(UICommand command)
		=> command switch
		{
			// Commands that are always available.
			UICommand.NewFile => true,
			UICommand.Find => true,
			UICommand.About => true,
			UICommand.Settings => true,
			UICommand.RestoreDefaultLayout => true,

			// Build and documentation availability depends on workspace.
			UICommand.Build => _workspaceProfile.SupportsBuild,
			UICommand.ScriptingDocumentation => _workspaceProfile.SupportsDocumentation,

			// Pane visibility commands are always available when visible.
			UICommand.ContentExplorer or UICommand.FileExplorer or UICommand.ReferenceBrowser
				or UICommand.CompilerLogs or UICommand.SearchResults or UICommand.LuaDiagnostics
				or UICommand.LuaReferencesResults => true,

			// Navigation commands need an active editor.
			UICommand.GoToDefinition or UICommand.FindReferences or UICommand.RenameSymbol
				=> _documentController.CurrentEditor is not null,

			// Editor state-dependent document commands.
			UICommand.Save => _documentController.CurrentEditor?.IsContentChanged == true,
			UICommand.SaveAs => _documentController.CurrentEditor is not null,
			UICommand.SaveAll => !_documentController.IsEveryDocumentSaved(),
			UICommand.Undo => _documentController.CurrentEditor?.CanUndo == true,
			UICommand.Redo => _documentController.CurrentEditor?.CanRedo == true,
			UICommand.Cut or UICommand.Copy or UICommand.Paste or UICommand.SelectAll => _documentController.CurrentEditor is not null,
			_ => CanExecuteBuiltInDocumentCommand(command)
		};

	private void DocumentController_CurrentEditorChanged(object? sender, EventArgs e)
		=> UpdateUi();

	private void HandleLuaDiagnosticsUpdated(LuaDiagnosticsPayload payload)
		=> _luaTrackedDocumentStateService.ApplyDiagnosticsUpdate(payload.FilePath, payload.Diagnostics);

	private void HandleLuaSemanticTokensUpdated(LuaSemanticTokensPayload payload)
		=> _luaTrackedDocumentStateService.ApplySemanticTokensUpdate(payload.FilePath, payload.SemanticTokens);

	private void HandleLuaStartupFailed(LanguageServerStartupFailure failure)
	{
		if (failure.IsPersistent)
			_messageService.ShowError(failure.Message, "Lua IntelliSense");
		else
			_messageService.ShowInformation(failure.Message, "Lua IntelliSense");

		UpdateCommandStates();
	}

	private void HandleLuaWorkspaceWatcherFailed(WorkspaceWatcherFailure failure)
		=> _messageService.ShowInformation(failure.Message, "Lua IntelliSense");

	private void NavigateToLuaDefinition(TextDefinitionLocation location)
	{
		ArgumentNullException.ThrowIfNull(location);

		string? targetFilePath = !string.IsNullOrWhiteSpace(location.FilePath)
			? location.FilePath
			: (_documentController.CurrentEditor as TextEditorBase)?.FilePath;

		if (string.IsNullOrWhiteSpace(targetFilePath))
			return;

		_documentController.OpenFile(targetFilePath);

		if (_documentController.CurrentEditor is TextEditorBase textEditor)
		{
			EditorNavigationHelper.ApplyLocation(
				textEditor,
				EditorNavigationHelper.CreateDefinitionLocation(textEditor, targetFilePath, location.LineNumber, location.ColumnNumber));
		}
	}

	private void DocumentController_EditorClosed(object? sender, EditorControlEventArgs e)
		=> UpdateUi();

	private void DocumentController_EditorTitleChanged(object? sender, EditorControlEventArgs e)
		=> UpdateUndoRedoCommandPresentation();

	private void ExecuteCommand(UICommand command)
		=> _ = TryExecuteCommand(command);

	private void UpdateCommandStates()
	{
		_menuService.UpdateCommandEnabledStates(CanExecuteCommand);
		_toolBarService.UpdateCommandEnabledStates(CanExecuteCommand);
		UpdateUndoRedoCommandPresentation();
	}

	private void UpdateDocumentCommandSurface(IEditorControl? editor, DocumentMode documentMode)
	{
		if (editor is null)
		{
			_menuService.SetDocumentCommandSurface(documentMode, []);
			_toolBarService.SetDocumentCommandSurface(documentMode, []);
			_statusBarService.SetStatusStripContext(null, documentMode, []);
			return;
		}

		_menuService.SetDocumentCommandSurface(
			documentMode,
			TypedDocumentCommandSurfaceProvider.Instance.GetMenuStripItems(editor, documentMode));
		_toolBarService.SetDocumentCommandSurface(
			documentMode,
			TypedDocumentCommandSurfaceProvider.Instance.GetToolStripItems(editor, documentMode));
		_statusBarService.SetStatusStripContext(editor, documentMode, GetDocumentStatusStripSegments(editor, documentMode));
	}

	private static IReadOnlyList<StudioStatusStripSegment> GetDocumentStatusStripSegments(IEditorControl editor, DocumentMode documentMode)
		=> documentMode == DocumentMode.ClassicScript
			? ClassicScriptStatusStripProvider.GetSegments(editor, documentMode)
			: [];

	private void UpdateUi()
	{
		IEditorControl? currentEditor = _documentController.CurrentEditor;
		DocumentMode documentMode = _documentController.GetDocumentMode(currentEditor);
		UpdateDocumentCommandSurface(currentEditor, documentMode);
		UpdateCurrentEditor(currentEditor, documentMode);
		_paneCatalog.UpdatePaneVisibilityChecks(_dockHost, _paneHostService);
		UpdateCommandStates();
	}

	private void UpdateUndoRedoCommandPresentation()
	{
		bool canUndo = _documentController.CurrentEditor?.CanUndo == true;
		_menuService.SetCommandText(UICommand.Undo, canUndo ? Strings.Default.Undo : Strings.Default.CantUndo);
		_toolBarService.SetCommandText(UICommand.Undo, canUndo ? Strings.Default.Undo : Strings.Default.CantUndo);
		_toolBarService.SetCommandToolTip(UICommand.Undo, canUndo ? Strings.Default.Undo : Strings.Default.CantUndo);

		bool canRedo = _documentController.CurrentEditor?.CanRedo == true;
		_menuService.SetCommandText(UICommand.Redo, canRedo ? Strings.Default.Redo : Strings.Default.CantRedo);
		_toolBarService.SetCommandText(UICommand.Redo, canRedo ? Strings.Default.Redo : Strings.Default.CantRedo);
		_toolBarService.SetCommandToolTip(UICommand.Redo, canRedo ? Strings.Default.Redo : Strings.Default.CantRedo);
	}

	private bool TryExecuteDocumentCommand(UICommand command)
	{
		if (_documentController.CurrentEditor is TextEditorBase textEditor)
		{
			switch (command)
			{
				case UICommand.TabsToSpaces:
					textEditor.ConvertTabsToSpaces();
					return true;

				case UICommand.SpacesToTabs:
					textEditor.ConvertSpacesToTabs();
					return true;

				case UICommand.Reindent:
					textEditor.TidyCode();
					return true;

				case UICommand.TrimWhiteSpace:
					textEditor.TidyCode(true);
					return true;

				case UICommand.ToggleComment:
					textEditor.ToggleCommentLines();
					return true;

				case UICommand.CommentOut:
					textEditor.CommentOutLines();
					return true;

				case UICommand.Uncomment:
					textEditor.UncommentLines();
					return true;

				case UICommand.ToggleBookmark:
					textEditor.ToggleBookmark();
					return true;

				case UICommand.PrevBookmark:
					textEditor.GoToPrevBookmark();
					return true;

				case UICommand.NextBookmark:
					textEditor.GoToNextBookmark();
					return true;

				case UICommand.ClearBookmarks:
					textEditor.ClearAllBookmarks(() => _messageService.ShowConfirmation("Are you sure you want to clear all bookmarks from the current document?"));
					return true;
			}
		}

		if (_documentController.CurrentEditor is IStringSectionNavigator navigator)
		{
			switch (command)
			{
				case UICommand.PrevSection:
					navigator.GoToPreviousSection();
					_paneCatalog.GetPane<DocumentOutlineToolWindow>(UICommand.ContentExplorer)?.SelectNode(navigator.CurrentSectionName);
					return true;

				case UICommand.NextSection:
					navigator.GoToNextSection();
					_paneCatalog.GetPane<DocumentOutlineToolWindow>(UICommand.ContentExplorer)?.SelectNode(navigator.CurrentSectionName);
					return true;

				case UICommand.ClearString:
					navigator.ClearSelectedString();
					return true;

				case UICommand.RemoveLastString:
					navigator.RemoveLastString();
					return true;
			}
		}

		return false;
	}

	private void UpdateCurrentEditor(IEditorControl? currentEditor, DocumentMode documentMode)
	{
		if (_paneCatalog.GetPane<DocumentOutlineToolWindow>(UICommand.ContentExplorer) is DocumentOutlineToolWindow documentOutline)
		{
			documentOutline.DocumentMode = documentMode;
			documentOutline.EditorControl = currentEditor;
		}

		if (_paneCatalog.GetPane<TextDiagnosticsToolWindow>(UICommand.LuaDiagnostics) is TextDiagnosticsToolWindow luaDiagnostics)
		{
			if (currentEditor is LuaEditor luaEditor)
				luaDiagnostics.ShowDiagnostics(luaEditor.FilePath, luaEditor.Document, []);
			else
				luaDiagnostics.ShowNoActiveDocument();
		}

		if (_paneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults) is TextReferencesResultsToolWindow luaReferencesResults)
		{
			if (currentEditor is LuaEditor)
				luaReferencesResults.ShowUnsupported();
			else
				luaReferencesResults.ShowNoActiveDocument();
		}
	}

	private void UpdateCompilerLogs(string text)
		=> _paneCatalog.GetPane<CompilerLogsToolWindow>(UICommand.CompilerLogs)?.UpdateLogs(text);

	private void ShowCompilerLogsPane()
		=> _paneCatalog.ShowPane(UICommand.CompilerLogs, _dockHost);

	private void ShowFindReplace()
	{
		IWin32Window? owner = _dialogOwnerProvider.GetOwner() ?? Form.ActiveForm;

		_findAndReplaceView.Show(
			owner?.Handle ?? IntPtr.Zero,
			_documentController.CurrentEditor?.SelectedContent?.ToString() ?? string.Empty);
	}

	private void ExecuteGoToDefinition()
	{
		if (_documentController.CurrentEditor is LuaEditor luaEditor)
		{
			_ = luaEditor.NavigateToDefinitionAtCaretAsync();
			return;
		}

		// For ClassicScript, also show the Reference Browser pane.
		if (_workspaceProfile.Kind == ScriptingWorkspaceKind.ClassicScript)
			_paneCatalog.ShowPane(UICommand.ReferenceBrowser, _dockHost);

		// Try built-in text editor navigation.
		if (_documentController.CurrentEditor is ClassicScriptEditor classicEditor)
		{
			var source = new TextDocumentSnapshot(classicEditor.Document);
			string? word = _languageServices.LineService.GetWordAtOffset(source, classicEditor.CaretOffset);

			if (word is not null)
				classicEditor.GoToObject(word);
		}
		else if (_documentController.CurrentEditor is TextEditorBase textEditor)
		{
			string? word = textEditor.GetWordFromOffset(textEditor.CaretOffset);

			if (word is not null)
				textEditor.GoToObject(word);
		}
	}

	private void ExecuteFindReferences()
	{
		// For ClassicScript, show the Reference Browser pane.
		if (_workspaceProfile.Kind == ScriptingWorkspaceKind.ClassicScript)
			_paneCatalog.ShowPane(UICommand.ReferenceBrowser, _dockHost);
	}

	private void ExecuteRenameSymbol()
	{
		// Rename is primarily a Lua feature via the language server.
		if (_documentController.CurrentEditor is LuaEditor luaEditor)
		{
			_ = luaEditor.NavigateToDefinitionAtCaretAsync();
		}
	}

	private void HandleFindAllPerformed(IReadOnlyList<FindReplaceSource> sources)
	{
		_paneCatalog.ShowPane(UICommand.SearchResults, _dockHost);
		_paneCatalog.GetPane<SearchResultsToolWindow>(UICommand.SearchResults)?.UpdateResults(
			new FindReplaceEventArgs(sources));
		_paneCatalog.UpdatePaneVisibilityChecks(_dockHost, _paneHostService);
	}

	private void ShowAboutDialog()
	{
		using (var form = new FormAbout(Resources.AboutScreen_800))
		{
			IWin32Window? owner = _dialogOwnerProvider.GetOwner() ?? Form.ActiveForm;

			if (owner is not null)
				form.ShowDialog(owner);
			else
				form.ShowDialog();
		}
	}
}
