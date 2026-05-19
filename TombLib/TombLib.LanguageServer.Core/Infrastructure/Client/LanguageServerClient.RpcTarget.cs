using StreamJsonRpc;
using System.Text.Json;

namespace TombLib.LanguageServer.Core;

public sealed partial class LanguageServerClient
{
	/// <summary>
	/// Exposes the host callbacks required by the language server over JSON-RPC.
	/// </summary>
	private sealed class LanguageServerClientRpcTarget
	{
		/// <summary>
		/// References the owning language-server client.
		/// </summary>
		private readonly LanguageServerClient _owner;

		/// <summary>
		/// Captures the transport generation that delivered the callback.
		/// </summary>
		private readonly long _transportGeneration;

		/// <summary>
		/// Initializes a new instance of the <see cref="LanguageServerClientRpcTarget"/> class.
		/// </summary>
		/// <param name="owner">The owning client.</param>
		/// <param name="transportGeneration">The transport generation associated with the callback target.</param>
		public LanguageServerClientRpcTarget(LanguageServerClient owner, long transportGeneration)
		{
			_owner = owner;
			_transportGeneration = transportGeneration;
		}

		/// <summary>
		/// Supplies configuration sections requested by the language server.
		/// </summary>
		/// <param name="parameters">The requested configuration sections.</param>
		/// <returns>The requested configuration objects.</returns>
		[JsonRpcMethod("workspace/configuration", UseSingleObjectParameterDeserialization = true)]
		public object?[] WorkspaceConfiguration(WorkspaceConfigurationParams parameters)
		{
			if (!_owner.IsActiveTransportGeneration(_transportGeneration))
				return new object?[(parameters.Items ?? []).Length];

			return _owner.BuildConfigurationResponse(parameters);
		}

		/// <summary>
		/// Returns the single workspace folder advertised to the language server.
		/// </summary>
		/// <returns>The current workspace folder array.</returns>
		[JsonRpcMethod("workspace/workspaceFolders")]
		public WorkspaceFolder[] WorkspaceFolders()
		{
			if (!_owner.IsActiveTransportGeneration(_transportGeneration))
				return [];

			return
			[
				new WorkspaceFolder(
					LanguageServerPathHelper.CreateFileUri(_owner._workspaceRootDirectoryPath),
					_owner._workspaceFolderName)
			];
		}

		/// <summary>
		/// Acknowledges a semantic-tokens refresh request and notifies the owner.
		/// </summary>
		/// <returns>A completed task that resolves to <see langword="null"/>.</returns>
		[JsonRpcMethod("workspace/semanticTokens/refresh")]
		public Task<object?> RefreshSemanticTokensAsync()
		{
			if (!_owner.CanAcceptServerCallbacksForGeneration(_transportGeneration))
				return Task.FromResult<object?>(null);

			_owner.QueueSemanticTokensRefreshCallback();

			return Task.FromResult<object?>(null);
		}

		/// <summary>
		/// Ignores dynamic capability registration because the client advertises it as unsupported.
		/// </summary>
		/// <param name="parameters">The capability registration payload.</param>
		/// <returns><see langword="null"/>.</returns>
		[JsonRpcMethod("client/registerCapability", UseSingleObjectParameterDeserialization = true)]
		public object? RegisterCapability(CapabilityRegistrationParams parameters)
		{
			if (parameters.Registrations is null || parameters.Registrations.Length == 0)
				return null;

			return IgnoreUnsupportedDynamicCapability(
				"client/registerCapability",
				DescribeCapabilityRegistrations(parameters.Registrations));
		}

		/// <summary>
		/// Ignores dynamic capability unregistration because the client advertises it as unsupported.
		/// </summary>
		/// <param name="parameters">The capability unregistration payload.</param>
		/// <returns><see langword="null"/>.</returns>
		[JsonRpcMethod("client/unregisterCapability", UseSingleObjectParameterDeserialization = true)]
		public object? UnregisterCapability(CapabilityUnregistrationParams parameters)
		{
			if (parameters.Unregistrations is null || parameters.Unregistrations.Length == 0)
				return null;

			return IgnoreUnsupportedDynamicCapability(
				"client/unregisterCapability",
				DescribeCapabilityUnregistrations(parameters.Unregistrations));
		}

		/// <summary>
		/// Logs and ignores a dynamic capability request that this client deliberately does not support.
		/// </summary>
		/// <param name="method">The JSON-RPC method name.</param>
		/// <param name="requestedCapabilities">The formatted requested capability names.</param>
		/// <returns><see langword="null"/>.</returns>
		private object? IgnoreUnsupportedDynamicCapability(string method, string requestedCapabilities)
		{
			if (!_owner.IsActiveTransportGeneration(_transportGeneration))
			{
				Log.Debug(
					"Ignoring unsupported dynamic capability request '{Method}' from stale language server transport generation {Generation}. Requested capabilities: {Capabilities}",
					method,
					_transportGeneration,
					requestedCapabilities);

				return null;
			}

			Log.Warn(
				"Ignoring unsupported dynamic capability request '{Method}' on transport generation {Generation} because the client advertises dynamicRegistration = false. Requested capabilities: {Capabilities}",
				method,
				_transportGeneration,
				requestedCapabilities);

