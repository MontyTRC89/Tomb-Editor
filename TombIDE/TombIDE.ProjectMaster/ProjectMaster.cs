using System;
using System.Drawing;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster
{
	public partial class ProjectMaster : UserControl
	{
		private IDE _ide;

		#region Initialization

		public ProjectMaster()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;

			// Initialize the sections
			section_ProjectInfo.Initialize(_ide);
			section_PluginList.Initialize(_ide);

			if (_ide.Project.GameVersion == TRVersion.Game.TR4)
			{
				button_ShowPlugins.Enabled = false;
				button_ShowPlugins.Visible = false;

				splitContainer_Info.Panel2Collapsed = true;
			}
			else if (_ide.IDEConfiguration.PluginsPanelHidden)
			{
				button_ShowPlugins.Enabled = true;
				button_ShowPlugins.Visible = true;

				splitContainer_Info.Panel2Collapsed = true;
				splitContainer_Info.Panel1.Padding = new Padding(30, 30, 30, 30);
			}
			else if (!_ide.IDEConfiguration.PluginsPanelHidden)
			{
				button_ShowPlugins.Enabled = false;
				button_ShowPlugins.Visible = false;

				splitContainer_Info.Panel1.Padding = new Padding(30, 30, 30, 10);
				splitContainer_Info.Panel2Collapsed = false;
			}
		}

		#endregion Initialization

		#region Events

		private void button_ShowPlugins_Click(object sender, EventArgs e)
		{
			splitContainer_Info.Panel1.Padding = new Padding(30, 30, 30, 10);
			splitContainer_Info.Panel2Collapsed = false;

			button_ShowPlugins.Enabled = false;
			button_ShowPlugins.Visible = false;

			_ide.IDEConfiguration.PluginsPanelHidden = false;
			_ide.IDEConfiguration.Save();
		}

		private void button_HidePlugins_Click(object sender, EventArgs e)
		{
			int prevPanelHeight = splitContainer_Info.Panel1.Height;

			splitContainer_Info.Panel2Collapsed = true;
			splitContainer_Info.Panel1.Padding = new Padding(30, 30, 30, 30);

			button_ShowPlugins.Location = new Point(button_ShowPlugins.Location.X, prevPanelHeight - 32 - 45);

			button_ShowPlugins.Enabled = true;
			button_ShowPlugins.Visible = true;

			_ide.IDEConfiguration.PluginsPanelHidden = true;
			_ide.IDEConfiguration.Save();

			animationTimer.Start();
		}

		private void animationTimer_Tick(object sender, EventArgs e)
		{
			button_ShowPlugins.Location = new Point(button_ShowPlugins.Location.X, button_ShowPlugins.Location.Y + 16);

			if (button_ShowPlugins.Location.Y >= splitContainer_Info.Panel1.Height - (32 + 45))
			{
				button_ShowPlugins.Location = new Point(button_ShowPlugins.Location.X, splitContainer_Info.Panel1.Height - (32 + 45));
				animationTimer.Stop();
			}
		}

		#endregion Events
	}
}
