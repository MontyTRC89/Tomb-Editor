using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using TombLib.Scripting.Rendering;

namespace TombLib.Test;

[TestClass]
public class MarkdownToolTipRendererTests
{
	[TestMethod]
	public void CreateCodeBlockEditor_DoesNotAcceptKeyboardFocus()
	{
		RunInSta(() =>
		{
			TextEditor editor = MarkdownToolTipRenderer.CreateCodeBlockEditor("lua", "local value = 1", Brushes.White);

			Assert.IsFalse(editor.Focusable);
			Assert.IsFalse(editor.IsTabStop);
			Assert.IsFalse(editor.TextArea.Focusable);
			Assert.IsFalse(editor.TextArea.IsTabStop);
		});
	}

	private static void RunInSta(Action action)
	{
		Exception? capturedException = null;
		using var completed = new ManualResetEventSlim(false);

		var thread = new Thread(() =>
		{
			try
			{
				action();
			}
			catch (Exception exception)
			{
				capturedException = exception;
			}
			finally
			{
				completed.Set();
			}
		});

		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		completed.Wait();
		thread.Join();

		if (capturedException is not null)
			ExceptionDispatchInfo.Capture(capturedException).Throw();
	}
}