using System;
using System.Collections.Generic;

namespace TombIDE.ScriptingStudio.FindAndReplace
{
	public class FindReplaceEventArgs : EventArgs
	{
		public List<FindReplaceSource> SourceCollection { get; }

		public FindReplaceEventArgs(List<FindReplaceSource> collection)
			=> SourceCollection = collection;
	}
}