using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Wad;
using NLog;
using DarkUI.Forms;

namespace TombEditor
{
    public partial class FormSound : DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private SoundSourceInstance _soundSource;
        private Wad2 _wad;
        private Random rng = new Random();

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

            foreach (var sound in _wad?.SoundInfosUnique ?? Enumerable.Empty<WadSoundInfo>())
                lstSounds.Items.Add(new DarkUI.Controls.DarkListItem(sound.Name) { Tag = sound });
            SetSound(_soundSource.SoundName);
        }

        private void SetSound(string soundName)
        {
            if (tbSound.Text != soundName)
                tbSound.Text = soundName;

            for (int i = 0; i < lstSounds.Items.Count; ++i)
                if (((WadSoundInfo)lstSounds.Items[i].Tag).Name.Equals(tbSound.Text, StringComparison.InvariantCultureIgnoreCase))
                {
                    lstSounds.SelectItem(i);
                    tbSound.BackColor = BackColor;
                    goto SeletectedSound;
                }
            lstSounds.SelectedIndices.Clear();
            tbSound.BackColor = Color.DarkRed;
            SeletectedSound:
            ;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            byte codeBits = 0;
            codeBits |= (byte)(cbBit1.Checked ? (1 << 0) : 0);
            codeBits |= (byte)(cbBit2.Checked ? (1 << 1) : 0);
            codeBits |= (byte)(cbBit3.Checked ? (1 << 2) : 0);
            codeBits |= (byte)(cbBit4.Checked ? (1 << 3) : 0);
            codeBits |= (byte)(cbBit5.Checked ? (1 << 4) : 0);
            _soundSource.SoundName = tbSound.Text;
            _soundSource.CodeBits = codeBits;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butPlay_Click(object sender, EventArgs e)
        {
            var soundInfo = _wad?.TryGetSound(tbSound.Text);
            if (soundInfo == null)
            {
                DarkMessageBox.Show(this, "This sound is missing.", "Unable to play sound.", MessageBoxIcon.Information);
                return;
            }
            try
            {
                soundInfo.Play();
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Unable to play sample");
                DarkMessageBox.Show(this, "Playing sound failed. " + exc, "Unable to play sound.", MessageBoxIcon.Information);
            }
        }

        private void tbSound_TextChanged(object sender, EventArgs e)
        {
            SetSound(tbSound.Text);
        }

        private void lstSounds_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (lstSounds.SelectedIndices.Count == 1)
                SetSound(((WadSoundInfo)lstSounds.Items[lstSounds.SelectedIndices[0]].Tag).Name);
        }
    }
}
