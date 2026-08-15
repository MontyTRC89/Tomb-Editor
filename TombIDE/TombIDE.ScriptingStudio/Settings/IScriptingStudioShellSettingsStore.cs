#nullable enable

using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombIDE.ScriptingStudio.Shortcuts;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombLib.Utils;

namespace TombIDE.ScriptingStudio.Settings;

/// <summary>
/// Stores and loads workspace-level shell settings, including dock layout,
/// editor configuration, and shortcut overrides.
/// </summary>
public interface IScriptingStudioShellSettingsStore
{
	/// <summary>
	/// Creates a new default settings instance for the specified workspace profile.
	/// </summary>
	ScriptingStudioShellWorkspaceSettings CreateDefault(ScriptingWorkspaceProfile workspaceProfile);

	/// <summary>
	/// Loads the persisted settings for the specified workspace profile.
	/// </summary>
	ScriptingStudioShellWorkspaceSettings Load(ScriptingWorkspaceProfile workspaceProfile);

	/// <summary>
	/// Loads persisted settings for a workspace kind and imports legacy settings using the supplied default layout.
	/// </summary>
	ScriptingStudioShellWorkspaceSettings Load(ScriptingWorkspaceKind workspaceKind, DockPanelState defaultLayout);

	/// <summary>
	/// Gets whether Lua is enabled for the specified primary workspace kind.
	/// </summary>
	bool IsLuaEnabled(ScriptingWorkspaceKind workspaceKind);

	/// <summary>
	/// Saves the specified settings for the given workspace kind.
	/// </summary>
	void Save(ScriptingWorkspaceKind workspaceKind, ScriptingStudioShellWorkspaceSettings settings);

	/// <summary>
	/// Atomically saves only the shortcut overrides for a workspace,
	/// preserving all other settings (layout, editor config, etc.).
	/// Returns <see langword="true"/> when the save succeeded.
	/// </summary>
	bool SaveShortcutOverrides(ScriptingWorkspaceKind workspaceKind, ShortcutOverrideCollection overrides);
}

public sealed class ScriptingStudioShellWorkspaceSettings
{
	public string AvalonDockLayoutXml { get; set; } = string.Empty;

	public DockPanelState DockPanelState { get; set; } = new();

	public bool InfoBoxAlwaysOnTop { get; set; } = true;

	public bool InfoBoxCloseTabsOnClose { get; set; }

	public bool IsLegacyImported { get; set; }

	/// <summary>
	/// Gets or sets whether the primary workspace also hosts Lua documents.
	/// The capability is evaluated when the shell is created and takes effect on the next shell session.
	/// </summary>
	public bool LuaEnabled { get; set; }

	public bool IsStatusStripVisible { get; set; } = true;

	public bool IsToolStripVisible { get; set; } = true;

	public bool ReindentOnSave { get; set; }

	public bool ShowCompilerLogsAfterBuild { get; set; } = true;

	public bool UseNewIncludeMethod { get; set; } = true;

	public ShortcutOverrideCollection ShortcutOverrides { get; set; } = new();

	public ScriptingStudioShellWorkspaceSettings Clone() => new()
	{
		AvalonDockLayoutXml = AvalonDockLayoutXml ?? string.Empty,
		DockPanelState = DockPanelStateCloneHelper.Clone(DockPanelState),
		InfoBoxAlwaysOnTop = InfoBoxAlwaysOnTop,
		InfoBoxCloseTabsOnClose = InfoBoxCloseTabsOnClose,
		IsLegacyImported = IsLegacyImported,
		LuaEnabled = LuaEnabled,
		IsStatusStripVisible = IsStatusStripVisible,
		IsToolStripVisible = IsToolStripVisible,
		ReindentOnSave = ReindentOnSave,
		ShowCompilerLogsAfterBuild = ShowCompilerLogsAfterBuild,
		UseNewIncludeMethod = UseNewIncludeMethod,
		ShortcutOverrides = CloneShortcutOverrides(ShortcutOverrides)
	};

	private static ShortcutOverrideCollection CloneShortcutOverrides(ShortcutOverrideCollection source)
	{
		var clone = new ShortcutOverrideCollection { Version = source.Version };

		foreach (ShortcutOverrideEntry entry in source.Overrides)
		{
			clone.Overrides.Add(new ShortcutOverrideEntry
			{
				CommandId = entry.CommandId,
				Bindings = entry.Bindings
					.Select(b => new ShortcutBindingSettings { KeyName = b.KeyName, Modifiers = b.Modifiers })
					.ToList()
			});
		}

		return clone;
	}
}

public sealed class ScriptingStudioShellSettingsDocument
{
	public ScriptingStudioShellWorkspaceSettings ClassicScript { get; set; } = new();

	public ScriptingStudioShellWorkspaceSettings GameFlowScript { get; set; } = new();

	public ScriptingStudioShellWorkspaceSettings Lua { get; set; } = new() { LuaEnabled = true };

	public ScriptingStudioShellWorkspaceSettings TRX { get; set; } = new();

