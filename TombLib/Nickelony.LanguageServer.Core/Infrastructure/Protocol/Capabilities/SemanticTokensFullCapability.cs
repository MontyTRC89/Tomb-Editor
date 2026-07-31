using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents the semantic tokens full capability advertised by the server.
/// </summary>
/// <param name="IsSupported">Whether full semantic token requests are supported.</param>
/// <param name="SupportsDelta">Whether delta refresh is supported.</param>
[JsonConverter(typeof(SemanticTokensFullCapabilityJsonConverter))]
public readonly record struct SemanticTokensFullCapability(bool IsSupported, bool SupportsDelta);
