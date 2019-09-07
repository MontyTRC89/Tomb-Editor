using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormAnimationEditor : DarkUI.Forms.DarkForm
    {

        private enum SearchType
        {
            None,
            StateID,
            AnimNumber,
            Name
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
                Text = "Animation editor - " + _moveable.Id.ToString(_wad.SuggestedGameVersion) + (!_saved ? "*" : "");
            }
        }

        private WadMoveableId _moveableId;
        private Wad2 _wad;
        private WadMoveable _moveable;
        private WadToolClass _tool;
        private List<AnimationNode> _workingAnimations;
        private DeviceManager _deviceManager;
        private List<WadBone> _bones;
        private AnimationNode _selectedNode;
        private WadRenderer _renderer;
        private AnimatedModel _model;

        // Player
        private Timer _timerPlayAnimation;
        private int _frameCount;
        private bool _previewSounds;
        private int _overallPlaybackCount = _materialIndexSwitchInterval; // To reset 1st time on playback
        private int _currentMaterialIndex;
        private SoundPreviewType _soundPreviewType = SoundPreviewType.Land;

        private static readonly int _materialIndexSwitchInterval = 30 * 3; // 3 seconds, 30 game frames

        // Clipboard
        private List<KeyFrame> _clipboardKeyFrames = new List<KeyFrame>();
        private AnimationNode  _clipboardNode = null;

        // Info
        private readonly PopUpInfo popup = new PopUpInfo();

        // Helpers
        private static string GetAnimLabel(int index, AnimationNode anim) => "(" + index + ") " + anim.WadAnimation.Name;
       
        public FormAnimationEditor(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadMoveableId id)
        {
            InitializeComponent();

            _renderer = new WadRenderer(deviceManager.___LegacyDevice);
            _tool = tool;
            _moveableId = id;
            _wad = wad;
            _moveable = _wad.Moveables[_moveableId];
            _model = _renderer.GetMoveable(_moveable);
            _deviceManager = deviceManager;

            panelRendering.Configuration = _tool.Configuration;

            // Update reference level
            UpdateReferenceLevelControls();

            // Initialize playback
            _timerPlayAnimation = new Timer() { Interval = 30 };
            _timerPlayAnimation.Tick += timerPlayAnimation_Tick;
            _previewSounds = false;

            // Add custom event handler for direct editing of animcommands
            timeline.AnimCommandDoubleClick += new EventHandler<WadAnimCommand>(timeline_AnimCommandDoubleClick);

            // Initialize the panel
            var skin = _moveableId;
            if (_moveableId.TypeId == 0)
            {
                if (_wad.SuggestedGameVersion == WadGameVersion.TR4_TRNG && _wad.Moveables.ContainsKey(WadMoveableId.LaraSkin))
                    skin = WadMoveableId.LaraSkin;
                if (_wad.SuggestedGameVersion == WadGameVersion.TR5 && _wad.Moveables.ContainsKey(WadMoveableId.LaraSkin))
                    skin = WadMoveableId.LaraSkin;
            }
            panelRendering.InitializeRendering(_tool, _wad, _deviceManager, _moveableId, skin);

            // Get a copy of the skeleton in linearized form
            _bones = _moveable.Bones;

            // Load skeleton in combobox
            comboBoneList.ComboBox.Items.Add("(select mesh)");
            foreach (var bone in panelRendering.Model.Bones)
                comboBoneList.ComboBox.Items.Add(bone.Name);
            comboBoneList.ComboBox.SelectedIndex = 0;

            // NOTE: we work with a pair WadAnimation - Animation. All changes to animation data like name,
            // framerate, next animation, state changes... will be saved directly to WadAnimation.
            // All changes to keyframes will be instead stored directly in the renderer's Animation class.
            // While saving, WadAnimation and Animation will be combined and original animations will be overwritten.
            _workingAnimations = new List<AnimationNode>();
            foreach (var animation in _moveable.Animations)
                _workingAnimations.Add(new AnimationNode(animation.Clone(), Animation.FromWad2(_bones, animation)));
            ReloadAnimations();

            if (_workingAnimations.Count() != 0)
            {
                SelectAnimation(_workingAnimations[0]);
                SelectFrame(0);
            }

            tool.EditorEventRaised += Tool_EditorEventRaised;

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

                Saved = false;
            }

            if (obj is WadToolClass.ReferenceLevelChangedEvent)
            {
                UpdateReferenceLevelControls();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _renderer.Dispose();
            if (disposing && (components != null))
            {
                components.Dispose();
                _tool.EditorEventRaised -= Tool_EditorEventRaised;
            }
            base.Dispose(disposing);
        }

        private void UpdateReferenceLevelControls()
        {
            panelRendering.Level = _tool.ReferenceLevel;
            panelRendering.Invalidate();

            comboRoomList.ComboBox.Items.Clear();

            if (_tool.ReferenceLevel != null)
            {
                // Load rooms into the combo box
                comboRoomList.Enabled = true;
                comboRoomList.ComboBox.Items.Add("(select room)");

                foreach (var room in _tool.ReferenceLevel.Rooms)
                    if (room != null)
                        comboRoomList.ComboBox.Items.Add(room);

                // Enable sound transport
                butTransportSound.Enabled = true;
                butTransportLandWater.Enabled = true;
            }
            else
            {
                comboRoomList.Enabled = false;

                // Disable sound transport
                butTransportSound.Enabled = false;
                butTransportLandWater.Enabled = false;
            }
        }

        private void ReloadAnimations()
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
                            searchType = SearchType.Name;
                        }
                    }
                }
                else
                {
                    if (!int.TryParse(searchString, out searchNumber))
                    {
                        searchType = SearchType.Name; // If no numerical, always search as by name
                        finalSearchString = searchString;
                    }
                    else
                        searchType = SearchType.StateID; // Otherwise prioritize state id
                }
            }

            // Filter regarding request
            for (int i = 0; i < _workingAnimations.Count; i++)
            {
                switch(searchType)
                {
                    case SearchType.StateID:
                        if (searchNumber >= 0 && _workingAnimations[i].WadAnimation.StateId != searchNumber)
                            continue;
                        break;
                    case SearchType.AnimNumber:
                        if (searchNumber >= 0 && i != searchNumber)
                            continue;
                        break;
                    case SearchType.Name:
                        if (!_workingAnimations[i].WadAnimation.Name.ToLower().Contains(finalSearchString))
                            continue;
                        break;
                    default:
                        break;
                }

                var item = new DarkListItem(GetAnimLabel(index++, _workingAnimations[i]));
                item.Tag = _workingAnimations[i];
                lstAnimations.Items.Add(item);
            }

            if (lstAnimations.Items.Count != 0)
                lstAnimations.SelectItem(0);

            lstAnimations.EnsureVisible();
        }

        private void SelectAnimation(AnimationNode node)
        {
            _selectedNode = node;

            tbName.Text = node.WadAnimation.Name;
            tbFramerate.Text = node.WadAnimation.FrameRate.ToString();
            tbNextAnimation.Text = node.WadAnimation.NextAnimation.ToString();
            tbNextFrame.Text = node.WadAnimation.NextFrame.ToString();
            tbStateId.Text = node.WadAnimation.StateId.ToString();
            tbStartVelocity.Text = node.WadAnimation.StartVelocity.ToString();
            tbEndVelocity.Text = node.WadAnimation.EndVelocity.ToString();
            tbLateralStartVelocity.Text = node.WadAnimation.StartLateralVelocity.ToString();
            tbLateralEndVelocity.Text = node.WadAnimation.EndLateralVelocity.ToString();

            // TODO: deprecated stuff
            /*tbSpeed.Text = (node.WadAnimation.Speed / 65536.0f).ToString();
            tbAccel.Text = (node.WadAnimation.Acceleration / 65536.0f).ToString();
            tbLatSpeed.Text = (node.WadAnimation.LateralSpeed / 65536.0f).ToString();
            tbLatAccel.Text = (node.WadAnimation.LateralAcceleration / 65536.0f).ToString();*/

            timeline.Animation = node;
            panelRendering.CurrentKeyFrame = 0;
            panelRendering.SelectedMesh = null;
            panelRendering.Animation = node;

            if (node.DirectXAnimation.KeyFrames.Count > 0)
                panelRendering.Model.BuildAnimationPose(node.DirectXAnimation.KeyFrames[0]);


            if (node.DirectXAnimation.KeyFrames.Count > 0)
            {
                OnKeyframesListChanged();
                timeline.Value = 0;
                timeline.ResetSelection();
            }
            else
            {
                statusFrame.Text = "";
                timeline.Invalidate();
            }

            panelRendering.Invalidate();

        }

        private void SelectFrame(int frameIndex)
        {
            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count > 0)
            {
                if (frameIndex >= _selectedNode.DirectXAnimation.KeyFrames.Count)
                    frameIndex = _selectedNode.DirectXAnimation.KeyFrames.Count - 1;
                var keyFrame = _selectedNode.DirectXAnimation.KeyFrames[frameIndex];
                panelRendering.Model.BuildAnimationPose(keyFrame);
                panelRendering.CurrentKeyFrame = frameIndex;
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
        }

        private void OnKeyframesListChanged()
        {
            if (_selectedNode != null)
            {
                _frameCount = timeline.Value * _selectedNode.WadAnimation.FrameRate;
                timeline.Minimum = 0;
                timeline.Maximum = _selectedNode.DirectXAnimation.KeyFrames.Count - 1;
                UpdateStatusLabel();
            }
        }
        
        private WadAnimation SaveAnimationChanges(AnimationNode animation)
        {
            var wadAnim = animation.WadAnimation.Clone();
            var directxAnim = animation.DirectXAnimation;

            // I need only to convert DX keyframes to Wad2 keyframes
            wadAnim.KeyFrames.Clear();
            foreach (var directxKeyframe in directxAnim.KeyFrames)
            {
                var keyframe = new WadKeyFrame();

                // Create the new bounding box
                keyframe.BoundingBox = new TombLib.BoundingBox(directxKeyframe.BoundingBox.Minimum,
                                                               directxKeyframe.BoundingBox.Maximum);

                // For now we take the first translation as offset
                keyframe.Offset = new System.Numerics.Vector3(directxKeyframe.Translations[0].X,
                                                              directxKeyframe.Translations[0].Y,
                                                              directxKeyframe.Translations[0].Z);

                // Convert angles from radians to degrees and save them
                foreach (var rot in directxKeyframe.Rotations)
                {
                    var angle = new WadKeyFrameRotation();
                    angle.Rotations = new System.Numerics.Vector3(rot.X * 180.0f / (float)Math.PI,
                                                                  rot.Y * 180.0f / (float)Math.PI,
                                                                  rot.Z * 180.0f / (float)Math.PI);
                    keyframe.Angles.Add(angle);
                }

                wadAnim.KeyFrames.Add(keyframe);
            }

            return wadAnim;
        }

        private bool SaveChanges(bool confirm)
        {
            if (confirm && DarkMessageBox.Show(this, "Do you really want to save changes to animations?", "Save changes",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return false;

            // Clear the old animations
            _moveable.Animations.Clear();

            // Combine WadAnimation and Animation classes
            foreach (var animation in _workingAnimations)
                _moveable.Animations.Add(SaveAnimationChanges(animation));

            // ReloadAnimations(); // Why the hell we need to do this? -- Lwmte

            _moveable.Version = DataVersion.GetNext();
            Saved = true;
            return true;
        }

        private void CalculateAnimationBoundingBox(bool clear = false)
        {
            if (_selectedNode != null)
            {
                int startFrame = panelRendering.CurrentKeyFrame;
                for (int i = 0; i < _selectedNode.DirectXAnimation.KeyFrames.Count; i++)
                {
                    timeline.Value = i;
                    CalculateKeyframeBoundingBox(i, clear);
                }
                timeline.Value = (startFrame);
            }
        }

        private void ClearAnimationBoundingBox(bool confirm = false)
        {
            if (_selectedNode == null)
                return;

            if (confirm && (DarkMessageBox.Show(this, "Do you really want to delete the collision box for each frame of animation '" + _selectedNode.WadAnimation.Name + "'?",
                           "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No))
                return;

            CalculateAnimationBoundingBox(true);
        }

        private void CalculateKeyframeBoundingBox(int index, bool clear = false)
        {
            if (_selectedNode != null)
            {
                var keyFrame = _selectedNode.DirectXAnimation.KeyFrames[index];
                
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
        }

        private void ClearFrameBoundingBox(bool confirm = false)
        {
            if (_selectedNode == null)
                return;

            if (confirm && (DarkMessageBox.Show(this, "Do you really want to delete the collision box for the current keyframe?",
                                        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No))
                return;

            CalculateKeyframeBoundingBox(panelRendering.CurrentKeyFrame, true);
        }

        private void AddNewAnimation()
        {
            var wadAnimation = new WadAnimation();
            wadAnimation.FrameRate = 1;
            wadAnimation.Name = "New Animation " + _workingAnimations.Count;

            var keyFrame = new WadKeyFrame();
            foreach (var bone in _bones)
                keyFrame.Angles.Add(new WadKeyFrameRotation());
            wadAnimation.KeyFrames.Add(keyFrame);

            var dxAnimation = Animation.FromWad2(_bones, wadAnimation);
            var node = new AnimationNode(wadAnimation, dxAnimation);

            var item = new DarkUI.Controls.DarkListItem(GetAnimLabel(_workingAnimations.Count, node));
            item.Tag = node;

            _workingAnimations.Add(node);
            lstAnimations.Items.Add(item);
            lstAnimations.SelectItem(lstAnimations.Items.Count - 1);
            lstAnimations.EnsureVisible();

            tbName.Focus();

            Saved = false;
        }

        private void DeleteAnimation()
        {
            if (_selectedNode != null)
            {
                if (DarkMessageBox.Show(this, "Do you really want to delete '" + _selectedNode.WadAnimation.Name + "'?",
                                        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int currentIndex = _workingAnimations.IndexOf(_selectedNode);

                    // Update all references
                    for (int i = 0; i < _workingAnimations.Count; i++)
                    {
                        // Ignore the animation I'm deleting
                        if (i == currentIndex)
                            continue;

                        var animation = _workingAnimations[i];

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
                    _workingAnimations.Remove(_selectedNode);

                    // Update GUI
                    lstAnimations.Items.RemoveAt(lstAnimations.SelectedIndices[0]);
                    if (lstAnimations.Items.Count != 0)
                        SelectAnimation(lstAnimations.Items[0].Tag as AnimationNode);
                    else
                    {
                        _selectedNode = null;
                        timeline.Animation = null;
                    }
                    panelRendering.Invalidate();
                    timeline.Invalidate();

                    Saved = false;
                }
            }
        }

        private void AddNewFrame(int index)
        {
            if (_selectedNode != null)
            {
                var keyFrame = new KeyFrame();
                foreach (var bone in _bones)
                {
                    keyFrame.Rotations.Add(Vector3.Zero);
                    keyFrame.Quaternions.Add(Quaternion.Identity);
                    keyFrame.Translations.Add(bone.Translation);
                    keyFrame.TranslationsMatrices.Add(Matrix4x4.CreateTranslation(bone.Translation));
                }

                if (_selectedNode.DirectXAnimation.KeyFrames.Count == 0)
                    index = 0;

                _selectedNode.DirectXAnimation.KeyFrames.Insert(index, keyFrame);
                OnKeyframesListChanged();
                Saved = false;
            }
        }

        private void InsertMultipleFrames()
        {
            using (var form = new FormInputBox("Add (n) frames", "How many frames do you want to add to current animation?", "1"))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    int framesCount = 0;
                    if (!int.TryParse(form.Result, out framesCount) || framesCount <= 0)
                    {
                        popup.ShowError(panelRendering, "You must insert a number greater than 0");
                        return;
                    }

                    // Add frames
                    for (int i = 0; i < framesCount; i++)
                        AddNewFrame(panelRendering.CurrentKeyFrame + 1);
                }
            }
        }

        private void DeleteFrames(IWin32Window owner, bool updateGUI = true) // No owner = no warnings!
        {
            if (!ValidAndSelected(owner != null)) return;

            if (owner != null &&
                DarkMessageBox.Show(this, "Do you really want to delete frame" +
                (timeline.SelectionSize == 1 ? " " + panelRendering.CurrentKeyFrame : "s " + timeline.Selection.X + "-" + timeline.Selection.Y) + "?",
                                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            int selectionStart = timeline.Selection.X; // Save last index
            int selectionEnd = timeline.Selection.Y;

            // Save cursor position to restore later. Only do it if cursor is outside selection.
            int cursorPos = (timeline.Value < timeline.Selection.X || timeline.Value > timeline.Selection.Y) ? timeline.Value : -1;

            // Remove the frames
            _selectedNode.DirectXAnimation.KeyFrames.RemoveRange(timeline.Selection.X, timeline.SelectionSize);
            
            Saved = false;
            if (!updateGUI) return;

            // Update GUI
            timeline.ResetSelection();
            OnKeyframesListChanged();
            if (_selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                int insertEnd = selectionStart + _clipboardKeyFrames.Count - 1;

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

            CopyFrames();
            DeleteFrames(null);
        }

        private void CopyFrames()
        {
            if (!ValidAndSelected()) return;

            _clipboardKeyFrames.Clear();
            for (int i = timeline.Selection.X; i <= timeline.Selection.Y; i++)
                _clipboardKeyFrames.Add(_selectedNode.DirectXAnimation.KeyFrames[i].Clone());

            timeline.Highlight(timeline.Selection.X, timeline.Selection.Y);
        }

        private void PasteFrames()
        {
            if (_clipboardKeyFrames != null && _clipboardKeyFrames.Count > 0 && _selectedNode != null)
            {
                int startIndex = timeline.SelectionIsEmpty ? timeline.Value : timeline.Selection.X; // Save last index
                int endIndex = timeline.SelectionIsEmpty ? timeline.Value : timeline.Selection.Y;

                // Save cursor position to restore later. Only do it if cursor is outside selection.
                int cursorPos = (timeline.SelectionIsEmpty || timeline.Value < timeline.Selection.X || timeline.Value > timeline.Selection.Y) ? timeline.Value : -1;

                // Replace if there is a selection.
                if (!timeline.SelectionIsEmpty)
                    DeleteFrames(null, false);

                _selectedNode.DirectXAnimation.KeyFrames.InsertRange(startIndex, _clipboardKeyFrames);
                OnKeyframesListChanged();

                int insertEnd = startIndex + _clipboardKeyFrames.Count - 1;
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
        }

        private void CutAnimation()
        {
            if (_selectedNode != null)
            {
                _clipboardNode = _selectedNode;
                DeleteAnimation();
            }
        }

        private void CopyAnimation()
        {
            if (_selectedNode != null)
            {
                _clipboardNode =_selectedNode.Clone();
            }
        }

        private void PasteAnimation()
        {
            if (_clipboardNode != null && _selectedNode != null)
            {
                int animationIndex = _workingAnimations.IndexOf(_selectedNode) + 1;
                _workingAnimations.Insert(animationIndex, _clipboardNode);
                _clipboardNode.DirectXAnimation.Name += " - Copy";
                _clipboardNode.WadAnimation.Name += " - Copy";
                //_clipboardNode = null;
                ReloadAnimations();
                SelectAnimation(_workingAnimations[animationIndex]);
                Saved = false;
            }
        }

        private void ReplaceAnimation()
        {
            if (_clipboardNode != null && _selectedNode != null)
            {
                int animationIndex = _workingAnimations.IndexOf(_selectedNode);
                _workingAnimations[animationIndex] = _clipboardNode;
                _clipboardNode.DirectXAnimation.Name += " - Copy";
                _clipboardNode.WadAnimation.Name += " - Copy";
                //_clipboardNode = null;
                ReloadAnimations();
                SelectAnimation(_workingAnimations[animationIndex]);
                Saved = false;
            }
        }

        private void SplitAnimation()
        {
            if (_selectedNode != null)
            {
                // I need at least 1 frame in the middle
                if (_selectedNode.DirectXAnimation.KeyFrames.Count < 3)
                {
                    popup.ShowError(panelRendering, "You must have at least 3 frames for splitting the animation");
                    return;
                }

                // Check if we have selected the first or the last frame
                int numFrames = _selectedNode.DirectXAnimation.KeyFrames.Count;
                if (panelRendering.CurrentKeyFrame == 0 || panelRendering.CurrentKeyFrame == numFrames - 1)
                {
                    popup.ShowError(panelRendering, "You can't set the first or the last frame for splitting the animation");
                    return;
                }
                var newWadAnimation = _selectedNode.WadAnimation.Clone();
                var newDirectXAnimation = _selectedNode.DirectXAnimation.Clone();

                int numFrames1 = panelRendering.CurrentKeyFrame;
                int numFrames2 = numFrames - numFrames1;

                // Remove frames from the two animations
                _selectedNode.DirectXAnimation.KeyFrames.RemoveRange(panelRendering.CurrentKeyFrame + 1, numFrames2 - 1);
                newDirectXAnimation.KeyFrames.RemoveRange(0, panelRendering.CurrentKeyFrame);

                // Add the new animation at the bottom of the list
                newWadAnimation.Name += " - splitted";
                _workingAnimations.Add(new AnimationNode(newWadAnimation, newDirectXAnimation));

                // Update the GUI
                ReloadAnimations();
                SelectAnimation(_selectedNode);

                Saved = false;
            }
        }

        private int UpdateAnimationParameter(Control control, float multiplier = 1.0f)
        {
            if (_selectedNode == null) return 0;

            float result = 0;
            if (control is DarkTextBox)
            {
                string toParse = ((DarkTextBox)control).Text.Replace(",", ".");
                if (!float.TryParse(toParse, out result))
                    return 0;
            }
            else if (control is DarkNumericUpDown)
                result = (float)((DarkNumericUpDown)control).Value;

            Saved = false;
            timeline.Invalidate();
            panelRendering.Invalidate();

            return (int)Math.Round(result * multiplier, 0);
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

            int frameIndex1 = timeline.Selection.X;
            int frameIndex2 = (timeline.SelectionSize == 1 && timeline.Selection.Y == timeline.Maximum) ? timeline.Minimum : timeline.Selection.Y;

            var frame1 = _selectedNode.DirectXAnimation.KeyFrames[frameIndex1];
            var frame2 = _selectedNode.DirectXAnimation.KeyFrames[frameIndex2];

            // Cut existing frames between 1 and 2

            if (frameIndex2 - frameIndex1 > 1)
                _selectedNode.DirectXAnimation.KeyFrames.RemoveRange(frameIndex1 + 1, frameIndex2 - frameIndex1 - 1);

            // Now calculate how many frames I must insert
            for (int i = 0; i < numFrames; i++)
            {
                var keyFrame = new KeyFrame();
                foreach (var bone in _bones)
                {
                    keyFrame.Rotations.Add(Vector3.Zero);
                    keyFrame.Quaternions.Add(Quaternion.Identity);
                    keyFrame.Translations.Add(frame1.Translations[0] + bone.Translation);
                    keyFrame.TranslationsMatrices.Add(Matrix4x4.CreateTranslation(frame1.Translations[0] + bone.Translation));
                }
                _selectedNode.DirectXAnimation.KeyFrames.Insert(frameIndex1 + 1 + i, keyFrame);
            }

            // Slerp factor
            float k = 1.0f / (numFrames + 1);

            // Now I have the right number of frames and I can do slerp
            for (int i = 0; i < numFrames; i++)
            {
                var keyframe = _selectedNode.DirectXAnimation.KeyFrames[frameIndex1 + i + 1];

                // Lerp translation of root bone
                keyframe.Translations[0] = Vector3.Lerp(frame1.Translations[0], frame2.Translations[0], k * (i + 1));
                keyframe.TranslationsMatrices[0] = Matrix4x4.CreateTranslation(keyframe.Translations[0]);

                // Slerp of quaternions
                for (int j = 0; j < keyframe.Quaternions.Count; j++)
                {
                    keyframe.Quaternions[j] = Quaternion.Slerp(frame1.Quaternions[j], frame2.Quaternions[j], k * (i + 1));
                    keyframe.Rotations[j] = MathC.QuaternionToEuler(keyframe.Quaternions[j]);
                }
            }

            // All done! Now I reset a bit the GUI
            OnKeyframesListChanged();
            timeline.Highlight(frameIndex1, frameIndex1 + numFrames + 1);
            timeline.ResetSelection();
            timeline.Value = frameIndex1 + numFrames + 1;
            popup.ShowInfo(panelRendering, "Successfully inserted " + numFrames + " interpolated frames between frames " + frameIndex1 + " and " + frameIndex2);

            Saved = false;
        }

        private void EditAnimCommands(WadAnimCommand cmd = null)
        {
            if (_selectedNode != null)
            {
                using (var form = new FormAnimCommandsEditor(_tool, _selectedNode.WadAnimation.AnimCommands, cmd))
                {
                    if (form.ShowDialog(this) != DialogResult.OK)
                        return;

                    // Add the new state changes
                    _selectedNode.WadAnimation.AnimCommands.Clear();
                    _selectedNode.WadAnimation.AnimCommands.AddRange(form.AnimCommands);

                    Saved = false;
                }

                timeline.Invalidate();
            }
        }

        private void EditStateChanges()
        {
            if (_selectedNode != null)
            {
                using (var form = new FormStateChangesEditor(_selectedNode.WadAnimation.StateChanges))
                {
                    if (form.ShowDialog(this) != DialogResult.OK)
                        return;

                    // Add the new state changes
                    _selectedNode.WadAnimation.StateChanges.Clear();
                    _selectedNode.WadAnimation.StateChanges.AddRange(form.StateChanges);

                    Saved = false;
                }

                timeline.Invalidate();
            }
        }

        private void PlayAnimation()
        {
            _timerPlayAnimation.Enabled = !_timerPlayAnimation.Enabled;

            if (_timerPlayAnimation.Enabled)
            {
                if (_selectedNode?.WadAnimation != null && _selectedNode.WadAnimation.KeyFrames.Count > 1)
                    _frameCount = timeline.Value * _selectedNode.WadAnimation.FrameRate;
                butTransportPlay.Image = Properties.Resources.transport_stop_24;
            }
            else
            {
                butTransportPlay.Image = Properties.Resources.transport_play_24;
            }
        }

        private void UpdateStatusLabel()
        {
            string newLabel =
                "Frame: " + (_frameCount + 1) + " / " + (_selectedNode.WadAnimation.FrameRate * (_selectedNode.WadAnimation.KeyFrames.Count - 1) + 1) + "   " +
                "Keyframe: " + (timeline.Value + 1) + " / " + _selectedNode.DirectXAnimation.KeyFrames.Count;

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
                popup.ShowError(panelRendering, "No frames selected. Please select at least 1 frame.");
            else if (_selectedNode == null)
                popup.ShowError(panelRendering, "No animation selected. Select animation to work with.");
            else if (_selectedNode.DirectXAnimation.KeyFrames.Count == 0)
                popup.ShowError(panelRendering, "Current animation contains no frames.");
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
            if (_selectedNode != null && tbName.Text.Trim() != "")
            {
                _selectedNode.WadAnimation.Name = tbName.Text.Trim();
                lstAnimations.Items[lstAnimations.SelectedIndices[0]].Text = GetAnimLabel(lstAnimations.SelectedIndices[0], _selectedNode);
                Saved = false;
            }
        }

        private void tbCollisionBoxMinX_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMinX.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Minimum = new Vector3(result, bb.Minimum.Y, bb.Minimum.Z);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
                Saved = false;
            }
        }

        private void tbCollisionBoxMinY_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMinY.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Minimum = new Vector3(bb.Minimum.X, result, bb.Minimum.Z);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
                Saved = false;
            }
        }

        private void tbCollisionBoxMinZ_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMinZ.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Minimum = new Vector3(bb.Minimum.X, bb.Minimum.Y, result);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
                Saved = false;
            }
        }

        private void tbCollisionBoxMaxX_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMaxX.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Maximum = new Vector3(result, bb.Maximum.Y, bb.Maximum.Z);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
                Saved = false;
            }
        }

        private void tbCollisionBoxMaxY_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMaxY.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Maximum = new Vector3(bb.Maximum.X, result, bb.Maximum.Z);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
                Saved = false;
            }
        }

        private void tbCollisionBoxMaxZ_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMaxZ.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Maximum = new Vector3(bb.Maximum.X, bb.Maximum.Y, result);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
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

        private void addNewAnimationToolStripMenuItem_Click(object sender, EventArgs e) => AddNewAnimation();
        private void deleteAnimationToolStripMenuItem_Click(object sender, EventArgs e) => DeleteAnimation();
        private void insertFrameAfterCurrentOneToolStripMenuItem_Click(object sender, EventArgs e) => AddNewFrame(panelRendering.CurrentKeyFrame + 1);
        private void cutFramesToolStripMenuItem_Click(object sender, EventArgs e) => CutFrames();
        private void copyFramesToolStripMenuItem_Click(object sender, EventArgs e) => CopyFrames();
        private void pasteFramesToolStripMenuItem_Click(object sender, EventArgs e) => PasteFrames();
        private void deleteFramesToolStripMenuItem_Click(object sender, EventArgs e) => DeleteFrames(this);
        private void cutAnimationToolStripMenuItem_Click(object sender, EventArgs e) => CutAnimation();
        private void copyAnimationToolStripMenuItem_Click(object sender, EventArgs e) => CopyAnimation();
        private void pasteAnimationToolStripMenuItem_Click(object sender, EventArgs e) => PasteAnimation();
        private void splitAnimationToolStripMenuItem_Click(object sender, EventArgs e) => SplitAnimation();
        private void calculateBoundingBoxForCurrentFrameToolStripMenuItem_Click(object sender, EventArgs e) => CalculateKeyframeBoundingBox(panelRendering.CurrentKeyFrame);
        private void calculateBoundingBoxForAllFramesToolStripMenuItem_Click(object sender, EventArgs e) => CalculateAnimationBoundingBox();
        private void interpolateFramesToolStripMenuItem_Click(object sender, EventArgs e) => InterpolateFrames();
        private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e) => SaveChanges(false);
        private void closeToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void butTbAddAnimation_Click(object sender, EventArgs e) => AddNewAnimation();
        private void butTbDeleteAnimation_Click(object sender, EventArgs e) => DeleteAnimation();
        private void butTbAddFrame_Click(object sender, EventArgs e) => AddNewFrame(panelRendering.CurrentKeyFrame + 1);
        private void butTbCutFrame_Click(object sender, EventArgs e) => CutFrames();
        private void butTbCopyFrame_Click(object sender, EventArgs e) => CopyFrames();
        private void butTbPasteFrame_Click(object sender, EventArgs e) => PasteFrames();
        private void butTbDeleteFrame_Click(object sender, EventArgs e) => DeleteFrames(this);
        private void butTbCutAnimation_Click(object sender, EventArgs e) => CutAnimation();
        private void butTbCopyAnimation_Click(object sender, EventArgs e) => CopyAnimation();
        private void butTbPasteAnimation_Click(object sender, EventArgs e) => PasteAnimation();
        private void butTbReplaceAnimation_Click(object sender, EventArgs e) => ReplaceAnimation();
        private void butTbSplitAnimation_Click(object sender, EventArgs e) => SplitAnimation();
        private void butTbSaveChanges_Click(object sender, EventArgs e) => SaveChanges(false);

        private void butAddNewAnimation_Click(object sender, EventArgs e) => AddNewAnimation();
        private void butDeleteAnimation_Click(object sender, EventArgs e) => DeleteAnimation();
        private void butSearchByStateID_Click(object sender, EventArgs e) => ReloadAnimations();
        private void butCalculateBoundingBoxForCurrentFrame_Click(object sender, EventArgs e) => CalculateKeyframeBoundingBox(panelRendering.CurrentKeyFrame);
        private void butCalculateAnimCollision_Click(object sender, EventArgs e) => CalculateAnimationBoundingBox();
        private void butClearCollisionBox_Click(object sender, EventArgs e) => ClearFrameBoundingBox(false);
        private void butClearAnimCollision_Click(object sender, EventArgs e) => ClearAnimationBoundingBox(false);
        private void insertFramesAfterCurrentToolStripMenuItem_Click(object sender, EventArgs e) => InsertMultipleFrames();
        private void butEditAnimCommands_Click(object sender, EventArgs e) => EditAnimCommands();
        private void butEditStateChanges_Click(object sender, EventArgs e) => EditStateChanges();

        private void tbStateId_Validated(object sender, EventArgs e) => _selectedNode.WadAnimation.StateId = (byte)UpdateAnimationParameter(tbStateId);
        private void tbNextAnimation_Validated(object sender, EventArgs e) => _selectedNode.WadAnimation.NextAnimation = (byte)UpdateAnimationParameter(tbNextAnimation);
        private void tbNextFrame_Validated(object sender, EventArgs e) => _selectedNode.WadAnimation.NextFrame = (byte)UpdateAnimationParameter(tbNextFrame);
        private void tbStartVelocity_Validated(object sender, EventArgs e) => _selectedNode.WadAnimation.StartVelocity = UpdateAnimationParameter(tbStartVelocity);
        private void tbEndVelocity_Validated(object sender, EventArgs e) => _selectedNode.WadAnimation.EndVelocity = UpdateAnimationParameter(tbEndVelocity);
        private void tbLateralStartVelocity_Validated(object sender, EventArgs e) => _selectedNode.WadAnimation.StartLateralVelocity = UpdateAnimationParameter(tbLateralStartVelocity);
        private void tbLateralEndVelocity_Validated(object sender, EventArgs e) => _selectedNode.WadAnimation.EndLateralVelocity = UpdateAnimationParameter(tbLateralEndVelocity);
        private void tbFramerate_Validated(object sender, EventArgs e) => _selectedNode.WadAnimation.FrameRate = (byte)MathC.Clamp(UpdateAnimationParameter(tbFramerate), 1, 255);

        private void timerPlayAnimation_Tick(object sender, EventArgs e)
        {
            if (_selectedNode?.WadAnimation == null || _selectedNode.WadAnimation.KeyFrames.Count <= 1)
                return;

            int realFrameNumber = _selectedNode.WadAnimation.FrameRate * (_selectedNode.DirectXAnimation.KeyFrames.Count - 1) + 1;

            _frameCount++;
            if (_frameCount >= realFrameNumber)
                _frameCount = 0;

            bool isKeyFrame = (_frameCount % (_selectedNode.WadAnimation.FrameRate == 0 ? 1 : _selectedNode.WadAnimation.FrameRate) == 0);

            // Update animation
            if (isKeyFrame)
            {
                if (timeline.Value == timeline.Maximum)
                    timeline.Value = 0;
                else
                    timeline.Value++;
            }

            UpdateStatusLabel();

            // Preview sounds
            if (_previewSounds && _tool.ReferenceLevel != null)
            {
                // This additional counter is used to randomize material index every 3 seconds of playback
                _overallPlaybackCount++;
                if (_overallPlaybackCount > _materialIndexSwitchInterval)
                {
                    _overallPlaybackCount = 0;

                    var listOfMaterialSounds = _tool.ReferenceLevel.Settings.GlobalSoundMap.Where(s => s.Name.IndexOf("FOOTSTEPS_", StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
                    _currentMaterialIndex = listOfMaterialSounds[(new Random()).Next(0, listOfMaterialSounds.Count() - 1)].Id;
                }

                foreach (var ac in _selectedNode.WadAnimation.AnimCommands)
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

                        var soundInfo = _tool.ReferenceLevel.Settings.GlobalSoundMap.FirstOrDefault(soundInfo_ => soundInfo_.Id == idToPlay);
                        if (soundInfo != null)
                        {
                            if (!_tool.ReferenceLevel.Settings.SelectedSounds.Contains(idToPlay))
                                popup.ShowWarning(panelRendering, "Sound info " + idToPlay + " is disabled in level settings.");
                            else
                                try
                                {
                                    // Task.Factory.StartNew(() => WadSoundPlayer.PlaySoundInfo(_level, soundInfo, false));
                                    WadSoundPlayer.PlaySoundInfo(_tool.ReferenceLevel, soundInfo, false);
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
            if (_selectedNode == null)
                return;

            if (saveFileDialogExport.ShowDialog(this) == DialogResult.Cancel)
                return;

            var animationToSave = SaveAnimationChanges(_selectedNode);

            if (!WadActions.ExportAnimationToXml(_moveable, animationToSave, saveFileDialogExport.FileName))
            {
                popup.ShowError(panelRendering, "Can't export current animation to XML file");
                return;
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedNode == null)
                return;

            if (openFileDialogImport.ShowDialog(this) == DialogResult.Cancel)
                return;

            var animation = WadActions.ImportAnimationFromXml(_wad, openFileDialogImport.FileName);
            if (animation == null)
            {
                popup.ShowError(panelRendering, "Can't import a valid animation");
                return;
            }

            if (animation.KeyFrames[0].Angles.Count != _bones.Count)
            {
                popup.ShowError(panelRendering, "You can only import an animation with the same number of bones of the current moveable");
                return;
            }

            if (DarkMessageBox.Show(this, "Do you want to overwrite current animation?", "Import animation",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _selectedNode.WadAnimation = animation;
                _selectedNode.DirectXAnimation = Animation.FromWad2(_bones, animation);
                SelectAnimation(_selectedNode);
                timeline.Value = 0;
                panelRendering.Invalidate();
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
            ReloadAnimations();
        }

        private void deleteCollisionBoxForCurrentFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearFrameBoundingBox(true);
        }

        private void deleteBoundingBoxForAllFramesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearAnimationBoundingBox(true);
        }

        private void formAnimationEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Saved)
            {
                var result = DarkMessageBox.Show(this, "You have unsaved changes. Do you want to save changes to animations?",
                                                 "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    SaveChanges(false);
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

        private Control GetFocusedControl(ContainerControl control) => (control.ActiveControl is ContainerControl ? GetFocusedControl((ContainerControl)control.ActiveControl) : control.ActiveControl);
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape: timeline.ResetSelection(); break;
                case Keys.Left:   timeline.ValueLoopDec(); break;
                case Keys.Right:  timeline.ValueLoopInc(); break;
            }

            // Don't process one-key and shift hotkeys if we're focused on control which allows text input
            var activeControlType = GetFocusedControl(this)?.GetType().Name;
            if  (activeControlType == "DarkTextBox" ||
                 activeControlType == "DarkAutocompleteTextBox" ||
                 activeControlType == "DarkComboBox" ||
                 activeControlType == "DarkListBox" ||
                 activeControlType == "UpDownEdit")
                return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Space: PlayAnimation(); break;
                case Keys.I: timeline.SelectionStart = timeline.Value; break;
                case Keys.O: timeline.SelectionEnd = timeline.Value; break;
                case Keys.Delete: DeleteFrames(this); break;

                case (Keys.Control | Keys.X): CutFrames(); break;
                case (Keys.Control | Keys.C): CopyFrames(); break;
                case (Keys.Control | Keys.V): PasteFrames(); break;
                case (Keys.Control | Keys.S): SaveChanges(true); break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void lstAnimations_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (lstAnimations.SelectedIndices.Count == 0)
                return;

            var node = (AnimationNode)lstAnimations.Items[lstAnimations.SelectedIndices[0]].Tag;
            SelectAnimation(node);
        }

        private void timeline_ValueChanged(object sender, EventArgs e) => SelectFrame(timeline.Value);
        private void timeline_AnimCommandDoubleClick(object sender, WadAnimCommand ac) => EditAnimCommands(ac);

        private void butTransportPlay_Click(object sender, EventArgs e) => PlayAnimation();
        private void butTransportStart_Click(object sender, EventArgs e) => timeline.Value = timeline.Minimum;
        private void butTransportFrameBack_Click(object sender, EventArgs e) => timeline.Value--;
        private void butTransportFrameForward_Click(object sender, EventArgs e) => timeline.Value++;
        private void butTransportEnd_Click(object sender, EventArgs e) => timeline.Value = timeline.Maximum;

        private void butTransportSound_Click(object sender, EventArgs e)
        {
            if (_tool.ReferenceLevel == null && !WadActions.LoadReferenceLevel(_tool, this)) return;

            _previewSounds = !_previewSounds;

            if (_previewSounds)
                butTransportSound.Image = Properties.Resources.transport_audio_24;
            else
                butTransportSound.Image = Properties.Resources.transport_mute_24;
        }

        private void butTransportLandWater_Click(object sender, EventArgs e)
        {
            if (_tool.ReferenceLevel == null && !WadActions.LoadReferenceLevel(_tool, this)) return;

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
            if (e.KeyCode == Keys.Return) ReloadAnimations();
        }

        private void butTbInterpolateFrames_Click(object sender, EventArgs e)
        {
            int frameCount = 3;
            int.TryParse(tbInterpolateFrameCount.Text, out frameCount);
            InterpolateFrames(frameCount);
        }

        private void timeline_SelectionChanged(object sender, EventArgs e)
        {
            UpdateStatusLabel();
        }
    }
}
