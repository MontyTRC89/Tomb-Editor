using DarkUI.Controls;
using System.Windows.Forms;

namespace TombEditor
{
    partial class FormTrigger
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTrigger));
            this.label1 = new DarkUI.Controls.DarkLabel();
            this.comboType = new DarkUI.Controls.DarkComboBox(this.components);
            this.comboTargetType = new DarkUI.Controls.DarkComboBox(this.components);
            this.label2 = new DarkUI.Controls.DarkLabel();
            this.label3 = new DarkUI.Controls.DarkLabel();
            this.tbParameter = new DarkUI.Controls.DarkTextBox();
            this.tbTimer = new DarkUI.Controls.DarkTextBox();
            this.label4 = new DarkUI.Controls.DarkLabel();
            this.cbBit4 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit3 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit2 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit1 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit5 = new DarkUI.Controls.DarkCheckBox();
            this.cbOneShot = new DarkUI.Controls.DarkCheckBox();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.comboParameter = new DarkUI.Controls.DarkComboBox(this.components);
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label1.Location = new System.Drawing.Point(16, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Trigger Type:";
            // 
            // comboType
            // 
            this.comboType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboType.ForeColor = System.Drawing.Color.White;
            this.comboType.FormattingEnabled = true;
            this.comboType.ItemHeight = 18;
            this.comboType.Items.AddRange(new object[] {
            "Trigger",
            "Pad",
            "Key",
            "Pickup",
            "Condition",
            "Heavy",
            "Dummy",
            "Switch",
            "Antipad",
            "Combat",
            "Antitrigger",
            "HeavySwitch",
            "HeavyAntritrigger",
            "Monkey"});
            this.comboType.Location = new System.Drawing.Point(89, 10);
            this.comboType.Name = "comboType";
            this.comboType.Size = new System.Drawing.Size(159, 24);
            this.comboType.TabIndex = 1;
            this.comboType.SelectedIndexChanged += new System.EventHandler(this.comboType_SelectedIndexChanged);
            // 
            // comboTargetType
            // 
            this.comboTargetType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboTargetType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboTargetType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTargetType.ForeColor = System.Drawing.Color.White;
            this.comboTargetType.FormattingEnabled = true;
            this.comboTargetType.ItemHeight = 18;
            this.comboTargetType.Items.AddRange(new object[] {
            "Object",
            "Camera",
            "Sink",
            "FlipEffect",
            "FlipOn",
            "FlipOff",
            "Target",
            "FlipMap",
            "FinishLevel",
            "Secret",
            "Variable",
            "PlayAudio",
            "FlyByCamera",
            "Cutscene",
            "        FMV = 14"});
            this.comboTargetType.Location = new System.Drawing.Point(89, 37);
            this.comboTargetType.Name = "comboTargetType";
            this.comboTargetType.Size = new System.Drawing.Size(159, 24);
            this.comboTargetType.TabIndex = 3;
            this.comboTargetType.SelectedIndexChanged += new System.EventHandler(this.comboTargetType_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label2.Location = new System.Drawing.Point(16, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "What:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label3.Location = new System.Drawing.Point(16, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Parameter:";
            // 
            // tbParameter
            // 
            this.tbParameter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbParameter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbParameter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbParameter.Location = new System.Drawing.Point(89, 64);
            this.tbParameter.Name = "tbParameter";
            this.tbParameter.Size = new System.Drawing.Size(100, 22);
            this.tbParameter.TabIndex = 5;
            // 
            // tbTimer
            // 
            this.tbTimer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.tbTimer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbTimer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tbTimer.Location = new System.Drawing.Point(89, 94);
            this.tbTimer.Name = "tbTimer";
            this.tbTimer.Size = new System.Drawing.Size(100, 22);
            this.tbTimer.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label4.Location = new System.Drawing.Point(16, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Timer:";
            // 
            // cbBit4
            // 
            this.cbBit4.AutoSize = true;
            this.cbBit4.Location = new System.Drawing.Point(248, 127);
            this.cbBit4.Name = "cbBit4";
            this.cbBit4.Size = new System.Drawing.Size(49, 17);
            this.cbBit4.TabIndex = 65;
            this.cbBit4.Text = "Bit 4";
            // 
            // cbBit3
            // 
            this.cbBit3.AutoSize = true;
            this.cbBit3.Location = new System.Drawing.Point(195, 127);
            this.cbBit3.Name = "cbBit3";
            this.cbBit3.Size = new System.Drawing.Size(49, 17);
            this.cbBit3.TabIndex = 64;
            this.cbBit3.Text = "Bit 3";
            // 
            // cbBit2
            // 
            this.cbBit2.AutoSize = true;
            this.cbBit2.Location = new System.Drawing.Point(142, 127);
            this.cbBit2.Name = "cbBit2";
            this.cbBit2.Size = new System.Drawing.Size(49, 17);
            this.cbBit2.TabIndex = 63;
            this.cbBit2.Text = "Bit 2";
            // 
            // cbBit1
            // 
            this.cbBit1.AutoSize = true;
            this.cbBit1.Location = new System.Drawing.Point(89, 127);
            this.cbBit1.Name = "cbBit1";
            this.cbBit1.Size = new System.Drawing.Size(49, 17);
            this.cbBit1.TabIndex = 62;
            this.cbBit1.Text = "Bit 1";
            // 
            // cbBit5
            // 
            this.cbBit5.AutoSize = true;
            this.cbBit5.Location = new System.Drawing.Point(301, 127);
            this.cbBit5.Name = "cbBit5";
            this.cbBit5.Size = new System.Drawing.Size(49, 17);
            this.cbBit5.TabIndex = 61;
            this.cbBit5.Text = "Bit 5";
            // 
            // cbOneShot
            // 
            this.cbOneShot.AutoSize = true;
            this.cbOneShot.Location = new System.Drawing.Point(89, 150);
            this.cbOneShot.Name = "cbOneShot";
            this.cbOneShot.Size = new System.Drawing.Size(75, 17);
            this.cbOneShot.TabIndex = 66;
            this.cbOneShot.Text = "One Shot";
            // 
            // butCancel
            // 
            this.butCancel.Location = new System.Drawing.Point(324, 188);
            this.butCancel.Name = "butCancel";
            this.butCancel.Padding = new System.Windows.Forms.Padding(5);
            this.butCancel.Size = new System.Drawing.Size(86, 23);
            this.butCancel.TabIndex = 68;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Location = new System.Drawing.Point(232, 188);
            this.butOK.Name = "butOK";
            this.butOK.Padding = new System.Windows.Forms.Padding(5);
            this.butOK.Size = new System.Drawing.Size(86, 23);
            this.butOK.TabIndex = 67;
            this.butOK.Text = "OK";
            this.butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // comboParameter
            // 
            this.comboParameter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboParameter.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboParameter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboParameter.ForeColor = System.Drawing.Color.White;
            this.comboParameter.FormattingEnabled = true;
            this.comboParameter.ItemHeight = 18;
            this.comboParameter.Location = new System.Drawing.Point(89, 64);
            this.comboParameter.Name = "comboParameter";
            this.comboParameter.Size = new System.Drawing.Size(535, 24);
            this.comboParameter.TabIndex = 69;
            // 
            // FormTrigger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 223);
            this.Controls.Add(this.comboParameter);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.cbOneShot);
            this.Controls.Add(this.cbBit4);
            this.Controls.Add(this.cbBit3);
            this.Controls.Add(this.cbBit2);
            this.Controls.Add(this.cbBit1);
            this.Controls.Add(this.cbBit5);
            this.Controls.Add(this.tbTimer);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbParameter);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboTargetType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboType);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormTrigger";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Trigger editor";
            this.Load += new System.EventHandler(this.FormTrigger_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkLabel label1;
        private DarkComboBox comboType;
        private DarkComboBox comboTargetType;
        private DarkLabel label2;
        private DarkLabel label3;
        private DarkTextBox tbParameter;
        private DarkTextBox tbTimer;
        private DarkLabel label4;
        private DarkCheckBox cbBit4;
        private DarkCheckBox cbBit3;
        private DarkCheckBox cbBit2;
        private DarkCheckBox cbBit1;
        private DarkCheckBox cbBit5;
        private DarkCheckBox cbOneShot;
        private DarkButton butCancel;
        private DarkButton butOK;
        private DarkComboBox comboParameter;
    }
}