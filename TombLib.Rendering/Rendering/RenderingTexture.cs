using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.Rendering
{
    public struct RenderingTexture : IEquatable<RenderingTexture>
    {
        public ImageC Image;
        public VectorInt2 From; // Inclusive
        public VectorInt2 To; // Exclusive
        public object Tag;

        public RenderingTexture(ImageC image)
        {
            Image = image;
            From = new VectorInt2();
            To = image.Size;
            Tag = null;
        }
        public RenderingTexture(ImageC image, VectorInt2 from, VectorInt2 to)
        {
            Image = image;
            From = from;
            To = to;
            Tag = null;
        }

        public override int GetHashCode()
        {
            return Image.GetHashCode() | (From.X * 176655133 + From.Y * 239618641 + To.X * 1913898487 + To.Y * 537937921);
        }

        public override bool Equals(object other)
        {
            return other is RenderingTexture ? this == (RenderingTexture)other : false;
        }

        public bool Equals(RenderingTexture other)
        {
            return this == other;
        }

        public static bool operator ==(RenderingTexture first, RenderingTexture second)
        {
            return first.Image == second.Image && first.From == second.From && first.To == second.To;
        }

        public static bool operator !=(RenderingTexture first, RenderingTexture second)
        {
            return first.Image != second.Image || first.From != second.From || first.To != second.To;
        }
    };
}
