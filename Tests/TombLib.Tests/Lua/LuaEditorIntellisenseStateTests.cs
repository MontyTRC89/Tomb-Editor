using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Services;
using TombLib.Scripting.Objects;
using static TombLib.Tests.WPFTestHelper;

namespace TombLib.Tests;

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
	public void ScheduleSignatureHelpRefresh_StoresCaretOffsetAndStartsTimer()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0))
			{
				Text = "spawn(room)",
				CaretOffset = 6
			};

			InvokeControllerInstanceMethod(editor, "_signatureHelpController", "ScheduleRefresh");

			Assert.IsTrue(GetSignatureHelpField<bool>(editor, "_signatureRefreshPending"));
			Assert.AreEqual(6, GetSignatureHelpField<int>(editor, "_pendingSignatureHelpOffset"));
			Assert.IsTrue(GetSignatureHelpField<DispatcherTimer>(editor, "_signatureRefreshTimer").IsEnabled);
		});
	}

	[TestMethod]
	public void CancelPendingSignatureHelpRefresh_ClearsPendingStateAndStopsTimer()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0))
			{
				Text = "spawn(room)",
				CaretOffset = 6
			};

			InvokeControllerInstanceMethod(editor, "_signatureHelpController", "ScheduleRefresh");
			InvokeControllerInstanceMethod(editor, "_signatureHelpController", "CancelPendingRefresh");

			Assert.IsFalse(GetSignatureHelpField<bool>(editor, "_signatureRefreshPending"));
			Assert.AreEqual(-1, GetSignatureHelpField<int>(editor, "_pendingSignatureHelpOffset"));
			Assert.IsFalse(GetSignatureHelpField<DispatcherTimer>(editor, "_signatureRefreshTimer").IsEnabled);
		});
	}

	[TestMethod]
	public void DismissSignatureHelp_ClearsPendingStateAndInvalidatesOutstandingRequests()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0));
			ContentPresenter presenter = GetSignatureHelpField<ContentPresenter>(editor, "_signaturePopupPresenter");
			Popup popup = GetSignatureHelpField<Popup>(editor, "_signaturePopup");

			presenter.Content = new TextBlock { Text = "signature" };
			SetSignatureHelpField(editor, "_signatureRequestToken", 5);
			SetSignatureHelpField(editor, "_signatureRequestInFlight", true);
			SetSignatureHelpField(editor, "_signatureRefreshPending", true);
			SetSignatureHelpField(editor, "_pendingSignatureHelpOffset", 9);

			InvokeInstanceMethod(editor, "DismissSignatureHelp", Type.EmptyTypes);

			Assert.IsFalse(GetSignatureHelpField<bool>(editor, "_signatureRequestInFlight"));
			Assert.AreEqual(6, GetSignatureHelpField<int>(editor, "_signatureRequestToken"));
			Assert.IsFalse(GetSignatureHelpField<bool>(editor, "_signatureRefreshPending"));
			Assert.AreEqual(-1, GetSignatureHelpField<int>(editor, "_pendingSignatureHelpOffset"));
			Assert.IsNull(presenter.Content);
			Assert.IsFalse(popup.IsOpen);
		});
	}

	[TestMethod]
	public void TryGetCompletionTrigger_ReturnsExplicitAndImplicitTriggers()
	{
		Assert.IsTrue(InvokeTryGetCompletionTrigger(".", out char? dotTrigger));
		Assert.AreEqual('.', dotTrigger);

		Assert.IsTrue(InvokeTryGetCompletionTrigger(":", out char? colonTrigger));
		Assert.AreEqual(':', colonTrigger);

		Assert.IsTrue(InvokeTryGetCompletionTrigger("a", out char? identifierTrigger));
		Assert.IsNull(identifierTrigger);
	}

	[TestMethod]
	public void TryGetCompletionTrigger_RejectsEmptyMultiCharacterAndNonIdentifierInput()
	{
		Assert.IsFalse(InvokeTryGetCompletionTrigger(null, out _));
		Assert.IsFalse(InvokeTryGetCompletionTrigger(string.Empty, out _));
		Assert.IsFalse(InvokeTryGetCompletionTrigger("ab", out _));
		Assert.IsFalse(InvokeTryGetCompletionTrigger(" ", out _));
	}

	[TestMethod]
	public void ShouldKeepCompletionWindowOpen_ReturnsTrueOnlyForIdentifierCharacters()
	{
		Assert.IsTrue(InvokePrivateStaticBooleanMethod(
			"ShouldKeepCompletionWindowOpen",
			[typeof(string)],
			"a"));

		Assert.IsTrue(InvokePrivateStaticBooleanMethod(
			"ShouldKeepCompletionWindowOpen",
			[typeof(string)],
			"_"));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"ShouldKeepCompletionWindowOpen",
			[typeof(string)],
			[null]));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"ShouldKeepCompletionWindowOpen",
			[typeof(string)],
			"."));
	}

	[TestMethod]
	public void ScheduleCompletionRequest_StartsTimerAndCancelPendingCompletionRequest_StopsIt()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0));
			object completionController = GetCompletionController(editor);

			InvokeControllerInstanceMethod(editor, "_completionController", "ScheduleRequest");
			Assert.IsTrue(GetPrivateField<DispatcherTimer>(completionController, "_completionRequestTimer").IsEnabled);

			InvokeControllerInstanceMethod(editor, "_completionController", "CancelPendingRequest");
			Assert.IsFalse(GetPrivateField<DispatcherTimer>(completionController, "_completionRequestTimer").IsEnabled);
		});
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
			object completionController = GetCompletionController(editor);

			SetPrivateField(completionController, "_completionRequestToken", 5);
			InvokeInstanceMethod(editor, "CloseCompletionWindow", Type.EmptyTypes);
			Assert.AreEqual(6, GetPrivateField<int>(completionController, "_completionRequestToken"));

			InvokeControllerInstanceMethod(editor, "_completionController", "CloseWindowForRefresh");
			Assert.AreEqual(6, GetPrivateField<int>(completionController, "_completionRequestToken"));
		});
	}

	[TestMethod]
	public void TryGetHoverRequestOffset_ReturnsIdentifierOffsetWhenEligible()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0))
			{
				Text = "spawn(room)"
			};

			bool result = InvokeTryGetHoverRequestOffset(editor, 2, out int hoverOffset);

			Assert.IsTrue(result);
			Assert.AreEqual(2, hoverOffset);
		});
	}

	[TestMethod]
	public void TryGetHoverRequestOffset_BlocksRequestsWhenCompletionWindowIsOpen()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0))
			{
				Text = "spawn(room)"
			};

			editor.InitializeCompletionWindow();

			bool result = InvokeTryGetHoverRequestOffset(editor, 2, out _);

			Assert.IsFalse(result);
		});
	}

	[TestMethod]
	public void ShowBestHoverToolTip_ShowsCombinedTooltipWhenHoverAndDiagnosticAreAvailable()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0));

			InvokeControllerInstanceMethod(
				editor,
				"_hoverController",
				"ShowBestToolTip",
				[typeof(LuaHoverInfo), typeof(bool), typeof(string), typeof(TextEditorDiagnosticSeverity)],
				new LuaHoverInfo("Hover docs.", false),
				true,
				"Warning message.",
				TextEditorDiagnosticSeverity.Warning);

			Popup popup = GetPrivateField<Popup>(editor, "_specialToolTip");
			ContentPresenter presenter = GetPrivateField<ContentPresenter>(editor, "_specialToolTipPresenter");

			Assert.IsTrue(popup.IsOpen);
			Assert.IsInstanceOfType(presenter.Content, typeof(StackPanel));

			var panel = (StackPanel)presenter.Content!;
			Assert.AreEqual(2, panel.Children.Count);
		});
	}

	[TestMethod]
	public void ShowBestHoverToolTip_ShowsHoverTooltipWhenOnlyHoverIsAvailable()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0));

			InvokeControllerInstanceMethod(
				editor,
				"_hoverController",
				"ShowBestToolTip",
				[typeof(LuaHoverInfo), typeof(bool), typeof(string), typeof(TextEditorDiagnosticSeverity)],
				new LuaHoverInfo("Hover docs.", false),
				false,
				string.Empty,
				TextEditorDiagnosticSeverity.Warning);

			Popup popup = GetPrivateField<Popup>(editor, "_specialToolTip");
			ContentPresenter presenter = GetPrivateField<ContentPresenter>(editor, "_specialToolTipPresenter");

			Assert.IsTrue(popup.IsOpen);
			Assert.IsNotNull(presenter.Content);
			Assert.IsNotInstanceOfType(presenter.Content, typeof(StackPanel));
		});
	}

	[TestMethod]
	public void ShowBestHoverToolTip_ShowsDiagnosticTooltipWhenOnlyDiagnosticIsAvailable()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0));

			InvokeControllerInstanceMethod(
				editor,
				"_hoverController",
				"ShowBestToolTip",
				[typeof(LuaHoverInfo), typeof(bool), typeof(string), typeof(TextEditorDiagnosticSeverity)],
				null,
				true,
				"Warning message.",
				TextEditorDiagnosticSeverity.Warning);

			Popup popup = GetPrivateField<Popup>(editor, "_specialToolTip");
			ContentPresenter presenter = GetPrivateField<ContentPresenter>(editor, "_specialToolTipPresenter");

			Assert.IsTrue(popup.IsOpen);
			Assert.IsNotNull(presenter.Content);
			Assert.IsNotInstanceOfType(presenter.Content, typeof(StackPanel));
		});
	}

	[TestMethod]
	public void ShowBestHoverToolTip_SuppressesTooltipWhenCompletionWindowIsOpen()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0));
			editor.InitializeCompletionWindow();

			InvokeControllerInstanceMethod(
				editor,
				"_hoverController",
				"ShowBestToolTip",
				[typeof(LuaHoverInfo), typeof(bool), typeof(string), typeof(TextEditorDiagnosticSeverity)],
				new LuaHoverInfo("Hover docs.", false),
				true,
				"Warning message.",
				TextEditorDiagnosticSeverity.Warning);

			Popup popup = GetPrivateField<Popup>(editor, "_specialToolTip");
			ContentPresenter presenter = GetPrivateField<ContentPresenter>(editor, "_specialToolTipPresenter");

			Assert.IsFalse(popup.IsOpen);
			Assert.IsNull(presenter.Content);
		});
	}

	[TestMethod]
	public void DismissTransientToolTips_CancelsHoverAndClearsTransientUi()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0));
			var hoverCancellationTokenSource = new CancellationTokenSource();
			ContentPresenter signaturePresenter = GetSignatureHelpField<ContentPresenter>(editor, "_signaturePopupPresenter");

			SetHoverField(editor, "_hoverCancellationTokenSource", hoverCancellationTokenSource);
			SetHoverField(editor, "_hoverRequestToken", 4);
			SetSignatureHelpField(editor, "_signatureRequestToken", 2);
			SetSignatureHelpField(editor, "_signatureRequestInFlight", true);
			SetSignatureHelpField(editor, "_signatureRefreshPending", true);
			SetSignatureHelpField(editor, "_pendingSignatureHelpOffset", 7);
			signaturePresenter.Content = new TextBlock { Text = "signature" };

			editor.InitializeCompletionWindow();
			editor.ShowToolTip("Hover docs.");

			InvokeInstanceMethod(editor, "DismissTransientToolTips", Type.EmptyTypes);

			Assert.IsTrue(hoverCancellationTokenSource.IsCancellationRequested);
			Assert.IsNull(GetHoverFieldValue(editor, "_hoverCancellationTokenSource"));
			Assert.AreEqual(5, GetHoverField<int>(editor, "_hoverRequestToken"));
			Assert.IsNotNull(GetPrivateFieldValue(editor, "_completionWindow"));
			Assert.AreEqual(3, GetSignatureHelpField<int>(editor, "_signatureRequestToken"));
			Assert.IsFalse(GetSignatureHelpField<bool>(editor, "_signatureRequestInFlight"));
			Assert.IsFalse(GetSignatureHelpField<bool>(editor, "_signatureRefreshPending"));
			Assert.AreEqual(-1, GetSignatureHelpField<int>(editor, "_pendingSignatureHelpOffset"));
			Assert.IsNull(signaturePresenter.Content);
			Assert.IsFalse(GetPrivateField<Popup>(editor, "_specialToolTip").IsOpen);
		});
	}

	[TestMethod]
	public void NavigateToDefinitionAtCaretAsync_RaisesDefinitionNavigationRequestedForResolvedLocation()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaIntellisenseProvider
			{
				DefinitionResponse = new LuaDefinitionLocation(@"C:\Workspace\Definitions\spawn.lua", 4, 2)
			};

			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua",
				Text = "spawn()",
				IntellisenseProvider = provider,
				CaretOffset = 2
			};

			LuaDefinitionLocation? navigatedLocation = null;

			editor.DefinitionNavigationRequested += location => navigatedLocation = location;

			Window window = ShowInHostWindow(editor);

			try
			{
				editor.NavigateToDefinitionAtCaretAsync().GetAwaiter().GetResult();
			}
			finally
			{
				window.Close();
			}

			Assert.IsNotNull(navigatedLocation);
			Assert.AreEqual(provider.DefinitionResponse!.FilePath, navigatedLocation.FilePath);
			Assert.AreEqual(provider.DefinitionResponse.LineNumber, navigatedLocation.LineNumber);
			Assert.AreEqual(provider.DefinitionResponse.ColumnNumber, navigatedLocation.ColumnNumber);
			Assert.AreEqual(1, provider.DefinitionRequests.Count);
			Assert.AreEqual(0, provider.DefinitionRequests[0].Line);
			Assert.AreEqual(0, provider.DefinitionRequests[0].Column);
		});
	}

	[TestMethod]
	public void NavigateToDefinitionAtCaretAsync_DoesNotRaiseEventWhenProviderReturnsNoDefinition()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaIntellisenseProvider();

			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua",
				Text = "spawn()",
				IntellisenseProvider = provider,
				CaretOffset = 2
			};

			int navigationRequestCount = 0;

			editor.DefinitionNavigationRequested += _ => navigationRequestCount++;

			Window window = ShowInHostWindow(editor);

			try
			{
				editor.NavigateToDefinitionAtCaretAsync().GetAwaiter().GetResult();
			}
			finally
			{
				window.Close();
			}

			Assert.AreEqual(0, navigationRequestCount);
			Assert.AreEqual(1, provider.DefinitionRequests.Count);
		});
	}

	[TestMethod]
	public void RequestSignatureHelpAsync_DismissesExistingPopupWhenProviderReturnsNoSignature()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaIntellisenseProvider();

			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua",
				Text = "spawn(",
				IntellisenseProvider = provider
			};

			Window window = ShowInHostWindow(editor);

			try
			{
				Popup popup = GetSignatureHelpField<Popup>(editor, "_signaturePopup");
				ContentPresenter presenter = GetSignatureHelpField<ContentPresenter>(editor, "_signaturePopupPresenter");

				presenter.Content = new TextBlock { Text = "signature" };
				popup.IsOpen = true;

				Task requestTask = (Task)(InvokeInstanceMethod(editor, "RequestSignatureHelpAsync", [typeof(int)], 6)
					?? throw new InvalidOperationException("Private instance method 'RequestSignatureHelpAsync' returned null."));

				requestTask.GetAwaiter().GetResult();

				Assert.IsFalse(popup.IsOpen);
				Assert.IsNull(presenter.Content);
				Assert.AreEqual(1, provider.SignatureRequests.Count);
			}
			finally
			{
				window.Close();
			}
		});
	}

	[TestMethod]
	public void RequestSignatureHelpAsync_WhenRequestIsInFlight_DefersRefreshToLatestOffset()
	{
		RunInSta(() =>
		{
			var firstResponse = new TaskCompletionSource<LuaSignatureInfo?>(TaskCreationOptions.RunContinuationsAsynchronously);

			LuaSignatureInfo secondSignature = new(
				"spawn(room, objectName)",
				"Spawns an object.",
				[new LuaParameterInfo("room", "Room id."), new LuaParameterInfo("objectName", "Object name.")],
				1);

			int servedResponses = 0;

			var provider = new FakeLuaIntellisenseProvider
			{
				SignatureHelpHandler = (_, _) =>
				{
					servedResponses++;

					return servedResponses == 1
						? firstResponse.Task
						: Task.FromResult<LuaSignatureInfo?>(secondSignature);
				}
			};

			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua",
				Text = "spawn(room)",
				IntellisenseProvider = provider
			};

			Window window = ShowInHostWindow(editor);

			try
			{
				Task firstRequestTask = (Task)(InvokeInstanceMethod(editor, "RequestSignatureHelpAsync", [typeof(int)], 6)
					?? throw new InvalidOperationException("Private instance method 'RequestSignatureHelpAsync' returned null."));

				Task deferredRequestTask = (Task)(InvokeInstanceMethod(editor, "RequestSignatureHelpAsync", [typeof(int)], 11)
					?? throw new InvalidOperationException("Private instance method 'RequestSignatureHelpAsync' returned null."));

				deferredRequestTask.GetAwaiter().GetResult();

				Assert.IsTrue(GetSignatureHelpField<bool>(editor, "_signatureRequestInFlight"));
				Assert.IsTrue(GetSignatureHelpField<bool>(editor, "_signatureRefreshPending"));
				Assert.AreEqual(11, GetSignatureHelpField<int>(editor, "_pendingSignatureHelpOffset"));
				Assert.AreEqual(1, provider.SignatureRequests.Count);

				firstResponse.SetResult(new LuaSignatureInfo(
					"spawn(room)",
					"Spawns an object.",
					[new LuaParameterInfo("room", "Room id.")],
					0));

				firstRequestTask.GetAwaiter().GetResult();
				Assert.IsTrue(GetSignatureHelpField<DispatcherTimer>(editor, "_signatureRefreshTimer").IsEnabled);

				InvokeControllerInstanceMethod(editor, "_signatureHelpController", "HandleRefreshTimerTick", [typeof(object), typeof(EventArgs)], null, EventArgs.Empty);
				window.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));

				Assert.AreEqual(2, provider.SignatureRequests.Count);
				Assert.AreEqual(11, provider.SignatureRequests[1].Column);
				Assert.IsFalse(GetSignatureHelpField<bool>(editor, "_signatureRefreshPending"));
				Assert.AreEqual(11, GetSignatureHelpField<int>(editor, "_pendingSignatureHelpOffset"));
				Assert.IsTrue(GetSignatureHelpField<Popup>(editor, "_signaturePopup").IsOpen);
			}
			finally
			{
				window.Close();
			}
		});
	}

	[TestMethod]
	public void RequestSignatureHelpAsync_ShowsSignaturePopupForResolvedSignature()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaIntellisenseProvider
			{
				SignatureResponse = new LuaSignatureInfo(
					"spawn(room)",
					"Spawns an object.",
					[new LuaParameterInfo("room", "Room id.")],
					0)
			};

			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua",
				Text = "spawn(",
				IntellisenseProvider = provider
			};

			Window window = ShowInHostWindow(editor);

			try
			{
				Task requestTask = (Task)(InvokeInstanceMethod(editor, "RequestSignatureHelpAsync", [typeof(int)], 6)
					?? throw new InvalidOperationException("Private instance method 'RequestSignatureHelpAsync' returned null."));

				requestTask.GetAwaiter().GetResult();
			}
			finally
			{
				window.Close();
			}

			Popup popup = GetSignatureHelpField<Popup>(editor, "_signaturePopup");
			ContentPresenter presenter = GetSignatureHelpField<ContentPresenter>(editor, "_signaturePopupPresenter");

			Assert.IsTrue(popup.IsOpen);
			Assert.IsNotNull(presenter.Content);
			Assert.AreEqual(1, provider.SignatureRequests.Count);
			Assert.AreEqual(0, provider.SignatureRequests[0].Line);
			Assert.AreEqual(6, provider.SignatureRequests[0].Column);
		});
	}

	private static bool InvokePrivateStaticBooleanMethod(string methodName, Type[] parameterTypes, params object?[] arguments)
	{
		return (bool)(InvokeStaticMethod(typeof(LuaEditor), methodName, parameterTypes, arguments)
			?? throw new InvalidOperationException($"Private static method '{methodName}' returned null."));
	}

	private static bool InvokeTryGetCompletionTrigger(string? inputText, out char? triggerCharacter)
	{
		MethodInfo method = typeof(LuaEditor).GetMethod(
			"TryGetCompletionTrigger",
			BindingFlags.Static | BindingFlags.NonPublic,
			binder: null,
			[typeof(string), typeof(char?).MakeByRefType()],
			modifiers: null)
			?? throw new InvalidOperationException("Private static method 'TryGetCompletionTrigger' was not found.");

		object?[] arguments = [inputText, null];
		bool result = (bool)(method.Invoke(null, arguments)
			?? throw new InvalidOperationException("Private static method 'TryGetCompletionTrigger' returned null."));

		triggerCharacter = arguments[1] as char?;
		return result;
	}

	private static bool InvokeTryGetHoverRequestOffset(LuaEditor editor, int hoveredOffset, out int hoverOffset)
	{
		MethodInfo method = GetHoverController(editor).GetType().GetMethod(
			"TryGetRequestOffset",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			binder: null,
			[typeof(int), typeof(int).MakeByRefType()],
			modifiers: null)
			?? throw new InvalidOperationException("Hover controller method 'TryGetRequestOffset' was not found.");

		object?[] arguments = [hoveredOffset, 0];
		bool result = (bool)(method.Invoke(GetHoverController(editor), arguments)
			?? throw new InvalidOperationException("Hover controller method 'TryGetRequestOffset' returned null."));

		hoverOffset = (int)arguments[1]!;
		return result;
	}

	private static void InvokeControllerInstanceMethod(LuaEditor editor, string controllerFieldName, string methodName)
		=> InvokeInstanceMethod(GetPrivateField<object>(editor, controllerFieldName), methodName, Type.EmptyTypes);

	private static object? InvokeControllerInstanceMethod(LuaEditor editor, string controllerFieldName, string methodName, Type[] parameterTypes, params object?[] arguments)
		=> InvokeInstanceMethod(GetPrivateField<object>(editor, controllerFieldName), methodName, parameterTypes, arguments);

	private static object GetCompletionController(LuaEditor editor)
		=> GetPrivateField<object>(editor, "_completionController");

	private static T GetSignatureHelpField<T>(LuaEditor editor, string fieldName)
		=> GetPrivateField<T>(GetSignatureHelpController(editor), fieldName);

	private static T GetHoverField<T>(LuaEditor editor, string fieldName)
		=> GetPrivateField<T>(GetHoverController(editor), fieldName);

	private static object GetHoverController(LuaEditor editor)
		=> GetPrivateField<object>(editor, "_hoverController");

	private static object? GetHoverFieldValue(LuaEditor editor, string fieldName)
		=> GetPrivateFieldValue(GetHoverController(editor), fieldName);

	private static object GetSignatureHelpController(LuaEditor editor)
		=> GetPrivateField<object>(editor, "_signatureHelpController");

	private static void SetHoverField(LuaEditor editor, string fieldName, object value)
		=> SetPrivateField(GetHoverController(editor), fieldName, value);

	private static void SetSignatureHelpField(LuaEditor editor, string fieldName, object value)
		=> SetPrivateField(GetSignatureHelpController(editor), fieldName, value);

	private readonly record struct ProviderRequest(string FilePath, string Content, int Line, int Column);

	private sealed class FakeLuaIntellisenseProvider : ILuaIntellisenseProvider
	{
		public bool IsAvailable { get; set; } = true;
		public bool SupportsReferences => false;
		public bool SupportsRename => false;
		public bool SupportsFormatting => false;

		public LuaHoverInfo? HoverResponse { get; set; }

		public LuaDefinitionLocation? DefinitionResponse { get; set; }

		public LuaSignatureInfo? SignatureResponse { get; set; }

		public Func<ProviderRequest, CancellationToken, Task<LuaSignatureInfo?>>? SignatureHelpHandler { get; set; }

		public IReadOnlyList<LuaCompletionItem> CompletionItems { get; set; } = [];

		public List<ProviderRequest> DefinitionRequests { get; } = [];

		public List<ProviderRequest> SignatureRequests { get; } = [];

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

		public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath)
			=> [];

		public IReadOnlyList<LuaSemanticToken> GetSemanticTokens(string filePath)
			=> [];

		public void OpenDocument(string filePath, string content)
		{ }

		public void UpdateDocument(string filePath, string content)
		{ }

		public void CloseDocument(string filePath)
		{ }

		public void RenameDocument(string oldFilePath, string newFilePath, string content)
		{ }

		public Task<IReadOnlyList<LuaCompletionItem>> GetCompletionItemsAsync(string filePath, string content,
			int line, int column, char? triggerCharacter = null, CancellationToken cancellationToken = default)
			=> Task.FromResult(CompletionItems);

		public Task<LuaHoverInfo?> GetHoverAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
			=> Task.FromResult(HoverResponse);

		public Task<LuaDefinitionLocation?> GetDefinitionAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
		{
			DefinitionRequests.Add(new ProviderRequest(filePath, content, line, column));
			return Task.FromResult(DefinitionResponse);
		}

		public Task<IReadOnlyList<LuaReferenceLocation>> GetReferencesAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
			=> Task.FromResult<IReadOnlyList<LuaReferenceLocation>>([]);

		public Task<LuaWorkspaceEdit?> RenameSymbolAsync(string filePath, string content,
			int line, int column, string newName, CancellationToken cancellationToken = default)
			=> Task.FromResult<LuaWorkspaceEdit?>(null);

		public Task<IReadOnlyList<LuaTextEdit>> FormatDocumentAsync(string filePath, string content,
			LuaFormattingOptions options, CancellationToken cancellationToken = default)
			=> Task.FromResult<IReadOnlyList<LuaTextEdit>>([]);

		public Task<LuaSignatureInfo?> GetSignatureHelpAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
		{
			var request = new ProviderRequest(filePath, content, line, column);
			SignatureRequests.Add(request);

			if (SignatureHelpHandler is not null)
				return SignatureHelpHandler(request, cancellationToken);

			return Task.FromResult(SignatureResponse);
		}

		public void Dispose()
		{ }
	}
}
