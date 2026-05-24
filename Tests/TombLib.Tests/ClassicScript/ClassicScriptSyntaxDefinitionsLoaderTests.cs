using System.Text;
using TombLib.Scripting.Specifications.ClassicScript.Syntaxes;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptSyntaxDefinitionsLoaderTests
{
	[TestMethod]
	public void Load_ParsesSectionArgumentCountAndArrayMetadata()
	{
		string filePath = CreateTemporaryResx(
			("Legend", "[Level] Legend= {MESSAGE_STRING}"),
			("Customize", "[Level] Customize= {TYPE (CUST_...)}, {Arguments (*Array*)}"),
			("LevelPC", "[PCExtensions] Level= .TR4 ; Default value"));

		try
		{
			var loader = new ClassicScriptSyntaxDefinitionsLoader();
			Dictionary<string, ClassicScriptSyntaxDefinition> definitions = loader.Load(filePath)
				.ToDictionary(definition => definition.Key, StringComparer.OrdinalIgnoreCase);

			Assert.AreEqual(3, definitions.Count);

			ClassicScriptSyntaxDefinition legend = definitions["Legend"];
			Assert.AreEqual("Level", legend.ApplicableSection);
			Assert.AreEqual(1, legend.ArgumentCount);
			Assert.IsFalse(legend.HasArrayArguments);

			ClassicScriptSyntaxDefinition customize = definitions["Customize"];
			Assert.AreEqual("Level", customize.ApplicableSection);
			Assert.AreEqual(2, customize.ArgumentCount);
			Assert.IsTrue(customize.HasArrayArguments);

			ClassicScriptSyntaxDefinition levelPc = definitions["LevelPC"];
			Assert.AreEqual("PCExtensions", levelPc.ApplicableSection);
			Assert.AreEqual(1, levelPc.ArgumentCount);
			Assert.IsFalse(levelPc.HasArrayArguments);
		}
		finally
		{
			if (File.Exists(filePath))
				File.Delete(filePath);
		}
	}

	[TestMethod]
	public void Load_MissingFile_ReturnsEmptyList()
	{
		var loader = new ClassicScriptSyntaxDefinitionsLoader();

		IReadOnlyList<ClassicScriptSyntaxDefinition> definitions = loader.Load(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".resx"));

		Assert.AreEqual(0, definitions.Count);
	}

	private static string CreateTemporaryResx(params (string Key, string Value)[] entries)
	{
		string filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".resx");
		var builder = new StringBuilder();

		builder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
		builder.AppendLine("<root>");

		foreach ((string key, string value) in entries)
		{
			builder.AppendLine($"  <data name=\"{key}\" xml:space=\"preserve\">");
			builder.AppendLine($"    <value>{System.Security.SecurityElement.Escape(value)}</value>");
			builder.AppendLine("  </data>");
		}

		builder.AppendLine("</root>");
		File.WriteAllText(filePath, builder.ToString());
		return filePath;
	}
}