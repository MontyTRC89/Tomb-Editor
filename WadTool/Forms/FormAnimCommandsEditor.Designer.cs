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
            this.btCancel = new DarkUI.Controls.DarkButton();
            this.btOk = new DarkUI.Controls.DarkButton();
            this.butAddEffect = new DarkUI.Controls.DarkButton();
            this.butDeleteEffect = new DarkUI.Controls.DarkButton();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.lstCommands = new DarkUI.Controls.DarkListView();
            this.butCommandDown = new DarkUI.Controls.DarkButton();
            this.butCommandUp = new DarkUI.Controls.DarkButton();
            this.animCommandEditor = new WadTool.AnimCommandEditor();
            this.darkGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(296, 292);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(80, 23);
            this.btCancel.TabIndex = 50;
            this.btCancel.Text = "Cancel";
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Location = new System.Drawing.Point(210, 292);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(80, 23);
            this.btOk.TabIndex = 51;
            this.btOk.Text = "OK";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // butAddEffect
            // 
            this.butAddEffect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddEffect.Image = global::WadTool.Properties.Resources.general_plus_math_16;
            this.butAddEffect.Location = new System.Drawing.Point(341, 6);
            this.butAddEffect.Name = "butAddEffect";
            this.butAddEffect.Size = new System.Drawing.Size(24, 24);
            this.butAddEffect.TabIndex = 95;
            this.butAddEffect.Click += new System.EventHandler(this.butAddEffect_Click);
            // 
            // butDeleteEffect
            // 
            this.butDeleteEffect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeleteEffect.Image = global::WadTool.Properties.Resources.trash_161;
            this.butDeleteEffect.Location = new System.Drawing.Point(341, 36);
            this.butDeleteEffect.Name = "butDeleteEffect";
            this.butDeleteEffect.Size = new System.Drawing.Size(24, 24);
            this.butDeleteEffect.TabIndex = 94;
            this.butDeleteEffect.Click += new System.EventHandler(this.butDeleteEffect_Click);
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkGroupBox1.Controls.Add(this.lstCommands);
            this.darkGroupBox1.Controls.Add(this.butCommandDown);
            this.darkGroupBox1.Controls.Add(this.butCommandUp);
            this.darkGroupBox1.Controls.Add(this.butAddEffect);
            this.darkGroupBox1.Controls.Add(this.butDeleteEffect);
            this.darkGroupBox1.Location = new System.Drawing.Point(5, 5);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(371, 134);
            this.darkGroupBox1.TabIndex = 99;
            this.darkGroupBox1.TabStop = false;
            // 
            // lstCommands
            // 
            this.lstCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCommands.Location = new System.Drawing.Point(6, 7);
            this.lstCommands.Name = "lstCommands";
            this.lstCommands.Size = new System.Drawing.Size(329, 121);
            this.lstCommands.TabIndex = 98;
            this.lstCommands.Text = "darkListView1";
            this.lstCommands.SelectedIndicesChanged += new System.EventHandler(this.lstCommands_SelectedIndicesChanged);
            // 
            // butCommandDown
            // 
            this.butCommandDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCommandDown.Image = global::WadTool.Properties.Resources.general_ArrowDown_16;
            this.butCommandDown.Location = new System.Drawing.Point(341, 104);
            this.butCommandDown.Name = "butCommandDown";
            this.butCommandDown.Size = new System.Drawing.Size(24, 24);
            this.butCommandDown.TabIndex = 97;
            this.butCommandDown.Click += new System.EventHandler(this.butCommandDown_Click);
            // 
            // butCommandUp
            // 
            this.butCommandUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCommandUp.Image = global::WadTool.Properties.Resources.general_ArrowUp_16;
            this.butCommandUp.Location = new System.Drawing.Point(341, 76);
            this.butCommandUp.Name = "butCommandUp";
            this.butCommandUp.Size = new System.Drawing.Size(24, 24);
            this.butCommandUp.TabIndex = 96;
            this.butCommandUp.Click += new System.EventHandler(this.butCommandUp_Click);
            // 
            // animCommandEditor
            // 
            this.animCommandEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.animCommandEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.animCommandEditor.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.animCommandEditor.Location = new System.Drawing.Point(5, 145);
            this.animCommandEditor.Name = "animCommandEditor";
            this.animCommandEditor.Size = new System.Drawing.Size(371, 141);
            this.animCommandEditor.TabIndex = 100;
            this.animCommandEditor.AnimCommandChanged += new System.EventHandler<WadTool.AnimCommandEditor.AnimCommandEventArgs>(this.animCommandEditor_AnimCommandChanged);
            // 
            // FormAnimCommandsEditor
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(381, 320);
            this.Controls.Add(this.animCommandEditor);
            this.Controls.Add(this.darkGroupBox1);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(397, 358);
            this.Name = "FormAnimCommandsEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Anim commands editor";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormAnimCommandsEditor_KeyDown);
            this.darkGroupBox1.ResumeLayout(false);
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
        private DarkUI.Controls.DarkListView lstCommands;
        private AnimCommandEditor animCommandEditor;
    }
}