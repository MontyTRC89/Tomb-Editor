using System;
using System.IO;
using System.Text.Json;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense
{
	internal static class LuaLanguageServerPathHelper
	{
		public static string CreateFileUri(string filePath)
			=> new Uri(NormalizeLocalPath(filePath)).AbsoluteUri;

		public static string NormalizeLocalPath(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("File path must not be empty.", nameof(filePath));

			string sanitizedFilePath = filePath.Replace('/', Path.DirectorySeparatorChar);
			return Path.GetFullPath(sanitizedFilePath);
		}

		public static string NormalizeLocalPath(Uri uri)
		{
			if (uri is null)
				throw new ArgumentNullException(nameof(uri));

			string localPath = uri.LocalPath;

			if (Path.DirectorySeparatorChar == '\\'
				&& localPath.Length >= 3
				&& localPath[0] == '/'
				&& char.IsLetter(localPath[1])
				&& localPath[2] == ':')
			{
				localPath = localPath[1..];
			}

			localPath = localPath.Replace('/', Path.DirectorySeparatorChar);
			return Path.GetFullPath(localPath);
		}

		public static bool TryNormalizeLocalPath(string filePath, out string normalizedFilePath)
		{
			normalizedFilePath = null;

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

		public static bool TryGetFilePath(JsonElement parameters, out string filePath)
		{
			filePath = null;

			if (parameters.ValueKind != JsonValueKind.Object
				|| !parameters.TryGetProperty("uri", out JsonElement uriElement)
				|| string.IsNullOrWhiteSpace(uriElement.GetString())
				|| !Uri.TryCreate(uriElement.GetString(), UriKind.Absolute, out Uri uri)
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
}