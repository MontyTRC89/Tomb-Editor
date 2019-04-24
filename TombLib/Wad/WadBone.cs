using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Wad
{
    public class WadBone
    {
        public string Name { get; set; }
        public WadMesh Mesh { get; set; }
        public Vector3 Translation { get; set; }
        public WadBone Parent { get; set; }
        public List<WadBone> Children { get; } = new List<WadBone>();
        public WadLinkOpcode OpCode { get; set; }

        public Matrix4x4 Transform => Matrix4x4.CreateTranslation(Translation);

        public WadBone Clone(WadBone parentBone)
        {
            WadBone newBone = new WadBone();
            newBone.Name = Name;
            newBone.Mesh = Mesh;
            newBone.Translation = Translation;
            newBone.Parent = parentBone;

            foreach (var childBone in Children)
                newBone.Children.Add(childBone.Clone(newBone));

            return newBone;
        }

        public WadBone Clone()
        {
            WadBone newBone = new WadBone();
            newBone.Name = Name;
            newBone.Mesh = Mesh;
            newBone.Translation = Translation;
            newBone.OpCode = OpCode;

            return newBone;
        }
    }
}
