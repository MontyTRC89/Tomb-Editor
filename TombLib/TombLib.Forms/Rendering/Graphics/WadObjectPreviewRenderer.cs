#nullable enable
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using TombLib;
using TombLib.LevelData;
using TombLib.Rendering.Graphics.Rhi;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.Rendering.Graphics.Preview;

/// <summary>
/// Builds the per-bone world transforms for a moveable's first-keyframe pose.
/// The bone hierarchy in TR-format WADs is encoded in <see cref="WadBone.OpCode"/>
/// + the bone order in <see cref="WadMoveable.Bones"/>, NOT in
/// <see cref="WadBone.Parent"/> (which is null on TR-converted moveables --
/// only Wad2-format moveables populate Parent directly). The legacy
/// AnimatedModel.BuildSkeleton replays the same stack walk; we do the
/// equivalent here so renderers don't need a legacy AnimatedModel.
/// </summary>
public static class WadMoveablePose
{
    public static Matrix4x4[] ComputeBoneTransforms(WadMoveable mv, WadKeyFrame frame)
    {
        int n = mv.Bones.Count;
        var transforms = new Matrix4x4[n];
        // Default-initialised Matrix4x4 is all zeros, not identity -- anything
        // we don't explicitly fill in must default to identity so we don't
        // crush vertices to (0,0,0) on malformed OpCode streams.
        for (int i = 0; i < n; i++) transforms[i] = Matrix4x4.Identity;
        if (n == 0)
            return transforms;

        // Root.
        Matrix4x4 rootRot = (frame != null && 0 < frame.Angles.Count)
                            ? frame.Angles[0].RotationMatrix
                            : Matrix4x4.Identity;
        Vector3   rootOff = frame != null ? frame.Offset : Vector3.Zero;
        transforms[0] = rootRot * Matrix4x4.CreateTranslation(rootOff);

        var stack    = new Stack<int>();
        int curIdx   = 0;

        for (int j = 1; j < n; j++)
        {
            var bone   = mv.Bones[j];
            Matrix4x4 rot = (frame != null && j < frame.Angles.Count)
                          ? frame.Angles[j].RotationMatrix
                          : Matrix4x4.Identity;
            Matrix4x4 trans = Matrix4x4.CreateTranslation(bone.Translation);

            // Mirror the legacy stack walk in AnimatedModel.BuildSkeleton.
            int parentIdx;
            switch (bone.OpCode)
            {
                case WadLinkOpcode.NotUseStack:
                    parentIdx = curIdx;
                    break;
                case WadLinkOpcode.Pop:
                    if (stack.Count > 0) curIdx = stack.Pop();
                    parentIdx = curIdx;
                    break;
                case WadLinkOpcode.Push:
                    stack.Push(curIdx);
                    parentIdx = curIdx;
                    break;
                case WadLinkOpcode.Read:
                    parentIdx = stack.Count > 0 ? stack.Peek() : curIdx;
                    break;
                default:
                    parentIdx = curIdx;
                    break;
            }

            transforms[j] = rot * trans * transforms[parentIdx];
            curIdx = j;
        }
        return transforms;
    }
}

/// <summary>
/// renderer for previewing a single <see cref="IWadObject"/>. Used by both
/// the item-browser preview panel and the content-browser thumbnail capturer.
///
/// <para>Holds ONE shared B8G8R8A8 atlas across every cached object -- same
/// approach as the legacy <c>WadRenderer.AllocateTexture</c>. New textures
/// are shelf-packed lazily and uploaded with <see cref="IRhiDevice.UpdateTexture"/>
/// (region-only) instead of re-creating a per-object texture on each
/// <see cref="Render"/> call. This is the difference between ~16 KB / new
/// texture GPU traffic (here) and N MB / object (the old per-object atlas
/// approach).</para>
///
/// <para>Hot paths use SIMD where it pays off: atlas row blits and CPU->CPU
/// region extracts go through AVX2/SSE2 stores (32B/16B per iter); the
/// per-vertex color pack uses SSE2 saturating int->byte packs (~5 ops vs ~15
/// scalar). Callers own the <see cref="IRhiDevice"/> and the active render
/// pass.</para>
/// </summary>
public sealed class WadObjectPreviewRenderer : IDisposable
{
    // ---- Shared atlas pool (up to MaxAtlases 2048-square textures) --------
    // The previous renderer used a single 2048-square atlas and fell back to
    // the white pixel once it filled up, hiding textures from the user
    // without any warning. We now keep a list of up to MaxAtlases atlases;
    // when the active one is full a new one is allocated. Only when the cap
    // is reached do we fall back to white (and warn via NLog).

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private const int AtlasSize           = 2048;
    private const int Gutter              = 4;
    private const int WhitePixelBlockSize = 3;
    private const int MaxAtlases          = 4;   // 4 x 2048^2 x 4 bytes = 64 MB cap

    private sealed class AtlasSlot
    {
        public TextureHandle Texture;
        public byte[]        Bytes = Array.Empty<byte>();
        public RectPackerSimpleStack? Packer;
    }
    private readonly List<AtlasSlot> _atlases = new();
    private bool _atlasPoolExhaustedReported;

    private readonly struct AtlasEntry
    {
        public readonly int        AtlasIndex;
        public readonly VectorInt2 Origin;
        public AtlasEntry(int atlasIndex, VectorInt2 origin)
        {
            AtlasIndex = atlasIndex;
            Origin     = origin;
        }
    }
    private readonly Dictionary<Texture, AtlasEntry> _texturePositions = new();
    private AtlasEntry _whitePixel;             // (1,1) -- middle of the
                                                 // reserved 3x3 white block in atlas 0.

    // ---- Per-object cache (vertex buffer + per-atlas segment list) --------

