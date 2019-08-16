using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
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

			tabControl.HideTab("Description");

			UpdateProjectPlugins();
			UpdateTreeView();

			ClearPluginInfo();
		}

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.PluginListsUpdatedEvent)
			{
				UpdateProjectPlugins();
				UpdateTreeView();
			}
		}
		private void UpdateProjectPlugins()
		{
			List<Plugin> projectPlugins = new List<Plugin>();

			foreach (string pluginFile in Directory.GetFiles(_ide.Project.ProjectPath, "plugin_*.dll", SearchOption.TopDirectoryOnly))
			{
				bool pluginFound = false;

				foreach (Plugin availablePlugin in _ide.AvailablePlugins)
				{
					if (Path.GetFileName(availablePlugin.InternalDllPath.ToLower()) == Path.GetFileName(pluginFile.ToLower()))
					{
						projectPlugins.Add(availablePlugin);
						pluginFound = true;
						break;
					}
				}

				if (!pluginFound)
				{
					Plugin plugin = new Plugin
					{
						Name = Path.GetFileName(pluginFile)
					};

					projectPlugins.Add(plugin);
				}
			}

			_ide.Project.InstalledPlugins = projectPlugins;
			XmlHandling.SaveTRPROJ(_ide.Project);
		}

		private void button_ManagePlugins_Click(object sender, EventArgs e)
		{
			using (FormPluginLibrary form = new FormPluginLibrary(_ide))
			{
				DialogResult result = form.ShowDialog(this);

				if (result == DialogResult.OK)
					UpdateTreeView();
			}
		}

		private void UpdateTreeView()
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

			treeView.Invalidate();
		}

		private void treeView_SelectedNodesChanged(object sender, EventArgs e)
		{
			if (treeView.SelectedNodes.Count == 0)
				ClearPluginInfo();
			else
				FillPluginInfo();
		}

		private void ClearPluginInfo()
		{
			button_OpenFolder.Enabled = false;

			panel_Logo.BackgroundImage = null;

			label_NoLogo.Visible = false;
			label_NoInfo.Visible = true;

			textBox_Title.Text = string.Empty;
			textBox_DLLName.Text = string.Empty;

			richTextBox_Description.Text = string.Empty;
			tabControl.HideTab(1);
		}

		private void FillPluginInfo()
		{
			label_NoInfo.Visible = false;

			Plugin plugin = (Plugin)treeView.SelectedNodes[0].Tag;

			textBox_Title.Text = plugin.Name;

			if (string.IsNullOrEmpty(plugin.InternalDllPath))
			{
				textBox_DLLName.Text = Path.GetFileName(treeView.SelectedNodes[0].Text);
				button_OpenFolder.Enabled = false;
			}
			else
			{
				textBox_DLLName.Text = Path.GetFileName(plugin.InternalDllPath);
				button_OpenFolder.Enabled = true;
			}

			try
			{
				bool logoFound = false;

				foreach (string file in Directory.GetFiles(Path.GetDirectoryName(plugin.InternalDllPath)))
				{
					string extension = Path.GetExtension(file.ToLower());

					if (extension == ".jpg" || extension == ".png" || extension == ".bmp" || extension == ".gif")
					{
						panel_Logo.BackgroundImage = Image.FromFile(file);
						logoFound = true;
						break;
					}
				}

				if (!logoFound)
				{
					panel_Logo.BackgroundImage = null;
					label_NoLogo.Visible = true;
				}
				else
					label_NoLogo.Visible = false;

				string descriptionFilePath = Path.Combine(Path.GetDirectoryName(plugin.InternalDllPath), Path.GetFileNameWithoutExtension(plugin.InternalDllPath) + ".txt");

				if (File.Exists(descriptionFilePath))
				{
					richTextBox_Description.Text = File.ReadAllText(descriptionFilePath, Encoding.GetEncoding(1252));
					tabControl.ShowTab(1);
				}
				else
				{
					if (tabControl.SelectedTab.Text == "Description")
						tabControl.SelectedIndex = 0;

					tabControl.HideTab(1);
				}
			}
			catch { }
		}

		private void button_OpenFolder_Click(object sender, EventArgs e)
		{
			Plugin selectedPlugin = (Plugin)treeView.SelectedNodes[0].Tag;
			SharedMethods.OpenFolderInExplorer(Path.GetDirectoryName(selectedPlugin.InternalDllPath));
		}
	}
}
