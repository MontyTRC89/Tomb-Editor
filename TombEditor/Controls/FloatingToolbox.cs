using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TombEditor.Controls
{

    public partial class FloatingToolbox : UserControl
    {
        public class FloatingToolboxContainer
        {
            public FloatingToolbox Toolbox;
            public FloatingToolboxContainer(FloatingToolbox toolbox) { Toolbox = toolbox; }
        }

        [Category("Appearance")]
        public short Transparency
        {
            get { return base.BackColor.A; }
            set
            {
                base.BackColor = Color.FromArgb(value, base.BackColor.R, base.BackColor.G, base.BackColor.B);
                Refresh();
            }
        }

        [Category("Layout")]
        public Size SnappingMargin
        {
            get { return _dragSnappingMargin; }
            set { _dragSnappingMargin = value; }
        }

        private Size _dragSnappingMargin = new Size(10, 10);
        private Rectangle _dragBounds;
        private Point _dragOffset;

        public FloatingToolbox()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        public void DragStart(Point offset)
        {
            _dragBounds = Parent.ClientRectangle;
            _dragBounds.Width -= Width;
            _dragBounds.Height -= Height;

            _dragOffset = (offset);

            Enabled = false;

            if (DoDragDrop(new FloatingToolboxContainer(this), DragDropEffects.Move) == DragDropEffects.None)
                Enabled = true;
        }

        public void DragStop()
        {
            Enabled = true;
        }

        private void ClampPosition()
        {
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ClampPosition();
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            ClampPosition();
        }

        protected override void OnQueryContinueDrag(QueryContinueDragEventArgs qcdevent)
        {
            base.OnQueryContinueDrag(qcdevent);

            if (Parent.RectangleToScreen(Parent.ClientRectangle).Contains(Cursor.Position))
            {
                var nextLocation = Parent.PointToClient(new Point(Cursor.Position.X - _dragOffset.X, Cursor.Position.Y - _dragOffset.Y));

                // Snap toolbox to parent border
                nextLocation.Offset((nextLocation.X < _dragSnappingMargin.Width ? -nextLocation.X : 0), (nextLocation.Y < _dragSnappingMargin.Height ? -nextLocation.Y : 0));
                nextLocation.Offset((nextLocation.X > _dragBounds.Width - _dragSnappingMargin.Width ? -(nextLocation.X - _dragBounds.Width) : 0), (nextLocation.Y > _dragBounds.Height - _dragSnappingMargin.Height ? -(nextLocation.Y - _dragBounds.Height) : 0));

                this.Location = nextLocation;
                Refresh(); // We need to invalidate all controls behind
            }
        }
    }
}
