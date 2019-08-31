using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormImportPrj : DarkForm
    {
        public string PrjPath { get; set; }
        public string SoundsPath { get; set; }

        public FormImportPrj()
        {
            InitializeComponent();
        }
        private void butOk_Click(object sender, EventArgs e)
        {
            if (tbPrjPath.Text.Trim() == "")
            {
                DarkMessageBox.Show(this, "You must select a PRJ file to import", "Error", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            PrjPath = tbPrjPath.Text;
            SoundsPath = tbTxtPath.Text;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ButBrowsePrj_Click(object sender, EventArgs e)
        {
            string result = LevelFileDialog.BrowseFile(this, "Select PRJ to import",
                                                      LevelSettings.FileFormatsLevelPrj,
                                                      false);
            if (result != null)
                tbPrjPath.Text = result;
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
