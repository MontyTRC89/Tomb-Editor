using TombLib.Scripting.Specifications.GameFlow;

namespace TombLib.Scripting.GameFlowScript.Resources
{
	public struct Patterns
	{
		public static string Comments => "//.*$";
		public static string BlockComments => "/[*]([^*]|([*][^/]))*[*]+/";
		public static string Sections => @"^\b(" + string.Join("|", GameFlowDefinitionsProvider.Sections) + @")\b:";
		public static string SpecialProperties => @"^\b(" + string.Join("|", GameFlowDefinitionsProvider.SpecialProperties) + @")\b:";
		public static string Properties => @"^\s*\b(" + string.Join("|", GameFlowDefinitionsProvider.Properties) + @")\b:";
		public static string Constants => @"\b(" + string.Join("|", GameFlowDefinitionsProvider.Constants) + @")\b";
		public static string Values => "\\d|\\w|\"|'|\\.|\\\\";

		public static string LevelProperty => @"^\bLEVEL:\s*";
	}
}
