using TombLib.Scripting.ClassicScript.Parsers;

namespace TombLib.Scripting.ClassicScript.Navigation;

public class WordDefinitionEventArgs : EventArgs
{
	public string Word { get; }
	public WordType Type { get; }
	public int HoveredOffset { get; }

	public WordDefinitionEventArgs(string word, WordType type, int hoveredOffset = -1)
	{
		Word = word;
		Type = type;
		HoveredOffset = hoveredOffset;
	}
}
