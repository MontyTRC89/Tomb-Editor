using System.Numerics;
using System.Runtime.InteropServices;

namespace TombLib.Graphics
{
    // Plain 32-byte vertex (Vector3 + 4-byte padding + Vector4) historically used by
    // the legacy SharpDX.Toolkit GeometricPrimitive helpers. After the rendering
    // unification only the type itself survives — used in a handful of places that
    // build CPU-side vertex lists for utility purposes (e.g. AddObjectHeightLine).
    // The renderer no longer consumes this struct directly; SolidLineVertex (in
    // TombLib.Rendering) is what the unified path expects.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SolidVertex : IVertex
    {
        public Vector3 Position;
        private readonly float _unusedPadding;
        public Vector4 Color;

        Vector3 IVertex.Position => Position;

        public SolidVertex(Vector3 pos)
        {
            Position = pos;
            _unusedPadding = 0;
            Color = Vector4.One;
        }
    }
}
