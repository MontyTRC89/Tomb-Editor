﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Controls;
using DarkUI.Forms;
using NLog;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib;
using TombLib.Forms;
using RectangleF = System.Drawing.RectangleF;
using DarkUI.Extensions;

namespace TombEditor.Forms
{
    public partial class FormAnimatedTextures : DarkForm
    {
        private enum ProceduralAnimationType
        {
            HorizontalStretch,
            VerticalStretch,
            Scale,
            HorizontalSkew1,
            HorizontalSkew2,
            VerticalSkew1,
            VerticalSkew2,
            Spin,
            HorizontalPan,
            VerticalPan,
            Shake
        }

        private enum AnimGenerationType
        {
            New,
            Clone,
            Merge,
            Replace,
            AddFrames
        }

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
        }
        private struct NgAnimatedTextureSettingPair
        {
            public readonly float Key;
            public readonly string Value;

            public NgAnimatedTextureSettingPair(float key, string value)
            {
                Key = key;
                Value = value;
            }

            public override string ToString()
            {
                return Value;
            }
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly PopUpInfo popup = new PopUpInfo();

        private readonly Editor _editor;
        private readonly Cache<CachedImageInfo, Bitmap> _imageCache;
        private readonly static ImageC transparentBackground = ImageC.FromSystemDrawingImage(Properties.Resources.misc_TransparentBackground);
        
        private readonly bool _isNg;

        private readonly Timer _previewTimer = new Timer();
        private AnimatedTextureFrame _previewCurrentFrame;
        private int _previewCurrentRepeatTimes;
        private const int _maxLegacyFrames = 16;
        private const string animNameCombineString = " (with ";
        private int _lastY;

        private List<AnimatedTextureSet> _backupSets = new List<AnimatedTextureSet>();


