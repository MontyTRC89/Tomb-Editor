using System;
using System.Windows.Forms;
using TombLib.Wad;
using DarkUI.Controls;
using DarkUI.Forms;
using System.IO;
using System.Media;
using TombLib.Utils;
using TombLib.LevelData;
using TombLib.Forms;
using TombLib.Wad.Catalog;

namespace SoundTool
{
    public partial class FormMain : DarkForm
    {
        public FormMain()
        {
            InitializeComponent();
            CreateNewArchive();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void UpdateStatistics()
        {
            /*string message = "Sound Infos: " + _wad.SoundInfo.Count + " of " +
                             _wad.SoundMapSize + "    " +
                             "Embedded WAV samples: " + _wad.Samples.Count;
            labelStatus.Text = message*/
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*if (DarkMessageBox.Show(this, "Do you really want to save changes to the catalogs?",
                                    "Confirm save", MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) != DialogResult.Yes)
                return;*/

            string filename = LevelFileDialog.BrowseFile(this, "Save sounds catalog to XML",
                                                         LevelSettings.FileFormatsSoundsXmlFiles,
                                                         true);
            if (filename == null)
                return;

            var sounds = new WadSounds();
            foreach (DataGridViewRow row in dgvSoundInfos.Rows)
            {
                WadSoundInfo info = (WadSoundInfo)row.Tag;
                sounds.SoundInfos.Add(info);
            }

            sounds.SoundInfos.Sort((a, b) => a.Id.CompareTo(b.Id));
            WadSounds.SaveToXml(filename, sounds);
        }

        private void buildMAINSFXToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void AboutSoundToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormAbout(Properties.Resources.misc_AboutScreen_800))
                form.ShowDialog(this);
        }

        private void NewXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewArchive();
        }

        private void CreateNewArchive()
        {
            dgvSoundInfos.Rows.Clear();
            soundInfoEditor.SoundInfo = null;
            this.Text = "Sound Tool - Untitled";
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = LevelFileDialog.BrowseFile(this, "Select archive to import",
                                                         LevelSettings.FileFormatsSoundsCatalogs,
                                                         false);
            if (filename == null)
                return;

            WadSounds sounds;

            // Read the sounds archive in XML or TXT format
            sounds = WadSounds.ReadFromFile(filename);

            dgvSoundInfos.Rows.Clear();

            // Fill the grid
            sounds.SoundInfos.Sort((a, b) => a.Id.CompareTo(b.Id));
            foreach (var soundInfo in sounds.SoundInfos)
            {
                dgvSoundInfos.Rows.Add(soundInfo.Id.ToString().PadLeft(4, '0'), soundInfo.Name);
                dgvSoundInfos.Rows[dgvSoundInfos.Rows.Count - 1].Tag = soundInfo;
            }

            soundInfoEditor.SoundInfo = null;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }

        private void DgvSoundInfos_Click(object sender, EventArgs e)
        {
            if (dgvSoundInfos.SelectedRows.Count == 0)
                return;

            var soundInfo = (WadSoundInfo)dgvSoundInfos.SelectedRows[0].Tag;
            soundInfoEditor.SoundInfo = soundInfo;
        }

        private void ButAddNewSoundInfo_Click(object sender, EventArgs e)
        {
            using (var form = new FormInputBox("Add new sound info", "Inser the ID of the new sound:"))
            {
                if (form.ShowDialog() == DialogResult.Cancel)
                    return;

                // Now check if ID already exists
                int id = 0;
                if (!int.TryParse(form.Result, out id))
                {
                    DarkMessageBox.Show(this, "You have inserted a wrong number", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                foreach (DataGridViewRow row in dgvSoundInfos.Rows)
                {
                    WadSoundInfo info = (WadSoundInfo)row.Tag;

                    if (info.Id == id)
                    {
                        DarkMessageBox.Show(this, "The ID that you have inserted is already assigned to sound " +
                                                  "\"" + info.Name + "\"", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                WadSoundInfo newInfo = new WadSoundInfo(id);
                newInfo.Name = "NEW_SOUND";

                int newIndex = 0;
                foreach (DataGridViewRow row in dgvSoundInfos.Rows)
                {
                    WadSoundInfo info = (WadSoundInfo)row.Tag;
                    if (id > info.Id)
                        break;
                    else
                        newIndex++;
                }

                dgvSoundInfos.Rows.Insert(newIndex, newInfo.Id.ToString().PadLeft(4, '0'), newInfo.Name);
            }
        }

        private void SoundInfoEditor_SoundInfoChanged(object sender, EventArgs e)
        {
            // Update name if necessary / save new sound info
            foreach (DataGridViewRow row in dgvSoundInfos.Rows)
            {
                WadSoundInfo info = (WadSoundInfo)row.Tag;
                WadSoundInfo newInfo = soundInfoEditor.SoundInfo;
                if (info.Id == newInfo.Id)
                {
                    row.Tag = newInfo;
                    row.Cells[1].Value = newInfo.Name;
                    dgvSoundInfos.InvalidateRow(row.Index);
                    return;
                }
            }
        }

        private void ButDeleteSoundInfo_Click(object sender, EventArgs e)
        {
            if (dgvSoundInfos.SelectedRows.Count == 0)
                return;

            if (DarkMessageBox.Show(this,"Do you really want to delete selected sound infos?", "Delete",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (DataGridViewRow row in dgvSoundInfos.SelectedRows)
                    dgvSoundInfos.Rows.Remove(row);
            }
        }
    }
}
