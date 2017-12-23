using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using TombLib.Graphics;
using TombLib.Utils;

namespace TombLib.Wad
{
    public class WadTexture : Texture, IRenderableObject, IEquatable<WadTexture>
    {
        private Hash _hash;

        public override bool ReplaceMagentaWithTransparency => true;

        public new ImageC Image
        {
            get { return base.Image; }
            set { base.Image = value; }
        }

        public override Texture Clone()
        {
            var texture = new WadTexture { Image = Image };
            texture.UpdateHash();

            return texture;
        }

        public byte[] ToByteArray()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                writer.Write(Size.X);
                writer.Write(Size.Y);

                Image.WriteToStreamRaw(ms);
                return ms.ToArray();
            }
        }

        public Hash UpdateHash()
        {
            _hash = Hash.FromByteArray(this.ToByteArray());
            return _hash;
        }

        public bool Equals(WadTexture other)
        {
            return (Hash == other.Hash);
        }

        public Hash Hash { get { return _hash; } }
        public VectorInt2 Size { get { return Image.Size; } }
        public int Width { get { return Image.Width; } }
        public int Height { get { return Image.Height; } }

        // Helper data
        public VectorInt2 PositionInTextureAtlas { get; set; }
        public VectorInt2 PositionInPackedTextureMap { get; set; }
        public VectorInt2 PositionInOriginalTexturePage { get; set; }
        public ushort Tile { get; set; }
    }
}
