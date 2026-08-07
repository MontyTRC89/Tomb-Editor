using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using MdXaml;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.UI.Highlighting;
using TombLib.Scripting.UI.Resources;
using TombLib.WPF;
using static TombLib.WPF.BrushHelpers;

namespace TombLib.Scripting.UI.Rendering;

public static class MarkdownToolTipRenderer
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private const int MaxVisibleCodeBlockLines = 14;
	private const double ToolTipMaxHeight = ToolTipDefaults.PopupMaxHeight;
	private const double ToolTipMaxWidth = ToolTipDefaults.PopupMaxWidth;
	private const double ToolTipTextMaxWidth = ToolTipDefaults.TextMaxWidth;

	private static readonly Regex FencedCodeBlockPattern = new Regex(
		@"(?ms)(^|\n)(?<fence>`{3,}|~{3,})[ \t]*(?<lang>[^\n]*)\n(?<code>.*?)(?:\n)\k<fence>[ \t]*(?=\n|$)",
		RegexOptions.Compiled);

	private static readonly FontFamily BodyFontFamily = SystemFonts.MessageFontFamily;
	private static readonly FontFamily CodeFontFamily = new FontFamily(TextEditorBaseDefaults.FontFamily);
	private static readonly double BodyFontSize = ToolTipDefaults.TextFontSize;
	private static readonly double CodeFontSize = Math.Max(BodyFontSize - 1.0, 13.0);
	private static readonly Brush DefaultForeground = TextEditorColorPalette.ToolTipForeground;
	private static readonly Brush DefaultBackground = TextEditorColorPalette.ToolTipBackground;
	private static readonly Brush DefaultLinkForeground = CreateFrozenBrush(Color.FromRgb(112, 192, 231));

	private static readonly HashSet<string> SupportedHyperlinkSchemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		Uri.UriSchemeHttp,
		Uri.UriSchemeHttps
	};

	public static FrameworkElement CreateContent(string content, Brush foreground, Brush background, bool allowScrolling = true)
	{
		string normalizedContent = NormalizeLineEndings(content);
		string originalContent = normalizedContent;

		if (string.IsNullOrWhiteSpace(normalizedContent))
			return CreatePlainTextContent(string.Empty, foreground, allowScrolling);

		try
		{
			List<CodeBlockInfo> fencedCodeBlocks = ExtractFencedCodeBlocks(ref normalizedContent);

			var markdown = new Markdown
			{
				DisabledContextMenu = true
			};

			FlowDocument document = markdown.Transform(normalizedContent);
			ApplyDocumentTheme(document, foreground);
			ReplaceCodeBlocks(document, fencedCodeBlocks, foreground, background, allowScrolling);
			ApplyInlineCodeTheme(document, foreground, background);
			ApplyHyperlinkTheme(document);

			var viewer = new FlowDocumentScrollViewer
			{
				Document = document,
				Background = Brushes.Transparent,
				BorderThickness = new Thickness(0.0),
				Padding = new Thickness(0.0),
				Margin = new Thickness(0.0),
				IsToolBarVisible = false,
				VerticalScrollBarVisibility = allowScrolling
					? ScrollBarVisibility.Auto
					: ScrollBarVisibility.Hidden,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
				HorizontalAlignment = HorizontalAlignment.Left,
				IsSelectionEnabled = false,
				Focusable = false,
				MaxHeight = ToolTipMaxHeight,
				MaxWidth = ToolTipMaxWidth
			};

			if (allowScrolling)
				viewer.PreviewMouseWheel += ScrollHost_PreviewMouseWheel;

			viewer.PreviewMouseLeftButtonUp += HyperlinkHost_PreviewMouseLeftButtonUp;
			return viewer;
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to render a markdown tooltip; using plain text instead.");
			return CreatePlainTextContent(originalContent, foreground, allowScrolling);
		}
	}

	public static FrameworkElement CreatePlainTextContent(string content, Brush foreground, bool allowScrolling = true)
		=> CreateFallbackContent(content, foreground, allowScrolling);

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
		Brush codeBorder = CreateCodeBorder(background);
		var codeSpanElements = new List<Inline>();

		foreach (TextElement element in EnumerateTextElements(document))
		{
			if (!string.Equals(element.Tag as string, "CodeSpan", StringComparison.Ordinal)
				|| element is not Inline inline)
				continue;

			codeSpanElements.Add(inline);
		}

		foreach (Inline inline in codeSpanElements)
		{
			string inlineText = NormalizeLineEndings(ExtractInlineText(inline)).TrimEnd('\n');

			ReplaceInline(inline, CreateInlineCodeContainer(inlineText, foreground, codeBackground, codeBorder));
		}
	}

	private static string ExtractInlineText(Inline inline)
	{
		if (inline is null)
			return string.Empty;

		switch (inline)
		{
			case Run run:
				return run.Text ?? string.Empty;

			case LineBreak:
				return Environment.NewLine;

			case Span span:
				var builder = new StringBuilder();

				foreach (Inline childInline in span.Inlines)
					builder.Append(ExtractInlineText(childInline));

				return builder.ToString();

			default:
				return new TextRange(inline.ContentStart, inline.ContentEnd).Text;
		}
	}

	private static Inline CreateInlineCodeContainer(string text, Brush foreground, Brush background, Brush borderBrush)
		=> new InlineUIContainer(
			new Border
			{
				Background = background,
				BorderBrush = borderBrush,
				BorderThickness = new Thickness(1.0),
				CornerRadius = new CornerRadius(2.0),
				Padding = new Thickness(4.0, 1.0, 4.0, 1.0),
				Child = new TextBlock
				{
					Text = text ?? string.Empty,
					Foreground = foreground ?? DefaultForeground,
					FontFamily = CodeFontFamily,
					FontSize = CodeFontSize,
					TextWrapping = TextWrapping.NoWrap
				}
			})
		{
			BaselineAlignment = BaselineAlignment.Center
		};

	private static void ReplaceInline(Inline source, Inline replacement)
	{
		switch (source?.Parent)
		{
			case Paragraph paragraph:
				paragraph.Inlines.InsertBefore(source, replacement);
				paragraph.Inlines.Remove(source);
				break;

			case Span span:
				span.Inlines.InsertBefore(source, replacement);
				span.Inlines.Remove(source);
				break;
		}
	}

	private static void ApplyHyperlinkTheme(FlowDocument document)
	{
		foreach (TextElement element in EnumerateTextElements(document))
		{
			if (element is not Hyperlink hyperlink)
				continue;

			bool canOpen = IsSupportedHyperlink(hyperlink.NavigateUri);

			if (!canOpen)
				hyperlink.NavigateUri = null;

			hyperlink.Foreground = DefaultLinkForeground;
			hyperlink.TextDecorations = TextDecorations.Underline;
			hyperlink.Cursor = canOpen ? Cursors.Hand : Cursors.Arrow;
			hyperlink.Focusable = false;
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

	private static void ReplaceCodeBlocks(FlowDocument document, IReadOnlyList<CodeBlockInfo> fencedCodeBlocks, Brush foreground, Brush background, bool allowScrolling)
	{
		var codeBlockLookup = new Dictionary<string, CodeBlockInfo>(StringComparer.Ordinal);

		foreach (CodeBlockInfo codeBlock in fencedCodeBlocks)
			codeBlockLookup[codeBlock.Placeholder] = codeBlock;

		ReplaceCodeBlocks(document.Blocks, codeBlockLookup, foreground, background, allowScrolling);
	}

	private static void ReplaceCodeBlocks(BlockCollection blocks, IReadOnlyDictionary<string, CodeBlockInfo> fencedCodeBlocks, Brush foreground, Brush background, bool allowScrolling)
	{
		Block currentBlock = blocks.FirstBlock;

		while (currentBlock is not null)
		{
			Block nextBlock = currentBlock.NextBlock;

			if (TryCreateReplacementBlock(currentBlock, fencedCodeBlocks, foreground, background, allowScrolling, out Block? replacementBlock)
				&& replacementBlock is not null)
			{
				blocks.InsertBefore(currentBlock, replacementBlock);
				blocks.Remove(currentBlock);
			}
			else
			{
				switch (currentBlock)
				{
					case Section section:
						ReplaceCodeBlocks(section.Blocks, fencedCodeBlocks, foreground, background, allowScrolling);
						break;

					case List list:
						foreach (ListItem item in list.ListItems)
							ReplaceCodeBlocks(item.Blocks, fencedCodeBlocks, foreground, background, allowScrolling);
						break;

					case Table table:
						foreach (TableRowGroup rowGroup in table.RowGroups)
							foreach (TableRow row in rowGroup.Rows)
								foreach (TableCell cell in row.Cells)
									ReplaceCodeBlocks(cell.Blocks, fencedCodeBlocks, foreground, background, allowScrolling);
						break;
				}
			}

			currentBlock = nextBlock;
		}
	}

	private static bool TryCreateReplacementBlock(Block block, IReadOnlyDictionary<string, CodeBlockInfo> fencedCodeBlocks, Brush foreground, Brush background, bool allowScrolling, out Block? replacementBlock)
	{
		replacementBlock = null;

		if (block is not Paragraph paragraph)
			return false;

		string rawText = NormalizeLineEndings(new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text);
		string normalizedText = rawText.Trim();

		if (fencedCodeBlocks.TryGetValue(normalizedText, out CodeBlockInfo? fencedCodeBlock)
			&& fencedCodeBlock is not null)
		{
			replacementBlock = new BlockUIContainer(CreateCodeBlockElement(fencedCodeBlock.Language, fencedCodeBlock.Code, foreground, background, allowScrolling));
			return true;
		}

		if (!string.Equals(paragraph.Tag as string, "CodeBlock", StringComparison.Ordinal))
			return false;

		replacementBlock = new BlockUIContainer(CreateCodeBlockElement(null, rawText.TrimEnd('\n'), foreground, background, allowScrolling));
		return true;
	}

	private static FrameworkElement CreateCodeBlockElement(string? language, string code, Brush foreground, Brush background, bool allowScrolling)
	{
		TextEditor editor = CreateCodeBlockEditor(language, code, foreground ?? DefaultForeground);
		string normalizedCode = editor.Text;

		double lineHeight = GetEditorLineHeight(editor);
		double maxVisibleHeight = Math.Max(lineHeight + 4.0, Math.Ceiling(MaxVisibleCodeBlockLines * lineHeight) + 2.0);
		double desiredHeight = Math.Max(lineHeight + 4.0, MeasureWrappedCodeHeight(normalizedCode, ToolTipTextMaxWidth, lineHeight));

		editor.Height = Math.Min(desiredHeight, maxVisibleHeight);
		editor.VerticalScrollBarVisibility = allowScrolling && desiredHeight > maxVisibleHeight
			? ScrollBarVisibility.Auto
			: ScrollBarVisibility.Hidden;

		if (allowScrolling)
			editor.PreviewMouseWheel += ScrollHost_PreviewMouseWheel;

		return new Border
		{
			Background = CreateCodeBackground(background),
			BorderBrush = CreateCodeBorder(background),
			BorderThickness = new Thickness(1.0),
			CornerRadius = new CornerRadius(3.0),
			Padding = new Thickness(8.0, 6.0, 8.0, 6.0),
			Margin = new Thickness(0.0, 4.0, 0.0, 6.0),
			MaxWidth = ToolTipTextMaxWidth,
			Child = editor
		};
	}

	internal static TextEditor CreateCodeBlockEditor(string? language, string code, Brush foreground)
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
			Width = ToolTipTextMaxWidth,
			MaxWidth = ToolTipTextMaxWidth,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
			FontFamily = CodeFontFamily,
			FontSize = CodeFontSize,
			ShowLineNumbers = false,
			WordWrap = true,
			Focusable = false,
			IsTabStop = false
		};

		editor.Options.AllowScrollBelowDocument = false;
		editor.Options.EnableHyperlinks = false;
		editor.Options.EnableEmailHyperlinks = false;
		editor.Options.HighlightCurrentLine = false;
		editor.Options.ShowBoxForControlCharacters = false;
		editor.TextArea.Margin = new Thickness(0.0);
		editor.TextArea.Focusable = false;
		editor.TextArea.IsTabStop = false;
		KeyboardNavigation.SetIsTabStop(editor, false);
		KeyboardNavigation.SetIsTabStop(editor.TextArea, false);

		// Tooltip code-block editors are intentionally passive: they reuse TextMate highlighting when available,
		// but do not own any unload-driven disposal hook because tooltip hosts can unload and reuse the same editor.
		if (!string.Equals(language?.Trim(), "lua", StringComparison.OrdinalIgnoreCase)
			|| !LuaTextMateSyntaxHighlighting.TryInstall(editor, out _))
		{
			editor.SyntaxHighlighting = ResolveHighlighting(language);
		}

		return editor;
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

	private static double MeasureWrappedCodeHeight(string code, double width, double lineHeight)
	{
		var textBlock = new TextBlock
		{
			Text = string.IsNullOrEmpty(code) ? " " : code,
			FontFamily = CodeFontFamily,
			FontSize = CodeFontSize,
			TextWrapping = TextWrapping.Wrap,
			MaxWidth = width
		};

		textBlock.Measure(new Size(width, double.PositiveInfinity));
		return Math.Max(Math.Ceiling(textBlock.DesiredSize.Height) + 2.0, Math.Ceiling(lineHeight) + 4.0);
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

	private static FrameworkElement CreateFallbackContent(string content, Brush foreground, bool allowScrolling)
	{
		var textBlock = new TextBlock
		{
			Foreground = foreground ?? DefaultForeground,
			Text = content ?? string.Empty,
			TextWrapping = TextWrapping.Wrap,
			FontFamily = BodyFontFamily,
			FontSize = BodyFontSize,
			MaxWidth = ToolTipTextMaxWidth
		};

		var scrollViewer = new ScrollViewer
		{
			Content = textBlock,
			MaxHeight = ToolTipMaxHeight,
			MaxWidth = ToolTipMaxWidth,
			VerticalScrollBarVisibility = allowScrolling
				? ScrollBarVisibility.Auto
				: ScrollBarVisibility.Hidden,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
			CanContentScroll = true
		};

		if (allowScrolling)
		{
			scrollViewer.PreviewMouseWheel += ScrollHost_PreviewMouseWheel;
		}

		return scrollViewer;
	}

	private static void HyperlinkHost_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		Hyperlink? hyperlink = (e.OriginalSource as DependencyObject)?.FindAncestorOrSelf<Hyperlink>();

		if (hyperlink is not null && TryOpenHyperlink(hyperlink.NavigateUri))
			e.Handled = true;
	}

	private static bool TryOpenHyperlink(Uri? uri)
	{
		if (uri is null || !IsSupportedHyperlink(uri))
			return false;

		try
		{
			Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
			return true;
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to open the hyperlink '{Uri}'.", uri?.AbsoluteUri);
			return false;
		}
	}

	private static bool IsSupportedHyperlink(Uri? uri)
		=> uri is not null && uri.IsAbsoluteUri && SupportedHyperlinkSchemes.Contains(uri.Scheme);

	private static void ScrollHost_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		ScrollViewer? scrollViewer = sender as ScrollViewer ?? (sender as DependencyObject)?.FindVisualDescendant<ScrollViewer>();

		if (scrollViewer is null || scrollViewer.ScrollableHeight <= 0.0)
			return;

		if (e.Delta > 0)
			scrollViewer.LineUp();
		else if (e.Delta < 0)
			scrollViewer.LineDown();

		e.Handled = true;
	}

	internal static string NormalizeLineEndings(string text)
		=> (text ?? string.Empty)
			.Replace("\r\n", "\n", StringComparison.Ordinal)
			.Replace('\r', '\n');

	private static IHighlightingDefinition? ResolveHighlighting(string? language)
	{
		if (string.IsNullOrWhiteSpace(language))
			return null;

		string normalizedLanguage = language.Trim().ToLowerInvariant();

		if (normalizedLanguage == "lua")
			return LuaFallbackHighlightingLoader.Load();

		IHighlightingDefinition? definition = HighlightingManager.Instance.GetDefinition(normalizedLanguage);

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
