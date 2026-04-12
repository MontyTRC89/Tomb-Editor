using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows;
using System.Windows.Forms;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Resources;

namespace TombIDE.ScriptingStudio.Settings
{
	internal partial class LuaSettingsControl : UserControl
	{
		private const string PreviewText =
			"---@class Weapon\n"
			+ "local Weapon = {}\n"
			+ "global levelName = \"Lara\"\n\n"
			+ "function Weapon:new(name)\n"
			+ "    local damage = math.max(levelName and 1 or 0, 1)\n"
			+ "    self.name = name\n"
			+ "    return damage\n"
			+ "end";

		private static readonly string[] PreviewLines = PreviewText.Replace("\r", string.Empty).Split('\n');
		private static readonly IReadOnlyList<LuaSemanticToken> PreviewTokens = CreatePreviewTokens();

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
			ConfigureThemePresetUi();
			UpdateThemeList();
			UpdateControlsWithSettings(config);
			UpdatePreview();
		}

		private void InitializePreview()
		{
			editorPreview = new LuaEditor(new Version(0, 0))
			{
				Text = PreviewText,
				IsReadOnly = true,
				HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden,
				VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Hidden
			};

			editorPreview.TextArea.Margin = new Thickness(3);
			elementHost.Child = editorPreview;
		}

		private void ConfigureThemePresetUi()
		{
			int previewBottom = groupBox_Preview.Bottom;

			groupBox_Colors.Enabled = true;
			groupBox_Colors.Text = "Theme";
			darkLabel4.Text = "Preset:";
			darkLabel4.Location = new System.Drawing.Point(12, 23);
			comboBox_ColorSchemes.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox_ColorSchemes.Location = new System.Drawing.Point(12, 42);
			comboBox_ColorSchemes.Width = groupBox_Colors.ClientSize.Width - 24;
			groupBox_Colors.Height = 77;

			Control[] hiddenControls =
			{
				button_ImportScheme,
				button_SaveScheme,
				button_DeleteScheme,
				button_OpenSchemesFolder,
				colorButton_Background,
				colorButton_Foreground,
				colorButton_Comments,
				colorButton_SpecialOperators,
				colorButton_Values,
				colorButton_Statements,
				colorButton_Operators,
				darkLabel5,
				darkLabel6,
				darkLabel7,
				darkLabel8,
				darkLabel10,
				darkLabel11,
				darkLabel12
			};

			for (int i = 0; i < hiddenControls.Length; i++)
				hiddenControls[i].Visible = false;

			groupBox_Preview.Top = groupBox_Colors.Bottom + 9;
			groupBox_Preview.Height = previewBottom - groupBox_Preview.Top;
		}

		private void FillFontList()
		{
			var fontList = new List<string>();

			foreach (FontFamily font in new InstalledFontCollection().Families)
				fontList.Add(font.Name);

			comboBox_FontFamily.Items.AddRange(fontList.ToArray());
		}

		private void UpdateThemeList()
		{
			string cachedSelectedItem = comboBox_ColorSchemes.SelectedItem?.ToString();

			comboBox_ColorSchemes.Items.Clear();

			foreach (LuaTheme theme in LuaThemeRepository.GetAvailableThemes())
				comboBox_ColorSchemes.Items.Add(theme.Name);

			if (!string.IsNullOrWhiteSpace(cachedSelectedItem) && comboBox_ColorSchemes.Items.Contains(cachedSelectedItem))
				comboBox_ColorSchemes.SelectedItem = cachedSelectedItem;
			else if (comboBox_ColorSchemes.Items.Contains(ConfigurationDefaults.SelectedThemeName))
				comboBox_ColorSchemes.SelectedItem = ConfigurationDefaults.SelectedThemeName;
			else if (comboBox_ColorSchemes.Items.Count > 0)
				comboBox_ColorSchemes.SelectedIndex = 0;
		}

		#endregion Construction

		#region Events

		private void VisiblePreviewSetting_Changed(object sender, EventArgs e)
			=> UpdatePreview();

		private void comboBox_FontFamily_SelectedIndexChanged(object sender, EventArgs e)
			=> UpdatePreview();

		private void comboBox_ColorSchemes_SelectedIndexChanged(object sender, EventArgs e)
			=> UpdatePreview();

		private void button_Color_Click(object sender, EventArgs e)
		{
		}

		private void menuItem_Bold_Click(object sender, EventArgs e)
		{
		}

		private void menuItem_Italic_Click(object sender, EventArgs e)
		{
		}

		private void button_SaveScheme_Click(object sender, EventArgs e)
		{
		}

		private void button_DeleteScheme_Click(object sender, EventArgs e)
		{
		}

		private void button_ImportScheme_Click(object sender, EventArgs e)
		{
		}

		private void button_OpenSchemesFolder_Click(object sender, EventArgs e)
		{
		}

		#endregion Events

		#region Loading

