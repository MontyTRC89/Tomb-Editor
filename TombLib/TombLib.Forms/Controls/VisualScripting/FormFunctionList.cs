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
    public partial class FormFunctionList : PopUpWindow
    {
        private Control _callbackControl;

        public NodeFunction SelectedFunction { get; private set; }

        public FormFunctionList(Point position, Control callbackControl, List<NodeFunction> functions) : base(position, true)
        {
            InitializeComponent();
            _callbackControl = callbackControl;

            treeFunctions.Nodes.Clear();

            lblDesc.ForeColor = Colors.DisabledText;

            var nodes = new List<DarkTreeNode>();
            var groups = functions.OfType<NodeFunction>().GroupBy(f => f.Section);

            DarkTreeNode neededNode = null;

            foreach (var group in groups)
            {
                var rootNode = new DarkTreeNode(group.Key);

                foreach (var item in group.OrderBy(f => !f.Conditional))
                {
                    var newNode = new DarkTreeNode(item.Name) { Tag = item };

                    if (item.Conditional)
                        newNode.Icon = Properties.Resources.general_NodeCondition_16;
                    else
                        newNode.Icon = Properties.Resources.general_NodeAction_16;

                    rootNode.Nodes.Add(newNode);

                    if (item == ((callbackControl as DarkSearchableComboBox)?.SelectedItem ?? null))
                        neededNode = newNode;
                }

                treeFunctions.Nodes.Add(rootNode);
            }

            if (neededNode != null)
            {
                treeFunctions.SelectNode(neededNode);
                treeFunctions.EnsureVisible();
            }

            txtSearch.Focus(); 
        }

        private void SearchNodes(string text)
        {
            var nodes = treeFunctions.GetAllNodes().Where(n => n.Nodes.Count == 0 && !string.IsNullOrEmpty(n.Text)).ToList();
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
            {
                treeFunctions.SelectNode(nodes[currNodeIndex]);
                treeFunctions.EnsureVisible();
            }
        }

        private void TryApplyFunction()
        {
            if (SelectedFunction == null)
                return;

            if (_callbackControl is DarkSearchableComboBox)
                (_callbackControl as DarkSearchableComboBox).SelectedItem = SelectedFunction;

            Close();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtSearch.Focus();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            if (!Disposing)
            {
                SelectedFunction = null;
                Close();
            }
        }

        private void treeFunctions_SelectedNodesChanged(object sender, EventArgs e)
        {
            lblDesc.Text = string.Empty;

            if (treeFunctions.SelectedNodes.Count == 0 ||
                treeFunctions.SelectedNodes.First().Nodes.Count != 0)
                return;

            var func = treeFunctions.SelectedNodes.FirstOrDefault()?.Tag as NodeFunction;
            SelectedFunction = func;

            if (func != null)
                lblDesc.Text = TextExtensions.SingleLineToMultiLine(func.Description);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                SearchNodes(txtSearch.Text);
        }

        private void treeFunctions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                TryApplyFunction();
        }

        private void butSearch_Click(object sender, EventArgs e) => SearchNodes(txtSearch.Text);
        private void treeFunctions_MouseDoubleClick(object sender, MouseEventArgs e) => TryApplyFunction();
    }
}