using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.CommandSurface
{
	public interface IStudioDocumentCommandHandler
	{
		bool TryHandle(UICommand command);
	}
}