        public FormAnimatedTextures(Editor editor)
        {
            InitializeComponent();

            _editor = editor;
            _editor.EditorEventRaised += EditorEventRaised;

            _previewTimer.Tick += _previewTimer_Tick;
            previewImage.Paint += _onPicturePreviewPaint;

            // Setup image cache
            _imageCache = new Cache<CachedImageInfo, Bitmap>(512, subsection =>
                {
                    return GetPerspectivePreview(subsection._image, subsection._sourceTexCoord0, subsection._sourceTexCoord1, subsection._sourceTexCoord2,
                        subsection._sourceTexCoord3, subsection._destinationSize).ToBitmap();
                });

            _isNg = _editor.Level.Settings.GameVersion == TRVersion.Game.TRNG;

            // Set window property handlers
            Configuration.LoadWindowProperties(this, _editor.Configuration);
            FormClosing += new FormClosingEventHandler((s, e) => Configuration.SaveWindowProperties(this, _editor.Configuration));

            // Setup controls
            SetupControls();

            // Setup data grid view
            texturesDataGridViewControls.DataGridView = texturesDataGridView;
            texturesDataGridViewControls.CreateNewRow = GetSelectedAnimatedTextureFrame;
            texturesDataGridViewColumnTexture.DataSource = new BindingList<LevelTexture>(editor.Level.Settings.Textures);
            
            // Init state
            if (comboAnimatedTextureSets.Items.Count > 0)
                comboAnimatedTextureSets.SelectedIndex = 0;

            // Setup texture map
            if (_editor.SelectedTexture.TextureIsInvisible)
                textureMap.ResetVisibleTexture(_editor.Level.Settings.Textures.Count > 0 ? _editor.Level.Settings.Textures[0] : null);
            else
                textureMap.ShowTexture(_editor.SelectedTexture);

            // Backup existing animated texture sets
            foreach (var set in editor.Level.Settings.AnimatedTextureSets)
                _backupSets.Add(set.Clone());

            // Hack to prevent artifacts with progress bar redraw
            Resize += (s, e) => { Refresh(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editor.EditorEventRaised -= EditorEventRaised;
                _previewTimer?.Dispose();
                _imageCache?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // @FIXME: Proper event handling for these events was removed, because there were
            // serious issues with DarkDataGridView on level reloading or unloading textures.
            if (obj is Editor.LevelChangedEvent || obj is Editor.LoadedTexturesChangedEvent)
                Close();

            // Update animated texture set combo box
            if (obj is Editor.AnimatedTexturesChangedEvent)
            {
                while (comboAnimatedTextureSets.Items.Count > _editor.Level.Settings.AnimatedTextureSets.Count)
                    comboAnimatedTextureSets.Items.RemoveAt(comboAnimatedTextureSets.Items.Count - 1);

                for (int i = 0; i < comboAnimatedTextureSets.Items.Count; ++i)
                    if (!ReferenceEquals(comboAnimatedTextureSets.Items[i], _editor.Level.Settings.AnimatedTextureSets[i]))
                        comboAnimatedTextureSets.Items[i] = _editor.Level.Settings.AnimatedTextureSets[i];

                while (comboAnimatedTextureSets.Items.Count < _editor.Level.Settings.AnimatedTextureSets.Count)
                    comboAnimatedTextureSets.Items.Add(_editor.Level.Settings.AnimatedTextureSets[comboAnimatedTextureSets.Items.Count]);

                if (comboAnimatedTextureSets.SelectedItem == null)
                    if (comboAnimatedTextureSets.Items.Count > 0)
                        comboAnimatedTextureSets.SelectedIndex = comboAnimatedTextureSets.Items.Count - 1;
                    else
                        comboAnimatedTextureSets.Text = "";
                comboAnimatedTextureSets.Invalidate();

                UpdateCurrentAnimationDisplay();
                textureMap.Invalidate();
            }
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

        private AnimatedTextureSet GetOrAddCurrentSet()
        {
            var currentSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (currentSet == null)
                currentSet = NewSet();
            return currentSet;
        }

        private AnimatedTextureSet NewSet()
        {
            var newSet = new AnimatedTextureSet() { Name = "Animation #" + _editor.Level.Settings.AnimatedTextureSets.Count };
            _editor.Level.Settings.AnimatedTextureSets.Add(newSet);
            _editor.AnimatedTexturesChange();
            comboAnimatedTextureSets.SelectedItem = newSet;
            return newSet;
        }

        private void AddFrame()
        {
            var selectedSet = GetOrAddCurrentSet();
            var frame = GetSelectedAnimatedTextureFrame();
            if (frame != null)
            {
                selectedSet.Frames.Add(frame);
                _editor.AnimatedTexturesChange();
            }

            texturesDataGridView.ClearSelection();
            texturesDataGridView.CurrentCell = texturesDataGridView.Rows[texturesDataGridView.Rows.Count - 1].Cells[0];
        }

        private void SetupControls()
        {
            // Clear previous elements
            comboCurrentTexture.Items.Clear();
            comboAnimatedTextureSets.Items.Clear();
            comboEffect.Items.Clear();
            comboUvRotate.Items.Clear();
            comboFps.Items.Clear();

            // Populate texture list
            comboCurrentTexture.Items.AddRange(_editor.Level.Settings.Textures.ToArray());
            comboCurrentTexture.SelectedItem = _editor.Level.Settings.Textures.FirstOrDefault();

            // Populate anim sets list
            while (comboAnimatedTextureSets.Items.Count < _editor.Level.Settings.AnimatedTextureSets.Count)
                comboAnimatedTextureSets.Items.Add(_editor.Level.Settings.AnimatedTextureSets[comboAnimatedTextureSets.Items.Count]);

            // Add common animation types
            comboEffect.Items.Add(AnimatedTextureAnimationType.Frames);
            comboEffect.Items.Add(AnimatedTextureAnimationType.UVRotate);

            // NG settings
            if (_isNg)
            {
                // For now, add only P-Frames mode, as Half-Rotate and River-Rotate modes are faulty.
                comboEffect.Items.Add(AnimatedTextureAnimationType.PFrames);

                // Fill uv rotate combobox
                for (int i = -64; i < 0; i++)
                    comboUvRotate.Items.Add(new NgAnimatedTextureSettingPair(i, "UvRotate = " + i));
                comboUvRotate.Items.Add(new NgAnimatedTextureSettingPair(0, "Default (from script)"));
                for (int i = 1; i <= 64; i++)
                    comboUvRotate.Items.Add(new NgAnimatedTextureSettingPair(i, "UvRotate = " + i));

                // Fill with NG predefined FPS values required for river rotate etc.
                for (int i = 1; i <= 32; i++)
                    comboFps.Items.Add(new NgAnimatedTextureSettingPair(i, i + " FPS"));
            }
            else
            {
                comboUvRotate.Enabled = false;
                comboFps.Enabled = false;
                numericUpDownFPS.Enabled = false;
            }

            // Legacy engine settings
            comboEffect.Enabled = _editor.Level.Settings.GameVersion >= TRVersion.Game.TR4;               

            comboProcPresets.SelectedIndex = 0;
            numFrames.Value = _maxLegacyFrames;
        }

        private void UpdateCurrentAnimationDisplay()
        {
            var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (selectedSet == null)
                selectedSet = new AnimatedTextureSet();

            // Setup frames
            var dataSource = (TransparentBindingList<AnimatedTextureFrame>)texturesDataGridView.DataSource;
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
                previewProgressBar.TextMode = DarkProgressBarMode.NoText;
            }
            else
            {
                _previewTimer.Enabled = true;
                previewProgressBar.TextMode = DarkProgressBarMode.XOfN;
            }
            _previewTimer.Interval = (int)MathC.Clamp(Math.Round(1000.0f / selectedSet.Fps), 1, int.MaxValue); // Without NG, the default value of 15 should be present at this point.
            _lastY = 0;

            // Update warning about too many frames
            int frameCount = 0;
            AnimatedTextureSet currentSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (currentSet != null)
                foreach (AnimatedTextureFrame frame in currentSet.Frames)
                    frameCount += frame.Repeat;

            if (tooManyFramesWarning.Visible = _editor.Level.Settings.GameVersion == TRVersion.Game.TRNG && frameCount > _maxLegacyFrames)
                toolTip.SetToolTip(tooManyFramesWarning, "This animation uses " + frameCount + " frames which is more than " + _maxLegacyFrames + "!\nThis will cause crash in TRNG!");

            comboEffect.SelectedItem = selectedSet.AnimationType;

            if (_isNg)
            {
                OnEffectChanged();
                switch (selectedSet.AnimationType)
                {
                    case AnimatedTextureAnimationType.Frames:
                        numericUpDownFPS.Value = (decimal)selectedSet.Fps;
                        break;
                    case AnimatedTextureAnimationType.UVRotate:
                    case AnimatedTextureAnimationType.HalfRotate:
                    case AnimatedTextureAnimationType.RiverRotate:
                        NgSelectComboboxValue(selectedSet.Fps, comboFps);
                        NgSelectComboboxValue(selectedSet.UvRotate, comboUvRotate);
                        break;
                }
            }
        }

        private void NgSelectComboboxValue(float value, DarkComboBox cb)
        {
            var bestItem = cb.Items.Cast<NgAnimatedTextureSettingPair>().First();
            foreach (NgAnimatedTextureSettingPair item in cb.Items)
                if (Math.Abs(item.Key - value) < Math.Abs(bestItem.Key - value)) // Chose the entry closest to the float we are looking for.
                    bestItem = item;
            cb.SelectedItem = bestItem;
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

            int frameIndex = 0;

            // Only advance to next frame for frame type sequences, otherwise show selected or first frame
            if (selectedSet.AnimationType == AnimatedTextureAnimationType.Frames)
            {
                if (++_previewCurrentRepeatTimes < (_previewCurrentFrame?.Repeat ?? 0))
                    return;

                for (int i = 0; i < frameCount; ++i)
                    if (selectedSet.Frames[i] == _previewCurrentFrame)
                    {
                        frameIndex = (i + 1) % frameCount; // Advance to next image
                        break;
                    }

                _previewCurrentRepeatTimes = 0;
            }
            else if (texturesDataGridView.CurrentRow != null)
                frameIndex = texturesDataGridView.CurrentRow.Index;

            _previewCurrentFrame = selectedSet.Frames[frameIndex];

            // Update view
            previewProgressBar.Minimum = 0;
            previewProgressBar.Maximum = frameCount - 1;
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
            if (selectedSet == null)
                return;

            var image = (sender as PictureBox).Image;
            if (image == null)
                return;

            var g = args.Graphics;

            if (selectedSet.IsUvRotate)
            {
                g.DrawImage(image, new Point(0, _lastY * 2 - 128));
                g.DrawImage(image, new Point(0, _lastY * 2));

                _lastY += selectedSet.UvRotate;
                if (_lastY >= 64 && selectedSet.UvRotate > 0)
                    _lastY = 0;
                if (_lastY <= 0 && selectedSet.UvRotate < 0)
                    _lastY = 64;
            }
            else
            {
                g.DrawImage(image, new Point(0, 0));
            }
        }

        private void butAnimatedTextureSetNew_Click(object sender, EventArgs e)
        {
            NewSet();
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
            settingsPanel.Enabled = enable;
            texturesDataGridViewControls.Enabled = enable;
            butAnimatedTextureSetDelete.Enabled = enable;
            butEditSetName.Enabled = enable;
            butCloneProcAnim.Enabled = enable;
            butMergeProcAnim.Enabled = enable;
            butReplaceProcAnim.Enabled = enable;
            butAddProcAnim.Enabled = enable;

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
                popup.ShowError(textureMap, "No valid texture region selected", "Invalid selection");
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

        private bool GenerateProceduralAnimation(ProceduralAnimationType type, int resultingFrameCount = _maxLegacyFrames, float effectStrength = 1.0f, bool smooth = true, bool loop = true, AnimGenerationType genType = AnimGenerationType.New)
        {
            TextureArea textureArea = textureMap.SelectedTexture;
            if (!(textureArea.Texture is LevelTexture))
            {
                popup.ShowError(textureMap, "No valid texture region selected", "Invalid selection");
                return false;
            }

            // Limit effect strength to reasonable value and additionally reverse it for scale/stretch types,
            // because for scale/stretch types, visible effect opposes mathematical function.
            effectStrength = (float)MathC.Clamp((type <= ProceduralAnimationType.Scale) ? -effectStrength : effectStrength, -1.0, 1.0);

            int startIndex = 0;
            AnimatedTextureSet targetSet = null;

            // Initialize any type which involves existing set
            if (genType != AnimGenerationType.New)
            {
                if (comboAnimatedTextureSets.SelectedItem == null || !(comboAnimatedTextureSets.SelectedItem is AnimatedTextureSet))
                {
                    popup.ShowError(textureMap, "No valid animation selected!");
                    return false;
                }

                targetSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;

                if (genType == AnimGenerationType.Clone)
                    targetSet = targetSet.Clone(); // Make a copy and use it for clone type
                else if (genType == AnimGenerationType.AddFrames && targetSet.Frames.Count > 0)
                    startIndex = texturesDataGridView.CurrentRow.Index + 1; // Insert frames at selection point
                else if (genType == AnimGenerationType.Replace)
                    targetSet.Frames.Clear();

                if(string.IsNullOrEmpty(targetSet.Name))
                    targetSet.Name = "Animation #" + (_editor.Level.Settings.AnimatedTextureSets.Count + 1);
            }
            else
                targetSet = new AnimatedTextureSet() { Name = "Procedural animation #" + (_editor.Level.Settings.AnimatedTextureSets.Count + 1) };

            if (genType == AnimGenerationType.New ||
                genType == AnimGenerationType.Replace ||
                genType == AnimGenerationType.AddFrames)
            {
                // Generate dummy reference frames and insert them into set

                for (int i = 0; i < resultingFrameCount; i++)
                {
                    var dummyFrame = new AnimatedTextureFrame
                    {
                        Texture = (LevelTexture)textureArea.Texture,
                        TexCoord0 = textureArea.TexCoord0,
                        TexCoord1 = textureArea.TexCoord1,
                        TexCoord2 = textureArea.TexCoord2,
                        TexCoord3 = textureArea.TexCoord3
                    };

                    if (genType != AnimGenerationType.AddFrames)
                        targetSet.Frames.Add(dummyFrame);
                    else
                        targetSet.Frames.Insert(startIndex, dummyFrame);
                }
            }

            // Actualize frame count, cause it may change in regard of generation type
            if (genType != AnimGenerationType.AddFrames)
                resultingFrameCount = targetSet.Frames.Count;

            if (resultingFrameCount <= 0)
            {
                popup.ShowError(textureMap, "Selected animation has no frames!");
                return false;
            }

            // Used to bypass smooth and loop, which doesn't make sense with single-frame anims
            bool realAnim = resultingFrameCount > 1;

            // Crop existing postfixes to prevent "Copy of Copy of Copy..." problem
            if (genType != AnimGenerationType.New)
            {
                int foundPostfixPos = targetSet.Name.IndexOf(animNameCombineString);
                if (foundPostfixPos != -1)
                    targetSet.Name = targetSet.Name.Substring(0, foundPostfixPos);
                targetSet.Name += animNameCombineString + comboProcPresets.SelectedItem.ToString() + " effect)";
            }

            var rnd = new Random(); // Needed for shake procedure

            // For shake procedure, smooth makes no sense without loop and vice versa.
            if (type == ProceduralAnimationType.Shake && smooth)
                loop = true;

            // Process frames

            for (int cnt = 0, i = startIndex; cnt < resultingFrameCount; cnt++, i++)
            {
                var referenceFrame = targetSet.Frames[i].Clone();
                float midFrame = loop && realAnim ? resultingFrameCount / 2.0f : resultingFrameCount;
                float bias = Math.Abs(cnt - midFrame) / midFrame;
                float weight = (smooth && realAnim ? (float)MathC.SmoothStep(0.0, 1.0, bias) : bias) * effectStrength;

                switch (type)
                {
                    case ProceduralAnimationType.HorizontalSkew1:
                    case ProceduralAnimationType.HorizontalSkew2:
                    case ProceduralAnimationType.VerticalSkew1:
                    case ProceduralAnimationType.VerticalSkew2:
                        {
                            bool otherDirection = (int)type % 2 == 0;

                            // Invert skew angle in regard of strength sign and direction
                            if (effectStrength > 0.0f)
                                weight = effectStrength - weight;
                            else
                                weight = Math.Abs(weight);
                            if (!otherDirection)
                                weight = 1.0f - weight;

                            int[] index = new int[2];
                            Vector2[] coord1 = new Vector2[2];
                            Vector2[] coord2 = new Vector2[2];

                            if (otherDirection)
                            {
                                index[0] = 0;
                                index[1] = 2;
                            }
                            else
                            {
                                index[0] = 1;
                                index[1] = 3;
                            }

                            if (type == ProceduralAnimationType.HorizontalSkew2)
                            {
                                coord2[0] = referenceFrame.TexCoord3;
                                coord2[1] = referenceFrame.TexCoord1;
                            }
                            else
                            {
                                coord2[0] = referenceFrame.TexCoord1;
                                coord2[1] = referenceFrame.TexCoord3;
                            }

                            if(type == ProceduralAnimationType.HorizontalSkew1)
                            {
                                coord1[0] = referenceFrame.TexCoord2;
                                coord1[1] = referenceFrame.TexCoord0;
                            }
                            else
                            {
                                coord1[0] = referenceFrame.TexCoord0;
                                coord1[1] = referenceFrame.TexCoord2;
                            }

                            for (int c = 0; c < 2; c++)
                            {
                                var result = Vector2.Lerp(coord1[c], coord2[c], weight);
                                switch (index[c])
                                {
                                    case 0: targetSet.Frames[i].TexCoord0 = result; break;
                                    case 1: targetSet.Frames[i].TexCoord1 = result; break;
                                    case 2: targetSet.Frames[i].TexCoord2 = result; break;
                                    case 3: targetSet.Frames[i].TexCoord3 = result; break;
                                }
                            }
                        }
                        break;

                    case ProceduralAnimationType.HorizontalStretch:
                    case ProceduralAnimationType.VerticalStretch:
                    case ProceduralAnimationType.Scale:
                        {
                            // Reverse effect strength for negative numbers for proper UV positioning
                            if (effectStrength < 0)
                                weight += Math.Abs(effectStrength);

                            // 0 = horizontal pass, 1 = vertical pass. Scale type uses both.
                            bool[] passes = new bool[2]
                            {
                                type == ProceduralAnimationType.HorizontalStretch || type == ProceduralAnimationType.Scale,
                                type == ProceduralAnimationType.VerticalStretch   || type == ProceduralAnimationType.Scale
                            };

                            for (int p = 0; p < 2; p++)
                                if (passes[p])
                                {
                                    var center1 = Vector2.Lerp(referenceFrame.TexCoord0, (p == 0 ? referenceFrame.TexCoord3 : referenceFrame.TexCoord1), 0.5f);
                                    var center2 = Vector2.Lerp(referenceFrame.TexCoord2, (p == 0 ? referenceFrame.TexCoord1 : referenceFrame.TexCoord3), 0.5f);

                                    targetSet.Frames[i].TexCoord0 = Vector2.Lerp(referenceFrame.TexCoord0, center1, weight);
                                    targetSet.Frames[i].TexCoord1 = Vector2.Lerp(referenceFrame.TexCoord1, (p == 0 ? center2 : center1), weight);
                                    targetSet.Frames[i].TexCoord2 = Vector2.Lerp(referenceFrame.TexCoord2, center2, weight);
                                    targetSet.Frames[i].TexCoord3 = Vector2.Lerp(referenceFrame.TexCoord3, (p == 0 ? center1 : center2), weight);

                                    if(p == 0)
                                        referenceFrame = targetSet.Frames[i].Clone(); // Reassign reference after first pass
                                }
                        }
                        break;

                    case ProceduralAnimationType.Spin:
                        {
                            // Invert weight and shift angle to start from the beginning
                            weight = effectStrength - weight;
                            double currAngle = ((2 * Math.PI) * weight) - Math.PI * 0.25;

                            var cross1 = Vector3.Cross(new Vector3(referenceFrame.TexCoord0.X, referenceFrame.TexCoord0.Y, 1),
                                                       new Vector3(referenceFrame.TexCoord2.X, referenceFrame.TexCoord2.Y, 1));
                            var cross2 = Vector3.Cross(new Vector3(referenceFrame.TexCoord1.X, referenceFrame.TexCoord1.Y, 1),
                                                       new Vector3(referenceFrame.TexCoord3.X, referenceFrame.TexCoord3.Y, 1));

                            var intersection = Vector3.Cross(cross1, cross2);
                            var center = new Vector2(intersection.X / intersection.Z, intersection.Y / intersection.Z);

                            float r0 = Vector2.Distance(center, referenceFrame.TexCoord0);
                            float r1 = Vector2.Distance(center, referenceFrame.TexCoord1);
                            float r2 = Vector2.Distance(center, referenceFrame.TexCoord2);
                            float r3 = Vector2.Distance(center, referenceFrame.TexCoord3);

                            if (float.IsNaN(r1) || float.IsNaN(r2) || float.IsNaN(r3) || float.IsNaN(r0))
                                continue;

                            targetSet.Frames[i].TexCoord0 = new Vector2(center.X + r0 * (float)Math.Cos(currAngle + Math.PI), center.Y + r0 * (float)Math.Sin(currAngle + Math.PI));
                            targetSet.Frames[i].TexCoord1 = new Vector2(center.X + r1 * (float)Math.Cos(currAngle + Math.PI * 1.5f), center.Y + r1 * (float)Math.Sin(currAngle + Math.PI * 1.5f));
                            targetSet.Frames[i].TexCoord2 = new Vector2(center.X + r2 * (float)Math.Cos(currAngle), center.Y + r2 * (float)Math.Sin(currAngle));
                            targetSet.Frames[i].TexCoord3 = new Vector2(center.X + r3 * (float)Math.Cos(currAngle + Math.PI / 2), center.Y + r3 * (float)Math.Sin(currAngle + Math.PI / 2));
                        }
                        break;

                    case ProceduralAnimationType.HorizontalPan:
                    case ProceduralAnimationType.VerticalPan:
                        {
                            bool horizontal = type == ProceduralAnimationType.HorizontalPan;
                            var multiplier = -Math.Sign(effectStrength) * (Math.Abs(weight) - Math.Abs(effectStrength)); // Start from origin

                            var dist1 = Vector2.Distance(referenceFrame.TexCoord0, horizontal ? referenceFrame.TexCoord3 : referenceFrame.TexCoord1) * multiplier;
                            var dist2 = Vector2.Distance(referenceFrame.TexCoord1, horizontal ? referenceFrame.TexCoord2 : referenceFrame.TexCoord0) * multiplier;

                            targetSet.Frames[i].TexCoord0 += horizontal ? new Vector2(dist1, 0) : new Vector2(0, dist1);
                            targetSet.Frames[i].TexCoord1 += horizontal ? new Vector2(dist2, 0) : new Vector2(0, dist1);
                            targetSet.Frames[i].TexCoord2 += horizontal ? new Vector2(dist1, 0) : new Vector2(0, dist2);
                            targetSet.Frames[i].TexCoord3 += horizontal ? new Vector2(dist2, 0) : new Vector2(0, dist2);
                        }
                        break;

                    case ProceduralAnimationType.Shake:
                        {
                            // Shake strength is constant in case effect is non-symmetric (looped).
                            // Negative shake doesn't make sense, so we apply Abs() onto effect strength.
                            int rndStrength = (int)((loop ? Math.Abs(effectStrength - weight) : effectStrength) * 16.0f);

                            float xRnd = rnd.Next(-rndStrength, rndStrength);
                            float yRnd = rnd.Next(-rndStrength, rndStrength);
                            Vector2 rndAdd = new Vector2(xRnd, yRnd);

                            targetSet.Frames[i].TexCoord0 += rndAdd;
                            targetSet.Frames[i].TexCoord1 += rndAdd;
                            targetSet.Frames[i].TexCoord2 += rndAdd;
                            targetSet.Frames[i].TexCoord3 += rndAdd;
                        }
                        break;
                }
            }

            // Add new set to level for types which create new animation, otherwise just send change event.
            if(genType < AnimGenerationType.Merge)
            {
                if (_editor.Level.Settings.AnimatedTextureSets.Contains(targetSet))
                {
                    popup.ShowInfo(textureMap, "Animation with same properties already exists.");
                    comboAnimatedTextureSets.SelectedItem = targetSet;
                    return false;
                }
                else
                {
                    _editor.Level.Settings.AnimatedTextureSets.Add(targetSet);
                    _editor.AnimatedTexturesChange();
                    comboAnimatedTextureSets.SelectedItem = targetSet;
                }
            }
            else
                _editor.AnimatedTexturesChange();

            // Point to last generated frame
            int rowIndex = startIndex + resultingFrameCount - 1;
            texturesDataGridView.CurrentCell = texturesDataGridView.Rows[rowIndex].Cells[0];
            texturesDataGridView.FirstDisplayedScrollingRowIndex = rowIndex;

            return true;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void texturesDataGridView_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            if (e.DesiredType == typeof(LevelTexture))
            {
                e.Value = _editor.Level.Settings.Textures.FirstOrDefault(texture => texture.ToString() == (string)e.Value);
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
                frame = (AnimatedTextureFrame)texturesDataGridView.Rows[e.RowIndex].DataBoundItem;
            }
            catch (Exception exc)
            {
                logger.Info(exc, "Cell formatting failed for row " + e.RowIndex + ".");
                return;
            }

            // Do cell formatting
            if (e.DesiredType == typeof(Image) || e.DesiredType.IsSubclassOf(typeof(Image)))
            {
                // Image column
                CachedImageInfo info;
                info._destinationSize = new Size(texturesDataGridView.Columns[e.ColumnIndex].Width - 1, texturesDataGridView.Rows[e.RowIndex].Height - 1);
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
                e.Value = "(" + area.X0 + ", " + area.Y1 + ")-> (" + area.X1 + ", " + area.Y1 + ")";
                e.FormattingApplied = true;
            }
        }

        private static ImageC GetPerspectivePreview(ImageC input, Vector2 texCoord01, Vector2 texCoord00, Vector2 texCoord10, Vector2 texCoord11, Size size)
        {
            ImageC output = ImageC.CreateNew(size.Width, size.Height);

            // Project the chosen texture onto the rectangular preview ...
            float xTexCoordFactor = 1.0f / size.Width;
            float yTexCoordFactor = 1.0f / size.Width;
            Vector2 max = input.Size - new Vector2(1.0f);

            for (int y = 0; y < size.Height; ++y)
                for (int x = 0; x < size.Width; ++x)
                {
                    float outputTexCoordX = (x + 0.5f) * xTexCoordFactor;
                    float outputTexCoordY = (y + 0.5f) * yTexCoordFactor;
                    Vector2 inputTexCoord = texCoord00 * ((1.0f - outputTexCoordX) * (1.0f - outputTexCoordY)) +
                                            texCoord01 * ((1.0f - outputTexCoordX) * outputTexCoordY) + (
                            texCoord10 * (outputTexCoordX * (1.0f - outputTexCoordY)) +
                            texCoord11 * (outputTexCoordX * outputTexCoordY)
                        );

                    // Bilinear filter from the input
                    inputTexCoord -= new Vector2(0.5f); // Offset of texture coordinate from texel midpoint
                    inputTexCoord = Vector2.Min(Vector2.Max(inputTexCoord, new Vector2()), max); // Clamp into available texture space

                    int firstX = (int)inputTexCoord.X;
                    int firstY = (int)inputTexCoord.Y;
                    int secondX = Math.Min(firstX + 1, input.Width - 1);
                    int secondY = Math.Min(firstY + 1, input.Height - 1);
                    float secondFactorX = inputTexCoord.X - firstX;
                    float secondFactorY = inputTexCoord.Y - firstY;

                    Vector4 foregroundPixel =
                        (Vector4)input.GetPixel(secondX, secondY) * (secondFactorX * secondFactorY) +
                        (Vector4)input.GetPixel(secondX, firstY) * (secondFactorX * (1.0f - secondFactorY)) + (
                        (Vector4)input.GetPixel(firstX, secondY) * ((1.0f - secondFactorX) * secondFactorY) +
                        (Vector4)input.GetPixel(firstX, firstY) * ((1.0f - secondFactorX) * (1.0f - secondFactorY)));
                    ColorC backgroundPixel = transparentBackground.GetPixel(x % transparentBackground.Size.X, y % transparentBackground.Size.Y);
                    output.SetPixel(x, y, (ColorC)ColorC.Mix(backgroundPixel, foregroundPixel));
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
                var frame = (AnimatedTextureFrame)texturesDataGridView.SelectedRows[0].DataBoundItem;
                var textureToShow = new TextureArea
                {
                    Texture = frame.Texture,
                    TexCoord0 = frame.TexCoord0,
                    TexCoord1 = frame.TexCoord1,
                    TexCoord2 = frame.TexCoord2,
                    TexCoord3 = frame.TexCoord3
                };

                if (textureMap.SelectedTexture != textureToShow)
                {
                    textureMap.ShowTexture(textureToShow);
                    _editor.SelectedTexture = textureToShow;
                }
            }
        }

        public class PanelTextureMapForAnimations : Controls.PanelTextureMap
        {
            protected override float MaxTextureSize => float.PositiveInfinity;
            protected override bool DrawTriangle => false;

            private static readonly Pen outlinePen = new Pen(Color.FromArgb(80, 192, 192, 192), 2);
            private static readonly Pen activeOutlinePen = new Pen(Color.FromArgb(200, 238, 82, 238), 2);
            private static readonly Brush textBrush = new SolidBrush(Color.Violet);
            private static readonly Brush textShadowBrush = new SolidBrush(Color.Black);
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
                    if (frame.Texture == VisibleTexture)
                    {
                        PointF[] edges = new[]
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
                            string counterString = i + 1 + "/" + set.Frames.Count;
                            SizeF textSize = e.Graphics.MeasureString(counterString, textFont);
                            RectangleF textArea = RectangleF.FromLTRB(upperLeft.X, upperLeft.Y, lowerRight.X, lowerRight.Y);

                            if (textArea.Width > textSize.Width && textArea.Height > textSize.Height)
                            {
                                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                                e.Graphics.DrawString(counterString, textFont, textShadowBrush, textArea, textFormat);
                                textArea.X -= 1;
                                textArea.Y -= 1;
                                e.Graphics.DrawString(counterString, textFont, textBrush, textArea, textFormat);
                            }
                        }
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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
            if (_isNg)
                OnEffectChanged();
        }

        private void OnEffectChanged()
        {
            var effect = (AnimatedTextureAnimationType)comboEffect.SelectedItem;
            switch (effect)
            {
                case AnimatedTextureAnimationType.Frames:
                    comboFps.Visible = false;
                    numericUpDownFPS.Visible = true;
                    numericUpDownFPS.Enabled = true;
                    comboUvRotate.Enabled = false;
                    break;

                case AnimatedTextureAnimationType.PFrames:
                    comboFps.Visible = false;
                    numericUpDownFPS.Visible = true;
                    numericUpDownFPS.Enabled = false;
                    comboUvRotate.Enabled = false;
                    break;

                case AnimatedTextureAnimationType.UVRotate:
                case AnimatedTextureAnimationType.HalfRotate:
                case AnimatedTextureAnimationType.RiverRotate:
                    comboFps.Visible = true;
                    numericUpDownFPS.Visible = false;
                    comboUvRotate.Enabled = true;

                    comboFps.SelectedIndex = 0;
                    comboUvRotate.SelectedIndex = 64;
                    break;
                default:
                    throw new NotSupportedException("Unsupported texture animation type encountered.");
            }
        }

        private void comboEffect_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (selectedSet == null)
                return;
            selectedSet.AnimationType = (AnimatedTextureAnimationType)comboEffect.SelectedItem;
            _editor.AnimatedTexturesChange();
        }

        private void comboFps_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (selectedSet == null)
                return;
            selectedSet.Fps = ((NgAnimatedTextureSettingPair)comboFps.SelectedItem).Key;
            _editor.AnimatedTexturesChange();
        }

        private void numericUpDownFPS_ValueChanged(object sender, EventArgs e)
        {
            var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (selectedSet == null)
                return;
            selectedSet.Fps = (float)numericUpDownFPS.Value;
            _editor.AnimatedTexturesChange();
        }

        private void comboUvRotate_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var selectedSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;
            if (selectedSet == null)
                return;
            selectedSet.UvRotate = (sbyte)((NgAnimatedTextureSettingPair)comboUvRotate.SelectedItem).Key;
            _editor.AnimatedTexturesChange();
        }

