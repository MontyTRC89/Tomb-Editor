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
            const float TOLERANCE           = 0.001f;
            const float ITERATION_COUNT_MAX = 100;

            // Directly return Y for exact endpoint.
            if (x <= (Start.X + TOLERANCE))
            {
                return Start.Y;
            }
            else if (x >= (End.X - TOLERANCE))
            {
                return End.Y;
            }

            // Newton-Raphson iteration for approximate Y alpha.
            float alpha = x / End.X;
            for (int i = 0; i < ITERATION_COUNT_MAX; i++)
            {
                var point = GetPoint(alpha);
                var derivative = GetDerivative(alpha);

                float delta = (point.X - x) / derivative.X;
                alpha -= delta;

                if (Math.Abs(delta) <= TOLERANCE)
                    break;
            }

            return GetPoint(alpha).Y;
        }

        private Vector2 GetDerivative(float alpha)
        {
            alpha = Math.Clamp(alpha, 0.0f, 1.0f);

            var points = _controlPoints.ToArray();
            int count = _controlPoints.Length - 1;

		    // Calculate derivative control points.
		    for (int i = 0; i < count; i++)
			    points[i] = (_controlPoints[i + 1] - _controlPoints[i]) * count;

		    // Reduce points using De Casteljau interpolation.
		    for (int i = 1; i < count; i++)
		    {
			    for (int j = 0; j < (count - i); j++)
				    points[j] = Vector2.Lerp(points[j], points[j + 1], alpha);
		    }

		    return points[0];
	    }

        public static bool operator ==(BezierCurve2D first, BezierCurve2D second)
        {
            if (ReferenceEquals(first, second))
                return true;

            if (first is null || second is null)
                return false;

            return first.Start == second.Start &&
                   first.End == second.End &&
                   first.StartHandle == second.StartHandle &&
                   first.EndHandle == second.EndHandle;
        }


        public static bool operator !=(BezierCurve2D first, BezierCurve2D second) => !(first == second);
    }
}
