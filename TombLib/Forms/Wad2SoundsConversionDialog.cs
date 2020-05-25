﻿using DarkUI.Forms;
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
    public partial class Wad2SoundsConversionDialog : DarkForm
    {
        public WadSounds Sounds { get; set; }

        private readonly TRVersion.Game _version;
        private readonly List<FileFormatConversions.SoundInfoConversionRow> _conversionRows;

        private bool _validateRow = true;

        public Wad2SoundsConversionDialog(TRVersion.Game version, List<FileFormatConversions.SoundInfoConversionRow> conversionRows)
        {
            _version = version.Native();
            _conversionRows = conversionRows;

            InitializeComponent();
        }

        private void Wad2SoundsConversionDialog_Load(object sender, EventArgs e)
        {
            // Try to load default Sounds.xml
            if (File.Exists("Catalogs\\Sounds.tr4.xml"))
            {
                string filename = Path.GetDirectoryName(Application.ExecutablePath) + "\\Catalogs\\Sounds.tr4.xml";
                var sounds = WadSounds.ReadFromFile(filename);
                if (sounds != null)
                {
                    Sounds = sounds;
                    tbSoundsCatalogPath.Text = filename;
                }
            }

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
                    DarkMessageBox.Show(this, "You have not selected a sound Id for sound '" +
                                        row.Cells[0].Value + "'. You must assign all sounds for converting your Wa2 file.",
                                        "Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);

                    return;
                }
            }

            bool continueProcedure = true;
            if (Sounds == null)
                foreach (DataGridViewRow row in dgvSoundInfos.Rows)
                {
                    bool exportToXml = (bool)row.Cells[3].Value;
                    bool exportSamples = (bool)row.Cells[4].Value;

                    if (exportToXml && !exportSamples)
                    {
                        if (DarkMessageBox.Show(this, "Are you trying to export sound info '" + row.Cells[2].Value + 
                                                "' to Xml " +
                                                "without having a valid sound catalog loaded. You will lose your samples names if you don't load " +
                                                "a catalog. Do you want to continue?", "Confirm",
                                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                            continueProcedure = false;
                        break;
                    }
                }

            if (!continueProcedure)
                return;

            for (int i = 0; i < _conversionRows.Count; i++)
            {
                DataGridViewRow row = dgvSoundInfos.Rows[i];

                _conversionRows[i].NewId = int.Parse(row.Cells[1].Value.ToString());
                _conversionRows[i].NewName = row.Cells[2].Value.ToString();
                _conversionRows[i].SaveToXml = (bool)row.Cells[3].Value;
                _conversionRows[i].ExportSamples = (bool)row.Cells[4].Value;
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
            DataGridViewRow row = dgvSoundInfos.Rows[e.RowIndex];

            if (e.ColumnIndex == 1)
            {
                int id;
                if (!int.TryParse(row.Cells[1].Value.ToString(), out id))
                {
                    row.DefaultCellStyle.BackColor = dgvSoundInfos.BackColor;
                    row.Cells[1].Value = "";
                    row.Cells[2].Value = "";
                    row.Cells[3].Value = false;
                    row.Cells[4].Value = false;
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
                            row.Cells[3].Value = false;
                            row.Cells[4].Value = false;

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
                        row.Cells[3].Value = false;
                        row.Cells[4].Value = false;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.DarkGreen;
                        row.Cells[2].Value = name;
                    }
                }
            }
            else if (e.ColumnIndex == 3)
            {
                if (!_validateRow)
                    return;
                _validateRow = false;

                if ((bool)row.Cells[3].Value == false)
                    row.Cells[4].Value = false;

                _validateRow = true;
            }
            else if (e.ColumnIndex == 4)
            {
                if (!_validateRow)
                    return;
                _validateRow = false;

                if ((bool)row.Cells[4].Value == true)
                    row.Cells[3].Value = true;

                _validateRow = true;
            }

            dgvSoundInfos.InvalidateRow(e.RowIndex);
            UpdateStatus();
        }

        private void butSearchSoundsCatalogPath_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, "Select sound catalog to import",
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

        private void butSelectAllSamples_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvSoundInfos.Rows)
                row.Cells[4].Value = true;

            dgvSoundInfos.Invalidate();
        }

        private void butUnselectAllSamples_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvSoundInfos.Rows)
                row.Cells[4].Value = false;

            dgvSoundInfos.Invalidate();
        }
    }
}
