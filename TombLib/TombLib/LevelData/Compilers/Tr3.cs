using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.IO;

namespace TombLib.LevelData.Compilers
{
    public partial class LevelCompilerClassicTR
    {
        private void WriteLevelTr3()
        {
            // Now begin to compile the geometry block in a MemoryStream
            using (var writer = new BinaryWriterEx(new FileStream(_dest, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                ReportProgress(80, "Writing texture data");

                // Write version
                writer.WriteBlockArray(new byte[] { 0x38, 0x00, 0x18, 0xFF });

                // Create palette and 8-bit indexed textures
                tr_color[] palette;
                var textureData = PackTextureMap32To8Bit(_texture32Data, out palette);

                // Write 8-bit palette
                foreach (tr_color c in palette)
                    c.write(writer);

                // Write fake 16-bit palette
                for (var i = 0; i < 1024; i++) writer.Write((byte)0x00);

                // Write textures
                int numTextureTiles = _texture32Data.GetLength(0) / (256 * 256 * 4);
                writer.Write(numTextureTiles);

                // Write 8-bit textures
                writer.Write(textureData);

                // Write 16-bit textures
                byte[] texture16Data = PackTextureMap32To16Bit(_texture32Data, _level.Settings);
                writer.Write(texture16Data);

                const int filler = 0;
                writer.Write(filler);

                ReportProgress(85, "Writing geometry data");

                // Write rooms
                var numRooms = (ushort)_sortedRooms.Count(r => r != null);
                writer.Write(numRooms);

                long offset;
                long offset2;
                foreach (var r in _sortedRooms.Where(r => r != null))
                    _tempRooms[r].WriteTr3(writer);

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

                // Write object textures
                _textureInfoManager.WriteTextureInfos(writer,_level);

                // Write items and AI objects
                writer.Write((uint)_items.Count);
                writer.WriteBlockArray(_items);

                // Calculate and write lightmap
                ReportProgress(97, "Calculating lightmap");
                writer.Write(CalculateLightmap(palette));

                // Dummy cinematic frames
                if (_level.Settings.WriteDummyCinematicFrames)
                {
                    ushort numCine = 2;
                    writer.Write(numCine);
                    for (var i = 0; i < numCine; i++)
                    {
                        var lara = _items.First(it => it.ObjectID == 0);
                        var roomPos = _sortedRooms[lara.Room].Position;
                        writer.Write((short)0);
                        writer.Write((short)0);
                        writer.Write((short)0);
                        writer.Write((ushort)(roomPos.X - Level.WorldUnit * 1.5f));
                        writer.Write((ushort)(roomPos.Y - Level.WorldUnit * 1.5f));
                        writer.Write((ushort)(roomPos.Z - Level.WorldUnit * 1.5f));
                        writer.Write((ushort)8736);
                        writer.Write((ushort)0);
                    }
                }
                else
                    writer.Write((ushort)0);

                // Dummy demodata
                const ushort numDemo = 0;
                writer.Write(numDemo);

                // Write sound meta data
                PrepareSoundsData();
                WriteSoundMetadata(writer);

                writer.Flush();
            }
        }
    }
}
