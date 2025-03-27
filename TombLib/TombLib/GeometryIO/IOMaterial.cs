﻿using TombLib.Utils;

namespace TombLib.GeometryIO
{
    public class IOMaterial
    {
        public string Name { get; private set; }
        public Texture Texture { get; set; }
        public bool AdditiveBlending { get; set; }
        public bool DoubleSided { get; set; }
        public int Shininess { get; set; }
		public bool DynamicWaterSurface { get; set; }

		// These 2 fields are used for exporting material texture to split texture pages.

		public string Path { get; set; }
        public int Page { get; set; }

        public IOMaterial(string name)
        {
            Name = name;
        }

        public IOMaterial(string name, Texture texture, string texturePath,bool additiveBlending, bool doubleSided, int shininess, int page, bool dynamicWaterSurface)
        {
            Name = name;
            Texture = texture;
            AdditiveBlending = additiveBlending;
            DoubleSided = doubleSided;
            Shininess = shininess;
            Path = texturePath;
            Page = page;
            DynamicWaterSurface = dynamicWaterSurface;

		}
    }
}
