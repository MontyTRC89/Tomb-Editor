namespace TombIDE.ScriptEditor.Forms
{
	partial class FormMnemonicInfo
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
			this.checkBox_AlwaysTop = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_CloseTabs = new DarkUI.Controls.DarkCheckBox();
			this.panel = new System.Windows.Forms.Panel();
			this.tabControl = new System.Windows.Forms.CustomTabControl();
			this.panel_Buttons = new System.Windows.Forms.Panel();
			this.panel.SuspendLayout();
			this.panel_Buttons.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkBox_AlwaysTop
			// 
			this.checkBox_AlwaysTop.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.checkBox_AlwaysTop.Location = new System.Drawing.Point(198, 6);
			this.checkBox_AlwaysTop.Margin = new System.Windows.Forms.Padding(190, 3, 12, 3);
			this.checkBox_AlwaysTop.Name = "checkBox_AlwaysTop";
			this.checkBox_AlwaysTop.Size = new System.Drawing.Size(180, 16);
			this.checkBox_AlwaysTop.TabIndex = 0;
			this.checkBox_AlwaysTop.Text = "Keep this window always on top";
			this.checkBox_AlwaysTop.CheckedChanged += new System.EventHandler(this.checkBox_AlwaysTop_CheckedChanged);
			// 
			// checkBox_CloseTabs
			// 
			this.checkBox_CloseTabs.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.checkBox_CloseTabs.Location = new System.Drawing.Point(402, 6);
			this.checkBox_CloseTabs.Margin = new System.Windows.Forms.Padding(12, 3, 190, 3);
			this.checkBox_CloseTabs.Name = "checkBox_CloseTabs";
			this.checkBox_CloseTabs.Size = new System.Drawing.Size(190, 16);
			this.checkBox_CloseTabs.TabIndex = 1;
			this.checkBox_CloseTabs.Text = "Close all tabs after window closes";
			this.checkBox_CloseTabs.CheckedChanged += new System.EventHandler(this.checkBox_CloseTabs_CheckedChanged);
			// 
			// panel
			// 
			this.panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel.Controls.Add(this.tabControl);
			this.panel.Controls.Add(this.panel_Buttons);
			this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel.Location = new System.Drawing.Point(0, 0);
			this.panel.Name = "panel";
			this.panel.Size = new System.Drawing.Size(784, 561);
			this.panel.TabIndex = 0;
			// 
			// tabControl
			// 
			this.tabControl.AllowDrop = true;
			this.tabControl.DisplayStyle = System.Windows.Forms.TabStyle.Dark;
			// 
			// 
			// 
			this.tabControl.DisplayStyleProvider.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl.DisplayStyleProvider.BorderColorHot = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl.DisplayStyleProvider.BorderColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl.DisplayStyleProvider.CloserColor = System.Drawing.Color.White;
			this.tabControl.DisplayStyleProvider.CloserColorActive = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(196)))), ((int)(((byte)(232)))));
			this.tabControl.DisplayStyleProvider.FocusTrack = false;
			this.tabControl.DisplayStyleProvider.HotTrack = true;
			this.tabControl.DisplayStyleProvider.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.tabControl.DisplayStyleProvider.Opacity = 1F;
			this.tabControl.DisplayStyleProvider.Overlap = 0;
			this.tabControl.DisplayStyleProvider.Padding = new System.Drawing.Point(6, 4);
			this.tabControl.DisplayStyleProvider.Radius = 10;
			this.tabControl.DisplayStyleProvider.ShowTabCloser = true;
			this.tabControl.DisplayStyleProvider.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.tabControl.DisplayStyleProvider.TextColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl.DisplayStyleProvider.TextColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(196)))), ((int)(((byte)(232)))));
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.tabControl.HotTrack = true;
			this.tabControl.ItemSize = new System.Drawing.Size(0, 30);
			this.tabControl.Location = new System.Drawing.Point(0, 0);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(782, 529);
			this.tabControl.TabIndex = 0;
			this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
			this.tabControl.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tabControl_MouseClick);
			// 
			// panel_Buttons
			// 
			this.panel_Buttons.Controls.Add(this.checkBox_CloseTabs);
			this.panel_Buttons.Controls.Add(this.checkBox_AlwaysTop);
			this.panel_Buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel_Buttons.Location = new System.Drawing.Point(0, 529);
			this.panel_Buttons.Name = "panel_Buttons";
			this.panel_Buttons.Size = new System.Drawing.Size(782, 30);
			this.panel_Buttons.TabIndex = 1;
			// 
			// FormMnemonicInfo
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 561);
			this.Controls.Add(this.panel);
			this.MinimumSize = new System.Drawing.Size(640, 480);
			this.Name = "FormMnemonicInfo";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Information about FLAG_NAME_GOES_HERE";
			this.panel.ResumeLayout(false);
			this.panel_Buttons.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkCheckBox checkBox_AlwaysTop;
		private DarkUI.Controls.DarkCheckBox checkBox_CloseTabs;
		private System.Windows.Forms.CustomTabControl tabControl;
		private System.Windows.Forms.Panel panel;
		private System.Windows.Forms.Panel panel_Buttons;
	}
}