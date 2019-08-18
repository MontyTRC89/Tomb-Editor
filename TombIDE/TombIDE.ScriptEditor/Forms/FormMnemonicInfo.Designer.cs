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
			this.label_FlagName = new DarkUI.Controls.DarkLabel();
			this.label_Paolone = new DarkUI.Controls.DarkLabel();
			this.panel_01 = new System.Windows.Forms.Panel();
			this.panel_02 = new System.Windows.Forms.Panel();
			this.richTextBox_Description = new System.Windows.Forms.RichTextBox();
			this.panel_01.SuspendLayout();
			this.panel_02.SuspendLayout();
			this.SuspendLayout();
			// 
			// label_FlagName
			// 
			this.label_FlagName.Dock = System.Windows.Forms.DockStyle.Top;
			this.label_FlagName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_FlagName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_FlagName.Location = new System.Drawing.Point(0, 0);
			this.label_FlagName.Name = "label_FlagName";
			this.label_FlagName.Size = new System.Drawing.Size(622, 32);
			this.label_FlagName.TabIndex = 0;
			this.label_FlagName.Text = "FLAG_NAME_GOES_HERE";
			this.label_FlagName.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// label_Paolone
			// 
			this.label_Paolone.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.label_Paolone.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Paolone.ForeColor = System.Drawing.Color.Gray;
			this.label_Paolone.Location = new System.Drawing.Point(0, 34);
			this.label_Paolone.Name = "label_Paolone";
			this.label_Paolone.Size = new System.Drawing.Size(622, 20);
			this.label_Paolone.TabIndex = 1;
			this.label_Paolone.Text = "(According to Paolone)";
			this.label_Paolone.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// panel_01
			// 
			this.panel_01.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_01.Controls.Add(this.label_Paolone);
			this.panel_01.Controls.Add(this.label_FlagName);
			this.panel_01.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel_01.Location = new System.Drawing.Point(0, 0);
			this.panel_01.Name = "panel_01";
			this.panel_01.Size = new System.Drawing.Size(624, 56);
			this.panel_01.TabIndex = 0;
			// 
			// panel_02
			// 
			this.panel_02.Controls.Add(this.richTextBox_Description);
			this.panel_02.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_02.Location = new System.Drawing.Point(0, 56);
			this.panel_02.Name = "panel_02";
			this.panel_02.Size = new System.Drawing.Size(624, 385);
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
			this.richTextBox_Description.Size = new System.Drawing.Size(624, 385);
			this.richTextBox_Description.TabIndex = 1;
			this.richTextBox_Description.Text = "";
			// 
			// FormMnemonicInfo
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(624, 441);
			this.Controls.Add(this.panel_02);
			this.Controls.Add(this.panel_01);
			this.Name = "FormMnemonicInfo";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Information about FLAG_NAME_GOES_HERE";
			this.panel_01.ResumeLayout(false);
			this.panel_02.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkLabel label_FlagName;
		private DarkUI.Controls.DarkLabel label_Paolone;
		private System.Windows.Forms.Panel panel_01;
		private System.Windows.Forms.Panel panel_02;
		private System.Windows.Forms.RichTextBox richTextBox_Description;
	}
}