using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TombEditor.Controls
{
    public partial class TriggerManager : UserControl
    {
        public TriggerManager()
        {
            InitializeComponent();

            // Link options list
            //tabbedContainer.LinkedControl = optionsList;
        }

        private void SelectTriggerMode() => tabbedContainer.SelectedIndex = rbLevelScript.Checked ? 0 : 1;

        private void rbLevelScript_CheckedChanged(object sender, EventArgs e) => SelectTriggerMode();
        private void rbConstructor_CheckedChanged(object sender, EventArgs e) => SelectTriggerMode();
    }
}
