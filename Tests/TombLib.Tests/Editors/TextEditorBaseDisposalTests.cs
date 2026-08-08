using System;
using System.Windows;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.UI.Diagnostics;

namespace TombLib.Tests;

[TestClass]
public class TextEditorBaseDisposalTests
{
	[TestMethod]
	public void Dispose_CanBeCalledTwice_DoesNotThrow()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();
				editor.Dispose();
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void UpdateSettings_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				Assert.ThrowsException<ObjectDisposedException>(
					() => editor.UpdateSettings(new ClassicScriptEditorConfiguration()));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void ApplyPersistedContent_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				Assert.ThrowsException<ObjectDisposedException>(() => editor.ApplyPersistedContent("content"));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void Load_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				// The guard throws before any file I/O occurs.
				Assert.ThrowsException<ObjectDisposedException>(() => editor.Load("unused.tmp"));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void Content_Setter_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				Assert.ThrowsException<ObjectDisposedException>(() => editor.Content = "new content");
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryRunContentChangedWorker_AfterDispose_ThrowsObjectDisposedException()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.Dispose();

				Assert.ThrowsException<ObjectDisposedException>(() => editor.TryRunContentChangedWorker());
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void Dispose_StopsDiagnosticsWorker()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var coordinator = (TextDiagnosticsCoordinator?)WPFTestHelper.GetPrivateFieldValue(editor, "_diagnosticsCoordinator");
				Assert.IsNotNull(coordinator);

				editor.Dispose();

				// Post-dispose scheduling must be a no-op and must not start the worker timer.
				coordinator.RunOnIdle("new content");

				Assert.IsFalse(coordinator.IsBusy);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	private static ClassicScriptLanguageServices CreateLanguageServices()
	{
		var lineService = new ClassicScriptLineService();
		var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
		var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();
		var commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);
		var indexService = new ClassicScriptIndexService(commandService, lineService, mnemonicCatalogService);
		var errorDetector = new ErrorDetector(lineService, commandService, syntaxCatalogService);

		return new ClassicScriptLanguageServices(
			new ClassicScriptDefinitionProvider(commandService),
			new ClassicScriptHoverProvider(lineService, commandService, mnemonicCatalogService),
			new ClassicScriptSignatureHelpProvider(commandService),
			errorDetector,
			lineService,
			commandService,
			indexService);
	}
}
