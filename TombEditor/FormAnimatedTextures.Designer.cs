using DarkUI.Controls;
using System.Windows.Forms;

namespace TombEditor
{
    partial class FormAnimatedTextures
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelTextureContainer = new System.Windows.Forms.Panel();
            this.picTextureMap = new TombEditor.Controls.PanelAnimatedTextures(this.components);
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.comboItems = new DarkUI.Controls.DarkComboBox(this.components);
            this.butDelete = new DarkUI.Controls.DarkButton();
            this.butAddNew = new DarkUI.Controls.DarkButton();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.butAddNewTexture = new DarkUI.Controls.DarkButton();
            this.butDeleteTexture = new DarkUI.Controls.DarkButton();
            this.comboEffect = new DarkUI.Controls.DarkComboBox(this.components);
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.lstTextures = new BrightIdeasSoftware.FastObjectListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn4 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn5 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imgList = new System.Windows.Forms.ImageList(this.components);
            this.timerPreview = new System.Windows.Forms.Timer(this.components);
            this.butPlayAndStop = new DarkUI.Controls.DarkButton();
            this.panelTextureContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTextureMap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lstTextures)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTextureContainer
            // 
            this.panelTextureContainer.AutoScroll = true;
            this.panelTextureContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTextureContainer.Controls.Add(this.picTextureMap);
            this.panelTextureContainer.Location = new System.Drawing.Point(423, 12);
            this.panelTextureContainer.Name = "panelTextureContainer";
            this.panelTextureContainer.Size = new System.Drawing.Size(286, 477);
            this.panelTextureContainer.TabIndex = 24;
            // 
            // picTextureMap
            // 
            this.picTextureMap.IsTextureSelected = false;
            this.picTextureMap.Location = new System.Drawing.Point(-1, 0);
            this.picTextureMap.Name = "picTextureMap";
            this.picTextureMap.Page = ((short)(0));
            this.picTextureMap.SelectedX = ((short)(0));
            this.picTextureMap.SelectedY = ((short)(0));
            this.picTextureMap.Size = new System.Drawing.Size(256, 567);
            this.picTextureMap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picTextureMap.TabIndex = 0;
            this.picTextureMap.TabStop = false;
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(13, 16);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(64, 13);
            this.darkLabel1.TabIndex = 25;
            this.darkLabel1.Text = "Current set";
            // 
            // comboItems
            // 
            this.comboItems.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboItems.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboItems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboItems.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboItems.ForeColor = System.Drawing.Color.White;
            this.comboItems.FormattingEnabled = true;
            this.comboItems.ItemHeight = 18;
            this.comboItems.Items.AddRange(new object[] {
            "(Select animated texture set)"});
            this.comboItems.Location = new System.Drawing.Point(80, 13);
            this.comboItems.Name = "comboItems";
            this.comboItems.Size = new System.Drawing.Size(337, 24);
            this.comboItems.TabIndex = 31;
            this.comboItems.SelectedIndexChanged += new System.EventHandler(this.comboItems_SelectedIndexChanged);
            // 
            // butDelete
            // 
            this.butDelete.Location = new System.Drawing.Point(172, 43);
            this.butDelete.Name = "butDelete";
            this.butDelete.Padding = new System.Windows.Forms.Padding(5);
            this.butDelete.Size = new System.Drawing.Size(86, 23);
            this.butDelete.TabIndex = 33;
            this.butDelete.Text = "Delete";
            this.butDelete.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
            // 
            // butAddNew
            // 
            this.butAddNew.Location = new System.Drawing.Point(80, 43);
            this.butAddNew.Name = "butAddNew";
            this.butAddNew.Padding = new System.Windows.Forms.Padding(5);
            this.butAddNew.Size = new System.Drawing.Size(86, 23);
            this.butAddNew.TabIndex = 32;
            this.butAddNew.Text = "Add new";
            this.butAddNew.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddNew.Click += new System.EventHandler(this.butAddNew_Click);
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(13, 151);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(48, 13);
            this.darkLabel2.TabIndex = 35;
            this.darkLabel2.Text = "Textures";
            // 
            // butAddNewTexture
            // 
            this.butAddNewTexture.Location = new System.Drawing.Point(304, 167);
            this.butAddNewTexture.Name = "butAddNewTexture";
            this.butAddNewTexture.Padding = new System.Windows.Forms.Padding(5);
            this.butAddNewTexture.Size = new System.Drawing.Size(113, 23);
            this.butAddNewTexture.TabIndex = 36;
            this.butAddNewTexture.Text = "← Add texture";
            this.butAddNewTexture.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddNewTexture.Click += new System.EventHandler(this.butAddNewTexture_Click);
            // 
            // butDeleteTexture
            // 
            this.butDeleteTexture.Location = new System.Drawing.Point(304, 196);
            this.butDeleteTexture.Name = "butDeleteTexture";
            this.butDeleteTexture.Padding = new System.Windows.Forms.Padding(5);
            this.butDeleteTexture.Size = new System.Drawing.Size(113, 23);
            this.butDeleteTexture.TabIndex = 37;
            this.butDeleteTexture.Text = "Delete texture";
            this.butDeleteTexture.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteTexture.Click += new System.EventHandler(this.butDeleteTexture_Click);
            // 
            // comboEffect
            // 
            this.comboEffect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboEffect.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboEffect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboEffect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboEffect.ForeColor = System.Drawing.Color.White;
            this.comboEffect.FormattingEnabled = true;
            this.comboEffect.ItemHeight = 18;
            this.comboEffect.Items.AddRange(new object[] {
            "Normal",
            "Half Rotate",
            "Full Rotate"});
            this.comboEffect.Location = new System.Drawing.Point(80, 116);
            this.comboEffect.Name = "comboEffect";
            this.comboEffect.Size = new System.Drawing.Size(218, 24);
            this.comboEffect.TabIndex = 39;
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(13, 119);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(39, 13);
            this.darkLabel3.TabIndex = 38;
            this.darkLabel3.Text = "Effect:";
            // 
            // picPreview
            // 
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.Location = new System.Drawing.Point(329, 425);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(64, 64);
            this.picPreview.TabIndex = 40;
            this.picPreview.TabStop = false;
            // 
            // darkLabel4
            // 
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(326, 371);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(46, 13);
            this.darkLabel4.TabIndex = 41;
            this.darkLabel4.Text = "Preview";
            // 
            // butCancel
            // 
            this.butCancel.Location = new System.Drawing.Point(317, 505);
            this.butCancel.Name = "butCancel";
            this.butCancel.Padding = new System.Windows.Forms.Padding(5);
            this.butCancel.Size = new System.Drawing.Size(86, 23);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Close";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // lstTextures
            // 
            this.lstTextures.AllColumns.Add(this.olvColumn1);
            this.lstTextures.AllColumns.Add(this.olvColumn4);
            this.lstTextures.AllColumns.Add(this.olvColumn5);
            this.lstTextures.AllColumns.Add(this.olvColumn2);
            this.lstTextures.CellEditUseWholeCell = false;
            this.lstTextures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1,
            this.olvColumn4,
            this.olvColumn5,
            this.olvColumn2});
            this.lstTextures.Cursor = System.Windows.Forms.Cursors.Default;
            this.lstTextures.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstTextures.FullRowSelect = true;
            this.lstTextures.GridLines = true;
            this.lstTextures.Location = new System.Drawing.Point(16, 167);
            this.lstTextures.MultiSelect = false;
            this.lstTextures.Name = "lstTextures";
            this.lstTextures.RowHeight = 64;
            this.lstTextures.ShowGroups = false;
            this.lstTextures.ShowImagesOnSubItems = true;
            this.lstTextures.Size = new System.Drawing.Size(282, 322);
            this.lstTextures.SmallImageList = this.imgList;
            this.lstTextures.TabIndex = 53;
            this.lstTextures.UseCompatibleStateImageBehavior = false;
            this.lstTextures.View = System.Windows.Forms.View.Details;
            this.lstTextures.VirtualMode = true;
            this.lstTextures.CellClick += new System.EventHandler<BrightIdeasSoftware.CellClickEventArgs>(this.lstTextures_CellClick);
            this.lstTextures.Click += new System.EventHandler(this.lstTextures_Click);
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "Texture";
            this.olvColumn1.Text = "Texture";
            this.olvColumn1.Width = 64;
            // 
            // olvColumn4
            // 
            this.olvColumn4.AspectName = "X";
            this.olvColumn4.Text = "X";
            // 
            // olvColumn5
            // 
            this.olvColumn5.AspectName = "Y";
            this.olvColumn5.Text = "Y";
            // 
            // olvColumn2
            // 
            this.olvColumn2.AspectName = "Page";
            this.olvColumn2.Text = "Page";
            // 
            // imgList
            // 
            this.imgList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imgList.ImageSize = new System.Drawing.Size(64, 64);
            this.imgList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // timerPreview
            // 
            this.timerPreview.Interval = 33;
            this.timerPreview.Tick += new System.EventHandler(this.timerPreview_Tick);
            // 
            // butPlayAndStop
            // 
            this.butPlayAndStop.Location = new System.Drawing.Point(329, 396);
            this.butPlayAndStop.Name = "butPlayAndStop";
            this.butPlayAndStop.Padding = new System.Windows.Forms.Padding(5);
            this.butPlayAndStop.Size = new System.Drawing.Size(64, 23);
            this.butPlayAndStop.TabIndex = 54;
            this.butPlayAndStop.Text = "Play";
            this.butPlayAndStop.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlayAndStop.Click += new System.EventHandler(this.butPlayAndStop_Click);
            // 
            // FormAnimatedTextures
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(721, 540);
            this.Controls.Add(this.butPlayAndStop);
            this.Controls.Add(this.lstTextures);
            this.Controls.Add(this.darkLabel4);
            this.Controls.Add(this.picPreview);
            this.Controls.Add(this.comboEffect);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.butDeleteTexture);
            this.Controls.Add(this.butAddNewTexture);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.butDelete);
            this.Controls.Add(this.butAddNew);
            this.Controls.Add(this.comboItems);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.panelTextureContainer);
            this.Controls.Add(this.butCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAnimatedTextures";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Animated textures";
            this.Load += new System.EventHandler(this.FormSink_Load);
            this.panelTextureContainer.ResumeLayout(false);
            this.panelTextureContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTextureMap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lstTextures)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Panel panelTextureContainer;
        private Controls.PanelAnimatedTextures picTextureMap;
        private DarkLabel darkLabel1;
        private DarkComboBox comboItems;
        private DarkButton butDelete;
        private DarkButton butAddNew;
        private DarkLabel darkLabel2;
        private DarkButton butAddNewTexture;
        private DarkButton butDeleteTexture;
        private DarkComboBox comboEffect;
        private DarkLabel darkLabel3;
        private PictureBox picPreview;
        private DarkLabel darkLabel4;
        private DarkButton butCancel;
        internal BrightIdeasSoftware.FastObjectListView lstTextures;
        private BrightIdeasSoftware.OLVColumn olvColumn4;
        private BrightIdeasSoftware.OLVColumn olvColumn5;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
        private ImageList imgList;
        private Timer timerPreview;
        private DarkButton butPlayAndStop;
        private BrightIdeasSoftware.OLVColumn olvColumn2;
    }
}