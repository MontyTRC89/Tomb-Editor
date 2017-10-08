using SharpDX;
using SharpDX.Toolkit.Graphics;
using System.Windows.Forms;
using System;

namespace TombLib.Graphics
{
    public enum GizmoAction : byte
    {
        Translate,
        Rotate,
        Scale
    }

    public enum GizmoAxis : byte
    {
        None,
        X,
        Y,
        Z
    }

    public class PickingResultGizmo : PickingResult
    {
        public GizmoAxis Axis { get; set; }
        public GizmoAction Action { get; set; }

        public PickingResultGizmo(GizmoAxis axis, GizmoAction action)
        {
            Axis = axis;
            Action = action;
        }
    }

    public abstract class BaseGizmo : IDisposable
    {
        protected GizmoAxis _axis;

        private readonly RasterizerState _rasterizerWireframe;
        private readonly DepthStencilState _depthStencilState;
        private readonly DepthStencilState _depthStencilStateDefault;

        private readonly Effect _effect;

        // Geometry of the gizmo
        private readonly GraphicsDevice _device;
        private readonly GeometricPrimitive _cylinder;
        private readonly GeometricPrimitive _sphere;
        private readonly GeometricPrimitive _cube;
        private GeometricPrimitive _torus;
        private float _torusRadius = float.MinValue;
        private static readonly Color4 _red = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
        private static readonly Color4 _green = new Color4(0.0f, 1.0f, 0.0f, 1.0f);
        private static readonly Color4 _blue = new Color4(0.0f, 0.0f, 1.0f, 1.0f);
        private static readonly Color4 _yellow = new Color4(1.0f, 1.0f, 0.0f, 1.0f);

        protected PickingResultGizmo _lastResult;
        protected Vector3 _lastIntersectionPoint = Vector3.Zero;
        protected float _startAngle = 0.0f;
        protected Vector3 _startDirection = Vector3.Zero;

        public BaseGizmo(GraphicsDevice device, Effect effect)
        {
            _effect = effect;
            _device = device;

            // Create the gizmo geometry
            _cylinder = GeometricPrimitive.Cylinder.New(_device, 1.0f, 1.0f, 5);
            _sphere = GeometricPrimitive.Sphere.New(_device, 1.0f, 16);
            _cube = GeometricPrimitive.Cube.New(_device, 1.0f);

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

            // Create the depth stencil state
            SharpDX.Direct3D11.DepthStencilStateDescription depthStencilState = SharpDX.Direct3D11.DepthStencilStateDescription.Default();
            depthStencilState.IsDepthEnabled = false;
            depthStencilState.DepthComparison = SharpDX.Direct3D11.Comparison.Never;
            depthStencilState.DepthWriteMask = SharpDX.Direct3D11.DepthWriteMask.Zero;
            _depthStencilState = DepthStencilState.New(_device, depthStencilState);

            _depthStencilStateDefault = DepthStencilState.New(_device, SharpDX.Direct3D11.DepthStencilStateDescription.Default());
        }

        public void Dispose()
        {
            _rasterizerWireframe.Dispose();
            _depthStencilState.Dispose();
            _depthStencilStateDefault.Dispose();
            _cylinder.Dispose();
            _sphere.Dispose();
            _cube.Dispose();
            _torus.Dispose();
        }

        public void SetGizmoAxis(GizmoAxis axis)
        {
            _axis = axis;
        }

