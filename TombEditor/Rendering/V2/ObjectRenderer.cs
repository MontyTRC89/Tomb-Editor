using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib.LevelData;
using TombLib.RenderingV2.Rhi;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombEditor.Rendering.V2;

/// <summary>
/// Draws the rooms' object instances using GPU instancing. One draw call
/// per unique source asset (WadStatic / WadMoveable / ImportedGeometry)
/// with N instances each — irrespective of how many copies are placed
/// across visible rooms.
///
/// <para>Per-instance data (model matrix + tint) lives in a single dynamic
/// vertex buffer that's rewritten every frame; the per-asset vertex buffer
/// is immutable and cached.</para>
/// </summary>
internal sealed class ObjectRenderer : IDisposable
{
    private readonly IRhiDevice _device;
    private PipelineHandle      _pipeline;
    // Skybox pipeline = same Object shader but depth-test/write disabled,
    // so the horizon mesh acts as a background that room geometry paints
    // over naturally (no need for a mid-pass ClearDepth like the legacy did).
    private PipelineHandle      _skyboxPipeline;
    private BufferHandle        _skyboxInstanceVb; // single InstanceData per frame
    private TextureAtlas        _atlas;

    // Per-asset GPU mesh (immutable VB, mesh in default-pose model space).
    private readonly Dictionary<WadStatic,        GpuMesh> _statics    = new();
    private readonly Dictionary<WadMoveable,      GpuMesh> _moveables  = new();
    private readonly Dictionary<ImportedGeometry, GpuMesh> _imported   = new();

    // Per-frame instance gathering. Cleared at the top of Render().
    private readonly Dictionary<WadStatic,        List<InstanceData>> _staticBatch    = new();
    private readonly Dictionary<WadMoveable,      List<InstanceData>> _moveableBatch  = new();
    private readonly Dictionary<ImportedGeometry, List<InstanceData>> _importedBatch  = new();

    private BufferHandle _instanceVb;
    private int          _instanceCapacity = 4096;
    private byte[]       _instanceCpu;

    private readonly VertexBufferBinding[] _scratchVbs = new VertexBufferBinding[2];

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ObjectVertex
    {
        public Vector3 Position;
        public uint    Color;       // R8G8B8A8_UNorm
        public ushort  UvU;
        public ushort  UvV;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct InstanceData
    {
        public Matrix4x4 Model;     // TRANSPOSED before write — see Object.hlsl docs
        public Vector4   Tint;
    }
    private const int InstanceStride = 80;

    private sealed class GpuMesh : IDisposable
    {
        public BufferHandle Vb;
        public int          VertexCount;
        public void Dispose() { }
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
                // Slot 0 — per-vertex.
                new VertexAttribute("POSITION", 0, Format.R32G32B32_Float, bufferSlot: 0, offset: 0),
                new VertexAttribute("COLOR",    0, Format.R8G8B8A8_UNorm,  bufferSlot: 0, offset: 12),
                new VertexAttribute("TEXCOORD", 0, Format.R16G16_UNorm,    bufferSlot: 0, offset: 16),
                // Slot 1 — per-instance: 4×float4 model matrix + 1×float4 tint.
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
            DebugName              = "ObjectPipeline",
        });

        AllocInstanceBuffer(_instanceCapacity);

