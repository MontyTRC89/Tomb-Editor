using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using TombEditor.Geometry;
using TombLib.Utils;
using Color = System.Drawing.Color;
using RectangleF = System.Drawing.RectangleF;
using System.Drawing.Drawing2D;
using DarkUI.Forms;

namespace TombEditor.Controls
{
    public class DepthBar
    {
        public const float MinDepth = -128;
        public const float MaxDepth = 127;
        public readonly List<DepthProbe> DepthProbes = new List<DepthProbe>();
        public event Action InvalidateParent;
        public event Func<IWin32Window> GetParent;
        public event Action<IEnumerable<Room>> SelectedRoom;

        private const float _marginX = 10.0f;
        private const float _marginY = 48.0f;
        private const float _barWidth = 36.0f;
        private const int _heightStringCount = 16;
        private const float _heightStringLineLength = 4.0f;
        private const float _heightStringLineDistance = 6.0f;
        private const float _heightStringFadeDistance = 12.0f;
        private const float _heightStringArrowSize = 3.0f;
        private const float _selectionInsideOffset = -1.0f;
        private const float _selectionOutsideOffset = -2.0f;
        private const float _selectionMaxPixelDistanceForMove = 8.0f;
        private const float _minDepthDifferenceBetweenIndependentlyMergedSequences = 12.0f;

        private float _selectedLimit0 { get; set; } = MinDepth;
        private float _selectedLimit1 { get; set; } = MaxDepth;
        private Room _roomMouseClicked;
        private HashSet<Room> _roomsToMove; // Set to a valid list only if room dragging is active
        private float _roomMouseOffset; // Relative depth difference to where it was clicked.
        private float _barMouseOffset;

        public static readonly Color[] ProbeColors = {
            Color.Crimson,
            Color.Purple,
            Color.DarkGreen,
            Color.Maroon,
            Color.DarkOrange,
            Color.MidnightBlue };

        public static readonly StringFormat ProbeStringLayout = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
        public static readonly Font ProbeFont = new Font("Segoe UI", 14.0f, FontStyle.Bold, GraphicsUnit.Pixel);
        private static readonly Font _heightStringFont = new Font("Segoe UI", 11.0f, FontStyle.Bold, GraphicsUnit.Pixel);
        private static readonly StringFormat _heightStringLayout = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
        private static readonly Brush _backgroundBrush = new SolidBrush(Color.FromArgb(245, 245, 245));
        private static readonly Pen _outlinePen = new Pen(Color.FromArgb(245, 80, 80, 80), 1);
        private static readonly Pen _heightLinesPen = new Pen(Color.FromArgb(220, 220, 220), 1);
        private static readonly Pen _heightLinesBigPen = new Pen(Color.FromArgb(200, 200, 200), 3);
        private static readonly Pen _sequenceSeperatorPen = _outlinePen;
        private static readonly Pen _portalPen = new Pen(Color.Black, 1) { DashStyle = DashStyle.Dot };
        private static readonly Pen _roomBoundsPen = _outlinePen;
        private static readonly Pen _selectionPen = new Pen(Color.FromArgb(220, 40, 0, 120), 4);
        private static readonly Brush _selectionBrush = new SolidBrush(Color.FromArgb(40, 40, 0, 120));
        private static readonly Brush _roomsNormalBrush = new SolidBrush(HighlightState.ColorFloor.ToWinFormsColor());
        private static readonly Brush _roomsWallBrush = new SolidBrush(HighlightState.ColorWall.ToWinFormsColor());
        private static readonly Brush _roomsOutsideOverdraw = new SolidBrush(Color.FromArgb(180, 240, 240, 240));

        public class DepthProbe
        {
            public Vector2 Position { get; set; }
            public Color Color { get; set; }

            public DepthProbe(DepthBar parent)
            {
                Color = parent.getProbeColor();
            }
        }

        private struct RelevantRoom
        {
            public Room Room;
            public Block Block;
            public float MinDepth;
            public float MaxDepth;
        }

        private enum SelectionMode
        {
            None,
            SelectedLimit0,
            SelectedLimit1,
            SelectedLimitBoth,
            RoomMove
        }
        private SelectionMode _selectionMode = SelectionMode.None;

        public RectangleF getBarArea(Size parentControlSize)
        {
            float barsWidth = _barWidth * (DepthProbes.Count + 1);
            return new RectangleF(
                parentControlSize.Width - barsWidth - _marginX, _marginY,
                barsWidth, Math.Max(parentControlSize.Height - 2 * _marginY, 64.0f));
        }

