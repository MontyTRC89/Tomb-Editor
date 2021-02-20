namespace TombIDE.ScriptEditor.UI
{
	/// <summary>
	/// Depending on the <c>StudioMode</c>, <b>File</b>, <b>Edit</b>, <b>Options</b>, <b>View</b> and <b>Help</b> menu sections might contain different items.<br />
	/// Changing this mode is needed because, for example <b>Lua</b> script files don't have to be compiled,<br />
	/// therefore we don't need a "Build" button in the <b>File</b> section of the studio.<br />
	/// This mode should only be set once (preferably in the class constructor of the Studio class or directly in the field's value).
	/// </summary>
	public enum StudioMode
	{
		ClassicScript,
		Lua
	}
}
