using CommunityToolkit.Mvvm.Messaging;
using Moq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.Shortcuts;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Documents;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.Hover;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Services;
using Nickelony.LanguageServer.Lua;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.Signatures;
using TombLib.Scripting.UI.Editors;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public class WorkbenchServiceTests
{
    private static ScriptingWorkspaceProfile CreateLuaProfile()
    {
        return new ScriptingWorkspaceProfile(
            ScriptingWorkspaceKind.Lua,
            TRVersion.Game.TombEngine,
            [],
            string.Empty,
            [],
            [],
            [],
            [],
            [],
            "*.lua",
            string.Empty,
            "--",
            supportsBuild: false,
            supportsDocumentation: false,
            new DockPanelState(),
            _ => { },
            () => new DockPanelState(),
            () => string.Empty,
            _ => { });
    }

    private static IMenuService CreateMenuServiceMock()
    {
        var mock = new Mock<IMenuService>();
        mock.Setup(m => m.MenuView).Returns(Mock.Of<System.Windows.FrameworkElement>());
        return mock.Object;
    }

    private static IToolBarService CreateToolBarServiceMock()
    {
        var mock = new Mock<IToolBarService>();
        mock.Setup(m => m.ToolBarView).Returns(Mock.Of<System.Windows.FrameworkElement>());
        return mock.Object;
    }

    private static IStatusBarService CreateStatusBarServiceMock()
    {
        var mock = new Mock<IStatusBarService>();
        mock.Setup(m => m.StatusBarView).Returns(Mock.Of<System.Windows.FrameworkElement>());
        return mock.Object;
    }

    private static IScriptingProjectContext CreateProjectContext()
    {
        var contextMock = new Mock<IScriptingProjectContext>();
        contextMock.Setup(c => c.ScriptRootDirectoryPath).Returns("C:\\Scripts");

        var projectMock = new Mock<IGameProject>();
        projectMock.Setup(p => p.GetCurrentEngineVersion()).Returns(new Version(1, 0));
        projectMock.Setup(p => p.GetEngineRootDirectoryPath()).Returns("C:\\Engine");
        projectMock.Setup(p => p.GetEngineExecutableFilePath()).Returns("C:\\Engine\\Game.exe");
        contextMock.Setup(c => c.Project).Returns(projectMock.Object);

        return contextMock.Object;
    }

    private static IShortcutBindingService CreateShortcutBindingService()
    {
        return new ShortcutBindingService(
            CreateDefaultCatalog(),
            new ShortcutOverrideCollection(),
            _ => true);
    }

    private static StudioCommandCatalog CreateDefaultCatalog()
    {
        return new StudioCommandCatalog([
            new StudioCommandDescriptor(UICommand.Save, nameof(UICommand.Save), isRemappable: true, isHostReserved: false,
                new ShortcutKey(System.Windows.Input.Key.S, System.Windows.Input.ModifierKeys.Control)),
            new StudioCommandDescriptor(UICommand.Find, nameof(UICommand.Find), isRemappable: true, isHostReserved: false,
                new ShortcutKey(System.Windows.Input.Key.F, System.Windows.Input.ModifierKeys.Control),
                new ShortcutKey(System.Windows.Input.Key.H, System.Windows.Input.ModifierKeys.Control))
        ]);
    }

    private static IEditorDocumentController CreateDocumentControllerMock()
    {
        var mock = new Mock<IEditorDocumentController>();
        mock.Setup(c => c.GetOpenEditors()).Returns([]);
        return mock.Object;
    }

    private static IAvalonDockHost CreateDockHostMock()
    {
        var mock = new Mock<IAvalonDockHost>();
        mock.Setup(d => d.View).Returns(Mock.Of<System.Windows.FrameworkElement>());
        mock.Setup(d => d.Dispatcher).Returns(System.Windows.Threading.Dispatcher.CurrentDispatcher);
        return mock.Object;
    }

    private static PaneCatalog CreatePaneCatalog()
    {
        return new PaneCatalog([]);
    }

    private static FindAndReplaceViewModel CreateFindAndReplaceViewModel()
    {
        var messenger = new Mock<IMessenger>().Object;
        var documentController = CreateDocumentControllerMock();
        return new FindAndReplaceViewModel(documentController, messenger, new FindReplaceService());
    }

    private static ILuaEditorLifecycleService CreateLuaEditorLifecycleServiceMock()
    {
        return new Mock<ILuaEditorLifecycleService>().Object;
    }

    private static ILuaIntellisenseBridge CreateLuaIntellisenseBridgeMock()
    {
        return new Mock<ILuaIntellisenseBridge>().Object;
    }

    private static LuaTrackedDocumentStateService CreateLuaTrackedDocumentStateService()
    {
        return new LuaTrackedDocumentStateService(
            new Mock<ITextEditorHost>().Object,
            new Mock<ILuaIntellisenseProvider>().Object);
    }

    private static ClassicScriptLanguageServices CreateClassicScriptLanguageServices()
    {
        return new ClassicScriptLanguageServices(
            new Mock<ITextDefinitionProvider>().Object,
            new Mock<ITextHoverProvider>().Object,
            new Mock<ITextSignatureHelpProvider>().Object,
            new ErrorDetector(
                new Mock<IClassicScriptLineService>().Object,
                new Mock<IClassicScriptCommandService>().Object,
                new ClassicScriptSyntaxCatalogService()),
            new Mock<IClassicScriptLineService>().Object,
            new Mock<IClassicScriptCommandService>().Object,
            new Mock<IClassicScriptIndexService>().Object);
    }

    private static GameFlowLanguageServices CreateGameFlowLanguageServices()
    {
        var lineService = new Mock<IGameFlowScriptLineService>().Object;
        var documentService = new Mock<IGameFlowScriptDocumentService>().Object;

        return new GameFlowLanguageServices(
            new Mock<ITextDefinitionProvider>().Object,
            new Mock<ITextHoverProvider>().Object,
            new GameFlowAutocompleteService(lineService),
            lineService,
            documentService,
            new GameFlowDocumentLookupService(documentService));
    }

    private static TRXLanguageServices CreateTRXLanguageServices()
    {
        var lineService = new TRXLineService();
        var documentService = new TRXDocumentService(lineService);
        var schemaService = new GameflowSchemaService(TrxResourcePaths.GetGameflowSchemaPath());

        return new TRXLanguageServices(schemaService, lineService, documentService);
    }

    [TestMethod]
    public void Constructor_WithNullWorkspaceProfile_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new WorkbenchService(
                    null!,
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateShortcutBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    () => false,
                    () => false,
                    CreateClassicScriptLanguageServices(),
                    CreateGameFlowLanguageServices(),
                    CreateTRXLanguageServices()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullProjectContext_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new WorkbenchService(
                    CreateLuaProfile(),
                    null!,
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateShortcutBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    () => false,
                    () => false,
                    CreateClassicScriptLanguageServices(),
                    CreateGameFlowLanguageServices(),
                    CreateTRXLanguageServices()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullMessenger_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new WorkbenchService(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    null!,
                    new Mock<IMessageService>().Object,
                    CreateShortcutBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    () => false,
                    () => false,
                    CreateClassicScriptLanguageServices(),
                    CreateGameFlowLanguageServices(),
                    CreateTRXLanguageServices()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullMessageService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new WorkbenchService(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    null!,
                    CreateShortcutBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    () => false,
                    () => false,
                    CreateClassicScriptLanguageServices(),
                    CreateGameFlowLanguageServices(),
                    CreateTRXLanguageServices()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullDocumentController_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new WorkbenchService(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateShortcutBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    null!,
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    () => false,
                    () => false,
                    CreateClassicScriptLanguageServices(),
                    CreateGameFlowLanguageServices(),
                    CreateTRXLanguageServices()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullDockHost_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new WorkbenchService(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateShortcutBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    null!,
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    () => false,
                    () => false,
                    CreateClassicScriptLanguageServices(),
                    CreateGameFlowLanguageServices(),
                    CreateTRXLanguageServices()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullPaneCatalog_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new WorkbenchService(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateShortcutBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    null!,
                    CreateFindAndReplaceViewModel(),
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    () => false,
                    () => false,
                    CreateClassicScriptLanguageServices(),
                    CreateGameFlowLanguageServices(),
                    CreateTRXLanguageServices()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullLuaEditorLifecycleService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new WorkbenchService(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateShortcutBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    null!,
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    () => false,
                    () => false,
                    CreateClassicScriptLanguageServices(),
                    CreateGameFlowLanguageServices(),
                    CreateTRXLanguageServices()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullLuaIntellisenseBridge_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new WorkbenchService(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateShortcutBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaEditorLifecycleServiceMock(),
                    null!,
                    CreateLuaTrackedDocumentStateService(),
                    () => false,
                    () => false,
                    CreateClassicScriptLanguageServices(),
                    CreateGameFlowLanguageServices(),
                    CreateTRXLanguageServices()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullLuaTrackedDocumentStateService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new WorkbenchService(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateShortcutBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    null!,
                    () => false,
                    () => false,
                    CreateClassicScriptLanguageServices(),
                    CreateGameFlowLanguageServices(),
                    CreateTRXLanguageServices()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullLanguageServices_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new WorkbenchService(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateShortcutBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    () => false,
                    () => false,
                    null!,
                    CreateGameFlowLanguageServices(),
                    CreateTRXLanguageServices()));
        });
    }

    [TestMethod]
    public void Constructor_WithNullGameFlowLanguageServices_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new WorkbenchService(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateShortcutBindingService(),
                    CreateMenuServiceMock(),
                    CreateToolBarServiceMock(),
                    CreateStatusBarServiceMock(),
                    new Mock<IPaneHostService>().Object,
                    new Mock<IWin32DialogOwnerProvider>().Object,
                    CreateDocumentControllerMock(),
                    CreateDockHostMock(),
                    CreatePaneCatalog(),
                    CreateFindAndReplaceViewModel(),
                    CreateLuaEditorLifecycleServiceMock(),
                    CreateLuaIntellisenseBridgeMock(),
                    CreateLuaTrackedDocumentStateService(),
                    () => false,
                    () => false,
                    CreateClassicScriptLanguageServices(),
                    null!,
                    CreateTRXLanguageServices()));
        });
    }
}
