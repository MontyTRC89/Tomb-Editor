using TombIDE.ScriptEditor.Objects;

namespace TombIDE.ScriptEditor
{
	public delegate void FileOpenedEventHandler(object sender, FileOpenedEventArgs e);

	public delegate void ReferenceDefinitionRequestedEventHandler(object sender, ReferenceDefinitionEventArgs e);
}
