using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Controls;
using TombLib.Wad;
using System.Drawing;

namespace TombLib.Controls
{
    public class WadTreeView : UserControl
    {
        private Wad2 _wad;
        private DarkTreeView tree;
        private DarkComboBox suggestedGameVersionComboBox;
        private DarkLabel darkLabel1;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Wad2 Wad
        {
            get { return _wad; }
            set { _wad = value; UpdateContent(); }
        }

        private bool _changing;

        public WadTreeView()
        {
            InitializeComponent();
            tree.SelectedNodes.CollectionChanged += (s, e) => { if (!_changing) SelectedWadObjectIdsChanged?.Invoke(this, EventArgs.Empty); };

            // Populate game version
            foreach (WadGameVersion gameVersion in Enum.GetValues(typeof(WadGameVersion)))
                suggestedGameVersionComboBox.Items.Add(gameVersion);
        }

        public IEnumerable<IWadObjectId> SelectedWadObjectIds => tree.SelectedNodes.Select(node => node.Tag).OfType<IWadObjectId>();

        public event EventHandler SelectedWadObjectIdsChanged;

        public void UpdateContent()
        {
            tree.Enabled = Wad != null;
            suggestedGameVersionComboBox.Enabled = Wad != null;

            // Update game version control
            if (Wad != null)
            {
                if (!Wad.SuggestedGameVersion.Equals(suggestedGameVersionComboBox.SelectedItem))
                    suggestedGameVersionComboBox.SelectedItem = Wad.SuggestedGameVersion;
            }
            else
                suggestedGameVersionComboBox.SelectedItem = null;

            // Update tree
            KeepSelection(() =>
                {
                    var nodes = tree.Nodes.ToList();
                    tree.Nodes.Clear();
                    if (Wad == null)
                        return;

                    {
                        var mainNode = AddOrReuseChild(nodes, "Moveables");
                        UpdateList(mainNode, _wad.Moveables.Values.Select(o => o.Id), o => o.ToString(Wad.SuggestedGameVersion));
                    }
                    {
                        var mainNode = AddOrReuseChild(nodes, "Statics");
                        UpdateList(mainNode, _wad.Statics.Values.Select(o => o.Id), o => o.ToString(Wad.SuggestedGameVersion));
                    }
                    {
                        var mainNode = AddOrReuseChild(nodes, "Sprite sequences");
                        UpdateList(mainNode, _wad.SpriteSequences.Values.Select(o => o.Id), o => o.ToString(Wad.SuggestedGameVersion));
                    }
                    {
                        var mainNode = AddOrReuseChild(nodes, "Fixed sound infos");
                        UpdateList(mainNode, _wad.FixedSoundInfos.Values.Select(o => o.Id), o => o.ToString(Wad.SuggestedGameVersion));
                    }
                    if (_wad.AdditionalSoundInfos.Count > 0)
                    {
                        var mainNode = AddOrReuseChild(nodes, "Additional sound infos (legacy imported and not used by any object)");
                        UpdateList(mainNode, _wad.AdditionalSoundInfos.Values.Select(o => o.Id), o => o.ToString(Wad.SuggestedGameVersion));
                    }
                    else
                        nodes.RemoveAll((node) => node.Text.Contains("Additional sound infos"));
                    tree.Nodes.AddRange(nodes);
                });
        }

        private static DarkTreeNode AddOrReuseChild(IList<DarkTreeNode> nodes, string text)
        {
            foreach (DarkTreeNode childNode in nodes)
                if (childNode.Text.Equals(text, StringComparison.InvariantCulture))
                    return childNode;
            {
                DarkTreeNode childNode = new DarkTreeNode(text) { Expanded = false };
                nodes.Add(childNode);
                return childNode;
            }
        }

        // This function tries to recycle tree nodes to preserve their extended attribute.
        private void UpdateList<T>(DarkTreeNode oldNode, IEnumerable<T> listOfThings, Func<T, string> formatObject)
        {
            IDictionary<object, DarkTreeNode> oldChildNodeLookup = oldNode.Nodes.ToDictionary(node => node.Tag);
            var newChildNodes = new List<DarkTreeNode>();

            foreach (T thing in listOfThings)
            {
                DarkTreeNode childNode;
                if (!oldChildNodeLookup.TryGetValue(thing, out childNode))
                    childNode = new DarkTreeNode { Tag = thing, Expanded = false };
                childNode.Text = formatObject(thing);
                newChildNodes.Add(childNode);
            }

            oldNode.Nodes.Clear();
            oldNode.Nodes.AddRange(newChildNodes);
        }


