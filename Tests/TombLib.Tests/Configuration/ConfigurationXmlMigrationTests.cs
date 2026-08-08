using System.IO;
using System.Text;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Configuration;

namespace TombLib.Tests;

[TestClass]
public class ConfigurationXmlMigrationTests
{
	public sealed class TestConfig : TextEditorConfigBase
	{
		public override string DefaultPath => "unused";
	}

	[TestMethod]
	public void MigrateLegacyAutoCloseQuotes_RewritesLegacyElement()
	{
		string xml = "<TestConfig><AutoCloseQuotes>true</AutoCloseQuotes></TestConfig>";

		string migrated = ConfigurationXmlMigration.MigrateLegacyAutoCloseQuotes(xml);

		Assert.IsFalse(migrated.Contains("<AutoCloseQuotes>"));
		Assert.IsTrue(migrated.Contains("<AutoCloseDoubleQuotes>true</AutoCloseDoubleQuotes>"));
		Assert.IsTrue(migrated.Contains("<AutoCloseSingleQuotes>true</AutoCloseSingleQuotes>"));
	}

	[TestMethod]
	public void MigrateLegacyAutoCloseQuotes_LeavesCurrentXmlUnchanged()
	{
		string xml = "<TestConfig><AutoCloseDoubleQuotes>false</AutoCloseDoubleQuotes><AutoCloseSingleQuotes>true</AutoCloseSingleQuotes></TestConfig>";

		string migrated = ConfigurationXmlMigration.MigrateLegacyAutoCloseQuotes(xml);

		Assert.AreEqual(xml, migrated);
	}

	[TestMethod]
	public void MigrateLegacyAutoCloseQuotes_InvalidXml_ReturnsOriginal()
	{
		string xml = "<TestConfig><AutoCloseQuotes>true</AutoCloseQuotes>";

		string migrated = ConfigurationXmlMigration.MigrateLegacyAutoCloseQuotes(xml);

		Assert.AreEqual(xml, migrated);
	}

	[TestMethod]
	public void Load_MigratesLegacyAutoCloseQuotesElement()
	{
		string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestConfig><AutoCloseQuotes>true</AutoCloseQuotes></TestConfig>";
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

		TestConfig loaded = TombLib.Scripting.UI.Bases.ConfigurationBase.Load<TestConfig>(stream);

		Assert.IsTrue(loaded.AutoCloseDoubleQuotes);
		Assert.IsTrue(loaded.AutoCloseSingleQuotes);
	}

	[TestMethod]
	public void Load_DoesNotCloseCallerStream()
	{
		string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestConfig><AutoCloseQuotes>true</AutoCloseQuotes></TestConfig>";
		var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

		TestConfig loaded = TombLib.Scripting.UI.Bases.ConfigurationBase.Load<TestConfig>(stream);

		// Load must not take ownership of the caller-supplied stream.
		Assert.IsTrue(loaded.AutoCloseDoubleQuotes);
		Assert.IsTrue(stream.CanRead);
	}
}
