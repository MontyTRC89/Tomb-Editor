﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers
{
    public sealed partial class LevelCompilerClassicTR
    {
        private Dictionary<ushort, ushort> _remappedTiles;

        private void WriteNgHeader(BinaryWriter writer, string ngVersion)
        {
            CollectNgRemappedTiles();

            var ngleStartSignature = System.Text.Encoding.ASCII.GetBytes("NG");
            var endSignature = System.Text.Encoding.ASCII.GetBytes("NGLE");
            var startOffset = writer.BaseStream.Position;

            // Write start signature
            writer.Write(ngleStartSignature);

            // Write chunks
            WriteNgChunkExtraRoomFlags(writer);
            WriteNgChunkStaticsTable(writer);
            WriteNgChunkAnimatedTextures(writer);
            WriteNgChunkMoveablesTable(writer);
            WriteNgChunkPluginsNames(writer);
            WriteNgChunkTexPartial(writer);
            WriteNgChunkIdFloorTable(writer);
            WriteNgChunkLevelFlags(writer);
            WriteNgChunkRemapRooms(writer);
            WriteNgChunkTomVersion(writer, ngVersion);
            WriteNgChunkRemappedTails(writer);
            WriteNgChunkLevelVersion(writer, ngVersion);

            // Write end signature

            writer.Write((int)0);
            writer.Write(endSignature);
            writer.Write((int)(writer.BaseStream.Position + 4 - startOffset));
        }

        private void CollectNgRemappedTiles()
        {
            _remappedTiles = new Dictionary<ushort, ushort>();

            foreach (var set in _textureInfoManager.AnimatedTextures)
            {
                foreach (var frame in set.Value)
                    if (!_remappedTiles.ContainsKey(frame))
                        _remappedTiles.Add(frame, (ushort)_level.Settings.AnimatedTextureSets.IndexOf(set.Key));
            }
        }

        private void WriteNgChunkAnimatedTextures(BinaryWriter writer)
        {
            var startOfChunk = writer.BaseStream.Position;

            writer.Write((ushort)0);
            writer.Write((ushort)0x8002);

            // Count number of textures with UVRotate
            writer.Write((byte)0x01);
            writer.Write(checked((byte)_textureInfoManager.UvRotateCount));
            writer.Write((short)_textureInfoManager.AnimatedTextures.Count);

            // Array VetInfoRangeAnim
            for (var i = 0; i < 40; i++)
            {
                if (i >= _textureInfoManager.AnimatedTextures.Count)
                    writer.Write((short)0);
                else
                {
                    var param = (ushort)0;
                    var set = _textureInfoManager.AnimatedTextures[i].Key;

                    switch (set.AnimationType)
                    {
                        case AnimatedTextureAnimationType.Frames:
                            param |= (ushort)MathC.Clamp(Math.Round(1000.0f / (set.Fps == 0 ? 16 : set.Fps)), 0, 0x1fff);
                            break;
                        case AnimatedTextureAnimationType.PFrames:
                            param = 0x4000;
                            break;
                        case AnimatedTextureAnimationType.UVRotate:
                            param = 0x8000;
                            param |= (ushort)(set.Fps > 31 ? 0 : (int)MathC.Clamp(Math.Round(set.Fps), 0, 0x1F) << 8); // 0 means 'MAX FPS'
                            param |= (ushort)(set.UvRotate & 0x00FF);
                            break;
                        case AnimatedTextureAnimationType.RiverRotate:
                            param = 0xA000;
                            param |= (ushort)(set.Fps > 31 ? 0 : (int)MathC.Clamp(Math.Round(set.Fps), 0, 0x1F) << 8); // 0 means 'MAX FPS'
                            param |= (ushort)(set.UvRotate & 0x00FF);
                            break;
                        case AnimatedTextureAnimationType.HalfRotate:
                            param = 0xC000;
                            param |= (ushort)(set.Fps > 31 ? 0 : (int)MathC.Clamp(Math.Round(set.Fps), 0, 0x1F) << 8); // 0 means 'MAX FPS'
                            param |= (ushort)(set.UvRotate & 0x00FF);
                            break;
                        default:
                            throw new NotSupportedException("Unsupported NG animation type encountered.");
                    }

                    writer.Write(param);
                }
            }

            // Array VetFromTex
            for (var i = 0; i < 40; i++)
            {
                if (i >= _textureInfoManager.AnimatedTextures.Count)
                    writer.Write((short)0);
                else
                {
                    var set = _textureInfoManager.AnimatedTextures[i];
                    writer.Write(_remappedTiles[set.Value.First()]);
                }
            }

            // Array VetToTex
            for (var i = 0; i < 40; i++)
            {
                if (i >= _textureInfoManager.AnimatedTextures.Count)
                    writer.Write((short)0);
                else
                {
                    var set = _textureInfoManager.AnimatedTextures[i];
                    writer.Write(_remappedTiles[set.Value.Last()]);
                }
            }

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
            writer.Write((ushort)(3 + _roomsUnmapping.Count * 4));
            writer.Write((ushort)0x8009);

            writer.Write((ushort)_roomsUnmapping.Count);
            for (var i = 0; i < _roomsUnmapping.Count; i++)
            {
                Room room = _roomsUnmapping[i];
                var buffer = new byte[] { room.TypeStrength, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                writer.Write(buffer);
            }
        }

        private void WriteNgChunkLevelVersion(BinaryWriter writer, string version)
        {
            var buffer = new byte[] { 0x07, 0x00, 0x24, 0x80 };
            writer.Write(buffer);
            WriteNgVersion(writer, version);
        }

        private void WriteNgChunkTomVersion(BinaryWriter writer, string version)
        {
            var buffer = new byte[] { 0x07, 0x00, 0x25, 0x80 };
            writer.Write(buffer);
            WriteNgVersion(writer, version);
        }

        private void WriteNgChunkPluginsNames(BinaryWriter writer)
        {
            var buffer = new byte[] { 0x03, 0x00, 0x47, 0x80, 0x00, 0x00 };
            writer.Write(buffer);
        }

        private void WriteNgChunkTexPartial(BinaryWriter writer)
        {
            var buffer = new byte[] { 0x03, 0x00, 0x17, 0x80, 0x00, 0x00 };
            writer.Write(buffer);
        }

        private void WriteNgChunkRemappedTails(BinaryWriter writer)
        {
            // Do not write remapped tails chunk if there's no anim textures
            if (_textureInfoManager.AnimatedTextures.Count < 1)
                return;

            // In theory we should remap tiles from TGA but it's impossible with TE
            // But writing fake remappings seems to make UVRotate textures finally working
            var startOfChunk = writer.BaseStream.Position;

            writer.Write((ushort)0);
            writer.Write((ushort)0x8018);

            writer.Write((ushort)_remappedTiles.Count);
            foreach (ushort textureId in _remappedTiles.Keys.ToArray())
            {
                writer.Write((ushort)_remappedTiles[textureId]);
                writer.Write((ushort)textureId);
            }
            
            var endOfChunk = writer.BaseStream.Position;
            var numWords = (endOfChunk - startOfChunk) / 2;
            writer.Seek((int)startOfChunk, SeekOrigin.Begin);
            writer.Write((ushort)numWords);
            writer.Seek((int)endOfChunk, SeekOrigin.Begin);
        }

        private void WriteNgChunkIdFloorTable(BinaryWriter writer)
        {
            var buffer = new byte[] { 0x03, 0x00, 0x48, 0x80, 0x00, 0x00 };
            writer.Write(buffer);
        }

        private void WriteNgChunkLevelFlags(BinaryWriter writer)
        {
            int flags = 0x01;
            if (_level.Settings.GameVersion == TRVersion.Game.TRNG)
                flags |= 0x02;
            var buffer = new byte[] { 0x04, 0x00, 0x0D, 0x80 };
            writer.Write(buffer);
            writer.Write(flags);
        }

        private void WriteNgChunkMoveablesTable(BinaryWriter writer)
        {
            writer.Write((ushort)(2 + _scriptingIdsTable.Length));
            writer.Write((ushort)0x8005);

            for (var i = 0; i < _scriptingIdsTable.Length; i++)
            {
                if (_scriptingIdsTable[i] == null)
                    writer.Write((short)-1);
                else
                {
                    var instance = _scriptingIdsTable[i];
                    if (instance is MoveableInstance && _moveablesTable.ContainsKey(instance as MoveableInstance))
                        writer.Write((short)_moveablesTable[instance as MoveableInstance]);
                    else if (instance is CameraInstance && _cameraTable.ContainsKey(instance as CameraInstance))
                        writer.Write((short)_cameraTable[instance as CameraInstance]);
                    else if (instance is SinkInstance && _sinkTable.ContainsKey(instance as SinkInstance))
                        writer.Write((short)_sinkTable[instance as SinkInstance]);
                    else if (instance is FlybyCameraInstance && _flybyTable.ContainsKey(instance as FlybyCameraInstance))
                        writer.Write((short)((instance as FlybyCameraInstance).Sequence));
                    else if (instance is SoundSourceInstance && _soundSourcesTable.ContainsKey(instance as SoundSourceInstance))
                        writer.Write((short)_soundSourcesTable[instance as SoundSourceInstance]);
                    else
                        writer.Write((short)-1);
                }
            }
        }

        private void WriteNgChunkStaticsTable(BinaryWriter writer)
        {
            writer.Write((ushort)(2 + _scriptingIdsTable.Length * 2));
            writer.Write((ushort)0x8021);

            for (var i = 0; i < _scriptingIdsTable.Length; i++)
            {
                if (_scriptingIdsTable[i] == null)
                {
                    writer.Write((short)0);
                    writer.Write((short)-1);
                }
                else
                {
                    var instance = _scriptingIdsTable[i];
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

        private void WriteNgVersion(BinaryWriter writer, string version)
        {
            // Parse NGLE version string
            var verChunks = version.Split(',');

            for (int i = 0; i < 4; i++)
            {
                ushort number = 0;
                if(i < verChunks.Length) ushort.TryParse(verChunks[i], out number);
                writer.Write(number);
            }

            writer.Write((ushort)0);
        }
    }
}
