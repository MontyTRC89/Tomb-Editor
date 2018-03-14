using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormMesh : DarkUI.Forms.DarkForm
    {
        private Wad2 _wad;
        private DeviceManager _deviceManager;
        private WadToolClass _tool;

        public FormMesh(WadToolClass tool, DeviceManager deviceManager, Wad2 wad)
        {
            InitializeComponent();

            _tool = tool;
            _wad = wad;
            _deviceManager = deviceManager;

            panelMesh.InitializePanel(_tool, _deviceManager);

            lstMeshes.DataSource = new BindingList<WadMesh>(new List<WadMesh>(_wad.MeshesUnique));
        }

        private void lstMeshes_Click(object sender, EventArgs e)
        {
            // Update big image view
            if (lstMeshes.SelectedRows.Count <= 0)
                return;

            panelMesh.Mesh = ((WadMesh)lstMeshes.SelectedRows[0].DataBoundItem);
            panelMesh.Invalidate();
        }
    }
}