        private void comboCurrentTexture_SelectedValueChanged(object sender, EventArgs e)
        {
            if (textureMap.VisibleTexture != comboCurrentTexture.SelectedItem)
                textureMap.ResetVisibleTexture(comboCurrentTexture.SelectedItem as LevelTexture);
        }

        private void butEditSetName_Click(object sender, EventArgs e)
        {
            var currentSet = comboAnimatedTextureSets.SelectedItem as AnimatedTextureSet;

            if (currentSet == null)
                return;

            using (var form = new FormInputBox("Edit set name", "Insert the name of this animation set:", currentSet.Name))
            {
                if (form.ShowDialog(this) == DialogResult.Cancel)
                    return;

                currentSet.Name = form.Result;
                _editor.AnimatedTexturesChange();
            }
        }

        private void butGenerateProcAnim_Click(object sender, EventArgs e)
        {
            GenerateProceduralAnimation((ProceduralAnimationType)comboProcPresets.SelectedIndex, (int)numFrames.Value, (float)numStrength.Value / 100.0f, cbSmooth.Checked, cbLoop.Checked, AnimGenerationType.New);
        }

        private void butCloneProcAnim_Click(object sender, EventArgs e)
        {
            GenerateProceduralAnimation((ProceduralAnimationType)comboProcPresets.SelectedIndex, (int)numFrames.Value, (float)numStrength.Value / 100.0f, cbSmooth.Checked, cbLoop.Checked, AnimGenerationType.Clone);
        }

