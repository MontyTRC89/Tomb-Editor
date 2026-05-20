using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib.LevelData;
using TombLib.RenderingV2.Rhi;
using TombLib.Wad;

namespace TombEditor.Rendering.V2;

/// <summary>
/// Draws the room's <see cref="ObjectInstance"/> contents: moveables in
/// default pose, statics and imported geometry. Untextured for now — only
/// position + per-vertex color modulated by a per-instance tint. Textures
/// (WAD atlas, ImportedGeometryTexture) come in a follow-up.
/// </summary>
internal sealed class ObjectRenderer : IDisposable
{
    private readonly IRhiDevice    _device;
    private PipelineHandle         _pipeline;

    // GPU mesh built once per source asset (WadStatic / WadMoveable / ImportedGeometry)
    // and reused across every instance.
    private readonly Dictionary<WadStatic, GpuMesh>          _statics      = new();
    private readonly Dictionary<WadMoveable, GpuMesh>        _moveables    = new();
    private readonly Dictionary<ImportedGeometry, GpuMesh>   _imported     = new();

    // Reusable scratch arrays so per-frame drawing doesn't allocate.
    private readonly VertexBufferBinding[] _scratchVb     = new VertexBufferBinding[1];
    private byte[]                          _scratchPush   = new byte[128];

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ObjectVertex
    {
        public Vector3 Position;
        public uint    Color;       // R8G8B8A8_UNorm
        public ushort  UvU;         // R16G16_UNorm atlas UV
        public ushort  UvV;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 128)]
    private struct ObjectPush
    {
        public Matrix4x4 ModelMatrix;     // 64B
        public Vector4   Tint;            // 16B (followed by 48B pad inside Size=128)
    }

    private sealed class GpuMesh : IDisposable
    {
        public BufferHandle Vb;
        public int          VertexCount;
        public bool         Disposed;

        public void Dispose() => Disposed = true;
    }

    public ObjectRenderer(IRhiDevice device)
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
            },
            VertexBufferLayouts    = new[] { new VertexBufferLayout(strideBytes: 20) },
            Topology               = PrimitiveTopology.TriangleList,
            // Object meshes are conventionally wound outward-facing — back-
            // cull as you'd expect for solid props.
            Rasterizer             = new RasterizerState(CullMode.Back),
            DepthStencil           = DepthStencilState.Default,
            BlendStates            = new[] { BlendState.Opaque },
            ColorAttachmentFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt,
            DebugName              = "ObjectPipeline",
        });
    }

    /// <summary>Drop every cached object mesh (call on LoadedWadsChanged / LevelChanged).</summary>
    public void InvalidateAll()
    {
        foreach (var m in _statics.Values)   _device.Destroy(m.Vb);
        foreach (var m in _moveables.Values) _device.Destroy(m.Vb);
        foreach (var m in _imported.Values)  _device.Destroy(m.Vb);
        _statics.Clear();
        _moveables.Clear();
        _imported.Clear();
    }

    private TextureAtlas _atlas;

    public void Render(ICommandList cl, IList<Room> visibleRooms, Level level, TextureAtlas atlas,
                       bool showMoveables, bool showStatics, bool showImportedGeometry)
    {
        if (level == null || atlas == null) return;
        _atlas = atlas;

        cl.SetPipeline(_pipeline);

        foreach (var room in visibleRooms)
        {
            if (room?.Objects == null) continue;

            foreach (var obj in room.Objects)
            {
                switch (obj)
                {
                    case MoveableInstance mv when showMoveables:
                        DrawMoveable(cl, mv, level);
                        break;
                    case StaticInstance st when showStatics:
                        DrawStatic(cl, st, level);
                        break;
                    case ImportedGeometryInstance ig when showImportedGeometry:
                        DrawImported(cl, ig);
                        break;
                }
            }
        }
    }

    private void DrawMoveable(ICommandList cl, MoveableInstance instance, Level level)
    {
        var moveable = level.Settings?.WadTryGetMoveable(instance.WadObjectId);
        if (moveable == null) return;
        if (!_moveables.TryGetValue(moveable, out var mesh))
        {
            mesh = BuildMoveableMesh(moveable);
            _moveables[moveable] = mesh;
        }
        if (mesh.VertexCount == 0) return;
        DrawInstance(cl, mesh, instance.ObjectMatrix, new Vector4(1, 1, 1, 1));
    }

    private void DrawStatic(ICommandList cl, StaticInstance instance, Level level)
    {
        var staticObj = level.Settings?.WadTryGetStatic(instance.WadObjectId);
        if (staticObj?.Mesh == null) return;
        if (!_statics.TryGetValue(staticObj, out var mesh))
        {
            mesh = BuildStaticMesh(staticObj);
            _statics[staticObj] = mesh;
        }
        if (mesh.VertexCount == 0) return;

        var tint = new Vector4(instance.Color.X, instance.Color.Y, instance.Color.Z, 1f);
        DrawInstance(cl, mesh, instance.ObjectMatrix, tint);
    }

    private void DrawImported(ICommandList cl, ImportedGeometryInstance instance)
    {
        if (instance.Model?.DirectXModel == null) return;
        if (!_imported.TryGetValue(instance.Model, out var mesh))
        {
            mesh = BuildImportedMesh(instance.Model);
            _imported[instance.Model] = mesh;
        }
        if (mesh.VertexCount == 0) return;
        var tint = new Vector4(instance.Color.X, instance.Color.Y, instance.Color.Z, 1f);
        DrawInstance(cl, mesh, instance.RotationPositionMatrix * Matrix4x4.CreateScale(instance.Scale), tint);
    }

    private void DrawInstance(ICommandList cl, GpuMesh mesh, Matrix4x4 model, Vector4 tint)
    {
        var push = new ObjectPush { ModelMatrix = model, Tint = tint };
        unsafe
        {
            fixed (byte* p = _scratchPush)
                *(ObjectPush*)p = push;
        }
        cl.PushConstants(_scratchPush);

        _scratchVb[0] = new VertexBufferBinding(mesh.Vb, 0);
        cl.SetVertexBuffers(_scratchVb);
        cl.Draw(mesh.VertexCount);
    }

    // ------------------------------------------------------------ Mesh builders

    private GpuMesh BuildStaticMesh(WadStatic stat)
    {
        var list = new List<ObjectVertex>();
        AppendWadMesh(list, stat.Mesh, Vector3.Zero);
        return UploadMesh(list, "Static:" + stat.Id);
    }

    private GpuMesh BuildMoveableMesh(WadMoveable mv)
    {
        var list = new List<ObjectVertex>();
        // Default pose: walk the bone tree summing relative Translation.
        // WadBone.AbsoluteTranslation isn't populated by TombEditor's WAD
        // loader (only WadTool fills it), so doing it ourselves is required
        // — otherwise every bone draws at the model origin and the moveable
        // collapses to a single overlapping mesh blob.
        var accum = new Dictionary<WadBone, Vector3>(mv.Bones.Count);
        foreach (var bone in mv.Bones)
        {
            Vector3 parentT = bone.Parent != null && accum.TryGetValue(bone.Parent, out var p)
                              ? p
                              : Vector3.Zero;
            Vector3 myT = parentT + bone.Translation;
            accum[bone] = myT;
            if (bone.Mesh != null)
                AppendWadMesh(list, bone.Mesh, myT);
        }
        return UploadMesh(list, "Moveable:" + mv.Id);
    }

    private GpuMesh BuildImportedMesh(ImportedGeometry imp)
    {
        var list = new List<ObjectVertex>();
        if (imp.DirectXModel?.Meshes != null)
        {
            foreach (var mesh in imp.DirectXModel.Meshes)
            {
                if (mesh.Vertices == null || mesh.Indices == null) continue;
                bool hasColors = mesh.HasVertexColors;
                foreach (var submesh in mesh.Submeshes.Values)
                {
                    var igTex = submesh.Material?.Texture;
                    // ImportedGeometryVertex.UV is 0..1 normalized. The atlas
                    // wants pixel-space coordinates → multiply by source size.
                    Vector2 texSize = igTex?.Image is { Width: > 0, Height: > 0 }
                                      ? new Vector2(igTex.Image.Width, igTex.Image.Height)
                                      : Vector2.One;
                    int baseIdx = submesh.BaseIndex;
                    int count   = submesh.NumIndices;
                    for (int k = 0; k < count; k++)
                    {
                        int vi = mesh.Indices[baseIdx + k];
                        var v = mesh.Vertices[vi];
                        Vector3 col = hasColors ? v.Color : Vector3.One;
                        Vector2 atlasUv = _atlas.GetAtlasUv(igTex, v.UV * texSize);
                        list.Add(new ObjectVertex
                        {
                            Position = v.Position,
                            Color    = PackColorRgba8(col),
                            UvU      = PackUNorm16(atlasUv.X),
                            UvV      = PackUNorm16(atlasUv.Y),
                        });
                    }
                }
            }
        }
        return UploadMesh(list, "Imported:" + (imp.ToString() ?? "?"));
    }

    private void AppendWadMesh(List<ObjectVertex> verts, WadMesh mesh, Vector3 offset)
    {
        if (mesh == null) return;
        var pos = mesh.VertexPositions;
        var col = mesh.VertexColors;
        bool hasColors = col != null && col.Count == pos.Count;
        foreach (var poly in mesh.Polys)
        {
            var ta = poly.Texture;
            var sampleTex = ta.Texture;
            Vector2 uv0 = _atlas.GetAtlasUv(sampleTex, ta.TexCoord0);
            Vector2 uv1 = _atlas.GetAtlasUv(sampleTex, ta.TexCoord1);
            Vector2 uv2 = _atlas.GetAtlasUv(sampleTex, ta.TexCoord2);
            Vector2 uv3 = _atlas.GetAtlasUv(sampleTex, ta.TexCoord3);

            if (poly.Shape == WadPolygonShape.Triangle)
            {
                Push(poly.Index0, uv0); Push(poly.Index1, uv1); Push(poly.Index2, uv2);
            }
            else // Quad → two triangles
            {
                Push(poly.Index0, uv0); Push(poly.Index1, uv1); Push(poly.Index2, uv2);
                Push(poly.Index0, uv0); Push(poly.Index2, uv2); Push(poly.Index3, uv3);
            }
        }

        void Push(int i, Vector2 uv)
        {
            if (i < 0 || i >= pos.Count) return;
            Vector3 c = hasColors ? col[i] : Vector3.One;
            verts.Add(new ObjectVertex
            {
                Position = pos[i] + offset,
                Color    = PackColorRgba8(c),
                UvU      = PackUNorm16(uv.X),
                UvV      = PackUNorm16(uv.Y),
            });
        }
    }

    private static ushort PackUNorm16(float v) =>
        (ushort)Math.Clamp((int)(v * 65535f + 0.5f), 0, 65535);

    private GpuMesh UploadMesh(List<ObjectVertex> verts, string debugName)
    {
        if (verts.Count == 0) return new GpuMesh { Vb = default, VertexCount = 0 };
        var span = MemoryMarshal.AsBytes(verts.ToArray().AsSpan());
        var vb = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: span.Length,
                usage:     BufferUsage.Immutable,
                bindFlags: BufferBindFlags.Vertex,
                debugName: debugName),
            span);
        return new GpuMesh { Vb = vb, VertexCount = verts.Count };
    }

    private static uint PackColorRgba8(Vector3 c)
    {
        uint r = (uint)Math.Clamp((int)(c.X * 255f + 0.5f), 0, 255);
        uint g = (uint)Math.Clamp((int)(c.Y * 255f + 0.5f), 0, 255);
        uint b = (uint)Math.Clamp((int)(c.Z * 255f + 0.5f), 0, 255);
        return r | (g << 8) | (b << 16) | (0xFFu << 24);
    }

    public void Dispose()
    {
        InvalidateAll();
        if (_pipeline.IsValid) _device.Destroy(_pipeline);
    }
}
