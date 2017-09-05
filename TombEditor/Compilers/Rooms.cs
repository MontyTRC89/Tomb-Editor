using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using TombEditor.Geometry;
using System.Drawing;
using System.Drawing.Imaging;
using TombLib.Utils;
using System.Threading.Tasks;

namespace TombEditor.Compilers
{
    public sealed partial class LevelCompilerTr4
    {
        private readonly Dictionary<Room, int> _roomsRemappingDictionary = new Dictionary<Room, int>();
        private readonly List<Room> _roomsUnmapping = new List<Room>();

        private void BuildRooms()
        {
            ReportProgress(20, "Building rooms");
            
            foreach (var room in _level.Rooms.Where(r => r != null))
            {
                _roomsRemappingDictionary.Add(room, _roomsUnmapping.Count);
                _roomsUnmapping.Add(room);
            }

            // TODO Enable parallization
            //Parallel.ForEach(_roomsRemappingDictionary.Keys, (Room room) =>
            //{
            //    tr_room trRoom = BuildRoom(room);
            //    lock (_tempRooms)
            //        _tempRooms.Add(room, trRoom);
            //});

            foreach (var room in _roomsRemappingDictionary.Keys)
            {
                _tempRooms.Add(room, BuildRoom(room));
            }



            ReportProgress(25, "    Number of rooms: " + _roomsUnmapping.Count);

            MatchPortalVertexColors();

            ReportProgress(28, "    Vertex colors on portals matched.");
        }

        private tr_room BuildRoom(Room room)
        {
            tr_color roomAmbientColor = PackColorTo24Bit(room.AmbientLight);

            var newRoom = new tr_room
            {
                OriginalRoom = room,
                Lights = new tr4_room_light[0],
                StaticMeshes = new tr_room_staticmesh[0],
                Portals = new tr_room_portal[0],
                Info = new tr_room_info
                {
                    X = (int)(room.Position.X * 1024.0f),
                    Z = (int)(room.Position.Z * 1024.0f),
                    YTop = (int)(-room.Position.Y * 256.0f - room.GetHighestCorner() * 256.0f),
                    YBottom = (int)(-room.Position.Y * 256.0f)
                },
                NumXSectors = room.NumXSectors,
                NumZSectors = room.NumZSectors,
                AlternateRoom = (room.Flipped && room.AlternateRoom != null) ? (short)_roomsRemappingDictionary[room.AlternateRoom] : (short)-1,
                AlternateGroup = (byte)((room.Flipped && room.AlternateRoom != null) ? room.AlternateGroup : 0),
                Flipped = room.Flipped,
                FlippedRoom = room.AlternateRoom,
                BaseRoom = room.AlternateBaseRoom,
                AmbientIntensity = ((uint)roomAmbientColor.Red << 16) | ((uint)roomAmbientColor.Green << 8) | (uint)roomAmbientColor.Blue,
                ReverbInfo = (byte)room.Reverberation,
                Flags = 0x40
            };

            // Room flags

            if (room.FlagWater)
                newRoom.Flags |= 0x01;
            if (room.FlagOutside)
                newRoom.Flags |= 0x20;
            if (room.FlagHorizon)
                newRoom.Flags |= 0x08;
            if (room.FlagQuickSand)
                newRoom.Flags |= 0x80;
            if (room.FlagMist)
                newRoom.Flags |= 0x100;
            if (room.FlagReflection)
                newRoom.Flags |= 0x200;
            if (room.FlagSnow)
                newRoom.Flags |= 0x400;
            if (room.FlagRain)
                newRoom.Flags |= 0x800;
            if (room.FlagDamage)
                newRoom.Flags |= 0x1000;

            // Set the water scheme. I don't know how is calculated, but I have a table of all combinations of 
            // water and reflectivity. The water scheme must be set for the TOP room, in water room is 0x00.
            var waterPortals = new List<Portal>();

            if (!room.FlagWater)
            {
                for (var x = 0; x < room.NumXSectors; x++)
                {
                    for (var z = 0; z < room.NumZSectors; z++)
                    {
                        if (room.Blocks[x, z].FloorPortal == null)
                            continue;

                        if (!room.Blocks[x, z].FloorPortal.AdjoiningRoom.FlagWater)
                            continue;

                        if (!waterPortals.Contains(room.Blocks[x, z].FloorPortal))
                            waterPortals.Add(room.Blocks[x, z].FloorPortal);
                    }
                }

                if (waterPortals.Count > 0)
                {
                    var waterRoom = waterPortals[0].AdjoiningRoom;

                    if (!room.FlagReflection && waterRoom.WaterLevel == 1)
                        newRoom.WaterScheme = 0x06;
                    if (!room.FlagReflection && waterRoom.WaterLevel == 2)
                        newRoom.WaterScheme = 0x0a;
                    if (!room.FlagReflection && waterRoom.WaterLevel == 3)
                        newRoom.WaterScheme = 0x0e;
                    if (!room.FlagReflection && waterRoom.WaterLevel == 4)
                        newRoom.WaterScheme = 0x12;

                    if (room.FlagReflection && room.ReflectionLevel == 1 && waterRoom.WaterLevel == 1)
                        newRoom.WaterScheme = 0x05;
                    if (room.FlagReflection && room.ReflectionLevel == 2 && waterRoom.WaterLevel == 1)
                        newRoom.WaterScheme = 0x06;
                    if (room.FlagReflection && room.ReflectionLevel == 3 && waterRoom.WaterLevel == 1)
                        newRoom.WaterScheme = 0x07;
                    if (room.FlagReflection && room.ReflectionLevel == 4 && waterRoom.WaterLevel == 1)
                        newRoom.WaterScheme = 0x08;

                    if (room.FlagReflection && room.ReflectionLevel == 1 && waterRoom.WaterLevel == 2)
                        newRoom.WaterScheme = 0x09;
                    if (room.FlagReflection && room.ReflectionLevel == 2 && waterRoom.WaterLevel == 2)
                        newRoom.WaterScheme = 0x0a;
                    if (room.FlagReflection && room.ReflectionLevel == 3 && waterRoom.WaterLevel == 2)
                        newRoom.WaterScheme = 0x0b;
                    if (room.FlagReflection && room.ReflectionLevel == 4 && waterRoom.WaterLevel == 2)
                        newRoom.WaterScheme = 0x0c;

                    if (room.FlagReflection && room.ReflectionLevel == 1 && waterRoom.WaterLevel == 3)
                        newRoom.WaterScheme = 0x0d;
                    if (room.FlagReflection && room.ReflectionLevel == 2 && waterRoom.WaterLevel == 3)
                        newRoom.WaterScheme = 0x0e;
                    if (room.FlagReflection && room.ReflectionLevel == 3 && waterRoom.WaterLevel == 3)
                        newRoom.WaterScheme = 0x0f;
                    if (room.FlagReflection && room.ReflectionLevel == 4 && waterRoom.WaterLevel == 3)
                        newRoom.WaterScheme = 0x10;

                    if (room.FlagReflection && room.ReflectionLevel == 1 && waterRoom.WaterLevel == 4)
                        newRoom.WaterScheme = 0x11;
                    if (room.FlagReflection && room.ReflectionLevel == 2 && waterRoom.WaterLevel == 4)
                        newRoom.WaterScheme = 0x12;
                    if (room.FlagReflection && room.ReflectionLevel == 3 && waterRoom.WaterLevel == 4)
                        newRoom.WaterScheme = 0x13;
                    if (room.FlagReflection && room.ReflectionLevel == 4 && waterRoom.WaterLevel == 4)
                        newRoom.WaterScheme = 0x14;
                }
            }

            if (room.FlagMist)
                newRoom.WaterScheme += (byte)room.MistLevel;

            var lowest = -room.GetLowestCorner() * 256 + newRoom.Info.YBottom;

            // Generate geometry
            {
                // Add room geometry
                var roomVerticesDictionary = new Dictionary<tr_room_vertex, ushort>();
                var roomVertices = new List<tr_room_vertex>();

                var roomTriangles = new List<tr_face3>();
                var roomQuads = new List<tr_face4>();

                // Future code once vertex system of rooms is refactored
                /*var editorRoomVertices = room.GetRoomVertices();
                for (int i = 0; i < editorRoomVertices.Count; i += 3)
                {
                    ushort vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, waterPortals, lowest, editorRoomVertices[i].Position, editorRoomVertices[i].FaceColor);
                    ushort vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, waterPortals, lowest, editorRoomVertices[i + 1].Position, editorRoomVertices[i + 1].FaceColor);
                    ushort vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, waterPortals, lowest, editorRoomVertices[i + 2].Position, editorRoomVertices[i + 2].FaceColor);
                    Util.ObjectTextureManager.Result result = _objectTextureManager.AddTexture();

                    roomTriangles.Add(new tr_face3 { Vertices = new ushort[3] { vertex0Index, vertex1Index, vertex2Index }, Texture = texture });
                }*/

                var editorRoomVertices = room.GetRoomVertices();
                for (int z = 0; z < room.NumZSectors; ++z)
                    for (int x = 0; x < room.NumXSectors; ++x)
                        for (BlockFace face = 0; face < Block.FaceCount; ++face)
                        {
                            var range = room.GetFaceVertexRange(x, z, face);
                            if (range.Count == 0)
                                continue;

                            TextureArea texture = room.Blocks[x, z].GetFaceTexture(face);
                            if (texture.TextureIsInvisble)
                                continue;

                            for (int i = range.Start; i < (range.Start + range.Count); i += 3)
                            {
                                ushort vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, waterPortals, lowest, editorRoomVertices[i].Position, editorRoomVertices[i].FaceColor);
                                ushort vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, waterPortals, lowest, editorRoomVertices[i + 1].Position, editorRoomVertices[i + 1].FaceColor);
                                ushort vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, waterPortals, lowest, editorRoomVertices[i + 2].Position, editorRoomVertices[i + 2].FaceColor);
                                texture.TexCoord0 = editorRoomVertices[i].UV;
                                texture.TexCoord1 = editorRoomVertices[i + 1].UV;
                                texture.TexCoord2 = editorRoomVertices[i + 2].UV;

                                Util.ObjectTextureManager.Result result;
                                lock (_objectTextureManager)
                                    result = _objectTextureManager.AddTexture(texture, true, true);
                                roomTriangles.Add(result.CreateFace3(vertex0Index, vertex1Index, vertex2Index, 0));
                            }
                        }

