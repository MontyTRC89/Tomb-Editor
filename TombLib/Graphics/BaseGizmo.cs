using SharpDX;
using SharpDX.Toolkit.Graphics;
using System.Windows.Forms;
using System;
using Buffer = SharpDX.Toolkit.Graphics;

namespace TombLib.Graphics
{
    internal enum GizmoMode : byte
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

    public abstract class BaseGizmo : IDisposable
    {
        private const int _rotationTrianglesCount = 64;
        private const int _lineRadiusTesselation = 8;
        private const float _rotationAlpha = 0.58f;
        private const float _scaleSpeed = 0.0004f;

        private readonly RasterizerState _rasterizerWireframe;

        private readonly Effect _effect;

        // Geometry of the gizmo
        private readonly GraphicsDevice _device;
        private Buffer<SolidVertex> _rotationHelperGeometry;
        private readonly GeometricPrimitive _cylinder;
        private readonly GeometricPrimitive _sphere;
        private readonly GeometricPrimitive _cube;
        private readonly GeometricPrimitive _cone;
        private GeometricPrimitive _torus;
        private float _torusRadius = float.MinValue;
        private static readonly Color4 _xAxisColor = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
        private static readonly Color4 _yAxisColor = new Color4(0.0f, 1.0f, 0.0f, 1.0f);
        private static readonly Color4 _zAxisColor = new Color4(0.0f, 0.0f, 1.0f, 1.0f);
        private static readonly Color4 _centerColor = new Color4(1.0f, 1.0f, 0.0f, 1.0f);
        private static readonly Color4 _hoveredAddition = new Color4(0.6f, 0.6f, 0.6f, 1.0f);
        private static readonly float _hoveredAdditionGreensCorrection = 1.5f;

        private GizmoMode _mode;
        private float _scaleBase;
        private float _rotationLastMouseAngle;
        private float _rotationLastMouseRadius;
        private float _rotationPickAngle;
        private float _rotationPickAngleOffset;

        private GizmoMode _hoveredMode;

        public BaseGizmo(GraphicsDevice device, Effect effect)
        {
            _effect = effect;
            _device = device;

            // Create the gizmo geometry
            _rotationHelperGeometry = Buffer<SolidVertex>.Vertex.New<SolidVertex>(device, _rotationTrianglesCount * 3 + 2);
            _cylinder = GeometricPrimitive.Cylinder.New(_device, 1.0f, 1.0f, _lineRadiusTesselation);
            _sphere = GeometricPrimitive.Sphere.New(_device, 1.0f, 16);
            _cube = GeometricPrimitive.Cube.New(_device, 1.0f);
            _cone = GeometricPrimitive.Cone.New(_device, 1.0f, 1.3f, 16);

            // Create the rasterizer state for wireframe drawing
            var renderStateDesc = new SharpDX.Direct3D11.RasterizerStateDescription
            {
                CullMode = SharpDX.Direct3D11.CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 0,
                FillMode = SharpDX.Direct3D11.FillMode.Wireframe,
                IsAntialiasedLineEnabled = true,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0
            };
            _rasterizerWireframe = RasterizerState.New(_device, renderStateDesc);
        }

        public void Dispose()
        {
            _rasterizerWireframe?.Dispose();
            _rotationHelperGeometry?.Dispose();
            _cylinder?.Dispose();
            _sphere?.Dispose();
            _cube?.Dispose();
            _cone?.Dispose();
            _torus?.Dispose();
        }

