namespace ScriptLib.IniScript;

public abstract record ColorSchemeBase
{
	public string Background { get; set; } = "Black";
	public string Foreground { get; set; } = "White";
}
