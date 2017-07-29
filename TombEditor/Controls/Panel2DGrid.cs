using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombLib.Graphics;
using TombEditor.Geometry;
using System.Drawing;
using System.ComponentModel;

namespace TombEditor.Controls
{
    public class Panel2DGrid : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedTrigger { get; set; } = -1;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedPortal { get; set; } = -1;

        private Editor _editor;
        private bool _firstSelection;
        private float _deltaX;
        private float _deltaY;
        private float _lastX;
        private float _lastY;
        private int _startX;
        private int _startY;
        private bool _drag;
        private static readonly Brush _floorBrush = new SolidBrush(Editor.ColorFloor);
        private static readonly Brush _wallBrush = new SolidBrush(Editor.ColorWall);
        private static readonly Brush _deathBrush = new SolidBrush(Editor.ColorDeath);
        private static readonly Brush _monkeyBrush = new SolidBrush(Editor.ColorMonkey);
        private static readonly Brush _boxBrush = new SolidBrush(Editor.ColorBox);
        private static readonly Brush _notWalkableBrush = new SolidBrush(Editor.ColorNotWalkable);
        private static readonly Pen _beetlePen = new Pen(Color.FromArgb(100, 100, 100), 4);
        private static readonly Pen _triggerTriggererPen = new Pen(Color.FromArgb(0, 0, 252), 4);
        private static readonly Brush _noCollisionBrush = new SolidBrush(Editor.ColorNoCollision);
        private static readonly Brush _triggerBrush = new SolidBrush(Editor.ColorTrigger);
        private static readonly Pen _climbPen = new Pen(Editor.ColorClimb, 4);
        private static readonly Font _font = new Font("Arial", 8);

        private const byte GridStep = 11;
        
