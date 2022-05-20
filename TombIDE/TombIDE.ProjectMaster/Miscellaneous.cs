using System.Windows.Forms;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster
{
	public partial class Miscellaneous : UserControl
	{
		public Miscellaneous()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
			=> section_ProjectInfo.Initialize(ide);
	}
}
