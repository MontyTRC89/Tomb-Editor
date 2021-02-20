using System;
using TombLib.Scripting.ClassicScript.Enums;

namespace TombLib.Scripting.ClassicScript.Objects
{
	public class WordDefinitionEventArgs : EventArgs
	{
		public string Word { get; }
		public WordType Type { get; }

		public WordDefinitionEventArgs(string word, WordType type)
		{
			Word = word;
			Type = type;
		}
	}
}
