using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MdXaml;
using TombLib.Scripting.Highlighting;
using TombLib.Scripting.Resources;

namespace TombLib.Scripting.Rendering
{
	public static class MarkdownToolTipRenderer
	{
		private const int MaxVisibleCodeBlockLines = 14;
		private const double ToolTipMaxHeight = 420.0;
		private const double ToolTipMaxWidth = 540.0;
		private const double ToolTipTextMaxWidth = 500.0;
		private static readonly Regex FencedCodeBlockPattern = new Regex(
			@"(?ms)(^|\n)(?<fence>`{3,}|~{3,})[ \t]*(?<lang>[^\n]*)\n(?<code>.*?)(?:\n)\k<fence>[ \t]*(?=\n|$)",
			RegexOptions.Compiled);
		private static readonly FontFamily BodyFontFamily = SystemFonts.MessageFontFamily;
		private static readonly FontFamily CodeFontFamily = new FontFamily(TextEditorBaseDefaults.FontFamily);
		private static readonly double BodyFontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, 14.0);
		private static readonly double CodeFontSize = Math.Max(BodyFontSize - 1.0, 13.0);
		private static readonly Brush DefaultForeground = CreateFrozenBrush(Colors.Gainsboro);
		private static readonly Brush DefaultBackground = CreateFrozenBrush(Color.FromRgb(64, 64, 64));
		private static readonly Lazy<IHighlightingDefinition> LuaHighlighting = new Lazy<IHighlightingDefinition>(LoadLuaHighlighting);

		public static FrameworkElement CreateContent(string content, Brush foreground, Brush background)
		{
			string normalizedContent = NormalizeLineEndings(content);
			string originalContent = normalizedContent;

			if (string.IsNullOrWhiteSpace(normalizedContent))
				return CreateFallbackContent(string.Empty, foreground);

			try
			{
				List<CodeBlockInfo> fencedCodeBlocks = ExtractFencedCodeBlocks(ref normalizedContent);

				var markdown = new Markdown
				{
					DisabledContextMenu = true
				};

				FlowDocument document = markdown.Transform(normalizedContent);
				ApplyDocumentTheme(document, foreground);
				ReplaceCodeBlocks(document, fencedCodeBlocks, foreground, background);
				ApplyInlineCodeTheme(document, foreground, background);

				return new FlowDocumentScrollViewer
				{
					Document = document,
					Background = Brushes.Transparent,
					BorderThickness = new Thickness(0.0),
					Padding = new Thickness(0.0),
					Margin = new Thickness(0.0),
					IsToolBarVisible = false,
					VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
					HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
					HorizontalAlignment = HorizontalAlignment.Left,
					Focusable = false,
					MaxHeight = ToolTipMaxHeight,
					MaxWidth = ToolTipMaxWidth
				};
			}
			catch (Exception exception)
			{
				Debug.WriteLine($"[MarkdownToolTipRenderer] Failed to render markdown tooltip: {exception}");
				return CreateFallbackContent(originalContent, foreground);
			}
		}

		private static void ApplyDocumentTheme(FlowDocument document, Brush foreground)
		{
			document.FontFamily = BodyFontFamily;
			document.FontSize = BodyFontSize;
			document.Foreground = foreground ?? DefaultForeground;
			document.Background = Brushes.Transparent;
			document.PagePadding = new Thickness(0.0);
			document.ColumnWidth = ToolTipTextMaxWidth;
		}

		private static void ApplyInlineCodeTheme(FlowDocument document, Brush foreground, Brush background)
		{
			Brush codeBackground = CreateCodeBackground(background);
			var codeSpanElements = new List<TextElement>();

			foreach (TextElement element in EnumerateTextElements(document))
			{
				if (!string.Equals(element.Tag as string, "CodeSpan", StringComparison.Ordinal))
					continue;

				codeSpanElements.Add(element);
			}

			foreach (TextElement element in codeSpanElements)
			{

				string inlineText = NormalizeLineEndings(new TextRange(element.ContentStart, element.ContentEnd).Text).TrimEnd('\n');

				if (element is Span span)
				{
					span.Inlines.Clear();
					span.Inlines.Add(new Run(" " + inlineText + " "));
				}
				else if (element is Run run)
				{
					run.Text = " " + inlineText + " ";
				}

				element.SetValue(TextElement.FontFamilyProperty, CodeFontFamily);
				element.SetValue(TextElement.ForegroundProperty, foreground ?? DefaultForeground);
				element.SetValue(TextElement.BackgroundProperty, codeBackground);
			}
		}

