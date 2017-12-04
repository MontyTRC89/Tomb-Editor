using TombEditor.Controls;

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
            this.components = new System.ComponentModel.Container();
            this.panelTextureMap = new TombEditor.Controls.PanelTextureMap();
            this.panelTextureTools = new System.Windows.Forms.Panel();
            this.butNoTexture = new DarkUI.Controls.DarkButton();
            this.butMirror = new DarkUI.Controls.DarkButton();
            this.butDoubleSide = new DarkUI.Controls.DarkButton();
            this.butTextureSounds = new DarkUI.Controls.DarkButton();
            this.cmbTileSize = new DarkUI.Controls.DarkComboBox();
            this.butAnimationRanges = new DarkUI.Controls.DarkButton();
            this.butRotate = new DarkUI.Controls.DarkButton();
            this.cmbBlending = new DarkUI.Controls.DarkComboBox();
            this.cmbBump = new DarkUI.Controls.DarkComboBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panelTextureTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTextureMap
            // 
            this.panelTextureMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTextureMap.Location = new System.Drawing.Point(0, 25);
            this.panelTextureMap.Name = "panelTextureMap";
            this.panelTextureMap.Size = new System.Drawing.Size(301, 631);
            this.panelTextureMap.TabIndex = 0;
            // 
            // panelTextureTools
            // 
            this.panelTextureTools.Controls.Add(this.butNoTexture);
            this.panelTextureTools.Controls.Add(this.butMirror);
            this.panelTextureTools.Controls.Add(this.butDoubleSide);
            this.panelTextureTools.Controls.Add(this.butTextureSounds);
            this.panelTextureTools.Controls.Add(this.cmbTileSize);
            this.panelTextureTools.Controls.Add(this.butAnimationRanges);
            this.panelTextureTools.Controls.Add(this.butRotate);
            this.panelTextureTools.Controls.Add(this.cmbBlending);
            this.panelTextureTools.Controls.Add(this.cmbBump);
            this.panelTextureTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelTextureTools.Location = new System.Drawing.Point(0, 656);
            this.panelTextureTools.Name = "panelTextureTools";
            this.panelTextureTools.Size = new System.Drawing.Size(301, 53);
            this.panelTextureTools.TabIndex = 10;
            // 
            // butNoTexture
            // 
            this.butNoTexture.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butNoTexture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(84)))), ((int)(((byte)(69)))));
            this.butNoTexture.Image = global::TombEditor.Properties.Resources.texture_NoTexture_1_16;
            this.butNoTexture.Location = new System.Drawing.Point(1, 3);
            this.butNoTexture.Name = "butNoTexture";
            this.butNoTexture.Size = new System.Drawing.Size(24, 23);
            this.butNoTexture.TabIndex = 12;
            this.toolTip.SetToolTip(this.butNoTexture, "No texture");
            this.butNoTexture.Click += new System.EventHandler(this.butNoTexture_Click);
            // 
            // butMirror
            // 
            this.butMirror.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butMirror.Image = global::TombEditor.Properties.Resources.texture_Mirror;
            this.butMirror.Location = new System.Drawing.Point(220, 3);
            this.butMirror.Name = "butMirror";
            this.butMirror.Size = new System.Drawing.Size(23, 23);
            this.butMirror.TabIndex = 11;
            this.toolTip.SetToolTip(this.butMirror, "Mirror texture");
            this.butMirror.Click += new System.EventHandler(this.butMirror_Click);
            // 
            // butDoubleSide
            // 
            this.butDoubleSide.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butDoubleSide.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(84)))), ((int)(((byte)(69)))));
            this.butDoubleSide.Image = global::TombEditor.Properties.Resources.texture_DoubleSided_1_16;
            this.butDoubleSide.Location = new System.Drawing.Point(27, 3);
            this.butDoubleSide.Name = "butDoubleSide";
            this.butDoubleSide.Size = new System.Drawing.Size(24, 23);
            this.butDoubleSide.TabIndex = 10;
            this.toolTip.SetToolTip(this.butDoubleSide, "Double sided");
            this.butDoubleSide.Click += new System.EventHandler(this.butDoubleSide_Click);
            // 
            // butTextureSounds
            // 
            this.butTextureSounds.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butTextureSounds.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butTextureSounds.Location = new System.Drawing.Point(152, 29);
            this.butTextureSounds.Name = "butTextureSounds";
            this.butTextureSounds.Size = new System.Drawing.Size(147, 23);
            this.butTextureSounds.TabIndex = 1;
            this.butTextureSounds.Text = "Texture sounds";
            this.butTextureSounds.Click += new System.EventHandler(this.butTextureSounds_Click);
            // 
            // cmbTileSize
            // 
            this.cmbTileSize.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmbTileSize.FormattingEnabled = true;
            this.cmbTileSize.Items.AddRange(new object[] {
            "32",
            "64",
            "128",
            "256"});
            this.cmbTileSize.Location = new System.Drawing.Point(245, 3);
            this.cmbTileSize.Name = "cmbTileSize";
            this.cmbTileSize.Size = new System.Drawing.Size(54, 23);
            this.cmbTileSize.TabIndex = 9;
            this.cmbTileSize.Text = null;
            this.toolTip.SetToolTip(this.cmbTileSize, "Selection tile size");
            this.cmbTileSize.SelectedIndexChanged += new System.EventHandler(this.cmbTileSize_SelectedIndexChanged);
            // 
            // butAnimationRanges
            // 
            this.butAnimationRanges.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butAnimationRanges.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAnimationRanges.Location = new System.Drawing.Point(1, 29);
            this.butAnimationRanges.Name = "butAnimationRanges";
            this.butAnimationRanges.Size = new System.Drawing.Size(147, 23);
            this.butAnimationRanges.TabIndex = 0;
            this.butAnimationRanges.Text = "Animation ranges";
            this.butAnimationRanges.Click += new System.EventHandler(this.butAnimationRanges_Click);
            // 
            // butRotate
            // 
            this.butRotate.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butRotate.Image = global::TombEditor.Properties.Resources.texture_Rotate;
            this.butRotate.Location = new System.Drawing.Point(195, 3);
            this.butRotate.Name = "butRotate";
            this.butRotate.Size = new System.Drawing.Size(23, 23);
            this.butRotate.TabIndex = 8;
            this.toolTip.SetToolTip(this.butRotate, "Rotate texture");
            this.butRotate.Click += new System.EventHandler(this.butRotate_Click);
            // 
            // cmbBlending
            // 
            this.cmbBlending.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmbBlending.FormattingEnabled = true;
            this.cmbBlending.Items.AddRange(new object[] {
            "Normal",
            "Add",
            "Subtract",
            "Exclude",
            "Screen",
            "Lighten"});
            this.cmbBlending.Location = new System.Drawing.Point(117, 3);
            this.cmbBlending.Name = "cmbBlending";
            this.cmbBlending.Size = new System.Drawing.Size(74, 23);
            this.cmbBlending.TabIndex = 6;
            this.cmbBlending.Text = null;
            this.toolTip.SetToolTip(this.cmbBlending, "Blending mode");
            this.cmbBlending.SelectedIndexChanged += new System.EventHandler(this.cmbBlending_SelectedIndexChanged);
            // 
            // cmbBump
            // 
            this.cmbBump.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmbBump.FormattingEnabled = true;
            this.cmbBump.Items.AddRange(new object[] {
            "None",
            "Level 1",
            "Level 2"});
            this.cmbBump.Location = new System.Drawing.Point(53, 3);
            this.cmbBump.Name = "cmbBump";
            this.cmbBump.Size = new System.Drawing.Size(62, 23);
            this.cmbBump.TabIndex = 7;
            this.cmbBump.Text = null;
            this.toolTip.SetToolTip(this.cmbBump, "Bump mapping");
            this.cmbBump.SelectedIndexChanged += new System.EventHandler(this.cmbBump_SelectedIndexChanged);
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
            this.MinimumSize = new System.Drawing.Size(301, 100);
            this.Name = "TexturePanel";
            this.SerializationKey = "TexturePanel";
            this.Size = new System.Drawing.Size(301, 709);
            this.panelTextureTools.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private PanelTextureMap panelTextureMap;
        private System.Windows.Forms.Panel panelTextureTools;
        private DarkUI.Controls.DarkButton butAnimationRanges;
        private DarkUI.Controls.DarkButton butTextureSounds;
        private DarkUI.Controls.DarkComboBox cmbBump;
        private DarkUI.Controls.DarkComboBox cmbBlending;
        private DarkUI.Controls.DarkButton butRotate;
        private DarkUI.Controls.DarkButton butMirror;
        private DarkUI.Controls.DarkButton butDoubleSide;
        private DarkUI.Controls.DarkComboBox cmbTileSize;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkButton butNoTexture;
    }
}
