#nullable enable

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>
    /// Pure-WPF visual-scripting node canvas: draggable action/condition nodes, Next/Else links rendered
    /// as lines, create-from-function-list, delete, and port-drag to (re)connect nodes.
    /// </summary>
    public partial class NodeEditorView : UserControl
    {
        private NodeViewModel? _pendingFrom;
        private bool _pendingElse;
        private Line? _tempLine;

        public NodeEditorView()
        {
            InitializeComponent();
        }

        private NodeEditorViewModel? ViewModel => DataContext as NodeEditorViewModel;

        private void OnArgDragOver(object sender, DragEventArgs e)
        {
            bool accepts = sender is FrameworkElement fe && fe.DataContext is ArgumentViewModel arg && arg.AcceptsDrop;
            e.Effects = accepts ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnArgDrop(object sender, DragEventArgs e)
        {
            if (sender is not FrameworkElement fe || fe.DataContext is not ArgumentViewModel arg)
                return;

            var formats = e.Data.GetFormats();
            if (formats.Length > 0)
                arg.HandleDrop(e.Data.GetData(formats[0]));

            e.Handled = true;
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            double factor = e.Delta > 0 ? 1.1 : 1.0 / 1.1;
            double scale = System.Math.Clamp(zoomTransform.ScaleX * factor, 0.3, 2.0);
            zoomTransform.ScaleX = scale;
            zoomTransform.ScaleY = scale;
            e.Handled = true;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel == null)
                return;

            if (e.Key == Key.Delete)
            {
                if (ViewModel.DeleteSelectedNodeCommand.CanExecute(null))
                    ViewModel.DeleteSelectedNodeCommand.Execute(null);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.C:
                        ViewModel.CopySelected(false);
                        e.Handled = true;
                        break;
                    case Key.X:
                        ViewModel.CopySelected(true);
                        e.Handled = true;
                        break;
                    case Key.V:
                        ViewModel.Paste();
                        e.Handled = true;
                        break;
                }
            }
        }

        private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null)
                return;

            var position = e.GetPosition(rootCanvas);

            // Right-click on a node falls through to that node's own context menu.
            if (FindNodeAt(position) != null)
                return;

            var menu = new ContextMenu();
            foreach (var group in System.Linq.Enumerable.GroupBy(ViewModel.Functions, f => f.Section))
            {
                var section = new MenuItem { Header = string.IsNullOrEmpty(group.Key) ? "Misc" : group.Key };
                foreach (var function in group)
                {
                    var func = function;
                    var item = new MenuItem { Header = func.Name };
                    item.Click += (_, _) => ViewModel.AddNodeAtPosition(func, position.X, position.Y);
                    section.Items.Add(item);
                }
                menu.Items.Add(section);
            }

            menu.IsOpen = true;
            e.Handled = true;
        }

        private void Node_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            if (sender is FrameworkElement element && element.DataContext is NodeViewModel node && ViewModel != null)
                SelectNode(node);
        }

        private void SetColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is NodeViewModel node)
                ApplyColor(node);
        }

        private void ApplyColor(NodeViewModel node)
        {
            var current = (node.HeaderBrush as SolidColorBrush)?.Color ?? Colors.Gray;
            var oldColor = System.Drawing.Color.FromArgb(255, current.R, current.G, current.B);

            using var dialog = new TombLib.Controls.RealtimeColorDialog(
                onColorChange: c => node.SetColor(Color.FromRgb(c.R, c.G, c.B)))
            {
                Color = oldColor,
                FullOpen = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                node.SetColor(Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));
            else
                node.SetColor(Color.FromRgb(oldColor.R, oldColor.G, oldColor.B));
        }

        // --- Node-action toolbar buttons (operate on the last selected node) ---

        private void RenameSelected_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel?.SelectedNode is not { } node)
                return;

            var vm = new InputBoxWindowViewModel(title: "Rename node", label: "New name:", placeholder: node.Title);
            ShowInputBox(vm);
            if (vm.DialogResult == true)
                node.Title = vm.Value;
        }

        private void ColorSelected_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel?.SelectedNode is { } node)
                ApplyColor(node);
        }

        private void LockSelected_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel?.SelectedNode is { } node)
                node.IsLocked = !node.IsLocked;
        }

        private void ExportSelected_Click(object sender, RoutedEventArgs e)
            => ViewModel?.CopySelected(false);

        private void ClearNodes_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null || ViewModel.Nodes.Count == 0)
                return;

            if (MessageBox.Show("Remove all nodes from this event?", "Clear nodes",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                ViewModel.ClearAllNodes();
        }

        private void FindNode_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            var vm = new InputBoxWindowViewModel(title: "Find node", label: "Node name:");
            ShowInputBox(vm);
            if (vm.DialogResult != true)
                return;

            var node = ViewModel.FindByName(vm.Value);
            if (node is null)
            {
                MessageBox.Show("No node named '" + vm.Value + "' was found.", "Find node",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SelectNode(node);
            ScrollToNode(node);
        }

        private void ShowInputBox(InputBoxWindowViewModel vm)
        {
            var dialog = new InputBoxWindow
            {
                DataContext = vm,
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            // Manually-constructed (not via IDialogService), so wire the DialogResult auto-close.
            dialog.HookModalAutoClose();
            dialog.ShowDialog();
        }

        private void ScrollToNode(NodeViewModel node)
        {
            double scale = zoomTransform.ScaleX;
            canvasScroll.ScrollToHorizontalOffset(node.CanvasLeft * scale - canvasScroll.ViewportWidth / 2.0);
            canvasScroll.ScrollToVerticalOffset(node.CanvasTop * scale - canvasScroll.ViewportHeight / 2.0);
        }

        private void DeleteNode_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is NodeViewModel node && ViewModel != null)
            {
                SelectNode(node);
                ViewModel.DeleteSelectedNodeCommand.Execute(null);
            }
        }

        private void SelectNode(NodeViewModel node)
        {
            if (ViewModel == null)
                return;

            foreach (var other in ViewModel.Nodes)
                other.IsSelected = other == node;

            ViewModel.SelectedNode = node;
        }

        private void Node_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is NodeViewModel node)
                node.Height = e.NewSize.Height;
        }

        private void NodeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is NodeViewModel node)
            {
                node.CanvasLeft += e.HorizontalChange;
                node.CanvasTop += e.VerticalChange;
            }
        }

        private void OutputPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Ellipse port || port.DataContext is not NodeViewModel from)
                return;

            e.Handled = true;
            _pendingFrom = from;
            _pendingElse = (string?)port.Tag == "Else";

            double startX = _pendingElse ? from.ElseX : from.OutX;
            double startY = _pendingElse ? from.ElseY : from.OutY;

            _tempLine = new Line
            {
                X1 = startX,
                Y1 = startY,
                X2 = startX,
                Y2 = startY,
                Stroke = Brushes.Gray,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 3, 3 },
                IsHitTestVisible = false
            };
            rootCanvas.Children.Add(_tempLine);

            // Capture on the canvas (reliable) rather than the tiny port ellipse.
            rootCanvas.CaptureMouse();
            rootCanvas.MouseMove += Canvas_PendingMove;
            rootCanvas.MouseLeftButtonUp += Canvas_PendingUp;
        }

        private void Canvas_PendingMove(object sender, MouseEventArgs e)
        {
            if (_tempLine == null)
                return;

            Point position = e.GetPosition(rootCanvas);
            _tempLine.X2 = position.X;
            _tempLine.Y2 = position.Y;
        }

        private void Canvas_PendingUp(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(rootCanvas);
            NodeViewModel? target = FindNodeAt(position);

            if (ViewModel != null && _pendingFrom != null)
            {
                if (target != null && target != _pendingFrom)
                    ViewModel.Connect(_pendingFrom, target, _pendingElse);
                else
                    ViewModel.Disconnect(_pendingFrom, _pendingElse);
            }

            EndPendingConnection();
        }

        private void EndPendingConnection()
        {
            if (_tempLine != null)
                rootCanvas.Children.Remove(_tempLine);
            _tempLine = null;
            _pendingFrom = null;

            rootCanvas.ReleaseMouseCapture();
            rootCanvas.MouseMove -= Canvas_PendingMove;
            rootCanvas.MouseLeftButtonUp -= Canvas_PendingUp;
        }

        private NodeViewModel? FindNodeAt(Point position)
        {
            if (ViewModel == null)
                return null;

            // Geometric search in canvas coordinates. Visual-tree hit-testing is unreliable here because
            // the nodes live in a zero-sized items panel. Iterate back-to-front so the topmost node wins.
            for (int i = ViewModel.Nodes.Count - 1; i >= 0; i--)
            {
                var node = ViewModel.Nodes[i];
                if (position.X >= node.CanvasLeft && position.X <= node.CanvasLeft + node.Width &&
                    position.Y >= node.CanvasTop && position.Y <= node.CanvasTop + node.Height)
                    return node;
            }
            return null;
        }
    }
}
