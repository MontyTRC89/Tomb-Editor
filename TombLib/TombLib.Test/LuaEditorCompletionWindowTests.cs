using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Services;
using TombLib.Scripting.Objects;

namespace TombLib.Test;

[TestClass]
public class LuaEditorCompletionWindowTests
{
	[TestMethod]
	public void RequestCompletionAsync_OpensCompletionWindowWithCurrentItemsAndOffsets()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaCompletionProvider();
			provider.EnqueueCompletionResponse(
			[
				new LuaCompletionItem("spawn_room", detail: "local variable")
			]);

			var editor = CreateEditor(provider, "spa");
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				CompletionWindow completionWindow = GetPrivateField<CompletionWindow>(editor, "_completionWindow");

				Assert.AreEqual(1, completionWindow.CompletionList.CompletionData.Count);
				Assert.AreEqual(0, completionWindow.StartOffset);
				Assert.AreEqual(3, completionWindow.EndOffset);
				Assert.IsNotNull(completionWindow.CompletionList.ListBox.SelectedItem);
				Assert.AreEqual(1, provider.CompletionRequests.Count);
				Assert.AreEqual(0, provider.CompletionRequests[0].Line);
				Assert.AreEqual(3, provider.CompletionRequests[0].Column);
			}
			finally
			{
				CloseCompletionWindow(editor);
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RequestCompletionAsync_RefreshClosesPreviousTooltipAndRecreatesWindow()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaCompletionProvider();
			provider.EnqueueCompletionResponse(
			[
				new LuaCompletionItem("spawn_room", detail: "local variable")
			]);
			provider.EnqueueCompletionResponse(
			[
				new LuaCompletionItem("spell_room", detail: "global variable")
			]);

			var editor = CreateEditor(provider, "spa");
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				CompletionWindow firstWindow = GetPrivateField<CompletionWindow>(editor, "_completionWindow");
				ToolTip firstToolTip = GetCompletionToolTip(firstWindow);
				firstToolTip.Content = new TextBlock { Text = "old tooltip" };
				firstToolTip.IsOpen = true;

				editor.Text = "spe";
				editor.CaretOffset = 3;

				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				CompletionWindow refreshedWindow = GetPrivateField<CompletionWindow>(editor, "_completionWindow");
				var refreshedItem = (LuaCompletionData)refreshedWindow.CompletionList.CompletionData[0];

				Assert.AreNotSame(firstWindow, refreshedWindow);
				Assert.IsFalse(firstToolTip.IsOpen);
				Assert.AreEqual("spell_room", refreshedItem.DisplayText);
			}
			finally
			{
				CloseCompletionWindow(editor);
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RequestCompletionAsync_EmptyResults_CloseExistingWindow()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaCompletionProvider();
			provider.EnqueueCompletionResponse(
			[
				new LuaCompletionItem("spawn_room", detail: "local variable")
			]);
			provider.EnqueueCompletionResponse([]);

			var editor = CreateEditor(provider, "spa");
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				FieldInfo completionWindowField = FindInstanceField(editor.GetType(), "_completionWindow")
					?? throw new InvalidOperationException("Private field '_completionWindow' was not found.");

				Assert.IsNull(completionWindowField.GetValue(editor));
			}
			finally
			{
				CloseCompletionWindow(editor);
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RequestCompletionAsync_DismissesSignatureHelpBeforeOpeningWindow()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaCompletionProvider();
			provider.EnqueueCompletionResponse(
			[
				new LuaCompletionItem("spawn_room", detail: "local variable")
			]);

			var editor = CreateEditor(provider, "spa");
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				Popup signaturePopup = GetSignatureHelpField<Popup>(editor, "_signaturePopup");
				ContentPresenter signaturePresenter = GetSignatureHelpField<ContentPresenter>(editor, "_signaturePopupPresenter");

				signaturePresenter.Content = new TextBlock { Text = "signature" };
				signaturePopup.IsOpen = true;
				SetSignatureHelpField(editor, "_signatureRequestToken", 4);

				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				Assert.IsFalse(signaturePopup.IsOpen);
				Assert.IsNull(signaturePresenter.Content);
				Assert.AreEqual(5, GetSignatureHelpField<int>(editor, "_signatureRequestToken"));
				Assert.IsNotNull(GetPrivateField<CompletionWindow>(editor, "_completionWindow"));
			}
			finally
			{
				CloseCompletionWindow(editor);
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void UpdateCompletionTooltipAsync_ResolvesSelectedCompletionItem()
	{
		RunInSta(() =>
		{
			int resolveCallCount = 0;
			var provider = new FakeLuaCompletionProvider();
			provider.EnqueueCompletionResponse(
			[
				new LuaCompletionItem(
					"spawn_room",
					resolveAsync: _ =>
					{
						resolveCallCount++;
						return Task.FromResult(new LuaCompletionItem("spawn_room", detail: "resolved detail"));
					})
			]);

			var editor = CreateEditor(provider, "spa");
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				CompletionWindow completionWindow = GetPrivateField<CompletionWindow>(editor, "_completionWindow");
				ToolTip toolTip = GetCompletionToolTip(completionWindow);
				completionWindow.CompletionList.ListBox.SelectedItem = completionWindow.CompletionList.CompletionData[0];

				int updateToken = GetPrivateField<int>(GetCompletionController(editor), "_completionToolTipUpdateToken");
				InvokeControllerTask(editor, "_completionController", "UpdateTooltipAsync", [typeof(ToolTip), typeof(int)], toolTip, updateToken).GetAwaiter().GetResult();

				Assert.AreEqual(1, resolveCallCount);
				Assert.IsTrue(toolTip.IsOpen);
				Assert.IsInstanceOfType(toolTip.Content, typeof(Border));

				var contentBorder = (Border)toolTip.Content!;
				var contentPanel = (StackPanel)(contentBorder.Child
					?? throw new AssertFailedException("Expected tooltip content panel."));
				var detailBlock = (TextBlock)contentPanel.Children[0];

				Assert.AreEqual("resolved detail", detailBlock.Text);
			}
			finally
			{
				CloseCompletionWindow(editor);
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void UpdateCompletionTooltipAsync_HidesTooltipWhenSelectionIsCleared()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaCompletionProvider();
			provider.EnqueueCompletionResponse(
			[
				new LuaCompletionItem("spawn_room", detail: "local variable")
			]);

			var editor = CreateEditor(provider, "spa");
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				CompletionWindow completionWindow = GetPrivateField<CompletionWindow>(editor, "_completionWindow");
				ToolTip toolTip = GetCompletionToolTip(completionWindow);
				toolTip.Content = new TextBlock { Text = "stale tooltip" };
				toolTip.IsOpen = true;
				completionWindow.CompletionList.ListBox.SelectedItem = null;

				int updateToken = GetPrivateField<int>(GetCompletionController(editor), "_completionToolTipUpdateToken");
				InvokeControllerTask(editor, "_completionController", "UpdateTooltipAsync", [typeof(ToolTip), typeof(int)], toolTip, updateToken).GetAwaiter().GetResult();

				Assert.IsFalse(toolTip.IsOpen);
			}
			finally
			{
				CloseCompletionWindow(editor);
				hostWindow.Close();
			}
		});
	}

	private static LuaEditor CreateEditor(ILuaIntellisenseProvider provider, string text)
		=> new(new Version(1, 0))
		{
			FilePath = @"C:\Workspace\Scripts\test.lua",
			Text = text,
			IntellisenseProvider = provider
		};

	private static Task InvokePrivateTask(object instance, string methodName, Type[] parameterTypes, params object?[] arguments)
		=> (Task)(InvokePrivateInstanceMethod(instance, methodName, parameterTypes, arguments)
			?? throw new InvalidOperationException($"Private instance method '{methodName}' returned null."));

	private static Task InvokeControllerTask(LuaEditor editor, string controllerFieldName, string methodName, Type[] parameterTypes, params object?[] arguments)
		=> (Task)(InvokePrivateInstanceMethod(GetPrivateField<object>(editor, controllerFieldName), methodName, parameterTypes, arguments)
			?? throw new InvalidOperationException($"Controller method '{methodName}' returned null."));

	private static object? InvokePrivateInstanceMethod(object instance, string methodName, Type[] parameterTypes, params object?[] arguments)
	{
		MethodInfo method = instance.GetType().GetMethod(
			methodName,
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			binder: null,
			parameterTypes,
			modifiers: null)
			?? throw new InvalidOperationException($"Private instance method '{methodName}' was not found.");

		return method.Invoke(instance, arguments);
	}

	private static void CloseCompletionWindow(LuaEditor editor)
		=> InvokePrivateInstanceMethod(editor, "CloseCompletionWindow", Type.EmptyTypes);

	private static object GetCompletionController(LuaEditor editor)
		=> GetPrivateField<object>(editor, "_completionController");

	private static T GetSignatureHelpField<T>(LuaEditor editor, string fieldName)
		=> GetPrivateField<T>(GetPrivateField<object>(editor, "_signatureHelpController"), fieldName);

	private static void SetSignatureHelpField(LuaEditor editor, string fieldName, object value)
		=> SetPrivateField(GetPrivateField<object>(editor, "_signatureHelpController"), fieldName, value);

	private static T GetPrivateField<T>(object instance, string fieldName)
	{
		FieldInfo field = FindInstanceField(instance.GetType(), fieldName)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		return (T)(field.GetValue(instance)
			?? throw new InvalidOperationException($"Private field '{fieldName}' returned null."));
	}

	private static void SetPrivateField(object instance, string fieldName, object value)
	{
		FieldInfo field = FindInstanceField(instance.GetType(), fieldName)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		field.SetValue(instance, value);
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

	private static ToolTip GetCompletionToolTip(CompletionWindow completionWindow)
	{
		FieldInfo field = typeof(CompletionWindow).GetField("toolTip", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("CompletionWindow private field 'toolTip' was not found.");

		return (ToolTip)(field.GetValue(completionWindow)
			?? throw new InvalidOperationException("CompletionWindow private field 'toolTip' returned null."));
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

	private readonly record struct CompletionRequest(string FilePath, string Content, int Line, int Column, char? TriggerCharacter);

	private sealed class FakeLuaCompletionProvider : ILuaIntellisenseProvider
	{
		private readonly Queue<IReadOnlyList<LuaCompletionItem>> _completionResponses = [];

		public bool IsAvailable { get; set; } = true;

		public List<CompletionRequest> CompletionRequests { get; } = [];

		public event Action<string, IReadOnlyList<TextEditorDiagnostic>>? DiagnosticsUpdated
		{
			add { }
			remove { }
		}

		public event Action<string, IReadOnlyList<LuaSemanticToken>>? SemanticTokensUpdated
		{
			add { }
			remove { }
		}

		public void EnqueueCompletionResponse(IReadOnlyList<LuaCompletionItem> items)
			=> _completionResponses.Enqueue(items);

		public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath)
			=> [];

		public IReadOnlyList<LuaSemanticToken> GetSemanticTokens(string filePath)
			=> [];

		public void OpenDocument(string filePath, string content)
		{
		}

		public void UpdateDocument(string filePath, string content)
		{
		}

		public void CloseDocument(string filePath)
		{
		}

		public void RenameDocument(string oldFilePath, string newFilePath, string content)
		{
		}

		public Task<IReadOnlyList<LuaCompletionItem>> GetCompletionItemsAsync(string filePath, string content,
			int line, int column, char? triggerCharacter = null, CancellationToken cancellationToken = default)
		{
			CompletionRequests.Add(new CompletionRequest(filePath, content, line, column, triggerCharacter));
			IReadOnlyList<LuaCompletionItem> response = _completionResponses.Count > 0 ? _completionResponses.Dequeue() : [];
			return Task.FromResult(response);
		}

		public Task<LuaHoverInfo?> GetHoverAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
			=> Task.FromResult<LuaHoverInfo?>(null);

		public Task<LuaDefinitionLocation?> GetDefinitionAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
			=> Task.FromResult<LuaDefinitionLocation?>(null);

		public Task<LuaSignatureInfo?> GetSignatureHelpAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
			=> Task.FromResult<LuaSignatureInfo?>(null);

		public void Dispose()
		{
		}
	}
}