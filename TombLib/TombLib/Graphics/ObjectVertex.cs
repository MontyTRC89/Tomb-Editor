using System.Numerics;
using System.Runtime.InteropServices;

namespace TombLib.Graphics
{
    // CPU-side vertex layout produced by ObjectMesh.FromWad2 and consumed by
    // Dx11RenderingDrawingMesh (which mirrors it into the MeshVertex GPU layout
    // — see Dx11RenderingDevice.MeshShader InputLayout).
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ObjectVertex : IVertex
    {
        public Vector3 Position;
        public Vector3 UVW;
        public Vector3 Normal;
        public Vector3 Color;
        public Vector4 Indices;
        public Vector4 Weights;

        Vector3 IVertex.Position => Position;
    }
}
