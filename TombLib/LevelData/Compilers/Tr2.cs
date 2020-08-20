using System.Collections.Generic;
using System.Drawing;
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
                ReportProgress(80, "Writing texture data");

                // Write version
                writer.WriteBlockArray(new byte[] { 0x2D, 0x00, 0x00, 0x00 });

                // Predefine hardcoded palette colours
                var predefinedPaletteColors = new List<Color>();

                // Following palette colors have hardcoded meaning in TR2
                // https://github.com/Arsunt/TR2Main/blob/0586ba8965fc3c260080d9e6ea05f3e17033ba4b/global/types.h#L931
                predefinedPaletteColors.Add(Color.FromArgb(0,   0,   0));
                predefinedPaletteColors.Add(Color.FromArgb(128, 128, 128));
                predefinedPaletteColors.Add(Color.FromArgb(255, 255, 255));
                predefinedPaletteColors.Add(Color.FromArgb(255, 0,   0));
                predefinedPaletteColors.Add(Color.FromArgb(255, 165, 0));
                predefinedPaletteColors.Add(Color.FromArgb(255, 255, 0));
                predefinedPaletteColors.Add(Color.FromArgb(0,   0,   0));  // Unused
                predefinedPaletteColors.Add(Color.FromArgb(0,   0,   0));  // Unused
                predefinedPaletteColors.Add(Color.FromArgb(0,   0,   0));  // Unused
                predefinedPaletteColors.Add(Color.FromArgb(0,   0,   0));  // Unused
                predefinedPaletteColors.Add(Color.FromArgb(0,   0,   0));  // Unused
                predefinedPaletteColors.Add(Color.FromArgb(0,   0,   0));  // Unused
                predefinedPaletteColors.Add(Color.FromArgb(0,   128, 0));
                predefinedPaletteColors.Add(Color.FromArgb(0,   255, 0));
                predefinedPaletteColors.Add(Color.FromArgb(0,   255, 255));
                predefinedPaletteColors.Add(Color.FromArgb(0,   0,   255));
                predefinedPaletteColors.Add(Color.FromArgb(255, 0,   255));

                // Create palette and 8-bit indexed textures
                tr_color[] palette;
                var textureData = PackTextureMap32To8Bit(_texture32Data, predefinedPaletteColors, out palette);

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

                // Write items
                writer.Write((uint)_items.Count);
                writer.WriteBlockArray(_items);

                // TODO Figure out light map
                var lightmap = new byte[8192];
                writer.Write(lightmap);

                const ushort numDemo = 0;
                const ushort numCinematicFrames = 0;
                writer.Write(numDemo);
                writer.Write(numCinematicFrames);

                // Write sound meta data
                PrepareSoundsData();
                WriteSoundMetadata(writer);
            }
        }
    }
}
