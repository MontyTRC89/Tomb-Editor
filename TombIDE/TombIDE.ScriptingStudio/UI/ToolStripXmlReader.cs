using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace TombIDE.ScriptingStudio.UI
{
	public static class ToolStripXmlReader
	{
		public static List<StudioToolStripItem> GetItemsFromXml(string xmlNamespacePath)
		{
			var document = new XmlDocument();
			document.LoadXml(GetResourceFileText(xmlNamespacePath, Assembly.GetExecutingAssembly().GetName().Name));

			XmlNode root = document.SelectSingleNode("Items");

			return GetItems(root);
		}

		private static List<StudioToolStripItem> GetItems(XmlNode root)
		{
			var itemList = new List<StudioToolStripItem>();

			foreach (XmlNode node in root.ChildNodes)
			{
				if (node.NodeType == XmlNodeType.Comment)
					continue;

				if (node.Name.Equals("Separator", StringComparison.OrdinalIgnoreCase))
					itemList.Add(new StudioSeparator());
				else if (node.Name.Equals("Button", StringComparison.OrdinalIgnoreCase))
					itemList.Add(new StudioToolStripButton());
				else if (node.Name.Equals("Item", StringComparison.OrdinalIgnoreCase))
				{
					var item = new StudioToolStripItem
					{
						LangKey = node.Attributes["LangKey"]?.Value,
						Command = node.Attributes["Command"]?.Value,
						Icon = node.Attributes["Icon"]?.Value,
						Keys = node.Attributes["Keys"]?.Value,
						KeysDisplay = node.Attributes["KeysDisplay"]?.Value,
						CheckOnClick = node.Attributes["CheckOnClick"]?.Value?.Equals("True", StringComparison.OrdinalIgnoreCase),
						OverrideText = node.Attributes["OverrideText"]?.Value,
						Position = node.Attributes["Position"]?.Value ?? "-1",
						DropDownItems = GetItems(node)
					};

					itemList.Add(item);
				}
			}

			return itemList;
		}

		public static string GetResourceFileText(string filename, string assemblyName)
		{
			string result = string.Empty;

			using (Stream stream = Assembly.Load(assemblyName).GetManifestResourceStream($"{assemblyName}.{filename}"))
			using (var reader = new StreamReader(stream))
				result = reader.ReadToEnd();

			return result;
		}
	}
}
