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
            this.Axis = axis;
            this.Action = action;
        }
    }

    public interface IScaleable
    {
        float Scale { get; set; }
    };

    public interface IRotateableY
    {
        float RotationY { get; set; }
    };

    public interface IRotateableYX : IRotateableY
    {
        float RotationX { get; set; }
    };

    public interface IRotateableYXRoll : IRotateableYX
    {
        float Roll { get; set; }
    };

    public abstract class BaseGizmo
    {
        private GizmoAxis _axis;
        
        private readonly RasterizerState _rasterizerWireframe;
        private readonly DepthStencilState _depthStencilState;
        private readonly DepthStencilState _depthStencilStateDefault;

        private readonly Effect _effect;
       
        // Geometry of the gizmo
        private readonly GraphicsDevice _device;
        private readonly Buffer<SolidVertex> _linesBuffer;

        private readonly GeometricPrimitive _sphere;
        private readonly GeometricPrimitive _cube;
        private readonly Color4 _red;
        private readonly Color4 _green;
        private readonly Color4 _blue;
        private readonly Color4 _yellow;

        protected PickingResultGizmo _lastResult;
        protected Vector3 _lastIntersectionPoint = Vector3.Zero;

        public BaseGizmo(GraphicsDevice device, Effect effect)
        {
            _effect = effect;
            _device = device;

            _red = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
            _green = new Color4(0.0f, 1.0f, 0.0f, 1.0f);
            _blue = new Color4(0.0f, 0.0f, 1.0f, 1.0f);
            _yellow = new Color4(1.0f, 1.0f, 0.0f, 1.0f);

            // Initialize the gizmo geometry
            var v0 = new SolidVertex { Position = new Vector3(0.0f, 0.0f, 0.0f) };
            var vX = new SolidVertex { Position = new Vector3(1.0f, 0.0f, 0.0f) };
            var vY = new SolidVertex { Position = new Vector3(0.0f, 1.0f, 0.0f) };
            var vZ = new SolidVertex { Position = new Vector3(0.0f, 0.0f, -1.0f) };
            var vertices = new[] { v0, vX, v0, vY, v0, vZ };

            _linesBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New
                (_device, vertices, SharpDX.Direct3D11.ResourceUsage.Dynamic);

            _sphere = GeometricPrimitive.Sphere.New(_device, 1.0f, 16);
            _cube = GeometricPrimitive.Cube.New(_device, 1.0f);

            // Initialize the rasterizer state for wireframe drawing
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

            // Initialize the depth stencil state
            SharpDX.Direct3D11.DepthStencilStateDescription depthStencilState = SharpDX.Direct3D11.DepthStencilStateDescription.Default();
            depthStencilState.IsDepthEnabled = false;
            depthStencilState.DepthComparison = SharpDX.Direct3D11.Comparison.Never;
            depthStencilState.DepthWriteMask = SharpDX.Direct3D11.DepthWriteMask.Zero;
            _depthStencilState = DepthStencilState.New(_device, depthStencilState);

            _depthStencilStateDefault = DepthStencilState.New(_device, SharpDX.Direct3D11.DepthStencilStateDescription.Default());
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

            if (Action == GizmoAction.Translate || Action == GizmoAction.Scale)
            {
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
                            newPos.X = intersection.X - 1024.0f;
                            delta = intersection.X - _lastIntersectionPoint.X;
                            _lastIntersectionPoint = intersection;
                        }
                        break;
                    case GizmoAxis.Y:
                        {
                            Plane plane = new Plane(newPos, Vector3.UnitX);
                            Vector3 intersection;
                            ray.Intersects(ref plane, out intersection);
                            newPos.Y = intersection.Y - 1024.0f;
                            delta = intersection.Y - _lastIntersectionPoint.Y;
                            _lastIntersectionPoint = intersection;
                        }
                        break;
                    case GizmoAxis.Z:
                        {
                            Plane plane = new Plane(newPos, Vector3.UnitY);
                            Vector3 intersection;
                            ray.Intersects(ref plane, out intersection);
                            newPos.Z = intersection.Z + 1024.0f;
                            delta = -(intersection.Z - _lastIntersectionPoint.Z);
                            _lastIntersectionPoint = intersection;
                        }
                        break;
                }

                DoGizmoAction(newPos, delta);
            }           

            return true;
        }

        public PickingResultGizmo DoPicking(Ray ray)
        {
            if (!DrawGizmo)
                return null;

            BoundingSphere sphereX = new BoundingSphere(Position + Vector3.UnitX * 1024.0f, TranslationSphereSize / 2.0f);
            if (ray.Intersects(ref sphereX, out _lastIntersectionPoint))
                return (_lastResult = new PickingResultGizmo(GizmoAxis.X, GizmoAction.Translate));

            BoundingSphere sphereY = new BoundingSphere(Position + Vector3.UnitY * 1024.0f, TranslationSphereSize / 2.0f);
            if (ray.Intersects(ref sphereY, out _lastIntersectionPoint))
                return (_lastResult = new PickingResultGizmo(GizmoAxis.Y, GizmoAction.Translate));

            BoundingSphere sphereZ = new BoundingSphere(Position - Vector3.UnitZ * 1024.0f, TranslationSphereSize / 2.0f);
            if (ray.Intersects(ref sphereZ, out _lastIntersectionPoint))
                return (_lastResult = new PickingResultGizmo(GizmoAxis.Z, GizmoAction.Translate));

            BoundingBox scaleX = new BoundingBox(Position + Vector3.UnitX * 512.0f - new Vector3(ScaleCubeSize / 2.0f),
                                                 Position + Vector3.UnitX * 512.0f + new Vector3(ScaleCubeSize / 2.0f));
            if (ray.Intersects(ref scaleX, out _lastIntersectionPoint))
                return (_lastResult = new PickingResultGizmo(GizmoAxis.X, GizmoAction.Scale));

            BoundingBox scaleY = new BoundingBox(Position + Vector3.UnitY * 512.0f - new Vector3(ScaleCubeSize / 2.0f),
                                                 Position + Vector3.UnitY * 512.0f + new Vector3(ScaleCubeSize / 2.0f));
            if (ray.Intersects(ref scaleY, out _lastIntersectionPoint))
                return (_lastResult = new PickingResultGizmo(GizmoAxis.Y, GizmoAction.Scale));

            BoundingBox scaleZ = new BoundingBox(Position - Vector3.UnitZ * 512.0f - new Vector3(ScaleCubeSize / 2.0f),
                                                 Position - Vector3.UnitZ * 512.0f + new Vector3(ScaleCubeSize / 2.0f));
            if (ray.Intersects(ref scaleZ, out _lastIntersectionPoint))
                return (_lastResult = new PickingResultGizmo(GizmoAxis.Z, GizmoAction.Scale));

            return null;
        }

        public void Draw(Matrix viewProjection)
        {
            if (!DrawGizmo)
                return;

            _device.SetDepthStencilState(_depthStencilState);
            _device.SetRasterizerState(_rasterizerWireframe);
            _device.SetVertexBuffer(_linesBuffer);
            _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _linesBuffer));

            var solidEffect = _effect;

            //_editor.Configuration.Gizmo_Size
            var model = Matrix.Scaling(Size) *
                        Matrix.Translation(Position);
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);

            // X axis
            solidEffect.Parameters["Color"].SetValue(_red);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _device.Draw(PrimitiveType.LineList, 2, 0);

            // Y axis
            solidEffect.Parameters["Color"].SetValue(_green);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _device.Draw(PrimitiveType.LineList, 2, 2);

            // Z axis
            solidEffect.Parameters["Color"].SetValue(_blue);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _device.Draw(PrimitiveType.LineList, 2, 4);

            _device.SetRasterizerState(_device.RasterizerStates.CullBack);

            _device.SetVertexBuffer(_sphere.VertexBuffer);
            _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _sphere.VertexBuffer));
            _device.SetIndexBuffer(_sphere.IndexBuffer, _sphere.IsIndex32Bits);

            // X axis translation
            model = Matrix.Scaling(TranslationSphereSize) *
                    Matrix.Translation(Position + Vector3.UnitX * Size);
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
            solidEffect.Parameters["Color"].SetValue(_red);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _device.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);

            // Y axis translation
            model = Matrix.Scaling(TranslationSphereSize) *
                    Matrix.Translation(Position + Vector3.UnitY * Size);
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
            solidEffect.Parameters["Color"].SetValue(_green);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _device.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);

            // Z axis translation
            model = Matrix.Scaling(TranslationSphereSize) *
                    Matrix.Translation(Position - Vector3.UnitZ * Size);
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
            solidEffect.Parameters["Color"].SetValue(_blue);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _device.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);

            // Scale
            if (SupportScale)
            {
                _device.SetVertexBuffer(_cube.VertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cube.VertexBuffer));
                _device.SetIndexBuffer(_cube.IndexBuffer, _cube.IsIndex32Bits);

                // X axis scale
                model = Matrix.Scaling(ScaleCubeSize) *
                        Matrix.Translation(Position + Vector3.UnitX * Size / 2.0f);
                solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                solidEffect.Parameters["Color"].SetValue(_red);
                solidEffect.CurrentTechnique.Passes[0].Apply();

                _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);

                // Y axis scale
                model = Matrix.Scaling(ScaleCubeSize) *
                        Matrix.Translation(Position + Vector3.UnitY * Size / 2.0f);
                solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                solidEffect.Parameters["Color"].SetValue(_green);
                solidEffect.CurrentTechnique.Passes[0].Apply();

                _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);

                // Z axis scale
                model = Matrix.Scaling(ScaleCubeSize) *
                        Matrix.Translation(Position - Vector3.UnitZ * Size / 2.0f);
                solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
                solidEffect.Parameters["Color"].SetValue(_blue);
                solidEffect.CurrentTechnique.Passes[0].Apply();

                _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
            }

            // center cube
            _device.SetVertexBuffer(_cube.VertexBuffer);
            _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cube.VertexBuffer));
            _device.SetIndexBuffer(_cube.IndexBuffer, _cube.IsIndex32Bits);

            model = Matrix.Scaling(CentreCubeSize) *
                    Matrix.Translation(Position);
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
            solidEffect.Parameters["Color"].SetValue(_yellow);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);

            _device.SetDepthStencilState(_depthStencilStateDefault);
        }

        public GizmoAction Action { get { return (_lastResult != null ? _lastResult.Action : GizmoAction.Translate); } }

        protected abstract void DoGizmoAction(Vector3 newPos, float delta);

        protected abstract bool DrawGizmo { get; }
        protected abstract Vector3 Position { get; }
        protected abstract float CentreCubeSize { get; }
        protected abstract float TranslationSphereSize { get; }
        protected abstract float ScaleCubeSize { get; }
        protected abstract float Size { get; }
        protected abstract bool SupportScale { get; }
        protected abstract bool SupportRotationY { get; }
        protected abstract bool SupportRotationYX { get; }
        protected abstract bool SupportRotationYXRoll { get; }
    }
}
