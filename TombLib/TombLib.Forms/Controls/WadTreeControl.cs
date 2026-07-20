using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Controls;
using TombLib.LevelData;
using TombLib.Wad;
using System.Drawing;
using DarkUI.Config;
using TombLib.Forms;
using System.Globalization;

namespace TombLib.Controls
{
    public class WadTreeView : UserControl
    {
        private Wad2 _wad;
        private DarkTreeView tree;
        private DarkComboBox suggestedGameVersionComboBox;
        private DarkLabel darkLabel1;
        private DarkButton butSearch;
        private DarkTextBox tbDate;
        private DarkLabel darkLabel3;
        private DarkTextBox tbNotes;
        private DarkLabel darkLabel2;
        private DarkPanel panelMetadata;
        private DarkPanel panelTree;
        private DarkPanel panelVersion;

        public bool ItemSelected => tree.SelectedNodes.Count > 0 && tree.SelectedNodes[0].Nodes.Count == 0;

        [Category("Behavior")]
        [DefaultValue(true)]
        public bool ReadOnly { get; set; }

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

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            tree.SelectedNodesChanged += (s, e) => { if (!_changing) SelectedWadObjectIdsChanged?.Invoke(this, EventArgs.Empty); };
            tbNotes.TextChanged += (s, e) => { MetadataChanged?.Invoke(this, EventArgs.Empty); };

            // Populate game version
            foreach (var gameVersion in TRVersion.NativeVersions)
                suggestedGameVersionComboBox.Items.Add(gameVersion);
        }

        public IEnumerable<IWadObjectId> SelectedWadObjectIds => tree.SelectedNodes.Select(node => node.Tag).OfType<IWadObjectId>();

        public event EventHandler ClickOnEmpty;
        public event EventHandler SelectedWadObjectIdsChanged;
        public event EventHandler MetadataChanged;

        public void UpdateContent()
        {
            bool wadLoaded = Wad != null;
            panelVersion.Visible = wadLoaded;
            panelMetadata.Visible = wadLoaded;
            panelTree.Visible = wadLoaded;

            tbNotes.ReadOnly = ReadOnly;

            // Update game version control
            if (wadLoaded)
            {
                if (!Wad.GameVersion.Equals(suggestedGameVersionComboBox.SelectedItem))
                    suggestedGameVersionComboBox.SelectedItem = Wad.GameVersion;
            }
            else
                suggestedGameVersionComboBox.SelectedItem = null;

            // Update metadata
            UpdateMetadata();

            // Update tree
            KeepSelection(() =>
            {
                var nodes = tree.Nodes.ToList();
                tree.Nodes.Clear();
                if (Wad == null)
                    return;

                {
                    var mainNode = AddOrReuseChild(nodes, "Moveables");
                    UpdateList(mainNode, _wad.Moveables.Values.Select(o => o.Id), o => o.ToString(Wad.GameVersion));
                }
                {
                    var mainNode = AddOrReuseChild(nodes, "Statics");
                    UpdateList(mainNode, _wad.Statics.Values.Select(o => o.Id), o => o.ToString(Wad.GameVersion));
                }
                {
                    var mainNode = AddOrReuseChild(nodes, "Sprite sequences");
                    UpdateList(mainNode, _wad.SpriteSequences.Values.Select(o => o.Id), o => o.ToString(Wad.GameVersion));
                }

                tree.Nodes.AddRange(nodes);
            });
        }

