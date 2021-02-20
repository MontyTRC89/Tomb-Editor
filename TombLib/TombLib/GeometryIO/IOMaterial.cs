using TombLib.Utils;

namespace TombLib.GeometryIO
{
    public class IOMaterial
    {
        public string Name { get; private set; }
        public string Path { get; set; }
        public Texture Texture { get; set; }
        public bool AdditiveBlending { get; set; }
        public bool DoubleSided { get; set; }
        public int Shininess { get; set; }
        public string TexturePath { get; set; }
        public int Page { get; set; }

        public IOMaterial(string name)
        {
            Name = name;
        }

        public IOMaterial(string name, Texture texture, string texturePath,bool additiveBlending, bool doubleSided, int shininess, int page)
        {
            Name = name;
            Texture = texture;
            AdditiveBlending = additiveBlending;
            DoubleSided = doubleSided;
            Shininess = shininess;
            TexturePath = texturePath;
            Page = page;
        }
    }
}
