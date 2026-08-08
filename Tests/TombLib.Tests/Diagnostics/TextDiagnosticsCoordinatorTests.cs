using Nickelony.LanguageServer.Abstractions.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
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

			worker.RunErrorCheck("content");

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

			worker.RunErrorCheck("content");

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
				coordinator.RunErrorCheck("Name=Level1");

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

	[TestMethod]
	public void RunErrorCheck_WhenBusy_RetainsAndRunsLatestPendingRequest()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 1);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("first");
				PumpUntil(() => detector.CallCount >= 1);
				Assert.IsTrue(coordinator.IsBusy);
				Assert.AreEqual(1, detector.CallCount);

				// A check issued while the first is active must be retained, not dropped.
				coordinator.RunErrorCheck("second");
				detector.Release();

				PumpUntil(() => !coordinator.IsBusy && detector.CallCount >= 2);

				Assert.AreEqual(2, detector.CallCount);
				Assert.AreEqual("second", detector.Contents[detector.Contents.Count - 1]);
				Assert.AreEqual(1, editor.Diagnostics.Count);
				Assert.AreEqual("result:second", editor.Diagnostics[0].Message);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RunErrorCheck_MultipleQueuedEdits_LatestPendingRequestWins()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 1);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("first");
				PumpUntil(() => detector.CallCount >= 1);

				coordinator.RunErrorCheck("second");
				coordinator.RunErrorCheck("third");
				detector.Release();

				PumpUntil(() => !coordinator.IsBusy && detector.CallCount >= 2);

				// Only one follow-up check runs, for the latest retained content.
				Assert.AreEqual(2, detector.CallCount);
				Assert.AreEqual("third", detector.Contents[detector.Contents.Count - 1]);
				Assert.AreEqual("result:third", editor.Diagnostics[0].Message);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RunErrorCheck_DetectorFailure_ThenNewerSuccessfulRequestPublishes()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new FailOnceBlockingDetector();
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("first");
				PumpUntil(() => detector.CallCount >= 1);

				coordinator.RunErrorCheck("second");
				detector.Release();

				PumpUntil(() => !coordinator.IsBusy && detector.CallCount >= 2);

				Assert.AreEqual(2, detector.CallCount);
				Assert.AreEqual("result:second", editor.Diagnostics[0].Message);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void Dispose_WhileBusy_CancelsPendingAndStopsCallbacks()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 1);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("first");
				PumpUntil(() => detector.CallCount >= 1);

				coordinator.RunErrorCheck("pending");
				coordinator.Dispose();

				// Post-dispose requests are no-ops and never wedge the worker.
				coordinator.RunErrorCheck("after-dispose");
				detector.Release();
				PumpUntil(() => !coordinator.IsBusy);

				Assert.IsFalse(coordinator.IsBusy);
				Assert.AreEqual(0, editor.Diagnostics.Count);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void Dispose_IsIdempotent()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), new SlowDetector(blockFirstCalls: 0));

				coordinator.Dispose();
				coordinator.Dispose();
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void SilentSession_WhileDetectionActive_IgnoresCompletedResult()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 1);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("content");
				PumpUntil(() => detector.CallCount >= 1);
				Assert.IsTrue(coordinator.IsBusy);

				editor.IsSilentSession = true;
				detector.Release();

				PumpUntil(() => !coordinator.IsBusy);

				// The in-flight request completed while silent, so its result must not replace editor diagnostics.
				Assert.IsFalse(coordinator.IsBusy);
				Assert.AreEqual(0, editor.Diagnostics.Count);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void SilentSession_WhileDetectionPending_DropsQueuedCheck()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 1);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector);

				coordinator.RunErrorCheck("first");
				PumpUntil(() => detector.CallCount >= 1);
				Assert.IsTrue(coordinator.IsBusy);

				coordinator.RunErrorCheck("second");
				editor.IsSilentSession = true;
				detector.Release();

				PumpUntil(() => !coordinator.IsBusy);

				// The queued check must not start while silent, and the completed first result must be discarded.
				Assert.AreEqual(1, detector.CallCount);
				Assert.AreEqual(0, editor.Diagnostics.Count);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void SilentSession_WhileIdleTimerPending_DoesNotStartDetection()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				var detector = new SlowDetector(blockFirstCalls: 0);
				var coordinator = new TextDiagnosticsCoordinator(editor, new Version(1, 0), detector, idleDelayInterval: TimeSpan.FromMilliseconds(20.0));

				coordinator.RunOnIdle("content");
				editor.IsSilentSession = true;

				PumpFor(TimeSpan.FromMilliseconds(200.0));

				// The pending idle timer must not start a check after the editor entered a silent session.
				Assert.AreEqual(0, detector.CallCount);
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

	private static void PumpUntil(Func<bool> condition)
	{
		Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
		int attempts = 0;

		while (!condition() && attempts++ < 5000)
			dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
	}

	private static void PumpFor(TimeSpan duration)
	{
		Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
		DateTime deadline = DateTime.Now + duration;

		while (DateTime.Now < deadline)
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

	private sealed class SlowDetector : IErrorDetector
	{
		private readonly int _blockFirstCalls;
		private readonly ManualResetEventSlim _release = new(false);
		private int _callCount;

		public SlowDetector(int blockFirstCalls)
			=> _blockFirstCalls = blockFirstCalls;

		public List<string> Contents { get; } = new();

		public int CallCount => _callCount;

		public IReadOnlyList<TextEditorDiagnostic> FindErrors(string editorContent, Version engineVersion)
		{
			int call = Interlocked.Increment(ref _callCount);
			Contents.Add(editorContent);

			if (call <= _blockFirstCalls)
				_release.Wait();

			return [new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Error, "result:" + editorContent, 0, Math.Max(1, editorContent.Length))];
		}

		public void Release()
			=> _release.Set();
	}

	private sealed class FailOnceBlockingDetector : IErrorDetector
	{
		private readonly ManualResetEventSlim _release = new(false);
		private int _callCount;

		public int CallCount => _callCount;

		public IReadOnlyList<TextEditorDiagnostic> FindErrors(string editorContent, Version engineVersion)
		{
			int call = Interlocked.Increment(ref _callCount);
			_release.Wait();

			if (call == 1)
				throw new InvalidOperationException("Detector failed.");

			return [new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Error, "result:" + editorContent, 0, Math.Max(1, editorContent.Length))];
		}

		public void Release()
			=> _release.Set();
	}
}