        public Panel2DGrid()
        {
            _editor = Editor.Instance;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
        
        private int GetZBlock(float z)
        {
            Room currentRoom = _editor.SelectedRoom;
            int numXblocks = currentRoom.NumXSectors;
            int numZblocks = currentRoom.NumZSectors;

            for (int i = 0; i < 20; i++)
                if (z >= i && z < (i + 1) * GridStep)
                    if (numZblocks % 2 != 0)
                        return (18 - i);
                    else
                        return (19 - i);

            return 0;
        }

        private int getStartX()
        {
            Room currentRoom = _editor.SelectedRoom;
            int numXblocks = currentRoom.NumXSectors;
            int numZblocks = currentRoom.NumZSectors;

            int startX = (int)(20 - numXblocks) / 2;
            //if (numXblocks % 2 != 0) startX++;

            return startX;
        }

        private int getStartZ()
        {
            Room currentRoom = _editor.SelectedRoom;
            int numXblocks = currentRoom.NumXSectors;
            int numZblocks = currentRoom.NumZSectors;

            int startZ = (int)(20 - numZblocks) / 2;
            // if (numZblocks % 2 != 0) startZ++;

            return startZ;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (_editor == null)
                return;
            if (_editor.SelectedRoom == null)
                return;

            _editor.ResetPanel2DMessage();

            Room currentRoom = _editor.SelectedRoom;
            int numXblocks = currentRoom.NumXSectors;
            int numZblocks = currentRoom.NumZSectors;

            int startX = getStartX(); // (20 - numXblocks) / 2;
            int startY = getStartZ(); // (20 - numZblocks) / 2;

            _startX = startX;
            _startY = startY;

            // gestisco il drag & drop
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                _lastX = e.X;
                _lastY = e.Y;

                int xBlock = (int)_lastX / GridStep;
                int zBlock = GetZBlock(_lastY); // 19 - (int)(LastY) / GridStep;

                bool foundSomething = false;

                if (SelectedTrigger == -1)
                {
                    // inizio il ciclo dal primo portale
                    SelectedPortal = GetNextPortal((int)_lastX / GridStep - startX, GetZBlock(_lastY) - startY);
                    if (SelectedPortal == -1)
                    {
                        SelectedTrigger = GetNextTrigger((int)_lastX / GridStep - startX, GetZBlock(_lastY) - startY);
                        if (SelectedTrigger != -1)
                        {
                            foundSomething = true;

                            PickingResult result = new PickingResult();
                            result._elementType = PickingElementType.Trigger;
                            result._element = SelectedTrigger;
                            _editor.PickingResult = result;
                        }
                    }
                    else
                    {
                        foundSomething = true;

                        PickingResult result = new PickingResult();
                        result._elementType = PickingElementType.Portal;
                        result._element = SelectedPortal;
                        _editor.PickingResult = result;
                    }
                }
                else
                {
                    if (SelectedPortal == -1 && SelectedTrigger != -1)
                    {
                        SelectedTrigger = GetNextTrigger((int)_lastX / GridStep - startX, GetZBlock(_lastY) - startY);
                        if (SelectedTrigger == -1)
                        {
                            SelectedPortal = GetNextPortal((int)_lastX / GridStep - startX, GetZBlock(_lastY) - startY);
                            if (SelectedPortal != -1)
                            {
                                foundSomething = true;

                                PickingResult result = new PickingResult();
                                result._elementType = PickingElementType.Portal;
                                result._element = SelectedPortal;
                                _editor.PickingResult = result;
                            }
                        }
                        else
                        {
                            foundSomething = true;

                            PickingResult result = new PickingResult();
                            result._elementType = PickingElementType.Trigger;
                            result._element = SelectedTrigger;
                            _editor.PickingResult = result;
                        }
                    }
                }

                if (!foundSomething)
                {
                    SelectedPortal = -1;
                    SelectedTrigger = -1;
                }
            }
            else
            {
                if (SelectedPortal == -1)
                {
                    if (!_drag)
                        _drag = true;
                    _lastX = e.X;
                    _lastY = e.Y;

                    int xBlock = (int)_lastX / GridStep;
                    int zBlock = GetZBlock(_lastY);

                    _firstSelection = false;

                    // se qualcuno dei valori X o Z è pari a -1 allora è una prima selezione
                    if (_editor.BlockSelectionStartX == -1 || _editor.BlockSelectionStartZ == -1)
                    {
                        _editor.BlockSelectionStartX = xBlock;
                        _editor.BlockSelectionStartZ = zBlock;
                        _editor.BlockSelectionEndX = _editor.BlockSelectionStartX;
                        _editor.BlockSelectionEndZ = _editor.BlockSelectionStartZ;
                        _firstSelection = true;
                        _editor.BlockEditingType = 0;
                    }
                    else
                    {
                        int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                        int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                        int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
                        int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

                        if (xBlock >= xMin && xBlock <= xMax && xBlock != -1 && zBlock >= zMin && zBlock <= zMax)
                        {
                            // non è una prima selezione perchè sto facendo click su un intervallo già selezionato
                            _firstSelection = false;
                        }
                        else
                        {
                            _editor.BlockSelectionStartX = xBlock - startX;
                            _editor.BlockSelectionStartZ = zBlock - startY;
                            _editor.BlockSelectionEndX = _editor.BlockSelectionStartX;
                            _editor.BlockSelectionEndZ = _editor.BlockSelectionStartZ;
                            _firstSelection = true;
                            _editor.BlockEditingType = 0;
                        }
                    }

                    PickingResult result = new PickingResult();
                    result._elementType = PickingElementType.Block;
                    result._subElementType = 0;
                    result._element = (xBlock << 5) + (zBlock & 31);
                    result._subElementType = 25;

                    _editor.PickingResult = result;
                    _editor.StartPickingResult = result;

                    SelectedTrigger = -1;
                    SelectedPortal = -1;
                }
            }

            _editor.DrawPanel3D();
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_editor == null)
                return;
            if (_editor.SelectedRoom == null)
                return;

            Room currentRoom = _editor.SelectedRoom;
            int numXblocks = currentRoom.NumXSectors;
            int numZblocks = currentRoom.NumZSectors;

            int startX = getStartX(); // (20 - numXblocks) / 2;
            int startY = getStartZ(); // (20 - numZblocks) / 2;

