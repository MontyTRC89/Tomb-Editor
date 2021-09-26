using DarkUI.Forms;
using System;
using System.Drawing;

namespace TombIDE
{
	public partial class FormEngineHelp : DarkForm
	{
		public FormEngineHelp(Point location)
		{
			InitializeComponent();

			Location = location;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Close();
        }

        private void label_Click(object sender, EventArgs e) => Close();
    }
}
