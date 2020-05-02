using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers
{
    public partial class LevelCompilerClassicTR
    {
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
                    _tempRooms[r].WriteTr5(writer);
                }

                // Write floordata
                var numFloorData = (uint)_floorData.Count;
                writer.Write(numFloorData);
                writer.WriteBlockArray(_floorData);

                // Write meshes
                var offset = writer.BaseStream.Position;

                const int numMeshData = 0;
                writer.Write(numMeshData);
                var totalMeshSize = 0;

                for (var i = 0; i < _meshes.Count; i++)
                {
                    var meshSize = _meshes[i].WriteTr4AndTr5(writer);
                    totalMeshSize += (int)meshSize;
                }

                var offset2 = writer.BaseStream.Position;
                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                uint meshDataSize = (uint)((offset2 - offset - 4) / 2);

                // Save the size of the meshes
                writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(meshDataSize);
                writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                // Write mesh pointers
                writer.Write((uint)_meshPointers.Count);
                writer.WriteBlockArray(_meshPointers);

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

                _textureInfoManager.WriteTextureInfos(writer, _level);

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

                    ReportProgress(91, "Writing textures");

                    // The room texture tile count currently also currently contains the wad textures
                    // But lets not bother with those fields too much since they only matter when bump maps are used and we don't use them.
                    writer.Write((ushort)_textureInfoManager.NumRoomPages);
                    writer.Write((ushort)_textureInfoManager.NumObjectsPages);
                    // Bump map pages must be multiplied by 2 or tile index will be wrong
                    writer.Write((ushort)(_textureInfoManager.NumBumpPages * 2));

                    // Compress data
                    ReportProgress(95, "Compressing data");

                    byte[] texture32 = null;
                    int texture32UncompressedSize = -1;
                    byte[] texture16 = null;
                    int texture16UncompressedSize = -1;
                    byte[] textureMisc = null;
                    int textureMiscUncompressedSize = -1;
                    byte[] geometryData = geometryDataBuffer;
                    int geometryDataUncompressedSize = geometryData.Length;

                    using (Task Texture32task = Task.Factory.StartNew(() =>
                    {
                        texture32 = ZLib.CompressData(_texture32Data);
                        texture32UncompressedSize = _texture32Data.Length;
                    }))
                    using (Task Texture16task = Task.Factory.StartNew(() =>
                    {
                        byte[] texture16Data = PackTextureMap32To16Bit(_texture32Data);
                        texture16 = ZLib.CompressData(texture16Data);
                        texture16UncompressedSize = texture16Data.Length;
                    }))
                    using (Task textureMiscTask = Task.Factory.StartNew(() =>
                    {
                        Stream textureMiscData = PrepareFontAndSkyTexture();
                        textureMisc = ZLib.CompressData(textureMiscData);
                        textureMiscUncompressedSize = (int)textureMiscData.Length;
                    }))

                        Task.WaitAll(Texture32task, Texture16task, textureMiscTask);

                    // Write data
                    ReportProgress(97, "Writing compressed data to file.");

                    writer.Write(texture32UncompressedSize);
                    writer.Write(texture32.Length);
                    writer.Write(texture32);

                    writer.Write(texture16UncompressedSize);
                    writer.Write(texture16.Length);
                    writer.Write(texture16);

                    writer.Write(textureMiscUncompressedSize);
                    writer.Write(textureMisc.Length);
                    writer.Write(textureMisc);

                    writer.Write((ushort)_level.Settings.Tr5LaraType);
                    writer.Write((ushort)_level.Settings.Tr5WeatherType);

                    for (var i = 0; i < 28; i++)
                        writer.Write((byte)0);

                    // In TR5 geometry data is not compressed
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
                                { } else continue;

                            using (var extraRoomDataChunk = chunkIO.WriteChunk(Tr5MainExtraRoomData))
                            {
                                // First and only param after signature is internal room number
                                chunkIO.Raw.Write(currRoom);

                                using (var volumeListChunk = chunkIO.WriteChunk(Tr5MainChunkVolumeList))
                                {
                                    var trRoom = _tempRooms[r];

                                    foreach (var vol in r.Volumes)
                                    {
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

                                            var min = vol.Position - (vol.Size / 2.0f);
                                            var max = vol.Position + (vol.Size / 2.0f);
                                            var rad = vol.Size.X / 2.0f;

                                            var bv = new t5m_bounding_volume()
                                            {
                                                VolumeType = (ushort)vol.Shape,
                                                Activators = (byte)vol.Activators,
                                                X = (int)Math.Round(trRoom.Info.X + vol.Position.X),
                                                Y = (int)-Math.Round(r.WorldPos.Y + vol.Position.Y + (vol.Size.Y / 2.0f)),
                                                Z = (int)Math.Round(trRoom.Info.Z + vol.Position.Z),
                                                RotationY = (ushort)Math.Max(0, Math.Min(ushort.MaxValue,
                                                                    Math.Round(vol.RotationY * (65536.0 / 360.0)))),
                                                RotationX = (ushort)Math.Max(0, Math.Min(ushort.MaxValue,
                                                                    Math.Round(vol.RotationX * (65536.0 / 360.0)))),
                                                Radius = (int)rad,
                                                Bounding_box = new tr_bounding_box()
                                                {
                                                    X1 = (short)min.X,
                                                    Y1 = (short)min.Y,
                                                    Z1 = (short)min.Z,
                                                    X2 = (short)max.X,
                                                    Y2 = (short)max.Y,
                                                    Z2 = (short)max.Z
                                                },
                                                ScriptIndex = scriptIndex
                                            };

                                            chunkIO.Raw.Write(bv.VolumeType);
                                            chunkIO.Raw.Write(bv.Activators);
                                            chunkIO.Raw.Write(bv.X);
                                            chunkIO.Raw.Write(bv.Y);
                                            chunkIO.Raw.Write(bv.Z);
                                            chunkIO.Raw.Write(bv.RotationY);
                                            chunkIO.Raw.Write(bv.RotationX);
                                            chunkIO.Raw.Write(bv.Radius);
                                            chunkIO.Raw.Write(bv.Bounding_box.X1);
                                            chunkIO.Raw.Write(bv.Bounding_box.Y1);
                                            chunkIO.Raw.Write(bv.Bounding_box.Z1);
                                            chunkIO.Raw.Write(bv.Bounding_box.X2);
                                            chunkIO.Raw.Write(bv.Bounding_box.Y2);
                                            chunkIO.Raw.Write(bv.Bounding_box.Z2);
                                            chunkIO.Raw.Write(bv.ScriptIndex);
                                        }
                                    }
                                }
                            }
                        }

                        string indent = "    ";

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

                                        string functionCode =
                                            "volscripts[" + i + "].OnEnter  = function(activator) \n" +
                                                indent + script.OnEnter.Replace("\n", "\n" + indent) + "\n"  + "end;" + "\n\n" +
                                            "volscripts[" + i + "].OnInside = function(activator) \n" +
                                                indent + script.OnInside.Replace("\n", "\n" + indent) + "\n" + "end;" + "\n\n" +
                                            "volscripts[" + i + "].OnLeave  = function(activator) \n" +
                                                indent + script.OnLeave.Replace("\n", "\n" + indent) + "\n"  + "end;" + "\n\n";

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
