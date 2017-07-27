using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using TombEditor.Geometry;

namespace TombEditor
{
    public class Gizmo
    {
        public Vector3 Position { get; set; }
        public Matrix ViewProjection { get; set; }

        private Editor _editor;
        private RasterizerState _rasterizerWireframe;

        // Geometry of the gizmo
        private Buffer<EditorVertex> _linesBuffer;
        private GeometricPrimitive _sphere;
        private SharpDX.Color4 _red;
        private SharpDX.Color4 _green;
        private SharpDX.Color4 _blue;

        public Gizmo()
        {
            _editor = Editor.Instance;

            _red = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
            _green = new Color4(0.0f, 1.0f, 0.0f, 1.0f);
            _blue = new Color4(0.0f, 0.0f, 1.0f, 1.0f);

            // Initialize the gizmo geometry
            EditorVertex v0 = new EditorVertex();
            v0.Position = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

            EditorVertex vX = new EditorVertex();
            vX.Position = new Vector4(1024.0f, 0.0f, 0.0f, 1.0f);

            EditorVertex vY = new EditorVertex();
            vY.Position = new Vector4(0.0f, 1024.0f, 0.0f, 1.0f);

            EditorVertex vZ = new EditorVertex();
            vZ.Position = new Vector4(0.0f, 0.0f, -1024.0f, 1.0f);

            EditorVertex[] vertices = new EditorVertex[] { v0, vX, v0, vY, v0, vZ };

            _linesBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New<EditorVertex>(_editor.GraphicsDevice, vertices, SharpDX.Direct3D11.ResourceUsage.Dynamic);

            _sphere = GeometricPrimitive.Sphere.New(_editor.GraphicsDevice, 128.0f, 16);

            // Initialize the rasterizer state for wireframe drawing
            SharpDX.Direct3D11.RasterizerStateDescription renderStateDesc = new SharpDX.Direct3D11.RasterizerStateDescription
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

            Effect solidEffect = _editor.Effects["Solid"];

            Matrix model = Matrix.Translation(Position) * Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.Level.Rooms[_editor.RoomIndex].Position));
            Matrix modelViewProjection = model * ViewProjection;

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
            model = Matrix.Translation(Position + Vector3.UnitX*1024.0f) * Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.Level.Rooms[_editor.RoomIndex].Position));
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * ViewProjection);
            solidEffect.Parameters["Color"].SetValue(_red);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);

            // Y axis sphere
            model = Matrix.Translation(Position + Vector3.UnitY * 1024.0f) * Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.Level.Rooms[_editor.RoomIndex].Position));
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * ViewProjection);
            solidEffect.Parameters["Color"].SetValue(_green);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);

            // Z axis sphere
            model = Matrix.Translation(Position - Vector3.UnitZ * 1024.0f) * Matrix.Translation(Utils.PositionInWorldCoordinates(_editor.Level.Rooms[_editor.RoomIndex].Position));
            solidEffect.Parameters["ModelViewProjection"].SetValue(model * ViewProjection);
            solidEffect.Parameters["Color"].SetValue(_blue);
            solidEffect.CurrentTechnique.Passes[0].Apply();

            _editor.GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);

        }
    }
}
