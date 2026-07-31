namespace Nickelony.LanguageServer.Core.Tests;

[TestClass]
public class LanguageServerPathHelperTests
{
	[TestMethod]
	public void CreateFileUri_AndTryGetFilePath_RoundTripNormalizedPath()
	{
		string expectedPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Path Helper", "test file.lua"));
		string rawPath = expectedPath.Replace('\\', '/');

		string uri = LanguageServerPathHelper.CreateFileUri(rawPath);

		Assert.IsTrue(LanguageServerPathHelper.TryGetFilePath(uri, out string filePath));
		Assert.AreEqual(expectedPath, filePath);
	}

	[TestMethod]
	public void NormalizeLocalPath_TrimsTrailingDirectorySeparatorForNonRootPath()
	{
		string rawPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Path Helper", "Folder")) + Path.DirectorySeparatorChar;
		string normalizedPath = LanguageServerPathHelper.NormalizeLocalPath(rawPath);

		Assert.AreEqual(Path.TrimEndingDirectorySeparator(Path.GetFullPath(rawPath)), normalizedPath);
	}

	[TestMethod]
	public void AreLocalPathsEqual_FollowsConfiguredPlatformCaseSensitivity()
	{
		string directoryPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Path Helper"));
		string lowerCasePath = LanguageServerPathHelper.NormalizeLocalPath(Path.Combine(directoryPath, "case.lua"));
		string upperCasePath = LanguageServerPathHelper.NormalizeLocalPath(Path.Combine(directoryPath, "CASE.lua"));

		Assert.AreEqual(
			!LanguageServerPathHelper.UsesCaseSensitiveLocalPaths,
			LanguageServerPathHelper.AreLocalPathsEqual(lowerCasePath, upperCasePath));
	}

	[TestMethod]
	public void NormalizeLocalPath_Uri_HandlesUncPath()
	{
		if (!OperatingSystem.IsWindows())
			return;

		Uri uri = new("file://server/share/folder/test.lua");
		string expectedPath = Path.GetFullPath(@"\\server\share\folder\test.lua");

		string normalizedPath = LanguageServerPathHelper.NormalizeLocalPath(uri);

		Assert.AreEqual(expectedPath, normalizedPath);
	}

	[TestMethod]
	public void TryGetFilePath_ReturnsFalseForNonFileUri()
	{
		Assert.IsFalse(LanguageServerPathHelper.TryGetFilePath("https://example.com/test.lua", out string filePath));
		Assert.AreEqual(string.Empty, filePath);
	}

	[TestMethod]
	public void TryNormalizeLocalPath_ReturnsFalseForBlankInput()
	{
		Assert.IsFalse(LanguageServerPathHelper.TryNormalizeLocalPath(" ", out string normalizedPath));
		Assert.AreEqual(string.Empty, normalizedPath);
	}
}
