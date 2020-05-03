namespace TombLib.Scripting.SyntaxTidying
{
	internal static class ScriptTidying
	{
		private static TextEditorConfigurations _configs;

		public static string ReindentScript(string editorContent)
		{
			_configs = new TextEditorConfigurations();

			editorContent = HandleSpacesBeforeEquals(editorContent);
			editorContent = HandleSpacesAfterEquals(editorContent);

			editorContent = HandleSpacesBeforeCommas(editorContent);
			editorContent = HandleSpacesAfterCommas(editorContent);

			editorContent = HandleSpaceReduction(editorContent);

			return SharedTidyingMethods.TrimEndingWhitespace(editorContent);
		}

		private static string HandleSpacesBeforeEquals(string editorContent)
		{
			if (_configs.ClassicScript.Tidy_PreEqualSpace)
			{
				editorContent = editorContent.Replace("=", " =");

				while (editorContent.Contains("  ="))
					editorContent = editorContent.Replace("  =", " =");
			}
			else
				while (editorContent.Contains(" ="))
					editorContent = editorContent.Replace(" =", "=");

			return editorContent;
		}

		private static string HandleSpacesAfterEquals(string editorContent)
		{
			if (_configs.ClassicScript.Tidy_PostEqualSpace)
			{
				editorContent = editorContent.Replace("=", "= ");

				while (editorContent.Contains("=  "))
					editorContent = editorContent.Replace("=  ", "= ");
			}
			else
				while (editorContent.Contains("= "))
					editorContent = editorContent.Replace("= ", "=");

			return editorContent;
		}

		private static string HandleSpacesBeforeCommas(string editorContent)
		{
			if (_configs.ClassicScript.Tidy_PreCommaSpace)
			{
				editorContent = editorContent.Replace(",", " ,");

				while (editorContent.Contains("  ,"))
					editorContent = editorContent.Replace("  ,", " ,");
			}
			else
				while (editorContent.Contains(" ,"))
					editorContent = editorContent.Replace(" ,", ",");

			return editorContent;
		}

		private static string HandleSpacesAfterCommas(string editorContent)
		{
			if (_configs.ClassicScript.Tidy_PostCommaSpace)
			{
				editorContent = editorContent.Replace(",", ", ");

				while (editorContent.Contains(",  "))
					editorContent = editorContent.Replace(",  ", ", ");
			}
			else
				while (editorContent.Contains(", "))
					editorContent = editorContent.Replace(", ", ",");

			return editorContent;
		}

		private static string HandleSpaceReduction(string editorContent)
		{
			if (_configs.ClassicScript.Tidy_ReduceSpaces)
				while (editorContent.Contains("  "))
					editorContent = editorContent.Replace("  ", " ");

			return editorContent;
		}
	}
}