                // Add geometry imported objects
                int geometryVertexIndexBase = roomVertices.Count;
                foreach (var geometry in room.Objects.OfType<RoomGeometryInstance>())
                    foreach (var mesh in geometry.Model.Meshes)
                        for (int j = 0; j < mesh.VertexCount; j++)
                        {
                            var trVertex = new tr_room_vertex();
                            trVertex.Position = new tr_vertex
                            {
                                X = (short)(mesh.Vertices[j].Position.X + geometry.Position.X),
                                Y = (short)(-mesh.Vertices[j].Position.Y + newRoom.Info.YBottom - geometry.Position.Y),
                                Z = (short)(mesh.Vertices[j].Position.Z + geometry.Position.Z)
                            };
                            trVertex.Lighting1 = 0;
                            trVertex.Lighting2 = 0x4210; // TODO: apply light calculations also to imported geometry
                            trVertex.Attributes = 0;

                            roomVertices.Add(trVertex);
                        }

                // Assign water effects
                if (room.FlagWater)
                {
                    for (int i = 0; i < roomVertices.Count; ++i)
                    {
                        var trVertex = roomVertices[i];
                        trVertex.Attributes = 0x4000;
                        roomVertices[i] = trVertex;
                    }
                }
                else if (waterPortals.Count > 0)
                {
                    for (int i = 0; i < roomVertices.Count; ++i)
                    {
                        var trVertex = roomVertices[i];
                        foreach (var portal in waterPortals)
                        {
                            if (trVertex.Position.X > portal.Area.X * 1024 && trVertex.Position.X <= portal.Area.Right * 1024 &&
                                trVertex.Position.Z > portal.Area.Y * 1024 && trVertex.Position.Z <= portal.Area.Bottom * 1024 &&
                                trVertex.Position.Y == lowest)
                            {
                                var xv = trVertex.Position.X / 1024;
                                var zv = trVertex.Position.Z / 1024;

                                if (!(room.IsFloorSolid(new DrawingPoint(xv, zv)) || room.Blocks[xv, zv].Type == BlockType.Wall ||
                                        room.Blocks[xv, zv].Type == BlockType.BorderWall) &&
                                    !(room.IsFloorSolid(new DrawingPoint(xv - 1, zv)) ||
                                        room.Blocks[xv - 1, zv].Type == BlockType.Wall ||
                                        room.Blocks[xv - 1, zv].Type == BlockType.BorderWall) &&
                                    !(room.IsFloorSolid(new DrawingPoint(xv, zv - 1)) ||
                                        room.Blocks[xv, zv - 1].Type == BlockType.Wall ||
                                        room.Blocks[xv, zv - 1].Type == BlockType.BorderWall) &&
                                    !(room.IsFloorSolid(new DrawingPoint(xv - 1, zv - 1)) ||
                                        room.Blocks[xv - 1, zv - 1].Type == BlockType.Wall ||
                                        room.Blocks[xv - 1, zv - 1].Type == BlockType.BorderWall))
                                {
                                    trVertex.Attributes = 0x6000;
                                }
                            }
                            else
                            {
                                if (!room.FlagReflection)
                                    continue;

                                if (trVertex.Position.X >= (portal.Area.X - 1) * 1024 && trVertex.Position.X <= (portal.Area.Right + 1) * 1024 &&
                                    trVertex.Position.Z >= (portal.Area.Y - 1) * 1024 && trVertex.Position.Z <= (portal.Area.Bottom + 1) * 1024)
                                {
                                    trVertex.Attributes = 0x4000;
                                }
                            }
                        }
                        roomVertices[i] = trVertex;
                    }
                }


