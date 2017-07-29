using System.Collections.Generic;

namespace TombEditor.Geometry
{
    public class AnimatedTextureSequenceVariant
    {
        public bool IsTriangle { get; set; }
        public TextureTileType Triangle { get; set; }
        public bool Flipped { get; set; }
        public int AnimatedSequence { get; set; }
        public short Size { get; set; }
        public short DeltaX { get; set; }
        public short DeltaY { get; set; }
        public bool Transparent { get; set; }
        public List<AnimatedTextureVariantTile> Tiles { get; set; } = new List<AnimatedTextureVariantTile>();
    }

    public class AnimatedTextureVariantTile
    {
        public int Tile { get; set; }
        public int NewId { get; set; }

        public AnimatedTextureVariantTile(int tile, int newId)
        {
            Tile = tile;
            NewId = newId;
        }
    }

    public class AnimatedTextureSet
    {
        public AnimatexTextureSetEffect Effect { get; set; }
        public List<AnimatedTexture> Textures { get; set; } = new List<Geometry.AnimatedTexture>();
        public List<AnimatedTextureSequenceVariant> Variants { get; set; }
    }
}
