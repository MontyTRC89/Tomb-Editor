using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Collections.Generic;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Tomb1Main.Resources;

namespace TombLib.Scripting.Tomb1Main.Utils
{
	public static class Autocomplete
	{
		public static List<ICompletionData> GetAutocompleteData()
		{
			var data = new List<ICompletionData>();

			foreach (string keyword in Keywords.Collections)
				data.Add(new CompletionData("\"" + keyword + "\": "));

			foreach (string keyword in Keywords.Properties)
				data.Add(new CompletionData("\"" + keyword + "\": "));

			foreach (string keyword in Keywords.Constants)
				data.Add(new CompletionData("\"" + keyword + "\","));

			foreach (string keyword in Keywords.Values)
				data.Add(new CompletionData(keyword + ","));

			return data;
		}
	}
}
