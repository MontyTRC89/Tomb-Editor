using StreamJsonRpc;

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
				return CreateUnavailableConfigurationResponse(parameters);

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

		private static object?[] CreateUnavailableConfigurationResponse(WorkspaceConfigurationParams parameters)
		{
			WorkspaceConfigurationItem[] items = parameters.Items ?? [];
			return new object?[items.Length];
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

		private static string DescribeCapabilityRegistrations(CapabilityRegistrationPayload[] registrations)
			=> string.Join(", ",
				Array.ConvertAll(registrations, static registration =>
					string.IsNullOrWhiteSpace(registration.Method) ? "<unknown>" : registration.Method));

		private static string DescribeCapabilityUnregistrations(CapabilityUnregistrationPayload[] unregistrations)
			=> string.Join(", ",
				Array.ConvertAll(unregistrations, static unregistration =>
					string.IsNullOrWhiteSpace(unregistration.Method) ? "<unknown>" : unregistration.Method));

		/// <summary>
		/// Acknowledges work-done progress creation requests without creating a client-side progress sink.
		/// </summary>
		/// <param name="parameters">The empty protocol payload.</param>
		/// <returns><see langword="null"/>.</returns>
		[JsonRpcMethod("window/workDoneProgress/create", UseSingleObjectParameterDeserialization = true)]
		public object? CreateWorkDoneProgress(EmptyParams parameters)
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
		/// <param name="parameters">The empty protocol payload.</param>
		[JsonRpcMethod("telemetry/event", UseSingleObjectParameterDeserialization = true)]
		public void TelemetryEvent(EmptyParams parameters)
			=> LogIgnoredUnsupportedCallback("telemetry/event", "the lean host-specific wrapper does not surface server telemetry events");

		/// <summary>
		/// Ignores generic progress notifications that the host does not surface.
		/// </summary>
		/// <param name="parameters">The empty protocol payload.</param>
		[JsonRpcMethod("$/progress", UseSingleObjectParameterDeserialization = true)]
		public void Progress(EmptyParams parameters)
			=> LogIgnoredUnsupportedCallback("$/progress", "the lean host-specific wrapper does not surface generic progress notifications");

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
