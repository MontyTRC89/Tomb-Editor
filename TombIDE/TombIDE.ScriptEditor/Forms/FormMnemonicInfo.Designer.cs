namespace TombIDE.ScriptEditor
{
	partial class FormMnemonicInfo
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.button_Close = new DarkUI.Controls.DarkButton();
			this.label_FlagName = new DarkUI.Controls.DarkLabel();
			this.panel_01 = new System.Windows.Forms.Panel();
			this.panel_02 = new System.Windows.Forms.Panel();
			this.richTextBox_Description = new System.Windows.Forms.RichTextBox();
			this.panel_03 = new System.Windows.Forms.Panel();
			this.panel_01.SuspendLayout();
			this.panel_02.SuspendLayout();
			this.panel_03.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Close
			// 
			this.button_Close.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Close.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Close.Location = new System.Drawing.Point(11, 11);
			this.button_Close.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
			this.button_Close.Name = "button_Close";
			this.button_Close.Size = new System.Drawing.Size(760, 24);
			this.button_Close.TabIndex = 0;
			this.button_Close.Text = "Close";
			// 
			// label_FlagName
			// 
			this.label_FlagName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_FlagName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_FlagName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_FlagName.Location = new System.Drawing.Point(0, 0);
			this.label_FlagName.Name = "label_FlagName";
			this.label_FlagName.Size = new System.Drawing.Size(782, 46);
			this.label_FlagName.TabIndex = 0;
			this.label_FlagName.Text = "FLAG_NAME_GOES_HERE";
			this.label_FlagName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panel_01
			// 
			this.panel_01.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_01.Controls.Add(this.label_FlagName);
			this.panel_01.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel_01.Location = new System.Drawing.Point(0, 0);
			this.panel_01.Name = "panel_01";
			this.panel_01.Size = new System.Drawing.Size(784, 48);
			this.panel_01.TabIndex = 0;
			// 
			// panel_02
			// 
			this.panel_02.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_02.Controls.Add(this.richTextBox_Description);
			this.panel_02.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_02.Location = new System.Drawing.Point(0, 48);
			this.panel_02.Name = "panel_02";
			this.panel_02.Size = new System.Drawing.Size(784, 465);
			this.panel_02.TabIndex = 1;
			// 
			// richTextBox_Description
			// 
			this.richTextBox_Description.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.richTextBox_Description.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox_Description.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox_Description.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.richTextBox_Description.ForeColor = System.Drawing.Color.Gainsboro;
			this.richTextBox_Description.Location = new System.Drawing.Point(0, 0);
			this.richTextBox_Description.Name = "richTextBox_Description";
			this.richTextBox_Description.ReadOnly = true;
			this.richTextBox_Description.Size = new System.Drawing.Size(782, 463);
			this.richTextBox_Description.TabIndex = 0;
			this.richTextBox_Description.Text = "";
			// 
			// panel_03
			// 
			this.panel_03.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_03.Controls.Add(this.button_Close);
			this.panel_03.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel_03.Location = new System.Drawing.Point(0, 513);
			this.panel_03.Name = "panel_03";
			this.panel_03.Size = new System.Drawing.Size(784, 48);
			this.panel_03.TabIndex = 2;
			// 
			// FormMnemonicInfo
			// 
			this.AcceptButton = this.button_Close;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 561);
			this.Controls.Add(this.panel_02);
			this.Controls.Add(this.panel_03);
			this.Controls.Add(this.panel_01);
			this.MinimumSize = new System.Drawing.Size(640, 480);
			this.Name = "FormMnemonicInfo";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Information about FLAG_NAME_GOES_HERE";
			this.panel_01.ResumeLayout(false);
			this.panel_02.ResumeLayout(false);
			this.panel_03.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Close;
		private DarkUI.Controls.DarkLabel label_FlagName;
		private System.Windows.Forms.Panel panel_01;
		private System.Windows.Forms.Panel panel_02;
		private System.Windows.Forms.Panel panel_03;
		private System.Windows.Forms.RichTextBox richTextBox_Description;
	}
}