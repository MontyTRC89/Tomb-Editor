namespace TombLib.Scripting.ClassicScript.Syntaxes;

public sealed record class ClassicScriptSyntaxDefinition(
	string Key,
	string SyntaxText,
	string ApplicableSection,
	int ArgumentCount,
	bool HasArrayArguments);
