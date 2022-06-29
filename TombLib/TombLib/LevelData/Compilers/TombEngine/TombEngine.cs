﻿using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.TombEngine
{
    public sealed partial class LevelCompilerTombEngine
    {
        private void WriteLevelTombEngine()
        {
            // Now begin to compile the geometry block in a MemoryStream
            byte[] geometryDataBuffer;
            using (var geometryDataStream = new MemoryStream())
            {
                var writer = new BinaryWriterEx(geometryDataStream); // Don't dispose
                ReportProgress(80, "Writing geometry data to memory buffer");

                var numRooms = (uint)_level.Rooms.Count(r => r != null);
                writer.Write(numRooms);

                foreach (var r in _level.ExistingRooms)
                {
                    _tempRooms[r].Write(writer);
                }

                // Write floordata
                var numFloorData = (uint)_floorData.Count;
                writer.Write(numFloorData);
                writer.WriteBlockArray(_floorData);

                // Write meshes
                writer.Write(_meshes.Count);
                foreach (var mesh in _meshes)
                {
                    writer.Write((byte)mesh.LightingType);

                    writer.Write( mesh.Sphere.Center.X);
                    writer.Write(-mesh.Sphere.Center.Y);
                    writer.Write( mesh.Sphere.Center.Z);
                    writer.Write( mesh.Sphere.Radius);

                    writer.Write(mesh.Vertices.Count);
                    foreach (var p in mesh.Vertices)
                        writer.Write(p.Position);
                    foreach (var c in mesh.Vertices)
                        writer.Write(c.Color);
                    foreach (var e in mesh.Vertices)
                        writer.Write(new Vector3(e.Glow, e.Move, e.Locked ? 0 : 1));
                    foreach (var b in mesh.Vertices)
                        writer.Write(b.Bone);

                    writer.Write(mesh.Buckets.Count);
                    foreach (var bucket in mesh.Buckets.Values)
                    {
                        writer.Write(bucket.Material.Texture);
                        writer.Write(bucket.Material.BlendMode);
                        writer.Write(bucket.Material.Animated);
                        writer.Write(bucket.Polygons.Count);
                        foreach (var poly in bucket.Polygons)
                        {
                            writer.Write((int)poly.Shape);
                            writer.Write((int)poly.AnimatedSequence);
                            writer.Write((int)poly.AnimatedFrame);
                            writer.Write((float)poly.ShineStrength);
                            foreach (int index in poly.Indices)
                                writer.Write(index);
                            foreach (var uv in poly.TextureCoordinates)
                                writer.Write(uv);
                            foreach (var n in poly.Normals)
                                writer.Write(n);
                            foreach (var t in poly.Tangents)
                                writer.Write(t);
                            foreach (var bt in poly.Bitangents)
                                writer.Write(bt);
                        }
                    }
                }

                // Write animations' data
                writer.Write((uint)_animations.Count);
                writer.WriteBlockArray(_animations);

                writer.Write((uint)_stateChanges.Count);
                writer.WriteBlockArray(_stateChanges);

                writer.Write((uint)_animDispatches.Count);
                writer.WriteBlockArray(_animDispatches);

                writer.Write((uint)_animCommands.Count);
                writer.WriteBlockArray(_animCommands);

                writer.Write((uint)_meshTrees.Count);
                writer.WriteBlockArray(_meshTrees);

                writer.Write((uint)_frames.Count);
                foreach (var frame in _frames)
                {
                    writer.Write((short)frame.BoundingBox.X1);
                    writer.Write((short)frame.BoundingBox.X2);
                    writer.Write((short)frame.BoundingBox.Y1);
                    writer.Write((short)frame.BoundingBox.Y2);
                    writer.Write((short)frame.BoundingBox.Z1);
                    writer.Write((short)frame.BoundingBox.Z2);
                    writer.Write((short)frame.Offset.X);
                    writer.Write((short)frame.Offset.Y);
                    writer.Write((short)frame.Offset.Z);
                    writer.Write((short)frame.Angles.Count);
                    foreach (var angle in frame.Angles)
                        writer.Write(angle);
                }

                writer.Write((uint)_moveables.Count);
                for (var k = 0; k < _moveables.Count; k++)
                {
                    writer.WriteBlock(_moveables[k]);
                    writer.Write((ushort)0xfeff);
                }

                writer.Write((uint)_staticMeshes.Count);
                writer.WriteBlockArray(_staticMeshes);

                // SPR block
                writer.WriteBlockArray(new byte[] { 0x53, 0x50, 0x52, 0x00 });

                writer.Write((uint)_spriteTextures.Count);
                writer.WriteBlockArray(_spriteTextures);

                writer.Write((uint)_spriteSequences.Count);
                writer.WriteBlockArray(_spriteSequences);

                // Write camera, flyby and sound sources
                writer.Write((uint)_cameras.Count);
                foreach (var camera in _cameras)
                {
                    writer.Write(camera.X);
                    writer.Write(camera.Y);
                    writer.Write(camera.Z);
                    writer.Write(camera.Room);
                    writer.Write(camera.Flags);
                    writer.Write(camera.Speed);
                    writer.Write(camera.LuaName);
                }

                writer.Write((uint)_flyByCameras.Count);
                writer.WriteBlockArray(_flyByCameras);

                writer.Write((uint)_sinks.Count);
                foreach (var sink in _sinks)
                {
                    writer.Write(sink.X);
                    writer.Write(sink.Y);
                    writer.Write(sink.Z);
                    writer.Write(sink.Strength);
                    writer.Write(sink.BoxIndex);
                    writer.Write(sink.LuaName);
                }

                writer.Write((uint)_soundSources.Count);
                foreach (var source in _soundSources)
                {
                    writer.Write(source.X);
                    writer.Write(source.Y);
                    writer.Write(source.Z);
                    writer.Write(source.SoundID);
                    writer.Write(source.Flags);
                    writer.Write(source.LuaName);
                }

                // Write pathfinding data
                writer.Write((uint)_boxes.Count);
                writer.WriteBlockArray(_boxes);

                writer.Write((uint)_overlaps.Count);
                writer.WriteBlockArray(_overlaps);

                for (var i = 0; i < _boxes.Count; i++)
                    writer.Write(_zones[i].GroundZone1_Normal);
                for (var i = 0; i < _boxes.Count; i++)
                    writer.Write(_zones[i].GroundZone2_Normal);
                for (var i = 0; i < _boxes.Count; i++)
                    writer.Write(_zones[i].GroundZone3_Normal);
                for (var i = 0; i < _boxes.Count; i++)
                    writer.Write(_zones[i].GroundZone4_Normal);
                for (var i = 0; i < _boxes.Count; i++)
                    writer.Write(_zones[i].GroundZone5_Normal);
                for (var i = 0; i < _boxes.Count; i++)
                    writer.Write(_zones[i].FlyZone_Normal);
                for (var i = 0; i < _boxes.Count; i++)
                    writer.Write(_zones[i].GroundZone1_Alternate);
                for (var i = 0; i < _boxes.Count; i++)
                    writer.Write(_zones[i].GroundZone2_Alternate);
                for (var i = 0; i < _boxes.Count; i++)
                    writer.Write(_zones[i].GroundZone3_Alternate);
                for (var i = 0; i < _boxes.Count; i++)
                    writer.Write(_zones[i].GroundZone4_Alternate);
                for (var i = 0; i < _boxes.Count; i++)
                    writer.Write(_zones[i].GroundZone5_Alternate);
                for (var i = 0; i < _boxes.Count; i++)
                    writer.Write(_zones[i].FlyZone_Alternate);

                // Write animated textures
                _textureInfoManager.WriteAnimatedTextures(writer);

                // Write object textures
                writer.Write(checked((byte)_textureInfoManager.UvRotateCount));
                writer.Write(new byte[] { 0x54, 0x45, 0x58, 0x00 });

                _textureInfoManager.WriteTextureInfosTombEngine(writer, _level);

                // Write items and AI objects
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
                    writer.Write(item.Ocb);
                    writer.Write(item.Flags);
                    writer.Write(item.LuaName);
                }

                writer.Write((uint)_aiItems.Count);
                foreach (var item in _aiItems)
                {
                    writer.Write(item.ObjectID);
                    writer.Write(item.Room);
                    writer.Write(item.X);
                    writer.Write(item.Y);
                    writer.Write(item.Z);
                    writer.Write(item.OCB);
                    writer.Write(item.Flags);
                    writer.Write(item.Angle);
                    writer.Write(item.BoxIndex);
                    writer.Write(item.LuaName);
                }

                // Write LUA function names
                writer.Write((uint)_luaFunctions.Count);
                foreach (string functionName in _luaFunctions)
                {
                    writer.Write(functionName);
                }

                // Write sound meta data
                PrepareSoundsData();
                WriteSoundMetadata(writer);

                geometryDataBuffer = geometryDataStream.ToArray();
            }

            using (var inStream = new MemoryStream())
            {
                using (var writer = new BinaryWriterEx(inStream, true))
                {
                    ReportProgress(90, "Writing final level");
                   
                    // Rooms atlas
                    writer.Write(_textureInfoManager.RoomsAtlas.Count);
                    foreach (var atlas in _textureInfoManager.RoomsAtlas)
                    {
                        writer.Write(atlas.ColorMap.Width);
                        writer.Write(atlas.ColorMap.Height);
                        using (var ms = new MemoryStream())
                        {
                            atlas.ColorMap.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            var output = RemoveColorChunks(ms);
                            writer.Write((int)output.Length);
                            writer.Write(output.ToArray());
                        }
                        writer.Write(atlas.HasNormalMap);
                        if (atlas.HasNormalMap)
                        {
                            using (var ms = new MemoryStream())
                            {
                                atlas.NormalMap.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                var output = RemoveColorChunks(ms);
                                writer.Write((int)output.Length);
                                writer.Write(output.ToArray());
                            }
                        }
                    }

                    // Moveables atlas
                    writer.Write(_textureInfoManager.MoveablesAtlas.Count);
                    foreach (var atlas in _textureInfoManager.MoveablesAtlas)
                    {
                        writer.Write(atlas.ColorMap.Width);
                        writer.Write(atlas.ColorMap.Height);
                        using (var ms = new MemoryStream())
                        {
                            atlas.ColorMap.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            var output = RemoveColorChunks(ms);
                            writer.Write((int)output.Length);
                            writer.Write(output.ToArray());
                        }
                        writer.Write(atlas.HasNormalMap);
                        if (atlas.HasNormalMap)
                        {
                            using (var ms = new MemoryStream())
                            {
                                atlas.NormalMap.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                var output = RemoveColorChunks(ms);
                                writer.Write((int)output.Length);
                                writer.Write(output.ToArray());
                            }
                        }
                    }

                    // Statics atlas
                    writer.Write(_textureInfoManager.StaticsAtlas.Count);
                    foreach (var atlas in _textureInfoManager.StaticsAtlas)
                    {
                        writer.Write(atlas.ColorMap.Width);
                        writer.Write(atlas.ColorMap.Height);
                        using (var ms = new MemoryStream())
                        {
                            atlas.ColorMap.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            var output = RemoveColorChunks(ms);
                            writer.Write((int)output.Length);
                            writer.Write(output.ToArray());
                        }
                        writer.Write(atlas.HasNormalMap);
                        if (atlas.HasNormalMap)
                        {
                            using (var ms = new MemoryStream())
                            {
                                atlas.NormalMap.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                var output = RemoveColorChunks(ms);
                                writer.Write((int)output.Length);
                                writer.Write(output.ToArray());
                            }
                        }
                    }

                    // Animated atlas
                    writer.Write(_textureInfoManager.AnimatedAtlas.Count);
                    foreach (var atlas in _textureInfoManager.AnimatedAtlas)
                    {
                        writer.Write(atlas.ColorMap.Width);
                        writer.Write(atlas.ColorMap.Height);
                        using (var ms = new MemoryStream())
                        {
                            atlas.ColorMap.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            var output = RemoveColorChunks(ms);
                            writer.Write((int)output.Length);
                            writer.Write(output.ToArray());
                        }
                        writer.Write(atlas.HasNormalMap);
                        if (atlas.HasNormalMap)
                        {
                            using (var ms = new MemoryStream())
                            {
                                atlas.NormalMap.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                var output = RemoveColorChunks(ms);
                                writer.Write((int)output.Length);
                                writer.Write(output.ToArray());
                            }
                        }
                    }

                    // Sprites textures
                    writer.Write(_spritesTexturesPages.Count);
                    foreach (var atlas in _spritesTexturesPages)
                    {
                        writer.Write(atlas.Width);
                        writer.Write(atlas.Height);
                        using (var ms = new MemoryStream())
                        {
                            atlas.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            var output = RemoveColorChunks(ms);
                            writer.Write((int)output.Length);
                            writer.Write(output.ToArray());
                        }
                    }

                    // Sky texture
                    var sky = GetSkyTexture();
                    using (var ms = new MemoryStream())
                    {
                        sky.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        writer.Write(sky.Width);
                        writer.Write(sky.Height);
                        var output = RemoveColorChunks(ms);
                        writer.Write((int)output.Length);
                        writer.Write(output.ToArray());
                    }

                    // Write data
                    ReportProgress(97, "Writing level data to file.");

                    byte[] geometryData = geometryDataBuffer;
                    writer.Write(geometryData);

                    ReportProgress(98, "Writing WAVE sounds");
                    WriteSoundData(writer);
                }

                ReportProgress(90, "Compressing level...");

                inStream.Seek(0, SeekOrigin.Begin);

                var data = ZLib.CompressData(inStream);
                using (var fs = new FileStream(_dest, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var writer = new BinaryWriter(fs))
                    {
                        // TEN header
                        writer.Write(new byte[] { 0x54, 0x45, 0x4E, 0x00 });

                        // TE compiler version
                        var version = Assembly.GetExecutingAssembly().GetName().Version;
                        writer.Write(new byte[] { (byte)version.Major, (byte)version.Minor, (byte)version.Build, 0x00 });

                        // Hashed system name (reserved for quick start feature)
                        writer.Write(Math.Abs(Environment.MachineName.GetHashCode()));

                        // Geometry data
                        writer.Write((int)inStream.Length);
                        writer.Write((int)data.Length);
                        writer.Write(data, 0, data.Length);
                    }
                }
            }           

            ReportProgress(99, "Done");
        }
    }
}
