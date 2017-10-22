using DarkUI.Controls;
using System.Windows.Forms;

namespace TombEditor
{
    partial class FormAnimatedTextures
    {
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.comboAnimatedTextureSets = new DarkUI.Controls.DarkComboBox();
            this.butAnimatedTextureSetDelete = new DarkUI.Controls.DarkButton();
            this.butAnimatedTextureSetNew = new DarkUI.Controls.DarkButton();
            this.comboEffect = new DarkUI.Controls.DarkComboBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.previewImage = new System.Windows.Forms.PictureBox();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.textureMap = new TombEditor.FormAnimatedTextures.PanelTextureMapForAnimations();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.animationSetSetupGroup = new DarkUI.Controls.DarkGroupBox();
            this.previewProgressBar = new DarkUI.Controls.DarkProgressBar();
            this.texturesDataGridView = new DarkUI.Controls.DarkDataGridView();
            this.texturesDataGridViewColumnImage = new System.Windows.Forms.DataGridViewImageColumn();
            this.texturesDataGridViewColumnRepeat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.texturesDataGridViewColumnTexture = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.texturesDataGridViewColumnArea = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.texturesDataGridViewColumnTexCoord0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.texturesDataGridViewColumnTexCoord1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.texturesDataGridViewColumnTexCoord2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.texturesDataGridViewColumnTexCoord3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.butUpdate = new DarkUI.Controls.DarkButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.texturesDataGridViewControls = new TombEditor.Controls.DarkDataGridViewControls();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.tooManyFramesWarning = new DarkUI.Controls.DarkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.previewImage)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.animationSetSetupGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.texturesDataGridView)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // darkLabel1
            // 
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(3, 9);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(125, 24);
            this.darkLabel1.TabIndex = 0;
            this.darkLabel1.Text = "Current animation set:";
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboAnimatedTextureSets
            // 
            this.comboAnimatedTextureSets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboAnimatedTextureSets.ItemHeight = 18;
            this.comboAnimatedTextureSets.Location = new System.Drawing.Point(129, 9);
            this.comboAnimatedTextureSets.Name = "comboAnimatedTextureSets";
            this.comboAnimatedTextureSets.Size = new System.Drawing.Size(263, 24);
            this.comboAnimatedTextureSets.TabIndex = 1;
            this.comboAnimatedTextureSets.Text = null;
            this.comboAnimatedTextureSets.SelectedIndexChanged += new System.EventHandler(this.comboAnimatedTextureSets_SelectedIndexChanged);
            // 
            // butAnimatedTextureSetDelete
            // 
            this.butAnimatedTextureSetDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butAnimatedTextureSetDelete.Enabled = false;
            this.butAnimatedTextureSetDelete.Location = new System.Drawing.Point(134, 0);
            this.butAnimatedTextureSetDelete.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.butAnimatedTextureSetDelete.Name = "butAnimatedTextureSetDelete";
            this.butAnimatedTextureSetDelete.Size = new System.Drawing.Size(129, 23);
            this.butAnimatedTextureSetDelete.TabIndex = 3;
            this.butAnimatedTextureSetDelete.Text = "Delete anim set";
            this.butAnimatedTextureSetDelete.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAnimatedTextureSetDelete.Click += new System.EventHandler(this.butAnimatedTextureSetDelete_Click);
            // 
            // butAnimatedTextureSetNew
            // 
            this.butAnimatedTextureSetNew.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butAnimatedTextureSetNew.Location = new System.Drawing.Point(0, 0);
            this.butAnimatedTextureSetNew.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.butAnimatedTextureSetNew.Name = "butAnimatedTextureSetNew";
            this.butAnimatedTextureSetNew.Size = new System.Drawing.Size(129, 23);
            this.butAnimatedTextureSetNew.TabIndex = 2;
            this.butAnimatedTextureSetNew.Text = "New anim set";
            this.butAnimatedTextureSetNew.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAnimatedTextureSetNew.Click += new System.EventHandler(this.butAnimatedTextureSetNew_Click);
            // 
            // comboEffect
            // 
            this.comboEffect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboEffect.Items.AddRange(new object[] {
            "Normal",
            "Half Rotate",
            "Full Rotate"});
            this.comboEffect.Location = new System.Drawing.Point(70, 12);
            this.comboEffect.Name = "comboEffect";
            this.comboEffect.Size = new System.Drawing.Size(163, 23);
            this.comboEffect.TabIndex = 7;
            this.comboEffect.Text = "Normal";
            // 
            // darkLabel3
            // 
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(9, 12);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(53, 23);
            this.darkLabel3.TabIndex = 6;
            this.darkLabel3.Text = "Effect:";
            this.darkLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // previewImage
            // 
            this.previewImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.previewImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.previewImage.Location = new System.Drawing.Point(250, 392);
            this.previewImage.Name = "previewImage";
            this.previewImage.Size = new System.Drawing.Size(130, 130);
            this.previewImage.TabIndex = 0;
            this.previewImage.TabStop = false;
            // 
            // darkLabel4
            // 
            this.darkLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(247, 371);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(133, 18);
            this.darkLabel4.TabIndex = 0;
            this.darkLabel4.Text = "Preview";
            this.darkLabel4.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // textureMap
            // 
            this.textureMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textureMap.Location = new System.Drawing.Point(404, 3);
            this.textureMap.Name = "textureMap";
            this.textureMap.Size = new System.Drawing.Size(395, 612);
            this.textureMap.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textureMap, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(802, 618);
            this.tableLayoutPanel1.TabIndex = 43;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel2);
            this.panel1.Controls.Add(this.darkLabel1);
            this.panel1.Controls.Add(this.comboAnimatedTextureSets);
            this.panel1.Controls.Add(this.animationSetSetupGroup);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(395, 613);
            this.panel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.butAnimatedTextureSetNew, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.butAnimatedTextureSetDelete, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(129, 39);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(263, 23);
            this.tableLayoutPanel2.TabIndex = 43;
            // 
            // animationSetSetupGroup
            // 
            this.animationSetSetupGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.animationSetSetupGroup.Controls.Add(this.tooManyFramesWarning);
            this.animationSetSetupGroup.Controls.Add(this.previewProgressBar);
            this.animationSetSetupGroup.Controls.Add(this.texturesDataGridView);
            this.animationSetSetupGroup.Controls.Add(this.darkLabel2);
            this.animationSetSetupGroup.Controls.Add(this.butUpdate);
            this.animationSetSetupGroup.Controls.Add(this.darkLabel4);
            this.animationSetSetupGroup.Controls.Add(this.panel2);
            this.animationSetSetupGroup.Controls.Add(this.previewImage);
            this.animationSetSetupGroup.Controls.Add(this.texturesDataGridViewControls);
            this.animationSetSetupGroup.Enabled = false;
            this.animationSetSetupGroup.Location = new System.Drawing.Point(6, 68);
            this.animationSetSetupGroup.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.animationSetSetupGroup.Name = "animationSetSetupGroup";
            this.animationSetSetupGroup.Size = new System.Drawing.Size(386, 544);
            this.animationSetSetupGroup.TabIndex = 42;
            this.animationSetSetupGroup.TabStop = false;
            this.animationSetSetupGroup.Text = "Animation set setup";
            // 
            // previewProgressBar
            // 
            this.previewProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.previewProgressBar.Location = new System.Drawing.Point(250, 522);
            this.previewProgressBar.Name = "previewProgressBar";
            this.previewProgressBar.Size = new System.Drawing.Size(130, 16);
            this.previewProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.previewProgressBar.TabIndex = 8;
            this.previewProgressBar.TextMode = DarkUI.Controls.DarkProgressBarMode.XOfN;
            // 
            // texturesDataGridView
            // 
            this.texturesDataGridView.AllowUserToAddRows = false;
            this.texturesDataGridView.AllowUserToOrderColumns = true;
            this.texturesDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.texturesDataGridView.AutoGenerateColumns = false;
            this.texturesDataGridView.ColumnHeadersHeight = 17;
            this.texturesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.texturesDataGridViewColumnImage,
            this.texturesDataGridViewColumnRepeat,
            this.texturesDataGridViewColumnTexture,
            this.texturesDataGridViewColumnArea,
            this.texturesDataGridViewColumnTexCoord0,
            this.texturesDataGridViewColumnTexCoord1,
            this.texturesDataGridViewColumnTexCoord2,
            this.texturesDataGridViewColumnTexCoord3});
            this.texturesDataGridView.Location = new System.Drawing.Point(6, 21);
            this.texturesDataGridView.Name = "texturesDataGridView";
            this.texturesDataGridView.RowHeadersWidth = 41;
            this.texturesDataGridView.RowTemplate.Height = 48;
            this.texturesDataGridView.Size = new System.Drawing.Size(267, 350);
            this.texturesDataGridView.TabIndex = 4;
            this.texturesDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.texturesDataGridView_CellFormatting);
            this.texturesDataGridView.CellParsing += new System.Windows.Forms.DataGridViewCellParsingEventHandler(this.texturesDataGridView_CellParsing);
            this.texturesDataGridView.SelectionChanged += new System.EventHandler(this.texturesDataGridView_SelectionChanged);
            // 
            // texturesDataGridViewColumnImage
            // 
            this.texturesDataGridViewColumnImage.HeaderText = "Image";
            this.texturesDataGridViewColumnImage.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.texturesDataGridViewColumnImage.Name = "texturesDataGridViewColumnImage";
            this.texturesDataGridViewColumnImage.ReadOnly = true;
            this.texturesDataGridViewColumnImage.Width = 48;
            // 
            // texturesDataGridViewColumnRepeat
            // 
            this.texturesDataGridViewColumnRepeat.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.texturesDataGridViewColumnRepeat.DataPropertyName = "Repeat";
            this.texturesDataGridViewColumnRepeat.HeaderText = "Repeat";
            this.texturesDataGridViewColumnRepeat.Name = "texturesDataGridViewColumnRepeat";
            this.texturesDataGridViewColumnRepeat.Width = 67;
            // 
            // texturesDataGridViewColumnTexture
            // 
            this.texturesDataGridViewColumnTexture.DataPropertyName = "Texture";
            this.texturesDataGridViewColumnTexture.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.texturesDataGridViewColumnTexture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.texturesDataGridViewColumnTexture.HeaderText = "Texture";
            this.texturesDataGridViewColumnTexture.Name = "texturesDataGridViewColumnTexture";
            this.texturesDataGridViewColumnTexture.Width = 80;
            // 
            // texturesDataGridViewColumnArea
            // 
            this.texturesDataGridViewColumnArea.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.texturesDataGridViewColumnArea.HeaderText = "Area";
            this.texturesDataGridViewColumnArea.Name = "texturesDataGridViewColumnArea";
            this.texturesDataGridViewColumnArea.ReadOnly = true;
            this.texturesDataGridViewColumnArea.Width = 54;
            // 
            // texturesDataGridViewColumnTexCoord0
            // 
            this.texturesDataGridViewColumnTexCoord0.DataPropertyName = "TexCoord0";
            this.texturesDataGridViewColumnTexCoord0.HeaderText = "Edge 0";
            this.texturesDataGridViewColumnTexCoord0.Name = "texturesDataGridViewColumnTexCoord0";
            this.texturesDataGridViewColumnTexCoord0.Width = 70;
            // 
            // texturesDataGridViewColumnTexCoord1
            // 
            this.texturesDataGridViewColumnTexCoord1.DataPropertyName = "TexCoord1";
            this.texturesDataGridViewColumnTexCoord1.HeaderText = "Edge 1";
            this.texturesDataGridViewColumnTexCoord1.Name = "texturesDataGridViewColumnTexCoord1";
            this.texturesDataGridViewColumnTexCoord1.Width = 70;
            // 
            // texturesDataGridViewColumnTexCoord2
            // 
            this.texturesDataGridViewColumnTexCoord2.DataPropertyName = "TexCoord2";
            this.texturesDataGridViewColumnTexCoord2.HeaderText = "Edge 2";
            this.texturesDataGridViewColumnTexCoord2.Name = "texturesDataGridViewColumnTexCoord2";
            this.texturesDataGridViewColumnTexCoord2.Width = 70;
            // 
            // texturesDataGridViewColumnTexCoord3
            // 
            this.texturesDataGridViewColumnTexCoord3.DataPropertyName = "TexCoord3";
            this.texturesDataGridViewColumnTexCoord3.HeaderText = "Edge 3";
            this.texturesDataGridViewColumnTexCoord3.Name = "texturesDataGridViewColumnTexCoord3";
            this.texturesDataGridViewColumnTexCoord3.Width = 70;
            // 
            // darkLabel2
            // 
            this.darkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(6, 371);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(238, 18);
            this.darkLabel2.TabIndex = 0;
            this.darkLabel2.Text = "Settings";
            this.darkLabel2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // butUpdate
            // 
            this.butUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butUpdate.Location = new System.Drawing.Point(279, 73);
            this.butUpdate.Name = "butUpdate";
            this.butUpdate.Size = new System.Drawing.Size(101, 20);
            this.butUpdate.TabIndex = 2;
            this.butUpdate.Text = "<--- Update ---|";
            this.butUpdate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butUpdate.Click += new System.EventHandler(this.butUpdate_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.darkLabel5);
            this.panel2.Controls.Add(this.darkLabel3);
            this.panel2.Controls.Add(this.comboEffect);
            this.panel2.Location = new System.Drawing.Point(6, 392);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(238, 146);
            this.panel2.TabIndex = 0;
            // 
            // darkLabel5
            // 
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(3, 10);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(230, 81);
            this.darkLabel5.TabIndex = 6;
            this.darkLabel5.Text = "To Do";
            this.darkLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // texturesDataGridViewControls
            // 
            this.texturesDataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.texturesDataGridViewControls.Enabled = false;
            this.texturesDataGridViewControls.Location = new System.Drawing.Point(279, 21);
            this.texturesDataGridViewControls.MinimumSize = new System.Drawing.Size(92, 100);
            this.texturesDataGridViewControls.Name = "texturesDataGridViewControls";
            this.texturesDataGridViewControls.NewName = "<--- New ---|";
            this.texturesDataGridViewControls.Size = new System.Drawing.Size(101, 350);
            this.texturesDataGridViewControls.TabIndex = 5;
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Location = new System.Drawing.Point(599, 621);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(203, 23);
            this.butOk.TabIndex = 8;
            this.butOk.Text = "Ok";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // tooManyFramesWarning
            // 
            this.tooManyFramesWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tooManyFramesWarning.BackColor = System.Drawing.Color.Firebrick;
            this.tooManyFramesWarning.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tooManyFramesWarning.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tooManyFramesWarning.Location = new System.Drawing.Point(279, 96);
            this.tooManyFramesWarning.Name = "tooManyFramesWarning";
            this.tooManyFramesWarning.Size = new System.Drawing.Size(101, 223);
            this.tooManyFramesWarning.TabIndex = 9;
            this.tooManyFramesWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.tooManyFramesWarning.Visible = false;
            // 
            // FormAnimatedTextures
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 647);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.butOk);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(691, 500);
            this.Name = "FormAnimatedTextures";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Animated textures";
            ((System.ComponentModel.ISupportInitialize)(this.previewImage)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.animationSetSetupGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.texturesDataGridView)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private DarkLabel darkLabel1;
        private DarkComboBox comboAnimatedTextureSets;
        private DarkButton butAnimatedTextureSetDelete;
        private DarkButton butAnimatedTextureSetNew;
        private DarkComboBox comboEffect;
        private DarkLabel darkLabel3;
        private PictureBox previewImage;
        private DarkLabel darkLabel4;
        private PanelTextureMapForAnimations textureMap;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private DarkButton butOk;
        private DarkGroupBox animationSetSetupGroup;
        private DarkLabel darkLabel2;
        private Panel panel2;
        private Controls.DarkDataGridViewControls texturesDataGridViewControls;
        private DarkDataGridView texturesDataGridView;
        private DarkButton butUpdate;
        private TableLayoutPanel tableLayoutPanel2;
        private DarkProgressBar previewProgressBar;
        private DarkLabel darkLabel5;
        private DataGridViewImageColumn texturesDataGridViewColumnImage;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnRepeat;
        private DataGridViewComboBoxColumn texturesDataGridViewColumnTexture;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnArea;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnTexCoord0;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnTexCoord1;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnTexCoord2;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnTexCoord3;
        private DarkLabel tooManyFramesWarning;
    }
}