		private void UpdateControlsWithSettings(LuaEditorConfiguration config)
		{
			numeric_FontSize.Value = (decimal)config.FontSize - 4;
			comboBox_FontFamily.SelectedItem = config.FontFamily;
			numeric_UndoStackSize.Value = config.UndoStackSize;

			LoadSettingsForCheckBoxes(config);

			if (comboBox_ColorSchemes.Items.Contains(config.SelectedThemeName))
				comboBox_ColorSchemes.SelectedItem = config.SelectedThemeName;
			else if (comboBox_ColorSchemes.Items.Contains(ConfigurationDefaults.SelectedThemeName))
				comboBox_ColorSchemes.SelectedItem = ConfigurationDefaults.SelectedThemeName;
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
			config.FontSize = (double)(numeric_FontSize.Value + 4);
			config.FontFamily = comboBox_FontFamily.SelectedItem?.ToString() ?? TextEditorBaseDefaults.FontFamily;
			config.UndoStackSize = (int)numeric_UndoStackSize.Value;

			ApplySettingsFromCheckBoxes(config);

			if (comboBox_ColorSchemes.SelectedItem is not null)
				config.SelectedThemeName = comboBox_ColorSchemes.SelectedItem.ToString();

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
			numeric_FontSize.Value = (decimal)(TextEditorBaseDefaults.FontSize - 4);
			comboBox_FontFamily.SelectedItem = TextEditorBaseDefaults.FontFamily;
			numeric_UndoStackSize.Value = TextEditorBaseDefaults.UndoStackSize;
			ResetCheckBoxSettings();

			if (comboBox_ColorSchemes.Items.Contains(ConfigurationDefaults.SelectedThemeName))
				comboBox_ColorSchemes.SelectedItem = ConfigurationDefaults.SelectedThemeName;

			UpdatePreview();
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

		public void ForcePreviewUpdate()
			=> editorPreview.Focus();

		private void UpdatePreview()
		{
			if (editorPreview is null)
				return;

			LuaEditorConfiguration previewConfig = CreatePreviewConfiguration();
			editorPreview.UpdateSettings(previewConfig);
			editorPreview.SetSemanticTokens(PreviewTokens);
			ForcePreviewUpdate();
		}

		private LuaEditorConfiguration CreatePreviewConfiguration()
		{
			var config = new LuaEditorConfiguration
			{
				FontSize = (double)(numeric_FontSize.Value + 4),
				FontFamily = comboBox_FontFamily.SelectedItem?.ToString() ?? TextEditorBaseDefaults.FontFamily,
				UndoStackSize = (int)numeric_UndoStackSize.Value,
				AutocompleteEnabled = checkBox_Autocomplete.Checked,
				WordWrapping = checkBox_WordWrapping.Checked,
				AutoCloseParentheses = checkBox_CloseParentheses.Checked,
				AutoCloseBrackets = checkBox_CloseBrackets.Checked,
				AutoCloseQuotes = checkBox_CloseQuotes.Checked,
				AutoCloseBraces = checkBox_CloseBraces.Checked,
				HighlightCurrentLine = checkBox_HighlightCurrentLine.Checked,
				ShowLineNumbers = checkBox_LineNumbers.Checked,
				ShowVisualSpaces = checkBox_VisibleSpaces.Checked,
				ShowVisualTabs = checkBox_VisibleTabs.Checked
			};

			config.SelectedThemeName = comboBox_ColorSchemes.SelectedItem?.ToString() ?? ConfigurationDefaults.SelectedThemeName;
			return config;
		}

		private void buttonContextMenu_Opening(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
		}

		private static IReadOnlyList<LuaSemanticToken> CreatePreviewTokens()
		{
			return new[]
			{
				CreatePreviewToken(0, "Weapon", "class"),
				CreatePreviewToken(1, "Weapon", "class"),
				CreatePreviewToken(2, "levelName", "variable", 1, "global"),
				CreatePreviewToken(4, "Weapon", "class"),
				CreatePreviewToken(4, "new", "method", 1, "declaration"),
				CreatePreviewToken(4, "name", "parameter"),
				CreatePreviewToken(5, "math", "namespace", 1, "defaultLibrary"),
				CreatePreviewToken(5, "max", "function", 1, "defaultLibrary"),
				CreatePreviewToken(5, "levelName", "variable", 1, "global"),
				CreatePreviewToken(6, "name", "property", 1),
				CreatePreviewToken(6, "name", "parameter", 2)
			};
		}

		private static LuaSemanticToken CreatePreviewToken(int lineIndex, string tokenText, string tokenType, params string[] modifiers)
		{
			return CreatePreviewToken(lineIndex, tokenText, tokenType, 1, modifiers);
		}

		private static LuaSemanticToken CreatePreviewToken(int lineIndex, string tokenText, string tokenType, int occurrence, params string[] modifiers)
		{
			int characterIndex = GetOccurrenceIndex(PreviewLines[lineIndex], tokenText, occurrence);
			return new LuaSemanticToken(lineIndex, characterIndex, tokenText.Length, tokenType, modifiers);
		}

		private static int GetOccurrenceIndex(string line, string tokenText, int occurrence)
		{
			int startIndex = -1;

			for (int currentOccurrence = 0; currentOccurrence < occurrence; currentOccurrence++)
			{
				startIndex = line.IndexOf(tokenText, startIndex + 1, StringComparison.Ordinal);

				if (startIndex < 0)
					throw new InvalidOperationException("Failed to locate preview token '" + tokenText + "'.");
			}

			return startIndex;
		}
	}
}
