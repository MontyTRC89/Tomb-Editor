#nullable enable

using TombLib.Scripting.Specifications.TRX;
using TombLib.Scripting.Specifications.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class GameflowSchemaServiceTests
{
	[TestMethod]
	public void GetSchemaKeywords_ReturnsBundledSchemaKeywords()
	{
		var service = new GameflowSchemaService(TrxResourcePaths.GetGameflowSchemaPath());

		var keywords = service.GetSchemaKeywords();

		Assert.IsNotNull(keywords);
		Assert.IsTrue(keywords.Collections.Length > 0 || keywords.Properties.Length > 0 || keywords.Constants.Length > 0);
	}
}