using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.RenderingV2.Rhi;
using TombLib.Utils;

namespace TombEditor.Rendering.V2;

/// <summary>
/// Draws the editor's "service" objects — lights, cameras, sinks, sound
/// sources, memos, flyby cameras, sprites, plus placeholders for moveables /
/// statics / imported-geometry instances whose backing asset is missing.
///
/// <para>Every object renders as a camera-facing billboard quad sampling the
/// embedded <see cref="ServiceObjectTexture"/> icon — same icons the legacy
/// sprite-mode rendering uses. On top of that, selecting a light draws its
/// inner / outer range volume (sphere for point / shadow / fog, cone for
/// spot / sun); selecting a flyby camera draws the FOV cone.</para>
///
/// <para>Two pipelines:
///   - Sprite pass: textured, alpha-blended, depth-tested.
///   - Line pass: solid colour, depth-tested, LineList topology — used for
///     the wireframe-style volumes (same look as the legacy
///     _rasterizerWireframe path without needing a separate fill mode).</para>
/// </summary>
internal sealed class ServiceObjectRenderer : IDisposable
{
    private readonly IRhiDevice    _device;
    private readonly PipelineHandle _spritePipeline;
    private readonly PipelineHandle _linePipeline;
    // Separate view-params cbuffers for the sprite and line passes. They must
    // NOT share one buffer: on Vulkan UpdateBuffer is an immediate memcpy into
    // mapped memory, so a single buffer written twice per frame would make
    // both draws read the value of the LAST write — collapsing every billboard
    // (the sprite pass would read the line pass' zeroed CamRight/CamUp).
    private readonly BufferHandle   _spriteViewCb;
    private readonly BufferHandle   _lineViewCb;
    private readonly TextureHandle  _iconAtlas;
    private readonly SamplerHandle  _iconSampler;

    private BufferHandle _spriteVb;
    private int          _spriteVbCapacity;
    private byte[]       _spriteVbCpu;

    private BufferHandle _lineVb;
    private int          _lineVbCapacity;
    private byte[]       _lineVbCpu;

    private readonly BufferHandle[]        _scratchCbuf = new BufferHandle[1];
    private readonly TextureHandle[]       _scratchTex  = new TextureHandle[1];
    private readonly SamplerHandle[]       _scratchSamp = new SamplerHandle[1];
    private readonly VertexBufferBinding[] _scratchVbs  = new VertexBufferBinding[1];

    // Picking constants exposed to Panel3D so the same hitbox math drives
    // both the renderer's billboards and the ray-cast.
    public const float MarkerHalfExtent  = 192f; // billboard half-size in world units
    public const float LightSphereRadius = 192f; // kept equal to MarkerHalfExtent so the
                                                  // billboard and its pick hitbox agree

    // ----- Icon atlas layout -------------------------------------------------
    // 16 icons of 89×89 each, packed in a 4×4 grid. Each cell gets an
    // edge-replicated gutter so bilinear / mip sampling can't bleed pixels
    // from the neighbouring icon.
    private const int IconSize   = 89;
    private const int IconGutter = 8;
    private const int IconCell   = IconSize + IconGutter * 2;  // 105
    private const int AtlasCols  = 4;
    private const int AtlasRows  = 4;
    private const int AtlasW     = IconCell * AtlasCols;       // 420
    private const int AtlasH     = IconCell * AtlasRows;

    // Light volume tessellation. Kept modest — these only appear for the
    // single selected light, so cost is negligible.
    private const int VolumeRingSegments = 24;
    private const int ConeSpokes         = 16;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct SpriteVertex
    {
        public Vector3 CenterWs;  // world-space object centre (same for all 4 verts of one billboard)
        public Vector2 Corner;    // ±half-extent in world units along camera right / up
        public Vector2 Uv;
        public uint    Tint;      // RGBA8
    }
    private const int SpriteStride = 12 + 8 + 8 + 4; // 32

