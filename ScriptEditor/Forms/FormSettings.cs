using DarkUI.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ScriptEditor
{
	public partial class FormSettings : DarkForm
	{
		/// <summary>
		/// The number of critical settings changed
		/// </summary>
		public int _restartItemCount = 0;

		public FormSettings()
		{
			InitializeComponent();

			fontSizeNumeric.Value = Properties.Settings.Default.FontSize;
			fontFaceCombo.SelectedItem = Properties.Settings.Default.FontFace;
			autosaveCombo.SelectedItem = Properties.Settings.Default.AutosaveTime == 0 ? "None" : Properties.Settings.Default.AutosaveTime.ToString();

			reindentCheck.Checked = Properties.Settings.Default.ReindentOnSave;
			closeBracketsCheck.Checked = Properties.Settings.Default.CloseBrackets;
			showSpacesCheck.Checked = Properties.Settings.Default.ShowSpaces;
			wordWrapCheck.Checked = Properties.Settings.Default.WordWrap;

			autocompleteCheck.Checked = Properties.Settings.Default.Autocomplete;
			toolTipCheck.Checked = Properties.Settings.Default.ToolTips;

			showToolbarCheck.Checked = Properties.Settings.Default.ShowToolbar;
			showNumbersCheck.Checked = Properties.Settings.Default.ShowLineNumbers;

			commentColorButton.BackColor = Properties.Settings.Default.CommentColor;
			refColorButton.BackColor = Properties.Settings.Default.ReferenceColor;
			valueColorButton.BackColor = Properties.Settings.Default.ValueColor;
			headerColorButton.BackColor = Properties.Settings.Default.HeaderColor;
			newColorButton.BackColor = Properties.Settings.Default.NewCommandColor;
			oldColorButton.BackColor = Properties.Settings.Default.OldCommandColor;
			unknownColorButton.BackColor = Properties.Settings.Default.UnknownColor;

			restartLabel.Visible = false;
		}

		private void reindentRulesButton_Click(object sender, EventArgs e)
		{
			using (FormReindentRules form = new FormReindentRules())
			{
				form.ShowDialog(this);
			}
		}

		private void applyButton_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.FontSize = fontSizeNumeric.Value;
			Properties.Settings.Default.FontFace = fontFaceCombo.SelectedItem.ToString();

			if (autosaveCombo.SelectedItem.ToString() == "None")
			{
				Properties.Settings.Default.AutosaveTime = 0;
			}
			else
			{
				Properties.Settings.Default.AutosaveTime = int.Parse(autosaveCombo.SelectedItem.ToString());
			}

			Properties.Settings.Default.ReindentOnSave = reindentCheck.Checked;
			Properties.Settings.Default.CloseBrackets = closeBracketsCheck.Checked;
			Properties.Settings.Default.ShowSpaces = showSpacesCheck.Checked;
			Properties.Settings.Default.WordWrap = wordWrapCheck.Checked;

			Properties.Settings.Default.Autocomplete = autocompleteCheck.Checked;
			Properties.Settings.Default.ToolTips = toolTipCheck.Checked;

			Properties.Settings.Default.ShowToolbar = showToolbarCheck.Checked;
			Properties.Settings.Default.ShowLineNumbers = showNumbersCheck.Checked;

			Properties.Settings.Default.CommentColor = commentColorButton.BackColor;
			Properties.Settings.Default.ReferenceColor = refColorButton.BackColor;
			Properties.Settings.Default.ValueColor = valueColorButton.BackColor;
			Properties.Settings.Default.HeaderColor = headerColorButton.BackColor;
			Properties.Settings.Default.NewCommandColor = newColorButton.BackColor;
			Properties.Settings.Default.OldCommandColor = oldColorButton.BackColor;
			Properties.Settings.Default.UnknownColor = unknownColorButton.BackColor;

			Properties.Settings.Default.Save();
		}

		private void resetDefaultButton_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this,
				Resources.Messages.ResetSettings, "Reset?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (result == DialogResult.No)
			{
				return;
			}

			fontSizeNumeric.Value = 12;
			fontFaceCombo.SelectedItem = "Consolas";
			autosaveCombo.SelectedItem = "None";

			reindentCheck.Checked = false;
			closeBracketsCheck.Checked = true;
			showSpacesCheck.Checked = false;
			wordWrapCheck.Checked = false;

			autocompleteCheck.Checked = true;
			toolTipCheck.Checked = true;

			showToolbarCheck.Checked = true;
			showNumbersCheck.Checked = true;

			commentColorButton.BackColor = Color.Green;
			refColorButton.BackColor = Color.Orchid;
			valueColorButton.BackColor = Color.LightSalmon;
			headerColorButton.BackColor = Color.SteelBlue;
			newColorButton.BackColor = Color.SpringGreen;
			oldColorButton.BackColor = Color.MediumAquamarine;
			unknownColorButton.BackColor = Color.Red;
		}

		private void commentColorButton_Click(object sender, EventArgs e)
		{
			DialogResult result = commentColorDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				commentColorButton.BackColor = commentColorDialog.Color;
				CheckRestartRequirement(commentColorButton.BackColor, Properties.Settings.Default.CommentColor);
			}
		}

		private void refColorButton_Click(object sender, EventArgs e)
		{
			DialogResult result = refColorDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				refColorButton.BackColor = refColorDialog.Color;
				CheckRestartRequirement(refColorButton.BackColor, Properties.Settings.Default.ReferenceColor);
			}
		}

		private void valueColorButton_Click(object sender, EventArgs e)
		{
			DialogResult result = valueColorDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				valueColorButton.BackColor = valueColorDialog.Color;
				CheckRestartRequirement(valueColorButton.BackColor, Properties.Settings.Default.ValueColor);
			}
		}

		private void headerColorButton_Click(object sender, EventArgs e)
		{
			DialogResult result = headerColorDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				headerColorButton.BackColor = headerColorDialog.Color;
				CheckRestartRequirement(headerColorButton.BackColor, Properties.Settings.Default.HeaderColor);
			}
		}

		private void newColorButton_Click(object sender, EventArgs e)
		{
			DialogResult result = newColorDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				newColorButton.BackColor = newColorDialog.Color;
				CheckRestartRequirement(newColorButton.BackColor, Properties.Settings.Default.NewCommandColor);
			}
		}

		private void oldColorButton_Click(object sender, EventArgs e)
		{
			DialogResult result = oldColorDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				oldColorButton.BackColor = oldColorDialog.Color;
				CheckRestartRequirement(oldColorButton.BackColor, Properties.Settings.Default.OldCommandColor);
			}
		}

		private void unknownColorButton_Click(object sender, EventArgs e)
		{
			DialogResult result = unknownColorDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				unknownColorButton.BackColor = unknownColorDialog.Color;
				CheckRestartRequirement(unknownColorButton.BackColor, Properties.Settings.Default.UnknownColor);
			}
		}

		private void autosaveCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			string settingValue = Properties.Settings.Default.AutosaveTime == 0 ? "None" : Properties.Settings.Default.AutosaveTime.ToString();
			CheckRestartRequirement(autosaveCombo.SelectedItem.ToString(), settingValue);
		}

		private void showSpacesCheck_CheckedChanged(object sender, EventArgs e) => CheckRestartRequirement(showSpacesCheck.Checked, Properties.Settings.Default.ShowSpaces);
		private void autocompleteCheck_CheckedChanged(object sender, EventArgs e) => CheckRestartRequirement(autocompleteCheck.Checked, Properties.Settings.Default.Autocomplete);
		private void toolTipCheck_CheckedChanged(object sender, EventArgs e) => CheckRestartRequirement(toolTipCheck.Checked, Properties.Settings.Default.ToolTips);

		private void CheckRestartRequirement(object currentState, object prevSetting)
		{
			if (currentState.ToString() != prevSetting.ToString())
			{
				_restartItemCount++;
				restartLabel.Visible = true;
			}
			else
			{
				if (_restartItemCount != 0)
				{
					_restartItemCount--;
				}

				if (_restartItemCount == 0)
				{
					restartLabel.Visible = false;
				}
			}
		}
	}
}
