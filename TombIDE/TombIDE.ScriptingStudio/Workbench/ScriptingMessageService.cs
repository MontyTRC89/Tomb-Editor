#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;
using TombIDE.ScriptingStudio.GameFlowScript;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Services;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.TRX;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Documents;
using TombLib.Scripting.ClassicScript.Writers;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua.Documents;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Text;
using GameFlowScriptReplacer = TombLib.Scripting.GameFlowScript.Writers.ScriptReplacer;
using TRXScriptReplacer = TombLib.Scripting.TRX.Writers.ScriptReplacer;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class ScriptingMessageService : IDisposable
{
	private readonly IEditorDocumentController _documentController;
	private readonly IMessenger _messenger;
	private readonly StudioDockLayoutPersistenceService _dockLayoutPersistenceService;
	private readonly IStudioWorkspaceAutomationProvider _workspaceAutomationProvider;
	private readonly ScriptingWorkspaceProfile _workspaceProfile;
	private readonly Func<string> _getDockLayoutXml;

	public ScriptingMessageService(
		IMessenger messenger,
		ScriptingWorkspaceProfile workspaceProfile,
		IEditorDocumentController documentController,
		string engineDirectoryPath,
		string engineExecutableFilePath,
		ScriptingMessageServiceOptions options,
		ClassicScriptLanguageServices languageServices,
		GameFlowLanguageServices gameFlowLanguageServices,
		TRXLanguageServices trxLanguageServices)
	{
		ArgumentNullException.ThrowIfNull(messenger);
		ArgumentNullException.ThrowIfNull(workspaceProfile);
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(engineDirectoryPath);
		ArgumentNullException.ThrowIfNull(engineExecutableFilePath);
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(languageServices);
		ArgumentNullException.ThrowIfNull(gameFlowLanguageServices);
		ArgumentNullException.ThrowIfNull(trxLanguageServices);

		_messenger = messenger;
		_workspaceProfile = workspaceProfile;
		_documentController = documentController;
		_getDockLayoutXml = options.GetDockLayoutXml;
		_dockLayoutPersistenceService = new StudioDockLayoutPersistenceService();
		_workspaceAutomationProvider = CreateWorkspaceAutomationProvider(
			workspaceProfile,
			documentController,
			engineDirectoryPath,
			engineExecutableFilePath,
			options.HostOperations,
			options.ShowCompilerLogsPane,
			options.UpdateCompilerLogs,
			options.ShowCompilerLogsAfterBuild,
			options.UseNewIncludeMethod,
			languageServices,
			gameFlowLanguageServices,
			trxLanguageServices);

		_messenger.Register<ScriptingMessageService, ScriptingAppendScriptRequestedMessage>(
			this,
			static (recipient, message) => recipient._workspaceAutomationProvider.AppendScript(message.Result));
		_messenger.Register<ScriptingMessageService, ScriptingAddLevelStringRequestedMessage>(
			this,
			static (recipient, message) => recipient._workspaceAutomationProvider.AddLevelString(message.LevelName));
		_messenger.Register<ScriptingMessageService, ScriptingAddPluginEntryRequestedMessage>(
			this,
			static (recipient, message) => recipient._workspaceAutomationProvider.AddPluginEntry(message.PluginString));
		_messenger.Register<ScriptingMessageService, ScriptingAddNgStringRequestedMessage>(
			this,
			static (recipient, message) => recipient._workspaceAutomationProvider.AddNgString(message.NgString));
		_messenger.Register<ScriptingMessageService, ScriptingRenameLevelRequestedMessage>(
			this,
			static (recipient, message) => recipient._workspaceAutomationProvider.RenameLevel(message.OldName, message.NewName));
		_messenger.Register<ScriptingMessageService, ScriptingReloadSyntaxHighlightingRequestedMessage>(
			this,
			static (recipient, _) => recipient._workspaceAutomationProvider.ReloadSyntaxHighlighting());
		_messenger.Register<ScriptingMessageService, ScriptingCanCloseRequestMessage>(
			this,
			static (recipient, message) => recipient.HandleCanClose(message));
		_messenger.Register<ScriptingMessageService, ScriptingIsScriptDefinedRequestMessage>(
			this,
			static (recipient, message) => message.Reply(recipient._workspaceAutomationProvider.IsScriptDefined(message.LevelName)));
		_messenger.Register<ScriptingMessageService, ScriptingIsStringDefinedRequestMessage>(
			this,
			static (recipient, message) => message.Reply(recipient._workspaceAutomationProvider.IsStringDefined(message.Value)));
	}

	public void Dispose()
		=> _messenger.UnregisterAll(this);

	public void ApplyEditorSettings()
		=> ApplyEditorSettings(_documentController, _workspaceProfile.Kind);

	public void Build()
		=> _workspaceAutomationProvider.Build();

	public void ShowDocumentation()
		=> _workspaceAutomationProvider.ShowDocumentation();

	private static IStudioWorkspaceAutomationProvider CreateWorkspaceAutomationProvider(
		ScriptingWorkspaceProfile workspaceProfile,
		IEditorDocumentController documentController,
		string engineDirectoryPath,
		string engineExecutableFilePath,
		IScriptingHostOperations hostOperations,
		Action showCompilerLogsPane,
		Action<string> updateCompilerLogs,
		Func<bool> showCompilerLogsAfterBuild,
		Func<bool> useNewIncludeMethod,
		ClassicScriptLanguageServices languageServices,
		GameFlowLanguageServices gameFlowLanguageServices,
		TRXLanguageServices trxLanguageServices)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(hostOperations);

		return workspaceProfile.Kind switch
		{
			ScriptingWorkspaceKind.ClassicScript => CreateClassicScriptProvider(workspaceProfile, documentController, engineDirectoryPath, hostOperations, showCompilerLogsPane, updateCompilerLogs, showCompilerLogsAfterBuild, useNewIncludeMethod, languageServices),
			ScriptingWorkspaceKind.GameFlowScript => CreateGameFlowProvider(workspaceProfile, documentController, engineDirectoryPath, engineExecutableFilePath, hostOperations, showCompilerLogsPane, updateCompilerLogs, showCompilerLogsAfterBuild, gameFlowLanguageServices),
			ScriptingWorkspaceKind.TRX => CreateTrxProvider(workspaceProfile, documentController, hostOperations, trxLanguageServices),
			ScriptingWorkspaceKind.Lua => CreateLuaProvider(workspaceProfile, documentController, hostOperations),
			_ => throw new NotSupportedException($"Unsupported scripting workspace kind: {workspaceProfile.Kind}.")
		};
	}

	private static IStudioWorkspaceAutomationProvider CreateClassicScriptProvider(
		ScriptingWorkspaceProfile workspaceProfile,
		IEditorDocumentController documentController,
		string engineDirectoryPath,
		IScriptingHostOperations hostOperations,
		Action showCompilerLogsPane,
		Action<string> updateCompilerLogs,
		Func<bool> showCompilerLogsAfterBuild,
		Func<bool> useNewIncludeMethod,
		ClassicScriptLanguageServices languageServices)
	{
		var textEditorHost = new DocumentControllerTextEditorHost(documentController);
		var documentLookupService = new ClassicScriptDocumentLookupService(languageServices.CommandService);
		var scriptReplacer = new ScriptReplacer(languageServices.LineService);
		var languageStringWriter = new LanguageStringWriter(languageServices.CommandService);
		var silentActionService = new StudioSilentActionService(documentController, hostOperations);

		return new ClassicScriptWorkspaceAutomationProvider(
			new Control(),
			workspaceProfile,
			silentActionService,
			documentController.ScriptRootDirectoryPath,
			engineDirectoryPath,
			new ClassicScriptWorkspaceAutomationCallbacks(
				scriptText =>
				{
					TextEditorBase editor = textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TR4));
					editor.AppendText(Environment.NewLine + scriptText + Environment.NewLine);
					editor.ScrollToLine(editor.LineCount);
				},
				levelName =>
				{
					TextEditorBase editor = textEditorHost.OpenTextEditor(
						PathHelper.GetLanguageFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TR4),
						openSourceView: true);
					languageStringWriter.WriteNewLevelNameString(editor, levelName);
				},
				pluginString => textEditorHost.OpenEditor<ClassicScriptEditor>(
					PathHelper.GetScriptFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TR4)) is ClassicScriptEditor editor
					&& editor.TryAddNewPluginEntry(pluginString),
				ngString => textEditorHost.OpenTextEditor(
					PathHelper.GetLanguageFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TRNG),
					openSourceView: true) is TextEditorBase languageEditor
					&& languageStringWriter.WriteNewNGString(languageEditor, ngString),
				levelName =>
				{
					TextEditorBase editor = textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TR4));
					return documentLookupService.IsLevelScriptDefined(new TextDocumentSnapshot(editor.Document), levelName);
				},
				levelName =>
				{
					TextEditorBase editor = textEditorHost.OpenTextEditor(
						PathHelper.GetLanguageFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TR4),
						openSourceView: true);
					return documentLookupService.IsLevelLanguageStringDefined(new TextDocumentSnapshot(editor.Document), levelName);
				},
				(oldName, newName) =>
				{
					TextEditorBase editor = textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TR4));
					scriptReplacer.RenameLevelScript(editor, oldName, newName);
				},
				(oldName, newName) =>
				{
					TextEditorBase editor = textEditorHost.OpenTextEditor(
						PathHelper.GetLanguageFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TR4),
						openSourceView: true);
					scriptReplacer.RenameLanguageString(editor, oldName, newName);
				},
				() => ApplyEditorSettings(documentController, workspaceProfile.Kind),
				documentController.SaveAll,
				showCompilerLogsPane,
				updateCompilerLogs),
			showCompilerLogsAfterBuild,
			useNewIncludeMethod);
	}

	private static IStudioWorkspaceAutomationProvider CreateGameFlowProvider(
		ScriptingWorkspaceProfile workspaceProfile,
		IEditorDocumentController documentController,
		string engineDirectoryPath,
		string engineExecutableFilePath,
		IScriptingHostOperations hostOperations,
		Action showCompilerLogsPane,
		Action<string> updateCompilerLogs,
		Func<bool> showCompilerLogsAfterBuild,
		GameFlowLanguageServices languageServices)
	{
		var textEditorHost = new DocumentControllerTextEditorHost(documentController);
		var documentLookupService = languageServices.DocumentLookupService;
		var silentActionService = new StudioSilentActionService(documentController, hostOperations);

		return new GameFlowWorkspaceAutomationProvider(
			new Control(),
			workspaceProfile,
			silentActionService,
			documentController.ScriptRootDirectoryPath,
			engineDirectoryPath,
			engineExecutableFilePath,
			new GameFlowWorkspaceAutomationCallbacks(
				scriptText =>
				{
					TextEditorBase editor = textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TR2));
					editor.AppendText(Environment.NewLine + scriptText + Environment.NewLine);
					editor.ScrollToLine(editor.LineCount);
				},
				levelName =>
				{
					TextEditorBase editor = textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TR2));
					return documentLookupService.IsLevelScriptDefined(new TextDocumentSnapshot(editor.Document), levelName);
				},
				(oldName, newName) =>
				{
					TextEditorBase editor = textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TR2));
					GameFlowScriptReplacer.RenameLevelScript(editor, oldName, newName);
				},
				documentController.SaveAll,
				showCompilerLogsPane,
				updateCompilerLogs),
			showCompilerLogsAfterBuild);
	}

	private static IStudioWorkspaceAutomationProvider CreateLuaProvider(
		ScriptingWorkspaceProfile workspaceProfile,
		IEditorDocumentController documentController,
		IScriptingHostOperations hostOperations)
	{
		var textEditorHost = new DocumentControllerTextEditorHost(documentController);
		var levelScriptService = new TombEngineLevelScriptService();
		var languageScriptService = new TombEngineLanguageScriptService();
		var silentActionService = new StudioSilentActionService(documentController, hostOperations);

		return new LuaWorkspaceAutomationProvider(
			silentActionService,
			documentController.ScriptRootDirectoryPath,
			new LuaWorkspaceAutomationCallbacks(
				result =>
				{
					bool scriptUpdated = false;
					bool languageUpdated = false;

					if (result.GameFlowScript.Length > 0)
					{
						TextEditorBase editor = textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TombEngine));
						editor.AppendText(Environment.NewLine + result.GameFlowScript + Environment.NewLine);
						editor.ScrollToLine(editor.LineCount);
						scriptUpdated = true;
					}

					if (result.LanguageScript.Length > 0
						&& textEditorHost.OpenTextEditor(PathHelper.GetLanguageFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TombEngine)) is TextEditorBase stringsEditor)
					{
						int? insertedLineNumber = languageScriptService.TryInsertLanguageScript(stringsEditor.Document, result.LanguageScript);

						if (insertedLineNumber is not null)
						{
							stringsEditor.ResetSelectionAt(insertedLineNumber.Value);
							stringsEditor.ScrollToLine(insertedLineNumber.Value);
							languageUpdated = true;
						}
					}

					CreateGeneratedFiles(documentController.ScriptRootDirectoryPath, result.FilesToCreate);
					return (scriptUpdated, languageUpdated);
				},
				levelName =>
				{
					TextDocument? scriptDocument = textEditorHost.TryGetTextDocument(PathHelper.GetScriptFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TombEngine));
					TextDocument? languageDocument = textEditorHost.TryGetTextDocument(PathHelper.GetLanguageFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TombEngine));

					if (scriptDocument is null || languageDocument is null)
						return false;

					return levelScriptService.IsLevelScriptDefined(scriptDocument, languageDocument, levelName);
				},
				levelName =>
				{
					TextEditorBase editor = textEditorHost.OpenTextEditor(PathHelper.GetLanguageFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TombEngine));
					var regex = new Regex($"\"{Regex.Escape(levelName)}\"");
					return editor.Document.Lines.Any(line => regex.IsMatch(editor.Document.GetText(line)));
				},
				(oldName, newName) =>
				{
					if (textEditorHost.OpenTextEditor(PathHelper.GetLanguageFilePath(documentController.ScriptRootDirectoryPath, TRVersion.Game.TombEngine)) is not TextEditorBase editor)
						return;

					var regex = new Regex($"\"{Regex.Escape(oldName)}\"");
					DocumentLine? stringLine = editor.Document.Lines.FirstOrDefault(line => regex.IsMatch(editor.Document.GetText(line)));

					if (stringLine is null)
						return;

					string lineText = editor.Document.GetText(stringLine);
					editor.ReplaceLine(stringLine, regex.Replace(lineText, $"\"{newName}\""));
					editor.ScrollToLine(stringLine.LineNumber);
				},
				static () => { }));
	}

	private static IStudioWorkspaceAutomationProvider CreateTrxProvider(
		ScriptingWorkspaceProfile workspaceProfile,
		IEditorDocumentController documentController,
		IScriptingHostOperations hostOperations,
		TRXLanguageServices trxLanguageServices)
	{
		var textEditorHost = new DocumentControllerTextEditorHost(documentController);
		var documentLookupService = trxLanguageServices.DocumentLookupService;
		var scriptReplacer = new TRXScriptReplacer();
		var silentActionService = new StudioSilentActionService(documentController, hostOperations);

		return new TrxWorkspaceAutomationProvider(
			silentActionService,
			documentController.ScriptRootDirectoryPath,
			workspaceProfile.GameVersion,
			new TrxWorkspaceAutomationCallbacks(
				levelName =>
				{
					TextEditorBase editor = textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(documentController.ScriptRootDirectoryPath, workspaceProfile.GameVersion));
					var source = new TextDocumentSnapshot(editor.Document);
					return documentLookupService.IsLevelScriptDefined(source, levelName);
				},
				(oldName, newName) =>
				{
					TextEditorBase editor = textEditorHost.OpenTextEditor(PathHelper.GetScriptFilePath(documentController.ScriptRootDirectoryPath, workspaceProfile.GameVersion));
					scriptReplacer.RenameLevelScript(editor, oldName, newName);
				}));
	}

	private void HandleCanClose(ScriptingCanCloseRequestMessage message)
	{
		_workspaceAutomationProvider.HandleProgramClosing();
		bool canClose = _documentController.AskSaveAll();
		_dockLayoutPersistenceService.SaveState(_workspaceProfile, _getDockLayoutXml());
		message.Reply(canClose);
	}

	private static void ApplyEditorSettings(IEditorDocumentController documentController, ScriptingWorkspaceKind workspaceKind)
	{
		var configs = new ConfigurationCollection();

		foreach (IEditorControl editor in documentController.GetOpenEditors())
		{
			switch (workspaceKind)
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
	}

	private static void CreateGeneratedFiles(string scriptRootDirectoryPath, IReadOnlyList<GeneratedScriptFile> files)
	{
		string scriptRootPath = Path.GetFullPath(scriptRootDirectoryPath);

		if (!scriptRootPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
			scriptRootPath += Path.DirectorySeparatorChar;

		foreach (GeneratedScriptFile file in files)
		{
			string filePath = Path.GetFullPath(Path.Combine(scriptRootPath, file.RelativePath));

			if (!filePath.StartsWith(scriptRootPath, StringComparison.OrdinalIgnoreCase))
				continue;

			string? directory = Path.GetDirectoryName(filePath);

			if (directory is not null && !Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			File.WriteAllText(filePath, file.Content);
		}
	}
}
