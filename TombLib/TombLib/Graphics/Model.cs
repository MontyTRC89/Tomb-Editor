using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.Graphics
{
    public enum ModelType : short
    {
        Static,
        Skinned,
        RoomGeometry,
        Room
    }

    // Pure CPU-side model container. Holds a list of meshes plus an optional skin
    // mesh (for skinned moveables). After the unified-path migration the GPU upload
    // is handled by the renderer cache (Dx11RenderingDrawingMesh) — the model itself
    // doesn't own a GraphicsDevice or GPU buffers.
    public abstract class Model<T, U> : IRenderableObject, IDisposable where U : unmanaged where T : IDisposable
    {
        public BoundingBox BoundingBox { get; set; }
        public List<T> Meshes { get; set; }
        public T Skin { get; set; }
        public ModelType Type { get; set; }
        public string Name { get; set; }
        public List<Material> Materials { get; private set; } = new List<Material>();
        public DataVersion Version { get; set; } = DataVersion.GetNext();

        public Model(ModelType type)
        {
            Type = type;
            Meshes = new List<T>();
        }

        public abstract void UpdateBuffers(Vector3? position = null);

        public void Dispose()
        {
            foreach (var m in Meshes)
                m.Dispose();
        }
    }
}
