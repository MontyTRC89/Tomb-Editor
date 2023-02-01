﻿using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib;
using TombEditor.Controls.ContextMenus;
using TombLib.Graphics;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private void OnMouseButtonUpLeft(Point location)
        {
            if (_editor.Mode == EditorMode.Geometry && !_gizmoEnabled && !_objectPlaced)
            {
                var newBlockPicking = DoPicking(GetRay(location.X, location.Y)) as PickingResultBlock;
                if (newBlockPicking != null && !_toolHandler.Dragged)
                {
                    var pos = newBlockPicking.Pos;
                    var zone = _editor.SelectedSectors.Empty ? new RectangleInt2(pos, pos) : _editor.SelectedSectors.Area;
                    bool belongsToFloor = newBlockPicking.BelongsToFloor;

                    if (ModifierKeys.HasFlag(Keys.Alt) && zone.Contains(pos))
                    {
                        // Split the faces
                        if (belongsToFloor)
                            EditorActions.FlipFloorSplit(_editor.SelectedRoom, zone);
                        else
                            EditorActions.FlipCeilingSplit(_editor.SelectedRoom, zone);
                        return;
                    }
                    else if (ModifierKeys.HasFlag(Keys.Shift) && zone.Contains(pos))
                    {
                        // Rotate sector
                        EditorActions.RotateSectors(_editor.SelectedRoom, zone, belongsToFloor);
                        return;
                    }
                    else if (_editor.Tool.Tool == EditorToolType.Selection || 
                            (_editor.Tool.Tool >= EditorToolType.Drag && _editor.Tool.Tool < EditorToolType.PortalDigger))
                    {
                        if (!_doSectorSelection && _editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pos))
                        {
                            // Rotate the arrows
                            if (ModifierKeys.HasFlag(Keys.Control))
                            {
                                if (_editor.SelectedSectors.Arrow == ArrowType.CornerSW)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EntireFace);
                                else if (_editor.SelectedSectors.Arrow == ArrowType.CornerSE)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.CornerSW);
                                else if (_editor.SelectedSectors.Arrow == ArrowType.CornerNE)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.CornerSE);
                                else if (_editor.SelectedSectors.Arrow == ArrowType.CornerNW)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.CornerNE);
                                else
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.CornerNW);
                            }
                            else
                            {
                                if (_editor.SelectedSectors.Arrow == ArrowType.EdgeW)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EntireFace);
                                else if (_editor.SelectedSectors.Arrow == ArrowType.EdgeS)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EdgeW);
                                else if (_editor.SelectedSectors.Arrow == ArrowType.EdgeE)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EdgeS);
                                else if (_editor.SelectedSectors.Arrow == ArrowType.EdgeN)
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EdgeE);
                                else
                                    _editor.SelectedSectors = _editor.SelectedSectors.ChangeArrows(ArrowType.EdgeN);
                            }
                        }
                    }
                }
            }

            // Handle gizmo manipulation with ghostblock (to update room properties in 2D grid)
            if (_gizmoEnabled && _editor.SelectedObject is GhostBlockInstance)
                _editor.RoomSectorPropertiesChange(_editor.SelectedRoom);

            // Handle multiple object selection
            if (_editor.Tool.Tool == EditorToolType.Selection && _toolHandler.Engaged && ModifierKeys.HasFlag(Keys.Control))
                EditorActions.SelectObjectsInArea(FindForm(), _editor.HighlightedSectors, false);
        }

        private void OnMouseButtonUpRight(Point location)
        {
            var distance = new Vector2(_startMousePosition.X, _startMousePosition.Y) - new Vector2(location.X, location.Y);
            if (distance.Length() < 4.0f)
            {
                _currentContextMenu?.Dispose();
                _currentContextMenu = null;

                PickingResult newPicking = DoPicking(GetRay(location.X, location.Y), true);
                if (newPicking is PickingResultObject)
                {
                    ObjectInstance target = ((PickingResultObject)newPicking).ObjectInstance;
                    if (target is ISpatial)
                        _currentContextMenu = new MaterialObjectContextMenu(_editor, this, target);
                }
                else if (newPicking is PickingResultBlock)
                {
                    var pickedBlock = newPicking as PickingResultBlock;
                    if (_editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pickedBlock.Pos))
                        _currentContextMenu = new SelectedGeometryContextMenu(_editor, this, pickedBlock.Room, _editor.SelectedSectors.Area, pickedBlock.Pos);
                    else
                        _currentContextMenu = new BlockContextMenu(_editor, this, pickedBlock.Room, pickedBlock.Pos);
                }
                _currentContextMenu?.Show(PointToScreen(location));
            }
        }
    }
}
