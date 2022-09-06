using DarkUI.Config;
using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombLib.Controls.VisualScripting
{
    public partial class FunctionList : PopUpWindow
    {
        private DarkSearchableComboBox _callbackControl;
        private int _currentIndex = -1;

        public FunctionList(Point position, DarkSearchableComboBox callbackControl) : base(position)
        {
            InitializeComponent();
            _callbackControl = callbackControl;

            treeFunctions.Nodes.Clear();

            lblDesc.ForeColor = Colors.DisabledText;

            var nodes = new List<DarkTreeNode>();
            var groups = callbackControl.Items.OfType<NodeFunction>().GroupBy(f => f.Section);

            DarkTreeNode neededNode = null;

            foreach (var group in groups)
            {
                var rootNode = new DarkTreeNode(group.Key);

                foreach (var item in group)
                {
                    var newNode = new DarkTreeNode(item.Name) { Tag = item };
                    rootNode.Nodes.Add(newNode);

                    if (item == callbackControl.SelectedItem)
                        neededNode = newNode;
                }

                treeFunctions.Nodes.Add(rootNode);
            }

            treeFunctions.SelectNode(neededNode);
            treeFunctions.EnsureVisible();

            txtSearch.Focus(); 
        }

        private void SearchNodes(string text)
        {
            var nodes = treeFunctions.GetAllNodes().Where(n => !string.IsNullOrEmpty(n.Text)).ToList();

            var currNodeIndex = -1;

            if (treeFunctions.SelectedNodes.Count > 0)
                currNodeIndex = nodes.IndexOf(treeFunctions.SelectedNodes[0]);

            var exactMatch = nodes.Any(n => n.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1);

            for (int i = currNodeIndex + 1; i <= nodes.Count; i++)
            {
                if (i == nodes.Count)
                {
                    if (currNodeIndex == -1)
                        break; // No match
                    else
                    {
                        i = -1;
                        currNodeIndex = -1;
                        continue; // Restart search
                    }
                }

                int startIndex;
                int levenshtein = Levenshtein.DistanceSubstring(nodes[i].Text.ToLower(), text.ToLower(), out startIndex);
                var match = nodes[i].Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1;

                if ((exactMatch && match) || (!exactMatch && levenshtein < 2))
                {
                    currNodeIndex = i;
                    break;
                }
            }

            if (currNodeIndex != -1)
                treeFunctions.SelectNode(nodes[currNodeIndex]);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            Close();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            using (var pen = new Pen(Colors.DarkBackground, 2))
                e.Graphics.DrawRectangle(pen, ClientRectangle);
        }

        private void treeFunctions_SelectedNodesChanged(object sender, EventArgs e)
        {
            _currentIndex = -1;
            lblDesc.Text = string.Empty;

            if (treeFunctions.SelectedNodes.Count == 0 ||
                treeFunctions.SelectedNodes.First().Nodes.Count != 0)
                return;

            var func = treeFunctions.SelectedNodes.FirstOrDefault()?.Tag as NodeFunction;
            if (func == null)
                return;

            var index = _callbackControl.Items.IndexOf(func);
            if (index == -1)
                return;

            _currentIndex = index;
            lblDesc.Text = func.Description.Replace("\\n", Environment.NewLine);
        }

        private void butSearch_Click(object sender, EventArgs e)
        {
            SearchNodes(txtSearch.Text);
        }

        private void treeFunctions_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_currentIndex != -1)
            {
                _callbackControl.SelectedIndex = _currentIndex;
                Close();
            }
        }
    }
}
