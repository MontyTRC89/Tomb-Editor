using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.Util
{
    public class LegacyTextureAllocator
    {
        public const int OutputTextureWidth = 256;
        public const int OutputTextureHeight = 256;

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
            public List<int> _priorityClass;
            public List<TextureView> _usedTextures;
            public int Compare(int firstIndex, int secondIndex)
            {
                // Compare priority eg for textures if they should be in first page
                // Higher priorties are put in first
                int firstPriorityClass = _priorityClass[firstIndex];
                int secondPriorityClass = _priorityClass[secondIndex];
                if (firstPriorityClass != secondPriorityClass)
                    return firstPriorityClass > secondPriorityClass ? -1 : 1;

                TextureView first = _usedTextures[firstIndex];
                TextureView second = _usedTextures[secondIndex];

                // Compare height
                int firstMaxHeight = Math.Max(first.Area.Width, first.Area.Height); //Because of flipping, the bigger dimension is the height.
                int secondMaxHeight = Math.Max(second.Area.Width, second.Area.Height);
                if (firstMaxHeight != secondMaxHeight)
                    return firstMaxHeight > secondMaxHeight ? -1 : 1; //Heigher textures first!

                // Compare area
                int firstArea = first.Area.Width * first.Area.Height;
                int secondArea = second.Area.Width * second.Area.Height;
                if (firstArea != secondArea)
                    return firstArea > secondArea ? -1 : 1; //Bigger textures first!

                return 0;
            }
        }

        private readonly List<int> _priorityClass = new List<int>();
        private readonly List<TextureView> _usedTextures = new List<TextureView>();
        private readonly Dictionary<TextureView, int> _usedTexturesLookup = new Dictionary<TextureView, int>();
        private readonly Dictionary<Hash, Texture> _hashedTextures = new Dictionary<Hash, Texture>();
        private Result[] _usedTexturePackInfos;

        public int GetOrAllocateTextureID(TextureView texture, int priorityClass = 0)
        {
            if (texture.Area.X0 < 0 || texture.Area.Y0 < 0 || texture.Area.X1 < 0 || texture.Area.Y1 < 0)
                throw new ArgumentOutOfRangeException("Negative texture coordinates found! Please check your imported geometry and try the Unwrap setting while importing");
            if (!new Rectangle2(new Vector2(), texture.Texture.Image.Size).Contains(texture.Area))
                throw new ArgumentOutOfRangeException(nameof(texture.Area));
            if (texture.Area.Width > 256 || texture.Area.Height > 256)
                throw new NotSupportedException("Texture page too big!");

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
            {
                if (priorityClass > _priorityClass[textureID])
                    _priorityClass[textureID] = priorityClass;
                return textureID;
            }

            textureID = _usedTextures.Count;
            _usedTextures.Add(texture);
            _priorityClass.Add(priorityClass);
            _usedTexturesLookup.Add(texture, textureID);
            return textureID;
        }

        public int GetTextureID(TextureView texture)
        {
            return _usedTexturesLookup[texture];
        }

        public TextureView GetTextureFromID(int id)
        {
            return _usedTextures[id];
        }

        public List<ImageC> PackTextures()
        {
            //Sort textures for packing order...
            int[] usedTexturesProcessingOrder = new int[_usedTextures.Count];
            for (int i = 0; i < _usedTextures.Count; ++i)
                usedTexturesProcessingOrder[i] = i;
            Array.Sort(usedTexturesProcessingOrder, new TextureComparer { _usedTextures = _usedTextures, _priorityClass = _priorityClass });

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

                if (resultingTextures.Count >= 65535)
                    throw new NotSupportedException("More then 65536 textures are not supported. That is A LOT (exactly 16GB of texture data), so its probably some other bug if you see this message.");
                var packer = new RectPackerTree(new VectorInt2(OutputTextureWidth, OutputTextureHeight));
                usedTexturePackInfo = new Result { OutputTextureID = (ushort)resultingTextures.Count, Pos = packer.TryAdd(usedTexture.Area.Size).Value };
                resultingTextures.Add(ImageC.CreateNew(OutputTextureWidth, OutputTextureHeight));
                resultingTexturesPacker.Add(packer);

                //Write texture data...
                PackNextUsedTexture:
                resultingTextures[usedTexturePackInfo.OutputTextureID].CopyFrom(usedTexturePackInfo.Pos.X, usedTexturePackInfo.Pos.Y,
                    usedTexture.Texture.Image, usedTexture.Area.X0, usedTexture.Area.Y0, usedTexture.Area.Width, usedTexture.Area.Height);
                _usedTexturePackInfos[UsedTextureIndex] = usedTexturePackInfo;
            }

            return resultingTextures;
        }

        /*private int GetOrAllocateTextureIDForPageAt(ref TextureArea texture, int pageX, int pageY,
                                                    int pageWidth, int pageHeight, int priorityClass)
        {
            Vector2 pageOffset = new Vector2(pageX, pageY);
            texture.TexCoord0 -= pageOffset;
            texture.TexCoord1 -= pageOffset;
            texture.TexCoord2 -= pageOffset;
            texture.TexCoord3 -= pageOffset;

            pageWidth = Math.Min(texture.Texture.Image.Width - pageX, pageWidth);
            pageHeight = Math.Min(texture.Texture.Image.Height - pageY, pageHeight);

            return GetOrAllocateTextureID(new TextureView(texture.Texture, RectangleInt2.FromLTRB(pageX, pageY, pageWidth, pageHeight)), priorityClass);
        }*/

        /*public int GetOrAllocateTextureID(ref TextureArea texture, bool isTriangle, int priorityClass = 0)
        {
            // Textures bigger than 256² must be split into pieces of 256 max each
            Vector2 minTexCoord = Vector2.Min(Vector2.Min(texture.TexCoord0, texture.TexCoord1), isTriangle ? texture.TexCoord2 : Vector2.Min(texture.TexCoord2, texture.TexCoord3));
            Vector2 maxTexCoord = Vector2.Max(Vector2.Max(texture.TexCoord0, texture.TexCoord1), isTriangle ? texture.TexCoord2 : Vector2.Max(texture.TexCoord2, texture.TexCoord3));

            int startX = (int)Math.Floor(minTexCoord.X);
            int startY = (int)Math.Floor(minTexCoord.Y);
            int endX = (int)Math.Ceiling(maxTexCoord.X) - 1;
            int endY = (int)Math.Ceiling(maxTexCoord.Y) - 1;

            const int pageWidth = OutputTextureWidth;
            const int pageHeight = OutputTextureHeight;

            if (endX - startX >= pageWidth || endY - startY >= pageHeight) // 'equals' is included too, because 1 was subtracted from 'end' above
                throw new ApplicationException("An applied texture is bigger than 256²! We don't support that yet.");

            if (startX / pageWidth == endX / pageWidth)
            { // Try to pack into a page that is at multiple of 256 on X
                if (startY / pageHeight == endY / pageHeight)
                { // Try to pack into a page that is at multiple of 256 on Y
                    return GetOrAllocateTextureIDForPageAt(ref texture,
                        startX / pageWidth * pageWidth,
                        startY / pageHeight * pageHeight, pageWidth, pageHeight, priorityClass);
                }
            }

            // Include the new texture specifically from whatever region it needs
            return GetOrAllocateTextureIDForPageAt(ref texture, startX, startY, endX - startX + 1, endY - startY + 1, priorityClass);
        }*/

        public Result GetPackInfo(int id)
        {
            return _usedTexturePackInfos[id];
        }

        public Result GetPackInfo(TextureView texture)
        {
            return _usedTexturePackInfos[GetTextureID(texture)];
        }
    }
}