        /// <returns>true, if an iteraction with the gizmo is happening</returns>
        public bool MouseMoved(Matrix viewProjection, int x, int y)
        {
            if ((!DrawGizmo) || (_axis == GizmoAxis.None))
                return false;

            var newPos = Vector3.Zero;
            float delta = 0;
            float angle = 0;


            // First get the ray in 3D space from X, Y mouse coordinates
            Ray ray = Ray.GetPickRay(x, y, _device.Viewport, viewProjection);

            newPos = Position;
            switch (_axis)
            {
                case GizmoAxis.X:
                    {
                        Plane plane = new Plane(newPos, Vector3.UnitY);
                        Vector3 intersection;
                        ray.Intersects(ref plane, out intersection);

                        // Translation
                        newPos.X = intersection.X - Size;

                        // Scale
                        delta = intersection.X - _lastIntersectionPoint.X;
                        _lastIntersectionPoint = intersection;

                        // Rotate
                        Vector3 rotationIntersection = Vector3.Zero;
                        Plane rotationPlane = new Plane(Position, Vector3.UnitX);
                        ray.Intersects(ref rotationPlane, out rotationIntersection);

                        var direction = (rotationIntersection - Position);
                        direction.Normalize();

                        float cos = Vector3.Dot(Vector3.UnitY, direction);
                        float sin = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitY, direction));
                        angle = (float)Math.Atan2(sin, cos);
                        if (angle < 0.0f) angle += (float)Math.PI * 2.0f;

                        var startAngle = _startAngle;
                        _startAngle = angle;
                        angle -= startAngle;
                    }
                    break;
                case GizmoAxis.Y:
                    {
                        Plane plane = new Plane(newPos, Vector3.UnitX);
                        Vector3 intersection;
                        ray.Intersects(ref plane, out intersection);

                        // Translation
                        newPos.Y = intersection.Y - Size;

                        // Scale
                        delta = intersection.Y - _lastIntersectionPoint.Y;
                        _lastIntersectionPoint = intersection;

                        // Rotate
                        Vector3 rotationIntersection = Vector3.Zero;
                        Plane rotationPlane = new Plane(Position, Vector3.UnitY);
                        ray.Intersects(ref rotationPlane, out rotationIntersection);

                        var direction = (rotationIntersection - Position);
                        direction.Normalize();

                        float cos = Vector3.Dot(Vector3.UnitZ, direction);
                        float sin = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitZ, direction));
                        angle = (float)Math.Atan2(sin, cos);
                        if (angle < 0.0f) angle += (float)Math.PI * 2.0f;

