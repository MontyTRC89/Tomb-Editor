using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Graphics;
using TombLib.Rendering.Graphics.Preview;
using TombLib.Rendering.Graphics.Rhi;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool.Controls
{
    /// <summary>
    /// Skeleton editor preview. Inherits the V2 <see cref="ItemPreviewPanel"/>
    /// for swapchain + camera plumbing; <see cref="RenderContents"/> draws the
    /// skeleton bones, grid, selection box, and gizmo.
    /// </summary>
    public class PanelRenderingSkeleton : ItemPreviewPanel
    {
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

        public Matrix4x4 GizmoTransform =>
            Matrix4x4.CreateScale(StaticScale) *
            Matrix4x4.CreateFromYawPitchRoll(StaticRotation.Y, StaticRotation.X, StaticRotation.Z) *
            Matrix4x4.CreateTranslation(StaticPosition);

        private WadToolClass _tool;
        private GizmoSkeletonEditor _gizmo;
        private LinePrimitiveRenderer _lines;

        private float _lastX;
        private float _lastY;

        public void Initialize(WadToolClass tool)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;
            _tool = tool;
            _gizmo = new GizmoSkeletonEditor(_tool, _tool.Configuration, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gizmo?.Dispose();
                _lines?.Dispose();
            }
            base.Dispose(disposing);
        }

        // ------------------------------------------------ Rendering

        protected override Vector4 ClearColor => Configuration?.RenderingItem_BackgroundColor ?? new Vector4(0.65f, 0.65f, 0.65f, 1.0f);

        public override float FieldOfView => Configuration?.RenderingItem_FieldOfView ?? 50.0f;
        public override float NavigationSpeedMouseWheelZoom => Configuration?.RenderingItem_NavigationSpeedMouseWheelZoom ?? 6.0f;
        public override float NavigationSpeedMouseZoom => Configuration?.RenderingItem_NavigationSpeedMouseZoom ?? 800.0f;
        public override float NavigationSpeedMouseTranslate => Configuration?.RenderingItem_NavigationSpeedMouseTranslate ?? 1500.0f;
        public override float NavigationSpeedMouseRotate => Configuration?.RenderingItem_NavigationSpeedMouseRotate ?? 4.0f;

        protected override void RenderContents(ICommandList cl, Matrix4x4 viewProjection)
        {
            var device = PreviewDevice.Device;
            var renderer = PreviewDevice.Renderer;
            _lines ??= new LinePrimitiveRenderer(device);

            // Bones (textured). Pack every bone into one instance batch so the
            // per-frame cbuffer is updated exactly once (multiple cbuffer
            // WriteDiscard updates between draws collapse into the last value
            // on some D3D11 drivers -- every bone would otherwise render with
            // the same transform).
            if (Skeleton != null)
            {
                renderer.BeginMeshBatch();
                foreach (var node in Skeleton)
                    if (node.Mesh != null)
                        renderer.QueueMesh(node.Mesh, node.GlobalTransform);
                renderer.FlushMeshBatch(cl, viewProjection);
            }

            // Line overlays: reference grid + selection bounding box.
            _lines.Begin();
            if (DrawGrid)
            {
                // Legacy GeometricPrimitive.GridPlane(8, 4) -> 8x4 cells over
                // [-1..+1] in legacy unit space; our preview camera lives in
                // WAD units (~256 per click), so scale up to match.
                _lines.AddGridXZ(size: 4096.0f, cells: 16, color: 0x80_FF_FF_FFu);
            }
            if (SelectedNode?.Mesh != null)
            {
                _lines.AddBox(SelectedNode.Mesh.BoundingBox, SelectedNode.GlobalTransform, color: 0xFF_00_FF_00u);
            }
            _lines.Flush(cl, viewProjection);

            // Gizmo overlay.
            if (DrawGizmo && SelectedNode != null)
            {
                var snap = _gizmo.GetPublicState();
                PreviewDevice.Gizmo.Render(cl, snap, viewProjection);
            }
        }

        // ------------------------------------------------ Mouse + picking

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            // Base class already zoomed; no extra work needed.
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Forward to base FIRST so the inherited _lastX/_lastY get primed
            // even when we early-return on a gizmo pick.
            base.OnMouseDown(e);
            _lastX = e.X;
            _lastY = e.Y;

            if (e.Button == MouseButtons.Left && _gizmo != null && Skeleton != null)
            {
                var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

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

                // Bone picking -- CPU ray/triangle test, identical to legacy.
                WadMeshBoneNode foundNode = null;
                float minDistance = float.PositiveInfinity;
                foreach (var node in Skeleton)
                {
                    if (DoNodePicking(ray, node, out float distance) && distance < minDistance)
                    {
                        minDistance = distance;
                        foundNode = node;
                    }
                }
                SelectedNode = foundNode;
                _tool.BonePicked();
            }

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_gizmo == null)
                return;

            var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);
            if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(ray)))
                Invalidate();
            if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ray))
                Invalidate();

            // Camera nav handled by base.OnMouseMove via right/middle drag.
            _lastX = e.X;
            _lastY = e.Y;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_gizmo != null && _gizmo.MouseUp())
                Invalidate();
        }

        private bool DoNodePicking(Ray ray, WadMeshBoneNode node, out float nodeDistance)
        {
            nodeDistance = 0;
            if (!Matrix4x4.Invert(node.GlobalTransform, out var inverse))
                return false;

            Vector3 from = MathC.HomogenousTransform(ray.Position, inverse);
            Vector3 to   = MathC.HomogenousTransform(ray.Position + ray.Direction, inverse);
            Ray localRay = new Ray(from, Vector3.Normalize(to - from));

            bool hit = false;
            float minDistance = float.PositiveInfinity;
            var mesh = node.Mesh;
            foreach (var poly in mesh.Polys)
            {
                Vector3 p0 = mesh.VertexPositions[poly.Index0];
                Vector3 p1 = mesh.VertexPositions[poly.Index1];
                Vector3 p2 = mesh.VertexPositions[poly.Index2];

                if (Collision.RayIntersectsTriangle(localRay, p0, p1, p2, true, out float d) && d < minDistance)
                {
                    minDistance = d;
                    hit = true;
                }

                if (poly.Shape == WadPolygonShape.Quad)
                {
                    Vector3 p3 = mesh.VertexPositions[poly.Index3];
                    if (Collision.RayIntersectsTriangle(localRay, p0, p2, p3, true, out d) && d < minDistance)
                    {
                        minDistance = d;
                        hit = true;
                    }
                }
            }

            if (hit) { nodeDistance = minDistance; return true; }
            return false;
        }

        public void UpdateModel()
        {
            // Drop cached per-mesh vertex buffers for the current skeleton --
            // a bone edit may have changed vertex positions in place.
            if (Skeleton != null)
                foreach (var node in Skeleton)
                    if (node.Mesh != null)
                        PreviewDevice.Renderer.InvalidateMesh(node.Mesh);
            Invalidate();
        }
    }
}
