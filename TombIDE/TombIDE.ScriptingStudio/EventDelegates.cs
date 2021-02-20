using TombIDE.ScriptingStudio.Objects;

namespace TombIDE.ScriptingStudio
{
	public delegate void FileOpenedEventHandler(object sender, FileOpenedEventArgs e);

	public delegate void ReferenceDefinitionRequestedEventHandler(object sender, ReferenceDefinitionEventArgs e);
}
