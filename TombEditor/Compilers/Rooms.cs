using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using TombEditor.Geometry;

namespace TombEditor.Compilers
{
    public partial class LevelCompilerTR4
    {
        private void BuildRooms()
        {
            ReportProgress(20, "Building rooms");

            // Map editor room indices to final level indices
            _roomsIdTable = new Dictionary<int, int>();
            var lastRoom = 0;

            for (var i = 0; i < _level.Rooms.Length - 1; i++)
            {
                var room = _level.Rooms[i];
                if (room == null) continue;

                _roomsIdTable.Add(i, lastRoom);
                lastRoom++;
            }

            // Average lighting at the portals
            MatchPortalShades();

            Rooms = new tr_room[_roomsIdTable.Count];

            for (var i = 0; i < _level.Rooms.Length - 1; i++)
            {
                var room = _level.Rooms[i];
                if (room == null) continue;

                var newRoom = new tr_room
                {
                    OriginalRoomId = i,
                    TextureSounds = new TextureSounds[room.NumXSectors, room.NumZSectors],
                    Lights = new tr4_room_light[0],
                    StaticMeshes = new tr_room_staticmesh[0],
                    Portals = new tr_room_portal[0],
                    Info = new tr_room_info
                    {
                        X = (int) (room.Position.X * 1024.0f),
                        Z = (int) (room.Position.Z * 1024.0f),
                        YTop = (int) (-room.Position.Y * 256.0f - room.GetHighestCorner() * 256.0f),
                        YBottom = (int) (-room.Position.Y * 256.0f)
                    }
                };

                newRoom.NumXSectors = room.NumXSectors;
                newRoom.NumZSectors = room.NumZSectors;
                newRoom.AlternateRoom = (short)(room.Flipped && room.AlternateRoom != -1 ? _roomsIdTable[room.AlternateRoom] : -1);
                newRoom.AlternateGroup = (byte)(room.Flipped && room.AlternateRoom != -1 ? room.AlternateGroup : 0);
                newRoom.Flipped = room.Flipped;
                newRoom.FlippedRoom = (short)(room.AlternateRoom != -1 ? _roomsIdTable[room.AlternateRoom] : -1);
                newRoom.BaseRoom = (short)(room.BaseRoom != -1 ? _roomsIdTable[room.BaseRoom] : -1);

                newRoom.AmbientIntensity2 = (ushort)(0x0000 + room.AmbientLight.R);
                newRoom.AmbientIntensity1 = (ushort)((room.AmbientLight.G << 8) + room.AmbientLight.B);

                newRoom.ReverbInfo = (byte)room.Reverberation;

                // Room flags
                newRoom.Flags = 0x40;

                if (room.FlagWater) newRoom.Flags += 0x01;
                if (room.FlagOutside) newRoom.Flags += 0x20;
                if (room.FlagHorizon) newRoom.Flags += 0x08;
                if (room.FlagQuickSand) newRoom.Flags += 0x80;
                if (room.FlagMist) newRoom.Flags += 0x100;
                if (room.FlagReflection) newRoom.Flags += 0x200;
                if (room.FlagSnow) newRoom.Flags += 0x400;
                if (room.FlagRain) newRoom.Flags += 0x800;
                if (room.FlagDamage) newRoom.Flags += 0x1000;

                // Set the water scheme. I don't know how is calculated, but I have a table of all combinations of 
                // water and reflectivity. The water scheme must be set for the TOP room, in water room is 0x00.
                var waterPortals = new List<int>();

                if (!room.FlagWater)
                {
                    for (var x = 0; x < room.NumXSectors; x++)
                    {
                        for (var z = 0; z < room.NumZSectors; z++)
                        {
                            if (room.Blocks[x, z].FloorPortal == -1)
                                continue;
                            
                            if (!_level.Rooms[_level.Portals[room.Blocks[x, z].FloorPortal].AdjoiningRoom].FlagWater)
                                continue;
                            
                            if (!waterPortals.Contains(room.Blocks[x, z].FloorPortal))
                                waterPortals.Add(room.Blocks[x, z].FloorPortal);
                        }
                    }

                    if (waterPortals.Count != 0)
                    {
                        var foundWaterRoom = _level.Portals[waterPortals[0]].AdjoiningRoom;

                        var waterRoom = _level.Rooms[foundWaterRoom];

                        if (!room.FlagReflection && waterRoom.WaterLevel == 1) newRoom.WaterScheme = 0x06;
                        if (!room.FlagReflection && waterRoom.WaterLevel == 2) newRoom.WaterScheme = 0x0a;
                        if (!room.FlagReflection && waterRoom.WaterLevel == 3) newRoom.WaterScheme = 0x0e;
                        if (!room.FlagReflection && waterRoom.WaterLevel == 4) newRoom.WaterScheme = 0x12;

                        if (room.FlagReflection && room.ReflectionLevel == 1 && waterRoom.WaterLevel == 1) newRoom.WaterScheme = 0x05;
                        if (room.FlagReflection && room.ReflectionLevel == 2 && waterRoom.WaterLevel == 1) newRoom.WaterScheme = 0x06;
                        if (room.FlagReflection && room.ReflectionLevel == 3 && waterRoom.WaterLevel == 1) newRoom.WaterScheme = 0x07;
                        if (room.FlagReflection && room.ReflectionLevel == 4 && waterRoom.WaterLevel == 1) newRoom.WaterScheme = 0x08;

                        if (room.FlagReflection && room.ReflectionLevel == 1 && waterRoom.WaterLevel == 2) newRoom.WaterScheme = 0x09;
                        if (room.FlagReflection && room.ReflectionLevel == 2 && waterRoom.WaterLevel == 2) newRoom.WaterScheme = 0x0a;
                        if (room.FlagReflection && room.ReflectionLevel == 3 && waterRoom.WaterLevel == 2) newRoom.WaterScheme = 0x0b;
                        if (room.FlagReflection && room.ReflectionLevel == 4 && waterRoom.WaterLevel == 2) newRoom.WaterScheme = 0x0c;

                        if (room.FlagReflection && room.ReflectionLevel == 1 && waterRoom.WaterLevel == 3) newRoom.WaterScheme = 0x0d;
                        if (room.FlagReflection && room.ReflectionLevel == 2 && waterRoom.WaterLevel == 3) newRoom.WaterScheme = 0x0e;
                        if (room.FlagReflection && room.ReflectionLevel == 3 && waterRoom.WaterLevel == 3) newRoom.WaterScheme = 0x0f;
                        if (room.FlagReflection && room.ReflectionLevel == 4 && waterRoom.WaterLevel == 3) newRoom.WaterScheme = 0x10;

                        if (room.FlagReflection && room.ReflectionLevel == 1 && waterRoom.WaterLevel == 4) newRoom.WaterScheme = 0x11;
                        if (room.FlagReflection && room.ReflectionLevel == 2 && waterRoom.WaterLevel == 4) newRoom.WaterScheme = 0x12;
                        if (room.FlagReflection && room.ReflectionLevel == 3 && waterRoom.WaterLevel == 4) newRoom.WaterScheme = 0x13;
                        if (room.FlagReflection && room.ReflectionLevel == 4 && waterRoom.WaterLevel == 4) newRoom.WaterScheme = 0x14;
                    }
                }

                if (room.FlagMist) newRoom.WaterScheme += (byte)room.MistLevel;

                var lowest = -room.GetLowestCorner() * 256 + newRoom.Info.YBottom;

                // Prepare optimized vertices
                room.OptimizedVertices = new List<EditorVertex>();
                var indicesDictionary = new Dictionary<int, int>();

                for (var x = 0; x < room.NumXSectors; x++)
                {
                    for (var z = 0; z < room.NumZSectors; z++)
                    {
                        var base1 = (short)((x << 9) + (z << 4));

                        for (var n = 0; n < room.NumVerticesInGrid[x, z]; n++)
                        {
                            indicesDictionary.Add(base1 + n, room.OptimizedVertices.Count);
                            room.OptimizedVertices.Add(room.VerticesGrid[x, z, n]);
                        }
                    }
                }

                newRoom.Vertices = new tr_room_vertex[room.OptimizedVertices.Count];
                for (var j = 0; j < room.OptimizedVertices.Count; j++)
                {
                    var rv = new tr_room_vertex();

                    var v = new tr_vertex
                    {
                        X = (short) room.OptimizedVertices[j].Position.X,
                        Y = (short) (-room.OptimizedVertices[j].Position.Y + newRoom.Info.YBottom),
                        Z = (short) room.OptimizedVertices[j].Position.Z
                    };

                    rv.Vertex = v;
                    rv.Lighting1 = 0;
                    rv.Lighting2 = (short)Pack24BitColorTo16Bit(room.OptimizedVertices[j].FaceColor);
                    rv.Attributes = 0;

                    // Water special effects
                    if (room.FlagWater)
                    {
                        rv.Attributes = 0x4000;
                    }
                    else
                    {
                        foreach (var portalId in waterPortals)
                        {
                            var portal = _level.Portals[portalId];

                            if (v.X > portal.X * 1024 && v.X < (portal.X + portal.NumXBlocks) * 1024 &&
                                v.Z > portal.Z * 1024 && v.Z < (portal.Z + portal.NumZBlocks) * 1024 &&
                                v.Y == lowest)
                            {
                                var xv = v.X / 1024;
                                var zv = v.Z / 1024;

                                if (!(room.Blocks[xv, zv].IsFloorSolid || room.Blocks[xv, zv].Type == BlockType.Wall || room.Blocks[xv, zv].Type == BlockType.BorderWall) &&
                                    !(room.Blocks[xv - 1, zv].IsFloorSolid || room.Blocks[xv - 1, zv].Type == BlockType.Wall || room.Blocks[xv - 1, zv].Type == BlockType.BorderWall) &&
                                    !(room.Blocks[xv, zv - 1].IsFloorSolid || room.Blocks[xv, zv - 1].Type == BlockType.Wall || room.Blocks[xv, zv - 1].Type == BlockType.BorderWall) &&
                                    !(room.Blocks[xv - 1, zv - 1].IsFloorSolid || room.Blocks[xv - 1, zv - 1].Type == BlockType.Wall || room.Blocks[xv - 1, zv - 1].Type == BlockType.BorderWall))
                                {
                                    rv.Attributes = 0x6000;
                                }
                            }
                            else
                            {
                                if (!room.FlagReflection)
                                    continue;
                                
                                if (v.X >= (portal.X - 1) * 1024 && v.X <= (portal.X + portal.NumXBlocks + 1) * 1024 &&
                                    v.Z >= (portal.Z - 1) * 1024 && v.Z <= (portal.Z + portal.NumZBlocks + 1) * 1024)
                                {
                                    rv.Attributes = 0x4000;
                                }
                            }
                        }
                    }

                    newRoom.Vertices[j] = rv;
                }

                var tempRectangles = new List<tr_face4>();
                var tempTriangles = new List<tr_face3>();

                for (var x = 0; x < room.NumXSectors; x++)
                {
                    for (var z = 0; z < room.NumZSectors; z++)
                    {
                        for (var f = 0; f < room.Blocks[x, z].Faces.Length; f++)
                        {
                            var face = room.Blocks[x, z].Faces[f];
                            if (face == null || !face.Defined) continue;

                            if ((f == 25 || f == 26) && (face.Invisible || face.Texture == -1))
                            {
                                newRoom.TextureSounds[x, z] = TextureSounds.Stone;
                            }

                            if (face.Invisible) continue;

                            // Assign texture sound
                            if ((f == (int)BlockFaces.Floor || f == (int)BlockFaces.FloorTriangle2))
                            {
                                newRoom.TextureSounds[x, z] = (face.Texture != -1 ? GetTextureSound(face.Texture) : TextureSounds.Stone);
                            }

                            if (face.Shape == BlockFaceShape.Rectangle)
                            {
                                var rectangle = new tr_face4();

                                var indices = new List<ushort>
                                {
                                    (ushort) indicesDictionary[face.Indices[0]],
                                    (ushort) indicesDictionary[face.Indices[1]],
                                    (ushort) indicesDictionary[face.Indices[2]],
                                    (ushort) indicesDictionary[face.Indices[3]]
                                };


                                var rot = face.Rotation;
                                if (rot != 0)
                                {
                                    for (var n = 0; n < rot; n++)
                                    {
                                        var tmp = indices[0];
                                        indices.RemoveAt(0);
                                        indices.Insert(3, tmp);
                                    }
                                }

                                rectangle.Vertices = new ushort[4];
                                rectangle.Vertices[0] = indices[0];
                                rectangle.Vertices[1] = indices[1];
                                rectangle.Vertices[2] = indices[2];
                                rectangle.Vertices[3] = indices[3];

                                rectangle.Texture = face.NewTexture;

                                tempRectangles.Add(rectangle);
                            }
                            else
                            {
                                var triangle = new tr_face3();

                                var indices = new List<ushort>
                                {
                                    (ushort) indicesDictionary[face.Indices[0]],
                                    (ushort) indicesDictionary[face.Indices[1]],
                                    (ushort) indicesDictionary[face.Indices[2]]
                                };


                                var rot = face.Rotation;
                                if (rot != 0)
                                {
                                    for (var n = 0; n < rot; n++)
                                    {
                                        var tmp = indices[0];
                                        indices.RemoveAt(0);
                                        indices.Insert(2, tmp);
                                    }
                                }

                                triangle.Vertices = new ushort[4];
                                triangle.Vertices[0] = indices[0];
                                triangle.Vertices[1] = indices[1];
                                triangle.Vertices[2] = indices[2];

                                triangle.Texture = face.NewTexture;

                                tempTriangles.Add(triangle);
                            }
                        }
                    }
                }

                newRoom.Rectangles = tempRectangles.ToArray();
                newRoom.Triangles = tempTriangles.ToArray();

                newRoom.NumRectangles = (ushort)tempRectangles.Count;
                newRoom.NumTriangles = (ushort)tempTriangles.Count;

                // Build portals
                var tempPortals = new List<tr_room_portal>();
                var tempIdPortals = new List<int>();

                for (var z = 0; z < room.NumZSectors; z++)
                {
                    for (var x = 0; x < room.NumXSectors; x++)
                    {
                        if (room.Blocks[x, z].WallPortal >= 0 && !tempIdPortals.Contains(room.Blocks[x, z].WallPortal))
                            tempIdPortals.Add(room.Blocks[x, z].WallPortal);

                        if (room.Blocks[x, z].FloorPortal >= 0 && !tempIdPortals.Contains(room.Blocks[x, z].FloorPortal))
                            tempIdPortals.Add(room.Blocks[x, z].FloorPortal);

                        if (room.Blocks[x, z].CeilingPortal >= 0 && !tempIdPortals.Contains(room.Blocks[x, z].CeilingPortal))
                            tempIdPortals.Add(room.Blocks[x, z].CeilingPortal);
                    }
                }

                foreach (var portalId in tempIdPortals)
                {
                    var portal = _editor.Level.Portals[portalId];
                    var newPortal = new tr_room_portal();

                    var xMin = 0;
                    var xMax = 0;
                    var zMin = 0;
                    var zMax = 0;

                    newPortal.AdjoiningRoom = (ushort)_roomsIdTable[(int)portal.AdjoiningRoom];

                    newPortal.Vertices = new tr_vertex[4];

                    // Normal and vertices
                    switch (portal.Direction)
                    {
                        case PortalDirection.North:
                        {
                            newPortal.Normal = new tr_vertex
                            {
                                X = 0,
                                Y = 0,
                                Z = -1
                            };

                            xMin = portal.X;
                            xMax = portal.X + portal.NumXBlocks;
                            zMin = room.NumZSectors - 1;
                            zMax = room.NumZSectors - 1;

                            var yMin = 32768;
                            var yMax = -32768;

                            int y1;
                            int y2;

                            for (var x = xMin; x < xMax; x++)
                            {
                                var currentBlock = room.Blocks[x, room.NumZSectors - 2];
                                var facingBlock = room.Blocks[x, room.NumZSectors - 1];

                                y1 = Math.Max(facingBlock.QAFaces[3], currentBlock.QAFaces[0]);
                                y2 = Math.Min(facingBlock.WSFaces[3], currentBlock.WSFaces[0]);

                                if (y1 < yMin) yMin = y1;
                                if (y2 > yMax) yMax = y2;
                            }

                            var lastBlock = room.Blocks[xMax - 1, room.NumZSectors - 2];
                            var lastFacingBlock = room.Blocks[xMax - 1, room.NumZSectors - 1];

                            y1 = Math.Max(lastFacingBlock.QAFaces[2], lastBlock.QAFaces[1]);
                            y2 = Math.Min(lastFacingBlock.WSFaces[2], lastBlock.WSFaces[1]);

                            if (y1 < yMin) yMin = y1;
                            if (y2 > yMax) yMax = y2;

                            yMax += room.Ceiling;

                            newPortal.Vertices[0] = new tr_vertex
                            {
                                X = (short) (xMin * 1024.0f),
                                Y = (short) (-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMin * 1024.0f)
                            };

                            newPortal.Vertices[1] = new tr_vertex
                            {
                                X = (short) (xMax * 1024.0f),
                                Y = (short) (-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMin * 1024.0f)
                            };

                            newPortal.Vertices[2] = new tr_vertex
                            {
                                X = (short) (xMax * 1024.0f),
                                Y = (short) (-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMax * 1024.0f)
                            };

                            newPortal.Vertices[3] = new tr_vertex
                            {
                                X = (short) (xMin * 1024.0f),
                                Y = (short) (-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMax * 1024.0f)
                            };
                        }
                            break;
                        case PortalDirection.East:
                        {
                            newPortal.Normal = new tr_vertex
                            {
                                X = -1,
                                Y = 0,
                                Z = 0
                            };

                            xMin = room.NumXSectors - 1;
                            xMax = room.NumXSectors - 1;
                            zMin = portal.Z + portal.NumZBlocks;
                            zMax = portal.Z;

                            var yMin = 32768;
                            var yMax = -32768;

                            int y1;
                            int y2;

                            for (var z = zMax; z < zMin; z++)
                            {
                                var currentBlock = room.Blocks[room.NumXSectors - 2, z];
                                var facingBlock = room.Blocks[room.NumXSectors - 1, z];

                                y1 = Math.Max(facingBlock.QAFaces[0], currentBlock.QAFaces[1]);
                                y2 = Math.Min(facingBlock.WSFaces[0], currentBlock.WSFaces[1]);

                                if (y1 < yMin) yMin = y1;
                                if (y2 > yMax) yMax = y2;
                            }

                            var lastBlock = room.Blocks[room.NumXSectors - 2, zMin - 1];
                            var lastFacingBlock = room.Blocks[room.NumXSectors - 1, zMin - 1];

                            y1 = Math.Max(lastFacingBlock.QAFaces[3], lastBlock.QAFaces[2]);
                            y2 = Math.Min(lastFacingBlock.WSFaces[3], lastBlock.WSFaces[2]);

                            if (y1 < yMin) yMin = y1;
                            if (y2 > yMax) yMax = y2;

                            yMax += room.Ceiling;

                            newPortal.Vertices[1] = new tr_vertex
                            {
                                X = (short) (xMin * 1024),
                                Y = (short) (-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMin * 1024.0f)
                            };

                            newPortal.Vertices[2] = new tr_vertex
                            {
                                X = (short) (xMax * 1024),
                                Y = (short) (-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMax * 1024.0f)
                            };

                            newPortal.Vertices[3] = new tr_vertex
                            {
                                X = (short) (xMax * 1024),
                                Y = (short) (-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMax * 1024.0f)
                            };

                            newPortal.Vertices[0] = new tr_vertex
                            {
                                X = (short) (xMin * 1024),
                                Y = (short) (-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMin * 1024.0f)
                            };
                        }
                            break;
                        case PortalDirection.South:
                        {
                            newPortal.Normal = new tr_vertex
                            {
                                X = 0,
                                Y = 0,
                                Z = 1
                            };

                            xMin = portal.X + portal.NumXBlocks;
                            xMax = portal.X;
                            zMin = 1;
                            zMax = 1;

                            var yMin = 32768;
                            var yMax = -32768;

                            int y1;
                            int y2;

                            for (var x = xMax; x < xMin; x++)
                            {
                                var currentBlock = room.Blocks[x, 1];
                                var facingBlock = room.Blocks[x, 0];

                                y1 = Math.Max(facingBlock.QAFaces[1], currentBlock.QAFaces[2]);
                                y2 = Math.Min(facingBlock.WSFaces[1], currentBlock.WSFaces[2]);

                                if (y1 < yMin) yMin = y1;
                                if (y2 > yMax) yMax = y2;
                            }

                            var lastBlock = room.Blocks[xMin - 1, 1];
                            var lastFacingBlock = room.Blocks[xMin - 1, 0];

                            y1 = Math.Max(lastFacingBlock.QAFaces[0], lastBlock.QAFaces[3]);
                            y2 = Math.Min(lastFacingBlock.WSFaces[0], lastBlock.WSFaces[3]);

                            if (y1 < yMin) yMin = y1;
                            if (y2 > yMax) yMax = y2;

                            yMax += room.Ceiling;

                            newPortal.Vertices[0] = new tr_vertex
                            {
                                X = (short) (xMin * 1024.0f),
                                Y = (short) (-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMin * 1024.0f - 1.0f)
                            };

                            newPortal.Vertices[1] = new tr_vertex
                            {
                                X = (short) (xMax * 1024.0f),
                                Y = (short) (-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMin * 1024.0f - 1.0f)
                            };

                            newPortal.Vertices[2] = new tr_vertex
                            {
                                X = (short) (xMax * 1024.0f),
                                Y = (short) (-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMax * 1024.0f - 1.0f)
                            };

                            newPortal.Vertices[3] = new tr_vertex
                            {
                                X = (short) (xMin * 1024.0f),
                                Y = (short) (-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMax * 1024.0f - 1.0f)
                            };
                        }
                            break;
                        case PortalDirection.West:
                        {
                            newPortal.Normal = new tr_vertex
                            {
                                X = 1,
                                Y = 0,
                                Z = 0
                            };

                            xMin = 1;
                            xMax = 1;
                            zMin = portal.Z;
                            zMax = portal.Z + portal.NumZBlocks;

                            var yMin = 32768;
                            var yMax = -32768;

                            int y1;
                            int y2;

                            for (var z = zMin; z < zMax; z++)
                            {
                                var currentBlock = room.Blocks[1, z];
                                var facingBlock = room.Blocks[0, z];

                                y1 = Math.Max(facingBlock.QAFaces[2], currentBlock.QAFaces[3]);
                                y2 = Math.Min(facingBlock.WSFaces[2], currentBlock.WSFaces[3]);

                                if (y1 < yMin) yMin = y1;
                                if (y2 > yMax) yMax = y2;
                            }

                            var lastBlock = room.Blocks[1, zMax - 1];
                            var lastFacingBlock = room.Blocks[0, zMax - 1];

                            y1 = Math.Max(lastFacingBlock.QAFaces[1], lastBlock.QAFaces[0]);
                            y2 = Math.Min(lastFacingBlock.WSFaces[1], lastBlock.WSFaces[0]);

                            if (y1 < yMin) yMin = y1;
                            if (y2 > yMax) yMax = y2;

                            yMax += room.Ceiling;

                            newPortal.Vertices[1] = new tr_vertex
                            {
                                X = (short) (xMin * 1024.0f - 1.0f),
                                Y = (short) (-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMin * 1024.0f)
                            };

                            newPortal.Vertices[2] = new tr_vertex
                            {
                                X = (short) (xMax * 1024.0f - 1.0f),
                                Y = (short) (-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMax * 1024.0f)
                            };

                            newPortal.Vertices[3] = new tr_vertex
                            {
                                X = (short) (xMax * 1024.0f - 1.0f),
                                Y = (short) (-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMax * 1024.0f)
                            };

                            newPortal.Vertices[0] = new tr_vertex
                            {
                                X = (short) (xMin * 1024.0f - 1.0f),
                                Y = (short) (-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMin * 1024.0f)
                            };
                        }
                            break;
                        case PortalDirection.Floor:
                        {
                            newPortal.Normal = new tr_vertex
                            {
                                X = 0,
                                Y = -1,
                                Z = 0
                            };

                            xMin = portal.X;
                            xMax = portal.X + portal.NumXBlocks;
                            var y = room.GetLowestCorner();
                            zMin = portal.Z;
                            zMax = portal.Z + portal.NumZBlocks;

                            newPortal.Vertices[1] = new tr_vertex
                            {
                                X = (short) (xMin * 1024.0f),
                                Y = (short) (-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMin * 1024.0f)
                            };

                            newPortal.Vertices[2] = new tr_vertex
                            {
                                X = (short) (xMin * 1024.0f),
                                Y = (short) (-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMax * 1024.0f)
                            };

                            newPortal.Vertices[3] = new tr_vertex
                            {
                                X = (short) (xMax * 1024.0f),
                                Y = (short) (-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMax * 1024.0f)
                            };

                            newPortal.Vertices[0] = new tr_vertex
                            {
                                X = (short) (xMax * 1024.0f),
                                Y = (short) (-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMin * 1024.0f)
                            };
                        }
                            break;
                        case PortalDirection.Ceiling:
                        {
                            newPortal.Normal = new tr_vertex
                            {
                                X = 0,
                                Y = 1,
                                Z = 0
                            };

                            xMin = portal.X;
                            xMax = portal.X + portal.NumXBlocks;
                            var y = room.GetHighestCorner();
                            zMin = portal.Z + portal.NumZBlocks;
                            zMax = portal.Z;

                            newPortal.Vertices[1] = new tr_vertex
                            {
                                X = (short) (xMin * 1024.0f),
                                Y = (short) (-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMin * 1024.0f)
                            };

                            newPortal.Vertices[2] = new tr_vertex
                            {
                                X = (short) (xMin * 1024.0f),
                                Y = (short) (-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMax * 1024.0f)
                            };

                            newPortal.Vertices[3] = new tr_vertex
                            {
                                X = (short) (xMax * 1024.0f),
                                Y = (short) (-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMax * 1024.0f)
                            };

                            newPortal.Vertices[0] = new tr_vertex
                            {
                                X = (short) (xMax * 1024.0f),
                                Y = (short) (-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short) (zMin * 1024.0f)
                            };
                        }
                            break;
                    }

                    tempPortals.Add(newPortal);
                }

                newRoom.NumPortals = (ushort)tempPortals.Count;
                newRoom.Portals = tempPortals.ToArray();

                newRoom.Sectors = new tr_room_sector[room.NumXSectors * room.NumZSectors];
                newRoom.AuxSectors = new tr_sector_aux[room.NumXSectors, room.NumZSectors];

                for (var z = 0; z < room.NumZSectors; z++)
                {
                    for (var x = 0; x < room.NumXSectors; x++)
                    {
                        var sector = new tr_room_sector();
                        var aux = new tr_sector_aux();

                        /* if (x == 0 || z == 0 || x == room.NumXSectors - 1 || z == room.NumZSectors - 1 || room.Blocks[x, z].Type == BlockType.Wall ||
                            Math.Abs(room.Blocks[x, z].FloorSlopeX) > 2 || Math.Abs(room.Blocks[x, z].FloorSlopeZ) > 2)
                        {*/
                            sector.BoxIndex = 0x7ff6;
                      /*  }*
                        else
                        {
                            sector.BoxIndex = sound;
                        }*/

                        sector.FloorDataIndex = 0;

                        if (room.Blocks[x, z].FloorPortal >= 0)
                            sector.RoomBelow = (byte)_roomsIdTable[_editor.Level.Portals[room.Blocks[x, z].FloorPortal].AdjoiningRoom];
                        else
                            sector.RoomBelow = 0xff;

                        if (room.Blocks[x, z].CeilingPortal >= 0)
                            sector.RoomAbove = (byte)_roomsIdTable[_editor.Level.Portals[room.Blocks[x, z].CeilingPortal].AdjoiningRoom];
                        else
                            sector.RoomAbove = 0xff;

                        if (x == 0 || z == 0 || x == room.NumXSectors - 1 || z == room.NumZSectors - 1 ||
                            room.Blocks[x, z].Type == BlockType.BorderWall || room.Blocks[x, z].Type == BlockType.Wall)
                        {
                            sector.Floor = (sbyte)(-room.Position.Y - room.GetHighestFloorCorner(x, z));
                            sector.Ceiling = (sbyte)(-room.Position.Y - room.Ceiling - room.GetLowestCeilingCorner(x, z));
                        }
                        else
                        {
                            sector.Floor = (sbyte)(-room.Position.Y - room.GetHighestFloorCorner(x, z));
                            sector.Ceiling = (sbyte)(-room.Position.Y - room.Ceiling - room.GetLowestCeilingCorner(x, z));
                        }

                        //Setup some aux data for box generation
                        if (room.Blocks[x, z].Type == BlockType.BorderWall) aux.BorderWall = true;
                        if ((room.Blocks[x, z].Flags & BlockFlags.Monkey) != 0) aux.Monkey = true;
                        if ((room.Blocks[x, z].Flags & BlockFlags.Box) != 0) aux.Box = true;
                        if ((room.Blocks[x, z].Flags & BlockFlags.NotWalkableFloor) != 0) aux.NotWalkableFloor = true;
                        if (!room.FlagWater && (Math.Abs(room.Blocks[x, z].FloorSlopeX) == 1 || Math.Abs(room.Blocks[x, z].FloorSlopeX) == 2 ||
                            Math.Abs(room.Blocks[x, z].FloorSlopeZ) == 1 || Math.Abs(room.Blocks[x, z].FloorSlopeZ) == 2)) aux.SoftSlope = true;
                        if (!room.FlagWater && (Math.Abs(room.Blocks[x, z].FloorSlopeX) > 2 || Math.Abs(room.Blocks[x, z].FloorSlopeZ) > 2)) aux.HardSlope = true;
                        if (room.Blocks[x, z].Type == BlockType.Wall) aux.Wall = true;

                        // I must setup portal only if current sector is not solid and opacity if different from 1
                        if (room.Blocks[x, z].FloorPortal != -1)
                        {
                            if ((!room.Blocks[x, z].IsFloorSolid && room.Blocks[x, z].FloorOpacity != PortalOpacity.Opacity1) ||
                                (room.Blocks[x, z].IsFloorSolid && room.Blocks[x, z].NoCollisionFloor))
                            {
                                Portal portal = _editor.Level.Portals[room.Blocks[x, z].FloorPortal];
                                sector.RoomBelow = (byte)_roomsIdTable[portal.AdjoiningRoom];
                            }
                            else
                            {
                                sector.RoomBelow = 255;
                            }
                        }
                        else
                        {
                            sector.RoomBelow = 255;
                        }

                        if ((room.Blocks[x, z].FloorPortal != -1 && room.Blocks[x, z].FloorOpacity != PortalOpacity.Opacity1 && !room.Blocks[x, z].IsFloorSolid))
                        {
                            aux.Portal = true;
                            aux.FloorPortal = room.Blocks[x, z].FloorPortal;
                        }
                        else
                        {
                            aux.FloorPortal = -1;
                        }

                        aux.IsFloorSolid = room.Blocks[x, z].IsFloorSolid;

                        aux.MeanFloorHeight = (sbyte)(-room.Position.Y - room.GetMeanFloorHeight(x, z));

                        if ((room.Blocks[x, z].CeilingPortal != -1 && room.Blocks[x, z].CeilingOpacity != PortalOpacity.Opacity1))
                        {
                            aux.CeilingPortal = room.Blocks[x, z].CeilingPortal;
                        }
                        else
                        {
                            aux.CeilingPortal = -1;
                        }

                        if (room.Blocks[x, z].WallPortal != -1 && room.Blocks[x, z].WallOpacity != PortalOpacity.Opacity1)
                            aux.WallPortal = _roomsIdTable[_editor.Level.Portals[room.Blocks[x, z].WallPortal].AdjoiningRoom];
                        else
                            aux.WallPortal = -1;

                        aux.LowestFloor = (sbyte)(-room.Position.Y - room.GetLowestFloorCorner(x, z));
                        var q0 = room.Blocks[x, z].QAFaces[0];
                        var q1 = room.Blocks[x, z].QAFaces[1];
                        var q2 = room.Blocks[x, z].QAFaces[2];
                        var q3 = room.Blocks[x, z].QAFaces[3];

                        if (!Room.IsQuad(x, z, q0, q1, q2, q3, true) && room.Blocks[x, z].FloorSlopeX == 0 && room.Blocks[x, z].FloorSlopeZ == 0)
                        {
                            if (room.Blocks[x, z].RealSplitFloor == 0)
                            {
                                aux.LowestFloor = (sbyte)(-room.Position.Y - Math.Min(room.Blocks[x, z].QAFaces[0], room.Blocks[x, z].QAFaces[2]));
                            }
                            else
                            {
                                aux.LowestFloor = (sbyte)(-room.Position.Y - Math.Min(room.Blocks[x, z].QAFaces[1], room.Blocks[x, z].QAFaces[3]));
                            }
                        }

                        newRoom.AuxSectors[x, z] = aux;
                        newRoom.Sectors[room.NumZSectors * x + z] = sector;
                    }
                }

                var tempStaticMeshes = new List<tr_room_staticmesh>();

                foreach (var staticMesh in room.StaticMeshes)
                {
                    var instance = (StaticMeshInstance)_editor.Level.Objects[staticMesh];
                    
                    var sm = new tr_room_staticmesh
                    {
                        X = (uint) (newRoom.Info.X + instance.Position.X),
                        Y = (uint) (newRoom.Info.YBottom - instance.Position.Y),
                        Z = (uint) (newRoom.Info.Z + instance.Position.Z),
                        Rotation = (ushort) (instance.Rotation / 45 * 8192),
                        ObjectID = (ushort) instance.Model.ObjectID,
                        Intensity1 = Pack24BitColorTo16Bit(instance.Color),
                        Intensity2 = 0
                    };

                    tempStaticMeshes.Add(sm);
                }

                newRoom.NumStaticMeshes = (ushort)tempStaticMeshes.Count;
                newRoom.StaticMeshes = tempStaticMeshes.ToArray();

                var tempLights = new List<tr4_room_light>();
                foreach (var light in room.Lights)
                {
                    if (light.Type == LightType.Effect) continue;

                    var newLight = new tr4_room_light
                    {
                        X = (int) (newRoom.Info.X + light.Position.X),
                        Y = (int) (-light.Position.Y + newRoom.Info.YBottom),
                        Z = (int) (newRoom.Info.Z + light.Position.Z),
                        Color = new tr_color
                        {
                            Red = light.Color.R,
                            Green = light.Color.G,
                            Blue = light.Color.B
                        },
                        Intensity = (ushort) (((short) (Math.Abs(light.Intensity) * 31.0f) << 8) | 0x00ff)
                    };

                    switch (light.Type)
                    {
                        case LightType.Light:
                            // Point light
                            newLight.LightType = 1;
                            newLight.In = light.In * 1024;
                            newLight.Out = light.Out * 1024;
                            break;
                        case LightType.Shadow:
                            // Point shadow
                            newLight.LightType = 3;
                            newLight.In = light.In * 1024;
                            newLight.Out = light.Out * 1024;
                            break;
                        case LightType.Spot:
                            // Spot light
                            newLight.LightType = 2;
                            newLight.In = (float)Math.Cos(MathUtil.DegreesToRadians(light.In));
                            newLight.Out = (float)Math.Cos(MathUtil.DegreesToRadians(light.Out));
                            newLight.Length = light.Len * 1024.0f;
                            newLight.CutOff = light.Cutoff * 1024.0f;
                            newLight.DirectionX = (float)(-Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Sin(MathUtil.DegreesToRadians(light.DirectionY)));
                            newLight.DirectionY = (float)(Math.Sin(MathUtil.DegreesToRadians(light.DirectionX)));
                            newLight.DirectionZ = (float)(-Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Cos(MathUtil.DegreesToRadians(light.DirectionY)));
                            break;
                        case LightType.Sun:
                            // Sun light
                            newLight.LightType = 0;
                            newLight.In = 0;
                            newLight.Out = 0;
                            newLight.Length = 0;
                            newLight.CutOff = 0;
                            newLight.DirectionX = -(float)(Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Sin(MathUtil.DegreesToRadians(light.DirectionY)));
                            newLight.DirectionY = -(float)(-Math.Sin(MathUtil.DegreesToRadians(light.DirectionX)));
                            newLight.DirectionZ = -(float)(Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Cos(MathUtil.DegreesToRadians(light.DirectionY)));
                            break;
                        case LightType.FogBulb:
                            // Fog bulb
                            newLight.LightType = 4;
                            newLight.In = light.In * 1024;
                            newLight.Out = light.Out * 1024;
                            break;
                    }

                    tempLights.Add(newLight);
                }

                newRoom.NumLights = (ushort)tempLights.Count;
                newRoom.Lights = tempLights.ToArray();

                Rooms[_roomsIdTable[i]] = newRoom;
            }

            ReportProgress(25, "    Number of rooms: " + Rooms.Length);
        }
        
