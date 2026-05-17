using System.Text.Json;
using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Represents an empty protocol payload.
/// </summary>
public readonly record struct EmptyParams();

/// <summary>
/// Represents a workspace configuration request payload.
/// </summary>
/// <param name="Items">The requested configuration sections.</param>
public readonly record struct WorkspaceConfigurationParams(
	[property: JsonPropertyName("items")] WorkspaceConfigurationItem[]? Items);

/// <summary>
/// Identifies a single configuration section requested from the host.
/// </summary>
/// <param name="Section">The dotted configuration section name.</param>
public readonly record struct WorkspaceConfigurationItem(
	[property: JsonPropertyName("section")] string? Section);

/// <summary>
/// Represents a workspace folder advertised to the language server.
/// </summary>
/// <param name="Uri">The workspace folder URI.</param>
/// <param name="Name">The display name of the workspace folder.</param>
public readonly record struct WorkspaceFolder(
	[property: JsonPropertyName("uri")] string Uri,
	[property: JsonPropertyName("name")] string Name);

/// <summary>
/// Represents a message notification sent from the language server.
/// </summary>
/// <param name="Type">The protocol message severity.</param>
/// <param name="Message">The message text.</param>
public readonly record struct WindowMessageParams(
	[property: JsonPropertyName("type")] int? Type,
	[property: JsonPropertyName("message")] string? Message);

/// <summary>
/// Represents a typed diagnostics notification raised by the language server for a tracked document.
/// </summary>
/// <param name="Uri">The document URI receiving diagnostics.</param>
/// <param name="Version">The document version associated with the diagnostics.</param>
/// <param name="Diagnostics">The diagnostic entries for the document.</param>
public readonly record struct PublishDiagnosticsParams(
	[property: JsonPropertyName("uri")] string? Uri,
	[property: JsonPropertyName("version")] int? Version,
	[property: JsonPropertyName("diagnostics")] DiagnosticPayload[]? Diagnostics);

/// <summary>
/// Represents a single diagnostic entry from a publish-diagnostics notification.
/// </summary>
/// <param name="Range">The affected document range.</param>
/// <param name="Severity">The protocol severity value.</param>
/// <param name="Message">The user-facing diagnostic message.</param>
/// <param name="Source">The diagnostic source identifier.</param>
/// <param name="Code">The optional diagnostic code value.</param>
public readonly record struct DiagnosticPayload(
	[property: JsonPropertyName("range")] ProtocolRangePayload? Range,
	[property: JsonPropertyName("severity")] int? Severity,
	[property: JsonPropertyName("message")] string? Message,
	[property: JsonPropertyName("source")] string? Source,
	[property: JsonPropertyName("code")] JsonElement? Code);

/// <summary>
/// Represents a nullable protocol range payload.
/// </summary>
/// <param name="Start">The nullable start position.</param>
/// <param name="End">The nullable end position.</param>
public readonly record struct ProtocolRangePayload(
	[property: JsonPropertyName("start")] ProtocolNullablePosition? Start,
	[property: JsonPropertyName("end")] ProtocolNullablePosition? End);

/// <summary>
/// Represents a nullable zero-based protocol position.
/// </summary>
/// <param name="Line">The zero-based line index.</param>
/// <param name="Character">The zero-based character index.</param>
public readonly record struct ProtocolNullablePosition(
	[property: JsonPropertyName("line")] int? Line,
	[property: JsonPropertyName("character")] int? Character);