	public ScriptingStudioShellWorkspaceSettings GetWorkspace(ScriptingWorkspaceKind workspaceKind) => workspaceKind switch
	{
		ScriptingWorkspaceKind.ClassicScript => ClassicScript,
		ScriptingWorkspaceKind.GameFlowScript => GameFlowScript,
		ScriptingWorkspaceKind.TRX => TRX,
		ScriptingWorkspaceKind.Lua => Lua,
		_ => throw new NotSupportedException($"Unsupported scripting workspace kind: {workspaceKind}.")
	};

	public void SetWorkspace(ScriptingWorkspaceKind workspaceKind, ScriptingStudioShellWorkspaceSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings);

		switch (workspaceKind)
		{
			case ScriptingWorkspaceKind.ClassicScript:
				ClassicScript = settings;
				break;

			case ScriptingWorkspaceKind.GameFlowScript:
				GameFlowScript = settings;
				break;

			case ScriptingWorkspaceKind.TRX:
				TRX = settings;
				break;

			case ScriptingWorkspaceKind.Lua:
				Lua = settings;
				break;

			default:
				throw new NotSupportedException($"Unsupported scripting workspace kind: {workspaceKind}.");
		}
	}
}

internal sealed class XmlScriptingStudioShellSettingsStore : IScriptingStudioShellSettingsStore
{
	private readonly ScriptingStudioLegacySettingsSnapshot _legacySettingsSnapshot;
	private readonly string _settingsPath;

	public XmlScriptingStudioShellSettingsStore(
		ScriptingStudioLegacySettingsSnapshot legacySettingsSnapshot,
		string? settingsPath = null)
	{
		_legacySettingsSnapshot = legacySettingsSnapshot ?? throw new ArgumentNullException(nameof(legacySettingsSnapshot));
		_settingsPath = settingsPath ?? DefaultSettingsPath;
	}

