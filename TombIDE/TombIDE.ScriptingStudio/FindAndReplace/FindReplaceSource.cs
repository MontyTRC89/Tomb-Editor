using System.Collections.Generic;

namespace TombIDE.ScriptingStudio.FindAndReplace;

public class FindReplaceSource : List<FindReplaceItem>
{
	public string Name { get; set; }

	public FindReplaceSource()
	{ }

	public FindReplaceSource(string name)
		=> Name = name;
}
