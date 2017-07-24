using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using SharpDX.Direct3D11;
using Texture2D=SharpDX.Toolkit.Graphics.Texture2D;
using SharpDX;

namespace TombLib.Graphics
{
    public abstract class Mesh<T> : GraphicsResource where T : struct
    {
        public int BaseIndex { get; set; }
        public int NumIndices { get; set; }

        protected Buffer<T> _vb;
        protected Buffer _ib;

        public Mesh(GraphicsDevice device, string name)
            : base(device, name)
        {
            Vertices = new List<T>();
            Indices = new List<int>();
            SubMeshes = new List<Submesh>();
        }

        public abstract void BuildBuffers();
        /*
        {
            _vb = Buffer.Vertex.New<T>(GraphicsDevice, Vertices.ToArray(), ResourceUsage.Dynamic);
            _ib = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), ResourceUsage.Dynamic);
        }*/

        public List<T> Vertices { get; set; }

        public List<int> Indices { get; set; }

        public Buffer<T> VertexBuffer
        {
            get
            {
                return _vb;
            }
        }

        public Buffer IndexBuffer
        {
            get
            {
                return _ib;
            }
        }

        public List<Submesh> SubMeshes { get; set; }

        public BoundingSphere BoundingSphere { get; set; }

        public BoundingBox BoundingBox { get; set; }
    }
}
