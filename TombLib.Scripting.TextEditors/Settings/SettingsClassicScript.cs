﻿using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using TombLib.Scripting.Objects;
using TombLib.Scripting.TextEditors.ColorSchemes;
using TombLib.Scripting.TextEditors.Configs;
using TombLib.Scripting.TextEditors.Configs.Defaults;
using TombLib.Scripting.TextEditors.Forms;
using TombLib.Scripting.TextEditors.SyntaxHighlighting;

namespace TombLib.Scripting.TextEditors.Controls.Settings
{
	internal partial class SettingsClassicScript : UserControl
	{
		// TODO: Refactor !!!

		private ScriptTextEditor editorPreview;

		#region Construction

		public SettingsClassicScript()
		{
			InitializeComponent();
		}

		public void Initialize(ClassicScriptEditorConfiguration config)
		{
			InitializePreview();

			FillFontList();
			UpdateSchemeList();
			UpdateControlsWithSettings(config);
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

			editorPreview.TextArea.Margin = new Thickness(3);

			elementHost.Child = editorPreview;
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

		#endregion Construction

		#region Events

		private void VisiblePreviewSetting_Changed(object sender, EventArgs e) =>
			UpdatePreviewTemp();

		private void comboBox_FontFamily_SelectedIndexChanged(object sender, EventArgs e) =>
			UpdatePreviewTemp(false);

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
				button_DeleteScheme.Enabled = false; // Disallow deleting the last available scheme

			ToggleSaveSchemeButton();

			string fullSchemePath = Path.Combine(DefaultPaths.GetClassicScriptColorConfigsPath(), comboBox_ColorSchemes.SelectedItem.ToString() + ".cssch");
			ClassicScriptColorScheme selectedScheme = XmlHandling.ReadXmlFile<ClassicScriptColorScheme>(fullSchemePath);

			UpdateColorButtons(selectedScheme);
			UpdatePreviewColors(selectedScheme);
		}

		private void button_Color_Click(object sender, EventArgs e) =>
			ChangeColor((DarkButton)sender);

		private void menuItem_Bold_Click(object sender, EventArgs e)
		{
			menuItem_Bold.Checked = !menuItem_Bold.Checked;
			UpdateButton(sender);
		}

		private void menuItem_Italic_Click(object sender, EventArgs e)
		{
			menuItem_Italic.Checked = !menuItem_Italic.Checked;
			UpdateButton(sender);
		}

