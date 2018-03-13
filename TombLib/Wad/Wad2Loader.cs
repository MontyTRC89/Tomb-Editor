using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using TombLib.IO;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public static class Wad2Loader
    {
        public static Wad2 LoadFromFile(string fileName)
        {
            Wad2 result;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                result = LoadFromStream(fileStream);
            result.FileName = fileName;
            return result;
        }

        public static Wad2 LoadFromStream(Stream stream)
        {
            byte[] magicNumber = new byte[4];
            stream.Read(magicNumber, 0, 4);
            stream.Seek(-4, SeekOrigin.Current);
            if (magicNumber.SequenceEqual(Wad2Chunks.MagicNumberObsolete))
            {
                // TODO In the long term it would be good to get rid of this obsolete code.
                using (var chunkIO = new ChunkReader(Wad2Chunks.MagicNumberObsolete, stream))
                    return LoadWad2(chunkIO, true);
            }
            else
            {
                using (var chunkIO = new ChunkReader(Wad2Chunks.MagicNumber, stream))
                    return LoadWad2(chunkIO, false);
            }
        }

        private static Wad2 LoadWad2(ChunkReader chunkIO, bool obsolete)
        {
            if (obsolete)
                LEB128.ReadUInt(chunkIO.Raw);
            var wad = new Wad2();

            Dictionary<long, WadTexture> textures = null;
            Dictionary<long, WadSample> samples = null;
            Dictionary<long, WadSoundInfo> soundInfos = null;
            Dictionary<long, WadMesh> meshes = null;
            Dictionary<long, WadSprite> sprites = null;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id == Wad2Chunks.SuggestedGameVersion)
                {
                    wad.SuggestedGameVersion = (WadGameVersion)chunkIO.ReadChunkLong(chunkSize);
                    return true;
                }
                if (LoadTextures(chunkIO, id, ref textures))
                    return true;
                else if (LoadSamples(chunkIO, id, ref samples))
                    return true;
                else if (LoadSoundInfos(chunkIO, id, ref soundInfos, samples))
                    return true;
                else if (LoadFixedSoundInfos(chunkIO, id, wad, soundInfos))
                    return true;
                else if (LoadMeshes(chunkIO, id, ref meshes, textures))
                    return true;
                else if (LoadSprites(chunkIO, id, ref sprites))
                    return true;
                else if (LoadSpriteSequences(chunkIO, id, wad, sprites))
                    return true;
                else if (LoadMoveables(chunkIO, id, wad, soundInfos, meshes))
                    return true;
                else if (LoadStatics(chunkIO, id, wad, meshes))
                    return true;
                return false;
            });

            if (obsolete)
                foreach (KeyValuePair<long, WadSoundInfo> soundInfo in soundInfos)
                    if (TrCatalog.IsSoundFixedByDefault(WadGameVersion.TR4_TRNG, checked((uint)soundInfo.Key)))
                    {
                        var Id = new WadFixedSoundInfoId(checked((uint)soundInfo.Key));
                        wad.FixedSoundInfos.Add(Id, new WadFixedSoundInfo(Id) { SoundInfo = soundInfo.Value });
                    }
            return wad;
        }

        private static bool LoadTextures(ChunkReader chunkIO, ChunkId idOuter, ref Dictionary<long, WadTexture> outTextures)
        {
            if (idOuter != Wad2Chunks.Textures)
                return false;

            Dictionary<long, WadTexture> textures = new Dictionary<long,  WadTexture>();
            long obsoleteIndex = 0; // Move this into each chunk once we got rid of old style *.wad2 files.

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Texture)
                    return false;

                var width = LEB128.ReadInt(chunkIO.Raw);
                var height = LEB128.ReadInt(chunkIO.Raw);
                byte[] textureData = null;
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.TextureIndex)
                        obsoleteIndex = chunkIO.ReadChunkLong(chunkSize2);
                    if (id2 == Wad2Chunks.TextureData)
                        textureData = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                    else
                        return false;
                    return true;
                });

                textures.Add(obsoleteIndex++, new WadTexture(ImageC.FromByteArray(textureData, width, height)));
                return true;
            });

            outTextures = textures;
            return true;
        }

        private static bool LoadSamples(ChunkReader chunkIO, ChunkId idOuter, ref Dictionary<long, WadSample> outSamples)
        {
            if (idOuter != Wad2Chunks.Samples)
                return false;

            var samples = new Dictionary<long, WadSample>();
            long obsoleteIndex = 0; // Move this into each chunk once we got rid of old style *.wad2 files.

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Sample)
                    return false;

                string FilenameObsolete = null;
                byte[] data = null;

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.SampleIndex)
                        obsoleteIndex = chunkIO.ReadChunkLong(chunkSize2);
                    else if (id2 == Wad2Chunks.SampleFilenameObsolete)
                        FilenameObsolete = chunkIO.ReadChunkString(chunkSize2);
                    else if (id2 == Wad2Chunks.SampleData)
                        data = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                    else
                        return false;
                    return true;
                });

                if (data == null && !string.IsNullOrEmpty(FilenameObsolete))
                {
                    string fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().FullName), "Sounds\\TR4\\Samples", FilenameObsolete + ".wav");
                    data = File.ReadAllBytes(fullPath);
                }

                samples.Add(obsoleteIndex++, new WadSample(WadSample.ConvertSampleFormat(data)));
                return true;
            });

            outSamples = samples;
            return true;
        }

        private static bool LoadSoundInfos(ChunkReader chunkIO, ChunkId idOuter, ref Dictionary<long, WadSoundInfo> outSoundInfos, Dictionary<long, WadSample> samples)
        {
            if (idOuter != Wad2Chunks.SoundInfosObsolete && idOuter != Wad2Chunks.SoundInfos)
                return false;

            var soundInfos = new Dictionary<long, WadSoundInfo>();
            if (idOuter == Wad2Chunks.SoundInfosObsolete)
                LEB128.ReadInt(chunkIO.Raw); // Read obsolete value
            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.SoundInfo)
                    return false;

                var soundInfo = new WadSoundInfoMetaData("Unnamed");
                long index = 0;
                if (idOuter == Wad2Chunks.SoundInfosObsolete)
                {
                    index = LEB128.ReadLong(chunkIO.Raw);
                    soundInfo.VolumeDiv255 = (byte)(LEB128.ReadByte(chunkIO.Raw) * 255 / 100);
                    soundInfo.RangeInSectors = LEB128.ReadByte(chunkIO.Raw);
                    soundInfo.PitchFactorDiv128 = (byte)(LEB128.ReadByte(chunkIO.Raw) * 255 / 100);
                    soundInfo.ChanceDiv255 = (byte)(LEB128.ReadByte(chunkIO.Raw) * 255 / 100);
                    soundInfo.FlagN = LEB128.ReadByte(chunkIO.Raw) == 1;
                    soundInfo.RandomizePitch = LEB128.ReadByte(chunkIO.Raw) == 1;
                    soundInfo.RandomizeGain = LEB128.ReadByte(chunkIO.Raw) == 1;
                    soundInfo.LoopBehaviour = (WadSoundLoopBehaviour)LEB128.ReadUShort(chunkIO.Raw);
                }

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.SoundInfoIndex)
                        index = chunkIO.ReadChunkLong(chunkSize2);
                    else if (id2 == Wad2Chunks.SoundInfoVolume)
                        soundInfo.VolumeDiv255 = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Wad2Chunks.SoundInfoRange)
                        soundInfo.RangeInSectors = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Wad2Chunks.SoundInfoPitch)
                        soundInfo.PitchFactorDiv128 = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Wad2Chunks.SoundInfoChance)
                        soundInfo.ChanceDiv255 = chunkIO.ReadChunkByte(chunkSize2);
                    else if (id2 == Wad2Chunks.SoundInfoFlagN)
                        soundInfo.FlagN = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Wad2Chunks.SoundInfoRandomizePitch)
                        soundInfo.RandomizePitch = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Wad2Chunks.SoundInfoRandomizeGain)
                        soundInfo.RandomizeGain = chunkIO.ReadChunkBool(chunkSize2);
                    else if (id2 == Wad2Chunks.SoundInfoLoopBehaviour)
                        soundInfo.LoopBehaviour = (WadSoundLoopBehaviour)(3 & chunkIO.ReadChunkByte(chunkSize2));
                    else if (id2 == Wad2Chunks.SoundInfoName || id2 == Wad2Chunks.SoundInfoNameObsolete)
                        soundInfo.Name = chunkIO.ReadChunkString(chunkSize2);
                    else if (id2 == Wad2Chunks.SoundInfoSampleIndex)
                        soundInfo.Samples.Add(samples[chunkIO.ReadChunkInt(chunkSize2)]);
                    else
                        return false;
                    return true;
                });

                if (string.IsNullOrWhiteSpace(soundInfo.Name))
                    soundInfo.Name = TrCatalog.GetOriginalSoundName(WadGameVersion.TR4_TRNG, unchecked((uint)index));

                soundInfos.Add(index, new WadSoundInfo(soundInfo));
                return true;
            });

            outSoundInfos = soundInfos;
            return true;
        }

        private static bool LoadFixedSoundInfos(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, Dictionary<long, WadSoundInfo> soundInfos)
        {
            if (idOuter != Wad2Chunks.FixedSoundInfos)
                return false;

            var fixedSoundInfos = new SortedList<WadFixedSoundInfoId, WadFixedSoundInfo>();
            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.FixedSoundInfo)
                    return false;
                int soundId = -1;
                int SoundInfoId = -1;
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.FixedSoundInfoId)
                        soundId = chunkIO.ReadChunkInt(chunkSize2);
                    else if (id2 == Wad2Chunks.FixedSoundInfoSoundInfoId)
                        SoundInfoId = chunkIO.ReadChunkInt(chunkSize2);
                    else
                        return false;
                    return true;
                });
                if (soundId == -1 || SoundInfoId == -1)
                    throw new Exception("Invalid fixed sound info.");

                var Id = new WadFixedSoundInfoId(checked((uint)soundId));
                fixedSoundInfos.Add(Id, new WadFixedSoundInfo(Id) { SoundInfo = soundInfos[SoundInfoId] });
                return true;
            });

            wad.FixedSoundInfos = fixedSoundInfos;
            return true;
        }

        private static bool LoadMeshes(ChunkReader chunkIO, ChunkId idOuter, ref Dictionary<long, WadMesh> outMeshes, Dictionary<long, WadTexture> textures)
        {
            if (idOuter != Wad2Chunks.Meshes)
                return false;

            var meshes = new Dictionary<long, WadMesh>();
            long obsoleteIndex = 0; // Move this into each chunk once we got rid of old style *.wad2 files.
            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Mesh)
                    return false;

                var mesh = new WadMesh();
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.MeshIndex)
                        obsoleteIndex = chunkIO.ReadChunkLong(chunkSize2);
                    else if (id2 == Wad2Chunks.MeshSphere)
                    {
                        // Read bounding sphere
                        float radius = 0;
                        Vector3 center = Vector3.Zero;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.MeshSphereCenter)
                                center = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.MeshSphereRadius)
                                radius = chunkIO.ReadChunkFloat(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        mesh.BoundingSphere = new BoundingSphere(center, radius);
                    }
                    else if (id2 == Wad2Chunks.MeshBoundingBox)
                    {
                        // Read bounding box
                        Vector3 min = Vector3.Zero;
                        Vector3 max = Vector3.Zero;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.MeshBoundingBoxMin)
                                min = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.MeshBoundingBoxMax)
                                max = chunkIO.ReadChunkVector3(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        mesh.BoundingBox = new BoundingBox(min, max);
                    }
                    else if (id2 == Wad2Chunks.MeshVertexPositions)
                    {
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.MeshVertexPosition)
                                mesh.VerticesPositions.Add(chunkIO.ReadChunkVector3(chunkSize3));
                            else
                                return false;
                            return true;
                        });
                    }
                    else if (id2 == Wad2Chunks.MeshVertexNormals)
                    {
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.MeshVertexNormal)
                                mesh.VerticesNormals.Add(chunkIO.ReadChunkVector3(chunkSize3));
                            else
                                return false;
                            return true;
                        });
                    }
                    else if (id2 == Wad2Chunks.MeshVertexShades)
                    {
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.MeshVertexShade)
                                mesh.VerticesShades.Add(chunkIO.ReadChunkShort(chunkSize3));
                            else
                                return false;
                            return true;
                        });
                    }
                    else if (id2 == Wad2Chunks.MeshPolygons)
                    {
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.MeshQuad ||
                                id3 == Wad2Chunks.MeshTriangle)
                            {
                                var polygon = new WadPolygon();
                                polygon.Shape = id3 == Wad2Chunks.MeshQuad ? WadPolygonShape.Quad : WadPolygonShape.Triangle;
                                polygon.Index0 = LEB128.ReadInt(chunkIO.Raw);
                                polygon.Index1 = LEB128.ReadInt(chunkIO.Raw);
                                polygon.Index2 = LEB128.ReadInt(chunkIO.Raw);
                                if (id3 == Wad2Chunks.MeshQuad)
                                    polygon.Index3 = LEB128.ReadInt(chunkIO.Raw);
                                polygon.ShineStrength = LEB128.ReadByte(chunkIO.Raw);

                                TextureArea textureArea = new TextureArea();
                                textureArea.Texture = textures[LEB128.ReadInt(chunkIO.Raw)];
                                textureArea.TexCoord0 = chunkIO.Raw.ReadVector2();
                                textureArea.TexCoord1 = chunkIO.Raw.ReadVector2();
                                textureArea.TexCoord2 = chunkIO.Raw.ReadVector2();
                                if (id3 == Wad2Chunks.MeshQuad)
                                    textureArea.TexCoord3 = chunkIO.Raw.ReadVector2();
                                textureArea.BlendMode = (BlendMode)LEB128.ReadLong(chunkIO.Raw);
                                textureArea.DoubleSided = chunkIO.Raw.ReadBoolean();
                                polygon.Texture = textureArea;

                                chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    return false;
                                });

                                mesh.Polys.Add(polygon);
                            }
                            else
                            {
                                return false;
                            }
                            return true;
                        });
                    }
                    else
                    {
                        return false;
                    }

                    return true;
                });

                mesh.UpdateHash();
                meshes.Add(obsoleteIndex, mesh);
                return true;
            });

            outMeshes = meshes;
            return true;
        }

        private static bool LoadSprites(ChunkReader chunkIO, ChunkId idOuter, ref Dictionary<long, WadSprite> outSprites)
        {
            if (idOuter != Wad2Chunks.Sprites)
                return false;

            var sprites = new Dictionary<long, WadSprite>();
            long obsoleteIndex = 0; // Move this into each chunk once we got rid of old style *.wad2 files.
            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Sprite)
                    return false;

                int width = LEB128.ReadInt(chunkIO.Raw);
                int height = LEB128.ReadInt(chunkIO.Raw);
                byte[] imageData = null;

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.SpriteIndex)
                        obsoleteIndex = chunkIO.ReadChunkLong(chunkSize2);
                    else if (id2 == Wad2Chunks.SpriteData)
                        imageData = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                    else
                        return false;
                    return true;
                });

                sprites.Add(obsoleteIndex++, new WadSprite { Texture = new WadTexture(ImageC.FromByteArray(imageData, width, height)) });
                return true;
            });

            outSprites = sprites;
            return true;
        }

        private static bool LoadSpriteSequences(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, Dictionary<long, WadSprite> sprites)
        {
            if (idOuter != Wad2Chunks.SpriteSequences)
                return false;

            var spriteSequences = new SortedList<WadSpriteSequenceId, WadSpriteSequence>();
            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.SpriteSequence)
                    return false;

                var s = new WadSpriteSequence(new WadSpriteSequenceId(LEB128.ReadUInt(chunkIO.Raw)));
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.SpriteSequenceSpriteIndex)
                        s.Sprites.Add(sprites[chunkIO.ReadChunkInt(chunkSize2)]);
                    else
                        return false;
                    return true;
                });

                spriteSequences.Add(s.Id, s);
                return true;
            });

            wad.SpriteSequences = spriteSequences;
            return true;
        }

        private static WadBone LoadBone(ChunkReader chunkIO, WadMoveable mov)
        {
            WadBone bone = new WadBone();

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id == Wad2Chunks.MoveableBoneName)
                    bone.Name = chunkIO.ReadChunkString(chunkSize);
                else if (id == Wad2Chunks.MoveableBoneTranslation)
                {
                    bone.Translation = chunkIO.ReadChunkVector3(chunkSize);
                    bone.Transform = Matrix4x4.CreateTranslation(bone.Translation);
                }
                else if (id == Wad2Chunks.MoveableBoneMeshPointer)
                    bone.Mesh = mov.Meshes[chunkIO.ReadChunkInt(chunkSize)];
                else if (id == Wad2Chunks.MoveableBone)
                    bone.Children.Add(LoadBone(chunkIO, mov));
                else
                    return false;
                return true;
            });

            foreach (var childBone in bone.Children)
                childBone.Parent = bone;

            return bone;
        }

        private static bool LoadMoveables(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, Dictionary<long, WadSoundInfo> soundInfos, Dictionary<long, WadMesh> meshes)
        {
            if (idOuter != Wad2Chunks.Moveables)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Moveable)
                    return false;

                uint objTypeId = LEB128.ReadUInt(chunkIO.Raw);
                var mov = new WadMoveable(new WadMoveableId(objTypeId));
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.MoveableMesh)
                        mov.Meshes.Add(meshes[chunkIO.ReadChunkInt(chunkSize2)]);
                    else if (id2 == Wad2Chunks.MoveableBone)
                        mov.Skeleton = LoadBone(chunkIO, mov);
                    else if (id2 == Wad2Chunks.AnimationObsolete || id2 == Wad2Chunks.Animation)
                    {
                        var animation = new WadAnimation();

                        animation.StateId = LEB128.ReadUShort(chunkIO.Raw);
                        animation.RealNumberOfFrames = LEB128.ReadUShort(chunkIO.Raw);
                        animation.FrameDuration = LEB128.ReadByte(chunkIO.Raw);
                        if (id2 == Wad2Chunks.AnimationObsolete)
                        {
                            LEB128.ReadUShort(chunkIO.Raw);
                            LEB128.ReadUShort(chunkIO.Raw);
                        }
                        animation.Speed = LEB128.ReadInt(chunkIO.Raw);
                        animation.Acceleration = LEB128.ReadInt(chunkIO.Raw);
                        animation.LateralSpeed = LEB128.ReadInt(chunkIO.Raw);
                        animation.LateralAcceleration = LEB128.ReadInt(chunkIO.Raw);
                        animation.NextAnimation = LEB128.ReadUShort(chunkIO.Raw);
                        animation.NextFrame = LEB128.ReadUShort(chunkIO.Raw);

                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.AnimationName)
                            {
                                animation.Name = chunkIO.ReadChunkString(chunkSize3);
                            }
                            else if (id3 == Wad2Chunks.KeyFrame)
                            {
                                var keyframe = new WadKeyFrame();
                                chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 == Wad2Chunks.KeyFrameOffset)
                                    {
                                        keyframe.Offset = chunkIO.ReadChunkVector3(chunkSize4);
                                    }
                                    else if (id4 == Wad2Chunks.KeyFrameBoundingBox)
                                    {
                                        var kfMin = Vector3.Zero;
                                        var kfMax = Vector3.Zero;
                                        chunkIO.ReadChunks((id5, chunkSize5) =>
                                        {
                                            if (id5 == Wad2Chunks.MeshBoundingBoxMin)
                                                kfMin = chunkIO.ReadChunkVector3(chunkSize5);
                                            else if (id5 == Wad2Chunks.MeshBoundingBoxMax)
                                                kfMax = chunkIO.ReadChunkVector3(chunkSize5);
                                            else
                                                return false;
                                            return true;
                                        });
                                        keyframe.BoundingBox = new BoundingBox(kfMin, kfMax);
                                    }
                                    else if (id4 == Wad2Chunks.KeyFrameAngle)
                                    {
                                        var angle = new WadKeyFrameRotation();
                                        angle.Axis = (WadKeyFrameRotationAxis)LEB128.ReadUShort(chunkIO.Raw);
                                        angle.X = LEB128.ReadInt(chunkIO.Raw);
                                        angle.Y = LEB128.ReadInt(chunkIO.Raw);
                                        angle.Z = LEB128.ReadInt(chunkIO.Raw);
                                        keyframe.Angles.Add(angle);
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    return true;
                                });
                                animation.KeyFrames.Add(keyframe);
                            }
                            else if (id3 == Wad2Chunks.StateChange)
                            {
                                var stateChange = new WadStateChange();
                                stateChange.StateId = LEB128.ReadUShort(chunkIO.Raw);
                                chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 == Wad2Chunks.Dispatch)
                                    {
                                        var dispatch = new WadAnimDispatch();
                                        dispatch.InFrame = LEB128.ReadUShort(chunkIO.Raw);
                                        dispatch.OutFrame = LEB128.ReadUShort(chunkIO.Raw);
                                        dispatch.NextAnimation = LEB128.ReadUShort(chunkIO.Raw);
                                        dispatch.NextFrame = LEB128.ReadUShort(chunkIO.Raw);
                                        stateChange.Dispatches.Add(dispatch);
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    return true;
                                });
                                animation.StateChanges.Add(stateChange);
                            }
                            else if (id3 == Wad2Chunks.AnimCommand)
                            {
                                var command = new WadAnimCommand();
                                long offset = chunkIO.Raw.BaseStream.Position;
                                command.Type = (WadAnimCommandType)LEB128.ReadUShort(chunkIO.Raw);
                                command.Parameter1 = LEB128.ReadUShort(chunkIO.Raw);
                                command.Parameter2 = LEB128.ReadUShort(chunkIO.Raw);
                                command.Parameter3 = LEB128.ReadUShort(chunkIO.Raw);
                                long readCount = offset - chunkIO.Raw.BaseStream.Position;

                                if (readCount < chunkSize3)
                                {
                                    command.SoundInfo = soundInfos[LEB128.ReadInt(chunkIO.Raw)];
                                }
                                else
                                { // Code to load obsolete *.wad2 files...
                                    command.SoundInfo = soundInfos[command.Parameter2 & 0x3FFF];
                                    command.Parameter2 &= 0xC000; // Clear sound ID
                                }

                                animation.AnimCommands.Add(command);
                            }
                            else
                            {
                                return false;
                            }
                            return true;
                        });
                        mov.Animations.Add(animation);
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                });

                mov.LinearizeSkeleton();
                wad.Moveables.Add(mov.Id, mov);
                return true;
            });

            return true;
        }

        private static bool LoadStatics(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, Dictionary<long, WadMesh> meshes)
        {
            if (idOuter != Wad2Chunks.Statics)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Static)
                    return false;

                var s = new WadStatic(new WadStaticId(LEB128.ReadUInt(chunkIO.Raw)));
                s.Mesh = meshes[LEB128.ReadInt(chunkIO.Raw)];
                s.Flags = LEB128.ReadShort(chunkIO.Raw);

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.StaticVisibilityBox)
                    {
                        var min = Vector3.Zero;
                        var max = Vector3.Zero;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.MeshBoundingBoxMin)
                                min = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.MeshBoundingBoxMax)
                                max = chunkIO.ReadChunkVector3(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        s.VisibilityBox = new BoundingBox(min, max);
                    }
                    else if (id2 == Wad2Chunks.StaticCollisionBox)
                    {
                        var min = Vector3.Zero;
                        var max = Vector3.Zero;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.MeshBoundingBoxMin)
                                min = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.MeshBoundingBoxMax)
                                max = chunkIO.ReadChunkVector3(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        s.CollisionBox = new BoundingBox(min, max);
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                });

                wad.Statics.Add(s.Id, s);
                return true;
            });

            return true;
        }
    }
}