                // Add imported geometry faces
                foreach (var geometry in room.Objects.OfType<RoomGeometryInstance>())
                    foreach (var mesh in geometry.Model.Meshes)
                    {
                        for (int j = 0; j < mesh.IndexCount; j += 3)
                        {
                            var triangle = new tr_face3();

                            ushort index0 = (ushort)(geometryVertexIndexBase + mesh.Indices[j + 0]);
                            ushort index1 = (ushort)(geometryVertexIndexBase + mesh.Indices[j + 1]);
                            ushort index2 = (ushort)(geometryVertexIndexBase + mesh.Indices[j + 2]);

                            triangle.Texture = 20;

                            // TODO Move texture area into the mesh
                            TextureArea texture;
                            texture.DoubleSided = false;
                            texture.BlendMode = BlendMode.Normal;
                            texture.Texture = null; // TODO Give geometry objects real textures
                            texture.TexCoord0 = mesh.Vertices[mesh.Indices[j + 0]].UV;
                            texture.TexCoord1 = mesh.Vertices[mesh.Indices[j + 1]].UV;
                            texture.TexCoord2 = mesh.Vertices[mesh.Indices[j + 2]].UV;
                            texture.TexCoord3 = new Vector2();

                            Util.ObjectTextureManager.Result result;
                            lock (_objectTextureManager)
                                result = _objectTextureManager.AddTexture(texture, true, true);
                            roomTriangles.Add(result.CreateFace3(index0, index1, index2, 0));
                        }

                        geometryVertexIndexBase += mesh.VertexCount;
                    }

