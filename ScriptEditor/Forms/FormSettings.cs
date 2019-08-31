using DarkUI.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ScriptEditor
{
	public partial class FormSettings : DarkForm
	{
		/// <summary>
		/// The number of critical settings changed.
		/// </summary>
		public int RestartItemCount = 0;

		/// <summary>
		/// Configuration object.
		/// </summary>
		private Configuration _config = Configuration.Load();

		public FormSettings()
		{
			InitializeComponent();

			numeric_FontSize.Value = (int)_config.FontSize;
			comboBox_FontFace.SelectedItem = _config.FontFamily;

			// General options
			checkBox_Autocomplete.Checked = _config.Autocomplete;
			checkBox_AutoCloseBrackets.Checked = _config.AutoCloseBrackets;
			checkBox_ShowSpaces.Checked = _config.ShowSpaces;
			checkBox_WordWrap.Checked = _config.WordWrap;
			checkBox_ReindentOnSave.Checked = _config.Tidy_ReindentOnSave;

			// Script syntax colors
			button_CommentColor.BackColor = ColorTranslator.FromHtml(_config.ScriptColors_Comment);
			button_SectionColor.BackColor = ColorTranslator.FromHtml(_config.ScriptColors_Section);
			button_NewCommandColor.BackColor = ColorTranslator.FromHtml(_config.ScriptColors_NewCommand);
			button_OldCommandColor.BackColor = ColorTranslator.FromHtml(_config.ScriptColors_OldCommand);
			button_UnknownCommandColor.BackColor = ColorTranslator.FromHtml(_config.ScriptColors_UnknownCommand);
			button_ValueColor.BackColor = ColorTranslator.FromHtml(_config.ScriptColors_Value);
			button_ReferenceColor.BackColor = ColorTranslator.FromHtml(_config.ScriptColors_Reference);

			label_RestartRequired.Visible = false;
		}

		private void button_Apply_Click(object sender, EventArgs e)
		{
			_config.FontSize = Convert.ToSingle(numeric_FontSize.Value);
			_config.FontFamily = comboBox_FontFace.SelectedItem.ToString();

			// General options
			_config.Autocomplete = checkBox_Autocomplete.Checked;
			_config.AutoCloseBrackets = checkBox_AutoCloseBrackets.Checked;
			_config.ShowSpaces = checkBox_ShowSpaces.Checked;
			_config.WordWrap = checkBox_WordWrap.Checked;
			_config.Tidy_ReindentOnSave = checkBox_ReindentOnSave.Checked;

			// Script syntax colors
			_config.ScriptColors_Comment = ColorTranslator.ToHtml(button_CommentColor.BackColor);
			_config.ScriptColors_Section = ColorTranslator.ToHtml(button_SectionColor.BackColor);
			_config.ScriptColors_NewCommand = ColorTranslator.ToHtml(button_NewCommandColor.BackColor);
			_config.ScriptColors_OldCommand = ColorTranslator.ToHtml(button_OldCommandColor.BackColor);
			_config.ScriptColors_UnknownCommand = ColorTranslator.ToHtml(button_UnknownCommandColor.BackColor);
			_config.ScriptColors_Value = ColorTranslator.ToHtml(button_ValueColor.BackColor);
			_config.ScriptColors_Reference = ColorTranslator.ToHtml(button_ReferenceColor.BackColor);

			_config.Save();
		}

		private void button_ResetDefault_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this,
				Resources.Messages.ResetSettings, "Reset?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (result == DialogResult.Yes)
			{
				numeric_FontSize.Value = 12;
				comboBox_FontFace.SelectedItem = "Consolas";

				// General options
				checkBox_Autocomplete.Checked = true;
				checkBox_AutoCloseBrackets.Checked = true;
				checkBox_ShowSpaces.Checked = false;
				checkBox_WordWrap.Checked = false;
				checkBox_ReindentOnSave.Checked = false;

				// Script syntax colors
				button_CommentColor.BackColor = Color.Green;
				button_SectionColor.BackColor = Color.SteelBlue;
				button_NewCommandColor.BackColor = Color.SpringGreen;
				button_OldCommandColor.BackColor = Color.MediumAquamarine;
				button_UnknownCommandColor.BackColor = Color.Red;
				button_ValueColor.BackColor = Color.LightSalmon;
				button_ReferenceColor.BackColor = Color.Orchid;

				label_RestartRequired.Visible = true;
				RestartItemCount = 9999; // The user has to restart the editor, no matter what
			}
		}

		private void button_ReindentRules_Click(object sender, EventArgs e)
		{
			using (FormReindentRules form = new FormReindentRules())
			{
				if (form.ShowDialog(this) == DialogResult.OK)
					_config = Configuration.Load();
			}
		}

		private void button_CommentColor_Click(object sender, EventArgs e)
		{
			DialogResult result = dialog_CommentColor.ShowDialog();

			if (result == DialogResult.OK)
			{
				button_CommentColor.BackColor = dialog_CommentColor.Color;
				CheckRestartRequirement(button_CommentColor.BackColor, _config.ScriptColors_Comment);
			}
		}

		private void button_SectionColor_Click(object sender, EventArgs e)
		{
			DialogResult result = dialog_SectionColor.ShowDialog();

			if (result == DialogResult.OK)
			{
				button_SectionColor.BackColor = dialog_SectionColor.Color;
				CheckRestartRequirement(button_SectionColor.BackColor, _config.ScriptColors_Section);
			}
		}

		private void button_NewCommandColor_Click(object sender, EventArgs e)
		{
			DialogResult result = dialog_NewCommandColor.ShowDialog();

			if (result == DialogResult.OK)
			{
				button_NewCommandColor.BackColor = dialog_NewCommandColor.Color;
				CheckRestartRequirement(button_NewCommandColor.BackColor, _config.ScriptColors_NewCommand);
			}
		}

		private void button_OldCommandColor_Click(object sender, EventArgs e)
		{
			DialogResult result = dialog_OldCommandColor.ShowDialog();

			if (result == DialogResult.OK)
			{
				button_OldCommandColor.BackColor = dialog_OldCommandColor.Color;
				CheckRestartRequirement(button_OldCommandColor.BackColor, _config.ScriptColors_OldCommand);
			}
		}

		private void button_UnknownCommandColor_Click(object sender, EventArgs e)
		{
			DialogResult result = dialog_UnknownCommandColor.ShowDialog();

			if (result == DialogResult.OK)
			{
				button_UnknownCommandColor.BackColor = dialog_UnknownCommandColor.Color;
				CheckRestartRequirement(button_UnknownCommandColor.BackColor, _config.ScriptColors_UnknownCommand);
			}
		}

		private void button_ValueColor_Click(object sender, EventArgs e)
		{
			DialogResult result = dialog_ValueColor.ShowDialog();

			if (result == DialogResult.OK)
			{
				button_ValueColor.BackColor = dialog_ValueColor.Color;
				CheckRestartRequirement(button_ValueColor.BackColor, _config.ScriptColors_Value);
			}
		}

		private void button_ReferenceColor_Click(object sender, EventArgs e)
		{
			DialogResult result = dialog_ReferenceColor.ShowDialog();

			if (result == DialogResult.OK)
			{
				button_ReferenceColor.BackColor = dialog_ReferenceColor.Color;
				CheckRestartRequirement(button_ReferenceColor.BackColor, _config.ScriptColors_Reference);
			}
		}

		private void checkBox_Autocomplete_CheckedChanged(object sender, EventArgs e) => CheckRestartRequirement(checkBox_Autocomplete.Checked, _config.Autocomplete);
		private void checkBox_ShowSpaces_CheckedChanged(object sender, EventArgs e) => CheckRestartRequirement(checkBox_ShowSpaces.Checked, _config.ShowSpaces);

		private void CheckRestartRequirement(object currentState, object prevSetting)
		{
			if (currentState.ToString() != prevSetting.ToString())
			{
				RestartItemCount++;
				label_RestartRequired.Visible = true;
			}
			else
			{
				if (RestartItemCount != 0)
					RestartItemCount--;

				if (RestartItemCount == 0)
					label_RestartRequired.Visible = false;
			}
		}
	}
}
