using StreamJsonRpc;
using System.Text.Json;

namespace Nickelony.LanguageServer.Core;

public sealed partial class LanguageServerClient
{
	/// <summary>
	/// Sends a JSON-RPC notification to the language server.
	/// </summary>
	/// <param name="method">The LSP method name.</param>
	/// <param name="parameters">The notification payload.</param>
	/// <param name="cancellationToken">A token that can cancel the local dispatch attempt while the JSON-RPC notification task is still incomplete.</param>
	public async Task SendNotificationAsync(string method, object parameters, CancellationToken cancellationToken)
	{
		LanguageServerTransportSession session = GetRequiredReadySession(allowDisposed: false);

		try
		{
			await SendNotificationCoreAsync(session, method, parameters, cancellationToken, allowDisposed: false).ConfigureAwait(false);
			TryRefreshCachedSettingsSnapshotFromNotification(method, parameters);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception exception) when (IsTransportOperationFailure(exception))
		{
			TryMarkTransportUnhealthy(session.Generation);
			LogTransportOperationFailure("notification", method, session.Generation, exception);

			throw new LanguageServerTransportUnavailableException(innerException: exception, message: null);
		}
		catch (Exception exception)
		{
			LogNonTransportNotificationFailure(method, session.Generation, exception);
			throw;
		}
	}

	/// <summary>
	/// Sends a JSON-RPC request to the language server and returns the typed response payload.
	/// </summary>
	/// <typeparam name="TResult">The typed response payload to deserialize.</typeparam>
	/// <param name="method">The LSP method name.</param>
	/// <param name="parameters">The request payload.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The typed response payload.</returns>
	public async Task<TResult> SendRequestAsync<TResult>(string method, object parameters, CancellationToken cancellationToken)
	{
		LanguageServerTransportSession session = GetRequiredReadySession(allowDisposed: false);

		TResult result;

		try
		{
			result = await SendRequestCoreAsync<TResult>(session, method, parameters, cancellationToken, allowDisposed: false).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception exception) when (IsTransportOperationFailure(exception))
		{
			TryMarkTransportUnhealthy(session.Generation);
			LogTransportOperationFailure("request", method, session.Generation, exception);

			throw new LanguageServerTransportUnavailableException(innerException: exception, message: null);
		}
		catch (Exception exception)
		{
			LogNonTransportRequestFailure(method, session.Generation, exception);
			throw;
		}

		if (!CanAcceptRequestResultForSession(session))
		{
			_logger.LogDebug(
				"Discarding language server request '{Method}' result from transport generation {Generation} because the transport was superseded or marked unavailable before completion.",
				method,
				session.Generation);

			throw new LanguageServerTransportChangedException();
		}

		return result;
	}

