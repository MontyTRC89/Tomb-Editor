using DarkUI.Forms;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.GeometryIO;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using System.Diagnostics;
using NLog;
using TombLib.Graphics;

namespace WadTool
{
    public partial class FormMain : DarkForm
    {
        private WadToolClass _tool;
        private DeviceManager _deviceManager;

        public FormMain(WadToolClass tool)
        {
            _deviceManager = new DeviceManager();

            InitializeComponent();

            // Only how debug menu when a debugger is attached...
            debugToolStripMenuItem.Visible = Debugger.IsAttached;


            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _tool = tool;

            panel3D.InitializePanel(_tool, _deviceManager);
            panel3D.AnimationScrollBar = scrollbarAnimations;
            tool.EditorEventRaised += Tool_EditorEventRaised;

            int TODO_MAKE_SURE_WADTOOL_IS_UPFRONT_ABOUT_WAD2_CONTAINING_SOUNDS;
            int TODO_ALLOW_DRAG_DROP;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                _tool.EditorEventRaised -= Tool_EditorEventRaised;
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.DestinationWadChangedEvent)
            {
                if (_tool.DestinationWad != null)
                {
                    _tool.DestinationWad.GraphicsDevice = _deviceManager.Device;
                    _tool.DestinationWad.PrepareDataForDirectX();
                }

                treeDestWad.Wad = _tool.DestinationWad;
                treeDestWad.UpdateContent();

                panel3D.UpdateAnimationScrollbar();
                panel3D.Invalidate();
            }
            else if (obj is WadToolClass.SourceWadChangedEvent)
            {
                if (_tool.SourceWad != null)
                {
                    _tool.SourceWad.GraphicsDevice = _deviceManager.Device;
                    _tool.SourceWad.PrepareDataForDirectX();
                }

                treeSourceWad.Wad = _tool.SourceWad;
                treeSourceWad.UpdateContent();
                
                panel3D.UpdateAnimationScrollbar();
                panel3D.Invalidate();
            }
            else if (obj is WadToolClass.MainSelectionChangedEvent)
            {
                var mainSelection = _tool.MainSelection;
                if (mainSelection == null)
                {
                    panel3D.CurrentWad = null;
                    panel3D.CurrentObjectId = null;
                }
                else
                {
                    panel3D.CurrentWad = _tool.GetWad(mainSelection.Value.WadArea);
                    panel3D.CurrentObjectId = mainSelection.Value.Id;
                    panel3D.AnimationIndex = 0;
                    panel3D.KeyFrameIndex = 0;
                    panel3D.Invalidate();
                }

                panel3D.UpdateAnimationScrollbar();
                panel3D.Invalidate();
            }
        }

        private void openSourceWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butOpenSourceWad_Click(null, null);
        }

        private void treeSourceWad_Click(object sender, EventArgs e)
        {
            IWadObjectId currentSelection = treeSourceWad.SelectedWadObjectIds.FirstOrDefault();
            if (currentSelection != null)
                _tool.MainSelection = new MainSelection { WadArea = WadArea.Source, Id = currentSelection };
        }

        private void treeDestWad_Click(object sender, EventArgs e)
        {
            IWadObjectId currentSelection = treeDestWad.SelectedWadObjectIds.FirstOrDefault();
            if (currentSelection != null)
                _tool.MainSelection = new MainSelection { WadArea = WadArea.Destination, Id = currentSelection };
        }

        private void openDestinationWad2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WadActions.LoadWad(_tool, this, true);
        }

        private void butOpenDestWad_Click(object sender, EventArgs e)
        {
            WadActions.LoadWad(_tool, this, true);
        }

        private void butOpenSourceWad_Click(object sender, EventArgs e)
        {
            WadActions.LoadWad(_tool, this, false);
        }

        private void butAddObject_Click(object sender, EventArgs e)
        {
            WadActions.CopyObject(_tool, this, treeSourceWad.SelectedWadObjectIds.ToList(), false);
        }

        private void butDeleteObject_Click(object sender, EventArgs e)
        {
            foreach (IWadObjectId id in treeDestWad.SelectedWadObjectIds)
                _tool.DestinationWad.Remove(id);
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

            var currentSound = (WadSample)node.Tag;

            currentSound.Play();
        }

        private void butNewWad_Click(object sender, EventArgs e)
        {
            WadActions.CreateNewWad(_tool, this);
        }

        private void butChangeSlot_Click(object sender, EventArgs e)
        {
            WadActions.ChangeSlot(_tool, this);
        }

        private void lstAnimations_Click(object sender, EventArgs e)
        {
            // Get selected object
            if (_tool.MainSelection != null)
                return;
            Wad2 wad = _tool.GetWad(_tool.MainSelection.Value.WadArea);
            WadMoveable moveable = _tool.MainSelection.Value.Id as WadMoveable;
            if (moveable == null)
                return;

            // Get selected animation
            if (treeAnimations.SelectedNodes.Count == 0)
                return;
            var node = treeAnimations.SelectedNodes[0];
            int animationIndex = (int)node.Tag;

            // Reset panel 3D
            panel3D.AnimationIndex = animationIndex;
            panel3D.KeyFrameIndex = 0;
            panel3D.Invalidate();
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
            WadActions.EditObject(_tool, this, _deviceManager);
        }

        private void treeSourceWad_DoubleClick(object sender, EventArgs e)
        {
            WadActions.EditObject(_tool, this, _deviceManager);
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
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    WadActions.DeleteObjects(_tool, this, WadArea.Source, treeSourceWad.SelectedWadObjectIds.ToList());
                    break;
            }
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
        }
    }
}
