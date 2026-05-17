using StreamJsonRpc;
using System.Text.Json;

namespace TombLib.LanguageServer.Core;

public sealed partial class LanguageServerClient
{
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

		JsonElement settingsElement;

		try
		{
			settingsElement = JsonSerializer.SerializeToElement(_settingsProvider(), ConfigurationJsonSerializerOptions);
		}
		catch (Exception exception)
		{
			Log.Warn(exception,
				"Failed to build the workspace/configuration response; returning null values for {SectionCount} requested section(s).",
				items.Length);
			return CreateMissingConfigurationResponse(items.Length);
		}

		var results = new object?[items.Length];

		for (int i = 0; i < items.Length; i++)
		{
			try
			{
				results[i] = GetConfigurationSection(settingsElement, items[i].Section);
			}
			catch (Exception exception)
			{
				Log.Warn(exception,
					"Failed to extract workspace/configuration section '{Section}'; returning null for that section.",
					string.IsNullOrWhiteSpace(items[i].Section) ? "<root>" : items[i].Section);
				results[i] = null;
			}
		}

		return results;
	}

	private static object?[] CreateMissingConfigurationResponse(int sectionCount)
	{
		return new object?[sectionCount];
	}

	/// <summary>
	/// Extracts a nested configuration section from the serialized settings payload.
	/// </summary>
	/// <param name="settingsElement">The serialized root settings element.</param>
	/// <param name="section">The dotted configuration section path.</param>
	/// <returns>The extracted section object, or <see langword="null"/> when the section is missing.</returns>
	private static object? GetConfigurationSection(JsonElement settingsElement, string? section)
	{
		if (string.IsNullOrWhiteSpace(section))
			return settingsElement.Clone();

		JsonElement currentSection = settingsElement;
		string[] parts = section.Split('.');

		foreach (string part in parts)
		{
			if (currentSection.ValueKind is not JsonValueKind.Object
				|| !TryGetProperty(currentSection, part, out JsonElement nextSection))
			{
				return null;
			}

			currentSection = nextSection;
		}

		return currentSection.Clone();
	}

	private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
	{
		if (element.TryGetProperty(propertyName, out value))
			return true;

		foreach (JsonProperty property in element.EnumerateObject())
		{
			if (!string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
				continue;

			value = property.Value;
			return true;
		}

		value = default;
		return false;
	}
}
