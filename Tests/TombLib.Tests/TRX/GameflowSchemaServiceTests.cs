#nullable enable

using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class GameFlowSchemaServiceTests
{
	[TestMethod]
	public void GetSchemaKeywords_ReturnsBundledSchemaKeywords()
	{
		var service = new GameFlowSchemaService(TRXResourcePaths.GetGameFlowSchemaPath());

		var keywords = service.GetSchemaKeywords();

		Assert.IsNotNull(keywords);
		Assert.IsTrue(keywords.Collections.Length > 0 || keywords.Properties.Length > 0 || keywords.Constants.Length > 0);
	}
}