using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.CommandSurface
{
	public interface IStudioDocumentCommandStatusProvider
	{
		bool TryGetEnabled(IEditorControl editor, UICommand command, out bool isEnabled);
	}
}