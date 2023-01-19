using System.Drawing;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private void OnMouseButtonDownLeft(Point location)
        {
            // Do picking on the scene
            PickingResult newPicking = DoPicking(GetRay(location.X, location.Y), _editor.Configuration.Rendering3D_SelectObjectsInAnyRoom);

            if (newPicking is PickingResultBlock)
            {
                var newBlockPicking = (PickingResultBlock)newPicking;

                // Move camera to selected sector
                if (_editor.Action is EditorActionRelocateCamera)
                {
                    if (newBlockPicking.Room != _editor.SelectedRoom)
                        _editor.SelectedRoom = newBlockPicking.Room;
                    _editor.MoveCameraToSector(newBlockPicking.Pos);
                    return;
                }

                // Ignore block picking if it's not from current room.
                // Alternately, if room autoswitch is active, switch and select it.

                if (newBlockPicking.Room != _editor.SelectedRoom)
                {
                    if (_editor.Configuration.Rendering3D_AutoswitchCurrentRoom)
                        _editor.SelectedRoom = newBlockPicking.Room;
                    else
                        return;
                }

                // Place objects
                if (_editor.Action is IEditorActionPlace)
                {
                    var action = (IEditorActionPlace)_editor.Action;
                    EditorActions.PlaceObject(_editor.SelectedRoom, newBlockPicking.Pos, action.CreateInstance(_editor.Level, _editor.SelectedRoom));
                    _objectPlaced = true;
                    if (!action.ShouldBeActive)
                        _editor.Action = null;
                    return;
                }

                VectorInt2 pos = newBlockPicking.Pos;

                // Handle face selection
                if ((_editor.Tool.Tool == EditorToolType.Selection || _editor.Tool.Tool == EditorToolType.Group || _editor.Tool.Tool >= EditorToolType.Drag) &&
                    (ModifierKeys == Keys.None || ModifierKeys == Keys.Control))
                {
                    if (!_editor.SelectedSectors.Valid || !_editor.SelectedSectors.Area.Contains(pos))
                    {
                        // Select rectangle
                        if (_editor.Tool.Tool == EditorToolType.Selection && ModifierKeys.HasFlag(Keys.Control))
                        {
                            // Multiple object selection
                            _toolHandler.Engage(location.X, location.Y, newBlockPicking);
                            _editor.HighlightedSectors = new SectorSelection { Start = pos, End = pos };
                        }
                        else
                        {
                            // Normal face selection
                            _editor.SelectedSectors = new SectorSelection { Start = pos, End = pos };
                        }
                        _doSectorSelection = true;
                        return;
                    }
                }

                // Act based on editor mode
                bool belongsToFloor = newBlockPicking.BelongsToFloor;

                switch (_editor.Mode)
                {
                    case EditorMode.Geometry:
                        if (_editor.Tool.Tool != EditorToolType.Selection && _editor.Tool.Tool != EditorToolType.PortalDigger)
                        {
                            _toolHandler.Engage(location.X, location.Y, newBlockPicking);

                            if (_editor.Tool.Tool == EditorToolType.Brush || _editor.Tool.Tool == EditorToolType.Shovel)
                                _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom.AndAdjoiningRooms);
                            else if (_editor.Tool.Tool < EditorToolType.Drag)
                                _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

                            // HACK: Clear bulldozer's initial block heights
                            if (_editor.Tool.Tool == EditorToolType.Flatten)
                                OnMouseMovedLeft(location);

                            if (!ModifierKeys.HasFlag(Keys.Alt) && !ModifierKeys.HasFlag(Keys.Shift) && _toolHandler.Process(pos.X, pos.Y))
                            {
                                if (_editor.Tool.Tool == EditorToolType.Smooth)
                                    EditorActions.SmoothSector(_editor.SelectedRoom, pos.X, pos.Y, belongsToFloor ? BlockVertical.Floor : BlockVertical.Ceiling);
                                else if (_editor.Tool.Tool < EditorToolType.Flatten)
                                    EditorActions.EditSectorGeometry(_editor.SelectedRoom,
                                        new RectangleInt2(pos, pos),
                                        ArrowType.EntireFace,
                                        belongsToFloor ? BlockVertical.Floor : BlockVertical.Ceiling,
                                        (short)((_editor.Tool.Tool == EditorToolType.Shovel || _editor.Tool.Tool == EditorToolType.Pencil && ModifierKeys.HasFlag(Keys.Control)) ^ belongsToFloor ? 1 : -1),
                                        _editor.Tool.Tool == EditorToolType.Brush || _editor.Tool.Tool == EditorToolType.Shovel,
                                        false, false, true, true);
                            }
                        }
                        else if (_editor.Tool.Tool == EditorToolType.PortalDigger && _editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pos))
                        {
                            Room newRoom = null;

                            if (newBlockPicking.IsVerticalPlane)
                            {
                                newRoom = EditorActions.CreateAdjoiningRoom(_editor.SelectedRoom,
                                    _editor.SelectedSectors,
                                    PortalInstance.GetOppositeDirection(PortalInstance.GetDirection(newBlockPicking.Face.GetDirection())), false,
                                    1, !ModifierKeys.HasFlag(Keys.Control));
                            }
                            else
                            {
                                newRoom = EditorActions.CreateAdjoiningRoom(_editor.SelectedRoom,
                                    _editor.SelectedSectors,
                                    newBlockPicking.BelongsToFloor ? PortalDirection.Floor : PortalDirection.Ceiling, false,
                                    (short)(ModifierKeys.HasFlag(Keys.Shift) ? 1 : 4), !ModifierKeys.HasFlag(Keys.Control),
                                    ModifierKeys.HasFlag(Keys.Alt));
                            }

                            if (newRoom != null)
                            {
                                if (!ModifierKeys.HasFlag(Keys.Control))
                                    _editor.HighlightedSectors = new SectorSelection() { Area = newRoom.LocalArea };
                                _toolHandler.Engage(location.X, location.Y, newBlockPicking, false, newRoom);

                                if (!ShowPortals && !ShowAllRooms)
                                    _editor.SendMessage("Parent is invisible. Turn on Draw Portals mode.", TombLib.Forms.PopupType.Info);
                            }
                            return;
                        }
                        break;

                    case EditorMode.Lighting:
                    case EditorMode.FaceEdit:
                        // Disable texturing in lighting mode, if option is set
                        if (_editor.Mode == EditorMode.Lighting &&
                            !_editor.Configuration.Rendering3D_AllowTexturingInLightingMode)
                            break;

                        // Do texturing
                        if (_editor.Tool.Tool != EditorToolType.Group && _editor.Tool.Tool != EditorToolType.GridPaint)
                        {
                            if (ModifierKeys.HasFlag(Keys.Shift))
                            {
                                EditorActions.RotateTexture(_editor.SelectedRoom, pos, newBlockPicking.Face);
                                break;
                            }
                            else if (ModifierKeys.HasFlag(Keys.Control))
                            {
                                EditorActions.MirrorTexture(_editor.SelectedRoom, pos, newBlockPicking.Face);
                                break;
                            }
                        }

                        if (ModifierKeys.HasFlag(Keys.Alt))
                        {
                            EditorActions.PickTexture(_editor.SelectedRoom, pos, newBlockPicking.Face);
                        }
                        else if (_editor.Tool.Tool == EditorToolType.GridPaint && !_editor.HighlightedSectors.Empty)
                        {
                            EditorActions.TexturizeGroup(_editor.SelectedRoom,
                                _editor.HighlightedSectors,
                                _editor.SelectedSectors,
                                _editor.SelectedTexture,
                                newBlockPicking.Face,
                                ModifierKeys.HasFlag(Keys.Control),
                                !ModifierKeys.HasFlag(Keys.Shift));
                            _toolHandler.Engage(location.X, location.Y, newBlockPicking, false);
                        }
                        else if (_editor.SelectedSectors.Valid && _editor.SelectedSectors.Area.Contains(pos) || _editor.SelectedSectors.Empty)
                        {
                            switch (_editor.Tool.Tool)
                            {
                                case EditorToolType.Fill:
                                    if (newBlockPicking.IsFloorHorizontalPlane)
                                        EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Floor);
                                    else if (newBlockPicking.IsCeilingHorizontalPlane)
                                        EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Ceiling);
                                    else if (newBlockPicking.IsVerticalPlane)
                                        EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Wall);
                                    break;

                                case EditorToolType.Group:
                                    if (_editor.SelectedSectors.Valid)
                                        EditorActions.TexturizeGroup(_editor.SelectedRoom,
                                            _editor.SelectedSectors,
                                            _editor.SelectedSectors,
                                            _editor.SelectedTexture,
                                            newBlockPicking.Face,
                                            ModifierKeys.HasFlag(Keys.Control),
                                            !ModifierKeys.HasFlag(Keys.Shift));
                                    break;

                                case EditorToolType.Brush:
                                case EditorToolType.Pencil:
                                    EditorActions.ApplyTexture(_editor.SelectedRoom, pos, newBlockPicking.Face, _editor.SelectedTexture);
                                    _toolHandler.Engage(location.X, location.Y, newBlockPicking, false);
                                    break;

                                default:
                                    break;
                            }

                        }
                        break;
                }
            }
            else if (newPicking is PickingResultGizmo)
            {
                if (_editor.SelectedObject is PositionBasedObjectInstance)
                    _editor.UndoManager.PushObjectTransformed((PositionBasedObjectInstance)_editor.SelectedObject);
                else if (_editor.SelectedObject is GhostBlockInstance)
                    _editor.UndoManager.PushGhostBlockTransformed((GhostBlockInstance)_editor.SelectedObject);

                // Set gizmo axis
                _gizmo.ActivateGizmo((PickingResultGizmo)newPicking);
                _gizmoEnabled = true;
            }
            else if (newPicking is PickingResultObject)
            {
                var obj = ((PickingResultObject)newPicking).ObjectInstance;

                if (obj.Room != _editor.SelectedRoom && _editor.Configuration.Rendering3D_AutoswitchCurrentRoom)
                    _editor.SelectedRoom = obj.Room;

                // Auto-bookmark any object
                if (_editor.Configuration.Rendering3D_AutoBookmarkSelectedObject && !(obj is ImportedGeometryInstance) && !ModifierKeys.HasFlag(Keys.Alt))
                    EditorActions.BookmarkObject(obj);

                if (ModifierKeys.HasFlag(Keys.Alt)) // Pick item or imported geo without selection
                {
                    if (obj is ItemInstance)
                        _editor.ChosenItem = ((ItemInstance)obj).ItemType;
                    else if (obj is ImportedGeometryInstance)
                        _editor.ChosenImportedGeometry = ((ImportedGeometryInstance)obj).Model;
                }
                else if (_editor.SelectedObject != obj)
                {
                    if (ModifierKeys.HasFlag(Keys.Control)) // User is attempting to multi-select
                    {
                        EditorActions.MultiSelect(obj);
                    }
                    else // User is not attempting to multi-select
                    {
                        // Animate objects about to be selected
                        if (obj is GhostBlockInstance && _editor.Configuration.Rendering3D_AnimateGhostBlockUnfolding)
                            _movementTimer.Animate(AnimationMode.GhostBlockUnfold, 0.4f);

                        _editor.SelectedObject = obj;
                    }
                }

                if (obj is ItemInstance)
                    _dragObjectPicked = true; // Prepare for drag-n-drop

                if (obj is ISpatial)
                    _editor.LastSelection = LastSelectionType.SpatialObject;
            }
            else if (newPicking == null)
            {
                // Click outside room; if mouse is released without action, unselect all
                _noSelectionConfirm = true;
            }
        }

        private void OnMouseButtonDownRight(Point location)
        {
            _startMousePosition = location;
        }
    }
}
