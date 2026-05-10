#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal sealed partial class LuaLanguageServerIntellisenseProvider
{
	private void HandleSemanticTokensRefreshRequested()
		=> ObserveBackgroundTask(RefreshTrackedSemanticTokensAsync(CancellationToken.None), "Semantic tokens refresh");

	private async Task RefreshTrackedSemanticTokensAsync(CancellationToken cancellationToken)
	{
		if (_isDisposed || _client is null || _client.SemanticTokenTypes.Count == 0)
			return;

		IReadOnlyList<LuaDocumentSnapshot> documents = _documents.GetOpenDocuments();

		for (int i = 0; i < documents.Count; i++)
			await RefreshSemanticTokensAsync(documents[i], cancellationToken).ConfigureAwait(false);
	}

	private async Task RefreshSemanticTokensAsync(LuaDocumentSnapshot document, CancellationToken cancellationToken)
	{
		if (_client is null || _client.SemanticTokenTypes.Count == 0)
			return;

		CancellationToken effectiveToken = ReplaceSemanticTokenRequest(document.FilePath, cancellationToken, out CancellationTokenSource? linkedSource);

		try
		{
			LuaSemanticTokensDeltaState deltaState = _documents.GetSemanticTokensDeltaState(document.FilePath);
			bool useDelta = _client.SupportsSemanticTokensDelta
				&& deltaState.PreviousResultId is not null
				&& deltaState.PreviousData is not null;
			JsonElement response = await SendSemanticTokensRequestAsync(document, deltaState.PreviousResultId, useDelta, effectiveToken).ConfigureAwait(false);

			if (response.ValueKind == JsonValueKind.Undefined)
				return;

			LuaSemanticTokensDecodeResult decodeResult = DecodeSemanticTokensResponse(response, document, deltaState.PreviousData, useDelta);

			if (decodeResult.RetryWithFullRefresh)
			{
				_documents.StoreSemanticTokensDeltaState(document.FilePath, null, null);

				JsonElement fullResponse = await SendSemanticTokensRequestAsync(document, previousResultId: null, useDelta: false, effectiveToken).ConfigureAwait(false);

				if (fullResponse.ValueKind == JsonValueKind.Undefined)
					return;

				decodeResult = DecodeSemanticTokensResponse(fullResponse, document, previousData: null, deltaWasRequested: false);
			}

			_documents.StoreSemanticTokensDeltaState(document.FilePath, decodeResult.ResultId, decodeResult.Data);

			if (!_documents.TryStoreSemanticTokens(document.FilePath, document.Version, decodeResult.Tokens))
				return;

			RaiseSemanticTokensUpdated(document.FilePath, decodeResult.Tokens);
		}
		catch (OperationCanceledException) when (effectiveToken.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
		{
			// A newer document version superseded this request.
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (IOException exception)
		{
			Log.Debug(exception, "Lua semantic tokens request failed for '{FilePath}' due to a transport error; falling back to TextMate highlighting until the next sync.",
				document.FilePath);
		}
		catch (ObjectDisposedException)
		{
			// The client was torn down between scheduling and dispatch.
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Lua semantic tokens request failed for '{FilePath}'; falling back to TextMate highlighting.",
				document.FilePath);
		}
		finally
		{
			ClearSemanticTokenRequest(document.FilePath, linkedSource);
		}
	}

	private Task<JsonElement> SendSemanticTokensRequestAsync(
		LuaDocumentSnapshot document,
		string? previousResultId,
		bool useDelta,
		CancellationToken cancellationToken)
	{
		if (_client is null)
			return Task.FromResult(default(JsonElement));

		string method = useDelta ? "textDocument/semanticTokens/full/delta" : "textDocument/semanticTokens/full";
		object parameters = useDelta
			? new { textDocument = new { uri = document.Uri }, previousResultId }
			: new { textDocument = new { uri = document.Uri } };

		return SendBoundedRequestAsync(_client, method, parameters, cancellationToken);
	}

	private LuaSemanticTokensDecodeResult DecodeSemanticTokensResponse(
		JsonElement response, LuaDocumentSnapshot document, int[]? previousData, bool deltaWasRequested)
	{
		if (_client is null)
			return new LuaSemanticTokensDecodeResult([], null, null, false);

		if (deltaWasRequested)
		{
			LuaSemanticTokensDeltaResponse delta = LuaLanguageServerSemanticTokensDeltaParser.Parse(response);

			if (delta.Edits is { } edits && previousData is not null)
			{
				int[]? patchedData = LuaLanguageServerSemanticTokensDeltaParser.ApplyEdits(previousData, edits);

				if (patchedData is not null)
				{
					IReadOnlyList<LuaSemanticToken> tokens = LuaLanguageServerSemanticTokensDecoder.Decode(
						patchedData, document, _client.SemanticTokenTypes, _client.SemanticTokenModifiers);
					return new LuaSemanticTokensDecodeResult(tokens, patchedData, delta.ResultId, false);
				}

				Log.Debug("Lua semantic-tokens delta edits could not be applied for '{FilePath}'; falling back to a full reparse.", document.FilePath);
				return new LuaSemanticTokensDecodeResult([], null, null, true);
			}

			if (delta.Data is { } fullData)
			{
				IReadOnlyList<LuaSemanticToken> tokens = LuaLanguageServerSemanticTokensDecoder.Decode(
					fullData, document, _client.SemanticTokenTypes, _client.SemanticTokenModifiers);

				return new LuaSemanticTokensDecodeResult(tokens, fullData, delta.ResultId, false);
			}

			Log.Debug("Lua semantic-tokens delta response for '{FilePath}' did not contain usable data; requesting a full refresh.", document.FilePath);
			return new LuaSemanticTokensDecodeResult([], null, null, true);
		}

		LuaSemanticTokensDeltaResponse fullResponse = LuaLanguageServerSemanticTokensDeltaParser.Parse(response);

		if (fullResponse.Data is { } data)
		{
			IReadOnlyList<LuaSemanticToken> tokens = LuaLanguageServerSemanticTokensDecoder.Decode(
				data, document, _client.SemanticTokenTypes, _client.SemanticTokenModifiers);

			return new LuaSemanticTokensDecodeResult(tokens, data, fullResponse.ResultId, false);
		}

		return new LuaSemanticTokensDecodeResult([], null, fullResponse.ResultId, false);
	}

	private CancellationToken ReplaceSemanticTokenRequest(string filePath, CancellationToken cancellationToken, out CancellationTokenSource? linkedSource)
	{
		var freshSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		CancellationTokenSource? previousSource = null;

		_semanticTokenRequests.AddOrUpdate(
			filePath,
			freshSource,
			(_, existing) =>
			{
				previousSource = existing;
				return freshSource;
			});

		CancelAndDispose(previousSource);

		linkedSource = freshSource;
		return freshSource.Token;
	}

	private void ClearSemanticTokenRequest(string filePath, CancellationTokenSource? linkedSource)
	{
		if (linkedSource is null)
			return;

		_semanticTokenRequests.TryRemove(new KeyValuePair<string, CancellationTokenSource>(filePath, linkedSource));
		linkedSource.Dispose();
	}

	private void CancelSemanticTokenRequest(string filePath)
	{
		if (_semanticTokenRequests.TryRemove(filePath, out CancellationTokenSource? source))
			CancelAndDispose(source);
	}

	private void CancelAllSemanticTokenRequests()
	{
		if (_semanticTokenRequests.IsEmpty)
			return;

		foreach (KeyValuePair<string, CancellationTokenSource> entry in _semanticTokenRequests)
		{
			if (_semanticTokenRequests.TryRemove(entry.Key, out CancellationTokenSource? source))
				CancelAndDispose(source);
		}
	}

	private static void CancelAndDispose(CancellationTokenSource? source)
	{
		if (source is null)
			return;

		try
		{
			source.Cancel();
		}
		catch (ObjectDisposedException)
		{ }
		finally
		{
			source.Dispose();
		}
	}
}
