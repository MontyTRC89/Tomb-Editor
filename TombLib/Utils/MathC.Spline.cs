using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace TombLib
{
    // Ported code from: http://www.java-gaming.org/index.php?topic=9830.0

    public static class Spline
    {
        private class Cubic
        {
            private float a, b, c, d;

            public Cubic(float a, float b, float c, float d)
            {
                this.a = a;
                this.b = b;
                this.c = c;
                this.d = d;
            }

            public float Eval(float u) => (((d * u) + c) * u + b) * u + a;
        }

        private static List<Cubic> CalcNaturalCubic(List<float> points)
        {
            if (points.Count <= 1)
                return new List<Cubic>();

            int num = points.Count - 1;

            float[] gamma = new float[num + 1];
            float[] delta = new float[num + 1];
            float[] D = new float[num + 1];

            /*
                 We solve the equation
                [2 1       ] [D[0]]   [3(x[1] - x[0])  ]
                |1 4 1     | |D[1]|   |3(x[2] - x[0])  |
                |  1 4 1   | | .  | = |      .         |
                |    ..... | | .  |   |      .         |
                |     1 4 1| | .  |   |3(x[n] - x[n-2])|
                [       1 2] [D[n]]   [3(x[n] - x[n-1])]

                by using row operations to convert the matrix to upper triangular
                and then back sustitution.  The D[i] are the derivatives at the knots.
            */

            gamma[0] = 1.0f / 2.0f;
            for (int i = 1; i < num; i++)
                gamma[i] = 1.0f / (4.0f - gamma[i - 1]);

            gamma[num] = 1.0f / (2.0f - gamma[num - 1]);

            float p0 = points[0];
            float p1 = points[1];

            delta[0] = 3.0f * (p1 - p0) * gamma[0];

            for (int i = 1; i < num; i++)
            {
                p0 = points[i - 1];
                p1 = points[i + 1];
                delta[i] = (3.0f * (p1 - p0) - delta[i - 1]) * gamma[i];
            }

            p0 = points[num - 1];
            p1 = points[num];

            delta[num] = (3.0f * (p1 - p0) - delta[num - 1]) * gamma[num];

            D[num] = delta[num];
            for (int i = num - 1; i >= 0; i--)
                D[i] = delta[i] - gamma[i] * D[i + 1];

            // Now compute the coefficients of the cubics 
            var result = new List<Cubic>();

            for (int i = 0; i < num; i++)
            {
                p0 = points[i];
                p1 = points[i + 1];

                result.Add(new Cubic
                (p0, D[i],
                 3 * (p1 - p0) - 2 * D[i] - D[i + 1],
                 2 * (p0 - p1) + D[i] + D[i + 1]));
            }

            return result;
        }

        public static List<Vector3> Calculate(List<Vector3> points, int subdivisions)
        {
            if (points.Count <= 1)
                return points;

            var pX = CalcNaturalCubic(points.Select(p => p.X).ToList());
            var pY = CalcNaturalCubic(points.Select(p => p.Y).ToList());
            var pZ = CalcNaturalCubic(points.Select(p => p.Z).ToList());

            var result = new List<Vector3>();
            float grain = 1.0f / subdivisions;

            for (int i = 0; i < subdivisions; i++)
            {
                var currPos = i * grain * pX.Count;
                var currCube = (int)(currPos);
                var cubePos = currPos - currCube;

                result.Add(new Vector3(pX[currCube].Eval(cubePos), pY[currCube].Eval(cubePos), pZ[currCube].Eval(cubePos)));
            }
            result.Add(points.Last()); // Add last point

            return result;
        }
    }
}
