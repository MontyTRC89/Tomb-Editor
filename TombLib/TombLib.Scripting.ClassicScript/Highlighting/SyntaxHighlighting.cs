using ICSharpCode.AvalonEdit.Highlighting;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.ClassicScript.Commands;
using TombLib.Scripting.ClassicScript.Mnemonics;

namespace TombLib.Scripting.ClassicScript.Highlighting;

public sealed class SyntaxHighlighting : IHighlightingDefinition
{
	private readonly ColorScheme _scheme;
	private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService = new();
	private readonly ClassicScriptCommandCatalogService _commandCatalogService = new();

	#region Construction

	public SyntaxHighlighting(ColorScheme scheme)
		=> _scheme = scheme;

	#endregion Construction

	#region Rules

	public HighlightingRuleSet MainRuleSet
	{
		get
		{
			var ruleSet = new HighlightingRuleSet();

			/* Comments */
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex(";.*$"),
				Color = new HighlightingColor
				{
					Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Comments.HtmlColor)),
					FontWeight = _scheme.Comments.IsBold ? FontWeights.Bold : FontWeights.Normal,
					FontStyle = _scheme.Comments.IsItalic ? FontStyles.Italic : FontStyles.Normal
				}
			});

			/* Sections */
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex(@"\[\b(" + string.Join("|", _commandCatalogService.Sections) + @")\b\]", RegexOptions.IgnoreCase),
				Color = new HighlightingColor
				{
					Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Sections.HtmlColor)),
					FontWeight = _scheme.Sections.IsBold ? FontWeights.Bold : FontWeights.Normal,
					FontStyle = _scheme.Sections.IsItalic ? FontStyles.Italic : FontStyles.Normal
				}
			});

			/* Standard commands */
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex(@"\b(" + string.Join("|", _commandCatalogService.OldCommands) + @")\b\s*=", RegexOptions.IgnoreCase),
				Color = new HighlightingColor
				{
					Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.StandardCommands.HtmlColor)),
					FontWeight = _scheme.StandardCommands.IsBold ? FontWeights.Bold : FontWeights.Normal,
					FontStyle = _scheme.StandardCommands.IsItalic ? FontStyles.Italic : FontStyles.Normal
				}
			});

			/* New commands */
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex(@"\b(" + string.Join("|", _commandCatalogService.NewCommands.Where(name => !name.StartsWith('#'))) + @")\b\s*=", RegexOptions.IgnoreCase),
				Color = new HighlightingColor
				{
					Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.NewCommands.HtmlColor)),
					FontWeight = _scheme.NewCommands.IsBold ? FontWeights.Bold : FontWeights.Normal,
					FontStyle = _scheme.NewCommands.IsItalic ? FontStyles.Italic : FontStyles.Normal
				}
			});

			/* Next line keys */
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex(">"),
				Color = new HighlightingColor
				{
					Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.NewCommands.HtmlColor)),
					FontWeight = FontWeights.Bold // Always bold
				}
			});

			/* Mnemonics */
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex(_mnemonicCatalogService.GetMnemonicPattern(), RegexOptions.IgnoreCase),
				Color = new HighlightingColor
				{
					Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.References.HtmlColor)),
					FontWeight = _scheme.References.IsBold ? FontWeights.Bold : FontWeights.Normal,
					FontStyle = _scheme.References.IsItalic ? FontStyles.Italic : FontStyles.Normal
				}
			});

			/* Hex values */
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex(@"\$[a-f0-9]*", RegexOptions.IgnoreCase),
				Color = new HighlightingColor
				{
					Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.References.HtmlColor)),
					FontWeight = _scheme.References.IsBold ? FontWeights.Bold : FontWeights.Normal,
					FontStyle = _scheme.References.IsItalic ? FontStyles.Italic : FontStyles.Normal
				}
			});

			/* Directives (#...) */
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex(@"#(define|first_id|include)\s", RegexOptions.IgnoreCase),
				Color = new HighlightingColor
				{
					Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.References.HtmlColor)),
					FontWeight = _scheme.References.IsBold ? FontWeights.Bold : FontWeights.Normal,
					FontStyle = _scheme.References.IsItalic ? FontStyles.Italic : FontStyles.Normal
				}
			});

			/* Values */
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex("\\d|\\w|\"|'|\\.|\\\\"),
				Color = new HighlightingColor
				{
					Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Values.HtmlColor)),
					FontWeight = _scheme.Values.IsBold ? FontWeights.Bold : FontWeights.Normal,
					FontStyle = _scheme.Values.IsItalic ? FontStyles.Italic : FontStyles.Normal
				}
			});

			ruleSet.Name = "ClassicScript Rules";
			return ruleSet;
		}
	}

	#endregion Rules

	#region Other

	public string Name => "ClassicScript Rules";

	public IEnumerable<HighlightingColor> NamedHighlightingColors => throw new NotImplementedException();
	public IDictionary<string, string> Properties => throw new NotImplementedException();

	public HighlightingColor GetNamedColor(string name)
		=> throw new NotImplementedException();

	public HighlightingRuleSet GetNamedRuleSet(string name)
		=> throw new NotImplementedException();

	#endregion Other
}
