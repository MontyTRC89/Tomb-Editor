using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
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
