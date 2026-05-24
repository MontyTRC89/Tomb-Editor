using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using TombLib.Scripting.Services;
using TombLib.Scripting.TRX;

namespace TombLib.Tests;

[TestClass]
public class Tomb1MainEditorToolTipTests
{
	[TestMethod]
	public void ShowMarkdownToolTip_OpensPopupAndForceCloseClearsContent()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new TRXEditor(new Version(1, 0));
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.ShowMarkdownToolTip("`Level` tooltip");
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				Popup popup = WPFTestHelper.GetPrivateField<Popup>(editor, "_specialToolTip");
				ContentPresenter presenter = WPFTestHelper.GetPrivateField<EditorToolTipPresenter>(editor, "_toolTipPresenter").ContentPresenter;

				Assert.IsTrue(popup.IsOpen);
				Assert.IsNotNull(presenter.Content);

				WPFTestHelper.InvokeInstanceMethod(editor, "CloseDefinitionToolTip", [typeof(bool)], true);

				Assert.IsFalse(popup.IsOpen);
				Assert.IsNull(presenter.Content);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}
}
