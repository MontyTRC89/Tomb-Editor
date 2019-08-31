using TombLib.Script;

namespace ScriptEditor
{
	public class Project
	{
		public string Name { get; set; }
		public string GamePath { get; set; }
		public string ScriptPath { get; set; }
		public ScriptCompilers Compiler { get; set; }
		public string NGCenterPath { get; set; }
	}
}
