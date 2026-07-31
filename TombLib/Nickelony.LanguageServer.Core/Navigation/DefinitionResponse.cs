using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents one or more usable definition targets returned by a language server.
/// Compatibility accessors expose the first target so existing host parsers can stay unchanged.
/// </summary>
[JsonConverter(typeof(DefinitionResponseJsonConverter))]
public sealed record DefinitionResponse
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DefinitionResponse"/> class.
	/// </summary>
	/// <param name="targets">The usable definition targets returned by the server.</param>
	public DefinitionResponse(IReadOnlyList<DefinitionTargetResponse>? targets)
		=> Targets = targets ?? [];

	/// <summary>
	/// Gets the usable definition targets returned by the server, in response order.
	/// </summary>
	public IReadOnlyList<DefinitionTargetResponse> Targets { get; }

	/// <summary>
	/// Gets the first usable definition target, when any were returned.
	/// </summary>
	public DefinitionTargetResponse? FirstTarget => Targets.Count > 0 ? Targets[0] : null;

	/// <summary>
	/// Gets the first usable target document URI for compatibility with existing callers.
	/// </summary>
	public string? Uri => FirstTarget?.Uri;

	/// <summary>
	/// Gets the one-based line number of the first usable target for compatibility with existing callers.
	/// </summary>
	public int LineNumber => FirstTarget?.LineNumber ?? 0;

	/// <summary>
	/// Gets the one-based column number of the first usable target for compatibility with existing callers.
	/// </summary>
	public int ColumnNumber => FirstTarget?.ColumnNumber ?? 0;
}
