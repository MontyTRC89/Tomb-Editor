using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.Utils;

namespace WadTool
{
    public partial class FormMain : DarkForm
    {
        private readonly WadToolClass _tool;
        private readonly DeviceManager _deviceManager;
        private bool _playAnimation;

        public FormMain(WadToolClass tool)
        {
            _deviceManager = new DeviceManager();

            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            // Only how debug menu when a debugger is attached...
            debugToolStripMenuItem.Visible = Debugger.IsAttached;

            _tool = tool;

            panel3D.Configuration = tool.Configuration;
            panel3D.InitializePanel(_deviceManager);
            panel3D.AnimationScrollBar = scrollbarAnimations;
            tool.EditorEventRaised += Tool_EditorEventRaised;

            int TODO_ALLOW_DRAG_DROP;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                _tool.EditorEventRaised -= Tool_EditorEventRaised;
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.SelectedObjectEditedEvent)
            {

            }
            if (obj is WadToolClass.DestinationWadChangedEvent)
            {
                treeDestWad.Wad = _tool.DestinationWad;
                treeDestWad.UpdateContent();

                panel3D.UpdateAnimationScrollbar();
                panel3D.Invalidate();
            }
            if (obj is WadToolClass.SourceWadChangedEvent)
            {
                treeSourceWad.Wad = _tool.SourceWad;
                treeSourceWad.UpdateContent();

                panel3D.UpdateAnimationScrollbar();
                panel3D.Invalidate();
            }
            if (obj is WadToolClass.MainSelectionChangedEvent ||
                obj is WadToolClass.DestinationWadChangedEvent ||
                obj is WadToolClass.SourceWadChangedEvent)
            {
                var mainSelection = _tool.MainSelection;
                if (mainSelection == null)
                {
                    panel3D.CurrentObject = null;
                }
                else
                {
                    Wad2 wad = _tool.GetWad(mainSelection.Value.WadArea);

                    // Display the object (or set it to Lara's skin instead if it's Lara)
                    if (mainSelection.Value.Id is WadMoveableId &&
                        ((WadMoveableId)mainSelection.Value.Id) == WadMoveableId.Lara &&
                        (wad.SuggestedGameVersion == WadGameVersion.TR4_TRNG || wad.SuggestedGameVersion == WadGameVersion.TR5) &&
                        wad.Moveables.ContainsKey(WadMoveableId.LaraSkin))
                    {
                        panel3D.CurrentObject = wad.TryGet(WadMoveableId.LaraSkin);
                    }
                    else
                    {
                        panel3D.CurrentObject = wad.TryGet(mainSelection.Value.Id);
                    }
                    panel3D.AnimationIndex = 0;
                    panel3D.KeyFrameIndex = 0;

                    // Update animations list
                    if (mainSelection.Value.Id is WadMoveableId)
                    {
                        var moveableId = (WadMoveableId)mainSelection.Value.Id;
                        var moveable = wad.Moveables[moveableId];
                        var animationsNodes = new List<DarkTreeNode>();
                        for (int i = 0; i < moveable.Animations.Count; i++)
                        {
                            var nodeAnimation = new DarkTreeNode(moveable.Animations[i].Name);
                            nodeAnimation.Tag = i;
                            animationsNodes.Add(nodeAnimation);
                        }
                        treeAnimations.Nodes.Clear();
                        treeAnimations.Nodes.AddRange(animationsNodes);
                    }

                    panel3D.Invalidate();
                }

                panel3D.UpdateAnimationScrollbar();
                panel3D.Invalidate();
            }
        }

        private void Panel3D_ObjectWasModified(object sender, System.EventArgs e)
        {
        }

