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
    public partial class FormStaticMeshEditor : DarkUI.Forms.DarkForm
    {
        public WadStatic StaticMesh { get; set; }

        private WadToolClass _tool;

        public FormStaticMeshEditor()
        {
            InitializeComponent();
            _tool = WadToolClass.Instance;
            panelRendering.InitializePanel(_tool.Device);
        }

        private void FormStaticMeshEditor_Load(object sender, EventArgs e)
        {
            panelRendering.StaticMesh = StaticMesh;
        }
    }
}
