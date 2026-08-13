using CommunityToolkit.Mvvm.Messaging;
using Moq;
using System.ComponentModel;
using System.Windows;
using TombIDE.ScriptingStudio.Composition;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.TRX;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public class RootShellViewModelTests
{
    private static readonly ClassicScriptLanguageServices ClassicLanguageServices = ScriptingLanguageServicesTestFactory.CreateClassicScript();
    private static readonly GameFlowLanguageServices GameFlowLanguageServices = ScriptingLanguageServicesTestFactory.CreateGameFlowScript();
    private static readonly TRXLanguageServices TrxLanguageServices = ScriptingLanguageServicesTestFactory.CreateTRX();

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

    private static RootShellViewModel CreateViewModel(
        ScriptingWorkspaceProfile? profile = null,
        IScriptingProjectContext? projectContext = null,
        IScriptingStudioShellSettingsStore? settingsStore = null,
        IMessageService? messageService = null,
        ILocalizationService? localizationService = null,
        IMenuService? menuService = null,
        IToolBarService? toolBarService = null,
        IStatusBarService? statusBarService = null,
        IPaneHostService? paneHostService = null,
        IWorkbenchService? workbenchService = null,
        ShellWorkbenchSettings? workbenchSettings = null)
    {
        profile ??= CreateLuaProfile();
        settingsStore ??= CreateSettingsStore(profile);
        messageService ??= new Mock<IMessageService>().Object;
        localizationService ??= CreateLocalizationService();
        projectContext ??= CreateProjectContext();

        menuService ??= CreateMenuServiceMock().Object;
        toolBarService ??= CreateToolBarServiceMock().Object;
        statusBarService ??= CreateStatusBarServiceMock().Object;
        paneHostService ??= CreatePaneHostServiceMock().Object;
        workbenchService ??= CreateWorkbenchServiceMock().Object;
        workbenchSettings ??= new ShellWorkbenchSettings();

        return new RootShellViewModel(
            profile,
            projectContext,
            settingsStore,
            new Mock<IMessenger>().Object,
            messageService,
            localizationService,
            menuService,
            toolBarService,
            statusBarService,
            paneHostService,
            workbenchService,
            workbenchSettings,
            ClassicLanguageServices,
            GameFlowLanguageServices,
            TrxLanguageServices);
    }

    private static Mock<IMenuService> CreateMenuServiceMock()
    {
        var mock = new Mock<IMenuService>();
        mock.Setup(m => m.MenuView).Returns(Mock.Of<FrameworkElement>());
        return mock;
    }

    private static Mock<IToolBarService> CreateToolBarServiceMock()
    {
        var mock = new Mock<IToolBarService>();
        mock.Setup(m => m.ToolBarView).Returns(Mock.Of<FrameworkElement>());
        return mock;
    }

    private static Mock<IStatusBarService> CreateStatusBarServiceMock()
    {
        var mock = new Mock<IStatusBarService>();
        mock.Setup(m => m.StatusBarView).Returns(Mock.Of<FrameworkElement>());
        return mock;
    }

    private static Mock<IPaneHostService> CreatePaneHostServiceMock() => new();

    private static Mock<IWorkbenchService> CreateWorkbenchServiceMock()
    {
        var mock = new Mock<IWorkbenchService>();
        mock.Setup(m => m.WorkbenchView).Returns(Mock.Of<FrameworkElement>());
        mock.Setup(m => m.CaptureLayout()).Returns(string.Empty);
        return mock;
    }

    private static IScriptingStudioShellSettingsStore CreateSettingsStore(ScriptingWorkspaceProfile profile)
    {
        var storeMock = new Mock<IScriptingStudioShellSettingsStore>();
        var defaults = new ScriptingStudioShellWorkspaceSettings();

        storeMock
            .Setup(s => s.Load(It.IsAny<ScriptingWorkspaceProfile>()))
            .Returns(new ScriptingStudioShellWorkspaceSettings());
        storeMock
            .Setup(s => s.CreateDefault(It.IsAny<ScriptingWorkspaceProfile>()))
            .Returns(defaults);

        return storeMock.Object;
    }

    private static ILocalizationService CreateLocalizationService()
    {
        var localizationMock = new Mock<ILocalizationService>();
        localizationMock
            .Setup(l => l.WithKeysFor(It.IsAny<INotifyPropertyChanged>()))
            .Returns(localizationMock.Object);
        localizationMock
            .Setup(l => l[It.IsAny<string>()])
            .Returns("LocalizedText");

        return localizationMock.Object;
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

    [TestMethod]
    public void Constructor_WithValidArguments_CreatesViewModel()
    {
        StaTestHelper.RunInSta(() =>
        {
            var menuServiceMock = CreateMenuServiceMock();
            var viewModel = CreateViewModel(menuService: menuServiceMock.Object);

            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.MenuView);
            Assert.IsNotNull(viewModel.ToolBarView);
            Assert.IsNotNull(viewModel.StatusBarView);
            Assert.IsNotNull(viewModel.WorkbenchView);

            menuServiceMock.VerifyAdd(m => m.CommandInvoked += It.IsAny<EventHandler<TombIDE.ScriptingStudio.ToolStrips.StudioCommandInvokedEventArgs>>(), Times.Once);
        });
    }

    [TestMethod]
    public void Constructor_WithNullWorkspaceProfile_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new RootShellViewModel(
                    null!,
                    CreateProjectContext(),
                    CreateSettingsStore(CreateLuaProfile()),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateLocalizationService(),
                    CreateMenuServiceMock().Object,
                    CreateToolBarServiceMock().Object,
                    CreateStatusBarServiceMock().Object,
                    CreatePaneHostServiceMock().Object,
                    CreateWorkbenchServiceMock().Object,
                    new ShellWorkbenchSettings(),
                    ClassicLanguageServices,
                    GameFlowLanguageServices,
                    TrxLanguageServices));
        });
    }

    [TestMethod]
    public void Constructor_WithNullMessenger_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new RootShellViewModel(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    CreateSettingsStore(CreateLuaProfile()),
                    null!,
                    new Mock<IMessageService>().Object,
                    CreateLocalizationService(),
                    CreateMenuServiceMock().Object,
                    CreateToolBarServiceMock().Object,
                    CreateStatusBarServiceMock().Object,
                    CreatePaneHostServiceMock().Object,
                    CreateWorkbenchServiceMock().Object,
                    new ShellWorkbenchSettings(),
                    ClassicLanguageServices,
                    GameFlowLanguageServices,
                    TrxLanguageServices));
        });
    }

    [TestMethod]
    public void Constructor_WithNullMenuService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new RootShellViewModel(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    CreateSettingsStore(CreateLuaProfile()),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateLocalizationService(),
                    null!,
                    CreateToolBarServiceMock().Object,
                    CreateStatusBarServiceMock().Object,
                    CreatePaneHostServiceMock().Object,
                    CreateWorkbenchServiceMock().Object,
                    new ShellWorkbenchSettings(),
                    ClassicLanguageServices,
                    GameFlowLanguageServices,
                    TrxLanguageServices));
        });
    }

    [TestMethod]
    public void Constructor_WithNullWorkbenchService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new RootShellViewModel(
                    CreateLuaProfile(),
                    CreateProjectContext(),
                    CreateSettingsStore(CreateLuaProfile()),
                    new Mock<IMessenger>().Object,
                    new Mock<IMessageService>().Object,
                    CreateLocalizationService(),
                    CreateMenuServiceMock().Object,
                    CreateToolBarServiceMock().Object,
                    CreateStatusBarServiceMock().Object,
                    CreatePaneHostServiceMock().Object,
                    null!,
                    new ShellWorkbenchSettings(),
                    ClassicLanguageServices,
                    GameFlowLanguageServices,
                    TrxLanguageServices));
        });
    }

    [TestMethod]
    public void Dispose_DisposesAllServices()
    {
        StaTestHelper.RunInSta(() =>
        {
            var menuServiceMock = CreateMenuServiceMock();
            var toolBarServiceMock = CreateToolBarServiceMock();
            var statusBarServiceMock = CreateStatusBarServiceMock();
            var paneHostServiceMock = CreatePaneHostServiceMock();
            var workbenchServiceMock = CreateWorkbenchServiceMock();

            var viewModel = CreateViewModel(
                menuService: menuServiceMock.Object,
                toolBarService: toolBarServiceMock.Object,
                statusBarService: statusBarServiceMock.Object,
                paneHostService: paneHostServiceMock.Object,
                workbenchService: workbenchServiceMock.Object);

            viewModel.Dispose();

            workbenchServiceMock.Verify(w => w.Dispose(), Times.Once);
            menuServiceMock.Verify(m => m.Dispose(), Times.Once);
            toolBarServiceMock.Verify(t => t.Dispose(), Times.Once);
            statusBarServiceMock.Verify(s => s.Dispose(), Times.Once);
            paneHostServiceMock.Verify(p => p.Dispose(), Times.Once);
        });
    }
}
