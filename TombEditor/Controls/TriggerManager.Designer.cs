
namespace TombEditor.Controls
{
    partial class TriggerManager
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
            this.cbActivatorFlyBy = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorStatics = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorOtherMoveables = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorNPC = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorLara = new DarkUI.Controls.DarkCheckBox();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.darkPanel3 = new DarkUI.Controls.DarkPanel();
            this.rbConstructor = new DarkUI.Controls.DarkRadioButton();
            this.rbLevelScript = new DarkUI.Controls.DarkRadioButton();
            this.darkPanel4 = new DarkUI.Controls.DarkPanel();
            this.tabbedContainer = new TombLib.Controls.DarkTabbedContainer();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lstFunctions = new DarkUI.Controls.DarkListView();
            this.darkPanel1 = new DarkUI.Controls.DarkPanel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.darkTextBox1 = new DarkUI.Controls.DarkTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.darkPanel2 = new DarkUI.Controls.DarkPanel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkGroupBox1.SuspendLayout();
            this.darkPanel3.SuspendLayout();
            this.darkPanel4.SuspendLayout();
            this.tabbedContainer.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.darkPanel1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.darkPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbActivatorFlyBy
            // 
            this.cbActivatorFlyBy.AutoSize = true;
            this.cbActivatorFlyBy.Location = new System.Drawing.Point(331, 7);
            this.cbActivatorFlyBy.Name = "cbActivatorFlyBy";
            this.cbActivatorFlyBy.Size = new System.Drawing.Size(90, 17);
            this.cbActivatorFlyBy.TabIndex = 19;
            this.cbActivatorFlyBy.Text = "FlyBy camera";
            // 
            // cbActivatorStatics
            // 
            this.cbActivatorStatics.AutoSize = true;
            this.cbActivatorStatics.Location = new System.Drawing.Point(269, 7);
            this.cbActivatorStatics.Name = "cbActivatorStatics";
            this.cbActivatorStatics.Size = new System.Drawing.Size(59, 17);
            this.cbActivatorStatics.TabIndex = 18;
            this.cbActivatorStatics.Text = "Statics";
            // 
            // cbActivatorOtherMoveables
            // 
            this.cbActivatorOtherMoveables.AutoSize = true;
            this.cbActivatorOtherMoveables.Location = new System.Drawing.Point(172, 7);
            this.cbActivatorOtherMoveables.Name = "cbActivatorOtherMoveables";
            this.cbActivatorOtherMoveables.Size = new System.Drawing.Size(96, 17);
            this.cbActivatorOtherMoveables.TabIndex = 17;
            this.cbActivatorOtherMoveables.Text = "Other objects";
            // 
            // cbActivatorNPC
            // 
            this.cbActivatorNPC.AutoSize = true;
            this.cbActivatorNPC.Location = new System.Drawing.Point(119, 7);
            this.cbActivatorNPC.Name = "cbActivatorNPC";
            this.cbActivatorNPC.Size = new System.Drawing.Size(47, 17);
            this.cbActivatorNPC.TabIndex = 16;
            this.cbActivatorNPC.Text = "NPC";
            // 
            // cbActivatorLara
            // 
            this.cbActivatorLara.AutoSize = true;
            this.cbActivatorLara.Location = new System.Drawing.Point(66, 7);
            this.cbActivatorLara.Name = "cbActivatorLara";
            this.cbActivatorLara.Size = new System.Drawing.Size(47, 17);
            this.cbActivatorLara.TabIndex = 15;
            this.cbActivatorLara.Text = "Lara";
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkGroupBox1.Controls.Add(this.darkLabel3);
            this.darkGroupBox1.Controls.Add(this.cbActivatorLara);
            this.darkGroupBox1.Controls.Add(this.cbActivatorFlyBy);
            this.darkGroupBox1.Controls.Add(this.cbActivatorNPC);
            this.darkGroupBox1.Controls.Add(this.cbActivatorStatics);
            this.darkGroupBox1.Controls.Add(this.cbActivatorOtherMoveables);
            this.darkGroupBox1.Location = new System.Drawing.Point(3, 3);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(752, 28);
            this.darkGroupBox1.TabIndex = 20;
            this.darkGroupBox1.TabStop = false;
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(3, 8);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(60, 13);
            this.darkLabel3.TabIndex = 20;
            this.darkLabel3.Text = "Activators:";
            // 
            // darkPanel3
            // 
            this.darkPanel3.Controls.Add(this.rbConstructor);
            this.darkPanel3.Controls.Add(this.rbLevelScript);
            this.darkPanel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.darkPanel3.Location = new System.Drawing.Point(0, 0);
            this.darkPanel3.Name = "darkPanel3";
            this.darkPanel3.Size = new System.Drawing.Size(758, 25);
            this.darkPanel3.TabIndex = 21;
            // 
            // rbConstructor
            // 
            this.rbConstructor.AutoSize = true;
            this.rbConstructor.Location = new System.Drawing.Point(148, 5);
            this.rbConstructor.Name = "rbConstructor";
            this.rbConstructor.Size = new System.Drawing.Size(86, 17);
            this.rbConstructor.TabIndex = 1;
            this.rbConstructor.Text = "Constructor";
            this.rbConstructor.CheckedChanged += new System.EventHandler(this.rbConstructor_CheckedChanged);
            // 
            // rbLevelScript
            // 
            this.rbLevelScript.AutoSize = true;
            this.rbLevelScript.Checked = true;
            this.rbLevelScript.Location = new System.Drawing.Point(9, 5);
            this.rbLevelScript.Name = "rbLevelScript";
            this.rbLevelScript.Size = new System.Drawing.Size(133, 17);
            this.rbLevelScript.TabIndex = 0;
            this.rbLevelScript.TabStop = true;
            this.rbLevelScript.Text = "Level script functions";
            this.rbLevelScript.CheckedChanged += new System.EventHandler(this.rbLevelScript_CheckedChanged);
            // 
            // darkPanel4
            // 
            this.darkPanel4.Controls.Add(this.darkGroupBox1);
            this.darkPanel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.darkPanel4.Location = new System.Drawing.Point(0, 364);
            this.darkPanel4.Name = "darkPanel4";
            this.darkPanel4.Size = new System.Drawing.Size(758, 36);
            this.darkPanel4.TabIndex = 22;
            // 
            // tabbedContainer
            // 
            this.tabbedContainer.Controls.Add(this.tabPage1);
            this.tabbedContainer.Controls.Add(this.tabPage2);
            this.tabbedContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabbedContainer.Location = new System.Drawing.Point(0, 25);
            this.tabbedContainer.Name = "tabbedContainer";
            this.tabbedContainer.SelectedIndex = 0;
            this.tabbedContainer.Size = new System.Drawing.Size(758, 339);
            this.tabbedContainer.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lstFunctions);
            this.tabPage1.Controls.Add(this.darkPanel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(750, 313);
            this.tabPage1.TabIndex = 8;
            this.tabPage1.Text = "Level script functions";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // lstFunctions
            // 
            this.lstFunctions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstFunctions.Location = new System.Drawing.Point(3, 3);
            this.lstFunctions.Name = "lstFunctions";
            this.lstFunctions.Size = new System.Drawing.Size(744, 276);
            this.lstFunctions.TabIndex = 0;
            this.lstFunctions.Text = "darkListView1";
            // 
            // darkPanel1
            // 
            this.darkPanel1.Controls.Add(this.darkLabel2);
            this.darkPanel1.Controls.Add(this.darkTextBox1);
            this.darkPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.darkPanel1.Location = new System.Drawing.Point(3, 285);
            this.darkPanel1.Name = "darkPanel1";
            this.darkPanel1.Size = new System.Drawing.Size(744, 25);
            this.darkPanel1.TabIndex = 1;
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(0, 5);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(61, 13);
            this.darkLabel2.TabIndex = 1;
            this.darkLabel2.Text = "Argument:";
            // 
            // darkTextBox1
            // 
            this.darkTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkTextBox1.Location = new System.Drawing.Point(63, 3);
            this.darkTextBox1.Name = "darkTextBox1";
            this.darkTextBox1.Size = new System.Drawing.Size(681, 22);
            this.darkTextBox1.TabIndex = 0;
            this.darkTextBox1.Text = "                                             ";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.darkPanel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(750, 313);
            this.tabPage2.TabIndex = 9;
            this.tabPage2.Text = "Constructor";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // darkPanel2
            // 
            this.darkPanel2.Controls.Add(this.darkLabel1);
            this.darkPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkPanel2.Location = new System.Drawing.Point(3, 3);
            this.darkPanel2.Name = "darkPanel2";
            this.darkPanel2.Size = new System.Drawing.Size(744, 307);
            this.darkPanel2.TabIndex = 0;
            // 
            // darkLabel1
            // 
            this.darkLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(0, 0);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(744, 307);
            this.darkLabel1.TabIndex = 0;
            this.darkLabel1.Text = "SORY!";
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TriggerManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabbedContainer);
            this.Controls.Add(this.darkPanel4);
            this.Controls.Add(this.darkPanel3);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Name = "TriggerManager";
            this.Size = new System.Drawing.Size(758, 400);
            this.darkGroupBox1.ResumeLayout(false);
            this.darkGroupBox1.PerformLayout();
            this.darkPanel3.ResumeLayout(false);
            this.darkPanel3.PerformLayout();
            this.darkPanel4.ResumeLayout(false);
            this.tabbedContainer.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.darkPanel1.ResumeLayout(false);
            this.darkPanel1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.darkPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private TombLib.Controls.DarkTabbedContainer tabbedContainer;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkCheckBox cbActivatorLara;
        private DarkUI.Controls.DarkCheckBox cbActivatorFlyBy;
        private DarkUI.Controls.DarkCheckBox cbActivatorNPC;
        private DarkUI.Controls.DarkCheckBox cbActivatorStatics;
        private DarkUI.Controls.DarkCheckBox cbActivatorOtherMoveables;
        private DarkUI.Controls.DarkPanel darkPanel3;
        private DarkUI.Controls.DarkRadioButton rbConstructor;
        private DarkUI.Controls.DarkRadioButton rbLevelScript;
        private DarkUI.Controls.DarkPanel darkPanel4;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private System.Windows.Forms.TabPage tabPage1;
        private DarkUI.Controls.DarkListView lstFunctions;
        private DarkUI.Controls.DarkPanel darkPanel1;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkTextBox darkTextBox1;
        private System.Windows.Forms.TabPage tabPage2;
        private DarkUI.Controls.DarkPanel darkPanel2;
        private DarkUI.Controls.DarkLabel darkLabel1;
    }
}