    private readonly struct AtlasSegment
    {
        public readonly int AtlasIndex;
        public readonly int FirstVertex;
        public readonly int VertexCount;
        public AtlasSegment(int atlasIndex, int firstVertex, int vertexCount)
        {
            AtlasIndex  = atlasIndex;
            FirstVertex = firstVertex;
            VertexCount = vertexCount;
        }
    }

    private sealed class PerObject
    {
        public BufferHandle    Vb;
        public int             VertexCount;
        public BoundingBox     Bounds;
        public AtlasSegment[]  Segments = Array.Empty<AtlasSegment>();
    }
    private readonly Dictionary<IWadObject, PerObject> _cache = new();
    // Standalone mesh cache for editor scenes (skeleton / mesh editors) that
    // render WadMeshes directly with per-call transforms -- not associated to
    // any single IWadObject. Lives in the same shared atlas as _cache.
    private readonly Dictionary<WadMesh, PerObject>     _meshCache = new();

    // ---- Pipeline + per-frame scratch -------------------------------------

    private readonly IRhiDevice    _device;
    private readonly PipelineHandle _pipeline;
    private readonly BufferHandle   _viewCb;
    private readonly BufferHandle   _identityInstanceVb;
    private BufferHandle            _dynamicInstanceVb;   // resized on demand
    private int                     _dynamicInstanceCapacity;
    private byte[]                  _batchCpu = new byte[InstanceStride * 16];
    private readonly List<(PerObject Mesh, int InstanceOffset)> _pendingDraws = new();
    private int                     _pendingInstanceCount;
    private readonly SamplerHandle  _sampler;

    private readonly BufferHandle[]        _scratchCbuf = new BufferHandle[1];
    private readonly TextureHandle[]       _scratchTex  = new TextureHandle[1];
    private readonly SamplerHandle[]       _scratchSamp = new SamplerHandle[1];
    private readonly VertexBufferBinding[] _scratchVbs  = new VertexBufferBinding[2];

