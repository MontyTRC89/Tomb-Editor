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
                Lights = new List<tr4_room_light>(),
                StaticMeshes = new List<tr_room_staticmesh>(),
                Portals = new List<tr_room_portal>(),
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
                    ushort vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, waterPortals, editorRoomVertices[i].Position, editorRoomVertices[i].FaceColor);
                    ushort vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, waterPortals, editorRoomVertices[i + 1].Position, editorRoomVertices[i + 1].FaceColor);
                    ushort vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, waterPortals, editorRoomVertices[i + 2].Position, editorRoomVertices[i + 2].FaceColor);
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
                                ushort vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, waterPortals, editorRoomVertices[i].Position, editorRoomVertices[i].FaceColor);
                                ushort vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, waterPortals, editorRoomVertices[i + 1].Position, editorRoomVertices[i + 1].FaceColor);
                                ushort vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, waterPortals, editorRoomVertices[i + 2].Position, editorRoomVertices[i + 2].FaceColor);
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
                                Y = (short)-(mesh.Vertices[j].Position.Y + room.WorldPos.Y + geometry.Position.Y),
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
                                trVertex.Position.Y == -(room.GetLowestCorner() * 256 + room.WorldPos.Y))
                            {
                                var xv = trVertex.Position.X / 1024;
                                var zv = trVertex.Position.Z / 1024;

                                if ((room.GetFloorRoomConnectionInfo(new DrawingPoint(xv, zv)).AnyType != Room.RoomConnectionType.NoPortal || room.Blocks[xv, zv].IsAnyWall) &&
                                    (room.GetFloorRoomConnectionInfo(new DrawingPoint(xv - 1, zv)).AnyType != Room.RoomConnectionType.NoPortal || room.Blocks[xv - 1, zv].IsAnyWall) &&
                                    (room.GetFloorRoomConnectionInfo(new DrawingPoint(xv, zv - 1)).AnyType != Room.RoomConnectionType.NoPortal || room.Blocks[xv, zv - 1].IsAnyWall) &&
                                    (room.GetFloorRoomConnectionInfo(new DrawingPoint(xv - 1, zv - 1)).AnyType != Room.RoomConnectionType.NoPortal || room.Blocks[xv - 1, zv - 1].IsAnyWall))
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

            ConvertPortals(room, tempIdPortals, newRoom);

            ConvertSectors(room, newRoom);

            foreach (var instance in room.Objects.OfType<StaticInstance>())
                newRoom.StaticMeshes.Add(new tr_room_staticmesh
                {
                    X = (int)Math.Round(newRoom.Info.X + instance.Position.X),
                    Y = (int)-Math.Round(room.WorldPos.Y + instance.Position.Y),
                    Z = (int)Math.Round(newRoom.Info.Z + instance.Position.Z),
                    Rotation = (ushort)(Math.Max(0, Math.Min(ushort.MaxValue,
                        Math.Round(instance.RotationY * (65536.0 / 360.0))))),
                    ObjectID = (ushort)instance.WadObjectId,
                    Intensity1 = PackColorTo16Bit(new Vector4(instance.Color.Z, instance.Color.Y, instance.Color.X, instance.Color.W)),
                    Intensity2 = instance.Ocb
                });

            ConvertLights(room, newRoom);

            return newRoom;
        }

        private static ushort GetOrAddVertex(Room room, Dictionary<tr_room_vertex, ushort> roomVerticesDictionary, 
            List<tr_room_vertex> roomVertices, List<Portal> waterPortals, Vector3 Position, Vector4 Color)
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

        private static void ConvertLights(Room room, tr_room newRoom)
        {
            foreach (var light in room.Objects.OfType<Light>().Where(l => l.IsDynamicallyUsed))
            {
                var newLight = new tr4_room_light
                {
                    X = (int)Math.Round(newRoom.Info.X + light.Position.X),
                    Y = (int)Math.Round(-light.Position.Y + room.WorldPos.Y),
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

                newRoom.Lights.Add(newLight);
            }
        }

        private void ConvertSectors(Room room, tr_room newRoom)
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

                    // Setup portals
                    if (room.GetFloorRoomConnectionInfo(new DrawingPoint(x, z)).TraversableType != Room.RoomConnectionType.NoPortal)
                    {
                        sector.RoomBelow = (byte)_roomsRemappingDictionary[block.FloorPortal.AdjoiningRoom];
                        aux.Portal = true;
                        aux.FloorPortal = block.FloorPortal;
                    }
                    else
                    {
                        sector.RoomBelow = 255;
                        aux.FloorPortal = null;
                    }

                    if (room.GetCeilingRoomConnectionInfo(new DrawingPoint(x, z)).TraversableType != Room.RoomConnectionType.NoPortal)
                        sector.RoomAbove = (byte)_roomsRemappingDictionary[block.CeilingPortal.AdjoiningRoom];
                    else
                        sector.RoomAbove = 255;

                    if (block.WallPortal != null && block.WallPortal.Opacity != PortalOpacity.SolidFaces)
                        aux.WallPortal = block.WallPortal.AdjoiningRoom;
                    else
                        aux.WallPortal = null;

                    // Setup some flags for box generation
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

                    // Setup floor heights
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
                    
                    aux.LowestFloor = (sbyte)(-room.Position.Y - block.FloorMin);
                    var q0 = block.QAFaces[Block.FaceXnZp];
                    var q1 = block.QAFaces[Block.FaceXpZp];
                    var q2 = block.QAFaces[Block.FaceXpZn];
                    var q3 = block.QAFaces[Block.FaceXnZn];

                    if (!Block.IsQuad(q0, q1, q2, q3) && block.FloorIfQuadSlopeX == 0 &&
                        block.FloorIfQuadSlopeZ == 0)
                    {
                        if (!block.FloorSplitDirectionIsXEqualsY)
                        {
                            aux.LowestFloor = (sbyte)(-room.Position.Y - Math.Min(block.QAFaces[Block.FaceXnZp],
                                                           block.QAFaces[Block.FaceXpZn]));
                        }
                        else
                        {
                            aux.LowestFloor = (sbyte)(-room.Position.Y - Math.Min(block.QAFaces[Block.FaceXpZp],
                                                           block.QAFaces[Block.FaceXnZn]));
                        }
                    }

                    newRoom.AuxSectors[x, z] = aux;
                    newRoom.Sectors[room.NumZSectors * x + z] = sector;
                }
            }
        }

        private void ConvertPortals(Room room, IEnumerable<Portal> tempIdPortals, tr_room newRoom)
        {
            foreach (var portal in tempIdPortals)
            {
                // Choose which block faces will actually matter for the dimensions of this portal
                int[] relevantDirections;
                switch (portal.Direction)
                {
                    case PortalDirection.WallPositiveZ:
                        relevantDirections = new int[] { Block.FaceXnZn, Block.FaceXpZn };
                        break;
                    case PortalDirection.WallPositiveX:
                        relevantDirections = new int[] { Block.FaceXnZn, Block.FaceXnZp };
                        break;
                    case PortalDirection.WallNegativeZ:
                        relevantDirections = new int[] { Block.FaceXpZp, Block.FaceXnZp };
                        break;
                    case PortalDirection.WallNegativeX:
                        relevantDirections = new int[] { Block.FaceXpZp, Block.FaceXpZn };
                        break;
                    case PortalDirection.Floor:
                        relevantDirections = new int[] { 0, 1, 2, 3 };
                        break;
                    case PortalDirection.Ceiling:
                        relevantDirections = new int[] { 0, 1, 2, 3 };
                        break;
                    default:
                        throw new ApplicationException("Unknown PortalDirection");
                }

                // Calculate dimensions of portal
                float yMin = float.MaxValue;
                float yMax = float.MinValue;
                for (int z = portal.Area.Top; z <= portal.Area.Bottom; ++z)
                    for (int x = portal.Area.Left; x <= portal.Area.Right; ++x)
                    {
                        Block block = room.Blocks[x, z];
                        foreach (int relevantDirection in relevantDirections)
                        {
                            float floor = 256.0f * block.QAFaces[relevantDirection] + room.WorldPos.Y;
                            float ceiling = 256.0f * block.WSFaces[relevantDirection] + room.WorldPos.Y;
                            yMin = Math.Min(yMin, Math.Min(floor, ceiling));
                            yMax = Math.Max(yMax, Math.Max(floor, ceiling));
                        }
                    }
                yMin = (float)Math.Floor(yMin);
                yMax = (float)Math.Ceiling(yMax);

                float xMin = portal.Area.X * 1024.0f;
                float xMax = (portal.Area.Right + 1) * 1024.0f;
                float zMin = portal.Area.Y * 1024.0f;
                float zMax = (portal.Area.Bottom + 1) * 1024.0f;

                // Determine normal and portal vertices
                tr_vertex normal;
                tr_vertex[] portalVertices = new tr_vertex[4];
                switch (portal.Direction)
                {
                    case PortalDirection.WallPositiveZ:
                        normal = new tr_vertex(0, 0, -1);
                        portalVertices[0] = new tr_vertex((short)xMin, (short)-yMax, (short)(zMax - 1024));
                        portalVertices[1] = new tr_vertex((short)xMax, (short)-yMax, (short)(zMax - 1024));
                        portalVertices[2] = new tr_vertex((short)xMax, (short)-yMin, (short)(zMax - 1024));
                        portalVertices[3] = new tr_vertex((short)xMin, (short)-yMin, (short)(zMax - 1024));
                        break;
                    case PortalDirection.WallPositiveX:
                        normal = new tr_vertex(-1, 0, 0);
                        portalVertices[0] = new tr_vertex((short)(xMax - 1024), (short)-yMin, (short)zMax);
                        portalVertices[1] = new tr_vertex((short)(xMax - 1024), (short)-yMax, (short)zMax);
                        portalVertices[2] = new tr_vertex((short)(xMax - 1024), (short)-yMax, (short)zMin);
                        portalVertices[3] = new tr_vertex((short)(xMax - 1024), (short)-yMin, (short)zMin);
                        break;
                    case PortalDirection.WallNegativeZ:
                        normal = new tr_vertex(0, 0, 1);
                        portalVertices[0] = new tr_vertex((short)xMax, (short)-yMax, (short)(zMin + 1023));
                        portalVertices[1] = new tr_vertex((short)xMin, (short)-yMax, (short)(zMin + 1023));
                        portalVertices[2] = new tr_vertex((short)xMin, (short)-yMin, (short)(zMin + 1023));
                        portalVertices[3] = new tr_vertex((short)xMax, (short)-yMin, (short)(zMin + 1023));
                        break;
                    case PortalDirection.WallNegativeX:
                        normal = new tr_vertex(1, 0, 0);
                        portalVertices[0] = new tr_vertex((short)(xMin + 1023), (short)-yMin, (short)zMin);
                        portalVertices[1] = new tr_vertex((short)(xMin + 1023), (short)-yMax, (short)zMin);
                        portalVertices[2] = new tr_vertex((short)(xMin + 1023), (short)-yMax, (short)zMax);
                        portalVertices[3] = new tr_vertex((short)(xMin + 1023), (short)-yMin, (short)zMax);
                        break;
                    case PortalDirection.Floor:
                        normal = new tr_vertex(0, -1, 0);
                        portalVertices[0] = new tr_vertex((short)xMax, (short)-yMin, (short)zMin);
                        portalVertices[1] = new tr_vertex((short)xMin, (short)-yMin, (short)zMin);
                        portalVertices[2] = new tr_vertex((short)xMin, (short)-yMin, (short)zMax);
                        portalVertices[3] = new tr_vertex((short)xMax, (short)-yMin, (short)zMax);
                        break;
                    case PortalDirection.Ceiling:
                        normal = new tr_vertex(0, 1, 0);
                        portalVertices[0] = new tr_vertex((short)xMax, (short)-yMax, (short)zMax);
                        portalVertices[1] = new tr_vertex((short)xMin, (short)-yMax, (short)zMax);
                        portalVertices[2] = new tr_vertex((short)xMin, (short)-yMax, (short)zMin);
                        portalVertices[3] = new tr_vertex((short)xMax, (short)-yMax, (short)zMin);
                        break;
                    default:
                        throw new ApplicationException("Unknown PortalDirection");
                }

                // Create portal
                newRoom.Portals.Add(new tr_room_portal
                {
                    AdjoiningRoom = (ushort)_roomsRemappingDictionary[portal.AdjoiningRoom],
                    Vertices = portalVertices,
                    Normal = normal
                });
            }
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

                        var connectedRoomLocalPosUint = new tr_vertex((short)(trVertexPos.X), (short)(trVertexPos.Y), (short)(trVertexPos.Z));

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
