using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using NLog;
using TombLib.Graphics;
using TombEditor.Geometry;
using Rectangle = System.Drawing.Rectangle;


namespace TombEditor.Controls
{
    public class Panel2DGrid : Panel
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); 
        
        private bool _doSectorSelection = false;
        private Editor _editor;
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
            _editor.EditorEventRaised += EditorEventRaised;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
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
                (obj is Editor.SelectedSectorsChangedEvent) ||
                (obj is Editor.RoomSectorPropertiesChangedEvent) ||
                ((obj is Editor.SelectedObjectChangedEvent) && IsObjectChangeRelevant((Editor.SelectedObjectChangedEvent)obj)))
            {
                Invalidate();
            }

            // Update curser
            if (obj is Editor.ActionChangedEvent)
            {
                EditorAction currentAction = ((Editor.ActionChangedEvent)obj).Current;
                Cursor = currentAction.RelocateCameraActive ? Cursors.Cross : Cursors.Arrow;
            }
        }

        private static bool IsObjectChangeRelevant(Editor.SelectedObjectChangedEvent e)
        {
            bool PreviousIsRelevant = (e.Previous.HasValue &&
                ((e.Previous.Value.Type == ObjectInstanceType.Trigger) ||
                (e.Previous.Value.Type == ObjectInstanceType.Portal)));
            bool CurrentIsRelevant = (e.Current.HasValue &&
                ((e.Current.Value.Type == ObjectInstanceType.Trigger) ||
                (e.Current.Value.Type == ObjectInstanceType.Portal)));
            return PreviousIsRelevant || CurrentIsRelevant;
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

        private SharpDX.DrawingPoint fromVisualCoord(PointF point)
        {
            RectangleF roomArea = getVisualRoomArea();
            return new SharpDX.DrawingPoint(
                (int)Math.Max(0, Math.Min(_editor.SelectedRoom.NumXSectors - 1, (point.X - roomArea.X) / _gridStep)),
                (int)Math.Max(0, Math.Min(_editor.SelectedRoom.NumZSectors - 1, (roomArea.Bottom - point.Y) / _gridStep)));
        }
        
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            if ((_editor == null) || (_editor.SelectedRoom == null))
                return;

            // Move camera to selected sector
            if (_editor.Action.RelocateCameraActive)
            {
                _editor.MoveCameraToSector(fromVisualCoord(e.Location));
                return;
            }
            SharpDX.DrawingPoint sectorPos = fromVisualCoord(e.Location);

            // Find existing sector based object (eg portal or trigger)
            SectorBasedObjectInstance selectedSectorObject = null;
            if (_editor.SelectedObject.HasValue)
                if (_editor.SelectedObject.Value.Type == ObjectInstanceType.Portal)
                    selectedSectorObject = _editor.Level.Portals.First(portal => portal.Id == _editor.SelectedObject.Value.Id);
                else if (_editor.SelectedObject.Value.Type == ObjectInstanceType.Trigger)
                    selectedSectorObject = _editor.Level.Triggers[_editor.SelectedObject.Value.Id];

            // Choose action
            if (e.Button == MouseButtons.Left)
            {
                if ((selectedSectorObject is Portal) && selectedSectorObject.Area.Contains(sectorPos))
                {
                    _editor.SelectedRoom = ((Portal)selectedSectorObject).AdjoiningRoom;
                    _editor.SelectedObject = _editor.Level.Portals[((Portal)selectedSectorObject).OtherId].ObjectPtr;
                }
                else if ((selectedSectorObject is TriggerInstance) && selectedSectorObject.Area.Contains(sectorPos))
                { // Open trigger options
                    EditorActions.EditObject(_editor.SelectedRoom, selectedSectorObject.ObjectPtr, this.Parent);
                }
                else
                { // Do block selection
                    _editor.SelectedSectors = new SectorSelection { Start = sectorPos, End = sectorPos };

                    if (selectedSectorObject != null) // Unselect previous object
                        _editor.SelectedObject = null;
                    _doSectorSelection = true;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Find next object
                var portalsInRoom = _editor.SelectedRoom.Portals.Cast<SectorBasedObjectInstance>();
                var triggersInRoom = _editor.Level.Triggers.Values
                    .Where((obj) => (obj.Room == _editor.SelectedRoom))
                    .Cast<SectorBasedObjectInstance>();
                var relevantTriggersAndPortals = portalsInRoom.Concat(triggersInRoom)
                    .Where((obj) => obj.Area.Contains(sectorPos));

                SectorBasedObjectInstance nextPortalOrTrigger = relevantTriggersAndPortals.
                    FindFirstAfterWithWrapAround((obj) => obj == selectedSectorObject, (obj) => true);
                if (nextPortalOrTrigger != null)
                    _editor.SelectedObject = nextPortalOrTrigger.ObjectPtr;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            if ((_editor?.SelectedRoom == null) || _editor.Action.RelocateCameraActive)
                return;

            if ((e.Button == MouseButtons.Left) && _doSectorSelection)
                _editor.SelectedSectors = new SectorSelection { Start = _editor.SelectedSectors.Start, End = fromVisualCoord(e.Location) };
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _doSectorSelection = false;
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
                        else if ((block.FloorPortal != null && !block.IsFloorSolid) || block.CeilingPortal != null || block.WallPortal != null)
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
                ObjectPtr? selectedObject = _editor.SelectedObject;
                if (selectedObject.HasValue && (selectedObject.Value.Type == ObjectInstanceType.Portal))
                {
                    Portal portal = _editor.Level.Portals.First(p => p.Id == selectedObject.Value.Id);
                    e.Graphics.DrawRectangle(_selectedPortalPen, toVisualCoord(portal.Area));
                    DrawMessage(e, portal.ToString(), toVisualCoord(new Point(portal.X + portal.NumXBlocks / 2, portal.Z + portal.NumZBlocks)));
                }
                if (selectedObject.HasValue && (selectedObject.Value.Type == ObjectInstanceType.Trigger))
                {
                    TriggerInstance trigger = _editor.Level.Triggers[selectedObject.Value.Id];
                    e.Graphics.DrawRectangle(_selectedTriggerPen, toVisualCoord(trigger.Area));
                    DrawMessage(e, trigger.ToString(), toVisualCoord(new Point(trigger.X + trigger.NumXBlocks / 2, trigger.Z + trigger.NumZBlocks)));
                }
                else if (_editor.SelectedSectors.Valid)
                {
                    e.Graphics.DrawRectangle(_selectionPen, toVisualCoord(_editor.SelectedSectors.Area));
                }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "An exception occured while drawing the 2D grid.");
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
    }
}
