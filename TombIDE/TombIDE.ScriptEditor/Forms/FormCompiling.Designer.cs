namespace TombIDE.ScriptEditor
{
	partial class FormCompiling
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCompiling));
			this.label_Compiling = new DarkUI.Controls.DarkLabel();
			this.label_CompileInfo = new DarkUI.Controls.DarkLabel();
			this.SuspendLayout();
			// 
			// label_Compiling
			// 
			this.label_Compiling.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Compiling.ForeColor = System.Drawing.Color.Lime;
			this.label_Compiling.Location = new System.Drawing.Point(9, 9);
			this.label_Compiling.Margin = new System.Windows.Forms.Padding(0);
			this.label_Compiling.Name = "label_Compiling";
			this.label_Compiling.Size = new System.Drawing.Size(246, 25);
			this.label_Compiling.TabIndex = 2;
			this.label_Compiling.Text = "Compiling NG Script...";
			this.label_Compiling.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_CompileInfo
			// 
			this.label_CompileInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_CompileInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_CompileInfo.Location = new System.Drawing.Point(9, 34);
			this.label_CompileInfo.Margin = new System.Windows.Forms.Padding(0);
			this.label_CompileInfo.Name = "label_CompileInfo";
			this.label_CompileInfo.Size = new System.Drawing.Size(246, 93);
			this.label_CompileInfo.TabIndex = 3;
			this.label_CompileInfo.Text = resources.GetString("label_CompileInfo.Text");
			this.label_CompileInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// FormCompiling
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(264, 136);
			this.Controls.Add(this.label_CompileInfo);
			this.Controls.Add(this.label_Compiling);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FormCompiling";
			this.Opacity = 0.96D;
			this.ShowInTaskbar = false;
			this.Text = "Compiling...";
			this.TopMost = true;
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkLabel label_Compiling;
		private DarkUI.Controls.DarkLabel label_CompileInfo;
	}
}