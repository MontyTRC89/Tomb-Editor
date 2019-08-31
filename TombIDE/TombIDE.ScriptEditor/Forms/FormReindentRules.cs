using DarkUI.Forms;
using System;
using TombIDE.Shared;

namespace TombIDE.ScriptEditor
{
	internal partial class FormReindentRules : DarkForm
	{
		private IDE _ide;

		public FormReindentRules(IDE ide)
		{
			_ide = ide;

			InitializeComponent();

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
			textBox_Preview.Text = checkBox_PreEqualSpace.Checked ? textBox_Preview.Text.Replace("=", " =") : textBox_Preview.Text.Replace(" =", "=");
		}

		private void checkBox_PostEqualSpace_CheckedChanged(object sender, EventArgs e)
		{
			textBox_Preview.Text = checkBox_PostEqualSpace.Checked ? textBox_Preview.Text.Replace("=", "= ") : textBox_Preview.Text.Replace("= ", "=");
		}

		private void checkBox_PreCommaSpace_CheckedChanged(object sender, EventArgs e)
		{
			textBox_Preview.Text = checkBox_PreCommaSpace.Checked ? textBox_Preview.Text.Replace(",", " ,") : textBox_Preview.Text.Replace(" ,", ",");
		}

		private void checkBox_PostCommaSpace_CheckedChanged(object sender, EventArgs e)
		{
			textBox_Preview.Text = checkBox_PostCommaSpace.Checked ? textBox_Preview.Text.Replace(",", ", ") : textBox_Preview.Text.Replace(", ", ",");
		}

		private void checkBox_ReduceSpaces_CheckedChanged(object sender, EventArgs e)
		{
			textBox_Preview.Text = checkBox_ReduceSpaces.Checked ? textBox_Preview.Text.Replace("  ;", ";") : textBox_Preview.Text.Replace(";", "  ;");
		}
	}
}
