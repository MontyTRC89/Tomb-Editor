using SharpDX;
using System;
using System.Collections.Generic;

namespace TombLib.Utils
{
    public class ConservativeRasterizer
    {
        public delegate void ScanareaDelegate(int startX, int startY, int endX, int endY);

        private static bool IsFinite(float value)
        {
            return (value - value) == (value - value); // Convert infinity to NaN and detect NaN
        }
        private static void RasterizeBetweenLines(
            float yStart, float yEnd,
            Vector2 Line0Start, Vector2 Line0End,
            Vector2 Line1Start, Vector2 Line1End, ScanareaDelegate scanarea)
        {
            float Line0StepX = (Line0End.X - Line0Start.X) / (Line0End.Y - Line0Start.Y);
            float Line1StepX = (Line1End.X - Line1Start.X) / (Line1End.Y - Line1Start.Y);

            float Line0BaseX = Line0Start.X - Line0StepX * Line0Start.Y;
            float Line1BaseX = Line1Start.X - Line1StepX * Line1Start.Y;

            if (IsFinite(Line0StepX))
            {
                if (IsFinite(Line1StepX))
                { // Both lines are defined

                    int yStartInt = (int)Math.Floor(yStart);
                    int yEndInt = (int)Math.Ceiling(yEnd);

                    for (int y = yStartInt; y < yEndInt; ++y)
                    {
                        float thisY = Math.Min(yEnd, Math.Max(yStart, y));
                        float thisLine0 = Line0BaseX + thisY * Line0StepX;
                        float thisLine1 = Line1BaseX + thisY * Line1StepX;

                        float nextY = Math.Min(yEnd, Math.Max(yStart, y + 1));
                        float nextLine0 = Line0BaseX + nextY * Line0StepX;
                        float nextLine1 = Line1BaseX + nextY * Line1StepX;

                        float fromX = Math.Min(Math.Min(thisLine0, nextLine0), Math.Min(thisLine1, nextLine1));
                        float toX = Math.Max(Math.Max(thisLine0, nextLine0), Math.Max(thisLine1, nextLine1));

                        scanarea((int)Math.Floor(fromX), y, (int)Math.Ceiling(toX), y + 1);
                    }
                }
                else
                { // Line 1 has undefiend slope
                    float y = (Line1Start.Y + Line1End.Y) * 0.5f;
                    float line0x = Line0BaseX + y * Line0StepX;
                    float fromX = Math.Min(line0x, Math.Min(Line1Start.X, Line1End.X));
                    float toX = Math.Max(line0x, Math.Max(Line1Start.X, Line1End.X));
                    int yInt = (int)Math.Floor(y);
                    scanarea((int)Math.Floor(fromX), yInt, (int)Math.Ceiling(toX), yInt + 1);
                }
            }
            else
            {
                if (IsFinite(Line1StepX))
                { // Line 0 has undefined slope
                    float y = (Line0Start.Y + Line0End.Y) * 0.5f;
                    float line1x = Line1BaseX + y * Line1StepX;
                    float fromX = Math.Min(line1x, Math.Min(Line0Start.X, Line0End.X));
                    float toX = Math.Max(line1x, Math.Max(Line0Start.X, Line0End.X));
                    int yInt = (int)Math.Floor(y);
                    scanarea((int)Math.Floor(fromX), yInt, (int)Math.Ceiling(toX), yInt + 1);
                }
                else
                { // Line 0 and 1 have undefined slope
                    return;
                }
            }
        }

        private static void Swap<T>(ref T first, ref T second)
        {
            T temp = first;
            first = second;
            second = temp;
        }

        public static void RasterizeTriangle(Vector2 p0, Vector2 p1, Vector2 p2, ScanareaDelegate scanarea)
        {
            // Sort to ensure vertical point order:
            //       p0 (Minimum Y)
            //       p1
            //       p2 (Maximum Y)

            if (p0.Y > p1.Y)
                Swap(ref p0, ref p1);
            if (p1.Y > p2.Y)
                Swap(ref p1, ref p2);
            if (p0.Y > p1.Y)
                Swap(ref p0, ref p1);

            RasterizeBetweenLines(p0.Y, p1.Y, p0, p1, p0, p2, scanarea);
            RasterizeBetweenLines(p1.Y, p2.Y, p1, p2, p0, p2, scanarea);
        }

        public static void RasterizeQuad(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, ScanareaDelegate scanarea)
        {
            RasterizeTriangle(p0, p1, p2, scanarea);
            RasterizeTriangle(p0, p2, p3, scanarea);
        }

        public static void RasterizeTriangleUniquely(Vector2 p0, Vector2 p1, Vector2 p2, ScanareaDelegate scanarea)
        {
            // Performance could be improved by using an algorithm that ensures unique invokation automatically
            // and does not call 'scanarea' for each and every point seperately

            var result = new HashSet<Tuple<int, int>>();
            RasterizeTriangle(p0, p1, p2,
                (startX, startY, endX, endY) =>
                {
                    for (int y = startY; y < endY; ++y)
                        for (int x = startX; x < endX; ++x)
                            result.Add(new Tuple<int, int>(x, y));
                });

            foreach (var coordinate in result)
                scanarea.Invoke(coordinate.Item1, coordinate.Item2, coordinate.Item1 + 1, coordinate.Item2 + 1);
        }

        public static void RasterizeQuadUniquely(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, ScanareaDelegate scanarea)
        {
            // Performance could be improved by using an algorithm that ensures unique invokation automatically
            // and does not call 'scanarea' for each and every point seperately

            var result = new HashSet<Tuple<int, int>>();
            RasterizeQuad(p0, p1, p2, p3,
                (startX, startY, endX, endY) =>
                {
                    for (int y = startY; y < endY; ++y)
                        for (int x = startX; x < endX; ++x)
                            result.Add(new Tuple<int, int>(x, y));
                });

            foreach (var coordinate in result)
                scanarea.Invoke(coordinate.Item1, coordinate.Item2, coordinate.Item1 + 1, coordinate.Item2 + 1);
        }
    }
}