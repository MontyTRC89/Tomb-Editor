using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.LanguageServer.Lua;

internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses signature help metadata from a LuaLS signature-help response.
	/// </summary>
	internal static LuaSignatureInfo? ParseSignatureHelp(SignatureHelpResponse? response)
	{
		if (response?.Signatures is not { Length: > 0 } signatures)
			return null;

		int activeSignature = response.ActiveSignature is int parsedActiveSignature
			? Math.Clamp(parsedActiveSignature, 0, signatures.Length - 1)
			: 0;

		SignatureHelpSignaturePayload signatureElement = signatures[activeSignature];

		if (string.IsNullOrWhiteSpace(signatureElement.Label))
			return null;

		string label = signatureElement.Label;

		string? documentation = signatureElement.Documentation.ValueKind != JsonValueKind.Undefined
			? LuaMarkupTextHelper.ExtractMarkupText(signatureElement.Documentation)
			: null;

		int activeParameter = ResolveActiveParameter(response, signatureElement);

		var parameters = new List<LuaParameterInfo>();

		if (signatureElement.Parameters is { Length: > 0 } parametersElement)
		{
			for (int i = 0; i < parametersElement.Length; i++)
			{
				SignatureHelpParameterPayload paramElement = parametersElement[i];

				string? parameterLabel = paramElement.Label.ValueKind == JsonValueKind.String
					? paramElement.Label.GetString()
					: TryExtractParameterLabel(label, paramElement.Label, out string? extractedLabel)
						? extractedLabel
						: null;

				string? parameterDocumentation = paramElement.Documentation.ValueKind != JsonValueKind.Undefined
					? LuaMarkupTextHelper.ExtractMarkupText(paramElement.Documentation)
					: null;

				parameters.Add(new LuaParameterInfo(parameterLabel ?? string.Empty, parameterDocumentation));
			}
		}

		return new LuaSignatureInfo(label, documentation, parameters, activeParameter);
	}

	private static int ResolveActiveParameter(SignatureHelpResponse response, SignatureHelpSignaturePayload signatureElement)
	{
		if (response.ActiveParameter is int responseActiveParameter)
			return Math.Max(0, responseActiveParameter);

		if (signatureElement.ActiveParameter is int signatureActiveParameter)
			return Math.Max(0, signatureActiveParameter);

		return 0;
	}

	private static bool TryExtractParameterLabel(string signatureLabel, JsonElement parameterLabelElement,
		[NotNullWhen(true)] out string? parameterLabel)
	{
		parameterLabel = null;

		if (string.IsNullOrEmpty(signatureLabel) || parameterLabelElement.ValueKind != JsonValueKind.Array)
			return false;

		JsonElement.ArrayEnumerator labelParts = parameterLabelElement.EnumerateArray();

		if (!labelParts.MoveNext() || !labelParts.Current.TryGetInt32(out int startIndex))
			return false;

		if (!labelParts.MoveNext() || !labelParts.Current.TryGetInt32(out int endIndex))
			return false;

		startIndex = Math.Max(0, Math.Min(startIndex, signatureLabel.Length));
		endIndex = Math.Max(startIndex, Math.Min(endIndex, signatureLabel.Length));

		if (endIndex <= startIndex)
			return false;

		parameterLabel = signatureLabel[startIndex..endIndex];
		return true;
	}
}
