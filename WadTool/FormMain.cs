using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormMain : DarkUI.Forms.DarkForm
    {
        private WadToolClass _tool;

        public FormMain()
        {
            InitializeComponent();

            _tool = WadToolClass.Instance;
            _tool.Initialize();

            panel3D.InitializePanel(_tool.Device);
        }

        private void butTest_Click(object sender, EventArgs e)
        {

        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }

        private void openSourceWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butOpenSourceWad_Click(null, null);
        }

        private void treeSourceWad_MouseClick(object sender, MouseEventArgs e)
        {
            if (treeSourceWad.SelectedNodes.Count == 0) return;

            var node = treeSourceWad.SelectedNodes[0];

            if (node.Tag == null || (node.Tag.GetType() != typeof(WadMoveable) && node.Tag.GetType() != typeof(WadStatic))) return;

            panel3D.CurrentWad = _tool.SourceWad;
            panel3D.CurrentObject = (WadObject)node.Tag;
            panel3D.Invalidate();
        }

        private void openDestinationWad2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butOpenDestWad2_Click(null, null);
        }

        private void treeDestWad_MouseClick(object sender, MouseEventArgs e)
        {
            if (treeDestWad.SelectedNodes.Count == 0) return;

            var node = treeDestWad.SelectedNodes[0];

            if (node.Tag == null || (node.Tag.GetType() != typeof(WadMoveable) && node.Tag.GetType() != typeof(WadStatic))) return;

            panel3D.CurrentWad = _tool.DestinationWad;
            panel3D.CurrentObject = (WadObject)node.Tag;
            panel3D.Invalidate();
        }

        private void butOpenDestWad2_Click(object sender, EventArgs e)
        {
            // Open the file dialog
            openFileDialogWad.Filter = "Tomb Editor Wad2 (*.wad2)|*.wad2";
            openFileDialogWad.Title = "Open destination Wad2";
            if (openFileDialogWad.ShowDialog() == DialogResult.Cancel) return;

            // Load the Wad2
            string fileName = openFileDialogWad.FileName.ToLower();
            using (var stream = File.OpenRead(fileName))
            {
                var newWad = Wad2.LoadFromStream(stream);
                if (newWad == null) return;

                if (_tool.DestinationWad != null) _tool.DestinationWad.Dispose();

                newWad.GraphicsDevice = _tool.Device;
                newWad.PrepareDataForDirectX();
                _tool.DestinationWad = newWad;
            }

            // Update the UI
            treeSourceWad.Nodes.Clear();

            var nodeMoveables = new DarkUI.Controls.DarkTreeNode("Moveables");
            treeDestWad.Nodes.Add(nodeMoveables);

            foreach (var moveable in _tool.DestinationWad.Moveables)
            {
                var nodeMoveable = new DarkUI.Controls.DarkTreeNode(moveable.Value.ToString());
                nodeMoveable.Tag = moveable.Value;

                treeDestWad.Nodes[0].Nodes.Add(nodeMoveable);
            }

            var nodeStatics = new DarkUI.Controls.DarkTreeNode("Statics");
            treeDestWad.Nodes.Add(nodeStatics);

            foreach (var staticMesh in _tool.DestinationWad.Statics)
            {
                var nodeStatic = new DarkUI.Controls.DarkTreeNode(staticMesh.Value.ToString());
                nodeStatic.Tag = staticMesh.Value;

                treeDestWad.Nodes[1].Nodes.Add(nodeStatic);
            }
        }

        private void butOpenSourceWad_Click(object sender, EventArgs e)
        {
            // Open the file dialog
            openFileDialogWad.Filter = "Tomb Raider WAD (*.wad)|*.wad|Tomb Editor Wad2 (*.wad2)|*.wad2";
            openFileDialogWad.Title = "Open source WAD/Wad2";
            if (openFileDialogWad.ShowDialog() == DialogResult.Cancel) return;

            // Load the WAD/Wad2
            string fileName = openFileDialogWad.FileName.ToLower();
            if (fileName.EndsWith(".wad"))
            {
                TR4Wad originalWad = new TR4Wad();
                originalWad.LoadWad(fileName);

                var newWad = WadOperations.ConvertTr4Wad(originalWad);
                if (newWad == null) return;

                if (_tool.SourceWad != null) _tool.SourceWad.Dispose();

                newWad.GraphicsDevice = _tool.Device;
                newWad.PrepareDataForDirectX();
                _tool.SourceWad = newWad;
            }
            else
            {
                using (var stream = File.OpenRead(fileName))
                {
                    var newWad = Wad2.LoadFromStream(stream);
                    if (newWad == null) return;

                    if (_tool.SourceWad != null) _tool.SourceWad.Dispose();

                    newWad.GraphicsDevice = _tool.Device;
                    newWad.PrepareDataForDirectX();
                    _tool.SourceWad = newWad;
                }
            }

            // Update the UI
            treeSourceWad.Nodes.Clear();

            var nodeMoveables = new DarkUI.Controls.DarkTreeNode("Moveables");
            treeSourceWad.Nodes.Add(nodeMoveables);

            foreach (var moveable in _tool.SourceWad.Moveables)
            {
                var nodeMoveable = new DarkUI.Controls.DarkTreeNode(moveable.Value.ToString());
                nodeMoveable.Tag = moveable.Value;

                treeSourceWad.Nodes[0].Nodes.Add(nodeMoveable);
            }

            var nodeStatics = new DarkUI.Controls.DarkTreeNode("Statics");
            treeSourceWad.Nodes.Add(nodeStatics);

            foreach (var staticMesh in _tool.SourceWad.Statics)
            {
                var nodeStatic = new DarkUI.Controls.DarkTreeNode(staticMesh.Value.ToString());
                nodeStatic.Tag = staticMesh.Value;

                treeSourceWad.Nodes[1].Nodes.Add(nodeStatic);
            }
        }
    }
}
