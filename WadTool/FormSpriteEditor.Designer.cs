namespace WadTool
{
    partial class FormSpriteEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSpriteEditor));
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.comboSlot = new DarkUI.Controls.DarkComboBox();
            this.lstSprites = new DarkUI.Controls.DarkListView();
            this.butDeleteSprite = new DarkUI.Controls.DarkButton();
            this.butSaveChanges = new DarkUI.Controls.DarkButton();
            this.butAddNewTexture = new DarkUI.Controls.DarkButton();
            this.picSprite = new System.Windows.Forms.PictureBox();
            this.openFileDialogSprites = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.picSprite)).BeginInit();
            this.SuspendLayout();
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.AutoSize = false;
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 335);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(546, 24);
            this.darkStatusStrip1.SizingGrip = false;
            this.darkStatusStrip1.TabIndex = 0;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(11, 13);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(97, 13);
            this.darkLabel1.TabIndex = 46;
            this.darkLabel1.Text = "Current sequence: ";
            // 
            // comboSlot
            // 
            this.comboSlot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.comboSlot.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(81)))), ((int)(((byte)(81)))));
            this.comboSlot.BorderStyle = System.Windows.Forms.ButtonBorderStyle.Solid;
            this.comboSlot.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(43)))), ((int)(((byte)(43)))));
            this.comboSlot.ButtonIcon = ((System.Drawing.Bitmap)(resources.GetObject("comboSlot.ButtonIcon")));
            this.comboSlot.DrawDropdownHoverOutline = false;
            this.comboSlot.DrawFocusRectangle = false;
            this.comboSlot.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.comboSlot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSlot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboSlot.ForeColor = System.Drawing.Color.Gainsboro;
            this.comboSlot.FormattingEnabled = true;
            this.comboSlot.Location = new System.Drawing.Point(114, 10);
            this.comboSlot.Name = "comboSlot";
            this.comboSlot.Size = new System.Drawing.Size(150, 21);
            this.comboSlot.TabIndex = 47;
            this.comboSlot.Text = null;
            this.comboSlot.TextPadding = new System.Windows.Forms.Padding(2);
            // 
            // lstSprites
            // 
            this.lstSprites.Location = new System.Drawing.Point(12, 39);
            this.lstSprites.Name = "lstSprites";
            this.lstSprites.Size = new System.Drawing.Size(252, 261);
            this.lstSprites.TabIndex = 41;
            this.lstSprites.Text = "darkListView1";
            this.lstSprites.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lstSprites_MouseClick);
            // 
            // butDeleteSprite
            // 
            this.butDeleteSprite.Image = global::WadTool.Properties.Resources.trash_16;
            this.butDeleteSprite.Location = new System.Drawing.Point(146, 306);
            this.butDeleteSprite.Name = "butDeleteSprite";
            this.butDeleteSprite.Padding = new System.Windows.Forms.Padding(5);
            this.butDeleteSprite.Size = new System.Drawing.Size(118, 23);
            this.butDeleteSprite.TabIndex = 42;
            this.butDeleteSprite.Text = "Delete sprite";
            this.butDeleteSprite.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteSprite.Click += new System.EventHandler(this.butDeleteSprite_Click);
            // 
            // butSaveChanges
            // 
            this.butSaveChanges.Image = global::WadTool.Properties.Resources.save_16;
            this.butSaveChanges.Location = new System.Drawing.Point(421, 306);
            this.butSaveChanges.Name = "butSaveChanges";
            this.butSaveChanges.Padding = new System.Windows.Forms.Padding(5);
            this.butSaveChanges.Size = new System.Drawing.Size(112, 23);
            this.butSaveChanges.TabIndex = 45;
            this.butSaveChanges.Text = "Save changes";
            this.butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSaveChanges.Click += new System.EventHandler(this.butSaveChanges_Click);
            // 
            // butAddNewTexture
            // 
            this.butAddNewTexture.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.butAddNewTexture.Location = new System.Drawing.Point(12, 306);
            this.butAddNewTexture.Name = "butAddNewTexture";
            this.butAddNewTexture.Padding = new System.Windows.Forms.Padding(5);
            this.butAddNewTexture.Size = new System.Drawing.Size(128, 23);
            this.butAddNewTexture.TabIndex = 43;
            this.butAddNewTexture.Text = "Add new sprite";
            this.butAddNewTexture.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddNewTexture.Click += new System.EventHandler(this.butAddNewTexture_Click);
            // 
            // picSprite
            // 
            this.picSprite.BackColor = System.Drawing.Color.White;
            this.picSprite.Location = new System.Drawing.Point(277, 44);
            this.picSprite.Name = "picSprite";
            this.picSprite.Size = new System.Drawing.Size(256, 256);
            this.picSprite.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picSprite.TabIndex = 44;
            this.picSprite.TabStop = false;
            // 
            // openFileDialogSprites
            // 
            this.openFileDialogSprites.Title = "Add new sprite";
            // 
            // FormSpriteEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 359);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.comboSlot);
            this.Controls.Add(this.lstSprites);
            this.Controls.Add(this.butDeleteSprite);
            this.Controls.Add(this.butSaveChanges);
            this.Controls.Add(this.butAddNewTexture);
            this.Controls.Add(this.picSprite);
            this.Controls.Add(this.darkStatusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSpriteEditor";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sprite editor";
            this.Load += new System.EventHandler(this.FormSpriteEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picSprite)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}