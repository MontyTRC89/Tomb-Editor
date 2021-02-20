using System;
using TombLib.Scripting.ClassicScript.Enums;

namespace TombIDE.ScriptingStudio.Objects
{
	public class ReferenceDefinitionEventArgs : EventArgs
	{
		public string Keyword { get; }
		public ReferenceType Type { get; }

		public ReferenceDefinitionEventArgs(string keyword, ReferenceType type)
		{
			Keyword = keyword;
			Type = type;
		}
	}
}
