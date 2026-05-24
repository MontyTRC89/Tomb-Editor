using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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
    /// Mesh editor preview. Inherits the V2 <see cref="ItemPreviewPanel"/> for
    /// swapchain + camera plumbing; <see cref="RenderContents"/> draws the
    /// currently-edited <see cref="WadMesh"/> plus per-mode overlays (vertex
    /// sphere markers, normals, bounding sphere, gizmo).
    ///
    /// <para>The V2 preview pipeline always renders textured + linear-filtered,
    /// so the legacy WireframeMode / Bilinear / AlphaTest toggles are accepted
    /// for API compatibility but currently have no visible effect.</para>
    /// </summary>
    public class PanelRenderingMesh : ItemPreviewPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadMesh Mesh
        {
            get { return _mesh; }
            set
            {
                if (_mesh == value)
                    return;

                _mesh = value;
                _previewMesh = _mesh == null ? null : _mesh.Clone();
                InvalidateMeshCache();

                if (ResetCameraOnMeshChange)
                    ResetCamera();

                if (EditingMode == MeshEditingMode.VertexWeights)
                    ColorizeVertexWeights();

                CurrentElement = -1;
            }
        }
        private WadMesh _mesh;

        public WadMesh VisibleMesh => ((_editingMode == MeshEditingMode.VertexWeights || _previewTimer.Enabled) && _previewMesh != null) ? _previewMesh : _mesh;
        private WadMesh _previewMesh;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MeshEditingMode EditingMode
        {
            get { return _editingMode; }
            set
            {
                if (_editingMode == value)
                    return;

                if (value != MeshEditingMode.VertexEffects)
                    StopPreview();

                if (value == MeshEditingMode.VertexWeights)
                    ColorizeVertexWeights();

                CurrentElement = -1;
                _editingMode = value;
                Invalidate();
            }
        }
        private MeshEditingMode _editingMode = MeshEditingMode.None;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentElement
        {
            get { return _currentElement; }
            set
            {
                if (Mesh == null)
                    return;

                bool engageUndo = false;

                switch (EditingMode)
                {
                    case MeshEditingMode.FaceAttributes:
                        if (_currentElement == value) return;
                        SelectElement(value);
                        engageUndo = !Control.ModifierKeys.HasFlag(Keys.Alt);
                        break;

                    case MeshEditingMode.VertexEffects:
                    case MeshEditingMode.VertexColorsAndNormals:
                    case MeshEditingMode.VertexWeights:
                    case MeshEditingMode.VertexRemap:
                        SelectElement(value);
                        engageUndo = !Control.ModifierKeys.HasFlag(Keys.Alt) && EditingMode != MeshEditingMode.VertexRemap;
                        break;

                    default:
                        _currentElement = -1;
                        break;
                }

                if (engageUndo && !_actionStarted && value != -1)
                {
                    _tool.UndoManager.PushMeshChanged(this);
                    _actionStarted = true;
                }

                _tool.MeshEditorElementChanged(_currentElement);

                if (EditingMode == MeshEditingMode.VertexWeights && value != -1)
                    ColorizeVertexWeights();
            }
        }
        private int _currentElement = -1;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool WireframeMode
        {
            get { return _wireframeMode; }
            set
            {
                if (_wireframeMode == value)
                    return;
                _wireframeMode = value;
                Invalidate();
            }
        }
        private bool _wireframeMode = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AlphaTest
        {
            get { return _alphaTest; }
            set
            {
                if (_alphaTest == value)
                    return;
                _alphaTest = value;
                Invalidate();
            }
        }
        private bool _alphaTest = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Bilinear
        {
            get { return _bilinear; }
            set
            {
                if (_bilinear == value)
                    return;
                _bilinear = value;
                Invalidate();
            }
        }
        private bool _bilinear = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGrid
        {
            get { return _drawGrid; }
            set
            {
                if (_drawGrid == value)
                    return;
                _drawGrid = value;
                Invalidate();
            }
        }
        private bool _drawGrid = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawExtraInfo
        {
            get { return _drawInformationForAllElements; }
            set
            {
                if (_drawInformationForAllElements == value)
                    return;
                _drawInformationForAllElements = value;
                Invalidate();
            }
        }
        private bool _drawInformationForAllElements = false;

        public bool ResetCameraOnMeshChange = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SafeVertexRemapLimit
        {
            // HACK: Determine remappable vertices (only for legacy engines).
            // For more info: https://www.tombraiderforums.com/showthread.php?t=132749
            get
            {
                int safeIndex = int.MaxValue;
                if (_tool.DestinationWad.GameVersion != TRVersion.Game.TombEngine)
                {
                    if (_mesh.VertexPositions.Count <= 255)
                        safeIndex = 127;
                    else
                    {
                        var step = (Math.Truncate((float)_mesh.VertexPositions.Count / 256.0f) - 1) * 256.0f;
                        safeIndex = _mesh.VertexPositions.Count - (int)step - 1;
                        if (safeIndex > 127) safeIndex = 127;
                    }
                }
                return safeIndex;
            }
        }

        private float VertexSphereRadius
        {
            get
            {
                if (Mesh == null || Mesh.VertexPositions.Count == 0)
                    return 0;

                var distances = new List<float>();
                foreach (var p in Mesh.Polys)
                {
                    distances.Add(Vector3.Distance(Mesh.VertexPositions[p.Index0], Mesh.VertexPositions[p.Index1]));
                    distances.Add(Vector3.Distance(Mesh.VertexPositions[p.Index1], Mesh.VertexPositions[p.Index2]));
                    if (p.IsTriangle)
                        distances.Add(Vector3.Distance(Mesh.VertexPositions[p.Index2], Mesh.VertexPositions[p.Index0]));
                    else
                    {
                        distances.Add(Vector3.Distance(Mesh.VertexPositions[p.Index2], Mesh.VertexPositions[p.Index3]));
                        distances.Add(Vector3.Distance(Mesh.VertexPositions[p.Index3], Mesh.VertexPositions[p.Index0]));
                    }
                }
                return distances.Sum() / distances.Count / 12.0f;
            }
        }

        // ------- General state -----------------------------------------------
        private WadToolClass _tool;
        private GizmoMeshEditor _gizmo;
        private LinePrimitiveRenderer _lines;

        // ------- Interaction state -------------------------------------------
        private bool _actionStarted;
        private bool _highlightFace;
        private Point _lastMousePosition;
        private List<Vector3> _lastElementPos = new List<Vector3>();
        private List<int> _clickchain = new List<int>();

        // ------- Vertex effect preview --------------------------------------
        private readonly Timer _previewTimer = new Timer { Interval = 33 };
        private int _frameCount;

        // ------- Vertex weight preview --------------------------------------
        private const int _maxBones = 32;
        private Vector4[] _boneColors = new Vector4[_maxBones];

        // ------- Constants ---------------------------------------------------
        private readonly List<int> _oldLaraHairIndices   = new List<int>() { 37, 38, 39, 40 };
        private readonly List<int> _youngLaraHairIndices = new List<int>() { 68, 69, 70, 71, 76, 77, 78, 79 };

        protected override Vector4 ClearColor => _tool?.Configuration?.RenderingItem_BackgroundColor ?? new Vector4(0.39f, 0.58f, 0.93f, 1f);

        public override float FieldOfView => _tool?.Configuration?.RenderingItem_FieldOfView ?? 50f;
        // Mesh editor operates at a much smaller scale than the other previews,
        // so divide the wheel-zoom speed by 4 (legacy behaviour).
        public override float NavigationSpeedMouseWheelZoom => (_tool?.Configuration?.RenderingItem_NavigationSpeedMouseWheelZoom ?? 6f) / 4f;
        public override float NavigationSpeedMouseZoom => _tool?.Configuration?.RenderingItem_NavigationSpeedMouseZoom ?? 800f;
        public override float NavigationSpeedMouseTranslate => _tool?.Configuration?.RenderingItem_NavigationSpeedMouseTranslate ?? 1500f;
        public override float NavigationSpeedMouseRotate => _tool?.Configuration?.RenderingItem_NavigationSpeedMouseRotate ?? 4f;

        public PanelRenderingMesh()
        {
            // Mesh editor opens centred on origin (legacy default), not the
            // base panel's +256-Y offset.
            Camera = new ArcBallCamera(new Vector3(0.0f, 0.0f, 0.0f), 0, 0,
                -(float)Math.PI / 2, (float)Math.PI / 2,
                512.0f, 100, 1000000, (float)Math.PI / 4.0f);

            _previewTimer.Tick += new EventHandler(PreviewTimer_Tick);
            GenerateBoneColors();
        }

        public void InitializeRendering(WadToolClass tool, DeviceManager deviceManager)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;
            _tool = tool;
            _gizmo = new GizmoMeshEditor(_tool.Configuration, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _previewTimer.Stop();
                _previewTimer.Tick -= new EventHandler(PreviewTimer_Tick);
                _gizmo?.Dispose();
                _lines?.Dispose();
            }
            base.Dispose(disposing);
        }

        // ------------------------------------------------ Rendering

        protected override void RenderContents(ICommandList cl, Matrix4x4 viewProjection)
        {
            if (VisibleMesh == null)
                return;

            // Mesh edits in the mesh editor happen in-place on the WadMesh
            // (poly textures, vertex colours, weights, positions, …). The V2
            // mesh cache keys by reference, so without an explicit drop the
            // cached VB would keep showing the pre-edit data. Legacy parity:
            // WadRenderer.GetStatic re-built every paint via DataVersion bumps.
            PreviewDevice.Renderer.InvalidateMesh(VisibleMesh);

            var device = PreviewDevice.Device;
            _lines ??= new LinePrimitiveRenderer(device);

            _lines.Begin();

            if (DrawGrid)
            {
                // Mesh editor uses a much smaller working area than the
                // skeleton/item previews; legacy GridPlane(8, 4) → ~256-unit
                // cells in this view.
                _lines.AddGridXZ(size: 1024f, cells: 16, color: 0xFF_FF_FF_FFu);
            }

            // The textured mesh draws underneath every overlay. In the legacy
            // weight-edit mode the textured pass was skipped so flat weight
            // colours filled the screen; V2 doesn't have a wireframe-only
            // pipeline, so we always draw the textured mesh — vertex spheres
            // + text labels carry the weight information instead.
            PreviewDevice.Renderer.RenderMesh(cl, VisibleMesh, Matrix4x4.Identity, viewProjection);

            // --- Vertex sphere markers (vertex-targeted modes) ---------------
            int safeIndex = (Mesh != null) ? SafeVertexRemapLimit : int.MaxValue;
            if (Mesh != null &&
                (EditingMode == MeshEditingMode.VertexRemap ||
                 EditingMode == MeshEditingMode.VertexEffects ||
                 EditingMode == MeshEditingMode.VertexWeights ||
                 EditingMode == MeshEditingMode.VertexColorsAndNormals))
            {
                float r = Math.Max(VertexSphereRadius, 1.0f);
                for (int i = 0; i < Mesh.VertexPositions.Count; i++)
                {
                    var selected = i == _currentElement;
                    if (!selected && _currentElement != -1 &&
                        Mesh.VertexPositions[i] == Mesh.VertexPositions[_currentElement])
                        continue;

                    uint color = GetVertexMarkerColor(i, safeIndex, selected);
                    _lines.AddWireSphere(VisibleMesh.VertexPositions[i], r, segments: 8, color: color);
                }

                // Normals visualisation (only in colors+normals mode).
                if (EditingMode == MeshEditingMode.VertexColorsAndNormals && Mesh.HasNormals)
                {
                    float normalLen = r * 3.0f;
                    int count = Math.Min(Mesh.VertexNormals.Count, Mesh.VertexPositions.Count);
                    for (int i = 0; i < count; i++)
                    {
                        var n = Mesh.VertexNormals[i];
                        if (n.LengthSquared() <= 1e-6f) continue;
                        var p = Mesh.VertexPositions[i];
                        var nn = n / n.Length();
                        uint nc = i == _currentElement ? 0xFF_00_00_FFu : 0xFF_FF_FF_FFu;
                        _lines.AddLine(p, p + nn * normalLen, nc, nc);
                    }
                }
            }

            // --- Selected face outline (FaceAttributes mode) -----------------
            if (Mesh != null && EditingMode == MeshEditingMode.FaceAttributes &&
                _currentElement >= 0 && _currentElement < Mesh.Polys.Count)
            {
                var poly = Mesh.Polys[_currentElement];
                Vector3 p0 = Mesh.VertexPositions[poly.Index0];
                Vector3 p1 = Mesh.VertexPositions[poly.Index1];
                Vector3 p2 = Mesh.VertexPositions[poly.Index2];
                const uint red = 0xFF_00_00_FFu;
                if (poly.Shape == WadPolygonShape.Triangle)
                {
                    _lines.AddLine(p0, p1, red, red);
                    _lines.AddLine(p1, p2, red, red);
                    _lines.AddLine(p2, p0, red, red);
                }
                else
                {
                    Vector3 p3 = Mesh.VertexPositions[poly.Index3];
                    _lines.AddLine(p0, p1, red, red);
                    _lines.AddLine(p1, p2, red, red);
                    _lines.AddLine(p2, p3, red, red);
                    _lines.AddLine(p3, p0, red, red);
                }
            }

            // --- Bounding sphere (Sphere mode) -------------------------------
            if (Mesh != null && EditingMode == MeshEditingMode.Sphere)
            {
                _lines.AddWireSphere(Mesh.BoundingSphere.Center, Mesh.BoundingSphere.Radius,
                                     segments: 32, color: 0x80_FF_FF_FFu);
            }

            _lines.Flush(cl, viewProjection);

            // --- Gizmo (Sphere mode w/ extra info) ---------------------------
            if (Mesh != null && EditingMode == MeshEditingMode.Sphere && DrawExtraInfo && _gizmo != null)
            {
                var snap = _gizmo.GetPublicState();
                PreviewDevice.Gizmo.Render(cl, snap, viewProjection);
            }
        }

        private uint GetVertexMarkerColor(int i, int safeIndex, bool selected)
        {
            if (selected) return 0x80_00_00_FFu; // red, semi-transparent

            switch (EditingMode)
            {
                case MeshEditingMode.VertexRemap:
                    return i <= safeIndex
                        ? 0xCC_FF_4D_00u   // blue (safe)
                        : 0xCC_00_CC_CCu;  // yellow (unsafe)
                case MeshEditingMode.VertexEffects:
                    if (Mesh.HasAttributes)
                    {
                        var a = Mesh.VertexAttributes[i];
                        float glow = a.Glow == 0 ? 0 : (a.Glow + 64.0f) / 128.0f;
                        float move = a.Move == 0 ? 0 : (a.Move + 64.0f) / 128.0f;
                        // Pack as RGBA with R=move (legacy used B=move), G=glow.
                        return PackColor(0f, glow, move, 0.7f);
                    }
                    return 0xCC_00_00_00u; // black
                case MeshEditingMode.VertexWeights:
                    return Mesh.HasWeights ? 0xFF_FF_FF_FFu : 0xFF_00_00_00u;
                case MeshEditingMode.VertexColorsAndNormals:
                    return 0x99_FF_FF_FFu; // white, semi-transparent
                default:
                    return 0xFF_FF_FF_FFu;
            }
        }

        private static uint PackColor(float r, float g, float b, float a)
        {
            byte br = (byte)Math.Clamp((int)(r * 255f + 0.5f), 0, 255);
            byte bg = (byte)Math.Clamp((int)(g * 255f + 0.5f), 0, 255);
            byte bb = (byte)Math.Clamp((int)(b * 255f + 0.5f), 0, 255);
            byte ba = (byte)Math.Clamp((int)(a * 255f + 0.5f), 0, 255);
            return (uint)(br | (bg << 8) | (bb << 16) | (ba << 24));
        }

        protected override void CollectText(List<TextLabel> labels, Matrix4x4 viewProjection)
        {
            if (Mesh == null || _tool == null) return;
            if (!(EditingMode == MeshEditingMode.VertexRemap ||
                  EditingMode == MeshEditingMode.VertexEffects ||
                  EditingMode == MeshEditingMode.VertexWeights ||
                  EditingMode == MeshEditingMode.VertexColorsAndNormals))
                return;

            bool overlays = _tool.Configuration.Rendering3D_DrawFontOverlays;

            for (int i = 0; i < Mesh.VertexPositions.Count; i++)
            {
                var selected = i == _currentElement;
                if (!(DrawExtraInfo || selected)) continue;

                // Skip duplicates that share a vertex coord with the selected
                // one — they overlap on screen.
                if (!selected && _currentElement != -1 &&
                    Mesh.VertexPositions[i] == Mesh.VertexPositions[_currentElement])
                    continue;

                string msg = null;
                switch (EditingMode)
                {
                    case MeshEditingMode.VertexRemap: msg = i.ToString(); break;
                    case MeshEditingMode.VertexWeights:
                        if (Mesh.HasWeights) msg = Mesh.VertexWeights[i].ToString();
                        break;
                    case MeshEditingMode.VertexEffects:
                        if (Mesh.HasAttributes)
                            msg = Mesh.VertexAttributes[i].Glow + ", " + Mesh.VertexAttributes[i].Move;
                        break;
                }

                if (string.IsNullOrEmpty(msg)) continue;
                labels.Add(TextLabel.World(msg,
                    worldPosition: VisibleMesh.VertexPositions[i],
                    color: new Vector4(1f, 1f, 1f, 1f),
                    background: overlays,
                    alignment: new Vector2(0f, 0f),
                    pixelOffset: new Vector2(2, -2)));
            }
        }

        // ------------------------------------------------ Selection / cache

        public void SelectElement(int element, bool highlight = false)
        {
            if (EditingMode == MeshEditingMode.FaceAttributes)
                _currentElement = (Mesh.Polys.Count > element) ? element : -1;
            else if (EditingMode != MeshEditingMode.Sphere)
                _currentElement = (Mesh.VertexPositions.Count > element) ? element : -1;

            _highlightFace = highlight;

            if (EditingMode != MeshEditingMode.None)
                Invalidate();
        }

        /// <summary>Drop the V2 cached vertex buffer for the visible mesh, so
        /// vertex edits applied in-place (positions, colours, attributes)
        /// surface on the next paint.</summary>
        public void InvalidateMeshCache()
        {
            if (_mesh != null) PreviewDevice.Renderer.InvalidateMesh(_mesh);
            if (_previewMesh != null) PreviewDevice.Renderer.InvalidateMesh(_previewMesh);
        }

        public void InitializeVertexBuffer()
        {
            // Legacy API: used to size a face vertex buffer; the V2 renderer
            // builds its own VB lazily, so we just refresh the cache.
            InvalidateMeshCache();
        }

        // ------------------------------------------------ Mouse / picking

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Focus();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _lastMousePosition = e.Location;

            if (DrawExtraInfo && _gizmo != null && EditingMode == MeshEditingMode.Sphere)
            {
                var result = _gizmo.DoPicking(Ray.GetPickRay(Camera, ClientSize, e.X, e.Y));
                if (result != null)
                {
                    _gizmo.ActivateGizmo(result);
                    Invalidate();
                    return;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                // Base ItemPreviewPanel already handled camera nav.
                _lastMousePosition = e.Location;
                return;
            }

            if (EditingMode != MeshEditingMode.None &&
                EditingMode != MeshEditingMode.VertexRemap &&
                EditingMode != MeshEditingMode.Sphere &&
                e.Button == MouseButtons.Left)
            {
                TryPickElement(e.X, e.Y, continuous: true);
            }
            else if (EditingMode == MeshEditingMode.Sphere && _gizmo != null)
            {
                var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);
                if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(ray)))
                    Invalidate();
                if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ray))
                {
                    Invalidate();
                    if (!_actionStarted)
                    {
                        _tool.UndoManager.PushMeshChanged(this);
                        _actionStarted = true;
                    }
                }
            }

            _lastMousePosition = e.Location;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != MouseButtons.Left) return;

            CurrentElement = -1;

            if (EditingMode == MeshEditingMode.Sphere && _gizmo != null && _gizmo.MouseUp())
                Invalidate();

            if (EditingMode != MeshEditingMode.None)
                TryPickElement(e.X, e.Y);

            _actionStarted = false;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.Button == MouseButtons.Right)
                ResetCamera();
        }

        private void TryPickElement(float x, float y, bool continuous = false)
        {
            if (_mesh == null) return;

            var ray = Ray.GetPickRay(Camera, ClientSize, x, y);
            float distance = float.MaxValue;
            int candidate = -1;

            if (EditingMode == MeshEditingMode.VertexRemap ||
                EditingMode == MeshEditingMode.VertexEffects ||
                EditingMode == MeshEditingMode.VertexWeights ||
                EditingMode == MeshEditingMode.VertexColorsAndNormals)
            {
                var radius = VertexSphereRadius / 2.0f;
                for (int i = 0; i < _mesh.VertexPositions.Count; i++)
                {
                    var vertex = VisibleMesh.VertexPositions[i];
                    var sphere = new BoundingSphere(vertex, radius);
                    if (Collision.RayIntersectsSphere(ray, sphere, out float newDistance))
                    {
                        if (newDistance <= distance || candidate == -1)
                        {
                            distance = newDistance;
                            candidate = i;
                        }
                    }

                    if (candidate != -1)
                    {
                        if (continuous && candidate != CurrentElement)
                        {
                            CurrentElement = candidate;
                            return;
                        }
                        else if (!continuous && !_clickchain.Contains(candidate))
                        {
                            CurrentElement = candidate;

                            // Reset clickchain on a new coordinate.
                            if (_lastElementPos.Count != 1 || _lastElementPos[0] != _mesh.VertexPositions[candidate])
                            {
                                _lastElementPos = new List<Vector3>() { _mesh.VertexPositions[candidate] };
                                _clickchain.Clear();
                            }
                            _clickchain.Add(candidate);
                            return;
                        }
                    }
                }

                if (!continuous)
                {
                    if (_clickchain.Count > 0)
                    {
                        _clickchain.Clear();
                        TryPickElement(x, y);
                    }
                    else
                        CurrentElement = -1;
                }
            }
            else
            {
                // Face picking
                for (int i = 0; i < _mesh.Polys.Count; i++)
                {
                    var poly = _mesh.Polys[i];

                    for (int j = 0; j < (poly.Shape == WadPolygonShape.Quad ? 2 : 1); j++)
                    {
                        var v0 = _mesh.VertexPositions[(j == 0 ? poly.Index0 : poly.Index2)];
                        var v1 = _mesh.VertexPositions[(j == 0 ? poly.Index1 : poly.Index3)];
                        var v2 = _mesh.VertexPositions[(j == 0 ? poly.Index2 : poly.Index0)];

                        if (Collision.RayIntersectsTriangle(ray, v0, v1, v2, poly.Texture.DoubleSided, out float newDistance))
                        {
                            if (newDistance <= distance || candidate == -1)
                            {
                                distance = newDistance;
                                candidate = i;
                            }
                        }
                    }
                }

                if (candidate != -1)
                {
                    if (continuous && candidate != CurrentElement)
                        CurrentElement = candidate;
                    else
                    {
                        CurrentElement = candidate;
                        _clickchain.Clear();
                    }
                }
            }
        }

        public new void ResetCamera()
        {
            var center = Vector3.Zero;
            var radius = 256.0f;
            if (Mesh != null)
            {
                var bs = Mesh.CalculateBoundingSphere();
                center = bs.Center;
                radius = bs.Radius * 1.15f;
            }
            Camera = new ArcBallCamera(center, 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2,
                radius * 3, 50, 1000000, (float)Math.PI / 4.0f);
            Invalidate();
        }

        // ------------------------------------------------ Vertex effects timer

        private void TransformVertices()
        {
            if (_mesh == null || _previewMesh == null || _mesh.VertexPositions.Count == 0 || !_mesh.HasAttributes)
                return;

            for (int i = 0; i < _mesh.VertexPositions.Count; i++)
            {
                var v = _mesh.VertexPositions[i];
                var a = _mesh.VertexAttributes[i];

                var hash = MathC.GetVector3Hash(v).GetHashCode();
                var wibble = (float)Math.Sin((((_frameCount + hash) % 64) / 64.0f) * (Math.PI * 2));

                var newPos = v;
                var newCol = _mesh.HasColors ? _mesh.VertexColors[i] : Vector3.One;

                if (a.Glow > 0.0f)
                {
                    float intensity = a.Glow / 63.0f * (float)MathC.Lerp(-0.5f, 1.0f, wibble * 0.5f + 0.5f);
                    newCol = Vector3.Min(newCol + new Vector3(intensity, intensity, intensity), new Vector3(2));
                }

                if (a.Move > 0.0f)
                    newPos.Y += wibble * a.Move / 63.0f * 128.0f;

                _previewMesh.VertexPositions[i] = newPos;

                if (!_previewMesh.HasColors)
                    _previewMesh.VertexColors.Add(newCol);
                else
                    _previewMesh.VertexColors[i] = newCol;
            }
        }

        public void ColorizeVertexWeights()
        {
            if (_mesh == null || _mesh.VertexPositions.Count == 0 || !_mesh.HasWeights)
                return;

            if (_previewMesh == null)
                _previewMesh = _mesh.Clone();

            for (int i = 0; i < _mesh.VertexPositions.Count; i++)
            {
                var color = Vector4.UnitW;
                for (int w = 0; w < 4; w++)
                {
                    int boneIndex = _mesh.VertexWeights[i].Index[w];
                    float weight = _mesh.VertexWeights[i].Weight[w];

                    if (boneIndex >= 0 && boneIndex < _maxBones && weight > 0)
                    {
                        var boneColor = _boneColors[boneIndex];
                        color += boneColor * weight;
                    }
                }

                if (!_previewMesh.HasColors)
                    _previewMesh.VertexColors.Add(color.To3());
                else
                    _previewMesh.VertexColors[i] = color.To3();
            }

            InvalidateMeshCache();
        }

        private void GenerateBoneColors()
        {
            for (int i = 0; i < _maxBones; i++)
                _boneColors[i] = MathC.GetRandomColorByIndex(i, _maxBones);
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            TransformVertices();
            InvalidateMeshCache();
            Invalidate();
            _frameCount++;
        }

        public void StartPreview()
        {
            _frameCount = 0;
            _previewMesh = _mesh.Clone();
            _previewTimer.Start();
        }

        public void StopPreview()
        {
            _previewTimer.Stop();
            _previewMesh = null;
            InvalidateMeshCache();
            Invalidate();
        }
    }
}
