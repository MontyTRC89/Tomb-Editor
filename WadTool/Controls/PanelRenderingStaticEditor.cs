﻿using DarkUI.Forms;
using SharpDX.Toolkit.Graphics;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Graphics;
using TombLib.Graphics.Primitives;
using TombLib.Utils;
using TombLib.Wad;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace WadTool.Controls
{
    public class PanelRenderingStaticEditor : Panel
    {
        public enum StaticEditorAction
        {
            Normal,
            PlaceLight
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadStatic Static { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadLight SelectedLight { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawVisibilityBox { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawCollisionBox { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGrid { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGizmo { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawLights { get; set; }

        public StaticEditorAction Action { get; set; }

        public Matrix4x4 GizmoTransform
        {
            get
            {
                return Matrix4x4.CreateScale(StaticScale) *
                       Matrix4x4.CreateFromYawPitchRoll(StaticRotation.Y, StaticRotation.X, StaticRotation.Z) *
                       Matrix4x4.CreateTranslation(StaticPosition);
            }
        }

        private GraphicsDevice _device;
        private DeviceManager _deviceManager;
        private SwapChainGraphicsPresenter _presenter;
        private RasterizerState _rasterizerWireframe;
        private WadToolClass _tool;
        private float _lastX;
        private float _lastY;
        private SpriteBatch _spriteBatch;
        private GizmoStaticEditor _gizmo;
        private GizmoStaticEditorLight _gizmoLight;
        private GeometricPrimitive _plane;
        private GeometricPrimitive _cube;
        private GeometricPrimitive _sphere;
        private GeometricPrimitive _littleSphere;
        private WadRenderer _wadRenderer;

        public Vector3 StaticPosition { get; set; } = Vector3.Zero;
        public Vector3 StaticRotation { get; set; } = Vector3.Zero;
        public float StaticScale { get; set; } = 1.0f;

        private static readonly Vector4 _red = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        private static readonly Vector4 _green = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        private static readonly Vector4 _blue = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

        private Buffer<SolidVertex> _vertexBufferVisibility;
        private Buffer<SolidVertex> _vertexBufferCollision;

        public void InitializePanel(WadToolClass tool, DeviceManager deviceManager)
        {
            _tool = tool;
            _device = deviceManager.Device;
            _deviceManager = deviceManager;
            _wadRenderer = new WadRenderer(_device);

            // Initialize the viewport, after the panel is added and sized on the form
            var pp = new PresentationParameters
            {
                BackBufferFormat = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                BackBufferWidth = ClientSize.Width,
                BackBufferHeight = ClientSize.Height,
                DepthStencilFormat = DepthFormat.Depth24Stencil8,
                DeviceWindowHandle = this,
                IsFullScreen = false,
                MultiSampleCount = MSAALevel.None,
                PresentationInterval = PresentInterval.Immediate,
                RenderTargetUsage = SharpDX.DXGI.Usage.RenderTargetOutput | SharpDX.DXGI.Usage.BackBuffer,
                Flags = SharpDX.DXGI.SwapChainFlags.None
            };

            _presenter = new SwapChainGraphicsPresenter(_device, pp);

            Camera = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 0, 1000000, (float)Math.PI / 4.0f);

            // This effect is used for editor special meshes like sinks, cameras, light meshes, etc
            new BasicEffect(_device);

            // Initialize the rasterizer state for wireframe drawing
            SharpDX.Direct3D11.RasterizerStateDescription renderStateDesc =
                new SharpDX.Direct3D11.RasterizerStateDescription
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

            _spriteBatch = new SpriteBatch(_device);
            _gizmo = new GizmoStaticEditor(_tool.Configuration, _device, _deviceManager.Effects["Solid"], this);
            _gizmoLight = new GizmoStaticEditorLight(_tool.Configuration, _device, _deviceManager.Effects["Solid"], this);
            _plane = GeometricPrimitive.GridPlane.New(_device, 8, 4);
            _cube = GeometricPrimitive.LinesCube.New(_device, 1024.0f, 1024.0f, 1024.0f);
            _littleSphere = GeometricPrimitive.Sphere.New(_device, 2 * 128.0f, 8);
            _sphere = GeometricPrimitive.Sphere.New(_device, 1024.0f, 6);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _spriteBatch?.Dispose();
                _gizmo?.Dispose();
                _gizmoLight?.Dispose();
                _plane?.Dispose();
                _cube?.Dispose();
                _sphere?.Dispose();
                _littleSphere?.Dispose();
                _presenter?.Dispose();
                _rasterizerWireframe?.Dispose();
                _wadRenderer?.Dispose();
                _vertexBufferVisibility?.Dispose();
                _vertexBufferCollision?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (_device == null || _presenter == null)
                e.Graphics.FillRectangle(Brushes.White, ClientRectangle);
            // Don't paint the background
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Draw();
        }

        private Buffer<SolidVertex> GetVertexBufferFromBoundingBox(BoundingBox box)
        {
            var p0 = new SolidVertex(new Vector3(box.Minimum.X, box.Minimum.Y, box.Minimum.Z));
            var p1 = new SolidVertex(new Vector3(box.Maximum.X, box.Minimum.Y, box.Minimum.Z));
            var p2 = new SolidVertex(new Vector3(box.Maximum.X, box.Minimum.Y, box.Maximum.Z));
            var p3 = new SolidVertex(new Vector3(box.Minimum.X, box.Minimum.Y, box.Maximum.Z));
            var p4 = new SolidVertex(new Vector3(box.Minimum.X, box.Maximum.Y, box.Minimum.Z));
            var p5 = new SolidVertex(new Vector3(box.Maximum.X, box.Maximum.Y, box.Minimum.Z));
            var p6 = new SolidVertex(new Vector3(box.Maximum.X, box.Maximum.Y, box.Maximum.Z));
            var p7 = new SolidVertex(new Vector3(box.Minimum.X, box.Maximum.Y, box.Maximum.Z));

            var vertices = new[]
            {
                p4, p5, p5, p1, p1, p0, p0, p4,
                    p5, p6, p6, p2, p2, p1, p1, p5,
                    p2, p6, p6, p7, p7, p3, p3, p2,
                    p7, p4, p4, p0, p0, p3, p3, p7,
                    p7, p6, p6, p5, p5, p4, p4, p7,
                    p0, p1, p1, p2, p2, p3, p3, p0
            };

            return Buffer.New(_device, vertices, BufferFlags.VertexBuffer, SharpDX.Direct3D11.ResourceUsage.Default);
        }

        public void Draw()
        {
            if (_device == null || _presenter == null)
                return;

            _device.Presenter = _presenter;
            _device.SetViewports(new SharpDX.ViewportF(0, 0, ClientSize.Width, ClientSize.Height));
            _device.SetRenderTargets(_device.Presenter.DepthStencilBuffer,
                _device.Presenter.BackBuffer);
            _device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, SharpDX.Color.CornflowerBlue, 1.0f, 0);
            _device.SetDepthStencilState(_device.DepthStencilStates.Default);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
            _device.SetBlendState(_device.BlendStates.Opaque);

            Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);

            Effect solidEffect = _deviceManager.Effects["Solid"];

            if (Static != null)
            {
                var model = _wadRenderer.GetStatic(Static);

                var effect = _deviceManager.Effects["StaticModel"];

                var world = GizmoTransform;

                effect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                effect.Parameters["Color"].SetValue(Vector4.One);
                effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                effect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    var mesh = model.Meshes[i];

                    _device.SetVertexBuffer(0, mesh.VertexBuffer);
                    _device.SetIndexBuffer(mesh.IndexBuffer, true);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, mesh.VertexBuffer));

                    effect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                    effect.Techniques[0].Passes[0].Apply();

                    foreach (var submesh in mesh.Submeshes)
                        _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.MeshBaseIndex);
                }

                // Draw boxes
                if (DrawVisibilityBox || DrawCollisionBox)
                {

                    if (DrawVisibilityBox)
                    {
                        if (_vertexBufferVisibility != null)
                            _vertexBufferVisibility.Dispose();
                        _vertexBufferVisibility = GetVertexBufferFromBoundingBox(Static.VisibilityBox);

                        _device.SetVertexBuffer(_vertexBufferVisibility);
                        _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferVisibility));
                        _device.SetIndexBuffer(null, false);

                        solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                        solidEffect.Parameters["Color"].SetValue(_green);
                        solidEffect.CurrentTechnique.Passes[0].Apply();

                        _device.Draw(PrimitiveType.LineList, _vertexBufferVisibility.ElementCount);
                    }

                    if (DrawCollisionBox)
                    {
                        if (_vertexBufferCollision != null)
                            _vertexBufferCollision.Dispose();
                        _vertexBufferCollision = GetVertexBufferFromBoundingBox(Static.CollisionBox);

                        _device.SetVertexBuffer(_vertexBufferCollision);
                        _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferCollision));
                        _device.SetIndexBuffer(null, false);

                        solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                        solidEffect.Parameters["Color"].SetValue(_red);
                        solidEffect.CurrentTechnique.Passes[0].Apply();

                        _device.Draw(PrimitiveType.LineList, _vertexBufferCollision.ElementCount);
                    }
                }
            }

            if (DrawGrid)
            {
                // Draw the grid
                _device.SetVertexBuffer(0, _plane.VertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _plane.VertexBuffer));
                _device.SetIndexBuffer(_plane.IndexBuffer, true);

                solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                solidEffect.Parameters["Color"].SetValue(Vector4.One);
                solidEffect.Techniques[0].Passes[0].Apply();

                _device.Draw(PrimitiveType.LineList, _plane.VertexBuffer.ElementCount);
            }

            if (DrawLights)
            {
                _device.SetRasterizerState(_rasterizerWireframe);

                foreach (var light in Static.Lights)
                {
                    // Draw the little sphere
                    _device.SetVertexBuffer(0, _littleSphere.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _littleSphere.VertexBuffer));
                    _device.SetIndexBuffer(_littleSphere.IndexBuffer, false);

                    var world = Matrix4x4.CreateTranslation(light.Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 0.0f, 1.0f));
                    solidEffect.Techniques[0].Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);

                    if (SelectedLight == light)
                    {
                        _device.SetVertexBuffer(0, _sphere.VertexBuffer);
                        _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _sphere.VertexBuffer));
                        _device.SetIndexBuffer(_sphere.IndexBuffer, false);

                        world = Matrix4x4.CreateScale(light.Radius * 2.0f) * Matrix4x4.CreateTranslation(light.Position);
                        solidEffect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                        solidEffect.Techniques[0].Passes[0].Apply();

                        _device.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);
                    }
                }

                _device.SetRasterizerState(_device.RasterizerStates.CullBack);
            }

            if (DrawGizmo)
            {
                // Draw the gizmo
                _gizmo.Draw(viewProjection);
            }

            if (SelectedLight != null)
            {
                // Draw the gizmo of selected light
                _gizmoLight.Draw(viewProjection);
            }

            // Draw debug strings
            _spriteBatch.Begin(SpriteSortMode.Immediate, _device.BlendStates.AlphaBlend);

            _spriteBatch.DrawString(_deviceManager.Font,
                                    "Position: " + StaticPosition,
                                    new SharpDX.Vector2(0, 0),
                                    SharpDX.Color.White);
            _spriteBatch.DrawString(_deviceManager.Font,
                                    "Rotation: " + StaticRotation.X * (180 / Math.PI),
                                    new SharpDX.Vector2(0, 18),
                                    SharpDX.Color.White);
            _spriteBatch.DrawString(_deviceManager.Font,
                                    "Scale: " + StaticScale,
                                    new SharpDX.Vector2(0, 36),
                                    SharpDX.Color.White);

            _spriteBatch.End();

            _device.Present();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_presenter != null)
            {
                _presenter.Resize(ClientSize.Width, ClientSize.Height, SharpDX.DXGI.Format.B8G8R8A8_UNorm);
                Invalidate();
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

        private Ray GetRay(float x, float y)
        {
            Size size = ClientSize;
            return SharpDxConversions.GetPickRay(new Vector2(x, y),
                Camera.GetViewProjectionMatrix(size.Width, size.Height), 0, 0, size.Width, size.Height);
        }

        private void PlaceLight(int x, int y)
        {
            // Get the intersection point between ray and the horizontal plane
            var ray = GetRay(x, y);
            var plane = new Plane(Vector3.UnitY, 0.0f);
            var point = Vector3.Zero;
            Collision.RayIntersectsPlane(ray, plane, out point);

            // Add the light at the intersection point
            var light = new WadLight(point, 1.0f, 0.5f);
            Static.Lights.Add(light);

            _tool.StaticLightsChanged();

            Action = StaticEditorAction.Normal;
            Invalidate();
        }

        private void DeleteLight(WadLight light)
        {
            if (DarkMessageBox.Show(Parent, "Do you really want to delete this light?", "Confirm delete",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Static.Lights.Remove(light);
                SelectedLight = null;
                _tool.StaticLightsChanged();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                if (Action != StaticEditorAction.PlaceLight)
                {
                    if (DrawGizmo)
                    {
                        var result = _gizmo.DoPicking(GetRay(e.X, e.Y));
                        if (result != null)
                        {
                            _gizmo.ActivateGizmo(result);
                            Invalidate();
                            return;
                        }
                    }

                    if (SelectedLight != null)
                    {
                        var result = _gizmoLight.DoPicking(GetRay(e.X, e.Y));
                        if (result != null)
                        {
                            _gizmoLight.ActivateGizmo(result);
                            Invalidate();
                            return;
                        }
                    }

                    // Try to pick lights
                    float minDistance = float.MaxValue;
                    SelectedLight = null;
                    foreach (var light in Static.Lights)
                    {
                        float distance = 0;
                        if (Collision.RayIntersectsSphere(GetRay(e.X, e.Y), new BoundingSphere(light.Position, 128.0f),
                                                          out distance))
                        {
                            if (distance <= minDistance)
                                minDistance = distance;
                            SelectedLight = light;
                        }
                    }

                    _tool.StaticSelectedLightChanged();

                    if (SelectedLight != null)
                        Invalidate();
                }
                else
                {
                    PlaceLight(e.X, e.Y);
                }
            }

            Invalidate();

            _lastX = e.X;
            _lastY = e.Y;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
                Action = StaticEditorAction.Normal;
            else if (e.KeyCode == Keys.Delete && SelectedLight != null)
                DeleteLight(SelectedLight);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(GetRay(e.X, e.Y))))
                Invalidate();
            if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), GetRay(e.X, e.Y)))
                Invalidate();

            if (_gizmoLight.GizmoUpdateHoverEffect(_gizmoLight.DoPicking(GetRay(e.X, e.Y))))
                Invalidate();
            if (_gizmoLight.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), GetRay(e.X, e.Y)))
                Invalidate();

            if (Action == StaticEditorAction.Normal)
                Cursor = Cursors.Default;
            else
                Cursor = Cursors.Cross;

            if (e.Button == MouseButtons.Right)
            {
                // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                float deltaX = (e.X - _lastX) / Height;
                float deltaY = (e.Y - _lastY) / Height;

                _lastX = e.X;
                _lastY = e.Y;

                if ((ModifierKeys & Keys.Control) == Keys.Control)
                    Camera.Zoom(-deltaY * _tool.Configuration.RenderingItem_NavigationSpeedMouseZoom);
                else if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                    Camera.MoveCameraPlane(new Vector3(-deltaX, -deltaY, 0) * _tool.Configuration.RenderingItem_NavigationSpeedMouseTranslate);
                else
                    Camera.Rotate(deltaX * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate,
                                  -deltaY * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate);
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_gizmo.MouseUp())
                Invalidate();

            if (_gizmoLight.MouseUp())
                Invalidate();
        }

        public void UpdateLights()
        {
            Static.Mesh.VerticesShades.Clear();

            for (int i = 0; i < Static.Mesh.VerticesPositions.Count; i++)
            {
                float newShade = Static.AmbientLight / 255.0f;
                foreach (var light in Static.Lights)
                {
                    // Get the light direction vector
                    var lightDirection = light.Position - Static.Mesh.VerticesPositions[i];
                    if (lightDirection.Length() > light.Radius * 1024)
                        continue;

                    var l = lightDirection / lightDirection.Length();

                    // Get the normal
                    var n = Static.Mesh.VerticesNormals[i] / Static.Mesh.VerticesNormals[i].Length();

                    // Calculate cosine
                    float dot = Vector3.Dot(n, l);
                    if (dot <= 0)
                        continue;

                    newShade += dot * light.Intensity * (1.0f - (light.Position - Static.Mesh.VerticesPositions[i]).Length() /
                                (light.Radius * 1024.0f));
                }

                Static.Mesh.VerticesShades.Add((short)((255.0f - newShade * 255.0f) * 8191 / 255));
            }

            _tool.StaticLightsChanged();
        }
    }
}
