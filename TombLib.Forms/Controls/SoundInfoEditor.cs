using DarkUI.Config;
using DarkUI.Controls;
using DarkUI.Forms;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using TombLib.Forms;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.Controls
{
    [DefaultEvent("SoundInfoChanged")]
    public partial class SoundInfoEditor : UserControl
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string _clipboardName = "SoundInfo";
        private bool _soundInfoCurrentlyChanging = false;
        private bool _readonly = false;

        public SoundInfoEditor()
        {
            InitializeComponent();

            toolTip.SetToolTip(numericVolumeLabel, toolTip.GetToolTip(numericVolume));
            toolTip.SetToolTip(numericPitchLabel, toolTip.GetToolTip(numericPitch));
            toolTip.SetToolTip(numericRangeLabel, toolTip.GetToolTip(numericRange));
            toolTip.SetToolTip(numericVolumeLabel, toolTip.GetToolTip(numericVolume));
        }

        private void butClipboardCopy_Click(object sender, EventArgs e)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var serializer = new XmlSerializer(typeof(WadSoundInfo));
                serializer.Serialize(stream, SoundInfo);
                Clipboard.SetData(_clipboardName, stream.ToArray());
            }
        }

        private void butClipboardPaste_Click(object sender, EventArgs e)
        {
            var data = Clipboard.GetData("SoundInfo") as byte[];
            if (data == null)
            {
                DarkMessageBox.Show(this, "Windows clipboard does not currently not contain a sound info.", "Clipboard empty", MessageBoxIcon.Information);
                return;
            }

            // Load sound info
            using (MemoryStream stream = new MemoryStream(data, false))
            {
                var serializer = new XmlSerializer(typeof(WadSoundInfo));
                SoundInfo = new WadSoundInfo((WadSoundInfo)serializer.Deserialize(stream));
            }
        }

        private void butPlayPreview_Click(object sender, EventArgs e)
        {
            //WadSoundPlayer.PlaySoundInfo(SoundInfo);
        }

        [DefaultValue(true)]
        public bool HasName
        {
            get { return tbName.Visible; }
            set { tbName.Enabled = value; }
        }

        protected void OnSoundInfoChanged(object sender, EventArgs e)
        {
            if(sender is DarkComboBox)
            {
                // Update play type hint

                switch (((DarkComboBox)sender).SelectedIndex)
                {
                    case 0:
                        lblModeTooltip.Text = "Normal mode, sample plays once and can be called many times";
                        break;
                    case 1:
                        lblModeTooltip.Text = "The sound sound will be ignored until the current one is stopped";
                        break;
                    case 2:
                        lblModeTooltip.Text = "The sound will be replayed from the beginning if triggered again";
                        break;
                    case 3:
                        lblModeTooltip.Text = "The sound will seamlessly loop until stopped by engine event";
                        break;
                }
            }

            if (!_soundInfoCurrentlyChanging)
                SoundInfoChanged?.Invoke(this, e);
        }

        public event EventHandler SoundInfoChanged;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadSoundInfo SoundInfo
        {
            get
            {
                // XML_SOUND_SYSTEM
                WadSoundInfo result = new WadSoundInfo(int.Parse(tbID.Text));
                result.Name = tbName.Text;
                result.Volume = /*(float)*/(int)(numericVolume.Value * 0.01m);
                result.PitchFactor = /*(float)*/(int)(numericPitch.Value * 0.01m);
                result.RangeInSectors = /*(float)*/(int)numericRange.Value;
                result.Chance = /*(float)*/(int)(numericChance.Value * 0.01m);
                result.RandomizeVolume = cbRandomizeVolume.Checked;
                result.RandomizePitch = cbRandomizePitch.Checked;
                result.DisablePanning = cbDisablePanning.Checked;
                result.LoopBehaviour = (WadSoundLoopBehaviour)(comboLoop.SelectedIndex);
                result.Global = cbGlobal.Checked;
                foreach (DataGridViewRow row in dgvSamples.Rows)
                    result.EmbeddedSamples.Add(new WadSample(row.Cells[0].Value.ToString()));
                return new WadSoundInfo(result);
            }
            set
            {
                if (value == null)
                {
                    this.Enabled = false;
                    return;
                }
                else
                    this.Enabled = true;

                try
                {
                    // Prevent a number of sound info changed events
                    _soundInfoCurrentlyChanging = true;

                    tbID.Text = value.Id.ToString();
                    tbName.Text = value.Name;
                    numericVolume.Value = Math.Min(numericVolume.Maximum, Math.Max(numericVolume.Minimum, 100m * (decimal)value.Volume));
                    numericPitch.Value = Math.Min(numericPitch.Maximum, Math.Max(numericPitch.Minimum, 100m * (decimal)value.PitchFactor));
                    numericRange.Value = Math.Min(numericRange.Maximum, Math.Max(numericRange.Minimum, (decimal)value.RangeInSectors));
                    numericChance.Value = Math.Min(numericChance.Maximum, Math.Max(numericChance.Minimum, 100m * (decimal)value.Chance));
                    cbRandomizeVolume.Checked = value.RandomizeVolume;
                    cbRandomizePitch.Checked = value.RandomizePitch;
                    cbDisablePanning.Checked = value.DisablePanning;
                    comboLoop.SelectedIndex = (int)(value.LoopBehaviour);
                    cbGlobal.Checked = value.Global;
                    dgvSamples.Rows.Clear();
                    foreach (var sample in value.EmbeddedSamples)
                        dgvSamples.Rows.Add(sample.SamplePath);
                }
                finally
                {
                    _soundInfoCurrentlyChanging = false;
                }
            }
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

        private void ButAddSample_Click(object sender, EventArgs e)
        {
            using (var form = new FormInputBox("New sample", "Insert the filename of the new sample without .wav extension:"))
            {
                if (form.ShowDialog() == DialogResult.Cancel)
                    return;
                dgvSamples.Rows.Add(form.Result + ".wav");
            }
        }

        private void ButDeleteSample_Click(object sender, EventArgs e)
        {
            if (dgvSamples.SelectedRows.Count == 0)
                return;
            foreach (DataGridViewRow row in dgvSamples.SelectedRows)
                dgvSamples.Rows.Remove(row);
        }

        private void DgvSamples_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            OnSoundInfoChanged(null, null);
        }

        private void DgvSamples_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            OnSoundInfoChanged(null, null);
        }
    }
}
