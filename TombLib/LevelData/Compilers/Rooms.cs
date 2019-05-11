using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers
{
    public sealed partial class LevelCompilerClassicTR
    {
        private readonly Dictionary<Room, int> _roomsRemappingDictionary = new Dictionary<Room, int>(new ReferenceEqualityComparer<Room>());
        private readonly List<Room> _roomsUnmapping = new List<Room>();

        private void BuildRooms()
        {
            ReportProgress(15, "Building rooms");

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

            _staticsTable = new Dictionary<StaticInstance, int>(new ReferenceEqualityComparer<StaticInstance>());

            foreach (var room in _roomsRemappingDictionary.Keys)
            {
                _tempRooms.Add(room, BuildRoom(room));
            }

#if DEBUG
            using (var writer = new StreamWriter(File.OpenWrite("Portals.txt")))
            {
                for (int r = 0; r < _tempRooms.Count; r++)
                {
                    var newRoom = _tempRooms.ElementAt(r).Value;

                    writer.WriteLine("---------------------------------------");
                    writer.WriteLine("Room #" + r);
                    writer.WriteLine("---------------------------------------");

                    for (int p = 0; p < newRoom.Portals.Count; p++)
                    {
                        var portal = newRoom.Portals[p];
                        writer.WriteLine("\tPortal #" + p);
                        writer.WriteLine("\t--------------------");
                        writer.WriteLine("\tTo: Room #" + portal.AdjoiningRoom);
                        writer.WriteLine("\tNormal: <" + portal.Normal.X + ", " +
                                                         portal.Normal.Y + ", " +
                                                         portal.Normal.Z + ">");

                        for (int v = 0; v < portal.Vertices.Length; v++)
                        {
                            writer.WriteLine("\t\tVertex: " + v + " <" + portal.Vertices[v].X + ", " +
                                                                         portal.Vertices[v].Y + ", " +
                                                                         portal.Vertices[v].Z + ">");
                        }
                    }
                }
            }
#endif

            ReportProgress(20, "    Number of rooms: " + _roomsUnmapping.Count);

            MatchPortalVertexColors();

            ReportProgress(25, "    Vertex colors on portals matched.");
        }

        private Vector3 CalculateLightForVertex(Room room, Vector3 position, Vector3 normal)
        {
            Vector3 output = room.AmbientLight * 128;

            foreach (var obj in room.Objects)
                if (obj is LightInstance)
                {
                    var light = obj as LightInstance;
                    output += RoomGeometry.CalculateLightForVertex(room, light, position, normal, false);                    
                }

            return Vector3.Max(output, new Vector3()) * (1.0f / 128.0f); ;
        }

        private tr_room BuildRoom(Room room)
        {
            tr_color roomAmbientColor = PackColorTo24Bit(room.AmbientLight);

            if (room.NumXSectors > Room.MaxRecommendedRoomDimensions || room.NumZSectors > Room.MaxRecommendedRoomDimensions)
                _progressReporter.ReportWarn("Room '" + room + "' is very big! Rooms bigger than " + Room.MaxRecommendedRoomDimensions + " sectors per side cause trouble with rendering.");

            var newRoom = new tr_room
            {
                OriginalRoom = room,
                Lights = new List<tr4_room_light>(),
                StaticMeshes = new List<tr_room_staticmesh>(),
                Portals = new List<tr_room_portal>(),
                Info = new tr_room_info
                {
                    X = room.WorldPos.X,
                    Z = room.WorldPos.Z,
                    YTop = (int)-(room.WorldPos.Y + room.GetHighestCorner() * 256.0f),
                    YBottom = (int)-(room.WorldPos.Y + room.GetLowestCorner() * 256.0f)
                },
                NumXSectors = checked((ushort)room.NumXSectors),
                NumZSectors = checked((ushort)room.NumZSectors),
                AlternateRoom = room.Alternated && room.AlternateRoom != null ? (short)_roomsRemappingDictionary[room.AlternateRoom] : (short)-1,
                AlternateGroup = (byte)(room.Alternated /*&& room.AlternateRoom != null*/ ? room.AlternateGroup : 0),
                Flipped = room.Alternated,
                FlippedRoom = room.AlternateRoom,
                BaseRoom = room.AlternateBaseRoom,
                ReverbInfo = (byte)room.Reverberation,
                Flags = 0x40
            };

            // Store ambient intensity
            if (_level.Settings.GameVersion == GameVersion.TR2)
                newRoom.AmbientIntensity = 0xFFF; // TODO: correct ambient light
            else if (_level.Settings.GameVersion == GameVersion.TR3)
                newRoom.AmbientIntensity = PackColorTo16Bit(room.AmbientLight);
            else
                newRoom.AmbientIntensity = ((uint)roomAmbientColor.Red << 16) | ((uint)roomAmbientColor.Green << 8) | roomAmbientColor.Blue;

            // Room flags
            if (room.FlagHorizon)
                newRoom.Flags |= 0x0008;
            if (room.FlagDamage)
                newRoom.Flags |= 0x0010;
            if (room.FlagOutside)
                newRoom.Flags |= 0x0020;
            if (room.FlagNoLensflare)
                newRoom.Flags |= 0x0080;
            if (room.FlagCold)
                newRoom.Flags |= 0x1000;

            // Room type
            switch (room.Type)
            {
                case RoomType.Water:
                    newRoom.Flags |= 0x0001;
                    break;
                case RoomType.Quicksand:
                    newRoom.Flags |= 0x0004;
                    break;
                case RoomType.Rain:
                    newRoom.Flags |= 0x0800;
                    break;
                case RoomType.Snow:
                    newRoom.Flags |= 0x0400;
                    break;
            }

            var lightEffect = room.LightEffect;
            var waterPortals = room.Portals.Where(p => p.Direction == PortalDirection.Floor && p.AdjoiningRoom.Type >= RoomType.Water).ToList();

            // Calculate bottom room-based water scheme, if mode is default or reflection
            if (waterPortals.Count > 0 && room.Type < RoomType.Water && 
                (lightEffect == RoomLightEffect.Default || lightEffect == RoomLightEffect.Reflection))
            {
                var waterRoom = waterPortals.First().AdjoiningRoom;
                int effectiveReflectionLevel = room.LightEffectStrength;
                if (effectiveReflectionLevel <= 0)
                    effectiveReflectionLevel = 2;
                if (effectiveReflectionLevel > 4)
                    effectiveReflectionLevel = 4;

                int effectiveWaterLevel = Math.Min(Math.Max(waterRoom.LightEffectStrength, (byte)1), (byte)4);
                newRoom.WaterScheme = (byte)(effectiveWaterLevel * 4 + effectiveReflectionLevel);
            }
            else
            {
                if (room.Type == RoomType.Water && lightEffect == RoomLightEffect.Default) // Reset water scheme for default water room effect
                    newRoom.WaterScheme = 0;
                else
                    newRoom.WaterScheme = (byte)(room.LightEffectStrength * 5); // Normal calculation
            }

            // Force different effect type 
            if (lightEffect == RoomLightEffect.Default)
            {
                switch (room.Type)
                {
                    case RoomType.Water:
                        lightEffect = RoomLightEffect.Glow;
                        break;
                    case RoomType.Quicksand:
                        lightEffect = RoomLightEffect.Movement;
                        break;
                    default:
                        lightEffect = RoomLightEffect.None;
                        break;
                }
            }

            // Clamp water scheme to maximum possible un-garbaged value for movement effect
            if((lightEffect == RoomLightEffect.Movement || lightEffect == RoomLightEffect.GlowAndMovement) && newRoom.WaterScheme > 12)
                newRoom.WaterScheme = 12;

            // Light effect
            switch (lightEffect)
            {
                case RoomLightEffect.Glow:
                case RoomLightEffect.Movement:
                case RoomLightEffect.GlowAndMovement:
                    newRoom.Flags |= 0x0100;
                    break;

                case RoomLightEffect.Reflection:
                    newRoom.Flags |= 0x0200;
                    break;
            }

            // Generate geometry
            {
                // Add room geometry
                var vertexPositions = room.RoomGeometry.VertexPositions;
                var vertexColors = room.RoomGeometry.VertexColors;

                var roomVerticesDictionary = new Dictionary<tr_room_vertex, ushort>();
                var roomVertices = new List<tr_room_vertex>();

                var roomTriangles = new List<tr_face3>();
                var roomQuads = new List<tr_face4>();

                for (int z = 0; z < room.NumZSectors; ++z)
                    for (int x = 0; x < room.NumXSectors; ++x)
                        for (BlockFace face = 0; face < BlockFace.Count; ++face)
                        {
                            var range = room.RoomGeometry.VertexRangeLookup.TryGetOrDefault(new SectorInfo(x, z, face));
                            var shape = room.GetFaceShape(x, z, face);

                            if (range.Count == 0)
                                continue;

                            TextureArea texture = room.Blocks[x, z].GetFaceTexture(face);
                            if(texture.TextureIsInvisible)
                                continue;

                            if(texture.TextureIsUnavailable)
                            {
                                _progressReporter.ReportWarn("Missing texture at sector (" + x + "," + z + ") in room " + room.Name + ". Check texture file location.");
                                continue;
                            }

                            if((shape == BlockFaceShape.Triangle && texture.TriangleCoordsOutOfBounds) || (shape == BlockFaceShape.Quad && texture.QuadCoordsOutOfBounds))
                            {
                                _progressReporter.ReportWarn("Texture is out of bounds at sector (" + x + "," + z + ") in room " + room.Name + ". Wrong or resized texture file?");
                                continue;
                            }

                            int rangeEnd = range.Start + range.Count;
                            for (int i = range.Start; i < rangeEnd; i += 3)
                            {
                                ushort vertex0Index, vertex1Index, vertex2Index;
                                
                                if(shape == BlockFaceShape.Quad)
                                {
                                    ushort vertex3Index;

                                    if (face == BlockFace.Ceiling)
                                    {
                                        texture.Mirror();
                                        vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 1], vertexColors[i + 1]);
                                        vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 2], vertexColors[i + 2]);
                                        vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 0], vertexColors[i + 0]);
                                        vertex3Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 5], vertexColors[i + 5]);
                                    }
                                    else
                                    {
                                        vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 3], vertexColors[i + 3]);
                                        vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 2], vertexColors[i + 2]);
                                        vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 0], vertexColors[i + 0]);
                                        vertex3Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 1], vertexColors[i + 1]);
                                    }

                                    // lock (_objectTextureManager)
                                    // {
                                    //      result = _objectTextureManager.AddTexturePossiblyAnimated(texture, false, true);
                                    // }

                                    var result = _textureInfoManager.AddTexture(texture, true, false);
                                    roomQuads.Add(result.CreateFace4(new ushort[] { vertex0Index, vertex1Index, vertex2Index, vertex3Index },
                                                    texture.DoubleSided, 0));
                                    i += 3;
                                }
                                else
                                {
                                    if (face == BlockFace.Ceiling || face == BlockFace.CeilingTriangle2)
                                        texture.Mirror(true);

                                    vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 0], vertexColors[i + 0]);
                                    vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 1], vertexColors[i + 1]);
                                    vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 2], vertexColors[i + 2]);

                                    // lock (_objectTextureManager)
                                    // {
                                    //     result = _objectTextureManager.AddTexturePossiblyAnimated(texture, true, true);
                                    // }

                                    var result = _textureInfoManager.AddTexture(texture, true, true);
                                    roomTriangles.Add(result.CreateFace3(new ushort[] { vertex0Index, vertex1Index, vertex2Index },
                                                    texture.DoubleSided, 0));
                                }
                            }
                        }

                // Add geometry imported objects
                int geometryVertexIndexBase = roomVertices.Count;
                foreach (var geometry in room.Objects.OfType<ImportedGeometryInstance>())
                {
                    if (geometry.Model?.DirectXModel == null)
                        continue;

                    var meshes = geometry.Model.DirectXModel.Meshes;
                    var worldTransform = geometry.RotationMatrix *
                                         Matrix4x4.CreateScale(geometry.Scale) *
                                         Matrix4x4.CreateTranslation(geometry.Position);
                    var normalTransform = geometry.RotationMatrix;

                    foreach (var mesh in meshes)
                    {
                        if (!geometry.MeshNameMatchesFilter(mesh.Name))
                            continue;

                        foreach (var submesh in mesh.Submeshes)
                        {
                            for (int j = 0; j < mesh.Vertices.Count; j++)
                            {
                                // Apply the transform to the vertex
                                Vector3 position = MathC.HomogenousTransform(mesh.Vertices[j].Position, worldTransform);
                                Vector3 normal = MathC.HomogenousTransform(mesh.Vertices[j].Normal, normalTransform);
                                normal = Vector3.Normalize(normal);

                                var trVertex = new tr_room_vertex
                                {
                                    Position = new tr_vertex
                                    {
                                        X = (short)position.X,
                                        Y = (short)-(position.Y + room.WorldPos.Y),
                                        Z = (short)position.Z
                                    },
                                    Lighting1 = 0,
                                    Lighting2 = 0,
                                    Attributes = 0
                                };

                                // Pack the light according to chosen lighting model
                                if (geometry.LightingModel == ImportedGeometryLightingModel.NoLighting)
                                    trVertex.Lighting2 = PackColorTo16Bit(room.AmbientLight);
                                else if (geometry.LightingModel == ImportedGeometryLightingModel.VertexColors)
                                    trVertex.Lighting2 = PackColorTo16Bit(mesh.Vertices[j].Color);
                                else
                                    trVertex.Lighting2 = PackColorTo16Bit(CalculateLightForVertex(room, position, normal));

                                // Check for maximum vertices reached
                                if (roomVertices.Count >= 65536)
                                {
                                    throw new Exception("Room '" + room.Name + "' has too many vertices (limit = 65536)! Try to remove some imported geometry objects.");
                                }

                                roomVertices.Add(trVertex);
                            }

                            for (int j = 0; j < submesh.Value.Indices.Count; j += 3)
                            {
                                int numPolygons = roomQuads.Count + roomTriangles.Count;
                                if (_level.Settings.GameVersion != GameVersion.TR5Main && numPolygons > 3000)
                                {
                                    throw new Exception("Room '" + room.Name + "' has too many polygons (count = " + numPolygons + ", limit = 3000)! Try to remove some imported geometry objects.");
                                }

                                var triangle = new tr_face3();

                                ushort index0 = (ushort)(geometryVertexIndexBase + submesh.Value.Indices[j + 0]);
                                ushort index1 = (ushort)(geometryVertexIndexBase + submesh.Value.Indices[j + 1]);
                                ushort index2 = (ushort)(geometryVertexIndexBase + submesh.Value.Indices[j + 2]);

                                triangle.Texture = 20;

                                // TODO Move texture area into the mesh
                                TextureArea texture = new TextureArea();
                                texture.DoubleSided = false;
                                texture.BlendMode = BlendMode.Normal;
                                texture.Texture = submesh.Value.Material.Texture;
                                texture.TexCoord0 = mesh.Vertices[submesh.Value.Indices[j + 0]].UV;
                                texture.TexCoord1 = mesh.Vertices[submesh.Value.Indices[j + 1]].UV;
                                texture.TexCoord2 = mesh.Vertices[submesh.Value.Indices[j + 2]].UV;
                                texture.TexCoord3 = texture.TexCoord2;

                                // TODO: what happens for flipped textures?
                                if (texture.TexCoord0.X < 0.0f) texture.TexCoord0.X = 0.0f;
                                if (texture.TexCoord0.Y < 0.0f) texture.TexCoord0.Y = 0.0f;
                                if (texture.TexCoord1.X < 0.0f) texture.TexCoord1.X = 0.0f;
                                if (texture.TexCoord1.Y < 0.0f) texture.TexCoord1.Y = 0.0f;
                                if (texture.TexCoord2.X < 0.0f) texture.TexCoord2.X = 0.0f;
                                if (texture.TexCoord2.Y < 0.0f) texture.TexCoord2.Y = 0.0f;
                                if (texture.TexCoord3.X < 0.0f) texture.TexCoord3.X = 0.0f;
                                if (texture.TexCoord3.Y < 0.0f) texture.TexCoord3.Y = 0.0f;

                                var result = _textureInfoManager.AddTexture(texture, true, true);
                                roomTriangles.Add(result.CreateFace3(new ushort[] { index0, index1, index2 }, false, 0));
                            }

                            geometryVertexIndexBase += mesh.Vertices.Count;
                        }
                    }
                }

                for (int i = 0; i < roomVertices.Count; ++i)
                {
                    var trVertex = roomVertices[i];
                    ushort flags = 0x0000;

                    ///@FIXME: commented code by Monty for MIST light type
                    //var xv = trVertex.Position.X / 1024;
                    //var zv = trVertex.Position.Z / 1024;
                    // For a better effect (see City of the dead) I don't set this effect to border walls (TODO: tune this)
                    //if (xv > 1 && zv > 1 && xv < room.NumXSectors - 2 && zv < room.NumZSectors - 2)

                    if (lightEffect == RoomLightEffect.Glow ||
                        lightEffect == RoomLightEffect.GlowAndMovement)
                        flags |= 0x4000;

                    bool allowMovement = true;

                    foreach (var portal in room.Portals)
                    {
                        // Assign movement from bottom water or quicksand room
                        if ((waterPortals.Contains(portal) && !portal.PositionOnPortal(new VectorInt3(trVertex.Position.X, trVertex.Position.Y, trVertex.Position.Z), false, true)))
                        {
                            var xv = trVertex.Position.X / 1024;
                            var zv = trVertex.Position.Z / 1024;

                            var connectionInfo1 = room.GetFloorRoomConnectionInfo(new VectorInt2(xv, zv));
                            var connectionInfo2 = room.GetFloorRoomConnectionInfo(new VectorInt2(xv - 1, zv));
                            var connectionInfo3 = room.GetFloorRoomConnectionInfo(new VectorInt2(xv, zv - 1));
                            var connectionInfo4 = room.GetFloorRoomConnectionInfo(new VectorInt2(xv - 1, zv - 1));

                            // A candidate vertex must belong to portal sectors, non triangular, not wall, not solid floor
                            if (connectionInfo1.AnyType != Room.RoomConnectionType.NoPortal &&
                                !room.Blocks[xv, zv].IsAnyWall &&
                                connectionInfo1.TraversableType == Room.RoomConnectionType.FullPortal && 
                                connectionInfo2.AnyType != Room.RoomConnectionType.NoPortal && 
                                !room.Blocks[xv - 1, zv].IsAnyWall && 
                                connectionInfo2.TraversableType == Room.RoomConnectionType.FullPortal && 
                                connectionInfo3.AnyType != Room.RoomConnectionType.NoPortal && 
                                !room.Blocks[xv, zv - 1].IsAnyWall && 
                                connectionInfo3.TraversableType == Room.RoomConnectionType.FullPortal && 
                                connectionInfo4.AnyType != Room.RoomConnectionType.NoPortal && 
                                !room.Blocks[xv - 1, zv - 1].IsAnyWall && 
                                connectionInfo4.TraversableType == Room.RoomConnectionType.FullPortal)
                            {
                                flags |= 0x2000;
                                break;
                            }
                        }

                        // Enable reflection for dry room above water/quicksand room or vice versa
                        if (lightEffect == RoomLightEffect.Reflection && 
                            ((room.Type == RoomType.Water || room.Type == RoomType.Quicksand) != (portal.AdjoiningRoom.Type == RoomType.Water || portal.AdjoiningRoom.Type == RoomType.Quicksand)))
                        {
                            // Assign reflection, if set, for all enclosed portal faces
                            if (portal.PositionOnPortal(new VectorInt3(trVertex.Position.X, trVertex.Position.Y, trVertex.Position.Z), false, false) ||
                                portal.PositionOnPortal(new VectorInt3(trVertex.Position.X, trVertex.Position.Y, trVertex.Position.Z), true, false))
                            {
                                flags |= 0x4000;
                                break;
                            }
                        }
                        // Disable movement for portal faces
                        else 
                        {
                            if (portal.PositionOnPortal(new VectorInt3(trVertex.Position.X, trVertex.Position.Y, trVertex.Position.Z), false, false) ||
                                portal.PositionOnPortal(new VectorInt3(trVertex.Position.X, trVertex.Position.Y, trVertex.Position.Z), true, false))
                                allowMovement = false;
                        }
                    }

                    if (allowMovement && (lightEffect == RoomLightEffect.Movement || lightEffect == RoomLightEffect.GlowAndMovement))
                        flags |= 0x2000;

                    trVertex.Attributes = flags;
                    roomVertices[i] = trVertex;
                }

                newRoom.Vertices = roomVertices;
                newRoom.Quads = roomQuads;
                newRoom.Triangles = roomTriangles;
            }

            // Build portals
            var tempIdPortals = new List<PortalInstance>();

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
            {
                // For TRNG statics chunk
                _staticsTable.Add(instance, newRoom.StaticMeshes.Count);

                newRoom.StaticMeshes.Add(new tr_room_staticmesh
                {
                    X = (int)Math.Round(newRoom.Info.X + instance.Position.X),
                    Y = (int)-Math.Round(room.WorldPos.Y + instance.Position.Y),
                    Z = (int)Math.Round(newRoom.Info.Z + instance.Position.Z),
                    Rotation = (ushort)Math.Max(0, Math.Min(ushort.MaxValue,
                        Math.Round(instance.RotationY * (65536.0 / 360.0)))),
                    ObjectID = checked((ushort)instance.WadObjectId.TypeId),
                    Intensity1 = PackColorTo16Bit(new Vector3(instance.Color.Z, instance.Color.Y, instance.Color.X)),
                    Intensity2 = (ushort)(_level.Settings.GameVersion == GameVersion.TR5 || _level.Settings.GameVersion == GameVersion.TR5Main ? 0x0001 : instance.Ocb)
                });
            }

            ConvertLights(room, newRoom);

            return newRoom;
        }

        private static ushort GetOrAddVertex(Room room, Dictionary<tr_room_vertex, ushort> roomVerticesDictionary, List<tr_room_vertex> roomVertices, Vector3 Position, Vector3 Color)
        {
            tr_room_vertex trVertex;
            trVertex.Position = new tr_vertex
            {
                X = (short)Position.X,
                Y = (short)-(Position.Y + room.WorldPos.Y),
                Z = (short)Position.Z
            };
            trVertex.Lighting1 = 0;
            trVertex.Lighting2 = PackColorTo16Bit(Color);
            trVertex.Attributes = 0;
            trVertex.Normal = new tr_vertex
            {
                X = 0,
                Y = 0,
                Z = 0
            };
            trVertex.Color = PackColorTo32Bit(Color);

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

        private void ConvertLights(Room room, tr_room newRoom)
        {
            int lightCount = 0;

            foreach (var light in room.Objects.OfType<LightInstance>())
            {
                // If target game is <= TR4 then ignore all special lights and fog bulbs
                if (_level.Settings.GameVersion < GameVersion.TR4 &&
                      (light.Type == LightType.Spot || light.Type == LightType.Sun || light.Type == LightType.FogBulb))
                    continue;

                if (!light.Enabled || !light.IsDynamicallyUsed)
                    continue;
                tr_color color = PackColorTo24Bit(light.Color);
                ushort intensity = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, Math.Abs(light.Intensity) * 8192.0f));
                if (intensity == 0 || color.Red == 0 && color.Green == 0 && color.Blue == 0)
                    continue;
                lightCount += 1;

                var newLight = new tr4_room_light
                {
                    X = (int)Math.Round(newRoom.Info.X + light.Position.X),
                    Y = (int)-Math.Round(light.Position.Y + room.WorldPos.Y),
                    Z = (int)Math.Round(newRoom.Info.Z + light.Position.Z),
                    Color = color,
                    Intensity = intensity
                };

                switch (light.Type)
                {
                    case LightType.Point:
                        newLight.LightType = 1;
                        newLight.In = light.InnerRange * 1024.0f;
                        newLight.Out = light.OuterRange * 1024.0f;
                        break;
                    case LightType.Shadow:
                        newLight.LightType = 3;
                        newLight.In = light.InnerRange * 1024.0f;
                        newLight.Out = light.OuterRange * 1024.0f;
                        break;
                    case LightType.Spot:
                        newLight.LightType = 2;
                        newLight.In = (float)Math.Cos(light.InnerAngle * (Math.PI / 180));
                        newLight.Out = (float)Math.Cos(light.OuterAngle * (Math.PI / 180));
                        newLight.Length = light.InnerRange * 1024.0f;
                        newLight.CutOff = light.OuterRange * 1024.0f;
                        Vector3 spotDirection = light.GetDirection();
                        newLight.DirectionX = -spotDirection.X;
                        newLight.DirectionY = spotDirection.Y;
                        newLight.DirectionZ = -spotDirection.Z;
                        break;
                    case LightType.Sun:
                        newLight.LightType = 0;
                        newLight.In = 0;
                        newLight.Out = 0;
                        newLight.Length = 0;
                        newLight.CutOff = 0;
                        Vector3 sunDirection = light.GetDirection();
                        newLight.DirectionX = -sunDirection.X;
                        newLight.DirectionY = sunDirection.Y;
                        newLight.DirectionZ = -sunDirection.Z;
                        break;
                    case LightType.FogBulb:
                        newLight.LightType = 4;
                        newLight.In = light.InnerRange * 1024;
                        newLight.Out = light.OuterRange * 1024;
                        break;
                    case LightType.Effect:
                        continue;
                    default:
                        throw new Exception("Unknown light type '" + light.Type + "' encountered.");
                }

                newRoom.Lights.Add(newLight);
            }

            if (lightCount > 20)
            {
                throw new ApplicationException("Room '" + room + "' has more than 20 dynamic lights (It has " + lightCount + "). This can cause crashes with the original engine!");
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

                    if (_level.Settings.GameVersion >= GameVersion.TR3)
                        sector.BoxIndex = (ushort)(0x7ff0 | (0xf & (int)GetTextureSound(room, x, z)));
                    else
                        sector.BoxIndex = 0xffff;
                    sector.FloorDataIndex = 0;

                    // Setup portals
                    if (room.GetFloorRoomConnectionInfo(new VectorInt2(x, z)).TraversableType != Room.RoomConnectionType.NoPortal)
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

                    if (room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z)).TraversableType != Room.RoomConnectionType.NoPortal)
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
                    if (room.Type != RoomType.Water && (Math.Abs(block.Floor.IfQuadSlopeX) == 1 ||
                                                        Math.Abs(block.Floor.IfQuadSlopeX) == 2 ||
                                                        Math.Abs(block.Floor.IfQuadSlopeZ) == 1 ||
                                                        Math.Abs(block.Floor.IfQuadSlopeZ) == 2))
                        aux.SoftSlope = true;
                    if (room.Type != RoomType.Water && (Math.Abs(block.Floor.IfQuadSlopeX) > 2 || Math.Abs(block.Floor.IfQuadSlopeZ) > 2))
                        aux.HardSlope = true;
                    if (block.Type == BlockType.Wall)
                        aux.Wall = true;

                    // Setup floor heights
                    if (x == 0 || z == 0 || x == room.NumXSectors - 1 || z == room.NumZSectors - 1 ||
                        block.Type == BlockType.BorderWall || block.Type == BlockType.Wall)
                    {
                        sector.Floor = (sbyte)(-room.Position.Y - block.Floor.Max);
                        sector.Ceiling = (sbyte)(-room.Position.Y - block.Ceiling.Min);
                    }
                    else
                    {
                        sector.Floor = (sbyte)(-room.Position.Y - block.Floor.Max);
                        sector.Ceiling = (sbyte)(-room.Position.Y - block.Ceiling.Min);
                    }

                    aux.LowestFloor = (sbyte)(-room.Position.Y - block.Floor.Min);
                    var q0 = block.Floor.XnZp;
                    var q1 = block.Floor.XpZp;
                    var q2 = block.Floor.XpZn;
                    var q3 = block.Floor.XnZn;

                    if (!BlockSurface.IsQuad2(q0, q1, q2, q3) && block.Floor.IfQuadSlopeX == 0 &&
                        block.Floor.IfQuadSlopeZ == 0)
                    {
                        if (!block.Floor.SplitDirectionIsXEqualsZ)
                        {
                            aux.LowestFloor = (sbyte)(-room.Position.Y - Math.Min(block.Floor.XnZp,
                                                           block.Floor.XpZn));
                        }
                        else
                        {
                            aux.LowestFloor = (sbyte)(-room.Position.Y - Math.Min(block.Floor.XpZp,
                                                           block.Floor.XnZn));
                        }
                    }

                    newRoom.AuxSectors[x, z] = aux;
                    newRoom.Sectors[room.NumZSectors * x + z] = sector;
                }
            }
        }

        private void ConvertPortals(Room room, IEnumerable<PortalInstance> portals, tr_room newRoom)
        {
            foreach (var portal in portals)
            {
                switch (portal.Direction)
                {
                    case PortalDirection.WallNegativeZ:
                        ConvertWallPortal(room, portal, newRoom.Portals, new BlockEdge[] { BlockEdge.XnZn, BlockEdge.XpZn },
                            new BlockEdge[] { BlockEdge.XnZp, BlockEdge.XpZp });
                        break;
                    case PortalDirection.WallNegativeX:
                        ConvertWallPortal(room, portal, newRoom.Portals, new BlockEdge[] { BlockEdge.XnZn, BlockEdge.XnZp },
                             new BlockEdge[] { BlockEdge.XpZn, BlockEdge.XpZp });
                        break;
                    case PortalDirection.WallPositiveZ:
                        ConvertWallPortal(room, portal, newRoom.Portals, new BlockEdge[] { BlockEdge.XpZp, BlockEdge.XnZp },
                            new BlockEdge[] { BlockEdge.XpZn, BlockEdge.XnZn });
                        break;
                    case PortalDirection.WallPositiveX:
                        ConvertWallPortal(room, portal, newRoom.Portals, new BlockEdge[] { BlockEdge.XpZp, BlockEdge.XpZn },
                            new BlockEdge[] { BlockEdge.XnZp, BlockEdge.XnZn });
                        break;
                    case PortalDirection.Floor:
                        ConvertFloorCeilingPortal(room, portal, newRoom.Portals, false);
                        break;
                    case PortalDirection.Ceiling:
                        ConvertFloorCeilingPortal(room, portal, newRoom.Portals, true);
                        break;
                    default:
                        throw new ApplicationException("Unknown PortalDirection");
                }
            }
        }

        private void ConvertWallPortal(Room room, PortalInstance portal, List<tr_room_portal> outPortals, BlockEdge[] relevantEdges, BlockEdge[] oppositeRelevantEdges)
        {
            // Calculate dimensions of portal
            var yMin = float.MaxValue;
            var yMax = float.MinValue;

            var startX = 0;
            var endX = 0;
            var startZ = 0;
            var endZ = 0;

            var oppositeStartX = 0;
            var oppositeEndX = 0;
            var oppositeStartZ = 0;
            var oppositeEndZ = 0;

            switch (portal.Direction)
            {
                case PortalDirection.WallNegativeX:
                    startX = 1;
                    endX = 1;
                    startZ = Math.Min(portal.Area.Y0, portal.Area.Y1);
                    endZ = Math.Max(portal.Area.Y0, portal.Area.Y1);

                    oppositeStartX = portal.AdjoiningRoom.NumXSectors - 2;
                    oppositeEndX = portal.AdjoiningRoom.NumXSectors - 2;
                    oppositeStartZ = Math.Min(portal.Area.Y0, portal.Area.Y1) + 
                        portal.Room.Position.Z - portal.AdjoiningRoom.Position.Z;
                    oppositeEndZ = Math.Max(portal.Area.Y0, portal.Area.Y1) +
                        portal.Room.Position.Z - portal.AdjoiningRoom.Position.Z;

                    break;

                case PortalDirection.WallPositiveX:
                    startX = room.NumXSectors - 2;
                    endX = room.NumXSectors - 2;
                    startZ = Math.Min(portal.Area.Y0, portal.Area.Y1);
                    endZ = Math.Max(portal.Area.Y0, portal.Area.Y1);

                    oppositeStartX = 1;
                    oppositeEndX = 1;
                    oppositeStartZ = Math.Min(portal.Area.Y0, portal.Area.Y1) +
                        portal.Room.Position.Z - portal.AdjoiningRoom.Position.Z;
                    oppositeEndZ = Math.Max(portal.Area.Y0, portal.Area.Y1) +
                        portal.Room.Position.Z - portal.AdjoiningRoom.Position.Z;

                    break;

                case PortalDirection.WallNegativeZ:
                    startX = Math.Min(portal.Area.X0, portal.Area.X1);
                    endX = Math.Max(portal.Area.X0, portal.Area.X1);
                    startZ = 1;
                    endZ = 1;

                    oppositeStartX = Math.Min(portal.Area.X0, portal.Area.X1) +
                        portal.Room.Position.X - portal.AdjoiningRoom.Position.X;
                    oppositeEndX = Math.Max(portal.Area.X0, portal.Area.X1) +
                        portal.Room.Position.X - portal.AdjoiningRoom.Position.X;
                    oppositeStartZ = portal.AdjoiningRoom.NumZSectors - 2;
                    oppositeEndZ = portal.AdjoiningRoom.NumZSectors - 2;

                    break;

                case PortalDirection.WallPositiveZ:
                    startX = Math.Min(portal.Area.X0, portal.Area.X1);
                    endX = Math.Max(portal.Area.X0, portal.Area.X1);
                    startZ = room.NumZSectors - 2;
                    endZ = room.NumZSectors - 2;

                    oppositeStartX = Math.Min(portal.Area.X0, portal.Area.X1) +
                       portal.Room.Position.X - portal.AdjoiningRoom.Position.X;
                    oppositeEndX = Math.Max(portal.Area.X0, portal.Area.X1) +
                        portal.Room.Position.X - portal.AdjoiningRoom.Position.X;
                    oppositeStartZ = 1;
                    oppositeEndZ = 1;

                    break;
            }

            for (var z = 0; z <= endZ - startZ; ++z)
                for (var x = 0; x <= endX - startX; ++x)
                {
                    Block block = room.Blocks[x + startX, z + startZ];
                    Block oppositeBlock = portal.AdjoiningRoom.Blocks[x + oppositeStartX, z + oppositeStartZ];

                    for (int i = 0; i < relevantEdges.Length; i++)
                    {
                        // We need to check both the current block and the opposite block and choose 
                        // the largest area possible for avoiding strange rendering bugs
                        var relevantDirection = relevantEdges[i];
                        var oppositeRelevantDirection = oppositeRelevantEdges[i];

                        var floor = 256.0f * block.Floor.GetHeight(relevantDirection) + room.WorldPos.Y;
                        var ceiling = 256.0f * block.Ceiling.GetHeight(relevantDirection) + room.WorldPos.Y;

                        var floorOpposite = 256.0f * oppositeBlock.Floor.GetHeight(oppositeRelevantDirection) + portal.AdjoiningRoom.WorldPos.Y;
                        var ceilingOpposite = 256.0f * oppositeBlock.Ceiling.GetHeight(oppositeRelevantDirection) + portal.AdjoiningRoom.WorldPos.Y;

                        floor = Math.Min(floor, floorOpposite);
                        ceiling = Math.Max(ceiling, ceilingOpposite);

                        yMin = Math.Min(yMin, Math.Min(floor, ceiling));
                        yMax = Math.Max(yMax, Math.Max(floor, ceiling));
                    }
                }
            yMin = (float)Math.Floor(yMin);
            yMax = (float)Math.Ceiling(yMax);

            var xMin = portal.Area.X0 * 1024.0f;
            var xMax = (portal.Area.X1 + 1) * 1024.0f;
            var zMin = portal.Area.Y0 * 1024.0f;
            var zMax = (portal.Area.Y1 + 1) * 1024.0f;

            // Determine normal and portal vertices
            tr_vertex[] portalVertices = new tr_vertex[4];
            tr_vertex normal;
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
                default:
                    throw new ApplicationException("Unknown PortalDirection");
            }

            // Create portal
            outPortals.Add(new tr_room_portal
            {
                AdjoiningRoom = (ushort)_roomsRemappingDictionary[portal.AdjoiningRoom],
                Vertices = portalVertices,
                Normal = normal
            });
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct PortalPlane
        {
            public readonly short SlopeX;
            public readonly short SlopeZ;
            public readonly int Height;

            public int EvaluateHeight(int x, int z)
            {
                return Height + x * SlopeX + z * SlopeZ;
            }

            public PortalPlane(int ankerX, short ankerY, int ankerZ, int slopeX, int slopeZ)
            {
                SlopeX = (short)slopeX;
                SlopeZ = (short)slopeZ;
                Height = ankerY - ankerX * slopeX - ankerZ * slopeZ;
            }

            public static unsafe bool FastEquals(PortalPlane first, PortalPlane second)
            {
                return *(ulong*)&first == *(ulong*)&second;
            }
        }

        private void AddPortalPlane(List<PortalPlane> portalPlanes, List<RectangleInt2> portalAreas, int x, int z, PortalPlane portalPlane)
        {
            // Try to extend an existing portal plane
            for (int i = 0; i < portalPlanes.Count; ++i)
                if (PortalPlane.FastEquals(portalPlanes[i], portalPlane))
                {
                    var area = portalAreas[i];
                    area.X0 = Math.Min(area.X0, x);
                    area.X1 = Math.Max(area.X1, x);
                    area.Y0 = Math.Min(area.Y0, z);
                    area.Y1 = Math.Max(area.Y1, z);
                    portalAreas[i] = area;
                    return;
                }

            // Add new portal plane
            portalPlanes.Add(portalPlane);
            portalAreas.Add(new RectangleInt2(x, z, x, z));
        }

        private void ConvertFloorCeilingPortal(Room room, PortalInstance portal, List<tr_room_portal> outPortals, bool isCeiling)
        {
            // Construct planes that contain all portal sectors
            List<PortalPlane> portalPlanes = new List<PortalPlane>();
            List<RectangleInt2> portalAreas = new List<RectangleInt2>();
            for (int z = portal.Area.Y0; z <= portal.Area.Y1; ++z)
                for (int x = portal.Area.X0; x <= portal.Area.X1; ++x)
                {
                    Block block = room.Blocks[x, z];
                    Room.RoomConnectionInfo roomConnectionInfo = isCeiling ?
                        room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z)) :
                        room.GetFloorRoomConnectionInfo(new VectorInt2(x, z));

                    if (roomConnectionInfo.AnyType != Room.RoomConnectionType.NoPortal)
                    {
                        BlockSurface s = isCeiling ? block.Ceiling : block.Floor;
                        if (BlockSurface.IsQuad2(s.XnZn, s.XpZn, s.XnZp, s.XpZp))
                        { // Diagonal is split, one face
                            AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XnZn, z, s.XpZn - s.XnZn, s.XnZp - s.XnZn));
                        }
                        else if (isCeiling ? block.Ceiling.SplitDirectionIsXEqualsZ : block.Floor.SplitDirectionIsXEqualsZ)
                        { // Diagonal is split X = Y
                            if (roomConnectionInfo.AnyType == Room.RoomConnectionType.FullPortal || roomConnectionInfo.AnyType == Room.RoomConnectionType.TriangularPortalXnZp)
                                AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XnZn, z, s.XpZp - s.XnZp, s.XnZp - s.XnZn));
                            if (roomConnectionInfo.AnyType == Room.RoomConnectionType.FullPortal || roomConnectionInfo.AnyType == Room.RoomConnectionType.TriangularPortalXpZn)
                                AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XnZn, z, s.XpZn - s.XnZn, s.XpZp - s.XpZn));
                        }
                        else
                        { // Diagonal is split X = -Y
                            if (roomConnectionInfo.AnyType == Room.RoomConnectionType.FullPortal || roomConnectionInfo.AnyType == Room.RoomConnectionType.TriangularPortalXnZn)
                                AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XnZn, z, s.XpZn - s.XnZn, s.XnZp - s.XnZn));
                            if (roomConnectionInfo.AnyType == Room.RoomConnectionType.FullPortal || roomConnectionInfo.AnyType == Room.RoomConnectionType.TriangularPortalXpZp)
                                AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x + 1, s.XpZp, z + 1, s.XpZp - s.XnZp, s.XpZp - s.XpZn));
                        }
                    }
                }

            // Add portals for all planes in the portal
            for (int i = 0; i < portalPlanes.Count; ++i)
            {
                PortalPlane portalPlane = portalPlanes[i];
                RectangleInt2 portalArea = portalAreas[i];

                float xMin = portalArea.X0 * 1024.0f;
                float xMax = (portalArea.X1 + 1) * 1024.0f;
                float zMin = portalArea.Y0 * 1024.0f;
                float zMax = (portalArea.Y1 + 1) * 1024.0f;

                float yAtXMinZMin = (room.Position.Y + portalPlane.EvaluateHeight(portalArea.X0, portalArea.Y0)) * 256;
                float yAtXMaxZMin = (room.Position.Y + portalPlane.EvaluateHeight(portalArea.X1 + 1, portalArea.Y0)) * 256;
                float yAtXMinZMax = (room.Position.Y + portalPlane.EvaluateHeight(portalArea.X0, portalArea.Y1 + 1)) * 256;
                float yAtXMaxZMax = (room.Position.Y + portalPlane.EvaluateHeight(portalArea.X1 + 1, portalArea.Y1 + 1)) * 256;

                // Choose portal coordinates
                tr_vertex[] portalVertices = new tr_vertex[4];
                tr_vertex normal = new tr_vertex((short)-portalPlane.SlopeX, 4, (short)-portalPlane.SlopeZ);
                if (isCeiling)
                {
                    normal = new tr_vertex(portalPlane.SlopeX, 4, portalPlane.SlopeZ);

                    // HACK: this prevents flickering when camera is exactly on the portal
                    var n = new Vector3(normal.X, normal.Y, normal.Z);
                    if (normal.X < 0.0f)
                        n.X = -1;
                    if (normal.X == 0.0f)
                        n.X = 0;
                    if (normal.X > 0.0f)
                        n.X = 1;
                    if (normal.Y < 0.0f)
                        n.Y = -1;
                    if (normal.Y == 0.0f)
                        n.Y = 0;
                    if (normal.Y > 0.0f)
                        n.Y = 1;
                    if (normal.Z < 0.0f)
                        n.Z = -1;
                    if (normal.Z == 0.0f)
                        n.Z = 0;
                    if (normal.Z > 0.0f)
                        n.Z = 1;

                    //if (yAtXMaxZMin < 0.0f) n.Y = -n.Y;

                    portalVertices[0] = new tr_vertex((short)(xMax + n.X), (short)(-yAtXMaxZMin - n.Y), (short)(zMin + n.Z));
                    portalVertices[1] = new tr_vertex((short)(xMin + n.X), (short)(-yAtXMinZMin - n.Y), (short)(zMin + n.Z));
                    portalVertices[2] = new tr_vertex((short)(xMin + n.X), (short)(-yAtXMinZMax - n.Y), (short)(zMax + n.Z));
                    portalVertices[3] = new tr_vertex((short)(xMax + n.X), (short)(-yAtXMaxZMax - n.Y), (short)(zMax + n.Z));
                }
                else
                {
                    normal = new tr_vertex((short)-portalPlane.SlopeX, -4, (short)-portalPlane.SlopeZ);

                    portalVertices[0] = new tr_vertex((short)xMax, (short)(-yAtXMaxZMax), (short)zMax);
                    portalVertices[1] = new tr_vertex((short)xMin, (short)(-yAtXMinZMax), (short)zMax);
                    portalVertices[2] = new tr_vertex((short)xMin, (short)(-yAtXMinZMin), (short)zMin);
                    portalVertices[3] = new tr_vertex((short)xMax, (short)(-yAtXMaxZMin), (short)zMin);
                }

                // Make the normal vector as short as possible
                while (normal.X % 2 == 0 && normal.Y % 2 == 0 && normal.Z % 2 == 0)
                    normal = new tr_vertex((short)(normal.X / 2), (short)(normal.Y / 2), (short)(normal.Z / 2));

                // Add portal
                outPortals.Add(new tr_room_portal
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
            var vertexColorLookups = new Dictionary<Room, Dictionary<tr_vertex, ushort>>(new ReferenceEqualityComparer<Room>());
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

                        var connectedRoomLocalPosUint = new tr_vertex((short)trVertexPos.X, (short)trVertexPos.Y, (short)trVertexPos.Z);

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
                        Vector3 averageColor = new Vector3(R, G, B) * (1.0f / 16.0f / Count);
                        vertex.Lighting2 = PackColorTo16Bit(averageColor);
                        vertices[i] = vertex;
                    }
                }
            });
        }

        private static void FindConnectedRooms(List<Room> outSharedRooms, Room currentRoom, Vector3 worldPos, bool checkFloorCeiling)
        {
            if (outSharedRooms.Contains(currentRoom))
                return;
            outSharedRooms.Add(currentRoom);

            Vector3 localPos = worldPos - currentRoom.WorldPos;
            int sectorPosX = (int)(localPos.X * (1.0f / 1024.0f) + 0.5f);
            int sectorPosZ = (int)(localPos.Z * (1.0f / 1024.0f) + 0.5f);
            int sectorPosX2 = sectorPosX - 1;
            int sectorPosZ2 = sectorPosZ - 1;

            // Check up to 4 sectors around the destination
            if (sectorPosX >= 0 && sectorPosX < currentRoom.NumXSectors)
            {
                if (sectorPosZ >= 0 && sectorPosZ < currentRoom.NumZSectors)
                    FindConnectedRoomsCheckSector(outSharedRooms, currentRoom, worldPos, checkFloorCeiling, sectorPosX, sectorPosZ);
                if (sectorPosZ2 >= 0 && sectorPosZ2 < currentRoom.NumZSectors)
                    FindConnectedRoomsCheckSector(outSharedRooms, currentRoom, worldPos, checkFloorCeiling, sectorPosX, sectorPosZ2);
            }
            if (sectorPosX2 >= 0 && sectorPosX2 < currentRoom.NumXSectors)
            {
                if (sectorPosZ >= 0 && sectorPosZ < currentRoom.NumZSectors)
                    FindConnectedRoomsCheckSector(outSharedRooms, currentRoom, worldPos, checkFloorCeiling, sectorPosX2, sectorPosZ);
                if (sectorPosZ2 >= 0 && sectorPosZ2 < currentRoom.NumZSectors)
                    FindConnectedRoomsCheckSector(outSharedRooms, currentRoom, worldPos, checkFloorCeiling, sectorPosX2, sectorPosZ2);
            }
        }

        private static void FindConnectedRoomsCheckSector(List<Room> outSharedRooms, Room currentRoom, Vector3 worldPos, bool checkFloorCeiling, int x, int z)
        {
            Block block = currentRoom.Blocks[x, z];

            if (checkFloorCeiling)
            {
                if (block.FloorPortal != null)
                    foreach (var roomPair in Room.GetPossibleAlternateRoomPairs(currentRoom, block.FloorPortal.AdjoiningRoom))
                    {
                        if (roomPair.Key != currentRoom)
                            FindConnectedRooms(outSharedRooms, roomPair.Key, worldPos, checkFloorCeiling);
                        FindConnectedRooms(outSharedRooms, roomPair.Value, worldPos, false);
                    }
                if (block.CeilingPortal != null)
                    foreach (var roomPair in Room.GetPossibleAlternateRoomPairs(currentRoom, block.CeilingPortal.AdjoiningRoom))
                    {
                        if (roomPair.Key != currentRoom)
                            FindConnectedRooms(outSharedRooms, roomPair.Key, worldPos, checkFloorCeiling);
                        FindConnectedRooms(outSharedRooms, roomPair.Value, worldPos, false);
                    }
            }

            if (block.WallPortal != null)
                foreach (var roomPair in Room.GetPossibleAlternateRoomPairs(currentRoom, block.WallPortal.AdjoiningRoom))
                {
                    if (roomPair.Key != currentRoom)
                        FindConnectedRooms(outSharedRooms, roomPair.Key, worldPos, checkFloorCeiling);
                    FindConnectedRooms(outSharedRooms, roomPair.Value, worldPos, checkFloorCeiling);
                }
        }

        private static ushort PackColorTo16Bit(Vector3 color)
        {
            color *= 16.0f;
            color += new Vector3(0.5f); // Round correctly
            color = Vector3.Min(new Vector3(31), Vector3.Max(new Vector3(0), color));

            ushort tmp = 0;
            tmp |= (ushort)((ushort)color.X << 10);
            tmp |= (ushort)((ushort)color.Y << 5);
            tmp |= (ushort)color.Z;
            return tmp;
        }

        private static uint PackColorTo32Bit(Vector3 color)
        {
            color *= 128.0f;
            color += new Vector3(0.5f); // Round correctly
            color = Vector3.Min(new Vector3(255), Vector3.Max(new Vector3(0), color));

            uint result = (uint)(0xff000000 + ((byte)color.X << 16) + ((byte)color.Y << 8) + (byte)color.Z);
            return result;
        }

        private static tr_color PackColorTo24Bit(Vector3 color)
        {
            color *= 128.0f;
            color += new Vector3(0.5f); // Round correctly
            color = Vector3.Min(new Vector3(255), Vector3.Max(new Vector3(0), color));

            tr_color result;
            result.Red = (byte)color.X;
            result.Green = (byte)color.Y;
            result.Blue = (byte)color.Z;
            return result;
        }
    }
}
