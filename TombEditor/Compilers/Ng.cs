using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombEditor.Geometry;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombEditor.Compilers
{
    public sealed partial class LevelCompilerTr4
    {
        private void WriteNgHeader(BinaryWriter writer)
        {
            var ngleStartSignature = System.Text.ASCIIEncoding.ASCII.GetBytes("NG");
            var endSignature = System.Text.ASCIIEncoding.ASCII.GetBytes("NGLE");
            var startOffset = writer.BaseStream.Position;

            // Write start signature
            writer.Write(ngleStartSignature);

            // Write chunks
            WriteNgChunkLevelFlags(writer);
            WriteNgChunkExtraRoomFlags(writer);
            WriteNgChunkStaticsTable(writer);
            WriteNgChunkAnimatedTextures(writer);
            WriteNgChunkMoveablesTable(writer);
            WriteNgChunkPluginsNames(writer);
            WriteNgChunkIdFloorTable(writer);
            WriteNgChunkRemapRooms(writer);
            WriteNgChunkVersion(writer);

            // Write end signature
            writer.Write(endSignature);
            writer.Write((int)(writer.BaseStream.Position + 4 - startOffset));
        }

        private void WriteNgChunkAnimatedTextures(BinaryWriter writer)
        {
            var startOfChunk = writer.BaseStream.Position;

            writer.Write((ushort)0);
            writer.Write((ushort)0x8002);

            // Count number of textures with UVRotate
            var numUvRotate = (short)0;
            foreach (var set in _objectTextureManager.CompiledAnimatedTextures)
                if (set.AnimationType == AnimatedTextureAnimationType.FullRotate ||
                    set.AnimationType == AnimatedTextureAnimationType.HalfRotate ||
                    set.AnimationType == AnimatedTextureAnimationType.RiverRotate)
                    numUvRotate++;
            writer.Write((short)(numUvRotate + 0x0100));
            writer.Write((short)_objectTextureManager.CompiledAnimatedTextures.Count);

            // Array VetInfoRangeAnim
            for (var i = 0; i < 40; i++)
            {
                if (i >= _objectTextureManager.CompiledAnimatedTextures.Count)
                    writer.Write((short)0);
                else
                {
                    var param = (ushort)0;
                    var set = _objectTextureManager.CompiledAnimatedTextures[i];

                    switch (set.AnimationType)
                    {
                        case AnimatedTextureAnimationType.Frames:
                            param = 0x00;
                            param |= (ushort)(set.Delay & 0x1FFF);
                            break;
                        case AnimatedTextureAnimationType.PFrames:
                            param = 0x4000;
                            break;
                        case AnimatedTextureAnimationType.FullRotate:
                            param = 0x8000;
                            param |= (ushort)((set.Fps << 8) & 0x1F00);
                            param |= (ushort)(set.UvRotate & 0x00FF);
                            break;
                        case AnimatedTextureAnimationType.RiverRotate:
                            param = 0xA000;
                            param |= (ushort)((set.Fps << 8) & 0x1F00);
                            param |= (ushort)(set.UvRotate & 0x00FF);
                            break;
                        case AnimatedTextureAnimationType.HalfRotate:
                            param = 0xC000;
                            param |= (ushort)((set.Fps << 8) & 0x1F00);
                            param |= (ushort)(set.UvRotate & 0x00FF);
                            break;
                    }

                    writer.Write(param);
                }
            }

            // Array VetFromTex
            for (var i = 0; i < 40; i++)
                writer.Write((ushort)0);

            // Array VetToTex
            for (var i = 0; i < 40; i++)
                writer.Write((ushort)0);

            var sizeDefault = (short)64;
            writer.Write(sizeDefault);

            var endOfChunk = writer.BaseStream.Position;
            var numWords = (endOfChunk - startOfChunk) / 2;
            writer.Seek((int)startOfChunk, SeekOrigin.Begin);
            writer.Write((ushort)numWords);
            writer.Seek((int)endOfChunk, SeekOrigin.Begin);
        }

        private void WriteNgChunkRemapRooms(BinaryWriter writer)
        {
            writer.Write((ushort)(2 + _level.Rooms.Length));
            writer.Write((ushort)0x8037);

            for (var i = 0; i < _level.Rooms.Length; i++)
            {
                if (_level.Rooms[i] == null)
                    writer.Write((short)-1);
                else
                    writer.Write((short)_roomsRemappingDictionary[_level.Rooms[i]]);
            }
        }

        private void WriteNgChunkExtraRoomFlags(BinaryWriter writer)
        {
            writer.Write((ushort)(3 + _tempRooms.Count * 4));
            writer.Write((ushort)0x8009);

            writer.Write((ushort)_tempRooms.Count);
            for (var i = 0; i < _tempRooms.Count; i++)
            {
                var waterLevel = (byte)_tempRooms.ElementAt(i).Key.WaterLevel;
                if (waterLevel != 0) waterLevel--;

                var buffer = new byte[] { waterLevel, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; // TODO: ask Paolone for water level 
                writer.Write(buffer);
            }
        }

        private void WriteNgChunkVersion(BinaryWriter writer)
        {
            var buffer = new byte[] { 0x07, 0x00, 0x25, 0x80, 0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            writer.Write(buffer);

            buffer = new byte[] { 0x07, 0x00, 0x24, 0x80, 0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            writer.Write(buffer);
        }

        private void WriteNgChunkPluginsNames(BinaryWriter writer)
        {
            var buffer = new byte[] { 0x03, 0x00, 0x47, 0x80, 0x00, 0x00 };
            writer.Write(buffer);
        }

        private void WriteNgChunkIdFloorTable(BinaryWriter writer)
        {
            var buffer = new byte[] { 0x03, 0x00, 0x48, 0x80, 0x00, 0x00 };
            writer.Write(buffer);
        }

        private void WriteNgChunkLevelFlags(BinaryWriter writer)
        {
            var flags = 0x01;
            if (_level.Wad.SoundMapSize != TrCatalog.GetSoundMapSize(TombRaiderVersion.TR4, false)) flags |= 0x02;
            var buffer = new byte[] { 0x04, 0x00, 0x0D, 0x80 };
            writer.Write(buffer);
            writer.Write((int)flags);
        }

        private void WriteNgChunkMoveablesTable(BinaryWriter writer)
        {
            writer.Write((ushort)(2 + _level.GlobalScriptingIdsTable.Length));
            writer.Write((ushort)0x8005);

            for (var i = 0; i < _level.GlobalScriptingIdsTable.Length; i++)
            {
                if (_level.GlobalScriptingIdsTable[i] == null)
                    writer.Write((short)-1);
                else
                {
                    var instance = _level.GlobalScriptingIdsTable[i];
                    if (instance is MoveableInstance && _moveablesTable.ContainsKey(instance as MoveableInstance))
                        writer.Write((short)_moveablesTable[instance as MoveableInstance]);
                    else if (instance is CameraInstance && _cameraTable.ContainsKey(instance as CameraInstance))
                        writer.Write((short)_cameraTable[instance as CameraInstance]);
                    else if (instance is SinkInstance && _sinkTable.ContainsKey(instance as SinkInstance))
                        writer.Write((short)_sinkTable[instance as SinkInstance]);
                    else if (instance is FlybyCameraInstance && _flybyTable.ContainsKey(instance as FlybyCameraInstance))
                        writer.Write((short)_flybyTable[instance as FlybyCameraInstance]);
                    else if (instance is SoundSourceInstance && _soundSourcesTable.ContainsKey(instance as SoundSourceInstance))
                        writer.Write((short)_soundSourcesTable[instance as SoundSourceInstance]);
                    else
                        writer.Write((short)-1);
                }
            }
        }

        private void WriteNgChunkStaticsTable(BinaryWriter writer)
        {
            writer.Write((ushort)(2 + _level.GlobalScriptingIdsTable.Length * 2));
            writer.Write((ushort)0x8021);

            for (var i = 0; i < _level.GlobalScriptingIdsTable.Length; i++)
            {
                if (_level.GlobalScriptingIdsTable[i] == null)
                {
                    writer.Write((short)0);
                    writer.Write((short)-1);
                }
                else
                {
                    var instance = _level.GlobalScriptingIdsTable[i];
                    if (instance is StaticInstance)
                    {
                        var staticMesh = instance as StaticInstance;
                        writer.Write((short)_level.Rooms.ReferenceIndexOf(staticMesh.Room));
                        writer.Write((short)_staticsTable[staticMesh]);
                    }
                    else
                    {
                        writer.Write((short)0);
                        writer.Write((short)-1);
                    }
                }
            }
        }
    }
}
