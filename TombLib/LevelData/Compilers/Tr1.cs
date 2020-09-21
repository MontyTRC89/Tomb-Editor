using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TombLib.IO;

namespace TombLib.LevelData.Compilers
{
    public partial class LevelCompilerClassicTR
    {
        private void WriteLevelTr1()
        {
            // Now begin to compile the geometry block in a MemoryStream
            using (var writer = new BinaryWriterEx(new FileStream(_dest, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                ReportProgress(80, "Writing texture data");

                // Write version
                writer.WriteBlockArray(new byte[] { 0x20, 0x00, 0x00, 0x00 });

                // Write textures
                int numTextureTiles = _texture32Data.GetLength(0) / (256 * 256 * 4);
                writer.Write(numTextureTiles);

                // Predefine hardcoded palette colours
                var predefinedPaletteColors = new List<Color>();

                // Colors 0-23 reserved for HP bar
                predefinedPaletteColors.Add(Color.FromArgb(2,  0,  0  ));
                predefinedPaletteColors.Add(Color.FromArgb(0,  0,  0  ));
                predefinedPaletteColors.Add(Color.FromArgb(6,  1,  0  ));
                predefinedPaletteColors.Add(Color.FromArgb(12, 3,  1  ));
                predefinedPaletteColors.Add(Color.FromArgb(18, 7,  1  ));
                predefinedPaletteColors.Add(Color.FromArgb(24, 10, 1  ));
                predefinedPaletteColors.Add(Color.FromArgb(22, 17, 0  ));
                predefinedPaletteColors.Add(Color.FromArgb(26, 20, 3  ));
                predefinedPaletteColors.Add(Color.FromArgb(28, 23, 11 ));
                predefinedPaletteColors.Add(Color.FromArgb(34, 22, 11 ));
                predefinedPaletteColors.Add(Color.FromArgb(40, 26, 14 ));
                predefinedPaletteColors.Add(Color.FromArgb(41, 30, 18 ));
                predefinedPaletteColors.Add(Color.FromArgb(44, 32, 20 ));
                predefinedPaletteColors.Add(Color.FromArgb(50, 36, 22 ));
                predefinedPaletteColors.Add(Color.FromArgb(55, 40, 25 ));
                predefinedPaletteColors.Add(Color.FromArgb(58, 48, 28 ));
                predefinedPaletteColors.Add(Color.FromArgb(62, 52, 30 ));
                predefinedPaletteColors.Add(Color.FromArgb(29, 33, 29 ));
                predefinedPaletteColors.Add(Color.FromArgb(22, 25, 22 ));
                predefinedPaletteColors.Add(Color.FromArgb(19, 20, 19 ));
                predefinedPaletteColors.Add(Color.FromArgb(16, 16, 16 ));
                predefinedPaletteColors.Add(Color.FromArgb(12, 12, 12 ));
                predefinedPaletteColors.Add(Color.FromArgb(9,  9,  8  ));
                predefinedPaletteColors.Add(Color.FromArgb(14, 11, 5  ));

                // Colors 24-47 reserved for AP bar
                predefinedPaletteColors.Add(Color.FromArgb(20, 12, 5  ));
                predefinedPaletteColors.Add(Color.FromArgb(22, 16, 9  ));
                predefinedPaletteColors.Add(Color.FromArgb(21, 5,  8  ));
                predefinedPaletteColors.Add(Color.FromArgb(15, 3,  6  ));
                predefinedPaletteColors.Add(Color.FromArgb(31, 8,  8  ));
                predefinedPaletteColors.Add(Color.FromArgb(40, 10, 7  ));
                predefinedPaletteColors.Add(Color.FromArgb(46, 11, 8  ));
                predefinedPaletteColors.Add(Color.FromArgb(35, 28, 14 ));
                predefinedPaletteColors.Add(Color.FromArgb(25, 29, 25 ));
                predefinedPaletteColors.Add(Color.FromArgb(32, 36, 33 ));
                predefinedPaletteColors.Add(Color.FromArgb(36, 40, 36 ));
                predefinedPaletteColors.Add(Color.FromArgb(39, 44, 40 ));
                predefinedPaletteColors.Add(Color.FromArgb(44, 49, 44 ));
                predefinedPaletteColors.Add(Color.FromArgb(51, 51, 51 ));
                predefinedPaletteColors.Add(Color.FromArgb(57, 57, 57 ));
                predefinedPaletteColors.Add(Color.FromArgb(63, 63, 38 ));
                predefinedPaletteColors.Add(Color.FromArgb(63, 59, 34 ));
                predefinedPaletteColors.Add(Color.FromArgb(23, 40, 39 ));
                predefinedPaletteColors.Add(Color.FromArgb(22, 47, 49 ));
                predefinedPaletteColors.Add(Color.FromArgb(33, 51, 51 ));
                predefinedPaletteColors.Add(Color.FromArgb(47, 55, 55 ));
                predefinedPaletteColors.Add(Color.FromArgb(47, 47, 35 ));
                predefinedPaletteColors.Add(Color.FromArgb(43, 23, 34 ));
                predefinedPaletteColors.Add(Color.FromArgb(55, 61, 51 ));

                // Create palette and 8-bit indexed textures
                tr_color[] palette;
                var textureData = PackTextureMap32To8Bit(_texture32Data, predefinedPaletteColors, out palette);

                // Write 8-bit textures
                writer.Write(textureData);

                const int filler = 0;
                writer.Write(filler);

                ReportProgress(85, "Writing geometry data");

                // Write rooms
                var numRooms = (ushort)_sortedRooms.Count(r => r != null);
                writer.Write(numRooms);

                foreach (var r in _sortedRooms.Where(r => r != null))
                    _tempRooms[r].WriteTr1(writer);

                // Write floordata
                var numFloorData = (uint)_floorData.Count;
                writer.Write(numFloorData);
                writer.WriteBlockArray(_floorData);

                // Write meshes
                long offset = writer.BaseStream.Position;

                const int numMeshData = 0;
                writer.Write(numMeshData);
                var totalMeshSize = 0;

                for (var i = 0; i < _meshes.Count; i++)
                {
                    var meshSize = _meshes[i].WriteTr3(writer);
                    totalMeshSize += (int)meshSize;
                }

                long offset2 = writer.BaseStream.Position;
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

                // Write cameras, sound sources
                writer.Write((uint)_cameras.Count);
                writer.WriteBlockArray(_cameras);

                writer.Write((uint)_soundSources.Count);
                writer.WriteBlockArray(_soundSources);

                // Write pathfinding data
                writer.Write((uint)_boxes.Length);
                for (var i = 0; i < _boxes.Length; i++)
                {
                    writer.Write(_boxes[i].Zmin * 1024);
                    writer.Write(_boxes[i].Zmax * 1024);
                    writer.Write(_boxes[i].Xmin * 1024);
                    writer.Write(_boxes[i].Xmax * 1024);
                    writer.Write(_boxes[i].TrueFloor);
                    writer.Write(_boxes[i].OverlapIndex);
                }

                writer.Write((uint)_overlaps.Length);
                writer.WriteBlockArray(_overlaps);

                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].GroundZone1_Normal);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].GroundZone2_Normal);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].FlyZone_Normal);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].GroundZone1_Alternate);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].GroundZone2_Alternate);
                for (var i = 0; i < _boxes.Length; i++)
                    writer.Write(_zones[i].FlyZone_Alternate);

                // Write animated textures
                _textureInfoManager.WriteAnimatedTextures(writer);

                // Write items
                writer.Write((uint)_items.Count);
                foreach (var item in _items)
                {
                    writer.Write(item.ObjectID);
                    writer.Write(item.Room);
                    writer.Write(item.X);
                    writer.Write(item.Y);
                    writer.Write(item.Z);
                    writer.Write(item.Angle);
                    writer.Write(item.Intensity1);
                    writer.Write(item.Flags);
                }

                // Calculate and write lightmap
                ReportProgress(97, "Calculating lightmap");
                writer.Write(CalculateLightmap(palette));

                // Write palette
                foreach (var c in palette)
                    c.write(writer);

                const ushort numCinematicFrames = 0;
                const ushort numDemo = 0;
                writer.Write(numCinematicFrames);
                writer.Write(numDemo);

                // Write sound meta data
                PrepareSoundsData();
                WriteSoundMetadata(writer);

                ReportProgress(97, "Writing WAVE sounds");
                WriteSoundData(writer);
            }
        }
    }
}
