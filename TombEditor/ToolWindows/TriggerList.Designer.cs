namespace TombEditor.ToolWindows
{
    partial class TriggerList
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lstTriggers = new DarkUI.Controls.DarkListBox(this.components);
            this.butAddTrigger = new DarkUI.Controls.DarkButton();
            this.butEditTrigger = new DarkUI.Controls.DarkButton();
            this.butDeleteTrigger = new DarkUI.Controls.DarkButton();
            this.SuspendLayout();
            // 
            // lstTriggers
            // 
            this.lstTriggers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.lstTriggers.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstTriggers.ForeColor = System.Drawing.Color.White;
            this.lstTriggers.FormattingEnabled = true;
            this.lstTriggers.ItemHeight = 18;
            this.lstTriggers.Location = new System.Drawing.Point(5, 29);
            this.lstTriggers.Name = "lstTriggers";
            this.lstTriggers.Size = new System.Drawing.Size(260, 58);
            this.lstTriggers.TabIndex = 55;
            this.lstTriggers.SelectedIndexChanged += new System.EventHandler(this.lstTriggers_SelectedIndexChanged);
            // 
            // butAddTrigger
            // 
            this.butAddTrigger.Image = global::TombEditor.Properties.Resources.plus_math_16;
            this.butAddTrigger.Location = new System.Drawing.Point(181, 93);
            this.butAddTrigger.Name = "butAddTrigger";
            this.butAddTrigger.Padding = new System.Windows.Forms.Padding(5);
            this.butAddTrigger.Size = new System.Drawing.Size(24, 24);
            this.butAddTrigger.TabIndex = 56;
            this.butAddTrigger.Click += new System.EventHandler(this.butAddTrigger_Click);
            // 
            // butEditTrigger
            // 
            this.butEditTrigger.Image = global::TombEditor.Properties.Resources.edit_16;
            this.butEditTrigger.Location = new System.Drawing.Point(211, 93);
            this.butEditTrigger.Name = "butEditTrigger";
            this.butEditTrigger.Padding = new System.Windows.Forms.Padding(5);
            this.butEditTrigger.Size = new System.Drawing.Size(24, 24);
            this.butEditTrigger.TabIndex = 53;
            this.butEditTrigger.Click += new System.EventHandler(this.butEditTrigger_Click);
            // 
            // butDeleteTrigger
            // 
            this.butDeleteTrigger.Image = global::TombEditor.Properties.Resources.trash_16;
            this.butDeleteTrigger.Location = new System.Drawing.Point(241, 93);
            this.butDeleteTrigger.Name = "butDeleteTrigger";
            this.butDeleteTrigger.Padding = new System.Windows.Forms.Padding(5);
            this.butDeleteTrigger.Size = new System.Drawing.Size(24, 24);
            this.butDeleteTrigger.TabIndex = 52;
            this.butDeleteTrigger.Click += new System.EventHandler(this.butDeleteTrigger_Click);
            // 
            // TriggerList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.butAddTrigger);
            this.Controls.Add(this.lstTriggers);
            this.Controls.Add(this.butEditTrigger);
            this.Controls.Add(this.butDeleteTrigger);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Left;
            this.DockText = "Triggers";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MaximumSize = new System.Drawing.Size(268, 124);
            this.Name = "TriggerList";
            this.Size = new System.Drawing.Size(268, 124);
            this.ResumeLayout(false);

        }

        #endregion

        private DarkUI.Controls.DarkButton butAddTrigger;
        private DarkUI.Controls.DarkListBox lstTriggers;
        private DarkUI.Controls.DarkButton butEditTrigger;
        private DarkUI.Controls.DarkButton butDeleteTrigger;
    }
}
