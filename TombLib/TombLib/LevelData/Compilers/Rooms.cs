using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.Compilers
{
    public sealed partial class LevelCompilerClassicTR
    {
        private readonly Dictionary<Room, int> _roomsRemappingDictionary = new Dictionary<Room, int>(new ReferenceEqualityComparer<Room>());
        private readonly List<Room> _roomsUnmapping = new List<Room>();
        private Dictionary<ShadeMatchSignature, uint> _vertexColors;

        private void BuildRooms()
        {
            ReportProgress(5, "Lighting Rooms");
            Parallel.ForEach<Room>(_sortedRooms.Where(r => r != null), (room) => {
                room.RebuildLighting(!_level.Settings.FastMode);
            });

            ReportProgress(15, "Building rooms");

            foreach (var room in _sortedRooms.Where(r => r != null))
            {
                _roomsRemappingDictionary.Add(room, _roomsUnmapping.Count);
                _roomsUnmapping.Add(room);
            }

            _staticsTable = new Dictionary<StaticInstance, int>(new ReferenceEqualityComparer<StaticInstance>());

            foreach (var room in _roomsRemappingDictionary.Keys)
                _tempRooms.Add(room, BuildRoom(room));

            // Remove WaterScheme values for water rooms
            Parallel.ForEach(_tempRooms.Values, (tr_room trRoom) => { if ((trRoom.Flags & 0x0001) != 0) trRoom.WaterScheme = 0; });

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

            if (!_level.Settings.FastMode)
            {
                ReportProgress(23, "    Matching vertex colors on portals...");

                _vertexColors = new Dictionary<ShadeMatchSignature, uint>();
                var rooms = _tempRooms.Values.ToList();
                for (int flipped = 0; flipped <= 1; flipped++)
                    foreach (var room in rooms)
                        MatchDoorShades(rooms, room, (_level.Settings.GameVersion < TRVersion.Game.TR3), flipped == 1);
            }

            Parallel.ForEach(_tempRooms.Values, (tr_room trRoom) =>
            {
                for (int i = 0; i < trRoom.Vertices.Count; i++)
                {
                    var v = trRoom.Vertices[i];
                    if (!v.IsOnPortal)
                        continue;

                    var sig = new ShadeMatchSignature()
                    {
                        IsWater = ((trRoom.Flags & 1) == 1),
                        AlternateGroup = trRoom.AlternateKind == AlternateKind.AlternateRoom ? trRoom.AlternateGroup : -1,
                        Position = new VectorInt3(trRoom.Info.X + v.Position.X, v.Position.Y, trRoom.Info.Z + v.Position.Z)
                    };

                    if (_level.Settings.GameVersion != TRVersion.Game.TR5)
                    {
                        if (_vertexColors.ContainsKey(sig))
                        {
                            v.Lighting1 = (ushort)_vertexColors[sig];
                            v.Lighting2 = (ushort)_vertexColors[sig];
                            trRoom.Vertices[i] = v;
                        }
                    }
                    else
                    {
                        if (_vertexColors.ContainsKey(sig))
                        {
                            v.Color = _vertexColors[sig];
                            trRoom.Vertices[i] = v;
                        }
                    }
                }
            });

            ReportProgress(25, "    Vertex colors on portals matched.");
        }

        private Vector3 CalculateLightForCustomVertex(Room room, Vector3 position, Vector3 normal, bool forImportedGeometry, Vector3 ambientColor)
        {
            Vector3 output = ambientColor;

            if (position.X >= 0 && position.Z >= 0 &&
                position.X < room.NumXSectors * Level.WorldUnit && position.Z < room.NumZSectors * Level.WorldUnit)
                foreach (var obj in room.Objects)
                    if (obj is LightInstance)
                    {
                        var light = obj as LightInstance;

                        // Disable this light for imported geometry, if IsUsedForImportedGeometry flag is not set,
                        // or for static meshes, if IsStaticallyUsed is not set

                        if ((!light.IsUsedForImportedGeometry && forImportedGeometry) ||
                            (!light.IsStaticallyUsed && !forImportedGeometry))
                            continue;

                        output += RoomGeometry.CalculateLightForVertex(room, light, position, normal, false, false);
                    }

            return Vector3.Max(output, new Vector3()) * (1.0f / 128.0f); ;
        }

        private tr_room BuildRoom(Room room)
        {
            tr_color roomAmbientColor = PackColorTo24Bit(room.Properties.AmbientLight);

            int maxDimensions = _limits[Limit.RoomDimensions];
            if (room.NumXSectors >= maxDimensions || room.NumZSectors >= maxDimensions)
                _progressReporter.ReportWarn("Room '" + room + "' is very big! Rooms bigger than " + maxDimensions + " sectors per side may cause trouble with rendering.");

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
                    YTop = (int)-(room.WorldPos.Y + room.GetHighestCorner() * Level.QuarterWorldUnit),
                    YBottom = (int)-(room.WorldPos.Y + room.GetLowestCorner() * Level.QuarterWorldUnit)
                },
                NumXSectors = checked((ushort)room.NumXSectors),
                NumZSectors = checked((ushort)room.NumZSectors),
                AlternateRoom = room.Alternated && room.AlternateRoom != null ? (short)_roomsRemappingDictionary[room.AlternateRoom] : (short)-1,
                AlternateGroup = (byte)(room.Alternated /*&& room.AlternateRoom != null*/ ? room.AlternateGroup : 0),
                Flipped = room.Alternated,
                FlippedRoom = room.AlternateRoom,
                BaseRoom = room.AlternateBaseRoom,
                ReverbInfo = room.Properties.Reverberation,
                Flags = 0x40
            };

            if (!room.Alternated)
                newRoom.AlternateKind = AlternateKind.NotAlternated;
            else if (room.AlternateBaseRoom != null)
                newRoom.AlternateKind = AlternateKind.AlternateRoom;
            else if (room.AlternateRoom != null)
                newRoom.AlternateKind = AlternateKind.BaseRoom;

            // Store ambient intensity
            if (_level.Settings.GameVersion <= TRVersion.Game.TR3)
                newRoom.AmbientIntensity = PackColorTo13BitGreyscale(room.Properties.AmbientLight);
            else
                newRoom.AmbientIntensity = ((uint)roomAmbientColor.Red << 16) | 
                                           ((uint)roomAmbientColor.Green << 8) | 
                                                  roomAmbientColor.Blue;

            // Properly identify game version to swap light mode, quicksand and no lensflare flags
            bool isTR2  = room.Level.Settings.GameVersion == TRVersion.Game.TR2;
            bool isTR23 = isTR2 || room.Level.Settings.GameVersion == TRVersion.Game.TR3;
            bool isNL   = room.Level.Settings.GameVersion.Legacy() >= TRVersion.Game.TR4;
            bool isNG   = room.Level.IsNG;

            // Room flags
            if (room.Properties.FlagHorizon)
                newRoom.Flags |= 0x0008;
            if (room.Properties.FlagOutside)
                newRoom.Flags |= 0x0020;

            // TRNG-specific flags
            if (isNG && room.Properties.FlagDamage)
                newRoom.Flags |= 0x0010;
            if (isNG && room.Properties.FlagCold)
                newRoom.Flags |= 0x1000;
            if (isNL && room.Properties.FlagNoLensflare)
                newRoom.Flags |= 0x0080;

            // Room type
            switch (room.Properties.Type)
            {
                case RoomType.Water:
                    newRoom.Flags |= 0x0001;
                    break;
                case RoomType.Quicksand:
                    if (isNL)
                        newRoom.Flags |= 0x0004;
                    else
                        newRoom.Flags |= 0x0080;
                    break;
                case RoomType.Rain:
                    if (isNG) newRoom.Flags |= 0x0800;
                    break;
                case RoomType.Snow:
                    if (isNG) newRoom.Flags |= 0x0400;
                    break;
            }

            var lightEffect = room.Properties.LightEffect;
            var waterPortals = room.Portals.Where(p => p.Direction == PortalDirection.Floor && p.AdjoiningRoom.Properties.Type >= RoomType.Water).ToList();

            bool waterSchemeSet = false;

            // Calculate bottom room-based water scheme in advance, if mode is default, mist or reflection
            if (waterPortals.Count > 0 && room.Properties.Type < RoomType.Water &&
                (lightEffect == RoomLightEffect.Default || lightEffect == RoomLightEffect.Reflection || lightEffect == RoomLightEffect.Mist))
            {
                var waterRoom = waterPortals.First().AdjoiningRoom;
                newRoom.WaterScheme = (byte)((waterRoom.Properties.LightEffectStrength * 4) + room.Properties.LightEffectStrength);
                waterSchemeSet = true;
            }

            // Force different effect type 
            if (lightEffect == RoomLightEffect.Default)
            {
                switch (room.Properties.Type)
                {
                    case RoomType.Water:
                        if (!isTR2)
                            lightEffect = RoomLightEffect.Glow; // TR2 does water glowing automatically
                        break;
                    case RoomType.Quicksand:
                        lightEffect = RoomLightEffect.Movement;
                        break;
                    default:
                        lightEffect = RoomLightEffect.None;
                        break;
                }
            }

            // Light effect
            // WARNING: DO NOT use raw value of 1 in WaterScheme ever with classic tomb engines,
            // it will result in broken effect.

            switch (lightEffect)
            {
                case RoomLightEffect.GlowAndMovement:
                    if (!waterSchemeSet) newRoom.WaterScheme = (byte)(room.Properties.LightEffectStrength * 5.0f);
                    newRoom.LightMode = 3; // Used in TR2 only
                    newRoom.Flags |= 0x0100;
                    break;

                case RoomLightEffect.Movement:
                    if (!waterSchemeSet) newRoom.WaterScheme = (byte)(room.Properties.LightEffectStrength * 5.0f);
                    newRoom.LightMode = 1; // Used in TR2 only
                    break;

                case RoomLightEffect.Glow:
                case RoomLightEffect.Mist:
                    if (!waterSchemeSet) newRoom.WaterScheme = (byte)(room.Properties.LightEffectStrength == 0 ? 0 : room.Properties.LightEffectStrength + 1);
                    newRoom.Flags |= 0x0100;
                    newRoom.LightMode = 2; // Used in TR2 only
                    break;

                case RoomLightEffect.Reflection:
                    newRoom.Flags |= 0x0200;
                    newRoom.LightMode = 2; // Used in TR2 only
                    break;

                case RoomLightEffect.None:
                    if (!waterSchemeSet)
                        newRoom.WaterScheme = (byte)(room.Properties.LightEffectStrength * 5.0f);
                    break;
            }

            // Light interpolation mode
            var interpMode = room.Properties.LightInterpolationMode;
            if (interpMode == RoomLightInterpolationMode.Default)
            {
                switch (room.Properties.Type)
                {
                    case RoomType.Water:
                        interpMode = RoomLightInterpolationMode.NoInterpolate;
                        break;

                    default:
                        interpMode = RoomLightInterpolationMode.Interpolate;
                        break;
                }
            }

            // Generate geometry
            {
                // Add room geometry

                var vertexPositions = room.RoomGeometry.VertexPositions;
                var vertexColors = room.RoomGeometry.VertexColors;

                var roomVerticesDictionary = new Dictionary<int, ushort>();
                var roomVertices = new List<tr_room_vertex>();

                var roomTriangles = new List<tr_face3>();
                var roomQuads = new List<tr_face4>();

                // Add room's own geometry

                if (!room.Properties.Hidden)
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

                                var doubleSided = _level.Settings.GameVersion >  TRVersion.Game.TR2 && texture.DoubleSided;
                                var copyFace    = _level.Settings.GameVersion <= TRVersion.Game.TR2 && texture.DoubleSided;

                                int rangeEnd = range.Start + range.Count;
                                for (int i = range.Start; i < rangeEnd; i += 3)
                                {
                                    ushort vertex0Index, vertex1Index, vertex2Index;
                                
                                    if (shape == BlockFaceShape.Quad)
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
                                    
                                        var result = _textureInfoManager.AddTexture(texture, true, false);
                                        roomQuads.Add(result.CreateFace4(new ushort[] { vertex0Index, vertex1Index, vertex2Index, vertex3Index },
                                                        doubleSided, 0));
                                        if (copyFace)
                                        {
                                            texture.Mirror();
                                            result = _textureInfoManager.AddTexture(texture, true, false);
                                            roomQuads.Add(result.CreateFace4(new ushort[] { vertex3Index, vertex2Index, vertex1Index, vertex0Index },
                                                            doubleSided, 0));
                                        }
                                        i += 3;
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Ceiling || face == BlockFace.CeilingTriangle2)
                                            texture.Mirror(true);

                                        vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 0], vertexColors[i + 0]);
                                        vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 1], vertexColors[i + 1]);
                                        vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 2], vertexColors[i + 2]);
                                    
                                        var result = _textureInfoManager.AddTexture(texture, true, true);

                                        if (result.ConvertToQuad)
                                            roomQuads.Add(result.CreateFace4(new ushort[] { vertex0Index, vertex1Index, vertex2Index, vertex2Index },
                                                            doubleSided, 0));
                                        else
                                            roomTriangles.Add(result.CreateFace3(new ushort[] { vertex0Index, vertex1Index, vertex2Index },
                                                            doubleSided, 0));

                                        if (copyFace)
                                        {
                                            texture.Mirror();
                                            result = _textureInfoManager.AddTexture(texture, true, true);
                                            roomTriangles.Add(result.CreateFace3(new ushort[] { vertex2Index, vertex1Index, vertex0Index },
                                                            doubleSided, 0));
                                        }
                                    }
                                }
                            }

                // Merge static meshes

                if (!_level.Settings.FastMode)
                foreach (var staticMesh in room.Objects.OfType<StaticInstance>())
                {
                    // Сheck if static Mesh is in the Auto Merge list
                    var entry = _level.Settings.AutoStaticMeshMerges.Find((mergeEntry) =>
                        mergeEntry.meshId == staticMesh.WadObjectId.TypeId);

                    if (entry == null)
                        continue;

                    bool interpretShadesAsEffect = entry.InterpretShadesAsEffect;
                    bool clearShades = entry.ClearShades;
                    int meshVertexBase = roomVertices.Count;
                    var worldTransform = staticMesh.RotationMatrix *
                                         Matrix4x4.CreateTranslation(staticMesh.Position);
                    var normalTransform = staticMesh.RotationMatrix;
                    WadStatic wadStatic = _level.Settings.WadTryGetStatic(staticMesh.WadObjectId);

                    for (int j = 0; j < wadStatic.Mesh.VertexPositions.Count; j++)
                    {
                        // Apply the transform to the vertex
                        Vector3 position = MathC.HomogenousTransform(wadStatic.Mesh.VertexPositions[j], worldTransform);
                        Vector3 normal = MathC.HomogenousTransform(wadStatic.Mesh.VertexNormals[j], normalTransform);
                        normal = Vector3.Normalize(normal);
                        int lightingEffect = 0;
                        float shade = 1.0f;
                        if (interpretShadesAsEffect &&
                            _level.Settings.GameVersion >= TRVersion.Game.TR3)
                        {
                            if (j < wadStatic.Mesh.VertexColors.Count)
                            {
                                var luma = wadStatic.Mesh.VertexColors[j].GetLuma();
                                if (luma < 0.5f) lightingEffect = 0x2000;   // Movement
                                else if (luma < 1.0f) lightingEffect = 0x4000; // Glow
                            }
                        }
                        else
                        {
                            // If we have vertex colors, use them as a luma factor for the resulting vertex color
                            if (!clearShades && j < wadStatic.Mesh.VertexColors.Count)
                                shade = wadStatic.Mesh.VertexColors[j].GetLuma();
                        }
                        Vector3 color;
                        if (!entry.TintAsAmbient)
                        {
                            color = CalculateLightForCustomVertex(room, position, normal, false, room.Properties.AmbientLight * 128);
                            // Apply Shade factor
                            color *= shade;
                            // Apply Instance Color
                            color *= staticMesh.Color;
                        }
                        else
                        {
                            color = CalculateLightForCustomVertex(room, position, normal, false, staticMesh.Color * 128);
                            //Apply Shade factor
                            color *= shade;
                        }
                        var vertexColor = PackLightColor(color, _level.Settings.GameVersion);
                        var trVertex = new tr_room_vertex
                        {
                            Position = new tr_vertex
                            {
                                X = (short)position.X,
                                Y = (short)-(position.Y + room.WorldPos.Y),
                                Z = (short)position.Z
                            },
                            Lighting1 = vertexColor,
                            Lighting2 = vertexColor,
                            Attributes = (ushort)lightingEffect
                        };
                        roomVertices.Add(trVertex);
                    }

                    for (int i = 0; i < wadStatic.Mesh.Polys.Count; i++)
                    {
                        WadPolygon poly = wadStatic.Mesh.Polys[i];
                        ushort index0 = (ushort)(poly.Index0 + meshVertexBase);
                        ushort index1 = (ushort)(poly.Index1 + meshVertexBase);
                        ushort index2 = (ushort)(poly.Index2 + meshVertexBase);
                        ushort index3 = (ushort)(poly.Index3 + meshVertexBase);

                            if (poly.Shape == WadPolygonShape.Triangle) {
                                FixWadTextureCoordinates(ref poly.Texture);
                                var result = _textureInfoManager.AddTexture(poly.Texture, true, true);
                                tr_face3 tri = result.CreateFace3(new ushort[] { index0, index1, index2 }, false, 0);
                                roomTriangles.Add(tri);
                            } else {
                                FixWadTextureCoordinates(ref poly.Texture);
                                var result = _textureInfoManager.AddTexture(poly.Texture, true, false);
                                tr_face4 quad = result.CreateFace4(new ushort[] { index0, index1, index2, index3 }, false, 0);
                                roomQuads.Add(quad);
                            }
                        }
                }

                // Add geometry imported objects

                foreach (var geometry in room.Objects.OfType<ImportedGeometryInstance>())
                {
                    if (geometry.Model?.DirectXModel == null)
                        continue;

                    var meshes = geometry.Model.DirectXModel.Meshes;
                    var worldTransform = geometry.RotationMatrix *
                                         Matrix4x4.CreateScale(geometry.Scale) *
                                         Matrix4x4.CreateTranslation(geometry.Position);
                    var normalTransform = geometry.RotationMatrix;
                    var indexList = new List<int>();
                    int baseIndex = 0;

                    foreach (var mesh in meshes)
                    {
                        int currentMeshIndexCount = 0;

                        for (int j = 0; j < mesh.Vertices.Count; j++)
                        {
                            var vertex = mesh.Vertices[j];

                            // Since imported geometry can be used as reimported room mesh, we need to make sure
                            // coordinates are integer to comply with portal positions, so MatchDoorShades function
                            // will later work correctly. While technically rounding vertex positions is incorrect,
                            // thankfully TR engines use large-scale coordinate system, so we can ignore such precision loss.

                            vertex.Position = MathC.Round(vertex.Position);

                            // Apply the transform to the vertex
                            Vector3 position = MathC.HomogenousTransform(vertex.Position, worldTransform);
                            Vector3 normal = MathC.HomogenousTransform(vertex.Normal, normalTransform);
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
                            if (geometry.LightingModel == ImportedGeometryLightingModel.VertexColors)
                            {
                                var color = PackLightColor(vertex.Color * geometry.Color, _level.Settings.GameVersion);
                                trVertex.Lighting1 = color;
                                trVertex.Lighting2 = color;
                            }
                            else if (geometry.LightingModel == ImportedGeometryLightingModel.CalculateFromLightsInRoom)
                            {
                                var color = PackLightColor(CalculateLightForCustomVertex(room, position, normal, true, room.Properties.AmbientLight * geometry.Color * 128), _level.Settings.GameVersion);
                                trVertex.Lighting1 = color;
                                trVertex.Lighting2 = color;
                            }
                            else if (geometry.LightingModel == ImportedGeometryLightingModel.TintAsAmbient)
                            {
                                var color = PackLightColor(geometry.Color, _level.Settings.GameVersion);
                                trVertex.Lighting1 = color;
                                trVertex.Lighting2 = color;
                            }
                            else
                            {
                                var color = PackLightColor(room.Properties.AmbientLight * geometry.Color, _level.Settings.GameVersion);
                                trVertex.Lighting1 = color;
                                trVertex.Lighting2 = color;
                            }

                            // HACK: Find a vertex with same coordinates and merge with it.
                            // This is needed to overcome disjointed vertices bug buried deep in geometry importer AND assimp's own
                            // strange behaviour which splits imported geometry based on materials.
                            // We still preserve sharp edges if explicitly specified by flag though.

                            int existingIndex;
                            if (geometry.SharpEdges)
                            {
                                existingIndex = roomVertices.Count;
                                roomVertices.Add(trVertex);
                            }
                            else
                            {
                                existingIndex = roomVertices.IndexOf(v => v.Position == trVertex.Position && v.Color == trVertex.Color);
                                if (existingIndex == -1)
                                {
                                    existingIndex = roomVertices.Count;
                                    roomVertices.Add(trVertex);
                                }
                            }

                            indexList.Add(existingIndex);
                            currentMeshIndexCount++;
                        }

                        foreach (var submesh in mesh.Submeshes)
                        {
                            for (int j = 0; j < submesh.Value.Indices.Count; j += 3)
                            {
                                ushort index0 = (ushort)(indexList[j + baseIndex + 0]);
                                ushort index1 = (ushort)(indexList[j + baseIndex + 1]);
                                ushort index2 = (ushort)(indexList[j + baseIndex + 2]);

                                // TODO Move texture area into the mesh
                                TextureArea texture = new TextureArea();
                                texture.DoubleSided = submesh.Key.DoubleSided;
                                texture.BlendMode = submesh.Key.AdditiveBlending ? BlendMode.Additive : BlendMode.Normal;
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

                                var doubleSided = _level.Settings.GameVersion >  TRVersion.Game.TR2 && texture.DoubleSided;
                                var copyFace    = _level.Settings.GameVersion <= TRVersion.Game.TR2 && texture.DoubleSided;

                                var result = _textureInfoManager.AddTexture(texture, true, true);
                                roomTriangles.Add(result.CreateFace3(new ushort[] { index0, index1, index2 }, doubleSided, 0));

                                if (copyFace)
                                {
                                    texture.Mirror();
                                    result = _textureInfoManager.AddTexture(texture, true, true);
                                    roomTriangles.Add(result.CreateFace3(new ushort[] { index2, index1, index0 }, doubleSided, 0));
                                }
                            }
                        }

                        baseIndex += currentMeshIndexCount;
                    }
                }

                // Check for the limits reached

                int maxVertexCount = _limits[Limit.RoomVertexCount];
                if (roomVertices.Count >= maxVertexCount)
                    _progressReporter.ReportWarn("Room '" + room.Name + "' has too many vertices (limit is " + maxVertexCount + ")! Try to remove some imported geometry objects.");

                int maxPolyCount = _limits[Limit.RoomFaceCount];
                int numPolygons  = roomQuads.Count + roomTriangles.Count;
                if (numPolygons > maxPolyCount)
                    _progressReporter.ReportWarn("Room '" + room.Name + "' has " + numPolygons + " polygons, while limit is around " + maxPolyCount + ". Try to unmerge statics or remove some imported geometry objects.");

                // Assign vertex effects

                for (int i = 0; i < roomVertices.Count; ++i)
                {
                    var trVertex = roomVertices[i];
                    ushort flags = 0x0000;

                    bool allowMovement = true;
                    bool allowGlow = true;

                    foreach (var portal in room.Portals)
                    {
                        var xv = trVertex.Position.X / (int)Level.WorldUnit;
                        var zv = trVertex.Position.Z / (int)Level.WorldUnit;

                        var otherRoomLightEffect = portal.AdjoiningRoom.Properties.LightEffect;
                        if (otherRoomLightEffect == RoomLightEffect.Default)
                        {
                            switch (portal.AdjoiningRoom.Properties.Type)
                            {
                                case RoomType.Water:
                                    otherRoomLightEffect = RoomLightEffect.Glow;
                                    break;
                                case RoomType.Quicksand:
                                    otherRoomLightEffect = RoomLightEffect.Movement;
                                    break;
                                default:
                                    otherRoomLightEffect = RoomLightEffect.None;
                                    break;
                            }
                        }

                        // Check for imported geometry out of room bounds
                        if (xv > 0 && zv > 0 && xv < room.NumXSectors && zv < room.NumZSectors)
                        {
                            var connectionInfo1 = room.GetFloorRoomConnectionInfo(new VectorInt2(xv, zv));
                            var connectionInfo2 = room.GetFloorRoomConnectionInfo(new VectorInt2(xv - 1, zv));
                            var connectionInfo3 = room.GetFloorRoomConnectionInfo(new VectorInt2(xv, zv - 1));
                            var connectionInfo4 = room.GetFloorRoomConnectionInfo(new VectorInt2(xv - 1, zv - 1));

                            bool isTraversablePortal = connectionInfo1.TraversableType == Room.RoomConnectionType.FullPortal &&
                                                       connectionInfo2.TraversableType == Room.RoomConnectionType.FullPortal &&
                                                       connectionInfo3.TraversableType == Room.RoomConnectionType.FullPortal &&
                                                       connectionInfo4.TraversableType == Room.RoomConnectionType.FullPortal;

                            bool isOppositeCorner = connectionInfo1.TraversableType == Room.RoomConnectionType.TriangularPortalXnZn &&
                                                    !connectionInfo2.IsTriangularPortal &&
                                                    !connectionInfo3.IsTriangularPortal &&
                                                    !connectionInfo4.IsTriangularPortal
                                                    ||
                                                    !connectionInfo1.IsTriangularPortal &&
                                                    connectionInfo2.TraversableType == Room.RoomConnectionType.TriangularPortalXpZn &&
                                                    !connectionInfo3.IsTriangularPortal &&
                                                    !connectionInfo4.IsTriangularPortal
                                                    ||
                                                    !connectionInfo1.IsTriangularPortal &&
                                                    !connectionInfo2.IsTriangularPortal &&
                                                    connectionInfo3.TraversableType == Room.RoomConnectionType.TriangularPortalXnZp &&
                                                    !connectionInfo4.IsTriangularPortal
                                                    ||
                                                    !connectionInfo1.IsTriangularPortal &&
                                                    !connectionInfo2.IsTriangularPortal &&
                                                    !connectionInfo3.IsTriangularPortal &&
                                                    connectionInfo4.TraversableType == Room.RoomConnectionType.TriangularPortalXpZp;

                            // A bit complex but working code for water surface movement.
                            // Works better than winroomedit as it takes adjacent portals into account.
                            if ((waterPortals.Contains(portal) && !portal.PositionOnPortal(new VectorInt3(trVertex.Position.X, trVertex.Position.Y, trVertex.Position.Z), false, true)))
                            {
                                // A candidate vertex must belong to portal sectors, non triangular, not wall, not solid floor
                                if ((isTraversablePortal || isOppositeCorner) &&
                                    connectionInfo1.AnyType != Room.RoomConnectionType.NoPortal &&
                                    !room.Blocks[xv, zv].IsAnyWall &&
                                    connectionInfo2.AnyType != Room.RoomConnectionType.NoPortal &&
                                    !room.Blocks[xv - 1, zv].IsAnyWall &&
                                    connectionInfo3.AnyType != Room.RoomConnectionType.NoPortal &&
                                    !room.Blocks[xv, zv - 1].IsAnyWall &&
                                    connectionInfo4.AnyType != Room.RoomConnectionType.NoPortal &&
                                    !room.Blocks[xv - 1, zv - 1].IsAnyWall)
                                {
                                    flags |= 0x2000;
                                }
                            }

                            if (lightEffect == RoomLightEffect.Mist && portal.Direction == PortalDirection.Floor && isTraversablePortal)
                            {
                                // Assign mist, if set, for vertices inside portal
                                if (portal.PositionOnPortal(new VectorInt3(trVertex.Position.X, trVertex.Position.Y, trVertex.Position.Z), true, false))
                                {
                                    flags |= 0x4000;
                                    break;
                                }
                            }
                            else if (lightEffect == RoomLightEffect.Reflection && portal.Direction == PortalDirection.Floor && 
                                ((room.Properties.Type == RoomType.Water || room.Properties.Type == RoomType.Quicksand) != (portal.AdjoiningRoom.Properties.Type == RoomType.Water || portal.AdjoiningRoom.Properties.Type == RoomType.Quicksand)))
                            {
                                // Assign reflection, if set, for all enclosed portal faces
                                if (portal.PositionOnPortal(new VectorInt3(trVertex.Position.X, trVertex.Position.Y, trVertex.Position.Z), false, false) ||
                                    portal.PositionOnPortal(new VectorInt3(trVertex.Position.X, trVertex.Position.Y, trVertex.Position.Z), true, false))
                                {
                                    flags |= 0x4000;
                                    break;
                                }
                            }
                            else if ((lightEffect == RoomLightEffect.Movement || lightEffect == RoomLightEffect.GlowAndMovement) 
                                    || isTR23) // Always check portal edges for TR2-3 because of "no movement" 0x8000 flag in that games
                            {
                                // Disable movement for portal faces
                                if (portal.PositionOnPortal(new VectorInt3(trVertex.Position.X, trVertex.Position.Y, trVertex.Position.Z), false, false) ||
                                    portal.PositionOnPortal(new VectorInt3(trVertex.Position.X, trVertex.Position.Y, trVertex.Position.Z), true, false))
                                {
                                    // Still allow movement, if adjoining room has very same properties
                                    if (!(otherRoomLightEffect == lightEffect &&
                                          portal.AdjoiningRoom.Properties.LightEffectStrength == room.Properties.LightEffectStrength))
                                        allowMovement = false;
                                }
                            }

                            if (lightEffect == RoomLightEffect.Glow || lightEffect == RoomLightEffect.GlowAndMovement)
                            {
                                // Apply effect on all faces, if room light interp mode is sharp-cut
                                if (interpMode != RoomLightInterpolationMode.NoInterpolate)
                                {
                                    // Disable glow for portal faces
                                    if (portal.PositionOnPortal(new VectorInt3(trVertex.Position.X, trVertex.Position.Y, trVertex.Position.Z), false, false) ||
                                        portal.PositionOnPortal(new VectorInt3(trVertex.Position.X, trVertex.Position.Y, trVertex.Position.Z), true, false))
                                    {
                                        // Still allow glow, if adjoining room has very same properties
                                        if (!((otherRoomLightEffect == RoomLightEffect.Glow ||
                                               otherRoomLightEffect == RoomLightEffect.GlowAndMovement) &&
                                               portal.AdjoiningRoom.Properties.LightEffectStrength == room.Properties.LightEffectStrength))
                                            allowGlow = false;
                                    }
                                }
                            }
                        }
                    }

                    if (allowMovement && (lightEffect == RoomLightEffect.Movement || lightEffect == RoomLightEffect.GlowAndMovement))
                        flags |= 0x2000;
                    if (allowGlow     && (lightEffect == RoomLightEffect.Glow     || lightEffect == RoomLightEffect.GlowAndMovement))
                        flags |= 0x4000;

                    // Remap vertex flags to LightEffect intensity for TR2.
                    // FIXME: In original TR2 winroomedit and Dxtre3d, intensity value was based on dummy light object
                    // placed in a room. As we don't have this mechanism yet, just assign effect to whole room.

                    if (isTR2)
                    {
                        bool glowMapped = (flags & 0x4000) == 0x4000;
                        bool moveMapped = (flags & 0x2000) == 0x2000;

                        // Clear existing flags
                        flags = 0x0000;

                        // Force remap if sunset effect is used.
                        // Also prevent hard edges on room transitions which was happening in original TR2 by checking allowMovement flag.
                        if (room.Properties.LightEffect == RoomLightEffect.Sunset)
                        {
                            if (allowMovement)
                                flags = (ushort)(17 + room.Properties.LightEffectStrength * 3.25f); // Closest to max. value of 30
                            else
                                flags = (ushort)16;
                        }
                        else if (room.Properties.LightEffect != RoomLightEffect.None)
                        {
                            // Remap TR3+ glow / movement to TR2 glow / flicker
                            if (glowMapped || moveMapped)
                                flags = (ushort)(16 - room.Properties.LightEffectStrength * 3.0f); // Closest to max. value of 15
                            else
                                flags = (ushort)16;
                        }
                    }

                    // Additionally set "no movement" flag for water rooms. This feature is present in TR2-3, in later games
                    // 0x8000 flag is broken.
                    if (isTR23 && !allowMovement)
                        flags |= 0x8000;


                    if (lightEffect == RoomLightEffect.None && trVertex.Attributes != 0x0000)
                    {
                        // Workaround for desynced water reflections (possibly make it an option in TombEngine?)
                        // If vertex already has attribute assigned (e.g. merged statics), only apply it in case room has no
                        // global vertex effect. It is necessary because if original vertex effect is different from global room vertex
                        // effect, and (possibly) vertex count doesn't match seed, vertex effect seed may become desynced.
                        // This is original TR renderer bug and should be resolved in TombEngine DX11 renderer.
                        // Do not remove this condition.
                    }
                    else
                        trVertex.Attributes = flags;

                    roomVertices[i] = trVertex;
                }

                // Add sprites and dummy vertices for them (only for TR1-2 targets, unsupported since TR3)

                newRoom.Sprites = new List<tr_room_sprite>();
                if (_level.Settings.GameVersion <= TRVersion.Game.TR2)
                    foreach (var sprite in room.Objects.OfType<SpriteInstance>())
                    {
                        if (!sprite.SpriteIsValid)
                        {
                            _progressReporter.ReportWarn(sprite.ToShortString() + " is not present in any of wads and won't be included.");
                            continue;
                        }
                        newRoom.Sprites.Add(new tr_room_sprite() { SpriteID = sprite.SpriteID, Vertex = roomVertices.Count });
                        roomVertices.Add(new tr_room_vertex()
                        { Position = new tr_vertex((short) (sprite.Position.X), 
                                                   (short)-(room.WorldPos.Y + sprite.Position.Y), 
                                                   (short) (sprite.Position.Z)) });
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

                // Calculate color / intensity
                var intensity1 = PackLightColor(new Vector3(instance.Color.Z, instance.Color.Y, instance.Color.X), _level.Settings.GameVersion);

                // Resolve intensity2. It is used for TR2 only, also TRNG reuses this field for static OCB.
                // For TR5, intensity2 must be set to 1 or static mesh won't be drawn.
                // For all other game versions, this field is either not used or not written.

                ushort intensity2;
                switch (_level.Settings.GameVersion)
                {
                    case TRVersion.Game.TR2:
                        intensity2 = intensity1;
                        break;

                    case TRVersion.Game.TRNG:
                        intensity2 = (ushort)instance.Ocb;
                        break;

                    case TRVersion.Game.TR5:
                        intensity2 = 1;
                        break;
                        
                    default:
                        intensity2 = 0;
                        break;
                }

                newRoom.StaticMeshes.Add(new tr_room_staticmesh
                {
                    X = (int)Math.Round(newRoom.Info.X + instance.Position.X),
                    Y = (int)-Math.Round(room.WorldPos.Y + instance.Position.Y),
                    Z = (int)Math.Round(newRoom.Info.Z + instance.Position.Z),
                    Rotation = (ushort)Math.Max(0, Math.Min(ushort.MaxValue,
                        Math.Round(instance.RotationY * (65536.0 / 360.0)))),
                    ObjectID = checked((ushort)instance.WadObjectId.TypeId),
                    Intensity1 = intensity1,
                    Intensity2 = intensity2
                });
            }

            ConvertLights(room, newRoom);

            // Clear double-sided flag for versions prior to TR3, so at least surfaces remain visible
            if (_level.Settings.GameVersion < TRVersion.Game.TR3)
            {
                for (int i = 0; i < newRoom.Triangles.Count; i++)
                {
                    var tri = newRoom.Triangles[i];
                    if ((tri.Texture & 0x8000) == 0x8000)
                        tri.Texture = (ushort)(tri.Texture & 0x7FFF);
                }

                for (int i = 0; i < newRoom.Quads.Count; i++)
                {
                    var tri = newRoom.Quads[i];
                    if ((tri.Texture & 0x8000) == 0x8000)
                        tri.Texture = (ushort)(tri.Texture & 0x7FFF);
                }
            }

            return newRoom;
        }

        public static ushort GetOrAddVertex(Room room, Dictionary<int, ushort> roomVerticesDictionary, List<tr_room_vertex> roomVertices, Vector3 Position, Vector3 Color)
        {
            tr_room_vertex trVertex;
            trVertex.Position = new tr_vertex
            {
                X = (short)Position.X,
                Y = (short)-(Position.Y + room.WorldPos.Y),
                Z = (short)Position.Z
            };

            trVertex.Lighting1 = 0;
            trVertex.Attributes = 0;
            trVertex.Lighting2 = 0;
            trVertex.Color = 0;
            trVertex.IsOnPortal = false;
            trVertex.Normal = new tr_vertex
            {
                X = 0,
                Y = 0,
                Z = 0
            };

            // Ignore this for TRNG and TR4
            if (room.Level.Settings.GameVersion == TRVersion.Game.TR5)
                trVertex.Color = PackColorTo32Bit(Color);
            else
            {
                var color = PackLightColor(Color, room.Level.Settings.GameVersion);
                trVertex.Lighting1 = color;
                trVertex.Lighting2 = color;
            }

            return GetOrAddVertex(room, roomVerticesDictionary, roomVertices, trVertex);
        }

        private static ushort GetOrAddVertex(Room room, Dictionary<int, ushort> roomVerticesDictionary, List<tr_room_vertex> roomVertices, tr_room_vertex trVertex)
        {
            // Do the check here, so we can save some time with unuseful calculations
            ushort vertexIndex;
            if (roomVerticesDictionary.TryGetValue(trVertex.GetHashCode(), out vertexIndex))
                return vertexIndex;

            // Add vertex
            vertexIndex = (ushort)roomVertices.Count;
            roomVerticesDictionary.Add(trVertex.GetHashCode(), vertexIndex);
            roomVertices.Add(trVertex);
            return vertexIndex;
        }

        private void ConvertLights(Room room, tr_room newRoom)
        {
            int lightCount = 0;

            foreach (var light in room.Objects.OfType<LightInstance>())
            {
                // If target game is <= TR3 then ignore all special lights (except sun for TR3) and fog bulbs
                if (_level.Settings.GameVersion < TRVersion.Game.TR4 &&
                    (light.Type == LightType.Spot || light.Type == LightType.Shadow || light.Type == LightType.FogBulb))
                    continue;

                // Sun type is present in TR3
                if (_level.Settings.GameVersion < TRVersion.Game.TR3 && light.Type == LightType.Sun)
                    continue;

                if (!light.Enabled || !light.IsDynamicallyUsed)
                    continue;

                tr_color color;
                ushort intensity;

                if (light.Type == LightType.FogBulb)
                {
                    if (_level.Settings.GameVersion.Legacy() == TRVersion.Game.TR4)
                    {
                        // HACK: remap TR4 fog bulb intensity to color (native TR4 hack)
                        var remappedColor = new Vector3(MathC.Clamp(light.Intensity, 0.0f, 2.0f));
                        color = PackColorTo24Bit(remappedColor);
                    }
                    else
                    {
                        // TR5 stores fog bulb intensity and color separately as real values
                        color = PackColorTo24Bit(light.Color);
                    }

                    intensity = (ushort)8191;
                }
                else
                {
                    color = PackColorTo24Bit(light.Color);
                    intensity = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, Math.Abs(light.Intensity) * 8191.0f));
                }

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
                        newLight.In  = light.InnerRange * Level.WorldUnit;
                        newLight.Out = light.OuterRange * Level.WorldUnit;
                        break;
                    case LightType.Shadow:
                        newLight.LightType = 3;
                        newLight.In  = light.InnerRange * Level.WorldUnit;
                        newLight.Out = light.OuterRange * Level.WorldUnit;
                        break;
                    case LightType.Spot:
                        newLight.LightType = 2;
                        newLight.In = (float)Math.Cos(light.InnerAngle * (Math.PI / 180));
                        newLight.Out = (float)Math.Cos(light.OuterAngle * (Math.PI / 180));
                        newLight.Length = light.InnerRange * Level.WorldUnit;
                        newLight.CutOff = light.OuterRange * Level.WorldUnit;
                        Vector3 spotDirection = light.GetDirection();
                        newLight.DirectionX = -spotDirection.X;
                        newLight.DirectionY =  spotDirection.Y;
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
                        newLight.DirectionY =  sunDirection.Y;
                        newLight.DirectionZ = -sunDirection.Z;
                        break;
                    case LightType.FogBulb:
                        newLight.LightType = 4;
                        newLight.In  = light.InnerRange * Level.WorldUnit;
                        newLight.Out = light.OuterRange * Level.WorldUnit;
                        newLight.Length = light.Intensity; // Store float intensity as length
                        break;
                    case LightType.Effect:
                        continue;
                    default:
                        throw new Exception("Unknown light type '" + light.Type + "' encountered.");
                }

                newRoom.Lights.Add(newLight);
            }

            int lightLimit = _limits[Limit.RoomLightCount];
            if (lightCount > lightLimit)
                _progressReporter.ReportWarn("Room '" + room + "' has more than " + lightLimit + " dynamic lights (count is " + lightCount + "). This may cause crashes.");
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

                    if (_level.Settings.GameVersion >= TRVersion.Game.TR3)
                        sector.BoxIndex = (ushort)(0x7ff0 | (0xf & (short)GetTextureSound(room, x, z)));
                    else
                        sector.BoxIndex = 0xffff;
                    sector.FloorDataIndex = 0;

                    // Setup portals
                    if (room.GetFloorRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType != Room.RoomConnectionType.NoPortal)
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

                    if (room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType != Room.RoomConnectionType.NoPortal)
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
                    if (room.Properties.Type != RoomType.Water && (Math.Abs(block.Floor.IfQuadSlopeX) == 1 ||
                                                        Math.Abs(block.Floor.IfQuadSlopeX) == 2 ||
                                                        Math.Abs(block.Floor.IfQuadSlopeZ) == 1 ||
                                                        Math.Abs(block.Floor.IfQuadSlopeZ) == 2))
                        aux.SoftSlope = true;
                    if (room.Properties.Type != RoomType.Water && (Math.Abs(block.Floor.IfQuadSlopeX) > 2 || Math.Abs(block.Floor.IfQuadSlopeZ) > 2))
                        aux.HardSlope = true;
                    if (block.Type == BlockType.Wall)
                        aux.Wall = true;

                    // Setup floor heights
                    if (x == 0 || z == 0 || x == room.NumXSectors - 1 || z == room.NumZSectors - 1 ||
                        block.Type == BlockType.BorderWall || block.Type == BlockType.Wall)
                    {
                        sector.Floor = (sbyte)(-room.Position.Y - block.Floor.Max);
                        sector.Ceiling = (sbyte)(-room.Position.Y - block.Ceiling.Min);
                        if (sector.Floor < sector.Ceiling) sector.Floor = sector.Ceiling;
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

                        var floor   = Level.QuarterWorldUnit * block.Floor.GetHeight(relevantDirection) + room.WorldPos.Y;
                        var ceiling = Level.QuarterWorldUnit * block.Ceiling.GetHeight(relevantDirection) + room.WorldPos.Y;

                        var floorOpposite   = Level.QuarterWorldUnit * oppositeBlock.Floor.GetHeight(oppositeRelevantDirection) + portal.AdjoiningRoom.WorldPos.Y;
                        var ceilingOpposite = Level.QuarterWorldUnit * oppositeBlock.Ceiling.GetHeight(oppositeRelevantDirection) + portal.AdjoiningRoom.WorldPos.Y;

                        floor = Math.Min(floor, floorOpposite);
                        ceiling = Math.Max(ceiling, ceilingOpposite);

                        yMin = Math.Min(yMin, Math.Min(floor, ceiling));
                        yMax = Math.Max(yMax, Math.Max(floor, ceiling));
                    }
                }
            yMin = (float)Math.Floor(yMin);
            yMax = (float)Math.Ceiling(yMax);

            var xMin = portal.Area.X0 * Level.WorldUnit;
            var xMax = (portal.Area.X1 + 1) * Level.WorldUnit;
            var zMin = portal.Area.Y0 * Level.WorldUnit;
            var zMax = (portal.Area.Y1 + 1) * Level.WorldUnit;

            // Determine normal and portal vertices
            tr_vertex[] portalVertices = new tr_vertex[4];
            tr_vertex normal;
            switch (portal.Direction)
            {
                case PortalDirection.WallPositiveZ:
                    normal = new tr_vertex(0, 0, -1);
                    portalVertices[0] = new tr_vertex((short)xMin, (short)-yMax, (short)(zMax - Level.WorldUnit));
                    portalVertices[1] = new tr_vertex((short)xMax, (short)-yMax, (short)(zMax - Level.WorldUnit));
                    portalVertices[2] = new tr_vertex((short)xMax, (short)-yMin, (short)(zMax - Level.WorldUnit));
                    portalVertices[3] = new tr_vertex((short)xMin, (short)-yMin, (short)(zMax - Level.WorldUnit));
                    break;
                case PortalDirection.WallPositiveX:
                    normal = new tr_vertex(-1, 0, 0);
                    portalVertices[0] = new tr_vertex((short)(xMax - Level.WorldUnit), (short)-yMin, (short)zMax);
                    portalVertices[1] = new tr_vertex((short)(xMax - Level.WorldUnit), (short)-yMax, (short)zMax);
                    portalVertices[2] = new tr_vertex((short)(xMax - Level.WorldUnit), (short)-yMax, (short)zMin);
                    portalVertices[3] = new tr_vertex((short)(xMax - Level.WorldUnit), (short)-yMin, (short)zMin);
                    break;
                case PortalDirection.WallNegativeZ:
                    normal = new tr_vertex(0, 0, 1);
                    portalVertices[0] = new tr_vertex((short)xMax, (short)-yMax, (short)(zMin + Level.WorldUnit - 1));
                    portalVertices[1] = new tr_vertex((short)xMin, (short)-yMax, (short)(zMin + Level.WorldUnit - 1));
                    portalVertices[2] = new tr_vertex((short)xMin, (short)-yMin, (short)(zMin + Level.WorldUnit - 1));
                    portalVertices[3] = new tr_vertex((short)xMax, (short)-yMin, (short)(zMin + Level.WorldUnit - 1));
                    break;
                case PortalDirection.WallNegativeX:
                    normal = new tr_vertex(1, 0, 0);
                    portalVertices[0] = new tr_vertex((short)(xMin + Level.WorldUnit - 1), (short)-yMin, (short)zMin);
                    portalVertices[1] = new tr_vertex((short)(xMin + Level.WorldUnit - 1), (short)-yMax, (short)zMin);
                    portalVertices[2] = new tr_vertex((short)(xMin + Level.WorldUnit - 1), (short)-yMax, (short)zMax);
                    portalVertices[3] = new tr_vertex((short)(xMin + Level.WorldUnit - 1), (short)-yMin, (short)zMax);
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
                        room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z), true) :
                        room.GetFloorRoomConnectionInfo(new VectorInt2(x, z), true);

                    if (roomConnectionInfo.AnyType != Room.RoomConnectionType.NoPortal)
                    {
                        BlockSurface s = isCeiling ? block.Ceiling : block.Floor;

                        if (s.DiagonalSplit != DiagonalSplit.None || BlockSurface.IsQuad2(s.XnZn, s.XpZn, s.XnZp, s.XpZp))
                        {
                            switch (s.DiagonalSplit)
                            {
                                // Ordinary quad
                                case DiagonalSplit.None:
                                    AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XnZn, z, s.XpZn - s.XnZn, s.XnZp - s.XnZn));
                                    break;

                                case DiagonalSplit.XnZn:
                                    AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XpZp, z, 0, 0));
                                    break;

                                case DiagonalSplit.XnZp:
                                    AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XpZn, z, 0, 0));
                                    break;

                                case DiagonalSplit.XpZn:
                                    AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XnZp, z, 0, 0));
                                    break;

                                case DiagonalSplit.XpZp:
                                    AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XnZn, z, 0, 0));
                                    break;
                            }
                        }
                        else if (s.SplitDirectionIsXEqualsZ)
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

                float xMin = portalArea.X0 * Level.WorldUnit;
                float xMax = (portalArea.X1 + 1) * Level.WorldUnit;
                float zMin = portalArea.Y0 * Level.WorldUnit;
                float zMax = (portalArea.Y1 + 1) * Level.WorldUnit;

                float yAtXMinZMin = (room.Position.Y + portalPlane.EvaluateHeight(portalArea.X0, portalArea.Y0)) * Level.QuarterWorldUnit;
                float yAtXMaxZMin = (room.Position.Y + portalPlane.EvaluateHeight(portalArea.X1 + 1, portalArea.Y0)) * Level.QuarterWorldUnit;
                float yAtXMinZMax = (room.Position.Y + portalPlane.EvaluateHeight(portalArea.X0, portalArea.Y1 + 1)) * Level.QuarterWorldUnit;
                float yAtXMaxZMax = (room.Position.Y + portalPlane.EvaluateHeight(portalArea.X1 + 1, portalArea.Y1 + 1)) * Level.QuarterWorldUnit;

                // Choose portal coordinates
                tr_vertex[] portalVertices = new tr_vertex[4];
                tr_vertex normal = new tr_vertex((short)-portalPlane.SlopeX, 4, (short)-portalPlane.SlopeZ);
                if (isCeiling)
                {
                    // TEST: this should solve flickering rooms when camera is on portal
                    Vector3 n = Vector3.UnitY;

                    portalVertices[0] = new tr_vertex((short)xMax, (short)(-yAtXMaxZMin - 1), (short)zMin);
                    portalVertices[1] = new tr_vertex((short)xMin, (short)(-yAtXMinZMin - 1), (short)zMin);
                    portalVertices[2] = new tr_vertex((short)xMin, (short)(-yAtXMinZMax - 1), (short)zMax);
                    portalVertices[3] = new tr_vertex((short)xMax, (short)(-yAtXMaxZMax - 1), (short)zMax);
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

        private void MatchDoorShades(List<tr_room> roomList, tr_room room, bool grayscale, bool flipped)
        {
            // Do we want to interpolate?
            if (room.OriginalRoom.Properties.LightInterpolationMode == RoomLightInterpolationMode.NoInterpolate)
                return;

            foreach (var p in room.Portals)
            {
                var otherRoom = roomList[p.AdjoiningRoom];

                // Here we must decide if match or not, basing on flipped flag.
                // In winroomedit.exe, all flipped rooms were swapped with their counterparts,
                // here instead we'll decide per portal
                if (!flipped)
                {
                    // We allow here normal vs normal or base room vs base room
                    if (room.AlternateKind == AlternateKind.AlternateRoom || otherRoom.AlternateKind == AlternateKind.AlternateRoom)
                        continue;
                }
                else
                {
                    // We allow here flipped vs flipped, flipped vs normal and flipped vs base room.
                    // NOTICE: We only take other flipped room into account if it belongs to SAME ALTERNATE GROUP.
                    if (otherRoom.AlternateKind == AlternateKind.BaseRoom && roomList[otherRoom.AlternateRoom].AlternateGroup == room.AlternateGroup)
                        otherRoom = roomList[otherRoom.AlternateRoom];

                    // Don't proceed if current room isn't alternate or if alternate group don't match (paranoidal foolproof in case previous check failed).
                    if (room.AlternateKind != AlternateKind.AlternateRoom || (otherRoom.AlternateKind == AlternateKind.AlternateRoom && room.AlternateGroup != otherRoom.AlternateGroup))
                        continue;
                }

                // Check if the other room must be interpolated
                if (otherRoom.OriginalRoom.Properties.LightInterpolationMode == RoomLightInterpolationMode.NoInterpolate)
                    continue;

                // If we have a pair of water room and dry room, orginal behaviour of TRLE was to not interpolate,
                // but now we have flags
                bool isWaterAndDryPair = ((room.Flags & 1) == 1 ^ (otherRoom.Flags & 1) == 1);

                if (!isWaterAndDryPair || (isWaterAndDryPair &&
                    (room.OriginalRoom.Properties.LightInterpolationMode == RoomLightInterpolationMode.Interpolate ||
                     otherRoom.OriginalRoom.Properties.LightInterpolationMode == RoomLightInterpolationMode.Interpolate)))
                {
                    int x1 = p.Vertices[0].X;
                    int y1 = p.Vertices[0].Y;
                    int z1 = p.Vertices[0].Z;

                    int x2 = x1 + 1;
                    int y2 = y1 + 1;
                    int z2 = z1 + 1;

                    for (int i = 1; i < 4; i++)
                    {
                        if (p.Vertices[i].X < x1)
                            x1 = p.Vertices[i].X;
                        else if (p.Vertices[i].X > x2)
                            x2 = p.Vertices[i].X + 1;

                        if (p.Vertices[i].Y < y1)
                            y1 = p.Vertices[i].Y;
                        else if (p.Vertices[i].Y > y2)
                            y2 = p.Vertices[i].Y + 1;

                        if (p.Vertices[i].Z < z1)
                            z1 = p.Vertices[i].Z;
                        else if (p.Vertices[i].Z > z2)
                            z2 = p.Vertices[i].Z + 1;
                    }

                    for (int i = 0; i < room.Vertices.Count; i++)
                    {
                        var v1 = room.Vertices[i];
                        var sig = new ShadeMatchSignature()
                        {
                            // NOTE: We keep alternate group and water flag in dictionary as well, this way we only apply vertex colour to
                            // appropriate rooms, and not all rooms at once.
                            IsWater = (room.Flags & 1) == 1,
                            AlternateGroup = room.AlternateKind == AlternateKind.AlternateRoom ? room.AlternateGroup : -1,
                            Position = new VectorInt3(v1.Position.X + room.Info.X, v1.Position.Y, v1.Position.Z + room.Info.Z)
                        };

                        if (v1.Position.X >= x1 && v1.Position.X <= x2)
                            if (v1.Position.Y >= y1 && v1.Position.Y <= y2)
                                if (v1.Position.Z >= z1 && v1.Position.Z <= z2)
                                {
                                    v1.IsOnPortal = true;
                                    room.Vertices[i] = v1;

                                    int otherX = v1.Position.X + room.Info.X - otherRoom.Info.X;
                                    int otherY = v1.Position.Y;
                                    int otherZ = v1.Position.Z + room.Info.Z - otherRoom.Info.Z;

                                    for (int j = 0; j < otherRoom.Vertices.Count; j++)
                                    {
                                        uint refColor = 0;
                                        var v2 = otherRoom.Vertices[j];
                                        var isPresentInLookup = _vertexColors.TryGetValue(sig, out refColor);

                                        if (!isPresentInLookup)
                                        {
                                            if (_level.Settings.GameVersion != TRVersion.Game.TR5)
                                                refColor = v1.Lighting2;
                                            else
                                                refColor = v1.Color;
                                        }

                                        if (room.Info.X + v1.Position.X == otherRoom.Info.X + v2.Position.X &&
                                            v1.Position.Y == v2.Position.Y &&
                                            room.Info.Z + v1.Position.Z == otherRoom.Info.Z + v2.Position.Z)
                                        {
                                            uint newColor = 0;

                                            // NOTE: We DON'T INTERPOLATE colours of both rooms in case we're dealing with alternate room and matched room
                                            // isn't alternate room itself. Instead, we simply copy vertex colour from matched base room.
                                            // This way we don't get sharp-cut half-transitioned vertex colour.

                                            if (flipped && otherRoom.AlternateKind != AlternateKind.AlternateRoom)
                                            {
                                                var baseSig = new ShadeMatchSignature() { IsWater = sig.IsWater, AlternateGroup = -1, Position = sig.Position };

                                                if (!_vertexColors.TryGetValue(baseSig, out newColor))
                                                {
                                                    if (_level.Settings.GameVersion != TRVersion.Game.TR5)
                                                        newColor = v2.Lighting2;
                                                    else
                                                        newColor = v2.Color;
                                                }
                                            }
                                            else
                                            {
                                                if (grayscale)
                                                    newColor = (ushort)(8160 - (((8160 - v2.Lighting2) / 2) + ((8160 - refColor) / 2)));
                                                else if (_level.Settings.GameVersion != TRVersion.Game.TR5)
                                                    newColor = (ushort)((((v2.Lighting2 & 0x1f) + (refColor & 0x1f)) >> 1) |
                                                                        32 * (((((v2.Lighting2 >> 5) & 0x1f) + ((refColor >> 5) & 0x1f)) >> 1) |
                                                                            32 * ((((v2.Lighting2 >> 10) & 0x1f) + ((refColor >> 10) & 0x1f)) >> 1)));
                                                else
                                                    newColor = (uint)(0xff000000 | (((((v2.Color & 0xff) + (refColor & 0xff)) >> 1) |
                                                                        256 * (((((v2.Color >> 8) & 0xff) + ((refColor >> 8) & 0xff)) >> 1) |
                                                                            256 * ((((v2.Color >> 16) & 0xff) + ((refColor >> 16) & 0xff)) >> 1)))));
                                            }

                                            if (!isPresentInLookup)
                                                _vertexColors.TryAdd(sig, newColor);
                                            else
                                                _vertexColors[sig] = newColor;
                                        }
                                    }
                                }
                    }
                }
            }
        }

        private static void FindConnectedRooms(List<Room> outSharedRooms, Room currentRoom, Vector3 worldPos, bool checkFloorCeiling)
        {
            if (outSharedRooms.Contains(currentRoom))
                return;
            outSharedRooms.Add(currentRoom);

            Vector3 localPos = worldPos - currentRoom.WorldPos;
            int sectorPosX = (int)(localPos.X * (1.0f / Level.WorldUnit) + 0.5f);
            int sectorPosZ = (int)(localPos.Z * (1.0f / Level.WorldUnit) + 0.5f);
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

        private static ushort PackLightColor(Vector3 color, TRVersion.Game version)
        {
            if (version >= TRVersion.Game.TR3)
                return PackColorTo16Bit(color);
            else
                return PackColorTo13BitGreyscale(color);
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

        // Takes the average color value and packs it into an inversed 13 Bit luma value (Higher = Darker)
        // Used for TR1? and TR2 and TR3 for AmbientInensity
        private static ushort PackColorTo13BitGreyscale(Vector3 color)
        {
            // Normalize
            float avg = MathC.Clamp((color / 2.0f).GetLuma(), 0.0f, 1.0f);
            // Invert
            avg = 1.0f - avg;
            // Multiply by 255 to get 8 bits
            avg *= 255.0f;
            ushort value = (ushort)avg;
            // Bitshift to left for whatever reason
            value <<= 5;
            return value;
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
