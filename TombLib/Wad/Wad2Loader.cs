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
        public static Wad2 LoadFromWad2(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var chunkIO = new ChunkReader(Wad2Chunks.MagicNumber, fileStream))
                return LoadWad2(chunkIO, filename);
        }

        private static Wad2 LoadWad2(ChunkReader chunkIO, string thisPath)
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

                wad.Textures.Add(texture.Hash, texture);

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
                            if (id3 == Wad2Chunks.MeshRectangle)
                            {
                                var rectangle = new WadPolygon(WadPolygonShape.Rectangle);
                                rectangle.Transparent = (LEB128.ReadByte(chunkIO.Raw) == 1);
                                rectangle.Attributes = LEB128.ReadByte(chunkIO.Raw);
                                rectangle.ShineStrength = LEB128.ReadByte(chunkIO.Raw);
                                rectangle.Texture = wad.Textures.ElementAt(LEB128.ReadInt(chunkIO.Raw)).Value;
                                rectangle.Indices.Add(LEB128.ReadInt(chunkIO.Raw));
                                rectangle.Indices.Add(LEB128.ReadInt(chunkIO.Raw));
                                rectangle.Indices.Add(LEB128.ReadInt(chunkIO.Raw));
                                rectangle.Indices.Add(LEB128.ReadInt(chunkIO.Raw));
                                chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 == Wad2Chunks.MeshPolygonTexCoord)
                                        rectangle.UV.Add(chunkIO.ReadChunkVector2(chunkSize4));
                                    else
                                        return false;
                                    return true;
                                });
                                mesh.Polys.Add(rectangle);
                            }
                            else if (id3 == Wad2Chunks.MeshTriangle)
                            {
                                var triangle = new WadPolygon(WadPolygonShape.Triangle);
                                triangle.Transparent = (LEB128.ReadByte(chunkIO.Raw) == 1);
                                triangle.Attributes = LEB128.ReadByte(chunkIO.Raw);
                                triangle.ShineStrength = LEB128.ReadByte(chunkIO.Raw);
                                triangle.Texture = wad.Textures.ElementAt(LEB128.ReadInt(chunkIO.Raw)).Value;
                                triangle.Indices.Add(LEB128.ReadInt(chunkIO.Raw));
                                triangle.Indices.Add(LEB128.ReadInt(chunkIO.Raw));
                                triangle.Indices.Add(LEB128.ReadInt(chunkIO.Raw));
                                chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 == Wad2Chunks.MeshPolygonTexCoord)
                                        triangle.UV.Add(chunkIO.ReadChunkVector2(chunkSize4));
                                    else
                                        return false;
                                    return true;
                                });
                                mesh.Polys.Add(triangle);
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

                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.AnimationName)
                                animation.Name = chunkIO.ReadChunkString(chunkSize3);
                            else
                                return false;
                            return true;
                        });

                        animation.StateId= LEB128.ReadUShort(chunkIO.Raw);
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
                            if (id3 == Wad2Chunks.KeyFrame)
                            {
                                var keyframe = new WadKeyFrame();

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

                return true;
            });

            return true;
        }

        private static bool LoadStatics(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            return true;
        }

        private static bool LoadSpriteSequences(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            return true;
        }

        private static bool LoadSounds(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            return true;
        }
    }
}
