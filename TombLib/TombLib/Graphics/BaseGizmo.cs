using System;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.Graphics
{
    public enum GizmoOrientation : byte
    {
        Normal,
        UpsideDown
    }

    public enum GizmoMode : byte
    {
        None,
        TranslateX,
        TranslateY,
        TranslateZ,
        RotateZ,
        RotateX,
        RotateY,
        ScaleX,
        ScaleY,
        ScaleZ
    }

    public class PickingResultGizmo : PickingResult
    {
        internal float RotationPickAngle { get; set; }
        internal float RotationPickDistance { get; set; }
        internal GizmoMode Mode { get; set; }

        internal PickingResultGizmo(GizmoMode mode)
        {
            Mode = mode;
        }
        internal PickingResultGizmo(GizmoMode mode, float rotationPickAngle, float rotationPickDistance)
        {
            Mode = mode;
            RotationPickAngle = rotationPickAngle;
            RotationPickDistance = rotationPickDistance;
        }
    }

    /// <summary>
    /// Headless picking + drag-math core for the editor transform gizmo. The
    /// actual visuals are drawn by the V2 <c>GizmoRenderer</c>, which reads
    /// <see cref="GetPublicState"/> at the start of each render pass. This
    /// class owns no GPU resources — concrete derivations only override the
    /// abstract callbacks below to write the picked transform back into their
    /// editor state.
    /// </summary>
    public abstract class BaseGizmo : IDisposable
    {
        private const float _scaleSpeed              = 0.0004f;
        private const float _arrowHeadOffsetMultiplier = 1.13f;

        private GizmoMode _mode;
        private Vector3 _scaleBase;
        private Vector3 _initialPosition;
        private float _rotationLastMouseAngle;
        private float _rotationLastMouseRadius;
        private float _rotationPickAngle;
        private float _rotationPickAngleOffset;

        private Matrix4x4 _frozenRotateMatrixY;
        private Matrix4x4 _frozenRotateMatrixX;
        private Matrix4x4 _frozenRotateMatrixZ;

        private GizmoMode _hoveredMode;

        protected BaseGizmo() { }

        /// <summary>
        /// Kept for binary compatibility with existing
        /// <c>_gizmo?.Dispose()</c> call sites — the gizmo no longer owns
        /// any unmanaged resources of its own.
        /// </summary>
        public void Dispose() { }

        private static bool ConstructPlaneIntersection(Vector3 Position, Matrix4x4 viewProjection, Ray ray, Vector3 perpendicularVector0, Vector3 perpendicularVector1, out Vector3 intersection)
        {
            // Choose the perpendicular plane that is more parallel to the camera plane to
            // maximize the available accuracy in the view space.
            Vector3 viewDirection = new Vector3(viewProjection.M31, viewProjection.M32, viewProjection.M33);
            float perpendicularVector0Dot = Math.Abs(Vector3.Dot(viewDirection, perpendicularVector0));
            float perpendicularVector1Dot = Math.Abs(Vector3.Dot(viewDirection, perpendicularVector1));
            Plane plane = MathC.CreatePlaneAtPoint(Position, perpendicularVector0Dot > perpendicularVector1Dot ? perpendicularVector0 : perpendicularVector1);

            // Construct intersection
            return Collision.RayIntersectsPlane(ray, plane, out intersection);
        }

        private static float SimplifyAngle(float angle)
        {
            return (float)(angle - Math.Round(angle / (2 * Math.PI)) * (2 * Math.PI));
        }

        /// <returns>true, if an iteraction with the gizmo is happening</returns>
        public bool MouseMoved(Matrix4x4 viewProjection, Ray ray)
        {
            if (!DrawGizmo || _mode == GizmoMode.None)
                return false;

            // Init movement variables
            bool upside = Orientation == GizmoOrientation.UpsideDown;
            var arrowShift = (upside ? -Size : Size) * _arrowHeadOffsetMultiplier;

            // Flip sizing dimensions if object is rotateable on Y axis
            bool flippedScale = false;
            if ((_mode == GizmoMode.ScaleX || _mode == GizmoMode.ScaleZ) && SupportRotationY)
                flippedScale = MathC.RadToDeg(RotationY) % 180.0f >= 45.0f;

            // First get the ray in 3D space from X, Y mouse coordinates
            switch (_mode)
            {
                case GizmoMode.TranslateX:
                    {
                        Vector3 intersection;
                        if (ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitY, Vector3.UnitZ, out intersection))
                        {
                            var delta = _initialPosition.X - arrowShift;
                            GizmoMove(new Vector3(intersection.X - arrowShift - delta, Position.Y, Position.Z));
                            GizmoMoveDelta(new Vector3(intersection.X - arrowShift - delta - Position.X, 0.0f, 0.0f));
                        }
                    }
                    break;
                case GizmoMode.TranslateY:
                    {
                        Vector3 intersection;
                        if (ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitZ, out intersection))
                        {
                            var delta = _initialPosition.Y - arrowShift;
                            GizmoMove(new Vector3(Position.X, intersection.Y - arrowShift - delta, Position.Z));
                            GizmoMoveDelta(new Vector3(0.0f, intersection.Y - arrowShift - delta - Position.Y, 0.0f));
                        }
                    }
                    break;
                case GizmoMode.TranslateZ:
                    {
                        Vector3 intersection;
                        if (ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitY, out intersection))
                        {
                            var delta = _initialPosition.Z - arrowShift;
                            GizmoMove(new Vector3(Position.X, Position.Y, intersection.Z - arrowShift - delta));
                            GizmoMoveDelta(new Vector3(0.0f, 0.0f, intersection.Z - arrowShift - delta - Position.Z));
                        }
                    }
                    break;
                case GizmoMode.ScaleX:
                    {
                        Vector3 intersection;
                        if (ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitY, Vector3.UnitZ, out intersection))
                        {
                            var delta = _initialPosition.X - arrowShift;
                            var offset = intersection.X - arrowShift - delta - Position.X;

                            if (flippedScale)
                                GizmoScaleZ(_scaleBase.Z * (float)Math.Exp(_scaleSpeed * offset));
                            else
                                GizmoScaleX(_scaleBase.X * (float)Math.Exp(_scaleSpeed * offset));
                        }
                    }
                    break;
                case GizmoMode.ScaleY:
                    {
                        Vector3 intersection;
                        if (ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitZ, out intersection))
                        {
                            var delta = _initialPosition.Y - arrowShift;
                            var offset = intersection.Y - arrowShift - delta - Position.Y;

                            GizmoScaleY(_scaleBase.Y * (float)Math.Exp(_scaleSpeed * offset));
                        }
                    }
                    break;
                case GizmoMode.ScaleZ:
                    {
                        Vector3 intersection;
                        if (ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitY, out intersection))
                        {
                            var delta = _initialPosition.Z - arrowShift;
                            var offset = -(intersection.Z - arrowShift - delta - Position.Z);

                            if (flippedScale)
                                GizmoScaleX(_scaleBase.X * (float)Math.Exp(_scaleSpeed * offset));
                            else
                                GizmoScaleZ(_scaleBase.Z * (float)Math.Exp(_scaleSpeed * offset));
                        }
                    }
                    break;
                case GizmoMode.RotateY:
                    {
                        Plane rotationPlane = MathC.CreatePlaneAtPoint(Position, MathC.HomogenousTransform(Vector3.UnitY, _frozenRotateMatrixY));
                        Vector3 rotationIntersection;
                        if (Collision.RayIntersectsPlane(ray, rotationPlane, out rotationIntersection))
                        {
                            Vector3 direction = rotationIntersection - Position;
                            _rotationLastMouseRadius = direction.Length();
                            direction = Vector3.Normalize(direction);

                            float sin = Vector3.Dot(Vector3.UnitZ, direction);
                            float cos = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitZ, direction));
                            _rotationLastMouseAngle = (float)Math.Atan2(-sin, cos);
                            GizmoRotateY(SimplifyAngle(_rotationPickAngleOffset + _rotationLastMouseAngle));
                        }
                    }
                    break;
                case GizmoMode.RotateX:
                    {
                        Plane rotationPlane = MathC.CreatePlaneAtPoint(Position, MathC.HomogenousTransform(Vector3.UnitX, _frozenRotateMatrixX));
                        Vector3 rotationIntersection;
                        if (Collision.RayIntersectsPlane(ray, rotationPlane, out rotationIntersection))
                        {
                            Vector3 direction = rotationIntersection - Position;
                            _rotationLastMouseRadius = direction.Length();
                            direction = Vector3.Normalize(direction);

                            float sin = Vector3.Dot(Vector3.UnitY, direction);
                            float cos = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitY, direction));
                            _rotationLastMouseAngle = (float)Math.Atan2(-sin, cos);
                            GizmoRotateX(SimplifyAngle(_rotationPickAngleOffset + _rotationLastMouseAngle));
                        }
                    }
                    break;
                case GizmoMode.RotateZ:
                    {
                        Plane rotationPlane = MathC.CreatePlaneAtPoint(Position, MathC.HomogenousTransform(Vector3.UnitZ, _frozenRotateMatrixZ));
                        Vector3 rotationIntersection;
                        if (Collision.RayIntersectsPlane(ray, rotationPlane, out rotationIntersection))
                        {

                            Vector3 direction = rotationIntersection - Position;
                            _rotationLastMouseRadius = direction.Length();
                            direction = Vector3.Normalize(direction);

                            float sin = Vector3.Dot(Vector3.UnitY, direction);
                            float cos = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitY, direction));
                            _rotationLastMouseAngle = (float)Math.Atan2(-sin, cos);
                            GizmoRotateZ(SimplifyAngle(_rotationPickAngleOffset + _rotationLastMouseAngle));
                        }
                    }
                    break;
            }

            return true;
        }

        /// <returns>If the parent should be redrawn</returns>
        public bool MouseUp()
        {
            GizmoMode oldMode = _mode;
            _mode = GizmoMode.None;
            return oldMode != GizmoMode.None;
        }

        private void CalculateInitialPosition(Vector3 pickPosition)
        {
            if (_mode == GizmoMode.None)
                _initialPosition = pickPosition - Position;
        }

        public PickingResultGizmo DoPicking(Ray ray)
        {
            if (!DrawGizmo)
                return null;

            // Init movement variables
            var pickPos = Vector3.Zero;
            var upsideSize = Orientation == GizmoOrientation.UpsideDown ? -Size : Size;
            var arrowShift = upsideSize * _arrowHeadOffsetMultiplier;
            var scaleShift = upsideSize  / 2.0f;

            // Check for translation
            if (SupportTranslateX)
            {
                BoundingSphere sphereX = new BoundingSphere(Position + Vector3.UnitX * arrowShift, TranslationConeSize / 1.5f);
                if (Collision.RayIntersectsSphere(ray, sphereX, out pickPos))
                {
                    CalculateInitialPosition(pickPos);
                    return new PickingResultGizmo(GizmoMode.TranslateX);
                }
            }
            if (SupportTranslateY)
            {
                BoundingSphere sphereY = new BoundingSphere(Position + Vector3.UnitY * arrowShift, TranslationConeSize / 1.5f);
                if (Collision.RayIntersectsSphere(ray, sphereY, out pickPos))
                {
                    CalculateInitialPosition(pickPos);
                    return new PickingResultGizmo(GizmoMode.TranslateY);
                }
            }
            if (SupportTranslateZ)
            {
                BoundingSphere sphereZ = new BoundingSphere(Position - Vector3.UnitZ * arrowShift, TranslationConeSize / 1.5f);
                if (Collision.RayIntersectsSphere(ray, sphereZ, out pickPos))
                {
                    CalculateInitialPosition(pickPos);
                    return new PickingResultGizmo(GizmoMode.TranslateZ);
                }
            }

            // Check for scale
            if (SupportScale)
            {
                BoundingBox scaleX = new BoundingBox(Position + Vector3.UnitX * scaleShift - new Vector3(ScaleCubeSize / 2.0f),
                                                     Position + Vector3.UnitX * scaleShift + new Vector3(ScaleCubeSize / 2.0f));
                if (Collision.RayIntersectsBox(ray, scaleX, out pickPos))
                {
                    CalculateInitialPosition(pickPos);
                    return new PickingResultGizmo(GizmoMode.ScaleX);
                }

                BoundingBox scaleY = new BoundingBox(Position + Vector3.UnitY * scaleShift - new Vector3(ScaleCubeSize / 2.0f),
                                                     Position + Vector3.UnitY * scaleShift + new Vector3(ScaleCubeSize / 2.0f));
                if (Collision.RayIntersectsBox(ray, scaleY, out pickPos))
                {
                    CalculateInitialPosition(pickPos);
                    return new PickingResultGizmo(GizmoMode.ScaleY);
                }

                BoundingBox scaleZ = new BoundingBox(Position - Vector3.UnitZ * scaleShift - new Vector3(ScaleCubeSize / 2.0f),
                                                     Position - Vector3.UnitZ * scaleShift + new Vector3(ScaleCubeSize / 2.0f));
                if (Collision.RayIntersectsBox(ray, scaleZ, out pickPos))
                {
                    CalculateInitialPosition(pickPos);
                    return new PickingResultGizmo(GizmoMode.ScaleZ);
                }
            }

            // Check for rotation
            float pickRadius = LineThickness / 2 + (Size * 0.045f);

            if (SupportRotationZ)
            {
                Plane planeZ = MathC.CreatePlaneAtPoint(Position, MathC.HomogenousTransform(Vector3.UnitZ, RotateMatrixZ));
                Vector3 intersectionPoint;
                if (Collision.RayIntersectsPlane(ray, planeZ, out intersectionPoint))
                {
                    var distance = (intersectionPoint - Position).Length();
                    if (distance >= Size - pickRadius && distance <= Size + pickRadius)
                    {
                        Vector3 startDirection = Vector3.Normalize(intersectionPoint - Position);

                        float sin = Vector3.Dot(Vector3.UnitY, startDirection);
                        float cos = Vector3.Dot(planeZ.Normal, Vector3.Cross(Vector3.UnitY, startDirection));
                        return new PickingResultGizmo(GizmoMode.RotateZ, (float)Math.Atan2(-sin, cos), distance);
                    }
                }
            }

            if (SupportRotationX)
            {
                Plane planeX = MathC.CreatePlaneAtPoint(Position, MathC.HomogenousTransform(Vector3.UnitX, RotateMatrixX));
                Vector3 intersectionPoint;
                if (Collision.RayIntersectsPlane(ray, planeX, out intersectionPoint))
                {
                    var distance = (intersectionPoint - Position).Length();
                    if (distance >= Size - pickRadius && distance <= Size + pickRadius)
                    {
                        Vector3 startDirection = Vector3.Normalize(intersectionPoint - Position);

                        float sin = Vector3.Dot(Vector3.UnitY, startDirection);
                        float cos = Vector3.Dot(planeX.Normal, Vector3.Cross(Vector3.UnitY, startDirection));
                        return new PickingResultGizmo(GizmoMode.RotateX, (float)Math.Atan2(-sin, cos), distance);
                    }
                }
            }

            if (SupportRotationY)
            {
                Plane planeY = MathC.CreatePlaneAtPoint(Position, MathC.HomogenousTransform(Vector3.UnitY, RotateMatrixY));
                Vector3 intersectionPoint;
                if (Collision.RayIntersectsPlane(ray, planeY, out intersectionPoint))
                {
                    var distance = (intersectionPoint - Position).Length();
                    if (distance >= Size - pickRadius && distance <= Size + pickRadius)
                    {
                        Vector3 startDirection = Vector3.Normalize(intersectionPoint - Position);

                        float sin = Vector3.Dot(Vector3.UnitZ, startDirection);
                        float cos = Vector3.Dot(planeY.Normal, Vector3.Cross(Vector3.UnitZ, startDirection));
                        return new PickingResultGizmo(GizmoMode.RotateY, (float)Math.Atan2(-sin, cos), distance);
                    }
                }
            }

            return null;
        }

        public void ActivateGizmo(PickingResultGizmo pickingResult)
        {
            _mode = pickingResult.Mode;

            _scaleBase = SupportScale ? Scale : Vector3.One;
            _rotationPickAngle = SimplifyAngle(pickingResult.RotationPickAngle);
            _rotationPickAngleOffset = SimplifyAngle(
                (pickingResult.Mode == GizmoMode.RotateY ? RotationY :
                 pickingResult.Mode == GizmoMode.RotateX ? RotationX :
                 pickingResult.Mode == GizmoMode.RotateZ ? RotationZ : 0.0f) - pickingResult.RotationPickAngle);
            _rotationLastMouseAngle = SimplifyAngle(pickingResult.RotationPickAngle);
            _rotationLastMouseRadius = pickingResult.Distance;

            _frozenRotateMatrixY = RotateMatrixY;
            _frozenRotateMatrixX = RotateMatrixX;
            _frozenRotateMatrixZ = RotateMatrixZ;
        }

        /// <returns>If the parent should be redrawn</returns>
        public bool GizmoUpdateHoverEffect(PickingResultGizmo pickingResult)
        {
            GizmoMode oldHoveredMode = _hoveredMode;

            // Disable hover mode
            if (pickingResult == null)
            {
                _hoveredMode = GizmoMode.None;
                return oldHoveredMode != _hoveredMode;
            }

            // Set hover mode
            _hoveredMode = pickingResult.Mode;
            return oldHoveredMode != _hoveredMode;
        }

        private Matrix4x4 RotateMatrixY => Matrix4x4.Identity;
        private Matrix4x4 RotateMatrixX => RotateMatrixY * Matrix4x4.CreateRotationY(SupportRotationY ? RotationY : 0.0f);
        private Matrix4x4 RotateMatrixZ => RotateMatrixX * Matrix4x4.CreateRotationX(SupportRotationX ? RotationX : 0.0f);

        // They are called both, just leave the implementation empty if not needed, don't use exceptions
        protected abstract void GizmoMove(Vector3 newPos);
        protected abstract void GizmoMoveDelta(Vector3 delta);
        protected abstract void GizmoScaleY(float newScale);
        protected abstract void GizmoScaleX(float newScale);
        protected abstract void GizmoScaleZ(float newScale);
        protected abstract void GizmoRotateY(float newAngle);
        protected abstract void GizmoRotateX(float newAngle);
        protected abstract void GizmoRotateZ(float newAngle);

        protected abstract Vector3 Position { get; }
        protected abstract float RotationY { get; }
        protected abstract float RotationX { get; }
        protected abstract float RotationZ { get; }
        protected abstract Vector3 Scale { get; }
        protected abstract float CentreCubeSize { get; }
        protected abstract float TranslationConeSize { get; }
        protected abstract float ScaleCubeSize { get; }
        protected abstract float Size { get; }
        protected abstract float LineThickness { get; }
        protected abstract GizmoOrientation Orientation { get; }
        protected abstract bool SupportTranslateX { get; }
        protected abstract bool SupportTranslateY { get; }
        protected abstract bool SupportTranslateZ { get; }
        protected abstract bool SupportScale { get; }
        protected abstract bool SupportRotationY { get; }
        protected abstract bool SupportRotationX { get; }
        protected abstract bool SupportRotationZ { get; }
        protected virtual bool DrawGizmo => SupportTranslateX || SupportTranslateY || SupportTranslateZ || SupportScale || SupportRotationY || SupportRotationX || SupportRotationZ;

        /// <summary>
        /// Read-only snapshot of the gizmo's internal state for the V2
        /// renderer. The V2 <c>GizmoRenderer</c> reads this at the start of
        /// every render pass and uses it to rebuild the gizmo geometry.
        /// </summary>
        public readonly struct PublicState
        {
            public readonly Vector3 Position;
            public readonly float   Size;
            public readonly float   CentreCubeSize;
            public readonly float   TranslationConeSize;
            public readonly float   ScaleCubeSize;
            public readonly float   LineThickness;
            public readonly GizmoOrientation Orientation;
            public readonly bool    SupportTranslateX, SupportTranslateY, SupportTranslateZ;
            public readonly bool    SupportScale;
            public readonly bool    SupportRotationX, SupportRotationY, SupportRotationZ;
            public readonly float   RotationX, RotationY, RotationZ;
            public readonly Vector3 Scale;
            public readonly GizmoMode ActiveMode;
            public readonly GizmoMode HoveredMode;
            public readonly bool    DrawGizmo;

            // Rotation matrices applied to each ring. When ActiveMode is the
            // matching RotateX/Y/Z the renderer should use the frozen matrix
            // instead of the live one, so the ring stays visually pinned
            // while the user drags.
            public readonly Matrix4x4 RotateMatrixX, RotateMatrixY, RotateMatrixZ;
            public readonly Matrix4x4 FrozenRotateMatrixX, FrozenRotateMatrixY, FrozenRotateMatrixZ;

            // Rotation pie helper (only meaningful while ActiveMode is a Rotate*).
            public readonly float RotationPickAngle;
            public readonly float RotationLastMouseAngle;
            public readonly float RotationLastMouseRadius;

            internal PublicState(BaseGizmo g)
            {
                Position             = g.Position;
                Size                 = g.Size;
                CentreCubeSize       = g.CentreCubeSize;
                TranslationConeSize  = g.TranslationConeSize;
                ScaleCubeSize        = g.ScaleCubeSize;
                LineThickness        = g.LineThickness;
                Orientation          = g.Orientation;
                SupportTranslateX    = g.SupportTranslateX;
                SupportTranslateY    = g.SupportTranslateY;
                SupportTranslateZ    = g.SupportTranslateZ;
                SupportScale         = g.SupportScale;
                SupportRotationX     = g.SupportRotationX;
                SupportRotationY     = g.SupportRotationY;
                SupportRotationZ     = g.SupportRotationZ;
                // Reads of g.RotationX / RotationY / RotationZ on the
                // concrete gizmo cast to IRotateableY[X[Roll]]. Skipping the
                // cast when the axis isn't supported matches what
                // RotateMatrix* do internally — otherwise selecting a
                // StaticInstance (IRotateableY only) would throw on the
                // RotationX/Z reads here.
                RotationY            = g.SupportRotationY ? g.RotationY : 0.0f;
                RotationX            = g.SupportRotationX ? g.RotationX : 0.0f;
                RotationZ            = g.SupportRotationZ ? g.RotationZ : 0.0f;
                Scale                = g.SupportScale ? g.Scale : Vector3.One;
                ActiveMode           = g._mode;
                HoveredMode          = g._hoveredMode;
                DrawGizmo            = g.DrawGizmo;

                RotateMatrixY        = g.RotateMatrixY;
                RotateMatrixX        = g.RotateMatrixX;
                RotateMatrixZ        = g.RotateMatrixZ;
                FrozenRotateMatrixY  = g._frozenRotateMatrixY;
                FrozenRotateMatrixX  = g._frozenRotateMatrixX;
                FrozenRotateMatrixZ  = g._frozenRotateMatrixZ;
                RotationPickAngle       = g._rotationPickAngle;
                RotationLastMouseAngle  = g._rotationLastMouseAngle;
                RotationLastMouseRadius = g._rotationLastMouseRadius;
            }
        }

        public PublicState GetPublicState() => new PublicState(this);
    }
}
