namespace ScriptEditor
{
	partial class FormPathInput
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
			this.darkTextBox1 = new DarkUI.Controls.DarkTextBox();
			this.darkLabel1 = new DarkUI.Controls.DarkLabel();
			this.darkButton1 = new DarkUI.Controls.DarkButton();
			this.SuspendLayout();
			// 
			// darkTextBox1
			// 
			this.darkTextBox1.Location = new System.Drawing.Point(12, 25);
			this.darkTextBox1.Name = "darkTextBox1";
			this.darkTextBox1.Size = new System.Drawing.Size(295, 20);
			this.darkTextBox1.TabIndex = 0;
			// 
			// darkLabel1
			// 
			this.darkLabel1.AutoSize = true;
			this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel1.Location = new System.Drawing.Point(12, 9);
			this.darkLabel1.Name = "darkLabel1";
			this.darkLabel1.Size = new System.Drawing.Size(60, 13);
			this.darkLabel1.TabIndex = 1;
			this.darkLabel1.Text = "darkLabel1";
			// 
			// darkButton1
			// 
			this.darkButton1.Location = new System.Drawing.Point(313, 22);
			this.darkButton1.Name = "darkButton1";
			this.darkButton1.Size = new System.Drawing.Size(75, 23);
			this.darkButton1.TabIndex = 2;
			this.darkButton1.Text = "darkButton1";
			// 
			// FormPathInput
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.ClientSize = new System.Drawing.Size(400, 150);
			this.Controls.Add(this.darkButton1);
			this.Controls.Add(this.darkLabel1);
			this.Controls.Add(this.darkTextBox1);
			this.Name = "FormPathInput";
			this.Text = "Select script folder";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DarkUI.Controls.DarkTextBox darkTextBox1;
		private DarkUI.Controls.DarkLabel darkLabel1;
		private DarkUI.Controls.DarkButton darkButton1;
	}
}