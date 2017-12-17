using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Vector2 TransformVec2(Vector2 position, float oldWidth, float oldHeight)
        {
            if (MirrorX)
                position.X = oldWidth - position.X;
            switch (QuadrantRotation)
            {
                case 0:
                    return position;
                case 1:
                    return new Vector2(oldHeight - position.Y, position.X);
                case 2:
                    return new Vector2(oldWidth - position.X, oldHeight - position.Y);
                case 3:
                    return new Vector2(position.Y, oldWidth - position.X);
                default:
                    throw new InvalidOperationException();
            }
        }

        public Vector3 TransformVec3(Vector3 position, float oldWidth, float oldHeight)
        {
            Vector2 result = TransformVec2(new Vector2(position.X, position.Z), oldWidth, oldHeight);
            return new Vector3(result.X, position.Y, result.Y);
        }

        public DrawingPoint TransformIVec2(DrawingPoint position, DrawingPoint oldSize)
        {
            Vector2 result = TransformVec2(new Vector2(position.X, position.Y), oldSize.X - 1, oldSize.Y - 1);
            return new DrawingPoint((int)result.X, (int)result.Y);
        }

        public Rectangle TransformRect(Rectangle area, DrawingPoint oldSize)
        {
            DrawingPoint first = TransformIVec2(new DrawingPoint(area.Left, area.Top), oldSize);
            DrawingPoint second = TransformIVec2(new DrawingPoint(area.Right, area.Bottom), oldSize);
            return new Rectangle(Math.Min(first.X, second.X), Math.Min(first.Y, second.Y), Math.Max(first.X, second.X), Math.Max(first.Y, second.Y));
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