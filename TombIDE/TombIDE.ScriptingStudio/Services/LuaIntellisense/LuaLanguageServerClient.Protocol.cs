#nullable enable

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal sealed partial class LuaLanguageServerClient
{
	private Task SendNotificationCoreAsync(string method, object parameters, CancellationToken cancellationToken, bool allowDisposed)
		=> SendNotificationCoreAsync(GetRequiredActiveSession(allowDisposed), method, parameters, cancellationToken, allowDisposed);

	private Task SendNotificationCoreAsync(LuaLanguageServerTransportSession session, string method, object parameters, CancellationToken cancellationToken, bool allowDisposed)
		=> WriteMessageAsync(session, new { jsonrpc = "2.0", method, @params = parameters }, cancellationToken, allowDisposed);

	private async Task<JsonElement> SendRequestCoreAsync(string method, object parameters, CancellationToken cancellationToken, bool allowDisposed)
		=> await SendRequestCoreAsync(GetRequiredActiveSession(allowDisposed), method, parameters, cancellationToken, allowDisposed).ConfigureAwait(false);

	private async Task<JsonElement> SendRequestCoreAsync(LuaLanguageServerTransportSession session, string method, object parameters, CancellationToken cancellationToken, bool allowDisposed)
	{
		long requestId = Interlocked.Increment(ref _requestId);
		var responseSource = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);

		if (!session.PendingRequests.TryAdd(requestId, responseSource))
			throw new InvalidOperationException("Unable to track a new language server request.");

		await using var registration = cancellationToken.Register(() => responseSource.TrySetCanceled(cancellationToken));

		try
		{
			await WriteMessageAsync(session, new { jsonrpc = "2.0", id = requestId, method, @params = parameters }, cancellationToken, allowDisposed).ConfigureAwait(false);
			return await responseSource.Task.ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			SendCancelNotification(session, requestId);
			throw;
		}
		finally
		{
			session.PendingRequests.TryRemove(requestId, out _);
		}
	}

	private void SendCancelNotification(LuaLanguageServerTransportSession session, long requestId)
	{
		if (_isDisposed)
			return;

		Task.Run(async () =>
		{
			try
			{
				await WriteMessageAsync(session,
					new { jsonrpc = "2.0", method = "$/cancelRequest", @params = new { id = requestId } },
					CancellationToken.None,
					allowDisposed: true).ConfigureAwait(false);
			}
			catch
			{
				// Best-effort: cancellation notifications are advisory.
			}
		});
	}

	private async Task HandleServerRequestAsync(LuaLanguageServerTransportSession session, JsonElement idElement, string method, JsonElement parameters)
	{
		switch (method)
		{
			case "workspace/configuration":
				await WriteResponseAsync(session, idElement, BuildConfigurationResponse(parameters)).ConfigureAwait(false);
				return;

			case "workspace/workspaceFolders":
				await WriteResponseAsync(session, idElement, BuildWorkspaceFolderResponse()).ConfigureAwait(false);
				return;

			case "workspace/semanticTokens/refresh":
				try
				{
					SemanticTokensRefreshRequested?.Invoke();
				}
				catch (Exception exception)
				{
					Log.Warn(exception, "Lua semantic-tokens refresh request handler threw; acknowledging the request anyway.");
				}

				await WriteResponseAsync(session, idElement, result: null).ConfigureAwait(false);
				return;

			case "client/registerCapability":
			case "client/unregisterCapability":
			case "window/workDoneProgress/create":
				// Acknowledge with a successful empty result so the server can proceed.
				await WriteResponseAsync(session, idElement, result: null).ConfigureAwait(false);
				return;

			default:
				// Reply with JSON-RPC "Method Not Found" (-32601) for everything else so the server does not block on us.
				await WriteErrorResponseAsync(session, idElement, code: -32601, message: $"Method '{method}' is not supported by the client.").ConfigureAwait(false);
				return;
		}
	}

	private Task WriteResponseAsync(LuaLanguageServerTransportSession session, JsonElement idElement, object? result)
		=> WriteMessageAsync(session, new { jsonrpc = "2.0", id = idElement, result }, CancellationToken.None, allowDisposed: true);

	private Task WriteErrorResponseAsync(LuaLanguageServerTransportSession session, JsonElement idElement, int code, string message)
		=> WriteMessageAsync(session, new { jsonrpc = "2.0", id = idElement, error = new { code, message } }, CancellationToken.None, allowDisposed: true);

	private object[] BuildConfigurationResponse(JsonElement parameters)
	{
		if (!parameters.TryGetProperty("items", out JsonElement itemsElement) || itemsElement.ValueKind != JsonValueKind.Array)
			return [];

		JsonElement settingsElement = JsonSerializer.SerializeToElement(_settingsProvider());
		JsonElement luaElement = settingsElement.GetProperty("Lua");

		var results = new List<object>();

		foreach (JsonElement item in itemsElement.EnumerateArray())
		{
			string? section = item.TryGetProperty("section", out JsonElement sectionElement)
				? sectionElement.GetString()
				: null;

			results.Add(GetConfigurationSection(settingsElement, luaElement, section));
		}

		return [.. results];
	}

	private static object GetConfigurationSection(JsonElement settingsElement, JsonElement luaElement, string? section)
	{
		if (string.IsNullOrWhiteSpace(section))
			return settingsElement.Clone();

		if (section.Equals("Lua", StringComparison.OrdinalIgnoreCase))
			return luaElement.Clone();

		if (section.StartsWith("Lua.", StringComparison.OrdinalIgnoreCase))
		{
			JsonElement nestedSection = luaElement;
			string[] parts = section[4..].Split('.');

			foreach (string part in parts)
			{
				if (!nestedSection.TryGetProperty(part, out JsonElement nextSection))
					return new { };

				nestedSection = nextSection;
			}

			return nestedSection.Clone();
		}

		return new { };
	}

	private object[] BuildWorkspaceFolderResponse() =>
	[
		new
		{
			uri = LuaLanguageServerPathHelper.CreateFileUri(_workspaceRootDirectoryPath),
			name = Path.GetFileName(_workspaceRootDirectoryPath)
		}
	];

	private async Task WriteMessageAsync(LuaLanguageServerTransportSession session, object payload, CancellationToken cancellationToken, bool allowDisposed)
	{
		ThrowIfDisposed(allowDisposed);

		byte[] payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload);

		// Header layout: "Content-Length: <digits>\r\n\r\n". Worst-case digits fit comfortably in 32 bytes.
		const int HeaderReserve = 64;
		byte[] frameBuffer = ArrayPool<byte>.Shared.Rent(HeaderReserve + payloadBytes.Length);
		int headerLength = WriteContentLengthHeader(frameBuffer, payloadBytes.Length);
		int frameLength = headerLength + payloadBytes.Length;

		try
		{
			Buffer.BlockCopy(payloadBytes, 0, frameBuffer, headerLength, payloadBytes.Length);

			await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);

			try
			{
				await session.OutputStream.WriteAsync(frameBuffer.AsMemory(0, frameLength), cancellationToken).ConfigureAwait(false);
				await session.OutputStream.FlushAsync(cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				_writeLock.Release();
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(frameBuffer);
		}
	}

	private static int WriteContentLengthHeader(Span<byte> destination, int payloadLength)
	{
		ReadOnlySpan<byte> prefix = "Content-Length: "u8;
		ReadOnlySpan<byte> terminator = "\r\n\r\n"u8;

		prefix.CopyTo(destination);
		int position = prefix.Length;

		if (!Utf8Formatter.TryFormat(payloadLength, destination[position..], out int digitsWritten))
			throw new InvalidOperationException("Failed to encode the LSP Content-Length header.");

		position += digitsWritten;
		terminator.CopyTo(destination[position..]);
		return position + terminator.Length;
	}
}
