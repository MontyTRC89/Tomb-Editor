using TombLib.Scripting.Lua.Objects;

namespace TombLib.Test;

[TestClass]
public class LuaColorSchemeTests
{
	[TestMethod]
	public void DefaultColorScheme_UsesLuaEditorDefaults()
	{
		var scheme = new ColorScheme();

		Assert.AreEqual("#202020", scheme.Background);
		Assert.AreEqual("Gainsboro", scheme.Foreground);
		Assert.AreEqual("#6A9955", scheme.Comments.HtmlColor);
		Assert.AreEqual("#569CD6", scheme.Values.HtmlColor);
		Assert.AreEqual("#569CD6", scheme.Statements.HtmlColor);
		Assert.AreEqual("Gainsboro", scheme.Operators.HtmlColor);
		Assert.AreEqual("#569CD6", scheme.SpecialOperators.HtmlColor);
	}
}