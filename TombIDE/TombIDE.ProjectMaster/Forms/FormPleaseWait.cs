using DarkUI.Forms;
using System.Windows.Forms;

namespace TombIDE.ProjectMaster.Forms
{
	public partial class FormPleaseWait : DarkForm
	{
		private const int CP_NOCLOSE_BUTTON = 0x200;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ClassStyle |= CP_NOCLOSE_BUTTON;

				return cp;
			}
		}

		public FormPleaseWait()
		{
			InitializeComponent();
		}
	}
}
