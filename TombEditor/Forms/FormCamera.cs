using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormCamera : DarkForm
    {
        private readonly CameraInstance _instance;

        public FormCamera(CameraInstance instance)
        {
            InitializeComponent();

            _instance = instance;
            ckFixed.Checked = _instance.Fixed;
            nudMoveTimer.Value = _instance.MoveTimer;

            ckFixed.Enabled      = (instance.Room.Level.Settings.GameVersion >= TRVersion.Game.TR4);
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            _instance.Fixed = ckFixed.Checked;
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
