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
using System.Collections.Generic;
using System.Threading.Tasks;

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
                    _parent.objectFileDataGridView.InvalidateRow(_parent._objectFileDataGridViewDataSource.IndexOf(this));
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

        private class ReferencedTextureWrapper
        {
            private readonly FormLevelSettings _parent;
            public LevelTexture Texture;

            public ReferencedTextureWrapper(FormLevelSettings parent, LevelTexture texture)
            {
                _parent = parent;
                Texture = texture;
            }

            public string Path
            {
                get { return Texture.Path; }
                set
                {
                    Texture = (LevelTexture)Texture.Clone(); // We don't actually clone the image data array, so it's fine.
                    Texture.SetPath(_parent._levelSettings, value);
                    _parent._texturePreviewCache.RemoveAll(obj => obj.LevelTexture == Texture);
                    _parent.textureFileDataGridView.InvalidateRow(_parent._textureFileDataGridViewDataSource.IndexOf(this));
                }
            }

            public string Message
            {
                get
                {
                    if (Texture.LoadException == null)
                        return "Successfully loaded";
                    return Texture.LoadException.Message + " (" + Texture.LoadException.GetType().Name + ")";
                }
            }

            public string Size
            {
                get
                {
                    if (Texture.LoadException != null || Texture.Image == TombLib.Utils.Texture.UnloadedPlaceholder)
                        return "-";
                    return Texture.Image.Width + " × " + Texture.Image.Height;
                }
            }

            public bool ReplaceMagentaWithTransparency
            {
                get { return Texture.ReplaceMagentaWithTransparency; }
                set
                {
                    Texture = (LevelTexture)Texture.Clone(); // We don't actually clone the image data array, so it's fine.
                    Texture.SetReplaceMagentaWithTransparency(_parent._levelSettings, value);
                    _parent._texturePreviewCache.RemoveAll(obj => obj.LevelTexture == Texture);
                    _parent.textureFileDataGridView.InvalidateRow(_parent._textureFileDataGridViewDataSource.IndexOf(this));
                }
            }

            public bool Convert512PixelsToDoubleRows
            {
                get { return Texture.Convert512PixelsToDoubleRows; }
                set
                {
                    Texture = (LevelTexture)Texture.Clone(); // We don't actually clone the image data array, so it's fine.
                    Texture.SetConvert512PixelsToDoubleRows(_parent._levelSettings, value);
                    _parent._texturePreviewCache.RemoveAll(obj => obj.LevelTexture == Texture);
                    _parent.textureFileDataGridView.InvalidateRow(_parent._textureFileDataGridViewDataSource.IndexOf(this));
                }
            }
        }
        private struct TextureCachePreviewKey : IEquatable<TextureCachePreviewKey>
        {
            public Size RequiredSize { get; }
            public LevelTexture LevelTexture { get { LevelTexture result = null; _levelTexture?.TryGetTarget(out result); return result; } }
            private int _hashCode;
            private WeakReference<LevelTexture> _levelTexture; // Use a weak reference to avoid keeping big images alive.
            public TextureCachePreviewKey(Size size, LevelTexture levelTexture)
            {
                RequiredSize = size;
                _levelTexture = new WeakReference<LevelTexture>(levelTexture);
                _hashCode = RequiredSize.GetHashCode() + 866557253 * (levelTexture?.GetHashCode() ?? 0);
            }
            public bool Equals(TextureCachePreviewKey other) => RequiredSize == other.RequiredSize && (LevelTexture == null ? other.LevelTexture == null : LevelTexture.Equals(other.LevelTexture));
            public override bool Equals(object other) => other is TextureCachePreviewKey && Equals((TextureCachePreviewKey)other);
            public override int GetHashCode() => _hashCode;
        }

        private readonly Color _correctColor;
        private readonly Color _wrongColor;
        private readonly Color _columnMessageCorrectColor;
        private readonly Color _columnMessageWrongColor;
        private readonly Editor _editor;
        private readonly LevelSettings _levelSettings;

        private string fontTextureFilePathPicPreviewCurrentPath;
        private string skyTextureFilePathPicPreviewCurrentPath;
        private string tr5ExtraSpritesFilePathPicPreviewCurrentPath;
        private readonly PictureTooltip _pictureTooltip;
        private readonly BindingList<ReferencedWadWrapper> _objectFileDataGridViewDataSource = new BindingList<ReferencedWadWrapper>();
        private readonly BindingList<ReferencedTextureWrapper> _textureFileDataGridViewDataSource = new BindingList<ReferencedTextureWrapper>();
        private readonly BindingList<OldWadSoundPath> _soundDataGridViewDataSource = new BindingList<OldWadSoundPath>();
        private readonly Cache<TextureCachePreviewKey, Bitmap> _texturePreviewCache;
        private FormPreviewWad _previewWad = null;
        private FormPreviewTexture _previewTexture = null;

        public FormLevelSettings(Editor editor)
        {
            _editor = editor;
            _levelSettings = editor.Level.Settings.Clone();

            InitializeComponent();

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MinimumSize = new Size(793, 533) + (Size - ClientSize);
            Size = MinimumSize;

            // Remember colors
            _correctColor = gameLevelFilePathTxt.BackColor;
            _wrongColor = _correctColor.MixWith(Color.DarkRed, 0.55);
            _columnMessageCorrectColor = objectFileDataGridView.BackColor.MixWith(Color.LimeGreen, 0.55);
            _columnMessageWrongColor = objectFileDataGridView.BackColor.MixWith(Color.DarkRed, 0.55);

            // Initialize texture file data grid view
            foreach (var texture in _levelSettings.Textures)
                _textureFileDataGridViewDataSource.Add(new ReferencedTextureWrapper(this, texture)); // We don't need to clone because we don't modify the wad, we create new wads
            _textureFileDataGridViewDataSource.ListChanged += delegate
            {
                _levelSettings.Textures.Clear();
                _levelSettings.Textures.AddRange(_textureFileDataGridViewDataSource.Select(o => o.Texture));
            };
            textureFileDataGridView.DataSource = _textureFileDataGridViewDataSource;
            textureFileDataGridViewControls.DataGridView = textureFileDataGridView;
            textureFileDataGridViewControls.CreateNewRow = textureFileDataGridViewCreateNewRow;
            textureFileDataGridViewControls.AllowUserDelete = true;
            textureFileDataGridViewControls.AllowUserMove = true;
            textureFileDataGridViewControls.AllowUserNew = true;
            textureFileDataGridViewControls.Enabled = true;
            _texturePreviewCache = new Cache<TextureCachePreviewKey, Bitmap>(1024, CreateTexturePreview);

            // Initialize object file data grid view
            foreach (var wad in _levelSettings.Wads)
                _objectFileDataGridViewDataSource.Add(new ReferencedWadWrapper(this, wad)); // We don't need to clone because we don't modify the wad, we create new wads
            _objectFileDataGridViewDataSource.ListChanged += delegate
            {
                _levelSettings.Wads.Clear();
                _levelSettings.Wads.AddRange(_objectFileDataGridViewDataSource.Select(o => o.Wad));
            };
            objectFileDataGridView.DataSource = _objectFileDataGridViewDataSource;
            objectFileDataGridViewControls.DataGridView = objectFileDataGridView;
            objectFileDataGridViewControls.CreateNewRow = objectFileDataGridViewCreateNewRow;
            objectFileDataGridViewControls.AllowUserDelete = true;
            objectFileDataGridViewControls.AllowUserMove = true;
            objectFileDataGridViewControls.AllowUserNew = true;
            objectFileDataGridViewControls.Enabled = true;

            // Initialize sound path data grid view
            foreach (var soundPath in _levelSettings.OldWadSoundPaths)
                _soundDataGridViewDataSource.Add(soundPath.Clone());
            _soundDataGridViewDataSource.ListChanged += delegate
                {
                    _levelSettings.OldWadSoundPaths.Clear();
                    foreach (var soundPath in _soundDataGridViewDataSource)
                        _levelSettings.OldWadSoundPaths.Add(soundPath.Clone());
                };
            soundDataGridView.DataSource = _soundDataGridViewDataSource;
            soundDataGridViewControls.DataGridView = soundDataGridView;
            soundDataGridViewControls.CreateNewRow = soundDataGridViewCreateNewRow;
            soundDataGridViewControls.AllowUserDelete = true;
            soundDataGridViewControls.AllowUserMove = true;
            soundDataGridViewControls.AllowUserNew = true;
            soundDataGridViewControls.Enabled = true;

            // Initialize picture previews
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
            tabbedContainer.LinkedListView = optionsList;

            // Initialize controls
            UpdateDialog();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _previewWad?.Dispose();
                _previewTexture?.Dispose();
                _texturePreviewCache?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void UpdateDialog()
        {
            levelFilePathTxt.Text = _levelSettings.LevelFilePath;
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
            string gameDirectory = _levelSettings.MakeAbsolute(_levelSettings.GameDirectory);
            string gameLevelFilePath = _levelSettings.MakeAbsolute(_levelSettings.GameLevelFilePath);
            string gameExecutableFilePath = _levelSettings.MakeAbsolute(_levelSettings.GameExecutableFilePath);

            levelFilePathTxt.BackColor = Directory.Exists(FileSystemUtils.GetDirectoryNameTry(levelFilePath)) ? _correctColor : _wrongColor;
            gameDirectoryTxt.BackColor = Directory.Exists(gameDirectory) ? _correctColor : _wrongColor;
            gameLevelFilePathTxt.BackColor = Directory.Exists(FileSystemUtils.GetDirectoryNameTry(gameLevelFilePath)) ? _correctColor : _wrongColor;
            gameExecutableFilePathTxt.BackColor = File.Exists(gameExecutableFilePath) ? _correctColor : _wrongColor;

            pathToolTip.SetToolTip(levelFilePathTxt, levelFilePath);
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
            panelRoomAmbientLight.BackColor = (_levelSettings.DefaultAmbientLight * new Vector3(0.5f)).ToWinFormsColor();

            // Hide version-specific controls

            bool currentVersionToCheck = (_levelSettings.GameVersion == GameVersion.TRNG);
            lblGameEnableQuickStartFeature2.Visible = currentVersionToCheck;

            currentVersionToCheck = (_levelSettings.GameVersion == GameVersion.TR5);
            GameEnableQuickStartFeatureCheckBox.Visible = !currentVersionToCheck;
            lblGameEnableQuickStartFeature1.Visible = !currentVersionToCheck;
            lblLaraType.Visible = currentVersionToCheck;
            comboLaraType.Visible = currentVersionToCheck;
            lblTr5Weather.Visible = currentVersionToCheck;
            comboTr5Weather.Visible = currentVersionToCheck;
            panelTr5Sprites.Visible = currentVersionToCheck;


        }

        private void FitPreview(Control form, Rectangle screenArea)
        {
            const int WindowBorderMargin = 5;
            const int RightMargin = 5;
            Point pos = screenArea.Location + new Size(0, screenArea.Height / 2);
            pos -= new Size(form.Width + RightMargin, form.Height / 2);
            Rectangle parentWindowBounds = Bounds;
            pos.Y = Math.Max(pos.Y, parentWindowBounds.Top + WindowBorderMargin);
            pos.Y = Math.Min(pos.Y, parentWindowBounds.Bottom - form.Height - WindowBorderMargin);
            form.Location = pos;
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
                "Select the level name", LevelSettings.FileFormatsLevel, null, true);
            if (result != null)
            {
                _levelSettings.LevelFilePath = result;
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
                "Select a font texture", LevelSettings.FileFormatsLoadRawExtraTexture, VariableType.LevelDirectory, false);
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
                "Select a sky texture", LevelSettings.FileFormatsLoadRawExtraTexture, VariableType.LevelDirectory, false);
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
            if (e.RowIndex < 0 || e.RowIndex >= _soundDataGridViewDataSource.Count)
                return;

            if (soundDataGridView.Columns[e.ColumnIndex].Name == soundDataGridViewColumnPath.Name)
            {
                OldWadSoundPath path = _soundDataGridViewDataSource[e.RowIndex];
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
            if (e.RowIndex < 0 || e.RowIndex >= _soundDataGridViewDataSource.Count)
                return;

            if (soundDataGridView.Columns[e.ColumnIndex].Name == soundDataGridViewColumnSearch.Name)
            {
                string result = LevelFileDialog.BrowseFolder(this, _levelSettings, _soundDataGridViewDataSource[e.RowIndex].Path,
                    "Select the sound folder (should contain *.wav audio files)", VariableType.LevelDirectory);
                if (result != null)
                    _soundDataGridViewDataSource[e.RowIndex] = new OldWadSoundPath(result);
            }
        }

        // Texture list
        private Bitmap CreateTexturePreview(TextureCachePreviewKey data)
        {
            // Create image
            Bitmap result = new Bitmap(data.RequiredSize.Width, data.RequiredSize.Height);
            try
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    using (TextureBrush brush = new TextureBrush(Properties.Resources.misc_TransparentBackground))
                        g.FillRectangle(brush, new Rectangle(new Point(), result.Size));
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    if (data.LevelTexture != null)
                        data.LevelTexture.Image.GetTempSystemDrawingBitmap(temp => g.DrawImage(temp,
                            new Rectangle(new Point(), result.Size),
                            new Rectangle(new Point(), temp.Size), GraphicsUnit.Pixel));
                }
                return result;
            }
            catch (Exception)
            {
                result.Dispose();
                throw;
            }
        }

        private IEnumerable<ReferencedTextureWrapper> textureFileDataGridViewCreateNewRow()
        {
            List<string> paths = LevelFileDialog.BrowseFiles(this, _levelSettings, _levelSettings.LevelFilePath,
                "Select new texture files", LevelTexture.FileExtensions, VariableType.LevelDirectory).ToList();

            // Load textures concurrently
            ReferencedTextureWrapper[] results = new ReferencedTextureWrapper[paths.Count];
            Parallel.For(0, paths.Count, i => results[i] = new ReferencedTextureWrapper(this, new LevelTexture(_levelSettings, paths[i])));
            return results;
        }

        private void textureFileDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _textureFileDataGridViewDataSource.Count)
                return;
            LevelTexture texture = _textureFileDataGridViewDataSource[e.RowIndex].Texture;

            if (textureFileDataGridView.Columns[e.ColumnIndex].Name == textureFileDataGridViewMessageColumn.Name)
            {
                if (texture.LoadException == null)
                {
                    e.CellStyle.BackColor = _columnMessageCorrectColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_columnMessageCorrectColor, 0.4);
                }
                else
                {
                    e.CellStyle.BackColor = _columnMessageWrongColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_columnMessageWrongColor, 0.4);
                }
            }
            else if (textureFileDataGridView.Columns[e.ColumnIndex].Name == textureFileDataGridViewPathColumn.Name)
            {
                string absolutePath = _levelSettings.MakeAbsolute(texture.Path);
                textureFileDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = absolutePath;
            }
            else if (textureFileDataGridView.Columns[e.ColumnIndex].Name == textureFileDataGridViewPreviewColumn.Name)
            {
                var cell = (DataGridViewImageCell)(textureFileDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex]);
                Size availableSpace = new Size(textureFileDataGridView.Columns[e.ColumnIndex].Width - 1, textureFileDataGridView.Rows[e.RowIndex].Height - 1);

                // Figure out required size of preview image
                float neededAspectRatio = (float)availableSpace.Width / availableSpace.Height;
                float givenAspectRatio = (float)texture.Image.Width / texture.Image.Height;
                float aspectRatioAdjust = neededAspectRatio / givenAspectRatio;
                Vector2 factor = Vector2.Min(new Vector2(1.0f / aspectRatioAdjust, aspectRatioAdjust), new Vector2(1.0f));
                Size previewImageSize = new Size((int)Math.Ceiling(availableSpace.Width * factor.X), (int)Math.Ceiling(availableSpace.Height * factor.Y));

                // Request and asign image
                e.Value = _texturePreviewCache[new TextureCachePreviewKey(previewImageSize, texture)];
            }
            else if (textureFileDataGridView.Columns[e.ColumnIndex].Name == textureFileDataGridViewShowColumn.Name)
            {
                var cell = textureFileDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                ((DarkDataGridViewButtonCell)cell).Enabled = texture.LoadException == null;
            }
        }

        private void textureFileDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _textureFileDataGridViewDataSource.Count)
                return;
            LevelTexture texture = _textureFileDataGridViewDataSource[e.RowIndex].Texture;

            if (textureFileDataGridView.Columns[e.ColumnIndex].Name == textureFileDataGridViewSearchColumn.Name)
            {
                string result = LevelFileDialog.BrowseFile(this, _levelSettings, _textureFileDataGridViewDataSource[e.RowIndex].Path,
                    "Select a new texture file", LevelTexture.FileExtensions, VariableType.LevelDirectory, false);
                if (result != null)
                    _textureFileDataGridViewDataSource[e.RowIndex].Path = result;
            }
            else if (textureFileDataGridView.Columns[e.ColumnIndex].Name == textureFileDataGridViewShowColumn.Name)
            {
                if (texture.LoadException != null)
                    return;

                // Open preview
                _previewTexture?.Dispose();
                _previewTexture = new FormPreviewTexture(texture, _editor);
                var screenArea = textureFileDataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                FitPreview(_previewTexture, new Rectangle(textureFileDataGridView.PointToScreen(screenArea.Location), screenArea.Size));
                _previewTexture.Show(this);
            }
            else if (textureFileDataGridView.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
                textureFileDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        // Object list
        private IEnumerable<ReferencedWadWrapper> objectFileDataGridViewCreateNewRow()
        {
            List<string> paths = LevelFileDialog.BrowseFiles(this, _levelSettings, _levelSettings.LevelFilePath,
                "Select new object files", ReferencedWad.FileExtensions, VariableType.LevelDirectory).ToList();

            // Load objects concurrently
            ReferencedWadWrapper[] results = new ReferencedWadWrapper[paths.Count];
            var synchronizedDialog = new GraphicalDialogHandler(this);
            Parallel.For(0, paths.Count, i => results[i] = new ReferencedWadWrapper(this, new ReferencedWad(_levelSettings, paths[i], synchronizedDialog)));
            return results;
        }

        private void objectFileDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _objectFileDataGridViewDataSource.Count)
                return;
            ReferencedWad wad = _objectFileDataGridViewDataSource[e.RowIndex].Wad;

            if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewMessageColumn.Name)
            {
                if (wad.LoadException == null)
                {
                    e.CellStyle.BackColor = _columnMessageCorrectColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_columnMessageCorrectColor, 0.4);
                }
                else
                {
                    e.CellStyle.BackColor = _columnMessageWrongColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_columnMessageWrongColor, 0.4);
                }
            }
            else if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewPathColumn.Name)
            {
                string absolutePath = _levelSettings.MakeAbsolute(wad.Path);
                objectFileDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = absolutePath;
            }
            else if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewShowColumn.Name)
            {
                var cell = objectFileDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                ((DarkDataGridViewButtonCell)cell).Enabled = wad.LoadException == null;
            }
        }

        private void objectFileDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _objectFileDataGridViewDataSource.Count)
                return;
            ReferencedWad wad = _objectFileDataGridViewDataSource[e.RowIndex].Wad;

            if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewSearchColumn.Name)
            {
                string result = LevelFileDialog.BrowseFile(this, _levelSettings, _objectFileDataGridViewDataSource[e.RowIndex].Path,
                    "Select a new object file", ReferencedWad.FileExtensions, VariableType.LevelDirectory, false);
                if (result != null)
                    _objectFileDataGridViewDataSource[e.RowIndex].Path = result;
            }
            else if (objectFileDataGridView.Columns[e.ColumnIndex].Name == objectFileDataGridViewShowColumn.Name)
            {
                if (wad.LoadException != null)
                    return;

                // Open preview
                _previewWad?.Dispose();
                _previewWad = new FormPreviewWad(wad.Wad, TombLib.Graphics.DeviceManager.DefaultDeviceManager.Device, _editor);
                var screenArea = objectFileDataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                FitPreview(_previewWad, new Rectangle(objectFileDataGridView.PointToScreen(screenArea.Location), screenArea.Size));
                _previewWad.Show(this);
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
                "Select place for compiled level", LevelSettings.FileFormatsLevelCompiled, VariableType.GameDirectory, true);
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
                "Select an executable", new[] { new FileFormat("Windows executables", "exe") }, VariableType.GameDirectory, false);
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
                "Select a font texture", LevelSettings.FileFormatsLoadRawExtraTexture, VariableType.LevelDirectory, false);
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
