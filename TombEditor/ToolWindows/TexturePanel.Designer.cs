﻿using TombEditor.Controls;

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
            this.panelTextureTools = new System.Windows.Forms.Panel();
            this.butBumpMaps = new DarkUI.Controls.DarkButton();
            this.butMirror = new DarkUI.Controls.DarkButton();
            this.butDoubleSide = new DarkUI.Controls.DarkButton();
            this.butTextureSounds = new DarkUI.Controls.DarkButton();
            this.cmbTileSize = new DarkUI.Controls.DarkComboBox();
            this.butAnimationRanges = new DarkUI.Controls.DarkButton();
            this.butRotate = new DarkUI.Controls.DarkButton();
            this.cmbBlending = new DarkUI.Controls.DarkComboBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.butBrowseTexture = new DarkUI.Controls.DarkButton();
            this.butDeleteTexture = new DarkUI.Controls.DarkButton();
            this.textureSelectionPanel = new System.Windows.Forms.Panel();
            this.butAddTexture = new DarkUI.Controls.DarkButton();
            this.comboCurrentTexture = new DarkUI.Controls.DarkComboBox();
            this.panelTextureMap = new TombEditor.Controls.PanelTextureMap();
            this.butSearch = new DarkUI.Controls.DarkButton();
            this.panelTextureTools.SuspendLayout();
            this.textureSelectionPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTextureTools
            // 
            this.panelTextureTools.Controls.Add(this.butBumpMaps);
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
            // butBumpMaps
            // 
            this.butBumpMaps.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.butBumpMaps.Checked = false;
            this.butBumpMaps.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butBumpMaps.Location = new System.Drawing.Point(193, 30);
            this.butBumpMaps.Name = "butBumpMaps";
            this.butBumpMaps.Size = new System.Drawing.Size(90, 23);
            this.butBumpMaps.TabIndex = 12;
            this.butBumpMaps.Text = "Bumpmaps";
            this.toolTip.SetToolTip(this.butBumpMaps, "Edit bumpmaps...");
            this.butBumpMaps.Click += new System.EventHandler(this.butBumpMaps_Click);
            // 
            // butMirror
            // 
            this.butMirror.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butMirror.Checked = false;
            this.butMirror.Image = global::TombEditor.Properties.Resources.texture_Mirror;
            this.butMirror.Location = new System.Drawing.Point(173, 3);
            this.butMirror.Name = "butMirror";
            this.butMirror.Size = new System.Drawing.Size(23, 23);
            this.butMirror.TabIndex = 8;
            this.butMirror.Tag = "MirrorTexture";
            // 
            // butDoubleSide
            // 
            this.butDoubleSide.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butDoubleSide.Checked = false;
            this.butDoubleSide.Image = global::TombEditor.Properties.Resources.texture_DoubleSided_1_16;
            this.butDoubleSide.Location = new System.Drawing.Point(3, 3);
            this.butDoubleSide.Name = "butDoubleSide";
            this.butDoubleSide.Size = new System.Drawing.Size(24, 23);
            this.butDoubleSide.TabIndex = 5;
            this.butDoubleSide.Tag = "SetTextureDoubleSided";
            // 
            // butTextureSounds
            // 
            this.butTextureSounds.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.butTextureSounds.Checked = false;
            this.butTextureSounds.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butTextureSounds.Location = new System.Drawing.Point(98, 30);
            this.butTextureSounds.Name = "butTextureSounds";
            this.butTextureSounds.Size = new System.Drawing.Size(90, 23);
            this.butTextureSounds.TabIndex = 11;
            this.butTextureSounds.Text = "Sounds";
            this.toolTip.SetToolTip(this.butTextureSounds, "Edit texture sounds...");
            this.butTextureSounds.Click += new System.EventHandler(this.butTextureSounds_Click);
            // 
            // cmbTileSize
            // 
            this.cmbTileSize.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cmbTileSize.FormattingEnabled = true;
            this.cmbTileSize.Location = new System.Drawing.Point(201, 3);
            this.cmbTileSize.Name = "cmbTileSize";
            this.cmbTileSize.Size = new System.Drawing.Size(82, 23);
            this.cmbTileSize.TabIndex = 9;
            this.toolTip.SetToolTip(this.cmbTileSize, "Selection tile size");
            this.cmbTileSize.SelectionChangeCommitted += new System.EventHandler(this.cmbTileSize_SelectionChangeCommitted);
            // 
            // butAnimationRanges
            // 
            this.butAnimationRanges.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.butAnimationRanges.Checked = false;
            this.butAnimationRanges.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAnimationRanges.Location = new System.Drawing.Point(3, 30);
            this.butAnimationRanges.Name = "butAnimationRanges";
            this.butAnimationRanges.Size = new System.Drawing.Size(90, 23);
            this.butAnimationRanges.TabIndex = 10;
            this.butAnimationRanges.Tag = "EditAnimationRanges";
            this.butAnimationRanges.Text = "Animations";
            // 
            // butRotate
            // 
            this.butRotate.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butRotate.Checked = false;
            this.butRotate.Image = global::TombEditor.Properties.Resources.texture_Rotate;
            this.butRotate.Location = new System.Drawing.Point(146, 3);
            this.butRotate.Name = "butRotate";
            this.butRotate.Size = new System.Drawing.Size(23, 23);
            this.butRotate.TabIndex = 7;
            this.butRotate.Tag = "RotateTexture";
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
            this.cmbBlending.Location = new System.Drawing.Point(31, 3);
            this.cmbBlending.Name = "cmbBlending";
            this.cmbBlending.Size = new System.Drawing.Size(110, 23);
            this.cmbBlending.TabIndex = 6;
            this.toolTip.SetToolTip(this.cmbBlending, "Blending mode");
            this.cmbBlending.SelectedIndexChanged += new System.EventHandler(this.cmbBlending_SelectedIndexChanged);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // butBrowseTexture
            // 
            this.butBrowseTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butBrowseTexture.Checked = false;
            this.butBrowseTexture.Enabled = false;
            this.butBrowseTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butBrowseTexture.Image = global::TombEditor.Properties.Resources.actions_refresh_16;
            this.butBrowseTexture.Location = new System.Drawing.Point(229, 3);
            this.butBrowseTexture.Name = "butBrowseTexture";
            this.butBrowseTexture.Size = new System.Drawing.Size(24, 23);
            this.butBrowseTexture.TabIndex = 2;
            this.toolTip.SetToolTip(this.butBrowseTexture, "Replace texture...");
            this.butBrowseTexture.Click += new System.EventHandler(this.butBrowseTexture_Click);
            // 
            // butDeleteTexture
            // 
            this.butDeleteTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteTexture.Checked = false;
            this.butDeleteTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butDeleteTexture.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.butDeleteTexture.Location = new System.Drawing.Point(258, 3);
            this.butDeleteTexture.Name = "butDeleteTexture";
            this.butDeleteTexture.Size = new System.Drawing.Size(24, 23);
            this.butDeleteTexture.TabIndex = 3;
            this.toolTip.SetToolTip(this.butDeleteTexture, "Delete texture");
            this.butDeleteTexture.Click += new System.EventHandler(this.butDeleteTexture_Click);
            // 
            // textureSelectionPanel
            // 
            this.textureSelectionPanel.Controls.Add(this.butSearch);
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
            // butAddTexture
            // 
            this.butAddTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddTexture.Checked = false;
            this.butAddTexture.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddTexture.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.butAddTexture.Location = new System.Drawing.Point(200, 3);
            this.butAddTexture.Name = "butAddTexture";
            this.butAddTexture.Size = new System.Drawing.Size(24, 23);
            this.butAddTexture.TabIndex = 1;
            this.butAddTexture.Tag = "AddTexture";
            // 
            // comboCurrentTexture
            // 
            this.comboCurrentTexture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboCurrentTexture.FormattingEnabled = true;
            this.comboCurrentTexture.Location = new System.Drawing.Point(3, 3);
            this.comboCurrentTexture.Name = "comboCurrentTexture";
            this.comboCurrentTexture.Size = new System.Drawing.Size(169, 23);
            this.comboCurrentTexture.TabIndex = 0;
            this.comboCurrentTexture.SelectedValueChanged += new System.EventHandler(this.comboCurrentTexture_SelectedValueChanged);
            // 
            // panelTextureMap
            // 
            this.panelTextureMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTextureMap.Location = new System.Drawing.Point(3, 54);
            this.panelTextureMap.Name = "panelTextureMap";
            this.panelTextureMap.Size = new System.Drawing.Size(279, 586);
            this.panelTextureMap.TabIndex = 4;
            // 
            // butSearch
            // 
            this.butSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSearch.Checked = false;
            this.butSearch.Image = global::TombEditor.Properties.Resources.general_search_16;
            this.butSearch.Location = new System.Drawing.Point(171, 3);
            this.butSearch.Name = "butSearch";
            this.butSearch.Selectable = false;
            this.butSearch.Size = new System.Drawing.Size(24, 23);
            this.butSearch.TabIndex = 4;
            this.toolTip.SetToolTip(this.butSearch, "Search for texture sets");
            this.butSearch.Click += new System.EventHandler(this.butSearch_Click);
            // 
            // TexturePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelTextureMap);
            this.Controls.Add(this.textureSelectionPanel);
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
        private DarkUI.Controls.DarkButton butBumpMaps;
        private PanelTextureMap panelTextureMap;
        private DarkUI.Controls.DarkButton butSearch;
    }
}
