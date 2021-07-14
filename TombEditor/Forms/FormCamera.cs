using System;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormCamera : DarkForm
    {
        private readonly CameraInstance _instance;
        private readonly Editor _editor = Editor.Instance;

        public FormCamera(CameraInstance instance)
        {
            InitializeComponent();

            _instance = instance;
            ckFixed.Checked = _instance.Fixed;
            nudMoveTimer.Value = _instance.MoveTimer;

            ckFixed.Enabled      = (instance.Room.Level.Settings.GameVersion >= TRVersion.Game.TR4);
            nudMoveTimer.Enabled = (instance.Room.Level.Settings.GameVersion <= TRVersion.Game.TR2);

            if (_editor.Level.Settings.GameVersion == TRVersion.Game.TombEngine)
            {
                tbLuaName.Text = _instance.LuaName;
            }
            else
            {
                labelLuaName.Visible = false;
                tbLuaName.Visible = false;
            }
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            if (_editor.Level.Settings.GameVersion == TRVersion.Game.TombEngine)
            {
                if (!_instance.TrySetLuaName(tbLuaName.Text))
                {
                    DarkMessageBox.Show(this, "The value of Lua Name is already taken by another object", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            _instance.Fixed = ckFixed.Checked;
            _instance.MoveTimer = (byte)nudMoveTimer.Value;

            if (_editor.Level.Settings.GameVersion == TRVersion.Game.TombEngine)
            {
                _instance.LuaName = tbLuaName.Text;
            }

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
