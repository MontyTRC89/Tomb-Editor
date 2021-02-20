namespace TombIDE.ScriptingStudio
{
	partial class StudioBase
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
			this.StatusStrip = new TombIDE.ScriptingStudio.ToolStrips.StudioStatusStrip();
			this.ToolStrip = new TombIDE.ScriptingStudio.ToolStrips.StudioToolStrip();
			this.MenuStrip = new TombIDE.ScriptingStudio.ToolStrips.StudioMenuStrip();
			this.textEditorContextMenu = new TombIDE.ScriptingStudio.Controls.TextEditorContextMenu();
			this.SuspendLayout();
			// 
			// StatusStrip
			// 
			this.StatusStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.StatusStrip.Location = new System.Drawing.Point(0, 572);
			this.StatusStrip.Name = "StatusStrip";
			this.StatusStrip.Size = new System.Drawing.Size(960, 28);
			this.StatusStrip.TabIndex = 1;
			// 
			// ToolStrip
			// 
			this.ToolStrip.AutoSize = false;
			this.ToolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.ToolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.ToolStrip.Location = new System.Drawing.Point(0, 24);
			this.ToolStrip.Name = "ToolStrip";
			this.ToolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			this.ToolStrip.Size = new System.Drawing.Size(960, 25);
			this.ToolStrip.TabIndex = 3;
			this.ToolStrip.ItemClicked += new System.EventHandler(this.ToolStrip_ItemClicked);
			// 
			// MenuStrip
			// 
			this.MenuStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.MenuStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.MenuStrip.Location = new System.Drawing.Point(0, 0);
			this.MenuStrip.Name = "MenuStrip";
			this.MenuStrip.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
			this.MenuStrip.Size = new System.Drawing.Size(960, 24);
			this.MenuStrip.TabIndex = 2;
			this.MenuStrip.ItemClicked += new System.EventHandler(this.ToolStrip_ItemClicked);
			// 
			// textEditorContextMenu
			// 
			this.textEditorContextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.textEditorContextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.textEditorContextMenu.Name = "textEditorContextMenu";
			this.textEditorContextMenu.Size = new System.Drawing.Size(240, 156);
			// 
			// StudioBase
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.ToolStrip);
			this.Controls.Add(this.MenuStrip);
			this.Controls.Add(this.StatusStrip);
			this.Name = "StudioBase";
			this.Size = new System.Drawing.Size(960, 600);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Controls.TextEditorContextMenu textEditorContextMenu;
		public ToolStrips.StudioMenuStrip MenuStrip;
		public ToolStrips.StudioToolStrip ToolStrip;
		public ToolStrips.StudioStatusStrip StatusStrip;
	}
}