#nullable enable
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
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
        for (int i = 0; i < n; i++) transforms[i] = Matrix4x4.Identity;
        if (n == 0)
            return transforms;

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
/// Renderer for previewing a single <see cref="IWadObject"/>. Used by both
/// the item-browser preview panel and the content-browser thumbnail capturer.
///
/// <para>Holds ONE shared B8G8R8A8 Texture2DArray with up to
/// <see cref="MaxLayers"/> 2048-square layers; new textures are guillotine-
/// packed lazily into the first layer that has room (allocating the next
/// layer on overflow), and uploaded with <see cref="IRhiDevice.UpdateTexture"/>
/// region-by-region instead of re-creating a per-object texture per draw.</para>
///
/// <para>Storing all layers in one array texture means the shader binds the
/// atlas exactly once per frame and reads the layer index from a per-vertex
/// attribute -- no per-segment draws, no rebind churn.</para>
/// </summary>
public sealed class WadObjectPreviewRenderer : IDisposable
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private const int AtlasSize           = 2048;
    private const int Gutter              = 4;
    private const int WhitePixelBlockSize = 3;
    public  const int MaxLayers           = 4;   // 4 x 2048^2 x 4 bytes = 64 MB cap

    private sealed class LayerState
    {
        public byte[]         Bytes  = Array.Empty<byte>();
        public RectPackerTree? Packer;
    }
    private readonly LayerState?[] _layers = new LayerState?[MaxLayers];
    private TextureHandle          _atlasArray;
    private bool                   _atlasArrayCreated;
    private int                    _layerCount;
    private bool                   _atlasPoolExhaustedReported;

    private readonly struct AtlasEntry
    {
        public readonly int        Layer;
        public readonly VectorInt2 Origin;
        public AtlasEntry(int layer, VectorInt2 origin)
        {
            Layer  = layer;
            Origin = origin;
        }
    }
    private readonly Dictionary<Texture, AtlasEntry> _texturePositions = new();
    private AtlasEntry _whitePixel;

    private sealed class PerObject
    {
        public BufferHandle Vb;
        public int          VertexCount;
        public BoundingBox  Bounds;
    }
    private readonly Dictionary<IWadObject, PerObject> _cache     = new();
    private readonly Dictionary<WadMesh, PerObject>    _meshCache = new();

    private readonly IRhiDevice    _device;
    private readonly PipelineHandle _pipeline;
    private readonly BufferHandle   _viewCb;
    private readonly BufferHandle   _identityInstanceVb;
    private BufferHandle            _dynamicInstanceVb;
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
        public uint    Layer;
    }
    private const int ObjectVertexStride = 24;

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
                // Array order MUST match the VK_LOCATION(N) numbering in
                // Object.hlsl -- the Vulkan backend assigns SPIR-V locations
                // from the array index. Layer goes LAST because it is
                // VK_LOCATION(8) in the shader, even though it lives on the
                // per-vertex slot 0.
                new VertexAttribute("POSITION", 0, Format.R32G32B32_Float, bufferSlot: 0, offset: 0),
                new VertexAttribute("COLOR",    0, Format.R8G8B8A8_UNorm,  bufferSlot: 0, offset: 12),
                new VertexAttribute("TEXCOORD", 0, Format.R16G16_UNorm,    bufferSlot: 0, offset: 16),
                new VertexAttribute("TEXCOORD", 1, Format.R32G32B32A32_Float, bufferSlot: 1, offset: 0,  perInstance: true),
                new VertexAttribute("TEXCOORD", 2, Format.R32G32B32A32_Float, bufferSlot: 1, offset: 16, perInstance: true),
                new VertexAttribute("TEXCOORD", 3, Format.R32G32B32A32_Float, bufferSlot: 1, offset: 32, perInstance: true),
                new VertexAttribute("TEXCOORD", 4, Format.R32G32B32A32_Float, bufferSlot: 1, offset: 48, perInstance: true),
                new VertexAttribute("TEXCOORD", 5, Format.R32G32B32A32_Float, bufferSlot: 1, offset: 64, perInstance: true),
                new VertexAttribute("TEXCOORD", 6, Format.R32_UInt,        bufferSlot: 0, offset: 20),
            },
            VertexBufferLayouts = new[]
            {
                new VertexBufferLayout(strideBytes: ObjectVertexStride),
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

        _sampler = device.CreateSampler(new SamplerDesc(
            FilterMode.Linear, AddressMode.Clamp, maxAnisotropy: 1));

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
    /// Write each populated atlas layer to <paramref name="directory"/> as a
    /// PNG file (<c>PreviewAtlas0.png</c>, <c>PreviewAtlas1.png</c>, ...).
    /// Returns the list of files written.
    /// </summary>
    public IReadOnlyList<string> DumpAtlases(string directory)
    {
        Directory.CreateDirectory(directory);

        var written = new List<string>();
        for (int i = 0; i < _layerCount; i++)
        {
            var ls = _layers[i];
            if (ls == null || ls.Bytes.Length == 0)
                continue;

            var image = ImageC.FromByteArray(ls.Bytes, AtlasSize, AtlasSize);
            string path = Path.Combine(directory, $"PreviewAtlas{i}.png");
            image.SaveToFile(path);
            written.Add(path);
        }
        return written;
    }

    public void InvalidateAll()
    {
        foreach (var per in _cache.Values)
            if (per.Vb.IsValid) _device.Destroy(per.Vb);
        _cache.Clear();

        foreach (var per in _meshCache.Values)
            if (per.Vb.IsValid) _device.Destroy(per.Vb);
        _meshCache.Clear();

        _texturePositions.Clear();
        for (int i = 0; i < _layers.Length; i++)
            _layers[i] = null;
        _layerCount = 0;
        _whitePixel = default;
        _atlasPoolExhaustedReported = false;

        if (_atlasArrayCreated && _atlasArray.IsValid)
        {
            _device.Destroy(_atlasArray);
            _atlasArray = default;
            _atlasArrayCreated = false;
        }
    }

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

    public void RenderMesh(ICommandList cl, WadMesh mesh, Matrix4x4 model, Matrix4x4 viewProjection)
    {
        BeginMeshBatch();
        QueueMesh(mesh, model);
        FlushMeshBatch(cl, viewProjection);
    }

    public void RenderMesh(ICommandList cl, WadMesh mesh, Matrix4x4 model, Matrix4x4 viewProjection, Vector4 tint)
    {
        BeginMeshBatch();
        QueueMesh(mesh, model, tint);
        FlushMeshBatch(cl, viewProjection);
    }

    public void BeginMeshBatch()
    {
        _pendingDraws.Clear();
        _pendingInstanceCount = 0;
    }

    public void QueueMesh(WadMesh mesh, Matrix4x4 model)
        => QueueMesh(mesh, model, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));

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

    public void FlushMeshBatch(ICommandList cl, Matrix4x4 viewProjection)
    {
        if (_pendingInstanceCount == 0 || !_atlasArrayCreated)
            return;

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

        cl.UpdateBuffer(_dynamicInstanceVb, 0,
            new ReadOnlySpan<byte>(_batchCpu, 0, _pendingInstanceCount * InstanceStride));

        cl.SetPipeline(_pipeline);
        _scratchCbuf[0] = _viewCb;
        _scratchTex [0] = _atlasArray;
        _scratchSamp[0] = _sampler;
        cl.SetBindings(new Bindings
        {
            ConstantBuffers = _scratchCbuf,
            Textures        = _scratchTex,
            Samplers        = _scratchSamp,
        });

        foreach (var (mesh, instanceOffset) in _pendingDraws)
        {
            _scratchVbs[0] = new VertexBufferBinding(mesh.Vb, 0);
            _scratchVbs[1] = new VertexBufferBinding(_dynamicInstanceVb, instanceOffset);
            cl.SetVertexBuffers(_scratchVbs);
            cl.Draw(mesh.VertexCount, 1, 0);
        }

        _pendingDraws.Clear();
        _pendingInstanceCount = 0;
    }

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
        EnsureAtlasArray();

        var verts  = new List<ObjectVertex>();
        var bounds = new BoundingBox();
        bool boundsInit = false;
        void AccBounds(Vector3 p)
        {
            if (!boundsInit) { bounds = new BoundingBox(p, p); boundsInit = true; }
            else bounds = new BoundingBox(Vector3.Min(bounds.Minimum, p), Vector3.Max(bounds.Maximum, p));
        }
        AppendWadMesh(verts, mesh, Matrix4x4.Identity, AccBounds);

        if (verts.Count == 0)
            return null;

        var span = MemoryMarshal.AsBytes(CollectionsMarshal.AsSpan(verts));
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
            VertexCount = verts.Count,
            Bounds      = boundsInit ? bounds : new BoundingBox(Vector3.Zero, Vector3.Zero),
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

    public void Render(ICommandList cl, IWadObject obj, Matrix4x4 viewProjection)
    {
        if (obj == null)
            return;
        var per = GetOrBuild(obj);
        if (per == null || per.VertexCount == 0)
            return;

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

    // Create the array texture lazily on first use. We always allocate the
    // full MaxLayers up front -- a sparse Texture2DArray costs the same VRAM
    // as a populated one, and lazy-resize would require a GPU copy when the
    // pool grows.
    private void EnsureAtlasArray()
    {
        if (_atlasArrayCreated)
            return;
        _atlasArray = _device.CreateTexture(
            new TextureDesc(TextureKind.Texture2D, AtlasSize, AtlasSize,
                            Format.B8G8R8A8_UNorm, TextureBindFlags.ShaderResource,
                            arrayLayers: MaxLayers, mipLevels: 1,
                            debugName: "PreviewAtlasArray"),
            ReadOnlySpan<byte>.Empty);
        _atlasArrayCreated = true;
        EnsureLayer(0);
        ReserveWhitePixel();
    }

    private LayerState EnsureLayer(int index)
    {
        var ls = _layers[index];
        if (ls != null)
            return ls;
        ls = new LayerState
        {
            Bytes  = new byte[AtlasSize * AtlasSize * 4],
            Packer = new RectPackerTree(new VectorInt2(AtlasSize, AtlasSize)),
        };
        _layers[index] = ls;
        if (index + 1 > _layerCount) _layerCount = index + 1;
        if (index > 0)
            _logger.Info("Preview atlas layer {0} allocated (pool now {1}/{2}).",
                         index, _layerCount, MaxLayers);
        return ls;
    }

    private void ReserveWhitePixel()
    {
        var ls = _layers[0]!;
        var pos = ls.Packer!.TryAdd(new VectorInt2(WhitePixelBlockSize, WhitePixelBlockSize));
        if (!pos.HasValue)
            return;
        FillBlock(ls.Bytes, AtlasSize,
                  pos.Value.X, pos.Value.Y,
                  WhitePixelBlockSize, WhitePixelBlockSize,
                  0xFFFFFFFFu);
        _whitePixel = new AtlasEntry(0, new VectorInt2(pos.Value.X + 1, pos.Value.Y + 1));
        UploadRegion(0, pos.Value.X, pos.Value.Y, WhitePixelBlockSize, WhitePixelBlockSize);
    }

    private (int Layer, Vector2 Uv) GetAtlasUv(Texture? tex, Vector2 srcPixelCoord)
    {
        EnsureAtlasArray();
        AtlasEntry entry = EnsurePacked(tex);
        const float Inv = 1.0f / AtlasSize;
        return (entry.Layer, new Vector2(
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

        // Try every layer in order, allocating the next on demand. A packer
        // that returned null for one size keeps returning null for it, but a
        // smaller rect from a different texture might still fit.
        for (int i = 0; i < MaxLayers; i++)
        {
            var ls  = i < _layerCount ? _layers[i] : EnsureLayer(i);
            var pos = ls!.Packer!.TryAdd(padded);
            if (pos == null)
                continue;

            int paddedX = pos.Value.X;
            int paddedY = pos.Value.Y;
            var inner   = new VectorInt2(paddedX + Gutter, paddedY + Gutter);

            BlitWithGutter(ls.Bytes, AtlasSize, inner, tex.Image);
            UploadRegion(i, paddedX, paddedY, padded.X, padded.Y);

            var entry = new AtlasEntry(i, inner);
            _texturePositions[tex] = entry;
            return entry;
        }

        if (!_atlasPoolExhaustedReported)
        {
            _atlasPoolExhaustedReported = true;
            _logger.Warn("Preview atlas pool exhausted ({0} layers, {1}x{1} each). " +
                         "Some textures will render as plain white. Consider raising MaxLayers " +
                         "or downscaling source textures.",
                         MaxLayers, AtlasSize);
        }
        return _whitePixel;
    }

    private static unsafe void BlitWithGutter(byte[] atlasBytes, int atlasSize, VectorInt2 inner, ImageC img)
    {
        int w = img.Width;
        int h = img.Height;
        byte[] src = img.ToByteArray();
        int rowBytes = w * 4;

        fixed (byte* atlasPtr = atlasBytes)
        fixed (byte* srcPtr   = src)
        {
            for (int y = 0; y < h; y++)
            {
                byte* dst = atlasPtr + ((inner.Y + y) * atlasSize + inner.X) * 4;
                SimdMemcpy(dst, srcPtr + y * rowBytes, rowBytes);
            }

            byte* topSrc = atlasPtr + ( inner.Y          * atlasSize + inner.X) * 4;
            byte* botSrc = atlasPtr + ((inner.Y + h - 1) * atlasSize + inner.X) * 4;
            for (int g = 1; g <= Gutter; g++)
            {
                byte* topDst = atlasPtr + ((inner.Y - g)         * atlasSize + inner.X) * 4;
                byte* botDst = atlasPtr + ((inner.Y + h - 1 + g) * atlasSize + inner.X) * 4;
                SimdMemcpy(topDst, topSrc, rowBytes);
                SimdMemcpy(botDst, botSrc, rowBytes);
            }

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

    // Ship the freshly-blitted CPU region to the GPU at (layer, x, y). With
    // mipLevels = 1, the subresource index is just the layer.
    private unsafe void UploadRegion(int layer, int x, int y, int w, int h)
    {
        var ls       = _layers[layer]!;
        int rowBytes = w * 4;
        var region   = new byte[rowBytes * h];
        fixed (byte* src = ls.Bytes)
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
        _device.UpdateTexture(_atlasArray, layer, x, y, w, h, rowBytes, region);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static uint PackColorRgba8(Vector3 c)
    {
        if (Sse2.IsSupported)
        {
            Vector128<float> v = Vector128.Create(c.X, c.Y, c.Z, 1.0f);
            v = Sse.Multiply(v, Vector128.Create(255.0f));
            Vector128<int>   vi = Sse2.ConvertToVector128Int32(v);
            Vector128<short> sh = Sse2.PackSignedSaturate(vi, vi);
            Vector128<byte>  by = Sse2.PackUnsignedSaturate(sh, sh);
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
        EnsureAtlasArray();

        var verts  = new List<ObjectVertex>();
        var bounds = new BoundingBox();
        bool boundsInit = false;

        void AccBounds(Vector3 p)
        {
            if (!boundsInit) { bounds = new BoundingBox(p, p); boundsInit = true; }
            else bounds = new BoundingBox(Vector3.Min(bounds.Minimum, p), Vector3.Max(bounds.Maximum, p));
        }

        switch (obj)
        {
            case WadMoveable mv:
                BuildMoveable(mv, verts, AccBounds);
                break;
            case WadStatic st when st.Mesh != null:
                AppendWadMesh(verts, st.Mesh, Matrix4x4.Identity, AccBounds);
                break;
            case ImportedGeometry ig:
                BuildImported(ig, verts, AccBounds);
                break;
            default:
                return null;
        }

        if (verts.Count == 0)
            return null;

        var span = MemoryMarshal.AsBytes(CollectionsMarshal.AsSpan(verts));
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
            VertexCount = verts.Count,
            Bounds      = boundsInit ? bounds : new BoundingBox(Vector3.Zero, Vector3.Zero),
        };
    }

    private void BuildMoveable(WadMoveable mv, List<ObjectVertex> verts, Action<Vector3> trackBounds)
    {
        WadKeyFrame frame = (mv.Animations.Count > 0 && mv.Animations[0].KeyFrames.Count > 0)
                             ? mv.Animations[0].KeyFrames[0]
                             : null;
        var transforms = WadMoveablePose.ComputeBoneTransforms(mv, frame);
        for (int i = 0; i < mv.Bones.Count; i++)
        {
            var bone = mv.Bones[i];
            if (bone.Mesh != null)
                AppendWadMesh(verts, bone.Mesh, transforms[i], trackBounds);
        }
    }

    private void BuildImported(ImportedGeometry imp, List<ObjectVertex> verts, Action<Vector3> trackBounds)
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
                int baseIdx = submesh.BaseIndex;
                int count   = submesh.NumIndices;
                for (int k = 0; k < count; k++)
                {
                    int vi = mesh.Indices[baseIdx + k];
                    var v = mesh.Vertices[vi];
                    Vector3 col      = hasColors ? v.Color : Vector3.One;
                    // v.UV is already in source-pixel space (PremultiplyUV
                    // applied at import time), so we hand it straight to
                    // GetAtlasUv without rescaling.
                    var (layer, uv) = GetAtlasUv(tex, v.UV);
                    verts.Add(new ObjectVertex
                    {
                        Position = v.Position,
                        Color    = PackColorRgba8(col),
                        UvU      = PackUNorm16(uv.X),
                        UvV      = PackUNorm16(uv.Y),
                        Layer    = (uint)layer,
                    });
                    trackBounds(v.Position);
                }
            }
        }
    }

    private void AppendWadMesh(List<ObjectVertex> verts, WadMesh mesh, Matrix4x4 transform, Action<Vector3> trackBounds)
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
            var (layer0, uv0) = GetAtlasUv(ta.Texture, coords[0]);
            var (_,      uv1) = GetAtlasUv(ta.Texture, coords[1]);
            var (_,      uv2) = GetAtlasUv(ta.Texture, coords[2]);
            var (_,      uv3) = poly.Shape == WadPolygonShape.Quad
                                ? GetAtlasUv(ta.Texture, coords[3])
                                : (layer0, default(Vector2));

            // All UVs of a single poly target the same texture -- one layer.
            uint layer = (uint)layer0;

            if (poly.Shape == WadPolygonShape.Triangle)
            {
                Push(poly.Index0, layer, uv0);
                Push(poly.Index1, layer, uv1);
                Push(poly.Index2, layer, uv2);
            }
            else
            {
                Push(poly.Index0, layer, uv0);
                Push(poly.Index1, layer, uv1);
                Push(poly.Index2, layer, uv2);
                Push(poly.Index0, layer, uv0);
                Push(poly.Index2, layer, uv2);
                Push(poly.Index3, layer, uv3);
            }
        }

        void Push(int i, uint layer, Vector2 uv)
        {
            if (i < 0 || i >= pos.Count)
                return;
            Vector3 c = hasColors ? col[i] : Vector3.One;
            Vector3 p = Vector3.Transform(pos[i], transform);
            verts.Add(new ObjectVertex
            {
                Position = p,
                Color    = PackColorRgba8(c),
                UvU      = PackUNorm16(uv.X),
                UvV      = PackUNorm16(uv.Y),
                Layer    = layer,
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
