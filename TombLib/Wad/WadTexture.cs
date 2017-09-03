using TombLib.Utils;

namespace TombLib.Wad
{
    public class WadTexture : Texture
    {
        public WadTexturePageType Type;
        public string Hash;
        
        public bool ReplaceMagentaWithTransparency => true;

        public new ImageC Image
        {
            get => base.Image;
            set => base.Image = value;
        }

        public override Texture Clone()
        {
            return new WadTexture { Image = Image, Hash = Hash, Type = Type };
        }
    }    
}
