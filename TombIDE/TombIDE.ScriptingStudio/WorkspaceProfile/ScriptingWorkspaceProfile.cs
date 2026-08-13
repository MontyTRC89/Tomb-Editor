using System;
using System.Collections.Generic;
using System.Linq;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared.Docking;
using TombLib.LevelData;

namespace TombIDE.ScriptingStudio.WorkspaceProfile;

public enum ScriptingWorkspaceKind
{
	ClassicScript,
	GameFlowScript,
	TRX,
	Lua
}

public enum ScriptingSettingsPageKind
{
	ClassicScript,
	GameFlowScript,
	TRX,
	Lua
}

public sealed class ScriptingWorkspaceSettingsPage
{
	public ScriptingWorkspaceSettingsPage(ScriptingSettingsPageKind kind, string title)
		: this(kind, title, [])
	{ }

	public ScriptingWorkspaceSettingsPage(ScriptingSettingsPageKind kind, string title, IReadOnlyList<DocumentMode> documentModes)
	{
		Kind = kind;
		Title = title ?? throw new ArgumentNullException(nameof(title));
		DocumentModes = documentModes ?? [];
	}

	public ScriptingSettingsPageKind Kind { get; }

	public string Title { get; }

	public IReadOnlyList<DocumentMode> DocumentModes { get; }

	public bool Matches(DocumentMode documentMode)
	{
		return documentMode == DocumentMode.None
			|| DocumentModes.Count == 0
			|| DocumentModes.Contains(documentMode);
	}
}

public sealed class ScriptingWorkspaceViewContribution
{
	public ScriptingWorkspaceViewContribution(UICommand command)
	{
		Command = command;
	}

	public UICommand Command { get; }
}

public sealed class ScriptingWorkspaceProfile
{
	private readonly Func<string> _loadAvalonDockLayoutXml;
	private readonly Func<DockPanelState> _loadDockPanelState;
	private readonly Action<IEditorDocumentController> _registerEditors;
	private readonly Action<string> _saveAvalonDockLayoutXml;

	public ScriptingWorkspaceProfile(
		ScriptingWorkspaceKind kind,
		TRVersion.Game gameVersion,
		IReadOnlyList<DocumentMode> allowedDocumentModes,
		string initialFilePath,
		IReadOnlyList<ScriptingWorkspaceViewContribution> viewContributions,
		IReadOnlyList<StudioStatusStripSegment> statusStripSegments,
		IReadOnlyList<ScriptingWorkspaceSettingsPage> settingsPages,
		IReadOnlyList<StudioToolStripItem> menuStripContributions,
		IReadOnlyList<StudioToolStripItem> toolStripContributions,
		string fileExplorerFilter,
		string fileExplorerExcludedDirectoryFilter,
		string commentPrefix,
		bool supportsBuild,
		bool supportsDocumentation,
		DockPanelState defaultLayout,
		Action<IEditorDocumentController> registerEditors,
		Func<DockPanelState> loadDockPanelState,
		Func<string> loadAvalonDockLayoutXml,
		Action<string> saveAvalonDockLayoutXml)
	{
		Kind = kind;
		GameVersion = gameVersion;
		AllowedDocumentModes = allowedDocumentModes ?? Array.Empty<DocumentMode>();
		InitialFilePath = initialFilePath ?? string.Empty;
		ViewContributions = viewContributions ?? Array.Empty<ScriptingWorkspaceViewContribution>();
		StatusStripSegments = statusStripSegments ?? Array.Empty<StudioStatusStripSegment>();
		SettingsPages = settingsPages ?? Array.Empty<ScriptingWorkspaceSettingsPage>();
		MenuStripContributions = menuStripContributions ?? Array.Empty<StudioToolStripItem>();
		ToolStripContributions = toolStripContributions ?? Array.Empty<StudioToolStripItem>();
		FileExplorerFilter = fileExplorerFilter ?? string.Empty;
		FileExplorerExcludedDirectoryFilter = fileExplorerExcludedDirectoryFilter ?? string.Empty;
		CommentPrefix = commentPrefix ?? string.Empty;
		SupportsBuild = supportsBuild;
		SupportsDocumentation = supportsDocumentation;
		DefaultLayout = defaultLayout;
		_registerEditors = registerEditors ?? throw new ArgumentNullException(nameof(registerEditors));
		_loadDockPanelState = loadDockPanelState ?? throw new ArgumentNullException(nameof(loadDockPanelState));
		_loadAvalonDockLayoutXml = loadAvalonDockLayoutXml ?? throw new ArgumentNullException(nameof(loadAvalonDockLayoutXml));
		_saveAvalonDockLayoutXml = saveAvalonDockLayoutXml ?? throw new ArgumentNullException(nameof(saveAvalonDockLayoutXml));
	}

	public ScriptingWorkspaceKind Kind { get; }

	public TRVersion.Game GameVersion { get; }

	public IReadOnlyList<DocumentMode> AllowedDocumentModes { get; }

	public string InitialFilePath { get; }

	public IReadOnlyList<ScriptingWorkspaceSettingsPage> SettingsPages { get; }

	public IReadOnlyList<ScriptingWorkspaceViewContribution> ViewContributions { get; }

	public IReadOnlyList<StudioStatusStripSegment> StatusStripSegments { get; }

	public IReadOnlyList<StudioToolStripItem> MenuStripContributions { get; }

	public IReadOnlyList<StudioToolStripItem> ToolStripContributions { get; }

	public string FileExplorerFilter { get; }

	public string FileExplorerExcludedDirectoryFilter { get; }

	public string CommentPrefix { get; }

	public bool SupportsBuild { get; }

	public bool SupportsDocumentation { get; }

	public DockPanelState DefaultLayout { get; }

	public void RegisterEditors(IEditorDocumentController documentController)
	{
		ArgumentNullException.ThrowIfNull(documentController);

		_registerEditors(documentController);
	}

	public DockPanelState LoadDockPanelState()
		=> _loadDockPanelState();

	public string LoadAvalonDockLayoutXml()
		=> _loadAvalonDockLayoutXml();

	public void SaveAvalonDockLayoutXml(string layoutXml)
		=> _saveAvalonDockLayoutXml(layoutXml);

	public bool SupportsView(UICommand command)
		=> ViewContributions.Any(contribution => contribution.Command == command);
}
