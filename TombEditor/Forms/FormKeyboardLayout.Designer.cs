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
            this.butOK = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.listenKeys = new DarkUI.Controls.DarkLabel();
            this.butDefaults = new DarkUI.Controls.DarkButton();
            this.lblConflicts = new DarkUI.Controls.DarkLabel();
            this.commandList = new DarkUI.Controls.DarkDataGridView();
            this.commandListColumnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.commandListColumnCommand = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.commandListColumnAdd = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.commandListColumnDelete = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.commandListColumnHotkeys = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.commandList)).BeginInit();
            this.SuspendLayout();
            // 
            // butOK
            // 
            this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOK.Location = new System.Drawing.Point(435, 468);
            this.butOK.Name = "butOK";
            this.butOK.Size = new System.Drawing.Size(80, 23);
            this.butOK.TabIndex = 6;
            this.butOK.Text = "OK";
            this.butOK.Click += new System.EventHandler(this.butOK_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(521, 468);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 7;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // listenKeys
            // 
            this.listenKeys.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listenKeys.BackColor = System.Drawing.Color.DarkGreen;
            this.listenKeys.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listenKeys.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.listenKeys.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.listenKeys.Location = new System.Drawing.Point(151, 468);
            this.listenKeys.Name = "listenKeys";
            this.listenKeys.Size = new System.Drawing.Size(278, 23);
            this.listenKeys.TabIndex = 5;
            this.listenKeys.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.listenKeys.Visible = false;
            this.listenKeys.Click += new System.EventHandler(this.listenKeys_Click);
            // 
            // butDefaults
            // 
            this.butDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butDefaults.Location = new System.Drawing.Point(7, 468);
            this.butDefaults.Name = "butDefaults";
            this.butDefaults.Size = new System.Drawing.Size(138, 23);
            this.butDefaults.TabIndex = 4;
            this.butDefaults.Text = "Set all to default";
            this.butDefaults.Click += new System.EventHandler(this.butDefaults_Click);
            // 
            // lblConflicts
            // 
            this.lblConflicts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblConflicts.AutoSize = true;
            this.lblConflicts.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblConflicts.ForeColor = System.Drawing.Color.DarkGray;
            this.lblConflicts.Location = new System.Drawing.Point(4, 446);
            this.lblConflicts.Name = "lblConflicts";
            this.lblConflicts.Size = new System.Drawing.Size(52, 13);
            this.lblConflicts.TabIndex = 1;
            this.lblConflicts.Text = "Conflicts";
            this.lblConflicts.Visible = false;
            // 
            // commandList
            // 
            this.commandList.AllowUserToAddRows = false;
            this.commandList.AllowUserToDeleteRows = false;
            this.commandList.AllowUserToDragDropRows = false;
            this.commandList.AllowUserToOrderColumns = true;
            this.commandList.AllowUserToPasteCells = false;
            this.commandList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commandList.AutoGenerateColumns = false;
            this.commandList.ColumnHeadersHeight = 17;
            this.commandList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.commandListColumnType,
            this.commandListColumnCommand,
            this.commandListColumnAdd,
            this.commandListColumnDelete,
            this.commandListColumnHotkeys});
            this.commandList.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.commandList.Location = new System.Drawing.Point(7, 7);
            this.commandList.MultiSelect = false;
            this.commandList.Name = "commandList";
            this.commandList.RowHeadersWidth = 41;
            this.commandList.Size = new System.Drawing.Size(594, 432);
            this.commandList.TabIndex = 0;
            this.commandList.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.commandList_CellContentClick);
            this.commandList.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.commandList_CellDoubleClick);
            this.commandList.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.commandList_CellFormatting);
            this.commandList.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.commandList_CellPainting);
            this.commandList.CellParsing += new System.Windows.Forms.DataGridViewCellParsingEventHandler(this.commandList_CellParsing);
            this.commandList.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.commandList_CellValidating);
            // 
            // commandListColumnType
            // 
            this.commandListColumnType.DataPropertyName = "Type";
            this.commandListColumnType.FillWeight = 15F;
            this.commandListColumnType.HeaderText = "Type";
            this.commandListColumnType.Name = "commandListColumnType";
            this.commandListColumnType.ReadOnly = true;
            this.commandListColumnType.Width = 80;
            // 
            // commandListColumnCommand
            // 
            this.commandListColumnCommand.DataPropertyName = "FriendlyName";
            this.commandListColumnCommand.FillWeight = 55F;
            this.commandListColumnCommand.HeaderText = "Command";
            this.commandListColumnCommand.Name = "commandListColumnCommand";
            this.commandListColumnCommand.ReadOnly = true;
            this.commandListColumnCommand.Width = 200;
            // 
            // commandListColumnAdd
            // 
            this.commandListColumnAdd.HeaderText = "";
            this.commandListColumnAdd.Name = "commandListColumnAdd";
            this.commandListColumnAdd.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.commandListColumnAdd.Width = 22;
            // 
            // commandListColumnDelete
            // 
            this.commandListColumnDelete.HeaderText = "";
            this.commandListColumnDelete.Name = "commandListColumnDelete";
            this.commandListColumnDelete.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.commandListColumnDelete.Width = 22;
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
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(608, 498);
            this.Controls.Add(this.commandList);
            this.Controls.Add(this.lblConflicts);
            this.Controls.Add(this.butDefaults);
            this.Controls.Add(this.listenKeys);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOK);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.KeyPreview = true;
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
        private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkLabel listenKeys;
        private DarkUI.Controls.DarkButton butDefaults;
        private DarkUI.Controls.DarkLabel lblConflicts;
        private DarkUI.Controls.DarkDataGridView commandList;
        private System.Windows.Forms.DataGridViewTextBoxColumn commandListColumnType;
        private System.Windows.Forms.DataGridViewTextBoxColumn commandListColumnCommand;
        private DarkUI.Controls.DarkDataGridViewButtonColumn commandListColumnAdd;
        private DarkUI.Controls.DarkDataGridViewButtonColumn commandListColumnDelete;
        private System.Windows.Forms.DataGridViewTextBoxColumn commandListColumnHotkeys;
    }
}