        private void butMergeProcAnim_Click(object sender, EventArgs e)
        {
            GenerateProceduralAnimation((ProceduralAnimationType)comboProcPresets.SelectedIndex, (int)numFrames.Value, (float)numStrength.Value / 100.0f, cbSmooth.Checked, cbLoop.Checked, AnimGenerationType.Merge);
        }

        private void butReplaceProcAnim_Click(object sender, EventArgs e)
        {
            GenerateProceduralAnimation((ProceduralAnimationType)comboProcPresets.SelectedIndex, (int)numFrames.Value, (float)numStrength.Value / 100.0f, cbSmooth.Checked, cbLoop.Checked, AnimGenerationType.Replace);
        }

        private void butAddProcAnim_Click(object sender, EventArgs e)
        {
            GenerateProceduralAnimation((ProceduralAnimationType)comboProcPresets.SelectedIndex, (int)numFrames.Value, (float)numStrength.Value / 100.0f, cbSmooth.Checked, cbLoop.Checked, AnimGenerationType.AddFrames);
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            _editor.Level.Settings.AnimatedTextureSets.Clear();
            foreach (var set in _backupSets)
                _editor.Level.Settings.AnimatedTextureSets.Add(set);
            _editor.AnimatedTexturesChange();
            Close();
        }
    }
}
