using ICSharpCode.AvalonEdit.Highlighting;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using TombLib.Scripting.ClassicScript.Commands;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.UI.Highlighting;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.ClassicScript.Highlighting;

/// <summary>
/// Provides the highlighting definition for the ClassicScript editor.
/// </summary>
public sealed class SyntaxHighlighting : IHighlightingDefinition
{
	private readonly ColorScheme _scheme;
	private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService = new();
	private readonly ClassicScriptCommandCatalogService _commandCatalogService = new();

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="SyntaxHighlighting"/> class.
	/// </summary>
	/// <param name="scheme">The color scheme used for the highlighting rules.</param>
	public SyntaxHighlighting(ColorScheme scheme)
		=> _scheme = scheme;

	// Rules

	private HighlightingRuleSet? _cachedRuleSet;
	private int _cachedMnemonicVersion = -1;

	/// <summary>
	/// Gets the main rule set, rebuilt when the mnemonic catalog snapshot changes.
	/// </summary>
	public HighlightingRuleSet MainRuleSet
	{
		get
		{
			// Rebuild only when the rule set has not been built yet or the mnemonic catalog
			// snapshot changed (for example after plugin deployment).
			int mnemonicVersion = ClassicScriptMnemonicCatalogService.CurrentSnapshotVersion;

			if (_cachedRuleSet is null || _cachedMnemonicVersion != mnemonicVersion)
			{
				_cachedRuleSet = BuildRuleSet();
				_cachedMnemonicVersion = mnemonicVersion;
			}

			return _cachedRuleSet;
		}
	}

	private HighlightingRuleSet BuildRuleSet()
	{
		var ruleSet = new HighlightingRuleSet();

		/* Comments */
		ruleSet.Rules.Add(new HighlightingRule
		{
			Regex = new Regex(";.*$"),
			Color = CreateColor(_scheme.Comments)
		});

		/* Sections */
		if (_commandCatalogService.Sections.Count > 0)
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = BuildWordBoundaryAlternation(@"\[\b({0})\b\]", _commandCatalogService.Sections, RegexOptions.IgnoreCase | RegexOptions.Compiled),
				Color = CreateColor(_scheme.Sections)
			});

		/* Standard commands */
		if (_commandCatalogService.OldCommands.Count > 0)
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = BuildWordBoundaryAlternation(@"\b({0})\b\s*=", _commandCatalogService.OldCommands, RegexOptions.IgnoreCase | RegexOptions.Compiled),
				Color = CreateColor(_scheme.StandardCommands)
			});

		/* New commands */
		string[] newCommands = _commandCatalogService.NewCommands.Where(name => !name.StartsWith('#')).ToArray();

		if (newCommands.Length > 0)
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = BuildWordBoundaryAlternation(@"\b({0})\b\s*=", newCommands, RegexOptions.IgnoreCase | RegexOptions.Compiled),
				Color = CreateColor(_scheme.NewCommands)
			});

		/* Next line keys */
		ruleSet.Rules.Add(new HighlightingRule
		{
			Regex = new Regex(">"),
			Color = new HighlightingColor
			{
				Foreground = new SimpleHighlightingBrush(ScriptingColorParser.ParseColorOrDefault(_scheme.NewCommands.HtmlColor, ScriptingColorParser.DefaultHighlightingColor)),
				FontWeight = FontWeights.Bold // Always bold
			}
		});

		/* Mnemonics */
		if (_mnemonicCatalogService.GetAllFlags().Count > 0)
			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = _mnemonicCatalogService.GetMnemonicRegex(),
				Color = CreateColor(_scheme.References)
			});

		/* Hex values */
		ruleSet.Rules.Add(new HighlightingRule
		{
			Regex = new Regex(@"\$[a-f0-9]*", RegexOptions.IgnoreCase),
			Color = CreateColor(_scheme.References)
		});

		/* Directives (#...) */
		ruleSet.Rules.Add(new HighlightingRule
		{
			Regex = new Regex(@"#(define|first_id|include)\s", RegexOptions.IgnoreCase),
			Color = CreateColor(_scheme.References)
		});

		/* Values */
		ruleSet.Rules.Add(new HighlightingRule
		{
			Regex = new Regex("\\d|\\w|\"|'|\\.|\\\\"),
			Color = CreateColor(_scheme.Values)
		});

		ruleSet.Name = "ClassicScript Rules";
		return ruleSet;
	}

	private static HighlightingColor CreateColor(HighlightingObject scheme) => new()
	{
		Foreground = new SimpleHighlightingBrush(ScriptingColorParser.ParseColorOrDefault(scheme.HtmlColor, ScriptingColorParser.DefaultHighlightingColor)),
		FontWeight = scheme.IsBold ? FontWeights.Bold : FontWeights.Normal,
		FontStyle = scheme.IsItalic ? FontStyles.Italic : FontStyles.Normal
	};

	/// <summary>
	/// Builds a compiled regex from a word-boundary template with every alternative regex-escaped.
	/// </summary>
	private static Regex BuildWordBoundaryAlternation(string template, IEnumerable<string> names, RegexOptions options)
		=> new(string.Format(template, string.Join("|", names.Select(Regex.Escape))), options);

	// Other

	/// <summary>
	/// Gets the name of the highlighting definition.
	/// </summary>
	public string Name => "ClassicScript Rules";

	/// <summary>
	/// Gets the named highlighting colors. ClassicScript highlighting defines no named colors.
	/// </summary>
	public IEnumerable<HighlightingColor> NamedHighlightingColors => [];

	/// <summary>
	/// Gets the highlighting properties. ClassicScript highlighting defines no custom properties.
	/// </summary>
	public IDictionary<string, string> Properties => new Dictionary<string, string>();

	/// <summary>
	/// Resolves a named highlighting color. ClassicScript highlighting defines no named colors.
	/// </summary>
	public HighlightingColor? GetNamedColor(string name)
		=> null;

	/// <summary>
	/// Resolves a named rule set. Only the main rule set is defined by ClassicScript highlighting.
	/// </summary>
	public HighlightingRuleSet? GetNamedRuleSet(string name)
		=> name == MainRuleSet.Name ? MainRuleSet : null;
}
