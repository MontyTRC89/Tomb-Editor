namespace TombEditor.Forms
{
    partial class FormKeyboardLayout
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
            this.butAdd = new DarkUI.Controls.DarkButton();
            this.butClear = new DarkUI.Controls.DarkButton();
            this.butOK = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.listenKeys = new DarkUI.Controls.DarkLabel();
            this.butDefaults = new DarkUI.Controls.DarkButton();
            this.lblConflicts = new DarkUI.Controls.DarkLabel();
            this.commandList = new DarkUI.Controls.DarkDataGridView();
            this.commandListColumnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.commandListColumnCommand = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.commandListColumnHotkeys = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.commandList)).BeginInit();
            this.SuspendLayout();
            // 
            // butAdd
            // 
            this.butAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butAdd.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.butAdd.Location = new System.Drawing.Point(7, 298);
            this.butAdd.Name = "butAdd";
            this.butAdd.Size = new System.Drawing.Size(23, 23);
            this.butAdd.TabIndex = 3;
            this.butAdd.Click += new System.EventHandler(this.butAdd_Click);
            // 
            // butClear
            // 
            this.butClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butClear.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.butClear.Location = new System.Drawing.Point(36, 298);
            this.butClear.Name = "butClear";
            this.butClear.Size = new System.Drawing.Size(23, 23);
            this.butClear.TabIndex = 4;
            this.butClear.Click += new System.EventHandler(this.butClear_Click);
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Location = new System.Drawing.Point(339, 298);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 5;
            this.butOK.Text = "OK";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Location = new System.Drawing.Point(425, 298);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 6;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // listenKeys
            // 
            this.listenKeys.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listenKeys.BackColor = System.Drawing.Color.Firebrick;
            this.listenKeys.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listenKeys.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.listenKeys.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.listenKeys.Location = new System.Drawing.Point(151, 298);
            this.listenKeys.Name = "listenKeys";
            this.listenKeys.Size = new System.Drawing.Size(182, 23);
            this.listenKeys.TabIndex = 20;
            this.listenKeys.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.listenKeys.Visible = false;
            this.listenKeys.Click += new System.EventHandler(this.listenKeys_Click);
            // 
            // butDefaults
            // 
            this.butDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butDefaults.Location = new System.Drawing.Point(65, 298);
            this.butDefaults.Name = "butDefaults";
            this.butDefaults.Size = new System.Drawing.Size(80, 23);
            this.butDefaults.TabIndex = 23;
            this.butDefaults.Text = "Defaults";
            this.butDefaults.Click += new System.EventHandler(this.butDefaults_Click);
            // 
            // lblConflicts
            // 
            this.lblConflicts.AutoSize = true;
            this.lblConflicts.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.lblConflicts.Location = new System.Drawing.Point(4, 277);
            this.lblConflicts.Name = "lblConflicts";
            this.lblConflicts.Size = new System.Drawing.Size(52, 13);
            this.lblConflicts.TabIndex = 24;
            this.lblConflicts.Text = "Conflicts";
            this.lblConflicts.Visible = false;
            // 
            // commandList
            // 
            this.commandList.AllowUserToAddRows = false;
            this.commandList.AllowUserToDeleteRows = false;
            this.commandList.AllowUserToDragDropRows = false;
            this.commandList.AllowUserToPasteCells = false;
            this.commandList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commandList.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.commandList.ColumnHeadersHeight = 17;
            this.commandList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.commandListColumnType,
            this.commandListColumnCommand,
            this.commandListColumnHotkeys});
            this.commandList.Location = new System.Drawing.Point(7, 7);
            this.commandList.MultiSelect = false;
            this.commandList.Name = "commandList";
            this.commandList.ReadOnly = true;
            this.commandList.RowHeadersWidth = 41;
            this.commandList.Size = new System.Drawing.Size(498, 262);
            this.commandList.TabIndex = 25;
            this.commandList.VirtualMode = true;
            this.commandList.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.commandList_CellDoubleClick);
            this.commandList.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.commandList_CellValueNeeded);
            // 
            // commandListColumnType
            // 
            this.commandListColumnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.commandListColumnType.FillWeight = 15F;
            this.commandListColumnType.HeaderText = "Type";
            this.commandListColumnType.Name = "commandListColumnType";
            this.commandListColumnType.ReadOnly = true;
            // 
            // commandListColumnCommand
            // 
            this.commandListColumnCommand.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.commandListColumnCommand.FillWeight = 55F;
            this.commandListColumnCommand.HeaderText = "Command";
            this.commandListColumnCommand.Name = "commandListColumnCommand";
            this.commandListColumnCommand.ReadOnly = true;
            // 
            // commandListColumnHotkeys
            // 
            this.commandListColumnHotkeys.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.commandListColumnHotkeys.FillWeight = 30F;
            this.commandListColumnHotkeys.HeaderText = "Hotkeys";
            this.commandListColumnHotkeys.Name = "commandListColumnHotkeys";
            this.commandListColumnHotkeys.ReadOnly = true;
            // 
            // FormKeyboardLayout
            // 
            this.AcceptButton = this.butOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 328);
            this.Controls.Add(this.commandList);
            this.Controls.Add(this.lblConflicts);
            this.Controls.Add(this.butDefaults);
            this.Controls.Add(this.listenKeys);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Controls.Add(this.butClear);
            this.Controls.Add(this.butAdd);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormKeyboardLayout";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Keyboard Layout";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FormKeyboardLayout_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.commandList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkButton butAdd;
        private DarkUI.Controls.DarkButton butClear;
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkLabel listenKeys;
        private DarkUI.Controls.DarkButton butDefaults;
        private DarkUI.Controls.DarkLabel lblConflicts;
        private DarkUI.Controls.DarkDataGridView commandList;
        private System.Windows.Forms.DataGridViewTextBoxColumn commandListColumnType;
        private System.Windows.Forms.DataGridViewTextBoxColumn commandListColumnCommand;
        private System.Windows.Forms.DataGridViewTextBoxColumn commandListColumnHotkeys;
    }
}