        private void PrepareRoomTextures()
        {
            _animTexturesRooms = new List<int>();
            _animTexturesGeneral = new List<int>();

            ReportProgress(1, "Preparing room textures");

            // Reset animated textures
            foreach (var texture in _level.AnimatedTextures)
            {
                texture.Variants = new List<AnimatedTextureSequenceVariant>();
                foreach (var texture2 in texture.Textures)
                {
                    texture2.NewID = -1;
                }
            }

            _tempAnimatedTextures = new List<AnimatedTextureSequenceVariant>();

            _tempObjectTextures = new List<tr_object_texture>();

            // I start with a 128 pages texture map (32 MB in memory)
            var roomTextureMap = new byte[1024, 32768];

            // First, I have to filter only used textures and sort them (for now I use bubble sort, in the future a tree)
            var tempTexturesList = new List<LevelTexture>();
            _texturesIdTable = new Dictionary<int, int>();

            for (var i = 0; i < _editor.Level.TextureSamples.Count; i++)
            {
                var oldSample = _editor.Level.TextureSamples[i];

                // don't count for unused textures
                // if (oldSample.UsageCount <= 0) continue;

                oldSample.OldID = oldSample.ID;

                tempTexturesList.Add(oldSample);
            }

            ReportProgress(2, "Sorting room textures");

            for (var i = 0; i < _level.AnimatedTextures.Count; i++)
            {
                tempTexturesList.AddRange(_level.AnimatedTextures[i]
                    .Textures.Select((t, k) => new LevelTexture
                    {
                        X = t.X,
                        Y = t.Y,
                        Width = 64,
                        Height = 64,
                        Page = t.Page,
                        Animated = true,
                        AnimatedSequence = i,
                        AnimatedTexture = k
                    }));
            }

            _tempTexturesArray = tempTexturesList.ToArray();

            ReportProgress(3, "Building room texture map");

            // I've sorted the textures by height, now I build the texture map
            var numRoomTexturePages = _editor.Level.TextureMap.Height / 256;
            for (var x = 0; x < 256; x++)
            {
                for (var y = 0; y < _editor.Level.TextureMap.Height; y++)
                {
                    var c = _editor.Level.TextureMap.GetPixel(x, y);

                    if (c.R == 255 & c.G == 0 && c.B == 255)
                    {
                        roomTextureMap[x * 4 + 0, y] = 0;
                        roomTextureMap[x * 4 + 1, y] = 0;
                        roomTextureMap[x * 4 + 2, y] = 0;
                        roomTextureMap[x * 4 + 3, y] = 0;
                    }
                    else
                    {
                        roomTextureMap[x * 4 + 0, y] = c.B;
                        roomTextureMap[x * 4 + 1, y] = c.G;
                        roomTextureMap[x * 4 + 2, y] = c.R;
                        roomTextureMap[x * 4 + 3, y] = 255;
                    }
                }
            }

            // Rebuild the ID table
            for (var i = 0; i < _tempTexturesArray.Length; i++)
            {
                var tex = _tempTexturesArray[i];
                tex.AlphaTest = true;
                tex.NewX = tex.X;
                tex.NewY = tex.Y;
                tex.NewPage = tex.Page;

                if (!tex.Animated)
                    _texturesIdTable.Add(tex.OldID, i);
                else
                    _level.AnimatedTextures[tex.AnimatedSequence].Textures[tex.AnimatedTexture].Texture = tex;
            }

            // Build the TR4 texture tiles

            foreach (var room in _editor.Level.Rooms)
            {
                if (room == null) continue;

                for (var x = 0; x < room.NumXSectors; x++)
                {
                    for (var z = 0; z < room.NumZSectors; z++)
                    {
                        foreach (var face in room.Blocks[x, z].Faces)
                        {
                            // Ignore undefined faces and untextured faces
                            if (!face.Defined || face.Texture == -1)
                                continue;

                            // Build (or get) the texture info
                            face.NewTexture = BuildTextureInfo(face);
                        }
                    }
                }
            }

            _roomTexturePages = new byte[numRoomTexturePages * 256 * 256 * 4];
            for (var y = 0; y < 256 * numRoomTexturePages; y++)
            {
                for (var x = 0; x < 1024; x++)
                {
                    _roomTexturePages[y * 1024 + x] = roomTextureMap[x, y];
                }
            }

            _numRoomTexturePages = numRoomTexturePages;

            ReportProgress(5, "    Room texture pages: " + _numRoomTexturePages);

            ReportProgress(6, "Building animated textures table");

            // Prepare animated textures

            // Build remaining tiles
            foreach (var textureSet in _level.AnimatedTextures)
            {
                foreach (var texture in textureSet.Variants)
                {
                    for (var k = 0; k < texture.Tiles.Count; k++)
                    {
                        if (texture.Tiles[k].NewID == -1)
                        {
                            texture.Tiles[k].NewID = BuildAnimatedTextureInfo(texture,
                                texture.Tiles[k],
                                textureSet.Textures[k].Texture);
                        }
                    }
                }
            }

            NumAnimatedTextures = 0;
            var tempAnimatedTextures = new List<tr_animated_textures_set>();
            foreach (var textureSet in _level.AnimatedTextures)
            {
                foreach (var sequence in textureSet.Variants)
                {
                    var newSet = new tr_animated_textures_set();

                    var tempTextureIds = new List<short>();
                    foreach (var tile in sequence.Tiles)
                    {
                        tempTextureIds.Add((short)tile.NewID);
                        if (!_animTexturesGeneral.Contains(tile.NewID))
                            _animTexturesGeneral.Add(tile.NewID);

                        NumAnimatedTextures++;
                    }

                    if (tempTextureIds.Count <= 0)
                        continue;
                    
                    newSet.Textures = tempTextureIds.ToArray();
                    newSet.NumTextures = (short)(newSet.Textures.Length - 1);
                    tempAnimatedTextures.Add(newSet);
                    NumAnimatedTextures++;
                }
            }

            // This because between NumAnimatedTextures and the array itself there's an extra short with the number of sets
            NumAnimatedTextures++;

            AnimatedTextures = tempAnimatedTextures.ToArray();

            _animTexturesGeneral.Sort();
            _animTexturesRooms.Sort();
        }
        
