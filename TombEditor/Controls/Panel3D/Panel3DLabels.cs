using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using TombLib.LevelData;
using TombLib.RenderingV2.Text;
using TombLib.Utils;

namespace TombEditor.Controls.Panel3D
{
    // text overlay. Builds the list of TextLabels the renderer draws
    // every frame — the same strings the legacy renderer produced in
    // Panel3DDraw.DrawText / DrawPlaceholders / DrawLights / ... — and hands
    // them to the renderer through RenderScene. All editor-specific string
    // formatting stays here; the renderer only projects and rasterises.
    public partial class Panel3D
    {
        private readonly Stopwatch _fpsWatch = Stopwatch.StartNew();
        private double _fps;

        private static readonly Vector4 _textWhite = new(1f, 1f, 1f, 1f);

        // Legacy object tags sit slightly above-right of the anchor.
        private static readonly Vector2 _objectTagOffset = new(10f, -10f);
        private static readonly Vector2 _alignTopLeft    = new(0f, 0f);
        private static readonly Vector2 _alignCenter     = new(0.5f, 0.5f);

        /// <summary>
        /// Collects every text label for the current frame. Mirrors the legacy
        /// Panel3DDraw.DrawText: room names, cardinal directions, FPS + selected
        /// object read-out, the selected object's detailed tag, and memos
        /// flagged "always display".
        /// </summary>
        internal List<TextLabel> BuildSceneLabels()
        {
            var labels = new List<TextLabel>();
            if (_editor?.Level == null)
                return labels;

            bool overlay = _editor.Configuration.Rendering3D_DrawFontOverlays;

            // Rooms whose names / contents we label: just the selected room in
            // the default view, the whole level when "show all rooms" is on —
            // matching the renderer's own visible-room set.
            IEnumerable<Room> rooms = ShowAllRooms
                ? _editor.Level.Rooms.Where(r => r != null)
                : (_editor.SelectedRoom != null ? new[] { _editor.SelectedRoom } : Array.Empty<Room>());

            // --- Room names --------------------------------------------------
            if (ShowRoomNames)
                foreach (Room room in rooms)
                {
                    if (room?.RoomGeometry == null) continue;
                    labels.Add(TextLabel.World(room.Name, room.WorldPos + room.GetLocalCenter(),
                                               _textWhite, overlay, _alignCenter));
                }

            // --- Cardinal directions ----------------------------------------
            if (ShowCardinalDirections && _editor.SelectedRoom != null)
                AddCardinalDirections(labels, overlay);

            // --- FPS + selected-object read-out (top-left, screen-space) -----
            AddDebugString(labels, overlay);

            // --- Selected object detailed tag -------------------------------
            ObjectInstance selected = _editor.SelectedObject;
            if (selected is ObjectGroup group)
            {
                labels.Add(TextLabel.World(
                    "Group of " + group.Count() + " objects\n" +
                    GetObjectPositionString(group.Room, group),
                    group.Room.WorldPos + group.Position,
                    _textWhite, overlay, _alignTopLeft, _objectTagOffset));
            }
            else if (selected != null)
            {
                string text = GetObjectLabelString(selected);
                if (!string.IsNullOrEmpty(text) && TryGetLabelAnchor(selected, out Vector3 anchor))
                    labels.Add(TextLabel.World(text, anchor, _textWhite, overlay,
                                               _alignTopLeft, _objectTagOffset));
            }

            // --- Memos flagged "always display" -----------------------------
            if (ShowOtherObjects)
                foreach (Room room in rooms)
                {
                    if (room?.Objects == null) continue;
                    foreach (var obj in room.Objects)
                        if (obj is MemoInstance memo && memo.AlwaysDisplay
                            && !ReferenceEquals(memo, selected) && !string.IsNullOrEmpty(memo.Text))
                            labels.Add(TextLabel.World(memo.Text, memo.Room.WorldPos + memo.Position,
                                                       _textWhite, overlay,
                                                       _alignTopLeft, _objectTagOffset));
                }

            return labels;
        }

