using System.IO;
using System.Linq;
using TombLib.IO;

namespace TombEditor.Compilers
{
    public sealed partial class LevelCompilerTr4
    {
        private void WriteLevelTr4()
        {
            var wad = _level.Wad.OriginalWad;

            // Now begin to compile the geometry block in a MemoryStream
            using (var writer = new BinaryWriterEx(new FileStream("temp.bin", FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                ReportProgress(85, "Writing geometry data to memory buffer");

                const int filler = 0;
                writer.Write(filler);

                var numRooms = (ushort) _level.Rooms.Count(r => r != null);
                writer.Write(numRooms);

                foreach (var r in _level.Rooms.Where(r => r != null))
                {
                    _tempRooms[r].Write(writer);
                }

                // Write floordata
                var numFloorData = (uint) _floorData.Length;
                writer.Write(numFloorData);
                writer.WriteBlockArray(_floorData);

                // Write meshes
                var offset = writer.BaseStream.Position;

                const int numMeshData = 0;
                writer.Write(numMeshData);
                var totalMeshSize = 0;

                for (var i = 0; i < _meshes.Length; i++)
                {
                    var meshSize = _meshes[i].WriteTr4(writer);

                    for (var n = 0; n < _meshPointers.Length; n++)
                    {
                        if (wad.HelperPointers[n] == i)
                        {
                            _meshPointers[n] = (uint) totalMeshSize;
                        }
                    }

                    totalMeshSize += (int) meshSize;
                }

                var offset2 = writer.BaseStream.Position;
                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                uint meshDataSize = (uint) ((offset2 - offset - 4) / 2);

                // Save the size of the meshes
                writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(meshDataSize);
                writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                // Write mesh pointers
                writer.Write((uint) _meshPointers.Length);
                writer.WriteBlockArray(_meshPointers);

                // Write animations' data
                writer.Write((uint) _animations.Length);
                writer.WriteBlockArray(_animations);

                writer.Write((uint) _stateChanges.Length);
                writer.WriteBlockArray(_stateChanges);

                writer.Write((uint) _animDispatches.Length);
                writer.WriteBlockArray(_animDispatches);

                writer.Write((uint) _animCommands.Length);
                writer.WriteBlockArray(_animCommands);

                writer.Write((uint) _meshTrees.Length);
                writer.WriteBlockArray(_meshTrees);

                writer.Write((uint) _frames.Length);
                writer.WriteBlockArray(_frames);

                writer.Write((uint) _moveables.Length);
                writer.WriteBlockArray(_moveables);

                writer.Write((uint) _staticMeshes.Length);
                writer.WriteBlockArray(_staticMeshes);

                // SPR block
                _spr = new byte[] {0x53, 0x50, 0x52};
                writer.WriteBlockArray(_spr);

                writer.Write((uint) _spriteTextures.Length);
                writer.WriteBlockArray(_spriteTextures);

                writer.Write((uint) _spriteSequences.Length);
                writer.WriteBlockArray(_spriteSequences);

                // Write camera, flyby and sound sources
                writer.Write((uint) _cameras.Length);
                writer.WriteBlockArray(_cameras);

                writer.Write((uint) _flyByCameras.Length);
                writer.WriteBlockArray(_flyByCameras);

                writer.Write((uint) _soundSources.Length);
                writer.WriteBlockArray(_soundSources);

                // Write pathfinding data
                writer.Write((uint) _boxes.Length);
                writer.WriteBlockArray(_boxes);

                writer.Write((uint) _overlaps.Length);
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
                writer.Write(_numAnimatedTextures);

                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                short numSets = (short) _animatedTextures.Length;
                writer.Write(numSets);

                for (var i = 0; i < _animatedTextures.Length; i++)
                {
                    writer.Write(_animatedTextures[i].NumTextures);

                    foreach (var texture in _animatedTextures[i].Textures)
                    {
                        writer.Write(texture);
                    }
                }

                // Write object textures
                writer.Write(new byte[] { 0x00, 0x54, 0x45, 0x58 });

                _objectTextures = _tempObjectTextures.ToArray();

                writer.Write((uint) _objectTextures.Length);
                writer.WriteBlockArray(_objectTextures);

                // Write items and AI objects
                writer.Write((uint) _items.Length);
                writer.WriteBlockArray(_items);

                writer.Write((uint) _aiItems.Length);
                writer.WriteBlockArray(_aiItems);

                const short numDemo = 0;
                writer.Write(numDemo);

                // Write sound data
                byte[] sampleIndices;
                byte[] soundDetails;
                byte[] soundMap;
                uint numSampleIndices;
                using (var readerSounds = new BinaryReaderEx(new FileStream(
                    _level.Wad.OriginalWad.BasePath + "\\" + _level.Wad.OriginalWad.BaseName + ".sfx", FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    soundMap = readerSounds.ReadBytes(370 * 2);
                    _numSoundDetails = (uint) readerSounds.ReadInt32();
                    soundDetails = readerSounds.ReadBytes((int) _numSoundDetails * 8);
                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    numSampleIndices = (uint) readerSounds.ReadInt32();
                    sampleIndices = readerSounds.ReadBytes((int) numSampleIndices * 4);
                }

                writer.Write(soundMap);
                writer.Write(_numSoundDetails);
                writer.Write(soundDetails);
                writer.Write(numSampleIndices);
                // writer.Write(sampleIndices);
                int filler3 = 0;
                for (int i = 0; i < numSampleIndices; i++) writer.Write(filler3);
                // writer.WriteBlockArray(sfxBuffer);

                writer.Write(numDemo);
                writer.Write(numDemo);
                writer.Write(numDemo);

                writer.Flush();
            }

            using (var writer = new BinaryWriterEx(new FileStream(_dest, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                using (var reader = new BinaryReaderEx(new FileStream("temp.bin", FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    ReportProgress(90, "Writing final level");

                    var version = new byte[] {0x54, 0x52, 0x34, 0x00};
                    writer.WriteBlockArray(version);

                    ReportProgress(95, "Writing textures");

                    writer.Write(_numRoomTextureTiles);
                    writer.Write(_numObjectTextureTiles);
                    writer.Write(NumBumpTextureTiles);

                    writer.Write(_texture32UncompressedSize);
                    writer.Write(_texture32CompressedSize);
                    writer.WriteBlockArray(_texture32);

                    writer.Write(_texture16UncompressedSize);
                    writer.Write(_texture16CompressedSize);
                    writer.WriteBlockArray(_texture16);

                    writer.Write(_miscTextureUncompressedSize);
                    writer.Write(_miscTextureCompressedSize);
                    writer.WriteBlockArray(_miscTexture);

                    ReportProgress(95, "Compressing geometry data");

                    var geometrySize = (int) reader.BaseStream.Length;
                    var levelData = reader.ReadBytes(geometrySize);
                    var buffer = Utils.CompressDataZLIB(levelData);
                    _levelUncompressedSize = (uint) geometrySize;
                    _levelCompressedSize = (uint) buffer.Length;

                    ReportProgress(80, "Writing goemetry data");

                    writer.Write(_levelUncompressedSize);
                    writer.Write(_levelCompressedSize);
                    writer.WriteBlockArray(buffer);

                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    int numSamples = _level.Wad.OriginalWad.Sounds.Count;
                    writer.WriteBlock(numSamples);

                    ReportProgress(80, "Writing WAVE sounds");

                    writer.Write(_bufferSamples);

                    ReportProgress(99, "Done");

                    writer.Flush();
                }
            }
        }

        private bool WriteLevelTr3()
        {
            var wad = _level.Wad.OriginalWad;

            // Now begin to compile the geometry block in a MemoryStream
            using (var writer = new BinaryWriterEx(new FileStream(_dest, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                ReportProgress(85, "Writing geometry data to memory buffer");

                // Write version
                var version = new byte[] {0x38, 0x00, 0x18, 0xFF};
                writer.WriteBlockArray(version);

                using (var readerPalette = new BinaryReader(new FileStream("Editor\\palette.bin", FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    var palette = readerPalette.ReadBytes(1792);
                    // Write palette
                    writer.Write(palette);
                }

                // Write textures
                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                int numTextureTiles = _numRoomTextureTiles + _numObjectTextureTiles + 1;
                writer.Write(numTextureTiles);

                // Fake 8 bit textures
                var fakeTextures = new byte[256 * 256 * numTextureTiles];
                writer.Write(fakeTextures);

                // 16 bit textures
                writer.Write(_textures16);

                using (var readerRaw = new BinaryReader(new FileStream("sprites3.raw", FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    var raw = readerRaw.ReadBytes(131072);
                    writer.Write(raw);
                }

                const int filler = 0;
                writer.Write(filler);

                var numRooms = (ushort) _level.Rooms.Count(r => r != null);
                writer.Write(numRooms);

                long offset;
                long offset2;
                foreach (var room in _level.Rooms.Where(r => r != null))
                {
                    var tempRoom = _tempRooms[room];
                    writer.WriteBlock(tempRoom.Info);

                    offset = writer.BaseStream.Position;

                    const int numdw = 0;
                    writer.Write(numdw);

                    var tmp = (ushort) tempRoom.Vertices.Length;
                    writer.Write(tmp);
                    writer.WriteBlockArray(tempRoom.Vertices);

                    tmp = (ushort) tempRoom.Rectangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (var k = 0; k < tempRoom.Rectangles.Length; k++)
                        {
                            tempRoom.Rectangles[k].Write(writer);
                        }
                    }

                    tmp = (ushort) tempRoom.Triangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (var k = 0; k < tempRoom.Triangles.Length; k++)
                        {
                            tempRoom.Triangles[k].Write(writer);
                        }
                    }

                    // For sprites, not used
                    tmp = 0;
                    writer.Write(tmp);

                    // Now save current offset and calculate the size of the geometry
                    offset2 = writer.BaseStream.Position;
                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    ushort roomGeometrySize = (ushort) ((offset2 - offset - 4) / 2);

                    // Save the size of the geometry
                    writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                    writer.Write(roomGeometrySize);
                    writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                    // Write portals
                    tmp = (ushort) tempRoom.Portals.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0)
                        writer.WriteBlockArray(tempRoom.Portals);

                    // Write sectors
                    writer.Write(tempRoom.NumZSectors);
                    writer.Write(tempRoom.NumXSectors);
                    writer.WriteBlockArray(tempRoom.Sectors);

                    // Write room color
                    writer.Write(tempRoom.AmbientIntensity1);
                    writer.Write(tempRoom.AmbientIntensity2);

                    // Write lights
                    tmp = (ushort) tempRoom.Lights.Length;
                    writer.WriteBlock(tmp);

                    for (var j = 0; j < tmp; j++)
                    {
                        var light = tempRoom.Lights[j];
                        writer.Write(light.X);
                        writer.Write(light.Y);
                        writer.Write(light.Z);

                        int intensity = light.Intensity;
                        // ReSharper disable once SuggestVarOrType_BuiltInTypes
                        int falloff = (int) light.Out;

                        writer.Write(intensity);
                        writer.Write(falloff);
                    }

                    // Write static meshes
                    tmp = (ushort) tempRoom.StaticMeshes.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0)
                        writer.WriteBlockArray(tempRoom.StaticMeshes);

                    // Write final data
                    writer.Write(tempRoom.AlternateRoom);
                    writer.Write(tempRoom.Flags);
                    writer.Write(tempRoom.WaterScheme);
                    writer.Write(tempRoom.ReverbInfo);
                    writer.Write(tempRoom.AlternateGroup);
                }

                // Write floordata
                var numFloorData = (uint) _floorData.Length;
                writer.Write(numFloorData);
                writer.WriteBlockArray(_floorData);

                // Write meshes
                offset = writer.BaseStream.Position;

                const int numMeshData = 0;
                writer.Write(numMeshData);
                var totalMeshSize = 0;

                for (var i = 0; i < _meshes.Length; i++)
                {
                    var meshSize = _meshes[i].WriteTr3(writer);

                    for (var n = 0; n < _meshPointers.Length; n++)
                    {
                        if (wad.HelperPointers[n] == i)
                        {
                            _meshPointers[n] = (uint) totalMeshSize;
                        }
                    }

                    totalMeshSize += (int) meshSize;

                    //if (i < NumMeshes - 1) MeshPointers[i + 1] = MeshPointers[i] + (uint)meshSize;
                }

                offset2 = writer.BaseStream.Position;
                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                uint meshDataSize = (uint) ((offset2 - offset - 4) / 2);

                // Save the size of the meshes
                writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(meshDataSize);
                writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                // Write mesh pointers
                writer.Write((uint) _meshPointers.Length);
                writer.WriteBlockArray(_meshPointers);

                // Write animations' data
                writer.Write((uint) _animations.Length);
                foreach (var anim in _animations)
                {
                    anim.Write(writer);
                }

                writer.Write((uint) _stateChanges.Length);
                writer.WriteBlockArray(_stateChanges);

                writer.Write((uint) _animDispatches.Length);
                writer.WriteBlockArray(_animDispatches);

                writer.Write((uint) _animCommands.Length);
                writer.WriteBlockArray(_animCommands);

                writer.Write((uint) _meshTrees.Length);
                writer.WriteBlockArray(_meshTrees);

                writer.Write((uint) _frames.Length);
                writer.WriteBlockArray(_frames);

                writer.Write((uint) _moveables.Length);
                writer.WriteBlockArray(_moveables);

                writer.Write((uint) _staticMeshes.Length);
                writer.WriteBlockArray(_staticMeshes);

                // SPR block
                using (var readerSprites = new BinaryReader(new FileStream("sprites3.bin", FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    var bufferSprites = readerSprites.ReadBytes((int) readerSprites.BaseStream.Length);
                    writer.Write(bufferSprites);
                }

                /*writer.Write(NumSpriteTextures);
                writer.WriteBlockArray(SpriteTextures);

                writer.Write(NumSpriteSequences);
                writer.WriteBlockArray(SpriteSequences);
                */
                // Write camera, flyby and sound sources
                writer.Write((uint) _cameras.Length);
                writer.WriteBlockArray(_cameras);

                writer.Write((uint) _soundSources.Length);
                writer.WriteBlockArray(_soundSources);

                // Write pathfinding data
                writer.Write((uint) _boxes.Length);
                writer.WriteBlockArray(_boxes);

                writer.Write((uint) _overlaps.Length);
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

                //   writer.WriteBlockArray(Zones);

                // Write animated textures
                writer.Write(_numAnimatedTextures);

                writer.Write((short) _animatedTextures.Length);

                for (var i = 0; i < _animatedTextures.Length; i++)
                {
                    writer.Write(_animatedTextures[i].NumTextures);

                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    foreach (short texture in _animatedTextures[i].Textures)
                    {
                        writer.Write(texture);
                    }
                }

                // Write object textures
                _objectTextures = _tempObjectTextures.ToArray();

                writer.Write((uint) _objectTextures.Length);
                for (var j = 0; j < _objectTextures.Length; j++)
                {
                    writer.Write(_objectTextures[j].Attributes);
                    writer.Write(_objectTextures[j].Tile);
                    writer.WriteBlockArray(_objectTextures[j].Vertices);
                }

                // Write items and AI objects
                writer.Write((uint) _items.Length);
                writer.WriteBlockArray(_items);

                var lightmap = new byte[8192];
                writer.Write(lightmap);

                const short numDemo = 0;
                writer.Write(numDemo);
                writer.Write(numDemo);

                // Write sound data
                byte[] sfxBuffer;
                using (var readerSounds = new BinaryReaderEx(new FileStream(
                        @"Graphics\Wads\" + _level.Wad.OriginalWad.BaseName + ".sfx", FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    sfxBuffer = readerSounds.ReadBytes((int) readerSounds.BaseStream.Length);
                    readerSounds.BaseStream.Seek(0, SeekOrigin.Begin);
                    readerSounds.ReadBytes(370 * 2);
                    _numSoundDetails = (uint) readerSounds.ReadInt16();
                }

                writer.WriteBlockArray(sfxBuffer);

                writer.Flush();
            }

            return true;
        }
    }
}
