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
			this.button_Close = new DarkUI.Controls.DarkButton();
			this.label = new DarkUI.Controls.DarkLabel();
			this.SuspendLayout();
			// 
			// button_Close
			// 
			this.button_Close.ButtonStyle = DarkUI.Controls.DarkButtonStyle.Flat;
			this.button_Close.Checked = false;
			this.button_Close.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Close.Location = new System.Drawing.Point(424, 3);
			this.button_Close.Name = "button_Close";
			this.button_Close.Size = new System.Drawing.Size(23, 23);
			this.button_Close.TabIndex = 1;
			this.button_Close.Text = "X";
			// 
			// label
			// 
			this.label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label.Location = new System.Drawing.Point(0, 0);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(450, 230);
			this.label.TabIndex = 0;
			this.label.Text = resources.GetString("label.Text");
			// 
			// FormEngineHelp
			// 
			this.AcceptButton = this.button_Close;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(450, 230);
			this.Controls.Add(this.button_Close);
			this.Controls.Add(this.label);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormEngineHelp";
			this.Opacity = 0.95D;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.TopMost = true;
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Close;
		private DarkUI.Controls.DarkLabel label;
	}
}