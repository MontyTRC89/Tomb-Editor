namespace WadTool
{
    partial class FormAnimCommandsEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.btOk = new DarkUI.Controls.DarkButton();
            this.butAddEffect = new DarkUI.Controls.DarkButton();
            this.butDeleteEffect = new DarkUI.Controls.DarkButton();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.butCopyToAll = new DarkUI.Controls.DarkButton();
            this.gridViewCommands = new DarkUI.Controls.DarkDataGridView();
            this.colCommands = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.butCopy = new DarkUI.Controls.DarkButton();
            this.butCommandDown = new DarkUI.Controls.DarkButton();
            this.butCommandUp = new DarkUI.Controls.DarkButton();
            this.animCommandEditor = new WadTool.AnimCommandEditor();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.darkGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewCommands)).BeginInit();
            this.SuspendLayout();
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.Checked = false;
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(296, 354);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(80, 23);
            this.btCancel.TabIndex = 50;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Checked = false;
            this.btOk.Location = new System.Drawing.Point(210, 354);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(80, 23);
            this.btOk.TabIndex = 51;
            this.btOk.Text = "OK";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // butAddEffect
            // 
            this.butAddEffect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddEffect.Checked = false;
            this.butAddEffect.Image = global::WadTool.Properties.Resources.general_plus_math_16;
            this.butAddEffect.Location = new System.Drawing.Point(341, 6);
            this.butAddEffect.Name = "butAddEffect";
            this.butAddEffect.Size = new System.Drawing.Size(24, 24);
            this.butAddEffect.TabIndex = 95;
            this.toolTip1.SetToolTip(this.butAddEffect, "New animcommand");
            this.butAddEffect.Click += new System.EventHandler(this.butAddEffect_Click);
            // 
            // butDeleteEffect
            // 
            this.butDeleteEffect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteEffect.Checked = false;
            this.butDeleteEffect.Image = global::WadTool.Properties.Resources.trash_161;
            this.butDeleteEffect.Location = new System.Drawing.Point(340, 96);
            this.butDeleteEffect.Name = "butDeleteEffect";
            this.butDeleteEffect.Size = new System.Drawing.Size(24, 24);
            this.butDeleteEffect.TabIndex = 94;
            this.toolTip1.SetToolTip(this.butDeleteEffect, "Delete animcommand");
            this.butDeleteEffect.Click += new System.EventHandler(this.butDeleteEffect_Click);
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkGroupBox1.Controls.Add(this.butCopyToAll);
            this.darkGroupBox1.Controls.Add(this.gridViewCommands);
            this.darkGroupBox1.Controls.Add(this.butCopy);
            this.darkGroupBox1.Controls.Add(this.butCommandDown);
            this.darkGroupBox1.Controls.Add(this.butCommandUp);
            this.darkGroupBox1.Controls.Add(this.butAddEffect);
            this.darkGroupBox1.Controls.Add(this.butDeleteEffect);
            this.darkGroupBox1.Location = new System.Drawing.Point(5, 5);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(371, 196);
            this.darkGroupBox1.TabIndex = 99;
            this.darkGroupBox1.TabStop = false;
            // 
            // butCopyToAll
            // 
            this.butCopyToAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butCopyToAll.Checked = false;
            this.butCopyToAll.Image = global::WadTool.Properties.Resources.copy_to_all_16;
            this.butCopyToAll.Location = new System.Drawing.Point(341, 66);
            this.butCopyToAll.Name = "butCopyToAll";
            this.butCopyToAll.Size = new System.Drawing.Size(24, 24);
            this.butCopyToAll.TabIndex = 101;
            this.toolTip1.SetToolTip(this.butCopyToAll, "Copy selected animcommand to selected frames");
            this.butCopyToAll.Click += new System.EventHandler(this.butCopyToAll_Click);
            // 
            // gridViewCommands
            // 
            this.gridViewCommands.AllowUserToAddRows = false;
            this.gridViewCommands.AllowUserToDragDropRows = false;
            this.gridViewCommands.AllowUserToOrderColumns = true;
            this.gridViewCommands.AllowUserToResizeColumns = false;
            this.gridViewCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridViewCommands.AutoGenerateColumns = false;
            this.gridViewCommands.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridViewCommands.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.gridViewCommands.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridViewCommands.ColumnHeadersHeight = 23;
            this.gridViewCommands.ColumnHeadersVisible = false;
            this.gridViewCommands.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCommands});
            this.gridViewCommands.Location = new System.Drawing.Point(6, 6);
            this.gridViewCommands.MultiSelect = false;
            this.gridViewCommands.Name = "gridViewCommands";
            this.gridViewCommands.ReadOnly = true;
            this.gridViewCommands.RowHeadersWidth = 41;
            this.gridViewCommands.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.gridViewCommands.Size = new System.Drawing.Size(329, 184);
            this.gridViewCommands.TabIndex = 100;
            this.gridViewCommands.SelectionChanged += new System.EventHandler(this.GridViewCommands_SelectionChanged);
            // 
            // colCommands
            // 
            this.colCommands.DataPropertyName = "Description";
            dataGridViewCellStyle1.NullValue = "//";
            this.colCommands.DefaultCellStyle = dataGridViewCellStyle1;
            this.colCommands.HeaderText = "Commands";
            this.colCommands.MaxInputLength = 100;
            this.colCommands.Name = "colCommands";
            this.colCommands.ReadOnly = true;
            // 
            // butCopy
            // 
            this.butCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butCopy.Checked = false;
            this.butCopy.Image = global::WadTool.Properties.Resources.copy_16;
            this.butCopy.Location = new System.Drawing.Point(341, 36);
            this.butCopy.Name = "butCopy";
            this.butCopy.Size = new System.Drawing.Size(24, 24);
            this.butCopy.TabIndex = 99;
            this.toolTip1.SetToolTip(this.butCopy, "Copy selected animcommands");
            this.butCopy.Click += new System.EventHandler(this.butCopy_Click);
            // 
            // butCommandDown
            // 
            this.butCommandDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCommandDown.Checked = false;
            this.butCommandDown.Image = global::WadTool.Properties.Resources.general_ArrowDown_16;
            this.butCommandDown.Location = new System.Drawing.Point(341, 166);
            this.butCommandDown.Name = "butCommandDown";
            this.butCommandDown.Size = new System.Drawing.Size(24, 24);
            this.butCommandDown.TabIndex = 97;
            this.toolTip1.SetToolTip(this.butCommandDown, "Move down");
            this.butCommandDown.Click += new System.EventHandler(this.butCommandDown_Click);
            // 
            // butCommandUp
            // 
            this.butCommandUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCommandUp.Checked = false;
            this.butCommandUp.Image = global::WadTool.Properties.Resources.general_ArrowUp_16;
            this.butCommandUp.Location = new System.Drawing.Point(341, 138);
            this.butCommandUp.Name = "butCommandUp";
            this.butCommandUp.Size = new System.Drawing.Size(24, 24);
            this.butCommandUp.TabIndex = 96;
            this.toolTip1.SetToolTip(this.butCommandUp, "Move up");
            this.butCommandUp.Click += new System.EventHandler(this.butCommandUp_Click);
            // 
            // animCommandEditor
            // 
            this.animCommandEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.animCommandEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.animCommandEditor.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.animCommandEditor.Location = new System.Drawing.Point(5, 207);
            this.animCommandEditor.Name = "animCommandEditor";
            this.animCommandEditor.Size = new System.Drawing.Size(371, 141);
            this.animCommandEditor.TabIndex = 100;
            this.animCommandEditor.AnimCommandChanged += new System.EventHandler<WadTool.AnimCommandEditor.AnimCommandEventArgs>(this.AnimCommandEditor_AnimCommandChanged);
            // 
            // FormAnimCommandsEditor
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(381, 382);
            this.Controls.Add(this.animCommandEditor);
            this.Controls.Add(this.darkGroupBox1);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(397, 420);
            this.Name = "FormAnimCommandsEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Anim commands editor";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormAnimCommandsEditor_KeyDown);
            this.darkGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridViewCommands)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DarkUI.Controls.DarkButton btCancel;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkButton butAddEffect;
        private DarkUI.Controls.DarkButton butDeleteEffect;
        private DarkUI.Controls.DarkGroupBox darkGroupBox1;
        private DarkUI.Controls.DarkButton butCommandDown;
        private DarkUI.Controls.DarkButton butCommandUp;
        private AnimCommandEditor animCommandEditor;
        private DarkUI.Controls.DarkButton butCopy;
        private DarkUI.Controls.DarkDataGridView gridViewCommands;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCommands;
        private DarkUI.Controls.DarkButton butCopyToAll;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}