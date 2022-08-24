using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Controls;
using DarkUI.Icons;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombLib.Controls.VisualScripting
{
    public partial class VisibleNodeBase : DarkFloatingToolbox
    {
        protected const int _gripWidth = 200;
        protected const int _gripHeight = 6;

        protected const int _visibilityMargin = 32;

        protected List<Rectangle> _grips = new List<Rectangle>();
        protected int _currentGrip = -1;
        protected int _lastSnappedGrip = -1;

        private bool _mouseDown = false;

        public NodeEditor Editor => Parent as NodeEditor;
        public TriggerNode Node { get; protected set; }

        // This constructor overload is needed so that VS designer don't get crazy.
        public VisibleNodeBase() : this(new TriggerNodeAction()) { }

        public VisibleNodeBase(TriggerNode node)
        {
            InitializeComponent();
            SnapToBorders = false;
            Node = node;
        }

        public void RefreshPosition()
        {
            if (IsInView())
            {
                if (!Visible) Visible = true;
                Location = Editor.ToVisualCoord(Node.ScreenPosition);
            }
            else
                Visible = false;
        }

        public void StorePosition()
        {
            Node.ScreenPosition = Editor.FromVisualCoord(Location);
        }

        public PointF[] GetNodeScreenPosition(ConnectionMode mode, bool force = false)
        {
            var result = new PointF[2] { new PointF(-1, -1), new PointF(-1, -1) };

            if (Node == null)
                return result;

            if (_grips.Count < (int)mode + 1)
                return result;

            var grip = _grips[(int)mode];

            var location = Editor.ToVisualCoord(Node.ScreenPosition);
            var x = location.X + grip.Left;
            var y = location.Y + grip.Top + grip.Height / 2;

            return new PointF[2]
            {
                new PointF(x, y),
                new PointF(x + grip.Width, y)
            };
        }

        private bool ValidConnection(ConnectionMode mode, TriggerNode node)
        {
            if (node == null || node == Node)
                return false;

            if (mode == Editor.HotNodeMode)
                return false;

            // HACK: Later, maybe implement connection mode class (next/previous)
            if ((mode == ConnectionMode.Else && Editor.HotNodeMode == ConnectionMode.Next) ||
                (Editor.HotNodeMode == ConnectionMode.Else && mode == ConnectionMode.Next))
                return false;

            if (Editor.TestRecursivity(node, Node, mode == ConnectionMode.Next))
                return false;

            switch (mode)
            {
                case ConnectionMode.Next:
                    if (Node.Next != null)
                        return false;
                    break;

                case ConnectionMode.Previous:
                case ConnectionMode.Else:
                    if (Node.Previous != null)
                        return false;
                    break;
            }

            return true;
        }
        
        private int GetGrip(Point location)
        {
            int result = -1;

            foreach (var grip in _grips)
            {
                var inflatedGrip = grip;
                inflatedGrip.Inflate(4, 4); // Forgive users for near-misses
                if (inflatedGrip.Contains(location))
                    result = _grips.IndexOf(grip);
            }

            return result;
        }

        public bool IsInView()
        {
            var newLocation = Editor.ToVisualCoord(Node.ScreenPosition);
            var newRect = ClientRectangle;
            newRect.Offset(newLocation);
            newRect.Inflate(_visibilityMargin, _visibilityMargin);

            return newRect.IntersectsWith(Editor.ClientRectangle);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);

            var obj = e.Data.GetData(e.Data.GetFormats()[0]) as TriggerNode;

            if (obj == Node)
                return;

            if (obj != null)
                e.Effect = DragDropEffects.Copy;

            var grip = GetGrip(PointToClient(new Point(e.X, e.Y)));
            if (grip == -1)
                return;

            var mode = (ConnectionMode)grip;

            if (ValidConnection(mode, obj) && Editor.AnimateSnap(mode, this))
                _lastSnappedGrip = grip;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);

            if (_lastSnappedGrip == -1)
            {
                Editor.ResetHotNode();
                return;
            }

            var mode = (ConnectionMode)_lastSnappedGrip;
            var obj = e.Data.GetData(e.Data.GetFormats()[0]) as TriggerNode;

            if (!ValidConnection(mode, obj))
            {
                Editor.ResetHotNode();
                return;
            }

            switch (mode)
            {
                case ConnectionMode.Previous:

                    if (obj is TriggerNodeCondition && Editor.HotNodeMode == ConnectionMode.Else)
                        (obj as TriggerNodeCondition).Else = Node;
                    else
                        obj.Next = Node;

                    Node.Previous = obj;

                    if (Editor.Nodes.Contains(Node))
                        Editor.Nodes.Remove(Node);

                    break;

                case ConnectionMode.Next:

                    obj.Previous = Node;
                    Node.Next = obj;

                    if (Editor.Nodes.Contains(obj))
                        Editor.Nodes.Remove(obj);

                    break;

                case ConnectionMode.Else:

                    if (this is VisibleNodeCondition)
                    {
                        var condNode = Node as TriggerNodeCondition;

                        obj.Previous = Node;
                        condNode.Else = obj;

                        if (Editor.Nodes.Contains(obj))
                            Editor.Nodes.Remove(obj);
                    }
                    break;
            }

            Editor.HotNode = null;
            Editor.Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _mouseDown = false;
            Editor.HotNode = null;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _currentGrip = _lastSnappedGrip = - 1;
            _mouseDown = false;

            Invalidate();
            Editor.FindForm().ActiveControl = null;
            Editor.Focus();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            _currentGrip = GetGrip(e.Location);
            Invalidate();

            if (_mouseDown || e.Button != MouseButtons.Left)
                return;

            _mouseDown = true;

            Capture = true;
            BringToFront();

            var grip = GetGrip(e.Location);
            if (grip != -1)
            {
                Editor.HotNodeMode = (ConnectionMode)grip;
                Editor.FindForm().ActiveControl = null;
                DoDragDrop(Node, DragDropEffects.Copy);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Editor.SelectNode(Node, !(Control.ModifierKeys == Keys.Control));
                BringToFront();
            }

            if (GetGrip(e.Location) == -1)
                base.OnMouseDown(e);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            if (Editor?.Resizing ?? true)
                return;

            var oldPosition = Node.ScreenPosition;
            StorePosition();
            Editor.MoveSelectedNodes(Node, Node.ScreenPosition - oldPosition);

            Editor.Update(); // Use instead of Invalidate() to avoid WinXP-like trails
            base.OnLocationChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!IsInView())
                return;
            
            base.OnPaint(e);

            foreach (var grip in _grips)
            {
                if (_grips.IndexOf(grip) == _currentGrip)
                    using (var brush = new SolidBrush((BackColor.ToFloat3Color() + new Vector3(0.2f, 0.2f, 0.2f)).ToWinFormsColor()))
                        e.Graphics.FillRectangle(brush, grip);

                using (var brush = new TextureBrush(MenuIcons.grip_fill, WrapMode.Tile))
                    e.Graphics.FillRectangle(brush, grip);
            }
        }
    }
}