			return null;
		}

		/// <summary>
		/// Formats one capability-registration payload array for diagnostic logging.
		/// </summary>
		/// <param name="registrations">The capability registrations to describe.</param>
		/// <returns>The comma-separated method list.</returns>
		private static string DescribeCapabilityRegistrations(CapabilityRegistrationPayload[] registrations)
		{
			return string.Join(", ",
				Array.ConvertAll(registrations, static registration =>
					string.IsNullOrWhiteSpace(registration.Method) ? "<unknown>" : registration.Method));
		}

		/// <summary>
		/// Formats one capability-unregistration payload array for diagnostic logging.
		/// </summary>
		/// <param name="unregistrations">The capability unregistrations to describe.</param>
		/// <returns>The comma-separated method list.</returns>
		private static string DescribeCapabilityUnregistrations(CapabilityUnregistrationPayload[] unregistrations)
		{
			return string.Join(", ",
				Array.ConvertAll(unregistrations, static unregistration =>
					string.IsNullOrWhiteSpace(unregistration.Method) ? "<unknown>" : unregistration.Method));
		}

		/// <summary>
		/// Acknowledges work-done progress creation requests without creating a client-side progress sink.
		/// </summary>
		/// <param name="parameters">The protocol payload ignored by the host.</param>
		/// <returns><see langword="null"/>.</returns>
		[JsonRpcMethod("window/workDoneProgress/create", UseSingleObjectParameterDeserialization = true)]
		public object? CreateWorkDoneProgress(JsonElement parameters)
		{
			LogIgnoredUnsupportedCallback("window/workDoneProgress/create", "the lean host-specific wrapper does not expose a client-side progress sink");
			return null;
		}

		/// <summary>
		/// Queues diagnostics published by the language server.
		/// </summary>
		/// <param name="parameters">The diagnostics notification payload.</param>
		[JsonRpcMethod("textDocument/publishDiagnostics", UseSingleObjectParameterDeserialization = true)]
		public void PublishDiagnostics(PublishDiagnosticsParams parameters)
		{
			if (!_owner.CanAcceptServerCallbacksForGeneration(_transportGeneration))
				return;

			_owner.RaiseDiagnosticsPublished(_transportGeneration, parameters);
		}

		/// <summary>
		/// Logs a non-modal server message through the host logger.
		/// </summary>
		/// <param name="parameters">The window message payload.</param>
		[JsonRpcMethod("window/logMessage", UseSingleObjectParameterDeserialization = true)]
		public void LogMessage(WindowMessageParams parameters)
			=> LogServerMessage("window/logMessage", parameters);

		/// <summary>
		/// Logs a modal-style server message through the host logger.
		/// </summary>
		/// <param name="parameters">The window message payload.</param>
		[JsonRpcMethod("window/showMessage", UseSingleObjectParameterDeserialization = true)]
		public void ShowMessage(WindowMessageParams parameters)
			=> LogServerMessage("window/showMessage", parameters);

		/// <summary>
		/// Ignores telemetry events that the host does not surface.
		/// </summary>
		/// <param name="parameters">The protocol payload ignored by the host.</param>
		[JsonRpcMethod("telemetry/event", UseSingleObjectParameterDeserialization = true)]
		public void TelemetryEvent(JsonElement parameters)
			=> LogIgnoredUnsupportedCallback("telemetry/event", "the wrapper does not surface server telemetry events");

		/// <summary>
		/// Ignores generic progress notifications that the host does not surface.
		/// </summary>
		/// <param name="parameters">The protocol payload ignored by the host.</param>
		[JsonRpcMethod("$/progress", UseSingleObjectParameterDeserialization = true)]
		public void Progress(JsonElement parameters)
			=> LogIgnoredUnsupportedCallback("$/progress", "the wrapper does not surface generic progress notifications");

		/// <summary>
		/// Ignores nonstandard hello notifications that some language servers emit during startup.
		/// </summary>
		/// <param name="parameters">The protocol payload ignored by the host.</param>
		[JsonRpcMethod("$/hello", UseSingleObjectParameterDeserialization = true)]
		public void Hello(JsonElement parameters)
			=> LogIgnoredUnsupportedCallback("$/hello", "the wrapper does not surface nonstandard startup notifications");

		/// <summary>
		/// Logs one unsupported server callback without surfacing it to the host.
		/// </summary>
		/// <param name="method">The callback method name.</param>
		/// <param name="reason">Why the callback is ignored.</param>
		private void LogIgnoredUnsupportedCallback(string method, string reason)
		{
			if (!_owner.IsActiveTransportGeneration(_transportGeneration))
			{
				Log.Debug(
					"Ignoring unsupported server callback '{Method}' from stale language server transport generation {Generation}. Reason: {Reason}",
					method,
					_transportGeneration,
					reason);

				return;
			}

			Log.Debug(
				"Ignoring unsupported server callback '{Method}' on transport generation {Generation}. Reason: {Reason}",
				method,
				_transportGeneration,
				reason);
		}
	}
}
