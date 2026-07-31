namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Normalizes local paths and file URIs so the language server and host editor use a consistent document identity.
/// </summary>
public static class LanguageServerPathHelper
{
	private const string UseCaseSensitiveLocalPathsSwitch = "Nickelony.LanguageServer.Core.UseCaseSensitiveLocalPaths";
	private const string UseCaseInsensitiveLocalPathsSwitch = "Nickelony.LanguageServer.Core.UseCaseInsensitiveLocalPaths";

	/// <summary>
	/// Gets a value indicating whether normalized local-path identity should treat character casing as significant.
	/// Windows and standard macOS setups default to case-insensitive matching, while Linux defaults to case-sensitive matching.
	/// Hosts may override the default through the documented AppContext switches.
	/// </summary>
	public static bool UsesCaseSensitiveLocalPaths { get; } = DeterminePathCaseSensitivity();

	/// <summary>
	/// Gets the comparer used for normalized local-path dictionary keys.
	/// </summary>
	public static StringComparer LocalPathComparer { get; } = UsesCaseSensitiveLocalPaths
		? StringComparer.Ordinal
		: StringComparer.OrdinalIgnoreCase;

	/// <summary>
	/// Gets the string comparison used for normalized local-path equality checks.
	/// </summary>
	public static StringComparison LocalPathComparison { get; } = UsesCaseSensitiveLocalPaths
		? StringComparison.Ordinal
		: StringComparison.OrdinalIgnoreCase;

	/// <summary>
	/// Converts a local file path into a normalized file URI for language-server requests.
	/// </summary>
	/// <param name="filePath">The local file path to convert.</param>
	/// <returns>The absolute file URI.</returns>
	public static string CreateFileUri(string filePath)
		=> new Uri(NormalizeLocalPath(filePath)).AbsoluteUri;

	/// <summary>
	/// Reports whether two normalized local paths identify the same logical path on the current host.
	/// </summary>
	/// <param name="left">The first normalized local path.</param>
	/// <param name="right">The second normalized local path.</param>
	/// <returns><see langword="true"/> when both paths should be treated as identical; otherwise, <see langword="false"/>.</returns>
	public static bool AreLocalPathsEqual(string? left, string? right)
		=> string.Equals(left, right, LocalPathComparison);

	/// <summary>
	/// Normalizes a local path into the absolute form used by the language server.
	/// </summary>
	/// <param name="filePath">The path to normalize.</param>
	/// <returns>The normalized absolute path.</returns>
	public static string NormalizeLocalPath(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			throw new ArgumentException("File path must not be empty.", nameof(filePath));

		string sanitizedFilePath = Path.DirectorySeparatorChar == Path.AltDirectorySeparatorChar
			? filePath
			: filePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

		return Path.TrimEndingDirectorySeparator(Path.GetFullPath(sanitizedFilePath));
	}

	/// <summary>
	/// Normalizes a file URI into the absolute local-path form used by the language server client.
	/// </summary>
	/// <param name="uri">The file URI to normalize.</param>
	/// <returns>The normalized absolute local path.</returns>
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
	/// Attempts to extract and normalize a local file path from a file URI.
	/// </summary>
	/// <param name="uriText">The file URI text.</param>
	/// <param name="filePath">The normalized local file path when successful.</param>
	/// <returns><see langword="true"/> when a local file path was resolved; otherwise, <see langword="false"/>.</returns>
	public static bool TryGetFilePath(string? uriText, out string filePath)
	{
		filePath = string.Empty;

		if (string.IsNullOrWhiteSpace(uriText)
			|| !Uri.TryCreate(uriText, UriKind.Absolute, out Uri? uri)
			|| uri?.IsFile != true)
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

	/// <summary>
	/// Gets the case-normalized dictionary key for a normalized local path.
	/// </summary>
	/// <param name="normalizedFilePath">The normalized local file path.</param>
	/// <returns>The dictionary key, uppercased when the local file system is case-insensitive.</returns>
	internal static string GetPathKeyFromNormalizedPath(string normalizedFilePath)
		=> UsesCaseSensitiveLocalPaths ? normalizedFilePath : normalizedFilePath.ToUpperInvariant();

	private static bool DeterminePathCaseSensitivity()
	{
		if (AppContext.TryGetSwitch(UseCaseSensitiveLocalPathsSwitch, out bool useCaseSensitiveLocalPaths)
			&& useCaseSensitiveLocalPaths)
		{
			return true;
		}

		if (AppContext.TryGetSwitch(UseCaseInsensitiveLocalPathsSwitch, out bool useCaseInsensitiveLocalPaths)
			&& useCaseInsensitiveLocalPaths)
		{
			return false;
		}

		return OperatingSystem.IsLinux();
	}
}
