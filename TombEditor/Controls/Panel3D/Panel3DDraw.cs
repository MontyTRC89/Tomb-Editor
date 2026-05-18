using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;

using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;
using TombLib.LevelData.SectorStructs;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
// Resolves the BlendMode name conflict between the rendering layer (this is what we
// want here) and TombLib.Utils.BlendMode (the TR engine's texture blend mode).
using BlendMode = TombLib.Rendering.BlendMode;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        // Both branches (height line + room bounding boxes) now go through the unified
        // RenderingDrawingLines path. The `object effect` parameter is no longer used
        // and is kept only to minimize the diff at the call site (DrawScene).
        private void DrawDebugLines(object effect)
        {
            var drawRoomBounds = _editor.Configuration.Rendering3D_AlwaysShowCurrentRoomBounds;

            if (!_drawHeightLine && !drawRoomBounds)
                return;

            if (_drawHeightLine)
            {
                Span<SolidLineVertex> verts = stackalloc SolidLineVertex[2];
                verts[0] = new SolidLineVertex { Position = _heightLineFrom, Color = Vector4.One };
                verts[1] = new SolidLineVertex { Position = _heightLineTo,   Color = Vector4.One };
                _linesBatch.SetVertices(verts);
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = _renderingStateBuffer,
                    World = Matrix4x4.CreateTranslation(_editor.SelectedObject.Room.WorldPos),
                });
            }

            if (!_flyModeTimer.Enabled && drawRoomBounds)
            {
                _bboxBatchVertices.Clear();
                if (_editor.SelectedRooms.Count > 0)
                    foreach (Room room in _editor.SelectedRooms)
                        AppendRoomBoundingBox(_bboxBatchVertices, room);
                else
                    AppendRoomBoundingBox(_bboxBatchVertices, _editor.SelectedRoom);

                if (_bboxBatchVertices.Count > 0)
                {
                    _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_bboxBatchVertices));
                    _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                    {
                        RenderTarget = SwapChain,
                        StateBuffer = _renderingStateBuffer,
                    });
                }
            }
        }

        // Computes the AABB of `room` and appends its 24 wireframe line vertices to the
        // batch. Caller is responsible for batching multiple rooms into a single draw.
        private static void AppendRoomBoundingBox(List<SolidLineVertex> vertices, Room room)
        {
            float minY = room.WorldPos.Y + room.GetLowestCorner();
            float maxY = room.WorldPos.Y + room.GetHighestCorner();
            float sizeX = room.NumXSectors * Level.SectorSizeUnit;
            float sizeY = maxY - minY;
            float sizeZ = room.NumZSectors * Level.SectorSizeUnit;

            var world = Matrix4x4.CreateScale(sizeX * 0.5f, sizeY * 0.5f, sizeZ * 0.5f)
                      * Matrix4x4.CreateTranslation(
                            room.WorldPos.X + sizeX * 0.5f,
                            (minY + maxY) * 0.5f,
                            room.WorldPos.Z + sizeZ * 0.5f);

            WireGeometry.AppendWireCube(vertices, world, Vector4.One);
        }

        private void DrawText(Room[] roomsToDraw, List<Text> textToDraw)
        {
            // Draw room names
            if (ShowRoomNames)
            {
                Size size = ClientSize;
                for (int i = 0; i < roomsToDraw.Length; i++)
                {
                    var pos = (Matrix4x4.CreateTranslation(roomsToDraw[i].WorldPos) * _viewProjection).TransformPerspectively(roomsToDraw[i].GetLocalCenter());
                    if (pos.Z <= 1.0f)
                        textToDraw.Add(new Text
                        {
                            Font = _fontDefault,
                            Pos = pos.To2(),
                            Overlay = _editor.Configuration.Rendering3D_DrawFontOverlays,
                            String = roomsToDraw[i].Name
                        });
                }
            }

            // Draw North, South, East and West
            if (ShowCardinalDirections)
                DrawCardinalDirections(textToDraw);

            // Construct debug string
            string DebugString = "";
            if (_editor.Configuration.Rendering3D_ShowFPS)
                DebugString += "FPS: " + Math.Round(1.0f / _watch.Elapsed.TotalSeconds, 2) + "\n";

            if (_editor.SelectedObject != null)
                DebugString += "Selected Object: " + _editor.SelectedObject.ToShortString();

            // Draw debug string
            textToDraw.Add(new Text
            {
                Font = _fontDefault,
                PixelPos = new Vector2(10, -10),
                Alignment = new Vector2(0.0f, 0.0f),
                Overlay = _editor.Configuration.Rendering3D_DrawFontOverlays,
                String = DebugString
            });

            // If multiple objects are selected, display multiselection label
            var activeObjectGroup = _editor.SelectedObject as ObjectGroup;
            if (activeObjectGroup != null)
            {
                // Add text message
                textToDraw.Add(CreateTextTagForObject(
                    activeObjectGroup.RotationPositionMatrix * _viewProjection,
                    $"Group of {activeObjectGroup.Count()} objects" +
                    "\n" + GetObjectPositionString(activeObjectGroup.Room, activeObjectGroup)));
            }

            // Finish strings
            SwapChain.RenderText(textToDraw);
        }

        // Bounding boxes for moveables and statics: legacy code did one indexed Draw
        // per object (N draw calls + N effect parameter changes). Migrated path bakes
        // every object's 12-edge wireframe into ONE SolidLineVertex list with per-vertex
        // colour, so the whole list of objects becomes a single LineList draw call.
        //
        // Wireframe topology
        // ------------------
        // 12 edges × 2 vertices = 24 line vertices per box. The 8 cube corners are
        // first transformed by the per-object matrix (scale × translation × rot/pos),
        // then emitted in pairs for each edge.
        private void DrawBoundingBoxes(object solidEffect, List<ObjectInstance> objectList)
        {
            if (objectList.Count == 0)
                return;

            // Reuse a single list across calls — caller is on the UI thread, so this
            // is single-threaded by construction.
            _bboxBatchVertices.Clear();
            var selectionColor = _editor.Configuration.UI_ColorScheme.ColorSelection;
            var defaultColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);

            foreach (var obj in objectList)
            {
                Matrix4x4 worldMatrix;
                if (obj is MoveableInstance mov)
                {
                    var model = _editor?.Level?.Settings?.WadTryGetMoveable(mov.WadObjectId);
                    if (model == null || model.Animations.Count == 0 || model.Animations[0].KeyFrames.Count == 0)
                        continue;
                    var frame = model.Animations[0].KeyFrames[0];
                    // _linesCube was a -128..+128 cube; legacy scaled it down by /256
                    // to get actual size. We use a unit cube directly so the scale is
                    // simply the bounding box size (no /256 factor).
                    worldMatrix = Matrix4x4.CreateScale(frame.BoundingBox.Size / 2.0f) *
                                  Matrix4x4.CreateTranslation(frame.BoundingBox.Center) *
                                  mov.RotationPositionMatrix;
                }
                else if (obj is StaticInstance stat)
                {
                    var mesh = _editor?.Level?.Settings?.WadTryGetStatic(stat.WadObjectId);
                    if (mesh == null || mesh.Mesh == null || mesh.Mesh.BoundingBox.Size.Length() == 0.0f)
                        continue;
                    worldMatrix = Matrix4x4.CreateScale(mesh.CollisionBox.Size * stat.Scale / 2.0f) *
                                  Matrix4x4.CreateTranslation(mesh.CollisionBox.Center * stat.Scale) *
                                  stat.RotationPositionMatrix;
                }
                else
                    continue;

                var color = _highlightedObjects.Contains(obj) ? selectionColor : defaultColor;
                WireGeometry.AppendWireCube(_bboxBatchVertices, worldMatrix, color);
            }

            if (_bboxBatchVertices.Count == 0)
                return;

            _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_bboxBatchVertices));
            _linesBatch.Render(new RenderingDrawingLines.RenderArgs
            {
                RenderTarget = SwapChain,
                StateBuffer = _renderingStateBuffer,
                Topology = RenderingDrawingLines.Topology.LineList,
            });
        }

        // Reused across DrawBoundingBoxes calls to avoid per-frame allocation.
        private readonly List<SolidLineVertex> _bboxBatchVertices = new List<SolidLineVertex>();

        // Flyby path is a tube of triangles drawn along the spline through the flyby
        // cameras. Vertex generation lives in AddFlybyPath; this method only draws.
        // Vertices are already in world space (no World matrix needed).
        private void DrawFlybyPath(object effect)
        {
            if (!TryGetSelectedFlybySequence(out int sequence))
                return;

            if (!AddFlybyPath(sequence))
                return;

            _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_flybyPathVertices));
            _linesBatch.Render(new RenderingDrawingLines.RenderArgs
            {
                RenderTarget = SwapChain,
                StateBuffer = _renderingStateBuffer,
                Topology = RenderingDrawingLines.Topology.TriangleList,
            });
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

        // Triangle ribbons highlighting the sector-split lines on the floor/ceiling.
        // Each ribbon is 6 vertices (2 triangles); total per call ~dozens of ribbons.
        // Migrated to the unified RenderingDrawingLines path with TriangleList topology;
        // the legacy SolidVertex / per-call Buffer<SolidVertex> path is gone.
        private void DrawSectorSplitHighlights(object effect)
        {
            if (_editor.HighlightedSplit == 0 || _editor.SelectedSectors == SectorSelection.None)
                return;

            int splitIndex = _editor.HighlightedSplit - 2;
            Room currentRoom = _editor.SelectedRoom;

            var vertices = new List<SolidLineVertex>();

            const int
                XZ_OFFSET = 8,
                HEIGHT = 24;

            void DrawRibbon(Vector3 p1, Vector3 p2, int height, int xOffset, int yOffset, int zOffset)
            {
                float halfHeight = height / 2.0f;
                var c = Vector4.One;
                Vector3 p1Top = new Vector3((p1.X * Level.SectorSizeUnit) + xOffset, p1.Y + halfHeight + yOffset, (p1.Z * Level.SectorSizeUnit) + zOffset);
                Vector3 p2Top = new Vector3((p2.X * Level.SectorSizeUnit) + xOffset, p2.Y + halfHeight + yOffset, (p2.Z * Level.SectorSizeUnit) + zOffset);
                Vector3 p1Bot = new Vector3((p1.X * Level.SectorSizeUnit) + xOffset, p1.Y - halfHeight + yOffset, (p1.Z * Level.SectorSizeUnit) + zOffset);
                Vector3 p2Bot = new Vector3((p2.X * Level.SectorSizeUnit) + xOffset, p2.Y - halfHeight + yOffset, (p2.Z * Level.SectorSizeUnit) + zOffset);

                vertices.Add(new SolidLineVertex { Position = p1Top, Color = c });
                vertices.Add(new SolidLineVertex { Position = p2Top, Color = c });
                vertices.Add(new SolidLineVertex { Position = p1Bot, Color = c });

                vertices.Add(new SolidLineVertex { Position = p1Bot, Color = c });
                vertices.Add(new SolidLineVertex { Position = p2Top, Color = c });
                vertices.Add(new SolidLineVertex { Position = p2Bot, Color = c });
            }

            void HandlePositiveZ(int x, int z, SectorSurface surface, int yOffset)
            {
                if (surface.DiagonalSplit is DiagonalSplit.XpZn or DiagonalSplit.XnZn)
                    return;

                Vector3
                    p1 = new Vector3(x + 1, surface.XpZp + yOffset, z + 1) + currentRoom.Position,
                    p2 = new Vector3(x, surface.XnZp + yOffset, z + 1) + currentRoom.Position;

                DrawRibbon(p1, p2, HEIGHT, 0, 0, XZ_OFFSET);
            }

            void HandlePositiveX(int x, int z, SectorSurface surface, int yOffset)
            {
                if (surface.DiagonalSplit is DiagonalSplit.XnZp or DiagonalSplit.XnZn)
                    return;

                Vector3
                    p1 = new Vector3(x + 1, surface.XpZn + yOffset, z) + currentRoom.Position,
                    p2 = new Vector3(x + 1, surface.XpZp + yOffset, z + 1) + currentRoom.Position;

                DrawRibbon(p1, p2, HEIGHT, XZ_OFFSET, 0, 0);
            }

            void HandleNegativeZ(int x, int z, SectorSurface surface, int yOffset)
            {
                if (surface.DiagonalSplit is DiagonalSplit.XpZp or DiagonalSplit.XnZp)
                    return;

                Vector3
                    p1 = new Vector3(x, surface.XnZn + yOffset, z) + currentRoom.Position,
                    p2 = new Vector3(x + 1, surface.XpZn + yOffset, z) + currentRoom.Position;

                DrawRibbon(p1, p2, HEIGHT, 0, 0, -XZ_OFFSET);
            }

            void HandleNegativeX(int x, int z, SectorSurface surface, int yOffset)
            {
                if (surface.DiagonalSplit is DiagonalSplit.XpZn or DiagonalSplit.XpZp)
                    return;

                Vector3
                    p1 = new Vector3(x, surface.XnZp + yOffset, z + 1) + currentRoom.Position,
                    p2 = new Vector3(x, surface.XnZn + yOffset, z) + currentRoom.Position;

                DrawRibbon(p1, p2, HEIGHT, -XZ_OFFSET, 0, 0);
            }

            void HandleDiagonal(int x, int z, SectorSurface surface, int yOffset)
            {
                Vector3 p1, p2;

                switch (surface.DiagonalSplit)
                {
                    case DiagonalSplit.XnZp:
                        p1 = new Vector3(x, surface.XnZn + yOffset, z) + currentRoom.Position;
                        p2 = new Vector3(x + 1, surface.XpZp + yOffset, z + 1) + currentRoom.Position;

                        DrawRibbon(p1, p2, HEIGHT, XZ_OFFSET, 0, -XZ_OFFSET);
                        break;

                    case DiagonalSplit.XpZp:
                        p1 = new Vector3(x, surface.XnZp + yOffset, z + 1) + currentRoom.Position;
                        p2 = new Vector3(x + 1, surface.XpZn + yOffset, z) + currentRoom.Position;

                        DrawRibbon(p1, p2, HEIGHT, -XZ_OFFSET, 0, -XZ_OFFSET);
                        break;

                    case DiagonalSplit.XnZn:
                        p1 = new Vector3(x + 1, surface.XpZn + yOffset, z) + currentRoom.Position;
                        p2 = new Vector3(x, surface.XnZp + yOffset, z + 1) + currentRoom.Position;

                        DrawRibbon(p1, p2, HEIGHT, XZ_OFFSET, 0, XZ_OFFSET);
                        break;

                    case DiagonalSplit.XpZn:
                        p1 = new Vector3(x + 1, surface.XpZp + yOffset, z + 1) + currentRoom.Position;
                        p2 = new Vector3(x, surface.XnZn + yOffset, z) + currentRoom.Position;

                        DrawRibbon(p1, p2, HEIGHT, -XZ_OFFSET, 0, XZ_OFFSET);
                        break;
                }
            }

            for (int x = _editor.SelectedSectors.Area.X0; x <= _editor.SelectedSectors.Area.X1; x++)
                for (int z = _editor.SelectedSectors.Area.Y0; z <= _editor.SelectedSectors.Area.Y1; z++)
                {
                    Sector sector = currentRoom.Sectors[x, z],
                        targetSector = sector;

                    int yOffset = 0;

                    if (sector.WallPortal is not null)
                    {
                        RoomSectorPair pair = currentRoom.GetSectorTryThroughPortal(x, z);

                        if (pair.Room != currentRoom && pair.Sector is not null)
                        {
                            targetSector = pair.Sector;
                            yOffset = pair.Room.Position.Y - currentRoom.Position.Y;
                        }
                    }

                    if (splitIndex is < 0 or > 7) // QA or WS
                    {
                        // PositiveZ Floor
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_PositiveZ_QA)))
                            HandlePositiveZ(x, z, targetSector.Floor, yOffset);
                        // PositiveZ Ceiling
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_PositiveZ_WS)))
                            HandlePositiveZ(x, z, targetSector.Ceiling, yOffset);

                        // PositiveX Floor
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_PositiveX_QA)))
                            HandlePositiveX(x, z, targetSector.Floor, yOffset);
                        // PositiveX Ceiling
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_PositiveX_WS)))
                            HandlePositiveX(x, z, targetSector.Ceiling, yOffset);

                        // NegativeZ Floor
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_NegativeZ_QA)))
                            HandleNegativeZ(x, z, targetSector.Floor, yOffset);
                        // NegativeZ Ceiling
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_NegativeZ_WS)))
                            HandleNegativeZ(x, z, targetSector.Ceiling, yOffset);

                        // NegativeX Floor
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_NegativeX_QA)))
                            HandleNegativeX(x, z, targetSector.Floor, yOffset);
                        // NegativeX Ceiling
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_NegativeX_WS)))
                            HandleNegativeX(x, z, targetSector.Ceiling, yOffset);

                        // Diagonal Floor
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_Diagonal_QA)))
                            HandleDiagonal(x, z, targetSector.Floor, yOffset);
                        // Diagonal Ceiling
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_Diagonal_WS)))
                            HandleDiagonal(x, z, targetSector.Ceiling, yOffset);
                    }
                    else // Actual splits
                    {
                        // Floor split
                        if (splitIndex < targetSector.ExtraFloorSplits.Count)
                        {
                            var floorSurface = new SectorSurface
                            {
                                XnZp = targetSector.ExtraFloorSplits[splitIndex].XnZp,
                                XpZp = targetSector.ExtraFloorSplits[splitIndex].XpZp,
                                XpZn = targetSector.ExtraFloorSplits[splitIndex].XpZn,
                                XnZn = targetSector.ExtraFloorSplits[splitIndex].XnZn,
                                DiagonalSplit = targetSector.Floor.DiagonalSplit
                            };

                            // PositiveZ
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveZ, splitIndex))))
                                HandlePositiveZ(x, z, floorSurface, yOffset);

                            // PositiveX
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveX, splitIndex))))
                                HandlePositiveX(x, z, floorSurface, yOffset);

                            // NegativeZ
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeZ, splitIndex))))
                                HandleNegativeZ(x, z, floorSurface, yOffset);

                            // NegativeX
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeX, splitIndex))))
                                HandleNegativeX(x, z, floorSurface, yOffset);

                            // Diagonal
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.Diagonal, splitIndex))))
                                HandleDiagonal(x, z, floorSurface, yOffset);
                        }

                        // Ceiling split
                        if (splitIndex < targetSector.ExtraCeilingSplits.Count)
                        {
                            var ceilingSurface = new SectorSurface
                            {
                                XnZp = targetSector.ExtraCeilingSplits[splitIndex].XnZp,
                                XpZp = targetSector.ExtraCeilingSplits[splitIndex].XpZp,
                                XpZn = targetSector.ExtraCeilingSplits[splitIndex].XpZn,
                                XnZn = targetSector.ExtraCeilingSplits[splitIndex].XnZn,
                                DiagonalSplit = targetSector.Ceiling.DiagonalSplit
                            };

                            // PositiveZ
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveZ, splitIndex))))
                                HandlePositiveZ(x, z, ceilingSurface, yOffset);

                            // PositiveX
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveX, splitIndex))))
                                HandlePositiveX(x, z, ceilingSurface, yOffset);

                            // NegativeZ
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeZ, splitIndex))))
                                HandleNegativeZ(x, z, ceilingSurface, yOffset);

                            // NegativeX
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeX, splitIndex))))
                                HandleNegativeX(x, z, ceilingSurface, yOffset);

                            // Diagonal
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.Diagonal, splitIndex))))
                                HandleDiagonal(x, z, ceilingSurface, yOffset);
                        }
                    }
                }

            if (vertices.Count == 0)
                return;

            _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(vertices));
            _linesBatch.Render(new RenderingDrawingLines.RenderArgs
            {
                RenderTarget = SwapChain,
                StateBuffer = _renderingStateBuffer,
                Topology = RenderingDrawingLines.Topology.TriangleList,
            });
        }

        // Lights rendering, fully migrated to RenderingDrawingLines.
        //
        // Two batches accumulate independently:
        //   _lightsBatchVertices     — small placeholder spheres for every light in
        //                               every room (only used when sprite icons are
        //                               disabled). All lights collapse into 1 draw.
        //   _selectedLightVertices   — large overlay (range spheres, projection cones)
        //                               for the currently selected light only. Always 1
        //                               draw regardless of light type.
        //
        // Sprite icons are rendered via DrawOrQueueServiceObject which still appends
        // to the global `sprites` list — the legacy code there is left untouched.
        private void DrawLights(object effect, Room[] roomsWhoseObjectsToDraw, List<Text> textToDraw, List<Sprite> sprites)
        {
            var lights = roomsWhoseObjectsToDraw.SelectMany(r => r.Objects).OfType<LightInstance>();
            bool useSpriteIcons = _editor.Configuration.Rendering3D_UseSpritesForServiceObjects;

            // === Pass 1: per-light placeholder ===
            _lightsBatchVertices.Clear();
            foreach (var light in lights)
            {
                var color = ColorForLightType(light);
                if (_highlightedObjects.Contains(light))
                    color = _editor.Configuration.UI_ColorScheme.ColorSelection;

                if (useSpriteIcons)
                {
                    // Sprite path bypasses 3D placeholder geometry entirely.
                    DrawOrQueueServiceObject(light, null, color, effect, sprites);
                }
                else
                {
                    // Wireframe sphere placeholder. Size matches the legacy _littleSphere
                    // (tessellation 8, half-size 128) — see Panel3DInit. Emit a fully
                    // UV-tessellated sphere as TRIANGLES; the batch is submitted with
                    // Wireframe=true so the rasterizer fills only the triangle edges,
                    // reproducing the legacy "tessellated wire sphere" look (instead of
                    // the three-great-circles silhouette that AppendWireSphere produces).
                    var world = Matrix4x4.CreateScale(_littleSphereRadius) * Matrix4x4.CreateTranslation(light.Position + light.Room.WorldPos);
                    WireGeometry.AppendSolidSphere(_lightsBatchVertices, world, color, latSegments: 8, longSegments: 12);
                }
            }

            // === Pass 2: selected light range overlay ===
            _selectedLightVertices.Clear();
            if (_editor.SelectedObject is LightInstance selectedLight && lights.Contains(selectedLight))
            {
                if (ShowLightMeshes)
                    AppendSelectedLightOverlay(selectedLight);

                textToDraw.Add(CreateTextTagForObject(
                    selectedLight.ObjectMatrix * _viewProjection,
                    selectedLight.Type.ToString().SplitCamelcase() + " Light" + "\n" + GetObjectPositionString(selectedLight.Room, selectedLight)));

                AddObjectHeightLine(selectedLight.Room, selectedLight.Position);
            }

            // === Submit batches ===
            // Light rings (radius/spot cones) draw on top of world geometry —
            // depth test disabled so they're visible through walls.
            if (_lightsBatchVertices.Count > 0)
            {
                _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_lightsBatchVertices));
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = _renderingStateBuffer,
                    Topology = RenderingDrawingLines.Topology.TriangleList,
                    Wireframe = true,
                    Depth = DepthMode.NoZ,
                });
            }
            if (_selectedLightVertices.Count > 0)
            {
                _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_selectedLightVertices));
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = _renderingStateBuffer,
                    Depth = DepthMode.NoZ,
                });
            }
        }

        // Reused list; cleared+filled per frame.
        private readonly List<SolidLineVertex> _lightsBatchVertices = new List<SolidLineVertex>();
        private readonly List<SolidLineVertex> _selectedLightVertices = new List<SolidLineVertex>();
        private readonly List<SolidLineVertex> _placeholderBatchVertices = new List<SolidLineVertex>();

        // Sprite-or-wirecube placeholder for service objects (sinks, cameras, sound
        // sources, etc.). Mirrors the behaviour of DrawOrQueueServiceObject minus the
        // legacy mesh draw — caller accumulates wire cubes into _placeholderBatchVertices
        // for a single batched draw.
        private void QueueServiceObjectPlaceholder(ISpatial instance, Vector4 color, List<Sprite> sprites)
        {
            if (_editor.CameraPreviewMode != CameraPreviewType.None)
                return;

            // Sprite mode: defer to the existing sprite-only branch of
            // DrawOrQueueServiceObject. The Effect parameter is unused on that branch.
            if (_editor.Configuration.Rendering3D_UseSpritesForServiceObjects)
            {
                DrawOrQueueServiceObject(instance, null, color, null, sprites);
                return;
            }

            Matrix4x4 transform;
            if (instance is PositionBasedObjectInstance pbi)
                transform = pbi.RotationPositionMatrix;
            else if (instance is GhostBlockInstance gbi)
                transform = gbi.CenterMatrix(true);
            else
                return;

            // _littleCube was a 256-unit cube (half-size = _littleCubeRadius = 128); a
            // unit cube (-1..+1) scaled by the radius reproduces the same world size.
            var world = Matrix4x4.CreateScale(_littleCubeRadius) * transform;
            WireGeometry.AppendWireCube(_placeholderBatchVertices, world, color);
        }

        private static Vector4 ColorForLightType(LightInstance light) => light.Type switch
        {
            LightType.Point   => new Vector4(1.0f, 1.0f, 0.25f, 1.0f),
            LightType.Spot    => new Vector4(1.0f, 1.0f, 0.25f, 1.0f),
            LightType.FogBulb => new Vector4(1.0f, 0.0f, 1.0f,  1.0f),
            LightType.Shadow  => new Vector4(0.5f, 0.5f, 0.5f,  1.0f),
            LightType.Effect  => new Vector4(1.0f, 1.0f, 0.25f, 1.0f),
            LightType.Sun     => new Vector4(1.0f, 0.5f, 0.0f,  1.0f),
            _                 => Vector4.One,
        };

        // Range/projection visualization for the selected light. Each branch matches the
        // legacy semantics 1:1 (same scale formulas, same colours: green = inner, blue = outer).
        private void AppendSelectedLightOverlay(LightInstance light)
        {
            var greenColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            var blueColor  = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

            switch (light.Type)
            {
                case LightType.Point:
                case LightType.Shadow:
                case LightType.FogBulb:
                    // Inner range only for Point/Shadow (FogBulb has no inner).
                    if (light.Type == LightType.Point || light.Type == LightType.Shadow)
                    {
                        var inner = Matrix4x4.CreateScale(light.InnerRange * 1024.0f /* TR units */) * light.ObjectMatrix;
                        WireGeometry.AppendWireSphere(_selectedLightVertices, inner, greenColor, segments: 32);
                    }
                    var outer = Matrix4x4.CreateScale(light.OuterRange * 1024.0f) * light.ObjectMatrix;
                    WireGeometry.AppendWireSphere(_selectedLightVertices, outer, blueColor, segments: 32);
                    break;

                case LightType.Spot:
                    {
                        // Cone unit length convention in the editor: 1024 TR units.
                        // coneAngle = atan2(512, 1024) is the legacy primitive's half-angle.
                        const float coneUnit = 1024.0f;
                        float coneAngle = (float)Math.Atan2(512, 1024);
                        // Inner cone (green)
                        float lenH = light.InnerRange * coneUnit;
                        float lenW = light.InnerAngle * (float)(Math.PI / 180) / coneAngle * lenH * 0.5f;
                        var innerWorld = Matrix4x4.CreateScale(lenW, lenW, lenH) * light.ObjectMatrix;
                        WireGeometry.AppendWireCone(_selectedLightVertices, innerWorld, greenColor);
                        // Outer cone (blue)
                        float cutoffH = light.OuterRange * coneUnit;
                        float cutoffW = light.OuterAngle * (float)(Math.PI / 180) / coneAngle * cutoffH * 0.5f;
                        var outerWorld = Matrix4x4.CreateScale(cutoffW, cutoffW, cutoffH) * light.ObjectMatrix;
                        WireGeometry.AppendWireCone(_selectedLightVertices, outerWorld, blueColor);
                    }
                    break;

                case LightType.Sun:
                    {
                        // Long thin cone showing direction. Matches the legacy
                        // CreateScale(0.01, 0.01, 1.0) on a primitive with base radius
                        // 512 and length 1024 → effective 5.12 wide, 1024 long.
                        var world = Matrix4x4.CreateScale(5.12f, 5.12f, 1024.0f) * light.ObjectMatrix;
                        WireGeometry.AppendWireCone(_selectedLightVertices, world, greenColor);
                    }
                    break;
            }
        }

        // Center placeholders + corner control cubes for ghost blocks. Migrated to a
        // single accumulated wire-cube batch:
        //   - Non-selected ghost block: 1 wire cube at the block center (default colour).
        //   - Selected ghost block: 8 wire cubes at the corner control matrices
        //                           (4 floor + 4 ceiling); the SelectedCorner is shown
        //                           in a brighter highlight color (the legacy used
        //                           wireframe rasterizer for the same purpose; with our
        //                           always-wire path we use color instead).
        // The animated unfold (`_movementTimer.Mode == GhostBlockUnfold`) is preserved
        // by lerping between center and corner matrices like the legacy code.
        private void DrawGhostBlocks(object effect, List<GhostBlockInstance> ghostBlocksToDraw, List<Text> textToDraw, List<Sprite> sprites)
        {
            if (ghostBlocksToDraw.Count == 0)
                return;

            var baseColor = _editor.Configuration.UI_ColorScheme.ColorFloor;
            var normalColor = new Vector4(baseColor.To3() * 0.4f, 0.9f);
            var selectColor = new Vector4(baseColor.To3() * 0.5f, 1.0f);
            // Highlight color for the corner currently being grabbed/dragged. Brightens
            // the selection color so the user sees which corner is hot.
            var cornerHighlight = new Vector4(System.Numerics.Vector3.Min(selectColor.To3() * 1.6f, System.Numerics.Vector3.One), 1.0f);

            _ghostBlocksBatchVertices.Clear();

            foreach (var instance in ghostBlocksToDraw)
            {
                bool isSelected = _editor.SelectedObject == instance;

                if (isSelected)
                {
                    textToDraw.Add(CreateTextTagForObject(
                        instance.CenterMatrix(instance.SelectedFloor) * _viewProjection,
                        instance.InfoMessage()));

                    // 4 floor corners + 4 ceiling corners.
                    for (int f = 0; f < 2; f++)
                    {
                        bool floor = f == 0;
                        for (int j = 0; j < 4; j++)
                        {
                            Matrix4x4 cornerMatrix;
                            if (_movementTimer.Mode == AnimationMode.GhostBlockUnfold && !instance.SelectedCorner.HasValue)
                                cornerMatrix = Matrix4x4.Lerp(instance.CenterMatrix(true), instance.ControlMatrixes(floor)[j], _movementTimer.MoveMultiplier);
                            else
                                cornerMatrix = instance.ControlMatrixes(floor)[j];

                            bool isThisCornerSelected = instance.SelectedCorner.HasValue
                                && (int)instance.SelectedCorner.Value == j
                                && instance.SelectedFloor == floor;
                            var color = isThisCornerSelected ? cornerHighlight : selectColor;

                            // ControlMatrixes already encode the per-corner translation
                            // and scale appropriate for a unit cube — the legacy code
                            // bound _littleCube (256u) directly without extra scale, so
                            // the matrix is calibrated for a 256u cube. Our unit cube
                            // (-1..+1, span 2) needs an extra ×128 scale to match.
                            var world = Matrix4x4.CreateScale(_littleCubeRadius) * cornerMatrix;
                            WireGeometry.AppendWireCube(_ghostBlocksBatchVertices, world, color);
                        }
                    }
                }
                else
                {
                    // Non-selected: single center cube. Reuse the placeholder helper
                    // so that the sprite-icon mode is honoured here too.
                    QueueServiceObjectPlaceholderInto(instance, normalColor, sprites, _ghostBlocksBatchVertices);
                }
            }

            if (_ghostBlocksBatchVertices.Count == 0)
                return;

            _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_ghostBlocksBatchVertices));
            _linesBatch.Render(new RenderingDrawingLines.RenderArgs
            {
                RenderTarget = SwapChain,
                StateBuffer = _renderingStateBuffer,
            });
        }

        private readonly List<SolidLineVertex> _ghostBlocksBatchVertices = new List<SolidLineVertex>();

        // Same as QueueServiceObjectPlaceholder but writes into a caller-supplied list
        // (so each pass/group can have its own batch with different state).
        private void QueueServiceObjectPlaceholderInto(ISpatial instance, Vector4 color, List<Sprite> sprites, List<SolidLineVertex> batch)
        {
            if (_editor.CameraPreviewMode != CameraPreviewType.None)
                return;
            if (_editor.Configuration.Rendering3D_UseSpritesForServiceObjects)
            {
                DrawOrQueueServiceObject(instance, null, color, null, sprites);
                return;
            }
            Matrix4x4 transform;
            if (instance is PositionBasedObjectInstance pbi)
                transform = pbi.RotationPositionMatrix;
            else if (instance is GhostBlockInstance gbi)
                transform = gbi.CenterMatrix(true);
            else
                return;
            var world = Matrix4x4.CreateScale(_littleCubeRadius) * transform;
            WireGeometry.AppendWireCube(batch, world, color);
        }

        // Translucent body of each ghost block: 84 triangles per block (4 sides ×
        // (4 quads + diagonal step + diagonal triangle) totaling 78–84 verts).
        // Migrated to RenderingDrawingLines with TriangleList topology, NonPremultiplied
        // blending and DepthRead so blocks fade behind opaque geometry without writing
        // to depth (matches legacy `_legacyDevice.SetDepthStencilState(DepthRead)`).
        // All ghost blocks accumulate into a single batch — N draw calls become 1.
        private void DrawGhostBlockBodies(object effect, List<GhostBlockInstance> ghostBlocksToDraw)
        {
            if (ghostBlocksToDraw.Count == 0)
                return;

            var baseColor = _editor.Configuration.UI_ColorScheme.ColorFloor;
            var normalColor = new Vector4(baseColor.To3() * 0.4f, 0.9f);
            var selectColor = new Vector4(baseColor.To3() * 0.5f, 1.0f);

            _ghostBlockBodyVertices.Clear();

            foreach (var instance in ghostBlocksToDraw)
            {
                var selected = _editor.SelectedObject == instance;

                if (!instance.Valid)
                    continue;

                // Create a vertex array
                SolidVertex[] vtxs = new SolidVertex[84]; // 78 with diagonal steps

                // Derive base sector colours
                var p1c = new Vector4(baseColor.To3() * (selected ? 0.8f : 0.4f), selected ? 0.7f : 0.5f);
                var p2c = new Vector4(baseColor.To3() * (selected ? 0.5f : 0.2f), selected ? 0.7f : 0.5f);

                // Fill it up
                for (int f = 0, c = 0; f < 2; f++)
                {
                    bool floor = f == 0;

                    if (floor && !instance.ValidFloor || !floor && !instance.ValidCeiling)
                        continue;

                    var split = floor ? instance.Sector.Floor.DiagonalSplit : instance.Sector.Ceiling.DiagonalSplit;
                    bool toggled = floor ? instance.FloorSplitToggled : instance.CeilingSplitToggled;
                    var vPos = instance.ControlPositions(floor, false);
                    var vOrg = instance.ControlPositions(floor, true);

                    bool[] shift = new bool[4];
                    shift[0] = split == DiagonalSplit.XpZp || split == DiagonalSplit.XpZn;
                    shift[1] = split == DiagonalSplit.XpZp || split == DiagonalSplit.XnZp;
                    shift[2] = split == DiagonalSplit.XnZn || split == DiagonalSplit.XnZp;
                    shift[3] = split == DiagonalSplit.XnZn || split == DiagonalSplit.XpZn;

                    for (int i = 0; i < 4; i++)
                    {
                        Vector3[] fPos = new Vector3[4];

                        switch (i)
                        {
                            case 0: // Xn
                                fPos[0] = vOrg[0];
                                fPos[1] = vOrg[3];
                                fPos[2] = vPos[3];
                                fPos[3] = vPos[0];
                                if (shift[i])
                                    if (split == DiagonalSplit.XpZp)
                                    {
                                        fPos[0].Y = vOrg[3].Y;
                                        fPos[3].Y = (vOrg[3] + (vPos[0] - vOrg[0])).Y;
                                    }
                                    else
                                    {
                                        fPos[1].Y = vOrg[0].Y;
                                        fPos[2].Y = (vOrg[0] + (vPos[3] - vOrg[3])).Y;
                                    }
                                break;

                            case 1: // Zn
                                fPos[0] = vOrg[3];
                                fPos[1] = vOrg[2];
                                fPos[2] = vPos[2];
                                fPos[3] = vPos[3];
                                if (shift[i])
                                    if (split == DiagonalSplit.XnZp)
                                    {
                                        fPos[0].Y = vOrg[2].Y;
                                        fPos[3].Y = (vOrg[2] + (vPos[3] - vOrg[3])).Y;
                                    }
                                    else
                                    {
                                        fPos[1].Y = vOrg[3].Y;
                                        fPos[2].Y = (vOrg[3] + (vPos[2] - vOrg[2])).Y;
                                    }
                                break;

                            case 2: // Xp
                                fPos[0] = vOrg[2];
                                fPos[1] = vOrg[1];
                                fPos[2] = vPos[1];
                                fPos[3] = vPos[2];
                                if (shift[i])
                                    if (split == DiagonalSplit.XnZn)
                                    {
                                        fPos[0].Y = vOrg[1].Y;
                                        fPos[3].Y = (vOrg[1] + (vPos[2] - vOrg[2])).Y;
                                    }
                                    else
                                    {
                                        fPos[1].Y = vOrg[2].Y;
                                        fPos[2].Y = (vOrg[2] + (vPos[1] - vOrg[1])).Y;
                                    }
                                break;

                            case 3: // Zp
                                fPos[0] = vOrg[1];
                                fPos[1] = vOrg[0];
                                fPos[2] = vPos[0];
                                fPos[3] = vPos[1];
                                if (shift[i])
                                    if (split == DiagonalSplit.XpZn)
                                    {
                                        fPos[0].Y = vOrg[0].Y;
                                        fPos[3].Y = (vOrg[0] + (vPos[1] - vOrg[1])).Y;
                                    }
                                    else
                                    {
                                        fPos[1].Y = vOrg[1].Y;
                                        fPos[2].Y = (vOrg[1] + (vPos[0] - vOrg[0])).Y;
                                    }
                                break;
                        }

                        vtxs[c].Position = fPos[0]; vtxs[c].Color = p1c; c++;
                        vtxs[c].Position = fPos[1]; vtxs[c].Color = p1c; c++;
                        vtxs[c].Position = fPos[3]; vtxs[c].Color = p2c; c++;
                        vtxs[c].Position = fPos[1]; vtxs[c].Color = p1c; c++;
                        vtxs[c].Position = fPos[2]; vtxs[c].Color = p1c; c++;
                        vtxs[c].Position = fPos[3]; vtxs[c].Color = p2c; c++;
                    }

                    // Equality flags to further hide nonexistent triangle
                    bool[] equal = new bool[3];
                    int r = 0;

                    switch (split)
                    {
                        case DiagonalSplit.XpZn: r = 0; break;
                        case DiagonalSplit.XnZn: r = 1; break;
                        case DiagonalSplit.XnZp: r = 2; break;
                        case DiagonalSplit.XpZp: r = 3; break;
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        int ch = 0;
                        bool triShift = i == 0 && (split == DiagonalSplit.XpZn || split == DiagonalSplit.XnZn) ||
                                        i != 0 && (split == DiagonalSplit.XpZp || split == DiagonalSplit.XnZp);

                        ch = i == 0 ? toggled ? 3 : 0 : toggled ? 1 : 2;
                        equal[0] = vPos[ch] == vOrg[ch];
                        vtxs[c].Position = vPos[ch];
                        if (triShift) vtxs[c].Position.Y = vOrg[r].Y + (vPos[ch] - vOrg[ch]).Y;
                        vtxs[c].Color = i == 1 ? p1c : p2c;
                        c++;

                        ch = i == 0 ? toggled ? 0 : 1 : toggled ? 2 : 3;
                        equal[1] = vPos[ch] == vOrg[ch];
                        vtxs[c].Position = vPos[ch];
                        vtxs[c].Color = i == 1 ? p1c : p2c;
                        c++;

                        ch = i == 0 ? toggled ? 1 : 2 : toggled ? 3 : 0;
                        equal[2] = vPos[ch] == vOrg[ch]; vtxs[c].Position = vPos[ch];
                        if (triShift) vtxs[c].Position.Y = vOrg[r].Y + (vPos[ch] - vOrg[ch]).Y;
                        vtxs[c].Color = i == 1 ? p1c : p2c;

                        if (equal[0] && equal[1] && equal[2])
                            vtxs[c].Color = vtxs[c - 1].Color = vtxs[c - 2].Color = Vector4.Zero;
                        c++;
                    }

                    // Draw diagonals
                    bool flip = split == DiagonalSplit.XnZp || split == DiagonalSplit.XpZn;
                    bool draw = split != DiagonalSplit.None && !(floor ? instance.FloorIsQuad : instance.CeilingIsQuad);

                    vtxs[c].Position = flip ? vOrg[1] : vOrg[0]; vtxs[c].Color = draw ? p1c : Vector4.Zero; c++;
                    vtxs[c].Position = flip ? vOrg[3] : vOrg[2]; vtxs[c].Color = draw ? p2c : Vector4.Zero; c++;
                    vtxs[c].Position = flip ? vPos[3] : vPos[2]; vtxs[c].Color = draw ? p1c : Vector4.Zero; c++;
                    vtxs[c].Position = flip ? vPos[3] : vPos[2]; vtxs[c].Color = draw ? p1c : Vector4.Zero; c++;
                    vtxs[c].Position = flip ? vPos[1] : vPos[0]; vtxs[c].Color = draw ? p2c : Vector4.Zero; c++;
                    vtxs[c].Position = flip ? vOrg[1] : vOrg[0]; vtxs[c].Color = draw ? p1c : Vector4.Zero; c++;

                }

                // Append this block's 84 vertices to the shared batch — vertex colors
                // already contain the per-vertex alpha; no transform needed because
                // ControlPositions returns world-space coordinates.
                for (int v = 0; v < 84; v++)
                    _ghostBlockBodyVertices.Add(new SolidLineVertex
                    {
                        Position = vtxs[v].Position,
                        Color = vtxs[v].Color,
                    });
            }

            if (_ghostBlockBodyVertices.Count == 0)
                return;

            _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_ghostBlockBodyVertices));
            _linesBatch.Render(new RenderingDrawingLines.RenderArgs
            {
                RenderTarget = SwapChain,
                StateBuffer = _renderingStateBuffer,
                Topology = RenderingDrawingLines.Topology.TriangleList,
                Blend = BlendMode.NonPremultipliedAlpha,
                Depth = DepthMode.DepthRead,
            });
        }

        // Reused across DrawGhostBlockBodies calls.
        private readonly List<SolidLineVertex> _ghostBlockBodyVertices = new List<SolidLineVertex>();

        // Volumes have three visual layers:
        //   Layer 1 — small placeholder cube at each volume position. Color encodes
        //             enabled/disabled and selection. Always rendered.
        //   Layer 2 — the actual 3D volume extent (Box or Sphere) shown as a
        //             translucent SOLID fill. TombEngine-only. Submitted as a
        //             TriangleList batch with low alpha so contents behind the
        //             volume remain readable while the shape is clearly visible.
        //   Layer 3 — wireframe outline of the same extent, drawn on top of the
        //             fill so the volume's edges read sharply against the fill.
        //
        // All layers use NonPremultipliedAlpha + DepthRead to fade behind opaque
        // geometry without writing to depth.
        private void DrawVolumes(object effect, List<VolumeInstance> volumesToDraw, List<Text> textToDraw, List<Sprite> sprites)
        {
            if (volumesToDraw.Count == 0)
                return;

            var drawVolume = _editor.Level.IsTombEngine;
            var baseColor = _editor.Configuration.UI_ColorScheme.ColorTrigger;
            var normalColor = new Vector4(baseColor.To3() * 0.6f, 0.55f);
            var selectColor = new Vector4(baseColor.To3(), 0.7f);
            var disabledNormalColor = new Vector4(new Vector3(normalColor.To3().GetLuma()), 0.55f);
            var disabledSelectColor = new Vector4(new Vector3(selectColor.To3().GetLuma()), 0.55f);

            _volumeCenterVertices.Clear();
            _volumeBodyVertices.Clear();
            _volumeBodySolidVertices.Clear();

            for (int i = 0; i < volumesToDraw.Count; i++)
            {
                var instance = volumesToDraw[i];
                bool isSelected = _editor.SelectedObject == instance;
                bool isHighlighted = _highlightedObjects.Contains(instance);

                Vector4 placeholderColor = isSelected
                    ? (instance.Enabled ? selectColor : disabledSelectColor)
                    : (instance.Enabled ? normalColor : disabledNormalColor);

                if (isSelected)
                    textToDraw.Add(CreateTextTagForObject(
                        instance.RotationPositionMatrix * _viewProjection,
                        instance.ToString()));

                // Layer 1 — placeholder cube. Wire cube around the volume center.
                var placeholderWorld = Matrix4x4.CreateScale(_littleCubeRadius) * instance.RotationPositionMatrix;
                WireGeometry.AppendWireCube(_volumeCenterVertices, placeholderWorld, placeholderColor);

                // Layers 2 & 3 — 3D volume extent: translucent solid fill + wire outline.
                if (drawVolume)
                {
                    Vector4 bodyColor = isHighlighted
                        ? (instance.Enabled ? selectColor : disabledSelectColor)
                        : (instance.Enabled ? normalColor : disabledNormalColor);
                    // Fill uses the same hue but a much lower alpha so geometry behind
                    // the volume remains visible. Matches the legacy translucent body.
                    Vector4 fillColor = new Vector4(bodyColor.X, bodyColor.Y, bodyColor.Z, 0.18f);

                    switch (instance.Shape())
                    {
                        case VolumeShape.Box:
                            {
                                var bv = (BoxVolumeInstance)instance;
                                // Legacy scaled the 256-unit _littleCube primitive by
                                // (Size / _littleCubeRadius / 2). For our unit cube
                                // (-1..+1) the equivalent scale is (Size / 2).
                                var world = Matrix4x4.CreateScale(bv.Size / 2.0f) * instance.RotationPositionMatrix;
                                WireGeometry.AppendSolidCube(_volumeBodySolidVertices, world, fillColor);
                                WireGeometry.AppendWireCube(_volumeBodyVertices, world, bodyColor);
                            }
                            break;
                        case VolumeShape.Sphere:
                            {
                                var sv = (SphereVolumeInstance)instance;
                                // Legacy scaled the 1024-unit _sphere by Size / (128*8).
                                // For our unit sphere (radius 1) the equivalent radius
                                // scale is sv.Size (the sphere's full size in TR units).
                                var world = Matrix4x4.CreateScale(sv.Size) * instance.RotationPositionMatrix;
                                WireGeometry.AppendSolidSphere(_volumeBodySolidVertices, world, fillColor);
                                WireGeometry.AppendWireSphere(_volumeBodyVertices, world, bodyColor, segments: 24);
                            }
                            break;
                    }
                }
            }

            // Submit layers in back-to-front order: placeholder wire, fill, outline.
            if (_volumeCenterVertices.Count > 0)
            {
                _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_volumeCenterVertices));
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = _renderingStateBuffer,
                    Blend = BlendMode.NonPremultipliedAlpha,
                    Depth = DepthMode.DepthRead,
                });
            }
            if (_volumeBodySolidVertices.Count > 0)
            {
                _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_volumeBodySolidVertices));
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = _renderingStateBuffer,
                    Blend = BlendMode.NonPremultipliedAlpha,
                    Depth = DepthMode.DepthRead,
                    Topology = RenderingDrawingLines.Topology.TriangleList,
                });
            }
            if (_volumeBodyVertices.Count > 0)
            {
                _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_volumeBodyVertices));
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = _renderingStateBuffer,
                    Blend = BlendMode.NonPremultipliedAlpha,
                    Depth = DepthMode.DepthRead,
                });
            }
        }

        private readonly List<SolidLineVertex> _volumeCenterVertices = new List<SolidLineVertex>();
        private readonly List<SolidLineVertex> _volumeBodyVertices = new List<SolidLineVertex>();
        private readonly List<SolidLineVertex> _volumeBodySolidVertices = new List<SolidLineVertex>();

        private void DrawSprites(Room[] roomsWhoseObjectsToDraw, List<Sprite> sprites, bool disableSelection)
        {
            if (_editor.Level.Settings.GameVersion.Native() > TRVersion.Game.TR2)
                return;

            var sequences = _editor.Level.Settings.WadGetAllSpriteSequences();

            foreach (Room room in roomsWhoseObjectsToDraw)
                foreach (var instance in room.Objects.OfType<SpriteInstance>())
                {
                    var sequence = sequences.FirstOrDefault(s => s.Key.TypeId == instance.Sequence).Value;
                    if (sequence != null && sequence.Sprites.Count > instance.Frame)
                    {
                        float depth;
                        var sprite = sequence.Sprites[instance.Frame];
                        var pos = instance.GetViewportRect(sprite.Alignment, Camera.GetPosition(), _viewProjection, ClientSize, out depth);

                        if (depth < 1.0f) // Discard offscreen sprites
                        {
                            var selected = _highlightedObjects.Contains(instance);
                            var newSprite = new Sprite
                            {
                                Texture = sprite.Texture.Image,
                                PosStart = pos.Start,
                                PosEnd = pos.End,
                                Depth = depth
                            };

                            if (!disableSelection && selected)
                                newSprite.Tint = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            else if (_editor.Mode == EditorMode.Lighting)
                                newSprite.Tint = new Vector4(new Vector3(instance.Color.GetLuma()), 1.0f);
                            else
                                newSprite.Tint = Vector4.One;

                            sprites.Add(newSprite);
                        }
                    }
                }
        }

        // Two-pass renderer:
        //   Pass 1 — small placeholder cube for every supported object type (sprites,
        //            cameras, flyby cameras, memos, sinks, sound sources, plus 3D
        //            meshes that fail to load → fallback). Each instance becomes a
        //            wire cube in a single accumulated batch (or a sprite icon if the
        //            user enabled Rendering3D_UseSpritesForServiceObjects).
        //   Pass 2 — flyby camera FOV cones with alpha-blended translucent fill. Still
        //            on the legacy path because it requires solid translucent triangles
        //            (RenderingDrawingLines is line-only). Migration deferred until
        //            RenderingDrawingMesh ships.
        //
        // Per-instance state changes (rasterizer mode for selection, vertex buffer
        // bind/unbind) collapsed into the single accumulated batch.
        private void DrawPlaceholders(object effect, Room[] roomsWhoseObjectsToDraw, List<Text> textToDraw, List<Sprite> sprites)
        {
            _placeholderBatchVertices.Clear();

            var groups = roomsWhoseObjectsToDraw.SelectMany(r => r.Objects).GroupBy(o => o.GetType());
            foreach (var group in groups)
            {
                if (group.Key == typeof(SpriteInstance))
                    foreach (SpriteInstance instance in group)
                    {
                        if (_editor.SelectedObject == instance)
                        {
                            textToDraw.Add(CreateTextTagForObject(
                                instance.WorldPositionMatrix * _viewProjection,
                                instance.ShortName() +
                                "\n" + GetObjectPositionString(instance.Room, instance)));
                            AddObjectHeightLine(instance.Room, instance.Position);
                        }

                        if (_editor.Level.Settings.GameVersion.Native() > TRVersion.Game.TR2 || !instance.SpriteIsValid)
                        {
                            var color = _editor.SelectedObject == instance
                                ? _editor.Configuration.UI_ColorScheme.ColorSelection
                                : new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
                            QueueServiceObjectPlaceholder(instance, color, sprites);
                        }
                    }

                if (group.Key == typeof(CameraInstance) && _editor.CameraPreviewMode == CameraPreviewType.None)
                    foreach (CameraInstance instance in group)
                    {
                        var color = new Vector4(0.4f, 0.9f, 0.0f, 1.0f);
                        if (_highlightedObjects.Contains(instance))
                        {
                            color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            if (_editor.SelectedObject == instance)
                            {
                                textToDraw.Add(CreateTextTagForObject(
                                    instance.RotationPositionMatrix * _viewProjection,
                                    "Camera " + (instance.CameraMode == CameraInstanceMode.Locked ? "(Locked)" : instance.CameraMode == CameraInstanceMode.Sniper ? "(Sniper)" : "") +
                                    instance.GetScriptIDOrName() + "\n" +
                                    GetObjectPositionString(instance.Room, instance) + GetObjectTriggerString(instance)));
                                AddObjectHeightLine(instance.Room, instance.Position);
                            }
                        }
                        QueueServiceObjectPlaceholder(instance, color, sprites);
                    }

                if (group.Key == typeof(FlybyCameraInstance) && _editor.CameraPreviewMode == CameraPreviewType.None)
                    foreach (FlybyCameraInstance instance in group)
                    {
                        var color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                        if (TryGetSelectedFlybySequence(out int selectedSequence) && selectedSequence == instance.Sequence)
                            color = MathC.GetRandomColorByIndex(instance.Sequence, 32, 0.7f);
                        if (_highlightedObjects.Contains(instance))
                        {
                            color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            if (_editor.SelectedObject == instance)
                            {
                                textToDraw.Add(CreateTextTagForObject(
                                    instance.RotationPositionMatrix * _viewProjection,
                                    "Flyby cam (" + instance.Sequence + ":" + instance.Number + ") " +
                                    instance.GetScriptIDOrName() + "\n" +
                                    GetObjectPositionString(instance.Room, instance) + GetObjectTriggerString(instance)));
                                AddObjectHeightLine(instance.Room, instance.Position);
                            }
                        }
                        QueueServiceObjectPlaceholder(instance, color, sprites);
                    }

                if (group.Key == typeof(MemoInstance))
                    foreach (MemoInstance instance in group)
                    {
                        var color = _highlightedObjects.Contains(instance)
                            ? _editor.Configuration.UI_ColorScheme.ColorSelection
                            : Vector4.One;
                        if (_editor.SelectedObject == instance || instance.AlwaysDisplay)
                            textToDraw.Add(CreateTextTagForObject(instance.RotationPositionMatrix * _viewProjection, instance.Text));
                        QueueServiceObjectPlaceholder(instance, color, sprites);
                    }

                if (group.Key == typeof(SinkInstance))
                    foreach (SinkInstance instance in group)
                    {
                        var color = new Vector4(0.0f, 0.6f, 1.0f, 1.0f);
                        if (_highlightedObjects.Contains(instance))
                        {
                            color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            if (_editor.SelectedObject == instance)
                            {
                                textToDraw.Add(CreateTextTagForObject(
                                    instance.RotationPositionMatrix * _viewProjection,
                                    instance.ToShortString() + "\n" +
                                    GetObjectPositionString(instance.Room, instance) + GetObjectTriggerString(instance)));
                                AddObjectHeightLine(instance.Room, instance.Position);
                            }
                        }
                        QueueServiceObjectPlaceholder(instance, color, sprites);
                    }

                if (group.Key == typeof(SoundSourceInstance))
                    foreach (SoundSourceInstance instance in group)
                    {
                        var color = new Vector4(1.0f, 0.7f, 0.0f, 1.0f);
                        if (_highlightedObjects.Contains(instance))
                        {
                            color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            if (_editor.SelectedObject == instance)
                            {
                                textToDraw.Add(CreateTextTagForObject(
                                    instance.RotationPositionMatrix * _viewProjection,
                                    "Sound source ID " + (instance.SoundId != -1 ? instance.SoundId + ": " + instance.SoundNameToDisplay : "No sound assigned yet") +
                                    instance.GetScriptIDOrName() + "\n" +
                                    GetObjectPositionString(instance.Room, instance)));
                                AddObjectHeightLine(instance.Room, instance.Position);
                            }
                        }
                        QueueServiceObjectPlaceholder(instance, color, sprites);
                    }

                if (ShowMoveables && group.Key == typeof(MoveableInstance))
                    foreach (MoveableInstance instance in group)
                    {
                        if (_editor?.Level?.Settings?.WadTryGetMoveable(instance.WadObjectId) != null)
                            continue;
                        var color = new Vector4(0.4f, 0.4f, 1.0f, 1.0f);
                        if (_highlightedObjects.Contains(instance))
                        {
                            color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            if (_editor.SelectedObject == instance)
                            {
                                textToDraw.Add(CreateTextTagForObject(
                                    instance.RotationPositionMatrix * _viewProjection,
                                    instance.ShortName() + "\nUnavailable " + instance.ItemType +
                                    instance.GetScriptIDOrName() + "\n" +
                                    GetObjectPositionString(instance.Room, instance) + GetObjectTriggerString(instance)));
                                AddObjectHeightLine(instance.Room, instance.Position);
                            }
                        }
                        QueueServiceObjectPlaceholder(instance, color, sprites);
                    }

                if (ShowStatics && group.Key == typeof(StaticInstance))
                    foreach (StaticInstance instance in group)
                    {
                        if (_editor?.Level?.Settings?.WadTryGetStatic(instance.WadObjectId) != null)
                            continue;
                        var color = new Vector4(0.4f, 0.4f, 1.0f, 1.0f);
                        if (_highlightedObjects.Contains(instance))
                        {
                            color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            if (_editor.SelectedObject == instance)
                            {
                                textToDraw.Add(CreateTextTagForObject(
                                    instance.RotationPositionMatrix * _viewProjection,
                                    instance.ShortName() + "\nUnavailable " + instance.ItemType + GetObjectTriggerString(instance)));
                                AddObjectHeightLine(instance.Room, instance.Position);
                            }
                        }
                        QueueServiceObjectPlaceholder(instance, color, sprites);
                    }

                if (ShowImportedGeometry && group.Key == typeof(ImportedGeometryInstance))
                    foreach (ImportedGeometryInstance instance in group)
                    {
                        if (instance.Model?.DirectXModel == null || instance.Model?.DirectXModel.Meshes.Count == 0 || instance.Hidden)
                        {
                            var color = new Vector4(0.5f, 0.3f, 1.0f, 1.0f);
                            if (_highlightedObjects.Contains(instance))
                            {
                                color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                                if (_editor.SelectedObject == instance)
                                {
                                    textToDraw.Add(CreateTextTagForObject(
                                        instance.RotationPositionMatrix * _viewProjection,
                                        instance.ToString()));
                                    AddObjectHeightLine(instance.Room, instance.Position);
                                }
                            }
                            QueueServiceObjectPlaceholder(instance, color, sprites);
                        }
                    }
            }

            // Submit pass-1 batch (wire cubes for every placeholder: cameras,
            // sinks, fog bulbs, sound sources, etc.). Depth disabled so the
            // markers stay visible through world geometry.
            if (_placeholderBatchVertices.Count > 0)
            {
                _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_placeholderBatchVertices));
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = _renderingStateBuffer,
                    Depth = DepthMode.NoZ,
                });
            }

            if (_editor.CameraPreviewMode != CameraPreviewType.None)
                return;

            // Pass 2 — flyby camera FOV cones (hidden during flyby preview).
            //
            // Two batches:
            //   solid (TriangleList) for non-selected flybys — translucent filled cone
            //   wire  (LineList)     for selected   flybys — wireframe FOV cone + roll pointer
            // Both use NonPremultipliedAlpha because vertex colors carry straight alpha.
            //
            // Coordinate system for the legacy _cone primitive: apex at origin, base
            // ring at z=1024 with radius 512. Our WireGeometry helpers use a unit cone
            // (apex at origin, base at z=1, radius 1), so every legacy scale factor S
            // gets multiplied by the legacy primitive size (512 for radius, 1024 for
            // length) when building the World matrix for our path.
            const float legacyConeBaseRadius = 512.0f;
            const float legacyConeLength = 1024.0f;

            _flybySolidConeVertices.Clear();
            _flybyWireConeVertices.Clear();

            foreach (Room room in roomsWhoseObjectsToDraw)
                foreach (var instance in room.Objects.OfType<FlybyCameraInstance>())
                {
                    var color = MathC.GetRandomColorByIndex(instance.Sequence, 32, 0.7f);
                    if (_highlightedObjects.Contains(instance))
                        color = _editor.Configuration.UI_ColorScheme.ColorSelection;

                    // Distance fade: shrink alpha when very close to camera so the cone
                    // stops obscuring nearby geometry.
                    float distance = Vector3.Distance(instance.WorldPosition, Camera.GetPosition());
                    if (distance < (_coneRadius * 0.5f))
                        color.W *= distance / (_coneRadius * 0.5f);

                    if (_editor.SelectedObject == instance)
                    {
                        // Selected flyby: wire FOV cone + wire roll pointer.
                        float coneAngle = (float)Math.Atan2(512, 1024);
                        float cutoffScaleW = instance.Fov * (float)(Math.PI / 360) / coneAngle;

                        var fovWorld = Matrix4x4.CreateScale(
                                cutoffScaleW * legacyConeBaseRadius,
                                cutoffScaleW * legacyConeBaseRadius,
                                legacyConeLength) * instance.ObjectMatrix;
                        WireGeometry.AppendWireCone(_flybyWireConeVertices, fovWorld, color);

                        // Roll pointer: small cone offset to the side, rotated 90° on X
                        // so it points "up" relative to the FOV cone — indicates the
                        // camera roll axis.
                        var step = 1f / _coneRadius;
                        var scaleU = _littleCubeRadius * 2;
                        var pScale = _littleCubeRadius / 5;
                        var vOffset = -cutoffScaleW / 2 * _coneRadius - scaleU;
                        var hOffset = legacyConeLength;

                        var rollWorld = Matrix4x4.CreateScale(
                                step * pScale * legacyConeBaseRadius,
                                step * pScale * legacyConeBaseRadius,
                                step * scaleU * legacyConeLength) *
                            Matrix4x4.CreateTranslation(new Vector3(0, hOffset, vOffset)) *
                            Matrix4x4.CreateRotationX((float)(Math.PI / 2)) *
                            instance.ObjectMatrix;
                        WireGeometry.AppendWireCone(_flybyWireConeVertices, rollWorld, color);
                    }
                    else
                    {
                        // Non-selected flyby: small solid translucent cone slightly
                        // behind the camera position to indicate direction without
                        // visually dominating the view.
                        var unselectedScale = 1.0f / _coneRadius * _littleCubeRadius * 2.0f; // = 0.25
                        var translateBack = -_coneRadius * 1.2f;
                        var translateExtra = _editor.Configuration.Rendering3D_UseSpritesForServiceObjects
                            ? -_coneRadius * 0.5f : 0f;

                        var world = Matrix4x4.CreateScale(
                                unselectedScale * legacyConeBaseRadius,
                                unselectedScale * legacyConeBaseRadius,
                                unselectedScale * legacyConeLength) *
                            Matrix4x4.CreateRotationY((float)Math.PI) *
                            Matrix4x4.CreateTranslation(new Vector3(0, 0, translateBack + translateExtra)) *
                            instance.ObjectMatrix;
                        WireGeometry.AppendSolidCone(_flybySolidConeVertices, world, color);
                    }
                }

            // Flyby camera frustum cones — always visible (depth ignored).
            if (_flybySolidConeVertices.Count > 0)
            {
                _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_flybySolidConeVertices));
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = _renderingStateBuffer,
                    Topology = RenderingDrawingLines.Topology.TriangleList,
                    Blend = BlendMode.NonPremultipliedAlpha,
                    Depth = DepthMode.NoZ,
                });
            }
            if (_flybyWireConeVertices.Count > 0)
            {
                _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_flybyWireConeVertices));
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = _renderingStateBuffer,
                    Blend = BlendMode.NonPremultipliedAlpha,
                    Depth = DepthMode.NoZ,
                });
            }
        }

        private readonly List<SolidLineVertex> _flybySolidConeVertices = new List<SolidLineVertex>();
        private readonly List<SolidLineVertex> _flybyWireConeVertices = new List<SolidLineVertex>();

        // Sprite-icon placeholder for service objects. Used by DrawLights, the
        // QueueServiceObjectPlaceholder helper, etc. When the user has enabled
        // Rendering3D_UseSpritesForServiceObjects this is the path that runs;
        // otherwise the wire-cube/wire-sphere helpers in WireGeometry handle the
        // placeholder rendering directly.
        //
        // The `primitive` and `effect` parameters are kept on the signature for
        // source compatibility with the older callers — both are unused now.
        private void DrawOrQueueServiceObject(ISpatial instance, object primitive, Vector4 color, object effect, List<Sprite> sprites)
        {
            if (_editor.CameraPreviewMode != CameraPreviewType.None)
                return;
            if (!_editor.Configuration.Rendering3D_UseSpritesForServiceObjects)
                return; // Non-sprite path is handled by WireGeometry helpers in callers.

            foreach (bool shadow in new[] { true, false })
            {
                if (shadow)
                {
                    if (_editor.Level.Settings.GameVersion != TRVersion.Game.TombEngine)
                        continue;
                    if (!(instance is LightInstance) || !(instance as LightInstance).CanCastDynamicShadows)
                        continue;
                }
                var newSprite = ServiceObjectTextures.GetSprite(instance,
                    Camera.GetPosition(),
                    _viewProjection,
                    ClientSize,
                    shadow ? new Vector4(Vector3.Zero, 1.0f) : color,
                    shadow ? new Vector2(8.0f, -8.0f) : Vector2.Zero,
                    _highlightedObjects.Contains((ObjectInstance)instance));
                if (newSprite == null)
                    return;
                sprites.Add(newSprite);
            }
        }

        private void DrawCardinalDirections(List<Text> textToDraw)
        {
            string[] messages;
            if (_editor.Configuration.Rendering3D_UseRoomEditorDirections)
                messages = new string[] { "+Z (East)", "-Z (West)", "+X (South)", "-X (North)" };
            else
                messages = new string[] { "+Z (North)", "-Z (South)", "+X (East)", "-X (West)" };

            Vector3[] positions = new Vector3[4]
                {
                        new Vector3(0, 0, _editor.SelectedRoom.NumZSectors *  Level.HalfSectorSizeUnit),
                        new Vector3(0, 0, _editor.SelectedRoom.NumZSectors * -Level.HalfSectorSizeUnit),
                        new Vector3(_editor.SelectedRoom.NumXSectors *  Level.HalfSectorSizeUnit, 0, 0),
                        new Vector3(_editor.SelectedRoom.NumXSectors * -Level.HalfSectorSizeUnit, 0, 0)
                 };

            var center = _editor.SelectedRoom.GetLocalCenter();
            var matrix = Matrix4x4.CreateTranslation(_editor.SelectedRoom.WorldPos) * _viewProjection;
            for (int i = 0; i < 4; i++)
            {
                var pos = matrix.TransformPerspectively(center + positions[i]);
                if (pos.Z <= 1.0f)
                    textToDraw.Add(new Text
                    {
                        Font = _fontDefault,
                        Pos = pos.To2(),
                        Overlay = _editor.Configuration.Rendering3D_DrawFontOverlays,
                        String = messages[i]
                    });
            }
        }

        // Skybox: the Horizon moveable rendered around the camera at scale 128 with
        // depth disabled (cleared after the draw so subsequent geometry is drawn over).
        // Migrated to RenderingDrawingMesh; same per-mesh pattern as DrawMoveables but
        // with a simpler color/lighting setup (always white tint, no static lighting).
        private void DrawSkybox()
        {
            var version = _editor.Level.Settings.GameVersion;
            WadMoveableId? horizonId = WadMoveableId.GetHorizon(version);
            if (!horizonId.HasValue)
                return;

            var moveable = _editor?.Level?.Settings?.WadTryGetMoveable(horizonId.Value);
            if (moveable == null)
                return;

            AnimatedModel model = _wadRenderer.GetMoveable(moveable);

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var legacyMesh = model.Meshes[i];
                if (legacyMesh.Vertices.Count == 0)
                    continue;

                var drawMesh = GetOrCreateDrawingMesh(legacyMesh);
                var world = Matrix4x4.CreateScale(128.0f) *
                            model.AnimationTransforms[i] *
                            Matrix4x4.CreateTranslation(Camera.GetPosition());

                drawMesh.Render(new RenderingDrawingMesh.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = _renderingStateBuffer,
                    Atlas = _wadRenderer.Texture,
                    World = world,
                    Tint = Vector4.One,
                    StaticLighting = false,
                    ColoredVertices = false,
                    BilinearFilter = BilinearFilter,
                    NoDepth = true,
                });
            }

            // Belt and suspenders: explicit depth clear too, so DX11/OpenGL
            // (which historically relied on this) keep matching behaviour.
            SwapChain.ClearDepth();
        }

        // Moveables: rigid per-mesh draws with per-bone world matrix (the legacy
        // AnimationTransforms[i] is the per-mesh-bone transform already in world space
        // when multiplied by ObjectMatrix). Migrated to RenderingDrawingMesh; no GPU
        // skinning needed because the per-bone matrix is computed CPU-side and applied
        // as the World transform.
        //
        // KNOWN LIMITATION: TombEngine Lara skin (RenderSkin path) is NOT migrated.
        // It's a separate code path (skin.RenderSkin) that does GPU skinning with
        // bone matrices on a different mesh. That migration is deferred to a follow-up
        // because RenderSkin is implemented inside AnimatedModel — touching it would
        // affect WadTool too.
        private void DrawMoveables(List<MoveableInstance> moveablesToDraw, List<Text> textToDraw, bool disableSelection = false)
        {
            if (moveablesToDraw.Count == 0)
                return;

            var groups = moveablesToDraw.GroupBy(m => m.WadObjectId);
            foreach (var group in groups)
            {
                var movID = _editor?.Level?.Settings?.WadTryGetMoveable(group.Key);
                if (movID == null)
                    continue;

                var model = _wadRenderer.GetMoveable(movID);
                var skin = model;
                var version = _editor.Level.Settings.GameVersion;
                var colored = version.Native() <= TRVersion.Game.TR2 && group.First().CanBeColored();

                if (group.Key == WadMoveableId.Lara) // Lara uses a separate skin moveable when TombEngine
                {
                    var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(version, group.Key.TypeId));
                    var moveableSkin = _editor.Level.Settings.WadTryGetMoveable(skinId);
                    if (moveableSkin != null && moveableSkin.Meshes.Count == model.Meshes.Count)
                    {
                        movID = moveableSkin;
                        skin = _wadRenderer.GetMoveable(moveableSkin);
                    }
                }

                // Text labels for the selected instance + (TombEngine + Lara) skin.
                foreach (var instance in group)
                {
                    if (_editor.SelectedObject == instance)
                    {
                        textToDraw.Add(CreateTextTagForObject(
                            instance.RotationPositionMatrix * _viewProjection,
                            instance.ItemType.MoveableId.ShortName(_editor.Level.Settings.GameVersion) +
                            instance.GetScriptIDOrName() + "\n" +
                            GetObjectPositionString(instance.Room, instance) + "\n" +
                            GetObjectRotationString(instance.Room, instance) +
                            (instance.Ocb == 0 ? string.Empty : "\nOCB: " + instance.Ocb) +
                            GetObjectTriggerString(instance)));
                        AddObjectHeightLine(instance.Room, instance.Position);
                    }

                    if (!_editor.Level.IsTombEngine || skin.Skin == null)
                        continue;

                    // GPU skinning for Lara's TombEngine skin mesh. The bone matrices
                    // are `invBindPose × animTransform` (System.Numerics row-vector
                    // convention); MeshShader's `mul(blended, position)` form picks up
                    // the transpose automatically via HLSL's column-major reading, so
                    // no explicit Matrix4x4.Transpose is needed (in contrast to the
                    // legacy AnimatedModel.RenderSkin which uses row-vec mul(v, M)).
                    var skinDraw = GetOrCreateDrawingMesh(skin.Skin);
                    int boneCount = model.AnimationTransforms.Count;
                    var bones = new Matrix4x4[boneCount];
                    for (int b = 0; b < boneCount; ++b)
                    {
                        if (Matrix4x4.Invert(model.BindPoseTransforms[b], out var invBindPose))
                            bones[b] = invBindPose * model.AnimationTransforms[b];
                        else
                            bones[b] = Matrix4x4.Identity;
                    }

                    Vector4 skinTint;
                    bool skinStaticLighting;
                    if (!disableSelection && _highlightedObjects.Contains(instance))
                    {
                        skinTint = _editor.Configuration.UI_ColorScheme.ColorSelection;
                        skinStaticLighting = false;
                    }
                    else if (ShowRealTintForObjects && _editor.Mode == EditorMode.Lighting)
                    {
                        skinTint = ConvertColor(instance.Room.Properties.AmbientLight * instance.Color);
                        skinStaticLighting = true;
                    }
                    else
                    {
                        skinTint = Vector4.One;
                        skinStaticLighting = false;
                    }

                    skinDraw.Render(new RenderingDrawingMesh.RenderArgs
                    {
                        RenderTarget = SwapChain,
                        StateBuffer = _renderingStateBuffer,
                        Atlas = _wadRenderer.Texture,
                        World = instance.ObjectMatrix,
                        Tint = skinTint,
                        StaticLighting = skinStaticLighting,
                        ColoredVertices = _editor.Level.IsTombEngine,
                        AlphaTest = HideTransparentFaces,
                        BilinearFilter = BilinearFilter,
                        Skinned = true,
                        BoneMatrices = bones,
                    });
                }

                // === New path: per-mesh draws ===
                for (int i = 0; i < skin.Meshes.Count; i++)
                {
                    var legacyMesh = skin.Meshes[i];
                    if (legacyMesh.Vertices.Count == 0)
                        continue;
                    if (_editor.Level.IsTombEngine && skin.Skin != null && legacyMesh.Hidden)
                        continue;

                    var drawMesh = GetOrCreateDrawingMesh(legacyMesh);

                    foreach (var instance in group)
                    {
                        Vector4 tint;
                        bool staticLighting;
                        if (!disableSelection && _highlightedObjects.Contains(instance))
                        {
                            tint = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            staticLighting = false;
                        }
                        else if (ShowRealTintForObjects && _editor.Mode == EditorMode.Lighting)
                        {
                            if (colored || movID.Meshes[i].LightingType != WadMeshLightingType.Normals)
                            {
                                tint = ConvertColor(instance.Color);
                                staticLighting = true;
                            }
                            else
                            {
                                var color = _editor.Level.IsTombEngine ? instance.Room.Properties.AmbientLight * instance.Color : instance.Room.Properties.AmbientLight;
                                tint = ConvertColor(color);
                                staticLighting = _editor.Level.IsTombEngine;
                            }
                        }
                        else
                        {
                            tint = Vector4.One;
                            staticLighting = false;
                        }

                        var world = model.AnimationTransforms[i] * instance.ObjectMatrix;

                        drawMesh.Render(new RenderingDrawingMesh.RenderArgs
                        {
                            RenderTarget = SwapChain,
                            StateBuffer = _renderingStateBuffer,
                            Atlas = _wadRenderer.Texture,
                            World = world,
                            Tint = tint,
                            StaticLighting = staticLighting,
                            ColoredVertices = _editor.Level.IsTombEngine,
                            AlphaTest = HideTransparentFaces,
                            BilinearFilter = BilinearFilter,
                        });
                    }
                }
            }
        }

        // Imported geometry — third-party 3D models (FBX/OBJ/COLLADA) loaded into the
        // level. Migrated to RenderingDrawingImportedGeometry; per-submesh textures
        // are bound directly (NOT via atlas), so the new abstraction differs from
        // RenderingDrawingMesh.
        private void DrawImportedGeometry(List<ImportedGeometryInstance> importedGeometryToDraw, List<Text> textToDraw, bool disableSelection = false)
        {
            if (importedGeometryToDraw.Count == 0)
                return;

            var groups = importedGeometryToDraw.GroupBy(g => g.Model.UniqueID);
            foreach (var group in groups)
            {
                var model = group.First().Model.DirectXModel;
                if (model == null || model.Meshes == null || model.Meshes.Count == 0)
                    continue;

                var meshes = model.Meshes;
                for (var i = 0; i < meshes.Count; i++)
                {
                    var legacyMesh = meshes[i];
                    if (legacyMesh.Vertices.Count == 0)
                        continue;

                    var drawMesh = GetOrCreateImportedDrawingMesh(legacyMesh);

                    foreach (var instance in group)
                    {
                        if (instance.Hidden)
                            continue;

                        Vector4 tint;
                        bool useVertexColors;
                        if (!disableSelection && _highlightedObjects.Contains(instance))
                        {
                            tint = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            useVertexColors = false;
                        }
                        else if (DisablePickingForImportedGeometry)
                        {
                            tint = new Vector4(0.4f, 0.4f, 1.0f, 1.0f);
                            useVertexColors = false;
                        }
                        else
                        {
                            useVertexColors = _editor.Mode == EditorMode.Lighting && ShowRealTintForObjects && instance.LightingModel == ImportedGeometryLightingModel.VertexColors;
                            if (ShowRealTintForObjects && _editor.Mode == EditorMode.Lighting)
                            {
                                switch (instance.LightingModel)
                                {
                                    case ImportedGeometryLightingModel.NoLighting:
                                    case ImportedGeometryLightingModel.CalculateFromLightsInRoom:
                                        tint = ConvertColor(instance.Color * instance.Room.Properties.AmbientLight);
                                        break;
                                    case ImportedGeometryLightingModel.VertexColors:
                                    case ImportedGeometryLightingModel.TintAsAmbient:
                                        tint = ConvertColor(instance.Color);
                                        break;
                                    default:
                                        tint = Vector4.One;
                                        break;
                                }
                            }
                            else
                                tint = Vector4.One;
                        }

                        drawMesh.Render(new RenderingDrawingImportedGeometry.RenderArgs
                        {
                            RenderTarget = SwapChain,
                            StateBuffer = _renderingStateBuffer,
                            World = instance.ObjectMatrix,
                            Tint = tint,
                            UseVertexColors = useVertexColors,
                            AlphaTest = HideTransparentFaces,
                            BilinearFilter = BilinearFilter,
                            ForceAdditive = DisablePickingForImportedGeometry,
                        });

                        if (i == 0 && _editor.SelectedObject == instance)
                        {
                            textToDraw.Add(CreateTextTagForObject(
                                instance.RotationPositionMatrix * _viewProjection,
                                instance + "\n" + GetObjectPositionString(_editor.SelectedRoom, instance) + "\n" +
                                GetObjectRotationString(_editor.SelectedRoom, instance) + "\n" +
                                "Scale: " + instance.Scale + "\n" +
                                "Triangles: " + instance.Model.DirectXModel.TotalTriangles));
                            AddObjectHeightLine(_editor.SelectedRoom, instance.Position);
                        }
                    }
                }
            }
        }

        // Per-mesh cached RenderingDrawingImportedGeometry. Each ImportedGeometryMesh
        // has its OWN per-submesh texture references — when we build the abstraction's
        // Description we extract texture + size from each submesh's material.
        private RenderingDrawingImportedGeometry GetOrCreateImportedDrawingMesh(TombLib.LevelData.ImportedGeometryMesh legacyMesh)
        {
            if (_importedMeshCache.TryGetValue(legacyMesh, out var cached))
                return cached;

            var verts = new RenderingDrawingImportedGeometry.Vertex[legacyMesh.Vertices.Count];
            for (int i = 0; i < legacyMesh.Vertices.Count; ++i)
            {
                var s = legacyMesh.Vertices[i];
                verts[i] = new RenderingDrawingImportedGeometry.Vertex
                {
                    Position = s.Position,
                    UV = s.UV,
                    Color = s.Color,
                    Normal = s.Normal,
                };
            }

            var subList = new List<RenderingDrawingImportedGeometry.Submesh>(legacyMesh.Submeshes.Count);
            foreach (var kv in legacyMesh.Submeshes)
            {
                if (kv.Value.NumIndices == 0)
                    continue;
                var matTexture = kv.Value.Material.Texture;
                object texObj = null;
                Vector2 texSize = Vector2.Zero;
                if (matTexture is TombLib.LevelData.ImportedGeometryTexture igt)
                {
                    texObj = igt.DirectXTexture;
                    texSize = new Vector2(matTexture.Image.Width, matTexture.Image.Height);
                }
                subList.Add(new RenderingDrawingImportedGeometry.Submesh
                {
                    IndexStart = kv.Value.BaseIndex,
                    IndexCount = kv.Value.NumIndices,
                    DoubleSided = kv.Key.DoubleSided,
                    AdditiveBlending = kv.Key.AdditiveBlending,
                    Texture = texObj,
                    TextureSize = texSize,
                });
            }

            var mesh = Device.CreateDrawingImportedGeometry(new RenderingDrawingImportedGeometry.Description
            {
                Vertices = verts,
                Indices = legacyMesh.Indices,
                Submeshes = subList,
            });
            _importedMeshCache[legacyMesh] = mesh;
            return mesh;
        }

        // Static meshes — no skinning, single ObjectMatrix per instance.
        // Migrated to RenderingDrawingMesh: the legacy ObjectMesh's CPU-side vertex
        // data is mirrored into a Dx11RenderingDrawingMesh on first use (cached in
        // _meshCache). The atlas texture passes through directly from the WadRenderer.
        //
        // Brush overlay support: NOT yet ported to MeshShader. ApplyBrushToModelEffect
        // is still called on the legacy effect (no harm — it's just a parameter set
        // on a now-unused effect). Caller migration TODO.
        private void DrawStatics(List<StaticInstance> staticsToDraw, List<Text> textToDraw, bool disableSelection = false)
        {
            if (staticsToDraw.Count == 0)
                return;

            var groups = staticsToDraw.GroupBy(s => s.WadObjectId);
            foreach (var group in groups)
            {
                var statID = _editor?.Level?.Settings?.WadTryGetStatic(group.Key);
                if (statID == null)
                    continue;
                var model = _wadRenderer.GetStatic(statID);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    var legacyMesh = model.Meshes[i];
                    if (legacyMesh.Vertices.Count == 0)
                        continue;

                    var drawMesh = GetOrCreateDrawingMesh(legacyMesh);

                    foreach (var instance in group)
                    {
                        Vector4 tint;
                        bool staticLighting;
                        if (!disableSelection && _highlightedObjects.Contains(instance))
                        {
                            tint = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            staticLighting = false;
                        }
                        else if (_editor.Mode == EditorMode.Lighting)
                        {
                            var entry = _editor.Level.Settings.GetStaticMergeEntry(instance.WadObjectId);
                            if (!ShowRealTintForObjects || entry == null && statID.Mesh.LightingType == WadMeshLightingType.VertexColors || entry != null && entry.Merge && entry.TintAsAmbient)
                                tint = ConvertColor(instance.Color);
                            else if (_editor.Level.IsTombEngine)
                                tint = ConvertColor(instance.Room.Properties.AmbientLight * instance.Color);
                            else
                                tint = ConvertColor(instance.Room.Properties.AmbientLight);

                            if (entry != null && entry.Merge)
                                staticLighting = !entry.ClearShades;
                            else
                                staticLighting = _editor.Level.IsTombEngine ? true : statID.Mesh.LightingType == WadMeshLightingType.VertexColors;
                        }
                        else
                        {
                            tint = Vector4.One;
                            staticLighting = false;
                        }

                        drawMesh.Render(new RenderingDrawingMesh.RenderArgs
                        {
                            RenderTarget = SwapChain,
                            StateBuffer = _renderingStateBuffer,
                            Atlas = _wadRenderer.Texture,
                            World = instance.ObjectMatrix,
                            Tint = tint,
                            StaticLighting = staticLighting,
                            ColoredVertices = _editor.Level.IsTombEngine,
                            AlphaTest = HideTransparentFaces,
                            BilinearFilter = BilinearFilter,
                        });

                        if (i == 0 && _editor.SelectedObject == instance)
                        {
                            textToDraw.Add(CreateTextTagForObject(
                                instance.RotationPositionMatrix * _viewProjection,
                                instance.ItemType.StaticId.ToString(_editor.Level.Settings.GameVersion) +
                                instance.GetScriptIDOrName() + "\n" +
                                GetObjectPositionString(_editor.SelectedRoom, instance) +
                                "\n" + "Rotation Y: " + Math.Round(instance.RotationY, 2) +
                                GetObjectTriggerString(instance)));
                            AddObjectHeightLine(_editor.SelectedRoom, instance.Position);
                        }
                    }
                }
            }
        }

        // Returns a Dx11RenderingDrawingMesh mirroring the legacy ObjectMesh's CPU data.
        // First call per-mesh builds and caches; subsequent calls hit the cache.
        // ObjectVertex and MeshVertex have IDENTICAL field layouts (both Vector3×4 +
        // Vector4×2, sequential, naturally aligned), so the per-vertex copy is just a
        // field-by-field assignment.
        private RenderingDrawingMesh GetOrCreateDrawingMesh(TombLib.Graphics.ObjectMesh legacyMesh)
        {
            if (_meshCache.TryGetValue(legacyMesh, out var cached))
                return cached;

            var verts = new MeshVertex[legacyMesh.Vertices.Count];
            for (int i = 0; i < legacyMesh.Vertices.Count; ++i)
            {
                var s = legacyMesh.Vertices[i];
                verts[i] = new MeshVertex
                {
                    Position = s.Position,
                    UVW = s.UVW,
                    Normal = s.Normal,
                    Color = s.Color,
                    BoneIndex = s.Indices,
                    BoneWeight = s.Weights,
                };
            }

            // Submeshes: convert from Dictionary<Material, Submesh> to a flat array.
            var subList = new List<RenderingDrawingMesh.Submesh>(legacyMesh.Submeshes.Count);
            foreach (var kv in legacyMesh.Submeshes)
            {
                if (kv.Value.NumIndices == 0)
                    continue;
                subList.Add(new RenderingDrawingMesh.Submesh
                {
                    IndexStart = kv.Value.BaseIndex,
                    IndexCount = kv.Value.NumIndices,
                    DoubleSided = kv.Key.DoubleSided,
                    AdditiveBlending = kv.Key.AdditiveBlending,
                });
            }

            var mesh = Device.CreateDrawingMesh(new RenderingDrawingMesh.Description
            {
                Vertices = verts,
                Indices = legacyMesh.Indices,
                Submeshes = subList,
            });
            _meshCache[legacyMesh] = mesh;
            return mesh;
        }

        private void DrawScene()
        {
            // Verify that editor is ready
            if (_editor == null || _editor.Level == null || _editor.SelectedRoom == null
                || DeviceManager.DefaultDeviceManager.Device == null)
                return;

            // If any render exceptions were raised, bring app into safe mode and bypass rendering.
            if (SwapChain.RenderException != null)
            {
                if (!_editor.Configuration.Rendering3D_SafeMode)
                    _editor.Configuration.Rendering3D_SafeMode = true;
                return;
            }

            _watch.Restart();

            // Select light mode (0 = 32-bit, 1 = 16-bit, 2 = monochrome)
            int lightMode = 0;
            switch (_editor.Level.Settings.GameVersion)
            {
                case TRVersion.Game.TR1 or TRVersion.Game.TR1X:
                case TRVersion.Game.TR2 or TRVersion.Game.TR2X:
                    lightMode = 2;
                    break;

                case TRVersion.Game.TR3:
                case TRVersion.Game.TR4:
                    lightMode = 1;
                    break;

                case TRVersion.Game.TR5:
                case TRVersion.Game.TombEngine:
                    lightMode = 0;
                    break;

                case TRVersion.Game.TRNG:
                    lightMode = _editor.Level.Settings.Room32BitLighting ? 0 : 1;
                    break;
            }

            // New rendering setup
            bool useFlybyViewProjection = _editor.CameraPreviewMode != CameraPreviewType.None && _flybyPreview != null && (_flybyPreview.StaticFrame.HasValue || !_flybyPreview.IsFinished);

            _viewProjection = useFlybyViewProjection
                ? _flybyPreview.BuildViewProjection(ClientSize.Width, ClientSize.Height, Camera.FieldOfView)
                : Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);

            // Determine brush overlay state.
            var brushState = ComputeBrushOverlay();
            bool drawFlybyDof = TryGetFlybyDofOverlayState(out FlybyDofOverlayState flybyDofState);

            // In ObjectPlacement (brush) mode, use only the brush-specific ShowTextures flag,
            // the global white-lighting override is ignored so it doesn't bleed into brush mode.
            bool brushHidesTextures = _editor.Mode == EditorMode.ObjectPlacement && !_editor.Configuration.ObjectBrush_ShowTextures;
            bool whiteTextureOnly = _editor.Mode == EditorMode.ObjectPlacement ? brushHidesTextures : ShowLightingWhiteTextureOnly;

            _renderingStateBuffer.Set(new RenderingState
            {
                ShowExtraBlendingModes = ShowExtraBlendingModes,
                RoomGridForce = _editor.Mode == EditorMode.Geometry || brushHidesTextures,
                RoomDisableVertexColors = _editor.Mode == EditorMode.FaceEdit || _editor.Mode == EditorMode.ObjectPlacement,
                RoomGridLineWidth = _editor.Configuration.Rendering3D_LineWidth,
                TransformMatrix = _viewProjection,
                ShowLightingWhiteTextureOnly = whiteTextureOnly,
                LightMode = lightMode,
                BrushShape = brushState.Shape,
                BrushCenter = brushState.Center,
                BrushColor = brushState.Color,
                BrushRotation = brushState.Rotation,
                DofCenterRange = drawFlybyDof ? flybyDofState.CenterRange : Vector4.Zero,
                DofDirectionDistance = drawFlybyDof ? flybyDofState.DirectionDistance : Vector4.Zero,
                DofColorStrength = drawFlybyDof ? flybyDofState.ColorStrength : Vector4.Zero
            });

            var renderArgs = new RenderingDrawingRoom.RenderArgs
            {
                RenderTarget = SwapChain,
                StateBuffer = _renderingStateBuffer,
                BilinearFilter = BilinearFilter
            };

            // Prepare sprite and text lists for collecting
            var spritesToDraw = new List<Sprite>();
            var textToDraw = new List<Text>();

            // Reset
            _drawHeightLine = false;
            SwapChain.BindForce();
            Device.ResetState();

            // Update frustum
            _frustum.Update(Camera, ClientSize);

            // Collect stuff to draw
            var roomsToDraw = CollectRoomsToDraw().Where(r => _frustum.Contains(r.WorldBoundingBox)).ToArray();
            var moveablesToDraw = CollectMoveablesToDraw(roomsToDraw);
            var staticsToDraw = CollectStaticsToDraw(roomsToDraw);
            var importedGeometryToDraw = CollectImportedGeometryToDraw(roomsToDraw);
            var volumesToDraw = CollectVolumesToDraw(roomsToDraw);
            var ghostBlocksToDraw = CollectGhostBlocksToDraw(roomsToDraw);

            // Draw skybox
            if (ShowHorizon)
                DrawSkybox();

            // Draw enabled rooms
            Device.ResetState();
            foreach (Room room in roomsToDraw.Where(r => !DisablePickingForHiddenRooms || !r.Properties.Hidden))
                _renderingCachedRooms[room].Render(renderArgs);

            // Determine if selection should be visible or not.
            var hiddenSelection = _editor.Mode == EditorMode.Lighting && _editor.HiddenSelection;

            // Draw moveables and static meshes. The unified mesh path manages its
            // own rasterizer state per-submesh; the legacy depth-bias rasterizer is
            // not needed because Z-fighting between rooms and moveables is now
            // controlled by the renderer's own state.
            if (ShowMoveables)
                DrawMoveables(moveablesToDraw, textToDraw, hiddenSelection);
            if (ShowStatics)
                DrawStatics(staticsToDraw, textToDraw, hiddenSelection);

            // Draw room imported geometry
            if (importedGeometryToDraw.Count != 0 && ShowImportedGeometry)
                DrawImportedGeometry(importedGeometryToDraw, textToDraw, hiddenSelection);

            // Common effect for service objects — kept for source-compat with the
            // DrawXxx method signatures that still take an `Effect` parameter (the
            // value is no longer dereferenced by any migrated caller; removing the
            // signature parameter is mechanical churn for a follow-up).
            object effect = null;

            // Draw volumes
            if (ShowVolumes)
                DrawVolumes(effect, volumesToDraw, textToDraw, spritesToDraw);

            // Draw moveables and statics bounding boxes
            if (ShowBoundingBoxes)
            {
                var list = moveablesToDraw.Select(m => m as ObjectInstance)
                                          .Concat(staticsToDraw.Select(s => s as ObjectInstance)).ToList();
                DrawBoundingBoxes(effect, list);
            }

            if (ShowOtherObjects)
            {
                // Draw sprites
                DrawSprites(roomsToDraw, spritesToDraw, hiddenSelection);
                // Draw placeholder objects (sinks, cameras, fly-by cameras, sound sources and missing 3D objects)
                DrawPlaceholders(effect, roomsToDraw, textToDraw, spritesToDraw);
                // Draw light objects and bounding volumes
                DrawLights(effect, roomsToDraw, textToDraw, spritesToDraw);
                // Draw flyby path (hidden during flyby preview)
                if (_editor.CameraPreviewMode == CameraPreviewType.None)
                    DrawFlybyPath(effect);
                // Draw sector split highlights
                DrawSectorSplitHighlights(effect);
            }

            // Draw ghost block cubes
            if (ShowGhostBlocks)
                DrawGhostBlocks(effect, ghostBlocksToDraw, textToDraw, spritesToDraw);

            // Depth-sort sprites
            spritesToDraw = spritesToDraw.OrderByDescending(s => s.Depth).ToList();

            // Draw depth-dependent sprites. SwapChain.RenderSprites manages its own
            // blend / depth state (PremultipliedAlpha + DepthDefault); the legacy
            // _legacyDevice.SetBlendState pre-call was a no-op pass-through and is
            // gone now.
            var depthSprites = spritesToDraw.Where(s => s.Depth.HasValue).ToList();
            if (depthSprites.Count > 0)
                SwapChain.RenderSprites(_renderingTextures, BilinearFilter, false, depthSprites);

            // Draw ghost block bodies
            if (ShowGhostBlocks)
                DrawGhostBlockBodies(effect, ghostBlocksToDraw);

            // Hidden rooms (translucent overlay so they don't fully occlude geometry).
            // The RoomShader honours per-vertex alpha; the dedicated DepthRead state
            // is set by RenderingDrawingRoom internally when the alpha bit is on.
            var hiddenRooms = roomsToDraw.Where(r => DisablePickingForHiddenRooms && r.Properties.Hidden).ToList();
            if (hiddenRooms.Count > 0)
                foreach (Room room in hiddenRooms)
                    _renderingCachedRooms[room].Render(renderArgs);

            // Draw the height of the object and room bounding box
            DrawDebugLines(effect);

            Device.ResetState();

            // Draw the gizmo (hidden during camera preview)
            if (CanUseGizmo())
            {
                SwapChain.ClearDepth();
                _gizmo.Draw(SwapChain, _renderingStateBuffer, _viewProjection);
            }

            // Draw depth-independent sprites (HUD overlays, gizmo helpers).
            var flatSprites = spritesToDraw.Where(s => !s.Depth.HasValue).ToList();
            if (flatSprites.Count > 0)
                SwapChain.RenderSprites(_renderingTextures, BilinearFilter, true, flatSprites);

            _watch.Stop();

            // At last, construct additional labels and draw all in-game text
            DrawText(roomsToDraw, textToDraw);
        }
    }
}
