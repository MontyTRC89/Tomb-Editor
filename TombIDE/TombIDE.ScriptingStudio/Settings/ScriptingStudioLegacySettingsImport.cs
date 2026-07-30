#nullable enable

using System;
using TombIDE.Shared;
using TombIDE.Shared.Docking;

namespace TombIDE.ScriptingStudio.Settings;

internal static class ScriptingStudioLegacySettingsImport
{
	public static ScriptingStudioLegacySettingsSnapshot CreateSnapshot(IDEConfiguration ideConfiguration)
	{
		ArgumentNullException.ThrowIfNull(ideConfiguration);

		return new ScriptingStudioLegacySettingsSnapshot
		{
			ClassicScriptDockPanelState = DockPanelStateCloneHelper.Clone(ideConfiguration.CS_DockPanelState),
			ClassicScriptLayoutXml = ideConfiguration.CS_AvalonDockLayoutXml ?? string.Empty,
			GameFlowScriptDockPanelState = DockPanelStateCloneHelper.Clone(ideConfiguration.GFL_DockPanelState),
			GameFlowScriptLayoutXml = ideConfiguration.GFL_AvalonDockLayoutXml ?? string.Empty,
			TrxDockPanelState = DockPanelStateCloneHelper.Clone(ideConfiguration.TRX_DockPanelState),
			TrxLayoutXml = ideConfiguration.TRX_AvalonDockLayoutXml ?? string.Empty,
			LuaDockPanelState = DockPanelStateCloneHelper.Clone(ideConfiguration.Lua_DockPanelState),
			LuaLayoutXml = ideConfiguration.Lua_AvalonDockLayoutXml ?? string.Empty,
			UseNewIncludeMethod = ideConfiguration.UseNewIncludeMethod,
			ShowCompilerLogsAfterBuild = ideConfiguration.ShowCompilerLogsAfterBuild,
			ReindentOnSave = ideConfiguration.ReindentOnSave,
			InfoBoxAlwaysOnTop = ideConfiguration.InfoBox_AlwaysOnTop,
			InfoBoxCloseTabsOnClose = ideConfiguration.InfoBox_CloseTabsOnClose
		};
	}
}

internal sealed class ScriptingStudioLegacySettingsSnapshot
{
	public DockPanelState ClassicScriptDockPanelState { get; set; } = new();

	public string ClassicScriptLayoutXml { get; set; } = string.Empty;

	public DockPanelState GameFlowScriptDockPanelState { get; set; } = new();

	public string GameFlowScriptLayoutXml { get; set; } = string.Empty;

	public bool InfoBoxAlwaysOnTop { get; set; } = true;

	public bool InfoBoxCloseTabsOnClose { get; set; }

	public DockPanelState LuaDockPanelState { get; set; } = new();

	public string LuaLayoutXml { get; set; } = string.Empty;

	public bool ReindentOnSave { get; set; }

	public bool ShowCompilerLogsAfterBuild { get; set; } = true;

	public DockPanelState TrxDockPanelState { get; set; } = new();

	public string TrxLayoutXml { get; set; } = string.Empty;

	public bool UseNewIncludeMethod { get; set; } = true;
}