        private void MatchPortalShades()
        {
            //  return; 

            for (var i = 0; i < _level.Portals.Count; i++)
            {
                _level.Portals[i].LightAveraged = false;
            }

            for (var i = 0; i < _level.Portals.Count; i++)
            {
                // Get current portal and its paired portal
                var currentPortal = _level.Portals.ElementAt(i).Value;
                var otherPortal = _level.Portals[currentPortal.OtherID];

                // If the light was already averaged, then continue loop
                //if (currentPortal.LightAveraged) continue;

                // Get the rooms
                var currentRoom = _level.Rooms[currentPortal.Room];
                var otherRoom = _level.Rooms[otherPortal.Room];

                if (currentPortal.Direction == PortalDirection.North)
                {
                    for (int x = currentPortal.X; x <= currentPortal.X + currentPortal.NumXBlocks; x++)
                    {
                        var facingX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);

                        for (var m = 0; m < currentRoom.NumVerticesInGrid[x, currentRoom.NumZSectors - 1]; m++)
                        {
                            var v1 = currentRoom.VerticesGrid[x, currentRoom.NumZSectors - 1, m];

                            for (var n = 0; n < otherRoom.NumVerticesInGrid[facingX, 1]; n++)
                            {
                                var v2 = otherRoom.VerticesGrid[facingX, 1, n];

                                if (v1.Position.Y != (v2.Position.Y + currentRoom.Position.Y * -256.0f -
                                                      otherRoom.Position.Y * -256.0f))
                                    continue;
                                
                                var meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                var meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                var meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

                                v1.FaceColor.X = meanR;
                                v1.FaceColor.Y = meanG;
                                v1.FaceColor.Z = meanB;

                                v2.FaceColor.X = meanR;
                                v2.FaceColor.Y = meanG;
                                v2.FaceColor.Z = meanB;

                                currentRoom.VerticesGrid[x, currentRoom.NumZSectors - 1, m] = v1;
                                otherRoom.VerticesGrid[facingX, 1, n] = v2;

                                break;
                            }
                        }

                        _level.Rooms[currentPortal.Room] = currentRoom;
                        _level.Rooms[otherPortal.Room] = otherRoom;
                    }
                }

                if (currentPortal.Direction == PortalDirection.South)
                {
                    for (int x = currentPortal.X; x <= currentPortal.X + currentPortal.NumXBlocks; x++)
                    {
                        var facingX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);

                        for (var m = 0; m < currentRoom.NumVerticesInGrid[x, 1]; m++)
                        {
                            var v1 = currentRoom.VerticesGrid[x, 1, m];

                            for (var n = 0; n < otherRoom.NumVerticesInGrid[facingX, otherRoom.NumZSectors - 1]; n++)
                            {
                                var v2 = otherRoom.VerticesGrid[facingX, otherRoom.NumZSectors - 1, n];

                                if (v1.Position.Y != (v2.Position.Y + currentRoom.Position.Y * -256.0f -
                                                      otherRoom.Position.Y * -256.0f))
                                    continue;
                                
                                var meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                var meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                var meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

                                v1.FaceColor.X = meanR;
                                v1.FaceColor.Y = meanG;
                                v1.FaceColor.Z = meanB;

                                v2.FaceColor.X = meanR;
                                v2.FaceColor.Y = meanG;
                                v2.FaceColor.Z = meanB;

                                currentRoom.VerticesGrid[x, 1, m] = v1;
                                otherRoom.VerticesGrid[facingX, otherRoom.NumZSectors - 1, n] = v2;

                                break;
                            }
                        }

                        _level.Rooms[currentPortal.Room] = currentRoom;
                        _level.Rooms[otherPortal.Room] = otherRoom;
                    }
                }

