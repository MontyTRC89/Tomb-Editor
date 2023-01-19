﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private Room GetCurrentRoom()
        {
            foreach (var room in _editor.Level.Rooms)
            {
                if (room == null)
                    continue;

                Vector3 p = Camera.GetPosition();
                BoundingBox b = room.WorldBoundingBox;

                if (p.X >= b.Minimum.X && p.Y >= b.Minimum.Y && p.Z >= b.Minimum.Z &&
                    p.X <= b.Maximum.X && p.Y <= b.Maximum.Y && p.Z <= b.Maximum.Z &&
                    _editor.SelectedRoom.IsAlternate == room.IsAlternate)
                {
                    return room;
                }
            }

            return null;
        }

        private static float GetFloorHeight(Room room, Vector3 position)
        {
            int xBlock = (int)Math.Max(0, Math.Min(room.NumXSectors - 1, Math.Floor(position.X / Level.BlockSizeUnit)));
            int zBlock = (int)Math.Max(0, Math.Min(room.NumZSectors - 1, Math.Floor(position.Z / Level.BlockSizeUnit)));

            // Get the base floor height
            return room.Blocks[xBlock, zBlock].Floor.Min * Level.HeightUnit;
        }

        private Vector4 ConvertColor(Vector3 originalColor)
        {
            switch (_editor.Level.Settings.GameVersion)
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

        private void AddObjectHeightLine(Room room, Vector3 position)
        {
            float floorHeight = GetFloorHeight(room, position);

            // Get the distance between point and floor in units
            float height = position.Y - floorHeight;

            // Prepare two vertices for the line
            var vertices = new[]
            {
                new SolidVertex { Position = position, Color = Vector4.One },
                new SolidVertex { Position = new Vector3(position.X, floorHeight, position.Z), Color = Vector4.One }
            };

            // Prepare the Vertex Buffer
            if (_objectHeightLineVertexBuffer != null)
                _objectHeightLineVertexBuffer.Dispose();
            _objectHeightLineVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New(_legacyDevice,
                vertices, SharpDX.Direct3D11.ResourceUsage.Dynamic);

            _drawHeightLine = true;
        }

        private bool AddFlybyPath(int sequence)
        {
            // Collect all flyby cameras
            List<FlybyCameraInstance> flybyCameras = new List<FlybyCameraInstance>();

            foreach (var room in _editor.Level.ExistingRooms)
                foreach (var instance in room.Objects.OfType<FlybyCameraInstance>())
                {
                    if (instance.Sequence == sequence)
                        flybyCameras.Add(instance);
                }

            // Is it actually necessary to show the path?
            if (flybyCameras.Count < 2)
                return false;

            // Sort cameras
            flybyCameras.Sort((x, y) => x.Number.CompareTo(y.Number));

            // Calculate spline path
            var camList = flybyCameras.Select(cam => cam.Position + cam.Room.WorldPos).ToList();
            var pointList = Spline.Calculate(camList, flybyCameras.Count * _flybyPathSmoothness);

            // Construct vertex array
            List<SolidVertex> vertices = new List<SolidVertex>();

            var startColor = new Vector4(0.8f, 1.0f, 0.8f, 1.0f);
            var endColor = new Vector4(1.0f, 0.8f, 0.8f, 1.0f);

            float th = _flybyPathThickness;
            for (int i = 0; i < pointList.Count - 1; i++)
            {
                var color = Vector4.Lerp(startColor, endColor, i / (float)pointList.Count);

                var points = new List<Vector3[]>()
                {
                    new Vector3[]
                    {
                        pointList[i],
                        new Vector3(pointList[i].X + th, pointList[i].Y + th, pointList[i].Z + th),
                        new Vector3(pointList[i].X - th, pointList[i].Y + th, pointList[i].Z + th)
                    },
                    new Vector3[]
                    {
                        pointList[i],
                        new Vector3(pointList[i + 1].X + th, pointList[i + 1].Y + th, pointList[i + 1].Z + th),
                        new Vector3(pointList[i + 1].X - th, pointList[i + 1].Y + th, pointList[i + 1].Z + th)
                    }
                };

                for (int j = 0; j < _flybyPathIndices.Count; j++)
                {
                    var v = new SolidVertex();
                    v.Position = points[_flybyPathIndices[j].Y][_flybyPathIndices[j].X];
                    v.Color = color;
                    vertices.Add(v);
                }
            }

            // Prepare the Vertex Buffer
            if (_flybyPathVertexBuffer != null)
                _flybyPathVertexBuffer.Dispose();
            _flybyPathVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New(_legacyDevice, vertices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);

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
