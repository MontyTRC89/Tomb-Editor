namespace TombLib.Controls
{
    partial class TriggerParameterControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.colorPreview = new DarkUI.Controls.DarkPanel();
            this.label = new DarkUI.Controls.DarkLabel();
            this.numericUpDown = new DarkUI.Controls.DarkNumericUpDown();
            this.butView = new DarkUI.Controls.DarkButton();
            this.butReset = new DarkUI.Controls.DarkButton();
            this.combo = new TombLib.Controls.DarkSearchableComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // colorPreview
            // 
            this.colorPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.colorPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.colorPreview.Enabled = false;
            this.colorPreview.Location = new System.Drawing.Point(187, 0);
            this.colorPreview.Name = "colorPreview";
            this.colorPreview.Size = new System.Drawing.Size(55, 23);
            this.colorPreview.TabIndex = 1;
            // 
            // label
            // 
            this.label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label.Location = new System.Drawing.Point(2, 2);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(240, 19);
            this.label.TabIndex = 6;
            this.label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label.Visible = false;
            this.label.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label_MouseDown);
            // 
            // numericUpDown
            // 
            this.numericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDown.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.numericUpDown.IncrementAlternate = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.numericUpDown.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown.LoopValues = false;
            this.numericUpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDown.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.numericUpDown.Name = "numericUpDown";
            this.numericUpDown.Size = new System.Drawing.Size(259, 23);
            this.numericUpDown.TabIndex = 2;
            this.numericUpDown.Visible = false;
            this.numericUpDown.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // butView
            // 
            this.butView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butView.Checked = false;
            this.butView.Location = new System.Drawing.Point(264, 0);
            this.butView.Name = "butView";
            this.butView.Size = new System.Drawing.Size(51, 23);
            this.butView.TabIndex = 4;
            this.butView.Text = "Locate";
            this.butView.Visible = false;
            this.butView.Click += new System.EventHandler(this.butView_Click);
            // 
            // butReset
            // 
            this.butReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butReset.BackColor = System.Drawing.Color.Maroon;
            this.butReset.BackColorUseGeneric = false;
            this.butReset.Checked = false;
            this.butReset.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butReset.Location = new System.Drawing.Point(264, 0);
            this.butReset.Name = "butReset";
            this.butReset.Size = new System.Drawing.Size(51, 23);
            this.butReset.TabIndex = 5;
            this.butReset.Text = "Reset";
            this.butReset.Visible = false;
            this.butReset.Click += new System.EventHandler(this.butReset_Click);
            // 
            // combo
            // 
            this.combo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.combo.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.combo.Location = new System.Drawing.Point(0, 0);
            this.combo.Name = "combo";
            this.combo.Size = new System.Drawing.Size(259, 23);
            this.combo.TabIndex = 0;
            this.combo.TabStop = false;
            this.combo.Visible = false;
            this.combo.SelectedIndexChanged += new System.EventHandler(this.combo_SelectedIndexChanged);
            // 
            // TriggerParameterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.colorPreview);
            this.Controls.Add(this.label);
            this.Controls.Add(this.numericUpDown);
            this.Controls.Add(this.combo);
            this.Controls.Add(this.butReset);
            this.Controls.Add(this.butView);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MaximumSize = new System.Drawing.Size(30000, 23);
            this.MinimumSize = new System.Drawing.Size(100, 23);
            this.Name = "TriggerParameterControl";
            this.Size = new System.Drawing.Size(315, 23);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkButton butView;
        private TombLib.Controls.DarkSearchableComboBox combo;
        private DarkUI.Controls.DarkLabel label;
        private DarkUI.Controls.DarkNumericUpDown numericUpDown;
        private DarkUI.Controls.DarkPanel colorPreview;
        private DarkUI.Controls.DarkButton butReset;
    }
}
