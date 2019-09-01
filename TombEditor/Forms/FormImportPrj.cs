using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using System.IO;

namespace TombEditor.Forms
{
    public partial class FormImportPrj : DarkForm
    {
        public string PrjPath { get; set; }

        public string SoundsPath { get; set; }
        public bool RespectMousepatchOnFlybyHandling { get; set; }
        public bool UseHalfPixelCorrection { get; set; }

        public FormImportPrj(string prjPath, bool respectMousepatch, bool useHalfPixelCorrection)
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(prjPath))
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }

            Text = "Import PRJ (" + Path.GetFileNameWithoutExtension(prjPath) + ")";
            PrjPath = prjPath;
            cbRespectMousepatch.Checked = respectMousepatch;
            cbUseHalfPixelCorrection.Checked = useHalfPixelCorrection;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            SoundsPath = tbTxtPath.Text;
            RespectMousepatchOnFlybyHandling = cbRespectMousepatch.Checked;
            UseHalfPixelCorrection = cbUseHalfPixelCorrection.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ButBrowseTxt_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, "Select sounds catalog to import",
                                                     LevelSettings.FileFormatsSoundsCatalogs,
                                                     false);
            if (result != null)
                tbTxtPath.Text = result;
        }
    }
}
