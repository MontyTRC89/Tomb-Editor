using DarkUI.Controls;
using DarkUI.Forms;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombEditor.Controls;
using TombEditor.Geometry;
using TombEditor.Geometry.IO;

namespace TombEditor
{
    public partial class FormLevelSettings : DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private class PictureTooltip : ToolTip
        {
            public PictureTooltip()
            {
                OwnerDraw = true;
                Popup += OnPopup;
                Draw += OnDraw;
                InitialDelay = 10;
                AutoPopDelay = short.MaxValue; // Unfortunately the maximum possible value
                ShowAlways = true;
            }

            private void OnPopup(object sender, PopupEventArgs e) // use this event to set the size of the tool tip
            {
                e.ToolTipSize = new Size(513, 513);
            }

            private void OnDraw(object sender, DrawToolTipEventArgs e) // use this to customzie the tool tip
            {
                PictureBox parent = e.AssociatedControl as PictureBox;
                if (parent == null)
                    return;

                // Draw background
                if (parent.BackgroundImage != null)
                    using (Brush backgroundBrush = new TextureBrush(parent.BackgroundImage, WrapMode.Tile))
                        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
                else
                    using (Brush backgroundBrush = new SolidBrush(parent.BackColor))
                        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);

                // Draw image (if available)
                if (parent.Image != null)
                    e.Graphics.DrawImage(parent.Image, e.Bounds);

                // Draw error message (if necessary)
                Exception exc = parent.Tag as Exception;
                if (exc != null)
                {
                    Rectangle textArea = e.Bounds;
                    textArea.Inflate(-5, -5);
                    e.Graphics.DrawString(" The image could not be loaded.\n Message: " + exc.Message,
                        DefaultFont, Brushes.White, textArea);
                }

                // Draw border
                e.Graphics.DrawRectangle(Pens.Black, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }
        }

        private const string _specialTextureFilter =
                    "Image files (*.png, *.tga, *.bmp, *.pc, *.raw, *.jpg, *.jpeg)|*.png;*.tga;*.bmp;*.pc;*.raw;*.jpg;*.jpeg|" +
                    "PNG images (*.png)|*.png|" +
                    "Targe images (*.tga)|*.tga|" +
                    "Bitmap images (*.bmp)|*.bmp|" +
                    "Raw image files (*.pc, *.raw)|*.pc;*.raw|" +
                    "JPEG images (*.jpg, *.jpeg)|*.jpg;*.jpeg|" +
                    "All files (*.*)|*";

        private readonly Color _correctColor;
        private readonly Color _wrongColor;
        private Editor _editor;
        private LevelSettings _levelSettings;
        private string fontTextureFilePathPicPreviewCurrentPath;
        private string skyTextureFilePathPicPreviewCurrentPath;
        private PictureTooltip _pictureTooltip;
        private readonly BindingList<SoundPath> soundDataGridViewDataSource = new BindingList<SoundPath>();

        public FormLevelSettings(Editor editor)
        {
            InitializeComponent();

            _editor = editor;
            _levelSettings = editor.Level.Settings.Clone();

            // Initialize sound path data grid view
            foreach (var soundPath in _levelSettings.SoundPaths)
                soundDataGridViewDataSource.Add(soundPath.Clone());
            soundDataGridViewDataSource.ListChanged += delegate
                {
                    _levelSettings.SoundPaths.Clear();
                    foreach (var soundPath in soundDataGridViewDataSource)
                        _levelSettings.SoundPaths.Add(soundPath.Clone());
                };
            soundDataGridView.DataSource = soundDataGridViewDataSource;
            soundDataGridViewControls.DataGridView = soundDataGridView;
            soundDataGridViewControls.CreateNewRow = soundDataGridViewCreateNewRow;
            
            // Initialize picture previews
            _correctColor = gameLevelFilePathTxt.BackColor;
            _wrongColor = _correctColor.MixWith(Color.DarkRed, 0.55);
            skyTextureFilePathPicPreview.BackColor = _wrongColor;
            fontTextureFilePathPicPreview.BackColor = _wrongColor;

            _pictureTooltip = new PictureTooltip();
            _pictureTooltip.SetToolTip(fontTextureFilePathPicPreview, "Font Preview");
            _pictureTooltip.SetToolTip(skyTextureFilePathPicPreview, "Sky Preview");
            components.Add(_pictureTooltip);


            // Populate variable list
            foreach (VariableType variableType in Enum.GetValues(typeof(VariableType)))
                if (variableType != VariableType.None)
                {
                    pathVariablesDataGridView.Rows.Add(LevelSettings.VariableCreate(variableType), "");
                }
            
            // Initialize controls
            UpdateDialog();
        }
        
