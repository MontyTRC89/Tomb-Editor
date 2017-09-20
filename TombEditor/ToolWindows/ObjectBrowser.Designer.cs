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
            this.components = new System.ComponentModel.Container();
            this.butResetSearch = new DarkUI.Controls.DarkButton();
            this.butFindItem = new DarkUI.Controls.DarkButton();
            this.panelStaticMeshColor = new System.Windows.Forms.Panel();
            this.darkLabel14 = new DarkUI.Controls.DarkLabel();
            this.comboItems = new DarkUI.Controls.DarkComboBox(this.components);
            this.butItemsNext = new DarkUI.Controls.DarkButton();
            this.butItemsBack = new DarkUI.Controls.DarkButton();
            this.butAddItem = new DarkUI.Controls.DarkButton();
            this.panelItem = new TombEditor.Controls.PanelRenderingItem();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.SuspendLayout();
            // 
            // butResetSearch
            // 
            this.butResetSearch.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butResetSearch.Image = global::TombEditor.Properties.Resources.undo_16;
            this.butResetSearch.Location = new System.Drawing.Point(221, 238);
            this.butResetSearch.Name = "butResetSearch";
            this.butResetSearch.Padding = new System.Windows.Forms.Padding(5);
            this.butResetSearch.Size = new System.Drawing.Size(60, 23);
            this.butResetSearch.TabIndex = 70;
            this.butResetSearch.Text = "Reset";
            this.butResetSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butResetSearch.Click += new System.EventHandler(this.butResetSearch_Click);
            // 
            // butFindItem
            // 
            this.butFindItem.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butFindItem.Image = global::TombEditor.Properties.Resources.search_16;
            this.butFindItem.Location = new System.Drawing.Point(221, 209);
            this.butFindItem.Name = "butFindItem";
            this.butFindItem.Padding = new System.Windows.Forms.Padding(5);
            this.butFindItem.Size = new System.Drawing.Size(60, 23);
            this.butFindItem.TabIndex = 69;
            this.butFindItem.Text = "Find";
            this.butFindItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butFindItem.Click += new System.EventHandler(this.butFindItem_Click);
            // 
            // panelStaticMeshColor
            // 
            this.panelStaticMeshColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelStaticMeshColor.Location = new System.Drawing.Point(219, 91);
            this.panelStaticMeshColor.Name = "panelStaticMeshColor";
            this.panelStaticMeshColor.Size = new System.Drawing.Size(60, 20);
            this.panelStaticMeshColor.TabIndex = 68;
            this.panelStaticMeshColor.Click += new System.EventHandler(this.panelStaticMeshColor_Click);
            // 
            // darkLabel14
            // 
            this.darkLabel14.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel14.Location = new System.Drawing.Point(216, 58);
            this.darkLabel14.Name = "darkLabel14";
            this.darkLabel14.Size = new System.Drawing.Size(66, 30);
            this.darkLabel14.TabIndex = 67;
            this.darkLabel14.Text = "Static mesh color";
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
            this.comboItems.Location = new System.Drawing.Point(35, 28);
            this.comboItems.Name = "comboItems";
            this.comboItems.Size = new System.Drawing.Size(186, 24);
            this.comboItems.TabIndex = 66;
            this.comboItems.SelectedIndexChanged += new System.EventHandler(this.comboItems_SelectedIndexChanged);
            // 
            // butItemsNext
            // 
            this.butItemsNext.Image = global::TombEditor.Properties.Resources.angle_right_16;
            this.butItemsNext.Location = new System.Drawing.Point(227, 28);
            this.butItemsNext.Name = "butItemsNext";
            this.butItemsNext.Padding = new System.Windows.Forms.Padding(5);
            this.butItemsNext.Size = new System.Drawing.Size(24, 24);
            this.butItemsNext.TabIndex = 65;
            this.butItemsNext.Click += new System.EventHandler(this.butItemsNext_Click);
            // 
            // butItemsBack
            // 
            this.butItemsBack.Image = global::TombEditor.Properties.Resources.angle_left_16;
            this.butItemsBack.Location = new System.Drawing.Point(5, 28);
            this.butItemsBack.Name = "butItemsBack";
            this.butItemsBack.Padding = new System.Windows.Forms.Padding(5);
            this.butItemsBack.Size = new System.Drawing.Size(24, 24);
            this.butItemsBack.TabIndex = 64;
            this.butItemsBack.Click += new System.EventHandler(this.butItemsBack_Click);
            // 
            // butAddItem
            // 
            this.butAddItem.Image = global::TombEditor.Properties.Resources.plus_math_16;
            this.butAddItem.Location = new System.Drawing.Point(257, 28);
            this.butAddItem.Name = "butAddItem";
            this.butAddItem.Padding = new System.Windows.Forms.Padding(5);
            this.butAddItem.Size = new System.Drawing.Size(24, 24);
            this.butAddItem.TabIndex = 63;
            this.butAddItem.Click += new System.EventHandler(this.butAddItem_Click);
            // 
            // panelItem
            // 
            this.panelItem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelItem.Location = new System.Drawing.Point(5, 58);
            this.panelItem.Name = "panelItem";
            this.panelItem.Size = new System.Drawing.Size(210, 203);
            this.panelItem.TabIndex = 62;
            // 
            // colorDialog
            // 
            this.colorDialog.AnyColor = true;
            this.colorDialog.FullOpen = true;
            // 
            // ObjectBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.butResetSearch);
            this.Controls.Add(this.butFindItem);
            this.Controls.Add(this.panelStaticMeshColor);
            this.Controls.Add(this.darkLabel14);
            this.Controls.Add(this.comboItems);
            this.Controls.Add(this.butItemsNext);
            this.Controls.Add(this.butItemsBack);
            this.Controls.Add(this.butAddItem);
            this.Controls.Add(this.panelItem);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Left;
            this.DockText = "Object Browser";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MaximumSize = new System.Drawing.Size(0, 264);
            this.Name = "ObjectBrowser";
            this.SerializationKey = "ObjectBrowser";
            this.Size = new System.Drawing.Size(284, 264);
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkButton butResetSearch;
        private DarkUI.Controls.DarkButton butFindItem;
        private System.Windows.Forms.Panel panelStaticMeshColor;
        private DarkUI.Controls.DarkLabel darkLabel14;
        private DarkUI.Controls.DarkComboBox comboItems;
        private DarkUI.Controls.DarkButton butItemsNext;
        private DarkUI.Controls.DarkButton butItemsBack;
        private DarkUI.Controls.DarkButton butAddItem;
        private Controls.PanelRenderingItem panelItem;
        private System.Windows.Forms.ColorDialog colorDialog;
    }
}