            _startX = startX;
            _startY = startY;
            
            // manage the drag & drop camera
            if (_drag && e.Button == MouseButtons.Right)
            {
                _deltaX = e.X - _lastX;
                _deltaY = e.Y - _lastY;
            }
            else
            {
                if (SelectedPortal == -1)
                {
                    if (_drag)
                    {
                        _lastX = e.X;
                        _lastY = e.Y;

                        if (_lastY < 0)
                            _lastY = 0;

                        int xBlock = (int)_lastX / GridStep;
                        int zBlock = GetZBlock(_lastY);

                        _editor.BlockSelectionStartX = (_editor.StartPickingResult._element >> 5) - startX;
                        _editor.BlockSelectionStartZ = (_editor.StartPickingResult._element & 31) - startY;
                        _editor.BlockEditingType = 0;
                        _editor.BlockSelectionEndX = xBlock - startX;
                        _editor.BlockSelectionEndZ = zBlock - startY;

                        if (_editor.BlockSelectionEndX < 0)
                            _editor.BlockSelectionEndX = 0;
                        if (_editor.BlockSelectionEndZ < 0)
                            _editor.BlockSelectionEndZ = 0;
                        if (_editor.BlockSelectionEndX > _editor.SelectedRoom.NumXSectors - 1)
                            _editor.BlockSelectionEndX = _editor.SelectedRoom.NumXSectors - 1;
                        if (_editor.BlockSelectionEndZ > _editor.SelectedRoom.NumZSectors - 1)
                            _editor.BlockSelectionEndZ = _editor.SelectedRoom.NumZSectors - 1;

                        _firstSelection = true;

                        PickingResult result = new PickingResult();
                        result._elementType = PickingElementType.Block;
                        result._subElementType = 0;
                        result._element = (xBlock << 5) + (zBlock & 31);
                        result._subElementType = 25;

                        _editor.PickingResult = result;
                    }
                }
            }

