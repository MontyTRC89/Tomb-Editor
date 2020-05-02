namespace TombLib.Scripting.SyntaxTidying
{
	internal static class ScriptTidying
	{
		private static TextEditorConfiguration _config;

		public static string ReindentScript(string editorContent)
		{
			_config = new TextEditorConfiguration();

			editorContent = HandleSpacesBeforeEquals(editorContent);
			editorContent = HandleSpacesAfterEquals(editorContent);

			editorContent = HandleSpacesBeforeCommas(editorContent);
			editorContent = HandleSpacesAfterCommas(editorContent);

			editorContent = HandleSpaceReduction(editorContent);

			return SharedTidyingMethods.TrimEndingWhitespace(editorContent);
		}

		private static string HandleSpacesBeforeEquals(string editorContent)
		{
			if (_config.ClassicScriptConfiguration.Tidy_PreEqualSpace)
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
			if (_config.ClassicScriptConfiguration.Tidy_PostEqualSpace)
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
			if (_config.ClassicScriptConfiguration.Tidy_PreCommaSpace)
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
			if (_config.ClassicScriptConfiguration.Tidy_PostCommaSpace)
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
			if (_config.ClassicScriptConfiguration.Tidy_ReduceSpaces)
				while (editorContent.Contains("  "))
					editorContent = editorContent.Replace("  ", " ");

			return editorContent;
		}
	}
}
