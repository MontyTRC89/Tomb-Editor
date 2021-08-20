using System;
using System.Linq;
using System.Windows.Forms;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using System.ComponentModel;
using DarkUI.Config;

namespace WadTool
{
    public partial class AnimCommandEditor : UserControl
    {
        private bool _currentlyDoingCommandSelection = false;
        private AnimationEditor _editor;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadAnimCommand Command
        {
            get { return _command; }
            set
            {
                _command = value;
                UpdateUI(_command);
            }
        } private WadAnimCommand _command;

        public class AnimCommandEventArgs : EventArgs { public WadAnimCommand Command { get; set; } }
        public event EventHandler<AnimCommandEventArgs> AnimCommandChanged;
        private void InvokeChanged() { if (!_currentlyDoingCommandSelection) AnimCommandChanged?.Invoke(this, new AnimCommandEventArgs() { Command = _command }); }

        public AnimCommandEditor() { InitializeComponent(); panelView.BackColor = Colors.GreyBackground; }
        public void Initialize(AnimationEditor editor, bool disableFrameControls = false)
        {
            _editor = editor;
            tbPlaySoundFrame.Enabled = !disableFrameControls;
            tbFlipEffectFrame.Enabled = !disableFrameControls;

            butPlaySound.Enabled = (_editor.Tool.ReferenceLevel != null &&
                _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.Count > 0);

            ReloadSounds();
        }

        public void UpdateUI(WadAnimCommand cmd)
        {
            if (_currentlyDoingCommandSelection)
                return;

            if (cmd == null)
            {
                comboCommandType.Enabled = false;
                commandControls.Visible = false;
                return;
            }
            else
            {
                comboCommandType.Enabled = true;
                commandControls.Visible = true;
            }

            try
            {
                _currentlyDoingCommandSelection = true;

                comboCommandType.SelectedIndex = (int)(cmd.Type) - 1;

                switch (cmd.Type)
                {
                    case WadAnimCommandType.SetPosition:
                        commandControls.Visible = true;
                        commandControls.SelectedTab = tabSetPosition;

                        tbPosX.Value = cmd.Parameter1;
                        tbPosY.Value = cmd.Parameter2;
                        tbPosZ.Value = cmd.Parameter3;
                        break;

                    case WadAnimCommandType.SetJumpDistance:
                        commandControls.Visible = true;
                        commandControls.SelectedTab = tabSetJumpDistance;

                        tbHorizontal.Value = cmd.Parameter1;
                        tbVertical.Value = cmd.Parameter2;
                        break;

                    case WadAnimCommandType.EmptyHands:
                    case WadAnimCommandType.KillEntity:
                        commandControls.Visible = false;
                        break;

                    case WadAnimCommandType.PlaySound:
                        commandControls.Visible = true;
                        commandControls.SelectedTab = tabPlaySound;

                        tbPlaySoundFrame.Value = cmd.Parameter1;
                        nudSoundId.Value = cmd.Parameter2 & 0x3FFF;

                        switch (cmd.Parameter2 & 0xC000)
                        {
                            default:
                                comboPlaySoundConditions.SelectedIndex = 0;
                                break;
                            case 0x4000:
                                comboPlaySoundConditions.SelectedIndex = 1;
                                break;
                            case 0x8000:
                                comboPlaySoundConditions.SelectedIndex = 2;
                                break;
                        }
                        break;

                    case WadAnimCommandType.FlipEffect:
                        commandControls.Visible = true;
                        commandControls.SelectedTab = tabFlipeffect;

                        tbFlipEffectFrame.Value = cmd.Parameter1;
                        tbFlipEffect.Value = cmd.Parameter2 & 0x3FFF;

                        switch (cmd.Parameter2 & 0xC000)
                        {
                            default:
                                comboFlipeffectConditions.SelectedIndex = 0;
                                break;
                            case 0x4000:
                                comboFlipeffectConditions.SelectedIndex = 1;
                                break;
                            case 0x8000:
                                comboFlipeffectConditions.SelectedIndex = 2;
                                break;
                        }
                        break;
                }
            }
            finally
            {
                _currentlyDoingCommandSelection = false;
            }
        }

        private void ReloadSounds()
        {
            comboSound.Items.Clear();

            var defaultSoundList = TrCatalog.GetAllSounds(_editor.Tool.DestinationWad.GameVersion);
            var soundCatalogPresent = _editor.Tool.ReferenceLevel != null && _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.Count > 0;

            var maxKnownSound = -1;

            foreach (var sound in defaultSoundList)
                if (sound.Key > maxKnownSound) maxKnownSound = (int)sound.Key;

            if (soundCatalogPresent)
                foreach (var sound in _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap)
                    if (sound.Id > maxKnownSound) maxKnownSound = sound.Id;

            for (int i = 0; i <= maxKnownSound; i++)
            {
                var lbl = i.ToString().PadLeft(4, '0') + ": ";

                if (soundCatalogPresent && _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.Any(item => item.Id == i))
                    lbl += _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.First(item => item.Id == i).Name;
                else if (defaultSoundList.Any(item => item.Key == i))
                    lbl += defaultSoundList.First(item => item.Key == i).Value;
                else
                    lbl += "Unknown sound";

                comboSound.Items.Add(lbl);
            }

            comboSound.Items.Add("Custom sound ID");
            comboSound.SelectedIndex = 0;
        }

