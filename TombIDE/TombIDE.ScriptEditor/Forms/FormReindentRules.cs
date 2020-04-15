using DarkUI.Forms;
using System;
using TombIDE.ScriptEditor.Controls;
using TombIDE.Shared;

namespace TombIDE.ScriptEditor
{
	internal partial class FormReindentRules : DarkForm
	{
		private IDE _ide;

		private AvalonTextBox textBox;

		public FormReindentRules(IDE ide)
		{
			_ide = ide;

			InitializeComponent();

			textBox = new AvalonTextBox
			{
				Text = "[Level]\r\nName=Coastal Ruins\r\nRain=ENABLED\r\nLayer1=128,128,128,-8\r\nMirror=69,$2137   ; Crossbow room\r\nLevel=DATA\\COASTAL,105",
				ShowLineNumbers = false
			};

			elementHost.Child = textBox;

			checkBox_PreEqualSpace.Checked = _ide.Configuration.Tidy_PreEqualSpace;
			checkBox_PostEqualSpace.Checked = _ide.Configuration.Tidy_PostEqualSpace;

			checkBox_PreCommaSpace.Checked = _ide.Configuration.Tidy_PreCommaSpace;
			checkBox_PostCommaSpace.Checked = _ide.Configuration.Tidy_PostCommaSpace;

			checkBox_ReduceSpaces.Checked = _ide.Configuration.Tidy_ReduceSpaces;
		}

		private void button_Default_Click(object sender, EventArgs e)
		{
			checkBox_PreEqualSpace.Checked = true;
			checkBox_PostEqualSpace.Checked = true;

			checkBox_PreCommaSpace.Checked = false;
			checkBox_PostCommaSpace.Checked = true;

			checkBox_ReduceSpaces.Checked = true;
		}

		private void button_Save_Click(object sender, EventArgs e)
		{
			_ide.Configuration.Tidy_PreEqualSpace = checkBox_PreEqualSpace.Checked;
			_ide.Configuration.Tidy_PostEqualSpace = checkBox_PostEqualSpace.Checked;

			_ide.Configuration.Tidy_PreCommaSpace = checkBox_PreCommaSpace.Checked;
			_ide.Configuration.Tidy_PostCommaSpace = checkBox_PostCommaSpace.Checked;

			_ide.Configuration.Tidy_ReduceSpaces = checkBox_ReduceSpaces.Checked;

			_ide.Configuration.Save();
		}

		private void checkBox_PreEqualSpace_CheckedChanged(object sender, EventArgs e)
		{
			textBox.Text = checkBox_PreEqualSpace.Checked ? textBox.Text.Replace("=", " =") : textBox.Text.Replace(" =", "=");
		}

		private void checkBox_PostEqualSpace_CheckedChanged(object sender, EventArgs e)
		{
			textBox.Text = checkBox_PostEqualSpace.Checked ? textBox.Text.Replace("=", "= ") : textBox.Text.Replace("= ", "=");
		}

		private void checkBox_PreCommaSpace_CheckedChanged(object sender, EventArgs e)
		{
			textBox.Text = checkBox_PreCommaSpace.Checked ? textBox.Text.Replace(",", " ,") : textBox.Text.Replace(" ,", ",");
		}

		private void checkBox_PostCommaSpace_CheckedChanged(object sender, EventArgs e)
		{
			textBox.Text = checkBox_PostCommaSpace.Checked ? textBox.Text.Replace(",", ", ") : textBox.Text.Replace(", ", ",");
		}

		private void checkBox_ReduceSpaces_CheckedChanged(object sender, EventArgs e)
		{
			textBox.Text = checkBox_ReduceSpaces.Checked ? textBox.Text.Replace("  ;", ";") : textBox.Text.Replace(";", "  ;");
		}
	}
}