        private Color getProbeColor()
        {
            return DepthProbes
                .Select(probe => probe.Color)
                .Concat(ProbeColors) // Make a list of *all* colors
                .GroupBy(color => color) // Group same colors
                .OrderBy(group => group.Count()) // Sort color groups after how often they appeared
                .First().First(); // Use first color of the least occurring color group.
        }

        private static float FromVisualY(RectangleF barArea, float y)
        {
            float depth = MaxDepth - ((y - barArea.Y) / barArea.Height) * (MaxDepth - MinDepth);
            depth = Math.Max(Math.Min(depth, MaxDepth), MinDepth);
            return depth;
        }

        private static float ToVisualY(RectangleF barArea, float depth)
        {
            return ((MaxDepth - depth) / (MaxDepth - MinDepth)) * barArea.Height + barArea.Y;
        }

        public static bool CheckForLockedRooms(IWin32Window window, IEnumerable<Room> rooms)
        {
            if (rooms.All(room => !room.Locked))
                return false;

            // Inform user and offer an option to unlock the room position
            string message = "Room movement is disabled because some room positions are locked. Do you really want to continue? " +
                "Those are the rooms that are currently locked: " + rooms.Where(room => room.Locked)
                .Aggregate("", (str, room) => str + ", " + room, str => str.Substring(str.Length - 2));
            if (DarkMessageBox.Show(window, message, "Locked rooms", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return true;

            // Unlock rooms
            foreach (Room room in rooms)
                room.Locked = false;
            return false;
        }

        /// <returns>true, if the selection should continue in the background of the bar.</returns>
        public bool MouseDown(MouseEventArgs e, Size parentControlSize, Level level, Vector2 clickPos)
        {
            _selectionMode = SelectionMode.None;

            // check if the mouse click was in the bar area
            RectangleF barArea = getBarArea(parentControlSize);
            RectangleF selectionArea = barArea;
            selectionArea.Inflate(10.0f, _selectionMaxPixelDistanceForMove * 0.8f);
            if (!selectionArea.Contains(e.Location))
                return true;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    // check if the mouse click was on one of the two sliders
                    float distanceToSelectedLimit0 = Math.Abs(e.Y - ToVisualY(barArea, _selectedLimit0));
                    float distanceToSelectedLimit1 = Math.Abs(e.Y - ToVisualY(barArea, _selectedLimit1));
                    if (distanceToSelectedLimit0 < distanceToSelectedLimit1)
                    {
                        if (distanceToSelectedLimit0 < _selectionMaxPixelDistanceForMove)
                            _selectionMode = SelectionMode.SelectedLimit0;
                    }
                    else
                    {
                        if (distanceToSelectedLimit1 < _selectionMaxPixelDistanceForMove)
                            _selectionMode = SelectionMode.SelectedLimit1;
                    }

                    // check if a the click happend on a room
                    if (barArea.Contains(e.Location) && (_selectionMode == SelectionMode.None))
                    {
                        for (int groupIndex = 0; groupIndex < GroupCount; ++groupIndex)
                        {
                            RectangleF groupArea = groupGetArea(barArea, groupIndex);
                            if (groupArea.Contains(e.Location))
                            {
                                float mouseDepth = FromVisualY(barArea, e.Y);
                                List<List<RelevantRoom>> roomSequences = groupBuildRoomSequences(level, clickPos, groupIndex);
                                float sequenceWidth = groupArea.Width / roomSequences.Count;
                                for (int i = 0; i < roomSequences.Count; ++i)
                                {
                                    float posX0 = groupArea.X + sequenceWidth * i;
                                    float posX1 = groupArea.X + sequenceWidth * (i + 1);
                                    if ((e.X >= posX0) && (e.X <= posX1))
                                        for (int j = roomSequences[i].Count - 1; j >= 0; --j)
                                            if ((mouseDepth <= roomSequences[i][j].MaxDepth) && (mouseDepth >= roomSequences[i][j].MinDepth))
                                            {
                                                _roomMouseClicked = roomSequences[i][j].Room;
                                                _roomMouseOffset = mouseDepth - _roomMouseClicked.Position.Y;
                                                SelectedRoom?.Invoke(new Room[] { _roomMouseClicked });
                                                InvalidateParent?.Invoke();
                                                _selectionMode = SelectionMode.RoomMove;
                                                return false;
                                            }
                                }
                                break;
                            }
                        }

                        selectionArea.Y = ToVisualY(barArea, Math.Max(_selectedLimit0, _selectedLimit1));
                        selectionArea.Height = Math.Abs(ToVisualY(barArea, _selectedLimit0) - ToVisualY(barArea, _selectedLimit1));

                        if (selectionArea.Contains(e.Location))
                        {
                            _barMouseOffset = distanceToSelectedLimit0;
                            _selectionMode = SelectionMode.SelectedLimitBoth;
                        }
                    }
                    break;

                case MouseButtons.Middle:
                case MouseButtons.XButton1:
                case MouseButtons.XButton2:
                    for (int groupIndex = 0; groupIndex < DepthProbes.Count; ++groupIndex)
                    {
                        RectangleF groupArea = groupGetArea(barArea, groupIndex);
                        if (groupArea.Contains(e.Location))
                        {
                            DepthProbes.RemoveAt(groupIndex);
                            InvalidateParent?.Invoke();
                            break;
                        }
                    }
                    break;
            }

            return false;
        }

        public void MouseMove(MouseEventArgs e, Size parentControlSize, Level level)
        {
            RectangleF barArea = getBarArea(parentControlSize);

            switch (_selectionMode)
            {
                case SelectionMode.SelectedLimit0:
                    _selectedLimit0 = FromVisualY(barArea, e.Y);
                    InvalidateParent?.Invoke();
                    break;

                case SelectionMode.SelectedLimit1:
                    _selectedLimit1 = FromVisualY(barArea, e.Y);
                    InvalidateParent?.Invoke();
                    break;

                case SelectionMode.SelectedLimitBoth:
                    float barHeight = _selectedLimit0 - _selectedLimit1;
                    _selectedLimit0 = FromVisualY(barArea, _barMouseOffset + e.Y);
                    _selectedLimit1 = _selectedLimit0 + barHeight;
                    InvalidateParent?.Invoke();
                    break;

                case SelectionMode.RoomMove:
                    float destinationHeight = ((float)Math.Round(FromVisualY(barArea, e.Y) - _roomMouseOffset));

                    if (_roomsToMove == null)
                    {
                        HashSet<Room> roomsToMove = level.GetConnectedRooms(_roomMouseClicked);
                        if (CheckForLockedRooms(GetParent?.Invoke(), roomsToMove))
                        {
                            _roomsToMove = null;
                            _selectionMode = SelectionMode.None;
                            break;
                        }
                        _roomsToMove = roomsToMove;
                        InvalidateParent?.Invoke();
                    }

                    // limit room movement to valid range
                    float maxHeight = MaxDepth;
                    float minHeight = MinDepth;
                    foreach (Room room in _roomsToMove)
                    {
                        float roomUpperLimit = MaxDepth - (room.Position.Y - _roomMouseClicked.Position.Y + room.GetHighestCorner());
                        float roomLowerLimit = MinDepth - (room.Position.Y - _roomMouseClicked.Position.Y + room.GetLowestCorner());
                        maxHeight = Math.Min(maxHeight, roomUpperLimit);
                        minHeight = Math.Max(minHeight, roomLowerLimit);
                    }
                    destinationHeight = Math.Max(Math.Min(destinationHeight, maxHeight), minHeight);

                    // do movement
                    EditorActions.MoveRooms(new Vector3(0, destinationHeight - _roomMouseClicked.Position.Y, 0), _roomsToMove);
                    break;
            }
        }

        public void MouseUp(MouseEventArgs e, Size parentControlSize, Level level)
        {
            _selectionMode = SelectionMode.None;
            if (_roomsToMove != null)
                InvalidateParent?.Invoke();
            _roomsToMove = null;
            _roomMouseClicked = null;
        }

        public void Draw(PaintEventArgs e, Size parentControlSize, Level level, Vector2 cursorPos, Func<Room, Brush, ConditionallyOwned<Brush>> getRoomBrush)
        {
            RectangleF barArea = getBarArea(parentControlSize);
            float selectedLimit0PosY = ToVisualY(barArea, _selectedLimit0);
            float selectedLimit1PosY = ToVisualY(barArea, _selectedLimit1);

            // Draw box
            e.Graphics.FillRectangle(_backgroundBrush, barArea);

            // Draw height lines
            if (barArea.IntersectsWith(e.ClipRectangle))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                for (int depth = (int)MinDepth; depth <= (int)MaxDepth; ++depth)
                {
                    float posY = ToVisualY(barArea, depth);
                    e.Graphics.DrawLine(_heightLinesPen, barArea.Left, posY, barArea.Right, posY);
                }
                e.Graphics.SmoothingMode = SmoothingMode.Default;
            }

            // Draw height strings
            for (int i = 0; i <= _heightStringCount; ++i)
            {
                float depth = MaxDepth - ((MaxDepth - MinDepth) / _heightStringCount) * i;
                float posY = ToVisualY(barArea, depth);

                // Hide height string when close to selection limits
                float distanceToSelectionLimit0 = Math.Abs(posY - selectedLimit0PosY);
                float distanceToSelectionLimit1 = Math.Abs(posY - selectedLimit1PosY);
                float distance = Math.Min(distanceToSelectionLimit0, distanceToSelectionLimit1) - (_heightStringFont.Height * 0.85f);

                if (distance > _heightStringFadeDistance)
                {
                    DrawHeightString(e, barArea, _outlinePen, (float)Math.Round(depth));

                    if (i > 0 && i < _heightStringCount)
                        e.Graphics.DrawLine(_heightLinesBigPen, barArea.Left, posY, barArea.Right, posY);
                }
                else if (distance > 0.0f)
                {
                    using (var alphaOutlinePen = (Pen)_outlinePen.Clone())
                    {
                        alphaOutlinePen.Color = Color.FromArgb((int)(alphaOutlinePen.Color.A * (distance / _heightStringFadeDistance)),
                            alphaOutlinePen.Color.R, alphaOutlinePen.Color.G, alphaOutlinePen.Color.B);
                        DrawHeightString(e, barArea, alphaOutlinePen, (float)Math.Round(depth));
                    }
                    if (i > 0 && i < _heightStringCount)
                        using (var alphaPen = (Pen)_heightLinesBigPen.Clone())
                        {
                            alphaPen.Color = Color.FromArgb((int)(alphaPen.Color.A * (distance / _heightStringFadeDistance)),
                                alphaPen.Color.R, alphaPen.Color.G, alphaPen.Color.B);
                            e.Graphics.DrawLine(alphaPen, barArea.Left, posY, barArea.Right, posY);
                        }
                }
            }

            // Draw common selection range
            DrawHeightString(e, barArea, _selectionPen, _selectedLimit0, true);
            DrawHeightString(e, barArea, _selectionPen, _selectedLimit1, true);

            RectangleF selectionRect = new RectangleF(new PointF(barArea.Left, Math.Min(selectedLimit1PosY, selectedLimit0PosY)),
                new SizeF(_barWidth, Math.Abs(selectedLimit0PosY - selectedLimit1PosY)));
            e.Graphics.FillRectangle(_selectionBrush, selectionRect);

            // Draw probe selection ranges
            if (!barArea.Contains(e.ClipRectangle))
                for (int groupIndex = 0; groupIndex < DepthProbes.Count; ++groupIndex)
                {
                    RectangleF groupArea = groupGetArea(barArea, groupIndex);
                    selectionRect = new RectangleF(new PointF(groupArea.Left, Math.Min(selectedLimit1PosY, selectedLimit0PosY)),
                        new SizeF(groupArea.Width, Math.Abs(selectedLimit0PosY - selectedLimit1PosY)));

                    using (var b = new SolidBrush(Color.FromArgb(60, DepthProbes[groupIndex].Color.R, DepthProbes[groupIndex].Color.G, DepthProbes[groupIndex].Color.B)))
                        e.Graphics.FillRectangle(b, selectionRect);
                }

            // Draw depth bar content
            if (barArea.IntersectsWith(e.ClipRectangle))
            {
                // Draw group
                for (int groupIndex = 0; groupIndex < GroupCount; ++groupIndex)
                {
                    RectangleF groupArea = groupGetArea(barArea, groupIndex);
                    if (!groupArea.IntersectsWith(e.ClipRectangle))
                        continue;

                    // Draw sequences
                    List<List<RelevantRoom>> roomSequences = groupBuildRoomSequences(level, cursorPos, groupIndex);
                    float sequenceWidth = groupArea.Width / roomSequences.Count;
                    for (int roomSequenceIndex = 0; roomSequenceIndex < roomSequences.Count; ++roomSequenceIndex)
                    {
                        var roomSequence = roomSequences[roomSequenceIndex];
                        float posX0 = groupArea.X + sequenceWidth * roomSequenceIndex;
                        float posX1 = groupArea.X + sequenceWidth * (roomSequenceIndex + 1);
                        for (int i = 0; i < roomSequence.Count; ++i)
                        {
                            RelevantRoom room = roomSequence[i];
                            float posY0 = ToVisualY(groupArea, room.MaxDepth);
                            float posY1 = ToVisualY(groupArea, room.MinDepth);

                            // Draw fill color for room
                            Brush colorBrush = _roomsNormalBrush;
                            if ((room.Block != null) && (room.Block.Type != BlockType.Floor))
                                colorBrush = _roomsWallBrush;
                            using (var colorBrush2 = getRoomBrush(room.Room, colorBrush))
                                e.Graphics.FillRectangle(colorBrush2, posX0, posY0, posX1 - posX0, posY1 - posY0);
                            if (!CheckRoom(room.MinDepth, room.MaxDepth))
                                e.Graphics.FillRectangle(_roomsOutsideOverdraw, posX0, posY0, posX1 - posX0, posY1 - posY0);

                            // Find portals on the selected sector
                            Pen belowPen = _roomBoundsPen;
                            if ((room.Block != null) && (room.Block.FloorPortal != null))
                            {
                                Room portalRoom = room.Block.FloorPortal.AdjoiningRoom;
                                if (((i - 1) >= 0) && (roomSequence[i - 1].Room == portalRoom))
                                    belowPen = _portalPen;
                            }
                            Pen abovePen = _roomBoundsPen;
                            if ((room.Block != null) && (room.Block.CeilingPortal != null))
                            {
                                Room portalRoom = room.Block.CeilingPortal.AdjoiningRoom;
                                if (((i + 1) < roomSequence.Count) && (roomSequence[i + 1].Room == portalRoom))
                                    abovePen = _portalPen;
                            }

                            //Draw room borders
                            e.Graphics.DrawLine(belowPen, posX0, posY1, posX1, posY1);
                            e.Graphics.DrawLine(abovePen, posX0, posY0, posX1, posY0);
                            e.Graphics.DrawLine(_sequenceSeperatorPen, posX0, posY0, posX0, posY1);
                            e.Graphics.DrawLine(_sequenceSeperatorPen, posX1, posY0, posX1, posY1);
                        }
                    }
                }
            }

            // Draw outline around the groups
            for (int groupIndex = 0; groupIndex < GroupCount; ++groupIndex)
            {
                RectangleF groupArea = groupGetArea(barArea, groupIndex);
                e.Graphics.DrawRectangle(_outlinePen, groupArea);
            }
        }

