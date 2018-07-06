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
            this.butMirror = new DarkUI.Controls.DarkButton();
            this.butDoubleSide = new DarkUI.Controls.DarkButton();
            this.butTextureSounds = new DarkUI.Controls.DarkButton();
            this.cmbTileSize = new DarkUI.Controls.DarkComboBox();
            this.butAnimationRanges = new DarkUI.Controls.DarkButton();
            this.butRotate = new DarkUI.Controls.DarkButton();
            this.cmbBlending = new DarkUI.Controls.DarkComboBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.textureSelectionPanel = new System.Windows.Forms.Panel();
            this.butBrowseTexture = new DarkUI.Controls.DarkButton();
            this.butAddTexture = new DarkUI.Controls.DarkButton();
            this.comboCurrentTexture = new DarkUI.Controls.DarkComboBox();
            this.butDeleteTexture = new DarkUI.Controls.DarkButton();
            this.panelTextureTools.SuspendLayout();
            this.textureSelectionPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTextureMap
            // 
            this.panelTextureMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTextureMap.Location = new System.Drawing.Point(3, 54);
            this.panelTextureMap.Name = "panelTextureMap";
            this.panelTextureMap.Size = new System.Drawing.Size(279, 586);
            this.panelTextureMap.TabIndex = 0;
            // 
            // panelTextureTools
            // 
            this.panelTextureTools.Controls.Add(this.butMirror);
            this.panelTextureTools.Controls.Add(this.butDoubleSide);
            this.panelTextureTools.Controls.Add(this.butTextureSounds);
            this.panelTextureTools.Controls.Add(this.cmbTileSize);
            this.panelTextureTools.Controls.Add(this.butAnimationRanges);
            this.panelTextureTools.Controls.Add(this.butRotate);
            this.panelTextureTools.Controls.Add(this.cmbBlending);
            this.panelTextureTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelTextureTools.Location = new System.Drawing.Point(0, 641);
            this.panelTextureTools.Name = "panelTextureTools";
            this.panelTextureTools.Size = new System.Drawing.Size(286, 56);
            this.panelTextureTools.TabIndex = 10;
            // 
            // butMirror
            // 
            this.butMirror.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butMirror.Image = global::TombEditor.Properties.Resources.texture_Mirror;
            this.butMirror.Location = new System.Drawing.Point(172, 3);
            this.butMirror.Name = "butMirror";
            this.butMirror.Size = new System.Drawing.Size(23, 23);
            this.butMirror.TabIndex = 11;
            this.toolTip.SetToolTip(this.butMirror, "Mirror texture");
            this.butMirror.Click += new System.EventHandler(this.butMirror_Click);
            // 
            // butDoubleSide
            // 
            this.butDoubleSide.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butDoubleSide.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.butDoubleSide.Image = global::TombEditor.Properties.Resources.texture_DoubleSided_1_16;
            this.butDoubleSide.Location = new System.Drawing.Point(3, 3);
            this.butDoubleSide.Name = "butDoubleSide";
            this.butDoubleSide.Size = new System.Drawing.Size(24, 23);
            this.butDoubleSide.TabIndex = 10;
            this.toolTip.SetToolTip(this.butDoubleSide, "Double sided");
            this.butDoubleSide.Click += new System.EventHandler(this.butDoubleSide_Click);
            // 
            // butTextureSounds
            // 
            this.butTextureSounds.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.butTextureSounds.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butTextureSounds.Location = new System.Drawing.Point(145, 30);
            this.butTextureSounds.Name = "butTextureSounds";
            this.butTextureSounds.Size = new System.Drawing.Size(138, 23);
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
            this.cmbTileSize.Location = new System.Drawing.Point(200, 3);
            this.cmbTileSize.Name = "cmbTileSize";
            this.cmbTileSize.Size = new System.Drawing.Size(83, 23);
            this.cmbTileSize.TabIndex = 9;
            this.toolTip.SetToolTip(this.cmbTileSize, "Selection tile size");
            this.cmbTileSize.SelectedIndexChanged += new System.EventHandler(this.cmbTileSize_SelectedIndexChanged);
            // 
            // butAnimationRanges
            // 
            this.butAnimationRanges.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.butAnimationRanges.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAnimationRanges.Location = new System.Drawing.Point(3, 30);
            this.butAnimationRanges.Name = "butAnimationRanges";
            this.butAnimationRanges.Size = new System.Drawing.Size(137, 23);
            this.butAnimationRanges.TabIndex = 0;
            this.butAnimationRanges.Text = "Animation ranges";
            this.butAnimationRanges.Click += new System.EventHandler(this.butAnimationRanges_Click);
            // 
            // butRotate
            // 
            this.butRotate.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butRotate.Image = global::TombEditor.Properties.Resources.texture_Rotate;
            this.butRotate.Location = new System.Drawing.Point(145, 3);
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
            this.cmbBlending.Location = new System.Drawing.Point(32, 3);
            this.cmbBlending.Name = "cmbBlending";
            this.cmbBlending.Size = new System.Drawing.Size(108, 23);
            this.cmbBlending.TabIndex = 6;
            this.toolTip.SetToolTip(this.cmbBlending, "Blending mode");
            this.cmbBlending.SelectedIndexChanged += new System.EventHandler(this.cmbBlending_SelectedIndexChanged);
            // 
            // textureSelectionPanel
            // 
            this.textureSelectionPanel.Controls.Add(this.butBrowseTexture);
            this.textureSelectionPanel.Controls.Add(this.butAddTexture);
            this.textureSelectionPanel.Controls.Add(this.comboCurrentTexture);
            this.textureSelectionPanel.Controls.Add(this.butDeleteTexture);
            this.textureSelectionPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.textureSelectionPanel.Location = new System.Drawing.Point(0, 25);
            this.textureSelectionPanel.Name = "textureSelectionPanel";
            this.textureSelectionPanel.Size = new System.Drawing.Size(286, 28);
            this.textureSelectionPanel.TabIndex = 11;
            // 
            // butBrowseTexture
            // 
            this.butBrowseTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butBrowseTexture.Enabled = false;
            this.butBrowseTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butBrowseTexture.Image = global::TombEditor.Properties.Resources.general_Open_16;
            this.butBrowseTexture.Location = new System.Drawing.Point(229, 3);
            this.butBrowseTexture.Name = "butBrowseTexture";
            this.butBrowseTexture.Size = new System.Drawing.Size(24, 23);
            this.butBrowseTexture.TabIndex = 3;
            this.butBrowseTexture.Click += new System.EventHandler(this.butBrowseTexture_Click);
            // 
            // butAddTexture
            // 
            this.butAddTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddTexture.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.butAddTexture.Location = new System.Drawing.Point(200, 3);
            this.butAddTexture.Name = "butAddTexture";
            this.butAddTexture.Size = new System.Drawing.Size(24, 23);
            this.butAddTexture.TabIndex = 2;
            this.butAddTexture.Click += new System.EventHandler(this.butAddTexture_Click);
            // 
            // comboCurrentTexture
            // 
            this.comboCurrentTexture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboCurrentTexture.FormattingEnabled = true;
            this.comboCurrentTexture.Location = new System.Drawing.Point(3, 3);
            this.comboCurrentTexture.Name = "comboCurrentTexture";
            this.comboCurrentTexture.Size = new System.Drawing.Size(192, 23);
            this.comboCurrentTexture.TabIndex = 0;
            this.comboCurrentTexture.SelectedValueChanged += new System.EventHandler(this.comboCurrentTexture_SelectedValueChanged);
            // 
            // butDeleteTexture
            // 
            this.butDeleteTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butDeleteTexture.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.butDeleteTexture.Location = new System.Drawing.Point(258, 3);
            this.butDeleteTexture.Name = "butDeleteTexture";
            this.butDeleteTexture.Size = new System.Drawing.Size(24, 23);
            this.butDeleteTexture.TabIndex = 2;
            this.butDeleteTexture.Click += new System.EventHandler(this.butDeleteTexture_Click);
            // 
            // TexturePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textureSelectionPanel);
            this.Controls.Add(this.panelTextureMap);
            this.Controls.Add(this.panelTextureTools);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Right;
            this.DockText = "Texture Panel";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(286, 100);
            this.Name = "TexturePanel";
            this.SerializationKey = "TexturePanel";
            this.Size = new System.Drawing.Size(286, 697);
            this.panelTextureTools.ResumeLayout(false);
            this.textureSelectionPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private PanelTextureMap panelTextureMap;
        private System.Windows.Forms.Panel panelTextureTools;
        private DarkUI.Controls.DarkButton butAnimationRanges;
        private DarkUI.Controls.DarkButton butTextureSounds;
        private DarkUI.Controls.DarkComboBox cmbBlending;
        private DarkUI.Controls.DarkButton butRotate;
        private DarkUI.Controls.DarkButton butMirror;
        private DarkUI.Controls.DarkButton butDoubleSide;
        private DarkUI.Controls.DarkComboBox cmbTileSize;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Panel textureSelectionPanel;
        private DarkUI.Controls.DarkComboBox comboCurrentTexture;
        private DarkUI.Controls.DarkButton butAddTexture;
        private DarkUI.Controls.DarkButton butDeleteTexture;
        private DarkUI.Controls.DarkButton butBrowseTexture;
    }
}
