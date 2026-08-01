using System;
using System.Threading;
using System.Threading.Tasks;
using Nickelony.LanguageServer.Abstractions.Navigation;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	/// <summary>
	/// Owns Lua definition-navigation request state and the editor-side flow for F12 and Ctrl+Click navigation.
	/// </summary>
	private sealed class LuaDefinitionNavigationController
	{
		private readonly LuaEditor _editor;
		private CancellationTokenSource? _definitionCancellationTokenSource;
		private int _definitionRequestToken;

		internal LuaDefinitionNavigationController(LuaEditor editor)
		{
			_editor = editor;
		}

		internal void CancelPendingRequest()
			=> CancelAndDispose(ref _definitionCancellationTokenSource);

		internal void InvalidateRequests()
			=> _definitionRequestToken++;

		internal async Task<bool> TryNavigateAsync(int offset, CancellationToken cancellationToken)
		{
			if (!_editor.IsIntellisenseAvailable())
				return false;

			var intellisenseProvider = _editor.IntellisenseProvider;

			if (intellisenseProvider is null)
				return false;

			try
			{
				if (!LuaEditorInteractionRules.TryGetDefinitionStartOffset(_editor.Document, offset, out int definitionOffset))
					return false;

				CancellationToken definitionCancellationToken = ResetCancellationTokenSource(ref _definitionCancellationTokenSource);
				using CancellationTokenSource? linkedCancellationTokenSource = cancellationToken.CanBeCanceled
					? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, definitionCancellationToken)
					: null;

				CancellationToken effectiveCancellationToken = linkedCancellationTokenSource?.Token ?? definitionCancellationToken;
				int requestToken = ++_definitionRequestToken;
				int requestDocumentVersion = _editor._editorDocumentVersion;
				int requestGeneration = _editor._editorRequestGeneration;

				(int line, int column) = _editor.GetPositionFromOffset(definitionOffset);

				TextDefinitionLocation? definitionLocation = await intellisenseProvider
					.GetDefinitionAsync(_editor.FilePath, _editor.Text, line, column, effectiveCancellationToken)
					.ConfigureAwait(true);

				if (!_editor.IsAsyncEditorResultCurrent(effectiveCancellationToken, requestToken, _definitionRequestToken,
					requestDocumentVersion, requestGeneration))
				{
					return false;
				}

				if (definitionLocation is null)
					return false;

				_editor.DefinitionNavigationRequested?.Invoke(definitionLocation);
				return true;
			}
			catch (OperationCanceledException)
			{
				return false;
			}
			catch (Exception exception)
			{
				LogEditorFailure("Go to definition", exception);
				return false;
			}
		}
	}
}
