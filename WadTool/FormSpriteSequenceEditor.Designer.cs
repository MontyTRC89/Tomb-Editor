namespace WadTool
{
    partial class FormSpriteSequenceEditor
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
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.lstSprites = new DarkUI.Controls.DarkListView();
            this.openFileDialogSprites = new System.Windows.Forms.OpenFileDialog();
            this.comboSlot = new DarkUI.Controls.DarkComboBox();
            this.butDeleteSprite = new DarkUI.Controls.DarkButton();
            this.butSaveChanges = new DarkUI.Controls.DarkButton();
            this.butAddNewTexture = new DarkUI.Controls.DarkButton();
            this.picSprite = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picSprite)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 339);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(546, 24);
            this.darkStatusStrip1.TabIndex = 0;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(17, 17);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(97, 13);
            this.darkLabel1.TabIndex = 46;
            this.darkLabel1.Text = "Current sequence: ";
            // 
            // lstSprites
            // 
            this.lstSprites.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstSprites.Location = new System.Drawing.Point(18, 43);
            this.lstSprites.Name = "lstSprites";
            this.lstSprites.Size = new System.Drawing.Size(252, 254);
            this.lstSprites.TabIndex = 41;
            this.lstSprites.Text = "darkListView1";
            this.lstSprites.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lstSprites_MouseClick);
            // 
            // openFileDialogSprites
            // 
            this.openFileDialogSprites.Title = "Add new sprite";
            // 
            // comboSlot
            // 
            this.comboSlot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboSlot.FormattingEnabled = true;
            this.comboSlot.Location = new System.Drawing.Point(120, 14);
            this.comboSlot.Name = "comboSlot";
            this.comboSlot.Size = new System.Drawing.Size(150, 21);
            this.comboSlot.TabIndex = 47;
            this.comboSlot.Text = null;
            // 
            // butDeleteSprite
            // 
            this.butDeleteSprite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butDeleteSprite.Image = global::WadTool.Properties.Resources.trash_16;
            this.butDeleteSprite.Location = new System.Drawing.Point(152, 303);
            this.butDeleteSprite.Name = "butDeleteSprite";
            this.butDeleteSprite.Size = new System.Drawing.Size(118, 23);
            this.butDeleteSprite.TabIndex = 42;
            this.butDeleteSprite.Text = "Delete sprite";
            this.butDeleteSprite.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteSprite.Click += new System.EventHandler(this.butDeleteSprite_Click);
            // 
            // butSaveChanges
            // 
            this.butSaveChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSaveChanges.Image = global::WadTool.Properties.Resources.save_16;
            this.butSaveChanges.Location = new System.Drawing.Point(149, 303);
            this.butSaveChanges.Name = "butSaveChanges";
            this.butSaveChanges.Size = new System.Drawing.Size(112, 23);
            this.butSaveChanges.TabIndex = 45;
            this.butSaveChanges.Text = "Save changes";
            this.butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSaveChanges.Click += new System.EventHandler(this.butSaveChanges_Click);
            // 
            // butAddNewTexture
            // 
            this.butAddNewTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butAddNewTexture.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.butAddNewTexture.Location = new System.Drawing.Point(18, 303);
            this.butAddNewTexture.Name = "butAddNewTexture";
            this.butAddNewTexture.Size = new System.Drawing.Size(128, 23);
            this.butAddNewTexture.TabIndex = 43;
            this.butAddNewTexture.Text = "Add new sprite";
            this.butAddNewTexture.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddNewTexture.Click += new System.EventHandler(this.butAddNewTexture_Click);
            // 
            // picSprite
            // 
            this.picSprite.BackColor = System.Drawing.Color.White;
            this.picSprite.Location = new System.Drawing.Point(5, 41);
            this.picSprite.Name = "picSprite";
            this.picSprite.Size = new System.Drawing.Size(256, 256);
            this.picSprite.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picSprite.TabIndex = 44;
            this.picSprite.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, -2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(546, 334);
            this.tableLayoutPanel1.TabIndex = 48;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.picSprite);
            this.panel2.Controls.Add(this.butSaveChanges);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(273, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(273, 334);
            this.panel2.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.darkLabel1);
            this.panel1.Controls.Add(this.comboSlot);
            this.panel1.Controls.Add(this.butAddNewTexture);
            this.panel1.Controls.Add(this.lstSprites);
            this.panel1.Controls.Add(this.butDeleteSprite);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(273, 334);
            this.panel1.TabIndex = 0;
            // 
            // FormSpriteSequenceEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 363);
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(554, 390);
            this.Name = "FormSpriteSequenceEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sprite editor";
            ((System.ComponentModel.ISupportInitialize)(this.picSprite)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkComboBox comboSlot;
        private DarkUI.Controls.DarkListView lstSprites;
        private DarkUI.Controls.DarkButton butDeleteSprite;
        private DarkUI.Controls.DarkButton butSaveChanges;
        private DarkUI.Controls.DarkButton butAddNewTexture;
        private System.Windows.Forms.PictureBox picSprite;
        private System.Windows.Forms.OpenFileDialog openFileDialogSprites;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
    }
}