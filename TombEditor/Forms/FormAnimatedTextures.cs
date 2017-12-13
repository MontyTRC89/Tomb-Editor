using DarkUI.Controls;
using DarkUI.Extensions;
using DarkUI.Forms;
using NLog;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombEditor.Geometry;
using TombLib.Utils;
using RectangleF = System.Drawing.RectangleF;

namespace TombEditor
{
    public partial class FormAnimatedTextures : DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private class InitEvent : IEditorEvent { }
        private class TransparentBindingList<T> : BindingList<T>
        {
            public TransparentBindingList(IList<T> list) : base(list) { }
            public new IList<T> Items => base.Items;
        }
        private struct CachedImageInfo
        {
            public ImageC _image;
            public Vector2 _sourceTexCoord0;
            public Vector2 _sourceTexCoord1;
            public Vector2 _sourceTexCoord2;
            public Vector2 _sourceTexCoord3;
            public Size _destinationSize;
        };
        private struct NgAnimatedTextureSettingPair
        {
            public int Key;
            public string Value;

            public NgAnimatedTextureSettingPair(int key,string value)
            {
                Key = key;
                Value = value;
            }

            public override string ToString()
            {
                return Value;
            }
        }

        private Editor _editor;
        private Cache<CachedImageInfo, Bitmap> _imageCache;

        private Timer _previewTimer = new Timer();
        private AnimatedTextureFrame _previewCurrentFrame = null;
        private int _previewCurrentRepeatTimes = 0;
        private const float _previewFps = 15;
        private const float _maxLegacyFrames = 16;
        private int _lastY = 0;

        private bool _isNg;

