namespace TombIDE
{
	partial class FormFindMusic
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
			this.darkLabel1 = new DarkUI.Controls.DarkLabel();
			this.darkLabel2 = new DarkUI.Controls.DarkLabel();
			this.textBox_ArchivePath = new DarkUI.Controls.DarkTextBox();
			this.button_Download = new DarkUI.Controls.DarkButton();
			this.button_Browse = new DarkUI.Controls.DarkButton();
			this.darkLabel3 = new DarkUI.Controls.DarkLabel();
			this.button_Continue = new DarkUI.Controls.DarkButton();
			this.button_Cancel = new DarkUI.Controls.DarkButton();
			this.SuspendLayout();
			// 
			// darkLabel1
			// 
			this.darkLabel1.AutoSize = true;
			this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel1.Location = new System.Drawing.Point(20, 20);
			this.darkLabel1.Margin = new System.Windows.Forms.Padding(10, 10, 10, 0);
			this.darkLabel1.Name = "darkLabel1";
			this.darkLabel1.Size = new System.Drawing.Size(405, 13);
			this.darkLabel1.TabIndex = 0;
			this.darkLabel1.Text = "In order to install the game, you have to download the game\'s /music/ folder:";
			// 
			// darkLabel2
			// 
			this.darkLabel2.AutoSize = true;
			this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel2.Location = new System.Drawing.Point(20, 89);
			this.darkLabel2.Margin = new System.Windows.Forms.Padding(10, 20, 3, 0);
			this.darkLabel2.Name = "darkLabel2";
			this.darkLabel2.Size = new System.Drawing.Size(253, 13);
			this.darkLabel2.TabIndex = 1;
			this.darkLabel2.Text = "If you\'ve already downloaded it, please select it:";
			// 
			// textBox_ArchivePath
			// 
			this.textBox_ArchivePath.Location = new System.Drawing.Point(23, 112);
			this.textBox_ArchivePath.Margin = new System.Windows.Forms.Padding(10, 10, 3, 3);
			this.textBox_ArchivePath.Name = "textBox_ArchivePath";
			this.textBox_ArchivePath.Size = new System.Drawing.Size(320, 22);
			this.textBox_ArchivePath.TabIndex = 2;
			this.textBox_ArchivePath.TextChanged += new System.EventHandler(this.textBox_ArchivePath_TextChanged);
			// 
			// button_Download
			// 
			this.button_Download.Checked = false;
			this.button_Download.Location = new System.Drawing.Point(23, 43);
			this.button_Download.Margin = new System.Windows.Forms.Padding(10, 10, 10, 3);
			this.button_Download.Name = "button_Download";
			this.button_Download.Size = new System.Drawing.Size(402, 23);
			this.button_Download.TabIndex = 3;
			this.button_Download.Text = "Download music.zip";
			this.button_Download.Click += new System.EventHandler(this.button_Download_Click);
			// 
			// button_Browse
			// 
			this.button_Browse.Checked = false;
			this.button_Browse.Location = new System.Drawing.Point(349, 112);
			this.button_Browse.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
			this.button_Browse.Name = "button_Browse";
			this.button_Browse.Size = new System.Drawing.Size(75, 23);
			this.button_Browse.TabIndex = 4;
			this.button_Browse.Text = "Browse...";
			this.button_Browse.Click += new System.EventHandler(this.button_Browse_Click);
			// 
			// darkLabel3
			// 
			this.darkLabel3.AutoSize = true;
			this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel3.Location = new System.Drawing.Point(20, 157);
			this.darkLabel3.Margin = new System.Windows.Forms.Padding(10, 20, 3, 0);
			this.darkLabel3.Name = "darkLabel3";
			this.darkLabel3.Size = new System.Drawing.Size(182, 13);
			this.darkLabel3.TabIndex = 5;
			this.darkLabel3.Text = "After you\'re done, press continue:";
			// 
			// button_Continue
			// 
			this.button_Continue.Checked = false;
			this.button_Continue.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Continue.Enabled = false;
			this.button_Continue.Location = new System.Drawing.Point(23, 180);
			this.button_Continue.Margin = new System.Windows.Forms.Padding(10, 10, 10, 3);
			this.button_Continue.Name = "button_Continue";
			this.button_Continue.Size = new System.Drawing.Size(402, 23);
			this.button_Continue.TabIndex = 6;
			this.button_Continue.Text = "Continue";
			this.button_Continue.Click += new System.EventHandler(this.button_Continue_Click);
			// 
			// button_Cancel
			// 
			this.button_Cancel.Checked = false;
			this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_Cancel.Location = new System.Drawing.Point(22, 216);
			this.button_Cancel.Margin = new System.Windows.Forms.Padding(10, 10, 10, 3);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.Size = new System.Drawing.Size(402, 23);
			this.button_Cancel.TabIndex = 7;
			this.button_Cancel.Text = "Cancel";
			// 
			// FormFindMusic
			// 
			this.AcceptButton = this.button_Continue;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.button_Cancel;
			this.ClientSize = new System.Drawing.Size(448, 261);
			this.Controls.Add(this.button_Cancel);
			this.Controls.Add(this.button_Continue);
			this.Controls.Add(this.darkLabel3);
			this.Controls.Add(this.button_Browse);
			this.Controls.Add(this.button_Download);
			this.Controls.Add(this.textBox_ArchivePath);
			this.Controls.Add(this.darkLabel2);
			this.Controls.Add(this.darkLabel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "FormFindMusic";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Select music.zip";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DarkUI.Controls.DarkLabel darkLabel1;
		private DarkUI.Controls.DarkLabel darkLabel2;
		private DarkUI.Controls.DarkTextBox textBox_ArchivePath;
		private DarkUI.Controls.DarkButton button_Download;
		private DarkUI.Controls.DarkButton button_Browse;
		private DarkUI.Controls.DarkLabel darkLabel3;
		private DarkUI.Controls.DarkButton button_Continue;
		private DarkUI.Controls.DarkButton button_Cancel;
	}
}