		private static List<CodeBlockInfo> ExtractFencedCodeBlocks(ref string content)
		{
			var codeBlocks = new List<CodeBlockInfo>();
			int index = 0;

			content = FencedCodeBlockPattern.Replace(content, match =>
			{
				string placeholder = $"__TOMBIDE_MD_CODE_BLOCK_{index++}__";
				codeBlocks.Add(new CodeBlockInfo(placeholder, match.Groups["lang"].Value.Trim(), match.Groups["code"].Value));
				return match.Groups[1].Value + placeholder;
			});

			return codeBlocks;
		}

		private static void ReplaceCodeBlocks(FlowDocument document, IReadOnlyList<CodeBlockInfo> fencedCodeBlocks, Brush foreground, Brush background)
		{
			var codeBlockLookup = new Dictionary<string, CodeBlockInfo>(StringComparer.Ordinal);

			foreach (CodeBlockInfo codeBlock in fencedCodeBlocks)
				codeBlockLookup[codeBlock.Placeholder] = codeBlock;

			ReplaceCodeBlocks(document.Blocks, codeBlockLookup, foreground, background);
		}

		private static void ReplaceCodeBlocks(BlockCollection blocks, IReadOnlyDictionary<string, CodeBlockInfo> fencedCodeBlocks, Brush foreground, Brush background)
		{
			Block currentBlock = blocks.FirstBlock;

			while (currentBlock is not null)
			{
				Block nextBlock = currentBlock.NextBlock;

				if (TryCreateReplacementBlock(currentBlock, fencedCodeBlocks, foreground, background, out Block replacementBlock))
				{
					blocks.InsertBefore(currentBlock, replacementBlock);
					blocks.Remove(currentBlock);
				}
				else
				{
					switch (currentBlock)
					{
						case Section section:
							ReplaceCodeBlocks(section.Blocks, fencedCodeBlocks, foreground, background);
							break;

						case List list:
							foreach (ListItem item in list.ListItems)
								ReplaceCodeBlocks(item.Blocks, fencedCodeBlocks, foreground, background);
							break;

						case Table table:
							foreach (TableRowGroup rowGroup in table.RowGroups)
								foreach (TableRow row in rowGroup.Rows)
									foreach (TableCell cell in row.Cells)
										ReplaceCodeBlocks(cell.Blocks, fencedCodeBlocks, foreground, background);
							break;
					}
				}

				currentBlock = nextBlock;
			}
		}

		private static bool TryCreateReplacementBlock(Block block, IReadOnlyDictionary<string, CodeBlockInfo> fencedCodeBlocks, Brush foreground, Brush background, out Block replacementBlock)
		{
			replacementBlock = null;

			if (block is not Paragraph paragraph)
				return false;

			string rawText = NormalizeLineEndings(new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text);
			string normalizedText = rawText.Trim();

			if (fencedCodeBlocks.TryGetValue(normalizedText, out CodeBlockInfo fencedCodeBlock))
			{
				replacementBlock = new BlockUIContainer(CreateCodeBlockElement(fencedCodeBlock.Language, fencedCodeBlock.Code, foreground, background));
				return true;
			}

			if (!string.Equals(paragraph.Tag as string, "CodeBlock", StringComparison.Ordinal))
				return false;

			replacementBlock = new BlockUIContainer(CreateCodeBlockElement(null, rawText.TrimEnd('\n'), foreground, background));
			return true;
		}

