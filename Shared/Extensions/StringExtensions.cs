using System.Diagnostics.CodeAnalysis;

namespace Shared.Extensions;

public static class StringExtensions
{
	/// <summary>
	/// Determines whether the specified string has a value (is not null, empty, or whitespace).
	/// </summary>
	/// <param name="value">The string to check.</param>
	/// <returns><see langword="true" /> if the string has a value; otherwise, <see langword="false" />.</returns>
	public static bool HasValue([NotNullWhen(true)] this string? value)
		=> !string.IsNullOrWhiteSpace(value);

	/// <summary>
	/// Determines whether the specified string equals any of the provided comparison strings.
	/// </summary>
	/// <param name="value">The string to check.</param>
	/// <param name="comparisonType">One of the enumeration values that specifies how the strings will be compared.</param>
	/// <param name="strings">An array of strings to compare with the current string.</param>
	/// <returns><see langword="true" /> if the string equals any of the comparison strings; otherwise, <see langword="false" />.</returns>
	public static bool EqualsAny(this string value, StringComparison comparisonType, params string[] strings)
	{
		foreach (string @string in strings)
		{
			if (value.Equals(@string, comparisonType))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Determines whether the specified string equals another string, ignoring case.
	/// </summary>
	/// <param name="value">The string to check.</param>
	/// <param name="other">The string to compare with.</param>
	/// <returns><see langword="true" /> if the strings are equal (ignoring case); otherwise, <see langword="false" />.</returns>
	public static bool EqualsIgnoreCase(this string value, string? other)
		=> value.Equals(other, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Returns the zero-based index of the first occurrence of the specified string in the source string, ignoring case.
	/// </summary>
	/// <param name="source">The source string to search in.</param>
	/// <param name="value">The string to seek.</param>
	/// <returns>The zero-based index position of the value parameter if found, -1 if not found or 0 if <paramref name="value" /> is empty.</returns>
	public static int IndexOfIgnoreCase(this string source, string value)
		=> source.IndexOf(value, StringComparison.OrdinalIgnoreCase);
}
