using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Threading;
using TombLib.Scripting.Completion;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Tests;

[TestClass]
public class TextCompletionControllerDisposalTests
{
	[TestMethod]
	public void PublicOperations_AfterDisposal_ReturnSafeDefaultsAndDoNotThrow()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor(new Version(1, 0));
			var controller = new TextCompletionController(editor);

			controller.Dispose();

			controller.InitializeScheduling(() => Task.CompletedTask);
			controller.ScheduleRequest();
			controller.CancelPendingRequest();
			controller.CloseWindow();
			controller.CancelTooltipUpdate();
			controller.InvalidateRequests();
			controller.ScheduleCloseIfEmpty();
			controller.RebaseOpenCompletionItems(1, 1);

			// Query members return safe defaults instead of touching editor-owned state.
			Assert.IsNull(controller.ActiveWindow);
			Assert.AreEqual(-1, controller.BeginRequest());
			Assert.IsFalse(controller.IsRequestCurrent(1));
			Assert.IsFalse(controller.OpenOrRefresh([]));
			Assert.IsFalse(controller.ApplyDecision(TextCompletionSessionDecision.None));
		});
	}

	[TestMethod]
	public void Dispose_IsIdempotent_AndUnsubscribesTimerHandlers()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new PlainTextEditor(new Version(1, 0));
			var controller = new TextCompletionController(editor);
			controller.InitializeScheduling(() => Task.CompletedTask);

			DispatcherTimer requestTimer = WPFTestHelper.GetPrivateField<DispatcherTimer>(controller, "_requestTimer");
			DispatcherTimer toolTipUpdateTimer = WPFTestHelper.GetPrivateField<DispatcherTimer>(controller, "_toolTipUpdateTimer");

			Assert.AreEqual(1, GetTickHandlerCount(requestTimer));
			Assert.AreEqual(1, GetTickHandlerCount(toolTipUpdateTimer));

			controller.Dispose();
			controller.Dispose();

			// Disposal must be idempotent and must unsubscribe both timer handlers.
			Assert.AreEqual(0, GetTickHandlerCount(requestTimer));
			Assert.AreEqual(0, GetTickHandlerCount(toolTipUpdateTimer));
		});
	}

	private static int GetTickHandlerCount(DispatcherTimer timer)
	{
		FieldInfo field = typeof(DispatcherTimer).GetField(
			"Tick",
			BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("DispatcherTimer.Tick field was not found.");

		var handler = (EventHandler?)field.GetValue(timer);
		return handler?.GetInvocationList().Length ?? 0;
	}
}
