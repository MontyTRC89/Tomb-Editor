using DarkUI.Controls;

namespace TombEditor
{
    partial class FormBuildLevel
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
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.bw = new System.ComponentModel.BackgroundWorker();
            this.pbStato = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Location = new System.Drawing.Point(286, 378);
            this.butCancel.Name = "butCancel";
            this.butCancel.Padding = new System.Windows.Forms.Padding(5);
            this.butCancel.Size = new System.Drawing.Size(86, 23);
            this.butCancel.TabIndex = 1;
            this.butCancel.Text = "Close";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // lstLog
            // 
            this.lstLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstLog.FormattingEnabled = true;
            this.lstLog.Location = new System.Drawing.Point(13, 13);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(629, 325);
            this.lstLog.TabIndex = 2;
            // 
            // bw
            // 
            this.bw.WorkerReportsProgress = true;
            this.bw.WorkerSupportsCancellation = true;
            this.bw.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bw_DoWork);
            this.bw.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bw_ProgressChanged);
            this.bw.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bw_RunWorkerCompleted);
            // 
            // pbStato
            // 
            this.pbStato.Location = new System.Drawing.Point(13, 345);
            this.pbStato.Name = "pbStato";
            this.pbStato.Size = new System.Drawing.Size(629, 23);
            this.pbStato.TabIndex = 3;
            // 
            // FormBuildLevel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(654, 413);
            this.Controls.Add(this.pbStato);
            this.Controls.Add(this.lstLog);
            this.Controls.Add(this.butCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormBuildLevel";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Build level";
            this.Load += new System.EventHandler(this.FormObject_Load);
            this.Shown += new System.EventHandler(this.FormBuildLevel_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private DarkButton butCancel;
        private System.Windows.Forms.ListBox lstLog;
        private System.ComponentModel.BackgroundWorker bw;
        private System.Windows.Forms.ProgressBar pbStato;
    }
}