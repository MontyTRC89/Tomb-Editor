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

				bool isSeparator = node.Name.Equals("Separator", StringComparison.OrdinalIgnoreCase);
				bool isButton = node.Name.Equals("Button", StringComparison.OrdinalIgnoreCase);
				bool isNormalItem = node.Name.Equals("Item", StringComparison.OrdinalIgnoreCase);

				if (isSeparator)
					itemList.Add(new StudioSeparator());
				else if (isButton || isNormalItem)
				{
					var item = new StudioToolStripItem
					{
						LangKey = node.Attributes["LangKey"]?.Value ?? string.Empty,
						Command = node.Attributes["Command"]?.Value ?? string.Empty,
						Icon = node.Attributes["Icon"]?.Value ?? string.Empty,
						Keys = node.Attributes["Keys"]?.Value ?? string.Empty,
						KeysDisplay = node.Attributes["KeysDisplay"]?.Value ?? string.Empty,
						CheckOnClick = node.Attributes["CheckOnClick"]?.Value?.Equals("True", StringComparison.OrdinalIgnoreCase) ?? false,
						Position = node.Attributes["Position"]?.Value ?? string.Empty,
						DropDownItems = GetItems(node)
					};

					if (isButton)
						itemList.Add(StudioToolStripButton.FromNormalItem(item));
					else
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
