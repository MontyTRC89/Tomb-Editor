using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Utils;

namespace TombEditor.Controls
{
    public class Panel2DGrid : Panel
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private bool _doSectorSelection;
        private readonly Editor _editor;
        private Room _room;
        private ToolTip _toolTip = new ToolTip();

        private static readonly float _outlineSectorColoringInfoWidth = 3;
        private static readonly Pen _gridPen = Pens.Black;
        private static readonly Pen _selectedPortalPen = new Pen(Color.YellowGreen, 2);
        private static readonly Pen _selectedTriggerPen = new Pen(Color.White, 2);
        private static readonly Pen _selectionPen = new Pen(Color.Red, 2);

        public static readonly HashSet<SectorColoringType> IgnoredHighlights = new HashSet<SectorColoringType>
        {
            SectorColoringType.FloorPortal,
            SectorColoringType.CeilingPortal
        };

        public Room Room
        {
            get { return _room; }
            set { if (_room == value) return; _room = value; Invalidate(); }
        }

        public Panel2DGrid()
        {
            DoubleBuffered = true;

            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _editor = Editor.Instance;
                _editor.EditorEventRaised += EditorEventRaised;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            base.Dispose(disposing);
            _toolTip.Dispose();
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update drawing
            if (obj is SectorColoringManager.ChangeSectorColoringInfoEvent ||
                obj is Editor.SelectedSectorsChangedEvent ||
                obj is Editor.RoomSectorPropertiesChangedEvent ||
                obj is Editor.RoomGeometryChangedEvent ||
                obj is Editor.ConfigurationChangedEvent ||
                obj is Editor.SelectedObjectChangedEvent && IsObjectChangeRelevant((Editor.SelectedObjectChangedEvent)obj))
            {
                Invalidate();
            }

            // Update cursor
            if (obj is Editor.ActionChangedEvent)
            {
                IEditorAction currentAction = ((Editor.ActionChangedEvent)obj).Current;
                Cursor = currentAction is EditorActionRelocateCamera ? Cursors.Cross : Cursors.Arrow;
            }
        }

        private static bool IsObjectChangeRelevant(Editor.SelectedObjectChangedEvent e)
        {
            return e.Previous is SectorBasedObjectInstance || e.Current is SectorBasedObjectInstance;
        }

        protected virtual VectorInt2 RoomSize => Room.SectorSize;

        protected virtual VectorInt2 GetGridDimensions()
        {
            return VectorInt2.Max(RoomSize, new VectorInt2(Room.DefaultRoomDimensions, Room.DefaultRoomDimensions));
        }

        protected float GetGridStep()
        {
            VectorInt2 gridDimensions = GetGridDimensions();
            Size ClientSize = this.ClientSize;
            if ((ClientSize.Width * gridDimensions.Y) < (ClientSize.Height * gridDimensions.X))
                return (float)(ClientSize.Width) / gridDimensions.X;
            else
                return (float)(ClientSize.Height) / gridDimensions.Y;
        }

        protected RectangleF GetVisualAreaTotal()
        {
            Vector2 gridSize = (Vector2)GetGridDimensions() * GetGridStep();
            return new RectangleF(
                (ClientSize.Width - gridSize.X) * 0.5f,
                (ClientSize.Height - gridSize.Y) * 0.5f,
                gridSize.X,
                gridSize.Y);
        }

        protected RectangleF GetVisualAreaRoom()
        {
            RectangleF totalArea = GetVisualAreaTotal();
            float gridStep = GetGridStep();
            VectorInt2 gridDimensions = GetGridDimensions();
            VectorInt2 roomSize = RoomSize;

            return new RectangleF(
                totalArea.X + gridStep * ((gridDimensions.X - roomSize.X) / 2),
                totalArea.Y + gridStep * ((gridDimensions.Y - roomSize.Y) / 2),
                gridStep * roomSize.X,
                gridStep * roomSize.Y);
        }

        protected PointF ToVisualCoord(VectorInt2 sectorCoord)
        {
            RectangleF roomArea = GetVisualAreaRoom();
            float gridStep = GetGridStep();
            return new PointF(sectorCoord.X * gridStep + roomArea.X, roomArea.Bottom - (sectorCoord.Y + 1) * gridStep);
        }

        protected RectangleF ToVisualCoord(RectangleInt2 sectorArea)
        {
            PointF convertedPoint0 = ToVisualCoord(sectorArea.Start);
            PointF convertedPoint1 = ToVisualCoord(sectorArea.End);
            float gridStep = GetGridStep();
            return RectangleF.FromLTRB(
                Math.Min(convertedPoint0.X, convertedPoint1.X), Math.Min(convertedPoint0.Y, convertedPoint1.Y),
                Math.Max(convertedPoint0.X, convertedPoint1.X) + gridStep, Math.Max(convertedPoint0.Y, convertedPoint1.Y) + gridStep);
        }

