using TombLib.Scripting.Signatures;

namespace TombLib.LanguageServer.Lua.Tests;

public partial class LuaLanguageServerResponseParserTests
{
	[TestMethod]
	public void ParseSignatureHelp_UsesParameterLabelOffsetsAndActiveParameter()
	{
		TextSignatureHelpInfo? signatureInfo = LuaLanguageServerResponseParser.ParseSignatureHelp(
			DeserializeSignatureHelpResponse(new
			{
				activeSignature = 0,
				activeParameter = 1,
				signatures = new[]
				{
					new
					{
						label = "spawn(room, objectName)",
						documentation = new
						{
							kind = "markdown",
							value = "Spawns an object."
						},
						parameters = new object[]
						{
							new
							{
								label = new[] { 6, 10 },
								documentation = "Room id."
							},
							new
							{
								label = new[] { 12, 22 },
								documentation = "Object name."
							}
						}
					}
				}
			}));

		Assert.IsNotNull(signatureInfo);
		Assert.AreEqual("spawn(room, objectName)", signatureInfo.Label);
		Assert.AreEqual("Spawns an object.", signatureInfo.Documentation);
		Assert.AreEqual(1, signatureInfo.ActiveParameterIndex);
		Assert.AreEqual(2, signatureInfo.Parameters.Count);
		Assert.AreEqual("objectName", signatureInfo.Parameters[1].Label);
		Assert.AreEqual("Object name.", signatureInfo.Parameters[1].Documentation);
	}

	[TestMethod]
	public void ParseSignatureHelp_UsesSignatureLevelActiveParameterWhenResponseOmitsIt()
	{
		TextSignatureHelpInfo? signatureInfo = LuaLanguageServerResponseParser.ParseSignatureHelp(
			DeserializeSignatureHelpResponse(new
			{
				signatures = new[]
				{
					new
					{
						label = "move(x, y)",
						activeParameter = 1,
						parameters = new object[]
						{
							new { label = "x" },
							new { label = "y" }
						}
					}
				}
			}));

		Assert.IsNotNull(signatureInfo);
		Assert.AreEqual(1, signatureInfo.ActiveParameterIndex);
		Assert.AreEqual("y", signatureInfo.Parameters[1].Label);
	}

	[TestMethod]
	public void ParseSignatureHelp_ClampsOutOfRangeActiveParameterToLastAvailableParameter()
	{
		TextSignatureHelpInfo? signatureInfo = LuaLanguageServerResponseParser.ParseSignatureHelp(
			DeserializeSignatureHelpResponse(new
			{
				activeSignature = 0,
				activeParameter = 9,
				signatures = new[]
				{
					new
					{
						label = "spawn(room, objectName)",
						parameters = new object[]
						{
							new { label = "room" },
							new { label = "objectName" }
						}
					}
				}
			}));

		Assert.IsNotNull(signatureInfo);
		Assert.AreEqual(2, signatureInfo.Parameters.Count);
		Assert.AreEqual(1, signatureInfo.ActiveParameterIndex);
		Assert.AreEqual("objectName", signatureInfo.Parameters[signatureInfo.ActiveParameterIndex].Label);
	}
}