        private void comboCommandType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_command == null || _currentlyDoingCommandSelection)
                return;

            WadAnimCommandType newType = (WadAnimCommandType)(comboCommandType.SelectedIndex + 1);
            _command.Type = newType;
            UpdateUI(_command);
            InvokeChanged();
        }

        private void tbPosX_ValueChanged(object sender, EventArgs e)
        {
            if (_command == null || _command.Type != WadAnimCommandType.SetPosition)
                return;
            _command.Parameter1 = (short)tbPosX.Value;
            InvokeChanged();
        }

        private void tbPosY_ValueChanged(object sender, EventArgs e)
        {
            if (_command == null || _command.Type != WadAnimCommandType.SetPosition)
                return;
            _command.Parameter2 = (short)tbPosY.Value;
            InvokeChanged();
        }

        private void tbPosZ_ValueChanged(object sender, EventArgs e)
        {
            if (_command == null || _command.Type != WadAnimCommandType.SetPosition)
                return;
            _command.Parameter3 = (short)tbPosZ.Value;
            InvokeChanged();
        }

        private void tbHorizontal_ValueChanged(object sender, EventArgs e)
        {
            if (_command == null || _command.Type != WadAnimCommandType.SetJumpDistance)
                return;
            _command.Parameter1 = (short)tbHorizontal.Value;
            InvokeChanged();
        }

        private void tbVertical_ValueChanged(object sender, EventArgs e)
        {
            if (_command == null || _command.Type != WadAnimCommandType.SetJumpDistance)
                return;
            _command.Parameter2 = (short)tbVertical.Value;
            InvokeChanged();
        }

        private void tbFlipEffectFrame_ValueChanged(object sender, EventArgs e)
        {
            if (_command == null || _command.Type != WadAnimCommandType.FlipEffect)
                return;
            _command.Parameter1 = (short)tbFlipEffectFrame.Value;
            InvokeChanged();
        }

        private void tbFlipEffect_ValueChanged(object sender, EventArgs e)
        {
            if (_command == null || _command.Type != WadAnimCommandType.FlipEffect)
                return;

            _command.Parameter2 &= unchecked((short)~0x3FFF);
            _command.Parameter2 |= (short)tbFlipEffect.Value;
            InvokeChanged();
        }

        private void tbPlaySoundFrame_ValueChanged(object sender, EventArgs e)
        {
            if (_command == null || _command.Type != WadAnimCommandType.PlaySound)
                return;
            _command.Parameter1 = (short)tbPlaySoundFrame.Value;
            InvokeChanged();
        }

        private void comboPlaySoundConditions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_command == null || _command.Type != WadAnimCommandType.PlaySound)
                return;
            _command.Parameter2 &= unchecked((short)~0xC000);
            _command.Parameter2 |= (short)(comboPlaySoundConditions.SelectedIndex << 14);
            InvokeChanged();
        }

        private void comboSound_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_command == null || _command.Type != WadAnimCommandType.PlaySound)
                return;

            if (comboSound.SelectedIndex < comboSound.Items.Count - 1)
                nudSoundId.Value = comboSound.SelectedIndex;
        }

        private void comboFlipeffectConditions_SelectedIndexChanged(object sender, EventArgs e)
        {
            _command.Parameter2 &= unchecked((short)~0xC000);
            _command.Parameter2 |= (short)(comboFlipeffectConditions.SelectedIndex << 14);
            InvokeChanged();
        }

        private void butPlaySound_Click(object sender, EventArgs e)
        {
            if (_editor.Tool.ReferenceLevel == null ||
                _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.Count == 0)
                return;

            var soundInfo = _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.FirstOrDefault(soundInfo_ => soundInfo_.Id == (int)nudSoundId.Value);
            if (soundInfo != null)
                try { WadSoundPlayer.PlaySoundInfo(_editor.Tool.ReferenceLevel, soundInfo); }
                catch (Exception) { } // FIXME: do something!
        }

        private void nudSoundId_ValueChanged(object sender, EventArgs e)
        {
            _command.Parameter2 &= unchecked((short)~0x3FFF);
            _command.Parameter2 |= (short)nudSoundId.Value;

            if (nudSoundId.Value < comboSound.Items.Count - 1)
                comboSound.SelectedIndex = (int)nudSoundId.Value;
            else
                comboSound.SelectedIndex = comboSound.Items.Count - 1;

            InvokeChanged();
        }
    }
}
