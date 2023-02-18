using DarkUI.Docking;
using System;
using System.Windows.Forms;

namespace TombEditor.WPF.ToolWindows
{
    public partial class ToolPalette : UserControl
    {
        public ToolPalette()
        {
            InitializeComponent();
        }

        private void toolBox_SizeChanged(object sender, EventArgs e)
        {
            AutoSize = true;
        }
    }
}
