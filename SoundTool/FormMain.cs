using System;
using System.Windows.Forms;
using TombLib.Wad;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Forms;
using TombLib.Utils;
using System.IO;
using TombLib.LevelData.IO;
using System.Collections.Generic;
using DarkUI.Config;

namespace SoundTool
{
    public partial class FormMain : DarkForm
    {
        private PopUpInfo popup = new PopUpInfo();
        private Configuration _configuration;

        public Level ReferenceLevel
        {
            get { return soundInfoEditor.ReferenceLevel; }
            set { soundInfoEditor.ReferenceLevel = value; }
        }

        private bool Saved
        {
            get { return _saved; }
            set { _saved = value; UpdateUI(); }
        }
        private bool _saved = true;
        private string _currentArchive = null;

        public FormMain(string archive = null)
        {
            // Load config
            _configuration = new Configuration().LoadOrUseDefault<Configuration>();

            // Account for brightness loaded from config
            Colors.Brightness = _configuration.UI_FormColor_Brightness / 100.0f;
            BackColor = Colors.GreyBackground;

            // Do Designer stuff only now, when color config is loaded
            InitializeComponent();

            Configuration.LoadWindowProperties(this, _configuration);

            if (!string.IsNullOrEmpty(_configuration.SoundTool_ReferenceProject))
                LoadReferenceLevel(_configuration.SoundTool_ReferenceProject);

            // If no args, create new catalog
            if (archive == null || !OpenArchive(archive))
                CreateNewArchive();

            UpdateUI();
        }
        public FormMain(Level level, string archive) : this(archive) { ReferenceLevel = level; }

        private void CreateNewArchive()
        {
            CheckForSavedChanges();
            dgvSoundInfos.Rows.Clear();
            soundInfoEditor.SoundInfo = null;
            _currentArchive = null;
            Saved = true;
        }

        private bool OpenArchive(string filename = null)
        {
            CheckForSavedChanges();

            if (filename == null)
                filename = LevelFileDialog.BrowseFile(this, "Select archive to open",
                                                      WadSounds.FormatExtensions,
                                                      false);

            if (filename == null || !File.Exists(filename))
                return false;

            // Read the sounds archive in XML or TXT format
            var sounds = WadSounds.ReadFromFile(filename);

            if (sounds == null)
                return false;

            dgvSoundInfos.Rows.Clear();

            // Fill the grid
            sounds.SoundInfos.Sort((a, b) => a.Id.CompareTo(b.Id));
            foreach (var soundInfo in sounds.SoundInfos)
            {
                dgvSoundInfos.Rows.Add(soundInfo.Id.ToString().PadLeft(4, '0'), soundInfo.Name);
                dgvSoundInfos.Rows[dgvSoundInfos.Rows.Count - 1].Tag = soundInfo;
            }

            SelectSoundInfo();

            // Decide on saved flag based on file format.
            // If TXT was loaded (i.e. conversion was made), mark the file as unsaved.
            var extension = Path.GetExtension(filename);
            if (extension == ".xml")
            {
                _currentArchive = filename;
                Saved = true;
            }
            else
            {
                _currentArchive = null;
                Saved = false;
            }
            return true;
        }

