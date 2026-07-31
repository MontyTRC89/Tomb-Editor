using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TombLib.LanguageServer.Core;

public sealed partial class LanguageServerClient
{
	private static readonly string[] TextDocumentDynamicRegistrationCapabilityNames =
	[
		"completion",
		"hover",
		"definition",
		"references",
		"rename",
		"formatting",
		"signatureHelp",
		"semanticTokens",
		"publishDiagnostics"
	];

	/// <summary>
	/// Marks the current transport unhealthy so the provider restarts it on the next request.
	/// </summary>
	public void MarkTransportUnhealthy()
	{
		if (_isDisposed)
			return;

		PublishedCapabilitySnapshot snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);
		TryMarkTransportUnhealthy(snapshot.TransportGeneration);
	}

	/// <summary>
	/// Marks one specific transport generation unhealthy only when it is still the active generation.
	/// </summary>
	/// <param name="transportGeneration">The observed transport generation to invalidate.</param>
	/// <returns><see langword="true"/> when the observed generation was still active and was marked unhealthy; otherwise, <see langword="false"/>.</returns>
	public bool TryMarkTransportUnhealthy(long transportGeneration)
	{
		if (_isDisposed)
			return false;

		return TryMarkTransportUnhealthyForGeneration(transportGeneration, out _);
	}

	/// <summary>
	/// Builds the client-capabilities payload while forcing dynamic registration off for capabilities the host handles statically.
	/// </summary>
	/// <returns>The client-capabilities payload to send during initialization.</returns>
	private object BuildClientCapabilitiesPayload()
	{
		object? clientCapabilities = _clientCapabilitiesProvider(_workspaceRootDirectoryPath);

		if (clientCapabilities is null)
			return new { };

		JsonNode? capabilitiesNode = JsonSerializer.SerializeToNode(clientCapabilities);

		if (capabilitiesNode is not JsonObject capabilitiesObject)
			return clientCapabilities;

		EnforceDynamicRegistrationFalse(capabilitiesObject, "workspace", "didChangeWatchedFiles");

		for (int i = 0; i < TextDocumentDynamicRegistrationCapabilityNames.Length; i++)
			EnforceDynamicRegistrationFalse(capabilitiesObject, "textDocument", TextDocumentDynamicRegistrationCapabilityNames[i]);

		return capabilitiesObject;
	}

	/// <summary>
	/// Forces the <c>dynamicRegistration</c> flag to <see langword="false"/> for one nested capability object when it exists.
	/// </summary>
	/// <param name="rootObject">The root capabilities object.</param>
	/// <param name="parentPropertyName">The parent property containing the capability object.</param>
	/// <param name="capabilityPropertyName">The capability property to rewrite.</param>
	private static void EnforceDynamicRegistrationFalse(JsonObject rootObject, string parentPropertyName, string capabilityPropertyName)
	{
		if (!TryGetJsonObjectProperty(rootObject, parentPropertyName, out JsonObject? parentObject))
			return;

		if (!TryGetJsonObjectProperty(parentObject, capabilityPropertyName, out JsonObject? capabilityObject))
			return;

		capabilityObject["dynamicRegistration"] = false;
	}

	/// <summary>
	/// Tries to read one JSON object property while rejecting non-object values.
	/// </summary>
	/// <param name="rootObject">The containing JSON object.</param>
	/// <param name="propertyName">The property name to read.</param>
	/// <param name="propertyObject">Receives the nested object value when present.</param>
	/// <returns><see langword="true"/> when the property exists and is a JSON object.</returns>
	private static bool TryGetJsonObjectProperty(JsonObject rootObject, string propertyName, [NotNullWhen(true)] out JsonObject? propertyObject)
	{
		propertyObject = null;

		if (!rootObject.TryGetPropertyValue(propertyName, out JsonNode? propertyNode))
			return false;

		propertyObject = propertyNode as JsonObject;
		return propertyObject is not null;
	}

	/// <summary>
	/// Captures the server capabilities relevant to the host provider.
	/// </summary>
	/// <param name="initializeResponse">The initialize response received from the server.</param>
	/// <remarks>This method is used by <c>LanguageServerClientTests</c> via reflection. Do not remove without updating the tests.</remarks>
	private void CaptureServerCapabilities(InitializeResponse initializeResponse)
		=> CaptureServerCapabilitiesForGeneration(TransportGeneration, initializeResponse);

	/// <summary>
	/// Captures the server capabilities for one transport generation when it is still current.
	/// </summary>
	/// <param name="transportGeneration">The transport generation that owns the initialize response.</param>
	/// <param name="initializeResponse">The initialize response received from the server.</param>
	private void CaptureServerCapabilitiesForGeneration(long transportGeneration, InitializeResponse initializeResponse)
	{
		ServerCapabilities capabilities = initializeResponse.Capabilities
			?? throw new NotSupportedException(
				"The language server did not advertise the full or incremental text synchronization required by the host provider.");

		TextDocumentSyncCapability textDocumentSync = capabilities.TextDocumentSync
			?? throw new NotSupportedException(
				"The language server did not advertise the full or incremental text synchronization required by the host provider.");

		if (textDocumentSync.Kind == TextDocumentSyncKind.None)
		{
			throw new NotSupportedException(
				"The language server did not advertise the full or incremental text synchronization required by the host provider.");
		}

		TextDocumentSyncKind textDocumentSyncKind = textDocumentSync.Kind;

		bool supportsCompletionResolve = capabilities.CompletionProvider?.ResolveProvider == true;
		bool supportsReferences = capabilities.ReferencesProvider?.IsSupported == true;
		bool supportsRename = capabilities.RenameProvider?.IsSupported == true;
		bool supportsFormatting = capabilities.DocumentFormattingProvider?.IsSupported == true;

		bool supportsSemanticTokensFull = false;
		bool supportsSemanticTokensDelta = false;

		IReadOnlyList<string> semanticTokenTypes = EmptyCapabilityList;
		IReadOnlyList<string> semanticTokenModifiers = EmptyCapabilityList;

		if (capabilities.SemanticTokensProvider is { } semanticTokensProvider)
		{
			supportsSemanticTokensFull = semanticTokensProvider.Full?.IsSupported == true;
			supportsSemanticTokensDelta = semanticTokensProvider.Full?.SupportsDelta == true;

			if (semanticTokensProvider.Legend is { } legend)
			{
				semanticTokenTypes = Array.AsReadOnly(legend.TokenTypes ?? []);
				semanticTokenModifiers = Array.AsReadOnly(legend.TokenModifiers ?? []);
			}
		}

		PublishServerCapabilitiesForGeneration(
			transportGeneration,
			textDocumentSyncKind,
			semanticTokenTypes,
			semanticTokenModifiers,
			supportsCompletionResolve,
			supportsReferences,
			supportsRename,
			supportsFormatting,
			supportsSemanticTokensFull,
			supportsSemanticTokensDelta);
	}

	/// <summary>
	/// Creates the published capability snapshot used while one transport generation is active but still completing startup.
	/// </summary>
	/// <param name="transportGeneration">The active transport generation.</param>
	/// <returns>The startup snapshot for the active generation.</returns>
	private static PublishedCapabilitySnapshot CreateActiveSessionCapabilitySnapshot(long transportGeneration) => new(
		transportGeneration,
		IsReady: false,
		AcceptsServerCallbacks: transportGeneration != 0,
		TextDocumentSyncKind: TextDocumentSyncKind.None,
		SemanticTokenTypes: EmptyCapabilityList,
		SemanticTokenModifiers: EmptyCapabilityList,
		SupportsCompletionResolve: false,
		SupportsReferences: null,
		SupportsRename: null,
		SupportsFormatting: null,
		SupportsSemanticTokensFull: false,
		SupportsSemanticTokensDelta: false
	);

	/// <summary>
	/// Creates the default published capability snapshot for one transport generation.
	/// </summary>
	/// <param name="transportGeneration">The transport generation to publish.</param>
	/// <param name="isReady">Whether the transport completed initialization.</param>
	/// <returns>The default capability snapshot.</returns>
	private static PublishedCapabilitySnapshot CreateDefaultCapabilitySnapshot(long transportGeneration = 0, bool isReady = false) => new(
		transportGeneration,
		isReady,
		AcceptsServerCallbacks: transportGeneration != 0 && isReady,
		TextDocumentSyncKind: TextDocumentSyncKind.None,
		SemanticTokenTypes: EmptyCapabilityList,
		SemanticTokenModifiers: EmptyCapabilityList,
		SupportsCompletionResolve: false,
		SupportsReferences: null,
		SupportsRename: null,
		SupportsFormatting: null,
		SupportsSemanticTokensFull: false,
		SupportsSemanticTokensDelta: false
	);

	/// <summary>
	/// Publishes one immutable capability snapshot to concurrent readers with a single atomic swap.
	/// </summary>
	/// <param name="snapshot">The snapshot to publish.</param>
	private void PublishCapabilitySnapshot(PublishedCapabilitySnapshot snapshot)
		=> Volatile.Write(ref _publishedCapabilitySnapshot, snapshot);

	/// <summary>
	/// Publishes negotiated server capabilities when the target generation is still active.
	/// </summary>
	private void PublishServerCapabilitiesForGeneration(
		long transportGeneration,
		TextDocumentSyncKind textDocumentSyncKind,
		IReadOnlyList<string> semanticTokenTypes,
		IReadOnlyList<string> semanticTokenModifiers,
		bool supportsCompletionResolve,
		bool? supportsReferences,
		bool? supportsRename,
		bool? supportsFormatting,
		bool supportsSemanticTokensFull,
		bool supportsSemanticTokensDelta)
	{
		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			PublishedCapabilitySnapshot currentSnapshot = Volatile.Read(ref _publishedCapabilitySnapshot);

			if (currentSnapshot.TransportGeneration != transportGeneration)
				return;

			PublishCapabilitySnapshot(new PublishedCapabilitySnapshot(
				currentSnapshot.TransportGeneration,
				currentSnapshot.IsReady,
				currentSnapshot.AcceptsServerCallbacks,
				textDocumentSyncKind,
				semanticTokenTypes,
				semanticTokenModifiers,
				supportsCompletionResolve,
				supportsReferences,
				supportsRename,
				supportsFormatting,
				supportsSemanticTokensFull,
				supportsSemanticTokensDelta));
		}
	}

	/// <summary>
	/// Marks one transport generation unhealthy only when it still owns the published snapshot.
	/// </summary>
	/// <param name="transportGeneration">The generation to mark unhealthy.</param>
	private bool TryMarkTransportUnhealthyForGeneration(long transportGeneration, out PublishedCapabilitySnapshot snapshot)
	{
		if (!TryGetPublishedCapabilitySnapshotForGeneration(transportGeneration, out snapshot))
			return false;

		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			PublishedCapabilitySnapshot currentSnapshot = Volatile.Read(ref _publishedCapabilitySnapshot);

			if (currentSnapshot.TransportGeneration != transportGeneration)
				return false;

			PublishCapabilitySnapshot(new PublishedCapabilitySnapshot(
				transportGeneration,
				IsReady: false,
				AcceptsServerCallbacks: false,
				TextDocumentSyncKind: TextDocumentSyncKind.None,
				SemanticTokenTypes: EmptyCapabilityList,
				SemanticTokenModifiers: EmptyCapabilityList,
				SupportsCompletionResolve: false,
				SupportsReferences: null,
				SupportsRename: null,
				SupportsFormatting: null,
				SupportsSemanticTokensFull: false,
				SupportsSemanticTokensDelta: false));
		}

		if (snapshot.IsReady && transportGeneration != 0)
			_logger.LogWarning("Marked language server transport generation {Generation} unhealthy; the host will restart it before the next public request.", transportGeneration);

		return true;
	}

	/// <summary>
	/// Updates only the readiness flag while keeping the rest of the published capability snapshot aligned.
	/// </summary>
	/// <param name="isReady">Whether the active transport is ready.</param>
	/// <remarks>This method is used by <c>LanguageServerClientTests</c> via reflection. Do not remove without updating the tests.</remarks>
	private void SetCapabilityReadiness(bool isReady)
	{
		PublishedCapabilitySnapshot snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);
		SetCapabilityReadinessForGeneration(snapshot.TransportGeneration, isReady);
	}

	/// <summary>
	/// Updates the readiness flag for one transport generation when it still owns the published snapshot.
	/// </summary>
	/// <param name="transportGeneration">The generation whose readiness should change.</param>
	/// <param name="isReady">Whether the active transport is ready.</param>
	private void SetCapabilityReadinessForGeneration(long transportGeneration, bool isReady)
	{
		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			PublishedCapabilitySnapshot snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);

			if (snapshot.TransportGeneration != transportGeneration)
				return;

			if (!isReady)
			{
				PublishCapabilitySnapshot(CreateDefaultCapabilitySnapshot(snapshot.TransportGeneration));
				return;
			}

			PublishCapabilitySnapshot(snapshot with { IsReady = true, AcceptsServerCallbacks = true });
		}
	}

	/// <summary>
	/// Gets the published capability snapshot when it still belongs to the requested transport generation.
	/// </summary>
	/// <param name="transportGeneration">The generation that should own the snapshot.</param>
	/// <param name="snapshot">Receives the snapshot when the generation matches.</param>
	/// <returns><see langword="true"/> when the generation still owns the snapshot; otherwise, <see langword="false"/>.</returns>
	private bool TryGetPublishedCapabilitySnapshotForGeneration(long transportGeneration, out PublishedCapabilitySnapshot snapshot)
	{
		snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);
		return snapshot.TransportGeneration == transportGeneration;
	}
}
