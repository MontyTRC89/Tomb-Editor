using System;
using System.Windows.Forms;
using TombLib.Wad;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Forms;
using TombLib.Utils;
using System.Drawing;
using System.IO;
using TombLib.LevelData.IO;
using System.Collections.Generic;
using TombLib.IO;

namespace SoundTool
{
    public partial class FormMain : DarkForm
    {
        private static readonly IReadOnlyCollection<FileFormat> 
            _fileFormatSfx = new[] { new FileFormat("Tomb Raider 2/3 MAIN.SFX", "sfx") };

        private PopUpInfo popup = new PopUpInfo();
        private Configuration _configuration;

        private Level ReferenceLevel
        {
            get { return soundInfoEditor.ReferenceLevel; }
            set { soundInfoEditor.ReferenceLevel = value; }
        }

        private WadSounds Sounds
        {
            get
            {
                var sounds = new WadSounds();
                foreach (DataGridViewRow row in dgvSoundInfos.Rows)
                {
                    WadSoundInfo info = (WadSoundInfo)row.Tag;
                    sounds.SoundInfos.Add(info);
                }
                return sounds;
            }
        }

        private bool Saved
        {
            get { return _saved; }
            set { _saved = value; UpdateUI(); }
        }
        private bool _saved = true;
        private string _currentArchive = null;

        public FormMain(Configuration config, string archive = null, string refLevel = null)
        {
            // Load config
            _configuration = config;
            
            InitializeComponent();
            Configuration.LoadWindowProperties(this, _configuration);
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            // Load either specified ref level or from config (if exists)
            if (!string.IsNullOrEmpty(refLevel))
                LoadReferenceLevel(refLevel);
            else if (!string.IsNullOrEmpty(_configuration.SoundTool_ReferenceProject))
                LoadReferenceLevel(_configuration.SoundTool_ReferenceProject);

            // If no args, create new catalog
            if (archive == null || !OpenArchive(archive))
                CreateNewArchive();

            UpdateUI();
        }

        private void CreateNewArchive()
        {
            if (CheckForSavedChanges() == null)
                return;

            dgvSoundInfos.Rows.Clear();
            soundInfoEditor.SoundInfo = null;
            _currentArchive = null;
            Saved = true;
        }

