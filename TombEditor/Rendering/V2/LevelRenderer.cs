using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.LevelData.SectorStructs;
using TombLib.Rendering;
using TombLib.RenderingV2.Backends.Dx11;
using TombLib.RenderingV2.Rhi;
using TombLib.Utils;

namespace TombEditor.Rendering.V2;

/// <summary>
/// V2 renderer for the editor's main 3D viewport (Panel3D). Owns a single
/// <see cref="Dx11Device"/> + swapchain bound to the host control's HWND.
///
/// <para>Current scope: solid-color room geometry. Each visible room is
/// uploaded once to an immutable vertex buffer (position + color in world
/// space), then drawn with the <c>RoomGeometry</c> shader. Sector textures,
/// highlights, blend modes, objects, sprites, gizmo etc. are still TODO.</para>
/// </summary>
public sealed class LevelRenderer : IDisposable
{
    private readonly Dx11Device     
        _device;
    private SwapchainHandle         _swap;
    private int                     _width;
    private int                     _height;

    // Room geometry pass resources.
    private PipelineHandle          _roomPipeline;
    private BufferHandle            _viewCb;
    private TextureAtlas?           _atlas;
    private Level?                  _atlasLevel;
    private readonly Dictionary<Room, RoomMesh> _roomMeshes = new();
    private readonly Frustum                    _frustum    = new();
    private readonly List<Room>                 _visibleRooms = new();
    // Mesh content depends on the editor mode (Geometry vs Texturing).
    // When the mode changes we invalidate every cached room so it gets
    // rebuilt with the right vertex strategy.
    private bool                                _meshesAreTexturing;
    // Moveables / statics / imported geometry pass.
    private ObjectRenderer?                     _objects;

