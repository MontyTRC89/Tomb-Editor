using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.Rendering;
using TombLib.Utils;
// Disambiguate BlendMode (rendering vs. TR engine texture blend mode).
using BlendMode = TombLib.Rendering.BlendMode;

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

    // Migrated from the legacy SharpDX.Toolkit + Effect path to RenderingDrawingLines.
    // The gizmo's geometry (translate axes, scale axes, rotation rings, rotation
    // indicator fan) is now built CPU-side per Draw call into the device's dynamic
    // vertex buffer pool — no permanent GeometricPrimitive instances, no Effect
    // parameter dance.
    //
    // Constructor takes a RenderingDevice. Draw takes the active SwapChain + StateBuffer
    // (the panel passes them in). Subclasses are unchanged in behaviour; only their
    // constructor signature changed: (GraphicsDevice device, Effect effect) → (RenderingDevice device).
    public abstract class BaseGizmo : IDisposable
    {
        private const int _rotationTrianglesCount = 64;
        private const float _rotationAlpha = 0.58f;
        private const float _scaleSpeed = 0.0004f;

        private readonly RenderingDevice _device;
        private readonly RenderingDrawingLines _batch;
        // Reused per-frame triangle list. Cleared at the start of each Draw call.
        private readonly List<SolidLineVertex> _vertices = new List<SolidLineVertex>();

        private static readonly Vector4 _xAxisColor = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        private static readonly Vector4 _yAxisColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        private static readonly Vector4 _zAxisColor = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
        private static readonly Vector4 _hoveredAddition = new Vector4(0.6f, 0.6f, 0.6f, 1.0f);
        private static readonly float _arrowHeadOffsetMultiplier = 1.13f;

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

        public BaseGizmo(RenderingDevice device)
        {
            _device = device;
            _batch = device.CreateDrawingLines(new RenderingDrawingLines.Description { Dynamic = true });
        }

        public void Dispose()
        {
            _batch?.Dispose();
        }

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

        // Renders the gizmo into the supplied SwapChain. The state buffer must already
        // hold the view-projection matrix as TransformMatrix; the panel sets it up.
        public void Draw(RenderingSwapChain swapChain, RenderingStateBuffer stateBuffer, Matrix4x4 viewProjection)
        {
            if (!DrawGizmo)
                return;

            bool upside = Orientation == GizmoOrientation.UpsideDown;
            GizmoMode highlight = _mode == GizmoMode.None ? _hoveredMode : _mode;

            var drawRotateMatrixY = _mode == GizmoMode.RotateY ? _frozenRotateMatrixY : RotateMatrixY;
            var drawRotateMatrixX = _mode == GizmoMode.RotateX ? _frozenRotateMatrixX : RotateMatrixX;
            var drawRotateMatrixZ = _mode == GizmoMode.RotateZ ? _frozenRotateMatrixZ : RotateMatrixZ;

            _vertices.Clear();

            // Rotation rings (3 tori around the gizmo center).
            if (SupportRotationX | SupportRotationY | SupportRotationZ)
            {
                float tubeRadius = LineThickness * 0.5f / Size;
                if (SupportRotationY)
                {
                    var world = Matrix4x4.CreateScale(Size * 2.0f) * drawRotateMatrixY * Matrix4x4.CreateTranslation(Position);
                    var color = _yAxisColor + (highlight == GizmoMode.RotateY ? _hoveredAddition : default);
                    WireGeometry.AppendSolidTorus(_vertices, world, color, tubeRadius);
                }
                if (SupportRotationX)
                {
                    var world = Matrix4x4.CreateScale(Size * 2.0f) * Matrix4x4.CreateRotationZ((float)Math.PI / 2.0f) * drawRotateMatrixX * Matrix4x4.CreateTranslation(Position);
                    var color = _xAxisColor + (highlight == GizmoMode.RotateX ? _hoveredAddition : default);
                    WireGeometry.AppendSolidTorus(_vertices, world, color, tubeRadius);
                }
                if (SupportRotationZ)
                {
                    var world = Matrix4x4.CreateScale(Size * 2.0f) * Matrix4x4.CreateRotationX((float)Math.PI / 2.0f) * drawRotateMatrixZ * Matrix4x4.CreateTranslation(Position);
                    var color = _zAxisColor + (highlight == GizmoMode.RotateZ ? _hoveredAddition : default);
                    WireGeometry.AppendSolidTorus(_vertices, world, color, tubeRadius);
                }
            }

            // Scale axes — short cylinders + cube tips.
            if (SupportScale)
            {
                // Scale-axis cylinder length = Size / 2 along Y axis. Legacy used a
                // [-0.5, 0.5] cylinder pre-translated by 0.5; our cylinder is [0, 1]
                // already so no pre-translation is needed.
                float cylRadius = LineThickness * 1.1f * 0.5f;
                float cylLen = (upside ? -Size : Size) / 2.0f;
                AppendAxisCylinder(cylLen, cylRadius, _xAxisColor, highlight, GizmoMode.ScaleX, axisRotZ: -(float)Math.PI / 2.0f);
                AppendAxisCylinder(cylLen, cylRadius, _yAxisColor, highlight, GizmoMode.ScaleY);
                AppendAxisCylinder(cylLen, cylRadius, _zAxisColor, highlight, GizmoMode.ScaleZ, axisRotX: -(float)Math.PI / 2.0f);

                // Scale cube tips.
                AppendScaleCube(Vector3.UnitX,  upside, _xAxisColor, highlight, GizmoMode.ScaleX);
                AppendScaleCube(Vector3.UnitY,  upside, _yAxisColor, highlight, GizmoMode.ScaleY);
                AppendScaleCube(-Vector3.UnitZ, upside, _zAxisColor, highlight, GizmoMode.ScaleZ);
            }

            // Translate axes — long cylinders + cone tips.
            float trCylRadius = LineThickness * 0.5f;
            float trCylLen = (upside ? -Size : Size) * _arrowHeadOffsetMultiplier;
            if (SupportTranslateX)
                AppendAxisCylinder(trCylLen, trCylRadius, _xAxisColor, highlight, GizmoMode.TranslateX, axisRotZ: -(float)Math.PI / 2.0f);
            if (SupportTranslateY)
                AppendAxisCylinder(trCylLen, trCylRadius, _yAxisColor, highlight, GizmoMode.TranslateY);
            if (SupportTranslateZ)
                AppendAxisCylinder(trCylLen, trCylRadius, _zAxisColor, highlight, GizmoMode.TranslateZ, axisRotX: -(float)Math.PI / 2.0f);

            if (SupportTranslateX)
                AppendTranslateCone(Vector3.UnitX,  upside, _xAxisColor, highlight, GizmoMode.TranslateX, axisRotY: (float)(upside ? -Math.PI * 1.5f : -Math.PI * 0.5f));
            if (SupportTranslateY)
                AppendTranslateCone(Vector3.UnitY,  upside, _yAxisColor, highlight, GizmoMode.TranslateY, axisRotX: (float)(upside ? Math.PI * 1.5f : Math.PI * 0.5f));
            if (SupportTranslateZ)
                AppendTranslateCone(-Vector3.UnitZ, upside, _zAxisColor, highlight, GizmoMode.TranslateZ, axisRotY: (float)(upside ? Math.PI : 0.0f));

            // Submit gizmo body — opaque triangles, depth-tested.
            if (_vertices.Count > 0)
            {
                _batch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_vertices));
                _batch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = swapChain,
                    StateBuffer = stateBuffer,
                    Topology = RenderingDrawingLines.Topology.TriangleList,
                });
            }

            // Active rotation indicator: a fan of triangles from the rotation center
            // covering the angle between the pick start and the current cursor angle.
            switch (_mode)
            {
                case GizmoMode.RotateY:
                case GizmoMode.RotateX:
                case GizmoMode.RotateZ:
                    DrawRotationIndicator(swapChain, stateBuffer);
                    break;
            }
        }

        private void AppendAxisCylinder(float length, float radius, Vector4 color, GizmoMode highlight, GizmoMode myMode, float axisRotZ = 0, float axisRotX = 0)
        {
            var c = color + (highlight == myMode ? _hoveredAddition : default);
            // Cylinder is along +Y in [0, 1]. Scale Y by length, X/Z by radius. Then
            // optional rotation to point along +X (RotZ -PI/2) or -Z (RotX -PI/2).
            // Finally translate to gizmo Position.
            var world = Matrix4x4.CreateScale(radius, length, radius) *
                        (axisRotZ != 0 ? Matrix4x4.CreateRotationZ(axisRotZ) : Matrix4x4.Identity) *
                        (axisRotX != 0 ? Matrix4x4.CreateRotationX(axisRotX) : Matrix4x4.Identity) *
                        Matrix4x4.CreateTranslation(Position);
            WireGeometry.AppendSolidCylinder(_vertices, world, c, segments: 8);
        }

        private void AppendScaleCube(Vector3 axis, bool upside, Vector4 color, GizmoMode highlight, GizmoMode myMode)
        {
            var c = color + (highlight == myMode ? _hoveredAddition : default);
            var world = Matrix4x4.CreateScale(ScaleCubeSize) *
                        Matrix4x4.CreateTranslation(Position + axis * (upside ? -Size : Size) / 2.0f);
            WireGeometry.AppendSolidCube(_vertices, world, c);
        }

        private void AppendTranslateCone(Vector3 axis, bool upside, Vector4 color, GizmoMode highlight, GizmoMode myMode, float axisRotY = 0, float axisRotX = 0)
        {
            var c = color + (highlight == myMode ? _hoveredAddition : default);
            // Cone primitive (apex at origin, base at z=1 radius 1) with scale = TranslationConeSize.
            // Position the cone's APEX at the end of the translate axis; the cone
            // points outwards along +Z post-rotation.
            var basePos = Position + (axis + axis * 0.1f) * ((upside ? -Size : Size) * _arrowHeadOffsetMultiplier);
            var world = (axisRotY != 0 ? Matrix4x4.CreateRotationY(axisRotY) : Matrix4x4.Identity) *
                        (axisRotX != 0 ? Matrix4x4.CreateRotationX(axisRotX) : Matrix4x4.Identity) *
                        Matrix4x4.CreateScale(TranslationConeSize) *
                        Matrix4x4.CreateTranslation(basePos);
            WireGeometry.AppendSolidCone(_vertices, world, c);
        }

        private void DrawRotationIndicator(RenderingSwapChain swapChain, RenderingStateBuffer stateBuffer)
        {
            float startAngle;
            float endAngle;
            float lastMouseAngle;
            Matrix4x4 baseMatrix;
            Vector4 color;
            switch (_mode)
            {
                case GizmoMode.RotateY:
                    startAngle = _rotationPickAngle;
                    endAngle = _rotationLastMouseAngle;
                    lastMouseAngle = _rotationLastMouseAngle;
                    baseMatrix = _frozenRotateMatrixY;
                    color = _yAxisColor;
                    break;
                case GizmoMode.RotateX:
                    startAngle = -((float)Math.PI * 0.5f) - _rotationPickAngle;
                    endAngle = -((float)Math.PI * 0.5f) - _rotationLastMouseAngle;
                    lastMouseAngle = -((float)Math.PI * 0.5f) - _rotationLastMouseAngle;
                    baseMatrix = Matrix4x4.CreateRotationZ((float)Math.PI / 2.0f) * _frozenRotateMatrixX;
                    color = _xAxisColor;
                    break;
                case GizmoMode.RotateZ:
                    startAngle = (float)Math.PI + _rotationPickAngle;
                    endAngle = (float)Math.PI + _rotationLastMouseAngle;
                    lastMouseAngle = (float)Math.PI + _rotationLastMouseAngle;
                    baseMatrix = Matrix4x4.CreateRotationX((float)Math.PI / 2.0f) * _frozenRotateMatrixZ;
                    color = _zAxisColor;
                    break;
                default:
                    return;
            }
            // Choose shortest path
            float shortestAngle = endAngle - startAngle;
            shortestAngle = (float)(shortestAngle - Math.Round(shortestAngle / (Math.PI * 2)) * (Math.PI * 2));
            endAngle = startAngle + shortestAngle;
            if (startAngle > endAngle)
                (startAngle, endAngle) = (endAngle, startAngle);

            var world = Matrix4x4.CreateScale(Size) * baseMatrix * Matrix4x4.CreateTranslation(Position);
            float angleStep = (endAngle - startAngle) / _rotationTrianglesCount;

            // Triangle fan from origin to ring slice.
            var fan = new List<SolidLineVertex>(_rotationTrianglesCount * 3);
            var middle = Vector3.Transform(new Vector3(), world);
            var lastP = Vector3.Transform(new Vector3((float)Math.Cos(startAngle), 0, (float)-Math.Sin(startAngle)), world);
            var fanColor = color * _rotationAlpha;
            for (int i = 0; i < _rotationTrianglesCount; ++i)
            {
                float currentAngle = startAngle + (i + 1) * angleStep;
                var currentP = Vector3.Transform(new Vector3((float)Math.Cos(currentAngle), 0, (float)-Math.Sin(currentAngle)), world);
                fan.Add(new SolidLineVertex { Position = middle,   Color = fanColor });
                fan.Add(new SolidLineVertex { Position = lastP,    Color = fanColor });
                fan.Add(new SolidLineVertex { Position = currentP, Color = fanColor });
                lastP = currentP;
            }

            _batch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(fan));
            _batch.Render(new RenderingDrawingLines.RenderArgs
            {
                RenderTarget = swapChain,
                StateBuffer = stateBuffer,
                Topology = RenderingDrawingLines.Topology.TriangleList,
                Blend = BlendMode.NonPremultipliedAlpha,
            });

            // Pointer line from center to current mouse position.
            var pointerP = Vector3.Transform(new Vector3(
                _rotationLastMouseRadius / Size * (float)Math.Cos(lastMouseAngle), 0,
                _rotationLastMouseRadius / Size * (float)-Math.Sin(lastMouseAngle)), world);
            var line = new[]
            {
                new SolidLineVertex { Position = middle,   Color = Vector4.One },
                new SolidLineVertex { Position = pointerP, Color = Vector4.One },
            };
            _batch.SetVertices(line);
            _batch.Render(new RenderingDrawingLines.RenderArgs
            {
                RenderTarget = swapChain,
                StateBuffer = stateBuffer,
                Topology = RenderingDrawingLines.Topology.LineList,
            });
        }

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

        public bool DrawGizmo
        {
            get
            {
                if (Mode == GizmoMode.RotateY || Mode == GizmoMode.RotateX || Mode == GizmoMode.RotateZ)
                    return SupportRotationY || SupportRotationX || SupportRotationZ;
                return true;
            }
        }

        public GizmoMode Mode => _mode;
    }
}
