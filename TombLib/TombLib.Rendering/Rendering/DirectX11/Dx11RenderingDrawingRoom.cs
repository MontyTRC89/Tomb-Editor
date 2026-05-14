using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorStructs;
using TombLib.Utils;
using D3D11Usage = Silk.NET.Direct3D11.Usage;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace TombLib.Rendering.DirectX11
{
    // Pre-baked GPU geometry for one Room. Created on demand by Panel3D and cached
    // (one entry per Room) until something invalidates the room (geometry edit, sector
    // texture change, light change, portal modification on a neighbour, etc.).
    //
    // Memory layout
    // -------------
    // The vertex buffer is IMMUTABLE and holds 5 interleaved-but-separately-bound
    // streams in a single allocation. SoA layout is used because (a) it keeps every
    // vertex small and cache-aligned and (b) the room shader's input layout uses
    // distinct slots per attribute, so binding offsets work out cleanly.
    //
    //   stream 0  POSITION   float3   12 B
    //   stream 1  COLOR      uint     4 B   (CompressColor — premultiplied vertex tint)
    //   stream 2  OVERLAY    uint     4 B   (sector overlay color + 0.4 alpha for hidden rooms)
    //   stream 3  UVW+BLEND  ulong    8 B   (CompressUvw — atlas X,Y,page,blendMode)
    //   stream 4  EDITOR_UV  uint     4 B   (low bits: per-vertex editor-grid UV;
    //                                         high bits: sector texture / highlight flags)
    // Total: 32 B/vertex, vs. 60+ in a naive layout.
    //
    // Geometry layout
    // ---------------
    // singleSidedVertexCount = roomGeometry.VertexPositions.Count (one triangle = 3 verts)
    // Past that, double-sided triangles are duplicated with reversed winding so we don't
    // need rasterizer-state changes mid-room. The duplicated verts share atlas UVs but
    // get the back-face winding via reversed vertex order (i*3+2, +1, +0).
    //
    // Texture lifecycle
    // -----------------
    // The atlas texture allocator can garbage-collect at any time (when it's full).
    // GarbageCollectTexture() is registered as a callback that:
    //   1) reads back our entire VB into RAM (immutable buffers can't be partially updated),
    //   2) reports which atlas entries we still reference (for the GC to keep alive),
    //   3) returns an "adjust" delegate that the allocator calls AFTER the GC pass with the
    //      new atlas Map, so we can rewrite all UVWs and rebuild the immutable buffer.
    // The TexturesInvalidated/TexturesInvalidatedRetried pair handles the case where an
    // atlas allocation fails MID-construction; we restart texturing once. If it fails
    // twice we accept some textures will render as "unavailable".
    public unsafe class Dx11RenderingDrawingRoom : RenderingDrawingRoom
    {
        public readonly Dx11RenderingDevice Device;
        public readonly ID3D11ShaderResourceView* TextureView;
        public readonly RenderingTextureAllocator TextureAllocator;
        public ID3D11Buffer* VertexBuffer;
        public readonly Dx11VertexBufferBinding[] VertexBufferBindings;
        public readonly int VertexCount;
        public readonly int VertexBufferSize;
        public bool TexturesInvalidated = false;
        public bool TexturesInvalidatedRetried = false;

        public Dx11RenderingDrawingRoom(Dx11RenderingDevice device, Description description)
        {
            Device = device;
            TextureView = ((Dx11RenderingTextureAllocator)(description.TextureAllocator)).TextureView;
            TextureAllocator = description.TextureAllocator;

            // Reciprocal of atlas size, baked into the UV packing format. The 16777216
            // constant is 2^24 — see CompressUvw for layout.
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(TextureAllocator.Size.X, TextureAllocator.Size.Y);

            RoomGeometry roomGeometry = description.Room.RoomGeometry;
            // TombEngine allows >256 px texture spans across a triangle (out-of-bounds
            // marker shows on classic engines for this case).
            float maxTexCoordSpan = description.Room.Level?.IsTombEngine == true ? 1024.0f : 256.0f;

            Vector3 worldPos = description.Room.WorldPos + description.Offset;
            int singleSidedVertexCount = roomGeometry.VertexPositions.Count;
            int vertexCount = VertexCount = singleSidedVertexCount + roomGeometry.DoubleSidedTriangleCount * 3;
            if (vertexCount == 0)
                return;

            // Total bytes for the SoA layout described in the class comment.
            VertexBufferSize = vertexCount * (sizeof(Vector3) + sizeof(uint) + sizeof(uint) + sizeof(ulong) + sizeof(uint));
            // `fixed (byte* data = new byte[N])` lets us treat the heap-allocated buffer
            // as a stack-pinned region just for the duration of the scope. The `fixed`
            // here is what allows the unsafe pointer arithmetic below; it is NOT a stack
            // allocation (the array is on the GC heap).
            fixed (byte* data = new byte[VertexBufferSize])
            {
                // Pointers into the 5 contiguous streams. Layout is SoA: stream N starts
                // right after stream N-1 ends, with each stream sized vertexCount * elemSize.
                Vector3* positions               = (Vector3*)(data);
                uint*    colors                  = (uint*)   (data + vertexCount * sizeof(Vector3));
                uint*    overlays                = (uint*)   (data + vertexCount * (sizeof(Vector3) + sizeof(uint)));
                ulong*   uvwAndBlendModes        = (ulong*)  (data + vertexCount * (sizeof(Vector3) + sizeof(uint) + sizeof(uint)));
                uint*    editorUVAndSectorTexture = (uint*)  (data + vertexCount * (sizeof(Vector3) + sizeof(uint) + sizeof(uint) + sizeof(ulong)));

                // Pass 1: per-vertex attributes that don't depend on the triangle's face.
                // World-space position is baked here so the shader has no model matrix.
                for (int i = 0; i < singleSidedVertexCount; ++i)
                    positions[i] = roomGeometry.VertexPositions[i] + worldPos;
                for (int i = 0; i < singleSidedVertexCount; ++i)
                    colors[i] = Dx11RenderingDevice.CompressColor(roomGeometry.VertexColors[i]);
                for (int i = 0; i < singleSidedVertexCount; ++i)
                {
                    // Editor UVs are in {0, 1, 2, 3} per axis (corner indices). Pack into
                    // 4 bits — the next loop will OR sector flags into the upper 28 bits.
                    Vector2 vertexEditorUv = roomGeometry.VertexEditorUVs[i];
                    uint editorUv = 0;
                    editorUv |= (uint)((int)vertexEditorUv.X) & 3;
                    editorUv |= ((uint)((int)vertexEditorUv.Y) & 3) << 2;
                    editorUVAndSectorTexture[i] = editorUv;
                }

                // Pass 2: per-triangle face attributes (sector overlay color, highlight,
                // dim, selection, sector arrow texture). Run as a loop with a 1-deep
                // cache (lastFaceIdentity) because adjacent triangles in roomGeometry
                // typically belong to the same SectorFace, so we'd recompute the same
                // SectorTextureGet result over and over.
                //
                // editorUVAndSectorTexture bit layout (after this pass):
                //   bits 0..3  : per-vertex editor UV (set in pass 1)
                //   bit  4     : highlighted face
                //   bit  5     : dimmed face
                //   bit  6     : has sector overlay texture (vs. solid color)
                //   bit  7     : selected & textured (draws extra outline)
                //   bits 8..15 : sector texture index (when bit 6 is set)
                //              | OR red channel of sector color (when bit 6 is clear)
                //   bits 16..23: green channel of sector color
                //   bits 24..31: blue channel of sector color
                {
                    SectorFaceIdentity lastFaceIdentity = new SectorFaceIdentity(-1, -1, SectorFace.Floor);
                    uint lastSectorTexture = 0;
                    uint overlay = 0;
                    for (int i = 0, triangleCount = singleSidedVertexCount / 3; i < triangleCount; ++i)
                    {
                        SectorFaceIdentity currentFaceIdentity = roomGeometry.TriangleSectorInfo[i];
                        if (!lastFaceIdentity.Equals(currentFaceIdentity))
                        {
                            SectorTextureResult result = description.SectorTextureGet(description.Room, currentFaceIdentity.Position.X, currentFaceIdentity.Position.Y, currentFaceIdentity.Face);

                            lastFaceIdentity = currentFaceIdentity;
                            lastSectorTexture = 0;
                            if (result.SectorTexture != SectorTexture.None)
                            { // Use sector texture
                                lastSectorTexture = 0x40 | (((uint)result.SectorTexture - 1) << 8);
                            }
                            else
                            { // Use sector color
                                lastSectorTexture =
                                    (((uint)(result.Color.X * 255)) << 8) |
                                    (((uint)(result.Color.Y * 255)) << 16) |
                                    (((uint)(result.Color.Z * 255)) << 24);
                            }
                            // Highlight / dim sectors
                            if (result.Highlighted) lastSectorTexture |= 0x10;
                            if (result.Dimmed)      lastSectorTexture |= 0x20;
                            // Indicate selected textured faces
                            if (result.Selected && roomGeometry.TriangleTextureAreas[i].Texture != null)
                                lastSectorTexture |= 0x80;

                            // Assign overlay color which will be used in geometry mode if face has service texture (e.g. arrows)
                            overlay = Dx11RenderingDevice.CompressColor(new Vector3(result.Overlay.X, result.Overlay.Y, result.Overlay.Z), (result.Hidden ? 0.4f : 1.0f), false);
                        }
                        editorUVAndSectorTexture[i * 3 + 0] |= lastSectorTexture;
                        editorUVAndSectorTexture[i * 3 + 1] |= lastSectorTexture;
                        editorUVAndSectorTexture[i * 3 + 2] |= lastSectorTexture;

                        overlays[i * 3 + 0] = overlay;
                        overlays[i * 3 + 1] = overlay;
                        overlays[i * 3 + 2] = overlay;
                    }
                }

                // Pass 3: per-triangle texture allocation. Goto-based retry is here for
                // a specific corner case: if the atlas runs out of room MID-pass, the
                // allocator triggers a GC that may invalidate previously-resolved entries
                // (TexturesInvalidated flag). We restart the entire pass once. Two retries
                // would mean the atlas is genuinely too small — accept some unavailable
                // markers and move on (TexturesInvalidatedRetried gates the second retry).
                //
                // uvwAndBlendModes sentinel values (matched in shader and by GC adjust):
                //   < 0x1000000 (== 1ul<<24): non-textured (geometry render or invisible)
                //                             — GC pass skips these to avoid bogus lookups
                //   anything else            : real packed atlas UVW
                RetryTexturing:
                ;
                {
                    int doubleSidedVertexIndex = singleSidedVertexCount;
                    for (int i = 0, triangleCount = singleSidedVertexCount / 3; i < triangleCount; ++i)
                    {
                        TextureArea texture = roomGeometry.TriangleTextureAreas[i];

                        if (texture.Texture == null)
                        {
                            // Untextured triangle: shader switches to "geometric view"
                            // and uses the sector overlay color from editorUVAndSectorTexture.
                            uvwAndBlendModes[i * 3 + 0] = 1ul << 24;
                            uvwAndBlendModes[i * 3 + 1] = 1ul << 24;
                            uvwAndBlendModes[i * 3 + 2] = 1ul << 24;
                        }
                        else if (texture.Texture is TextureInvisible)
                        {
                            // Invisible texture: shader discards the fragment.
                            uvwAndBlendModes[i * 3 + 0] = 0ul << 24;
                            uvwAndBlendModes[i * 3 + 1] = 0ul << 24;
                            uvwAndBlendModes[i * 3 + 2] = 0ul << 24;
                        }
                        else
                        {
                            // Textured triangle. Three failure modes are recoverable
                            // (unavailable file / out-of-bounds in either of two ways)
                            // and replaced with a diagnostic placeholder texture so the
                            // user can spot the problem in the viewport.
                            if (texture.Texture.IsUnavailable)
                            { // Texture is unvailable (i.e. file couldn't be loaded.
                                ImageC image = Dx11RenderingDevice.TextureUnavailable;
                                VectorInt3 position = TextureAllocator.Get(image);
                                uvwAndBlendModes[i * 3 + 0] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 0]) * (image.Size - VectorInt2.One) + new Vector2(0.5f), (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 1] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 1]) * (image.Size - VectorInt2.One) + new Vector2(0.5f), (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 2] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 2]) * (image.Size - VectorInt2.One) + new Vector2(0.5f), (uint)texture.BlendMode);
                            }
                            else if (texture.AreTriangleCoordsOutOfBounds(maxTexCoordSpan))
                            { // Texture is available but coordinates are out of bounds
                                ImageC image = Dx11RenderingDevice.TextureCoordOutOfBounds;
                                VectorInt3 position = TextureAllocator.Get(image);
                                uvwAndBlendModes[i * 3 + 0] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 0]) * (image.Size - VectorInt2.One) + new Vector2(0.5f), (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 1] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 1]) * (image.Size - VectorInt2.One) + new Vector2(0.5f), (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 2] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 2]) * (image.Size - VectorInt2.One) + new Vector2(0.5f), (uint)texture.BlendMode);
                            }
                            else if (!texture.ParentArea.IsZero && !texture.ParentArea.Intersects(texture.GetRect()))
                            { // Texture is available but coordinates are ouf of bounds
                                ImageC image = Dx11RenderingDevice.TextureCoordOutOfBounds;
                                VectorInt3 position = TextureAllocator.Get(image);
                                uvwAndBlendModes[i * 3 + 0] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 0]) * (image.Size - VectorInt2.One) + new Vector2(0.5f), (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 1] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 1]) * (image.Size - VectorInt2.One) + new Vector2(0.5f), (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 2] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 2]) * (image.Size - VectorInt2.One) + new Vector2(0.5f), (uint)texture.BlendMode);
                            }
                            else
                            { // Texture is available
                                VectorInt3 position = TextureAllocator.GetForTriangle(texture);
                                uvwAndBlendModes[i * 3 + 0] = Dx11RenderingDevice.CompressUvw(position, textureScaling, texture.TexCoord0, (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 1] = Dx11RenderingDevice.CompressUvw(position, textureScaling, texture.TexCoord1, (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 2] = Dx11RenderingDevice.CompressUvw(position, textureScaling, texture.TexCoord2, (uint)texture.BlendMode);
                            }

                            // Double-sided faces: instead of toggling rasterizer state,
                            // emit a second triangle with reversed winding (verts in
                            // order 2, 1, 0) at the tail of the buffer. All other
                            // attributes are copied verbatim. doubleSidedVertexIndex is
                            // monotonically advanced and verified at the end of the loop.
                            if (texture.DoubleSided)
                            {
                                positions[doubleSidedVertexIndex] = positions[i * 3 + 2];
                                colors[doubleSidedVertexIndex] = colors[i * 3 + 2];
                                overlays[doubleSidedVertexIndex] = overlays[i * 3 + 2];
                                uvwAndBlendModes[doubleSidedVertexIndex] = uvwAndBlendModes[i * 3 + 2];
                                editorUVAndSectorTexture[doubleSidedVertexIndex++] = editorUVAndSectorTexture[i * 3 + 2];

                                positions[doubleSidedVertexIndex] = positions[i * 3 + 1];
                                colors[doubleSidedVertexIndex] = colors[i * 3 + 1];
                                overlays[doubleSidedVertexIndex] = overlays[i * 3 + 1];
                                uvwAndBlendModes[doubleSidedVertexIndex] = uvwAndBlendModes[i * 3 + 1];
                                editorUVAndSectorTexture[doubleSidedVertexIndex++] = editorUVAndSectorTexture[i * 3 + 1];

                                positions[doubleSidedVertexIndex] = positions[i * 3 + 0];
                                colors[doubleSidedVertexIndex] = colors[i * 3 + 0];
                                overlays[doubleSidedVertexIndex] = overlays[i * 3 + 0];
                                uvwAndBlendModes[doubleSidedVertexIndex] = uvwAndBlendModes[i * 3 + 0];
                                editorUVAndSectorTexture[doubleSidedVertexIndex++] = editorUVAndSectorTexture[i * 3 + 0];
                            }
                        }
                    }
                    if (doubleSidedVertexIndex != vertexCount)
                        throw new ArgumentException("Double sided triangle count of RoomGeometry is wrong!");

                    // Retry texturing once at max
                    if (TexturesInvalidated && !TexturesInvalidatedRetried)
                    {
                        TexturesInvalidatedRetried = true;
                        goto RetryTexturing;
                    }
                }

                // Create GPU resources
                var desc = new BufferDesc
                {
                    ByteWidth = (uint)VertexBufferSize,
                    Usage = D3D11Usage.Immutable,
                    BindFlags = (uint)BindFlag.VertexBuffer,
                    CPUAccessFlags = 0,
                    MiscFlags = 0,
                    StructureByteStride = 0,
                };
                var subresData = new SubresourceData
                {
                    PSysMem = data,
                };
                ID3D11Buffer* buf;
                SilkMarshal.ThrowHResult(device.Device->CreateBuffer(&desc, &subresData, &buf));
                VertexBuffer = buf;

                VertexBufferBindings = new Dx11VertexBufferBinding[] {
                    new Dx11VertexBufferBinding(VertexBuffer, sizeof(Vector3), (int)((byte*)positions - data)),
                    new Dx11VertexBufferBinding(VertexBuffer, sizeof(uint), (int)((byte*)colors - data)),
                    new Dx11VertexBufferBinding(VertexBuffer, sizeof(uint), (int)((byte*)overlays - data)),
                    new Dx11VertexBufferBinding(VertexBuffer, sizeof(ulong), (int)((byte*)uvwAndBlendModes - data)),
                    new Dx11VertexBufferBinding(VertexBuffer, sizeof(uint), (int)((byte*)editorUVAndSectorTexture - data))
                };
                Dx11RenderingDevice.SetDebugName((ID3D11DeviceChild*)VertexBuffer, "Room " + (description.Room.Name ?? ""));
            }
            TextureAllocator.GarbageCollectionCollectEvent.Add(GarbageCollectTexture);
        }

        public override void Dispose()
        {
            TextureAllocator.GarbageCollectionCollectEvent.Remove(GarbageCollectTexture);
            if (VertexBuffer != null)
                VertexBuffer->Release();
        }

        // Two-phase texture-allocator GC participation. Phase 1 (this method) is called
        // BEFORE the atlas is rebuilt: we read back our VB, walk the UVW stream and
        // report which atlas entries we still reference (inOutUsedTextures). Returning
        // a non-null adjust delegate causes phase 2 to be invoked AFTER the rebuild,
        // with the new atlas Map; we use it to remap UVWs and rebuild our (immutable) VB.
        //
        // Why a readback: vertex buffers are created with ResourceUsage.Immutable so we
        // can't update them in place. Dx11RenderingDevice.ReadBuffer copies the GPU
        // resource into a staging buffer and back into a managed byte[].
        public RenderingTextureAllocator.GarbageCollectionAdjustDelegate GarbageCollectTexture(RenderingTextureAllocator allocator,
            RenderingTextureAllocator.Map map, HashSet<RenderingTextureAllocator.Map.Entry> inOutUsedTextures)
        {
            TexturesInvalidated = true;
            if (VertexBuffer == null)
                return null;

            byte[] data = Device.ReadBuffer(VertexBuffer, VertexBufferSize);
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(TextureAllocator.Size.X, TextureAllocator.Size.Y);
            int uvwAndBlendModesOffset = (int)VertexBufferBindings[3].Offset;

            fixed (byte* dataPtr = data)
            {
                ulong* uvwAndBlendModesPtr = (ulong*)(dataPtr + uvwAndBlendModesOffset);
                for (int i = 0; i < VertexCount; ++i)
                {
                    // 0x1000000 == 1ul<<24, the sentinel for "geometry / invisible".
                    // Skipping these avoids feeding bogus UVs into Map.Lookup.
                    if (uvwAndBlendModesPtr[i] < 0x1000000)
                        continue;
                    var texture = map.Lookup(Dx11RenderingDevice.UncompressUvw(uvwAndBlendModesPtr[i], textureScaling));
                    if (texture == null)
#if DEBUG
                        throw new ArgumentOutOfRangeException("Texture unrecognized.");
#else
                        continue;
#endif
                    inOutUsedTextures.Add(texture);
                }
            }

            // Provide a methode to update the buffer with new UV coordinates
            return delegate (RenderingTextureAllocator allocator2, RenderingTextureAllocator.Map map2)
            {
                if (allocator2 == null || map2 == null)
                    return;

                Vector2 textureScaling2 = new Vector2(16777216.0f) / new Vector2(TextureAllocator.Size.X, TextureAllocator.Size.Y);

                // Update data
                fixed (byte* dataPtr = data)
                {
                    ulong* uvwAndBlendModesPtr = (ulong*)(dataPtr + uvwAndBlendModesOffset);
                    for (int i = 0; i < VertexCount; ++i)
                    {
                        if (uvwAndBlendModesPtr[i] < 0x1000000) // Very small coordinates make no sense, they are used as a placeholder
                            continue;
                        var texture = map2.Lookup(Dx11RenderingDevice.UncompressUvw(uvwAndBlendModesPtr[i], textureScaling));

                        if (texture != null)
                        {
                            Vector2 uv;
                            uint highestBits;
                            Dx11RenderingDevice.UncompressUvw(uvwAndBlendModesPtr[i], texture.Pos, textureScaling, out uv, out highestBits);
                            uvwAndBlendModesPtr[i] = Dx11RenderingDevice.CompressUvw(allocator2.Get(texture.Texture), textureScaling2, uv, highestBits);
                        }
                    }
                }

                // Upload data
                var oldVertexBuffer = VertexBuffer;
                fixed (byte* dataPtr = data)
                {
                    var desc = new BufferDesc
                    {
                        ByteWidth = (uint)VertexBufferSize,
                        Usage = D3D11Usage.Immutable,
                        BindFlags = (uint)BindFlag.VertexBuffer,
                        CPUAccessFlags = 0,
                        MiscFlags = 0,
                        StructureByteStride = 0,
                    };
                    var subresData = new SubresourceData
                    {
                        PSysMem = dataPtr,
                    };
                    ID3D11Buffer* buf;
                    SilkMarshal.ThrowHResult(Device.Device->CreateBuffer(&desc, &subresData, &buf));
                    VertexBuffer = buf;

                    if (oldVertexBuffer != null)
                        oldVertexBuffer->Release();
                }

                for (int i = 0; i < VertexBufferBindings.Length; ++i)
                    if (VertexBufferBindings[i].Buffer == oldVertexBuffer)
                        VertexBufferBindings[i].Buffer = VertexBuffer;
            };
        }

        public override void Render(RenderArgs arg)
        {
            if (VertexCount == 0)
                return;
            var context = Device.Context;

            // Setup state
            ((Dx11RenderingSwapChain)arg.RenderTarget).Bind();
            Device.RoomShader.Apply(context, arg.StateBuffer);
            { var ss = arg.BilinearFilter ? Device.SamplerDefault : Device.SamplerRoundToNearest; context->PSSetSamplers(0, 1, &ss); }
            { var srvs = stackalloc ID3D11ShaderResourceView*[] { TextureView, Device.SectorTextureArrayView }; context->PSSetShaderResources(0, 2, srvs); }
            Dx11RenderingDevice.SetVertexBuffers(context, 0, VertexBufferBindings);

            // Render
            context->Draw((uint)VertexCount, 0);
        }
    }
}
