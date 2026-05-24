using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.CommandSurface;

public interface IStudioDocumentCommandSurfaceProvider
{
	IReadOnlyList<StudioToolStripItem> GetContextMenuItems(IEditorControl editor, DocumentMode documentMode);

	IReadOnlyList<StudioToolStripItem> GetMenuStripItems(IEditorControl editor, DocumentMode documentMode);

	IReadOnlyList<StudioToolStripItem> GetToolStripItems(IEditorControl editor, DocumentMode documentMode);
}