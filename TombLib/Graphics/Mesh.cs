using SharpDX.Toolkit.Graphics;
using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Graphics
{
    public abstract class Mesh<T> : GraphicsResource, IRenderableObject where T : struct, IVertex
    {
        public List<T> Vertices { get; set; } = new List<T>();
        public List<int> Indices { get; set; } = new List<int>();
        public Dictionary<Material, Submesh> Submeshes { get; private set; } = new Dictionary<Material, Submesh>();

        public BoundingSphere BoundingSphere { get; set; }
        public BoundingBox BoundingBox { get; set; }

        public Mesh(GraphicsDevice device, string name)
            : base(device, name)
        { }

        public void UpdateBoundingBox()
        {
            // Calculate bounding box
            Vector3 minVertex = new Vector3(float.MaxValue);
            Vector3 maxVertex = new Vector3(float.MinValue);
            foreach (var vertex in Vertices)
            {
                minVertex = Vector3.Min(minVertex, vertex.Position);
                maxVertex = Vector3.Max(maxVertex, vertex.Position);
            }
            BoundingBox = new BoundingBox(minVertex, maxVertex);
        }
    }
}
