using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Collections.Generic;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.GameFlowScript.Utils
{
	public static class Autocomplete
	{
		public static List<ICompletionData> GetAutocompleteData()
		{
			var data = new List<ICompletionData>();

			foreach (string keyword in Keywords.Sections)
				data.Add(new CompletionData(keyword + ": "));

			foreach (string keyword in Keywords.SpecialProperties)
				data.Add(new CompletionData(keyword + ": "));

			foreach (string keyword in Keywords.Properties)
				data.Add(new CompletionData(keyword + ": "));

			foreach (string keyword in Keywords.Constants)
				data.Add(new CompletionData(keyword));

			return data;
		}
	}
}