    public IRhiDevice Device => _device;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ObjectVertex
    {
        public Vector3 Position;
        public uint    Color;
        public ushort  UvU;
        public ushort  UvV;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 256)]
    private struct ViewParams
    {
        public Matrix4x4 ViewProjection;
        public float     GridLineWidth;
        public float     GridEnabled;
        public float     _pad1, _pad2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct InstanceData
    {
        public Matrix4x4 Model;
        public Vector4   Tint;
    }
    private const int InstanceStride = 80;

    public WadObjectPreviewRenderer(IRhiDevice device)
    {
        _device = device;

        var (vs, ps) = ShaderLibrary.Load("Object");
        _pipeline = device.CreatePipeline(new PipelineDesc
        {
            VertexShader   = vs,
            FragmentShader = ps,
            VertexAttributes = new[]
            {
                new VertexAttribute("POSITION", 0, Format.R32G32B32_Float, bufferSlot: 0, offset: 0),
                new VertexAttribute("COLOR",    0, Format.R8G8B8A8_UNorm,  bufferSlot: 0, offset: 12),
                new VertexAttribute("TEXCOORD", 0, Format.R16G16_UNorm,    bufferSlot: 0, offset: 16),
                new VertexAttribute("TEXCOORD", 1, Format.R32G32B32A32_Float, bufferSlot: 1, offset: 0,  perInstance: true),
                new VertexAttribute("TEXCOORD", 2, Format.R32G32B32A32_Float, bufferSlot: 1, offset: 16, perInstance: true),
                new VertexAttribute("TEXCOORD", 3, Format.R32G32B32A32_Float, bufferSlot: 1, offset: 32, perInstance: true),
                new VertexAttribute("TEXCOORD", 4, Format.R32G32B32A32_Float, bufferSlot: 1, offset: 48, perInstance: true),
                new VertexAttribute("TEXCOORD", 5, Format.R32G32B32A32_Float, bufferSlot: 1, offset: 64, perInstance: true),
            },
            VertexBufferLayouts = new[]
            {
                new VertexBufferLayout(strideBytes: 20),
                new VertexBufferLayout(strideBytes: InstanceStride, perInstance: true),
            },
            Topology               = PrimitiveTopology.TriangleList,
            Rasterizer             = new RasterizerState(CullMode.Back),
            DepthStencil           = DepthStencilState.Default,
            BlendStates            = new[] { BlendState.Opaque },
            ColorAttachmentFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt,
            DebugName              = "WadObjectPreviewPipeline",
        });

        _viewCb = device.CreateBuffer(
            new BufferDesc(
                sizeBytes: Unsafe.SizeOf<ViewParams>(),
                usage:     BufferUsage.DynamicUniform,
                bindFlags: BufferBindFlags.Constant,
                debugName: "PreviewViewParams"),
            ReadOnlySpan<byte>.Empty);

        // Single-instance buffer with identity model + white tint. The object
        // mesh is in its own model space; the camera/view-projection is what
        // moves it around.
        Span<InstanceData> id = stackalloc InstanceData[1];
        id[0] = new InstanceData
        {
            Model = Matrix4x4.Transpose(Matrix4x4.Identity),
            Tint  = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
        };
        _identityInstanceVb = device.CreateBuffer(
            new BufferDesc(
                sizeBytes: InstanceStride,
                usage:     BufferUsage.Immutable,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "PreviewIdentityInstance"),
            MemoryMarshal.AsBytes(id));

        // Linear+Clamp; preview camera distance is known, no need for
        // anisotropic / mip chain. (The gutter then only has to fight the
        // sampler footprint at mip 0.)
        _sampler = device.CreateSampler(new SamplerDesc(
            FilterMode.Linear, AddressMode.Clamp, maxAnisotropy: 1));

        // Dynamic instance buffer used by mesh batches. Resized on demand --
        // start at 16 instances so single-mesh draws don't pay any growth cost.
        _dynamicInstanceCapacity = 16;
        _dynamicInstanceVb = device.CreateBuffer(
            new BufferDesc(
                sizeBytes: _dynamicInstanceCapacity * InstanceStride,
                usage:     BufferUsage.DynamicVertex,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "PreviewDynamicInstance"),
            ReadOnlySpan<byte>.Empty);
    }

    private void EnsureBatchCapacity(int instanceCount)
    {
        int needed = instanceCount * InstanceStride;
        if (_batchCpu.Length < needed)
            Array.Resize(ref _batchCpu, Math.Max(needed, _batchCpu.Length * 2));

        if (_dynamicInstanceCapacity >= instanceCount)
            return;
        int newCap = _dynamicInstanceCapacity;
        while (newCap < instanceCount) newCap *= 2;
        if (_dynamicInstanceVb.IsValid) _device.Destroy(_dynamicInstanceVb);
        _dynamicInstanceCapacity = newCap;
        _dynamicInstanceVb = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: newCap * InstanceStride,
                usage:     BufferUsage.DynamicVertex,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "PreviewDynamicInstance"),
            ReadOnlySpan<byte>.Empty);
    }

    /// <summary>
    /// Write each live atlas to <paramref name="directory"/> as a PNG file
    /// (<c>PreviewAtlas0.png</c>, <c>PreviewAtlas1.png</c>, ...). Useful for
    /// debugging packing / texture-fit issues from the editor's debug menu.
    /// Returns the list of files written.
    /// </summary>
    public IReadOnlyList<string> DumpAtlases(string directory)
    {
        Directory.CreateDirectory(directory);

        var written = new List<string>();
        for (int i = 0; i < _atlases.Count; i++)
        {
            var slot = _atlases[i];
            if (slot.Bytes.Length == 0)
                continue;

            // _atlases[i].Bytes is the same B8G8R8A8 layout we uploaded to
            // the GPU, so ImageC's native BGRA format consumes it directly
            // with no conversion.
            var image = ImageC.FromByteArray(slot.Bytes, AtlasSize, AtlasSize);
            string path = Path.Combine(directory, $"PreviewAtlas{i}.png");
            image.SaveToFile(path);
            written.Add(path);
        }
        return written;
    }

    /// <summary>Drop every atlas + every cached vertex buffer.</summary>
    public void InvalidateAll()
    {
        foreach (var per in _cache.Values)
        {
            if (per.Vb.IsValid)
                _device.Destroy(per.Vb);
        }
        _cache.Clear();

        foreach (var per in _meshCache.Values)
        {
            if (per.Vb.IsValid)
                _device.Destroy(per.Vb);
        }
        _meshCache.Clear();

        _texturePositions.Clear();
        foreach (var atlas in _atlases)
        {
            if (atlas.Texture.IsValid)
                _device.Destroy(atlas.Texture);
        }
        _atlases.Clear();
        _whitePixel = default;
        _atlasPoolExhaustedReported = false;
    }

    /// <summary>
    /// Drop the cached vertex buffer for one specific <see cref="WadMesh"/>.
    /// Editors call this after editing the mesh in place so the next paint
    /// re-tessellates from the new vertex / poly data.
    /// </summary>
    public void InvalidateMesh(WadMesh mesh)
    {
        if (mesh == null)
            return;
        if (_meshCache.TryGetValue(mesh, out var per))
        {
            if (per.Vb.IsValid) _device.Destroy(per.Vb);
            _meshCache.Remove(mesh);
        }
    }

    /// <summary>
    /// Render a single <see cref="WadMesh"/> with the supplied model transform
    /// (baked into the VP). Caches the per-mesh vertex buffer keyed by
    /// <paramref name="mesh"/> in the same shared atlas as <see cref="Render"/>.
    /// Used by the skeleton / mesh editors that need to draw arbitrary bones
    /// without going through an <see cref="IWadObject"/> wrapper.
    /// </summary>
    /// <summary>
    /// Single-call mesh draw. Internally batches as a 1-instance flush so the
    /// view cbuffer is updated exactly once. Editors that draw multiple bone
    /// meshes per frame MUST use <see cref="BeginMeshBatch"/> + per-bone
    /// <see cref="QueueMesh"/> + <see cref="FlushMeshBatch"/> -- calling
    /// RenderMesh in a tight loop performs N WriteDiscard updates of the same
    /// dynamic cbuffer, which on D3D11 silently collapses to the value of
    /// the last update and makes every mesh draw with the same transform.
    /// </summary>
    public void RenderMesh(ICommandList cl, WadMesh mesh, Matrix4x4 model, Matrix4x4 viewProjection)
    {
        BeginMeshBatch();
        QueueMesh(mesh, model);
        FlushMeshBatch(cl, viewProjection);
    }

    /// <summary>
    /// Tinted single-call variant. Same batching caveat as the untinted
    /// overload: use the batch API for multi-mesh frames.
    /// </summary>
    public void RenderMesh(ICommandList cl, WadMesh mesh, Matrix4x4 model, Matrix4x4 viewProjection, Vector4 tint)
    {
        BeginMeshBatch();
        QueueMesh(mesh, model, tint);
        FlushMeshBatch(cl, viewProjection);
    }

    /// <summary>Reset the pending batch -- call before queueing any meshes for a frame.</summary>
    public void BeginMeshBatch()
    {
        _pendingDraws.Clear();
        _pendingInstanceCount = 0;
    }

    /// <summary>Queue one mesh into the pending batch. White tint (no recolour).</summary>
    public void QueueMesh(WadMesh mesh, Matrix4x4 model)
        => QueueMesh(mesh, model, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));

    /// <summary>
    /// Queue one mesh into the pending batch with a per-instance tint. The
    /// shader treats <c>tint.a</c> as a 0..1 replace factor (legacy mesh
    /// selection convention): a = 0 multiplies, a = 1 hard-replaces vertex
    /// colour with tint.rgb.
    /// </summary>
    public void QueueMesh(WadMesh mesh, Matrix4x4 model, Vector4 tint)
    {
        if (mesh == null)
            return;
        var per = GetOrBuildMesh(mesh);
        if (per == null || per.VertexCount == 0)
            return;

        int instanceIndex = _pendingInstanceCount;
        EnsureBatchCapacity(instanceIndex + 1);

        var data = new InstanceData
        {
            Model = Matrix4x4.Transpose(model),
            Tint  = tint,
        };
        MemoryMarshal.Write(_batchCpu.AsSpan(instanceIndex * InstanceStride), ref data);

        _pendingDraws.Add((per, instanceIndex * InstanceStride));
        _pendingInstanceCount++;
    }

    /// <summary>
    /// Upload the accumulated instance data ONCE, then issue one draw per
    /// queued mesh with the appropriate per-instance offset. Mirrors the
    /// pattern used by <c>TombEditor.ObjectRenderer</c>.
    /// </summary>
    public void FlushMeshBatch(ICommandList cl, Matrix4x4 viewProjection)
    {
        if (_pendingInstanceCount == 0)
            return;

        // Single cbuffer update for the frame's camera VP -- no per-bone bake-in.
        var vp = new ViewParams
        {
            ViewProjection = viewProjection,
            GridLineWidth  = 0.0f,
            GridEnabled    = 0.0f,
        };
        unsafe
        {
            var span = new ReadOnlySpan<byte>(&vp, sizeof(ViewParams));
            cl.UpdateBuffer(_viewCb, 0, span);
        }

        // Single instance VB upload for ALL queued meshes.
        cl.UpdateBuffer(_dynamicInstanceVb, 0,
            new ReadOnlySpan<byte>(_batchCpu, 0, _pendingInstanceCount * InstanceStride));

        cl.SetPipeline(_pipeline);
        _scratchCbuf[0] = _viewCb;
        _scratchSamp[0] = _sampler;

        // Track the last bound atlas so we only re-issue SetBindings when the
        // atlas actually changes (most meshes touch a single atlas).
        int boundAtlas = -1;

        foreach (var (mesh, instanceOffset) in _pendingDraws)
        {
            _scratchVbs[0] = new VertexBufferBinding(mesh.Vb, 0);
            _scratchVbs[1] = new VertexBufferBinding(_dynamicInstanceVb, instanceOffset);
            cl.SetVertexBuffers(_scratchVbs);

            foreach (var seg in mesh.Segments)
            {
                if (seg.AtlasIndex != boundAtlas)
                {
                    _scratchTex[0] = _atlases[seg.AtlasIndex].Texture;
                    cl.SetBindings(new Bindings
                    {
                        ConstantBuffers = _scratchCbuf,
                        Textures        = _scratchTex,
                        Samplers        = _scratchSamp,
                    });
                    boundAtlas = seg.AtlasIndex;
                }
                cl.Draw(seg.VertexCount, 1, seg.FirstVertex);
            }
        }

        _pendingDraws.Clear();
        _pendingInstanceCount = 0;
    }

    /// <summary>CPU-side bounding box of the cached mesh, if built.</summary>
    public bool TryGetMeshBounds(WadMesh mesh, out BoundingBox box)
    {
        if (mesh != null && _meshCache.TryGetValue(mesh, out var per))
        {
            box = per.Bounds;
            return true;
        }
        box = default;
        return false;
    }

    private PerObject? GetOrBuildMesh(WadMesh mesh)
    {
        if (_meshCache.TryGetValue(mesh, out var existing))
            return existing;
        EnsureFirstAtlas();

        var acc    = new VertexAccumulator();
        var bounds = new BoundingBox();
        bool boundsInit = false;
        void AccBounds(Vector3 p)
        {
            if (!boundsInit)
            {
                bounds = new BoundingBox(p, p);
                boundsInit = true;
            }
            else
            {
                bounds = new BoundingBox(Vector3.Min(bounds.Minimum, p), Vector3.Max(bounds.Maximum, p));
            }
        }
        AppendWadMesh(acc, mesh, Matrix4x4.Identity, AccBounds);

        var (vertices, segments) = acc.Flatten();
        if (vertices.Length == 0)
            return null;

        var span = MemoryMarshal.AsBytes(vertices.AsSpan());
        var vb   = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: span.Length,
                usage:     BufferUsage.Immutable,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "PreviewMeshStandalone"),
            span);

        var per = new PerObject
        {
            Vb          = vb,
            VertexCount = vertices.Length,
            Bounds      = boundsInit ? bounds : new BoundingBox(Vector3.Zero, Vector3.Zero),
            Segments    = segments,
        };
        _meshCache[mesh] = per;
        return per;
    }

    public bool TryGetBounds(IWadObject obj, out BoundingBox box)
    {
        if (obj != null && _cache.TryGetValue(obj, out var po))
        {
            box = po.Bounds;
            return true;
        }
        box = default;
        return false;
    }

    /// <summary>
    /// Render <paramref name="obj"/> with the supplied view-projection inside
    /// the caller's open render pass. Builds + caches the mesh on first call
    /// per object; the atlas is shared across the cache.
    /// </summary>
    public void Render(ICommandList cl, IWadObject obj, Matrix4x4 viewProjection)
    {
        if (obj == null)
            return;
        var per = GetOrBuild(obj);
        if (per == null || per.VertexCount == 0)
            return;

        // IWadObject builds bake all bone transforms into the VB at construction
        // time, so a single draw with identity model is correct. Still go
        // through the batch path so the cbuffer update follows the same
        // one-per-flush discipline as RenderMesh.
        BeginMeshBatch();

        int instanceIndex = _pendingInstanceCount;
        EnsureBatchCapacity(instanceIndex + 1);
        var data = new InstanceData
        {
            Model = Matrix4x4.Transpose(Matrix4x4.Identity),
            Tint  = new Vector4(1.0f, 1.0f, 1.0f, 0.0f),
        };
        MemoryMarshal.Write(_batchCpu.AsSpan(instanceIndex * InstanceStride), ref data);
        _pendingDraws.Add((per, instanceIndex * InstanceStride));
        _pendingInstanceCount++;

        FlushMeshBatch(cl, viewProjection);
    }

    // ==================================================== Shared atlas state

    // Allocate a fresh atlas slot; the first one also reserves the 3x3 white
    // block at the top-left so polys with missing / unavailable textures have
    // somewhere to land. Returns the new slot's index.
    private int AllocateAtlas()
    {
        var slot = new AtlasSlot
        {
            Bytes  = new byte[AtlasSize * AtlasSize * 4],
            Packer = new RectPackerSimpleStack(new VectorInt2(AtlasSize, AtlasSize)),
        };

        // Create the atlas WITHOUT initial data. The Dx11 backend promotes a
        // ShaderResource-only texture with initialData to Usage.Immutable,
        // which silently rejects UpdateSubresource -- i.e. every subsequent
        // texture region upload below would no-op and the atlas would stay
        // zero-filled (models render fully black). Empty initialData keeps
        // the texture at Usage.Default; UpdateSubresource then works.
        slot.Texture = _device.CreateTexture(
            new TextureDesc(TextureKind.Texture2D, AtlasSize, AtlasSize,
                            Format.B8G8R8A8_UNorm, TextureBindFlags.ShaderResource,
                            mipLevels: 1,
                            debugName: $"PreviewAtlas{_atlases.Count}"),
            ReadOnlySpan<byte>.Empty);

        int atlasIndex = _atlases.Count;
        _atlases.Add(slot);

        if (atlasIndex == 0)
        {
            // First atlas only -- reserve the white-pixel block.
            var pos = slot.Packer.TryAdd(new VectorInt2(WhitePixelBlockSize, WhitePixelBlockSize));
            if (pos.HasValue)
            {
                FillBlock(slot.Bytes, AtlasSize,
                          pos.Value.X, pos.Value.Y,
                          WhitePixelBlockSize, WhitePixelBlockSize,
                          0xFFFFFFFFu);
                _whitePixel = new AtlasEntry(0, new VectorInt2(pos.Value.X + 1, pos.Value.Y + 1));
                UploadRegion(atlasIndex, pos.Value.X, pos.Value.Y,
                             WhitePixelBlockSize, WhitePixelBlockSize);
            }
        }
        else
        {
            _logger.Info("Preview atlas {0} allocated (pool size now {1}/{2}).",
                         atlasIndex, _atlases.Count, MaxAtlases);
        }

        return atlasIndex;
    }

    private void EnsureFirstAtlas()
    {
        if (_atlases.Count == 0)
            AllocateAtlas();
    }

    /// <summary>
    /// Atlas-pixel coord -> (atlas index, normalised UV). Lazily packs
    /// <paramref name="tex"/> into the first atlas that has room, allocating
    /// a new one if every existing atlas is full. Uploads the texture's
    /// region only (not the whole atlas) to the GPU.
    /// </summary>
    private (int Atlas, Vector2 Uv) GetAtlasUv(Texture? tex, Vector2 srcPixelCoord)
    {
        EnsureFirstAtlas();
        AtlasEntry entry = EnsurePacked(tex);
        const float Inv = 1.0f / AtlasSize;
        return (entry.AtlasIndex, new Vector2(
            (entry.Origin.X + srcPixelCoord.X) * Inv,
            (entry.Origin.Y + srcPixelCoord.Y) * Inv));
    }

    private AtlasEntry EnsurePacked(Texture? tex)
    {
        if (tex == null || tex is TextureInvisible || tex.IsUnavailable || tex.Image == null)
            return _whitePixel;
        if (_texturePositions.TryGetValue(tex, out var existing))
            return existing;

        var padded = new VectorInt2(tex.Image.Width + Gutter * 2, tex.Image.Height + Gutter * 2);

        // Try every existing atlas in order. The packer is stateful: once it
        // returns null for one size it'll keep returning null for that size,
        // but a smaller rect from a different texture might still fit.
        for (int i = 0; i < _atlases.Count; i++)
        {
            var slot = _atlases[i];
            var pos  = slot.Packer!.TryAdd(padded);
            if (pos == null)
                continue;

            int paddedX = pos.Value.X;
            int paddedY = pos.Value.Y;
            var inner   = new VectorInt2(paddedX + Gutter, paddedY + Gutter);

            BlitWithGutter(slot.Bytes, AtlasSize, inner, tex.Image);
            UploadRegion(i, paddedX, paddedY, padded.X, padded.Y);

            var entry = new AtlasEntry(i, inner);
            _texturePositions[tex] = entry;
            return entry;
        }

        // Every existing atlas is full -- try to grow the pool.
        if (_atlases.Count < MaxAtlases)
        {
            int newIndex = AllocateAtlas();
            var slot = _atlases[newIndex];
            var pos  = slot.Packer!.TryAdd(padded);
            if (pos.HasValue)
            {
                int paddedX = pos.Value.X;
                int paddedY = pos.Value.Y;
                var inner   = new VectorInt2(paddedX + Gutter, paddedY + Gutter);

                BlitWithGutter(slot.Bytes, AtlasSize, inner, tex.Image);
                UploadRegion(newIndex, paddedX, paddedY, padded.X, padded.Y);

                var entry = new AtlasEntry(newIndex, inner);
                _texturePositions[tex] = entry;
                return entry;
            }
        }

        // Pool exhausted (or single texture > AtlasSize). Fall back to white
        // and warn once per session so the user knows why some models look
        // untextured.
        if (!_atlasPoolExhaustedReported)
        {
            _atlasPoolExhaustedReported = true;
            _logger.Warn("Preview atlas pool exhausted ({0} atlases, {1}x{1} each). " +
                         "Some textures will render as plain white. Consider raising MaxAtlases " +
                         "or downscaling source textures.",
                         MaxAtlases, AtlasSize);
        }
        return _whitePixel;
    }

    // Blit + 4-pixel edge-replicated gutter at (inner). SIMD copies for the
    // inner rows + top/bottom gutter rows; left/right gutter is 4 bytes per
    // row so SIMD wouldn't pay off there.
    private static unsafe void BlitWithGutter(byte[] atlasBytes, int atlasSize, VectorInt2 inner, ImageC img)
    {
        int w = img.Width;
        int h = img.Height;
        byte[] src = img.ToByteArray();
        int rowBytes = w * 4;

        fixed (byte* atlasPtr = atlasBytes)
        fixed (byte* srcPtr   = src)
        {
            // Inner image rows.
            for (int y = 0; y < h; y++)
            {
                byte* dst = atlasPtr + ((inner.Y + y) * atlasSize + inner.X) * 4;
                SimdMemcpy(dst, srcPtr + y * rowBytes, rowBytes);
            }

            // Top + bottom gutter: replicate the adjacent in-image row Gutter
            // times each. Pre-computes the same value Address.Clamp would
            // sample at runtime, but inside the atlas (we can't clamp to a
            // sub-region of the atlas at sample time).
            byte* topSrc = atlasPtr + ( inner.Y          * atlasSize + inner.X) * 4;
            byte* botSrc = atlasPtr + ((inner.Y + h - 1) * atlasSize + inner.X) * 4;
            for (int g = 1; g <= Gutter; g++)
            {
                byte* topDst = atlasPtr + ((inner.Y - g)         * atlasSize + inner.X) * 4;
                byte* botDst = atlasPtr + ((inner.Y + h - 1 + g) * atlasSize + inner.X) * 4;
                SimdMemcpy(topDst, topSrc, rowBytes);
                SimdMemcpy(botDst, botSrc, rowBytes);
            }

            // Left + right gutter, including the corners. yClamped maps rows
            // in the top / bottom gutter to the nearest in-image row so the
            // corner cells take the corner colour.
            for (int y = -Gutter; y < h + Gutter; y++)
            {
                int yc = y < 0 ? 0 : (y >= h ? h - 1 : y);
                byte* rowBase = atlasPtr + ((inner.Y + y) * atlasSize) * 4;
                uint  leftPx  = *(uint*)(atlasPtr + ((inner.Y + yc) * atlasSize + inner.X)         * 4);
                uint  rightPx = *(uint*)(atlasPtr + ((inner.Y + yc) * atlasSize + inner.X + w - 1) * 4);
                for (int g = 1; g <= Gutter; g++)
                {
                    *(uint*)(rowBase + (inner.X - g)         * 4) = leftPx;
                    *(uint*)(rowBase + (inner.X + w - 1 + g) * 4) = rightPx;
                }
            }
        }
    }

    // Extract the freshly-blitted region from the CPU atlas mirror into a
    // tight buffer (no row padding) and ship it to the GPU. We don't reupload
    // the entire 16 MB atlas -- UpdateTexture only touches the (x,y,w,h) box.
    private unsafe void UploadRegion(int atlasIndex, int x, int y, int w, int h)
    {
        var slot     = _atlases[atlasIndex];
        int rowBytes = w * 4;
        var region   = new byte[rowBytes * h];
        fixed (byte* src = slot.Bytes)
        fixed (byte* dst = region)
        {
            for (int row = 0; row < h; row++)
            {
                SimdMemcpy(
                    dst + row * rowBytes,
                    src + ((y + row) * AtlasSize + x) * 4,
                    rowBytes);
            }
        }
        _device.UpdateTexture(slot.Texture, 0, x, y, w, h, rowBytes, region);
    }

    private static unsafe void FillBlock(byte[] atlasBytes, int atlasSize,
                                          int x, int y, int w, int h, uint px)
    {
        fixed (byte* ptr = atlasBytes)
        {
            for (int yy = 0; yy < h; yy++)
            {
                uint* row = (uint*)(ptr + ((y + yy) * atlasSize + x) * 4);
                for (int xx = 0; xx < w; xx++)
                    row[xx] = px;
            }
        }
    }

    // SIMD memcpy: AVX2 32-byte unaligned stores, SSE2 16-byte fallback,
    // scalar tail. Same pattern as TextureAtlas.cs's blit hot path.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe void SimdMemcpy(byte* dst, byte* src, int byteCount)
    {
        int i = 0;
        if (Avx2.IsSupported)
        {
            int n = byteCount & ~31;
            for (; i < n; i += 32)
                Avx.Store(dst + i, Avx.LoadVector256(src + i));
        }
        else if (Sse2.IsSupported)
        {
            int n = byteCount & ~15;
            for (; i < n; i += 16)
                Sse2.Store(dst + i, Sse2.LoadVector128(src + i));
        }
        for (; i < byteCount; i++) dst[i] = src[i];
    }

    // SSE2 RGB Vector3 (0..1) -> packed BGRA uint. Multiply by 255, convert
    // to int32, then two saturating packs (int32->int16->uint8) clamp to
    // [0, 255] for free. ~5 SSE2 ops vs ~15 scalar (3 muls + 3 clamps + 3
    // int conversions + 3 shifts + ORs). Bonus: the high alpha lane lands
    // at 255 automatically because we seeded it with 1.0f.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static uint PackColorRgba8(Vector3 c)
    {
        if (Sse2.IsSupported)
        {
            Vector128<float> v = Vector128.Create(c.X, c.Y, c.Z, 1.0f);
            v = Sse.Multiply(v, Vector128.Create(255.0f));
            Vector128<int>   vi = Sse2.ConvertToVector128Int32(v);              // round-to-nearest
            Vector128<short> sh = Sse2.PackSignedSaturate(vi, vi);              // int32 -> int16, clamp
            Vector128<byte>  by = Sse2.PackUnsignedSaturate(sh, sh);            // int16 -> uint8, clamp [0,255]
            return by.AsUInt32().ToScalar();
        }
        uint r = (uint)Math.Clamp((int)(c.X * 255.0f + 0.5f), 0, 255);
        uint g = (uint)Math.Clamp((int)(c.Y * 255.0f + 0.5f), 0, 255);
        uint b = (uint)Math.Clamp((int)(c.Z * 255.0f + 0.5f), 0, 255);
        return r | (g << 8) | (b << 16) | (0xFFu << 24);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static ushort PackUNorm16(float v) =>
        (ushort)Math.Clamp((int)(v * 65535.0f + 0.5f), 0, 65535);

    // ============================================================ Build path

    // Per-atlas vertex bucket used while a mesh is being tessellated. Each
    // poly is emitted to the bucket whose atlas index matches the texture's
    // EnsurePacked result, so vertices using the same atlas end up
    // contiguous in the final VB and can be drawn with one Draw() call per
    // atlas.
    private sealed class VertexAccumulator
    {
        public readonly Dictionary<int, List<ObjectVertex>> Buckets = new();

        public void Add(int atlasIndex, in ObjectVertex v)
        {
            if (!Buckets.TryGetValue(atlasIndex, out var list))
            {
                list = new List<ObjectVertex>();
                Buckets[atlasIndex] = list;
            }
            list.Add(v);
        }

        // Flatten the buckets into a single contiguous vertex array, in
        // order of ascending atlas index, and produce the matching segment
        // list (atlas index + first vertex + count) for the draw loop.
        public (ObjectVertex[] Vertices, AtlasSegment[] Segments) Flatten()
        {
            int total = 0;
            foreach (var l in Buckets.Values)
                total += l.Count;

            var vertices = new ObjectVertex[total];
            var segments = new AtlasSegment[Buckets.Count];
            int first = 0;
            int seg   = 0;
            foreach (var kv in Buckets.OrderBy(kv => kv.Key))
            {
                kv.Value.CopyTo(vertices, first);
                segments[seg++] = new AtlasSegment(kv.Key, first, kv.Value.Count);
                first += kv.Value.Count;
            }
            return (vertices, segments);
        }
    }

    private PerObject? GetOrBuild(IWadObject obj)
    {
        if (_cache.TryGetValue(obj, out var existing))
            return existing;
        var per = Build(obj);
        if (per != null)
            _cache[obj] = per;
        return per;
    }

    private PerObject? Build(IWadObject obj)
    {
        EnsureFirstAtlas();

        var acc    = new VertexAccumulator();
        var bounds = new BoundingBox();
        bool boundsInit = false;

        void AccBounds(Vector3 p)
        {
            if (!boundsInit)
            {
                bounds = new BoundingBox(p, p);
                boundsInit = true;
            }
            else
            {
                bounds = new BoundingBox(Vector3.Min(bounds.Minimum, p), Vector3.Max(bounds.Maximum, p));
            }
        }

        switch (obj)
        {
            case WadMoveable mv:
                BuildMoveable(mv, acc, AccBounds);
                break;
            case WadStatic st when st.Mesh != null:
                AppendWadMesh(acc, st.Mesh, Matrix4x4.Identity, AccBounds);
                break;
            case ImportedGeometry ig:
                BuildImported(ig, acc, AccBounds);
                break;
            default:
                return null;
        }

        var (vertices, segments) = acc.Flatten();
        if (vertices.Length == 0)
            return null;

        var span = MemoryMarshal.AsBytes(vertices.AsSpan());
        var vb   = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: span.Length,
                usage:     BufferUsage.Immutable,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "PreviewMesh"),
            span);

        return new PerObject
        {
            Vb          = vb,
            VertexCount = vertices.Length,
            Bounds      = boundsInit ? bounds : new BoundingBox(Vector3.Zero, Vector3.Zero),
            Segments    = segments,
        };
    }

    private void BuildMoveable(WadMoveable mv, VertexAccumulator acc, Action<Vector3> trackBounds)
    {
        WadKeyFrame frame = (mv.Animations.Count > 0 && mv.Animations[0].KeyFrames.Count > 0)
                             ? mv.Animations[0].KeyFrames[0]
                             : null;
        var transforms = WadMoveablePose.ComputeBoneTransforms(mv, frame);
        for (int i = 0; i < mv.Bones.Count; i++)
        {
            var bone = mv.Bones[i];
            if (bone.Mesh != null)
                AppendWadMesh(acc, bone.Mesh, transforms[i], trackBounds);
        }
    }

    private void BuildImported(ImportedGeometry imp, VertexAccumulator acc, Action<Vector3> trackBounds)
    {
        var model = imp.DirectXModel;
        if (model?.Meshes == null)
            return;

        foreach (var mesh in model.Meshes)
        {
            if (mesh.Vertices == null || mesh.Indices == null)
                continue;
            bool hasColors = mesh.HasVertexColors;
            foreach (var submesh in mesh.Submeshes.Values)
            {
                var tex = submesh.Material?.Texture;
                Vector2 texSize = tex?.Image is { Width: > 0, Height: > 0 }
                                  ? new Vector2(tex.Image.Width, tex.Image.Height)
                                  : Vector2.One;
                int baseIdx = submesh.BaseIndex;
                int count   = submesh.NumIndices;
                for (int k = 0; k < count; k++)
                {
                    int vi = mesh.Indices[baseIdx + k];
                    var v = mesh.Vertices[vi];
                    Vector3 col      = hasColors ? v.Color : Vector3.One;
                    var (atlas, uv) = GetAtlasUv(tex, v.UV * texSize);
                    acc.Add(atlas, new ObjectVertex
                    {
                        Position = v.Position,
                        Color    = PackColorRgba8(col),
                        UvU      = PackUNorm16(uv.X),
                        UvV      = PackUNorm16(uv.Y),
                    });
                    trackBounds(v.Position);
                }
            }
        }
    }

    private void AppendWadMesh(VertexAccumulator acc, WadMesh mesh, Matrix4x4 transform, Action<Vector3> trackBounds)
    {
        if (mesh == null)
            return;
        var pos = mesh.VertexPositions;
        var col = mesh.VertexColors;
        bool hasColors = col != null && col.Count == pos.Count;

        foreach (var poly in mesh.Polys)
        {
            var ta = poly.Texture;
            // Half-pixel inset -- mirrors legacy ObjectMesh.FromWad2 which calls
            // poly.CorrectTexCoords(0.5f) to keep bilinear filtering from
            // sampling the atlas gutter at face edges (the "bordino" effect).
            var coords = poly.CorrectTexCoords(0.5f);
            var (atlas0, uv0) = GetAtlasUv(ta.Texture, coords[0]);
            var (atlas1, uv1) = GetAtlasUv(ta.Texture, coords[1]);
            var (atlas2, uv2) = GetAtlasUv(ta.Texture, coords[2]);
            var (atlas3, uv3) = poly.Shape == WadPolygonShape.Quad
                                ? GetAtlasUv(ta.Texture, coords[3])
                                : (atlas0, default(Vector2));

            // All UVs of a single poly target the same texture, so all four
            // atlas indices match. Use the first one.
            int atlas = atlas0;

            if (poly.Shape == WadPolygonShape.Triangle)
            {
                Push(poly.Index0, atlas, uv0);
                Push(poly.Index1, atlas, uv1);
                Push(poly.Index2, atlas, uv2);
            }
            else
            {
                Push(poly.Index0, atlas, uv0);
                Push(poly.Index1, atlas, uv1);
                Push(poly.Index2, atlas, uv2);
                Push(poly.Index0, atlas, uv0);
                Push(poly.Index2, atlas, uv2);
                Push(poly.Index3, atlas, uv3);
            }
        }

        void Push(int i, int atlas, Vector2 uv)
        {
            if (i < 0 || i >= pos.Count)
                return;
            Vector3 c = hasColors ? col[i] : Vector3.One;
            Vector3 p = Vector3.Transform(pos[i], transform);
            acc.Add(atlas, new ObjectVertex
            {
                Position = p,
                Color    = PackColorRgba8(c),
                UvU      = PackUNorm16(uv.X),
                UvV      = PackUNorm16(uv.Y),
            });
            trackBounds(p);
        }
    }

    public void Dispose()
    {
        InvalidateAll();
        if (_sampler.IsValid)            _device.Destroy(_sampler);
        if (_identityInstanceVb.IsValid) _device.Destroy(_identityInstanceVb);
        if (_dynamicInstanceVb.IsValid)  _device.Destroy(_dynamicInstanceVb);
        if (_viewCb.IsValid)             _device.Destroy(_viewCb);
        if (_pipeline.IsValid)           _device.Destroy(_pipeline);
    }
}

