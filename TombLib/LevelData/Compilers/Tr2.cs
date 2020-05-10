using System.IO;
using System.Linq;
using TombLib.IO;

namespace TombLib.LevelData.Compilers
{
    public partial class LevelCompilerClassicTR
    {
        private void WriteLevelTr2()
        {
            // Now begin to compile the geometry block in a MemoryStream
            using (var writer = new BinaryWriterEx(new FileStream(_dest, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                ReportProgress(80, "Writing geometry data to memory buffer");

                // Write version
                writer.WriteBlockArray(new byte[] { 0x2D, 0x00, 0x00, 0x00 });

                // TODO: for now I write fake palette, they should be needed only for 8 bit textures
                for (var i = 0; i < 768; i++) writer.Write((byte)0x00);
                for (var i = 0; i < 1024; i++) writer.Write((byte)0x00);

                // Write textures
                int numTextureTiles = _texture32Data.GetLength(0) / (256 * 256 * 4);
                writer.Write(numTextureTiles);

                // TODO 8 bit textures (altough who uses 8 bit textures in 2018?)
                var fakeTextures = new byte[256 * 256 * numTextureTiles];
                writer.Write(fakeTextures);

                // 16 bit textures
                byte[] texture16Data = PackTextureMap32To16Bit(_texture32Data);
                writer.Write(texture16Data);

                const int filler = 0;
                writer.Write(filler);

                var numRooms = (ushort)_level.Rooms.Count(r => r != null);
                writer.Write(numRooms);

                long offset;
                long offset2;
                foreach (var r in _level.Rooms.Where(r => r != null))
                {
                    _tempRooms[r].WriteTr2(writer);
                }

                // Write floordata
                var numFloorData = (uint)_floorData.Count;
                writer.Write(numFloorData);
                writer.WriteBlockArray(_floorData);

                // Write meshes
                offset = writer.BaseStream.Position;

                const int numMeshData = 0;
                writer.Write(numMeshData);
                var totalMeshSize = 0;

                for (var i = 0; i < _meshes.Count; i++)
                {
                    var meshSize = _meshes[i].WriteTr3(writer);
                    totalMeshSize += (int)meshSize;
                }

                offset2 = writer.BaseStream.Position;
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
                writer.WriteBlockArray(_moveables);

                writer.Write((uint)_staticMeshes.Count);
                writer.WriteBlockArray(_staticMeshes);

                // Write object textures
                _textureInfoManager.WriteTextureInfos(writer, _level);

                // Sprites
                writer.Write((uint)_spriteTextures.Count);
                writer.WriteBlockArray(_spriteTextures);

                writer.Write((uint)_spriteSequences.Count);
                writer.WriteBlockArray(_spriteSequences);

                // Write camera, sound sources
                writer.Write((uint)_cameras.Count);
                writer.WriteBlockArray(_cameras);

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

                // Write items and AI objects
                writer.Write((uint)_items.Count);
                writer.WriteBlockArray(_items);

                // TODO Figure out light map
                var lightmap = new byte[8192];
                writer.Write(lightmap);

                const short numDemo = 0;
                writer.Write(numDemo);
                writer.Write(numDemo);

                // Write sounds

                // Write sound map
                var soundMapSize = 370;
                uint numBytesWritten = 0;
                var lastSound = 0;
                for (int i = 0; i < soundMapSize; i++) {
                    short soundMapValue = -1;
                    if (_level.Settings.WadTryGetSoundInfo(i) != null) {
                        soundMapValue = (short)lastSound;
                        lastSound++;
                    }

                    writer.Write(soundMapValue);
                    numBytesWritten += sizeof(short);
                }
                _progressReporter.ReportInfo("Num SoundMap Bytes written : " + numBytesWritten);

                // Write sound details
                uint numSoundInfos = 0;
                foreach(var wad in _level.Settings.Wads) {
                    numSoundInfos += (uint)(wad.Wad.Sounds.SoundInfos.Count);
                }
                writer.Write((uint)numSoundInfos);

                ushort lastSample = 0;

                foreach (var wad in _level.Settings.Wads) {
                    for (int i = 0; i < wad.Wad.Sounds.SoundInfos.Count; i++) {
                        var wadInfo = wad.Wad.Sounds.SoundInfos[i];
                        var soundInfo = new tr3_sound_details();

                        soundInfo.Sample = lastSample;
                        soundInfo.Volume = (byte)wadInfo.Volume;
                        soundInfo.Chance = (byte)wadInfo.Chance;

                        ushort characteristics = (/*wadInfo.Samples.Count */ 1 << 2);
                        if (wadInfo.DisablePanning)
                            characteristics |= 0x1000;
                        if (wadInfo.RandomizePitch)
                            characteristics |= 0x2000;
                        if (wadInfo.RandomizeVolume)
                            characteristics |= 0x4000;
                        characteristics |= (byte)wadInfo.LoopBehaviour;

                        soundInfo.Characteristics = characteristics;

                        writer.Write(soundInfo.Sample);
                        writer.Write((short)soundInfo.Volume);
                        writer.Write((short)soundInfo.Chance);
                        writer.Write((short)soundInfo.Pitch);
                        writer.Write(soundInfo.Characteristics);

                        lastSample += (ushort)wadInfo.Samples.Count;
                    }
                }


                /*for (int i = 0; i < _level.Wad.SoundInfo.Count; i++) {
                    var wadInfo = _level.Wad.SoundInfo.ElementAt(i).Value;
                    var soundInfo = new tr_sound_details();

                    soundInfo.Sample = lastSample;
                    soundInfo.Volume = wadInfo.Volume;
                    soundInfo.Chance = wadInfo.Chance;

                    ushort characteristics = (ushort)(/ *wadInfo.Samples.Count * / 1 << 2);
                    if (wadInfo.FlagN)
                        characteristics |= 0x1000;
                    if (wadInfo.RandomizePitch)
                        characteristics |= 0x2000;
                    if (wadInfo.RandomizeGain)
                        characteristics |= 0x4000;
                    characteristics |= (byte)wadInfo.Loop;

                    soundInfo.Characteristics = characteristics;

                    writer.Write(soundInfo.Sample);
                    writer.Write((short)soundInfo.Volume);
                    writer.Write((short)soundInfo.Chance);
                    writer.Write(soundInfo.Characteristics);

                    lastSample += (short)wadInfo.Samples.Count;
                }*/

                // TODO: samples are in MAIN.SFX so I have to found a way to write samples indices here
                int numSampleIndices = lastSample;
                writer.Write(numSampleIndices);
                int filler3 = 0;
                for (int i = 0; i < numSampleIndices; i++)
                    writer.Write(/*filler3*/ i);

                writer.Flush();

                //_soundManager.UpdateMainSfx();
            }
        }
    }
}
