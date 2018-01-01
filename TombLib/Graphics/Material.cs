using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.GeometryIO;
using TombLib.Utils;

namespace TombLib.Graphics
{
    public class Material
    {
        public string Name { get; private set; }
        public Texture Texture { get; set; }
        public bool AdditiveBlending { get; set; }
        public bool DoubleSided { get; set; }

        public Material(string name)
        {
            Name = name;
        }

        public Material(string name, Texture texture, bool additiveBlending, bool doubleSided)
        {
            Name = name;
            Texture = texture;
            AdditiveBlending = additiveBlending;
            DoubleSided = doubleSided;
        }
    }
}
