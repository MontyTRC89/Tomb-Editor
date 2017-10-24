namespace WadTool
{
    partial class FormSpriteSequencesEditor
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
            this.lstSequences = new DarkUI.Controls.DarkListView();
            this.butAddNewSequence = new DarkUI.Controls.DarkButton();
            this.butDeleteSequence = new DarkUI.Controls.DarkButton();
            this.butEditSequence = new DarkUI.Controls.DarkButton();
            this.SuspendLayout();
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 142);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(408, 24);
            this.darkStatusStrip1.TabIndex = 0;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // lstSequences
            // 
            this.lstSequences.Location = new System.Drawing.Point(13, 12);
            this.lstSequences.Name = "lstSequences";
            this.lstSequences.Size = new System.Drawing.Size(252, 124);
            this.lstSequences.TabIndex = 20;
            this.lstSequences.Text = "darkListView1";
            this.lstSequences.DoubleClick += new System.EventHandler(this.lstSequences_DoubleClick);
            // 
            // butAddNewSequence
            // 
            this.butAddNewSequence.Image = global::WadTool.Properties.Resources.plus_math_16;
            this.butAddNewSequence.Location = new System.Drawing.Point(271, 12);
            this.butAddNewSequence.Name = "butAddNewSequence";
            this.butAddNewSequence.Padding = new System.Windows.Forms.Padding(5);
            this.butAddNewSequence.Size = new System.Drawing.Size(128, 23);
            this.butAddNewSequence.TabIndex = 22;
            this.butAddNewSequence.Text = "Add new sequence";
            this.butAddNewSequence.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddNewSequence.Click += new System.EventHandler(this.butAddNewSequence_Click);
            // 
            // butDeleteSequence
            // 
            this.butDeleteSequence.Image = global::WadTool.Properties.Resources.trash_16;
            this.butDeleteSequence.Location = new System.Drawing.Point(271, 70);
            this.butDeleteSequence.Name = "butDeleteSequence";
            this.butDeleteSequence.Padding = new System.Windows.Forms.Padding(5);
            this.butDeleteSequence.Size = new System.Drawing.Size(128, 23);
            this.butDeleteSequence.TabIndex = 21;
            this.butDeleteSequence.Text = "Delete sequence";
            this.butDeleteSequence.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteSequence.Click += new System.EventHandler(this.butDeleteSequence_Click);
            // 
            // butEditSequence
            // 
            this.butEditSequence.Image = global::WadTool.Properties.Resources.edit_16;
            this.butEditSequence.Location = new System.Drawing.Point(271, 41);
            this.butEditSequence.Name = "butEditSequence";
            this.butEditSequence.Padding = new System.Windows.Forms.Padding(5);
            this.butEditSequence.Size = new System.Drawing.Size(128, 23);
            this.butEditSequence.TabIndex = 23;
            this.butEditSequence.Text = "Edit sequence";
            this.butEditSequence.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butEditSequence.Click += new System.EventHandler(this.butEditSequence_Click);
            // 
            // FormSpriteSequencesEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 166);
            this.Controls.Add(this.butEditSequence);
            this.Controls.Add(this.butAddNewSequence);
            this.Controls.Add(this.butDeleteSequence);
            this.Controls.Add(this.lstSequences);
            this.Controls.Add(this.darkStatusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSpriteSequencesEditor";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sprite editor";
            this.Load += new System.EventHandler(this.FormSpriteEditor_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private DarkUI.Controls.DarkButton butAddNewSequence;
        private DarkUI.Controls.DarkButton butDeleteSequence;
        private DarkUI.Controls.DarkListView lstSequences;
        private DarkUI.Controls.DarkButton butEditSequence;
    }
}