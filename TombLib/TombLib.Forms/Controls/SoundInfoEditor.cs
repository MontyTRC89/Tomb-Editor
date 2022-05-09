using DarkUI.Config;
using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.Controls
{
    [DefaultEvent("SoundInfoChanged")]
    public partial class SoundInfoEditor : UserControl
    {
        private static IReadOnlyList<FileFormat> FileExtensions { get; } = new List<FileFormat>()
        { new FileFormat("Waveform audio", "wav") };

        private const string _clipboardName = "SoundInfo";
        private bool _soundInfoCurrentlyChanging = false;
        private bool _readonly = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Level ReferenceLevel
        {
            get { return _referenceLevel; }
            set
            {
                if (value == _referenceLevel)
                    return;

                _referenceLevel = value;
                UpdateUI(SoundInfo, false);
            }
        }
        private Level _referenceLevel = null;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadSoundInfo SoundInfo
        {
            get
            {
                int id = 0;
                int.TryParse(tbID.Text, out id);
                
                WadSoundInfo result = new WadSoundInfo(id);
                result.Name = tbName.Text;
                result.Volume = (int)(numericVolume.Value);
                result.PitchFactor = (int)(numericPitch.Value);
                result.RangeInSectors = (int)numericRange.Value;
                result.Chance = (int)(numericChance.Value);
                result.RandomizeVolume = cbRandomizeVolume.Checked;
                result.RandomizePitch = cbRandomizePitch.Checked;
                result.DisablePanning = cbDisablePanning.Checked;
                result.LoopBehaviour = (WadSoundLoopBehaviour)(comboLoop.SelectedIndex);
                result.Global = cbGlobal.Checked;
                result.Indexed = cbIndexed.Checked;
                foreach (DataGridViewRow row in dgvSamples.Rows)
                    result.Samples.Add(new WadSample(row.Cells[0].Value.ToString()));
                return new WadSoundInfo(result);
            }
            set
            {
                if (value == null)
                {
                    this.Enabled = false;
                    picDisabledOverlay.Visible = true;
                    return;
                }
                else
                {
                    this.Enabled = true;
                    picDisabledOverlay.Visible = false;
                    UpdateUI(value, false);
                }
            }
        }

        [DefaultValue(true)]
        public bool HasName
        {
            get { return tbName.Visible; }
            set { tbName.Enabled = value; }
        }

        [DefaultValue(false)]
        public bool ReadOnly
        {
            get { return _readonly; }
            set
            {
                _readonly = value;
                tbName.Enabled = !value;
                numericVolume.Enabled = !value;
                numericPitch.Enabled = !value;
                numericRange.Enabled = !value;
                numericChance.Enabled = !value;
                cbRandomizeVolume.Enabled = !value;
                cbRandomizePitch.Enabled = !value;
                cbDisablePanning.Enabled = !value;
                comboLoop.Enabled = !value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor { get { return Colors.GreyBackground; } }

        public event EventHandler SoundInfoChanged;
        public void PlayCurrentSoundInfo() => WadSoundPlayer.PlaySoundInfo(ReferenceLevel, SoundInfo);

        public SoundInfoEditor()
        {
            InitializeComponent();

            picDisabledOverlay.Dock = DockStyle.Fill;
            BackColor = Colors.GreyBackground;
            comboLoop.SelectedIndex = 0;

            toolTip.SetToolTip(numericVolumeLabel, toolTip.GetToolTip(numericVolume));
            toolTip.SetToolTip(numericPitchLabel, toolTip.GetToolTip(numericPitch));
            toolTip.SetToolTip(numericRangeLabel, toolTip.GetToolTip(numericRange));
            toolTip.SetToolTip(numericVolumeLabel, toolTip.GetToolTip(numericVolume));
        }

        protected void OnSoundInfoChanged(object sender, EventArgs e)
        {
            if (sender is DarkComboBox)
            {
                // Update play type hint

                switch (((DarkComboBox)sender).SelectedIndex)
                {
                    case 0:
                        lblModeTooltip.Text = "Play in any case";
                        break;
                    case 1:
                        lblModeTooltip.Text = "Ignore if already playing";
                        break;
                    case 2:
                        lblModeTooltip.Text = "Rewind if already playing";
                        break;
                    case 3:
                        lblModeTooltip.Text = "Loop until stopped by engine";
                        break;
                }
            }

            if (!_soundInfoCurrentlyChanging)
                SoundInfoChanged?.Invoke(this, e);
        }

        private void UpdateUI(WadSoundInfo newSoundInfo, bool onlyParams)
        {
            try
            {
                // Prevent a number of sound info changed events
                _soundInfoCurrentlyChanging = true;

                butPlayPreview.Visible = _referenceLevel != null;
                numericVolume.Value = Math.Min(numericVolume.Maximum, Math.Max(numericVolume.Minimum, (decimal)newSoundInfo.Volume));
                numericPitch.Value = Math.Min(numericPitch.Maximum, Math.Max(numericPitch.Minimum, (decimal)newSoundInfo.PitchFactor));
                numericRange.Value = Math.Min(numericRange.Maximum, Math.Max(numericRange.Minimum, (decimal)newSoundInfo.RangeInSectors));
                numericChance.Value = Math.Min(numericChance.Maximum, Math.Max(numericChance.Minimum, (decimal)newSoundInfo.Chance));
                cbRandomizeVolume.Checked = newSoundInfo.RandomizeVolume;
                cbRandomizePitch.Checked = newSoundInfo.RandomizePitch;
                cbDisablePanning.Checked = newSoundInfo.DisablePanning;
                comboLoop.SelectedIndex = (int)(newSoundInfo.LoopBehaviour);
                cbGlobal.Checked = newSoundInfo.Global;
                cbIndexed.Checked = newSoundInfo.Indexed;

                if (!onlyParams)
                {
                    tbID.Text = newSoundInfo.Id.ToString();
                    tbName.Text = newSoundInfo.Name;
                    dgvSamples.Rows.Clear();

                    if (newSoundInfo.Samples != null && newSoundInfo.Samples.Count > 0)
                        foreach (var sample in newSoundInfo.Samples)
                            AddSampleToList(sample.FileName);
                }
            }
            finally
            {
                _soundInfoCurrentlyChanging = false;
            }
        }

        private bool AddSampleToList(string name)
        {
            var  path = string.Empty;
            bool notFound = false;

            if (_referenceLevel != null)
            {
                var foundPath = WadSounds.TryGetSamplePath(_referenceLevel.Settings.GetRecursiveListOfSoundPaths(), name);
                notFound = string.IsNullOrEmpty(foundPath);
                path = notFound ? "[ not found in any reference project sample paths ]" : foundPath;
            }

            dgvSamples.Rows.Add(name, path);

            // Highlight row if sample is missing

            if (notFound)
                dgvSamples.Rows[dgvSamples.Rows.Count - 1].DefaultCellStyle.BackColor = dgvSamples.BackColor.MixWith(Color.DarkRed, 0.55);

            return !notFound;
        }

        public void Copy()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var serializer = new XmlSerializer(typeof(WadSoundInfo));
                serializer.Serialize(stream, SoundInfo);
                Clipboard.SetData(_clipboardName, stream.ToArray());
            }
        }

        public void Paste(bool onlyParams = false)
        {
            var data = Clipboard.GetData("SoundInfo") as byte[];
            if (data == null)
                return;

            // Load sound info
            using (MemoryStream stream = new MemoryStream(data, false))
            {
                var serializer = new XmlSerializer(typeof(WadSoundInfo));
                var pastedInfo = new WadSoundInfo((WadSoundInfo)serializer.Deserialize(stream));
                pastedInfo.Id = SoundInfo.Id; // ID is unchangeable from UI

                UpdateUI(pastedInfo, onlyParams);

                if (!_soundInfoCurrentlyChanging)
                    SoundInfoChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void butResetToDefaults_Click(object sender, EventArgs e)
        {
            UpdateUI(new WadSoundInfo(SoundInfo.Id), true);

            if (!_soundInfoCurrentlyChanging)
                SoundInfoChanged?.Invoke(this, e);
        }

        private void butAddSample_Click(object sender, EventArgs e)
        {
            using (var form = new FormInputBox("New sample", "Insert the filename of the new sample without .wav extension:"))
            {
                form.StartPosition = FormStartPosition.CenterParent;
                if (form.ShowDialog() == DialogResult.Cancel)
                    return;
                AddSampleToList(form.Result + ".wav");
            }
        }

        private void butDeleteSample_Click(object sender, EventArgs e)
        {
            if (dgvSamples.SelectedRows.Count == 0)
                return;
            foreach (DataGridViewRow row in dgvSamples.SelectedRows)
                dgvSamples.Rows.RemoveAt(dgvSamples.SelectedRows[0].Index);
        }

        private void butBrowse_Click(object sender, EventArgs e)
        {
            var files = LevelFileDialog.BrowseFiles(FindForm(), ReferenceLevel?.Settings, null, "Choose samples", FileExtensions, null);
            if (files != null)
            {
                bool samplesAreMisplaced = false;

                foreach (var file in files)
                {
                    bool alreadyInList = false;
                    var fileName = Path.GetFileName(file).ToLower();

                    foreach (DataGridViewRow row in dgvSamples.Rows)
                    {
                        if (row.Cells[0].Value.ToString().ToLower().Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
                        { alreadyInList = true; break; }
                    }
                    if (alreadyInList) break;

                    samplesAreMisplaced = !AddSampleToList(fileName);
                }

                if (samplesAreMisplaced)
                    DarkMessageBox.Show(this, 
                        "Selected samples aren't placed in any of your reference level's sample paths.\nPlease copy samples to one of the sample paths or add specified path to path list.", 
                        "Wrong folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgvSamples_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e) => OnSoundInfoChanged(null, null);
        private void dgvSamples_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e) => OnSoundInfoChanged(null, null);
        private void butPlayPreview_Click(object sender, EventArgs e) => PlayCurrentSoundInfo();
        private void butClipboardCopy_Click(object sender, EventArgs e) => Copy();
        private void butClipboardPaste_Click(object sender, EventArgs e) => Paste();
    }
}
