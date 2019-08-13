using DarkUI.Controls;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.Projects;

namespace TombIDE.ProjectMaster
{
	public partial class SectionPluginList : UserControl
	{
		private IDE _ide;

		public SectionPluginList()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;
			_ide.IDEEventRaised += OnIDEEventRaised;
		}

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.PluginDeletedEvent)
			{
				UpdatePluginList();
			}
		}

		private void button_ManagePlugins_Click(object sender, System.EventArgs e)
		{
			using (FormPluginLibrary form = new FormPluginLibrary(_ide))
			{
				form.ShowDialog(this);
			}
		}

		private void UpdatePluginList()
		{
			List<Plugin> installedPlugins = new List<Plugin>();

			foreach (Plugin plugin in _ide.AvailablePlugins)
			{
				string localDllFilePath = Path.Combine(_ide.Project.ProjectPath, Path.GetFileName(plugin.InternalDllPath));

				if (File.Exists(localDllFilePath))
					installedPlugins.Add(plugin);
			}

			treeView.Nodes.Clear();

			foreach (Plugin plugin in installedPlugins)
			{
				DarkTreeNode node = new DarkTreeNode
				{
					Text = plugin.Name,
					Tag = plugin
				};

				treeView.Nodes.Add(node);
			}
		}

		private void treeView_SelectedNodesChanged(object sender, System.EventArgs e)
		{
			UpdatePluginInfo();
		}

		private void UpdatePluginInfo()
		{
			if (treeView.SelectedNodes.Count == 0)
				return;

			try
			{
				Plugin plugin = (Plugin)treeView.SelectedNodes[0].Tag;

				textBox_Title.Text = plugin.Name;
				textBox_DLLName.Text = Path.GetFileName(plugin.InternalDllPath);

				panel_Logo.BackgroundImage = Image.FromFile(Directory.GetFiles(Path.GetDirectoryName(plugin.InternalDllPath), "*.jpg").First());
			}
			catch { }
		}
	}
}
