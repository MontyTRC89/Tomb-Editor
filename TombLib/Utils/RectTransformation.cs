using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Utils
{
    public struct RectTransformation
    {
        public bool MirrorX { get; set; }

        private int _quadrantRotation;
        public int QuadrantRotation
        {
            get { return _quadrantRotation; }
            set
            {
                _quadrantRotation = value & 3; // Module won't do the right thing for negative numbers.
            }
        }

        public void TransformValueQuad<T>(ref T rotation0, ref T rotation1, ref T rotation2, ref T rotation3)
        {
            if (MirrorX)
                Swap.Do(ref rotation0, ref rotation2);

            for (int i = 0; i < QuadrantRotation; ++i)
            {
                var temp = rotation0;
                rotation0 = rotation3;
                rotation3 = rotation2;
                rotation2 = rotation1;
                rotation1 = temp;
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
            Vector2 result = Transform((Vector2)position, oldSize - new Vector2(1, 1));
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