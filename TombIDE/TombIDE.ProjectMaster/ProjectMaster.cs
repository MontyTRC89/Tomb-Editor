using System.Windows.Forms;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster
{
	public partial class ProjectMaster : UserControl
	{
		public ProjectMaster()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
			=> section_ProjectInfo.Initialize(ide);
	}
}
