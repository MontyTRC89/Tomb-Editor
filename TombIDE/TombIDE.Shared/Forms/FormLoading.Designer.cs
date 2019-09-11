namespace TombIDE.Shared
{
	partial class FormLoading
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
			this.components = new System.ComponentModel.Container();
			this.label = new DarkUI.Controls.DarkLabel();
			this.panel = new System.Windows.Forms.Panel();
			this.progressBar = new DarkUI.Controls.DarkProgressBar();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// label
			// 
			this.label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label.Location = new System.Drawing.Point(11, 13);
			this.label.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(440, 28);
			this.label.TabIndex = 0;
			this.label.Text = "Updating the project\'s level list...";
			this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panel
			// 
			this.panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel.Controls.Add(this.label);
			this.panel.Controls.Add(this.progressBar);
			this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel.Location = new System.Drawing.Point(0, 0);
			this.panel.Name = "panel";
			this.panel.Size = new System.Drawing.Size(464, 89);
			this.panel.TabIndex = 0;
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(11, 47);
			this.progressBar.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(440, 28);
			this.progressBar.TabIndex = 1;
			// 
			// timer
			// 
			this.timer.Interval = 300;
			this.timer.Tick += new System.EventHandler(this.timer_Tick);
			// 
			// FormLoading
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 89);
			this.Controls.Add(this.panel);
			this.FlatBorder = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormLoading";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Loading...";
			this.panel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkLabel label;
		private DarkUI.Controls.DarkProgressBar progressBar;
		private System.Windows.Forms.Panel panel;
		private System.Windows.Forms.Timer timer;
	}
}