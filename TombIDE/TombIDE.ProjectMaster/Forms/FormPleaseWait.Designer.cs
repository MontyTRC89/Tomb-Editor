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
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.labelStatus = new DarkUI.Controls.DarkLabel();
			this.SuspendLayout();
			// 
			// label
			// 
			this.label.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label.Location = new System.Drawing.Point(12, 9);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(480, 30);
			this.label.TabIndex = 0;
			this.label.Text = "Please wait...";
			this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(12, 70);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(480, 23);
			this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.progressBar.TabIndex = 1;
			// 
			// labelStatus
			// 
			this.labelStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.labelStatus.Location = new System.Drawing.Point(12, 42);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(480, 20);
			this.labelStatus.TabIndex = 2;
			this.labelStatus.Text = "Initializing...";
			this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// FormPleaseWait
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(504, 105);
			this.Controls.Add(this.labelStatus);
			this.Controls.Add(this.progressBar);
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
		private System.Windows.Forms.ProgressBar progressBar;
		private DarkUI.Controls.DarkLabel labelStatus;
	}
}