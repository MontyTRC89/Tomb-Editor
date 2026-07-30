using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.CommandSurface;

/// <summary>
/// Handles the execution of UI commands for the active studio document.
/// </summary>
public interface IStudioDocumentCommandHandler
{
	/// <summary>
	/// Attempts to handle the specified command.
	/// </summary>
	/// <param name="command">The command to handle.</param>
	/// <returns><see langword="true"/> if the command was handled; otherwise, <see langword="false"/>.</returns>
	bool TryHandle(UICommand command);
}
