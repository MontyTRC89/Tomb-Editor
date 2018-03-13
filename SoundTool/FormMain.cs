using System;
using System.Windows.Forms;
using TombLib.Wad;
using DarkUI.Controls;
using DarkUI.Forms;
using System.IO;
using System.Media;

namespace SoundTool
{
    public partial class FormMain : DarkForm
    {
        private WadGameVersion _version;
        private int _currentSound;

        public FormMain()
        {
            InitializeComponent();

            SoundsCatalog.LoadAllCatalogsFromXml("Sounds");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tR2CatalogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _version = WadGameVersion.TR2;

            buildMAINSFXToolStripMenuItem.Enabled = true;
            cbMandatorySound.Visible = false;
            cbMandatorySound.Checked = false;
            cbNgLocked.Visible = false;
            cbNgLocked.Checked = false;

            ReloadSounds();
        }

        private void ReloadSounds()
        {
            lstSoundInfos.Items.Clear();

            var sounds = SoundsCatalog.GetAllSounds(_version);
            foreach (var pair in sounds)
            {
                var item = new DarkListItem(pair.Key.ToString().PadLeft(3, '0') + ": " + pair.Value.Name);
                item.Tag = pair.Key;
                lstSoundInfos.Items.Add(item);
            }
        }

        private void butAddNewWave_Click(object sender, EventArgs e)
        {

        }

        private void tR3CatalogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _version = WadGameVersion.TR3;

            buildMAINSFXToolStripMenuItem.Enabled = true;
            cbMandatorySound.Visible = false;
            cbMandatorySound.Checked = false;
            cbNgLocked.Visible = false;
            cbNgLocked.Checked = false;

            ReloadSounds();
        }

        private void tR4CatalogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _version = WadGameVersion.TR4_TRNG;

            buildMAINSFXToolStripMenuItem.Enabled = false;
            cbMandatorySound.Visible = true;
            //if (_version)
            cbNgLocked.Visible = true;

