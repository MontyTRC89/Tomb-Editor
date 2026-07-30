using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.CommandSurface;

/// <summary>
/// Provides the enabled-status of UI commands for the active editor document.
/// </summary>
public interface IStudioDocumentCommandStatusProvider
{
	/// <summary>
	/// Attempts to determine whether the specified command is currently enabled for the given editor.
	/// </summary>
	/// <param name="editor">The editor to evaluate.</param>
	/// <param name="command">The command to check.</param>
	/// <param name="isEnabled">When this method returns, contains the enabled state if determined.</param>
	/// <returns><see langword="true"/> if the enabled state was determined; otherwise, <see langword="false"/>.</returns>
	bool TryGetEnabled(IEditorControl editor, UICommand command, out bool isEnabled);
}
