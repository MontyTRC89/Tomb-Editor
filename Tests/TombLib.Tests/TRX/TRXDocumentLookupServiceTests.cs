#nullable enable

using TombLib.Scripting.Text;
using TombLib.Scripting.TRX.Documents;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class TRXDocumentLookupServiceTests
{
	private static TRXDocumentLookupService CreateService()
	{
		var lineService = new TRXLineService();
		var documentService = new TRXDocumentService(lineService);

		return new TRXDocumentLookupService(documentService);
	}

	[TestMethod]
	public void IsLevelScriptDefined_ReturnsTrueForMatchingTitleLine()
	{
		var service = CreateService();
		var source = new StringTextSnapshot(
			"{\r\n" +
			"  \"levels\": [\r\n" +
			"    {\r\n" +
			"      \"title\": \"Vatican City\",\r\n" +
			"      \"file\": \"data\\\\level21.phd\"\r\n" +
			"    }\r\n" +
			"  ]\r\n" +
			"}\r\n");

		bool isDefined = service.IsLevelScriptDefined(source, "Vatican City");

		Assert.IsTrue(isDefined);
	}

	[TestMethod]
	public void IsLevelScriptDefined_ReturnsFalseForMissingLevel()
	{
		var service = CreateService();
		var source = new StringTextSnapshot(
			"{\r\n" +
			"  \"levels\": [\r\n" +
			"    {\r\n" +
			"      \"title\": \"Vatican City\",\r\n" +
			"      \"file\": \"data\\\\level21.phd\"\r\n" +
			"    }\r\n" +
			"  ]\r\n" +
			"}\r\n");

		bool isDefined = service.IsLevelScriptDefined(source, "Colosseum");

		Assert.IsFalse(isDefined);
	}
}