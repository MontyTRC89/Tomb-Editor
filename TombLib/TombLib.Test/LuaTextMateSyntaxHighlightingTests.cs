using ICSharpCode.AvalonEdit.Highlighting;
using TombLib.Scripting.Highlighting;

namespace TombLib.Test;

[TestClass]
public class LuaTextMateSyntaxHighlightingTests
{
	[TestMethod]
	public void LoadFallbackHighlighting_ReturnsLuaDefinition()
	{
		IHighlightingDefinition? highlighting = LuaTextMateSyntaxHighlighting.LoadFallbackHighlighting();

		Assert.IsNotNull(highlighting);
		Assert.AreEqual("Lua", highlighting.Name);
	}
}