        public FormAnimatedTextures(Editor editor)
        {
            InitializeComponent();
            _previewTimer.Tick += _previewTimer_Tick;
            previewImage.Paint += _onPicturePreviewPaint;
            _editor = editor;
            _editor.EditorEventRaised += _editor_EditorEventRaised;

            // Setup image cache
            _imageCache = new Cache<CachedImageInfo, Bitmap>(512, (subsection) =>
                {
                    return GetPerspectivePreview(subsection._image, subsection._sourceTexCoord0, subsection._sourceTexCoord1, subsection._sourceTexCoord2,
                        subsection._sourceTexCoord3, subsection._destinationSize).ToBitmap();
                });

            // Setup data grid view
            texturesDataGridViewControls.DataGridView = texturesDataGridView;
            texturesDataGridViewControls.CreateNewRow = GetSelectedAnimatedTextureFrame;
            texturesDataGridViewColumnTexture.DataSource = new BindingList<LevelTexture>(editor.Level.Settings.Textures);

            // NG settings
            _isNg = _editor.Level.Settings.GameVersion == GameVersion.TRNG;
            if (_isNg)
            {
                // Fill effect combobox
                foreach (var animationType in Enum.GetValues(typeof(AnimatedTextureAnimationType)))
                    comboEffect.Items.Add(animationType);

                // Fill uv rotate combobox
                for (var i = -64; i < 0; i++)
                    comboUvRotate.Items.Add(new NgAnimatedTextureSettingPair(i, "UvRotate = " + i));
                comboUvRotate.Items.Add(new NgAnimatedTextureSettingPair(0, "Default (from script)"));
                for (var i = 1; i <= 64; i++)
                    comboUvRotate.Items.Add(new NgAnimatedTextureSettingPair(i, "UvRotate = " + i));
            }
            else
            {
                labelHeaderNgSettings.Visible = false;
                settingsPanelNG.Visible = false;
            }

            // Init state
            _editor_EditorEventRaised(new InitEvent());
            if (comboAnimatedTextureSets.Items.Count > 0)
                comboAnimatedTextureSets.SelectedIndex = 0;

            // Setup texture map
            if (_editor.SelectedTexture.TextureIsInvisble)
                textureMap.ResetVisibleTexture(_editor.Level.Settings.Textures.Count > 0 ? _editor.Level.Settings.Textures[0] : null);
            else
                textureMap.ShowTexture(_editor.SelectedTexture);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editor.EditorEventRaised -= _editor_EditorEventRaised;
                _previewTimer?.Dispose();
                _imageCache?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void _editor_EditorEventRaised(IEditorEvent obj)
        {
            // Update texture combo box
            if (obj is InitEvent || obj is Editor.LoadedTexturesChangedEvent)
                texturesDataGridViewColumnTexture.DataSource = new BindingList<LevelTexture>(_editor.Level.Settings.Textures);

            // Update animated texture set combo box
            if (obj is InitEvent || obj is Editor.AnimatedTexturesChanged)
            {
                while (comboAnimatedTextureSets.Items.Count > _editor.Level.Settings.AnimatedTextureSets.Count)
                    comboAnimatedTextureSets.Items.RemoveAt(comboAnimatedTextureSets.Items.Count - 1);
                for (int i = 0; i < comboAnimatedTextureSets.Items.Count; ++i)
                    if (!object.ReferenceEquals(comboAnimatedTextureSets.Items[i], _editor.Level.Settings.AnimatedTextureSets[i]))
                        comboAnimatedTextureSets.Items[i] = _editor.Level.Settings.AnimatedTextureSets[i];
                while (comboAnimatedTextureSets.Items.Count < _editor.Level.Settings.AnimatedTextureSets.Count)
                    comboAnimatedTextureSets.Items.Add(_editor.Level.Settings.AnimatedTextureSets[comboAnimatedTextureSets.Items.Count]);
                if (comboAnimatedTextureSets.SelectedItem == null)
                    comboAnimatedTextureSets.Text = "";
                comboAnimatedTextureSets.Invalidate();
            }

            // Update display animated texture setup
            if (obj is InitEvent || obj is Editor.AnimatedTexturesChanged)
                UpdateCurrentAnimationDisplay();

            // Invalidate texture view
            if (obj is Editor.AnimatedTexturesChanged)
                textureMap.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.N)
            {
                AddFrame();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void AddFrame()
        {
            var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if(selectedSet == null)
            {
                selectedSet = new AnimatedTextureSet();
                _editor.Level.Settings.AnimatedTextureSets.Add(selectedSet);
                _editor.AnimatedTexturesChange();
                comboAnimatedTextureSets.SelectedItem = selectedSet;
            }

            var frame = GetSelectedAnimatedTextureFrame();
            if (frame != null)
            {
                selectedSet.Frames.Add(frame);
                _editor.AnimatedTexturesChange();
            }
        }
        
        private void UpdateCurrentAnimationDisplay()
        {
            var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (selectedSet == null)
                selectedSet = new AnimatedTextureSet();

            // Setup frames
            var dataSource = (TransparentBindingList<AnimatedTextureFrame>)(texturesDataGridView.DataSource);
            if (dataSource?.Items != selectedSet.Frames)
            {
                var newDataSource = new TransparentBindingList<AnimatedTextureFrame>(selectedSet.Frames);
                texturesDataGridView.DataSource = newDataSource;
                newDataSource.ListChanged += NewDataSource_ListChanged; // This needs to happen *after* it's set as a datasource
                // otherwise the view is updated before the datagridview is in a valid state and an exception can occure in certain cicumstances while cell formatting a deleted row.
            }
            else
            {
                try
                {
                    dataSource.ListChanged -= NewDataSource_ListChanged;
                    dataSource.ResetBindings(); // TODO Preserve selection?
                }
                finally
                {
                    dataSource.ListChanged += NewDataSource_ListChanged;
                }
            }
            UpdateEnable();

            // Setup preview
            if (selectedSet.Frames.Count == 0)
            {
                _previewTimer.Enabled = false;
                previewImage.Image = null;
                previewProgressBar.Maximum = 0;
                previewProgressBar.Value = 0;
                previewProgressBar.TextMode = DarkUI.Controls.DarkProgressBarMode.NoText;
            }
            else
            {
                _previewTimer.Enabled = true;
                previewProgressBar.TextMode = DarkUI.Controls.DarkProgressBarMode.XOfN;
            }

            if (!_isNg)
            {
                _previewTimer.Interval = (int)Math.Round(1000 / _previewFps);
            }
            else
            {
                switch (selectedSet.AnimationType)
                {
                    case AnimatedTextureAnimationType.Frames:
                        if (selectedSet.Fps > 0)
                            _previewTimer.Interval = (int)Math.Round(1000 / selectedSet.Fps / 2.0f);
                        else if (selectedSet.Fps < 0)
                            _previewTimer.Interval = -selectedSet.Fps * 1000;
                        break;

                    case AnimatedTextureAnimationType.PFrames:
                        _previewTimer.Interval = (int)Math.Round(1000 / _previewFps); // TODO: verify this
                        break;

                    case AnimatedTextureAnimationType.FullRotate:
                        _previewTimer.Interval = (int)Math.Round(1000 / (selectedSet.Fps != 0 ? selectedSet.Fps : _previewFps));
                        _lastY = 0;
                        break;
                }
            }

            // Update warning about too many frames
            int frameCount = 0;
            AnimatedTextureSet currentSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (currentSet != null)
                foreach (AnimatedTextureFrame frame in currentSet.Frames)
                    frameCount += frame.Repeat;

            if (tooManyFramesWarning.Visible = frameCount > _maxLegacyFrames)
                warningToolTip.SetToolTip(tooManyFramesWarning, "This animation uses " + frameCount + " frames which is more than " + _maxLegacyFrames + "! This will cause crashes with old engines!");

            if (_isNg)
            {
                comboEffect.SelectedItem = selectedSet.AnimationType;
                OnEffectChanged();

                switch (selectedSet.AnimationType)
                {
                    case AnimatedTextureAnimationType.Frames:
                        NgSelectComboboxValue(selectedSet.Fps, comboFps);
                        break;
                    case AnimatedTextureAnimationType.FullRotate:
                    case AnimatedTextureAnimationType.HalfRotate:
                    case AnimatedTextureAnimationType.RiverRotate:
                        NgSelectComboboxValue(selectedSet.Fps, comboFps);
                        NgSelectComboboxValue(selectedSet.UvRotate, comboUvRotate);
                        break;
                }
            }
        }

        private void NgSelectComboboxValue(int value, DarkComboBox cb)
        {
            foreach (NgAnimatedTextureSettingPair item in cb.Items)
                if (item.Key == value)
                {
                    cb.SelectedItem = item;
                    return;
                }
        }

        private void NewDataSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            _editor.AnimatedTexturesChange();
        }

        private void _previewTimer_Tick(object sender, EventArgs e)
        {
            var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;

            // Get frame and advance frame
            int frameCount = selectedSet?.Frames?.Count ?? 0;
            if (frameCount == 0)
            {
                previewImage.Image = null;
                return;
            }
            if (++_previewCurrentRepeatTimes < (_previewCurrentFrame?.Repeat ?? 0))
                return;
            int frameIndex = 0;
            for (int i = 0; i < frameCount; ++i)
                if (selectedSet.Frames[i] == _previewCurrentFrame)
                {
                    frameIndex = (i + 1) % frameCount; // Advance to next image
                    break;
                }
            _previewCurrentFrame = selectedSet.Frames[frameIndex];
            _previewCurrentRepeatTimes = 0;

            // Update view
            previewProgressBar.Minimum = 0;
            previewProgressBar.Maximum = (frameCount - 1);
            previewProgressBar.SetProgressNoAnimation(frameIndex);
            previewImage.Image = _imageCache[new CachedImageInfo
            {
                _image = _previewCurrentFrame.Texture.Image,
                _sourceTexCoord0 = _previewCurrentFrame.TexCoord0,
                _sourceTexCoord1 = _previewCurrentFrame.TexCoord1,
                _sourceTexCoord2 = _previewCurrentFrame.TexCoord2,
                _sourceTexCoord3 = _previewCurrentFrame.TexCoord3,
                _destinationSize = previewImage.ClientSize
            }];
        }

        private void _onPicturePreviewPaint(object sender, PaintEventArgs args)
        {
            var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (selectedSet == null) return;

            var image = (sender as PictureBox).Image;
            if (image == null) return;

            var g = args.Graphics;

            if (selectedSet.IsUvRotate)
            {
                g.DrawImage(image, new Point(0, _lastY * 2 - 128));
                g.DrawImage(image, new Point(0, _lastY * 2));

                _lastY += selectedSet.UvRotate;
                if (_lastY == 64 && selectedSet.Fps > 0) _lastY = 0;
                if (_lastY == 0 && selectedSet.Fps < 0) _lastY = 64;
            }
            else
            {
                g.DrawImage(image, new Point(0, 0));
            }
        }

        private void butAnimatedTextureSetNew_Click(object sender, EventArgs e)
        {
            var newSet = new AnimatedTextureSet();
            _editor.Level.Settings.AnimatedTextureSets.Add(newSet);
            _editor.AnimatedTexturesChange();
            comboAnimatedTextureSets.SelectedItem = newSet;
        }

        private void butAnimatedTextureSetDelete_Click(object sender, EventArgs e)
        {
            var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (selectedSet == null)
                return;

            if (selectedSet.Frames.Count > 0)
                if (DarkMessageBox.Show(this, "Are you sure you want to delete the animation set '" + selectedSet +
                    "'?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

            _editor.Level.Settings.AnimatedTextureSets.Remove(selectedSet);
            _editor.AnimatedTexturesChange();
        }

        private void UpdateEnable()
        {
            //if (!_loaded) return;
            bool enable = comboAnimatedTextureSets.SelectedItem is AnimatedTextureSet;
            settingsPanelNG.Enabled = enable;
            texturesDataGridViewControls.Enabled = enable;
            butAnimatedTextureSetDelete.Enabled = enable;

            if (enable)
            {
                AnimatedTextureSet selectedSet = (AnimatedTextureSet)comboAnimatedTextureSets.SelectedItem;
                texturesDataGridView.Enabled = selectedSet.Frames.Count > 0;
            }
            else
                texturesDataGridView.Enabled = enable;
        }

        private void comboAnimatedTextureSets_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateEnable();
            UpdateCurrentAnimationDisplay();
            textureMap.Invalidate();
        }

        private void butUpdate_Click(object sender, EventArgs e)
        {
            var newFrame = GetSelectedAnimatedTextureFrame();
            if (newFrame == null)
                return;

            foreach (DataGridViewRow selectedRow in texturesDataGridView.SelectedRows)
            {
                var frame = selectedRow.DataBoundItem as AnimatedTextureFrame;
                frame.Texture = newFrame.Texture;
                frame.TexCoord0 = newFrame.TexCoord0;
                frame.TexCoord1 = newFrame.TexCoord1;
                frame.TexCoord2 = newFrame.TexCoord2;
                frame.TexCoord3 = newFrame.TexCoord3;
            }
            _editor.AnimatedTexturesChange();
        }

        private AnimatedTextureFrame GetSelectedAnimatedTextureFrame()
        {
            TextureArea textureArea = textureMap.SelectedTexture;
            if (!(textureArea.Texture is LevelTexture))
            {
                DarkMessageBox.Show(this, "No valid texture region selected", "Invalid texture selection", MessageBoxIcon.Error);
                return null;
            }

            return new AnimatedTextureFrame
            {
                Texture = (LevelTexture)textureArea.Texture,
                TexCoord0 = textureArea.TexCoord0,
                TexCoord1 = textureArea.TexCoord1,
                TexCoord2 = textureArea.TexCoord2,
                TexCoord3 = textureArea.TexCoord3
            };
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void texturesDataGridView_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            if (e.DesiredType == typeof(LevelTexture))
            {
                e.Value = _editor.Level.Settings.Textures.FirstOrDefault((texture) => texture.ToString() == (string)(e.Value));
                e.ParsingApplied = true;
            }
        }

        private void texturesDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Get frame for the currently formatted row
            if (e.RowIndex >= texturesDataGridView.Rows.Count || e.ColumnIndex >= texturesDataGridView.Columns.Count)
                return;
            AnimatedTextureFrame frame;
            try
            {
                frame = (AnimatedTextureFrame)(texturesDataGridView.Rows[e.RowIndex].DataBoundItem);
            }
            catch (Exception exc)
            {
                logger.Info(exc, "Cell formatting failed for row " + e.RowIndex + ".");
                return;
            }

            // Do cell formatting
            if ((e.DesiredType == typeof(Image)) || e.DesiredType.IsSubclassOf(typeof(Image)))
            {
                // Image column
                CachedImageInfo info;
                info._destinationSize = new Size(texturesDataGridView.Columns[e.ColumnIndex].Width, texturesDataGridView.Rows[e.RowIndex].Height);
                info._sourceTexCoord0 = frame.TexCoord0;
                info._sourceTexCoord1 = frame.TexCoord1;
                info._sourceTexCoord2 = frame.TexCoord2;
                info._sourceTexCoord3 = frame.TexCoord3;
                info._image = frame.Texture.Image;

                e.Value = _imageCache[info];
                e.FormattingApplied = true;
            }
            else if (texturesDataGridView.Columns[e.ColumnIndex].Name == texturesDataGridViewColumnArea.Name)
            {
                var area = frame.Area;
                e.Value = "(" + area.X + ", " + area.Y + ")-> (" + area.Right + ", " + area.Bottom + ")";
                e.FormattingApplied = true;
            }
        }

        private static ImageC GetPerspectivePreview(ImageC input, Vector2 texCoord01, Vector2 texCoord00, Vector2 texCoord10, Vector2 texCoord11, Size size)
        {
            // Project the chosen texture onto the rectangular preview ...
            ImageC output = ImageC.CreateNew(size.Width, size.Height);

            float xTexCoordFactor = 1.0f / size.Width;
            float yTexCoordFactor = 1.0f / size.Width;
            Vector2 max = input.Size - new Vector2(1.0f);

            for (int y = 0; y < size.Height; ++y)
                for (int x = 0; x < size.Width; ++x)
                {
                    float outputTexCoordX = (x + 0.5f) * xTexCoordFactor;
                    float outputTexCoordY = (y + 0.5f) * yTexCoordFactor;
                    Vector2 inputTexCoord = (
                            texCoord00 * ((1.0f - outputTexCoordX) * (1.0f - outputTexCoordY)) +
                            texCoord01 * ((1.0f - outputTexCoordX) * outputTexCoordY)
                        ) + (
                            texCoord10 * (outputTexCoordX * (1.0f - outputTexCoordY)) +
                            texCoord11 * (outputTexCoordX * outputTexCoordY)
                        );

                    // Bilinear filter from the input
                    inputTexCoord -= new Vector2(0.5f); // Offset of texture coordinate from texel midpoint
                    inputTexCoord = Vector2.Min(Vector2.Max(inputTexCoord, new Vector2()), max); // Clamp into available texture space

                    int firstX = (int)(inputTexCoord.X);
                    int firstY = (int)(inputTexCoord.Y);
                    int secondX = Math.Min(firstX + 1, input.Width);
                    int secondY = Math.Min(firstY + 1, input.Height);
                    float secondFactorX = inputTexCoord.X - firstX;
                    float secondFactorY = inputTexCoord.Y - firstY;

                    Vector4 outputPixel = (
                            ((Vector4)input.GetPixel(secondX, secondY) * (secondFactorX * secondFactorY)) +
                            ((Vector4)input.GetPixel(secondX, firstY) * (secondFactorX * (1.0f - secondFactorY)))
                        ) + (
                            ((Vector4)input.GetPixel(firstX, secondY) * ((1.0f - secondFactorX) * secondFactorY)) +
                            ((Vector4)input.GetPixel(firstX, firstY) * ((1.0f - secondFactorX) * (1.0f - secondFactorY)))
                        );
                    output.SetPixel(x, y, (ColorC)outputPixel);
                }
            return output;
        }

        private void texturesDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            butUpdate.Enabled = texturesDataGridView.SelectedRows.Count > 0 && texturesDataGridView.Rows.Count > 0;
        }

