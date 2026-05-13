using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool.Controls
{
    public class PanelRenderingAnimationEditor : RenderingPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Configuration { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimatedModel Model { get { return _model; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimatedModel Skin { get { return _skinModel; } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Level Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Room Room { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector3 RoomPosition { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector3 GridPosition
        {
            get { return _gridPosition; }
            set
            {
                if (value == _gridPosition) return;
                _gridPosition = new Vector3(value.X % 4096.0f, value.Y % 4096.0f, value.Z % 4096.0f);
            }
        }
        private Vector3 _gridPosition;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<WadMeshBoneNode> Skeleton { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObjectMesh SelectedMesh
        {
            get { return _selectedMesh; }
            set { _selectedMesh = value; Invalidate(); }
        }
        private ObjectMesh _selectedMesh;

        // General state
        private AnimationEditor _editor;

        // Interaction state
        private float _lastX;
        private float _lastY;

        // Rendering state
        private RenderingTextureAllocator _fontTexture;
        private RenderingFont _fontDefault;

        // Unified path
        private RenderingDrawingLines _linesBatch;
        private readonly List<SolidLineVertex> _lines = new List<SolidLineVertex>();
        private readonly Dictionary<TombLib.Graphics.ObjectMesh, RenderingDrawingMesh> _meshCache = new Dictionary<TombLib.Graphics.ObjectMesh, RenderingDrawingMesh>();

        // Raw D3D11 device. Carried for source-compat with members that still use it.
        private SharpDX.Direct3D11.Device _device;
        private DeviceManager _deviceManager;
        private WadRenderer _wadRenderer;
        private GizmoAnimationEditor _gizmo;
        private AnimatedModel _model;
        private AnimatedModel _skinModel;

        public void InitializeRendering(AnimationEditor editor, DeviceManager deviceManager, WadMoveable skin)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            base.InitializeRendering(deviceManager.Device, Configuration.RenderingItem_Antialias);
            ResetCamera();

            _editor = editor;
            _wadRenderer = deviceManager.CreateWadRenderer(false, true, 4096, 2048, true);
            _model = _wadRenderer.GetMoveable(editor.Moveable);

            Configuration = _editor.Tool.Configuration;

            if (skin != null)
                _skinModel = _wadRenderer.GetMoveable(editor.Moveable.ReplaceDummyMeshes(skin));

            // Actual "InitializeRendering"
            _fontTexture = deviceManager.Device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(512, 512, 2) });
            _fontDefault = deviceManager.Device.CreateFont(new RenderingFont.Description
            {
                FontName = _editor.Tool.Configuration.Rendering3D_FontName,
                FontSize = _editor.Tool.Configuration.Rendering3D_FontSize,
                FontIsBold = _editor.Tool.Configuration.Rendering3D_FontIsBold,
                TextureAllocator = _fontTexture
            });

            _linesBatch = deviceManager.Device.CreateDrawingLines(new RenderingDrawingLines.Description { Dynamic = true });

            // Legacy rendering — only the gizmo remains on this path.
            {
                _device = deviceManager.D3D11Device;
                _deviceManager = deviceManager;
                _gizmo = new GizmoAnimationEditor(editor, deviceManager.Device, this);
            }
        }

        public void ResetCamera()
        {
            Camera = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 100, 1000000, (float)Math.PI / 4.0f);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fontTexture?.Dispose();
                _fontDefault?.Dispose();
                _gizmo?.Dispose();
                _model?.Dispose();
                _skinModel?.Dispose();
                _wadRenderer?.Dispose();
                _linesBatch?.Dispose();
                foreach (var m in _meshCache.Values)
                    m.Dispose();
                _meshCache.Clear();
            }
            base.Dispose(disposing);
        }

        private RenderingDrawingMesh GetOrCreateDrawingMesh(TombLib.Graphics.ObjectMesh legacyMesh)
        {
            if (_meshCache.TryGetValue(legacyMesh, out var cached))
                return cached;
            var verts = new MeshVertex[legacyMesh.Vertices.Count];
            for (int i = 0; i < legacyMesh.Vertices.Count; ++i)
            {
                var s = legacyMesh.Vertices[i];
                verts[i] = new MeshVertex { Position = s.Position, UVW = s.UVW, Normal = s.Normal, Color = s.Color, BoneIndex = s.Indices, BoneWeight = s.Weights };
            }
            var subList = new List<RenderingDrawingMesh.Submesh>(legacyMesh.Submeshes.Count);
            foreach (var kv in legacyMesh.Submeshes)
            {
                if (kv.Value.NumIndices == 0) continue;
                subList.Add(new RenderingDrawingMesh.Submesh
                {
                    IndexStart = kv.Value.BaseIndex,
                    IndexCount = kv.Value.NumIndices,
                    DoubleSided = kv.Key.DoubleSided,
                    AdditiveBlending = kv.Key.AdditiveBlending,
                });
            }
            var mesh = Device.CreateDrawingMesh(new RenderingDrawingMesh.Description
            {
                Vertices = verts, Indices = legacyMesh.Indices, Submeshes = subList,
            });
            _meshCache[legacyMesh] = mesh;
            return mesh;
        }

        protected override Vector4 ClearColor => Configuration.RenderingItem_BackgroundColor;

        protected override void OnDraw()
        {
            ((TombLib.Rendering.DirectX11.Dx11RenderingSwapChain)SwapChain).BindForce();
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            var viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            using var stateBuffer = Device.CreateStateBuffer();
            stateBuffer.Set(new RenderingState { TransformMatrix = viewProjection });

            _lines.Clear();

            if (_model != null)
            {
                var skin = (_skinModel != null ? _skinModel : _model);

                // Build per-mesh world matrix (animation pose vs. bind pose) for each
                // mesh. Same logic as the legacy code.
                var matrices = new List<Matrix4x4>();
                if (_editor.ValidAnimationAndFrames)
                    for (var b = 0; b < _model.Meshes.Count; b++)
                        matrices.Add(_model.AnimationTransforms[b]);
                else
                    foreach (var bone in _model.Bones)
                        matrices.Add(bone.GlobalTransform);

                bool showSkin = _editor.Wad.GameVersion == TRVersion.Game.TombEngine && Configuration.AnimationEditor_ShowSkin && skin.Skin != null;

                for (int i = 0; i < skin.Meshes.Count; i++)
                {
                    var legacyMesh = skin.Meshes[i];
                    if (legacyMesh.Vertices.Count == 0) continue;
                    if (showSkin && legacyMesh.Hidden) continue;

                    var drawMesh = GetOrCreateDrawingMesh(legacyMesh);
                    var tint = (SelectedMesh == _model.Meshes[i] && _editor.ValidAnimationAndFrames)
                        ? new Vector4(1, 0, 0, 1)
                        : Vector4.One;

                    drawMesh.Render(new RenderingDrawingMesh.RenderArgs
                    {
                        RenderTarget = SwapChain,
                        StateBuffer = stateBuffer,
                        Atlas = _wadRenderer.Texture,
                        World = matrices[i],
                        Tint = tint,
                        StaticLighting = false,
                        ColoredVertices = false,
                    });
                }

                // GPU-skinned skin pass — replaces the legacy AnimatedModel.RenderSkin.
                if (showSkin)
                {
                    var skinDraw = GetOrCreateDrawingMesh(Skin.Skin);
                    int boneCount = _model.AnimationTransforms.Count;
                    var bones = new Matrix4x4[boneCount];
                    for (int b = 0; b < boneCount; ++b)
                    {
                        if (Matrix4x4.Invert(_model.BindPoseTransforms[b], out var invBindPose))
                            bones[b] = invBindPose * _model.AnimationTransforms[b];
                        else
                            bones[b] = Matrix4x4.Identity;
                    }
                    skinDraw.Render(new RenderingDrawingMesh.RenderArgs
                    {
                        RenderTarget = SwapChain,
                        StateBuffer = stateBuffer,
                        Atlas = _wadRenderer.Texture,
                        World = Matrix4x4.Identity,
                        Tint = Vector4.One,
                        StaticLighting = false,
                        ColoredVertices = false,
                        AlphaTest = true,
                        Skinned = true,
                        BoneMatrices = bones,
                    });
                }

                if (_editor.ValidAnimationAndFrames)
                {
                    if (SelectedMesh != null)
                    {
                        int meshIndex = _model.Meshes.IndexOf(SelectedMesh);
                        var box = Skin.Meshes[meshIndex].BoundingBox;
                        var center = (box.Minimum + box.Maximum) * 0.5f;
                        var halfSize = (box.Maximum - box.Minimum) * 0.5f;
                        var world = Matrix4x4.CreateScale(halfSize) * Matrix4x4.CreateTranslation(center) * _model.AnimationTransforms[meshIndex];
                        WireGeometry.AppendWireCube(_lines, world, new Vector4(1, 0, 0, 1));
                    }
                    if (Configuration.AnimationEditor_ShowCollisionBox)
                        WireGeometry.AppendWireBoundingBox(_lines, _editor.CurrentKeyFrame.BoundingBox, new Vector4(0, 1, 0, 1));
                }
            }

            if (Configuration.AnimationEditor_ShowGrid)
            {
                // Grid at GridPosition (legacy used a translation matrix and the
                // GridPlane primitive). Bake the translation into the stateBuffer so
                // we can append to the same _lines batch as the boxes above.
                var gridLines = new List<SolidLineVertex>();
                WireGeometry.AppendGrid(gridLines, sizePerSide: 8, divisions: 4, color: Vector4.One);
                var shift = new Vector3(-GridPosition.X, GridPosition.Y, -GridPosition.Z);
                foreach (var v in gridLines)
                    _lines.Add(new SolidLineVertex { Position = v.Position + shift, Color = v.Color });
            }

            if (_lines.Count > 0)
            {
                _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_lines));
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = stateBuffer,
                });
            }

            if (Configuration.AnimationEditor_ShowGizmo &&
                SelectedMesh != null && _editor.ValidAnimationAndFrames)
            {
                ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();
                SwapChain.ClearDepth();
                _gizmo.Draw(SwapChain, stateBuffer, viewProjection);
            }

            if (_editor.CurrentAnim != null && 
                Configuration.RenderingItem_ShowDebugInfo)
            {
                ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState(); // To make sure SharpDx.Toolkit didn't change settings.
                string debugMessage = "Frame: " + (_editor.CurrentFrameIndex + 1) + "/" + _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
                if (SelectedMesh != null)
                {
                    debugMessage += "\nMesh: " + SelectedMesh.Name;
                    debugMessage += "\nBone: " + _model.Bones[_model.Meshes.IndexOf(SelectedMesh)].Name;
                    debugMessage += "\nRotation: " + _editor.CurrentKeyFrame.Rotations[Model.Meshes.IndexOf(SelectedMesh)];
                }
                SwapChain.RenderText(new Text
                {
                    Font = _fontDefault,
                    Overlay = true,
                    PixelPos = new Vector2(10, -10),
                    Alignment = new Vector2(0, 0),
                    String = debugMessage
                });
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            // Make this control able to receive scroll and key board events...
            base.OnMouseEnter(e);

            if (Form.ActiveForm == FindForm())
                Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Camera.Zoom(-e.Delta * Configuration.RenderingItem_NavigationSpeedMouseWheelZoom);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

                if (_editor.ValidAnimationAndFrames)
                {
                    // Try to do gizmo picking
                    if (Configuration.AnimationEditor_ShowGizmo)
                    {
                        var result = _gizmo.DoPicking(ray);
                        if (result != null)
                        {
                            _gizmo.ActivateGizmo(result);
                            _editor.Tool.AnimationEditorGizmoPicked();
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
                        if (DoMeshPicking(ray, i, out distance))
                        {
                            if (distance < minDistance)
                            {
                                distance = minDistance;
                                foundMesh = _model.Meshes[i];
                            }
                        }
                    }

                    if (SelectedMesh != foundMesh)
                    {
                        SelectedMesh = foundMesh;
                        _editor.Tool.AnimationEditorMeshSelected(Model, SelectedMesh);
                    }
                }
                else
                {
                    SelectedMesh = null;
                    _editor.Tool.AnimationEditorMeshSelected(Model, SelectedMesh);
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

            var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

            if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(ray)))
                Invalidate();
            if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ray))
                Invalidate();

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
                        Camera.Zoom(-deltaY * Configuration.RenderingItem_NavigationSpeedMouseZoom);
                    else if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                        Camera.Rotate(deltaX * Configuration.RenderingItem_NavigationSpeedMouseRotate,
                                     -deltaY * Configuration.RenderingItem_NavigationSpeedMouseRotate);
                }
                if ((e.Button == MouseButtons.Right && (ModifierKeys & Keys.Shift) == Keys.Shift) ||
                     e.Button == MouseButtons.Middle)
                    Camera.MoveCameraPlane(new Vector3(deltaX, deltaY, 0) * Configuration.RenderingItem_NavigationSpeedMouseTranslate);

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
            if (!Matrix4x4.Invert((_editor.CurrentAnim != null ? _model.AnimationTransforms[meshIndex] : _model.BindPoseTransforms[meshIndex]), out inverseObjectMatrix))
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
                    if (Collision.RayIntersectsTriangle(transformedRay, p1, p2, p3, true, out distance) && distance < minDistance)
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
