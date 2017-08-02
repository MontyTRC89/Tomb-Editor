using SharpDX;
using SharpDX.Toolkit.Graphics;
using TombEditor.Geometry;
using System.Windows.Forms;

namespace TombEditor
{
    public enum GizmoAxis : byte
    {
        None,
        X,
        Y,
        Z
    }
    
    public class PickingResultGizmo : Controls.PickingResult
    {
        public GizmoAxis Axis { get; set; }
        public PickingResultGizmo(float Distance, GizmoAxis axis)
        {
            Axis = axis;
        }
    }

    public class Gizmo
    {
        private GizmoAxis _axis;
        private Vector3 _position;
        private bool _drawGizmo;

        private readonly Editor _editor;
        private readonly RasterizerState _rasterizerWireframe;

        // Geometry of the gizmo
        private readonly DeviceManager _deviceManager;
        private readonly GraphicsDevice _device;
        private readonly Buffer<EditorVertex> _linesBuffer;

        private readonly GeometricPrimitive _sphere;
        private readonly Color4 _red;
        private readonly Color4 _green;
        private readonly Color4 _blue;

        public Gizmo(DeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
            _device = deviceManager.Device;
            _editor = Editor.Instance;

            _red = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
            _green = new Color4(0.0f, 1.0f, 0.0f, 1.0f);
            _blue = new Color4(0.0f, 0.0f, 1.0f, 1.0f);

            // Initialize the gizmo geometry
            var v0 = new EditorVertex {Position = new Vector4(0.0f, 0.0f, 0.0f, 1.0f)};
            var vX = new EditorVertex {Position = new Vector4(1024.0f, 0.0f, 0.0f, 1.0f)};
            var vY = new EditorVertex {Position = new Vector4(0.0f, 1024.0f, 0.0f, 1.0f)};
            var vZ = new EditorVertex {Position = new Vector4(0.0f, 0.0f, -1024.0f, 1.0f)};
            var vertices = new[] {v0, vX, v0, vY, v0, vZ};

            _linesBuffer =
                Buffer.Vertex.New(_device, vertices, SharpDX.Direct3D11.ResourceUsage.Dynamic);

            _sphere = GeometricPrimitive.Sphere.New(_device, 128.0f, 16);

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
        }

        public void SetGizmoAxis(GizmoAxis axis)
        {
            _axis = axis;
        }

        public void MouseMoved(Matrix viewProjection, int x, int y, Keys modifierKeys)
        {
            if ((_editor.SelectedObject.HasValue) || (_axis == GizmoAxis.None))
                return;

            // For picking, I'll check first sphere/cubes bounding boxes and then eventually
            Room room = _editor.SelectedRoom;

            // First get the ray in 3D space from X, Y mouse coordinates
            Ray ray = Ray.GetPickRay(x, y, _device.Viewport,
                Matrix.Translation(Utils.PositionInWorldCoordinates(room.Position)) * viewProjection);

            Vector3 delta = new Vector3();
            switch (_axis)
            {
                case GizmoAxis.X:
                    {
                        Plane plane = new Plane(_position, Vector3.UnitY);
                        Vector3 intersection;
                        ray.Intersects(ref plane, out intersection);
                        delta = new Vector3(intersection.X - (_position.X + 1024.0f), 0, 0);
                    }
                    break;
                case GizmoAxis.Y:
                    {
                        Plane plane = new Plane(_position, Vector3.UnitX);
                        Vector3 intersection;
                        ray.Intersects(ref plane, out intersection);
                        delta = new Vector3(0, intersection.Y - (_position.Y + 1024.0f), 0);
                    }
                    break;
                case GizmoAxis.Z:
                    {
                        Plane plane = new Plane(_position, Vector3.UnitY);
                        Vector3 intersection;
                        ray.Intersects(ref plane, out intersection);
                        delta = new Vector3(0, 0, intersection.Z - (_position.Z - 1024.0f));
                    }
                    break;
            }

            EditorActions.MoveObject(_editor.SelectedRoom, _editor.SelectedObject.Value, delta, modifierKeys);
            return;
        }

        public PickingResultGizmo DoPicking(Ray ray)
        {
            if (!_drawGizmo)
                return null;

            float distance;

            BoundingSphere sphereX = new BoundingSphere(_position + Vector3.UnitX * 1024.0f, 64.0f);
            if (ray.Intersects(ref sphereX, out distance))
                return new PickingResultGizmo(distance, GizmoAxis.X);

            BoundingSphere sphereY = new BoundingSphere(_position + Vector3.UnitY * 1024.0f, 64.0f);
            if (ray.Intersects(ref sphereY, out distance))
                return new PickingResultGizmo(distance, GizmoAxis.Y);

            BoundingSphere sphereZ = new BoundingSphere(_position - Vector3.UnitZ * 1024.0f, 64.0f);
            if (ray.Intersects(ref sphereZ, out distance))
                return new PickingResultGizmo(distance, GizmoAxis.Z);

            return null;
        }

        public void Draw(Matrix viewProjection)
        {
            if (_drawGizmo)
                return;

            int TODO;
            _device.Clear(ClearOptions.DepthBuffer, Color4.White, 1.0f, 0);

            _device.SetRasterizerState(_rasterizerWireframe);
            _device.SetVertexBuffer(_linesBuffer);
            _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _linesBuffer));

            var solidEffect = _deviceManager.Effects["Solid"];

            var model = Matrix.Translation(_position) *
                        Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));
            var modelViewProjection = model * viewProjection;

            solidEffect.Parameters["ModelViewProjection"].SetValue(modelViewProjection);
            solidEffect.Parameters["SelectionEnabled"].SetValue(false);

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

            // X axis sphere
            model = Matrix.Translation(_position + Vector3.UnitX * 1024.0f) *
                    Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
            solidEffect.Parameters["Color"].SetValue(_red);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _device.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);

            // Y axis sphere
            model = Matrix.Translation(_position + Vector3.UnitY * 1024.0f) *
                    Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
            solidEffect.Parameters["Color"].SetValue(_green);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _device.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);

            // Z axis sphere
            model = Matrix.Translation(_position - Vector3.UnitZ * 1024.0f) *
                    Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * viewProjection);
            solidEffect.Parameters["Color"].SetValue(_blue);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _device.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);
        }
    }
}
