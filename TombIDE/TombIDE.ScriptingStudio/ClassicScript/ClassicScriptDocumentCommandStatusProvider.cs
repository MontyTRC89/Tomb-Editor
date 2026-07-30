using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.ClassicScript
{
	public sealed class ClassicScriptDocumentCommandStatusProvider : IStudioDocumentCommandStatusProvider
	{
		public bool TryGetEnabled(IEditorControl editor, UICommand command, out bool isEnabled)
		{
			bool hasClassicScriptEditor = editor is ClassicScriptEditor;

			switch (command)
			{
				case UICommand.TypeFirstAvailableId:
				case UICommand.NewFileAtCaret:
					isEnabled = hasClassicScriptEditor;
					return true;

				default:
					isEnabled = false;
					return false;
			}
		}
	}
}
