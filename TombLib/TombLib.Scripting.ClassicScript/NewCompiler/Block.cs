namespace TombLib.Scripting.ClassicScript.NewCompiler;

public abstract class Block
{
	public string Name { get; protected set; }
	public int LineStartIndex { get; protected set; }
	public int LineCount { get; protected set; }

	protected Block(int lineStartIndex)
		=> LineStartIndex = lineStartIndex;
}
