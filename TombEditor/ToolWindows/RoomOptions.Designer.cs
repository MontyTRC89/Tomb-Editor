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
            this.darkLabel19 = new DarkUI.Controls.DarkLabel();
            this.darkLabel18 = new DarkUI.Controls.DarkLabel();
            this.darkLabel16 = new DarkUI.Controls.DarkLabel();
            this.cbFlagOutside = new DarkUI.Controls.DarkCheckBox();
            this.cbFlagCold = new DarkUI.Controls.DarkCheckBox();
            this.cbFlagDamage = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel15 = new DarkUI.Controls.DarkLabel();
            this.panelRoomAmbientLight = new System.Windows.Forms.Panel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.cbNoLensflare = new DarkUI.Controls.DarkCheckBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.butSearch = new DarkUI.Controls.DarkButton();
            this.tbRoomTags = new DarkUI.Controls.DarkTextBox();
            this.butLocked = new DarkUI.Controls.DarkButton();
            this.butSplitRoom = new DarkUI.Controls.DarkButton();
            this.butRoomUp = new DarkUI.Controls.DarkButton();
            this.butRoomDown = new DarkUI.Controls.DarkButton();
            this.butEditRoomName = new DarkUI.Controls.DarkButton();
            this.butCropRoom = new DarkUI.Controls.DarkButton();
            this.comboFlipMap = new DarkUI.Controls.DarkComboBox();
            this.comboReverberation = new DarkUI.Controls.DarkComboBox();
            this.comboLightEffect = new DarkUI.Controls.DarkComboBox();
            this.comboRoomType = new DarkUI.Controls.DarkComboBox();
            this.comboRoom = new DarkUI.Controls.DarkComboBox();
            this.butDublicateRoom = new DarkUI.Controls.DarkButton();
            this.butDeleteRoom = new DarkUI.Controls.DarkButton();
            this.numLightEffectStrength = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.numLightEffectStrength)).BeginInit();
            this.SuspendLayout();
            // 
            // cbNoPathfinding
            // 
            this.cbNoPathfinding.AutoCheck = false;
            this.cbNoPathfinding.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbNoPathfinding.Location = new System.Drawing.Point(129, 130);
            this.cbNoPathfinding.Name = "cbNoPathfinding";
            this.cbNoPathfinding.Size = new System.Drawing.Size(96, 17);
            this.cbNoPathfinding.TabIndex = 12;
            this.cbNoPathfinding.Tag = "SetRoomNoPathfinding";
            this.cbNoPathfinding.Text = "No pathfinding";
            // 
            // cbHorizon
            // 
            this.cbHorizon.AutoCheck = false;
            this.cbHorizon.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbHorizon.Location = new System.Drawing.Point(3, 130);
            this.cbHorizon.Name = "cbHorizon";
            this.cbHorizon.Size = new System.Drawing.Size(58, 17);
            this.cbHorizon.TabIndex = 10;
            this.cbHorizon.Tag = "SetRoomSkybox";
            this.cbHorizon.Text = "Skybox";
            // 
            // darkLabel19
            // 
            this.darkLabel19.AutoSize = true;
            this.darkLabel19.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel19.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel19.Location = new System.Drawing.Point(87, 85);
            this.darkLabel19.Name = "darkLabel19";
            this.darkLabel19.Size = new System.Drawing.Size(48, 13);
            this.darkLabel19.TabIndex = 103;
            this.darkLabel19.Text = "Flipmap";
            // 
            // darkLabel18
            // 
            this.darkLabel18.AutoSize = true;
            this.darkLabel18.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel18.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel18.Location = new System.Drawing.Point(148, 84);
            this.darkLabel18.Name = "darkLabel18";
            this.darkLabel18.Size = new System.Drawing.Size(42, 13);
            this.darkLabel18.TabIndex = 100;
            this.darkLabel18.Text = "Reverb";
            // 
            // darkLabel16
            // 
            this.darkLabel16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel16.AutoSize = true;
            this.darkLabel16.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel16.Location = new System.Drawing.Point(79, 178);
            this.darkLabel16.Name = "darkLabel16";
            this.darkLabel16.Size = new System.Drawing.Size(125, 13);
            this.darkLabel16.TabIndex = 96;
            this.darkLabel16.Text = "Light / transform effect";
            // 
            // cbFlagOutside
            // 
            this.cbFlagOutside.AutoCheck = false;
            this.cbFlagOutside.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbFlagOutside.Location = new System.Drawing.Point(3, 151);
            this.cbFlagOutside.Name = "cbFlagOutside";
            this.cbFlagOutside.Size = new System.Drawing.Size(50, 17);
            this.cbFlagOutside.TabIndex = 14;
            this.cbFlagOutside.Tag = "SetRoomOutside";
            this.cbFlagOutside.Text = "Wind";
            // 
            // cbFlagCold
            // 
            this.cbFlagCold.AutoCheck = false;
            this.cbFlagCold.AutoSize = true;
            this.cbFlagCold.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbFlagCold.Location = new System.Drawing.Point(64, 151);
            this.cbFlagCold.Name = "cbFlagCold";
            this.cbFlagCold.Size = new System.Drawing.Size(50, 17);
            this.cbFlagCold.TabIndex = 13;
            this.cbFlagCold.Tag = "SetRoomCold";
            this.cbFlagCold.Text = "Cold";
            // 
            // cbFlagDamage
            // 
            this.cbFlagDamage.AutoCheck = false;
            this.cbFlagDamage.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbFlagDamage.Location = new System.Drawing.Point(64, 130);
            this.cbFlagDamage.Name = "cbFlagDamage";
            this.cbFlagDamage.Size = new System.Drawing.Size(64, 17);
            this.cbFlagDamage.TabIndex = 9;
            this.cbFlagDamage.Tag = "SetRoomDamage";
            this.cbFlagDamage.Text = "Damage";
            // 
            // darkLabel15
            // 
            this.darkLabel15.AutoSize = true;
            this.darkLabel15.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel15.Location = new System.Drawing.Point(0, 85);
            this.darkLabel15.Name = "darkLabel15";
            this.darkLabel15.Size = new System.Drawing.Size(62, 13);
            this.darkLabel15.TabIndex = 91;
            this.darkLabel15.Text = "Room type";
            // 
            // panelRoomAmbientLight
            // 
            this.panelRoomAmbientLight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelRoomAmbientLight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRoomAmbientLight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelRoomAmbientLight.Location = new System.Drawing.Point(3, 194);
            this.panelRoomAmbientLight.Name = "panelRoomAmbientLight";
            this.panelRoomAmbientLight.Size = new System.Drawing.Size(67, 23);
            this.panelRoomAmbientLight.TabIndex = 15;
            this.panelRoomAmbientLight.Click += new System.EventHandler(this.panelRoomAmbientLight_Click);
            // 
            // darkLabel3
            // 
            this.darkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(0, 178);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(77, 13);
            this.darkLabel3.TabIndex = 88;
            this.darkLabel3.Text = "Ambient light";
            // 
            // cbNoLensflare
            // 
            this.cbNoLensflare.AutoCheck = false;
            this.cbNoLensflare.AutoSize = true;
            this.cbNoLensflare.Location = new System.Drawing.Point(129, 151);
            this.cbNoLensflare.Name = "cbNoLensflare";
            this.cbNoLensflare.Size = new System.Drawing.Size(88, 17);
            this.cbNoLensflare.TabIndex = 11;
            this.cbNoLensflare.Tag = "SetRoomNoLensflare";
            this.cbNoLensflare.Text = "No lensflare";
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // butSearch
            // 
            this.butSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSearch.Image = global::TombEditor.Properties.Resources.general_search_16;
            this.butSearch.Location = new System.Drawing.Point(228, 28);
            this.butSearch.Name = "butSearch";
            this.butSearch.Selectable = false;
            this.butSearch.Size = new System.Drawing.Size(24, 23);
            this.butSearch.TabIndex = 106;
            this.toolTip.SetToolTip(this.butSearch, "Search for rooms");
            this.butSearch.Click += new System.EventHandler(this.butSearch_Click);
            // 
            // tbRoomTags
            // 
            this.tbRoomTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbRoomTags.Location = new System.Drawing.Point(39, 57);
            this.tbRoomTags.Name = "tbRoomTags";
            this.tbRoomTags.Size = new System.Drawing.Size(183, 22);
            this.tbRoomTags.TabIndex = 109;
            this.tbRoomTags.Tag = "SetRoomTags";
            this.toolTip.SetToolTip(this.tbRoomTags, "Set room tags, separated by spaces");
            this.tbRoomTags.TextChanged += new System.EventHandler(this.TbTags_TextChanged);
            // 
            // butLocked
            // 
            this.butLocked.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butLocked.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.butLocked.Image = global::TombEditor.Properties.Resources.general_Lock_16;
            this.butLocked.Location = new System.Drawing.Point(228, 57);
            this.butLocked.Name = "butLocked";
            this.butLocked.Size = new System.Drawing.Size(54, 22);
            this.butLocked.TabIndex = 2;
            this.butLocked.Tag = "LockRoom";
            // 
            // butSplitRoom
            // 
            this.butSplitRoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSplitRoom.Image = global::TombEditor.Properties.Resources.actions_Split_16;
            this.butSplitRoom.Location = new System.Drawing.Point(228, 143);
            this.butSplitRoom.Name = "butSplitRoom";
            this.butSplitRoom.Size = new System.Drawing.Size(24, 23);
            this.butSplitRoom.TabIndex = 4;
            this.butSplitRoom.Tag = "SplitRoom";
            // 
            // butRoomUp
            // 
            this.butRoomUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butRoomUp.Image = global::TombEditor.Properties.Resources.general_ArrowUp_16;
            this.butRoomUp.Location = new System.Drawing.Point(258, 114);
            this.butRoomUp.Name = "butRoomUp";
            this.butRoomUp.Size = new System.Drawing.Size(24, 23);
            this.butRoomUp.TabIndex = 5;
            this.butRoomUp.Tag = "MoveRoomUp";
            // 
            // butRoomDown
            // 
            this.butRoomDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butRoomDown.Image = global::TombEditor.Properties.Resources.general_ArrowDown_16;
            this.butRoomDown.Location = new System.Drawing.Point(258, 143);
            this.butRoomDown.Name = "butRoomDown";
            this.butRoomDown.Size = new System.Drawing.Size(24, 23);
            this.butRoomDown.TabIndex = 6;
            this.butRoomDown.Tag = "MoveRoomDown";
            // 
            // butEditRoomName
            // 
            this.butEditRoomName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butEditRoomName.Image = global::TombEditor.Properties.Resources.general_edit_16;
            this.butEditRoomName.Location = new System.Drawing.Point(258, 28);
            this.butEditRoomName.Name = "butEditRoomName";
            this.butEditRoomName.Size = new System.Drawing.Size(24, 23);
            this.butEditRoomName.TabIndex = 1;
            this.butEditRoomName.Tag = "EditRoomName";
            // 
            // butCropRoom
            // 
            this.butCropRoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butCropRoom.Image = global::TombEditor.Properties.Resources.general_crop_16;
            this.butCropRoom.Location = new System.Drawing.Point(228, 114);
            this.butCropRoom.Name = "butCropRoom";
            this.butCropRoom.Size = new System.Drawing.Size(24, 23);
            this.butCropRoom.TabIndex = 3;
            this.butCropRoom.Tag = "CropRoom";
            // 
            // comboFlipMap
            // 
            this.comboFlipMap.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboFlipMap.Items.AddRange(new object[] {
            "None",
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
            this.comboFlipMap.Location = new System.Drawing.Point(90, 101);
            this.comboFlipMap.Name = "comboFlipMap";
            this.comboFlipMap.Size = new System.Drawing.Size(55, 23);
            this.comboFlipMap.TabIndex = 7;
            this.comboFlipMap.SelectedIndexChanged += new System.EventHandler(this.comboFlipMap_SelectedIndexChanged);
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
            this.comboReverberation.Location = new System.Drawing.Point(151, 101);
            this.comboReverberation.Name = "comboReverberation";
            this.comboReverberation.Size = new System.Drawing.Size(71, 23);
            this.comboReverberation.TabIndex = 18;
            this.comboReverberation.SelectedIndexChanged += new System.EventHandler(this.comboReverberation_SelectedIndexChanged);
            // 
            // comboLightEffect
            // 
            this.comboLightEffect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboLightEffect.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboLightEffect.Items.AddRange(new object[] {
            "None",
            "Default",
            "Reflection",
            "Glow",
            "Movement",
            "Glow & Movement",
            "Mist"});
            this.comboLightEffect.Location = new System.Drawing.Point(82, 194);
            this.comboLightEffect.Name = "comboLightEffect";
            this.comboLightEffect.Size = new System.Drawing.Size(140, 23);
            this.comboLightEffect.TabIndex = 16;
            this.comboLightEffect.SelectedIndexChanged += new System.EventHandler(this.comboLightEffect_SelectedIndexChanged);
            // 
            // comboRoomType
            // 
            this.comboRoomType.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboRoomType.Items.AddRange(new object[] {
            "Normal",
            "Water",
            "Quicksand",
            "Rain 1",
            "Rain 2",
            "Rain 3",
            "Rain 4",
            "Snow 1",
            "Snow 2",
            "Snow 3",
            "Snow 4"});
            this.comboRoomType.Location = new System.Drawing.Point(3, 101);
            this.comboRoomType.Name = "comboRoomType";
            this.comboRoomType.Size = new System.Drawing.Size(81, 23);
            this.comboRoomType.TabIndex = 8;
            this.comboRoomType.SelectedIndexChanged += new System.EventHandler(this.comboRoomType_SelectedIndexChanged);
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
            this.comboRoom.Size = new System.Drawing.Size(226, 23);
            this.comboRoom.TabIndex = 0;
            this.comboRoom.SelectedIndexChanged += new System.EventHandler(this.comboRoom_SelectedIndexChanged);
            // 
            // butDublicateRoom
            // 
            this.butDublicateRoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDublicateRoom.Image = global::TombEditor.Properties.Resources.general_copy_16;
            this.butDublicateRoom.Location = new System.Drawing.Point(228, 85);
            this.butDublicateRoom.Name = "butDublicateRoom";
            this.butDublicateRoom.Size = new System.Drawing.Size(24, 23);
            this.butDublicateRoom.TabIndex = 104;
            this.butDublicateRoom.Tag = "DuplicateRoom";
            // 
            // butDeleteRoom
            // 
            this.butDeleteRoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteRoom.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.butDeleteRoom.Location = new System.Drawing.Point(258, 85);
            this.butDeleteRoom.Name = "butDeleteRoom";
            this.butDeleteRoom.Size = new System.Drawing.Size(24, 23);
            this.butDeleteRoom.TabIndex = 105;
            this.butDeleteRoom.Tag = "DeleteRooms";
            // 
            // numLightEffectStrength
            // 
            this.numLightEffectStrength.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numLightEffectStrength.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numLightEffectStrength.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numLightEffectStrength.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numLightEffectStrength.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numLightEffectStrength.Location = new System.Drawing.Point(228, 194);
            this.numLightEffectStrength.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numLightEffectStrength.MousewheelSingleIncrement = true;
            this.numLightEffectStrength.Name = "numLightEffectStrength";
            this.numLightEffectStrength.Size = new System.Drawing.Size(54, 23);
            this.numLightEffectStrength.TabIndex = 107;
            this.numLightEffectStrength.ValueChanged += new System.EventHandler(this.numLightEffectStrength_ValueChanged);
            // 
            // darkLabel1
            // 
            this.darkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(225, 178);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(52, 13);
            this.darkLabel1.TabIndex = 108;
            this.darkLabel1.Text = "Strength";
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(0, 59);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(33, 13);
            this.darkLabel2.TabIndex = 110;
            this.darkLabel2.Text = "Tags:";
            // 
            // RoomOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.tbRoomTags);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.numLightEffectStrength);
            this.Controls.Add(this.butSearch);
            this.Controls.Add(this.butDeleteRoom);
            this.Controls.Add(this.butDublicateRoom);
            this.Controls.Add(this.butLocked);
            this.Controls.Add(this.cbNoLensflare);
            this.Controls.Add(this.cbNoPathfinding);
            this.Controls.Add(this.cbHorizon);
            this.Controls.Add(this.comboFlipMap);
            this.Controls.Add(this.darkLabel19);
            this.Controls.Add(this.butRoomUp);
            this.Controls.Add(this.comboReverberation);
            this.Controls.Add(this.darkLabel18);
            this.Controls.Add(this.comboLightEffect);
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
            this.MinimumSize = new System.Drawing.Size(284, 220);
            this.Name = "RoomOptions";
            this.SerializationKey = "RoomOptions";
            this.Size = new System.Drawing.Size(284, 220);
            ((System.ComponentModel.ISupportInitialize)(this.numLightEffectStrength)).EndInit();
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
        private DarkUI.Controls.DarkComboBox comboLightEffect;
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
        private DarkUI.Controls.DarkCheckBox cbNoLensflare;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkButton butLocked;
        private DarkUI.Controls.DarkButton butDublicateRoom;
        private DarkUI.Controls.DarkButton butDeleteRoom;
        private DarkUI.Controls.DarkButton butSearch;
        private DarkUI.Controls.DarkNumericUpDown numLightEffectStrength;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkTextBox tbRoomTags;
        private DarkUI.Controls.DarkLabel darkLabel2;
    }
}
