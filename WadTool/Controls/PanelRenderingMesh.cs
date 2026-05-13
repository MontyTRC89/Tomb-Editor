using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
// Disambiguate BlendMode (the rendering one is what we want here; TombLib.Utils.BlendMode
// is the TR engine's texture blend mode).
using BlendMode = TombLib.Rendering.BlendMode;

namespace WadTool.Controls
{
    public class PanelRenderingMesh : RenderingPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; } = new ArcBallCamera(new Vector3(0.0f, 0.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 512.0f, 100, 1000000, (float)Math.PI / 4.0f);

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
                InitializeVertexBuffer();

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
            get {  return _editingMode; }
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
                        if (_currentElement == value) return;  // Face mode does not need continuous editing of same element
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

                _wadRenderer.Dispose();
                _wadRenderer = DeviceManager.DefaultDeviceManager.CreateWadRenderer(false, value, 4096, 2048, false);
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
            // A helper function to define vertex pick sphere radius
            get
            {
                var distances = new List<float>();

                if (Mesh == null || Mesh.VertexPositions.Count == 0)
                    return 0;

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

        // General state
        private WadToolClass _tool;

        // Interaction state
        private bool _actionStarted;
        private bool _highlightFace;
        private Point _lastMousePosition;
        private List<Vector3> _lastElementPos = new List<Vector3>();
        private List<int> _clickchain = new List<int>();

        // Unified path
        private RenderingDrawingLines _linesBatch;
        private readonly List<SolidLineVertex> _wireLines = new List<SolidLineVertex>();
        private readonly List<SolidLineVertex> _filledTriangles = new List<SolidLineVertex>();
        private readonly List<SolidLineVertex> _depthReadTriangles = new List<SolidLineVertex>();
        private readonly Dictionary<TombLib.Graphics.ObjectMesh, RenderingDrawingMesh> _meshCache = new Dictionary<TombLib.Graphics.ObjectMesh, RenderingDrawingMesh>();

        // Raw D3D11 device. Used to reconstruct the WadRenderer when settings change.
        private SharpDX.Direct3D11.Device _device;
        private GizmoMeshEditor _gizmo;
        private float _normalLength = 1.0f;
        private WadRenderer _wadRenderer;
        private WadStatic _dummyStatic = new WadStatic(new WadStaticId(0));

        // Rendering state
        private RenderingTextureAllocator _fontTexture;
        private RenderingFont _fontDefault;

        // Vertex effect preview
        private readonly Timer _previewTimer = new Timer { Interval = 33 };
        private int _frameCount;

        // Vertex weight preview
        private const int _maxBones = 32;
        private Vector4[] _boneColors = new Vector4[_maxBones];

        // Constants
        private readonly List<int> _oldLaraHairIndices = new List<int>() { 37, 38, 39, 40 };
        private readonly List<int> _youngLaraHairIndices = new List<int>() { 68, 69, 70, 71, 76, 77, 78, 79 };

        protected override Vector4 ClearColor => _tool.Configuration.RenderingItem_BackgroundColor;

        public PanelRenderingMesh()
        {
            _previewTimer.Tick += new EventHandler(PreviewTimer_Tick);
            GenerateBoneColors();
        }

        public void InitializeRendering(WadToolClass tool, DeviceManager deviceManager)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            base.InitializeRendering(deviceManager.Device, tool.Configuration.RenderingItem_Antialias);
            _tool = tool;

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
                _device = deviceManager.D3D11Device;
                _wadRenderer = deviceManager.CreateWadRenderer(false, false, 4096, 2048, false);
                _gizmo = new GizmoMeshEditor(_tool.Configuration, deviceManager.Device, this);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _previewTimer.Stop();
                _previewTimer.Tick -= new EventHandler(PreviewTimer_Tick);

                _gizmo?.Dispose();
                _linesBatch?.Dispose();
                foreach (var m in _meshCache.Values)
                    m.Dispose();
                _meshCache.Clear();
                _wadRenderer?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnDraw()
        {
            if (VisibleMesh == null)
                return;

            ((TombLib.Rendering.DirectX11.Dx11RenderingSwapChain)SwapChain).BindForce();
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            var viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            using var stateBuffer = Device.CreateStateBuffer();
            stateBuffer.Set(new RenderingState { TransformMatrix = viewProjection });

            _wireLines.Clear();
            _filledTriangles.Clear();
            _depthReadTriangles.Clear();

            if (DrawGrid)
                WireGeometry.AppendGrid(_wireLines, sizePerSide: 8, divisions: 4, color: Vector4.One);

            _dummyStatic.Mesh = VisibleMesh;
            _dummyStatic.Version = DataVersion.GetNext();
            var staticModel = _wadRenderer.GetStatic(_dummyStatic);

            var world  = Matrix4x4.Identity;
            var textToDraw  = new List<Text>();
            var linesToDraw = _wireLines; // alias — normals append directly here

            // At first, draw either vertex spheres (if mode is set to vertex remap)
            // or individual colored shininess faces (if mode is set to shininess editing).

            if (EditingMode == MeshEditingMode.VertexRemap ||
                EditingMode == MeshEditingMode.VertexEffects ||
                EditingMode == MeshEditingMode.VertexWeights ||
                EditingMode == MeshEditingMode.VertexColorsAndNormals)
            {
                // Draw model first in vertex modes (so vertex spheres overlay it).
                DrawModel(staticModel, stateBuffer, world);

                var safeIndex = SafeVertexRemapLimit;

                for (int i = 0; i < _mesh.VertexPositions.Count; i++)
                {
                    var selected = (i == _currentElement);

                    // Skip duplicates from the click chain.
                    if (!selected && _currentElement != -1 && _mesh.VertexPositions[i] == _mesh.VertexPositions[_currentElement])
                        continue;

                    Vector4 color;
                    if (selected)
                        color = new Vector4(1, 0, 0, 0.5f);
                    else switch (EditingMode)
                    {
                        case MeshEditingMode.VertexRemap:
                            color = (i <= safeIndex)
                                ? new Vector4(0, 0.3f, 1, 0.8f)
                                : new Vector4(0.8f, 0.8f, 0, 0.8f);
                            break;
                        case MeshEditingMode.VertexEffects:
                            if (_mesh.HasAttributes)
                            {
                                var glowPower = _mesh.VertexAttributes[i].Glow == 0 ? 0 : (_mesh.VertexAttributes[i].Glow + 64.0f) / 128.0f;
                                var movePower = _mesh.VertexAttributes[i].Move == 0 ? 0 : (_mesh.VertexAttributes[i].Move + 64.0f) / 128.0f;
                                color = new Vector4(0, glowPower, movePower, 0.7f);
                            }
                            else color = new Vector4(0, 0, 0, 0.8f);
                            break;
                        case MeshEditingMode.VertexWeights:
                            color = _mesh.HasWeights ? Vector4.One : new Vector4(0, 0, 0, 1);
                            break;
                        case MeshEditingMode.VertexColorsAndNormals:
                            color = new Vector4(1, 1, 1, 0.6f);
                            break;
                        default:
                            color = Vector4.One;
                            break;
                    }

                    // Vertex sphere sized by VertexSphereRadius (legacy used a
                    // pre-built _littleSphere primitive of that diameter). Built CPU-side
                    // per frame so VertexSphereRadius can change without re-init.
                    var sphereWorld = Matrix4x4.CreateScale(VertexSphereRadius * 0.5f) * Matrix4x4.CreateTranslation(VisibleMesh.VertexPositions[i]);
                    WireGeometry.AppendSolidSphere(_filledTriangles, sphereWorld, color, latSegments: 8, longSegments: 12);

                    if (DrawExtraInfo || selected)
                    {
                        var posMatrix = Matrix4x4.CreateTranslation(VisibleMesh.VertexPositions[i]) * viewProjection;
                        if (posMatrix.TransformPerspectively(new Vector3()).Z <= 1.0f)
                        {
                            var pos = posMatrix.TransformPerspectively(new Vector3()).To2();
                            var message = string.Empty;

                            switch (EditingMode)
                            {
                                case MeshEditingMode.VertexRemap:
                                    {
                                        // Filter out labels which sit on the same coordinate and show ellipsis instead
                                        var existingText = textToDraw.Where(t => t.Pos == pos).ToList();
                                        if (existingText.Count > 0)
                                        {
                                            if (existingText[0].String != _currentElement.ToString())
                                                existingText[0].String = "...";
                                            continue;
                                        }

                                        message = i.ToString();
                                    }
                                    break;

                                case MeshEditingMode.VertexWeights:
                                    {
                                        if (_mesh.HasWeights)
                                            message = _mesh.VertexWeights[i].ToString();
                                    }
                                    break;

                                case MeshEditingMode.VertexEffects:
                                    {
                                        if (_mesh.HasAttributes)
                                            message = _mesh.VertexAttributes[i].Glow + ", " + _mesh.VertexAttributes[i].Move;
                                    }
                                    break;

                                case MeshEditingMode.VertexColorsAndNormals:
                                    {
                                        if (_mesh.HasNormals)
                                        {
                                            var nColor = selected ? new Vector4(1, 0, 0, 1) : Vector4.One;
                                            var p = Vector3.Transform(_mesh.VertexPositions[i], world);
                                            var n = Vector3.TransformNormal(_mesh.VertexNormals[i] /
                                                _mesh.VertexNormals[i].Length(), world);
                                            linesToDraw.Add(new SolidLineVertex { Position = p,                       Color = nColor });
                                            linesToDraw.Add(new SolidLineVertex { Position = p + n * _normalLength,    Color = nColor });
                                        }
                                    }
                                    break;
                            }


                            if (!string.IsNullOrEmpty(message))
                                textToDraw.Add(new Text
                                {
                                    Font = _fontDefault,
                                    TextAlignment = new Vector2(0.0f, 0.0f),
                                    PixelPos = new VectorInt2(2, -2),
                                    Pos = pos,
                                    Overlay = _tool.Configuration.Rendering3D_DrawFontOverlays,
                                    String = message
                                });
                        }
                    }
                }
            }
            
            if (EditingMode == MeshEditingMode.FaceAttributes ||
                EditingMode == MeshEditingMode.VertexWeights)
            {
                if ((DrawExtraInfo || _highlightFace || EditingMode == MeshEditingMode.VertexWeights) && _mesh.Polys.Count > 0)
                {
                    // Build face triangles directly into the unified solid-triangle batch.
                    // The legacy code used a fixed-size SolidVertex[] sized to a static
                    // _faceVertexBuffer.ElementCount; we just append per poly.
                    for (int i = 0; i < _mesh.Polys.Count; i++)
                    {
                        var poly = _mesh.Polys[i];
                        var strength = poly.ShineStrength == 0 ? 0 : (poly.ShineStrength + 32.0f) / 95.0f;
                        int triangleCount = poly.Shape == WadPolygonShape.Quad ? 2 : 1;
                        for (int j = 0; j < triangleCount; j++)
                        {
                            for (int v = 0; v < 3; v++)
                            {
                                int vn = j * 3 + v;
                                int index = 0;
                                Vector3 pos = Vector3.Zero;
                                Vector4 color = Vector4.Zero;

                                if (DrawExtraInfo || i == _currentElement || EditingMode == MeshEditingMode.VertexWeights)
                                {
                                    switch (vn)
                                    {
                                        case 0: index = poly.Index0; break;
                                        case 1: index = poly.Index1; break;
                                        case 2: index = poly.Index2; break;
                                        case 3: index = poly.Index2; break;
                                        case 4: index = poly.Index3; break;
                                        case 5: index = poly.Index0; break;
                                    }
                                    pos = _mesh.VertexPositions[index];

                                    if (EditingMode == MeshEditingMode.FaceAttributes)
                                        color = DrawExtraInfo ? new Vector4(1, 1 - strength, 1 - strength, 1) : new Vector4(1, 0, 0, 1);
                                    else if (_previewMesh != null && _mesh.HasWeights)
                                        color = new Vector4(_previewMesh.VertexColors[index], 1.0f);
                                }
                                _filledTriangles.Add(new SolidLineVertex { Position = pos, Color = color });
                            }
                        }
                    }
                }

                if (WireframeMode || !DrawExtraInfo)
                    DrawModel(staticModel, stateBuffer, world);
            }
            else if (EditingMode == MeshEditingMode.Sphere)
            {
                DrawModel(staticModel, stateBuffer, world);

                // Big translucent sphere overlay around mesh bounding sphere. Uses
                // DepthRead so it's visible behind opaque geometry.
                var sphereWorld = Matrix4x4.CreateScale(_mesh.BoundingSphere.Radius) * Matrix4x4.CreateTranslation(_mesh.BoundingSphere.Center);
                WireGeometry.AppendSolidSphere(_depthReadTriangles, sphereWorld, new Vector4(Vector3.One, 0.5f), latSegments: 12, longSegments: 16);
            }
            else if (EditingMode == MeshEditingMode.None)
            {
                DrawModel(staticModel, stateBuffer, world);
            }

            // Submit the three line/triangle batches. Order: solid filled (filledTriangles)
            // first → wireframe lines (wireLines) second → DepthRead translucent
            // (depthReadTriangles) last so it overlays correctly.
            if (_filledTriangles.Count > 0)
            {
                _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_filledTriangles));
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = stateBuffer,
                    Topology = RenderingDrawingLines.Topology.TriangleList,
                    Blend = BlendMode.NonPremultipliedAlpha,
                });
            }
            if (_wireLines.Count > 0)
            {
                _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_wireLines));
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = stateBuffer,
                });
            }
            if (_depthReadTriangles.Count > 0)
            {
                _linesBatch.SetVertices(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_depthReadTriangles));
                _linesBatch.Render(new RenderingDrawingLines.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = stateBuffer,
                    Topology = RenderingDrawingLines.Topology.TriangleList,
                    Blend = BlendMode.NonPremultipliedAlpha,
                    Depth = DepthMode.DepthRead,
                });
            }

            if (textToDraw.Count > 0)
                SwapChain.RenderText(textToDraw);

            if (EditingMode == MeshEditingMode.Sphere && DrawExtraInfo)
            {
                ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();
                SwapChain.ClearDepth();
                _gizmo.Draw(SwapChain, stateBuffer, viewProjection);
            }
        }

        private void GenerateBoneColors()
        {
            for (int i = 0; i < _maxBones; i++)
                _boneColors[i] = MathC.GetRandomColorByIndex(i, _maxBones);
        }

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

        // Draws the whole mesh through the unified RenderingDrawingMesh path.
        // KNOWN LIMITATION: WireframeMode is no longer rendered as wireframe-rasterized
        // triangles (the new path doesn't support per-mesh wireframe). When toggled
        // on, the mesh still renders solid; users can rely on the editor overlays
        // (face highlights, vertex spheres) to inspect topology. Adding a wireframe
        // mode to RenderingDrawingMesh would require either rasterizer state on
        // RenderArgs or a new submesh-wireframe path.
        private void DrawModel(StaticModel mesh, RenderingStateBuffer stateBuffer, Matrix4x4 world)
        {
            if (mesh.Meshes.Count == 0)
                return;
            if (!WireframeMode && EditingMode == MeshEditingMode.VertexWeights)
                return;

            var showColors = EditingMode == MeshEditingMode.VertexColorsAndNormals || (EditingMode == MeshEditingMode.VertexEffects && _previewTimer.Enabled);
            var tint = WireframeMode ? new Vector4(1.0f - ClearColor.To3().GetLuma()) : Vector4.One;
            var coloredVertices = _tool.DestinationWad.GameVersion == TRVersion.Game.TombEngine;

            foreach (var legacyMesh in mesh.Meshes)
            {
                if (legacyMesh.Vertices.Count == 0)
                    continue;
                var drawMesh = GetOrCreateDrawingMesh(legacyMesh);
                drawMesh.Render(new RenderingDrawingMesh.RenderArgs
                {
                    RenderTarget = SwapChain,
                    StateBuffer = stateBuffer,
                    Atlas = _wadRenderer.Texture,
                    World = world,
                    Tint = tint,
                    StaticLighting = showColors,
                    ColoredVertices = coloredVertices,
                    AlphaTest = !WireframeMode && AlphaTest,
                    BilinearFilter = _bilinear,
                });
            }
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

        protected override void OnMouseEnter(EventArgs e)
        {
            // Make this control able to receive scroll and key board events...
            base.OnMouseEnter(e);
            Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Camera.Zoom(-e.Delta * _tool.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom / 4);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _lastMousePosition = e.Location;

            if (DrawExtraInfo)
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
                // Warp cursor
                var delta = WarpMouseCursor(e.Location, _lastMousePosition);

                if (e.Button == MouseButtons.Right)
                {
                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                        Camera.Zoom(-delta.Y * _tool.Configuration.RenderingItem_NavigationSpeedMouseZoom);
                    else if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                        Camera.Rotate(delta.X * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate,
                                     -delta.Y * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate);
                }
                if ((e.Button == MouseButtons.Right && (ModifierKeys & Keys.Shift) == Keys.Shift) ||
                     e.Button == MouseButtons.Middle)
                    Camera.MoveCameraPlane(new Vector3(delta.X, delta.Y, 0) * _tool.Configuration.RenderingItem_NavigationSpeedMouseTranslate);

                Invalidate();
            }
            else if (EditingMode != MeshEditingMode.None &&
                     EditingMode != MeshEditingMode.VertexRemap && 
                     EditingMode != MeshEditingMode.Sphere &&
                     e.Button == MouseButtons.Left)
            {
                TryPickElement(e.X, e.Y, true);
            }
            else if (EditingMode == MeshEditingMode.Sphere)
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

            if (e.Button != MouseButtons.Left)
                return;

            CurrentElement = -1;

            if (EditingMode == MeshEditingMode.Sphere && _gizmo.MouseUp())
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
            if (_mesh == null)
                return;

            var ray = Ray.GetPickRay(Camera, ClientSize, x, y);

            float distance = float.MaxValue;
            int candidate = -1;

            if (EditingMode == MeshEditingMode.VertexRemap ||
                EditingMode == MeshEditingMode.VertexEffects ||
                EditingMode == MeshEditingMode.VertexWeights ||
                EditingMode == MeshEditingMode.VertexColorsAndNormals)
            {
                // Try to pick a vertex sphere

                var radius = VertexSphereRadius / 2.0f;
                for (int i = 0; i < _mesh.VertexPositions.Count; i++)
                {
                    var vertex = VisibleMesh.VertexPositions[i];
                    var sphere = new BoundingSphere(vertex, radius);
                    float newDistance;

                    if (Collision.RayIntersectsSphere(ray, sphere, out newDistance))
                    {
                        if (newDistance <= distance || candidate == -1)
                        {
                            distance = newDistance;
                            candidate = i;
                        }
                    }

                    // Clickchain is only used for vertex picking since model may have 2 vertices at same coordinate
                    // but not 2 faces with same coordinates (it means Z-fighting will appear and it is a model issue).

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

                            // Reset clickchain in case other coordinate is picked
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
                        TryPickElement(x, y); // All similar vertices clicked, restart picking
                    }
                    else
                        CurrentElement = -1;
                }
            }
            else
            {
                // Try to pick a face

                for (int i = 0; i < _mesh.Polys.Count; i++)
                {
                    var poly = _mesh.Polys[i];
                    
                    for (int j = 0; j < (poly.Shape == WadPolygonShape.Quad ? 2 : 1); j++)
                    {
                        var v = new Vector3[3]
                        {
                            _mesh.VertexPositions[(j == 0 ? poly.Index0 : poly.Index2)],
                            _mesh.VertexPositions[(j == 0 ? poly.Index1 : poly.Index3)],
                            _mesh.VertexPositions[(j == 0 ? poly.Index2 : poly.Index0)]
                        };

                        float newDistance;
                        if (Collision.RayIntersectsTriangle(ray, v[0], v[1], v[2], poly.Texture.DoubleSided, out newDistance))
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
                    {
                        CurrentElement = candidate;
                    }
                    else
                    {
                        CurrentElement = candidate;
                        _clickchain.Clear(); // Clickchain is not used for face picking, so clear it just in case.
                    }
                }
            }
        }

        // Legacy init for the _faceVertexBuffer and _littleSphere primitives is no
        // longer needed: face triangles and vertex spheres are built CPU-side per
        // frame into the _filledTriangles batch (see OnDraw). We still keep this
        // method (called when the mesh changes) to update _normalLength which is
        // used by the normals-line drawing.
        public void InitializeVertexBuffer()
        {
            _normalLength = VertexSphereRadius * 3.0f;
        }

        public void ResetCamera()
        {
            // Smart reset camera which fits an object into window. Later reuse for TE item preview!

            var center = Vector3.Zero;
            var radius = 256.0f;

            if (Mesh != null)
            {
                var bs = Mesh.CalculateBoundingSphere();
                center = bs.Center;
                radius = bs.Radius * 1.15f; // Zoom out a bit
            }

            Camera = new ArcBallCamera(center, 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, radius * 3, 50, 1000000, (float)Math.PI / 4.0f);
            Invalidate();
        }

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
                    newPos.Y += wibble * a.Move / 63.0f * 128.0f; // 128 units offset to top and bottom (256 total)

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
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            TransformVertices();
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
            Invalidate();
        }
    }
}
