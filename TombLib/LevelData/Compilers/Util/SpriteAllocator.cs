using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.Util
{
    public class SpriteAllocator
    {
        private int _pageSize;

        // Typical level textures are to big to use them directly
        // Therefore the texture can be cut into pieces of 256² maximum size.
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct TextureView : IEquatable<TextureView>
        {
            public Texture Texture;
            public RectangleInt2 Area;

            public TextureView(Texture texture, RectangleInt2 area)
            {
                Texture = texture;
                Area = area;
            }

            public static implicit operator TextureView(Texture texture)
            {
                return new TextureView(texture, new RectangleInt2(new VectorInt2(), texture.Image.Size));
            }

            // Custom implementation of these because default implementation is *insanely* slow.
            // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
            public static bool operator ==(TextureView first, TextureView second)
            {
                return first.Texture == second.Texture && first.Area == second.Area;
            }
            public static bool operator !=(TextureView first, TextureView second) => !(first == second);
            public bool Equals(TextureView other) => this == other;
            public override bool Equals(object other) => other is TextureView && this == (TextureView)other;
            public override int GetHashCode() => Texture.GetHashCode();
        }

        public struct Result
        {
            public ushort OutputTextureID;
            public VectorInt2 Pos;
            public void TransformTexCoord(ref ushort TexCoordX, ref ushort TexCoordY)
            {
                TexCoordX += (ushort)(256 * Pos.X);
                TexCoordY += (ushort)(256 * Pos.Y);
            }
        }

        private struct TextureComparer : IComparer<int>
        {
            public List<TextureView> _usedTextures;
            public int Compare(int firstIndex, int secondIndex)
            {
                TextureView first = _usedTextures[firstIndex];
                TextureView second = _usedTextures[secondIndex];

                // Compare height
                int firstMaxHeight = Math.Max(first.Area.Width, first.Area.Height); // Because of flipping, the bigger dimension is the height.
                int secondMaxHeight = Math.Max(second.Area.Width, second.Area.Height);
                if (firstMaxHeight != secondMaxHeight)
                    return firstMaxHeight > secondMaxHeight ? -1 : 1; // Heigher textures first!

                // Compare area
                int firstArea = first.Area.Width * first.Area.Height;
                int secondArea = second.Area.Width * second.Area.Height;
                if (firstArea != secondArea)
                    return firstArea > secondArea ? -1 : 1; // Bigger textures first!

                return 0;
            }
        }
        
        private readonly List<TextureView> _usedTextures = new List<TextureView>();
        private readonly Dictionary<TextureView, int> _usedTexturesLookup = new Dictionary<TextureView, int>();
        private readonly Dictionary<Hash, Texture> _hashedTextures = new Dictionary<Hash, Texture>();
        private Result[] _usedTexturePackInfos;

        public SpriteAllocator(int pageSize = 256)
        {
            _pageSize = pageSize;
        }

        public int GetOrAllocateTextureID(TextureView texture, int priorityClass = 0)
        {
            if (!new Rectangle2(new Vector2(), texture.Texture.Image.Size).Contains(texture.Area))
                throw new ArgumentOutOfRangeException(nameof(texture.Area));

            if (texture.Area.X0 < 0 || texture.Area.Y0 < 0 || texture.Area.X1 < 0 || texture.Area.Y1 < 0)
                return -1; // Negative texture coordinates found!
            if (texture.Area.Width > _pageSize || texture.Area.Height > _pageSize)
                return -1; // Texture page too big!

            // Deduplicate hashed textures
            {
                TextureHashed textureHashed = texture.Texture as TextureHashed;
                if (textureHashed != null)
                {
                    Texture existingTexture;
                    if (_hashedTextures.TryGetValue(textureHashed.Hash, out existingTexture))
                        texture.Texture = existingTexture;
                    else
                        _hashedTextures.Add(textureHashed.Hash, texture.Texture);
                }
            }

            // Check texture
            int textureID;
            if (_usedTexturesLookup.TryGetValue(texture, out textureID))
                return textureID;

            textureID = _usedTextures.Count;
            _usedTextures.Add(texture);
            _usedTexturesLookup.Add(texture, textureID);
            return textureID;
        }

        public int GetTextureID(TextureView texture)
        {
            return _usedTexturesLookup[texture];
        }

        public List<ImageC> PackTextures()
        {
            //Sort textures for packing order...
            int[] usedTexturesProcessingOrder = new int[_usedTextures.Count];
            for (int i = 0; i < _usedTextures.Count; ++i)
                usedTexturesProcessingOrder[i] = i;
            Array.Sort(usedTexturesProcessingOrder, new TextureComparer { _usedTextures = _usedTextures });

            //Pack the textures...
            List<ImageC> resultingTextures = new List<ImageC>();
            List<RectPacker> resultingTexturesPacker = new List<RectPacker>();
            _usedTexturePackInfos = new Result[_usedTextures.Count];
            foreach (int UsedTextureIndex in usedTexturesProcessingOrder)
            {
                TextureView usedTexture = _usedTextures[UsedTextureIndex];

                Result usedTexturePackInfo;
                for (ushort i = 0; i < resultingTextures.Count; ++i)
                {
                    VectorInt2? packingPosition = resultingTexturesPacker[i].TryAdd(usedTexture.Area.Size);
                    if (packingPosition.HasValue)
                    {
                        usedTexturePackInfo = new Result { OutputTextureID = i, Pos = packingPosition.Value };
                        goto PackNextUsedTexture;
                    }
                }

                var packer = new RectPackerTree(new VectorInt2(_pageSize));
                usedTexturePackInfo = new Result { OutputTextureID = (ushort)resultingTextures.Count, Pos = packer.TryAdd(usedTexture.Area.Size).Value };
                resultingTextures.Add(ImageC.CreateNew(_pageSize, _pageSize));
                resultingTexturesPacker.Add(packer);

                //Write texture data...
                PackNextUsedTexture:
                resultingTextures[usedTexturePackInfo.OutputTextureID].CopyFrom(usedTexturePackInfo.Pos.X, usedTexturePackInfo.Pos.Y,
                    usedTexture.Texture.Image, usedTexture.Area.X0, usedTexture.Area.Y0, usedTexture.Area.Width, usedTexture.Area.Height);
                _usedTexturePackInfos[UsedTextureIndex] = usedTexturePackInfo;
            }

            return resultingTextures;
        }

        public Result GetPackInfo(int id)
        {
            return _usedTexturePackInfos[id];
        }
    }
}
