#nullable enable

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
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
            DataContextChanged += OnDataContextChanged;
        }

        private NodeEditorViewModel? ViewModel => DataContext as NodeEditorViewModel;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Scrolling a node into view is the only visual concern the view-model needs from us.
            if (e.OldValue is NodeEditorViewModel oldViewModel)
                oldViewModel.BringNodeIntoViewRequested -= ScrollToNode;
            if (e.NewValue is NodeEditorViewModel newViewModel)
            {
                newViewModel.BringNodeIntoViewRequested += ScrollToNode;

                // Start the view over the event's existing nodes instead of the canvas's top-left
                // corner. Deferred so the ScrollViewer has a measured viewport (a new editor is
                // created for every event switch, before layout has run).
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, CenterOnNodes);
            }
        }

        /// <summary>Scrolls so the bounding box of all nodes is centered in the viewport.</summary>
        private void CenterOnNodes()
        {
            var viewModel = ViewModel;
            if (viewModel == null || viewModel.Nodes.Count == 0)
                return;

            if (canvasScroll.ViewportWidth <= 0.0 || canvasScroll.ViewportHeight <= 0.0)
            {
                canvasScroll.UpdateLayout();
                if (canvasScroll.ViewportWidth <= 0.0)
                    return;
            }

            double minX = double.PositiveInfinity, minY = double.PositiveInfinity;
            double maxX = double.NegativeInfinity, maxY = double.NegativeInfinity;

            foreach (var node in viewModel.Nodes)
            {
                minX = System.Math.Min(minX, node.CanvasLeft);
                minY = System.Math.Min(minY, node.CanvasTop);
                maxX = System.Math.Max(maxX, node.CanvasLeft + node.Width);
                maxY = System.Math.Max(maxY, node.CanvasTop + node.Height);
            }

            double scale = zoomTransform.ScaleX;
            canvasScroll.ScrollToHorizontalOffset(((minX + maxX) / 2.0 * scale) - (canvasScroll.ViewportWidth / 2.0));
            canvasScroll.ScrollToVerticalOffset(((minY + maxY) / 2.0 * scale) - (canvasScroll.ViewportHeight / 2.0));
        }

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

            e.Handled = true;

            double oldScale = zoomTransform.ScaleX;
            double factor = e.Delta > 0 ? 1.1 : 1.0 / 1.1;
            double newScale = System.Math.Clamp(oldScale * factor, 0.3, 2.0);
            if (newScale == oldScale)
                return;

            // Anchor the zoom on the cursor: keep the canvas point under the mouse stationary
            // by compensating the scroll offsets for the scale change.
            Point viewportPos = e.GetPosition(canvasScroll);
            double anchorX = (canvasScroll.HorizontalOffset + viewportPos.X) / oldScale;
            double anchorY = (canvasScroll.VerticalOffset + viewportPos.Y) / oldScale;

            zoomTransform.ScaleX = newScale;
            zoomTransform.ScaleY = newScale;

            // The new extent must be measured before the offsets can reach it.
            canvasScroll.UpdateLayout();
            canvasScroll.ScrollToHorizontalOffset((anchorX * newScale) - viewportPos.X);
            canvasScroll.ScrollToVerticalOffset((anchorY * newScale) - viewportPos.Y);
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

            // The menu is composed here because it depends on the click position; every item
            // delegates the actual work to the view-model's AddNodeAtPositionCommand.
            var menu = new ContextMenu();
            foreach (var group in System.Linq.Enumerable.GroupBy(ViewModel.Functions, f => f.Section))
            {
                // "__" so underscores in script-defined names display literally (not access keys).
                var section = new MenuItem { Header = string.IsNullOrEmpty(group.Key) ? Localizer.Instance["TombEditor.NodeEditor.Misc"] : group.Key.Replace("_", "__") };
                foreach (var function in group)
                {
                    section.Items.Add(new MenuItem
                    {
                        Header = function.Name?.Replace("_", "__"),
                        Command = ViewModel.AddNodeAtPositionCommand,
                        CommandParameter = new NodeEditorViewModel.AddNodeRequest(function, position.X, position.Y)
                    });
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
                ViewModel.SelectedNode = node;
        }

        private void ScrollToNode(NodeViewModel node)
        {
            double scale = zoomTransform.ScaleX;
            canvasScroll.ScrollToHorizontalOffset(node.CanvasLeft * scale - canvasScroll.ViewportWidth / 2.0);
            canvasScroll.ScrollToVerticalOffset(node.CanvasTop * scale - canvasScroll.ViewportHeight / 2.0);
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
