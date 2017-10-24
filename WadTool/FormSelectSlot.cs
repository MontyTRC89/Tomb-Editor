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
        public int ObjectId { get; set; }
        public string ObjectName { get; set; }

        private WadToolClass _tool = WadToolClass.Instance;

        public FormSelectSlot()
        {
            InitializeComponent();
        }

        private void FormSelectSlot_Load(object sender, EventArgs e)
        {
            ReloadSlots();
        }

        private void ReloadSlots()
        {
            treeSlots.Nodes.Clear();
            treeSlots.Invalidate();

            if (IsMoveable)
            {
                var moveables = TrCatalog.GetAllMoveables(_tool.DestinationWad.Version);
                var nodes = new List<DarkUI.Controls.DarkTreeNode>();
                foreach (var moveable in moveables)
                {
                    if (tbSearch.Text != "" && !moveable.Value.ToString().ToLower().Contains(tbSearch.Text.ToLower())) continue;

                    var nodeMoveable = new DarkUI.Controls.DarkTreeNode(moveable.Value.ToString());
                    nodeMoveable.Tag = moveable.Key;

                    nodes.Add(nodeMoveable);
                }
                treeSlots.Nodes.AddRange(nodes);
            }
            else
            {
                var staticMeshes = TrCatalog.GetAllStaticMeshes(_tool.DestinationWad.Version);
                var nodes = new List<DarkUI.Controls.DarkTreeNode>();
                foreach (var staticMesh in staticMeshes)
                {
                    if (tbSearch.Text != "" && !staticMesh.Value.ToString().ToLower().Contains(tbSearch.Text.ToLower())) continue;

                    var nodeStatic = new DarkUI.Controls.DarkTreeNode(staticMesh.Value.ToString());
                    nodeStatic.Tag = staticMesh.Key;

                    nodes.Add(nodeStatic);
                }
                treeSlots.Nodes.AddRange(nodes);
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

            ObjectId = (int)node.Tag;  
            ObjectName = node.Text;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void tbSearch_KeyUp(object sender, KeyEventArgs e)
        {
            ReloadSlots();
        }
    }
}
