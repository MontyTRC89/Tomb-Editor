namespace TombEditor.ToolWindows
{
    partial class ObjectBrowser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectBrowser));
            this.butResetSearch = new DarkUI.Controls.DarkButton();
            this.butFindItem = new DarkUI.Controls.DarkButton();
            this.panelStaticMeshColor = new System.Windows.Forms.Panel();
            this.darkLabel14 = new DarkUI.Controls.DarkLabel();
            this.butItemsBack = new DarkUI.Controls.DarkButton();
            this.panelItem = new TombEditor.Controls.PanelRenderingItem();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.comboItems = new DarkUI.Controls.DarkComboBox();
            this.panelHeaderRight = new System.Windows.Forms.Panel();
            this.butItemsNext = new DarkUI.Controls.DarkButton();
            this.butAddItem = new DarkUI.Controls.DarkButton();
            this.panelHeaderLeft = new System.Windows.Forms.Panel();
            this.panelRight = new System.Windows.Forms.Panel();
            this.panelRightTop = new System.Windows.Forms.Panel();
            this.panelRightBottom = new System.Windows.Forms.Panel();
            this.panelViewer = new System.Windows.Forms.Panel();
            this.panelHeader.SuspendLayout();
            this.panelHeaderRight.SuspendLayout();
            this.panelHeaderLeft.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.panelRightTop.SuspendLayout();
            this.panelRightBottom.SuspendLayout();
            this.panelViewer.SuspendLayout();
            this.SuspendLayout();
            //
            // butResetSearch
            //
            this.butResetSearch.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butResetSearch.Image = global::TombEditor.Properties.Resources.general_undo_16;
            this.butResetSearch.Location = new System.Drawing.Point(5, 30);
            this.butResetSearch.Name = "butResetSearch";
            this.butResetSearch.Padding = new System.Windows.Forms.Padding(5);
            this.butResetSearch.Size = new System.Drawing.Size(60, 23);
            this.butResetSearch.TabIndex = 6;
            this.butResetSearch.Text = "Reset";
            this.butResetSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butResetSearch.Click += new System.EventHandler(this.butResetSearch_Click);
            //
            // butFindItem
            //
            this.butFindItem.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butFindItem.Image = global::TombEditor.Properties.Resources.general_search_16;
            this.butFindItem.Location = new System.Drawing.Point(5, 1);
            this.butFindItem.Name = "butFindItem";
            this.butFindItem.Padding = new System.Windows.Forms.Padding(5);
            this.butFindItem.Size = new System.Drawing.Size(60, 23);
            this.butFindItem.TabIndex = 5;
            this.butFindItem.Text = "Find";
            this.butFindItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butFindItem.Click += new System.EventHandler(this.butFindItem_Click);
            //
            // panelStaticMeshColor
            //
            this.panelStaticMeshColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelStaticMeshColor.Location = new System.Drawing.Point(5, 36);
            this.panelStaticMeshColor.Name = "panelStaticMeshColor";
            this.panelStaticMeshColor.Size = new System.Drawing.Size(60, 20);
            this.panelStaticMeshColor.TabIndex = 4;
            this.panelStaticMeshColor.Click += new System.EventHandler(this.panelStaticMeshColor_Click);
            //
            // darkLabel14
            //
            this.darkLabel14.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel14.Location = new System.Drawing.Point(2, 3);
            this.darkLabel14.Name = "darkLabel14";
            this.darkLabel14.Size = new System.Drawing.Size(66, 30);
            this.darkLabel14.TabIndex = 67;
            this.darkLabel14.Text = "Static mesh color";
            //
            // butItemsBack
            //
            this.butItemsBack.Image = global::TombEditor.Properties.Resources.general_angle_left_16;
            this.butItemsBack.Location = new System.Drawing.Point(1, 0);
            this.butItemsBack.Name = "butItemsBack";
            this.butItemsBack.Padding = new System.Windows.Forms.Padding(5);
            this.butItemsBack.Size = new System.Drawing.Size(24, 24);
            this.butItemsBack.TabIndex = 0;
            this.butItemsBack.Click += new System.EventHandler(this.butItemsBack_Click);
            //
            // panelItem
            //
            this.panelItem.AutoSize = true;
            this.panelItem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelItem.Location = new System.Drawing.Point(2, 2);
            this.panelItem.Name = "panelItem";
            this.panelItem.Size = new System.Drawing.Size(213, 203);
            this.panelItem.TabIndex = 62;
            //
            // colorDialog
            //
            this.colorDialog.AnyColor = true;
            this.colorDialog.FullOpen = true;
            //
            // panelHeader
            //
            this.panelHeader.Controls.Add(this.comboItems);
            this.panelHeader.Controls.Add(this.panelHeaderRight);
            this.panelHeader.Controls.Add(this.panelHeaderLeft);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 25);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.panelHeader.Size = new System.Drawing.Size(284, 27);
            this.panelHeader.TabIndex = 72;
            //
            // comboItems
            // 
            this.comboItems.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboItems.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(81)))), ((int)(((byte)(81)))));
            this.comboItems.BorderStyle = System.Windows.Forms.ButtonBorderStyle.Solid;
            this.comboItems.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.comboItems.ButtonIcon = ((System.Drawing.Bitmap)(resources.GetObject("comboItems.ButtonIcon")));
            this.comboItems.Dock = System.Windows.Forms.DockStyle.Top;
            this.comboItems.DrawDropdownHoverOutline = false;
            this.comboItems.DrawFocusRectangle = false;
            this.comboItems.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboItems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboItems.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboItems.ForeColor = System.Drawing.Color.White;
            this.comboItems.FormattingEnabled = true;
            this.comboItems.ItemHeight = 18;
            this.comboItems.Location = new System.Drawing.Point(29, 2);
            this.comboItems.Name = "comboItems";
            this.comboItems.Size = new System.Drawing.Size(196, 24);
            this.comboItems.TabIndex = 1;
            this.comboItems.Text = null;
            this.comboItems.TextPadding = new System.Windows.Forms.Padding(2);
            this.comboItems.SelectedIndexChanged += new System.EventHandler(this.comboItems_SelectedIndexChanged);
            //
            // panelHeaderRight
            //
            this.panelHeaderRight.Controls.Add(this.butItemsNext);
            this.panelHeaderRight.Controls.Add(this.butAddItem);
            this.panelHeaderRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelHeaderRight.Location = new System.Drawing.Point(225, 2);
            this.panelHeaderRight.Name = "panelHeaderRight";
            this.panelHeaderRight.Size = new System.Drawing.Size(59, 25);
            this.panelHeaderRight.TabIndex = 76;
            //
            // butItemsNext
            //
            this.butItemsNext.Image = global::TombEditor.Properties.Resources.general_angle_right_16;
            this.butItemsNext.Location = new System.Drawing.Point(3, 0);
            this.butItemsNext.Name = "butItemsNext";
            this.butItemsNext.Padding = new System.Windows.Forms.Padding(5);
            this.butItemsNext.Size = new System.Drawing.Size(24, 24);
            this.butItemsNext.TabIndex = 2;
            this.butItemsNext.Click += new System.EventHandler(this.butItemsNext_Click);
            //
            // butAddItem
            //
            this.butAddItem.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.butAddItem.Location = new System.Drawing.Point(33, 0);
            this.butAddItem.Name = "butAddItem";
            this.butAddItem.Padding = new System.Windows.Forms.Padding(5);
            this.butAddItem.Size = new System.Drawing.Size(24, 24);
            this.butAddItem.TabIndex = 3;
            this.butAddItem.Click += new System.EventHandler(this.butAddItem_Click);
            //
            // panelHeaderLeft
            //
            this.panelHeaderLeft.Controls.Add(this.butItemsBack);
            this.panelHeaderLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelHeaderLeft.Location = new System.Drawing.Point(1, 2);
            this.panelHeaderLeft.Name = "panelHeaderLeft";
            this.panelHeaderLeft.Size = new System.Drawing.Size(28, 25);
            this.panelHeaderLeft.TabIndex = 75;
            //
            // panelRight
            //
            this.panelRight.Controls.Add(this.panelRightTop);
            this.panelRight.Controls.Add(this.panelRightBottom);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelRight.Location = new System.Drawing.Point(217, 52);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(67, 207);
            this.panelRight.TabIndex = 73;
            //
            // panelRightTop
            //
            this.panelRightTop.Controls.Add(this.darkLabel14);
            this.panelRightTop.Controls.Add(this.panelStaticMeshColor);
            this.panelRightTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelRightTop.Location = new System.Drawing.Point(0, 0);
            this.panelRightTop.Name = "panelRightTop";
            this.panelRightTop.Size = new System.Drawing.Size(67, 56);
            this.panelRightTop.TabIndex = 0;
            //
            // panelRightBottom
            //
            this.panelRightBottom.Controls.Add(this.butFindItem);
            this.panelRightBottom.Controls.Add(this.butResetSearch);
            this.panelRightBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelRightBottom.Location = new System.Drawing.Point(0, 151);
            this.panelRightBottom.Name = "panelRightBottom";
            this.panelRightBottom.Size = new System.Drawing.Size(67, 56);
            this.panelRightBottom.TabIndex = 1;
            //
            // panelViewer
            //
            this.panelViewer.AutoSize = true;
            this.panelViewer.Controls.Add(this.panelItem);
            this.panelViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelViewer.Location = new System.Drawing.Point(0, 52);
            this.panelViewer.Name = "panelViewer";
            this.panelViewer.Padding = new System.Windows.Forms.Padding(2);
            this.panelViewer.Size = new System.Drawing.Size(217, 207);
            this.panelViewer.TabIndex = 74;
            //
            // ObjectBrowser
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.Controls.Add(this.panelViewer);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelHeader);
            this.DockText = "Object Browser";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(237, 168);
            this.Name = "ObjectBrowser";
            this.SerializationKey = "ObjectBrowser";
            this.Size = new System.Drawing.Size(284, 259);
            this.panelHeader.ResumeLayout(false);
            this.panelHeaderRight.ResumeLayout(false);
            this.panelHeaderLeft.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            this.panelRightTop.ResumeLayout(false);
            this.panelRightBottom.ResumeLayout(false);
            this.panelViewer.ResumeLayout(false);
            this.panelViewer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkButton butResetSearch;
        private DarkUI.Controls.DarkButton butFindItem;
        private System.Windows.Forms.Panel panelStaticMeshColor;
        private DarkUI.Controls.DarkLabel darkLabel14;
        private DarkUI.Controls.DarkButton butItemsBack;
        private Controls.PanelRenderingItem panelItem;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Panel panelHeaderRight;
        private DarkUI.Controls.DarkButton butItemsNext;
        private DarkUI.Controls.DarkButton butAddItem;
        private System.Windows.Forms.Panel panelHeaderLeft;
        private DarkUI.Controls.DarkComboBox comboItems;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Panel panelRightBottom;
        private System.Windows.Forms.Panel panelRightTop;
        private System.Windows.Forms.Panel panelViewer;
    }
}
