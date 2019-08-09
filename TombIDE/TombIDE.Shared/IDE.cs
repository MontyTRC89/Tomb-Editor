using System;
using System.Collections.Generic;
using System.Threading;
using TombLib.Projects;

namespace TombIDE.Shared
{
	public interface IIDEEvent { }

	public interface IProjectMasterEvent : IIDEEvent { }

	public class IDE : IDisposable
	{
		// The IDE event
		public event Action<IIDEEvent> IDEEventRaised;

		public void RaiseEvent(IIDEEvent eventObj) =>
			SynchronizationContext.Current.Send(eventObj_ => IDEEventRaised?.Invoke((IIDEEvent)eventObj_), eventObj);

		// The configuration
		public Configuration Configuration { get; }

		public List<Project> AvailableProjects { get; }

		#region ProjectAdded

		public class ProjectAddedEvent : IIDEEvent
		{
			public Project AddedProject { get; internal set; }
		}

		public void AddProjectToList(Project project) =>
			RaiseEvent(new ProjectAddedEvent { AddedProject = project });

		#endregion ProjectAdded

		#region ActiveProjectChanged

		public class ActiveProjectChangedEvent : IIDEEvent // Will probably never be used
		{
			public Project Previous { get; internal set; }
			public Project Current { get; internal set; }
		}

		private Project _project;

		public Project Project
		{
			get { return _project; }
			set
			{
				if (_project != value)
				{
					Project previous = _project;
					_project = value;

					RaiseEvent(new ActiveProjectChangedEvent { Previous = previous, Current = value });
				}
			}
		}

		#endregion ActiveProjectChanged

		#region ActiveProjectRenamed

		public class ActiveProjectRenamedEvent : IIDEEvent
		{ }

		#endregion ActiveProjectRenamed

		#region SelectedIDETabChanged

		public class SelectedIDETabChangedEvent : IIDEEvent
		{
			public string Previous { get; internal set; }
			public string Current { get; internal set; }
		}

		public string SelectedIDETab { get; internal set; }

		/// <summary>
		/// Either "Project Master", "Script Editor" or "Tools"
		/// </summary>
		public void SelectIDETab(string tabName)
		{
			if (SelectedIDETab != tabName)
			{
				string previous = SelectedIDETab;
				SelectedIDETab = tabName;

				RaiseEvent(new SelectedIDETabChangedEvent { Previous = previous, Current = tabName });
			}
		}

		#endregion SelectedIDETabChanged

		#region ProgramButtonsChanged

		public class ProgramButtonsChangedEvent : IIDEEvent
		{ }

		#endregion ProgramButtonsChanged

		#region ProgramClosing

		public class ProgramClosingEvent : IIDEEvent
		{ }

		public bool ClosingCancelled { get; set; }

		#endregion ProgramClosing

		/* IProjectMasterEvent */

		#region LevelAdded

		public class LevelAddedEvent : IProjectMasterEvent
		{
			public ProjectLevel AddedLevel { get; internal set; }
			public List<string> ScriptMessages { get; internal set; }
		}

		public void AddLevelToProject(ProjectLevel projectLevel) => AddLevelToProject(projectLevel, new List<string>());

		public void AddLevelToProject(ProjectLevel projectLevel, List<string> scriptMessages) =>
			RaiseEvent(new LevelAddedEvent { AddedLevel = projectLevel, ScriptMessages = scriptMessages });

		#endregion LevelAdded

		#region SelectedLevelChanged

		public class SelectedLevelChangedEvent : IProjectMasterEvent
		{
			public ProjectLevel Previous { get; internal set; }
			public ProjectLevel Current { get; internal set; }
		}

		private ProjectLevel _selectedLevel;

		public ProjectLevel SelectedLevel
		{
			get { return _selectedLevel; }
			set
			{
				if (_selectedLevel != value)
				{
					ProjectLevel previous = _selectedLevel;
					_selectedLevel = value;

					RaiseEvent(new SelectedLevelChangedEvent { Previous = previous, Current = value });
				}
			}
		}

		#endregion SelectedLevelChanged

		#region SelectedLevelSettingsChanged

		public class SelectedLevelSettingsChangedEvent : IProjectMasterEvent
		{ }

		#endregion SelectedLevelSettingsChanged

		#region ProjectScriptPathChanged

		public class ProjectScriptPathChangedEvent : IProjectMasterEvent
		{
			public string OldPath { get; internal set; }
			public string NewPath { get; internal set; }
		}

		public void ChangeScriptFolder(string newPath)
		{
			string oldPath = Project.ScriptPath;

			if (newPath != oldPath)
				RaiseEvent(new ProjectScriptPathChangedEvent { OldPath = oldPath, NewPath = newPath });
		}

		#endregion ProjectScriptPathChanged

		#region ProjectLevelsPathChanged

		public class ProjectLevelsPathChangedEvent : IProjectMasterEvent
		{
			public string OldPath { get; internal set; }
			public string NewPath { get; internal set; }
		}

		public void ChangeLevelsFolder(string newPath)
		{
			string oldPath = Project.LevelsPath;

			if (newPath != oldPath)
				RaiseEvent(new ProjectLevelsPathChangedEvent { OldPath = oldPath, NewPath = newPath });
		}

		#endregion ProjectLevelsPathChanged

		#region PRJ2FileDeleted

		public class PRJ2FileDeletedEvent : IProjectMasterEvent
		{ }

		#endregion PRJ2FileDeleted

		#region RequestedPresenceCheck

		public class RequestedScriptPresenceCheckEvent : IProjectMasterEvent
		{
			public string LevelName { get; internal set; }
		}

		public bool IsLevelScriptDefined(string levelName)
		{
			RaiseEvent(new RequestedScriptPresenceCheckEvent { LevelName = levelName });
			return LevelScriptDefined;
		}

		public class RequestedLanguageStringPresenceCheckEvent : IProjectMasterEvent
		{
			public string LevelName { get; internal set; }
		}

		public bool IsLanguageStringDefined(string levelName)
		{
			RaiseEvent(new RequestedLanguageStringPresenceCheckEvent { LevelName = levelName });
			return LevelLanguageStringDefined;
		}

		public bool LevelScriptDefined { get; set; }
		public bool LevelLanguageStringDefined { get; set; }

		#endregion RequestedPresenceCheck

		#region RequestedScriptEntryRename

		public class RequestedScriptEntryRenameEvent : IProjectMasterEvent
		{
			public string PreviousName { get; internal set; }
			public string CurrentName { get; internal set; }
		}

		public void RenameSelectedLevelScriptEntry(string newName) =>
			RaiseEvent(new RequestedScriptEntryRenameEvent { PreviousName = SelectedLevel.Name, CurrentName = newName });

		#endregion RequestedScriptEntryRename

		// Construction and destruction
		public IDE(Configuration configuration, List<Project> availableProjects)
		{
			Configuration = configuration;
			AvailableProjects = availableProjects;
		}

		public void Dispose()
		{ }
	}
}