        private void texturesDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (texturesDataGridView.SelectedRows.Count > 0)
            {
                var frame = (AnimatedTextureFrame)(texturesDataGridView.SelectedRows[0].DataBoundItem);
                var textureToShow = new TextureArea
                {
                    Texture = frame.Texture,
                    TexCoord0 = frame.TexCoord0,
                    TexCoord1 = frame.TexCoord1,
                    TexCoord2 = frame.TexCoord2,
                    TexCoord3 = frame.TexCoord3
                };

                if (textureMap.SelectedTexture != textureToShow)
                    textureMap.ShowTexture(textureToShow);
            }
        }

        public class PanelTextureMapForAnimations : Controls.PanelTextureMap
        {
            protected override float MaxTextureSize => float.PositiveInfinity;
            protected override bool DrawTriangle => false;

            private static readonly Pen outlinePen = new Pen(System.Drawing.Color.Silver, 2);
            private static readonly Pen activeOutlinePen = new Pen(System.Drawing.Color.Violet, 2);
            private static readonly Brush textBrush = new SolidBrush(System.Drawing.Color.Violet);
            private static readonly Brush textShadowBrush = new SolidBrush(System.Drawing.Color.Black);
            private static readonly Font textFont = new Font("Segoe UI", 12.0f, FontStyle.Bold, GraphicsUnit.Pixel);
            private static readonly StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

            protected override void OnPaintSelection(PaintEventArgs e)
            {
                // Paint other animated textures
                LevelSettings levelSettings = ParentForm._editor.Level.Settings;
                if (levelSettings.AnimatedTextureSets.Count > 0)
                {
                    var selectedSet = ParentForm.comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;

                    foreach (AnimatedTextureSet set in levelSettings.AnimatedTextureSets)
                        if (set != selectedSet)
                            DrawSetOutlines(e, set, false);

                    DrawSetOutlines(e, selectedSet, true);
                }

                // Paint current selection
                base.OnPaintSelection(e);
            }

            private void DrawSetOutlines(PaintEventArgs e, AnimatedTextureSet set, bool current)
            {
                if (set == null)
                    return;

                for (int i = 0; i < set.Frames.Count; ++i)
                {
                    AnimatedTextureFrame frame = set.Frames[i];
                    if (frame.Texture == SelectedTexture.Texture)
                    {
                        PointF[] edges = new PointF[]
                        {
                                ToVisualCoord(frame.TexCoord0),
                                ToVisualCoord(frame.TexCoord1),
                                ToVisualCoord(frame.TexCoord2),
                                ToVisualCoord(frame.TexCoord3)
                        };

                        PointF upperLeft = new PointF(
                            Math.Min(Math.Min(edges[0].X, edges[1].X), Math.Min(edges[2].X, edges[3].X)),
                            Math.Min(Math.Min(edges[0].Y, edges[1].Y), Math.Min(edges[2].Y, edges[3].Y)));
                        PointF lowerRight = new PointF(
                            Math.Max(Math.Max(edges[0].X, edges[1].X), Math.Max(edges[2].X, edges[3].X)),
                            Math.Max(Math.Max(edges[0].Y, edges[1].Y), Math.Max(edges[2].Y, edges[3].Y)));

                        if (current)
                        {
                            string counterString = (i + 1) + "/" + set.Frames.Count;
                            SizeF textSize = e.Graphics.MeasureString(counterString, textFont);
                            RectangleF textArea = RectangleF.FromLTRB(upperLeft.X, upperLeft.Y, lowerRight.X, lowerRight.Y);

                            if(textArea.Width > textSize.Width && textArea.Height > textSize.Height)
                            {
                                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                                e.Graphics.DrawString(counterString, textFont, textShadowBrush, textArea, textFormat);
                                textArea.X -= 1;
                                textArea.Y -= 1;
                                e.Graphics.DrawString(counterString, textFont, textBrush, textArea, textFormat);
                            }
                        }
                        e.Graphics.DrawPolygon(current ? activeOutlinePen : outlinePen, edges);
                    }
                }
            }

            protected FormAnimatedTextures ParentForm
            {
                get
                {
                    Control parent = Parent;
                    while (!(parent is FormAnimatedTextures))
                        parent = parent.Parent;
                    return (FormAnimatedTextures)parent;
                }
            }
        }

        private void textureMap_DoubleClick(object sender, EventArgs e)
        {
            AddFrame();
        }

        private void comboEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnEffectChanged();
        }

        private void OnEffectChanged()
        {
            var effect = (AnimatedTextureAnimationType)comboEffect.SelectedItem;
            switch (effect)
            {
                case AnimatedTextureAnimationType.Frames:
                    labelFps.Visible = true;
                    comboFps.Visible = true;
                    labelUvRotate.Visible = false;
                    comboUvRotate.Visible = false;

                    comboFps.Items.Clear();
                    comboFps.Items.Add(new NgAnimatedTextureSettingPair(0, "Default"));
                    for (var i = 30; i > 1; i--)
                        comboFps.Items.Add(new NgAnimatedTextureSettingPair(i, i + " Fps"));
                    for (var i = 1; i <= 8; i++)
                        comboFps.Items.Add(new NgAnimatedTextureSettingPair(-i, i + " Sfps"));
                    comboFps.SelectedIndex = 0;

                    break;

                case AnimatedTextureAnimationType.PFrames:
                    labelFps.Visible = false;
                    comboFps.Visible = false;
                    labelUvRotate.Visible = false;
                    comboUvRotate.Visible = false;
                    break;

                case AnimatedTextureAnimationType.FullRotate:
                case AnimatedTextureAnimationType.HalfRotate:
                case AnimatedTextureAnimationType.RiverRotate:
                    labelFps.Visible = true;
                    comboFps.Visible = true;
                    labelUvRotate.Visible = true;
                    comboUvRotate.Visible = true;

                    comboFps.Items.Clear();
                    comboFps.Items.Add(new NgAnimatedTextureSettingPair(0, "Default"));
                    for (var i = 1; i <= 32; i++)
                        comboFps.Items.Add(new NgAnimatedTextureSettingPair(i, i + " Fps"));

                    comboFps.SelectedIndex = 0;
                    comboUvRotate.SelectedIndex = 64;

                    break;
            }
        }

        private void comboEffect_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (selectedSet == null) return;
            selectedSet.AnimationType = (AnimatedTextureAnimationType)comboEffect.SelectedItem;
            _editor.AnimatedTexturesChange();
        }

        private void comboFps_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (selectedSet == null) return;
            selectedSet.Fps = (sbyte)((NgAnimatedTextureSettingPair)comboFps.SelectedItem).Key;
            _editor.AnimatedTexturesChange();
        }

        private void comboUvRotate_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (selectedSet == null) return;
            selectedSet.UvRotate = (sbyte)((NgAnimatedTextureSettingPair)comboUvRotate.SelectedItem).Key;
            _editor.AnimatedTexturesChange();
        }
    }
}
