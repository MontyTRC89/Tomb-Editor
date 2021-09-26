namespace TombIDE.ProjectMaster
{
	partial class FormSplashPreview
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
            this.label = new System.Windows.Forms.Label();
            this.panel = new System.Windows.Forms.Panel();
            this.timer_Animation = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // label
            // 
            this.label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label.ForeColor = System.Drawing.Color.Silver;
            this.label.Location = new System.Drawing.Point(0, 256);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(512, 60);
            this.label.TabIndex = 0;
            this.label.Text = "Press ESC or click here to EXIT this dialog...";
            this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label.Click += new System.EventHandler(this.Label_Click);
            // 
            // panel
            // 
            this.panel.BackColor = System.Drawing.Color.Black;
            this.panel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(512, 256);
            this.panel.TabIndex = 1;
            // 
            // timer_Animation
            // 
            this.timer_Animation.Interval = 1;
            this.timer_Animation.Tick += new System.EventHandler(this.Timer_Animation_Tick);
            // 
            // FormSplashPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(512, 316);
            this.Controls.Add(this.panel);
            this.Controls.Add(this.label);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSplashPreview";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label;
		private System.Windows.Forms.Panel panel;
		private System.Windows.Forms.Timer timer_Animation;
	}
}