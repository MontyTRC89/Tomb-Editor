using System.Collections.Generic;

namespace TombLib.Scripting.Objects
{
	public class FindReplaceSource : List<FindReplaceItem>
	{
		public string Name { get; set; }

		public FindReplaceSource()
		{ }
		public FindReplaceSource(string name)
			=> Name = name;
	}
}