    // Reusable per-frame scratch arrays. Keeping these as fields avoids
    // a fresh managed allocation on every Bindings / SetVertexBuffers /
    // BeginPass call — adds up to thousands of GC allocations per second
    // otherwise.
    private readonly BufferHandle[]         _scratchCbuf     = new BufferHandle[1];
    private readonly TextureHandle[]        _scratchTex      = new TextureHandle[1];
    private readonly SamplerHandle[]        _scratchSamp     = new SamplerHandle[1];
    private readonly VertexBufferBinding[]  _scratchVb       = new VertexBufferBinding[1];
    private readonly LoadOp[]               _scratchLoadOps  = { LoadOp.Clear };
    private readonly StoreOp[]              _scratchStoreOps = { StoreOp.Store };
    private readonly Vector4[]              _scratchClearCol = new Vector4[1];

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 256)]
    private struct ViewParams
    {
        public Matrix4x4 ViewProjection;   // 64B
        public float     GridLineWidth;    // 4B  -- legacy default 10.0
        public float     GridEnabled;      // 4B  -- 1=outline, 0=texturing mode
        public float     _pad1, _pad2;
    }

    // Packed vertex layout (24 B vs the 40 B float-only version):
    //   Position : float3              (12 B)
    //   Color    : R8G8B8A8_UNorm      ( 4 B)
    //   AtlasUv  : R16G16_UNorm        ( 4 B)
    //   GridUv   : R16G16_Float (half) ( 4 B)
    // Compaction strategy mirrors what the legacy Dx11RenderingDrawingRoom
    // does: room geometry is heavy on vertex count and the room shader only
    // needs limited precision for color / UV.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct RoomVertex
    {
        public Vector3 Position;     // 0..12
        public uint    ColorRgba8;   // 12..16 (R8G8B8A8_UNorm)
        public ushort  UvU;          // 16..18 (R16G16_UNorm, atlas)
        public ushort  UvV;          // 18..20
        public Half    GridUvU;      // 20..22 (R16G16_Float)
        public Half    GridUvV;      // 22..24
    }

    private static uint PackColor(Vector3 c, bool overlayFlag = false)
    {
        uint r = (uint)Math.Clamp((int)(c.X * 255f + 0.5f), 0, 255);
        uint g = (uint)Math.Clamp((int)(c.Y * 255f + 0.5f), 0, 255);
        uint b = (uint)Math.Clamp((int)(c.Z * 255f + 0.5f), 0, 255);
        // Alpha byte doubles as the sector-overlay flag: 255 → composite
        // sprite add/sub style in the pixel shader, 0 → plain multiply.
        uint a = overlayFlag ? 255u : 0u;
        return r | (g << 8) | (b << 16) | (a << 24);
    }

    private static ushort PackUNorm16(float v) =>
        (ushort)Math.Clamp((int)(v * 65535f + 0.5f), 0, 65535);

    private sealed class RoomMesh : IDisposable
    {
        public BufferHandle Vb;
        public int          VertexCount;
        public bool         Disposed;

        public RoomMesh(BufferHandle vb, int vertexCount)
        {
            Vb          = vb;
            VertexCount = vertexCount;
        }

        public void Dispose() => Disposed = true;
    }

    public IRhiDevice     Device    => _device;
    public SwapchainHandle Swapchain => _swap;
    /// <summary>Rooms drawn by the last RenderFrame call (post visibility + frustum cull).</summary>
    public IReadOnlyList<Room> LastVisibleRooms => _visibleRooms;

    public LevelRenderer(IntPtr hwnd, int width, int height)
    {
        _width  = Math.Max(1, width);
        _height = Math.Max(1, height);
        _device = new Dx11Device();

        // Share the D3D device with the preview panel + thumbnail renderer.
        // Without this, V2PreviewDevice would lazily allocate its own
        // ID3D11Device on the first thumbnail tick (~50-200 ms cold-start hit
        // that lands right when the user just finished loading a wad).
        V2PreviewDevice.RegisterSharedDevice(_device);

        _swap = _device.CreateSwapchain(new SwapchainDesc(
            hwnd:    hwnd,
            width:   _width,
            height:  _height,
            color:   Format.R8G8B8A8_UNorm,
            depth:   Format.D24_UNorm_S8_UInt,
            samples: 4,        // 4x MSAA — smooths sector grid lines and geometry edges
            vsync:   true));

        InitRoomPipeline();
    }

    private void InitRoomPipeline()
    {
        var (vs, ps) = ShaderLibrary.Load("RoomGeometry");
        _roomPipeline = _device.CreatePipeline(new PipelineDesc
        {
            VertexShader   = vs,
            FragmentShader = ps,
            VertexAttributes = new[]
            {
                new VertexAttribute("POSITION", 0, Format.R32G32B32_Float, bufferSlot: 0, offset: 0),
                new VertexAttribute("COLOR",    0, Format.R8G8B8A8_UNorm,  bufferSlot: 0, offset: 12),
                new VertexAttribute("TEXCOORD", 0, Format.R16G16_UNorm,    bufferSlot: 0, offset: 16),
                new VertexAttribute("TEXCOORD", 1, Format.R16G16_Float,    bufferSlot: 0, offset: 20),
            },
            VertexBufferLayouts    = new[] { new VertexBufferLayout(strideBytes: 24) },
            Topology               = PrimitiveTopology.TriangleList,
            // TR room geometry is wound so that triangles face *into* the
            // room. With CullMode.None the back-facing exterior surfaces
            // overlap the interior ones and the level looks "inside-out".
            // Match the legacy renderer: cull back faces so walls disappear
            // when the camera is outside the room.
            Rasterizer             = new RasterizerState(CullMode.Back),
            DepthStencil           = DepthStencilState.Default,
            BlendStates            = new[] { BlendState.Opaque },
            ColorAttachmentFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt,
            DebugName              = "RoomGeometryPipeline",
        });

        _viewCb = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: System.Runtime.CompilerServices.Unsafe.SizeOf<ViewParams>(),
                usage:     BufferUsage.DynamicUniform,
                bindFlags: BufferBindFlags.Constant,
                debugName: "RoomViewParams"),
            ReadOnlySpan<byte>.Empty);

        _objects = new ObjectRenderer(_device);
    }

    public void Resize(int width, int height)
    {
        width  = Math.Max(1, width);
        height = Math.Max(1, height);
        if (width == _width && height == _height) return;

        _device.WaitIdle();
        _device.ResizeSwapchain(_swap, width, height);
        _width  = width;
        _height = height;
    }

    public void RenderFrame(in RenderScene scene)
    {
        var cl = _device.BeginCommandList();

        // Upload view-projection. NOTE: no transpose. .NET's Matrix4x4 is
        // row-major and HLSL matrices default to column-major, so the
        // raw byte upload is read by the shader as the transpose, which is
        // exactly what mul(M, v_column) expects when the CPU code uses
        // row-vector convention. The legacy renderer follows the same
        // convention — verified against Dx11RenderingStateBuffer.
        var vp = new ViewParams
        {
            ViewProjection = scene.ViewProjection,
            GridLineWidth  = scene.GridLineWidth,
            GridEnabled    = scene.TexturingMode ? 0f : 1f,
        };
        unsafe
        {
            var span = new ReadOnlySpan<byte>(&vp, sizeof(ViewParams));
            cl.UpdateBuffer(_viewCb, 0, span);
        }

        _scratchClearCol[0] = new Vector4(0.08f, 0.08f, 0.12f, 1f);
        cl.BeginPass(new PassDesc
        {
            UseSwapchain   = true,
            Swapchain      = _swap,
            ColorLoadOps   = _scratchLoadOps,
            ColorStoreOps  = _scratchStoreOps,
            DepthLoadOp    = LoadOp.Clear,
            DepthStoreOp   = StoreOp.Store,
            ClearColors    = _scratchClearCol,
            ClearDepth     = 1.0f,
            ViewportWidth  = _width,
            ViewportHeight = _height,
        });

        if (scene.Level != null)
        {
            EnsureAtlas(scene.Level);

            if (_meshesAreTexturing != scene.TexturingMode)
            {
                DropRoomMeshes();
                _meshesAreTexturing = scene.TexturingMode;
            }

            cl.SetPipeline(_roomPipeline);
            _scratchCbuf[0] = _viewCb;
            _scratchTex [0] = _atlas!.Texture;
            _scratchSamp[0] = _atlas.Sampler;
            cl.SetBindings(new Bindings
            {
                ConstantBuffers = _scratchCbuf,
                Textures        = _scratchTex,
                Samplers        = _scratchSamp,
            });

            // One SectorTextureDefault per frame, mutated per room: only the
            // selected room gets a populated SelectionArea / HighlightArea
            // (matches the legacy CacheRoom behavior).
            var st = new SectorTextureDefault
            {
                ColoringInfo                  = scene.ColoringInfo,
                DrawIllegalSlopes             = scene.ShowIllegalSlopes,
                DrawSlideDirections           = scene.ShowSlideDirections,
                ProbeAttributesThroughPortals = scene.ProbeAttributesThroughPortals,
                HideHiddenRooms               = scene.HideHiddenRooms,
            };

            // Match the legacy visible-room set: with ShowAllRooms off and
            // no portal walking, only the selected room is drawn — a 50-room
            // level becomes 1 draw call instead of 50. Frustum-cull the rest
            // so large levels stay quick when ShowAllRooms is on.
            CollectVisibleRooms(scene);

            foreach (Room room in _visibleRooms)
            {
                if (ReferenceEquals(room, scene.SelectedRoom))
                {
                    st.SelectionArea  = scene.SelectionArea;
                    st.HighlightArea  = scene.HighlightArea;
                    st.SelectionArrow = scene.SelectionArrow;
                }
                else
                {
                    st.SelectionArea  = new RectangleInt2(-1, -1, -1, -1);
                    st.HighlightArea  = new RectangleInt2(-1, -1, -1, -1);
                    st.SelectionArrow = ArrowType.EntireFace;
                }

                var mesh = GetOrCreateRoomMesh(room, st.Get, scene.TexturingMode);
                if (mesh.VertexCount == 0) continue;
                _scratchVb[0] = new VertexBufferBinding(mesh.Vb, 0);
                cl.SetVertexBuffers(_scratchVb);
                cl.Draw(mesh.VertexCount);
            }

            // Objects (moveables / statics / imported geometry). Reuses the
            // same view-projection cbuffer + atlas + sampler bindings; the
            // object pipeline expects ModelMatrix + Tint via push constants.
            if (_objects != null)
                _objects.Render(cl, _visibleRooms, scene.Level, _atlas,
                                scene.ShowMoveables, scene.ShowStatics, scene.ShowImportedGeometry);
        }

        cl.EndPass();
        _device.Submit(cl);
        _device.Present(_swap);
    }

    /// <summary>Backwards-compatible no-arg overload kept for the boot path that hasn't built a scene yet.</summary>
    public void RenderFrame()
    {
        var cl = _device.BeginCommandList();
        cl.BeginPass(new PassDesc
        {
            UseSwapchain   = true,
            Swapchain      = _swap,
            ColorLoadOps   = new[] { LoadOp.Clear },
            ColorStoreOps  = new[] { StoreOp.Store },
            DepthLoadOp    = LoadOp.Clear,
            DepthStoreOp   = StoreOp.Store,
            ClearColors    = new[] { new Vector4(0.0f, 0.35f, 0.45f, 1f) },
            ClearDepth     = 1.0f,
            ViewportWidth  = _width,
            ViewportHeight = _height,
        });
        cl.EndPass();
        _device.Submit(cl);
        _device.Present(_swap);
    }

    private void CollectVisibleRooms(in RenderScene scene)
    {
        _visibleRooms.Clear();
        if (scene.Level == null) return;

        // Default editor view (no ShowAllRooms / no ShowPortals): just the
        // selected room. Same as the legacy CollectRoomsToDraw fast path.
        if (!scene.ShowAllRooms && !scene.ShowPortals)
        {
            if (scene.SelectedRoom?.RoomGeometry != null)
                _visibleRooms.Add(scene.SelectedRoom);
            return;
        }

        // ShowAllRooms (or ShowPortals — for now treated the same; proper
        // portal walking from the camera room will come with the portal
        // pass). Frustum-cull the level's room set.
        _frustum.Update(scene.Camera, scene.ViewportSize);
        foreach (Room room in scene.Level.Rooms)
        {
            if (room?.RoomGeometry == null) continue;
            // Skip the alternate/flipped variant book-keeping rooms so we
            // don't double-draw them. Mirror the legacy ShowAllRooms branch:
            // when the selected room isn't a flipped-alternate, hide the
            // alternated rooms (they're shown only when their base is
            // explicitly switched). For first pass, drop alternated ones.
            if (room.Alternated && room.AlternateBaseRoom != null) continue;
            if (!_frustum.Contains(room.WorldBoundingBox)) continue;
            _visibleRooms.Add(room);
        }
    }

    private void EnsureAtlas(Level level)
    {
        if (_atlas != null && ReferenceEquals(_atlasLevel, level)) return;
        _atlas?.Dispose();
        _atlas = new TextureAtlas(_device, level);
        _atlasLevel = level;
        // Mesh UVs depend on the atlas layout, so previously cached meshes
        // (built against the OLD atlas) become invalid the moment we swap.
        DropRoomMeshes();
    }

    private RoomMesh GetOrCreateRoomMesh(Room room, SectorTextureGetDelegate sectorTextureGet, bool texturing)
    {
        if (_roomMeshes.TryGetValue(room, out var existing))
            return existing;
        var mesh = BuildRoomMesh(room, sectorTextureGet, texturing);
        _roomMeshes[room] = mesh;
        return mesh;
    }

    private void DropRoomMeshes()
    {
        foreach (var m in _roomMeshes.Values)
            _device.Destroy(m.Vb);
        _roomMeshes.Clear();
    }

    /// <summary>Invalidate a single room's cached mesh (call on RoomGeometryChanged).</summary>
    public void InvalidateRoom(Room room)
    {
        if (_roomMeshes.Remove(room, out var mesh))
            _device.Destroy(mesh.Vb);
    }

    /// <summary>Drop all cached room meshes and the atlas (call on LevelChanged).</summary>
    public void InvalidateAllRooms()
    {
        DropRoomMeshes();
        _atlas?.Dispose();
        _atlas = null;
        _atlasLevel = null;
        _objects?.InvalidateAll();
    }

    // Editor-look tints applied on top of the SectorTextureDefault.Color
    // classification. Match the legacy "selection red / highlight yellow"
    // visual reasonably closely without being pixel-perfect.
    private static readonly Vector3 _selectionTint   = new(1.0f, 0.15f, 0.15f);
    private static readonly Vector3 _highlightTint   = new(1.0f, 0.95f, 0.30f);

    private RoomMesh BuildRoomMesh(Room room, SectorTextureGetDelegate sectorTextureGet, bool texturing)
    {
        var geom = room.RoomGeometry;
        int singleSidedVertexCount = geom.VertexPositions.Count;
        if (singleSidedVertexCount == 0 || _atlas == null)
            return new RoomMesh(default, 0);

        int triCount = singleSidedVertexCount / 3;

        // Count visible triangles. Hidden / Invisible drop out of the VB.
        int visibleTris = 0;
        var triResults = new SectorTextureResult[triCount];
        for (int i = 0; i < triCount; i++)
        {
            var ta = geom.TriangleTextureAreas[i];
            if (ta.Texture is TextureInvisible) continue;

            SectorFaceIdentity faceId = geom.TriangleSectorInfo[i];
            triResults[i] = sectorTextureGet(room, faceId.Position.X, faceId.Position.Y, faceId.Face);

            if (triResults[i].Hidden) continue;
            visibleTris++;
        }

        if (visibleTris == 0)
            return new RoomMesh(default, 0);

        Vector3 wp = room.WorldPos;
        var verts = new RoomVertex[visibleTris * 3];
        int outIdx = 0;

        for (int i = 0; i < triCount; i++)
        {
            var ta = geom.TriangleTextureAreas[i];
            if (ta.Texture is TextureInvisible) continue;

            var res = triResults[i];
            if (res.Hidden) continue;

            Vector2 eu0 = geom.VertexEditorUVs[i * 3 + 0];
            Vector2 eu1 = geom.VertexEditorUVs[i * 3 + 1];
            Vector2 eu2 = geom.VertexEditorUVs[i * 3 + 2];

            Vector3 c0, c1, c2;
            Vector2 uv0, uv1, uv2;
            bool overlay = false;

            if (texturing)
            {
                // Texturing mode:
                //   - Textured face → full-bright texture (RoomDisableVertexColors path)
                //   - Untextured face → fall back to the room lighting so
                //     the geometry is still visible (otherwise unfinished
                //     levels would be a uniform sheet of white).
                Vector3 tint = res.Dimmed ? new Vector3(0.5f) : Vector3.One;
                if (res.Highlighted) tint = Vector3.Lerp(tint, _highlightTint, 0.30f);
                if (res.Selected)    tint = Vector3.Lerp(tint, _selectionTint, 0.45f);

                if (ta.Texture != null && !ta.Texture.IsUnavailable && ta.Texture is not TextureInvisible)
                {
                    uv0 = _atlas.GetAtlasUv(ta.Texture, ta.TexCoord0);
                    uv1 = _atlas.GetAtlasUv(ta.Texture, ta.TexCoord1);
                    uv2 = _atlas.GetAtlasUv(ta.Texture, ta.TexCoord2);
                    c0 = c1 = c2 = tint;
                }
                else
                {
                    uv0 = uv1 = uv2 = _atlas.WhitePixelUv;
                    c0 = tint * geom.VertexColors[i * 3 + 0];
                    c1 = tint * geom.VertexColors[i * 3 + 1];
                    c2 = tint * geom.VertexColors[i * 3 + 2];
                }
            }
            else
            {
                // Geometry / Lighting / ObjectPlacement modes: sector
                // classification colour + sector overlay sprite (slope
                // arrows, slide markers, ...).
                Vector3 baseColor = new(res.Color.X, res.Color.Y, res.Color.Z);
                if (res.Dimmed)      baseColor *= 0.5f;
                if (res.Highlighted) baseColor = Vector3.Lerp(baseColor, _highlightTint, 0.55f);
                if (res.Selected)    baseColor = Vector3.Lerp(baseColor, _selectionTint, 0.70f);
                c0 = c1 = c2 = baseColor;

                if (res.SectorTexture != SectorTexture.None)
                {
                    uv0 = _atlas.GetSectorOverlayUv(res.SectorTexture, Vector2.Abs(eu0));
                    uv1 = _atlas.GetSectorOverlayUv(res.SectorTexture, Vector2.Abs(eu1));
                    uv2 = _atlas.GetSectorOverlayUv(res.SectorTexture, Vector2.Abs(eu2));
                    overlay = true;
                }
                else
                {
                    uv0 = uv1 = uv2 = _atlas.WhitePixelUv;
                }
            }

            verts[outIdx + 0] = new RoomVertex
            {
                Position   = geom.VertexPositions[i * 3 + 0] + wp,
                ColorRgba8 = PackColor(c0, overlay),
                UvU        = PackUNorm16(uv0.X),
                UvV        = PackUNorm16(uv0.Y),
                GridUvU    = (Half)eu0.X,
                GridUvV    = (Half)eu0.Y,
            };
            verts[outIdx + 1] = new RoomVertex
            {
                Position   = geom.VertexPositions[i * 3 + 1] + wp,
                ColorRgba8 = PackColor(c1, overlay),
                UvU        = PackUNorm16(uv1.X),
                UvV        = PackUNorm16(uv1.Y),
                GridUvU    = (Half)eu1.X,
                GridUvV    = (Half)eu1.Y,
            };
            verts[outIdx + 2] = new RoomVertex
            {
                Position   = geom.VertexPositions[i * 3 + 2] + wp,
                ColorRgba8 = PackColor(c2, overlay),
                UvU        = PackUNorm16(uv2.X),
                UvV        = PackUNorm16(uv2.Y),
                GridUvU    = (Half)eu2.X,
                GridUvV    = (Half)eu2.Y,
            };
            outIdx += 3;
        }

        var bytes = MemoryMarshal.AsBytes(verts.AsSpan());
        var vb = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: bytes.Length,
                usage:     BufferUsage.Immutable,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "Room:" + (room.Name ?? "?")),
            bytes);
        return new RoomMesh(vb, verts.Length);
    }

    public void Dispose()
    {
        _device.WaitIdle();
        InvalidateAllRooms();
        _atlas?.Dispose();
        _objects?.Dispose();
        if (_viewCb.IsValid)        _device.Destroy(_viewCb);
        if (_roomPipeline.IsValid)  _device.Destroy(_roomPipeline);
        if (_swap.IsValid)          _device.Destroy(_swap);
        _device.Dispose();
    }
}
