#nullable enable

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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

        private void Node_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            if (sender is FrameworkElement element && element.DataContext is NodeViewModel node && ViewModel != null)
                SelectNode(node);
        }

        private void SetColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element || element.DataContext is not NodeViewModel node)
                return;

            var current = (node.HeaderBrush as System.Windows.Media.SolidColorBrush)?.Color ?? System.Windows.Media.Colors.Gray;
            var oldColor = System.Drawing.Color.FromArgb(255, current.R, current.G, current.B);

            using var dialog = new TombLib.Controls.RealtimeColorDialog(
                onColorChange: c => node.SetColor(System.Windows.Media.Color.FromRgb(c.R, c.G, c.B)))
            {
                Color = oldColor,
                FullOpen = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                node.SetColor(System.Windows.Media.Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));
            else
                node.SetColor(System.Windows.Media.Color.FromRgb(oldColor.R, oldColor.G, oldColor.B));
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
            DependencyObject? hit = null;
            VisualTreeHelper.HitTest(rootCanvas, null,
                result => { hit = result.VisualHit; return HitTestResultBehavior.Stop; },
                new PointHitTestParameters(position));

            DependencyObject? current = hit;
            while (current != null)
            {
                if (current is FrameworkElement element && element.DataContext is NodeViewModel node)
                    return node;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