        private void AddCardinalDirections(List<TextLabel> labels, bool overlay)
        {
            Room room = _editor.SelectedRoom;
            string[] messages = _editor.Configuration.Rendering3D_UseRoomEditorDirections
                ? new[] { "+Z (East)", "-Z (West)", "+X (South)", "-X (North)" }
                : new[] { "+Z (North)", "-Z (South)", "+X (East)", "-X (West)" };

            Vector3[] offsets =
            {
                new(0, 0,  room.NumZSectors * Level.HalfSectorSizeUnit),
                new(0, 0, -room.NumZSectors * Level.HalfSectorSizeUnit),
                new( room.NumXSectors * Level.HalfSectorSizeUnit, 0, 0),
                new(-room.NumXSectors * Level.HalfSectorSizeUnit, 0, 0),
            };

            Vector3 center = room.WorldPos + room.GetLocalCenter();
            for (int i = 0; i < 4; i++)
                labels.Add(TextLabel.World(messages[i], center + offsets[i],
                                           _textWhite, overlay, _alignCenter));
        }

        private void AddDebugString(List<TextLabel> labels, bool overlay)
        {
            string debug = "";

            if (_editor.Configuration.Rendering3D_ShowFPS)
            {
                double dt = _fpsWatch.Elapsed.TotalSeconds;
                _fpsWatch.Restart();
                if (dt > 0)
                {
                    double instant = 1.0 / dt;
                    // Light smoothing so the read-out doesn't flicker.
                    _fps = _fps <= 0 ? instant : _fps * 0.9 + instant * 0.1;
                }
                debug += "FPS: " + Math.Round(_fps, 1) + "\n";
            }

            if (_editor.SelectedObject != null)
                debug += "Selected Object: " + _editor.SelectedObject.ToShortString();

            if (debug.Length > 0)
                labels.Add(TextLabel.Screen(debug.TrimEnd('\n'), new Vector2(10f, 10f),
                                            _textWhite, overlay, _alignTopLeft));
        }

        /// <summary>
        /// World-space vertical line from the selected object down to the
        /// floor under it — the legacy "object height line". Null when no
        /// position-based object is selected.
        /// </summary>
        internal (Vector3 From, Vector3 To)? BuildHeightLine()
        {
            if (_editor?.SelectedObject is not PositionBasedObjectInstance pbi || pbi.Room == null)
                return null;
            try
            {
                Vector3 wp   = pbi.Room.WorldPos;
                Vector3 from = wp + pbi.Position;
                Vector3 to   = wp + new Vector3(pbi.Position.X,
                                                GetFloorHeight(pbi.Room, pbi.Position),
                                                pbi.Position.Z);
                return (from, to);
            }
            catch { return null; }
        }

        /// <summary>
        /// The object-brush overlay (painting mode): the floor circle under
        /// the cursor. Null when the brush is inactive.
        /// </summary>
        internal (Vector3 Center, float Radius)? BuildBrushOverlay()
        {
            var bo = ComputeBrushOverlay();
            if (bo.Shape == 0) return null;
            return (new Vector3(bo.Center.X, bo.Center.Y, bo.Center.Z), bo.Center.W);
        }

        /// <summary>
        /// The flyby depth-of-field overlay parameters, when a TombEngine
        /// flyby camera with a DOF mode is selected; null otherwise.
        /// </summary>
        internal (Vector4 CenterRange, Vector4 DirectionDistance, Vector4 ColorStrength)? BuildDofOverlay()
        {
            return TryGetFlybyDofOverlayState(out var dof)
                ? (dof.CenterRange, dof.DirectionDistance, dof.ColorStrength)
                : null;
        }

        // World-space anchor for an object's detailed tag. For position-based
        // objects this is the object origin; ghost blocks use their centre.
        private static bool TryGetLabelAnchor(ObjectInstance obj, out Vector3 anchor)
        {
            if (obj is PositionBasedObjectInstance pbi && pbi.Room != null)
            {
                anchor = pbi.Room.WorldPos + pbi.Position;
                return true;
            }
            if (obj is GhostBlockInstance ghost && ghost.Room != null)
            {
                anchor = ghost.CenterMatrix(ghost.SelectedFloor).Translation;
                return true;
            }
            anchor = default;
            return false;
        }

