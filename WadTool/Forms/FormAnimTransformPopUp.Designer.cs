namespace WadTool
{
    partial class FormAnimTransformPopUp
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.panelRotation = new DarkUI.Controls.DarkSectionPanel();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.nudRotX = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.nudRotY = new DarkUI.Controls.DarkNumericUpDown();
            this.nudRotZ = new DarkUI.Controls.DarkNumericUpDown();
            this.panelTranslation = new DarkUI.Controls.DarkSectionPanel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.nudTransX = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.nudTransY = new DarkUI.Controls.DarkNumericUpDown();
            this.nudTransZ = new DarkUI.Controls.DarkNumericUpDown();
            this.panel1.SuspendLayout();
            this.panelRotation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRotX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRotY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRotZ)).BeginInit();
            this.panelTranslation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransZ)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(44)))));
            this.panel1.Controls.Add(this.butCancel);
            this.panel1.Controls.Add(this.panelRotation);
            this.panel1.Controls.Add(this.panelTranslation);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(331, 118);
            this.panel1.TabIndex = 0;
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.butCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
            this.butCancel.BackColorUseGeneric = false;
            this.butCancel.Image = global::WadTool.Properties.Resources.actions_delete_16;
            this.butCancel.Location = new System.Drawing.Point(291, 3);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(37, 112);
            this.butCancel.TabIndex = 10;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // panelRotation
            // 
            this.panelRotation.Controls.Add(this.darkLabel4);
            this.panelRotation.Controls.Add(this.darkLabel5);
            this.panelRotation.Controls.Add(this.nudRotX);
            this.panelRotation.Controls.Add(this.darkLabel6);
            this.panelRotation.Controls.Add(this.nudRotY);
            this.panelRotation.Controls.Add(this.nudRotZ);
            this.panelRotation.Location = new System.Drawing.Point(3, 3);
            this.panelRotation.Name = "panelRotation";
            this.panelRotation.SectionHeader = "Rotation";
            this.panelRotation.Size = new System.Drawing.Size(286, 55);
            this.panelRotation.TabIndex = 9;
            this.panelRotation.MouseDown += new System.Windows.Forms.MouseEventHandler(this.darkSectionPanel1_MouseDown);
            // 
            // darkLabel4
            // 
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel4.ForeColor = System.Drawing.Color.LightGray;
            this.darkLabel4.Location = new System.Drawing.Point(188, 30);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(14, 13);
            this.darkLabel4.TabIndex = 8;
            this.darkLabel4.Text = "R";
            // 
            // darkLabel5
            // 
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel5.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel5.ForeColor = System.Drawing.Color.LightGray;
            this.darkLabel5.Location = new System.Drawing.Point(97, 30);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(14, 13);
            this.darkLabel5.TabIndex = 7;
            this.darkLabel5.Text = "P";
            // 
            // nudRotX
            // 
            this.nudRotX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudRotX.DecimalPlaces = 5;
            this.nudRotX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudRotX.IncrementAlternate = new decimal(new int[] {
            50,
            0,
            0,
            65536});
            this.nudRotX.Location = new System.Drawing.Point(20, 28);
            this.nudRotX.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.nudRotX.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.nudRotX.MousewheelSingleIncrement = true;
            this.nudRotX.Name = "nudRotX";
            this.nudRotX.Size = new System.Drawing.Size(73, 22);
            this.nudRotX.TabIndex = 0;
            this.nudRotX.ValueChanged += new System.EventHandler(this.nudRotX_ValueChanged);
            // 
            // darkLabel6
            // 
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel6.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel6.ForeColor = System.Drawing.Color.LightGray;
            this.darkLabel6.Location = new System.Drawing.Point(4, 30);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(14, 13);
            this.darkLabel6.TabIndex = 6;
            this.darkLabel6.Text = "Y";
            // 
            // nudRotY
            // 
            this.nudRotY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudRotY.DecimalPlaces = 5;
            this.nudRotY.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudRotY.IncrementAlternate = new decimal(new int[] {
            50,
            0,
            0,
            65536});
            this.nudRotY.Location = new System.Drawing.Point(112, 28);
            this.nudRotY.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.nudRotY.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.nudRotY.MousewheelSingleIncrement = true;
            this.nudRotY.Name = "nudRotY";
            this.nudRotY.Size = new System.Drawing.Size(73, 22);
            this.nudRotY.TabIndex = 1;
            this.nudRotY.ValueChanged += new System.EventHandler(this.nudRotY_ValueChanged);
            // 
            // nudRotZ
            // 
            this.nudRotZ.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudRotZ.DecimalPlaces = 5;
            this.nudRotZ.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudRotZ.IncrementAlternate = new decimal(new int[] {
            50,
            0,
            0,
            65536});
            this.nudRotZ.Location = new System.Drawing.Point(204, 28);
            this.nudRotZ.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.nudRotZ.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.nudRotZ.MousewheelSingleIncrement = true;
            this.nudRotZ.Name = "nudRotZ";
            this.nudRotZ.Size = new System.Drawing.Size(73, 22);
            this.nudRotZ.TabIndex = 2;
            this.nudRotZ.ValueChanged += new System.EventHandler(this.nudRotZ_ValueChanged);
            // 
            // panelTranslation
            // 
            this.panelTranslation.Controls.Add(this.darkLabel3);
            this.panelTranslation.Controls.Add(this.darkLabel2);
            this.panelTranslation.Controls.Add(this.nudTransX);
            this.panelTranslation.Controls.Add(this.darkLabel1);
            this.panelTranslation.Controls.Add(this.nudTransY);
            this.panelTranslation.Controls.Add(this.nudTransZ);
            this.panelTranslation.Location = new System.Drawing.Point(3, 60);
            this.panelTranslation.Name = "panelTranslation";
            this.panelTranslation.SectionHeader = "Translation";
            this.panelTranslation.Size = new System.Drawing.Size(286, 55);
            this.panelTranslation.TabIndex = 7;
            this.panelTranslation.MouseDown += new System.Windows.Forms.MouseEventHandler(this.darkSectionPanel2_MouseDown);
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel3.ForeColor = System.Drawing.Color.LightGray;
            this.darkLabel3.Location = new System.Drawing.Point(188, 30);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(14, 13);
            this.darkLabel3.TabIndex = 8;
            this.darkLabel3.Text = "Z";
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel2.ForeColor = System.Drawing.Color.LightGray;
            this.darkLabel2.Location = new System.Drawing.Point(97, 30);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(14, 13);
            this.darkLabel2.TabIndex = 7;
            this.darkLabel2.Text = "Y";
            // 
            // nudTransX
            // 
            this.nudTransX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudTransX.DecimalPlaces = 5;
            this.nudTransX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudTransX.IncrementAlternate = new decimal(new int[] {
            160,
            0,
            0,
            65536});
            this.nudTransX.Location = new System.Drawing.Point(20, 28);
            this.nudTransX.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudTransX.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.nudTransX.MousewheelSingleIncrement = true;
            this.nudTransX.Name = "nudTransX";
            this.nudTransX.Size = new System.Drawing.Size(73, 22);
            this.nudTransX.TabIndex = 0;
            this.nudTransX.ValueChanged += new System.EventHandler(this.nudTransX_ValueChanged);
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkLabel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.darkLabel1.ForeColor = System.Drawing.Color.LightGray;
            this.darkLabel1.Location = new System.Drawing.Point(4, 30);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(14, 13);
            this.darkLabel1.TabIndex = 6;
            this.darkLabel1.Text = "X";
            // 
            // nudTransY
            // 
            this.nudTransY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudTransY.DecimalPlaces = 5;
            this.nudTransY.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudTransY.IncrementAlternate = new decimal(new int[] {
            160,
            0,
            0,
            65536});
            this.nudTransY.Location = new System.Drawing.Point(112, 28);
            this.nudTransY.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudTransY.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.nudTransY.MousewheelSingleIncrement = true;
            this.nudTransY.Name = "nudTransY";
            this.nudTransY.Size = new System.Drawing.Size(73, 22);
            this.nudTransY.TabIndex = 1;
            this.nudTransY.ValueChanged += new System.EventHandler(this.nudTransY_ValueChanged);
            // 
            // nudTransZ
            // 
            this.nudTransZ.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.nudTransZ.DecimalPlaces = 5;
            this.nudTransZ.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nudTransZ.IncrementAlternate = new decimal(new int[] {
            160,
            0,
            0,
            65536});
            this.nudTransZ.Location = new System.Drawing.Point(204, 28);
            this.nudTransZ.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.nudTransZ.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.nudTransZ.MousewheelSingleIncrement = true;
            this.nudTransZ.Name = "nudTransZ";
            this.nudTransZ.Size = new System.Drawing.Size(73, 22);
            this.nudTransZ.TabIndex = 2;
            this.nudTransZ.ValueChanged += new System.EventHandler(this.nudTransZ_ValueChanged);
            // 
            // FormAnimTransformPopUp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 118);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(331, 61);
            this.Name = "FormAnimTransformPopUp";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FormAnimTransformPopUp";
            this.Deactivate += new System.EventHandler(this.FormPopUpSearch_Deactivate);
            this.panel1.ResumeLayout(false);
            this.panelRotation.ResumeLayout(false);
            this.panelRotation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRotX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRotY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRotZ)).EndInit();
            this.panelTranslation.ResumeLayout(false);
            this.panelTranslation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTransZ)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private DarkUI.Controls.DarkNumericUpDown nudTransX;
        private DarkUI.Controls.DarkNumericUpDown nudTransZ;
        private DarkUI.Controls.DarkNumericUpDown nudTransY;
        private DarkUI.Controls.DarkSectionPanel panelRotation;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkNumericUpDown nudRotX;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkNumericUpDown nudRotY;
        private DarkUI.Controls.DarkNumericUpDown nudRotZ;
        private DarkUI.Controls.DarkSectionPanel panelTranslation;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkButton butCancel;
    }
}