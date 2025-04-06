namespace TombLib.Controls.VisualScripting
{
    partial class ArgumentEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            container = new DarkTabbedContainer();
            tabBoolean = new System.Windows.Forms.TabPage();
            cbBool = new DarkUI.Controls.DarkCheckBox();
            tabNumerical = new System.Windows.Forms.TabPage();
            nudNumerical = new DarkUI.Controls.DarkNumericUpDown();
            tabVector2 = new System.Windows.Forms.TabPage();
            tableVector2 = new System.Windows.Forms.TableLayoutPanel();
            nudVector2Y = new DarkUI.Controls.DarkNumericUpDown();
            nudVector2X = new DarkUI.Controls.DarkNumericUpDown();
            tabVector3 = new System.Windows.Forms.TabPage();
            tableVector3 = new System.Windows.Forms.TableLayoutPanel();
            nudVector3Z = new DarkUI.Controls.DarkNumericUpDown();
            nudVector3Y = new DarkUI.Controls.DarkNumericUpDown();
            nudVector3X = new DarkUI.Controls.DarkNumericUpDown();
            tabString = new System.Windows.Forms.TabPage();
            tbString = new DarkUI.Controls.DarkTextBox();
            panelMultiline = new DarkUI.Controls.DarkPanel();
            butMultiline = new DarkUI.Controls.DarkButton();
            tabColor = new System.Windows.Forms.TabPage();
            panelColor = new DarkUI.Controls.DarkPanel();
            tabTime = new System.Windows.Forms.TabPage();
            tableTime = new System.Windows.Forms.TableLayoutPanel();
            nudTimeCents = new DarkUI.Controls.DarkNumericUpDown();
            nudTimeSeconds = new DarkUI.Controls.DarkNumericUpDown();
            nudTimeMinutes = new DarkUI.Controls.DarkNumericUpDown();
            nudTimeHours = new DarkUI.Controls.DarkNumericUpDown();
            tabList = new System.Windows.Forms.TabPage();
            cbList = new DarkSearchableComboBox();
            panelAction = new DarkUI.Controls.DarkPanel();
            butAction = new DarkUI.Controls.DarkButton();
            container.SuspendLayout();
            tabBoolean.SuspendLayout();
            tabNumerical.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudNumerical).BeginInit();
            tabVector2.SuspendLayout();
            tableVector2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudVector2Y).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudVector2X).BeginInit();
            tabVector3.SuspendLayout();
            tableVector3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudVector3Z).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudVector3Y).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudVector3X).BeginInit();
            tabString.SuspendLayout();
            panelMultiline.SuspendLayout();
            tabColor.SuspendLayout();
            tabTime.SuspendLayout();
            tableTime.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudTimeCents).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTimeSeconds).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTimeMinutes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTimeHours).BeginInit();
            tabList.SuspendLayout();
            panelAction.SuspendLayout();
            SuspendLayout();
            // 
            // container
            // 
            container.Controls.Add(tabBoolean);
            container.Controls.Add(tabNumerical);
            container.Controls.Add(tabVector2);
            container.Controls.Add(tabVector3);
            container.Controls.Add(tabString);
            container.Controls.Add(tabColor);
            container.Controls.Add(tabTime);
            container.Controls.Add(tabList);
            container.Dock = System.Windows.Forms.DockStyle.Fill;
            container.Location = new System.Drawing.Point(0, 0);
            container.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            container.Name = "container";
            container.SelectedIndex = 0;
            container.Size = new System.Drawing.Size(511, 57);
            container.TabIndex = 0;
            // 
            // tabBoolean
            // 
            tabBoolean.Controls.Add(cbBool);
            tabBoolean.Location = new System.Drawing.Point(4, 24);
            tabBoolean.Margin = new System.Windows.Forms.Padding(0);
            tabBoolean.Name = "tabBoolean";
            tabBoolean.Size = new System.Drawing.Size(503, 29);
            tabBoolean.TabIndex = 0;
            tabBoolean.Text = "Boolean";
            tabBoolean.UseVisualStyleBackColor = true;
            // 
            // cbBool
            // 
            cbBool.AutoSize = true;
            cbBool.Dock = System.Windows.Forms.DockStyle.Fill;
            cbBool.Location = new System.Drawing.Point(0, 0);
            cbBool.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbBool.Name = "cbBool";
            cbBool.Size = new System.Drawing.Size(503, 29);
            cbBool.TabIndex = 0;
            cbBool.Text = "None";
            cbBool.CheckedChanged += rb_CheckedChanged;
            // 
            // tabNumerical
            // 
            tabNumerical.Controls.Add(nudNumerical);
            tabNumerical.Location = new System.Drawing.Point(4, 24);
            tabNumerical.Margin = new System.Windows.Forms.Padding(1);
            tabNumerical.Name = "tabNumerical";
            tabNumerical.Size = new System.Drawing.Size(503, 29);
            tabNumerical.TabIndex = 1;
            tabNumerical.Text = "Numerical";
            tabNumerical.UseVisualStyleBackColor = true;
            // 
            // nudNumerical
            // 
            nudNumerical.DecimalPlaces = 2;
            nudNumerical.Dock = System.Windows.Forms.DockStyle.Fill;
            nudNumerical.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudNumerical.Location = new System.Drawing.Point(0, 0);
            nudNumerical.LoopValues = false;
            nudNumerical.Margin = new System.Windows.Forms.Padding(0);
            nudNumerical.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudNumerical.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudNumerical.Name = "nudNumerical";
            nudNumerical.Size = new System.Drawing.Size(503, 23);
            nudNumerical.TabIndex = 0;
            nudNumerical.ValueChanged += nudNumerical_ValueChanged;
            // 
            // tabVector2
            // 
            tabVector2.Controls.Add(tableVector2);
            tabVector2.Location = new System.Drawing.Point(4, 24);
            tabVector2.Margin = new System.Windows.Forms.Padding(1);
            tabVector2.Name = "tabVector2";
            tabVector2.Size = new System.Drawing.Size(503, 29);
            tabVector2.TabIndex = 6;
            tabVector2.Text = "Vector2";
            tabVector2.UseVisualStyleBackColor = true;
            // 
            // tableVector2
            // 
            tableVector2.ColumnCount = 2;
            tableVector2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            tableVector2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            tableVector2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            tableVector2.Controls.Add(nudVector2Y, 1, 0);
            tableVector2.Controls.Add(nudVector2X, 0, 0);
            tableVector2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableVector2.Location = new System.Drawing.Point(0, 0);
            tableVector2.Margin = new System.Windows.Forms.Padding(0);
            tableVector2.Name = "tableVector2";
            tableVector2.RowCount = 1;
            tableVector2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableVector2.Size = new System.Drawing.Size(503, 29);
            tableVector2.TabIndex = 1;
            // 
            // nudVector2Y
            // 
            nudVector2Y.AllowDrop = true;
            nudVector2Y.DecimalPlaces = 2;
            nudVector2Y.Dock = System.Windows.Forms.DockStyle.Fill;
            nudVector2Y.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudVector2Y.Location = new System.Drawing.Point(253, 0);
            nudVector2Y.LoopValues = false;
            nudVector2Y.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            nudVector2Y.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudVector2Y.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudVector2Y.Name = "nudVector2Y";
            nudVector2Y.Size = new System.Drawing.Size(248, 23);
            nudVector2Y.TabIndex = 1;
            nudVector2Y.ValueChanged += nudVector2_ValueChanged;
            // 
            // nudVector2X
            // 
            nudVector2X.AllowDrop = true;
            nudVector2X.DecimalPlaces = 2;
            nudVector2X.Dock = System.Windows.Forms.DockStyle.Fill;
            nudVector2X.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudVector2X.Location = new System.Drawing.Point(0, 0);
            nudVector2X.LoopValues = false;
            nudVector2X.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            nudVector2X.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudVector2X.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudVector2X.Name = "nudVector2X";
            nudVector2X.Size = new System.Drawing.Size(249, 23);
            nudVector2X.TabIndex = 0;
            nudVector2X.ValueChanged += nudVector2_ValueChanged;
            // 
            // tabVector3
            // 
            tabVector3.Controls.Add(tableVector3);
            tabVector3.Location = new System.Drawing.Point(4, 24);
            tabVector3.Margin = new System.Windows.Forms.Padding(1);
            tabVector3.Name = "tabVector3";
            tabVector3.Size = new System.Drawing.Size(503, 29);
            tabVector3.TabIndex = 2;
            tabVector3.Text = "Vector3";
            tabVector3.UseVisualStyleBackColor = true;
            // 
            // tableVector3
            // 
            tableVector3.ColumnCount = 3;
            tableVector3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            tableVector3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            tableVector3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            tableVector3.Controls.Add(nudVector3Z, 2, 0);
            tableVector3.Controls.Add(nudVector3Y, 1, 0);
            tableVector3.Controls.Add(nudVector3X, 0, 0);
            tableVector3.Dock = System.Windows.Forms.DockStyle.Fill;
            tableVector3.Location = new System.Drawing.Point(0, 0);
            tableVector3.Margin = new System.Windows.Forms.Padding(0);
            tableVector3.Name = "tableVector3";
            tableVector3.RowCount = 1;
            tableVector3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableVector3.Size = new System.Drawing.Size(503, 29);
            tableVector3.TabIndex = 0;
            // 
            // nudVector3Z
            // 
            nudVector3Z.AllowDrop = true;
            nudVector3Z.DecimalPlaces = 2;
            nudVector3Z.Dock = System.Windows.Forms.DockStyle.Fill;
            nudVector3Z.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudVector3Z.Location = new System.Drawing.Point(336, 0);
            nudVector3Z.LoopValues = false;
            nudVector3Z.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
            nudVector3Z.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudVector3Z.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudVector3Z.Name = "nudVector3Z";
            nudVector3Z.Size = new System.Drawing.Size(167, 23);
            nudVector3Z.TabIndex = 2;
            nudVector3Z.ValueChanged += nudVector3_ValueChanged;
            nudVector3Z.DragDrop += vector3Control_DragDrop;
            nudVector3Z.DragEnter += draggableControl_DragEnter;
            // 
            // nudVector3Y
            // 
            nudVector3Y.AllowDrop = true;
            nudVector3Y.DecimalPlaces = 2;
            nudVector3Y.Dock = System.Windows.Forms.DockStyle.Fill;
            nudVector3Y.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudVector3Y.Location = new System.Drawing.Point(169, 0);
            nudVector3Y.LoopValues = false;
            nudVector3Y.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            nudVector3Y.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudVector3Y.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudVector3Y.Name = "nudVector3Y";
            nudVector3Y.Size = new System.Drawing.Size(163, 23);
            nudVector3Y.TabIndex = 1;
            nudVector3Y.ValueChanged += nudVector3_ValueChanged;
            nudVector3Y.DragDrop += vector3Control_DragDrop;
            nudVector3Y.DragEnter += draggableControl_DragEnter;
            // 
            // nudVector3X
            // 
            nudVector3X.AllowDrop = true;
            nudVector3X.DecimalPlaces = 2;
            nudVector3X.Dock = System.Windows.Forms.DockStyle.Fill;
            nudVector3X.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudVector3X.Location = new System.Drawing.Point(0, 0);
            nudVector3X.LoopValues = false;
            nudVector3X.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            nudVector3X.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudVector3X.Minimum = new decimal(new int[] { 1000000, 0, 0, int.MinValue });
            nudVector3X.Name = "nudVector3X";
            nudVector3X.Size = new System.Drawing.Size(165, 23);
            nudVector3X.TabIndex = 0;
            nudVector3X.ValueChanged += nudVector3_ValueChanged;
            nudVector3X.DragDrop += vector3Control_DragDrop;
            nudVector3X.DragEnter += draggableControl_DragEnter;
            // 
            // tabString
            // 
            tabString.Controls.Add(tbString);
            tabString.Controls.Add(panelMultiline);
            tabString.Location = new System.Drawing.Point(4, 24);
            tabString.Margin = new System.Windows.Forms.Padding(1);
            tabString.Name = "tabString";
            tabString.Size = new System.Drawing.Size(503, 29);
            tabString.TabIndex = 3;
            tabString.Text = "String";
            tabString.UseVisualStyleBackColor = true;
            // 
            // tbString
            // 
            tbString.Dock = System.Windows.Forms.DockStyle.Fill;
            tbString.Location = new System.Drawing.Point(0, 0);
            tbString.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbString.Name = "tbString";
            tbString.Size = new System.Drawing.Size(475, 23);
            tbString.TabIndex = 0;
            tbString.TextChanged += tbString_TextChanged;
            // 
            // panelMultiline
            // 
            panelMultiline.Controls.Add(butMultiline);
            panelMultiline.Dock = System.Windows.Forms.DockStyle.Right;
            panelMultiline.Location = new System.Drawing.Point(475, 0);
            panelMultiline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panelMultiline.Name = "panelMultiline";
            panelMultiline.Size = new System.Drawing.Size(28, 29);
            panelMultiline.TabIndex = 2;
            // 
            // butMultiline
            // 
            butMultiline.Checked = false;
            butMultiline.Dock = System.Windows.Forms.DockStyle.Fill;
            butMultiline.Image = Properties.Resources.general_Multiline_text_16;
            butMultiline.Location = new System.Drawing.Point(0, 0);
            butMultiline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            butMultiline.Name = "butMultiline";
            butMultiline.Size = new System.Drawing.Size(28, 29);
            butMultiline.TabIndex = 2;
            butMultiline.Click += butMultiline_Click;
            // 
            // tabColor
            // 
            tabColor.Controls.Add(panelColor);
            tabColor.Location = new System.Drawing.Point(4, 24);
            tabColor.Margin = new System.Windows.Forms.Padding(1);
            tabColor.Name = "tabColor";
            tabColor.Size = new System.Drawing.Size(503, 29);
            tabColor.TabIndex = 4;
            tabColor.Text = "Color";
            tabColor.UseVisualStyleBackColor = true;
            // 
            // panelColor
            // 
            panelColor.AllowDrop = true;
            panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelColor.Dock = System.Windows.Forms.DockStyle.Fill;
            panelColor.Location = new System.Drawing.Point(0, 0);
            panelColor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panelColor.Name = "panelColor";
            panelColor.Size = new System.Drawing.Size(503, 29);
            panelColor.TabIndex = 0;
            panelColor.BackColorChanged += panelColor_BackColorChanged;
            panelColor.DragDrop += panelColor_DragDrop;
            panelColor.DragEnter += panelColor_DragEnter;
            panelColor.MouseClick += panelColor_MouseClick;
            // 
            // tabTime
            // 
            tabTime.Controls.Add(tableTime);
            tabTime.Location = new System.Drawing.Point(4, 24);
            tabTime.Margin = new System.Windows.Forms.Padding(1);
            tabTime.Name = "tabTime";
            tabTime.Size = new System.Drawing.Size(503, 29);
            tabTime.TabIndex = 7;
            tabTime.Text = "Time";
            tabTime.UseVisualStyleBackColor = true;
            // 
            // tableTime
            // 
            tableTime.ColumnCount = 4;
            tableTime.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableTime.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableTime.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableTime.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableTime.Controls.Add(nudTimeCents, 3, 0);
            tableTime.Controls.Add(nudTimeSeconds, 2, 0);
            tableTime.Controls.Add(nudTimeMinutes, 1, 0);
            tableTime.Controls.Add(nudTimeHours, 0, 0);
            tableTime.Dock = System.Windows.Forms.DockStyle.Fill;
            tableTime.Location = new System.Drawing.Point(0, 0);
            tableTime.Margin = new System.Windows.Forms.Padding(0);
            tableTime.Name = "tableTime";
            tableTime.RowCount = 1;
            tableTime.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableTime.Size = new System.Drawing.Size(503, 29);
            tableTime.TabIndex = 1;
            // 
            // nudTimeCents
            // 
            nudTimeCents.AllowDrop = true;
            nudTimeCents.Dock = System.Windows.Forms.DockStyle.Fill;
            nudTimeCents.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudTimeCents.Location = new System.Drawing.Point(377, 0);
            nudTimeCents.LoopValues = false;
            nudTimeCents.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
            nudTimeCents.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            nudTimeCents.Name = "nudTimeCents";
            nudTimeCents.Size = new System.Drawing.Size(126, 23);
            nudTimeCents.TabIndex = 3;
            nudTimeCents.ValueChanged += nudTime_ValueChanged;
            // 
            // nudTimeSeconds
            // 
            nudTimeSeconds.AllowDrop = true;
            nudTimeSeconds.Dock = System.Windows.Forms.DockStyle.Fill;
            nudTimeSeconds.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudTimeSeconds.Location = new System.Drawing.Point(252, 0);
            nudTimeSeconds.LoopValues = false;
            nudTimeSeconds.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
            nudTimeSeconds.Maximum = new decimal(new int[] { 59, 0, 0, 0 });
            nudTimeSeconds.Name = "nudTimeSeconds";
            nudTimeSeconds.Size = new System.Drawing.Size(123, 23);
            nudTimeSeconds.TabIndex = 2;
            nudTimeSeconds.ValueChanged += nudTime_ValueChanged;
            // 
            // nudTimeMinutes
            // 
            nudTimeMinutes.AllowDrop = true;
            nudTimeMinutes.Dock = System.Windows.Forms.DockStyle.Fill;
            nudTimeMinutes.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudTimeMinutes.Location = new System.Drawing.Point(127, 0);
            nudTimeMinutes.LoopValues = false;
            nudTimeMinutes.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            nudTimeMinutes.Maximum = new decimal(new int[] { 59, 0, 0, 0 });
            nudTimeMinutes.Name = "nudTimeMinutes";
            nudTimeMinutes.Size = new System.Drawing.Size(121, 23);
            nudTimeMinutes.TabIndex = 1;
            nudTimeMinutes.ValueChanged += nudTime_ValueChanged;
            // 
            // nudTimeHours
            // 
            nudTimeHours.AllowDrop = true;
            nudTimeHours.Dock = System.Windows.Forms.DockStyle.Fill;
            nudTimeHours.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            nudTimeHours.Location = new System.Drawing.Point(0, 0);
            nudTimeHours.LoopValues = false;
            nudTimeHours.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            nudTimeHours.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudTimeHours.Name = "nudTimeHours";
            nudTimeHours.Size = new System.Drawing.Size(123, 23);
            nudTimeHours.TabIndex = 0;
            nudTimeHours.ValueChanged += nudTime_ValueChanged;
            // 
            // tabList
            // 
            tabList.Controls.Add(cbList);
            tabList.Controls.Add(panelAction);
            tabList.Location = new System.Drawing.Point(4, 24);
            tabList.Margin = new System.Windows.Forms.Padding(1);
            tabList.Name = "tabList";
            tabList.Size = new System.Drawing.Size(503, 29);
            tabList.TabIndex = 5;
            tabList.Text = "List";
            tabList.UseVisualStyleBackColor = true;
            // 
            // cbList
            // 
            cbList.AllowDrop = true;
            cbList.Dock = System.Windows.Forms.DockStyle.Fill;
            cbList.Location = new System.Drawing.Point(0, 0);
            cbList.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            cbList.Name = "cbList";
            cbList.SearchThreshold = 10;
            cbList.Size = new System.Drawing.Size(475, 24);
            cbList.TabIndex = 0;
            cbList.SelectedIndexChanged += cbList_SelectedIndexChanged;
            cbList.DragDrop += cbList_DragDrop;
            cbList.DragEnter += draggableControl_DragEnter;
            // 
            // panelAction
            // 
            panelAction.Controls.Add(butAction);
            panelAction.Dock = System.Windows.Forms.DockStyle.Right;
            panelAction.Location = new System.Drawing.Point(475, 0);
            panelAction.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panelAction.Name = "panelAction";
            panelAction.Size = new System.Drawing.Size(28, 29);
            panelAction.TabIndex = 1;
            // 
            // butAction
            // 
            butAction.Checked = false;
            butAction.Dock = System.Windows.Forms.DockStyle.Fill;
            butAction.Image = Properties.Resources.general_target_16;
            butAction.Location = new System.Drawing.Point(0, 0);
            butAction.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            butAction.Name = "butAction";
            butAction.Size = new System.Drawing.Size(28, 29);
            butAction.TabIndex = 0;
            butAction.Click += butAction_Click;
            // 
            // ArgumentEditor
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(container);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ArgumentEditor";
            Size = new System.Drawing.Size(511, 57);
            container.ResumeLayout(false);
            tabBoolean.ResumeLayout(false);
            tabBoolean.PerformLayout();
            tabNumerical.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudNumerical).EndInit();
            tabVector2.ResumeLayout(false);
            tableVector2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudVector2Y).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudVector2X).EndInit();
            tabVector3.ResumeLayout(false);
            tableVector3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudVector3Z).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudVector3Y).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudVector3X).EndInit();
            tabString.ResumeLayout(false);
            tabString.PerformLayout();
            panelMultiline.ResumeLayout(false);
            tabColor.ResumeLayout(false);
            tabTime.ResumeLayout(false);
            tableTime.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudTimeCents).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTimeSeconds).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTimeMinutes).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTimeHours).EndInit();
            tabList.ResumeLayout(false);
            panelAction.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DarkTabbedContainer container;
        private System.Windows.Forms.TabPage tabBoolean;
        private System.Windows.Forms.TabPage tabNumerical;
        private System.Windows.Forms.TabPage tabVector3;
        private System.Windows.Forms.TabPage tabString;
        private System.Windows.Forms.TabPage tabColor;
        private System.Windows.Forms.TabPage tabList;
        private DarkUI.Controls.DarkNumericUpDown nudNumerical;
        private System.Windows.Forms.TableLayoutPanel tableVector3;
        private DarkUI.Controls.DarkNumericUpDown nudVector3X;
        private DarkUI.Controls.DarkNumericUpDown nudVector3Z;
        private DarkUI.Controls.DarkNumericUpDown nudVector3Y;
        private DarkUI.Controls.DarkTextBox tbString;
        private DarkUI.Controls.DarkPanel panelColor;
        private DarkSearchableComboBox cbList;
        private DarkUI.Controls.DarkPanel panelAction;
        private DarkUI.Controls.DarkButton butAction;
        private DarkUI.Controls.DarkCheckBox cbBool;
        private DarkUI.Controls.DarkPanel panelMultiline;
        private DarkUI.Controls.DarkButton butMultiline;
        private System.Windows.Forms.TabPage tabVector2;
        private System.Windows.Forms.TableLayoutPanel tableVector2;
        private DarkUI.Controls.DarkNumericUpDown nudVector2Y;
        private DarkUI.Controls.DarkNumericUpDown nudVector2X;
        private System.Windows.Forms.TabPage tabTime;
        private System.Windows.Forms.TableLayoutPanel tableTime;
        private DarkUI.Controls.DarkNumericUpDown nudTimeCents;
        private DarkUI.Controls.DarkNumericUpDown nudTimeSeconds;
        private DarkUI.Controls.DarkNumericUpDown nudTimeMinutes;
        private DarkUI.Controls.DarkNumericUpDown nudTimeHours;
    }
}
