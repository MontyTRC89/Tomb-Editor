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
        public float ViewScale { get; set; } = 6.0f;

        private DepthBar _depthBar = new DepthBar();
        private Editor _editor;
        private HashSet<Room> _roomsToMove; // Set to a valid list only if room dragging is active
        private Room _roomMouseClicked;
        private Vector2 _roomMouseOffset; // Relative vector to the position of the room for where it was clicked.
        private Vector2? _viewMoveMouseWorldCoord;
        private int? _currentlyEditedDepthProbeIndex;
        private static readonly Brush _roomsNormalBrush = new SolidBrush(Color.FromArgb(180, 20, 200, 200));
        private static readonly Brush _roomsNormalAboveBrush = new SolidBrush(Color.FromArgb(120, 50, 50, 200));
        private static readonly Brush _roomsNormalBelowBrush = new SolidBrush(Color.FromArgb(180, 85, 85, 85));
        private static readonly Brush _roomsSelectionBrush = new SolidBrush(Color.FromArgb(180, 220, 20, 20));
        private static readonly Brush _roomsDragBrush = new SolidBrush(Color.FromArgb(40, 220, 20, 20));
        private static readonly Brush _roomsToMoveBrush = new SolidBrush(Color.FromArgb(70, 230, 230, 20));
        private static readonly Brush _roomsOutsideOverdraw = new SolidBrush(Color.FromArgb(185, 255, 255, 255));
        private static readonly Pen _roomBorderPen = new Pen(Color.Black, 1);
        private static readonly Pen _roomPortalPen = new Pen(Color.FromArgb(220, 7, 70, 70), 1) { DashStyle = DashStyle.Dot };
        private static readonly Pen _gridPenThin = new Pen(Color.LightGray, 1);
        private static readonly Pen _gridPenThick = new Pen(Color.LightGray, 3);
        private const float _probeRadius = 4;

        public Panel2DMap()
        {
            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            _depthBar.InvalidateParent += Invalidate;
            _depthBar.SelectRoom += delegate(Room room) { _editor.SelectRoomAndCenterCamera(room); Invalidate(); };
            _depthBar.RoomsMoved += delegate { _editor.RoomGeometryChange(null); _editor.CenterCamera(); };

            ResetView();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            base.Dispose(disposing);
        }
        
        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update drawing
            if ((obj is Editor.SelectedRoomChangedEvent) ||
                (obj is Editor.RoomGeometryChangedEvent) ||
                (obj is Editor.RoomListChangedEvent))
            {
                if (_editor.Mode == EditorMode.Map2D)
                    Invalidate();
            }
        }

        public void ResetView()
        {
            ViewPosition = (new Vector2(Width, Height) * 0.5f - new Vector2(16.0f)) / ViewScale;
        }
        
        private Vector2 FromVisualCoord(PointF pos)
        {
            return new Vector2((pos.X - Width * 0.5f) / ViewScale + ViewPosition.X, (Height * 0.5f - pos.Y) / ViewScale + ViewPosition.Y);
        }

        private PointF ToVisualCoord(Vector2 pos)
        {
            return new PointF((pos.X - ViewPosition.X) * ViewScale + Width * 0.5f, Height * 0.5f - (pos.Y - ViewPosition.Y) * ViewScale);
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

            Vector2 clickPos = FromVisualCoord(e.Location);
            if (!_depthBar.MouseDown(e, Size, _editor.Level, clickPos))
                return;

            if (e.Button == MouseButtons.Left)
            {
                // Try selecting or moving a room
                _roomMouseClicked = DoPicking(clickPos);
                if (_roomMouseClicked == null)
                    return;

                _editor.SelectedRoom = _roomMouseClicked;
                _roomsToMove = _editor.Level.GetConnectedRooms(_editor.SelectedRoom);
                _roomMouseOffset = clickPos - _roomMouseClicked.SectorPos;

                // Update state
                _editor.SelectedRoom = _editor.SelectedRoom;
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Move view with mouse curser
                // Mouse curser is a fixed point
                _viewMoveMouseWorldCoord = clickPos;
            }
            else if ((e.Button == MouseButtons.Middle) || (e.Button == MouseButtons.XButton2))
            {
                _currentlyEditedDepthProbeIndex =  _depthBar.DepthProbes.FindIndex((depthProbe) => (
                    (Math.Round(depthProbe.X) == Math.Round(clickPos.X)) &&
                    (Math.Round(depthProbe.Y) == Math.Round(clickPos.Y))));
                if (_currentlyEditedDepthProbeIndex == -1)
                {
                    _currentlyEditedDepthProbeIndex = _depthBar.DepthProbes.Count;
                    _depthBar.DepthProbes.Add(new Vector2());
                }
                _depthBar.DepthProbes[_currentlyEditedDepthProbeIndex.Value] = clickPos;
                Invalidate();
            }
            else if ((e.Button == MouseButtons.XButton1) && (_depthBar.DepthProbes.Count > 0))
            {
                // Remove depth probe closest to mouse pointer
                int currentProbeIndex = 0;
                for (int i = 0; i < _depthBar.DepthProbes.Count; ++i)
                    if ((clickPos - _depthBar.DepthProbes[i]).LengthSquared() < (clickPos - _depthBar.DepthProbes[currentProbeIndex]).LengthSquared())
                        currentProbeIndex = i;
                if ((clickPos - _depthBar.DepthProbes[currentProbeIndex]).LengthSquared() <= 14.0f)
                    _depthBar.DepthProbes.RemoveAt(currentProbeIndex);
                Invalidate();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            if ((e.Button == MouseButtons.Middle) || (e.Button == MouseButtons.XButton2))
            {
                Vector2 clickPos = FromVisualCoord(e.Location);
                int currentProbeIndex = _depthBar.DepthProbes.FindIndex((depthProbe) => (
                   (Math.Round(depthProbe.X) == Math.Round(clickPos.X)) &&
                   (Math.Round(depthProbe.Y) == Math.Round(clickPos.Y))));
                if (currentProbeIndex != -1)
                {
                    _depthBar.DepthProbes.RemoveAt(currentProbeIndex);
                    Invalidate();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Update depth bar...
            _depthBar.MouseMove(e, Size);
            RectangleF area = _depthBar.groupGetArea(_depthBar.getBarArea(Size), _depthBar.DepthProbes.Count); // Only redraw the depth bar group for the curser. 
            Invalidate(new Rectangle((int)area.X, (int)area.Y, (int)area.Width, (int)area.Height));

            if (_editor.Action.RelocateCameraActive)
                return;

            if ((e.Button == MouseButtons.Left) && (_roomsToMove != null))
                UpdateRoomPosition(FromVisualCoord(e.Location) - _roomMouseOffset, _roomMouseClicked, _roomsToMove);
            else if ((e.Button == MouseButtons.Right) && (_viewMoveMouseWorldCoord != null))
                MoveToFixedPoint(e.Location, _viewMoveMouseWorldCoord.Value);
            else if (((e.Button == MouseButtons.Middle) || (e.Button == MouseButtons.XButton2)) && _currentlyEditedDepthProbeIndex.HasValue)
            {
                _depthBar.DepthProbes[_currentlyEditedDepthProbeIndex.Value] = FromVisualCoord(e.Location);
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _depthBar.MouseUp(e, Size);

            if (e.Button == MouseButtons.Left && (_roomsToMove != null))
            {
                _roomsToMove = null;
                _roomMouseClicked = null;
                Invalidate();
            }
            else if ((e.Button == MouseButtons.Right) && (_viewMoveMouseWorldCoord != null))
            {
                _viewMoveMouseWorldCoord = null;
            }
            _currentlyEditedDepthProbeIndex = null;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Vector2 FixedPointInWorld = FromVisualCoord(e.Location);
            ViewScale *= (float)Math.Exp(e.Delta * _editor.Configuration.Map2D_NavigationSpeedMouseZoom);
            MoveToFixedPoint(e.Location, FixedPointInWorld);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Make control receive key events as suggested here...
            // https://stackoverflow.com/questions/20079373/trouble-creating-keydown-event-in-panel
            switch (keyData)
            {
                case Keys.Down:
                    ViewPosition += new Vector2(0.0f, -_editor.Configuration.Map2D_NavigationSpeedKeyMove / ViewScale);
                    Invalidate();
                    break;
                case Keys.Up:
                    ViewPosition += new Vector2(0.0f, _editor.Configuration.Map2D_NavigationSpeedKeyMove / ViewScale);
                    Invalidate();
                    break;
                case Keys.Left:
                    ViewPosition += new Vector2(-_editor.Configuration.Map2D_NavigationSpeedKeyMove / ViewScale, 0.0f);
                    Invalidate();
                    break;
                case Keys.Right:
                    ViewPosition += new Vector2(_editor.Configuration.Map2D_NavigationSpeedKeyMove / ViewScale, 0.0f);
                    Invalidate();
                    break;
                case Keys.PageDown:
                    ViewScale *= (float)Math.Exp(-_editor.Configuration.Map2D_NavigationSpeedKeyZoom);
                    Invalidate();
                    break;
                case Keys.PageUp:
                    ViewScale *= (float)Math.Exp(_editor.Configuration.Map2D_NavigationSpeedKeyZoom);
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
            if ((_editor == null) || (_editor.Level == null))
                return;

            // draw 2d map if necessary and not occluded by 2d bar
            if (!_depthBar.getBarArea(Size).Contains(e.ClipRectangle))
            {
                e.Graphics.Clear(Color.White);

                // Draw hidden rooms
                float currentRangeMin = _editor.SelectedRoom.Position.Y + _editor.SelectedRoom.GetLowestCorner();
                float currentRangeMax = _editor.SelectedRoom.Position.Y + _editor.SelectedRoom.GetHighestCorner();
                IEnumerable<Room> sortedRoomList = _editor.Level.GetVerticallyAscendingRoomList();
                bool drewAny = false;
                foreach (Room room in sortedRoomList)
                    if (!_depthBar.CheckRoom(room)) // Check if the room fits the depth bar criterion
                    {
                        drewAny = true;
                        DrawRoom(e, room, currentRangeMin, currentRangeMax, true, false);
                    }
                if (drewAny)
                    e.Graphics.FillRectangle(_roomsOutsideOverdraw, e.ClipRectangle); //Make the rooms in the background appear faded

                // Draw grid lines
                Vector2 GridLines0 = FromVisualCoord(new PointF());
                Vector2 GridLines1 = FromVisualCoord(new PointF() + Size);
                Vector2 GridLinesStart = Vector2.Min(GridLines0, GridLines1);
                Vector2 GridLinesEnd = Vector2.Max(GridLines0, GridLines1);
                GridLinesStart = Vector2.Clamp(GridLinesStart, new Vector2(0.0f), new Vector2(Level.MaxSectorCoord));
                GridLinesEnd = Vector2.Clamp(GridLinesEnd, new Vector2(0.0f), new Vector2(Level.MaxSectorCoord));
                Point GridLinesStartInt = new Point((int)Math.Floor(GridLinesStart.X), (int)Math.Floor(GridLinesStart.Y));
                Point GridLinesEndInt = new Point((int)Math.Ceiling(GridLinesEnd.X), (int)Math.Ceiling(GridLinesEnd.Y));

                for (int x = GridLinesStartInt.X; x <= GridLinesEndInt.X; ++x)
                    e.Graphics.DrawLine(((x % 10) == 0) ? _gridPenThick : _gridPenThin,
                        ToVisualCoord(new Vector2(x, 0)), ToVisualCoord(new Vector2(x, Level.MaxSectorCoord)));

                for (int y = GridLinesStartInt.Y; y <= GridLinesEndInt.Y; ++y)
                    e.Graphics.DrawLine(((y % 10) == 0) ? _gridPenThick : _gridPenThin,
                        ToVisualCoord(new Vector2(0, y)), ToVisualCoord(new Vector2(Level.MaxSectorCoord, y)));

                // Draw visible rooms
                foreach (Room room in sortedRoomList) 
                    if (_depthBar.CheckRoom(room)) // Check if the room fits the depth bar criterion
                        DrawRoom(e, room, currentRangeMin, currentRangeMax, true, true);

                // Draw probe positions with digits
                for (int i = 0; i < _depthBar.DepthProbes.Count; ++i)
                {
                    PointF depthProbeVisualPos = ToVisualCoord(_depthBar.DepthProbes[i]);
                    e.Graphics.DrawLine(DepthBar.ProbePen, depthProbeVisualPos.X - _probeRadius, depthProbeVisualPos.Y - _probeRadius, depthProbeVisualPos.X + _probeRadius, depthProbeVisualPos.Y + _probeRadius);
                    e.Graphics.DrawLine(DepthBar.ProbePen, depthProbeVisualPos.X - _probeRadius, depthProbeVisualPos.Y + _probeRadius, depthProbeVisualPos.X + _probeRadius, depthProbeVisualPos.Y - _probeRadius);
                    e.Graphics.DrawString(i.ToString(), DepthBar.ProbeFont, DepthBar.ProbePen.Brush, new RectangleF(depthProbeVisualPos.X - 500.0f, depthProbeVisualPos.Y - 500.0f, 1000.0f, 500.0f), DepthBar.ProbeStringLayout);
                }
            }

            // draw depth bar
            Vector2 curserPos = FromVisualCoord(PointToClient(MousePosition));
            _depthBar.Draw(e, Size, _editor.Level, curserPos, _editor.SelectedRoom, _roomsToMove);
        }

        private void DrawRoom(PaintEventArgs e, Room room, float currentRangeMin, float currentRangeMax, bool drawFilled, bool drawOutline)
        {
            int width = room.NumXSectors;
            int height = room.NumZSectors;
            if (drawFilled)
            {
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
                        RectangleF previousRectangle = rectangles[rectangles.Count - 2];
                        RectangleF thisRectangle = rectangles[rectangles.Count - 1];
                        if ((thisRectangle.X == previousRectangle.X) &&
                            (thisRectangle.Width == previousRectangle.Width) &&
                            (thisRectangle.Top == previousRectangle.Bottom))
                        {
                            rectangles.RemoveAt(rectangles.Count - 1);
                            previousRectangle.Height += 1;
                            rectangles[rectangles.Count - 1] = previousRectangle;
                        }
                    }
                }

                // Transform coordinates
                for (int j = 0; j < rectangles.Count; ++j)
                    rectangles[j] = new RectangleF(
                        ToVisualCoord(new Vector2(rectangles[j].X + room.SectorPos.X, rectangles[j].Bottom + room.SectorPos.Y)),
                        new SizeF(rectangles[j].Width * ViewScale, rectangles[j].Height * ViewScale));

                // Draw the rectangular stripes
                Brush brush = _roomsNormalBrush;
                if ((room.Position.Y + room.GetHighestCorner()) <= currentRangeMin)
                    brush = _roomsNormalBelowBrush;
                if ((room.Position.Y + room.GetLowestCorner()) >= currentRangeMax)
                    brush = _roomsNormalAboveBrush;
                
                if (room == _editor.SelectedRoom)
                    brush = (_roomsToMove == null) ? _roomsSelectionBrush : _roomsDragBrush;
                else if ((_roomsToMove != null) && _roomsToMove.Contains(room))
                    brush = _roomsToMoveBrush;
                else if ((_depthBar.RoomsToMove != null) && _depthBar.RoomsToMove.Contains(room))
                    brush = _roomsToMoveBrush;

                if (rectangles.Count > 0)
                    e.Graphics.FillRectangles(brush, rectangles.ToArray());
            }

            if (drawOutline)
            {
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
                            e.Graphics.DrawLine((aboveBlock.WallPortal != -1) || (thisBlock.WallPortal != -1) ? _roomPortalPen : _roomBorderPen,
                                ToVisualCoord(new Vector2(x + room.SectorPos.X, z + room.SectorPos.Y)),
                                ToVisualCoord(new Vector2(x + 1 + room.SectorPos.X, z + room.SectorPos.Y)));
                        if (wallLeft != wallThis)
                            e.Graphics.DrawLine((leftBlock.WallPortal != -1) || (thisBlock.WallPortal != -1) ? _roomPortalPen : _roomBorderPen,
                                ToVisualCoord(new Vector2(x + room.SectorPos.X, z + room.SectorPos.Y)),
                                ToVisualCoord(new Vector2(x + room.SectorPos.X, z + 1 + room.SectorPos.Y)));
                    }
            }
        }

        private void UpdateRoomPosition(Vector2 newRoomPos, Room roomReference, HashSet<Room> roomsToMove)
        {
            newRoomPos = new Vector2((float)Math.Round(newRoomPos.X), (float)Math.Round(newRoomPos.Y));
            //currentRoomPos = Vector2.Clamp(currentRoomPos, new Vector2(0), new Vector2(Level.MaxSectorCoord));
            Vector2 roomMovement = newRoomPos - roomReference.SectorPos;
            foreach (Room room in roomsToMove)
                room.SectorPos += roomMovement;

            // Update state
            Invalidate();
            _editor.RoomSectorPropertiesChange(roomReference);
            _editor.CenterCamera();
        }

        private Room DoPicking(Vector2 pos)
        {
            IEnumerable<Room> roomList = _editor.Level.GetVerticallyAscendingRoomList();

            // Do collision detection for each room from top to bottom
            foreach (Room room in roomList.Reverse())
            {
                // Check if the room fits the depth bar criterion
                if (!_depthBar.CheckRoom(room))
                    continue;

                // Check if there is a collision
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
