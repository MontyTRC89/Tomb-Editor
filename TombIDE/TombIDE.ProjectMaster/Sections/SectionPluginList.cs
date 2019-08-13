using System.Windows.Forms;
using TombIDE.Shared;

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
		}
	}
}
