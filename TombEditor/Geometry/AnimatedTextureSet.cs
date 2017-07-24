using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor.Geometry
{
    public class AnimatedTextureSequenceVariant
    {
        public bool IsTriangle { get; set; }
        public TextureTileType Triangle { get; set; }
        public bool Flipped { get; set; }
        public int AnimatedSequence { get; set; }
        public int Size { get; set; }
        public bool Transparent { get; set; }
        public List<AnimatedTextureVariantTile> Tiles { get; set; }

        public AnimatedTextureSequenceVariant()
        {
            Tiles = new List<AnimatedTextureVariantTile>();
        }
    }

    public class AnimatedTextureVariantTile
    {
        public int Tile { get; set; }
        public int NewID { get; set; }

        public AnimatedTextureVariantTile(int tile, int newID)
        {
            this.Tile = tile;
            this.NewID = newID;
        }
    }

    public class AnimatedTextureSet
    {
        public AnimatexTextureSetEffect Effect { get; set; }

        public List<AnimatedTexture> Textures { get; set; }

        public List<AnimatedTextureSequenceVariant> Variants { get; set; }

        public AnimatedTextureSet()
        {
            Textures = new List<Geometry.AnimatedTexture>();
        }
    }
}
