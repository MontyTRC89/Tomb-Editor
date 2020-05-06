using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using TombLib.Scripting.TextEditors.SyntaxHighlighting;

namespace TombLib.Scripting.TextEditors.Controls.Settings
{
	internal partial class SettingsClassicScript : UserControl
	{
		// TODO: Refactor

		private ScriptTextEditor editorPreview;

		private TextEditorConfigurations _config;

		public SettingsClassicScript(TextEditorConfigurations config, List<string> monospacedFontList)
		{
			InitializeComponent();

			_config = config;

			comboBox_FontFamily.Items.AddRange(monospacedFontList.ToArray());

			editorPreview = new ScriptTextEditor
			{
				Text = "[Level]\nRain=ENABLED,12   ; Has error\nLayer1=128,128,>\n\t\t128,-8\nMirror=69,$2137\n[Level]",
				Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_config.ClassicScript.Colors.Values)),
				ShowLineNumbers = false,
				IsReadOnly = true,
				HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden,
				VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden
			};

			editorPreview.TextArea.Margin = new Thickness(6);

			elementHost.Child = editorPreview;

			UpdateControlsWithSettings();
		}

		public void UpdateControlsWithSettings()
		{
			/* General */
			numeric_FontSize.Value = (decimal)_config.ClassicScript.FontSize - 4; // -4 because WPF has a different font size scale
			comboBox_FontFamily.SelectedItem = _config.ClassicScript.FontFamily;
			numeric_UndoStackSize.Value = _config.ClassicScript.UndoStackSize;
			checkBox_Autocomplete.Checked = _config.ClassicScript.AutocompleteEnabled;
			checkBox_LiveErrors.Checked = _config.ClassicScript.LiveErrorUnderlining;
			checkBox_CloseBrackets.Checked = _config.ClassicScript.AutoCloseBrackets;
			checkBox_CloseQuotes.Checked = _config.ClassicScript.AutoCloseQuotes;
			checkBox_WordWrapping.Checked = _config.ClassicScript.WordWrapping;
			checkBox_LineNumbers.Checked = _config.ClassicScript.ShowLineNumbers;
			checkBox_SectionSeparators.Checked = _config.ClassicScript.ShowSectionSeparators;
			checkBox_VisibleSpaces.Checked = _config.ClassicScript.ShowVisualSpaces;
			checkBox_VisibleTabs.Checked = _config.ClassicScript.ShowVisualTabs;
			checkBox_ToolTips.Checked = _config.ClassicScript.ShowDefinitionToolTips;

			/* Syntax colors */
			button_SectionsColor.BackColor = ColorTranslator.FromHtml(_config.ClassicScript.Colors.Sections);
			button_ValuesColor.BackColor = ColorTranslator.FromHtml(_config.ClassicScript.Colors.Values);
			button_ReferencesColor.BackColor = ColorTranslator.FromHtml(_config.ClassicScript.Colors.References);
			button_StandardCommandsColor.BackColor = ColorTranslator.FromHtml(_config.ClassicScript.Colors.StandardCommands);
			button_NewCommandsColor.BackColor = ColorTranslator.FromHtml(_config.ClassicScript.Colors.NewCommands);
			button_CommentsColor.BackColor = ColorTranslator.FromHtml(_config.ClassicScript.Colors.Comments);

			/* Identation */
			checkBox_PreEqualSpace.Checked = _config.ClassicScript.Tidy_PreEqualSpace;
			checkBox_PostEqualSpace.Checked = _config.ClassicScript.Tidy_PostEqualSpace;
			checkBox_PreCommaSpace.Checked = _config.ClassicScript.Tidy_PreCommaSpace;
			checkBox_PostCommaSpace.Checked = _config.ClassicScript.Tidy_PostCommaSpace;
			checkBox_ReduceSpaces.Checked = _config.ClassicScript.Tidy_ReduceSpaces;
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

			editorPreview.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_config.ClassicScript.Colors.Values));
			editorPreview.SyntaxHighlighting = new ScriptSyntaxHighlighting();
		}

		private void checkBox_PreEqualSpace_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Text = checkBox_PreEqualSpace.Checked ? editorPreview.Text.Replace("=", " =") : editorPreview.Text.Replace(" =", "=");
			editorPreview.Focus();
		}

		private void checkBox_PostEqualSpace_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Text = checkBox_PostEqualSpace.Checked ? editorPreview.Text.Replace("=", "= ") : editorPreview.Text.Replace("= ", "=");
			editorPreview.Focus();
		}

		private void checkBox_PreCommaSpace_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Text = checkBox_PreCommaSpace.Checked ? editorPreview.Text.Replace(",", " ,") : editorPreview.Text.Replace(" ,", ",");
			editorPreview.Focus();
		}

		private void checkBox_PostCommaSpace_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Text = checkBox_PostCommaSpace.Checked ? editorPreview.Text.Replace(",", ", ") : editorPreview.Text.Replace(", ", ",");
			editorPreview.Focus();
		}

		private void checkBox_ReduceSpaces_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Text = checkBox_ReduceSpaces.Checked ? editorPreview.Text.Replace("  ;", ";") : editorPreview.Text.Replace(";", "  ;");
			editorPreview.Focus();
		}

		private void button_Color_Click(object sender, EventArgs e) =>
			ChangeColor((DarkButton)sender);

		private void ChangeColor(DarkButton targetButton)
		{
			colorDialog.Color = targetButton.BackColor;

			if (colorDialog.ShowDialog(this) == DialogResult.OK)
				targetButton.BackColor = colorDialog.Color;

			ApplySettings();

			editorPreview.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_config.ClassicScript.Colors.Values));
			editorPreview.SyntaxHighlighting = new ScriptSyntaxHighlighting();
		}

		public void ApplySettings()
		{
			/* General */
			_config.ClassicScript.FontSize = (double)(numeric_FontSize.Value + 4); // +4 because WPF has a different font size scale
			_config.ClassicScript.FontFamily = comboBox_FontFamily.SelectedItem.ToString();
			_config.ClassicScript.UndoStackSize = (int)numeric_UndoStackSize.Value;
			_config.ClassicScript.AutocompleteEnabled = checkBox_Autocomplete.Checked;
			_config.ClassicScript.LiveErrorUnderlining = checkBox_LiveErrors.Checked;
			_config.ClassicScript.AutoCloseBrackets = checkBox_CloseBrackets.Checked;
			_config.ClassicScript.AutoCloseQuotes = checkBox_CloseQuotes.Checked;
			_config.ClassicScript.WordWrapping = checkBox_WordWrapping.Checked;
			_config.ClassicScript.ShowLineNumbers = checkBox_LineNumbers.Checked;
			_config.ClassicScript.ShowSectionSeparators = checkBox_SectionSeparators.Checked;
			_config.ClassicScript.ShowVisualSpaces = checkBox_VisibleSpaces.Checked;
			_config.ClassicScript.ShowVisualTabs = checkBox_VisibleTabs.Checked;
			_config.ClassicScript.ShowDefinitionToolTips = checkBox_ToolTips.Checked;

			/* Syntax colors */
			_config.ClassicScript.Colors.Sections = ColorTranslator.ToHtml(button_SectionsColor.BackColor);
			_config.ClassicScript.Colors.Values = ColorTranslator.ToHtml(button_ValuesColor.BackColor);
			_config.ClassicScript.Colors.References = ColorTranslator.ToHtml(button_ReferencesColor.BackColor);
			_config.ClassicScript.Colors.StandardCommands = ColorTranslator.ToHtml(button_StandardCommandsColor.BackColor);
			_config.ClassicScript.Colors.NewCommands = ColorTranslator.ToHtml(button_NewCommandsColor.BackColor);
			_config.ClassicScript.Colors.Comments = ColorTranslator.ToHtml(button_CommentsColor.BackColor);

			/* Identation */
			_config.ClassicScript.Tidy_PreEqualSpace = checkBox_PreEqualSpace.Checked;
			_config.ClassicScript.Tidy_PostEqualSpace = checkBox_PostEqualSpace.Checked;
			_config.ClassicScript.Tidy_PreCommaSpace = checkBox_PreCommaSpace.Checked;
			_config.ClassicScript.Tidy_PostCommaSpace = checkBox_PostCommaSpace.Checked;
			_config.ClassicScript.Tidy_ReduceSpaces = checkBox_ReduceSpaces.Checked;

			_config.ClassicScript.Save();
		}

		public void UpdatePreview()
		{
			editorPreview.Focus();
		}

		private void checkBox_LiveErrors_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.LiveErrorUnderlining = checkBox_LiveErrors.Checked;

			if (editorPreview.LiveErrorUnderlining)
				editorPreview.ManuallyCheckForErrors();
			else
				editorPreview.ResetErrorsOnAllLines();

			editorPreview.Focus();
		}

		private void checkBox_LineNumbers_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.ShowLineNumbers = checkBox_LineNumbers.Checked;
			editorPreview.Focus();
		}

		private void checkBox_SectionSeparators_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.ShowSectionSeparators = checkBox_SectionSeparators.Checked;
			editorPreview.Focus();
		}

		private void checkBox_VisibleSpaces_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Options.ShowSpaces = checkBox_VisibleSpaces.Checked;
			editorPreview.Focus();
		}

		private void checkBox_VisibleTabs_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Options.ShowTabs = checkBox_VisibleTabs.Checked;
			editorPreview.Focus();
		}

		private void checkBox_ToolTips_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.ShowDefinitionToolTips = checkBox_ToolTips.Checked;
			editorPreview.Focus();
		}

		private void comboBox_FontFamily_SelectedIndexChanged(object sender, EventArgs e)
		{
			editorPreview.FontFamily = new System.Windows.Media.FontFamily(comboBox_FontFamily.SelectedItem.ToString());
			editorPreview.Focus();
		}

		private void numeric_FontSize_ValueChanged(object sender, EventArgs e)
		{
			editorPreview.FontSize = (double)(numeric_FontSize.Value + 4); // +4 because WPF has a different font size scale
			editorPreview.Focus();
		}

		private void checkBox_WordWrapping_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.WordWrap = checkBox_WordWrapping.Checked;
			editorPreview.Focus();
		}
	}
}
