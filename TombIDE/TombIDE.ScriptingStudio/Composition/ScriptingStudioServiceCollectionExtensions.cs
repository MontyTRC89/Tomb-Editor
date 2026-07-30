#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using MvvmDialogs;
using System;
using System.Windows.Input;
using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.DocumentOutline;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Host;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.Shortcuts;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Messaging;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.SharedClasses;
using TombLib.LanguageServer.Lua;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Documents;
using TombLib.Scripting.GameFlowScript.Hover;
using TombLib.Scripting.GameFlowScript.Navigation;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Documents;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Navigation;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.UI.Editors;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.Composition;

/// <summary>
/// Registers the services needed to host the active ScriptingStudio shell path.
/// </summary>
public static class ScriptingStudioServiceCollectionExtensions
{
	public static IServiceCollection AddScriptingStudioHostComposition(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddSingleton<IMessenger>(_ => new WeakReferenceMessenger());
		services.AddTransient<IUiDispatcherService>(_ => SynchronizationContextUiDispatcherService.FromCurrentContext());
		services.AddSingleton(CreateDefaultCommandCatalog);
		services.AddSingleton<StudioStatusStripContributionService>();
		services.AddTransient<IScriptingStudioShellFactory, ScriptingStudioShellFactory>();

		services.AddSingleton<ClassicScriptMnemonicCatalogService>();
		services.AddSingleton<ClassicScriptSyntaxCatalogService>();
		services.AddSingleton<IClassicScriptLineService, ClassicScriptLineService>();
		services.AddSingleton<IClassicScriptCommandService, ClassicScriptCommandService>();
		services.AddSingleton<IClassicScriptIndexService, ClassicScriptIndexService>();
		services.AddSingleton<ClassicScriptLanguageServices>(sp =>
		{
			var lineService = sp.GetRequiredService<IClassicScriptLineService>();
			var commandService = sp.GetRequiredService<IClassicScriptCommandService>();
			var indexService = sp.GetRequiredService<IClassicScriptIndexService>();
			var mnemonicCatalogService = sp.GetRequiredService<ClassicScriptMnemonicCatalogService>();
			var syntaxCatalogService = sp.GetRequiredService<ClassicScriptSyntaxCatalogService>();
			var errorDetector = new ErrorDetector(lineService, commandService, syntaxCatalogService);

			return new ClassicScriptLanguageServices(
				new ClassicScriptDefinitionProvider(commandService),
				new ClassicScriptHoverProvider(lineService, commandService, mnemonicCatalogService),
				new ClassicScriptSignatureHelpProvider(commandService),
				errorDetector,
				lineService,
				commandService,
				indexService);
		});

		// GameFlowScript services.
		services.AddSingleton<IGameFlowScriptLineService, GameFlowScriptLineService>();
		services.AddSingleton<IGameFlowScriptDocumentService, GameFlowScriptDocumentService>();
		services.AddSingleton<GameFlowLanguageServices>(sp =>
		{
			var lineService = sp.GetRequiredService<IGameFlowScriptLineService>();
			var documentService = sp.GetRequiredService<IGameFlowScriptDocumentService>();

			return new GameFlowLanguageServices(
				new GameFlowDefinitionProvider(documentService),
				new GameFlowHoverProvider(),
				new GameFlowAutocompleteService(lineService),
				lineService,
				documentService,
				new GameFlowDocumentLookupService(documentService));
		});

		// TRX services.
		services.AddSingleton<IGameflowSchemaService>(_ =>
			new GameflowSchemaService(TrxResourcePaths.GetGameflowSchemaPath()));
		services.AddSingleton<ITRXLineService, TRXLineService>();
		services.AddSingleton<ITRXDocumentService, TRXDocumentService>();
		services.AddSingleton<TRXLanguageServices>(sp =>
		{
			var schemaService = sp.GetRequiredService<IGameflowSchemaService>();
			var lineService = sp.GetRequiredService<ITRXLineService>();
			var documentService = sp.GetRequiredService<ITRXDocumentService>();

			return new TRXLanguageServices(schemaService, lineService, documentService);
		});

		AddScriptingStudioShellServices(services);

		return services;
	}

