using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;
using System.IO;
using System.Media;
using TombLib.Wad;
using NLog;
using DarkUI.Forms;

namespace TombEditor
{
    public partial class FormSound : DarkUI.Forms.DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private SoundSourceInstance _soundSource;
        private Wad2 _wad;
        private ushort _selectedSoundId = 0;

        public FormSound(SoundSourceInstance soundSource, Wad2 wad)
        {
            _soundSource = soundSource;
            _wad = wad;

            InitializeComponent();
            cbBit1.Checked = (_soundSource.CodeBits & (1 << 0)) != 0;
            cbBit2.Checked = (_soundSource.CodeBits & (1 << 1)) != 0;
            cbBit3.Checked = (_soundSource.CodeBits & (1 << 2)) != 0;
            cbBit4.Checked = (_soundSource.CodeBits & (1 << 3)) != 0;
            cbBit5.Checked = (_soundSource.CodeBits & (1 << 4)) != 0;

            foreach (var sound in _wad?.SoundInfo ?? Enumerable.Empty<KeyValuePair<ushort, WadSoundInfo>>())
                lstSounds.Items.Add(new DarkUI.Controls.DarkListItem(sound.Value.Name) { Tag = sound.Key });
            SetSound(_soundSource.SoundId);
        }

        private void SetSound(ushort soundId)
        {
            _selectedSoundId = soundId;
            tbSound.Text = soundId.ToString();
            if (_wad?.SoundInfo?.ContainsKey(soundId) ?? false)
                tbSound.Text = "(" + soundId + ") " + _wad.SoundInfo[soundId].Name;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            _soundSource.SoundId = _selectedSoundId;

            byte codeBits = 0;
            codeBits |= (byte)(cbBit1.Checked ? (1 << 0) : 0);
            codeBits |= (byte)(cbBit2.Checked ? (1 << 1) : 0);
            codeBits |= (byte)(cbBit3.Checked ? (1 << 2) : 0);
            codeBits |= (byte)(cbBit4.Checked ? (1 << 3) : 0);
            codeBits |= (byte)(cbBit5.Checked ? (1 << 4) : 0);
            _soundSource.CodeBits = codeBits;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void butPlay_Click(object sender, EventArgs e)
        {
            if (_wad.SoundInfo == null)
            {
                DarkMessageBox.Show(this, "No wad with sounds loaded.", "Unable to play sound.", MessageBoxIcon.Information);
                return;
            }

            var sound = _wad.SoundInfo[_selectedSoundId];

            if (sound.WaveSounds.Count > 0)
            {
                try
                {
                    sound.WaveSounds[0].Play();
                }
                catch (Exception exc)
                {
                    logger.Warn(exc, "Unable to play sample");
                }
            }
        }

        private void lstSounds_MouseClick(object sender, MouseEventArgs e)
        {
            if (lstSounds.SelectedIndices.Count == 0)
                return;
            SetSound((ushort)lstSounds.Items[lstSounds.SelectedIndices[0]].Tag);
        }

        private void tbSound_TextChanged(object sender, EventArgs e)
        {
            string text = tbSound.Text;
            if (text.StartsWith("(") && text.Contains(")"))
                text = text.Substring(1, text.IndexOf(")") - 1);

            ushort soundId;
            if (!ushort.TryParse(tbSound.Text, out soundId))
            {
                tbSound.BackColor = BackColor.MixWith(Color.DarkRed, 0.55);
                return;
            }

            tbSound.BackColor = BackColor;
            if (soundId == _selectedSoundId)
                return;
            SetSound(soundId);
        }
    }
}
