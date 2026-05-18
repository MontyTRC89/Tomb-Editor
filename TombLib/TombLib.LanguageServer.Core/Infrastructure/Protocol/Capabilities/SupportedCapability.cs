using System.Text.Json.Serialization;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Represents a capability that may be advertised as either a boolean or an object.
/// </summary>
/// <param name="IsSupported">Whether the capability is supported.</param>
[JsonConverter(typeof(SupportedCapabilityJsonConverter))]
public readonly record struct SupportedCapability(bool IsSupported);