                if (currentPortal.Direction == PortalDirection.East)
                {
                    for (int z = currentPortal.Z; z <= currentPortal.Z + currentPortal.NumZBlocks; z++)
                    {
                        var facingZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                        for (var m = 0; m < currentRoom.NumVerticesInGrid[currentRoom.NumXSectors - 1, z]; m++)
                        {
                            var v1 = currentRoom.VerticesGrid[currentRoom.NumXSectors - 1, z, m];

                            for (var n = 0; n < otherRoom.NumVerticesInGrid[1, facingZ]; n++)
                            {
                                var v2 = otherRoom.VerticesGrid[1, facingZ, n];

                                if (v1.Position.Y != (v2.Position.Y + currentRoom.Position.Y * -256.0f -
                                                      otherRoom.Position.Y * -256.0f))
                                    continue;
                                
                                var meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                var meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                var meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

                                v1.FaceColor.X = meanR;
                                v1.FaceColor.Y = meanG;
                                v1.FaceColor.Z = meanB;

                                v2.FaceColor.X = meanR;
                                v2.FaceColor.Y = meanG;
                                v2.FaceColor.Z = meanB;

                                currentRoom.VerticesGrid[currentRoom.NumXSectors - 1, z, m] = v1;
                                otherRoom.VerticesGrid[1, facingZ, n] = v2;

                                break;
                            }
                        }

                        _level.Rooms[currentPortal.Room] = currentRoom;
                        _level.Rooms[otherPortal.Room] = otherRoom;
                    }
                }

