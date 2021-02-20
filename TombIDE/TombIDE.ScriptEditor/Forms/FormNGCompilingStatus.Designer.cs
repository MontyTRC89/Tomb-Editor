namespace TombIDE.ScriptingStudio.Forms
{
	partial class FormNGCompilingStatus
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
			this.label_CompileInfo_01 = new DarkUI.Controls.DarkLabel();
			this.label_CompileInfo_02 = new DarkUI.Controls.DarkLabel();
			this.label_Compiling = new DarkUI.Controls.DarkLabel();
			this.label_Debug = new DarkUI.Controls.DarkLabel();
			this.label_DebugInfo = new DarkUI.Controls.DarkLabel();
			this.SuspendLayout();
			// 
			// label_CompileInfo_01
			// 
			this.label_CompileInfo_01.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_CompileInfo_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_CompileInfo_01.Location = new System.Drawing.Point(9, 34);
			this.label_CompileInfo_01.Margin = new System.Windows.Forms.Padding(0);
			this.label_CompileInfo_01.Name = "label_CompileInfo_01";
			this.label_CompileInfo_01.Size = new System.Drawing.Size(246, 56);
			this.label_CompileInfo_01.TabIndex = 5;
			this.label_CompileInfo_01.Text = "Please DO NOT move your cursor or switch active windows.\r\n";
			this.label_CompileInfo_01.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_CompileInfo_02
			// 
			this.label_CompileInfo_02.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_CompileInfo_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_CompileInfo_02.Location = new System.Drawing.Point(9, 90);
			this.label_CompileInfo_02.Margin = new System.Windows.Forms.Padding(0);
			this.label_CompileInfo_02.Name = "label_CompileInfo_02";
			this.label_CompileInfo_02.Size = new System.Drawing.Size(246, 37);
			this.label_CompileInfo_02.TabIndex = 6;
			this.label_CompileInfo_02.Text = "You may only move your cursor if nothing happens for about a minute.\r\n";
			this.label_CompileInfo_02.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_Compiling
			// 
			this.label_Compiling.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Compiling.ForeColor = System.Drawing.Color.Lime;
			this.label_Compiling.Location = new System.Drawing.Point(9, 9);
			this.label_Compiling.Margin = new System.Windows.Forms.Padding(0);
			this.label_Compiling.Name = "label_Compiling";
			this.label_Compiling.Size = new System.Drawing.Size(246, 25);
			this.label_Compiling.TabIndex = 4;
			this.label_Compiling.Text = "Compiling NG Script...";
			this.label_Compiling.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
			// label_DebugInfo
			// 
			this.label_DebugInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_DebugInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_DebugInfo.Location = new System.Drawing.Point(9, 34);
			this.label_DebugInfo.Margin = new System.Windows.Forms.Padding(0);
			this.label_DebugInfo.Name = "label_DebugInfo";
			this.label_DebugInfo.Size = new System.Drawing.Size(246, 93);
			this.label_DebugInfo.TabIndex = 1;
			this.label_DebugInfo.Text = "This is not a fully functional version of NG_Center!\r\n\r\nAny changes made to the s" +
    "cript inside NG_Center WILL NOT be saved.\r\n\r\nJust use this window to view the er" +
    "rors and NOTHING ELSE.";
			this.label_DebugInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// FormNGCompilingStatus
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(264, 136);
			this.Controls.Add(this.label_CompileInfo_02);
			this.Controls.Add(this.label_CompileInfo_01);
			this.Controls.Add(this.label_Compiling);
			this.Controls.Add(this.label_DebugInfo);
			this.Controls.Add(this.label_Debug);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FormNGCompilingStatus";
			this.Opacity = 0.96D;
			this.ShowInTaskbar = false;
			this.Text = "Compiling NG Script...";
			this.TopMost = true;
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkLabel label_CompileInfo_01;
		private DarkUI.Controls.DarkLabel label_CompileInfo_02;
		private DarkUI.Controls.DarkLabel label_Compiling;
		private DarkUI.Controls.DarkLabel label_Debug;
		private DarkUI.Controls.DarkLabel label_DebugInfo;
	}
}