        private static void DrawHeightString(PaintEventArgs e, RectangleF barArea, Pen pen, float depth, bool selection = false)
        {
            if (barArea.Contains(e.ClipRectangle))
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            float screenPosY = ToVisualY(barArea, depth);

            if (selection)
            {
                PointF[] heightPolyPoints = new PointF[]
                {
                    new PointF(barArea.X - _heightStringArrowSize - 4, screenPosY + _heightStringArrowSize / 1.5f),
                    new PointF(barArea.X - _heightStringArrowSize - 4, screenPosY - _heightStringArrowSize / 1.5f),
                    new PointF(barArea.X - 4, screenPosY)
                };
                e.Graphics.DrawPolygon(pen, heightPolyPoints);
            }
            else
                e.Graphics.DrawLine(pen, barArea.X, screenPosY, barArea.X - _heightStringLineLength, screenPosY);

            RectangleF textArea = new RectangleF(0.0f, screenPosY - _heightStringFont.Height,
                barArea.X - (_heightStringLineDistance + _heightStringLineLength), _heightStringFont.Height * 2);
            string text = string.Format((selection ? "y = {0:F0}" : "{0:F0}"), depth);
            e.Graphics.DrawString(text, _heightStringFont, pen.Brush, textArea, _heightStringLayout);

            e.Graphics.SmoothingMode = SmoothingMode.Default;
        }

