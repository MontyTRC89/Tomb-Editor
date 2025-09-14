using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorStructs;

namespace TombLib.Utils
{
    // Represents a set of 90° operations to be performed.
    // All possible such transformations boil down to a mirror on X first, and a variable number of counterclockwise rotations afterwards.
    // Mirror on Z: MirrorX = true, QuadrantRotation = 2
    // Rotate clockwise: MirrorX = false, QuadrantRotation = 3
    public struct RectTransformation
    {
        public bool MirrorX { get; set; }

        private int _quadrantRotation;

        // Rotations in multiples of 90° counterclockwise.
        public int QuadrantRotation
        {
            get { return _quadrantRotation; }
            set
            {
                _quadrantRotation = value & 3; // Module won't do the right thing for negative numbers.
            }
        }

        public void TransformValueQuad(Dictionary<FaceLayerInfo, TextureArea> faceTextures, SectorFace rotation0, SectorFace rotation1, SectorFace rotation2, SectorFace rotation3)
        {
            for (FaceLayer layer = 0; layer < FaceLayer.Count; layer++)
            {
                FaceLayerInfo
                    key0 = new(rotation0, layer),
                    key1 = new(rotation1, layer),
                    key2 = new(rotation2, layer),
                    key3 = new(rotation3, layer);

                if (MirrorX)
                    faceTextures.TrySwap(key0, key2);

                for (int i = 0; i < QuadrantRotation; ++i)
                {
                    TextureArea? temp = null;

                    if (faceTextures.ContainsKey(key0))
                        temp = faceTextures[key0];

                    faceTextures.TrySwap(key0, key3);
                    faceTextures.TrySwap(key3, key2);
                    faceTextures.TrySwap(key2, key1);

                    if (temp.HasValue)
                        faceTextures[key1] = temp.Value;
                }
            }
        }

        public void TransformValueDiagonalQuad<T>(ref T rotation0, ref T rotation1, ref T rotation2, ref T rotation3)
        {
            if (MirrorX)
            {
                Swap.Do(ref rotation2, ref rotation3);
                Swap.Do(ref rotation1, ref rotation0);
            }

            for (int i = 0; i < QuadrantRotation; ++i)
            {
                var temp = rotation0;
                rotation0 = rotation3;
                rotation3 = rotation2;
                rotation2 = rotation1;
                rotation1 = temp;
            }
        }

        public Vector2 Transform(Vector2 position, Vector2 oldSize)
        {
            if (MirrorX)
                position.X = oldSize.X - position.X;
            switch (QuadrantRotation)
            {
                case 0:
                    return position;
                case 1:
                    return new Vector2(oldSize.Y - position.Y, position.X);
                case 2:
                    return new Vector2(oldSize.X - position.X, oldSize.Y - position.Y);
                case 3:
                    return new Vector2(position.Y, oldSize.X - position.X);
                default:
                    throw new InvalidOperationException();
            }
        }

        public VectorInt2 Transform(VectorInt2 position, VectorInt2 oldSize)
        {
            Vector2 result = Transform(position, oldSize - new Vector2(1, 1));
            return new VectorInt2((int)result.X, (int)result.Y);
        }

        public RectangleInt2 TransformRect(RectangleInt2 area, VectorInt2 oldSize)
        {
            VectorInt2 first = Transform(area.Start, oldSize);
            VectorInt2 second = Transform(area.End, oldSize);
            return new RectangleInt2(VectorInt2.Min(first, second), VectorInt2.Max(first, second));
        }

        public Rectangle2 Transform(Rectangle2 area, Vector2 oldSize)
        {
            Vector2 first = Transform(area.Start, oldSize);
            Vector2 second = Transform(area.End, oldSize);
            return new Rectangle2(Vector2.Min(first, second), Vector2.Max(first, second));
        }

        public Vector3 TransformVec3(Vector3 position, float oldWidth, float oldHeight)
        {
            Vector2 result = Transform(new Vector2(position.X, position.Z), new Vector2(oldWidth, oldHeight));
            return new Vector3(result.X, position.Y, result.Y);
        }

        public VectorInt3 TransformVecInt3(VectorInt3 position, int oldWidth, int oldHeight)
        {
            VectorInt2 result = Transform(new VectorInt2(position.X, position.Z), new VectorInt2(oldWidth, oldHeight));
            return new VectorInt3(result.X, position.Y, result.Y);
        }

        public static RectTransformation operator *(RectTransformation first, RectTransformation second)
        {
            if (!second.MirrorX)
                return new RectTransformation { MirrorX = first.MirrorX, QuadrantRotation = first.QuadrantRotation + second.QuadrantRotation };
            else
            {
                if (second.QuadrantRotation == 0 || second.QuadrantRotation == 2)
                    return new RectTransformation { MirrorX = !first.MirrorX, QuadrantRotation = first.QuadrantRotation + second.QuadrantRotation };
                else
                    return new RectTransformation { MirrorX = !first.MirrorX, QuadrantRotation = first.QuadrantRotation + second.QuadrantRotation + 2 };
            }
        }
    }
}
