using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public enum ModelType : short
    {
        Static,
        Skinned
    }

    public abstract class Model<T, U> where U : struct
    {
        protected Buffer<U> _vb;
        protected Buffer _ib;

        public Model(GraphicsDevice device, uint objectId, ModelType type)
        {
            GraphicsDevice = device;
            ObjectID = objectId;
            Type = type;
            Meshes = new List<T>();
        }

        public uint ObjectID { get; set; }

        public BoundingBox BoundingBox { get; set; }

        public List<T> Meshes { get; set; }

        public GraphicsDevice GraphicsDevice { get; set; }

        public ModelType Type{ get; set; }

        public abstract void BuildBuffers();

        public List<U> Vertices { get; set; }

        public List<int> Indices { get; set; }

        public Buffer<U> VertexBuffer
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
    }
}
