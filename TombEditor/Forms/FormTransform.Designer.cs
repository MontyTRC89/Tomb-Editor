namespace TombEditor.Forms
{
    partial class FormTransform
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
            darkLabel29 = new DarkUI.Controls.DarkLabel();
            darkLabel28 = new DarkUI.Controls.DarkLabel();
            darkLabel21 = new DarkUI.Controls.DarkLabel();
            darkLabel26 = new DarkUI.Controls.DarkLabel();
            nudTransX = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel27 = new DarkUI.Controls.DarkLabel();
            nudTransY = new DarkUI.Controls.DarkNumericUpDown();
            nudTransZ = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel1 = new DarkUI.Controls.DarkLabel();
            darkLabel18 = new DarkUI.Controls.DarkLabel();
            nudRotX = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel19 = new DarkUI.Controls.DarkLabel();
            nudRotY = new DarkUI.Controls.DarkNumericUpDown();
            nudRotZ = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel2 = new DarkUI.Controls.DarkLabel();
            darkLabel3 = new DarkUI.Controls.DarkLabel();
            darkLabel4 = new DarkUI.Controls.DarkLabel();
            nudScaleX = new DarkUI.Controls.DarkNumericUpDown();
            darkLabel5 = new DarkUI.Controls.DarkLabel();
            nudScaleY = new DarkUI.Controls.DarkNumericUpDown();
            nudScaleZ = new DarkUI.Controls.DarkNumericUpDown();
            butCancel = new DarkUI.Controls.DarkButton();
            butOk = new DarkUI.Controls.DarkButton();
            ((System.ComponentModel.ISupportInitialize)nudTransX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTransY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTransZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudRotX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudRotY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudRotZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudScaleX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudScaleY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudScaleZ).BeginInit();
            SuspendLayout();
            // 
            // darkLabel29
            // 
            darkLabel29.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel29.Location = new System.Drawing.Point(10, 12);
            darkLabel29.Name = "darkLabel29";
            darkLabel29.Size = new System.Drawing.Size(57, 13);
            darkLabel29.TabIndex = 112;
            darkLabel29.Text = "Position:";
            // 
            // darkLabel28
            // 
            darkLabel28.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel28.Location = new System.Drawing.Point(10, 40);
            darkLabel28.Name = "darkLabel28";
            darkLabel28.Size = new System.Drawing.Size(55, 13);
            darkLabel28.TabIndex = 111;
            darkLabel28.Text = "Rotation:";
            // 
            // darkLabel21
            // 
            darkLabel21.AutoSize = true;
            darkLabel21.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel21.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel21.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel21.Location = new System.Drawing.Point(238, 12);
            darkLabel21.Name = "darkLabel21";
            darkLabel21.Size = new System.Drawing.Size(14, 13);
            darkLabel21.TabIndex = 106;
            darkLabel21.Text = "Z";
            // 
            // darkLabel26
            // 
            darkLabel26.AutoSize = true;
            darkLabel26.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel26.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel26.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel26.Location = new System.Drawing.Point(155, 12);
            darkLabel26.Name = "darkLabel26";
            darkLabel26.Size = new System.Drawing.Size(14, 13);
            darkLabel26.TabIndex = 104;
            darkLabel26.Text = "Y";
            // 
            // nudTransX
            // 
            nudTransX.IncrementAlternate = new decimal(new int[] { 160, 0, 0, 65536 });
            nudTransX.Location = new System.Drawing.Point(87, 10);
            nudTransX.LoopValues = false;
            nudTransX.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            nudTransX.Minimum = new decimal(new int[] { int.MaxValue, 0, 0, int.MinValue });
            nudTransX.Name = "nudTransX";
            nudTransX.Size = new System.Drawing.Size(64, 22);
            nudTransX.TabIndex = 108;
            nudTransX.ValueChanged += ValidateInstance;
            nudTransX.Validated += ValidateInstance;
            // 
            // darkLabel27
            // 
            darkLabel27.AutoSize = true;
            darkLabel27.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel27.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel27.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel27.Location = new System.Drawing.Point(71, 12);
            darkLabel27.Name = "darkLabel27";
            darkLabel27.Size = new System.Drawing.Size(14, 13);
            darkLabel27.TabIndex = 102;
            darkLabel27.Text = "X";
            // 
            // nudTransY
            // 
            nudTransY.IncrementAlternate = new decimal(new int[] { 160, 0, 0, 65536 });
            nudTransY.Location = new System.Drawing.Point(170, 10);
            nudTransY.LoopValues = false;
            nudTransY.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            nudTransY.Minimum = new decimal(new int[] { int.MaxValue, 0, 0, int.MinValue });
            nudTransY.Name = "nudTransY";
            nudTransY.Size = new System.Drawing.Size(64, 22);
            nudTransY.TabIndex = 109;
            nudTransY.ValueChanged += ValidateInstance;
            nudTransY.Validated += ValidateInstance;
            // 
            // nudTransZ
            // 
            nudTransZ.IncrementAlternate = new decimal(new int[] { 160, 0, 0, 65536 });
            nudTransZ.Location = new System.Drawing.Point(253, 10);
            nudTransZ.LoopValues = false;
            nudTransZ.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            nudTransZ.Minimum = new decimal(new int[] { int.MaxValue, 0, 0, int.MinValue });
            nudTransZ.Name = "nudTransZ";
            nudTransZ.Size = new System.Drawing.Size(65, 22);
            nudTransZ.TabIndex = 110;
            nudTransZ.ValueChanged += ValidateInstance;
            nudTransZ.Validated += ValidateInstance;
            // 
            // darkLabel1
            // 
            darkLabel1.AutoSize = true;
            darkLabel1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel1.Location = new System.Drawing.Point(238, 40);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(14, 13);
            darkLabel1.TabIndex = 101;
            darkLabel1.Text = "R";
            // 
            // darkLabel18
            // 
            darkLabel18.AutoSize = true;
            darkLabel18.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel18.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel18.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel18.Location = new System.Drawing.Point(155, 40);
            darkLabel18.Name = "darkLabel18";
            darkLabel18.Size = new System.Drawing.Size(14, 13);
            darkLabel18.TabIndex = 100;
            darkLabel18.Text = "P";
            // 
            // nudRotX
            // 
            nudRotX.DecimalPlaces = 2;
            nudRotX.Enabled = false;
            nudRotX.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudRotX.Location = new System.Drawing.Point(87, 38);
            nudRotX.LoopValues = true;
            nudRotX.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            nudRotX.Minimum = new decimal(new int[] { 360, 0, 0, int.MinValue });
            nudRotX.Name = "nudRotX";
            nudRotX.Size = new System.Drawing.Size(64, 22);
            nudRotX.TabIndex = 103;
            nudRotX.ValueChanged += ValidateInstance;
            nudRotX.Validated += ValidateInstance;
            // 
            // darkLabel19
            // 
            darkLabel19.AutoSize = true;
            darkLabel19.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel19.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel19.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel19.Location = new System.Drawing.Point(71, 40);
            darkLabel19.Name = "darkLabel19";
            darkLabel19.Size = new System.Drawing.Size(14, 13);
            darkLabel19.TabIndex = 99;
            darkLabel19.Text = "Y";
            // 
            // nudRotY
            // 
            nudRotY.DecimalPlaces = 2;
            nudRotY.Enabled = false;
            nudRotY.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudRotY.Location = new System.Drawing.Point(170, 38);
            nudRotY.LoopValues = true;
            nudRotY.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            nudRotY.Minimum = new decimal(new int[] { 360, 0, 0, int.MinValue });
            nudRotY.Name = "nudRotY";
            nudRotY.Size = new System.Drawing.Size(64, 22);
            nudRotY.TabIndex = 105;
            nudRotY.ValueChanged += ValidateInstance;
            nudRotY.Validated += ValidateInstance;
            // 
            // nudRotZ
            // 
            nudRotZ.DecimalPlaces = 2;
            nudRotZ.Enabled = false;
            nudRotZ.IncrementAlternate = new decimal(new int[] { 50, 0, 0, 65536 });
            nudRotZ.Location = new System.Drawing.Point(254, 38);
            nudRotZ.LoopValues = true;
            nudRotZ.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            nudRotZ.Minimum = new decimal(new int[] { 360, 0, 0, int.MinValue });
            nudRotZ.Name = "nudRotZ";
            nudRotZ.Size = new System.Drawing.Size(64, 22);
            nudRotZ.TabIndex = 107;
            nudRotZ.ValueChanged += ValidateInstance;
            nudRotZ.Validated += ValidateInstance;
            // 
            // darkLabel2
            // 
            darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new System.Drawing.Point(10, 68);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new System.Drawing.Size(55, 13);
            darkLabel2.TabIndex = 119;
            darkLabel2.Text = "Scale:";
            // 
            // darkLabel3
            // 
            darkLabel3.AutoSize = true;
            darkLabel3.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel3.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel3.Location = new System.Drawing.Point(238, 68);
            darkLabel3.Name = "darkLabel3";
            darkLabel3.Size = new System.Drawing.Size(14, 13);
            darkLabel3.TabIndex = 115;
            darkLabel3.Text = "Z";
            // 
            // darkLabel4
            // 
            darkLabel4.AutoSize = true;
            darkLabel4.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel4.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel4.Location = new System.Drawing.Point(155, 68);
            darkLabel4.Name = "darkLabel4";
            darkLabel4.Size = new System.Drawing.Size(14, 13);
            darkLabel4.TabIndex = 114;
            darkLabel4.Text = "Y";
            // 
            // nudScaleX
            // 
            nudScaleX.DecimalPlaces = 2;
            nudScaleX.Enabled = false;
            nudScaleX.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            nudScaleX.IncrementAlternate = new decimal(new int[] { 1, 0, 0, 0 });
            nudScaleX.Location = new System.Drawing.Point(87, 66);
            nudScaleX.LoopValues = false;
            nudScaleX.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            nudScaleX.Name = "nudScaleX";
            nudScaleX.Size = new System.Drawing.Size(64, 22);
            nudScaleX.TabIndex = 116;
            nudScaleX.ValueChanged += ValidateInstance;
            nudScaleX.Validated += nudScaleX_Validated;
            // 
            // darkLabel5
            // 
            darkLabel5.AutoSize = true;
            darkLabel5.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            darkLabel5.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            darkLabel5.ForeColor = System.Drawing.Color.FromArgb(211, 211, 211);
            darkLabel5.Location = new System.Drawing.Point(71, 68);
            darkLabel5.Name = "darkLabel5";
            darkLabel5.Size = new System.Drawing.Size(14, 13);
            darkLabel5.TabIndex = 113;
            darkLabel5.Text = "X";
            // 
            // nudScaleY
            // 
            nudScaleY.DecimalPlaces = 2;
            nudScaleY.Enabled = false;
            nudScaleY.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            nudScaleY.IncrementAlternate = new decimal(new int[] { 1, 0, 0, 0 });
            nudScaleY.Location = new System.Drawing.Point(170, 66);
            nudScaleY.LoopValues = false;
            nudScaleY.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            nudScaleY.Name = "nudScaleY";
            nudScaleY.Size = new System.Drawing.Size(64, 22);
            nudScaleY.TabIndex = 117;
            nudScaleY.ValueChanged += ValidateInstance;
            // 
            // nudScaleZ
            // 
            nudScaleZ.DecimalPlaces = 2;
            nudScaleZ.Enabled = false;
            nudScaleZ.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            nudScaleZ.IncrementAlternate = new decimal(new int[] { 1, 0, 0, 0 });
            nudScaleZ.Location = new System.Drawing.Point(254, 66);
            nudScaleZ.LoopValues = false;
            nudScaleZ.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            nudScaleZ.Name = "nudScaleZ";
            nudScaleZ.Size = new System.Drawing.Size(64, 22);
            nudScaleZ.TabIndex = 118;
            nudScaleZ.ValueChanged += ValidateInstance;
            // 
            // butCancel
            // 
            butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butCancel.Checked = false;
            butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            butCancel.Location = new System.Drawing.Point(238, 100);
            butCancel.Name = "butCancel";
            butCancel.Size = new System.Drawing.Size(80, 23);
            butCancel.TabIndex = 121;
            butCancel.Text = "Cancel";
            butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butCancel.Click += butCancel_Click;
            // 
            // butOk
            // 
            butOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butOk.Checked = false;
            butOk.Location = new System.Drawing.Point(152, 100);
            butOk.Name = "butOk";
            butOk.Size = new System.Drawing.Size(80, 23);
            butOk.TabIndex = 120;
            butOk.Text = "OK";
            butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butOk.Click += butOk_Click;
            // 
            // FormTransform
            // 
            AcceptButton = butOk;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = butCancel;
            ClientSize = new System.Drawing.Size(329, 132);
            Controls.Add(butCancel);
            Controls.Add(butOk);
            Controls.Add(darkLabel2);
            Controls.Add(darkLabel3);
            Controls.Add(darkLabel4);
            Controls.Add(nudScaleX);
            Controls.Add(darkLabel5);
            Controls.Add(nudScaleY);
            Controls.Add(nudScaleZ);
            Controls.Add(darkLabel29);
            Controls.Add(darkLabel28);
            Controls.Add(darkLabel21);
            Controls.Add(darkLabel26);
            Controls.Add(nudTransX);
            Controls.Add(darkLabel27);
            Controls.Add(nudTransY);
            Controls.Add(nudTransZ);
            Controls.Add(darkLabel1);
            Controls.Add(darkLabel18);
            Controls.Add(nudRotX);
            Controls.Add(darkLabel19);
            Controls.Add(nudRotY);
            Controls.Add(nudRotZ);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormTransform";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Transform Edit";
            ((System.ComponentModel.ISupportInitialize)nudTransX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTransY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTransZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudRotX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudRotY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudRotZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudScaleX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudScaleY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudScaleZ).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DarkUI.Controls.DarkLabel darkLabel29;
        private DarkUI.Controls.DarkLabel darkLabel28;
        private DarkUI.Controls.DarkLabel darkLabel21;
        private DarkUI.Controls.DarkLabel darkLabel26;
        private DarkUI.Controls.DarkNumericUpDown nudTransX;
        private DarkUI.Controls.DarkLabel darkLabel27;
        private DarkUI.Controls.DarkNumericUpDown nudTransY;
        private DarkUI.Controls.DarkNumericUpDown nudTransZ;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel18;
        private DarkUI.Controls.DarkNumericUpDown nudRotX;
        private DarkUI.Controls.DarkLabel darkLabel19;
        private DarkUI.Controls.DarkNumericUpDown nudRotY;
        private DarkUI.Controls.DarkNumericUpDown nudRotZ;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkNumericUpDown nudScaleX;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkNumericUpDown nudScaleY;
        private DarkUI.Controls.DarkNumericUpDown nudScaleZ;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
    }
}