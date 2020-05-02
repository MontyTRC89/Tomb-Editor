namespace TombEditor.Forms
{
    partial class FormVolume
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
            this.panelButtons = new System.Windows.Forms.Panel();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.panelControls = new System.Windows.Forms.Panel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.cmbEvent = new DarkUI.Controls.DarkComboBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.tbName = new DarkUI.Controls.DarkTextBox();
            this.cbEnabled = new DarkUI.Controls.DarkCheckBox();
            this.tbScript = new TombLib.Controls.LuaTextBox();
            this.panelButtons.SuspendLayout();
            this.panelControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.cbEnabled);
            this.panelButtons.Controls.Add(this.butOK);
            this.panelButtons.Controls.Add(this.butCancel);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 336);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(719, 31);
            this.panelButtons.TabIndex = 86;
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Checked = false;
            this.butOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.butOK.Location = new System.Drawing.Point(550, 4);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 15;
            this.butOK.Text = "OK";
            this.butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(636, 4);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 16;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // panelControls
            // 
            this.panelControls.Controls.Add(this.tbName);
            this.panelControls.Controls.Add(this.darkLabel2);
            this.panelControls.Controls.Add(this.darkLabel1);
            this.panelControls.Controls.Add(this.tbScript);
            this.panelControls.Controls.Add(this.cmbEvent);
            this.panelControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControls.Location = new System.Drawing.Point(0, 0);
            this.panelControls.Name = "panelControls";
            this.panelControls.Size = new System.Drawing.Size(719, 336);
            this.panelControls.TabIndex = 87;
            // 
            // darkLabel1
            // 
            this.darkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(3, 312);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(38, 13);
            this.darkLabel1.TabIndex = 2;
            this.darkLabel1.Text = "Event:";
            // 
            // cmbEvent
            // 
            this.cmbEvent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbEvent.FormattingEnabled = true;
            this.cmbEvent.Items.AddRange(new object[] {
            "On Enter",
            "On Leave",
            "On Inside",
            "Global Environment"});
            this.cmbEvent.Location = new System.Drawing.Point(47, 309);
            this.cmbEvent.Name = "cmbEvent";
            this.cmbEvent.Size = new System.Drawing.Size(669, 23);
            this.cmbEvent.TabIndex = 0;
            this.cmbEvent.SelectedIndexChanged += new System.EventHandler(this.cmbEvent_SelectedIndexChanged);
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(7, 11);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(70, 13);
            this.darkLabel2.TabIndex = 3;
            this.darkLabel2.Text = "Script name:";
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(83, 9);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(633, 22);
            this.tbName.TabIndex = 4;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // cbEnabled
            // 
            this.cbEnabled.AutoSize = true;
            this.cbEnabled.Location = new System.Drawing.Point(3, 8);
            this.cbEnabled.Name = "cbEnabled";
            this.cbEnabled.Size = new System.Drawing.Size(104, 17);
            this.cbEnabled.TabIndex = 17;
            this.cbEnabled.Text = "Disable volume";
            this.cbEnabled.CheckedChanged += new System.EventHandler(this.cbEnabled_CheckedChanged);
            // 
            // tbScript
            // 
            this.tbScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbScript.Code = "";
            this.tbScript.Location = new System.Drawing.Point(3, 35);
            this.tbScript.Name = "tbScript";
            this.tbScript.Size = new System.Drawing.Size(713, 269);
            this.tbScript.TabIndex = 1;
            // 
            // FormVolume
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 367);
            this.Controls.Add(this.panelControls);
            this.Controls.Add(this.panelButtons);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Name = "FormVolume";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Volume editor";
            this.panelButtons.ResumeLayout(false);
            this.panelButtons.PerformLayout();
            this.panelControls.ResumeLayout(false);
            this.panelControls.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelButtons;
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butCancel;
        private System.Windows.Forms.Panel panelControls;
        private DarkUI.Controls.DarkComboBox cmbEvent;
        private TombLib.Controls.LuaTextBox tbScript;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkTextBox tbName;
        private DarkUI.Controls.DarkCheckBox cbEnabled;
    }
}