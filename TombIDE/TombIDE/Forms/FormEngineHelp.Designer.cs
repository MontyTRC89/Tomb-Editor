namespace TombIDE
{
	partial class FormEngineHelp
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEngineHelp));
			this.label = new DarkUI.Controls.DarkLabel();
			this.SuspendLayout();
			// 
			// label
			// 
			this.label.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label.Location = new System.Drawing.Point(10, 10);
			this.label.Name = "label";
			this.label.Padding = new System.Windows.Forms.Padding(3);
			this.label.Size = new System.Drawing.Size(400, 420);
			this.label.TabIndex = 0;
			this.label.Text = resources.GetString("label.Text");
			this.label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label.Click += new System.EventHandler(this.label_Click);
			// 
			// FormEngineHelp
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(420, 440);
			this.Controls.Add(this.label);
			this.FlatBorder = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormEngineHelp";
			this.Opacity = 0.95D;
			this.Padding = new System.Windows.Forms.Padding(10);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.TopMost = true;
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkLabel label;
	}
}