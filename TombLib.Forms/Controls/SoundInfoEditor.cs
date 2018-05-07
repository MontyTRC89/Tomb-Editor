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
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.Controls
{
    [DefaultEvent("SoundInfoChanged")]
    public partial class SoundInfoEditor : UserControl
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string _clipboardName = "SoundInfo";
        private Cache<Tuple<WadSample, Size, int>, Bitmap> sampleImageCache = new Cache<Tuple<WadSample, Size, int>, Bitmap>(256, DrawWaveform);
        private string _currentPath = null;
        private bool _soundInfoCurrentlyChanging = false;
        private uint _targetSampleRate = WadSample.GameSupportedSampleRate;

        public SoundInfoEditor()
        {
            InitializeComponent();

            // Setup data grid view controls
            dataGridViewControls.DataGridView = dataGridView;
            dataGridViewControls.CreateNewRow = CreateNewSample;
            dataGridViewControls.Enabled = true;

            // Update the track bar when the form is started up.
            trackBar_Scroll(this, EventArgs.Empty);
            trackBar_Resize(this, EventArgs.Empty);

            toolTip.SetToolTip(numericVolumeLabel, toolTip.GetToolTip(numericVolume));
            toolTip.SetToolTip(numericPitchLabel, toolTip.GetToolTip(numericPitch));
            toolTip.SetToolTip(numericRangeLabel, toolTip.GetToolTip(numericRange));
            toolTip.SetToolTip(numericVolumeLabel, toolTip.GetToolTip(numericVolume));
        }

        private object CreateNewSample()
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = WadSample.FileFormatsToRead.GetFilter();
                if (!string.IsNullOrWhiteSpace(_currentPath))
                    try
                    {
                        fileDialog.InitialDirectory = Path.GetDirectoryName(_currentPath);
                        fileDialog.FileName = Path.GetFileName(_currentPath);
                    }
                    catch { }
                fileDialog.Title = "Select a sound file that you want to see imported.";

                DialogResult dialogResult = fileDialog.ShowDialog(this);
                _currentPath = fileDialog.FileName;
                if (dialogResult != DialogResult.OK)
                    return null;

                // Load sounds
                try
                {
                    WadSample result = new WadSample(WadSample.ConvertSampleFormat(File.ReadAllBytes(fileDialog.FileName), fileSampleRate =>
                    {
                        if (fileSampleRate == TargetSampleRate)
                            return new WadSample.ResampleInfo { Resample = false, SampleRate = TargetSampleRate };
                        if (((ICollection<WadSample>)dataGridView.DataSource).Count == 0)
                            return new WadSample.ResampleInfo { Resample = false, SampleRate = fileSampleRate };

                        // Ask the user about how to proceed if the sample rate mismatches...
                        switch (DarkMessageBox.Show(this, "The file '" + fileDialog.FileName + "' sample rate does not match the sound info sample rate." +
                            "\nDo you want to resample it? This process is lossy, but otherwise the length and pitch of the sound will change.", "Sample rate does not match", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                        {
                            case DialogResult.Yes:
                                return new WadSample.ResampleInfo { Resample = true, SampleRate = TargetSampleRate };
                            case DialogResult.No:
                                return new WadSample.ResampleInfo { Resample = false, SampleRate = TargetSampleRate };
                            default:
                                throw new OperationCanceledException();
                        }
                    }));

                    // Update sample rate if it changed
                    if (TargetSampleRate != result.SampleRate)
                        TargetSampleRate = result.SampleRate;
                    return result;
                }
                catch (OperationCanceledException) { return null; }
                catch (Exception exc)
                {
                    logger.Error(exc, "Unable to open file '" + fileDialog.FileName + "'.");
                    DarkMessageBox.Show(this, "Unable to load sprite from file '" + fileDialog.FileName + "'. " + exc.ToString(), "Unable to load sprite.", MessageBoxIcon.Error);
                    return null;
                }
            }
        }

        private void butExport_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new SaveFileDialog())
            {
                fileDialog.Filter = new[] { new FileFormat("WAVE file", "wav", "wave") }.GetFilter();
                if (!string.IsNullOrWhiteSpace(_currentPath))
                    try
                    {
                        fileDialog.InitialDirectory = Path.GetDirectoryName(_currentPath);
                        fileDialog.FileName = Path.GetFileName(_currentPath);
                    }
                    catch { }
                fileDialog.Title = "Choose a sound file name.";
                fileDialog.AddExtension = true;

                DialogResult dialogResult = fileDialog.ShowDialog(this);
                _currentPath = fileDialog.FileName;
                if (dialogResult != DialogResult.OK)
                    return;

                // Save sprites
                try
                {
                    foreach (DataGridViewRow row in dataGridView.SelectedRows)
                    {
                        string fileName = fileDialog.FileName;
                        if (dataGridView.SelectedRows.Count > 1)
                            fileName = Path.Combine(Path.GetDirectoryName(fileName),
                                Path.GetFileNameWithoutExtension(fileName) + row.Index.ToString("0000") + Path.GetExtension(fileName));
                        File.WriteAllBytes(fileName, ((WadSample)(row.DataBoundItem)).Data);
                    }
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Unable to save file '" + fileDialog.FileName + "'.");
                    DarkMessageBox.Show(this, "Unable to save sound. " + exc, "Saving sound failed.", MessageBoxIcon.Error);
                }
            }
        }

        private void butClipboardCopy_Click(object sender, EventArgs e)
        {
            Wad2 tempWad = new Wad2();
            tempWad.FixedSoundInfos.Add(new WadFixedSoundInfoId(), new WadFixedSoundInfo(new WadFixedSoundInfoId()) { SoundInfo = SoundInfo });
            using (MemoryStream stream = new MemoryStream())
            {
                Wad2Writer.SaveToStream(tempWad, stream);
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
                Wad2 tempWad = Wad2Loader.LoadFromStream(stream);
                SoundInfo = tempWad.FixedSoundInfos.Values.First().SoundInfo;
            }
        }

        private void butPlayPreview_Click(object sender, EventArgs e)
        {
            WadSoundPlayer.PlaySoundInfo(SoundInfo);
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (dataGridView.Columns[e.ColumnIndex].Name == PlayButtonColumn.Name)
            {
                WadSample sample = ((IReadOnlyList<WadSample>)dataGridView.DataSource)[e.RowIndex];
                WadSoundPlayer.PlaySample(sample);
            }
        }

        private static Bitmap DrawWaveform(Tuple<WadSample, Size, int> request)
        {
            float secondsPerPixel = 0.1f / request.Item3;
            ImageC image = WadSoundPlayer.DrawWaveformForSample(request.Item1,
                new VectorInt2(request.Item2.Width, request.Item2.Height), 0, secondsPerPixel, new ColorC(0, 0, 128));
            return image.ToBitmap();
        }

        private void dataGridView_CellFormattingSafe(object sender, DarkDataGridViewSafeCellFormattingEventArgs e)
        {
            var row = dataGridView.Rows[e.RowIndex];
            var column = dataGridView.Columns[e.ColumnIndex];

            var sampleData = (WadSample)(row.DataBoundItem);
            if (sampleData == null)
                return;
            if (column.Name == SizeColumn.Name)
            {
                e.Value = (sampleData.Data.Length / 1024) + " KB";
                e.FormattingApplied = true;
            }
            else if (column.Name == DurationColumn.Name)
            {
                e.Value = sampleData.Duration.TotalMilliseconds + " ms";
                e.FormattingApplied = true;
            }
            else if (column.Name == WaveformColumn.Name)
            {
                e.Value = sampleImageCache[new Tuple<WadSample, Size, int>(sampleData, new Size(column.Width, row.Height), trackBar.Value)];
                e.FormattingApplied = true;
            }
        }

        private void dataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (dataGridView.Columns[e.ColumnIndex].Name == PlayButtonColumn.Name)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                Image image = Properties.Resources.actions_play_16;
                e.Graphics.DrawImage(image,
                    e.CellBounds.Left + (e.CellBounds.Width - image.Width) / 2,
                    e.CellBounds.Top + (e.CellBounds.Height - image.Height) / 2,
                    image.Width, image.Height);
                e.Handled = true;
            }
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            trackBar_100MillisecondMark.Location = new Point(trackBar.Value + 5, trackBar_100MillisecondMark.Location.Y);
            dataGridView.InvalidateColumn(dataGridView.Columns[WaveformColumn.Name].Index);
        }

        private void trackBar_Resize(object sender, EventArgs e)
        {
            trackBar.Maximum = trackBar.Width - (trackBar.Minimum * 2);
        }

        private void comboSampleRate_SelectedValueChanged(object sender, EventArgs e)
        {
            if (comboSampleRate.SelectedItem != null)
            {
                comboSampleRateTextBox.Text = comboSampleRate.SelectedItem.ToString();
                AskUserAndResampleIfNecessary();
            }
        }

        private void comboSampleRateTextBox_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = !CheckSampleRate(comboSampleRateTextBox.Text);
        }

        private void comboSampleRateTextBox_Validated(object sender, EventArgs e)
        {
            AskUserAndResampleIfNecessary();
        }

        private void comboSampleRateTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                comboSampleRate.Text = comboSampleRateTextBox.Text = TargetSampleRate.ToString();
                ActiveControl = comboSampleRateLabel;
            }
        }

        private void comboSampleRateTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = CheckSampleRate(comboSampleRateTextBox.Text);
            comboSampleRateTextBox.BackColor = comboSampleRateLabel.BackColor =
                comboSampleRate.BackColor = ok ? Colors.LightBackground : Color.DarkRed;
            comboSampleRate.Text = comboSampleRateTextBox.Text;
        }

        private bool CheckSampleRate(string sampleRateText)
        {
            uint number;
            if (!uint.TryParse(sampleRateText, out number))
                return false;
            if (number < 1000)
                return false;
            if (number > 98000)
                return false;
            return true;
        }

        private void AskUserAndResampleIfNecessary()
        {
            if (_soundInfoCurrentlyChanging || !CheckSampleRate(comboSampleRateTextBox.Text))
                return;

            var samples = (IList<WadSample>)(dataGridView.DataSource);
            if (samples.Count == 0)
                return;

            // Ask if existing samples should be resampled or pitch shifted
            uint oldSampleRate = samples[0].SampleRate;
            uint newSampleRate = uint.Parse(comboSampleRateTextBox.Text);
            if (oldSampleRate == newSampleRate)
                return;
            switch (DarkMessageBox.Show(this, "The sample rate was changed from " + oldSampleRate + " Hz to " + newSampleRate + " Hz." +
                "\nDo you want to resample the currently set samples? This process is lossy, but otherwise the length and pitch of the sound will change.",
                "Sample rate changed", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
            {
                case DialogResult.Yes:
                    TargetSampleRate = newSampleRate;
                    for (int i = 0; i < samples.Count; ++i)
                        samples[i] = samples[i].ChangeSampleRate(newSampleRate, true);
                    dataGridView.InvalidateColumn(dataGridView.Columns[WaveformColumn.Name].Index);
                    dataGridView.InvalidateColumn(dataGridView.Columns[SizeColumn.Name].Index);
                    break;

                case DialogResult.No:
                    TargetSampleRate = newSampleRate;
                    for (int i = 0; i < samples.Count; ++i)
                        samples[i] = samples[i].ChangeSampleRate(newSampleRate, false);
                    dataGridView.InvalidateColumn(dataGridView.Columns[WaveformColumn.Name].Index);
                    dataGridView.InvalidateColumn(dataGridView.Columns[DurationColumn.Name].Index);
                    break;

                default:
                    TargetSampleRate = oldSampleRate;
                    return;
            }
        }

        private uint TargetSampleRate
        {
            get { return _targetSampleRate; }
            set
            {
                _targetSampleRate = value;
                comboSampleRate.Text = comboSampleRateTextBox.Text = value.ToString();
                numericPitch.Maximum = 100m * (decimal)WadSoundInfoMetaData.GetMaxPitch(value);
            }
        }

        [DefaultValue(true)]
        public bool HasName
        {
            get { return tbName.Visible; }
            set
            {
                tbName.Visible = value;
                tbNameLabel.Visible = value;
            }
        }

        protected void OnSoundInfoChanged(object sender, EventArgs e)
        {
            if (!_soundInfoCurrentlyChanging)
                SoundInfoChanged?.Invoke(this, e);
        }

        public event EventHandler SoundInfoChanged;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadSoundInfo SoundInfo
        {
            get
            {
                WadSoundInfoMetaData result = new WadSoundInfoMetaData(tbName.Text);
                result.Volume = (float)(numericVolume.Value * 0.01m);
                result.PitchFactor = (float)(numericPitch.Value * 0.01m);
                result.RangeInSectors = (float)numericRange.Value;
                result.Chance = (float)(numericChance.Value * 0.01m);
                result.RandomizeVolume = cbRandomizeVolume.Checked;
                result.RandomizePitch = cbRandomizePitch.Checked;
                result.DisablePanning = cbDisablePanning.Checked;
                result.LoopBehaviour = (WadSoundLoopBehaviour)(comboLoop.SelectedIndex);
                if (dataGridView.DataSource != null)
                    result.Samples.AddRange((IEnumerable<WadSample>)dataGridView.DataSource);
                return new WadSoundInfo(result);
            }
            set
            {
                try
                {
                    // Prevent a number of sound info changed events
                    _soundInfoCurrentlyChanging = true;

                    // Update control
                    if (value.Data.Samples.Count > 0)
                        TargetSampleRate = value.Data.Samples[0].SampleRate;
                    else
                        TargetSampleRate = WadSample.GameSupportedSampleRate;

                    tbName.Text = value.Name;
                    numericVolume.Value = Math.Min(numericVolume.Maximum, Math.Max(numericVolume.Minimum, 100m * (decimal)value.Data.Volume));
                    numericPitch.Value = Math.Min(numericPitch.Maximum, Math.Max(numericPitch.Minimum, 100m * (decimal)value.Data.PitchFactor));
                    numericRange.Value = Math.Min(numericRange.Maximum, Math.Max(numericRange.Minimum, (decimal)value.Data.RangeInSectors));
                    numericChance.Value = Math.Min(numericChance.Maximum, Math.Max(numericChance.Minimum, 100m * (decimal)value.Data.Chance));
                    cbRandomizeVolume.Checked = value.Data.RandomizeVolume;
                    cbRandomizePitch.Checked = value.Data.RandomizePitch;
                    cbDisablePanning.Checked = value.Data.DisablePanning;
                    comboLoop.SelectedIndex = (int)(value.Data.LoopBehaviour);

                    var list = new BindingList<WadSample>(new List<WadSample>(value.Data.Samples));
                    list.ListChanged += OnSoundInfoChanged;
                    dataGridView.DataSource = list;
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
            get { return tbName.Enabled; }
            set
            {
                tbName.Enabled = !value;
                numericVolume.Enabled = !value;
                numericPitch.Enabled = !value;
                numericRange.Enabled = !value;
                numericChance.Enabled = !value;
                cbRandomizeVolume.Enabled = !value;
                cbRandomizePitch.Enabled = !value;
                cbDisablePanning.Enabled = !value;
                comboLoop.Enabled = !value;
                dataGridView.ReadOnly = value;
                comboSampleRate.Enabled = !value;
                comboSampleRateTextBox.Enabled = !value;
                comboSampleRateLabel.Enabled = !value;
                dataGridViewControls.Enabled = !value;
                dataGridView.ReadOnly = value;
            }
        }
    }
}
