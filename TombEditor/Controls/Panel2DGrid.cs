using NLog;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
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

        private static readonly float _outlineSectorColoringInfoWidth = 3;
        private static readonly Pen _gridPen = Pens.Black;
        private static readonly Pen _selectedPortalPen = new Pen(Color.YellowGreen, 2);
        private static readonly Pen _selectedTriggerPen = new Pen(Color.White, 2);
        private static readonly Pen _selectionPen = new Pen(Color.Red, 2);

        private float _gridSize => Math.Min(ClientSize.Width, ClientSize.Height);
        private float _gridStep => _gridSize / Room.MaxRoomDimensions;
        private ToolTip _toolTip = new ToolTip();

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
                obj is Editor.SelectedRoomChangedEvent ||
                obj is Editor.SelectedSectorsChangedEvent ||
                obj is Editor.RoomSectorPropertiesChangedEvent ||
                obj is Editor.SelectedObjectChangedEvent && IsObjectChangeRelevant((Editor.SelectedObjectChangedEvent)obj))
            {
                Invalidate();
            }

            // Update curser
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

        private RectangleF GetVisualAreaTotal()
        {
            return new RectangleF(
                (ClientSize.Width - _gridSize) * 0.5f,
                (ClientSize.Height - _gridSize) * 0.5f,
                _gridSize,
                _gridSize);
        }

        private RectangleF GetVisualAreaRoom()
        {
            Room currentRoom = _editor.SelectedRoom;
            RectangleF totalArea = GetVisualAreaTotal();

            return new RectangleF(
                totalArea.X + _gridStep * ((Room.MaxRoomDimensions - currentRoom.NumXSectors) / 2),
                totalArea.Y + _gridStep * ((Room.MaxRoomDimensions - currentRoom.NumZSectors) / 2),
                _gridStep * currentRoom.NumXSectors,
                _gridStep * currentRoom.NumZSectors);
        }

        private PointF ToVisualCoord(VectorInt2 sectorCoord)
        {
            RectangleF roomArea = GetVisualAreaRoom();
            return new PointF(sectorCoord.X * _gridStep + roomArea.X, roomArea.Bottom - (sectorCoord.Y + 1) * _gridStep);
        }

        private RectangleF ToVisualCoord(RectangleInt2 sectorArea)
        {
            PointF convertedPoint0 = ToVisualCoord(sectorArea.Start);
            PointF convertedPoint1 = ToVisualCoord(sectorArea.End);
            return RectangleF.FromLTRB(
                Math.Min(convertedPoint0.X, convertedPoint1.X), Math.Min(convertedPoint0.Y, convertedPoint1.Y),
                Math.Max(convertedPoint0.X, convertedPoint1.X) + _gridStep, Math.Max(convertedPoint0.Y, convertedPoint1.Y) + _gridStep);
        }

        private VectorInt2 FromVisualCoord(PointF point)
        {
            RectangleF roomArea = GetVisualAreaRoom();
            return new VectorInt2(
                (int)Math.Max(0, Math.Min(_editor.SelectedRoom.NumXSectors - 1, (point.X - roomArea.X) / _gridStep)),
                (int)Math.Max(0, Math.Min(_editor.SelectedRoom.NumZSectors - 1, (roomArea.Bottom - point.Y) / _gridStep)));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _toolTip.Hide(this);

            if (_editor == null || _editor.SelectedRoom == null)
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
                    selectedSectorObject.Room == _editor.SelectedRoom &&
                    selectedSectorObject.Area.Contains(sectorPos))
                {
                    if (selectedSectorObject is PortalInstance)
                    {
                        Room room = _editor.SelectedRoom;
                        _editor.SelectRoomAndResetCamera(((PortalInstance)selectedSectorObject).AdjoiningRoom);
                        _editor.SelectedObject = ((PortalInstance)selectedSectorObject).FindOppositePortal(room);
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
                var portalsInRoom = _editor.SelectedRoom.Portals.Cast<SectorBasedObjectInstance>();
                var triggersInRoom = _editor.SelectedRoom.Triggers.Cast<SectorBasedObjectInstance>();
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
            try
            {
                if (_editor == null || _editor.SelectedRoom == null)
                    return;

                Room currentRoom = _editor.SelectedRoom;
                RectangleF totalArea = GetVisualAreaTotal();
                RectangleF roomArea = GetVisualAreaRoom();
                bool probePortals = _editor.Configuration.Editor_ProbeAttributesThroughPortals;

                e.Graphics.FillRectangle(Brushes.White, totalArea);

                // Draw tiles
                for (int x = 0; x < currentRoom.NumXSectors; x++)
                    for (int z = 0; z < currentRoom.NumZSectors; z++)
                    {
                        RectangleF rectangle = new RectangleF(roomArea.X + x * _gridStep, roomArea.Y + (currentRoom.NumZSectors - 1 - z) * _gridStep, _gridStep, _gridStep);

                        var currentSectorColoringInfos = _editor.SectorColoringManager.ColoringInfo.GetColors(currentRoom, x, z, probePortals);
                        if (currentSectorColoringInfos != null)
                        {
                            for (int i = 0; i < currentSectorColoringInfos.Count; i++)
                            {
                                e.Graphics.SmoothingMode = currentSectorColoringInfos[i].Shape == SectorColoringShape.Rectangle ? SmoothingMode.Default : SmoothingMode.AntiAlias;

                                switch (currentSectorColoringInfos[i].Shape)
                                {
                                    case SectorColoringShape.Rectangle:
                                        using (var b = new SolidBrush(currentSectorColoringInfos[i].Color.ToWinFormsColor()))
                                            e.Graphics.FillRectangle(b, rectangle);
                                        break;
                                    case SectorColoringShape.Frame:
                                        RectangleF frameAttribRect = rectangle;
                                        frameAttribRect.Inflate(-(_outlineSectorColoringInfoWidth / 2), -(_outlineSectorColoringInfoWidth / 2));
                                        using (var b = new Pen(currentSectorColoringInfos[i].Color.ToWinFormsColor(), _outlineSectorColoringInfoWidth))
                                            e.Graphics.DrawRectangle(b, frameAttribRect);
                                        break;
                                    case SectorColoringShape.Hatch:
                                        using (var b = new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.Transparent, currentSectorColoringInfos[i].Color.ToWinFormsColor()))
                                            e.Graphics.FillRectangle(b, rectangle);
                                        break;
                                    case SectorColoringShape.EdgeZp:
                                    case SectorColoringShape.EdgeZn:
                                    case SectorColoringShape.EdgeXp:
                                    case SectorColoringShape.EdgeXn:
                                        RectangleF edgeRect;
                                        if (currentSectorColoringInfos[i].Shape == SectorColoringShape.EdgeZp)
                                            edgeRect = new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, _outlineSectorColoringInfoWidth);
                                        else if (currentSectorColoringInfos[i].Shape == SectorColoringShape.EdgeZn)
                                            edgeRect = new RectangleF(rectangle.X, rectangle.Bottom - _outlineSectorColoringInfoWidth, rectangle.Width, _outlineSectorColoringInfoWidth);
                                        else if (currentSectorColoringInfos[i].Shape == SectorColoringShape.EdgeXp)
                                            edgeRect = new RectangleF(rectangle.Right - _outlineSectorColoringInfoWidth, rectangle.Y, _outlineSectorColoringInfoWidth, rectangle.Height);
                                        else
                                            edgeRect = new RectangleF(rectangle.X, rectangle.Y, _outlineSectorColoringInfoWidth, rectangle.Height);
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
                                            points[0] = new PointF(rectangle.Left, rectangle.Top);
                                            points[1] = new PointF(rectangle.Left, rectangle.Bottom);
                                            points[2] = new PointF(rectangle.Right, rectangle.Bottom);
                                        }
                                        else if (currentSectorColoringInfos[i].Shape == SectorColoringShape.TriangleXnZp)
                                        {
                                            points[0] = new PointF(rectangle.Left, rectangle.Bottom);
                                            points[1] = new PointF(rectangle.Left, rectangle.Top);
                                            points[2] = new PointF(rectangle.Right, rectangle.Top);
                                        }
                                        else if (currentSectorColoringInfos[i].Shape == SectorColoringShape.TriangleXpZn)
                                        {
                                            points[0] = new PointF(rectangle.Left, rectangle.Bottom);
                                            points[1] = new PointF(rectangle.Right, rectangle.Top);
                                            points[2] = new PointF(rectangle.Right, rectangle.Bottom);
                                        }
                                        else
                                        {
                                            points[0] = new PointF(rectangle.Left, rectangle.Top);
                                            points[1] = new PointF(rectangle.Right, rectangle.Top);
                                            points[2] = new PointF(rectangle.Right, rectangle.Bottom);
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

                // Draw black grid lines
                for (int x = 0; x <= Room.MaxRoomDimensions; ++x)
                    e.Graphics.DrawLine(_gridPen, totalArea.X + x * _gridStep, totalArea.Y, totalArea.X + x * _gridStep, totalArea.Y + _gridSize);
                for (int y = 0; y <= Room.MaxRoomDimensions; ++y)
                    e.Graphics.DrawLine(_gridPen, totalArea.X, totalArea.Y + y * _gridStep, totalArea.X + _gridSize, totalArea.Y + y * _gridStep);

                // Draw selection
                if (_editor.SelectedSectors.Valid)
                    e.Graphics.DrawRectangle(_selectionPen, ToVisualCoord(_editor.SelectedSectors.Area));

                var instance = _editor.SelectedObject as SectorBasedObjectInstance;
                if (instance != null && instance.Room == _editor.SelectedRoom)
                {
                    Pen pen = instance is PortalInstance ? _selectedPortalPen : _selectedTriggerPen;
                    RectangleF visualArea = ToVisualCoord(instance.Area);
                    e.Graphics.DrawRectangle(pen, visualArea);
                }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "An exception occured while drawing the 2D grid.");
            }
        }
    }
}
