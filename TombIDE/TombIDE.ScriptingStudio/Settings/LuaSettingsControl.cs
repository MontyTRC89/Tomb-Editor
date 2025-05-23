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
using TombLib.Scripting.Lua;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Resources;
using TombLib.Utils;

namespace TombIDE.ScriptingStudio.Settings
{
	internal partial class LuaSettingsControl : UserControl
	{
		// TODO: Refactor !!!

		private LuaEditor editorPreview;

		#region Construction

		public LuaSettingsControl()
		{
			InitializeComponent();
		}

		public void Initialize(LuaEditorConfiguration config)
		{
			InitializePreview();

			FillFontList();
			UpdateSchemeList();
			UpdateControlsWithSettings(config);
		}

		private void InitializePreview()
		{
			editorPreview = new LuaEditor(new Version(0, 0))
			{
				Text =
					"if _G[k] then\n" +
					"	print(\"WARNING! Key \"..k..\" already exists in global environment!\")\n" +
					"else\n" +
					"	_G[k] = v\n" +
					"	if \"table\" == type(v) then\n" +
					"		if nil == v.__type then\n" +
					"			ShortenInner(v)\n" +
					"		end\n" +
					"	end\n" +
					"end",
				IsReadOnly = true,
				HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden,
				VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden
			};

			editorPreview.TextArea.Margin = new Thickness(3);

			elementHost.Child = editorPreview;
		}

		private void FillFontList()
		{
			var fontList = new List<string>();

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

			foreach (string file in Directory.GetFiles(DefaultPaths.LuaColorConfigsDirectory, "*.luasch", SearchOption.TopDirectoryOnly))
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

		private void comboBox_ColorSchemes_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBox_ColorSchemes.Items.Count == 1)
				button_DeleteScheme.Enabled = false; // Disallow deleting the last available scheme

			ToggleSaveSchemeButton();

			string fullSchemePath = Path.Combine(DefaultPaths.LuaColorConfigsDirectory, comboBox_ColorSchemes.SelectedItem.ToString() + ".luasch");
			ColorScheme selectedScheme = XmlUtils.ReadXmlFile<ColorScheme>(fullSchemePath);

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
			using (var form = new FormSaveSchemeAs(ColorSchemeType.GameFlowScript))
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					var currentScheme = new ColorScheme
					{
						Values = (HighlightingObject)colorButton_Values.Tag,
						Operators = (HighlightingObject)colorButton_Operators.Tag,
						SpecialOperators = (HighlightingObject)colorButton_SpecialOperators.Tag,
						Statements = (HighlightingObject)colorButton_Statements.Tag,
						Comments = (HighlightingObject)colorButton_Comments.Tag,
						Background = ColorTranslator.ToHtml(colorButton_Background.BackColor),
						Foreground = ColorTranslator.ToHtml(colorButton_Foreground.BackColor)
					};

