using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor
{
    public partial class FormCamera : DarkForm
    {
        private CameraInstance _instance;

        public FormCamera(CameraInstance instance)
        {
            InitializeComponent();

            _instance = instance;
            ckFixed.Checked = _instance.Fixed;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            _instance.Fixed = ckFixed.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
