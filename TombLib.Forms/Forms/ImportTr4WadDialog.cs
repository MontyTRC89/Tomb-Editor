using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Utils;
using TombLib.Wad.Tr4Wad;

namespace TombLib.Forms
{
    public partial class ImportTr4WadDialog : DarkForm
    {
        private DialogDescriptonMissingSounds _dialogInfo;

        public ImportTr4WadDialog(DialogDescriptonMissingSounds dialogInfo)
        {
            _dialogInfo = dialogInfo;
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
            foreach (var path in _dialogInfo.SoundPaths)
                lstPaths.Items.Add(path);
        }

        private void ReloadSamples()
        {
            dgvSamples.Rows.Clear();

            foreach (var info in _dialogInfo.Samples)
                dgvSamples.Rows.Add(info.Sample, info.Path, "Search", info.Found);

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var foundSamples = 0;
            foreach (var info in _dialogInfo.Samples)
                if (info.Found) foundSamples++;

            var numSamples = _dialogInfo.Samples.Count;
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
            _dialogInfo.FindTr4Samples();
            ReloadSamples();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (!_dialogInfo.FindTr4Samples())
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
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void butAddPath_Click(object sender, EventArgs e)
        {
            using (var dialog = new BrowseFolderDialog())
            {
                dialog.Title = "Select a new sound folder (should contain *.wav audio files)";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _dialogInfo.SoundPaths.Add(dialog.Folder);
                    _dialogInfo.FindTr4Samples();
                    ReloadSoundPaths();
                    ReloadSamples();
                }
            }
        }

        private void butDeletePath_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.Show(this, "Are you really sure to delete selected path?", "Delete sound path",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _dialogInfo.SoundPaths.RemoveAt(lstPaths.SelectedIndex);
                _dialogInfo.FindTr4Samples();
                ReloadSoundPaths();
                ReloadSamples();
            }
        }

        private void lstPaths_Click(object sender, EventArgs e)
        {
            butDeletePath.Enabled = lstPaths.SelectedIndex != -1 && lstPaths.Items.Count != 0;
        }

        private void dgvSamples_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.RowIndex < 0) || (e.RowIndex >= dgvSamples.Rows.Count))
                return;

            if (e.ColumnIndex == 2)
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Title = "Search WAV sample";
                    dialog.Filter = "WAV sample (*.wav)|*.wav|All files (*.*)|*.*";
                    if (dialog.ShowDialog(this) == DialogResult.OK && File.Exists(dialog.FileName))
                    {
                        _dialogInfo.Samples[e.RowIndex].Path = dialog.FileName;
                        dgvSamples.Rows[e.RowIndex].Cells[1].Value = dialog.FileName;
                        dgvSamples.Rows[e.RowIndex].Cells[3].Value = true;

                        UpdateStatus();
                    }
                }
            }
        }

        private void lstPaths_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Modifiers.HasFlag(Keys.Control))
                Clipboard.SetText(lstPaths.SelectedItem?.ToString() ?? "");
        }
    }
}
