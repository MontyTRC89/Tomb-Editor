namespace TombEditor.ToolWindows
{
    partial class RoomOptions
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
            this.components = new System.ComponentModel.Container();
            this.cbNoPathfinding = new DarkUI.Controls.DarkCheckBox();
            this.cbHorizon = new DarkUI.Controls.DarkCheckBox();
            this.comboFlipMap = new DarkUI.Controls.DarkComboBox();
            this.darkLabel19 = new DarkUI.Controls.DarkLabel();
            this.comboReverberation = new DarkUI.Controls.DarkComboBox();
            this.darkLabel18 = new DarkUI.Controls.DarkLabel();
            this.comboMist = new DarkUI.Controls.DarkComboBox();
            this.darkLabel17 = new DarkUI.Controls.DarkLabel();
            this.comboReflection = new DarkUI.Controls.DarkComboBox();
            this.darkLabel16 = new DarkUI.Controls.DarkLabel();
            this.cbFlagOutside = new DarkUI.Controls.DarkCheckBox();
            this.cbFlagCold = new DarkUI.Controls.DarkCheckBox();
            this.cbFlagDamage = new DarkUI.Controls.DarkCheckBox();
            this.comboRoomType = new DarkUI.Controls.DarkComboBox();
            this.darkLabel15 = new DarkUI.Controls.DarkLabel();
            this.comboRoom = new DarkUI.Controls.DarkComboBox();
            this.panelRoomAmbientLight = new System.Windows.Forms.Panel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.butRoomUp = new DarkUI.Controls.DarkButton();
            this.butRoomDown = new DarkUI.Controls.DarkButton();
            this.butEditRoomName = new DarkUI.Controls.DarkButton();
            this.butCropRoom = new DarkUI.Controls.DarkButton();
            this.butSplitRoom = new DarkUI.Controls.DarkButton();
            this.cbNoLensflare = new DarkUI.Controls.DarkCheckBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.cbLocked = new DarkUI.Controls.DarkCheckBox();
            this.SuspendLayout();
            // 
            // cbNoPathfinding
            // 
            this.cbNoPathfinding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbNoPathfinding.AutoSize = true;
            this.cbNoPathfinding.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbNoPathfinding.Location = new System.Drawing.Point(108, 125);
            this.cbNoPathfinding.Name = "cbNoPathfinding";
            this.cbNoPathfinding.Size = new System.Drawing.Size(106, 17);
            this.cbNoPathfinding.TabIndex = 15;
            this.cbNoPathfinding.Text = "No pathfinding";
            this.toolTip.SetToolTip(this.cbNoPathfinding, "Disable zones and boxes generation");
            this.cbNoPathfinding.CheckedChanged += new System.EventHandler(this.cbNoPathfinding_CheckedChanged);
            // 
            // cbHorizon
            // 
            this.cbHorizon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbHorizon.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbHorizon.Location = new System.Drawing.Point(108, 79);
            this.cbHorizon.Name = "cbHorizon";
            this.cbHorizon.Size = new System.Drawing.Size(58, 17);
            this.cbHorizon.TabIndex = 12;
            this.cbHorizon.Text = "Skybox";
            this.toolTip.SetToolTip(this.cbHorizon, "Skybox is visible");
            this.cbHorizon.CheckedChanged += new System.EventHandler(this.cbHorizon_CheckedChanged);
            // 
            // comboFlipMap
            // 
            this.comboFlipMap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboFlipMap.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboFlipMap.Items.AddRange(new object[] {
            "Not flipped",
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15"});
            this.comboFlipMap.Location = new System.Drawing.Point(3, 71);
            this.comboFlipMap.Name = "comboFlipMap";
            this.comboFlipMap.Size = new System.Drawing.Size(99, 23);
            this.comboFlipMap.TabIndex = 8;
            this.comboFlipMap.SelectedIndexChanged += new System.EventHandler(this.comboFlipMap_SelectedIndexChanged);
            // 
            // darkLabel19
            // 
            this.darkLabel19.AutoSize = true;
            this.darkLabel19.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel19.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel19.Location = new System.Drawing.Point(0, 55);
            this.darkLabel19.Name = "darkLabel19";
            this.darkLabel19.Size = new System.Drawing.Size(48, 13);
            this.darkLabel19.TabIndex = 103;
            this.darkLabel19.Text = "Flipmap";
            // 
            // comboReverberation
            // 
            this.comboReverberation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboReverberation.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboReverberation.Items.AddRange(new object[] {
            "None",
            "Small",
            "Medium",
            "Large",
            "Pipe"});
            this.comboReverberation.Location = new System.Drawing.Point(202, 192);
            this.comboReverberation.Name = "comboReverberation";
            this.comboReverberation.Size = new System.Drawing.Size(79, 23);
            this.comboReverberation.TabIndex = 19;
            this.comboReverberation.SelectedIndexChanged += new System.EventHandler(this.comboReverberation_SelectedIndexChanged);
            // 
            // darkLabel18
            // 
            this.darkLabel18.AutoSize = true;
            this.darkLabel18.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel18.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel18.Location = new System.Drawing.Point(199, 176);
            this.darkLabel18.Name = "darkLabel18";
            this.darkLabel18.Size = new System.Drawing.Size(42, 13);
            this.darkLabel18.TabIndex = 100;
            this.darkLabel18.Text = "Reverb";
            // 
            // comboMist
            // 
            this.comboMist.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboMist.Items.AddRange(new object[] {
            "No",
            "1",
            "2",
            "3",
            "4"});
            this.comboMist.Location = new System.Drawing.Point(144, 192);
            this.comboMist.Name = "comboMist";
            this.comboMist.Size = new System.Drawing.Size(52, 23);
            this.comboMist.TabIndex = 18;
            this.comboMist.SelectedIndexChanged += new System.EventHandler(this.comboMist_SelectedIndexChanged);
            // 
            // darkLabel17
            // 
            this.darkLabel17.AutoSize = true;
            this.darkLabel17.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel17.Location = new System.Drawing.Point(141, 176);
            this.darkLabel17.Name = "darkLabel17";
            this.darkLabel17.Size = new System.Drawing.Size(29, 13);
            this.darkLabel17.TabIndex = 98;
            this.darkLabel17.Text = "Mist";
            // 
            // comboReflection
            // 
            this.comboReflection.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboReflection.Items.AddRange(new object[] {
            "No",
            "1",
            "2",
            "3",
            "4"});
            this.comboReflection.Location = new System.Drawing.Point(86, 192);
            this.comboReflection.Name = "comboReflection";
            this.comboReflection.Size = new System.Drawing.Size(52, 23);
            this.comboReflection.TabIndex = 17;
            this.comboReflection.SelectedIndexChanged += new System.EventHandler(this.comboReflection_SelectedIndexChanged);
            // 
            // darkLabel16
            // 
            this.darkLabel16.AutoSize = true;
            this.darkLabel16.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel16.Location = new System.Drawing.Point(83, 176);
            this.darkLabel16.Name = "darkLabel16";
            this.darkLabel16.Size = new System.Drawing.Size(59, 13);
            this.darkLabel16.TabIndex = 96;
            this.darkLabel16.Text = "Reflection";
            // 
            // cbFlagOutside
            // 
            this.cbFlagOutside.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbFlagOutside.AutoSize = true;
            this.cbFlagOutside.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbFlagOutside.Location = new System.Drawing.Point(199, 79);
            this.cbFlagOutside.Name = "cbFlagOutside";
            this.cbFlagOutside.Size = new System.Drawing.Size(54, 17);
            this.cbFlagOutside.TabIndex = 13;
            this.cbFlagOutside.Text = "Wind";
            this.toolTip.SetToolTip(this.cbFlagOutside, "Affects particles and Lara\'s hair");
            this.cbFlagOutside.CheckedChanged += new System.EventHandler(this.cbFlagOutside_CheckedChanged);
            // 
            // cbFlagCold
            // 
            this.cbFlagCold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbFlagCold.AutoSize = true;
            this.cbFlagCold.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbFlagCold.Location = new System.Drawing.Point(199, 56);
            this.cbFlagCold.Name = "cbFlagCold";
            this.cbFlagCold.Size = new System.Drawing.Size(50, 17);
            this.cbFlagCold.TabIndex = 11;
            this.cbFlagCold.Text = "Cold";
            this.toolTip.SetToolTip(this.cbFlagCold, "Room affects cold value (NGLE-only)");
            this.cbFlagCold.CheckedChanged += new System.EventHandler(this.cbFlagCold_CheckedChanged);
            // 
            // cbFlagDamage
            // 
            this.cbFlagDamage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbFlagDamage.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbFlagDamage.Location = new System.Drawing.Point(108, 56);
            this.cbFlagDamage.Name = "cbFlagDamage";
            this.cbFlagDamage.Size = new System.Drawing.Size(60, 17);
            this.cbFlagDamage.TabIndex = 10;
            this.cbFlagDamage.Text = "Damage";
            this.toolTip.SetToolTip(this.cbFlagDamage, "Room causes damage (NGLE-only)");
            this.cbFlagDamage.CheckedChanged += new System.EventHandler(this.cbFlagDamage_CheckedChanged);
            // 
            // comboRoomType
            // 
            this.comboRoomType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboRoomType.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboRoomType.Items.AddRange(new object[] {
            "Normal",
            "Water 1",
            "Water 2",
            "Water 3",
            "Water 4",
            "Rain 1",
            "Rain 2",
            "Rain 3",
            "Rain 4",
            "Snow 1",
            "Snow 2",
            "Snow 3",
            "Snow 4",
            "Quicksand 1",
            "Quicksand 2",
            "Quicksand 3",
            "Quicksand 4"});
            this.comboRoomType.Location = new System.Drawing.Point(3, 115);
            this.comboRoomType.Name = "comboRoomType";
            this.comboRoomType.Size = new System.Drawing.Size(99, 23);
            this.comboRoomType.TabIndex = 9;
            this.comboRoomType.SelectedIndexChanged += new System.EventHandler(this.comboRoomType_SelectedIndexChanged);
            // 
            // darkLabel15
            // 
            this.darkLabel15.AutoSize = true;
            this.darkLabel15.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel15.Location = new System.Drawing.Point(0, 99);
            this.darkLabel15.Name = "darkLabel15";
            this.darkLabel15.Size = new System.Drawing.Size(62, 13);
            this.darkLabel15.TabIndex = 91;
            this.darkLabel15.Text = "Room type";
            // 
            // comboRoom
            // 
            this.comboRoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboRoom.DropDownHeight = 406;
            this.comboRoom.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboRoom.IntegralHeight = false;
            this.comboRoom.Location = new System.Drawing.Point(3, 28);
            this.comboRoom.Name = "comboRoom";
            this.comboRoom.Size = new System.Drawing.Size(220, 23);
            this.comboRoom.TabIndex = 0;
            this.comboRoom.SelectedIndexChanged += new System.EventHandler(this.comboRoom_SelectedIndexChanged);
            // 
            // panelRoomAmbientLight
            // 
            this.panelRoomAmbientLight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRoomAmbientLight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelRoomAmbientLight.Location = new System.Drawing.Point(3, 193);
            this.panelRoomAmbientLight.Name = "panelRoomAmbientLight";
            this.panelRoomAmbientLight.Size = new System.Drawing.Size(67, 24);
            this.panelRoomAmbientLight.TabIndex = 16;
            this.panelRoomAmbientLight.Click += new System.EventHandler(this.panelRoomAmbientLight_Click);
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(0, 177);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(77, 13);
            this.darkLabel3.TabIndex = 88;
            this.darkLabel3.Text = "Ambient light";
            // 
            // colorDialog
            // 
            this.colorDialog.AnyColor = true;
            this.colorDialog.FullOpen = true;
            // 
            // butRoomUp
            // 
            this.butRoomUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butRoomUp.Image = global::TombEditor.Properties.Resources.actions_RoomRaise_16;
            this.butRoomUp.Location = new System.Drawing.Point(259, 88);
            this.butRoomUp.Name = "butRoomUp";
            this.butRoomUp.Size = new System.Drawing.Size(24, 24);
            this.butRoomUp.TabIndex = 6;
            this.toolTip.SetToolTip(this.butRoomUp, "Move room up");
            this.butRoomUp.Click += new System.EventHandler(this.butRoomUp_Click);
            // 
            // butRoomDown
            // 
            this.butRoomDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butRoomDown.Image = global::TombEditor.Properties.Resources.actions_RoomLower_16;
            this.butRoomDown.Location = new System.Drawing.Point(259, 118);
            this.butRoomDown.Name = "butRoomDown";
            this.butRoomDown.Size = new System.Drawing.Size(24, 24);
            this.butRoomDown.TabIndex = 7;
            this.toolTip.SetToolTip(this.butRoomDown, "Move room down");
            this.butRoomDown.Click += new System.EventHandler(this.butRoomDown_Click);
            // 
            // butEditRoomName
            // 
            this.butEditRoomName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butEditRoomName.Image = global::TombEditor.Properties.Resources.general_edit_16;
            this.butEditRoomName.Location = new System.Drawing.Point(229, 28);
            this.butEditRoomName.Name = "butEditRoomName";
            this.butEditRoomName.Size = new System.Drawing.Size(24, 24);
            this.butEditRoomName.TabIndex = 1;
            this.toolTip.SetToolTip(this.butEditRoomName, "Edit room name");
            this.butEditRoomName.Click += new System.EventHandler(this.butEditRoomName_Click);
            // 
            // butCropRoom
            // 
            this.butCropRoom.Image = global::TombEditor.Properties.Resources.general_crop_16;
            this.butCropRoom.Location = new System.Drawing.Point(259, 28);
            this.butCropRoom.Name = "butCropRoom";
            this.butCropRoom.Size = new System.Drawing.Size(24, 24);
            this.butCropRoom.TabIndex = 2;
            this.toolTip.SetToolTip(this.butCropRoom, "Crop room");
            this.butCropRoom.Click += new System.EventHandler(this.butCropRoom_Click);
            // 
            // butSplitRoom
            // 
            this.butSplitRoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSplitRoom.Image = global::TombEditor.Properties.Resources.general_split_files_16;
            this.butSplitRoom.Location = new System.Drawing.Point(259, 58);
            this.butSplitRoom.Name = "butSplitRoom";
            this.butSplitRoom.Size = new System.Drawing.Size(24, 24);
            this.butSplitRoom.TabIndex = 5;
            this.toolTip.SetToolTip(this.butSplitRoom, "Split room");
            this.butSplitRoom.Click += new System.EventHandler(this.butSplitRoom_Click);
            // 
            // cbNoLensflare
            // 
            this.cbNoLensflare.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbNoLensflare.AutoSize = true;
            this.cbNoLensflare.Location = new System.Drawing.Point(108, 102);
            this.cbNoLensflare.Name = "cbNoLensflare";
            this.cbNoLensflare.Size = new System.Drawing.Size(88, 17);
            this.cbNoLensflare.TabIndex = 14;
            this.cbNoLensflare.Text = "No lensflare";
            this.toolTip.SetToolTip(this.cbNoLensflare, "Disable global lensflare");
            this.cbNoLensflare.CheckedChanged += new System.EventHandler(this.cbNoLensflare_CheckedChanged);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // cbLocked
            // 
            this.cbLocked.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbLocked.AutoSize = true;
            this.cbLocked.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLocked.Location = new System.Drawing.Point(108, 148);
            this.cbLocked.Name = "cbLocked";
            this.cbLocked.Size = new System.Drawing.Size(105, 17);
            this.cbLocked.TabIndex = 104;
            this.cbLocked.Text = "Position locked";
            this.toolTip.SetToolTip(this.cbLocked, "Shows a warning message if this room is accidently moved on the 2D map.");
            this.cbLocked.CheckedChanged += new System.EventHandler(this.cbLocked_CheckedChanged);
            // 
            // RoomOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbLocked);
            this.Controls.Add(this.cbNoLensflare);
            this.Controls.Add(this.cbNoPathfinding);
            this.Controls.Add(this.cbHorizon);
            this.Controls.Add(this.comboFlipMap);
            this.Controls.Add(this.darkLabel19);
            this.Controls.Add(this.butRoomUp);
            this.Controls.Add(this.comboReverberation);
            this.Controls.Add(this.darkLabel18);
            this.Controls.Add(this.comboMist);
            this.Controls.Add(this.darkLabel17);
            this.Controls.Add(this.comboReflection);
            this.Controls.Add(this.darkLabel16);
            this.Controls.Add(this.cbFlagOutside);
            this.Controls.Add(this.cbFlagCold);
            this.Controls.Add(this.cbFlagDamage);
            this.Controls.Add(this.comboRoomType);
            this.Controls.Add(this.darkLabel15);
            this.Controls.Add(this.comboRoom);
            this.Controls.Add(this.panelRoomAmbientLight);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.butRoomDown);
            this.Controls.Add(this.butEditRoomName);
            this.Controls.Add(this.butCropRoom);
            this.Controls.Add(this.butSplitRoom);
            this.DockText = "Room Options";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(284, 218);
            this.Name = "RoomOptions";
            this.SerializationKey = "RoomOptions";
            this.Size = new System.Drawing.Size(284, 218);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkCheckBox cbNoPathfinding;
        private DarkUI.Controls.DarkCheckBox cbHorizon;
        private DarkUI.Controls.DarkComboBox comboFlipMap;
        private DarkUI.Controls.DarkLabel darkLabel19;
        private DarkUI.Controls.DarkButton butRoomUp;
        private DarkUI.Controls.DarkComboBox comboReverberation;
        private DarkUI.Controls.DarkLabel darkLabel18;
        private DarkUI.Controls.DarkComboBox comboMist;
        private DarkUI.Controls.DarkLabel darkLabel17;
        private DarkUI.Controls.DarkComboBox comboReflection;
        private DarkUI.Controls.DarkLabel darkLabel16;
        private DarkUI.Controls.DarkCheckBox cbFlagOutside;
        private DarkUI.Controls.DarkCheckBox cbFlagCold;
        private DarkUI.Controls.DarkCheckBox cbFlagDamage;
        private DarkUI.Controls.DarkComboBox comboRoomType;
        private DarkUI.Controls.DarkLabel darkLabel15;
        private DarkUI.Controls.DarkComboBox comboRoom;
        private System.Windows.Forms.Panel panelRoomAmbientLight;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkButton butRoomDown;
        private DarkUI.Controls.DarkButton butEditRoomName;
        private DarkUI.Controls.DarkButton butCropRoom;
        private DarkUI.Controls.DarkButton butSplitRoom;
        private System.Windows.Forms.ColorDialog colorDialog;
        private DarkUI.Controls.DarkCheckBox cbNoLensflare;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkCheckBox cbLocked;
    }
}
