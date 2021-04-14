namespace TombEditor.ToolWindows
{
    partial class Palette
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.paletteToolBar = new System.Windows.Forms.FlowLayoutPanel();
            this.butResetToDefaults = new DarkUI.Controls.DarkButton();
            this.butSampleFromTextures = new DarkUI.Controls.DarkButton();
            this.butEditColor = new DarkUI.Controls.DarkButton();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.lightPalette = new TombEditor.Controls.PanelPalette();
            this.paletteToolBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lightPalette)).BeginInit();
            this.SuspendLayout();
            // 
            // paletteToolBar
            // 
            this.paletteToolBar.AutoSize = true;
            this.paletteToolBar.Controls.Add(this.butResetToDefaults);
            this.paletteToolBar.Controls.Add(this.butSampleFromTextures);
            this.paletteToolBar.Controls.Add(this.butEditColor);
            this.paletteToolBar.Dock = System.Windows.Forms.DockStyle.Left;
            this.paletteToolBar.Location = new System.Drawing.Point(0, 25);
            this.paletteToolBar.Name = "paletteToolBar";
            this.paletteToolBar.Size = new System.Drawing.Size(30, 111);
            this.paletteToolBar.TabIndex = 84;
            // 
            // butResetToDefaults
            // 
            this.butResetToDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butResetToDefaults.Checked = false;
            this.butResetToDefaults.Image = global::TombEditor.Properties.Resources.actions_refresh_16;
            this.butResetToDefaults.Location = new System.Drawing.Point(3, 3);
            this.butResetToDefaults.Name = "butResetToDefaults";
            this.butResetToDefaults.Size = new System.Drawing.Size(24, 24);
            this.butResetToDefaults.TabIndex = 5;
            this.butResetToDefaults.Tag = "ResetPalette";
            // 
            // butSampleFromTextures
            // 
            this.butSampleFromTextures.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSampleFromTextures.Checked = false;
            this.butSampleFromTextures.Image = global::TombEditor.Properties.Resources.actions_TextureMode_16;
            this.butSampleFromTextures.Location = new System.Drawing.Point(3, 33);
            this.butSampleFromTextures.Name = "butSampleFromTextures";
            this.butSampleFromTextures.Size = new System.Drawing.Size(24, 24);
            this.butSampleFromTextures.TabIndex = 6;
            this.butSampleFromTextures.Tag = "SamplePaletteFromTextures";
            // 
            // butEditColor
            // 
            this.butEditColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butEditColor.Checked = false;
            this.butEditColor.Enabled = false;
            this.butEditColor.Image = global::TombEditor.Properties.Resources.general_edit_16;
            this.butEditColor.Location = new System.Drawing.Point(3, 63);
            this.butEditColor.Name = "butEditColor";
            this.butEditColor.Size = new System.Drawing.Size(24, 24);
            this.butEditColor.TabIndex = 7;
            this.butEditColor.Tag = "EditObjectColor";
            this.toolTip.SetToolTip(this.butEditColor, "Edit color for selected object");
            // 
            // lightPalette
            // 
            this.lightPalette.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lightPalette.Editable = true;
            this.lightPalette.Location = new System.Drawing.Point(30, 25);
            this.lightPalette.Margin = new System.Windows.Forms.Padding(2);
            this.lightPalette.Name = "lightPalette";
            this.lightPalette.Padding = new System.Windows.Forms.Padding(3);
            this.lightPalette.Size = new System.Drawing.Size(615, 111);
            this.lightPalette.TabIndex = 82;
            this.lightPalette.TabStop = false;
            // 
            // Palette
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lightPalette);
            this.Controls.Add(this.paletteToolBar);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Bottom;
            this.DockText = "Palette";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(100, 100);
            this.Name = "Palette";
            this.SerializationKey = "Palette";
            this.Size = new System.Drawing.Size(645, 136);
            this.paletteToolBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lightPalette)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.PanelPalette lightPalette;
        private DarkUI.Controls.DarkButton butSampleFromTextures;
        private DarkUI.Controls.DarkButton butResetToDefaults;
        private System.Windows.Forms.FlowLayoutPanel paletteToolBar;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkButton butEditColor;
    }
}
