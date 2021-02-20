using System;
using System.Collections.Generic;

namespace TombLib.Scripting.Objects
{
	public class FindReplaceEventArgs : EventArgs
	{
		public List<FindReplaceSource> SourceCollection { get; }

		public FindReplaceEventArgs(List<FindReplaceSource> collection)
			=> SourceCollection = collection;
	}
}
