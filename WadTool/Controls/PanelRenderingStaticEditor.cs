using DarkUI.Forms;
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
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.Wad;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace WadTool.Controls
{
    public class PanelRenderingStaticEditor : RenderingPanel
    {
        public enum StaticEditorAction
        {
            Normal,
            PlaceLight
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadStatic Static { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; } = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 0, 1000000, (float)Math.PI / 4.0f);
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawNormals { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector3 StaticPosition { get; set; } = Vector3.Zero;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector3 StaticRotation { get; set; } = Vector3.Zero;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float StaticScale { get; set; } = 1.0f;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        // General state
        private WadToolClass _tool;

        // Interaction state
        private float _lastX;
        private float _lastY;

        // Rendering state
        private RenderingTextureAllocator _fontTexture;
        private RenderingFont _fontDefault;

        // Legacy rendering state
        private GraphicsDevice _device;
        private DeviceManager _deviceManager;
        private RasterizerState _rasterizerWireframe;
        private GizmoStaticEditor _gizmo;
        private GizmoStaticEditorLight _gizmoLight;
        private GeometricPrimitive _plane;
        private GeometricPrimitive _sphere;
        private GeometricPrimitive _littleSphere;
        private WadRenderer _wadRenderer;
        private Buffer<SolidVertex> _vertexBufferVisibility;
        private Buffer<SolidVertex> _vertexBufferCollision;

        public void InitializeRendering(WadToolClass tool, DeviceManager deviceManager)
        {
            base.InitializeRendering(deviceManager.Device);
            _tool = tool;

            // Actual "InitializeRendering"
            _fontTexture = Device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(512, 512, 2) });
            _fontDefault = Device.CreateFont(new RenderingFont.Description { FontName = "Segoe UI", FontSize = 24, FontIsBold = true, TextureAllocator = _fontTexture });

            // Legacy rendering
            {
                _device = deviceManager.___LegacyDevice;
                _deviceManager = deviceManager;
                _wadRenderer = new WadRenderer(_device);
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
                _gizmo = new GizmoStaticEditor(_tool.Configuration, _device, _deviceManager.___LegacyEffects["Solid"], this);
                _gizmoLight = new GizmoStaticEditorLight(_tool.Configuration, _device, _deviceManager.___LegacyEffects["Solid"], this);
                _plane = GeometricPrimitive.GridPlane.New(_device, 8, 4);
                _littleSphere = GeometricPrimitive.Sphere.New(_device, 2 * 128.0f, 8);
                _sphere = GeometricPrimitive.Sphere.New(_device, 1024.0f, 6);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fontTexture?.Dispose();
                _fontDefault?.Dispose();
                _gizmo?.Dispose();
                _gizmoLight?.Dispose();
                _plane?.Dispose();
                _sphere?.Dispose();
                _littleSphere?.Dispose();
                _rasterizerWireframe?.Dispose();
                _wadRenderer?.Dispose();
                _vertexBufferVisibility?.Dispose();
                _vertexBufferCollision?.Dispose();
            }

            base.Dispose(disposing);
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

        protected override Vector4 ClearColor => new Vector4(0.39f, 0.58f, 0.93f, 1.0f);

        protected override void OnDraw()
        {
            // To make sure things are in a defined state for legacy rendering...
            ((TombLib.Rendering.DirectX11.Dx11RenderingSwapChain)SwapChain).BindForce();
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            _device.SetDepthStencilState(_device.DepthStencilStates.Default);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
            _device.SetBlendState(_device.BlendStates.Opaque);

            Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);

            Effect solidEffect = _deviceManager.___LegacyEffects["Solid"];

            if (Static != null)
            {
                var model = _wadRenderer.GetStatic(Static);

                var effect = _deviceManager.___LegacyEffects["StaticModel"];

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
                    {
                        _device.Draw(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                    }

                    //foreach (var submesh in mesh.Submeshes)
                    //    _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.MeshBaseIndex);
                }

                // Draw boxes
                if (DrawVisibilityBox || DrawCollisionBox)
                {

                    if (DrawVisibilityBox)
                    {
                        _vertexBufferVisibility?.Dispose();
                        _vertexBufferVisibility = GetVertexBufferFromBoundingBox(Static.VisibilityBox);

                        _device.SetVertexBuffer(_vertexBufferVisibility);
                        _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferVisibility));
                        _device.SetIndexBuffer(null, false);

                        solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                        solidEffect.CurrentTechnique.Passes[0].Apply();

                        _device.Draw(PrimitiveType.LineList, _vertexBufferVisibility.ElementCount);
                    }

                    if (DrawCollisionBox)
                    {
                        _vertexBufferCollision?.Dispose();
                        _vertexBufferCollision = GetVertexBufferFromBoundingBox(Static.CollisionBox);

                        _device.SetVertexBuffer(_vertexBufferCollision);
                        _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferCollision));
                        _device.SetIndexBuffer(null, false);

                        solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                        solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                        solidEffect.CurrentTechnique.Passes[0].Apply();

                        _device.Draw(PrimitiveType.LineList, _vertexBufferCollision.ElementCount);
                    }
                }

                // Draw normals
                if (DrawNormals)
                {
                    var lines = new List<SolidVertex>();
                    for (int i = 0; i < Static.Mesh.VerticesNormals.Count; i++)
                    {
                        var p = Vector3.Transform(Static.Mesh.VerticesPositions[i], world);
                        var n = Vector3.TransformNormal(Static.Mesh.VerticesNormals[i] /
                            Static.Mesh.VerticesNormals[i].Length(), world);

                        var v = new SolidVertex();
                        v.Position = p;
                        v.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                        lines.Add(v);

                        v = new SolidVertex();
                        v.Position = p + n * 256.0f;
                        v.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                        lines.Add(v);
                    }

                    Buffer<SolidVertex> bufferLines = Buffer.New(_device, lines.ToArray(), BufferFlags.VertexBuffer, SharpDX.Direct3D11.ResourceUsage.Default);
                    _device.SetVertexBuffer(bufferLines);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, bufferLines));
                    _device.SetIndexBuffer(null, false);

                    solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.Draw(PrimitiveType.LineList, bufferLines.ElementCount);
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
                SwapChain.ClearDepth();
                _gizmo.Draw(viewProjection);
            }

            if (SelectedLight != null)
            {
                // Draw the gizmo of selected light
                SwapChain.ClearDepth();
                _gizmoLight.Draw(viewProjection);
            }

            // Draw debug strings
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState(); // To make sure SharpDx.Toolkit didn't change settings.
            SwapChain.RenderText(new Text
            {
                Font = _fontDefault,
                PixelPos = new Vector2(5, 5),
                Alignment = new Vector2(0, 0),
                String =
                    "Position: " + StaticPosition +
                    "\nRotation: " + StaticRotation.X * (180 / Math.PI) +
                    "\nScale: " + StaticScale
            });
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
            return Ray.GetPickRay(new Vector2(x, y), Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ClientSize.Width, ClientSize.Height);
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

        public void DeleteLight(WadLight light)
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

            Matrix4x4 world = Matrix4x4.CreateTranslation(StaticPosition);

            for (int i = 0; i < Static.Mesh.VerticesPositions.Count; i++)
            {
                float newShade = Static.AmbientLight / 255.0f;
                foreach (var light in Static.Lights)
                {
                    // Transform current vertex and normal
                    var p = Vector3.Transform(Static.Mesh.VerticesPositions[i], world);
                    var n = Vector3.TransformNormal(Static.Mesh.VerticesNormals[i] / Static.Mesh.VerticesNormals[i].Length(), world);

                    // Get the light direction vector
                    var lightDirection = light.Position - p;
                    if (lightDirection.Length() > light.Radius * 1024)
                        continue;

                    var l = lightDirection / lightDirection.Length();

                    // Calculate cosine
                    float dot = Vector3.Dot(n, l);
                    if (dot <= 0)
                        continue;

                    newShade += dot * light.Intensity * (1.0f - (light.Position - p).Length() /
                                (light.Radius * 1024.0f));
                }

                Static.Mesh.VerticesShades.Add((short)((255.0f - newShade * 255.0f) * 8191 / 255));
            }

            _tool.StaticLightsChanged();
        }
    }
}
