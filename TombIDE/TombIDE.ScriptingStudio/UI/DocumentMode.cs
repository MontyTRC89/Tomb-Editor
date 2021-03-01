namespace TombIDE.ScriptingStudio.UI
{
	/// <summary>
	/// Depending on the <c>DocumentMode</c>, the <b>Document</b> menu section might contain different items.<br />
	/// Changing this mode is needed because, for example the <c>StringEditor</c> doesn't have a "Reindent Script" functionality,<br />
	/// therefore this option shouldn't be visible to the user.<br />
	/// Changing this mode will therefore only show suitable items for the current editor the user is working in.<br />
	/// This mode is usually changed when switching between documents.<br />
	/// </summary>
	public enum DocumentMode
	{
		None,
		PlainText,
		ClassicScript,
		Lua,
		Strings
	}
}
