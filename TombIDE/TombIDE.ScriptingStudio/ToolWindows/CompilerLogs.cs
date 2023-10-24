using DarkUI.Docking;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.ToolWindows
{
	public partial class CompilerLogs : DarkToolWindow
	{
		public CompilerLogs()
		{
			InitializeComponent();
			DockText = Strings.Default.CompilerLogs;
		}

		public void UpdateLogs(string text)
		{
			richTextBox.Text = text;
			richTextBox.ScrollToBottom();

			SetTextColor(richTextBox, "ERROR:", Color.Red);
			SetTextColor(richTextBox, "Completed compilation", Color.LightGreen);
		}

		private static void SetTextColor(RichTextBox textBox, string regexPattern, Color color)
		{
			foreach (Match item in Regex.Matches(textBox.Text, regexPattern))
			{
				textBox.Select(item.Index, item.Length);
				textBox.SelectionColor = color;
			}
		}
	}
}
