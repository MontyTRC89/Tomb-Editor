using DarkUI.Docking;
using TombIDE.Shared;

namespace TombIDE.ScriptEditor.ToolWindows
{
	public partial class CompilerLogs : DarkToolWindow
	{
		public CompilerLogs()
		{
			InitializeComponent();
			DockText = Strings.Default.CompilerLogs;
		}

		public void UpdateLogs(string text)
			=> richTextBox.Text = text;
	}
}