                if (currentPortal.Direction == PortalDirection.West)
                {
                    for (int z = currentPortal.Z; z <= currentPortal.Z + currentPortal.NumZBlocks; z++)
                    {
                        var facingZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                        for (var m = 0; m < currentRoom.NumVerticesInGrid[1, z]; m++)
                        {
                            var v1 = currentRoom.VerticesGrid[1, z, m];

                            for (var n = 0; n < otherRoom.NumVerticesInGrid[otherRoom.NumXSectors - 1, facingZ]; n++)
                            {
                                var v2 = otherRoom.VerticesGrid[otherRoom.NumXSectors - 1, facingZ, n];

                                if (v1.Position.Y != (v2.Position.Y + currentRoom.Position.Y * -256.0f -
                                                      otherRoom.Position.Y * -256.0f))
                                    continue;
                                
                                var meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                var meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                var meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

                                v1.FaceColor.X = meanR;
                                v1.FaceColor.Y = meanG;
                                v1.FaceColor.Z = meanB;

                                v2.FaceColor.X = meanR;
                                v2.FaceColor.Y = meanG;
                                v2.FaceColor.Z = meanB;

                                currentRoom.VerticesGrid[1, z, m] = v1;
                                otherRoom.VerticesGrid[otherRoom.NumXSectors - 1, facingZ, n] = v2;

                                break;
                            }
                        }

                        _level.Rooms[currentPortal.Room] = currentRoom;
                        _level.Rooms[otherPortal.Room] = otherRoom;
                    }
                }

