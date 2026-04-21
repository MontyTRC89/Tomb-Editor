#nullable enable

using System;
using System.IO;
using System.Text.Json;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal static class LuaLanguageServerPathHelper
{
	/// <summary>
	/// Converts a local file path into a normalized file URI for LuaLS requests.
	/// </summary>
	/// <param name="filePath">The local file path to convert.</param>
	/// <returns>The absolute file URI.</returns>
	public static string CreateFileUri(string filePath)
		=> new Uri(NormalizeLocalPath(filePath)).AbsoluteUri;

	/// <summary>
	/// Normalizes a local path into the absolute form used by the language server.
	/// </summary>
	/// <param name="filePath">The path to normalize.</param>
	/// <returns>The normalized absolute path.</returns>
	public static string NormalizeLocalPath(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			throw new ArgumentException("File path must not be empty.", nameof(filePath));

		string sanitizedFilePath = filePath.Replace('/', Path.DirectorySeparatorChar);
		return Path.GetFullPath(sanitizedFilePath);
	}

	public static string NormalizeLocalPath(Uri uri)
	{
		string localPath = uri.LocalPath;

		// On Windows, Uri.LocalPath may produce "/C:/..." which needs the leading slash trimmed.
		if (Path.DirectorySeparatorChar == '\\'
			&& localPath.Length >= 3
			&& localPath[0] == '/'
			&& char.IsLetter(localPath[1])
			&& localPath[2] == ':')
		{
			localPath = localPath[1..];
		}

		return NormalizeLocalPath(localPath);
	}

	/// <summary>
	/// Attempts to normalize a local path without throwing for invalid input.
	/// </summary>
	/// <param name="filePath">The path to normalize.</param>
	/// <param name="normalizedFilePath">The normalized absolute path when successful.</param>
	/// <returns><see langword="true"/> when normalization succeeded; otherwise, <see langword="false"/>.</returns>
	public static bool TryNormalizeLocalPath(string filePath, out string normalizedFilePath)
	{
		normalizedFilePath = string.Empty;

		if (string.IsNullOrWhiteSpace(filePath))
			return false;

		try
		{
			normalizedFilePath = NormalizeLocalPath(filePath);
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Attempts to extract and normalize a local file path from an LSP payload.
	/// </summary>
	/// <param name="parameters">The JSON payload containing the document URI.</param>
	/// <param name="filePath">The normalized local file path when successful.</param>
	/// <returns><see langword="true"/> when a local file path was resolved; otherwise, <see langword="false"/>.</returns>
	public static bool TryGetFilePath(JsonElement parameters, out string filePath)
	{
		filePath = string.Empty;

		if (parameters.ValueKind != JsonValueKind.Object
			|| !parameters.TryGetProperty("uri", out JsonElement uriElement))
		{
			return false;
		}

		string? uriText = uriElement.GetString();

		if (string.IsNullOrWhiteSpace(uriText)
			|| !Uri.TryCreate(uriText, UriKind.Absolute, out Uri? uri)
			|| uri is null
			|| !uri.IsFile)
		{
			return false;
		}

		try
		{
			filePath = NormalizeLocalPath(uri);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