        private void SaveArchive(string filename = null)
        {
            if (filename == null)
                filename = LevelFileDialog.BrowseFile(this, "Save sounds catalog to XML",
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

            _currentArchive = filename;
            Saved = true;
        }

        private int FindFreeSoundID()
        {
            List<int> ids = new List<int>();
            foreach (DataGridViewRow row in dgvSoundInfos.Rows) ids.Add(((WadSoundInfo)row.Tag).Id);
            ids.Sort();
            int next = -1;
            foreach (var id in ids) { next++; if (id != next) return next; }
            return next + 1;
        }

        private void AddSoundInfo()
        {
            using (var form = new FormInputBox("Add new sound info", "Insert the ID of the new sound:", FindFreeSoundID().ToString()))
            {
                form.StartPosition = FormStartPosition.CenterParent;
                if (form.ShowDialog() == DialogResult.Cancel)
                    return;

                // Now check if ID already exists
                int id = 0;
                if (!int.TryParse(form.Result, out id))
                {
                    popup.ShowError(soundInfoEditor, "You have inserted a wrong number");
                    return;
                }

                foreach (DataGridViewRow row in dgvSoundInfos.Rows)
                {
                    WadSoundInfo info = (WadSoundInfo)row.Tag;

                    if (info.Id == id)
                    {

                        popup.ShowError(soundInfoEditor, "The ID that you have inserted is already assigned to sound \n" + info.Name);
                        dgvSoundInfos.ClearSelection();
                        row.Selected = true;
                        dgvSoundInfos.FirstDisplayedScrollingRowIndex = row.Index;
                        return;
                    }
                }

                WadSoundInfo newInfo = new WadSoundInfo(id);
                newInfo.Name = "NEW_SOUND";

                int newIndex = dgvSoundInfos.Rows.Count;
                dgvSoundInfos.Rows.Add(newInfo.Id.ToString().PadLeft(4, '0'), newInfo.Name);
                dgvSoundInfos.ClearSelection();
                dgvSoundInfos.Rows[newIndex].Tag = newInfo;
                dgvSoundInfos.Rows[newIndex].Selected = true;
                dgvSoundInfos.FirstDisplayedScrollingRowIndex = newIndex;
                soundInfoEditor.SoundInfo = newInfo;

                Saved = false;
            }
        }

        private void DeleteSoundInfos()
        {
            if (dgvSoundInfos.SelectedRows.Count == 0)
                return;

            if (DarkMessageBox.Show(this, "Do you really want to delete selected sound infos?", "Delete",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (DataGridViewRow row in dgvSoundInfos.SelectedRows)
                    dgvSoundInfos.Rows.Remove(row);
            }

            Saved = false;
        }

        private void SelectSoundInfo(int index = 0)
        {
            if (dgvSoundInfos.Rows.Count == 0 ||
                dgvSoundInfos.Rows.Count <= index ||
                dgvSoundInfos.Rows[index].Tag == null)
            {
                soundInfoEditor.SoundInfo = null;
                return;
            }

            var soundInfo = (WadSoundInfo)dgvSoundInfos.Rows[index].Tag;
            soundInfoEditor.SoundInfo = soundInfo;
        }

        private void UpdateUI()
        {
            var refLoaded = ReferenceLevel != null;

            string winTitle = "SoundTool - " + (_currentArchive == null ? "Untitled" : _currentArchive);
            if (!_saved) winTitle += "*";
            Text = winTitle;

            string message = "Sound Infos: " + dgvSoundInfos.Rows.Count;

            if (refLoaded)
                message += " | Reference project: " + ReferenceLevel.Settings.MakeAbsolute(ReferenceLevel.Settings.LevelFilePath);
            labelStatus.Text = message;

            unloadReferenceProjectToolStripMenuItem.Enabled = refLoaded;
            saveToolStripMenuItem.Enabled = _currentArchive == null || !Saved;
        }

        private bool CheckForSavedChanges()
        {
            if (!_saved)
            {
                if (DarkMessageBox.Show(this, "Save changes to already opened file?",
                                       "Confirm unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SaveArchive(_currentArchive);
                    return true;
                }
                else
                    return false;
            }
            return true;
        }

        private void LoadReferenceLevel(string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = LevelFileDialog.BrowseFile(this, "Open Tomb Editor reference project", LevelSettings.FileFormatsLevel, false);

            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                return;

            ReferenceLevel = Prj2Loader.LoadFromPrj2(fileName, null, new Prj2Loader.Settings { IgnoreTextures = true, IgnoreWads = true });
            _configuration.SoundTool_ReferenceProject = fileName;
            UpdateUI();
        }

        private void UnloadReferenceLevel()
        {
            ReferenceLevel = null;
            _configuration.SoundTool_ReferenceProject = string.Empty;
            UpdateUI();
        }

        private void SearchForSound()
        {
            if (dgvSoundInfos.Rows.Count == 0 || dgvSoundInfos.CurrentRow == null)
            {
                popup.ShowInfo(soundInfoEditor, "No sounds present. Nothing to search.");
                return;
            }

            var currentRow = dgvSoundInfos.CurrentRow.Index;
            if (currentRow == -1 || currentRow == dgvSoundInfos.Rows.Count - 1) currentRow = 0;

            RestartSearch:
            for (int i = currentRow + 1; i < dgvSoundInfos.Rows.Count; i++)
            {
                var item = (WadSoundInfo)dgvSoundInfos.Rows[i].Tag;
                if (item.Name.IndexOf(tbSearch.Text, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                    item.Id.ToString().IndexOf(tbSearch.Text) >= 0)
                {
                    dgvSoundInfos.ClearSelection();
                    dgvSoundInfos.Rows[i].Selected = true;
                    dgvSoundInfos.CurrentCell = dgvSoundInfos.Rows[i].Cells[0];
                    return;
                }
            }

            if (currentRow > 0)
            {
                currentRow = 0;
                goto RestartSearch;
            }
        }

        private void NewXMLToolStripMenuItem_Click(object sender, EventArgs e) => CreateNewArchive();
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e) => OpenArchive();
        private void saveToolStripMenuItem_Click(object sender, EventArgs e) => SaveArchive(_currentArchive);
        private void saveXMLAsToolStripMenuItem_Click(object sender, EventArgs e) => SaveArchive();
        private void loadReferenceLevelToolStripMenuItem_Click(object sender, EventArgs e) => LoadReferenceLevel();
        private void unloadReferenceProjectToolStripMenuItem_Click(object sender, EventArgs e) => UnloadReferenceLevel();
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void butAddNewSoundInfo_Click(object sender, EventArgs e) => AddSoundInfo();
        private void butDeleteSoundInfo_Click(object sender, EventArgs e) => DeleteSoundInfos();
        private void butSearch_Click(object sender, EventArgs e) { SearchForSound(); }
        private void tbSearch_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) SearchForSound(); }

        private void AboutSoundToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormAbout(Properties.Resources.misc_AboutScreen_800))
                form.ShowDialog(this);
        }

