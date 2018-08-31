using System.Windows.Forms;
using DarkUI.Controls;

namespace TombEditor.Forms
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
            this.label1 = new DarkUI.Controls.DarkLabel();
            this.label2 = new DarkUI.Controls.DarkLabel();
            this.labelTarget = new DarkUI.Controls.DarkLabel();
            this.labelTimer = new DarkUI.Controls.DarkLabel();
            this.cbBit4 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit3 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit2 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit1 = new DarkUI.Controls.DarkCheckBox();
            this.cbBit5 = new DarkUI.Controls.DarkCheckBox();
            this.cbOneShot = new DarkUI.Controls.DarkCheckBox();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.labelExtra = new DarkUI.Controls.DarkLabel();
            this.tbScript = new DarkUI.Controls.DarkTextBox();
            this.labelScript = new DarkUI.Controls.DarkLabel();
            this.butCopyToClipboard = new DarkUI.Controls.DarkButton();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.scriptExportPanel = new System.Windows.Forms.Panel();
            this.paramTriggerType = new TombLib.Controls.TriggerParameterControl();
            this.paramTargetType = new TombLib.Controls.TriggerParameterControl();
            this.paramExtra = new TombLib.Controls.TriggerParameterControl();
            this.paramTimer = new TombLib.Controls.TriggerParameterControl();
            this.paramTarget = new TombLib.Controls.TriggerParameterControl();
            this.cbRawMode = new DarkUI.Controls.DarkCheckBox();
            this.scriptExportPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label1.Location = new System.Drawing.Point(9, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Trigger Type:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.label2.Location = new System.Drawing.Point(9, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "What:";
            // 
            // labelTarget
            // 
            this.labelTarget.AutoSize = true;
            this.labelTarget.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelTarget.Location = new System.Drawing.Point(9, 68);
            this.labelTarget.Name = "labelTarget";
            this.labelTarget.Size = new System.Drawing.Size(57, 13);
            this.labelTarget.TabIndex = 4;
            this.labelTarget.Text = "(#) Param:";
            // 
            // labelTimer
            // 
            this.labelTimer.AutoSize = true;
            this.labelTimer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelTimer.Location = new System.Drawing.Point(9, 94);
            this.labelTimer.Name = "labelTimer";
            this.labelTimer.Size = new System.Drawing.Size(55, 13);
            this.labelTimer.TabIndex = 6;
            this.labelTimer.Text = "(&&) Timer:";
            // 
            // cbBit4
            // 
            this.cbBit4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbBit4.AutoSize = true;
            this.cbBit4.Location = new System.Drawing.Point(241, 148);
            this.cbBit4.Name = "cbBit4";
            this.cbBit4.Size = new System.Drawing.Size(49, 17);
            this.cbBit4.TabIndex = 9;
            this.cbBit4.Text = "Bit 4";
            // 
            // cbBit3
            // 
            this.cbBit3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbBit3.AutoSize = true;
            this.cbBit3.Location = new System.Drawing.Point(188, 148);
            this.cbBit3.Name = "cbBit3";
            this.cbBit3.Size = new System.Drawing.Size(49, 17);
            this.cbBit3.TabIndex = 8;
            this.cbBit3.Text = "Bit 3";
            // 
            // cbBit2
            // 
            this.cbBit2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbBit2.AutoSize = true;
            this.cbBit2.Location = new System.Drawing.Point(135, 148);
            this.cbBit2.Name = "cbBit2";
            this.cbBit2.Size = new System.Drawing.Size(49, 17);
            this.cbBit2.TabIndex = 7;
            this.cbBit2.Text = "Bit 2";
            // 
            // cbBit1
            // 
            this.cbBit1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbBit1.AutoSize = true;
            this.cbBit1.Location = new System.Drawing.Point(82, 148);
            this.cbBit1.Name = "cbBit1";
            this.cbBit1.Size = new System.Drawing.Size(49, 17);
            this.cbBit1.TabIndex = 6;
            this.cbBit1.Text = "Bit 1";
            // 
            // cbBit5
            // 
            this.cbBit5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbBit5.AutoSize = true;
            this.cbBit5.Location = new System.Drawing.Point(294, 148);
            this.cbBit5.Name = "cbBit5";
            this.cbBit5.Size = new System.Drawing.Size(49, 17);
            this.cbBit5.TabIndex = 10;
            this.cbBit5.Text = "Bit 5";
            // 
            // cbOneShot
            // 
            this.cbOneShot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbOneShot.AutoSize = true;
            this.cbOneShot.Location = new System.Drawing.Point(82, 170);
            this.cbOneShot.Name = "cbOneShot";
            this.cbOneShot.Size = new System.Drawing.Size(75, 17);
            this.cbOneShot.TabIndex = 11;
            this.cbOneShot.Text = "One Shot";
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(589, 198);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 16;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Location = new System.Drawing.Point(503, 198);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 15;
            this.butOK.Text = "OK";
            this.butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // labelExtra
            // 
            this.labelExtra.AutoSize = true;
            this.labelExtra.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelExtra.Location = new System.Drawing.Point(9, 120);
            this.labelExtra.Name = "labelExtra";
            this.labelExtra.Size = new System.Drawing.Size(50, 13);
            this.labelExtra.TabIndex = 71;
            this.labelExtra.Text = "(E) Extra:";
            // 
            // tbScript
            // 
            this.tbScript.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbScript.Location = new System.Drawing.Point(42, 0);
            this.tbScript.Name = "tbScript";
            this.tbScript.ReadOnly = true;
            this.tbScript.Size = new System.Drawing.Size(181, 22);
            this.tbScript.TabIndex = 13;
            // 
            // labelScript
            // 
            this.labelScript.AutoSize = true;
            this.labelScript.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelScript.Location = new System.Drawing.Point(3, 2);
            this.labelScript.Name = "labelScript";
            this.labelScript.Size = new System.Drawing.Size(39, 13);
            this.labelScript.TabIndex = 75;
            this.labelScript.Text = "Script:";
            // 
            // butCopyToClipboard
            // 
            this.butCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butCopyToClipboard.Image = global::TombEditor.Properties.Resources.general_copy_16;
            this.butCopyToClipboard.Location = new System.Drawing.Point(227, 0);
            this.butCopyToClipboard.Name = "butCopyToClipboard";
            this.butCopyToClipboard.Size = new System.Drawing.Size(22, 22);
            this.butCopyToClipboard.TabIndex = 12;
            this.toolTip.SetToolTip(this.butCopyToClipboard, "Copy to clipboard");
            this.butCopyToClipboard.Click += new System.EventHandler(this.butCopyToClipboard_Click);
            // 
            // scriptExportPanel
            // 
            this.scriptExportPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.scriptExportPanel.Controls.Add(this.tbScript);
            this.scriptExportPanel.Controls.Add(this.labelScript);
            this.scriptExportPanel.Controls.Add(this.butCopyToClipboard);
            this.scriptExportPanel.Location = new System.Drawing.Point(420, 148);
            this.scriptExportPanel.Name = "scriptExportPanel";
            this.scriptExportPanel.Size = new System.Drawing.Size(249, 25);
            this.scriptExportPanel.TabIndex = 78;
            // 
            // paramTriggerType
            // 
            this.paramTriggerType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.paramTriggerType.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.paramTriggerType.Level = null;
            this.paramTriggerType.Location = new System.Drawing.Point(82, 12);
            this.paramTriggerType.Name = "paramTriggerType";
            this.paramTriggerType.Size = new System.Drawing.Size(587, 23);
            this.paramTriggerType.TabIndex = 1;
            this.paramTriggerType.ParameterChanged += new System.EventHandler(this.paramTriggerType_ParameterChanged);
            // 
            // paramTargetType
            // 
            this.paramTargetType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.paramTargetType.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.paramTargetType.Level = null;
            this.paramTargetType.Location = new System.Drawing.Point(82, 38);
            this.paramTargetType.Name = "paramTargetType";
            this.paramTargetType.Size = new System.Drawing.Size(587, 23);
            this.paramTargetType.TabIndex = 2;
            this.paramTargetType.ParameterChanged += new System.EventHandler(this.paramTargetType_ParameterChanged);
            // 
            // paramExtra
            // 
            this.paramExtra.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.paramExtra.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.paramExtra.Level = null;
            this.paramExtra.Location = new System.Drawing.Point(82, 116);
            this.paramExtra.Name = "paramExtra";
            this.paramExtra.Size = new System.Drawing.Size(587, 23);
            this.paramExtra.TabIndex = 5;
            this.paramExtra.ParameterChanged += new System.EventHandler(this.paramExtra_ParameterChanged);
            // 
            // paramTimer
            // 
            this.paramTimer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.paramTimer.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.paramTimer.Level = null;
            this.paramTimer.Location = new System.Drawing.Point(82, 90);
            this.paramTimer.Name = "paramTimer";
            this.paramTimer.Size = new System.Drawing.Size(587, 23);
            this.paramTimer.TabIndex = 4;
            this.paramTimer.ParameterChanged += new System.EventHandler(this.paramTimer_ParameterChanged);
            // 
            // paramTarget
            // 
            this.paramTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.paramTarget.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.paramTarget.Level = null;
            this.paramTarget.Location = new System.Drawing.Point(82, 64);
            this.paramTarget.Name = "paramTarget";
            this.paramTarget.Size = new System.Drawing.Size(587, 23);
            this.paramTarget.TabIndex = 3;
            this.paramTarget.ParameterChanged += new System.EventHandler(this.paramTarget_ParameterChanged);
            // 
            // cbRawMode
            // 
            this.cbRawMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbRawMode.AutoSize = true;
            this.cbRawMode.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbRawMode.Location = new System.Drawing.Point(474, 176);
            this.cbRawMode.Name = "cbRawMode";
            this.cbRawMode.Size = new System.Drawing.Size(196, 17);
            this.cbRawMode.TabIndex = 14;
            this.cbRawMode.Text = "Raw mode (show numeric values)";
            this.cbRawMode.CheckedChanged += new System.EventHandler(this.cbRawMode_CheckedChanged);
            // 
            // FormTrigger
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(678, 227);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.paramTriggerType);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.paramTargetType);
            this.Controls.Add(this.scriptExportPanel);
            this.Controls.Add(this.paramExtra);
            this.Controls.Add(this.paramTimer);
            this.Controls.Add(this.paramTarget);
            this.Controls.Add(this.labelExtra);
            this.Controls.Add(this.cbRawMode);
            this.Controls.Add(this.cbOneShot);
            this.Controls.Add(this.cbBit4);
            this.Controls.Add(this.cbBit3);
            this.Controls.Add(this.cbBit2);
            this.Controls.Add(this.cbBit1);
            this.Controls.Add(this.cbBit5);
            this.Controls.Add(this.labelTimer);
            this.Controls.Add(this.labelTarget);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.MinimizeBox = false;
            this.Name = "FormTrigger";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Trigger editor";
            this.scriptExportPanel.ResumeLayout(false);
            this.scriptExportPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkLabel label1;
        private DarkLabel label2;
        private DarkLabel labelTarget;
        private DarkLabel labelTimer;
        private DarkCheckBox cbBit4;
        private DarkCheckBox cbBit3;
        private DarkCheckBox cbBit2;
        private DarkCheckBox cbBit1;
        private DarkCheckBox cbBit5;
        private DarkCheckBox cbOneShot;
        private DarkButton butCancel;
        private DarkButton butOK;
        private DarkLabel labelExtra;
        private DarkTextBox tbScript;
        private DarkLabel labelScript;
        private DarkButton butCopyToClipboard;
        private ToolTip toolTip;
        private TombLib.Controls.TriggerParameterControl paramTarget;
        private TombLib.Controls.TriggerParameterControl paramTimer;
        private TombLib.Controls.TriggerParameterControl paramExtra;
        private Panel scriptExportPanel;
        private TombLib.Controls.TriggerParameterControl paramTargetType;
        private TombLib.Controls.TriggerParameterControl paramTriggerType;
        private DarkCheckBox cbRawMode;
    }
}