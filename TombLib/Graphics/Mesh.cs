using System.Collections.Generic;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using SharpDX;

namespace TombLib.Graphics
{
    public abstract class Mesh<T> : GraphicsResource where T : struct, IVertex
    {
        public int BaseIndex { get; set; }
        public int NumIndices { get; set; }
        public List<T> Vertices { get; set; } = new List<T>();
        public List<int> Indices { get; set; } = new List<int>();
        public Buffer<T> VertexBuffer { get; protected set; }
        public Buffer IndexBuffer { get; protected set; }

        public List<Submesh> SubMeshes { get; set; }
        public BoundingSphere BoundingSphere { get; set; }
        public BoundingBox BoundingBox { get; set; }

        protected Buffer<T> Vb;
        protected Buffer Ib;

        public Mesh(GraphicsDevice device, string name)
            : base(device, name)
        {
            SubMeshes = new List<Submesh>();
        }

        public abstract void BuildBuffers();
    }
}
