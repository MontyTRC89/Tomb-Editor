using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.ToolStrips
{
	internal static class ItemValidator
	{
		public static IEnumerable<ToolStripItem> GetValidMenuItems(Enum modeEnum, ToolStripItem[] items)
		{
			if (modeEnum is StudioMode)
				return GetItems(items, typeof(UI.StudioModePresets.MenuStripPresets).GetField(modeEnum.ToString()).GetValue(null) as UIElement[]);
			else if (modeEnum is DocumentMode)
				return GetItems(items, typeof(UI.DocumentModePresets.MenuStripPresets).GetField(modeEnum.ToString()).GetValue(null) as UIElement[]);

			return null;
		}

		public static IEnumerable<ToolStripItem> GetValidToolStripItems(Enum modeEnum, ToolStripItem[] items)
		{
			if (modeEnum is StudioMode)
				return GetItems(items, typeof(UI.StudioModePresets.ToolStripPresets).GetField(modeEnum.ToString()).GetValue(null) as UIElement[]);
			else if (modeEnum is DocumentMode)
				return GetItems(items, typeof(UI.DocumentModePresets.ToolStripPresets).GetField(modeEnum.ToString()).GetValue(null) as UIElement[]);

			return null;
		}

		public static IEnumerable<ToolStripItem> GetItems(ToolStripItem[] items, UIElement[] elements, bool includeSeparators = true)
		{
			if (elements != null)
				foreach (ToolStripItem item in items)
					if ((includeSeparators && item is ToolStripSeparator)
						|| item.Tag is UIElement element && elements.Contains(element))
						yield return item;
		}
	}
}
