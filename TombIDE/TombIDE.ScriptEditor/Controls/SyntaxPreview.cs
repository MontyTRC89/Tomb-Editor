using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombLib.Scripting.Resources;
using TombLib.Scripting.TextEditors;

namespace TombIDE.ScriptEditor.Controls
{
	internal class SyntaxPreview : RichTextBox
	{
		/* Properties */

		public int CurrentArgumentIndex { get; set; }

		public int SelectionEnd { get { return SelectionStart + SelectionLength; } }
		public int VisibleCharCount { get { return Width / CharacterWidth; } }
		public int ViewStart { get; private set; } = 0;

		/* Private fields */

		private TextEditorConfigs _configs;

		private string _cachedText;
		private int _cachedArgumentIndex;

		/* Private constants */

		private const int CharacterWidth = 7; // For ("Consolas", 9.75f, FontStyle.Bold)

		#region Construction and public methods

		public SyntaxPreview()
		{
			Font = new Font("Consolas", 9.75f, FontStyle.Bold);

			BorderStyle = BorderStyle.None;
			ScrollBars = RichTextBoxScrollBars.None;

			ReadOnly = true;
			WordWrap = false;

			ReloadSettings();
		}

		public void ReloadSettings()
		{
			_configs = TextEditorConfigs.Load();

			BackColor = ColorTranslator.FromHtml(_configs.ClassicScript.ColorScheme.Background);
			ForeColor = ColorTranslator.FromHtml(_configs.ClassicScript.ColorScheme.Values.HtmlColor);

			DoSyntaxHighlighting();
		}

		#endregion Construction and public methods

		#region Events

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);

			DoSyntaxHighlighting();

			if (SelectAndUnderlineCurrentArgument() && (CurrentArgumentIndex != _cachedArgumentIndex || Text != _cachedText))
				ScrollToSelectedArgument();

			_cachedText = Text;
			_cachedArgumentIndex = CurrentArgumentIndex;
		}

		#endregion Events

		#region Methods

		private void DoSyntaxHighlighting()
		{
			// Clear all styles
			SelectAll();
			SelectionColor = ColorTranslator.FromHtml(_configs.ClassicScript.ColorScheme.Values.HtmlColor);

			// Set the colors
			SetTextColor(@"\[\b(" + string.Join("|", ScriptKeywords.Sections) + @"|Any)\b\]", ColorTranslator.FromHtml(_configs.ClassicScript.ColorScheme.Sections.HtmlColor));
			SetTextColor(ScriptPatterns.StandardCommands, ColorTranslator.FromHtml(_configs.ClassicScript.ColorScheme.StandardCommands.HtmlColor));
			SetTextColor(ScriptPatterns.NewCommands, ColorTranslator.FromHtml(_configs.ClassicScript.ColorScheme.NewCommands.HtmlColor));
			SetTextColor("(ENABLED|DISABLED|#INCLUDE|#DEFINE|#FIRST_ID)", ColorTranslator.FromHtml(_configs.ClassicScript.ColorScheme.References.HtmlColor));
			SetTextColor(@"\(.*?_\.*?\)", ColorTranslator.FromHtml(_configs.ClassicScript.ColorScheme.References.HtmlColor));
			SetTextColor(@"(,|/|\(\*Array\*\))", ColorTranslator.FromHtml(_configs.ClassicScript.ColorScheme.Foreground));
			SetTextColor(ScriptPatterns.Comments, ColorTranslator.FromHtml(_configs.ClassicScript.ColorScheme.Comments.HtmlColor));
		}

		private void SetTextColor(string regexPattern, Color color)
		{
			foreach (Match item in Regex.Matches(Text, regexPattern))
			{
				Select(item.Index, item.Length);
				SelectionColor = color;
			}
		}

		private bool SelectAndUnderlineCurrentArgument()
		{
			if (string.IsNullOrWhiteSpace(Text) || !Text.Contains("="))
				return false;

			string[] arguments = Text.Split(',');

			if (arguments.Length == 1)
				return false;

			if (CurrentArgumentIndex >= arguments.Length)
				return false;

			if (CurrentArgumentIndex == -1)
				return false;

			string currentArgument = arguments[CurrentArgumentIndex];

			string token;

			if (CurrentArgumentIndex == 0)
				token = currentArgument.Split('=')[1].Trim();
			else
				token = currentArgument.Trim();

			List<string> prevArgs = new List<string>();

			for (int i = 0; i < CurrentArgumentIndex; i++)
				prevArgs.Add(arguments[i]);

			int startIndex = string.Join(",", prevArgs.ToArray()).Length;

			Select(Text.IndexOf(token, startIndex), token.Length);
			SelectionFont = new Font(SelectionFont.FontFamily, SelectionFont.Size, FontStyle.Underline | FontStyle.Bold);

			return true;
		}

		private void ScrollToSelectedArgument()
		{
			string textBeforeArgument = Text.Substring(0, SelectionStart);
			string[] prevArguments = textBeforeArgument.Split(',');

			string textAfterArgument = Text.Substring(SelectionEnd);
			int nextArgumentTextLength = string.IsNullOrEmpty(textAfterArgument) ?
				0 : textAfterArgument.Split(',')[1].Length + 2; // +2 because of the missing commas before and after the substring

			int visibleRangeEnd = ViewStart + VisibleCharCount; // In characters

			if (SelectionEnd > (visibleRangeEnd - nextArgumentTextLength))
			{
				int selectionEnd_X_Location = SelectionEnd * CharacterWidth; // In pixels
				int nextArgumentTextWidth = nextArgumentTextLength * CharacterWidth; // In pixels

				if (selectionEnd_X_Location > (Width - nextArgumentTextWidth))
				{
					int charsToScroll = SelectionEnd - VisibleCharCount + nextArgumentTextLength;
					PerformHorizontalScrollToIndex(charsToScroll);

					ViewStart = charsToScroll;
				}
			}
			else if (SelectionStart < ViewStart || prevArguments.Length == 1)
			{
				int charsToScroll = prevArguments.Length == 1 ? 0 : SelectionStart - 2; // -2 to prevent text cut-offs
				PerformHorizontalScrollToIndex(charsToScroll);

				ViewStart = charsToScroll;
			}
		}

		private void PerformHorizontalScrollToIndex(int characterIndex)
		{
			NativeMethods.BeginControlUpdate(Handle);

			NativeMethods.PerformFullHorizontalScroll(Handle, ScrollDirection.Left);

			for (int i = 0; i < characterIndex; i++)
				NativeMethods.PerformSingleHorizontalScroll(Handle, ScrollDirection.Right);

			NativeMethods.EndControlUpdate(Handle);

			Invalidate();
		}

		#endregion Methods
	}
}
