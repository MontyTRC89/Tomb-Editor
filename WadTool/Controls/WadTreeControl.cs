using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Controls;
using TombLib.Wad;

namespace WadTool.Controls
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


        public void KeepSelection(Action update)
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
            OnDoubleClick(EventArgs.Empty);
        }

        private void tree_Click(object sender, EventArgs e)
        {
            OnClick(EventArgs.Empty);
        }

        private void InitializeComponent()
        {
            tree = new DarkTreeView();
            suggestedGameVersionComboBox = new DarkComboBox();
            darkLabel1 = new DarkLabel();
            SuspendLayout();
            // 
            // tree
            // 
            tree.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
            | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            tree.Enabled = false;
            tree.Location = new System.Drawing.Point(0, 30);
            tree.MaxDragChange = 20;
            tree.MultiSelect = true;
            tree.Name = "tree";
            tree.Size = new System.Drawing.Size(150, 120);
            tree.TabIndex = 1;
            tree.Click += new EventHandler(tree_Click);
            tree.DoubleClick += new EventHandler(tree_DoubleClick);
            // 
            // suggestedGameVersionComboBox
            // 
            suggestedGameVersionComboBox.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            suggestedGameVersionComboBox.Enabled = false;
            suggestedGameVersionComboBox.FormattingEnabled = true;
            suggestedGameVersionComboBox.Location = new System.Drawing.Point(103, 3);
            suggestedGameVersionComboBox.Name = "suggestedGameVersionComboBox";
            suggestedGameVersionComboBox.Size = new System.Drawing.Size(47, 21);
            suggestedGameVersionComboBox.TabIndex = 0;
            suggestedGameVersionComboBox.Text = null;
            suggestedGameVersionComboBox.SelectedIndexChanged += new EventHandler(suggestedGameVersionComboBox_SelectedIndexChanged);
            // 
            // darkLabel1
            // 
            darkLabel1.AutoSize = true;
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            darkLabel1.Location = new System.Drawing.Point(3, 6);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(91, 13);
            darkLabel1.TabIndex = 3;
            darkLabel1.Text = "Game slot names:";
            // 
            // WadTreeView
            // 
            Controls.Add(darkLabel1);
            Controls.Add(suggestedGameVersionComboBox);
            Controls.Add(tree);
            Name = "WadTreeView";
            ResumeLayout(false);
            PerformLayout();

        }
    }
}
