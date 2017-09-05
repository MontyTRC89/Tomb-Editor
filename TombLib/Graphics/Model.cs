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

        protected static void PutSkinnedVertexAndIndex(Vector3 v, SkinnedMesh mesh, Vector2 uv,
                                                       int submeshIndex, int boneIndex, Vector2 positionInAtlas)
        {
            SkinnedVertex newVertex = new SkinnedVertex();

            newVertex.Position = new Vector3(v.X, v.Y, v.Z);
            newVertex.Normal = Vector3.Zero;
            newVertex.Tangent = Vector3.Zero;
            newVertex.Binormal = Vector3.Zero;
            newVertex.UV = new Vector2((positionInAtlas.X + uv.X) / (float)Wad2.TextureAtlasSize, 
                                       (positionInAtlas.Y + uv.Y) / (float)Wad2.TextureAtlasSize);

            mesh.Vertices.Add(newVertex);
            mesh.Indices.Add((ushort)(mesh.Vertices.Count - 1));
        }

        protected static void PutStaticVertexAndIndex(Vector3 v, StaticMesh mesh, Vector2 uv, int submeshIndex,
                                                      short color, Vector2 positionInAtlas)
        {
            StaticVertex newVertex = new StaticVertex();

            newVertex.Position = new Vector3(v.X, v.Y, v.Z);
            newVertex.Normal = Vector3.Zero;
            newVertex.UV = new Vector2((positionInAtlas.X + uv.X) / (float)Wad2.TextureAtlasSize, 
                                       (positionInAtlas.Y + uv.Y) / (float)Wad2.TextureAtlasSize);

            var shade = 1.0f - color / 8191.0f;
            newVertex.Shade = new Vector2(shade, 0.0f);

            mesh.Vertices.Add(newVertex);
            mesh.Indices.Add((ushort)(mesh.Vertices.Count - 1));
        }
    }
}
