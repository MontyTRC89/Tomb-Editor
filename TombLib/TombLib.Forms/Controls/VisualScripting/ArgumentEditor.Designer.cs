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
            this.container = new TombLib.Controls.DarkTabbedContainer();
            this.tabBoolean = new System.Windows.Forms.TabPage();
            this.cbBool = new DarkUI.Controls.DarkCheckBox();
            this.tabNumerical = new System.Windows.Forms.TabPage();
            this.nudNumerical = new DarkUI.Controls.DarkNumericUpDown();
            this.tabVector3 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.nudVector3Z = new DarkUI.Controls.DarkNumericUpDown();
            this.nudVector3Y = new DarkUI.Controls.DarkNumericUpDown();
            this.nudVector3X = new DarkUI.Controls.DarkNumericUpDown();
            this.tabString = new System.Windows.Forms.TabPage();
            this.tbString = new DarkUI.Controls.DarkTextBox();
            this.panelMultiline = new DarkUI.Controls.DarkPanel();
            this.butMultiline = new DarkUI.Controls.DarkButton();
            this.tabColor = new System.Windows.Forms.TabPage();
            this.panelColor = new DarkUI.Controls.DarkPanel();
            this.tabList = new System.Windows.Forms.TabPage();
            this.cbList = new TombLib.Controls.DarkSearchableComboBox();
            this.panelLocate = new DarkUI.Controls.DarkPanel();
            this.butLocate = new DarkUI.Controls.DarkButton();
            this.container.SuspendLayout();
            this.tabBoolean.SuspendLayout();
            this.tabNumerical.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNumerical)).BeginInit();
            this.tabVector3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudVector3Z)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudVector3Y)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudVector3X)).BeginInit();
            this.tabString.SuspendLayout();
            this.panelMultiline.SuspendLayout();
            this.tabColor.SuspendLayout();
            this.tabList.SuspendLayout();
            this.panelLocate.SuspendLayout();
            this.SuspendLayout();
            // 
            // container
            // 
            this.container.Controls.Add(this.tabBoolean);
            this.container.Controls.Add(this.tabNumerical);
            this.container.Controls.Add(this.tabVector3);
            this.container.Controls.Add(this.tabString);
            this.container.Controls.Add(this.tabColor);
            this.container.Controls.Add(this.tabList);
            this.container.Dock = System.Windows.Forms.DockStyle.Fill;
            this.container.Location = new System.Drawing.Point(0, 0);
            this.container.Name = "container";
            this.container.SelectedIndex = 0;
            this.container.Size = new System.Drawing.Size(438, 49);
            this.container.TabIndex = 0;
            // 
            // tabBoolean
            // 
            this.tabBoolean.Controls.Add(this.cbBool);
            this.tabBoolean.Location = new System.Drawing.Point(4, 22);
            this.tabBoolean.Margin = new System.Windows.Forms.Padding(0);
            this.tabBoolean.Name = "tabBoolean";
            this.tabBoolean.Size = new System.Drawing.Size(430, 23);
            this.tabBoolean.TabIndex = 0;
            this.tabBoolean.Text = "Boolean";
            this.tabBoolean.UseVisualStyleBackColor = true;
            // 
            // cbBool
            // 
            this.cbBool.AutoSize = true;
            this.cbBool.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbBool.Location = new System.Drawing.Point(0, 0);
            this.cbBool.Name = "cbBool";
            this.cbBool.Size = new System.Drawing.Size(430, 23);
            this.cbBool.TabIndex = 0;
            this.cbBool.Text = "None";
            this.cbBool.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // tabNumerical
            // 
            this.tabNumerical.Controls.Add(this.nudNumerical);
            this.tabNumerical.Location = new System.Drawing.Point(4, 22);
            this.tabNumerical.Margin = new System.Windows.Forms.Padding(1);
            this.tabNumerical.Name = "tabNumerical";
            this.tabNumerical.Size = new System.Drawing.Size(430, 23);
            this.tabNumerical.TabIndex = 1;
            this.tabNumerical.Text = "Numerical";
            this.tabNumerical.UseVisualStyleBackColor = true;
            // 
            // nudNumerical
            // 
            this.nudNumerical.DecimalPlaces = 2;
            this.nudNumerical.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudNumerical.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudNumerical.Location = new System.Drawing.Point(0, 0);
            this.nudNumerical.LoopValues = false;
            this.nudNumerical.Margin = new System.Windows.Forms.Padding(0);
            this.nudNumerical.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudNumerical.Minimum = new decimal(new int[] {
            1000000,
            0,
            0,
            -2147483648});
            this.nudNumerical.Name = "nudNumerical";
            this.nudNumerical.Size = new System.Drawing.Size(430, 20);
            this.nudNumerical.TabIndex = 0;
            this.nudNumerical.ValueChanged += new System.EventHandler(this.nudNumerical_ValueChanged);
            // 
            // tabVector3
            // 
            this.tabVector3.Controls.Add(this.tableLayoutPanel1);
            this.tabVector3.Location = new System.Drawing.Point(4, 22);
            this.tabVector3.Margin = new System.Windows.Forms.Padding(1);
            this.tabVector3.Name = "tabVector3";
            this.tabVector3.Size = new System.Drawing.Size(430, 23);
            this.tabVector3.TabIndex = 2;
            this.tabVector3.Text = "Vector3";
            this.tabVector3.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.nudVector3Z, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.nudVector3Y, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.nudVector3X, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(430, 23);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // nudVector3Z
            // 
            this.nudVector3Z.DecimalPlaces = 2;
            this.nudVector3Z.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudVector3Z.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudVector3Z.Location = new System.Drawing.Point(288, 0);
            this.nudVector3Z.LoopValues = false;
            this.nudVector3Z.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.nudVector3Z.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudVector3Z.Minimum = new decimal(new int[] {
            1000000,
            0,
            0,
            -2147483648});
            this.nudVector3Z.Name = "nudVector3Z";
            this.nudVector3Z.Size = new System.Drawing.Size(142, 20);
            this.nudVector3Z.TabIndex = 2;
            this.nudVector3Z.ValueChanged += new System.EventHandler(this.nudVector3_ValueChanged);
            // 
            // nudVector3Y
            // 
            this.nudVector3Y.DecimalPlaces = 2;
            this.nudVector3Y.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudVector3Y.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudVector3Y.Location = new System.Drawing.Point(145, 0);
            this.nudVector3Y.LoopValues = false;
            this.nudVector3Y.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.nudVector3Y.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudVector3Y.Minimum = new decimal(new int[] {
            1000000,
            0,
            0,
            -2147483648});
            this.nudVector3Y.Name = "nudVector3Y";
            this.nudVector3Y.Size = new System.Drawing.Size(139, 20);
            this.nudVector3Y.TabIndex = 1;
            this.nudVector3Y.ValueChanged += new System.EventHandler(this.nudVector3_ValueChanged);
            // 
            // nudVector3X
            // 
            this.nudVector3X.DecimalPlaces = 2;
            this.nudVector3X.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudVector3X.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.nudVector3X.Location = new System.Drawing.Point(0, 0);
            this.nudVector3X.LoopValues = false;
            this.nudVector3X.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.nudVector3X.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudVector3X.Minimum = new decimal(new int[] {
            1000000,
            0,
            0,
            -2147483648});
            this.nudVector3X.Name = "nudVector3X";
            this.nudVector3X.Size = new System.Drawing.Size(141, 20);
            this.nudVector3X.TabIndex = 0;
            this.nudVector3X.ValueChanged += new System.EventHandler(this.nudVector3_ValueChanged);
            // 
            // tabString
            // 
            this.tabString.Controls.Add(this.tbString);
            this.tabString.Controls.Add(this.panelMultiline);
            this.tabString.Location = new System.Drawing.Point(4, 22);
            this.tabString.Margin = new System.Windows.Forms.Padding(1);
            this.tabString.Name = "tabString";
            this.tabString.Size = new System.Drawing.Size(430, 23);
            this.tabString.TabIndex = 3;
            this.tabString.Text = "String";
            this.tabString.UseVisualStyleBackColor = true;
            // 
            // tbString
            // 
            this.tbString.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbString.Location = new System.Drawing.Point(0, 0);
            this.tbString.Name = "tbString";
            this.tbString.Size = new System.Drawing.Size(407, 20);
            this.tbString.TabIndex = 0;
            this.tbString.TextChanged += new System.EventHandler(this.tbString_TextChanged);
            // 
            // panelMultiline
            // 
            this.panelMultiline.Controls.Add(this.butMultiline);
            this.panelMultiline.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelMultiline.Location = new System.Drawing.Point(406, 0);
            this.panelMultiline.Name = "panelMultiline";
            this.panelMultiline.Size = new System.Drawing.Size(24, 23);
            this.panelMultiline.TabIndex = 2;
            // 
            // butMultiline
            // 
            this.butMultiline.Checked = false;
            this.butMultiline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butMultiline.Image = global::TombLib.Properties.Resources.general_Multiline_text_16;
            this.butMultiline.Location = new System.Drawing.Point(0, 0);
            this.butMultiline.Name = "butMultiline";
            this.butMultiline.Size = new System.Drawing.Size(24, 23);
            this.butMultiline.TabIndex = 2;
            this.butMultiline.Click += new System.EventHandler(this.butMultiline_Click);
            // 
            // tabColor
            // 
            this.tabColor.Controls.Add(this.panelColor);
            this.tabColor.Location = new System.Drawing.Point(4, 22);
            this.tabColor.Margin = new System.Windows.Forms.Padding(1);
            this.tabColor.Name = "tabColor";
            this.tabColor.Size = new System.Drawing.Size(430, 23);
            this.tabColor.TabIndex = 4;
            this.tabColor.Text = "Color";
            this.tabColor.UseVisualStyleBackColor = true;
            // 
            // panelColor
            // 
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelColor.Location = new System.Drawing.Point(0, 0);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(430, 23);
            this.panelColor.TabIndex = 0;
            this.panelColor.BackColorChanged += new System.EventHandler(this.panelColor_BackColorChanged);
            this.panelColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelColor_MouseClick);
            // 
            // tabList
            // 
            this.tabList.Controls.Add(this.cbList);
            this.tabList.Controls.Add(this.panelLocate);
            this.tabList.Location = new System.Drawing.Point(4, 22);
            this.tabList.Margin = new System.Windows.Forms.Padding(1);
            this.tabList.Name = "tabList";
            this.tabList.Size = new System.Drawing.Size(430, 23);
            this.tabList.TabIndex = 5;
            this.tabList.Text = "List";
            this.tabList.UseVisualStyleBackColor = true;
            // 
            // cbList
            // 
            this.cbList.AllowDrop = true;
            this.cbList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbList.Location = new System.Drawing.Point(0, 0);
            this.cbList.Name = "cbList";
            this.cbList.SearchThreshold = 10;
            this.cbList.Size = new System.Drawing.Size(406, 21);
            this.cbList.TabIndex = 0;
            this.cbList.SelectedIndexChanged += new System.EventHandler(this.cbList_SelectedIndexChanged);
            this.cbList.DragDrop += new System.Windows.Forms.DragEventHandler(this.cbList_DragDrop);
            this.cbList.DragEnter += new System.Windows.Forms.DragEventHandler(this.cbList_DragEnter);
            // 
            // panelLocate
            // 
            this.panelLocate.Controls.Add(this.butLocate);
            this.panelLocate.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelLocate.Location = new System.Drawing.Point(406, 0);
            this.panelLocate.Name = "panelLocate";
            this.panelLocate.Size = new System.Drawing.Size(24, 23);
            this.panelLocate.TabIndex = 1;
            // 
            // butLocate
            // 
            this.butLocate.Checked = false;
            this.butLocate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butLocate.Image = global::TombLib.Properties.Resources.general_target_16;
            this.butLocate.Location = new System.Drawing.Point(0, 0);
            this.butLocate.Name = "butLocate";
            this.butLocate.Size = new System.Drawing.Size(24, 23);
            this.butLocate.TabIndex = 0;
            this.butLocate.Click += new System.EventHandler(this.butLocate_Click);
            // 
            // ArgumentEditor
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.container);
            this.Name = "ArgumentEditor";
            this.Size = new System.Drawing.Size(438, 49);
            this.container.ResumeLayout(false);
            this.tabBoolean.ResumeLayout(false);
            this.tabBoolean.PerformLayout();
            this.tabNumerical.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudNumerical)).EndInit();
            this.tabVector3.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudVector3Z)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudVector3Y)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudVector3X)).EndInit();
            this.tabString.ResumeLayout(false);
            this.tabString.PerformLayout();
            this.panelMultiline.ResumeLayout(false);
            this.tabColor.ResumeLayout(false);
            this.tabList.ResumeLayout(false);
            this.panelLocate.ResumeLayout(false);
            this.ResumeLayout(false);

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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DarkUI.Controls.DarkNumericUpDown nudVector3X;
        private DarkUI.Controls.DarkNumericUpDown nudVector3Z;
        private DarkUI.Controls.DarkNumericUpDown nudVector3Y;
        private DarkUI.Controls.DarkTextBox tbString;
        private DarkUI.Controls.DarkPanel panelColor;
        private DarkSearchableComboBox cbList;
        private DarkUI.Controls.DarkPanel panelLocate;
        private DarkUI.Controls.DarkButton butLocate;
        private DarkUI.Controls.DarkCheckBox cbBool;
        private DarkUI.Controls.DarkPanel panelMultiline;
        private DarkUI.Controls.DarkButton butMultiline;
    }
}
