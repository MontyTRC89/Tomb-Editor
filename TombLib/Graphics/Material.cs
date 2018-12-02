﻿using TombLib.Utils;

namespace TombLib.Graphics
{
    public class Material
    {
        public const string Material_Opaque = "TeOp";
        public const string Material_OpaqueDoubleSided = "TeOpDS";
        public const string Material_AdditiveBlending = "TeBl";
        public const string Material_AdditiveBlendingDoubleSided = "TeBlDS";

        public string Name { get; private set; }
        public Texture Texture { get; set; }
        public bool AdditiveBlending { get; set; }
        public bool DoubleSided { get; set; }
        public int Shininess { get; set; }

        public Material(string name)
        {
            Name = name;
        }

        public Material(string name, Texture texture, bool additiveBlending, bool doubleSided, int shininess)
        {
            Name = name;
            Texture = texture;
            AdditiveBlending = additiveBlending;
            DoubleSided = doubleSided;
            Shininess = shininess;
        }
    }
}
