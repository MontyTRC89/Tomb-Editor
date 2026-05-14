using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class PanelRenderingStaticEditor : RenderingPanel
    {
        public enum StaticEditorAction
        {
            Normal,
            PlaceLight
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Configuration { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadStatic Static { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; } = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 100, 1000000, (float)Math.PI / 4.0f);
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
        // Unified-path resources (Tappa-1 abstractions).
        private RenderingStateBuffer _stateBuffer;
        private RenderingDrawingLines _linesBatch;
        private readonly List<SolidLineVertex> _lines = new List<SolidLineVertex>();
        private readonly Dictionary<TombLib.Graphics.ObjectMesh, RenderingDrawingMesh> _meshCache = new Dictionary<TombLib.Graphics.ObjectMesh, RenderingDrawingMesh>();

        private DeviceManager _deviceManager;
        private GizmoStaticEditor _gizmo;
        private GizmoStaticEditorLight _gizmoLight;
        private WadRenderer _wadRenderer;

        public void InitializeRendering(WadToolClass tool, DeviceManager deviceManager)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            base.InitializeRendering(deviceManager.Device, tool.Configuration.RenderingItem_Antialias);
            _tool = tool;

            // Actual "InitializeRendering"
            _fontTexture = deviceManager.Device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(512, 512, 2) });
            _fontDefault = deviceManager.Device.CreateFont(new RenderingFont.Description
            {
                FontName = _tool.Configuration.Rendering3D_FontName,
                FontSize = _tool.Configuration.Rendering3D_FontSize,
                FontIsBold = _tool.Configuration.Rendering3D_FontIsBold,
                TextureAllocator = _fontTexture
            });

            // Unified path
            _linesBatch = deviceManager.Device.CreateDrawingLines(new RenderingDrawingLines.Description { Dynamic = true });
            _stateBuffer = deviceManager.Device.CreateStateBuffer();

            // Legacy rendering — only the gizmos remain on this path.
            {
                _deviceManager = deviceManager;
                _wadRenderer = _deviceManager.CreateWadRenderer(false, true, 4096, 2048, false);
                _gizmo = new GizmoStaticEditor(_tool.Configuration, deviceManager.Device, this);
                _gizmoLight = new GizmoStaticEditorLight(_tool.Configuration, deviceManager.Device, this);
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
                _wadRenderer?.Dispose();
                _stateBuffer?.Dispose();
                _linesBatch?.Dispose();
                foreach (var m in _meshCache.Values)
                    m.Dispose();
                _meshCache.Clear();
            }

            base.Dispose(disposing);
        }

        // Cache helper, identical pattern to Panel3D.GetOrCreateDrawingMesh.
        private RenderingDrawingMesh GetOrCreateDrawingMesh(TombLib.Graphics.ObjectMesh legacyMesh)
        {
            if (_meshCache.TryGetValue(legacyMesh, out var cached))
                return cached;

            var verts = new MeshVertex[legacyMesh.Vertices.Count];
            for (int i = 0; i < legacyMesh.Vertices.Count; ++i)
            {
                var s = legacyMesh.Vertices[i];
                verts[i] = new MeshVertex
                {
                    Position = s.Position,
                    UVW = s.UVW,
                    Normal = s.Normal,
                    Color = s.Color,
                    BoneIndex = s.Indices,
                    BoneWeight = s.Weights,
                };
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
                Vertices = verts,
                Indices = legacyMesh.Indices,
                Submeshes = subList,
            });
            _meshCache[legacyMesh] = mesh;
            return mesh;
        }

        protected override Vector4 ClearColor => Configuration.RenderingItem_BackgroundColor;

        protected override void OnDraw()
        {
            SwapChain.BindForce();
            Device.ResetState();

            var viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            _stateBuffer.Set(new RenderingState { TransformMatrix = viewProjection });

            // Accumulate every line/wire pass into one batch (grid + lights wireframes
            // + boxes + normals). One Render() at the end.
            _lines.Clear();

            if (DrawGrid)
            {
                // Replicates the legacy GeometricPrimitive.GridPlane.New(_device, 8, 4):
                // an 8×8 grid of cells, each cell 4×4 sectors? Actually the API is
                // GridPlane.New(device, sizeOfPlane, gridDivisions) producing line
                // segments at unit intervals. To preserve appearance we emit a unit
                // grid scaled appropriately.
                WireGeometry.AppendGrid(_lines, sizePerSide: 8, divisions: 4, color: Vector4.One);
            }

            if (DrawLights)
            {
                foreach (var light in Static.Lights)
                {
                    // Small placeholder sphere at the light position.
                    var lWorld = Matrix4x4.CreateScale(128.0f) * Matrix4x4.CreateTranslation(light.Position);
                    WireGeometry.AppendWireSphere(_lines, lWorld, new Vector4(1, 1, 0, 1), segments: 12);

                    if (SelectedLight == light)
                    {
                        var rWorld = Matrix4x4.CreateScale(light.Radius) * Matrix4x4.CreateTranslation(light.Position);
                        WireGeometry.AppendWireSphere(_lines, rWorld, new Vector4(0, 1, 0, 1), segments: 24);
                    }
                }
            }

            if (Static != null)
            {
                var model = _wadRenderer.GetStatic(Static);
                var world = GizmoTransform;
                var staticLighting = Static.Mesh.LightingType != WadMeshLightingType.Normals;
                var coloredVertices = _tool.DestinationWad.GameVersion == TRVersion.Game.TombEngine;

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    var legacyMesh = model.Meshes[i];
                    if (legacyMesh.Vertices.Count == 0)
                        continue;
                    var drawMesh = GetOrCreateDrawingMesh(legacyMesh);
                    drawMesh.Render(new RenderingDrawingMesh.RenderArgs
                    {
                        RenderTarget = SwapChain,
                        StateBuffer = _stateBuffer,
                        Atlas = _wadRenderer.Texture,
                        World = world,
                        Tint = Vector4.One,
                        StaticLighting = staticLighting,
                        ColoredVertices = coloredVertices,
                        BilinearFilter = true,
                    });
                }

                if (DrawVisibilityBox)
                    WireGeometry.AppendWireBoundingBox(_lines, Static.VisibilityBox, new Vector4(0, 0, 1, 1));
                if (DrawCollisionBox)
                    WireGeometry.AppendWireBoundingBox(_lines, Static.CollisionBox, new Vector4(0, 1, 0, 1));

                if (DrawNormals)
                {
                    var c = Vector4.One;
                    for (int i = 0; i < Static.Mesh.VertexNormals.Count; i++)
                    {
                        var p = Vector3.Transform(Static.Mesh.VertexPositions[i], world);
                        var n = Vector3.TransformNormal(Static.Mesh.VertexNormals[i] / Static.Mesh.VertexNormals[i].Length(), world);
                        _lines.Add(new SolidLineVertex { Position = p,             Color = c });
                        _lines.Add(new SolidLineVertex { Position = p + n * 32.0f, Color = c });
                    }
                }
            }

            // Submit all accumulated lines in a single draw call.
            if (_lines.Count > 0)
            {
                _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_lines));
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = _stateBuffer,
                });
            }

            // Gizmos still on the legacy Solid effect path. They need the state to be
            // restored to legacy expectations; ResetState flushes our overrides first.
            if (DrawGizmo)
            {
                Device.ResetState();
                SwapChain.ClearDepth();
                _gizmo.Draw(SwapChain, _stateBuffer, viewProjection);
            }
            if (SelectedLight != null)
            {
                Device.ResetState();
                SwapChain.ClearDepth();
                _gizmoLight.Draw(SwapChain, _stateBuffer, viewProjection);
            }

            // Draw debug strings
            Device.ResetState(); // To make sure SharpDx.Toolkit didn't change settings.
            SwapChain.RenderText(new Text
            {
                Font = _fontDefault,
                PixelPos = new Vector2(10, -10),
                Alignment = new Vector2(0, 0),
                Overlay = true,
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

        private void PlaceLight(int x, int y)
        {
            // Get the intersection point between ray and the horizontal plane
            var ray = Ray.GetPickRay(Camera, ClientSize, x, y);
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

            var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

            if (e.Button == MouseButtons.Left)
            {
                if (Action != StaticEditorAction.PlaceLight)
                {
                    if (DrawGizmo)
                    {
                        var result = _gizmo.DoPicking(ray);
                        if (result != null)
                        {
                            _gizmo.ActivateGizmo(result);
                            Invalidate();
                            return;
                        }
                    }

                    if (SelectedLight != null)
                    {
                        var result = _gizmoLight.DoPicking(ray);
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
                        if (Collision.RayIntersectsSphere(ray, new BoundingSphere(light.Position, 128.0f),
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

            var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

            if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(ray)))
                Invalidate();
            if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ray))
                Invalidate();

            if (_gizmoLight.GizmoUpdateHoverEffect(_gizmoLight.DoPicking(ray)))
                Invalidate();
            if (_gizmoLight.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ray))
                Invalidate();

            if (Action == StaticEditorAction.Normal)
                Cursor = Cursors.Default;
            else
                Cursor = Cursors.Cross;

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

            ((FormStaticEditor)FindForm()).UpdatePositionUI();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_gizmo.MouseUp())
                Invalidate();

            if (_gizmoLight.MouseUp())
                Invalidate();

            ((FormStaticEditor)FindForm()).UpdatePositionUI();
        }

        public void UpdateLights()
        {
            Static.Mesh.VertexColors.Clear();

            Matrix4x4 world = Matrix4x4.CreateFromYawPitchRoll(StaticRotation.Y, StaticRotation.X, StaticRotation.Z) *  
                              Matrix4x4.CreateTranslation(StaticPosition);

            for (int i = 0; i < Static.Mesh.VertexPositions.Count; i++)
            {
                float newShade = Static.AmbientLight / 255.0f;
                foreach (var light in Static.Lights)
                {
                    // Transform current vertex and normal
                    var p = Vector3.Transform(Static.Mesh.VertexPositions[i], world);
                    var n = Vector3.TransformNormal(Static.Mesh.VertexNormals[i] / Static.Mesh.VertexNormals[i].Length(), world);

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
                                (light.Radius * Level.SectorSizeUnit));
                }

                Static.Mesh.VertexColors.Add(new Vector3(Math.Min(newShade, 1.0f)));
            }

            _tool.StaticLightsChanged();
        }

        public void UpdateMesh() => _wadRenderer.Dispose();
    }
}
