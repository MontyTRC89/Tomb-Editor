using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;

namespace TombEditor.Compilers
{
    public sealed partial class LevelCompilerTr4
    {
        private void WriteLevelTr4()
        {
            var wad = _level.Wad.OriginalWad;

            // Now begin to compile the geometry block in a MemoryStream
            using (var geometryDataBuffer = new MemoryStream())
            {
                {
                    var writer = new BinaryWriterEx(geometryDataBuffer); // Don't dispose
                    ReportProgress(85, "Writing geometry data to memory buffer");

                    const int filler = 0;
                    writer.Write(filler);

                    var numRooms = (ushort)_level.Rooms.Count(r => r != null);
                    writer.Write(numRooms);

                    foreach (var r in _level.Rooms.Where(r => r != null))
                    {
                        _tempRooms[r].Write(writer);
                    }

                    // Write floordata
                    var numFloorData = (uint)_floorData.Length;
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
                    writer.Write((uint)_meshPointers.Length);
                    writer.WriteBlockArray(_meshPointers);

                    // Write animations' data
                    writer.Write((uint)_animations.Length);
                    writer.WriteBlockArray(_animations);

                    writer.Write((uint)_stateChanges.Length);
                    writer.WriteBlockArray(_stateChanges);

                    writer.Write((uint)_animDispatches.Length);
                    writer.WriteBlockArray(_animDispatches);

                    writer.Write((uint)_animCommands.Length);
                    writer.WriteBlockArray(_animCommands);

                    writer.Write((uint)_meshTrees.Length);
                    writer.WriteBlockArray(_meshTrees);

                    writer.Write((uint)_frames.Length);
                    writer.WriteBlockArray(_frames);

                    writer.Write((uint)_moveables.Length);
                    writer.WriteBlockArray(_moveables);

                    writer.Write((uint)_staticMeshes.Length);
                    writer.WriteBlockArray(_staticMeshes);

                    // SPR block
                    writer.WriteBlockArray(new byte[] { 0x53, 0x50, 0x52 });

                    writer.Write((uint)_spriteTextures.Length);
                    writer.WriteBlockArray(_spriteTextures);

                    writer.Write((uint)_spriteSequences.Length);
                    writer.WriteBlockArray(_spriteSequences);

                    // Write camera, flyby and sound sources
                    writer.Write((uint)_cameras.Length);
                    writer.WriteBlockArray(_cameras);

                    writer.Write((uint)_flyByCameras.Length);
                    writer.WriteBlockArray(_flyByCameras);

                    writer.Write((uint)_soundSources.Length);
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
                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    int numAnimatedTexture = 1;
                    for (var i = 0; i < _animatedTextures.Length; i++)
                        numAnimatedTexture += _animatedTextures.Length + 1;

                    writer.Write((uint)numAnimatedTexture);
                    writer.Write((ushort)_animatedTextures.Length);
                    for (var i = 0; i < _animatedTextures.Length; i++)
                    {
                        writer.Write((ushort)(_animatedTextures[i].Textures.GetLength(0)));

                        foreach (var texture in _animatedTextures[i].Textures)
                        {
                            writer.Write(texture);
                        }
                    }

                    // Write object textures
                    writer.Write(new byte[] { 0x00, 0x54, 0x45, 0x58 });

                    _objectTextureManager.WriteObjectTexturesForTr4(writer);

                    // Write items and AI objects
                    writer.Write((uint)_items.Length);
                    writer.WriteBlockArray(_items);

                    writer.Write((uint)_aiItems.Length);
                    writer.WriteBlockArray(_aiItems);

                    const short numDemo = 0;
                    writer.Write(numDemo);

                    // Write sound data

                    // Write sound map
                    int lastSound = 0;
                    for (int i = 0; i < 370; i++)
                    {
                        short soundMapValue = -1;
                        if (_level.Wad.SoundInfo.ContainsKey((ushort)i))
                        {
                            soundMapValue = (short)lastSound;
                            lastSound++;
                        }

                        writer.Write(soundMapValue);
                    }

                    // Write sound details
                    _numSoundDetails = (uint)_level.Wad.SoundInfo.Count;
                    writer.Write(_numSoundDetails);

                    short lastSample = 0;

                    for (int i = 0; i < _level.Wad.SoundInfo.Count; i++)
                    {
                        var wadInfo = _level.Wad.SoundInfo.ElementAt(i).Value;
                        var soundInfo = new tr_sound_details();

                        soundInfo.Sample = lastSample;
                        soundInfo.Volume = wadInfo.Volume;
                        soundInfo.Range = wadInfo.Range;
                        soundInfo.Pitch = wadInfo.Pitch;
                        soundInfo.Chance = wadInfo.Chance;

                        ushort characteristics = (ushort)(wadInfo.WaveSounds.Count << 2);
                        if (wadInfo.FlagN) characteristics |= 0x1000;
                        if (wadInfo.RandomizePitch) characteristics |= 0x2000;
                        if (wadInfo.RandomizeGain) characteristics |= 0x4000;
                        characteristics |= wadInfo.Loop;

                        soundInfo.Characteristics = characteristics;

                        writer.WriteBlock<tr_sound_details>(soundInfo);

                        lastSample += (short)wadInfo.WaveSounds.Count;
                    }

                    int numSampleIndices = lastSample;
                    writer.Write(numSampleIndices);
                    int filler3 = 0;
                    for (int i = 0; i < numSampleIndices; i++)
                        writer.Write(filler3);

                    writer.Write((short)0);
                    writer.Write((short)0);
                    writer.Write((short)0);

                    writer.Flush();
                }
                geometryDataBuffer.Seek(0, SeekOrigin.Begin);

                using (var writer = new BinaryWriterEx(new FileStream(_dest, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    ReportProgress(90, "Writing final level");
                    writer.WriteBlockArray(new byte[] { 0x54, 0x52, 0x34, 0x00 });

                    ReportProgress(95, "Writing textures");

                    // The room texture tile count currently also currently contains the wad textures
                    // But lets not bother with those fielsd too much since they only matter when bump maps are used and we don't use them.
                    writer.Write((ushort)(_texture32Data.GetLength(0) / (256 * 256 * 4)));
                    writer.Write((ushort)0);
                    writer.Write((ushort)0);

                    // Compress data
                    ReportProgress(96, "Compressing data");

                    byte[] texture32 = null;
                    int texture32UncompressedSize = -1;
                    byte[] texture16 = null;
                    int texture16UncompressedSize = -1;
                    byte[] textureMisc = null;
                    int textureMiscUncompressedSize = -1;
                    byte[] geometryData = null;
                    int geometryDataUncompressedSize = -1;
                        
                    using (Task Texture32task = Task.Factory.StartNew(() =>
                    {
                        texture32 = ZLib.CompressData(_texture32Data);
                        texture32UncompressedSize = _texture32Data.Length;
                    }))
                    using (Task Texture16task = Task.Factory.StartNew(() =>
                    {
                        byte[] texture16Data = PackTextureMap32To16Bit(_texture32Data, 256, _texture32Data.GetLength(0) / (256 * 4));
                        texture16 = ZLib.CompressData(texture16Data);
                        texture16UncompressedSize = texture16Data.Length;
                    }))
                    using (Task textureMiscTask = Task.Factory.StartNew(() =>
                    {
                        Stream textureMiscData = PrepareFontAndSkyTexture();
                        textureMisc = ZLib.CompressData(textureMiscData);
                        textureMiscUncompressedSize = (int)(textureMiscData.Length);
                    }))
                    using (Task GeometryDataTask = Task.Factory.StartNew(() =>
                    {
                        geometryData = ZLib.CompressData(geometryDataBuffer);
                        geometryDataUncompressedSize = (int)(geometryDataBuffer.Length);
                    }))
                        Task.WaitAll(Texture32task, Texture16task, textureMiscTask, GeometryDataTask);
                    
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

                    writer.Write(geometryDataUncompressedSize);
                    writer.Write(geometryData.Length);
                    writer.Write(geometryData);

                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    //writer.Write(_level.Wad.Sounds.Count);

                    ReportProgress(98, "Writing WAVE sounds");

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
                writer.WriteBlockArray(new byte[] { 0x38, 0x00, 0x18, 0xFF });

                using (var readerPalette = new BinaryReader(new FileStream("Editor\\palette.bin", FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    var palette = readerPalette.ReadBytes(1792);
                    // Write palette
                    writer.Write(palette);
                }

                // Write textures
                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                int numTextureTiles = _texture32Data.GetLength(0) / (256 * 256 * 4) + 1;
                writer.Write(numTextureTiles);

                // Fake 8 bit textures
                var fakeTextures = new byte[256 * 256 * numTextureTiles];
                writer.Write(fakeTextures);

                // 16 bit textures
                byte[] texture16Data = PackTextureMap32To16Bit(_texture32Data, 256, _texture32Data.GetLength(0) / (256 * 4));
                writer.Write(texture16Data);

                using (var readerRaw = new BinaryReader(new FileStream("sprites3.raw", FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    var raw = readerRaw.ReadBytes(131072);
                    writer.Write(raw);
                }

                const int filler = 0;
                writer.Write(filler);

                var numRooms = (ushort)_level.Rooms.Count(r => r != null);
                writer.Write(numRooms);

                long offset;
                long offset2;
                foreach (var room in _level.Rooms.Where(r => r != null))
                {
                    var tempRoom = _tempRooms[room];
                    writer.WriteBlock(tempRoom.Info);

                    offset = writer.BaseStream.Position;

                    writer.Write((int)0);

                    writer.Write((ushort)tempRoom.Vertices.Count);
                    writer.WriteBlockArray(tempRoom.Vertices);

                    writer.Write((ushort)tempRoom.Quads.Count);
                    for (var k = 0; k < tempRoom.Quads.Count; k++)
                        tempRoom.Quads[k].Write(writer);

                    writer.Write((ushort)tempRoom.Triangles.Count);
                    for (var k = 0; k < tempRoom.Triangles.Count; k++)
                        tempRoom.Triangles[k].Write(writer);

                    // For sprites, not used
                    writer.Write((ushort)0);

                    // Now save current offset and calculate the size of the geometry
                    offset2 = writer.BaseStream.Position;
                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    ushort roomGeometrySize = (ushort)((offset2 - offset - 4) / 2);

                    // Save the size of the geometry
                    writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                    writer.Write(roomGeometrySize);
                    writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                    // Write portals
                    writer.WriteBlock((ushort)tempRoom.Portals.Count);
                    if (tempRoom.Portals.Count != 0)
                        writer.WriteBlockArray(tempRoom.Portals);

                    // Write sectors
                    writer.Write(tempRoom.NumZSectors);
                    writer.Write(tempRoom.NumXSectors);
                    writer.WriteBlockArray(tempRoom.Sectors);

                    // Write room color
                    writer.Write(tempRoom.AmbientIntensity);

                    // Write lights
                    writer.WriteBlock((ushort)tempRoom.Lights.Count);

                    for (var j = 0; j < tempRoom.Lights.Count; j++)
                    {
                        var light = tempRoom.Lights[j];
                        writer.Write(light.X);
                        writer.Write(light.Y);
                        writer.Write(light.Z);

                        int intensity = light.Intensity;
                        // ReSharper disable once SuggestVarOrType_BuiltInTypes
                        int falloff = (int)light.Out;

                        writer.Write(intensity);
                        writer.Write(falloff);
                    }

                    // Write static meshes
                    writer.WriteBlock((ushort)tempRoom.StaticMeshes.Count);
                    if (tempRoom.StaticMeshes.Count != 0)
                        writer.WriteBlockArray(tempRoom.StaticMeshes);

                    // Write final data
                    writer.Write(tempRoom.AlternateRoom);
                    writer.Write(tempRoom.Flags);
                    writer.Write(tempRoom.WaterScheme);
                    writer.Write(tempRoom.ReverbInfo);
                    writer.Write(tempRoom.AlternateGroup);
                }

                // Write floordata
                var numFloorData = (uint)_floorData.Length;
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
                    totalMeshSize += (int)meshSize;
                }

                offset2 = writer.BaseStream.Position;
                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                uint meshDataSize = (uint)((offset2 - offset - 4) / 2);

                // Save the size of the meshes
                writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(meshDataSize);
                writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                // Write mesh pointers
                writer.Write((uint)_meshPointers.Length);
                writer.WriteBlockArray(_meshPointers);

                // Write animations' data
                writer.Write((uint)_animations.Length);
                foreach (var anim in _animations)
                {
                    anim.Write(writer);
                }

                writer.Write((uint)_stateChanges.Length);
                writer.WriteBlockArray(_stateChanges);

                writer.Write((uint)_animDispatches.Length);
                writer.WriteBlockArray(_animDispatches);

                writer.Write((uint)_animCommands.Length);
                writer.WriteBlockArray(_animCommands);

                writer.Write((uint)_meshTrees.Length);
                writer.WriteBlockArray(_meshTrees);

                writer.Write((uint)_frames.Length);
                writer.WriteBlockArray(_frames);

                writer.Write((uint)_moveables.Length);
                writer.WriteBlockArray(_moveables);

                writer.Write((uint)_staticMeshes.Length);
                writer.WriteBlockArray(_staticMeshes);

                // SPR block
                using (var readerSprites = new BinaryReader(new FileStream("sprites3.bin", FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    var bufferSprites = readerSprites.ReadBytes((int)readerSprites.BaseStream.Length);
                    writer.Write(bufferSprites);
                }

                /*writer.Write(NumSpriteTextures);
                writer.WriteBlockArray(SpriteTextures);

                writer.Write(NumSpriteSequences);
                writer.WriteBlockArray(SpriteSequences);
                */
                // Write camera, flyby and sound sources
                writer.Write((uint)_cameras.Length);
                writer.WriteBlockArray(_cameras);

                writer.Write((uint)_soundSources.Length);
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

                //   writer.WriteBlockArray(Zones);

                // Write animated textures
                int numAnimatedTextures = 1; // Offset by 1
                for (var i = 0; i < _animatedTextures.Length; i++)
                    numAnimatedTextures += _animatedTextures[i].Textures.Length;
                writer.Write((uint)numAnimatedTextures);

                writer.Write((short)_animatedTextures.Length);

                for (var i = 0; i < _animatedTextures.Length; i++)
                {
                    writer.Write((short)(_animatedTextures[i].Textures.Length));

                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    foreach (short texture in _animatedTextures[i].Textures)
                    {
                        writer.Write(texture);
                    }
                }

                // Write object textures
                _objectTextureManager.WriteObjectTexturesForTr4(writer);
                if (0 == 0 * _items.Length)
                    throw new NotSupportedException("WriteObjectTexturesForTr4 needs small adjustments for tr3.");

                // Write items and AI objects
                writer.Write((uint)_items.Length);
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
                    sfxBuffer = readerSounds.ReadBytes((int)readerSounds.BaseStream.Length);
                    readerSounds.BaseStream.Seek(0, SeekOrigin.Begin);
                    readerSounds.ReadBytes(370 * 2);
                    _numSoundDetails = (uint)readerSounds.ReadInt16();
                }

                writer.WriteBlockArray(sfxBuffer);

                writer.Flush();
            }

            return true;
        }
    }
}