		private static FrameworkElement CreateCodeBlockElement(string language, string code, Brush foreground, Brush background)
		{
			string normalizedCode = NormalizeCodeBlockText(code);

			var editor = new TextEditor
			{
				Text = normalizedCode,
				IsReadOnly = true,
				Background = Brushes.Transparent,
				Foreground = foreground ?? DefaultForeground,
				BorderThickness = new Thickness(0.0),
				Margin = new Thickness(0.0),
				Padding = new Thickness(0.0),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
				FontFamily = CodeFontFamily,
				FontSize = CodeFontSize,
				ShowLineNumbers = false,
				WordWrap = false
			};

			editor.Options.AllowScrollBelowDocument = false;
			editor.Options.EnableHyperlinks = false;
			editor.Options.EnableEmailHyperlinks = false;
			editor.Options.HighlightCurrentLine = false;
			editor.Options.ShowBoxForControlCharacters = false;
			editor.TextArea.Margin = new Thickness(0.0);

			if (!string.Equals(language?.Trim(), "lua", StringComparison.OrdinalIgnoreCase)
				|| !LuaTextMateSyntaxHighlighting.TryInstall(editor, out _))
			{
				editor.SyntaxHighlighting = ResolveHighlighting(language);
			}

			int lineCount = Math.Max(1, editor.Document.LineCount);
			double lineHeight = GetEditorLineHeight(editor);
			double visibleLineCount = Math.Min(lineCount, MaxVisibleCodeBlockLines);

			editor.Height = Math.Max(lineHeight + 4.0, Math.Ceiling(visibleLineCount * lineHeight) + 2.0);
			editor.VerticalScrollBarVisibility = lineCount > MaxVisibleCodeBlockLines
				? ScrollBarVisibility.Auto
				: ScrollBarVisibility.Hidden;

			return new Border
			{
				Background = CreateCodeBackground(background),
				BorderBrush = CreateCodeBorder(background),
				BorderThickness = new Thickness(1.0),
				CornerRadius = new CornerRadius(3.0),
				Padding = new Thickness(8.0, 6.0, 8.0, 6.0),
				Margin = new Thickness(0.0, 4.0, 0.0, 6.0),
				Child = editor
			};
		}

		private static double GetEditorLineHeight(TextEditor editor)
		{
			double lineHeight = editor.TextArea.TextView.DefaultLineHeight;

			if (!double.IsNaN(lineHeight) && lineHeight > 0.0)
				return Math.Ceiling(lineHeight);

			var typeface = new Typeface(editor.FontFamily, editor.FontStyle, editor.FontWeight, editor.FontStretch);
			var formattedText = new FormattedText(
				"Ag",
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				typeface,
				editor.FontSize,
				Brushes.Transparent,
				1.0);

			return Math.Ceiling(Math.Max(1.0, formattedText.Height));
		}

		private static string NormalizeCodeBlockText(string code)
		{
			string normalizedCode = NormalizeLineEndings(code);

			if (string.IsNullOrEmpty(normalizedCode))
				return string.Empty;

			string[] lines = normalizedCode.Split('\n');
			int lastContentLineIndex = lines.Length - 1;

			while (lastContentLineIndex >= 0 && string.IsNullOrWhiteSpace(lines[lastContentLineIndex]))
				lastContentLineIndex--;

			if (lastContentLineIndex < 0)
				return string.Empty;

			return string.Join(Environment.NewLine, lines, 0, lastContentLineIndex + 1);
		}

		private static IEnumerable<TextElement> EnumerateTextElements(FlowDocument document)
		{
			foreach (Block block in document.Blocks)
				foreach (TextElement element in EnumerateBlock(block))
					yield return element;
		}

		private static IEnumerable<TextElement> EnumerateBlock(Block block)
		{
			yield return block;

			switch (block)
			{
				case Paragraph paragraph:
					foreach (Inline inline in paragraph.Inlines)
						foreach (TextElement element in EnumerateInline(inline))
							yield return element;
					break;

				case Section section:
					foreach (Block childBlock in section.Blocks)
						foreach (TextElement element in EnumerateBlock(childBlock))
							yield return element;
					break;

				case List list:
					foreach (ListItem item in list.ListItems)
					{
						yield return item;
						foreach (Block childBlock in item.Blocks)
							foreach (TextElement element in EnumerateBlock(childBlock))
								yield return element;
					}
					break;

				case Table table:
					foreach (TableRowGroup rowGroup in table.RowGroups)
					{
						yield return rowGroup;
						foreach (TableRow row in rowGroup.Rows)
						{
							yield return row;
							foreach (TableCell cell in row.Cells)
							{
								yield return cell;
								foreach (Block childBlock in cell.Blocks)
									foreach (TextElement element in EnumerateBlock(childBlock))
										yield return element;
							}
						}
					}
					break;
			}
		}

