using DarkUI.Forms;
using System;

namespace ScriptEditor
{
	public partial class FormReindentRules : DarkForm
	{
		/// <summary>
		/// Configuration object.
		/// </summary>
		private Configuration _config = Configuration.Load();

		public FormReindentRules()
		{
			InitializeComponent();

			checkBox_PreEqualSpace.Checked = _config.Tidy_PreEqualSpace;
			checkBox_PostEqualSpace.Checked = _config.Tidy_PostEqualSpace;

			checkBox_PreCommaSpace.Checked = _config.Tidy_PreCommaSpace;
			checkBox_PostCommaSpace.Checked = _config.Tidy_PostCommaSpace;

			checkBox_ReduceSpaces.Checked = _config.Tidy_ReduceSpaces;
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
			_config.Tidy_PreEqualSpace = checkBox_PreEqualSpace.Checked;
			_config.Tidy_PostEqualSpace = checkBox_PostEqualSpace.Checked;

			_config.Tidy_PreCommaSpace = checkBox_PreCommaSpace.Checked;
			_config.Tidy_PostCommaSpace = checkBox_PostCommaSpace.Checked;

			_config.Tidy_ReduceSpaces = checkBox_ReduceSpaces.Checked;

			_config.Save();
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
