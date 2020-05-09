using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using TombLib.Scripting.TextEditors.ColorSchemes;
using TombLib.Scripting.TextEditors.Configs;
using TombLib.Scripting.TextEditors.Forms;
using TombLib.Scripting.TextEditors.SyntaxHighlighting;

namespace TombLib.Scripting.TextEditors.Controls.Settings
{
	internal partial class SettingsClassicScript : UserControl
	{
		// TODO: Refactor !!!

		private ScriptTextEditor editorPreview;

		private ClassicScriptEditorConfiguration _config;

		#region Construction and public methods

		public SettingsClassicScript(ClassicScriptEditorConfiguration config)
		{
			InitializeComponent();

			_config = config;

			InitializePreview();

			FillFontList();
			UpdateSchemeList();
			UpdateControlsWithSettings();
		}

		private void FillFontList()
		{
			List<string> fontList = new List<string>();

			foreach (FontFamily font in new InstalledFontCollection().Families)
				fontList.Add(font.Name);

			comboBox_FontFamily.Items.AddRange(fontList.ToArray());
		}

		private void UpdateSchemeList()
		{
			string cachedSelectedItem = null;

			if (comboBox_ColorSchemes.SelectedItem != null)
				cachedSelectedItem = comboBox_ColorSchemes.SelectedItem.ToString();

			comboBox_ColorSchemes.Items.Clear();

			foreach (string file in Directory.GetFiles(DefaultPaths.GetClassicScriptColorConfigsPath(), "*.cssch", SearchOption.TopDirectoryOnly))
				comboBox_ColorSchemes.Items.Add(Path.GetFileNameWithoutExtension(file));

			if (cachedSelectedItem != null)
				comboBox_ColorSchemes.SelectedItem = cachedSelectedItem;
		}

		private void UpdateControlsWithSettings()
		{
			/* General */
			numeric_FontSize.Value = (decimal)_config.FontSize - 4; // -4 because AvalonEdit has a different font size scale
			comboBox_FontFamily.SelectedItem = _config.FontFamily;
			numeric_UndoStackSize.Value = _config.UndoStackSize;
			checkBox_Autocomplete.Checked = _config.AutocompleteEnabled;
			checkBox_LiveErrors.Checked = _config.LiveErrorUnderlining;
			checkBox_CloseBrackets.Checked = _config.AutoCloseBrackets;
			checkBox_CloseQuotes.Checked = _config.AutoCloseQuotes;
			checkBox_WordWrapping.Checked = _config.WordWrapping;
			checkBox_LineNumbers.Checked = _config.ShowLineNumbers;
			checkBox_SectionSeparators.Checked = _config.ShowSectionSeparators;
			checkBox_VisibleSpaces.Checked = _config.ShowVisualSpaces;
			checkBox_VisibleTabs.Checked = _config.ShowVisualTabs;
			checkBox_ToolTips.Checked = _config.ShowDefinitionToolTips;

			comboBox_ColorSchemes.SelectedItem = _config.SelectedColorSchemeName;

			/* Identation */
			checkBox_PreEqualSpace.Checked = _config.Tidy_PreEqualSpace;
			checkBox_PostEqualSpace.Checked = _config.Tidy_PostEqualSpace;
			checkBox_PreCommaSpace.Checked = _config.Tidy_PreCommaSpace;
			checkBox_PostCommaSpace.Checked = _config.Tidy_PostCommaSpace;
			checkBox_ReduceSpaces.Checked = _config.Tidy_ReduceSpaces;
		}

		private void InitializePreview()
		{
			editorPreview = new ScriptTextEditor
			{
				Text = "[Level]\nRain=ENABLED,12   ; Has error\nLayer1=128,128,>\n\t\t128,-8\nMirror=69,$2137\n[Level]",
				IsReadOnly = true,
				HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden,
				VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden
			};

			editorPreview.TextArea.Margin = new Thickness(6);

			elementHost.Child = editorPreview;
		}

		public void ForcePreviewUpdate() =>
			editorPreview.Focus();

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

