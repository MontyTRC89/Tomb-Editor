using Nickelony.LanguageServer.Abstractions.Diagnostics;
using Nickelony.LanguageServer.Abstractions.Hover;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TombLib.Scripting.Hover;
using TombLib.Scripting.UI.Hover;

namespace TombLib.Tests;

[TestClass]
public class TextHoverControllerTests
{
	[TestMethod]
	public void HandleMouseHoverAsync_ThrowingProvider_FallsBackToDiagnosticWithoutEscaping()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				bool diagnosticShown = false;

				var controller = new TextHoverController(
					owner,
					_ => 5,
					_ => new TextHoverRequestState(
						ShouldRequestHover: true,
						RequestOffset: 5,
						CanShowToolTip: true,
						CanShowDiagnosticFallback: true,
						HasDiagnostic: true,
						DiagnosticMessage: "diagnostic",
						DiagnosticSeverity: TextEditorDiagnosticSeverity.Error),
					(offset, cancellationToken) => throw new InvalidOperationException("Hover failed."),
					_ => 5,
					(message, severity) => diagnosticShown = true,
					_ => { },
					(info, message, severity) => { });

				var eventArgs = new MouseEventArgs(Mouse.PrimaryDevice, 0)
				{
					RoutedEvent = Mouse.MouseMoveEvent
				};

				controller.HandleMouseHoverAsync(eventArgs).GetAwaiter().GetResult();

				// The exception must not escape the public async boundary; the controller falls
				// back to the diagnostic tooltip and logs the failure.
				Assert.IsTrue(diagnosticShown);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void HandleMouseHoverAsync_AfterDisposal_DoesNotShowTooltipOrApplyState()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				bool tooltipShown = false;
				bool diagnosticShown = false;
				bool stateApplied = false;

				var controller = new TextHoverController(
					owner,
					_ => 5,
					_ => new TextHoverRequestState(
						ShouldRequestHover: true,
						RequestOffset: 5,
						CanShowToolTip: true,
						CanShowDiagnosticFallback: true,
						HasDiagnostic: true,
						DiagnosticMessage: "diagnostic",
						DiagnosticSeverity: TextEditorDiagnosticSeverity.Error),
					(offset, cancellationToken) => Task.FromResult<TextHoverInfo?>(
						new TextHoverInfo("hover", TextHoverContentKind.PlainText, "symbol")),
					_ => 5,
					(message, severity) => diagnosticShown = true,
					_ => tooltipShown = true,
					(info, message, severity) => { },
					_ => stateApplied = true);

				var eventArgs = new MouseEventArgs(Mouse.PrimaryDevice, 0)
				{
					RoutedEvent = Mouse.MouseMoveEvent
				};

				controller.Dispose();
				controller.HandleMouseHoverAsync(eventArgs).GetAwaiter().GetResult();

				// A disposed controller must not show tooltips, fall back to diagnostics, or apply state.
				Assert.IsFalse(tooltipShown);
				Assert.IsFalse(diagnosticShown);
				Assert.IsFalse(stateApplied);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void CancelPendingRequest_WhileRequestInFlight_DoesNotShowTooltip()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				bool tooltipShown = false;
				var completion = new TaskCompletionSource<TextHoverInfo?>(TaskCreationOptions.RunContinuationsAsynchronously);
				TextHoverController? controller = null;

				controller = new TextHoverController(
					owner,
					_ => 5,
					_ => new TextHoverRequestState(
						ShouldRequestHover: true,
						RequestOffset: 5,
						CanShowToolTip: true,
						CanShowDiagnosticFallback: false,
						HasDiagnostic: false,
						DiagnosticMessage: null,
						DiagnosticSeverity: TextEditorDiagnosticSeverity.Error),
					(offset, cancellationToken) => completion.Task,
					_ => 5,
					(message, severity) => { },
					_ => tooltipShown = true,
					(info, message, severity) => { });

				var eventArgs = new MouseEventArgs(Mouse.PrimaryDevice, 0)
				{
					RoutedEvent = Mouse.MouseMoveEvent
				};

				Task hoverTask = controller.HandleMouseHoverAsync(eventArgs);
				controller.CancelPendingRequest();
				completion.TrySetResult(new TextHoverInfo("hover", TextHoverContentKind.PlainText, "symbol"));

				hoverTask.GetAwaiter().GetResult();

				// A cancelled in-flight request must not publish its result or show a tooltip.
				Assert.IsFalse(tooltipShown);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void CancelPendingRequest_And_InvalidateRequests_AfterDisposal_DoNotThrow()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();

			var controller = new TextHoverController(
				owner,
				_ => 5,
				_ => new TextHoverRequestState(
					ShouldRequestHover: false,
					RequestOffset: -1,
					CanShowToolTip: false,
					CanShowDiagnosticFallback: false,
					HasDiagnostic: false,
					DiagnosticMessage: null,
					DiagnosticSeverity: TextEditorDiagnosticSeverity.Error),
				(offset, cancellationToken) => Task.FromResult<TextHoverInfo?>(null),
				_ => 5,
				(message, severity) => { },
				_ => { },
				(info, message, severity) => { });

			controller.Dispose();
			controller.CancelPendingRequest();
			controller.InvalidateRequests();
		});
	}

	[TestMethod]
	public void HandleMouseHoverAsync_InFlightRequestCompletingAfterDisposal_DoesNotShowTooltip()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				bool tooltipShown = false;
				var completion = new TaskCompletionSource<TextHoverInfo?>(TaskCreationOptions.RunContinuationsAsynchronously);
				TextHoverController? controller = null;

				controller = new TextHoverController(
					owner,
					_ => 5,
					_ => new TextHoverRequestState(
						ShouldRequestHover: true,
						RequestOffset: 5,
						CanShowToolTip: true,
						CanShowDiagnosticFallback: true,
						HasDiagnostic: false,
						DiagnosticMessage: null,
						DiagnosticSeverity: TextEditorDiagnosticSeverity.Error),
					(offset, cancellationToken) => completion.Task,
					_ => 5,
					(message, severity) => { },
					_ => tooltipShown = true,
					(info, message, severity) => { });

				var eventArgs = new MouseEventArgs(Mouse.PrimaryDevice, 0)
				{
					RoutedEvent = Mouse.MouseMoveEvent
				};

				Task hoverTask = controller.HandleMouseHoverAsync(eventArgs);
				controller.Dispose();
				completion.TrySetResult(new TextHoverInfo("hover", TextHoverContentKind.PlainText, "symbol"));

				hoverTask.GetAwaiter().GetResult();

				// A request that completes after disposal must not show a tooltip.
				Assert.IsFalse(tooltipShown);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}
}
