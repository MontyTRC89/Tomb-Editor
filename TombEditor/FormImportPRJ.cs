using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombEditor.Compilers;
using TombEditor.Geometry;

namespace TombEditor
{
    public partial class FormImportPRJ : DarkForm
    {
        public string FileName { get; set; }
        public Level Level { get; set; }

        private Editor _editor = Editor.Instance;

        public FormImportPRJ()
        {
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = (Level != null ? DialogResult.OK : DialogResult.Cancel);
            this.Close();
        }

        private void FormObject_Load(object sender, EventArgs e)
        {
              
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Level = Level.LoadFromPrj(FileName, this);
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string message = e.UserState as string;

            pbStato.Value = e.ProgressPercentage;

            if (message != null && message != "")
            {
                lstLog.Items.Add(message);
                lstLog.SelectedIndex = lstLog.Items.Count - 1;
                lstLog.TopIndex = lstLog.Items.Count - 1;
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pbStato.Value = 100;
        }

        private void FormBuildLevel_Shown(object sender, EventArgs e)
        {
            GC.Collect();
            Level = Level.LoadFromPrj(FileName, this);
            GC.Collect();
        }

        public void ReportProgress(int progress, string message)
        {
            pbStato.Value = progress;

            if (message != null && message != "")
            {
                lstLog.Items.Add(message);
                lstLog.SelectedIndex = lstLog.Items.Count - 1;
                lstLog.TopIndex = lstLog.Items.Count - 1;
            }

            this.Refresh();
            Application.DoEvents();
        }

        public string OpenTGA()
        {
            if (openFileDialogTGA.ShowDialog() != DialogResult.OK)
                return "";
            return openFileDialogTGA.FileName;
        }

        public string OpenWAD()
        {
            if (openFileDialogWAD.ShowDialog() != DialogResult.OK)
                return "";
            return openFileDialogWAD.FileName;
        }
    }
}