			/* Identation */
			checkBox_PreEqualSpace.Checked = false;
			checkBox_PostEqualSpace.Checked = true;
			checkBox_PreCommaSpace.Checked = false;
			checkBox_PostCommaSpace.Checked = true;
			checkBox_ReduceSpaces.Checked = true;

			ApplySettings();
		}

		#endregion Construction and public methods

		#region Events

		public void ApplySettings()
		{
			/* General */
			_config.FontSize = (double)(numeric_FontSize.Value + 4); // +4 because AvalonEdit has a different font size scale
			_config.FontFamily = comboBox_FontFamily.SelectedItem.ToString();
			_config.UndoStackSize = (int)numeric_UndoStackSize.Value;
			_config.AutocompleteEnabled = checkBox_Autocomplete.Checked;
			_config.LiveErrorUnderlining = checkBox_LiveErrors.Checked;
			_config.AutoCloseBrackets = checkBox_CloseBrackets.Checked;
			_config.AutoCloseQuotes = checkBox_CloseQuotes.Checked;
			_config.WordWrapping = checkBox_WordWrapping.Checked;
			_config.ShowLineNumbers = checkBox_LineNumbers.Checked;
			_config.ShowSectionSeparators = checkBox_SectionSeparators.Checked;
			_config.ShowVisualSpaces = checkBox_VisibleSpaces.Checked;
			_config.ShowVisualTabs = checkBox_VisibleTabs.Checked;
			_config.ShowDefinitionToolTips = checkBox_ToolTips.Checked;

			_config.SelectedColorSchemeName = comboBox_ColorSchemes.SelectedItem.ToString();

			/* Identation */
			_config.Tidy_PreEqualSpace = checkBox_PreEqualSpace.Checked;
			_config.Tidy_PostEqualSpace = checkBox_PostEqualSpace.Checked;
			_config.Tidy_PreCommaSpace = checkBox_PreCommaSpace.Checked;
			_config.Tidy_PostCommaSpace = checkBox_PostCommaSpace.Checked;
			_config.Tidy_ReduceSpaces = checkBox_ReduceSpaces.Checked;

			_config.Save();
		}

		private void numeric_FontSize_ValueChanged(object sender, EventArgs e)
		{
			editorPreview.FontSize = (double)(numeric_FontSize.Value + 4); // +4 because AvalonEdit has a different font size scale
			ForcePreviewUpdate();
		}

		private void comboBox_FontFamily_SelectedIndexChanged(object sender, EventArgs e)
		{
			editorPreview.FontFamily = new System.Windows.Media.FontFamily(comboBox_FontFamily.SelectedItem.ToString());
			ForcePreviewUpdate();
		}

		private void checkBox_LiveErrors_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.LiveErrorUnderlining = checkBox_LiveErrors.Checked;

			if (editorPreview.LiveErrorUnderlining)
				editorPreview.ManuallyCheckForErrors();
			else
				editorPreview.ResetErrorsOnAllLines();

