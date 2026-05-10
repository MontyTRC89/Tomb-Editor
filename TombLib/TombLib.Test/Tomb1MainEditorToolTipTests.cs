using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using TombLib.Scripting.Tomb1Main;

namespace TombLib.Test;

[TestClass]
public class Tomb1MainEditorToolTipTests
{
	[TestMethod]
	public void ShowMarkdownToolTip_OpensPopupAndForceCloseClearsContent()
	{
		WpfTestHelper.RunInSta(() =>
		{
			var editor = new Tomb1MainEditor(new Version(1, 0));
			Window hostWindow = WpfTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.ShowMarkdownToolTip("`Level` tooltip");
				WpfTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				Popup popup = WpfTestHelper.GetPrivateField<Popup>(editor, "_specialToolTip");
				ContentPresenter presenter = WpfTestHelper.GetPrivateField<ContentPresenter>(editor, "_specialToolTipPresenter");

				Assert.IsTrue(popup.IsOpen);
				Assert.IsNotNull(presenter.Content);

				WpfTestHelper.InvokeInstanceMethod(editor, "CloseDefinitionToolTip", [typeof(bool)], true);

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
