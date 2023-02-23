using Pfim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.TombEngine
{
    internal class TombEngineSpriteAllocator
    {
        private int _atlasSize;

        // Typical level textures are to big to use them directly
        // Therefore the texture can be cut into pieces of 256² maximum size.
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct TextureView : IEquatable<TextureView>
        {
            public Texture Texture;
            public RectangleInt2 Area;
            public VectorInt2 Position;

            public TextureView(Texture texture, RectangleInt2 area)
            {
                Texture = texture;
                Area = area;
                Position = VectorInt2.Zero;
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

        public TombEngineSpriteAllocator(int atlasSize = 4096)
        {
            _atlasSize = atlasSize;
        }

        public int GetOrAllocateTextureID(TextureView texture, int priorityClass = 0)
        {
            if (!new Rectangle2(new Vector2(), texture.Texture.Image.Size).Contains(texture.Area))
                throw new ArgumentOutOfRangeException(nameof(texture.Area));

            if (texture.Area.X0 < 0 || texture.Area.Y0 < 0 || texture.Area.X1 < 0 || texture.Area.Y1 < 0)
                return -1; // Negative texture coordinates found!
            if (texture.Area.Width > _atlasSize || texture.Area.Height > _atlasSize)
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

        private void AddPadding(VectorInt2 position, ImageC from, ImageC to, int pageOffset, int padding, int? customX = null, int? customY = null)
        {
            var x = 0;
            var y = 0;
            var width = from.Width;
            var height = from.Height;
            var dataOffset = 0;

            // Add actual padding (ported code from OT bordered_texture_atlas.cpp)

            var topLeft = from.GetPixel(x, y);
            var topRight = from.GetPixel(x + width - 1, y);
            var bottomLeft = from.GetPixel(x, y + height - 1);
            var bottomRight = from.GetPixel(x + width - 1, y + height - 1);

            for (int xP = 0; xP < padding; xP++)
            {
                // copy left line
                if (xP < padding)
                    to.CopyFrom(position.X + xP, dataOffset + position.Y + padding, from,
                               x, y, 1, height);

                // copy right line
                if (xP < padding)
                    to.CopyFrom(position.X + xP + width + padding, dataOffset + position.Y + padding, from,
                               x + width - 1, y, 1, height);

                for (int yP = 0; yP < padding; yP++)
                {
                    // copy top line
                    if (yP < padding)
                        to.CopyFrom(position.X + padding, dataOffset + position.Y + yP, from,
                                   x, y, width, 1);
                    // copy bottom line
                    if (yP < padding)
                        to.CopyFrom(position.X + padding, dataOffset + position.Y + yP + height + padding, from,
                                   x, y + height - 1, width, 1);

                    // expand top-left pixel
                    if (xP < padding && yP < padding)
                        to.SetPixel(position.X + xP, dataOffset + position.Y + yP, topLeft);
                    // expand top-right pixel
                    if (xP < padding && yP < padding)
                        to.SetPixel(position.X + xP + width + padding, dataOffset + position.Y + yP, topRight);
                    // expand bottom-left pixel
                    if (xP < padding && yP < padding)
                        to.SetPixel(position.X + xP, dataOffset + position.Y + yP + height + padding, bottomLeft);
                    // expand bottom-right pixel
                    if (xP < padding && yP < padding)
                        to.SetPixel(position.X + xP + width + padding, dataOffset + position.Y + yP + height + padding, bottomRight);
                }
            }
        }

        public List<ImageC> PackTextures()
        {
            // Increase dynamically the atlas size for fitting all sprites without wasting space.
            // For now just one atlas, in the future we can have many atlases.

            int padding = 8;

            //Sort textures for packing order...
            int[] usedTexturesProcessingOrder = new int[_usedTextures.Count];
            for (int i = 0; i < _usedTextures.Count; ++i)
                usedTexturesProcessingOrder[i] = i;
            Array.Sort(usedTexturesProcessingOrder, new TextureComparer { _usedTextures = _usedTextures });

            bool done;
            int atlasWidth = 256;
            int atlasHeight = 256;

            do
            {
                RectPacker texPacker = new RectPackerTree(new VectorInt2(atlasWidth, atlasHeight));

                done = true;

                foreach (int usedTextureIndex in usedTexturesProcessingOrder)
                {
                    TextureView usedTexture = _usedTextures[usedTextureIndex];

                    // Get the size of the quad surrounding texture area, typically should be the texture area itself
                    int w = (int)(usedTexture.Area.Width);
                    int h = (int)(usedTexture.Area.Height);

                    // Calculate adaptive padding at all sides
                    int tP = padding;
                    int bP = padding;
                    int lP = padding;
                    int rP = padding;

                    int horizontalPaddingSpace = _atlasSize - w;
                    int verticalPaddingSpace = _atlasSize - h;

                    // If hor/ver padding won't fully fit, get existing space and calculate padding out of it
                    if (verticalPaddingSpace < tP + bP)
                    {
                        tP = verticalPaddingSpace / 2;
                        bP = verticalPaddingSpace - tP;
                    }

                    if (horizontalPaddingSpace < padding * 2)
                    {
                        lP = horizontalPaddingSpace / 2;
                        rP = horizontalPaddingSpace - lP;
                    }

                    w += lP + rP;
                    h += tP + bP;

                    // Pack texture
                    VectorInt2? pos = texPacker.TryAdd(new VectorInt2(w, h));
                    if (pos.HasValue)
                    {
                        usedTexture.Position = pos.Value;
                        _usedTextures[usedTextureIndex] = usedTexture;  
                    }
                    else
                    {
                        done = false;
                        atlasWidth += 256;
                        atlasHeight += 256;
                        break;
                    }
                }
            } while (!done);

            ImageC resultingTexture = ImageC.CreateNew(atlasWidth, atlasHeight);

            List<ImageC> resultingTextures = new List<ImageC>();
            _usedTexturePackInfos = new Result[_usedTextures.Count];

            for (int i = 0; i < _usedTextures.Count; i++)
            {
                var texture = _usedTextures[i];
                resultingTexture.CopyFrom(texture.Position.X + padding, texture.Position.Y + padding,
                   texture.Texture.Image, texture.Area.X0, texture.Area.Y0, texture.Area.Width, texture.Area.Height);
                _usedTexturePackInfos[i] = new Result
                {
                    OutputTextureID = 0,
                    Pos = texture.Position
                };
                AddPadding(texture.Position, texture.Texture.Image, resultingTexture, 0, padding);
            }

            resultingTextures.Add(resultingTexture);

            return resultingTextures;
        }

        public Result GetPackInfo(int id)
        {
            return _usedTexturePackInfos[id];
        }
    }
}
