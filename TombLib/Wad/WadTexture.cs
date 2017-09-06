using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TombLib.Utils;

namespace TombLib.Wad
{
    public class WadTexture : Texture, IEquatable<WadTexture>
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
                writer.Write(Width);
                writer.Write(Height);

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
        public int Width { get { return Image.Width; } }
        public int Height { get { return Image.Height; } }

        // Helper data
        public Vector2 PositionInPackedTexture { get; set; }
        public ushort Tile { get; set; }
    }
}
