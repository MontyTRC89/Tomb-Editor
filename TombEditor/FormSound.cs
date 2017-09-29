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

namespace TombEditor
{
    public partial class FormSound : DarkUI.Forms.DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private SoundSourceInstance _soundSource;
        private Wad2 _wad;
        private Editor _editor;
        private ushort _selectedSound = 0;

        public FormSound(SoundSourceInstance soundSource, Wad2 wad)
        {
            _soundSource = soundSource;
            _wad = wad;
            _editor = Editor.Instance;

            InitializeComponent();

            foreach (var sound in _editor.Level.Wad.SoundInfo)
            {
                var item = new DarkUI.Controls.DarkListItem(sound.Value.Name);
                item.Tag = sound.Key;
                lstSounds.Items.Add(item);
            }

            cbBit1.Checked = (_soundSource.CodeBits & (1 << 0)) != 0;
            cbBit2.Checked = (_soundSource.CodeBits & (1 << 1)) != 0;
            cbBit3.Checked = (_soundSource.CodeBits & (1 << 2)) != 0;
            cbBit4.Checked = (_soundSource.CodeBits & (1 << 3)) != 0;
            cbBit5.Checked = (_soundSource.CodeBits & (1 << 4)) != 0;

            if (_editor.Level.Wad.SoundInfo.ContainsKey((ushort)_soundSource.SoundId))
            {
                var soundInfo = _editor.Level.Wad.SoundInfo[(ushort)_soundSource.SoundId];
                _selectedSound = (ushort)_soundSource.SoundId;
                tbSound.Text = soundInfo.Name;
            }
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            _soundSource.SoundId = (short)_selectedSound;

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
            if (lstSounds.SelectedIndices.Count == 0)
                return;

            var soundId = (ushort)lstSounds.Items[lstSounds.SelectedIndices[0]].Tag;
            var sound = _editor.Level.Wad.SoundInfo[soundId];

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

        private void FormSound_Load(object sender, EventArgs e)
        {

        }

        private void lstSounds_MouseClick(object sender, MouseEventArgs e)
        {
            if (lstSounds.SelectedIndices.Count == 0)
                return;

            var soundId = (ushort)lstSounds.Items[lstSounds.SelectedIndices[0]].Tag;
            var sound = _editor.Level.Wad.SoundInfo[soundId];
            _selectedSound = soundId;

            tbSound.Text = sound.Name;
        }
    }
}
