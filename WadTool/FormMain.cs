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
        private bool _playAnimation;

        public FormMain(WadToolClass tool)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            // Only how debug menu when a debugger is attached...
            debugToolStripMenuItem.Visible = Debugger.IsAttached;

            _tool = tool;

            panel3D.Configuration = tool.Configuration;
            panel3D.InitializeRendering(DeviceManager.DefaultDeviceManager.Device);
            panel3D.AnimationScrollBar = scrollbarAnimations;
            tool.EditorEventRaised += Tool_EditorEventRaised;

            Tool_EditorEventRaised(new InitEvent());
        }

        private class InitEvent : IEditorEvent { };

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
            if (obj is InitEvent)
            {
                // At startup initialise a new Wad2
                _tool.DestinationWad = new Wad2 { SuggestedGameVersion = WadGameVersion.TR4_TRNG };
                _tool.RaiseEvent(new WadToolClass.DestinationWadChangedEvent());
            }
            if (obj is WadToolClass.SelectedObjectEditedEvent || obj is InitEvent)
            {
                
            }
            if (obj is WadToolClass.DestinationWadChangedEvent || obj is InitEvent)
            {
                treeDestWad.Wad = _tool.DestinationWad;
                treeDestWad.UpdateContent();

                panel3D.UpdateAnimationScrollbar();
                panel3D.Invalidate();

                Text = "Wad Tool - " + (_tool.DestinationWad?.FileName ?? "New");

                if (_tool.DestinationWad != null)
                {
                    labelStatistics.Text = "Moveables: " + _tool.DestinationWad.Moveables.Count + " | " +
                                           "Statics: " + _tool.DestinationWad.Statics.Count + " | " +
                                           "Sprites sequences: " + _tool.DestinationWad.SpriteSequences.Count + " | " +
                                           "Fixed sounds: " + _tool.DestinationWad.FixedSoundInfos.Count + " | " +
                                           "Additional sounds: " + _tool.DestinationWad.AdditionalSoundInfos.Count + " | " +
                                           "Textures: " + _tool.DestinationWad.MeshTexturesUnique.Count;
                }
                else
                {
                    labelStatistics.Text = "";
                }
            }
            if (obj is WadToolClass.SourceWadChangedEvent || obj is InitEvent)
            {
                treeSourceWad.Wad = _tool.SourceWad;
                treeSourceWad.UpdateContent();

                panel3D.UpdateAnimationScrollbar();
                panel3D.Invalidate();
            }
            if (obj is WadToolClass.MainSelectionChangedEvent ||
                obj is WadToolClass.DestinationWadChangedEvent ||
                obj is WadToolClass.SourceWadChangedEvent || obj is InitEvent)
            {
                var mainSelection = _tool.MainSelection;
                if (mainSelection == null)
                {
                    panel3D.CurrentObject = null;

                    butEditAnimations.Visible = false;
                    butEditSkeleton.Visible = false;
                    butEditStaticModel.Visible = false;
                    butEditSpriteSequence.Visible = false;
                    butEditSound.Visible = false;
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

                    // Update the toolbar below the rendering area
                    butEditAnimations.Visible = (mainSelection.Value.Id is WadMoveableId);
                    butEditSkeleton.Visible = (mainSelection.Value.Id is WadMoveableId);
                    butEditStaticModel.Visible = (mainSelection.Value.Id is WadStaticId);
                    butEditSpriteSequence.Visible = (mainSelection.Value.Id is WadSpriteSequenceId);
                    butEditSound.Visible = (mainSelection.Value.Id is WadFixedSoundInfo ||
                                            mainSelection.Value.Id is WadAdditionalSoundInfoId);

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
            IWadObjectId currentSelection = treeDestWad.SelectedWadObjectIds.FirstOrDefault();
            if (currentSelection != null)
                _tool.MainSelection = new MainSelection { WadArea = WadArea.Destination, Id = currentSelection };

            // Update context menu
            if (currentSelection is WadMoveableId)
                treeDestWad.ContextMenuStrip = contextMenuMoveableItem;
            else
                treeDestWad.ContextMenuStrip = null;
        }

        private void treeSourceWad_SelectedWadObjectIdsChanged(object sender, EventArgs e)
        {
            IWadObjectId currentSelection = treeSourceWad.SelectedWadObjectIds.FirstOrDefault();
            if (currentSelection != null)
                _tool.MainSelection = new MainSelection { WadArea = WadArea.Source, Id = currentSelection };
        }

        private void openDestinationWad2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.LoadWadOpenFileDialog(_tool, this, true);
        }

        private void butOpenDestWad_Click(object sender, EventArgs e)
        {
            WadActions.LoadWadOpenFileDialog(_tool, this, true);
        }

        private void butOpenSourceWad_Click(object sender, EventArgs e)
        {
            WadActions.LoadWadOpenFileDialog(_tool, this, false);
        }

        private void butAddObject_Click(object sender, EventArgs e)
        {
            WadActions.CopyObject(_tool, this, treeSourceWad.SelectedWadObjectIds.ToList(), false);
        }

        private void butDeleteObject_Click(object sender, EventArgs e)
        {
            if (treeDestWad.SelectedWadObjectIds.Count() == 0)
                return;

            if (DarkMessageBox.Show(this, "Do you really want to delete selected objects?", "Delete objects",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            foreach (IWadObjectId id in treeDestWad.SelectedWadObjectIds)
                _tool.DestinationWad.Remove(id);
            _tool.MainSelection = null;
            _tool.DestinationWadChanged();
        }

        private void butSave_Click(object sender, EventArgs e)
        {
            WadActions.SaveWad(_tool, this, _tool.DestinationWad, false);
        }

        private void butSaveAs_Click(object sender, EventArgs e)
        {
            WadActions.SaveWad(_tool, this, _tool.DestinationWad, true);
        }

        private void saveWad2AsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butSaveAs_Click(null, null);
        }

        private void butAddObjectToDifferentSlot_Click(object sender, EventArgs e)
        {
            WadActions.CopyObject(_tool, this, treeSourceWad.SelectedWadObjectIds.ToList(), true);
        }

        private void butNewWad_Click(object sender, EventArgs e)
        {
            WadActions.CreateNewWad(_tool, this);
        }

        private void butChangeSlot_Click(object sender, EventArgs e)
        {
            WadActions.ChangeSlot(_tool, this);
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
            WadActions.CreateNewWad(_tool, this);
        }

        private void treeDestWad_DoubleClick(object sender, EventArgs e)
        {
            WadActions.EditObject(_tool, this, DeviceManager.DefaultDeviceManager);
        }

        private void treeSourceWad_DoubleClick(object sender, EventArgs e)
        {
            WadActions.EditObject(_tool, this, DeviceManager.DefaultDeviceManager);
        }

        private void treeDestWad_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    WadActions.DeleteObjects(_tool, this, WadArea.Destination, treeDestWad.SelectedWadObjectIds.ToList());
                    break;
            }
        }

        private void treeSourceWad_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void newMoveableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.CreateObject(_tool, this, new WadMoveable(new WadMoveableId()));
        }

        private void newStaticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.CreateObject(_tool, this, new WadStatic(new WadStaticId()));
        }

        private void newSpriteSequenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.CreateObject(_tool, this, new WadSpriteSequence(new WadSpriteSequenceId()));
        }

        private void newFixedSoundInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
            using (var form = new FormMesh(_tool, DeviceManager.DefaultDeviceManager, _tool.DestinationWad))
                form.ShowDialog(this);
        }

        private void butEditItem_Click(object sender, EventArgs e)
        {

        }

        private void butRenameAnimation_Click(object sender, EventArgs e)
        {

        }

        private void butRenameSound_Click(object sender, EventArgs e)
        {

        }

        private void destinationSoundInfoOverviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.ShowSoundOverview(_tool, this, WadArea.Destination);
        }

        private void sourceSoundInfoOverviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.ShowSoundOverview(_tool, this, WadArea.Source);
        }

        private void butEditSkeleton_Click(object sender, EventArgs e)
        {
            WadActions.EditSkeletion(_tool, this);
        }

        private void butEditAnimations_Click(object sender, EventArgs e)
        {
            WadActions.EditAnimations(_tool, this);
        }

        private void editSkeletonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.EditSkeletion(_tool, this);
        }

        private void editAnimationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.EditAnimations(_tool, this);
        }

        private void butEditStaticModel_Click(object sender, EventArgs e)
        {
            WadActions.EditObject(_tool, this, DeviceManager.DefaultDeviceManager);
        }

        private void butEditSpriteSequence_Click(object sender, EventArgs e)
        {
            WadActions.EditObject(_tool, this, DeviceManager.DefaultDeviceManager);
        }

        private void butEditSound_Click(object sender, EventArgs e)
        {
            WadActions.EditObject(_tool, this, DeviceManager.DefaultDeviceManager);
        }
    }
}
