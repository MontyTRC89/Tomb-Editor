namespace TombLib.Scripting.Enums
{
	public enum EditorType
	{
		/// <summary>
		/// Automatically detects the most suitable <c>EditorType</c>, depending on the file contents (if programmed ofc.).<br />
		/// Use this value in <c>EditorTypeHelper.GetDefaultEditorType()</c> in order to do it yourself.
		/// </summary>
		Default,

		Text,
		Strings
	}
}