    // One service-object icon. Billboards are collected, depth-sorted
    // farthest-first, then emitted so overlapping alpha-blended icons
    // composite in the correct order.
    private readonly record struct Billboard(Vector3 Center, ServiceObjectTexture Icon, uint Tint);
    private readonly List<Billboard> _billboards = new();

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct LineVertex
    {
        public Vector3 Position;
        public uint    Color;     // RGBA8
    }
    private const int LineStride = 16;

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 256)]
    private struct SpriteViewParams
    {
        public Matrix4x4 ViewProjection;   // 64
        public Vector4   CamRightWs;       // 16
        public Vector4   CamUpWs;          // 16
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 256)]
    private struct LineViewParams
    {
        public Matrix4x4 ViewProjection;
    }

    public ServiceObjectRenderer(IRhiDevice device)
    {
        _device = device;

        // ----- Sprite pipeline -----
        var (svs, sps) = ShaderLibrary.Load("ServiceSprite");
        _spritePipeline = device.CreatePipeline(new PipelineDesc
        {
            VertexShader   = svs,
            FragmentShader = sps,
            VertexAttributes = new[]
            {
                new VertexAttribute("POSITION", 0, Format.R32G32B32_Float,    bufferSlot: 0, offset: 0),
                new VertexAttribute("TEXCOORD", 0, Format.R32G32_Float,       bufferSlot: 0, offset: 12),
                new VertexAttribute("TEXCOORD", 1, Format.R32G32_Float,       bufferSlot: 0, offset: 20),
                new VertexAttribute("COLOR",    0, Format.R8G8B8A8_UNorm,     bufferSlot: 0, offset: 28),
            },
            VertexBufferLayouts    = new[] { new VertexBufferLayout(strideBytes: SpriteStride) },
            Topology               = PrimitiveTopology.TriangleList,
            Rasterizer             = new RasterizerState(CullMode.None),
            // Depth-tested (hidden behind walls) but no depth write — the
            // alpha-blended billboards must not clip each other with their
            // transparent quad corners; draw order is the depth-sort instead.
            DepthStencil           = DepthStencilState.DepthReadOnly,
            BlendStates            = new[] { BlendState.AlphaBlend },
            ColorAttachmentFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt,
            DebugName              = "ServiceSpritePipeline",
        });

        // ----- Line pipeline (reuses the Gizmo shader: Position + Color) -----
        var (lvs, lps) = ShaderLibrary.Load("Gizmo");
        _linePipeline = device.CreatePipeline(new PipelineDesc
        {
            VertexShader   = lvs,
            FragmentShader = lps,
            VertexAttributes = new[]
            {
                new VertexAttribute("POSITION", 0, Format.R32G32B32_Float, bufferSlot: 0, offset: 0),
                new VertexAttribute("COLOR",    0, Format.R8G8B8A8_UNorm,  bufferSlot: 0, offset: 12),
            },
            VertexBufferLayouts    = new[] { new VertexBufferLayout(strideBytes: LineStride) },
            Topology               = PrimitiveTopology.LineList,
            Rasterizer             = new RasterizerState(CullMode.None),
            DepthStencil           = DepthStencilState.Default,
            BlendStates            = new[] { BlendState.AlphaBlend },
            ColorAttachmentFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt,
            DebugName              = "ServiceLinePipeline",
        });

        _spriteViewCb = device.CreateBuffer(
            new BufferDesc(
                sizeBytes: Unsafe.SizeOf<SpriteViewParams>(),
                usage:     BufferUsage.DynamicUniform,
                bindFlags: BufferBindFlags.Constant,
                debugName: "ServiceSpriteViewParams"),
            ReadOnlySpan<byte>.Empty);
        _lineViewCb = device.CreateBuffer(
            new BufferDesc(
                sizeBytes: Unsafe.SizeOf<LineViewParams>(),
                usage:     BufferUsage.DynamicUniform,
                bindFlags: BufferBindFlags.Constant,
                debugName: "ServiceLineViewParams"),
            ReadOnlySpan<byte>.Empty);

        AllocSpriteVb(8192);
        AllocLineVb(2048);

        // Icon atlas: BGRA8, single mip — CreateTexture takes only mip-0 data
        // here. The per-cell edge-replicated gutter keeps bilinear sampling
        // from bleeding across neighbouring icons.
        var atlasBytes = BuildIconAtlas();
        _iconAtlas = device.CreateTexture(
            new TextureDesc(TextureKind.Texture2D, AtlasW, AtlasH,
                            Format.B8G8R8A8_UNorm, TextureBindFlags.ShaderResource,
                            mipLevels: 1, debugName: "ServiceIconAtlas"),
            atlasBytes);
        // Anisotropic sampling takes several taps even without a mip chain,
        // which softens the minification aliasing on distant billboards.
        _iconSampler = device.CreateSampler(new SamplerDesc(
            FilterMode.Anisotropic, AddressMode.Clamp, maxAnisotropy: 16));
    }

    private static byte[] BuildIconAtlas()
    {
        var bytes = new byte[AtlasW * AtlasH * 4];
        var asm = typeof(ServiceObjectTexture).Assembly;
        var names = Enum.GetNames(typeof(ServiceObjectTexture));
        for (int i = 0; i < names.Length && i < AtlasCols * AtlasRows; i++)
        {
            string resName = nameof(TombLib) + "." + nameof(TombLib.Rendering) +
                             ".ServiceObjectTextures." + names[i] + ".png";
            using var stream = asm.GetManifestResourceStream(resName);
            if (stream == null) continue;
            var img = ImageC.FromStream(stream);
            if (img.Width != IconSize || img.Height != IconSize) continue;
            byte[] src = img.ToByteArray();

            int col = i % AtlasCols, row = i / AtlasCols;
            int ox = col * IconCell + IconGutter;
            int oy = row * IconCell + IconGutter;

            // Inner icon.
            for (int y = 0; y < IconSize; y++)
                Buffer.BlockCopy(src, y * IconSize * 4, bytes,
                                 ((oy + y) * AtlasW + ox) * 4, IconSize * 4);

            // Top / bottom gutter — replicate the icon's edge rows.
            for (int g = 1; g <= IconGutter; g++)
            {
                Buffer.BlockCopy(bytes, (oy * AtlasW + ox) * 4,
                                 bytes, ((oy - g) * AtlasW + ox) * 4, IconSize * 4);
                Buffer.BlockCopy(bytes, ((oy + IconSize - 1) * AtlasW + ox) * 4,
                                 bytes, ((oy + IconSize - 1 + g) * AtlasW + ox) * 4, IconSize * 4);
            }

            // Left / right gutter (full padded height — fills the corners too).
            for (int y = -IconGutter; y < IconSize + IconGutter; y++)
            {
                int yc       = Math.Clamp(y, 0, IconSize - 1);
                int leftIdx  = ((oy + yc) * AtlasW + ox) * 4;
                int rightIdx = ((oy + yc) * AtlasW + ox + IconSize - 1) * 4;
                int rowBase  = (oy + y) * AtlasW * 4;
                for (int g = 1; g <= IconGutter; g++)
                {
                    Buffer.BlockCopy(bytes, leftIdx,  bytes, rowBase + (ox - g) * 4, 4);
                    Buffer.BlockCopy(bytes, rightIdx, bytes, rowBase + (ox + IconSize - 1 + g) * 4, 4);
                }
            }
        }
        return bytes;
    }

    private void AllocSpriteVb(int vertexCapacity)
    {
        if (_spriteVb.IsValid) _device.Destroy(_spriteVb);
        _spriteVbCapacity = vertexCapacity;
        _spriteVbCpu = new byte[vertexCapacity * SpriteStride];
        _spriteVb = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: vertexCapacity * SpriteStride,
                usage:     BufferUsage.DynamicVertex,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "ServiceSpriteVertices"),
            ReadOnlySpan<byte>.Empty);
    }
    private void AllocLineVb(int vertexCapacity)
    {
        if (_lineVb.IsValid) _device.Destroy(_lineVb);
        _lineVbCapacity = vertexCapacity;
        _lineVbCpu = new byte[vertexCapacity * LineStride];
        _lineVb = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: vertexCapacity * LineStride,
                usage:     BufferUsage.DynamicVertex,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "ServiceLineVertices"),
            ReadOnlySpan<byte>.Empty);
    }

    public void Render(ICommandList cl, IReadOnlyList<Room> visibleRooms, Level level,
                        Matrix4x4 viewProjection, Vector3 camPos, Vector3 camTarget,
                        HighlightedObjects highlighted, Vector4 selectionTint,
                        bool showMoveables, bool showStatics, bool showImportedGeometry,
                        bool showLightMeshes)
    {
        if (level == null || visibleRooms == null || visibleRooms.Count == 0) return;

        // Camera basis for world-space billboarding (right + up).
        Vector3 forward = Vector3.Normalize(camTarget - camPos);
        if (forward.LengthSquared() < 1e-6f) forward = -Vector3.UnitZ;
        Vector3 worldUp = Vector3.UnitY;
        Vector3 camRight = Vector3.Normalize(Vector3.Cross(forward, worldUp));
        if (camRight.LengthSquared() < 1e-6f) camRight = Vector3.UnitX;
        Vector3 camUp = Vector3.Normalize(Vector3.Cross(camRight, forward));

        // ---- Build sprite + line geometry (resize loop on each VB) ----
        // Collect every billboard, then depth-sort farthest-first so the
        // alpha-blended icons composite correctly where they overlap.
        _billboards.Clear();
        CollectBillboards(_billboards, visibleRooms, level, highlighted, selectionTint,
                          showMoveables, showStatics, showImportedGeometry);
        _billboards.Sort((a, b) =>
            Vector3.DistanceSquared(b.Center, camPos).CompareTo(
            Vector3.DistanceSquared(a.Center, camPos)));

        int spriteN;
        for (;;)
        {
            var span = MemoryMarshal.Cast<byte, SpriteVertex>(_spriteVbCpu.AsSpan());
            spriteN = 0;
            foreach (var bb in _billboards)
                EmitBillboard(bb.Center, bb.Icon, bb.Tint, span, ref spriteN);
            if (spriteN <= _spriteVbCapacity) break;
            AllocSpriteVb(Math.Max(spriteN, _spriteVbCapacity * 2));
        }

        int lineN;
        for (;;)
        {
            var span = MemoryMarshal.Cast<byte, LineVertex>(_lineVbCpu.AsSpan());
            lineN = BuildLines(span, visibleRooms, highlighted, showLightMeshes);
            if (lineN <= _lineVbCapacity) break;
            AllocLineVb(Math.Max(lineN, _lineVbCapacity * 2));
        }

        // ---- Sprite pass ----
        if (spriteN > 0)
        {
            cl.UpdateBuffer(_spriteVb, 0, new ReadOnlySpan<byte>(_spriteVbCpu, 0, spriteN * SpriteStride));

            var vp = new SpriteViewParams
            {
                ViewProjection = viewProjection,
                CamRightWs     = new Vector4(camRight, 0f),
                CamUpWs        = new Vector4(camUp,    0f),
            };
            unsafe
            {
                var vpSpan = new ReadOnlySpan<byte>(&vp, sizeof(SpriteViewParams));
                cl.UpdateBuffer(_spriteViewCb, 0, vpSpan);
            }

            cl.SetPipeline(_spritePipeline);
            _scratchCbuf[0] = _spriteViewCb;
            _scratchTex[0]  = _iconAtlas;
            _scratchSamp[0] = _iconSampler;
            cl.SetBindings(new Bindings
            {
                ConstantBuffers = _scratchCbuf,
                Textures        = _scratchTex,
                Samplers        = _scratchSamp,
            });
            _scratchVbs[0] = new VertexBufferBinding(_spriteVb, 0);
            cl.SetVertexBuffers(_scratchVbs);
            cl.Draw(spriteN);
        }

        // ---- Line pass (light / flyby volumes) ----
        if (lineN > 0)
        {
            cl.UpdateBuffer(_lineVb, 0, new ReadOnlySpan<byte>(_lineVbCpu, 0, lineN * LineStride));

            var lvp = new LineViewParams { ViewProjection = viewProjection };
            unsafe
            {
                var lvpSpan = new ReadOnlySpan<byte>(&lvp, sizeof(LineViewParams));
                cl.UpdateBuffer(_lineViewCb, 0, lvpSpan);
            }

            cl.SetPipeline(_linePipeline);
            _scratchCbuf[0] = _lineViewCb;
            cl.SetBindings(new Bindings { ConstantBuffers = _scratchCbuf });
            _scratchVbs[0] = new VertexBufferBinding(_lineVb, 0);
            cl.SetVertexBuffers(_scratchVbs);
            cl.Draw(lineN);
        }
    }

    public void Dispose()
    {
        if (_spriteVb.IsValid)       _device.Destroy(_spriteVb);
        if (_lineVb.IsValid)         _device.Destroy(_lineVb);
        if (_spriteViewCb.IsValid)   _device.Destroy(_spriteViewCb);
        if (_lineViewCb.IsValid)     _device.Destroy(_lineViewCb);
        if (_iconAtlas.IsValid)      _device.Destroy(_iconAtlas);
        if (_iconSampler.IsValid)    _device.Destroy(_iconSampler);
        if (_spritePipeline.IsValid) _device.Destroy(_spritePipeline);
        if (_linePipeline.IsValid)   _device.Destroy(_linePipeline);
    }

    // ======================================================== Sprite build

    private static void CollectBillboards(List<Billboard> outList, IReadOnlyList<Room> rooms, Level level,
                                          HighlightedObjects highlighted, Vector4 selectionTint,
                                          bool showMoveables, bool showStatics, bool showImportedGeometry)
    {
        uint selRgba = PackRgba(selectionTint);
        foreach (var room in rooms)
        {
            if (room?.Objects == null) continue;
            Vector3 wp = room.WorldPos;
            foreach (var obj in room.Objects)
            {
                if (!TryGetServiceIcon(obj, level, showMoveables, showStatics, showImportedGeometry,
                                       out ServiceObjectTexture icon, out Vector3 position))
                    continue;

                bool sel = highlighted != null && highlighted.Contains(obj);
                uint tint = sel ? selRgba : ServiceObjectColor(obj);
                outList.Add(new Billboard(wp + position, icon, tint));
            }
        }
    }

    // Per-type sprite tint. Mirrors the legacy DrawPlaceholders / DrawLights /
    // DrawSprites colour table — the icons themselves are mostly white/grey
    // detail so the tint determines the at-a-glance colour code.
    private static uint ServiceObjectColor(ObjectInstance obj)
    {
        switch (obj)
        {
            case LightInstance light:
                return light.Type switch
                {
                    LightType.Point   => Rgb(1.0f, 1.0f, 0.25f),
                    LightType.Spot    => Rgb(1.0f, 1.0f, 0.25f),
                    LightType.Effect  => Rgb(1.0f, 1.0f, 0.25f),
                    LightType.FogBulb => Rgb(1.0f, 0.0f, 1.0f),
                    LightType.Shadow  => Rgb(0.5f, 0.5f, 0.5f),
                    LightType.Sun     => Rgb(1.0f, 0.5f, 0.0f),
                    _                 => 0xFF_FF_FF_FFu,
                };

            case CameraInstance:        return Rgb(0.4f, 0.9f, 0.0f);
            case FlybyCameraInstance fb:
                // Per-sequence hue, same scheme as the legacy
                // GetRandomColorByIndex flyby colouring so two flybys of the
                // same sequence share a colour.
                {
                    var c = TombLib.MathC.GetRandomColorByIndex(fb.Sequence, 32, 0.7f);
                    return Rgb(c.X, c.Y, c.Z);
                }
            case SinkInstance:          return Rgb(0.0f, 0.6f, 1.0f);
            case SoundSourceInstance:   return Rgb(1.0f, 0.7f, 0.0f);
            case MemoInstance:          return 0xFF_FF_FF_FFu;
            case SpriteInstance:        return Rgb(1.0f, 0.5f, 0.0f);

            // Missing-asset placeholders: same blueish/violet cue as legacy
            // DrawPlaceholders uses for the unavailable cube.
            case MoveableInstance:      return Rgb(0.4f, 0.4f, 1.0f);
            case StaticInstance:        return Rgb(0.4f, 0.4f, 1.0f);
            case ImportedGeometryInstance: return Rgb(0.5f, 0.3f, 1.0f);

            default:                    return 0xFF_FF_FF_FFu;
        }
    }

    private static uint Rgb(float r, float g, float b)
    {
        uint ri = (uint)Math.Clamp((int)(r * 255f + 0.5f), 0, 255);
        uint gi = (uint)Math.Clamp((int)(g * 255f + 0.5f), 0, 255);
        uint bi = (uint)Math.Clamp((int)(b * 255f + 0.5f), 0, 255);
        return ri | (gi << 8) | (bi << 16) | (0xFFu << 24);
    }

    // Determines which icon (if any) should represent this instance, plus
    // its world-space position. Returns false for objects that aren't drawn
    // by the service pass (portals, triggers, ghost block, volumes, plus
    // moveables/statics/imported with a valid asset — those are handled by
    // the ObjectRenderer).
    private static bool TryGetServiceIcon(ObjectInstance obj, Level level,
                                           bool showMoveables, bool showStatics, bool showImportedGeometry,
                                           out ServiceObjectTexture icon, out Vector3 pos)
    {
        switch (obj)
        {
            case LightInstance light:
                icon = light.Type switch
                {
                    LightType.Effect  => ServiceObjectTexture.light_effect,
                    LightType.FogBulb => ServiceObjectTexture.light_fog,
                    LightType.Point   => ServiceObjectTexture.light_point,
                    LightType.Shadow  => ServiceObjectTexture.light_shadow,
                    LightType.Spot    => ServiceObjectTexture.light_spot,
                    LightType.Sun     => ServiceObjectTexture.light_sun,
                    _                 => ServiceObjectTexture.unknown,
                };
                pos = light.Position;
                return true;

            case CameraInstance cam:
                icon = ServiceObjectTexture.camera; pos = cam.Position; return true;
            case FlybyCameraInstance flyby:
                icon = ServiceObjectTexture.flyby_camera; pos = flyby.Position; return true;
            case SinkInstance sink:
                icon = ServiceObjectTexture.sink; pos = sink.Position; return true;
            case SoundSourceInstance snd:
                icon = ServiceObjectTexture.sound_source; pos = snd.Position; return true;
            case MemoInstance memo:
                icon = ServiceObjectTexture.memo; pos = memo.Position; return true;
            case SpriteInstance sprite:
                icon = ServiceObjectTexture.sprite; pos = sprite.Position; return true;

            // Missing-asset placeholders — same icons the legacy uses in
            // sprite mode so the user still sees something at the position.
            case MoveableInstance mi when showMoveables:
                if (level.Settings?.WadTryGetMoveable(mi.WadObjectId) == null)
                { icon = ServiceObjectTexture.unknown; pos = mi.Position; return true; }
                break;
            case StaticInstance si when showStatics:
                if (level.Settings?.WadTryGetStatic(si.WadObjectId) == null)
                { icon = ServiceObjectTexture.unknown; pos = si.Position; return true; }
                break;
            case ImportedGeometryInstance ig when showImportedGeometry:
                if (ig.Model?.DirectXModel == null ||
                    ig.Model.DirectXModel.Meshes.Count == 0 || ig.Hidden)
                { icon = ServiceObjectTexture.imp_geo; pos = ig.Position; return true; }
                break;
        }

        icon = ServiceObjectTexture.unknown; pos = default;
        return false;
    }

    private static void EmitBillboard(Vector3 centre, ServiceObjectTexture icon, uint tint,
                                       Span<SpriteVertex> v, ref int n)
    {
        if (n + 6 > v.Length) { n += 6; return; }

        int idx = (int)icon;
        int col = idx % AtlasCols, row = idx / AtlasCols;
        float u0 = (col * IconCell + IconGutter)            / (float)AtlasW;
        float v0 = (row * IconCell + IconGutter)            / (float)AtlasH;
        float u1 = (col * IconCell + IconGutter + IconSize) / (float)AtlasW;
        float v1 = (row * IconCell + IconGutter + IconSize) / (float)AtlasH;
        float h  = MarkerHalfExtent;

        // Quad corners in (right, up) coefficients. Up = +Y in the shader's
        // computed basis, so +up = top-of-screen. UV.v=0 is also top of the
        // image, so the +up corner gets v0.
        var p00 = new SpriteVertex { CenterWs = centre, Corner = new Vector2(-h, -h), Uv = new Vector2(u0, v1), Tint = tint };
        var p10 = new SpriteVertex { CenterWs = centre, Corner = new Vector2(+h, -h), Uv = new Vector2(u1, v1), Tint = tint };
        var p11 = new SpriteVertex { CenterWs = centre, Corner = new Vector2(+h, +h), Uv = new Vector2(u1, v0), Tint = tint };
        var p01 = new SpriteVertex { CenterWs = centre, Corner = new Vector2(-h, +h), Uv = new Vector2(u0, v0), Tint = tint };

        v[n++] = p00; v[n++] = p10; v[n++] = p11;
        v[n++] = p00; v[n++] = p11; v[n++] = p01;
    }

    // ======================================================== Line build
    //
    // Inner range = "green", outer = "blue" — same colour convention as the
    // legacy DrawLights / flyby cone routines. Drawn only for the selected
    // light / flyby (cheap and avoids visual clutter from neighbours).

    private const uint LineInner = 0xFF_00_FF_00u; // green
    private const uint LineOuter = 0xFF_FF_00_00u; // blue
    private const uint LineCone  = 0xFF_00_FF_00u; // green for spot inner & sun
    // Light range units: legacy assumes a 1024-diameter base sphere, so
    // OuterRange / InnerRange scale linearly by 1024 to get world units.
    private const float LightRangeToWorld = 1024f;

    private static int BuildLines(Span<LineVertex> v, IReadOnlyList<Room> rooms,
                                   HighlightedObjects highlighted, bool showLightMeshes)
    {
        if (highlighted == null) return 0;
        int n = 0;
        foreach (var room in rooms)
        {
            if (room?.Objects == null) continue;
            Vector3 wp = room.WorldPos;
            foreach (var obj in room.Objects)
            {
                if (!highlighted.Contains(obj)) continue;

                switch (obj)
                {
                    case LightInstance light when showLightMeshes:
                        EmitLightVolume(wp + light.Position, light, v, ref n);
                        break;

                    case FlybyCameraInstance flyby:
                        // Cone pointing along ObjectMatrix.Forward, length scaled to FOV.
                        EmitFlybyCone(flyby, v, ref n);
                        break;
                }
            }
        }
        return n;
    }

    private static void EmitLightVolume(Vector3 centre, LightInstance light,
                                         Span<LineVertex> v, ref int n)
    {
        switch (light.Type)
        {
            case LightType.Point:
            case LightType.Shadow:
            case LightType.FogBulb:
                if (light.InnerRange > 0f)
                    EmitWireSphere(centre, light.InnerRange * LightRangeToWorld, LineInner, v, ref n);
                EmitWireSphere(centre, light.OuterRange * LightRangeToWorld, LineOuter, v, ref n);
                break;

            case LightType.Spot:
                {
                    var fwd = TransformDir(light.ObjectMatrix, -Vector3.UnitZ);
                    if (light.InnerAngle > 0f)
                        EmitWireCone(centre, fwd, light.InnerRange * LightRangeToWorld,
                                     light.InnerAngle * (float)(Math.PI / 180), LineInner, v, ref n);
                    EmitWireCone(centre, fwd, light.OuterRange * LightRangeToWorld,
                                 light.OuterAngle * (float)(Math.PI / 180), LineOuter, v, ref n);
                    break;
                }

            case LightType.Sun:
                {
                    // Thin direction indicator (very narrow cone).
                    var fwd = TransformDir(light.ObjectMatrix, -Vector3.UnitZ);
                    EmitWireCone(centre, fwd, LightRangeToWorld, 0.05f, LineCone, v, ref n);
                    break;
                }
        }
    }

    private static void EmitFlybyCone(FlybyCameraInstance flyby, Span<LineVertex> v, ref int n)
    {
        if (flyby.Room == null) return;
        Vector3 centre = flyby.Room.WorldPos + flyby.Position;
        Vector3 fwd    = TransformDir(flyby.ObjectMatrix, -Vector3.UnitZ);
        // FOV is total horizontal field of view in degrees. Convert to a
        // half-angle, and pick a fixed length that's still readable in the
        // viewport (1 sector).
        float halfAngle = flyby.Fov * (float)(Math.PI / 360); // /360 = /2 then deg→rad
        if (halfAngle < 0.02f) halfAngle = 0.02f;
        EmitWireCone(centre, fwd, LightRangeToWorld, halfAngle, LineOuter, v, ref n);
    }

    private static Vector3 TransformDir(Matrix4x4 m, Vector3 d)
    {
        var r = Vector3.TransformNormal(d, m);
        return r.LengthSquared() > 1e-6f ? Vector3.Normalize(r) : -Vector3.UnitZ;
    }

    // 3 great circles around X / Y / Z axes — enough to read as a sphere
    // without flooding the line buffer.
    private static void EmitWireSphere(Vector3 centre, float radius, uint color,
                                        Span<LineVertex> v, ref int n)
    {
        int needed = VolumeRingSegments * 2 * 3;
        if (n + needed > v.Length) { n += needed; return; }

        EmitCircle(centre, Vector3.UnitX, Vector3.UnitY, radius, color, v, ref n); // XY
        EmitCircle(centre, Vector3.UnitX, Vector3.UnitZ, radius, color, v, ref n); // XZ
        EmitCircle(centre, Vector3.UnitY, Vector3.UnitZ, radius, color, v, ref n); // YZ
    }

    private static void EmitCircle(Vector3 centre, Vector3 a, Vector3 b, float radius, uint color,
                                    Span<LineVertex> v, ref int n)
    {
        for (int i = 0; i < VolumeRingSegments; i++)
        {
            float t0 = (float)(i       * (Math.PI * 2.0) / VolumeRingSegments);
            float t1 = (float)((i + 1) * (Math.PI * 2.0) / VolumeRingSegments);
            Vector3 p0 = centre + (a * (float)Math.Cos(t0) + b * (float)Math.Sin(t0)) * radius;
            Vector3 p1 = centre + (a * (float)Math.Cos(t1) + b * (float)Math.Sin(t1)) * radius;
            Push(p0, color, v, ref n);
            Push(p1, color, v, ref n);
        }
    }

    // Wire cone: N spokes from the apex to the base + a base circle.
    private static void EmitWireCone(Vector3 apex, Vector3 dir, float length, float halfAngle,
                                      uint color, Span<LineVertex> v, ref int n)
    {
        int needed = ConeSpokes * 2 + VolumeRingSegments * 2;
        if (n + needed > v.Length) { n += needed; return; }

        // Build orthonormal basis around the cone direction.
        Vector3 reference = Math.Abs(dir.Y) > 0.9f ? Vector3.UnitX : Vector3.UnitY;
        Vector3 u = Vector3.Normalize(Vector3.Cross(reference, dir));
        Vector3 w = Vector3.Cross(dir, u);

        float baseRadius = length * (float)Math.Tan(halfAngle);
        Vector3 baseCentre = apex + dir * length;

        // Spokes apex → base ring.
        for (int i = 0; i < ConeSpokes; i++)
        {
            float t = (float)(i * (Math.PI * 2.0) / ConeSpokes);
            Vector3 rim = baseCentre + (u * (float)Math.Cos(t) + w * (float)Math.Sin(t)) * baseRadius;
            Push(apex, color, v, ref n);
            Push(rim,  color, v, ref n);
        }

        // Base circle.
        for (int i = 0; i < VolumeRingSegments; i++)
        {
            float t0 = (float)(i       * (Math.PI * 2.0) / VolumeRingSegments);
            float t1 = (float)((i + 1) * (Math.PI * 2.0) / VolumeRingSegments);
            Vector3 p0 = baseCentre + (u * (float)Math.Cos(t0) + w * (float)Math.Sin(t0)) * baseRadius;
            Vector3 p1 = baseCentre + (u * (float)Math.Cos(t1) + w * (float)Math.Sin(t1)) * baseRadius;
            Push(p0, color, v, ref n);
            Push(p1, color, v, ref n);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Push(Vector3 p, uint color, Span<LineVertex> v, ref int n)
    {
        if (n < v.Length)
        {
            v[n].Position = p;
            v[n].Color    = color;
        }
        n++;
    }

    private static uint PackRgba(Vector4 c)
    {
        uint r = (uint)Math.Clamp((int)(c.X * 255f + 0.5f), 0, 255);
        uint g = (uint)Math.Clamp((int)(c.Y * 255f + 0.5f), 0, 255);
        uint b = (uint)Math.Clamp((int)(c.Z * 255f + 0.5f), 0, 255);
        uint a = (uint)Math.Clamp((int)(c.W * 255f + 0.5f), 0, 255);
        return r | (g << 8) | (b << 16) | (a << 24);
    }
}
