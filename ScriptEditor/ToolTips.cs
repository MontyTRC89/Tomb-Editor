using FastColoredTextBoxNS;
using System.Collections;
using System.Globalization;
using System.Resources;

namespace ScriptEditor
{
	public class ToolTips
	{
		public static ToolTipNeededEventArgs CreateToolTip(FastColoredTextBox textEditor, ToolTipNeededEventArgs e)
		{
			// If tool tips are enabled in the settings and the hovered word isn't whitespace
			if (Properties.Settings.Default.ToolTips && !string.IsNullOrWhiteSpace(e.HoveredWord))
			{
				// If the hovered word is a header
				if (textEditor.GetLineText(e.Place.iLine).StartsWith("["))
				{
					ShowHeaderToolTip(textEditor, e);
				}
				else
				{
					ShowCommandToolTip(textEditor, e);
				}
			}

			return e;
		}

		private static ToolTipNeededEventArgs ShowHeaderToolTip(FastColoredTextBox textEditor, ToolTipNeededEventArgs e)
		{
			// ToolTip title with brackets added
			e.ToolTipTitle = "[" + e.HoveredWord + "]";

			// Get resources from HeaderToolTips.resx
			ResourceManager headerToolTipResource = new ResourceManager(typeof(Resources.HeaderToolTips));
			ResourceSet resourceSet = headerToolTipResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			// Loop through resources
			foreach (DictionaryEntry entry in resourceSet)
			{
				// If the hovered word matches a "header key"
				if (e.HoveredWord == entry.Key.ToString())
				{
					e.ToolTipText = entry.Value.ToString();
					return e;
				}
			}

			return e;
		}

		private static ToolTipNeededEventArgs ShowCommandToolTip(FastColoredTextBox textEditor, ToolTipNeededEventArgs e)
		{
			// Get resources from CommandToolTips.resx
			ResourceManager commandToolTipResource = new ResourceManager(typeof(Resources.CommandToolTips));
			ResourceSet resourceSet = commandToolTipResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			// There are different definitions for the "Level" key, so handle them all!
			if (e.HoveredWord == "Level")
			{
				HandleLevelToolTip(textEditor, e);
			}
			else
			{
				e.ToolTipTitle = e.HoveredWord;

				// Loop through resources
				foreach (DictionaryEntry entry in resourceSet)
				{
					// If the hovered word matches a "command key"
					if (e.HoveredWord == entry.Key.ToString())
					{
						e.ToolTipText = entry.Value.ToString();
						return e;
					}
				}
			}

			return e;
		}

		private static ToolTipNeededEventArgs HandleLevelToolTip(FastColoredTextBox textEditor, ToolTipNeededEventArgs e)
		{
			// Get the current line number
			int i = e.Place.iLine;

			do
			{
				if (i < 0)
				{
					// The line number might go to -1 and it will crash the app, so stop the loop to prevent it!
					return e;
				}

				if (textEditor.GetLineText(i).StartsWith("[PSXExtensions]"))
				{
					e.ToolTipTitle = "Level [PSXExtensions]";
					e.ToolTipText = Resources.CommandToolTips.LevelPSX;
					return e;
				}
				else if (textEditor.GetLineText(i).StartsWith("[PCExtensions]"))
				{
					e.ToolTipTitle = "Level [PCExtensions]";
					e.ToolTipText = Resources.CommandToolTips.LevelPC;
					return e;
				}
				else if (textEditor.GetLineText(i).StartsWith("[Title]"))
				{
					e.ToolTipTitle = "Level [Title]";
					e.ToolTipText = Resources.CommandToolTips.LevelTitle;
					return e;
				}
				else if (textEditor.GetLineText(i).StartsWith("[Level]"))
				{
					e.ToolTipTitle = "Level";
					e.ToolTipText = Resources.CommandToolTips.LevelLevel;
					return e;
				}

				i--; // Go 1 line higher if no header was found yet
			}
			while (!textEditor.GetLineText(i + 1).StartsWith("["));

			return e;
		}
	}
}
