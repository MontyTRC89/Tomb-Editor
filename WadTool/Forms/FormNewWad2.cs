using System;
using System.Windows.Forms;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormNewWad2 : DarkUI.Forms.DarkForm
    {
        public WadGameVersion Version { get; set; }

        public FormNewWad2()
        {
            InitializeComponent();
            foreach (WadGameVersion version in Enum.GetValues(typeof(WadGameVersion)))
                comboGameVersion.Items.Add(version);

            comboGameVersion.SelectedItem = WadGameVersion.TR4_TRNG;
            ActiveControl = butCreate;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butCreate_Click(object sender, EventArgs e)
        {
            Version = (WadGameVersion)comboGameVersion.SelectedItem;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
