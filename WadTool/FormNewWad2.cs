using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormNewWad2 : DarkUI.Forms.DarkForm
    {
        public WadTombRaiderVersion Version { get; set; }
        public WadSoundManagementSystem SoundManagementSystem { get; set; }
        public bool IsNG { get; set; }

        public FormNewWad2()
        {
            InitializeComponent();

            comboSoundSystem.Items.Add(WadSoundManagementSystem.ClassicTrle);
            comboSoundSystem.Items.Add(WadSoundManagementSystem.DynamicSoundMap);

            //comboGameVersion.Items.Add(WadTombRaiderVersion.TR2);
            //comboGameVersion.Items.Add(WadTombRaiderVersion.TR3);
            comboGameVersion.Items.Add(WadTombRaiderVersion.TR4);
            comboGameVersion.Items.Add(WadTombRaiderVersion.TR5);

            comboGameVersion.SelectedItem = WadTombRaiderVersion.TR4;
            comboSoundSystem.SelectedItem = WadSoundManagementSystem.ClassicTrle;
            comboSoundSystem.Enabled = false;
        }

        private void comboGameVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboGameVersion.SelectedItem == null)
                return;
            var version = (WadTombRaiderVersion)comboGameVersion.SelectedItem;
            if (version == WadTombRaiderVersion.TR4)
                cbNG.Visible = true;
            else
            {
                cbNG.Visible = false;
                cbNG.Checked = false;
            }

            if (version >= WadTombRaiderVersion.TR4)
            {
                panelSoundManagement.Visible = true;
                comboSoundSystem.SelectedIndex = 0;
            }
            else
                panelSoundManagement.Visible = false;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butCreate_Click(object sender, EventArgs e)
        {
            Version = (WadTombRaiderVersion)comboGameVersion.SelectedItem;
            if (Version >= WadTombRaiderVersion.TR4)
                SoundManagementSystem = (WadSoundManagementSystem)comboSoundSystem.SelectedItem;
            else
                SoundManagementSystem = WadSoundManagementSystem.ClassicTrle;
            if (Version == WadTombRaiderVersion.TR4)
                IsNG = cbNG.Checked;
            else
                IsNG = false;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void comboSoundSystem_SelectedValueChanged(object sender, EventArgs e)
        {
            if (comboSoundSystem.SelectedItem == null)
                return;
            var system = (WadSoundManagementSystem)comboSoundSystem.SelectedItem;
            labelSoundSystem.Visible = (system == WadSoundManagementSystem.DynamicSoundMap);
        }
    }
}