	public ScriptingStudioShellWorkspaceSettings CreateDefault(ScriptingWorkspaceProfile workspaceProfile)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);

		return new ScriptingStudioShellWorkspaceSettings
		{
			DockPanelState = DockPanelStateCloneHelper.Clone(workspaceProfile.DefaultLayout),
			IsLegacyImported = true
		};
	}

	public ScriptingStudioShellWorkspaceSettings Load(ScriptingWorkspaceProfile workspaceProfile)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);

		ScriptingStudioShellSettingsDocument document = LoadDocument();
		ScriptingStudioShellWorkspaceSettings workspaceSettings = document.GetWorkspace(workspaceProfile.Kind).Clone();

		if (workspaceSettings.IsLegacyImported)
			return workspaceSettings;

		ScriptingStudioShellWorkspaceSettings importedSettings = ImportLegacySettings(workspaceProfile);
		document.SetWorkspace(workspaceProfile.Kind, importedSettings.Clone());
		SaveDocument(document);
		return importedSettings;
	}

	public ScriptingStudioShellWorkspaceSettings Load(ScriptingWorkspaceKind workspaceKind, DockPanelState defaultLayout)
	{
		ArgumentNullException.ThrowIfNull(defaultLayout);

		ScriptingStudioShellSettingsDocument document = LoadDocument();
		ScriptingStudioShellWorkspaceSettings workspaceSettings = document.GetWorkspace(workspaceKind).Clone();

		if (workspaceSettings.IsLegacyImported)
			return workspaceSettings;

		ScriptingStudioShellWorkspaceSettings importedSettings = ImportLegacySettings(workspaceKind, defaultLayout);
		document.SetWorkspace(workspaceKind, importedSettings.Clone());
		SaveDocument(document);
		return importedSettings;
	}

	public void Save(ScriptingWorkspaceKind workspaceKind, ScriptingStudioShellWorkspaceSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings);

		ScriptingStudioShellSettingsDocument document = LoadDocument();
		document.SetWorkspace(workspaceKind, settings.Clone());
		SaveDocument(document);
	}

	public bool IsLuaEnabled(ScriptingWorkspaceKind workspaceKind)
		=> LoadDocument().GetWorkspace(workspaceKind).LuaEnabled;

	private static DockPanelState GetLegacyDockPanelState(ScriptingStudioLegacySettingsSnapshot legacySettingsSnapshot, ScriptingWorkspaceKind workspaceKind)
		=> workspaceKind switch
		{
			ScriptingWorkspaceKind.ClassicScript => DockPanelStateCloneHelper.Clone(legacySettingsSnapshot.ClassicScriptDockPanelState),
			ScriptingWorkspaceKind.GameFlowScript => DockPanelStateCloneHelper.Clone(legacySettingsSnapshot.GameFlowScriptDockPanelState),
			ScriptingWorkspaceKind.TRX => DockPanelStateCloneHelper.Clone(legacySettingsSnapshot.TrxDockPanelState),
			ScriptingWorkspaceKind.Lua => DockPanelStateCloneHelper.Clone(legacySettingsSnapshot.LuaDockPanelState),
			_ => throw new NotSupportedException($"Unsupported scripting workspace kind: {workspaceKind}.")
		};

	private static string GetLegacyLayoutXml(ScriptingStudioLegacySettingsSnapshot legacySettingsSnapshot, ScriptingWorkspaceKind workspaceKind)
		=> workspaceKind switch
		{
			ScriptingWorkspaceKind.ClassicScript => legacySettingsSnapshot.ClassicScriptLayoutXml,
			ScriptingWorkspaceKind.GameFlowScript => legacySettingsSnapshot.GameFlowScriptLayoutXml,
			ScriptingWorkspaceKind.TRX => legacySettingsSnapshot.TrxLayoutXml,
			ScriptingWorkspaceKind.Lua => legacySettingsSnapshot.LuaLayoutXml,
			_ => throw new NotSupportedException($"Unsupported scripting workspace kind: {workspaceKind}.")
		};

	private ScriptingStudioShellWorkspaceSettings ImportLegacySettings(ScriptingWorkspaceProfile workspaceProfile)
		=> ImportLegacySettings(workspaceProfile.Kind, workspaceProfile.DefaultLayout);

	private ScriptingStudioShellWorkspaceSettings ImportLegacySettings(ScriptingWorkspaceKind workspaceKind, DockPanelState defaultLayout)
	{
		ScriptingStudioShellWorkspaceSettings settings = new()
		{
			DockPanelState = DockPanelStateCloneHelper.Clone(defaultLayout),
			IsLegacyImported = true
		};
		settings.UseNewIncludeMethod = _legacySettingsSnapshot.UseNewIncludeMethod;
		settings.ShowCompilerLogsAfterBuild = _legacySettingsSnapshot.ShowCompilerLogsAfterBuild;
		settings.ReindentOnSave = _legacySettingsSnapshot.ReindentOnSave;
		settings.InfoBoxAlwaysOnTop = _legacySettingsSnapshot.InfoBoxAlwaysOnTop;
		settings.InfoBoxCloseTabsOnClose = _legacySettingsSnapshot.InfoBoxCloseTabsOnClose;
		settings.DockPanelState = GetLegacyDockPanelState(_legacySettingsSnapshot, workspaceKind);
		settings.AvalonDockLayoutXml = GetLegacyLayoutXml(_legacySettingsSnapshot, workspaceKind) ?? string.Empty;
		settings.IsLegacyImported = true;
		return settings;
	}

	private ScriptingStudioShellSettingsDocument LoadDocument()
	{
		try
		{
			if (!File.Exists(_settingsPath))
				return new ScriptingStudioShellSettingsDocument();

			return XmlUtils.ReadXmlFile<ScriptingStudioShellSettingsDocument>(_settingsPath);
		}
		catch (Exception) when (File.Exists(_settingsPath))
		{
			return new ScriptingStudioShellSettingsDocument();
		}
	}

	private void SaveDocument(ScriptingStudioShellSettingsDocument document)
	{
		string? directoryPath = Path.GetDirectoryName(_settingsPath);
		if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
			Directory.CreateDirectory(directoryPath);

		XmlUtils.WriteXmlFile(_settingsPath, document);
	}

	public bool SaveShortcutOverrides(ScriptingWorkspaceKind workspaceKind, ShortcutOverrideCollection overrides)
	{
		ArgumentNullException.ThrowIfNull(overrides);

		try
		{
			ScriptingStudioShellSettingsDocument document = LoadDocument();
			ScriptingStudioShellWorkspaceSettings workspaceSettings = document.GetWorkspace(workspaceKind);
			workspaceSettings.ShortcutOverrides = overrides;
			document.SetWorkspace(workspaceKind, workspaceSettings);

			// Write to a temporary file first, then atomically replace.
			string tempPath = _settingsPath + ".tmp";
			XmlUtils.WriteXmlFile(tempPath, document);
			File.Move(tempPath, _settingsPath, overwrite: true);

			return true;
		}
		catch (Exception ex)
		{
			LogManager.GetCurrentClassLogger().Error(ex, "Failed to save shortcut overrides for {0}.", workspaceKind);
			return false;
		}
	}

	private static string DefaultSettingsPath => Path.Combine(DefaultPaths.ConfigsDirectory, "TombIDEScriptingStudioAddonSettings.xml");
}

internal static class DockPanelStateCloneHelper
{
	public static DockPanelState Clone(DockPanelState? state)
	{
		if (state is null)
			return new DockPanelState();

		var clone = new DockPanelState();

		foreach (DockRegionState region in state.Regions ?? [])
			clone.Regions.Add(Clone(region));

		return clone;
	}

	private static DockGroupState Clone(DockGroupState state) => new()
	{
		Contents = new List<string>(state.Contents ?? []),
		Order = state.Order,
		Size = state.Size,
		VisibleContent = state.VisibleContent ?? string.Empty
	};

	private static DockRegionState Clone(DockRegionState state)
	{
		var clone = new DockRegionState(state.Area, state.Size);

		foreach (DockGroupState group in state.Groups ?? [])
			clone.Groups.Add(Clone(group));

		return clone;
	}
}
