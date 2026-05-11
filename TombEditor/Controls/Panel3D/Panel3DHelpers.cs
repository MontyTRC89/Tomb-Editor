using System;
using System.Collections.Generic;
using System.Numerics;
using TombEditor.Controls.FlybyTimeline.Sequence;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Utils;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private static int GetFloorHeight(Room room, Vector3 position)
        {
            int xSector = (int)Math.Max(0, Math.Min(room.NumXSectors - 1, Math.Floor(position.X / Level.SectorSizeUnit)));
            int zSector = (int)Math.Max(0, Math.Min(room.NumZSectors - 1, Math.Floor(position.Z / Level.SectorSizeUnit)));

            // Get the base floor height
            return room.Sectors[xSector, zSector].Floor.Min;
        }

        private Vector4 ConvertColor(Vector3 originalColor)
        {
            switch (_editor.Level.Settings.GameVersion.Native())
            {
                case TRVersion.Game.TR1:
                case TRVersion.Game.TR2:
                    return new Vector4(new Vector3(originalColor.GetLuma()), 1.0f);

                case TRVersion.Game.TombEngine:
                    return new Vector4(originalColor, 1.0f);

                // All engine versions up to TR5 use 15-bit color as static mesh tint

                default:
                    {
                        var R = (float)Math.Floor(originalColor.X * 32.0f);
                        var G = (float)Math.Floor(originalColor.Y * 32.0f);
                        var B = (float)Math.Floor(originalColor.Z * 32.0f);
                        return new Vector4(R / 32.0f, G / 32.0f, B / 32.0f, 1.0f);
                    }
            }
        }

        // Stores the two endpoints of a vertical line going from `position` down to
        // the floor of `room`. DrawDebugLines builds the actual SolidLineVertex pair
        // on the fly each frame — no GPU buffer is allocated here.
        private void AddObjectHeightLine(Room room, Vector3 position)
        {
            int floorHeight = GetFloorHeight(room, position);
            _heightLineFrom = position;
            _heightLineTo = new Vector3(position.X, floorHeight, position.Z);
            _drawHeightLine = true;
        }

        // Builds the flyby path geometry (a tube of triangles) into the reusable
        // _flybyPathVertices list. Returns false if the path is too short to draw.
        // Caller (DrawFlybyPath) hands the list to the unified RenderingDrawingLines
        // batch — no GPU buffer is allocated here.
        private bool AddFlybyPath(int sequence)
        {
            var flybyCameras = FlybySequenceHelper.GetCameras(_editor.Level, sequence);

            if (flybyCameras.Count < 2)
                return false;

            var vertices = _flybyPathVertices;
            vertices.Clear();
            var startColor = MathC.GetRandomColorByIndex(sequence, 32, 0.7f);
            var endColor = MathC.GetRandomColorByIndex(sequence, 32, 0.3f);

            float th = _flybyPathThickness;

            // Process flyby cameras to calculate paths
            var camList = new List<Vector3>();
            for (int i = 0; i < flybyCameras.Count; i++)
            {
                var cam = flybyCameras[i];
                camList.Add(cam.Position + cam.Room.WorldPos);

                // Check for a sequence cut and jump to appropriate camera, if setup is correct
                bool isCut = FlybySequenceHelper.TryResolveCutTargetIndex(flybyCameras, i, out int targetIndex);

                if (isCut)
                    i = targetIndex - 1;

                // Check for the end of the list
                bool isLast = i == flybyCameras.Count - 1;

                if (isCut || isLast)
                {
                    // Calculate the spline path for the current segment
                    var pointList = CatmullRomSpline.EvaluatePositions(camList, _flybyPathSmoothness);

                    // Add vertices for the current path segment
                    for (int j = 0; j < pointList.Count - 1; j++)
                    {
                        var color = Vector4.Lerp(startColor, endColor, j / (float)pointList.Count);
                        var points = new List<Vector3[]>()
                        {
                            new Vector3[]
                            {
                                pointList[j],
                                new Vector3(pointList[j].X + th, pointList[j].Y + th, pointList[j].Z + th),
                                new Vector3(pointList[j].X - th, pointList[j].Y + th, pointList[j].Z + th)
                            },
                            new Vector3[]
                            {
                                pointList[j + 1],
                                new Vector3(pointList[j + 1].X + th, pointList[j + 1].Y + th, pointList[j + 1].Z + th),
                                new Vector3(pointList[j + 1].X - th, pointList[j + 1].Y + th, pointList[j + 1].Z + th)
                            }
                        };

                        for (int k = 0; k < _flybyPathIndices.Count; k++)
                        {
                            vertices.Add(new SolidLineVertex
                            {
                                Position = points[_flybyPathIndices[k].Y][_flybyPathIndices[k].X],
                                Color = color
                            });
                        }
                    }

                    // Reset camList for the next segment
                    camList.Clear();

                    // If it's not the last camera, add the current camera as the start of the next segment
                    if (!isCut && !isLast)
                        camList.Add(cam.Position + cam.Room.WorldPos);
                }
            }

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
