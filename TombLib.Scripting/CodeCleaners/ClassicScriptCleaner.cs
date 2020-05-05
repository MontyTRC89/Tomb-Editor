namespace TombLib.Scripting.CodeCleaners
{
	public sealed class ClassicScriptCleaner : BasicCleaner
	{
		public bool PreEqualSpace { get; set; } = false;
		public bool PostEqualSpace { get; set; } = true;

		public bool PreCommaSpace { get; set; } = false;
		public bool PostCommaSpace { get; set; } = true;

		public bool ReduceSpaces { get; set; } = true;

		public string ReindentScript(string editorContent)
		{
			editorContent = HandleSpacesBeforeEquals(editorContent);
			editorContent = HandleSpacesAfterEquals(editorContent);

			editorContent = HandleSpacesBeforeCommas(editorContent);
			editorContent = HandleSpacesAfterCommas(editorContent);

			editorContent = HandleSpaceReduction(editorContent);

			return TrimEndingWhitespace(editorContent);
		}

		private string HandleSpacesBeforeEquals(string editorContent)
		{
			if (PreEqualSpace)
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

		private string HandleSpacesAfterEquals(string editorContent)
		{
			if (PostEqualSpace)
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

		private string HandleSpacesBeforeCommas(string editorContent)
		{
			if (PreCommaSpace)
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

		private string HandleSpacesAfterCommas(string editorContent)
		{
			if (PostCommaSpace)
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

		private string HandleSpaceReduction(string editorContent)
		{
			if (ReduceSpaces)
				while (editorContent.Contains("  "))
					editorContent = editorContent.Replace("  ", " ");

			return editorContent;
		}
	}
}
