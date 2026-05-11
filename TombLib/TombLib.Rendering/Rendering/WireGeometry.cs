using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Rendering
{
    // Static helpers that emit wireframe geometry (cube, sphere, cone) into a
    // List<SolidLineVertex> ready to be handed to RenderingDrawingLines as a single
    // LineList batch. Used by every caller that previously bound a triangle
    // GeometricPrimitive (e.g. _littleCube, _sphere, _cone) and rendered it with
    // the wireframe rasterizer state to get a "wire" silhouette.
    //
    // Why CPU-side line generation instead of binding a static triangle mesh:
    //   - All instances accumulate into ONE draw call regardless of count, instead
    //     of one Draw per object.
    //   - No per-instance constant buffer rebind, no World matrix uniform — the
    //     transform is baked into vertex positions on the CPU.
    //   - Aligns with the unified RenderingDrawingLines abstraction, removes a
    //     dependency on SharpDX.Toolkit GeometricPrimitive.
    //
    // All methods APPEND to the supplied list — they never clear it. Callers are
    // expected to reuse a list across frames (Clear+Add) for zero-alloc steady state.
    public static class WireGeometry
    {
        // Unit-cube wireframe (8 corners spanning -1..+1, 12 edges = 24 line vertices).
        // Caller bakes scale into the World matrix.
        public static void AppendWireCube(List<SolidLineVertex> vertices, Matrix4x4 world, Vector4 color)
        {
            var c000 = Vector3.Transform(new Vector3(-1, -1, -1), world);
            var c100 = Vector3.Transform(new Vector3( 1, -1, -1), world);
            var c010 = Vector3.Transform(new Vector3(-1,  1, -1), world);
            var c110 = Vector3.Transform(new Vector3( 1,  1, -1), world);
            var c001 = Vector3.Transform(new Vector3(-1, -1,  1), world);
            var c101 = Vector3.Transform(new Vector3( 1, -1,  1), world);
            var c011 = Vector3.Transform(new Vector3(-1,  1,  1), world);
            var c111 = Vector3.Transform(new Vector3( 1,  1,  1), world);

            // bottom face
            Edge(vertices, c000, c100, color); Edge(vertices, c100, c101, color);
            Edge(vertices, c101, c001, color); Edge(vertices, c001, c000, color);
            // top face
            Edge(vertices, c010, c110, color); Edge(vertices, c110, c111, color);
            Edge(vertices, c111, c011, color); Edge(vertices, c011, c010, color);
            // vertical edges
            Edge(vertices, c000, c010, color); Edge(vertices, c100, c110, color);
            Edge(vertices, c101, c111, color); Edge(vertices, c001, c011, color);
        }

        // Flat XZ-plane grid in editor units. `sizePerSide` is half-extent in sectors;
        // `divisions` controls intermediate lines per sector. Used by every editor
        // panel that shows a reference grid (TombEditor's room grid, WadTool's
        // mesh/skeleton/animation/static panels' reference grid).
        public static void AppendGrid(List<SolidLineVertex> vertices, int sizePerSide, int divisions, Vector4 color)
        {
            const float sectorSize = 1024.0f;
            float halfExtent = sizePerSide * sectorSize;
            int totalLines = sizePerSide * divisions * 2 + 1; // per axis (per side × subdivision × 2 for negative side + center)
            float step = sectorSize / divisions;
            for (int i = -sizePerSide * divisions; i <= sizePerSide * divisions; ++i)
            {
                float c = i * step;
                // X-aligned line at z=c
                vertices.Add(new SolidLineVertex { Position = new Vector3(-halfExtent, 0, c), Color = color });
                vertices.Add(new SolidLineVertex { Position = new Vector3( halfExtent, 0, c), Color = color });
                // Z-aligned line at x=c
                vertices.Add(new SolidLineVertex { Position = new Vector3(c, 0, -halfExtent), Color = color });
                vertices.Add(new SolidLineVertex { Position = new Vector3(c, 0,  halfExtent), Color = color });
            }
        }

        // Convenience wrapper: AABB → wire cube. Computes the appropriate scale and
        // translation that maps a unit cube (-1..+1) to the AABB extents.
        public static void AppendWireBoundingBox(List<SolidLineVertex> vertices, TombLib.BoundingBox box, Vector4 color)
        {
            var center = (box.Minimum + box.Maximum) * 0.5f;
            var halfSize = (box.Maximum - box.Minimum) * 0.5f;
            var world = Matrix4x4.CreateScale(halfSize) * Matrix4x4.CreateTranslation(center);
            AppendWireCube(vertices, world, color);
        }

        // Unit-sphere wireframe (radius 1) made of three orthogonal great circles
        // (X, Y, Z planes). `segments` controls smoothness — 16 is fine for small
        // gizmi, 32 for selection highlights.
        //
        // Why three rings instead of full latitude/longitude grid: the editor uses
        // wire spheres as positional indicators (light radii, sound source range),
        // not as proper meshes. Three rings convey "spherical extent" with a
        // fraction of the vertex count of a tessellated sphere.
        public static void AppendWireSphere(List<SolidLineVertex> vertices, Matrix4x4 world, Vector4 color, int segments = 24)
        {
            // Each ring contributes `segments` line segments = `segments * 2` verts.
            // Three rings × segments lines.
            AppendCircle(vertices, world, color, segments, axis: 0);
            AppendCircle(vertices, world, color, segments, axis: 1);
            AppendCircle(vertices, world, color, segments, axis: 2);
        }

        // Unit-cone wireframe matching the legacy GeometricPrimitive.Cone convention:
        //   apex at (0, 0, 0) and base ring at z = 1 with unit radius.
        // Caller passes `Matrix4x4.CreateScale(baseRadius, baseRadius, length) * world`
        // so the X/Y scale controls the base radius and the Z scale the length.
        // Used by spot/sun light projection cones.
        public static void AppendWireCone(List<SolidLineVertex> vertices, Matrix4x4 world, Vector4 color, int segments = 24)
        {
            var apex = Vector3.Transform(Vector3.Zero, world);

            // Base ring at Z = 1
            float twoPi = (float)(2 * System.Math.PI);
            Vector3 first = Vector3.Transform(new Vector3(1, 0, 1), world);
            Vector3 prev = first;
            for (int i = 1; i <= segments; ++i)
            {
                float t = (i / (float)segments) * twoPi;
                Vector3 cur = i == segments
                    ? first
                    : Vector3.Transform(new Vector3((float)System.Math.Cos(t), (float)System.Math.Sin(t), 1), world);
                Edge(vertices, prev, cur, color);
                prev = cur;
            }

            // Four generatrices (apex → base ring) at 0°, 90°, 180°, 270° to make the
            // cone shape readable when seen edge-on.
            for (int i = 0; i < 4; ++i)
            {
                float t = (i / 4.0f) * twoPi;
                Vector3 baseP = Vector3.Transform(new Vector3((float)System.Math.Cos(t), (float)System.Math.Sin(t), 1), world);
                Edge(vertices, apex, baseP, color);
            }
        }

        // axis: 0 = ring in YZ plane (around X), 1 = XZ plane (around Y), 2 = XY plane (around Z).
        private static void AppendCircle(List<SolidLineVertex> vertices, Matrix4x4 world, Vector4 color, int segments, int axis)
        {
            float twoPi = (float)(2 * System.Math.PI);
            Vector3 first = PointOnCircle(world, axis, 0);
            Vector3 prev = first;
            for (int i = 1; i <= segments; ++i)
            {
                float t = (i / (float)segments) * twoPi;
                Vector3 cur = i == segments ? first : PointOnCircle(world, axis, t);
                Edge(vertices, prev, cur, color);
                prev = cur;
            }
        }

        private static Vector3 PointOnCircle(Matrix4x4 world, int axis, float t)
        {
            float c = (float)System.Math.Cos(t);
            float s = (float)System.Math.Sin(t);
            Vector3 local = axis switch
            {
                0 => new Vector3(0, c, s),
                1 => new Vector3(c, 0, s),
                _ => new Vector3(c, s, 0),
            };
            return Vector3.Transform(local, world);
        }

        private static void Edge(List<SolidLineVertex> vertices, Vector3 a, Vector3 b, Vector4 color)
        {
            vertices.Add(new SolidLineVertex { Position = a, Color = color });
            vertices.Add(new SolidLineVertex { Position = b, Color = color });
        }

        // ---------------------------------------------------------------------
        // Solid (TriangleList) variants — emit filled triangles instead of edges.
        // Used when the caller wants a translucent/opaque filled silhouette
        // (volume body fill, flyby cone visualization, etc.). The output goes
        // into the same SolidLineVertex format and the caller submits with
        // RenderingDrawingLines.Topology.TriangleList.
        // ---------------------------------------------------------------------

        // Unit-cube triangles (12 triangles = 36 vertices). Same vertex layout as
        // AppendWireCube; different topology.
        public static void AppendSolidCube(List<SolidLineVertex> vertices, Matrix4x4 world, Vector4 color)
        {
            var c000 = Vector3.Transform(new Vector3(-1, -1, -1), world);
            var c100 = Vector3.Transform(new Vector3( 1, -1, -1), world);
            var c010 = Vector3.Transform(new Vector3(-1,  1, -1), world);
            var c110 = Vector3.Transform(new Vector3( 1,  1, -1), world);
            var c001 = Vector3.Transform(new Vector3(-1, -1,  1), world);
            var c101 = Vector3.Transform(new Vector3( 1, -1,  1), world);
            var c011 = Vector3.Transform(new Vector3(-1,  1,  1), world);
            var c111 = Vector3.Transform(new Vector3( 1,  1,  1), world);

            // 6 faces × 2 triangles. Winding chosen so that with default back-face
            // culling (CullNone is what RenderingDrawingLines uses anyway) every
            // face faces outwards.
            Tri(vertices, c000, c010, c100, color); Tri(vertices, c100, c010, c110, color); // -Z
            Tri(vertices, c001, c101, c011, color); Tri(vertices, c101, c111, c011, color); // +Z
            Tri(vertices, c000, c001, c010, color); Tri(vertices, c001, c011, c010, color); // -X
            Tri(vertices, c100, c110, c101, color); Tri(vertices, c101, c110, c111, color); // +X
            Tri(vertices, c000, c100, c001, color); Tri(vertices, c100, c101, c001, color); // -Y
            Tri(vertices, c010, c011, c110, color); Tri(vertices, c110, c011, c111, color); // +Y
        }

        // Unit-sphere triangles via UV-sphere tessellation. `latSegments` controls
        // poles-to-equator rings; `longSegments` controls slices around the axis.
        // Default 12×16 = 12*16*2 = 384 triangles, comparable to the legacy _sphere
        // primitive with tessellation 6.
        public static void AppendSolidSphere(List<SolidLineVertex> vertices, Matrix4x4 world, Vector4 color, int latSegments = 12, int longSegments = 16)
        {
            float pi = (float)System.Math.PI;
            float twoPi = 2 * pi;

            for (int lat = 0; lat < latSegments; ++lat)
            {
                float theta1 = (lat       / (float)latSegments) * pi - pi * 0.5f;
                float theta2 = ((lat + 1) / (float)latSegments) * pi - pi * 0.5f;
                float y1 = (float)System.Math.Sin(theta1), r1 = (float)System.Math.Cos(theta1);
                float y2 = (float)System.Math.Sin(theta2), r2 = (float)System.Math.Cos(theta2);

                for (int lon = 0; lon < longSegments; ++lon)
                {
                    float phi1 = (lon       / (float)longSegments) * twoPi;
                    float phi2 = ((lon + 1) / (float)longSegments) * twoPi;
                    float c1 = (float)System.Math.Cos(phi1), s1 = (float)System.Math.Sin(phi1);
                    float c2 = (float)System.Math.Cos(phi2), s2 = (float)System.Math.Sin(phi2);

                    var p11 = Vector3.Transform(new Vector3(r1 * c1, y1, r1 * s1), world);
                    var p21 = Vector3.Transform(new Vector3(r2 * c1, y2, r2 * s1), world);
                    var p12 = Vector3.Transform(new Vector3(r1 * c2, y1, r1 * s2), world);
                    var p22 = Vector3.Transform(new Vector3(r2 * c2, y2, r2 * s2), world);

                    Tri(vertices, p11, p12, p21, color);
                    Tri(vertices, p21, p12, p22, color);
                }
            }
        }

        // Solid cylinder (TriangleList). Axis along Y, base at y=0 unit radius, top at
        // y=1 unit radius. Caller scales for length+radius via the World matrix.
        // Includes side walls and TWO end caps (bottom y=0 fan, top y=1 fan).
        // Used by BaseGizmo for translate/scale axis bars.
        public static void AppendSolidCylinder(List<SolidLineVertex> vertices, Matrix4x4 world, Vector4 color, int segments = 16)
        {
            float twoPi = (float)(2 * System.Math.PI);
            // Pre-compute ring corners.
            var bottomRing = new Vector3[segments];
            var topRing = new Vector3[segments];
            for (int i = 0; i < segments; ++i)
            {
                float t = (i / (float)segments) * twoPi;
                float c = (float)System.Math.Cos(t);
                float s = (float)System.Math.Sin(t);
                bottomRing[i] = Vector3.Transform(new Vector3(c, 0, s), world);
                topRing[i]    = Vector3.Transform(new Vector3(c, 1, s), world);
            }
            var bottomCenter = Vector3.Transform(new Vector3(0, 0, 0), world);
            var topCenter    = Vector3.Transform(new Vector3(0, 1, 0), world);

            for (int i = 0; i < segments; ++i)
            {
                int next = (i + 1) % segments;
                // Side: two triangles per segment forming a quad.
                Tri(vertices, bottomRing[i], topRing[i],   bottomRing[next], color);
                Tri(vertices, bottomRing[next], topRing[i], topRing[next],    color);
                // Bottom cap (winds opposite for outward normal at y=0 facing -Y)
                Tri(vertices, bottomCenter, bottomRing[next], bottomRing[i], color);
                // Top cap (faces +Y)
                Tri(vertices, topCenter, topRing[i], topRing[next], color);
            }
        }

        // Solid torus (TriangleList). Major radius 1 in the XZ plane; `tubeRadius`
        // controls the tube thickness. `majorSegments` = ring tessellation around the
        // major axis; `tubeSegments` = tessellation around the tube. Used for the
        // gizmo rotation rings.
        public static void AppendSolidTorus(List<SolidLineVertex> vertices, Matrix4x4 world, Vector4 color, float tubeRadius, int majorSegments = 48, int tubeSegments = 8)
        {
            float twoPi = (float)(2 * System.Math.PI);
            // Build a 2D grid of vertices on the surface of the torus, then emit
            // triangles per quad. To avoid storing all vertices we generate ring by
            // ring and emit triangles between the previous and current ring.
            Vector3[] prevRing = null;
            Vector3[] firstRing = null;
            for (int j = 0; j <= majorSegments; ++j)
            {
                float u = (j / (float)majorSegments) * twoPi;
                float cu = (float)System.Math.Cos(u);
                float su = (float)System.Math.Sin(u);
                var ring = new Vector3[tubeSegments];
                for (int i = 0; i < tubeSegments; ++i)
                {
                    float v = (i / (float)tubeSegments) * twoPi;
                    float cv = (float)System.Math.Cos(v);
                    float sv = (float)System.Math.Sin(v);
                    // Major ring point + tube offset along (radial, +Y).
                    float r = 1.0f + tubeRadius * cv;
                    float y = tubeRadius * sv;
                    ring[i] = Vector3.Transform(new Vector3(r * cu, y, r * su), world);
                }
                if (j == 0) firstRing = ring;
                if (prevRing != null)
                {
                    for (int i = 0; i < tubeSegments; ++i)
                    {
                        int next = (i + 1) % tubeSegments;
                        Tri(vertices, prevRing[i],  prevRing[next], ring[i],     color);
                        Tri(vertices, prevRing[next], ring[next],   ring[i],     color);
                    }
                }
                prevRing = ring;
            }
        }

        // Solid cone triangles. Apex at origin, base at z=1 with unit radius (matches
        // the legacy GeometricPrimitive.Cone convention). Caller scales via the World
        // matrix. Includes side triangles (apex to base ring) — no base cap (the cone
        // is open at the base, matching the legacy behaviour for visualization).
        public static void AppendSolidCone(List<SolidLineVertex> vertices, Matrix4x4 world, Vector4 color, int segments = 24)
        {
            var apex = Vector3.Transform(Vector3.Zero, world);
            float twoPi = (float)(2 * System.Math.PI);

            Vector3 prev = Vector3.Transform(new Vector3(1, 0, 1), world);
            Vector3 first = prev;
            for (int i = 1; i <= segments; ++i)
            {
                float t = (i / (float)segments) * twoPi;
                Vector3 cur = i == segments
                    ? first
                    : Vector3.Transform(new Vector3((float)System.Math.Cos(t), (float)System.Math.Sin(t), 1), world);
                Tri(vertices, apex, prev, cur, color);
                prev = cur;
            }
        }

        private static void Tri(List<SolidLineVertex> vertices, Vector3 a, Vector3 b, Vector3 c, Vector4 color)
        {
            vertices.Add(new SolidLineVertex { Position = a, Color = color });
            vertices.Add(new SolidLineVertex { Position = b, Color = color });
            vertices.Add(new SolidLineVertex { Position = c, Color = color });
        }
    }
}
