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
            int lastRoom = 0;

            for (int i = 0; i < _level.Rooms.Length - 1; i++)
            {
                Room room = _level.Rooms[i];
                if (room == null) continue;

                _roomsIdTable.Add(i, lastRoom);
                lastRoom++;
            }

            // Average lighting at the portals
            MatchPortalShades();

            Rooms = new tr_room[_roomsIdTable.Count];

            for (int i = 0; i < _level.Rooms.Length - 1; i++)
            {
                Room room = _level.Rooms[i];
                if (room == null) continue;

                tr_room newRoom = new tr_room();

                newRoom.OriginalRoomId = i;

                newRoom.TextureSounds = new TextureSounds[room.NumXSectors, room.NumZSectors];

                newRoom.Lights = new tr4_room_light[0];
                newRoom.StaticMeshes = new tr_room_staticmesh[0];
                newRoom.Portals = new tr_room_portal[0];

                newRoom.Info = new tr_room_info();
                newRoom.Info.X = (int)(room.Position.X * 1024.0f);
                newRoom.Info.Z = (int)(room.Position.Z * 1024.0f);
                newRoom.Info.YTop = (int)(-room.Position.Y * 256.0f - room.GetHighestCorner() * 256.0f);
                newRoom.Info.YBottom = (int)(-room.Position.Y * 256.0f);
                newRoom.NumXSectors = (ushort)(room.NumXSectors);
                newRoom.NumZSectors = (ushort)(room.NumZSectors);
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
                List<int> waterPortals = new List<int>();

                if (!room.FlagWater)
                {
                    short foundWaterRoom = -1;

                    for (int x = 0; x < room.NumXSectors; x++)
                    {
                        for (int z = 0; z < room.NumZSectors; z++)
                        {
                            if (room.Blocks[x, z].FloorPortal != -1)
                            {
                                if (_level.Rooms[_level.Portals[room.Blocks[x, z].FloorPortal].AdjoiningRoom].FlagWater)
                                {
                                    if (!waterPortals.Contains(room.Blocks[x, z].FloorPortal)) waterPortals.Add(room.Blocks[x, z].FloorPortal);
                                }
                            }
                        }
                    }

                    if (waterPortals.Count != 0)
                    {
                        foundWaterRoom = _level.Portals[waterPortals[0]].AdjoiningRoom;

                        Room waterRoom = _level.Rooms[foundWaterRoom];

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

                int lowest = -room.GetLowestCorner() * 256 + newRoom.Info.YBottom;

                // Prepare optimized vertices
                room.OptimizedVertices = new List<EditorVertex>();
                Dictionary<int, int> indicesDictionary = new Dictionary<int, int>();

                for (int x = 0; x < room.NumXSectors; x++)
                {
                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        short base1 = (short)((x << 9) + (z << 4));

                        for (int n = 0; n < room.NumVerticesInGrid[x, z]; n++)
                        {
                            indicesDictionary.Add(base1 + n, room.OptimizedVertices.Count);
                            room.OptimizedVertices.Add(room.VerticesGrid[x, z, n]);
                        }
                    }
                }

                newRoom.Vertices = new tr_room_vertex[room.OptimizedVertices.Count];
                for (int j = 0; j < room.OptimizedVertices.Count; j++)
                {
                    tr_room_vertex rv = new tr_room_vertex();

                    tr_vertex v = new tr_vertex();
                    v.X = (short)room.OptimizedVertices[j].Position.X;
                    v.Y = (short)(-room.OptimizedVertices[j].Position.Y + newRoom.Info.YBottom);
                    v.Z = (short)room.OptimizedVertices[j].Position.Z;

                    if (i==0)
                    {
                        int gggh = 0;
                    }

                    rv.Vertex = v;
                    rv.Lighting1 = 0;
                    rv.Lighting2 = (short)Pack24BitColorTo16bit(room.OptimizedVertices[j].FaceColor);
                    rv.Attributes = 0;

                    // Water special effects
                    if (room.FlagWater)
                    {
                        rv.Attributes = 0x4000;
                    }
                    else
                    {
                        for (int ip = 0; ip < waterPortals.Count; ip++)
                        {
                            Portal portal = _level.Portals[waterPortals[ip]];

                            if (v.X > portal.X * 1024 && v.X < (portal.X + portal.NumXBlocks) * 1024 &&
                                v.Z > portal.Z * 1024 && v.Z < (portal.Z + portal.NumZBlocks) * 1024 &&
                                v.Y == lowest)
                            {
                                int xv = v.X / 1024;
                                int zv = v.Z / 1024;

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
                                if (room.FlagReflection)
                                {
                                    if (v.X >= (portal.X - 1) * 1024 && v.X <= (portal.X + portal.NumXBlocks + 1) * 1024 &&
                                        v.Z >= (portal.Z - 1) * 1024 && v.Z <= (portal.Z + portal.NumZBlocks + 1) * 1024)
                                    {
                                        rv.Attributes = 0x4000;
                                    }
                                }
                            }
                        }
                    }

                    newRoom.Vertices[j] = rv;
                }

                List<tr_face4> tempRectangles = new List<tr_face4>();
                List<tr_face3> tempTriangles = new List<tr_face3>();

                for (int x = 0; x < room.NumXSectors; x++)
                {
                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        for (int f = 0; f < room.Blocks[x, z].Faces.Length; f++)
                        {
                            BlockFace face = room.Blocks[x, z].Faces[f];
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
                                tr_face4 rectangle = new tr_face4();

                                List<ushort> indices = new List<ushort>();

                                indices.Add((ushort)indicesDictionary[face.Indices[0]]);
                                indices.Add((ushort)indicesDictionary[face.Indices[1]]);
                                indices.Add((ushort)indicesDictionary[face.Indices[2]]);
                                indices.Add((ushort)indicesDictionary[face.Indices[3]]);

                                byte rot = face.Rotation;
                                if (rot != 0)
                                {
                                    for (int n = 0; n < rot; n++)
                                    {
                                        ushort tmp = indices[0];
                                        indices.RemoveAt(0);
                                        indices.Insert(3, tmp);
                                    }
                                }

                                rectangle.Vertices = new ushort[4];
                                rectangle.Vertices[0] = indices[0];
                                rectangle.Vertices[1] = indices[1];
                                rectangle.Vertices[2] = indices[2];
                                rectangle.Vertices[3] = indices[3];

                                rectangle.Texture = (short)face.NewTexture;

                                tempRectangles.Add(rectangle);
                            }
                            else
                            {
                                tr_face3 triangle = new tr_face3();

                                List<ushort> indices = new List<ushort>();

                                indices.Add((ushort)indicesDictionary[face.Indices[0]]);
                                indices.Add((ushort)indicesDictionary[face.Indices[1]]);
                                indices.Add((ushort)indicesDictionary[face.Indices[2]]);

                                byte rot = face.Rotation;
                                if (rot != 0)
                                {
                                    for (int n = 0; n < rot; n++)
                                    {
                                        ushort tmp = indices[0];
                                        indices.RemoveAt(0);
                                        indices.Insert(2, tmp);
                                    }
                                }

                                triangle.Vertices = new ushort[4];
                                triangle.Vertices[0] = indices[0];
                                triangle.Vertices[1] = indices[1];
                                triangle.Vertices[2] = indices[2];

                                triangle.Texture = (short)face.NewTexture;

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
                List<tr_room_portal> tempPortals = new List<tr_room_portal>();
                List<int> tempIdPortals = new List<int>();
                Dictionary<int, int> portalToRooms = new Dictionary<int, int>();

                for (int z = 0; z < room.NumZSectors; z++)
                {
                    for (int x = 0; x < room.NumXSectors; x++)
                    {
                        if (room.Blocks[x, z].WallPortal >= 0 && !tempIdPortals.Contains(room.Blocks[x, z].WallPortal))
                            tempIdPortals.Add(room.Blocks[x, z].WallPortal);

                        if (room.Blocks[x, z].FloorPortal >= 0 && !tempIdPortals.Contains(room.Blocks[x, z].FloorPortal))
                            tempIdPortals.Add(room.Blocks[x, z].FloorPortal);

                        if (room.Blocks[x, z].CeilingPortal >= 0 && !tempIdPortals.Contains(room.Blocks[x, z].CeilingPortal))
                            tempIdPortals.Add(room.Blocks[x, z].CeilingPortal);
                    }
                }

                if (i==110)
                {
                    int jjj = 0;
                }

                for (int j = 0; j < tempIdPortals.Count; j++)
                {
                    Portal portal = _editor.Level.Portals[tempIdPortals[j]];
                    tr_room_portal newPortal = new tr_room_portal();

                    Block startBlock;
                    Block endBlock;

                    int xMin = 0;
                    int xMax = 0;
                    int yMin1 = 0;
                    int yMax1 = 0;
                    int yMin2 = 0;
                    int yMax2 = 0;
                    int zMin = 0;
                    int zMax = 0;

                    newPortal.AdjoiningRoom = (ushort)_roomsIdTable[(int)portal.AdjoiningRoom];

                    newPortal.Vertices = new tr_vertex[4];

                    // Normal and vertices
                    if (portal.Direction == PortalDirection.North)
                    {
                        newPortal.Normal = new tr_vertex();
                        newPortal.Normal.X = 0;
                        newPortal.Normal.Y = 0;
                        newPortal.Normal.Z = -1;

                        startBlock = room.Blocks[portal.X, room.NumZSectors - 2];
                        endBlock = room.Blocks[portal.X + portal.NumXBlocks - 1, room.NumZSectors - 2];

                        xMin = portal.X;
                        xMax = portal.X + portal.NumXBlocks;
                        yMin1 = startBlock.QAFaces[0];
                        yMax1 = startBlock.WSFaces[0] + room.Ceiling;
                        yMin2 = endBlock.QAFaces[1];
                        yMax2 = endBlock.WSFaces[1] + room.Ceiling;
                        zMin = room.NumZSectors - 1;
                        zMax = room.NumZSectors - 1;

                        int yMin = 32768;
                        int yMax = -32768;

                        int y1;
                        int y2;

                        for (int x = xMin; x < xMax; x++)
                        {
                            Block currentBlock = room.Blocks[x, room.NumZSectors - 2];
                            Block facingBlock = room.Blocks[x, room.NumZSectors - 1];

                            y1 = Math.Max(facingBlock.QAFaces[3], currentBlock.QAFaces[0]);
                            y2 = Math.Min(facingBlock.WSFaces[3], currentBlock.WSFaces[0]);

                            if (y1 < yMin) yMin = y1;
                            if (y2 > yMax) yMax = y2;
                        }

                        Block lastBlock = room.Blocks[xMax - 1, room.NumZSectors - 2];
                        Block lastFacingBlock = room.Blocks[xMax - 1, room.NumZSectors - 1];

                        y1 = Math.Max(lastFacingBlock.QAFaces[2], lastBlock.QAFaces[1]);
                        y2 = Math.Min(lastFacingBlock.WSFaces[2], lastBlock.WSFaces[1]);

                        if (y1 < yMin) yMin = y1;
                        if (y2 > yMax) yMax = y2;

                        yMax += room.Ceiling;

                        newPortal.Vertices[0] = new tr_vertex();
                        newPortal.Vertices[0].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[0].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[0].Z = (short)(zMin * 1024.0f);

                        newPortal.Vertices[1] = new tr_vertex();
                        newPortal.Vertices[1].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[1].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[1].Z = (short)(zMin * 1024.0f);

                        newPortal.Vertices[2] = new tr_vertex();
                        newPortal.Vertices[2].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[2].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[2].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[3] = new tr_vertex();
                        newPortal.Vertices[3].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[3].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[3].Z = (short)(zMax * 1024.0f);
                    }
                    else if (portal.Direction == PortalDirection.East)
                    {
                        newPortal.Normal = new tr_vertex();
                        newPortal.Normal.X = -1;
                        newPortal.Normal.Y = 0;
                        newPortal.Normal.Z = 0;

                        startBlock = room.Blocks[room.NumXSectors - 2, portal.Z + portal.NumZBlocks - 1];
                        endBlock = room.Blocks[room.NumXSectors - 2, portal.Z];

                        xMin = room.NumXSectors - 1;
                        xMax = room.NumXSectors - 1;
                        yMin1 = startBlock.QAFaces[1];
                        yMax1 = startBlock.WSFaces[1] + room.Ceiling;
                        yMin2 = endBlock.QAFaces[2];
                        yMax2 = endBlock.WSFaces[2] + room.Ceiling;
                        zMin = portal.Z + portal.NumZBlocks;
                        zMax = portal.Z;

                        int yMin = 32768;
                        int yMax = -32768;

                        int y1;
                        int y2;

                        for (int z = zMax; z < zMin; z++)
                        {
                            Block currentBlock = room.Blocks[room.NumXSectors - 2, z];
                            Block facingBlock = room.Blocks[room.NumXSectors - 1, z];

                            y1 = Math.Max(facingBlock.QAFaces[0], currentBlock.QAFaces[1]);
                            y2 = Math.Min(facingBlock.WSFaces[0], currentBlock.WSFaces[1]);

                            if (y1 < yMin) yMin = y1;
                            if (y2 > yMax) yMax = y2;
                        }

                        Block lastBlock = room.Blocks[room.NumXSectors - 2, zMin - 1];
                        Block lastFacingBlock = room.Blocks[room.NumXSectors - 1, zMin - 1];

                        y1 = Math.Max(lastFacingBlock.QAFaces[3], lastBlock.QAFaces[2]);
                        y2 = Math.Min(lastFacingBlock.WSFaces[3], lastBlock.WSFaces[2]);

                        if (y1 < yMin) yMin = y1;
                        if (y2 > yMax) yMax = y2;

                        yMax += room.Ceiling;

                        newPortal.Vertices[1] = new tr_vertex();
                        newPortal.Vertices[1].X = (short)(xMin * 1024);
                        newPortal.Vertices[1].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[1].Z = (short)(zMin * 1024.0f);

                        newPortal.Vertices[2] = new tr_vertex();
                        newPortal.Vertices[2].X = (short)(xMax * 1024);
                        newPortal.Vertices[2].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[2].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[3] = new tr_vertex();
                        newPortal.Vertices[3].X = (short)(xMax * 1024);
                        newPortal.Vertices[3].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[3].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[0] = new tr_vertex();
                        newPortal.Vertices[0].X = (short)(xMin * 1024);
                        newPortal.Vertices[0].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[0].Z = (short)(zMin * 1024.0f);
                    }
                    else if (portal.Direction == PortalDirection.South)
                    {
                        newPortal.Normal = new tr_vertex();
                        newPortal.Normal.X = 0;
                        newPortal.Normal.Y = 0;
                        newPortal.Normal.Z = 1;

                        startBlock = room.Blocks[portal.X + portal.NumXBlocks - 1, 1];
                        endBlock = room.Blocks[portal.X, 1];

                        xMin = portal.X + portal.NumXBlocks;
                        xMax = portal.X;
                        yMin1 = startBlock.QAFaces[2];
                        yMax1 = startBlock.WSFaces[2] + room.Ceiling;
                        yMin2 = endBlock.QAFaces[3];
                        yMax2 = endBlock.WSFaces[3] + room.Ceiling;
                        zMin = 1;
                        zMax = 1;

                        int yMin = 32768;
                        int yMax = -32768;

                        int y1;
                        int y2;

                        for (int x = xMax; x < xMin; x++)
                        {
                            Block currentBlock = room.Blocks[x, 1];
                            Block facingBlock = room.Blocks[x, 0];

                            y1 = Math.Max(facingBlock.QAFaces[1], currentBlock.QAFaces[2]);
                            y2 = Math.Min(facingBlock.WSFaces[1], currentBlock.WSFaces[2]);

                            if (y1 < yMin) yMin = y1;
                            if (y2 > yMax) yMax = y2;
                        }

                        Block lastBlock = room.Blocks[xMin - 1, 1];
                        Block lastFacingBlock = room.Blocks[xMin - 1, 0];

                        y1 = Math.Max(lastFacingBlock.QAFaces[0], lastBlock.QAFaces[3]);
                        y2 = Math.Min(lastFacingBlock.WSFaces[0], lastBlock.WSFaces[3]);

                        if (y1 < yMin) yMin = y1;
                        if (y2 > yMax) yMax = y2;

                        yMax += room.Ceiling;

                        newPortal.Vertices[0] = new tr_vertex();
                        newPortal.Vertices[0].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[0].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[0].Z = (short)(zMin * 1024.0f - 1.0f);

                        newPortal.Vertices[1] = new tr_vertex();
                        newPortal.Vertices[1].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[1].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[1].Z = (short)(zMin * 1024.0f - 1.0f);

                        newPortal.Vertices[2] = new tr_vertex();
                        newPortal.Vertices[2].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[2].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[2].Z = (short)(zMax * 1024.0f - 1.0f);

                        newPortal.Vertices[3] = new tr_vertex();
                        newPortal.Vertices[3].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[3].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[3].Z = (short)(zMax * 1024.0f - 1.0f);
                    }
                    else if (portal.Direction == PortalDirection.West)
                    {
                        newPortal.Normal = new tr_vertex();
                        newPortal.Normal.X = 1;
                        newPortal.Normal.Y = 0;
                        newPortal.Normal.Z = 0;

                        startBlock = room.Blocks[1, portal.Z];
                        endBlock = room.Blocks[1, portal.Z + portal.NumZBlocks - 1];

                        xMin = 1;
                        xMax = 1;
                        yMin1 = startBlock.QAFaces[0];
                        yMax1 = startBlock.WSFaces[0] + room.Ceiling;
                        yMin2 = endBlock.QAFaces[3];
                        yMax2 = endBlock.WSFaces[3] + room.Ceiling;
                        zMin = portal.Z;
                        zMax = portal.Z + portal.NumZBlocks;

                        int yMin = 32768;
                        int yMax = -32768;

                        int y1;
                        int y2;

                        for (int z = zMin; z < zMax; z++)
                        {
                            Block currentBlock = room.Blocks[1, z];
                            Block facingBlock = room.Blocks[0, z];

                            y1 = Math.Max(facingBlock.QAFaces[2], currentBlock.QAFaces[3]);
                            y2 = Math.Min(facingBlock.WSFaces[2], currentBlock.WSFaces[3]);

                            if (y1 < yMin) yMin = y1;
                            if (y2 > yMax) yMax = y2;
                        }

                        Block lastBlock = room.Blocks[1, zMax - 1];
                        Block lastFacingBlock = room.Blocks[0, zMax - 1];

                        y1 = Math.Max(lastFacingBlock.QAFaces[1], lastBlock.QAFaces[0]);
                        y2 = Math.Min(lastFacingBlock.WSFaces[1], lastBlock.WSFaces[0]);

                        if (y1 < yMin) yMin = y1;
                        if (y2 > yMax) yMax = y2;

                        yMax += room.Ceiling;

                        newPortal.Vertices[1] = new tr_vertex();
                        newPortal.Vertices[1].X = (short)(xMin * 1024.0f - 1.0f);
                        newPortal.Vertices[1].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[1].Z = (short)(zMin * 1024.0f);

                        newPortal.Vertices[2] = new tr_vertex();
                        newPortal.Vertices[2].X = (short)(xMax * 1024.0f - 1.0f);
                        newPortal.Vertices[2].Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[2].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[3] = new tr_vertex();
                        newPortal.Vertices[3].X = (short)(xMax * 1024.0f - 1.0f);
                        newPortal.Vertices[3].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[3].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[0] = new tr_vertex();
                        newPortal.Vertices[0].X = (short)(xMin * 1024.0f - 1.0f);
                        newPortal.Vertices[0].Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[0].Z = (short)(zMin * 1024.0f);
                    }
                    else if (portal.Direction == PortalDirection.Floor)
                    {
                        newPortal.Normal = new tr_vertex();
                        newPortal.Normal.X = 0;
                        newPortal.Normal.Y = -1;
                        newPortal.Normal.Z = 0;

                        startBlock = room.Blocks[portal.X, portal.Z];
                        endBlock = room.Blocks[portal.X, portal.Z + portal.NumZBlocks - 1];

                        xMin = portal.X;
                        xMax = portal.X + portal.NumXBlocks;
                        int y = room.GetLowestCorner();
                        zMin = portal.Z;
                        zMax = portal.Z + portal.NumZBlocks;

                        newPortal.Vertices[1] = new tr_vertex();
                        newPortal.Vertices[1].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[1].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[1].Z = (short)(zMin * 1024.0f);

                        newPortal.Vertices[2] = new tr_vertex();
                        newPortal.Vertices[2].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[2].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[2].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[3] = new tr_vertex();
                        newPortal.Vertices[3].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[3].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[3].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[0] = new tr_vertex();
                        newPortal.Vertices[0].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[0].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[0].Z = (short)(zMin * 1024.0f);
                    }
                    else if (portal.Direction == PortalDirection.Ceiling)
                    {
                        newPortal.Normal = new tr_vertex();
                        newPortal.Normal.X = 0;
                        newPortal.Normal.Y = 1;
                        newPortal.Normal.Z = 0;

                        startBlock = room.Blocks[portal.X, portal.Z];
                        endBlock = room.Blocks[portal.X, portal.Z + portal.NumZBlocks - 1];

                        xMin = portal.X;
                        xMax = portal.X + portal.NumXBlocks;
                        int y = room.GetHighestCorner();
                        zMin = portal.Z + portal.NumZBlocks;
                        zMax = portal.Z;

                        newPortal.Vertices[1] = new tr_vertex();
                        newPortal.Vertices[1].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[1].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[1].Z = (short)(zMin * 1024.0f);

                        newPortal.Vertices[2] = new tr_vertex();
                        newPortal.Vertices[2].X = (short)(xMin * 1024.0f);
                        newPortal.Vertices[2].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[2].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[3] = new tr_vertex();
                        newPortal.Vertices[3].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[3].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[3].Z = (short)(zMax * 1024.0f);

                        newPortal.Vertices[0] = new tr_vertex();
                        newPortal.Vertices[0].X = (short)(xMax * 1024.0f);
                        newPortal.Vertices[0].Y = (short)(-y * 256.0f + newRoom.Info.YBottom);
                        newPortal.Vertices[0].Z = (short)(zMin * 1024.0f);
                    }

                    tempPortals.Add(newPortal);
                }

                newRoom.NumPortals = (ushort)tempPortals.Count;
                newRoom.Portals = tempPortals.ToArray();

                newRoom.Sectors = new tr_room_sector[room.NumXSectors * room.NumZSectors];
                newRoom.AuxSectors = new tr_sector_aux[room.NumXSectors, room.NumZSectors];

                for (int z = 0; z < room.NumZSectors; z++)
                {
                    for (int x = 0; x < room.NumXSectors; x++)
                    {
                        tr_room_sector sector = new tr_room_sector();
                        tr_sector_aux aux = new tr_sector_aux();

                        byte sound = (byte)newRoom.TextureSounds[x, z];

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
                            aux.WallPortal = _roomsIdTable[(int)_editor.Level.Portals[room.Blocks[x, z].WallPortal].AdjoiningRoom];
                        else
                            aux.WallPortal = -1;

                        aux.LowestFloor = (sbyte)(-room.Position.Y - room.GetLowestFloorCorner(x, z));
                        short q0 = room.Blocks[x, z].QAFaces[0];
                        short q1 = room.Blocks[x, z].QAFaces[1];
                        short q2 = room.Blocks[x, z].QAFaces[2];
                        short q3 = room.Blocks[x, z].QAFaces[3];

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

                List<tr_room_staticmesh> tempStaticMeshes = new List<tr_room_staticmesh>();

                for (int j = 0; j < room.StaticMeshes.Count; j++)
                {
                    tr_room_staticmesh sm = new tr_room_staticmesh();
                    StaticMeshInstance instance = (StaticMeshInstance)_editor.Level.Objects[room.StaticMeshes[j]];

                    sm.X = (uint)(newRoom.Info.X + instance.Position.X);
                    sm.Y = (uint)(newRoom.Info.YBottom - instance.Position.Y);
                    sm.Z = (uint)(newRoom.Info.Z + instance.Position.Z);

                    sm.Rotation = (ushort)(instance.Rotation / 45 * 8192);
                    sm.ObjectID = (ushort)instance.Model.ObjectID;
                    sm.Intensity1 = Pack24BitColorTo16bit(instance.Color);
                    sm.Intensity2 = 0;

                    tempStaticMeshes.Add(sm);
                }

                newRoom.NumStaticMeshes = (ushort)tempStaticMeshes.Count;
                newRoom.StaticMeshes = tempStaticMeshes.ToArray();

                List<tr4_room_light> tempLights = new List<tr4_room_light>();
                for (int j = 0; j < room.Lights.Count; j++)
                {
                    tr4_room_light newLight = new tr4_room_light();
                    Light light = room.Lights[j];

                    if (light.Type == LightType.Effect) continue;

                    newLight.X = (int)(newRoom.Info.X + light.Position.X);
                    newLight.Y = (int)(-light.Position.Y + newRoom.Info.YBottom);
                    newLight.Z = (int)(newRoom.Info.Z + light.Position.Z);

                    newLight.Color = new tr_color();
                    newLight.Color.Red = light.Color.R;
                    newLight.Color.Green = light.Color.G;
                    newLight.Color.Blue = light.Color.B;

                    newLight.Intensity = (ushort)(((short)(Math.Abs(light.Intensity) * 31.0f) << 8) | 0x00ff);

                    if (light.Type == LightType.Light)
                    {
                        // Point light
                        newLight.LightType = 1;
                        newLight.In = light.In * 1024;
                        newLight.Out = light.Out * 1024;
                    }
                    else if (light.Type == LightType.Shadow)
                    {
                        // Point shadow
                        newLight.LightType = 3;
                        newLight.In = light.In * 1024;
                        newLight.Out = light.Out * 1024;
                    }
                    else if (light.Type == LightType.Spot)
                    {
                        // Spot light
                        newLight.LightType = 2;
                        newLight.In = (float)Math.Cos(MathUtil.DegreesToRadians(light.In));
                        newLight.Out = (float)Math.Cos(MathUtil.DegreesToRadians(light.Out));
                        newLight.Length = light.Len * 1024.0f;
                        newLight.CutOff = light.Cutoff * 1024.0f;
                        newLight.DirectionX = (float)(-Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Sin(MathUtil.DegreesToRadians(light.DirectionY)));
                        newLight.DirectionY = (float)(Math.Sin(MathUtil.DegreesToRadians(light.DirectionX)));
                        newLight.DirectionZ = (float)(-Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Cos(MathUtil.DegreesToRadians(light.DirectionY)));
                    }
                    else if (light.Type == LightType.Sun)
                    {
                        // Sun light
                        newLight.LightType = 0;
                        newLight.In = 0;
                        newLight.Out = 0;
                        newLight.Length = 0;
                        newLight.CutOff = 0;
                        newLight.DirectionX = -(float)(Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Sin(MathUtil.DegreesToRadians(light.DirectionY)));
                        newLight.DirectionY = -(float)(-Math.Sin(MathUtil.DegreesToRadians(light.DirectionX)));
                        newLight.DirectionZ = -(float)(Math.Cos(MathUtil.DegreesToRadians(light.DirectionX)) * Math.Cos(MathUtil.DegreesToRadians(light.DirectionY)));
                    }
                    else if (light.Type == LightType.FogBulb)
                    {
                        // Fog bulb
                        newLight.LightType = 4;
                        newLight.In = light.In * 1024;
                        newLight.Out = light.Out * 1024;
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
            int i, k;
            _animTexturesRooms = new List<int>();
            _animTexturesGeneral = new List<int>();

            ReportProgress(1, "Preparing room textures");

            // Reset animated textures
            for (i = 0; i < _level.AnimatedTextures.Count; i++)
            {
                _level.AnimatedTextures[i].Variants = new List<AnimatedTextureSequenceVariant>();

                for (k = 0; k < _level.AnimatedTextures[i].Textures.Count; k++)
                {
                    _level.AnimatedTextures[i].Textures[k].NewID = -1;
                }
            }

            _tempAnimatedTextures = new List<AnimatedTextureSequenceVariant>();

            _tempObjectTextures = new List<tr_object_texture>();

            // I start with a 128 pages texture map (32 MB in memory)
            byte[,] _roomTextureMap = new byte[1024, 32768];

            // First, I have to filter only used textures and sort them (for now I use bubble sort, in the future a tree)
            List<LevelTexture> _tempTexturesList = new List<LevelTexture>();
            _texturesIdTable = new Dictionary<int, int>();

            for (i = 0; i < _editor.Level.TextureSamples.Count; i++)
            {
                LevelTexture oldSample = _editor.Level.TextureSamples.ElementAt(i).Value;

                // don't count for unused textures
                // if (oldSample.UsageCount <= 0) continue;

                oldSample.OldID = oldSample.ID;

                _tempTexturesList.Add(oldSample);
            }

            ReportProgress(2, "Sorting room textures");

            for (i = 0; i < _level.AnimatedTextures.Count; i++)
            {
                for (k = 0; k < _level.AnimatedTextures[i].Textures.Count; k++)
                {
                    LevelTexture newText = new LevelTexture();

                    newText.X = _level.AnimatedTextures[i].Textures[k].X;
                    newText.Y = _level.AnimatedTextures[i].Textures[k].Y;
                    newText.Width = 64;
                    newText.Height = 64;
                    newText.Page = _level.AnimatedTextures[i].Textures[k].Page;
                    newText.Animated = true;
                    newText.AnimatedSequence = i;
                    newText.AnimatedTexture = k;

                    _tempTexturesList.Add(newText);
                }
            }

            _tempTexturesArray = _tempTexturesList.ToArray();

            ReportProgress(3, "Building room texture map");

            // I've sorted the textures by height, now I build the texture map
            int numRoomTexturePages = 1;
            int x;
            int y;
            int z;
            int j;

            numRoomTexturePages = _editor.Level.TextureMap.Height / 256;
            for (x = 0; x < 256; x++)
            {
                for (y = 0; y < _editor.Level.TextureMap.Height; y++)
                {
                    System.Drawing.Color c = _editor.Level.TextureMap.GetPixel(x, y);

                    if (c.R == 255 & c.G == 0 && c.B == 255)
                    {
                        _roomTextureMap[x * 4 + 0, y] = 0;
                        _roomTextureMap[x * 4 + 1, y] = 0;
                        _roomTextureMap[x * 4 + 2, y] = 0;
                        _roomTextureMap[x * 4 + 3, y] = 0;
                    }
                    else
                    {
                        _roomTextureMap[x * 4 + 0, y] = c.B;
                        _roomTextureMap[x * 4 + 1, y] = c.G;
                        _roomTextureMap[x * 4 + 2, y] = c.R;
                        _roomTextureMap[x * 4 + 3, y] = 255;
                    }
                }
            }

            // Rebuild the ID table
            for (i = 0; i < _tempTexturesArray.Length; i++)
            {
                LevelTexture tex = _tempTexturesArray[i];
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
            List<tr_object_texture> _textureTiles = new List<tr_object_texture>();

            for (i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] == null) continue;

                Room room = _editor.Level.Rooms[i];

                for (x = 0; x < room.NumXSectors; x++)
                {
                    for (z = 0; z < room.NumZSectors; z++)
                    {
                        for (int f = 0; f < room.Blocks[x, z].Faces.Length; f++)
                        {
                            BlockFace face = room.Blocks[x, z].Faces[f];

                            // Ignore undefined faces and untextured faces
                            if (!face.Defined || face.Texture == -1) continue;

                            // Build (or get) the texture info
                            face.NewTexture = BuildTextureInfo(face);
                        }
                    }
                }
            }

            _roomTexturePages = new byte[numRoomTexturePages * 256 * 256 * 4];
            for (y = 0; y < 256 * numRoomTexturePages; y++)
            {
                for (x = 0; x < 1024; x++)
                {
                    _roomTexturePages[y * 1024 + x] = _roomTextureMap[x, y];
                }
            }

            _numRoomTexturePages = numRoomTexturePages;

            ReportProgress(5, "    Room texture pages: " + _numRoomTexturePages);

            ReportProgress(6, "Building animated textures table");

            // Prepare animated textures

            // Build remaining tiles
            for (i = 0; i < _level.AnimatedTextures.Count; i++)
            {
                for (j = 0; j < _level.AnimatedTextures[i].Variants.Count; j++)
                {
                    for (k = 0; k < _level.AnimatedTextures[i].Variants[j].Tiles.Count; k++)
                    {
                        if (_level.AnimatedTextures[i].Variants[j].Tiles[k].NewID == -1)
                        {
                            _level.AnimatedTextures[i].Variants[j].Tiles[k].NewID = BuildAnimatedTextureInfo(_level.AnimatedTextures[i].Variants[j],
                                                                                                             _level.AnimatedTextures[i].Variants[j].Tiles[k],
                                                                                                             _level.AnimatedTextures[i].Textures[k].Texture);
                        }
                    }
                }
            }

            NumAnimatedTextures = 0;
            List<tr_animated_textures_set> tempAnimatedTextures = new List<tr_animated_textures_set>();
            for (i = 0; i < _level.AnimatedTextures.Count; i++)
            {
                for (j = 0; j < _level.AnimatedTextures[i].Variants.Count; j++)
                {
                    tr_animated_textures_set newSet = new tr_animated_textures_set();

                    List<short> tempTextureIds = new List<short>();
                    for (k = 0; k < _level.AnimatedTextures[i].Variants[j].Tiles.Count; k++)
                    {
                        tempTextureIds.Add((short)_level.AnimatedTextures[i].Variants[j].Tiles[k].NewID);
                        if (!_animTexturesGeneral.Contains(_level.AnimatedTextures[i].Variants[j].Tiles[k].NewID))
                            _animTexturesGeneral.Add(_level.AnimatedTextures[i].Variants[j].Tiles[k].NewID);

                        NumAnimatedTextures++;
                    }

                    if (tempTextureIds.Count > 0)
                    {
                        newSet.Textures = tempTextureIds.ToArray();
                        newSet.NumTextures = (short)(newSet.Textures.Length - 1);
                        tempAnimatedTextures.Add(newSet);
                        NumAnimatedTextures++;
                    }
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

            for (int i = 0; i < _level.Portals.Count; i++)
            {
                _level.Portals.ElementAt(i).Value.LightAveraged = false;
            }

            for (int i = 0; i < _level.Portals.Count; i++)
            {
                // Get current portal and its paired portal
                Portal currentPortal = _level.Portals.ElementAt(i).Value;
                Portal otherPortal = _level.Portals[currentPortal.OtherID];

                // If the light was already averaged, then continue loop
                //if (currentPortal.LightAveraged) continue;

                // Get the rooms
                Room currentRoom = _level.Rooms[currentPortal.Room];
                Room otherRoom = _level.Rooms[otherPortal.Room];

                if (currentPortal.Direction == PortalDirection.North)
                {
                    for (int x = currentPortal.X; x <= currentPortal.X + currentPortal.NumXBlocks; x++)
                    {
                        int facingX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);

                        for (int m = 0; m < currentRoom.NumVerticesInGrid[x, currentRoom.NumZSectors - 1]; m++)
                        {
                            EditorVertex v1 = currentRoom.VerticesGrid[x, currentRoom.NumZSectors - 1, m];

                            for (int n = 0; n < otherRoom.NumVerticesInGrid[facingX, 1]; n++)
                            {
                                EditorVertex v2 = otherRoom.VerticesGrid[facingX, 1, n];

                                if (v1.Position.Y == (v2.Position.Y + currentRoom.Position.Y * -256.0f - otherRoom.Position.Y * -256.0f))
                                {
                                    int meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                    int meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                    int meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

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
                        }

                        _level.Rooms[currentPortal.Room] = currentRoom;
                        _level.Rooms[otherPortal.Room] = otherRoom;
                    }
                }

                if (currentPortal.Direction == PortalDirection.South)
                {
                    for (int x = currentPortal.X; x <= currentPortal.X + currentPortal.NumXBlocks; x++)
                    {
                        int facingX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);

                        for (int m = 0; m < currentRoom.NumVerticesInGrid[x, 1]; m++)
                        {
                            EditorVertex v1 = currentRoom.VerticesGrid[x, 1, m];

                            for (int n = 0; n < otherRoom.NumVerticesInGrid[facingX, otherRoom.NumZSectors - 1]; n++)
                            {
                                EditorVertex v2 = otherRoom.VerticesGrid[facingX, otherRoom.NumZSectors - 1, n];

                                if (v1.Position.Y == (v2.Position.Y + currentRoom.Position.Y * -256.0f - otherRoom.Position.Y * -256.0f))
                                {
                                    int meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                    int meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                    int meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

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
                        }

                        _level.Rooms[currentPortal.Room] = currentRoom;
                        _level.Rooms[otherPortal.Room] = otherRoom;
                    }
                }

                if (currentPortal.Direction == PortalDirection.East)
                {
                    for (int z = currentPortal.Z; z <= currentPortal.Z + currentPortal.NumZBlocks; z++)
                    {
                        int facingZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                        for (int m = 0; m < currentRoom.NumVerticesInGrid[currentRoom.NumXSectors - 1, z]; m++)
                        {
                            EditorVertex v1 = currentRoom.VerticesGrid[currentRoom.NumXSectors - 1, z, m];

                            for (int n = 0; n < otherRoom.NumVerticesInGrid[1, facingZ]; n++)
                            {
                                EditorVertex v2 = otherRoom.VerticesGrid[1, facingZ, n];

                                if (v1.Position.Y == (v2.Position.Y + currentRoom.Position.Y * -256.0f - otherRoom.Position.Y * -256.0f))
                                {
                                    int meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                    int meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                    int meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

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
                        }

                        _level.Rooms[currentPortal.Room] = currentRoom;
                        _level.Rooms[otherPortal.Room] = otherRoom;
                    }
                }

                if (currentPortal.Direction == PortalDirection.West)
                {
                    for (int z = currentPortal.Z; z <= currentPortal.Z + currentPortal.NumZBlocks; z++)
                    {
                        int facingZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                        for (int m = 0; m < currentRoom.NumVerticesInGrid[1, z]; m++)
                        {
                            EditorVertex v1 = currentRoom.VerticesGrid[1, z, m];

                            for (int n = 0; n < otherRoom.NumVerticesInGrid[otherRoom.NumXSectors - 1, facingZ]; n++)
                            {
                                EditorVertex v2 = otherRoom.VerticesGrid[otherRoom.NumXSectors - 1, facingZ, n];

                                if (v1.Position.Y == (v2.Position.Y + currentRoom.Position.Y * -256.0f - otherRoom.Position.Y * -256.0f))
                                {
                                    int meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                    int meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                    int meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

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
                                int facingX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);
                                int facingZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                                for (int m = 0; m < currentRoom.NumVerticesInGrid[x, z]; m++)
                                {
                                    EditorVertex v1 = currentRoom.VerticesGrid[x, z, m];

                                    for (int n = 0; n < otherRoom.NumVerticesInGrid[facingX, facingZ]; n++)
                                    {
                                        EditorVertex v2 = otherRoom.VerticesGrid[facingX, facingZ, n];

                                        if (v1.Position.Y == (v2.Position.Y + currentRoom.Position.Y * -256.0f - otherRoom.Position.Y * -256.0f))
                                        {
                                            int meanR = (int)(v1.FaceColor.X + v2.FaceColor.X) >> 1;
                                            int meanG = (int)(v1.FaceColor.Y + v2.FaceColor.Y) >> 1;
                                            int meanB = (int)(v1.FaceColor.Z + v2.FaceColor.Z) >> 1;

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
        
        private static ushort Pack24BitColorTo16bit(Vector4 color)
        {
            ushort r1 = (ushort)(color.X );
            ushort g1 = (ushort)(color.Y );
            ushort b1 = (ushort)(color.Z );

            ushort r = (ushort)Math.Floor(color.X  / 8);
            ushort g = (ushort)Math.Floor(color.Y  / 8);
            ushort b = (ushort)Math.Floor(color.Z  / 8);

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

            if (tmp > 0x7fff)
            {
                int hhgjk = 0;
            }

            return tmp;
        }
        
        private static ushort Pack24BitColorTo16bit(System.Drawing.Color color)
        {
            ushort r1 = (ushort)(color.R);
            ushort g1 = (ushort)(color.G);
            ushort b1 = (ushort)(color.B);

            ushort r = (ushort)(color.R / 8);
            ushort g = (ushort)(color.G / 8);
            ushort b = (ushort)(color.B / 8);

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