        private void dgvSoundInfos_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSoundInfos.SelectedRows.Count == 0)
                return;
            SelectSoundInfo(dgvSoundInfos.SelectedRows[0].Index);
        }

        private void soundInfoEditor_SoundInfoChanged(object sender, EventArgs e)
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

                    Saved = false;
                    return;
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            CheckForSavedChanges();
            Configuration.SaveWindowProperties(this, _configuration);
            _configuration.SaveTry();
            WadSoundPlayer.StopSample();
            base.OnFormClosing(e);
        }

        protected override void WndProc(ref Message message)
        {
            switch (message.Msg)
            {
                case SingleInstanceManagement.WM_COPYDATA:
                    var fileName = SingleInstanceManagement.Catch(ref message);
                    if (fileName != null)
                    {
                        SingleInstanceManagement.RestoreWindowState(this);
                        OpenArchive(fileName);
                    }
                    break;

                case SingleInstanceManagement.WM_SHOWWINDOW:
                    SingleInstanceManagement.RestoreWindowState(this);
                    break;

                default:
                    base.WndProc(ref message);
                    break;
            }
        }

        private void optionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var formInput = new FormInputBox("UI brightness", "Specify UI brightness (50-100, requires restart):", _configuration.UI_FormColor_Brightness.ToString()))
            {
                formInput.StartPosition = FormStartPosition.CenterParent;
                if (formInput.ShowDialog(this) != DialogResult.Cancel)
                {
                    float newBrightness = _configuration.UI_FormColor_Brightness;
                    if (float.TryParse(formInput.Result, out newBrightness))
                    {
                        newBrightness = Math.Max(Math.Min(newBrightness, 100), 50);
                        _configuration.UI_FormColor_Brightness = newBrightness;
                    }
                }
            }
        }
    }
}
