using System.Diagnostics.CodeAnalysis;

namespace TombLib.Scripting.Extensions;

public static class StringExtensions
{
	/// <summary>
	/// Checks if the given string equals another string, ignoring case.
	/// </summary>
	/// <param name="value">The string to compare.</param>
	/// <param name="other">The string to compare against.</param>
	/// <returns><see langword="true"/> if the strings are equal ignoring case; otherwise, <see langword="false"/>.</returns>
	public static bool IgnoreCaseEquals(this string value, string? other)
		=> value.Equals(other, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Checks if the given string equals any of the provided strings, using the specified comparison type.
	/// </summary>
	/// <param name="value">The string to compare.</param>
	/// <param name="comparisonType">The type of string comparison to use.</param>
	/// <param name="strings">The strings to compare against.</param>
	/// <returns><see langword="true"/> if the string equals any of the provided strings; otherwise, <see langword="false"/>.</returns>
	public static bool EqualsAny(this string value, StringComparison comparisonType, params string[] strings)
		=> strings.Any(@string => value.Equals(@string, comparisonType));

	/// <summary>
	/// Checks if the given string equals any of the provided strings, ignoring case.
	/// </summary>
	/// <param name="value">The string to compare.</param>
	/// <param name="strings">The strings to compare against.</param>
	/// <returns><see langword="true"/> if the string equals any of the provided strings ignoring case; otherwise, <see langword="false"/>.</returns>
	public static bool IgnoreCaseEqualsAny(this string value, params string[] strings)
		=> EqualsAny(value, StringComparison.OrdinalIgnoreCase, strings);

    /// <summary>
    /// Checks if the given string starts with any of the provided strings, using the specified comparison type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="comparisonType">The type of string comparison to use.</param>
    /// <param name="strings">The strings to check against.</param>
    /// <returns><see langword="true"/> if the string starts with any of the provided strings; otherwise, <see langword="false"/>.</returns>
    public static bool StartsWithAny(this string value, StringComparison comparisonType, params string[] strings)
        => strings.Any(@string => value.StartsWith(@string, comparisonType));

    /// <summary>
    /// Checks if the given string starts with any of the provided characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="chars">The characters to check against.</param>
    /// <returns><see langword="true"/> if the string starts with any of the provided characters; otherwise, <see langword="false"/>.</returns>
    public static bool StartsWithAny(this string value, params char[] chars)
        => chars.Any(value.StartsWith);

    /// <summary>
    /// Checks if the given string ends with any of the provided strings, using the specified comparison type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="comparisonType">The type of string comparison to use.</param>
    /// <param name="strings">The strings to check against.</param>
    /// <returns><see langword="true"/> if the string ends with any of the provided strings; otherwise, <see langword="false"/>.</returns>
    public static bool EndsWithAny(this string value, StringComparison comparisonType, params string[] strings)
        => strings.Any(@string => value.EndsWith(@string, comparisonType));

    /// <summary>
    /// Checks if the given string ends with any of the provided characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="chars">The characters to check against.</param>
    /// <returns><see langword="true"/> if the string ends with any of the provided characters; otherwise, <see langword="false"/>.</returns>
    public static bool EndsWithAny(this string value, params char[] chars)
        => chars.Any(value.EndsWith);

    /// <summary>
    /// Checks if the given string contains any of the provided strings, using the specified comparison type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="comparisonType">The type of string comparison to use.</param>
    /// <param name="strings">The strings to check against.</param>
    /// <returns><see langword="true"/> if the string contains any of the provided strings; otherwise, <see langword="false"/>.</returns>
    public static bool ContainsAny(this string value, StringComparison comparisonType, params string[] strings)
        => strings.Any(@string => value.Contains(@string, comparisonType));

    /// <summary>
    /// Checks if the given string contains any of the provided characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="chars">The characters to check against.</param>
    /// <returns><see langword="true"/> if the string contains any of the provided characters; otherwise, <see langword="false"/>.</returns>
    public static bool ContainsAny(this string value, params char[] chars)
        => chars.Any(value.Contains);

	/// <summary>
	/// Determines whether the specified string is not null or empty.
	/// </summary>
	/// <param name="value">The string to check.</param>
	/// <returns><see langword="true"/> if the string is not null or empty; otherwise, <see langword="false"/>.</returns>
	public static bool HasValue([NotNullWhen(true)] this string? value) => !string.IsNullOrEmpty(value);

	/// <summary>
	/// Determines whether the specified string is not null, empty, or consists only of white-space characters.
	/// </summary>
	/// <param name="value">The string to check.</param>
	/// <returns><see langword="true"/> if the string is not null, empty, or white-space; otherwise, <see langword="false"/>.</returns>
	public static bool HasText([NotNullWhen(true)] this string? value) => !string.IsNullOrWhiteSpace(value);

	/// <summary>
	/// Returns the original string if it has text; otherwise, returns the specified fallback string.
	/// </summary>
	/// <param name="value">The string to check.</param>
	/// <param name="fallback">The fallback string to return if the original string is null, empty, or white-space.</param>
	/// <returns>The original string if it has text; otherwise, the fallback string.</returns>
	[return: NotNullIfNotNull(nameof(fallback))]
	public static string? Or(this string? value, string? fallback) => value.HasText() ? value : fallback;
}