        private static Vector3 ConstructPlaneIntersection(Vector3 Position, Matrix viewProjection, Ray ray, Vector3 perpendicularVector0, Vector3 perpendicularVector1)
        {
            // Choose the perpendicular plane that is more parallel to the camera plane to
            // maximize the available accuracy in the view space.
            Vector3 viewDirection = new Vector3(viewProjection.Row3.X, viewProjection.Row3.Y, viewProjection.Row3.Z);
            float perpendicularVector0Dot = Math.Abs(Vector3.Dot(viewDirection, perpendicularVector0));
            float perpendicularVector1Dot = Math.Abs(Vector3.Dot(viewDirection, perpendicularVector1));
            Plane plane = new Plane(Position, perpendicularVector0Dot > perpendicularVector1Dot ? perpendicularVector0 : perpendicularVector1);

            // Construct intersection
            Vector3 intersection;
            ray.Intersects(ref plane, out intersection);
            return intersection;
        }

        private static float SimplifyAngle(float angle)
        {
            return (float)(angle - Math.Round(angle / (2 * Math.PI)) * (2 * Math.PI));
        }

        /// <returns>true, if an iteraction with the gizmo is happening</returns>
        public bool MouseMoved(Matrix viewProjection, int x, int y)
        {
            if ((!DrawGizmo) || (_mode == GizmoMode.None))
                return false;

            // First get the ray in 3D space from X, Y mouse coordinates
            Ray ray = Ray.GetPickRay(x, y, _device.Viewport, viewProjection);
            switch (_mode)
            {
                case GizmoMode.TranslateX:
                    {
                        Vector3 intersection = ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitY, Vector3.UnitZ);
                        GizmoMove(new Vector3(intersection.X - Size, Position.Y, Position.Z));
                    }
                    break;
                case GizmoMode.TranslateY:
                    {
                        Vector3 intersection = ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitZ);
                        GizmoMove(new Vector3(Position.X, intersection.Y - Size, Position.Z));
                    }
                    break;
                case GizmoMode.TranslateZ:
                    {
                        Vector3 intersection = ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitY);
                        GizmoMove(new Vector3(Position.X, Position.Y, intersection.Z + Size));
                    }
                    break;
                case GizmoMode.ScaleX:
                    {
                        Vector3 intersection = ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitY, Vector3.UnitZ);
                        GizmoScale(_scaleBase * (float)Math.Exp(_scaleSpeed * (intersection.X - Position.X)));
                    }
                    break;
                case GizmoMode.ScaleY:
                    {
                        Vector3 intersection = ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitZ);
                        GizmoScale(_scaleBase * (float)Math.Exp(_scaleSpeed * (intersection.Y - Position.Y)));
                    }
                    break;
                case GizmoMode.ScaleZ:
                    {
                        Vector3 intersection = ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitY);
                        GizmoScale(_scaleBase * (float)Math.Exp(_scaleSpeed / (intersection.Z - Position.Z)));
                    }
                    break;
                case GizmoMode.RotateY:
                    {
                        Vector3 rotationIntersection;
                        Plane rotationPlane = new Plane(Position, Vector3.TransformCoordinate(Vector3.UnitY, RotateMatrixY));
                        ray.Intersects(ref rotationPlane, out rotationIntersection);

                        Vector3 direction = rotationIntersection - Position;
                        _rotationLastMouseRadius = direction.Length();
                        direction.Normalize();

                        float sin = Vector3.Dot(Vector3.UnitZ, direction);
                        float cos = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitZ, direction));
                        _rotationLastMouseAngle = (float)Math.Atan2(-sin, cos);
                        GizmoRotateY(SimplifyAngle(_rotationPickAngleOffset + _rotationLastMouseAngle));
                    }
                    break;
                case GizmoMode.RotateX:
                    {
                        Vector3 rotationIntersection;
                        Plane rotationPlane = new Plane(Position, Vector3.TransformCoordinate(Vector3.UnitX, RotateMatrixX));
                        ray.Intersects(ref rotationPlane, out rotationIntersection);

                        Vector3 direction = rotationIntersection - Position;
                        _rotationLastMouseRadius = direction.Length();
                        direction.Normalize();

                        float sin = Vector3.Dot(Vector3.UnitY, direction);
                        float cos = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitY, direction));
                        _rotationLastMouseAngle = (float)Math.Atan2(-sin, cos);
                        GizmoRotateX(SimplifyAngle(_rotationPickAngleOffset + _rotationLastMouseAngle));
                    }
                    break;
                case GizmoMode.RotateZ:
                    {
                        Vector3 rotationIntersection;
                        Plane rotationPlane = new Plane(Position, Vector3.TransformCoordinate(Vector3.UnitZ, RotateMatrixZ));
                        ray.Intersects(ref rotationPlane, out rotationIntersection);

                        Vector3 direction = rotationIntersection - Position;
                        _rotationLastMouseRadius = direction.Length();
                        direction.Normalize();

                        float sin = Vector3.Dot(Vector3.UnitY, direction);
                        float cos = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitY, direction));
                        _rotationLastMouseAngle = (float)Math.Atan2(-sin, cos);
                        GizmoRotateZ(SimplifyAngle(_rotationPickAngleOffset + _rotationLastMouseAngle));
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

        public PickingResultGizmo DoPicking(Ray ray)
        {
            if (!DrawGizmo)
                return null;

            // Check for translation
            if (SupportTranslate)
            {
                float unused;
                BoundingSphere sphereX = new BoundingSphere(Position + Vector3.UnitX * Size, TranslationSphereSize / 1.7f);
                if (ray.Intersects(ref sphereX, out unused))
                    return new PickingResultGizmo(GizmoMode.TranslateX);

                BoundingSphere sphereY = new BoundingSphere(Position + Vector3.UnitY * Size, TranslationSphereSize / 1.7f);
                if (ray.Intersects(ref sphereY, out unused))
                    return new PickingResultGizmo(GizmoMode.TranslateY);

                BoundingSphere sphereZ = new BoundingSphere(Position - Vector3.UnitZ * Size, TranslationSphereSize / 1.7f);
                if (ray.Intersects(ref sphereZ, out unused))
                    return new PickingResultGizmo(GizmoMode.TranslateZ);
            }

            // Check for scale
            if (SupportScale)
            {
                float unused;
                BoundingBox scaleX = new BoundingBox(Position + Vector3.UnitX * Size / 2.0f - new Vector3(ScaleCubeSize / 2.0f),
                                                     Position + Vector3.UnitX * Size / 2.0f + new Vector3(ScaleCubeSize / 2.0f));
                if (ray.Intersects(ref scaleX, out unused))
                    return new PickingResultGizmo(GizmoMode.ScaleX);

                BoundingBox scaleY = new BoundingBox(Position + Vector3.UnitY * Size / 2.0f - new Vector3(ScaleCubeSize / 2.0f),
                                                     Position + Vector3.UnitY * Size / 2.0f + new Vector3(ScaleCubeSize / 2.0f));
                if (ray.Intersects(ref scaleY, out unused))
                    return new PickingResultGizmo(GizmoMode.ScaleY);

                BoundingBox scaleZ = new BoundingBox(Position - Vector3.UnitZ * Size / 2.0f - new Vector3(ScaleCubeSize / 2.0f),
                                                     Position - Vector3.UnitZ * Size / 2.0f + new Vector3(ScaleCubeSize / 2.0f));
                if (ray.Intersects(ref scaleZ, out unused))
                    return new PickingResultGizmo(GizmoMode.ScaleZ);
            }

            // Check for rotation
            float pickRadius = LineThickness / 2 + 55.0f;

            if (SupportRotationZ)
            {
                Plane planeZ = new Plane(Position, Vector3.TransformCoordinate(Vector3.UnitZ, RotateMatrixZ));
                Vector3 intersectionPoint;
                if (ray.Intersects(ref planeZ, out intersectionPoint))
                {
                    var distance = (intersectionPoint - Position).Length();
                    if (distance >= (Size - pickRadius) && distance <= (Size + pickRadius))
                    {
                        Vector3 startDirection = intersectionPoint - Position;
                        startDirection.Normalize();

                        float sin = Vector3.Dot(Vector3.UnitY, startDirection);
                        float cos = Vector3.Dot(planeZ.Normal, Vector3.Cross(Vector3.UnitY, startDirection));
                        return new PickingResultGizmo(GizmoMode.RotateZ, (float)Math.Atan2(-sin, cos), distance);
                    }
                }
            }

            if (SupportRotationX)
            {
                Plane planeX = new Plane(Position, Vector3.TransformCoordinate(Vector3.UnitX, RotateMatrixX));
                Vector3 intersectionPoint;
                if (ray.Intersects(ref planeX, out intersectionPoint))
                {
                    var distance = (intersectionPoint - Position).Length();
                    if (distance >= (Size - pickRadius) && distance <= (Size + pickRadius))
                    {
                        Vector3 startDirection = intersectionPoint - Position;
                        startDirection.Normalize();

                        float sin = Vector3.Dot(Vector3.UnitY, startDirection);
                        float cos = Vector3.Dot(planeX.Normal, Vector3.Cross(Vector3.UnitY, startDirection));
                        return new PickingResultGizmo(GizmoMode.RotateX, (float)Math.Atan2(-sin, cos), distance);
                    }
                }
            }

            if (SupportRotationY)
            {
                Plane planeY = new Plane(Position, Vector3.TransformCoordinate(Vector3.UnitY, RotateMatrixY));
                Vector3 intersectionPoint;
                if (ray.Intersects(ref planeY, out intersectionPoint))
                {
                    var distance = (intersectionPoint - Position).Length();
                    if (distance >= (Size - pickRadius) && distance <= (Size + pickRadius))
                    {
                        Vector3 startDirection = intersectionPoint - Position;
                        startDirection.Normalize();

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

            _scaleBase = SupportScale ? Scale : 1.0f;
            _rotationPickAngle = SimplifyAngle(pickingResult.RotationPickAngle);
            _rotationPickAngleOffset = SimplifyAngle(
                ((pickingResult.Mode == GizmoMode.RotateY) ? RotationY :
                (pickingResult.Mode == GizmoMode.RotateX) ? RotationX :
                (pickingResult.Mode == GizmoMode.RotateZ) ? RotationZ : 0.0f) - pickingResult.RotationPickAngle);
            _rotationLastMouseAngle = SimplifyAngle(pickingResult.RotationPickAngle);
            _rotationLastMouseRadius = pickingResult.Distance;
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

        private Matrix RotateMatrixY => Matrix.Identity;
        private Matrix RotateMatrixX => RotateMatrixY * Matrix.RotationY(SupportRotationY ? RotationY : 0.0f);
        private Matrix RotateMatrixZ => RotateMatrixX * Matrix.RotationX(SupportRotationX ? RotationX : 0.0f);

        public void Draw(Matrix viewProjection)
        {
            if (!DrawGizmo)
                return;

            _device.Clear(ClearOptions.DepthBuffer, Color4.Black, 1.0f, 0);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);

            var solidEffect = _effect;
            GizmoMode highlight = _mode == GizmoMode.None ? _hoveredMode : _mode;

            // Rotation
            if (SupportRotationX | SupportRotationY | SupportRotationZ)
            {
                // Setup torus model
                float requiredTorusRadius = LineThickness * 0.5f / Size;
                if (_torusRadius != requiredTorusRadius)
                {
                    _torus?.Dispose();
                    _torus = GeometricPrimitive.Torus.New(_device, 1.0f, requiredTorusRadius, 48, _lineRadiusTesselation);
                    _torusRadius = requiredTorusRadius;
                }
                _torus.SetupForRendering(_device);

                // Rotation X
                if (SupportRotationX)
                {
                    var model = Matrix.Scaling(Size * 2.0f) *
                        Matrix.RotationZ((float)Math.PI / 2.0f) *
                        RotateMatrixX *
                        Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_xAxisColor + (highlight == GizmoMode.RotateX ? _hoveredAddition : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _torus.IndexBuffer.ElementCount);
                }

                // Rotation Y
                if (SupportRotationY)
                {
                    var model = Matrix.Scaling(Size * 2.0f) *
                        RotateMatrixY *
                        Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_yAxisColor + (highlight == GizmoMode.RotateY ? _hoveredAddition * _hoveredAdditionGreensCorrection : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _torus.IndexBuffer.ElementCount);
                }

                // Rotation Z
                if (SupportRotationZ)
                {
                    var model = Matrix.Scaling(Size * 2.0f) *
                        Matrix.RotationX((float)Math.PI / 2.0f) *
                        RotateMatrixZ *
                        Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_zAxisColor + (highlight == GizmoMode.RotateZ ? _hoveredAddition : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, _torus.IndexBuffer.ElementCount);
                }
            }

            _device.Clear(ClearOptions.DepthBuffer, Color4.Black, 1.0f, 0);

            // Scale
            if (SupportScale)
            {
                _cylinder.SetupForRendering(_device);
                _device.SetRasterizerState(_device.RasterizerStates.CullFront);

                // X axis
                {
                    var model = Matrix.Translation(new Vector3(0.0f, 0.5f, 0.0f)) *
                        Matrix.Scaling(new Vector3(LineThickness * 1.1f, Size / 2.0f, LineThickness * 1.1f)) *
                        Matrix.RotationZ(-(float)Math.PI / 2.0f) *
                        Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_xAxisColor + (highlight == GizmoMode.ScaleX ? _hoveredAddition : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                // Y axis
                {
                    var model = Matrix.Translation(new Vector3(0.0f, 0.5f, 0.0f)) *
                        Matrix.Scaling(new Vector3(LineThickness * 1.1f, Size / 2.0f, LineThickness * 1.1f)) *
                        Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_yAxisColor + (highlight == GizmoMode.ScaleY ? _hoveredAddition * _hoveredAdditionGreensCorrection : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                // Z axis
                {
                    var model = Matrix.Translation(new Vector3(0.0f, 0.5f, 0.0f)) *
                        Matrix.Scaling(new Vector3(LineThickness * 1.1f, Size / 2.0f, LineThickness * 1.1f)) *
                        Matrix.RotationX(-(float)Math.PI / 2.0f) *
                        Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_zAxisColor + (highlight == GizmoMode.ScaleZ ? _hoveredAddition : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                _cube.SetupForRendering(_device);
                _device.SetRasterizerState(_device.RasterizerStates.CullBack);

                // X axis scale
                {
                    var model = Matrix.Scaling(ScaleCubeSize) *
                        Matrix.Translation(Position + Vector3.UnitX * Size / 2.0f);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_xAxisColor + (highlight == GizmoMode.ScaleX ? _hoveredAddition : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }

                // Y axis scale
                {
                    var model = Matrix.Scaling(ScaleCubeSize) *
                        Matrix.Translation(Position + Vector3.UnitY * Size / 2.0f);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_yAxisColor + (highlight == GizmoMode.ScaleY ? _hoveredAddition * _hoveredAdditionGreensCorrection : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }

                // Z axis scale
                {
                    var model = Matrix.Scaling(ScaleCubeSize) *
                        Matrix.Translation(Position - Vector3.UnitZ * Size / 2.0f);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_zAxisColor + (highlight == GizmoMode.ScaleZ ? _hoveredAddition : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }
            }

            // Translation
            if (SupportTranslate)
            {
                _cylinder.SetupForRendering(_device);
                _device.SetRasterizerState(_device.RasterizerStates.CullFront);

                // X axis
                {
                    var model = Matrix.Translation(new Vector3(0.0f, 0.5f, 0.0f)) *
                        Matrix.Scaling(new Vector3(LineThickness, Size, LineThickness)) *
                        Matrix.RotationZ(-(float)Math.PI / 2.0f) *
                        Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_xAxisColor + (highlight == GizmoMode.TranslateX ? _hoveredAddition : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                // Y axis
                {
                    var model = Matrix.Translation(new Vector3(0.0f, 0.5f, 0.0f)) *
                        Matrix.Scaling(new Vector3(LineThickness, Size, LineThickness)) *
                        Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_yAxisColor + (highlight == GizmoMode.TranslateY ? _hoveredAddition * _hoveredAdditionGreensCorrection : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                // Z axis
                {
                    var model = Matrix.Translation(new Vector3(0.0f, 0.5f, 0.0f)) *
                        Matrix.Scaling(new Vector3(LineThickness, Size, LineThickness)) *
                        Matrix.RotationX(-(float)Math.PI / 2.0f) *
                        Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_zAxisColor + (highlight == GizmoMode.TranslateZ ? _hoveredAddition : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                _cone.SetupForRendering(_device);
                _device.SetRasterizerState(_device.RasterizerStates.CullNone);

                // X axis translation
                {
                    var model = Matrix.RotationY((float)-Math.PI * 0.5f) *
                        Matrix.Scaling(TranslationSphereSize) *
                        Matrix.Translation(Position + (Vector3.UnitX + new Vector3(0.1f, 0, 0)) * Size);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_xAxisColor + (highlight == GizmoMode.TranslateX ? _hoveredAddition : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }

                // Y axis translation
                {
                    var model = Matrix.RotationX((float)Math.PI * 0.5f) *
                        Matrix.Scaling(TranslationSphereSize) *
                        Matrix.Translation(Position + (Vector3.UnitY + new Vector3(0, 0.1f, 0)) * Size);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_yAxisColor + (highlight == GizmoMode.TranslateY ? _hoveredAddition * _hoveredAdditionGreensCorrection : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }

                // Z axis translation
                {
                    var model = Matrix.Scaling(TranslationSphereSize) *
                        Matrix.Translation(Position - (Vector3.UnitZ + new Vector3(0, 0, 0.1f)) * Size);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_zAxisColor + (highlight == GizmoMode.TranslateZ ? _hoveredAddition : new Color4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }
            }

            // All time geometry
            {
                /*_device.SetVertexBuffer(_cube.VertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cube.VertexBuffer));
                _device.SetIndexBuffer(_cube.IndexBuffer, _cube.IsIndex32Bits);

                // center cube
                {
                    var model = Matrix.Scaling(CentreCubeSize) * Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_centerColor);
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }*/
            }

            // Rotation display vertices
            switch (_mode)
            {
                case GizmoMode.RotateY:
                case GizmoMode.RotateX:
                case GizmoMode.RotateZ:

                    // Figure out relevant angle
                    float startAngle;
                    float endAngle;
                    float lastMouseAngle;
                    Matrix baseMatrix;
                    Color4 color;
                    switch (_mode)
                    {
                        case GizmoMode.RotateY:
                            startAngle = _rotationPickAngle;
                            endAngle = RotationY - _rotationPickAngleOffset;
                            lastMouseAngle = _rotationLastMouseAngle;
                            baseMatrix = RotateMatrixY;
                            color = _yAxisColor;
                            break;
                        case GizmoMode.RotateX:
                            startAngle = -MathUtil.PiOverTwo - (_rotationPickAngle);
                            endAngle = -MathUtil.PiOverTwo - (RotationX - _rotationPickAngleOffset);
                            lastMouseAngle = -MathUtil.PiOverTwo - _rotationLastMouseAngle;
                            baseMatrix = Matrix.RotationZ((float)Math.PI / 2.0f) * RotateMatrixX;
                            color = _xAxisColor;
                            break;
                        case GizmoMode.RotateZ:
                            startAngle = MathUtil.Pi + _rotationPickAngle;
                            endAngle = MathUtil.Pi + RotationZ - _rotationPickAngleOffset;
                            lastMouseAngle = MathUtil.Pi + _rotationLastMouseAngle;
                            baseMatrix = Matrix.RotationX((float)Math.PI / 2.0f) * RotateMatrixZ;
                            color = _zAxisColor;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    { // Choose shortest path
                        float shortestAngle = endAngle - startAngle;
                        shortestAngle = (float)(shortestAngle - Math.Round(shortestAngle / (Math.PI * 2)) * (Math.PI * 2));
                        endAngle = startAngle + shortestAngle;
                    }
                    if (startAngle > endAngle)
                    {
                        float temp = startAngle;
                        startAngle = endAngle;
                        endAngle = temp;
                    }

                    // Build rotation geometry
                    var rotationHelperGeometry = new SolidVertex[_rotationTrianglesCount * 3 + 2];
                    float angleStep = (endAngle - startAngle) / _rotationTrianglesCount;
                    var middleVertex = new SolidVertex(new Vector3());
                    var lastVertex = new SolidVertex(new Vector3((float)Math.Cos(startAngle), 0, (float)-Math.Sin(startAngle)));
                    for (int i = 0; i < _rotationTrianglesCount; ++i)
                    {
                        float currentAngle = startAngle + (i + 1) * angleStep;
                        var currentVertex = new SolidVertex(new Vector3((float)Math.Cos(currentAngle), 0, (float)-Math.Sin(currentAngle)));
                        rotationHelperGeometry[i * 3 + 0] = middleVertex;
                        rotationHelperGeometry[i * 3 + 1] = lastVertex;
                        rotationHelperGeometry[i * 3 + 2] = currentVertex;
                        lastVertex = currentVertex;
                    }

                    rotationHelperGeometry[_rotationTrianglesCount * 3] = new SolidVertex(new Vector3(
                        (_rotationLastMouseRadius / Size) * (float)Math.Cos(lastMouseAngle), 0,
                        (_rotationLastMouseRadius / Size) * (float)-Math.Sin(lastMouseAngle)));
                    rotationHelperGeometry[_rotationTrianglesCount * 3 + 1] = middleVertex;
                    _rotationHelperGeometry.SetData(rotationHelperGeometry);

                    // Draw
                    _device.SetRasterizerState(_device.RasterizerStates.CullNone);
                    _device.SetVertexBuffer(_rotationHelperGeometry);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _rotationHelperGeometry));
                    var model = Matrix.Scaling(Size) * baseMatrix * Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(color * _rotationAlpha);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.Draw(PrimitiveType.TriangleList, _rotationTrianglesCount * 3);

                    solidEffect.Parameters["Color"].SetValue(Vector4.One);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.SetRasterizerState(_rasterizerWireframe);
                    _device.Draw(PrimitiveType.LineList, 2, _rotationTrianglesCount * 3);

                    break;
            }
        }

        protected abstract void GizmoMove(Vector3 newPos);
        protected abstract void GizmoScale(float newScale);
        protected abstract void GizmoRotateY(float newAngle);
        protected abstract void GizmoRotateX(float newAngle);
        protected abstract void GizmoRotateZ(float newAngle);

        protected abstract Vector3 Position { get; }
        protected abstract float RotationY { get; }
        protected abstract float RotationX { get; }
        protected abstract float RotationZ { get; }
        protected abstract float Scale { get; }
        protected abstract float CentreCubeSize { get; }
        protected abstract float TranslationSphereSize { get; }
        protected abstract float ScaleCubeSize { get; }
        protected abstract float Size { get; }
        protected abstract float LineThickness { get; }
        protected abstract bool SupportTranslate { get; }
        protected abstract bool SupportScale { get; }
        protected abstract bool SupportRotationY { get; }
        protected abstract bool SupportRotationX { get; }
        protected abstract bool SupportRotationZ { get; }
        protected virtual bool DrawGizmo => SupportTranslate || SupportScale || SupportRotationY || SupportRotationX || SupportRotationZ;
    }
}
