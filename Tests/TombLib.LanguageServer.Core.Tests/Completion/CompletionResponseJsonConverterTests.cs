using System.Text.Json;

namespace TombLib.LanguageServer.Core.Tests;

[TestClass]
public class CompletionResponseJsonConverterTests
{
	[TestMethod]
	public void DeserializeCompletionResponse_AppliesItemDefaultsToItems()
	{
		CompletionResponse? response = JsonSerializer.Deserialize<CompletionResponse>(
			"""
			{
			  "isIncomplete": true,
			  "itemDefaults": {
			    "editRange": {
			      "insert": {
			        "start": { "line": 2, "character": 1 },
			        "end": { "line": 2, "character": 3 }
			      },
			      "replace": {
			        "start": { "line": 2, "character": 1 },
			        "end": { "line": 2, "character": 8 }
			      }
			    },
			    "insertTextFormat": 2,
			    "data": {
			      "origin": "defaults"
			    }
			  },
			  "items": [
			    {
			      "label": "call",
			      "textEditText": "call(${1:arg})"
			    },
			    {
			      "label": "warn"
			    }
			  ]
			}
			""");

		Assert.IsNotNull(response);
		Assert.IsTrue(response.IsIncomplete);
		Assert.IsNotNull(response.Items);
		Assert.AreEqual(2, response.Items.Count);

		CompletionItemPayload firstItem = response.Items[0];
		Assert.AreEqual(2, firstItem.InsertTextFormat);
		Assert.IsNotNull(firstItem.TextEdit);
		Assert.AreEqual("call(${1:arg})", firstItem.TextEdit.NewText);
		Assert.IsNotNull(firstItem.TextEdit.Insert);
		Assert.IsNotNull(firstItem.TextEdit.Replace);
		Assert.AreEqual(2, firstItem.TextEdit.Insert?.Start?.Line);
		Assert.AreEqual(1, firstItem.TextEdit.Insert?.Start?.Character);
		Assert.AreEqual(8, firstItem.TextEdit.Replace?.End?.Character);
		Assert.IsNotNull(firstItem.ExtensionData);
		Assert.IsTrue(firstItem.ExtensionData.TryGetValue("data", out JsonElement firstItemData));
		Assert.AreEqual("defaults", firstItemData.GetProperty("origin").GetString());

		CompletionItemPayload secondItem = response.Items[1];
		Assert.AreEqual(2, secondItem.InsertTextFormat);
		Assert.IsNotNull(secondItem.TextEdit);
		Assert.AreEqual("warn", secondItem.TextEdit.NewText);
		Assert.IsNotNull(secondItem.ExtensionData);
		Assert.IsTrue(secondItem.ExtensionData.TryGetValue("data", out JsonElement secondItemData));
		Assert.AreEqual("defaults", secondItemData.GetProperty("origin").GetString());
	}
}
