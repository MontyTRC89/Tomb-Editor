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
        // Collections for LUA triggers
        private List<TriggerInstance> _luaTriggers;
        private Dictionary<int, int> _luaIdToItems;

        private static readonly ChunkId Tr5MainChunkTriggersList = ChunkId.FromString("Tr5Triggers");
        private static readonly ChunkId Tr5MainChunkTrigger = ChunkId.FromString("Tr5Trigger");
        private static readonly ChunkId Tr5MainChunkLuaIds = ChunkId.FromString("Tr5LuaIds");
        private static readonly ChunkId Tr5MainChunkLuaId = ChunkId.FromString("Tr5LuaId");

        private void PrepareLuaTriggers()
        {
            _luaTriggers = new List<TriggerInstance>();
            foreach (var room in _level.Rooms)
                if (room != null)
                    foreach (var trigger in room.Triggers)
                        if (trigger.TargetType == TriggerTargetType.LuaScript)
                            _luaTriggers.Add(trigger);
        }

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
                _objectTextureManager.WriteAnimatedTexturesForTr4(writer);

                // Write object textures
                writer.Write(checked((byte)_objectTextureManager.UvRotateCount));
                writer.Write(new byte[] { 0x54, 0x45, 0x58, 0x00 });

                _objectTextureManager.WriteObjectTextures(writer, _level);

                // Write items and AI objects
                writer.Write((uint)_items.Count);
                writer.WriteBlockArray(_items);

                writer.Write((uint)_aiItems.Count);
                writer.WriteBlockArray(_aiItems);

                // Write sound meta data
                _soundManager.WriteSoundMetadata(writer);

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
                    // But lets not bother with those fielsd too much since they only matter when bump maps are used and we don't use them.
                    writer.Write((ushort)(_texture32Data.GetLength(0) / (256 * 256 * 4)));
                    writer.Write((ushort)0);
                    writer.Write((ushort)0);

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
                    _soundManager.WriteSoundData(writer);

                    // Write extra data
                    using (var ms = new MemoryStream())
                    {
                        var chunkIO = new ChunkWriter(new byte[] { 0x54, 0x52, 0x35, 0x4D }, new BinaryWriterFast(ms));

                        chunkIO.WriteChunkWithChildren(Tr5MainChunkTriggersList, () =>
                        {
                            foreach (var trigger in _luaTriggers)
                            {
                                chunkIO.WriteChunk(Tr5MainChunkTrigger, () =>
                                {
                                    string functionName = "Trigger_" + _luaTriggers.IndexOf(trigger);
                                    string functionCode = "function " + functionName + "()\n" +
                                                          trigger.LuaScript + "\n" +
                                                          "end\n";

                                    chunkIO.Raw.WriteStringUTF8(functionName);
                                    chunkIO.Raw.WriteStringUTF8(functionCode);
                                });
                            }
                        });

                        chunkIO.WriteChunkWithChildren(Tr5MainChunkLuaIds, () =>
                        {
                            for (int i = 0; i < _luaIdToItems.Count; i++)
                            {
                                chunkIO.WriteChunk(Tr5MainChunkLuaId, () =>
                                {
                                    chunkIO.Raw.Write(_luaIdToItems.ElementAt(i).Key);
                                    chunkIO.Raw.Write(_luaIdToItems.ElementAt(i).Value);
                                });
                            }
                        });

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
