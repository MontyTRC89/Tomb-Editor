using System;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Bases;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.ToolWindows;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Tomb1Main.Parsers;
using TombLib.Scripting.Tomb1Main.Writers;

namespace TombIDE.ScriptingStudio
{
	public sealed class LuaStudio : StudioBase
	{
		public override StudioMode StudioMode => StudioMode.Tomb1Main;

		#region Construction

		public LuaStudio() : base(IDE.Global.Project.ScriptPath, IDE.Global.Project.EnginePath)
		{
			DockPanelState = IDE.Global.IDEConfiguration.T1M_DockPanelState;

			FileExplorer.Filter = "*.lua";
			FileExplorer.CommentPrefix = "--";

			EditorTabControl.CheckPreviousSession();

			string initialFilePath = PathHelper.GetScriptFilePath(IDE.Global.Project.ScriptPath, true);

			if (!string.IsNullOrWhiteSpace(initialFilePath))
				EditorTabControl.OpenFile(initialFilePath);
		}

		#endregion Construction

		#region IDE Events

		protected override void OnIDEEventRaised(IIDEEvent obj)
		{
			base.OnIDEEventRaised(obj);

			IDEEvent_HandleSilentActions(obj);
		}

		private void IDEEvent_HandleSilentActions(IIDEEvent obj)
		{
			if (obj is IDE.ProgramClosingEvent)
			{
				IDE.Global.IDEConfiguration.Lua_DockPanelState = DockPanel.GetDockPanelState();
				IDE.Global.IDEConfiguration.Save();
			}
		}

		protected override void RestoreDefaultLayout()
		{
			DockPanelState = DefaultLayouts.Tomb1MainLayout;

			DockPanel.RemoveContent();
			DockPanel.RestoreDockPanelState(DockPanelState, FindDockContentByKey);
		}

		#endregion IDE Events

		#region Other methods

		protected override void ApplyUserSettings(IEditorControl editor)
			=> editor.UpdateSettings(Configs.Lua);

		protected override void ApplyUserSettings()
		{
			foreach (TabPage tab in EditorTabControl.TabPages)
				ApplyUserSettings(EditorTabControl.GetEditorOfTab(tab));

			UpdateSettings();
		}

		protected override void Build()
		{
			// Nothing.
		}

		#endregion Other methods
	}
}
