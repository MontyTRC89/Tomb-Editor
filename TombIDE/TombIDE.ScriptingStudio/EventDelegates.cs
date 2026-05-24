using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.DocumentOutline;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.FileExplorer;
using TombLib.Scripting.UI.DataGrid;

namespace TombLib.Scripting
{
	public delegate void FindReplaceEventHandler(object sender, FindReplaceEventArgs e);

	public delegate void CellValueChangedEventHandler(object sender, CellContentChangedEventArgs e);

	public delegate void ObjectClickedEventHandler(object sender, ObjectClickedEventArgs e);
}

namespace TombIDE.ScriptingStudio
{
	public delegate void FileOpenedEventHandler(object sender, FileOpenedEventArgs e);

	public delegate void ReferenceDefinitionRequestedEventHandler(object sender, ReferenceDefinitionEventArgs e);
}
