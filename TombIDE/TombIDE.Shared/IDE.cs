﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace TombIDE.Shared
{
	public interface IIDEEvent { }

	public class IDE : IDisposable
	{
		/* Initialization */

		public event Action<IIDEEvent> IDEEventRaised;

		public void RaiseEvent(IIDEEvent eventObj) =>
			SynchronizationContext.Current.Send(eventObj_ => IDEEventRaised?.Invoke((IIDEEvent)eventObj_), eventObj);

		public IDEConfiguration IDEConfiguration { get; }
		public List<Project> AvailableProjects { get; }
		public List<Plugin> AvailablePlugins { get; }

		/// <summary>
		/// The currently opened TombIDE project.
		/// <para>Note: Set this before the FormMain initialization is finished. DO NOT change it afterwards.</para>
		/// </summary>
		public Project Project { get; set; }

		/* Main IDE events */

		#region SelectedIDETabChanged

		public class SelectedIDETabChangedEvent : IIDEEvent
		{
			public IDETab Previous { get; internal set; }
			public IDETab Current { get; internal set; }
		}

		public IDETab SelectedIDETab { get; private set; }

		public void SelectIDETab(IDETab tab)
		{
			if (SelectedIDETab != tab)
			{
				IDETab previous = SelectedIDETab;
				SelectedIDETab = tab;

				RaiseEvent(new SelectedIDETabChangedEvent { Previous = previous, Current = tab });
			}
		}

		#endregion SelectedIDETabChanged

		#region ProgramButtonsModified

		public class ProgramButtonsModifiedEvent : IIDEEvent { }

		public void ProgramButtonsModified() =>
			RaiseEvent(new ProgramButtonsModifiedEvent());

		#endregion ProgramButtonsModified

		#region ApplicationRestarting

		public class ApplicationRestartingEvent : IIDEEvent { }

		/// <summary>
		/// WARNING: This method doesn't ask IDE.CanClose() for closing permissions !!!
		/// </summary>
		public void RestartApplication() =>
			RaiseEvent(new ApplicationRestartingEvent());

		#endregion ApplicationRestarting

		#region ProgramClosing

		public class ProgramClosingEvent : IIDEEvent
		{
			public bool CanClose { get; set; } // Result
		}

		/// <summary>
		/// Asks the Script Editor if all files are saved and if the application can be safely closed.
		/// </summary>
		public bool CanClose()
		{
			ProgramClosingEvent closingEvent = new ProgramClosingEvent();

			RaiseEvent(closingEvent);

			return closingEvent.CanClose;
		}

		#endregion ProgramClosing

		/* Project Master Events */

		#region SelectedLevelChanged

		/// <summary>
		/// The currently selected level in the "Level List" section.
		/// </summary>
		public ProjectLevel SelectedLevel
		{
			get { return _selectedLevel; }
			set
			{
				if (_selectedLevel != value)
				{
					_selectedLevel = value;
					RaiseEvent(new SelectedLevelChangedEvent());
				}
			}
		}

		private ProjectLevel _selectedLevel;

		public class SelectedLevelChangedEvent : IIDEEvent { }

		#endregion SelectedLevelChanged

		#region SelectedLevelSettingsChanged

		public class SelectedLevelSettingsChangedEvent : IIDEEvent { }

		public void SelectedLevelSettingsChanged() =>
			RaiseEvent(new SelectedLevelSettingsChangedEvent());

		#endregion SelectedLevelSettingsChanged

		#region ProjectScriptPathChanged

		public class ProjectScriptPathChangedEvent : IIDEEvent
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

		public class ProjectLevelsPathChangedEvent : IIDEEvent
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

		#region RequestedPluginListRefresh

		public class RequestedPluginListRefreshEvent : IIDEEvent { }

		public void RefreshPluginLists() =>
			RaiseEvent(new RequestedPluginListRefreshEvent());

		#endregion RequestedPluginListRefresh

		#region PluginListsUpdated

		public class PluginListsUpdatedEvent : IIDEEvent { }

		public void FinishPluginListUpdate() =>
			RaiseEvent(new PluginListsUpdatedEvent());

		#endregion PluginListsUpdated

		#region PRJ2FileDeleted

		public class PRJ2FileDeletedEvent : IIDEEvent
		{ }

		#endregion PRJ2FileDeleted

		/* Script Editor Events */

		#region ScriptEditor_OpenFile

		public class ScriptEditor_OpenFileEvent : IIDEEvent
		{
			public string RequestedFilePath { get; internal set; }
		}

		public void ScriptEditor_OpenFile(string requestedFilePath) =>
			RaiseEvent(new ScriptEditor_OpenFileEvent { RequestedFilePath = requestedFilePath });

		#endregion ScriptEditor_OpenFile

		#region ScriptEditor_SelectObject

		public class ScriptEditor_SelectObjectEvent : IIDEEvent
		{
			public string ObjectName { get; internal set; }
			public ObjectType ObjectType { get; internal set; }
		}

		public void ScriptEditor_SelectObject(string objectName, ObjectType type) =>
			RaiseEvent(new ScriptEditor_SelectObjectEvent { ObjectName = objectName, ObjectType = type });

		#endregion ScriptEditor_SelectObject

		#region ScriptEditor_AppendScriptLines

		public class ScriptEditor_AppendScriptLinesEvent : IIDEEvent
		{
			public List<string> Lines { get; internal set; }
		}

		/// <summary>
		/// Sends a request to the Script Editor to append new lines of code at the end of the main script file.
		/// </summary>
		public void ScriptEditor_AppendScriptLines(List<string> lines) =>
			RaiseEvent(new ScriptEditor_AppendScriptLinesEvent { Lines = lines });

		#endregion ScriptEditor_AppendScriptLines

		#region ScriptEditor_AddNewLevelString

		public class ScriptEditor_AddNewLevelStringEvent : IIDEEvent
		{
			public string LevelName { get; internal set; }
		}

		public void ScriptEditor_AddNewLevelString(string levelName) =>
			RaiseEvent(new ScriptEditor_AddNewLevelStringEvent { LevelName = levelName });

		#endregion ScriptEditor_AddNewLevelString

		#region ScriptEditor_AddNewNGString

		public class ScriptEditor_AddNewNGStringEvent : IIDEEvent
		{
			public string PluginName { get; internal set; }
			public string InternalDllPath { get; internal set; }
		}

		/// <summary>
		/// Sends a request to the Script Editor to add a new ExtraNG string at the end of the main {LANGUAGE}.txt file.
		/// <para>Note: It automatically adds index prefixes, like "0: {STRING}", "1: {STRING}" etc.</para>
		/// </summary>
		public void ScriptEditor_AddNewNGString(string pluginName, string internalDllPath) =>
			RaiseEvent(new ScriptEditor_AddNewNGStringEvent { PluginName = pluginName, InternalDllPath = internalDllPath });

		#endregion ScriptEditor_AddNewNGString

		#region ScriptEditor_ContentChanged

		public class ScriptEditor_ContentChangedEvent : IIDEEvent { }

		public void ScriptEditor_IndicateExternalChange() =>
			RaiseEvent(new ScriptEditor_ContentChangedEvent());

		#endregion ScriptEditor_ContentChanged

		#region ScriptEditor_PresenceCheck

		public class ScriptEditor_ScriptPresenceCheckEvent : IIDEEvent
		{
			public string LevelName { get; internal set; }
		}

		public bool ScriptEditor_IsScriptDefined(string levelName)
		{
			RaiseEvent(new ScriptEditor_ScriptPresenceCheckEvent { LevelName = levelName });
			return ScriptDefined;
		}

		public class ScriptEditor_StringPresenceCheckEvent : IIDEEvent
		{
			public string LevelName { get; internal set; }
		}

		public bool ScriptEditor_IsStringDefined(string levelName)
		{
			RaiseEvent(new ScriptEditor_StringPresenceCheckEvent { LevelName = levelName });
			return StringDefined;
		}

		public bool ScriptDefined { get; set; }
		public bool StringDefined { get; set; }

		#endregion ScriptEditor_PresenceCheck

		#region ScriptEditor_RenameLevel

		public class ScriptEditor_RenameLevelEvent : IIDEEvent
		{
			public string OldName { get; internal set; }
			public string NewName { get; internal set; }
		}

		public void ScriptEditor_RenameLevel(string targetLevelName, string newName) =>
			RaiseEvent(new ScriptEditor_RenameLevelEvent { OldName = targetLevelName, NewName = newName });

		#endregion ScriptEditor_RenameLevel

		#region ScriptEditor_OpenReferenceDescription

		public class ScriptEditor_OpenReferenceDescriptionEvent : IIDEEvent
		{
			public string ReferenceName { get; internal set; }
			public ReferenceType ReferenceType { get; internal set; }
		}

		public void ScriptEditor_OpenReferenceDescription(string referenceName, ReferenceType type) =>
			RaiseEvent(new ScriptEditor_OpenReferenceDescriptionEvent { ReferenceName = referenceName, ReferenceType = type });

		#endregion ScriptEditor_OpenReferenceDescription

		// Construction and destruction
		public IDE(IDEConfiguration ideConfiguration, List<Project> availableProjects, List<Plugin> availablePlugins)
		{
			IDEConfiguration = ideConfiguration;
			AvailableProjects = availableProjects;
			AvailablePlugins = availablePlugins;
		}

		public void Dispose()
		{ }
	}
}
