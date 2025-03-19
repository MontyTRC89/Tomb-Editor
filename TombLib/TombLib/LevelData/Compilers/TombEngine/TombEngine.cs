using NAudio.Flac;
using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using TombLib.IO;
using TombLib.Types;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.TombEngine
{
    public sealed partial class LevelCompilerTombEngine
    {
        private void WriteLevelTombEngine()
        {
            byte[] dynamicDataBuffer;
            using (var dynamicDataStream = new MemoryStream())
            {
                var writer = new BinaryWriterEx(dynamicDataStream); // Don't dispose
                ReportProgress(80, "Writing dynamic data to memory buffer");

                // Write room dynamic data
                writer.Write(_level.ExistingRooms.Count);
                foreach (var r in _level.ExistingRooms)
                    _tempRooms[r].WriteDynamicData(writer);

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

                dynamicDataBuffer = dynamicDataStream.ToArray();
            }

            // Now begin to compile the geometry block in a MemoryStream
            byte[] geometryDataBuffer;
            using (var geometryDataStream = new MemoryStream())
            {
                var writer = new BinaryWriterEx(geometryDataStream); // Don't dispose
                ReportProgress(85, "Writing geometry data to memory buffer");

                writer.Write(_level.ExistingRooms.Count);
                foreach (var r in _level.ExistingRooms)
                    _tempRooms[r].WriteStaticData(writer);

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

                writer.Write(_meshTrees.Count);
                writer.WriteBlockArray(_meshTrees);

                writer.Write(_moveables.Count);
                foreach (var moveable in _moveables)
                {
                    writer.Write(moveable.ObjectID);
                    writer.Write(moveable.NumMeshes);
                    writer.Write(moveable.StartingMesh);
                    writer.Write(moveable.MeshTree);

                    writer.Write(moveable.NumAnimations);
                    foreach (var animation in moveable.Animations)
                    {
                        writer.Write(animation.StateID);
                        writer.Write(animation.Interpolation);
                        writer.Write(animation.FrameEnd);
                        writer.Write(animation.NextAnimation);
                        writer.Write(animation.NextFrame);
                        writer.Write(animation.BlendFrameCount);
                        writer.Write(animation.BlendCurve.Start);
                        writer.Write(animation.BlendCurve.End);
                        writer.Write(animation.BlendCurve.StartHandle);
                        writer.Write(animation.BlendCurve.EndHandle);

                        var startX = new Vector2(0.0f, animation.VelocityStart.X);
                        var endX = new Vector2(1.0f, animation.VelocityEnd.X);
                        var fixedMotionCurveX = new BezierCurve2D(startX, endX, startX, endX);
                        writer.Write(fixedMotionCurveX.Start);
                        writer.Write(fixedMotionCurveX.End);
                        writer.Write(fixedMotionCurveX.StartHandle);
                        writer.Write(fixedMotionCurveX.EndHandle);

                        var startY = new Vector2(0.0f, animation.VelocityStart.Y);
                        var endY = new Vector2(1.0f, animation.VelocityEnd.Y);
                        var fixedMotionCurveY = new BezierCurve2D(startY, endY, startY, endY);
                        writer.Write(fixedMotionCurveY.Start);
                        writer.Write(fixedMotionCurveY.End);
                        writer.Write(fixedMotionCurveY.StartHandle);
                        writer.Write(fixedMotionCurveY.EndHandle);
                        
                        var startZ = new Vector2(0.0f, animation.VelocityStart.Z);
                        var endZ = new Vector2(1.0f, animation.VelocityEnd.Z);
                        var fixedMotionCurveZ = new BezierCurve2D(startZ, endZ, startZ, endZ);
                        writer.Write(fixedMotionCurveZ.Start);
                        writer.Write(fixedMotionCurveZ.End);
                        writer.Write(fixedMotionCurveZ.StartHandle);
                        writer.Write(fixedMotionCurveZ.EndHandle);

                        writer.Write(animation.KeyFrames.Count);
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
                            writer.Write(keyFrame.RootOffset);
                            
                            writer.Write(keyFrame.BoneOrientations.Count);
                            writer.WriteBlockArray(keyFrame.BoneOrientations);
                        }

                        writer.Write(animation.StateChanges.Count);
                        foreach (var stateChange in animation.StateChanges)
                        {
                            writer.Write(stateChange.StateID);
                            writer.Write(stateChange.FrameLow);
                            writer.Write(stateChange.FrameHigh);
                            writer.Write(stateChange.NextAnimation);
                            writer.Write(stateChange.NextFrameLow);
                            writer.Write(stateChange.BlendEndFrame);
                            writer.Write(stateChange.BlendFrameCount);
                            writer.Write(stateChange.BlendCurve.Start);
                            writer.Write(stateChange.BlendCurve.End);
                            writer.Write(stateChange.BlendCurve.StartHandle);
                            writer.Write(stateChange.BlendCurve.EndHandle);
                        }

                        writer.Write(animation.NumAnimCommands);
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

                        writer.Write(animation.Flags);
                    }
                }

                writer.Write(_staticMeshes.Count);
                writer.WriteBlockArray(_staticMeshes);

                // SPR block
                writer.Write(_spriteTextures.Count);
                writer.WriteBlockArray(_spriteTextures);

                writer.Write(_spriteSequences.Count);
                writer.WriteBlockArray(_spriteSequences);

                // Write pathfinding data
                writer.Write(_boxes.Count);
                writer.WriteBlockArray(_boxes);

                writer.Write(_overlaps.Count);
                writer.WriteBlockArray(_overlaps);

                int zoneCount = Enum.GetValues(typeof(ZoneType)).Length;
                writer.Write(zoneCount);

                foreach (int flipped in new[] { 0, 1 })
                    for (int i = 0; i < zoneCount; i++)
                        _zones.ForEach(z => writer.Write(z.Zones[flipped][i]));

                // Write mirrors
                writer.Write(_mirrors.Count);
                foreach (var mirror in _mirrors)
                {
                    writer.Write(mirror.Room);
                    writer.Write(mirror.Plane.X);
                    writer.Write(mirror.Plane.Y);
                    writer.Write(mirror.Plane.Z);
                    writer.Write(mirror.Plane.W);
                    writer.Write(mirror.ReflectLara);
                    writer.Write(mirror.ReflectMoveables);
                    writer.Write(mirror.ReflectStatics);
                    writer.Write(mirror.ReflectSprites);
                    writer.Write(mirror.ReflectLights);
                }

                // Write animated textures
                _textureInfoManager.WriteAnimatedTextures(writer);

                geometryDataBuffer = geometryDataStream.ToArray();
            }

            using (var mediaStream = new MemoryStream())
            {
                using (var writer = new BinaryWriterEx(mediaStream, true))
                {
                    WriteTextureData(writer);

                    // Write sound meta data
                    PrepareSoundsData();
                    WriteSoundMetadata(writer);
                    WriteSoundData(writer);
                }

                ReportProgress(95, "Compressing level...");

                mediaStream.Seek(0, SeekOrigin.Begin);

                var mediaBlock    = ZLib.CompressData(mediaStream, System.IO.Compression.CompressionLevel.SmallestSize);
                var geometryBlock = ZLib.CompressData(geometryDataBuffer, System.IO.Compression.CompressionLevel.SmallestSize);
                var dynamicBlock  = ZLib.CompressData(dynamicDataBuffer, System.IO.Compression.CompressionLevel.Optimal);

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

                        // Checksum to detect incorrect level version on rapid reload
                        int checksum = Checksum.Calculate(mediaBlock) ^ Checksum.Calculate(geometryBlock);
                        writer.Write(checksum);

                        // Audiovisual data (textures and sounds)
                        writer.Write((int)mediaStream.Length);
                        writer.Write((int)mediaBlock.Length);
                        writer.Write(mediaBlock, 0, mediaBlock.Length);
                        ReportProgress(96, $"    Media data size: " + TextExtensions.ToDataSize(mediaBlock.Length));

                        // Geometry data
                        writer.Write((int)geometryDataBuffer.Length);
                        writer.Write((int)geometryBlock.Length);
                        writer.Write(geometryBlock, 0, geometryBlock.Length);
                        ReportProgress(96, $"    Geometry data size: " + TextExtensions.ToDataSize(geometryBlock.Length));

                        // Dynamic data
                        writer.Write((int)dynamicDataBuffer.Length);
                        writer.Write((int)dynamicBlock.Length);
                        writer.Write(dynamicBlock, 0, dynamicBlock.Length);
                        ReportProgress(96, $"    Dynamic data size: " + TextExtensions.ToDataSize(dynamicBlock.Length));
                    }
                }
            }

            ReportProgress(100, "Done");
        }
    }
}
