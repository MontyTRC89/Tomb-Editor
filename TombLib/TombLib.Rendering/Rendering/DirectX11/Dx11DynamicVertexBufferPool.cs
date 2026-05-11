using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TombLib.Rendering.DirectX11
{
    // Ring-buffer pool of DYNAMIC vertex buffers used for transient per-frame geometry
    // (sprites, glyphs, debug lines). Replaces the old pattern of:
    //
    //     using (var vb = new Buffer(device, dataPtr, BufferDescription { Immutable, ... }))
    //     {
    //         context.IASetVertexBuffers(...);
    //         context.Draw(...);
    //     }
    //
    // which allocated, uploaded, registered with the DXGI tracking layer and disposed a
    // GPU resource for every single draw — measurable as a per-frame stutter when many
    // text labels or sprites are visible (room name overlays, light icons, etc.).
    //
    // How the ring works
    // ------------------
    // We keep a fixed-capacity ID3D11Buffer with CPU_ACCESS_WRITE and ResourceUsage.Dynamic.
    // For every Allocate(size) we:
    //   - if (cursor + size > capacity) wrap to 0 with MapMode.WriteDiscard (renames the
    //     resource so the GPU can keep using the previous contents)
    //   - otherwise map with MapMode.WriteNoOverwrite (no rename, GPU keeps reading)
    // The returned Slice exposes the writable IntPtr plus the byte offset that the caller
    // must pass to VertexBufferBinding so the IA reads from the correct region.
    //
    // Caller MUST Unmap() exactly once per Allocate(), before issuing the Draw().
    // The pattern is intentionally explicit (not RAII) to keep the hot path zero-alloc.
    //
    // Capacity sizing
    // ---------------
    // Default 4 MB is enough for ~170k textured vertices per frame at 24 bytes each.
    // Sprite/glyph batches in this editor stay well under that. If a single Allocate()
    // ever exceeds capacity we fall back to a one-shot Buffer (slow path, logged) rather
    // than asserting — text overlays should never silently fail.
    public sealed class Dx11DynamicVertexBufferPool : IDisposable
    {
        public readonly Buffer Buffer;
        public readonly int Capacity;

        private readonly DeviceContext _context;
        private int _cursor;

        public Dx11DynamicVertexBufferPool(Dx11RenderingDevice device, int capacityBytes = 4 * 1024 * 1024, string debugName = "DynamicVB")
        {
            _context = device.Context;
            Capacity = capacityBytes;
            Buffer = new Buffer(device.Device, new BufferDescription(
                capacityBytes,
                ResourceUsage.Dynamic,
                BindFlags.VertexBuffer,
                CpuAccessFlags.Write,
                ResourceOptionFlags.None,
                0));
            Buffer.SetDebugName(debugName);
        }

        public void Dispose() => Buffer.Dispose();

        // Reserves `size` bytes inside the ring and returns a writable slice. The returned
        // IntPtr stays valid only until Unmap() is called. The slice carries its own
        // byte offset because the IA needs it as the third arg of VertexBufferBinding.
        public Slice Allocate(int size)
        {
            if (size > Capacity)
                return AllocateOversized(size);

            MapMode mode;
            if (_cursor + size > Capacity)
            {
                _cursor = 0;
                mode = MapMode.WriteDiscard; // rename: previous contents may still be in flight, GPU keeps them
            }
            else
            {
                mode = MapMode.WriteNoOverwrite; // safe because cursor never overlaps regions still being read
            }

            int offset = _cursor;
            _cursor += size;

            DataBox box = _context.MapSubresource(Buffer, 0, mode, MapFlags.None);
            return new Slice(this, IntPtr.Add(box.DataPointer, offset), offset, size, oversized: null);
        }

        // Slow path for the rare batch larger than the ring capacity. We allocate a
        // throwaway IMMUTABLE buffer just for that draw — same cost as the old code,
        // but isolated from the steady state so the per-frame cost stays predictable.
        private Slice AllocateOversized(int size)
        {
            // Heap buffer to stage the data; caller writes through the returned IntPtr.
            // The GCHandle is kept alive until Unmap() builds the GPU resource.
            byte[] staging = new byte[size];
            GCHandle handle = GCHandle.Alloc(staging, GCHandleType.Pinned);
            return new Slice(this, handle.AddrOfPinnedObject(), 0, size,
                oversized: new OversizedState(handle, _context.Device));
        }

        public readonly struct Slice
        {
            public readonly Dx11DynamicVertexBufferPool Pool;
            public readonly IntPtr Data;
            public readonly int Offset; // byte offset inside Pool.Buffer (0 for oversized)
            public readonly int Size;
            internal readonly OversizedState Oversized;

            internal Slice(Dx11DynamicVertexBufferPool pool, IntPtr data, int offset, int size, OversizedState oversized)
            {
                Pool = pool;
                Data = data;
                Offset = offset;
                Size = size;
                Oversized = oversized;
            }

            // Returns the buffer the IA should bind. For ring slices that's the shared
            // dynamic buffer; for the oversized fallback it's a freshly built immutable.
            // The fallback buffer is owned by the slice and must be disposed via DisposeOversized().
            public Buffer Finish()
            {
                if (Oversized != null)
                    return Oversized.BuildAndRelease(Data, Size);
                Pool._context.UnmapSubresource(Pool.Buffer, 0);
                return Pool.Buffer;
            }
        }

        internal sealed class OversizedState
        {
            private GCHandle _handle;
            private readonly Device _device;
            public OversizedState(GCHandle handle, Device device)
            {
                _handle = handle;
                _device = device;
            }

            // Caller must Dispose() the returned buffer after the Draw().
            public Buffer BuildAndRelease(IntPtr data, int size)
            {
                try
                {
                    return new Buffer(_device, data, new BufferDescription(
                        size, ResourceUsage.Immutable, BindFlags.VertexBuffer,
                        CpuAccessFlags.None, ResourceOptionFlags.None, 0));
                }
                finally
                {
                    _handle.Free();
                }
            }
        }
    }
}
