namespace TombEditor.Forms
{
    partial class FormResizeRoom
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.cbAllowOversizedRooms = new DarkUI.Controls.DarkCheckBox();
            this.numericZn = new DarkUI.Controls.DarkNumericUpDown();
            this.numericZp = new DarkUI.Controls.DarkNumericUpDown();
            this.numericXp = new DarkUI.Controls.DarkNumericUpDown();
            this.numericXn = new DarkUI.Controls.DarkNumericUpDown();
            this.gridControl = new TombEditor.Forms.FormResizeRoom.GridControl();
            this.optUseFloor = new DarkUI.Controls.DarkRadioButton();
            this.optUseWalls = new DarkUI.Controls.DarkRadioButton();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.roomIcon = new System.Windows.Forms.PictureBox();
            this.darkGroupBox2 = new DarkUI.Controls.DarkGroupBox();
            this.darkRadioButton1 = new DarkUI.Controls.DarkRadioButton();
            this.darkCheckBox1 = new DarkUI.Controls.DarkCheckBox();
            this.darkButton1 = new DarkUI.Controls.DarkButton();
            this.darkButton2 = new DarkUI.Controls.DarkButton();
            this.darkRadioButton2 = new DarkUI.Controls.DarkRadioButton();
            this.darkGroupBox3 = new DarkUI.Controls.DarkGroupBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.darkNumericUpDown1 = new DarkUI.Controls.DarkNumericUpDown();
            this.darkNumericUpDown2 = new DarkUI.Controls.DarkNumericUpDown();
            this.darkNumericUpDown3 = new DarkUI.Controls.DarkNumericUpDown();
            this.darkNumericUpDown4 = new DarkUI.Controls.DarkNumericUpDown();
            this.darkGroupBox4 = new DarkUI.Controls.DarkGroupBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.numericZn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericZp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericXp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericXn)).BeginInit();
            this.darkGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.roomIcon)).BeginInit();
            this.darkGroupBox2.SuspendLayout();
            this.darkGroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.darkNumericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.darkNumericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.darkNumericUpDown3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.darkNumericUpDown4)).BeginInit();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(435, 286);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 7;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Location = new System.Drawing.Point(349, 286);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 6;
            this.butOK.Text = "OK";
            this.butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // cbAllowOversizedRooms
            // 
            this.cbAllowOversizedRooms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAllowOversizedRooms.Location = new System.Drawing.Point(259, 253);
            this.cbAllowOversizedRooms.Name = "cbAllowOversizedRooms";
            this.cbAllowOversizedRooms.Size = new System.Drawing.Size(255, 17);
            this.cbAllowOversizedRooms.TabIndex = 5;
            this.cbAllowOversizedRooms.Text = "Allow rooms bigger than 32x32";
            this.cbAllowOversizedRooms.CheckedChanged += new System.EventHandler(this.cbAllowOversizedRooms_CheckedChanged);
            // 
            // numericZn
            // 
            this.numericZn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericZn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericZn.IncrementAlternate = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericZn.Location = new System.Drawing.Point(97, 159);
            this.numericZn.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.numericZn.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.numericZn.MousewheelSingleIncrement = true;
            this.numericZn.Name = "numericZn";
            this.numericZn.Size = new System.Drawing.Size(62, 22);
            this.numericZn.TabIndex = 7;
            this.numericZn.ValueChanged += new System.EventHandler(this.numericZn_ValueChanged);
            // 
            // numericZp
            // 
            this.numericZp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericZp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericZp.IncrementAlternate = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericZp.Location = new System.Drawing.Point(97, 25);
            this.numericZp.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.numericZp.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.numericZp.MousewheelSingleIncrement = true;
            this.numericZp.Name = "numericZp";
            this.numericZp.Size = new System.Drawing.Size(62, 22);
            this.numericZp.TabIndex = 1;
            this.numericZp.ValueChanged += new System.EventHandler(this.numericZp_ValueChanged);
            // 
            // numericXp
            // 
            this.numericXp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericXp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericXp.IncrementAlternate = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericXp.Location = new System.Drawing.Point(9, 92);
            this.numericXp.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.numericXp.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.numericXp.MousewheelSingleIncrement = true;
            this.numericXp.Name = "numericXp";
            this.numericXp.Size = new System.Drawing.Size(62, 22);
            this.numericXp.TabIndex = 5;
            this.numericXp.ValueChanged += new System.EventHandler(this.numericXp_ValueChanged);
            // 
            // numericXn
            // 
            this.numericXn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericXn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericXn.IncrementAlternate = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericXn.Location = new System.Drawing.Point(183, 92);
            this.numericXn.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.numericXn.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.numericXn.MousewheelSingleIncrement = true;
            this.numericXn.Name = "numericXn";
            this.numericXn.Size = new System.Drawing.Size(62, 22);
            this.numericXn.TabIndex = 3;
            this.numericXn.ValueChanged += new System.EventHandler(this.numericXn_ValueChanged);
            // 
            // gridControl
            // 
            this.gridControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gridControl.Location = new System.Drawing.Point(6, 21);
            this.gridControl.Name = "gridControl";
            this.gridControl.Padding = new System.Windows.Forms.Padding(2);
            this.gridControl.Room = null;
            this.gridControl.Size = new System.Drawing.Size(234, 234);
            this.gridControl.TabIndex = 1;
            // 
            // optUseFloor
            // 
            this.optUseFloor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.optUseFloor.Checked = true;
            this.optUseFloor.Location = new System.Drawing.Point(261, 228);
            this.optUseFloor.Name = "optUseFloor";
            this.optUseFloor.Size = new System.Drawing.Size(204, 19);
            this.optUseFloor.TabIndex = 4;
            this.optUseFloor.TabStop = true;
            this.optUseFloor.Text = "Fill new sectors with floor";
            this.optUseFloor.CheckedChanged += new System.EventHandler(this.optUseFloor_CheckedChanged);
            // 
            // optUseWalls
            // 
            this.optUseWalls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.optUseWalls.Location = new System.Drawing.Point(261, 205);
            this.optUseWalls.Name = "optUseWalls";
            this.optUseWalls.Size = new System.Drawing.Size(204, 19);
            this.optUseWalls.TabIndex = 3;
            this.optUseWalls.Text = "Fill new sectors with walls";
            this.optUseWalls.CheckedChanged += new System.EventHandler(this.optUseWalls_CheckedChanged);
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkGroupBox1.Controls.Add(this.roomIcon);
            this.darkGroupBox1.Controls.Add(this.numericXn);
            this.darkGroupBox1.Controls.Add(this.numericXp);
            this.darkGroupBox1.Controls.Add(this.numericZp);
            this.darkGroupBox1.Controls.Add(this.numericZn);
            this.darkGroupBox1.Location = new System.Drawing.Point(261, 8);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(255, 191);
            this.darkGroupBox1.TabIndex = 2;
            this.darkGroupBox1.TabStop = false;
            this.darkGroupBox1.Text = "New dimensions";
            // 
            // roomIcon
            // 
            this.roomIcon.InitialImage = null;
            this.roomIcon.Location = new System.Drawing.Point(77, 53);
            this.roomIcon.Name = "roomIcon";
            this.roomIcon.Size = new System.Drawing.Size(100, 100);
            this.roomIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.roomIcon.TabIndex = 8;
            this.roomIcon.TabStop = false;
            // 
            // darkGroupBox2
            // 
            this.darkGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkGroupBox2.Controls.Add(this.gridControl);
            this.darkGroupBox2.Location = new System.Drawing.Point(7, 8);
            this.darkGroupBox2.Name = "darkGroupBox2";
            this.darkGroupBox2.Size = new System.Drawing.Size(246, 261);
            this.darkGroupBox2.TabIndex = 8;
            this.darkGroupBox2.TabStop = false;
            this.darkGroupBox2.Text = "Preview";
            // 
            // darkRadioButton1
            // 
            this.darkRadioButton1.Location = new System.Drawing.Point(259, 203);
            this.darkRadioButton1.Name = "darkRadioButton1";
            this.darkRadioButton1.Size = new System.Drawing.Size(204, 19);
            this.darkRadioButton1.TabIndex = 3;
            this.darkRadioButton1.Text = "Fill new sectors with walls";
            this.darkRadioButton1.CheckedChanged += new System.EventHandler(this.optUseWalls_CheckedChanged);
            // 
            // darkCheckBox1
            // 
            this.darkCheckBox1.Location = new System.Drawing.Point(257, 251);
            this.darkCheckBox1.Name = "darkCheckBox1";
            this.darkCheckBox1.Size = new System.Drawing.Size(255, 17);
            this.darkCheckBox1.TabIndex = 5;
            this.darkCheckBox1.Text = "Allow rooms bigger than 32x32";
            this.darkCheckBox1.CheckedChanged += new System.EventHandler(this.cbAllowOversizedRooms_CheckedChanged);
            // 
            // darkButton1
            // 
            this.darkButton1.Location = new System.Drawing.Point(347, 283);
            this.darkButton1.Name = "darkButton1";
            this.darkButton1.Size = new System.Drawing.Size(80, 23);
            this.darkButton1.TabIndex = 6;
            this.darkButton1.Text = "OK";
            this.darkButton1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.darkButton1.Click += new System.EventHandler(this.butOK_Click);
            // 
            // darkButton2
            // 
            this.darkButton2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.darkButton2.Location = new System.Drawing.Point(433, 283);
            this.darkButton2.Name = "darkButton2";
            this.darkButton2.Size = new System.Drawing.Size(80, 23);
            this.darkButton2.TabIndex = 7;
            this.darkButton2.Text = "Cancel";
            this.darkButton2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.darkButton2.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // darkRadioButton2
            // 
            this.darkRadioButton2.Checked = true;
            this.darkRadioButton2.Location = new System.Drawing.Point(259, 226);
            this.darkRadioButton2.Name = "darkRadioButton2";
            this.darkRadioButton2.Size = new System.Drawing.Size(204, 19);
            this.darkRadioButton2.TabIndex = 4;
            this.darkRadioButton2.TabStop = true;
            this.darkRadioButton2.Text = "Fill new sectors with floor";
            this.darkRadioButton2.CheckedChanged += new System.EventHandler(this.optUseFloor_CheckedChanged);
            // 
            // darkGroupBox3
            // 
            this.darkGroupBox3.Controls.Add(this.pictureBox2);
            this.darkGroupBox3.Controls.Add(this.darkNumericUpDown1);
            this.darkGroupBox3.Controls.Add(this.darkNumericUpDown2);
            this.darkGroupBox3.Controls.Add(this.darkNumericUpDown3);
            this.darkGroupBox3.Controls.Add(this.darkNumericUpDown4);
            this.darkGroupBox3.Location = new System.Drawing.Point(259, 6);
            this.darkGroupBox3.Name = "darkGroupBox3";
            this.darkGroupBox3.Size = new System.Drawing.Size(255, 191);
            this.darkGroupBox3.TabIndex = 2;
            this.darkGroupBox3.TabStop = false;
            this.darkGroupBox3.Text = "New dimensions";
            // 
            // pictureBox2
            // 
            this.pictureBox2.InitialImage = null;
            this.pictureBox2.Location = new System.Drawing.Point(77, 53);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(100, 100);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox2.TabIndex = 8;
            this.pictureBox2.TabStop = false;
            // 
            // darkNumericUpDown1
            // 
            this.darkNumericUpDown1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.darkNumericUpDown1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkNumericUpDown1.IncrementAlternate = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.darkNumericUpDown1.Location = new System.Drawing.Point(183, 92);
            this.darkNumericUpDown1.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.darkNumericUpDown1.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.darkNumericUpDown1.MousewheelSingleIncrement = true;
            this.darkNumericUpDown1.Name = "darkNumericUpDown1";
            this.darkNumericUpDown1.Size = new System.Drawing.Size(62, 20);
            this.darkNumericUpDown1.TabIndex = 3;
            this.darkNumericUpDown1.ValueChanged += new System.EventHandler(this.numericXn_ValueChanged);
            // 
            // darkNumericUpDown2
            // 
            this.darkNumericUpDown2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.darkNumericUpDown2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkNumericUpDown2.IncrementAlternate = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.darkNumericUpDown2.Location = new System.Drawing.Point(9, 92);
            this.darkNumericUpDown2.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.darkNumericUpDown2.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.darkNumericUpDown2.MousewheelSingleIncrement = true;
            this.darkNumericUpDown2.Name = "darkNumericUpDown2";
            this.darkNumericUpDown2.Size = new System.Drawing.Size(62, 20);
            this.darkNumericUpDown2.TabIndex = 5;
            this.darkNumericUpDown2.ValueChanged += new System.EventHandler(this.numericXp_ValueChanged);
            // 
            // darkNumericUpDown3
            // 
            this.darkNumericUpDown3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.darkNumericUpDown3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkNumericUpDown3.IncrementAlternate = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.darkNumericUpDown3.Location = new System.Drawing.Point(97, 25);
            this.darkNumericUpDown3.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.darkNumericUpDown3.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.darkNumericUpDown3.MousewheelSingleIncrement = true;
            this.darkNumericUpDown3.Name = "darkNumericUpDown3";
            this.darkNumericUpDown3.Size = new System.Drawing.Size(62, 20);
            this.darkNumericUpDown3.TabIndex = 1;
            this.darkNumericUpDown3.ValueChanged += new System.EventHandler(this.numericZp_ValueChanged);
            // 
            // darkNumericUpDown4
            // 
            this.darkNumericUpDown4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.darkNumericUpDown4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkNumericUpDown4.IncrementAlternate = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.darkNumericUpDown4.Location = new System.Drawing.Point(97, 159);
            this.darkNumericUpDown4.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.darkNumericUpDown4.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.darkNumericUpDown4.MousewheelSingleIncrement = true;
            this.darkNumericUpDown4.Name = "darkNumericUpDown4";
            this.darkNumericUpDown4.Size = new System.Drawing.Size(62, 20);
            this.darkNumericUpDown4.TabIndex = 7;
            this.darkNumericUpDown4.ValueChanged += new System.EventHandler(this.numericZn_ValueChanged);
            // 
            // darkGroupBox4
            // 
            this.darkGroupBox4.Location = new System.Drawing.Point(0, 0);
            this.darkGroupBox4.Name = "darkGroupBox4";
            this.darkGroupBox4.Size = new System.Drawing.Size(200, 100);
            this.darkGroupBox4.TabIndex = 0;
            this.darkGroupBox4.TabStop = false;
            // 
            // FormResizeRoom
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(524, 317);
            this.Controls.Add(this.darkGroupBox2);
            this.Controls.Add(this.darkGroupBox1);
            this.Controls.Add(this.optUseFloor);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.cbAllowOversizedRooms);
            this.Controls.Add(this.optUseWalls);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(540, 355);
            this.Name = "FormResizeRoom";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Crop Room";
            ((System.ComponentModel.ISupportInitialize)(this.numericZn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericZp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericXp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericXn)).EndInit();
            this.darkGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.roomIcon)).EndInit();
            this.darkGroupBox2.ResumeLayout(false);
            this.darkGroupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.darkNumericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.darkNumericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.darkNumericUpDown3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.darkNumericUpDown4)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkNumericUpDown numericXn;
        private DarkUI.Controls.DarkNumericUpDown numericXp;
        private DarkUI.Controls.DarkNumericUpDown numericZp;
        private DarkUI.Controls.DarkNumericUpDown numericZn;
        private DarkUI.Controls.DarkCheckBox cbAllowOversizedRooms;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOK;
        private GridControl gridControl;
        private DarkUI.Controls.DarkRadioButton optUseFloor;
        private DarkUI.Controls.DarkRadioButton optUseWalls;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private System.Windows.Forms.PictureBox roomIcon;
        private DarkUI.Controls.DarkGroupBox darkGroupBox2;
        private DarkUI.Controls.DarkRadioButton darkRadioButton1;
        private DarkUI.Controls.DarkCheckBox darkCheckBox1;
        private DarkUI.Controls.DarkButton darkButton1;
        private DarkUI.Controls.DarkButton darkButton2;
        private DarkUI.Controls.DarkRadioButton darkRadioButton2;
        private DarkUI.Controls.DarkGroupBox darkGroupBox3;
        private System.Windows.Forms.PictureBox pictureBox2;
        private DarkUI.Controls.DarkNumericUpDown darkNumericUpDown1;
        private DarkUI.Controls.DarkNumericUpDown darkNumericUpDown2;
        private DarkUI.Controls.DarkNumericUpDown darkNumericUpDown3;
        private DarkUI.Controls.DarkNumericUpDown darkNumericUpDown4;
        private DarkUI.Controls.DarkGroupBox darkGroupBox4;
        private System.Windows.Forms.ToolTip toolTip;
    }
}