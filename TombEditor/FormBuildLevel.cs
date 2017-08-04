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
    public partial class FormBuildLevel : DarkForm
    {
        public bool AutoCloseWhenDone { get; set; }
        private Editor _editor = Editor.Instance;

        public FormBuildLevel()
        {
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormObject_Load(object sender, EventArgs e)
        {

        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            string baseName = System.IO.Path.GetFileNameWithoutExtension(_editor.Level.WadFile);

            GC.Collect();

            LevelCompilerTr4 comp = new LevelCompilerTr4(_editor.Level, "Game\\Data\\" + baseName + ".tr4", bw);
            comp.CompileLevel();
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string message = e.UserState as string;

            pbStato.Value = e.ProgressPercentage;

            lstLog.Items.Add(message);
            lstLog.SelectedIndex = lstLog.Items.Count - 1;
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            GC.Collect();

            pbStato.Value = 100;

            if (AutoCloseWhenDone)
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void FormBuildLevel_Shown(object sender, EventArgs e)
        {
            bw.RunWorkerAsync();
        }
    }
}
