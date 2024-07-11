using System;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;

namespace TombLib.Types
{
    public class BezierCurve2D
    {
        private const int CONTROL_POINT_COUNT = 4;

        private readonly Vector2[] _controlPoints = new Vector2[CONTROL_POINT_COUNT];

        public Vector2 Start
        {
            get
            {
                return _controlPoints[0];
            }
            set
            {
                _controlPoints[0] = value;
            }
        }

        public Vector2 End
        {
            get
            {
                return _controlPoints[3];
            }
            set
            {
                _controlPoints[3] = value;
            }
        }

        public Vector2 StartHandle
        {
            get
            {
                return _controlPoints[1];
            }
            set
            {
                _controlPoints[1] = value;
            }
        }

        public Vector2 EndHandle
        {
            get
            {
                return _controlPoints[2];
            }
            set
            {
                _controlPoints[2] = value;
            }
        }

        public static BezierCurve2D Linear { get; } = new BezierCurve2D(Vector2.Zero, Vector2.One, Vector2.Zero, Vector2.One);
        public static BezierCurve2D EaseIn { get; } = new BezierCurve2D(Vector2.Zero, Vector2.One, new Vector2(0.25f, 0.0f), Vector2.One);
        public static BezierCurve2D EaseOut { get; } = new BezierCurve2D(Vector2.Zero, Vector2.One, Vector2.Zero, new Vector2(0.75f, 1.0f));
        public static BezierCurve2D EaseInOut { get; } = new BezierCurve2D(Vector2.Zero, Vector2.One, new Vector2(0.25f, 0.0f), new Vector2(0.75f, 1.0f));

        public BezierCurve2D(Vector2 start, Vector2 end, Vector2 startHandle, Vector2 endHandle)
        {
            Start = start;
            End = end;
            StartHandle = startHandle;
            EndHandle = endHandle;
        }

        public Vector2 GetPoint(float alpha)
        {
            alpha = Math.Clamp(alpha, 0.0f, 1.0f);

            // De Casteljau interpolation.
            var points = _controlPoints.ToArray();
            for (int i = 1; i < _controlPoints.Length; i++)
            {
                for (int j = 0; j < (_controlPoints.Length - i); j++)
                    points[j] = Vector2.Lerp(points[j], points[j + 1], alpha);
            }

            return points[0];
        }

        public float GetY(float x)
        {
            const float TOLERANCE = 0.001f;

            // Directly return Y for exact endpoint.
            if (x <= (Start.X + TOLERANCE))
            {
                return Start.Y;
            }
            else if (x >= (End.X - TOLERANCE))
            {
                return End.Y;
            }

            float low = 0.0f;
            float high = 1.0f;
            var point = new Vector2();

            // Binary search for approximate Y.
            float alpha = 0.5f;
            while ((high - low) > TOLERANCE)
            {
                alpha = (low + high) / 2;
                point = GetPoint(alpha);

                if (point.X < x)
                {
                    low = alpha;
                }
                else
                {
                    high = alpha;
                }
            }

            return point.Y;
        }
    }
}
