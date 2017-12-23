using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool.Controls
{
    public class PanelRenderingStaticMeshEditor : Panel
    {
        public WadStatic StaticMesh { get; set; }
        public ArcBallCamera Camera { get; set; }

        public bool DrawVisibilityBox { get; set; }
        public bool DrawCollisionBox { get; set; }
        public bool DrawGrid { get; set; }
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
        private SwapChainGraphicsPresenter _presenter;
        private RasterizerState _rasterizerWireframe;
        private WadToolClass _tool;
        private VertexInputLayout _layout;
        private float _lastX;
        private float _lastY;
        private SpriteBatch _spriteBatch;
        private GizmoStaticMeshEditor _gizmo;
        private GeometricPrimitive _plane;
        private GeometricPrimitive _cube;

        public Vector3 StaticPosition { get; set; } = Vector3.Zero;
        public Vector3 StaticRotation { get; set; } = Vector3.Zero;
        public float StaticScale { get; set; } = 1.0f;

        private static readonly Vector4 _red = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        private static readonly Vector4 _green = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        private static readonly Vector4 _blue = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

        private Buffer<SolidVertex> _vertexBufferVisibility;
        private Buffer<SolidVertex> _vertexBufferCollision;

        public void InitializePanel(GraphicsDevice device)
        {
            _device = device;

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

            _tool = WadToolClass.Instance;
            _spriteBatch = new SpriteBatch(_tool.Device);
            _gizmo = new GizmoStaticMeshEditor(_tool.Device, _tool.Effects["Solid"], this);
            _plane = GeometricPrimitive.GridPlane.New(_tool.Device, 8, 4);
            _cube = GeometricPrimitive.LinesCube.New(_tool.Device, 1024.0f, 1024.0f, 1024.0f);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (_device == null || _presenter == null)
                e.Graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_device == null || _presenter == null)
                e.Graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
            else
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

            var vertices = new SolidVertex[]
            {
                p4, p5, p5, p1, p1, p0, p0, p4,
                    p5, p6, p6, p2, p2, p1, p1, p5,
                    p2, p6, p6, p7, p7, p3, p3, p2,
                    p7, p4, p4, p0, p0, p3, p3, p7,
                    p7, p6, p6, p5, p5, p4, p4, p7,
                    p0, p1, p1, p2, p2, p3, p3, p0
            };

            return Buffer<SolidVertex>.New(_tool.Device, vertices, BufferFlags.VertexBuffer, SharpDX.Direct3D11.ResourceUsage.Default);
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

            Effect solidEffect = _tool.Effects["Solid"];

            if (StaticMesh != null)
            {
                var model = _tool.DestinationWad.DirectXStatics[StaticMesh.ObjectID];

                Effect mioEffect = _tool.Effects["StaticModel"];

                var world = GizmoTransform;

                mioEffect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());

                mioEffect.Parameters["Color"].SetValue(Vector4.One);

                mioEffect.Parameters["Texture"].SetResource(_tool.DestinationWad.DirectXTexture);
                mioEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                _device.SetVertexBuffer(0, model.VertexBuffer);
                _device.SetIndexBuffer(model.IndexBuffer, true);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    StaticMesh mesh = model.Meshes[i];

                    _layout = VertexInputLayout.FromBuffer<StaticVertex>(0, mesh.VertexBuffer);
                    _device.SetVertexInputLayout(_layout);

                    mioEffect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                    mioEffect.Techniques[0].Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, mesh.NumIndices, mesh.BaseIndex);
                }

                // Draw boxes
                if (DrawVisibilityBox || DrawCollisionBox)
                {

                    if (DrawVisibilityBox)
                    {
                        if (_vertexBufferVisibility != null) _vertexBufferVisibility.Dispose();
                        _vertexBufferVisibility = GetVertexBufferFromBoundingBox(StaticMesh.VisibilityBox);

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
                        if (_vertexBufferCollision != null) _vertexBufferCollision.Dispose();
                        _vertexBufferCollision = GetVertexBufferFromBoundingBox(StaticMesh.CollisionBox);

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
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer<SolidVertex>(0, _plane.VertexBuffer));
                _device.SetIndexBuffer(_plane.IndexBuffer, true);

                solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                solidEffect.Parameters["Color"].SetValue(Vector4.One);
                solidEffect.Techniques[0].Passes[0].Apply();

                _device.Draw(PrimitiveType.LineList, _plane.VertexBuffer.ElementCount);
            }

            if (DrawGizmo)
            {
                // Draw the gizmo
                _gizmo.Draw(viewProjection);
            }

            // Draw debug strings
            _spriteBatch.Begin(SpriteSortMode.Immediate, _device.BlendStates.AlphaBlend);

            _spriteBatch.DrawString(_tool.Font,
                                    "Position: " + StaticPosition,
                                    new SharpDX.Vector2(0, 0),
                                    SharpDX.Color.White);
            _spriteBatch.DrawString(_tool.Font,
                                    "Rotation: " + (StaticRotation.X * (180 / Math.PI)),
                                    new SharpDX.Vector2(0, 18),
                                    SharpDX.Color.White);
            _spriteBatch.DrawString(_tool.Font,
                                    "Scale: " + StaticScale,
                                    new SharpDX.Vector2(0, 36),
                                    SharpDX.Color.White);

            _spriteBatch.End();

            _device.Present();
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
            return new SharpDX.ViewportF(0, 0, ClientSize.Width, ClientSize.Height).GetPickRay(new Vector2(x, y),
                Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                if (DrawGizmo)
                {
                    var result = _gizmo.DoPicking(GetRay(e.X, e.Y));
                    if (result != null)
                        _gizmo.ActivateGizmo(result);
                    return;
                }
            }

            _lastX = e.X;
            _lastY = e.Y;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(GetRay(e.X, e.Y))))
                Invalidate();
            if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), e.X, e.Y))
                Invalidate();

            if (e.Button == MouseButtons.Right)
            {
                // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                float deltaX = (e.X - _lastX) / (float)Height;
                float deltaY = (e.Y - _lastY) / (float)Height;

                _lastX = e.X;
                _lastY = e.Y;

                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    Camera.Zoom(-deltaY * _tool.Configuration.RenderingItem_NavigationSpeedMouseZoom);
                else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
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
    }
}
