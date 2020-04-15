namespace TombIDE.ScriptEditor
{
	partial class FormDebugMode
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
			this.label_Debug = new DarkUI.Controls.DarkLabel();
			this.label_Info = new DarkUI.Controls.DarkLabel();
			this.SuspendLayout();
			// 
			// label_Debug
			// 
			this.label_Debug.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Debug.ForeColor = System.Drawing.Color.Orange;
			this.label_Debug.Location = new System.Drawing.Point(9, 9);
			this.label_Debug.Margin = new System.Windows.Forms.Padding(0);
			this.label_Debug.Name = "label_Debug";
			this.label_Debug.Size = new System.Drawing.Size(246, 25);
			this.label_Debug.TabIndex = 0;
			this.label_Debug.Text = "YOU ARE IN DEBUG MODE!";
			this.label_Debug.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_Info
			// 
			this.label_Info.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Info.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Info.Location = new System.Drawing.Point(9, 34);
			this.label_Info.Margin = new System.Windows.Forms.Padding(0);
			this.label_Info.Name = "label_Info";
			this.label_Info.Size = new System.Drawing.Size(246, 93);
			this.label_Info.TabIndex = 1;
			this.label_Info.Text = "This is not a fully functional version of NG_Center!\r\n\r\nAny changes made to the s" +
    "cript inside NG_Center WILL NOT be saved.\r\n\r\nJust use this window to view the er" +
    "rors and NOTHING ELSE.";
			this.label_Info.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// FormDebugMode
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(264, 136);
			this.Controls.Add(this.label_Info);
			this.Controls.Add(this.label_Debug);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FormDebugMode";
			this.Opacity = 0.96D;
			this.ShowInTaskbar = false;
			this.Text = "DEBUG MODE";
			this.TopMost = true;
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkLabel label_Debug;
		private DarkUI.Controls.DarkLabel label_Info;
	}
}