namespace TombEditor.ToolWindows
{
    partial class MainView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStrip = new DarkUI.Controls.DarkToolStrip();
            this.but2D = new System.Windows.Forms.ToolStripButton();
            this.but3D = new System.Windows.Forms.ToolStripButton();
            this.butFaceEdit = new System.Windows.Forms.ToolStripButton();
            this.butLightingMode = new System.Windows.Forms.ToolStripButton();
            this.butCenterCamera = new System.Windows.Forms.ToolStripButton();
            this.butDrawPortals = new System.Windows.Forms.ToolStripButton();
            this.butDrawHorizon = new System.Windows.Forms.ToolStripButton();
            this.butDrawRoomNames = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.butFlipMap = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.butCopy = new System.Windows.Forms.ToolStripButton();
            this.butPaste = new System.Windows.Forms.ToolStripButton();
            this.butStamp = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.butOpacityNone = new System.Windows.Forms.ToolStripButton();
            this.butOpacitySolidFaces = new System.Windows.Forms.ToolStripButton();
            this.butOpacityTraversableFaces = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.butTextureFloor = new System.Windows.Forms.ToolStripButton();
            this.butTextureCeiling = new System.Windows.Forms.ToolStripButton();
            this.butTextureWalls = new System.Windows.Forms.ToolStripButton();
            this.butAdditiveBlending = new System.Windows.Forms.ToolStripButton();
            this.butDoubleSided = new System.Windows.Forms.ToolStripButton();
            this.butInvisible = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.butAddCamera = new System.Windows.Forms.ToolStripButton();
            this.butAddFlybyCamera = new System.Windows.Forms.ToolStripButton();
            this.butAddSoundSource = new System.Windows.Forms.ToolStripButton();
            this.butAddSink = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.butCompileLevel = new System.Windows.Forms.ToolStripButton();
            this.butCompileLevelAndPlay = new System.Windows.Forms.ToolStripButton();
            this.panel3D = new TombEditor.Controls.PanelRendering3D();
            this.panel2DMap = new TombEditor.Controls.Panel2DMap();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // toolStrip
            //
            this.toolStrip.AutoSize = false;
            this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.but2D,
            this.but3D,
            this.butFaceEdit,
            this.butLightingMode,
            this.butCenterCamera,
            this.butDrawPortals,
            this.butDrawHorizon,
            this.butDrawRoomNames,
            this.toolStripSeparator1,
            this.butFlipMap,
            this.toolStripSeparator6,
            this.butCopy,
            this.butPaste,
            this.butStamp,
            this.toolStripSeparator5,
            this.butOpacityNone,
            this.butOpacitySolidFaces,
            this.butOpacityTraversableFaces,
            this.toolStripSeparator4,
            this.butTextureFloor,
            this.butTextureCeiling,
            this.butTextureWalls,
            this.butAdditiveBlending,
            this.butDoubleSided,
            this.butInvisible,
            this.toolStripSeparator2,
            this.butAddCamera,
            this.butAddFlybyCamera,
            this.butAddSoundSource,
            this.butAddSink,
            this.toolStripSeparator3,
            this.butCompileLevelAndPlay,
            this.butCompileLevel});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            this.toolStrip.Size = new System.Drawing.Size(1333, 28);
            this.toolStrip.TabIndex = 12;
            this.toolStrip.Text = "darkToolStrip1";
            //
            // but2D
            //
            this.but2D.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.but2D.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.but2D.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.but2D.Image = global::TombEditor.Properties.Resources._2DView_16;
            this.but2D.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.but2D.Name = "but2D";
            this.but2D.Size = new System.Drawing.Size(23, 25);
            this.but2D.Text = "2D map (F1)";
            this.but2D.Click += new System.EventHandler(this.but2D_Click);
            //
            // but3D
            //
            this.but3D.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.but3D.Checked = true;
            this.but3D.CheckState = System.Windows.Forms.CheckState.Checked;
            this.but3D.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.but3D.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.but3D.Image = global::TombEditor.Properties.Resources._3DView_16;
            this.but3D.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.but3D.Name = "but3D";
            this.but3D.Size = new System.Drawing.Size(23, 25);
            this.but3D.Text = "toolStripButton1";
            this.but3D.ToolTipText = "Geometry mode (F2)";
            this.but3D.Click += new System.EventHandler(this.but3D_Click);
            //
            // butFaceEdit
            //
            this.butFaceEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butFaceEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butFaceEdit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butFaceEdit.Image = global::TombEditor.Properties.Resources.TextureMode_16;
            this.butFaceEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butFaceEdit.Name = "butFaceEdit";
            this.butFaceEdit.Size = new System.Drawing.Size(23, 25);
            this.butFaceEdit.Text = "toolStripButton3";
            this.butFaceEdit.ToolTipText = "Face edit (F3)";
            this.butFaceEdit.Click += new System.EventHandler(this.butFaceEdit_Click);
            //
            // butLightingMode
            //
            this.butLightingMode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butLightingMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butLightingMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butLightingMode.Image = global::TombEditor.Properties.Resources.light_on_16;
            this.butLightingMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butLightingMode.Name = "butLightingMode";
            this.butLightingMode.Size = new System.Drawing.Size(23, 25);
            this.butLightingMode.Text = "toolStripButton4";
            this.butLightingMode.ToolTipText = "Lighting mode (F4)";
            this.butLightingMode.Click += new System.EventHandler(this.butLightingMode_Click);
            //
            // butCenterCamera
            //
            this.butCenterCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butCenterCamera.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butCenterCamera.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butCenterCamera.Image = global::TombEditor.Properties.Resources.center_direction_16;
            this.butCenterCamera.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butCenterCamera.Name = "butCenterCamera";
            this.butCenterCamera.Size = new System.Drawing.Size(23, 25);
            this.butCenterCamera.Text = "toolStripButton5";
            this.butCenterCamera.ToolTipText = "Center 3D camera (F6)";
            this.butCenterCamera.Click += new System.EventHandler(this.butCenterCamera_Click);
            //
            // butDrawPortals
            //
            this.butDrawPortals.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butDrawPortals.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butDrawPortals.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butDrawPortals.Image = global::TombEditor.Properties.Resources.door_opened_16;
            this.butDrawPortals.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butDrawPortals.Name = "butDrawPortals (F6)";
            this.butDrawPortals.Size = new System.Drawing.Size(23, 25);
            this.butDrawPortals.Text = "toolStripButton6";
            this.butDrawPortals.ToolTipText = "Draw portals";
            this.butDrawPortals.Click += new System.EventHandler(this.butDrawPortals_Click);
            //
            // butDrawHorizon
            //
            this.butDrawHorizon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butDrawHorizon.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butDrawHorizon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butDrawHorizon.Image = global::TombEditor.Properties.Resources.earth_element_16;
            this.butDrawHorizon.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butDrawHorizon.Name = "butDrawHorizon";
            this.butDrawHorizon.Size = new System.Drawing.Size(23, 25);
            this.butDrawHorizon.Text = "toolStripButton6";
            this.butDrawHorizon.ToolTipText = "Draw horizon";
            this.butDrawHorizon.Click += new System.EventHandler(this.butDrawHorizon_Click);
            //
            // butDrawRoomNames
            //
            this.butDrawRoomNames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butDrawRoomNames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butDrawRoomNames.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butDrawRoomNames.Image = global::TombEditor.Properties.Resources.generic_text_16;
            this.butDrawRoomNames.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butDrawRoomNames.Name = "butDrawRoomNames";
            this.butDrawRoomNames.Size = new System.Drawing.Size(23, 25);
            this.butDrawRoomNames.Text = "toolStripButton1";
            this.butDrawRoomNames.ToolTipText = "Draw room names";
            this.butDrawRoomNames.Click += new System.EventHandler(this.butDrawRoomNames_Click);
            //
            // toolStripSeparator1
            //
            this.toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            //
            // butFlipMap
            //
            this.butFlipMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butFlipMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butFlipMap.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butFlipMap.Image = global::TombEditor.Properties.Resources.copy_link_16;
            this.butFlipMap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butFlipMap.Name = "butFlipMap";
            this.butFlipMap.Size = new System.Drawing.Size(23, 25);
            this.butFlipMap.Text = "F";
            this.butFlipMap.ToolTipText = "Flip map";
            this.butFlipMap.Click += new System.EventHandler(this.butFlipMap_Click);
            //
            // toolStripSeparator6
            //
            this.toolStripSeparator6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator6.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 28);
            //
            // butCopy
            //
            this.butCopy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butCopy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butCopy.Image = global::TombEditor.Properties.Resources.copy_16;
            this.butCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butCopy.Name = "butCopy";
            this.butCopy.Size = new System.Drawing.Size(23, 25);
            this.butCopy.Text = "toolStripButton2";
            this.butCopy.ToolTipText = "Copy (Ctrl+C)";
            this.butCopy.Click += new System.EventHandler(this.butCopy_Click);
            this.butCopy.Enabled = false;
            //
            // butPaste
            //
            this.butPaste.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butPaste.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butPaste.Image = global::TombEditor.Properties.Resources.clipboard_16;
            this.butPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butPaste.Name = "butPaste";
            this.butPaste.Size = new System.Drawing.Size(23, 25);
            this.butPaste.Text = "toolStripButton2";
            this.butPaste.ToolTipText = "Paste (Ctrl+V)";
            this.butPaste.Click += new System.EventHandler(this.butPaste_Click);
            //
            // butStamp
            //
            this.butStamp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butStamp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butStamp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butStamp.Image = global::TombEditor.Properties.Resources.rubber_stamp_16;
            this.butStamp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butStamp.Name = "butStamp";
            this.butStamp.Size = new System.Drawing.Size(23, 25);
            this.butStamp.Text = "toolStripButton2";
            this.butStamp.ToolTipText = "Stamp (Ctrl+B)";
            this.butStamp.Click += new System.EventHandler(this.butStamp_Click);
            this.butStamp.Enabled = false;
            //
            // toolStripSeparator5
            //
            this.toolStripSeparator5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator5.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 28);
            //
            // butOpacityNone
            //
            this.butOpacityNone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butOpacityNone.CheckOnClick = true;
            this.butOpacityNone.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butOpacityNone.Enabled = false;
            this.butOpacityNone.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butOpacityNone.Image = global::TombEditor.Properties.Resources.rectangle_filled_16;
            this.butOpacityNone.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butOpacityNone.Name = "butOpacityNone";
            this.butOpacityNone.Size = new System.Drawing.Size(23, 25);
            this.butOpacityNone.Text = "toolStripButton1";
            this.butOpacityNone.ToolTipText = "Clear ('No Toggle Opacity')";
            this.butOpacityNone.Click += new System.EventHandler(this.butOpacityNone_Click);
            //
            // butOpacitySolidFaces
            //
            this.butOpacitySolidFaces.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butOpacitySolidFaces.CheckOnClick = true;
            this.butOpacitySolidFaces.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butOpacitySolidFaces.Enabled = false;
            this.butOpacitySolidFaces.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butOpacitySolidFaces.Image = global::TombEditor.Properties.Resources.ToggleOpacity1_16;
            this.butOpacitySolidFaces.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.butOpacitySolidFaces.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butOpacitySolidFaces.Name = "butOpacitySolidFaces";
            this.butOpacitySolidFaces.Size = new System.Drawing.Size(23, 25);
            this.butOpacitySolidFaces.Text = "toolStripButton2";
            this.butOpacitySolidFaces.ToolTipText = "Textured and solid ('Toggle Opacity 1')";
            this.butOpacitySolidFaces.Click += new System.EventHandler(this.butOpacitySolidFaces_Click);
            //
            // butOpacityTraversableFaces
            //
            this.butOpacityTraversableFaces.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butOpacityTraversableFaces.CheckOnClick = true;
            this.butOpacityTraversableFaces.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butOpacityTraversableFaces.Enabled = false;
            this.butOpacityTraversableFaces.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butOpacityTraversableFaces.Image = global::TombEditor.Properties.Resources.ToggleOpacity2_16;
            this.butOpacityTraversableFaces.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butOpacityTraversableFaces.Name = "butOpacityTraversableFaces";
            this.butOpacityTraversableFaces.Size = new System.Drawing.Size(23, 25);
            this.butOpacityTraversableFaces.Text = "toolStripButton3";
            this.butOpacityTraversableFaces.ToolTipText = "Textured and traversable ('Toggle Opacity 2')";
            this.butOpacityTraversableFaces.Click += new System.EventHandler(this.butOpacityTraversableFaces_Click);
            //
            // toolStripSeparator4
            //
            this.toolStripSeparator4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator4.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 28);
            //
            // butTextureFloor
            //
            this.butTextureFloor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTextureFloor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTextureFloor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTextureFloor.Image = global::TombEditor.Properties.Resources.TextureFloor_16;
            this.butTextureFloor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTextureFloor.Name = "butTextureFloor";
            this.butTextureFloor.Size = new System.Drawing.Size(23, 25);
            this.butTextureFloor.Text = "toolStripButton4";
            this.butTextureFloor.ToolTipText = "Texture floor (Alt+T)";
            this.butTextureFloor.Click += new System.EventHandler(this.butTextureFloor_Click);
            this.butTextureFloor.Enabled = false;
            //
            // butTextureCeiling
            //
            this.butTextureCeiling.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTextureCeiling.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTextureCeiling.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTextureCeiling.Image = global::TombEditor.Properties.Resources.TextureCeiling_16;
            this.butTextureCeiling.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTextureCeiling.Name = "butTextureCeiling";
            this.butTextureCeiling.Size = new System.Drawing.Size(23, 25);
            this.butTextureCeiling.Text = "toolStripButton5";
            this.butTextureCeiling.ToolTipText = "Texture ceiling (Alt+V)";
            this.butTextureCeiling.Click += new System.EventHandler(this.butTextureCeiling_Click);
            this.butTextureCeiling.Enabled = false;
            //
            // butTextureWalls
            //
            this.butTextureWalls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butTextureWalls.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butTextureWalls.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butTextureWalls.Image = global::TombEditor.Properties.Resources.TextureWalls_16;
            this.butTextureWalls.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butTextureWalls.Name = "butTextureWalls";
            this.butTextureWalls.Size = new System.Drawing.Size(23, 25);
            this.butTextureWalls.Text = "toolStripButton6";
            this.butTextureWalls.ToolTipText = "Texture walls (Alt+U)";
            this.butTextureWalls.Click += new System.EventHandler(this.butTextureWalls_Click);
            this.butTextureWalls.Enabled = false;
            //
            // butAdditiveBlending
            //
            this.butAdditiveBlending.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butAdditiveBlending.CheckOnClick = true;
            this.butAdditiveBlending.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butAdditiveBlending.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butAdditiveBlending.Image = global::TombEditor.Properties.Resources.SemiTransparent_16;
            this.butAdditiveBlending.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butAdditiveBlending.Name = "butAdditiveBlending";
            this.butAdditiveBlending.Size = new System.Drawing.Size(23, 25);
            this.butAdditiveBlending.Text = "\'Transparent\' texture (Additive blending) (Shift+1)";
            this.butAdditiveBlending.Click += new System.EventHandler(this.butAdditiveBlending_Click);
            //
            // butDoubleSided
            //
            this.butDoubleSided.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butDoubleSided.CheckOnClick = true;
            this.butDoubleSided.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butDoubleSided.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butDoubleSided.Image = global::TombEditor.Properties.Resources.DoubleSided_16;
            this.butDoubleSided.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butDoubleSided.Name = "butDoubleSided";
            this.butDoubleSided.Size = new System.Drawing.Size(23, 25);
            this.butDoubleSided.Text = "Double sided texture (Shift+2)";
            this.butDoubleSided.Click += new System.EventHandler(this.butDoubleSided_Click);
            //
            // butInvisible
            //
            this.butInvisible.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butInvisible.CheckOnClick = true;
            this.butInvisible.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butInvisible.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butInvisible.Image = global::TombEditor.Properties.Resources.invisible_16;
            this.butInvisible.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butInvisible.Name = "butInvisible";
            this.butInvisible.Size = new System.Drawing.Size(23, 25);
            this.butInvisible.Text = "Invisible face (Shift+3)";
            //
            // toolStripSeparator2
            //
            this.toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
            //
            // butAddCamera
            //
            this.butAddCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butAddCamera.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butAddCamera.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butAddCamera.Image = global::TombEditor.Properties.Resources.Camera_161;
            this.butAddCamera.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butAddCamera.Name = "butAddCamera";
            this.butAddCamera.Size = new System.Drawing.Size(23, 25);
            this.butAddCamera.Text = "Add camera";
            this.butAddCamera.Click += new System.EventHandler(this.butAddCamera_Click);
            //
            // butAddFlybyCamera
            //
            this.butAddFlybyCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butAddFlybyCamera.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butAddFlybyCamera.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butAddFlybyCamera.Image = global::TombEditor.Properties.Resources.movie_projector_16;
            this.butAddFlybyCamera.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butAddFlybyCamera.Name = "butAddFlybyCamera";
            this.butAddFlybyCamera.Size = new System.Drawing.Size(23, 25);
            this.butAddFlybyCamera.Text = "toolStripButton2";
            this.butAddFlybyCamera.ToolTipText = "Add fly-by camera";
            this.butAddFlybyCamera.Click += new System.EventHandler(this.butAddFlybyCamera_Click);
            //
            // butAddSoundSource
            //
            this.butAddSoundSource.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butAddSoundSource.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butAddSoundSource.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butAddSoundSource.Image = global::TombEditor.Properties.Resources.volume_up_16;
            this.butAddSoundSource.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butAddSoundSource.Name = "butAddSoundSource";
            this.butAddSoundSource.Size = new System.Drawing.Size(23, 25);
            this.butAddSoundSource.Text = "toolStripButton3";
            this.butAddSoundSource.ToolTipText = "Add sound source";
            this.butAddSoundSource.Click += new System.EventHandler(this.butAddSoundSource_Click);
            //
            // butAddSink
            //
            this.butAddSink.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butAddSink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butAddSink.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butAddSink.Image = global::TombEditor.Properties.Resources.tornado_16;
            this.butAddSink.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butAddSink.Name = "butAddSink";
            this.butAddSink.Size = new System.Drawing.Size(23, 25);
            this.butAddSink.Text = "toolStripButton4";
            this.butAddSink.ToolTipText = "Add sink";
            this.butAddSink.Click += new System.EventHandler(this.butAddSink_Click);
            //
            // toolStripSeparator3
            //
            this.toolStripSeparator3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
            //
            // butCompileLevel
            //
            this.butCompileLevel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butCompileLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butCompileLevel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butCompileLevel.Image = global::TombEditor.Properties.Resources.software_installer_16;
            this.butCompileLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butCompileLevel.Name = "butCompileLevel";
            this.butCompileLevel.Size = new System.Drawing.Size(23, 25);
            this.butCompileLevel.Text = "toolStripButton1";
            this.butCompileLevel.ToolTipText = "Build level (Shift+F5)";
            this.butCompileLevel.Click += new System.EventHandler(this.butCompileLevel_Click);
            //
            // butCompileLevelAndPlay
            //
            this.butCompileLevelAndPlay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butCompileLevelAndPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butCompileLevelAndPlay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butCompileLevelAndPlay.Image = global::TombEditor.Properties.Resources.play_16;
            this.butCompileLevelAndPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butCompileLevelAndPlay.Name = "butCompileLevelAndPlay";
            this.butCompileLevelAndPlay.Size = new System.Drawing.Size(23, 25);
            this.butCompileLevelAndPlay.Text = "toolStripButton2";
            this.butCompileLevelAndPlay.ToolTipText = "Build level & play (F5)";
            this.butCompileLevelAndPlay.Click += new System.EventHandler(this.butCompileLevelAndPlay_Click);
            //
            // panel3D
            //
            this.panel3D.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            this.panel3D.AllowDrop = true;
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3D.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3D.Location = new System.Drawing.Point(3, 31);
            this.panel3D.Name = "panel3D";
            this.panel3D.Size = new System.Drawing.Size(1327, 703);
            this.panel3D.TabIndex = 13;
            //
            // panel2DMap
            //
            this.panel2DMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2DMap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2DMap.Location = new System.Drawing.Point(4, 31);
            this.panel2DMap.Name = "panel2DMap";
            this.panel2DMap.Size = new System.Drawing.Size(1326, 703);
            this.panel2DMap.TabIndex = 14;
            this.panel2DMap.Visible = false;
            //
            // MainView
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2DMap);
            this.Controls.Add(this.panel3D);
            this.Controls.Add(this.toolStrip);
            this.DockText = "";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Name = "MainView";
            this.SerializationKey = "MainView";
            this.Size = new System.Drawing.Size(1333, 737);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private DarkUI.Controls.DarkToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton but2D;
        private System.Windows.Forms.ToolStripButton but3D;
        private System.Windows.Forms.ToolStripButton butFaceEdit;
        private System.Windows.Forms.ToolStripButton butLightingMode;
        private System.Windows.Forms.ToolStripButton butCenterCamera;
        private System.Windows.Forms.ToolStripButton butDrawPortals;
        private System.Windows.Forms.ToolStripButton butDrawHorizon;
        private System.Windows.Forms.ToolStripButton butDrawRoomNames;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton butFlipMap;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton butCopy;
        private System.Windows.Forms.ToolStripButton butPaste;
        private System.Windows.Forms.ToolStripButton butStamp;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton butOpacityNone;
        private System.Windows.Forms.ToolStripButton butOpacitySolidFaces;
        private System.Windows.Forms.ToolStripButton butOpacityTraversableFaces;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton butTextureFloor;
        private System.Windows.Forms.ToolStripButton butTextureCeiling;
        private System.Windows.Forms.ToolStripButton butTextureWalls;
        private System.Windows.Forms.ToolStripButton butAdditiveBlending;
        private System.Windows.Forms.ToolStripButton butDoubleSided;
        private System.Windows.Forms.ToolStripButton butInvisible;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton butAddCamera;
        private System.Windows.Forms.ToolStripButton butAddFlybyCamera;
        private System.Windows.Forms.ToolStripButton butAddSoundSource;
        private System.Windows.Forms.ToolStripButton butAddSink;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton butCompileLevel;
        private System.Windows.Forms.ToolStripButton butCompileLevelAndPlay;
        private TombEditor.Controls.PanelRendering3D panel3D;
        private TombEditor.Controls.Panel2DMap panel2DMap;
    }
}