			ForcePreviewUpdate();
		}

		private void checkBox_WordWrapping_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.WordWrap = checkBox_WordWrapping.Checked;
			ForcePreviewUpdate();
		}

		private void checkBox_LineNumbers_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.ShowLineNumbers = checkBox_LineNumbers.Checked;
			ForcePreviewUpdate();
		}

		private void checkBox_SectionSeparators_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.ShowSectionSeparators = checkBox_SectionSeparators.Checked;
			ForcePreviewUpdate();
		}

		private void checkBox_VisibleSpaces_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Options.ShowSpaces = checkBox_VisibleSpaces.Checked;
			ForcePreviewUpdate();
		}

		private void checkBox_VisibleTabs_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Options.ShowTabs = checkBox_VisibleTabs.Checked;
			ForcePreviewUpdate();
		}

		private void checkBox_ToolTips_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.ShowDefinitionToolTips = checkBox_ToolTips.Checked;
			ForcePreviewUpdate();
		}

		private void checkBox_PreEqualSpace_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Text = checkBox_PreEqualSpace.Checked ? editorPreview.Text.Replace("=", " =") : editorPreview.Text.Replace(" =", "=");
			ForcePreviewUpdate();
		}

		private void checkBox_PostEqualSpace_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Text = checkBox_PostEqualSpace.Checked ? editorPreview.Text.Replace("=", "= ") : editorPreview.Text.Replace("= ", "=");
			ForcePreviewUpdate();
		}

		private void checkBox_PreCommaSpace_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Text = checkBox_PreCommaSpace.Checked ? editorPreview.Text.Replace(",", " ,") : editorPreview.Text.Replace(" ,", ",");
			ForcePreviewUpdate();
		}

		private void checkBox_PostCommaSpace_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Text = checkBox_PostCommaSpace.Checked ? editorPreview.Text.Replace(",", ", ") : editorPreview.Text.Replace(", ", ",");
			ForcePreviewUpdate();
		}

		private void checkBox_ReduceSpaces_CheckedChanged(object sender, EventArgs e)
		{
			editorPreview.Text = checkBox_ReduceSpaces.Checked ? editorPreview.Text.Replace("  ;", ";") : editorPreview.Text.Replace(";", "  ;");
			ForcePreviewUpdate();
		}

		private void comboBox_ColorSchemes_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBox_ColorSchemes.Items.Count == 1)
				button_DeleteScheme.Enabled = false;

			if (comboBox_ColorSchemes.SelectedItem.ToString() == "~UNTITLED")
			{
				button_SaveScheme.Enabled = true;
				button_SaveScheme.Visible = true;

				comboBox_ColorSchemes.Width = 395;
			}
			else
			{
				button_SaveScheme.Enabled = false;
				button_SaveScheme.Visible = false;

				comboBox_ColorSchemes.Width = 426;
			}

			_config.SelectedColorSchemeName = comboBox_ColorSchemes.SelectedItem.ToString();
			_config.Save();

			SetButtonColors();

			editorPreview.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_config.ColorScheme.Background));
			editorPreview.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_config.ColorScheme.Values));

			editorPreview.SyntaxHighlighting = new ScriptSyntaxHighlighting();
		}

		private void button_Color_Click(object sender, EventArgs e) =>
			ChangeColor((DarkButton)sender);

		private void ChangeColor(DarkButton targetButton)
		{
			colorDialog.Color = targetButton.BackColor;

			if (colorDialog.ShowDialog(this) == DialogResult.OK)
			{
				targetButton.BackColor = colorDialog.Color;

				ClassicScriptColorScheme currentScheme = new ClassicScriptColorScheme
				{
					Sections = ColorTranslator.ToHtml(button_SectionsColor.BackColor),
					Values = ColorTranslator.ToHtml(button_ValuesColor.BackColor),
					References = ColorTranslator.ToHtml(button_ReferencesColor.BackColor),
					StandardCommands = ColorTranslator.ToHtml(button_StandardCommandsColor.BackColor),
					NewCommands = ColorTranslator.ToHtml(button_NewCommandsColor.BackColor),
					Comments = ColorTranslator.ToHtml(button_CommentsColor.BackColor),
					Background = ColorTranslator.ToHtml(button_BackgroundColor.BackColor),
					Foreground = ColorTranslator.ToHtml(button_TextColor.BackColor)
				};

				bool itemFound = false;

				foreach (string item in comboBox_ColorSchemes.Items)
				{
					if (item == "~UNTITLED")
						continue;

					ClassicScriptColorScheme itemScheme = XmlHandling.ReadXmlFile<ClassicScriptColorScheme>(Path.Combine(DefaultPaths.GetClassicScriptColorConfigsPath(), item + ".cssch"));

					if (currentScheme.Equals(itemScheme))
					{
						comboBox_ColorSchemes.SelectedItem = item;
						itemFound = true;
						break;
					}
				}

				if (!itemFound)
				{
					if (!comboBox_ColorSchemes.Items.Contains("~UNTITLED"))
						comboBox_ColorSchemes.Items.Add("~UNTITLED");

					XmlHandling.SaveXmlFile(Path.Combine(DefaultPaths.GetClassicScriptColorConfigsPath(), "~UNTITLED.cssch"), currentScheme);

					comboBox_ColorSchemes.SelectedItem = "~UNTITLED";
				}

				ApplySettings();

				editorPreview.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_config.ColorScheme.Background));
				editorPreview.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_config.ColorScheme.Values));

				editorPreview.SyntaxHighlighting = new ScriptSyntaxHighlighting();
			}
		}

		private void button_SaveScheme_Click(object sender, EventArgs e)
		{
			using (FormSaveSchemeAs form = new FormSaveSchemeAs(ColorSchemeType.ClassicScript))
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					ClassicScriptColorScheme currentScheme = new ClassicScriptColorScheme
					{
						Sections = ColorTranslator.ToHtml(button_SectionsColor.BackColor),
						Values = ColorTranslator.ToHtml(button_ValuesColor.BackColor),
						References = ColorTranslator.ToHtml(button_ReferencesColor.BackColor),
						StandardCommands = ColorTranslator.ToHtml(button_StandardCommandsColor.BackColor),
						NewCommands = ColorTranslator.ToHtml(button_NewCommandsColor.BackColor),
						Comments = ColorTranslator.ToHtml(button_CommentsColor.BackColor),
						Background = ColorTranslator.ToHtml(button_BackgroundColor.BackColor),
						Foreground = ColorTranslator.ToHtml(button_TextColor.BackColor)
					};

					XmlHandling.SaveXmlFile(form.SchemeFilePath, currentScheme);

					comboBox_ColorSchemes.Items.Add(Path.GetFileNameWithoutExtension(form.SchemeFilePath));
					comboBox_ColorSchemes.SelectedItem = Path.GetFileNameWithoutExtension(form.SchemeFilePath);

					comboBox_ColorSchemes.Items.Remove("~UNTITLED");
				}
		}

		private void button_DeleteScheme_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this,
				"Are you sure you want to delete the " + comboBox_ColorSchemes.SelectedItem + " color scheme?", "Are you sure?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				string selectedSchemeFilePath = Path.Combine(DefaultPaths.GetClassicScriptColorConfigsPath(), comboBox_ColorSchemes.SelectedItem + ".cssch");

				if (File.Exists(selectedSchemeFilePath))
				{
					Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(selectedSchemeFilePath,
						Microsoft.VisualBasic.FileIO.UIOption.AllDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

					comboBox_ColorSchemes.Items.Remove(comboBox_ColorSchemes.SelectedItem);
					comboBox_ColorSchemes.SelectedIndex = 0;
				}
			}
		}

		private void button_ImportScheme_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Filter = "Classic Script Scheme|*.cssch";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					File.Copy(dialog.FileName, Path.Combine(DefaultPaths.GetClassicScriptColorConfigsPath(), Path.GetFileName(dialog.FileName)), true);
					UpdateSchemeList();

					comboBox_ColorSchemes.SelectedItem = Path.GetFileNameWithoutExtension(dialog.FileName);
				}
			}
		}

		private void button_OpenSchemesFolder_Click(object sender, EventArgs e)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = "explorer.exe",
				Arguments = DefaultPaths.GetClassicScriptColorConfigsPath()
			};

			Process.Start(startInfo);
		}

		#endregion Events

		private void SetButtonColors()
		{
			button_SectionsColor.BackColor = ColorTranslator.FromHtml(_config.ColorScheme.Sections);
			button_ValuesColor.BackColor = ColorTranslator.FromHtml(_config.ColorScheme.Values);
			button_ReferencesColor.BackColor = ColorTranslator.FromHtml(_config.ColorScheme.References);
			button_StandardCommandsColor.BackColor = ColorTranslator.FromHtml(_config.ColorScheme.StandardCommands);
			button_NewCommandsColor.BackColor = ColorTranslator.FromHtml(_config.ColorScheme.NewCommands);
			button_CommentsColor.BackColor = ColorTranslator.FromHtml(_config.ColorScheme.Comments);
			button_BackgroundColor.BackColor = ColorTranslator.FromHtml(_config.ColorScheme.Background);
			button_TextColor.BackColor = ColorTranslator.FromHtml(_config.ColorScheme.Foreground);
		}
	}
}
