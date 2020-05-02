using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using TombLib.Scripting.Rendering;

namespace TombLib.Scripting.Controls.Settings
{
	internal partial class SettingsClassicScript : UserControl
	{
		private ScriptTextEditor textBox;

		private TextEditorConfiguration _config;

		public SettingsClassicScript(TextEditorConfiguration config, List<string> monospacedFontList)
		{
			InitializeComponent();

			_config = config;

			comboBox_FontFamily.Items.AddRange(monospacedFontList.ToArray());

			textBox = new ScriptTextEditor
			{
				Text = "[Level]\nRain=ENABLED,12   ; Has error\nLayer1=128,128,>\n\t\t128,-8\nMirror=69,$2137\n[Level]",
				ShowLineNumbers = false,
				IsReadOnly = true,
				HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden,
				VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden
			};

			textBox.TextArea.Margin = new Thickness(6);

			elementHost.Child = textBox;

			UpdateControlsWithSettings();
		}

		public void UpdateControlsWithSettings()
		{
			/* General */
			numeric_FontSize.Value = (decimal)_config.ClassicScriptConfiguration.FontSize - 4; // -4 because WPF has a different font size scale
			comboBox_FontFamily.SelectedItem = _config.ClassicScriptConfiguration.FontFamily;
			numeric_UndoStackSize.Value = _config.ClassicScriptConfiguration.UndoStackSize;
			checkBox_Autocomplete.Checked = _config.ClassicScriptConfiguration.AutocompleteEnabled;
			checkBox_LiveErrors.Checked = _config.ClassicScriptConfiguration.LiveErrorUnderlining;
			checkBox_CloseBrackets.Checked = _config.ClassicScriptConfiguration.AutoCloseBrackets;
			checkBox_CloseQuotes.Checked = _config.ClassicScriptConfiguration.AutoCloseQuotes;
			checkBox_WordWrapping.Checked = _config.ClassicScriptConfiguration.WordWrapping;
			checkBox_LineNumbers.Checked = _config.ClassicScriptConfiguration.ShowLineNumbers;
			checkBox_SectionSeparators.Checked = _config.ClassicScriptConfiguration.ShowSectionSeparators;
			checkBox_VisibleSpaces.Checked = _config.ClassicScriptConfiguration.ShowVisualSpaces;
			checkBox_VisibleTabs.Checked = _config.ClassicScriptConfiguration.ShowVisualTabs;
			checkBox_ToolTips.Checked = _config.ClassicScriptConfiguration.ShowDefinitionToolTips;

			/* Syntax colors */
			button_SectionsColor.BackColor = ColorTranslator.FromHtml(_config.ClassicScriptConfiguration.Colors_Sections);
			button_ValuesColor.BackColor = ColorTranslator.FromHtml(_config.ClassicScriptConfiguration.Colors_Values);
			button_ReferencesColor.BackColor = ColorTranslator.FromHtml(_config.ClassicScriptConfiguration.Colors_References);
			button_StandardCommandsColor.BackColor = ColorTranslator.FromHtml(_config.ClassicScriptConfiguration.Colors_StandardCommands);
			button_NewCommandsColor.BackColor = ColorTranslator.FromHtml(_config.ClassicScriptConfiguration.Colors_NewCommands);
			button_CommentsColor.BackColor = ColorTranslator.FromHtml(_config.ClassicScriptConfiguration.Colors_Comments);

			/* Identation */
			checkBox_PreEqualSpace.Checked = _config.ClassicScriptConfiguration.Tidy_PreEqualSpace;
			checkBox_PostEqualSpace.Checked = _config.ClassicScriptConfiguration.Tidy_PostEqualSpace;
			checkBox_PreCommaSpace.Checked = _config.ClassicScriptConfiguration.Tidy_PreCommaSpace;
			checkBox_PostCommaSpace.Checked = _config.ClassicScriptConfiguration.Tidy_PostCommaSpace;
			checkBox_ReduceSpaces.Checked = _config.ClassicScriptConfiguration.Tidy_ReduceSpaces;
		}