		private static IEnumerable<TextElement> EnumerateInline(Inline inline)
		{
			yield return inline;

			if (inline is Span span)
				foreach (Inline childInline in span.Inlines)
					foreach (TextElement element in EnumerateInline(childInline))
						yield return element;
		}

		private static FrameworkElement CreateFallbackContent(string content, Brush foreground)
			=> new TextBlock
			{
				Foreground = foreground ?? DefaultForeground,
				Text = content ?? string.Empty,
				TextWrapping = TextWrapping.Wrap,
				FontFamily = BodyFontFamily,
				FontSize = BodyFontSize,
				MaxWidth = ToolTipTextMaxWidth
			};

		private static string NormalizeLineEndings(string text)
			=> (text ?? string.Empty)
				.Replace("\r\n", "\n", StringComparison.Ordinal)
				.Replace('\r', '\n');

		private static IHighlightingDefinition ResolveHighlighting(string language)
		{
			if (string.IsNullOrWhiteSpace(language))
				return null;

			string normalizedLanguage = language.Trim().ToLowerInvariant();

			if (normalizedLanguage == "lua")
				return LuaHighlighting.Value;

			IHighlightingDefinition definition = HighlightingManager.Instance.GetDefinition(normalizedLanguage);

			if (definition is not null)
				return definition;

			definition = HighlightingManager.Instance.GetDefinitionByExtension(normalizedLanguage.StartsWith(".", StringComparison.Ordinal)
				? normalizedLanguage
				: "." + normalizedLanguage);

			if (definition is not null)
				return definition;

			return normalizedLanguage switch
			{
				"cs" => HighlightingManager.Instance.GetDefinitionByExtension(".cs"),
				"csharp" => HighlightingManager.Instance.GetDefinitionByExtension(".cs"),
				"js" => HighlightingManager.Instance.GetDefinitionByExtension(".js"),
				"ts" => HighlightingManager.Instance.GetDefinitionByExtension(".ts"),
				"json5" => HighlightingManager.Instance.GetDefinitionByExtension(".json"),
				_ => null
			};
		}

		private static IHighlightingDefinition LoadLuaHighlighting()
		{
			string xmlFilePath = Path.Combine(AppContext.BaseDirectory, "Configs", "TextEditors", "ColorSchemes", "Lua", "Default.xml");

			if (!File.Exists(xmlFilePath))
				return null;

			using var stream = new FileStream(xmlFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var reader = new XmlTextReader(stream);
			return HighlightingLoader.Load(reader, HighlightingManager.Instance);
		}

		private static Brush CreateCodeBackground(Brush background)
		{
			Color baseColor = background is SolidColorBrush solidBrush
				? solidBrush.Color
				: ((SolidColorBrush)DefaultBackground).Color;

			return CreateFrozenBrush(Blend(baseColor, Colors.Black, 0.32));
		}

		private static Brush CreateCodeBorder(Brush background)
		{
			Color baseColor = background is SolidColorBrush solidBrush
				? solidBrush.Color
				: ((SolidColorBrush)DefaultBackground).Color;

			return CreateFrozenBrush(Blend(baseColor, Colors.White, 0.18));
		}

		private static Color Blend(Color first, Color second, double ratio)
		{
			double clampedRatio = Math.Max(0.0, Math.Min(1.0, ratio));
			double inverseRatio = 1.0 - clampedRatio;

			return Color.FromArgb(
				(byte)Math.Round(first.A * inverseRatio + second.A * clampedRatio),
				(byte)Math.Round(first.R * inverseRatio + second.R * clampedRatio),
				(byte)Math.Round(first.G * inverseRatio + second.G * clampedRatio),
				(byte)Math.Round(first.B * inverseRatio + second.B * clampedRatio));
		}

		private static SolidColorBrush CreateFrozenBrush(Color color)
		{
			var brush = new SolidColorBrush(color);
			brush.Freeze();
			return brush;
		}

		private sealed class CodeBlockInfo
		{
			public CodeBlockInfo(string placeholder, string language, string code)
			{
				Placeholder = placeholder;
				Language = language;
				Code = code;
			}

			public string Placeholder { get; }
			public string Language { get; }
			public string Code { get; }
		}
	}
}