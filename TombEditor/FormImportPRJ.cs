using DarkUI.Forms;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using TombEditor.Geometry;
using TombEditor.Geometry.IO;

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
            Level = PrjLoader.LoadFromPrj(FileName, this, _editor.GraphicsDevice);
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
            Level = PrjLoader.LoadFromPrj(FileName, this, _editor.GraphicsDevice);
            GC.Collect();
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

        public string OpenTGA(string lookFor)
        {
            if (string.IsNullOrEmpty(lookFor))
            {
                openFileDialogTGA.Title = "Import TGA";
                openFileDialogTGA.Filter = "TGA winroomedit file (*.tga)|*.tga|All files (*.*)|*.*";
            }
            else
            {
                var fileName = Path.GetFileName(lookFor);
                
                openFileDialogTGA.Title = $"Replace {lookFor}";
                openFileDialogTGA.Filter =
                    $"{fileName}|{fileName}|TGA winroomedit file (*.tga)|*.tga|All files (*.*)|*.*";
            }
            
            if (openFileDialogTGA.ShowDialog(this) != DialogResult.OK)
                return null;
            return openFileDialogTGA.FileName;
        }

        public string OpenWAD(string lookFor)
        {
            if (string.IsNullOrEmpty(lookFor))
            {
                openFileDialogWAD.Title = "Load WAD";
                openFileDialogWAD.Filter = "Tomb Raider WAD (*.wad)|*.wad|All files (*.*)|*.*";
            }
            else
            {
                var fileName = Path.GetFileName(lookFor);
                
                openFileDialogWAD.Title = $"Replace {lookFor}";
                openFileDialogWAD.Filter = $"{fileName}|{fileName}|Tomb Raider WAD (*.wad)|*.wad|All files (*.*)|*.*";
            }

            if (openFileDialogWAD.ShowDialog(this) != DialogResult.OK)
                return "";
            return openFileDialogWAD.FileName;
        }
    }
}
