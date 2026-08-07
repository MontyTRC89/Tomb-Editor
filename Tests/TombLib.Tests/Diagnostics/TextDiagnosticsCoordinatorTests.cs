using Nickelony.LanguageServer.Abstractions.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.UI.Diagnostics;

namespace TombLib.Tests;

[TestClass]
public class TextDiagnosticsCoordinatorTests
{
	[TestMethod]
	public void ErrorDetectionWorker_SurfacesDetectorFailureThroughCompletedEvent()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var worker = new ErrorDetectionWorker(new ThrowingErrorDetector(), new Version(1, 0), TimeSpan.FromMilliseconds(50.0));
			RunWorkerCompletedEventArgs? completedArgs = null;
			worker.RunWorkerCompleted += (_, e) => completedArgs = e;

			worker.CheckForErrorsAsync("content");

			PumpUntil(() => completedArgs is not null);

			Assert.IsNotNull(completedArgs);
			Assert.IsNotNull(completedArgs.Error);
			Assert.IsFalse(worker.IsBusy);
		});
	}

	[TestMethod]
	public void ErrorDetectionWorker_SurfacesProviderFailureThroughCompletedEvent()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var worker = new ErrorDetectionWorker(null, new Version(1, 0), TimeSpan.FromMilliseconds(50.0))
			{
				DiagnosticsProvider = new ThrowingDiagnosticsProvider()
			};
			RunWorkerCompletedEventArgs? completedArgs = null;
			worker.RunWorkerCompleted += (_, e) => completedArgs = e;

			worker.CheckForErrorsAsync("content");

			PumpUntil(() => completedArgs is not null);

			Assert.IsNotNull(completedArgs);
			Assert.IsNotNull(completedArgs.Error);
			Assert.IsFalse(worker.IsBusy);
		});
	}

	[TestMethod]
	public void Coordinator_KeepsLastKnownDiagnostics_WhenDetectorThrows()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var initialDiagnostics = new[] { new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Warning, "existing", 0, 4) };
				editor.SetDiagnostics(initialDiagnostics);

				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), new ThrowingErrorDetector());
				coordinator.CheckAsync("Name=Level1");

				PumpUntil(() => !coordinator.IsBusy);

				// The failing detector must not replace the editor's last-known diagnostics.
				Assert.AreEqual(1, editor.Diagnostics.Count);
				Assert.AreEqual("existing", editor.Diagnostics[0].Message);
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

	private static void PumpUntil(Func<bool> condition)
	{
		Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
		int attempts = 0;

		while (!condition() && attempts++ < 5000)
			dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
	}

	private sealed class ThrowingErrorDetector : IErrorDetector
	{
		public IReadOnlyList<TextEditorDiagnostic> FindErrors(string editorContent, Version engineVersion)
			=> throw new InvalidOperationException("Detector failed.");
	}

	private sealed class ThrowingDiagnosticsProvider : ITextDiagnosticsProvider
	{
		public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(TextDiagnosticsRequest request)
			=> throw new InvalidOperationException("Provider failed.");
	}
}
