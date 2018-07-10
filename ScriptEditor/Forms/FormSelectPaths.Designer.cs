namespace ScriptEditor
{
	partial class FormSelectPaths : DarkUI.Forms.DarkForm
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
			this.applyButton = new DarkUI.Controls.DarkButton();
			this.browseGameButton = new DarkUI.Controls.DarkButton();
			this.browseScriptButton = new DarkUI.Controls.DarkButton();
			this.darkLabel1 = new DarkUI.Controls.DarkLabel();
			this.darkLabel2 = new DarkUI.Controls.DarkLabel();
			this.gamePathTextBox = new DarkUI.Controls.DarkTextBox();
			this.scriptPathTextBox = new DarkUI.Controls.DarkTextBox();
			this.SuspendLayout();
			// 
			// applyButton
			// 
			this.applyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.applyButton.Location = new System.Drawing.Point(12, 66);
			this.applyButton.Name = "applyButton";
			this.applyButton.Size = new System.Drawing.Size(454, 22);
			this.applyButton.TabIndex = 4;
			this.applyButton.Text = "Apply";
			this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
			// 
			// browseGameButton
			// 
			this.browseGameButton.Location = new System.Drawing.Point(391, 38);
			this.browseGameButton.Name = "browseGameButton";
			this.browseGameButton.Size = new System.Drawing.Size(75, 22);
			this.browseGameButton.TabIndex = 3;
			this.browseGameButton.Text = "Browse...";
			this.browseGameButton.Click += new System.EventHandler(this.browseGameButton_Click);
			// 
			// browseScriptButton
			// 
			this.browseScriptButton.Location = new System.Drawing.Point(391, 10);
			this.browseScriptButton.Name = "browseScriptButton";
			this.browseScriptButton.Size = new System.Drawing.Size(75, 22);
			this.browseScriptButton.TabIndex = 2;
			this.browseScriptButton.Text = "Browse...";
			this.browseScriptButton.Click += new System.EventHandler(this.browseScriptButton_Click);
			// 
			// darkLabel1
			// 
			this.darkLabel1.AutoSize = true;
			this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel1.Location = new System.Drawing.Point(12, 14);
			this.darkLabel1.Name = "darkLabel1";
			this.darkLabel1.Size = new System.Drawing.Size(66, 13);
			this.darkLabel1.TabIndex = 5;
			this.darkLabel1.Text = "Script folder:";
			// 
			// darkLabel2
			// 
			this.darkLabel2.AutoSize = true;
			this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.darkLabel2.Location = new System.Drawing.Point(12, 42);
			this.darkLabel2.Name = "darkLabel2";
			this.darkLabel2.Size = new System.Drawing.Size(67, 13);
			this.darkLabel2.TabIndex = 6;
			this.darkLabel2.Text = "Game folder:";
			// 
			// gamePathTextBox
			// 
			this.gamePathTextBox.Location = new System.Drawing.Point(84, 40);
			this.gamePathTextBox.Name = "gamePathTextBox";
			this.gamePathTextBox.Size = new System.Drawing.Size(300, 20);
			this.gamePathTextBox.TabIndex = 1;
			// 
			// scriptPathTextBox
			// 
			this.scriptPathTextBox.Location = new System.Drawing.Point(84, 12);
			this.scriptPathTextBox.Name = "scriptPathTextBox";
			this.scriptPathTextBox.Size = new System.Drawing.Size(300, 20);
			this.scriptPathTextBox.TabIndex = 0;
			// 
			// FormSelectPaths
			// 
			this.AcceptButton = this.applyButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.ClientSize = new System.Drawing.Size(478, 100);
			this.Controls.Add(this.darkLabel2);
			this.Controls.Add(this.darkLabel1);
			this.Controls.Add(this.applyButton);
			this.Controls.Add(this.browseGameButton);
			this.Controls.Add(this.browseScriptButton);
			this.Controls.Add(this.gamePathTextBox);
			this.Controls.Add(this.scriptPathTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "FormSelectPaths";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Select required paths:";
			this.Shown += new System.EventHandler(this.FormSelectPaths_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DarkUI.Controls.DarkButton applyButton;
		private DarkUI.Controls.DarkButton browseGameButton;
		private DarkUI.Controls.DarkButton browseScriptButton;
		private DarkUI.Controls.DarkLabel darkLabel1;
		private DarkUI.Controls.DarkLabel darkLabel2;
		private DarkUI.Controls.DarkTextBox gamePathTextBox;
		private DarkUI.Controls.DarkTextBox scriptPathTextBox;
	}
}