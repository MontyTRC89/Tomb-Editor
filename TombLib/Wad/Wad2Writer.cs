using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.IO;

namespace TombLib.Wad
{
    partial class Wad2
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static List<WadSound> _wavesTable;
        private static List<WadMesh> _meshesTable;
        private static List<WadTexture> _texturesTable;
        private static List<WadSprite> _spritesTable;

        public static void SaveToWad2(string filename, Wad2 wad)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var chunkIO = new ChunkWriter(Wad2Chunks.MagicNumber, fileStream, ChunkWriter.Compression.None))
                WriteWad2(chunkIO, wad);
        }

        private static void WriteWad2(ChunkWriter chunkIO, Wad2 wad)
        {
            _wavesTable = new List<WadSound>();
            foreach (var wave in wad.WaveSounds)
                _wavesTable.Add(wave.Value);

            _meshesTable = new List<WadMesh>();
            foreach (var mesh in wad.Meshes)
                _meshesTable.Add(mesh.Value);

            _spritesTable = new List<WadSprite>();
            foreach (var sprite in wad.SpriteTextures)
                _spritesTable.Add(sprite.Value);

            _texturesTable = new List<WadTexture>();
            foreach (var texture in wad.Textures)
                _texturesTable.Add(texture.Value);

            WriteTextures(chunkIO, wad);
            WriteSprites(chunkIO, wad);
            WriteMeshes(chunkIO, wad);
            WriteSamples(chunkIO, wad);
            WriteMoveables(chunkIO, wad);
            WriteStatics(chunkIO, wad);
            WriteSpriteSequences(chunkIO, wad);
            WriteSounds(chunkIO, wad);

            chunkIO.WriteChunkEnd();
        }

        private static void WriteTextures(ChunkWriter chunkIO, Wad2 wad)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.Textures, () =>
            {
                foreach (var texture in wad.Textures)
                {
                    chunkIO.WriteChunkWithChildren(Wad2Chunks.Texture, () =>
                    {
                        var txt = texture.Value;

                        LEB128.Write(chunkIO.Raw, txt.Width);
                        LEB128.Write(chunkIO.Raw, txt.Height);
                        chunkIO.WriteChunkArrayOfBytes(Wad2Chunks.TextureData, txt.Image.ToByteArray());
                    });
                }
            });
        }

        private static void WriteMeshes(ChunkWriter chunkIO, Wad2 wad)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.Meshes, () =>
            {
                foreach (var mesh in wad.Meshes)
                {
                    chunkIO.WriteChunkWithChildren(Wad2Chunks.Mesh, () =>
                    {
                        var msh = mesh.Value;

                        // Write bounding sphere
                        chunkIO.WriteChunk(Wad2Chunks.Sphere, () =>
                        {
                            chunkIO.WriteChunkVector3(Wad2Chunks.SphereCentre, msh.BoundingSphere.Center);
                            chunkIO.WriteChunkFloat(Wad2Chunks.SphereRadius, msh.BoundingSphere.Radius);
                        });

                        // Write bounding box
                        chunkIO.WriteChunk(Wad2Chunks.BoundingBox, () =>
                        {
                            chunkIO.WriteChunkVector3(Wad2Chunks.BoundingBoxMin, msh.BoundingBox.Minimum);
                            chunkIO.WriteChunkVector3(Wad2Chunks.BoundingBoxMax, msh.BoundingBox.Maximum);
                        });

                        // Write positions
                        chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshVertexPositions, () =>
                        {
                            foreach (var pos in msh.VerticesPositions)
                            {
                                chunkIO.WriteChunkVector3(Wad2Chunks.MeshVertexPosition, pos);
                            }
                        });

                        // Write normals
                        chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshVertexNormals, () =>
                        {
                            foreach (var normal in msh.VerticesNormals)
                            {
                                chunkIO.WriteChunkVector3(Wad2Chunks.MeshVertexNormal, normal);
                            }
                        });

                        // Write shades
                        chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshVertexShades, () =>
                        {
                            foreach (var shade in msh.VerticesShades)
                            {
                                chunkIO.WriteChunkInt(Wad2Chunks.MeshVertexShade, shade);
                            }
                        });

                        // Write polygons
                        chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshPolygons, () =>
                        {
                            foreach (var poly in msh.Polys)
                            {
                                if (poly.Shape == WadPolygonShape.Rectangle)
                                {
                                    chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshRectangle, () =>
                                    {
                                        LEB128.Write(chunkIO.Raw, (ushort)poly.Shape);
                                        LEB128.Write(chunkIO.Raw, (byte)(poly.Transparent ? 1 : 0));
                                        LEB128.Write(chunkIO.Raw, (byte)poly.Attributes);

                                        chunkIO.WriteChunk(Wad2Chunks.MeshPolygonIndices, () =>
                                        {
                                            LEB128.Write(chunkIO.Raw, (int)poly.Indices[0]);
                                            LEB128.Write(chunkIO.Raw, (int)poly.Indices[1]);
                                            LEB128.Write(chunkIO.Raw, (int)poly.Indices[2]);
                                            LEB128.Write(chunkIO.Raw, (int)poly.Indices[3]);
                                        });

                                        chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshPolygonTexCoords, () =>
                                        {
                                            chunkIO.WriteChunkVector2(Wad2Chunks.MeshPolygonTexCoord, poly.UV[0]);
                                            chunkIO.WriteChunkVector2(Wad2Chunks.MeshPolygonTexCoord, poly.UV[1]);
                                            chunkIO.WriteChunkVector2(Wad2Chunks.MeshPolygonTexCoord, poly.UV[2]);
                                            chunkIO.WriteChunkVector2(Wad2Chunks.MeshPolygonTexCoord, poly.UV[3]);
                                        });
                                    });
                                }
                                else
                                {
                                    chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshTriangle, () =>
                                    {
                                        LEB128.Write(chunkIO.Raw, (ushort)poly.Shape);
                                        LEB128.Write(chunkIO.Raw, (byte)(poly.Transparent ? 1 : 0));
                                        LEB128.Write(chunkIO.Raw, (byte)poly.Attributes);

                                        chunkIO.WriteChunk(Wad2Chunks.MeshPolygonIndices, () =>
                                        {
                                            LEB128.Write(chunkIO.Raw, (int)poly.Indices[0]);
                                            LEB128.Write(chunkIO.Raw, (int)poly.Indices[1]);
                                            LEB128.Write(chunkIO.Raw, (int)poly.Indices[2]);
                                        });

                                        chunkIO.WriteChunkWithChildren(Wad2Chunks.MeshPolygonTexCoords, () =>
                                        {
                                            chunkIO.WriteChunkVector2(Wad2Chunks.MeshPolygonTexCoord, poly.UV[0]);
                                            chunkIO.WriteChunkVector2(Wad2Chunks.MeshPolygonTexCoord, poly.UV[1]);
                                            chunkIO.WriteChunkVector2(Wad2Chunks.MeshPolygonTexCoord, poly.UV[2]);
                                        });
                                    });
                                }
                            }
                        });
                    });
                }
            });
        }

        private static void WriteSamples(ChunkWriter chunkIO, Wad2 wad)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.Waves, () =>
            {
                foreach (var sample in wad.WaveSounds)
                {
                    chunkIO.WriteChunkWithChildren(Wad2Chunks.Wave, () =>
                    {
                        var wave = sample.Value;

                        chunkIO.WriteChunkString(Wad2Chunks.WaveName, wave.Name);
                        chunkIO.WriteChunkArrayOfBytes(Wad2Chunks.WaveData, wave.WaveData);
                    });
                }
            });
        }

        private static void WriteSprites(ChunkWriter chunkIO, Wad2 wad)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.Sprites, () =>
            {
                foreach (var sprite in wad.SpriteTextures)
                {
                    chunkIO.WriteChunkWithChildren(Wad2Chunks.Sprite, () =>
                    {
                        var txt = sprite.Value;
                        LEB128.Write(chunkIO.Raw, txt.Width);
                        LEB128.Write(chunkIO.Raw, txt.Height);
                        chunkIO.WriteChunkArrayOfBytes(Wad2Chunks.TextureData, txt.Image.ToByteArray());
                    });
                }
            });
        }

        private static void WriteSpriteSequences(ChunkWriter chunkIO, Wad2 wad)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.SpriteSequences, () =>
            {
                foreach (var sequence in wad.SpriteSequences)
                {
                    chunkIO.WriteChunkWithChildren(Wad2Chunks.SpriteSequence, () =>
                    {
                        LEB128.Write(chunkIO.Raw, sequence.ObjectID);
                        chunkIO.WriteChunkWithChildren(Wad2Chunks.SpriteSequenceSprites, () =>
                        {
                            foreach (var spr in sequence.Sprites)
                                chunkIO.WriteChunkInt(Wad2Chunks.SpriteSequenceSprite, _spritesTable.IndexOf(spr));
                        });
                    });
                }
            });
        }

        private static void WriteMoveables(ChunkWriter chunkIO, Wad2 wad)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.Moveables, () =>
            {
                foreach (var moveable in wad.Moveables)
                {
                    chunkIO.WriteChunkWithChildren(Wad2Chunks.Moveable, () =>
                    {
                        var m = moveable.Value;

                        LEB128.Write(chunkIO.Raw, m.ObjectID);
                        chunkIO.WriteChunkVector3(Wad2Chunks.MoveableOffset, m.Offset);

                        chunkIO.WriteChunkWithChildren(Wad2Chunks.MoveableMeshes, () =>
                        {
                            foreach (var mesh in m.Meshes)
                                chunkIO.WriteChunkInt(Wad2Chunks.MoveableMesh, _meshesTable.IndexOf(mesh));
                        });

                        chunkIO.WriteChunkWithChildren(Wad2Chunks.MoveableLinks, () =>
                        {
                            foreach (var link in m.Links)
                            {
                                chunkIO.WriteChunkWithChildren(Wad2Chunks.MoveableLink, () =>
                                {
                                    LEB128.Write(chunkIO.Raw, (ushort)link.Opcode);
                                    chunkIO.WriteChunkVector3(Wad2Chunks.MoveableLinkOffset, link.Offset);
                                });
                            }
                        });

                        chunkIO.WriteChunkWithChildren(Wad2Chunks.Animations, () =>
                        {
                            foreach (var animation in m.Animations)
                            {
                                chunkIO.WriteChunkWithChildren(Wad2Chunks.Animation, () =>
                                {
                                    chunkIO.WriteChunkString(Wad2Chunks.AnimationName, animation.Name);
                                    LEB128.Write(chunkIO.Raw, animation.StateId);
                                    LEB128.Write(chunkIO.Raw, animation.RealNumberOfFrames);
                                    LEB128.Write(chunkIO.Raw, animation.FrameDuration);
                                    LEB128.Write(chunkIO.Raw, animation.FrameStart);
                                    LEB128.Write(chunkIO.Raw, animation.FrameEnd);
                                    LEB128.Write(chunkIO.Raw, animation.Speed);
                                    LEB128.Write(chunkIO.Raw, animation.Acceleration);
                                    LEB128.Write(chunkIO.Raw, animation.LateralSpeed);
                                    LEB128.Write(chunkIO.Raw, animation.LateralAcceleration);
                                    LEB128.Write(chunkIO.Raw, animation.NextAnimation);
                                    LEB128.Write(chunkIO.Raw, animation.NextFrame);

                                    chunkIO.WriteChunkWithChildren(Wad2Chunks.KeyFrames, () =>
                                    {
                                        foreach (var kf in animation.KeyFrames)
                                        {
                                            chunkIO.WriteChunkWithChildren(Wad2Chunks.KeyFrame, () =>
                                            {
                                                chunkIO.WriteChunkVector3(Wad2Chunks.KeyFrameOffset, kf.Offset);
                                                chunkIO.WriteChunk(Wad2Chunks.BoundingBox, () =>
                                                {
                                                    chunkIO.WriteChunkVector3(Wad2Chunks.BoundingBoxMin, kf.BoundingBox.Minimum);
                                                    chunkIO.WriteChunkVector3(Wad2Chunks.BoundingBoxMax, kf.BoundingBox.Maximum);
                                                });

                                                chunkIO.WriteChunkWithChildren(Wad2Chunks.KeyFrameAngles, () =>
                                                {
                                                    foreach (var angle in kf.Angles)
                                                    {
                                                        chunkIO.WriteChunk(Wad2Chunks.KeyFrameAngle, () =>
                                                        {
                                                            LEB128.Write(chunkIO.Raw, (ushort)angle.Axis);
                                                            LEB128.Write(chunkIO.Raw, angle.X);
                                                            LEB128.Write(chunkIO.Raw, angle.Y);
                                                            LEB128.Write(chunkIO.Raw, angle.Z);
                                                        });
                                                    }
                                                });
                                            });
                                        }
                                    });

                                    chunkIO.WriteChunkWithChildren(Wad2Chunks.StateChanges, () =>
                                    {
                                        foreach (var stateChange in animation.StateChanges)
                                        {
                                            chunkIO.WriteChunkWithChildren(Wad2Chunks.StateChange, () =>
                                            {
                                                LEB128.Write(chunkIO.Raw, stateChange.StateId);
                                                chunkIO.WriteChunkWithChildren(Wad2Chunks.Dispatches, () =>
                                                {
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
                                            });
                                        }
                                    });

                                    chunkIO.WriteChunkWithChildren(Wad2Chunks.AnimCommands, () =>
                                    {
                                        foreach (var command in animation.AnimCommands)
                                        {
                                            chunkIO.WriteChunk(Wad2Chunks.AnimCommand, () =>
                                            {
                                                LEB128.Write(chunkIO.Raw, (ushort)command.Type);
                                                LEB128.Write(chunkIO.Raw, command.Parameter1);
                                                LEB128.Write(chunkIO.Raw, command.Parameter2);
                                                LEB128.Write(chunkIO.Raw, command.Parameter3);
                                            });
                                        }
                                    });
                                });
                            }
                        });
                    });
                }
            });
        }

        private static void WriteStatics(ChunkWriter chunkIO, Wad2 wad)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.Statics, () =>
            {
                foreach (var staticMesh in wad.Statics)
                {
                    chunkIO.WriteChunkWithChildren(Wad2Chunks.Static, () =>
                    {
                        var s = staticMesh.Value;

                        LEB128.Write(chunkIO.Raw, s.ObjectID);
                        LEB128.Write(chunkIO.Raw, _meshesTable.IndexOf(s.Mesh));
                        LEB128.Write(chunkIO.Raw, s.Flags);

                        chunkIO.WriteChunk(Wad2Chunks.BoundingBox, () =>
                        {
                            chunkIO.WriteChunkVector3(Wad2Chunks.BoundingBoxMin, s.VisibilityBox.Minimum);
                            chunkIO.WriteChunkVector3(Wad2Chunks.BoundingBoxMax, s.VisibilityBox.Maximum);
                        });

                        chunkIO.WriteChunk(Wad2Chunks.BoundingBox, () =>
                        {
                            chunkIO.WriteChunkVector3(Wad2Chunks.BoundingBoxMin, s.CollisionBox.Minimum);
                            chunkIO.WriteChunkVector3(Wad2Chunks.BoundingBoxMax, s.CollisionBox.Maximum);
                        });
                    });
                }
            });
        }

        private static void WriteSounds(ChunkWriter chunkIO, Wad2 wad)
        {
            chunkIO.WriteChunkWithChildren(Wad2Chunks.Sounds, () =>
            {
                foreach (var sound in wad.SoundInfo)
                {
                    chunkIO.WriteChunkWithChildren(Wad2Chunks.Sound, () =>
                    {
                        var s = sound.Value;

                        chunkIO.WriteChunkString(Wad2Chunks.SoundName, s.Name);
                        
                        LEB128.Write(chunkIO.Raw, s.Volume);
                        LEB128.Write(chunkIO.Raw, s.Range);
                        LEB128.Write(chunkIO.Raw, s.Pitch);
                        LEB128.Write(chunkIO.Raw, s.Chance);
                        LEB128.Write(chunkIO.Raw, (s.FlagN ? 1 : 0));
                        LEB128.Write(chunkIO.Raw, (s.RandomizePitch ? 1 : 0));
                        LEB128.Write(chunkIO.Raw, (s.RandomizeGain ? 1 : 0));
                        LEB128.Write(chunkIO.Raw, (ushort)s.Loop);

                        foreach (var wav in s.WaveSounds)
                            chunkIO.WriteChunkInt(Wad2Chunks.SoundSample, _wavesTable.IndexOf(wav));
                    });
                }
            });
        }
    }
}
