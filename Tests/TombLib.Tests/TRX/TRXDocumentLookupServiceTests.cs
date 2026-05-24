#nullable enable

using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class TRXDocumentLookupServiceTests
{
	[TestMethod]
	public void IsLevelScriptDefined_ReturnsTrueForMatchingTitleLine()
	{
		var service = new TRXDocumentLookupService();
		var document = new TextDocument(
			"{\r\n" +
			"  \"levels\": [\r\n" +
			"    {\r\n" +
			"      \"title\": \"Vatican City\",\r\n" +
			"      \"file\": \"data\\\\level21.phd\"\r\n" +
			"    }\r\n" +
			"  ]\r\n" +
			"}\r\n");

		bool isDefined = service.IsLevelScriptDefined(document, "Vatican City");

		Assert.IsTrue(isDefined);
	}

	[TestMethod]
	public void IsLevelScriptDefined_ReturnsFalseForMissingLevel()
	{
		var service = new TRXDocumentLookupService();
		var document = new TextDocument(
			"{\r\n" +
			"  \"levels\": [\r\n" +
			"    {\r\n" +
			"      \"title\": \"Vatican City\",\r\n" +
			"      \"file\": \"data\\\\level21.phd\"\r\n" +
			"    }\r\n" +
			"  ]\r\n" +
			"}\r\n");

		bool isDefined = service.IsLevelScriptDefined(document, "Colosseum");

		Assert.IsFalse(isDefined);
	}
}