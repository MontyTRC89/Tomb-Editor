using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using TombEditor.Geometry;
using SharpDX;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;
using RectangleF = System.Drawing.RectangleF;
using System.ComponentModel;

namespace TombEditor.Controls
{
    public class Panel2DMap : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector2 ViewPosition { get; set; } = new Vector2(60.0f, 60.0f);
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float ViewScale { get; set; } = 4.0f;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float ViewSpeedScaleScroll = 0.001f;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float ViewSpeedScaleKey = 0.17f;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float ViewSpeedMoveKey = 107.0f;

        private Editor _editor;
        private HashSet<Room> _roomsToMove; // Set to a valid list only if room dragging is active
        private Room _roomMouseClicked;
        private Vector2 _roomMouseOffset; // Relative vector to the position of the room for where it was clicked.
        private Vector2 _viewMoveMouseWorldCoord;
        private static readonly Brush _roomsNormalBrush = new SolidBrush(Color.FromArgb(170, 20, 200, 200));
        private static readonly Brush _roomsSelectionBrush = new SolidBrush(Color.FromArgb(170, 220, 20, 20));
        private static readonly Brush _roomsDragBrush = new SolidBrush(Color.FromArgb(40, 220, 20, 20));
        private static readonly Brush _roomsToMoveBrush = new SolidBrush(Color.FromArgb(70, 230, 230, 20));
        private static readonly Pen _roomBorderPen = new Pen(Color.Black, 1);
        private static readonly Pen _roomPortalPen = createPenWithDash(Color.FromArgb(220, 7, 70, 70), 1, DashStyle.Dot);
        private static readonly Pen _gridPenThin = new Pen(Color.LightGray, 1);
        private static readonly Pen _gridPenThick = new Pen(Color.LightGray, 3);
        
