using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.Forms
{
    public partial class Prj2SoundsConversionDialog : DarkForm
    {
        public WadSounds Sounds { get; set; }

        private readonly TRVersion.Game _version;
        private readonly List<FileFormatConversions.SoundInfoConversionRow> _conversionRows;
       
        public Prj2SoundsConversionDialog(TRVersion.Game version, List<FileFormatConversions.SoundInfoConversionRow> conversionRows)
        {
            _version = version.Native();
            _conversionRows = conversionRows;

            InitializeComponent();
        }

        private void Prj2SoundsConversionDialog_Load(object sender, EventArgs e)
        {
            // Add rows
            ReloadSoundInfos();
        }

        private void ReloadSoundInfos()
        {
            dgvSoundInfos.Rows.Clear();

            foreach (var row in _conversionRows)
            {
                dgvSoundInfos.Rows.Add(row.OldName, (row.NewId != -1 ? row.NewId.ToString() : ""), row.NewName, 
                                       row.SaveToXml, row.ExportSamples);
                if (row.NewId != -1)
                    dgvSoundInfos.Rows[dgvSoundInfos.Rows.Count - 1].DefaultCellStyle.BackColor = Color.DarkGreen;
            }

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var fixedSounds = 0;
            var soundsToSave = 0;
            foreach (var row in _conversionRows)
            {
                if (row.NewId != -1)
                    fixedSounds++;
                if (row.SaveToXml)
                    soundsToSave++;
            }

            var numSounds = _conversionRows.Count;
            var missingSounds = numSounds - fixedSounds;

            statusSamples.Text = "Sounds = " + numSounds + " | Fixed = " + fixedSounds + " | Missing = " + missingSounds;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvSoundInfos.Rows)
            {
                int id;
                string value = row.Cells[1].Value.ToString();

                if (value == "" || !int.TryParse(value, out id))
                {
                    if (DarkMessageBox.Show(this, "You have not selected a sound Id for some sounds of the list. Do you want to continue?",
                                            "Error",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                    else
                        break;
                }
            }

            for (int i = 0; i < _conversionRows.Count; i++)
            {
                DataGridViewRow row = dgvSoundInfos.Rows[i];

                string strId = row.Cells[1].Value.ToString();
                if (strId == "" || strId == null)
                    continue;

                _conversionRows[i].NewId = int.Parse(strId);
                _conversionRows[i].NewName = row.Cells[2].Value.ToString();
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void dgvSamples_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 3)
            {
                DataGridViewRow row = dgvSoundInfos.Rows[e.RowIndex];
                row.Cells[e.ColumnIndex].Value = !((bool)row.Cells[e.ColumnIndex].Value);
            }
        }

        private void ButSelectAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvSoundInfos.Rows)
                row.Cells[3].Value = true;

            dgvSoundInfos.Invalidate();
        }

        private void ButUnselectAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvSoundInfos.Rows)
                row.Cells[3].Value = false;

            dgvSoundInfos.Invalidate();
        }

        private void DgvSoundInfos_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            
        }

        private void DgvSoundInfos_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 1)
                return;

            DataGridViewRow row = dgvSoundInfos.Rows[e.RowIndex];

            int id;
            if (!int.TryParse(row.Cells[1].Value.ToString(), out id))
            {
                row.DefaultCellStyle.BackColor = dgvSoundInfos.BackColor;
                row.Cells[1].Value = "";
                row.Cells[2].Value = "";
            }
            else
            {
                // Search if this Id was already assigned
                foreach (DataGridViewRow row2 in dgvSoundInfos.Rows)
                {
                    // Ignore the same row
                    if (row2.Index == row.Index)
                        continue;

                    // Ignore empty values
                    int id2;
                    if (!int.TryParse(row2.Cells[1].Value.ToString(), out id2))
                        continue;

                    // If is the same then warn the user
                    if (id == id2)
                    {
                        row.DefaultCellStyle.BackColor = dgvSoundInfos.BackColor;
                        row.Cells[1].Value = "";
                        row.Cells[2].Value = "";

                        DarkMessageBox.Show(this, "The selected Id " + id + " was already assigned to sound '" +
                                            row2.Cells[0].Value + "'", "Error",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // If additional catalog is loaded, this has the priority
                string name = "";
                if (Sounds != null)
                {
                    var info = Sounds.TryGetSoundInfo(id);
                    if (info != null)
                        name = info.Name;
                    else
                        name = TrCatalog.GetOriginalSoundName(_version, (uint)id);
                }
                else
                    name = TrCatalog.GetOriginalSoundName(_version, (uint)id);

                if (name == null || name == "")
                {
                    row.DefaultCellStyle.BackColor = dgvSoundInfos.BackColor;
                    row.Cells[1].Value = "";
                    row.Cells[2].Value = "";
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.DarkGreen;
                    row.Cells[2].Value = name;
                }
            }

            dgvSoundInfos.InvalidateRow(e.RowIndex);
            UpdateStatus();
        }

        private void butSearchSoundsCatalogPath_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, "Select sounds catalog to import",
                                                       LevelSettings.FileFormatsSoundsCatalogs,
                                                       false);
            if (result != null)
            {
                var sounds = WadSounds.ReadFromFile(result);
                if (sounds == null) return;
                Sounds = sounds;
                tbSoundsCatalogPath.Text = result;
            }
        }
    }
}