            ReloadSounds();
        }

        private void tR5CatalogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _version = WadGameVersion.TR5;

            buildMAINSFXToolStripMenuItem.Enabled = false;
            cbMandatorySound.Visible = true;
            cbNgLocked.Visible = false;
            cbNgLocked.Checked = false;

            ReloadSounds();
        }

        private void lstSoundInfos_Click(object sender, EventArgs e)
        {
            if (lstSoundInfos.SelectedIndices.Count == 0) return;

            // Get the selected sound info
            var item = lstSoundInfos.Items[lstSoundInfos.SelectedIndices[0]];
            var soundInfo = SoundsCatalog.GetSound(_version, (ushort)item.Tag);

            // Fill the UI
            tbName.Text = soundInfo.Name;
            tbChance.Text = soundInfo.Chance.ToString();
            tbVolume.Text = soundInfo.Volume.ToString();
            tbRange.Text = soundInfo.Range.ToString();
            tbPitch.Text = soundInfo.Pitch.ToString();
            cbFlagN.Checked = soundInfo.FlagN;
            comboLoop.SelectedIndex = 0;
            if (soundInfo.FlagW)
                comboLoop.SelectedIndex = 1;
            else if (soundInfo.FlagR)
                comboLoop.SelectedIndex = 2;
            else if (soundInfo.FlagL)
                comboLoop.SelectedIndex = 3;
            cbRandomizeGain.Checked = soundInfo.FlagR;
            cbRandomizePitch.Checked = soundInfo.FlagP;
            cbMandatorySound.Checked = soundInfo.MandatorySound;
            cbNgLocked.Checked = soundInfo.NgLocked;

            lstSamples.Items.Clear();
            foreach (var sample in soundInfo.Samples)
            {
                var itemSample = new DarkListItem(sample);
                lstSamples.Items.Add(itemSample);
            }

            _currentSound = (ushort)item.Tag;

            butSaveChanges.Visible = true;
        }

        private void butSaveChanges_Click(object sender, EventArgs e)
        {
            // Get the current sound info
            var soundInfo = SoundsCatalog.GetSound(_version, (ushort)_currentSound);

            // Save changes
            soundInfo.Chance = Int16.Parse(tbChance.Text);
            soundInfo.Volume = Int16.Parse(tbVolume.Text);
            soundInfo.Range = Int16.Parse(tbRange.Text);
            soundInfo.Pitch = Int16.Parse(tbPitch.Text);
            soundInfo.FlagL = soundInfo.FlagW = soundInfo.FlagR = false;
            if (comboLoop.SelectedIndex == 1)
                soundInfo.FlagW = true;
            else if (comboLoop.SelectedIndex == 2)
                soundInfo.FlagR = true;
            else if (comboLoop.SelectedIndex == 3)
                soundInfo.FlagL = true;
            soundInfo.FlagN = cbFlagN.Checked;
            soundInfo.FlagR = cbRandomizeGain.Checked;
            soundInfo.FlagP = cbRandomizePitch.Checked;
            soundInfo.Name = tbName.Text;
            soundInfo.MandatorySound = cbMandatorySound.Checked;
            soundInfo.NgLocked = cbNgLocked.Checked;

            soundInfo.Samples.Clear();
            foreach (var item in lstSamples.Items)
                soundInfo.Samples.Add(item.Text);

            lstSoundInfos.Items[lstSoundInfos.SelectedIndices[0]].Text = _currentSound.ToString().PadLeft(3, '0') + ": " + tbName.Text;

            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            /*string message = "Sound Infos: " + _wad.SoundInfo.Count + " of " +
                             _wad.SoundMapSize + "    " +
                             "Embedded WAV samples: " + _wad.Samples.Count;
            labelStatus.Text = message*/
        }

        private string GetSoundsPath()
        {
            switch (_version)
            {
                case WadGameVersion.TR2: return "Sounds\\TR2";
                case WadGameVersion.TR3: return "Sounds\\TR3";
                case WadGameVersion.TR4_TRNG: return "Sounds\\TR4";
                case WadGameVersion.TR5: return "Sounds\\TR5";
                default:
                    throw new NotSupportedException();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.Show(this, "Do you really want to save changes to the catalogs?",
                                    "Confirm save", MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            SoundsCatalog.SaveToXml(GetSoundsPath() + "\\Sounds.xml", _version);
        }

        private void convertTXTToXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string input;
            string output;

            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Open Sounds.txt file";
                dialog.Filter = "Text file (*.txt)|*.txt";

                if (dialog.ShowDialog() == DialogResult.Cancel)
                    return;

                input = dialog.FileName;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Save XML file";
                dialog.Filter = "XML file (*.xml)|*.xml";

                if (dialog.ShowDialog() == DialogResult.Cancel)
                    return;

                output = dialog.FileName;
            }

            if (input == output)
            {
                DarkMessageBox.Show(this, "You can't select the same file as input TXT and output XML",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (SoundsCatalog.ConvertTxtFileToXml(input, output))
            {
                DarkMessageBox.Show(this, "Your sounds TXT file was converted corretly into the new XML file format",
                                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                DarkMessageBox.Show(this, "There was an error while converting your file",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void butPlaySound_Click(object sender, EventArgs e)
        {
            if (lstSamples.SelectedIndices.Count == 0) return;

            // Get the selected sound info
            var item = lstSamples.Items[lstSamples.SelectedIndices[0]];
            var sample = item.Text;

            string path = "Sounds\\";
            switch (_version)
            {
                case WadGameVersion.TR1: path += "TR1"; break;
                case WadGameVersion.TR2: path += "TR2"; break;
                case WadGameVersion.TR3: path += "TR3"; break;
                case WadGameVersion.TR4_TRNG: path += "TR4"; break;
                case WadGameVersion.TR5: path += "TR5"; break;
            }
            path += "\\Samples\\" + sample + ".wav";

            using (var player = new SoundPlayer(File.OpenRead(path)))
            {
                player.Play();
            }
        }

        private void butDeleteWave_Click(object sender, EventArgs e)
        {
            if (lstSamples.SelectedIndices.Count == 0) return;

            var item = lstSamples.Items[lstSamples.SelectedIndices[0]];
            var wave = item.Text;

            // Ask to the user the permission to delete WAV
            if (DarkMessageBox.Show(this,
                   "Are you really sure to delete '" + wave + "'?",
                   "Delete sample", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            lstSamples.Items.Remove(item);
        }

        private void tR1CatalogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _version = WadGameVersion.TR1;
            buildMAINSFXToolStripMenuItem.Enabled = false;
            ReloadSounds();
        }

        private void buildMAINSFXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var path = "Sounds\\" + (_version == WadGameVersion.TR2 ? "TR2" : "TR3");
            /*using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.Cancel)
                    return;
                path = dialog.SelectedPath;
            }*/

            // Check if MAIN.SFX already exists
            var pathSfx = path + "\\MAIN.SFX";
            var pathSam = path + "\\MAIN.SAM";
            if (File.Exists(pathSfx))
            {
                if (DarkMessageBox.Show(this, "The MAIN.SFX file already exists. Do you want to overwrite it?",
                                        "Build MAIN.SFX", MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }

            // Load all samples and build the MAIN.SFX
            var catalog = SoundsCatalog.GetAllSounds(_version);

            try
            {
                if (File.Exists(pathSfx))
                    File.Delete(pathSfx);

                if (File.Exists(pathSam))
                    File.Delete(pathSam);

                using (var writer = new BinaryWriter(File.OpenWrite(pathSfx)))
                {
                    using (var writerSam = new BinaryWriter(File.OpenWrite(pathSam)))
                    {
                        int lastSample = 0;
                        foreach (var pair in catalog)
                        {
                            var sound = pair.Value;
                            foreach (var sample in sound.Samples)
                            {
                                var samplePath = "Sounds\\" + (_version == WadGameVersion.TR2 ? "TR2" : "TR3") + "\\Samples\\" +
                                                 sample + ".wav";

                                if (!File.Exists(samplePath))
                                    samplePath = "Editor\\Misc\\NullSample.wav";

                                using (var reader = new BinaryReader(File.OpenRead(samplePath)))
                                {
                                    var buffer = reader.ReadBytes((int)reader.BaseStream.Length);
                                    writer.Write(buffer);
                                }
                            }

                            writerSam.Write(lastSample);
                            lastSample += sound.Samples.Count;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show(this, "An error occurred while building the MAIN.SFX file. Message: " +
                                    ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DarkMessageBox.Show(this, "The MAIN.SFX was built correctly",
                                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
