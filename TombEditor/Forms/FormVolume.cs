using DarkUI.Forms;
using System;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormVolume : DarkForm
    {
        private Level _level;
        private TriggerVolumeInstance _volume;

        private VolumeScriptInstance _backupScripts;
        private bool _backupState;
        private int prevIndex = -1;

        public FormVolume(Level level, TriggerVolumeInstance volume)
        {
            InitializeComponent();

            _level = level;
            _volume = volume;
            _backupScripts = _volume.Scripts.Clone();
            _backupState = _volume.Enabled;

            cmbEvent.SelectedIndex = 0; // Select first script
            tbName.Text = _volume.Scripts.Name;
            cbEnabled.Checked = _volume.Enabled;
        }

        private void SaveCurrentScript()
        {
            switch (prevIndex)
            {
                case 0:
                    _volume.Scripts.OnEnter = tbScript.Code;
                    break;
                case 1:
                    _volume.Scripts.OnLeave = tbScript.Code;
                    break;
                case 2:
                    _volume.Scripts.OnInside = tbScript.Code;
                    break;
                case 3:
                    _volume.Scripts.Environment = tbScript.Code;
                    break;
            }
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            SaveCurrentScript();
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            _volume.Scripts = _backupScripts;
            _volume.Enabled = _backupState;
            Close();
        }

        private void cmbEvent_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveCurrentScript();

            switch (cmbEvent.SelectedIndex)
            {
                case 0:
                    tbScript.Code = _volume.Scripts.OnEnter;
                    break;
                case 1:
                    tbScript.Code = _volume.Scripts.OnLeave;
                    break;
                case 2:
                    tbScript.Code = _volume.Scripts.OnInside;
                    break;
                case 3:
                    tbScript.Code = _volume.Scripts.Environment;
                    break;
            }

            prevIndex = cmbEvent.SelectedIndex;
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (tbName.Text != _volume.Scripts.Name)
                _volume.Scripts.Name = tbName.Text;
        }

        private void cbEnabled_CheckedChanged(object sender, EventArgs e)
        {
            _volume.Enabled = cbEnabled.Checked;
        }
    }
}
