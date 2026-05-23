using System.Collections.Generic;
using System.Numerics;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        // Sector-picking result used by the object-brush + ToolHandler. The V2
        // mouse path has its own object picker; this file is the legacy ray /
        // room-geometry pick that survived because the brush still needs it.
        private class PickingResultSector : PickingResult
        {
            public float      VerticalCoord { get; set; }
            public VectorInt2 Pos           { get; set; }
            public Room       Room          { get; set; }
            public SectorFace Face          { get; set; }

            public bool IsFloorHorizontalPlane   => Face.IsFloor();
            public bool IsCeilingHorizontalPlane => Face.IsCeiling();
            public bool IsVerticalPlane          => !IsFloorHorizontalPlane && !IsCeilingHorizontalPlane;
            public bool BelongsToFloor           => IsFloorHorizontalPlane || Face.IsFloorWall();
            public bool BelongsToCeiling         => IsCeilingHorizontalPlane || Face.IsCeilingWall();

            public PickingResultSector(float distance, float verticalCoord, VectorInt2 pos, Room room, SectorFace face)
            {
                Distance      = distance;
                VerticalCoord = verticalCoord;
                Pos           = pos;
                Room          = room;
                Face          = face;
            }
        }

        // Build a world-space pick ray from screen coords. Self-contained — no
        // dependency on a cached view-projection — so the brush works
        // regardless of paint timing.
        private Ray GetRay(float x, float y)
        {
            if (Camera == null) return new Ray(Vector3.Zero, Vector3.UnitZ);
            var vp = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            return Ray.GetPickRay(new Vector2(x, y), vp, ClientSize.Width, ClientSize.Height);
        }

        // Pure ray vs room-geometry pick — pure CPU math, no rendering data.
        // Object picking is no longer supported here (the mouse path does
        // its own object picking); the only remaining caller is the
        // object-brush helper, which always passes <c>skipObjects: true</c>.
        private PickingResult DoPicking(Ray ray, bool pickAnyRoom = false, bool skipObjects = false)
        {
            // The gizmo always draws on top of everything else, so it claims
            // the first chance to capture the click.
            PickingResult result = CanUseGizmo() ? _gizmo.DoPicking(ray) : null;
            if (result != null)
                return result;

            List<Room> rooms = pickAnyRoom ? CollectRoomsToDraw(_editor.SelectedRoom)
                                           : new List<Room> { _editor.SelectedRoom };

            foreach (var room in rooms)
            {
                // Pick hidden rooms only for place actions, or when the global
                // hidden-pick setting is off.
                if (DisablePickingForHiddenRooms
                    && room.Properties.Hidden
                    && room == _editor.SelectedRoom
                    && _editor.Action is not IEditorActionPlace)
                    continue;

                var hit = room.RoomGeometry?.RayIntersectsGeometry(
                    new Ray(ray.Position - room.WorldPos, ray.Direction));
                if (hit != null && (result == null || hit.Value.Distance < result.Distance))
                    result = new PickingResultSector(
                        hit.Value.Distance, hit.Value.VerticalCoord, hit.Value.Pos, room, hit.Value.Face);
            }

            return result;
        }
    }
}
