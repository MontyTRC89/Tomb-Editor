using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using TombLib.Graphics;
using TombEditor.Geometry;
using Rectangle = System.Drawing.Rectangle;


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
        private bool _drag;
        private static readonly Pen _gridPen = Pens.Black;
        private static readonly Brush _portalBrush = new SolidBrush(Color.Black);
        private static readonly Brush _floorBrush = new SolidBrush(Editor.ColorFloor);
        private static readonly Brush _wallBrush = new SolidBrush(Editor.ColorWall);
        private static readonly Brush _borderWallBrush = new SolidBrush(Color.Gray);
        private static readonly Brush _deathBrush = new SolidBrush(Editor.ColorDeath);
        private static readonly Brush _monkeyBrush = new SolidBrush(Editor.ColorMonkey);
        private static readonly Brush _boxBrush = new SolidBrush(Editor.ColorBox);
        private static readonly Brush _notWalkableBrush = new SolidBrush(Editor.ColorNotWalkable);
        private static readonly Pen _beetlePen = new Pen(Color.FromArgb(100, 100, 100), 4);
        private static readonly Pen _triggerTriggererPen = new Pen(Color.FromArgb(0, 0, 252), 4);
        private static readonly Brush _noCollisionBrush = new SolidBrush(Editor.ColorNoCollision);
        private static readonly Brush _triggerBrush = new SolidBrush(Editor.ColorTrigger);
        private static readonly Brush _climbBrush = new SolidBrush(Editor.ColorClimb);
        private static readonly float _climbWidth = 4;
        private static readonly Font _font = new Font("Arial", 8);
        private static readonly Pen _selectedPortalPen = new Pen(Color.YellowGreen, 2);
        private static readonly Pen _selectedTriggerPen = new Pen(Color.White, 2);
        private static readonly Pen _selectionPen = new Pen(Color.Red, 2);
        private static readonly Brush _messageBackground = new SolidBrush(Color.Yellow);
        private static readonly Pen _messagePen = Pens.Black;
        private const float _textMargin = 2.0f;
        private const float _gridStep = 11.0f;
        private const int _gridSize = 20;

        public Panel2DGrid()
        {
            _editor = Editor.Instance;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        private RectangleF getVisualRoomArea()
        {
            Room currentRoom = _editor.SelectedRoom;
            return new RectangleF(
                _gridStep * ((_gridSize - currentRoom.NumXSectors) / 2),
                _gridStep * ((_gridSize - currentRoom.NumZSectors) / 2),
                _gridStep * currentRoom.NumXSectors,
                _gridStep * currentRoom.NumZSectors);
        }

        private PointF toVisualCoord(Point point)
        {
            RectangleF roomArea = getVisualRoomArea();
            return new PointF(point.X * _gridStep + roomArea.X, roomArea.Bottom - (point.Y + 1) * _gridStep);
        }

        private RectangleF toVisualCoord(SharpDX.Rectangle area)
        {
            RectangleF roomArea = getVisualRoomArea();
            PointF convertedPoint0 = toVisualCoord(new Point(area.Left, area.Top));
            PointF convertedPoint1 = toVisualCoord(new Point(area.Right, area.Bottom));
            return RectangleF.FromLTRB(
                Math.Min(convertedPoint0.X, convertedPoint1.X), Math.Min(convertedPoint0.Y, convertedPoint1.Y),
                Math.Max(convertedPoint0.X, convertedPoint1.X) + _gridStep, Math.Max(convertedPoint0.Y, convertedPoint1.Y) + _gridStep);
        }

        private Point fromVisualCoord(PointF point)
        {
            RectangleF roomArea = getVisualRoomArea();
            return new Point(
                (int)Math.Max(0, Math.Min(_editor.SelectedRoom.NumXSectors - 1, (point.X - roomArea.X) / _gridStep)),
                (int)Math.Max(0, Math.Min(_editor.SelectedRoom.NumZSectors - 1, (roomArea.Bottom - point.Y) / _gridStep)));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            if ((_editor == null) || (_editor.SelectedRoom == null))
                return;
            
            // Manage the drag & drop camera
            if (e.Button == MouseButtons.Right)
            {
                Point roomPoint = fromVisualCoord(e.Location);
                bool foundSomething = false;

                if (SelectedTrigger == -1)
                {
                    // Start the cycle from the first portal
                    SelectedPortal = GetNextPortal(roomPoint);
                    if (SelectedPortal == -1)
                    {
                        SelectedTrigger = GetNextTrigger(roomPoint);
                        if (SelectedTrigger != -1)
                        {
                            foundSomething = true;

                            PickingResult result = new PickingResult();
                            result.ElementType = PickingElementType.Trigger;
                            result.Element = SelectedTrigger;
                            _editor.PickingResult = result;
                        }
                    }
                    else
                    {
                        foundSomething = true;

                        PickingResult result = new PickingResult();
                        result.ElementType = PickingElementType.Portal;
                        result.Element = SelectedPortal;
                        _editor.PickingResult = result;
                    }
                }
                else if (SelectedPortal == -1)
                {
                    SelectedTrigger = GetNextTrigger(roomPoint);
                    if (SelectedTrigger == -1)
                    {
                        SelectedPortal = GetNextPortal(roomPoint);
                        if (SelectedPortal != -1)
                        {
                            foundSomething = true;

                            PickingResult result = new PickingResult();
                            result.ElementType = PickingElementType.Portal;
                            result.Element = SelectedPortal;
                            _editor.PickingResult = result;
                        }
                    }
                    else
                    {
                        foundSomething = true;

                        PickingResult result = new PickingResult();
                        result.ElementType = PickingElementType.Trigger;
                        result.Element = SelectedTrigger;
                        _editor.PickingResult = result;
                    }
                }

                if (!foundSomething)
                {
                    SelectedPortal = -1;
                    SelectedTrigger = -1;
                }

                // Update state
                _editor.DrawPanel3D();
                _editor.UpdateStatistics();
                Invalidate();
            }
            else if ((e.Button == MouseButtons.Left) && (SelectedPortal == -1))
            {
                _drag = true;
                _firstSelection = false;
                Point roomPoint = fromVisualCoord(e.Location);

                // If any of the X or Z values is equal to - 1 then it is a first selection
                if (_editor.BlockSelectionStart.X == -1 || _editor.BlockSelectionStart.Y == -1)
                {
                    _editor.BlockSelectionStart = roomPoint;
                    _editor.BlockSelectionEnd = roomPoint;
                    _firstSelection = true;
                    _editor.BlockEditingType = 0;
                }
                else if (_editor.BlockSelection.Contains(roomPoint))
                {
                    // This is not a first selection because the clicking happend on an already selected range
                    _firstSelection = false;
                }
                else
                {
                    _editor.BlockSelectionStart = roomPoint;
                    _editor.BlockSelectionEnd = roomPoint;
                    _firstSelection = true;
                    _editor.BlockEditingType = 0;
                }

                PickingResult result = new PickingResult();
                result.ElementType = PickingElementType.Block;
                result.SubElementType = 0;
                result.Element = (roomPoint.X << 5) + (roomPoint.Y & 31);
                result.SubElementType = 25;
                _editor.PickingResult = result;

                SelectedTrigger = -1;
                SelectedPortal = -1;

                // Update state
                _editor.DrawPanel3D();
                _editor.UpdateStatistics();
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            if ((_editor == null) || (_editor.SelectedRoom == null))
                return;
            
            if ((e.Button == MouseButtons.Left) && (SelectedPortal == -1) && _drag)
            {
                Point roomPoint = fromVisualCoord(e.Location);

                //_editor.BlockSelectionStart = new Point(_editor.StartPickingResult.Element >> 5, _editor.StartPickingResult.Element & 31);
                _editor.BlockSelectionEnd = roomPoint;
                _editor.BlockEditingType = 0;
                _firstSelection = true;

                PickingResult result = new PickingResult();
                result.ElementType = PickingElementType.Block;
                result.SubElementType = 0;
                result.Element = (roomPoint.X << 5) + (roomPoint.Y & 31);
                result.SubElementType = 25;
                _editor.PickingResult = result;

                // Update state
                _editor.DrawPanel3D();
                _editor.UpdateStatistics();
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if ((_editor == null) || (_editor.SelectedRoom == null))
                return;
            
            if ((e.Button == MouseButtons.Left) && (SelectedPortal == -1) && _drag && _firstSelection)
            {
                _editor.LoadTriggersInUI();
                _editor.BlockEditingType = 0;
            }
            if ((e.Button == MouseButtons.Left) && (SelectedPortal != -1))
            {
                Point roomPoint = fromVisualCoord(e.Location);
                
                Portal p = _editor.Level.Portals[SelectedPortal];
                if (p.Area.Contains(roomPoint))
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

                // Update state
                _editor.DrawPanel3D();
                _editor.UpdateStatistics();
                Invalidate();
            }

            _drag = false;
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                e.Graphics.Clear(Color.White);

                if ((_editor == null) || (_editor.SelectedRoom == null))
                    return;

                Room currentRoom = _editor.SelectedRoom;
                RectangleF roomArea = getVisualRoomArea();
                
                // Draw tiles
                for (int x = 0; x < currentRoom.NumXSectors; x++)
                    for (int z = 0; z < currentRoom.NumZSectors; z++)
                    {
                        RectangleF rectangle = new RectangleF(roomArea.X + x * _gridStep, roomArea.Y + (currentRoom.NumZSectors - 1 - z) * _gridStep, _gridStep + 1, _gridStep + 1);
                        Block block = currentRoom.Blocks[x, z];

                        // Draw floor tile
                        if (block.Triggers.Count != 0)
                            e.Graphics.FillRectangle(_triggerBrush, rectangle);
                        else if ((block.FloorPortal != -1 && !block.IsFloorSolid) || block.CeilingPortal != -1 || block.WallPortal != -1)
                            e.Graphics.FillRectangle(_portalBrush, rectangle);
                        else if (block.Type == BlockType.BorderWall)
                            e.Graphics.FillRectangle(_borderWallBrush, rectangle);
                        else if (block.Type == BlockType.Wall)
                            e.Graphics.FillRectangle(_wallBrush, rectangle);
                        else if (block.Flags.HasFlag(BlockFlags.NotWalkableFloor))
                            e.Graphics.FillRectangle(_notWalkableBrush, rectangle);
                        else if (block.Flags.HasFlag(BlockFlags.Box))
                            e.Graphics.FillRectangle(_boxBrush, rectangle);
                        else if (block.Flags.HasFlag(BlockFlags.Monkey))
                            e.Graphics.FillRectangle(_monkeyBrush, rectangle);
                        else if (block.Flags.HasFlag(BlockFlags.Death) || block.Flags.HasFlag(BlockFlags.Electricity) || block.Flags.HasFlag(BlockFlags.Lava))
                            e.Graphics.FillRectangle(_deathBrush, rectangle);
                        else if (block.NoCollisionFloor || block.NoCollisionCeiling)
                            e.Graphics.FillRectangle(_noCollisionBrush, rectangle);
                        else
                            e.Graphics.FillRectangle(_floorBrush, rectangle);

                        //Draw additional features on floor tile
                        if (block.Climb[0])
                            e.Graphics.FillRectangle(_climbBrush, rectangle.X, rectangle.Y, rectangle.Width, _climbWidth);
                        if (block.Climb[1])
                            e.Graphics.FillRectangle(_climbBrush, rectangle.Right - _climbWidth, rectangle.Y, _climbWidth, rectangle.Height);
                        if (block.Climb[2])
                        e.Graphics.FillRectangle(_climbBrush, rectangle.X, rectangle.Bottom - _climbWidth, rectangle.Width, _climbWidth);
                        if (block.Climb[3])
                            e.Graphics.FillRectangle(_climbBrush, rectangle.X, rectangle.Y, _climbWidth, rectangle.Height);
                        RectangleF beetleTriggerRectangle = rectangle;
                        beetleTriggerRectangle.Inflate(-2, -2);
                        if ((block.Flags & BlockFlags.Beetle) != 0)
                            e.Graphics.DrawRectangle(_beetlePen, beetleTriggerRectangle);
                        if ((currentRoom.Blocks[x, z].Flags & BlockFlags.TriggerTriggerer) != 0)
                            e.Graphics.DrawRectangle(_triggerTriggererPen, beetleTriggerRectangle);
                    }

                // Draw black grid lines           
                for (float x = 0; x < Width; x += _gridStep)
                    e.Graphics.DrawLine(_gridPen, x, 0, x, 320);

                for (float y = 0; y < Height; y += _gridStep)
                    e.Graphics.DrawLine(_gridPen, 0, y, 320, y);

                // Draw selection
                if (SelectedPortal != -1)
                {
                    Portal portal = _editor.Level.Portals[SelectedPortal];
                    e.Graphics.DrawRectangle(_selectedPortalPen, toVisualCoord(portal.Area));
                    DrawMessage(e, portal.ToString(), toVisualCoord(new Point(portal.X + portal.NumXBlocks / 2, portal.Z + portal.NumZBlocks)));
                }
                else if (SelectedTrigger != -1)
                {
                    TriggerInstance trigger = _editor.Level.Triggers[SelectedTrigger];
                    e.Graphics.DrawRectangle(_selectedTriggerPen, toVisualCoord(trigger.Area));
                    DrawMessage(e, trigger.ToString(), toVisualCoord(new Point(trigger.X + trigger.NumXBlocks / 2, trigger.Z + trigger.NumZBlocks)));
                }
                else if (_editor.BlockSelectionAvailable)
                {
                    e.Graphics.DrawRectangle(_selectionPen, toVisualCoord(_editor.BlockSelection));
                }
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Log(NLog.LogLevel.Error, exc, "An exception occured while drawing the 2D grid.");
            }
        }

        private void DrawMessage(PaintEventArgs e, string text, PointF visualPos)
        {
            // Measure how much area is required for the message
            RectangleF textRectangle = new RectangleF(visualPos, 
                e.Graphics.MeasureString(text, _font, this.Width, StringFormat.GenericDefault));
            textRectangle.Offset(textRectangle.Width * -0.5f, 0.0f);
            textRectangle.Inflate(_textMargin, _textMargin);

            // Move the area so that it is always visible
            textRectangle.Offset(-Math.Min(textRectangle.Left, 0.0f), -Math.Min(textRectangle.Top, 0.0f));
            textRectangle.Offset(-Math.Max(textRectangle.Right - Width, 0.0f), -Math.Max(textRectangle.Bottom - Height, 0.0f));

            // Draw the message
            e.Graphics.FillRectangle(_messageBackground, textRectangle);
            e.Graphics.DrawRectangle(_messagePen, textRectangle);
            textRectangle.Inflate(-_textMargin, -_textMargin);
            e.Graphics.DrawString(text, _font, _messagePen.Brush, textRectangle, StringFormat.GenericDefault);
        }

        private int GetNextPortal(Point point)
        {
            for (int i = 1; i <= _editor.Level.Portals.Count; i++)
            {
                int index = (i + SelectedPortal) % _editor.Level.Portals.Count;
                Portal portal = _editor.Level.Portals.ElementAt(index).Value;
                if ((_editor.RoomIndex == portal.Room) && portal.Area.Contains(point))
                    return portal.ID;
            }
            return -1;
        }

        private int GetNextTrigger(Point point)
        {
            for (int i = 1; i <= _editor.Level.Triggers.Count; i++)
            {
                int index = (i + SelectedTrigger) % _editor.Level.Triggers.Count;
                TriggerInstance trigger = _editor.Level.Triggers.ElementAt(index).Value;
                if ((_editor.RoomIndex == trigger.Room) && trigger.Area.Contains(point))
                    return trigger.ID;
            }
            return -1;
        }
    }
}
