﻿using DarkUI.Forms;
using System;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using System.IO;

namespace WadTool
{
    public partial class FormMain : DarkForm
    {
        private readonly WadToolClass _tool;

        private readonly PopUpInfo popup = new PopUpInfo();

        public FormMain(WadToolClass tool)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            // Only how debug menu when a debugger is attached...
            debugToolStripMenuItem.Visible = Debugger.IsAttached;

            _tool = tool;

            panel3D.Configuration = tool.Configuration;
            panel3D.InitializeRendering(DeviceManager.DefaultDeviceManager.Device, tool.Configuration.RenderingItem_Antialias);
            tool.EditorEventRaised += Tool_EditorEventRaised;

            Tool_EditorEventRaised(new InitEvent());

            // Load window size
            Configuration.LoadWindowProperties(this, _tool.Configuration);

            // Try to load reference project on start-up, if specified in config
            if (!string.IsNullOrEmpty(tool.Configuration.Tool_ReferenceProject) &&
                File.Exists(tool.Configuration.Tool_ReferenceProject))
                WadActions.LoadReferenceLevel(tool, this, tool.Configuration.Tool_ReferenceProject);
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
                if (_tool.Configuration.Tool_MakeEmptyWadAtStartup)
                {
                    _tool.DestinationWad = new Wad2 { GameVersion = TRVersion.Game.TR4 };
                    _tool.RaiseEvent(new WadToolClass.DestinationWadChangedEvent());
                }
            }

            if (obj is WadToolClass.MessageEvent)
            {
                var msg = (WadToolClass.MessageEvent)obj;
                PopUpInfo.Show(popup, this, panel3D, msg.Message, msg.Type);
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

                if (_tool.DestinationWad != null)
                {
                    labelStatistics.Text = "Moveables: " + _tool.DestinationWad.Moveables.Count + " | " +
                                           "Statics: " + _tool.DestinationWad.Statics.Count + " | " +
                                           "Sprites sequences: " + _tool.DestinationWad.SpriteSequences.Count + " | " +
                                           "Textures: " + _tool.DestinationWad.MeshTexturesUnique.Count;
                }
                else
                {
                    labelStatistics.Text = "";
                }
            }

            if (obj is WadToolClass.SourceWadChangedEvent || obj is InitEvent)
            {
                panelSource.SectionHeader = "Source";
                if (_tool?.SourceWad != null)
                    panelSource.SectionHeader += (String.IsNullOrEmpty(_tool.SourceWad.FileName) ? " (Imported)" : " (" + Path.GetFileName(_tool.SourceWad.FileName) + ")");

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
                }
                else
                {
                    Wad2 wad = _tool.GetWad(mainSelection.Value.WadArea);

                    // Display the object (or set it to Lara's skin instead if it's Lara)
                    if (mainSelection.Value.Id is WadMoveableId)
                        panel3D.CurrentObject = wad.TryGet(new WadMoveableId(TrCatalog.GetMoveableSkin(wad.GameVersion, ((WadMoveableId)mainSelection.Value.Id).TypeId)));
                    else
                        panel3D.CurrentObject = wad.TryGet(mainSelection.Value.Id);

                    panel3D.AnimationIndex = 0;
                    panel3D.KeyFrameIndex = 0;

                    // Update the toolbar below the rendering area
                    butEditAnimations.Visible = (mainSelection.Value.Id is WadMoveableId);
                    butEditSkeleton.Visible = (mainSelection.Value.Id is WadMoveableId);
                    butEditStaticModel.Visible = (mainSelection.Value.Id is WadStaticId);
                    butEditSpriteSequence.Visible = (mainSelection.Value.Id is WadSpriteSequenceId);

                    panel3D.ResetCamera();
                    panel3D.Invalidate();
                }

