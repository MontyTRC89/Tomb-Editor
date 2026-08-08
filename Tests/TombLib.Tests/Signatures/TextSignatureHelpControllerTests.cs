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
	public void RequestAsync_ProviderReturnsNull_WhenNotVisible_DismissesSignatureHelp()
	{
		WPFTestHelper.RunInSta(() =>
		{
			int dismissCount = 0;
			int showCount = 0;

			var controller = new TextSignatureHelpController(
				() => 0,
				(offset, requestToken) => Task.FromResult<TextSignatureHelpInfo?>(null),
				_ => showCount++,
				() => dismissCount++);

			controller.RequestAsync(5).GetAwaiter().GetResult();

			// A null result with nothing visible dismisses signature help.
			Assert.AreEqual(1, dismissCount);
			Assert.AreEqual(0, showCount);
			Assert.IsFalse(controller.IsVisible);
			Assert.IsNull(controller.CurrentSignatureHelp);
		});
	}

	[TestMethod]
	public void RequestAsync_ProviderReturnsNull_WhenVisible_PreservesVisibleSignatureHelp()
	{
		WPFTestHelper.RunInSta(() =>
		{
			int servedResponses = 0;
			int dismissCount = 0;
			int showCount = 0;

			var controller = new TextSignatureHelpController(
				() => 0,
				(offset, requestToken) =>
				{
					servedResponses++;
					return Task.FromResult<TextSignatureHelpInfo?>(servedResponses == 1
						? new TextSignatureHelpInfo("spawn(room)", 0, "Spawns an object.", [])
						: null);
				},
				_ => showCount++,
				() => dismissCount++);

			controller.RequestAsync(5).GetAwaiter().GetResult();
			controller.RequestAsync(9).GetAwaiter().GetResult();

			// A null refresh result must not tear down an already-visible signature popup.
			Assert.AreEqual(1, showCount);
			Assert.AreEqual(0, dismissCount);
			Assert.IsTrue(controller.IsVisible);
			Assert.IsNotNull(controller.CurrentSignatureHelp);
		});
	}

	[TestMethod]
	public void RequestAsync_ProviderReturnsSignature_ShowsSignatureHelp()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var signature = new TextSignatureHelpInfo("spawn(room)", 0, "Spawns an object.", []);
			TextSignatureHelpInfo? shownSignature = null;

			var controller = new TextSignatureHelpController(
				() => 0,
				(offset, requestToken) => Task.FromResult<TextSignatureHelpInfo?>(signature),
				info => shownSignature = info,
				() => { });

			controller.RequestAsync(5).GetAwaiter().GetResult();

			Assert.IsNotNull(shownSignature);
			Assert.AreEqual(signature, shownSignature);
			Assert.IsTrue(controller.IsVisible);
			Assert.AreEqual(signature, controller.CurrentSignatureHelp);
		});
	}

	[TestMethod]
	public void RequestAsync_SupersedingRequest_InvokesCancelHookAndDropsInFlightResult()
	{
		WPFTestHelper.RunInSta(() =>
		{
			int cancelHookCalls = 0;
			bool shown = false;
			var completion = new TaskCompletionSource<TextSignatureHelpInfo?>(TaskCreationOptions.RunContinuationsAsynchronously);

			var controller = new TextSignatureHelpController(
				() => 0,
				(offset, requestToken) => completion.Task,
				_ => shown = true,
				() => { },
				cancelInFlightRequest: () => cancelHookCalls++);

			Task firstRequest = controller.RequestAsync(5);
			Task secondRequest = controller.RequestAsync(9);

			// A superseding request must cancel the active provider call instead of queueing behind it.
			Assert.AreEqual(1, cancelHookCalls);

			completion.TrySetResult(new TextSignatureHelpInfo("spawn(room)", 0, "Spawns an object.", []));
			firstRequest.GetAwaiter().GetResult();
			secondRequest.GetAwaiter().GetResult();

			// The cancelled in-flight result is dropped; the pending offset is refreshed separately.
			Assert.IsFalse(shown);
			Assert.IsFalse(controller.IsVisible);
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