		public void ResetToDefault()
		{
			/* General */
			numeric_FontSize.Value = 12;
			comboBox_FontFamily.SelectedItem = "Consolas";
			numeric_UndoStackSize.Value = 256;
			checkBox_Autocomplete.Checked = true;
			checkBox_LiveErrors.Checked = true;
			checkBox_CloseBrackets.Checked = true;
			checkBox_CloseQuotes.Checked = true;
			checkBox_WordWrapping.Checked = false;
			checkBox_LineNumbers.Checked = true;
			checkBox_SectionSeparators.Checked = true;
			checkBox_VisibleSpaces.Checked = false;
			checkBox_VisibleTabs.Checked = true;
			checkBox_ToolTips.Checked = true;

			/* Syntax colors */
			button_SectionsColor.BackColor = Color.SteelBlue;
			button_ValuesColor.BackColor = Color.LightSalmon;
			button_ReferencesColor.BackColor = Color.Orchid;
			button_StandardCommandsColor.BackColor = Color.MediumAquamarine;
			button_NewCommandsColor.BackColor = Color.SpringGreen;
			button_CommentsColor.BackColor = Color.Green;

			/* Identation */
			checkBox_PreEqualSpace.Checked = false;
			checkBox_PostEqualSpace.Checked = true;
			checkBox_PreCommaSpace.Checked = false;
			checkBox_PostCommaSpace.Checked = true;
			checkBox_ReduceSpaces.Checked = true;

			ApplySettings();

			textBox.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_config.ClassicScriptConfiguration.Colors_Values));
			textBox.SyntaxHighlighting = new ScriptSyntaxHighlighting();
		}

		private void checkBox_PreEqualSpace_CheckedChanged(object sender, EventArgs e)
		{
			textBox.Text = checkBox_PreEqualSpace.Checked ? textBox.Text.Replace("=", " =") : textBox.Text.Replace(" =", "=");
			textBox.Focus();
		}

		private void checkBox_PostEqualSpace_CheckedChanged(object sender, EventArgs e)
		{
			textBox.Text = checkBox_PostEqualSpace.Checked ? textBox.Text.Replace("=", "= ") : textBox.Text.Replace("= ", "=");
			textBox.Focus();
		}

		private void checkBox_PreCommaSpace_CheckedChanged(object sender, EventArgs e)
		{
			textBox.Text = checkBox_PreCommaSpace.Checked ? textBox.Text.Replace(",", " ,") : textBox.Text.Replace(" ,", ",");
			textBox.Focus();
		}

		private void checkBox_PostCommaSpace_CheckedChanged(object sender, EventArgs e)
		{
			textBox.Text = checkBox_PostCommaSpace.Checked ? textBox.Text.Replace(",", ", ") : textBox.Text.Replace(", ", ",");
			textBox.Focus();
		}

		private void checkBox_ReduceSpaces_CheckedChanged(object sender, EventArgs e)
		{
			textBox.Text = checkBox_ReduceSpaces.Checked ? textBox.Text.Replace("  ;", ";") : textBox.Text.Replace(";", "  ;");
			textBox.Focus();
		}

		private void button_Color_Click(object sender, EventArgs e) =>
			ChangeColor((DarkButton)sender);

		private void ChangeColor(DarkButton targetButton)
		{
			colorDialog.Color = targetButton.BackColor;

			if (colorDialog.ShowDialog(this) == DialogResult.OK)
				targetButton.BackColor = colorDialog.Color;

			ApplySettings();

			textBox.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_config.ClassicScriptConfiguration.Colors_Values));
			textBox.SyntaxHighlighting = new ScriptSyntaxHighlighting();
		}

		public void ApplySettings()
		{
			/* General */
			_config.ClassicScriptConfiguration.FontSize = (double)(numeric_FontSize.Value + 4); // +4 because WPF has a different font size scale
			_config.ClassicScriptConfiguration.FontFamily = comboBox_FontFamily.SelectedItem.ToString();
			_config.ClassicScriptConfiguration.UndoStackSize = (int)numeric_UndoStackSize.Value;
			_config.ClassicScriptConfiguration.AutocompleteEnabled = checkBox_Autocomplete.Checked;
			_config.ClassicScriptConfiguration.LiveErrorUnderlining = checkBox_LiveErrors.Checked;
			_config.ClassicScriptConfiguration.AutoCloseBrackets = checkBox_CloseBrackets.Checked;
			_config.ClassicScriptConfiguration.AutoCloseQuotes = checkBox_CloseQuotes.Checked;
			_config.ClassicScriptConfiguration.WordWrapping = checkBox_WordWrapping.Checked;
			_config.ClassicScriptConfiguration.ShowLineNumbers = checkBox_LineNumbers.Checked;
			_config.ClassicScriptConfiguration.ShowSectionSeparators = checkBox_SectionSeparators.Checked;
			_config.ClassicScriptConfiguration.ShowVisualSpaces = checkBox_VisibleSpaces.Checked;
			_config.ClassicScriptConfiguration.ShowVisualTabs = checkBox_VisibleTabs.Checked;
			_config.ClassicScriptConfiguration.ShowDefinitionToolTips = checkBox_ToolTips.Checked;

			/* Syntax colors */
			_config.ClassicScriptConfiguration.Colors_Sections = ColorTranslator.ToHtml(button_SectionsColor.BackColor);
			_config.ClassicScriptConfiguration.Colors_Values = ColorTranslator.ToHtml(button_ValuesColor.BackColor);
			_config.ClassicScriptConfiguration.Colors_References = ColorTranslator.ToHtml(button_ReferencesColor.BackColor);
			_config.ClassicScriptConfiguration.Colors_StandardCommands = ColorTranslator.ToHtml(button_StandardCommandsColor.BackColor);
			_config.ClassicScriptConfiguration.Colors_NewCommands = ColorTranslator.ToHtml(button_NewCommandsColor.BackColor);
			_config.ClassicScriptConfiguration.Colors_Comments = ColorTranslator.ToHtml(button_CommentsColor.BackColor);

			/* Identation */
			_config.ClassicScriptConfiguration.Tidy_PreEqualSpace = checkBox_PreEqualSpace.Checked;
			_config.ClassicScriptConfiguration.Tidy_PostEqualSpace = checkBox_PostEqualSpace.Checked;
			_config.ClassicScriptConfiguration.Tidy_PreCommaSpace = checkBox_PreCommaSpace.Checked;
			_config.ClassicScriptConfiguration.Tidy_PostCommaSpace = checkBox_PostCommaSpace.Checked;
			_config.ClassicScriptConfiguration.Tidy_ReduceSpaces = checkBox_ReduceSpaces.Checked;

			_config.ClassicScriptConfiguration.Save();
		}

		public void UpdatePreview()
		{
			textBox.Focus();
		}

		private void checkBox_LiveErrors_CheckedChanged(object sender, EventArgs e)
		{
			textBox.LiveErrorUnderlining = checkBox_LiveErrors.Checked;

			if (textBox.LiveErrorUnderlining)
				textBox.ManuallyCheckForErrors();
			else
				textBox.ResetErrorsOnAllLines();

			textBox.Focus();
		}

		private void checkBox_LineNumbers_CheckedChanged(object sender, EventArgs e)
		{
			textBox.ShowLineNumbers = checkBox_LineNumbers.Checked;
			textBox.Focus();
		}

		private void checkBox_SectionSeparators_CheckedChanged(object sender, EventArgs e)
		{
			textBox.ShowSectionSeparators = checkBox_SectionSeparators.Checked;
			textBox.Focus();
		}

		private void checkBox_VisibleSpaces_CheckedChanged(object sender, EventArgs e)
		{
			textBox.Options.ShowSpaces = checkBox_VisibleSpaces.Checked;
			textBox.Focus();
		}

		private void checkBox_VisibleTabs_CheckedChanged(object sender, EventArgs e)
		{
			textBox.Options.ShowTabs = checkBox_VisibleTabs.Checked;
			textBox.Focus();
		}

		private void checkBox_ToolTips_CheckedChanged(object sender, EventArgs e)
		{
			textBox.ShowDefinitionToolTips = checkBox_ToolTips.Checked;
			textBox.Focus();
		}

		private void comboBox_FontFamily_SelectedIndexChanged(object sender, EventArgs e)
		{
			textBox.FontFamily = new System.Windows.Media.FontFamily(comboBox_FontFamily.SelectedItem.ToString());
			textBox.Focus();
		}

		private void numeric_FontSize_ValueChanged(object sender, EventArgs e)
		{
			textBox.FontSize = (double)(numeric_FontSize.Value + 4); // +4 because WPF has a different font size scale
			textBox.Focus();
		}

		private void checkBox_WordWrapping_CheckedChanged(object sender, EventArgs e)
		{
			textBox.WordWrap = checkBox_WordWrapping.Checked;
			textBox.Focus();
		}
	}
}
