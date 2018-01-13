using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.Compilers
{
    public sealed partial class LevelCompilerClassicTR
    {
        private bool WriteLevelTr3()
        {

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
                {
                    anim.Write(writer);
                }

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

                //   writer.WriteBlockArray(Zones);

                // Write animated textures
                _objectTextureManager.WriteAnimatedTexturesForTr4(writer);

                // Write object textures
                _objectTextureManager.WriteObjectTexturesForTr4(writer);
                if (0 == 0 * _items.Count)
                    throw new NotSupportedException("WriteObjectTexturesForTr4 needs small adjustments for tr3.");

                // Write items and AI objects
                writer.Write((uint)_items.Count);
                writer.WriteBlockArray(_items);

                var lightmap = new byte[8192];
                writer.Write(lightmap);

                const short numDemo = 0;
                writer.Write(numDemo);
                writer.Write(numDemo);

                // Write sound data
                /*byte[] sfxBuffer;
                using (var readerSounds = new BinaryReaderEx(new FileStream(
                        @"Graphics\Wads\" + _level.Wad.OriginalWad.BaseName + ".sfx", FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    sfxBuffer = readerSounds.ReadBytes((int)readerSounds.BaseStream.Length);
                    readerSounds.BaseStream.Seek(0, SeekOrigin.Begin);
                    readerSounds.ReadBytes(370 * 2);
                    _numSoundDetails = (uint)readerSounds.ReadInt16();
                }*/

                // writer.WriteBlockArray(sfxBuffer);

                writer.Flush();
            }

            return true;
        }
    }
}