                panel3D.UpdateAnimationScrollbar();
                panel3D.Invalidate();
            }

            if (obj is WadToolClass.ReferenceLevelChangedEvent)
            {
                if (_tool.ReferenceLevel != null)
                {
                    butCloseRefLevel.Enabled = true;
                    lblRefLevel.Enabled = true;
                    closeReferenceLevelToolStripMenuItem.Enabled = true;
                    lblRefLevel.Text = Path.GetFileNameWithoutExtension(_tool.ReferenceLevel.Settings.LevelFilePath);
                }
                else
                {
                    butCloseRefLevel.Enabled = false;
                    lblRefLevel.Enabled = false;
                    closeReferenceLevelToolStripMenuItem.Enabled = false;
                    lblRefLevel.Text = "(project not loaded)";
                }
            }

            if (obj is WadToolClass.UnsavedChangesEvent)
            {
                UpdateSaveUI(((WadToolClass.UnsavedChangesEvent)obj).UnsavedChanges);
            }
        }

        private void UpdateSaveUI(bool hasUnsavedChanges)
        {
            Text  = "WadTool";
            if (_tool?.DestinationWad != null)
            {
                var newOrImported = String.IsNullOrEmpty(_tool.DestinationWad.FileName);

                Text += " - ";
                Text += newOrImported ? "Untitled" : _tool.DestinationWad.FileName;
                Text += hasUnsavedChanges ? "*" : "";

                var reallyHasUnsavedChanges = newOrImported || hasUnsavedChanges;

                if (reallyHasUnsavedChanges)
                {
                    butSaveAs.Enabled = true;
                    saveWad2AsToolStripMenuItem.Enabled = true;
                }

                butSave.Enabled = reallyHasUnsavedChanges;
                saveWad2ToolStripMenuItem.Enabled = reallyHasUnsavedChanges;
            }
            else
            {
                butSave.Enabled = false;
                saveWad2ToolStripMenuItem.Enabled = false;
                butSaveAs.Enabled = false;
                saveWad2AsToolStripMenuItem.Enabled = false;
            }
        }

        private DialogResult CheckIfSaved()
        {
            if (butSave.Enabled && !_tool.DestinationWad.WadIsEmpty)
                return DarkMessageBox.Show(this, "You have unsaved changes. Do you want to save changes?",
                                           "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            else
                return DialogResult.OK;
        }

        private void TryMakeNewWad()
        {
            var result = CheckIfSaved();
            if (result == DialogResult.Yes)
                WadActions.SaveWad(_tool, this, _tool.DestinationWad, false);
            else if (result == DialogResult.Cancel)
                return;

            WadActions.CreateNewWad(_tool, this);
        }

        private void TryOpenDestWad()
        {
            var result = CheckIfSaved();
            if (result == DialogResult.Yes)
                WadActions.SaveWad(_tool, this, _tool.DestinationWad, false);
            else if (result == DialogResult.Cancel)
                return;

            WadActions.LoadWadOpenFileDialog(_tool, this, true);
        }

        private void CopyObject(bool otherSlot)
        {
            var result = WadActions.CopyObject(_tool, this, treeSourceWad.SelectedWadObjectIds.ToList(), otherSlot);
            if (result != null)
            {
                if (result.Count > 0)
                    treeDestWad.Select(result);
                else
                    _tool.SendMessage("No objects were copied because they are already in different slots.", PopupType.Warning);
            }
            else
                _tool.SendMessage("No objects were copied.", PopupType.Info);
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
            else if (currentSelection is WadStaticId)
                treeDestWad.ContextMenuStrip = cmStatics;
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
            TryOpenDestWad();
        }

        private void butOpenDestWad_Click(object sender, EventArgs e)
        {
            TryOpenDestWad();
        }

        private void butOpenSourceWad_Click(object sender, EventArgs e)
        {
            WadActions.LoadWadOpenFileDialog(_tool, this, false);
        }

        private void butAddObject_Click(object sender, EventArgs e)
        {
            CopyObject(false);
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
            _tool.WadChanged(WadArea.Destination);
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
            CopyObject(true);
        }

        private void butNewWad_Click(object sender, EventArgs e)
        {
            TryMakeNewWad();
        }

        private void butChangeSlot_Click(object sender, EventArgs e)
        {
            var result = WadActions.ChangeSlot(_tool, this);
            if (result != null)
                treeDestWad.Select(result);
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
            TryMakeNewWad();
        }

        private void treeDestWad_DoubleClick(object sender, EventArgs e)
        {
            if (treeDestWad.ItemSelected)
                butEditItem_Click(null, null);
        }

        private void treeSourceWad_DoubleClick(object sender, EventArgs e)
        {
            if(treeSourceWad.SelectedWadObjectIds.Count() > 0)
                CopyObject(false);
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
            var result = WadActions.CreateObject(_tool, this, new WadMoveable(new WadMoveableId()));
            if (result != null)
                treeDestWad.Select(result);
        }

        private void newStaticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = WadActions.CreateObject(_tool, this, new WadStatic(new WadStaticId()));
            if (result != null)
                treeDestWad.Select(result);
        }

        private void newSpriteSequenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = WadActions.CreateObject(_tool, this, new WadSpriteSequence(new WadSpriteSequenceId()));
            if (result != null)
                treeDestWad.Select(result);
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
            WadActions.EditObject(_tool, this, DeviceManager.DefaultDeviceManager);
        }

        private void butRenameAnimation_Click(object sender, EventArgs e)
        {

        }

        private void butEditSkeleton_Click(object sender, EventArgs e)
        {
            WadActions.EditSkeletion(_tool, this);
        }

        private void butEditAnimations_Click(object sender, EventArgs e)
        {
            WadActions.EditObject(_tool, this, DeviceManager.DefaultDeviceManager);
        }

        private void editSkeletonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.EditSkeletion(_tool, this);
        }

        private void editAnimationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.EditObject(_tool, this, DeviceManager.DefaultDeviceManager);
        }

        private void butEditStaticModel_Click(object sender, EventArgs e)
        {
            WadActions.EditObject(_tool, this, DeviceManager.DefaultDeviceManager);
        }

        private void butEditSpriteSequence_Click(object sender, EventArgs e)
        {
            WadActions.EditObject(_tool, this, DeviceManager.DefaultDeviceManager);
        }

        private void changeSlotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butChangeSlot_Click(null, null);
        }

        private void deleteObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butDeleteObject_Click(null, null);
        }

        private void editObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butEditItem_Click(null, null);
        }

        private void toolStripMenuItemMoveablesChangeSlot_Click(object sender, EventArgs e)
        {
            butChangeSlot_Click(null, null);
        }

        private void toolStripMenuItemMoveablesDelete_Click(object sender, EventArgs e)
        {
            butDeleteObject_Click(null, null);
        }

        private void treeDestWad_ClickOnEmpty(object sender, EventArgs e)
        {
            WadActions.LoadWadOpenFileDialog(_tool, this, true);
        }

        private void treeSourceWad_ClickOnEmpty(object sender, EventArgs e)
        {
            WadActions.LoadWadOpenFileDialog(_tool, this, false);
        }

        private void butOpenRefLevel_Click(object sender, EventArgs e)
        {
            WadActions.LoadReferenceLevel(_tool, this);
        }

        private void openReferenceLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.LoadReferenceLevel(_tool, this);
        }

        private void butCloseRefLevel_Click(object sender, EventArgs e)
        {
            WadActions.UnloadReferenceLevel(_tool);
        }

        private void lblRefLevel_Click(object sender, EventArgs e)
        {
            if (_tool.ReferenceLevel == null) WadActions.LoadReferenceLevel(_tool, this);
        }

        private void closeReferenceLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.UnloadReferenceLevel(_tool);
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormOptions form = new FormOptions(_tool))
                form.ShowDialog(this);

            // FIXME: Later, when WT bloats up, move this to events

            _tool.UndoManager.Resize(_tool.Configuration.AnimationEditor_UndoDepth);
            panel3D.Invalidate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing ||
               e.CloseReason == CloseReason.None)
            {
                var result = CheckIfSaved();
                if (result == DialogResult.Yes)
                {
                    WadActions.SaveWad(_tool, this, _tool.DestinationWad, false);
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                Configuration.SaveWindowProperties(this, _tool.Configuration);
                _tool.Configuration.SaveTry();
            }

            base.OnFormClosing(e);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void ConvertSourceToTR5MainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Wad2 dest = WadActions.ConvertWad2ToTR5Main(_tool, this, _tool.SourceWad);
            Wad2Writer.SaveToFile(dest, "F:\\temp.wad2");
            WadActions.LoadWad(_tool, this, true, "F:\\temp.wad2");
        }
    }
}
