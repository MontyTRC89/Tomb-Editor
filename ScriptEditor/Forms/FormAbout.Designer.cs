namespace ScriptEditor
{
	partial class FormAbout : DarkUI.Forms.DarkForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.iMadeDis = new DarkUI.Controls.DarkLabel();
			this.SuspendLayout();
			// 
			// iMadeDis
			// 
			this.iMadeDis.AutoSize = true;
			this.iMadeDis.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.iMadeDis.Location = new System.Drawing.Point(12, 9);
			this.iMadeDis.Name = "iMadeDis";
			this.iMadeDis.Size = new System.Drawing.Size(72, 13);
			this.iMadeDis.TabIndex = 0;
			this.iMadeDis.Text = "Made by: Me!";
			// 
			// FormAbout
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.iMadeDis);
			this.Name = "FormAbout";
			this.Text = "FormAbout";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DarkUI.Controls.DarkLabel iMadeDis;
	}
}