using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Config;
using DarkUI.Forms;
using NLog;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using DarkUI.Controls;

namespace TombEditor.Forms
{
    public partial class FormLevelSettings : DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private class PictureTooltip : ToolTip
        {
            private readonly DarkForm parentForm;

            public PictureTooltip(DarkForm parent)
            {
                parentForm = parent;
                OwnerDraw = true;
                Popup += OnPopup;
                Draw += OnDraw;
                InitialDelay = 10;
                AutoPopDelay = short.MaxValue; // Unfortunately the maximum possible value
                ShowAlways = true;
            }

            private void OnPopup(object sender, PopupEventArgs e) // use this event to set the size of the tool tip
            {
                e.ToolTipSize = new Size(256, 256);
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
                    using (Brush backgroundBrush = new SolidBrush(Colors.GreyBackground))
                        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);

                // Draw image (if available)
                if (parent.Image != null)
                    e.Graphics.DrawImage(parent.Image, e.Bounds);

                e.Graphics.InterpolationMode = InterpolationMode.Bicubic;

                // Draw error message (if necessary)
                Exception exc = parent.Tag as Exception;
                if (exc != null)
                {
                    Rectangle textArea = e.Bounds;
                    textArea.Inflate(-5, -5);
                    e.Graphics.DrawString(" The image could not be loaded.\n\n Message: " + exc.Message,
                        parentForm.Font, Brushes.DarkGray, textArea,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }

                // Draw border
                e.Graphics.DrawRectangle(Pens.Black, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }
        }

        private class ReferencedWadWrapper
        {
            private readonly FormLevelSettings _parent;
            public ReferencedWad Wad;
            public ReferencedWadWrapper(FormLevelSettings parent, ReferencedWad wad)
            {
                _parent = parent;
                Wad = wad;
            }
            public string Path
            {
                get { return Wad.Path; }
                set
                {
                    Wad = new ReferencedWad(_parent._levelSettings, value);
                    for (int i = 0; i < _parent.objectFileDataGridView.ColumnCount; ++i)
                        _parent.objectFileDataGridView.InvalidateColumn(i);
                }
            }
            public string Message
            {
                get
                {
                    if (Wad.LoadException == null)
                        return "Successfully loaded";
                    return Wad.LoadException.Message + " (" + Wad.LoadException.GetType().Name + ")";
                }
            }
        }

        private readonly Color _correctColor;
        private readonly Color _wrongColor;
        private readonly Editor _editor;
        private readonly LevelSettings _levelSettings;
        private string fontTextureFilePathPicPreviewCurrentPath;
        private string skyTextureFilePathPicPreviewCurrentPath;
        private string tr5ExtraSpritesFilePathPicPreviewCurrentPath;
        private readonly PictureTooltip _pictureTooltip;
        private readonly BindingList<ReferencedWadWrapper> objectFileDataGridViewDataSource = new BindingList<ReferencedWadWrapper>();
        private readonly BindingList<OldWadSoundPath> soundDataGridViewDataSource = new BindingList<OldWadSoundPath>();
        private readonly Color _objectFileDataGridViewCorrectColor;
        private readonly Color _objectFileDataGridViewWrongColor;
        private FormWadPreview _wadPreview = null;

