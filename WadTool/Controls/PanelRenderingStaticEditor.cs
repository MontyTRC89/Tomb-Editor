using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Graphics;
using TombLib.RenderingV2.Preview;
using TombLib.RenderingV2.Rhi;
using TombLib.RenderingV2.Text;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool.Controls
{
    /// <summary>
    /// Static editor preview. Inherits the V2 <see cref="ItemPreviewPanel"/>
    /// for swapchain + camera plumbing; <see cref="RenderContents"/> draws the
    /// static mesh under the gizmo transform, optional visibility / collision
    /// boxes, lights, normals and the transform / light gizmos.
    /// </summary>
    public class PanelRenderingStaticEditor : ItemPreviewPanel
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

        public Matrix4x4 GizmoTransform =>
            Matrix4x4.CreateScale(StaticScale) *
            Matrix4x4.CreateFromYawPitchRoll(StaticRotation.Y, StaticRotation.X, StaticRotation.Z) *
            Matrix4x4.CreateTranslation(StaticPosition);

        private WadToolClass _tool;
        private GizmoStaticEditor _gizmo;
        private GizmoStaticEditorLight _gizmoLight;
        private LinePrimitiveRenderer _lines;

        private float _lastX;
        private float _lastY;

        // Override the base camera default (centred at +256 Y) with the
        // legacy static-editor framing.
        public PanelRenderingStaticEditor()
        {
            Camera = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0,
                -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 100, 1000000,
                (float)Math.PI / 4.0f);
        }

        public void Initialize(WadToolClass tool)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;
            _tool = tool;
            _gizmo      = new GizmoStaticEditor(_tool.Configuration, this);
            _gizmoLight = new GizmoStaticEditorLight(_tool.Configuration, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gizmo?.Dispose();
                _gizmoLight?.Dispose();
                _lines?.Dispose();
            }
            base.Dispose(disposing);
        }

        // ------------------------------------------------ Rendering

        protected override Vector4 ClearColor => Configuration?.RenderingItem_BackgroundColor ?? new Vector4(0.39f, 0.58f, 0.93f, 1f);

        public override float FieldOfView => Configuration?.RenderingItem_FieldOfView ?? 50f;
        public override float NavigationSpeedMouseWheelZoom => Configuration?.RenderingItem_NavigationSpeedMouseWheelZoom ?? 6f;
        public override float NavigationSpeedMouseZoom => Configuration?.RenderingItem_NavigationSpeedMouseZoom ?? 800f;
        public override float NavigationSpeedMouseTranslate => Configuration?.RenderingItem_NavigationSpeedMouseTranslate ?? 1500f;
        public override float NavigationSpeedMouseRotate => Configuration?.RenderingItem_NavigationSpeedMouseRotate ?? 4f;

        protected override void RenderContents(ICommandList cl, Matrix4x4 viewProjection)
        {
            var device = PreviewDevice.Device;
            _lines ??= new LinePrimitiveRenderer(device);

            // Mesh (textured), under the gizmo transform.
            if (Static?.Mesh != null)
            {
                // The model is invalidated on every Static.Mesh edit via
                // UpdateMesh(), so changes to vertex positions / colours go
                // through the cache cleanly.
                PreviewDevice.Renderer.RenderMesh(cl, Static.Mesh, GizmoTransform, viewProjection);
            }

            // -------- Line overlays (grid, boxes, light spheres, normals) ----
            _lines.Begin();

            if (DrawGrid)
            {
                // Legacy GridPlane(8, 4) → 8×4 cells over [-1..+1]; preview
                // camera works in WAD units so scale up to the same on-screen
                // density as the skeleton/main preview panels.
                _lines.AddGridXZ(size: 4096f, cells: 16, color: 0xFF_FF_FF_FFu);
            }

            if (Static != null)
            {
                if (DrawVisibilityBox)
                    _lines.AddBox(Static.VisibilityBox, Matrix4x4.Identity, 0xFF_FF_00_00u); // blue (BGRA bits: B=FF)
                if (DrawCollisionBox)
                    _lines.AddBox(Static.CollisionBox,  Matrix4x4.Identity, 0xFF_00_FF_00u); // green

                if (DrawLights)
                {
                    foreach (var light in Static.Lights)
                    {
                        // Little marker sphere at the light position (yellow).
                        _lines.AddWireSphere(light.Position, radius: 128f, segments: 16,
                                              color: 0xFF_00_FF_FFu); // RGBA: R=FF, G=FF (yellow)

                        // Selected light's radius (green) — legacy uses
                        // light.Radius * SectorSize (1024) for the visible
                        // sphere extent.
                        if (SelectedLight == light)
                        {
                            _lines.AddWireSphere(light.Position,
                                                  radius: 1024f * light.Radius,
                                                  segments: 24,
                                                  color: 0xFF_00_FF_00u);
                        }
                    }
                }

                if (DrawNormals && Static.Mesh != null)
                {
                    var world = GizmoTransform;
                    int count = Math.Min(Static.Mesh.VertexNormals.Count, Static.Mesh.VertexPositions.Count);
                    for (int i = 0; i < count; i++)
                    {
                        var rawNormal = Static.Mesh.VertexNormals[i];
                        float len = rawNormal.Length();
                        if (len <= 1e-6f) continue;
                        var p = Vector3.Transform(Static.Mesh.VertexPositions[i], world);
                        var n = Vector3.TransformNormal(rawNormal / len, world);
                        _lines.AddLine(p, p + n * 32.0f, 0xFF_FF_FF_FFu, 0xFF_FF_FF_FFu);
                    }
                }
            }

            _lines.Flush(cl, viewProjection);

            // -------- Gizmos -------------------------------------------------
            if (DrawGizmo && _gizmo != null && Static != null)
            {
                var snap = _gizmo.GetPublicState();
                PreviewDevice.Gizmo.Render(cl, snap, viewProjection);
            }

            if (SelectedLight != null && _gizmoLight != null)
            {
                var snap = _gizmoLight.GetPublicState();
                PreviewDevice.Gizmo.Render(cl, snap, viewProjection);
            }
        }

        protected override void CollectText(List<TextLabel> labels, Matrix4x4 viewProjection)
        {
            // Pin position / rotation / scale to the top-left corner of the
            // panel — matches the legacy debug overlay.
            string msg =
                "Position: " + StaticPosition +
                "\nRotation: " + StaticRotation.X * (180.0 / Math.PI) +
                "\nScale: " + StaticScale;
            labels.Add(TextLabel.Screen(msg,
                screenPosition: new Vector2(10, 10),
                color: new Vector4(1f, 1f, 1f, 1f),
                background: Configuration?.Rendering3D_DrawFontOverlays ?? false,
                alignment: new Vector2(0f, 0f)));
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

        private void PlaceLight(int x, int y)
        {
            var ray = Ray.GetPickRay(Camera, ClientSize, x, y);
            var plane = new Plane(Vector3.UnitY, 0.0f);
            if (!Collision.RayIntersectsPlane(ray, plane, out Vector3 point))
                return;

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
            // Forward to base FIRST so the inherited _lastX/_lastY are always
            // primed — otherwise the first right/middle drag delta after a
            // gizmo-pick (early-return below) is computed against stale values.
            base.OnMouseDown(e);
            _lastX = e.X;
            _lastY = e.Y;

            if (e.Button == MouseButtons.Left && Static != null)
            {
                var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

                if (Action != StaticEditorAction.PlaceLight)
                {
                    if (DrawGizmo && _gizmo != null)
                    {
                        var result = _gizmo.DoPicking(ray);
                        if (result != null)
                        {
                            _gizmo.ActivateGizmo(result);
                            Invalidate();
                            return;
                        }
                    }

                    if (SelectedLight != null && _gizmoLight != null)
                    {
                        var result = _gizmoLight.DoPicking(ray);
                        if (result != null)
                        {
                            _gizmoLight.ActivateGizmo(result);
                            Invalidate();
                            return;
                        }
                    }

                    // Light picking — sphere test against each light marker.
                    float minDistance = float.MaxValue;
                    SelectedLight = null;
                    foreach (var light in Static.Lights)
                    {
                        if (Collision.RayIntersectsSphere(ray, new BoundingSphere(light.Position, 128f),
                                                          out float distance))
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

            if (_gizmo != null)
            {
                if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(ray)))
                    Invalidate();
                if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ray))
                    Invalidate();
            }

            if (_gizmoLight != null)
            {
                if (_gizmoLight.GizmoUpdateHoverEffect(_gizmoLight.DoPicking(ray)))
                    Invalidate();
                if (_gizmoLight.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ray))
                    Invalidate();
            }

            Cursor = Action == StaticEditorAction.Normal ? Cursors.Default : Cursors.Cross;

            _lastX = e.X;
            _lastY = e.Y;

            // Base class already handled the camera nav via right/middle drag.

            if (FindForm() is FormStaticEditor form)
                form.UpdatePositionUI();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_gizmo != null && _gizmo.MouseUp())
                Invalidate();
            if (_gizmoLight != null && _gizmoLight.MouseUp())
                Invalidate();

            if (FindForm() is FormStaticEditor form)
                form.UpdatePositionUI();
        }

        // ------------------------------------------------ Mesh / lights state

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
                    var p = Vector3.Transform(Static.Mesh.VertexPositions[i], world);
                    var n = Vector3.TransformNormal(Static.Mesh.VertexNormals[i] / Static.Mesh.VertexNormals[i].Length(), world);

                    var lightDirection = light.Position - p;
                    if (lightDirection.Length() > light.Radius * 1024)
                        continue;

                    var l = lightDirection / lightDirection.Length();
                    float dot = Vector3.Dot(n, l);
                    if (dot <= 0)
                        continue;

                    newShade += dot * light.Intensity * (1.0f - (light.Position - p).Length() /
                                (light.Radius * TombLib.LevelData.Level.SectorSizeUnit));
                }

                Static.Mesh.VertexColors.Add(new Vector3(Math.Min(newShade, 1.0f)));
            }

            // The cached vertex buffer baked the old colours — invalidate so
            // the next paint re-uploads with the new shade.
            PreviewDevice.Renderer.InvalidateMesh(Static.Mesh);

            _tool.StaticLightsChanged();
        }

        public void UpdateMesh()
        {
            // Drop the cached static mesh + per-object cache so vertex
            // positions / colours edited externally are picked up on the next
            // paint.
            if (Static?.Mesh != null)
                PreviewDevice.Renderer.InvalidateMesh(Static.Mesh);
            PreviewDevice.Renderer.InvalidateAll();
        }
    }
}
