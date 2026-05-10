using StreamJsonRpc;
using System.Text.Json;

namespace TombLib.Scripting.Lua.LanguageServer;

public sealed partial class LuaLanguageServerClient
{
	private Task SendNotificationCoreAsync(LuaLanguageServerTransportSession session, string method, object parameters, CancellationToken cancellationToken, bool allowDisposed)
	{
		ThrowIfDisposed(allowDisposed);

		if (cancellationToken.IsCancellationRequested)
			return Task.FromCanceled(cancellationToken);

		JsonRpc jsonRpc = session.JsonRpc
			?? throw new IOException("The Lua language server JSON-RPC transport is not available.");

		return jsonRpc.NotifyWithParameterObjectAsync(method, parameters);
	}

	private Task<TResult> SendRequestCoreAsync<TResult>(LuaLanguageServerTransportSession session, string method, object parameters, CancellationToken cancellationToken, bool allowDisposed)
	{
		ThrowIfDisposed(allowDisposed);

		JsonRpc jsonRpc = session.JsonRpc
			?? throw new IOException("The Lua language server JSON-RPC transport is not available.");

		return jsonRpc.InvokeWithParameterObjectAsync<TResult>(method, parameters, cancellationToken);
	}

	private object[] BuildConfigurationResponse(LuaWorkspaceConfigurationParams parameters)
	{
		LuaWorkspaceConfigurationItem[] items = parameters.Items ?? [];

		if (items.Length == 0)
			return [];

		JsonElement settingsElement = JsonSerializer.SerializeToElement(_settingsProvider());
		JsonElement luaElement = settingsElement.GetProperty("Lua");

		var results = new List<object>();

		foreach (LuaWorkspaceConfigurationItem item in items)
			results.Add(GetConfigurationSection(settingsElement, luaElement, item.Section));

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

	private sealed class LuaLanguageServerClientRpcTarget
	{
		private readonly LuaLanguageServerClient _owner;
		private readonly long _transportGeneration;

		public LuaLanguageServerClientRpcTarget(LuaLanguageServerClient owner, long transportGeneration)
		{
			_owner = owner;
			_transportGeneration = transportGeneration;
		}

		[JsonRpcMethod("workspace/configuration", UseSingleObjectParameterDeserialization = true)]
		public object[] WorkspaceConfiguration(LuaWorkspaceConfigurationParams parameters)
			=> _owner.BuildConfigurationResponse(parameters);

		[JsonRpcMethod("workspace/workspaceFolders")]
		public LuaWorkspaceFolder[] WorkspaceFolders() =>
		[
			new LuaWorkspaceFolder(
				LuaLanguageServerPathHelper.CreateFileUri(_owner._workspaceRootDirectoryPath),
				Path.GetFileName(_owner._workspaceRootDirectoryPath))
		];

		[JsonRpcMethod("workspace/semanticTokens/refresh")]
		public Task<object?> RefreshSemanticTokensAsync()
		{
			try
			{
				_owner.SemanticTokensRefreshRequested?.Invoke();
			}
			catch (Exception exception)
			{
				Log.Warn(exception, "Lua semantic-tokens refresh request handler threw; acknowledging the request anyway.");
			}

			return Task.FromResult<object?>(null);
		}

		[JsonRpcMethod("client/registerCapability", UseSingleObjectParameterDeserialization = true)]
		public object? RegisterCapability(LuaEmptyParams parameters)
			=> null;

		[JsonRpcMethod("client/unregisterCapability", UseSingleObjectParameterDeserialization = true)]
		public object? UnregisterCapability(LuaEmptyParams parameters)
			=> null;

		[JsonRpcMethod("window/workDoneProgress/create", UseSingleObjectParameterDeserialization = true)]
		public object? CreateWorkDoneProgress(LuaEmptyParams parameters)
			=> null;

		[JsonRpcMethod("textDocument/publishDiagnostics", UseSingleObjectParameterDeserialization = true)]
		public void PublishDiagnostics(LuaPublishDiagnosticsParams parameters)
			=> _owner.RaiseDiagnosticsPublished(_transportGeneration, parameters);

		[JsonRpcMethod("window/logMessage", UseSingleObjectParameterDeserialization = true)]
		public void LogMessage(LuaWindowMessageParams parameters)
			=> LogServerMessage("window/logMessage", parameters);

		[JsonRpcMethod("window/showMessage", UseSingleObjectParameterDeserialization = true)]
		public void ShowMessage(LuaWindowMessageParams parameters)
			=> LogServerMessage("window/showMessage", parameters);

		[JsonRpcMethod("telemetry/event", UseSingleObjectParameterDeserialization = true)]
		public void TelemetryEvent(LuaEmptyParams parameters)
		{ }

		[JsonRpcMethod("$/progress", UseSingleObjectParameterDeserialization = true)]
		public void Progress(LuaEmptyParams parameters)
		{ }
	}
}
