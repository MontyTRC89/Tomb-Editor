using DarkUI.Controls;
using System.Windows.Forms;

namespace TombEditor
{
    /*partial class FormTextureSounds
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
            this.components = new System.ComponentModel.Container();
            this.panelTextureContainer = new System.Windows.Forms.Panel();
            this.picTextureMap = new TombEditor.Controls.PanelTextureSounds();
            this.butAssignSound = new DarkUI.Controls.DarkButton();
            this.comboSounds = new DarkUI.Controls.DarkComboBox(this.components);
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.imgList = new System.Windows.Forms.ImageList(this.components);
            this.timerPreview = new System.Windows.Forms.Timer(this.components);
            this.panelTextureContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTextureMap)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTextureContainer
            // 
            this.panelTextureContainer.AutoScroll = true;
            this.panelTextureContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTextureContainer.Controls.Add(this.picTextureMap);
            this.panelTextureContainer.Location = new System.Drawing.Point(317, 10);
            this.panelTextureContainer.Name = "panelTextureContainer";
            this.panelTextureContainer.Size = new System.Drawing.Size(286, 477);
            this.panelTextureContainer.TabIndex = 24;
            // 
            // picTextureMap
            // 
            this.picTextureMap.ContainerForm = null;
            this.picTextureMap.IsTextureSelected = false;
            this.picTextureMap.Location = new System.Drawing.Point(-1, 0);
            this.picTextureMap.Name = "picTextureMap";
            this.picTextureMap.Page = ((short)(0));
            this.picTextureMap.SelectedX = ((short)(0));
            this.picTextureMap.SelectedY = ((short)(0));
            this.picTextureMap.Size = new System.Drawing.Size(256, 567);
            this.picTextureMap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picTextureMap.TabIndex = 0;
            this.picTextureMap.TabStop = false;
            // 
            // butAssignSound
            // 
            this.butAssignSound.Location = new System.Drawing.Point(80, 40);
            this.butAssignSound.Name = "butAssignSound";
            this.butAssignSound.Padding = new System.Windows.Forms.Padding(5);
            this.butAssignSound.Size = new System.Drawing.Size(113, 23);
            this.butAssignSound.TabIndex = 37;
            this.butAssignSound.Text = "Assign sound";
            this.butAssignSound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAssignSound.Click += new System.EventHandler(this.butAssignSound_Click);
            // 
            // comboSounds
            // 
            this.comboSounds.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboSounds.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboSounds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSounds.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboSounds.ForeColor = System.Drawing.Color.White;
            this.comboSounds.FormattingEnabled = true;
            this.comboSounds.ItemHeight = 18;
            this.comboSounds.Items.AddRange(new object[] {
            "Mud",
            "Snow",
            "Sand",
            "Gravel",
            "Ice",
            "Water",
            "Stone",
            "Wood",
            "Metal",
            "Marble",
            "Grass",
            "Concrete",
            "OldWood",
            "OldMetal"});
            this.comboSounds.Location = new System.Drawing.Point(80, 10);
            this.comboSounds.Name = "comboSounds";
            this.comboSounds.Size = new System.Drawing.Size(218, 24);
            this.comboSounds.TabIndex = 39;
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(13, 13);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(39, 13);
            this.darkLabel3.TabIndex = 38;
            this.darkLabel3.Text = "Effect:";
            // 
            // butCancel
            // 
            this.butCancel.Location = new System.Drawing.Point(266, 505);
            this.butCancel.Name = "butCancel";
            this.butCancel.Padding = new System.Windows.Forms.Padding(5);
            this.butCancel.Size = new System.Drawing.Size(86, 23);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Close";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // imgList
            // 
            this.imgList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imgList.ImageSize = new System.Drawing.Size(64, 64);
            this.imgList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // timerPreview
            // 
            this.timerPreview.Interval = 33;
            // 
            // FormTextureSounds
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 540);
            this.Controls.Add(this.comboSounds);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.butAssignSound);
            this.Controls.Add(this.panelTextureContainer);
            this.Controls.Add(this.butCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormTextureSounds";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Texture sounds";
            this.Load += new System.EventHandler(this.FormTextureSounds_Load);
            this.panelTextureContainer.ResumeLayout(false);
            this.panelTextureContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTextureMap)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Panel panelTextureContainer;
        private Controls.PanelTextureSounds picTextureMap;
        private DarkButton butAssignSound;
        private DarkComboBox comboSounds;
        private DarkLabel darkLabel3;
        private DarkButton butCancel;
        private ImageList imgList;
        private Timer timerPreview;
    }*/
}