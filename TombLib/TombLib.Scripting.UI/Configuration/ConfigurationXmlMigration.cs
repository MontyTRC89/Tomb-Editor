using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace TombLib.Scripting.UI.Configuration;

/// <summary>
/// Migrates legacy configuration XML to the current schema before it is deserialized.
/// </summary>
public static class ConfigurationXmlMigration
{
	/// <summary>
	/// Rewrites the legacy <c>AutoCloseQuotes</c> element into the current
	/// <c>AutoCloseDoubleQuotes</c> and <c>AutoCloseSingleQuotes</c> elements.
	/// </summary>
	/// <param name="xml">The configuration XML to migrate.</param>
	/// <returns>
	/// The migrated XML, or the original XML unchanged when it is empty or cannot be parsed.
	/// </returns>
	public static string MigrateLegacyAutoCloseQuotes(string xml)
	{
		if (string.IsNullOrWhiteSpace(xml))
			return xml;

		try
		{
			var document = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
			XElement[] legacyElements = document.Descendants("AutoCloseQuotes").ToArray();

			foreach (XElement element in legacyElements)
			{
				string? value = element.Value;
				element.ReplaceWith(
					new XElement("AutoCloseDoubleQuotes", value),
					new XElement("AutoCloseSingleQuotes", value));
			}

			return document.ToString(SaveOptions.DisableFormatting);
		}
		catch (XmlException)
		{
			return xml;
		}
	}
}