        public Panel2DMap()
        {
            _editor = Editor.Instance;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
        
        public void ResetView()
        {
            ViewPosition = (new Vector2(Width, Height) * 0.5f - new Vector2(16.0f)) / ViewScale;
        }

        private static Pen createPenWithDash(Color color, float width, DashStyle dashStyle)
        {
            Pen result = new Pen(color, width);
            result.DashStyle = dashStyle;
            return result;
        }

        private Vector2 FromVisualCoord(PointF pos)
        {
            return new Vector2((pos.X - Width * 0.5f) / ViewScale + ViewPosition.X, (pos.Y - Height * 0.5f) / ViewScale + ViewPosition.Y);
        }

        private PointF ToVisualCoord(Vector2 pos)
        {
            return new PointF((pos.X - ViewPosition.X) * ViewScale + Width * 0.5f, (pos.Y - ViewPosition.Y) * ViewScale  + Height * 0.5f);
        }

        private void MoveToFixedPoint(PointF VisualPoint, Vector2 WorldPoint)
        {
            //Adjust ViewPosition in such a way, that the FixedPoint does not move visually
            ViewPosition = -WorldPoint;
            ViewPosition = -FromVisualCoord(VisualPoint);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                // Try selecting or moving a room
                Vector2 clickPos = FromVisualCoord(e.Location);

                _roomMouseClicked = DoPicking(clickPos);
                if (_roomMouseClicked == null)
                    return;

                _editor.SelectedRoom = _roomMouseClicked;
                _roomsToMove = _editor.Level.GetConnectedRooms(_editor.SelectedRoom);
                _roomMouseOffset = clickPos - _roomMouseClicked.SectorPos;

                // Update state
                _editor.SelectRoom(_editor.SelectedRoom);
                Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Move view with mouse curser
                // Mouse curser is a fixed point
                _viewMoveMouseWorldCoord = FromVisualCoord(e.Location);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left && (_roomsToMove != null))
                UpdateRoomPosition(FromVisualCoord(e.Location) - _roomMouseOffset, _roomMouseClicked, _roomsToMove);
            else if (e.Button == MouseButtons.Right)
                MoveToFixedPoint(e.Location, _viewMoveMouseWorldCoord);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left && (_roomsToMove != null))
            {
                HashSet<Room> roomsToMove = _roomsToMove;
                _roomsToMove = null;
                UpdateRoomPosition(FromVisualCoord(e.Location) - _roomMouseOffset, _roomMouseClicked, roomsToMove);
            }
            else if (e.Button == MouseButtons.Right)
                MoveToFixedPoint(e.Location, _viewMoveMouseWorldCoord);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Vector2 FixedPointInWorld = FromVisualCoord(e.Location);
            ViewScale *= (float)Math.Exp(e.Delta * ViewSpeedScaleScroll);
            MoveToFixedPoint(e.Location, FixedPointInWorld);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Make control receive key events as suggested here...
            // https://stackoverflow.com/questions/20079373/trouble-creating-keydown-event-in-panel
            switch (keyData)
            {
                case Keys.Down:
                    ViewPosition += new Vector2(0.0f, ViewSpeedMoveKey / ViewScale);
                    Invalidate();
                    break;
                case Keys.Up:
                    ViewPosition += new Vector2(0.0f, -ViewSpeedMoveKey / ViewScale);
                    Invalidate();
                    break;
                case Keys.Left:
                    ViewPosition += new Vector2(-ViewSpeedMoveKey / ViewScale, 0.0f);
                    Invalidate();
                    break;
                case Keys.Right:
                    ViewPosition += new Vector2(ViewSpeedMoveKey / ViewScale, 0.0f);
                    Invalidate();
                    break;
                case Keys.PageDown:
                    ViewScale *= (float)Math.Exp(-ViewSpeedScaleKey);
                    Invalidate();
                    break;
                case Keys.PageUp:
                    ViewScale *= (float)Math.Exp(ViewSpeedScaleKey);
                    Invalidate();
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        
        protected override void OnMouseEnter(EventArgs e)
        {
            // Make this control able to receive scroll and key board events...
            base.OnMouseEnter(e);
            Focus();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            if ((_editor == null) || (_editor.Level == null))
                return;

            // Draw grid lines
            Vector2 GridLinesStart = FromVisualCoord(new PointF());
            Vector2 GridLinesEnd = FromVisualCoord(new PointF() + Size);
            GridLinesStart = Vector2.Clamp(GridLinesStart, new Vector2(0.0f), new Vector2(Level.MaxSectorCoord));
            GridLinesEnd = Vector2.Clamp(GridLinesEnd, new Vector2(0.0f), new Vector2(Level.MaxSectorCoord));
            Point GridLinesStartInt = new Point((int)Math.Floor(GridLinesStart.X), (int)Math.Floor(GridLinesStart.Y));
            Point GridLinesEndInt = new Point((int)Math.Ceiling(GridLinesEnd.X), (int)Math.Ceiling(GridLinesEnd.Y));

            for (int x = GridLinesStartInt.X; x <= GridLinesEndInt.X; ++x)
                g.DrawLine(((x % 10) == 0) ? _gridPenThick : _gridPenThin,
                    ToVisualCoord(new Vector2(x, 0)), ToVisualCoord(new Vector2(x, Level.MaxSectorCoord)));

            for (int y = GridLinesStartInt.Y; y <= GridLinesEndInt.Y; ++y)
                g.DrawLine(((y % 10) == 0) ? _gridPenThick : _gridPenThin, 
                    ToVisualCoord(new Vector2(0, y)), ToVisualCoord(new Vector2(Level.MaxSectorCoord, y)));

            // First, order the rooms so that we can than draw them from the highest to the lowest
            IEnumerable<Room> roomList = GetSortedRoomList();

            // Draw the rooms one by one
            foreach (Room room in roomList.Reverse())
            {
                int width = room.NumXSectors;
                int height = room.NumZSectors;
                
                // Fill area of room with rectangular stripes
                List<RectangleF> rectangles = new List<RectangleF>();
                for (int z = 1; z < (height - 1); ++z)
                {
                    int previousRectangleCount = rectangles.Count;
                    for (int x = 1; x < (width - 1); ++x)
                        if (room.Blocks[x, z].Type == BlockType.Floor)
                        {
                            int xBegin = x;
                            // Search for the next sector without a wall
                            for (; x < (width - 1); ++x)
                                if (room.Blocks[x, z].Type != BlockType.Floor)
                                    break;
                            rectangles.Add(new RectangleF(xBegin, z, x - xBegin, 1));
                        }

                    // Try to combine rectangle with the rectangle of the previous row
                    if ((rectangles.Count >= 2) && ((previousRectangleCount + 1) == rectangles.Count))
                    {
                        RectangleF PreviousRectangle = rectangles[rectangles.Count - 2];
                        RectangleF ThisRectangle = rectangles[rectangles.Count - 1];
                        if ((ThisRectangle.X == PreviousRectangle.X) &&
                            (ThisRectangle.Width == PreviousRectangle.Width) &&
                            (ThisRectangle.Top == PreviousRectangle.Bottom))
                        {
                            rectangles.RemoveAt(rectangles.Count - 1);
                            PreviousRectangle.Height += 1;
                            rectangles[rectangles.Count - 1] = PreviousRectangle;
                        }
                    }
                }

                // Transform coordinates
                for (int j = 0; j < rectangles.Count; ++j)
                    rectangles[j] = new RectangleF(
                        ToVisualCoord(new Vector2(rectangles[j].X + room.SectorPos.X, rectangles[j].Y + room.SectorPos.Y)),
                        new SizeF(rectangles[j].Width * ViewScale, rectangles[j].Height * ViewScale));

                // Draw
                Brush brush = _roomsNormalBrush;
                if (room == _editor.SelectedRoom)
                    brush = (_roomsToMove == null) ? _roomsSelectionBrush : _roomsDragBrush;
                else if ((_roomsToMove != null) && _roomsToMove.Contains(room))
                    brush = _roomsToMoveBrush;
                g.FillRectangles(brush, rectangles.ToArray());

                // Determine outlines of room
                for (int z = 1; z < height; ++z)
                    for (int x = 1; x < width; ++x)
                    {
                        Block thisBlock = room.Blocks[x, z];
                        Block aboveBlock = room.Blocks[x, z - 1];
                        Block leftBlock = room.Blocks[x - 1, z];
                        bool wallThis = thisBlock.Type != BlockType.Floor;
                        bool wallAbove = aboveBlock.Type != BlockType.Floor;
                        bool wallLeft = leftBlock.Type != BlockType.Floor;
                        if (wallAbove != wallThis)
                            g.DrawLine((aboveBlock.WallPortal != -1) || (thisBlock.WallPortal != -1) ? _roomPortalPen : _roomBorderPen,
                                ToVisualCoord(new Vector2(x + room.SectorPos.X, z + room.SectorPos.Y)),
                                ToVisualCoord(new Vector2(x + 1 + room.SectorPos.X, z + room.SectorPos.Y)));
                        if (wallLeft != wallThis)
                            g.DrawLine((leftBlock.WallPortal != -1) || (thisBlock.WallPortal != -1) ? _roomPortalPen : _roomBorderPen,
                                ToVisualCoord(new Vector2(x + room.SectorPos.X, z + room.SectorPos.Y)),
                                ToVisualCoord(new Vector2(x + room.SectorPos.X, z + 1 + room.SectorPos.Y)));
                    }
            }

            _editor.UpdateStatistics();
        }

        private struct RoomHeightComparer : IComparer<KeyValuePair<float, Room>>
        {
            public int Compare(KeyValuePair<float, Room> x, KeyValuePair<float, Room> y)
            {
                if (x.Key < y.Key)
                    return 1;
                if (x.Key > y.Key)
                    return -1;
                return 0;
            }
        };

        private void UpdateRoomPosition(Vector2 currentRoomPos, Room roomReference, HashSet<Room> roomsToMove)
        {
            currentRoomPos = new Vector2((float)Math.Round(currentRoomPos.X), (float)Math.Round(currentRoomPos.Y));
            //currentRoomPos = Vector2.Clamp(currentRoomPos, new Vector2(0), new Vector2(Level.MaxSectorCoord));
            Vector2 roomMovement = currentRoomPos - roomReference.SectorPos;
            foreach (Room room in roomsToMove)
                room.SectorPos += roomMovement;

            // Update state
            Invalidate();
            _editor.UpdateStatistics();
            _editor.CenterCamera();
        }

        private IEnumerable<Room> GetSortedRoomList()
        {
            var roomList = new List<KeyValuePair<float, Room>>();
            foreach (Room room in _editor.Level.Rooms)
                if (room != null)
                    roomList.Add(new KeyValuePair<float, Room>(room.Position.Y + room.GetHighestCorner(), room));
            roomList.Sort(new RoomHeightComparer());
            return roomList.Select(roomKey => roomKey.Value);
        }

        private Room DoPicking(Vector2 pos)
        {
            IEnumerable<Room> roomList = GetSortedRoomList();

            // Do collision detection for each room from top to bottom
            foreach (Room room in roomList)
            {
                float roomLocalX = pos.X - room.SectorPos.X;
                float roomLocalZ = pos.Y - room.SectorPos.Y;
                if ((roomLocalX >= 1) && (roomLocalZ >= 1) && (roomLocalX < (room.NumXSectors - 1)) && (roomLocalZ < (room.NumZSectors - 1)))
                    if (room.Blocks[(int)roomLocalX, (int)roomLocalZ].Type == BlockType.Floor)
                        return room;
            }

            return null;
        }
    }
}
