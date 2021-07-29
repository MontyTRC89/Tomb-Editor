using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TombLib.IO;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public static class Wad2Writer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static IReadOnlyCollection<FileFormat> FileFormats = new[] { new FileFormat("Wad2 file", "wad2") };

        public static void SaveToFile(Wad2 wad, string filename)
        {
            // We save first to a temporary memory stream
            using (var stream = new MemoryStream())
            {
                SaveToStream(wad, stream);

                // Save to temporary file as well, so original wad2 won't vanish in case of crash
                var tempName = filename + ".tmp";
                if (File.Exists(tempName)) File.Delete(tempName);

                stream.Seek(0, SeekOrigin.Begin);
                using (var writer = new BinaryWriter(new FileStream(tempName, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    var buffer = stream.ToArray();
                    writer.Write(buffer, 0, buffer.Length);
                }

                // Save successful, write temp file over original (if exists)
                if (File.Exists(filename)) File.Delete(filename);
                File.Move(tempName, filename);
            }

            // Save sounds to XML file
            if (wad.Sounds.SoundInfos.Count > 0)
            {
                string xmlFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".xml");
                WadSounds.SaveToXml(xmlFilename, wad.Sounds);
            }
        }

        public static void SaveToStream(Wad2 wad, Stream stream)
        {
            using (var chunkIO = new ChunkWriter(Wad2Chunks.MagicNumber, stream, ChunkWriter.Compression.None))
                WriteWad2(chunkIO, wad);
        }

        public static void SaveToBinaryWriterFast(Wad2 wad, BinaryWriterFast fastWriter)
        {
            using (var chunkIO = new ChunkWriter(Wad2Chunks.MagicNumber, fastWriter))
                WriteWad2(chunkIO, wad);
        }

        private static void WriteWad2(ChunkWriter chunkIO, Wad2 wad)
        {
            chunkIO.WriteChunkInt(Wad2Chunks.GameVersion, (long)wad.GameVersion);
            chunkIO.WriteChunkInt(Wad2Chunks.SoundSystem, (long)SoundSystem.Xml);

            var meshTable = new List<WadMesh>(wad.MeshesUnique);
            var spriteTable = new List<WadSprite>(wad.SpriteSequences.Values.SelectMany(spriteSequence => spriteSequence.Sprites));
            var textureTable = new List<WadTexture>(wad.MeshTexturesUnique);

            WriteTextures(chunkIO, textureTable);
            WriteSprites(chunkIO, spriteTable);
            WriteSpriteSequences(chunkIO, wad, spriteTable);
            WriteMoveables(chunkIO, wad, textureTable);
            WriteStatics(chunkIO, wad, textureTable);
            chunkIO.WriteChunkEnd();
        }

        private static void WriteTextures(ChunkWriter chunkIO, List<WadTexture> textureTable)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.Textures, () =>
            {
                foreach (var texture in textureTable)
                {
                    chunkIO.WriteChunkWithChildren(Wad2Chunks.Texture, () =>
                    {
                        LEB128.Write(chunkIO.Raw, texture.Image.Width);
                        LEB128.Write(chunkIO.Raw, texture.Image.Height);
                        chunkIO.WriteChunkArrayOfBytes(Wad2Chunks.TextureData, texture.Image.ToByteArray());
                    });
                }
            });
        }

        private static void WriteSprites(ChunkWriter chunkIO, List<WadSprite> spriteTable)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.Sprites, () =>
            {
                for (int i = 0; i < spriteTable.Count; ++i)
                {
                    var sprite = spriteTable[i];
                    chunkIO.WriteChunkWithChildren(Wad2Chunks.Sprite, () =>
                    {
                        LEB128.Write(chunkIO.Raw, sprite.Texture.Image.Width);
                        LEB128.Write(chunkIO.Raw, sprite.Texture.Image.Height);
                        chunkIO.WriteChunkInt(Wad2Chunks.SpriteIndex, i);
                        chunkIO.WriteChunkArrayOfBytes(Wad2Chunks.SpriteData, sprite.Texture.Image.ToByteArray());
                        chunkIO.WriteChunk(Wad2Chunks.SpriteSides, () => {
                            chunkIO.Raw.Write(sprite.Alignment.X0);
                            chunkIO.Raw.Write(sprite.Alignment.Y0);
                            chunkIO.Raw.Write(sprite.Alignment.X1);
                            chunkIO.Raw.Write(sprite.Alignment.Y1);
                        });
                    });
                }
            });
        }

        private static void WriteSpriteSequences(ChunkWriter chunkIO, Wad2 wad, List<WadSprite> spriteTable)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.SpriteSequences, () =>
            {
                foreach (var sequence in wad.SpriteSequences.Values)
                {
                    chunkIO.WriteChunkWithChildren(Wad2Chunks.SpriteSequence, () =>
                    {
                        LEB128.Write(chunkIO.Raw, sequence.Id.TypeId);
                        foreach (var spr in sequence.Sprites)
                            chunkIO.WriteChunkInt(Wad2Chunks.SpriteSequenceSpriteIndex, spriteTable.IndexOf(spr));
                    });
                }
            });
        }

        public static void WriteMesh(ChunkWriter chunkIO, WadMesh mesh, List<WadTexture> textureTable)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.Mesh, () =>
            {
                chunkIO.WriteChunkInt(Wad2Chunks.MeshIndex, 0);
                chunkIO.WriteChunkString(Wad2Chunks.MeshName, mesh.Name);

                // Write bounding sphere
                chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshSphere, () =>
                {
                    chunkIO.WriteChunkVector3(Wad2Chunks.MeshSphereCenter, mesh.BoundingSphere.Center);
                    chunkIO.WriteChunkFloat(Wad2Chunks.MeshSphereRadius, mesh.BoundingSphere.Radius);
                });

                // Write bounding box
                chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshBoundingBox, () =>
                {
                    chunkIO.WriteChunkVector3(Wad2Chunks.MeshBoundingBoxMin, mesh.BoundingBox.Minimum);
                    chunkIO.WriteChunkVector3(Wad2Chunks.MeshBoundingBoxMax, mesh.BoundingBox.Maximum);
                });

                // Write positions
                chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshVertexPositions, () =>
                {
                    foreach (var pos in mesh.VertexPositions)
                    {
                        chunkIO.WriteChunkVector3(Wad2Chunks.MeshVertexPosition, pos);
                    }
                });

                // Write normals
                chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshVertexNormals, () =>
                {
                    foreach (var normal in mesh.VertexNormals)
                    {
                        chunkIO.WriteChunkVector3(Wad2Chunks.MeshVertexNormal, normal);
                    }
                });

                // Write shades
                chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshVertexShades, () =>
                {
                    foreach (var color in mesh.VertexColors)
                    {
                        chunkIO.WriteChunkVector3(Wad2Chunks.MeshVertexColor, color);
                    }
                });

                // Write vertex attributes
                chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshVertexAttributes, () =>
                {
                    foreach (var attr in mesh.VertexAttributes)
                        chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshVertexAttribute, () =>
                        {
                            LEB128.Write(chunkIO.Raw, attr.Glow);
                            LEB128.Write(chunkIO.Raw, attr.Move);
                        });
                });

                // Write light mode
                chunkIO.WriteChunkInt(Wad2Chunks.MeshLightingType, (int)mesh.LightingType);

                // Write polygons
                chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshPolygons, () =>
                {
                    foreach (var poly in mesh.Polys)
                    {
                        bool isQuad = poly.Shape == WadPolygonShape.Quad;

                        chunkIO.WriteChunkWithChildren(isQuad ? Wad2Chunks.MeshQuad : Wad2Chunks.MeshTriangle, () =>
                        {
                            LEB128.Write(chunkIO.Raw, poly.Index0);
                            LEB128.Write(chunkIO.Raw, poly.Index1);
                            LEB128.Write(chunkIO.Raw, poly.Index2);
                            if (isQuad)
                                LEB128.Write(chunkIO.Raw, poly.Index3);
                            LEB128.Write(chunkIO.Raw, poly.ShineStrength);

                            LEB128.Write(chunkIO.Raw, textureTable.IndexOf(poly.Texture.Texture as WadTexture));
                            chunkIO.Raw.Write(poly.Texture.TexCoord0);
                            chunkIO.Raw.Write(poly.Texture.TexCoord1);
                            chunkIO.Raw.Write(poly.Texture.TexCoord2);
                            if (isQuad)
                                chunkIO.Raw.Write(poly.Texture.TexCoord3);
                            LEB128.Write(chunkIO.Raw, (long)poly.Texture.BlendMode);
                            chunkIO.Raw.Write(poly.Texture.DoubleSided);
                        });
                    }
                });
            });
        }

        private static void WriteBone(ChunkWriter chunkIO, WadBone bone, List<WadBone> bones)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.MoveableBone, () =>
            {
                chunkIO.WriteChunkString(Wad2Chunks.MoveableBoneName, bone.Name);
                chunkIO.WriteChunkInt(Wad2Chunks.MoveableBoneMeshPointer, bones.IndexOf(bone));
                chunkIO.WriteChunkVector3(Wad2Chunks.MoveableBoneTranslation, bone.Translation);
                foreach (var childBone in bone.Children)
                    WriteBone(chunkIO, childBone, bones);
            });
        }

        private static void WriteMoveables(ChunkWriter chunkIO, Wad2 wad, List<WadTexture> textureTable)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.Moveables, () =>
            {
                foreach (var moveable in wad.Moveables)
                {
                    chunkIO.WriteChunkWithChildren(Wad2Chunks.Moveable, () =>
                    {
                        var m = moveable.Value;

                        LEB128.Write(chunkIO.Raw, m.Id.TypeId);

                        foreach (var mesh in m.Meshes)
                            WriteMesh(chunkIO, mesh, textureTable);

                        foreach (var b in m.Bones)
                        {
                            chunkIO.WriteChunkWithChildren(Wad2Chunks.MoveableBoneNew, () =>
                            {
                                LEB128.Write(chunkIO.Raw, (byte)b.OpCode);
                                chunkIO.Raw.WriteStringUTF8(b.Name);
                                chunkIO.WriteChunkVector3(Wad2Chunks.MoveableBoneTranslation, b.Translation);
                                chunkIO.WriteChunkInt(Wad2Chunks.MoveableBoneMeshPointer, m.Bones.IndexOf(b));
                            });
                        }

                        foreach (var animation in m.Animations)
                        {
                            chunkIO.WriteChunkWithChildren(Wad2Chunks.Animation2, () =>
                            {
                                LEB128.Write(chunkIO.Raw, animation.StateId);
                                LEB128.Write(chunkIO.Raw, animation.EndFrame);
                                LEB128.Write(chunkIO.Raw, animation.FrameRate);

                                LEB128.Write(chunkIO.Raw, animation.NextAnimation);
                                LEB128.Write(chunkIO.Raw, animation.NextFrame);

                                chunkIO.WriteChunkString(Wad2Chunks.AnimationName, animation.Name);

                                foreach (var kf in animation.KeyFrames)
                                {
                                    chunkIO.WriteChunkWithChildren(Wad2Chunks.KeyFrame, () =>
                                    {
                                        chunkIO.WriteChunkVector3(Wad2Chunks.KeyFrameOffset, kf.Offset);
                                        chunkIO.WriteChunkWithChildren(Wad2Chunks.KeyFrameBoundingBox, () =>
                                        {
                                            chunkIO.WriteChunkVector3(Wad2Chunks.MeshBoundingBoxMin, kf.BoundingBox.Minimum);
                                            chunkIO.WriteChunkVector3(Wad2Chunks.MeshBoundingBoxMax, kf.BoundingBox.Maximum);
                                        });
                                        foreach (var angle in kf.Angles)
                                        {
                                            chunkIO.WriteChunkVector3(Wad2Chunks.KeyFrameAngle, angle.Rotations);
                                        }
                                    });
                                }

                                foreach (var stateChange in animation.StateChanges)
                                {
                                    chunkIO.WriteChunkWithChildren(Wad2Chunks.StateChange, () =>
                                    {
                                        LEB128.Write(chunkIO.Raw, stateChange.StateId);
                                        foreach (var dispatch in stateChange.Dispatches)
                                        {
                                            chunkIO.WriteChunk(Wad2Chunks.Dispatch, () =>
                                            {
                                                LEB128.Write(chunkIO.Raw, dispatch.InFrame);
                                                LEB128.Write(chunkIO.Raw, dispatch.OutFrame);
                                                LEB128.Write(chunkIO.Raw, dispatch.NextAnimation);
                                                LEB128.Write(chunkIO.Raw, dispatch.NextFrame);
                                            });
                                        }
                                    });
                                }

                                foreach (var command in animation.AnimCommands)
                                {
                                    chunkIO.WriteChunkWithChildren(Wad2Chunks.AnimCommand, () =>
                                    {
                                        LEB128.Write(chunkIO.Raw, (ushort)command.Type);
                                        LEB128.Write(chunkIO.Raw, command.Parameter1);
                                        LEB128.Write(chunkIO.Raw, command.Parameter2);
                                        LEB128.Write(chunkIO.Raw, command.Parameter3);
                                        chunkIO.WriteChunkInt(Wad2Chunks.AnimCommandSoundInfo, -1);
                                    });
                                }

                                // New chunk for velocities
                                chunkIO.WriteChunkVector4(Wad2Chunks.AnimationVelocities,
                                                          new System.Numerics.Vector4(animation.StartVelocity,
                                                                                      animation.EndVelocity,
                                                                                      animation.StartLateralVelocity,
                                                                                      animation.EndLateralVelocity));
                            });
                        }
                    });
                }
            });
        }

        private static void WriteStatics(ChunkWriter chunkIO, Wad2 wad, List<WadTexture> textureTable)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.Statics, () =>
            {
                foreach (var staticMesh in wad.Statics)
                {
                    chunkIO.WriteChunkWithChildren(Wad2Chunks.Static2, () =>
                    {
                        var s = staticMesh.Value;

                        LEB128.Write(chunkIO.Raw, s.Id.TypeId);
                        LEB128.Write(chunkIO.Raw, s.Flags);

                        WriteMesh(chunkIO, s.Mesh, textureTable);

                        chunkIO.WriteChunkInt(Wad2Chunks.StaticAmbientLight, s.AmbientLight);

                        foreach (var light in s.Lights)
                        {
                            chunkIO.WriteChunkWithChildren(Wad2Chunks.StaticLight, () =>
                            {
                                chunkIO.WriteChunkVector3(Wad2Chunks.StaticLightPosition, light.Position);
                                chunkIO.WriteChunkFloat(Wad2Chunks.StaticLightRadius, light.Radius);
                                chunkIO.WriteChunkFloat(Wad2Chunks.StaticLightIntensity, light.Intensity);
                            });
                        }

                        chunkIO.WriteChunkWithChildren(Wad2Chunks.StaticVisibilityBox, () =>
                        {
                            chunkIO.WriteChunkVector3(Wad2Chunks.MeshBoundingBoxMin, s.VisibilityBox.Minimum);
                            chunkIO.WriteChunkVector3(Wad2Chunks.MeshBoundingBoxMax, s.VisibilityBox.Maximum);
                        });

                        chunkIO.WriteChunkWithChildren(Wad2Chunks.StaticCollisionBox, () =>
                        {
                            chunkIO.WriteChunkVector3(Wad2Chunks.MeshBoundingBoxMin, s.CollisionBox.Minimum);
                            chunkIO.WriteChunkVector3(Wad2Chunks.MeshBoundingBoxMax, s.CollisionBox.Maximum);
                        });
                    });
                }
            });
        }
    }
}
