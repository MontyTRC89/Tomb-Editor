using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
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
		RunInSta(() =>
		{
			var editor = new Tomb1MainEditor(new Version(1, 0));
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				editor.ShowMarkdownToolTip("`Level` tooltip");
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				Popup popup = GetPrivateField<Popup>(editor, "_specialToolTip");
				ContentPresenter presenter = GetPrivateField<ContentPresenter>(editor, "_specialToolTipPresenter");

				Assert.IsTrue(popup.IsOpen);
				Assert.IsNotNull(presenter.Content);

				InvokeInstanceMethod(editor, "CloseDefinitionToolTip", [typeof(bool)], true);

				Assert.IsFalse(popup.IsOpen);
				Assert.IsNull(presenter.Content);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	private static object? InvokeInstanceMethod(object instance, string methodName, Type[] parameterTypes, params object?[] arguments)
	{
		MethodInfo method = instance.GetType().GetMethod(
			methodName,
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			binder: null,
			parameterTypes,
			modifiers: null)
			?? throw new InvalidOperationException($"Instance method '{methodName}' was not found.");

		return method.Invoke(instance, arguments);
	}

	private static T GetPrivateField<T>(object instance, string fieldName)
	{
		FieldInfo field = FindInstanceField(instance.GetType(), fieldName)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		return (T)(field.GetValue(instance)
			?? throw new InvalidOperationException($"Private field '{fieldName}' returned null."));
	}

	private static FieldInfo? FindInstanceField(Type type, string fieldName)
	{
		for (Type? currentType = type; currentType is not null; currentType = currentType.BaseType)
		{
			FieldInfo? field = currentType.GetField(
				fieldName,
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

			if (field is not null)
				return field;
		}

		return null;
	}

	private static Window ShowInHostWindow(FrameworkElement content)
	{
		var window = new Window
		{
			Content = content,
			Width = 800.0,
			Height = 600.0,
			ShowActivated = false,
			ShowInTaskbar = false,
			WindowStyle = WindowStyle.None
		};

		window.Show();
		PumpDispatcher(window.Dispatcher, DispatcherPriority.Background);
		return window;
	}

	private static void PumpDispatcher(Dispatcher dispatcher, DispatcherPriority priority)
		=> dispatcher.Invoke(priority, new Action(() => { }));

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
