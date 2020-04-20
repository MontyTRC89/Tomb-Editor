using DarkUI.Forms;
using System.ComponentModel;

namespace TombIDE.ScriptEditor
{
	public partial class FormNGCompilingStatus : DarkForm
	{
		public FormNGCompilingStatus()
		{
			InitializeComponent();
		}

		public void ShowDebugMode()
		{
			Text = "DEBUG MODE";

			label_CompileInfo_01.Visible = false;
			label_CompileInfo_02.Visible = false;
			label_Compiling.Visible = false;

			label_Debug.Visible = true;
			label_DebugInfo.Visible = true;
		}

		public void ShowCompilingMode()
		{
			Text = "Compiling NG Script...";

			label_Debug.Visible = false;
			label_DebugInfo.Visible = false;

			label_CompileInfo_01.Visible = true;
			label_CompileInfo_02.Visible = true;
			label_Compiling.Visible = true;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Hide();
			e.Cancel = true;

			base.OnClosing(e);
		}
	}
}
