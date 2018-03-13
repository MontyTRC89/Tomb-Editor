using System.Numerics;
using System.Runtime.CompilerServices;

namespace TombLib.Utils
{
    public static class PolygonFunctions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LineIntersectsWithRayFrom00(Vector2 pos0, Vector2 pos1)
        {
            if ((pos1.Y > 0.0f) == (pos0.Y > 0.0f))
                return 0;

            float flt0 = (pos1.Y - pos0.Y) * pos1.X;
            float flt1 = (pos1.X - pos0.X) * pos1.Y;

            if ((pos0.Y > pos1.Y) ? (flt0 < flt1) : (flt0 > flt1))
                return 1;
            return 0;
        }

        public static bool DoesTriangleContainPoint(Vector2 pos0, Vector2 pos1, Vector2 pos2, Vector2 testPos)
        {
            pos0 -= testPos;
            pos1 -= testPos;
            pos2 -= testPos;

            int intersectionCount = LineIntersectsWithRayFrom00(pos0, pos1) + LineIntersectsWithRayFrom00(pos1, pos2) + LineIntersectsWithRayFrom00(pos2, pos0);
            return (intersectionCount & 1) != 0;
        }

        public static bool DoesQuadContainPoint(Vector2 pos0, Vector2 pos1, Vector2 pos2, Vector2 pos3, Vector2 testPos)
        {
            pos0 -= testPos;
            pos1 -= testPos;
            pos2 -= testPos;
            pos3 -= testPos;

            int intersectionCount = LineIntersectsWithRayFrom00(pos0, pos1);
            intersectionCount += LineIntersectsWithRayFrom00(pos1, pos2);
            intersectionCount += LineIntersectsWithRayFrom00(pos2, pos3);
            intersectionCount += LineIntersectsWithRayFrom00(pos3, pos0);
            return (intersectionCount & 1) != 0;
        }
    }
}
