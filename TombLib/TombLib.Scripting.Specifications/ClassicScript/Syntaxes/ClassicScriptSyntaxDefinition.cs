#nullable enable

namespace TombLib.Scripting.Specifications.ClassicScript.Syntaxes;

public sealed record class ClassicScriptSyntaxDefinition(
	string Key,
	string SyntaxText,
	string ApplicableSection,
	int ArgumentCount,
	bool HasArrayArguments);