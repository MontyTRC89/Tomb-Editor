using System.Windows.Forms;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.LevelCompile;

/// <summary>
/// Provides functionality for batch building levels.
/// </summary>
public interface ILevelCompileService
{
	/// <summary>
	/// Performs a batch rebuild of all levels in the project.
	/// </summary>
	/// <param name="project">The project containing levels to rebuild.</param>
	/// <param name="owner">The owner window for dialogs.</param>
	void RebuildAllLevels(IGameProject project, IWin32Window owner);

	/// <summary>
	/// Performs a rebuild of a single level.
	/// </summary>
	/// <param name="level">The level to rebuild.</param>
	void RebuildLevel(ILevelProject level);
}
