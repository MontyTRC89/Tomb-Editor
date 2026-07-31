using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a typed diagnostics notification raised by the language server for a tracked document.
/// </summary>
/// <param name="Uri">The document URI receiving diagnostics.</param>
/// <param name="Version">The document version associated with the diagnostics.</param>
/// <param name="Diagnostics">The diagnostic entries for the document.</param>
public readonly record struct PublishDiagnosticsParams(
	[property: JsonPropertyName("uri")] string? Uri,
	[property: JsonPropertyName("version")] int? Version,
	[property: JsonPropertyName("diagnostics")] DiagnosticPayload[]? Diagnostics)
{
	/// <summary>
	/// Creates a detached diagnostics snapshot so queued subscribers do not share a mutable array instance.
	/// </summary>
	/// <returns>The cloned diagnostics payload.</returns>
	public PublishDiagnosticsParams CreateSnapshot()
		=> this with { Diagnostics = Diagnostics is null ? null : [.. Diagnostics] };
}

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
