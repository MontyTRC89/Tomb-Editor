using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Wad.Tr4Wad;

namespace TombLib.Forms
{
    public partial class FormImportTr4Wad : DarkForm
    {
        public bool IgnoreMissingSamples { get; set; }

        public FormImportTr4Wad()
        {
            InitializeComponent();
        }

        private void FormImportTr4Wad_Load(object sender, EventArgs e)
        {
            // Add sound paths
            ReloadSoundPaths();

            // Add samples
            ReloadSamples();
        }

        private void ReloadSoundPaths()
        {
            lstPaths.Items.Clear();
            foreach (var path in Tr4WadOperations.SoundPaths)
                lstPaths.Items.Add(path);
        }

        private void ReloadSamples()
        {
            dgvSamples.Rows.Clear();

            var foundSamples = 0;
            foreach (var info in Tr4WadOperations.Samples)
            {
                dgvSamples.Rows.Add(info.Sample, info.Path, info.Found);
                if (info.Found) foundSamples++;
            }

            var numSamples = Tr4WadOperations.Samples.Count;
            var missingSamples = numSamples - foundSamples;

            statusSamples.Text = "Samples = " + numSamples + " | Found = " + foundSamples + " | Missing = " + missingSamples;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butReloadSamples_Click(object sender, EventArgs e)
        {
            Tr4WadOperations.FindTr4Samples();
            ReloadSamples();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (!Tr4WadOperations.FindTr4Samples())
            {
                var result = DarkMessageBox.Show(this, "Warning: some samples are still missing. " + Environment.NewLine + 
                                                 "If you continue, you'll have to fix them manually with sound manager. " + Environment.NewLine +
                                                 "Do you want to continue and ignore missing files? You can also abort the importing process",
                                                 "Missing samples",
                                                 MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.Yes: DialogResult = DialogResult.OK; Close(); break;
                    case DialogResult.Cancel: DialogResult = DialogResult.Cancel; break;
                    default: return;
                }
            }
        }

        private void butAddPath_Click(object sender, EventArgs e)
        {
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                Tr4WadOperations.SoundPaths.Add(folderBrowser.SelectedPath);
                Tr4WadOperations.FindTr4Samples();
                ReloadSoundPaths();
                ReloadSamples();
            }
        }

        private void butDeletePath_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.Show(this, "Are you really sure to delete selected path?", "Delete sound path",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Tr4WadOperations.SoundPaths.RemoveAt(lstPaths.SelectedIndex);
                Tr4WadOperations.FindTr4Samples();
                ReloadSoundPaths();
                ReloadSamples();
            }
        }

        private void lstPaths_Click(object sender, EventArgs e)
        {
            butDeletePath.Enabled = lstPaths.SelectedIndex != -1 && lstPaths.Items.Count != 0;
        }
    }
}
