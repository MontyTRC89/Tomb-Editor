namespace TombLib.Scripting.Tomb1Main.Resources;

public sealed class Patterns
{
	public Patterns(bool isTR2)
	{
		Comments = "//.*$";
		Constants = $"\"\\b({string.Join("|", Keywords.GetAllConstants(isTR2))})\\b\"";
		Collections = $"\"\\b({string.Join("|", Keywords.GetAllCollections(isTR2))})\\b\"";
		Properties = $"\"\\b({string.Join("|", Keywords.GetAllProperties(isTR2))})\\b\"";
		Values = $@"\b({string.Join("|", Keywords.Values)})\b";
		Strings = "\"(.+?)\"";
	}

	public string Comments { get; }
	public string Constants { get; }
	public string Collections { get; }
	public string Properties { get; }
	public string Values { get; }
	public string Strings { get; }

	public static string LevelProperty => "\"title\":\\s*\"";
	public static string LevelCommentName => @"^\s*\/\/\s*(Level)?\s*\d+\s*(:|\.)\s*(.+)$";
}
