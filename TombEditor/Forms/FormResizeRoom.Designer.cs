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
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.cbAllowOversizedRooms = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.numericZn = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.numericZp = new DarkUI.Controls.DarkNumericUpDown();
            this.numericXp = new DarkUI.Controls.DarkNumericUpDown();
            this.numericXn = new DarkUI.Controls.DarkNumericUpDown();
            this.gridControl = new TombEditor.Forms.FormResizeRoom.GridControl();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.optUseFloor = new DarkUI.Controls.DarkRadioButton();
            this.optUseWalls = new DarkUI.Controls.DarkRadioButton();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericZn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericZp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericXp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericXn)).BeginInit();
            this.darkGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(610, 401);
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
            this.butOK.Location = new System.Drawing.Point(524, 401);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 6;
            this.butOK.Text = "OK";
            this.butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // darkLabel4
            // 
            this.darkLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.darkLabel4.ForeColor = System.Drawing.Color.Aqua;
            this.darkLabel4.Location = new System.Drawing.Point(106, 107);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(78, 78);
            this.darkLabel4.TabIndex = 8;
            // 
            // cbAllowOversizedRooms
            // 
            this.cbAllowOversizedRooms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAllowOversizedRooms.Location = new System.Drawing.Point(403, 351);
            this.cbAllowOversizedRooms.Name = "cbAllowOversizedRooms";
            this.cbAllowOversizedRooms.Size = new System.Drawing.Size(284, 49);
            this.cbAllowOversizedRooms.TabIndex = 5;
            this.cbAllowOversizedRooms.Text = "Allow rooms bigger than 32 by 32. ATTENTION: This will probably cause issues with" +
    " rendering in game. Only use this, if you know exactly what you are doing.";
            this.cbAllowOversizedRooms.CheckedChanged += new System.EventHandler(this.cbAllowOversizedRooms_CheckedChanged);
            // 
            // darkLabel2
            // 
            this.darkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(95, 185);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(96, 41);
            this.darkLabel2.TabIndex = 6;
            this.darkLabel2.Text = "Extend in direction Z- (South)";
            this.darkLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // darkLabel1
            // 
            this.darkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(103, 16);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(96, 41);
            this.darkLabel1.TabIndex = 0;
            this.darkLabel1.Text = "Extend in direction Z+ (North)";
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // darkLabel5
            // 
            this.darkLabel5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(187, 93);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(96, 41);
            this.darkLabel5.TabIndex = 4;
            this.darkLabel5.Text = "Extend in direction X+ (East)";
            this.darkLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericZn
            // 
            this.numericZn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericZn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericZn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericZn.IncrementAlternate = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericZn.Location = new System.Drawing.Point(98, 229);
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
            this.numericZn.Size = new System.Drawing.Size(93, 20);
            this.numericZn.TabIndex = 7;
            this.numericZn.ValueChanged += new System.EventHandler(this.numericZn_ValueChanged);
            // 
            // darkLabel3
            // 
            this.darkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(3, 93);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(96, 41);
            this.darkLabel3.TabIndex = 2;
            this.darkLabel3.Text = "Extend in direction X- (West)";
            this.darkLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericZp
            // 
            this.numericZp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericZp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericZp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericZp.IncrementAlternate = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericZp.Location = new System.Drawing.Point(99, 60);
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
            this.numericZp.Size = new System.Drawing.Size(93, 20);
            this.numericZp.TabIndex = 1;
            this.numericZp.ValueChanged += new System.EventHandler(this.numericZp_ValueChanged);
            // 
            // numericXp
            // 
            this.numericXp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericXp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericXp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericXp.IncrementAlternate = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericXp.Location = new System.Drawing.Point(190, 137);
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
            this.numericXp.Size = new System.Drawing.Size(93, 20);
            this.numericXp.TabIndex = 5;
            this.numericXp.ValueChanged += new System.EventHandler(this.numericXp_ValueChanged);
            // 
            // numericXn
            // 
            this.numericXn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericXn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numericXn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.numericXn.IncrementAlternate = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericXn.Location = new System.Drawing.Point(6, 137);
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
            this.numericXn.Size = new System.Drawing.Size(93, 20);
            this.numericXn.TabIndex = 3;
            this.numericXn.ValueChanged += new System.EventHandler(this.numericXn_ValueChanged);
            // 
            // gridControl
            // 
            this.gridControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gridControl.Location = new System.Drawing.Point(12, 26);
            this.gridControl.Name = "gridControl";
            this.gridControl.Room = null;
            this.gridControl.Size = new System.Drawing.Size(378, 396);
            this.gridControl.TabIndex = 1;
            // 
            // darkLabel6
            // 
            this.darkLabel6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(15, -1);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(375, 24);
            this.darkLabel6.TabIndex = 0;
            this.darkLabel6.Text = "Preview";
            this.darkLabel6.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // optUseFloor
            // 
            this.optUseFloor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.optUseFloor.Location = new System.Drawing.Point(406, 324);
            this.optUseFloor.Name = "optUseFloor";
            this.optUseFloor.Size = new System.Drawing.Size(204, 19);
            this.optUseFloor.TabIndex = 4;
            this.optUseFloor.Text = "Fill new sectors with floor";
            this.optUseFloor.CheckedChanged += new System.EventHandler(this.optUseFloor_CheckedChanged);
            // 
            // optUseWalls
            // 
            this.optUseWalls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.optUseWalls.Checked = true;
            this.optUseWalls.Location = new System.Drawing.Point(406, 301);
            this.optUseWalls.Name = "optUseWalls";
            this.optUseWalls.Size = new System.Drawing.Size(204, 19);
            this.optUseWalls.TabIndex = 3;
            this.optUseWalls.TabStop = true;
            this.optUseWalls.Text = "Fill new sectors with walls";
            this.optUseWalls.CheckedChanged += new System.EventHandler(this.optUseWalls_CheckedChanged);
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkGroupBox1.Controls.Add(this.darkLabel1);
            this.darkGroupBox1.Controls.Add(this.numericXn);
            this.darkGroupBox1.Controls.Add(this.darkLabel2);
            this.darkGroupBox1.Controls.Add(this.numericXp);
            this.darkGroupBox1.Controls.Add(this.darkLabel5);
            this.darkGroupBox1.Controls.Add(this.numericZp);
            this.darkGroupBox1.Controls.Add(this.numericZn);
            this.darkGroupBox1.Controls.Add(this.darkLabel3);
            this.darkGroupBox1.Controls.Add(this.darkLabel4);
            this.darkGroupBox1.Location = new System.Drawing.Point(400, 19);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(290, 272);
            this.darkGroupBox1.TabIndex = 2;
            this.darkGroupBox1.TabStop = false;
            this.darkGroupBox1.Text = "New dimensions";
            // 
            // FormResizeRoom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(699, 434);
            this.Controls.Add(this.darkGroupBox1);
            this.Controls.Add(this.optUseFloor);
            this.Controls.Add(this.gridControl);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.cbAllowOversizedRooms);
            this.Controls.Add(this.darkLabel6);
            this.Controls.Add(this.optUseWalls);
            this.MinimizeBox = false;
            this.Name = "FormResizeRoom";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Resize Room";
            ((System.ComponentModel.ISupportInitialize)(this.numericZn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericZp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericXp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericXn)).EndInit();
            this.darkGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkNumericUpDown numericXn;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkNumericUpDown numericXp;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkNumericUpDown numericZp;
        private DarkUI.Controls.DarkNumericUpDown numericZn;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkCheckBox cbAllowOversizedRooms;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOK;
        private GridControl gridControl;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkRadioButton optUseFloor;
        private DarkUI.Controls.DarkRadioButton optUseWalls;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
    }
}