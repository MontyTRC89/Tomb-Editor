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
        protected const int _elementSpacing = 8;
        protected const int _elementHeight = 24;

        protected const int _visibilityMargin = 32;

        protected List<Rectangle> _grips = new List<Rectangle>();
        protected int _currentGrip = -1;
        protected int _lastSnappedGrip = -1;

        private bool _mouseDown = false;

        public NodeEditor Editor => Parent as NodeEditor;
        public TriggerNode Node { get; protected set; }

        private List<ArgumentEditor> _argControls = new List<ArgumentEditor>();

        // This constructor overload is needed so that VS designer don't get crazy.
        public VisibleNodeBase() : this(new TriggerNodeAction()) { }

        public VisibleNodeBase(TriggerNode node)
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            InitializeComponent();
            SnapToBorders = false;
            Node = node;

            SpawnGrips();
        }

        protected virtual void SpawnGrips()
        {
            _grips.Clear();
            _grips.Add(new Rectangle(Width / 2 - _gripWidth / 2, 0, _gripWidth, _gripHeight));
            Invalidate();
        }

        public void SpawnFunctionList(List<NodeFunction> functions)
        {
            if (functions == null)
                return;

            cbFunction.Items.Clear();
            foreach (var f in functions)
                cbFunction.Items.Add(f);

            if (cbFunction.Items.Count > 0)
                cbFunction.SelectedIndex = 0;
        }

        public void SpawnUIElements()
        {
            var func = cbFunction.SelectedItem as NodeFunction;

            if (func == null)
                return;

            if (_argControls.Count > func.Arguments.Count)
                for (int i = _argControls.Count - 1; i >= 0; i--)
                {
                    var control = _argControls[i];
                    control.ValueChanged -= Ctrl_ValueChanged;

                    if (Controls.Contains(control))
                        Controls.Remove(control);

                    _argControls.RemoveAt(i);
                }

            int oldArgControls = _argControls.Count;
            for (int i = oldArgControls; i < func.Arguments.Count; i++)
            {
                var newY = cbFunction.Location.Y +
                           cbFunction.Size.Height +
                           _elementSpacing +
                           (_elementSpacing + _elementHeight) * _argControls.Count;

                var ctrl = new ArgumentEditor()
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Name = "argEditor" + i.ToString(),
                    Visible = false
                };

                ctrl.SetArgumentType(func.Arguments[i], Editor);

                _argControls.Add(ctrl);
                Controls.Add(ctrl);

                // If placed in constructor, it won't work properly.
                ctrl.Size = new Size(cbFunction.Width, cbFunction.Height);
                ctrl.Location = new Point(cbFunction.Left, newY);
                ctrl.Visible = true;

                ctrl.ValueChanged += Ctrl_ValueChanged;
            }

            var newHeight = cbFunction.Location.Y +
                            cbFunction.Size.Height +
                            _elementSpacing +
                            (_elementSpacing + _elementHeight) * _argControls.Count + 1;

            Size = new Size(Size.Width, newHeight);

            for (int i = 0; i < _argControls.Count; i++)
                _argControls[i].SetArgumentType(func.Arguments[i], Editor);

            for (int i = 0; i < Node.Arguments.Count; i++)
                RefreshArgument(i);

            Editor?.Invalidate();
        }

        private void Ctrl_ValueChanged(object sender, EventArgs e)
        {
            var ctrl = sender as ArgumentEditor;
            int index = _argControls.IndexOf(ctrl);

            if (index != -1 && Node.Arguments.Count > index)
                Node.Arguments[index] = ctrl.Text;
        }

        public void RefreshArgument(int index)
        {
            if (Node.Arguments.Count < index && _argControls.Count < index)
                _argControls[index].Text = Node.Arguments[index];
        }

        public void RefreshPosition()
        {
            if (IsInView())
            {
                if (!Visible) Visible = true;

                var newX = MathC.Clamp(Node.ScreenPosition.X, 0, Editor.GridSize);
                var newY = MathC.Clamp(Node.ScreenPosition.Y, 0, Editor.GridSize);
                Location = Editor.ToVisualCoord(new Vector2(newX, newY));
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
                    if (Node.Previous != null)
                        return false;
                    break;

                case ConnectionMode.Else:
                    if ((Node as TriggerNodeCondition)?.Else != null)
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
            if (Editor == null)
                return true;

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

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _currentGrip = _lastSnappedGrip = - 1;
            _mouseDown = false;

            Invalidate();

            if (Editor == null)
                return;

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
                bool ctrlPressed  = Control.ModifierKeys.HasFlag(Keys.Control);
                bool shiftPressed = Control.ModifierKeys.HasFlag(Keys.Shift);

                Editor.SelectNode(Node, !shiftPressed, !ctrlPressed && !shiftPressed);
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

            Editor.Refresh(); // Use instead of Invalidate() to avoid WinXP-like trails
            base.OnLocationChanged(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SpawnGrips();
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

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Prevent controls flickering
                return cp;
            }
        }

        private void cbFunction_SelectedIndexChanged(object sender, EventArgs e)
        {
            Node.Function = (cbFunction.SelectedItem as NodeFunction).Name;
            Node.Arguments.Clear();

            // TODO: Update variables properly
            for (int i = 0; i < (cbFunction.SelectedItem as NodeFunction).Arguments.Count; i++)
                Node.Arguments.Add(string.Empty);

            SpawnUIElements();
        }
    }
}