                newRoom.Vertices = roomVertices;
                newRoom.Quads = roomQuads;
                newRoom.Triangles = roomTriangles;
            }

            // Build portals
            var tempIdPortals = new List<Portal>();

            for (var z = 0; z < room.NumZSectors; z++)
            {
                for (var x = 0; x < room.NumXSectors; x++)
                {
                    if (room.Blocks[x, z].WallPortal != null && !tempIdPortals.Contains(room.Blocks[x, z].WallPortal))
                        tempIdPortals.Add(room.Blocks[x, z].WallPortal);

                    if (room.Blocks[x, z].FloorPortal != null &&
                        !tempIdPortals.Contains(room.Blocks[x, z].FloorPortal))
                        tempIdPortals.Add(room.Blocks[x, z].FloorPortal);

                    if (room.Blocks[x, z].CeilingPortal != null &&
                        !tempIdPortals.Contains(room.Blocks[x, z].CeilingPortal))
                        tempIdPortals.Add(room.Blocks[x, z].CeilingPortal);
                }
            }

            ConvertPortals(tempIdPortals, room, ref newRoom);

            ConvertSectors(room, ref newRoom);

            var tempStaticMeshes = room.Objects.OfType<StaticInstance>()
                .Select(instance => new tr_room_staticmesh
                {
                    X = (int)Math.Round(newRoom.Info.X + instance.Position.X),
                    Y = (int)Math.Round(newRoom.Info.YBottom - instance.Position.Y),
                    Z = (int)Math.Round(newRoom.Info.Z + instance.Position.Z),
                    Rotation = (ushort)(Math.Max(0, Math.Min(ushort.MaxValue,
                        Math.Round(instance.RotationY * (65536.0 / 360.0))))),
                    ObjectID = (ushort)instance.WadObjectId,
                    Intensity1 = PackColorTo16Bit(instance.Color),
                    Intensity2 = 0
                })
                .ToArray();

            newRoom.NumStaticMeshes = (ushort)tempStaticMeshes.GetLength(0);
            newRoom.StaticMeshes = tempStaticMeshes;

            ConvertLights(room, ref newRoom);

            return newRoom;
        }

        private static ushort GetOrAddVertex(Room room, Dictionary<tr_room_vertex, ushort> roomVerticesDictionary, 
            List<tr_room_vertex> roomVertices, List<Portal> waterPortals, int lowest, Vector3 Position, Vector4 Color)
        {
            tr_room_vertex trVertex;
            trVertex.Position = new tr_vertex
            {
                X = (short)(Position.X),
                Y = (short)-(Position.Y + room.WorldPos.Y),
                Z = (short)(Position.Z)
            };
            trVertex.Lighting1 = 0;
            trVertex.Lighting2 = PackColorTo16Bit(Color);
            trVertex.Attributes = 0;

            // Do we need this vertex?
            ushort vertexIndex;
            if (roomVerticesDictionary.TryGetValue(trVertex, out vertexIndex))
                return vertexIndex;

            // Add vertex
            vertexIndex = (ushort)roomVertices.Count;
            roomVerticesDictionary.Add(trVertex, vertexIndex);
            roomVertices.Add(trVertex);
            return vertexIndex;
        }

        private static void ConvertLights(Room room, ref tr_room newRoom)
        {
            var result = new List<tr4_room_light>();
            foreach (var light in room.Objects.OfType<Light>().Where(l => l.IsDynamicallyUsed))
            {
                var newLight = new tr4_room_light
                {
                    X = (int)Math.Round(newRoom.Info.X + light.Position.X),
                    Y = (int)Math.Round(-light.Position.Y + newRoom.Info.YBottom),
                    Z = (int)Math.Round(newRoom.Info.Z + light.Position.Z),
                    Color = PackColorTo24Bit(new Vector4(light.Color, 1.0f)),
                    Intensity = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, Math.Abs(light.Intensity) * 8192.0f))
                };

                if (newLight.Intensity == 0)
                    continue;

                Vector3 direction = light.GetDirection();

                switch (light.Type)
                {
                    case LightType.Light:
                        newLight.LightType = 1;
                        newLight.In = light.In * 1024.0f;
                        newLight.Out = light.Out * 1024.0f;
                        break;
                    case LightType.Shadow:
                        newLight.LightType = 3;
                        newLight.In = light.In * 1024.0f;
                        newLight.Out = light.Out * 1024.0f;
                        break;
                    case LightType.Spot:
                        newLight.LightType = 2;
                        newLight.In = (float)Math.Cos(MathUtil.DegreesToRadians(light.In));
                        newLight.Out = (float)Math.Cos(MathUtil.DegreesToRadians(light.Out));
                        newLight.Length = light.Len * 1024.0f;
                        newLight.CutOff = light.Cutoff * 1024.0f;
                        newLight.DirectionX = -direction.X;
                        newLight.DirectionY = direction.Y;
                        newLight.DirectionZ = -direction.Z;
                        break;
                    case LightType.Sun:
                        newLight.LightType = 0;
                        newLight.In = 0;
                        newLight.Out = 0;
                        newLight.Length = 0;
                        newLight.CutOff = 0;
                        newLight.DirectionX = -direction.X;
                        newLight.DirectionY = direction.Y;
                        newLight.DirectionZ = -direction.Z;
                        break;
                    case LightType.FogBulb:
                        newLight.LightType = 4;
                        newLight.In = light.In * 1024;
                        newLight.Out = light.Out * 1024;
                        break;
                    case LightType.Effect:
                        continue;
                    default:
                        throw new Exception("Unknown light type '" + light.Type + "' encountered.");
                }

                result.Add(newLight);
            }

            newRoom.NumLights = (ushort)result.Count;
            newRoom.Lights = result.ToArray();
        }

        private void ConvertSectors(Room room, ref tr_room newRoom)
        {
            newRoom.Sectors = new tr_room_sector[room.NumXSectors * room.NumZSectors];
            newRoom.AuxSectors = new TrSectorAux[room.NumXSectors, room.NumZSectors];

            for (var z = 0; z < room.NumZSectors; z++)
            {
                for (var x = 0; x < room.NumXSectors; x++)
                {
                    var block = room.Blocks[x, z];
                    var sector = new tr_room_sector();
                    var aux = new TrSectorAux();

                    sector.BoxIndex = (ushort)(0x7ff0 | (0xf & (int)GetTextureSound(room, x, z)));
                    sector.FloorDataIndex = 0;

                    if (block.FloorPortal != null)
                        sector.RoomBelow = (byte)_roomsRemappingDictionary[block.FloorPortal.AdjoiningRoom];
                    else
                        sector.RoomBelow = 0xff;

                    if (block.CeilingPortal != null)
                        sector.RoomAbove = (byte)_roomsRemappingDictionary[block.CeilingPortal.AdjoiningRoom];
                    else
                        sector.RoomAbove = 0xff;

                    if (x == 0 || z == 0 || x == room.NumXSectors - 1 || z == room.NumZSectors - 1 ||
                        block.Type == BlockType.BorderWall || block.Type == BlockType.Wall)
                    {
                        sector.Floor = (sbyte)(-room.Position.Y - block.FloorMax);
                        sector.Ceiling = (sbyte)(-room.Position.Y - block.CeilingMin);
                    }
                    else
                    {
                        sector.Floor = (sbyte)(-room.Position.Y - block.FloorMax);
                        sector.Ceiling = (sbyte)(-room.Position.Y - block.CeilingMin);
                    }

                    //Setup some aux data for box generation
                    if (block.Type == BlockType.BorderWall)
                        aux.BorderWall = true;
                    if ((block.Flags & BlockFlags.Monkey) != 0)
                        aux.Monkey = true;
                    if ((block.Flags & BlockFlags.Box) != 0)
                        aux.Box = true;
                    if ((block.Flags & BlockFlags.NotWalkableFloor) != 0)
                        aux.NotWalkableFloor = true;
                    if (!room.FlagWater && (Math.Abs(block.FloorIfQuadSlopeX) == 1 ||
                                            Math.Abs(block.FloorIfQuadSlopeX) == 2 ||
                                            Math.Abs(block.FloorIfQuadSlopeZ) == 1 ||
                                            Math.Abs(block.FloorIfQuadSlopeZ) == 2))
                        aux.SoftSlope = true;
                    if (!room.FlagWater && (Math.Abs(block.FloorIfQuadSlopeX) > 2 ||
                                            Math.Abs(block.FloorIfQuadSlopeZ) > 2))
                        aux.HardSlope = true;
                    if (block.Type == BlockType.Wall)
                        aux.Wall = true;

                    // I must setup portal only if current sector is not solid and opacity if different from 1
                    if (block.FloorPortal != null)
                    {
                        if ((!room.IsFloorSolid(new DrawingPoint(x, z)) &&
                             block.FloorOpacity != PortalOpacity.Opacity1) ||
                            (room.IsFloorSolid(new DrawingPoint(x, z)) && block.NoCollisionFloor))
                        {
                            var portal = block.FloorPortal;
                            sector.RoomBelow = (byte)_roomsRemappingDictionary[block.FloorPortal.AdjoiningRoom];
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

                    if ((block.FloorPortal != null &&
                         block.FloorOpacity != PortalOpacity.Opacity1 &&
                         !room.IsFloorSolid(new DrawingPoint(x, z))))
                    {
                        aux.Portal = true;
                        aux.FloorPortal = block.FloorPortal;
                    }
                    else
                    {
                        aux.FloorPortal = null;
                    }

                    aux.IsFloorSolid = room.IsFloorSolid(new DrawingPoint(x, z));

                    if ((block.CeilingPortal != null &&
                         block.CeilingOpacity != PortalOpacity.Opacity1))
                    {
                    }
                    else
                    {
                    }

                    if (block.WallPortal != null && block.WallOpacity != PortalOpacity.Opacity1)
                        aux.WallPortal = block.WallPortal.AdjoiningRoom;
                    else
                        aux.WallPortal = null;

                    aux.LowestFloor = (sbyte)(-room.Position.Y - block.FloorMin);
                    var q0 = block.QAFaces[0];
                    var q1 = block.QAFaces[1];
                    var q2 = block.QAFaces[2];
                    var q3 = block.QAFaces[3];

                    if (!Block.IsQuad(q0, q1, q2, q3) && block.FloorIfQuadSlopeX == 0 &&
                        block.FloorIfQuadSlopeZ == 0)
                    {
                        if (!block.FloorSplitDirectionIsXEqualsY)
                        {
                            aux.LowestFloor = (sbyte)(-room.Position.Y - Math.Min(block.QAFaces[0],
                                                           block.QAFaces[2]));
                        }
                        else
                        {
                            aux.LowestFloor = (sbyte)(-room.Position.Y - Math.Min(block.QAFaces[1],
                                                           block.QAFaces[3]));
                        }
                    }

                    newRoom.AuxSectors[x, z] = aux;
                    newRoom.Sectors[room.NumZSectors * x + z] = sector;
                }
            }
        }

        private void ConvertPortals(IEnumerable<Portal> tempIdPortals, Room room, ref tr_room newRoom)
        {
            var result = new List<tr_room_portal>();

            foreach (var portal in tempIdPortals)
            {
                int xMin;
                int xMax;
                int zMin;
                int zMax;

                var newPortal = new tr_room_portal
                {
                    AdjoiningRoom = (ushort)_roomsRemappingDictionary[portal.AdjoiningRoom],
                    Vertices = new tr_vertex[4]
                };

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

                            xMin = portal.Area.Left;
                            xMax = portal.Area.Right + 1;
                            zMin = room.NumZSectors - 1;
                            zMax = room.NumZSectors - 1;

                            var yMin = 32768;
                            var yMax = -32768;

                            int y1;
                            int y2;

                            Room adjoiningRoom = portal.AdjoiningRoom;
                            int minFloorCurrentRoom = room.GetLowestCorner() + (int)room.Position.Y;
                            int minFloorOtherRoom = adjoiningRoom.GetLowestCorner() + (int)adjoiningRoom.Position.Y;

                            for (var x = xMin; x < xMax; x++)
                            {
                                var currentBlock = room.Blocks[x, room.NumZSectors - 2];
                                // var facingBlock = room.Blocks[x, room.NumZSectors - 1];
                                var otherBlock = adjoiningRoom.Blocks[x + (int)(room.Position.X - adjoiningRoom.Position.X), 1];

                                y1 = Math.Max(minFloorOtherRoom + otherBlock.QAFaces[3], minFloorCurrentRoom + currentBlock.QAFaces[0]);
                                y2 = Math.Min(minFloorOtherRoom + otherBlock.WSFaces[3], minFloorCurrentRoom + currentBlock.WSFaces[0]);

                                if (y1 < yMin)
                                    yMin = y1;
                                if (y2 > yMax)
                                    yMax = y2;
                            }

                            var lastBlock = room.Blocks[xMax - 1, room.NumZSectors - 2];
                            //  var lastFacingBlock = room.Blocks[xMax - 1, room.NumZSectors - 1];
                            var lastOtherBlock = adjoiningRoom.Blocks[xMax - 1 + (int)(room.Position.X - adjoiningRoom.Position.X), 1];

                            y1 = Math.Max(minFloorOtherRoom + lastOtherBlock.QAFaces[2], minFloorCurrentRoom + lastBlock.QAFaces[1]);
                            y2 = Math.Min(minFloorOtherRoom + lastOtherBlock.WSFaces[2], minFloorCurrentRoom + lastBlock.WSFaces[1]);

                            if (y1 < yMin)
                                yMin = y1;
                            if (y2 > yMax)
                                yMax = y2;

                            yMin -= minFloorCurrentRoom;
                            yMax -= minFloorCurrentRoom;

                            newPortal.Vertices[0] = new tr_vertex
                            {
                                X = (short)(xMin * 1024.0f),
                                Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMin * 1024.0f)
                            };

                            newPortal.Vertices[1] = new tr_vertex
                            {
                                X = (short)(xMax * 1024.0f),
                                Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMin * 1024.0f)
                            };

                            newPortal.Vertices[2] = new tr_vertex
                            {
                                X = (short)(xMax * 1024.0f),
                                Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMax * 1024.0f)
                            };

                            newPortal.Vertices[3] = new tr_vertex
                            {
                                X = (short)(xMin * 1024.0f),
                                Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMax * 1024.0f)
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
                            zMin = portal.Area.Bottom + 1;
                            zMax = portal.Area.Y;

                            var yMin = 32768;
                            var yMax = -32768;

                            int y1;
                            int y2;

                            Room adjoiningRoom = portal.AdjoiningRoom;
                            int minFloorCurrentRoom = room.GetLowestCorner() + (int)room.Position.Y;
                            int minFloorOtherRoom = adjoiningRoom.GetLowestCorner() + (int)adjoiningRoom.Position.Y;

                            for (var z = zMax; z < zMin; z++)
                            {
                                var currentBlock = room.Blocks[room.NumXSectors - 2, z];
                                //var facingBlock = room.Blocks[room.NumXSectors - 1, z];
                                var otherBlock = adjoiningRoom.Blocks[1, z + (int)(room.Position.Z - adjoiningRoom.Position.Z)];

                                y1 = Math.Max(minFloorOtherRoom + otherBlock.QAFaces[0], minFloorCurrentRoom + currentBlock.QAFaces[1]);
                                y2 = Math.Min(minFloorOtherRoom + otherBlock.WSFaces[0], minFloorCurrentRoom + currentBlock.WSFaces[1]);

                                if (y1 < yMin)
                                    yMin = y1;
                                if (y2 > yMax)
                                    yMax = y2;
                            }

                            var lastBlock = room.Blocks[room.NumXSectors - 2, zMin - 1];
                            //  var lastFacingBlock = room.Blocks[room.NumXSectors - 1, zMin - 1];
                            var lastOtherBlock = adjoiningRoom.Blocks[1, zMin - 1 + (int)(room.Position.Z - adjoiningRoom.Position.Z)];

                            y1 = Math.Max(minFloorOtherRoom + lastOtherBlock.QAFaces[3], minFloorCurrentRoom + lastBlock.QAFaces[2]);
                            y2 = Math.Min(minFloorOtherRoom + lastOtherBlock.WSFaces[3], minFloorCurrentRoom + lastBlock.WSFaces[2]);

                            if (y1 < yMin)
                                yMin = y1;
                            if (y2 > yMax)
                                yMax = y2;

                            yMin -= minFloorCurrentRoom;
                            yMax -= minFloorCurrentRoom;

                            newPortal.Vertices[1] = new tr_vertex
                            {
                                X = (short)(xMin * 1024),
                                Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMin * 1024.0f)
                            };

                            newPortal.Vertices[2] = new tr_vertex
                            {
                                X = (short)(xMax * 1024),
                                Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMax * 1024.0f)
                            };

                            newPortal.Vertices[3] = new tr_vertex
                            {
                                X = (short)(xMax * 1024),
                                Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMax * 1024.0f)
                            };

                            newPortal.Vertices[0] = new tr_vertex
                            {
                                X = (short)(xMin * 1024),
                                Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMin * 1024.0f)
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

                            xMin = portal.Area.Right + 1;
                            xMax = portal.Area.Left;
                            zMin = 1;
                            zMax = 1;

                            var yMin = 32768;
                            var yMax = -32768;

                            int y1;
                            int y2;

                            Room adjoiningRoom = portal.AdjoiningRoom;
                            int minFloorCurrentRoom = room.GetLowestCorner() + (int)room.Position.Y;
                            int minFloorOtherRoom = adjoiningRoom.GetLowestCorner() + (int)adjoiningRoom.Position.Y;

                            for (var x = xMax; x < xMin; x++)
                            {
                                var currentBlock = room.Blocks[x, 1];
                                // var facingBlock = room.Blocks[x, 0];
                                var otherBlock = adjoiningRoom.Blocks[x + (int)(room.Position.X - adjoiningRoom.Position.X), adjoiningRoom.NumZSectors - 1];

                                y1 = Math.Max(minFloorOtherRoom + otherBlock.QAFaces[1], minFloorCurrentRoom + currentBlock.QAFaces[2]);
                                y2 = Math.Min(minFloorOtherRoom + otherBlock.WSFaces[1], minFloorCurrentRoom + currentBlock.WSFaces[2]);

                                if (y1 < yMin)
                                    yMin = y1;
                                if (y2 > yMax)
                                    yMax = y2;
                            }

                            var lastBlock = room.Blocks[xMin - 1, 1];
                            //  var lastFacingBlock = room.Blocks[xMin - 1, 0];
                            var lastOtherBlock = adjoiningRoom.Blocks[xMin - 1 + (int)(room.Position.X - adjoiningRoom.Position.X), adjoiningRoom.NumZSectors - 1];

                            y1 = Math.Max(minFloorOtherRoom + lastOtherBlock.QAFaces[0], minFloorCurrentRoom + lastBlock.QAFaces[3]);
                            y2 = Math.Min(minFloorOtherRoom + lastOtherBlock.WSFaces[0], minFloorCurrentRoom + lastBlock.WSFaces[3]);

                            if (y1 < yMin)
                                yMin = y1;
                            if (y2 > yMax)
                                yMax = y2;

                            yMin -= minFloorCurrentRoom;
                            yMax -= minFloorCurrentRoom;

                            newPortal.Vertices[0] = new tr_vertex
                            {
                                X = (short)(xMin * 1024.0f),
                                Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMin * 1024.0f - 1.0f)
                            };

                            newPortal.Vertices[1] = new tr_vertex
                            {
                                X = (short)(xMax * 1024.0f),
                                Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMin * 1024.0f - 1.0f)
                            };

                            newPortal.Vertices[2] = new tr_vertex
                            {
                                X = (short)(xMax * 1024.0f),
                                Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMax * 1024.0f - 1.0f)
                            };

                            newPortal.Vertices[3] = new tr_vertex
                            {
                                X = (short)(xMin * 1024.0f),
                                Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMax * 1024.0f - 1.0f)
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
                            zMin = portal.Area.Y;
                            zMax = portal.Area.Bottom + 1;

                            var yMin = 32768;
                            var yMax = -32768;

                            int y1;
                            int y2;

                            Room adjoiningRoom = portal.AdjoiningRoom;
                            int minFloorCurrentRoom = room.GetLowestCorner() + (int)room.Position.Y;
                            int minFloorOtherRoom = adjoiningRoom.GetLowestCorner() + (int)adjoiningRoom.Position.Y;

                            for (var z = zMin; z < zMax; z++)
                            {
                                var currentBlock = room.Blocks[1, z];
                                //  var facingBlock = room.Blocks[0, z];
                                var otherBlock = adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 1, z + (int)(room.Position.Z - adjoiningRoom.Position.Z)];

                                y1 = Math.Max(minFloorOtherRoom + otherBlock.QAFaces[2], minFloorCurrentRoom + currentBlock.QAFaces[3]);
                                y2 = Math.Min(minFloorOtherRoom + otherBlock.WSFaces[2], minFloorCurrentRoom + currentBlock.WSFaces[3]);

                                if (y1 < yMin)
                                    yMin = y1;
                                if (y2 > yMax)
                                    yMax = y2;
                            }

                            var lastBlock = room.Blocks[1, zMax - 1];
                            //var lastFacingBlock = room.Blocks[0, zMax - 1];
                            var lastOtherBlock = adjoiningRoom.Blocks[adjoiningRoom.NumXSectors - 1, zMax - 1 + (int)(room.Position.Z - adjoiningRoom.Position.Z)];

                            y1 = Math.Max(minFloorOtherRoom + lastOtherBlock.QAFaces[1], minFloorCurrentRoom + lastBlock.QAFaces[0]);
                            y2 = Math.Min(minFloorOtherRoom + lastOtherBlock.WSFaces[1], minFloorCurrentRoom + lastBlock.WSFaces[0]);

                            if (y1 < yMin)
                                yMin = y1;
                            if (y2 > yMax)
                                yMax = y2;

                            yMin -= minFloorCurrentRoom;
                            yMax -= minFloorCurrentRoom;

                            newPortal.Vertices[1] = new tr_vertex
                            {
                                X = (short)(xMin * 1024.0f - 1.0f),
                                Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMin * 1024.0f)
                            };

                            newPortal.Vertices[2] = new tr_vertex
                            {
                                X = (short)(xMax * 1024.0f - 1.0f),
                                Y = (short)(-yMax * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMax * 1024.0f)
                            };

                            newPortal.Vertices[3] = new tr_vertex
                            {
                                X = (short)(xMax * 1024.0f - 1.0f),
                                Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMax * 1024.0f)
                            };

                            newPortal.Vertices[0] = new tr_vertex
                            {
                                X = (short)(xMin * 1024.0f - 1.0f),
                                Y = (short)(-yMin * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMin * 1024.0f)
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

                            xMin = portal.Area.Left;
                            xMax = portal.Area.Right + 1;
                            var y = room.GetLowestCorner();
                            zMin = portal.Area.Y;
                            zMax = portal.Area.Bottom + 1;

                            newPortal.Vertices[1] = new tr_vertex
                            {
                                X = (short)(xMin * 1024.0f),
                                Y = (short)(-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMin * 1024.0f)
                            };

                            newPortal.Vertices[2] = new tr_vertex
                            {
                                X = (short)(xMin * 1024.0f),
                                Y = (short)(-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMax * 1024.0f)
                            };

                            newPortal.Vertices[3] = new tr_vertex
                            {
                                X = (short)(xMax * 1024.0f),
                                Y = (short)(-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMax * 1024.0f)
                            };

                            newPortal.Vertices[0] = new tr_vertex
                            {
                                X = (short)(xMax * 1024.0f),
                                Y = (short)(-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMin * 1024.0f)
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

                            xMin = portal.Area.Left;
                            xMax = portal.Area.Right + 1;
                            var y = room.GetHighestCorner();
                            zMin = portal.Area.Bottom + 1;
                            zMax = portal.Area.Y;

                            newPortal.Vertices[1] = new tr_vertex
                            {
                                X = (short)(xMin * 1024.0f),
                                Y = (short)(-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMin * 1024.0f)
                            };

                            newPortal.Vertices[2] = new tr_vertex
                            {
                                X = (short)(xMin * 1024.0f),
                                Y = (short)(-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMax * 1024.0f)
                            };

                            newPortal.Vertices[3] = new tr_vertex
                            {
                                X = (short)(xMax * 1024.0f),
                                Y = (short)(-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMax * 1024.0f)
                            };

                            newPortal.Vertices[0] = new tr_vertex
                            {
                                X = (short)(xMax * 1024.0f),
                                Y = (short)(-y * 256.0f + newRoom.Info.YBottom),
                                Z = (short)(zMin * 1024.0f)
                            };
                        }
                        break;
                }

                result.Add(newPortal);
            }

            newRoom.NumPortals = (ushort)result.Count;
            newRoom.Portals = result.ToArray();
        }
        
        private void MatchPortalVertexColors()
        {
            // Match vertex colors on portal 
            // Operate on final vertices here because:
            //   - We don't want to change the lighting of rooms on output
            //   - Geometry objects should also be make use of this.

            // Build lookup
            var vertexColorLookups = new Dictionary<Room, Dictionary<tr_vertex, ushort>>();
            Parallel.ForEach(_tempRooms.Values, (tr_room trRoom) =>
            {
                var vertexLookup = new Dictionary<tr_vertex, ushort>();
                var vertices = trRoom.Vertices;
                for (int i = 0; i < vertices.Count; ++i)
                {
                    tr_room_vertex vertex = vertices[i];
                    if (!vertexLookup.ContainsKey(vertex.Position))
                        vertexLookup.Add(vertex.Position, vertex.Lighting2);
                }
                lock (vertexColorLookups)
                    vertexColorLookups.Add(trRoom.OriginalRoom, vertexLookup);
            });

            // Match portals in each room ...
            Parallel.ForEach(_tempRooms.Values, (tr_room trRoom) =>
            {
                Vector3 roomWorldPos = trRoom.OriginalRoom.WorldPos;

                // Altough the usage pattern is superficially more suited for Set/Dictionary,
                // a List is used here since it will only have a few entries at max and linear search most likely beats everything else at that.
                List<Room> roomsSharedByVertex = new List<Room>();
                var vertices = trRoom.Vertices;
                for (int i = 0; i < vertices.Count; ++i)
                {
                    tr_room_vertex vertex = vertices[i];
                    Vector3 worldPos = new Vector3(vertex.Position.X + roomWorldPos.X, -vertex.Position.Y, vertex.Position.Z + roomWorldPos.Z);

                    // Find connected rooms that might share this vertex
                    roomsSharedByVertex.Clear();
                    roomsSharedByVertex.Add(trRoom.OriginalRoom);
                    FindConnectedRooms(roomsSharedByVertex, trRoom.OriginalRoom, worldPos, true);
                    if (roomsSharedByVertex.Count == 0)
                        continue;

                    int R = (vertex.Lighting2 >> 10) & 0x1F;
                    int G = (vertex.Lighting2 >> 5) & 0x1F;
                    int B = vertex.Lighting2 & 0x1F;
                    int Count = 1;

                    foreach (Room connectedRoom in roomsSharedByVertex)
                    {
                        if (connectedRoom == trRoom.OriginalRoom)
                            continue;

                        // Find position in room local coordinates
                        Vector3 connectedRoomWorldPos = connectedRoom.WorldPos;
                        Vector3 trVertexPos = new Vector3(worldPos.X - connectedRoomWorldPos.X, -worldPos.Y, worldPos.Z - connectedRoomWorldPos.Z);
                        float maxCoordinate = Math.Max(
                            Math.Abs(trVertexPos.X),
                            Math.Max(
                                Math.Abs(trVertexPos.Y),
                                Math.Abs(trVertexPos.Z)));
                        if (maxCoordinate > short.MaxValue)
                            continue;

                        tr_vertex connectedRoomLocalPosUint = new tr_vertex
                        {
                            X = (short)(trVertexPos.X),
                            Y = (short)(trVertexPos.Y),
                            Z = (short)(trVertexPos.Z)
                        };

                        // Lookup vertex
                        Dictionary<tr_vertex, ushort> connectedRoomColorLookup;
                        if (!vertexColorLookups.TryGetValue(connectedRoom, out connectedRoomColorLookup))
                            continue;
                        ushort lighting;
                        if (!connectedRoomColorLookup.TryGetValue(connectedRoomLocalPosUint, out lighting))
                            continue;

                        // Accumulate color for average
                        R += (lighting >> 10) & 0x1F;
                        G += (lighting >> 5) & 0x1F;
                        B += lighting & 0x1F;
                        ++Count;
                    }

                    // Set color
                    if (Count > 1)
                    {
                        Vector4 averageColor = new Vector4(R, G, B, 0.0f) * ((1.0f / 16.0f) / Count);
                        vertex.Lighting2 = PackColorTo16Bit(averageColor);
                        vertices[i] = vertex;
                    }
                }
            });
        }

        private static void FindConnectedRooms(List<Room> outSharedRooms, Room currentRoom, Vector3 worldPos, bool checkFloorCeiling)
        {
            Vector3 localPos = worldPos - currentRoom.WorldPos;
            int sectorPosX = (int)(localPos.X * (1.0f / 1024.0f) + 0.5f);
            int sectorPosZ = (int)(localPos.Z * (1.0f / 1024.0f) + 0.5f);
            int sectorPosX2 = sectorPosX - 1;
            int sectorPosZ2 = sectorPosZ - 1;

            // Check up to 4 sectors around the destination
            if ((sectorPosX >= 0) && (sectorPosX < currentRoom.NumXSectors))
            {
                if ((sectorPosZ >= 0) && (sectorPosZ < currentRoom.NumZSectors))
                    FindConnectedRoomsCheckSector(outSharedRooms, currentRoom, worldPos, checkFloorCeiling, sectorPosX, sectorPosZ);
                if ((sectorPosZ2 >= 0) && (sectorPosZ2 < currentRoom.NumZSectors))
                    FindConnectedRoomsCheckSector(outSharedRooms, currentRoom, worldPos, checkFloorCeiling, sectorPosX, sectorPosZ2);
            }
            if ((sectorPosX2 >= 0) && (sectorPosX2 < currentRoom.NumXSectors))
            {
                if ((sectorPosZ >= 0) && (sectorPosZ < currentRoom.NumZSectors))
                    FindConnectedRoomsCheckSector(outSharedRooms, currentRoom, worldPos, checkFloorCeiling, sectorPosX2, sectorPosZ);
                if ((sectorPosZ2 >= 0) && (sectorPosZ2 < currentRoom.NumZSectors))
                    FindConnectedRoomsCheckSector(outSharedRooms, currentRoom, worldPos, checkFloorCeiling, sectorPosX2, sectorPosZ2);
            }
        }

        private static void FindConnectedRoomsCheckSector(List<Room> outSharedRooms, Room currentRoom, Vector3 worldPos, bool checkFloorCeiling, int x, int z)
        {
            Block block = currentRoom.Blocks[x, z];

            if (checkFloorCeiling)
            {
                if ((block.FloorPortal != null) && !outSharedRooms.Contains(block.FloorPortal.AdjoiningRoom))
                {
                    outSharedRooms.Add(block.FloorPortal.AdjoiningRoom);
                    FindConnectedRooms(outSharedRooms, block.FloorPortal.AdjoiningRoom, worldPos, false);
                }
                if ((block.CeilingPortal != null) && !outSharedRooms.Contains(block.CeilingPortal.AdjoiningRoom))
                {
                    outSharedRooms.Add(block.CeilingPortal.AdjoiningRoom);
                    FindConnectedRooms(outSharedRooms, block.CeilingPortal.AdjoiningRoom, worldPos, false);
                }
            }

            if ((block.WallPortal != null) && !outSharedRooms.Contains(block.WallPortal.AdjoiningRoom))
            {
                outSharedRooms.Add(block.WallPortal.AdjoiningRoom);
                FindConnectedRooms(outSharedRooms, block.WallPortal.AdjoiningRoom, worldPos, checkFloorCeiling);
            }
        }

        private static ushort PackColorTo16Bit(Vector4 color)
        {
            color *= 16.0f;
            color += new Vector4(0.5f); // Round correctly
            color = Vector4.Min(new Vector4(31), Vector4.Max(new Vector4(0), color));
            
            ushort tmp = 0;
            tmp |= (ushort)((ushort)(color.X) << 10);
            tmp |= (ushort)((ushort)(color.Y) << 5);
            tmp |= (ushort)(color.Z);
            return tmp;
        }

        private static tr_color PackColorTo24Bit(Vector4 color)
        {
            color *= 128.0f;
            color += new Vector4(0.5f); // Round correctly
            color = Vector4.Min(new Vector4(255), Vector4.Max(new Vector4(0), color));

            tr_color result;
            result.Red = (byte)color.X;
            result.Green = (byte)color.Y;
            result.Blue = (byte)color.Z;
            return result;
        }
    }
}
