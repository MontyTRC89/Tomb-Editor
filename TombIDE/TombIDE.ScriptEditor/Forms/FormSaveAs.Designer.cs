namespace TombIDE
{
	partial class FormSaveAs
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
			this.button_Cancel = new DarkUI.Controls.DarkButton();
			this.button_Save = new DarkUI.Controls.DarkButton();
			this.comboBox_FileFormat = new DarkUI.Controls.DarkComboBox();
			this.label = new DarkUI.Controls.DarkLabel();
			this.label_Format = new DarkUI.Controls.DarkLabel();
			this.panel_01 = new System.Windows.Forms.Panel();
			this.textBox_NewFileName = new DarkUI.Controls.DarkTextBox();
			this.panel_02 = new System.Windows.Forms.Panel();
			this.panel_01.SuspendLayout();
			this.panel_02.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Cancel
			// 
			this.button_Cancel.Checked = false;
			this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_Cancel.Location = new System.Drawing.Point(379, 8);
			this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 9, 0, 0);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.Size = new System.Drawing.Size(75, 23);
			this.button_Cancel.TabIndex = 2;
			this.button_Cancel.Text = "Cancel";
			// 
			// button_Save
			// 
			this.button_Save.Checked = false;
			this.button_Save.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Save.Location = new System.Drawing.Point(301, 8);
			this.button_Save.Margin = new System.Windows.Forms.Padding(3, 9, 0, 0);
			this.button_Save.Name = "button_Save";
			this.button_Save.Size = new System.Drawing.Size(75, 23);
			this.button_Save.TabIndex = 1;
			this.button_Save.Text = "Save";
			this.button_Save.Click += new System.EventHandler(this.button_Save_Click);
			// 
			// comboBox_FileFormat
			// 
			this.comboBox_FileFormat.FormattingEnabled = true;
			this.comboBox_FileFormat.Items.AddRange(new object[] {
            ".TXT (Text file)",
            ".LUA (LUA Script File)"});
			this.comboBox_FileFormat.Location = new System.Drawing.Point(64, 54);
			this.comboBox_FileFormat.Margin = new System.Windows.Forms.Padding(0, 3, 3, 6);
			this.comboBox_FileFormat.Name = "comboBox_FileFormat";
			this.comboBox_FileFormat.Size = new System.Drawing.Size(392, 21);
			this.comboBox_FileFormat.TabIndex = 2;
			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label.Location = new System.Drawing.Point(3, 3);
			this.label.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(82, 13);
			this.label.TabIndex = 0;
			this.label.Text = "New File Name:";
			// 
			// label_Format
			// 
			this.label_Format.AutoSize = true;
			this.label_Format.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Format.Location = new System.Drawing.Point(3, 57);
			this.label_Format.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.label_Format.Name = "label_Format";
			this.label_Format.Size = new System.Drawing.Size(61, 13);
			this.label_Format.TabIndex = 3;
			this.label_Format.Text = "File Format:";
			// 
			// panel_01
			// 
			this.panel_01.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_01.Controls.Add(this.label_Format);
			this.panel_01.Controls.Add(this.comboBox_FileFormat);
			this.panel_01.Controls.Add(this.textBox_NewFileName);
			this.panel_01.Controls.Add(this.label);
			this.panel_01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_01.Location = new System.Drawing.Point(0, 0);
			this.panel_01.Margin = new System.Windows.Forms.Padding(0);
			this.panel_01.Name = "panel_01";
			this.panel_01.Size = new System.Drawing.Size(464, 85);
			this.panel_01.TabIndex = 0;
			// 
			// textBox_NewFileName
			// 
			this.textBox_NewFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_NewFileName.Location = new System.Drawing.Point(6, 19);
			this.textBox_NewFileName.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.textBox_NewFileName.Name = "textBox_NewFileName";
			this.textBox_NewFileName.Size = new System.Drawing.Size(450, 26);
			this.textBox_NewFileName.TabIndex = 1;
			// 
			// panel_02
			// 
			this.panel_02.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_02.Controls.Add(this.button_Cancel);
			this.panel_02.Controls.Add(this.button_Save);
			this.panel_02.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel_02.Location = new System.Drawing.Point(0, 85);
			this.panel_02.Name = "panel_02";
			this.panel_02.Size = new System.Drawing.Size(464, 41);
			this.panel_02.TabIndex = 2;
			// 
			// FormSaveAs
			// 
			this.AcceptButton = this.button_Save;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.button_Cancel;
			this.ClientSize = new System.Drawing.Size(464, 126);
			this.Controls.Add(this.panel_01);
			this.Controls.Add(this.panel_02);
			this.FlatBorder = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FormSaveAs";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Save as...";
			this.panel_01.ResumeLayout(false);
			this.panel_01.PerformLayout();
			this.panel_02.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Cancel;
		private DarkUI.Controls.DarkButton button_Save;
		private DarkUI.Controls.DarkComboBox comboBox_FileFormat;
		private DarkUI.Controls.DarkLabel label;
		private DarkUI.Controls.DarkLabel label_Format;
		private DarkUI.Controls.DarkTextBox textBox_NewFileName;
		private System.Windows.Forms.Panel panel_01;
		private System.Windows.Forms.Panel panel_02;
	}
}