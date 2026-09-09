using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TombLib.Scripting.UI.Navigation;

namespace TombLib.Tests;

[TestClass]
public class TextDefinitionTriggerControllerTests
{
	[TestMethod]
	public void TryHandleKeyDownAsync_F12AndSuccessfulNavigation_HandlesEventAndUsesCaretOffset()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				int requestedOffset = -1;
				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) =>
					{
						requestedOffset = offset;
						return Task.FromResult(true);
					});

				var eventArgs = new KeyEventArgs(
					Keyboard.PrimaryDevice,
					PresentationSource.FromVisual(hostWindow)!,
					0,
					Key.F12)
				{
					RoutedEvent = Keyboard.KeyDownEvent
				};

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, 42).GetAwaiter().GetResult();

				Assert.IsTrue(handled);
				Assert.IsTrue(eventArgs.Handled);
				Assert.AreEqual(42, requestedOffset);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandleKeyDownAsync_F12AndFailedNavigation_DoesNotHandleEvent()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) => Task.FromResult(false));

				var eventArgs = new KeyEventArgs(
					Keyboard.PrimaryDevice,
					PresentationSource.FromVisual(hostWindow)!,
					0,
					Key.F12)
				{
					RoutedEvent = Keyboard.KeyDownEvent
				};

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, 42).GetAwaiter().GetResult();

				Assert.IsFalse(handled);
				Assert.IsFalse(eventArgs.Handled);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandleKeyDownAsync_ThrowingNavigation_ReturnsFalseWithoutEscaping()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) => throw new InvalidOperationException("Navigation failed."));

				var eventArgs = new KeyEventArgs(
					Keyboard.PrimaryDevice,
					PresentationSource.FromVisual(hostWindow)!,
					0,
					Key.F12)
				{
					RoutedEvent = Keyboard.KeyDownEvent
				};

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, 42).GetAwaiter().GetResult();

				Assert.IsFalse(handled);
				Assert.IsFalse(eventArgs.Handled);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}
}