        // ---- Skybox pipeline: same vertex layout / shader, just no depth.
        _skyboxPipeline = device.CreatePipeline(new PipelineDesc
        {
            VertexShader   = vs,
            FragmentShader = ps,
            VertexAttributes = new[]
            {
                new VertexAttribute("POSITION", 0, Format.R32G32B32_Float,    bufferSlot: 0, offset: 0),
                new VertexAttribute("COLOR",    0, Format.R8G8B8A8_UNorm,     bufferSlot: 0, offset: 12),
                new VertexAttribute("TEXCOORD", 0, Format.R16G16_UNorm,       bufferSlot: 0, offset: 16),
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
            // CullMode.None — TR horizon meshes are typically inside-out
            // boxes/spheres; we don't want back-face culling to swallow them
            // regardless of which face the camera looks at.
            Rasterizer             = new RasterizerState(CullMode.None),
            DepthStencil           = DepthStencilState.Disabled,
            BlendStates            = new[] { BlendState.Opaque },
            ColorAttachmentFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthAttachmentFormat  = Format.D24_UNorm_S8_UInt,
            DebugName              = "ObjectSkyboxPipeline",
        });

        _skyboxInstanceVb = device.CreateBuffer(
            new BufferDesc(
                sizeBytes: InstanceStride,
                usage:     BufferUsage.DynamicVertex,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "SkyboxInstance"),
            ReadOnlySpan<byte>.Empty);
    }

    /// <summary>
    /// Draw the level's horizon (skybox) moveable at the camera position.
    /// Must be called inside an active pass, *before* room geometry — the
    /// skybox writes color with depth disabled so subsequent geometry
    /// naturally paints over it.
    /// </summary>
    public void RenderSkybox(ICommandList cl, Level level, TextureAtlas atlas, Vector3 camPos)
    {
        if (level == null || atlas == null) return;
        _atlas = atlas;

        var horizonId = WadMoveableId.GetHorizon(level.Settings.GameVersion);
        if (!horizonId.HasValue) return;
        var horizon = level.Settings?.WadTryGetMoveable(horizonId.Value);
        if (horizon == null) return;

        var mesh = GetOrBuildMoveable(horizon);
        if (mesh.VertexCount == 0) return;

        // Scale × Translate(camPos). Matches the legacy DrawSkybox transform.
        var model = Matrix4x4.CreateScale(128f) * Matrix4x4.CreateTranslation(camPos);
        var inst = new InstanceData
        {
            Model = Matrix4x4.Transpose(model),
            // alpha=1 → full-replace path: o.Color = white, so the horizon
            // shows its pure texture with no vertex-colour modulation — matches
            // the legacy DrawSkybox (ColoredVertices = false).
            Tint  = new Vector4(1f, 1f, 1f, 1f),
        };
        unsafe
        {
            var span = new ReadOnlySpan<byte>(&inst, InstanceStride);
            cl.UpdateBuffer(_skyboxInstanceVb, 0, span);
        }

        cl.SetPipeline(_skyboxPipeline);
        _scratchVbs[0] = new VertexBufferBinding(mesh.Vb, 0);
        _scratchVbs[1] = new VertexBufferBinding(_skyboxInstanceVb, 0);
        cl.SetVertexBuffers(_scratchVbs);
        cl.Draw(mesh.VertexCount, instanceCount: 1);
    }