        private int GroupCount
        {
            get { return DepthProbes.Count + 1; }
        }

        public RectangleF groupGetArea(RectangleF barArea, int groupIndex)
        {
            return new RectangleF(barArea.Right - (groupIndex + 1) * _barWidth, barArea.Y, _barWidth, barArea.Height);
        }

        private List<List<RelevantRoom>> groupBuildRoomSequences(Level level, Vector2 cursorPos, int groupIndex)
        {
            // Decide what bar is at the given index
            Vector2 probePos = groupIndex == DepthProbes.Count ? cursorPos : DepthProbes[groupIndex].Position;
            bool shouldCheckRoomsToMove = (groupIndex == DepthProbes.Count) && (_roomsToMove != null);

            // Iterate over all rooms under the curser and add them to the room sequences
            IEnumerable<Room> sortedRoomList = level.GetVerticallyAscendingRoomList(room =>
            {
                Vector2 roomLocal = probePos - room.SectorPos.ToVec2();
                bool CollidesWithProbe = (roomLocal.X >= 1) && (roomLocal.Y >= 1) && (roomLocal.X < (room.NumXSectors - 1)) && (roomLocal.Y < (room.NumZSectors - 1));
                return (shouldCheckRoomsToMove ? _roomsToMove.Contains(room) : CollidesWithProbe);
            });

            var roomSequences = new List<List<RelevantRoom>>();
            foreach (Room room in sortedRoomList)
            {
                Vector2 roomLocal = probePos - room.SectorPos.ToVec2();
                bool CollidesWithProbe = (roomLocal.X >= 1) && (roomLocal.Y >= 1) && (roomLocal.X < (room.NumXSectors - 1)) && (roomLocal.Y < (room.NumZSectors - 1));

                Block block = CollidesWithProbe ? room.Blocks[(int)(roomLocal.X), (int)(roomLocal.Y)] : null;
                RelevantRoom relevantRoom = new RelevantRoom
                {
                    Room = room,
                    Block = block,
                    MinDepth = room.Position.Y + room.GetLowestCorner(),
                    MaxDepth = room.Position.Y + room.GetHighestCorner()
                };

                // Search for a fit in the sequence for rooms it the current room is connected to on this sector
                if ((block != null) && (block.FloorPortal != null))
                {
                    var portal = block.FloorPortal;
                    var roomAbove = portal.AdjoiningRoom;
                    foreach (var roomSequence in roomSequences)
                        if (roomSequence.Last().Room == roomAbove)
                        {
                            roomSequence.Add(relevantRoom);
                            goto AddedRoomSucessfully;
                        }
                }
                roomSequences.Add(new List<RelevantRoom>());
                roomSequences.Last().Add(relevantRoom);
                AddedRoomSucessfully:
                ;
            }

            // Also try moving rooms into the same sequence that are connected directly, just not on the square that is selected
            for (int i = 1; i < roomSequences.Count; ++i) // triangular iteration
            {
                var roomSequenceAbove = roomSequences[i];
                foreach (var portal in roomSequenceAbove[0].Room.Portals)
                {
                    var connectedRoom = portal.AdjoiningRoom;

                    for (int j = 0; j < i; ++j)
                    {
                        var roomSequenceBelow = roomSequences[j];
                        if (roomSequences[j].Last().Room != connectedRoom)
                            continue;

                        float distanceBetweenSequences = roomSequenceAbove[0].MinDepth - roomSequenceBelow.Last().MaxDepth;
                        if (!(distanceBetweenSequences >= 0.0))
                            continue;

                        roomSequences[j].AddRange(roomSequences[i]);
                        roomSequences.RemoveAt(i);
                        --i;
                        goto NextRoom;
                    }
                }
                NextRoom:
                ;
            }

            // Try to merge independent sequences if they are sufficently far apart
            for (int i = 0; i < roomSequences.Count; ++i)
            {
                var roomSequenceAbove = roomSequences[i];
                for (int j = 0; j < roomSequences.Count; ++j)
                {
                    var roomSequenceBelow = roomSequences[j];
                    float distanceBetweenSequences = roomSequenceAbove[0].MinDepth - roomSequenceBelow.Last().MaxDepth;
                    if (distanceBetweenSequences >= _minDepthDifferenceBetweenIndependentlyMergedSequences)
                    {
                        roomSequenceBelow.AddRange(roomSequenceAbove);
                        roomSequences.RemoveAt(i);
                        --i;
                        goto NextRoom;
                    }
                }
                NextRoom:
                ;
            }

            return roomSequences;
        }

        public float SelectedMin => Math.Min(_selectedLimit0, _selectedLimit1);
        public float SelectedMax => Math.Max(_selectedLimit0, _selectedLimit1);
        public HashSet<Room> RoomsToMove => _roomsToMove;
        public bool CheckRoom(float roomMinDepth, float roomMaxDepth) => (roomMinDepth >= SelectedMin) && (roomMaxDepth <= SelectedMax);
        public bool CheckRoom(Room room) => CheckRoom(room.Position.Y + room.GetLowestCorner(), room.Position.Y + room.GetHighestCorner());
    };
}