                if (currentPortal.Direction == PortalDirection.Floor || currentPortal.Direction == PortalDirection.Ceiling)
                {
                    if (!(currentRoom.FlagWater ^ otherRoom.FlagWater))
                    {
                        for (int z = currentPortal.Z; z <= currentPortal.Z + currentPortal.NumZBlocks; z++)
                        {
                            for (int x = currentPortal.X; x <= currentPortal.X + currentPortal.NumXBlocks; x++)
                            {
                                var facingX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);
                                var facingZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                                for (var m = 0; m < currentRoom.NumVerticesInGrid[x, z]; m++)
                                {
                                    var v1 = currentRoom.VerticesGrid[x, z, m];

                                    for (var n = 0; n < otherRoom.NumVerticesInGrid[facingX, facingZ]; n++)
                                    {
                                        var v2 = otherRoom.VerticesGrid[facingX, facingZ, n];

                                        if (v1.Position.Y !=
                                            (v2.Position.Y + currentRoom.Position.Y * -256.0f -
                                             otherRoom.Position.Y * -256.0f))
                                            continue;
                                        
                                        var meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                        var meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                        var meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

                                        v1.FaceColor.X = meanR;
                                        v1.FaceColor.Y = meanG;
                                        v1.FaceColor.Z = meanB;

                                        v2.FaceColor.X = meanR;
                                        v2.FaceColor.Y = meanG;
                                        v2.FaceColor.Z = meanB;

                                        currentRoom.VerticesGrid[x, z, m] = v1;
                                        otherRoom.VerticesGrid[facingX, facingZ, n] = v2;

                                        break;
                                    }
                                }

                                _level.Rooms[currentPortal.Room] = currentRoom;
                                _level.Rooms[otherPortal.Room] = otherRoom;
                            }
                        }
                    }
                }

