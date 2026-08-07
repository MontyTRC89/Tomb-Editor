namespace TombLib.Scripting.GameFlowScript.Resources;

public static class Patterns
{
	public static string Comments => "//.*$";
	public static string BlockComments => "/[*]([^*]|([*][^/]))*[*]+/";
	public static string Sections => @"^\b(" + string.Join("|", GameFlowDefinitionCatalog.Sections) + @")\b:";
	public static string SpecialProperties => @"^\b(" + string.Join("|", GameFlowDefinitionCatalog.SpecialProperties) + @")\b:";
	public static string Properties => @"^\s*\b(" + string.Join("|", GameFlowDefinitionCatalog.Properties) + @")\b:";
	public static string Constants => @"\b(" + string.Join("|", GameFlowDefinitionCatalog.Constants) + @")\b";
	public static string Values => "\\d|\\w|\"|'|\\.|\\\\";

	public static string LevelProperty => @"^\bLEVEL:\s*";
}
