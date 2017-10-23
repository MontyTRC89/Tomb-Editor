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
using TombLib.Wad;

namespace WadTool
{
    public partial class FormSelectWave : DarkForm
    {
        public WadSample SelectedWave { get; private set; }

        private WadToolClass _tool;

        public FormSelectWave()
        {
            InitializeComponent();

            _tool = WadToolClass.Instance;
        }

        private void FormSelectWave_Load(object sender, EventArgs e)
        {
            ReloadWaves();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (lstWaves.SelectedIndices.Count == 0) return;

            var item = lstWaves.Items[lstWaves.SelectedIndices[0]];
            var wave = (WadSample)item.Tag;

            SelectedWave = wave;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butAddNewWave_Click(object sender, EventArgs e)
        {
            if (openFileDialogWave.ShowDialog() == DialogResult.Cancel) return;

            // Create the new WAVE
            WadSample sound;

            using (var reader = new BinaryReader(File.OpenRead(openFileDialogWave.FileName)))
            {
                sound = new WadSample(Path.GetFileName(openFileDialogWave.FileName),
                                     reader.ReadBytes((int)reader.BaseStream.Length));
            }

            // Check if the sound exists
            if (_tool.DestinationWad.Samples.ContainsKey(sound.Hash))
            {
                DarkMessageBox.Show(this, "The selected wave already exists in this Wad2", "Information", MessageBoxIcon.Information);
                return;
            }
            else
            {
                _tool.DestinationWad.Samples.Add(sound.Hash, sound);
                ReloadWaves();
            }
        }

        private void ReloadWaves()
        {
            lstWaves.Items.Clear();

            foreach (var wave in _tool.DestinationWad.Samples)
            {
                if (tbSearch.Text != "" && !wave.Value.Name.Contains(tbSearch.Text)) continue;

                var item = new DarkUI.Controls.DarkListItem(wave.Value.Name);
                item.Tag = wave.Value;
                lstWaves.Items.Add(item); 
            }
        }

        private void butPlaySound_Click(object sender, EventArgs e)
        {
            if (lstWaves.SelectedIndices.Count == 0) return;

            var item = lstWaves.Items[lstWaves.SelectedIndices[0]];
            var wave = (WadSample)item.Tag;

            wave.Play();
        }

        private void tbSearch_KeyUp(object sender, KeyEventArgs e)
        {
            ReloadWaves();
        }
    }
}
