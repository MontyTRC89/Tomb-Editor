namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Defines the scoring weights used when ranking parsed Lua completion items.
/// </summary>
internal static class LuaCompletionPriorityWeights
{
	internal const double PreselectedBonus = 1000000.0;
	internal const double ResponseOrderWeight = 100000.0;
	internal const double LocalScope = 20000.0;
	internal const double UpvalueOrParameter = 15000.0;
	internal const double VariableKind = 10000.0;
	internal const double FieldOrPropertyKind = 9000.0;
	internal const double MethodOrFunctionKind = 7000.0;
	internal const double KeywordKindPenalty = -5000.0;
}
