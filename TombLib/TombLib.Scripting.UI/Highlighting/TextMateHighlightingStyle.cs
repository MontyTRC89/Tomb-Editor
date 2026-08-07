using System.Windows;
using System.Windows.Media;

namespace TombLib.Scripting.UI.Highlighting;

internal sealed class TextMateHighlightingStyle
{
	public static readonly TextMateHighlightingStyle Empty = new TextMateHighlightingStyle(null, false, false, null);

	public TextMateHighlightingStyle(Brush? foreground, bool isBold, bool isItalic, TextDecorationCollection? textDecorations)
	{
		Foreground = foreground;
		IsBold = isBold;
		IsItalic = isItalic;
		TextDecorations = textDecorations;
	}

	public Brush? Foreground { get; }
	public bool IsBold { get; }
	public bool IsItalic { get; }
	public TextDecorationCollection? TextDecorations { get; }

	public bool HasFormatting
		=> Foreground is not null || IsBold || IsItalic || TextDecorations is not null;

	public Typeface CreateTypeface(Typeface baseTypeface)
	{
		FontStyle fontStyle = IsItalic ? FontStyles.Italic : baseTypeface.Style;
		FontWeight fontWeight = IsBold ? FontWeights.Bold : baseTypeface.Weight;

		return new Typeface(baseTypeface.FontFamily, fontStyle, fontWeight, baseTypeface.Stretch);
	}
}
