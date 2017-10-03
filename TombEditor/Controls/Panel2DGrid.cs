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
using System.Drawing.Drawing2D;

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
        private static readonly Brush _forceFloorSolidBrush = new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.Transparent, Editor.ColorFloor.MixWith(Color.Black, 0.1));
        private static readonly Pen _beetlePen = new Pen(Color.FromArgb(100, 100, 100), 4);
        private static readonly Pen _triggerTriggererPen = new Pen(Color.FromArgb(0, 0, 252), 4);
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
        private float _gridSize => Math.Min(ClientSize.Width, ClientSize.Height);
        private float _gridStep => _gridSize / Room.MaxRoomDimensions;

        public Panel2DGrid()
        {
            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
            DoubleBuffered = true;
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
            return (e.Previous is SectorBasedObjectInstance) || (e.Current is SectorBasedObjectInstance);
        }

        private RectangleF getVisualAreaTotal()
        {
            return new RectangleF(
                (ClientSize.Width - _gridSize) * 0.5f,
                (ClientSize.Height - _gridSize) * 0.5f,
                _gridSize,
                _gridSize);
        }

        private RectangleF getVisualAreaRoom()
        {
            Room currentRoom = _editor.SelectedRoom;
            RectangleF totalArea = getVisualAreaTotal();

            return new RectangleF(
                totalArea.X + _gridStep * ((Room.MaxRoomDimensions - currentRoom.NumXSectors) / 2),
                totalArea.Y + _gridStep * ((Room.MaxRoomDimensions - currentRoom.NumZSectors) / 2),
                _gridStep * currentRoom.NumXSectors,
                _gridStep * currentRoom.NumZSectors);
        }

        public PointF toVisualCoord(SharpDX.DrawingPoint sectorCoord)
        {
            RectangleF roomArea = getVisualAreaRoom();
            return new PointF(sectorCoord.X * _gridStep + roomArea.X, roomArea.Bottom - (sectorCoord.Y + 1) * _gridStep);
        }

        public RectangleF ToVisualCoord(SharpDX.Rectangle sectorArea)
        {
            PointF convertedPoint0 = toVisualCoord(new SharpDX.DrawingPoint(sectorArea.Left, sectorArea.Top));
            PointF convertedPoint1 = toVisualCoord(new SharpDX.DrawingPoint(sectorArea.Right, sectorArea.Bottom));
            return RectangleF.FromLTRB(
                Math.Min(convertedPoint0.X, convertedPoint1.X), Math.Min(convertedPoint0.Y, convertedPoint1.Y),
                Math.Max(convertedPoint0.X, convertedPoint1.X) + _gridStep, Math.Max(convertedPoint0.Y, convertedPoint1.Y) + _gridStep);
        }

        public SharpDX.DrawingPoint FromVisualCoord(PointF point)
        {
            RectangleF roomArea = getVisualAreaRoom();
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
                _editor.MoveCameraToSector(FromVisualCoord(e.Location));
                return;
            }
            SharpDX.DrawingPoint sectorPos = FromVisualCoord(e.Location);

            // Find existing sector based object (eg portal or trigger)
            SectorBasedObjectInstance selectedSectorObject = _editor.SelectedObject as SectorBasedObjectInstance;

            // Choose action
            if (e.Button == MouseButtons.Left)
            {
                if ((selectedSectorObject != null) &&
                    (selectedSectorObject.Room == _editor.SelectedRoom) &&
                    selectedSectorObject.Area.Contains(sectorPos))
                {
                    if (selectedSectorObject is PortalInstance)
                    {
                        Room room = _editor.SelectedRoom;
                        _editor.SelectedRoom = ((PortalInstance)selectedSectorObject).AdjoiningRoom;
                        _editor.SelectedObject = ((PortalInstance)selectedSectorObject).FindOppositePortal(room);
                    }
                    else if (selectedSectorObject is TriggerInstance)
                    { // Open trigger options
                        EditorActions.EditObject(selectedSectorObject, this.Parent);
                    }
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
                var triggersInRoom = _editor.SelectedRoom.Triggers.Cast<SectorBasedObjectInstance>();
                var relevantTriggersAndPortals = portalsInRoom.Concat(triggersInRoom)
                    .Where((obj) => obj.Area.Contains(sectorPos));

                SectorBasedObjectInstance nextPortalOrTrigger = relevantTriggersAndPortals.
                    FindFirstAfterWithWrapAround((obj) => obj == selectedSectorObject, (obj) => true);
                if (nextPortalOrTrigger != null)
                    _editor.SelectedObject = nextPortalOrTrigger;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if ((_editor?.SelectedRoom == null) || _editor.Action.RelocateCameraActive)
                return;

            if ((e.Button == MouseButtons.Left) && _doSectorSelection)
                _editor.SelectedSectors = new SectorSelection { Start = _editor.SelectedSectors.Start, End = FromVisualCoord(e.Location) };
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _doSectorSelection = false;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                if ((_editor == null) || (_editor.SelectedRoom == null))
                    return;

                Room currentRoom = _editor.SelectedRoom;
                RectangleF totalArea = getVisualAreaTotal();
                RectangleF roomArea = getVisualAreaRoom();

                e.Graphics.FillRectangle(Brushes.White, totalArea);

                // Draw tiles
                for (int x = 0; x < currentRoom.NumXSectors; x++)
                    for (int z = 0; z < currentRoom.NumZSectors; z++)
                    {
                        RectangleF rectangle = new RectangleF(roomArea.X + x * _gridStep, roomArea.Y + (currentRoom.NumZSectors - 1 - z) * _gridStep, _gridStep, _gridStep);
                        Block block = currentRoom.Blocks[x, z];

                        // Draw floor tile
                        if (block.Triggers.Count != 0)
                            e.Graphics.FillRectangle(_triggerBrush, rectangle);
                        else if (block.FloorPortal != null || block.CeilingPortal != null || block.WallPortal != null)
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
                        else if (block.Flags.HasFlag(BlockFlags.DeathFire) || block.Flags.HasFlag(BlockFlags.DeathElectricity) || block.Flags.HasFlag(BlockFlags.DeathLava))
                            e.Graphics.FillRectangle(_deathBrush, rectangle);
                        else
                            e.Graphics.FillRectangle(_floorBrush, rectangle);

                        //Draw additional features on floor tile
                        if (block.ForceFloorSolid)
                            e.Graphics.FillRectangle(_forceFloorSolidBrush, rectangle);
                        if (block.Flags.HasFlag(BlockFlags.ClimbPositiveZ))
                            e.Graphics.FillRectangle(_climbBrush, rectangle.X, rectangle.Y, rectangle.Width, _climbWidth);
                        if (block.Flags.HasFlag(BlockFlags.ClimbPositiveX))
                            e.Graphics.FillRectangle(_climbBrush, rectangle.Right - _climbWidth, rectangle.Y, _climbWidth, rectangle.Height);
                        if (block.Flags.HasFlag(BlockFlags.ClimbNegativeZ))
                        e.Graphics.FillRectangle(_climbBrush, rectangle.X, rectangle.Bottom - _climbWidth, rectangle.Width, _climbWidth);
                        if (block.Flags.HasFlag(BlockFlags.ClimbNegativeX))
                            e.Graphics.FillRectangle(_climbBrush, rectangle.X, rectangle.Y, _climbWidth, rectangle.Height);
                        RectangleF beetleTriggerRectangle = rectangle;
                        beetleTriggerRectangle.Inflate(-2, -2);
                        if ((block.Flags & BlockFlags.Beetle) != 0)
                            e.Graphics.DrawRectangle(_beetlePen, beetleTriggerRectangle);
                        if ((currentRoom.Blocks[x, z].Flags & BlockFlags.TriggerTriggerer) != 0)
                            e.Graphics.DrawRectangle(_triggerTriggererPen, beetleTriggerRectangle);
                    }

                // Draw black grid lines
                for (int x = 0; x <= Room.MaxRoomDimensions; ++x)
                    e.Graphics.DrawLine(_gridPen, totalArea.X + x * _gridStep, totalArea.Y, totalArea.X + x * _gridStep, totalArea.Y + _gridSize);

                for (int y = 0; y <= Room.MaxRoomDimensions; ++y)
                    e.Graphics.DrawLine(_gridPen, totalArea.X, totalArea.Y + y * _gridStep, totalArea.X + _gridSize, totalArea.Y + y * _gridStep);

                // Draw selection
                if (_editor.SelectedSectors.Valid)
                    e.Graphics.DrawRectangle(_selectionPen, ToVisualCoord(_editor.SelectedSectors.Area));

                var instance = _editor.SelectedObject as SectorBasedObjectInstance;
                if ((instance != null) && (instance.Room == _editor.SelectedRoom))
                {
                    Pen pen = instance is PortalInstance ? _selectedPortalPen : _selectedTriggerPen;
                    RectangleF visualArea = ToVisualCoord(instance.Area);
                    e.Graphics.DrawRectangle(pen, visualArea);
                    DrawMessage(e, instance.ToString(), visualArea);
                }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "An exception occured while drawing the 2D grid.");
            }
        }

        private void DrawMessage(PaintEventArgs e, string text, RectangleF visualArea)
        {
            SizeF textSize = e.Graphics.MeasureString(text, _font, ClientSize.Width, StringFormat.GenericDefault);
            textSize += new SizeF(_textMargin * 2, _textMargin * 2);

            // Choose appropriate space that does not occlude 'visualArea' if possible
            float spaceAbove = visualArea.Y;
            float spaceBelow = Height - visualArea.Bottom;
            float posX = visualArea.X + (visualArea.Width - textSize.Width) * 0.5f;
            RectangleF textRectangle;
            if ((spaceAbove <= textSize.Height) && (spaceBelow >= textSize.Height))
            { // Place message below
                textRectangle = new RectangleF(new PointF(posX, visualArea.Bottom + _textMargin), textSize);
            }
            else
            { // Place message above
                textRectangle = new RectangleF(new PointF(posX, visualArea.Y - textSize.Height - _textMargin), textSize);
            }

            // Move the area so that it is always visible
            textRectangle.Offset(-Math.Min(textRectangle.Left, 0.0f), -Math.Min(textRectangle.Top, 0.0f));
            textRectangle.Offset(-Math.Max(textRectangle.Right - ClientSize.Width, 0.0f), -Math.Max(textRectangle.Bottom - Height, 0.0f));

            // Draw the message
            e.Graphics.FillRectangle(_messageBackground, textRectangle);
            e.Graphics.DrawRectangle(_messagePen, textRectangle);
            textRectangle.Inflate(-_textMargin, -_textMargin);
            e.Graphics.DrawString(text, _font, _messagePen.Brush, textRectangle, StringFormat.GenericDefault);
        }
    }
}
