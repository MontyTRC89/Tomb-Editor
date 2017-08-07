using DarkUI.Forms;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using TombEditor.Geometry;
using TombEditor.Geometry.IO;

namespace TombEditor
{
    public partial class FormImportPRJ : DarkForm, IProgressReporter
    {
        public string FileName { get; set; }
        public Level Level { get; set; }
        
        private DeviceManager _deviceManager;

        public FormImportPRJ(DeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = (Level != null ? DialogResult.OK : DialogResult.Cancel);
            this.Close();
        }
        
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Level = PrjLoader.LoadFromPrj(FileName, _deviceManager.Device, this);
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
            Level = PrjLoader.LoadFromPrj(FileName, _deviceManager.Device, this);
        }

        public void ReportProgress(int progress, string message)
        {
            pbStato.Value = progress;

            if (!string.IsNullOrEmpty(message))
            {
                lstLog.Items.Add(message);
                lstLog.SelectedIndex = lstLog.Items.Count - 1;
                lstLog.TopIndex = lstLog.Items.Count - 1;
            }

            this.Refresh();
            Application.DoEvents();
        }
    }
}
