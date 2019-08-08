using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TombIDE.Shared;

namespace TombIDE.ScriptEditor
{
	internal partial class FormSettings : DarkForm
	{
		/// <summary>
		/// The number of critical settings changed.
		/// </summary>
		public int RestartItemCount = 0;

		private IDE _ide;

		public FormSettings(IDE ide)
		{
			_ide = ide;

			InitializeComponent();

			List<string> monospacedFonts = MonospacedFonts.GetMonospacedFontNames();
			comboBox_FontFace.Items.AddRange(monospacedFonts.ToArray());

			numeric_FontSize.Value = (int)_ide.Configuration.FontSize;
			comboBox_FontFace.SelectedItem = _ide.Configuration.FontFamily;

			// General options
			checkBox_Autocomplete.Checked = _ide.Configuration.Autocomplete;
			checkBox_AutoCloseBrackets.Checked = _ide.Configuration.AutoCloseBrackets;
			checkBox_WordWrap.Checked = _ide.Configuration.WordWrap;
			checkBox_ReindentOnSave.Checked = _ide.Configuration.Tidy_ReindentOnSave;

			// Script syntax colors
			button_CommentColor.BackColor = ColorTranslator.FromHtml(_ide.Configuration.ScriptColors_Comment);
			button_SectionColor.BackColor = ColorTranslator.FromHtml(_ide.Configuration.ScriptColors_Section);
			button_NewCommandColor.BackColor = ColorTranslator.FromHtml(_ide.Configuration.ScriptColors_NewCommand);
			button_OldCommandColor.BackColor = ColorTranslator.FromHtml(_ide.Configuration.ScriptColors_OldCommand);
			button_UnknownCommandColor.BackColor = ColorTranslator.FromHtml(_ide.Configuration.ScriptColors_UnknownCommand);
			button_ValueColor.BackColor = ColorTranslator.FromHtml(_ide.Configuration.ScriptColors_Value);
			button_ReferenceColor.BackColor = ColorTranslator.FromHtml(_ide.Configuration.ScriptColors_Reference);

			label_RestartRequired.Visible = false;
		}

		private void button_Apply_Click(object sender, EventArgs e)
		{
			_ide.Configuration.FontSize = Convert.ToSingle(numeric_FontSize.Value);
			_ide.Configuration.FontFamily = comboBox_FontFace.SelectedItem.ToString();

			// General options
			_ide.Configuration.Autocomplete = checkBox_Autocomplete.Checked;
			_ide.Configuration.AutoCloseBrackets = checkBox_AutoCloseBrackets.Checked;
			_ide.Configuration.WordWrap = checkBox_WordWrap.Checked;
			_ide.Configuration.Tidy_ReindentOnSave = checkBox_ReindentOnSave.Checked;

			// Script syntax colors
			_ide.Configuration.ScriptColors_Comment = ColorTranslator.ToHtml(button_CommentColor.BackColor);
			_ide.Configuration.ScriptColors_Section = ColorTranslator.ToHtml(button_SectionColor.BackColor);
			_ide.Configuration.ScriptColors_NewCommand = ColorTranslator.ToHtml(button_NewCommandColor.BackColor);
			_ide.Configuration.ScriptColors_OldCommand = ColorTranslator.ToHtml(button_OldCommandColor.BackColor);
			_ide.Configuration.ScriptColors_UnknownCommand = ColorTranslator.ToHtml(button_UnknownCommandColor.BackColor);
			_ide.Configuration.ScriptColors_Value = ColorTranslator.ToHtml(button_ValueColor.BackColor);
			_ide.Configuration.ScriptColors_Reference = ColorTranslator.ToHtml(button_ReferenceColor.BackColor);

			_ide.Configuration.Save();
		}

		private void button_ResetDefault_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this,
				"Are you sure you want to reset all settings to default?", "Reset?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (result == DialogResult.Yes)
			{
				numeric_FontSize.Value = 12;
				comboBox_FontFace.SelectedItem = "Consolas";

				// General options
				checkBox_Autocomplete.Checked = true;
				checkBox_AutoCloseBrackets.Checked = true;
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
				RestartItemCount = 9999; // The user has to restart the editor now, no matter what
			}
		}

		private void button_ReindentRules_Click(object sender, EventArgs e)
		{
			using (FormReindentRules form = new FormReindentRules(_ide))
				form.ShowDialog(this);
		}

		private void button_CommentColor_Click(object sender, EventArgs e)
		{
			if (dialog_CommentColor.ShowDialog(this) == DialogResult.OK)
			{
				button_CommentColor.BackColor = dialog_CommentColor.Color;
				CheckRestartRequirement(button_CommentColor.BackColor, _ide.Configuration.ScriptColors_Comment);
			}
		}

		private void button_SectionColor_Click(object sender, EventArgs e)
		{
			if (dialog_SectionColor.ShowDialog(this) == DialogResult.OK)
			{
				button_SectionColor.BackColor = dialog_SectionColor.Color;
				CheckRestartRequirement(button_SectionColor.BackColor, _ide.Configuration.ScriptColors_Section);
			}
		}

		private void button_NewCommandColor_Click(object sender, EventArgs e)
		{
			if (dialog_NewCommandColor.ShowDialog(this) == DialogResult.OK)
			{
				button_NewCommandColor.BackColor = dialog_NewCommandColor.Color;
				CheckRestartRequirement(button_NewCommandColor.BackColor, _ide.Configuration.ScriptColors_NewCommand);
			}
		}

		private void button_OldCommandColor_Click(object sender, EventArgs e)
		{
			if (dialog_OldCommandColor.ShowDialog(this) == DialogResult.OK)
			{
				button_OldCommandColor.BackColor = dialog_OldCommandColor.Color;
				CheckRestartRequirement(button_OldCommandColor.BackColor, _ide.Configuration.ScriptColors_OldCommand);
			}
		}

		private void button_UnknownCommandColor_Click(object sender, EventArgs e)
		{
			if (dialog_UnknownCommandColor.ShowDialog(this) == DialogResult.OK)
			{
				button_UnknownCommandColor.BackColor = dialog_UnknownCommandColor.Color;
				CheckRestartRequirement(button_UnknownCommandColor.BackColor, _ide.Configuration.ScriptColors_UnknownCommand);
			}
		}

		private void button_ValueColor_Click(object sender, EventArgs e)
		{
			if (dialog_ValueColor.ShowDialog(this) == DialogResult.OK)
			{
				button_ValueColor.BackColor = dialog_ValueColor.Color;
				CheckRestartRequirement(button_ValueColor.BackColor, _ide.Configuration.ScriptColors_Value);
			}
		}

		private void button_ReferenceColor_Click(object sender, EventArgs e)
		{
			if (dialog_ReferenceColor.ShowDialog(this) == DialogResult.OK)
			{
				button_ReferenceColor.BackColor = dialog_ReferenceColor.Color;
				CheckRestartRequirement(button_ReferenceColor.BackColor, _ide.Configuration.ScriptColors_Reference);
			}
		}

		private void checkBox_Autocomplete_CheckedChanged(object sender, EventArgs e) => CheckRestartRequirement(checkBox_Autocomplete.Checked, _ide.Configuration.Autocomplete);

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
