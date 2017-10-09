using SharpDX;
using SharpDX.Toolkit.Graphics;
using System.Windows.Forms;
using System;

namespace TombLib.Graphics
{
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
        public GizmoMode Mode { get; set; }

        public PickingResultGizmo(GizmoMode mode)
        {
            Mode = mode;
        }
    }

    public abstract class BaseGizmo : IDisposable
    {
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
        private static readonly Color4 _selected = new Color4(0.6f, 0.4f, 0.8f, 0.8f);

        public GizmoMode Mode { get; set; }
        private Vector3 _scaleLastIntersectionPoint = Vector3.Zero;
        private float _rotationStartAngle = 0.0f;

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

        /// <returns>true, if an iteraction with the gizmo is happening</returns>
        public bool MouseMoved(Matrix viewProjection, int x, int y)
        {
            if ((!DrawGizmo) || (Mode == GizmoMode.None))
                return false;

            // First get the ray in 3D space from X, Y mouse coordinates
            Ray ray = Ray.GetPickRay(x, y, _device.Viewport, viewProjection);
            switch (Mode)
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
                        GizmoScale(intersection.X - _scaleLastIntersectionPoint.X);
                        _scaleLastIntersectionPoint = intersection;
                    }
                    break;
                case GizmoMode.ScaleY:
                    {
                        Vector3 intersection = ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitZ);
                        GizmoScale(intersection.Y - _scaleLastIntersectionPoint.Y);
                        _scaleLastIntersectionPoint = intersection;
                    }
                    break;
                case GizmoMode.ScaleZ:
                    {
                        Vector3 intersection = ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitY);
                        GizmoScale(_scaleLastIntersectionPoint.Z - intersection.Z);
                        _scaleLastIntersectionPoint = intersection;
                    }
                    break;
                case GizmoMode.RotateY:
                    {
                        Vector3 rotationIntersection;
                        Plane rotationPlane = new Plane(Position, Vector3.UnitY);
                        ray.Intersects(ref rotationPlane, out rotationIntersection);

                        var direction = (rotationIntersection - Position);
                        direction.Normalize();

                        float cos = Vector3.Dot(Vector3.UnitZ, direction);
                        float sin = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitZ, direction));
                        float angle = (float)Math.Atan2(sin, cos);
                        if (angle < 0.0f) angle += (float)Math.PI * 2.0f;

                        GizmoRotateY(angle - _rotationStartAngle);
                        _rotationStartAngle = angle;
                    }
                    break;
                case GizmoMode.RotateX:
                    {
                        Matrix transform = Matrix.RotationY(SupportRotationY ? RotationY : 0.0f);
                        Vector3 rotationIntersection;
                        Plane rotationPlane = new Plane(Position, Vector3.TransformCoordinate(Vector3.UnitX, transform));
                        ray.Intersects(ref rotationPlane, out rotationIntersection);

                        var direction = (rotationIntersection - Position);
                        direction.Normalize();

                        float cos = Vector3.Dot(Vector3.UnitY, direction);
                        float sin = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitY, direction));
                        float angle = (float)Math.Atan2(sin, cos);
                        if (angle < 0.0f) angle += (float)Math.PI * 2.0f;

                        GizmoRotateX(angle - _rotationStartAngle);
                        _rotationStartAngle = angle;
                    }
                    break;
                case GizmoMode.RotateZ:
                    {
                        Matrix transform = Matrix.RotationY(SupportRotationY ? RotationY : 0.0f) * Matrix.RotationX(SupportRotationX ? RotationX : 0.0f);
                        Vector3 rotationIntersection;
                        Plane rotationPlane = new Plane(Position, Vector3.TransformCoordinate(Vector3.UnitZ, transform));
                        ray.Intersects(ref rotationPlane, out rotationIntersection);

                        var direction = (rotationIntersection - Position);
                        direction.Normalize();

                        float cos = Vector3.Dot(Vector3.UnitY, direction);
                        float sin = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitY, direction));
                        float angle = (float)Math.Atan2(sin, cos);
                        if (angle < 0.0f) angle += (float)Math.PI * 2.0f;

                        GizmoRotateZ(angle - _rotationStartAngle);
                        _rotationStartAngle = angle;
                    }
                    break;
            }

            return true;
        }

        public PickingResultGizmo DoPicking(Ray ray)
        {
            if (!DrawGizmo)
                return null;

            // Check for translation
            if (SupportTranslate)
            {
                BoundingSphere sphereX = new BoundingSphere(Position + Vector3.UnitX * Size, TranslationSphereSize / 2.0f);
                if (ray.Intersects(ref sphereX, out _scaleLastIntersectionPoint))
                    return new PickingResultGizmo(Mode = GizmoMode.TranslateX);

                BoundingSphere sphereY = new BoundingSphere(Position + Vector3.UnitY * Size, TranslationSphereSize / 2.0f);
                if (ray.Intersects(ref sphereY, out _scaleLastIntersectionPoint))
                    return new PickingResultGizmo(Mode = GizmoMode.TranslateY);

                BoundingSphere sphereZ = new BoundingSphere(Position - Vector3.UnitZ * Size, TranslationSphereSize / 2.0f);
                if (ray.Intersects(ref sphereZ, out _scaleLastIntersectionPoint))
                    return new PickingResultGizmo(Mode = GizmoMode.TranslateZ);
            }

            // Check for scale
            if (SupportScale)
            {
                BoundingBox scaleX = new BoundingBox(Position + Vector3.UnitX * Size / 2.0f - new Vector3(ScaleCubeSize / 2.0f),
                                                     Position + Vector3.UnitX * Size / 2.0f + new Vector3(ScaleCubeSize / 2.0f));
                if (ray.Intersects(ref scaleX, out _scaleLastIntersectionPoint))
                    return new PickingResultGizmo(GizmoMode.ScaleX);

                BoundingBox scaleY = new BoundingBox(Position + Vector3.UnitY * Size / 2.0f - new Vector3(ScaleCubeSize / 2.0f),
                                                     Position + Vector3.UnitY * Size / 2.0f + new Vector3(ScaleCubeSize / 2.0f));
                if (ray.Intersects(ref scaleY, out _scaleLastIntersectionPoint))
                    return new PickingResultGizmo(GizmoMode.ScaleY);

                BoundingBox scaleZ = new BoundingBox(Position - Vector3.UnitZ * Size / 2.0f - new Vector3(ScaleCubeSize / 2.0f),
                                                     Position - Vector3.UnitZ * Size / 2.0f + new Vector3(ScaleCubeSize / 2.0f));
                if (ray.Intersects(ref scaleZ, out _scaleLastIntersectionPoint))
                    return new PickingResultGizmo(GizmoMode.ScaleZ);
            }

            // Check for rotation
            float pickRadius = LineThickness / 2 + 55.0f;

            if (SupportRotationY)
            {
                Plane planeY = new Plane(Position, Vector3.UnitY);
                if (ray.Intersects(ref planeY, out _scaleLastIntersectionPoint))
                {
                    var distance = (_scaleLastIntersectionPoint - Position).Length();
                    if (distance >= (Size - pickRadius) && distance <= (Size + pickRadius))
                    {
                        Vector3 startDirection = (_scaleLastIntersectionPoint - Position);
                        startDirection.Normalize();

                        float cos = Vector3.Dot(Vector3.UnitZ, startDirection);
                        float sin = Vector3.Dot(planeY.Normal, Vector3.Cross(Vector3.UnitZ, startDirection));
                        _rotationStartAngle = (float)Math.Atan2(sin, cos);
                        if (_rotationStartAngle < 0.0f)
                            _rotationStartAngle += (float)Math.PI * 2.0f;

                        return new PickingResultGizmo(GizmoMode.RotateY);
                    }
                }
            }

            if (SupportRotationX)
            {
                Matrix transform = Matrix.RotationY(SupportRotationY ? RotationY : 0.0f);
                Plane planeX = new Plane(Position, Vector3.TransformCoordinate(Vector3.UnitX, transform));
                if (ray.Intersects(ref planeX, out _scaleLastIntersectionPoint))
                {
                    var distance = (_scaleLastIntersectionPoint - Position).Length();
                    if (distance >= (Size - pickRadius) && distance <= (Size + pickRadius))
                    {
                        Vector3 startDirection = (_scaleLastIntersectionPoint - Position);
                        startDirection.Normalize();

                        float cos = Vector3.Dot(Vector3.UnitY, startDirection);
                        float sin = Vector3.Dot(planeX.Normal, Vector3.Cross(Vector3.UnitY, startDirection));
                        _rotationStartAngle = (float)Math.Atan2(sin, cos);
                        if (_rotationStartAngle < 0.0f) _rotationStartAngle += (float)Math.PI * 2.0f;

                        return new PickingResultGizmo(GizmoMode.RotateX);
                    }
                }
            }

            if (SupportRotationZ)
            {
                Matrix transform = Matrix.RotationY(SupportRotationY ? RotationY : 0.0f) * Matrix.RotationX(SupportRotationX ? RotationX : 0.0f);
                Plane planeZ = new Plane(Position, Vector3.TransformCoordinate(Vector3.UnitZ, transform));
                if (ray.Intersects(ref planeZ, out _scaleLastIntersectionPoint))
                {
                    var distance = (_scaleLastIntersectionPoint - Position).Length();
                    if (distance >= (Size - pickRadius) && distance <= (Size + pickRadius))
                    {
                        Vector3 startDirection = (_scaleLastIntersectionPoint - Position);
                        startDirection.Normalize();

                        float cos = Vector3.Dot(Vector3.UnitY, startDirection);
                        float sin = Vector3.Dot(planeZ.Normal, Vector3.Cross(Vector3.UnitY, startDirection));
                        _rotationStartAngle = (float)Math.Atan2(sin, cos);
                        if (_rotationStartAngle < 0.0f) _rotationStartAngle += (float)Math.PI * 2.0f;

                        return new PickingResultGizmo(GizmoMode.RotateZ);
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
                    solidEffect.Parameters["Color"].SetValue(Mode == GizmoMode.RotateY ? _selected : _green);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _torus.IndexBuffer.ElementCount);
                }

                // Rotation X
                if (SupportRotationX)
                {
                    var model = Matrix.Scaling(Size * 2.0f) *
                            Matrix.RotationZ((float)Math.PI / 2.0f) *
                            Matrix.RotationY(SupportRotationY ? RotationY : 0.0f) *
                            Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(Mode == GizmoMode.RotateX ? _selected : _red);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _torus.IndexBuffer.ElementCount);
                }

                // Rotation Z
                if (SupportRotationZ)
                {
                    var model = Matrix.Scaling(Size * 2.0f) *
                            Matrix.RotationX((float)Math.PI / 2.0f) *
                            Matrix.RotationY(SupportRotationY ? RotationY : 0.0f) *
                            Matrix.RotationX(SupportRotationX ? RotationX : 0.0f) *
                            Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(Mode == GizmoMode.RotateZ ? _selected : _blue);
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, _torus.IndexBuffer.ElementCount);
                }
            }

            // Scale
            if (SupportScale)
            {
                _cube.SetupForRendering(_device);

                // X axis scale
                {
                    var model = Matrix.Scaling(ScaleCubeSize) *
                            Matrix.Translation(Position + Vector3.UnitX * Size / 2.0f);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(Mode == GizmoMode.ScaleX ? _selected : _red);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }

                // Y axis scale
                {
                    var model = Matrix.Scaling(ScaleCubeSize) *
                            Matrix.Translation(Position + Vector3.UnitY * Size / 2.0f);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(Mode == GizmoMode.ScaleY ? _selected : _green);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }

                // Z axis scale
                {
                    var model = Matrix.Scaling(ScaleCubeSize) *
                            Matrix.Translation(Position - Vector3.UnitZ * Size / 2.0f);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(Mode == GizmoMode.ScaleZ ? _selected : _blue);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }
            }

            // Translation
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
                    solidEffect.Parameters["Color"].SetValue(Mode == GizmoMode.TranslateX ? _selected : _red);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                // Y axis
                {
                    var model = Matrix.Translation(new Vector3(0.0f, 0.5f, 0.0f)) *
                                Matrix.Scaling(new Vector3(LineThickness, Size, LineThickness)) *
                                Matrix.Translation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(Mode == GizmoMode.TranslateY ? _selected : _green);
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
                    solidEffect.Parameters["Color"].SetValue(Mode == GizmoMode.TranslateZ ? _selected : _blue);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                _sphere.SetupForRendering(_device);

                // X axis translation
                {
                    var model = Matrix.Scaling(TranslationSphereSize) *
                            Matrix.Translation(Position + Vector3.UnitX * Size);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(Mode == GizmoMode.TranslateX ? _selected : _red);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);
                }

                // Y axis translation
                {
                    var model = Matrix.Scaling(TranslationSphereSize) *
                            Matrix.Translation(Position + Vector3.UnitY * Size);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(Mode == GizmoMode.TranslateY ? _selected : _green);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);
                }

                // Z axis translation
                {
                    var model = Matrix.Scaling(TranslationSphereSize) *
                            Matrix.Translation(Position - Vector3.UnitZ * Size);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                    solidEffect.Parameters["Color"].SetValue(Mode == GizmoMode.TranslateZ ? _selected : _blue);
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

        protected abstract void GizmoMove(Vector3 newPos);
        protected abstract void GizmoScale(float scale);
        protected abstract void GizmoRotateY(float angle);
        protected abstract void GizmoRotateX(float angle);
        protected abstract void GizmoRotateZ(float angle);

        protected abstract Vector3 Position { get; }
        protected abstract float RotationY { get; }
        protected abstract float RotationX { get; }
        protected abstract float RotationZ { get; }
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
