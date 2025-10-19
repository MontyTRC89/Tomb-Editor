#nullable enable

using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Collections.Generic;

public interface IGameflowAutocompleteService
{
	List<ICompletionData> GetAutocompleteData();
}
