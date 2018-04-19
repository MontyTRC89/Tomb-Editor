using TombLib.Utils;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Graphics.Primitives;
using TombLib.Wad;
using TombLib;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace WadTool.Controls
{
    public class PanelRenderingAnimationEditor : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimationNode Animation { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimatedModel Model { get { return _model; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentKeyFrame { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool PlayAnimation { get; set; }

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

        private GraphicsDevice _device;
        private DeviceManager _deviceManager;
        private SwapChainGraphicsPresenter _presenter;
        private RasterizerState _rasterizerWireframe;
        private WadToolClass _tool;
        private float _lastX;
        private float _lastY;
        private SpriteBatch _spriteBatch;
        private GizmoAnimationEditor _gizmo;
        private GeometricPrimitive _plane;
        private GeometricPrimitive _cube;
        private GeometricPrimitive _sphere;
        private GeometricPrimitive _littleSphere;
        private Wad2 _wad;
        private AnimatedModel _model;
        private WadMoveable _moveable;
        private WadMoveableId _moveableId;
        private AnimatedModel _skinModel;
        private WadMoveable _skinMoveable;
        private WadMoveableId _skinMoveableId;
        private WadRenderer _wadRenderer;

        public List<WadMeshBoneNode> Skeleton { get; set; }
        public ObjectMesh SelectedMesh { get; set; }

        private static readonly Vector4 _red = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        private static readonly Vector4 _green = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        private static readonly Vector4 _blue = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

        private Buffer<SolidVertex> _vertexBufferVisibility;

        public void InitializePanel(WadToolClass tool, Wad2 wad, DeviceManager deviceManager, WadMoveableId moveableId,
                                    WadMoveableId skinMoveableId)
        {
            _tool = tool;
            _wadRenderer = new WadRenderer(deviceManager.Device);
            _device = deviceManager.Device;
            _deviceManager = deviceManager;
            _wad = wad;
            _moveableId = moveableId;
            _moveable = _wad.Moveables[_moveableId];
            _model = _wadRenderer.GetMoveable(_moveable);

            if (skinMoveableId != null)
            {
                _skinMoveableId = skinMoveableId;
                _skinMoveable = _wad.Moveables[_skinMoveableId];
                _skinModel = _wadRenderer.GetMoveable(_skinMoveable);
            }

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
            _gizmo = new GizmoAnimationEditor(_tool, _tool.Configuration, _device, _deviceManager.Effects["Solid"], this);
            _plane = GeometricPrimitive.GridPlane.New(_device, 8, 4);
            _cube = GeometricPrimitive.LinesCube.New(_device, 1024.0f, 1024.0f, 1024.0f);
            _littleSphere = GeometricPrimitive.Sphere.New(_device, 2 * 128.0f, 8);
            _sphere = GeometricPrimitive.Sphere.New(_device, 1024.0f, 6);

            DrawGizmo = true;
            DrawGrid = true;
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

            if (_model != null)
            {
                var skin = (_skinModel != null ? _skinModel : _model);
                var effect = _deviceManager.Effects["Model"];

                effect.Parameters["Color"].SetValue(Vector4.One);
                effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                effect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                // Build animation transforms
                var matrices = new List<Matrix4x4>();
                if (Animation != null)
                {
                    for (var b = 0; b < _model.Meshes.Count; b++)
                        matrices.Add(_model.AnimationTransforms[b]);
                }
                else
                {
                    foreach (var bone in _model.Bones)
                        matrices.Add(bone.GlobalTransform);
                }

                for (int i = 0; i < skin.Meshes.Count; i++)
                {
                    var mesh = skin.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    _device.SetVertexBuffer(0, mesh.VertexBuffer);
                    _device.SetIndexBuffer(mesh.IndexBuffer, true);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, mesh.VertexBuffer));

                    effect.Parameters["ModelViewProjection"].SetValue((matrices[i] * viewProjection).ToSharpDX());

                    effect.Techniques[0].Passes[0].Apply();

                    foreach (var submesh in mesh.Submeshes)
                        _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.MeshBaseIndex);
                }

                // Draw box
                if (SelectedMesh != null)
                {
                    if (_vertexBufferVisibility != null) _vertexBufferVisibility.Dispose();
                    int meshIndex = _model.Meshes.IndexOf(SelectedMesh);
                    var world = (Animation != null ? _model.AnimationTransforms[meshIndex] : _model.BindPoseTransforms[meshIndex]);
                    _vertexBufferVisibility = GetVertexBufferFromBoundingBox(SelectedMesh.BoundingBox);

                    _device.SetVertexBuffer(_vertexBufferVisibility);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferVisibility));
                    _device.SetIndexBuffer(null, false);

                    solidEffect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_green);
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.Draw(PrimitiveType.LineList, _vertexBufferVisibility.ElementCount);
                }

                if (Animation != null)
                {
                    if (_vertexBufferVisibility != null) _vertexBufferVisibility.Dispose();
                    _vertexBufferVisibility = GetVertexBufferFromBoundingBox(Animation.DirectXAnimation.KeyFrames[CurrentKeyFrame].BoundingBox);

                    _device.SetVertexBuffer(_vertexBufferVisibility);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferVisibility));
                    _device.SetIndexBuffer(null, false);

                    solidEffect.Parameters["ModelViewProjection"].SetValue((viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_blue);
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

            if (DrawGizmo && SelectedMesh != null)
            {
                // Draw the gizmo
                _gizmo.Draw(viewProjection);
            }

            // Draw debug strings
            if (SelectedMesh != null)
            {
                /*Vector3 screenPos = _device.Viewport.Project(SelectedMesh.Centre - Vector3.UnitY * 128.0f,
                                    SelectedMesh.GlobalTransform * viewProjection);
                _spriteBatch.Begin(SpriteSortMode.Immediate, _device.BlendStates.AlphaBlend);

                _spriteBatch.DrawString(_deviceManager.Font,
                                        "Name: " + SelectedMesh.Bone.Name,
                                        new Vector2(screenPos.X, screenPos.Y).ToSharpDX(),
                                        SharpDX.Color.White);

                _spriteBatch.DrawString(_deviceManager.Font,
                                        "Local offset: " + SelectedMesh.Bone.Translation,
                                        new Vector2(screenPos.X, screenPos.Y + 20.0f).ToSharpDX(),
                                        SharpDX.Color.White);

                _spriteBatch.End();*/
            }

            _device.Present();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            try
            {
                if (_presenter != null)
                {
                    _presenter.Resize(ClientSize.Width, ClientSize.Height, SharpDX.DXGI.Format.B8G8R8A8_UNorm);
                    Invalidate();
                }
            }
            catch (Exception ex)
            {

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
            return new SharpDX.ViewportF(0, 0, ClientSize.Width, ClientSize.Height).GetPickRay(new Vector2(x, y),
                Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height));
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
                ObjectMesh foundMesh = null;
                for (int i = 0; i < _model.Meshes.Count; i++)
                {
                    float distance = 0;
                    float minDistance = float.PositiveInfinity;
                    if (DoMeshPicking(GetRay(e.X, e.Y), i, out distance))
                    {
                        if (distance < minDistance)
                        {
                            distance = minDistance;
                            foundMesh = _model.Meshes[i];
                        }
                    }
                }
                SelectedMesh = foundMesh;
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
            if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), e.X, e.Y))
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

        private bool DoMeshPicking(Ray ray, int meshIndex, out float meshDistance)
        {
            meshDistance = 0;

            // Transform view ray to object space space
            Matrix4x4 inverseObjectMatrix;
            if (!Matrix4x4.Invert((Animation != null ? _model.AnimationTransforms[meshIndex] : _model.BindPoseTransforms[meshIndex]), out inverseObjectMatrix))
                return false;
            Vector3 transformedRayPos = MathC.HomogenousTransform(ray.Position, inverseObjectMatrix);
            Vector3 transformedRayDestination = MathC.HomogenousTransform(ray.Position + ray.Direction, inverseObjectMatrix);
            Ray transformedRay = new Ray(transformedRayPos, transformedRayDestination - transformedRayPos);
            transformedRay.Direction = Vector3.Normalize(transformedRay.Direction);

            // Now do a ray - triangle intersection test
            bool hit = false;
            float minDistance = float.PositiveInfinity;
            var mesh = _model.Meshes[meshIndex];
            foreach (var submesh in mesh.Submeshes)
                for (int k = 0; k < submesh.Value.Indices.Count; k += 3)
                {
                    Vector3 p1 = mesh.Vertices[submesh.Value.Indices[k]].Position;
                    Vector3 p2 = mesh.Vertices[submesh.Value.Indices[k + 1]].Position;
                    Vector3 p3 = mesh.Vertices[submesh.Value.Indices[k + 2]].Position;

                    float distance;
                    if (Collision.RayIntersectsTriangle(transformedRay, p1, p2, p3, out distance) && distance < minDistance)
                    {
                        minDistance = distance;
                        hit = true;
                    }
                }

            if (hit)
            {
                meshDistance = minDistance;
                return true;
            }
            else
                return false;
        }
    }
}
