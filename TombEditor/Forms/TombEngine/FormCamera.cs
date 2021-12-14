using System;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms.TombEngine
{
    public partial class FormCamera : DarkForm
    {
        private readonly CameraInstance _instance;
        private readonly Editor _editor = Editor.Instance;

        public FormCamera(CameraInstance instance)
        {
            InitializeComponent();

            _instance = instance;
            ckFixed.Checked = _instance.CameraMode == CameraInstanceMode.Locked;
            nudMoveTimer.Value = _instance.MoveTimer;
            tbLuaName.Text = _instance.LuaName;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            if (!_instance.TrySetLuaName(tbLuaName.Text, this)) 
                return;

            _instance.CameraMode = ckFixed.Checked ? CameraInstanceMode.Locked : CameraInstanceMode.Default;
            _instance.MoveTimer = (byte)nudMoveTimer.Value;

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
