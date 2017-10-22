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
using TombLib.Wad.Catalog;

namespace WadTool
{
    public partial class FormSelectSlot : DarkUI.Forms.DarkForm
    {
        public bool IsMoveable { get; set; }
        public uint ObjectId { get; set; }
        public string ObjectName { get; set; }

        private WadToolClass _tool = WadToolClass.Instance;

        public FormSelectSlot()
        {
            InitializeComponent();
        }

        private void FormSelectSlot_Load(object sender, EventArgs e)
        {
            if (IsMoveable)
            {
                foreach (var moveable in TrCatalog.GetAllMoveables(_tool.DestinationWad.Version))
                {
                    var nodeMoveable = new DarkUI.Controls.DarkTreeNode(moveable.Value.ToString());
                    nodeMoveable.Tag = moveable.Key;

                    treeSlots.Nodes.Add(nodeMoveable);
                }
            }
            else
            {
                foreach (var staticMesh in TrCatalog.GetAllStaticMeshes(_tool.DestinationWad.Version))
                {
                    var nodeStatic = new DarkUI.Controls.DarkTreeNode(staticMesh.Value.ToString());
                    nodeStatic.Tag = staticMesh.Key;

                    treeSlots.Nodes.Add(nodeStatic);
                }
            }
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (treeSlots.SelectedNodes.Count == 0) return;

            var node = treeSlots.SelectedNodes[0];

            if (node.Tag == null) return;

            ObjectId = (uint)node.Tag;  
            ObjectName = node.Text;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
