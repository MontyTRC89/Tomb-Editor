namespace TombEditor
{
    partial class FormSound
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
            this.lstSamples = new BrightIdeasSoftware.FastObjectListView();
            this.olvColumn3 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn4 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn5 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.butPlay = new System.Windows.Forms.Button();
            this.butCancel = new System.Windows.Forms.Button();
            this.butOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbSound = new System.Windows.Forms.TextBox();
            this.cbBit5 = new System.Windows.Forms.CheckBox();
            this.cbBit1 = new System.Windows.Forms.CheckBox();
            this.cbBit2 = new System.Windows.Forms.CheckBox();
            this.cbBit3 = new System.Windows.Forms.CheckBox();
            this.cbBit4 = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.lstSamples)).BeginInit();
            this.SuspendLayout();
            // 
            // lstSamples
            // 
            this.lstSamples.AllColumns.Add(this.olvColumn3);
            this.lstSamples.AllColumns.Add(this.olvColumn4);
            this.lstSamples.AllColumns.Add(this.olvColumn5);
            this.lstSamples.CellEditUseWholeCell = false;
            this.lstSamples.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn3,
            this.olvColumn4,
            this.olvColumn5});
            this.lstSamples.Cursor = System.Windows.Forms.Cursors.Default;
            this.lstSamples.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstSamples.FullRowSelect = true;
            this.lstSamples.GridLines = true;
            this.lstSamples.Location = new System.Drawing.Point(12, 12);
            this.lstSamples.MultiSelect = false;
            this.lstSamples.Name = "lstSamples";
            this.lstSamples.RowHeight = 25;
            this.lstSamples.ShowGroups = false;
            this.lstSamples.ShowImagesOnSubItems = true;
            this.lstSamples.Size = new System.Drawing.Size(597, 387);
            this.lstSamples.TabIndex = 52;
            this.lstSamples.UseCompatibleStateImageBehavior = false;
            this.lstSamples.View = System.Windows.Forms.View.Details;
            this.lstSamples.VirtualMode = true;
            this.lstSamples.Click += new System.EventHandler(this.lstSamples_Click);
            // 
            // olvColumn3
            // 
            this.olvColumn3.AspectName = "ID";
            this.olvColumn3.Text = "ID";
            // 
            // olvColumn4
            // 
            this.olvColumn4.AspectName = "Name";
            this.olvColumn4.Text = "Name";
            this.olvColumn4.Width = 250;
            // 
            // olvColumn5
            // 
            this.olvColumn5.AspectName = "File";
            this.olvColumn5.Text = "File";
            this.olvColumn5.Width = 250;
            // 
            // butPlay
            // 
            this.butPlay.Image = global::TombEditor.Properties.Resources.sound1;
            this.butPlay.Location = new System.Drawing.Point(523, 405);
            this.butPlay.Name = "butPlay";
            this.butPlay.Size = new System.Drawing.Size(86, 32);
            this.butPlay.TabIndex = 53;
            this.butPlay.Text = "Play";
            this.butPlay.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.butPlay.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlay.UseVisualStyleBackColor = true;
            this.butPlay.Click += new System.EventHandler(this.butPlay_Click);
            // 
            // butCancel
            // 
            this.butCancel.Image = global::TombEditor.Properties.Resources.cross;
            this.butCancel.Location = new System.Drawing.Point(310, 490);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(86, 32);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.UseVisualStyleBackColor = true;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Image = global::TombEditor.Properties.Resources.tick;
            this.butOK.Location = new System.Drawing.Point(218, 490);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(86, 32);
            this.butOK.TabIndex = 0;
            this.butOK.Text = "OK";
            this.butOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOK.UseVisualStyleBackColor = true;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 415);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 54;
            this.label1.Text = "Sound:";
            // 
            // tbSound
            // 
            this.tbSound.Location = new System.Drawing.Point(59, 412);
            this.tbSound.Name = "tbSound";
            this.tbSound.ReadOnly = true;
            this.tbSound.Size = new System.Drawing.Size(259, 20);
            this.tbSound.TabIndex = 55;
            // 
            // cbBit5
            // 
            this.cbBit5.AutoSize = true;
            this.cbBit5.Location = new System.Drawing.Point(271, 448);
            this.cbBit5.Name = "cbBit5";
            this.cbBit5.Size = new System.Drawing.Size(47, 17);
            this.cbBit5.TabIndex = 56;
            this.cbBit5.Text = "Bit 5";
            this.cbBit5.UseVisualStyleBackColor = true;
            // 
            // cbBit1
            // 
            this.cbBit1.AutoSize = true;
            this.cbBit1.Location = new System.Drawing.Point(59, 448);
            this.cbBit1.Name = "cbBit1";
            this.cbBit1.Size = new System.Drawing.Size(47, 17);
            this.cbBit1.TabIndex = 57;
            this.cbBit1.Text = "Bit 1";
            this.cbBit1.UseVisualStyleBackColor = true;
            // 
            // cbBit2
            // 
            this.cbBit2.AutoSize = true;
            this.cbBit2.Location = new System.Drawing.Point(112, 448);
            this.cbBit2.Name = "cbBit2";
            this.cbBit2.Size = new System.Drawing.Size(47, 17);
            this.cbBit2.TabIndex = 58;
            this.cbBit2.Text = "Bit 2";
            this.cbBit2.UseVisualStyleBackColor = true;
            // 
            // cbBit3
            // 
            this.cbBit3.AutoSize = true;
            this.cbBit3.Location = new System.Drawing.Point(165, 448);
            this.cbBit3.Name = "cbBit3";
            this.cbBit3.Size = new System.Drawing.Size(47, 17);
            this.cbBit3.TabIndex = 59;
            this.cbBit3.Text = "Bit 3";
            this.cbBit3.UseVisualStyleBackColor = true;
            // 
            // cbBit4
            // 
            this.cbBit4.AutoSize = true;
            this.cbBit4.Location = new System.Drawing.Point(218, 448);
            this.cbBit4.Name = "cbBit4";
            this.cbBit4.Size = new System.Drawing.Size(47, 17);
            this.cbBit4.TabIndex = 60;
            this.cbBit4.Text = "Bit 4";
            this.cbBit4.UseVisualStyleBackColor = true;
            // 
            // FormSound
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 531);
            this.Controls.Add(this.cbBit4);
            this.Controls.Add(this.cbBit3);
            this.Controls.Add(this.cbBit2);
            this.Controls.Add(this.cbBit1);
            this.Controls.Add(this.cbBit5);
            this.Controls.Add(this.tbSound);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.butPlay);
            this.Controls.Add(this.lstSamples);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSound";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sound source";
            this.Load += new System.EventHandler(this.FormSound_Load);
            ((System.ComponentModel.ISupportInitialize)(this.lstSamples)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button butOK;
        private System.Windows.Forms.Button butCancel;
        internal BrightIdeasSoftware.FastObjectListView lstSamples;
        private BrightIdeasSoftware.OLVColumn olvColumn3;
        private BrightIdeasSoftware.OLVColumn olvColumn4;
        private BrightIdeasSoftware.OLVColumn olvColumn5;
        private System.Windows.Forms.Button butPlay;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbSound;
        private System.Windows.Forms.CheckBox cbBit5;
        private System.Windows.Forms.CheckBox cbBit1;
        private System.Windows.Forms.CheckBox cbBit2;
        private System.Windows.Forms.CheckBox cbBit3;
        private System.Windows.Forms.CheckBox cbBit4;
    }
}