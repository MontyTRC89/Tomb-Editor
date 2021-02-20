using System;

namespace TombLib.Scripting.Extensions
{
	public static class StringExtensions
	{
		public static bool Contains(this string source, string value, StringComparison comparisonType)
			=> source?.IndexOf(value, comparisonType) >= 0;
	}
}