/// <summary>
/// Geometry/bound helpers for previewing a single WAD object. Pure CPU -- no
/// GPU resources, no dependency on the legacy <c>WadRenderer</c>.
/// </summary>
public static class WadObjectPreviewHelper
{
    public static IWadObject GetRenderObject(IWadObject wadObject, LevelSettings settings)
    {
        if (wadObject is WadMoveable moveable)
        {
            var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(settings.GameVersion, moveable.Id.TypeId));
            var skin = settings.WadTryGetMoveable(skinId);
            if (skin != null && skin != moveable)
                return moveable.ReplaceDummyMeshes(skin);
        }
        return wadObject;
    }

    /// <summary>
    /// CPU-side bounding sphere -- bone-walks moveables, reads static / imported
    /// geometry vertex positions directly. Replaces the legacy version that
    /// needed a WadRenderer to compute it.
    /// </summary>
    public static (Vector3 center, float radius) ComputeBoundingSphere(IWadObject obj)
    {
        Vector3 min, max;
        bool init = false;
        min = max = Vector3.Zero;

        void Acc(Vector3 p)
        {
            if (!init) { min = max = p; init = true; }
            else
            {
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }
        }

        switch (obj)
        {
            case WadMoveable mv:
                {
                    WadKeyFrame frame = (mv.Animations.Count > 0 && mv.Animations[0].KeyFrames.Count > 0)
                                         ? mv.Animations[0].KeyFrames[0]
                                         : null;
                    var transforms = WadMoveablePose.ComputeBoneTransforms(mv, frame);
                    for (int i = 0; i < mv.Bones.Count; i++)
                    {
                        var bone = mv.Bones[i];
                        if (bone.Mesh != null)
                            foreach (var p in bone.Mesh.VertexPositions)
                                Acc(Vector3.Transform(p, transforms[i]));
                    }
                    break;
                }
            case WadStatic st:
                if (st.Mesh != null)
                    foreach (var p in st.Mesh.VertexPositions) Acc(p);
                break;
            case ImportedGeometry ig:
                if (ig.DirectXModel?.Meshes != null)
                    foreach (var m in ig.DirectXModel.Meshes)
                        if (m.Vertices != null)
                            foreach (var v in m.Vertices) Acc(v.Position);
                break;
        }

        if (!init)
            return (new Vector3(0.0f, 256.0f, 0.0f), 640.0f);

        var center = (min + max) * 0.5f;
        var radius = (max - min).Length() * 0.5f;
        if (radius <= 0.0f) radius = 256.0f;
        return (center, radius);
    }
}
