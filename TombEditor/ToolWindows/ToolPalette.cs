using DarkUI.Docking;
using System;

namespace TombEditor.ToolWindows
{
    public partial class ToolPalette : DarkToolWindow
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
