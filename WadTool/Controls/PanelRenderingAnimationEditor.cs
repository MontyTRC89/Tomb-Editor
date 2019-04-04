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
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace WadTool.Controls
{
    public class PanelRenderingAnimationEditor : RenderingPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Configuration { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; } = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 0, 1000000, (float)Math.PI / 4.0f);
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimationNode Animation { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimatedModel Model { get { return _model; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimatedModel Skin { get { return _skinModel; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentKeyFrame { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool PlayAnimation { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float CurrentTime { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawVisibilityBox { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawCollisionBox { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGrid { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGizmo { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawLights { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawRoom { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Level Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Room Room { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector3 RoomPosition { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<WadMeshBoneNode> Skeleton { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObjectMesh SelectedMesh { get; set; }

        // General state
        private WadToolClass _tool;
        private Wad2 _wad;
        private WadMoveableId _moveableId;
        private WadMoveable _moveable;
        private WadMoveableId _skinMoveableId;
        private WadMoveable _skinMoveable;

        // Interaction state
        private float _lastX;
        private float _lastY;

        // Rendering state
        private RenderingTextureAllocator _fontTexture;
        private RenderingFont _fontDefault;

        // Legacy rendering state
        private GraphicsDevice _device;
        private DeviceManager _deviceManager;
        private GizmoAnimationEditor _gizmo;
        private GeometricPrimitive _plane;
        private AnimatedModel _model;
        private AnimatedModel _skinModel;
        private WadRenderer _wadRenderer;
        private Buffer<SolidVertex> _vertexBufferVisibility;

        public void InitializeRendering(WadToolClass tool, Wad2 wad, DeviceManager deviceManager, WadMoveableId moveableId,
                                    WadMoveableId skinMoveableId)
        {
            base.InitializeRendering(deviceManager.Device, false);

            _tool = tool;
            _wadRenderer = new WadRenderer(deviceManager.___LegacyDevice);
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

            // Actual "InitializeRendering"
            _fontTexture = Device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(512, 512, 2) });
            _fontDefault = Device.CreateFont(new RenderingFont.Description { FontName = "Segoe UI", FontSize = 24, FontIsBold = true, TextureAllocator = _fontTexture });

            // Legacy rendering
            {
                _device = deviceManager.___LegacyDevice;
                _deviceManager = deviceManager;
                new BasicEffect(_device); // This effect is used for editor special meshes like sinks, cameras, light meshes, etc
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
                _gizmo = new GizmoAnimationEditor(_tool, _tool.Configuration, _device, _deviceManager.___LegacyEffects["Solid"], this);
                _plane = GeometricPrimitive.GridPlane.New(_device, 8, 4);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fontTexture?.Dispose();
                _fontDefault?.Dispose();
                _gizmo?.Dispose();
                _plane?.Dispose();
                _model?.Dispose();
                _skinModel?.Dispose();
                _wadRenderer?.Dispose();
                _vertexBufferVisibility?.Dispose();
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

        protected override Vector4 ClearColor => Configuration.RenderingItem_BackgroundColor;

        protected override void OnDraw()
        {
            // To make sure things are in a defined state for legacy rendering...
            ((TombLib.Rendering.DirectX11.Dx11RenderingSwapChain)SwapChain).BindForce();
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            _device.SetDepthStencilState(_device.DepthStencilStates.Default);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
            _device.SetBlendState(_device.BlendStates.Opaque);

            Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);

            /*if (Level != null && Room != null)
            {
                Effect roomsEffect = _deviceManager.Effects["Room"];

                roomsEffect.Parameters["TextureEnabled"].SetValue(true);
                roomsEffect.Parameters["DrawSectorOutlinesAndUseEditorUV"].SetValue(false);
                roomsEffect.Parameters["Highlight"].SetValue(false);
                roomsEffect.Parameters["Dim"].SetValue(false);
                roomsEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                roomsEffect.Parameters["Texture"].SetResource(_textureAtlas);
                roomsEffect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.AnisotropicWrap);
                roomsEffect.Parameters["UseVertexColors"].SetValue(true);
                roomsEffect.Parameters["TextureAtlasRemappingSize"].SetValue(_textureAtlasRemappingSize);
                roomsEffect.Parameters["TextureCoordinateFactor"].SetValue(_textureAtlas == null ? new Vector2(0) : new Vector2(1.0f / _textureAtlas.Width, 1.0f / _textureAtlas.Height));

                _device.SetVertexBuffer(0, Room.VertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, Room.VertexBuffer));

                var world = Matrix4x4.CreateTranslation(new Vector3(-RoomPosition.X, 0, -RoomPosition.Z));

                roomsEffect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());

                roomsEffect.Techniques[0].Passes[0].Apply();
                _device.Draw(PrimitiveType.TriangleList, Room.VertexBuffer.ElementCount);

            }*/

            Effect solidEffect = _deviceManager.___LegacyEffects["Solid"];

            if (_model != null)
            {
                var skin = (_skinModel != null ? _skinModel : _model);
                var effect = _deviceManager.___LegacyEffects["Model"];

                effect.Parameters["Color"].SetValue(Vector4.One);
                effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                effect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                // Build animation transforms
                var matrices = new List<Matrix4x4>();
                if (Animation != null && Animation.DirectXAnimation.KeyFrames.Count != 0)
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
                    {
                        _device.Draw(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                    }

                    //foreach (var submesh in mesh.Submeshes)
                    //   _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.MeshBaseIndex);
                }

                // Draw box
                if (SelectedMesh != null)
                {
                    if (_vertexBufferVisibility != null)
                        _vertexBufferVisibility.Dispose();
                    int meshIndex = _model.Meshes.IndexOf(SelectedMesh);
                    var world = (Animation != null ? _model.AnimationTransforms[meshIndex] : _model.BindPoseTransforms[meshIndex]);
                    _vertexBufferVisibility = GetVertexBufferFromBoundingBox(Skin.Meshes[meshIndex].BoundingBox);

                    _device.SetVertexBuffer(_vertexBufferVisibility);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferVisibility));
                    _device.SetIndexBuffer(null, false);

                    solidEffect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.Draw(PrimitiveType.LineList, _vertexBufferVisibility.ElementCount);
                }

                if (Animation != null && Animation.DirectXAnimation.KeyFrames.Count != 0)
                {
                    if (_vertexBufferVisibility != null)
                        _vertexBufferVisibility.Dispose();
                    _vertexBufferVisibility = GetVertexBufferFromBoundingBox(Animation.DirectXAnimation.KeyFrames[CurrentKeyFrame].BoundingBox);

                    _device.SetVertexBuffer(_vertexBufferVisibility);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferVisibility));
                    _device.SetIndexBuffer(null, false);

                    solidEffect.Parameters["ModelViewProjection"].SetValue((viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
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
                SwapChain.ClearDepth();
                _gizmo.Draw(viewProjection);
            }

            if (Animation != null)
            {
                ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState(); // To make sure SharpDx.Toolkit didn't change settings.
                string debugMessage = "Frame: " + (CurrentKeyFrame + 1) + "/" + Animation.DirectXAnimation.KeyFrames.Count;
                if (SelectedMesh != null)
                {
                    debugMessage += "\nMesh: " + SelectedMesh.Name;
                    debugMessage += "\nBone: " + _model.Bones[_model.Meshes.IndexOf(SelectedMesh)].Name;
                    debugMessage += "\nRotation: " + Animation.DirectXAnimation.KeyFrames[CurrentKeyFrame].Rotations[Model.Meshes.IndexOf(SelectedMesh)];
                }
                SwapChain.RenderText(new Text
                {
                    Font = _fontDefault,
                    PixelPos = new Vector2(10, 25),
                    Alignment = new Vector2(0, 0),
                    String = debugMessage
                });
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
            return Ray.GetPickRay(new Vector2(x, y), Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ClientSize.Width, ClientSize.Height);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                if (Animation != null && Animation.DirectXAnimation.KeyFrames.Count != 0)
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
                    _tool.AnimationEditorMeshSelected(Model, SelectedMesh);
                }
                else
                {
                    SelectedMesh = null;
                    _tool.AnimationEditorMeshSelected(Model, SelectedMesh);
                }
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
            var mesh = _skinModel.Meshes[meshIndex];
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
