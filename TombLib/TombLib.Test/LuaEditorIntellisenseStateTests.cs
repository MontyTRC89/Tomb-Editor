using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using TombLib.Scripting.Lua;

namespace TombLib.Test;

[TestClass]
public class LuaEditorIntellisenseStateTests
{
	[TestMethod]
	public void ShouldRefreshSignatureHelpAfterTextInput_ReturnsTrueWhenSignatureHelpIsActiveOrPending()
	{
		bool shouldRefresh = InvokePrivateStaticBooleanMethod(
			"ShouldRefreshSignatureHelpAfterTextInput",
			[typeof(string), typeof(bool)],
			"a",
			true);

		Assert.IsTrue(shouldRefresh);
	}

	[TestMethod]
	public void ShouldRefreshSignatureHelpAfterTextInput_ReturnsFalseWhenSignatureHelpIsInactive()
	{
		bool shouldRefresh = InvokePrivateStaticBooleanMethod(
			"ShouldRefreshSignatureHelpAfterTextInput",
			[typeof(string), typeof(bool)],
			"a",
			false);

		Assert.IsFalse(shouldRefresh);
	}

	[TestMethod]
	public void ShouldDismissSignatureHelpOnAutoClosingSkip_ReturnsTrueOnlyForMatchingParenthesis()
	{
		bool shouldDismissMatchingParenthesis = InvokePrivateStaticBooleanMethod(
			"ShouldDismissSignatureHelpOnAutoClosingSkip",
			[typeof(string), typeof(string)],
			")",
			")");

		bool shouldDismissOtherElement = InvokePrivateStaticBooleanMethod(
			"ShouldDismissSignatureHelpOnAutoClosingSkip",
			[typeof(string), typeof(string)],
			"]",
			")");

		Assert.IsTrue(shouldDismissMatchingParenthesis);
		Assert.IsFalse(shouldDismissOtherElement);
	}

	[TestMethod]
	public void IsAsyncEditorResultCurrent_ReturnsTrueForCurrentLoadedAvailableRequest()
	{
		bool isCurrent = InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			false,
			3,
			3,
			8,
			8,
			2,
			2,
			true,
			true);

		Assert.IsTrue(isCurrent);
	}

	[TestMethod]
	public void IsAsyncEditorResultCurrent_RejectsCanceledOrStaleResults()
	{
		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			true,
			3,
			3,
			8,
			8,
			2,
			2,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			false,
			3,
			4,
			8,
			8,
			2,
			2,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			false,
			3,
			3,
			8,
			9,
			2,
			2,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			false,
			3,
			3,
			8,
			8,
			2,
			3,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			false,
			3,
			3,
			8,
			8,
			2,
			2,
			false,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			false,
			3,
			3,
			8,
			8,
			2,
			2,
			true,
			false));
	}

	[TestMethod]
	public void IsCompletionItemCurrent_ReturnsTrueForMatchingMetadata()
	{
		bool isCurrent = InvokePrivateStaticBooleanMethod(
			"IsCompletionItemCurrent",
			[typeof(int?), typeof(int), typeof(int?), typeof(int), typeof(bool), typeof(bool)],
			8,
			8,
			2,
			2,
			true,
			true);

		Assert.IsTrue(isCurrent);
	}

	[TestMethod]
	public void IsCompletionItemCurrent_AllowsUnstampedItemsButRejectsStaleOrIncompleteMetadata()
	{
		Assert.IsTrue(InvokePrivateStaticBooleanMethod(
			"IsCompletionItemCurrent",
			[typeof(int?), typeof(int), typeof(int?), typeof(int), typeof(bool), typeof(bool)],
			null,
			8,
			null,
			2,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsCompletionItemCurrent",
			[typeof(int?), typeof(int), typeof(int?), typeof(int), typeof(bool), typeof(bool)],
			8,
			9,
			2,
			2,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsCompletionItemCurrent",
			[typeof(int?), typeof(int), typeof(int?), typeof(int), typeof(bool), typeof(bool)],
			8,
			8,
			null,
			2,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsCompletionItemCurrent",
			[typeof(int?), typeof(int), typeof(int?), typeof(int), typeof(bool), typeof(bool)],
			8,
			8,
			2,
			2,
			false,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsCompletionItemCurrent",
			[typeof(int?), typeof(int), typeof(int?), typeof(int), typeof(bool), typeof(bool)],
			8,
			8,
			2,
			2,
			true,
			false));
	}

	[TestMethod]
	public void CloseCompletionWindow_InvalidatesPendingRequests_ButRefreshCloseDoesNot()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0));

			SetPrivateField(editor, "_completionRequestToken", 5);
			InvokePrivateInstanceMethod(editor, "CloseCompletionWindow");
			Assert.AreEqual(6, GetPrivateField<int>(editor, "_completionRequestToken"));

			InvokePrivateInstanceMethod(editor, "CloseCompletionWindowForRefresh");
			Assert.AreEqual(6, GetPrivateField<int>(editor, "_completionRequestToken"));
		});
	}

	private static bool InvokePrivateStaticBooleanMethod(string methodName, Type[] parameterTypes, params object?[] arguments)
	{
		MethodInfo method = typeof(LuaEditor).GetMethod(methodName,
			BindingFlags.Static | BindingFlags.NonPublic,
			binder: null,
			parameterTypes,
			modifiers: null)
			?? throw new InvalidOperationException($"Private static method '{methodName}' was not found.");

		return (bool)(method.Invoke(null, arguments)
			?? throw new InvalidOperationException($"Private static method '{methodName}' returned null."));
	}

	private static void InvokePrivateInstanceMethod(object instance, string methodName)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private instance method '{methodName}' was not found.");

		method.Invoke(instance, null);
	}

	private static T GetPrivateField<T>(object instance, string fieldName)
	{
		FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		return (T)(field.GetValue(instance)
			?? throw new InvalidOperationException($"Private field '{fieldName}' returned null."));
	}

	private static void SetPrivateField(object instance, string fieldName, object value)
	{
		FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		field.SetValue(instance, value);
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