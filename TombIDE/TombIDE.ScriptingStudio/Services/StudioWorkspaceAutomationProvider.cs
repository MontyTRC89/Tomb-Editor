using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.Services
{
	public interface IStudioWorkspaceAutomationProvider
	{
		void HandleIDEEvent(IIDEEvent ideEvent);

		void Build();

		void ShowDocumentation();
	}
}