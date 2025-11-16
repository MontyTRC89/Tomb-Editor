using System;
using System.Windows.Forms;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.EngineUpdate;

/// <summary>
/// Represents a service that can update a specific game engine version.
/// </summary>
public interface IEngineUpdateService
{
	/// <summary>
	/// Checks if the current engine version can be auto-updated.
	/// </summary>
	/// <param name="currentVersion">The current engine version installed in the project.</param>
	/// <returns><see langword="true"/> if auto-update is supported for this version; otherwise, <see langword="false"/>.</returns>
	bool CanAutoUpdate(Version currentVersion);

	/// <summary>
	/// Gets the reason why auto-update is not supported, if applicable.
	/// </summary>
	/// <param name="currentVersion">The current engine version installed in the project.</param>
	/// <returns>A message explaining why auto-update is not supported, or <see langword="null"/> if it is supported.</returns>
	string? GetAutoUpdateBlockReason(Version currentVersion);

	/// <summary>
	/// Performs the engine update operation.
	/// </summary>
	/// <param name="project">The project to update.</param>
	/// <param name="currentVersion">The current engine version.</param>
	/// <param name="latestVersion">The latest available engine version.</param>
	/// <param name="owner">The owner window for dialogs.</param>
	/// <returns><see langword="true"/> if the update was successful; otherwise, <see langword="false"/>.</returns>
	bool UpdateEngine(IGameProject project, Version currentVersion, Version latestVersion, IWin32Window owner);
}