	/// <summary>
	/// Sends a JSON-RPC notification over a specific transport session.
	/// </summary>
	/// <param name="session">The target transport session.</param>
	/// <param name="method">The JSON-RPC method name.</param>
	/// <param name="parameters">The notification payload.</param>
	/// <param name="cancellationToken">Cancels waiting for local JSON-RPC dispatch while the notification task is still incomplete.</param>
	/// <param name="allowDisposed">Whether disposed-state checks should be skipped.</param>
	/// <returns>The send task.</returns>
	private async Task SendNotificationCoreAsync(LanguageServerTransportSession session, string method, object parameters, CancellationToken cancellationToken, bool allowDisposed)
	{
		ThrowIfDisposed(allowDisposed);

		if (cancellationToken.IsCancellationRequested)
			await Task.FromCanceled(cancellationToken).ConfigureAwait(false);

		JsonRpc jsonRpc = session.JsonRpc
			?? throw new IOException("The language server JSON-RPC transport is not available.");

		Task notificationTask = jsonRpc.NotifyWithParameterObjectAsync(method, parameters);

		if (!cancellationToken.CanBeCanceled)
		{
			await notificationTask.ConfigureAwait(false);
			return;
		}

		// StreamJsonRpc may complete the notification task once local dispatch is handed off,
		// before the peer has necessarily flushed or processed the bytes.
		await notificationTask.WaitAsync(cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Sends a JSON-RPC request over a specific transport session.
	/// </summary>
	/// <typeparam name="TResult">The expected response payload type.</typeparam>
	/// <param name="session">The target transport session.</param>
	/// <param name="method">The JSON-RPC method name.</param>
	/// <param name="parameters">The request payload.</param>
	/// <param name="cancellationToken">Cancels the request. The transport does not impose its own default timeout.</param>
	/// <param name="allowDisposed">Whether disposed-state checks should be skipped.</param>
	/// <returns>The typed response task.</returns>
	private Task<TResult> SendRequestCoreAsync<TResult>(LanguageServerTransportSession session, string method, object parameters, CancellationToken cancellationToken, bool allowDisposed)
	{
		ThrowIfDisposed(allowDisposed);

		JsonRpc jsonRpc = session.JsonRpc
			?? throw new IOException("The language server JSON-RPC transport is not available.");

		return jsonRpc.InvokeWithParameterObjectAsync<TResult>(method, parameters, cancellationToken);
	}

	private static bool IsTransportOperationFailure(Exception exception)
		=> exception is IOException or ObjectDisposedException;

	/// <summary>
	/// Builds the configuration response payload requested by the language server.
	/// </summary>
	/// <param name="parameters">The requested configuration sections.</param>
	/// <returns>The configuration response objects in request order.</returns>
	private object?[] BuildConfigurationResponse(WorkspaceConfigurationParams parameters)
	{
		WorkspaceConfigurationItem[] items = parameters.Items ?? [];

		if (items.Length == 0)
			return [];

		try
		{
			JsonElement settingsElement = GetCachedSettingsSnapshot().SettingsElement;
			var results = new object?[items.Length];

			for (int i = 0; i < items.Length; i++)
			{
				try
				{
					results[i] = JsonConfigurationSectionReader.GetSection(settingsElement, items[i].Section);
				}
				catch (Exception exception)
				{
					_logger.LogWarning(exception,
						"Failed to extract workspace/configuration section '{Section}'; returning null for that section.",
						string.IsNullOrWhiteSpace(items[i].Section) ? "<root>" : items[i].Section);

					results[i] = null;
				}
			}

			return results;
		}
		catch (Exception exception)
		{
			_logger.LogWarning(exception,
				"Failed to build the workspace/configuration response; returning null values for {SectionCount} requested section(s).",
				items.Length);

			return new object?[items.Length];
		}
	}

	private CachedSettingsSnapshot GetCachedSettingsSnapshot()
	{
		lock (_settingsSnapshotSyncRoot)
		{
			if (_cachedSettingsSnapshot is { } cachedSettingsSnapshot)
				return cachedSettingsSnapshot;
		}

		return RefreshCachedSettingsSnapshotFromProvider();
	}

	private CachedSettingsSnapshot RefreshCachedSettingsSnapshotFromProvider()
		=> CacheSettingsSnapshot(_settingsProvider());

	private CachedSettingsSnapshot CacheSettingsSnapshot(object settingsPayload)
	{
		CachedSettingsSnapshot settingsSnapshot = CreateCachedSettingsSnapshot(settingsPayload);

		lock (_settingsSnapshotSyncRoot)
		{
			_cachedSettingsSnapshot = settingsSnapshot;
			return settingsSnapshot;
		}
	}

	private static CachedSettingsSnapshot CreateCachedSettingsSnapshot(object settingsPayload)
	{
		JsonElement settingsElement = settingsPayload is JsonElement jsonElement
			? jsonElement.Clone()
			: JsonSerializer.SerializeToElement(settingsPayload, ConfigurationJsonSerializerOptions);

		return new CachedSettingsSnapshot(settingsPayload, settingsElement);
	}

	private void TryRefreshCachedSettingsSnapshotFromNotification(string method, object parameters)
	{
		if (!string.Equals(method, "workspace/didChangeConfiguration", StringComparison.Ordinal))
			return;

		try
		{
			if (parameters is DidChangeConfigurationParams didChangeConfigurationParameters)
			{
				CacheSettingsSnapshot(didChangeConfigurationParameters.Settings);
				return;
			}

			JsonElement payload = JsonSerializer.SerializeToElement(parameters, ConfigurationJsonSerializerOptions);

			if (!payload.TryGetProperty("settings", out JsonElement settingsElement))
				return;

			CacheSettingsSnapshot(settingsElement);
		}
		catch (Exception exception)
		{
			_logger.LogDebug(exception,
				"Failed to refresh the cached workspace settings snapshot from an outgoing didChangeConfiguration notification.");
		}
	}
}