        // The per-type detailed label, ported verbatim from the legacy
        // Panel3DDraw text tags. Returns null for object kinds the legacy
        // renderer never labelled (portals, sector triggers, ...).
        private string GetObjectLabelString(ObjectInstance obj)
        {
            switch (obj)
            {
                case LightInstance light:
                    return light.Type.ToString().SplitCamelcase() + " Light" + "\n" +
                           GetObjectPositionString(light.Room, light);

                case CameraInstance camera:
                    return "Camera " +
                           (camera.CameraMode == CameraInstanceMode.Locked ? "(Locked)" :
                            camera.CameraMode == CameraInstanceMode.Sniper ? "(Sniper)" : "") +
                           camera.GetScriptIDOrName() + "\n" +
                           GetObjectPositionString(camera.Room, camera) + GetObjectTriggerString(camera);

                case FlybyCameraInstance flyby:
                    return "Flyby cam (" + flyby.Sequence + ":" + flyby.Number + ") " +
                           flyby.GetScriptIDOrName() + "\n" +
                           GetObjectPositionString(flyby.Room, flyby) + GetObjectTriggerString(flyby);

                case SinkInstance sink:
                    return sink.ToShortString() + "\n" +
                           GetObjectPositionString(sink.Room, sink) + GetObjectTriggerString(sink);

                case SoundSourceInstance sound:
                    return "Sound source ID " +
                           (sound.SoundId != -1 ? sound.SoundId + ": " + sound.SoundNameToDisplay
                                                : "No sound assigned yet") +
                           sound.GetScriptIDOrName() + "\n" +
                           GetObjectPositionString(sound.Room, sound);

                case SpriteInstance sprite:
                    return sprite.ShortName() + "\n" +
                           GetObjectPositionString(sprite.Room, sprite);

                case MemoInstance memo:
                    return memo.Text;

                case GhostBlockInstance ghost:
                    return ghost.InfoMessage();

                case VolumeInstance volume:
                    return volume.ToString();

                case MoveableInstance moveable:
                    if (_editor.Level?.Settings?.WadTryGetMoveable(moveable.WadObjectId) != null)
                        return moveable.ItemType.MoveableId.ShortName(_editor.Level.Settings.GameVersion) +
                               moveable.GetScriptIDOrName() + "\n" +
                               GetObjectPositionString(moveable.Room, moveable) + "\n" +
                               GetObjectRotationString(moveable.Room, moveable) +
                               (moveable.Ocb == 0 ? string.Empty : "\nOCB: " + moveable.Ocb) +
                               GetObjectTriggerString(moveable);
                    return moveable.ShortName() + "\nUnavailable " + moveable.ItemType +
                           moveable.GetScriptIDOrName() + "\n" +
                           GetObjectPositionString(moveable.Room, moveable) + GetObjectTriggerString(moveable);

                case StaticInstance stat:
                    if (_editor.Level?.Settings?.WadTryGetStatic(stat.WadObjectId) != null)
                        return stat.ItemType.StaticId.ToString(_editor.Level.Settings.GameVersion) +
                               stat.GetScriptIDOrName() + "\n" +
                               GetObjectPositionString(stat.Room, stat) + "\n" +
                               "Rotation Y: " + Math.Round(stat.RotationY, 2) +
                               GetObjectTriggerString(stat);
                    return stat.ShortName() + "\nUnavailable " + stat.ItemType +
                           GetObjectTriggerString(stat);

                case ImportedGeometryInstance imported:
                    if (imported.Model?.DirectXModel != null
                        && imported.Model.DirectXModel.Meshes.Count != 0 && !imported.Hidden)
                        return imported + "\n" +
                               GetObjectPositionString(imported.Room, imported) + "\n" +
                               GetObjectRotationString(imported.Room, imported) + "\n" +
                               "Scale: " + imported.Scale + "\n" +
                               "Triangles: " + imported.Model.DirectXModel.TotalTriangles;
                    return imported.ToString();

                default:
                    return null;
            }
        }
    }
}
