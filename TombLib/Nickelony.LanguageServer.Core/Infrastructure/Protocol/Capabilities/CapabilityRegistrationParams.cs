using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a client capability registration request.
/// </summary>
/// <param name="Registrations">The requested capability registrations.</param>
public readonly record struct CapabilityRegistrationParams(
	[property: JsonPropertyName("registrations")] CapabilityRegistrationPayload[]? Registrations);

/// <summary>
/// Represents a single dynamic capability registration entry.
/// </summary>
/// <param name="Id">The server-defined registration identifier.</param>
/// <param name="Method">The capability method being registered.</param>
public readonly record struct CapabilityRegistrationPayload(
	[property: JsonPropertyName("id")] string? Id,
	[property: JsonPropertyName("method")] string? Method);
