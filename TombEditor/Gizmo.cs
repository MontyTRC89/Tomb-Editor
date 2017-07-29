using SharpDX;
using SharpDX.Toolkit.Graphics;
using TombEditor.Geometry;

namespace TombEditor
{
    public class Gizmo
    {
        public Vector3 Position { get; set; }
        public Matrix ViewProjection { private get; set; }

        private readonly Editor _editor;
        private readonly RasterizerState _rasterizerWireframe;

        // Geometry of the gizmo
        private readonly Buffer<EditorVertex> _linesBuffer;

        private readonly GeometricPrimitive _sphere;
        private readonly Color4 _red;
        private readonly Color4 _green;
        private readonly Color4 _blue;

        public Gizmo()
        {
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
                Buffer.Vertex.New(_editor.GraphicsDevice, vertices, SharpDX.Direct3D11.ResourceUsage.Dynamic);

            _sphere = GeometricPrimitive.Sphere.New(_editor.GraphicsDevice, 128.0f, 16);

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

            _rasterizerWireframe = RasterizerState.New(_editor.GraphicsDevice, renderStateDesc);
        }

        public void Draw()
        {
            _editor.GraphicsDevice.SetRasterizerState(_rasterizerWireframe);
            _editor.GraphicsDevice.SetVertexBuffer(_linesBuffer);
            _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _linesBuffer));

            var solidEffect = _editor.Effects["Solid"];

            var model = Matrix.Translation(Position) *
                        Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));
            var modelViewProjection = model * ViewProjection;

            solidEffect.Parameters["ModelViewProjection"].SetValue(modelViewProjection);
            solidEffect.Parameters["SelectionEnabled"].SetValue(false);

            // X axis
            solidEffect.Parameters["Color"].SetValue(_red);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _editor.GraphicsDevice.Draw(PrimitiveType.LineList, 2, 0);

            // Y axis
            solidEffect.Parameters["Color"].SetValue(_green);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _editor.GraphicsDevice.Draw(PrimitiveType.LineList, 2, 2);

            // Z axis
            solidEffect.Parameters["Color"].SetValue(_blue);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _editor.GraphicsDevice.Draw(PrimitiveType.LineList, 2, 4);

            _editor.GraphicsDevice.SetRasterizerState(_editor.GraphicsDevice.RasterizerStates.CullBack);
            _editor.GraphicsDevice.SetVertexBuffer(_sphere.VertexBuffer);
            _editor.GraphicsDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _sphere.VertexBuffer));
            _editor.GraphicsDevice.SetIndexBuffer(_sphere.IndexBuffer, _sphere.IsIndex32Bits);

            // X axis sphere
            model = Matrix.Translation(Position + Vector3.UnitX * 1024.0f) *
                    Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * ViewProjection);
            solidEffect.Parameters["Color"].SetValue(_red);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);

            // Y axis sphere
            model = Matrix.Translation(Position + Vector3.UnitY * 1024.0f) *
                    Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * ViewProjection);
            solidEffect.Parameters["Color"].SetValue(_green);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);

            // Z axis sphere
            model = Matrix.Translation(Position - Vector3.UnitZ * 1024.0f) *
                    Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.SelectedRoom.Position));
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * ViewProjection);
            solidEffect.Parameters["Color"].SetValue(_blue);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);
        }
    }
}
