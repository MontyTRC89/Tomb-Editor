using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Forms;
using TombLib.GeometryIO;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace WadTool
{
    public partial class FormAnimationEditor : DarkForm
    {
        private enum SearchType
        {
            None,
            StateID,
            AnimNumber,
            Name,
            StateName
        }

        private enum SoundPreviewType
        {
            Land,
            LandWithMaterial,
            Water
        }

        private bool _saved = false;
        private bool Saved
        {
            get { return _saved; }
            set
            {
                if (value == _saved) return;
                _saved = value;
                butTbSaveAllChanges.Enabled = !_saved;
                saveChangesToolStripMenuItem.Enabled = !_saved;
                Text = "Animation editor - " + _editor.Moveable.Id.ToString(_editor.Wad.SuggestedGameVersion) + (!_saved ? "*" : "");
            }
        }

        private AnimationEditor _editor;  // Editor
        private DeviceManager _deviceManager; // Renderer

        // Player
        private Timer _timerPlayAnimation;
        private int _frameCount;
        private bool _previewSounds;
        private int _overallPlaybackCount = _materialIndexSwitchInterval; // To reset 1st time on playback
        private int _currentMaterialIndex;
        private SoundPreviewType _soundPreviewType = SoundPreviewType.Land;

        private static readonly int _materialIndexSwitchInterval = 30 * 3; // 3 seconds, 30 game frames

        // Chained playback vars
        private bool _chainedPlayback;
        private int _chainedPlaybackInitialAnim;
        private int _chainedPlaybackInitialCursorPos;
        private VectorInt2 _chainedPlaybackInitialSelection;

        // Info
        private readonly PopUpInfo popup = new PopUpInfo();

        // Helpers
        private static string GetAnimLabel(int index, AnimationNode anim) => "(" + anim.Index + ") " + anim.WadAnimation.Name;

        public FormAnimationEditor(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadMoveableId id)
        {
            InitializeComponent();

            _editor = new AnimationEditor(tool, wad, id);
            _deviceManager = deviceManager;

            panelRendering.Configuration = _editor.Tool.Configuration;

            // Update UI
            UpdateReferenceLevelControls();
            PopulateComboStateID();

            // Hacks for textbox...
            tbStateId.AutoSize = false;
            tbStateId.Height = 23;
            tbStateId.MouseWheel += tbStateID_MouseWheel;

            // Initialize playback
            _timerPlayAnimation = new Timer() { Interval = 30 };
            _timerPlayAnimation.Tick += timerPlayAnimation_Tick;
            _previewSounds = false;

            // Add custom event handler for direct editing of animcommands
            timeline.AnimCommandDoubleClick += new EventHandler<WadAnimCommand>(timeline_AnimCommandDoubleClick);

            // Initialize the panel
            WadMoveable skin;
            var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(_editor.Wad.SuggestedGameVersion, id.TypeId));

            if (_editor.Wad.Moveables.ContainsKey(skinId))
                skin = _editor.Wad.Moveables[skinId];
            else
                skin = _editor.Wad.Moveables[id];

            panelRendering.InitializeRendering(_editor, _deviceManager, skin);

            // Load skeleton in combobox
            comboBoneList.ComboBox.Items.Add("(select mesh)");
            foreach (var bone in panelRendering.Model.Bones)
                comboBoneList.ComboBox.Items.Add(bone.Name);
            comboBoneList.ComboBox.SelectedIndex = 0;

            RebuildAnimationsList();

            if (_editor.Animations.Count != 0)
            {
                SelectAnimation(_editor.Animations[0]);
                SelectFrame(0);
            }

            _editor.Tool.EditorEventRaised += Tool_EditorEventRaised;

            Saved = true;
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.AnimationEditorMeshSelectedEvent)
            {
                var e = obj as WadToolClass.AnimationEditorMeshSelectedEvent;
                if (e != null)
                    comboBoneList.ComboBox.SelectedIndex = e.Model.Meshes.IndexOf(e.Mesh) + 1;
                else
                    comboBoneList.ComboBox.SelectedIndex = 0;
            }

            if (obj is WadToolClass.AnimationEditorAnimationChangedEvent)
            {
                var e = obj as WadToolClass.AnimationEditorAnimationChangedEvent;
                if (e != null)
                {
                    var anim = lstAnimations.Items.FirstOrDefault(item => ((AnimationNode)item.Tag).Index == e.Animation.Index);
                    if (anim != null)
                    {
                        var index = lstAnimations.Items.IndexOf(anim);
                        var node = (AnimationNode)anim.Tag;

                        // Back-up cursor position
                        int cursorPos = -1;
                        if ((e.Animation.DirectXAnimation.KeyFrames.Count == node.DirectXAnimation.KeyFrames.Count) &&
                            (e.Animation.WadAnimation.FrameRate == node.WadAnimation.FrameRate))
                            cursorPos = timeline.Value;

                        lstAnimations.Items[index].Tag = e.Animation;
                        lstAnimations.Items[index].Text = GetAnimLabel(((AnimationNode)anim.Tag).Index, ((AnimationNode)anim.Tag));
                        lstAnimations.SelectItem(index);

                        // Restore cursor position
                        if (cursorPos != -1)
                            timeline.Value = cursorPos;
                        else
                            timeline.Value = timeline.Minimum;

                        Saved = false;
                    }
                }
            }

            if (obj is WadToolClass.AnimationEditorGizmoPickedEvent)
            {
                Saved = false;
            }

            if (obj is WadToolClass.ReferenceLevelChangedEvent)
            {
                UpdateReferenceLevelControls();
            }

            if (obj is WadToolClass.UndoStackChangedEvent)
            {
                var stackEvent = (WadToolClass.UndoStackChangedEvent)obj;
                butTbUndo.Enabled = stackEvent.UndoPossible;
                butTbRedo.Enabled = stackEvent.RedoPossible;
                undoToolStripMenuItem.Enabled = stackEvent.UndoPossible;
                redoToolStripMenuItem.Enabled = stackEvent.RedoPossible;
                Saved = false;
            }

            if (obj is WadToolClass.MessageEvent)
            {
                var msg = (WadToolClass.MessageEvent)obj;
                PopUpInfo.Show(popup, this, panelRendering, msg.Message, msg.Type);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                _editor.Tool.EditorEventRaised -= Tool_EditorEventRaised;
            }
            base.Dispose(disposing);
        }

        private void UpdateReferenceLevelControls()
        {
            panelRendering.Level = _editor.Tool.ReferenceLevel;
            panelRendering.Invalidate();

            comboRoomList.ComboBox.Items.Clear();

            if (_editor.Tool.ReferenceLevel != null)
            {
                // Load rooms into the combo box
                comboRoomList.Enabled = true;
                comboRoomList.ComboBox.Items.Add("(select room)");

                foreach (var room in _editor.Tool.ReferenceLevel.Rooms)
                    if (room != null)
                        comboRoomList.ComboBox.Items.Add(room);

                // Enable sound transport
                if (_editor.Tool.ReferenceLevel.Settings.SoundSystem == SoundSystem.Xml)
                {
                    butTransportSound.Enabled = true;
                    butTransportLandWater.Enabled = true;
                }
            }
            else
            {
                comboRoomList.Enabled = false;

                // Disable sound transport
                butTransportSound.Enabled = false;
                butTransportLandWater.Enabled = false;
            }
        }

        private void RebuildAnimationsList()
        {
            lstAnimations.Items.Clear();

            // Try to filter by request
            int index = 0;
            int searchNumber = -1;
            var searchString = tbSearchAnimation.Text.Trim().ToLower();
            string finalSearchString = "";
            SearchType searchType = SearchType.None;

            if (!string.IsNullOrEmpty(searchString))
            {
                // Find possible tokens
                int tokenEndIndex = searchString.IndexOf(":");

                if (tokenEndIndex >= 0)
                {
                    string possibleToken = searchString.Substring(0, tokenEndIndex).ToLower();
                    if (!string.IsNullOrEmpty(possibleToken))
                    {
                        switch (possibleToken)
                        {
                            case "num":
                            case "n":
                            case "number":
                            case "anim":
                            case "animation":
                            case "a":
                                searchType = SearchType.AnimNumber;
                                break;

                            case "state":
                            case "s":
                            case "stateid":
                                searchType = SearchType.StateID;
                                break;

                            case "name":
                                searchType = SearchType.Name;
                                break;
                        }

                        // Force name search type if there's not a number after numerical token
                        if (!int.TryParse(searchString.Substring(tokenEndIndex + 1), out searchNumber) && searchType != SearchType.Name)
                        {
                            finalSearchString = searchString.Substring(tokenEndIndex + 1);
                            if (searchType == SearchType.StateID) searchType = SearchType.StateName;
                            else searchType = SearchType.Name;
                        }
                    }
                }
                else
                {
                    if (!int.TryParse(searchString, out searchNumber))
                    {
                        // If no numerical, always search as by name
                        if (searchType == SearchType.StateID) searchType = SearchType.StateName;
                        else searchType = SearchType.Name;
                        finalSearchString = searchString;
                    }
                    else
                        searchType = SearchType.StateID; // Otherwise prioritize state id
                }
            }

            // Scan for state ID by known names which are in combobox by this time
            if (searchType == SearchType.StateName)
            {
                var names = cmbStateID.Items.Cast<string>().ToList();
                var firstEntry = names.IndexOf(name => name.ToLower().Contains(finalSearchString));

                if (firstEntry >= 0)
                {
                    searchNumber = firstEntry;
                    searchType = SearchType.StateID; // Switch back to state ID search, as we found index.
                }
            }

            // Filter regarding request
            for (int i = 0; i < _editor.Animations.Count; i++)
            {
                switch (searchType)
                {
                    case SearchType.StateID:
                        if (searchNumber >= 0 && _editor.Animations[i].WadAnimation.StateId != searchNumber)
                            continue;
                        break;
                    case SearchType.AnimNumber:
                        if (searchNumber >= 0 && i != searchNumber)
                            continue;
                        break;
                    case SearchType.Name:
                        if (!_editor.Animations[i].WadAnimation.Name.ToLower().Contains(finalSearchString))
                            continue;
                        break;
                    default:
                        break;
                }

                var item = new DarkListItem(GetAnimLabel(index++, _editor.Animations[i]));
                item.Tag = _editor.Animations[i];
                lstAnimations.Items.Add(item);
            }

            if (lstAnimations.Items.Count != 0)
                lstAnimations.SelectItem(0);

            lstAnimations.EnsureVisible();
        }

        private void SelectAnimation(AnimationNode node)
        {
            _editor.CurrentAnim = node;

            tbName.Text = node.WadAnimation.Name;
            nudFramerate.Value = node.WadAnimation.FrameRate;
            nudNextAnim.Value = node.WadAnimation.NextAnimation;
            nudNextFrame.Value = node.WadAnimation.NextFrame;
            tbStartVertVel.Text = node.WadAnimation.StartVelocity.ToString();
            tbEndVertVel.Text = node.WadAnimation.EndVelocity.ToString();
            tbStartHorVel.Text = node.WadAnimation.StartLateralVelocity.ToString();
            tbEndHorVel.Text = node.WadAnimation.EndLateralVelocity.ToString();

            tbStateId.Text = node.WadAnimation.StateId.ToString();
            UpdateStateChange();

            timeline.Animation = node;
            panelRendering.SelectedMesh = null;

            if (node.DirectXAnimation.KeyFrames.Count > 0)
                panelRendering.Model.BuildAnimationPose(node.DirectXAnimation.KeyFrames[0]);

            OnKeyframesListChanged();
            timeline.Value = 0;
            timeline.ResetSelection();

            panelRendering.Invalidate();

        }

        private void UpdateSelection()
        {
            _editor.Selection = timeline.Selection;
            UpdateStatusLabel();
        }

        private void SelectFrame(int frameIndex)
        {
            if (!_editor.ValidAnimationAndFrames) return;

            if (frameIndex >= _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count)
                frameIndex = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;

            _editor.CurrentFrameIndex = frameIndex;

            var keyFrame = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex];
            panelRendering.Model.BuildAnimationPose(keyFrame);
            panelRendering.Invalidate();

            // Update GUI
            OnKeyframesListChanged();

            tbCollisionBoxMinX.Text = keyFrame.BoundingBox.Minimum.X.ToString();
            tbCollisionBoxMinY.Text = keyFrame.BoundingBox.Minimum.Y.ToString();
            tbCollisionBoxMinZ.Text = keyFrame.BoundingBox.Minimum.Z.ToString();
            tbCollisionBoxMaxX.Text = keyFrame.BoundingBox.Maximum.X.ToString();
            tbCollisionBoxMaxY.Text = keyFrame.BoundingBox.Maximum.Y.ToString();
            tbCollisionBoxMaxZ.Text = keyFrame.BoundingBox.Maximum.Z.ToString();
        }

        private void OnKeyframesListChanged()
        {
            if (_editor.ValidAnimationAndFrames)
            {
                _frameCount = timeline.Value * _editor.CurrentAnim.WadAnimation.FrameRate;
                timeline.Minimum = 0;
                timeline.Maximum = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;
                UpdateStatusLabel();
            }
            else
                statusFrame.Text = "";
        }

        private void CalculateAnimationBoundingBox(bool clear = false)
        {
            if (!_editor.ValidAnimationAndFrames) return;
            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            int startFrame = timeline.Value;
            for (int i = 0; i < _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count; i++)
            {
                timeline.Value = i;
                CalculateKeyframeBoundingBox(i, false, clear);
            }
            timeline.Value = (startFrame);
        }

        private void ClearAnimationBoundingBox(bool confirm = false)
        {
            if (!_editor.ValidAnimationAndFrames) return;

            if (confirm && (DarkMessageBox.Show(this, "Do you really want to delete the collision box for each frame of animation '" + _editor.CurrentAnim.WadAnimation.Name + "'?",
                           "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No))
                return;

            CalculateAnimationBoundingBox(true);
        }

        private void CalculateKeyframeBoundingBox(int index, bool undo, bool clear)
        {
            if (!_editor.ValidAnimationAndFrames) return;
            if (undo) _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            var keyFrame = _editor.CurrentAnim.DirectXAnimation.KeyFrames[index];

            if (clear)
                keyFrame.BoundingBox = new BoundingBox();
            else
                keyFrame.CalculateBoundingBox(panelRendering.Model, panelRendering.Skin);

            panelRendering.Invalidate();

            tbCollisionBoxMinX.Text = keyFrame.BoundingBox.Minimum.X.ToString();
            tbCollisionBoxMinY.Text = keyFrame.BoundingBox.Minimum.Y.ToString();
            tbCollisionBoxMinZ.Text = keyFrame.BoundingBox.Minimum.Z.ToString();
            tbCollisionBoxMaxX.Text = keyFrame.BoundingBox.Maximum.X.ToString();
            tbCollisionBoxMaxY.Text = keyFrame.BoundingBox.Maximum.Y.ToString();
            tbCollisionBoxMaxZ.Text = keyFrame.BoundingBox.Maximum.Z.ToString();

            Saved = false;
        }

        private void ClearFrameBoundingBox(bool confirm = false)
        {
            if (!_editor.ValidAnimationAndFrames) return;

            if (confirm && (DarkMessageBox.Show(this, "Do you really want to delete the collision box for the current keyframe?",
                                        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No))
                return;

            CalculateKeyframeBoundingBox(timeline.Value, true, true);
        }

        private void AddNewAnimation()
        {
            var wadAnimation = new WadAnimation();
            wadAnimation.FrameRate = 1;
            wadAnimation.Name = "New Animation " + _editor.Animations.Count;

            var keyFrame = new WadKeyFrame();
            foreach (var bone in _editor.Moveable.Bones)
                keyFrame.Angles.Add(new WadKeyFrameRotation());
            wadAnimation.KeyFrames.Add(keyFrame);

            var dxAnimation = Animation.FromWad2(_editor.Moveable.Bones, wadAnimation);
            var node = new AnimationNode(wadAnimation, dxAnimation, _editor.Animations.Count);

            var item = new DarkListItem(GetAnimLabel(_editor.Animations.Count, node));
            item.Tag = node;

            _editor.Animations.Add(node);
            lstAnimations.Items.Add(item);
            lstAnimations.SelectItem(lstAnimations.Items.Count - 1);
            lstAnimations.EnsureVisible();

            tbName.Focus();

            Saved = false;
        }

        private void DeleteAnimation()
        {
            if (_editor.CurrentAnim == null) return;

            if (DarkMessageBox.Show(this, "Do you really want to delete '" + _editor.CurrentAnim.WadAnimation.Name + "'?",
                                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            int currentIndex = _editor.Animations.IndexOf(_editor.CurrentAnim);

            // Update all references
            for (int i = 0; i < _editor.Animations.Count; i++)
            {
                // Ignore the animation I'm deleting
                if (i == currentIndex)
                    continue;

                var animation = _editor.Animations[i];

                // Update NextAnimation
                if (animation.WadAnimation.NextAnimation > currentIndex)
                    animation.WadAnimation.NextAnimation--;

                // Update state changes
                foreach (var stateChange in animation.WadAnimation.StateChanges)
                    foreach (var dispatch in stateChange.Dispatches)
                        if (dispatch.NextAnimation > currentIndex)
                            dispatch.NextAnimation--;
            }

            // Remove the animation
            _editor.Animations.Remove(_editor.CurrentAnim);

            // Update GUI
            lstAnimations.Items.RemoveAt(lstAnimations.SelectedIndices[0]);
            if (lstAnimations.Items.Count != 0)
            {
                if (lstAnimations.SelectedIndices[0] != -1 && lstAnimations.SelectedItem != null)
                    SelectAnimation(lstAnimations.SelectedItem.Tag as AnimationNode);
                else
                {
                    lstAnimations.SelectItem(0);
                    lstAnimations.EnsureVisible();
                    SelectAnimation(_editor.Animations[0]);
                }
            }
            else
            {
                _editor.CurrentAnim = null;
                timeline.Animation = null;
            }

            panelRendering.Invalidate();
            timeline.Invalidate();

            Saved = false;
        }

        private void AddNewFrame(int index, bool undo)
        {
            if (_editor.CurrentAnim == null) return;
            if (undo) _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            var keyFrame = new KeyFrame();
            foreach (var bone in _editor.Moveable.Bones)
            {
                keyFrame.Rotations.Add(Vector3.Zero);
                keyFrame.Quaternions.Add(Quaternion.Identity);
                keyFrame.Translations.Add(bone.Translation);
                keyFrame.TranslationsMatrices.Add(Matrix4x4.CreateTranslation(bone.Translation));
            }

            if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == 0)
                index = 0;

            _editor.CurrentAnim.DirectXAnimation.KeyFrames.Insert(index, keyFrame);
            OnKeyframesListChanged();
            Saved = false;
        }

        private void InsertMultipleFrames()
        {
            if (_editor.CurrentAnim == null) return;

            using (var form = new FormInputBox("Add frames", "How many frames to add?", "1"))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

                    int framesCount = 0;
                    if (!int.TryParse(form.Result, out framesCount) || framesCount <= 0)
                    {
                        popup.ShowError(panelRendering, "You must insert a number greater than 1");
                        return;
                    }

                    // Add frames
                    for (int i = 0; i < framesCount; i++)
                        AddNewFrame(timeline.Value + 1, false);
                }
            }
        }

        private void DeleteFrames(IWin32Window owner, bool undo, bool updateGUI) // No owner = no warnings!
        {
            if (!ValidAndSelected(owner != null)) return;

            if (owner != null &&
                DarkMessageBox.Show(this, "Do you really want to delete frame" +
                (timeline.SelectionSize == 1 ? " " + timeline.Value : "s " + timeline.Selection.X + "-" + timeline.Selection.Y) + "?",
                                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            if (undo) _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            int selectionStart = timeline.Selection.X; // Save last index
            int selectionEnd = timeline.Selection.Y;

            // Save cursor position to restore later. Only do it if cursor is outside selection.
            int cursorPos = (timeline.Value < timeline.Selection.X || timeline.Value > timeline.Selection.Y) ? timeline.Value : -1;

            // Remove the frames
            _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveRange(timeline.Selection.X, timeline.SelectionSize);

            Saved = false;
            if (!updateGUI) return;

            // Update GUI
            timeline.ResetSelection();
            OnKeyframesListChanged();
            if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count != 0)
            {
                int insertEnd = selectionStart + _editor.ClipboardKeyFrames.Count - 1;

                if (cursorPos != -1)
                {
                    if (cursorPos < selectionStart)
                        timeline.Value = cursorPos;
                    else
                        timeline.Value = selectionStart + (cursorPos - selectionEnd - 1);
                }
                else
                    timeline.Value = selectionStart;
            }
            else
            {
                timeline.Value = 0;
                statusFrame.Text = "";
            }
            panelRendering.Invalidate();
        }

        private void CutFrames()
        {
            if (!ValidAndSelected()) return;

            CopyFrames(false);
            DeleteFrames(null, true, true);
        }

        private void CopyFrames(bool updateGUI = true)
        {
            if (!ValidAndSelected()) return;

            _editor.ClipboardKeyFrames.Clear();
            for (int i = timeline.Selection.X; i <= timeline.Selection.Y; i++)
                _editor.ClipboardKeyFrames.Add(_editor.CurrentAnim.DirectXAnimation.KeyFrames[i].Clone());

            if (updateGUI)
                timeline.Highlight(timeline.Selection.X, timeline.Selection.Y);
        }

        private void PasteFrames()
        {
            if (_editor.CurrentAnim == null) return;
            if (_editor.ClipboardKeyFrames == null || _editor.ClipboardKeyFrames.Count <= 0)
            {
                popup.ShowWarning(panelRendering, "Nothing to paste!");
                return;
            }

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            int startIndex = timeline.SelectionIsEmpty ? timeline.Value : timeline.Selection.X; // Save last index
            int endIndex = timeline.SelectionIsEmpty ? timeline.Value : timeline.Selection.Y;

            // Save cursor position to restore later. Only do it if cursor is outside selection.
            int cursorPos = (timeline.SelectionIsEmpty || timeline.Value < timeline.Selection.X || timeline.Value > timeline.Selection.Y) ? timeline.Value : -1;

            // Replace if there is a selection.
            if (!timeline.SelectionIsEmpty)
                DeleteFrames(null, false, false);

            _editor.CurrentAnim.DirectXAnimation.KeyFrames.InsertRange(startIndex, _editor.ClipboardKeyFrames);
            OnKeyframesListChanged();

            int insertEnd = startIndex + _editor.ClipboardKeyFrames.Count - 1;
            if (cursorPos != -1)
            {
                if (cursorPos < startIndex)
                    timeline.Value = cursorPos;
                else
                    timeline.Value = insertEnd + (cursorPos - endIndex) + (timeline.SelectionIsEmpty ? 1 : 0);
            }
            else
                timeline.Value = insertEnd;

            timeline.Highlight(startIndex, insertEnd);
            panelRendering.Invalidate();
            Saved = false;
        }

        private void CutAnimation()
        {
            if (_editor.CurrentAnim == null)
            {
                popup.ShowWarning(panelRendering, "No animation to cut!");
                return;
            }

            _editor.ClipboardNode = _editor.CurrentAnim;
            DeleteAnimation();
        }

        private void CopyAnimation()
        {
            if (_editor.CurrentAnim == null)
            {
                popup.ShowWarning(panelRendering, "No animation to copy!");
                return;
            }

            _editor.ClipboardNode = _editor.CurrentAnim.Clone();
        }

        private void PasteAnimation()
        {
            if (_editor.ClipboardNode == null || _editor.CurrentAnim == null)
            {
                popup.ShowWarning(panelRendering, "No animation to paste!");
                return;
            }

            int animationIndex = _editor.Animations.IndexOf(_editor.CurrentAnim) + 1;
            _editor.Animations.Insert(animationIndex, _editor.ClipboardNode);
            _editor.ClipboardNode.DirectXAnimation.Name += " - Copy";
            _editor.ClipboardNode.WadAnimation.Name += " - Copy";
            RebuildAnimationsList();
            SelectAnimation(_editor.Animations[animationIndex]);
            Saved = false;
        }

        private void ReplaceAnimation()
        {
            if (_editor.ClipboardNode == null || _editor.CurrentAnim == null)
            {
                popup.ShowWarning(panelRendering, "No animation to replace!");
                return;
            }

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            int animationIndex = _editor.Animations.IndexOf(_editor.CurrentAnim);
            _editor.Animations[animationIndex] = _editor.ClipboardNode;
            _editor.ClipboardNode.DirectXAnimation.Name += " - Copy";
            _editor.ClipboardNode.WadAnimation.Name += " - Copy";
            RebuildAnimationsList();
            SelectAnimation(_editor.Animations[animationIndex]);
            Saved = false;
        }

        private void SplitAnimation()
        {
            if (_editor.CurrentAnim == null)
            {
                popup.ShowWarning(panelRendering, "No animation to split!");
                return;
            }

            // I need at least 1 frame in the middle
            if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count < 3)
            {
                popup.ShowError(panelRendering, "You must have at least 3 frames for splitting the animation");
                return;
            }

            // Check if we have selected the first or the last frame
            int numFrames = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
            if (timeline.Value == 0 || timeline.Value == numFrames - 1)
            {
                popup.ShowError(panelRendering, "You can't set the first or the last frame for splitting the animation");
                return;
            }
            var newWadAnimation = _editor.CurrentAnim.WadAnimation.Clone();
            var newDirectXAnimation = _editor.CurrentAnim.DirectXAnimation.Clone();

            int numFrames1 = timeline.Value;
            int numFrames2 = numFrames - numFrames1;

            // Remove frames from the two animations
            _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveRange(timeline.Value + 1, numFrames2 - 1);
            newDirectXAnimation.KeyFrames.RemoveRange(0, timeline.Value);

            // Add the new animation at the bottom of the list
            newWadAnimation.Name += " - splitted";
            _editor.Animations.Add(new AnimationNode(newWadAnimation, newDirectXAnimation, _editor.Animations.Count));

            // Update the GUI
            RebuildAnimationsList();
            SelectAnimation(_editor.CurrentAnim);

            Saved = false;
        }

        private void UpdateStateChange(bool getFromCombo = false)
        {
            if (_editor.CurrentAnim == null) return;

            bool decodeFailed = false;
            ushort oldValue = _editor.CurrentAnim.WadAnimation.StateId;
            ushort newValue = ushort.MaxValue; // Let's hope nobody will ever use state ID 65535...

            // ID decoding from textbox is not needed in case textbox field was forced manually (eg from combobox).
            if (getFromCombo)
                tbStateId.Text = (string)cmbStateID.SelectedItem;

            // Try to identify if there was a string input in textbox.
            if (getFromCombo || !ushort.TryParse(tbStateId.Text, out newValue))
            {
                string searchString = tbStateId.Text.Trim();
                int possibleID = -1;

                for (int i = 0; i < 2; i++)
                {
                    possibleID = TrCatalog.TryToGetStateID(_editor.Wad.SuggestedGameVersion, _editor.Moveable.Id.TypeId, searchString);
                    if (possibleID >= 0) break;
                    // Try to do one more pass with spaces replaced by underlines
                    searchString = searchString.Replace(' ', '_');
                }

                // String input. Try to find appropriate name in trcatalog.
                if (possibleID >= 0)
                    newValue = (ushort)possibleID; // Name found, use found state ID
                else
                {
                    popup.ShowError(panelRendering, "State ID with given name wasn't found.");
                    decodeFailed = true;
                    newValue = oldValue; // Decoding failed, reset state ID to old value
                }
            }

            // Restore state name to textbox, if possible.
            if (newValue != ushort.MaxValue)
            {
                var possibleName = TrCatalog.GetStateName(_editor.Wad.SuggestedGameVersion, _editor.Moveable.Id.TypeId, newValue);

                // Put name into textbox, if found among state numbers. Otherwise, use raw number.
                if (cmbStateID.Items.Contains(possibleName))
                    tbStateId.Text = possibleName;
                else
                    tbStateId.Text = newValue.ToString();
            }

            // Add legacy numerical ID as a tooltip
            if (!decodeFailed)
                toolTip1.SetToolTip(tbStateId, "State ID = " + newValue);

            // Don't update if not changed or parse failed
            if (oldValue == newValue || newValue == ushort.MaxValue)
                return;

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
            _editor.CurrentAnim.WadAnimation.StateId = newValue;
            Saved = false;
        }

        private void PopulateComboStateID()
        {
            cmbStateID.Items.Clear();

            for (uint i = 0; i < 256; i++) // Max state value in TR5 was 137 but just in case...
            {
                string name = TrCatalog.GetStateName(_editor.Wad.SuggestedGameVersion, _editor.Moveable.Id.TypeId, i);
                if (name.Contains("Unknown"))
                    continue;
                else
                    cmbStateID.Items.Add(name);
            }
        }

        private void UpdateAnimationParameter(Control control)
        {
            if (_editor.CurrentAnim == null) return;

            float result = 0;
            if (control is DarkTextBox)
            {
                string toParse = ((DarkTextBox)control).Text.Replace(",", ".");
                if (!float.TryParse(toParse, out result))
                    result = 0;
            }
            else if (control is DarkNumericUpDown)
                result = (float)((DarkNumericUpDown)control).Value;

            float oldValue = 0;
            bool roundToByte = false;
            bool roundToShort = false;

            // Get actual old value
            switch (control.Name)
            {
                case "nudNextAnim":
                    oldValue = _editor.CurrentAnim.WadAnimation.NextAnimation;
                    roundToShort = true;
                    break;
                case "nudNextFrame":
                    oldValue = _editor.CurrentAnim.WadAnimation.NextFrame;
                    roundToShort = true;
                    break;
                case "nudFramerate":
                    oldValue = _editor.CurrentAnim.WadAnimation.FrameRate;
                    roundToByte = true;
                    break;
                case "tbStartVertVel":
                    oldValue = _editor.CurrentAnim.WadAnimation.StartVelocity;
                    break;
                case "tbEndVertVel":
                    oldValue = _editor.CurrentAnim.WadAnimation.EndVelocity;
                    break;
                case "tbStartHorVel":
                    oldValue = _editor.CurrentAnim.WadAnimation.StartLateralVelocity;
                    break;
                case "tbEndHorVel":
                    oldValue = _editor.CurrentAnim.WadAnimation.EndLateralVelocity;
                    break;
            }

            // Clamp if necessary
            if (roundToByte)
                oldValue = (byte)MathC.Clamp(oldValue, 1, 255);
            else if (roundToShort)
                oldValue = (ushort)oldValue;

            // Fix visible values
            if (control is DarkTextBox) ((DarkTextBox)control).Text = result.ToString();
            else if (control is DarkNumericUpDown) ((DarkNumericUpDown)control).Value = (decimal)result;

            // Don't update if not changed
            if (oldValue == result)
                return;

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            // Update actual values
            switch (control.Name)
            {
                case "nudNextAnim":
                    _editor.CurrentAnim.WadAnimation.NextAnimation = (ushort)result;
                    break;
                case "nudNextFrame":
                    _editor.CurrentAnim.WadAnimation.NextFrame = (ushort)result;
                    break;
                case "nudFramerate":
                    _editor.CurrentAnim.WadAnimation.FrameRate = (byte)result;
                    break;
                case "tbStartVertVel":
                    _editor.CurrentAnim.WadAnimation.StartVelocity = result;
                    break;
                case "tbEndVertVel":
                    _editor.CurrentAnim.WadAnimation.EndVelocity = result;
                    break;
                case "tbStartHorVel":
                    _editor.CurrentAnim.WadAnimation.StartLateralVelocity = result;
                    break;
                case "tbEndHorVel":
                    _editor.CurrentAnim.WadAnimation.EndLateralVelocity = result;
                    break;
            }

            Saved = false;
            timeline.Invalidate();
            panelRendering.Invalidate();
        }

        private void ValidateCollisionBox(DarkTextBox textbox)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMaxZ.Text, out result))
                return;

            if (_editor.ValidAnimationAndFrames)
            {
                _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
                var bb = _editor.CurrentAnim.DirectXAnimation.KeyFrames[timeline.Value].BoundingBox;

                switch (textbox.Name)
                {
                    case "tbCollisionBoxMinX":
                        bb.Minimum = new Vector3(result, bb.Minimum.Y, bb.Minimum.Z);
                        break;
                    case "tbCollisionBoxMinY":
                        bb.Minimum = new Vector3(bb.Minimum.X, result, bb.Minimum.Z);
                        break;
                    case "tbCollisionBoxMinZ":
                        bb.Minimum = new Vector3(bb.Minimum.X, bb.Minimum.Y, result);
                        break;
                    case "tbCollisionBoxMaxX":
                        bb.Maximum = new Vector3(result, bb.Maximum.Y, bb.Maximum.Z);
                        break;
                    case "tbCollisionBoxMaxY":
                        bb.Maximum = new Vector3(bb.Maximum.X, result, bb.Maximum.Z);
                        break;
                    case "tbCollisionBoxMaxZ":
                        bb.Maximum = new Vector3(bb.Maximum.X, bb.Maximum.Y, result);
                        break;
                }

                _editor.CurrentAnim.DirectXAnimation.KeyFrames[timeline.Value].BoundingBox = bb;
                panelRendering.Invalidate();
                Saved = false;
            }
        }

        private void InterpolateFrames(int frameIndex1, int frameIndex2, int numFrames, bool updateGUI = true)
        {
            var frame1 = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex1];
            var frame2 = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex2];

            // Cut existing frames between 1 and 2

            if (frameIndex2 - frameIndex1 > 1)
                _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveRange(frameIndex1 + 1, frameIndex2 - frameIndex1 - 1);

            // Now calculate how many frames I must insert
            for (int i = 0; i < numFrames; i++)
            {
                var keyFrame = new KeyFrame();
                foreach (var bone in _editor.Moveable.Bones)
                {
                    keyFrame.Rotations.Add(Vector3.Zero);
                    keyFrame.Quaternions.Add(Quaternion.Identity);
                    keyFrame.Translations.Add(frame1.Translations[0] + bone.Translation);
                    keyFrame.TranslationsMatrices.Add(Matrix4x4.CreateTranslation(frame1.Translations[0] + bone.Translation));
                }
                _editor.CurrentAnim.DirectXAnimation.KeyFrames.Insert(frameIndex1 + 1 + i, keyFrame);
            }

            // Slerp factor
            float k = 1.0f / (numFrames + 1);

            // Now I have the right number of frames and I can do slerp
            for (int i = 0; i < numFrames; i++)
            {
                var keyframe = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex1 + i + 1];

                // Lerp translation of root bone
                keyframe.Translations[0] = Vector3.Lerp(frame1.Translations[0], frame2.Translations[0], k * (i + 1));
                keyframe.TranslationsMatrices[0] = Matrix4x4.CreateTranslation(keyframe.Translations[0]);

                // Slerp of quaternions
                for (int j = 0; j < keyframe.Quaternions.Count; j++)
                {
                    keyframe.Quaternions[j] = Quaternion.Slerp(frame1.Quaternions[j], frame2.Quaternions[j], k * (i + 1));
                    keyframe.Rotations[j] = MathC.QuaternionToEuler(keyframe.Quaternions[j]);
                }

                keyframe.CalculateBoundingBox(panelRendering.Model, panelRendering.Skin);
            }

            Saved = false;

            if (updateGUI)
            {
                // All done! Now I reset a bit the GUI
                OnKeyframesListChanged();
                timeline.Highlight(frameIndex1, frameIndex1 + numFrames + 1);
                timeline.ResetSelection();
                timeline.Value = frameIndex1 + numFrames + 1;
                popup.ShowInfo(panelRendering, "Successfully inserted " + numFrames + " interpolated frames between frames " + frameIndex1 + " and " + frameIndex2);
            }
        }

        private void InterpolateFrames(int numFrames = -1)
        {
            if (!ValidAndSelected()) return;

            if (numFrames < 0)
                using (var inputBox = new FormInputBox("Interpolation", "Enter number of interpolated frames:") { Width = 300 })
                {
                    if (inputBox.ShowDialog(this) == DialogResult.Cancel)
                        return;

                    if (!int.TryParse(inputBox.Result, out numFrames))
                        numFrames = 3; // Default value
                }
            else if (numFrames == 0)
            {
                popup.ShowError(panelRendering, "Interpolation requires at least 1 frame to insert");
                return;
            }

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            int start = timeline.Selection.X;
            int end = (timeline.SelectionSize == 1 && timeline.Selection.Y == timeline.Maximum) ? timeline.Minimum : timeline.Selection.Y;

            InterpolateFrames(start, end, numFrames);
        }
        private void DeleteEveryNFrame(int n)
        {
            for (int i = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1; i >= 0; i -= n)
            {
                _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveAt(i);
            }

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
        }
        private void InterpolateAnimation(int numFrames, bool fixAnimCommands, bool updateGUI = true)
        {
            int stepCount = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;
            if (stepCount <= 0) return;

            if (numFrames <= 0) return;

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            for (int i = 0; i < stepCount; i++)
            {
                int startFrame = i * (numFrames + 1);
                InterpolateFrames(startFrame, startFrame + 1, numFrames, false);
            }

            if (fixAnimCommands && _editor.CurrentAnim.WadAnimation.AnimCommands.Count > 0)
            {
                foreach (var ac in _editor.CurrentAnim.WadAnimation.AnimCommands)
                {
                    if (ac.Type >= WadAnimCommandType.PlaySound)
                        ac.Parameter1 *= (short)(numFrames + 1);
                }
            }

            Saved = false;

            if (updateGUI)
            {
                SelectAnimation(_editor.CurrentAnim);
                timeline.Highlight();
            }
        }

        private void EditAnimCommands(WadAnimCommand cmd = null)
        {
            if (_editor.CurrentAnim == null) return;

            var existingWindow = Application.OpenForms["FormAnimCommandsEditor"];
            if (existingWindow == null)
            {
                FormAnimCommandsEditor acEditor = new FormAnimCommandsEditor(_editor, _editor.CurrentAnim, cmd);
                acEditor.Show(this);
            }
            else
            {
                existingWindow.Focus();
                if (cmd != null)
                    ((FormAnimCommandsEditor)existingWindow).SelectCommand(cmd);
            }
        }

        private void EditStateChanges()
        {
            if (_editor.CurrentAnim == null) return;

            using (var form = new FormStateChangesEditor(_editor.CurrentAnim.WadAnimation.StateChanges))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

                // Add the new state changes
                _editor.CurrentAnim.WadAnimation.StateChanges.Clear();
                _editor.CurrentAnim.WadAnimation.StateChanges.AddRange(form.StateChanges);

                Saved = false;
            }

            timeline.Invalidate();
        }

        private void ImportAnimations()
        {
            if (_editor.CurrentAnim == null)
                return;

            string path = LevelFileDialog.BrowseFile(this, "Select a file with animations",
                BaseGeometryImporter.AnimationFileExtensions, false);

            WadAnimation animation = null;

            if (Path.GetExtension(path) == ".anim")
                animation = WadActions.ImportAnimationFromXml(_editor.Tool, path);
            else
                animation = WadActions.ImportAnimationFromModel(_editor.Tool, this, _editor.Moveable.Bones.Count, path);

            if (animation == null)
                return;

            if (animation.KeyFrames[0].Angles.Count != _editor.Moveable.Bones.Count)
            {
                popup.ShowError(panelRendering, "You can only import an animation with the same number of bones as current moveable.");
                return;
            }

            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            // Backup animcommands & state changes
            var oldCommands = _editor.CurrentAnim.WadAnimation.AnimCommands;
            var oldChanges = _editor.CurrentAnim.WadAnimation.StateChanges;

            _editor.CurrentAnim.WadAnimation = animation;

            // Restore animcommands & state changes
            _editor.CurrentAnim.WadAnimation.AnimCommands.AddRange(oldCommands);
            _editor.CurrentAnim.WadAnimation.StateChanges.AddRange(oldChanges);

            _editor.CurrentAnim.DirectXAnimation = Animation.FromWad2(_editor.Moveable.Bones, animation);
            SelectAnimation(_editor.CurrentAnim);
            timeline.Value = 0;
            lstAnimations.SelectedItem.Text = GetAnimLabel(lstAnimations.SelectedIndices[0], _editor.CurrentAnim);
            panelRendering.Invalidate();
        }

        private void PlayAnimation()
        {
            _timerPlayAnimation.Enabled = !_timerPlayAnimation.Enabled;

            if (_timerPlayAnimation.Enabled)
            {
                if (_editor.CurrentAnim?.WadAnimation != null)
                {
                    // Backup current state if chained playback is about to begin
                    _chainedPlaybackInitialAnim = _editor.CurrentAnim.Index;
                    _chainedPlaybackInitialCursorPos = timeline.Value;
                    _chainedPlaybackInitialSelection = timeline.Selection;

                    if (_editor.CurrentAnim.WadAnimation.KeyFrames.Count > 1)
                        _frameCount = timeline.Value * _editor.CurrentAnim.WadAnimation.FrameRate;
                }

                butTransportPlay.Image = Properties.Resources.transport_stop_24;
            }
            else
            {
                // Restore state of editor prior to chained playback. If user manually toggled
                // chained mode off during playback, state won't restore. This is intended.
                // FIXME: Make it an option!
                if (_chainedPlayback)
                {
                    var origNode = _editor.Animations.FirstOrDefault(item => item.Index == _chainedPlaybackInitialAnim);

                    if (origNode != null)
                    {
                        // Only try to update UI if we've switched to different anim
                        if (origNode != _editor.CurrentAnim)
                        {
                            SelectAnimation(origNode);

                            var listItem = lstAnimations.Items.FirstOrDefault(item => ((AnimationNode)item.Tag).Index == _chainedPlaybackInitialAnim);
                            if (listItem != null)
                            {
                                lstAnimations.SelectItem(lstAnimations.Items.IndexOf(listItem));
                                lstAnimations.EnsureVisible();
                            }
                            else
                            {
                                lstAnimations.SelectedIndices.Clear();
                                lstAnimations.Invalidate();
                            }

                            timeline.Value = _chainedPlaybackInitialCursorPos;
                            timeline.SelectionStart = _chainedPlaybackInitialSelection.X;
                            timeline.SelectionEnd = _chainedPlaybackInitialSelection.Y;
                        }
                    }
                }

                butTransportPlay.Image = Properties.Resources.transport_play_24;
            }
        }

        private void SaveChanges()
        {
            _editor.SaveChanges();
            Saved = true;
        }

        private void UpdateStatusLabel()
        {
            string newLabel =
                "Frame: " + (_frameCount) + " / " + (_editor.CurrentAnim.WadAnimation.FrameRate * (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1)) + "   " +
                "Keyframe: " + timeline.Value + " / " + (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1);

            if (!timeline.SelectionIsEmpty)
            {
                newLabel += "   Selected keyframe";
                if (timeline.SelectionSize == 1)
                    newLabel += ": " + timeline.Selection.X;
                else
                    newLabel += "s: " + timeline.Selection.X + " to " + timeline.Selection.Y;
            }

            statusFrame.Text = newLabel;
        }

        private bool ValidAndSelected(bool prompt = true)
        {
            if (timeline.SelectionIsEmpty)
            {
                if (prompt) popup.ShowError(panelRendering, "No frames selected. Please select at least 1 frame.");
            }
            else if (_editor.CurrentAnim == null)
            {
                if (prompt) popup.ShowError(panelRendering, "No animation selected. Select animation to work with.");
            }
            else if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == 0)
            {
                if (prompt) popup.ShowError(panelRendering, "Current animation contains no frames.");
            }
            else
                return true;

            return false;
        }

        private void drawGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelRendering.DrawGrid = !panelRendering.DrawGrid;
            panelRendering.Invalidate();
        }

        private void drawGizmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelRendering.DrawGizmo = !panelRendering.DrawGizmo;
            panelRendering.Invalidate();
        }

        private void drawCollisionBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelRendering.DrawCollisionBox = !panelRendering.DrawCollisionBox;
            panelRendering.Invalidate();
        }
        
        private void tbName_Validated(object sender, EventArgs e)
        {
            string newName = tbName.Text.Trim();

            if (_editor.CurrentAnim != null && !string.IsNullOrEmpty(newName) && newName != _editor.CurrentAnim.WadAnimation.Name)
            {
                _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

                _editor.CurrentAnim.WadAnimation.Name = newName;
                lstAnimations.SelectedItem.Text = GetAnimLabel(lstAnimations.SelectedIndices[0], _editor.CurrentAnim);
                Saved = false;
            }
        }
        
        private void comboBoneList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoneList.ComboBox.SelectedIndex < 1)
                panelRendering.SelectedMesh = null;
            else
                panelRendering.SelectedMesh = panelRendering.Model.Meshes[comboBoneList.ComboBox.SelectedIndex - 1];

            panelRendering.Invalidate();
        }

        // Menu controls one-liners

        private void addNewAnimationToolStripMenuItem_Click(object sender, EventArgs e) => AddNewAnimation();
        private void deleteAnimationToolStripMenuItem_Click(object sender, EventArgs e) => DeleteAnimation();
        private void insertFrameAfterCurrentOneToolStripMenuItem_Click(object sender, EventArgs e) => AddNewFrame(timeline.Value + 1, true);
        private void cutFramesToolStripMenuItem_Click(object sender, EventArgs e) => CutFrames();
        private void copyFramesToolStripMenuItem_Click(object sender, EventArgs e) => CopyFrames();
        private void pasteFramesToolStripMenuItem_Click(object sender, EventArgs e) => PasteFrames();
        private void deleteFramesToolStripMenuItem_Click(object sender, EventArgs e) => DeleteFrames(this, true, true);
        private void cutAnimationToolStripMenuItem_Click(object sender, EventArgs e) => CutAnimation();
        private void copyAnimationToolStripMenuItem_Click(object sender, EventArgs e) => CopyAnimation();
        private void pasteAnimationToolStripMenuItem_Click(object sender, EventArgs e) => PasteAnimation();
        private void splitAnimationToolStripMenuItem_Click(object sender, EventArgs e) => SplitAnimation();
        private void calculateBoundingBoxForCurrentFrameToolStripMenuItem_Click(object sender, EventArgs e) => CalculateKeyframeBoundingBox(timeline.Value, true, false);
        private void calculateBoundingBoxForAllFramesToolStripMenuItem_Click(object sender, EventArgs e) => CalculateAnimationBoundingBox();
        private void deleteCollisionBoxForCurrentFrameToolStripMenuItem_Click(object sender, EventArgs e) => ClearFrameBoundingBox(true);
        private void deleteBoundingBoxForAllFramesToolStripMenuItem_Click(object sender, EventArgs e) => ClearAnimationBoundingBox(true);
        private void interpolateFramesToolStripMenuItem_Click(object sender, EventArgs e) => InterpolateFrames();
        private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e) => SaveChanges();
        private void importToolStripMenuItem_Click(object sender, EventArgs e) => ImportAnimations();
        private void undoToolStripMenuItem_Click(object sender, EventArgs e) => _editor.Tool.UndoManager.Undo();
        private void redoToolStripMenuItem_Click(object sender, EventArgs e) => _editor.Tool.UndoManager.Redo();
        private void closeToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        // Toolbox controls one-liners

        private void butTbAddAnimation_Click(object sender, EventArgs e) => AddNewAnimation();
        private void butTbDeleteAnimation_Click(object sender, EventArgs e) => DeleteAnimation();
        private void butTbAddFrame_Click(object sender, EventArgs e) => AddNewFrame(timeline.Value + 1, true);
        private void butTbCutFrame_Click(object sender, EventArgs e) => CutFrames();
        private void butTbCopyFrame_Click(object sender, EventArgs e) => CopyFrames();
        private void butTbPasteFrame_Click(object sender, EventArgs e) => PasteFrames();
        private void butTbDeleteFrame_Click(object sender, EventArgs e) => DeleteFrames(this, true, true);
        private void butTbCutAnimation_Click(object sender, EventArgs e) => CutAnimation();
        private void butTbCopyAnimation_Click(object sender, EventArgs e) => CopyAnimation();
        private void butTbPasteAnimation_Click(object sender, EventArgs e) => PasteAnimation();
        private void butTbReplaceAnimation_Click(object sender, EventArgs e) => ReplaceAnimation();
        private void butTbSplitAnimation_Click(object sender, EventArgs e) => SplitAnimation();
        private void butTbSaveChanges_Click(object sender, EventArgs e) => SaveChanges();
        private void butTbUndo_Click(object sender, EventArgs e) => _editor.Tool.UndoManager.Undo();
        private void butTbRedo_Click(object sender, EventArgs e) => _editor.Tool.UndoManager.Redo();
        private void butTbImport_Click(object sender, EventArgs e) => ImportAnimations();

        // General controls one-liners

        private void butAddNewAnimation_Click(object sender, EventArgs e) => AddNewAnimation();
        private void butDeleteAnimation_Click(object sender, EventArgs e) => DeleteAnimation();
        private void butSearchByStateID_Click(object sender, EventArgs e) => RebuildAnimationsList();
        private void butCalculateBoundingBoxForCurrentFrame_Click(object sender, EventArgs e) => CalculateKeyframeBoundingBox(timeline.Value, true, false);
        private void butCalculateAnimCollision_Click(object sender, EventArgs e) => CalculateAnimationBoundingBox();
        private void butClearCollisionBox_Click(object sender, EventArgs e) => ClearFrameBoundingBox(false);
        private void butClearAnimCollision_Click(object sender, EventArgs e) => ClearAnimationBoundingBox(false);
        private void insertFramesAfterCurrentToolStripMenuItem_Click(object sender, EventArgs e) => InsertMultipleFrames();
        private void butEditAnimCommands_Click(object sender, EventArgs e) => EditAnimCommands();
        private void butEditStateChanges_Click(object sender, EventArgs e) => EditStateChanges();

        // Property controls one-liners

        private void tbName_KeyDown(object sender, KeyEventArgs e) { if (e.KeyData == Keys.Enter) tbName_Validated(this, e); }
        private void tbStateId_KeyDown(object sender, KeyEventArgs e) { if (e.KeyData == Keys.Enter) UpdateStateChange(); }
        private void tbStateId_Validated(object sender, EventArgs e) => UpdateStateChange();
        private void nudFramerate_Validated(object sender, EventArgs e) => UpdateAnimationParameter(nudFramerate);
        private void nudNextAnim_Validated(object sender, EventArgs e) => UpdateAnimationParameter(nudNextAnim);
        private void nudNextFrame_Validated(object sender, EventArgs e) => UpdateAnimationParameter(nudNextFrame);
        private void tbStartVertVel_Validated(object sender, EventArgs e) => UpdateAnimationParameter(tbStartVertVel);
        private void tbEndVertVel_Validated(object sender, EventArgs e) => UpdateAnimationParameter(tbEndVertVel);
        private void tbStartHorVel_Validated(object sender, EventArgs e) => UpdateAnimationParameter(tbStartHorVel);
        private void tbEndHorVel_Validated(object sender, EventArgs e) => UpdateAnimationParameter(tbEndHorVel);
        private void tbCollisionBoxMinX_Validated(object sender, EventArgs e) => ValidateCollisionBox(tbCollisionBoxMinX);
        private void tbCollisionBoxMinY_Validated(object sender, EventArgs e) => ValidateCollisionBox(tbCollisionBoxMinY);
        private void tbCollisionBoxMinZ_Validated(object sender, EventArgs e) => ValidateCollisionBox(tbCollisionBoxMinZ);
        private void tbCollisionBoxMaxX_Validated(object sender, EventArgs e) => ValidateCollisionBox(tbCollisionBoxMaxX);
        private void tbCollisionBoxMaxY_Validated(object sender, EventArgs e) => ValidateCollisionBox(tbCollisionBoxMaxY);
        private void tbCollisionBoxMaxZ_Validated(object sender, EventArgs e) => ValidateCollisionBox(tbCollisionBoxMaxZ);

        // Timeline & transport one-liners

        private void timeline_SelectionChanged(object sender, EventArgs e) => UpdateSelection();
        private void timeline_ValueChanged(object sender, EventArgs e) => SelectFrame(timeline.Value);
        private void timeline_AnimCommandDoubleClick(object sender, WadAnimCommand ac) => EditAnimCommands(ac);

        private void butTransportPlay_Click(object sender, EventArgs e) => PlayAnimation();
        private void butTransportStart_Click(object sender, EventArgs e) => timeline.Value = timeline.Minimum;
        private void butTransportFrameBack_Click(object sender, EventArgs e) => timeline.Value--;
        private void butTransportFrameForward_Click(object sender, EventArgs e) => timeline.Value++;
        private void butTransportEnd_Click(object sender, EventArgs e) => timeline.Value = timeline.Maximum;


        private void timerPlayAnimation_Tick(object sender, EventArgs e)
        {
            if (_editor.CurrentAnim?.WadAnimation == null || _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count < 1)
                return;

            int realFrameNumber = _editor.CurrentAnim.WadAnimation.FrameRate * (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1) + 1;

            _frameCount++;

            if (_frameCount >= realFrameNumber)
            {
                // Chain playback handling
                if (_chainedPlayback)
                {
                    var nextIndex = _editor.CurrentAnim.WadAnimation.NextAnimation;
                    var nextFrame = _editor.CurrentAnim.WadAnimation.NextFrame;
                    var nextNode  = _editor.Animations.FirstOrDefault(item => item.Index == nextIndex);

                    if (nextNode != null)
                    {
                        // Only try to update UI if next anim is different

                        if (nextNode != _editor.CurrentAnim)
                        {
                            SelectAnimation(nextNode);

                            var listItem = lstAnimations.Items.FirstOrDefault(item => ((AnimationNode)item.Tag).Index == nextIndex);
                            if (listItem != null)
                            {
                                lstAnimations.SelectItem(lstAnimations.Items.IndexOf(listItem));
                                lstAnimations.EnsureVisible();
                            }
                            else
                            {
                                lstAnimations.SelectedIndices.Clear();
                                lstAnimations.Invalidate();
                            }
                        }

                        var maxFrameNumber = nextNode.WadAnimation.FrameRate * (nextNode.DirectXAnimation.KeyFrames.Count - 1) + 1;
                        if (nextFrame > maxFrameNumber)
                        {
                            _frameCount = 0;
                            popup.ShowWarning(panelRendering, "No frame " + nextFrame + " in animation " + nextIndex + ". Using first frame.");
                        }
                        else
                            _frameCount = nextFrame;
                    }
                    else
                        popup.ShowWarning(panelRendering, "Animation " + nextIndex + " wasn't found. Chain is broken.");
                }
                else
                    _frameCount = 0;
            }

            bool isKeyFrame = (_frameCount % (_editor.CurrentAnim.WadAnimation.FrameRate == 0 ? 1 : _editor.CurrentAnim.WadAnimation.FrameRate) == 0);

            // Update animation
            if (isKeyFrame)
            {
                int newFrameNumber = _frameCount / _editor.CurrentAnim.WadAnimation.FrameRate;

                if (newFrameNumber > timeline.Maximum)
                    timeline.Value = 0;
                else
                    timeline.Value = newFrameNumber;
            }

            UpdateStatusLabel();

            // Preview sounds
            if (_previewSounds && _editor.Tool.ReferenceLevel != null && 
                _editor.Tool.ReferenceLevel.Settings.SoundSystem == SoundSystem.Xml)
            {
                // This additional counter is used to randomize material index every 3 seconds of playback
                _overallPlaybackCount++;
                if (_overallPlaybackCount > _materialIndexSwitchInterval)
                {
                    _overallPlaybackCount = 0;

                    var listOfMaterialSounds = _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.Where(s => s.Name.IndexOf("FOOTSTEPS_", StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();

                    while (true)
                    {
                        var newMaterialIndex = (new Random()).Next(0, listOfMaterialSounds.Count() - 1);
                        if (listOfMaterialSounds.Count == 1 || newMaterialIndex != _currentMaterialIndex)
                        {
                            _currentMaterialIndex = listOfMaterialSounds[newMaterialIndex].Id;
                            break;
                        }
                    }
                }

                foreach (var ac in _editor.CurrentAnim.WadAnimation.AnimCommands)
                {
                    int idToPlay = -1;

                    if (ac.Type == WadAnimCommandType.PlaySound)
                        idToPlay = ac.Parameter2 & 0x3FFF;
                    else if (_soundPreviewType == SoundPreviewType.LandWithMaterial && ac.Type == WadAnimCommandType.FlipEffect && (ac.Parameter2 & 0x3FFF) == 32)
                        idToPlay = _currentMaterialIndex;

                    if (idToPlay != -1 && ac.Parameter1 == _frameCount)
                    {
                        int sfx_type = ac.Type == WadAnimCommandType.FlipEffect ? 0x4000 : ac.Parameter2 & 0xC000;

                        // Don't play footprint FX sounds in water
                        if (ac.Type == WadAnimCommandType.FlipEffect && _soundPreviewType == SoundPreviewType.Water) continue;

                        // Don't play water sounds not in water and vice versa
                        if (sfx_type == 0x8000 && _soundPreviewType != SoundPreviewType.Water) continue;
                        if (sfx_type == 0x4000 && _soundPreviewType == SoundPreviewType.Water) continue;

                        var soundInfo = _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.FirstOrDefault(soundInfo_ => soundInfo_.Id == idToPlay);
                        if (soundInfo != null)
                        {
                            if (!_editor.Tool.ReferenceLevel.Settings.SelectedSounds.Contains(idToPlay))
                                popup.ShowWarning(panelRendering, "Sound info " + idToPlay + " is disabled in level settings.");
                            else
                                try
                                {
                                    // Task.Factory.StartNew(() => WadSoundPlayer.PlaySoundInfo(_level, soundInfo, false));
                                    WadSoundPlayer.PlaySoundInfo(_editor.Tool.ReferenceLevel, soundInfo, false);
                                }
                                catch (Exception exc)
                                {
                                    popup.ShowWarning(panelRendering, "Unable to play sound info " + idToPlay + ". Exception: \n" + exc.Message);
                                }
                        }
                        else
                            popup.ShowWarning(panelRendering, "Sound info " + idToPlay + " missing in reference project");
                    }
                }
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.CurrentAnim == null)
                return;

            string path = LevelFileDialog.BrowseFile(this, "Specify file to save animation",
                new List<FileFormat>() { new FileFormat("TombEditor XML", "anim") }, true);

            var animationToSave = _editor.GetSavedAnimation(_editor.CurrentAnim);

            if (!WadActions.ExportAnimationToXml(_editor.Moveable, animationToSave, path))
            {
                popup.ShowError(panelRendering, "Can't export current animation to XML file");
                return;
            }
        }

        private void comboRoomList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboRoomList.ComboBox.SelectedIndex == 0)
            {
                panelRendering.Room = null;
                panelRendering.RoomPosition = Vector3.Zero;
            }
            else
            {
                panelRendering.Room = (Room)comboRoomList.ComboBox.SelectedItem;
                panelRendering.RoomPosition = panelRendering.Room.GetLocalCenter();
            }

            panelRendering.Invalidate();
        }

        private void butShowAll_Click(object sender, EventArgs e)
        {
            tbSearchAnimation.Text = "";
            RebuildAnimationsList();
        }

        private void formAnimationEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Saved)
            {
                var result = DarkMessageBox.Show(this, "You have unsaved changes. Do you want to save changes to animations?",
                                                 "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _editor.SaveChanges();
                    DialogResult = DialogResult.OK;
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else
                    DialogResult = DialogResult.Cancel;
            }

            _timerPlayAnimation.Stop();
            _timerPlayAnimation.Tick -= timerPlayAnimation_Tick;
        }

        
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Don't process one-key and shift hotkeys if we're focused on control which allows text input
            if (WinFormsUtils.CurrentControlSupportsInput(this, keyData))
                return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Escape: timeline.ResetSelection(); break;
                case Keys.Left: timeline.ValueLoopDec(); break;
                case Keys.Right: timeline.ValueLoopInc(); break;
                case Keys.Up: timeline.Value = timeline.Minimum; break;
                case Keys.Down: timeline.Value = timeline.Maximum; break;
                case Keys.Space: PlayAnimation(); break;
                case Keys.I: timeline.SelectionStart = timeline.Value; break;
                case Keys.O: timeline.SelectionEnd = timeline.Value; break;
                case Keys.Delete: DeleteFrames(this, true, true); break;

                case (Keys.Control | Keys.A): timeline.SelectAll(); break;
                case (Keys.Control | Keys.X): CutFrames(); break;
                case (Keys.Control | Keys.C): CopyFrames(); break;
                case (Keys.Control | Keys.V): PasteFrames(); break;
                case (Keys.Control | Keys.S): SaveChanges(); break;
                case (Keys.Control | Keys.Z): _editor.Tool.UndoManager.Undo(); break;
                case (Keys.Control | Keys.Y): _editor.Tool.UndoManager.Redo(); break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void lstAnimations_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (lstAnimations.SelectedIndices.Count == 0)
                return;

            var node = (AnimationNode)lstAnimations.SelectedItem.Tag;
            SelectAnimation(node);
        }

        private void butTransportSound_Click(object sender, EventArgs e)
        {
            if (_editor.Tool.ReferenceLevel == null && !WadActions.LoadReferenceLevel(_editor.Tool, this)) return;

            _previewSounds = !_previewSounds;

            if (_previewSounds)
                butTransportSound.Image = Properties.Resources.transport_audio_24;
            else
                butTransportSound.Image = Properties.Resources.transport_mute_24;
        }

        private void butTransportLandWater_Click(object sender, EventArgs e)
        {
            if (_editor.Tool.ReferenceLevel == null && !WadActions.LoadReferenceLevel(_editor.Tool, this)) return;

            if (_soundPreviewType == SoundPreviewType.Land) _soundPreviewType = SoundPreviewType.LandWithMaterial;
            else if (_soundPreviewType == SoundPreviewType.LandWithMaterial) _soundPreviewType = SoundPreviewType.Water;
            else if (_soundPreviewType == SoundPreviewType.Water) _soundPreviewType = SoundPreviewType.Land;

            switch (_soundPreviewType)
            {
                case SoundPreviewType.Land:
                    butTransportLandWater.Image = Properties.Resources.transport_on_nothing_24;
                    break;
                case SoundPreviewType.LandWithMaterial:
                    butTransportLandWater.Image = Properties.Resources.transport_on_land_24;
                    break;
                case SoundPreviewType.Water:
                    butTransportLandWater.Image = Properties.Resources.transport_on_water_24;
                    break;
            }
        }

        private void tbSearchByStateID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                RebuildAnimationsList();
        }

        private void butTbInterpolateFrames_Click(object sender, EventArgs e)
        {
            int frameCount = 3;
            int.TryParse(tbInterpolateFrameCount.Text, out frameCount);
            InterpolateFrames(frameCount);
        }

        private void resampleAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormInputBox("Resample animation", "Enter resample multiplier (speed)", "1"))
            {
                form.Width = 300;
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

                    int result = 0;
                    if (!int.TryParse(form.Result, out result) || result <= 1)
                    {
                        popup.ShowError(panelRendering, "You must insert a number greater than 1");
                        return;
                    }

                    InterpolateAnimation(result - 1, true, true);
                }
            }
        }

        private void resampleAnimationToKeyframesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.CurrentAnim.WadAnimation.FrameRate < 2)
            {
                popup.ShowError(panelRendering, "Animation is already at framerate 1. You need other framerate to resample.");
                return;
            }
            
            InterpolateAnimation(_editor.CurrentAnim.WadAnimation.FrameRate - 1, false, false);
            _editor.CurrentAnim.WadAnimation.FrameRate = 1;

            Saved = false;

            SelectAnimation(_editor.CurrentAnim);
            timeline.Highlight();
        }

        private void transportChained_Click(object sender, EventArgs e)
        {
            _chainedPlayback = !_chainedPlayback;

            if (_chainedPlayback)
                butTransportChained.Image = Properties.Resources.transport_chain_enabled_24;
            else
                butTransportChained.Image = Properties.Resources.transport_chain_disabled_24;
        }

        private void lstAnimations_Click(object sender, EventArgs e)
        {
            // Update saved state's anim number, in case chained playback is in effect
            if (_editor.CurrentAnim != null && lstAnimations.SelectedItem != null)
            {
                _chainedPlaybackInitialAnim = _editor.CurrentAnim.Index;
                _chainedPlaybackInitialCursorPos = 0;
                _chainedPlaybackInitialSelection = new VectorInt2();
            }
        }

        private void cmbStateID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStateID.SelectedIndex != -1)
                UpdateStateChange(true);
        }

        private void tbStateID_MouseWheel(object sender, MouseEventArgs e)
        {
            // FIXME: this code is disabled to prevent undo stack flood.
            // If anybody will discover a way to prevent doing that, I will be glad.

            /*
            if (e.Delta < 0 && cmbStateID.SelectedIndex < cmbStateID.Items.Count - 1)
                cmbStateID.SelectedIndex++;
            else if (e.Delta > 0 && cmbStateID.SelectedIndex > 0)
                cmbStateID.SelectedIndex--;
            */
        }

        private void butSearchStateID_Click(object sender, EventArgs e)
        {
            var searchPopUp = new PopUpSearch(cmbStateID);
            searchPopUp.Show(this);
        }

        private void DeleteEveryNthFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormInputBox("Delete every nth frame", "Enter number", "2"))
            {
                form.Width = 300;
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

                    int result = 0;
                    if (!int.TryParse(form.Result, out result) || result <= 1)
                    {
                        popup.ShowError(panelRendering, "You must insert a number greater than 1");
                        return;
                    }
                    DeleteEveryNFrame(result);
                }

                if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1 < timeline.Value) {
                    timeline.Value = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;
                }
                SelectAnimation(_editor.CurrentAnim);
                timeline.Highlight();
            }
        }

        private void findReplaceAnimcommandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormReplaceAnimCommands(_editor))
                form.ShowDialog(this);
        }
    }
}