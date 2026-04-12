using System;
using System.Collections.Generic;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class ColorScheme : ColorSchemeBase
	{
		public HighlightingObject Comments { get; set; } = CreateDefaultHighlighting("#6A9955");
		public HighlightingObject Values { get; set; } = CreateDefaultHighlighting("#569CD6");
		public HighlightingObject Statements { get; set; } = CreateDefaultHighlighting("#569CD6");
		public HighlightingObject Operators { get; set; } = CreateDefaultHighlighting("Gainsboro");
		public HighlightingObject SpecialOperators { get; set; } = CreateDefaultHighlighting("#569CD6");

		public ColorScheme()
		{
			Background = "#202020";
			Foreground = "Gainsboro";
		}

		public static bool operator ==(ColorScheme? left, ColorScheme? right)
			=> ReferenceEquals(left, right) || left is not null && left.Equals(right);

		public static bool operator !=(ColorScheme? left, ColorScheme? right)
			=> !(left == right);

		public override bool Equals(object? obj)
		{
			if (obj is not ColorScheme other)
				return false;

			return EqualityComparer<HighlightingObject>.Default.Equals(Comments, other.Comments)
				&& EqualityComparer<HighlightingObject>.Default.Equals(Values, other.Values)
				&& EqualityComparer<HighlightingObject>.Default.Equals(Statements, other.Statements)
				&& EqualityComparer<HighlightingObject>.Default.Equals(Operators, other.Operators)
				&& EqualityComparer<HighlightingObject>.Default.Equals(SpecialOperators, other.SpecialOperators)
				&& string.Equals(Background, other.Background, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(Foreground, other.Foreground, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode()
		{
			var hashCode = new HashCode();
			hashCode.Add(Comments);
			hashCode.Add(Values);
			hashCode.Add(Statements);
			hashCode.Add(Operators);
			hashCode.Add(SpecialOperators);
			hashCode.Add(Background ?? string.Empty, StringComparer.OrdinalIgnoreCase);
			hashCode.Add(Foreground ?? string.Empty, StringComparer.OrdinalIgnoreCase);
			return hashCode.ToHashCode();
		}

		private static HighlightingObject CreateDefaultHighlighting(string htmlColor)
			=> new HighlightingObject { HtmlColor = htmlColor };
	}
}
