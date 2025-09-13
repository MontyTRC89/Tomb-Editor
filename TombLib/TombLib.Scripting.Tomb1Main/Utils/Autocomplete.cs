using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Collections.Generic;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Tomb1Main.Resources;

namespace TombLib.Scripting.Tomb1Main.Utils
{
	public static class Autocomplete
	{
		public static List<ICompletionData> GetAutocompleteData(bool isTR2)
		{
			var data = new List<ICompletionData>();

			foreach (string keyword in Keywords.GetAllCollections(isTR2))
				data.Add(new CompletionData("\"" + keyword + "\": "));

			foreach (string keyword in Keywords.GetAllProperties(isTR2))
				data.Add(new CompletionData("\"" + keyword + "\": "));

			foreach (string keyword in Keywords.GetAllConstants(isTR2))
				data.Add(new CompletionData("\"" + keyword + "\","));

			foreach (string keyword in Keywords.Values)
				data.Add(new CompletionData(keyword + ","));

			return data;
		}
	}
}
