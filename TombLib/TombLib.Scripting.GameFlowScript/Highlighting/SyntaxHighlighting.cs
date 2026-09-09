using ICSharpCode.AvalonEdit.Highlighting;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.UI.Highlighting;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.GameFlowScript.Highlighting;

/// <summary>
/// Provides the highlighting definition for the GameFlow editor.
/// </summary>
public sealed class SyntaxHighlighting : IHighlightingDefinition
{
	private readonly ColorScheme _scheme;

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="SyntaxHighlighting"/> class.
	/// </summary>
	/// <param name="scheme">The color scheme used for the highlighting rules.</param>
	public SyntaxHighlighting(ColorScheme scheme) => _scheme = scheme;

	// Rules

	private HighlightingRuleSet? _cachedRuleSet;

	/// <summary>
	/// Gets the main rule set, built lazily from the color scheme.
	/// </summary>
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
			Regex = new(Patterns.Comments),
			Color = CreateColor(_scheme.Comments)
		});

		ruleSet.Rules.Add(new HighlightingRule
		{
			Regex = new(Patterns.BlockComments),
			Color = CreateColor(_scheme.Comments)
		});

		if (GameFlowDefinitionCatalog.Sections.Count > 0)
		{
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new(Patterns.Sections, RegexOptions.IgnoreCase),
				Color = CreateColor(_scheme.Sections)
			});
		}

		if (GameFlowDefinitionCatalog.SpecialProperties.Count > 0)
		{
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new(Patterns.SpecialProperties, RegexOptions.IgnoreCase),
				Color = CreateColor(_scheme.SpecialProperties)
			});
		}

		if (GameFlowDefinitionCatalog.Properties.Count > 0)
		{
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new(Patterns.Properties, RegexOptions.IgnoreCase),
				Color = CreateColor(_scheme.Properties)
			});
		}

		if (GameFlowDefinitionCatalog.Constants.Count > 0)
		{
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new(Patterns.Constants, RegexOptions.IgnoreCase),
				Color = CreateColor(_scheme.Constants)
			});
		}

		ruleSet.Rules.Add(new HighlightingRule
		{
			Regex = new(Patterns.Values),
			Color = CreateColor(_scheme.Values)
		});

		ruleSet.Name = "GameFlowScript Rules";
		return ruleSet;
	}

	private static HighlightingColor CreateColor(HighlightingObject scheme) => new()
	{
		Foreground = new SimpleHighlightingBrush(ScriptingColorParser.ParseColorOrDefault(scheme.HtmlColor, ScriptingColorParser.DefaultHighlightingColor)),
		FontWeight = scheme.IsBold ? FontWeights.Bold : FontWeights.Normal,
		FontStyle = scheme.IsItalic ? FontStyles.Italic : FontStyles.Normal
	};

	// Other

	/// <summary>
	/// Gets the name of the highlighting definition.
	/// </summary>
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
	public HighlightingColor? GetNamedColor(string name) => null;

	/// <summary>
	/// Resolves a named rule set. Only the main rule set is defined by GameFlow highlighting.
	/// </summary>
	public HighlightingRuleSet? GetNamedRuleSet(string name)
		=> name == MainRuleSet.Name ? MainRuleSet : null;
}