                _level.Portals[currentPortal.ID].LightAveraged = true;
                _level.Portals[otherPortal.ID].LightAveraged = true;
            }
        }
        
        private static ushort Pack24BitColorTo16Bit(Vector4 color)
        {
            var r1 = (ushort)color.X;
            var g1 = (ushort)color.Y;
            var b1 = (ushort)color.Z;

            var r = (ushort)Math.Floor(color.X  / 8);
            var g = (ushort)Math.Floor(color.Y  / 8);
            var b = (ushort)Math.Floor(color.Z  / 8);

            if (r1 < 8) r = 0;
            if (g1 < 8) g = 0;
            if (b1 < 8) b = 0;

            ushort tmp = 0;

            if (r1 > 255) r1 = 255;
            if (g1 > 255) g1 = 255;
            if (b1 > 255) b1 = 255;

            if (r1 == 255 && g1 == 255 && b1 == 255)
            {
                tmp = 0x7fff;
            }
            else
            {
                tmp |= 0;
                tmp |= (ushort)(r << 10);
                tmp |= (ushort)(g << 5);
                tmp |= b;
            }

            return tmp;
        }
        
        private static ushort Pack24BitColorTo16Bit(System.Drawing.Color color)
        {
            var r1 = (ushort)color.R;
            var g1 = (ushort)color.G;
            var b1 = (ushort)color.B;

            var r = (ushort)(color.R / 8);
            var g = (ushort)(color.G / 8);
            var b = (ushort)(color.B / 8);

            if (r1 < 8) r = 0;
            if (g1 < 8) g = 0;
            if (b1 < 8) b = 0;

            ushort tmp = 0;

            if (r1 == 255 && g1 == 255 && b1 == 255)
            {
                tmp = 0xffff;
            }
            else
            {
                tmp |= 0;
                tmp |= (ushort)((b << 10) & 0x7c00);
                tmp |= (ushort)((g << 5) & 0x03e0);
                tmp |= (ushort)(r & 0x1f);
            }

            return tmp;
        }
    }
}
