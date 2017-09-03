using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using TombLib.Wad;

namespace TombLib.Graphics
{
    public enum ModelType : short
    {
        Static,
        Skinned,
        RoomGeometry
    }

    public abstract class Model<TMesh, TVertex> : IDisposable where TVertex : struct
    {
        public Buffer<TVertex> VertexBuffer { get; protected set; }
        public Buffer IndexBuffer { get; protected set; }
        public BoundingBox BoundingBox { get; set; }
        public List<TMesh> Meshes { get; set; }
        public GraphicsDevice GraphicsDevice { get; set; }
        public ModelType Type { get; set; }
        public List<TVertex> Vertices { get; set; }
        public List<int> Indices { get; set; }
        public string Name { get; set; }

        protected Model(GraphicsDevice device, ModelType type)
        {
            GraphicsDevice = device;
            Type = type;
            Meshes = new List<TMesh>();
        }

        public virtual void Dispose()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }

        public abstract void BuildBuffers();

        protected static List<Vector2> CalculateUvCoordinates(WadPolygon poly,
            Dictionary<uint, WadTextureSample> textureSamples)
        {
            var uv = new List<Vector2>();

            // recupero le informazioni necessarie
            int shape = (poly.Texture & 0x7000) >> 12;
            int flipped = (poly.Texture & 0x8000) >> 15;
            int textureId = poly.Texture & 0xfff;
            if (textureId > 2047)
                textureId = -(textureId - 4096);


            // calcolo i quattro angoli della texture
            var texture = textureSamples[(uint)textureId];

            int yBlock = texture.Page / 8;
            int xBlock = texture.Page % 8;

            var nw = new Vector2((xBlock * 256 + texture.X) / 2048.0f, (yBlock * 256 + texture.Y) / 2048.0f);
            var ne = new Vector2((xBlock * 256 + texture.X + texture.Width) / 2048.0f,
                (yBlock * 256 + texture.Y) / 2048.0f);
            var se = new Vector2((xBlock * 256 + texture.X + texture.Width) / 2048.0f,
                (yBlock * 256 + texture.Y + texture.Height) / 2048.0f);
            var sw = new Vector2((xBlock * 256 + texture.X) / 2048.0f,
                (yBlock * 256 + texture.Y + texture.Height) / 2048.0f);

            // in base alla forma assegno nel giusto ordine le coordinate
            if (poly.Shape == Shape.Rectangle)
            {
                if (flipped == 1)
                {
                    uv.Add(ne);
                    uv.Add(nw);
                    uv.Add(sw);
                    uv.Add(se);
                }
                else
                {
                    uv.Add(nw);
                    uv.Add(ne);
                    uv.Add(se);
                    uv.Add(sw);
                }
            }
            else
            {
                switch (shape)
                {
                    case 0:
                        if (flipped == 1)
                        {
                            uv.Add(ne);
                            uv.Add(nw);
                            uv.Add(se);
                        }
                        else
                        {
                            uv.Add(nw);
                            uv.Add(ne);
                            uv.Add(sw);
                        }
                        break;

                    case 2:
                        if (flipped == 1)
                        {
                            uv.Add(nw);
                            uv.Add(sw);
                            uv.Add(ne);
                        }
                        else
                        {
                            uv.Add(ne);
                            uv.Add(se);
                            uv.Add(nw);
                        }
                        break;

                    case 4:
                        if (flipped == 1)
                        {
                            uv.Add(sw);
                            uv.Add(se);
                            uv.Add(nw);
                        }
                        else
                        {
                            uv.Add(se);
                            uv.Add(sw);
                            uv.Add(ne);
                        }
                        break;

                    case 6:
                        if (flipped == 1)
                        {
                            uv.Add(se);
                            uv.Add(ne);
                            uv.Add(sw);
                        }
                        else
                        {
                            uv.Add(sw);
                            uv.Add(nw);
                            uv.Add(se);
                        }
                        break;
                }
            }

            return uv;
        }

        protected static void AddSkinnedVertexAndIndex(WadVector v, SkinnedMesh mesh, Vector2 uv, int submeshIndex,
            int boneIndex)
        {
            var newVertex = new SkinnedVertex
            {
                Position = new Vector3(v.X, -v.Y, v.Z),
                Normal = Vector3.Zero,
                Tangent = Vector3.Zero,
                Binormal = Vector3.Zero,
                BoneWeigths = new Vector4(1, 0, 0, 0),
                BoneIndices = new Vector4(boneIndex, 0, 0, 0),
                UV = uv
            };

            mesh.Vertices.Add(newVertex);
            mesh.SubMeshes[submeshIndex].Indices.Add((ushort)(mesh.Vertices.Count - 1));
        }

        protected static void AddStaticVertexAndIndex(WadVector v, StaticMesh mesh, Vector2 uv, int submeshIndex,
            short color)
        {
            var newVertex = new StaticVertex
            {
                Position = new Vector3(v.X, -v.Y, v.Z),
                Normal = Vector3.Zero,
                UV = uv
            };

            var shade = 1.0f - color / 8191.0f;
            newVertex.Shade = new Vector2(shade, 0.0f);

            mesh.Vertices.Add(newVertex);
            mesh.SubMeshes[submeshIndex].Indices.Add((ushort)(mesh.Vertices.Count - 1));
        }
    }
}
