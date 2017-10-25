using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TombEditor.Geometry;
using TombLib.Utils;

namespace TombEditor.Compilers.Util
{
    public class TextureAllocator
    {
        public const int OutputTextureWidth = 256;
        public const int OutputTextureHeight = 256;

        // Typical level textures are to big to use them directly
        // Therefore the texture can be cut into pieces of 256² maximum size.
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct TextureView : IEquatable<TextureView>
        {
            public Texture Texture;
            public int PosX;
            public int PosY;
            public int Width;
            public int Height;
            public bool MustBeInFirstPage;

            public TextureView(Texture texture, int posX, int posY, int width, int height, bool mustBeInFirstPage)
            {
                Texture = texture;
                PosX = posX;
                PosY = posY;
                Width = width;
                Height = height;
                MustBeInFirstPage = mustBeInFirstPage;
            }

            public static implicit operator TextureView(Texture texture)
            {
                return new TextureView(texture, 0, 0, texture.Image.Width, texture.Image.Height, false);
            }

            // Custom implementation of these because default implementation is *insanely* slow.
            // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
            public static unsafe bool operator ==(TextureView first, TextureView second)
            {
                return (first.Texture == second.Texture) &&
                    (*(ulong*)(&first.PosX) == *(ulong*)(&second.PosX)) &&
                    (*(ulong*)(&first.Width) == *(ulong*)(&second.Width));
            }

            public static bool operator !=(TextureView first, TextureView second)
            {
                return !(first == second);
            }

            public bool Equals(TextureView other)
            {
                return this == other;
            }

            public override bool Equals(object obj)
            {
                return this == (TextureView)obj;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public struct Result
        {
            public ushort OutputTextureID;
            public RectPacker.Point Pos;
            public void TransformTexCoord(ref ushort TexCoordX, ref ushort TexCoordY)
            {
                TexCoordX += (ushort)(256 * Pos.X);
                TexCoordY += (ushort)(256 * Pos.Y);
            }
        }

        private struct TextureComparer : IComparer<int>
        {
            public List<TextureView> usedTextures;
            public int Compare(int firstIndex, int secondIndex)
            {
                TextureView first = usedTextures[firstIndex];
                TextureView second = usedTextures[secondIndex];

                // Must be in first page?
                if (first.MustBeInFirstPage && !second.MustBeInFirstPage)
                    return -1;
                if (!first.MustBeInFirstPage && second.MustBeInFirstPage)
                    return 1;

                // Compare height
                int firstMaxHeight = Math.Max(first.Width, first.Height); //Because of flipping, the bigger dimension is the height.
                int secondMaxHeight = Math.Max(second.Width, second.Height);
                if (firstMaxHeight != secondMaxHeight)
                    return (firstMaxHeight > secondMaxHeight) ? 1 : -1; //Heigher textures first!

                // Compare area
                int firstArea = first.Width * first.Height;
                int secondArea = second.Width * second.Height;
                if (firstArea != secondArea)
                    return (firstArea > secondArea) ? 1 : -1; //Bigger textures first!

                return 0;
            }
        }

        private readonly List<TextureView> _usedTextures = new List<TextureView>();
        private readonly Dictionary<TextureView, int> _usedTexturesLookup = new Dictionary<TextureView, int>();
        private Result[] _usedTexturePackInfos;

        public int GetOrAllocateTextureID(TextureView texture)
        {
            if ((texture.Width > 256) || (texture.Height > 256))
                throw new NotSupportedException("Texture page too big!");

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
            Array.Sort(usedTexturesProcessingOrder, new TextureComparer { usedTextures = _usedTextures });

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
                    RectPacker.Point? packingPosition = resultingTexturesPacker[i].TryAdd(usedTexture.Width, usedTexture.Height);
                    if (packingPosition.HasValue)
                    {
                        usedTexturePackInfo = new Result { OutputTextureID = i, Pos = packingPosition.Value };
                        goto PackNextUsedTexture;
                    }
                }

                if (resultingTextures.Count >= 65535)
                    throw new NotSupportedException("More then 65536 textures are not supported. That is A LOT (exactly 16GB of texture data), so its probably some other bug if you see this message.");
                var packer = new RectPackerSimpleStack(OutputTextureWidth, OutputTextureHeight);
                usedTexturePackInfo = new Result { OutputTextureID = (ushort)resultingTextures.Count, Pos = packer.TryAdd(usedTexture.Width, usedTexture.Height).Value };
                resultingTextures.Add(ImageC.CreateNew(OutputTextureWidth, OutputTextureHeight));
                resultingTexturesPacker.Add(packer);

                //Write texture data...
                PackNextUsedTexture:
                resultingTextures[usedTexturePackInfo.OutputTextureID].CopyFrom(usedTexturePackInfo.Pos.X, usedTexturePackInfo.Pos.Y,
                    usedTexture.Texture.Image, usedTexture.PosX, usedTexture.PosY, usedTexture.Width, usedTexture.Height);
                _usedTexturePackInfos[UsedTextureIndex] = usedTexturePackInfo;
            }

            return resultingTextures;
        }

