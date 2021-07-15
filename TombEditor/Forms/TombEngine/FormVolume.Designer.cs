namespace TombEditor.Forms.TombEngine
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
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.cbActivatorLara = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorNPC = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorOtherMoveables = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorStatics = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorFlyBy = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.comboboxOnEnter = new DarkUI.Controls.DarkComboBox();
            this.comboboxOnInside = new DarkUI.Controls.DarkComboBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.comboboxOnLeave = new DarkUI.Controls.DarkComboBox();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(419, 159);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 2;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Checked = false;
            this.butOk.Location = new System.Drawing.Point(333, 159);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 1;
            this.butOk.Text = "OK";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(12, 9);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(62, 13);
            this.darkLabel2.TabIndex = 9;
            this.darkLabel2.Text = "Activators:";
            // 
            // cbActivatorLara
            // 
            this.cbActivatorLara.AutoSize = true;
            this.cbActivatorLara.Location = new System.Drawing.Point(12, 34);
            this.cbActivatorLara.Name = "cbActivatorLara";
            this.cbActivatorLara.Size = new System.Drawing.Size(47, 17);
            this.cbActivatorLara.TabIndex = 10;
            this.cbActivatorLara.Text = "Lara";
            // 
            // cbActivatorNPC
            // 
            this.cbActivatorNPC.AutoSize = true;
            this.cbActivatorNPC.Location = new System.Drawing.Point(12, 57);
            this.cbActivatorNPC.Name = "cbActivatorNPC";
            this.cbActivatorNPC.Size = new System.Drawing.Size(47, 17);
            this.cbActivatorNPC.TabIndex = 11;
            this.cbActivatorNPC.Text = "NPC";
            // 
            // cbActivatorOtherMoveables
            // 
            this.cbActivatorOtherMoveables.AutoSize = true;
            this.cbActivatorOtherMoveables.Location = new System.Drawing.Point(12, 80);
            this.cbActivatorOtherMoveables.Name = "cbActivatorOtherMoveables";
            this.cbActivatorOtherMoveables.Size = new System.Drawing.Size(110, 17);
            this.cbActivatorOtherMoveables.TabIndex = 12;
            this.cbActivatorOtherMoveables.Text = "Other moveabes";
            // 
            // cbActivatorStatics
            // 
            this.cbActivatorStatics.AutoSize = true;
            this.cbActivatorStatics.Location = new System.Drawing.Point(12, 103);
            this.cbActivatorStatics.Name = "cbActivatorStatics";
            this.cbActivatorStatics.Size = new System.Drawing.Size(59, 17);
            this.cbActivatorStatics.TabIndex = 13;
            this.cbActivatorStatics.Text = "Statics";
            // 
            // cbActivatorFlyBy
            // 
            this.cbActivatorFlyBy.AutoSize = true;
            this.cbActivatorFlyBy.Location = new System.Drawing.Point(12, 126);
            this.cbActivatorFlyBy.Name = "cbActivatorFlyBy";
            this.cbActivatorFlyBy.Size = new System.Drawing.Size(90, 17);
            this.cbActivatorFlyBy.TabIndex = 14;
            this.cbActivatorFlyBy.Text = "FlyBy camera";
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(176, 9);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(49, 13);
            this.darkLabel3.TabIndex = 15;
            this.darkLabel3.Text = "Actions:";
            // 
            // darkLabel4
            // 
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(179, 34);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(53, 13);
            this.darkLabel4.TabIndex = 16;
            this.darkLabel4.Text = "OnEnter:";
            // 
            // comboboxOnEnter
            // 
            this.comboboxOnEnter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboboxOnEnter.FormattingEnabled = true;
            this.comboboxOnEnter.Location = new System.Drawing.Point(238, 28);
            this.comboboxOnEnter.Name = "comboboxOnEnter";
            this.comboboxOnEnter.Size = new System.Drawing.Size(261, 23);
            this.comboboxOnEnter.TabIndex = 17;
            // 
            // comboboxOnInside
            // 
            this.comboboxOnInside.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboboxOnInside.FormattingEnabled = true;
            this.comboboxOnInside.Location = new System.Drawing.Point(238, 57);
            this.comboboxOnInside.Name = "comboboxOnInside";
            this.comboboxOnInside.Size = new System.Drawing.Size(261, 23);
            this.comboboxOnInside.TabIndex = 19;
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(179, 63);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(57, 13);
            this.darkLabel1.TabIndex = 18;
            this.darkLabel1.Text = "OnInside:";
            // 
            // comboboxOnLeave
            // 
            this.comboboxOnLeave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboboxOnLeave.FormattingEnabled = true;
            this.comboboxOnLeave.Location = new System.Drawing.Point(238, 86);
            this.comboboxOnLeave.Name = "comboboxOnLeave";
            this.comboboxOnLeave.Size = new System.Drawing.Size(261, 23);
            this.comboboxOnLeave.TabIndex = 21;
            // 
            // darkLabel5
            // 
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(179, 92);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(54, 13);
            this.darkLabel5.TabIndex = 20;
            this.darkLabel5.Text = "OnLeave:";
            // 
            // FormTriggerVolume
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(507, 190);
            this.Controls.Add(this.comboboxOnLeave);
            this.Controls.Add(this.darkLabel5);
            this.Controls.Add(this.comboboxOnInside);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.comboboxOnEnter);
            this.Controls.Add(this.darkLabel4);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.cbActivatorFlyBy);
            this.Controls.Add(this.cbActivatorStatics);
            this.Controls.Add(this.cbActivatorOtherMoveables);
            this.Controls.Add(this.cbActivatorNPC);
            this.Controls.Add(this.cbActivatorLara);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormTriggerVolume";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit trigger volume";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkCheckBox cbActivatorLara;
        private DarkUI.Controls.DarkCheckBox cbActivatorNPC;
        private DarkUI.Controls.DarkCheckBox cbActivatorOtherMoveables;
        private DarkUI.Controls.DarkCheckBox cbActivatorStatics;
        private DarkUI.Controls.DarkCheckBox cbActivatorFlyBy;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkComboBox comboboxOnEnter;
        private DarkUI.Controls.DarkComboBox comboboxOnInside;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkComboBox comboboxOnLeave;
        private DarkUI.Controls.DarkLabel darkLabel5;
    }
}