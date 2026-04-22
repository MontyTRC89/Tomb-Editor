#nullable enable

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal sealed partial class LuaLanguageServerClient
{
	private async Task ReadLoopAsync()
	{
		Exception? failure = null;

		try
		{
			while (!_isDisposed)
			{
				int? contentLengthValue = await ReadHeadersAsync().ConfigureAwait(false);

				if (contentLengthValue is null || !TryAcceptPayloadSize(contentLengthValue.Value, out int contentLength))
					break;

				byte[]? payloadBytes = await ReadPayloadAsync(contentLength).ConfigureAwait(false);

				if (payloadBytes is null)
					break;

				using JsonDocument document = JsonDocument.Parse(payloadBytes);
				await HandleMessageAsync(document.RootElement).ConfigureAwait(false);
			}
		}
		catch (Exception exception)
		{
			failure = exception;
		}
		finally
		{
			IsReady = false;

			if (!_pendingRequests.IsEmpty)
				FailPendingRequests(failure ?? new IOException("The Lua language server connection was closed."));
		}
	}

	private async Task ReadStandardErrorLoopAsync()
	{
		try
		{
			while (!_isDisposed)
			{
				Process? process = _process;

				if (process is null || process.HasExited)
					break;

				string? line = await process.StandardError.ReadLineAsync(_lifetimeCts.Token).ConfigureAwait(false);

				if (line is null)
					break;

				if (!string.IsNullOrWhiteSpace(line))
					Log.Debug("[LuaLS stderr] {Line}", line);
			}
		}
		catch (OperationCanceledException)
		{ }
		catch
		{
			// Ignore stderr read failures.
		}
	}

	private async Task<int?> ReadHeadersAsync()
	{
		while (true)
		{
			int headerTerminatorIndex = FindHeaderTerminatorIndex();

			if (headerTerminatorIndex >= 0)
				return ConsumeHeaders(headerTerminatorIndex);

			if (!await ReadIntoReceiveBufferAsync().ConfigureAwait(false))
				return null;
		}
	}

	private int? ConsumeHeaders(int headerTerminatorIndex)
	{
		string headerText = Encoding.ASCII.GetString(_receiveBuffer, 0, headerTerminatorIndex);
		ConsumeReceiveBuffer(headerTerminatorIndex + HeaderTerminator.Length);
		return ExtractContentLength(headerText);
	}

	private static int? ExtractContentLength(string headerText)
	{
		foreach (string line in headerText.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries))
		{
			int separatorIndex = line.IndexOf(':');

			if (separatorIndex <= 0)
				continue;

			ReadOnlySpan<char> key = line.AsSpan(0, separatorIndex).Trim();

			if (!key.Equals("Content-Length".AsSpan(), StringComparison.OrdinalIgnoreCase))
				continue;

			ReadOnlySpan<char> value = line.AsSpan(separatorIndex + 1).Trim();

			if (int.TryParse(value, out int parsedContentLength) && parsedContentLength > 0)
				return parsedContentLength;

			return null;
		}

		return null;
	}

	private int FindHeaderTerminatorIndex()
		=> new ReadOnlySpan<byte>(_receiveBuffer, 0, _receiveBufferCount).IndexOf(HeaderTerminator);

	private async Task<bool> ReadIntoReceiveBufferAsync()
	{
		if (_inputStream is null)
			return false;

		EnsureReceiveBufferCapacity(_receiveBufferCount + ReceiveChunkSize);

		int bytesAvailable = _receiveBuffer.Length - _receiveBufferCount;
		int bytesRead = await _inputStream
			.ReadAsync(_receiveBuffer.AsMemory(_receiveBufferCount, bytesAvailable), _lifetimeCts.Token)
			.ConfigureAwait(false);

		if (bytesRead == 0)
			return false;

		_receiveBufferCount += bytesRead;
		return true;
	}

	private void EnsureReceiveBufferCapacity(int requiredCapacity)
	{
		if (_receiveBuffer.Length >= requiredCapacity)
			return;

		int newCapacity = Math.Max(ReceiveChunkSize, _receiveBuffer.Length);

		while (newCapacity < requiredCapacity)
			newCapacity *= 2;

		byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newCapacity);

		if (_receiveBufferCount > 0)
			Buffer.BlockCopy(_receiveBuffer, 0, newBuffer, 0, _receiveBufferCount);

		ReturnReceiveBuffer();
		_receiveBuffer = newBuffer;
	}

	private void ReturnReceiveBuffer()
	{
		if (_receiveBuffer.Length == 0)
			return;

		ArrayPool<byte>.Shared.Return(_receiveBuffer, clearArray: false);
		_receiveBuffer = [];
	}

	private void ConsumeReceiveBuffer(int bytesToConsume)
	{
		int remainingBytes = _receiveBufferCount - bytesToConsume;

		if (remainingBytes > 0)
			Buffer.BlockCopy(_receiveBuffer, bytesToConsume, _receiveBuffer, 0, remainingBytes);

		_receiveBufferCount = Math.Max(0, remainingBytes);
	}

	private async Task<byte[]?> ReadPayloadAsync(int contentLength)
	{
		if (_inputStream is null)
			return null;

		byte[] payloadBytes = new byte[contentLength];
		int totalBytesRead = 0;

		if (_receiveBufferCount > 0)
		{
			int bufferedBytesToCopy = Math.Min(contentLength, _receiveBufferCount);
			Buffer.BlockCopy(_receiveBuffer, 0, payloadBytes, 0, bufferedBytesToCopy);
			ConsumeReceiveBuffer(bufferedBytesToCopy);
			totalBytesRead = bufferedBytesToCopy;
		}

		while (totalBytesRead < contentLength)
		{
			int bytesRead = await _inputStream
				.ReadAsync(payloadBytes.AsMemory(totalBytesRead, contentLength - totalBytesRead), _lifetimeCts.Token)
				.ConfigureAwait(false);

			if (bytesRead == 0)
				return null;

			totalBytesRead += bytesRead;
		}

		return payloadBytes;
	}

	private async Task HandleMessageAsync(JsonElement message)
	{
		bool hasId = message.TryGetProperty("id", out JsonElement idElement);
		bool hasMethod = message.TryGetProperty("method", out JsonElement methodElement);

		if (hasId && !hasMethod)
		{
			HandleServerResponse(idElement, message);
			return;
		}

		if (!hasMethod)
			return;

		string? method = methodElement.GetString();

		if (string.IsNullOrWhiteSpace(method))
			return;

		JsonElement parameters = message.TryGetProperty("params", out JsonElement paramsElement)
			? paramsElement.Clone()
			: default;

		if (hasId)
			await HandleServerRequestAsync(idElement.Clone(), method, parameters).ConfigureAwait(false);
		else
			HandleServerNotification(method, parameters);
	}

	private void HandleServerResponse(JsonElement idElement, JsonElement message)
	{
		if (!idElement.TryGetInt64(out long requestId))
			return;

		if (!_pendingRequests.TryGetValue(requestId, out TaskCompletionSource<JsonElement>? responseSource))
		{
			Log.Debug("Received Lua language server response with no matching pending request (id={RequestId}).", requestId);
			return;
		}

		if (message.TryGetProperty("error", out JsonElement errorElement))
		{
			string messageText = errorElement.TryGetProperty("message", out JsonElement errorMessageElement)
				? errorMessageElement.GetString() ?? "Lua language server request failed."
				: "Lua language server request failed.";

			responseSource.TrySetException(new InvalidOperationException(messageText));
			return;
		}

		if (message.TryGetProperty("result", out JsonElement resultElement))
			responseSource.TrySetResult(resultElement.Clone());
		else
			responseSource.TrySetResult(default);
	}

	private void HandleServerNotification(string method, JsonElement parameters)
	{
		switch (method)
		{
			case "textDocument/publishDiagnostics":
				RaiseDiagnosticsPublished(parameters);
				return;

			case "window/logMessage":
			case "window/showMessage":
				LogServerMessage(method, parameters);
				return;

			case "telemetry/event":
			case "$/progress":
				return;
		}
	}

	private void RaiseDiagnosticsPublished(JsonElement parameters)
	{
		// Stash only the newest diagnostics payload per file and wake the pump if it is idle.
		// Parameters were already cloned by the dispatcher in HandleMessageAsync.
		_pendingDiagnostics[GetDiagnosticsQueueKey(parameters)] = parameters;
		_diagnosticsSignal.Writer.TryWrite(true);
	}

	private async Task PumpDiagnosticsAsync()
	{
		ChannelReader<bool> reader = _diagnosticsSignal.Reader;

		try
		{
			while (await reader.WaitToReadAsync(_lifetimeCts.Token).ConfigureAwait(false))
			{
				while (reader.TryRead(out _))
				{ }

				while (!_pendingDiagnostics.IsEmpty)
				{
					KeyValuePair<string, JsonElement>[] pendingDiagnostics = [.. _pendingDiagnostics];

					for (int i = 0; i < pendingDiagnostics.Length; i++)
					{
						if (!_pendingDiagnostics.TryRemove(pendingDiagnostics[i].Key, out JsonElement parameters))
							continue;

						try
						{
							DiagnosticsPublished?.Invoke(parameters);
						}
						catch (Exception exception)
						{
							Log.Warn(exception, "Lua diagnostics handler threw; the diagnostics pump is being kept alive.");
						}
					}
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Expected on dispose.
		}
	}

	private string GetDiagnosticsQueueKey(JsonElement parameters)
	{
		if (parameters.ValueKind == JsonValueKind.Object
			&& parameters.TryGetProperty("uri", out JsonElement uriElement)
			&& uriElement.ValueKind == JsonValueKind.String)
		{
			string? uri = uriElement.GetString();

			if (!string.IsNullOrWhiteSpace(uri))
				return uri;
		}

		return "diagnostics:" + Interlocked.Increment(ref _diagnosticsFallbackSequence);
	}

	private static void LogServerMessage(string method, JsonElement parameters)
	{
		if (parameters.ValueKind != JsonValueKind.Object)
			return;

		string? messageText = parameters.TryGetProperty("message", out JsonElement messageElement)
			? messageElement.GetString()
			: null;

		if (string.IsNullOrWhiteSpace(messageText))
			return;

		int messageType = parameters.TryGetProperty("type", out JsonElement typeElement)
			&& typeElement.TryGetInt32(out int parsedType) ? parsedType : 4;

		switch (messageType)
		{
			case 1: Log.Error("[LuaLS {Method}] {Message}", method, messageText); break;
			case 2: Log.Warn("[LuaLS {Method}] {Message}", method, messageText); break;
			case 3: Log.Info("[LuaLS {Method}] {Message}", method, messageText); break;
			default: Log.Debug("[LuaLS {Method}] {Message}", method, messageText); break;
		}
	}
}
