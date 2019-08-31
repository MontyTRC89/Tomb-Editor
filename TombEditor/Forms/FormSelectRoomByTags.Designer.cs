using System.Windows.Forms;

namespace TombEditor.Forms
{
    partial class FormSelectRoomByTags
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
            this.butOk = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.tbTagSearch = new DarkUI.Controls.DarkTextBox();
            this.cbAllTags = new DarkUI.Controls.DarkCheckBox();
            this.tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Location = new System.Drawing.Point(217, 39);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 1;
            this.butOk.Text = "OK";
            this.butOk.Click += new System.EventHandler(this.ButOk_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(303, 39);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 2;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.ButCancel_Click);
            // 
            // darkLabel1
            // 
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(12, 9);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(35, 20);
            this.darkLabel1.TabIndex = 2;
            this.darkLabel1.Text = "Tags:";
            this.darkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbTagSearch
            // 
            this.tbTagSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTagSearch.Location = new System.Drawing.Point(53, 11);
            this.tbTagSearch.Name = "tbTagSearch";
            this.tbTagSearch.Size = new System.Drawing.Size(330, 22);
            this.tbTagSearch.TabIndex = 0;
            // 
            // cbAllTags
            // 
            this.cbAllTags.AutoSize = true;
            this.cbAllTags.Location = new System.Drawing.Point(53, 43);
            this.cbAllTags.Name = "cbAllTags";
            this.cbAllTags.Size = new System.Drawing.Size(140, 17);
            this.cbAllTags.TabIndex = 3;
            this.cbAllTags.Tag = "cbAllTags";
            this.cbAllTags.Text = "All tags instead of any";
            this.tooltip.SetToolTip(this.cbAllTags, "Only Select Rooms that contain the specified tags instead of containing any");
            this.cbAllTags.CheckedChanged += new System.EventHandler(this.CbAllTags_CheckedChanged);
            // 
            // FormSelectRoomByTags
            // 
            this.AcceptButton = this.butOk;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(393, 72);
            this.Controls.Add(this.cbAllTags);
            this.Controls.Add(this.tbTagSearch);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOk);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(409, 110);
            this.Name = "FormSelectRoomByTags";
            this.ShowInTaskbar = false;
            this.Text = "Select Rooms by Tags";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkLabel darkLabel1;
        public DarkUI.Controls.DarkTextBox tbTagSearch;
        public DarkUI.Controls.DarkCheckBox cbAllTags;
        private ToolTip tooltip;
    }
}
