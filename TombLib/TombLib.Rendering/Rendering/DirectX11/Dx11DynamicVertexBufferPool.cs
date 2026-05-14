using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System;
using System.Runtime.InteropServices;
using D3D11Usage = Silk.NET.Direct3D11.Usage;

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
    public sealed unsafe class Dx11DynamicVertexBufferPool : IDisposable
    {
        public readonly ID3D11Buffer* Buffer;
        public readonly int Capacity;

        private readonly ID3D11DeviceContext* _context;
        private readonly ID3D11Device* _device;
        private int _cursor;

        public Dx11DynamicVertexBufferPool(Dx11RenderingDevice device, int capacityBytes = 4 * 1024 * 1024, string debugName = "DynamicVB")
        {
            _context = device.Context;
            _device = device.Device;
            Capacity = capacityBytes;

            var desc = new BufferDesc
            {
                ByteWidth = (uint)capacityBytes,
                Usage = D3D11Usage.Dynamic,
                BindFlags = (uint)BindFlag.VertexBuffer,
                CPUAccessFlags = (uint)CpuAccessFlag.Write,
                MiscFlags = 0,
                StructureByteStride = 0,
            };
            ID3D11Buffer* buf;
            SilkMarshal.ThrowHResult(_device->CreateBuffer(&desc, null, &buf));
            Buffer = buf;
            Dx11RenderingDevice.SetDebugName((ID3D11DeviceChild*)Buffer, debugName);
        }

        public void Dispose() => Buffer->Release();

        // Reserves `size` bytes inside the ring and returns a writable slice. The returned
        // IntPtr stays valid only until Unmap() is called. The slice carries its own
        // byte offset because the IA needs it as the third arg of VertexBufferBinding.
        public Slice Allocate(int size)
        {
            if (size > Capacity)
                return AllocateOversized(size);

            Map mode;
            if (_cursor + size > Capacity)
            {
                _cursor = 0;
                mode = Map.WriteDiscard; // rename: previous contents may still be in flight, GPU keeps them
            }
            else
            {
                mode = Map.WriteNoOverwrite; // safe because cursor never overlaps regions still being read
            }

            int offset = _cursor;
            _cursor += size;

            MappedSubresource mapped;
            SilkMarshal.ThrowHResult(
                _context->Map((ID3D11Resource*)Buffer, 0, mode, 0, &mapped));
            return new Slice(this, IntPtr.Add((IntPtr)mapped.PData, offset), offset, size, oversized: null);
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
                oversized: new OversizedState(handle, _device));
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
            // The fallback buffer is owned by the slice and must be released via the caller.
            public ID3D11Buffer* Finish()
            {
                if (Oversized != null)
                    return Oversized.BuildAndRelease(Data, Size);
                Pool._context->Unmap((ID3D11Resource*)Pool.Buffer, 0);
                return Pool.Buffer;
            }
        }

        internal sealed class OversizedState
        {
            private GCHandle _handle;
            private readonly ID3D11Device* _device;
            public OversizedState(GCHandle handle, ID3D11Device* device)
            {
                _handle = handle;
                _device = device;
            }

            // Caller must Release() the returned buffer after the Draw().
            public ID3D11Buffer* BuildAndRelease(IntPtr data, int size)
            {
                try
                {
                    var desc = new BufferDesc
                    {
                        ByteWidth = (uint)size,
                        Usage = D3D11Usage.Immutable,
                        BindFlags = (uint)BindFlag.VertexBuffer,
                        CPUAccessFlags = 0,
                        MiscFlags = 0,
                        StructureByteStride = 0,
                    };
                    var subresource = new SubresourceData
                    {
                        PSysMem = (void*)data,
                        SysMemPitch = 0,
                        SysMemSlicePitch = 0,
                    };
                    ID3D11Buffer* buf;
                    SilkMarshal.ThrowHResult(_device->CreateBuffer(&desc, &subresource, &buf));
                    return buf;
                }
                finally
                {
                    _handle.Free();
                }
            }
        }
    }
}
