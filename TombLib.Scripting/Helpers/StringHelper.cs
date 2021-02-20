using System;

namespace TombLib.Scripting.Helpers
{
	public static class StringHelper
	{
		public static bool BulkStringComparision(string value, StringComparison comparisonType, params string[] strings)
		{
			foreach (string @string in strings)
				if (value.Equals(@string, comparisonType))
					return true;

			return false;
		}
	}
}
