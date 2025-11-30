using System;
using TombIDE.ProjectMaster.Services.EngineUpdate;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.EngineVersion;

public sealed class EngineVersionService : IEngineVersionService
{
	private readonly IEngineUpdateServiceFactory _updateServiceFactory;

	public EngineVersionService(IEngineUpdateServiceFactory updateServiceFactory)
		=> _updateServiceFactory = updateServiceFactory ?? throw new ArgumentNullException(nameof(updateServiceFactory));

	public EngineVersionInfo GetVersionInfo(IGameProject project)
	{
		var info = new EngineVersionInfo();

		// TR4 doesn't have version tracking
		if (project.GameVersion is TRVersion.Game.TR4)
		{
			info.CurrentVersion = null;
			info.LatestVersion = null;
			info.SupportsAutoUpdate = false;

			return info;
		}

		info.CurrentVersion = project.GetCurrentEngineVersion();
		info.LatestVersion = project.GetLatestEngineVersion();

		// Check if auto-update is supported
		var updateService = _updateServiceFactory.GetUpdateService(project.GameVersion);

		if (updateService is not null && info.CurrentVersion is not null)
		{
			info.SupportsAutoUpdate = updateService.CanAutoUpdate(info.CurrentVersion);
			info.AutoUpdateBlockReason = updateService.GetAutoUpdateBlockReason(info.CurrentVersion);
		}
		else
		{
			info.SupportsAutoUpdate = false;
		}

		return info;
	}
}
