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
        private readonly Editor _editor = Editor.Instance;
        private int _soundId = -1;

        public FormSoundSource(SoundSourceInstance soundSource, IEnumerable<WadSoundInfo> soundInfos)
        {
            _soundSource = soundSource;
            _soundInfos = soundInfos;
            _soundId = _soundSource.SoundId;
            
            InitializeComponent();

            comboPlayMode.SelectedIndex = (int)soundSource.PlayMode;

            foreach (var sound in _soundInfos.OrderBy(soundInfo => soundInfo.Name))
                lstSounds.Items.Add(new DarkUI.Controls.DarkListItem(sound.Id.ToString().PadLeft(4, '0') + ": " + sound.Name) { Tag = sound });
            SelectSound(_soundSource.SoundId);
        }

        private void SelectSound(int id)
        {
            if (id == -1)
                return;

            WadSoundInfo info = _editor.Level.Settings.WadTryGetSoundInfo(id);
            if (info == null)
                return;

            tbSound.Text = info.Name;

            for (int i = 0; i < lstSounds.Items.Count; ++i)
            {
                Console.WriteLine(i);
                if (((WadSoundInfo)lstSounds.Items[i].Tag).Id == id)
                {
                    lstSounds.SelectItem(i);
                    tbSound.BackColor = BackColor;
                    _soundId = id;
                    return;
                }
            }

            _soundId = -1;
            lstSounds.SelectedIndices.Clear();
            tbSound.BackColor = Color.DarkRed;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            //WadSoundPlayer.StopSample();
            Close();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            _soundSource.SoundId = _soundId;
            _soundSource.PlayMode = (SoundSourcePlayMode)comboPlayMode.SelectedIndex;

            DialogResult = DialogResult.OK;
            //WadSoundPlayer.StopSample();
            Close();
        }

        private void butPlay_Click(object sender, EventArgs e)
        {
            /*string text = tbSound.Text;
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
            }*/
        }

        private void lstSounds_SelectedIndicesChanged(object sender, EventArgs e)
        {
   
            
        }

        private void LstSounds_Click(object sender, EventArgs e)
        {
            if (lstSounds.SelectedIndices.Count == 1)
                SelectSound(((WadSoundInfo)lstSounds.Items[lstSounds.SelectedIndices[0]].Tag).Id);
        }
    }
}
