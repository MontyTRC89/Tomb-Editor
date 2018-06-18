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
            this.panelTextureMap = new TombEditor.Controls.PanelTextureMap();
            this.panelTextureTools = new System.Windows.Forms.Panel();
            this.panelTileSizeSelector = new System.Windows.Forms.Panel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.rbTileSize256 = new DarkUI.Controls.DarkRadioButton();
            this.rbTileSize128 = new DarkUI.Controls.DarkRadioButton();
            this.rbTileSize64 = new DarkUI.Controls.DarkRadioButton();
            this.rbTileSize32 = new DarkUI.Controls.DarkRadioButton();
            this.butBump = new DarkUI.Controls.DarkButton();
            this.butTextureSounds = new DarkUI.Controls.DarkButton();
            this.butAnimationRanges = new DarkUI.Controls.DarkButton();
            this.textureSelectionPanel = new System.Windows.Forms.Panel();
            this.butBrowseTexture = new DarkUI.Controls.DarkButton();
            this.butAddTexture = new DarkUI.Controls.DarkButton();
            this.comboCurrentTexture = new DarkUI.Controls.DarkComboBox();
            this.butDeleteTexture = new DarkUI.Controls.DarkButton();
            this.panelTextureTools.SuspendLayout();
            this.panelTileSizeSelector.SuspendLayout();
            this.textureSelectionPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // panelTextureMap
            //
            this.panelTextureMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTextureMap.Location = new System.Drawing.Point(0, 54);
            this.panelTextureMap.Name = "panelTextureMap";
            this.panelTextureMap.Size = new System.Drawing.Size(284, 709);
            this.panelTextureMap.TabIndex = 0;
            //
            // panelTextureTools
            //
            this.panelTextureTools.Controls.Add(this.panelTileSizeSelector);
            this.panelTextureTools.Controls.Add(this.butBump);
            this.panelTextureTools.Controls.Add(this.butTextureSounds);
            this.panelTextureTools.Controls.Add(this.butAnimationRanges);
            this.panelTextureTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelTextureTools.Location = new System.Drawing.Point(0, 764);
            this.panelTextureTools.Name = "panelTextureTools";
            this.panelTextureTools.Size = new System.Drawing.Size(284, 50);
            this.panelTextureTools.TabIndex = 10;
            //
            // panelTileSizeSelector
            //
            this.panelTileSizeSelector.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.panelTileSizeSelector.Controls.Add(this.darkLabel1);
            this.panelTileSizeSelector.Controls.Add(this.rbTileSize256);
            this.panelTileSizeSelector.Controls.Add(this.rbTileSize128);
            this.panelTileSizeSelector.Controls.Add(this.rbTileSize64);
            this.panelTileSizeSelector.Controls.Add(this.rbTileSize32);
            this.panelTileSizeSelector.Location = new System.Drawing.Point(0, 0);
            this.panelTileSizeSelector.Name = "panelTileSizeSelector";
            this.panelTileSizeSelector.Size = new System.Drawing.Size(282, 25);
            this.panelTileSizeSelector.TabIndex = 3;
            //
            // darkLabel1
            //
            this.darkLabel1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(5, 6);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(98, 13);
            this.darkLabel1.TabIndex = 3;
            this.darkLabel1.Text = "Selection tile size:";
            //
            // rbTileSize256
            //
            this.rbTileSize256.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.rbTileSize256.AutoSize = true;
            this.rbTileSize256.Location = new System.Drawing.Point(239, 4);
            this.rbTileSize256.Name = "rbTileSize256";
            this.rbTileSize256.Size = new System.Drawing.Size(43, 17);
            this.rbTileSize256.TabIndex = 2;
            this.rbTileSize256.TabStop = true;
            this.rbTileSize256.Text = "256";
            this.rbTileSize256.CheckedChanged += new System.EventHandler(this.rbTileSize256_CheckedChanged);
            //
            // rbTileSize128
            //
            this.rbTileSize128.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.rbTileSize128.AutoSize = true;
            this.rbTileSize128.Location = new System.Drawing.Point(192, 4);
            this.rbTileSize128.Name = "rbTileSize128";
            this.rbTileSize128.Size = new System.Drawing.Size(43, 17);
            this.rbTileSize128.TabIndex = 1;
            this.rbTileSize128.TabStop = true;
            this.rbTileSize128.Text = "128";
            this.rbTileSize128.CheckedChanged += new System.EventHandler(this.rbTileSize128_CheckedChanged);
            //
            // rbTileSize64
            //
            this.rbTileSize64.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.rbTileSize64.AutoSize = true;
            this.rbTileSize64.Location = new System.Drawing.Point(150, 4);
            this.rbTileSize64.Name = "rbTileSize64";
            this.rbTileSize64.Size = new System.Drawing.Size(37, 17);
            this.rbTileSize64.TabIndex = 0;
            this.rbTileSize64.TabStop = true;
            this.rbTileSize64.Text = "64";
            this.rbTileSize64.CheckedChanged += new System.EventHandler(this.rbTileSize64_CheckedChanged);
            //
            // rbTileSize32
            //
            this.rbTileSize32.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.rbTileSize32.AutoSize = true;
            this.rbTileSize32.Location = new System.Drawing.Point(107, 4);
            this.rbTileSize32.Name = "rbTileSize32";
            this.rbTileSize32.Size = new System.Drawing.Size(37, 17);
            this.rbTileSize32.TabIndex = 4;
            this.rbTileSize32.TabStop = true;
            this.rbTileSize32.Text = "32";
            this.rbTileSize32.CheckedChanged += new System.EventHandler(this.rbTileSize32_CheckedChanged);
            //
            // butBump
            //
            this.butBump.Location = new System.Drawing.Point(0, 0);
            this.butBump.Name = "butBump";
            this.butBump.Size = new System.Drawing.Size(75, 23);
            this.butBump.TabIndex = 4;
            this.butBump.Visible = false;
            //
            // butTextureSounds
            //
            this.butTextureSounds.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butTextureSounds.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butTextureSounds.Location = new System.Drawing.Point(144, 27);
            this.butTextureSounds.Name = "butTextureSounds";
            this.butTextureSounds.Size = new System.Drawing.Size(138, 23);
            this.butTextureSounds.TabIndex = 1;
            this.butTextureSounds.Text = "Texture sounds";
            this.butTextureSounds.Click += new System.EventHandler(this.butTextureSounds_Click);
            //
            // butAnimationRanges
            //
            this.butAnimationRanges.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butAnimationRanges.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAnimationRanges.Location = new System.Drawing.Point(3, 27);
            this.butAnimationRanges.Name = "butAnimationRanges";
            this.butAnimationRanges.Size = new System.Drawing.Size(138, 23);
            this.butAnimationRanges.TabIndex = 0;
            this.butAnimationRanges.Text = "Animation ranges";
            this.butAnimationRanges.Click += new System.EventHandler(this.butAnimationRanges_Click);
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
            this.textureSelectionPanel.Size = new System.Drawing.Size(284, 28);
            this.textureSelectionPanel.TabIndex = 11;
            //
            // butBrowseTexture
            //
            this.butBrowseTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butBrowseTexture.Enabled = false;
            this.butBrowseTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butBrowseTexture.Image = global::TombEditor.Properties.Resources.general_Open_16;
            this.butBrowseTexture.Location = new System.Drawing.Point(226, 3);
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
            this.butAddTexture.Location = new System.Drawing.Point(197, 3);
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
            this.comboCurrentTexture.Location = new System.Drawing.Point(5, 3);
            this.comboCurrentTexture.Name = "comboCurrentTexture";
            this.comboCurrentTexture.Size = new System.Drawing.Size(186, 23);
            this.comboCurrentTexture.TabIndex = 0;
            this.comboCurrentTexture.SelectedValueChanged += new System.EventHandler(this.comboCurrentTexture_SelectedValueChanged);
            this.comboCurrentTexture.DropDown += comboCurrentTexture_DropDown;
            //
            // butDeleteTexture
            //
            this.butDeleteTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butDeleteTexture.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.butDeleteTexture.Location = new System.Drawing.Point(255, 3);
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
            this.MinimumSize = new System.Drawing.Size(284, 100);
            this.Name = "TexturePanel";
            this.SerializationKey = "TexturePanel";
            this.Size = new System.Drawing.Size(284, 814);
            this.panelTextureTools.ResumeLayout(false);
            this.panelTileSizeSelector.ResumeLayout(false);
            this.panelTileSizeSelector.PerformLayout();
            this.textureSelectionPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private PanelTextureMap panelTextureMap;
        private System.Windows.Forms.Panel panelTextureTools;
        private DarkUI.Controls.DarkButton butBump;
        private DarkUI.Controls.DarkButton butAnimationRanges;
        private DarkUI.Controls.DarkButton butTextureSounds;
        private System.Windows.Forms.Panel panelTileSizeSelector;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkRadioButton rbTileSize256;
        private DarkUI.Controls.DarkRadioButton rbTileSize128;
        private DarkUI.Controls.DarkRadioButton rbTileSize64;
        private DarkUI.Controls.DarkRadioButton rbTileSize32;
        private System.Windows.Forms.Panel textureSelectionPanel;
        private DarkUI.Controls.DarkComboBox comboCurrentTexture;
        private DarkUI.Controls.DarkButton butAddTexture;
        private DarkUI.Controls.DarkButton butDeleteTexture;
        private DarkUI.Controls.DarkButton butBrowseTexture;
    }
}
