using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
                InitializeVertexBuffer();
                ResetCamera();
                CurrentElement = -1;
            }
        }
        private WadMesh _mesh;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MeshEditingMode EditingMode
        {
            get {  return _editingMode; }
            set
            {
                if (_editingMode == value) 
                    return;

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
                 
                // We arent checking if current element is already set because user may
                // reselect same vertex/face with other incoming params changed (eg shine strength).

                switch (EditingMode)
                {
                    case MeshEditingMode.FaceAttributes:
                        _currentElement = (Mesh.Polys.Count > value) ? value : -1;
                        break;

                    case MeshEditingMode.VertexEffects:
                    case MeshEditingMode.VertexRemap:
                    case MeshEditingMode.VertexColorsAndNormals:
                        _currentElement = (Mesh.VertexPositions.Count > value) ? value : -1;
                        break;

                    default:
                        _currentElement = -1;
                        break;
                }

                _tool.MeshEditorElementChanged(_currentElement);

                if (EditingMode != MeshEditingMode.None)
                    Invalidate();
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

                foreach (var v in Mesh.VertexPositions)
                    foreach (var v2 in Mesh.VertexPositions)
                    {
                        if (v == v2) continue;
                        distances.Add(Vector3.Distance(v, v2));
                    }

                var medianDistance = distances.Sum() / distances.Count;
                return ((medianDistance * 0.7f) + (Mesh.BoundingSphere.Radius * 0.3f)) / 32.0f;
            }
        }

        // General state
        private WadToolClass _tool;

        // Interaction state
        private float _lastX;
        private float _lastY;
        private List<Vector3> _lastElementPos = new List<Vector3>();
        private List<int> _clickchain = new List<int>();

        // Legacy rendering state
        private GraphicsDevice _device;
        private RasterizerState _rasterizerWireframe;
        private VertexInputLayout _layout;
        private GeometricPrimitive _littleSphere;
        private Buffer<SolidVertex> _faceVertexBuffer;
        private WadRenderer _wadRenderer;

        // Rendering state
        private RenderingTextureAllocator _fontTexture;
        private RenderingFont _fontDefault;

        // Constants
        private readonly List<int> _oldLaraHairIndices = new List<int>() { 37, 38, 39, 40 };
        private readonly List<int> _youngLaraHairIndices = new List<int>() { 68, 69, 70, 71, 76, 77, 78, 79 };

        protected override Vector4 ClearColor => _tool.Configuration.RenderingItem_BackgroundColor;

        public void InitializeRendering(WadToolClass tool, DeviceManager deviceManager)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            base.InitializeRendering(deviceManager.Device, tool.Configuration.RenderingItem_Antialias);
            _tool = tool;

            // Legacy rendering
            {
                _device = deviceManager.___LegacyDevice;
                _wadRenderer = new WadRenderer(deviceManager.___LegacyDevice);

                _fontTexture = deviceManager.Device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(512, 512, 2) });
                _fontDefault = deviceManager.Device.CreateFont(new RenderingFont.Description
                {
                    FontName = _tool.Configuration.Rendering3D_FontName,
                    FontSize = _tool.Configuration.Rendering3D_FontSize,
                    FontIsBold = _tool.Configuration.Rendering3D_FontIsBold,
                    TextureAllocator = _fontTexture
                });

                _rasterizerWireframe = RasterizerState.New(_device, new SharpDX.Direct3D11.RasterizerStateDescription
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
                });

                _littleSphere = GeometricPrimitive.Sphere.New(_device, 2, 4);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rasterizerWireframe?.Dispose();
                _littleSphere?.Dispose();
                _wadRenderer?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnDraw()
        {
            // To make sure things are in a defined state for legacy rendering...
            ((TombLib.Rendering.DirectX11.Dx11RenderingSwapChain)SwapChain).BindForce();
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            var viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            var solidEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Solid"];

            _device.SetDepthStencilState(_device.DepthStencilStates.Default);

            _wadRenderer.Dispose();

            if (Mesh != null)
            {
                var mesh   = _wadRenderer.GetStatic(new WadStatic(new WadStaticId(0)) { Mesh = Mesh });
                var world  = Matrix4x4.Identity;

                var textToDraw  = new List<Text>();
                var linesToDraw = new List<SolidVertex>();

                // At first, draw either vertex spheres (if mode is set to vertex remap)
                // or individual colored shininess faces (if mode is set to shininess editing).

                if (EditingMode == MeshEditingMode.VertexRemap ||
                    EditingMode == MeshEditingMode.VertexEffects ||
                    EditingMode == MeshEditingMode.VertexColorsAndNormals)
                {
                    // Draw model first in vertex mode
                    DrawModel(mesh, world * viewProjection);

                    _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                    _device.SetBlendState(_device.BlendStates.AlphaBlend);
                    _device.SetDepthStencilState(_device.DepthStencilStates.DepthRead);

                    _device.SetVertexBuffer(_littleSphere.VertexBuffer);
                    _device.SetVertexInputLayout(_littleSphere.InputLayout);
                    _device.SetIndexBuffer(_littleSphere.IndexBuffer, _littleSphere.IsIndex32Bits);

                    var safeIndex    = SafeVertexRemapLimit;
                    var normalLength = VertexSphereRadius * 3.0f;

                    for (int i = 0; i < _mesh.VertexPositions.Count; i++)
                    {
                        var selected = (i == _currentElement);

                        // Don't draw vertices from clickchain
                        if (!selected && _clickchain.Contains(i) && _clickchain.Contains(_currentElement))
                            continue;

                        var posMatrix = Matrix4x4.Identity * Matrix4x4.CreateTranslation(_mesh.VertexPositions[i]) * viewProjection;
                        solidEffect.Parameters["ModelViewProjection"].SetValue(posMatrix.ToSharpDX());

                        if (selected)
                        {
                            // Highlight selection
                            solidEffect.Parameters["Color"].SetValue(new Vector4(1, 0, 0, 0.5f));
                        }
                        else
                        {
                            switch (EditingMode)
                            {
                                case MeshEditingMode.VertexRemap:

                                    // Highlight safe remap indices
                                    if (i <= safeIndex)  
                                        solidEffect.Parameters["Color"].SetValue(new Vector4(0, 0.3f, 1, 0.8f));
                                    else
                                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.8f, 0.8f, 0, 0.8f));
                                    break;

                                case MeshEditingMode.VertexEffects:

                                    // Mix glow and move attributes for now as green and blue color components for vertex spheres.
                                    // TODO: If in future there will be more vertex attributes, another way of indication must be invented.
                                    if (_mesh.HasAttributes)
                                        solidEffect.Parameters["Color"].SetValue(new Vector4(0, _mesh.VertexAttributes[i].Glow / 63.0f, _mesh.VertexAttributes[i].Move / 63.0f, 0.5f));
                                    else
                                        solidEffect.Parameters["Color"].SetValue(new Vector4(0, 0, 0, 0.6f));
                                    break;

                                case MeshEditingMode.VertexColorsAndNormals:

                                    // Simply draw normal color, since we don't need any extra indication for this mode
                                    solidEffect.Parameters["Color"].SetValue(new Vector4(1, 1, 1, 0.6f));
                                    break;
                            }
                        }

                        solidEffect.Techniques[0].Passes[0].Apply();
                        _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);

                        if (DrawExtraInfo || selected)
                        {
                            // Only draw texts which are actually visible
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
                                                var color = selected ? new Vector4(1, 0, 0, 1) : Vector4.One;

                                                var p = Vector3.Transform(_mesh.VertexPositions[i], world);
                                                var n = Vector3.TransformNormal(_mesh.VertexNormals[i] /
                                                    _mesh.VertexNormals[i].Length(), world);

                                                var v = new SolidVertex();
                                                v.Position = p;
                                                v.Color = color;
                                                linesToDraw.Add(v);

                                                v = new SolidVertex();
                                                v.Position = p + n * normalLength;
                                                v.Color = color;
                                                linesToDraw.Add(v);
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
                else if (EditingMode == MeshEditingMode.FaceAttributes)
                {
                    // Accumulate and draw extra face info (for now, only shininess values)

                    if (DrawExtraInfo)
                    {
                        _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                        _device.SetBlendState(_device.BlendStates.Opaque);

                        _device.SetVertexBuffer(_faceVertexBuffer);
                        _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _faceVertexBuffer));

                        // Create a vertex array
                        var vtxs = new SolidVertex[_faceVertexBuffer.ElementCount];
                        int vertexCount = 0;

                        for (int i = 0; i < _mesh.Polys.Count; i++)
                        {
                            var poly = _mesh.Polys[i];
                            var strength = _mesh.Polys[i].ShineStrength / 64.0f;
                            int vn = 0;

                            // Draw one triangle for triangular face or 2 triangles for quad face

                            for (int j = 0; j < (poly.Shape == WadPolygonShape.Quad ? 2 : 1); j++)
                                for (int v = 0; v < 3; v++)
                                {
                                    Vector3 pos = Vector3.Zero;

                                    switch (vn)
                                    {
                                        case 0: pos = _mesh.VertexPositions[_mesh.Polys[i].Index0]; break;
                                        case 1: pos = _mesh.VertexPositions[_mesh.Polys[i].Index1]; break;
                                        case 2: pos = _mesh.VertexPositions[_mesh.Polys[i].Index2]; break;

                                        case 3: pos = _mesh.VertexPositions[_mesh.Polys[i].Index2]; break;
                                        case 4: pos = _mesh.VertexPositions[_mesh.Polys[i].Index3]; break;
                                        case 5: pos = _mesh.VertexPositions[_mesh.Polys[i].Index0]; break;
                                    }

                                    vtxs[vertexCount] = new SolidVertex(pos) { Color = new Vector4(1, 1 - strength, 1 - strength, 1) };
                                    vn++;
                                    vertexCount++;
                                }
                        }

                        _faceVertexBuffer.SetData(vtxs);

                        solidEffect.Parameters["Color"].SetValue(Vector4.One);
                        solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                        solidEffect.Techniques[0].Passes[0].Apply();

                        _device.Draw(PrimitiveType.TriangleList, _faceVertexBuffer.ElementCount);
                    }

                    // Draw model last in face editing only if wireframe mode is set or extra mode is unset

                    if (WireframeMode || !DrawExtraInfo)
                        DrawModel(mesh, world * viewProjection);
                }

                if (textToDraw.Count > 0)
                {
                    _device.SetBlendState(_device.BlendStates.AlphaBlend);
                    SwapChain.RenderText(textToDraw);
                }

                if (linesToDraw.Count > 0)
                {
                    var bufferLines = SharpDX.Toolkit.Graphics.Buffer.New(_device, linesToDraw.ToArray(), BufferFlags.VertexBuffer, SharpDX.Direct3D11.ResourceUsage.Default);

                    _device.SetVertexBuffer(bufferLines);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, bufferLines));
                    _device.SetIndexBuffer(null, false);

                    solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.Draw(PrimitiveType.LineList, bufferLines.ElementCount);
                }
            }
        }

        private void DrawModel(StaticModel mesh, Matrix4x4 world)
        {
            // Next, draw whole textured mesh.
            // In case mode is set to shininess editing, only draw in wireframe mode to avoid Z-fighting.

            if (WireframeMode)
            {
                _device.SetRasterizerState(_rasterizerWireframe);
                _device.SetBlendState(_device.BlendStates.Opaque);
            }

            var effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];
            effect.Parameters["ModelViewProjection"].SetValue(world.ToSharpDX());
            effect.Parameters["Color"].SetValue(Vector4.One);
            effect.Parameters["StaticLighting"].SetValue(EditingMode == MeshEditingMode.VertexColorsAndNormals);
            effect.Parameters["ColoredVertices"].SetValue(EditingMode == MeshEditingMode.VertexColorsAndNormals);
            effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
            effect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);
            effect.Techniques[0].Passes[0].Apply();

            foreach (var mesh_ in mesh.Meshes)
            {
                _device.SetVertexBuffer(0, mesh_.VertexBuffer);
                _device.SetIndexBuffer(mesh_.IndexBuffer, true);
                _layout = VertexInputLayout.FromBuffer(0, mesh_.VertexBuffer);
                _device.SetVertexInputLayout(_layout);

                foreach (var submesh in mesh_.Submeshes)
                {
                    if (!WireframeMode)
                    {
                        if (submesh.Value.Material.AdditiveBlending)
                            _device.SetBlendState(_device.BlendStates.Additive);
                        else
                            _device.SetBlendState(_device.BlendStates.Opaque);

                        if (submesh.Value.Material.DoubleSided)
                            _device.SetRasterizerState(_device.RasterizerStates.CullNone);
                        else
                            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                    }

                    _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                }
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
            base.OnMouseDown(e);

            _lastX = e.X;
            _lastY = e.Y;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

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

            if (EditingMode != MeshEditingMode.None && e.Button == MouseButtons.Left)
                TryPickElement(e.X, e.Y);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button == MouseButtons.Right)
                ResetCamera();
        }

        private void TryPickElement(float x, float y)
        {
            if (_mesh == null)
                return;

            var matrix = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            var ray = Ray.GetPickRay(new Vector2(x, y), matrix, ClientSize.Width, ClientSize.Height);

            float distance = float.MaxValue;
            int candidate = -1;

            if (EditingMode == MeshEditingMode.VertexRemap ||
                EditingMode == MeshEditingMode.VertexEffects ||
                EditingMode == MeshEditingMode.VertexColorsAndNormals)
            {
                // Try to pick a vertex sphere

                for (int i = 0; i < _mesh.VertexPositions.Count; i++)
                {
                    var vertex = _mesh.VertexPositions[i];
                    var sphere = new BoundingSphere(vertex, VertexSphereRadius);
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

                    if (candidate != -1 && !_clickchain.Contains(candidate))
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

                if (_clickchain.Count > 0)
                {
                    _clickchain.Clear();
                    TryPickElement(x, y); // All similar vertices clicked, restart picking
                }
                else
                    CurrentElement = -1;
            }
            else
            {
                // Try to pick a face

                for (int i = 0; i < _mesh.Polys.Count; i++)
                {
                    var poly = _mesh.Polys[i];
                    float newDistance;
                    
                    for (int j = 0; j < (poly.Shape == WadPolygonShape.Quad ? 2 : 1); j++)
                    {
                        var v = new Vector3[3]
                        {
                            _mesh.VertexPositions[(j == 0 ? poly.Index0 : poly.Index2)],
                            _mesh.VertexPositions[(j == 0 ? poly.Index1 : poly.Index3)],
                            _mesh.VertexPositions[(j == 0 ? poly.Index2 : poly.Index0)]
                        };

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
                    CurrentElement = candidate;
                    _clickchain.Clear(); // Clickchain is not used for face picking, so clear it just in case.
                }
            }
        }

        public void InitializeVertexBuffer()
        {
            var vertexCount = 0;
            foreach (var poly in _mesh.Polys)
                if (poly.Shape == WadPolygonShape.Triangle) vertexCount += 3; else vertexCount += 6;
            _faceVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New<SolidVertex>(_device, vertexCount);

            _littleSphere = GeometricPrimitive.Sphere.New(_device, VertexSphereRadius, 4);
        }

        public void ResetCamera()
        {
            // Smart reset camera which fits an object into window. Later reuse for TE item preview!

            var center = Mesh != null ? Mesh.BoundingBox.Center : Vector3.Zero;
            var dims = Mesh != null ? new Vector2(Mesh.BoundingBox.Size.X, Mesh.BoundingBox.Size.Y) : Vector2.Zero;
            var screenSpace = new Vector2(Width, Height);
            var ratio = dims / screenSpace;
            var scale = 512.0f;

            // Fit mesh into window depending on ratio prevalence
            if (ratio.X > ratio.Y)
                scale = dims.X / screenSpace.X;
            else
                scale = dims.Y / screenSpace.Y;

            // Multiply distance by world units plus quarter-margin
            scale *= Level.WorldUnit;

            Camera = new ArcBallCamera(center, 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, scale, 50, 1000000, (float)Math.PI / 4.0f);
            Invalidate();
        }
    }
}
