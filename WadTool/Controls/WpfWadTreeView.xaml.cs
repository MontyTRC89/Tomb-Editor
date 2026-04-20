using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DarkUI.WPF.CustomControls;
using TombLib.LevelData;
using TombLib.Wad;

namespace WadTool.Controls
{
    public partial class WpfWadTreeView : UserControl
    {
        private Wad2 _wad;
        private bool _suppressSelectionEvent;

        public bool ReadOnly { get; set; }

        public Wad2 Wad
        {
            get => _wad;
            set
            {
                _wad = value;
                UpdateContent();
            }
        }

        public bool ItemSelected
        {
            get
            {
                if (treeView.SelectedItem is TreeViewItem item)
                    return item.Tag is IWadObjectId;
                return false;
            }
        }

        public IEnumerable<IWadObjectId> SelectedWadObjectIds
        {
            get
            {
                if (treeView.SelectedItem is TreeViewItem item && item.Tag is IWadObjectId id)
                    return new[] { id };
                return Enumerable.Empty<IWadObjectId>();
            }
        }

        public event EventHandler ClickOnEmpty;
        public event EventHandler SelectedWadObjectIdsChanged;
        public event EventHandler MetadataChanged;
        public event EventHandler ItemDoubleClicked;
        public event KeyEventHandler ItemKeyDown;

        public WpfWadTreeView()
        {
            InitializeComponent();

            foreach (var gameVersion in TRVersion.NativeVersions)
                gameVersionComboBox.Items.Add(gameVersion);
        }

        public void UpdateContent()
        {
            bool wadLoaded = _wad != null;
            mainContent.Visibility = wadLoaded ? Visibility.Visible : Visibility.Collapsed;
            placeholderText.Visibility = wadLoaded ? Visibility.Collapsed : Visibility.Visible;

            tbNotes.IsReadOnly = ReadOnly;

            if (wadLoaded)
            {
                if (!_wad.GameVersion.Equals(gameVersionComboBox.SelectedItem))
                    gameVersionComboBox.SelectedItem = _wad.GameVersion;
            }
            else
                gameVersionComboBox.SelectedItem = null;

            UpdateMetadata();
            RebuildTree();
        }

        public void UpdateMetadata()
        {
            tbDate.Text = _wad?.Timestamp.ToString(
                CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern);
            tbNotes.Text = _wad?.UserNotes;
        }

        public void Select(IWadObjectId id)
            => Select(new List<IWadObjectId> { id });

        public void Select(List<IWadObjectId> idList)
        {
            if (idList == null || idList.Count == 0)
                return;

            var idStrings = new HashSet<string>(idList.Select(id => id.ToString()));

            foreach (TreeViewItem category in treeView.Items)
            {
                foreach (TreeViewItem child in category.Items)
                {
                    if (child.Tag is IWadObjectId childId && idStrings.Contains(childId.ToString()))
                    {
                        category.IsExpanded = true;
                        child.IsSelected = true;
                        child.BringIntoView();
                        return;
                    }
                }
            }
        }

        public void SelectFirst()
        {
            foreach (TreeViewItem category in treeView.Items)
            {
                if (category.Items.Count > 0)
                {
                    category.IsExpanded = true;
                    var first = (TreeViewItem)category.Items[0];
                    first.IsSelected = true;
                    first.BringIntoView();
                    return;
                }
            }
        }

        private void RebuildTree()
        {
            // Preserve current selection.
            var selectedIds = new HashSet<string>(
                SelectedWadObjectIds.Select(id => id.ToString()));

            _suppressSelectionEvent = true;

            try
            {
                treeView.Items.Clear();

                if (_wad == null)
                    return;

                AddCategory("Moveables",
                    _wad.Moveables.Values.Select(o => o.Id),
                    o => o.ToString(_wad.GameVersion));

                AddCategory("Statics",
                    _wad.Statics.Values.Select(o => o.Id),
                    o => o.ToString(_wad.GameVersion));

                AddCategory("Sprite sequences",
                    _wad.SpriteSequences.Values.Select(o => o.Id),
                    o => o.ToString(_wad.GameVersion));

                // Restore selection.
                RestoreSelection(selectedIds);
            }
            finally
            {
                _suppressSelectionEvent = false;
            }

            SelectedWadObjectIdsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void AddCategory<T>(string header, IEnumerable<T> items, Func<T, string> format)
            where T : IWadObjectId
        {
            var category = new AlternatingTreeViewItem
            {
                Header = header,
                IsExpanded = IsCategoryExpanded(header)
            };

            foreach (var item in items)
            {
                var child = new AlternatingTreeViewItem
                {
                    Header = format(item),
                    Tag = item
                };
                category.Items.Add(child);
            }

            treeView.Items.Add(category);
        }

        private bool IsCategoryExpanded(string categoryName)
        {
            foreach (TreeViewItem existing in treeView.Items)
            {
                if (existing.Header as string == categoryName)
                    return existing.IsExpanded;
            }
            return false;
        }

        private void RestoreSelection(HashSet<string> selectedIds)
        {
            if (selectedIds.Count == 0)
                return;

            foreach (TreeViewItem category in treeView.Items)
            {
                foreach (TreeViewItem child in category.Items)
                {
                    if (child.Tag is IWadObjectId id && selectedIds.Contains(id.ToString()))
                    {
                        category.IsExpanded = true;
                        child.IsSelected = true;
                        child.BringIntoView();
                        return;
                    }
                }
            }
        }

        #region Event Handlers

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!_suppressSelectionEvent)
                SelectedWadObjectIdsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ItemDoubleClicked?.Invoke(this, EventArgs.Empty);
        }

        private void TreeView_KeyDown(object sender, KeyEventArgs e)
        {
            ItemKeyDown?.Invoke(this, e);
        }

        private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Detect click on empty area (no TreeViewItem hit).
            if (e.OriginalSource is FrameworkElement fe)
            {
                var item = fe.FindVisualParent<TreeViewItem>();
                if (item == null && _wad == null)
                    ClickOnEmpty?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Placeholder_Click(object sender, MouseButtonEventArgs e)
        {
            ClickOnEmpty?.Invoke(this, EventArgs.Empty);
        }

        private void GameVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gameVersionComboBox.SelectedItem == null || _wad == null)
                return;

            _wad.GameVersion = (TRVersion.Game)gameVersionComboBox.SelectedItem;
            UpdateContent();
        }

        private void Notes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_wad != null)
                _wad.UserNotes = tbNotes.Text;
            MetadataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Search functionality - opens a search popup to filter the tree.
            var searchWindow = new SearchPopup(treeView);
            searchWindow.Owner = Window.GetWindow(this);
            searchWindow.ShowDialog();
        }

        #endregion
    }

    // Helper extension for finding visual parents.
    internal static class VisualTreeHelperExtensions
    {
        public static T FindVisualParent<T>(this DependencyObject child) where T : DependencyObject
        {
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T found)
                    return found;
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}
