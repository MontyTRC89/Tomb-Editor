using Moq;
using MvvmDialogs;
using System.ComponentModel;
using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.DocumentOutline;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.Shell;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public class ViewModelTests
{
    // ── ReferenceBrowserViewModel ──

    [TestMethod]
    public void ReferenceBrowserViewModel_WithNullMessageService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new ReferenceBrowserViewModel(
                    null!,
                    CreateLocalizationService()));
        });
    }

    [TestMethod]
    public void ReferenceBrowserViewModel_WithNullLocalizationService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new ReferenceBrowserViewModel(
                    CreateMessageService(),
                    null!));
        });
    }

    [TestMethod]
    public void ReferenceBrowserViewModel_WithValidDependencies_InitializesCategories()
    {
        StaTestHelper.RunInSta(() =>
        {
            var viewModel = new ReferenceBrowserViewModel(
                CreateMessageService(),
                CreateLocalizationService());

            Assert.IsNotNull(viewModel.Categories);
            Assert.IsTrue(viewModel.Categories.Count > 0);
            Assert.IsNotNull(viewModel.SelectedCategory);
        });
    }

    // ── DocumentOutlineViewModel ──

    [TestMethod]
    public void DocumentOutlineViewModel_WithNullLocalizationService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new DocumentOutlineViewModel(null!));
        });
    }

    [TestMethod]
    public void DocumentOutlineViewModel_WithValidDependencies_HasEmptyNodes()
    {
        StaTestHelper.RunInSta(() =>
        {
            var viewModel = new DocumentOutlineViewModel(
                CreateLocalizationService());

            Assert.IsNotNull(viewModel.Nodes);
            Assert.AreEqual(0, viewModel.Nodes.Count);
            Assert.IsTrue(viewModel.IsEmpty);
        });
    }

    // ── FileExplorerViewModel ──

    [TestMethod]
    public void FileExplorerViewModel_WithNullDialogService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new FileExplorerViewModel(
                    null!,
                    CreateMessageService(),
                    CreateLocalizationService(),
                    CreateDialogOwnerProvider()));
        });
    }

    [TestMethod]
    public void FileExplorerViewModel_WithNullMessageService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new FileExplorerViewModel(
                    CreateDialogService(),
                    null!,
                    CreateLocalizationService(),
                    CreateDialogOwnerProvider()));
        });
    }

    [TestMethod]
    public void FileExplorerViewModel_WithNullLocalizationService_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new FileExplorerViewModel(
                    CreateDialogService(),
                    CreateMessageService(),
                    null!,
                    CreateDialogOwnerProvider()));
        });
    }

    [TestMethod]
    public void FileExplorerViewModel_WithNullDialogOwnerProvider_ThrowsArgumentNullException()
    {
        StaTestHelper.RunInSta(() =>
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new FileExplorerViewModel(
                    CreateDialogService(),
                    CreateMessageService(),
                    CreateLocalizationService(),
                    null!));
        });
    }

    [TestMethod]
    public void FileExplorerViewModel_WithValidDependencies_HasEmptyRootNodes()
    {
        StaTestHelper.RunInSta(() =>
        {
            var viewModel = new FileExplorerViewModel(
                CreateDialogService(),
                CreateMessageService(),
                CreateLocalizationService(),
                CreateDialogOwnerProvider());

            Assert.IsNotNull(viewModel.RootNodes);
            Assert.AreEqual(0, viewModel.RootNodes.Count);
            Assert.IsTrue(viewModel.IsEmpty);
        });
    }

    // ── Helpers ──

    private static IDialogService CreateDialogService()
        => new Mock<IDialogService>().Object;

    private static IMessageService CreateMessageService()
        => new Mock<IMessageService>().Object;

    private static ILocalizationService CreateLocalizationService()
    {
        var mock = new Mock<ILocalizationService>();
        mock.Setup(m => m[It.IsAny<string>()]).Returns((string key) => key);
        mock.Setup(m => m.Format(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) => string.Format(key, args));
        mock.Setup(m => m.WithKeysFor(It.IsAny<INotifyPropertyChanged>())).Returns(mock.Object);
        return mock.Object;
    }

    private static IWin32DialogOwnerProvider CreateDialogOwnerProvider()
        => new Mock<IWin32DialogOwnerProvider>().Object;
}
