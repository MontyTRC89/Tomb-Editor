#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses signature help metadata from a LuaLS signature-help response.
	/// </summary>
	public static LuaSignatureInfo? ParseSignatureHelp(JsonElement response)
	{
		if (response.ValueKind != JsonValueKind.Object
			|| !response.TryGetProperty("signatures", out JsonElement signaturesElement)
			|| signaturesElement.ValueKind != JsonValueKind.Array)
		{
			return null;
		}

		int signatureCount = signaturesElement.GetArrayLength();

		if (signatureCount == 0)
			return null;

		int activeSignature = response.TryGetProperty("activeSignature", out JsonElement activeSignatureElement)
			&& activeSignatureElement.TryGetInt32(out int parsedActiveSignature)
				? Math.Clamp(parsedActiveSignature, 0, signatureCount - 1)
				: 0;

		JsonElement signatureElement = signaturesElement[activeSignature];

		if (signatureElement.ValueKind != JsonValueKind.Object
			|| !signatureElement.TryGetProperty("label", out JsonElement labelElement))
		{
			return null;
		}

		string? label = labelElement.GetString();

		if (string.IsNullOrWhiteSpace(label))
			return null;

		string? documentation = signatureElement.TryGetProperty("documentation", out JsonElement documentationElement)
			? ExtractMarkupText(documentationElement)
			: null;

		int activeParameter = ResolveActiveParameter(response, signatureElement);

		var parameters = new List<LuaParameterInfo>();

		if (signatureElement.TryGetProperty("parameters", out JsonElement parametersElement)
			&& parametersElement.ValueKind == JsonValueKind.Array)
		{
			foreach (JsonElement paramElement in parametersElement.EnumerateArray())
			{
				string? parameterLabel = paramElement.TryGetProperty("label", out JsonElement paramLabelElement)
					? paramLabelElement.ValueKind == JsonValueKind.String
						? paramLabelElement.GetString()
						: TryExtractParameterLabel(label, paramLabelElement, out string? extractedLabel)
							? extractedLabel
							: null
					: null;

				string? parameterDocumentation = paramElement.TryGetProperty("documentation", out JsonElement parameterDocumentationElement)
					? ExtractMarkupText(parameterDocumentationElement)
					: null;

				parameters.Add(new LuaParameterInfo(parameterLabel ?? string.Empty, parameterDocumentation));
			}
		}

		return new LuaSignatureInfo(label, documentation, parameters, activeParameter);
	}

	private static int ResolveActiveParameter(JsonElement response, JsonElement signatureElement)
	{
		if (response.TryGetProperty("activeParameter", out JsonElement responseActiveParameterElement)
			&& responseActiveParameterElement.TryGetInt32(out int responseActiveParameter))
		{
			return Math.Max(0, responseActiveParameter);
		}

		if (signatureElement.TryGetProperty("activeParameter", out JsonElement signatureActiveParameterElement)
			&& signatureActiveParameterElement.TryGetInt32(out int signatureActiveParameter))
		{
			return Math.Max(0, signatureActiveParameter);
		}

		return 0;
	}

	private static bool TryExtractParameterLabel(string signatureLabel, JsonElement parameterLabelElement,
		[NotNullWhen(true)] out string? parameterLabel)
	{
		parameterLabel = null;

		if (string.IsNullOrEmpty(signatureLabel)
			|| parameterLabelElement.ValueKind != JsonValueKind.Array)
		{
			return false;
		}

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
