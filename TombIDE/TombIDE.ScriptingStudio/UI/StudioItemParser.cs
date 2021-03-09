using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Properties;
using TombIDE.Shared;
using TombIDE.Shared.Local;

namespace TombIDE.ScriptingStudio.UI
{
	public static class StudioItemParser
	{
		public static string GetItemText(StudioToolStripItem item)
			=> typeof(Localization).GetProperty(item.LangKey)?.GetValue(Strings.Default)?.ToString() ?? item.LangKey;

		public static Image FindImageInResources(string key)
			=> typeof(Resources).GetProperty(key, BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as Image;

		public static Keys FindPredefinedKeys(string key)
		{
			object keysValue = typeof(UIKeys).GetField(key, BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
			return keysValue != null ? (Keys)keysValue : Keys.None;
		}

		public static UICommand GetCommand(string key)
		{
			Enum.TryParse(key, true, out UICommand command);
			return command;
		}
	}
}
