using System.Text.Json;

namespace Nickelony.LanguageServer.Core.Tests;

[TestClass]
public class SemanticTokensDeltaParserTests
{
	[TestMethod]
	public void Parse_MissingStartReturnsEmptyDeltaPayload()
	{
		SemanticTokensWireResponse response = JsonSerializer.Deserialize<SemanticTokensWireResponse>(
			"""
			{
			  "resultId": "delta-1",
			  "edits": [
			    {
			      "deleteCount": 2,
			      "data": [1, 2, 3]
			    }
			  ]
			}
			""");

		SemanticTokensDeltaResponse result = SemanticTokensDeltaParser.Parse(response);

		Assert.AreEqual("delta-1", result.ResultId);
		Assert.IsNull(result.Data);
		Assert.IsNull(result.Edits);
	}
}
