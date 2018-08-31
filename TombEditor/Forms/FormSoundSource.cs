using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Forms;
using NLog;
using TombLib.LevelData;
using TombLib.Wad;
using System.Collections.Generic;

namespace TombEditor.Forms
{
    public partial class FormSoundSource : DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly SoundSourceInstance _soundSource;
        private readonly IEnumerable<WadSoundInfo> _soundInfos;
        private static readonly WadSoundInfo _newSoundDefault = new WadSoundInfo(new WadSoundInfoMetaData("Empty") { LoopBehaviour = WadSoundLoopBehaviour.Looped });

        public FormSoundSource(SoundSourceInstance soundSource, IEnumerable<WadSoundInfo> soundInfos)
        {
            _soundSource = soundSource;
            _soundInfos = soundInfos;

            InitializeComponent();

            optionPlaySoundFromWad.Checked = !string.IsNullOrEmpty(soundSource.WadReferencedSoundName);
            optionPlayCustomSound.Checked = string.IsNullOrEmpty(soundSource.WadReferencedSoundName);

            soundInfoEditor.SoundInfo = soundSource.EmbeddedSoundInfo ?? _newSoundDefault;
            foreach (var sound in _soundInfos.OrderBy(soundInfo => soundInfo.Name))
                lstSounds.Items.Add(new DarkUI.Controls.DarkListItem(sound.Name) { Tag = sound });
            SetSoundName(_soundSource.WadReferencedSoundName ?? "");
        }

        private void SetSoundName(string soundName)
        {
            if (tbSound.Text == soundName)
                return;
            tbSound.Text = soundName;

            for (int i = 0; i < lstSounds.Items.Count; ++i)
                if (((WadSoundInfo)lstSounds.Items[i].Tag).Name.Equals(tbSound.Text, StringComparison.InvariantCultureIgnoreCase))
                {
                    lstSounds.SelectItem(i);
                    tbSound.BackColor = BackColor;
                    return;
                }
            lstSounds.SelectedIndices.Clear();
            tbSound.BackColor = Color.DarkRed;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            WadSoundPlayer.StopSample();
            Close();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (optionPlaySoundFromWad.Checked)
                _soundSource.WadReferencedSoundName = tbSound.Text;
            else
                _soundSource.EmbeddedSoundInfo = soundInfoEditor.SoundInfo == WadSoundInfo.Empty ? null : soundInfoEditor.SoundInfo;

            DialogResult = DialogResult.OK;
            WadSoundPlayer.StopSample();
            Close();
        }

        private void butPlay_Click(object sender, EventArgs e)
        {
            string text = tbSound.Text;
            var soundInfo = _soundInfos.FirstOrDefault(soundInfo_ => soundInfo_.Name == text);
            if (soundInfo == null)
            {
                DarkMessageBox.Show(this, "This sound is missing.", "Unable to play sound.", MessageBoxIcon.Information);
                return;
            }
            try
            {
                WadSoundPlayer.PlaySoundInfo(soundInfo);
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Unable to play sample");
                DarkMessageBox.Show(this, "Playing sound failed. " + exc, "Unable to play sound.", MessageBoxIcon.Information);
            }
        }

        private void tbSound_TextChanged(object sender, EventArgs e)
        {
            SetSoundName(tbSound.Text);
        }

        private void lstSounds_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (lstSounds.SelectedIndices.Count == 1)
                SetSoundName(((WadSoundInfo)lstSounds.Items[lstSounds.SelectedIndices[0]].Tag).Name);
        }

        private void optionPlayCustomSound_CheckedChanged(object sender, EventArgs e)
        {
            optionPlayCustomSoundGroupBox.Visible = true;
            optionPlaySoundFromWadGroupBox.Visible = false;
        }

        private void optionPlaySoundFromWad_CheckedChanged(object sender, EventArgs e)
        {
            optionPlayCustomSoundGroupBox.Visible = false;
            optionPlaySoundFromWadGroupBox.Visible = true;
        }
    }
}
