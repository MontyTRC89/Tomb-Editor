using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.TR5Main
{
    public sealed partial class LevelCompilerTR5Main
    {
        private const string _indent = "    ";

        // Collections for volumes
        private List<VolumeScriptInstance> _volumeScripts;
        private Dictionary<int, int> _luaIdToItems;

        /**/ private static readonly ChunkId Tr5MainExtraRoomData = ChunkId.FromString("Tr5ExtraRoomData");
        /***/ private static readonly ChunkId Tr5MainChunkVolumeList = ChunkId.FromString("Tr5Volumes");
        /***/ private static readonly ChunkId Tr5MainChunkVolume = ChunkId.FromString("Tr5Volume");
        /**/ private static readonly ChunkId Tr5MainExtraData = ChunkId.FromString("Tr5ExtraData");
        /***/ private static readonly ChunkId Tr5MainChunkVolumeScriptList = ChunkId.FromString("Tr5VolumeScripts");
        /***/ private static readonly ChunkId Tr5MainChunkVolumeScript = ChunkId.FromString("Tr5VolumeScript");
        /***/ private static readonly ChunkId Tr5MainChunkLuaIds = ChunkId.FromString("Tr5LuaIds");
        /***/ private static readonly ChunkId Tr5MainChunkLuaId = ChunkId.FromString("Tr5LuaId");
        
        private void WriteLevelTr5Main()
        {
            // Now begin to compile the geometry block in a MemoryStream
            byte[] geometryDataBuffer;
            using (var geometryDataStream = new MemoryStream())
            {
                var writer = new BinaryWriterEx(geometryDataStream); // Don't dispose
                ReportProgress(80, "Writing geometry data to memory buffer");

                const int filler = 0;
                writer.Write(filler);

                var numRooms = (uint)_level.Rooms.Count(r => r != null);
                writer.Write(numRooms);

                foreach (var r in _level.Rooms.Where(r => r != null))
                {
                    _tempRooms[r].Write(writer);
                }

                // Write floordata
                var numFloorData = (uint)_floorData.Count;
                writer.Write(numFloorData);
                writer.WriteBlockArray(_floorData);

                // Write meshes
                writer.Write(_meshes.Count);
                foreach (var mesh in _meshes)
                {
                    writer.Write(mesh.Sphere.Center);
                    writer.Write(mesh.Sphere.Radius);

                    writer.Write(mesh.Vertices.Count);
                    foreach (var vertex in mesh.Vertices)
                    {
                        writer.Write(vertex.Position);
                        writer.Write(vertex.Normal);
                        writer.Write(vertex.TextureCoords);
                        writer.Write(vertex.Color);
                        writer.Write(vertex.Bone);
                        writer.Write(vertex.Index);
                    }

                    writer.Write(mesh.Buckets.Count);
                    foreach (var bucket in mesh.Buckets.Values)
                    {
                        writer.Write(bucket.Material.Texture);
                        writer.Write(bucket.Material.BlendMode);
                        writer.Write(bucket.Material.Animated);
                        writer.Write(bucket.Indices.Count);
                        foreach (var index in bucket.Indices)
                            writer.Write(index);
                    }
                }

                // Write animations' data
                writer.Write((uint)_animations.Count);
                foreach (var anim in _animations)
                    anim.Write(writer, _level);

                writer.Write((uint)_stateChanges.Count);
                writer.WriteBlockArray(_stateChanges);

                writer.Write((uint)_animDispatches.Count);
                writer.WriteBlockArray(_animDispatches);

                writer.Write((uint)_animCommands.Count);
                writer.WriteBlockArray(_animCommands);

                writer.Write((uint)_meshTrees.Count);
                writer.WriteBlockArray(_meshTrees);

                writer.Write((uint)_frames.Count);
                writer.WriteBlockArray(_frames);

                writer.Write((uint)_moveables.Count);
                for (var k = 0; k < _moveables.Count; k++)
                {
                    writer.WriteBlock(_moveables[k]);
                    writer.Write((ushort)0xfeff);
                }

                writer.Write((uint)_staticMeshes.Count);
                writer.WriteBlockArray(_staticMeshes);

                // SPR block
                writer.WriteBlockArray(new byte[] { 0x53, 0x50, 0x52, 0x00 });

                writer.Write((uint)_spriteTextures.Count);
                writer.WriteBlockArray(_spriteTextures);

                writer.Write((uint)_spriteSequences.Count);
                writer.WriteBlockArray(_spriteSequences);

                // Write camera, flyby and sound sources
                writer.Write((uint)_cameras.Count);
                writer.WriteBlockArray(_cameras);

                writer.Write((uint)_flyByCameras.Count);
                writer.WriteBlockArray(_flyByCameras);

                writer.Write((uint)_soundSources.Count);
                writer.WriteBlockArray(_soundSources);

                // Write pathfinding data
                writer.Write((uint)_boxes.Length);
                writer.WriteBlockArray(_boxes);

                writer.Write((uint)_overlaps.Length);
                writer.WriteBlockArray(_overlaps);

                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].GroundZone1_Normal);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].GroundZone2_Normal);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].GroundZone3_Normal);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].GroundZone4_Normal);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].FlyZone_Normal);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].GroundZone1_Alternate);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].GroundZone2_Alternate);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].GroundZone3_Alternate);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].GroundZone4_Alternate);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].FlyZone_Alternate);

                // Write animated textures
                _textureInfoManager.WriteAnimatedTextures(writer);

                // Write object textures
                writer.Write(checked((byte)_textureInfoManager.UvRotateCount));
                writer.Write(new byte[] { 0x54, 0x45, 0x58, 0x00 });

                _textureInfoManager.WriteTextureInfosTR5Main(writer, _level);

                // Write items and AI objects
                writer.Write((uint)_items.Count);
                writer.WriteBlockArray(_items);

                writer.Write((uint)_aiItems.Count);
                writer.WriteBlockArray(_aiItems);

                // Write sound meta data
                PrepareSoundsData();
                WriteSoundMetadata(writer);

                // Finish it
                writer.Write((ushort)0xcdcd);
                writer.Write((ushort)0xcdcd);
                writer.Write((ushort)0xcdcd);

                geometryDataBuffer = geometryDataStream.ToArray();
            }

            using (var fs = new FileStream(_dest, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var writer = new BinaryWriterEx(fs))
                {
                    ReportProgress(90, "Writing final level");
                    writer.WriteBlockArray(new byte[] { 0x54, 0x52, 0x34, 0x00 });

                    // Compress data
                    ReportProgress(95, "Compressing data");

                    // Rooms atlas
                    writer.Write(_textureInfoManager.RoomsAtlas.Count);
                    foreach (var atlas in _textureInfoManager.RoomsAtlas)
                    {
                        writer.Write(atlas.Width);
                        writer.Write(atlas.Height);
                        using (var ms = new MemoryStream())
                        {
                            atlas.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            var output = RemoveColorChunks(ms);
                            writer.Write((int)output.Length);
                            writer.Write(output.ToArray());
                        }
                    }

                    // Moveables atlas
                    writer.Write(_textureInfoManager.MoveablesAtlas.Count);
                    foreach (var atlas in _textureInfoManager.MoveablesAtlas)
                    {
                        writer.Write(atlas.Width);
                        writer.Write(atlas.Height);
                        using (var ms = new MemoryStream())
                        {
                            atlas.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            var output = RemoveColorChunks(ms);
                            writer.Write((int)output.Length);
                            writer.Write(output.ToArray());
                        }
                    }

                    // Statics atlas
                    writer.Write(_textureInfoManager.StaticsAtlas.Count);
                    foreach (var atlas in _textureInfoManager.StaticsAtlas)
                    {
                        writer.Write(atlas.Width);
                        writer.Write(atlas.Height);
                        using (var ms = new MemoryStream())
                        {
                            atlas.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            var output = RemoveColorChunks(ms);
                            writer.Write((int)output.Length);
                            writer.Write(output.ToArray());
                        }
                    }

                    // Sprites textures
                    writer.Write(_spritesTexturesPages.Count);
                    foreach (var atlas in _spritesTexturesPages)
                    {
                        writer.Write(atlas.Width);
                        writer.Write(atlas.Height);
                        using (var ms = new MemoryStream())
                        {
                            atlas.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            var output = RemoveColorChunks(ms);
                            writer.Write((int)output.Length);
                            writer.Write(output.ToArray());
                        }
                    }

                    // Font and sky textures
                    Stream textureMiscData = PrepareFontAndSkyTexture();
                    ImageC misc = ImageC.FromStreamRaw(textureMiscData, 256, 512);
                    using (var ms = new MemoryStream())
                    {
                        misc.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        writer.Write(256);
                        writer.Write(512);
                        var output = RemoveColorChunks(ms);
                        writer.Write((int)output.Length);
                        writer.Write(output.ToArray());
                    }

                    // Write data
                    ReportProgress(97, "Writing compressed data to file.");
                                       
                    writer.Write((ushort)_level.Settings.Tr5LaraType);
                    writer.Write((ushort)_level.Settings.Tr5WeatherType);

                    for (var i = 0; i < 28; i++)
                        writer.Write((byte)0);

                    // In TR5 geometry data is not compressed
                    byte[] geometryData = geometryDataBuffer;
                    int geometryDataUncompressedSize = geometryData.Length;

                    writer.Write(geometryDataUncompressedSize);
                    writer.Write(geometryDataUncompressedSize);
                    writer.Write(geometryData);

                    ReportProgress(98, "Writing WAVE sounds");
                    WriteSoundData(writer);

                    // Write extra data
                    _volumeScripts = new List<VolumeScriptInstance>();

                    using (var ms = new MemoryStream())
                    {
                        var chunkIO = new ChunkWriter(new byte[] { 0x54, 0x52, 0x35, 0x4D }, new BinaryWriterFast(ms));

                        int currRoom = 0;
                        foreach (var r in _level.Rooms.Where(r => r != null))
                        {
                            // Add further extra data conditions here, otherwise compiler will skip this room altogether
                            if (r.Volumes.Count() > 0)
                                currRoom++;
                            else
                            {
                                currRoom++;
                                continue;
                            }

                            using (var extraRoomDataChunk = chunkIO.WriteChunk(Tr5MainExtraRoomData))
                            {
                                // First and only param after signature is internal room number
                                chunkIO.Raw.Write(currRoom);

                                using (var volumeListChunk = chunkIO.WriteChunk(Tr5MainChunkVolumeList))
                                {
                                    var trRoom = _tempRooms[r];

                                    foreach (var vol in r.Volumes)
                                        using (var volumeChunk = chunkIO.WriteChunk(Tr5MainChunkVolume))
                                        {
                                            int scriptIndex = 0;
                                            if (_volumeScripts.Contains(vol.Scripts))
                                                scriptIndex = _volumeScripts.IndexOf(vol.Scripts);
                                            else
                                            {
                                                _volumeScripts.Add(vol.Scripts);
                                                scriptIndex = _volumeScripts.Count - 1;
                                            }

                                            // FIXME is it needed?
                                            int add = 0;
                                            if (vol is BoxVolumeInstance)
                                                add = (int)((vol as BoxVolumeInstance).Size.Y / 2.0f);

                                            var X = (int)Math.Round(trRoom.Info.X + vol.Position.X);
                                            var Y = (int)-Math.Round(r.WorldPos.Y + vol.Position.Y + add); 
                                            var Z = (int)Math.Round(trRoom.Info.Z + vol.Position.Z);

                                            if (vol is BoxVolumeInstance)
                                                chunkIO.Raw.Write(0);
                                            else if (vol is SphereVolumeInstance)
                                                chunkIO.Raw.Write(1);
                                            else if (vol is PrismVolumeInstance)
                                                chunkIO.Raw.Write(2);

                                            chunkIO.Raw.Write(X);
                                            chunkIO.Raw.Write(Y);
                                            chunkIO.Raw.Write(Z);
                                            chunkIO.Raw.Write((short)vol.Activators);
                                            chunkIO.Raw.Write(scriptIndex);

                                            if (vol is BoxVolumeInstance)
                                            {
                                                var bv = (BoxVolumeInstance)vol;
                                                var min = vol.Position - (bv.Size / 2.0f);
                                                var max = vol.Position + (bv.Size / 2.0f);
                                                var rotY = (ushort)Math.Max(0, Math.Min(ushort.MaxValue,
                                                                   Math.Round(bv.RotationY * (65536.0 / 360.0))));
                                                var rotX = (ushort)Math.Max(0, Math.Min(ushort.MaxValue,
                                                                   Math.Round(bv.RotationX * (65536.0 / 360.0))));

                                                chunkIO.Raw.Write(rotY);
                                                chunkIO.Raw.Write(rotX);
                                                chunkIO.Raw.Write((short)min.X);
                                                chunkIO.Raw.Write((short)min.Y);
                                                chunkIO.Raw.Write((short)min.Z);
                                                chunkIO.Raw.Write((short)max.X);
                                                chunkIO.Raw.Write((short)max.Y);
                                                chunkIO.Raw.Write((short)max.Z);
                                            }
                                            else if (vol is SphereVolumeInstance)
                                                chunkIO.Raw.Write((vol as SphereVolumeInstance).Size);
                                            else if (vol is PrismVolumeInstance)
                                            {
                                                var pv = (PrismVolumeInstance)vol;
                                                chunkIO.Raw.Write(pv.RotationY);
                                                chunkIO.Raw.Write(pv.Size);
                                            }
                                        }
                                }
                            }
                        }

                        /*
                        using (var extraDataChunk = chunkIO.WriteChunk(Tr5MainExtraData))
                        {
                            using (var volScriptListChunk = chunkIO.WriteChunk(Tr5MainChunkVolumeScriptList))
                            {
                                for (int i = 0; i < _volumeScripts.Count; i++)
                                {
                                    var script = _volumeScripts[i];
                                    using (var volScriptChunk = chunkIO.WriteChunk(Tr5MainChunkVolumeScript))
                                    {
                                        chunkIO.Raw.WriteStringUTF8(script.Name);

                                        string onEnter = string.Empty;
                                        string onInside = string.Empty;
                                        string onLeave = string.Empty;

                                        if (script.OnEnter.Trim().Length > 0)
                                            onEnter = "volscripts[" + i + "].OnEnter  = function(activator) \n" +
                                                        _indent + script.OnEnter.Replace("\n", "\n" + _indent) + "\n" + "end;";

                                        if (script.OnInside.Trim().Length > 0)
                                            onInside = "volscripts[" + i + "].OnInside = function(activator) \n" +
                                                        _indent + script.OnInside.Replace("\n", "\n" + _indent) + "\n" + "end;";

                                        if (script.OnLeave.Trim().Length > 0)
                                            onLeave  = "volscripts[" + i + "].OnLeave = function(activator) \n" +
                                                        _indent + script.OnLeave.Replace("\n", "\n" + _indent) + "\n" + "end;";

                                        string functionCode =
                                            onEnter  + (string.IsNullOrEmpty(onEnter)  ? string.Empty : "\n\n") +
                                            onInside + (string.IsNullOrEmpty(onInside) ? string.Empty : "\n\n") +
                                            onLeave  + (string.IsNullOrEmpty(onLeave)  ? string.Empty : "\n\n") ;

                                        chunkIO.Raw.WriteStringUTF8(functionCode);
                                    }
                                }
                            }

                            using (var chunkLuaIds = chunkIO.WriteChunk(Tr5MainChunkLuaIds))
                            {
                                for (int i = 0; i < _luaIdToItems.Count; i++)
                                {
                                    chunkIO.WriteChunk(Tr5MainChunkLuaId, () =>
                                    {
                                        chunkIO.Raw.Write(_luaIdToItems.ElementAt(i).Key);
                                        chunkIO.Raw.Write(_luaIdToItems.ElementAt(i).Value);
                                    });
                                }
                            }
                        }
                        */

                        chunkIO.Raw.Flush();

                        writer.Write((int)(ms.Length + 4));
                        writer.Write((int)(ms.Length + 4));
                        writer.Write(ms.ToArray(), 0, (int)ms.Length);
                        writer.Write((int)0);
                    }

                    ReportProgress(99, "Done");   
                }
            }
        }
    }
}
