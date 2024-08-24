using System.Windows.Forms;
using DarkUI.Controls;
using TombLib.Controls;

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
			components = new System.ComponentModel.Container();
			label1 = new DarkLabel();
			label2 = new DarkLabel();
			labelTarget = new DarkLabel();
			labelTimer = new DarkLabel();
			cbBit4 = new DarkCheckBox();
			cbBit3 = new DarkCheckBox();
			cbBit2 = new DarkCheckBox();
			cbBit1 = new DarkCheckBox();
			cbBit5 = new DarkCheckBox();
			cbOneShot = new DarkCheckBox();
			butCancel = new DarkButton();
			butOK = new DarkButton();
			labelExtra = new DarkLabel();
			tbScript = new DarkTextBox();
			labelScript = new DarkLabel();
			toolTip = new ToolTip(components);
			butCopyWithComments = new DarkButton();
			butCopyToClipboard = new DarkButton();
			butSearchTrigger = new DarkButton();
			butCopyAsAnimcommand = new DarkButton();
			scriptExportPanel = new Panel();
			cbRawMode = new DarkCheckBox();
			panelMain = new Panel();
			panelButtons = new Panel();
			panelClassicTriggerControls = new Panel();
			tableLayoutPanel = new TableLayoutPanel();
			paramPlugin = new TriggerParameterControl();
			labelPlugin = new DarkLabel();
			paramExtra = new TriggerParameterControl();
			paramTriggerType = new TriggerParameterControl();
			paramTargetType = new TriggerParameterControl();
			paramTimer = new TriggerParameterControl();
			paramTarget = new TriggerParameterControl();
			scriptExportPanel.SuspendLayout();
			panelMain.SuspendLayout();
			panelButtons.SuspendLayout();
			panelClassicTriggerControls.SuspendLayout();
			tableLayoutPanel.SuspendLayout();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Dock = DockStyle.Fill;
			label1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label1.Location = new System.Drawing.Point(3, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(74, 26);
			label1.TabIndex = 0;
			label1.Text = "Trigger Type:";
			label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Dock = DockStyle.Fill;
			label2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label2.Location = new System.Drawing.Point(3, 26);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(74, 26);
			label2.TabIndex = 2;
			label2.Text = "What:";
			label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelTarget
			// 
			labelTarget.AutoSize = true;
			labelTarget.Dock = DockStyle.Fill;
			labelTarget.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			labelTarget.Location = new System.Drawing.Point(3, 52);
			labelTarget.Name = "labelTarget";
			labelTarget.Size = new System.Drawing.Size(74, 26);
			labelTarget.TabIndex = 4;
			labelTarget.Text = "(#) Param:";
			labelTarget.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelTimer
			// 
			labelTimer.AutoSize = true;
			labelTimer.Dock = DockStyle.Fill;
			labelTimer.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			labelTimer.Location = new System.Drawing.Point(3, 78);
			labelTimer.Name = "labelTimer";
			labelTimer.Size = new System.Drawing.Size(74, 26);
			labelTimer.TabIndex = 6;
			labelTimer.Text = "(&&) Timer:";
			labelTimer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// cbBit4
			// 
			cbBit4.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			cbBit4.AutoSize = true;
			cbBit4.Location = new System.Drawing.Point(233, 133);
			cbBit4.Name = "cbBit4";
			cbBit4.Size = new System.Drawing.Size(44, 22);
			cbBit4.TabIndex = 9;
			cbBit4.Text = "Bit 4";
			// 
			// cbBit3
			// 
			cbBit3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			cbBit3.AutoSize = true;
			cbBit3.Location = new System.Drawing.Point(183, 133);
			cbBit3.Name = "cbBit3";
			cbBit3.Size = new System.Drawing.Size(44, 22);
			cbBit3.TabIndex = 8;
			cbBit3.Text = "Bit 3";
			// 
			// cbBit2
			// 
			cbBit2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			cbBit2.AutoSize = true;
			cbBit2.Location = new System.Drawing.Point(133, 133);
			cbBit2.Name = "cbBit2";
			cbBit2.Size = new System.Drawing.Size(44, 22);
			cbBit2.TabIndex = 7;
			cbBit2.Text = "Bit 2";
			// 
			// cbBit1
			// 
			cbBit1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			cbBit1.AutoSize = true;
			cbBit1.Location = new System.Drawing.Point(83, 133);
			cbBit1.Name = "cbBit1";
			cbBit1.Size = new System.Drawing.Size(44, 22);
			cbBit1.TabIndex = 6;
			cbBit1.Text = "Bit 1";
			// 
			// cbBit5
			// 
			cbBit5.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			cbBit5.AutoSize = true;
			cbBit5.Location = new System.Drawing.Point(283, 133);
			cbBit5.Name = "cbBit5";
			cbBit5.Size = new System.Drawing.Size(44, 22);
			cbBit5.TabIndex = 10;
			cbBit5.Text = "Bit 5";
			// 
			// cbOneShot
			// 
			cbOneShot.AutoSize = true;
			tableLayoutPanel.SetColumnSpan(cbOneShot, 5);
			cbOneShot.Location = new System.Drawing.Point(83, 161);
			cbOneShot.Name = "cbOneShot";
			cbOneShot.Size = new System.Drawing.Size(75, 17);
			cbOneShot.TabIndex = 11;
			cbOneShot.Text = "One Shot";
			// 
			// butCancel
			// 
			butCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			butCancel.Checked = false;
			butCancel.DialogResult = DialogResult.Cancel;
			butCancel.Location = new System.Drawing.Point(615, 4);
			butCancel.Name = "butCancel";
			butCancel.Size = new System.Drawing.Size(80, 23);
			butCancel.TabIndex = 16;
			butCancel.Text = "Cancel";
			butCancel.TextImageRelation = TextImageRelation.ImageBeforeText;
			butCancel.Click += butCancel_Click;
			// 
			// butOK
			// 
			butOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			butOK.Checked = false;
			butOK.Location = new System.Drawing.Point(529, 4);
			butOK.Name = "butOK";
			butOK.Size = new System.Drawing.Size(80, 23);
			butOK.TabIndex = 15;
			butOK.Text = "OK";
			butOK.TextImageRelation = TextImageRelation.ImageBeforeText;
			butOK.Click += butOK_Click;
			// 
			// labelExtra
			// 
			labelExtra.AutoSize = true;
			labelExtra.Dock = DockStyle.Fill;
			labelExtra.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			labelExtra.Location = new System.Drawing.Point(3, 104);
			labelExtra.Name = "labelExtra";
			labelExtra.Size = new System.Drawing.Size(74, 26);
			labelExtra.TabIndex = 71;
			labelExtra.Text = "(E) Extra:";
			labelExtra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tbScript
			// 
			tbScript.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			tbScript.Location = new System.Drawing.Point(45, 0);
			tbScript.Name = "tbScript";
			tbScript.ReadOnly = true;
			tbScript.Size = new System.Drawing.Size(158, 22);
			tbScript.TabIndex = 13;
			// 
			// labelScript
			// 
			labelScript.AutoSize = true;
			labelScript.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			labelScript.Location = new System.Drawing.Point(3, 2);
			labelScript.Name = "labelScript";
			labelScript.Size = new System.Drawing.Size(39, 13);
			labelScript.TabIndex = 75;
			labelScript.Text = "Script:";
			// 
			// butCopyWithComments
			// 
			butCopyWithComments.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			butCopyWithComments.Checked = false;
			butCopyWithComments.Image = Properties.Resources.general_copy_comments_16;
			butCopyWithComments.Location = new System.Drawing.Point(237, 0);
			butCopyWithComments.Name = "butCopyWithComments";
			butCopyWithComments.Size = new System.Drawing.Size(22, 22);
			butCopyWithComments.TabIndex = 76;
			toolTip.SetToolTip(butCopyWithComments, "Copy to clipboard with comments");
			butCopyWithComments.Click += butCopyWithComments_Click;
			// 
			// butCopyToClipboard
			// 
			butCopyToClipboard.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			butCopyToClipboard.Checked = false;
			butCopyToClipboard.Image = Properties.Resources.general_copy_16;
			butCopyToClipboard.Location = new System.Drawing.Point(209, 0);
			butCopyToClipboard.Name = "butCopyToClipboard";
			butCopyToClipboard.Size = new System.Drawing.Size(22, 22);
			butCopyToClipboard.TabIndex = 12;
			toolTip.SetToolTip(butCopyToClipboard, "Copy to clipboard");
			butCopyToClipboard.Click += butCopyToClipboard_Click;
			// 
			// butSearchTrigger
			// 
			butSearchTrigger.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			butSearchTrigger.Checked = false;
			butSearchTrigger.Image = Properties.Resources.general_Import_16;
			butSearchTrigger.Location = new System.Drawing.Point(265, 0);
			butSearchTrigger.Name = "butSearchTrigger";
			butSearchTrigger.Size = new System.Drawing.Size(22, 22);
			butSearchTrigger.TabIndex = 77;
			toolTip.SetToolTip(butSearchTrigger, "Import trigger");
			butSearchTrigger.Click += butSearchTrigger_Click;
			// 
			// butCopyAsAnimcommand
			// 
			butCopyAsAnimcommand.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			butCopyAsAnimcommand.Checked = false;
			butCopyAsAnimcommand.Image = Properties.Resources.general_animcommand_16;
			butCopyAsAnimcommand.Location = new System.Drawing.Point(293, 0);
			butCopyAsAnimcommand.Name = "butCopyAsAnimcommand";
			butCopyAsAnimcommand.Size = new System.Drawing.Size(22, 22);
			butCopyAsAnimcommand.TabIndex = 78;
			toolTip.SetToolTip(butCopyAsAnimcommand, "Export as animcommand");
			butCopyAsAnimcommand.Click += butCopyAsAnimcommand_Click;
			// 
			// scriptExportPanel
			// 
			scriptExportPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			tableLayoutPanel.SetColumnSpan(scriptExportPanel, 2);
			scriptExportPanel.Controls.Add(butCopyAsAnimcommand);
			scriptExportPanel.Controls.Add(butSearchTrigger);
			scriptExportPanel.Controls.Add(butCopyWithComments);
			scriptExportPanel.Controls.Add(tbScript);
			scriptExportPanel.Controls.Add(labelScript);
			scriptExportPanel.Controls.Add(butCopyToClipboard);
			scriptExportPanel.Location = new System.Drawing.Point(380, 133);
			scriptExportPanel.Name = "scriptExportPanel";
			scriptExportPanel.Size = new System.Drawing.Size(315, 22);
			scriptExportPanel.TabIndex = 78;
			// 
			// cbRawMode
			// 
			cbRawMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			cbRawMode.AutoSize = true;
			cbRawMode.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			cbRawMode.Location = new System.Drawing.Point(499, 161);
			cbRawMode.Name = "cbRawMode";
			cbRawMode.Size = new System.Drawing.Size(196, 17);
			cbRawMode.TabIndex = 14;
			cbRawMode.Text = "Raw mode (show numeric values)";
			cbRawMode.CheckedChanged += cbRawMode_CheckedChanged;
			// 
			// panelMain
			// 
			panelMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			panelMain.Controls.Add(panelButtons);
			panelMain.Controls.Add(panelClassicTriggerControls);
			panelMain.Dock = DockStyle.Fill;
			panelMain.Location = new System.Drawing.Point(0, 0);
			panelMain.MinimumSize = new System.Drawing.Size(600, 0);
			panelMain.Name = "panelMain";
			panelMain.Padding = new Padding(3, 5, 3, 3);
			panelMain.Size = new System.Drawing.Size(704, 223);
			panelMain.TabIndex = 82;
			// 
			// panelButtons
			// 
			panelButtons.Controls.Add(butOK);
			panelButtons.Controls.Add(butCancel);
			panelButtons.Dock = DockStyle.Bottom;
			panelButtons.Location = new System.Drawing.Point(3, 189);
			panelButtons.Name = "panelButtons";
			panelButtons.Size = new System.Drawing.Size(698, 31);
			panelButtons.TabIndex = 85;
			// 
			// panelClassicTriggerControls
			// 
			panelClassicTriggerControls.Controls.Add(tableLayoutPanel);
			panelClassicTriggerControls.Dock = DockStyle.Top;
			panelClassicTriggerControls.Location = new System.Drawing.Point(3, 5);
			panelClassicTriggerControls.Name = "panelClassicTriggerControls";
			panelClassicTriggerControls.Size = new System.Drawing.Size(698, 181);
			panelClassicTriggerControls.TabIndex = 82;
			// 
			// tableLayoutPanel
			// 
			tableLayoutPanel.ColumnCount = 8;
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95F));
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			tableLayoutPanel.Controls.Add(paramPlugin, 7, 0);
			tableLayoutPanel.Controls.Add(labelPlugin, 6, 0);
			tableLayoutPanel.Controls.Add(label1, 0, 0);
			tableLayoutPanel.Controls.Add(cbRawMode, 7, 6);
			tableLayoutPanel.Controls.Add(label2, 0, 1);
			tableLayoutPanel.Controls.Add(paramExtra, 1, 4);
			tableLayoutPanel.Controls.Add(cbOneShot, 1, 6);
			tableLayoutPanel.Controls.Add(labelTarget, 0, 2);
			tableLayoutPanel.Controls.Add(cbBit5, 5, 5);
			tableLayoutPanel.Controls.Add(cbBit4, 4, 5);
			tableLayoutPanel.Controls.Add(labelTimer, 0, 3);
			tableLayoutPanel.Controls.Add(cbBit3, 3, 5);
			tableLayoutPanel.Controls.Add(paramTriggerType, 1, 0);
			tableLayoutPanel.Controls.Add(cbBit2, 2, 5);
			tableLayoutPanel.Controls.Add(paramTargetType, 1, 1);
			tableLayoutPanel.Controls.Add(cbBit1, 1, 5);
			tableLayoutPanel.Controls.Add(paramTimer, 1, 3);
			tableLayoutPanel.Controls.Add(labelExtra, 0, 4);
			tableLayoutPanel.Controls.Add(paramTarget, 1, 2);
			tableLayoutPanel.Controls.Add(scriptExportPanel, 6, 5);
			tableLayoutPanel.Dock = DockStyle.Fill;
			tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			tableLayoutPanel.Margin = new Padding(0);
			tableLayoutPanel.Name = "tableLayoutPanel";
			tableLayoutPanel.RowCount = 8;
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.Size = new System.Drawing.Size(698, 181);
			tableLayoutPanel.TabIndex = 79;
			// 
			// paramPlugin
			// 
			paramPlugin.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			paramPlugin.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			paramPlugin.Level = null;
			paramPlugin.Location = new System.Drawing.Point(428, 3);
			paramPlugin.Name = "paramPlugin";
			paramPlugin.Size = new System.Drawing.Size(267, 23);
			paramPlugin.TabIndex = 80;
			paramPlugin.ParameterChanged += OnParameterChanged;
			// 
			// labelPlugin
			// 
			labelPlugin.AutoSize = true;
			labelPlugin.Dock = DockStyle.Fill;
			labelPlugin.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			labelPlugin.Location = new System.Drawing.Point(333, 0);
			labelPlugin.Name = "labelPlugin";
			labelPlugin.Size = new System.Drawing.Size(89, 26);
			labelPlugin.TabIndex = 79;
			labelPlugin.Text = "Plugin / Engine:";
			labelPlugin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// paramExtra
			// 
			paramExtra.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			tableLayoutPanel.SetColumnSpan(paramExtra, 7);
			paramExtra.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			paramExtra.Level = null;
			paramExtra.Location = new System.Drawing.Point(83, 107);
			paramExtra.Name = "paramExtra";
			paramExtra.Size = new System.Drawing.Size(612, 23);
			paramExtra.TabIndex = 72;
			paramExtra.ParameterChanged += OnParameterChanged;
			// 
			// paramTriggerType
			// 
			paramTriggerType.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			tableLayoutPanel.SetColumnSpan(paramTriggerType, 5);
			paramTriggerType.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			paramTriggerType.Level = null;
			paramTriggerType.Location = new System.Drawing.Point(83, 3);
			paramTriggerType.Name = "paramTriggerType";
			paramTriggerType.Size = new System.Drawing.Size(244, 23);
			paramTriggerType.TabIndex = 1;
			paramTriggerType.ParameterChanged += OnParameterChanged;
			// 
			// paramTargetType
			// 
			paramTargetType.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			tableLayoutPanel.SetColumnSpan(paramTargetType, 7);
			paramTargetType.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			paramTargetType.Level = null;
			paramTargetType.Location = new System.Drawing.Point(83, 29);
			paramTargetType.Name = "paramTargetType";
			paramTargetType.Size = new System.Drawing.Size(612, 23);
			paramTargetType.TabIndex = 2;
			paramTargetType.ParameterChanged += OnParameterChanged;
			// 
			// paramTimer
			// 
			paramTimer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			tableLayoutPanel.SetColumnSpan(paramTimer, 7);
			paramTimer.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			paramTimer.Level = null;
			paramTimer.Location = new System.Drawing.Point(83, 81);
			paramTimer.Name = "paramTimer";
			paramTimer.Size = new System.Drawing.Size(612, 23);
			paramTimer.TabIndex = 4;
			paramTimer.ParameterChanged += OnParameterChanged;
			// 
			// paramTarget
			// 
			paramTarget.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			tableLayoutPanel.SetColumnSpan(paramTarget, 7);
			paramTarget.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			paramTarget.Level = null;
			paramTarget.Location = new System.Drawing.Point(83, 55);
			paramTarget.Name = "paramTarget";
			paramTarget.Size = new System.Drawing.Size(612, 23);
			paramTarget.TabIndex = 3;
			paramTarget.ParameterChanged += OnParameterChanged;
			// 
			// FormTrigger
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = butCancel;
			ClientSize = new System.Drawing.Size(704, 223);
			Controls.Add(panelMain);
			DoubleBuffered = true;
			MinimizeBox = false;
			Name = "FormTrigger";
			ShowIcon = false;
			ShowInTaskbar = false;
			SizeGripStyle = SizeGripStyle.Hide;
			StartPosition = FormStartPosition.CenterParent;
			scriptExportPanel.ResumeLayout(false);
			scriptExportPanel.PerformLayout();
			panelMain.ResumeLayout(false);
			panelButtons.ResumeLayout(false);
			panelClassicTriggerControls.ResumeLayout(false);
			tableLayoutPanel.ResumeLayout(false);
			tableLayoutPanel.PerformLayout();
			ResumeLayout(false);
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
        private Panel scriptExportPanel;
        private TombLib.Controls.TriggerParameterControl paramTargetType;
        private TombLib.Controls.TriggerParameterControl paramTriggerType;
        private DarkCheckBox cbRawMode;
        private DarkButton butCopyWithComments;
        private Panel panelMain;
        private Panel panelButtons;
        private Panel panelClassicTriggerControls;
        private TriggerParameterControl paramExtra;
        private DarkButton butSearchTrigger;
        private DarkButton butCopyAsAnimcommand;
		private TableLayoutPanel tableLayoutPanel;
		private DarkLabel labelPlugin;
		private TriggerParameterControl paramPlugin;
	}
}