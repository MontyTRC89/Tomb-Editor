using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Sounds;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.Forms
{
    public partial class FormSoundEditor : DarkForm
    {
        private Wad2 _wad;
        private int _currentSound = -1;
        private bool _saveWadOnExit;
        private Dictionary<ushort, WadSoundInfo> _sounds;

        public FormSoundEditor(Wad2 wad, bool saveWadOnExit)
        {
            InitializeComponent();
            _wad = wad;
            _saveWadOnExit = saveWadOnExit;
            butSave.Visible = _saveWadOnExit;
            butClose.Visible = _saveWadOnExit;

            for (int i = 0; i < 370; i++)
                comboId.Items.Add(i.ToString());

            ReloadSoundInfos();
        }

        private void UpdateStatistics()
        {
            string message = "Sound Infos: " + _wad.Sounds.Count + " of " +
                             _wad.SoundMapSize + "    " +
                             "Embedded WAV samples: " + _wad.Samples.Count;
            labelStatus.Text = message;
        }

        private void ReloadSoundInfos()
        {
            lstSoundInfos.Items.Clear();
            _sounds = new Dictionary<ushort, WadSoundInfo>();

            var soundMapSize = SoundsCatalog.GetSoundMapSize(_wad.Version, false);
            for (var i = 0; i < soundMapSize; i++)
            {
                /*ushort soundId = (ushort)i;

                if (_wad.SoundInfo.ContainsKey(soundId))
                {
                    var sound = _wad.SoundInfo[soundId];
                    sound.Enabled = true;
                    _sounds.Add(soundId, sound);
                }
                else
                {
                    var soundCatalog = SoundsCatalog.GetSound(_wad.Version, soundId);
                    var info = new WadSoundInfo();
                    info.Enabled = false;
                    info.Name = soundCatalog.Name;
                    info.Volume = soundCatalog.Volume;
                    info.Chance = soundCatalog.Chance;
                    info.Pitch = soundCatalog.Pitch;
                    info.Range = soundCatalog.Range;
                    info.RandomizeGain = soundCatalog.FlagR;
                    info.RandomizePitch = soundCatalog.FlagP;
                    if (soundCatalog.FlagL)
                        info.Loop = WadSoundLoopType.L;
                    else if (soundCatalog.FlagR)
                        info.Loop = WadSoundLoopType.R;
                    foreach (var sampleName in soundCatalog.Samples)
                        info.Samples.Add(LoadSample())
                }*/
            }

            foreach (var soundInfo in _wad.Sounds)
            {
                var item = new DarkUI.Controls.DarkListItem(soundInfo.Key + ": " + soundInfo.Value.Name);
                item.Tag = soundInfo.Key;
                lstSoundInfos.Items.Add(item);
            }

            UpdateStatistics();
        }

        private void butAddNewSound_Click(object sender, EventArgs e)
        {
            ushort newSoundId = _wad.GetFirstFreeSoundSlot();
            if (newSoundId == ushort.MaxValue)
            {
                DarkMessageBox.Show(this, "You soundmap is already full", "Error", MessageBoxIcon.Error);
                return;
            }

            _currentSound = -1;
            comboId.SelectedIndex = newSoundId;

            tbName.Text = "NEW_SOUND";
            tbChance.Text = "0";
            tbVolume.Text = "0";
            tbRange.Text = "8";
            tbPitch.Text = "0";
            cbFlagN.Checked = false;
            cbRandomizeGain.Checked = false;
            cbRandomizePitch.Checked = false;

            lstWaves.Items.Clear();

            butSaveChanges.Visible = true;
        }

        private void lstSoundInfos_MouseClick(object sender, MouseEventArgs e)
        {
            if (lstSoundInfos.SelectedIndices.Count == 0) return;

            // Get the selected sound info
            var item = lstSoundInfos.Items[lstSoundInfos.SelectedIndices[0]];
            var soundInfo = _wad.Sounds[(ushort)item.Tag];

            // Fill the UI
            tbName.Text = soundInfo.Name;
            tbChance.Text = soundInfo.Chance.ToString();
            tbVolume.Text = soundInfo.Volume.ToString();
            tbRange.Text = soundInfo.Range.ToString();
            tbPitch.Text = soundInfo.Pitch.ToString();
            cbFlagN.Checked = soundInfo.FlagN;
            comboLoop.SelectedIndex = (byte)soundInfo.Loop;
            cbRandomizeGain.Checked = soundInfo.RandomizeGain;
            cbRandomizePitch.Checked = soundInfo.RandomizePitch;
            comboId.SelectedIndex = (ushort)item.Tag;

            lstWaves.Items.Clear();
            foreach (var wave in soundInfo.Samples)
            {
                var itemWave = new DarkUI.Controls.DarkListItem(wave.Name); 
                itemWave.Tag = wave;
                lstWaves.Items.Add(itemWave);
            }

            _currentSound = (ushort)item.Tag;

            butSaveChanges.Visible = true;
        }

        private void butSaveChanges_Click(object sender, EventArgs e)
        {
            // I can't overwrite other sounds
            if (comboId.SelectedIndex != _currentSound &&
                _wad.Sounds.ContainsKey((ushort)comboId.SelectedIndex))
            {
                DarkMessageBox.Show(this, "The selected slot is already assigned to another sound", "Error", MessageBoxIcon.Error);
                return;
            }

            if (lstWaves.Items.Count == 0)
            {
                DarkMessageBox.Show(this, "You must add at least one WAV sample", "Error", MessageBoxIcon.Error);
                return;
            }

            // Remap anim commands
            var oldSoundId = _currentSound;
            var newSoundId = (ushort)comboId.SelectedIndex;

            if (oldSoundId != -1)
            {
                foreach (var moveable in _wad.Moveables)
                {
                    foreach (var animation in moveable.Value.Animations)
                    {
                        foreach (var command in animation.AnimCommands)
                        {
                            if (command.Type == TombLib.Wad.WadAnimCommandType.PlaySound)
                            {
                                ushort soundId = (ushort)(command.Parameter2 & 0x3fff);
                                if (soundId == oldSoundId)
                                {
                                    // Remap current sound
                                    command.Parameter2 = (ushort)((command.Parameter2 & 0xc000) + newSoundId);
                                }
                            }
                        }
                    }
                }
            }

            // Get the current sound info
            WadSoundInfo soundInfo;

            if (oldSoundId == -1)
                soundInfo = new WadSoundInfo();
            else
                soundInfo = _wad.Sounds[(ushort)oldSoundId];

            // Save changes
            soundInfo.Chance = Byte.Parse(tbChance.Text);
            soundInfo.Volume = Byte.Parse(tbVolume.Text);
            soundInfo.Range = Byte.Parse(tbRange.Text);
            soundInfo.Pitch = Byte.Parse(tbPitch.Text);
            soundInfo.Loop = (WadSoundLoopType)comboLoop.SelectedIndex;
            soundInfo.FlagN = cbFlagN.Checked;
            soundInfo.RandomizeGain = cbRandomizeGain.Checked;
            soundInfo.RandomizePitch = cbRandomizePitch.Checked;
            soundInfo.Name = tbName.Text;

            soundInfo.Samples.Clear();
            foreach (var item in lstWaves.Items)
                soundInfo.Samples.Add((WadSample)item.Tag);

            if (oldSoundId == -1)
            {
                _wad.Sounds.Add(newSoundId, soundInfo);

                ReloadSoundInfos();
            }
            else
            {
                if (oldSoundId != newSoundId)
                {
                    _wad.Sounds.Remove((ushort)oldSoundId);
                    _wad.Sounds.Add(newSoundId, soundInfo);

                    ReloadSoundInfos();
                }
                else
                {
                    lstSoundInfos.Items[lstSoundInfos.SelectedIndices[0]].Text = oldSoundId + ": " + tbName.Text;
                }
            }

            UpdateStatistics();
        }

        private void butAddNewWave_Click(object sender, EventArgs e)
        {
            var form = new FormSelectSample(_wad);
            if (form.ShowDialog(this) == DialogResult.Cancel) return;

            // Search for already existing sample
            for (int i = 0; i < lstWaves.Items.Count; i++)
            {
                var item = lstWaves.Items[i];
                var wave = (WadSample)item.Tag;
                if (wave.Hash == form.SelectedWave.Hash)
                {
                    DarkMessageBox.Show(this, "This WAV sample is already present in this sound info", "Error", MessageBoxIcon.Error);
                    return;
                }
            }

            // Add the new WAV sample
            var newItem = new DarkUI.Controls.DarkListItem(form.SelectedWave.Name);
            newItem.Tag = form.SelectedWave;
            lstWaves.Items.Add(newItem);
        }

        private void butDeleteWave_Click(object sender, EventArgs e)
        {
            if (lstWaves.SelectedIndices.Count == 0) return;

            var item = lstWaves.Items[lstWaves.SelectedIndices[0]];
            var wave = (WadSample)item.Tag;

            // Ask to the user the permission to delete WAV
            if (DarkMessageBox.Show(this,
                   "Are you really sure to delete '" + wave.Name + "'? The WAV sample will be removed from this sound but not from Wad2 file until some sound is referencing it",
                   "Delete WAV", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            lstWaves.Items.Remove(item);
        }

        private void butPlaySound_Click(object sender, EventArgs e)
        {
            if (lstWaves.SelectedIndices.Count == 0) return;

            var item = lstWaves.Items[lstWaves.SelectedIndices[0]];
            var wave = (WadSample)item.Tag;

            wave.Play();
        }

        private void butDeleteSound_Click(object sender, EventArgs e)
        {
            if (lstSoundInfos.SelectedIndices.Count == 0) return;

            var item = lstSoundInfos.Items[lstSoundInfos.SelectedIndices[0]];
            var soundInfo = _wad.Sounds[(ushort)item.Tag];
            var soundIdToRemove = (ushort)item.Tag;

            // Get all moveables that are using this sound
            var moveables = _wad.GetAllMoveablesReferencingSound(soundIdToRemove);

            if (moveables.Count != 0)
            {
                var stringMoveables = "";
                foreach (var mov in moveables)
                    stringMoveables += mov.ToString() + Environment.NewLine;

                // Ask to the user the permission to delete WAV
                if (DarkMessageBox.Show(this,
                       "Are you really sure to delete '" + soundInfo.Name + "'? The following moveables are referincing " +
                       "this sound and their animation commands will be remapped to the first available sound: " + Environment.NewLine +
                       stringMoveables,
                       "Delete sound", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }
            else
            {
                // Ask to the user the permission to delete WAV
                if (DarkMessageBox.Show(this,
                       "Are you really sure to delete '" + soundInfo.Name + "'?",
                       "Delete sound", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            // Delete the sound and remap anim commands
            _wad.DeleteSound(soundIdToRemove);

            ReloadSoundInfos();
            _currentSound = -1;
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butSave_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
