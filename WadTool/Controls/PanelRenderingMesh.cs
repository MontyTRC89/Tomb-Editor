using SharpDX.Toolkit.Graphics;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.Graphics.Primitives;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool.Controls
{
    public class PanelRenderingMesh : RenderingPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Configuration { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; } = new ArcBallCamera(new Vector3(0.0f, 1536.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 1024.0f, 100, 1000000, (float)Math.PI / 4.0f);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadMesh Mesh
        {
            get { return _mesh; }
            set
            {
                if (_mesh == value)
                    return;

                _mesh = value;
                CurrentVertex = -1;
            }
        }
        private WadMesh _mesh;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGrid
        {
            get { return _drawGrid; }
            set
            {
                if (_drawGrid == value) return;
                _drawGrid = value;
                Invalidate();
            }
        }
        private bool _drawGrid;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawVertices
        {
            get { return _drawVertices; }
            set
            {
                if (_drawVertices == value) return;
                _drawVertices = value;
                Invalidate();
            }
        }
        private bool _drawVertices;

        public int CurrentVertex
        {
            get { return _currentVertex; }
            set
            {
                if (_currentVertex == value)
                    return;

                if (Mesh.VerticesPositions.Count > value)
                    _currentVertex = value;
                else
                    _currentVertex = -1;

                if (DrawVertices)
                    Invalidate();

                _tool.MeshEditorVertexChanged(CurrentVertex);
            }
        }
        private int _currentVertex;

        // General state
        private WadToolClass _tool;

        // Interaction state
        private float _lastX;
        private float _lastY;

        // Legacy rendering state
        private GraphicsDevice _device;
        private RasterizerState _rasterizerWireframe;
        private VertexInputLayout _layout;
        private GeometricPrimitive _plane;
        private GeometricPrimitive _littleSphere;
        private WadRenderer _wadRenderer;

        // Rendering state
        private const float _littleSphereRadius = 128.0f;

        protected override Vector4 ClearColor => Configuration.RenderingItem_BackgroundColor;

        public void InitializeRendering(WadToolClass tool, DeviceManager deviceManager)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            Configuration = tool.Configuration;

            base.InitializeRendering(deviceManager.Device, tool.Configuration.RenderingItem_Antialias);
            _tool = tool;

            // Legacy rendering
            {
                _device = deviceManager.___LegacyDevice;
                _wadRenderer = new WadRenderer(deviceManager.___LegacyDevice);
                new BasicEffect(_device); // This effect is used for editor special meshes like sinks, cameras, light meshes, etc
                _rasterizerWireframe = RasterizerState.New(_device, new SharpDX.Direct3D11.RasterizerStateDescription
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
                });
                _plane = GeometricPrimitive.GridPlane.New(_device, 8, 4);
                _littleSphere = GeometricPrimitive.Sphere.New(_device, 2 * _littleSphereRadius, 8);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rasterizerWireframe?.Dispose();
                _plane?.Dispose();
                _littleSphere?.Dispose();
                _wadRenderer?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnDraw()
        {
            // To make sure things are in a defined state for legacy rendering...
            ((TombLib.Rendering.DirectX11.Dx11RenderingSwapChain)SwapChain).BindForce();
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            _device.SetDepthStencilState(_device.DepthStencilStates.Default);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
            _device.SetBlendState(_device.BlendStates.Opaque);

            Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);

            Effect solidEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Solid"];

            _wadRenderer.Dispose();
            if (Mesh != null)
            {
                // TODO Keep data on GPU, optimize data upload
                // Use new renderer
                var mesh = _wadRenderer.GetStatic(new WadStatic(new WadStaticId(0)) { Mesh = Mesh });
                var effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];
                var world = Matrix4x4.Identity;

                effect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                effect.Parameters["Color"].SetValue(Vector4.One);
                effect.Parameters["StaticLighting"].SetValue(false);
                effect.Parameters["ColoredVertices"].SetValue(false);
                effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                effect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                effect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                effect.Techniques[0].Passes[0].Apply();

                foreach (var mesh_ in mesh.Meshes)
                {
                    _device.SetVertexBuffer(0, mesh_.VertexBuffer);
                    _device.SetIndexBuffer(mesh_.IndexBuffer, true);
                    _layout = VertexInputLayout.FromBuffer(0, mesh_.VertexBuffer);
                    _device.SetVertexInputLayout(_layout);

                    foreach (var submesh in mesh_.Submeshes)
                        _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.MeshBaseIndex);
                }
            }

            if (DrawVertices)
            {
                _device.SetVertexBuffer(0, _littleSphere.VertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _littleSphere.VertexBuffer));
                _device.SetIndexBuffer(_littleSphere.IndexBuffer, true);
                solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());

                bool hasDrawnCurrentVertex = true;

                for (int i = 0; i < _mesh.VerticesPositions.Count; i++)
                {
                    if (hasDrawnCurrentVertex)
                    {
                        solidEffect.Parameters["Color"].SetValue(new Vector4(1, 1, 0, 1));
                        hasDrawnCurrentVertex = false;
                    }

                    if (i == _currentVertex)
                    {
                        solidEffect.Parameters["Color"].SetValue(new Vector4(1, 0, 0, 1));
                        hasDrawnCurrentVertex = true;
                    }

                    solidEffect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);
                }
            }

            if (DrawGrid)
            {
                _device.SetRasterizerState(_rasterizerWireframe);

                // Draw the grid
                _device.SetVertexBuffer(0, _plane.VertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _plane.VertexBuffer));
                _device.SetIndexBuffer(_plane.IndexBuffer, true);

                solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                solidEffect.Parameters["Color"].SetValue(Vector4.One);
                solidEffect.Techniques[0].Passes[0].Apply();

                _device.Draw(PrimitiveType.LineList, _plane.VertexBuffer.ElementCount);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            // Make this control able to receive scroll and key board events...
            base.OnMouseEnter(e);
            Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Camera.Zoom(-e.Delta * _tool.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _lastX = e.X;
            _lastY = e.Y;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                float deltaX = (e.X - _lastX) / Height;
                float deltaY = (e.Y - _lastY) / Height;

                _lastX = e.X;
                _lastY = e.Y;

                if (e.Button == MouseButtons.Right)
                {
                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                        Camera.Zoom(-deltaY * _tool.Configuration.RenderingItem_NavigationSpeedMouseZoom);
                    else if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                        Camera.Rotate(deltaX * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate,
                                     -deltaY * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate);
                }
                if ((e.Button == MouseButtons.Right && (ModifierKeys & Keys.Shift) == Keys.Shift) ||
                     e.Button == MouseButtons.Middle)
                    Camera.MoveCameraPlane(new Vector3(deltaX, deltaY, 0) * _tool.Configuration.RenderingItem_NavigationSpeedMouseTranslate);

                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (DrawVertices)
                TryPickVertex(e.X, e.Y);
        }

        private void TryPickVertex(float x, float y)
        {
            var matrix = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            var ray = Ray.GetPickRay(new Vector2(x, y), matrix, ClientSize.Width, ClientSize.Height);

            float distance = float.MaxValue;
            int candidate = -1;

            for (int i = 0; i < _mesh.VerticesPositions.Count; i++)
            {
                var vertex = _mesh.VerticesPositions[i];
                var sphere = new BoundingSphere(vertex, _littleSphereRadius);
                float newDistance;

                if (Collision.RayIntersectsSphere(ray, sphere, out newDistance))
                {
                    if (newDistance < distance || candidate == -1)
                    {
                        distance = newDistance;
                        candidate = i;
                    }
                }

                if (candidate != -1)
                    CurrentVertex = candidate;
            }
        }
    }
}
