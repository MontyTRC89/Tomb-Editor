using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.Rendering
{
    public class RenderingTextureAllocatorUser : IDisposable
    {
        private KeyValuePair<RenderingTexture, RenderingTextureAllocated> QuickAccessTexture0;
        private KeyValuePair<RenderingTexture, RenderingTextureAllocated> QuickAccessTexture1;
        private int LastUsedTextureIndex = 0;
        private readonly Dictionary<RenderingTexture, RenderingTextureAllocated> UsedTextures = new Dictionary<RenderingTexture, RenderingTextureAllocated>();
        private readonly RenderingTextureAllocator TextureAllocator;

        public RenderingTextureAllocatorUser(RenderingTextureAllocator textureAllocator)
        {
            TextureAllocator = textureAllocator;
        }

        public void Dispose()
        {
            foreach (RenderingTextureAllocated allocatedTexture in UsedTextures.Values)
                allocatedTexture.RemoveUser(this);
        }

        public RenderingTextureAllocated AllocateTexture(RenderingTexture texture)
        {
            // Check if it's the last used texture.
            // This speeds up the common case because oftentimes similar textures are used often
            if (texture == QuickAccessTexture0.Key)
            {
                LastUsedTextureIndex = 0;
                return QuickAccessTexture0.Value;
            }
            if (texture == QuickAccessTexture1.Key)
            {
                LastUsedTextureIndex = 1;
                return QuickAccessTexture1.Value;
            }

            // Check if the texture is already used
            RenderingTextureAllocated result;
            if (!UsedTextures.TryGetValue(texture, out result))
            {
                // Request the texture from the allocator
                result = TextureAllocator.Get(texture, this);
                UsedTextures.Add(texture, result);
            }

            // Cache texture index
            if (LastUsedTextureIndex == 0)
            {
                QuickAccessTexture1 = new KeyValuePair<RenderingTexture, RenderingTextureAllocated>(texture, result);
                LastUsedTextureIndex = 1;
            }
            else
            {
                QuickAccessTexture0 = new KeyValuePair<RenderingTexture, RenderingTextureAllocated>(texture, result);
                LastUsedTextureIndex = 0;
            }
            return result;
        }

        public VectorInt3 AllocateTextureForTriangle(TextureArea texture)
        {
            Vector2 min = Vector2.Min(Vector2.Min(texture.TexCoord0, texture.TexCoord1), texture.TexCoord2);
            Vector2 max = Vector2.Max(Vector2.Max(texture.TexCoord0, texture.TexCoord1), texture.TexCoord2) + new Vector2(254.0f / 256.0f);
            RenderingTexture textureToAllocate = new RenderingTexture
            {
                From = new VectorInt2((int)min.X, (int)min.Y),
                To = new VectorInt2((int)max.X, (int)max.Y),
                Image = texture.Texture.Image
            };
            RenderingTextureAllocated allocatedTexture = AllocateTexture(textureToAllocate);
            return allocatedTexture.Position - new VectorInt3(textureToAllocate.From.X, textureToAllocate.From.Y, 0);
        }
    }
}
