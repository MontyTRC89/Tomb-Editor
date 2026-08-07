using System.IO;
using TombLib.Scripting.ClassicScript.Compilers;

namespace TombLib.Tests;

[TestClass]
public class NGCompilerFixLogsTests
{
	[TestMethod]
	public void FixLogFile_MissingLog_ReturnsNullWithoutThrowing()
	{
		string missingLogPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

		string? result = NGCompiler.FixLogFile(missingLogPath, "C:\\Engine", "C:\\VGE");

		Assert.IsNull(result);
	}

	[TestMethod]
	public void FixLogFile_ExistingLog_ReplacesPathsAndFixesTypo()
	{
		string logFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		File.WriteAllText(logFilePath, "ERROR: unknonw at C:\\VGE\\Script\\Script.txt");

		try
		{
			string? result = NGCompiler.FixLogFile(logFilePath, "C:\\Engine", "C:\\VGE");

			Assert.IsNotNull(result);
			Assert.IsFalse(result.Contains("ERROR: unknonw "));
			Assert.IsTrue(result.Contains("ERROR: unknown "));
			Assert.IsTrue(result.Contains("C:\\Engine\\Script\\Script.txt"));
			Assert.AreEqual(result, File.ReadAllText(logFilePath));
		}
		finally
		{
			File.Delete(logFilePath);
		}
	}
}