					XmlUtils.WriteXmlFile(form.SchemeFilePath, currentScheme);

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
				string selectedSchemeFilePath = Path.Combine(DefaultPaths.LuaColorConfigsDirectory, comboBox_ColorSchemes.SelectedItem + ".luasch");

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
			using (var dialog = new OpenFileDialog())
			{
				dialog.Filter = "Lua Scheme|*.luasch";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					File.Copy(dialog.FileName, Path.Combine(DefaultPaths.LuaColorConfigsDirectory, Path.GetFileName(dialog.FileName)), true);
					UpdateSchemeList();

					comboBox_ColorSchemes.SelectedItem = Path.GetFileNameWithoutExtension(dialog.FileName);
				}
			}
		}

		private void button_OpenSchemesFolder_Click(object sender, EventArgs e)
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = "explorer.exe",
				Arguments = DefaultPaths.LuaColorConfigsDirectory,
				UseShellExecute = true
			};

			Process.Start(startInfo);
		}

		#endregion Events

		#region Loading

		private void UpdateControlsWithSettings(LuaEditorConfiguration config)
		{
			numeric_FontSize.Value = (decimal)config.FontSize - 4; // -4 because AvalonEdit has a different font size scale
			comboBox_FontFamily.SelectedItem = config.FontFamily;
			numeric_UndoStackSize.Value = config.UndoStackSize;

			LoadSettingsForCheckBoxes(config);

			comboBox_ColorSchemes.SelectedItem = config.SelectedColorSchemeName;
		}

		private void LoadSettingsForCheckBoxes(LuaEditorConfiguration config)
		{
			checkBox_Autocomplete.Checked = config.AutocompleteEnabled;
			checkBox_WordWrapping.Checked = config.WordWrapping;

			checkBox_CloseParentheses.Checked = config.AutoCloseParentheses;
			checkBox_CloseBrackets.Checked = config.AutoCloseBrackets;
			checkBox_CloseQuotes.Checked = config.AutoCloseQuotes;
			checkBox_CloseBraces.Checked = config.AutoCloseBraces;

			checkBox_HighlightCurrentLine.Checked = config.HighlightCurrentLine;
			checkBox_LineNumbers.Checked = config.ShowLineNumbers;

			checkBox_VisibleSpaces.Checked = config.ShowVisualSpaces;
			checkBox_VisibleTabs.Checked = config.ShowVisualTabs;
		}

		#endregion Loading

		#region Applying

		public void ApplySettings(LuaEditorConfiguration config)
		{
			config.FontSize = (double)(numeric_FontSize.Value + 4); // +4 because AvalonEdit has a different font size scale
			config.FontFamily = comboBox_FontFamily.SelectedItem.ToString();
			config.UndoStackSize = (int)numeric_UndoStackSize.Value;

			ApplySettingsFromCheckBoxes(config);

			//config.SelectedColorSchemeName = comboBox_ColorSchemes.SelectedItem.ToString();

			config.Save();
		}

		private void ApplySettingsFromCheckBoxes(LuaEditorConfiguration config)
		{
			config.AutocompleteEnabled = checkBox_Autocomplete.Checked;
			config.WordWrapping = checkBox_WordWrapping.Checked;

			config.AutoCloseParentheses = checkBox_CloseParentheses.Checked;
			config.AutoCloseBrackets = checkBox_CloseBrackets.Checked;
			config.AutoCloseQuotes = checkBox_CloseQuotes.Checked;
			config.AutoCloseBraces = checkBox_CloseBraces.Checked;

			config.HighlightCurrentLine = checkBox_HighlightCurrentLine.Checked;
			config.ShowLineNumbers = checkBox_LineNumbers.Checked;

			config.ShowVisualSpaces = checkBox_VisibleSpaces.Checked;
			config.ShowVisualTabs = checkBox_VisibleTabs.Checked;
		}

		#endregion Applying

		#region Resetting

		public void ResetToDefault()
		{
			numeric_FontSize.Value = (decimal)(TextEditorBaseDefaults.FontSize - 4); // -4 because AvalonEdit has a different font size scale
			comboBox_FontFamily.SelectedItem = TextEditorBaseDefaults.FontFamily;
			numeric_UndoStackSize.Value = TextEditorBaseDefaults.UndoStackSize;

			ResetCheckBoxSettings();
		}

		private void ResetCheckBoxSettings()
		{
			checkBox_Autocomplete.Checked = TextEditorBaseDefaults.AutocompleteEnabled;
			checkBox_WordWrapping.Checked = TextEditorBaseDefaults.WordWrapping;

			checkBox_CloseParentheses.Checked = TextEditorBaseDefaults.AutoCloseParentheses;
			checkBox_CloseBrackets.Checked = TextEditorBaseDefaults.AutoCloseBrackets;
			checkBox_CloseQuotes.Checked = TextEditorBaseDefaults.AutoCloseQuotes;
			checkBox_CloseBraces.Checked = TextEditorBaseDefaults.AutoCloseBraces;

			checkBox_HighlightCurrentLine.Checked = TextEditorBaseDefaults.HighlightCurrentLine;
			checkBox_LineNumbers.Checked = TextEditorBaseDefaults.ShowLineNumbers;

			checkBox_VisibleSpaces.Checked = TextEditorBaseDefaults.ShowVisualSpaces;
			checkBox_VisibleTabs.Checked = TextEditorBaseDefaults.ShowVisualTabs;
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
			var currentScheme = new ColorScheme
			{
				Values = (HighlightingObject)colorButton_Values.Tag,
				Operators = (HighlightingObject)colorButton_Operators.Tag,
				SpecialOperators = (HighlightingObject)colorButton_SpecialOperators.Tag,
				Statements = (HighlightingObject)colorButton_Statements.Tag,
				Comments = (HighlightingObject)colorButton_Comments.Tag,
				Background = ColorTranslator.ToHtml(colorButton_Background.BackColor),
				Foreground = ColorTranslator.ToHtml(colorButton_Foreground.BackColor)
			};

			bool itemFound = false;

			foreach (string item in comboBox_ColorSchemes.Items)
			{
				if (item == "~UNTITLED")
					continue;

				ColorScheme itemScheme = XmlUtils.ReadXmlFile<ColorScheme>(Path.Combine(DefaultPaths.LuaColorConfigsDirectory, item + ".luasch"));

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

				XmlUtils.WriteXmlFile(Path.Combine(DefaultPaths.LuaColorConfigsDirectory, "~UNTITLED.luasch"), currentScheme);

				comboBox_ColorSchemes.SelectedItem = "~UNTITLED";
			}

			UpdatePreviewColors(currentScheme);
		}

		private void UpdateColorButtons(ColorScheme scheme)
		{
			colorButton_Values.BackColor = ColorTranslator.FromHtml(scheme.Values.HtmlColor);
			colorButton_Values.Tag = scheme.Values;

			colorButton_Operators.BackColor = ColorTranslator.FromHtml(scheme.Operators.HtmlColor);
			colorButton_Operators.Tag = scheme.Operators;

			colorButton_SpecialOperators.BackColor = ColorTranslator.FromHtml(scheme.SpecialOperators.HtmlColor);
			colorButton_SpecialOperators.Tag = scheme.SpecialOperators;

			colorButton_Statements.BackColor = ColorTranslator.FromHtml(scheme.Statements.HtmlColor);
			colorButton_Statements.Tag = scheme.Statements;

			colorButton_Comments.BackColor = ColorTranslator.FromHtml(scheme.Comments.HtmlColor);
			colorButton_Comments.Tag = scheme.Comments;

			UpdateColorButtonStyleText(colorButton_Values);
			UpdateColorButtonStyleText(colorButton_Operators);
			UpdateColorButtonStyleText(colorButton_SpecialOperators);
			UpdateColorButtonStyleText(colorButton_Statements);
			UpdateColorButtonStyleText(colorButton_Comments);
			UpdateColorButtonStyleText(colorButton_Comments);

			colorButton_Background.BackColor = ColorTranslator.FromHtml(scheme.Background);
			colorButton_Foreground.BackColor = ColorTranslator.FromHtml(scheme.Foreground);
		}

		private void buttonContextMenu_Opening(object sender, CancelEventArgs e)
		{
			var sourceButton = (DarkButton)((DarkContextMenu)sender).SourceControl;
			var highlighting = (HighlightingObject)sourceButton.Tag;

			menuItem_Bold.Checked = highlighting.IsBold;
			menuItem_Italic.Checked = highlighting.IsItalic;
		}

		private void UpdateButton(object sender)
		{
			var sourceButton = (DarkButton)((DarkContextMenu)((ToolStripMenuItem)sender).GetCurrentParent()).SourceControl;
			var highlighting = (HighlightingObject)sourceButton.Tag;

			highlighting.IsBold = menuItem_Bold.Checked;
			highlighting.IsItalic = menuItem_Italic.Checked;

			UpdateColorButtonStyleText(sourceButton);

			UpdatePreview();
		}

		private void UpdateColorButtonStyleText(DarkButton colorButton)
		{
			if (colorButton.Tag == null)
				return;

			var highlighting = (HighlightingObject)colorButton.Tag;

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

		private void UpdatePreviewColors(ColorScheme scheme)
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

			editorPreview.SyntaxHighlighting = new SyntaxHighlighting(scheme);
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

			if (comboBox_FontFamily.SelectedItem != null)
				editorPreview.FontFamily = new System.Windows.Media.FontFamily(comboBox_FontFamily.SelectedItem.ToString());

			editorPreview.WordWrap = checkBox_WordWrapping.Checked;
			editorPreview.Options.HighlightCurrentLine = checkBox_HighlightCurrentLine.Checked;
			editorPreview.ShowLineNumbers = checkBox_LineNumbers.Checked;

			editorPreview.Options.ShowSpaces = checkBox_VisibleSpaces.Checked;
			editorPreview.Options.ShowTabs = checkBox_VisibleTabs.Checked;

			if (forceUpdate)
				ForcePreviewUpdate();
		}
	}
}
