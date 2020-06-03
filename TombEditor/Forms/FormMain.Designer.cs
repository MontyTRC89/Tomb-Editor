using System.Windows.Forms;

namespace TombEditor.Forms
{
    partial class FormMain : DarkUI.Forms.DarkForm
    {
        #region Windows Form Designer generated code

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip = new DarkUI.Controls.DarkMenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openRecentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.importTRLEPRJToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.buildLevelPlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buildLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stampToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.bookmarkObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bookmarkRestoreObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.editObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchAndReplaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.relocateCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleFlyModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.drawWhiteTextureLightingOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowRealTintForObjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.roomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newRoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newRoomUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newRoomDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newRoomLeftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newRoomRightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newRoomFrontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newRoomBackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateRoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cropRoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitRoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeRoomsHorizontallyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.moveRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wholeRoomUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveRoomUp4ClicksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wholeRoomDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveRoomDown4ClicksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveRoomLeftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveRoomRightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveRoomForwardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveRoomBackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.transformRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotateRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotateRoomsCountercockwiseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mirrorRoomsOnXAxisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mirrorRoomsOnZAxisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectWaterRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectSkyRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectOutsideRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.selectConnectedRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectRoomsByTagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exportRoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.applyCurrentAmbientLightToAllRoomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addWadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeWadsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadWadsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadSoundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.addCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFlybyCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSinkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSoundSourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addImportedGeometryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addGhostBlockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addPortalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addTriggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.addSphereVolumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addPrismVolumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addBoxVolumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.findObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveLaraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitSectorObjectOnSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteAllTriggersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllObjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.setStaticMeshColorToRoomLightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setStaticMeshesColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeQuickItemGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.texturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unloadTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importConvertTexturesToPng = new System.Windows.Forms.ToolStripMenuItem();
            this.remapTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.textureFloorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textureWallsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textureCeilingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.clearAllTexturesInRoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearAllTexturesInRoomToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.findUntexturedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.animationRangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.transformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smoothRandomFloorUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smoothRandomFloorDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smoothRandomCeilingUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smoothRandomCeilingDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.sharpRandomFloorUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sharpRandomFloorDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sharpRandomCeilingUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sharpRandomCeilingDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.averageFloorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.averageCeilingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flattenFloorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flattenCeilingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetGeometryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.gridWallsIn3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gridWallsIn5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gridWallsIn3SquaresToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gridWallsIn5SquaresToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.editOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keyboardLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreDefaultLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.sectorOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.roomOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importedGeometryBrowserToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.triggerListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lightingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.paletteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.texturePanelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolPaletteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dockableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.floatingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.butRoomDown = new DarkUI.Controls.DarkButton();
            this.butEditRoomName = new DarkUI.Controls.DarkButton();
            this.butDeleteRoom = new DarkUI.Controls.DarkButton();
            this.statusStrip = new DarkUI.Controls.DarkStatusStrip();
            this.statusStripSelectedRoom = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStripGlobalSelectionArea = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStripLocalSelectionArea = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusAutosave = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusLastCompilation = new System.Windows.Forms.ToolStripStatusLabel();
            this.dockArea = new DarkUI.Docking.DarkDockPanel();
            this.panelDockArea = new System.Windows.Forms.Panel();
            this.assToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.panelDockArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.menuStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.roomsToolStripMenuItem,
            this.itemsToolStripMenuItem,
            this.texturesToolStripMenuItem,
            this.transformToolStripMenuItem,
            this.toolStripMenuItem4,
            this.debugToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(913, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "darkMenuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newLevelToolStripMenuItem,
            this.openLevelToolStripMenuItem,
            this.openRecentToolStripMenuItem,
            this.saveLevelToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripMenuSeparator1,
            this.importTRLEPRJToolStripMenuItem,
            this.toolStripMenuSeparator2,
            this.buildLevelPlayToolStripMenuItem,
            this.buildLevelToolStripMenuItem,
            this.toolStripMenuSeparator3,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(31, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newLevelToolStripMenuItem
            // 
            this.newLevelToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.newLevelToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.newLevelToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_create_new_16;
            this.newLevelToolStripMenuItem.Name = "newLevelToolStripMenuItem";
            this.newLevelToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.newLevelToolStripMenuItem.Tag = "NewLevel";
            this.newLevelToolStripMenuItem.Text = "NewLevel";
            // 
            // openLevelToolStripMenuItem
            // 
            this.openLevelToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.openLevelToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.openLevelToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_Open_16;
            this.openLevelToolStripMenuItem.Name = "openLevelToolStripMenuItem";
            this.openLevelToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.openLevelToolStripMenuItem.Tag = "OpenLevel";
            this.openLevelToolStripMenuItem.Text = "OpenLevel";
            // 
            // openRecentToolStripMenuItem
            // 
            this.openRecentToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.openRecentToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.openRecentToolStripMenuItem.Name = "openRecentToolStripMenuItem";
            this.openRecentToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.openRecentToolStripMenuItem.Text = "Open recent";
            // 
            // saveLevelToolStripMenuItem
            // 
            this.saveLevelToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.saveLevelToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.saveLevelToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_Save_16;
            this.saveLevelToolStripMenuItem.Name = "saveLevelToolStripMenuItem";
            this.saveLevelToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.saveLevelToolStripMenuItem.Tag = "SaveLevel";
            this.saveLevelToolStripMenuItem.Text = "SaveLevel";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.saveAsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.saveAsToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_Save_As_16;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.saveAsToolStripMenuItem.Tag = "SaveLevelAs";
            this.saveAsToolStripMenuItem.Text = "SaveLevelAs";
            // 
            // toolStripMenuSeparator1
            // 
            this.toolStripMenuSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuSeparator1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuSeparator1.Name = "toolStripMenuSeparator1";
            this.toolStripMenuSeparator1.Size = new System.Drawing.Size(125, 6);
            // 
            // importTRLEPRJToolStripMenuItem
            // 
            this.importTRLEPRJToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.importTRLEPRJToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.importTRLEPRJToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_Import_16;
            this.importTRLEPRJToolStripMenuItem.Name = "importTRLEPRJToolStripMenuItem";
            this.importTRLEPRJToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.importTRLEPRJToolStripMenuItem.Tag = "ImportPrj";
            this.importTRLEPRJToolStripMenuItem.Text = "ImportPrj";
            // 
            // toolStripMenuSeparator2
            // 
            this.toolStripMenuSeparator2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuSeparator2.Name = "toolStripMenuSeparator2";
            this.toolStripMenuSeparator2.Size = new System.Drawing.Size(125, 6);
            // 
            // buildLevelPlayToolStripMenuItem
            // 
            this.buildLevelPlayToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.buildLevelPlayToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.buildLevelPlayToolStripMenuItem.Image = global::TombEditor.Properties.Resources.actions_play_16;
            this.buildLevelPlayToolStripMenuItem.Name = "buildLevelPlayToolStripMenuItem";
            this.buildLevelPlayToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.buildLevelPlayToolStripMenuItem.Tag = "BuildAndPlay";
            this.buildLevelPlayToolStripMenuItem.Text = "BuildAndPlay";
            // 
            // buildLevelToolStripMenuItem
            // 
            this.buildLevelToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.buildLevelToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.buildLevelToolStripMenuItem.Image = global::TombEditor.Properties.Resources.actions_compile_16;
            this.buildLevelToolStripMenuItem.Name = "buildLevelToolStripMenuItem";
            this.buildLevelToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.buildLevelToolStripMenuItem.Tag = "BuildLevel";
            this.buildLevelToolStripMenuItem.Text = "BuildLevel";
            // 
            // toolStripMenuSeparator3
            // 
            this.toolStripMenuSeparator3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuSeparator3.Name = "toolStripMenuSeparator3";
            this.toolStripMenuSeparator3.Size = new System.Drawing.Size(125, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.exitToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.exitToolStripMenuItem.Tag = "QuitEditor";
            this.exitToolStripMenuItem.Text = "QuitEditor";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator2,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.stampToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.selectAllToolStripMenuItem,
            this.toolStripSeparator4,
            this.bookmarkObjectToolStripMenuItem,
            this.bookmarkRestoreObjectToolStripMenuItem,
            this.toolStripSeparator1,
            this.editObjectToolStripMenuItem,
            this.searchToolStripMenuItem,
            this.searchAndReplaceToolStripMenuItem});
            this.editToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(33, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.undoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.undoToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_undo_16;
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.undoToolStripMenuItem.Tag = "Undo";
            this.undoToolStripMenuItem.Text = "Undo";
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.redoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.redoToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_redo_16;
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.redoToolStripMenuItem.Tag = "Redo";
            this.redoToolStripMenuItem.Text = "Redo";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(181, 6);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cutToolStripMenuItem.Enabled = false;
            this.cutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.cutToolStripMenuItem.Tag = "Cut";
            this.cutToolStripMenuItem.Text = "Cut";
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.copyToolStripMenuItem.Enabled = false;
            this.copyToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.copyToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_copy_16;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.copyToolStripMenuItem.Tag = "Copy";
            this.copyToolStripMenuItem.Text = "Copy";
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pasteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pasteToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_clipboard_16;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.pasteToolStripMenuItem.Tag = "Paste";
            this.pasteToolStripMenuItem.Text = "Paste";
            // 
            // stampToolStripMenuItem
            // 
            this.stampToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.stampToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.stampToolStripMenuItem.Image = global::TombEditor.Properties.Resources.actions_rubber_stamp_16;
            this.stampToolStripMenuItem.Name = "stampToolStripMenuItem";
            this.stampToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.stampToolStripMenuItem.Tag = "StampObject";
            this.stampToolStripMenuItem.Text = "StampObject";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.deleteToolStripMenuItem.Enabled = false;
            this.deleteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.deleteToolStripMenuItem.Tag = "Delete";
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.selectAllToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.selectAllToolStripMenuItem.Tag = "SelectAll";
            this.selectAllToolStripMenuItem.Text = "SelectAll";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(181, 6);
            // 
            // bookmarkObjectToolStripMenuItem
            // 
            this.bookmarkObjectToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.bookmarkObjectToolStripMenuItem.Enabled = false;
            this.bookmarkObjectToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.bookmarkObjectToolStripMenuItem.Name = "bookmarkObjectToolStripMenuItem";
            this.bookmarkObjectToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.bookmarkObjectToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.bookmarkObjectToolStripMenuItem.Tag = "BookmarkObject";
            this.bookmarkObjectToolStripMenuItem.Text = "BookmarkObject";
            // 
            // bookmarkRestoreObjectToolStripMenuItem
            // 
            this.bookmarkRestoreObjectToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.bookmarkRestoreObjectToolStripMenuItem.Enabled = false;
            this.bookmarkRestoreObjectToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.bookmarkRestoreObjectToolStripMenuItem.Name = "bookmarkRestoreObjectToolStripMenuItem";
            this.bookmarkRestoreObjectToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.bookmarkRestoreObjectToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.bookmarkRestoreObjectToolStripMenuItem.Tag = "SelectBookmarkedObject";
            this.bookmarkRestoreObjectToolStripMenuItem.Text = "SelectBookmarkedObject";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(181, 6);
            // 
            // editObjectToolStripMenuItem
            // 
            this.editObjectToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.editObjectToolStripMenuItem.Enabled = false;
            this.editObjectToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.editObjectToolStripMenuItem.Name = "editObjectToolStripMenuItem";
            this.editObjectToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.editObjectToolStripMenuItem.Tag = "EditObject";
            this.editObjectToolStripMenuItem.Text = "EditObject";
            // 
            // searchToolStripMenuItem
            // 
            this.searchToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.searchToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.searchToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_search_16;
            this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            this.searchToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.searchToolStripMenuItem.Tag = "Search";
            this.searchToolStripMenuItem.Text = "Search";
            // 
            // searchAndReplaceToolStripMenuItem
            // 
            this.searchAndReplaceToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.searchAndReplaceToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.searchAndReplaceToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_search_and_replace_16;
            this.searchAndReplaceToolStripMenuItem.Name = "searchAndReplaceToolStripMenuItem";
            this.searchAndReplaceToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.searchAndReplaceToolStripMenuItem.Tag = "SearchAndReplaceObjects";
            this.searchAndReplaceToolStripMenuItem.Text = "SearchAndReplaceObjects";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetCameraToolStripMenuItem,
            this.relocateCameraToolStripMenuItem,
            this.toggleFlyModeToolStripMenuItem,
            this.toolStripSeparator8,
            this.drawWhiteTextureLightingOnlyToolStripMenuItem,
            this.ShowRealTintForObjectsToolStripMenuItem});
            this.viewToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // resetCameraToolStripMenuItem
            // 
            this.resetCameraToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.resetCameraToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.resetCameraToolStripMenuItem.Image = global::TombEditor.Properties.Resources.actions_center_direction_16;
            this.resetCameraToolStripMenuItem.Name = "resetCameraToolStripMenuItem";
            this.resetCameraToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.resetCameraToolStripMenuItem.Tag = "ResetCamera";
            this.resetCameraToolStripMenuItem.Text = "ResetCamera";
            // 
            // relocateCameraToolStripMenuItem
            // 
            this.relocateCameraToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.relocateCameraToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.relocateCameraToolStripMenuItem.Name = "relocateCameraToolStripMenuItem";
            this.relocateCameraToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.relocateCameraToolStripMenuItem.Tag = "RelocateCamera";
            this.relocateCameraToolStripMenuItem.Text = "RelocateCamera";
            // 
            // toggleFlyModeToolStripMenuItem
            // 
            this.toggleFlyModeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toggleFlyModeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toggleFlyModeToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_airplane_16;
            this.toggleFlyModeToolStripMenuItem.Name = "toggleFlyModeToolStripMenuItem";
            this.toggleFlyModeToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.toggleFlyModeToolStripMenuItem.Tag = "ToggleFlyMode";
            this.toggleFlyModeToolStripMenuItem.Text = "ToggleFlyMode";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator8.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(203, 6);
            // 
            // drawWhiteTextureLightingOnlyToolStripMenuItem
            // 
            this.drawWhiteTextureLightingOnlyToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.drawWhiteTextureLightingOnlyToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.drawWhiteTextureLightingOnlyToolStripMenuItem.Image = global::TombEditor.Properties.Resources.actions_DrawUntexturedLights_16;
            this.drawWhiteTextureLightingOnlyToolStripMenuItem.Name = "drawWhiteTextureLightingOnlyToolStripMenuItem";
            this.drawWhiteTextureLightingOnlyToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.drawWhiteTextureLightingOnlyToolStripMenuItem.Tag = "DrawWhiteTextureLightingOnly";
            this.drawWhiteTextureLightingOnlyToolStripMenuItem.Text = "DrawWhiteTextureLightingOnly";
            // 
            // ShowRealTintForObjectsToolStripMenuItem
            // 
            this.ShowRealTintForObjectsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.ShowRealTintForObjectsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.ShowRealTintForObjectsToolStripMenuItem.Image = global::TombEditor.Properties.Resources.actions_StaticTint_16;
            this.ShowRealTintForObjectsToolStripMenuItem.Name = "ShowRealTintForObjectsToolStripMenuItem";
            this.ShowRealTintForObjectsToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.ShowRealTintForObjectsToolStripMenuItem.Tag = "ShowRealTintForObjects";
            this.ShowRealTintForObjectsToolStripMenuItem.Text = "ShowRealTintForObjects";
            // 
            // roomsToolStripMenuItem
            // 
            this.roomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.roomsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newRoomToolStripMenuItem,
            this.duplicateRoomToolStripMenuItem,
            this.cropRoomToolStripMenuItem,
            this.splitRoomToolStripMenuItem,
            this.mergeRoomsHorizontallyToolStripMenuItem,
            this.deleteRoomsToolStripMenuItem,
            this.toolStripMenuSeparator5,
            this.moveRoomsToolStripMenuItem,
            this.transformRoomsToolStripMenuItem,
            this.selectRoomsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exportRoomToolStripMenuItem,
            this.importRoomsToolStripMenuItem,
            this.toolStripMenuItem2,
            this.applyCurrentAmbientLightToAllRoomsToolStripMenuItem});
            this.roomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.roomsToolStripMenuItem.Name = "roomsToolStripMenuItem";
            this.roomsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.roomsToolStripMenuItem.Text = "Rooms";
            // 
            // newRoomToolStripMenuItem
            // 
            this.newRoomToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.newRoomToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newRoomUpToolStripMenuItem,
            this.newRoomDownToolStripMenuItem,
            this.newRoomLeftToolStripMenuItem,
            this.newRoomRightToolStripMenuItem,
            this.newRoomFrontToolStripMenuItem,
            this.newRoomBackToolStripMenuItem});
            this.newRoomToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.newRoomToolStripMenuItem.Name = "newRoomToolStripMenuItem";
            this.newRoomToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.newRoomToolStripMenuItem.Text = "New room";
            // 
            // newRoomUpToolStripMenuItem
            // 
            this.newRoomUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.newRoomUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.newRoomUpToolStripMenuItem.Name = "newRoomUpToolStripMenuItem";
            this.newRoomUpToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.newRoomUpToolStripMenuItem.Tag = "NewRoomUp";
            this.newRoomUpToolStripMenuItem.Text = "NewRoomUp";
            // 
            // newRoomDownToolStripMenuItem
            // 
            this.newRoomDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.newRoomDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.newRoomDownToolStripMenuItem.Name = "newRoomDownToolStripMenuItem";
            this.newRoomDownToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.newRoomDownToolStripMenuItem.Tag = "NewRoomDown";
            this.newRoomDownToolStripMenuItem.Text = "NewRoomDown";
            // 
            // newRoomLeftToolStripMenuItem
            // 
            this.newRoomLeftToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.newRoomLeftToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.newRoomLeftToolStripMenuItem.Name = "newRoomLeftToolStripMenuItem";
            this.newRoomLeftToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.newRoomLeftToolStripMenuItem.Tag = "NewRoomLeft";
            this.newRoomLeftToolStripMenuItem.Text = "NewRoomLeft";
            // 
            // newRoomRightToolStripMenuItem
            // 
            this.newRoomRightToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.newRoomRightToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.newRoomRightToolStripMenuItem.Name = "newRoomRightToolStripMenuItem";
            this.newRoomRightToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.newRoomRightToolStripMenuItem.Tag = "NewRoomRight";
            this.newRoomRightToolStripMenuItem.Text = "NewRoomRight";
            // 
            // newRoomFrontToolStripMenuItem
            // 
            this.newRoomFrontToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.newRoomFrontToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.newRoomFrontToolStripMenuItem.Name = "newRoomFrontToolStripMenuItem";
            this.newRoomFrontToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.newRoomFrontToolStripMenuItem.Tag = "NewRoomFront";
            this.newRoomFrontToolStripMenuItem.Text = "NewRoomFront";
            // 
            // newRoomBackToolStripMenuItem
            // 
            this.newRoomBackToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.newRoomBackToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.newRoomBackToolStripMenuItem.Name = "newRoomBackToolStripMenuItem";
            this.newRoomBackToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.newRoomBackToolStripMenuItem.Tag = "NewRoomBack";
            this.newRoomBackToolStripMenuItem.Text = "NewRoomBack";
            // 
            // duplicateRoomToolStripMenuItem
            // 
            this.duplicateRoomToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.duplicateRoomToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.duplicateRoomToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_copy_16;
            this.duplicateRoomToolStripMenuItem.Name = "duplicateRoomToolStripMenuItem";
            this.duplicateRoomToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.duplicateRoomToolStripMenuItem.Tag = "DuplicateRoom";
            this.duplicateRoomToolStripMenuItem.Text = "DuplicateRoom";
            // 
            // cropRoomToolStripMenuItem
            // 
            this.cropRoomToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cropRoomToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cropRoomToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_crop_16;
            this.cropRoomToolStripMenuItem.Name = "cropRoomToolStripMenuItem";
            this.cropRoomToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.cropRoomToolStripMenuItem.Tag = "CropRoom";
            this.cropRoomToolStripMenuItem.Text = "CropRoom";
            // 
            // splitRoomToolStripMenuItem
            // 
            this.splitRoomToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.splitRoomToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.splitRoomToolStripMenuItem.Image = global::TombEditor.Properties.Resources.actions_Split_16;
            this.splitRoomToolStripMenuItem.Name = "splitRoomToolStripMenuItem";
            this.splitRoomToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.splitRoomToolStripMenuItem.Tag = "SplitRoom";
            this.splitRoomToolStripMenuItem.Text = "SplitRoom";
            // 
            // mergeRoomsHorizontallyToolStripMenuItem
            // 
            this.mergeRoomsHorizontallyToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.mergeRoomsHorizontallyToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.mergeRoomsHorizontallyToolStripMenuItem.Image = global::TombEditor.Properties.Resources.actions_Merge_16;
            this.mergeRoomsHorizontallyToolStripMenuItem.Name = "mergeRoomsHorizontallyToolStripMenuItem";
            this.mergeRoomsHorizontallyToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.mergeRoomsHorizontallyToolStripMenuItem.Tag = "MergeRoomsHorizontally";
            this.mergeRoomsHorizontallyToolStripMenuItem.Text = "MergeRoomsHorizontally";
            // 
            // deleteRoomsToolStripMenuItem
            // 
            this.deleteRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.deleteRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.deleteRoomsToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.deleteRoomsToolStripMenuItem.Name = "deleteRoomsToolStripMenuItem";
            this.deleteRoomsToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.deleteRoomsToolStripMenuItem.Tag = "DeleteRooms";
            this.deleteRoomsToolStripMenuItem.Text = "DeleteRooms";
            // 
            // toolStripMenuSeparator5
            // 
            this.toolStripMenuSeparator5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuSeparator5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuSeparator5.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuSeparator5.Name = "toolStripMenuSeparator5";
            this.toolStripMenuSeparator5.Size = new System.Drawing.Size(229, 6);
            // 
            // moveRoomsToolStripMenuItem
            // 
            this.moveRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.moveRoomsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wholeRoomUpToolStripMenuItem,
            this.moveRoomUp4ClicksToolStripMenuItem,
            this.wholeRoomDownToolStripMenuItem,
            this.moveRoomDown4ClicksToolStripMenuItem,
            this.moveRoomLeftToolStripMenuItem,
            this.moveRoomRightToolStripMenuItem,
            this.moveRoomForwardToolStripMenuItem,
            this.moveRoomBackToolStripMenuItem});
            this.moveRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.moveRoomsToolStripMenuItem.Name = "moveRoomsToolStripMenuItem";
            this.moveRoomsToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.moveRoomsToolStripMenuItem.Text = "Move rooms";
            // 
            // wholeRoomUpToolStripMenuItem
            // 
            this.wholeRoomUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.wholeRoomUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.wholeRoomUpToolStripMenuItem.Name = "wholeRoomUpToolStripMenuItem";
            this.wholeRoomUpToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.wholeRoomUpToolStripMenuItem.Tag = "MoveRoomUp";
            this.wholeRoomUpToolStripMenuItem.Text = "MoveRoomUp";
            // 
            // moveRoomUp4ClicksToolStripMenuItem
            // 
            this.moveRoomUp4ClicksToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.moveRoomUp4ClicksToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.moveRoomUp4ClicksToolStripMenuItem.Name = "moveRoomUp4ClicksToolStripMenuItem";
            this.moveRoomUp4ClicksToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.moveRoomUp4ClicksToolStripMenuItem.Tag = "MoveRoomUp4Clicks";
            this.moveRoomUp4ClicksToolStripMenuItem.Text = "MoveRoomUp4Clicks";
            // 
            // wholeRoomDownToolStripMenuItem
            // 
            this.wholeRoomDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.wholeRoomDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.wholeRoomDownToolStripMenuItem.Name = "wholeRoomDownToolStripMenuItem";
            this.wholeRoomDownToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.wholeRoomDownToolStripMenuItem.Tag = "MoveRoomDown";
            this.wholeRoomDownToolStripMenuItem.Text = "MoveRoomDown";
            // 
            // moveRoomDown4ClicksToolStripMenuItem
            // 
            this.moveRoomDown4ClicksToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.moveRoomDown4ClicksToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.moveRoomDown4ClicksToolStripMenuItem.Name = "moveRoomDown4ClicksToolStripMenuItem";
            this.moveRoomDown4ClicksToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.moveRoomDown4ClicksToolStripMenuItem.Tag = "MoveRoomDown4Clicks";
            this.moveRoomDown4ClicksToolStripMenuItem.Text = "MoveRoomDown4Clicks";
            // 
            // moveRoomLeftToolStripMenuItem
            // 
            this.moveRoomLeftToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.moveRoomLeftToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.moveRoomLeftToolStripMenuItem.Name = "moveRoomLeftToolStripMenuItem";
            this.moveRoomLeftToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.moveRoomLeftToolStripMenuItem.Tag = "MoveRoomLeft";
            this.moveRoomLeftToolStripMenuItem.Text = "MoveRoomLeft";
            // 
            // moveRoomRightToolStripMenuItem
            // 
            this.moveRoomRightToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.moveRoomRightToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.moveRoomRightToolStripMenuItem.Name = "moveRoomRightToolStripMenuItem";
            this.moveRoomRightToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.moveRoomRightToolStripMenuItem.Tag = "MoveRoomRight";
            this.moveRoomRightToolStripMenuItem.Text = "MoveRoomRight";
            // 
            // moveRoomForwardToolStripMenuItem
            // 
            this.moveRoomForwardToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.moveRoomForwardToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.moveRoomForwardToolStripMenuItem.Name = "moveRoomForwardToolStripMenuItem";
            this.moveRoomForwardToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.moveRoomForwardToolStripMenuItem.Tag = "MoveRoomForward";
            this.moveRoomForwardToolStripMenuItem.Text = "MoveRoomForward";
            // 
            // moveRoomBackToolStripMenuItem
            // 
            this.moveRoomBackToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.moveRoomBackToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.moveRoomBackToolStripMenuItem.Name = "moveRoomBackToolStripMenuItem";
            this.moveRoomBackToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.moveRoomBackToolStripMenuItem.Tag = "MoveRoomBack";
            this.moveRoomBackToolStripMenuItem.Text = "MoveRoomBack";
            // 
            // transformRoomsToolStripMenuItem
            // 
            this.transformRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.transformRoomsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rotateRoomsToolStripMenuItem,
            this.rotateRoomsCountercockwiseToolStripMenuItem,
            this.mirrorRoomsOnXAxisToolStripMenuItem,
            this.mirrorRoomsOnZAxisToolStripMenuItem});
            this.transformRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.transformRoomsToolStripMenuItem.Name = "transformRoomsToolStripMenuItem";
            this.transformRoomsToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.transformRoomsToolStripMenuItem.Text = "Transform rooms";
            // 
            // rotateRoomsToolStripMenuItem
            // 
            this.rotateRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.rotateRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.rotateRoomsToolStripMenuItem.Name = "rotateRoomsToolStripMenuItem";
            this.rotateRoomsToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.rotateRoomsToolStripMenuItem.Tag = "RotateRoomsClockwise";
            this.rotateRoomsToolStripMenuItem.Text = "RotateRoomsClockwise";
            // 
            // rotateRoomsCountercockwiseToolStripMenuItem
            // 
            this.rotateRoomsCountercockwiseToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.rotateRoomsCountercockwiseToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.rotateRoomsCountercockwiseToolStripMenuItem.Name = "rotateRoomsCountercockwiseToolStripMenuItem";
            this.rotateRoomsCountercockwiseToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.rotateRoomsCountercockwiseToolStripMenuItem.Tag = "RotateRoomsCounterClockwise";
            this.rotateRoomsCountercockwiseToolStripMenuItem.Text = "RotateRoomsCounterClockwise";
            // 
            // mirrorRoomsOnXAxisToolStripMenuItem
            // 
            this.mirrorRoomsOnXAxisToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.mirrorRoomsOnXAxisToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.mirrorRoomsOnXAxisToolStripMenuItem.Name = "mirrorRoomsOnXAxisToolStripMenuItem";
            this.mirrorRoomsOnXAxisToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.mirrorRoomsOnXAxisToolStripMenuItem.Tag = "MirrorRoomsX";
            this.mirrorRoomsOnXAxisToolStripMenuItem.Text = "MirrorRoomsX";
            // 
            // mirrorRoomsOnZAxisToolStripMenuItem
            // 
            this.mirrorRoomsOnZAxisToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.mirrorRoomsOnZAxisToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.mirrorRoomsOnZAxisToolStripMenuItem.Name = "mirrorRoomsOnZAxisToolStripMenuItem";
            this.mirrorRoomsOnZAxisToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.mirrorRoomsOnZAxisToolStripMenuItem.Tag = "MirrorRoomsZ";
            this.mirrorRoomsOnZAxisToolStripMenuItem.Text = "MirrorRoomsZ";
            // 
            // selectRoomsToolStripMenuItem
            // 
            this.selectRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.selectRoomsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectWaterRoomsToolStripMenuItem,
            this.selectSkyRoomsToolStripMenuItem,
            this.selectOutsideRoomsToolStripMenuItem,
            this.selectToolStripMenuItem,
            this.toolStripSeparator6,
            this.selectConnectedRoomsToolStripMenuItem,
            this.selectRoomsByTagsToolStripMenuItem});
            this.selectRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.selectRoomsToolStripMenuItem.Name = "selectRoomsToolStripMenuItem";
            this.selectRoomsToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.selectRoomsToolStripMenuItem.Text = "Select rooms";
            // 
            // selectWaterRoomsToolStripMenuItem
            // 
            this.selectWaterRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.selectWaterRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.selectWaterRoomsToolStripMenuItem.Name = "selectWaterRoomsToolStripMenuItem";
            this.selectWaterRoomsToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.selectWaterRoomsToolStripMenuItem.Tag = "SelectWaterRooms";
            this.selectWaterRoomsToolStripMenuItem.Text = "SelectWaterRooms";
            // 
            // selectSkyRoomsToolStripMenuItem
            // 
            this.selectSkyRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.selectSkyRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.selectSkyRoomsToolStripMenuItem.Name = "selectSkyRoomsToolStripMenuItem";
            this.selectSkyRoomsToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.selectSkyRoomsToolStripMenuItem.Tag = "SelectSkyRooms";
            this.selectSkyRoomsToolStripMenuItem.Text = "SelectSkyRooms";
            // 
            // selectOutsideRoomsToolStripMenuItem
            // 
            this.selectOutsideRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.selectOutsideRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.selectOutsideRoomsToolStripMenuItem.Name = "selectOutsideRoomsToolStripMenuItem";
            this.selectOutsideRoomsToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.selectOutsideRoomsToolStripMenuItem.Tag = "SelectOutsideRooms";
            this.selectOutsideRoomsToolStripMenuItem.Text = "SelectOutsideRooms";
            // 
            // selectToolStripMenuItem
            // 
            this.selectToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.selectToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.selectToolStripMenuItem.Name = "selectToolStripMenuItem";
            this.selectToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.selectToolStripMenuItem.Tag = "SelectQuicksandRooms";
            this.selectToolStripMenuItem.Text = "SelectQuicksandRooms";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator6.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(171, 6);
            // 
            // selectConnectedRoomsToolStripMenuItem
            // 
            this.selectConnectedRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.selectConnectedRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.selectConnectedRoomsToolStripMenuItem.Name = "selectConnectedRoomsToolStripMenuItem";
            this.selectConnectedRoomsToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.selectConnectedRoomsToolStripMenuItem.Tag = "SelectConnectedRooms";
            this.selectConnectedRoomsToolStripMenuItem.Text = "SelectConnectedRooms";
            // 
            // selectRoomsByTagsToolStripMenuItem
            // 
            this.selectRoomsByTagsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.selectRoomsByTagsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.selectRoomsByTagsToolStripMenuItem.Name = "selectRoomsByTagsToolStripMenuItem";
            this.selectRoomsByTagsToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.selectRoomsByTagsToolStripMenuItem.Tag = "SelectRoomsByTags";
            this.selectRoomsByTagsToolStripMenuItem.Text = "SelectRoomsByTags";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(229, 6);
            // 
            // exportRoomToolStripMenuItem
            // 
            this.exportRoomToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.exportRoomToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.exportRoomToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_Export_16;
            this.exportRoomToolStripMenuItem.Name = "exportRoomToolStripMenuItem";
            this.exportRoomToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.exportRoomToolStripMenuItem.Tag = "ExportRooms";
            this.exportRoomToolStripMenuItem.Text = "ExportRooms";
            // 
            // importRoomsToolStripMenuItem
            // 
            this.importRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.importRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.importRoomsToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_Import_16;
            this.importRoomsToolStripMenuItem.Name = "importRoomsToolStripMenuItem";
            this.importRoomsToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.importRoomsToolStripMenuItem.Tag = "ImportRooms";
            this.importRoomsToolStripMenuItem.Text = "ImportRooms";
            this.importRoomsToolStripMenuItem.Visible = false;
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem2.Image = global::TombEditor.Properties.Resources.objects_LightPoint_16;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(232, 22);
            this.toolStripMenuItem2.Tag = "ApplyAmbientLightToSelectedRooms";
            this.toolStripMenuItem2.Text = "ApplyAmbientLightToSelectedRooms";
            // 
            // applyCurrentAmbientLightToAllRoomsToolStripMenuItem
            // 
            this.applyCurrentAmbientLightToAllRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.applyCurrentAmbientLightToAllRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.applyCurrentAmbientLightToAllRoomsToolStripMenuItem.Image = global::TombEditor.Properties.Resources.objects_LightPoint_16;
            this.applyCurrentAmbientLightToAllRoomsToolStripMenuItem.Name = "applyCurrentAmbientLightToAllRoomsToolStripMenuItem";
            this.applyCurrentAmbientLightToAllRoomsToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.applyCurrentAmbientLightToAllRoomsToolStripMenuItem.Tag = "ApplyAmbientLightToAllRooms";
            this.applyCurrentAmbientLightToAllRoomsToolStripMenuItem.Text = "ApplyAmbientLightToAllRooms";
            // 
            // itemsToolStripMenuItem
            // 
            this.itemsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.itemsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addWadToolStripMenuItem,
            this.removeWadsToolStripMenuItem,
            this.reloadWadsToolStripMenuItem,
            this.reloadSoundsToolStripMenuItem,
            this.toolStripMenuSeparator6,
            this.toolStripMenuItem8,
            this.addPortalToolStripMenuItem,
            this.addTriggerToolStripMenuItem,
            this.toolStripMenuSeparator7,
            this.addSphereVolumeToolStripMenuItem,
            this.addPrismVolumeToolStripMenuItem,
            this.addBoxVolumeToolStripMenuItem,
            this.toolStripMenuSeparator8,
            this.findObjectToolStripMenuItem,
            this.moveLaraToolStripMenuItem,
            this.splitSectorObjectOnSelectionToolStripMenuItem,
            this.toolStripSeparator7,
            this.deleteAllTriggersToolStripMenuItem,
            this.deleteAllObjectsToolStripMenuItem,
            this.toolStripSeparator3,
            this.setStaticMeshColorToRoomLightToolStripMenuItem,
            this.setStaticMeshesColorToolStripMenuItem,
            this.makeQuickItemGroupToolStripMenuItem});
            this.itemsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.itemsToolStripMenuItem.Name = "itemsToolStripMenuItem";
            this.itemsToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.itemsToolStripMenuItem.Text = "Items";
            // 
            // addWadToolStripMenuItem
            // 
            this.addWadToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addWadToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addWadToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.addWadToolStripMenuItem.Name = "addWadToolStripMenuItem";
            this.addWadToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.addWadToolStripMenuItem.Tag = "AddWad";
            this.addWadToolStripMenuItem.Text = "AddWad";
            // 
            // removeWadsToolStripMenuItem
            // 
            this.removeWadsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.removeWadsToolStripMenuItem.Enabled = false;
            this.removeWadsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.removeWadsToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.removeWadsToolStripMenuItem.Name = "removeWadsToolStripMenuItem";
            this.removeWadsToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.removeWadsToolStripMenuItem.Tag = "RemoveWads";
            this.removeWadsToolStripMenuItem.Text = "RemoveWads";
            // 
            // reloadWadsToolStripMenuItem
            // 
            this.reloadWadsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.reloadWadsToolStripMenuItem.Enabled = false;
            this.reloadWadsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.reloadWadsToolStripMenuItem.Name = "reloadWadsToolStripMenuItem";
            this.reloadWadsToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.reloadWadsToolStripMenuItem.Tag = "ReloadWads";
            this.reloadWadsToolStripMenuItem.Text = "ReloadWads";
            // 
            // reloadSoundsToolStripMenuItem
            // 
            this.reloadSoundsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.reloadSoundsToolStripMenuItem.Enabled = false;
            this.reloadSoundsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.reloadSoundsToolStripMenuItem.Name = "reloadSoundsToolStripMenuItem";
            this.reloadSoundsToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.reloadSoundsToolStripMenuItem.Tag = "ReloadSounds";
            this.reloadSoundsToolStripMenuItem.Text = "ReloadSounds";
            // 
            // toolStripMenuSeparator6
            // 
            this.toolStripMenuSeparator6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuSeparator6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuSeparator6.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuSeparator6.Name = "toolStripMenuSeparator6";
            this.toolStripMenuSeparator6.Size = new System.Drawing.Size(219, 6);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem8.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addCameraToolStripMenuItem,
            this.addFlybyCameraToolStripMenuItem,
            this.addSinkToolStripMenuItem,
            this.addSoundSourceToolStripMenuItem,
            this.addImportedGeometryToolStripMenuItem,
            this.addGhostBlockToolStripMenuItem});
            this.toolStripMenuItem8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(222, 22);
            this.toolStripMenuItem8.Text = "Add item";
            // 
            // addCameraToolStripMenuItem
            // 
            this.addCameraToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addCameraToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addCameraToolStripMenuItem.Image = global::TombEditor.Properties.Resources.objects_Camera_16;
            this.addCameraToolStripMenuItem.Name = "addCameraToolStripMenuItem";
            this.addCameraToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.addCameraToolStripMenuItem.Tag = "AddCamera";
            this.addCameraToolStripMenuItem.Text = "AddCamera";
            // 
            // addFlybyCameraToolStripMenuItem
            // 
            this.addFlybyCameraToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addFlybyCameraToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addFlybyCameraToolStripMenuItem.Image = global::TombEditor.Properties.Resources.objects_movie_projector_16;
            this.addFlybyCameraToolStripMenuItem.Name = "addFlybyCameraToolStripMenuItem";
            this.addFlybyCameraToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.addFlybyCameraToolStripMenuItem.Tag = "AddFlybyCamera";
            this.addFlybyCameraToolStripMenuItem.Text = "AddFlybyCamera";
            // 
            // addSinkToolStripMenuItem
            // 
            this.addSinkToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addSinkToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addSinkToolStripMenuItem.Image = global::TombEditor.Properties.Resources.objects_tornado_16;
            this.addSinkToolStripMenuItem.Name = "addSinkToolStripMenuItem";
            this.addSinkToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.addSinkToolStripMenuItem.Tag = "AddSink";
            this.addSinkToolStripMenuItem.Text = "AddSink";
            // 
            // addSoundSourceToolStripMenuItem
            // 
            this.addSoundSourceToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addSoundSourceToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addSoundSourceToolStripMenuItem.Image = global::TombEditor.Properties.Resources.objects_speaker_16;
            this.addSoundSourceToolStripMenuItem.Name = "addSoundSourceToolStripMenuItem";
            this.addSoundSourceToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.addSoundSourceToolStripMenuItem.Tag = "AddSoundSource";
            this.addSoundSourceToolStripMenuItem.Text = "AddSoundSource";
            // 
            // addImportedGeometryToolStripMenuItem
            // 
            this.addImportedGeometryToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addImportedGeometryToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addImportedGeometryToolStripMenuItem.Image = global::TombEditor.Properties.Resources.objects_custom_geometry;
            this.addImportedGeometryToolStripMenuItem.Name = "addImportedGeometryToolStripMenuItem";
            this.addImportedGeometryToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.addImportedGeometryToolStripMenuItem.Tag = "AddImportedGeometry";
            this.addImportedGeometryToolStripMenuItem.Text = "AddImportedGeometry";
            // 
            // addGhostBlockToolStripMenuItem
            // 
            this.addGhostBlockToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addGhostBlockToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addGhostBlockToolStripMenuItem.Image = global::TombEditor.Properties.Resources.objects_geometry_override_16;
            this.addGhostBlockToolStripMenuItem.Name = "addGhostBlockToolStripMenuItem";
            this.addGhostBlockToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.addGhostBlockToolStripMenuItem.Tag = "AddGhostBlock";
            this.addGhostBlockToolStripMenuItem.Text = "AddGhostBlock";
            // 
            // addPortalToolStripMenuItem
            // 
            this.addPortalToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addPortalToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addPortalToolStripMenuItem.Name = "addPortalToolStripMenuItem";
            this.addPortalToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.addPortalToolStripMenuItem.Tag = "AddPortal";
            this.addPortalToolStripMenuItem.Text = "AddPortal";
            // 
            // addTriggerToolStripMenuItem
            // 
            this.addTriggerToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addTriggerToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addTriggerToolStripMenuItem.Name = "addTriggerToolStripMenuItem";
            this.addTriggerToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.addTriggerToolStripMenuItem.Tag = "AddTrigger";
            this.addTriggerToolStripMenuItem.Text = "AddTrigger";
            // 
            // toolStripMenuSeparator7
            // 
            this.toolStripMenuSeparator7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuSeparator7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuSeparator7.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuSeparator7.Name = "toolStripMenuSeparator7";
            this.toolStripMenuSeparator7.Size = new System.Drawing.Size(219, 6);
            // 
            // addSphereVolumeToolStripMenuItem
            // 
            this.addSphereVolumeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addSphereVolumeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addSphereVolumeToolStripMenuItem.Image = global::TombEditor.Properties.Resources.objects_volume_sphere_16;
            this.addSphereVolumeToolStripMenuItem.Name = "addSphereVolumeToolStripMenuItem";
            this.addSphereVolumeToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.addSphereVolumeToolStripMenuItem.Tag = "AddSphereVolume";
            this.addSphereVolumeToolStripMenuItem.Text = "AddSphereVolume";
            this.addSphereVolumeToolStripMenuItem.Visible = false;
            // 
            // addPrismVolumeToolStripMenuItem
            // 
            this.addPrismVolumeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addPrismVolumeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addPrismVolumeToolStripMenuItem.Image = global::TombEditor.Properties.Resources.objects_volume_prism_16;
            this.addPrismVolumeToolStripMenuItem.Name = "addPrismVolumeToolStripMenuItem";
            this.addPrismVolumeToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.addPrismVolumeToolStripMenuItem.Tag = "AddPrismVolume";
            this.addPrismVolumeToolStripMenuItem.Text = "AddPrismVolume";
            this.addPrismVolumeToolStripMenuItem.Visible = false;
            // 
            // addBoxVolumeToolStripMenuItem
            // 
            this.addBoxVolumeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addBoxVolumeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addBoxVolumeToolStripMenuItem.Image = global::TombEditor.Properties.Resources.objects_volume_box_16;
            this.addBoxVolumeToolStripMenuItem.Name = "addBoxVolumeToolStripMenuItem";
            this.addBoxVolumeToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.addBoxVolumeToolStripMenuItem.Tag = "AddBoxVolume";
            this.addBoxVolumeToolStripMenuItem.Text = "AddBoxVolume";
            this.addBoxVolumeToolStripMenuItem.Visible = false;
            // 
            // toolStripMenuSeparator8
            // 
            this.toolStripMenuSeparator8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuSeparator8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuSeparator8.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuSeparator8.Name = "toolStripMenuSeparator8";
            this.toolStripMenuSeparator8.Size = new System.Drawing.Size(219, 6);
            this.toolStripMenuSeparator8.Visible = false;
            // 
            // findObjectToolStripMenuItem
            // 
            this.findObjectToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.findObjectToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.findObjectToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_target_16;
            this.findObjectToolStripMenuItem.Name = "findObjectToolStripMenuItem";
            this.findObjectToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.findObjectToolStripMenuItem.Tag = "LocateItem";
            this.findObjectToolStripMenuItem.Text = "LocateItem";
            // 
            // moveLaraToolStripMenuItem
            // 
            this.moveLaraToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.moveLaraToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.moveLaraToolStripMenuItem.Name = "moveLaraToolStripMenuItem";
            this.moveLaraToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.moveLaraToolStripMenuItem.Tag = "MoveLara";
            this.moveLaraToolStripMenuItem.Text = "MoveLara";
            // 
            // splitSectorObjectOnSelectionToolStripMenuItem
            // 
            this.splitSectorObjectOnSelectionToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.splitSectorObjectOnSelectionToolStripMenuItem.Enabled = false;
            this.splitSectorObjectOnSelectionToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.splitSectorObjectOnSelectionToolStripMenuItem.Name = "splitSectorObjectOnSelectionToolStripMenuItem";
            this.splitSectorObjectOnSelectionToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.splitSectorObjectOnSelectionToolStripMenuItem.Tag = "SplitSectorObjectOnSelection";
            this.splitSectorObjectOnSelectionToolStripMenuItem.Text = "SplitSectorObjectOnSelection";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator7.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(219, 6);
            // 
            // deleteAllTriggersToolStripMenuItem
            // 
            this.deleteAllTriggersToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.deleteAllTriggersToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.deleteAllTriggersToolStripMenuItem.Name = "deleteAllTriggersToolStripMenuItem";
            this.deleteAllTriggersToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.deleteAllTriggersToolStripMenuItem.Tag = "DeleteAllTriggers";
            this.deleteAllTriggersToolStripMenuItem.Text = "DeleteAllTriggers";
            // 
            // deleteAllObjectsToolStripMenuItem
            // 
            this.deleteAllObjectsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.deleteAllObjectsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.deleteAllObjectsToolStripMenuItem.Name = "deleteAllObjectsToolStripMenuItem";
            this.deleteAllObjectsToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.deleteAllObjectsToolStripMenuItem.Tag = "DeleteAllObjects";
            this.deleteAllObjectsToolStripMenuItem.Text = "DeleteAllObjects";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(219, 6);
            // 
            // setStaticMeshColorToRoomLightToolStripMenuItem
            // 
            this.setStaticMeshColorToRoomLightToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.setStaticMeshColorToRoomLightToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.setStaticMeshColorToRoomLightToolStripMenuItem.Name = "setStaticMeshColorToRoomLightToolStripMenuItem";
            this.setStaticMeshColorToRoomLightToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.setStaticMeshColorToRoomLightToolStripMenuItem.Tag = "SetStaticMeshesColorToRoomLight";
            this.setStaticMeshColorToRoomLightToolStripMenuItem.Text = "SetStaticMeshesColorToRoomLight";
            // 
            // setStaticMeshesColorToolStripMenuItem
            // 
            this.setStaticMeshesColorToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.setStaticMeshesColorToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.setStaticMeshesColorToolStripMenuItem.Name = "setStaticMeshesColorToolStripMenuItem";
            this.setStaticMeshesColorToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.setStaticMeshesColorToolStripMenuItem.Tag = "SetStaticMeshesColor";
            this.setStaticMeshesColorToolStripMenuItem.Text = "SetStaticMeshesColor";
            // 
            // makeQuickItemGroupToolStripMenuItem
            // 
            this.makeQuickItemGroupToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.makeQuickItemGroupToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.makeQuickItemGroupToolStripMenuItem.Name = "makeQuickItemGroupToolStripMenuItem";
            this.makeQuickItemGroupToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            this.makeQuickItemGroupToolStripMenuItem.Tag = "MakeQuickItemGroup";
            this.makeQuickItemGroupToolStripMenuItem.Text = "MakeQuickItemGroup";
            // 
            // texturesToolStripMenuItem
            // 
            this.texturesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.texturesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadTextureToolStripMenuItem,
            this.removeTexturesToolStripMenuItem,
            this.unloadTexturesToolStripMenuItem,
            this.reloadTexturesToolStripMenuItem,
            this.importConvertTexturesToPng,
            this.remapTextureToolStripMenuItem,
            this.toolStripMenuSeparator9,
            this.textureFloorToolStripMenuItem,
            this.textureWallsToolStripMenuItem,
            this.textureCeilingToolStripMenuItem,
            this.toolStripMenuItem3,
            this.clearAllTexturesInRoomToolStripMenuItem,
            this.clearAllTexturesInRoomToolStripMenuItem1,
            this.findUntexturedToolStripMenuItem,
            this.toolStripMenuSeparator10,
            this.animationRangesToolStripMenuItem});
            this.texturesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.texturesToolStripMenuItem.Name = "texturesToolStripMenuItem";
            this.texturesToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.texturesToolStripMenuItem.Text = "Textures";
            // 
            // loadTextureToolStripMenuItem
            // 
            this.loadTextureToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.loadTextureToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.loadTextureToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.loadTextureToolStripMenuItem.Name = "loadTextureToolStripMenuItem";
            this.loadTextureToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.loadTextureToolStripMenuItem.Tag = "AddTexture";
            this.loadTextureToolStripMenuItem.Text = "AddTexture";
            // 
            // removeTexturesToolStripMenuItem
            // 
            this.removeTexturesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.removeTexturesToolStripMenuItem.Enabled = false;
            this.removeTexturesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.removeTexturesToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.removeTexturesToolStripMenuItem.Name = "removeTexturesToolStripMenuItem";
            this.removeTexturesToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.removeTexturesToolStripMenuItem.Tag = "RemoveTextures";
            this.removeTexturesToolStripMenuItem.Text = "RemoveTextures";
            // 
            // unloadTexturesToolStripMenuItem
            // 
            this.unloadTexturesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.unloadTexturesToolStripMenuItem.Enabled = false;
            this.unloadTexturesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.unloadTexturesToolStripMenuItem.Name = "unloadTexturesToolStripMenuItem";
            this.unloadTexturesToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.unloadTexturesToolStripMenuItem.Tag = "UnloadTextures";
            this.unloadTexturesToolStripMenuItem.Text = "UnloadTextures";
            // 
            // reloadTexturesToolStripMenuItem
            // 
            this.reloadTexturesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.reloadTexturesToolStripMenuItem.Enabled = false;
            this.reloadTexturesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.reloadTexturesToolStripMenuItem.Name = "reloadTexturesToolStripMenuItem";
            this.reloadTexturesToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.reloadTexturesToolStripMenuItem.Tag = "ReloadTextures";
            this.reloadTexturesToolStripMenuItem.Text = "ReloadTextures";
            // 
            // importConvertTexturesToPng
            // 
            this.importConvertTexturesToPng.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.importConvertTexturesToPng.Enabled = false;
            this.importConvertTexturesToPng.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.importConvertTexturesToPng.Image = global::TombEditor.Properties.Resources.general_Import_16;
            this.importConvertTexturesToPng.Name = "importConvertTexturesToPng";
            this.importConvertTexturesToPng.Size = new System.Drawing.Size(173, 22);
            this.importConvertTexturesToPng.Tag = "ConvertTexturesToPNG";
            this.importConvertTexturesToPng.Text = "ConvertTexturesToPNG";
            // 
            // remapTextureToolStripMenuItem
            // 
            this.remapTextureToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.remapTextureToolStripMenuItem.Enabled = false;
            this.remapTextureToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.remapTextureToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_crop_16;
            this.remapTextureToolStripMenuItem.Name = "remapTextureToolStripMenuItem";
            this.remapTextureToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.remapTextureToolStripMenuItem.Tag = "RemapTexture";
            this.remapTextureToolStripMenuItem.Text = "RemapTexture";
            // 
            // toolStripMenuSeparator9
            // 
            this.toolStripMenuSeparator9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuSeparator9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuSeparator9.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuSeparator9.Name = "toolStripMenuSeparator9";
            this.toolStripMenuSeparator9.Size = new System.Drawing.Size(170, 6);
            // 
            // textureFloorToolStripMenuItem
            // 
            this.textureFloorToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.textureFloorToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.textureFloorToolStripMenuItem.Image = global::TombEditor.Properties.Resources.texture_Floor2_16;
            this.textureFloorToolStripMenuItem.Name = "textureFloorToolStripMenuItem";
            this.textureFloorToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.textureFloorToolStripMenuItem.Tag = "TextureFloor";
            this.textureFloorToolStripMenuItem.Text = "TextureFloor";
            // 
            // textureWallsToolStripMenuItem
            // 
            this.textureWallsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.textureWallsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.textureWallsToolStripMenuItem.Image = global::TombEditor.Properties.Resources.texture_Walls2_16;
            this.textureWallsToolStripMenuItem.Name = "textureWallsToolStripMenuItem";
            this.textureWallsToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.textureWallsToolStripMenuItem.Tag = "TextureWalls";
            this.textureWallsToolStripMenuItem.Text = "TextureWalls";
            // 
            // textureCeilingToolStripMenuItem
            // 
            this.textureCeilingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.textureCeilingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.textureCeilingToolStripMenuItem.Image = global::TombEditor.Properties.Resources.texture_Ceiling2_16;
            this.textureCeilingToolStripMenuItem.Name = "textureCeilingToolStripMenuItem";
            this.textureCeilingToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.textureCeilingToolStripMenuItem.Tag = "TextureCeiling";
            this.textureCeilingToolStripMenuItem.Text = "TextureCeiling";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(170, 6);
            // 
            // clearAllTexturesInRoomToolStripMenuItem
            // 
            this.clearAllTexturesInRoomToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.clearAllTexturesInRoomToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.clearAllTexturesInRoomToolStripMenuItem.Image = global::TombEditor.Properties.Resources.toolbox_Eraser_16;
            this.clearAllTexturesInRoomToolStripMenuItem.Name = "clearAllTexturesInRoomToolStripMenuItem";
            this.clearAllTexturesInRoomToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.clearAllTexturesInRoomToolStripMenuItem.Tag = "ClearAllTexturesInRoom";
            this.clearAllTexturesInRoomToolStripMenuItem.Text = "ClearAllTexturesInRoom";
            // 
            // clearAllTexturesInRoomToolStripMenuItem1
            // 
            this.clearAllTexturesInRoomToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.clearAllTexturesInRoomToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.clearAllTexturesInRoomToolStripMenuItem1.Name = "clearAllTexturesInRoomToolStripMenuItem1";
            this.clearAllTexturesInRoomToolStripMenuItem1.Size = new System.Drawing.Size(173, 22);
            this.clearAllTexturesInRoomToolStripMenuItem1.Tag = "ClearAllTexturesInLevel";
            this.clearAllTexturesInRoomToolStripMenuItem1.Text = "ClearAllTexturesInLevel";
            // 
            // findUntexturedToolStripMenuItem
            // 
            this.findUntexturedToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.findUntexturedToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.findUntexturedToolStripMenuItem.Name = "findUntexturedToolStripMenuItem";
            this.findUntexturedToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.findUntexturedToolStripMenuItem.Tag = "FindUntextured";
            this.findUntexturedToolStripMenuItem.Text = "FindUntextured";
            // 
            // toolStripMenuSeparator10
            // 
            this.toolStripMenuSeparator10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuSeparator10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuSeparator10.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuSeparator10.Name = "toolStripMenuSeparator10";
            this.toolStripMenuSeparator10.Size = new System.Drawing.Size(170, 6);
            // 
            // animationRangesToolStripMenuItem
            // 
            this.animationRangesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.animationRangesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.animationRangesToolStripMenuItem.Image = global::TombEditor.Properties.Resources.texture_anim_ranges;
            this.animationRangesToolStripMenuItem.Name = "animationRangesToolStripMenuItem";
            this.animationRangesToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.animationRangesToolStripMenuItem.Tag = "EditAnimationRanges";
            this.animationRangesToolStripMenuItem.Text = "EditAnimationRanges";
            // 
            // transformToolStripMenuItem
            // 
            this.transformToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.transformToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.smoothRandomFloorUpToolStripMenuItem,
            this.smoothRandomFloorDownToolStripMenuItem,
            this.smoothRandomCeilingUpToolStripMenuItem,
            this.smoothRandomCeilingDownToolStripMenuItem,
            this.toolStripMenuSeparator11,
            this.sharpRandomFloorUpToolStripMenuItem,
            this.sharpRandomFloorDownToolStripMenuItem,
            this.sharpRandomCeilingUpToolStripMenuItem,
            this.sharpRandomCeilingDownToolStripMenuItem,
            this.toolStripMenuSeparator12,
            this.averageFloorToolStripMenuItem,
            this.averageCeilingToolStripMenuItem,
            this.flattenFloorToolStripMenuItem,
            this.flattenCeilingToolStripMenuItem,
            this.resetGeometryToolStripMenuItem,
            this.toolStripMenuSeparator13,
            this.gridWallsIn3ToolStripMenuItem,
            this.gridWallsIn5ToolStripMenuItem,
            this.gridWallsIn3SquaresToolStripMenuItem,
            this.gridWallsIn5SquaresToolStripMenuItem});
            this.transformToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.transformToolStripMenuItem.Name = "transformToolStripMenuItem";
            this.transformToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.transformToolStripMenuItem.Text = "Transform";
            // 
            // smoothRandomFloorUpToolStripMenuItem
            // 
            this.smoothRandomFloorUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.smoothRandomFloorUpToolStripMenuItem.Enabled = false;
            this.smoothRandomFloorUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.smoothRandomFloorUpToolStripMenuItem.Name = "smoothRandomFloorUpToolStripMenuItem";
            this.smoothRandomFloorUpToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.smoothRandomFloorUpToolStripMenuItem.Tag = "SmoothRandomFloorUp";
            this.smoothRandomFloorUpToolStripMenuItem.Text = "SmoothRandomFloorUp";
            // 
            // smoothRandomFloorDownToolStripMenuItem
            // 
            this.smoothRandomFloorDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.smoothRandomFloorDownToolStripMenuItem.Enabled = false;
            this.smoothRandomFloorDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.smoothRandomFloorDownToolStripMenuItem.Name = "smoothRandomFloorDownToolStripMenuItem";
            this.smoothRandomFloorDownToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.smoothRandomFloorDownToolStripMenuItem.Tag = "SmoothRandomFloorDown";
            this.smoothRandomFloorDownToolStripMenuItem.Text = "SmoothRandomFloorDown";
            // 
            // smoothRandomCeilingUpToolStripMenuItem
            // 
            this.smoothRandomCeilingUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.smoothRandomCeilingUpToolStripMenuItem.Enabled = false;
            this.smoothRandomCeilingUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.smoothRandomCeilingUpToolStripMenuItem.Name = "smoothRandomCeilingUpToolStripMenuItem";
            this.smoothRandomCeilingUpToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.smoothRandomCeilingUpToolStripMenuItem.Tag = "SmoothRandomCeilingUp";
            this.smoothRandomCeilingUpToolStripMenuItem.Text = "SmoothRandomCeilingUp";
            // 
            // smoothRandomCeilingDownToolStripMenuItem
            // 
            this.smoothRandomCeilingDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.smoothRandomCeilingDownToolStripMenuItem.Enabled = false;
            this.smoothRandomCeilingDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.smoothRandomCeilingDownToolStripMenuItem.Name = "smoothRandomCeilingDownToolStripMenuItem";
            this.smoothRandomCeilingDownToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.smoothRandomCeilingDownToolStripMenuItem.Tag = "SmoothRandomCeilingDown";
            this.smoothRandomCeilingDownToolStripMenuItem.Text = "SmoothRandomCeilingDown";
            // 
            // toolStripMenuSeparator11
            // 
            this.toolStripMenuSeparator11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuSeparator11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuSeparator11.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuSeparator11.Name = "toolStripMenuSeparator11";
            this.toolStripMenuSeparator11.Size = new System.Drawing.Size(195, 6);
            // 
            // sharpRandomFloorUpToolStripMenuItem
            // 
            this.sharpRandomFloorUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.sharpRandomFloorUpToolStripMenuItem.Enabled = false;
            this.sharpRandomFloorUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.sharpRandomFloorUpToolStripMenuItem.Name = "sharpRandomFloorUpToolStripMenuItem";
            this.sharpRandomFloorUpToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.sharpRandomFloorUpToolStripMenuItem.Tag = "SharpRandomFloorUp";
            this.sharpRandomFloorUpToolStripMenuItem.Text = "SharpRandomFloorUp";
            // 
            // sharpRandomFloorDownToolStripMenuItem
            // 
            this.sharpRandomFloorDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.sharpRandomFloorDownToolStripMenuItem.Enabled = false;
            this.sharpRandomFloorDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.sharpRandomFloorDownToolStripMenuItem.Name = "sharpRandomFloorDownToolStripMenuItem";
            this.sharpRandomFloorDownToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.sharpRandomFloorDownToolStripMenuItem.Tag = "SharpRandomFloorDown";
            this.sharpRandomFloorDownToolStripMenuItem.Text = "SharpRandomFloorDown";
            // 
            // sharpRandomCeilingUpToolStripMenuItem
            // 
            this.sharpRandomCeilingUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.sharpRandomCeilingUpToolStripMenuItem.Enabled = false;
            this.sharpRandomCeilingUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.sharpRandomCeilingUpToolStripMenuItem.Name = "sharpRandomCeilingUpToolStripMenuItem";
            this.sharpRandomCeilingUpToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.sharpRandomCeilingUpToolStripMenuItem.Tag = "SharpRandomCeilingUp";
            this.sharpRandomCeilingUpToolStripMenuItem.Text = "SharpRandomCeilingUp";
            // 
            // sharpRandomCeilingDownToolStripMenuItem
            // 
            this.sharpRandomCeilingDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.sharpRandomCeilingDownToolStripMenuItem.Enabled = false;
            this.sharpRandomCeilingDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.sharpRandomCeilingDownToolStripMenuItem.Name = "sharpRandomCeilingDownToolStripMenuItem";
            this.sharpRandomCeilingDownToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.sharpRandomCeilingDownToolStripMenuItem.Tag = "SharpRandomCeilingDown";
            this.sharpRandomCeilingDownToolStripMenuItem.Text = "SharpRandomCeilingDown";
            // 
            // toolStripMenuSeparator12
            // 
            this.toolStripMenuSeparator12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuSeparator12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuSeparator12.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuSeparator12.Name = "toolStripMenuSeparator12";
            this.toolStripMenuSeparator12.Size = new System.Drawing.Size(195, 6);
            // 
            // averageFloorToolStripMenuItem
            // 
            this.averageFloorToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.averageFloorToolStripMenuItem.Enabled = false;
            this.averageFloorToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.averageFloorToolStripMenuItem.Name = "averageFloorToolStripMenuItem";
            this.averageFloorToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.averageFloorToolStripMenuItem.Tag = "AverageFloor";
            this.averageFloorToolStripMenuItem.Text = "AverageFloor";
            // 
            // averageCeilingToolStripMenuItem
            // 
            this.averageCeilingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.averageCeilingToolStripMenuItem.Enabled = false;
            this.averageCeilingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.averageCeilingToolStripMenuItem.Name = "averageCeilingToolStripMenuItem";
            this.averageCeilingToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.averageCeilingToolStripMenuItem.Tag = "AverageCeiling";
            this.averageCeilingToolStripMenuItem.Text = "AverageCeiling";
            // 
            // flattenFloorToolStripMenuItem
            // 
            this.flattenFloorToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.flattenFloorToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.flattenFloorToolStripMenuItem.Name = "flattenFloorToolStripMenuItem";
            this.flattenFloorToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.flattenFloorToolStripMenuItem.Tag = "FlattenFloor";
            this.flattenFloorToolStripMenuItem.Text = "FlattenFloor";
            // 
            // flattenCeilingToolStripMenuItem
            // 
            this.flattenCeilingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.flattenCeilingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.flattenCeilingToolStripMenuItem.Name = "flattenCeilingToolStripMenuItem";
            this.flattenCeilingToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.flattenCeilingToolStripMenuItem.Tag = "FlattenCeiling";
            this.flattenCeilingToolStripMenuItem.Text = "FlattenCeiling";
            // 
            // resetGeometryToolStripMenuItem
            // 
            this.resetGeometryToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.resetGeometryToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.resetGeometryToolStripMenuItem.Name = "resetGeometryToolStripMenuItem";
            this.resetGeometryToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.resetGeometryToolStripMenuItem.Tag = "ResetGeometry";
            this.resetGeometryToolStripMenuItem.Text = "ResetGeometry";
            // 
            // toolStripMenuSeparator13
            // 
            this.toolStripMenuSeparator13.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuSeparator13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuSeparator13.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuSeparator13.Name = "toolStripMenuSeparator13";
            this.toolStripMenuSeparator13.Size = new System.Drawing.Size(195, 6);
            // 
            // gridWallsIn3ToolStripMenuItem
            // 
            this.gridWallsIn3ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.gridWallsIn3ToolStripMenuItem.Enabled = false;
            this.gridWallsIn3ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.gridWallsIn3ToolStripMenuItem.Name = "gridWallsIn3ToolStripMenuItem";
            this.gridWallsIn3ToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.gridWallsIn3ToolStripMenuItem.Tag = "GridWallsIn3";
            this.gridWallsIn3ToolStripMenuItem.Text = "GridWallsIn3";
            // 
            // gridWallsIn5ToolStripMenuItem
            // 
            this.gridWallsIn5ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.gridWallsIn5ToolStripMenuItem.Enabled = false;
            this.gridWallsIn5ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.gridWallsIn5ToolStripMenuItem.Name = "gridWallsIn5ToolStripMenuItem";
            this.gridWallsIn5ToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.gridWallsIn5ToolStripMenuItem.Tag = "GridWallsIn5";
            this.gridWallsIn5ToolStripMenuItem.Text = "GridWallsIn5";
            // 
            // gridWallsIn3SquaresToolStripMenuItem
            // 
            this.gridWallsIn3SquaresToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.gridWallsIn3SquaresToolStripMenuItem.Enabled = false;
            this.gridWallsIn3SquaresToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.gridWallsIn3SquaresToolStripMenuItem.Name = "gridWallsIn3SquaresToolStripMenuItem";
            this.gridWallsIn3SquaresToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.gridWallsIn3SquaresToolStripMenuItem.Tag = "GridWallsIn3Squares";
            this.gridWallsIn3SquaresToolStripMenuItem.Text = "GridWallsIn3Squares";
            // 
            // gridWallsIn5SquaresToolStripMenuItem
            // 
            this.gridWallsIn5SquaresToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.gridWallsIn5SquaresToolStripMenuItem.Enabled = false;
            this.gridWallsIn5SquaresToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.gridWallsIn5SquaresToolStripMenuItem.Name = "gridWallsIn5SquaresToolStripMenuItem";
            this.gridWallsIn5SquaresToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.gridWallsIn5SquaresToolStripMenuItem.Tag = "GridWallsIn5Squares";
            this.gridWallsIn5SquaresToolStripMenuItem.Text = "GridWallsIn5Squares";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem4.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem5,
            this.editOptionsToolStripMenuItem,
            this.keyboardLayoutToolStripMenuItem,
            this.toolStripSeparator5,
            this.toolStripMenuItem7,
            this.toolStripMenuItem6});
            this.toolStripMenuItem4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(40, 20);
            this.toolStripMenuItem4.Text = "Tools";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem5.Image = global::TombEditor.Properties.Resources.general_settings_16;
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(158, 22);
            this.toolStripMenuItem5.Tag = "EditLevelSettings";
            this.toolStripMenuItem5.Text = "EditLevelSettings";
            // 
            // editOptionsToolStripMenuItem
            // 
            this.editOptionsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.editOptionsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.editOptionsToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_options_16;
            this.editOptionsToolStripMenuItem.Name = "editOptionsToolStripMenuItem";
            this.editOptionsToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.editOptionsToolStripMenuItem.Tag = "EditOptions";
            this.editOptionsToolStripMenuItem.Text = "EditOptions";
            // 
            // keyboardLayoutToolStripMenuItem
            // 
            this.keyboardLayoutToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.keyboardLayoutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.keyboardLayoutToolStripMenuItem.Name = "keyboardLayoutToolStripMenuItem";
            this.keyboardLayoutToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.keyboardLayoutToolStripMenuItem.Tag = "EditKeyboardLayout";
            this.keyboardLayoutToolStripMenuItem.Text = "EditKeyboardLayout";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator5.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(155, 6);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(158, 22);
            this.toolStripMenuItem7.Tag = "StartWadTool";
            this.toolStripMenuItem7.Text = "StartWadTool";
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(158, 22);
            this.toolStripMenuItem6.Tag = "StartSoundTool";
            this.toolStripMenuItem6.Text = "StartSoundTool";
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.debugToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debugAction0ToolStripMenuItem,
            this.debugAction1ToolStripMenuItem,
            this.debugAction2ToolStripMenuItem,
            this.debugAction3ToolStripMenuItem,
            this.debugAction4ToolStripMenuItem,
            this.debugAction5ToolStripMenuItem,
            this.debugScriptToolStripMenuItem});
            this.debugToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 0, 120, 0);
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            this.debugToolStripMenuItem.Visible = false;
            // 
            // debugAction0ToolStripMenuItem
            // 
            this.debugAction0ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction0ToolStripMenuItem.Name = "debugAction0ToolStripMenuItem";
            this.debugAction0ToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.debugAction0ToolStripMenuItem.Text = "Debug Action 0";
            this.debugAction0ToolStripMenuItem.Click += new System.EventHandler(this.debugAction0ToolStripMenuItem_Click);
            // 
            // debugAction1ToolStripMenuItem
            // 
            this.debugAction1ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction1ToolStripMenuItem.Name = "debugAction1ToolStripMenuItem";
            this.debugAction1ToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.debugAction1ToolStripMenuItem.Text = "Debug Action 1";
            this.debugAction1ToolStripMenuItem.Click += new System.EventHandler(this.debugAction1ToolStripMenuItem_Click);
            // 
            // debugAction2ToolStripMenuItem
            // 
            this.debugAction2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction2ToolStripMenuItem.Name = "debugAction2ToolStripMenuItem";
            this.debugAction2ToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.debugAction2ToolStripMenuItem.Text = "Debug Action 2";
            this.debugAction2ToolStripMenuItem.Click += new System.EventHandler(this.debugAction2ToolStripMenuItem_Click);
            // 
            // debugAction3ToolStripMenuItem
            // 
            this.debugAction3ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction3ToolStripMenuItem.Name = "debugAction3ToolStripMenuItem";
            this.debugAction3ToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.debugAction3ToolStripMenuItem.Text = "Debug Action 3";
            this.debugAction3ToolStripMenuItem.Click += new System.EventHandler(this.debugAction3ToolStripMenuItem_Click);
            // 
            // debugAction4ToolStripMenuItem
            // 
            this.debugAction4ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction4ToolStripMenuItem.Name = "debugAction4ToolStripMenuItem";
            this.debugAction4ToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.debugAction4ToolStripMenuItem.Text = "Debug Action 4";
            this.debugAction4ToolStripMenuItem.Click += new System.EventHandler(this.debugAction4ToolStripMenuItem_Click);
            // 
            // debugAction5ToolStripMenuItem
            // 
            this.debugAction5ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction5ToolStripMenuItem.Name = "debugAction5ToolStripMenuItem";
            this.debugAction5ToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.debugAction5ToolStripMenuItem.Text = "Debug Action 5";
            this.debugAction5ToolStripMenuItem.Click += new System.EventHandler(this.debugAction5ToolStripMenuItem_Click);
            // 
            // debugScriptToolStripMenuItem
            // 
            this.debugScriptToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.debugScriptToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugScriptToolStripMenuItem.Name = "debugScriptToolStripMenuItem";
            this.debugScriptToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.debugScriptToolStripMenuItem.Text = "Debug script";
            this.debugScriptToolStripMenuItem.Click += new System.EventHandler(this.debugScriptToolStripMenuItem_Click);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.restoreDefaultLayoutToolStripMenuItem,
            this.toolStripMenuSeparator14,
            this.sectorOptionsToolStripMenuItem,
            this.roomOptionsToolStripMenuItem,
            this.itemBrowserToolStripMenuItem,
            this.importedGeometryBrowserToolstripMenuItem,
            this.triggerListToolStripMenuItem,
            this.lightingToolStripMenuItem,
            this.paletteToolStripMenuItem,
            this.texturePanelToolStripMenuItem,
            this.objectListToolStripMenuItem,
            this.toolPaletteToolStripMenuItem});
            this.windowToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // restoreDefaultLayoutToolStripMenuItem
            // 
            this.restoreDefaultLayoutToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.restoreDefaultLayoutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.restoreDefaultLayoutToolStripMenuItem.Name = "restoreDefaultLayoutToolStripMenuItem";
            this.restoreDefaultLayoutToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.restoreDefaultLayoutToolStripMenuItem.Text = "Restore default layout";
            this.restoreDefaultLayoutToolStripMenuItem.Click += new System.EventHandler(this.restoreDefaultLayoutToolStripMenuItem_Click);
            // 
            // toolStripMenuSeparator14
            // 
            this.toolStripMenuSeparator14.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuSeparator14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuSeparator14.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuSeparator14.Name = "toolStripMenuSeparator14";
            this.toolStripMenuSeparator14.Size = new System.Drawing.Size(191, 6);
            // 
            // sectorOptionsToolStripMenuItem
            // 
            this.sectorOptionsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.sectorOptionsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.sectorOptionsToolStripMenuItem.Name = "sectorOptionsToolStripMenuItem";
            this.sectorOptionsToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.sectorOptionsToolStripMenuItem.Text = "Sector Options";
            this.sectorOptionsToolStripMenuItem.Click += new System.EventHandler(this.sectorOptionsToolStripMenuItem_Click);
            // 
            // roomOptionsToolStripMenuItem
            // 
            this.roomOptionsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.roomOptionsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.roomOptionsToolStripMenuItem.Name = "roomOptionsToolStripMenuItem";
            this.roomOptionsToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.roomOptionsToolStripMenuItem.Text = "Room Options";
            this.roomOptionsToolStripMenuItem.Click += new System.EventHandler(this.roomOptionsToolStripMenuItem_Click);
            // 
            // itemBrowserToolStripMenuItem
            // 
            this.itemBrowserToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.itemBrowserToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.itemBrowserToolStripMenuItem.Name = "itemBrowserToolStripMenuItem";
            this.itemBrowserToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.itemBrowserToolStripMenuItem.Text = "Item Browser";
            this.itemBrowserToolStripMenuItem.Click += new System.EventHandler(this.itemBrowserToolStripMenuItem_Click);
            // 
            // importedGeometryBrowserToolstripMenuItem
            // 
            this.importedGeometryBrowserToolstripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.importedGeometryBrowserToolstripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.importedGeometryBrowserToolstripMenuItem.Name = "importedGeometryBrowserToolstripMenuItem";
            this.importedGeometryBrowserToolstripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.importedGeometryBrowserToolstripMenuItem.Text = "Imported Geometry Browser";
            this.importedGeometryBrowserToolstripMenuItem.Click += new System.EventHandler(this.importedGeometryBrowserToolstripMenuItem_Click);
            // 
            // triggerListToolStripMenuItem
            // 
            this.triggerListToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.triggerListToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.triggerListToolStripMenuItem.Name = "triggerListToolStripMenuItem";
            this.triggerListToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.triggerListToolStripMenuItem.Text = "Trigger List";
            this.triggerListToolStripMenuItem.Click += new System.EventHandler(this.triggerListToolStripMenuItem_Click);
            // 
            // lightingToolStripMenuItem
            // 
            this.lightingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.lightingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lightingToolStripMenuItem.Name = "lightingToolStripMenuItem";
            this.lightingToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.lightingToolStripMenuItem.Text = "Lighting";
            this.lightingToolStripMenuItem.Click += new System.EventHandler(this.lightingToolStripMenuItem_Click);
            // 
            // paletteToolStripMenuItem
            // 
            this.paletteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.paletteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.paletteToolStripMenuItem.Name = "paletteToolStripMenuItem";
            this.paletteToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.paletteToolStripMenuItem.Text = "Palette";
            this.paletteToolStripMenuItem.Click += new System.EventHandler(this.paletteToolStripMenuItem_Click);
            // 
            // texturePanelToolStripMenuItem
            // 
            this.texturePanelToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.texturePanelToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.texturePanelToolStripMenuItem.Name = "texturePanelToolStripMenuItem";
            this.texturePanelToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.texturePanelToolStripMenuItem.Text = "Texture Panel";
            this.texturePanelToolStripMenuItem.Click += new System.EventHandler(this.texturePanelToolStripMenuItem_Click);
            // 
            // objectListToolStripMenuItem
            // 
            this.objectListToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.objectListToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.objectListToolStripMenuItem.Name = "objectListToolStripMenuItem";
            this.objectListToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.objectListToolStripMenuItem.Text = "Object List";
            this.objectListToolStripMenuItem.Click += new System.EventHandler(this.objectListToolStripMenuItem_Click);
            // 
            // toolPaletteToolStripMenuItem
            // 
            this.toolPaletteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolPaletteToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dockableToolStripMenuItem,
            this.floatingToolStripMenuItem});
            this.toolPaletteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolPaletteToolStripMenuItem.Name = "toolPaletteToolStripMenuItem";
            this.toolPaletteToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.toolPaletteToolStripMenuItem.Text = "Tool Palette";
            // 
            // dockableToolStripMenuItem
            // 
            this.dockableToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.dockableToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.dockableToolStripMenuItem.Name = "dockableToolStripMenuItem";
            this.dockableToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
            this.dockableToolStripMenuItem.Text = "Dockable";
            this.dockableToolStripMenuItem.Click += new System.EventHandler(this.dockableToolStripMenuItem_Click);
            // 
            // floatingToolStripMenuItem
            // 
            this.floatingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.floatingToolStripMenuItem.CheckOnClick = true;
            this.floatingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.floatingToolStripMenuItem.Name = "floatingToolStripMenuItem";
            this.floatingToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
            this.floatingToolStripMenuItem.Text = "Floating";
            this.floatingToolStripMenuItem.CheckedChanged += new System.EventHandler(this.floatingToolStripMenuItem_CheckedChanged);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.aboutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.aboutToolStripMenuItem.Image = global::TombEditor.Properties.Resources.general_AboutIcon_16;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.aboutToolStripMenuItem.Text = "About Tomb Editor...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // butRoomDown
            // 
            this.butRoomDown.Checked = false;
            this.butRoomDown.Location = new System.Drawing.Point(0, 0);
            this.butRoomDown.Name = "butRoomDown";
            this.butRoomDown.Size = new System.Drawing.Size(75, 23);
            this.butRoomDown.TabIndex = 0;
            // 
            // butEditRoomName
            // 
            this.butEditRoomName.Checked = false;
            this.butEditRoomName.Location = new System.Drawing.Point(0, 0);
            this.butEditRoomName.Name = "butEditRoomName";
            this.butEditRoomName.Size = new System.Drawing.Size(75, 23);
            this.butEditRoomName.TabIndex = 0;
            // 
            // butDeleteRoom
            // 
            this.butDeleteRoom.Checked = false;
            this.butDeleteRoom.Location = new System.Drawing.Point(0, 0);
            this.butDeleteRoom.Name = "butDeleteRoom";
            this.butDeleteRoom.Size = new System.Drawing.Size(75, 23);
            this.butDeleteRoom.TabIndex = 0;
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.statusStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusStripSelectedRoom,
            this.statusStripGlobalSelectionArea,
            this.statusStripLocalSelectionArea,
            this.statusAutosave,
            this.statusLastCompilation});
            this.statusStrip.Location = new System.Drawing.Point(0, 440);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.statusStrip.Size = new System.Drawing.Size(913, 29);
            this.statusStrip.TabIndex = 29;
            this.statusStrip.Text = "statusStrip";
            // 
            // statusStripSelectedRoom
            // 
            this.statusStripSelectedRoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.statusStripSelectedRoom.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.statusStripSelectedRoom.Margin = new System.Windows.Forms.Padding(20, 3, 0, 2);
            this.statusStripSelectedRoom.Name = "statusStripSelectedRoom";
            this.statusStripSelectedRoom.Size = new System.Drawing.Size(0, 16);
            this.statusStripSelectedRoom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusStripGlobalSelectionArea
            // 
            this.statusStripGlobalSelectionArea.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.statusStripGlobalSelectionArea.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.statusStripGlobalSelectionArea.Margin = new System.Windows.Forms.Padding(30, 3, 0, 2);
            this.statusStripGlobalSelectionArea.Name = "statusStripGlobalSelectionArea";
            this.statusStripGlobalSelectionArea.Size = new System.Drawing.Size(0, 16);
            this.statusStripGlobalSelectionArea.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusStripLocalSelectionArea
            // 
            this.statusStripLocalSelectionArea.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.statusStripLocalSelectionArea.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.statusStripLocalSelectionArea.Margin = new System.Windows.Forms.Padding(30, 3, 0, 2);
            this.statusStripLocalSelectionArea.Name = "statusStripLocalSelectionArea";
            this.statusStripLocalSelectionArea.Size = new System.Drawing.Size(0, 16);
            this.statusStripLocalSelectionArea.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusAutosave
            // 
            this.statusAutosave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.statusAutosave.Margin = new System.Windows.Forms.Padding(20, 3, 0, 2);
            this.statusAutosave.Name = "statusAutosave";
            this.statusAutosave.Size = new System.Drawing.Size(0, 16);
            // 
            // statusLastCompilation
            // 
            this.statusLastCompilation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.statusLastCompilation.Margin = new System.Windows.Forms.Padding(20, 3, 0, 2);
            this.statusLastCompilation.Name = "statusLastCompilation";
            this.statusLastCompilation.Size = new System.Drawing.Size(0, 16);
            // 
            // dockArea
            // 
            this.dockArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockArea.Location = new System.Drawing.Point(0, 0);
            this.dockArea.MinimumSize = new System.Drawing.Size(274, 274);
            this.dockArea.Name = "dockArea";
            this.dockArea.Padding = new System.Windows.Forms.Padding(2);
            this.dockArea.Size = new System.Drawing.Size(913, 416);
            this.dockArea.TabIndex = 90;
            this.dockArea.ContentAdded += new System.EventHandler<DarkUI.Docking.DockContentEventArgs>(this.ToolWindow_Added);
            this.dockArea.ContentRemoved += new System.EventHandler<DarkUI.Docking.DockContentEventArgs>(this.ToolWindow_Removed);
            // 
            // panelDockArea
            // 
            this.panelDockArea.Controls.Add(this.dockArea);
            this.panelDockArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDockArea.Location = new System.Drawing.Point(0, 24);
            this.panelDockArea.Name = "panelDockArea";
            this.panelDockArea.Size = new System.Drawing.Size(913, 416);
            this.panelDockArea.TabIndex = 26;
            // 
            // assToolStripMenuItem
            // 
            this.assToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.assToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.assToolStripMenuItem.Name = "assToolStripMenuItem";
            this.assToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.assToolStripMenuItem.Text = "ass";
            // 
            // FormMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(913, 469);
            this.Controls.Add(this.panelDockArea);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Tomb Editor";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panelDockArea.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkMenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem roomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem texturesToolStripMenuItem;
        private DarkUI.Controls.DarkButton butDeleteRoom;
        private DarkUI.Controls.DarkButton butEditRoomName;
        private DarkUI.Controls.DarkButton butRoomDown;
        private DarkUI.Controls.DarkStatusStrip statusStrip;
        private System.Windows.Forms.ToolStripMenuItem newLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem importTRLEPRJToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator2;
        private System.Windows.Forms.ToolStripMenuItem buildLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem buildLevelPlayToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator3;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smoothRandomFloorUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smoothRandomFloorDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem averageFloorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem averageCeilingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addFlybyCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSinkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSoundSourceToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator8;
        private System.Windows.Forms.ToolStripMenuItem findObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator9;
        private System.Windows.Forms.ToolStripMenuItem textureFloorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textureCeilingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textureWallsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator10;
        private System.Windows.Forms.ToolStripMenuItem animationRangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importConvertTexturesToPng;
        private System.Windows.Forms.ToolStripMenuItem addWadToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator6;
        private System.Windows.Forms.ToolStripStatusLabel statusStripSelectedRoom;
        private System.Windows.Forms.ToolStripMenuItem smoothRandomCeilingUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smoothRandomCeilingDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator12;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator5;
        private System.Windows.Forms.ToolStripMenuItem duplicateRoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem splitRoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cropRoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction0ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction4ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator13;
        private System.Windows.Forms.ToolStripMenuItem gridWallsIn3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gridWallsIn5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator11;
        private System.Windows.Forms.ToolStripMenuItem sharpRandomFloorUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sharpRandomFloorDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sharpRandomCeilingUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sharpRandomCeilingDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel statusStripGlobalSelectionArea;
        private System.Windows.Forms.ToolStripStatusLabel statusStripLocalSelectionArea;
        private System.Windows.Forms.ToolStripMenuItem removeTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeWadsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadWadsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveLaraToolStripMenuItem;
        private DarkUI.Docking.DarkDockPanel dockArea;
        private System.Windows.Forms.Panel panelDockArea;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreDefaultLayoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator14;
        private System.Windows.Forms.ToolStripMenuItem sectorOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem roomOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemBrowserToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem triggerListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lightingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem paletteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem texturePanelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addGhostBlockToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator7;
        private System.Windows.Forms.ToolStripMenuItem addPortalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addTriggerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem transformToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exportRoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolPaletteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dockableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem floatingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel statusLastCompilation;
        private System.Windows.Forms.ToolStripMenuItem applyCurrentAmbientLightToAllRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel statusAutosave;
        private System.Windows.Forms.ToolStripMenuItem unloadTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem remapTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stampToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem bookmarkObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bookmarkRestoreObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem editObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem keyboardLayoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeRoomsHorizontallyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem splitSectorObjectOnSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearAllTexturesInRoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openRecentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flattenFloorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flattenCeilingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wholeRoomUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wholeRoomDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomLeftToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomRightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomForwardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomBackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomLeftToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomRightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomFrontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomBackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem clearAllTexturesInRoomToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem gridWallsIn3SquaresToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gridWallsIn5SquaresToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetGeometryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomUp4ClicksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomDown4ClicksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleFlyModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem relocateCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem transformRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotateRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotateRoomsCountercockwiseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mirrorRoomsOnXAxisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mirrorRoomsOnZAxisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectWaterRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectSkyRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectOutsideRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem selectConnectedRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectRoomsByTagsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem setStaticMeshColorToRoomLightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setStaticMeshesColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem reloadSoundsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem makeQuickItemGroupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addImportedGeometryToolStripMenuItem;
        private ToolStripMenuItem searchAndReplaceToolStripMenuItem;
        private ToolStripMenuItem findUntexturedToolStripMenuItem;
        private ToolStripMenuItem addSphereVolumeToolStripMenuItem;
        private ToolStripMenuItem addPrismVolumeToolStripMenuItem;
        private ToolStripMenuItem addBoxVolumeToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem8;
        private ToolStripMenuItem assToolStripMenuItem;
        private ToolStripMenuItem drawWhiteTextureLightingOnlyToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem ShowRealTintForObjectsToolStripMenuItem;
        private ToolStripMenuItem deleteAllObjectsToolStripMenuItem;
        private ToolStripMenuItem deleteAllTriggersToolStripMenuItem;
        private ToolStripMenuItem importedGeometryBrowserToolstripMenuItem;
    }
}