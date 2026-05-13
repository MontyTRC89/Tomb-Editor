using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool.Controls
{
    public class PanelRenderingSkeleton : RenderingPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; } = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 100, 1000000, (float)Math.PI / 4.0f);
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Configuration { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGrid { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGizmo { get; set; } = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector3 StaticPosition { get; set; } = Vector3.Zero;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector3 StaticRotation { get; set; } = Vector3.Zero;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float StaticScale { get; set; } = 1.0f;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<WadMeshBoneNode> Skeleton { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadMeshBoneNode SelectedNode { get; set; }

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

        // Unified path
        private RenderingDrawingLines _linesBatch;
        private readonly List<SolidLineVertex> _lines = new List<SolidLineVertex>();
        private readonly Dictionary<TombLib.Graphics.ObjectMesh, RenderingDrawingMesh> _meshCache = new Dictionary<TombLib.Graphics.ObjectMesh, RenderingDrawingMesh>();

        // Legacy rendering state — only the gizmo remains on this path.
        private SharpDX.Direct3D11.Device _device;
        private DeviceManager _deviceManager;
        private GizmoSkeletonEditor _gizmo;
        private WadRenderer _wadRenderer;
        private WadStatic _dummyStatic = new WadStatic(new WadStaticId(0));

        public void InitializeRendering(WadToolClass tool, DeviceManager deviceManager)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            base.InitializeRendering(deviceManager.Device, tool.Configuration.RenderingItem_Antialias);
            _tool = tool;
            _device = deviceManager.D3D11Device;
            _deviceManager = deviceManager;

            // Actual "InitializeRendering"
            _fontTexture = deviceManager.Device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(512, 512, 2) });
            _fontDefault = deviceManager.Device.CreateFont(new RenderingFont.Description
            {
                FontName = _tool.Configuration.Rendering3D_FontName,
                FontSize = _tool.Configuration.Rendering3D_FontSize,
                FontIsBold = _tool.Configuration.Rendering3D_FontIsBold,
                TextureAllocator = _fontTexture
            });

            _linesBatch = deviceManager.Device.CreateDrawingLines(new RenderingDrawingLines.Description { Dynamic = true });

            // Legacy rendering — only the gizmo remains on this path.
            {
                _wadRenderer = _deviceManager.CreateWadRenderer(false, true, 4096, 2048, false);
                _gizmo = new GizmoSkeletonEditor(_tool, _tool.Configuration, deviceManager.Device, this);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fontTexture?.Dispose();
                _fontDefault?.Dispose();
                _gizmo?.Dispose();
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

            Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            using var stateBuffer = Device.CreateStateBuffer();
            stateBuffer.Set(new RenderingState { TransformMatrix = viewProjection });

            _lines.Clear();
            if (DrawGrid)
                WireGeometry.AppendGrid(_lines, sizePerSide: 8, divisions: 4, color: Vector4.One);

            if (Skeleton != null)
            {
                var coloredVertices = _tool.DestinationWad.GameVersion == TombLib.LevelData.TRVersion.Game.TombEngine;
                foreach (var node in Skeleton)
                {
                    _dummyStatic.Mesh = node.Mesh;
                    _dummyStatic.Version = DataVersion.GetNext();
                    var staticModel = _wadRenderer.GetStatic(_dummyStatic);

                    foreach (var legacyMesh in staticModel.Meshes)
                    {
                        if (legacyMesh.Vertices.Count == 0) continue;
                        var drawMesh = GetOrCreateDrawingMesh(legacyMesh);
                        drawMesh.Render(new RenderingDrawingMesh.RenderArgs
                        {
                            RenderTarget = SwapChain,
                            StateBuffer = stateBuffer,
                            Atlas = _wadRenderer.Texture,
                            World = node.GlobalTransform,
                            Tint = Vector4.One,
                            StaticLighting = node.Mesh.LightingType != WadMeshLightingType.Normals,
                            ColoredVertices = coloredVertices,
                        });
                    }
                }

                if (SelectedNode != null)
                {
                    // BoundingBox is in node-local space; transform corners by GlobalTransform
                    // to put the wire box at the node's world position.
                    var box = SelectedNode.Mesh.BoundingBox;
                    var center = (box.Minimum + box.Maximum) * 0.5f;
                    var halfSize = (box.Maximum - box.Minimum) * 0.5f;
                    var world = Matrix4x4.CreateScale(halfSize) * Matrix4x4.CreateTranslation(center) * SelectedNode.GlobalTransform;
                    WireGeometry.AppendWireCube(_lines, world, new Vector4(0, 1, 0, 1));
                }
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

            if (DrawGizmo && SelectedNode != null)
            {
                ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();
                SwapChain.ClearDepth();
                _gizmo.Draw(SwapChain, stateBuffer, viewProjection);
            }

            // Draw debug strings
            if (SelectedNode != null)
            {
                ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState(); // To make sure SharpDx.Toolkit didn't change settings.
                Matrix4x4 worldViewProjection = SelectedNode.GlobalTransform * viewProjection;
                SwapChain.RenderText(new Text
                {
                    Font = _fontDefault,
                    Overlay = true,
                    Pos = worldViewProjection.TransformPerspectively(SelectedNode.Center - Vector3.UnitY * 128.0f).To2(),
                    TextAlignment = new Vector2(0, 0),
                    ScreenAlignment = new Vector2(0.5f, 0.5f),
                    String =
                        "Name: " + SelectedNode.Bone.Name +
                        "\nLocal offset: " + SelectedNode.Bone.Translation
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

                // Try to do gizmo picking
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

                // Try to do node picking
                WadMeshBoneNode foundNode = null;
                foreach (var node in Skeleton)
                {
                    float distance = 0;
                    float minDistance = float.PositiveInfinity;
                    if (DoNodePicking(ray, node, out distance))
                    {
                        if (distance < minDistance)
                        {
                            distance = minDistance;
                            foundNode = node;
                        }
                    }
                }
                SelectedNode = foundNode;
                _tool.BonePicked();
            }

            Invalidate();

            _lastX = e.X;
            _lastY = e.Y;

            base.OnMouseDown(e);
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
            var mesh = node.Mesh;
            foreach (var poly in mesh.Polys)
            {
                if (poly.Shape == WadPolygonShape.Quad)
                {
                    Vector3 p1 = mesh.VertexPositions[poly.Index0];
                    Vector3 p2 = mesh.VertexPositions[poly.Index1];
                    Vector3 p3 = mesh.VertexPositions[poly.Index2];
                    Vector3 p4 = mesh.VertexPositions[poly.Index3];

                    float distance;
                    if (Collision.RayIntersectsTriangle(transformedRay, p1, p2, p3, true, out distance) && distance < minDistance)
                    {
                        minDistance = distance;
                        hit = true;
                    }

                    if (Collision.RayIntersectsTriangle(transformedRay, p1, p3, p4, true, out distance) && distance < minDistance)
                    {
                        minDistance = distance;
                        hit = true;
                    }
                }
                else
                {
                    Vector3 p1 = mesh.VertexPositions[poly.Index0];
                    Vector3 p2 = mesh.VertexPositions[poly.Index1];
                    Vector3 p3 = mesh.VertexPositions[poly.Index2];

                    float distance;
                    if (Collision.RayIntersectsTriangle(transformedRay, p1, p2, p3, true, out distance) && distance < minDistance)
                    {
                        minDistance = distance;
                        hit = true;
                    }
                }
            }

            if (hit)
            {
                nodeDistance = minDistance;
                return true;
            }
            else
                return false;
        }
        public void UpdateModel() => _wadRenderer.Dispose();
    }
}