        private void KeepSelection(Action update)
        {
            var selectedNodes = new HashSet<object>(tree.SelectedNodes.Select(node => node.Tag).Where(tag => tag != null));
            tree.SelectedNodes.Clear();
            bool selectionIsDifferent = false;

            update();

            try
            {
                // Update nodes
                _changing = true;

                // Restore selection
                var newSelectedNodes = new List<DarkTreeNode>();
                foreach (DarkTreeNode node in CollectAllNodes(tree.Nodes))
                    if (node.Tag != null)
                        if (selectedNodes.Contains(node.Tag))
                            newSelectedNodes.Add(node);
                tree.SelectNodes(newSelectedNodes);
                selectionIsDifferent = newSelectedNodes.Count != selectedNodes.Count;
            }
            finally
            {
                _changing = false;
            }

            // See if anything changed in the selection
            if (selectionIsDifferent)
                SelectedWadObjectIdsChanged?.Invoke(this, EventArgs.Empty);

            // Workaround update problems
            Invalidate();
        }

        public static IEnumerable<DarkTreeNode> CollectAllNodes(IEnumerable<DarkTreeNode> @this)
        {
            foreach (DarkTreeNode node in @this)
            {
                yield return node;
                if (node.Nodes.Count != 0)
                    foreach (DarkTreeNode child in CollectAllNodes(node.Nodes))
                        yield return child;
            }
        }

        public void SelectFirst()
        {
            DarkTreeNode firstNode = CollectAllNodes(tree.Nodes).FirstOrDefault(node => node.Tag is IWadObjectId);
            if (firstNode != null)
            {
                tree.SelectNode(firstNode);

                // Expand
                while (firstNode != null)
                {
                    firstNode.Expanded = true;
                    firstNode = firstNode.ParentNode;
                }
            }
        }

        private void suggestedGameVersionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            object selection = suggestedGameVersionComboBox.SelectedItem;
            if (selection == null)
                return;
            Wad.SuggestedGameVersion = (WadGameVersion)selection;
            UpdateContent();
        }

        private void tree_DoubleClick(object sender, EventArgs e)
        {
            OnDoubleClick(e);
        }

        private void tree_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        private void Tree_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void Tree_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void Tree_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        private void InitializeComponent()
        {
            this.tree = new DarkUI.Controls.DarkTreeView();
            this.suggestedGameVersionComboBox = new DarkUI.Controls.DarkComboBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.SuspendLayout();
            // 
            // tree
            // 
            this.tree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tree.Enabled = false;
            this.tree.Location = new System.Drawing.Point(0, 30);
            this.tree.MaxDragChange = 20;
            this.tree.MultiSelect = true;
            this.tree.Name = "tree";
            this.tree.Size = new System.Drawing.Size(150, 120);
            this.tree.TabIndex = 1;
            this.tree.Click += new System.EventHandler(this.tree_Click);
            this.tree.DoubleClick += new System.EventHandler(this.tree_DoubleClick);
            this.tree.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Tree_KeyDown);
            this.tree.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Tree_KeyPress);
            this.tree.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Tree_KeyUp);
            this.tree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tree_MouseDown);
            // 
            // suggestedGameVersionComboBox
            // 
            this.suggestedGameVersionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.suggestedGameVersionComboBox.Enabled = false;
            this.suggestedGameVersionComboBox.FormattingEnabled = true;
            this.suggestedGameVersionComboBox.Location = new System.Drawing.Point(103, 3);
            this.suggestedGameVersionComboBox.Name = "suggestedGameVersionComboBox";
            this.suggestedGameVersionComboBox.Size = new System.Drawing.Size(47, 21);
            this.suggestedGameVersionComboBox.TabIndex = 0;
            this.suggestedGameVersionComboBox.SelectedIndexChanged += new System.EventHandler(this.suggestedGameVersionComboBox_SelectedIndexChanged);
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(3, 6);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(91, 13);
            this.darkLabel1.TabIndex = 3;
            this.darkLabel1.Text = "Game slot names:";
            // 
            // WadTreeView
            // 
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.suggestedGameVersionComboBox);
            this.Controls.Add(this.tree);
            this.Name = "WadTreeView";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        [DefaultValue(true)]
        public bool MultiSelect
        {
            get { return tree.MultiSelect; }
            set { tree.MultiSelect = value; }
        }

        private void tree_MouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }
    }
}