        protected VectorInt2 FromVisualCoord(PointF point)
        {
            RectangleF roomArea = GetVisualAreaRoom();
            float gridStep = GetGridStep();
            VectorInt2 roomSize = RoomSize;
            return new VectorInt2(
                (int)Math.Max(0, Math.Min(roomSize.X - 1, (point.X - roomArea.X) / gridStep)),
                (int)Math.Max(0, Math.Min(roomSize.Y - 1, (roomArea.Bottom - point.Y) / gridStep)));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _toolTip.Hide(this);

            if (_editor == null || Room == null)
                return;

            // Move camera to selected sector
            if (_editor.Action is EditorActionRelocateCamera)
            {
                _editor.MoveCameraToSector(FromVisualCoord(e.Location));
                return;
            }
            VectorInt2 sectorPos = FromVisualCoord(e.Location);

            // Find existing sector based object (eg portal or trigger)
            SectorBasedObjectInstance selectedSectorObject = _editor.SelectedObject as SectorBasedObjectInstance;

            // Choose action
            if (e.Button == MouseButtons.Left)
            {
                if (selectedSectorObject != null &&
                    selectedSectorObject.Room == Room &&
                    selectedSectorObject.Area.Contains(sectorPos))
                {
                    if (selectedSectorObject is PortalInstance)
                    {
                        var portal = (PortalInstance)selectedSectorObject;
                        Room room = Room;
                        if (room.AlternateBaseRoom != null && portal.AdjoiningRoom.Alternated)
                        { // Go straight to alternated room
                            _editor.SelectRoom(portal.AdjoiningRoom.AlternateRoom);
                            _editor.SelectedObject = portal.FindOppositePortal(room).FindAlternatePortal(portal.AdjoiningRoom.AlternateRoom);
                        }
                        else
                        { // Go straight to base room
                            _editor.SelectRoom(portal.AdjoiningRoom);
                            _editor.SelectedObject = portal.FindOppositePortal(room);
                        }
                    }
                    else if (selectedSectorObject is TriggerInstance)
                    { // Open trigger options
                        EditorActions.EditObject(selectedSectorObject, Parent);
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
                var portalsInRoom = Room.Portals.Cast<SectorBasedObjectInstance>();
                var triggersInRoom = Room.Triggers.Cast<SectorBasedObjectInstance>();
                var relevantTriggersAndPortals = portalsInRoom.Concat(triggersInRoom)
                    .Where(obj => obj.Area.Contains(sectorPos));

                SectorBasedObjectInstance nextPortalOrTrigger = relevantTriggersAndPortals.
                    FindFirstAfterWithWrapAround(obj => obj == selectedSectorObject, obj => true);
                if (nextPortalOrTrigger != null)
                {
                    _editor.SelectedObject = nextPortalOrTrigger;
                    _toolTip.Show(nextPortalOrTrigger.ToString(), this, e.Location + new Size(5, 5));
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_editor?.SelectedRoom == null || _editor.Action is EditorActionRelocateCamera)
                return;

            if (e.Button == MouseButtons.Left && _doSectorSelection)
                _editor.SelectedSectors = new SectorSelection { Start = _editor.SelectedSectors.Start, End = FromVisualCoord(e.Location) };
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _doSectorSelection = false;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _toolTip.Hide(this);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_editor == null || Room == null)
                return;

            try
            {
                RectangleF totalArea = GetVisualAreaTotal();
                RectangleF roomArea = GetVisualAreaRoom();
                VectorInt2 gridDimensions = GetGridDimensions();
                float gridStep = GetGridStep();
                VectorInt2 roomSize = RoomSize;

                // Draw background
                using(var b = new SolidBrush(_editor.Configuration.UI_ColorScheme.Color2DBackground.ToWinFormsColor()))
                    e.Graphics.FillRectangle(b, totalArea);

                // Draw tiles
                for (int x = 0; x < roomSize.X; x++)
                    for (int z = 0; z < roomSize.Y; z++)
                        PaintSectorTile(e, new RectangleF(roomArea.X + x * gridStep, roomArea.Y + (roomSize.Y - 1 - z) * gridStep, gridStep, gridStep), x, z);

                // Draw black grid lines
                for (int x = 0; x <= gridDimensions.X; ++x)
                    e.Graphics.DrawLine(_gridPen, totalArea.X + x * gridStep, totalArea.Y, totalArea.X + x * gridStep, totalArea.Y + gridStep * gridDimensions.Y);
                for (int y = 0; y <= gridDimensions.Y; ++y)
                    e.Graphics.DrawLine(_gridPen, totalArea.X, totalArea.Y + y * gridStep, totalArea.X + gridStep * gridDimensions.X, totalArea.Y + y * gridStep);

                // Draw selection
                if (DrawSelection)
                {
                    if (_editor.SelectedSectors.Valid)
                        e.Graphics.DrawRectangle(_selectionPen, ToVisualCoord(_editor.SelectedSectors.Area));

                    var instance = _editor.SelectedObject as SectorBasedObjectInstance;
                    if (instance != null && instance.Room == Room)
                    {
                        Pen pen = instance is PortalInstance ? _selectedPortalPen : _selectedTriggerPen;
                        RectangleF visualArea = ToVisualCoord(instance.Area);
                        e.Graphics.DrawRectangle(pen, visualArea);
                    }
                }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "An exception occured while drawing the 2D grid.");
            }
        }

        protected virtual bool DrawSelection { get; } = true;

        protected virtual void PaintSectorTile(PaintEventArgs e, RectangleF sectorArea, int x, int z)
        {
            var currentSectorColoringInfos = _editor.SectorColoringManager.ColoringInfo.GetColors(_editor.Configuration.UI_ColorScheme, Room, x, z, _editor.Configuration.UI_ProbeAttributesThroughPortals, IgnoredHighlights);
            if (currentSectorColoringInfos == null)
                return;

            for (int i = 0; i < currentSectorColoringInfos.Count; i++)
            {
                e.Graphics.SmoothingMode = currentSectorColoringInfos[i].Shape == SectorColoringShape.Rectangle ? SmoothingMode.Default : SmoothingMode.AntiAlias;

                switch (currentSectorColoringInfos[i].Shape)
                {
                    case SectorColoringShape.Rectangle:
                        using (var b = new SolidBrush(currentSectorColoringInfos[i].Color.ToWinFormsColor()))
                            e.Graphics.FillRectangle(b, sectorArea);
                        break;
                    case SectorColoringShape.Frame:
                        RectangleF frameAttribRect = sectorArea;
                        frameAttribRect.Inflate(-(_outlineSectorColoringInfoWidth / 2), -(_outlineSectorColoringInfoWidth / 2));
                        using (var b = new Pen(currentSectorColoringInfos[i].Color.ToWinFormsColor(), _outlineSectorColoringInfoWidth))
                            e.Graphics.DrawRectangle(b, frameAttribRect);
                        break;
                    case SectorColoringShape.Hatch:
                        using (var b = new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.Transparent, currentSectorColoringInfos[i].Color.ToWinFormsColor()))
                            e.Graphics.FillRectangle(b, sectorArea);
                        break;
                    case SectorColoringShape.EdgeZp:
                    case SectorColoringShape.EdgeZn:
                    case SectorColoringShape.EdgeXp:
                    case SectorColoringShape.EdgeXn:
                        RectangleF edgeRect;
                        if (currentSectorColoringInfos[i].Shape == SectorColoringShape.EdgeZp)
                            edgeRect = new RectangleF(sectorArea.X, sectorArea.Y, sectorArea.Width, _outlineSectorColoringInfoWidth);
                        else if (currentSectorColoringInfos[i].Shape == SectorColoringShape.EdgeZn)
                            edgeRect = new RectangleF(sectorArea.X, sectorArea.Bottom - _outlineSectorColoringInfoWidth, sectorArea.Width, _outlineSectorColoringInfoWidth);
                        else if (currentSectorColoringInfos[i].Shape == SectorColoringShape.EdgeXp)
                            edgeRect = new RectangleF(sectorArea.Right - _outlineSectorColoringInfoWidth, sectorArea.Y, _outlineSectorColoringInfoWidth, sectorArea.Height);
                        else
                            edgeRect = new RectangleF(sectorArea.X, sectorArea.Y, _outlineSectorColoringInfoWidth, sectorArea.Height);
                        using (var b = new SolidBrush(currentSectorColoringInfos[i].Color.ToWinFormsColor()))
                            e.Graphics.FillRectangle(b, edgeRect);
                        break;
                    case SectorColoringShape.TriangleXnZn:
                    case SectorColoringShape.TriangleXnZp:
                    case SectorColoringShape.TriangleXpZn:
                    case SectorColoringShape.TriangleXpZp:
                        PointF[] points = new PointF[3];
                        if (currentSectorColoringInfos[i].Shape == SectorColoringShape.TriangleXnZn)
                        {
                            points[0] = new PointF(sectorArea.Left, sectorArea.Top);
                            points[1] = new PointF(sectorArea.Left, sectorArea.Bottom);
                            points[2] = new PointF(sectorArea.Right, sectorArea.Bottom);
                        }
                        else if (currentSectorColoringInfos[i].Shape == SectorColoringShape.TriangleXnZp)
                        {
                            points[0] = new PointF(sectorArea.Left, sectorArea.Bottom);
                            points[1] = new PointF(sectorArea.Left, sectorArea.Top);
                            points[2] = new PointF(sectorArea.Right, sectorArea.Top);
                        }
                        else if (currentSectorColoringInfos[i].Shape == SectorColoringShape.TriangleXpZn)
                        {
                            points[0] = new PointF(sectorArea.Left, sectorArea.Bottom);
                            points[1] = new PointF(sectorArea.Right, sectorArea.Top);
                            points[2] = new PointF(sectorArea.Right, sectorArea.Bottom);
                        }
                        else
                        {
                            points[0] = new PointF(sectorArea.Left, sectorArea.Top);
                            points[1] = new PointF(sectorArea.Right, sectorArea.Top);
                            points[2] = new PointF(sectorArea.Right, sectorArea.Bottom);
                        }
                        using (var b = new SolidBrush(currentSectorColoringInfos[i].Color.ToWinFormsColor()))
                            e.Graphics.FillPolygon(b, points);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
