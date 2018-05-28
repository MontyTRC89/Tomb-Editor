using System;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.Collections.Generic;
using TombLib.LevelData;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using TombLib.Utils;
using System.Runtime.CompilerServices;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11RenderingDrawingRoom : RenderingDrawingRoom
    {
        public readonly Dx11RenderingDevice Device;
        public readonly ShaderResourceView TextureView;
        public readonly RenderingTextureAllocatorUser User;
        public readonly Buffer VertexBuffer;
        public readonly VertexBufferBinding[] VertexBufferBindings;
        public readonly int VertexCount;

        public unsafe Dx11RenderingDrawingRoom(Dx11RenderingDevice device, Description description)
        {
            Device = device;
            TextureView = ((Dx11RenderingTextureAllocator)(description.Allocator)).TextureView;
            User = new RenderingTextureAllocatorUser(description.Allocator);
            Vector2 textureScaling = new Vector2(16777216.0f) / new Vector2(description.Allocator.Size.X, description.Allocator.Size.Y);

            RoomGeometry roomGeometry = description.Room.RoomGeometry;

            // Create buffer
            Vector3 worldPos = description.Room.WorldPos + description.Offset;
            int singleSidedVertexCount = roomGeometry.VertexPositions.Count;
            int vertexCount = VertexCount = singleSidedVertexCount + roomGeometry.DoubleSidedTriangleCount * 3;
            int size = vertexCount * (sizeof(Vector3) + sizeof(uint) + sizeof(ulong) + sizeof(uint));
            fixed (byte* data = new byte[size])
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
                            if (result.HighlightedSelection)
                                lastSectorTexture |= 0x10;
                            if (result.SectorTexture != SectorTexture.None)
                            { // Use sector texture
                                lastSectorTexture = 0x20 | (((uint)result.SectorTexture - 1) << 6);
                            }
                            else
                            { // Use sector color
                                lastSectorTexture =
                                    (((uint)(result.Color.X * 255)) << 6) |
                                    (((uint)(result.Color.Y * 255)) << 14) |
                                    (((uint)(result.Color.Z * 255)) << 22);
                            }
                        }
                        editorUVAndSectorTexture[i * 3 + 0] |= lastSectorTexture;
                        editorUVAndSectorTexture[i * 3 + 1] |= lastSectorTexture;
                        editorUVAndSectorTexture[i * 3 + 2] |= lastSectorTexture;
                    }
                }
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
                        {
                            VectorInt3 position = User.AllocateTextureForTriangle(texture);
                            uvwAndBlendModes[i * 3 + 0] = CompressTextureCoordinate(position, textureScaling, texture.TexCoord0, texture.BlendMode);
                            uvwAndBlendModes[i * 3 + 1] = CompressTextureCoordinate(position, textureScaling, texture.TexCoord1, texture.BlendMode);
                            uvwAndBlendModes[i * 3 + 2] = CompressTextureCoordinate(position, textureScaling, texture.TexCoord2, texture.BlendMode);

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
                }

                // Create GPU resources
                VertexBuffer = new Buffer(device.Device, new IntPtr(data),
                new BufferDescription(size, ResourceUsage.Immutable, BindFlags.VertexBuffer,
                CpuAccessFlags.None, ResourceOptionFlags.None, 0));
                VertexBufferBindings = new VertexBufferBinding[] {
                    new VertexBufferBinding(VertexBuffer, sizeof(Vector3), (int)((byte*)positions - data)),
                    new VertexBufferBinding(VertexBuffer, sizeof(uint), (int)((byte*)colors - data)),
                    new VertexBufferBinding(VertexBuffer, sizeof(ulong), (int)((byte*)uvwAndBlendModes - data)),
                    new VertexBufferBinding(VertexBuffer, sizeof(uint), (int)((byte*)editorUVAndSectorTexture - data))
                };
            }
        }

        public override void Dispose()
        {
            User.Dispose();
            VertexBuffer.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong CompressTextureCoordinate(VectorInt3 position, Vector2 textureScaling, Vector2 uv, BlendMode blendMode)
        {
            uint blendMode2 = Math.Min((uint)blendMode, 15);
            uint x = (uint)((position.X + uv.X) * textureScaling.X);
            uint y = (uint)((position.Y + uv.Y) * textureScaling.Y);
            return x | ((ulong)y << 24) | ((ulong)position.Z << 48) | ((ulong)blendMode2 << 60);
        }

        public override void Render(RenderArgs arg)
        {
            var context = Device.Context;

            // Setup state
            ((Dx11RenderingSwapChain)arg.RenderTarget).Bind();
            Device.RoomShader.Apply(context, arg.StateBuffer);
            context.PixelShader.SetSampler(0, Device.Sampler);
            context.PixelShader.SetShaderResources(0, TextureView, Device.SectorTextureArrayView);
            context.InputAssembler.SetVertexBuffers(0, VertexBufferBindings);

            // Render
            context.Draw(VertexCount, 0);
        }
    }
}
