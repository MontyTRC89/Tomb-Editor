using DarkUI.Controls;
using DarkUI.Forms;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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
    public class PanelRenderingSkeleton : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGrid { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGizmo { get; set; }

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
        private GizmoSkeletonEditor _gizmo;
        private GeometricPrimitive _plane;
        private GeometricPrimitive _cube;
        private GeometricPrimitive _sphere;
        private GeometricPrimitive _littleSphere;
        private WadRenderer _wadRenderer; // TODO Remove internal hack that destroys rendering encapsulation

        public Vector3 StaticPosition { get; set; } = Vector3.Zero;
        public Vector3 StaticRotation { get; set; } = Vector3.Zero;
        public float StaticScale { get; set; } = 1.0f;

        public List<WadMeshBoneNode> Skeleton { get; set; }
        public WadMeshBoneNode SelectedNode { get; set; }

        private static readonly Vector4 _red = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        private static readonly Vector4 _green = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        private static readonly Vector4 _blue = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

        private Buffer<SolidVertex> _vertexBufferVisibility;

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
            _gizmo = new GizmoSkeletonEditor(_tool, _tool.Configuration, _device, _deviceManager.Effects["Solid"], this);
            _plane = GeometricPrimitive.GridPlane.New(_device, 8, 4);
            _cube = GeometricPrimitive.LinesCube.New(_device, 1024.0f, 1024.0f, 1024.0f);
            _littleSphere = GeometricPrimitive.Sphere.New(_device, 2 * 128.0f, 8);
            _sphere = GeometricPrimitive.Sphere.New(_device, 1024.0f, 6);

            DrawGizmo = true;
            DrawGrid = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _presenter?.Dispose();
                _rasterizerWireframe?.Dispose();
                _spriteBatch?.Dispose();
                _gizmo?.Dispose();
                _plane?.Dispose();
                _cube?.Dispose();
                _sphere?.Dispose();
                _littleSphere?.Dispose();
                _wadRenderer?.Dispose();
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

            _wadRenderer.Dispose();
            if (Skeleton != null)
            {
                var effect = _deviceManager.Effects["Model"];

                effect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                foreach (var node in Skeleton)
                {
                    // TODO Keep data on GPU, optimize data upload
                    // Use new renderer
                    var mesh = _wadRenderer.GetStatic(new WadStatic(new WadStaticId(0)) { Mesh = node.WadMesh });

                    _device.SetVertexBuffer(0, mesh.VertexBuffer);
                    _device.SetIndexBuffer(mesh.IndexBuffer, true);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, mesh.VertexBuffer));

                    effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                    effect.Parameters["ModelViewProjection"].SetValue((node.GlobalTransform * viewProjection).ToSharpDX());
                    effect.Techniques[0].Passes[0].Apply();

                    foreach (var mesh_ in mesh.Meshes)
                        foreach (var submesh in mesh_.Submeshes)
                            _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.MeshBaseIndex);
                }

                // Draw box
                if (SelectedNode != null)
                {
                    if (_vertexBufferVisibility != null)
                        _vertexBufferVisibility.Dispose();
                    _vertexBufferVisibility = GetVertexBufferFromBoundingBox(SelectedNode.WadMesh.BoundingBox);

                    _device.SetVertexBuffer(_vertexBufferVisibility);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferVisibility));
                    _device.SetIndexBuffer(null, false);

                    solidEffect.Parameters["ModelViewProjection"].SetValue((SelectedNode.GlobalTransform * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_green);
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.Draw(PrimitiveType.LineList, _vertexBufferVisibility.ElementCount);
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

            if (DrawGizmo && SelectedNode != null)
            {
                // Draw the gizmo
                _gizmo.Draw(viewProjection);
            }

            // Draw debug strings
            if (SelectedNode != null)
            {
                Vector3 screenPos = SharpDxConversions.Project(SelectedNode.Centre - Vector3.UnitY * 128.0f,
                                    SelectedNode.GlobalTransform * viewProjection, 0, 0, ClientSize.Width, ClientSize.Height);
                _spriteBatch.Begin(SpriteSortMode.Immediate, _device.BlendStates.AlphaBlend);

                _spriteBatch.DrawString(_deviceManager.Font,
                                        "Name: " + SelectedNode.Bone.Name,
                                        new Vector2(screenPos.X, screenPos.Y).ToSharpDX(),
                                        SharpDX.Color.White);

                _spriteBatch.DrawString(_deviceManager.Font,
                                        "Local offset: " + SelectedNode.Bone.Translation,
                                        new Vector2(screenPos.X, screenPos.Y + 20.0f).ToSharpDX(),
                                        SharpDX.Color.White);

                _spriteBatch.End();
            }

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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                // Try to do gizmo picking
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

                // Try to do node picking
                WadMeshBoneNode foundNode = null;
                foreach (var node in Skeleton)
                {
                    float distance = 0;
                    float minDistance = float.PositiveInfinity;
                    if (DoNodePicking(GetRay(e.X, e.Y), node, out distance))
                    {
                        if (distance < minDistance)
                        {
                            distance = minDistance;
                            foundNode = node;
                        }
                    }
                }
                SelectedNode = foundNode;
            }

            Invalidate();

            _lastX = e.X;
            _lastY = e.Y;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(GetRay(e.X, e.Y))))
                Invalidate();
            if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), GetRay(e.X, e.Y)))
                Invalidate();

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
        }

        private bool DoNodePicking(Ray ray, WadMeshBoneNode node, out float nodeDistance)
        {
            nodeDistance = 0;

            // Transform view ray to object space space
            Matrix4x4 inverseObjectMatrix;
            if (!Matrix4x4.Invert(node.GlobalTransform, out inverseObjectMatrix))
                return false;
            Vector3 transformedRayPos = MathC.HomogenousTransform(ray.Position, inverseObjectMatrix);
            Vector3 transformedRayDestination = MathC.HomogenousTransform(ray.Position + ray.Direction, inverseObjectMatrix);
            Ray transformedRay = new Ray(transformedRayPos, transformedRayDestination - transformedRayPos);
            transformedRay.Direction = Vector3.Normalize(transformedRay.Direction);

            // Now do a ray - triangle intersection test
            bool hit = false;
            float minDistance = float.PositiveInfinity;
            /*
            _wadRenderer.Dispose();
            foreach (var submesh in node.Bone.Children.Select(bone => bone.Mesh))
            for (int k = 0; k < submesh.Value.Indices.Count; k += 3)
            {
                var mesh = _wadRenderer.GetStatic(new WadStatic(new WadStaticId(0)) { Mesh = node.WadMesh });

                Vector3 p1 = mesh.Vertices[submesh.Value.Indices[k]].Position;
                Vector3 p2 = mesh.Vertices[submesh.Value.Indices[k + 1]].Position;
                Vector3 p3 = mesh.Vertices[submesh.Value.Indices[k + 2]].Position;

                float distance;
                if (Collision.RayIntersectsTriangle(transformedRay, p1, p2, p3, out distance) && distance < minDistance)
                {
                    minDistance = distance;
                    hit = true;
                }
            }*/
            // TODO Avoid using the renderer for pickingData transforms need to be available in wad mesh without rendering.
            int TODO_DoNodePicking;

            if (hit)
            {
                nodeDistance = minDistance;
                return true;
            }
            else
                return false;
        }
    }
}
