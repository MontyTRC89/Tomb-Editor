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
            this.cbLara = new DarkUI.Controls.DarkCheckBox();
            this.panelControls = new System.Windows.Forms.Panel();
            this.cbFlybys = new DarkUI.Controls.DarkCheckBox();
            this.cbStatics = new DarkUI.Controls.DarkCheckBox();
            this.cbOtherMoveables = new DarkUI.Controls.DarkCheckBox();
            this.cbNPC = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.tbName = new DarkUI.Controls.DarkTextBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.tbScript = new TombLib.Controls.LuaTextBox();
            this.cmbEvent = new DarkUI.Controls.DarkComboBox();
            this.panelButtons.SuspendLayout();
            this.panelControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.butOK);
            this.panelButtons.Controls.Add(this.butCancel);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 390);
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
            // cbLara
            // 
            this.cbLara.AutoSize = true;
            this.cbLara.Location = new System.Drawing.Point(115, 368);
            this.cbLara.Name = "cbLara";
            this.cbLara.Size = new System.Drawing.Size(47, 17);
            this.cbLara.TabIndex = 17;
            this.cbLara.Text = "Lara";
            this.cbLara.CheckedChanged += new System.EventHandler(this.cbLara_CheckedChanged);
            // 
            // panelControls
            // 
            this.panelControls.Controls.Add(this.cbFlybys);
            this.panelControls.Controls.Add(this.cbStatics);
            this.panelControls.Controls.Add(this.cbOtherMoveables);
            this.panelControls.Controls.Add(this.cbNPC);
            this.panelControls.Controls.Add(this.cbLara);
            this.panelControls.Controls.Add(this.darkLabel3);
            this.panelControls.Controls.Add(this.tbName);
            this.panelControls.Controls.Add(this.darkLabel2);
            this.panelControls.Controls.Add(this.darkLabel1);
            this.panelControls.Controls.Add(this.tbScript);
            this.panelControls.Controls.Add(this.cmbEvent);
            this.panelControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControls.Location = new System.Drawing.Point(0, 0);
            this.panelControls.Name = "panelControls";
            this.panelControls.Size = new System.Drawing.Size(719, 390);
            this.panelControls.TabIndex = 87;
            // 
            // cbFlybys
            // 
            this.cbFlybys.AutoSize = true;
            this.cbFlybys.Location = new System.Drawing.Point(410, 368);
            this.cbFlybys.Name = "cbFlybys";
            this.cbFlybys.Size = new System.Drawing.Size(57, 17);
            this.cbFlybys.TabIndex = 21;
            this.cbFlybys.Text = "Flybys";
            this.cbFlybys.CheckedChanged += new System.EventHandler(this.cbFlybys_CheckedChanged);
            // 
            // cbStatics
            // 
            this.cbStatics.AutoSize = true;
            this.cbStatics.Location = new System.Drawing.Point(345, 368);
            this.cbStatics.Name = "cbStatics";
            this.cbStatics.Size = new System.Drawing.Size(59, 17);
            this.cbStatics.TabIndex = 20;
            this.cbStatics.Text = "Statics";
            this.cbStatics.CheckedChanged += new System.EventHandler(this.cbStatics_CheckedChanged);
            // 
            // cbOtherMoveables
            // 
            this.cbOtherMoveables.AutoSize = true;
            this.cbOtherMoveables.Location = new System.Drawing.Point(226, 368);
            this.cbOtherMoveables.Name = "cbOtherMoveables";
            this.cbOtherMoveables.Size = new System.Drawing.Size(113, 17);
            this.cbOtherMoveables.TabIndex = 19;
            this.cbOtherMoveables.Text = "Other moveables";
            this.cbOtherMoveables.CheckedChanged += new System.EventHandler(this.cbOtherMoveables_CheckedChanged);
            // 
            // cbNPC
            // 
            this.cbNPC.AutoSize = true;
            this.cbNPC.Location = new System.Drawing.Point(168, 368);
            this.cbNPC.Name = "cbNPC";
            this.cbNPC.Size = new System.Drawing.Size(52, 17);
            this.cbNPC.TabIndex = 18;
            this.cbNPC.Text = "NPCs";
            this.cbNPC.CheckedChanged += new System.EventHandler(this.cbNPC_CheckedChanged);
            // 
            // darkLabel3
            // 
            this.darkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(3, 369);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(104, 13);
            this.darkLabel3.TabIndex = 5;
            this.darkLabel3.Text = "Possible activators:";
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
            // darkLabel1
            // 
            this.darkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(3, 340);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(38, 13);
            this.darkLabel1.TabIndex = 2;
            this.darkLabel1.Text = "Event:";
            // 
            // tbScript
            // 
            this.tbScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbScript.Code = "";
            this.tbScript.Location = new System.Drawing.Point(3, 35);
            this.tbScript.Name = "tbScript";
            this.tbScript.Size = new System.Drawing.Size(713, 296);
            this.tbScript.TabIndex = 1;
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
            this.cmbEvent.Location = new System.Drawing.Point(47, 337);
            this.cmbEvent.Name = "cmbEvent";
            this.cmbEvent.Size = new System.Drawing.Size(669, 23);
            this.cmbEvent.TabIndex = 0;
            this.cmbEvent.SelectedIndexChanged += new System.EventHandler(this.cmbEvent_SelectedIndexChanged);
            // 
            // FormVolume
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 421);
            this.Controls.Add(this.panelControls);
            this.Controls.Add(this.panelButtons);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Name = "FormVolume";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Volume editor";
            this.panelButtons.ResumeLayout(false);
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
        private DarkUI.Controls.DarkCheckBox cbLara;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkCheckBox cbOtherMoveables;
        private DarkUI.Controls.DarkCheckBox cbNPC;
        private DarkUI.Controls.DarkCheckBox cbStatics;
        private DarkUI.Controls.DarkCheckBox cbFlybys;
    }
}