        private void UpdateDialog()
        {
            levelFilePathTxt.Text = _levelSettings.LevelFilePath;
            textureFilePathTxt.Text = _levelSettings.TextureFilePath;
            wadFilePathTxt.Text = _levelSettings.WadFilePath;
            gameDirectoryTxt.Text = _levelSettings.GameDirectory;
            gameLevelFilePathTxt.Text = _levelSettings.GameLevelFilePath;
            gameExecutableFilePathTxt.Text = _levelSettings.GameExecutableFilePath;
            soundsIgnoreMissingSounds.Checked = _levelSettings.IgnoreMissingSounds;
            gameExecutableSuppressAskingForOptionsCheckBox.Checked = _levelSettings.GameExecutableSuppressAskingForOptions;

            fontTextureFilePathOptAuto.Checked = string.IsNullOrEmpty(_levelSettings.FontTextureFilePath);
            fontTextureFilePathOptCustom.Checked = !string.IsNullOrEmpty(_levelSettings.FontTextureFilePath);
            fontTextureFilePathTxt.Enabled = !string.IsNullOrEmpty(_levelSettings.FontTextureFilePath);
            fontTextureFilePathBut.Enabled = !string.IsNullOrEmpty(_levelSettings.FontTextureFilePath);
            if (string.IsNullOrEmpty(_levelSettings.FontTextureFilePath))
                fontTextureFilePathTxt.Text = GetLevelResourcePath("Font.pc");
            else
                fontTextureFilePathTxt.Text = _levelSettings.FontTextureFilePath;

            skyTextureFilePathOptAuto.Checked = string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath);
            skyTextureFilePathOptCustom.Checked = !string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath);
            skyTextureFilePathTxt.Enabled = !string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath);
            skyTextureFilePathBut.Enabled = !string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath);
            if (string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath))
                skyTextureFilePathTxt.Text = GetLevelResourcePath("pcsky.raw");
            else
                skyTextureFilePathTxt.Text = _levelSettings.SkyTextureFilePath;

            // Check correctness of the paths
            string levelFilePath = _levelSettings.LevelFilePath;
            string textureFilePath = _levelSettings.MakeAbsolute(_levelSettings.TextureFilePath);
            string wadFilePath = _levelSettings.MakeAbsolute(_levelSettings.WadFilePath);
            string gameDirectory = _levelSettings.MakeAbsolute(_levelSettings.GameDirectory);
            string gameLevelFilePath = _levelSettings.MakeAbsolute(_levelSettings.GameLevelFilePath);
            string gameExecutableFilePath = _levelSettings.MakeAbsolute(_levelSettings.GameExecutableFilePath);

            levelFilePathTxt.BackColor = Directory.Exists(Utils.GetDirectoryNameTry(levelFilePath)) ? _correctColor : _wrongColor;
            textureFilePathTxt.BackColor = File.Exists(textureFilePath) ? _correctColor : _wrongColor;
            wadFilePathTxt.BackColor = File.Exists(wadFilePath) ? _correctColor : _wrongColor;
            gameDirectoryTxt.BackColor = Directory.Exists(gameDirectory) ? _correctColor : _wrongColor;
            gameLevelFilePathTxt.BackColor = Directory.Exists(Utils.GetDirectoryNameTry(gameLevelFilePath)) ? _correctColor : _wrongColor;
            gameExecutableFilePathTxt.BackColor = File.Exists(gameExecutableFilePath) ? _correctColor : _wrongColor;

            pathToolTip.SetToolTip(levelFilePathTxt, levelFilePath);
            pathToolTip.SetToolTip(textureFilePathTxt, textureFilePath);
            pathToolTip.SetToolTip(wadFilePathTxt, wadFilePath);
            pathToolTip.SetToolTip(gameDirectoryTxt, gameDirectory);
            pathToolTip.SetToolTip(gameLevelFilePathTxt, gameLevelFilePath);
            pathToolTip.SetToolTip(gameExecutableFilePathTxt, gameExecutableFilePath);
        
            // Load previews
            string fontPath = _levelSettings.FontTextureFileNameAbsoluteOrDefault;
            pathToolTip.SetToolTip(fontTextureFilePathTxt, fontPath);
            if (fontTextureFilePathPicPreviewCurrentPath != fontPath)
            {
                fontTextureFilePathPicPreviewCurrentPath = fontPath;
                try
                {
                    fontTextureFilePathPicPreview.Image?.Dispose();
                    fontTextureFilePathPicPreview.Image = ResourceLoader.LoadRawExtraTexture(fontPath).ToBitmap();
                    fontTextureFilePathPicPreview.BackgroundImage = Properties.Resources.TransparentBackground;
                    fontTextureFilePathPicPreview.Tag = null;
                    fontTextureFilePathTxt.BackColor = _correctColor;
                }
                catch (Exception exc)
                {
                    logger.Info(exc, "Unable to load font texture preview.");
                    fontTextureFilePathPicPreview.Image = null;
                    fontTextureFilePathPicPreview.BackgroundImage = null;
                    fontTextureFilePathPicPreview.Tag = exc;
                    fontTextureFilePathTxt.BackColor = _wrongColor;
                }
            }

            string skyPath = _levelSettings.SkyTextureFileNameAbsoluteOrDefault;
            pathToolTip.SetToolTip(skyTextureFilePathTxt, skyPath);
            if (skyTextureFilePathPicPreviewCurrentPath != skyPath)
            {
                skyTextureFilePathPicPreviewCurrentPath = skyPath;
                try
                {
                    skyTextureFilePathPicPreview.Image?.Dispose();
                    skyTextureFilePathPicPreview.Image = ResourceLoader.LoadRawExtraTexture(skyPath).ToBitmap();
                    skyTextureFilePathPicPreview.BackgroundImage = Properties.Resources.TransparentBackground;
                    skyTextureFilePathPicPreview.Tag = null;
                    skyTextureFilePathTxt.BackColor = _correctColor;
                }
                catch (Exception exc)
                {
                    logger.Info(exc, "Unable to load sky texture preview.");
                    skyTextureFilePathPicPreview.Image = null;
                    skyTextureFilePathPicPreview.BackgroundImage = null;
                    skyTextureFilePathPicPreview.Tag = exc;
                    skyTextureFilePathTxt.BackColor = _wrongColor;
                }
            }

            // Update path variables
            foreach (DataGridViewRow row in pathVariablesDataGridView.Rows)
            {
                string value = _levelSettings.ParseVariables(row.Cells[0].Value.ToString());
                if (row.Cells[1].Value.ToString() != value)
                    row.Cells[1].Value = value;
            }
        }

        private string GetLevelResourcePath(string file)
        {
            return LevelSettings.VariableCreate(VariableType.LevelDirectory) + LevelSettings.Dir + file;
        }

        private string BrowseFile(string currentPath, string title, string filter, VariableType baseDirType, bool save)
        {
            string path = _levelSettings.MakeAbsolute(currentPath);
            using (FileDialog dialog = save ? (FileDialog)new SaveFileDialog() : new OpenFileDialog())
            {
                dialog.Filter = filter + "|All files (*.*)|*";
                dialog.Title = title;
                dialog.FileName = string.IsNullOrEmpty(currentPath) ? "" : Path.GetFileName(path);
                dialog.InitialDirectory = string.IsNullOrEmpty(currentPath) ? path : Path.GetDirectoryName(path);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return null;
                return _levelSettings.MakeRelative(dialog.FileName, baseDirType);
            }
        }

        private string BrowseFolder(string currentPath, string description, VariableType baseDirType)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = description;
                dialog.SelectedPath = _levelSettings.MakeAbsolute(currentPath);
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                SendKeys.Send("{TAB}{TAB}{RIGHT}");  // Workaround https://stackoverflow.com/questions/6942150/why-folderbrowserdialog-dialog-does-not-scroll-to-selected-folder
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return null;
                return _levelSettings.MakeRelative(dialog.SelectedPath, baseDirType);
            }
        }

        // Level path
        private void levelFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (_levelSettings.LevelFilePath == levelFilePathTxt.Text)
                return;
            _levelSettings.LevelFilePath = levelFilePathTxt.Text;
            UpdateDialog();
        }

        private void levelFilePathBut_Click(object sender, EventArgs e)
        {
            string result = BrowseFile(_levelSettings.LevelFilePath, "Select the level name", "Tomb Editor Level (*.prj2)|*.prj2", VariableType.None, true);
            if (result != null)
            {
                _levelSettings.LevelFilePath = result;
                UpdateDialog();
            }
        }

        // Texture path
        private void textureFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (_levelSettings.TextureFilePath == textureFilePathTxt.Text)
                return;
            _levelSettings.TextureFilePath = textureFilePathTxt.Text;
            UpdateDialog();
        }

        private void textureFilePathBut_Click(object sender, EventArgs e)
        {
            string path = ResourceLoader.BrowseTextureFile(_levelSettings, _levelSettings.TextureFilePath, this);
            if (path != _levelSettings.TextureFilePath)
            {
                _levelSettings.TextureFilePath = path;
                UpdateDialog();
            }
        }

        // Object file (*.wad) path
        private void wadFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (_levelSettings.WadFilePath == wadFilePathTxt.Text)
                return;
            _levelSettings.WadFilePath = wadFilePathTxt.Text;
            UpdateDialog();

        }

        private void wadFilePathBut_Click(object sender, EventArgs e)
        {
            string path = ResourceLoader.BrowseObjectFile(_levelSettings, _levelSettings.WadFilePath, this);
            if (path != _levelSettings.WadFilePath)
            {
                _levelSettings.WadFilePath = path;
                UpdateDialog();
            }
        }

        // Font Texture
        private void fontTextureFilePathOptAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (fontTextureFilePathOptAuto.Checked == string.IsNullOrEmpty(_levelSettings.FontTextureFilePath))
                return;
            _levelSettings.FontTextureFilePath = fontTextureFilePathOptAuto.Checked ? null : GetLevelResourcePath("Font.pc");
            UpdateDialog();
        }

        private void fontTextureFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_levelSettings.FontTextureFilePath) || (_levelSettings.FontTextureFilePath == fontTextureFilePathTxt.Text))
                return;
            if (string.IsNullOrEmpty(fontTextureFilePathTxt.Text)) // Don't set if it would be empty otherwise.
                return;
            _levelSettings.FontTextureFilePath = fontTextureFilePathTxt.Text;
            UpdateDialog();
        }

        private void fontTextureFilePathBut_Click(object sender, EventArgs e)
        {
            string result = BrowseFile(_levelSettings.FontTextureFilePath, "Select a font texture", _specialTextureFilter, VariableType.LevelDirectory, false);
            if (result != null)
            {
                _levelSettings.FontTextureFilePath = result;
                UpdateDialog();
            }
        }

        // Sky texture
        private void skyTextureFilePathOptAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (skyTextureFilePathOptAuto.Checked == string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath))
                return;
            _levelSettings.SkyTextureFilePath = skyTextureFilePathOptAuto.Checked ? null : GetLevelResourcePath("pcsky.raw");
            UpdateDialog();
        }

        private void skyTextureFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath) || (_levelSettings.SkyTextureFilePath == skyTextureFilePathTxt.Text))
                return;
            if (string.IsNullOrEmpty(skyTextureFilePathTxt.Text)) // Don't set if it would be empty otherwise.
                return;
            _levelSettings.SkyTextureFilePath = skyTextureFilePathTxt.Text;
            UpdateDialog();
        }

        private void skyTextureFilePathBut_Click(object sender, EventArgs e)
        {
            string result = BrowseFile(_levelSettings.SkyTextureFilePath, "Select a sky texture", _specialTextureFilter, VariableType.LevelDirectory, false);
            if (result != null)
            {
                _levelSettings.SkyTextureFilePath = result;
                UpdateDialog();
            }
        }

        // Game directory
        private void gameDirectoryTxt_TextChanged(object sender, EventArgs e)
        {
            if (_levelSettings.GameDirectory == gameDirectoryTxt.Text)
                return;
            _levelSettings.GameDirectory = gameDirectoryTxt.Text;
            UpdateDialog();
        }

        private void GameDirectoryBut_Click(object sender, EventArgs e)
        {
            string result = BrowseFolder(_levelSettings.GameDirectory, "Select the game folder. (Should contain Tomb4.exe)", VariableType.LevelDirectory);
            if (result != null)
            {
                _levelSettings.GameDirectory = result;
                UpdateDialog();
            }
        }

        // Sound list
        private void soundsIgnoreMissingSounds_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.IgnoreMissingSounds = soundsIgnoreMissingSounds.Checked;
            UpdateDialog();
        }

        private SoundPath soundDataGridViewCreateNewRow()
        {
            string result = BrowseFolder(_levelSettings.LevelFilePath, "Select a new sound folder. (Should contain *.wav audio files)", VariableType.LevelDirectory);
            if (result != null)
                return new SoundPath(result);
            return null;
        }

        private void soundDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((e.RowIndex < 0) || (e.RowIndex >= soundDataGridViewDataSource.Count))
                return;

            if (e.ColumnIndex == 1)
            {
                SoundPath path = soundDataGridViewDataSource[e.RowIndex];
                if (!Directory.Exists(_levelSettings.MakeAbsolute(path.Path)))
                {
                    e.CellStyle.BackColor = _wrongColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_wrongColor, 0.4);
                }
            }
        }

        private void soundDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.RowIndex < 0) || (e.RowIndex >= soundDataGridViewDataSource.Count))
                return;

            if ((soundDataGridView.Columns[e.ColumnIndex] == soundDataGridViewColumnSearch))
            {
                string result = BrowseFolder(soundDataGridViewDataSource[e.RowIndex].Path, "Select the sound folder. (Should contain *.wav audio files)", VariableType.LevelDirectory);
                if (result != null)
                    soundDataGridViewDataSource[e.RowIndex] = new SoundPath(result);
            }
        }

        // Game level directory
        private void gameLevelFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (_levelSettings.GameLevelFilePath == gameLevelFilePathTxt.Text)
                return;
            _levelSettings.GameLevelFilePath = gameLevelFilePathTxt.Text;
            UpdateDialog();
        }

        private void gameLevelFilePathBut_Click(object sender, EventArgs e)
        {
            string result = BrowseFile(_levelSettings.GameLevelFilePath, "Select place for compiled level.", "Tomb Raider 4 Levels (*.tr4)|*.tr4", VariableType.GameDirectory, true);
            if (result != null)
            {
                _levelSettings.GameLevelFilePath = result;
                UpdateDialog();
            }
        }
        
        // Game executable
        private void gameExecutableFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (_levelSettings.GameExecutableFilePath == gameExecutableFilePathTxt.Text)
                return;
            _levelSettings.GameExecutableFilePath = gameExecutableFilePathTxt.Text;
            UpdateDialog();
        }

        private void gameExecutableFilePathBut_Click(object sender, EventArgs e)
        {
            string result = BrowseFile(_levelSettings.GameExecutableFilePath, "Select an executable", "Windows executables (*.exe)|*.exe", VariableType.GameDirectory, false);
            if (result != null)
            {
                _levelSettings.GameExecutableFilePath = result;
                UpdateDialog();
            }
        }
        
        private void gameExecutableSuppressAskingForOptionsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.GameExecutableSuppressAskingForOptions = gameExecutableSuppressAskingForOptionsCheckBox.Checked;
            UpdateDialog();
        }

        // Path variable list
        private void pathVariablesDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                pathVariablesDataGridView.ClearSelection();
                pathVariablesDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
            }
        }

        private void pathVariablesDataGridViewContextMenuCopy_Click(object sender, EventArgs e)
        {
            string text = (pathVariablesDataGridView.SelectedCells.Count == 0) ? "" : 
                pathVariablesDataGridView.SelectedCells[0].Value.ToString();
            System.Windows.Forms.Clipboard.SetText(text);
        }

        // Dialog buttons
        private void butApply_Click(object sender, EventArgs e)
        {
            _editor.UpdateLevelSettings(_levelSettings.Clone());
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            _editor.UpdateLevelSettings(_levelSettings.Clone());
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            Close();
        }




        // Target path
        // Target name
        // Target start
        // wad path
        // texture path
        // font.pc
        // pcsky.raw
        // Show real path
        // List possible replacement sequences
        // Sound paths
        // Exclude path finding default

        // Remove path finding break
        // Test build / start
    }
}
