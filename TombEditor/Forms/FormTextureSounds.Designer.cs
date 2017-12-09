using DarkUI.Controls;
using System.Windows.Forms;

namespace TombEditor
{
    partial class FormTextureSounds
    {
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
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
            this.textureMap.Location = new System.Drawing.Point(8, 9);
            this.textureMap.Name = "textureMap";
            this.textureMap.Size = new System.Drawing.Size(409, 517);
            this.textureMap.TabIndex = 0;
            // 
            // butAssignSound
            // 
            this.butAssignSound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.butAssignSound.Location = new System.Drawing.Point(183, 532);
            this.butAssignSound.Name = "butAssignSound";
            this.butAssignSound.Size = new System.Drawing.Size(113, 24);
            this.butAssignSound.TabIndex = 2;
            this.butAssignSound.Text = "Assign sound";
            this.butAssignSound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAssignSound.Click += new System.EventHandler(this.butAssignSound_Click);
            // 
            // comboSounds
            // 
            this.comboSounds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboSounds.Location = new System.Drawing.Point(8, 533);
            this.comboSounds.Name = "comboSounds";
            this.comboSounds.Size = new System.Drawing.Size(169, 23);
            this.comboSounds.Sorted = true;
            this.comboSounds.TabIndex = 1;
            this.comboSounds.Text = null;
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butOk.Location = new System.Drawing.Point(303, 532);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(114, 24);
            this.butOk.TabIndex = 3;
            this.butOk.Text = "Ok";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // FormTextureSounds
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butOk;
            this.ClientSize = new System.Drawing.Size(424, 562);
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