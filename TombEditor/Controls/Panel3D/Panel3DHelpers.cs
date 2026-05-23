using System;
using System.Collections.Generic;
using System.Numerics;
using TombEditor.Controls.FlybyTimeline.Sequence;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private struct FlybyDofOverlayState
        {
            public int Mode;
            public Vector4 CenterRange;
            public Vector4 DirectionDistance;
            public Vector4 ColorStrength;
        }

        private bool TryGetSelectedFlybySequence(out int sequence)
        {
            if (_editor.SelectedObject is FlybyCameraInstance flyby)
            {
                sequence = flyby.Sequence;
                return true;
            }

            if (_editor.SelectedObject is ObjectGroup group)
            {
                bool hasFlyby = false;
                int selectedSequence = 0;
                foreach (var item in group)
                {
                    if (item is not FlybyCameraInstance selectedFlyby)
                        continue;
                    if (!hasFlyby)
                    {
                        selectedSequence = selectedFlyby.Sequence;
                        hasFlyby = true;
                        continue;
                    }
                    if (selectedFlyby.Sequence != selectedSequence)
                    {
                        sequence = 0;
                        return false;
                    }
                }
                if (hasFlyby)
                {
                    sequence = selectedSequence;
                    return true;
                }
            }

            sequence = 0;
            return false;
        }

        private static int GetFloorHeight(Room room, Vector3 position)
        {
            int xSector = (int)Math.Max(0, Math.Min(room.NumXSectors - 1, Math.Floor(position.X / Level.SectorSizeUnit)));
            int zSector = (int)Math.Max(0, Math.Min(room.NumZSectors - 1, Math.Floor(position.Z / Level.SectorSizeUnit)));

            // Get the base floor height
            return room.Sectors[xSector, zSector].Floor.Min;
        }

        private bool TryGetFlybyDofOverlayState(out FlybyDofOverlayState overlayState)
        {
            overlayState = default;

            if (!_editor.Level.IsTombEngine)
                return false;

            if (_editor.SelectedObject is not FlybyCameraInstance flyby || flyby.Room is null || flyby.DofMode == DofMode.None)
                return false;

            if (_editor.CameraPreviewMode != CameraPreviewType.None && _flybyPreview != null)
                return TryGetPreviewFlybyDofOverlayState(out overlayState);

            return TryCreateFlybyDofOverlayState(
                flyby.Room.WorldPos + flyby.Position,
                flyby.GetDirection(),
                flyby.DofDistance,
                flyby.DofRange,
                flyby.DofStrength,
                flyby.DofMode,
                out overlayState);
        }

        private bool TryGetPreviewFlybyDofOverlayState(out FlybyDofOverlayState overlayState)
        {
            overlayState = default;

            if (_editor.CameraPreviewMode == CameraPreviewType.None || _flybyPreview == null)
                return false;

            var frame = _flybyPreview.StaticFrame ?? _flybyPreview.LastFrame;

            if (frame.DofMode == DofMode.None)
                return false;

            var rotation = Matrix4x4.CreateFromYawPitchRoll(frame.RotationY, frame.RotationX, 0.0f);
            var direction = MathC.HomogenousTransform(Vector3.UnitZ, rotation);

            return TryCreateFlybyDofOverlayState(
                frame.Position,
                direction,
                frame.DofDistance,
                frame.DofRange,
                frame.DofStrength,
                frame.DofMode,
                out overlayState);
        }

        private static bool TryCreateFlybyDofOverlayState(Vector3 origin, Vector3 direction, float distance,
            float range, float strength, DofMode mode, out FlybyDofOverlayState overlayState)
        {
            overlayState = default;

            if (mode == DofMode.None)
                return false;

            float clampedStrength = Math.Clamp(strength, 0.0f, 1.0f);
            if (clampedStrength <= 0.0f)
                return false;

            float directionLengthSquared = direction.LengthSquared();
            if (!float.IsFinite(directionLengthSquared) || directionLengthSquared <= MathC.Epsilon)
                return false;

            float opacity = 0.35f + (clampedStrength * 0.55f);
            float darkeningMultiplier = 0.35f - (clampedStrength * 0.15f);

            overlayState = new FlybyDofOverlayState
            {
                Mode = (int)mode,
                CenterRange = new Vector4(origin, Math.Max(range, 1.0f)),
                DirectionDistance = new Vector4(Vector3.Normalize(direction), Math.Max(distance, 0.0f)),
                ColorStrength = new Vector4(darkeningMultiplier, darkeningMultiplier, darkeningMultiplier, (float)mode)
            };

            return true;
        }

        private class Comparer : IComparer<StaticInstance>, IComparer<MoveableInstance>, IComparer<ImportedGeometryInstance>
        {
            public int Compare(StaticInstance x, StaticInstance y)
            {
                return x.WadObjectId.TypeId.CompareTo(y.WadObjectId.TypeId);
            }

            public int Compare(MoveableInstance x, MoveableInstance y)
            {
                return x.WadObjectId.TypeId.CompareTo(y.WadObjectId.TypeId);
            }

            public int Compare(ImportedGeometryInstance x, ImportedGeometryInstance y)
            {
                try // Because TRTombalization makes direct comparison almost impossible to achieve without nullref exceptions
                {
                    var xModel = x?.Model ?? null;
                    var yModel = y?.Model ?? null;
                    if (xModel == null && yModel == null) return 0;
                    if (xModel == null && yModel != null) return 1;
                    if (xModel != null && yModel == null) return -1;
                    return x.Model.UniqueID.GetHashCode().CompareTo(y.Model.UniqueID.GetHashCode());
                }
                catch
                {
                    return 0;
                }
            }
        }

        protected override Vector4 ClearColor =>
            _editor?.SelectedRoom?.AlternateBaseRoom != null ?
                _editor.Configuration.UI_ColorScheme.ColorFlipRoom :
                ShowHorizon ? new Vector4(0) : _editor.Configuration.UI_ColorScheme.Color3DBackground;
    }
}
