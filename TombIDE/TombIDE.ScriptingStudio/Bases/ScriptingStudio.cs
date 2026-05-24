using System;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.Services;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Bases
{
	public abstract class ScriptingStudio : StudioBase
	{
		private Action _applyUserSettings;
		private Action<IEditorControl, ConfigurationCollection> _applyEditorSettings;
		private IStudioDocumentCommandHandler _documentCommandHandler;
		private IStudioDocumentCommandStatusProvider _documentCommandStatusProvider;
		private IStudioDocumentStatusStripProvider _documentStatusStripProvider;
		private Action _dockPanelLayoutRestored;
		private IStudioPaneContributionProvider _paneContributionProvider;
		private ScriptingWorkspaceProfile _workspaceProfile;
		private IStudioWorkspaceAutomationProvider _workspaceAutomationProvider;

		protected ScriptingStudio()
			: base(IDE.Instance.Project.GetScriptRootDirectory(), IDE.Instance.Project.GetEngineRootDirectoryPath())
		{
		}

		protected void InitializeHost(
			ScriptingWorkspaceProfile workspaceProfile,
			Action<IEditorControl, ConfigurationCollection> applyEditorSettings,
			Action applyUserSettings,
			IStudioDocumentCommandStatusProvider documentCommandStatusProvider = null,
			IStudioDocumentCommandHandler documentCommandHandler = null,
			IStudioDocumentStatusStripProvider documentStatusStripProvider = null,
			IStudioPaneContributionProvider paneContributionProvider = null,
			IStudioWorkspaceAutomationProvider workspaceAutomationProvider = null,
			Action dockPanelLayoutRestored = null)
		{
			_workspaceProfile = workspaceProfile ?? throw new ArgumentNullException(nameof(workspaceProfile));
			_applyEditorSettings = applyEditorSettings ?? throw new ArgumentNullException(nameof(applyEditorSettings));
			_applyUserSettings = applyUserSettings ?? throw new ArgumentNullException(nameof(applyUserSettings));
			_documentCommandStatusProvider = documentCommandStatusProvider;
			_documentCommandHandler = documentCommandHandler;
			_documentStatusStripProvider = documentStatusStripProvider;
			_paneContributionProvider = paneContributionProvider;
			_workspaceAutomationProvider = workspaceAutomationProvider;
			_dockPanelLayoutRestored = dockPanelLayoutRestored;

			ApplyWorkspaceProfileCommandSurfaceContributions();
			ApplyPaneContributionProvider();
			ApplyWorkspaceProfileStartupPolicy();
		}

		protected sealed override void ApplyUserSettings(IEditorControl editor)
			=> _applyEditorSettings(editor, Configs);

		protected sealed override void ApplyUserSettings()
			=> _applyUserSettings();

		protected sealed override IStudioDocumentCommandStatusProvider DocumentCommandStatusProvider
			=> _documentCommandStatusProvider;

		protected sealed override IStudioDocumentCommandHandler DocumentCommandHandler
			=> _documentCommandHandler;

		protected sealed override IStudioDocumentStatusStripProvider DocumentStatusStripProvider
			=> _documentStatusStripProvider;

		protected sealed override void OnDockPanelLayoutRestored()
			=> _dockPanelLayoutRestored?.Invoke();

		protected sealed override IStudioPaneContributionProvider PaneContributionProvider
			=> _paneContributionProvider;

		protected sealed override ScriptingWorkspaceProfile WorkspaceProfile
			=> _workspaceProfile;

		protected sealed override IStudioWorkspaceAutomationProvider WorkspaceAutomationProvider
			=> _workspaceAutomationProvider;
	}
}