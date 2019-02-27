using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.Utils;
using TombLib.Wad;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public enum ModelType : short
    {
        Static,
        Skinned,
        RoomGeometry,
        Room
    }

    public abstract class Model<T, U> : IRenderableObject, IDisposable where U : struct
    {
        //public Buffer<U> VertexBuffer { get; protected set; }
        //public Buffer IndexBuffer { get; protected set; }
        public BoundingBox BoundingBox { get; set; }
        public List<T> Meshes { get; set; }
        public GraphicsDevice GraphicsDevice { get; set; }
        public ModelType Type { get; set; }
        //public List<U> Vertices { get; set; }
        //public List<int> Indices { get; set; }
        public string Name { get; set; }
        public List<Material> Materials { get; private set; } = new List<Material>();
        public DataVersion Version { get; set; } = DataVersion.GetNext();

        public Model(GraphicsDevice device, ModelType type)
        {
            GraphicsDevice = device;
            Type = type;
            Meshes = new List<T>();
        }

        public abstract void UpdateBuffers();

        protected static void PutObjectVertexAndIndex(Vector3 v, ObjectMesh mesh, Submesh submesh, Vector2 uv, int submeshIndex,
                                                      short color, Vector2 positionInAtlas)
        {
            var newVertex = new ObjectVertex();

            newVertex.Position = new Vector3(v.X, v.Y, v.Z);
            newVertex.UV = new Vector2((positionInAtlas.X + uv.X) / WadRenderer.TextureAtlasSize,
                                       (positionInAtlas.Y + uv.Y) / WadRenderer.TextureAtlasSize);

            var shade = 1.0f - color / 8191.0f;
            newVertex.Shade = new Vector2(shade, 0.0f);

            mesh.Vertices.Add(newVertex);
            submesh.Indices.Add((ushort)(mesh.Vertices.Count - 1));
        }

        public void Dispose()
        {
            
        }
    }
}
