using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.Resources;
using TombLib.Scripting.TextEditors.Configs;

namespace TombLib.Scripting.TextEditors.SyntaxHighlighting
{
	public sealed class ScriptSyntaxHighlighting : IHighlightingDefinition
	{
		private readonly ClassicScriptEditorConfiguration _config = new ClassicScriptEditorConfiguration().Load<ClassicScriptEditorConfiguration>();

		public string Name { get { return "Script Rules"; } }

		public HighlightingRuleSet MainRuleSet
		{
			get
			{
				HighlightingRuleSet ruleSet = new HighlightingRuleSet();

				/* Comments */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(ScriptPatterns.Comments),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.Comments.HtmlColor)),
						FontWeight = _config.ColorScheme.Comments.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _config.ColorScheme.Comments.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				/* Sections */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(ScriptPatterns.Sections, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.Sections.HtmlColor)),
						FontWeight = _config.ColorScheme.Sections.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _config.ColorScheme.Sections.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				/* Standard commands */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(ScriptPatterns.StandardCommands, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.StandardCommands.HtmlColor)),
						FontWeight = _config.ColorScheme.StandardCommands.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _config.ColorScheme.StandardCommands.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				/* New commands */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(ScriptPatterns.NewCommands, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.NewCommands.HtmlColor)),
						FontWeight = _config.ColorScheme.NewCommands.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _config.ColorScheme.NewCommands.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				/* Next line keys */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(ScriptPatterns.NextLineKey),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.NewCommands.HtmlColor)),
						FontWeight = FontWeights.Bold // Always bold
					}
				});

				/* Commas */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(ScriptPatterns.Comma),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.Foreground)),
					}
				});

				/* Mnemonics */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(ScriptPatterns.Mnemonics, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.References.HtmlColor)),
						FontWeight = _config.ColorScheme.References.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _config.ColorScheme.References.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				/* Hex values */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(ScriptPatterns.HexValues, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.References.HtmlColor)),
						FontWeight = _config.ColorScheme.References.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _config.ColorScheme.References.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				/* Directives (#...) */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(ScriptPatterns.Directives, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.References.HtmlColor)),
						FontWeight = _config.ColorScheme.References.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _config.ColorScheme.References.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				/* Values */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex("[a-z]|[A-Z]|[0-9]"),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.Values.HtmlColor)),
						FontWeight = _config.ColorScheme.Values.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _config.ColorScheme.Values.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Name = "Script Rules";
				return ruleSet;
			}
		}

		public IEnumerable<HighlightingColor> NamedHighlightingColors { get { throw new NotImplementedException(); } }
		public IDictionary<string, string> Properties { get { throw new NotImplementedException(); } }

		public HighlightingColor GetNamedColor(string name)
		{
			throw new NotImplementedException();
		}

		public HighlightingRuleSet GetNamedRuleSet(string name)
		{
			throw new NotImplementedException();
		}
	}
}
