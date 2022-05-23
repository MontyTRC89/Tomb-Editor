namespace TombIDE.ScriptingStudio.ToolStrips
{
	partial class StudioStatusStrip
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

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.button_ResetZoom = new DarkUI.Controls.DarkButton();
			this.label_ColNumber = new System.Windows.Forms.ToolStripStatusLabel();
			this.label_RowNumber = new System.Windows.Forms.ToolStripStatusLabel();
			this.label_Selected = new System.Windows.Forms.ToolStripStatusLabel();
			this.label_Zoom = new DarkUI.Controls.DarkLabel();
			this.panel_Syntax = new System.Windows.Forms.Panel();
			this.SyntaxPreview = new TombIDE.ScriptingStudio.Controls.SyntaxPreview();
			this.statusStrip = new DarkUI.Controls.DarkStatusStrip();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.panel_Syntax.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_ResetZoom
			// 
			this.button_ResetZoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button_ResetZoom.Checked = false;
			this.button_ResetZoom.Enabled = false;
			this.button_ResetZoom.Image = global::TombIDE.ScriptingStudio.Properties.Resources.Reset_16;
			this.button_ResetZoom.Location = new System.Drawing.Point(934, 2);
			this.button_ResetZoom.Margin = new System.Windows.Forms.Padding(2);
			this.button_ResetZoom.Name = "button_ResetZoom";
			this.button_ResetZoom.Size = new System.Drawing.Size(24, 24);
			this.button_ResetZoom.TabIndex = 3;
			this.button_ResetZoom.Click += new System.EventHandler(this.button_ResetZoom_Click);
			// 
			// label_ColNumber
			// 
			this.label_ColNumber.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.label_ColNumber.Name = "label_ColNumber";
			this.label_ColNumber.Size = new System.Drawing.Size(62, 15);
			this.label_ColNumber.Text = "Column: 0";
			// 
			// label_RowNumber
			// 
			this.label_RowNumber.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.label_RowNumber.Name = "label_RowNumber";
			this.label_RowNumber.Size = new System.Drawing.Size(42, 15);
			this.label_RowNumber.Text = "Row: 0";
			// 
			// label_Selected
			// 
			this.label_Selected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.label_Selected.Name = "label_Selected";
			this.label_Selected.Size = new System.Drawing.Size(63, 15);
			this.label_Selected.Text = "Selected: 0";
			// 
			// label_Zoom
			// 
			this.label_Zoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label_Zoom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Zoom.Location = new System.Drawing.Point(822, 4);
			this.label_Zoom.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.label_Zoom.Name = "label_Zoom";
			this.label_Zoom.Size = new System.Drawing.Size(110, 20);
			this.label_Zoom.TabIndex = 2;
			this.label_Zoom.Text = "Zoom: 100%";
			this.label_Zoom.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panel_Syntax
			// 
			this.panel_Syntax.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel_Syntax.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Syntax.Controls.Add(this.SyntaxPreview);
			this.panel_Syntax.Location = new System.Drawing.Point(256, 4);
			this.panel_Syntax.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.panel_Syntax.Name = "panel_Syntax";
			this.panel_Syntax.Size = new System.Drawing.Size(560, 20);
			this.panel_Syntax.TabIndex = 1;
			this.panel_Syntax.Visible = false;
			// 
			// SyntaxPreview
			// 
			this.SyntaxPreview.BackColor = System.Drawing.Color.Black;
			this.SyntaxPreview.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.SyntaxPreview.CurrentArgumentIndex = 0;
			this.SyntaxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SyntaxPreview.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
			this.SyntaxPreview.ForeColor = System.Drawing.Color.White;
			this.SyntaxPreview.Location = new System.Drawing.Point(0, 0);
			this.SyntaxPreview.Name = "SyntaxPreview";
			this.SyntaxPreview.ReadOnly = true;
			this.SyntaxPreview.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			this.SyntaxPreview.Size = new System.Drawing.Size(558, 18);
			this.SyntaxPreview.TabIndex = 0;
			this.SyntaxPreview.Text = "";
			this.SyntaxPreview.WordWrap = false;
			// 
			// statusStrip
			// 
			this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.statusStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.label_RowNumber,
            this.label_ColNumber,
            this.label_Selected});
			this.statusStrip.Location = new System.Drawing.Point(0, 0);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Padding = new System.Windows.Forms.Padding(2, 5, 0, 3);
			this.statusStrip.Size = new System.Drawing.Size(960, 28);
			this.statusStrip.TabIndex = 0;
			// 
			// StudioStatusStrip
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.button_ResetZoom);
			this.Controls.Add(this.label_Zoom);
			this.Controls.Add(this.panel_Syntax);
			this.Controls.Add(this.statusStrip);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "StudioStatusStrip";
			this.Size = new System.Drawing.Size(960, 28);
			this.panel_Syntax.ResumeLayout(false);
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_ResetZoom;
		private DarkUI.Controls.DarkLabel label_Zoom;
		private DarkUI.Controls.DarkStatusStrip statusStrip;
		private System.Windows.Forms.Panel panel_Syntax;
		private System.Windows.Forms.ToolStripStatusLabel label_ColNumber;
		private System.Windows.Forms.ToolStripStatusLabel label_RowNumber;
		private System.Windows.Forms.ToolStripStatusLabel label_Selected;
		public Controls.SyntaxPreview SyntaxPreview;
		private System.Windows.Forms.ToolTip toolTip;
	}
}
