using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib.LevelData;
using TombLib.RenderingV2.Backends.Dx11;
using TombLib.RenderingV2.Rhi;

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
    private readonly Dx11Device     _device;
    private SwapchainHandle         _swap;
    private int                     _width;
    private int                     _height;

    // Room geometry pass resources.
    private PipelineHandle          _roomPipeline;
    private BufferHandle            _viewCb;
    private readonly Dictionary<Room, RoomMesh> _roomMeshes = new();

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 256)]
    private struct ViewParams
    {
        public Matrix4x4 ViewProjection;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct RoomVertex
    {
        public Vector3 Position;
        public Vector3 Color;
    }

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

    public LevelRenderer(IntPtr hwnd, int width, int height)
    {
        _width  = Math.Max(1, width);
        _height = Math.Max(1, height);
        _device = new Dx11Device();

        _swap = _device.CreateSwapchain(new SwapchainDesc(
            hwnd:    hwnd,
            width:   _width,
            height:  _height,
            color:   Format.R8G8B8A8_UNorm,
            depth:   Format.D24_UNorm_S8_UInt,
            samples: 1,
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
                new VertexAttribute("COLOR",    0, Format.R32G32B32_Float, bufferSlot: 0, offset: 12),
            },
            VertexBufferLayouts    = new[] { new VertexBufferLayout(strideBytes: 24) },
            Topology               = PrimitiveTopology.TriangleList,
            // Rooms are authored with the camera meant to fly *inside* them,
            // so the outward-facing walls are back-facing from outside.
            // Disable culling for now — proper PVS / portal rendering comes later.
            Rasterizer             = new RasterizerState(CullMode.None),
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
        var vp = new ViewParams { ViewProjection = scene.ViewProjection };
        unsafe
        {
            var span = new ReadOnlySpan<byte>(&vp, sizeof(ViewParams));
            cl.UpdateBuffer(_viewCb, 0, span);
        }

        cl.BeginPass(new PassDesc
        {
            UseSwapchain   = true,
            Swapchain      = _swap,
            ColorLoadOps   = new[] { LoadOp.Clear },
            ColorStoreOps  = new[] { StoreOp.Store },
            DepthLoadOp    = LoadOp.Clear,
            DepthStoreOp   = StoreOp.Store,
            ClearColors    = new[] { new Vector4(0.08f, 0.08f, 0.12f, 1f) },
            ClearDepth     = 1.0f,
            ViewportWidth  = _width,
            ViewportHeight = _height,
        });

        if (scene.Level != null)
        {
            cl.SetPipeline(_roomPipeline);
            cl.SetBindings(new Bindings { ConstantBuffers = new[] { _viewCb } });

            foreach (Room room in scene.Level.Rooms)
            {
                if (room == null || room.RoomGeometry == null) continue;
                var mesh = GetOrCreateRoomMesh(room);
                if (mesh.VertexCount == 0) continue;
                cl.SetVertexBuffers(new[] { new VertexBufferBinding(mesh.Vb, 0) });
                cl.Draw(mesh.VertexCount);
            }
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

    private RoomMesh GetOrCreateRoomMesh(Room room)
    {
        if (_roomMeshes.TryGetValue(room, out var existing))
            return existing;
        var mesh = BuildRoomMesh(room);
        _roomMeshes[room] = mesh;
        return mesh;
    }

    /// <summary>Invalidate a single room's cached mesh (call on RoomGeometryChanged).</summary>
    public void InvalidateRoom(Room room)
    {
        if (_roomMeshes.Remove(room, out var mesh))
            _device.Destroy(mesh.Vb);
    }

    /// <summary>Drop all cached room meshes (call on LevelChanged).</summary>
    public void InvalidateAllRooms()
    {
        foreach (var m in _roomMeshes.Values)
            _device.Destroy(m.Vb);
        _roomMeshes.Clear();
    }

    private RoomMesh BuildRoomMesh(Room room)
    {
        var geom = room.RoomGeometry;
        int count = geom.VertexPositions.Count;
        if (count == 0)
            return new RoomMesh(default, 0);

        // World-space bake: room-local pos + room.WorldPos.
        Vector3 wp = room.WorldPos;
        var verts = new RoomVertex[count];
        for (int i = 0; i < count; i++)
        {
            verts[i].Position = geom.VertexPositions[i] + wp;
            verts[i].Color    = geom.VertexColors[i];
        }

        var bytes = MemoryMarshal.AsBytes(verts.AsSpan());
        var vb = _device.CreateBuffer(
            new BufferDesc(
                sizeBytes: bytes.Length,
                usage:     BufferUsage.Immutable,
                bindFlags: BufferBindFlags.Vertex,
                debugName: "Room:" + (room.Name ?? "?")),
            bytes);
        return new RoomMesh(vb, count);
    }

    public void Dispose()
    {
        _device.WaitIdle();
        InvalidateAllRooms();
        if (_viewCb.IsValid)        _device.Destroy(_viewCb);
        if (_roomPipeline.IsValid)  _device.Destroy(_roomPipeline);
        if (_swap.IsValid)          _device.Destroy(_swap);
        _device.Dispose();
    }
}
