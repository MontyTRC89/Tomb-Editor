using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using TombLib.LevelData.Compilers.Util;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.Compilers.TombEngine
{
    public sealed partial class LevelCompilerTombEngine
    {
        private readonly Dictionary<Room, int> _roomsRemappingDictionary = new Dictionary<Room, int>(new ReferenceEqualityComparer<Room>());
        private readonly List<Room> _roomsUnmapping = new List<Room>();
        private Dictionary<WadPolygon, TombEngineTexInfoManager.Result> _mergedStaticMeshTextureInfos = new Dictionary<WadPolygon, TombEngineTexInfoManager.Result>();
        private Dictionary<ShadeMatchSignature, Vector3> _vertexColors;

        private void BuildRooms()
        {
            ReportProgress(5, "Lighting Rooms");
            Parallel.ForEach<Room>(_level.Rooms.Where(r => r != null), (room) =>
            {
                room.RebuildLighting(!_level.Settings.FastMode);
            });

            ReportProgress(15, "Building rooms");

            foreach (var room in _level.Rooms.Where(r => r != null))
            {
                _roomsRemappingDictionary.Add(room, _roomsUnmapping.Count);
                _roomsUnmapping.Add(room);
            }

            _staticsTable = new Dictionary<StaticInstance, int>(new ReferenceEqualityComparer<StaticInstance>());

            foreach (var room in _roomsRemappingDictionary.Keys)
                _tempRooms.Add(room, BuildRoom(room));

            // Remove WaterScheme values for water rooms
            Parallel.ForEach(_tempRooms.Values, (TombEngineRoom trRoom) => { if ((trRoom.Flags & 0x0001) != 0) trRoom.WaterScheme = 0; });

            Parallel.ForEach(_tempRooms.Values, (TombEngineRoom trRoom) => { 
                for (int i=0;i<trRoom.Polygons.Count;i++)
                {
                    if (trRoom.Polygons[i].Animated)
                    {
                        //trRoom.Polygons[i].AnimatedSequence = _textureInfoManager.AnimatedTextures[0].Value.
                    }
                }
            });

            ReportProgress(20, "    Number of rooms: " + _roomsUnmapping.Count);

            if (!_level.Settings.FastMode)
            {
                ReportProgress(23, "    Matching vertex colors on portals...");

                _vertexColors = new Dictionary<ShadeMatchSignature, Vector3>();
                var rooms = _tempRooms.Values.ToList();
                for (int flipped = 0; flipped <= 1; flipped++)
                    foreach (var room in rooms)
                        MatchDoorShades(rooms, room, false, flipped == 1);
            }

            Parallel.ForEach(_tempRooms.Values, (TombEngineRoom trRoom) =>
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
                        Position = new VectorInt3(trRoom.Info.X + (int)v.Position.X, (int)v.Position.Y, trRoom.Info.Z + (int)v.Position.Z)
                    };

                    if (_vertexColors.ContainsKey(sig))
                    {
                        v.Color = _vertexColors[sig];
                        v.Color = _vertexColors[sig];
                        trRoom.Vertices[i] = v;
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

            return Vector3.Max(output, new Vector3()) * (1.0f / 128.0f);
        }

        private TombEngineRoom BuildRoom(Room room)
        {
            tr_color roomAmbientColor = PackColorTo24Bit(room.Properties.AmbientLight);

            int maxDimensions = _limits[Limit.RoomDimensions];
            if (room.NumXSectors >= maxDimensions || room.NumZSectors >= maxDimensions)
                _progressReporter.ReportWarn("Room '" + room + "' is very big! Rooms bigger than " + maxDimensions + " sectors per side may cause trouble with rendering.");

            var newRoom = new TombEngineRoom
            {
                OriginalRoom = room,
                Lights = new List<TombEngineRoomLight>(),
                StaticMeshes = new List<TombEngineRoomStaticMesh>(),
                Portals = new List<tr_room_portal>(),
                Info = new tr_room_info
                {
                    X = room.WorldPos.X,
                    Z = room.WorldPos.Z,
                    YTop = (int)-(room.WorldPos.Y + room.GetHighestCorner()   * Level.QuarterWorldUnit),
                    YBottom = (int)-(room.WorldPos.Y + room.GetLowestCorner() * Level.QuarterWorldUnit)
                },
                NumXSectors = checked((ushort)room.NumXSectors),
                NumZSectors = checked((ushort)room.NumZSectors),
                AlternateRoom = room.Alternated && room.AlternateRoom != null ? (short)_roomsRemappingDictionary[room.AlternateRoom] : (short)-1,
                AlternateGroup = (byte)(room.Alternated /*&& room.AlternateRoom != null*/ ? room.AlternateGroup : 0),
                Flipped = room.Alternated,
                FlippedRoom = room.AlternateRoom,
                BaseRoom = room.AlternateBaseRoom,
                ReverbInfo = (byte)room.Properties.Reverberation,
                Flags = 0x40
            };

            if (!room.Alternated)
                newRoom.AlternateKind = AlternateKind.NotAlternated;
            else if (room.AlternateBaseRoom != null)
                newRoom.AlternateKind = AlternateKind.AlternateRoom;
            else if (room.AlternateRoom != null)
                newRoom.AlternateKind = AlternateKind.BaseRoom;

            // Store ambient intensity
            newRoom.AmbientLight = new Vector3(roomAmbientColor.Red / 255.0f, roomAmbientColor.Green / 255.0f, roomAmbientColor.Blue / 255.0f);

            // Properly identify game version to swap light mode, quicksand and no lensflare flags
            bool isNL = room.Level.Settings.GameVersion.Legacy() >= TRVersion.Game.TR4;
            bool isNG = room.Level.IsNG;

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

            // Light effect
            // WARNING: DO NOT use raw value of 1 in WaterScheme ever with classic tomb engines,
            // it will result in broken effect.

            switch (lightEffect)
            {
                case RoomLightEffect.GlowAndMovement:
                    if (!waterSchemeSet) newRoom.WaterScheme = (byte)(room.Properties.LightEffectStrength * 5.0f);
                    newRoom.Flags |= 0x0100;
                    break;

                case RoomLightEffect.Movement:
                    if (!waterSchemeSet) newRoom.WaterScheme = (byte)(room.Properties.LightEffectStrength * 5.0f);
                    break;

                case RoomLightEffect.Glow:
                case RoomLightEffect.Mist:
                    if (!waterSchemeSet) newRoom.WaterScheme = (byte)(room.Properties.LightEffectStrength == 0 ? 0 : room.Properties.LightEffectStrength + 1);
                    newRoom.Flags |= 0x0100;
                    break;

                case RoomLightEffect.Reflection:
                    newRoom.Flags |= 0x0200;
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

                var roomVerticesDictionary = new Dictionary<int, int>();
                var roomVertices = new List<TombEngineVertex>();
                var roomPolygons = new List<TombEnginePolygon>();

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
                                if (texture.TextureIsInvisible)
                                    continue;

                                if (texture.TextureIsUnavailable)
                                {
                                    _progressReporter.ReportWarn("Missing texture at sector (" + x + "," + z + ") in room " + room.Name + ". Check texture file location.");
                                    continue;
                                }

                                if ((shape == BlockFaceShape.Triangle && texture.TriangleCoordsOutOfBounds) || (shape == BlockFaceShape.Quad && texture.QuadCoordsOutOfBounds))
                                {
                                    _progressReporter.ReportWarn("Texture is out of bounds at sector (" + x + "," + z + ") in room " + room.Name + ". Wrong or resized texture file?");
                                    continue;
                                }
                                BlendMode realBlendMode = texture.BlendMode;
                                if(texture.BlendMode == BlendMode.Normal && texture.Texture.Image.HasAlphaInArea(texture.GetRect()))
                                {
                                    realBlendMode = BlendMode.AlphaBlend;
                                }

                                int rangeEnd = range.Start + range.Count;
                                for (int i = range.Start; i < rangeEnd; i += 3)
                                {
                                    int vertex0Index, vertex1Index, vertex2Index;

                                    if (shape == BlockFaceShape.Quad)
                                    {
                                        int vertex3Index;

                                        if (face == BlockFace.Ceiling)
                                        {
                                            texture.Mirror();
                                            vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 1], vertexColors[i + 1], 0);
                                            vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 2], vertexColors[i + 2], 1);
                                            vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 0], vertexColors[i + 0], 2);
                                            vertex3Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 5], vertexColors[i + 5], 3);
                                        }
                                        else
                                        {
                                            vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 3], vertexColors[i + 3], 0);
                                            vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 2], vertexColors[i + 2], 1);
                                            vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 0], vertexColors[i + 0], 2);
                                            vertex3Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 1], vertexColors[i + 1], 3);
                                        }

                                        var result = _textureInfoManager.AddTexture(texture, TextureDestination.RoomOrAggressive, false);
                                        var poly = result.CreateTombEnginePolygon4(new int[] { vertex0Index, vertex1Index, vertex2Index, vertex3Index },
                                                         (byte)realBlendMode, roomVertices);
                                        roomPolygons.Add(poly);
                                        roomVertices[vertex0Index].Polygons.Add(new NormalHelper(poly));
                                        roomVertices[vertex1Index].Polygons.Add(new NormalHelper(poly));
                                        roomVertices[vertex2Index].Polygons.Add(new NormalHelper(poly));
                                        roomVertices[vertex3Index].Polygons.Add(new NormalHelper(poly));
                                        if (texture.DoubleSided)
                                        {
                                            texture.Mirror();
                                            result = _textureInfoManager.AddTexture(texture, TextureDestination.RoomOrAggressive, false);
                                            poly = result.CreateTombEnginePolygon4(new int[] { vertex3Index, vertex2Index, vertex1Index, vertex0Index },
                                                            (byte)realBlendMode, roomVertices);
                                            roomPolygons.Add(poly);
                                            roomVertices[vertex0Index].Polygons.Add(new NormalHelper(poly));
                                            roomVertices[vertex1Index].Polygons.Add(new NormalHelper(poly));
                                            roomVertices[vertex2Index].Polygons.Add(new NormalHelper(poly));
                                            roomVertices[vertex3Index].Polygons.Add(new NormalHelper(poly));
                                        }
                                        i += 3;
                                    }
                                    else
                                    {
                                        if (face == BlockFace.Ceiling || face == BlockFace.CeilingTriangle2)
                                            texture.Mirror(true);

                                        vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 0], vertexColors[i + 0], 0);
                                        vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 1], vertexColors[i + 1], 1);
                                        vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 2], vertexColors[i + 2], 2);

                                        var result = _textureInfoManager.AddTexture(texture, TextureDestination.RoomOrAggressive, true);
                                        var poly = result.CreateTombEnginePolygon3(new int[] { vertex0Index, vertex1Index, vertex2Index },
                                                        (byte)realBlendMode, roomVertices);
                                        roomPolygons.Add(poly);
                                        roomVertices[vertex0Index].Polygons.Add(new NormalHelper(poly));
                                        roomVertices[vertex1Index].Polygons.Add(new NormalHelper(poly));
                                        roomVertices[vertex2Index].Polygons.Add(new NormalHelper(poly));
                                        if (texture.DoubleSided)
                                        {
                                            texture.Mirror();
                                            result = _textureInfoManager.AddTexture(texture, TextureDestination.RoomOrAggressive, false);
                                            poly = result.CreateTombEnginePolygon3(new int[] { vertex2Index, vertex1Index, vertex0Index },
                                                            (byte)realBlendMode, roomVertices);
                                            roomPolygons.Add(poly);
                                            roomVertices[vertex0Index].Polygons.Add(new NormalHelper(poly));
                                            roomVertices[vertex1Index].Polygons.Add(new NormalHelper(poly));
                                            roomVertices[vertex2Index].Polygons.Add(new NormalHelper(poly));
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

                        bool interpretShadesAsMovement = entry.InterpretShadesAsEffect;
                        bool clearShades = entry.ClearShades;
                        int meshVertexBase = roomVertices.Count;
                        var worldTransform = staticMesh.RotationMatrix *
                                             Matrix4x4.CreateTranslation(staticMesh.Position);
                        var normalTransform = staticMesh.RotationMatrix;
                        WadStatic wadStatic = _level.Settings.WadTryGetStatic(staticMesh.WadObjectId);

                        if (wadStatic == null || wadStatic.Mesh == null)
                            continue;

                        for (int j = 0; j < wadStatic.Mesh.VertexPositions.Count; j++)
                        {
                            // Apply the transform to the vertex
                            Vector3 position = MathC.HomogenousTransform(wadStatic.Mesh.VertexPositions[j], worldTransform);
                            Vector3 normal   = Vector3.Normalize(MathC.HomogenousTransform(wadStatic.Mesh.VertexNormals[j], normalTransform));
                            Vector3 shade    = Vector3.One;

                            var glow = 0f;
                            var move = 0f;
                            
                            if (interpretShadesAsMovement)
                            {
                                if (j < wadStatic.Mesh.VertexColors.Count)
                                {
                                    var luma = wadStatic.Mesh.VertexColors[j].GetLuma();
                                    if (luma < 0.5f) move = luma * 2.0f;   // Movement
                                    else if (luma < 1.0f) glow = (luma - 0.5f) * 2.0f; // Glow
                                }
                            }
                            else
                            {
                                // If we have vertex colors, use them as a luma factor for the resulting vertex color
                                if (!clearShades && wadStatic.Mesh.HasColors)
                                    shade = wadStatic.Mesh.VertexColors[j];

                                // FIXME: Those parameters MUST BE VERTEX-BASED IN TOMB ENGINE!!!
                                if (wadStatic.Mesh.HasAttributes)
                                {
                                    if (wadStatic.Mesh.VertexAttributes[j].Move > 0)
                                        move = (float)wadStatic.Mesh.VertexAttributes[j].Move / 64.0f; // Movement

                                    if (wadStatic.Mesh.VertexAttributes[j].Glow > 0)
                                        glow = (float)wadStatic.Mesh.VertexAttributes[j].Glow / 64.0f; // Glow
                                }
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

                            var trVertex = new TombEngineVertex
                            {
                                Position = new Vector3(position.X, -(position.Y + room.WorldPos.Y), (short)position.Z),
                                Color = color,
                                Normal = normal,
                                Glow = glow,
                                Move = move
                            };

                            roomVertices.Add(trVertex);
                        }

                        for (int i = 0; i < wadStatic.Mesh.Polys.Count; i++)
                        {
                            WadPolygon poly = wadStatic.Mesh.Polys[i];
                            int index0 = (poly.Index0 + meshVertexBase);
                            int index1 = (poly.Index1 + meshVertexBase);
                            int index2 = (poly.Index2 + meshVertexBase);
                            int index3 = (poly.Index3 + meshVertexBase);

                            if (poly.IsTriangle)
                            {

                                if (_mergedStaticMeshTextureInfos.ContainsKey(poly))
                                {
                                    var result = _mergedStaticMeshTextureInfos[poly];
                                    var tri = result.CreateTombEnginePolygon3(new int[] { index0, index1, index2 }, (byte)poly.Texture.BlendMode, roomVertices);
                                    roomPolygons.Add(tri);
                                }
                                else
                                {
                                    FixWadTextureCoordinates(ref poly.Texture);
                                    var result = _textureInfoManager.AddTexture(poly.Texture, TextureDestination.RoomOrAggressive, true);
                                    var tri = result.CreateTombEnginePolygon3(new int[] { index0, index1, index2 }, (byte)poly.Texture.BlendMode, roomVertices);
                                    roomPolygons.Add(tri);
                                    _mergedStaticMeshTextureInfos.Add(poly, result);
                                    roomVertices[index0].Polygons.Add(new NormalHelper(tri));
                                    roomVertices[index1].Polygons.Add(new NormalHelper(tri));
                                    roomVertices[index2].Polygons.Add(new NormalHelper(tri));
                                }
                            }
                            else
                            {
                                if (_mergedStaticMeshTextureInfos.ContainsKey(poly))
                                {
                                    var result = _mergedStaticMeshTextureInfos[poly];
                                    var quad = result.CreateTombEnginePolygon4(new int[] { index0, index1, index2, index3 }, (byte)poly.Texture.BlendMode, roomVertices);
                                    roomPolygons.Add(quad);
                                }
                                else
                                {
                                    FixWadTextureCoordinates(ref poly.Texture);
                                    var result = _textureInfoManager.AddTexture(poly.Texture, TextureDestination.RoomOrAggressive, false);
                                    var quad = result.CreateTombEnginePolygon4(new int[] { index0, index1, index2, index3 }, (byte)poly.Texture.BlendMode, roomVertices);
                                    roomPolygons.Add(quad);
                                    _mergedStaticMeshTextureInfos.Add(poly, result);
                                    roomVertices[index0].Polygons.Add(new NormalHelper(quad));
                                    roomVertices[index1].Polygons.Add(new NormalHelper(quad));
                                    roomVertices[index2].Polygons.Add(new NormalHelper(quad));
                                    roomVertices[index3].Polygons.Add(new NormalHelper(quad));
                                }
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

                            var trVertex = new TombEngineVertex
                            {
                                Position = new Vector3(position.X, -(position.Y + room.WorldPos.Y), position.Z),
                                Normal = normal
                            };

                            // Pack the light according to chosen lighting model
                            if (geometry.LightingModel == ImportedGeometryLightingModel.VertexColors)
                            {
                                trVertex.Color = vertex.Color;
                            }
                            else if (geometry.LightingModel == ImportedGeometryLightingModel.CalculateFromLightsInRoom)
                            {
                                var color = CalculateLightForCustomVertex(room, position, normal, true, room.Properties.AmbientLight * 128);
                                trVertex.Color = color;
                            }
                            else
                            {
                                var color = room.Properties.AmbientLight;
                                trVertex.Color = color;
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
                                int index0 = (indexList[j + baseIndex + 0]);
                                int index1 = (indexList[j + baseIndex + 1]);
                                int index2 = (indexList[j + baseIndex + 2]);

                                // TODO Move texture area into the mesh
                                TextureArea texture = new TextureArea();
                                texture.DoubleSided = false;
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

                                var result = _textureInfoManager.AddTexture(texture, TextureDestination.RoomOrAggressive, true);
                                var tri = result.CreateTombEnginePolygon3(new int[] { index0, index1, index2 }, (byte)texture.BlendMode, roomVertices);
                                roomPolygons.Add(tri);
                                roomVertices[index0].Polygons.Add(new NormalHelper(tri));
                                roomVertices[index1].Polygons.Add(new NormalHelper(tri));
                                roomVertices[index2].Polygons.Add(new NormalHelper(tri));
                            }
                        }

                        baseIndex += currentMeshIndexCount;
                    }
                }

                newRoom.Vertices = roomVertices;
                newRoom.Polygons = roomPolygons;

                // Now we need to build final normals, tangents, bitangents
            }

            // Assign vertex effects

            for (int i = 0; i < newRoom.Vertices.Count; ++i)
            {
                var trVertex = newRoom.Vertices[i];
                bool allowGlow = true;

                foreach (var portal in room.Portals)
                {
                    var xv = (int)(trVertex.Position.X / Level.WorldUnit);
                    var zv = (int)(trVertex.Position.Z / Level.WorldUnit);

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

                        var pos = new VectorInt3((int)trVertex.Position.X, (int)trVertex.Position.Y, (int)trVertex.Position.Z);

                        // A bit complex but working code for water surface movement.
                        // Works better than winroomedit as it takes adjacent portals into account.
                        if ((waterPortals.Contains(portal) && !portal.PositionOnPortal(pos, false, true)))
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
                                trVertex = trVertex.SetEffects(room, RoomLightEffect.Movement);
                            }
                        }

                        if (lightEffect == RoomLightEffect.Mist && portal.Direction == PortalDirection.Floor && isTraversablePortal)
                        {
                            // Assign mist, if set, for vertices inside portal
                            if (portal.PositionOnPortal(pos, true, false))
                            {
                                trVertex = trVertex.SetEffects(room, RoomLightEffect.Glow);
                                break;
                            }
                        }
                        else if (lightEffect == RoomLightEffect.Reflection && portal.Direction == PortalDirection.Floor &&
                            ((room.Properties.Type == RoomType.Water || room.Properties.Type == RoomType.Quicksand) != (portal.AdjoiningRoom.Properties.Type == RoomType.Water || portal.AdjoiningRoom.Properties.Type == RoomType.Quicksand)))
                        {
                            // Assign reflection, if set, for all enclosed portal faces
                            if (portal.PositionOnPortal(pos, false, false) ||
                                portal.PositionOnPortal(pos, true, false))
                            {
                                trVertex = trVertex.SetEffects(room, RoomLightEffect.Glow);
                                break;
                            }
                        }

                        // Disable movement for portal faces
                        if (portal.PositionOnPortal(pos, false, false))
                            trVertex.Locked = true;

                        if (lightEffect == RoomLightEffect.Glow || lightEffect == RoomLightEffect.GlowAndMovement)
                        {
                            // Apply effect on all faces, if room light interp mode is sharp-cut
                            if (interpMode != RoomLightInterpolationMode.NoInterpolate)
                            {
                                // Disable glow for portal faces
                                if (portal.PositionOnPortal(pos, false, false) ||
                                    portal.PositionOnPortal(pos, true, false))
                                {
                                    allowGlow = false;
                                }
                            }
                        }
                    }
                }

                if (lightEffect == RoomLightEffect.Movement || lightEffect == RoomLightEffect.GlowAndMovement)
                    trVertex = trVertex.SetEffects(room, RoomLightEffect.Movement);
                if (allowGlow && (lightEffect == RoomLightEffect.Glow || lightEffect == RoomLightEffect.GlowAndMovement))
                    trVertex = trVertex.SetEffects(room, RoomLightEffect.Glow);

                newRoom.Vertices[i] = trVertex;
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

                var sm = _level.Settings?.WadTryGetStatic(instance.WadObjectId);
                newRoom.StaticMeshes.Add(new TombEngineRoomStaticMesh
                {
                    X = (int)Math.Round(newRoom.Info.X + instance.Position.X),
                    Y = (int)-Math.Round(room.WorldPos.Y + instance.Position.Y),
                    Z = (int)Math.Round(newRoom.Info.Z + instance.Position.Z),
                    Rotation = (ushort)Math.Max(0, Math.Min(ushort.MaxValue,
                        Math.Round(instance.RotationY * (65536.0 / 360.0)))),
                    ObjectID = checked((ushort)instance.WadObjectId.TypeId),
                    Flags = (ushort)(0x0001),
                    Color = new Vector4(instance.Color.X,instance.Color.Y,instance.Color.Z,1.0f),
                    HitPoints = 0, //(short)(sm != null ? sm.HitPoints : (short)1)
                    LuaName = instance.LuaName ?? ""
                }) ;
            }

            ConvertLights(room, newRoom);

            return newRoom;
        }

        private static int GetOrAddVertex(Room room, Dictionary<int, int> roomVerticesDictionary, List<TombEngineVertex> roomVertices,
            Vector3 Position, Vector3 color, int index)
        {
            var trVertex = new TombEngineVertex();

            trVertex.Position = new Vector3(Position.X, -(Position.Y + room.WorldPos.Y), Position.Z);
            trVertex.Color = color / 2.0f;
            trVertex.IsOnPortal = false;
            trVertex.IndexInPoly = index;

            int vertexIndex;
            if (roomVerticesDictionary.TryGetValue(trVertex.GetHashCode(), out vertexIndex))
                return vertexIndex;

            vertexIndex = roomVertices.Count;
            roomVertices.Add(trVertex);
            roomVerticesDictionary.Add(trVertex.GetHashCode(), vertexIndex);
            return vertexIndex;
        }

        private static int GetOrAddVertex(Room room, Dictionary<int, int> roomVerticesDictionary, List<TombEngineVertex> roomVertices, TombEngineVertex trVertex)
        {
            // Do the check here, so we can save some time with unuseful calculations
            int vertexIndex;
            if (roomVerticesDictionary.TryGetValue(trVertex.GetHashCode(), out vertexIndex))
                return vertexIndex;

            // Add vertex
            vertexIndex = (ushort)roomVertices.Count;
            roomVerticesDictionary.Add(trVertex.GetHashCode(), vertexIndex);
            roomVertices.Add(trVertex);
            return vertexIndex;
        }

        private void ConvertLights(Room room, TombEngineRoom newRoom)
        {
            int lightCount = 0;

            foreach (var light in room.Objects.OfType<LightInstance>())
            {
                if (!light.Enabled || !light.IsDynamicallyUsed || light.Type == LightType.FogBulb)
                    continue;

                if (light.Intensity == 0 || light.Color.X == 0 && light.Color.Y == 0 && light.Color.Z == 0)
                    continue;

                lightCount += 1;

                var newLight = new TombEngineRoomLight
                {
                    Position = new VectorInt3(
                        (int)Math.Round(newRoom.Info.X + light.Position.X),
                        (int)-Math.Round(light.Position.Y + room.WorldPos.Y),
                        (int)Math.Round(newRoom.Info.Z + light.Position.Z)),
                    Color = light.Color,
                    Intensity = light.Intensity
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
                        newLight.Direction.X = -spotDirection.X;
                        newLight.Direction.Y = spotDirection.Y;
                        newLight.Direction.Z = -spotDirection.Z;
                        break;
                    case LightType.Sun:
                        newLight.LightType = 0;
                        newLight.In = 0;
                        newLight.Out = 0;
                        newLight.Length = 0;
                        newLight.CutOff = 0;
                        Vector3 sunDirection = light.GetDirection();
                        newLight.Direction.X = -sunDirection.X;
                        newLight.Direction.Y = sunDirection.Y;
                        newLight.Direction.Z = -sunDirection.Z;
                        break;
                    case LightType.FogBulb:
                        newLight.LightType = 4;
                        newLight.In  = light.InnerRange * Level.WorldUnit;
                        newLight.Out = light.OuterRange * Level.WorldUnit;
                        break;
                    case LightType.Effect:
                        continue;
                    default:
                        throw new Exception("Unknown light type '" + light.Type + "' encountered.");
                }

                newRoom.Lights.Add(newLight);
            }
        }

        private void ConvertSectors(Room room, TombEngineRoom newRoom)
        {
            newRoom.Sectors = new TombEngineRoomSector[room.NumXSectors * room.NumZSectors];
            newRoom.AuxSectors = new TrSectorAux[room.NumXSectors, room.NumZSectors];

            for (var z = 0; z < room.NumZSectors; z++)
            {
                for (var x = 0; x < room.NumXSectors; x++)
                {
                    var block = room.Blocks[x, z];
                    var sector = new TombEngineRoomSector();
                    var aux = new TrSectorAux();

                    sector.FloorCollision = new TombEngineCollisionInfo();
                    sector.FloorCollision.Portals = new int[2] {-1, -1};
                    sector.FloorCollision.Planes = new Vector3[2];
                    sector.CeilingCollision = new TombEngineCollisionInfo();
                    sector.CeilingCollision.Portals = new int[2] {-1, -1};
                    sector.CeilingCollision.Planes = new Vector3[2];
                    sector.WallPortal = -1;
                    sector.StepSound = (int)GetTextureSound(room, x, z);
                    sector.BoxIndex = -1;
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
                        sector.RoomBelow = -1;
                        aux.FloorPortal = null;
                    }

                    if (room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType != Room.RoomConnectionType.NoPortal)
                        sector.RoomAbove = (byte)_roomsRemappingDictionary[block.CeilingPortal.AdjoiningRoom];
                    else
                        sector.RoomAbove = -1;

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

        private void ConvertPortals(Room room, IEnumerable<PortalInstance> portals, TombEngineRoom newRoom)
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

        private void MatchDoorShades(List<TombEngineRoom> roomList, TombEngineRoom room, bool grayscale, bool flipped)
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
                            Position = new VectorInt3((int)v1.Position.X + room.Info.X, (int)v1.Position.Y, (int)v1.Position.Z + room.Info.Z)
                        };

                        if (v1.Position.X >= x1 && v1.Position.X <= x2)
                            if (v1.Position.Y >= y1 && v1.Position.Y <= y2)
                                if (v1.Position.Z >= z1 && v1.Position.Z <= z2)
                                {
                                    v1.IsOnPortal = true;
                                    room.Vertices[i] = v1;

                                    int otherX = (int)v1.Position.X + room.Info.X - otherRoom.Info.X;
                                    int otherY = (int)v1.Position.Y;
                                    int otherZ = (int)v1.Position.Z + room.Info.Z - otherRoom.Info.Z;

                                    for (int j = 0; j < otherRoom.Vertices.Count; j++)
                                    {
                                        var v2 = otherRoom.Vertices[j];
                                        Vector3 refColor;
                                        var isPresentInLookup = _vertexColors.TryGetValue(sig, out refColor);
                                        if (!isPresentInLookup) refColor = v1.Color;

                                        if (room.Info.X + v1.Position.X == otherRoom.Info.X + v2.Position.X &&
                                            v1.Position.Y == v2.Position.Y &&
                                            room.Info.Z + v1.Position.Z == otherRoom.Info.Z + v2.Position.Z)
                                        {
                                            Vector3 newColor;

                                            // NOTE: We DON'T INTERPOLATE colours of both rooms in case we're dealing with alternate room and matched room
                                            // isn't alternate room itself. Instead, we simply copy vertex colour from matched base room.
                                            // This way we don't get sharp-cut half-transitioned vertex colour.

                                            if (flipped && otherRoom.AlternateKind != AlternateKind.AlternateRoom)
                                            {
                                                var baseSig = new ShadeMatchSignature() { IsWater = sig.IsWater, AlternateGroup = -1, Position = sig.Position };
                                                if (!_vertexColors.TryGetValue(baseSig, out newColor)) newColor = v2.Color;
                                            }
                                            else
                                            {
                                                newColor = (v2.Color + refColor) / 2.0f;
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

        private TombEngineBucket GetOrAddBucket(int texture, byte blendMode, bool animated, int sequence, Dictionary<TombEngineMaterial, TombEngineBucket> buckets)
        {
            var material = new TombEngineMaterial
            {
                Texture = texture,
                BlendMode = blendMode,
                Animated = animated,
                AnimatedSequence = sequence
            };

            if (!buckets.ContainsKey(material))
                buckets.Add(material, new TombEngineBucket { Material = material });

            return buckets[material];
        }

        private void PrepareRoomsBuckets()
        {
            for (int i = 0; i < _tempRooms.Count; i++)
                PrepareRoomBuckets(_tempRooms.ElementAt(i).Value);
        }

        private void PrepareRoomBuckets(TombEngineRoom room)
        {
            // Build buckets and assign texture coordinates
            var textures = _textureInfoManager.GetObjectTextures();
            room.Buckets = new Dictionary<TombEngineMaterial, TombEngineBucket>(new TombEngineMaterial.TombEngineMaterialComparer());
            foreach (var poly in room.Polygons)
            {
                poly.AnimatedSequence = -1;
                poly.AnimatedFrame = -1;

                if (poly.Animated)
                {
                    var animInfo = _textureInfoManager.GetAnimatedTexture(poly.TextureId);
                    if (animInfo != null)
                    {
                        poly.AnimatedSequence = animInfo.Item1;
                        poly.AnimatedFrame = animInfo.Item2;
                        Console.WriteLine(animInfo.Item1);
                    }
                }

                var bucket = GetOrAddBucket(textures[poly.TextureId].AtlasIndex, poly.BlendMode, poly.Animated, poly.AnimatedSequence, room.Buckets);

                var texture = textures[poly.TextureId];

                // We output only triangles, no quads anymore
                if (poly.Shape == TombEnginePolygonShape.Quad)
                {
                    for (int n = 0; n < 4; n++)
                    {
                        poly.TextureCoordinates.Add(texture.TexCoordFloat[n]);
                        poly.Normals.Add(poly.Normal);
                        poly.Tangents.Add(Vector3.Zero);
                        poly.Bitangents.Add(Vector3.Zero);
                    }

                    bucket.Polygons.Add(poly);

                }
                else
                {
                    for (int n = 0; n < 3; n++)
                    {
                        poly.TextureCoordinates.Add(texture.TexCoordFloat[n]);
                        poly.Normals.Add(poly.Normal);
                        poly.Tangents.Add(Vector3.Zero);
                        poly.Bitangents.Add(Vector3.Zero);
                    }

                    bucket.Polygons.Add(poly);
                }
            }

            // Calculate tangents and bitangents
            for (int i = 0; i < room.Vertices.Count; i++)
            {
                var vertex = room.Vertices[i];
                var polygons = vertex.Polygons;

                for (int j = 0; j < polygons.Count; j++)
                {
                    var poly = polygons[j];

                    var e1 = room.Vertices[poly.Polygon.Indices[1]].Position - room.Vertices[poly.Polygon.Indices[0]].Position;
                    var e2 = room.Vertices[poly.Polygon.Indices[2]].Position - room.Vertices[poly.Polygon.Indices[0]].Position;

                    var uv1 = poly.Polygon.TextureCoordinates[1] - poly.Polygon.TextureCoordinates[0];
                    var uv2 = poly.Polygon.TextureCoordinates[2] - poly.Polygon.TextureCoordinates[0];

                    float r = 1.0f / (uv1.X * uv2.Y - uv1.Y * uv2.X);
                    poly.Polygon.Tangent = Vector3.Normalize((e1 * uv2.Y - e2 * uv1.Y) * r);
                    poly.Polygon.Bitangent = Vector3.Cross(poly.Polygon.Normal, poly.Polygon.Tangent); // Vector3.Normalize((e2 * uv1.X - e1 * uv2.X) * r);
                }
            }

            // Average everything
            for (int i = 0; i < room.Vertices.Count; i++)
            {
                var vertex = room.Vertices[i];
                var polygons = vertex.Polygons;

                //TODO: tangents and bitangents must be averaged? It seems that no
                var normal = Vector3.Zero;
                //var tangent = Vector3.Zero;
                //var bitangent = Vector3.Zero;

                for (int j = 0; j < polygons.Count; j++)
                {
                    var poly = polygons[j];
                    normal += poly.Polygon.Normal;
                    //tangent += poly.Polygon.Tangent;
                    //bitangent += poly.Polygon.Bitangent;
                }

                if (polygons.Count > 0)
                {
                    normal = Vector3.Normalize(normal / (float)polygons.Count);
                    //tangent = Vector3.Normalize(tangent / (float)polygons.Count);
                    //bitangent = Vector3.Normalize(bitangent / (float)polygons.Count);
                }

                for (int j = 0; j < polygons.Count; j++)
                {
                    var poly = polygons[j];

                    // TODO: for now we smooth all normals
                    for (int k = 0; k < poly.Polygon.Indices.Count; k++)
                    {
                        int index = poly.Polygon.Indices[k];
                        if (index == i)
                        {
                            poly.Polygon.Normals[k] = normal;
                            poly.Polygon.Tangents[k] = poly.Polygon.Tangent;
                            poly.Polygon.Bitangents[k] = poly.Polygon.Bitangent;
                            break;
                        }
                    }
                }
            }
        }
    }
}
