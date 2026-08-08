using Nickelony.LanguageServer.Abstractions.Signatures;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using TombLib.Scripting.UI.Signatures;

namespace TombLib.Tests;

[TestClass]
public class TextSignatureHelpControllerTests
{
	[TestMethod]
	public void RequestAsync_ThrowingProvider_DoesNotEscapeAndStaysDismissed()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var controller = new TextSignatureHelpController(
				() => 0,
				(offset, requestToken) => throw new InvalidOperationException("Signature help failed."),
				_ => { },
				() => { });

			controller.RequestAsync(5).GetAwaiter().GetResult();

			// The exception must not escape the public async boundary; the controller stays dismissed.
			Assert.IsFalse(controller.IsVisible);
			Assert.IsFalse(controller.IsActiveOrPending);
		});
	}

	[TestMethod]
	public void RequestAsync_StaleRequestAfterInvalidation_DoesNotShowSignatureHelp()
	{
		WPFTestHelper.RunInSta(() =>
		{
			bool shown = false;
			TextSignatureHelpController? controller = null;

			controller = new TextSignatureHelpController(
				() => 0,
				(offset, requestToken) =>
				{
					// The request is superseded while in flight, so its result must be dropped.
					// The provider callback runs only after the controller is assigned.
					controller!.InvalidateRequests();

					return Task.FromResult<TextSignatureHelpInfo?>(
						new TextSignatureHelpInfo("spawn(room)", 0, "Spawns an object.", []));
				},
				_ => shown = true,
				() => { });

			controller.RequestAsync(5).GetAwaiter().GetResult();

			Assert.IsFalse(shown);
			Assert.IsFalse(controller.IsVisible);
		});
	}

	[TestMethod]
	public void RequestAsync_AfterDisposal_ReturnsCompletedTaskAndDoesNotShow()
	{
		WPFTestHelper.RunInSta(() =>
		{
			bool shown = false;

			var controller = new TextSignatureHelpController(
				() => 0,
				(offset, requestToken) => Task.FromResult<TextSignatureHelpInfo?>(
					new TextSignatureHelpInfo("spawn(room)", 0, "Spawns an object.", [])),
				_ => shown = true,
				() => { });

			controller.Dispose();
			Task requestTask = controller.RequestAsync(5);

			Assert.IsTrue(requestTask.IsCompletedSuccessfully);
			Assert.IsFalse(shown);
			Assert.IsFalse(controller.IsVisible);
		});
	}

	[TestMethod]
	public void RequestAsync_InFlightRequestCompletingAfterDisposal_DoesNotShowSignatureHelp()
	{
		WPFTestHelper.RunInSta(() =>
		{
			bool shown = false;
			var completion = new TaskCompletionSource<TextSignatureHelpInfo?>(TaskCreationOptions.RunContinuationsAsynchronously);
			TextSignatureHelpController? controller = null;

			controller = new TextSignatureHelpController(
				() => 0,
				(offset, requestToken) => completion.Task,
				_ => shown = true,
				() => { });

			Task requestTask = controller.RequestAsync(5);
			controller.Dispose();
			completion.TrySetResult(new TextSignatureHelpInfo("spawn(room)", 0, "Spawns an object.", []));

			requestTask.GetAwaiter().GetResult();

			// A request that completes after disposal must not show signature help.
			Assert.IsFalse(shown);
			Assert.IsFalse(controller.IsVisible);
		});
	}

	[TestMethod]
	public void PublicOperations_AfterDisposal_DoNotThrowAndDoNotSchedule()
	{
		WPFTestHelper.RunInSta(() =>
		{
			bool shown = false;

			var controller = new TextSignatureHelpController(
				() => 0,
				(offset, requestToken) => Task.FromResult<TextSignatureHelpInfo?>(
					new TextSignatureHelpInfo("spawn(room)", 0, "Spawns an object.", [])),
				_ => shown = true,
				() => { });

			controller.Dispose();

			controller.Dismiss();
			controller.ScheduleRefresh();
			controller.CancelPendingRefresh();
			controller.InvalidateRequests();

			Assert.IsFalse(shown);
			Assert.IsFalse(controller.IsVisible);
			Assert.IsFalse(controller.IsActiveOrPending);
		});
	}

	[TestMethod]
	public void Dispose_UnsubscribesRefreshTimerTickHandler()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var controller = new TextSignatureHelpController(
				() => 0,
				(offset, requestToken) => Task.FromResult<TextSignatureHelpInfo?>(null),
				_ => { },
				() => { });

			DispatcherTimer refreshTimer = WPFTestHelper.GetPrivateField<DispatcherTimer>(controller, "_refreshTimer");

			Assert.AreEqual(1, GetTickHandlerCount(refreshTimer));

			controller.Dispose();

			// Disposal must unsubscribe the refresh timer's tick handler.
			Assert.AreEqual(0, GetTickHandlerCount(refreshTimer));
		});
	}

	private static int GetTickHandlerCount(DispatcherTimer timer)
	{
		System.Reflection.FieldInfo field = typeof(DispatcherTimer).GetField(
			"Tick",
			System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
			?? throw new InvalidOperationException("DispatcherTimer.Tick field was not found.");

		var handler = (System.EventHandler?)field.GetValue(timer);
		return handler?.GetInvocationList().Length ?? 0;
	}
}
