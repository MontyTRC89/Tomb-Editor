using NAudio.Flac;
using System;
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
                    _tempRooms[r].Write(writer);

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
                            foreach (var bt in poly.Binormals)
                                writer.Write(bt);
                        }
                    }
                }

                writer.Write((uint)_meshTrees.Count);
                writer.WriteBlockArray(_meshTrees);

                writer.Write((uint)_moveables.Count);
                foreach (var moveable in _moveables)
                {
                    writer.Write(moveable.ObjectID);
                    writer.Write(moveable.NumMeshes);
                    writer.Write(moveable.StartingMesh);
                    writer.Write(moveable.MeshTree);
                    writer.Write(moveable.NumAnimations);

                    foreach (var animation in moveable.Animations)
                    {
                        writer.Write((uint)animation.StateID);
                        writer.Write((uint)animation.FrameEnd);
                        writer.Write((uint)animation.NextAnimation);
                        writer.Write((uint)animation.NextFrame);
                        writer.Write((uint)animation.FrameRate);
                        writer.Write(animation.VelocityStart);
                        writer.Write(animation.VelocityEnd);

                        writer.Write((uint)animation.KeyFrames.Count);
                        foreach (var keyFrame in animation.KeyFrames)
                        {
                            var center = new Vector3(
                                keyFrame.BoundingBox.X1 + keyFrame.BoundingBox.X2,
                                keyFrame.BoundingBox.Y1 + keyFrame.BoundingBox.Y2,
                                keyFrame.BoundingBox.Z1 + keyFrame.BoundingBox.Z2) / 2;
                            var extents = new Vector3(
                                keyFrame.BoundingBox.X2 - keyFrame.BoundingBox.X1,
                                keyFrame.BoundingBox.Y2 - keyFrame.BoundingBox.Y1,
                                keyFrame.BoundingBox.Z2 - keyFrame.BoundingBox.Z1) / 2;

                            writer.Write(center);
                            writer.Write(extents);
                            writer.Write(keyFrame.Offset);
                            writer.Write((uint)keyFrame.Angles.Count);

                            foreach (var angle in keyFrame.Angles)
                                writer.Write(angle);
                        }

                        writer.Write((uint)animation.StateChanges.Count);
                        writer.WriteBlockArray(animation.StateChanges);

                        writer.Write((uint)animation.NumAnimCommands);
                        foreach (var element in animation.CommandData)
                        {
                            if (element is int intComponent)
                            {
                                writer.Write(intComponent);
                            }
                            else if (element is Vector3 vector3Component)
                            {
                                writer.Write(vector3Component);
                            }
                        }
                    }
                }

                writer.Write((uint)_staticMeshes.Count);
                writer.WriteBlockArray(_staticMeshes);

                // SPR block
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

                int zoneCount = Enum.GetValues(typeof(ZoneType)).Length;
                writer.Write(zoneCount);

                foreach (int flipped in new[] { 0, 1 })
                    for (int i = 0; i < zoneCount; i++)
                        _zones.ForEach(z => writer.Write(z.Zones[flipped][i]));

                // Write animated textures
                _textureInfoManager.WriteAnimatedTextures(writer);

                // Write items and AI objects
                writer.Write((uint)_items.Count);
                foreach (var item in _items)
                {
                    writer.Write(item.ObjectID);
                    writer.Write(item.Room);
                    writer.Write(item.X);
                    writer.Write(item.Y);
                    writer.Write(item.Z);
                    writer.Write(item.Yaw);
                    writer.Write(item.Pitch);
                    writer.Write(item.Roll);
                    writer.Write(item.Color);
                    writer.Write(item.OCB);
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
                    writer.Write(item.Yaw);
                    writer.Write(item.Pitch);
                    writer.Write(item.Roll);
                    writer.Write(item.OCB);
                    writer.Write(item.Flags);
                    writer.Write(item.BoxIndex);
                    writer.Write(item.LuaName);
                }

                // Write event sets
                int eventSetCount = _level.Settings.GlobalEventSets.Count + _level.Settings.VolumeEventSets.Count;
                writer.Write((uint)eventSetCount);

                if (eventSetCount > 0)
                {
                    writer.Write((uint)_level.Settings.GlobalEventSets.Count);
                    foreach (GlobalEventSet set in _level.Settings.GlobalEventSets)
                        set.Write(writer, _level.Settings.GlobalEventSets);

                    writer.Write((uint)_level.Settings.VolumeEventSets.Count);
                    foreach (VolumeEventSet set in _level.Settings.VolumeEventSets)
                        set.Write(writer, _level.Settings.VolumeEventSets);
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
                    WriteTextureData(writer);

                    ReportProgress(97, "Writing geometry data...");
                    byte[] geometryData = geometryDataBuffer;
                    writer.Write(geometryData);

                    ReportProgress(98, "Writing sound data...");
                    WriteSoundData(writer);
                }

                ReportProgress(99, "Compressing level...");

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

            ReportProgress(100, "Done");
        }
    }
}