        public void UpdateMetadata()
        {
            tbDate.Text = Wad?.Timestamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern);
            tbNotes.Text = Wad?.UserNotes;
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
            }
            finally
            {
                _changing = false;
            }

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

        public void Select(List<IWadObjectId> IdList)
        {
            var list = CollectAllNodes(tree.Nodes).ToList();
            List<DarkTreeNode> selectedNodesList = list.Where(node => node.Tag is IWadObjectId && IdList.Any(entry => entry.ToString() == ((IWadObjectId)(node.Tag)).ToString())).ToList();

            if (selectedNodesList.Count > 0)
            {
                tree.SelectedNodes.Clear();

                foreach (var node in selectedNodesList)
                {
                    var currentNode = node;
                    tree.SelectedNodes.Add(currentNode);

                    // Expand
                    while (currentNode != null)
                    {
                        currentNode.Expanded = true;
                        currentNode = currentNode.ParentNode;
                    }
                }

                tree.EnsureVisible();
            }
        }
        public void Select(IWadObjectId Id) => Select(new List<IWadObjectId>() { Id });
        public void SelectFirst() => Select((IWadObjectId)CollectAllNodes(tree.Nodes).FirstOrDefault(node => node.Tag is IWadObjectId).Tag);

        private void suggestedGameVersionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            object selection = suggestedGameVersionComboBox.SelectedItem;
            if (selection == null)
                return;
            Wad.GameVersion = (TRVersion.Game)selection;
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
            tree = new DarkTreeView();
            suggestedGameVersionComboBox = new DarkComboBox();
            darkLabel1 = new DarkLabel();
            butSearch = new DarkButton();
            darkLabel2 = new DarkLabel();
            tbNotes = new DarkTextBox();
            darkLabel3 = new DarkLabel();
            tbDate = new DarkTextBox();
            panelMetadata = new DarkPanel();
            panelTree = new DarkPanel();
            panelVersion = new DarkPanel();
            panelMetadata.SuspendLayout();
            panelTree.SuspendLayout();
            panelVersion.SuspendLayout();
            SuspendLayout();
            // 
            // tree
            // 
            tree.Dock = DockStyle.Fill;
            tree.ExpandOnDoubleClick = false;
            tree.Location = new Point(0, 0);
            tree.MaxDragChange = 20;
            tree.MultiSelect = true;
            tree.Name = "tree";
            tree.Size = new Size(288, 561);
            tree.TabIndex = 1;
            tree.Click += tree_Click;
            tree.DoubleClick += tree_DoubleClick;
            tree.KeyDown += Tree_KeyDown;
            tree.KeyPress += Tree_KeyPress;
            tree.KeyUp += Tree_KeyUp;
            tree.MouseDown += tree_MouseDown;
            // 
            // suggestedGameVersionComboBox
            // 
            suggestedGameVersionComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            suggestedGameVersionComboBox.FormattingEnabled = true;
            suggestedGameVersionComboBox.Location = new Point(81, 3);
            suggestedGameVersionComboBox.Name = "suggestedGameVersionComboBox";
            suggestedGameVersionComboBox.Size = new Size(178, 44);
            suggestedGameVersionComboBox.TabIndex = 0;
            suggestedGameVersionComboBox.SelectedIndexChanged += suggestedGameVersionComboBox_SelectedIndexChanged;
            suggestedGameVersionComboBox.Resize += suggestedGameVersionComboBox_Resize;
            // 
            // darkLabel1
            // 
            darkLabel1.AutoSize = true;
            darkLabel1.ForeColor = Color.FromArgb(220, 220, 220);
            darkLabel1.Location = new Point(0, 6);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new Size(185, 37);
            darkLabel1.TabIndex = 3;
            darkLabel1.Text = "Game version:";
            // 
            // butSearch
            // 
            butSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            butSearch.Checked = false;
            butSearch.Image = Properties.Resources.general_search_16;
            butSearch.Location = new Point(265, 3);
            butSearch.Name = "butSearch";
            butSearch.Size = new Size(21, 21);
            butSearch.TabIndex = 5;
            butSearch.Click += butSearch_Click;
            // 
            // darkLabel2
            // 
            darkLabel2.AutoSize = true;
            darkLabel2.BackColor = Color.Transparent;
            darkLabel2.ForeColor = Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new Point(0, 7);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new Size(72, 35);
            darkLabel2.TabIndex = 0;
            darkLabel2.Text = "Date:";
            // 
            // tbNotes
            // 
            tbNotes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tbNotes.Location = new Point(40, 33);
            tbNotes.Name = "tbNotes";
            tbNotes.Size = new Size(248, 40);
            tbNotes.TabIndex = 1;
            tbNotes.TextChanged += tbNotes_TextChanged;
            // 
            // darkLabel3
            // 
            darkLabel3.AutoSize = true;
            darkLabel3.BackColor = Color.Transparent;
            darkLabel3.ForeColor = Color.FromArgb(220, 220, 220);
            darkLabel3.Location = new Point(0, 35);
            darkLabel3.Name = "darkLabel3";
            darkLabel3.Size = new Size(86, 35);
            darkLabel3.TabIndex = 2;
            darkLabel3.Text = "Notes:";
            // 
            // tbDate
            // 
            tbDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tbDate.Location = new Point(40, 5);
            tbDate.Name = "tbDate";
            tbDate.ReadOnly = true;
            tbDate.Size = new Size(248, 40);
            tbDate.TabIndex = 3;
            // 
            // panelMetadata
            // 
            panelMetadata.Controls.Add(tbDate);
            panelMetadata.Controls.Add(darkLabel3);
            panelMetadata.Controls.Add(darkLabel2);
            panelMetadata.Controls.Add(tbNotes);
            panelMetadata.Dock = DockStyle.Bottom;
            panelMetadata.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            panelMetadata.Location = new Point(0, 591);
            panelMetadata.Name = "panelMetadata";
            panelMetadata.Size = new Size(288, 56);
            panelMetadata.TabIndex = 11;
            // 
            // panelTree
            // 
            panelTree.Controls.Add(tree);
            panelTree.Dock = DockStyle.Fill;
            panelTree.Location = new Point(0, 30);
            panelTree.Name = "panelTree";
            panelTree.Size = new Size(288, 561);
            panelTree.TabIndex = 12;
            // 
            // panelVersion
            // 
            panelVersion.Controls.Add(suggestedGameVersionComboBox);
            panelVersion.Controls.Add(darkLabel1);
            panelVersion.Controls.Add(butSearch);
            panelVersion.Dock = DockStyle.Top;
            panelVersion.Location = new Point(0, 0);
            panelVersion.Margin = new Padding(0);
            panelVersion.Name = "panelVersion";
            panelVersion.Size = new Size(288, 30);
            panelVersion.TabIndex = 13;
            // 
            // WadTreeView
            // 
            Controls.Add(panelTree);
            Controls.Add(panelVersion);
            Controls.Add(panelMetadata);
            Name = "WadTreeView";
            Size = new Size(288, 647);
            Load += WadTreeView_Load;
            Click += WadTreeView_Click;
            panelMetadata.ResumeLayout(false);
            panelMetadata.PerformLayout();
            panelTree.ResumeLayout(false);
            panelVersion.ResumeLayout(false);
            panelVersion.PerformLayout();
            ResumeLayout(false);

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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.IntersectClip(new RectangleF(new PointF(), ClientSize));

            // Draw notify message if no wad is loaded
            if (_wad == null)
            {
                // Draw background
                using (var b = new SolidBrush(BackColor))
                    e.Graphics.FillRectangle(b, ClientRectangle);

                string notifyMessage = "Click here to load new wad file.";

                using (var b = new SolidBrush(Colors.DisabledText))
                    e.Graphics.DrawString(notifyMessage, Font, b, ClientRectangle,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
        }

        private void WadTreeView_Click(object sender, EventArgs e)
        {
            if (_wad == null && ClickOnEmpty != null) ClickOnEmpty(this, e);
        }

        private void butSearch_Click(object sender, EventArgs e)
        {
            var searchPopUp = new PopUpSearch(tree) { ShowAboveControl = true };
            searchPopUp.Show(this);
        }

        private void suggestedGameVersionComboBox_Resize(object sender, EventArgs e)
        {
            butSearch.Size = new Size(suggestedGameVersionComboBox.Size.Height, suggestedGameVersionComboBox.Size.Height);
            butSearch.Location = new Point(ClientSize.Width - butSearch.Size.Width - Padding.Right - Margin.Right, suggestedGameVersionComboBox.Location.Y);
            suggestedGameVersionComboBox.Size = new Size(ClientSize.Width - suggestedGameVersionComboBox.Location.X - butSearch.Size.Width - Padding.Right - Margin.Right - 5, suggestedGameVersionComboBox.Size.Height);
        }

        private void tbNotes_TextChanged(object sender, EventArgs e)
        {
            Wad.UserNotes = tbNotes.Text;
        }

        private void WadTreeView_Load(object sender, EventArgs e)
        {

        }
    }
}