        public FormLevelSettings(Editor editor)
        {
            _editor = editor;
            _levelSettings = editor.Level.Settings.Clone();

            InitializeComponent();

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            //MinimumSize = new Size(678, 331) + (Size - ClientSize);
            //Size = MinimumSize;

            // Initialize object file data grid view
            foreach (var wad in _levelSettings.Wads)
                objectFileDataGridViewDataSource.Add(new ReferencedWadWrapper(this, wad)); // We don't need to clone because we don't modify the wad, we create new wads
            objectFileDataGridViewDataSource.ListChanged += delegate
            {
                _levelSettings.Wads.Clear();
                _levelSettings.Wads.AddRange(objectFileDataGridViewDataSource.Select(o => o.Wad));
            };
            objectFileDataGridView.DataSource = objectFileDataGridViewDataSource;
            objectFileDataGridViewControls.DataGridView = objectFileDataGridView;
            objectFileDataGridViewControls.CreateNewRow = objectFileDataGridViewCreateNewRow;
            objectFileDataGridViewControls.AllowUserDelete = true;
            objectFileDataGridViewControls.AllowUserMove = true;
            objectFileDataGridViewControls.AllowUserNew = true;
            objectFileDataGridViewControls.Enabled = true;
            _objectFileDataGridViewCorrectColor = objectFileDataGridView.BackColor.MixWith(Color.LimeGreen, 0.55);
            _objectFileDataGridViewWrongColor = objectFileDataGridView.BackColor.MixWith(Color.DarkRed, 0.55);

            // Initialize sound path data grid view
            foreach (var soundPath in _levelSettings.OldWadSoundPaths)
                soundDataGridViewDataSource.Add(soundPath.Clone());
            soundDataGridViewDataSource.ListChanged += delegate
                {
                    _levelSettings.OldWadSoundPaths.Clear();
                    foreach (var soundPath in soundDataGridViewDataSource)
                        _levelSettings.OldWadSoundPaths.Add(soundPath.Clone());
                };
            soundDataGridView.DataSource = soundDataGridViewDataSource;
            soundDataGridViewControls.DataGridView = soundDataGridView;
            soundDataGridViewControls.CreateNewRow = soundDataGridViewCreateNewRow;
            soundDataGridViewControls.AllowUserDelete = true;
            soundDataGridViewControls.AllowUserMove = true;
            soundDataGridViewControls.AllowUserNew = true;
            soundDataGridViewControls.Enabled = true;

            // Initialize picture previews
            _correctColor = gameLevelFilePathTxt.BackColor;
            _wrongColor = _correctColor.MixWith(Color.DarkRed, 0.55);
            skyTextureFilePathPicPreview.BackColor = _wrongColor;
            fontTextureFilePathPicPreview.BackColor = _wrongColor;
            tr5SpritesTextureFilePathPicPreview.BackColor = _wrongColor;

            _pictureTooltip = new PictureTooltip(this);
            _pictureTooltip.SetToolTip(fontTextureFilePathPicPreview, "Font Preview");
            _pictureTooltip.SetToolTip(skyTextureFilePathPicPreview, "Sky Preview");
            _pictureTooltip.SetToolTip(tr5SpritesTextureFilePathPicPreview, "TR5 extra sprites Preview");
            components.Add(_pictureTooltip);

            // Initialize imported geometry manager
            importedGeometryManager.LevelSettings = _levelSettings;

            // Populate variable list
            foreach (VariableType variableType in Enum.GetValues(typeof(VariableType)))
                pathVariablesDataGridView.Rows.Add(LevelSettings.VariableCreate(variableType), "");

            // Populate game version list
            //comboGameVersion.Items.AddRange(Enum.GetValues(typeof(GameVersion)).Cast<object>().ToArray());
            comboGameVersion.Items.Add(GameVersion.TR4);
            comboGameVersion.Items.Add(GameVersion.TRNG);
            comboGameVersion.Items.Add(GameVersion.TR5);

            // Populate TR5 lists
            comboTr5Weather.Items.AddRange(Enum.GetValues(typeof(Tr5WeatherType)).Cast<object>().ToArray());
            comboLaraType.Items.AddRange(Enum.GetValues(typeof(Tr5LaraType)).Cast<object>().ToArray());

            // Initialize options list
            optionsContainer.LinkedListView = optionsList;

            // Initialize controls
            UpdateDialog();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _wadPreview?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void UpdateDialog()
        {
            levelFilePathTxt.Text = _levelSettings.LevelFilePath;
            textureFilePathTxt.Text = _levelSettings.TextureFilePath;
            gameDirectoryTxt.Text = _levelSettings.GameDirectory;
            gameLevelFilePathTxt.Text = _levelSettings.GameLevelFilePath;
            gameExecutableFilePathTxt.Text = _levelSettings.GameExecutableFilePath;
            GameEnableQuickStartFeatureCheckBox.Checked = _levelSettings.GameEnableQuickStartFeature;
            comboGameVersion.Text = _levelSettings.GameVersion.ToString(); // Must also accept none enum values.
            tbScriptPath.Text = _levelSettings.ScriptDirectory;
            comboTr5Weather.Text = _levelSettings.Tr5WeatherType.ToString(); // Must also accept none enum values.
            comboLaraType.Text = _levelSettings.Tr5LaraType.ToString(); // Must also accept none enum values.

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

            tr5SpritesFilePathOptAuto.Checked = string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath);
            tr5SpritesFilePathOptCustom.Checked = !string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath);
            tr5SpritesTextureFilePathTxt.Enabled = !string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath);
            tr5SpritesTextureFilePathBut.Enabled = !string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath);
            if (string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath))
                tr5SpritesTextureFilePathTxt.Text = GetLevelResourcePath("Extra.Tr5.pc");
            else
                tr5SpritesTextureFilePathTxt.Text = _levelSettings.Tr5ExtraSpritesFilePath;

            // Check correctness of the paths
            string levelFilePath = _levelSettings.LevelFilePath;
            string textureFilePath = _levelSettings.MakeAbsolute(_levelSettings.TextureFilePath);
            string gameDirectory = _levelSettings.MakeAbsolute(_levelSettings.GameDirectory);
            string gameLevelFilePath = _levelSettings.MakeAbsolute(_levelSettings.GameLevelFilePath);
            string gameExecutableFilePath = _levelSettings.MakeAbsolute(_levelSettings.GameExecutableFilePath);

            levelFilePathTxt.BackColor = Directory.Exists(FileSystemUtils.GetDirectoryNameTry(levelFilePath)) ? _correctColor : _wrongColor;
            textureFilePathTxt.BackColor = File.Exists(textureFilePath) ? _correctColor : _wrongColor;
            gameDirectoryTxt.BackColor = Directory.Exists(gameDirectory) ? _correctColor : _wrongColor;
            gameLevelFilePathTxt.BackColor = Directory.Exists(FileSystemUtils.GetDirectoryNameTry(gameLevelFilePath)) ? _correctColor : _wrongColor;
            gameExecutableFilePathTxt.BackColor = File.Exists(gameExecutableFilePath) ? _correctColor : _wrongColor;

            pathToolTip.SetToolTip(levelFilePathTxt, levelFilePath);
            pathToolTip.SetToolTip(textureFilePathTxt, textureFilePath);
            pathToolTip.SetToolTip(gameDirectoryTxt, gameDirectory);
            pathToolTip.SetToolTip(gameLevelFilePathTxt, gameLevelFilePath);
            pathToolTip.SetToolTip(gameExecutableFilePathTxt, gameExecutableFilePath);

            // Load previews
            string fontPath = _levelSettings.MakeAbsolute(_levelSettings.FontTextureFilePath) ?? "<default>";
            pathToolTip.SetToolTip(fontTextureFilePathTxt, fontPath);
            if (fontTextureFilePathPicPreviewCurrentPath != fontPath)
            {
                fontTextureFilePathPicPreviewCurrentPath = fontPath;
                try
                {
                    fontTextureFilePathPicPreview.Image?.Dispose();
                    fontTextureFilePathPicPreview.Image = _levelSettings.LoadFontTexture().ToBitmap();
                    fontTextureFilePathPicPreview.BackgroundImage = Properties.Resources.misc_TransparentBackground;
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

            string skyPath = _levelSettings.MakeAbsolute(_levelSettings.SkyTextureFilePath) ?? "<default>";
            pathToolTip.SetToolTip(skyTextureFilePathTxt, skyPath);
            if (skyTextureFilePathPicPreviewCurrentPath != skyPath)
            {
                skyTextureFilePathPicPreviewCurrentPath = skyPath;
                try
                {
                    skyTextureFilePathPicPreview.Image?.Dispose();
                    skyTextureFilePathPicPreview.Image = _levelSettings.LoadSkyTexture().ToBitmap();
                    skyTextureFilePathPicPreview.BackgroundImage = Properties.Resources.misc_TransparentBackground;
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

            string tr5SpritesPath = _levelSettings.MakeAbsolute(_levelSettings.Tr5ExtraSpritesFilePath) ?? "<default>";
            pathToolTip.SetToolTip(tr5SpritesTextureFilePathTxt, tr5SpritesPath);
            if (tr5ExtraSpritesFilePathPicPreviewCurrentPath != tr5SpritesPath)
            {
                tr5ExtraSpritesFilePathPicPreviewCurrentPath = tr5SpritesPath;
                try
                {
                    tr5SpritesTextureFilePathPicPreview.Image?.Dispose();
                    tr5SpritesTextureFilePathPicPreview.Image = _levelSettings.LoadTr5ExtraSprites().ToBitmap();
                    tr5SpritesTextureFilePathPicPreview.BackgroundImage = Properties.Resources.misc_TransparentBackground;
                    tr5SpritesTextureFilePathPicPreview.Tag = null;
                    tr5SpritesTextureFilePathTxt.BackColor = _correctColor;
                }
                catch (Exception exc)
                {
                    logger.Info(exc, "Unable to load TR5 extra sprites preview.");
                    tr5SpritesTextureFilePathPicPreview.Image = null;
                    tr5SpritesTextureFilePathPicPreview.BackgroundImage = null;
                    tr5SpritesTextureFilePathPicPreview.Tag = exc;
                    tr5SpritesTextureFilePathTxt.BackColor = _wrongColor;
                }
            }

            // Update path variables
            foreach (DataGridViewRow row in pathVariablesDataGridView.Rows)
            {
                string value = _levelSettings.ParseVariables(row.Cells[0].Value.ToString());
                if (row.Cells[1].Value.ToString() != value)
                    row.Cells[1].Value = value;
            }

            // Update the default ambient light
            panelRoomAmbientLight.BackColor = (_levelSettings.DefaultAmbientLight * new Vector4(0.5f, 0.5f, 0.5f, 1.0f)).ToWinFormsColor();
        }

        private string GetLevelResourcePath(string file)
        {
            return LevelSettings.VariableCreate(VariableType.LevelDirectory) + LevelSettings.Dir + file;
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
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.LevelFilePath,
                "Select the level name", LevelSettings.FileFormatsLevel, true, null);
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
            string path = GraphicalDialogHandler.BrowseTextureFile(_levelSettings, _levelSettings.TextureFilePath, this);
            if (path != _levelSettings.TextureFilePath)
            {
                _levelSettings.TextureFilePath = path;
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
            if (string.IsNullOrEmpty(_levelSettings.FontTextureFilePath) || _levelSettings.FontTextureFilePath == fontTextureFilePathTxt.Text)
                return;
            if (string.IsNullOrEmpty(fontTextureFilePathTxt.Text)) // Don't set if it would be empty otherwise.
                return;
            _levelSettings.FontTextureFilePath = fontTextureFilePathTxt.Text;
            UpdateDialog();
        }

        private void fontTextureFilePathBut_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.FontTextureFilePath,
                "Select a font texture", LevelSettings.FileFormatsLoadRawExtraTexture, false, VariableType.LevelDirectory);
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
            if (string.IsNullOrEmpty(_levelSettings.SkyTextureFilePath) || _levelSettings.SkyTextureFilePath == skyTextureFilePathTxt.Text)
                return;
            if (string.IsNullOrEmpty(skyTextureFilePathTxt.Text)) // Don't set if it would be empty otherwise.
                return;
            _levelSettings.SkyTextureFilePath = skyTextureFilePathTxt.Text;
            UpdateDialog();
        }

        private void skyTextureFilePathBut_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.SkyTextureFilePath,
                "Select a sky texture", LevelSettings.FileFormatsLoadRawExtraTexture, false, VariableType.LevelDirectory);
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
            string result = LevelFileDialog.BrowseFolder(this, _levelSettings, _levelSettings.GameDirectory,
                "Select the game folder (should contain Tomb3.exe/Tomb4.exe/Tomb5.exe/...)", VariableType.LevelDirectory);
            if (result != null)
            {
                _levelSettings.GameDirectory = result;
                UpdateDialog();
            }
        }

        // Sound list
        private OldWadSoundPath soundDataGridViewCreateNewRow()
        {
            string result = LevelFileDialog.BrowseFolder(this, _levelSettings, _levelSettings.LevelFilePath,
                "Select a new sound folder (should contain *.wav audio files)", VariableType.LevelDirectory);
            if (result != null)
                return new OldWadSoundPath(result);
            return null;
        }

        private void soundDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= soundDataGridViewDataSource.Count)
                return;

            if (soundDataGridView.Columns[e.ColumnIndex].Name == soundDataGridViewColumnPath.Name)
            {
                OldWadSoundPath path = soundDataGridViewDataSource[e.RowIndex];
                string parsedPath = _levelSettings.ParseVariables(path.Path);
                string absolutePath = _levelSettings.MakeAbsolute(path.Path);
                if (Path.IsPathRooted(parsedPath) && !Directory.Exists(absolutePath))
                {
                    e.CellStyle.BackColor = _wrongColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_wrongColor, 0.4);
                }
                soundDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = absolutePath;
            }
        }

        private void soundDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= soundDataGridViewDataSource.Count)
                return;

            if (soundDataGridView.Columns[e.ColumnIndex].Name == soundDataGridViewColumnSearch.Name)
            {
                string result = LevelFileDialog.BrowseFolder(this, _levelSettings, soundDataGridViewDataSource[e.RowIndex].Path,
                    "Select the sound folder (should contain *.wav audio files)", VariableType.LevelDirectory);
                if (result != null)
                    soundDataGridViewDataSource[e.RowIndex] = new OldWadSoundPath(result);
            }
        }

        // Object list
        private ReferencedWadWrapper objectFileDataGridViewCreateNewRow()
        {
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.LevelFilePath,
                "Select a new object file", ReferencedWad.FileExtensions, false, VariableType.LevelDirectory);
            if (result != null)
                return new ReferencedWadWrapper(this, new ReferencedWad(_levelSettings, result, new GraphicalDialogHandler(this)));
            return null;
        }

        private void objectFileDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= objectFileDataGridViewDataSource.Count)
                return;

            if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewMessageColumn.Name)
            {
                ReferencedWad wad = objectFileDataGridViewDataSource[e.RowIndex].Wad;
                if (wad.LoadException == null)
                {
                    e.CellStyle.BackColor = _objectFileDataGridViewCorrectColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_objectFileDataGridViewCorrectColor, 0.4);
                }
                else
                {
                    e.CellStyle.BackColor = _objectFileDataGridViewWrongColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_objectFileDataGridViewWrongColor, 0.4);
                }
            }
            else if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewPathColumn.Name)
            {
                ReferencedWad wad = objectFileDataGridViewDataSource[e.RowIndex].Wad;
                string absolutePath = _levelSettings.MakeAbsolute(wad.Path);
                objectFileDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = absolutePath;
            }
            else if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewShowContentColumn.Name)
            {
                ReferencedWad wad = objectFileDataGridViewDataSource[e.RowIndex].Wad;
                var cell = objectFileDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                var button = (DarkDataGridViewButtonCell)cell;
                ((DarkDataGridViewButtonCell)cell).Enabled = wad.LoadException == null;
            }
        }

        private void objectFileDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= objectFileDataGridViewDataSource.Count)
                return;

            if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewSearchColumn.Name)
            {
                string result = LevelFileDialog.BrowseFile(this, _levelSettings, objectFileDataGridViewDataSource[e.RowIndex].Path,
                    "Select a new object file", ReferencedWad.FileExtensions, false, VariableType.LevelDirectory);
                if (result != null)
                    objectFileDataGridViewDataSource[e.RowIndex] = new ReferencedWadWrapper(this, new ReferencedWad(_levelSettings, result, new GraphicalDialogHandler(this)));
            }
            else if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewShowContentColumn.Name)
            {
                ReferencedWad wad = objectFileDataGridViewDataSource[e.RowIndex].Wad;
                if (wad.LoadException != null)
                    return;

                // Open preview
                _wadPreview?.Dispose();
                _wadPreview = new FormWadPreview(wad.Wad, TombLib.Graphics.DeviceManager.DefaultDeviceManager, _editor);

                // Set screen position
                const int WindowBorderMargin = 5;
                const int RightMargin = 5;
                Rectangle screenArea = objectFileDataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                screenArea = new Rectangle(objectFileDataGridView.PointToScreen(screenArea.Location), screenArea.Size);
                Point pos = screenArea.Location + new Size(0, screenArea.Height / 2);
                pos -= new Size(_wadPreview.Width + RightMargin, _wadPreview.Height / 2);
                Rectangle parentWindowBounds = Bounds;
                pos.Y = Math.Max(pos.Y, parentWindowBounds.Top + WindowBorderMargin);
                pos.Y = Math.Min(pos.Y, parentWindowBounds.Bottom - _wadPreview.Height - WindowBorderMargin);
                _wadPreview.Location = pos;

                _wadPreview.Show(this);
            }
        }

        // Game version
        private void comboGameVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            var gameVersion = (GameVersion)Enum.Parse(typeof(GameVersion), comboGameVersion.Text);
            if (_levelSettings.GameVersion == gameVersion)
                return;
            _levelSettings.GameVersion = gameVersion; // Must also check none enum values
            UpdateDialog();
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
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.GameLevelFilePath,
                "Select place for compiled level", LevelSettings.FileFormatsLevelCompiled, true, VariableType.GameDirectory);
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
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.GameExecutableFilePath,
                "Select an executable", new[] { new FileFormat("Windows executables", "exe") }, false, VariableType.GameDirectory);
            if (result != null)
            {
                _levelSettings.GameExecutableFilePath = result;
                UpdateDialog();
            }
        }

        private void GameEnableQuickStartFeatureCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _levelSettings.GameEnableQuickStartFeature = GameEnableQuickStartFeatureCheckBox.Checked;
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
            string text = pathVariablesDataGridView.SelectedCells.Count == 0 ? "" :
                pathVariablesDataGridView.SelectedCells[0].Value.ToString();
            Clipboard.SetText(text);
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

        private void scriptPathBut_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFolder(this, _levelSettings, _levelSettings.ScriptDirectory,
                "Select the folder of TXT sources for script", VariableType.ScriptDirectory);
            if (result != null)
            {
                _levelSettings.ScriptDirectory = result;
                UpdateDialog();
            }
        }

        private void panelRoomAmbientLight_Click(object sender, EventArgs e)
        {
            colorDialog.Color = (_levelSettings.DefaultAmbientLight * 0.5f).ToWinFormsColor();
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            _levelSettings.DefaultAmbientLight = colorDialog.Color.ToFloatColor() * 2.0f;
            UpdateDialog();
        }

        private void tr5SpritesFilePathOptAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (tr5SpritesFilePathOptAuto.Checked == string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath))
                return;
            _levelSettings.Tr5ExtraSpritesFilePath = tr5SpritesFilePathOptAuto.Checked ? null : GetLevelResourcePath("Extra.Tr5.pc");
            UpdateDialog();
        }

        private void tr5SpritesTextureFilePathTxt_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_levelSettings.Tr5ExtraSpritesFilePath) || _levelSettings.Tr5ExtraSpritesFilePath == tr5SpritesTextureFilePathTxt.Text)
                return;
            if (string.IsNullOrEmpty(tr5SpritesTextureFilePathTxt.Text)) // Don't set if it would be empty otherwise.
                return;
            _levelSettings.Tr5ExtraSpritesFilePath = tr5SpritesTextureFilePathTxt.Text;
            UpdateDialog();
        }

        private void tr5SpritesTextureFilePathBut_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, _levelSettings, _levelSettings.Tr5ExtraSpritesFilePath,
                "Select a font texture", LevelSettings.FileFormatsLoadRawExtraTexture, false, VariableType.LevelDirectory);
            if (result != null)
            {
                _levelSettings.Tr5ExtraSpritesFilePath = result;
                UpdateDialog();
            }
        }

        private void comboLaraType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var laraType = (Tr5LaraType)Enum.Parse(typeof(Tr5LaraType), comboLaraType.Text);
            if (_levelSettings.Tr5LaraType == laraType)
                return;
            _levelSettings.Tr5LaraType = laraType; // Must also check none enum values
            UpdateDialog();
        }

        private void comboTr5Weather_SelectedIndexChanged(object sender, EventArgs e)
        {
            var weather = (Tr5WeatherType)Enum.Parse(typeof(Tr5WeatherType), comboTr5Weather.Text);
            if (_levelSettings.Tr5WeatherType == weather)
                return;
            _levelSettings.Tr5WeatherType = weather; // Must also check none enum values
            UpdateDialog();
        }
    }
}
