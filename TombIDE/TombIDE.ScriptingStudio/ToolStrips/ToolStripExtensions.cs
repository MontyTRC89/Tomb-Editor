using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ScriptingStudio.ToolStrips
{
	internal static class ToolStripExtensions
	{
		public static ToolStripItem FindItem(this ToolStrip toolStrip, UICommand element)
		{
			foreach (ToolStripItem item in toolStrip.GetAllItems())
				if (item.Tag is UICommand e && e == element)
					return item;

			return null;
		}

		public static IEnumerable<ToolStripItem> GetAllItems(this ToolStrip toolStrip)
			=> GetAllItems(toolStrip.Items);

		public static IEnumerable<ToolStripItem> GetAllItems(this ToolStripMenuItem menuItem)
			=> GetAllItems(menuItem.DropDownItems);

		public static IEnumerable<ToolStripItem> GetAllItems(this ToolStripItemCollection itemCollection)
		{
			foreach (ToolStripItem item in itemCollection)
			{
				yield return item;

				if (item is ToolStripMenuItem)
				{
					var stack = new Stack<ToolStripMenuItem>();
					stack.Push(item as ToolStripMenuItem);

					while (stack.Count > 0)
						foreach (ToolStripItem dropDownItem in stack.Pop().DropDownItems)
						{
							yield return dropDownItem;

							if (dropDownItem is ToolStripMenuItem)
								stack.Push(dropDownItem as ToolStripMenuItem);
						}
				}
			}
		}

		public static void ReduceSeparators(this ToolStripItemCollection itemCollection)
		{
			TrimSeparatorsOnStart(itemCollection);
			TrimSeparatorsOnEnd(itemCollection);

			RemoveSeparatorDuplicates(itemCollection);
		}

		public static void RemoveSeparatorDuplicates(this ToolStripItemCollection itemCollection)
		{
			var itemsToDispose = new List<IDisposable>();
			int separatorsInARowCount = 0;

			for (int i = 0; i < itemCollection.Count; i++)
			{
				ToolStripItem item = itemCollection[i];
				separatorsInARowCount = item is ToolStripSeparator ? separatorsInARowCount + 1 : 0;

				if (separatorsInARowCount > 1)
					itemsToDispose.Add(item);
			}

			SharedMethods.DisposeItems(itemsToDispose.ToArray());
		}

		public static void TrimSeparatorsOnStart(this ToolStripItemCollection itemCollection)
		{
			var itemsToDispose = new List<IDisposable>();
			int separatorsInARowCount = 0;

			for (int i = 0; i < itemCollection.Count; i++)
			{
				ToolStripItem item = itemCollection[i];

				if ((i == 0 || separatorsInARowCount > 0) && item is ToolStripSeparator)
				{
					itemsToDispose.Add(item);
					separatorsInARowCount++;
				}
				else
					break;
			}

			SharedMethods.DisposeItems(itemsToDispose.ToArray());
		}

		public static void TrimSeparatorsOnEnd(this ToolStripItemCollection itemCollection)
		{
			var itemsToDispose = new List<IDisposable>();
			int separatorsInARowCount = 0;

			int lastItemIndex = itemCollection.Count - 1;

			for (int i = lastItemIndex; i >= 0; i--)
			{
				ToolStripItem item = itemCollection[i];

				if ((i == lastItemIndex || separatorsInARowCount > 0) && item is ToolStripSeparator)
				{
					itemsToDispose.Add(item);
					separatorsInARowCount++;
				}
				else
					break;
			}

			SharedMethods.DisposeItems(itemsToDispose.ToArray());
		}
	}
}
