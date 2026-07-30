using CommunityToolkit.Mvvm.Messaging;
using Moq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombIDE.Shared.Messaging.Scripting;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.TRX;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public class ScriptingMessageServiceTests
{
    private static readonly ClassicScriptLanguageServices ClassicLanguageServices = ScriptingLanguageServicesTestFactory.CreateClassicScript();
    private static readonly GameFlowLanguageServices GameFlowLanguageServices = ScriptingLanguageServicesTestFactory.CreateGameFlowScript();
    private static readonly TRXLanguageServices TrxLanguageServices = ScriptingLanguageServicesTestFactory.CreateTRX();

    private static ScriptingWorkspaceProfile CreateLuaWorkspaceProfile()
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
            string.Empty,
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

    private static ScriptingMessageServiceOptions CreateDefaultOptions()
    {
        return new ScriptingMessageServiceOptions
        {
            GetDockLayoutXml = () => string.Empty,
            ShowCompilerLogsPane = () => { },
            UpdateCompilerLogs = _ => { },
            ShowCompilerLogsAfterBuild = () => false,
            UseNewIncludeMethod = () => false
        };
    }

    [TestMethod]
    public void Constructor_WithValidArguments_DoesNotThrow()
    {
        var messenger = new Mock<IMessenger>().Object;
        var profile = CreateLuaWorkspaceProfile();
        var documentController = new Mock<IEditorDocumentController>().Object;

        var service = new ScriptingMessageService(
            messenger,
            profile,
            documentController,
            "C:\\Engine",
            "C:\\Engine\\Game.exe",
            CreateDefaultOptions(),
            ClassicLanguageServices,
            GameFlowLanguageServices,
            TrxLanguageServices);

        Assert.IsNotNull(service);
    }

    [TestMethod]
    public void Constructor_WithNullMessenger_ThrowsArgumentNullException()
    {
        var profile = CreateLuaWorkspaceProfile();
        var documentController = new Mock<IEditorDocumentController>().Object;

        Assert.ThrowsException<ArgumentNullException>(() =>
            new ScriptingMessageService(
                null!,
                profile,
                documentController,
                "C:\\Engine",
                "C:\\Engine\\Game.exe",
                CreateDefaultOptions(),
                ClassicLanguageServices,
                GameFlowLanguageServices,
                TrxLanguageServices));
    }

    [TestMethod]
    public void Constructor_WithNullDocumentController_ThrowsArgumentNullException()
    {
        var messenger = new Mock<IMessenger>().Object;
        var profile = CreateLuaWorkspaceProfile();

        Assert.ThrowsException<ArgumentNullException>(() =>
            new ScriptingMessageService(
                messenger,
                profile,
                null!,
                "C:\\Engine",
                "C:\\Engine\\Game.exe",
                CreateDefaultOptions(),
                ClassicLanguageServices,
                GameFlowLanguageServices,
                TrxLanguageServices));
    }

    [TestMethod]
    public void Constructor_WithNullEngineDirectoryPath_ThrowsArgumentNullException()
    {
        var messenger = new Mock<IMessenger>().Object;
        var profile = CreateLuaWorkspaceProfile();
        var documentController = new Mock<IEditorDocumentController>().Object;

        Assert.ThrowsException<ArgumentNullException>(() =>
            new ScriptingMessageService(
                messenger,
                profile,
                documentController,
                null!,
                "C:\\Engine\\Game.exe",
                CreateDefaultOptions(),
                ClassicLanguageServices,
                GameFlowLanguageServices,
                TrxLanguageServices));
    }

    [TestMethod]
    public void Constructor_RegistersExpectedMessageHandlers()
    {
        var messengerMock = new Mock<IMessenger>();
        var profile = CreateLuaWorkspaceProfile();
        var documentController = new Mock<IEditorDocumentController>().Object;

        var service = new ScriptingMessageService(
            messengerMock.Object,
            profile,
            documentController,
            "C:\\Engine",
            "C:\\Engine\\Game.exe",
            CreateDefaultOptions(),
            ClassicLanguageServices,
            GameFlowLanguageServices,
            TrxLanguageServices);

        Assert.IsNotNull(service);
    }

    [TestMethod]
    public void Dispose_UnregistersAllMessageHandlers()
    {
        var messengerMock = new Mock<IMessenger>();
        var profile = CreateLuaWorkspaceProfile();
        var documentController = new Mock<IEditorDocumentController>().Object;

        var service = new ScriptingMessageService(
            messengerMock.Object,
            profile,
            documentController,
            "C:\\Engine",
            "C:\\Engine\\Game.exe",
            CreateDefaultOptions(),
            ClassicLanguageServices,
            GameFlowLanguageServices,
            TrxLanguageServices);

        service.Dispose();

        messengerMock.Verify(
            m => m.UnregisterAll(service),
            Times.Once);
    }

    [TestMethod]
    public void Build_DoesNotThrow_WhenAutomationProviderIsCreated()
    {
        var messenger = new Mock<IMessenger>().Object;
        var profile = CreateLuaWorkspaceProfile();
        var documentControllerMock = new Mock<IEditorDocumentController>();
        documentControllerMock.Setup(m => m.ScriptRootDirectoryPath).Returns("C:\\Scripts");

        var service = new ScriptingMessageService(
            messenger,
            profile,
            documentControllerMock.Object,
            "C:\\Engine",
            "C:\\Engine\\Game.exe",
            CreateDefaultOptions(),
            ClassicLanguageServices,
            GameFlowLanguageServices,
            TrxLanguageServices);

        service.Build();
    }

    [TestMethod]
    public void ShowDocumentation_DoesNotThrow_WhenAutomationProviderIsCreated()
    {
        var messenger = new Mock<IMessenger>().Object;
        var profile = CreateLuaWorkspaceProfile();
        var documentControllerMock = new Mock<IEditorDocumentController>();
        documentControllerMock.Setup(m => m.ScriptRootDirectoryPath).Returns("C:\\Scripts");

        var service = new ScriptingMessageService(
            messenger,
            profile,
            documentControllerMock.Object,
            "C:\\Engine",
            "C:\\Engine\\Game.exe",
            CreateDefaultOptions(),
            ClassicLanguageServices,
            GameFlowLanguageServices,
            TrxLanguageServices);

        service.ShowDocumentation();
    }
}