	/// <summary>
	/// Registers shell-scoped services. These are resolved once per
	/// <see cref="IScriptingStudioShell"/> instance via a child scope.
	/// </summary>
	internal static void AddScriptingStudioShellServices(IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		// Scoped context and input bridge.
		services.AddScoped<ScriptingStudioShellContext>();
		services.AddScoped<IScriptingProjectContext>(sp =>
			sp.GetRequiredService<ScriptingStudioShellContext>().ProjectContext);

		// Dialog owner provider (set once by Mount).
		services.AddScoped<IWin32DialogOwnerProvider, Win32DialogOwnerProvider>();

		// Settings bridge for delegate parameters (populated by RootShellViewModel).
		services.AddScoped<ShellWorkbenchSettings>();

		// Lua IntelliSense provider (shell-scoped, one per workspace).
		services.AddScoped<ILuaIntellisenseProvider>(sp =>
		{
			var projectContext = sp.GetRequiredService<IScriptingProjectContext>();
			var messageService = sp.GetRequiredService<IMessageService>();

			TENApiService.InjectTENApi(
				projectContext.Project, projectContext.Project.GetCurrentEngineVersion());

			string? executablePath = LuaLanguageServerLocator.ResolveExecutablePath();

			if (string.IsNullOrWhiteSpace(executablePath))
			{
				messageService.ShowError(
					"The bundled Lua language server could not be found. Lua IntelliSense is unavailable.",
					"Lua IntelliSense");
			}

			return new LuaLanguageServerIntellisenseProvider(
				projectContext.ScriptRootDirectoryPath, executablePath);
		});

		// Text editor host adapter (bridges document controller to the editor host interface).
		services.AddScoped<ITextEditorHost>(sp =>
		{
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			return new DocumentControllerTextEditorHost(documentController);
		});

		// Lua tracked document state (manages per-document diagnostics and semantic tokens).
		services.AddScoped<LuaTrackedDocumentStateService>(sp =>
		{
			var textEditorHost = sp.GetRequiredService<ITextEditorHost>();
			var intellisenseProvider = sp.GetRequiredService<ILuaIntellisenseProvider>();
			return new LuaTrackedDocumentStateService(textEditorHost, intellisenseProvider);
		});

		// Lua IntelliSense event bridge (wires provider events to the UI dispatcher).
		services.AddScoped<ILuaIntellisenseBridge>(sp =>
		{
			var dockHost = sp.GetRequiredService<IAvalonDockHost>();
			var messenger = sp.GetRequiredService<IMessenger>();
			var intellisenseProvider = sp.GetRequiredService<ILuaIntellisenseProvider>();
			return new LuaIntellisenseEventBridge(dockHost, messenger, intellisenseProvider);
		});

		// Lua editor lifecycle coordinator (attaches/detaches Lua editor events).
		services.AddScoped<ILuaEditorLifecycleService>(sp =>
		{
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var messenger = sp.GetRequiredService<IMessenger>();
			var intellisenseProvider = sp.GetRequiredService<ILuaIntellisenseProvider>();
			var trackedDocumentStateService = sp.GetRequiredService<LuaTrackedDocumentStateService>();
			return new LuaDocumentLifecycleCoordinator(
				documentController,
				messenger,
				intellisenseProvider,
				trackedDocumentStateService);
		});

		// Settings store (needs legacy snapshot from context).
		services.AddScoped<IScriptingStudioShellSettingsStore>(sp =>
		{
			ScriptingStudioLegacySettingsSnapshot legacySnapshot =
				sp.GetRequiredService<ScriptingStudioShellContext>().LegacySettingsSnapshot;
			return new XmlScriptingStudioShellSettingsStore(legacySnapshot);
		});

		// Workspace profile (depends on project context, settings store, and language services).
		services.AddScoped<ScriptingWorkspaceProfile>(sp =>
		{
			IScriptingProjectContext projectContext =
				sp.GetRequiredService<IScriptingProjectContext>();
			IScriptingStudioShellSettingsStore settingsStore =
				sp.GetRequiredService<IScriptingStudioShellSettingsStore>();
			var classicScriptServices = sp.GetRequiredService<ClassicScriptLanguageServices>();
			var gameFlowServices = sp.GetRequiredService<GameFlowLanguageServices>();
			var trxServices = sp.GetRequiredService<TRXLanguageServices>();
			return ScriptingWorkspaceProfileSelector.Create(projectContext, settingsStore, classicScriptServices, gameFlowServices, trxServices);
		});

		// Shortcut binding service (shell-scoped, merges catalog defaults with persisted overrides).
		services.AddScoped<IShortcutBindingService>(sp =>
		{
			var catalog = sp.GetRequiredService<StudioCommandCatalog>();
			var settingsStore = sp.GetRequiredService<IScriptingStudioShellSettingsStore>();
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();

			ShortcutOverrideCollection overrides = settingsStore.Load(profile).ShortcutOverrides;

			return new ShortcutBindingService(catalog, overrides, newOverrides =>
				settingsStore.SaveShortcutOverrides(profile.Kind, newOverrides));
		});

		// Chrome services - each owns one builder/view.
		services.AddScoped<IMenuService>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var shortcutBindingService = sp.GetRequiredService<IShortcutBindingService>();
			return new MenuService(profile, shortcutBindingService);
		});
		services.AddScoped<IToolBarService>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var shortcutBindingService = sp.GetRequiredService<IShortcutBindingService>();
			return new ToolBarService(profile, shortcutBindingService);
		});
		services.AddScoped<IStatusBarService>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var statusStripContributionService = sp.GetRequiredService<StudioStatusStripContributionService>();
			return new StatusBarService(profile, statusStripContributionService);
		});
		services.AddScoped<IPaneHostService>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var menuService = sp.GetRequiredService<IMenuService>();
			var toolBarService = sp.GetRequiredService<IToolBarService>();
			return new PaneVisibilityStateService(profile, menuService, toolBarService);
		});

		// Pane infrastructure.
		services.AddScoped<StudioFileExplorerDocumentSyncService>();

		// ViewModels.
		services.AddScoped<DocumentOutlineViewModel>(sp =>
		{
			var localizationService = sp.GetRequiredService<ILocalizationService>();
			var languageServices = sp.GetRequiredService<ClassicScriptLanguageServices>();
			var gameFlowLanguageServices = sp.GetRequiredService<GameFlowLanguageServices>();
			var trxLanguageServices = sp.GetRequiredService<TRXLanguageServices>();
			var nodesProviderFactory = new DocumentOutlineNodesProviderFactory(languageServices, gameFlowLanguageServices, trxLanguageServices);
			return new DocumentOutlineViewModel(localizationService, nodesProviderFactory);
		});
		services.AddScoped<ReferenceBrowserViewModel>(sp =>
		{
			var messageService = sp.GetRequiredService<IMessageService>();
			var localizationService = sp.GetRequiredService<ILocalizationService>();
			return new ReferenceBrowserViewModel(messageService, localizationService);
		});
		services.AddScoped<FileExplorerViewModel>(sp =>
		{
			var dialogService = sp.GetRequiredService<IDialogService>();
			var messageService = sp.GetRequiredService<IMessageService>();
			var localizationService = sp.GetRequiredService<ILocalizationService>();
			var dialogOwnerProvider = sp.GetRequiredService<IWin32DialogOwnerProvider>();
			return new FileExplorerViewModel(dialogService, messageService, localizationService, dialogOwnerProvider);
		});

		// Find and replace.
		services.AddScoped<FindReplaceService>();
		services.AddScoped<FindAndReplaceViewModel>(sp =>
		{
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var messenger = sp.GetRequiredService<IMessenger>();
			var service = sp.GetRequiredService<FindReplaceService>();
			return new FindAndReplaceViewModel(documentController, messenger, service);
		});

		// Reference info dialog.
		services.AddScoped<ReferenceInfoViewModel>(sp =>
		{
			var workbenchSettings = sp.GetRequiredService<ShellWorkbenchSettings>();
			return new ReferenceInfoViewModel(
				workbenchSettings.GetInfoBoxAlwaysOnTop,
				workbenchSettings.SetInfoBoxAlwaysOnTop,
				workbenchSettings.GetInfoBoxCloseTabsOnClose,
				workbenchSettings.SetInfoBoxCloseTabsOnClose);
		});

		services.AddScoped<IStudioPaneContributionProvider, CompilerLogsPaneProvider>();
		services.AddScoped<IStudioPaneContributionProvider, SearchResultsPaneProvider>();
		services.AddScoped<IStudioPaneContributionProvider>(sp =>
		{
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var viewModel = sp.GetRequiredService<DocumentOutlineViewModel>();
			return new DocumentOutlinePaneProvider(documentController, viewModel);
		});
		services.AddScoped<IStudioPaneContributionProvider>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var viewModel = sp.GetRequiredService<FileExplorerViewModel>();
			return new FileExplorerPaneProvider(profile, documentController, viewModel);
		});
		services.AddScoped<IStudioPaneContributionProvider>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var viewModel = sp.GetRequiredService<ReferenceBrowserViewModel>();
			var referenceInfoViewModel = sp.GetRequiredService<ReferenceInfoViewModel>();
			return new ReferenceBrowserPaneProvider(profile, viewModel, referenceInfoViewModel);
		});
		services.AddScoped<IStudioPaneContributionProvider, LuaDiagnosticsPaneProvider>();
		services.AddScoped<IStudioPaneContributionProvider, LuaReferencesPaneProvider>();
		services.AddScoped<PaneCatalog>();

		// Editor document controller factory.
		services.AddScoped<IEditorDocumentControllerFactory, EditorDocumentControllerFactory>();

		// Editor document controller (created once per scope via factory).
		services.AddScoped<IEditorDocumentController>(sp =>
		{
			var factory = sp.GetRequiredService<IEditorDocumentControllerFactory>();
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var projectContext = sp.GetRequiredService<IScriptingProjectContext>();
			var messageService = sp.GetRequiredService<IMessageService>();
			var languageServices = sp.GetRequiredService<ClassicScriptLanguageServices>();
			return factory.Create(profile, projectContext, messageService, languageServices.LineService);
		});

		// AvalonDock host adapter.
		services.AddScoped<IAvalonDockHost>(sp =>
		{
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var paneCatalog = sp.GetRequiredService<PaneCatalog>();
			var view = new StudioAvalonDockHostView(documentController, paneCatalog.Panes);
			return new AvalonDockHostAdapter(view);
		});

		// Workbench service (receives delegates from ShellWorkbenchSettings).
		services.AddScoped<WorkbenchService>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var projectContext = sp.GetRequiredService<IScriptingProjectContext>();
			var messenger = sp.GetRequiredService<IMessenger>();
			var messageService = sp.GetRequiredService<IMessageService>();
			var shortcutBindingService = sp.GetRequiredService<IShortcutBindingService>();
			var menuService = sp.GetRequiredService<IMenuService>();
			var toolBarService = sp.GetRequiredService<IToolBarService>();
			var statusBarService = sp.GetRequiredService<IStatusBarService>();
			var paneHostService = sp.GetRequiredService<IPaneHostService>();
			var dialogOwnerProvider = sp.GetRequiredService<IWin32DialogOwnerProvider>();
			var documentController = sp.GetRequiredService<IEditorDocumentController>();
			var dockHost = sp.GetRequiredService<IAvalonDockHost>();
			var paneCatalog = sp.GetRequiredService<PaneCatalog>();
			var findAndReplaceViewModel = sp.GetRequiredService<FindAndReplaceViewModel>();
			var luaEditorLifecycleService = sp.GetRequiredService<ILuaEditorLifecycleService>();
			var luaIntellisenseBridge = sp.GetRequiredService<ILuaIntellisenseBridge>();
			var luaTrackedDocumentStateService = sp.GetRequiredService<LuaTrackedDocumentStateService>();
			var workbenchSettings = sp.GetRequiredService<ShellWorkbenchSettings>();
			var languageServices = sp.GetRequiredService<ClassicScriptLanguageServices>();
			var gameFlowLanguageServices = sp.GetRequiredService<GameFlowLanguageServices>();
			var trxLanguageServices = sp.GetRequiredService<TRXLanguageServices>();

			return new WorkbenchService(
				profile,
				projectContext,
				messenger,
				messageService,
				shortcutBindingService,
				menuService,
				toolBarService,
				statusBarService,
				paneHostService,
				dialogOwnerProvider,
				documentController,
				dockHost,
				paneCatalog,
				findAndReplaceViewModel,
				luaEditorLifecycleService,
				luaIntellisenseBridge,
				luaTrackedDocumentStateService,
				workbenchSettings.ShowCompilerLogsAfterBuild,
				workbenchSettings.UseNewIncludeMethod,
				languageServices,
				gameFlowLanguageServices,
				trxLanguageServices);
		});
		services.AddScoped<IWorkbenchService>(sp => sp.GetRequiredService<WorkbenchService>());

		// Shell ViewModel (receives all interfaces via constructor injection).
		services.AddScoped<RootShellViewModel>(sp =>
		{
			var profile = sp.GetRequiredService<ScriptingWorkspaceProfile>();
			var projectContext = sp.GetRequiredService<IScriptingProjectContext>();
			var settingsStore = sp.GetRequiredService<IScriptingStudioShellSettingsStore>();
			var messenger = sp.GetRequiredService<IMessenger>();
			var messageService = sp.GetRequiredService<IMessageService>();
			var localizationService = sp.GetRequiredService<ILocalizationService>();
			var menuService = sp.GetRequiredService<IMenuService>();
			var toolBarService = sp.GetRequiredService<IToolBarService>();
			var statusBarService = sp.GetRequiredService<IStatusBarService>();
			var paneHostService = sp.GetRequiredService<IPaneHostService>();
			var workbenchService = sp.GetRequiredService<IWorkbenchService>();
			var workbenchSettings = sp.GetRequiredService<ShellWorkbenchSettings>();
			var languageServices = sp.GetRequiredService<ClassicScriptLanguageServices>();
			var gameFlowLanguageServices = sp.GetRequiredService<GameFlowLanguageServices>();
			var trxLanguageServices = sp.GetRequiredService<TRXLanguageServices>();

			return new RootShellViewModel(
				profile,
				projectContext,
				settingsStore,
				messenger,
				messageService,
				localizationService,
				menuService,
				toolBarService,
				statusBarService,
				paneHostService,
				workbenchService,
				workbenchSettings,
				languageServices,
				gameFlowLanguageServices,
				trxLanguageServices);
		});
	}

	private static StudioCommandCatalog CreateDefaultCommandCatalog(IServiceProvider _)
	{
		var descriptors = new StudioCommandDescriptor[]
		{
			new(UICommand.NewFile, nameof(UICommand.NewFile), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.N, ModifierKeys.Control)),
			new(UICommand.Save, nameof(UICommand.Save), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.S, ModifierKeys.Control)),
			new(UICommand.SaveAll, nameof(UICommand.SaveAll), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.S, ModifierKeys.Control | ModifierKeys.Shift)),
			new(UICommand.Build, nameof(UICommand.Build), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.F9, ModifierKeys.None)),
			new(UICommand.Exit, nameof(UICommand.Exit), isRemappable: false, isHostReserved: true,
				new ShortcutKey(Key.F4, ModifierKeys.Alt)),
			new(UICommand.Undo, nameof(UICommand.Undo), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.Z, ModifierKeys.Control)),
			new(UICommand.Redo, nameof(UICommand.Redo), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.Y, ModifierKeys.Control)),
			new(UICommand.Cut, nameof(UICommand.Cut), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.X, ModifierKeys.Control)),
			new(UICommand.Copy, nameof(UICommand.Copy), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.C, ModifierKeys.Control)),
			new(UICommand.Paste, nameof(UICommand.Paste), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.V, ModifierKeys.Control)),
			new(UICommand.Find, nameof(UICommand.Find), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.F, ModifierKeys.Control),
				new ShortcutKey(Key.H, ModifierKeys.Control)),
			new(UICommand.SelectAll, nameof(UICommand.SelectAll), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.A, ModifierKeys.Control)),
			new(UICommand.Reindent, nameof(UICommand.Reindent), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.R, ModifierKeys.Control)),
			new(UICommand.TrimWhiteSpace, nameof(UICommand.TrimWhiteSpace), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.R, ModifierKeys.Control | ModifierKeys.Shift)),
			new(UICommand.ToggleComment, nameof(UICommand.ToggleComment), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.OemQuestion, ModifierKeys.Control)),
			new(UICommand.CommentOut, nameof(UICommand.CommentOut), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.C, ModifierKeys.Control | ModifierKeys.Shift)),
			new(UICommand.Uncomment, nameof(UICommand.Uncomment), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.U, ModifierKeys.Control | ModifierKeys.Shift)),
			new(UICommand.ToggleBookmark, nameof(UICommand.ToggleBookmark), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.B, ModifierKeys.Control)),
			new(UICommand.PrevBookmark, nameof(UICommand.PrevBookmark), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.OemComma, ModifierKeys.Control)),
			new(UICommand.NextBookmark, nameof(UICommand.NextBookmark), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.OemPeriod, ModifierKeys.Control)),
			new(UICommand.ClearBookmarks, nameof(UICommand.ClearBookmarks), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.B, ModifierKeys.Control | ModifierKeys.Shift)),
			new(UICommand.PrevSection, nameof(UICommand.PrevSection), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.Left, ModifierKeys.Control)),
			new(UICommand.NextSection, nameof(UICommand.NextSection), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.Right, ModifierKeys.Control)),
			new(UICommand.ClearString, nameof(UICommand.ClearString), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.Delete, ModifierKeys.None)),
			new(UICommand.RemoveLastString, nameof(UICommand.RemoveLastString), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.Delete, ModifierKeys.Control)),
			new(UICommand.NavigateBack, nameof(UICommand.NavigateBack), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.Left, ModifierKeys.Alt)),
			new(UICommand.NavigateForward, nameof(UICommand.NavigateForward), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.Right, ModifierKeys.Alt)),
			new(UICommand.GoToDefinition, nameof(UICommand.GoToDefinition), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.F12, ModifierKeys.None)),
			new(UICommand.FindReferences, nameof(UICommand.FindReferences), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.F12, ModifierKeys.Shift)),
			new(UICommand.RenameSymbol, nameof(UICommand.RenameSymbol), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.F2, ModifierKeys.None)),
			new(UICommand.TypeFirstAvailableId, nameof(UICommand.TypeFirstAvailableId), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.F1, ModifierKeys.None)),
			new(UICommand.NewFileAtCaret, nameof(UICommand.NewFileAtCaret), isRemappable: true, isHostReserved: false,
				new ShortcutKey(Key.F5, ModifierKeys.Control))
		};

		return new StudioCommandCatalog(descriptors);
	}
}