            _editor.DrawPanel3D();
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_editor == null)
                return;
            if (_editor.SelectedRoom == null)
                return;

            Room currentRoom = _editor.SelectedRoom;
            int numXblocks = currentRoom.NumXSectors;
            int numZblocks = currentRoom.NumZSectors;

            int startX = getStartX();
            int startY = getStartZ();

            _startX = startX;
            _startY = startY;

            // manage the drag & drop camera
            if (e.Button == MouseButtons.Right)
            {
                _deltaX = e.X - _lastX;
                _deltaY = e.Y - _lastY;
            }
            else
            {
                if (SelectedPortal == -1)
                {
                    if (_drag)
                    {
                        _lastX = e.X;
                        _lastY = e.Y;

                        if (_lastY < 0)
                            _lastY = 0;

                        int xBlock = (int)_lastX / GridStep;
                        int zBlock = GetZBlock(_lastY);// 20 - (int)(LastY) / GridStep;

                        if (_firstSelection)
                        {
                            _editor.BlockSelectionEndX = xBlock - startX;
                            _editor.BlockSelectionEndZ = zBlock - startY;

                            if (_editor.BlockSelectionEndX < 0)
                                _editor.BlockSelectionEndX = 0;
                            if (_editor.BlockSelectionEndZ < 0)
                                _editor.BlockSelectionEndZ = 0;
                            if (_editor.BlockSelectionEndX > _editor.SelectedRoom.NumXSectors - 1)
                                _editor.BlockSelectionEndX = _editor.SelectedRoom.NumXSectors - 1;
                            if (_editor.BlockSelectionEndZ > _editor.SelectedRoom.NumZSectors - 1)
                                _editor.BlockSelectionEndZ = _editor.SelectedRoom.NumZSectors - 1;

                            _editor.LoadTriggersInUI();

                            _editor.BlockEditingType = 0;
                        }

                        PickingResult result = new PickingResult();
                        result._elementType = PickingElementType.Block;
                        result._subElementType = 0;
                        result._element = (xBlock << 5) + (zBlock & 31);
                        result._subElementType = 25;

                        _editor.PickingResult = result;
                    }
                }
                else
                {
                    _lastX = e.X;
                    _lastY = e.Y;

                    int xBlock = (int)_lastX / GridStep - startX;
                    int zBlock = GetZBlock(_lastY) - startY;

                    Portal p = _editor.Level.Portals[SelectedPortal];

                    if (xBlock >= p.X && xBlock <= p.X + p.NumXBlocks && zBlock >= p.Z && zBlock <= p.Z + p.NumZBlocks)
                    {
                        _editor.SelectRoom(p.AdjoiningRoom);
                        _editor.ResetSelection();
                        _editor.ResetCamera();
                    }
                    else
                    {
                        SelectedPortal = -1;
                        Invalidate();
                    }

                    SelectedPortal = -1;
                    SelectedTrigger = -1;
                }
            }

            _drag = false;

            _editor.DrawPanel3D();
            Invalidate();
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;

                g.FillRectangle(Brushes.White, new Rectangle(0, 0, 320, 320));

                if (_editor == null)
                    return;
                if (_editor.SelectedRoom == null)
                    return;

                Room currentRoom = _editor.SelectedRoom;
                int numXblocks = currentRoom.NumXSectors;
                int numZblocks = currentRoom.NumZSectors;

                int startX = getStartX(); // (20 - numXblocks) / 2;
                int startY = getStartZ(); // (20 - numZblocks) / 2;

                _startX = startX;
                _startY = startY;

                // Console.WriteLine("X: " + StartX + ", Y: " + StartY);

                // disegno i confini della stanza
                g.FillRectangle(Brushes.Gray, new Rectangle(startX * GridStep, (startY + numZblocks - 1) * GridStep, numXblocks * GridStep, GridStep));
                g.FillRectangle(Brushes.Gray, new Rectangle(startX * GridStep, startY * GridStep, GridStep, GridStep * numZblocks));
                g.FillRectangle(Brushes.Gray, new Rectangle(startX * GridStep, startY * GridStep, numXblocks * GridStep, GridStep));
                g.FillRectangle(Brushes.Gray, new Rectangle((startX + numXblocks - 1) * GridStep, startY * GridStep, GridStep, GridStep * numZblocks));

                // disegno il pavimento
                g.FillRectangle(_floorBrush, new Rectangle((startX + 1) * GridStep, (startY + 1) * GridStep, (numXblocks - 2) * GridStep, (numZblocks - 2) * GridStep));

                int d = (numZblocks % 2 != 0 ? 1 : 0);

                // disegno i muri verdi
                for (int x = 0; x < numXblocks; x++)
                {
                    for (int z = 0; z < numZblocks; z++)
                    {
                        if ((currentRoom.Blocks[x, z].Flags & BlockFlags.Death) != 0)
                            g.FillRectangle(_deathBrush, new Rectangle((startX + x) * GridStep, (19 - (startY + z) - d) * GridStep, GridStep, GridStep));

                        if ((currentRoom.Blocks[x, z].Flags & BlockFlags.Electricity) != 0)
                            g.FillRectangle(_deathBrush, new Rectangle((startX + x) * GridStep, (19 - (startY + z) - d) * GridStep, GridStep, GridStep));

                        if ((currentRoom.Blocks[x, z].Flags & BlockFlags.Lava) != 0)
                            g.FillRectangle(_deathBrush, new Rectangle((startX + x) * GridStep, (19 - (startY + z) - d) * GridStep, GridStep, GridStep));

                        if ((currentRoom.Blocks[x, z].Flags & BlockFlags.Monkey) != 0)
                            g.FillRectangle(_monkeyBrush, new Rectangle((startX + x) * GridStep, (19 - (startY + z) - d) * GridStep, GridStep, GridStep));

                        if ((currentRoom.Blocks[x, z].Flags & BlockFlags.Box) != 0)
                            g.FillRectangle(_boxBrush, new Rectangle((startX + x) * GridStep, (19 - (startY + z) - d) * GridStep, GridStep, GridStep));

                        if ((currentRoom.Blocks[x, z].Flags & BlockFlags.NotWalkableFloor) != 0)
                            g.FillRectangle(_notWalkableBrush, new Rectangle((startX + x) * GridStep, (19 - (startY + z) - d) * GridStep, GridStep, GridStep));

                        if (currentRoom.Blocks[x, z].Type == BlockType.Wall)
                            g.FillRectangle(_wallBrush, new Rectangle((startX + x) * GridStep, (19 - (startY + z) - d) * GridStep, GridStep, GridStep));

                        if ((currentRoom.Blocks[x, z].FloorPortal != -1 && !currentRoom.Blocks[x, z].IsFloorSolid) || currentRoom.Blocks[x, z].CeilingPortal != -1 || currentRoom.Blocks[x, z].WallPortal != -1)
                            g.FillRectangle(Brushes.Black, new Rectangle((startX + x) * GridStep, (19 - (startY + z) - d) * GridStep, GridStep, GridStep));

                        if (currentRoom.Blocks[x, z].Triggers.Count != 0)
                            g.FillRectangle(_triggerBrush, new Rectangle((startX + x) * GridStep, (19 - (startY + z) - d) * GridStep, GridStep, GridStep));

                        if (currentRoom.Blocks[x, z].NoCollisionFloor || currentRoom.Blocks[x, z].NoCollisionCeiling)
                            g.FillRectangle(_noCollisionBrush, new Rectangle((startX + x) * GridStep, (19 - (startY + z) - d) * GridStep, GridStep, GridStep));

                        if (currentRoom.Blocks[x, z].Climb[0])
                            g.DrawLine(_climbPen, new PointF((startX + x) * GridStep, (19 - (startY + z - 3 / GridStep) - d) * GridStep), new PointF((startX + x + 1) * GridStep, (19 - (startY + z - 3 / GridStep) - d) * GridStep));

                        if (currentRoom.Blocks[x, z].Climb[1])
                            g.DrawLine(_climbPen, new PointF((startX + x + 1 - 3 / GridStep) * GridStep, (19 - (startY + z) - d) * GridStep), new PointF((startX + x + 1 - 3 / GridStep) * GridStep, (19 - (startY + z - 1) - d) * GridStep));

                        if (currentRoom.Blocks[x, z].Climb[2])
                            g.DrawLine(_climbPen, new PointF((startX + x) * GridStep, (19 - (startY + z + 3 / GridStep - 1) - d) * GridStep), new PointF((startX + x + 1) * GridStep, (19 - (startY + z + 3 / GridStep - 1) - d) * GridStep));

                        if (currentRoom.Blocks[x, z].Climb[3])
                            g.DrawLine(_climbPen, new PointF((startX + x + 3 / GridStep) * GridStep, (19 - (startY + z) - d) * GridStep), new PointF((startX + x + 3 / GridStep) * GridStep, (19 - (startY + z - 1) - d) * GridStep));

                        if ((currentRoom.Blocks[x, z].Flags & BlockFlags.Beetle) != 0)
                        {
                            g.DrawRectangle(_beetlePen, new Rectangle((startX + x) * GridStep + 2, (19 - (startY + z - 3 / GridStep) - d) * GridStep + 2, GridStep - 3, GridStep - 3));
                        }

                        if ((currentRoom.Blocks[x, z].Flags & BlockFlags.TriggerTriggerer) != 0)
                        {
                            g.DrawRectangle(_triggerTriggererPen, new Rectangle((startX + x) * GridStep + 2, (19 - (startY + z - 3 / GridStep) - d) * GridStep + 2, GridStep - 3, GridStep - 3));
                        }
                    }
                }

                // disegno le linee nere della griglia           
                for (int x = 0; x < 320; x += GridStep)
                {
                    g.DrawLine(Pens.Black, new Point(x, 0), new Point(x, 320));
                }

                for (int y = 0; y < 320; y += GridStep)
                {
                    g.DrawLine(Pens.Black, new Point(0, y), new Point(320, y));
                }

                if (SelectedPortal != -1)
                {
                    Portal p = _editor.Level.Portals[SelectedPortal];

                    int xMin = p.X;
                    int xMax = p.X + p.NumXBlocks - 1;
                    int zMin = p.Z;
                    int zMax = p.Z + p.NumZBlocks - 1;

                    Pen pen = new Pen(Brushes.YellowGreen, 2);

                    g.DrawLine(pen, new Point((startX + xMin) * GridStep, (startY + numZblocks - zMin - p.NumZBlocks) * GridStep), new Point((startX + xMax + 1) * GridStep, (startY + numZblocks - zMin - p.NumZBlocks) * GridStep));
                    g.DrawLine(pen, new Point((startX + xMax + 1) * GridStep, (startY + numZblocks - zMin - p.NumZBlocks) * GridStep), new Point((startX + xMax + 1) * GridStep, (startY + numZblocks - zMin) * GridStep));   // v
                    g.DrawLine(pen, new Point((startX + xMin) * GridStep, (startY + numZblocks - zMin) * GridStep), new Point((startX + xMax + 1) * GridStep, (startY + numZblocks - zMin) * GridStep));
                    g.DrawLine(pen, new Point((startX + xMin) * GridStep, (startY + numZblocks - zMin - p.NumZBlocks) * GridStep), new Point((startX + xMin) * GridStep, (startY + numZblocks - zMin) * GridStep));

                    string text = "Portal ";
                    if (p.Direction == PortalDirection.Floor)
                        text += " (On Floor) ";
                    if (p.Direction == PortalDirection.Ceiling)
                        text += " (On Ceiling) ";
                    text += "to Room #" + p.AdjoiningRoom.ToString();

                    DrawMessage(g, text, p.X, p.Z);
                }
                else if (SelectedTrigger != -1)
                {
                    TriggerInstance t = _editor.Level.Triggers[SelectedTrigger];

                    int xMin = t.X;
                    int xMax = t.X + t.NumXBlocks - 1;
                    int zMin = t.Z;
                    int zMax = t.Z + t.NumZBlocks - 1;
                    Pen pen = new Pen(Brushes.White, 2);

                    g.DrawLine(pen, new Point((startX + xMin) * GridStep, (startY + numZblocks - zMin - t.NumZBlocks) * GridStep), new Point((startX + xMax + 1) * GridStep, (startY + numZblocks - zMin - t.NumZBlocks) * GridStep));
                    g.DrawLine(pen, new Point((startX + xMax + 1) * GridStep, (startY + numZblocks - zMin - t.NumZBlocks) * GridStep), new Point((startX + xMax + 1) * GridStep, (startY + numZblocks - zMin) * GridStep));   // v
                    g.DrawLine(pen, new Point((startX + xMin) * GridStep, (startY + numZblocks - zMin) * GridStep), new Point((startX + xMax + 1) * GridStep, (startY + numZblocks - zMin) * GridStep));
                    g.DrawLine(pen, new Point((startX + xMin) * GridStep, (startY + numZblocks - zMin - t.NumZBlocks) * GridStep), new Point((startX + xMin) * GridStep, (startY + numZblocks - zMin) * GridStep));


                    string text = t.ToString();

                    DrawMessage(g, text, t.X, t.Z);
                }
                else
                {
                    // disegno il riquadro rosso di selezione
                    if (_editor.BlockSelectionStartX != -1 && _editor.BlockSelectionStartZ != -1 && _editor.BlockSelectionEndX != -1 && _editor.BlockSelectionEndZ != -1)
                    {
                        int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                        int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                        int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
                        int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

                        Pen pen = new Pen(Brushes.Red, 2);

                        int numX = xMax - xMin + 1;
                        int numZ = zMax - zMin + 1;

                        g.DrawLine(pen, new Point((startX + xMin) * GridStep, (startY + numZblocks - zMin - numZ) * GridStep), new Point((startX + xMax + 1) * GridStep, (startY + numZblocks - zMin - numZ) * GridStep));
                        g.DrawLine(pen, new Point((startX + xMax + 1) * GridStep, (startY + numZblocks - zMin - numZ) * GridStep), new Point((startX + xMax + 1) * GridStep, (startY + numZblocks - zMin) * GridStep));   // v
                        g.DrawLine(pen, new Point((startX + xMin) * GridStep, (startY + numZblocks - zMin) * GridStep), new Point((startX + xMax + 1) * GridStep, (startY + numZblocks - zMin) * GridStep));
                        g.DrawLine(pen, new Point((startX + xMin) * GridStep, (startY + numZblocks - zMin - numZ) * GridStep), new Point((startX + xMin) * GridStep, (startY + numZblocks - zMin) * GridStep)); // v ovest
                    }
                }
            }
            catch (Exception)
            { }

            _editor.UpdateStatistics();
        }

        private void DrawMessage(Graphics g, string text, int x, int z)
        {
            //_editor.DrawPanel2DMessage(text);

            SizeF dimension = g.MeasureString(text, _font, this.Width /*- x * GridStep*/, StringFormat.GenericDefault);

            short deltaX = (short)((this.Width - ((_startX + x) * GridStep + dimension.Width + 4.0f)) / GridStep);
            short deltaZ = (short)((this.Height - (this.Height - (_startY + z) * GridStep + dimension.Height + 4.0f)) / GridStep);

            if ((_startX + x) * GridStep + dimension.Width + 4.0f >= this.Width - GridStep)
                x = (short)((this.Width - dimension.Width - 4.0f) / GridStep - _startX); //  deltaX; // (int)(deltaX Math.Ceiling(dimension.Width / GridStep) - 1;
            else
                x++;

            //this.Width - (StartY + z) * GridStep + dimension.Height + 4.0f >= this.Width
            if (this.Height - (_startY + z) * GridStep + dimension.Height + 4.0f >= this.Height - GridStep)
                z = (short)((this.Height - dimension.Height - 4.0f) / GridStep - _startY); // deltaZ; // (int)Math.Ceiling(dimension.Height / GridStep);
            else
                z--;

            g.FillRectangle(Brushes.Yellow, new Rectangle((_startX + x) * GridStep, this.Width - (_startY + z + 1) * GridStep, (int)(dimension.Width + 4.0f), (int)(dimension.Height + 4.0f)));
            g.DrawRectangle(Pens.Black, new Rectangle((_startX + x) * GridStep, this.Width - (_startY + z + 1) * GridStep, (int)(dimension.Width + 4.0f), (int)(dimension.Height + 4.0f)));
            g.DrawString(text, _font, Brushes.Black, new RectangleF((_startX + x) * GridStep + 2, this.Width - (_startY + z + 1) * GridStep + 2, dimension.Width, dimension.Height), StringFormat.GenericDefault);
        }

        private int GetNextPortal(int x, int z)
        {
            int index = SelectedPortal;
            if (index == -1)
                index = 0;

            //   x = x + StartX;
            //  z = z + StartY;

            Room room = _editor.SelectedRoom;

            for (int i = index; i < _editor.Level.Portals.Count; i++)
            {
                if (i == SelectedPortal)
                    continue;

                Portal p = _editor.Level.Portals.ElementAt(i).Value;

                var theRoom = _editor.SelectedRoom;

                if (p.Room == theRoom && x >= p.X && x < p.X + p.NumXBlocks && z >= p.Z && z < p.Z + p.NumZBlocks)
                    return p.Id;
            }

            return -1;
        }

        private int GetNextTrigger(int x, int z)
        {
            int index = SelectedTrigger;
            if (index == -1)
                index = 0;

            //    x = x - StartX;
            //   z = z - StartY;

            for (int i = index; i < _editor.Level.Triggers.Count; i++)
            {
                if (i == SelectedTrigger)
                    continue;

                TriggerInstance t = _editor.Level.Triggers.ElementAt(i).Value;

                if (t.Room == _editor.SelectedRoom && x >= t.X && x < t.X + t.NumXBlocks && z >= t.Z && z < t.Z + t.NumZBlocks)
                    return t.Id;
            }

            return -1;
        }
    }
}