        private bool OpenArchive(string filename = null)
        {
            if (CheckForSavedChanges() == null)
                return false;

            if (filename == null)
                filename = LevelFileDialog.BrowseFile(this, null, _configuration.SoundTool_LastCatalogPath, "Select archive to open",
                                                      WadSounds.FormatExtensions, null,
                                                      false);

            if (filename == null || !File.Exists(filename))
                return false;

            try
            {
                // Read the sounds archive in XML or TXT format
                var sounds = WadSounds.ReadFromFile(filename);

                if (sounds == null)
                    return false;

                // File read correctly, save catalog path to recent
                _configuration.SoundTool_LastCatalogPath = filename;

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
            catch (Exception ex)
            {
                popup.ShowError(soundInfoEditor, "Loading failed. Exception: " + ex.Message);
                return false;
            }
        }

        private void SaveArchive(string filename = null)
        {
            if (filename == null)
                filename = LevelFileDialog.BrowseFile(this, "Save sound catalog to XML",
                                                         LevelSettings.FileFormatsSoundsXmlFiles,
                                                         true);
            if (filename == null)
                return;

            Sounds.SoundInfos.Sort((a, b) => a.Id.CompareTo(b.Id));
            WadSounds.SaveToXml(filename, Sounds);

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

            if (dgvSoundInfos.Rows.Count == 0)
                soundInfoEditor.SoundInfo = null;

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

        private void UpdateUI(bool reloadSoundInfo = true)
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

            if (reloadSoundInfo && dgvSoundInfos.SelectedRows.Count > 0 && dgvSoundInfos.Rows.Count > 0)
                SelectSoundInfo(dgvSoundInfos.SelectedRows[0].Index);
        }

        private bool? CheckForSavedChanges()
        {
            if (!_saved)
            {
                switch (DarkMessageBox.Show(this, "Save changes to already opened file?",
                         "Confirm unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        SaveArchive(_currentArchive);
                        return true;

                    case DialogResult.No:
                        return false;

                    case DialogResult.Cancel:
                        return null;
                }
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

        private void CompileMainSFX(TRVersion.Game version, bool onlyIndexed)
        {
            var sounds = Sounds.SoundInfos;

            if (sounds.Count == 0)
            {
                popup.ShowError(soundInfoEditor, "No sounds present. Nothing to compile!");
                return;
            }

            var settings = new LevelSettings() { GameVersion = version };
            settings.WadSoundPaths.Clear();
            var samplePath = LevelFileDialog.BrowseFolder(this, null, _configuration.SoundTool_LastMainSFXSamplePath,
                "Choose a path where all samples are stored", null);
            if (string.IsNullOrEmpty(samplePath))
                return; // User cancelled path selection
            settings.WadSoundPaths.Add(new WadSoundPath(samplePath));
            _configuration.SoundTool_LastMainSFXSamplePath = samplePath; // Update settings

            var mainSFXPath = LevelFileDialog.BrowseFile(this, null, _configuration.SoundTool_LastMainSFXPath,
                    "Choose a path to save MAIN.SFX", _fileFormatSfx, null, true);

            if (string.IsNullOrEmpty(mainSFXPath))
                return; // User cancelled saving

            bool missing;
            var samples = WadSample.CompileSamples(sounds, settings, onlyIndexed, out missing);

            try
            {
                using (var writer = new BinaryWriterEx(new FileStream(mainSFXPath, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    foreach (var sample in samples.Values)
                        writer.Write(sample.Data, 0, sample.Data.Length);
                }

                var message = "MAIN.SFX compiled successfully!";
                if (missing) message += "\n" + "Some samples weren't found and won't play in game.";
                popup.ShowInfo(soundInfoEditor, message);
                _configuration.SoundTool_LastMainSFXPath = mainSFXPath;
            }
            catch (Exception ex)
            {
                popup.ShowError(soundInfoEditor, "There was a error writing MAIN.SFX. \nException: " + ex.Message);
            }
        }

        private void UnpackMainSFX()
        {     
            if (Sounds.SoundInfos.Count == 0)
            {
                popup.ShowError(soundInfoEditor, "No sounds present. Nothing to unpack!");
                return;
            }

            var mainSFXPath = LevelFileDialog.BrowseFile(this, null, _configuration.SoundTool_LastMainSFXPath,
                    "Choose MAIN.SFX file", _fileFormatSfx, null, false);
            if (string.IsNullOrEmpty(mainSFXPath) || !File.Exists(mainSFXPath))
                return; // User cancelled filename selection or file is unavailable

            var samplePath = LevelFileDialog.BrowseFolder(this, null, _configuration.SoundTool_LastMainSFXSamplePath,
                "Choose a path where all samples will be unpacked", null);
            if (string.IsNullOrEmpty(samplePath) || !Directory.Exists(samplePath))
                return; // User cancelled path selection or directory is unavailable

            var sampleCount = 0;
            var soundCount  = 0;

            using (var reader = new BinaryReaderEx(new FileStream(mainSFXPath, FileMode.Open, FileAccess.Read)))
            {
                try
                {
                    foreach (var sound in Sounds.SoundInfos)
                    {
                        if (!sound.Indexed)
                            continue;
                        else
                        {
                            foreach (var sample in sound.Samples)
                            {
                                if (reader.BaseStream.Position >= reader.BaseStream.Length - 8)
                                    throw new Exception("File is shorter than expected. Make sure you're using correct sound catalog and MAIN.sfx file.");

                                var header = reader.ReadInt32();
                                if (header != 0x46464952)
                                    throw new Exception("Unexpected header found in MAIN.sfx. File may be corrupted.");

                                var size = reader.ReadInt32();
                                if (size == 0)
                                    continue;

                                reader.BaseStream.Position -= 8;
                                var data = reader.ReadBytes(size + 8);

                                using (var writer = new BinaryWriterEx(new FileStream(Path.Combine(samplePath, sample.FileName), FileMode.OpenOrCreate)))
                                {
                                    writer.WriteBlockArray(data);
                                    writer.Close();
                                    sampleCount++;
                                }
                            }
                            soundCount++;
                        }
                    }

                    if (reader.BaseStream.Position != reader.BaseStream.Length)
                        throw new Exception("File is longer than expected. Make sure you're using correct sound catalog and MAIN.sfx file.");

                }
                catch (Exception ex)
                {
                    reader.Close();
                    popup.ShowError(soundInfoEditor, "There was a error unpacking MAIN.SFX. \nException: " + ex.Message);
                    return;
                }

                popup.ShowInfo(soundInfoEditor, "MAIN.SFX unpacked successfully!" + "\nExtracted " + sampleCount + " samples in " + soundCount + " sound infos.");
                _configuration.SoundTool_LastMainSFXPath = Directory.GetParent(mainSFXPath).FullName;
                _configuration.SoundTool_LastMainSFXSamplePath = samplePath; // Update settings
            }
        }

        private void IndexAllSounds()
        {
            Sounds.SoundInfos.ForEach(s => s.Indexed = true);
            UpdateUI();
        }

        private void UnindexAllSounds()
        {
            Sounds.SoundInfos.ForEach(s => s.Indexed = false);
            UpdateUI();
        }

        private void NewXMLToolStripMenuItem_Click(object sender, EventArgs e) => CreateNewArchive();
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e) => OpenArchive();
        private void saveToolStripMenuItem_Click(object sender, EventArgs e) => SaveArchive(_currentArchive);
        private void saveXMLAsToolStripMenuItem_Click(object sender, EventArgs e) => SaveArchive();
        private void loadReferenceLevelToolStripMenuItem_Click(object sender, EventArgs e) => LoadReferenceLevel();
        private void unloadReferenceProjectToolStripMenuItem_Click(object sender, EventArgs e) => UnloadReferenceLevel();
        private void indexStripMenuItem3_Click(object sender, EventArgs e) => IndexAllSounds();
        private void unindexStripMenuItem3_Click(object sender, EventArgs e) => UnindexAllSounds();
        private void buildMSFX2StripMenuItem_Click(object sender, EventArgs e) => CompileMainSFX(TRVersion.Game.TR2, true);
        private void buildMSFX3StripMenuItem_Click(object sender, EventArgs e) => CompileMainSFX(TRVersion.Game.TR3, true);
        private void unpackMAINSFXToolStripMenuItem_Click(object sender, EventArgs e) => UnpackMainSFX();
        private void copyToolStripMenuItem_Click(object sender, EventArgs e) => soundInfoEditor.Copy();
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e) => soundInfoEditor.Paste();
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void butAddNewSoundInfo_Click(object sender, EventArgs e) => AddSoundInfo();
        private void butDeleteSoundInfo_Click(object sender, EventArgs e) => DeleteSoundInfos();
        private void butSearch_Click(object sender, EventArgs e) { SearchForSound(); }
        private void tbSearch_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) SearchForSound(); }
        private void dgvSoundInfos_DoubleClick(object sender, EventArgs e) => soundInfoEditor.PlayCurrentSoundInfo();

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

                    _saved = false;
                    UpdateUI(false);
                    return;
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (CheckForSavedChanges() == null)
            {
                e.Cancel = true;
                return;
            }

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
            using (var formOptions = new FormOptions(_configuration))
                formOptions.ShowDialog();
        }
    }
}