        private int GetOrAllocateTextureIDForPageAt(ref TextureArea texture, int pageX, int pageY, 
                                                    int pageWidth, int pageHeight, bool mustBeInFirstPage)
        {
            Vector2 pageOffset = new Vector2(pageX, pageY);
            texture.TexCoord0 -= pageOffset;
            texture.TexCoord1 -= pageOffset;
            texture.TexCoord2 -= pageOffset;
            texture.TexCoord3 -= pageOffset;

            pageWidth = Math.Min(texture.Texture.Image.Width - pageX, pageWidth);
            pageHeight = Math.Min(texture.Texture.Image.Height - pageY, pageHeight);

            return GetOrAllocateTextureID(new TextureView(texture.Texture, pageX, pageY, pageWidth, pageHeight, mustBeInFirstPage));
        }

        public int GetOrAllocateTextureID(ref TextureArea texture, bool isTriangle, bool mustBeInFirstPage)
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

            if (((endX - startX) >= pageWidth) || ((endY - startY) >= pageHeight)) // 'equals' is included too, because 1 was subtracted from 'end' above
                throw new ApplicationException("An applied texture is bigger than 256²! We don't support that yet.");

            if ((startX / pageWidth) == (endX / pageWidth))
            { // Try to pack into a page that is at multiple of 256 on X
                if ((startY / pageHeight) == (endY / pageHeight))
                { // Try to pack into a page that is at multiple of 256 on Y
                    return GetOrAllocateTextureIDForPageAt(ref texture,
                        (startX / pageWidth) * pageWidth,
                        (startY / pageHeight) * pageHeight, pageWidth, pageHeight, mustBeInFirstPage);
                }
                else if (((startY - pageHeight / 2) / pageHeight) == ((endY - pageHeight / 2) / pageHeight))
                { // Try to pack into a page that is at multiple of 256 on Y + 128
                    return GetOrAllocateTextureIDForPageAt(ref texture,
                        (startX / pageWidth) * pageWidth,
                        ((startY - pageHeight / 2) / pageHeight) * pageHeight + pageHeight / 2, pageWidth, pageHeight, mustBeInFirstPage);
                }
            }
            else if (((startX - pageWidth / 2) / pageWidth) == ((endX - pageWidth / 2) / pageWidth))
            { // Try to pack into a page that is at multiple of 256 on X + 128
                if ((startY / pageHeight) == (endY / pageHeight))
                { // Try to pack into a page that is at multiple of 256 on Y
                    return GetOrAllocateTextureIDForPageAt(ref texture,
                        ((startX - pageWidth / 2) / pageWidth) * pageWidth - pageWidth / 2,
                        (startY / pageHeight) * pageHeight, pageWidth, pageHeight, mustBeInFirstPage);
                }
                else if (((startY - pageHeight / 2) / pageHeight) == ((endY - pageHeight / 2) / pageHeight))
                { // Try to pack into a page that is at multiple of 256 on Y + 128
                    return GetOrAllocateTextureIDForPageAt(ref texture,
                        ((startX - pageWidth / 2) / pageWidth) * pageWidth - pageWidth / 2,
                        ((startY - pageHeight / 2) / pageHeight) * pageHeight + pageHeight / 2, pageWidth, pageHeight, mustBeInFirstPage);
                }
            }

            // Include the new texture specifically from whatever region it needs
            return GetOrAllocateTextureIDForPageAt(ref texture, startX, startY, endX - startX + 1, endY - startY + 1, mustBeInFirstPage);
        }

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