                        var startAngle = _startAngle;
                        _startAngle = angle;
                        angle -= startAngle;
                    }
                    break;
                case GizmoAxis.Z:
                    {
                        Plane plane = new Plane(newPos, Vector3.UnitY);
                        Vector3 intersection;
                        ray.Intersects(ref plane, out intersection);

                        // Translation
                        newPos.Z = intersection.Z + Size;

                        // Scale
                        delta = -(intersection.Z - _lastIntersectionPoint.Z);
                        _lastIntersectionPoint = intersection;

                        // Rotate
                        Vector3 rotationIntersection = Vector3.Zero;
                        Plane rotationPlane = new Plane(Position, Vector3.UnitZ);
                        ray.Intersects(ref rotationPlane, out rotationIntersection);

                        var direction = (rotationIntersection - Position);
                        direction.Normalize();

                        float cos = Vector3.Dot(Vector3.UnitY, direction);
                        float sin = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitY, direction));
                        angle = (float)Math.Atan2(sin, cos);
                        if (angle < 0.0f) angle += (float)Math.PI * 2.0f;

                        var startAngle = _startAngle;
                        _startAngle = angle;
                        angle -= startAngle;
                    }
                    break;
            }

            DoGizmoAction(newPos, angle, delta);

            return true;
        }

        public PickingResultGizmo DoPicking(Ray ray)
        {
            if (!DrawGizmo)
                return null;

            float circleTolerance = TranslationSphereSize;

            // Check for translation
            BoundingSphere sphereX = new BoundingSphere(Position + Vector3.UnitX * Size, TranslationSphereSize / 2.0f);
            if (ray.Intersects(ref sphereX, out _lastIntersectionPoint))
                return (_lastResult = new PickingResultGizmo(GizmoAxis.X, GizmoAction.Translate));

            BoundingSphere sphereY = new BoundingSphere(Position + Vector3.UnitY * Size, TranslationSphereSize / 2.0f);
            if (ray.Intersects(ref sphereY, out _lastIntersectionPoint))
                return (_lastResult = new PickingResultGizmo(GizmoAxis.Y, GizmoAction.Translate));

            BoundingSphere sphereZ = new BoundingSphere(Position - Vector3.UnitZ * Size, TranslationSphereSize / 2.0f);
            if (ray.Intersects(ref sphereZ, out _lastIntersectionPoint))
                return (_lastResult = new PickingResultGizmo(GizmoAxis.Z, GizmoAction.Translate));

            // Check for scale
            BoundingBox scaleX = new BoundingBox(Position + Vector3.UnitX * Size / 2.0f - new Vector3(ScaleCubeSize / 2.0f),
                                                 Position + Vector3.UnitX * Size / 2.0f + new Vector3(ScaleCubeSize / 2.0f));
            if (ray.Intersects(ref scaleX, out _lastIntersectionPoint))
                return (_lastResult = new PickingResultGizmo(GizmoAxis.X, GizmoAction.Scale));

            BoundingBox scaleY = new BoundingBox(Position + Vector3.UnitY * Size / 2.0f - new Vector3(ScaleCubeSize / 2.0f),
                                                 Position + Vector3.UnitY * Size / 2.0f + new Vector3(ScaleCubeSize / 2.0f));
            if (ray.Intersects(ref scaleY, out _lastIntersectionPoint))
                return (_lastResult = new PickingResultGizmo(GizmoAxis.Y, GizmoAction.Scale));

            BoundingBox scaleZ = new BoundingBox(Position - Vector3.UnitZ * Size / 2.0f - new Vector3(ScaleCubeSize / 2.0f),
                                                 Position - Vector3.UnitZ * Size / 2.0f + new Vector3(ScaleCubeSize / 2.0f));
            if (ray.Intersects(ref scaleZ, out _lastIntersectionPoint))
                return (_lastResult = new PickingResultGizmo(GizmoAxis.Z, GizmoAction.Scale));

            // Check for rotation
            if (SupportRotationX)
            {
                Plane planeX = new Plane(Position, Vector3.UnitX);
                if (ray.Intersects(ref planeX, out _lastIntersectionPoint))
                {
                    var distance = (_lastIntersectionPoint - Position).Length();
                    if (distance >= (Size - circleTolerance) && distance <= (Size + circleTolerance))
                    {
                        _startDirection = (_lastIntersectionPoint - Position);
                        _startDirection.Normalize();

                        float cos = Vector3.Dot(Vector3.UnitY, _startDirection);
                        float sin = Vector3.Dot(planeX.Normal, Vector3.Cross(Vector3.UnitY, _startDirection));
                        _startAngle = (float)Math.Atan2(sin, cos);
                        if (_startAngle < 0.0f) _startAngle += (float)Math.PI * 2.0f;

                        return (_lastResult = new PickingResultGizmo(GizmoAxis.X, GizmoAction.Rotate));
                    }
                }
            }

            if (SupportRotationY)
            {
                Plane planeY = new Plane(Position, Vector3.UnitY);
                if (ray.Intersects(ref planeY, out _lastIntersectionPoint))
                {
                    var distance = (_lastIntersectionPoint - Position).Length();
                    if (distance >= (Size - circleTolerance) && distance <= (Size + circleTolerance))
                    {
                        _startDirection = (_lastIntersectionPoint - Position);
                        _startDirection.Normalize();

                        float cos = Vector3.Dot(Vector3.UnitZ, _startDirection);
                        float sin = Vector3.Dot(planeY.Normal, Vector3.Cross(Vector3.UnitZ, _startDirection));
                        _startAngle = (float)Math.Atan2(sin, cos);
                        if (_startAngle < 0.0f) _startAngle += (float)Math.PI * 2.0f;

                        return (_lastResult = new PickingResultGizmo(GizmoAxis.Y, GizmoAction.Rotate));
                    }
                }
            }

            if (SupportRotationZ)
            {
                Plane planeZ = new Plane(Position, Vector3.UnitZ);
                if (ray.Intersects(ref planeZ, out _lastIntersectionPoint))
                {
                    var distance = (_lastIntersectionPoint - Position).Length();
                    if (distance >= (Size - circleTolerance) && distance <= (Size + circleTolerance))
                    {
                        _startDirection = (_lastIntersectionPoint - Position);
                        _startDirection.Normalize();

                        float cos = Vector3.Dot(Vector3.UnitY, _startDirection);
                        float sin = Vector3.Dot(planeZ.Normal, Vector3.Cross(Vector3.UnitY, _startDirection));
                        _startAngle = (float)Math.Atan2(sin, cos);
                        if (_startAngle < 0.0f) _startAngle += (float)Math.PI * 2.0f;

                        return (_lastResult = new PickingResultGizmo(GizmoAxis.Z, GizmoAction.Rotate));
                    }
                }
            }

            return null;
        }

        public void Draw(Matrix viewProjection)
        {
            if (!DrawGizmo)
                return;

            _device.SetDepthStencilState(_depthStencilState);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);

            var solidEffect = _effect;

            // Scale
            if (SupportScale)
            {
                _cube.SetupForRendering(_device);

                // X axis scale
                {
                    var model = Matrix.Scaling(ScaleCubeSize) *
                            Matrix.Translation(Position + Vector3.UnitX * Size / 2.0f);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_red);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }

                // Y axis scale
                {
                    var model = Matrix.Scaling(ScaleCubeSize) *
                            Matrix.Translation(Position + Vector3.UnitY * Size / 2.0f);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_green);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }

                // Z axis scale
                {
                    var model = Matrix.Scaling(ScaleCubeSize) *
                            Matrix.Translation(Position - Vector3.UnitZ * Size / 2.0f);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_blue);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }
            }

            // Rotation
            if (SupportRotationX | SupportRotationY | SupportRotationZ)
            {
                float requiredTorusRadius = LineThickness * 0.5f / Size;
                if (_torusRadius != requiredTorusRadius)
                {
                    _torus = GeometricPrimitive.Torus.New(_device, 1.0f, requiredTorusRadius, 48, 5);
                    _torusRadius = requiredTorusRadius;
                }
                _torus.SetupForRendering(_device);

                // Rotation Y
                if (SupportRotationY)
                {
                    var model = Matrix.Scaling(Size * 2.0f) *
                            Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_green);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _torus.IndexBuffer.ElementCount);
                }

                // Rotation X
                if (SupportRotationX)
                {
                    var model = Matrix.Scaling(Size * 2.0f) *
                            Matrix.RotationZ((float)Math.PI / 2.0f) *
                            Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_red);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _torus.IndexBuffer.ElementCount);
                }

                // Rotation Z
                if (SupportRotationZ)
                {
                    var model = Matrix.Scaling(Size * 2.0f) *
                            Matrix.RotationX((float)Math.PI / 2.0f) *
                            Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_blue);
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, _torus.IndexBuffer.ElementCount);
                }
            }

            if (SupportTranslate)
            {
                _cylinder.SetupForRendering(_device);

                // X axis
                {
                    var model = Matrix.Translation(new Vector3(0.0f, 0.5f, 0.0f)) *
                                Matrix.Scaling(new Vector3(LineThickness, Size, LineThickness)) *
                                Matrix.RotationZ(-(float)Math.PI / 2.0f) *
                                Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_red);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                // Y axis
                {
                    var model = Matrix.Translation(new Vector3(0.0f, 0.5f, 0.0f)) *
                                Matrix.Scaling(new Vector3(LineThickness, Size, LineThickness)) *
                                Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_green);
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
                    solidEffect.Parameters["Color"].SetValue(_blue);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                _sphere.SetupForRendering(_device);

                // X axis translation
                {
                    var model = Matrix.Scaling(TranslationSphereSize) *
                            Matrix.Translation(Position + Vector3.UnitX * Size);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_red);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);
                }

                // Y axis translation
                {
                    var model = Matrix.Scaling(TranslationSphereSize) *
                            Matrix.Translation(Position + Vector3.UnitY * Size);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_green);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);
                }

                // Z axis translation
                {
                    var model = Matrix.Scaling(TranslationSphereSize) *
                            Matrix.Translation(Position - Vector3.UnitZ * Size);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_blue);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);
                }
            }

            // All time geometry
            {
                _device.SetVertexBuffer(_cube.VertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cube.VertexBuffer));
                _device.SetIndexBuffer(_cube.IndexBuffer, _cube.IsIndex32Bits);

                // center cube
                {
                    var model = Matrix.Scaling(CentreCubeSize) * Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(_yellow);
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }
                _device.SetDepthStencilState(_depthStencilStateDefault);
            }
        }

        public GizmoAction Action { get { return (_lastResult != null ? _lastResult.Action : GizmoAction.Translate); } }

        public GizmoAxis Axis { get { return (_lastResult != null ? _lastResult.Axis : GizmoAxis.X); } }

        protected abstract void DoGizmoAction(Vector3 newPos, float angle, float scale);

        protected abstract Vector3 Position { get; }
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
