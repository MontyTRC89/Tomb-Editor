using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.Wad
{
    partial class Wad2
    {
        public static Wad2 LoadFromFile(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                return LoadFromStream(fileStream);
        }

        public static Wad2 LoadFromStream(Stream stream)
        {
            using (var chunkIO = new ChunkReader(Wad2Chunks.MagicNumber, stream))
                return LoadWad2(chunkIO);
        }

        private static Wad2 LoadWad2(ChunkReader chunkIO)
        {
            var wad = new Wad2();

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (LoadTextures(chunkIO, id, wad))
                    return true;
                else if (LoadSprites(chunkIO, id, wad))
                    return true;
                else if (LoadMeshes(chunkIO, id, wad))
                    return true;
                else if (LoadSamples(chunkIO, id, wad))
                    return true;
                else if (LoadMoveables(chunkIO, id, wad))
                    return true;
                else if (LoadStatics(chunkIO, id, wad))
                    return true;
                else if (LoadSpriteSequences(chunkIO, id, wad))
                    return true;
                else if (LoadSounds(chunkIO, id, wad))
                    return true;

                return false;
            });

            return wad;
        }

        private static bool LoadTextures(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            if (idOuter != Wad2Chunks.Textures)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Texture)
                    return false;

                var texture = new WadTexture();
                var width = LEB128.ReadInt(chunkIO.Raw);
                var height = LEB128.ReadInt(chunkIO.Raw);
                byte[] textureData = new byte[1];

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.TextureData)
                        textureData = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                    else
                        return false;
                    return true;
                });

                texture.Image = ImageC.FromByteArray(textureData, width, height);
                texture.UpdateHash();

                wad.Textures.Add(texture.Hash, texture);

                return true;
            });

            return true;
        }

        private static bool LoadSprites(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            if (idOuter != Wad2Chunks.Sprites)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Sprite)
                    return false;

                var texture = new WadSprite();
                var width = LEB128.ReadInt(chunkIO.Raw);
                var height = LEB128.ReadInt(chunkIO.Raw);
                byte[] textureData = new byte[1];

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.TextureData)
                        textureData = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                    else
                        return false;
                    return true;
                });

                texture.Image = ImageC.FromByteArray(textureData, width, height);
                texture.UpdateHash();

                wad.SpriteTextures.Add(texture.Hash, texture);

                return true;
            });

            return true;
        }

        private static bool LoadSamples(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            if (idOuter != Wad2Chunks.Waves)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Wave)
                    return false;

                string name = "";
                byte[] data = new byte[1];

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.WaveName)
                        name = chunkIO.ReadChunkString(chunkSize2);
                    else if (id2 == Wad2Chunks.WaveData)
                        data = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                    else
                        return false;
                    return true;
                });

                var sample = new WadSound(name, data);
                sample.UpdateHash();

                wad.WaveSounds.Add(sample.Hash, sample);

                return true;
            });

            return true;
        }

        private static bool LoadMeshes(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            if (idOuter != Wad2Chunks.Meshes)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Mesh)
                    return false;

                var mesh = new WadMesh();

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.Sphere)
                    {
                        // Read bounding sphere
                        float radius = 0;
                        Vector3 center = Vector3.Zero;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.SphereCentre)
                                center = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.SphereRadius)
                                radius = chunkIO.ReadChunkFloat(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        mesh.BoundingSphere = new BoundingSphere(center, radius);
                    }
                    else if (id2 == Wad2Chunks.BoundingBox)
                    {
                        // Read bounding box
                        Vector3 min = Vector3.Zero;
                        Vector3 max = Vector3.Zero;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.BoundingBoxMin)
                                min = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.BoundingBoxMax)
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
                            if ((id3 == Wad2Chunks.MeshQuad) ||
                                (id3 == Wad2Chunks.MeshTriangle))
                            {
                                var polygon = new WadPolygon(id3 == Wad2Chunks.MeshQuad ? WadPolygonShape.Quad : WadPolygonShape.Triangle);
                                polygon.Indices.Add(LEB128.ReadInt(chunkIO.Raw));
                                polygon.Indices.Add(LEB128.ReadInt(chunkIO.Raw));
                                polygon.Indices.Add(LEB128.ReadInt(chunkIO.Raw));
                                if (id3 == Wad2Chunks.MeshQuad)
                                    polygon.Indices.Add(LEB128.ReadInt(chunkIO.Raw));
                                polygon.ShineStrength = LEB128.ReadByte(chunkIO.Raw);

                                TextureArea textureArea = new TextureArea();
                                textureArea.Texture = wad.Textures.ElementAt(LEB128.ReadInt(chunkIO.Raw)).Value;
                                textureArea.TexCoord0 = chunkIO.Raw.ReadVector2();
                                textureArea.TexCoord1 = chunkIO.Raw.ReadVector2();
                                textureArea.TexCoord2 = chunkIO.Raw.ReadVector2();
                                if (id3 == Wad2Chunks.MeshQuad)
                                    textureArea.TexCoord3 = chunkIO.Raw.ReadVector2();
                                textureArea.BlendMode = (BlendMode)LEB128.ReadLong(chunkIO.Raw);
                                textureArea.DoubleSided = chunkIO.Raw.ReadBoolean();
                                polygon.Texture = textureArea;

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
                wad.Meshes.Add(mesh.Hash, mesh);

                return true;
            });

            return true;
        }

        private static bool LoadMoveables(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            if (idOuter != Wad2Chunks.Moveables)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Moveable)
                    return false;

                var mov = new WadMoveable();

                mov.ObjectID = LEB128.ReadUInt(chunkIO.Raw);
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.MoveableOffset)
                    {
                        mov.Offset = chunkIO.ReadChunkVector3(chunkSize2);
                    }
                    else if (id2 == Wad2Chunks.MoveableMesh)
                    {
                        mov.Meshes.Add(wad.Meshes.ElementAt(chunkIO.ReadChunkInt(chunkSize2)).Value);
                    }
                    else if (id2 == Wad2Chunks.MoveableLink)
                    {
                        var opcode = (WadLinkOpcode)LEB128.ReadUShort(chunkIO.Raw);
                        Vector3 offset = Vector3.Zero;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.MoveableLinkOffset)
                                offset = chunkIO.ReadChunkVector3(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        mov.Links.Add(new WadLink(opcode, offset));
                    }
                    else if (id2 == Wad2Chunks.Animation)
                    {
                        var animation = new WadAnimation();

                        animation.StateId = LEB128.ReadUShort(chunkIO.Raw);
                        animation.RealNumberOfFrames = LEB128.ReadUShort(chunkIO.Raw);
                        animation.FrameDuration = LEB128.ReadByte(chunkIO.Raw);
                        animation.FrameStart = LEB128.ReadUShort(chunkIO.Raw);
                        animation.FrameEnd = LEB128.ReadUShort(chunkIO.Raw);
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
                                            if (id5 == Wad2Chunks.BoundingBoxMin)
                                                kfMin = chunkIO.ReadChunkVector3(chunkSize5);
                                            else if (id5 == Wad2Chunks.BoundingBoxMax)
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
                                var command = new WadAnimCommand((WadAnimCommandType)LEB128.ReadUShort(chunkIO.Raw));
                                command.Parameter1 = LEB128.ReadUShort(chunkIO.Raw);
                                command.Parameter2 = LEB128.ReadUShort(chunkIO.Raw);
                                command.Parameter3 = LEB128.ReadUShort(chunkIO.Raw);
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
                wad.Moveables.Add(mov.ObjectID, mov);
                return true;
            });

            return true;
        }

        private static bool LoadStatics(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            if (idOuter != Wad2Chunks.Statics)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Static)
                    return false;

                var s = new WadStatic();
                s.ObjectID = LEB128.ReadUInt(chunkIO.Raw);
                s.Mesh = wad.Meshes.ElementAt(LEB128.ReadInt(chunkIO.Raw)).Value;
                s.Flags = LEB128.ReadShort(chunkIO.Raw);

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.StaticVisibilityBox)
                    {
                        var min = Vector3.Zero;
                        var max = Vector3.Zero;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.BoundingBoxMin)
                                min = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.BoundingBoxMax)
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
                            if (id3 == Wad2Chunks.BoundingBoxMin)
                                min = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.BoundingBoxMax)
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

                wad.Statics.Add(s.ObjectID, s);
                return true;
            });

            return true;
        }

        private static bool LoadSpriteSequences(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            if (idOuter != Wad2Chunks.SpriteSequences)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.SpriteSequence)
                    return false;

                var s = new WadSpriteSequence();
                s.ObjectID = LEB128.ReadUInt(chunkIO.Raw);
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.SpriteSequenceSprite)
                    {
                        s.Sprites.Add(wad.SpriteTextures.ElementAt(chunkIO.ReadChunkInt(chunkSize2)).Value);
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                });

                wad.SpriteSequences.Add(s);
                return true;
            });

            return true;
        }

        private static bool LoadSounds(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            if (idOuter != Wad2Chunks.Sounds)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Sound)
                    return false;

                var s = new WadSoundInfo();
                var soundId = LEB128.ReadUShort(chunkIO.Raw);
                s.Volume = LEB128.ReadByte(chunkIO.Raw);
                s.Range = LEB128.ReadByte(chunkIO.Raw);
                s.Pitch = LEB128.ReadByte(chunkIO.Raw);
                s.Chance = LEB128.ReadByte(chunkIO.Raw);
                s.FlagN = (LEB128.ReadByte(chunkIO.Raw) == 1);
                s.RandomizePitch = (LEB128.ReadByte(chunkIO.Raw) == 1);
                s.RandomizeGain = (LEB128.ReadByte(chunkIO.Raw) == 1);
                s.Loop = (WadSoundLoopType)LEB128.ReadUShort(chunkIO.Raw);

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.SoundName)
                    {
                        s.Name = chunkIO.ReadChunkString(chunkSize2);
                    }
                    else if (id2 == Wad2Chunks.SoundSample)
                    {
                        s.WaveSounds.Add(wad.WaveSounds.ElementAt(chunkIO.ReadChunkInt(chunkSize2)).Value);
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                });

                wad.SoundInfo.Add(soundId, s);
                return true;
            });

            return true;
        }
    }
}