    private void AllocInstanceBuffer(int capacity)
    {
        if (_instanceVb.IsValid) _device.Destroy(_instanceVb);
        _instanceCapacity = capacity;
        _instanceCpu = new byte[capacity * InstanceStride];
        _instanceVb = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: capacity * InstanceStride,
                usage:     BufferUsage.DynamicVertex,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "ObjectInstances"),
            ReadOnlySpan<byte>.Empty);
    }

    public void InvalidateAll()
    {
        foreach (var m in _statics.Values)   _device.Destroy(m.Vb);
        foreach (var m in _moveables.Values) _device.Destroy(m.Vb);
        foreach (var m in _imported.Values)  _device.Destroy(m.Vb);
        _statics.Clear();
        _moveables.Clear();
        _imported.Clear();
    }

    public void Render(ICommandList cl, IList<Room> visibleRooms, Level level, TextureAtlas atlas,
                       bool showMoveables, bool showStatics, bool showImportedGeometry,
                       HighlightedObjects highlighted = null, Vector4 selectionTint = default)
    {
        if (level == null || atlas == null) return;
        _atlas = atlas;

        // Tint convention (see Object.hlsl):
        //   .rgb = colour to apply, .a = replace factor in [0,1]
        //   a=0 → multiply (vertex.color × tint), used for the normal
        //         per-instance lighting tint.
        //   a=1 → full replace, used for the selection red — matches the
        //         legacy "output.Color = ColorSelection" overwrite that keeps
        //         selected dark meshes visible.
        Vector3 selRgb = selectionTint == default
                       ? new Vector3(1f, 0f, 0f)
                       : new Vector3(selectionTint.X, selectionTint.Y, selectionTint.Z);
        Vector4 selTint = new Vector4(selRgb, 1f);

        // ----- 1. Group instances by source asset.
        foreach (var v in _staticBatch.Values)   v.Clear();
        foreach (var v in _moveableBatch.Values) v.Clear();
        foreach (var v in _importedBatch.Values) v.Clear();

        foreach (var room in visibleRooms)
        {
            if (room?.Objects == null) continue;
            foreach (var obj in room.Objects)
            {
                bool isSelected = highlighted != null && highlighted.Contains(obj);
                switch (obj)
                {
                    case StaticInstance si when showStatics:
                        {
                            var s = level.Settings?.WadTryGetStatic(si.WadObjectId);
                            if (s?.Mesh == null) continue;
                            if (!_staticBatch.TryGetValue(s, out var list))
                                _staticBatch[s] = list = new List<InstanceData>();
                            list.Add(new InstanceData
                            {
                                Model = Matrix4x4.Transpose(si.ObjectMatrix),
                                Tint  = isSelected ? selTint
                                                   : new Vector4(si.Color.X, si.Color.Y, si.Color.Z, 0f),
                            });
                            break;
                        }
                    case MoveableInstance mi when showMoveables:
                        {
                            var mv = ResolveMoveable(level, mi.WadObjectId);
                            if (mv == null) continue;
                            if (!_moveableBatch.TryGetValue(mv, out var list))
                                _moveableBatch[mv] = list = new List<InstanceData>();
                            list.Add(new InstanceData
                            {
                                Model = Matrix4x4.Transpose(mi.ObjectMatrix),
                                Tint  = isSelected ? selTint : new Vector4(1, 1, 1, 0),
                            });
                            break;
                        }
                    case ImportedGeometryInstance ig when showImportedGeometry:
                        {
                            if (ig.Model?.DirectXModel == null) continue;
                            if (!_importedBatch.TryGetValue(ig.Model, out var list))
                                _importedBatch[ig.Model] = list = new List<InstanceData>();
                            var m = ig.RotationPositionMatrix * Matrix4x4.CreateScale(ig.Scale);
                            list.Add(new InstanceData
                            {
                                Model = Matrix4x4.Transpose(m),
                                Tint  = isSelected ? selTint
                                                   : new Vector4(ig.Color.X, ig.Color.Y, ig.Color.Z, 0f),
                            });
                            break;
                        }
                }
            }
        }

        // ----- 2. Pack everything into the dynamic instance VB.
        int totalInstances = 0;
        foreach (var v in _staticBatch.Values)   totalInstances += v.Count;
        foreach (var v in _moveableBatch.Values) totalInstances += v.Count;
        foreach (var v in _importedBatch.Values) totalInstances += v.Count;
        if (totalInstances == 0) return;

        if (totalInstances > _instanceCapacity)
            AllocInstanceBuffer(Math.Max(totalInstances, _instanceCapacity * 2));

        int writeOffset = 0;
        var groupOffsets = new List<(BufferHandle Vb, int VertexCount, int InstanceOffset, int InstanceCount)>(
            _staticBatch.Count + _moveableBatch.Count + _importedBatch.Count);

        var span = MemoryMarshal.Cast<byte, InstanceData>(_instanceCpu.AsSpan());

        foreach (var kv in _staticBatch)
        {
            var mesh = GetOrBuildStatic(kv.Key);
            if (mesh.VertexCount == 0 || kv.Value.Count == 0) continue;
            int baseOffset = writeOffset;
            for (int i = 0; i < kv.Value.Count; i++)
                span[(writeOffset / InstanceStride) + i] = kv.Value[i];
            writeOffset += kv.Value.Count * InstanceStride;
            groupOffsets.Add((mesh.Vb, mesh.VertexCount, baseOffset, kv.Value.Count));
        }
        foreach (var kv in _moveableBatch)
        {
            var mesh = GetOrBuildMoveable(kv.Key);
            if (mesh.VertexCount == 0 || kv.Value.Count == 0) continue;
            int baseOffset = writeOffset;
            for (int i = 0; i < kv.Value.Count; i++)
                span[(writeOffset / InstanceStride) + i] = kv.Value[i];
            writeOffset += kv.Value.Count * InstanceStride;
            groupOffsets.Add((mesh.Vb, mesh.VertexCount, baseOffset, kv.Value.Count));
        }
        foreach (var kv in _importedBatch)
        {
            var mesh = GetOrBuildImported(kv.Key);
            if (mesh.VertexCount == 0 || kv.Value.Count == 0) continue;
            int baseOffset = writeOffset;
            for (int i = 0; i < kv.Value.Count; i++)
                span[(writeOffset / InstanceStride) + i] = kv.Value[i];
            writeOffset += kv.Value.Count * InstanceStride;
            groupOffsets.Add((mesh.Vb, mesh.VertexCount, baseOffset, kv.Value.Count));
        }

        if (writeOffset == 0) return;
        cl.UpdateBuffer(_instanceVb, 0, new ReadOnlySpan<byte>(_instanceCpu, 0, writeOffset));

        // ----- 3. Bind pipeline and draw each group.
        cl.SetPipeline(_pipeline);
        foreach (var g in groupOffsets)
        {
            _scratchVbs[0] = new VertexBufferBinding(g.Vb, 0);
            _scratchVbs[1] = new VertexBufferBinding(_instanceVb, g.InstanceOffset);
            cl.SetVertexBuffers(_scratchVbs);
            cl.Draw(g.VertexCount, g.InstanceCount);
        }
    }

    // Resolve the WadMoveable to actually draw for an instance. Lara (id 0)
    // is drawn with the LARA_SKIN object's meshes when that object exists and
    // its mesh count matches — same rule as the legacy DrawMoveables.
    private static WadMoveable ResolveMoveable(Level level, WadMoveableId id)
    {
        var mv = level.Settings?.WadTryGetMoveable(id);
        if (mv == null) return null;

        if (id == WadMoveableId.Lara)
        {
            uint skinTypeId = TrCatalog.GetMoveableSkin(level.Settings.GameVersion, id.TypeId);
            var skin = level.Settings.WadTryGetMoveable(new WadMoveableId(skinTypeId));
            if (skin != null && skin.Meshes.Count == mv.Meshes.Count)
                return skin;
        }
        return mv;
    }

    // ----------------------------------------------------------- Mesh cache

    private GpuMesh GetOrBuildStatic(WadStatic s)
    {
        if (_statics.TryGetValue(s, out var m)) return m;
        m = BuildStaticMesh(s);
        _statics[s] = m;
        return m;
    }
    private GpuMesh GetOrBuildMoveable(WadMoveable mv)
    {
        if (_moveables.TryGetValue(mv, out var m)) return m;
        m = BuildMoveableMesh(mv);
        _moveables[mv] = m;
        return m;
    }
    private GpuMesh GetOrBuildImported(ImportedGeometry imp)
    {
        if (_imported.TryGetValue(imp, out var m)) return m;
        m = BuildImportedMesh(imp);
        _imported[imp] = m;
        return m;
    }

    // ------------------------------------------------------------ Mesh build

    private GpuMesh BuildStaticMesh(WadStatic stat)
    {
        var list = new List<ObjectVertex>();
        AppendWadMesh(list, stat.Mesh, Matrix4x4.Identity);
        return UploadMesh(list, "Static:" + stat.Id);
    }

    private GpuMesh BuildMoveableMesh(WadMoveable mv)
    {
        var list = new List<ObjectVertex>();
        WadKeyFrame frame = (mv.Animations.Count > 0 && mv.Animations[0].KeyFrames.Count > 0)
                             ? mv.Animations[0].KeyFrames[0]
                             : null;
        var transforms = WadMoveablePoseV2.ComputeBoneTransforms(mv, frame);
        for (int i = 0; i < mv.Bones.Count; i++)
        {
            var bone = mv.Bones[i];
            if (bone.Mesh != null) AppendWadMesh(list, bone.Mesh, transforms[i]);
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

    private void AppendWadMesh(List<ObjectVertex> verts, WadMesh mesh, Matrix4x4 transform)
    {
        if (mesh == null) return;
        var pos = mesh.VertexPositions;
        var col = mesh.VertexColors;
        bool hasColors = col != null && col.Count == pos.Count;
        foreach (var poly in mesh.Polys)
        {
            var ta = poly.Texture;
            var sampleTex = ta.Texture;
            uint bm = (uint)ta.BlendMode;   // per-face blend mode → vertex alpha
            Vector2 uv0 = _atlas.GetAtlasUv(sampleTex, ta.TexCoord0);
            Vector2 uv1 = _atlas.GetAtlasUv(sampleTex, ta.TexCoord1);
            Vector2 uv2 = _atlas.GetAtlasUv(sampleTex, ta.TexCoord2);
            Vector2 uv3 = _atlas.GetAtlasUv(sampleTex, ta.TexCoord3);

            if (poly.Shape == WadPolygonShape.Triangle)
            {
                Push(poly.Index0, uv0, bm); Push(poly.Index1, uv1, bm); Push(poly.Index2, uv2, bm);
            }
            else
            {
                Push(poly.Index0, uv0, bm); Push(poly.Index1, uv1, bm); Push(poly.Index2, uv2, bm);
                Push(poly.Index0, uv0, bm); Push(poly.Index2, uv2, bm); Push(poly.Index3, uv3, bm);
            }
        }

        void Push(int i, Vector2 uv, uint blendMode)
        {
            if (i < 0 || i >= pos.Count) return;
            Vector3 c = hasColors ? col[i] : Vector3.One;
            verts.Add(new ObjectVertex
            {
                Position = Vector3.Transform(pos[i], transform),
                Color    = PackColor(c, blendMode),
                UvU      = PackUNorm16(uv.X),
                UvV      = PackUNorm16(uv.Y),
            });
        }
    }

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

    private static uint PackColorRgba8(Vector3 c) => PackColor(c, 0xFFu);

    // Packs an RGB tint plus an 8-bit alpha payload — for WAD meshes the alpha
    // byte carries the face BlendMode so the shader can alpha-test cutouts.
    private static uint PackColor(Vector3 c, uint alphaByte)
    {
        uint r = (uint)Math.Clamp((int)(c.X * 255f + 0.5f), 0, 255);
        uint g = (uint)Math.Clamp((int)(c.Y * 255f + 0.5f), 0, 255);
        uint b = (uint)Math.Clamp((int)(c.Z * 255f + 0.5f), 0, 255);
        return r | (g << 8) | (b << 16) | ((alphaByte & 0xFFu) << 24);
    }

    private static ushort PackUNorm16(float v) =>
        (ushort)Math.Clamp((int)(v * 65535f + 0.5f), 0, 65535);

    public void Dispose()
    {
        InvalidateAll();
        if (_instanceVb.IsValid)       _device.Destroy(_instanceVb);
        if (_skyboxInstanceVb.IsValid) _device.Destroy(_skyboxInstanceVb);
        if (_pipeline.IsValid)         _device.Destroy(_pipeline);
        if (_skyboxPipeline.IsValid)   _device.Destroy(_skyboxPipeline);
    }
}
