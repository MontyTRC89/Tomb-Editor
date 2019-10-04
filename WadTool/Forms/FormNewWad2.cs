using System;
using System.Windows.Forms;
using TombLib.LevelData;

namespace WadTool
{
    public partial class FormNewWad2 : DarkUI.Forms.DarkForm
    {
        public TRVersion.Game Version { get; set; }

        public FormNewWad2()
        {
            InitializeComponent();
            foreach (var version in TRVersion.NativeVersions)
                comboGameVersion.Items.Add(version);

            comboGameVersion.SelectedItem = TRVersion.Game.TR4;
            ActiveControl = butCreate;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butCreate_Click(object sender, EventArgs e)
        {
            Version = (TRVersion.Game)comboGameVersion.SelectedItem;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
