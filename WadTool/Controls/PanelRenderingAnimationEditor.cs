using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.RenderingV2.Preview;
using TombLib.RenderingV2.Rhi;
using TombLib.RenderingV2.Text;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool.Controls
{
    /// <summary>
    /// Animation editor preview. Inherits the V2 <see cref="ItemPreviewPanel"/>
    /// for swapchain + camera plumbing; <see cref="RenderContents"/> renders
    /// the moveable's current pose using the V2 mesh renderer, plus selection
    /// + collision boxes, the reference grid, and the transform gizmo.
    ///
    /// <para>The CPU-side skeleton + animation pose is still computed via the
    /// legacy <see cref="AnimatedModel"/>, but only its <c>Bones</c>,
    /// <c>BindPoseTransforms</c> and <c>AnimationTransforms</c> are read —
    /// the GPU buffers it allocates are unused.</para>
    /// </summary>
    public class PanelRenderingAnimationEditor : ItemPreviewPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Configuration { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimatedModel Model => _model;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimatedModel Skin => _skinModel;

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
        public WadMesh SelectedMesh
        {
            get { return _selectedMesh; }
            set { _selectedMesh = value; Invalidate(); }
        }
        private WadMesh _selectedMesh;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DisablePicking { get; set; }

        // General state
        private AnimationEditor _editor;

        // Per-bone source meshes resolved at init time, matched 1:1 with the
        // bone count of _model.Meshes. We feed the WadMesh directly to the V2
        // renderer.
        private WadMoveable _renderMoveable;

        // CPU-only skeleton state (no GPU buffers). The V2 renderer reads
        // _model.AnimationTransforms[i] alongside _renderMoveable.Bones[i].Mesh.
        private AnimatedModel _model;
        private AnimatedModel _skinModel;

        // V2 overlay renderers.
        private LinePrimitiveRenderer _lines;
        private GizmoAnimationEditor _gizmo;

        // Interaction state
        private float _lastX;
        private float _lastY;

        public void InitializeRendering(AnimationEditor editor, WadMoveable skin)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            ResetCamera();
            _editor = editor;
            Configuration = _editor.Tool.Configuration;

            _model = AnimatedModel.FromWadMoveable(editor.Moveable, loadAnimations: true);

            _renderMoveable = editor.Moveable;
            if (skin != null)
            {
                var replaced = editor.Moveable.ReplaceDummyMeshes(skin);
                _skinModel = AnimatedModel.FromWadMoveable(replaced, loadAnimations: true);
                _renderMoveable = replaced;
            }

            _gizmo = new GizmoAnimationEditor(editor, this);
        }

        public new void ResetCamera()
        {
            Camera = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0,
                -(float)Math.PI / 2, (float)Math.PI / 2,
                2048.0f, 100, 1000000, (float)Math.PI / 4.0f);
            Invalidate();
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

            // --- Meshes (per-bone) -------------------------------------------
            // Single batched flush — see PanelRenderingSkeleton for the
            // rationale (one cbuffer update per frame, multi-draw via
            // per-instance offsets).
            if (_model != null && _renderMoveable != null)
            {
                bool validAnim = _editor != null && _editor.ValidAnimationAndFrames;
                int boneCount = Math.Min(_renderMoveable.Bones.Count,
                                         Math.Max(_model.Meshes.Count, _model.Bones.Count));

                var renderer = PreviewDevice.Renderer;
                renderer.BeginMeshBatch();
                for (int i = 0; i < boneCount; i++)
                {
                    var wadMesh = _renderMoveable.Bones[i].Mesh;
                    if (wadMesh == null || wadMesh.VertexPositions.Count == 0)
                        continue;

                    Matrix4x4 transform = validAnim
                        ? _model.AnimationTransforms[i]
                        : _model.Bones[i].GlobalTransform;

                    bool selected = validAnim && i < _model.Meshes.Count &&
                                    SelectedMesh == _model.Meshes[i];
                    if (selected)
                        renderer.QueueMesh(wadMesh, transform, new Vector4(1f, 0f, 0f, 1f));
                    else
                        renderer.QueueMesh(wadMesh, transform);
                }
                renderer.FlushMeshBatch(cl, viewProjection);
            }

            // --- Line overlays -----------------------------------------------
            _lines.Begin();

            if (Configuration != null && Configuration.AnimationEditor_ShowGrid)
            {
                // The grid follows the legacy "infinite plane" by snapping
                // to GridPosition % 4096 — gives a parallax-feel without ever
                // running out of cells.
                var shift = Matrix4x4.CreateTranslation(new Vector3(-GridPosition.X, GridPosition.Y, -GridPosition.Z));
                AddGridShifted(_lines, shift, size: 4096f, cells: 16, color: 0xFF_FF_FF_FFu);
            }

            if (_editor != null && _editor.ValidAnimationAndFrames && _model != null)
            {
                // Selected mesh bounding box (red).
                if (SelectedMesh != null && _model.Meshes.Count > 0)
                {
                    int meshIndex = _model.Meshes.IndexOf(SelectedMesh);
                    var bbSource = _skinModel != null ? _skinModel : _model;
                    if (meshIndex >= 0 && meshIndex < bbSource.Meshes.Count)
                    {
                        _lines.AddBox(bbSource.Meshes[meshIndex].BoundingBox,
                                       _model.AnimationTransforms[meshIndex],
                                       color: 0xFF_00_00_FFu); // RGBA: R=FF
                    }
                }

                if (Configuration != null && Configuration.AnimationEditor_ShowCollisionBox)
                {
                    _lines.AddBox(_editor.CurrentKeyFrame.BoundingBox,
                                   Matrix4x4.Identity,
                                   color: 0xFF_00_FF_00u); // green
                }
            }

            _lines.Flush(cl, viewProjection);

            // --- Gizmo --------------------------------------------------------
            if (Configuration != null && Configuration.AnimationEditor_ShowGizmo &&
                SelectedMesh != null && _editor != null && _editor.ValidAnimationAndFrames &&
                _gizmo != null)
            {
                var snap = _gizmo.GetPublicState();
                PreviewDevice.Gizmo.Render(cl, snap, viewProjection);
            }
        }

        private static void AddGridShifted(LinePrimitiveRenderer lines, Matrix4x4 shift,
                                            float size, int cells, uint color)
        {
            if (cells <= 0) return;
            float half = size * 0.5f;
            float step = size / cells;
            for (int i = 0; i <= cells; i++)
            {
                float x = -half + i * step;
                float z = -half + i * step;
                Vector3 a = Vector3.Transform(new Vector3(x, 0f, -half), shift);
                Vector3 b = Vector3.Transform(new Vector3(x, 0f,  half), shift);
                Vector3 c = Vector3.Transform(new Vector3(-half, 0f, z), shift);
                Vector3 d = Vector3.Transform(new Vector3( half, 0f, z), shift);
                lines.AddLine(a, b, color, color);
                lines.AddLine(c, d, color, color);
            }
        }

        protected override void CollectText(List<TextLabel> labels, Matrix4x4 viewProjection)
        {
            if (_editor == null || _editor.CurrentAnim == null ||
                Configuration == null || !Configuration.RenderingItem_ShowDebugInfo)
                return;

            string msg = "Frame: " + (_editor.CurrentFrameIndex + 1) + "/" +
                          _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
            if (SelectedMesh != null && _model != null)
            {
                int meshIndex = _model.Meshes.IndexOf(SelectedMesh);
                if (meshIndex >= 0)
                {
                    msg += "\nMesh: " + SelectedMesh.Name;
                    msg += "\nBone: " + _model.Bones[meshIndex].Name;
                    if (_editor.CurrentKeyFrame != null)
                        msg += "\nRotation: " + _editor.CurrentKeyFrame.Rotations[meshIndex];
                }
            }

            labels.Add(TextLabel.Screen(msg,
                screenPosition: new Vector2(10, 10),
                color: new Vector4(1f, 1f, 1f, 1f),
                background: Configuration.Rendering3D_DrawFontOverlays,
                alignment: new Vector2(0f, 0f)));
        }

        // ------------------------------------------------ Mouse / picking

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (Form.ActiveForm == FindForm())
                Focus();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Forward to base FIRST so the inherited _lastX/_lastY get primed
            // even when we early-return on a gizmo pick — otherwise the next
            // right/middle drag delta is computed against stale state.
            base.OnMouseDown(e);
            _lastX = e.X;
            _lastY = e.Y;

            if (e.Button == MouseButtons.Left)
            {
                if (_editor != null && _editor.ValidAnimationAndFrames && !DisablePicking &&
                    _model != null && _renderMoveable != null)
                {
                    var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

                    if (Configuration != null && Configuration.AnimationEditor_ShowGizmo && _gizmo != null)
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

                    // Bone picking — CPU ray/triangle test against each bone's
                    // WadMesh in local space (matches legacy semantics).
                    WadMesh foundMesh = null;
                    float minDistance = float.PositiveInfinity;
                    int boneCount = Math.Min(_renderMoveable.Bones.Count, _model.Meshes.Count);
                    for (int i = 0; i < boneCount; i++)
                    {
                        if (DoMeshPicking(ray, i, out float distance) && distance < minDistance)
                        {
                            minDistance = distance;
                            foundMesh = _model.Meshes[i];
                        }
                    }

                    if (SelectedMesh != foundMesh)
                    {
                        SelectedMesh = foundMesh;
                        _editor.Tool.AnimationEditorMeshSelected(Model, SelectedMesh);
                    }
                }
                else if (_editor != null)
                {
                    SelectedMesh = null;
                    _editor.Tool.AnimationEditorMeshSelected(Model, SelectedMesh);
                }
            }

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_gizmo == null) return;

            var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);
            if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(ray)))
                Invalidate();
            if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ray))
                Invalidate();

            // Base class handled camera nav.
            _lastX = e.X;
            _lastY = e.Y;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_gizmo != null && _gizmo.MouseUp())
                Invalidate();
        }

        private bool DoMeshPicking(Ray ray, int meshIndex, out float meshDistance)
        {
            meshDistance = 0;

            var transform = _editor.CurrentAnim != null
                ? _model.AnimationTransforms[meshIndex]
                : _model.BindPoseTransforms[meshIndex];
            if (!Matrix4x4.Invert(transform, out Matrix4x4 inverse))
                return false;

            Vector3 from = MathC.HomogenousTransform(ray.Position, inverse);
            Vector3 to   = MathC.HomogenousTransform(ray.Position + ray.Direction, inverse);
            Ray localRay = new Ray(from, Vector3.Normalize(to - from));

            // Pick against the skin geometry if available (matches what the
            // user sees), falling back to the raw moveable mesh otherwise.
            var src = (_skinModel != null && _renderMoveable != null)
                ? _renderMoveable.Bones[meshIndex].Mesh
                : _editor.Moveable.Bones[meshIndex].Mesh;
            if (src == null) return false;

            bool hit = false;
            float minDistance = float.PositiveInfinity;
            foreach (var poly in src.Polys)
            {
                Vector3 p0 = src.VertexPositions[poly.Index0];
                Vector3 p1 = src.VertexPositions[poly.Index1];
                Vector3 p2 = src.VertexPositions[poly.Index2];

                if (Collision.RayIntersectsTriangle(localRay, p0, p1, p2, true, out float d) && d < minDistance)
                {
                    minDistance = d;
                    hit = true;
                }

                if (poly.Shape == WadPolygonShape.Quad)
                {
                    Vector3 p3 = src.VertexPositions[poly.Index3];
                    if (Collision.RayIntersectsTriangle(localRay, p0, p2, p3, true, out d) && d < minDistance)
                    {
                        minDistance = d;
                        hit = true;
                    }
                }
            }

            if (hit) { meshDistance = minDistance; return true; }
            return false;
        }
    }
}