		private void button_SaveScheme_Click(object sender, EventArgs e)
		{
			using (FormSaveSchemeAs form = new FormSaveSchemeAs(ColorSchemeType.ClassicScript))
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					ClassicScriptColorScheme currentScheme = new ClassicScriptColorScheme
					{
						Sections = (HighlightingObject)colorButton_Sections.Tag,
						Values = (HighlightingObject)colorButton_Values.Tag,
						References = (HighlightingObject)colorButton_References.Tag,
						StandardCommands = (HighlightingObject)colorButton_StandardCommands.Tag,
						NewCommands = (HighlightingObject)colorButton_NewCommands.Tag,
						Comments = (HighlightingObject)colorButton_Comments.Tag,
						Background = ColorTranslator.ToHtml(colorButton_Background.BackColor),
						Foreground = ColorTranslator.ToHtml(colorButton_Foreground.BackColor)
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
				"Are you sure you want to delete the \"" + comboBox_ColorSchemes.SelectedItem + "\" color scheme?", "Are you sure?",
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

		#region Loading

		private void UpdateControlsWithSettings(ClassicScriptEditorConfiguration config)
		{
			numeric_FontSize.Value = (decimal)config.FontSize - 4; // -4 because AvalonEdit has a different font size scale
			comboBox_FontFamily.SelectedItem = config.FontFamily;
			numeric_UndoStackSize.Value = config.UndoStackSize;

			LoadSettingsForCheckBoxes(config);
			LoadSettingsForIdentationRules(config);

			comboBox_ColorSchemes.SelectedItem = config.SelectedColorSchemeName;
		}

		private void LoadSettingsForCheckBoxes(ClassicScriptEditorConfiguration config)
		{
			checkBox_Autocomplete.Checked = config.AutocompleteEnabled;
			checkBox_LiveErrors.Checked = config.LiveErrorUnderlining;

			checkBox_CloseBrackets.Checked = config.AutoCloseBrackets;
			checkBox_CloseQuotes.Checked = config.AutoCloseQuotes;

			checkBox_WordWrapping.Checked = config.WordWrapping;

			checkBox_HighlightCurrentLine.Checked = config.HighlightCurrentLine;

			checkBox_LineNumbers.Checked = config.ShowLineNumbers;
			checkBox_SectionSeparators.Checked = config.ShowSectionSeparators;

			checkBox_VisibleSpaces.Checked = config.ShowVisualSpaces;
			checkBox_VisibleTabs.Checked = config.ShowVisualTabs;

			checkBox_ToolTips.Checked = config.ShowDefinitionToolTips;
		}

		private void LoadSettingsForIdentationRules(ClassicScriptEditorConfiguration config)
		{
			checkBox_PreEqualSpace.Checked = config.Tidy_PreEqualSpace;
			checkBox_PostEqualSpace.Checked = config.Tidy_PostEqualSpace;

			checkBox_PreCommaSpace.Checked = config.Tidy_PreCommaSpace;
			checkBox_PostCommaSpace.Checked = config.Tidy_PostCommaSpace;

			checkBox_ReduceSpaces.Checked = config.Tidy_ReduceSpaces;
		}

		#endregion Loading

		#region Applying

		public void ApplySettings(ClassicScriptEditorConfiguration config)
		{
			config.FontSize = (double)(numeric_FontSize.Value + 4); // +4 because AvalonEdit has a different font size scale
			config.FontFamily = comboBox_FontFamily.SelectedItem.ToString();
			config.UndoStackSize = (int)numeric_UndoStackSize.Value;

			ApplySettingsFromCheckBoxes(config);
			ApplyIdentationRulesSettings(config);

			config.SelectedColorSchemeName = comboBox_ColorSchemes.SelectedItem.ToString();

			config.Save();
		}

		private void ApplySettingsFromCheckBoxes(ClassicScriptEditorConfiguration config)
		{
			config.AutocompleteEnabled = checkBox_Autocomplete.Checked;
			config.LiveErrorUnderlining = checkBox_LiveErrors.Checked;

			config.AutoCloseBrackets = checkBox_CloseBrackets.Checked;
			config.AutoCloseQuotes = checkBox_CloseQuotes.Checked;

			config.WordWrapping = checkBox_WordWrapping.Checked;

			config.HighlightCurrentLine = checkBox_HighlightCurrentLine.Checked;

			config.ShowLineNumbers = checkBox_LineNumbers.Checked;
			config.ShowSectionSeparators = checkBox_SectionSeparators.Checked;

			config.ShowVisualSpaces = checkBox_VisibleSpaces.Checked;
			config.ShowVisualTabs = checkBox_VisibleTabs.Checked;

			config.ShowDefinitionToolTips = checkBox_ToolTips.Checked;
		}

		private void ApplyIdentationRulesSettings(ClassicScriptEditorConfiguration config)
		{
			config.Tidy_PreEqualSpace = checkBox_PreEqualSpace.Checked;
			config.Tidy_PostEqualSpace = checkBox_PostEqualSpace.Checked;

			config.Tidy_PreCommaSpace = checkBox_PreCommaSpace.Checked;
			config.Tidy_PostCommaSpace = checkBox_PostCommaSpace.Checked;

			config.Tidy_ReduceSpaces = checkBox_ReduceSpaces.Checked;
		}

		#endregion Applying

		#region Resetting

		public void ResetToDefault()
		{
			numeric_FontSize.Value = (decimal)(TextEditorBaseDefaults.FontSize - 4); // -4 because AvalonEdit has a different font size scale
			comboBox_FontFamily.SelectedItem = TextEditorBaseDefaults.FontFamily;
			numeric_UndoStackSize.Value = TextEditorBaseDefaults.UndoStackSize;

			ResetCheckBoxSettings();
			ResetIdentationRules();
		}

		private void ResetCheckBoxSettings()
		{
			checkBox_Autocomplete.Checked = TextEditorBaseDefaults.AutocompleteEnabled;
			checkBox_LiveErrors.Checked = TextEditorBaseDefaults.LiveErrorUnderlining;

			checkBox_CloseBrackets.Checked = TextEditorBaseDefaults.AutoCloseBrackets;
			checkBox_CloseQuotes.Checked = TextEditorBaseDefaults.AutoCloseQuotes;

			checkBox_WordWrapping.Checked = TextEditorBaseDefaults.WordWrapping;

			checkBox_LineNumbers.Checked = TextEditorBaseDefaults.ShowLineNumbers;
			checkBox_SectionSeparators.Checked = ClassicScriptEditorDefaults.ShowSectionSeparators;

			checkBox_VisibleSpaces.Checked = TextEditorBaseDefaults.ShowVisualSpaces;
			checkBox_VisibleTabs.Checked = TextEditorBaseDefaults.ShowVisualTabs;

			checkBox_ToolTips.Checked = TextEditorBaseDefaults.ShowDefinitionToolTips;
		}

		private void ResetIdentationRules()
		{
			checkBox_PreEqualSpace.Checked = ClassicScriptEditorDefaults.Tidy_PreEqualSpace;
			checkBox_PostEqualSpace.Checked = ClassicScriptEditorDefaults.Tidy_PostEqualSpace;

			checkBox_PreCommaSpace.Checked = ClassicScriptEditorDefaults.Tidy_PreCommaSpace;
			checkBox_PostCommaSpace.Checked = ClassicScriptEditorDefaults.Tidy_PostCommaSpace;

			checkBox_ReduceSpaces.Checked = ClassicScriptEditorDefaults.Tidy_ReduceSpaces;
		}

		#endregion Resetting

		public void ForcePreviewUpdate() =>
			editorPreview.Focus();

		private void ChangeColor(DarkButton targetButton)
		{
			colorDialog.Color = targetButton.BackColor;

			if (colorDialog.ShowDialog(this) == DialogResult.OK)
			{
				targetButton.BackColor = colorDialog.Color;

				if (targetButton.Tag != null)
					((HighlightingObject)targetButton.Tag).HtmlColor = ColorTranslator.ToHtml(colorDialog.Color);

				UpdatePreview();

				UpdateColorButtonStyleText(targetButton);
			}
		}

		private void UpdatePreview()
		{
			ClassicScriptColorScheme currentScheme = new ClassicScriptColorScheme
			{
				Sections = (HighlightingObject)colorButton_Sections.Tag,
				Values = (HighlightingObject)colorButton_Values.Tag,
				References = (HighlightingObject)colorButton_References.Tag,
				StandardCommands = (HighlightingObject)colorButton_StandardCommands.Tag,
				NewCommands = (HighlightingObject)colorButton_NewCommands.Tag,
				Comments = (HighlightingObject)colorButton_Comments.Tag,
				Background = ColorTranslator.ToHtml(colorButton_Background.BackColor),
				Foreground = ColorTranslator.ToHtml(colorButton_Foreground.BackColor)
			};

			bool itemFound = false;

			foreach (string item in comboBox_ColorSchemes.Items)
			{
				if (item == "~UNTITLED")
					continue;

				ClassicScriptColorScheme itemScheme = XmlHandling.ReadXmlFile<ClassicScriptColorScheme>(Path.Combine(DefaultPaths.GetClassicScriptColorConfigsPath(), item + ".cssch"));

				if (currentScheme == itemScheme)
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

			UpdatePreviewColors(currentScheme);
		}

		private void UpdateColorButtons(ClassicScriptColorScheme scheme)
		{
			colorButton_Sections.BackColor = ColorTranslator.FromHtml(scheme.Sections.HtmlColor);
			colorButton_Sections.Tag = scheme.Sections;

			colorButton_Values.BackColor = ColorTranslator.FromHtml(scheme.Values.HtmlColor);
			colorButton_Values.Tag = scheme.Values;

			colorButton_References.BackColor = ColorTranslator.FromHtml(scheme.References.HtmlColor);
			colorButton_References.Tag = scheme.References;

			colorButton_StandardCommands.BackColor = ColorTranslator.FromHtml(scheme.StandardCommands.HtmlColor);
			colorButton_StandardCommands.Tag = scheme.StandardCommands;

			colorButton_NewCommands.BackColor = ColorTranslator.FromHtml(scheme.NewCommands.HtmlColor);
			colorButton_NewCommands.Tag = scheme.NewCommands;

			colorButton_Comments.BackColor = ColorTranslator.FromHtml(scheme.Comments.HtmlColor);
			colorButton_Comments.Tag = scheme.Comments;

			UpdateColorButtonStyleText(colorButton_Sections);
			UpdateColorButtonStyleText(colorButton_Values);
			UpdateColorButtonStyleText(colorButton_References);
			UpdateColorButtonStyleText(colorButton_StandardCommands);
			UpdateColorButtonStyleText(colorButton_NewCommands);
			UpdateColorButtonStyleText(colorButton_Comments);

			colorButton_Background.BackColor = ColorTranslator.FromHtml(scheme.Background);
			colorButton_Foreground.BackColor = ColorTranslator.FromHtml(scheme.Foreground);
		}

		private void buttonContextMenu_Opening(object sender, CancelEventArgs e)
		{
			DarkButton sourceButton = (DarkButton)((DarkContextMenu)sender).SourceControl;
			HighlightingObject highlighting = (HighlightingObject)sourceButton.Tag;

			menuItem_Bold.Checked = highlighting.IsBold;
			menuItem_Italic.Checked = highlighting.IsItalic;
		}

		private void UpdateButton(object sender)
		{
			DarkButton sourceButton = (DarkButton)((DarkContextMenu)((ToolStripMenuItem)sender).GetCurrentParent()).SourceControl;
			HighlightingObject highlighting = (HighlightingObject)sourceButton.Tag;

			highlighting.IsBold = menuItem_Bold.Checked;
			highlighting.IsItalic = menuItem_Italic.Checked;

			UpdateColorButtonStyleText(sourceButton);

			UpdatePreview();
		}

		private void UpdateColorButtonStyleText(DarkButton colorButton)
		{
			HighlightingObject highlighting = (HighlightingObject)colorButton.Tag;

			if (highlighting.IsBold && highlighting.IsItalic)
				colorButton.Text = "Style: Bold & Italic";
			else if (highlighting.IsBold)
				colorButton.Text = "Style: Bold";
			else if (highlighting.IsItalic)
				colorButton.Text = "Style: Italic";
			else
				colorButton.Text = "Style: Normal";

			if (colorButton.BackColor.R + (colorButton.BackColor.G * 1.25) + colorButton.BackColor.B > 384) // Green is a much lighter color
				colorButton.ForeColor = Color.Black;
			else
				colorButton.ForeColor = Color.White;
		}

		private void UpdatePreviewColors(ClassicScriptColorScheme scheme)
		{
			editorPreview.Background = new System.Windows.Media.SolidColorBrush
			(
				(System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString
				(
					scheme.Background
				)
			);

			editorPreview.Foreground = new System.Windows.Media.SolidColorBrush
			(
				(System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString
				(
					scheme.Foreground
				)
			);

			editorPreview.SyntaxHighlighting = new ClassicScriptSyntaxHighlighting(scheme);
		}

		private void ToggleSaveSchemeButton()
		{
			bool isUntitled = comboBox_ColorSchemes.SelectedItem.ToString().Equals("~UNTITLED", StringComparison.OrdinalIgnoreCase);

			button_SaveScheme.Enabled = isUntitled;
			button_SaveScheme.Visible = isUntitled;

			comboBox_ColorSchemes.Width = isUntitled ? 395 : 426;
		}

		private void UpdatePreviewTemp(bool forceUpdate = true)
		{
			editorPreview.FontSize = (double)(numeric_FontSize.Value + 4); // +4 because AvalonEdit has a different font size scale
			editorPreview.FontFamily = new System.Windows.Media.FontFamily(comboBox_FontFamily.SelectedItem.ToString());

			editorPreview.LiveErrorUnderlining = checkBox_LiveErrors.Checked;

			if (editorPreview.LiveErrorUnderlining)
				editorPreview.ManuallyCheckForErrors();
			else
				editorPreview.ResetErrorsOnAllLines();

			editorPreview.WordWrap = checkBox_WordWrapping.Checked;

			editorPreview.Options.HighlightCurrentLine = checkBox_HighlightCurrentLine.Checked;

			editorPreview.ShowLineNumbers = checkBox_LineNumbers.Checked;
			editorPreview.ShowSectionSeparators = checkBox_SectionSeparators.Checked;

			editorPreview.Options.ShowSpaces = checkBox_VisibleSpaces.Checked;
			editorPreview.Options.ShowTabs = checkBox_VisibleTabs.Checked;

			editorPreview.ShowDefinitionToolTips = checkBox_ToolTips.Checked;

			if (forceUpdate)
				ForcePreviewUpdate();
		}
	}
}
