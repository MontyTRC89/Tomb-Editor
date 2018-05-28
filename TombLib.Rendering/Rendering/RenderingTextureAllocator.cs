using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.Rendering
{
    public abstract class RenderingTextureAllocator : IDisposable
    {
        public class Description
        {
            public VectorInt3 Size { get; set; } = new VectorInt3(2048, 2048, 8); // 128 MB
            public Func<VectorInt2, RectPacker> CreateRectPacker = size => new RectPackerTree(size);
        }

        private class TexturePage
        {
            public RectPacker Packer;
            public TexturePage(VectorInt2 Size)
            {
                Packer = new RectPackerTree(Size);
            }
        }

        private readonly TexturePage[] Pages;
		private readonly Dictionary<RenderingTexture, RenderingTextureAllocated> AvailableTextures = new Dictionary<RenderingTexture, RenderingTextureAllocated>();
		public VectorInt3 Size { get; }

		public RenderingTextureAllocator(RenderingDevice device, Description description)
        {
            Pages = new TexturePage[description.Size.Z];
            for (int i = 0; i < description.Size.Z; ++i)
                Pages[i] = new TexturePage(new VectorInt2(description.Size.X, description.Size.Y));
			Size = description.Size;
		}

        public abstract void Dispose();
        protected abstract void UploadTexture(RenderingTexture Texture, VectorInt3 pos);

        /// <summary>
        /// RemoveUser must be called after the rendering texture is no longer used.
        /// </summary>
        public RenderingTextureAllocated Get(RenderingTexture texture, RenderingTextureAllocatorUser user)
        {
            RenderingTextureAllocated result;
            if (AvailableTextures.TryGetValue(texture, out result))
            {
                result.Users.Add(user);
                return result;
            }

            // Add texture
            // TODO Performance: Perhaps sort pages in some way? Better packing can probably be achived by sorting the textures.
            for (int i = 0; i < Pages.Length; ++i)
            {
                VectorInt2? allocatedPos = Pages[i].Packer.TryAdd(texture.To - texture.From);
                if (allocatedPos != null)
                {
                    VectorInt3 pos = new VectorInt3(allocatedPos.Value.X, allocatedPos.Value.Y, i);
                    UploadTexture(texture, pos);
                    result = new RenderingTextureAllocated { Position = pos };
                    AvailableTextures.Add(texture, result);
                    result.Users.Add(user);
                    return result;
                }
            }

            // TODO Not enough space, try rearranging texture, try increasing texture size, try freeing unused textures
            throw new NotImplementedException("The texture atlas has ran out of space.");
        }
    }
}
