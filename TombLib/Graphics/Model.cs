using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;
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

    public abstract class Model<T, U> : IDisposable where U : struct
    {
        public Buffer<U> VertexBuffer { get; protected set; }
        public Buffer IndexBuffer { get; protected set; }
        public BoundingBox BoundingBox { get; set; }
        public List<T> Meshes { get; set; }
        public GraphicsDevice GraphicsDevice { get; set; }
        public ModelType Type { get; set; }
        public List<U> Vertices { get; set; }
        public List<int> Indices { get; set; }
        public string Name { get; set; }

        public Model(GraphicsDevice device, ModelType type)
        {
            GraphicsDevice = device;
            Type = type;
            Meshes = new List<T>();
        }

        public virtual void Dispose()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }

        public abstract void BuildBuffers();

        protected static List<Vector2> CalculateUVCoordinates(WadPolygon poly, Dictionary<uint, WadTextureSample> textureSamples)
        {
            List<Vector2> uv = new List<Vector2>();

            // recupero le informazioni necessarie
            int shape = (poly.Texture & 0x7000) >> 12;
            int flipped = (poly.Texture & 0x8000) >> 15;
            int textureId = poly.Texture & 0xfff;
            if (textureId > 2047)
                textureId = -(textureId - 4096);


            // calcolo i quattro angoli della texture
            WadTextureSample texture = textureSamples[(uint)textureId];

            int yBlock = (int)(texture.Page / 8);
            int xBlock = (int)(texture.Page % 8);

            Vector2 nw = new Vector2((xBlock * 256 + texture.X) / 2048.0f, (yBlock * 256 + texture.Y) / 2048.0f);
            Vector2 ne = new Vector2((xBlock * 256 + texture.X + texture.Width) / 2048.0f, (yBlock * 256 + texture.Y) / 2048.0f);
            Vector2 se = new Vector2((xBlock * 256 + texture.X + texture.Width) / 2048.0f, (yBlock * 256 + texture.Y + texture.Height) / 2048.0f);
            Vector2 sw = new Vector2((xBlock * 256 + texture.X) / 2048.0f, (yBlock * 256 + texture.Y + texture.Height) / 2048.0f);

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

        protected static void AddSkinnedVertexAndIndex(WadVector v, SkinnedMesh mesh, Vector2 uv, int submeshIndex, int boneIndex)
        {
            SkinnedVertex newVertex = new SkinnedVertex();

            newVertex.Position = new Vector4(v.X, -v.Y, v.Z, 1);
            newVertex.Normal = Vector3.Zero;
            newVertex.Tangent = Vector3.Zero;
            newVertex.Binormal = Vector3.Zero;
            newVertex.BoneWeigths = new Vector4(1, 0, 0, 0);
            newVertex.BoneIndices = new Vector4(boneIndex, 0, 0, 0);
            newVertex.UV = uv;

            mesh.Vertices.Add(newVertex);
            mesh.SubMeshes[submeshIndex].Indices.Add((ushort)(mesh.Vertices.Count - 1));
        }

        protected static void AddStaticVertexAndIndex(WadVector v, StaticMesh mesh, Vector2 uv, int submeshIndex, short color)
        {
            StaticVertex newVertex = new StaticVertex();

            newVertex.Position = new Vector4(v.X, -v.Y, v.Z, 1);
            newVertex.Normal = Vector3.Zero;
            newVertex.UV = uv;

            var shade = 1.0f - color / 8191.0f;
            newVertex.Shade = new Vector2(shade, 0.0f);

            mesh.Vertices.Add(newVertex);
            mesh.SubMeshes[submeshIndex].Indices.Add((ushort)(mesh.Vertices.Count - 1));
        }
    }
}
