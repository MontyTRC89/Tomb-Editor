using DarkUI.Controls;
using System.Windows.Forms;

namespace TombEditor
{
    partial class FormTextureSounds
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
            this.textureMap = new TombEditor.FormTextureSounds.PanelTextureMapForSounds();
            this.butAssignSound = new DarkUI.Controls.DarkButton();
            this.comboSounds = new DarkUI.Controls.DarkComboBox();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.SuspendLayout();
            // 
            // textureMap
            // 
            this.textureMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textureMap.FreeSelection = false;
            this.textureMap.FreeSelectionWithShift = false;
            this.textureMap.Location = new System.Drawing.Point(8, 9);
            this.textureMap.Name = "textureMap";
            this.textureMap.Size = new System.Drawing.Size(577, 727);
            this.textureMap.TabIndex = 0;
            this.textureMap.TileSelectionSize = 64F;
            // 
            // butAssignSound
            // 
            this.butAssignSound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butAssignSound.Location = new System.Drawing.Point(183, 742);
            this.butAssignSound.Name = "butAssignSound";
            this.butAssignSound.Padding = new System.Windows.Forms.Padding(5);
            this.butAssignSound.Size = new System.Drawing.Size(113, 24);
            this.butAssignSound.TabIndex = 2;
            this.butAssignSound.Text = "Assign sound";
            this.butAssignSound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAssignSound.Click += new System.EventHandler(this.butAssignSound_Click);
            // 
            // comboSounds
            // 
            this.comboSounds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboSounds.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.comboSounds.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboSounds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSounds.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboSounds.ForeColor = System.Drawing.Color.White;
            this.comboSounds.FormattingEnabled = true;
            this.comboSounds.ItemHeight = 18;
            this.comboSounds.Location = new System.Drawing.Point(8, 742);
            this.comboSounds.Name = "comboSounds";
            this.comboSounds.Size = new System.Drawing.Size(169, 24);
            this.comboSounds.Sorted = true;
            this.comboSounds.TabIndex = 1;
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Location = new System.Drawing.Point(409, 742);
            this.butOk.Name = "butOk";
            this.butOk.Padding = new System.Windows.Forms.Padding(5);
            this.butOk.Size = new System.Drawing.Size(176, 24);
            this.butOk.TabIndex = 3;
            this.butOk.Text = "Ok";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // FormTextureSounds
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 750);
            this.Controls.Add(this.comboSounds);
            this.Controls.Add(this.butAssignSound);
            this.Controls.Add(this.butOk);
            this.Controls.Add(this.textureMap);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.MinimizeBox = false;
            this.Name = "FormTextureSounds";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Texture sounds";
            this.ResumeLayout(false);

        }

        #endregion
        private TombEditor.FormTextureSounds.PanelTextureMapForSounds textureMap;
        private DarkButton butAssignSound;
        private DarkComboBox comboSounds;
        private DarkButton butOk;
    }
}