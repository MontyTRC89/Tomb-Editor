using DarkUI.Config;
using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombLib.Controls.VisualScripting
{
    public enum ConnectionMode
    {
        Previous,
        Next,
        Else
    }

    public partial class NodeEditor : UserControl
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector2 ViewPosition { get; set; } = new Vector2(60.0f, 60.0f);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<TriggerNode> Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                UpdateVisibleNodes();
            }
        }
        private List<TriggerNode> _nodes;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<TriggerNode> SelectedNodes { get; private set; } = new List<TriggerNode>();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TriggerNode HotNode { get; set; } = null;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ConnectionMode HotNodeMode { get; set; } = ConnectionMode.Previous;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Resizing { get; set; } = false;

        public Color SelectionColor { get; set; } = Colors.BlueSelection;
        public float GridStep { get; set; } = 8.0f;
        public int GridSize { get; set; } = 256;

        private const float _hotNodeTransparency = 0.6f;
        private const float _connectedNodeTransparency = 0.8f;
        private const int _selectionThickness = 2;

        private const string _thenString = "pass";
        private const string _elseString = "fail";

        private static readonly Pen _gridPen = new Pen((Colors.DarkBackground.ToFloat3Color() * 1.15f).ToWinFormsColor(), 1);

        private Point _lastMousePosition;
        private Point _newMousePosition;
        private Vector2? _viewMoveMouseWorldCoord;

        private float _animProgress = 1.0f;
        private PointF[] _animSnapCoords = null;
        private readonly Timer _updateTimer;
        private bool _queueMove = false;

        public NodeEditor()
        {
            SetStyle(ControlStyles.UserPaint | 
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer, true);

            InitializeComponent();

            _updateTimer = new Timer { Interval = 10 };
            _updateTimer.Tick += UpdateTimer_Tick;
        }

        protected override void Dispose(bool disposing)
        {
            _updateTimer.Stop();
            _updateTimer.Tick -= UpdateTimer_Tick;

            Controls.Clear();

            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        public void Initialize(List<TriggerNode> nodes = null)
        {
            ViewPosition = new Vector2(GridSize / 2.0f,
                                       GridSize / 2.0f);
            Nodes = nodes == null ? new List<TriggerNode>() : nodes;
            UpdateVisibleNodes();
            _updateTimer.Start();
        }

        public void AddConditionNode()
        {
            var node = new TriggerNodeCondition()
            {
                Name = "Condition node " + Nodes.Count,
                ScreenPosition = ViewPosition,
                Color = Colors.GreyBackground.ToFloat3Color() * new Vector3(0.8f, 1.0f, 0.8f),
                Getter = "Test 2"
            };

            Nodes.Add(node);
            UpdateVisibleNodes();
            SelectNode(node, true);
        }

        public void AddActionNode()
        {
            var node = new TriggerNodeAction()
            {
                Name = "Action node " + Nodes.Count,
                ScreenPosition = ViewPosition,
                Color = Colors.GreyBackground.ToFloat3Color() * new Vector3(0.8f, 0.8f, 1.0f),
                Function = "Test 1"
            };

            Nodes.Add(node);
            UpdateVisibleNodes();
            SelectNode(node, true);
        }

        public void ClearNodes()
        {
            Nodes.Clear();
            UpdateVisibleNodes();
        }

        public void SelectNode(TriggerNode node, bool reset)
        {
            if (!LinearizedNodes().Contains(node))
                return;

            if (reset)
                SelectedNodes.Clear();

            if (!SelectedNodes.Contains(node))
                SelectedNodes.Add(node);
            else
                SelectedNodes.Remove(node);

            Refresh();
        }

        public void DeleteNodes()
        {
            if (SelectedNodes.Count == 0)
                return;

            foreach (var node in SelectedNodes)
            {
                DisconnectPreviousNode(node);
                DisconnectNextNode(node);
                DisconnectElseNode(node);
                Nodes.Remove(node);
            }

            UpdateVisibleNodes();
        }

        public Vector2 FromVisualCoord(PointF pos)
        {
            return new Vector2((pos.X - Width * 0.5f) / GridStep + ViewPosition.X, 
                               (Height * 0.5f - pos.Y) / GridStep + ViewPosition.Y);
        }

        public Rectangle2 FromVisualCoord(RectangleF area)
        {
            Vector2 visibleAreaStart = FromVisualCoord(new PointF(area.Left, area.Top));
            Vector2 visibleAreaEnd = FromVisualCoord(new PointF(area.Right, area.Bottom));
            return new Rectangle2(Vector2.Min(visibleAreaStart, visibleAreaEnd), Vector2.Max(visibleAreaStart, visibleAreaEnd));
        }

        public Point ToVisualCoord(Vector2 pos)
        {
            return new Point((int)Math.Round((pos.X - ViewPosition.X) * GridStep + Width * 0.5f), 
                             (int)Math.Round(Height * 0.5f - (pos.Y - ViewPosition.Y) * GridStep));
        }

        public RectangleF ToVisualCoord(Rectangle2 area)
        {
            PointF start = ToVisualCoord(area.Start);
            PointF end = ToVisualCoord(area.End);
            return RectangleF.FromLTRB(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Max(start.X, end.X), Math.Max(start.Y, end.Y));
        }

        private void LimitPosition()
        {
            ViewPosition = Vector2.Clamp(ViewPosition, new Vector2(), new Vector2(GridSize));
        }

        private void MoveToFixedPoint(PointF visualPoint, Vector2 worldPoint, bool limitPosition = false)
        {
            // Adjust ViewPosition in such a way, that the FixedPoint does not move visually
            ViewPosition = -worldPoint;
            ViewPosition = -FromVisualCoord(visualPoint);
            if (limitPosition)
                LimitPosition();

            foreach (var control in Controls.OfType<VisibleNodeBase>())
                control.RefreshPosition();

            Refresh();
        }

        public void UpdateVisibleNodes()
        {
            var visibleNodes = Controls.OfType<VisibleNodeBase>().ToList();
            var linearizedNodes = LinearizedNodes();

            // Remove orphaned controls
            for (int i = visibleNodes.Count - 1; i >= 0; i--)
            {
                var control = visibleNodes[i];
                if (!linearizedNodes.Contains(control.Node))
                    Controls.Remove(control);
            }

            var newControls = new List<VisibleNodeBase>();

            // Recreate new controls
            foreach (var node in Nodes)
                AddNodeControl(node, newControls);

            // Add all controls at once, to avoid flickering
            foreach (var control in newControls)
            {
                Controls.Add(control);
                control.RefreshPosition();
            }

            Refresh();
        }

        public void Clear()
        {
            _nodes = new List<TriggerNode>();
        }

        public bool AnimateSnap(ConnectionMode mode, VisibleNodeBase node)
        {
            if (_animProgress != -1.0f)
                return false;

            _animSnapCoords = node.GetNodeScreenPosition(mode, true);
            _animProgress = 0.0f;
            return true;
        }

        public void AnimateUnsnap()
        {
            _animSnapCoords = new PointF[2] { _lastMousePosition, _lastMousePosition };
            _animProgress = -1.0f;
        }

        private void AddNodeControl(TriggerNode node, List<VisibleNodeBase> collectedControls)
        {
            if (!Controls.OfType<VisibleNodeBase>().Any(c => c.Node == node))
            {
                VisibleNodeBase control = null;

                if (node is TriggerNodeAction)
                    control = new VisibleNodeAction(node);
                else if (node is TriggerNodeCondition)
                    control = new VisibleNodeCondition(node);

                collectedControls.Add(control);
                control.BackColor = node.Color.ToWinFormsColor();
                control.Visible = true;
                control.SnapToBorders = false;
                control.DragAnyPoint = true;
            }

            if (node.Next != null)
                AddNodeControl(node.Next, collectedControls);

            if (node is TriggerNodeCondition)
                if ((node as TriggerNodeCondition).Else != null)
                    AddNodeControl((node as TriggerNodeCondition).Else, collectedControls);
        }

        private List<TriggerNode> LinearizedNodes()
        {
            var result = new List<TriggerNode>();

            foreach (var node in Nodes)
                AddNodeToLinearizedList(node, result);

            return result;
        }

        private void AddNodeToLinearizedList(TriggerNode node, List<TriggerNode> list)
        {
            if (!list.Contains(node))
                list.Add(node);

            if (node.Next != null)
                AddNodeToLinearizedList(node.Next, list);

            if (node is TriggerNodeCondition && (node as TriggerNodeCondition).Else != null)
                AddNodeToLinearizedList((node as TriggerNodeCondition).Else, list);
        }

        public void DisconnectPreviousNode(TriggerNode node)
        {
            foreach (var n in LinearizedNodes())
            {
                if (n.Previous != node || n.Previous.Next != n)
                    continue;
                
                n.Previous = n.Previous.Next = null;

                if (!Nodes.Contains(n))
                    Nodes.Add(n);

                return;
            }
        }

        public void DisconnectElseNode(TriggerNode node)
        {
            foreach (var n in LinearizedNodes())
            {
                if (n.Previous != node)
                    continue;

                if (!(n.Previous is TriggerNodeCondition))
                    continue;

                var prevNode = (n.Previous as TriggerNodeCondition);
                if (prevNode.Else != n)
                    continue;

                prevNode.Else = null;
                n.Previous = null;

                if (!Nodes.Contains(n))
                    Nodes.Add(n);

                return;
            }
        }

        public void DisconnectNextNode(TriggerNode node)
        {
            foreach (var n in LinearizedNodes())
            {
                if (n.Next == node)
                    n.Next = n.Next.Previous = null;

                if (n is TriggerNodeCondition && (n as TriggerNodeCondition).Else == node)
                    (n as TriggerNodeCondition).Else = (n as TriggerNodeCondition).Else.Previous = null;
            }

            if (!Nodes.Contains(node))
                Nodes.Add(node);
        }

        public void ResetHotNode()
        {
            if (HotNode == null)
                return;

            if (!Nodes.Contains(HotNode) && HotNode.Previous == null)
                Nodes.Add(HotNode);
            HotNode = null;
            Refresh();
        }

        public Rectangle GetNodeRect(TriggerNode node)
        {
            foreach (var control in Controls)
            {
                if (!(control is VisibleNodeBase))
                    continue;

                var visibleNode = control as VisibleNodeBase;

                if (visibleNode.Node != node)
                    continue;

                var rect = visibleNode.ClientRectangle;
                rect.Offset(visibleNode.Location);
                return rect;
            }

            return new Rectangle();
        }

        public bool TestRecursivity(TriggerNode incomingNode, TriggerNode targetNode, bool backwards)
        {
            bool result = false;

            if (backwards)
            {
                if (targetNode.Previous != null)
                    result = TestRecursivity(incomingNode, targetNode.Previous, backwards);
            }
            else
            {
                if (targetNode.Next != null)
                    result = TestRecursivity(incomingNode, targetNode.Next, backwards);

                if (!result && targetNode is TriggerNodeCondition)
                {
                    var elseNode = (targetNode as TriggerNodeCondition).Else;
                    if (elseNode != null)
                        result = TestRecursivity(incomingNode, elseNode, backwards);
                }
            }

            if (!result)
                return incomingNode == targetNode;
            else
                return result;
        }

        private void DrawShadow(PaintEventArgs e, VisibleNodeBase node)
        {
            var rect = node.ClientRectangle;
            rect.Offset(node.Location);
            rect.Inflate(16, 16);
            e.Graphics.DrawImage(Properties.Resources.misc_Shadow, rect);
        }

        private void DrawHeader(PaintEventArgs e, VisibleNodeBase node)
        {
            var size = TextRenderer.MeasureText(node.Node.Name, Font);

            var rect = node.ClientRectangle;
            rect.Height = size.Height;
            rect.Offset(node.Location);
            rect.Offset(0, -(int)(size.Height * 1.2f));

            using (var b = new SolidBrush(Colors.LightText.ToFloat3Color().ToWinFormsColor(0.7f)))
                e.Graphics.DrawString(node.Node.Name, Font, b, rect,
                        new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });

            var condNode = node as VisibleNodeCondition;
            if (condNode == null)
                return;

            using (var b = new SolidBrush(Colors.LightText.ToFloat3Color().ToWinFormsColor(0.3f)))
            {
                size = TextRenderer.MeasureText(_thenString, Font);
                var nextPoint = condNode.GetNodeScreenPosition(ConnectionMode.Next);
                rect = new Rectangle((int)nextPoint[0].X, condNode.Location.Y + condNode.Height + (int)(size.Height * 0.4f),
                                     (int)(nextPoint[1].X - nextPoint[0].X), size.Height);

                e.Graphics.DrawString(_thenString, Font, b, rect,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                size = TextRenderer.MeasureText(_elseString, Font);
                nextPoint = condNode.GetNodeScreenPosition(ConnectionMode.Else);
                rect = new Rectangle((int)nextPoint[0].X, condNode.Location.Y + condNode.Height + (int)(size.Height * 0.4f),
                                     (int)(nextPoint[1].X - nextPoint[0].X), size.Height);

                e.Graphics.DrawString(_elseString, Font, b, rect,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
        }

        private void DrawVisibleNodeLink(PaintEventArgs e, List<VisibleNodeBase> nodes, VisibleNodeBase node)
        {
            for (int i = 0; i < 2; i++)
            {
                TriggerNode second;

                if (i == 0)
                    second = node.Node.Next;
                else if (i == 1 && node.Node is TriggerNodeCondition && (node.Node as TriggerNodeCondition).Else != null)
                    second = (node.Node as TriggerNodeCondition).Else;
                else
                    continue;

                if (second == null)
                    continue;

                var nextMode = i == 0 ? ConnectionMode.Next : ConnectionMode.Else;
                var nextVisibleNode = nodes.FirstOrDefault(n => n.Node == second);

                if (nextVisibleNode == null)
                    continue;

                // Reverse R/G color bias for else node
                var color = node.Node.Color;
                if (nextMode == ConnectionMode.Else)
                    color = new Vector3(color.Y, color.X, color.Z);

                var p1 = node.GetNodeScreenPosition(nextMode);
                var p2 = nextVisibleNode.GetNodeScreenPosition(ConnectionMode.Previous);
                DrawLink(e, color, _connectedNodeTransparency, p1, p2);
            }
        }

        private void DrawLink(PaintEventArgs e, Vector3 color, float alpha, PointF[] p1, PointF[] p2)
        {
            using (var b = new SolidBrush(color.ToWinFormsColor(alpha)))
                e.Graphics.FillPolygon(b, new PointF[4] { p1[0], p1[1], p2[1], p2[0] });
        }

        private void DrawHotNode(PaintEventArgs e, List<VisibleNodeBase> nodeList)
        {
            if (HotNode == null)
                return;

            var visibleNode = nodeList.FirstOrDefault(n => n.Node == HotNode);
            if (visibleNode == null)
                return;

            var start = visibleNode.GetNodeScreenPosition(HotNodeMode, true);

            var p = new PointF[2];

            if (_animProgress >= 0.0f)
            {
                p[0].X = (float)MathC.SmoothStep(_lastMousePosition.X, _animSnapCoords[0].X, _animProgress);
                p[0].Y = (float)MathC.SmoothStep(_lastMousePosition.Y, _animSnapCoords[0].Y, _animProgress);
                p[1].X = (float)MathC.SmoothStep(_lastMousePosition.X, _animSnapCoords[1].X, _animProgress);
                p[1].Y = (float)MathC.SmoothStep(_lastMousePosition.Y, _animSnapCoords[1].Y, _animProgress);
            }
            else
                p = new PointF[2] { _lastMousePosition, _lastMousePosition };

            var alpha = (float)MathC.Lerp(_hotNodeTransparency, _connectedNodeTransparency, _animProgress >= 0.0f ? _animProgress : 0.0f);
            
            // Reverse R/G bias for else node
            var color = HotNode.Color;
            if (HotNodeMode == ConnectionMode.Else)
                color = new Vector3(color.Y, color.X, color.Z);

            DrawLink(e, color, alpha, start, p);
        }

        private void DrawSelection(PaintEventArgs e)
        {
            if (SelectedNodes.Count == 0)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.None;

            using (var b = new SolidBrush(SelectionColor))
            {
                foreach (var node in SelectedNodes)
                {
                    var rect = GetNodeRect(node);
                    rect.Inflate(_selectionThickness, _selectionThickness);
                    e.Graphics.FillRectangle(b, rect);
                }
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            _lastMousePosition = PointToClient(Cursor.Position);

            if (HotNode != null)
            {
                if (Control.MouseButtons == MouseButtons.None)
                    if (_lastMousePosition.X < 0 || _lastMousePosition.X > Width ||
                        _lastMousePosition.Y < 0 || _lastMousePosition.Y > Height)
                        ResetHotNode();

                Refresh();
            }

            if (_animProgress >= 0.0f && _animProgress < 1.0f)
            {
                _animProgress += 0.1f;
                if (_animProgress > 1.0f)
                    _animProgress = 1.0f;
            }

            if (_queueMove)
            {
                MoveToFixedPoint(_newMousePosition, _viewMoveMouseWorldCoord.Value, true);
                Refresh();
                _queueMove = false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw background
            using (var b = new SolidBrush(Colors.DarkBackground))
                e.Graphics.FillRectangle(b, ClientRectangle);

            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                // Draw grid lines
                Vector2 GridLines0 = FromVisualCoord(new PointF());
                Vector2 GridLines1 = FromVisualCoord(new PointF() + Size);
                Vector2 GridLinesStart = Vector2.Min(GridLines0, GridLines1);
                Vector2 GridLinesEnd = Vector2.Max(GridLines0, GridLines1);
                GridLinesStart = Vector2.Clamp(GridLinesStart, new Vector2(0.0f), new Vector2(GridSize));
                GridLinesEnd = Vector2.Clamp(GridLinesEnd, new Vector2(0.0f), new Vector2(GridSize));
                Point GridLinesStartInt = new Point((int)Math.Floor(GridLinesStart.X), (int)Math.Floor(GridLinesStart.Y));
                Point GridLinesEndInt = new Point((int)Math.Ceiling(GridLinesEnd.X), (int)Math.Ceiling(GridLinesEnd.Y));

                for (int x = GridLinesStartInt.X; x <= GridLinesEndInt.X; ++x)
                    e.Graphics.DrawLine(_gridPen,
                        ToVisualCoord(new Vector2(x, 0)), ToVisualCoord(new Vector2(x, GridSize)));

                for (int y = GridLinesStartInt.Y; y <= GridLinesEndInt.Y; ++y)
                    e.Graphics.DrawLine(_gridPen,
                        ToVisualCoord(new Vector2(0, y)), ToVisualCoord(new Vector2(GridSize, y)));

                var nodeList = Controls.OfType<VisibleNodeBase>().ToList();

                // Draw connected nodes
                foreach (var n in nodeList)
                    DrawVisibleNodeLink(e, nodeList, n);

                // Draw hot node
                DrawHotNode(e, nodeList);

                // Draw shadows
                foreach (var n in nodeList)
                    DrawShadow(e, n);

                // Draw selected node highlights
                DrawSelection(e);

                // Draw labels (after everything else)
                foreach (var n in nodeList)
                    DrawHeader(e, n);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            foreach (var control in Controls.OfType<VisibleNodeBase>())
                control.RefreshPosition();
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            if (e.Data.GetDataPresent(typeof(DarkFloatingToolboxContainer)))
                e.Effect = DragDropEffects.Move;
            else
            {
                var obj = e.Data.GetData(e.Data.GetFormats()[0]) as TriggerNode;
                if (obj == null)
                    return;

                e.Effect = DragDropEffects.Copy;
                AnimateUnsnap();

                switch (HotNodeMode)
                {
                    case ConnectionMode.Previous:
                        DisconnectNextNode(obj);
                        break;

                    case ConnectionMode.Next:
                        DisconnectPreviousNode(obj);
                        break;

                    case ConnectionMode.Else:
                        DisconnectElseNode(obj);
                        break;

                    default:
                        break;
                }

                HotNode = obj;
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);

            var obj = e.Data.GetData(e.Data.GetFormats()[0]) as TriggerNode;
            if (obj == null || HotNode != obj)
                return;

            ResetHotNode();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _animSnapCoords = new PointF[2] { _lastMousePosition, _lastMousePosition };

            if (e.Button == MouseButtons.Right & _viewMoveMouseWorldCoord != null)
            {
                _newMousePosition = e.Location;
                _queueMove = true;
                Resizing = true;
            }
            else
                Resizing = false;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Refresh();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (!Focused && Form.ActiveForm == FindForm())
                Focus();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Right)
            {
                _lastMousePosition = e.Location;
                Capture = true; // Capture mouse for zoom and panning
                _viewMoveMouseWorldCoord = FromVisualCoord(e.Location);
            }
            else if (e.Button == MouseButtons.Left)
                SelectedNodes.Clear();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Prevent progress bar flickering
                return cp;
            }
        }
    }
}