using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace TombLib.Graphics
{
    // Pure CPU-side mesh data. After the unified-path migration the mesh classes are
    // no longer responsible for owning GPU resources — the renderer (Dx11RenderingDrawingMesh)
    // mirrors Vertices/Indices/Submeshes into immutable D3D11 buffers on first use
    // and caches them by mesh reference.
    //
    // Removed compared to the legacy SharpDX.Toolkit version:
    //   - GraphicsResource base class and Name/Tags fields → not needed
    //   - VertexBuffer, IndexBuffer, InputLayout → owned by the renderer cache
    //   - GraphicsDevice constructor parameter → no GPU device handle here
    //
    // The class still implements IRenderableObject (= IDisposable) for source-compat
    // with the few sites that wrap meshes in `using` blocks; the implementation is
    // a no-op now.
    public abstract class Mesh<T> : IRenderableObject where T : struct, IVertex
    {
        public string Name { get; protected set; }
        public List<Material> Materials { get; protected set; }
        public List<T> Vertices { get; protected set; } = new List<T>();
        public List<int> Indices { get; protected set; } = new List<int>();
        public Dictionary<Material, Submesh> Submeshes { get; private set; } = new Dictionary<Material, Submesh>();
        public bool Hidden { get; set; } = false;

        public BoundingSphere BoundingSphere { get; set; }
        public BoundingBox BoundingBox { get; set; }

        // Monotonic version counter — bumped on UpdateBuffers() so the renderer
        // cache can detect content changes and rebuild its GPU buffer. Replaces the
        // legacy "rebuild on every UpdateBuffers" pattern.
        public int Version { get; private set; }

        protected Mesh(string name)
        {
            Name = name;
        }

        public void UpdateBoundingBox()
        {
            Vector3 minVertex = new Vector3(float.MaxValue);
            Vector3 maxVertex = new Vector3(float.MinValue);
            foreach (var vertex in Vertices)
            {
                minVertex = Vector3.Min(minVertex, vertex.Position);
                maxVertex = Vector3.Max(maxVertex, vertex.Position);
            }
            BoundingBox = new BoundingBox(minVertex, maxVertex);
        }

        // Sorts triangles back-to-front per submesh based on distance from `position`.
        // Used to keep the cached renderer mesh's index order consistent with the legacy
        // behaviour, even though the actual sorting matters less now (the renderer doesn't
        // re-upload on every frame; cache invalidates only when Version changes).
        public void DepthSort(Vector3? position)
        {
            int lastBaseIndex = 0;
            Indices.Clear();

            foreach (var submesh in Submeshes)
            {
                submesh.Value.BaseIndex = lastBaseIndex;
                if (submesh.Value.NumIndices != 0)
                {
                    var indexList = new List<int[]>();
                    for (int i = 0; i < submesh.Value.NumIndices; i += 3)
                        indexList.Add(new int[3] { submesh.Value.Indices[i], submesh.Value.Indices[i + 1], submesh.Value.Indices[i + 2] });

                    if (position != null)
                        indexList = indexList.OrderByDescending(p => Vector3.Distance(position.Value,
                            (Vertices[p[0]].Position + Vertices[p[1]].Position + Vertices[p[2]].Position) / 3.0f)).ToList();

                    foreach (var tri in indexList)
                        Indices.AddRange(tri);
                }
                lastBaseIndex += submesh.Value.NumIndices;
            }
        }

        // Bumps Version so the renderer cache rebuilds its GPU buffer on next access.
        // Subclasses call this from their own UpdateBuffers() after rebuilding the
        // CPU-side index list (e.g. ObjectMesh.UpdateBuffers does DepthSort first).
        protected void BumpVersion() => Version++;

        public void Dispose()
        {
            // No-op: no GPU resources to dispose. Kept for IRenderableObject contract.
        }
    }
}