        private void openSourceWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butOpenSourceWad_Click(null, null);
        }

        private void treeDestWad_SelectedWadObjectIdsChanged(object sender, EventArgs e)
        {
            StopAnimation();
            IWadObjectId currentSelection = treeDestWad.SelectedWadObjectIds.FirstOrDefault();
            if (currentSelection != null)
                _tool.MainSelection = new MainSelection { WadArea = WadArea.Destination, Id = currentSelection };
        }

        private void treeSourceWad_SelectedWadObjectIdsChanged(object sender, EventArgs e)
        {
            StopAnimation();
            IWadObjectId currentSelection = treeSourceWad.SelectedWadObjectIds.FirstOrDefault();
            if (currentSelection != null)
                _tool.MainSelection = new MainSelection { WadArea = WadArea.Source, Id = currentSelection };
        }

        private void openDestinationWad2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.LoadWad(_tool, this, true);
        }

        private void butOpenDestWad_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.LoadWad(_tool, this, true);
        }

        private void butOpenSourceWad_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.LoadWad(_tool, this, false);
        }

        private void butAddObject_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.CopyObject(_tool, this, treeSourceWad.SelectedWadObjectIds.ToList(), false);
        }

        private void butDeleteObject_Click(object sender, EventArgs e)
        {
            if (treeDestWad.SelectedWadObjectIds.Count() == 0)
                return;

            if (DarkMessageBox.Show(this, "Do you really want to delete selected objects?", "Delete objects",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            StopAnimation();
            foreach (IWadObjectId id in treeDestWad.SelectedWadObjectIds)
                _tool.DestinationWad.Remove(id);
            _tool.MainSelection = null;
            _tool.DestinationWadChanged();
        }

        private void butSave_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.SaveWad(_tool, this, _tool.DestinationWad, false);
        }

        private void butSaveAs_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.SaveWad(_tool, this, _tool.DestinationWad, true);
        }

        private void saveWad2AsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopAnimation();
            butSaveAs_Click(null, null);
        }

        private void butAddObjectToDifferentSlot_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.CopyObject(_tool, this, treeSourceWad.SelectedWadObjectIds.ToList(), true);
        }

        private void butPlaySound_Click(object sender, EventArgs e)
        {
            if (_tool.DestinationWad == null)
            {
                DarkMessageBox.Show(this, "You must load or create a new destination Wad2", "Error", MessageBoxIcon.Error);
                return;
            }

            // Get the selected sound
            if (treeSounds.SelectedNodes.Count == 0)
                return;
            var node = treeSounds.SelectedNodes[0];
            if (node.Tag == null || !(node.Tag is WadSample))
                return;

            var currentSample = (WadSample)node.Tag;
            WadSoundPlayer.PlaySample(currentSample);
        }

        private void StopAnimation()
        {
            timerPlayAnimation.Stop();
            _playAnimation = false;
            butPlayAnimation.Enabled = true;
            butStop.Enabled = false;
        }

        private void butNewWad_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.CreateNewWad(_tool, this);
        }

        private void butChangeSlot_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.ChangeSlot(_tool, this);
        }

        private void lstAnimations_Click(object sender, EventArgs e)
        {
            Wad2 wad = _tool.GetWad(_tool.MainSelection.Value.WadArea);
            var moveableId = (WadMoveableId)_tool.MainSelection.Value.Id;
            var moveable = wad.Moveables[moveableId];
            if (moveable == null)
                return;

            // Get selected animation
            if (treeAnimations.SelectedNodes.Count == 0)
                return;
            var node = treeAnimations.SelectedNodes[0];
            int animationIndex = (int)node.Tag;
            var animation = moveable.Animations[animationIndex];

            // Reset scrollbar
            scrollbarAnimations.Value = 0;
            scrollbarAnimations.Maximum = animation.RealNumberOfFrames - 1;

            // Reset panel 3D
            panel3D.AnimationIndex = animationIndex;
            panel3D.KeyFrameIndex = 0;

            if (_playAnimation)
            {
                int frameRate = Math.Max(animation.FrameRate, (short)1);
                timerPlayAnimation.Stop();
                timerPlayAnimation.Interval = 1000 / 30 / frameRate;
                timerPlayAnimation.Start();
            }

            panel3D.Draw();
        }

        private void importModelAsStaticMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.ImportModelAsStaticMesh(_tool, this);
        }

        private void aboutWadToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormAbout(Properties.Resources.AboutScreen_800))
                form.ShowDialog(this);
        }

        private void newWad2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.CreateNewWad(_tool, this);
        }

        private void treeDestWad_DoubleClick(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.EditObject(_tool, this, _deviceManager);
        }

        private void treeSourceWad_DoubleClick(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.EditObject(_tool, this, _deviceManager);
        }

        private void treeDestWad_KeyDown(object sender, KeyEventArgs e)
        {
            StopAnimation();
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    WadActions.DeleteObjects(_tool, this, WadArea.Destination, treeDestWad.SelectedWadObjectIds.ToList());
                    break;
            }
        }

        private void treeSourceWad_KeyDown(object sender, KeyEventArgs e)
        {
            StopAnimation();
        }

        private void newMoveableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.CreateObject(_tool, this, new WadMoveable(new WadMoveableId()));
        }

        private void newStaticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.CreateObject(_tool, this, new WadStatic(new WadStaticId()));
        }

        private void newSpriteSequenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.CreateObject(_tool, this, new WadSpriteSequence(new WadSpriteSequenceId()));
        }

        private void newFixedSoundInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopAnimation();
            WadActions.CreateObject(_tool, this, new WadFixedSoundInfo(new WadFixedSoundInfoId()));
        }

        private void debugAction0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void debugAction1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void debugAction2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void debugAction3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void debugAction4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void debugAction5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TrCatalog.LoadCatalog("Editor\\TRCatalog.xml");
        }

        private void debugAction6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void debugAction7ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void debugAction8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void debugAction9ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormMesh(_tool, _deviceManager, _tool.DestinationWad))
                form.ShowDialog();
        }

        private void timerPlayAnimation_Tick(object sender, EventArgs e)
        {
            if (_playAnimation)
            {
                // Get selected moveable
                var wad = _tool.GetWad(_tool.MainSelection.Value.WadArea);
                var moveableId = (WadMoveableId)_tool.MainSelection.Value.Id;
                var moveable = wad.Moveables[moveableId];
                if (moveable == null || moveable.Animations.Count == 0)
                {
                    _playAnimation = false;
                    return;
                }

                // Get selected animation
                if (treeAnimations.SelectedNodes.Count == 0)
                    return;
                var node = treeAnimations.SelectedNodes[0];
                var animationIndex = (int)node.Tag;
                if (animationIndex >= moveable.Animations.Count)
                {
                    _playAnimation = false;
                    return;
                }
                var animation = moveable.Animations[animationIndex];

                // Update animation
                if (panel3D.KeyFrameIndex >= animation.RealNumberOfFrames)
                    panel3D.KeyFrameIndex = 0;
                else
                    panel3D.KeyFrameIndex++;
                panel3D.Draw();
            }
        }

        private void butPlayAnimation_Click(object sender, EventArgs e)
        {
            // Get selected moveable
            var wad = _tool.GetWad(_tool.MainSelection.Value.WadArea);
            var moveableId = (WadMoveableId)_tool.MainSelection.Value.Id;
            var moveable = wad.Moveables[moveableId];
            if (moveable == null || moveable.Animations.Count == 0)
            {
                _playAnimation = false;
                return;
            }

            // Get selected animation
            if (treeAnimations.SelectedNodes.Count == 0)
                return;
            var node = treeAnimations.SelectedNodes[0];
            var animationIndex = (int)node.Tag;
            if (animationIndex >= moveable.Animations.Count)
            {
                _playAnimation = false;
                return;
            }
            var animation = moveable.Animations[animationIndex];

            panel3D.AnimationIndex = animationIndex;
            panel3D.KeyFrameIndex = 0;

            // Start animation
            _playAnimation = true;
            int frameRate = Math.Max(animation.FrameRate, (short)1);
            timerPlayAnimation.Stop();
            timerPlayAnimation.Interval = 1000 / 30 / frameRate;
            timerPlayAnimation.Start();

            butPlayAnimation.Enabled = false;
            butStop.Enabled = true;
        }

        private void butStop_Click(object sender, EventArgs e)
        {
            _playAnimation = false;
            timerPlayAnimation.Stop();
            butStop.Enabled = false;
            butPlayAnimation.Enabled = true;
        }

        private void butEditItem_Click(object sender, EventArgs e)
        {
            StopAnimation();
        }

        private void butRenameAnimation_Click(object sender, EventArgs e)
        {
            StopAnimation();
        }

        private void butRenameSound_Click(object sender, EventArgs e)
        {
            StopAnimation();
        }

        private void soundInfoOverviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.ShowSoundOverview(_tool, this, WadArea.Destination);
        }

        private void darkButton1_Click(object sender, EventArgs e)
        {
            var wad = _tool.GetWad(_tool.MainSelection.Value.WadArea);
            var moveableId = (WadMoveableId)_tool.MainSelection.Value.Id;
            using (var form = new FormSkeletonEditor(_tool, _deviceManager, wad, moveableId))
            {
                if (form.ShowDialog() != DialogResult.OK)
                    return;
                _tool.SelectedObjectEdited();
            }
        }

        private void darkButton3_Click(object sender, EventArgs e)
        {
            var wad = _tool.GetWad(_tool.MainSelection.Value.WadArea);
            var moveableId = (WadMoveableId)_tool.MainSelection.Value.Id;
            using (var form = new FormAnimationEditor(_tool, _deviceManager, wad, moveableId))
            {
                if (form.ShowDialog() != DialogResult.OK)
                    return;
                _tool.SelectedObjectEdited();
            }
        }
    }
}
