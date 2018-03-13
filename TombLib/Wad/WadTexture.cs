﻿using System;
using System.IO;
using TombLib.Graphics;
using TombLib.Utils;

namespace TombLib.Wad
{
    public class WadTexture : Texture, IRenderableObject, IEquatable<WadTexture>, TextureHashed
    {
        public Hash Hash { get; }

        public WadTexture(ImageC image)
        {
            Image = image;

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                writer.Write(Image.Size.X);
                writer.Write(Image.Size.Y);
                Image.WriteToStreamRaw(ms);
                Hash = Hash.FromByteArray(ms.ToArray());
            }
        }

        public override Texture Clone() => this;

        public static bool operator==(WadTexture first, WadTexture second) => ReferenceEquals(first, null) ? ReferenceEquals(second, null) : (ReferenceEquals(second, null) ? false : first.Hash == second.Hash);
        public static bool operator!=(WadTexture first, WadTexture second) => !(first == second);
        public bool Equals(WadTexture other) => Hash == other.Hash;
        public override bool Equals(object other) => other is WadTexture && Hash == ((WadTexture)other).Hash;
        public override int GetHashCode() => Hash.GetHashCode();
    }
}
