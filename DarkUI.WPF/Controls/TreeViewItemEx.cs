using AvalonDock.Controls;
using System.Windows;
using System.Windows.Controls;

namespace DarkUI.WPF.Controls
{
	public class TreeViewItemEx : TreeViewItem
	{
		public TreeViewItemEx()
		{
			Loaded += TreeViewItemEx_Changed;
			Expanded += TreeViewItemEx_Changed;
			Collapsed += TreeViewItemEx_Changed;
		}

		private void TreeViewItemEx_Changed(object sender, RoutedEventArgs e)
		{
			TreeView? treeViewAncestor = this.FindVisualAncestor<TreeView>();
			treeViewAncestor?.SetAlternationIndexRecursively(0);
		}
	}
}
