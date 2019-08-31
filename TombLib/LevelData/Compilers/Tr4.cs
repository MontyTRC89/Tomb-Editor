using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers
{
    public partial class LevelCompilerClassicTR
    {
        private void WriteLevelTr4(string ngVersion = null)
        {
            // Now begin to compile the geometry block in a MemoryStream
            byte[] geometryDataBuffer;
            using (var geometryDataStream = new MemoryStream())
            {
                var writer = new BinaryWriterEx(geometryDataStream); // Don't dispose
                ReportProgress(80, "Writing geometry data to memory buffer");

                const int filler = 0;
                writer.Write(filler);

                var numRooms = (ushort)_level.Rooms.Count(r => r != null);
                writer.Write(numRooms);

                foreach (var r in _level.Rooms.Where(r => r != null))
                    _tempRooms[r].WriteTr4(writer);

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
                writer.WriteBlockArray(_moveables);

                writer.Write((uint)_staticMeshes.Count);
                writer.WriteBlockArray(_staticMeshes);

                // SPR block
                writer.WriteBlockArray(new byte[] { 0x53, 0x50, 0x52 });

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
                writer.Write(new byte[] { 0x54, 0x45, 0x58 });

                _textureInfoManager.WriteTextureInfos(writer, _level);

                // Write items and AI objects
                writer.Write((uint)_items.Count);
                writer.WriteBlockArray(_items);

                writer.Write((uint)_aiItems.Count);
                writer.WriteBlockArray(_aiItems);

                // Write sound meta data
                _soundManager.WriteSoundMetadata(writer);

                // Finish it
                writer.Write((short)0);
                writer.Write((short)0);
                writer.Write((short)0);

                geometryDataBuffer = geometryDataStream.ToArray();
            }

            using (var writer = new BinaryWriterEx(new FileStream(_dest, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                ReportProgress(90, "Writing final level");
                writer.WriteBlockArray(new byte[] { 0x54, 0x52, 0x34, 0x00 });

                ReportProgress(91, "Writing textures");

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
                byte[] geometryData = null;
                int geometryDataUncompressedSize = -1;

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
                using (Task GeometryDataTask = Task.Factory.StartNew(() =>
                {
                    geometryData = ZLib.CompressData(geometryDataBuffer);
                    geometryDataUncompressedSize = geometryDataBuffer.Length;
                }))
                    Task.WaitAll(Texture32task, Texture16task, textureMiscTask, GeometryDataTask);

                // Write data
                ReportProgress(96, "Writing compressed data to file.");

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

                ReportProgress(97, "Writing WAVE sounds");
                _soundManager.WriteSoundData(writer);

                // Write NG header
                if(!string.IsNullOrEmpty(ngVersion))
                {
                    ReportProgress(98, "Writing NG header");
                    WriteNgHeader(writer, ngVersion);
                }

                ReportProgress(99, "Done");
            }
        }
    }
}
