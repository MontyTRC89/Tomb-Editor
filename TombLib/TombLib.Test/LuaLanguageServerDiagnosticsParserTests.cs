using TombIDE.ScriptingStudio.Services.LuaIntellisense;

namespace TombLib.Test;

[TestClass]
public class LuaLanguageServerDiagnosticsParserTests
{
	[TestMethod]
	public void TryParse_PreservesZeroWidthDiagnosticOnEmptyLineByAnchoringToNextVisibleCharacter()
	{
		const string filePath = @"C:\Workspace\test.lua";
		const string content = "local value = 1\n\nnextLine = 2";

		bool parsed = LuaLanguageServerDiagnosticsParser.TryParse(
			CreateDiagnostics(line: 1, startCharacter: 0, endLine: 1, endCharacter: 0),
			filePath,
			content,
			documentVersion: 1,
			out LuaPublishedDiagnostics? publishedDiagnostics);

		Assert.IsTrue(parsed);
		Assert.IsNotNull(publishedDiagnostics);
		Assert.AreEqual(1, publishedDiagnostics.Diagnostics.Count);
		Assert.AreEqual(content.IndexOf("nextLine", StringComparison.Ordinal), publishedDiagnostics.Diagnostics[0].StartOffset);
		Assert.AreEqual(publishedDiagnostics.Diagnostics[0].StartOffset + 1, publishedDiagnostics.Diagnostics[0].EndOffset);
	}

	[TestMethod]
	public void TryParse_PreservesZeroWidthDiagnosticOnTrailingEmptyLineByAnchoringToPreviousVisibleCharacter()
	{
		const string filePath = @"C:\Workspace\test.lua";
		const string content = "return value\n";

		bool parsed = LuaLanguageServerDiagnosticsParser.TryParse(
			CreateDiagnostics(line: 1, startCharacter: 0, endLine: 1, endCharacter: 0),
			filePath,
			content,
			documentVersion: 1,
			out LuaPublishedDiagnostics? publishedDiagnostics);

		Assert.IsTrue(parsed);
		Assert.IsNotNull(publishedDiagnostics);
		Assert.AreEqual(1, publishedDiagnostics.Diagnostics.Count);
		Assert.AreEqual('e', content[publishedDiagnostics.Diagnostics[0].StartOffset]);
		Assert.AreEqual(publishedDiagnostics.Diagnostics[0].StartOffset + 1, publishedDiagnostics.Diagnostics[0].EndOffset);
	}

	private static LuaPublishDiagnosticsParams CreateDiagnostics(int line, int startCharacter, int endLine, int endCharacter)
		=> new(
			Uri: null,
			Version: 1,
			Diagnostics:
			[
				new LuaDiagnosticPayload(
					new LuaProtocolRangePayload(
						new LuaProtocolNullablePosition(line, startCharacter),
						new LuaProtocolNullablePosition(endLine, endCharacter)),
					1,
					"Syntax error.",
					null,
					null)
			]);
}
