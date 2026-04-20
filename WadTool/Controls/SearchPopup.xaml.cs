using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WadTool.Controls
{
    public partial class SearchPopup : Window
    {
        private readonly TreeView _treeView;
        private readonly List<TreeViewItem> _flatItems = new List<TreeViewItem>();
        private int _currentIndex = -1;

        public SearchPopup(TreeView treeView)
        {
            InitializeComponent();
            _treeView = treeView;

            // Flatten all leaf nodes.
            foreach (TreeViewItem category in _treeView.Items)
            {
                foreach (TreeViewItem child in category.Items)
                    _flatItems.Add(child);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            searchBox.Focus();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            var text = searchBox.Text;
            if (string.IsNullOrEmpty(text))
                return;

            // Search forward from current index.
            for (int i = _currentIndex + 1; i < _flatItems.Count + _currentIndex + 1; i++)
            {
                int index = i % _flatItems.Count;
                var header = _flatItems[index].Header as string ?? "";

                if (header.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _currentIndex = index;
                    SelectItem(_flatItems[index]);
                    return;
                }
            }

            // No match - flash the background.
            searchBox.Background = new SolidColorBrush(Color.FromRgb(0x8B, 0x2A, 0x2A));
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentIndex = -1;
            searchBox.ClearValue(BackgroundProperty);
        }

        private void SelectItem(TreeViewItem item)
        {
            // Expand parent.
            if (item.Parent is TreeViewItem parent)
                parent.IsExpanded = true;

            item.IsSelected = true;
            item.BringIntoView();
            item.Focus();

            searchBox.ClearValue(BackgroundProperty);
            searchBox.Focus();
        }
    }
}
