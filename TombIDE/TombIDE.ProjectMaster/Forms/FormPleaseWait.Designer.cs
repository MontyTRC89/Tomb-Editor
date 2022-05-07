namespace TombIDE.ProjectMaster.Forms
{
	partial class FormPleaseWait
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
			this.label = new DarkUI.Controls.DarkLabel();
			this.SuspendLayout();
			// 
			// label
			// 
			this.label.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label.Location = new System.Drawing.Point(0, 0);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(404, 101);
			this.label.TabIndex = 0;
			this.label.Text = "Please wait...\r\n\r\nCreating the archive may even take a few minutes...";
			this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// FormPleaseWait
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(404, 101);
			this.Controls.Add(this.label);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormPleaseWait";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Please wait...";
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkLabel label;
	}
}