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
            this.comboFlipMap = new DarkUI.Controls.DarkComboBox(this.components);
            this.darkLabel19 = new DarkUI.Controls.DarkLabel();
            this.comboReverberation = new DarkUI.Controls.DarkComboBox(this.components);
            this.darkLabel18 = new DarkUI.Controls.DarkLabel();
            this.comboMist = new DarkUI.Controls.DarkComboBox(this.components);
            this.darkLabel17 = new DarkUI.Controls.DarkLabel();
            this.comboReflection = new DarkUI.Controls.DarkComboBox(this.components);
            this.darkLabel16 = new DarkUI.Controls.DarkLabel();
            this.cbFlagOutside = new DarkUI.Controls.DarkCheckBox();
            this.cbFlagCold = new DarkUI.Controls.DarkCheckBox();
            this.cbFlagDamage = new DarkUI.Controls.DarkCheckBox();
            this.comboRoomType = new DarkUI.Controls.DarkComboBox(this.components);
            this.darkLabel15 = new DarkUI.Controls.DarkLabel();
            this.comboRoom = new DarkUI.Controls.DarkComboBox(this.components);
            this.panelRoomAmbientLight = new System.Windows.Forms.Panel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.butRoomUp = new DarkUI.Controls.DarkButton();
            this.butRoomDown = new DarkUI.Controls.DarkButton();
            this.butEditRoomName = new DarkUI.Controls.DarkButton();
            this.butDeleteRoom = new DarkUI.Controls.DarkButton();
            this.butCropRoom = new DarkUI.Controls.DarkButton();
            this.butSplitRoom = new DarkUI.Controls.DarkButton();
            this.butCopyRoom = new DarkUI.Controls.DarkButton();
            this.SuspendLayout();
            // 
            // cbNoPathfinding
            // 
            this.cbNoPathfinding.AutoSize = true;
            this.cbNoPathfinding.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbNoPathfinding.Location = new System.Drawing.Point(108, 146);
            this.cbNoPathfinding.Name = "cbNoPathfinding";
            this.cbNoPathfinding.Size = new System.Drawing.Size(106, 17);
            this.cbNoPathfinding.TabIndex = 106;
            this.cbNoPathfinding.Text = "No pathfinding";
            // 
            // cbHorizon
            // 
            this.cbHorizon.AutoSize = true;
            this.cbHorizon.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbHorizon.Location = new System.Drawing.Point(108, 123);
            this.cbHorizon.Name = "cbHorizon";
            this.cbHorizon.Size = new System.Drawing.Size(118, 17);
            this.cbHorizon.TabIndex = 105;
            this.cbHorizon.Text = "Draw sky & horizon";
            // 
            // comboFlipMap
            // 
            this.comboFlipMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboFlipMap.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboFlipMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFlipMap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboFlipMap.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboFlipMap.ForeColor = System.Drawing.Color.White;
            this.comboFlipMap.FormattingEnabled = true;
            this.comboFlipMap.ItemHeight = 18;
            this.comboFlipMap.Items.AddRange(new object[] {
            "Not flipped",
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7"});
            this.comboFlipMap.Location = new System.Drawing.Point(3, 71);
            this.comboFlipMap.Name = "comboFlipMap";
            this.comboFlipMap.Size = new System.Drawing.Size(99, 24);
            this.comboFlipMap.TabIndex = 104;
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
            this.comboReverberation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboReverberation.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboReverberation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboReverberation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboReverberation.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboReverberation.ForeColor = System.Drawing.Color.White;
            this.comboReverberation.FormattingEnabled = true;
            this.comboReverberation.ItemHeight = 18;
            this.comboReverberation.Items.AddRange(new object[] {
            "None",
            "Small",
            "Medium",
            "Large",
            "Pipe"});
            this.comboReverberation.Location = new System.Drawing.Point(217, 184);
            this.comboReverberation.Name = "comboReverberation";
            this.comboReverberation.Size = new System.Drawing.Size(66, 24);
            this.comboReverberation.TabIndex = 101;
            // 
            // darkLabel18
            // 
            this.darkLabel18.AutoSize = true;
            this.darkLabel18.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel18.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel18.Location = new System.Drawing.Point(214, 168);
            this.darkLabel18.Name = "darkLabel18";
            this.darkLabel18.Size = new System.Drawing.Size(42, 13);
            this.darkLabel18.TabIndex = 100;
            this.darkLabel18.Text = "Reverb";
            // 
            // comboMist
            // 
            this.comboMist.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboMist.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboMist.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMist.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboMist.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboMist.ForeColor = System.Drawing.Color.White;
            this.comboMist.FormattingEnabled = true;
            this.comboMist.ItemHeight = 18;
            this.comboMist.Items.AddRange(new object[] {
            "No",
            "1",
            "2",
            "3",
            "4"});
            this.comboMist.Location = new System.Drawing.Point(141, 185);
            this.comboMist.Name = "comboMist";
            this.comboMist.Size = new System.Drawing.Size(52, 24);
            this.comboMist.TabIndex = 99;
            // 
            // darkLabel17
            // 
            this.darkLabel17.AutoSize = true;
            this.darkLabel17.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel17.Location = new System.Drawing.Point(138, 169);
            this.darkLabel17.Name = "darkLabel17";
            this.darkLabel17.Size = new System.Drawing.Size(29, 13);
            this.darkLabel17.TabIndex = 98;
            this.darkLabel17.Text = "Mist";
            // 
            // comboReflection
            // 
            this.comboReflection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboReflection.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboReflection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboReflection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboReflection.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboReflection.ForeColor = System.Drawing.Color.White;
            this.comboReflection.FormattingEnabled = true;
            this.comboReflection.ItemHeight = 18;
            this.comboReflection.Items.AddRange(new object[] {
            "No",
            "1",
            "2",
            "3",
            "4"});
            this.comboReflection.Location = new System.Drawing.Point(83, 185);
            this.comboReflection.Name = "comboReflection";
            this.comboReflection.Size = new System.Drawing.Size(52, 24);
            this.comboReflection.TabIndex = 97;
            // 
            // darkLabel16
            // 
            this.darkLabel16.AutoSize = true;
            this.darkLabel16.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel16.Location = new System.Drawing.Point(80, 169);
            this.darkLabel16.Name = "darkLabel16";
            this.darkLabel16.Size = new System.Drawing.Size(59, 13);
            this.darkLabel16.TabIndex = 96;
            this.darkLabel16.Text = "Reflection";
            // 
            // cbFlagOutside
            // 
            this.cbFlagOutside.AutoSize = true;
            this.cbFlagOutside.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbFlagOutside.Location = new System.Drawing.Point(108, 100);
            this.cbFlagOutside.Name = "cbFlagOutside";
            this.cbFlagOutside.Size = new System.Drawing.Size(67, 17);
            this.cbFlagOutside.TabIndex = 95;
            this.cbFlagOutside.Text = "Outside";
            // 
            // cbFlagCold
            // 
            this.cbFlagCold.AutoSize = true;
            this.cbFlagCold.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbFlagCold.Location = new System.Drawing.Point(108, 77);
            this.cbFlagCold.Name = "cbFlagCold";
            this.cbFlagCold.Size = new System.Drawing.Size(50, 17);
            this.cbFlagCold.TabIndex = 94;
            this.cbFlagCold.Text = "Cold";
            // 
            // cbFlagDamage
            // 
            this.cbFlagDamage.AutoSize = true;
            this.cbFlagDamage.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbFlagDamage.Location = new System.Drawing.Point(108, 55);
            this.cbFlagDamage.Name = "cbFlagDamage";
            this.cbFlagDamage.Size = new System.Drawing.Size(68, 17);
            this.cbFlagDamage.TabIndex = 93;
            this.cbFlagDamage.Text = "Damage";
            // 
            // comboRoomType
            // 
            this.comboRoomType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboRoomType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboRoomType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRoomType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboRoomType.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboRoomType.ForeColor = System.Drawing.Color.White;
            this.comboRoomType.FormattingEnabled = true;
            this.comboRoomType.ItemHeight = 18;
            this.comboRoomType.Items.AddRange(new object[] {
            "Normal",
            "Water 1",
            "Water 2",
            "Water 3",
            "Water 4",
            "Rain",
            "Snow",
            "Quicksand"});
            this.comboRoomType.Location = new System.Drawing.Point(3, 115);
            this.comboRoomType.Name = "comboRoomType";
            this.comboRoomType.Size = new System.Drawing.Size(99, 24);
            this.comboRoomType.TabIndex = 92;
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
            this.comboRoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboRoom.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboRoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboRoom.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboRoom.ForeColor = System.Drawing.Color.White;
            this.comboRoom.FormattingEnabled = true;
            this.comboRoom.ItemHeight = 18;
            this.comboRoom.Location = new System.Drawing.Point(3, 28);
            this.comboRoom.Name = "comboRoom";
            this.comboRoom.Size = new System.Drawing.Size(220, 24);
            this.comboRoom.TabIndex = 90;
            this.comboRoom.SelectedIndexChanged += new System.EventHandler(this.comboRoom_SelectedIndexChanged);
            // 
            // panelRoomAmbientLight
            // 
            this.panelRoomAmbientLight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRoomAmbientLight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelRoomAmbientLight.Location = new System.Drawing.Point(3, 185);
            this.panelRoomAmbientLight.Name = "panelRoomAmbientLight";
            this.panelRoomAmbientLight.Size = new System.Drawing.Size(67, 24);
            this.panelRoomAmbientLight.TabIndex = 89;
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(0, 169);
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
            this.butRoomUp.Image = global::TombEditor.Properties.Resources.RoomRaise_16;
            this.butRoomUp.Location = new System.Drawing.Point(259, 88);
            this.butRoomUp.Name = "butRoomUp";
            this.butRoomUp.Padding = new System.Windows.Forms.Padding(5);
            this.butRoomUp.Size = new System.Drawing.Size(24, 24);
            this.butRoomUp.TabIndex = 102;
            this.butRoomUp.Click += new System.EventHandler(this.butRoomUp_Click);
            // 
            // butRoomDown
            // 
            this.butRoomDown.Image = global::TombEditor.Properties.Resources.RoomLower_16;
            this.butRoomDown.Location = new System.Drawing.Point(259, 118);
            this.butRoomDown.Name = "butRoomDown";
            this.butRoomDown.Padding = new System.Windows.Forms.Padding(5);
            this.butRoomDown.Size = new System.Drawing.Size(24, 24);
            this.butRoomDown.TabIndex = 87;
            this.butRoomDown.Click += new System.EventHandler(this.butRoomDown_Click);
            // 
            // butEditRoomName
            // 
            this.butEditRoomName.Image = global::TombEditor.Properties.Resources.edit_16;
            this.butEditRoomName.Location = new System.Drawing.Point(259, 27);
            this.butEditRoomName.Name = "butEditRoomName";
            this.butEditRoomName.Padding = new System.Windows.Forms.Padding(5);
            this.butEditRoomName.Size = new System.Drawing.Size(24, 24);
            this.butEditRoomName.TabIndex = 86;
            this.butEditRoomName.Click += new System.EventHandler(this.butEditRoomName_Click);
            // 
            // butDeleteRoom
            // 
            this.butDeleteRoom.Image = global::TombEditor.Properties.Resources.trash_16;
            this.butDeleteRoom.Location = new System.Drawing.Point(259, 58);
            this.butDeleteRoom.Name = "butDeleteRoom";
            this.butDeleteRoom.Padding = new System.Windows.Forms.Padding(5);
            this.butDeleteRoom.Size = new System.Drawing.Size(24, 24);
            this.butDeleteRoom.TabIndex = 85;
            this.butDeleteRoom.Click += new System.EventHandler(this.butDeleteRoom_Click);
            // 
            // butCropRoom
            // 
            this.butCropRoom.Image = global::TombEditor.Properties.Resources.crop_16;
            this.butCropRoom.Location = new System.Drawing.Point(229, 27);
            this.butCropRoom.Name = "butCropRoom";
            this.butCropRoom.Padding = new System.Windows.Forms.Padding(5);
            this.butCropRoom.Size = new System.Drawing.Size(24, 24);
            this.butCropRoom.TabIndex = 84;
            this.butCropRoom.Click += new System.EventHandler(this.butCropRoom_Click);
            // 
            // butSplitRoom
            // 
            this.butSplitRoom.Image = global::TombEditor.Properties.Resources.split_files_16;
            this.butSplitRoom.Location = new System.Drawing.Point(229, 88);
            this.butSplitRoom.Name = "butSplitRoom";
            this.butSplitRoom.Padding = new System.Windows.Forms.Padding(5);
            this.butSplitRoom.Size = new System.Drawing.Size(24, 24);
            this.butSplitRoom.TabIndex = 83;
            this.butSplitRoom.Click += new System.EventHandler(this.butSplitRoom_Click);
            // 
            // butCopyRoom
            // 
            this.butCopyRoom.Image = global::TombEditor.Properties.Resources.copy_16;
            this.butCopyRoom.Location = new System.Drawing.Point(229, 58);
            this.butCopyRoom.Name = "butCopyRoom";
            this.butCopyRoom.Padding = new System.Windows.Forms.Padding(5);
            this.butCopyRoom.Size = new System.Drawing.Size(24, 24);
            this.butCopyRoom.TabIndex = 82;
            this.butCopyRoom.Click += new System.EventHandler(this.butCopyRoom_Click);
            // 
            // RoomOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
            this.Controls.Add(this.butDeleteRoom);
            this.Controls.Add(this.butCropRoom);
            this.Controls.Add(this.butSplitRoom);
            this.Controls.Add(this.butCopyRoom);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Left;
            this.DockText = "Room Options";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MaximumSize = new System.Drawing.Size(0, 211);
            this.Name = "RoomOptions";
            this.SerializationKey = "RoomOptions";
            this.Size = new System.Drawing.Size(0, 211);
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
        private DarkUI.Controls.DarkButton butDeleteRoom;
        private DarkUI.Controls.DarkButton butCropRoom;
        private DarkUI.Controls.DarkButton butSplitRoom;
        private DarkUI.Controls.DarkButton butCopyRoom;
        private System.Windows.Forms.ColorDialog colorDialog;
    }
}
