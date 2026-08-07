using ICSharpCode.AvalonEdit.Highlighting;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Scripting.GameFlowScript.Highlighting;

public sealed class SyntaxHighlighting : IHighlightingDefinition
{
	private readonly ColorScheme _scheme;

	// Construction

	public SyntaxHighlighting(ColorScheme scheme)
		=> _scheme = scheme;

	// Rules

	private HighlightingRuleSet? _cachedRuleSet;

	public HighlightingRuleSet MainRuleSet
	{
		get
		{
			_cachedRuleSet ??= BuildRuleSet();
			return _cachedRuleSet;
		}
	}

	private HighlightingRuleSet BuildRuleSet()
	{
		var ruleSet = new HighlightingRuleSet();

		ruleSet.Rules.Add(new HighlightingRule
		{
			Regex = new Regex(Patterns.Comments),
			Color = CreateColor(_scheme.Comments)
		});

		ruleSet.Rules.Add(new HighlightingRule
		{
			Regex = new Regex(Patterns.BlockComments),
			Color = CreateColor(_scheme.Comments)
		});

		if (GameFlowDefinitionCatalog.Sections.Count > 0)
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex(Patterns.Sections, RegexOptions.IgnoreCase),
				Color = CreateColor(_scheme.Sections)
			});

		if (GameFlowDefinitionCatalog.SpecialProperties.Count > 0)
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex(Patterns.SpecialProperties, RegexOptions.IgnoreCase),
				Color = CreateColor(_scheme.SpecialProperties)
			});

		if (GameFlowDefinitionCatalog.Properties.Count > 0)
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex(Patterns.Properties, RegexOptions.IgnoreCase),
				Color = CreateColor(_scheme.Properties)
			});

		if (GameFlowDefinitionCatalog.Constants.Count > 0)
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex(Patterns.Constants, RegexOptions.IgnoreCase),
				Color = CreateColor(_scheme.Constants)
			});

		ruleSet.Rules.Add(new HighlightingRule
		{
			Regex = new Regex(Patterns.Values),
			Color = CreateColor(_scheme.Values)
		});

		ruleSet.Name = "GameFlowScript Rules";
		return ruleSet;
	}

	private static HighlightingColor CreateColor(HighlightingObject scheme)
		=> new()
		{
			Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(scheme.HtmlColor)),
			FontWeight = scheme.IsBold ? FontWeights.Bold : FontWeights.Normal,
			FontStyle = scheme.IsItalic ? FontStyles.Italic : FontStyles.Normal
		};

	// Other

	public string Name => "GameFlowScript Rules";

	/// <summary>
	/// Gets the named highlighting colors. GameFlow highlighting defines no named colors.
	/// </summary>
	public IEnumerable<HighlightingColor> NamedHighlightingColors => [];

	/// <summary>
	/// Gets the highlighting properties. GameFlow highlighting defines no custom properties.
	/// </summary>
	public IDictionary<string, string> Properties => new Dictionary<string, string>();

	/// <summary>
	/// Resolves a named highlighting color. GameFlow highlighting defines no named colors.
	/// </summary>
	public HighlightingColor? GetNamedColor(string name)
		=> null;

	/// <summary>
	/// Resolves a named rule set. Only the main rule set is defined by GameFlow highlighting.
	/// </summary>
	public HighlightingRuleSet? GetNamedRuleSet(string name)
		=> name == MainRuleSet.Name ? MainRuleSet : null;

}
