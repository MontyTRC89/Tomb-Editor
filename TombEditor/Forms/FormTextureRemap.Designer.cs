namespace TombEditor.Forms
{
    partial class FormTextureRemap
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
            this.sourcePanel = new System.Windows.Forms.Panel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.comboSourceTexture = new DarkUI.Controls.DarkComboBox();
            this.sourceTextureMap = new TombEditor.Forms.FormTextureRemap.PanelTextureMapForRemap();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.destinationPanel = new System.Windows.Forms.Panel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.comboDestinationTexture = new DarkUI.Controls.DarkComboBox();
            this.destinationTextureMap = new TombEditor.Forms.FormTextureRemap.PanelTextureMapForRemap();
            this.cbRestrictToSelectedRooms = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.cbUntextureCompletely = new DarkUI.Controls.DarkCheckBox();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.scalingFactor = new DarkUI.Controls.DarkNumericUpDown();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.darkCheckBox1 = new DarkUI.Controls.DarkCheckBox();
            this.sourcePanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.destinationPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scalingFactor)).BeginInit();
            this.SuspendLayout();
            // 
            // sourcePanel
            // 
            this.sourcePanel.Controls.Add(this.darkLabel1);
            this.sourcePanel.Controls.Add(this.comboSourceTexture);
            this.sourcePanel.Controls.Add(this.sourceTextureMap);
            this.sourcePanel.Location = new System.Drawing.Point(3, 3);
            this.sourcePanel.Name = "sourcePanel";
            this.sourcePanel.Size = new System.Drawing.Size(340, 537);
            this.sourcePanel.TabIndex = 22;
            // 
            // darkLabel1
            // 
            this.darkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(3, 5);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(334, 22);
            this.darkLabel1.TabIndex = 5;
            this.darkLabel1.Text = "Source";
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // comboSourceTexture
            // 
            this.comboSourceTexture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboSourceTexture.FormattingEnabled = true;
            this.comboSourceTexture.Location = new System.Drawing.Point(4, 32);
            this.comboSourceTexture.Name = "comboSourceTexture";
            this.comboSourceTexture.Size = new System.Drawing.Size(330, 21);
            this.comboSourceTexture.TabIndex = 4;
            this.comboSourceTexture.DropDown += new System.EventHandler(this.comboSourceTexture_DropDown);
            this.comboSourceTexture.SelectedValueChanged += new System.EventHandler(this.comboSourceTexture_SelectedValueChanged);
            // 
            // sourceTextureMap
            // 
            this.sourceTextureMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceTextureMap.FormParent = null;
            this.sourceTextureMap.IsDestination = false;
            this.sourceTextureMap.Location = new System.Drawing.Point(4, 59);
            this.sourceTextureMap.Margin = new System.Windows.Forms.Padding(3, 30, 3, 3);
            this.sourceTextureMap.Name = "sourceTextureMap";
            this.sourceTextureMap.Size = new System.Drawing.Size(330, 475);
            this.sourceTextureMap.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.destinationPanel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.sourcePanel, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 121);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(692, 543);
            this.tableLayoutPanel1.TabIndex = 23;
            // 
            // destinationPanel
            // 
            this.destinationPanel.Controls.Add(this.darkLabel2);
            this.destinationPanel.Controls.Add(this.comboDestinationTexture);
            this.destinationPanel.Controls.Add(this.destinationTextureMap);
            this.destinationPanel.Location = new System.Drawing.Point(349, 3);
            this.destinationPanel.Name = "destinationPanel";
            this.destinationPanel.Size = new System.Drawing.Size(340, 537);
            this.destinationPanel.TabIndex = 23;
            // 
            // darkLabel2
            // 
            this.darkLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(3, 5);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(334, 22);
            this.darkLabel2.TabIndex = 5;
            this.darkLabel2.Text = "Destination";
            this.darkLabel2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // comboDestinationTexture
            // 
            this.comboDestinationTexture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboDestinationTexture.FormattingEnabled = true;
            this.comboDestinationTexture.Location = new System.Drawing.Point(4, 32);
            this.comboDestinationTexture.Name = "comboDestinationTexture";
            this.comboDestinationTexture.Size = new System.Drawing.Size(330, 21);
            this.comboDestinationTexture.TabIndex = 4;
            this.comboDestinationTexture.DropDown += new System.EventHandler(this.comboDestinationTexture_DropDown);
            this.comboDestinationTexture.SelectedValueChanged += new System.EventHandler(this.comboDestinationTexture_SelectedValueChanged);
            // 
            // destinationTextureMap
            // 
            this.destinationTextureMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.destinationTextureMap.FormParent = null;
            this.destinationTextureMap.IsDestination = true;
            this.destinationTextureMap.Location = new System.Drawing.Point(4, 59);
            this.destinationTextureMap.Margin = new System.Windows.Forms.Padding(3, 30, 3, 3);
            this.destinationTextureMap.Name = "destinationTextureMap";
            this.destinationTextureMap.Size = new System.Drawing.Size(330, 475);
            this.destinationTextureMap.TabIndex = 0;
            // 
            // cbRestrictToSelectedRooms
            // 
            this.cbRestrictToSelectedRooms.Checked = true;
            this.cbRestrictToSelectedRooms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRestrictToSelectedRooms.Location = new System.Drawing.Point(33, 54);
            this.cbRestrictToSelectedRooms.Name = "cbRestrictToSelectedRooms";
            this.cbRestrictToSelectedRooms.Size = new System.Drawing.Size(277, 17);
            this.cbRestrictToSelectedRooms.TabIndex = 24;
            this.cbRestrictToSelectedRooms.Text = "Restrict to selected rooms";
            // 
            // darkLabel3
            // 
            this.darkLabel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(9, 9);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(334, 15);
            this.darkLabel3.TabIndex = 25;
            this.darkLabel3.Text = "Automated tool to remap texture areas throughout the level.";
            // 
            // cbUntextureCompletely
            // 
            this.cbUntextureCompletely.Location = new System.Drawing.Point(33, 31);
            this.cbUntextureCompletely.Name = "cbUntextureCompletely";
            this.cbUntextureCompletely.Size = new System.Drawing.Size(277, 17);
            this.cbUntextureCompletely.TabIndex = 24;
            this.cbUntextureCompletely.Text = "Untexture completely (ATTENTION)";
            this.cbUntextureCompletely.CheckedChanged += new System.EventHandler(this.cbUntextureCompletely_CheckedChanged);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Location = new System.Drawing.Point(418, 668);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(141, 25);
            this.butOk.TabIndex = 26;
            this.butOk.Text = "Ok";
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Location = new System.Drawing.Point(565, 668);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(137, 25);
            this.butCancel.TabIndex = 26;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // scalingFactor
            // 
            this.scalingFactor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.scalingFactor.DecimalPlaces = 3;
            this.scalingFactor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.scalingFactor.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.scalingFactor.Location = new System.Drawing.Point(33, 99);
            this.scalingFactor.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.scalingFactor.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.scalingFactor.MousewheelSingleIncrement = true;
            this.scalingFactor.Name = "scalingFactor";
            this.scalingFactor.Size = new System.Drawing.Size(84, 20);
            this.scalingFactor.TabIndex = 27;
            this.scalingFactor.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.scalingFactor.ValueChanged += new System.EventHandler(this.scalingFactor_ValueChanged);
            // 
            // darkLabel4
            // 
            this.darkLabel4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(123, 101);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(334, 18);
            this.darkLabel4.TabIndex = 25;
            this.darkLabel4.Text = "Scaling factor";
            // 
            // darkCheckBox1
            // 
            this.darkCheckBox1.Checked = true;
            this.darkCheckBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.darkCheckBox1.Location = new System.Drawing.Point(33, 76);
            this.darkCheckBox1.Name = "darkCheckBox1";
            this.darkCheckBox1.Size = new System.Drawing.Size(277, 17);
            this.darkCheckBox1.TabIndex = 24;
            this.darkCheckBox1.Text = "Remap animated textures too";
            // 
            // FormTextureRemap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(724, 697);
            this.Controls.Add(this.scalingFactor);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOk);
            this.Controls.Add(this.darkLabel4);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.cbUntextureCompletely);
            this.Controls.Add(this.darkCheckBox1);
            this.Controls.Add(this.cbRestrictToSelectedRooms);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimizeBox = false;
            this.Name = "FormTextureRemap";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Texture remapping assistant";
            this.sourcePanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.destinationPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scalingFactor)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel sourcePanel;
        private DarkUI.Controls.DarkComboBox comboSourceTexture;
        private PanelTextureMapForRemap sourceTextureMap;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel destinationPanel;
        private DarkUI.Controls.DarkComboBox comboDestinationTexture;
        private PanelTextureMapForRemap destinationTextureMap;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkCheckBox cbRestrictToSelectedRooms;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkCheckBox cbUntextureCompletely;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkNumericUpDown scalingFactor;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkCheckBox darkCheckBox1;
    }
}