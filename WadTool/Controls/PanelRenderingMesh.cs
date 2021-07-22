using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.Graphics.Primitives;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool.Controls
{
    public class PanelRenderingMesh : RenderingPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; } = new ArcBallCamera(new Vector3(0.0f, 0.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 512.0f, 100, 1000000, (float)Math.PI / 4.0f);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadMesh Mesh
        {
            get { return _mesh; }
            set
            {
                if (_mesh == value)
                    return;

                _mesh = value;
                ResetCamera();
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

                if (_tool.Configuration.MeshEditor_DrawVertices)
                    Invalidate();

                _tool.MeshEditorVertexChanged(CurrentVertex);
            }
        }
        private int _currentVertex = -1;

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
        private RenderingTextureAllocator _fontTexture;
        private RenderingFont _fontDefault;

        // Constants
        private const float _littleSphereRadius = 1.0f;
        private readonly List<int> _oldLaraHairIndices = new List<int>() { 37, 38, 39, 40 };
        private readonly List<int> _youngLaraHairIndices = new List<int>() { 68, 69, 70, 71, 76, 77, 78, 79 };

        protected override Vector4 ClearColor => _tool.Configuration.RenderingItem_BackgroundColor;

        public void InitializeRendering(WadToolClass tool, DeviceManager deviceManager)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            base.InitializeRendering(deviceManager.Device, tool.Configuration.RenderingItem_Antialias);
            _tool = tool;

            // Legacy rendering
            {
                _device = deviceManager.___LegacyDevice;
                _wadRenderer = new WadRenderer(deviceManager.___LegacyDevice);

                _fontTexture = deviceManager.Device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(512, 512, 2) });
                _fontDefault = deviceManager.Device.CreateFont(new RenderingFont.Description
                {
                    FontName = _tool.Configuration.Rendering3D_FontName,
                    FontSize = _tool.Configuration.Rendering3D_FontSize,
                    FontIsBold = _tool.Configuration.Rendering3D_FontIsBold,
                    TextureAllocator = _fontTexture
                });

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
                _littleSphere = GeometricPrimitive.Sphere.New(_device, 2 * _littleSphereRadius, 4);
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

            if (_tool.Configuration.MeshEditor_DrawWireframe)
                _device.SetRasterizerState(_rasterizerWireframe);
            else
                _device.SetRasterizerState(_device.RasterizerStates.CullBack);

            _device.SetDepthStencilState(_device.DepthStencilStates.Default);
            _device.SetBlendState(_device.BlendStates.Opaque);

            var viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            var solidEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Solid"];

            _wadRenderer.Dispose();

            if (Mesh != null)
            {
                var mesh   = _wadRenderer.GetStatic(new WadStatic(new WadStaticId(0)) { Mesh = Mesh });
                var effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];
                var world  = Matrix4x4.Identity;

                effect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                effect.Parameters["Color"].SetValue(Vector4.One);
                effect.Parameters["StaticLighting"].SetValue(false);
                effect.Parameters["ColoredVertices"].SetValue(false);
                effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                effect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);
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

                if (_tool.Configuration.MeshEditor_DrawVertices)
                {
                    _device.SetDepthStencilState(_device.DepthStencilStates.Default);
                    _device.SetRasterizerState(_device.RasterizerStates.CullBack);

                    _device.SetVertexBuffer(_littleSphere.VertexBuffer);
                    _device.SetVertexInputLayout(_littleSphere.InputLayout);
                    _device.SetIndexBuffer(_littleSphere.IndexBuffer, _littleSphere.IsIndex32Bits);

                    // HACK: Determine remappable vertices.
                    // For more info: https://www.tombraiderforums.com/showthread.php?t=132749

                    int safeIndex = int.MaxValue;
                    if (_tool.DestinationWad.GameVersion != TRVersion.Game.TombEngine)
                    {
                        if (_mesh.VerticesPositions.Count <= 255)
                            safeIndex = 127;
                        else
                        {
                            var step = (Math.Truncate((float)_mesh.VerticesPositions.Count / 256.0f) - 1) * 256.0f;
                            safeIndex = _mesh.VerticesPositions.Count - (int)step - 1;
                            if (safeIndex > 127) safeIndex = 127;
                        }
                    }

                    var textToDraw = new List<Text>();

                    for (int i = 0; i < _mesh.VerticesPositions.Count; i++)
                    {
                        var posMatrix = Matrix4x4.Identity * Matrix4x4.CreateTranslation(_mesh.VerticesPositions[i]) * viewProjection;
                        solidEffect.Parameters["ModelViewProjection"].SetValue((posMatrix).ToSharpDX());

                        var selected = (i == _currentVertex);
                        if (selected)
                        {
                            solidEffect.Parameters["Color"].SetValue(new Vector4(1, 0, 0, 1));
                        }
                        else
                        {
                            if (i <= safeIndex)
                                solidEffect.Parameters["Color"].SetValue(new Vector4(0, 0.7f, 1, 1));
                            else
                                solidEffect.Parameters["Color"].SetValue(new Vector4(1, 0.7f, 0, 1));
                        }

                        solidEffect.Techniques[0].Passes[0].Apply();
                        _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);

                        if (_tool.Configuration.MeshEditor_DrawVertexNumbers || selected)
                        {
                            // Only draw texts which are actually visible
                            if (posMatrix.TransformPerspectively(new Vector3()).Z <= 1.0f)
                            {
                                textToDraw.Add(new Text
                                {
                                    Font = _fontDefault,
                                    TextAlignment = new Vector2(0.0f, 0.0f),
                                    PixelPos = new VectorInt2(2, -2),
                                    Pos = posMatrix.TransformPerspectively(new Vector3()).To2(),
                                    Overlay = _tool.Configuration.Rendering3D_DrawFontOverlays,
                                    String = i.ToString()
                                });
                            }
                        }
                    }

                    _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                    _device.SetBlendState(_device.BlendStates.AlphaBlend);

                    if (textToDraw.Count > 0)
                        SwapChain.RenderText(textToDraw);
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

            if (_tool.Configuration.MeshEditor_DrawVertices && e.Button == MouseButtons.Left)
                TryPickVertex(e.X, e.Y);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button == MouseButtons.Right)
                ResetCamera();
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
                else
                    CurrentVertex = -1;
            }
        }

        public void ResetCamera()
        {
            // Smart reset camera which fits an object into window. Later reuse for TE item preview!

            var center = Mesh != null ? Mesh.BoundingBox.Center : Vector3.Zero;
            var dims = Mesh != null ? new Vector2(Mesh.BoundingBox.Size.X, Mesh.BoundingBox.Size.Y) : Vector2.Zero;
            var screenSpace = new Vector2(Width, Height);
            var ratio = dims / screenSpace;
            var scale = 512.0f;

            // Fit mesh into window depending on ratio prevalence
            if (ratio.X > ratio.Y)
                scale = dims.X / screenSpace.X;
            else
                scale = dims.Y / screenSpace.Y;

            // Multiply distance by world units plus quarter-margin
            scale *= Level.WorldUnit;

            Camera = new ArcBallCamera(center, 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, scale, 100, 1000000, (float)Math.PI / 4.0f);
            Invalidate();
        }
    }
}
