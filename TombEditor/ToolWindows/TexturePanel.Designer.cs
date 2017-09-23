namespace TombEditor.ToolWindows
{
    partial class TexturePanel
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
            this.panelTextureMap = new TombEditor.Controls.PanelTextureMap();
            this.panelTextureTools = new System.Windows.Forms.Panel();
            this.butBump = new DarkUI.Controls.DarkButton();
            this.butAnimationRanges = new DarkUI.Controls.DarkButton();
            this.butTextureSounds = new DarkUI.Controls.DarkButton();
            this.panelTextureTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTextureMap
            // 
            this.panelTextureMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTextureMap.Location = new System.Drawing.Point(0, 25);
            this.panelTextureMap.Name = "panelTextureMap";
            this.panelTextureMap.Size = new System.Drawing.Size(288, 733);
            this.panelTextureMap.TabIndex = 9;
            // 
            // panelTextureTools
            // 
            this.panelTextureTools.Controls.Add(this.butBump);
            this.panelTextureTools.Controls.Add(this.butAnimationRanges);
            this.panelTextureTools.Controls.Add(this.butTextureSounds);
            this.panelTextureTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelTextureTools.Location = new System.Drawing.Point(0, 788);
            this.panelTextureTools.Name = "panelTextureTools";
            this.panelTextureTools.Size = new System.Drawing.Size(738, 31);
            this.panelTextureTools.TabIndex = 10;
            // 
            // butBump
            // 
            this.butBump.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butBump.Location = new System.Drawing.Point(220, 4);
            this.butBump.Name = "butBump";
            this.butBump.Padding = new System.Windows.Forms.Padding(5);
            this.butBump.Size = new System.Drawing.Size(64, 23);
            this.butBump.TabIndex = 2;
            this.butBump.Text = "Bump";
            this.butBump.Visible = false;
            // 
            // butAnimationRanges
            // 
            this.butAnimationRanges.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAnimationRanges.Location = new System.Drawing.Point(111, 4);
            this.butAnimationRanges.Name = "butAnimationRanges";
            this.butAnimationRanges.Padding = new System.Windows.Forms.Padding(5);
            this.butAnimationRanges.Size = new System.Drawing.Size(103, 23);
            this.butAnimationRanges.TabIndex = 1;
            this.butAnimationRanges.Text = "Animation ranges";
            this.butAnimationRanges.Click += new System.EventHandler(this.butAnimationRanges_Click);
            // 
            // butTextureSounds
            // 
            this.butTextureSounds.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butTextureSounds.Location = new System.Drawing.Point(4, 4);
            this.butTextureSounds.Name = "butTextureSounds";
            this.butTextureSounds.Padding = new System.Windows.Forms.Padding(5);
            this.butTextureSounds.Size = new System.Drawing.Size(101, 23);
            this.butTextureSounds.TabIndex = 0;
            this.butTextureSounds.Text = "Texture sounds";
            this.butTextureSounds.Click += new System.EventHandler(this.butTextureSounds_Click);
            // 
            // TexturePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelTextureMap);
            this.Controls.Add(this.panelTextureTools);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Right;
            this.DockText = "Texture Panel";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Name = "TexturePanel";
            this.SerializationKey = "TexturePanel";
            this.Size = new System.Drawing.Size(288, 819);
            this.panelTextureTools.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.PanelTextureMap panelTextureMap;
        private System.Windows.Forms.Panel panelTextureTools;
        private DarkUI.Controls.DarkButton butBump;
        private DarkUI.Controls.DarkButton butAnimationRanges;
        private DarkUI.Controls.DarkButton butTextureSounds;
    }
}
