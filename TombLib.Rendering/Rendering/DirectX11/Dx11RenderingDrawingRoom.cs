using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using TombLib.LevelData;
using TombLib.Utils;
using Buffer = SharpDX.Direct3D11.Buffer;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11RenderingDrawingRoom : RenderingDrawingRoom
    {
        public readonly Dx11RenderingDevice Device;
        public readonly ShaderResourceView TextureView;
        public readonly RenderingTextureAllocator TextureAllocator;
        public Buffer VertexBuffer;
        public readonly VertexBufferBinding[] VertexBufferBindings;
        public readonly int VertexCount;
        public readonly int VertexBufferSize;
        public bool TexturesInvalidated = false;
        public bool TexturesInvalidatedRetried = false;

        public unsafe Dx11RenderingDrawingRoom(Dx11RenderingDevice device, Description description)
        {
            Device = device;
            TextureView = ((Dx11RenderingTextureAllocator)(description.TextureAllocator)).TextureView;
            TextureAllocator = description.TextureAllocator;
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(TextureAllocator.Size.X, TextureAllocator.Size.Y);

            RoomGeometry roomGeometry = description.Room.RoomGeometry;

            // Create buffer
            Vector3 worldPos = description.Room.WorldPos + description.Offset;
            int singleSidedVertexCount = roomGeometry.VertexPositions.Count;
            int vertexCount = VertexCount = singleSidedVertexCount + roomGeometry.DoubleSidedTriangleCount * 3;
            if (vertexCount == 0)
                return;
            VertexBufferSize = vertexCount * (sizeof(Vector3) + sizeof(uint) + sizeof(ulong) + sizeof(uint));
            fixed (byte* data = new byte[VertexBufferSize])
            {
                Vector3* positions = (Vector3*)(data);
                uint* colors = (uint*)(data + vertexCount * sizeof(Vector3));
                ulong* uvwAndBlendModes = (ulong*)(data + vertexCount * (sizeof(Vector3) + sizeof(uint)));
                uint* editorUVAndSectorTexture = (uint*)(data + vertexCount * (sizeof(Vector3) + sizeof(uint) + sizeof(ulong)));

                // Setup vertices
                for (int i = 0; i < singleSidedVertexCount; ++i)
                    positions[i] = roomGeometry.VertexPositions[i] + worldPos;
                for (int i = 0; i < singleSidedVertexCount; ++i)
                {
                    Vector3 vertexColor = roomGeometry.VertexColors[i];
                    vertexColor = Vector3.Max(new Vector3(), Vector3.Min(new Vector3(255.0f), vertexColor * 128.0f + new Vector3(0.5f)));
                    colors[i] = ((uint)vertexColor.X) | (((uint)vertexColor.Y) << 8) | (((uint)vertexColor.Z) << 16) | 0xff000000;
                }
                for (int i = 0; i < singleSidedVertexCount; ++i)
                {
                    Vector2 vertexEditorUv = roomGeometry.VertexEditorUVs[i];
                    uint editorUv = 0;
                    editorUv |= (uint)((int)vertexEditorUv.X) & 3;
                    editorUv |= ((uint)((int)vertexEditorUv.Y) & 3) << 2;
                    editorUVAndSectorTexture[i] = editorUv;
                }
                {
                    SectorInfo lastSectorInfo = new SectorInfo(-1, -1, BlockFace.Floor);
                    uint lastSectorTexture = 0;
                    for (int i = 0, triangleCount = singleSidedVertexCount / 3; i < triangleCount; ++i)
                    {
                        SectorInfo currentSectorInfo = roomGeometry.TriangleSectorInfo[i];
                        if (!lastSectorInfo.Equals(currentSectorInfo))
                        {
                            SectorTextureResult result = description.SectorTextureGet(description.Room, currentSectorInfo.Pos.X, currentSectorInfo.Pos.Y, currentSectorInfo.Face);

                            lastSectorInfo = currentSectorInfo;
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
                        }
                        editorUVAndSectorTexture[i * 3 + 0] |= lastSectorTexture;
                        editorUVAndSectorTexture[i * 3 + 1] |= lastSectorTexture;
                        editorUVAndSectorTexture[i * 3 + 2] |= lastSectorTexture;
                    }
                }

                RetryTexturing:
                ;
                {
                    int doubleSidedVertexIndex = singleSidedVertexCount;
                    for (int i = 0, triangleCount = singleSidedVertexCount / 3; i < triangleCount; ++i)
                    {
                        TextureArea texture = roomGeometry.TriangleTextureAreas[i];

                        if (texture.Texture == null)
                        { // Render as geometry
                            uvwAndBlendModes[i * 3 + 0] = 1ul << 24;
                            uvwAndBlendModes[i * 3 + 1] = 1ul << 24;
                            uvwAndBlendModes[i * 3 + 2] = 1ul << 24;
                        }
                        else if (texture.Texture is TextureInvisible)
                        { // Render as invisible
                            uvwAndBlendModes[i * 3 + 0] = 0ul << 24;
                            uvwAndBlendModes[i * 3 + 1] = 0ul << 24;
                            uvwAndBlendModes[i * 3 + 2] = 0ul << 24;
                        }
                        else
                        { // Render as textured (the texture may turn out to be unavailable)
                            if (texture.Texture.IsUnavailable)
                            { // Texture is unvailable (i.e. file couldn't be loaded.
                                ImageC image = Dx11RenderingDevice.TextureUnavailable;
                                VectorInt3 position = TextureAllocator.Get(image);
                                uvwAndBlendModes[i * 3 + 0] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 0]) * (image.Size - new VectorInt2(1, 1)) + new Vector2(0.5f), (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 1] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 1]) * (image.Size - new VectorInt2(1, 1)) + new Vector2(0.5f), (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 2] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 2]) * (image.Size - new VectorInt2(1, 1)) + new Vector2(0.5f), (uint)texture.BlendMode);
                            }
                            else if (texture.TriangleCoordsOutOfBounds)
                            { // Texture is available but coordinates are ouf of bounds
                                ImageC image = Dx11RenderingDevice.TextureCoordOutOfBounds;
                                VectorInt3 position = TextureAllocator.Get(image);
                                uvwAndBlendModes[i * 3 + 0] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 0]) * (image.Size - new VectorInt2(1, 1)) + new Vector2(0.5f), (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 1] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 1]) * (image.Size - new VectorInt2(1, 1)) + new Vector2(0.5f), (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 2] = Dx11RenderingDevice.CompressUvw(position, textureScaling, Vector2.Abs(roomGeometry.VertexEditorUVs[i * 3 + 2]) * (image.Size - new VectorInt2(1, 1)) + new Vector2(0.5f), (uint)texture.BlendMode);
                            }
                            else
                            { // Texture is available
                                VectorInt3 position = TextureAllocator.GetForTriangle(texture);
                                uvwAndBlendModes[i * 3 + 0] = Dx11RenderingDevice.CompressUvw(position, textureScaling, texture.TexCoord0, (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 1] = Dx11RenderingDevice.CompressUvw(position, textureScaling, texture.TexCoord1, (uint)texture.BlendMode);
                                uvwAndBlendModes[i * 3 + 2] = Dx11RenderingDevice.CompressUvw(position, textureScaling, texture.TexCoord2, (uint)texture.BlendMode);
                            }

                            // Duplicate double sided triangles
                            if (texture.DoubleSided)
                            {
                                positions[doubleSidedVertexIndex] = positions[i * 3 + 2];
                                colors[doubleSidedVertexIndex] = colors[i * 3 + 2];
                                uvwAndBlendModes[doubleSidedVertexIndex] = uvwAndBlendModes[i * 3 + 2];
                                editorUVAndSectorTexture[doubleSidedVertexIndex++] = editorUVAndSectorTexture[i * 3 + 2];

                                positions[doubleSidedVertexIndex] = positions[i * 3 + 1];
                                colors[doubleSidedVertexIndex] = colors[i * 3 + 1];
                                uvwAndBlendModes[doubleSidedVertexIndex] = uvwAndBlendModes[i * 3 + 1];
                                editorUVAndSectorTexture[doubleSidedVertexIndex++] = editorUVAndSectorTexture[i * 3 + 1];

                                positions[doubleSidedVertexIndex] = positions[i * 3 + 0];
                                colors[doubleSidedVertexIndex] = colors[i * 3 + 0];
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
                VertexBuffer = new Buffer(device.Device, new IntPtr(data),
                    new BufferDescription(VertexBufferSize, ResourceUsage.Immutable, BindFlags.VertexBuffer,
                    CpuAccessFlags.None, ResourceOptionFlags.None, 0));
                VertexBufferBindings = new VertexBufferBinding[] {
                    new VertexBufferBinding(VertexBuffer, sizeof(Vector3), (int)((byte*)positions - data)),
                    new VertexBufferBinding(VertexBuffer, sizeof(uint), (int)((byte*)colors - data)),
                    new VertexBufferBinding(VertexBuffer, sizeof(ulong), (int)((byte*)uvwAndBlendModes - data)),
                    new VertexBufferBinding(VertexBuffer, sizeof(uint), (int)((byte*)editorUVAndSectorTexture - data))
                };
                VertexBuffer.SetDebugName("Room " + (description.Room.Name ?? ""));
            }
            TextureAllocator.GarbageCollectionCollectEvent.Add(GarbageCollectTexture);
        }

        public override void Dispose()
        {
            TextureAllocator.GarbageCollectionCollectEvent.Remove(GarbageCollectTexture);
            if (VertexBuffer != null)
                VertexBuffer.Dispose();
        }

        public unsafe RenderingTextureAllocator.GarbageCollectionAdjustDelegate GarbageCollectTexture(RenderingTextureAllocator allocator,
            RenderingTextureAllocator.Map map, HashSet<RenderingTextureAllocator.Map.Entry> inOutUsedTextures)
        {
            TexturesInvalidated = true;
            if (VertexBuffer == null)
                return null;

            byte[] data = Device.ReadBuffer(VertexBuffer, VertexBufferSize);
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(TextureAllocator.Size.X, TextureAllocator.Size.Y);
            int uvwAndBlendModesOffset = VertexBufferBindings[2].Offset;

            // Collect all used textures
            fixed (byte* dataPtr = data)
            {
                ulong* uvwAndBlendModesPtr = (ulong*)(dataPtr + uvwAndBlendModesOffset);
                for (int i = 0; i < VertexCount; ++i)
                {
                    if (uvwAndBlendModesPtr[i] < 0x1000000) // Very small coordinates make no sense, they are used as a placeholder
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
                Vector2 textureScaling2 = new Vector2(16777216.0f) / new Vector2(TextureAllocator.Size.X, TextureAllocator.Size.Y);

                // Update data
                fixed (byte* dataPtr = data)
                {
                    ulong* uvwAndBlendModesPtr = (ulong*)(dataPtr + uvwAndBlendModesOffset);
                    for (int i = 0; i < VertexCount; ++i)
                    {
                        if (uvwAndBlendModesPtr[i] < 0x1000000) // Very small coordinates make no sense, they are used as a placeholder
                            continue;
                        var texture = map.Lookup(Dx11RenderingDevice.UncompressUvw(uvwAndBlendModesPtr[i], textureScaling));
                        Vector2 uv;
                        uint highestBits;
                        Dx11RenderingDevice.UncompressUvw(uvwAndBlendModesPtr[i], texture.Pos, textureScaling, out uv, out highestBits);
                        uvwAndBlendModesPtr[i] = Dx11RenderingDevice.CompressUvw(allocator2.Get(texture.Texture), textureScaling2, uv, highestBits);
                    }
                }

                // Upload data
                var oldVertexBuffer = VertexBuffer;
                fixed (byte* dataPtr = data)
                {
                    VertexBuffer = new Buffer(Device.Device, new IntPtr(dataPtr),
                        new BufferDescription(VertexBufferSize, ResourceUsage.Immutable, BindFlags.VertexBuffer,
                        CpuAccessFlags.None, ResourceOptionFlags.None, 0));
                    oldVertexBuffer.Dispose();
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
            context.PixelShader.SetSampler(0, Device.SamplerDefault);
            context.PixelShader.SetShaderResources(0, TextureView, Device.SectorTextureArrayView);
            context.InputAssembler.SetVertexBuffers(0, VertexBufferBindings);

            // Render
            context.Draw(VertexCount, 0);
        }
    }
}
