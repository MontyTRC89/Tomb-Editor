using System;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.Wad
{
    public enum WadPolygonShape : ushort
    {
        Quad = 0,
        Triangle = 1
    }

    public struct WadPolygon
    {
        public WadPolygonShape Shape;
        public int Index0;
        public int Index1;
        public int Index2;
        public int Index3;
        public TextureArea Texture;
        public byte ShineStrength;

        public bool IsTriangle => Shape == WadPolygonShape.Triangle;

        public Vector2[] CorrectTexCoords(float margin = 0.5f) =>
            MathC.CorrectTexCoords(Texture.TexCoords, IsTriangle, margin);

        public void Rotate(int iter = 1, bool isTriangle = false)
        {
            for (int i = 0; i < iter; i++)
            {
                if (!isTriangle)
                {
                    int tempIndex = Index3;
                    Index3 = Index2;
                    Index2 = Index1;
                    Index1 = Index0;
                    Index0 = tempIndex;
                }
                else
                {
                    int tempIndex = Index2;
                    Index2 = Index1;
                    Index1 = Index0;
                    Index0 = tempIndex;
                    Index3 = Index2;
                }
            }
        }

        public void Flip(bool isTriangle = false)
        {
            if (!isTriangle)
            {
                int tempIndex = Index0;
                Index0 = Index3;
                Index3 = tempIndex;

                tempIndex = Index1;
                Index1 = Index2;
                Index2 = tempIndex;
            }
            else
            {
                int tempIndex = Index0;
                Index0 = Index2;
                Index2 = tempIndex;
                Index3 = Index2;
            }
        }
    }
}
