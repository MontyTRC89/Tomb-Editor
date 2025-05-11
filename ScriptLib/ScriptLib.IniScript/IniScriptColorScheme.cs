using ScriptLib.Core;

namespace ScriptLib.IniScript;

public record IniScriptColorScheme : ColorSchemeBase
{
	public HighlightingObject Sections { get; set; }  = new();
	public HighlightingObject Values { get; set; } = new();
	public HighlightingObject References { get; set; } = new();
	public HighlightingObject StandardCommands { get; set; } = new();
	public HighlightingObject NewCommands { get; set; } = new();
	public HighlightingObject Comments { get; set; } = new();
}
