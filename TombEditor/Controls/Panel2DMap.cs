using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using TombEditor.Geometry;
using SharpDX;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace TombEditor.Controls
{
    public class Panel2DMap : Panel
    {
        private Editor _editor;
        public float DeltaX { get; set; }
        public float DeltaY { get; set; }
        public float LastX { get; set; }
        public float LastY { get; set; }
        public bool Drag { get; set; }
        private Bitmap _selectionBuffer;
        private Graphics _graphics;
        private List<int> _roomsToMove;

        public Panel2DMap()
        {
            _editor = Editor.Instance;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            _editor.RoomIndex = -1;
        }

        public void InitializePanel()
        {
            if (Width != 0 && Height != 0)
            {
                if (_selectionBuffer != null)
                    _selectionBuffer.Dispose();
                if (_graphics != null)
                    _graphics.Dispose();
                _selectionBuffer = new Bitmap(Width, Height);
                _graphics = Graphics.FromImage(_selectionBuffer);
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            InitializePanel();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                if (!Drag)
                    Drag = true;

                LastX = e.X / 4;
                LastY = e.Y / 4;

                short roomIndex = (short)DoPicking(e.X, e.Y);
                if (roomIndex == -1)
                {
                    Drag = false;
                    return;
                }

                _editor.RoomIndex = roomIndex;
                _editor.RoomIndex = _editor.RoomIndex;
                _editor.SelectRoom(roomIndex);
                _editor.UpdateRoomName();
                _editor.UpdateStatistics();

                _roomsToMove = new List<int>();
                ResetRoomsVisited();
                CollectRoomsToMove(_editor.RoomIndex, (int)DeltaX, (int)DeltaY);
            }

            Invalidate();
            _editor.DrawPanelGrid();
            _editor.UpdateStatistics();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left && Drag)
            {
                Room room = _editor.Level.Rooms[_editor.RoomIndex];

                int tempX = (int)(room.Position.X + e.X / 4 - LastX);
                int tempY = (int)(room.Position.Z - e.Y / 4 + LastY);

                if (_editor.RoomIndex < 0 || _editor.RoomIndex > 2047 || _editor.Level.Rooms[_editor.RoomIndex] == null ||
                    tempX < 0 || tempY < 0 || tempX > 160 || tempY > 160)
                {
                    Drag = false;
                    return;
                }

                DeltaX = e.X / 4 - LastX;
                DeltaY = e.Y / 4 - LastY;

                LastX = e.X / 4;
                LastY = e.Y / 4;

                MoveRooms((int)DeltaX, (int)DeltaY);
                //ResetRoomsVisited();
                //MoveRoomRecursive(_editor.RoomIndex, (int)DeltaX, (int)DeltaY);
            }

            Invalidate();
            _editor.DrawPanelGrid();
            _editor.UpdateStatistics();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left && Drag)
            {
                Room room = _editor.Level.Rooms[_editor.RoomIndex];

                int tempX = (int)(room.Position.X + e.X / 4 - LastX);
                int tempY = (int)(room.Position.Z - e.Y / 4 + LastY);

                if (_editor.RoomIndex < 0 || _editor.RoomIndex > 2047 || _editor.Level.Rooms[_editor.RoomIndex] == null ||
                    tempX < 0 || tempY < 0 || tempX > 160 || tempY > 160)
                {
                    Drag = false;
                    return;
                }

                DeltaX = e.X / 4 - LastX;
                DeltaY = e.Y / 4 - LastY;

                LastX = e.X / 4;
                LastY = e.Y / 4;

                MoveRooms((int)DeltaX, (int)DeltaY);
                //ResetRoomsVisited();
                //MoveRoomRecursive(_editor.RoomIndex, (int)DeltaX, (int)DeltaY);
            }

            Drag = false;
            Invalidate();
            _editor.DrawPanelGrid();
            _editor.CenterCamera();
            _editor.UpdateStatistics();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // disegno le linee nere della griglia           
            for (int x = 0; x < Width; x += 8)
            {
                g.DrawLine(Pens.LightGray, new System.Drawing.Point(x, 0), new System.Drawing.Point(x, Height));
            }

            for (int y = 0; y < Height; y += 8)
            {
                g.DrawLine(Pens.LightGray, new System.Drawing.Point(0, y), new System.Drawing.Point(Width, y));
            }

            if (_editor == null)
                return;
            if (_editor.Level == null)
                return;

            // per prima cosa ordino le stanze in modo da disegnarle dalla più alta alla più bassa
            List<int> roomList = new List<int>();
            List<int> heightList = new List<int>();

            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] != null)
                {
                    roomList.Add(i);
                    heightList.Add((int)_editor.Level.Rooms[i].Position.Y + _editor.Level.Rooms[i].GetHighestCorner());
                }
            }

            for (int j = 0; j < (roomList.Count - 1); j++)
                for (int i = 0; i < (roomList.Count - 1); i++)
                    if (heightList[i] > heightList[i + 1])
                    {
                        int temp = heightList[i];
                        heightList[i] = heightList[i + 1];
                        heightList[i + 1] = temp;
                        temp = roomList[i];
                        roomList[i] = roomList[i + 1];
                        roomList[i + 1] = temp;
                    }

            // disegno le stanze una per una
            Room room;
            Brush brush;

            for (int i = 0; i < roomList.Count; i++)
            {
                room = _editor.Level.Rooms[roomList[i]];
                if (room == null)
                    continue;

                if (roomList[i] == _editor.RoomIndex)
                    brush = Brushes.Red;
                else
                    brush = Brushes.Cyan;
                if (!Drag || roomList[i] == _editor.RoomIndex)
                    g.FillRectangle(brush, new Rectangle((int)(room.Position.X + 1) * 4, (Height / 4 - (int)(room.Position.Z + 1) - room.NumZSectors + 3) * 4, (room.NumXSectors - 2) * 4, (room.NumZSectors - 2) * 4));
                g.DrawRectangle(Pens.Black, new Rectangle((int)(room.Position.X + 1) * 4, (Height / 4 - (int)(room.Position.Z + 1) - room.NumZSectors + 3) * 4, (room.NumXSectors - 2) * 4, (room.NumZSectors - 2) * 4));
            }

            _editor.UpdateStatistics();
        }

        private int DoPicking(int x, int y)
        {
            // per prima cosa ordino le stanze in modo da disegnarle dalla più alta alla più bassa
            List<int> roomList = new List<int>();
            List<int> heightList = new List<int>();

            for (int i = 0; i < Editor.MaxNumberOfRooms; i++)
            {
                if (_editor.Level.Rooms[i] != null)
                {
                    roomList.Add(i);
                    heightList.Add((int)_editor.Level.Rooms[i].Position.Y + _editor.Level.Rooms[i].GetHighestCorner());
                }
            }

            for (int j = 0; j < (roomList.Count - 1); j++)
                for (int i = 0; i < (roomList.Count - 1); i++)
                    if (heightList[i] > heightList[i + 1])
                    {
                        int temp = heightList[i];
                        heightList[i] = heightList[i + 1];
                        heightList[i + 1] = temp;
                        temp = roomList[i];
                        roomList[i] = roomList[i + 1];
                        roomList[i + 1] = temp;
                    }

            // pulisco il buffer
            _graphics.Clear(Color.White);

            // disegno le stanze una per una
            Room room;
            Brush brush;

            for (int i = 0; i < roomList.Count; i++)
            {
                room = _editor.Level.Rooms[roomList[i]];
                brush = new SolidBrush(Color.FromArgb((0xff << 24) + roomList[i]));
                _graphics.FillRectangle(brush, new Rectangle((int)(room.Position.X + 1) * 4, (Height / 4 - (int)(room.Position.Z + 1) - room.NumZSectors + 3) * 4, (room.NumXSectors - 2) * 4, (room.NumZSectors - 2) * 4));
            }

            // recupero l'id della stanza in base al colore
            Color pickColor = _selectionBuffer.GetPixel(x, y);
            int roomId = (pickColor.G << 8) + pickColor.B;
            if (roomId == 65535)
                roomId = -1;

            return roomId;
        }

        public void MoveRoomRecursive(int room, int deltaX, int deltaY)
        {
            _editor.Level.Rooms[room].Visited = true;

            for (int i = 0; i < _editor.Level.Portals.Count; i++)
            {
                Portal p = _editor.Level.Portals.ElementAt(i).Value;

                if (p.Room == room)
                {
                    if (_editor.Level.Rooms[p.AdjoiningRoom].Visited)
                        continue;
                    MoveRoomRecursive(p.AdjoiningRoom, deltaX, deltaY);
                }
            }

            _editor.Level.Rooms[room].Position += new Vector3(deltaX, 0, -deltaY);
        }

        public void MoveRooms(int deltaX, int deltaY)
        {
            for (int i = 0; i < _roomsToMove.Count; i++)
            {
                _editor.Level.Rooms[_roomsToMove[i]].Position += new Vector3(deltaX, 0, -deltaY);
            }
        }

        public void CollectRoomsToMove(int room, int deltaX, int deltaY)
        {
            _editor.Level.Rooms[room].Visited = true;

            for (int i = 0; i < _editor.Level.Portals.Count; i++)
            {
                Portal p = _editor.Level.Portals.ElementAt(i).Value;

                if (p.Room == room)
                {
                    if (_editor.Level.Rooms[p.AdjoiningRoom].Visited)
                        continue;
                    CollectRoomsToMove(p.AdjoiningRoom, deltaX, deltaY);
                }
            }

            _roomsToMove.Add(room);

            // Add also the flipped room
            if (_editor.Level.Rooms[room].Flipped && _editor.Level.Rooms[room].AlternateRoom != -1 &&
                !_editor.Level.Rooms[_editor.Level.Rooms[room].AlternateRoom].Visited)
            {
                _roomsToMove.Add(_editor.Level.Rooms[room].AlternateRoom);
            }
        }

        private void ResetRoomsVisited()
        {
            for (int i = 0; i < Editor.MaxNumberOfRooms; i++)
            {
                if (_editor.Level.Rooms[i] != null)
                {
                    _editor.Level.Rooms[i].Visited = false;
                }
            }
        }
    }
}
